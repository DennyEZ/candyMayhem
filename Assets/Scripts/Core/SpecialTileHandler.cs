using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Match3.Data;

namespace Match3.Core
{
    /// <summary>
    /// Handles special tile activation effects (rockets, bombs, rainbow).
    /// Calculates affected positions and returns them for clearing.
    /// </summary>
    public class SpecialTileHandler : MonoBehaviour
    {
        [Title("References")]
        [Required]
        public BoardController BoardController;
        
        [Title("Effect Settings")]
        public float EffectDelay = 0.1f;  // Delay between chain effects
        
        /// <summary>
        /// Activates a special tile and returns all affected positions.
        /// </summary>
        public List<Vector2Int> ActivateSpecialTile(int x, int y, TileType type)
        {
            var affected = new List<Vector2Int>();
            
            switch (type)
            {
                case TileType.HorizontalRocket:
                    affected = GetRowPositions(y);
                    break;
                    
                case TileType.VerticalRocket:
                    affected = GetColumnPositions(x);
                    break;
                    
                case TileType.Bomb:
                    affected = GetBombPositions(x, y);
                    break;
                    
                case TileType.Rainbow:
                    // Rainbow needs a target color - handled separately
                    break;
            }
            
            return affected;
        }
        
        /// <summary>
        /// Activates rainbow tile targeting a specific color.
        /// </summary>
        public List<Vector2Int> ActivateRainbow(TileType targetColor)
        {
            var affected = new List<Vector2Int>();
            
            for (int x = 0; x < BoardController.Width; x++)
            {
                for (int y = 0; y < BoardController.Height; y++)
                {
                    var tile = BoardController.GetTile(x, y);
                    if (tile != null && tile.Type == targetColor)
                    {
                        affected.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            return affected;
        }
        
        /// <summary>
        /// Handles special + special tile combinations.
        /// Returns affected positions for the combo effect.
        /// </summary>
        public List<Vector2Int> HandleSpecialCombo(TileType type1, TileType type2, int x, int y)
        {
            var affected = new List<Vector2Int>();
            
            // Rainbow + anything = clear all of that special type's effect
            if (type1 == TileType.Rainbow || type2 == TileType.Rainbow)
            {
                var otherType = type1 == TileType.Rainbow ? type2 : type1;
                
                if (otherType == TileType.Rainbow)
                {
                    // Rainbow + Rainbow = clear entire board!
                    return GetAllPositions();
                }
                
                // Rainbow + Rocket = rockets on all tiles of a color
                // Rainbow + Bomb = bombs on all tiles of a color
                // For simplicity, just clear a lot
                affected = GetLargeAreaPositions(x, y, 5);
            }
            // Bomb + Bomb = larger explosion
            else if (type1 == TileType.Bomb && type2 == TileType.Bomb)
            {
                affected = GetBombPositions(x, y, 5);  // 5x5 instead of 3x3
            }
            // Rocket + Rocket = cross pattern
            else if ((type1 == TileType.HorizontalRocket || type1 == TileType.VerticalRocket) &&
                     (type2 == TileType.HorizontalRocket || type2 == TileType.VerticalRocket))
            {
                affected.AddRange(GetRowPositions(y));
                affected.AddRange(GetColumnPositions(x));
            }
            // Bomb + Rocket = 3 row or 3 column clear
            else if ((type1 == TileType.Bomb && (type2 == TileType.HorizontalRocket || type2 == TileType.VerticalRocket)) ||
                     (type2 == TileType.Bomb && (type1 == TileType.HorizontalRocket || type1 == TileType.VerticalRocket)))
            {
                bool isHorizontal = type1 == TileType.HorizontalRocket || type2 == TileType.HorizontalRocket;
                
                if (isHorizontal)
                {
                    // Clear 3 rows
                    for (int row = y - 1; row <= y + 1; row++)
                    {
                        if (row >= 0 && row < BoardController.Height)
                            affected.AddRange(GetRowPositions(row));
                    }
                }
                else
                {
                    // Clear 3 columns
                    for (int col = x - 1; col <= x + 1; col++)
                    {
                        if (col >= 0 && col < BoardController.Width)
                            affected.AddRange(GetColumnPositions(col));
                    }
                }
            }
            
            return affected;
        }
        
        private List<Vector2Int> GetRowPositions(int y)
        {
            var positions = new List<Vector2Int>();
            for (int x = 0; x < BoardController.Width; x++)
            {
                var tile = BoardController.GetTile(x, y);
                if (tile != null && !tile.Type.BlocksMovement())
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
            return positions;
        }
        
        private List<Vector2Int> GetColumnPositions(int x)
        {
            var positions = new List<Vector2Int>();
            for (int y = 0; y < BoardController.Height; y++)
            {
                var tile = BoardController.GetTile(x, y);
                if (tile != null && !tile.Type.BlocksMovement())
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
            return positions;
        }
        
        private List<Vector2Int> GetBombPositions(int centerX, int centerY, int radius = 1)
        {
            var positions = new List<Vector2Int>();
            
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    if (BoardController.IsValidPosition(x, y))
                    {
                        var tile = BoardController.GetTile(x, y);
                        if (tile != null && !tile.Type.BlocksMovement())
                        {
                            positions.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
            
            return positions;
        }
        
        private List<Vector2Int> GetLargeAreaPositions(int centerX, int centerY, int radius)
        {
            return GetBombPositions(centerX, centerY, radius);
        }
        
        private List<Vector2Int> GetAllPositions()
        {
            var positions = new List<Vector2Int>();
            
            for (int x = 0; x < BoardController.Width; x++)
            {
                for (int y = 0; y < BoardController.Height; y++)
                {
                    var tile = BoardController.GetTile(x, y);
                    if (tile != null && !tile.Type.BlocksMovement())
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            return positions;
        }
    }
}
