using System.Collections;
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

        // Poison-style damage over time. Ticks bypass the post-hit invulnerability window
        // (and don't extend it) so a lingering effect keeps chipping away even while the
        // player is otherwise taking normal hits.
        public void ApplyDamageOverTime(float damagePerTick, float tickInterval, int tickCount)
        {
            StartCoroutine(DamageOverTimeRoutine(damagePerTick, tickInterval, tickCount));
        }

        private IEnumerator DamageOverTimeRoutine(float damagePerTick, float tickInterval, int tickCount)
        {
            for (int i = 0; i < tickCount; i++)
            {
                yield return new WaitForSeconds(tickInterval);
                ApplyDotTick(damagePerTick);
            }
        }

        private void ApplyDotTick(float amount)
        {
            if (currentHealth <= 0f) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            currentHealth -= amount;
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
