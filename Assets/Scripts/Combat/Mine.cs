using System.Collections;
using UnityEngine;
using Zzinmak.Enemies;

namespace Zzinmak.Combat
{
    public class Mine : MonoBehaviour
    {
        [Tooltip("Idle frame first, followed by the explosion frames in playback order.")]
        [SerializeField] private Sprite[] explosionFrames;
        [SerializeField] private float frameDuration = 0.07f;
        [SerializeField] private float explosionRadius = 1.5f;
        [Tooltip("Index into explosionFrames where damage is applied (the actual blast frame), instead of the instant the mine is triggered.")]
        [SerializeField] private int damageFrameIndex = 11;
        [Tooltip("Extra blast VFX, layered on top of the mine's own frame animation.")]
        [SerializeField] private GameObject explosionEffectPrefab;
        [Tooltip("Index into explosionFrames where the extra blast VFX is spawned. Kept earlier than damageFrameIndex so the VFX has time to build up before the actual blast frame.")]
        [SerializeField] private int effectFrameIndex = 8;

        private SpriteRenderer spriteRenderer;
        private Collider2D triggerCollider;
        private float damage;
        private bool triggered;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            triggerCollider = GetComponent<Collider2D>();
        }

        public void Initialize(float mineDamage)
        {
            damage = mineDamage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;
            if (!other.CompareTag("Enemy")) return;

            Explode();
        }

        private void Explode()
        {
            triggered = true;

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            StartCoroutine(PlayExplosionAnimation());
        }

        private void ApplyDamage()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (var hit in hits)
            {
                if (!hit.CompareTag("Enemy")) continue;

                var enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        private IEnumerator PlayExplosionAnimation()
        {
            bool damageApplied = false;

            if (spriteRenderer != null && explosionFrames != null)
            {
                for (int i = 0; i < explosionFrames.Length; i++)
                {
                    var frame = explosionFrames[i];
                    if (frame == null) continue;

                    spriteRenderer.sprite = frame;

                    if (i == effectFrameIndex)
                    {
                        SpawnExplosionEffect();
                    }

                    if (i == damageFrameIndex)
                    {
                        ApplyDamage();
                        damageApplied = true;
                    }

                    yield return new WaitForSeconds(frameDuration);
                }
            }

            if (!damageApplied)
            {
                ApplyDamage();
                SpawnExplosionEffect();
            }

            Destroy(gameObject);
        }

        private void SpawnExplosionEffect()
        {
            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
