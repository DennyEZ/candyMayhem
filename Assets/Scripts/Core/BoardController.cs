using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Match3.Data;

namespace Match3.Core
{
    /// <summary>
    /// Pure logic controller for the game board.
    /// Manages the data grid and match detection.
    /// Does NOT handle visuals - that's BoardView's job.
    /// </summary>
    public class BoardController : MonoBehaviour
    {
        [Title("Debug")]
        [ShowInInspector, ReadOnly]
        private TileData[,] _grid;
        
        [ShowInInspector, ReadOnly]
        public int Width { get; private set; }
        
        [ShowInInspector, ReadOnly]
        public int Height { get; private set; }
        
        private Levels.LevelData _currentLevel;
        private System.Random _random;
        
        // Events for BoardView to listen to
        public event Action<int, int, TileData> OnTileCreated;
        public event Action<int, int, TileData> OnTileChanged;
        public event Action<List<MatchInfo>> OnMatchesFound;
        public event Action<TileData, TileData> OnTilesSwapped;
        public event Action<List<TileData>> OnTilesCleared;
        public event Action<List<(TileData tile, int fromY, int toY)>> OnTilesFell;
        
        /// <summary>
        /// Initializes the board with a level configuration.
        /// </summary>
        public void Initialize(Levels.LevelData levelData, int? seed = null)
        {
            _currentLevel = levelData;
            Width = levelData.Width;
            Height = levelData.Height;
            _random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
            
            _grid = new TileData[Width, Height];
            
            if (levelData.UseCustomLayout && levelData.InitialLayout != null)
            {
                LoadCustomLayout(levelData);
            }
            else
            {
                GenerateRandomBoard();
            }
            
            // Apply blockers
            if (levelData.UseBlockers)
            {
                ApplyBlockers(levelData);
            }
            
            // Ensure no initial matches
            ResolveInitialMatches();
        }
        
