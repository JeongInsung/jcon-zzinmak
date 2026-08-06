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

        // followLerpSpeed is calibrated against this frame time, so the smoothing matches the
        // old `followLerpSpeed * Time.deltaTime` lerp exactly at 60 fps and holds that same
        // response at other framerates instead of tightening/loosening with them.
        private const float ReferenceDeltaTime = 1f / 60f;

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
            float retention = Mathf.Clamp01(1f - followLerpSpeed * ReferenceDeltaTime);
            float t = 1f - Mathf.Pow(retention, Time.deltaTime / ReferenceDeltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);

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
