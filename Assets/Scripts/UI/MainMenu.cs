using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3.UI
{
    public class MainMenu : MonoBehaviour
    {
        public void PlayGame()
        {
            // Assuming "SampleScene" is your game scene name
            SceneManager.LoadScene("SampleScene");
        }

        public void QuitGame()
        {
            Debug.Log("Quit Game requested");
            Application.Quit();
        }
    }
}
