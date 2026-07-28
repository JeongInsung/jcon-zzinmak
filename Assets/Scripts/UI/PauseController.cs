using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zzinmak.UI
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private string mainMenuSceneName = "2 SCENE";

        public void Open()
        {
            if (pausePanel != null) pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
