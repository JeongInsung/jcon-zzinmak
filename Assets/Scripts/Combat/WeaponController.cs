using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Enemies;
using Zzinmak.Player;

namespace Zzinmak.Combat
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private Transform muzzle;
        [SerializeField] private List<WeaponData> startingWeapons = new List<WeaponData>();
        [SerializeField] private float projectileBurstDelay = 0.08f;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private AudioClip gunshotClip;
        [SerializeField] private float targetRefreshInterval = 0.1f;
        [SerializeField] private float fanAngleStep = 15f;

        private readonly List<WeaponRuntime> activeWeapons = new List<WeaponRuntime>();
        private PlayerStats stats;
        private SpriteRenderer spriteRenderer;
        private float targetRefreshTimer;

        public Transform CurrentTarget { get; private set; }

        private class WeaponRuntime
        {
            public WeaponData data;
            public float cooldown;
        }

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            foreach (var weaponData in startingWeapons)
            {
                activeWeapons.Add(new WeaponRuntime { data = weaponData, cooldown = 0f });
            }
        }

        private void Update()
        {
            targetRefreshTimer -= Time.deltaTime;
            if (targetRefreshTimer <= 0f)
            {
                targetRefreshTimer = targetRefreshInterval;
                float searchRange = activeWeapons.Count > 0 ? activeWeapons[0].data.range : 0f;
                CurrentTarget = FindNearestEnemy(searchRange);
            }

            foreach (var weapon in activeWeapons)
            {
                weapon.cooldown -= Time.deltaTime * stats.attackSpeedMultiplier;
                if (weapon.cooldown <= 0f)
                {
                    weapon.cooldown = weapon.data.fireInterval;
                    Fire(weapon.data);
                }
            }
        }

        private void Fire(WeaponData data)
        {
            var target = FindNearestEnemy(data.range);
            if (target == null) return;

            Vector3 origin = muzzle != null ? muzzle.position : transform.position;
            int totalProjectiles = data.projectileCount + stats.extraProjectiles;
            Vector2 dir = ((Vector2)target.position - (Vector2)origin).normalized;

            foreach (float angle in FanAngles(stats.fanShotCount))
            {
                Vector2 fanDir = Quaternion.Euler(0f, 0f, angle) * dir;
                Vector3 aimPoint = origin + (Vector3)(fanDir * data.range);
                StartCoroutine(FireBurst(data, aimPoint, totalProjectiles));
            }
        }

        // Center shot plus one extra direction per fan level, alternating sides
        // so the spread widens symmetrically into a fan/wedge shape as levels are gained.
        private IEnumerable<float> FanAngles(int fanShotCount)
        {
            yield return 0f;

            for (int i = 1; i <= fanShotCount; i++)
            {
                float angle = Mathf.CeilToInt(i / 2f) * fanAngleStep;
                yield return i % 2 == 1 ? angle : -angle;
            }
        }

        private IEnumerator FireBurst(WeaponData data, Vector3 aimPoint, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 origin = muzzle != null ? muzzle.position : transform.position;

                float damage = data.damage * stats.damageMultiplier;
                if (Random.value < stats.critChance)
                {
                    damage *= stats.critMultiplier;
                }

                var projectileObj = ObjectPool.Instance.Get(data.projectilePrefab, origin, Quaternion.identity);
                var projectile = projectileObj.GetComponent<Projectile>();
                projectile.Launch(aimPoint, data.projectileSpeed, damage, stats.pierceCount);

                SpawnMuzzleFlash(origin);
                if (gunshotClip != null) AudioSource.PlayClipAtPoint(gunshotClip, origin);

                if (i < count - 1)
                {
                    yield return new WaitForSeconds(projectileBurstDelay);
                }
            }
        }

        private void SpawnMuzzleFlash(Vector3 origin)
        {
            if (muzzleFlashPrefab == null) return;

            // The gun sprite itself never rotates to aim (only flips left/right), so
            // the flash should always sit flush in front of the muzzle facing the same
            // way - not rotated toward the actual (possibly up/down) shot trajectory.
            bool facingLeft = spriteRenderer != null && spriteRenderer.flipX;
            Quaternion rotation = facingLeft ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;

            var flashObj = ObjectPool.Instance.Get(muzzleFlashPrefab, origin, rotation);
            if (muzzle != null)
            {
                // Parented so the flash keeps tracking the muzzle (position and the
                // left/right mirroring PlayerController applies to it) for its short lifetime.
                flashObj.transform.SetParent(muzzle, true);
            }
        }

        private Transform FindNearestEnemy(float maxRange)
        {
            return EnemyTracker.GetNearest(transform.position, maxRange, requireLineOfSight: true);
        }

        public float GetCurrentDamage()
        {
            return activeWeapons.Count > 0 ? activeWeapons[0].data.damage * stats.damageMultiplier : 0f;
        }
    }
}
