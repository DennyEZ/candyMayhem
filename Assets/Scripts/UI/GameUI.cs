using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using Match3.Core;
using Match3.Levels;

namespace Match3.UI
{
    /// <summary>
    /// Main UI controller for the game.
    /// Displays score, moves, goals, and handles win/lose panels.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Title("References")]
        public GameManager GameManager;
        public Image BackgroundImage;
        [Required]
        public Views.TileSpriteConfig SpriteConfig;
        
        [Title("Header UI")]
        public TextMeshProUGUI LevelText;
        public TextMeshProUGUI MovesText;
        
        [Title("Goal UI")]
        public Transform GoalsContainer;
        public GameObject GoalItemPrefab;
        
        [Title("Panels")]
        public ResultPanel WinPanel;
        public ResultPanel LosePanel;
        public GameObject PausePanel;
        
        [Title("Buttons")]
        public Button PauseButton;
        public Button HUD_RestartButton;
        
        private List<GoalItemUI> _goalItems = new List<GoalItemUI>();
        
        private void Start()
        {
            if (GameManager == null)
                GameManager = FindAnyObjectByType<GameManager>();
            
            if (GameManager != null)
            {
                BindEvents();
            }
            
            // Hide panels initially
            if (WinPanel != null) WinPanel.Hide();
            if (LosePanel != null) LosePanel.Hide();
            if (PausePanel != null) PausePanel.SetActive(false);
            
            // Setup buttons
            if (PauseButton != null)
                PauseButton.onClick.AddListener(OnPauseClicked);
                
            if (HUD_RestartButton != null)
                HUD_RestartButton.onClick.AddListener(OnRestartClicked);
                
            // Setup Panel Events
            if (WinPanel != null)
            {
                WinPanel.OnNextLevelClicked.AddListener(OnNextLevelClicked);
                WinPanel.OnRestartClicked.AddListener(OnRestartClicked);
                WinPanel.OnQuitClicked.AddListener(OnQuitClicked);
                WinPanel.OnHomeClicked.AddListener(OnHomeClicked);
            }
            
            if (LosePanel != null)
            {
                LosePanel.OnRestartClicked.AddListener(OnRestartClicked);
                LosePanel.OnQuitClicked.AddListener(OnQuitClicked);
                LosePanel.OnHomeClicked.AddListener(OnHomeClicked);
            }
        }
        
        private void BindEvents()
        {
            GameManager.OnMovesChanged += UpdateMoves;
            GameManager.OnGoalProgress += UpdateGoalProgress;
            GameManager.OnLevelComplete += ShowWinPanel;
            GameManager.OnGameOver += ShowLosePanel;
            GameManager.OnStateChanged += OnGameStateChanged;
        }
        
        private void OnDestroy()
        {
            if (GameManager != null)
            {
                GameManager.OnMovesChanged -= UpdateMoves;
                GameManager.OnGoalProgress -= UpdateGoalProgress;
                GameManager.OnLevelComplete -= ShowWinPanel;
                GameManager.OnGameOver -= ShowLosePanel;
                GameManager.OnStateChanged -= OnGameStateChanged;
            }
        }
        
        /// <summary>
        /// Initializes UI for a new level.
        /// </summary>
        public void InitializeForLevel(LevelData level)
        {
            Debug.Log($"GameUI: Initializing for Level {level.LevelNumber}...");
            
            UpdateLevel(level.LevelNumber);
            
            if (BackgroundImage != null && level.BackgroundSprite != null)
            {
                BackgroundImage.sprite = level.BackgroundSprite;
            }
            
            UpdateMoves(level.MaxMoves);
            CreateGoalItems(level.Goals);
            
            // Ensure panels are hidden at start of level
            if (WinPanel != null) WinPanel.Hide();
            if (LosePanel != null) LosePanel.Hide();
            if (PausePanel != null) PausePanel.SetActive(false);
        }
        
