using Match3.Levels;

namespace Match3.Core
{
    /// <summary>
    /// Static context to pass data between scenes (LevelSelect -> GameScene).
    /// </summary>
    public static class LevelContext
    {
        public static LevelData SelectedLevel { get; set; }
    }
}
