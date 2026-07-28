using UnityEngine;
using Zzinmak.Combat;
using Zzinmak.Core;

namespace Zzinmak.Player
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float invulnerabilityDuration = 0.5f;

        private PlayerStats stats;
        private HitFlash hitFlash;
        private float currentHealth;
        private float invulnerableUntil;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            hitFlash = GetComponent<HitFlash>();
        }

        private void Start()
        {
            currentHealth = stats.maxHealth;
            GameEvents.RaiseHealthChanged(currentHealth, stats.maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (Time.time < invulnerableUntil) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            currentHealth -= amount;
            invulnerableUntil = Time.time + invulnerabilityDuration;
            GameEvents.RaiseHealthChanged(Mathf.Max(currentHealth, 0f), stats.maxHealth);
            hitFlash?.Flash();

            if (currentHealth <= 0f)
            {
                GameManager.Instance?.TriggerGameOver();
            }
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, stats.maxHealth);
            GameEvents.RaiseHealthChanged(currentHealth, stats.maxHealth);
        }
    }
}
