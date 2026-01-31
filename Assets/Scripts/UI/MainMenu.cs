using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3.UI
{
    public class MainMenu : MonoBehaviour
    {
        public void PlayGame()
        {
            SceneManager.LoadScene("LevelSelect");
        }

        public void QuitGame()
        {
            Debug.Log("Quit Game requested");
            Application.Quit();
        }
    }
}
