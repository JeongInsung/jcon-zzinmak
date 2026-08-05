using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zzinmak.Core;
using Zzinmak.Player;

namespace Zzinmak.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Image expBarFill;
        [SerializeField] private Image healthBarFill;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text scoreText;

        [Header("Bar fill smoothing")]
        [Tooltip("How much of the bar (0-1) it can sweep per second.")]
        [SerializeField] private float barFillSpeed = 1.5f;

        private float expTargetFill;
        private float healthTargetFill;

        [Header("Stat panel (below exp bar)")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text attackSpeedText;
        [SerializeField] private TMP_Text moveSpeedText;

        private void Update()
        {
            if (expBarFill != null)
            {
                expBarFill.fillAmount = Mathf.MoveTowards(expBarFill.fillAmount, expTargetFill, barFillSpeed * Time.deltaTime);
            }

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = Mathf.MoveTowards(healthBarFill.fillAmount, healthTargetFill, barFillSpeed * Time.deltaTime);
            }

            if (playerStats == null) return;

            if (attackText != null) attackText.text = "공격력 " + Mathf.RoundToInt(playerStats.damageMultiplier * 100f) + "%";
            if (attackSpeedText != null) attackSpeedText.text = "공격속도 " + Mathf.RoundToInt(playerStats.attackSpeedMultiplier * 100f) + "%";
            if (moveSpeedText != null) moveSpeedText.text = "이동속도 " + playerStats.moveSpeed.ToString("0.0");
        }

        private void OnEnable()
        {
            GameEvents.OnExpChanged += HandleExpChanged;
            GameEvents.OnHealthChanged += HandleHealthChanged;
            GameEvents.OnLevelChanged += HandleLevelChanged;
            GameEvents.OnTimerChanged += HandleTimerChanged;
            GameEvents.OnScoreChanged += HandleScoreChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnExpChanged -= HandleExpChanged;
            GameEvents.OnHealthChanged -= HandleHealthChanged;
            GameEvents.OnLevelChanged -= HandleLevelChanged;
            GameEvents.OnTimerChanged -= HandleTimerChanged;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
        }

        private void HandleExpChanged(float current, float needed)
        {
            expTargetFill = needed > 0f ? current / needed : 0f;
        }

        private void HandleHealthChanged(float current, float max)
        {
            healthTargetFill = max > 0f ? current / max : 0f;

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

        private void HandleScoreChanged(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = "Score: " + score;
            }
        }
    }
}
