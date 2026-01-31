using UnityEngine;
using Sirenix.OdinInspector;

namespace Match3.Core
{
    /// <summary>
    /// Manages persistent player progress (unlocked levels).
    /// </summary>
    public class ProgressionManager : MonoBehaviour
    {
        private const string UNLOCKED_LEVEL_KEY = "HighestUnlockedLevel";

        public static int GetUnlockedLevel()
        {
            // Default to level 1 if no data exists
            return PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
        }

        public static void UnlockNextLevel(int currentCompletedLevel)
        {
            int currentUnlocked = GetUnlockedLevel();
            int nextLevel = currentCompletedLevel + 1;

            if (nextLevel > currentUnlocked)
            {
                PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, nextLevel);
                PlayerPrefs.Save();
                Debug.Log($"Progress Saved: Level {nextLevel} Unlocked!");
            }
        }

        public static void UnlockLevelDirectly(int levelNumber)
        {
            int currentUnlocked = GetUnlockedLevel();
            if (levelNumber > currentUnlocked)
            {
                PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, levelNumber);
                PlayerPrefs.Save();
            }
        }

        #if UNITY_EDITOR
        [Title("Debug Controls")]
        [Button("Reset Progress", ButtonSizes.Large)]
        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(UNLOCKED_LEVEL_KEY);
            PlayerPrefs.Save();
            Debug.Log("Progress Reset: Locked to Level 1.");
        }

        [Button("Unlock Level 5", ButtonSizes.Medium)]
        public void CheatUnlock()
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, 5);
            PlayerPrefs.Save();
            Debug.Log("Cheat: Unlocked up to Level 5");
        }
        #endif
    }
}
