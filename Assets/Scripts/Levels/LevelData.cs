using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Match3.Levels
{
    /// <summary>
    /// Goal types for level objectives.
    /// </summary>
    public enum GoalType
    {
        CollectGem,     // Collect X gems of a color
        BreakIce,       // Break X ice tiles
        BreakCrate,     // Break X crates
        BreakStone,     // Break X stone (if destroyable)
        ReachScore,     // Reach X points
    }
    
    /// <summary>
    /// A single goal objective for a level.
    /// </summary>
    [Serializable]
    public class LevelGoal
    {
        [HorizontalGroup("Goal", Width = 0.3f)]
        [HideLabel]
        public GoalType Type;
        
        [HorizontalGroup("Goal", Width = 0.3f)]
        [HideLabel]
        [ShowIf("@Type == GoalType.CollectGem")]
        public Data.TileType TargetTileType;
        
        [HorizontalGroup("Goal", Width = 0.2f)]
        [HideLabel]
        [SuffixLabel("target")]
        public int TargetAmount = 10;
        
        [HideInInspector]
        public int CurrentAmount;
        
        public bool IsComplete => CurrentAmount >= TargetAmount;
        
        public float Progress => Mathf.Clamp01((float)CurrentAmount / TargetAmount);
        
        public void Reset()
        {
            CurrentAmount = 0;
        }
        
        public void AddProgress(int amount = 1)
        {
            CurrentAmount = Mathf.Min(CurrentAmount + amount, TargetAmount);
        }
    }
    
    /// <summary>
    /// ScriptableObject defining a level's configuration.
    /// Uses Odin Inspector for a powerful level editor experience.
    /// </summary>
    [CreateAssetMenu(fileName = "Level_", menuName = "Match3/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Title("Board Configuration")]
        [PropertyRange(5, 12)]
        public int Width = 8;
        
        [PropertyRange(5, 12)]
        public int Height = 8;
        
        [Title("Available Tile Colors")]
        [InfoBox("Select which gem colors can spawn in this level. More colors = harder.")]
        public List<Data.TileType> AvailableColors = new List<Data.TileType>
        {
            Data.TileType.Red,
            Data.TileType.Blue,
            Data.TileType.Green,
            Data.TileType.Yellow
        };
        
        [Title("Level Goals")]
        [InfoBox("Define what the player must accomplish to complete this level.")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        public List<LevelGoal> Goals = new List<LevelGoal>();
        
        [Title("Move Limit")]
        [PropertyRange(10, 100)]
        public int MaxMoves = 30;
        
        [Title("Initial Board Layout")]
        [InfoBox("Optional: Define a custom starting layout. Leave empty for random generation.")]
        [ShowIf("@UseCustomLayout")]
        [TableMatrix(HorizontalTitle = "X", VerticalTitle = "Y", SquareCells = true)]
        public Data.TileType[,] InitialLayout;
        
        [ToggleLeft]
        public bool UseCustomLayout = false;
        
        [Title("Blockers")]
        [InfoBox("Define blocker positions. Format: (x, y) -> TileType")]
        [ShowIf("@UseBlockers")]
        [DictionaryDrawerSettings(KeyLabel = "Position", ValueLabel = "Blocker Type")]
        public Dictionary<Vector2Int, Data.TileType> BlockerPositions = new Dictionary<Vector2Int, Data.TileType>();
        
        [ToggleLeft]
        public bool UseBlockers = false;
        
        [Title("Difficulty Settings")]
        [PropertyRange(0f, 1f)]
        [InfoBox("Higher values make special tile creation easier.")]
        public float SpecialTileBonus = 0.1f;

        [Button("Initialize Layout Grid", ButtonSizes.Large)]
        [ShowIf("UseCustomLayout")]
        private void InitializeLayoutGrid()
        {
            InitialLayout = new Data.TileType[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    InitialLayout[x, y] = Data.TileType.None;
                }
            }
        }
        
        [Button("Validate Level", ButtonSizes.Medium)]
        private void ValidateLevel()
        {
            var errors = new List<string>();
            
            if (AvailableColors.Count < 3)
                errors.Add("Need at least 3 colors for a playable game!");
            
            if (Goals.Count == 0)
                errors.Add("Level has no goals defined!");
            
            if (MaxMoves < 10)
                errors.Add("Move limit is very low. Is this intentional?");
            
            if (errors.Count == 0)
                Debug.Log($"✓ Level '{name}' is valid!");
            else
            {
                foreach (var error in errors)
                    Debug.LogWarning($"Level '{name}': {error}");
            }
        }
        
        /// <summary>
        /// Creates runtime copies of goals to track progress.
        /// </summary>
        public List<LevelGoal> CreateGoalInstances()
        {
            var instances = new List<LevelGoal>();
            foreach (var goal in Goals)
            {
                instances.Add(new LevelGoal
                {
                    Type = goal.Type,
                    TargetTileType = goal.TargetTileType,
                    TargetAmount = goal.TargetAmount,
                    CurrentAmount = 0
                });
            }
            return instances;
        }
        
        /// <summary>
        /// Gets a random available color for spawning new tiles.
        /// </summary>
        public Data.TileType GetRandomColor()
        {
            if (AvailableColors.Count == 0) return Data.TileType.Red;
            return AvailableColors[UnityEngine.Random.Range(0, AvailableColors.Count)];
        }
    }
}
