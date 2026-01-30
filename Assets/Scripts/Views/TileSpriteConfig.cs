using UnityEngine;
using Match3.Data;

namespace Match3.Views
{
    /// <summary>
    /// Configuration asset for tile sprites and colors.
    /// </summary>
    [CreateAssetMenu(fileName = "TileSpriteConfig", menuName = "Match3/Tile Sprite Config")]
    public class TileSpriteConfig : ScriptableObject
    {
        [Header("Normal Gems")]
        public Sprite RedSprite;
        public Sprite BlueSprite;
        public Sprite GreenSprite;
        public Sprite YellowSprite;
        public Sprite PurpleSprite;
        public Sprite OrangeSprite;
        
        [Header("Special Tiles")]
        public Sprite HorizontalRocketSprite;
        public Sprite VerticalRocketSprite;
        public Sprite BombSprite;
        public Sprite RainbowSprite;
        
        [Header("Blockers")]
        public Sprite Ice1Sprite;
        public Sprite Ice2Sprite;
        public Sprite Ice3Sprite;
        public Sprite StoneSprite;
        public Sprite Crate1Sprite;
        public Sprite Crate2Sprite;
        
        [Header("Fallback Shape")]
        public Sprite DefaultCircle;
        
        public Sprite GetSprite(TileType type)
        {
            switch (type)
            {
                case TileType.Red: return RedSprite ?? DefaultCircle;
                case TileType.Blue: return BlueSprite ?? DefaultCircle;
                case TileType.Green: return GreenSprite ?? DefaultCircle;
                case TileType.Yellow: return YellowSprite ?? DefaultCircle;
                case TileType.Purple: return PurpleSprite ?? DefaultCircle;
                case TileType.Orange: return OrangeSprite ?? DefaultCircle;
                case TileType.HorizontalRocket: return HorizontalRocketSprite ?? DefaultCircle;
                case TileType.VerticalRocket: return VerticalRocketSprite ?? DefaultCircle;
                case TileType.Bomb: return BombSprite ?? DefaultCircle;
                case TileType.Rainbow: return RainbowSprite ?? DefaultCircle;
                case TileType.Ice1: return Ice1Sprite ?? DefaultCircle;
                case TileType.Ice2: return Ice2Sprite ?? DefaultCircle;
                case TileType.Ice3: return Ice3Sprite ?? DefaultCircle;
                case TileType.Stone: return StoneSprite ?? DefaultCircle;
                case TileType.Crate1: return Crate1Sprite ?? DefaultCircle;
                case TileType.Crate2: return Crate2Sprite ?? DefaultCircle;
                default: return DefaultCircle;
            }
        }
        
        public Color GetColor(TileType type)
        {
            // Special tiles always use white (no tint) - they have their own sprite colors
            if (type.IsSpecialTile())
            {
                return Color.white;
            }
            
            // If we have a custom sprite for this gem type, return white (no tint)
            if (HasCustomSpriteForType(type))
            {
                return Color.white;
            }
            
            // Fallback colors for when using default circle (no custom sprites)
            switch (type)
            {
                case TileType.Red: return new Color(0.9f, 0.2f, 0.2f);
                case TileType.Blue: return new Color(0.2f, 0.4f, 0.9f);
                case TileType.Green: return new Color(0.2f, 0.8f, 0.3f);
                case TileType.Yellow: return new Color(0.95f, 0.85f, 0.2f);
                case TileType.Purple: return new Color(0.7f, 0.2f, 0.8f);
                case TileType.Orange: return new Color(0.95f, 0.5f, 0.1f);
                default: return Color.white;
            }
        }
        
        private bool HasCustomSpriteForType(TileType type)
        {
            switch (type)
            {
                case TileType.Red: return RedSprite != null;
                case TileType.Blue: return BlueSprite != null;
                case TileType.Green: return GreenSprite != null;
                case TileType.Yellow: return YellowSprite != null;
                case TileType.Purple: return PurpleSprite != null;
                case TileType.Orange: return OrangeSprite != null;
                default: return false;
            }
        }
    }
}
