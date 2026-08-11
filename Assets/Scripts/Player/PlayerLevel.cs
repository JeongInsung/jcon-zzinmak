using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Progression;

namespace Zzinmak.Player
{
    public class PlayerLevel : MonoBehaviour
    {
        [SerializeField] private LevelUpManager levelUpManager;

        public int Level { get; private set; } = 40;
        public float CurrentExp { get; private set; }
        public float ExpToNextLevel { get; private set; }

        private void Start()
        {
            ExpToNextLevel = CalculateExpForLevel(Level);
            GameEvents.RaiseLevelChanged(Level);
            GameEvents.RaiseExpChanged(CurrentExp, ExpToNextLevel);
        }

        public void AddExp(float amount)
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver) return;

            CurrentExp += amount;
            while (CurrentExp >= ExpToNextLevel)
            {
                CurrentExp -= ExpToNextLevel;
                Level++;
                ExpToNextLevel = CalculateExpForLevel(Level);
                GameEvents.RaiseLevelChanged(Level);
                levelUpManager.TriggerLevelUp();
            }

            GameEvents.RaiseExpChanged(CurrentExp, ExpToNextLevel);
        }

        private float CalculateExpForLevel(int level)
        {
            return 4f + (level - 1) * 3f;
        }

        // Bypasses the exp curve entirely - always exactly one level, regardless of how
        // much exp the source was "worth" or how close the player already was to leveling.
        // Used by boss kills, which shouldn't scale with the exp curve like normal gems do.
        public void ForceLevelUp()
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver) return;

            Level++;
            ExpToNextLevel = CalculateExpForLevel(Level);
            GameEvents.RaiseLevelChanged(Level);
            levelUpManager.TriggerLevelUp();
            GameEvents.RaiseExpChanged(CurrentExp, ExpToNextLevel);
        }
    }
}
