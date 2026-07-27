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
