using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Match3.Levels;
using Match3.Data;

namespace Match3.UI
{
    /// <summary>
    /// UI component for displaying a single goal objective.
    /// </summary>
    public class GoalItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image IconImage;
        public TextMeshProUGUI ProgressText;
        public Image ProgressFill;
        public GameObject CompletedCheckmark;
        
        [Header("Icons by Goal Type")]
        public Sprite CollectGemIcon;
        public Sprite BreakIceIcon;
        public Sprite BreakCrateIcon;
        public Sprite ScoreIcon;
        
        public LevelGoal Goal { get; private set; }
        
        /// <summary>
        /// Sets up this goal item display.
        /// </summary>
        public void Setup(LevelGoal goal)
        {
            Goal = goal;
            
            // Set icon based on goal type
            if (IconImage != null)
            {
                IconImage.sprite = GetIconForGoal(goal);
                IconImage.color = GetColorForGoal(goal);
            }
            
            UpdateProgress(goal);
            
            if (CompletedCheckmark != null)
                CompletedCheckmark.SetActive(false);
        }
        
        /// <summary>
        /// Updates the progress display.
        /// </summary>
        public void UpdateProgress(LevelGoal goal)
        {
            Goal = goal;
            
            if (ProgressText != null)
            {
                ProgressText.text = $"{goal.CurrentAmount}/{goal.TargetAmount}";
            }
            
            if (ProgressFill != null)
            {
                ProgressFill.fillAmount = goal.Progress;
            }
            
            if (CompletedCheckmark != null)
            {
                CompletedCheckmark.SetActive(goal.IsComplete);
            }
            
            // Visual feedback when complete
            if (goal.IsComplete && ProgressText != null)
            {
                ProgressText.color = Color.green;
            }
        }
        
        private Sprite GetIconForGoal(LevelGoal goal)
        {
            switch (goal.Type)
            {
                case GoalType.CollectGem:
                    return CollectGemIcon;
                case GoalType.BreakIce:
                    return BreakIceIcon;
                case GoalType.BreakCrate:
                    return BreakCrateIcon;
                case GoalType.ReachScore:
                    return ScoreIcon;
                default:
                    return CollectGemIcon;
            }
        }
        
        private Color GetColorForGoal(LevelGoal goal)
        {
            if (goal.Type == GoalType.CollectGem)
            {
                // Use the gem color
                switch (goal.TargetTileType)
                {
                    case TileType.Red: return new Color(0.9f, 0.2f, 0.2f);
                    case TileType.Blue: return new Color(0.2f, 0.4f, 0.9f);
                    case TileType.Green: return new Color(0.2f, 0.8f, 0.3f);
                    case TileType.Yellow: return new Color(0.95f, 0.85f, 0.2f);
                    case TileType.Purple: return new Color(0.7f, 0.2f, 0.8f);
                    case TileType.Orange: return new Color(0.95f, 0.5f, 0.1f);
                    default: return Color.white;
                }
            }
            
            return Color.white;
        }
    }
}
