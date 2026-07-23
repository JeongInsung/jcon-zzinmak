using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Enemies;

namespace Zzinmak.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;

        private Vector2 velocity;
        private float damage;
        private float remainingLife;

        public void Launch(Vector3 targetPosition, float speed, float dmg)
        {
            Vector2 dir = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
            velocity = dir * speed;
            damage = dmg;
            remainingLife = lifetime;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            ObjectPool.Instance.Return(gameObject);
        }
    }
}
