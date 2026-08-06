using UnityEngine;

namespace Zzinmak.Core
{
    public enum GameState { Playing, LevelUp, GameOver }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private const int ScorePerKill = 10;

        public GameState State { get; private set; } = GameState.Playing;
        public float ElapsedTime { get; private set; }
        public int KillCount { get; private set; }
        public int Score { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // vSyncCount is ignored on Android/iOS; targetFrameRate is the authoritative cap
            // there. Setting both explicitly stops frame pacing from depending on whichever
            // quality level happens to be active (editor "Very Low" runs vSync 0, Android
            // defaults to "Medium" with vSync 1) or on the display's native refresh rate.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (State != GameState.Playing) return;

            ElapsedTime += Time.deltaTime;
            GameEvents.RaiseTimerChanged(ElapsedTime);
        }

        public void AddKill()
        {
            KillCount++;
            Score += ScorePerKill;
            GameEvents.RaiseScoreChanged(Score);
        }

        public void EnterLevelUp()
        {
            if (State != GameState.Playing) return;

            State = GameState.LevelUp;
            Time.timeScale = 0f;
        }

        public void ExitLevelUp()
        {
            if (State != GameState.LevelUp) return;

            State = GameState.Playing;
            Time.timeScale = 1f;
        }

        public void TriggerGameOver()
        {
            if (State == GameState.GameOver) return;

            State = GameState.GameOver;
            Time.timeScale = 0f;
            GameEvents.RaiseGameOver();
        }
    }
}
