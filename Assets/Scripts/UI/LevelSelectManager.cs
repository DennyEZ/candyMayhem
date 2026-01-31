using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Match3.Core;
using Match3.Levels;

namespace Match3.UI
{
    public class LevelSelectManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private LevelItemView _levelItemPrefab;
        [SerializeField] private string _menuSceneName = "MainMenu";
        [SerializeField] private string _gameSceneName = "SampleScene";
        
        [Header("Data")]
        [SerializeField] private List<LevelData> _levels;
        
        private void Start()
        {
            Debug.Log("LevelSelectManager: Start called.");
            
            if (_gridContainer.GetComponent<UnityEngine.UI.LayoutGroup>() == null)
            {
                Debug.LogError("LevelSelectManager: 'Grid Container' is missing a Layout Group component (Grid Layout Group)! Buttons will stack on top of each other.");
            }
            
            InitializeGrid();
        }
        
        private void InitializeGrid()
        {
            if (_levels == null || _levels.Count == 0)
            {
                Debug.LogError("LevelSelectManager: No Level Data assigned in inspector!");
                return;
            }

            Debug.Log($"LevelSelectManager: Found {_levels.Count} levels configured.");

            // Clear existing items (safe reverse loop)
            int childCount = _gridContainer.childCount;
            Debug.Log($"LevelSelectManager: Clearing {childCount} existing items.");
            
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(_gridContainer.GetChild(i).gameObject);
            }
            
            int unlockedLevel = ProgressionManager.GetUnlockedLevel();
            Debug.Log($"LevelSelectManager: Player unlocked up to level {unlockedLevel}");
            
            foreach (var levelData in _levels)
            {
                if (levelData == null)
                {
                    Debug.LogWarning("LevelSelectManager: Found null slot in Levels list!");
                    continue;
                }
                
                var itemView = Instantiate(_levelItemPrefab, _gridContainer);
                itemView.gameObject.name = $"LevelButton_{levelData.LevelNumber}";
                
                bool isLocked = levelData.LevelNumber > unlockedLevel;
                
                Debug.Log($"Creating button for Level {levelData.LevelNumber} (Locked: {isLocked})");
                
                if (itemView != null)
                {
                    itemView.Initialize(levelData.LevelNumber, isLocked);
                    itemView.OnClick += () => OnLevelClicked(levelData);
                }
                else
                {
                    Debug.LogError("LevelSelectManager: Instantiated object has no LevelItemView component!");
                }
            }
        }
        
        private void OnLevelClicked(LevelData level)
        {
            Debug.Log($"Selected Level: {level.LevelNumber}");
            
            // Set context
            LevelContext.SelectedLevel = level;
            
            // Load Game
            SceneLoader.LoadScene(_gameSceneName);
        }
        
        public void OnBackButtonClicked()
        {
            SceneManager.LoadScene(_menuSceneName);
        }
    }
    
    // Simple helper to avoid string typos if needed elsewhere, 
    // or just use SceneManager directly.
    public static class SceneLoader 
    {
        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
