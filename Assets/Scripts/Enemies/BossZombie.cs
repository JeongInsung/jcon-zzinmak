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
        private static readonly int IdleState = Animator.StringToHash("Idle");
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
        [SerializeField] private int spitCount = 3;
        [Tooltip("Total angle (degrees) the spitCount projectiles fan out across, centered on the target direction.")]
        [SerializeField] private float spitSpreadAngle = 20f;

        [Header("UI")]
        [SerializeField] private BossHealthBar healthBar;

        private Enemy enemy;
        private Animator animator;
        private bool isAttacking;
        private bool isStandingGround;
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
            isStandingGround = false;
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
            if (isAttacking || enemy.IsDead || enemy.IsStunned) return;

            var target = enemy.Target;
            if (target == null) return;

            float distance = Vector2.Distance(transform.position, target.position);

            // Stands its ground once the player is within ranged range - only closes
            // in from outside it, rather than always walking up into melee.
            bool standGround = distance <= rangedRange;
            enemy.CanMove = !standGround;

            if (standGround != isStandingGround)
            {
                isStandingGround = standGround;
                animator.Play(standGround ? IdleState : WalkState, 0, 0f);
            }

            meleeTimer -= Time.deltaTime;
            rangedTimer -= Time.deltaTime;

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
                Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
                FireSpitVolley(dir);
            }

            yield return new WaitForSeconds(Mathf.Max(0f, rangedClipDuration - rangedSpitTime));

            EndAttack();
        }

        // Fans spitCount projectiles evenly across spitSpreadAngle, centered on baseDir -
        // e.g. count 3 fires one straight at the target plus one on either side.
        private void FireSpitVolley(Vector2 baseDir)
        {
            float halfSpread = spitSpreadAngle * 0.5f;

            for (int i = 0; i < spitCount; i++)
            {
                float t = spitCount > 1 ? (float)i / (spitCount - 1) : 0.5f;
                float angle = Mathf.Lerp(-halfSpread, halfSpread, t);
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDir;

                var projObj = ObjectPool.Instance.Get(spitProjectilePrefab, transform.position, Quaternion.identity);
                var projectile = projObj.GetComponent<BossProjectile>();
                projectile.Launch(dir, spitSpeed, rangedDamage * damageMultiplier);
            }
        }

        private void EndAttack()
        {
            isAttacking = false;
            if (enemy.IsDead) return;

            var target = enemy.Target;
            isStandingGround = target != null && Vector2.Distance(transform.position, target.position) <= rangedRange;
            enemy.CanMove = !isStandingGround;
            animator.Play(isStandingGround ? IdleState : WalkState, 0, 0f);
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
