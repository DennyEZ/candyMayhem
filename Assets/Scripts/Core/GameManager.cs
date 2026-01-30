using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Match3.Data;
using Match3.Levels;
using Match3.Views;

namespace Match3.Core
{
    /// <summary>
    /// Main game controller implementing FSM pattern.
    /// Coordinates all game systems and prevents invalid state transitions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Title("References")]
        [Required]
        public BoardController BoardController;
        
        [Required]
        public BoardView BoardView;
        
        [Required]
        public InputHandler InputHandler;
        
        [Required]
        public TilePool TilePool;
        
        [Title("Current Level")]
        [Required]
        public LevelData CurrentLevel;
        
        [Title("Game State")]
        [ShowInInspector, ReadOnly]
        private GameState _currentState = GameState.Initializing;
        
        public GameState CurrentState => _currentState;
        
        [ShowInInspector, ReadOnly]
        private int _movesRemaining;
        
        [ShowInInspector, ReadOnly]
        private int _currentScore;
        
        [ShowInInspector, ReadOnly]
        private List<LevelGoal> _activeGoals;
        
        // Events for UI
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnMovesChanged;
        public event Action<int> OnScoreChanged;
        public event Action<LevelGoal> OnGoalProgress;
        public event Action OnLevelComplete;
        public event Action OnGameOver;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            if (CurrentLevel != null)
            {
                StartLevel(CurrentLevel);
            }
        }
        
        /// <summary>
        /// Starts a new level.
        /// </summary>
        [Button("Start Level")]
        public void StartLevel(LevelData level)
        {
            CurrentLevel = level;
            _movesRemaining = level.MaxMoves;
            _currentScore = 0;
            _activeGoals = level.CreateGoalInstances();
            
            SetState(GameState.Initializing);
            
            // Initialize pool first
            TilePool.Initialize(level.Width * level.Height + 20);
            
            // Initialize board data
            BoardController.Initialize(level);
            
            // Initialize board view
            BoardView.Initialize(level.Width, level.Height);
            
            // Create visuals for the initial board
            CreateInitialBoardVisuals();
            
            // Initialize ice overlays (fixed to cells)
            if (level.UseIceOverlays && level.IcePositions != null)
            {
                BoardView.InitializeIceOverlays(level.IcePositions);
            }
            
            // Bind events
            BindEvents();
            
            // Ready for input
            SetState(GameState.WaitingForInput);
            
            OnMovesChanged?.Invoke(_movesRemaining);
            OnScoreChanged?.Invoke(_currentScore);
            
            Debug.Log($"Level started! Board: {level.Width}x{level.Height}, Moves: {_movesRemaining}");
        }
        
        private void CreateInitialBoardVisuals()
        {
            for (int x = 0; x < CurrentLevel.Width; x++)
            {
                for (int y = 0; y < CurrentLevel.Height; y++)
                {
                    var tileData = BoardController.GetTile(x, y);
                    if (tileData != null)
                    {
                        BoardView.CreateTileView(tileData);
                    }
                }
            }
            Debug.Log($"Created {CurrentLevel.Width * CurrentLevel.Height} tile visuals");
        }
        
        private void BindEvents()
        {
            // Unbind first to prevent duplicates
            InputHandler.OnSwipeDetected -= HandleSwipe;
            BoardController.OnTilesCleared -= HandleTilesCleared;
            BoardController.OnIceDamaged -= HandleIceDamaged;
            BoardController.OnCrateDamaged -= HandleCrateDamaged;
            BoardController.OnCrateDestroyed -= HandleCrateDestroyed;
            
            InputHandler.OnSwipeDetected += HandleSwipe;
            BoardController.OnTilesCleared += HandleTilesCleared;
            BoardController.OnIceDamaged += HandleIceDamaged;
            BoardController.OnCrateDamaged += HandleCrateDamaged;
            BoardController.OnCrateDestroyed += HandleCrateDestroyed;
        }
        
        private void OnDestroy()
        {
            if (InputHandler != null)
                InputHandler.OnSwipeDetected -= HandleSwipe;
            if (BoardController != null)
            {
                BoardController.OnTilesCleared -= HandleTilesCleared;
                BoardController.OnIceDamaged -= HandleIceDamaged;
                BoardController.OnCrateDamaged -= HandleCrateDamaged;
                BoardController.OnCrateDestroyed -= HandleCrateDestroyed;
            }
        }
        
        private void HandleIceDamaged(Data.TileData tile)
        {
            BoardView.UpdateIceOverlay(tile);
            
            // Track Break Ice goal only when ice is fully destroyed
            if (tile.IceLevel <= 0)
            {
                TrackIceBreakGoal();
            }
        }
        
        /// <summary>
        /// Tracks progress for Break Ice goals.
        /// </summary>
        private void TrackIceBreakGoal()
        {
            foreach (var goal in _activeGoals)
            {
                if (goal.IsComplete) continue;
                
                if (goal.Type == GoalType.BreakIce)
                {
                    goal.AddProgress();
                    OnGoalProgress?.Invoke(goal);
                }
            }
        }
        
        private void HandleCrateDamaged(Data.TileData tile)
        {
            BoardView.UpdateCrateSprite(tile);
        }
        
        private void HandleCrateDestroyed(UnityEngine.Vector2Int pos)
        {
            BoardView.RemoveCrateView(pos);
        }
        
        /// <summary>
        /// Handles player swipe input.
        /// </summary>
        private void HandleSwipe(int fromX, int fromY, int toX, int toY)
        {
            if (_currentState != GameState.WaitingForInput)
            {
                Debug.Log($"Ignoring swipe - current state: {_currentState}");
                return;
            }
            
            SetState(GameState.Swapping);
            StartCoroutine(ProcessSwap(fromX, fromY, toX, toY));
        }
        
        private IEnumerator ProcessSwap(int fromX, int fromY, int toX, int toY)
        {
            // Validate adjacency first
            if (!IsAdjacent(fromX, fromY, toX, toY))
            {
                Debug.Log($"Invalid swap: ({fromX},{fromY}) to ({toX},{toY}) are not adjacent");
                SetState(GameState.WaitingForInput);
                yield break;
            }
            
            // Check if either tile is a special tile - if so, activate it!
            var fromTile = BoardController.GetTile(fromX, fromY);
            var toTile = BoardController.GetTile(toX, toY);
            
            bool fromIsSpecial = fromTile != null && fromTile.Type.IsSpecialTile();
            bool toIsSpecial = toTile != null && toTile.Type.IsSpecialTile();
            
            if (fromIsSpecial || toIsSpecial)
            {
                // Use a move
                _movesRemaining--;
                OnMovesChanged?.Invoke(_movesRemaining);
                
                // Animate the swap
                yield return BoardView.AnimateSwap(fromX, fromY, toX, toY);
                BoardController.SwapTiles(fromX, fromY, toX, toY);
                
                // Activate special tiles
                var tilesToClear = new List<TileData>();
                
                if (fromIsSpecial)
                {
                    tilesToClear.AddRange(GetSpecialTileTargets(fromTile));
                    Debug.Log($"Activated {fromTile.Type} at ({fromX},{fromY})");
                }
                if (toIsSpecial)
                {
                    tilesToClear.AddRange(GetSpecialTileTargets(toTile));
                    Debug.Log($"Activated {toTile.Type} at ({toX},{toY})");
                }
                
                // Clear the targeted tiles
                if (tilesToClear.Count > 0)
                {
                    yield return ClearTilesDirectly(tilesToClear);
                    yield return ProcessMatchesAndCascade();
                }
                
                yield break;
            }
            
            // Normal swap logic for regular gems
            bool validSwap = BoardController.TrySwap(fromX, fromY, toX, toY);
            
            // Always animate the visual swap first
            yield return BoardView.AnimateSwap(fromX, fromY, toX, toY);
            
            if (validSwap)
            {
                // Use a move
                _movesRemaining--;
                OnMovesChanged?.Invoke(_movesRemaining);
                
                // Start match-cascade loop
                yield return ProcessMatchesAndCascade();
            }
            else
            {
                // Invalid move - animate swapping back
                Debug.Log("No match - swapping back");
                yield return BoardView.AnimateSwap(fromX, fromY, toX, toY);
                SetState(GameState.WaitingForInput);
            }
        }
        
        private List<TileData> GetSpecialTileTargets(TileData specialTile)
        {
            var targets = new List<TileData>();
            
            switch (specialTile.Type)
            {
                case TileType.HorizontalRocket:
                    // Clear entire row
                    for (int x = 0; x < BoardController.Width; x++)
                    {
                        var tile = BoardController.GetTile(x, specialTile.Y);
                        if (tile != null) targets.Add(tile);
                    }
                    break;
                    
                case TileType.VerticalRocket:
                    // Clear entire column
                    for (int y = 0; y < BoardController.Height; y++)
                    {
                        var tile = BoardController.GetTile(specialTile.X, y);
                        if (tile != null) targets.Add(tile);
                    }
                    break;
                    
                case TileType.Bomb:
                    // Clear 3x3 area
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int tx = specialTile.X + dx;
                            int ty = specialTile.Y + dy;
                            if (tx >= 0 && tx < BoardController.Width && ty >= 0 && ty < BoardController.Height)
                            {
                                var tile = BoardController.GetTile(tx, ty);
                                if (tile != null) targets.Add(tile);
                            }
                        }
                    }
                    break;
                    
                case TileType.Rainbow:
                    // Clear all tiles of a random color (or the swapped color)
                    // For now, clear a random normal gem type
                    var randomColor = (TileType)UnityEngine.Random.Range(10, 16);
                    for (int x = 0; x < BoardController.Width; x++)
                    {
                        for (int y = 0; y < BoardController.Height; y++)
                        {
                            var tile = BoardController.GetTile(x, y);
                            if (tile != null && tile.Type == randomColor)
                                targets.Add(tile);
                        }
                    }
                    break;
            }
            
            return targets;
        }
        
        private IEnumerator ClearTilesDirectly(List<TileData> tiles)
        {
            SetState(GameState.ClearingMatches);
            
            // Remove duplicates
            var uniqueTiles = new HashSet<TileData>(tiles);
            var tilesList = new List<TileData>(uniqueTiles);
            var actuallyCleared = new List<TileData>();
            
            // Clear from data - but respect ice!
            foreach (var tile in tilesList)
            {
                int x = tile.X;
                int y = tile.Y;
                
                // Check cell-level ice
                int iceLevel = BoardController.GetIceLevel(x, y);
                if (iceLevel > 0)
                {
                    // Damage ice instead of clearing tile
                    BoardController.SetIceLevel(x, y, iceLevel - 1);
                    int remaining = BoardController.GetIceLevel(x, y);
                    
                    // Update ice visual
                    var iceEventData = new Data.TileData(tile.Type, x, y);
                    iceEventData.IceLevel = remaining;
                    BoardView.UpdateIceOverlay(iceEventData);
                    
                    if (remaining <= 0)
                    {
                        // Ice destroyed - track goal
                        TrackIceBreakGoal();
                        
                        // Now clear the tile
                        BoardController.ClearTile(x, y);
                        actuallyCleared.Add(tile);
                    }
                    // If ice remains, tile stays
                }
                else
                {
                    // No ice - normal clear
                    BoardController.ClearTile(x, y);
                    actuallyCleared.Add(tile);
                }
            }
            
            // Animate clearing (only the tiles that were actually cleared)
            if (actuallyCleared.Count > 0)
            {
                yield return BoardView.AnimateClear(actuallyCleared);
            }
            
            // Add score
            AddScore(actuallyCleared.Count * 15);  // Bonus points for special tile
            
            // Sync views with data and collapse
            SetState(GameState.Collapsing);
            BoardView.SyncViewsWithData(BoardController);
            
            var falls = BoardController.CollapseColumns();
            
            // Spawn new tiles immediately to drop with the rest
            var newTiles = BoardController.SpawnNewTiles();
            
            // Animate both in parallel
            var fallRoutine = StartCoroutine(BoardView.AnimateFall(falls));
            var spawnRoutine = StartCoroutine(BoardView.AnimateSpawn(newTiles));
            
            yield return fallRoutine;
            yield return spawnRoutine;
            
            // Validate board after special tile effects
            BoardView.ValidateBoard(BoardController);
        }
        
        private bool IsAdjacent(int x1, int y1, int x2, int y2)
        {
            int dx = Mathf.Abs(x1 - x2);
            int dy = Mathf.Abs(y1 - y2);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
        
        private IEnumerator ProcessMatchesAndCascade()
        {
            bool hasMatches = true;
            
            while (hasMatches)
            {
                SetState(GameState.CheckingMatches);
                
                var matches = BoardController.FindAllMatches();
                
                if (matches.Count > 0)
                {
                    SetState(GameState.ClearingMatches);
                    
                    // Clear matches and track goals
                    var cleared = BoardController.ClearMatches(matches);
                    
                    // Animate clearing
                    yield return BoardView.AnimateClear(cleared);
                    
                    // Add score
                    AddScore(cleared.Count * 10);
                    
                    SetState(GameState.Collapsing);
                    
                    // Sync views with data before collapse (fixes cascade sync issues)
                    BoardView.SyncViewsWithData(BoardController);
                    
                    // Collapse columns
                    var falls = BoardController.CollapseColumns();
                    
                    // Spawn new tiles immediately
                    var newTiles = BoardController.SpawnNewTiles();
                    
                    // Animate both in parallel
                    var fallRoutine = StartCoroutine(BoardView.AnimateFall(falls));
                    var spawnRoutine = StartCoroutine(BoardView.AnimateSpawn(newTiles));
                    
                    yield return fallRoutine;
                    yield return spawnRoutine;
                }
                else
                {
                    hasMatches = false;
                }
            }
            
            // Check win/lose conditions
            if (CheckWinCondition())
            {
                SetState(GameState.LevelComplete);
                OnLevelComplete?.Invoke();
            }
            else if (_movesRemaining <= 0)
            {
                SetState(GameState.GameOver);
                OnGameOver?.Invoke();
            }
            else if (!BoardController.HasValidMoves())
            {
                SetState(GameState.Shuffling);
                // TODO: Implement shuffle
                yield return new WaitForSeconds(0.5f);
                SetState(GameState.WaitingForInput);
            }
            else
            {
                SetState(GameState.WaitingForInput);
            }
            
            // Final validation to catch any sync issues
            BoardView.ValidateBoard(BoardController);
        }
        
        private void HandleTilesCleared(List<Data.TileData> tiles)
        {
            foreach (var tile in tiles)
            {
                TrackGoalProgress(tile);
            }
        }
        
        private void TrackGoalProgress(Data.TileData tile)
        {
            foreach (var goal in _activeGoals)
            {
                if (goal.IsComplete) continue;
                
                bool shouldProgress = false;
                
                switch (goal.Type)
                {
                    case GoalType.CollectGem:
                        shouldProgress = tile.Type == goal.TargetTileType;
                        break;
                    // Note: BreakIce is tracked separately via HandleIceDamaged
                    case GoalType.BreakCrate:
                        shouldProgress = tile.Type.IsCrate();
                        break;
                }
                
                if (shouldProgress)
                {
                    goal.AddProgress();
                    OnGoalProgress?.Invoke(goal);
                }
            }
        }
        
        private bool CheckWinCondition()
        {
            foreach (var goal in _activeGoals)
            {
                if (!goal.IsComplete) return false;
            }
            return true;
        }
        
        private void AddScore(int points)
        {
            _currentScore += points;
            OnScoreChanged?.Invoke(_currentScore);
            
            // Check score-based goals
            foreach (var goal in _activeGoals)
            {
                if (goal.Type == GoalType.ReachScore && !goal.IsComplete)
                {
                    goal.CurrentAmount = _currentScore;
                    OnGoalProgress?.Invoke(goal);
                }
            }
        }
        
        private void SetState(GameState newState)
        {
            if (_currentState != newState)
            {
                Debug.Log($"GameState: {_currentState} -> {newState}");
                _currentState = newState;
                OnStateChanged?.Invoke(newState);
            }
        }
        
        /// <summary>
        /// Pauses/unpauses the game.
        /// </summary>
        public void TogglePause()
        {
            if (_currentState == GameState.Paused)
            {
                SetState(GameState.WaitingForInput);
                Time.timeScale = 1f;
            }
            else if (_currentState == GameState.WaitingForInput)
            {
                SetState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }
    }
}
