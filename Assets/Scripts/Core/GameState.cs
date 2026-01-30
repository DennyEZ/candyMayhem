namespace Match3.Core
{
    /// <summary>
    /// Game states for the Finite State Machine.
    /// Prevents input during animations and ensures clean game flow.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Game is initializing or loading a level.
        /// </summary>
        Initializing,
        
        /// <summary>
        /// Waiting for player input. Swipes are accepted.
        /// </summary>
        WaitingForInput,
        
        /// <summary>
        /// Two tiles are being swapped. Input is disabled.
        /// </summary>
        Swapping,
        
        /// <summary>
        /// Checking for matches after a swap or cascade.
        /// </summary>
        CheckingMatches,
        
        /// <summary>
        /// Matches found - playing match animations and effects.
        /// </summary>
        ClearingMatches,
        
        /// <summary>
        /// Tiles are falling to fill gaps. New tiles spawning from top.
        /// </summary>
        Collapsing,
        
        /// <summary>
        /// No valid moves remain - reshuffling the board.
        /// </summary>
        Shuffling,
        
        /// <summary>
        /// Level completed successfully.
        /// </summary>
        LevelComplete,
        
        /// <summary>
        /// Out of moves - game over.
        /// </summary>
        GameOver,
        
        /// <summary>
        /// Game is paused.
        /// </summary>
        Paused
    }
}
