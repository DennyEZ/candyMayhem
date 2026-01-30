using UnityEngine;
using Sirenix.OdinInspector;
using Match3.Data;
using DG.Tweening;

namespace Match3.Views
{
    /// <summary>
    /// Visual representation of a tile.
    /// This is the "body" - it only handles visuals and animations.
    /// The "brain" is TileData in BoardController.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileView : MonoBehaviour
    {
        [Title("Components")]
        [SerializeField, Required]
        private SpriteRenderer _spriteRenderer;
        
        [SerializeField]
        private SpriteRenderer _overlayRenderer;  // For ice overlay
        
        [Title("Sprites")]
        [SerializeField]
        private TileSpriteConfig _spriteConfig;
        
        [Title("Animation Settings")]
        public float SwapDuration = 0.2f;
        public float FallDuration = 0.3f;
        public float ClearDuration = 0.2f;
        public float SpawnDuration = 0.25f;
        
        public Ease SwapEase = Ease.OutBack;
        public Ease FallEase = Ease.OutBounce;
        public Ease ClearEase = Ease.InBack;
        
        [Title("Debug")]
        [ShowInInspector, ReadOnly]
        public int GridX { get; private set; }
        
        [ShowInInspector, ReadOnly]
        public int GridY { get; private set; }
        
        [ShowInInspector, ReadOnly]
        public TileType CurrentType { get; private set; }
        
        /// <summary>
        /// Updates the grid position tracking (used during fall animations).
        /// </summary>
        public void UpdateGridPosition(int x, int y)
        {
            GridX = x;
            GridY = y;
        }
        
        private Tween _currentTween;
        private Vector3 _originalScale = Vector3.one;
        
        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Ensure we have a valid scale
            if (transform.localScale == Vector3.zero)
                transform.localScale = Vector3.one;
            
            _originalScale = transform.localScale;
            
            // Ensure sprite is visible
            if (_spriteRenderer != null && _spriteRenderer.sprite == null)
            {
                // Create a simple white square as fallback
                _spriteRenderer.sprite = CreateFallbackSprite();
            }
        }
        
        private static Sprite _fallbackSprite;
        
        private Sprite CreateFallbackSprite()
        {
            if (_fallbackSprite != null) return _fallbackSprite;
            
            // Create a simple 32x32 white texture
            var texture = new Texture2D(32, 32);
            var colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();
            
            _fallbackSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            return _fallbackSprite;
        }
        
        /// <summary>
        /// Sets up this view for a tile at the given position.
        /// </summary>
        public void Setup(TileData data, Vector3 worldPosition)
        {
            GridX = data.X;
            GridY = data.Y;
            CurrentType = data.Type;
            
            transform.position = worldPosition;
            
            // Ensure sprite renderer and sprite are ready
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (_spriteRenderer.sprite == null)
                _spriteRenderer.sprite = CreateFallbackSprite();
            
            UpdateVisuals(data.Type);
            
            // Make sure it's visible
            gameObject.SetActive(true);
            _spriteRenderer.enabled = true;
            
            Debug.Log($"TileView setup at ({GridX},{GridY}) pos={worldPosition}, type={CurrentType}");
        }
        
        /// <summary>
        /// Updates the visual appearance based on tile type.
        /// </summary>
        public void UpdateVisuals(TileType type)
        {
            CurrentType = type;
            
            if (_spriteConfig != null)
            {
                _spriteRenderer.sprite = _spriteConfig.GetSprite(type);
                _spriteRenderer.color = _spriteConfig.GetColor(type);
            }
            else
            {
                // Fallback: use color-coding with fallback sprite
                if (_spriteRenderer.sprite == null)
                    _spriteRenderer.sprite = CreateFallbackSprite();
                _spriteRenderer.color = GetFallbackColor(type);
            }
            
            // Reset scale and alpha
            if (_originalScale == Vector3.zero)
                _originalScale = Vector3.one;
            transform.localScale = _originalScale;
            
            var color = _spriteRenderer.color;
            color.a = 1f;
            _spriteRenderer.color = color;
        }
        