        private void LoadCustomLayout(Levels.LevelData levelData)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var type = levelData.InitialLayout[x, y];
                    if (type == TileType.None)
                    {
                        type = levelData.GetRandomColor();
                    }
                    CreateTile(x, y, type);
                }
            }
        }
        
        private void GenerateRandomBoard()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var type = _currentLevel.GetRandomColor();
                    CreateTile(x, y, type);
                }
            }
        }
        
        private void ApplyBlockers(Levels.LevelData levelData)
        {
            foreach (var kvp in levelData.BlockerPositions)
            {
                var pos = kvp.Key;
                var blockerType = kvp.Value;
                
                if (IsValidPosition(pos.x, pos.y))
                {
                    // For ice, keep the gem underneath
                    if (blockerType.IsIce())
                    {
                        // Ice is an overlay - the gem stays
                        // We'll handle this in the view layer
                    }
                    else
                    {
                        CreateTile(pos.x, pos.y, blockerType);
                    }
                }
            }
        }
        
        private void ResolveInitialMatches()
        {
            int maxIterations = 100;
            int iterations = 0;
            
            while (HasAnyMatch() && iterations < maxIterations)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        var tile = _grid[x, y];
                        if (tile != null && tile.Type.IsNormalGem())
                        {
                            if (IsPartOfMatch(x, y))
                            {
                                // Replace with a different color
                                tile.Type = GetNonMatchingColor(x, y);
                                OnTileChanged?.Invoke(x, y, tile);
                            }
                        }
                    }
                }
                iterations++;
            }
        }
        
        private TileType GetNonMatchingColor(int x, int y)
        {
            var availableColors = new List<TileType>(_currentLevel.AvailableColors);
            
            // Remove colors that would create matches
            var colorsToAvoid = new HashSet<TileType>();
            
            // Check horizontal
            if (x >= 2)
            {
                var left1 = GetTile(x - 1, y);
                var left2 = GetTile(x - 2, y);
                if (left1 != null && left2 != null && left1.Type == left2.Type)
                    colorsToAvoid.Add(left1.Type);
            }
            
            // Check vertical
            if (y >= 2)
            {
                var down1 = GetTile(x, y - 1);
                var down2 = GetTile(x, y - 2);
                if (down1 != null && down2 != null && down1.Type == down2.Type)
                    colorsToAvoid.Add(down1.Type);
            }
            
            availableColors.RemoveAll(c => colorsToAvoid.Contains(c));
            
            if (availableColors.Count == 0)
                availableColors = new List<TileType>(_currentLevel.AvailableColors);
            
            return availableColors[_random.Next(availableColors.Count)];
        }
        
        private void CreateTile(int x, int y, TileType type)
        {
            var tile = new TileData(type, x, y);
            _grid[x, y] = tile;
            OnTileCreated?.Invoke(x, y, tile);
        }
        
        public TileData GetTile(int x, int y)
        {
            if (!IsValidPosition(x, y)) return null;
            return _grid[x, y];
        }
        
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
        
        /// <summary>
        /// Clears a single tile from the grid.
        /// </summary>
        public void ClearTile(int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                _grid[x, y] = null;
            }
        }
        
        /// <summary>
        /// Attempts to swap two adjacent tiles.
        /// Returns true if the swap is valid (creates a match).
        /// </summary>
        public bool TrySwap(int x1, int y1, int x2, int y2)
        {
            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
                return false;
            
            var tile1 = _grid[x1, y1];
            var tile2 = _grid[x2, y2];
            
            if (tile1 == null || tile2 == null)
                return false;
            
            if (!tile1.Type.CanFall() || !tile2.Type.CanFall())
                return false;
            
            // Perform swap in data
            SwapTiles(x1, y1, x2, y2);
            
            // Check if this creates a match
            bool createsMatch = IsPartOfMatch(x1, y1) || IsPartOfMatch(x2, y2);
            
            // Check for special tile combinations (always valid)
            bool isSpecialCombo = tile1.Type.IsSpecialTile() && tile2.Type.IsSpecialTile();
            
            if (createsMatch || isSpecialCombo)
            {
                OnTilesSwapped?.Invoke(tile1, tile2);
                return true;
            }
            
            // Swap back - invalid move
            SwapTiles(x1, y1, x2, y2);
            return false;
        }
        
        public void SwapTiles(int x1, int y1, int x2, int y2)
        {
            var temp = _grid[x1, y1];
            _grid[x1, y1] = _grid[x2, y2];
            _grid[x2, y2] = temp;
            
            if (_grid[x1, y1] != null)
                _grid[x1, y1].SetPosition(x1, y1);
            if (_grid[x2, y2] != null)
                _grid[x2, y2].SetPosition(x2, y2);
        }
        
        /// <summary>
        /// Finds all matches on the board.
        /// </summary>
        public List<MatchInfo> FindAllMatches()
        {
            var matches = new List<MatchInfo>();
            var processed = new HashSet<Vector2Int>();
            
            // Find horizontal matches
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width - 2; x++)
                {
                    var match = FindHorizontalMatch(x, y, processed);
                    if (match != null)
                        matches.Add(match);
                }
            }
            
            // Find vertical matches
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height - 2; y++)
                {
                    var match = FindVerticalMatch(x, y, processed);
                    if (match != null)
                        matches.Add(match);
                }
            }
            
            // TODO: Detect L and T shapes by combining overlapping matches
            
            if (matches.Count > 0)
            {
                OnMatchesFound?.Invoke(matches);
            }
            
            return matches;
        }
        
        private MatchInfo FindHorizontalMatch(int startX, int y, HashSet<Vector2Int> processed)
        {
            var tile = GetTile(startX, y);
            if (tile == null || !tile.CanMatch()) return null;
            
            var positions = new List<Vector2Int> { new Vector2Int(startX, y) };
            
            // Extend right
            for (int x = startX + 1; x < Width; x++)
            {
                var next = GetTile(x, y);
                if (next != null && tile.MatchesWith(next))
                {
                    positions.Add(new Vector2Int(x, y));
                }
                else break;
            }
            
            if (positions.Count >= 3)
            {
                // Mark as processed
                foreach (var pos in positions)
                    processed.Add(pos);
                
                var matchType = positions.Count >= 5 ? MatchType.Match5 :
                               positions.Count == 4 ? MatchType.Match4 : MatchType.Match3;
                
                return new MatchInfo(matchType, positions.ToArray(), tile.Type);
            }
            
            return null;
        }
        
        private MatchInfo FindVerticalMatch(int x, int startY, HashSet<Vector2Int> processed)
        {
            var tile = GetTile(x, startY);
            if (tile == null || !tile.CanMatch()) return null;
            
            var positions = new List<Vector2Int> { new Vector2Int(x, startY) };
            
            // Extend up
            for (int y = startY + 1; y < Height; y++)
            {
                var next = GetTile(x, y);
                if (next != null && tile.MatchesWith(next))
                {
                    positions.Add(new Vector2Int(x, y));
                }
                else break;
            }
            
            if (positions.Count >= 3)
            {
                foreach (var pos in positions)
                    processed.Add(pos);
                
                var matchType = positions.Count >= 5 ? MatchType.Match5 :
                               positions.Count == 4 ? MatchType.Match4 : MatchType.Match3;
                
                return new MatchInfo(matchType, positions.ToArray(), tile.Type);
            }
            
            return null;
        }
        
        private bool HasAnyMatch()
        {
            return FindAllMatches().Count > 0;
        }
        
        private bool IsPartOfMatch(int x, int y)
        {
            var tile = GetTile(x, y);
            if (tile == null || !tile.CanMatch()) return false;
            
            // Check horizontal
            int hCount = 1;
            for (int i = x - 1; i >= 0 && GetTile(i, y)?.MatchesWith(tile) == true; i--) hCount++;
            for (int i = x + 1; i < Width && GetTile(i, y)?.MatchesWith(tile) == true; i++) hCount++;
            
            if (hCount >= 3) return true;
            
            // Check vertical
            int vCount = 1;
            for (int j = y - 1; j >= 0 && GetTile(x, j)?.MatchesWith(tile) == true; j--) vCount++;
            for (int j = y + 1; j < Height && GetTile(x, j)?.MatchesWith(tile) == true; j++) vCount++;
            
            return vCount >= 3;
        }
        
        /// <summary>
        /// Clears matched tiles and returns them for goal tracking.
        /// </summary>
        public List<TileData> ClearMatches(List<MatchInfo> matches)
        {
            var clearedTiles = new List<TileData>();
            var clearedPositions = new HashSet<Vector2Int>();
            
            foreach (var match in matches)
            {
                foreach (var pos in match.Positions)
                {
                    if (clearedPositions.Contains(pos)) continue;
                    
                    var tile = GetTile(pos.x, pos.y);
                    if (tile != null)
                    {
                        clearedTiles.Add(tile);
                        clearedPositions.Add(pos);
                        _grid[pos.x, pos.y] = null;
                    }
                }
                
                // Create special tile at center if applicable
                var specialType = match.GetSpecialTileType(match.Positions[0].y == match.Positions[1].y);
                if (specialType != TileType.None)
                {
                    CreateSpecialTile(match.CenterPosition.x, match.CenterPosition.y, specialType, match.TileColor);
                }
            }
            
            OnTilesCleared?.Invoke(clearedTiles);
            return clearedTiles;
        }
        
        private void CreateSpecialTile(int x, int y, TileType specialType, TileType underlyingColor)
        {
            var tile = new TileData(specialType, x, y);
            tile.UnderlyingColor = underlyingColor;  // Preserve the color so it can match with other tiles
            _grid[x, y] = tile;
            OnTileCreated?.Invoke(x, y, tile);
            Debug.Log($"Created special tile {specialType} at ({x},{y}) with underlying color {underlyingColor}");
        }
        
        /// <summary>
        /// Makes tiles fall to fill gaps. Returns fall information for animation.
        /// Uses a simple gravity simulation: for each column, move all movable tiles
        /// down to fill any null/empty spaces.
        /// </summary>
        public List<(TileData tile, int fromY, int toY)> CollapseColumns()
        {
            var falls = new List<(TileData, int, int)>();
            
            for (int x = 0; x < Width; x++)
            {
                // Collect all movable tiles in this column (bottom to top)
                var movableTiles = new List<(TileData tile, int originalY)>();
                int lowestEmptyY = -1;
                
                for (int y = 0; y < Height; y++)
                {
                    var tile = _grid[x, y];
                    
                    if (tile == null)
                    {
                        // Empty space - track it
                        if (lowestEmptyY < 0) lowestEmptyY = y;
                    }
                    else if (tile.Type == TileType.None || tile.Type == TileType.Empty)
                    {
                        // Empty marker - treat as empty space for collapse but don't remove
                        if (tile.Type == TileType.Empty)
                        {
                            // Board hole - tiles can't fall through this
                            // Process tiles above this point separately
                            continue;
                        }
                        if (lowestEmptyY < 0) lowestEmptyY = y;
                        _grid[x, y] = null; // Clear None type
                    }
                    else if (tile.Type.BlocksMovement())
                    {
                        // Blocker - can't move, resets the fill position
                        lowestEmptyY = -1;
                    }
                    else if (tile.Type.CanFall())
                    {
                        // Movable tile - collect it
                        movableTiles.Add((tile, y));
                    }
                    else
                    {
                        // Edge case: tile exists but doesn't fall or block
                        // Treat as static (shouldn't happen normally)
                        Debug.LogWarning($"CollapseColumns: Tile at ({x},{y}) type {tile.Type} doesn't fall or block - treating as static");
                        lowestEmptyY = -1;
                    }
                }
                
                // Now place tiles back from the bottom up
                int writeY = 0;
                foreach (var (tile, originalY) in movableTiles)
                {
                    // Skip past any permanent blockers
                    while (writeY < Height)
                    {
                        var existingTile = _grid[x, writeY];
                        if (existingTile != null && existingTile.Type.BlocksMovement())
                        {
                            writeY++;
                        }
                        else if (existingTile != null && existingTile.Type == TileType.Empty)
                        {
                            writeY++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    if (writeY >= Height) break;
                    
                    if (originalY != writeY)
                    {
                        // Tile needs to fall
                        _grid[x, writeY] = tile;
                        if (originalY > writeY)
                        {
                            _grid[x, originalY] = null;
                        }
                        tile.SetPosition(x, writeY);
                        falls.Add((tile, originalY, writeY));
                    }
                    writeY++;
                }
                
                // Clear any remaining positions above the last tile (they should be null for spawning)
                for (int y = writeY; y < Height; y++)
                {
                    var existingTile = _grid[x, y];
                    if (existingTile != null && !existingTile.Type.BlocksMovement() && existingTile.Type != TileType.Empty)
                    {
                        // This tile should have been moved - something went wrong
                        Debug.LogError($"CollapseColumns: Leftover tile at ({x},{y}) type {existingTile.Type} after collapse - clearing");
                        _grid[x, y] = null;
                    }
                }
            }
            
            if (falls.Count > 0)
            {
                Debug.Log($"CollapseColumns: {falls.Count} tiles falling");
            }
            
            OnTilesFell?.Invoke(falls);
            return falls;
        }
        
        /// <summary>
        /// Spawns new tiles at the top to fill empty spaces.
        /// </summary>
        public List<TileData> SpawnNewTiles()
        {
            var newTiles = new List<TileData>();
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_grid[x, y] == null)
                    {
                        var type = _currentLevel.GetRandomColor();
                        var tile = new TileData(type, x, y) { IsNew = true };
                        _grid[x, y] = tile;
                        newTiles.Add(tile);
                        OnTileCreated?.Invoke(x, y, tile);
                    }
                }
            }
            
            return newTiles;
        }
        
        /// <summary>
        /// Checks if any valid moves exist on the board.
        /// </summary>
        public bool HasValidMoves()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Try swap right
                    if (x < Width - 1 && WouldCreateMatch(x, y, x + 1, y))
                        return true;
                    
                    // Try swap up
                    if (y < Height - 1 && WouldCreateMatch(x, y, x, y + 1))
                        return true;
                }
            }
            return false;
        }
        
        private bool WouldCreateMatch(int x1, int y1, int x2, int y2)
        {
            SwapTiles(x1, y1, x2, y2);
            bool hasMatch = IsPartOfMatch(x1, y1) || IsPartOfMatch(x2, y2);
            SwapTiles(x1, y1, x2, y2); // Swap back
            return hasMatch;
        }
    }
}
