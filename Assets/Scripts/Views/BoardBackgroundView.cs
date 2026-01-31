using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;
using Match3.Levels;

namespace Match3.Views
{
    /// <summary>
    /// Manages the background tiles for the game board.
    /// Supports dynamic board shapes using Unity's Tilemap system.
    /// </summary>
    public class BoardBackgroundView : MonoBehaviour
    {
        [Title("References")]
        [Required]
        public Grid Grid;
        
        [Required]
        public Tilemap Tilemap;
        
        [Title("Visuals")]
        [Tooltip("The rule tile or tile to use for the board background/frame")]
        [Required]
        public TileBase BackgroundTile;
        
        [Title("Settings")]
        [Tooltip("Should be >= 0 to render behind gems")]
        public int SortingOrder = 0;
        
        [Tooltip("Offset relative to the board grid")]
        public Vector3 PositionOffset = Vector3.zero;
        
        [Tooltip("Manual scaling multiplier for the background (Default: 1)")]
        public float ScaleMultiplier = 1.0f;

        /// <summary>
        /// Initializes the background based on the level data.
        /// </summary>
        public void Initialize(LevelData levelData, float tileSize, Vector2 boardOffset)
        {
            if (Grid == null || Tilemap == null || BackgroundTile == null)
            {
                Debug.LogWarning("BoardBackgroundView: Missing references!");
                return;
            }

            // Option: Size vs Spacing
            // User wants 1 sprite per cell without stretching.
            // So we set the CELL SIZE to match the board spacing (tileSize).
            // But we keep the Grid Scale at 1, so the sprites aren't stretched.
            
            float spacingSize = tileSize * ScaleMultiplier; // ScaleMultiplier can adjust spacing if needed
            
            Grid.cellSize = new Vector3(spacingSize, spacingSize, 0);
            Grid.transform.localScale = Vector3.one; // No stretching!
            
            // Adjust grid position to match board offset
            // BoardOffset is the CENTER of tile (0,0).
            // Grid origin (0,0) is Bottom-Left of that cell.
            // We shift origin by half of the spacing size.
            Vector3 centerOffset = new Vector3(spacingSize * 0.5f, spacingSize * 0.5f, 0);
            
            Grid.transform.position = (Vector3)boardOffset - centerOffset + PositionOffset;
            
            // Reset tilemap
            Tilemap.ClearAllTiles();
            var tilemapRenderer = Tilemap.GetComponent<TilemapRenderer>();
            if (tilemapRenderer != null)
            {
                tilemapRenderer.sortingOrder = SortingOrder;
            }

            // Fill tiles
            GenerateTiles(levelData);
            
            Debug.Log("Board background generated.");
        }

        [Button("Regenerate (Editor Debug)")]
        private void GenerateTiles(LevelData levelData)
        {
            if (levelData == null) return;
            
            Tilemap.ClearAllTiles();
            
            int width = levelData.Width;
            int height = levelData.Height;
            
            // Offset logic:
            // If we assume the Grid object is centered at (0,0), then (0,0) in tilemap is at 0,0 world.
            // We need to position tiles such that (x,y) matches BoardView (x,y).
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Check if this position is valid in the layout
                    if (IsPositionValid(levelData, x, y))
                    {
                        var pos = new Vector3Int(x, y, 0);
                        Tilemap.SetTile(pos, BackgroundTile);
                    }
                }
            }
        }
        
        private bool IsPositionValid(LevelData level, int x, int y)
        {
            if (level.UseCustomLayout && level.InitialLayout != null)
            {
                // Check bounds of array just in case
                if (x < level.InitialLayout.GetLength(0) && y < level.InitialLayout.GetLength(1))
                {
                    // Assume 'None' means empty hole if you have a None type, 
                    // OR specifically define a "Hole" type.
                    // For now, we assume all cells in bounds are valid unless we add a specific "Empty" type.
                    // If your game supports holes, check for TileType.Hole or similar here.
                    return true; 
                }
            }
            return true;
        }
    }
}
