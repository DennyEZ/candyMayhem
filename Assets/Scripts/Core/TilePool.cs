using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Match3.Views;

namespace Match3.Core
{
    /// <summary>
    /// Object pool for TileView objects.
    /// Prevents GC spikes by reusing objects instead of Instantiate/Destroy.
    /// </summary>
    public class TilePool : MonoBehaviour
    {
        [Title("Prefab")]
        [Required]
        public TileView TilePrefab;
        
        [Title("Pool Settings")]
        [PropertyRange(20, 200)]
        public int InitialPoolSize = 100;
        
        [ShowInInspector, ReadOnly]
        private Queue<TileView> _pool;
        
        [ShowInInspector, ReadOnly]
        private List<TileView> _activeViews;
        
        [ShowInInspector, ReadOnly]
        public int PooledCount => _pool?.Count ?? 0;
        
        [ShowInInspector, ReadOnly]
        public int ActiveCount => _activeViews?.Count ?? 0;
        
        private Transform _poolParent;
        
        private void Awake()
        {
            _pool = new Queue<TileView>();
            _activeViews = new List<TileView>();
            
            // Create pool parent for organization
            _poolParent = new GameObject("TilePool").transform;
            _poolParent.SetParent(transform);
        }
        
        /// <summary>
        /// Pre-warms the pool with a specified number of tiles.
        /// </summary>
        public void Initialize(int count)
        {
            // Clear existing pool
            foreach (var view in _activeViews)
            {
                if (view != null) Destroy(view.gameObject);
            }
            _activeViews.Clear();
            
            while (_pool.Count > 0)
            {
                var view = _pool.Dequeue();
                if (view != null) Destroy(view.gameObject);
            }
            
            // Create fresh pool
            for (int i = 0; i < count; i++)
            {
                var tile = CreateNewTile();
                tile.gameObject.SetActive(false);
                _pool.Enqueue(tile);
            }
            
            Debug.Log($"TilePool initialized with {count} tiles");
        }
        
        /// <summary>
        /// Gets a tile from the pool.
        /// </summary>
        public TileView Get()
        {
            TileView tile;
            
            if (_pool.Count > 0)
            {
                tile = _pool.Dequeue();
            }
            else
            {
                // Pool exhausted - create new tile
                tile = CreateNewTile();
                Debug.LogWarning("TilePool exhausted - creating new tile on demand");
            }
            
            tile.gameObject.SetActive(true);
            tile.Reset();
            _activeViews.Add(tile);
            
            return tile;
        }
        
        /// <summary>
        /// Returns a tile to the pool.
        /// </summary>
        public void Return(TileView tile)
        {
            if (tile == null) return;
            
            tile.gameObject.SetActive(false);
            tile.transform.SetParent(_poolParent);
            tile.Reset();
            
            _activeViews.Remove(tile);
            _pool.Enqueue(tile);
        }
        
        /// <summary>
        /// Returns all active tiles to the pool.
        /// </summary>
        [Button("Return All")]
        public void ReturnAll()
        {
            var tilesToReturn = new List<TileView>(_activeViews);
            foreach (var tile in tilesToReturn)
            {
                Return(tile);
            }
        }
        
        private TileView CreateNewTile()
        {
            var tile = Instantiate(TilePrefab, _poolParent);
            tile.name = $"Tile_{_pool.Count + _activeViews.Count}";
            return tile;
        }
    }
}
