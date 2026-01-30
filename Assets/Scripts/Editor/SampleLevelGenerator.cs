using UnityEngine;
using UnityEditor;
using Match3.Levels;
using Match3.Data;
using System.Collections.Generic;

namespace Match3.Editor
{
    /// <summary>
    /// Creates sample levels for testing.
    /// </summary>
    public static class SampleLevelGenerator
    {
#if UNITY_EDITOR
        [MenuItem("Match3/Generate Sample Levels")]
        public static void GenerateSampleLevels()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Levels"))
            {
                AssetDatabase.CreateFolder("Assets", "Levels");
            }
            
            CreateLevel1();
            CreateLevel2();
            CreateLevel3();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("✓ Generated 3 sample levels in Assets/Levels/");
        }
        
        private static void CreateLevel1()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            level.Width = 8;
            level.Height = 8;
            level.MaxMoves = 30;
            level.AvailableColors = new List<TileType>
            {
                TileType.Red, TileType.Blue, TileType.Green, TileType.Yellow
            };
            level.Goals = new List<LevelGoal>
            {
                new LevelGoal { Type = GoalType.CollectGem, TargetTileType = TileType.Red, TargetAmount = 20 },
                new LevelGoal { Type = GoalType.CollectGem, TargetTileType = TileType.Blue, TargetAmount = 20 }
            };
            
            AssetDatabase.CreateAsset(level, "Assets/Levels/Level_01.asset");
        }
        
        private static void CreateLevel2()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            level.Width = 7;
            level.Height = 9;
            level.MaxMoves = 25;
            level.AvailableColors = new List<TileType>
            {
                TileType.Red, TileType.Blue, TileType.Green, TileType.Yellow, TileType.Purple
            };
            level.Goals = new List<LevelGoal>
            {
                new LevelGoal { Type = GoalType.CollectGem, TargetTileType = TileType.Purple, TargetAmount = 30 },
                new LevelGoal { Type = GoalType.ReachScore, TargetAmount = 5000 }
            };
            
            // Add some ice overlays using the new system
            level.UseIceOverlays = true;
            level.IcePositions = new Dictionary<Vector2Int, int>
            {
                { new Vector2Int(3, 3), 1 },  // 1 layer ice
                { new Vector2Int(3, 4), 2 },  // 2 layer ice
                { new Vector2Int(3, 5), 1 },  // 1 layer ice
            };
            
            AssetDatabase.CreateAsset(level, "Assets/Levels/Level_02.asset");
        }
        
        private static void CreateLevel3()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            level.Width = 9;
            level.Height = 9;
            level.MaxMoves = 35;
            level.AvailableColors = new List<TileType>
            {
                TileType.Red, TileType.Blue, TileType.Green, TileType.Yellow, TileType.Purple, TileType.Orange
            };
            level.Goals = new List<LevelGoal>
            {
                new LevelGoal { Type = GoalType.BreakIce, TargetAmount = 15 },
                new LevelGoal { Type = GoalType.CollectGem, TargetTileType = TileType.Orange, TargetAmount = 25 }
            };
            
            // Ice pattern using new overlay system
            level.UseIceOverlays = true;
            level.IcePositions = new Dictionary<Vector2Int, int>();
            
            // Create a diamond pattern of ice
            int center = 4;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (Mathf.Abs(i - 2) + Mathf.Abs(j - 2) <= 2)
                    {
                        int iceLevel = 3 - (Mathf.Abs(i - 2) + Mathf.Abs(j - 2));
                        level.IcePositions[new Vector2Int(center - 2 + i, center - 2 + j)] = iceLevel;
                    }
                }
            }
            
            AssetDatabase.CreateAsset(level, "Assets/Levels/Level_03.asset");
        }
#endif
    }
}
