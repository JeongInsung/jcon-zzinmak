using System.Collections;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Enemies;

namespace Zzinmak.Combat
{
    public class DroneController : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float followLerpSpeed = 6f;
        [SerializeField] private float attackRange = 6f;
        [SerializeField] private float fireInterval = 1f;
        [SerializeField] private float projectileSpeed = 14f;
        [SerializeField] private float burstDelay = 0.08f;

        private TacticalDrone owner;
        private Transform followTarget;
        private Vector2 hoverOffset;
        private float cooldown;

        public void Initialize(TacticalDrone droneOwner, Transform target, Vector2 offset)
        {
            owner = droneOwner;
            followTarget = target;
            hoverOffset = offset;
        }

        private void Update()
        {
            if (followTarget == null) return;

            Vector3 desiredPosition = followTarget.position + (Vector3)hoverOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followLerpSpeed * Time.deltaTime);

            cooldown -= Time.deltaTime;
            if (cooldown <= 0f)
            {
                TryFire();
            }
        }

        private void TryFire()
        {
            var target = EnemyTracker.GetNearest(transform.position, attackRange);
            if (target == null) return;

            cooldown = fireInterval;
            StartCoroutine(FireBurst(target.position));
        }

        private IEnumerator FireBurst(Vector3 aimPoint)
        {
            int count = 1 + (owner != null ? owner.GetBonusProjectiles() : 0);
            for (int i = 0; i < count; i++)
            {
                var projectileObj = ObjectPool.Instance.Get(projectilePrefab, transform.position, Quaternion.identity);
                var projectile = projectileObj.GetComponent<Projectile>();
                projectile.Launch(aimPoint, projectileSpeed, owner.GetDamage());

                if (i < count - 1)
                {
                    yield return new WaitForSeconds(burstDelay);
                }
            }
        }
    }
}
