using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using Match3.Core;
using Match3.Data;

namespace Match3.Views
{
    /// <summary>
    /// Manages the visual representation of the game board.
    /// Handles all animations and visual updates.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [Title("References")]
        [Required]
        public TilePool TilePool;
        
        [Required]
        public Transform TileParent;
        
        [Title("Grid Settings")]
        public float TileSize = 1.2f;
        public float TileSpacing = 0.15f;
        
        [Title("Board Position")]
        public Vector2 BoardOffset = Vector2.zero;
        public bool CenterBoard = true;
        
        [Title("Animation Timing")]
        public float SwapDuration = 0.2f;
        public float FallDuration = 0.3f;
        public float ClearDuration = 0.2f;
        public float SpawnDelay = 0.05f;  // Stagger for cascade effect
        
        [ShowInInspector, ReadOnly]
        private TileView[,] _views;
        
        private int _width, _height;
        private float _effectiveTileSize;
        
        /// <summary>
        /// Initializes the board view for a given grid size.
        /// </summary>
        public void Initialize(int width, int height)
        {
            _width = width;
            _height = height;
            
            // Auto-scale to fit screen (9:16 aspect ratio)
            var cam = Camera.main;
            float screenHeight = cam.orthographicSize * 2f;
            float screenWidth = screenHeight * cam.aspect;
            
            // Leave some padding (10% on each side)
            float availableWidth = screenWidth * 0.85f;
            float availableHeight = screenHeight * 0.75f;  // Leave room for UI at top
            
            // Calculate tile size to fit both dimensions
            float maxTileSizeForWidth = availableWidth / width;
            float maxTileSizeForHeight = availableHeight / height;
            
            // Use the smaller to ensure it fits
            float autoTileSize = Mathf.Min(maxTileSizeForWidth, maxTileSizeForHeight);
            
            // Apply (but respect minimum)
            TileSize = Mathf.Max(autoTileSize * 0.9f, 0.5f);  // 0.9 for spacing
            TileSpacing = TileSize * 0.1f;
            
            _effectiveTileSize = TileSize + TileSpacing;
            
            Debug.Log($"Auto-scaled tile size to {TileSize:F2} for {width}x{height} grid");
            
            // Calculate offset to center board
            if (CenterBoard)
            {
                BoardOffset = new Vector2(
                    -(_width * _effectiveTileSize) / 2f + _effectiveTileSize / 2f,
                    -(_height * _effectiveTileSize) / 2f + _effectiveTileSize / 2f - 0.5f  // Shift down slightly for UI
                );
            }
            
            _views = new TileView[width, height];
            
            // Update InputHandler with grid info
            var inputHandler = FindAnyObjectByType<InputHandler>();
            if (inputHandler != null)
            {
                inputHandler.SetBoardSize(width, height, _effectiveTileSize, BoardOffset);
            }
        }
        
        /// <summary>
        /// Creates a new tile view at the specified grid position.
        /// </summary>
        public TileView CreateTileView(TileData data)
        {
            var view = TilePool.Get();
            var worldPos = GridToWorld(data.X, data.Y);
            
            view.transform.SetParent(TileParent);
            view.Setup(data, worldPos);
            
            _views[data.X, data.Y] = view;
            
            return view;
        }
        