        private Color GetFallbackColor(TileType type)
        {
            switch (type)
            {
                case TileType.Red: return new Color(0.9f, 0.2f, 0.2f);
                case TileType.Blue: return new Color(0.2f, 0.4f, 0.9f);
                case TileType.Green: return new Color(0.2f, 0.8f, 0.3f);
                case TileType.Yellow: return new Color(0.95f, 0.85f, 0.2f);
                case TileType.Purple: return new Color(0.7f, 0.2f, 0.8f);
                case TileType.Orange: return new Color(0.95f, 0.5f, 0.1f);
                case TileType.HorizontalRocket: return new Color(1f, 0.3f, 0.3f);
                case TileType.VerticalRocket: return new Color(0.3f, 0.3f, 1f);
                case TileType.Bomb: return new Color(0.3f, 0.3f, 0.3f);
                case TileType.Rainbow: return Color.white;
                case TileType.Ice1:
                case TileType.Ice2:
                case TileType.Ice3: return new Color(0.7f, 0.9f, 1f, 0.7f);
                case TileType.Stone: return new Color(0.5f, 0.5f, 0.5f);
                case TileType.Crate1:
                case TileType.Crate2: return new Color(0.6f, 0.4f, 0.2f);
                default: return Color.white;
            }
        }
        
        /// <summary>
        /// Animates movement to a new position.
        /// </summary>
        public Tween MoveTo(Vector3 targetPosition, float duration, Ease ease = Ease.OutQuad)
        {
            KillCurrentTween();
            _currentTween = transform.DOMove(targetPosition, duration).SetEase(ease);
            return _currentTween;
        }
        
        /// <summary>
        /// Animates the swap movement.
        /// </summary>
        public Tween AnimateSwap(Vector3 targetPosition)
        {
            return MoveTo(targetPosition, SwapDuration, SwapEase);
        }
        
        /// <summary>
        /// Animates falling to a new position.
        /// </summary>
        public Tween AnimateFall(Vector3 targetPosition)
        {
            return MoveTo(targetPosition, FallDuration, FallEase);
        }
        
        /// <summary>
        /// Animates spawning from above.
        /// </summary>
        public Tween AnimateSpawn(Vector3 startPosition, Vector3 targetPosition)
        {
            // Ensure we have a valid target scale
            if (_originalScale == Vector3.zero)
                _originalScale = Vector3.one;
            
            transform.position = startPosition;
            transform.localScale = Vector3.zero;
            
            // Make sure sprite is visible
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = true;
                var c = _spriteRenderer.color;
                c.a = 1f;
                _spriteRenderer.color = c;
            }
            
            KillCurrentTween();
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(_originalScale, SpawnDuration * 0.5f).SetEase(Ease.OutBack));
            sequence.Join(transform.DOMove(targetPosition, SpawnDuration).SetEase(Ease.OutBounce));
            
            // CRITICAL: Ensure tile is fully visible when animation completes
            sequence.OnComplete(() => {
                transform.localScale = _originalScale;
                transform.position = targetPosition;
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.enabled = true;
                    var c = _spriteRenderer.color;
                    c.a = 1f;
                    _spriteRenderer.color = c;
                }
            });
            
            _currentTween = sequence;
            return sequence;
        }
        
        /// <summary>
        /// Animates the clear/pop effect.
        /// </summary>
        public Tween AnimateClear()
        {
            KillCurrentTween();
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(_originalScale * 1.2f, ClearDuration * 0.3f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(Vector3.zero, ClearDuration * 0.7f).SetEase(ClearEase));
            sequence.Join(_spriteRenderer.DOFade(0f, ClearDuration * 0.7f));
            _currentTween = sequence;
            return sequence;
        }
        
        /// <summary>
        /// Shows a selection highlight.
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selected)
            {
                transform.DOScale(_originalScale * 1.1f, 0.15f).SetEase(Ease.OutBack);
            }
            else
            {
                transform.DOScale(_originalScale, 0.1f);
            }
        }
        
        /// <summary>
        /// Resets the view for pooling.
        /// </summary>
        public void Reset()
        {
            KillCurrentTween();
            
            // Ensure valid scale
            if (_originalScale == Vector3.zero)
                _originalScale = Vector3.one;
            transform.localScale = _originalScale;
            
            if (_spriteRenderer != null)
            {
                var color = _spriteRenderer.color;
                color.a = 1f;
                _spriteRenderer.color = color;
                _spriteRenderer.enabled = true;
            }
            
            GridX = -1;
            GridY = -1;
        }
        
        private void KillCurrentTween()
        {
            if (_currentTween != null && _currentTween.IsActive())
            {
                _currentTween.Kill();
            }
            _currentTween = null;
        }
        
        private void OnDestroy()
        {
            KillCurrentTween();
        }
    }
}
