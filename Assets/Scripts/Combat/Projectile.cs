using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Enemies;

namespace Zzinmak.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private ParticleSystem sparkle;
        [SerializeField] private Sprite[] animationFrames;
        [SerializeField] private float frameRate = 12f;

        private SpriteRenderer spriteRenderer;
        private Vector2 velocity;
        private float damage;
        private float remainingLife;
        private float frameTimer;
        private int frameIndex;
        private int pierceRemaining;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Launch(Vector3 targetPosition, float speed, float dmg, int pierceCount = 0)
        {
            Vector2 dir = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
            velocity = dir * speed;
            damage = dmg;
            remainingLife = lifetime;
            pierceRemaining = pierceCount;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            frameTimer = 0f;
            frameIndex = 0;
            if (animationFrames != null && animationFrames.Length > 0)
            {
                spriteRenderer.sprite = animationFrames[0];
            }

            // Pooled projectiles reuse the same ParticleSystem, so force a clean
            // restart instead of relying on playOnAwake (which won't refire on reuse).
            if (sparkle != null)
            {
                sparkle.Clear(true);
                sparkle.Play(true);
            }
        }

        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
            AdvanceAnimation();

            remainingLife -= Time.deltaTime;
            if (remainingLife <= 0f)
            {
                ObjectPool.Instance.Return(gameObject);
            }
        }

        private void AdvanceAnimation()
        {
            if (animationFrames == null || animationFrames.Length == 0) return;

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / frameRate;
            if (frameTimer < frameDuration) return;

            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % animationFrames.Length;
            spriteRenderer.sprite = animationFrames[frameIndex];
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            if (pierceRemaining > 0)
            {
                pierceRemaining--;
                return;
            }

            ObjectPool.Instance.Return(gameObject);
        }
    }
}
