using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Environment;
using Zzinmak.Player;

namespace Zzinmak.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BossProjectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 4f;
        [Header("Damage over time (optional - tickCount 0 disables it)")]
        [SerializeField] private float dotDamagePerTick = 0f;
        [SerializeField] private float dotTickInterval = 1f;
        [SerializeField] private int dotTickCount = 0;

        private Vector2 velocity;
        private float damage;
        private float remainingLife;

        public void Launch(Vector2 direction, float speed, float dmg)
        {
            Vector2 dir = direction.normalized;
            velocity = dir * speed;
            damage = dmg;
            remainingLife = lifetime;
            FaceDirection(dir);
        }

        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);

            remainingLife -= Time.deltaTime;
            if (remainingLife <= 0f)
            {
                ObjectPool.Instance.Return(gameObject);
            }
        }

        private void FaceDirection(Vector2 dir)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (BuildingCollision.IsFootCollider(other))
            {
                ObjectPool.Instance.Return(gameObject);
                return;
            }

            if (!other.CompareTag("Player")) return;

            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                if (dotTickCount > 0)
                {
                    health.ApplyDamageOverTime(dotDamagePerTick, dotTickInterval, dotTickCount);
                }
            }

            ObjectPool.Instance.Return(gameObject);
        }
    }
}
