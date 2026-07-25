using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zzinmak.Core;

namespace Zzinmak.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Image expBarFill;
        [SerializeField] private Image healthBarFill;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text killCountText;

        private void OnEnable()
        {
            GameEvents.OnExpChanged += HandleExpChanged;
            GameEvents.OnHealthChanged += HandleHealthChanged;
            GameEvents.OnLevelChanged += HandleLevelChanged;
            GameEvents.OnTimerChanged += HandleTimerChanged;
            GameEvents.OnKillCountChanged += HandleKillCountChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnExpChanged -= HandleExpChanged;
            GameEvents.OnHealthChanged -= HandleHealthChanged;
            GameEvents.OnLevelChanged -= HandleLevelChanged;
            GameEvents.OnTimerChanged -= HandleTimerChanged;
            GameEvents.OnKillCountChanged -= HandleKillCountChanged;
        }

        private void HandleExpChanged(float current, float needed)
        {
            if (expBarFill != null)
            {
                expBarFill.fillAmount = needed > 0f ? current / needed : 0f;
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = max > 0f ? current / max : 0f;
            }

            if (healthText != null)
            {
                healthText.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
            }
        }

        private void HandleLevelChanged(int level)
        {
            if (levelText != null)
            {
                levelText.text = "Lv." + level;
            }
        }

        private void HandleTimerChanged(float elapsed)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(elapsed / 60f);
                int seconds = Mathf.FloorToInt(elapsed % 60f);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        private void HandleKillCountChanged(int count)
        {
            if (killCountText != null)
            {
                killCountText.text = "Kills: " + count;
            }
        }
    }
}
