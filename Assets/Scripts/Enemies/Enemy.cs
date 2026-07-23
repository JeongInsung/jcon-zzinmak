using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Progression;

namespace Zzinmak.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private GameObject expGemPrefab;

        private Rigidbody2D body;
        private Transform target;
        private EnemyData data;
        private float currentHealth;
        private float lastContactDamageTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void Initialize(EnemyData enemyData, Transform playerTarget, float healthMultiplier)
        {
            data = enemyData;
            target = playerTarget;
            currentHealth = data.baseHealth * healthMultiplier;
            lastContactDamageTime = -data.contactDamageInterval;
        }

        private void FixedUpdate()
        {
            if (target == null || data == null) return;

            Vector2 dir = ((Vector2)target.position - body.position).normalized;
            body.MovePosition(body.position + dir * data.moveSpeed * Time.fixedDeltaTime);
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            GameManager.Instance.AddKill();

            if (expGemPrefab != null)
            {
                var gemObj = ObjectPool.Instance.Get(expGemPrefab, transform.position, Quaternion.identity);
                var gem = gemObj.GetComponent<ExpGem>();
                gem.SetValue(data.expReward);
            }

            ObjectPool.Instance.Return(gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (data == null) return;
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
