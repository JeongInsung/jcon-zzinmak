using System.Collections;
using UnityEngine;
using Zzinmak.Core;

namespace Zzinmak.Enemies
{
    [RequireComponent(typeof(Enemy))]
    public class AcidZombie : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float range = 5f;
        [SerializeField] private float damage = 6f;
        [SerializeField] private float cooldown = 3f;
        [SerializeField] private float projectileSpeed = 5f;
        [Tooltip("Delay before the projectile actually spawns, so movement/facing settle first.")]
        [SerializeField] private float throwDelay = 0.3f;

        private Enemy enemy;
        private float timer;
        private bool isAttacking;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
        }

        private void OnEnable()
        {
            timer = cooldown;
            isAttacking = false;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Update()
        {
            if (isAttacking || enemy.IsDead || enemy.IsStunned || projectilePrefab == null) return;

            var target = enemy.Target;
            if (target == null) return;

            timer -= Time.deltaTime;
            if (timer <= 0f && Vector2.Distance(transform.position, target.position) <= range)
            {
                timer = cooldown;
                StartCoroutine(Attack(target));
            }
        }

        private IEnumerator Attack(Transform target)
        {
            isAttacking = true;
            enemy.CanMove = false;
            enemy.FaceTarget();

            yield return new WaitForSeconds(throwDelay);

            if (target != null)
            {
                Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
                var projObj = ObjectPool.Instance.Get(projectilePrefab, transform.position, Quaternion.identity);
                var projectile = projObj.GetComponent<BossProjectile>();
                projectile.Launch(dir, projectileSpeed, damage);
            }

            enemy.CanMove = true;
            isAttacking = false;
        }
    }
}
