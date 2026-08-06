using UnityEngine;
using Zzinmak.Combat;
using Zzinmak.Core;

namespace Zzinmak.Player
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float invulnerabilityDuration = 0.5f;

        [Tooltip("Guardian charm item: once armed, the next hit that brings health to this value or below starts blocking the following hits instead of dealing damage.")]
        [SerializeField] private float guardianHealthThreshold = 10f;
        [SerializeField] private int guardianBlockCount = 3;

        private PlayerStats stats;
        private HitFlash hitFlash;
        private float currentHealth;
        private float invulnerableUntil;
        private bool guardianArmed;
        private int guardianBlocksRemaining;

        public bool HasGuardianCharm => guardianArmed || guardianBlocksRemaining > 0;

        public void ArmGuardianCharm()
        {
            guardianArmed = true;
        }

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

            if (guardianBlocksRemaining > 0)
            {
                guardianBlocksRemaining--;
                invulnerableUntil = Time.time + invulnerabilityDuration;
                hitFlash?.Flash();
                return;
            }

            currentHealth -= amount;
            invulnerableUntil = Time.time + invulnerabilityDuration;
            GameEvents.RaiseHealthChanged(Mathf.Max(currentHealth, 0f), stats.maxHealth);
            hitFlash?.Flash();

            if (guardianArmed && currentHealth > 0f && currentHealth <= guardianHealthThreshold)
            {
                guardianArmed = false;
                guardianBlocksRemaining = guardianBlockCount;
            }

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
