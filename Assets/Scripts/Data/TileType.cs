namespace Match3.Data
{
    /// <summary>
    /// All tile types in the game.
    /// Organized into categories: Normal gems, Special tiles, and Blockers.
    /// </summary>
    public enum TileType
    {
        // Empty/None
        None = 0,
        Empty = 1,  // Hole in board - no tile can exist here
        
        // Normal Gems (matchable colors)
        Red = 10,
        Blue = 11,
        Green = 12,
        Yellow = 13,
        Purple = 14,
        Orange = 15,
        
        // Special Tiles (created by special matches)
        HorizontalRocket = 20,  // 4-match horizontal - clears row
        VerticalRocket = 21,    // 4-match vertical - clears column
        Bomb = 22,              // L or T match - clears 3x3 area
        Rainbow = 23,           // 5-match - clears all of one color
        
        // Blockers
        Ice1 = 30,              // 1 layer ice
        Ice2 = 31,              // 2 layer ice
        Ice3 = 32,              // 3 layer ice
        Stone = 33,             // Indestructible obstacle
        Crate1 = 34,            // 1-hit crate
        Crate2 = 35,            // 2-hit crate
    }
    
    /// <summary>
    /// Extension methods for TileType enum.
    /// </summary>
    public static class TileTypeExtensions
    {
        public static bool IsNormalGem(this TileType type)
        {
            return type >= TileType.Red && type <= TileType.Orange;
        }
        
        public static bool IsSpecialTile(this TileType type)
        {
            return type >= TileType.HorizontalRocket && type <= TileType.Rainbow;
        }
        
        public static bool IsBlocker(this TileType type)
        {
            return type >= TileType.Ice1 && type <= TileType.Crate2;
        }
        
        public static bool IsIce(this TileType type)
        {
            return type >= TileType.Ice1 && type <= TileType.Ice3;
        }
        
        public static bool IsCrate(this TileType type)
        {
            return type >= TileType.Crate1 && type <= TileType.Crate2;
        }
        
        public static bool IsMatchable(this TileType type)
        {
            // Only normal gems can match - special tiles activate on swap instead
            return IsNormalGem(type);
        }
        
        public static bool CanFall(this TileType type)
        {
            return IsNormalGem(type) || IsSpecialTile(type);
        }
        
        public static bool BlocksMovement(this TileType type)
        {
            return type == TileType.Stone || type == TileType.Empty;
        }
        
        /// <summary>
        /// Gets the base color for special tiles (they retain their color identity).
        /// </summary>
        public static TileType GetBaseColor(this TileType type)
        {
            // Special tiles don't have a base color - they match with any
            if (type.IsSpecialTile()) return TileType.None;
            return type;
        }
    }
}
