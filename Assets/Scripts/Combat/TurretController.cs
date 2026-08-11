using System.Collections;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Enemies;

namespace Zzinmak.Combat
{
    public class TurretController : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float attackRange = 6f;
        [SerializeField] private float fireInterval = 1f;
        [SerializeField] private float projectileSpeed = 14f;
        [SerializeField] private float burstDelay = 0.08f;
        [SerializeField] private float targetRefreshInterval = 0.1f;
        [Tooltip("Sprite shown for each turret level. Index 0 = level 1, index 1 = level 2, etc. If the level exceeds the array, the last entry is reused.")]
        [SerializeField] private Sprite[] levelSprites;

        private SpriteRenderer spriteRenderer;
        private int level = 1;
        private float damage;
        private float remainingLife;
        private float cooldown;
        private float targetRefreshTimer;
        private Transform currentTarget;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(int turretLevel, float turretDamage, float duration)
        {
            level = Mathf.Max(1, turretLevel);
            damage = turretDamage;
            remainingLife = duration;
            cooldown = 0f;
            targetRefreshTimer = 0f;
            currentTarget = null;

            ApplyLevelSprite();
        }

        private void ApplyLevelSprite()
        {
            if (spriteRenderer == null || levelSprites == null || levelSprites.Length == 0) return;

            int index = Mathf.Clamp(level - 1, 0, levelSprites.Length - 1);
            spriteRenderer.sprite = levelSprites[index];
        }

        private void Update()
        {
            remainingLife -= Time.deltaTime;
            if (remainingLife <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            targetRefreshTimer -= Time.deltaTime;
            if (targetRefreshTimer <= 0f)
            {
                targetRefreshTimer = targetRefreshInterval;
                currentTarget = EnemyTracker.GetNearest(transform.position, attackRange);
            }

            if (currentTarget != null)
            {
                FaceTarget(currentTarget.position);
            }

            cooldown -= Time.deltaTime;
            if (cooldown <= 0f && currentTarget != null)
            {
                cooldown = fireInterval;
                StartCoroutine(FireBurst());
            }
        }

        // Left/right only. The base sprite art faces down, so flipX alone (a left-right
        // mirror of a downward-facing image) can't turn it sideways - rotate +/-90 instead.
        private void FaceTarget(Vector3 targetPosition)
        {
            float dirX = targetPosition.x - transform.position.x;
            if (Mathf.Abs(dirX) < 0.01f) return;

            float angle = dirX > 0f ? 90f : -90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private IEnumerator FireBurst()
        {
            for (int i = 0; i < level; i++)
            {
                var target = EnemyTracker.GetNearest(transform.position, attackRange);
                if (target == null) yield break;

                FaceTarget(target.position);

                var projectileObj = ObjectPool.Instance.Get(projectilePrefab, transform.position, Quaternion.identity);
                var projectile = projectileObj.GetComponent<Projectile>();
                projectile.Launch(target.position, projectileSpeed, damage);

                if (i < level - 1)
                {
                    yield return new WaitForSeconds(burstDelay);
                }
            }
        }
    }
}
