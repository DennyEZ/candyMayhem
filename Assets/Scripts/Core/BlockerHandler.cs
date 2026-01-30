using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Match3.Data;

namespace Match3.Core
{
    /// <summary>
    /// Handles blocker tile mechanics (ice, stone, crates).
    /// </summary>
    public class BlockerHandler : MonoBehaviour
    {
        [Title("References")]
        [Required]
        public BoardController BoardController;
        
        /// <summary>
        /// Ice overlay data - stored separately from the grid.
        /// Key: grid position, Value: ice layer count (1-3)
        /// </summary>
        [ShowInInspector, ReadOnly]
        private Dictionary<Vector2Int, int> _iceOverlays = new Dictionary<Vector2Int, int>();
        
        /// <summary>
        /// Crate health data.
        /// Key: grid position, Value: hits remaining
        /// </summary>
        [ShowInInspector, ReadOnly]
        private Dictionary<Vector2Int, int> _crateHealth = new Dictionary<Vector2Int, int>();
        
        public event System.Action<Vector2Int, int> OnIceCracked;
        public event System.Action<Vector2Int> OnIceDestroyed;
        public event System.Action<Vector2Int, int> OnCrateDamaged;
        public event System.Action<Vector2Int, TileType> OnCrateDestroyed;
        
        /// <summary>
        /// Initializes blockers from level data.
        /// </summary>
        public void InitializeBlockers(Levels.LevelData levelData)
        {
            _iceOverlays.Clear();
            _crateHealth.Clear();
            
            if (!levelData.UseBlockers) return;
            
            foreach (var kvp in levelData.BlockerPositions)
            {
                var pos = kvp.Key;
                var blockerType = kvp.Value;
                
                if (blockerType.IsIce())
                {
                    int layers = blockerType == TileType.Ice1 ? 1 :
                                 blockerType == TileType.Ice2 ? 2 : 3;
                    _iceOverlays[pos] = layers;
                }
                else if (blockerType.IsCrate())
                {
                    int health = blockerType == TileType.Crate1 ? 1 : 2;
                    _crateHealth[pos] = health;
                }
            }
        }
        
        /// <summary>
        /// Called when a match is made adjacent to blockers.
        /// Returns list of positions that were affected.
        /// </summary>
        public List<Vector2Int> ProcessAdjacentMatch(List<Vector2Int> matchPositions)
        {
            var affectedBlockers = new List<Vector2Int>();
            var adjacentPositions = new HashSet<Vector2Int>();
            
            // Collect all adjacent positions
            foreach (var pos in matchPositions)
            {
                adjacentPositions.Add(new Vector2Int(pos.x - 1, pos.y));
                adjacentPositions.Add(new Vector2Int(pos.x + 1, pos.y));
                adjacentPositions.Add(new Vector2Int(pos.x, pos.y - 1));
                adjacentPositions.Add(new Vector2Int(pos.x, pos.y + 1));
            }
            
            // Check ice overlays
            foreach (var pos in adjacentPositions)
            {
                if (_iceOverlays.ContainsKey(pos))
                {
                    DamageIce(pos);
                    affectedBlockers.Add(pos);
                }
            }
            
            // Check crates at match positions (crates break when matched)
            foreach (var pos in matchPositions)
            {
                if (_crateHealth.ContainsKey(pos))
                {
                    DamageCrate(pos);
                    affectedBlockers.Add(pos);
                }
            }
            
            return affectedBlockers;
        }
        
        /// <summary>
        /// Damages ice at position, cracking or destroying it.
        /// </summary>
        public void DamageIce(Vector2Int pos)
        {
            if (!_iceOverlays.ContainsKey(pos)) return;
            
            _iceOverlays[pos]--;
            
            if (_iceOverlays[pos] <= 0)
            {
                _iceOverlays.Remove(pos);
                OnIceDestroyed?.Invoke(pos);
            }
            else
            {
                OnIceCracked?.Invoke(pos, _iceOverlays[pos]);
            }
        }
        
        /// <summary>
        /// Damages crate at position.
        /// </summary>
        public void DamageCrate(Vector2Int pos)
        {
            if (!_crateHealth.ContainsKey(pos)) return;
            
            _crateHealth[pos]--;
            
            if (_crateHealth[pos] <= 0)
            {
                _crateHealth.Remove(pos);
                
                // Crate is destroyed - spawn a random gem at this position
                var tile = BoardController.GetTile(pos.x, pos.y);
                var newType = tile?.UnderlyingColor ?? TileType.Red;
                OnCrateDestroyed?.Invoke(pos, newType);
            }
            else
            {
                OnCrateDamaged?.Invoke(pos, _crateHealth[pos]);
            }
        }
        
        /// <summary>
        /// Checks if a position has ice overlay.
        /// </summary>
        public bool HasIce(int x, int y)
        {
            return _iceOverlays.ContainsKey(new Vector2Int(x, y));
        }
        
        /// <summary>
        /// Gets ice layer count at position (0 if none).
        /// </summary>
        public int GetIceLayers(int x, int y)
        {
            var pos = new Vector2Int(x, y);
            return _iceOverlays.ContainsKey(pos) ? _iceOverlays[pos] : 0;
        }
        
        /// <summary>
        /// Checks if position has a crate.
        /// </summary>
        public bool HasCrate(int x, int y)
        {
            return _crateHealth.ContainsKey(new Vector2Int(x, y));
        }
    }
}
