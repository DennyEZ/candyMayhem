using UnityEngine;
using UnityEditor;
using Match3.Core;
using Match3.Views;
using Match3.UI;
using Match3.Levels;

namespace Match3.Editor
{
    /// <summary>
    /// Automatically sets up the game scene with all required GameObjects.
    /// Run this from Match3 menu after creating sprites and levels.
    /// </summary>
    public static class SceneSetup
    {
#if UNITY_EDITOR
        [MenuItem("Match3/Setup Game Scene")]
        public static void SetupScene()
        {
            // 1. Create Tile Prefab if it doesn't exist
            CreateTilePrefab();
            
            // 2. Create Game Manager
            var gameManagerGO = new GameObject("GameManager");
            var gameManager = gameManagerGO.AddComponent<GameManager>();
            var boardController = gameManagerGO.AddComponent<BoardController>();
            var inputHandler = gameManagerGO.AddComponent<InputHandler>();
            var tilePool = gameManagerGO.AddComponent<TilePool>();
            var specialHandler = gameManagerGO.AddComponent<SpecialTileHandler>();
            var blockerHandler = gameManagerGO.AddComponent<BlockerHandler>();
            
            // 3. Create Board View
            var boardGO = new GameObject("Board");
            boardGO.transform.SetParent(gameManagerGO.transform);
            var boardView = boardGO.AddComponent<BoardView>();
            
            var tileParent = new GameObject("TileParent");
            tileParent.transform.SetParent(boardGO.transform);
            
            // 4. Wire references
            gameManager.BoardController = boardController;
            gameManager.BoardView = boardView;
            gameManager.InputHandler = inputHandler;
            gameManager.TilePool = tilePool;
            
            // Load tile prefab
            var tilePrefab = AssetDatabase.LoadAssetAtPath<TileView>("Assets/Prefabs/Tiles/Tile.prefab");
            if (tilePrefab != null)
            {
                tilePool.TilePrefab = tilePrefab;
            }
            else
            {
                Debug.LogWarning("Tile prefab not found! Please create it at Assets/Prefabs/Tiles/Tile.prefab");
            }
            
            boardView.TilePool = tilePool;
            boardView.TileParent = tileParent.transform;
            
            specialHandler.BoardController = boardController;
            blockerHandler.BoardController = boardController;
            
            // 5. Try to load Level_01
            var level = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Levels/Level_01.asset");
            if (level != null)
            {
                gameManager.CurrentLevel = level;
                Debug.Log("✓ Loaded Level_01");
            }
            else
            {
                Debug.LogWarning("Level_01 not found! Run Match3 → Generate Sample Levels first.");
            }
            
            // 6. Setup Camera
            var camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 6f;
                camera.transform.position = new Vector3(0, 0, -10);
                camera.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
            }
            
            // 7. Mark as dirty for saving
            EditorUtility.SetDirty(gameManagerGO);
            
            Debug.Log("✓ Game scene setup complete! Press Play to test.");
            Debug.Log("If tiles don't appear, make sure you've run:");
            Debug.Log("  1. Match3 → Generate Placeholder Sprites");
            Debug.Log("  2. Match3 → Generate Sample Levels");
            
            Selection.activeGameObject = gameManagerGO;
        }
        
        private static void CreateTilePrefab()
        {
            // Check if prefab already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tiles/Tile.prefab") != null)
            {
                Debug.Log("Tile prefab already exists");
                return;
            }
            
            // Ensure directories exist
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Tiles"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Tiles");
            
            // Create tile GameObject
            var tileGO = new GameObject("Tile");
            var spriteRenderer = tileGO.AddComponent<SpriteRenderer>();
            var tileView = tileGO.AddComponent<TileView>();
            
            // Set default sprite (Unity's built-in Knob)
            spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            if (spriteRenderer.sprite == null)
            {
                // Try loading generated sprite
                var generatedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Gems/Red.png");
                if (generatedSprite != null)
                    spriteRenderer.sprite = generatedSprite;
            }
            
            // Set sorting order so tiles appear above background
            spriteRenderer.sortingOrder = 1;
            
            // Scale down the tile
            tileGO.transform.localScale = Vector3.one * 0.8f;
            
            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(tileGO, "Assets/Prefabs/Tiles/Tile.prefab");
            
            // Cleanup scene object
            Object.DestroyImmediate(tileGO);
            
            Debug.Log("✓ Created Tile prefab at Assets/Prefabs/Tiles/Tile.prefab");
        }
        
        [MenuItem("Match3/Quick Full Setup")]
        public static void QuickFullSetup()
        {
            Debug.Log("=== Running Full Match-3 Setup ===");
            
            // Generate sprites
            PlaceholderSpriteGenerator.GenerateSprites();
            
            // Generate levels
            SampleLevelGenerator.GenerateSampleLevels();
            
            // Setup scene
            SetupScene();
            
            Debug.Log("=== Full setup complete! Press Play to test. ===");
        }
#endif
    }
}
