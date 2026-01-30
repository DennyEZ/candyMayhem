using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace Match3.Core
{
    /// <summary>
    /// Handles player input (swipe detection) for the match-3 game.
    /// Uses the new Input System package.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Title("Settings")]
        [PropertyRange(10f, 100f)]
        public float MinSwipeDistance = 30f;
        
        [PropertyRange(0.1f, 1f)]
        public float MaxSwipeTime = 0.5f;
        
        [Title("Grid Settings")]
        public float TileSize = 1f;
        public Vector2 BoardOffset = Vector2.zero;
        
        [Title("Debug")]
        [ShowInInspector, ReadOnly]
        private bool _isDragging;
        
        [ShowInInspector, ReadOnly]
        private Vector2 _startPosition;
        
        [ShowInInspector, ReadOnly]
        private float _startTime;
        
        [ShowInInspector, ReadOnly]
        private int _startGridX, _startGridY;
        
        private Camera _mainCamera;
        private int _boardWidth, _boardHeight;
        
        // Input System references
        private Mouse _mouse;
        private Touchscreen _touchscreen;
        
        /// <summary>
        /// Fired when a valid swipe is detected.
        /// Parameters: fromX, fromY, toX, toY (grid coordinates)
        /// </summary>
        public event Action<int, int, int, int> OnSwipeDetected;
        
        /// <summary>
        /// Fired when a tile is tapped (for future selection-based input).
        /// </summary>
        public event Action<int, int> OnTileTapped;
        
        private void Awake()
        {
            _mainCamera = Camera.main;
            _mouse = Mouse.current;
            _touchscreen = Touchscreen.current;
        }
        
        public void SetBoardSize(int width, int height, float tileSize, Vector2 offset)
        {
            _boardWidth = width;
            _boardHeight = height;
            TileSize = tileSize;
            BoardOffset = offset;
        }
        
        private void Update()
        {
            HandleMouseInput();
            HandleTouchInput();
        }
        
        private void HandleMouseInput()
        {
            if (_mouse == null) return;
            
            if (_mouse.leftButton.wasPressedThisFrame)
            {
                OnPointerDown(_mouse.position.ReadValue());
            }
            else if (_mouse.leftButton.wasReleasedThisFrame)
            {
                OnPointerUp(_mouse.position.ReadValue());
            }
        }
        
        private void HandleTouchInput()
        {
            if (_touchscreen == null || _touchscreen.touches.Count == 0) return;
            
            var touch = _touchscreen.touches[0];
            
            if (touch.press.wasPressedThisFrame)
            {
                OnPointerDown(touch.position.ReadValue());
            }
            else if (touch.press.wasReleasedThisFrame)
            {
                OnPointerUp(touch.position.ReadValue());
            }
        }
        
        private void OnPointerDown(Vector2 screenPosition)
        {
            _isDragging = true;
            _startPosition = screenPosition;
            _startTime = Time.time;
            
            // Convert to grid position
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPosition);
            ScreenToGrid(worldPos, out _startGridX, out _startGridY);
        }
        
        private void OnPointerUp(Vector2 screenPosition)
        {
            if (!_isDragging) return;
            _isDragging = false;
            
            float swipeTime = Time.time - _startTime;
            Vector2 swipeDelta = screenPosition - _startPosition;
            float swipeDistance = swipeDelta.magnitude;
            
            // Check if it's a valid swipe
            if (swipeDistance >= MinSwipeDistance && swipeTime <= MaxSwipeTime)
            {
                // First check if starting position is valid
                if (!IsValidGridPosition(_startGridX, _startGridY))
                {
                    Debug.Log($"Swipe started outside board at ({_startGridX}, {_startGridY})");
                    return;
                }
                
                // Determine swipe direction
                Vector2Int direction = GetSwipeDirection(swipeDelta);
                
                int toX = _startGridX + direction.x;
                int toY = _startGridY + direction.y;
                
                // Check if target position is valid
                if (!IsValidGridPosition(toX, toY))
                {
                    Debug.Log($"Swipe target outside board: ({toX}, {toY})");
                    return;
                }
                
                OnSwipeDetected?.Invoke(_startGridX, _startGridY, toX, toY);
            }
            else if (swipeDistance < MinSwipeDistance)
            {
                // It's a tap
                if (IsValidGridPosition(_startGridX, _startGridY))
                {
                    OnTileTapped?.Invoke(_startGridX, _startGridY);
                }
            }
        }
        
        private Vector2Int GetSwipeDirection(Vector2 delta)
        {
            // Determine if horizontal or vertical based on larger component
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                // Horizontal swipe
                return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                // Vertical swipe
                return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }
        
        private void ScreenToGrid(Vector2 worldPos, out int gridX, out int gridY)
        {
            // The BoardOffset points to the CENTER of tile (0,0)
            // So we need to offset and then round to nearest tile
            float adjustedX = worldPos.x - BoardOffset.x;
            float adjustedY = worldPos.y - BoardOffset.y;
            
            // Use RoundToInt since tiles are centered on grid positions
            gridX = Mathf.RoundToInt(adjustedX / TileSize);
            gridY = Mathf.RoundToInt(adjustedY / TileSize);
            
            Debug.Log($"Click at world ({worldPos.x:F2}, {worldPos.y:F2}) -> grid ({gridX}, {gridY})");
        }
        
        private bool IsValidGridPosition(int x, int y)
        {
            return x >= 0 && x < _boardWidth && y >= 0 && y < _boardHeight;
        }
        
        /// <summary>
        /// Converts grid coordinates to world position (center of tile).
        /// </summary>
        public Vector2 GridToWorld(int gridX, int gridY)
        {
            return new Vector2(
                gridX * TileSize + TileSize * 0.5f + BoardOffset.x,
                gridY * TileSize + TileSize * 0.5f + BoardOffset.y
            );
        }
    }
}