        private void UpdateLevel(int levelNumber)
        {
            if (LevelText != null)
            {
                LevelText.text = levelNumber.ToString();
            }
            else
            {
                Debug.LogWarning("GameUI: LevelText reference is missing!");
            }
        }
        
        private void CreateGoalItems(List<LevelGoal> goals)
        {
            // Clear existing goal items
            foreach (var item in _goalItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _goalItems.Clear();
            
            Debug.Log($"GameUI: Creating {goals.Count} goal items...");
            
            if (GoalsContainer == null) Debug.LogWarning("GameUI: GoalsContainer reference is missing!");
            if (GoalItemPrefab == null) Debug.LogWarning("GameUI: GoalItemPrefab reference is missing!");
            
            // Create new goal items
            if (GoalsContainer != null && GoalItemPrefab != null)
            {
                foreach (var goal in goals)
                {
                    var itemGO = Instantiate(GoalItemPrefab, GoalsContainer);
                    
                    // Force reset transform to avoid invisible UI issues
                    itemGO.transform.localScale = Vector3.one;
                    itemGO.transform.localPosition = Vector3.zero; // Let LayoutGroup handle X/Y, but ensure Z is 0
                    
                    var itemUI = itemGO.GetComponent<GoalItemUI>();
                    if (itemUI != null)
                    {
                        Sprite goalSprite = null;
                        
                        // If it's a gem goal, get the specific sprite
                        if (goal.Type == GoalType.CollectGem && SpriteConfig != null)
                        {
                            goalSprite = SpriteConfig.GetSprite(goal.TargetTileType);
                        }
                        
                        itemUI.Setup(goal, goalSprite);
                        _goalItems.Add(itemUI);
                    }
                    else
                    {
                        Debug.LogError("GameUI: GoalItemPrefab is missing GoalItemUI component!");
                    }
                }
            }
        }
        
        // Score removed

        
        private void UpdateMoves(int moves)
        {
            if (MovesText != null)
            {
                MovesText.text = moves.ToString();
                
                // Flash red when low on moves
                if (moves <= 5)
                {
                    MovesText.color = Color.red;
                }
                else
                {
                    MovesText.color = Color.white;
                }
            }
        }
        
        private void UpdateGoalProgress(LevelGoal goal)
        {
            foreach (var item in _goalItems)
            {
                if (item.Goal.Type == goal.Type && 
                    item.Goal.TargetTileType == goal.TargetTileType)
                {
                    item.UpdateProgress(goal);
                    break;
                }
            }
        }
        
        private void OnGameStateChanged(GameState state)
        {
            if (PauseButton != null)
            {
                PauseButton.interactable = state == GameState.WaitingForInput;
            }
        }
        
        private void ShowWinPanel()
        {
            if (WinPanel != null)
            {
                WinPanel.Show();
            }
        }
        
        private void ShowLosePanel()
        {
            if (LosePanel != null)
            {
                LosePanel.Show();
            }
        }
        
        private void OnPauseClicked()
        {
            if (GameManager != null)
            {
                GameManager.TogglePause();
                if (PausePanel != null)
                {
                    PausePanel.SetActive(GameManager.CurrentState == GameState.Paused);
                }
            }
        }
        
        private void OnRestartClicked()
        {
            if (GameManager != null && GameManager.CurrentLevel != null)
            {
                if (WinPanel != null) WinPanel.Hide();
                if (LosePanel != null) LosePanel.Hide();
                if (PausePanel != null) PausePanel.SetActive(false);
                
                GameManager.StartLevel(GameManager.CurrentLevel);
            }
        }

        private void OnQuitClicked()
        {
            
            SceneManager.LoadScene("MainMenu");
        }
        private void OnNextLevelClicked()
        {
            if (GameManager != null)
            {
                // Hide panels before new level starts
                if (WinPanel != null) WinPanel.Hide();
                if (LosePanel != null) LosePanel.Hide();
                
                GameManager.LoadNextLevel();
            }
        }
        
        private void OnHomeClicked()
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }
}
