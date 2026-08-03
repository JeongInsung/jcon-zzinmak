using System.Collections;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Player;

namespace Zzinmak.Enemies
{
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Animator))]
    public class BossZombie : MonoBehaviour
    {
        private static readonly int WalkState = Animator.StringToHash("Walk");
        private static readonly int Attack1State = Animator.StringToHash("Attack1");
        private static readonly int Attack2State = Animator.StringToHash("Attack2");
        private static readonly int DieState = Animator.StringToHash("Die");

        [Header("Melee attack (boss attack 1)")]
        [SerializeField] private float meleeRange = 1.5f;
        [SerializeField] private float meleeDamage = 20f;
        [SerializeField] private float meleeCooldown = 2.5f;
        [SerializeField] private float meleeClipDuration = 1f;
        [SerializeField] private float meleeHitTime = 0.4f;

        [Header("Ranged attack (boss attack 2 - spit)")]
        [SerializeField] private float rangedRange = 7f;
        [SerializeField] private float rangedMinRange = 2f;
        [SerializeField] private float rangedDamage = 12f;
        [SerializeField] private float rangedCooldown = 3.5f;
        [SerializeField] private float rangedClipDuration = 1f;
        [SerializeField] private float rangedSpitTime = 0.5f;
        [SerializeField] private GameObject spitProjectilePrefab;
        [SerializeField] private float spitSpeed = 6f;

        [Header("UI")]
        [SerializeField] private BossHealthBar healthBar;

        private Enemy enemy;
        private Animator animator;
        private bool isAttacking;
        private float meleeTimer;
        private float rangedTimer;
        private float damageMultiplier = 1f;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            animator = GetComponent<Animator>();
        }

        public void ApplyDamageMultiplier(float multiplier)
        {
            damageMultiplier = Mathf.Max(0.01f, multiplier);
        }

        private void OnEnable()
        {
            isAttacking = false;
            meleeTimer = 0f;
            rangedTimer = 0f;
            damageMultiplier = 1f;
            enemy.OnDeath += HandleDeath;
            enemy.OnHealthChanged += HandleHealthChanged;
            if (healthBar != null) healthBar.Show();
            animator.Play(WalkState, 0, 0f);
        }

        private void OnDisable()
        {
            enemy.OnDeath -= HandleDeath;
            enemy.OnHealthChanged -= HandleHealthChanged;
            StopAllCoroutines();
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (healthBar != null) healthBar.SetFill(current, max);
        }

        private void Update()
        {
            if (isAttacking || enemy.IsDead) return;

            var target = enemy.Target;
            if (target == null) return;

            meleeTimer -= Time.deltaTime;
            rangedTimer -= Time.deltaTime;

            float distance = Vector2.Distance(transform.position, target.position);

            if (distance <= meleeRange && meleeTimer <= 0f)
            {
                meleeTimer = meleeCooldown;
                StartCoroutine(MeleeAttack(target));
            }
            else if (distance <= rangedRange && distance > rangedMinRange && rangedTimer <= 0f)
            {
                rangedTimer = rangedCooldown;
                StartCoroutine(RangedAttack(target));
            }
        }

        private IEnumerator MeleeAttack(Transform target)
        {
            isAttacking = true;
            enemy.CanMove = false;
            enemy.FaceTarget();
            animator.Play(Attack1State, 0, 0f);

            yield return new WaitForSeconds(meleeHitTime);

            if (target != null && Vector2.Distance(transform.position, target.position) <= meleeRange + 0.3f)
            {
                var health = target.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(meleeDamage * damageMultiplier);
                }
            }

            yield return new WaitForSeconds(Mathf.Max(0f, meleeClipDuration - meleeHitTime));

            EndAttack();
        }

        private IEnumerator RangedAttack(Transform target)
        {
            isAttacking = true;
            enemy.CanMove = false;
            enemy.FaceTarget();
            animator.Play(Attack2State, 0, 0f);

            yield return new WaitForSeconds(rangedSpitTime);

            if (target != null && spitProjectilePrefab != null)
            {
                var projObj = ObjectPool.Instance.Get(spitProjectilePrefab, transform.position, Quaternion.identity);
                var projectile = projObj.GetComponent<BossProjectile>();
                projectile.Launch(target.position, spitSpeed, rangedDamage * damageMultiplier);
            }

            yield return new WaitForSeconds(Mathf.Max(0f, rangedClipDuration - rangedSpitTime));

            EndAttack();
        }

        private void EndAttack()
        {
            enemy.CanMove = true;
            isAttacking = false;
            if (!enemy.IsDead)
            {
                animator.Play(WalkState, 0, 0f);
            }
        }

        private void HandleDeath()
        {
            StopAllCoroutines();
            isAttacking = true;
            enemy.CanMove = false;
            if (healthBar != null) healthBar.Hide();
            animator.Play(DieState, 0, 0f);
        }
    }
}
