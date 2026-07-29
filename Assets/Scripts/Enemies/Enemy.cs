using System;
using System.Collections;
using UnityEngine;
using Zzinmak.Combat;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Progression;

namespace Zzinmak.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private GameObject expGemPrefab;
        [Tooltip("Seconds to wait after death before returning to the pool. 0 = return immediately (default zombie behavior). Set >0 to let a death animation play first.")]
        [SerializeField] private float deathReturnDelay = 0f;
        [Tooltip("Set true if the source art faces left by default (most sprites here face right). Flips the flipX mapping so the enemy still visually faces the direction it's moving/chasing.")]
        [SerializeField] private bool spriteFacesLeftByDefault = false;

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private HitFlash hitFlash;
        private Transform target;
        private EnemyData data;
        private float currentHealth;
        private float maxHealth;
        private float lastContactDamageTime;

        public event Action OnDeath;
        public event Action<float, float> OnHealthChanged;

        public Transform Target => target;
        public bool IsDead { get; private set; }
        public bool CanMove { get; set; } = true;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            hitFlash = GetComponent<HitFlash>();
        }

        private void OnEnable()
        {
            EnemyTracker.Register(this);
        }

        private void OnDisable()
        {
            EnemyTracker.Unregister(this);
        }

        public void Initialize(EnemyData enemyData, Transform playerTarget, float healthMultiplier)
        {
            data = enemyData;
            target = playerTarget;
            maxHealth = data.baseHealth * healthMultiplier;
            currentHealth = maxHealth;
            lastContactDamageTime = -data.contactDamageInterval;
            IsDead = false;
            CanMove = true;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void FixedUpdate()
        {
            if (target == null || data == null || IsDead || !CanMove) return;

            Vector2 dir = ((Vector2)target.position - body.position).normalized;
            body.MovePosition(body.position + dir * data.moveSpeed * Time.fixedDeltaTime);

            if (spriteRenderer != null && Mathf.Abs(dir.x) > 0.01f)
            {
                bool shouldFlip = dir.x < 0f;
                spriteRenderer.flipX = spriteFacesLeftByDefault ? !shouldFlip : shouldFlip;
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            currentHealth -= amount;
            OnHealthChanged?.Invoke(Mathf.Max(currentHealth, 0f), maxHealth);
            hitFlash?.Flash();

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            GameManager.Instance.AddKill();

            if (expGemPrefab != null)
            {
                var gemObj = ObjectPool.Instance.Get(expGemPrefab, transform.position, Quaternion.identity);
                var gem = gemObj.GetComponent<ExpGem>();
                gem.SetValue(data.expReward);
            }

            OnDeath?.Invoke();

            if (deathReturnDelay > 0f)
            {
                StartCoroutine(ReturnToPoolAfterDelay(deathReturnDelay));
            }
            else
            {
                ObjectPool.Instance.Return(gameObject);
            }
        }

        private IEnumerator ReturnToPoolAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ObjectPool.Instance.Return(gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (data == null || IsDead) return;
            if (!other.CompareTag("Player")) return;
            if (Time.time - lastContactDamageTime < data.contactDamageInterval) return;

            lastContactDamageTime = Time.time;
            var health = other.GetComponent<Player.PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(data.contactDamage);
            }
        }
    }
}