        /// <summary>
        /// Gets the tile view at a grid position.
        /// </summary>
        public TileView GetView(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height) return null;
            return _views[x, y];
        }
        
        /// <summary>
        /// Converts grid coordinates to world position.
        /// </summary>
        public Vector3 GridToWorld(int gridX, int gridY)
        {
            return new Vector3(
                gridX * _effectiveTileSize + BoardOffset.x,
                gridY * _effectiveTileSize + BoardOffset.y,
                0f
            );
        }
        
        /// <summary>
        /// Animates a swap between two tiles.
        /// Returns when animation is complete.
        /// </summary>
        public IEnumerator AnimateSwap(int x1, int y1, int x2, int y2)
        {
            var view1 = GetView(x1, y1);
            var view2 = GetView(x2, y2);
            
            if (view1 == null || view2 == null)
            {
                yield break;
            }
            
            var pos1 = GridToWorld(x1, y1);
            var pos2 = GridToWorld(x2, y2);
            
            // Animate both tiles
            var tween1 = view1.AnimateSwap(pos2);
            var tween2 = view2.AnimateSwap(pos1);
            
            // Swap in view array
            _views[x1, y1] = view2;
            _views[x2, y2] = view1;
            
            // Wait for animations
            yield return tween1.WaitForCompletion();
        }
        
        /// <summary>
        /// Animates clearing matched tiles.
        /// </summary>
        public IEnumerator AnimateClear(List<TileData> clearedTiles)
        {
            var tweens = new List<Tween>();
            var viewsToReturn = new List<TileView>();
            
            foreach (var tile in clearedTiles)
            {
                var view = GetView(tile.X, tile.Y);
                if (view != null)
                {
                    tweens.Add(view.AnimateClear());
                    viewsToReturn.Add(view);  // Store reference BEFORE nullifying
                    _views[tile.X, tile.Y] = null;
                }
            }
            
            // Wait for all clear animations
            if (tweens.Count > 0)
            {
                yield return tweens[0].WaitForCompletion();
            }
            
            // Return views to pool
            foreach (var view in viewsToReturn)
            {
                TilePool.Return(view);
            }
            
            Debug.Log($"Cleared {viewsToReturn.Count} tiles");
        }
        
        /// <summary>
        /// Animates tiles falling to fill gaps.
        /// </summary>
        public IEnumerator AnimateFall(List<(TileData tile, int fromY, int toY)> falls)
        {
            if (falls == null || falls.Count == 0)
            {
                yield break;
            }
            
            var tweens = new List<Tween>();
            
            // CRITICAL FIX: Build a snapshot of ALL views by their current GridX/GridY BEFORE any modifications
            // This prevents race conditions when multiple tiles fall in the same column
            var viewSnapshot = new Dictionary<(int x, int y), TileView>();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var view = _views[x, y];
                    if (view != null && view.gameObject.activeSelf)
                    {
                        viewSnapshot[(view.GridX, view.GridY)] = view;
                    }
                }
            }
            
            // Also check by searching TileParent for any views we might have missed
            foreach (Transform child in TileParent)
            {
                var view = child.GetComponent<TileView>();
                if (view != null && view.gameObject.activeSelf && view.GridX >= 0 && view.GridY >= 0)
                {
                    var key = (view.GridX, view.GridY);
                    if (!viewSnapshot.ContainsKey(key))
                    {
                        viewSnapshot[key] = view;
                    }
                }
            }
            
            // Sort falls from bottom to top (lower toY first) to prevent conflicts
            var sortedFalls = new List<(TileData tile, int fromY, int toY)>(falls);
            sortedFalls.Sort((a, b) => a.toY.CompareTo(b.toY));
            
            // Clear the views array for affected positions first
            foreach (var (tile, fromY, toY) in sortedFalls)
            {
                if (tile.X >= 0 && tile.X < _width && fromY >= 0 && fromY < _height)
                {
                    _views[tile.X, fromY] = null;
                }
            }
            
            // Now process falls using our snapshot
            foreach (var (tile, fromY, toY) in sortedFalls)
            {
                // Look up view from our snapshot using the ORIGINAL position
                TileView view = null;
                if (viewSnapshot.TryGetValue((tile.X, fromY), out var snapshotView))
                {
                    view = snapshotView;
                }
                
                if (view != null)
                {
                    var targetPos = GridToWorld(tile.X, toY);
                    tweens.Add(view.AnimateFall(targetPos));
                    
                    // Update view's internal position tracking
                    view.UpdateGridPosition(tile.X, toY);
                    
                    // Update view array with new position
                    _views[tile.X, toY] = view;
                }
                else
                {
                    Debug.LogWarning($"AnimateFall: No view found for tile at ({tile.X}, {fromY}) -> ({tile.X}, {toY}) - creating new view");
                    // Create a new view if completely missing
                    var newView = CreateTileView(tile);
                    // Start from above and animate down
                    var startPos = GridToWorld(tile.X, fromY);
                    var targetPos = GridToWorld(tile.X, toY);
                    newView.transform.position = startPos;
                    tweens.Add(newView.AnimateFall(targetPos));
                }
            }
            
            // Wait for all fall animations to complete
            if (tweens.Count > 0)
            {
                yield return new WaitForSeconds(FallDuration + 0.05f); // Small buffer for reliability
            }
        }
        
        /// <summary>
        /// Searches all active views for one matching the given grid position.
        /// Fallback when array lookup fails.
        /// </summary>
        private TileView FindViewByGridPosition(int gridX, int gridY)
        {
            foreach (Transform child in TileParent)
            {
                var view = child.GetComponent<TileView>();
                if (view != null && view.GridX == gridX && view.GridY == gridY && view.gameObject.activeSelf)
                {
                    return view;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Animates new tiles spawning from above.
        /// </summary>
        public IEnumerator AnimateSpawn(List<TileData> newTiles)
        {
            if (newTiles == null || newTiles.Count == 0)
            {
                yield break;
            }
            
            // Group tiles by column for staggered effect
            var tilesByColumn = new Dictionary<int, List<TileData>>();
            foreach (var tile in newTiles)
            {
                if (!tilesByColumn.ContainsKey(tile.X))
                {
                    tilesByColumn[tile.X] = new List<TileData>();
                }
                tilesByColumn[tile.X].Add(tile);
            }
            
            // Sort each column from bottom to top (lower Y first)
            foreach (var column in tilesByColumn.Values)
            {
                column.Sort((a, b) => a.Y.CompareTo(b.Y));
            }
            
            var allTweens = new List<Tween>();
            float maxDuration = 0f;
            
            // Spawn all tiles with stagger per column
            foreach (var kvp in tilesByColumn)
            {
                int columnIndex = kvp.Key;
                var tilesInColumn = kvp.Value;
                float columnDelay = columnIndex * SpawnDelay * 0.5f; // Slight stagger between columns
                
                for (int i = 0; i < tilesInColumn.Count; i++)
                {
                    var tile = tilesInColumn[i];
                    
                    // Check if view already exists (shouldn't but be safe)
                    if (_views[tile.X, tile.Y] != null && _views[tile.X, tile.Y].gameObject.activeSelf)
                    {
                        continue;
                    }
                    
                    var view = CreateTileView(tile);
                    
                    // Calculate spawn position - stagger height based on order in column
                    int spawnOffset = tilesInColumn.Count - i; // Higher tiles spawn from further above
                    var startPos = GridToWorld(tile.X, _height + spawnOffset);
                    var targetPos = GridToWorld(tile.X, tile.Y);
                    
                    // Immediate visibility setup
                    view.transform.localScale = Vector3.one;
                    view.transform.position = startPos;
                    
                    var tween = view.AnimateSpawn(startPos, targetPos);
                    if (tween != null)
                    {
                        allTweens.Add(tween);
                        maxDuration = Mathf.Max(maxDuration, tween.Duration());
                    }
                }
            }
            
            // Wait for all spawn animations to complete
            if (allTweens.Count > 0)
            {
                yield return new WaitForSeconds(maxDuration + 0.1f); // Buffer for reliability
            }
            
            // Final validation: ensure all tiles have views
            foreach (var tile in newTiles)
            {
                if (_views[tile.X, tile.Y] == null)
                {
                    Debug.LogError($"AnimateSpawn: View missing after spawn for tile at ({tile.X}, {tile.Y}) - creating fallback");
                    var fallbackView = CreateTileView(tile);
                    fallbackView.transform.position = GridToWorld(tile.X, tile.Y);
                    fallbackView.transform.localScale = Vector3.one;
                }
            }
        }
        
        /// <summary>
        /// Creates all tile views for the initial board.
        /// </summary>
        public void CreateInitialBoard(TileData[,] grid)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (grid[x, y] != null)
                    {
                        CreateTileView(grid[x, y]);
                    }
                }
            }
        }
        
        /// <summary>
        /// Clears all tile views.
        /// </summary>
        [Button("Clear Board")]
        public void ClearBoard()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var view = _views[x, y];
                    if (view != null)
                    {
                        TilePool.Return(view);
                        _views[x, y] = null;
                    }
                }
            }
        }
        
        /// <summary>
        /// Syncs the view array with the data layer to fix any position mismatches.
        /// Call this before collapse operations to ensure views are in correct positions.
        /// </summary>
        public void SyncViewsWithData(Core.BoardController boardController)
        {
            // First, build a dictionary of all active views by their internal GridX/GridY
            var viewsByPosition = new Dictionary<Vector2Int, TileView>();
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var view = _views[x, y];
                    if (view != null && view.gameObject.activeSelf)
                    {
                        // Use the view's internal tracking
                        viewsByPosition[new Vector2Int(view.GridX, view.GridY)] = view;
                    }
                }
            }
            
            // Clear the views array
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _views[x, y] = null;
                }
            }
            
            // Rebuild based on data layer
            int synced = 0;
            int created = 0;
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var data = boardController.GetTile(x, y);
                    if (data != null)
                    {
                        // Try to find existing view at this logical position
                        if (viewsByPosition.TryGetValue(new Vector2Int(x, y), out var existingView))
                        {
                            _views[x, y] = existingView;
                            existingView.UpdateGridPosition(x, y);
                            synced++;
                            viewsByPosition.Remove(new Vector2Int(x, y));
                        }
                        else
                        {
                            // No view exists - create one
                            var newView = CreateTileView(data);
                            created++;
                        }
                    }
                }
            }
            
            // Return orphaned views to pool
            foreach (var orphan in viewsByPosition.Values)
            {
                TilePool.Return(orphan);
            }
            
            if (created > 0)
            {
                Debug.Log($"SyncViewsWithData: synced {synced}, created {created} missing views");
            }
        }
        
        /// <summary>
        /// Validates that all tiles in the data layer have corresponding views.
        /// Creates missing views and logs any discrepancies.
        /// Call this after cascade sequences to catch any sync issues.
        /// </summary>
        public void ValidateBoard(Core.BoardController boardController)
        {
            int missingViews = 0;
            int orphanedViews = 0;
            
            // Check for missing views
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var data = boardController.GetTile(x, y);
                    var view = _views[x, y];
                    
                    if (data != null && (view == null || !view.gameObject.activeSelf))
                    {
                        // Data exists but no view - create one
                        Debug.LogWarning($"ValidateBoard: Missing view at ({x},{y}) for tile type {data.Type} - repairing");
                        var newView = CreateTileView(data);
                        newView.transform.position = GridToWorld(x, y);
                        newView.transform.localScale = Vector3.one;
                        missingViews++;
                    }
                    else if (data == null && view != null && view.gameObject.activeSelf)
                    {
                        // View exists but no data - orphaned
                        Debug.LogWarning($"ValidateBoard: Orphaned view at ({x},{y}) - returning to pool");
                        TilePool.Return(view);
                        _views[x, y] = null;
                        orphanedViews++;
                    }
                }
            }
            
            if (missingViews > 0 || orphanedViews > 0)
            {
                Debug.LogError($"ValidateBoard: Repaired {missingViews} missing views, removed {orphanedViews} orphaned views");
            }
        }
    }
}

