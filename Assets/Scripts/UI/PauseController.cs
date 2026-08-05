using UnityEngine;
using UnityEngine.SceneManagement;
using Zzinmak.Core;

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
            if (UiClickSound.Instance != null) UiClickSound.Instance.Play();

            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void ReturnToMainMenu()
        {
            if (UiClickSound.Instance != null) UiClickSound.Instance.Play();

            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void QuitGame()
        {
            if (UiClickSound.Instance != null) UiClickSound.Instance.Play();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
