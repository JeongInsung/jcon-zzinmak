using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Player;

namespace Zzinmak.Combat
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private Transform muzzle;
        [SerializeField] private List<WeaponData> startingWeapons = new List<WeaponData>();

        private readonly List<WeaponRuntime> activeWeapons = new List<WeaponRuntime>();
        private PlayerStats stats;

        public Transform CurrentTarget { get; private set; }

        private class WeaponRuntime
        {
            public WeaponData data;
            public float cooldown;
        }

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();

            foreach (var weaponData in startingWeapons)
            {
                activeWeapons.Add(new WeaponRuntime { data = weaponData, cooldown = 0f });
            }
        }

        private void Update()
        {
            float searchRange = activeWeapons.Count > 0 ? activeWeapons[0].data.range : 0f;
            CurrentTarget = FindNearestEnemy(searchRange);

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

            for (int i = 0; i < data.projectileCount; i++)
            {
                var projectileObj = ObjectPool.Instance.Get(data.projectilePrefab, origin, Quaternion.identity);
                var projectile = projectileObj.GetComponent<Projectile>();
                projectile.Launch(target.position, data.projectileSpeed, data.damage * stats.damageMultiplier);
            }
        }

        private Transform FindNearestEnemy(float maxRange)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform nearest = null;
            float nearestDistSqr = maxRange * maxRange;

            foreach (var enemy in enemies)
            {
                if (!enemy.activeInHierarchy) continue;

                float distSqr = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distSqr <= nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearest = enemy.transform;
                }
            }

            return nearest;
        }
    }
}
