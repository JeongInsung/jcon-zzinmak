using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zzinmak.Core;

namespace Zzinmak.UI
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text survivalTimeText;
        [SerializeField] private TMP_Text killCountText;
        [SerializeField] private TMP_Text bestTimeText;
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string mainMenuSceneName = "2 SCENE";

        private void OnEnable()
        {
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= HandleGameOver;
        }

        private void HandleGameOver()
        {
            float elapsed = GameManager.Instance != null ? GameManager.Instance.ElapsedTime : 0f;
            int kills = GameManager.Instance != null ? GameManager.Instance.KillCount : 0;

            if (survivalTimeText != null)
            {
                survivalTimeText.text = "생존 시간  " + FormatTime(elapsed);
            }

            if (killCountText != null)
            {
                killCountText.text = "처치 수  " + kills;
            }

            bool isNewRecord = SurvivalRecords.TrySubmit(elapsed);
            float bestTime = SurvivalRecords.GetAll()[0];

            if (bestTimeText != null)
            {
                bestTimeText.text = "최고 기록  " + SurvivalRecords.Format(bestTime) + (isNewRecord ? "  NEW!" : "");
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        private static string FormatTime(float elapsed)
        {
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
