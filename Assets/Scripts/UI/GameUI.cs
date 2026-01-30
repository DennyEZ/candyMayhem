using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        
        [Title("Header UI")]
        public TextMeshProUGUI ScoreText;
        public TextMeshProUGUI MovesText;
        
        [Title("Goal UI")]
        public Transform GoalsContainer;
        public GameObject GoalItemPrefab;
        
        [Title("Panels")]
        public GameObject WinPanel;
        public GameObject LosePanel;
        public GameObject PausePanel;
        
        [Title("Buttons")]
        public Button PauseButton;
        public Button RestartButton;
        public Button NextLevelButton;
        public Button QuitButton;
        
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
            if (WinPanel != null) WinPanel.SetActive(false);
            if (LosePanel != null) LosePanel.SetActive(false);
            if (PausePanel != null) PausePanel.SetActive(false);
            
            // Setup buttons
            if (PauseButton != null)
                PauseButton.onClick.AddListener(OnPauseClicked);
            if (RestartButton != null)
                RestartButton.onClick.AddListener(OnRestartClicked);
        }
        
        private void BindEvents()
        {
            GameManager.OnScoreChanged += UpdateScore;
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
                GameManager.OnScoreChanged -= UpdateScore;
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
            UpdateScore(0);
            UpdateMoves(level.MaxMoves);
            CreateGoalItems(level.Goals);
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
            
            // Create new goal items
            if (GoalsContainer != null && GoalItemPrefab != null)
            {
                foreach (var goal in goals)
                {
                    var itemGO = Instantiate(GoalItemPrefab, GoalsContainer);
                    var itemUI = itemGO.GetComponent<GoalItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.Setup(goal);
                        _goalItems.Add(itemUI);
                    }
                }
            }
        }
        
        private void UpdateScore(int score)
        {
            if (ScoreText != null)
            {
                ScoreText.text = score.ToString("N0");
            }
        }
        
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
                WinPanel.SetActive(true);
            }
        }
        
        private void ShowLosePanel()
        {
            if (LosePanel != null)
            {
                LosePanel.SetActive(true);
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
                if (WinPanel != null) WinPanel.SetActive(false);
                if (LosePanel != null) LosePanel.SetActive(false);
                if (PausePanel != null) PausePanel.SetActive(false);
                
                GameManager.StartLevel(GameManager.CurrentLevel);
            }
        }
    }
}
