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
        public Ease FallEase = Ease.InQuad;
        public Ease ClearEase = Ease.InBack;
        
        [Title("Debug")]
        [ShowInInspector, ReadOnly]
        public int GridX { get; private set; }
        
        [ShowInInspector, ReadOnly]
        public int GridY { get; private set; }
        
        [ShowInInspector, ReadOnly]
        public TileType CurrentType { get; private set; }

        public float ClipTopY { get; set; } = 9999f; // Default to no clipping
        
        /// <summary>
        /// Updates the grid position tracking (used during fall animations).
        /// </summary>
        public void UpdateGridPosition(int x, int y)
        {
            GridX = x;
            GridY = y;
        }

        private void LateUpdate()
        {
            if (_spriteRenderer == null) return;
            
            // Logic-based clipping: Hide if strictly above the clip line
            bool shouldBeVisible = transform.position.y <= ClipTopY;
            
            if (_spriteRenderer.enabled != shouldBeVisible)
            {
                _spriteRenderer.enabled = shouldBeVisible;
                if (_overlayRenderer != null) _overlayRenderer.enabled = shouldBeVisible;
            }
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
            
            // Apply ice overlay if present
            UpdateIceOverlay(data.IceLevel);
            
            // Make sure it's visible
            gameObject.SetActive(true);
            _spriteRenderer.enabled = true;
            
            Debug.Log($"TileView setup at ({GridX},{GridY}) pos={worldPosition}, type={CurrentType}, ice={data.IceLevel}");
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
                case TileType.Stone: return new Color(0.5f, 0.5f, 0.5f);
                case TileType.Crate1:
                case TileType.Crate2: return new Color(0.6f, 0.4f, 0.2f);
                default: return Color.white;
            }
        }
        
        /// <summary>
        /// Updates the ice overlay visual based on ice level.
        /// </summary>
        public void UpdateIceOverlay(int iceLevel)
        {
            if (_overlayRenderer == null)
            {
                // No overlay renderer assigned - skip ice visuals
                return;
            }
            
            if (iceLevel <= 0)
            {
                _overlayRenderer.gameObject.SetActive(false);
                return;
            }
            
            _overlayRenderer.gameObject.SetActive(true);
            
            // Adjust opacity based on ice level (more layers = more opaque)
            // Level 1: 0.3 alpha, Level 2: 0.5 alpha, Level 3: 0.7 alpha
            float alpha = 0.2f + (iceLevel * 0.15f);
            var color = new Color(0.7f, 0.9f, 1f, alpha);  // Light blue ice color
            _overlayRenderer.color = color;
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
        public Tween AnimateFall(Vector3 targetPosition, float duration = -1f)
        {
            float d = duration > 0 ? duration : FallDuration;
            return MoveTo(targetPosition, d, FallEase);
        }
        
        /// <summary>
        /// Animates spawning from above.
        /// </summary>
        public Tween AnimateSpawn(Vector3 startPosition, Vector3 targetPosition, float duration = -1f)
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
            
            float d = duration > 0 ? duration : SpawnDuration;
            
            KillCurrentTween();
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(_originalScale, d * 0.3f).SetEase(Ease.OutQuad));
            sequence.Join(transform.DOMove(targetPosition, d).SetEase(Ease.InQuad));
            
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
            
            // Trigger explosion effect
            if (_spriteRenderer != null)
            {
                // Use the logical color for particles (renderer color might be white tint)
                Color particleColor = GetFallbackColor(CurrentType);
                FX.ParticleFactory.PlayExplosion(transform.position, particleColor);
            }
            
            var sequence = DOTween.Sequence();
            // Quick shrink with no "puff" up, just clean destruction
            sequence.Append(transform.DOScale(Vector3.zero, ClearDuration * 0.5f).SetEase(Ease.InBack));
            sequence.Join(_spriteRenderer.DOFade(0f, ClearDuration * 0.5f));
            
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
            
            // Reset ice overlay
            UpdateIceOverlay(0);
            
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
