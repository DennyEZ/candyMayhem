using UnityEngine;

namespace Match3.Core
{
    /// <summary>
    /// Represents the result of a match detection.
    /// </summary>
    public enum MatchType
    {
        None,
        Match3,         // Standard 3-match
        Match4,         // 4 in a row - creates rocket
        Match5,         // 5 in a row - creates rainbow
        MatchL,         // L-shape - creates bomb
        MatchT,         // T-shape - creates bomb
    }
    
    /// <summary>
    /// Contains information about a detected match.
    /// </summary>
    public class MatchInfo
    {
        public MatchType Type;
        public Vector2Int[] Positions;
        public Vector2Int CenterPosition;  // Where special tile should spawn
        public Data.TileType TileColor;    // The matched color
        
        public MatchInfo(MatchType type, Vector2Int[] positions, Data.TileType color)
        {
            Type = type;
            Positions = positions;
            TileColor = color;
            
            // Calculate center position for special tile spawn
            if (positions.Length > 0)
            {
                int sumX = 0, sumY = 0;
                foreach (var pos in positions)
                {
                    sumX += pos.x;
                    sumY += pos.y;
                }
                CenterPosition = new Vector2Int(sumX / positions.Length, sumY / positions.Length);
            }
        }
        
        /// <summary>
        /// Gets the special tile type that should be created from this match.
        /// </summary>
        public Data.TileType GetSpecialTileType(bool isHorizontal = true)
        {
            switch (Type)
            {
                case MatchType.Match4:
                    return isHorizontal ? Data.TileType.HorizontalRocket : Data.TileType.VerticalRocket;
                case MatchType.Match5:
                    return Data.TileType.Rainbow;
                case MatchType.MatchL:
                case MatchType.MatchT:
                    return Data.TileType.Bomb;
                default:
                    return Data.TileType.None;
            }
        }
    }
}
