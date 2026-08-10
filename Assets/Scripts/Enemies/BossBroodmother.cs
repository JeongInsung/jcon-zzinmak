using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;

namespace Zzinmak.Enemies
{
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Animator))]
    public class BossBroodmother : MonoBehaviour
    {
        private static readonly int WalkState = Animator.StringToHash("Walk");
        private static readonly int SummonState = Animator.StringToHash("Summon");
        private static readonly int RangedState = Animator.StringToHash("Ranged");
        private const float SummonAnimDuration = 0.8f;

        [Header("Summon")]
        [SerializeField] private GameObject summonPrefab;
        [SerializeField] private EnemyData summonData;
        [SerializeField] private float summonInterval = 8f;
        [SerializeField] private int summonCountMin = 2;
        [SerializeField] private int summonCountMax = 3;
        [SerializeField] private float summonRadius = 2.5f;
        [SerializeField] private int maxActiveSummons = 8;

        [Header("Ranged attack")]
        [SerializeField] private GameObject rangedProjectilePrefab;
        [SerializeField] private float rangedRange = 6f;
        [SerializeField] private float rangedDamage = 8f;
        [SerializeField] private float rangedCooldown = 4f;
        [SerializeField] private float rangedProjectileSpeed = 6f;
        [SerializeField] private float rangedClipDuration = 1.3f;
        [SerializeField] private float rangedThrowTime = 0.6f;
        [Tooltip("Angle in degrees between each of the 3 syringes in the fan (outer two are offset by this much from the center shot).")]
        [SerializeField] private float rangedFanAngle = 18f;

        [Header("UI")]
        [SerializeField] private BossHealthBar healthBar;

        public event System.Action<GameObject> OnSummon;

        private Enemy enemy;
        private Animator animator;
        private float summonTimer;
        private float rangedTimer;
        private bool isAttacking;
        private Coroutine summonAnimRoutine;
        private readonly List<GameObject> activeSummons = new List<GameObject>();

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            summonTimer = summonInterval;
            rangedTimer = rangedCooldown;
            isAttacking = false;
            activeSummons.Clear();
            summonAnimRoutine = null;
            if (animator != null) animator.Play(WalkState, 0, 0f);
            enemy.OnDeath += HandleDeath;
            enemy.OnHealthChanged += HandleHealthChanged;
            if (healthBar != null) healthBar.Show();
        }

        private void OnDisable()
        {
            enemy.OnDeath -= HandleDeath;
            enemy.OnHealthChanged -= HandleHealthChanged;
            OnSummon = null;
            StopAllCoroutines();
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (healthBar != null) healthBar.SetFill(current, max);
        }

        private void Update()
        {
            if (enemy.IsDead) return;

            if (!isAttacking && summonPrefab != null && summonData != null)
            {
                summonTimer -= Time.deltaTime;
                if (summonTimer <= 0f)
                {
                    summonTimer = summonInterval;
                    SummonWave();
                }
            }

            if (isAttacking || enemy.IsStunned || summonAnimRoutine != null) return;

            var target = enemy.Target;
            if (target == null || rangedProjectilePrefab == null) return;

            rangedTimer -= Time.deltaTime;
            if (rangedTimer <= 0f && Vector2.Distance(transform.position, target.position) <= rangedRange)
            {
                rangedTimer = rangedCooldown;
                StartCoroutine(RangedAttack(target));
            }
        }

        private IEnumerator RangedAttack(Transform target)
        {
            isAttacking = true;
            enemy.CanMove = false;
            enemy.FaceTarget();
            if (animator != null) animator.Play(RangedState, 0, 0f);

            yield return new WaitForSeconds(rangedThrowTime);

            if (target != null)
            {
                Vector2 baseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;

                foreach (float angle in new[] { -rangedFanAngle, 0f, rangedFanAngle })
                {
                    var projObj = ObjectPool.Instance.Get(rangedProjectilePrefab, transform.position, Quaternion.identity);
                    var projectile = projObj.GetComponent<BossProjectile>();
                    projectile.Launch(RotateDegrees(baseDir, angle), rangedProjectileSpeed, rangedDamage);
                }
            }

            yield return new WaitForSeconds(Mathf.Max(0f, rangedClipDuration - rangedThrowTime));

            enemy.CanMove = true;
            isAttacking = false;
            if (!enemy.IsDead && animator != null)
            {
                animator.Play(WalkState, 0, 0f);
            }
        }

        private static Vector2 RotateDegrees(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        private void SummonWave()
        {
            activeSummons.RemoveAll(zombie => zombie == null || !zombie.activeInHierarchy);
            if (activeSummons.Count >= maxActiveSummons) return;

            int count = Random.Range(summonCountMin, summonCountMax + 1);
            count = Mathf.Min(count, maxActiveSummons - activeSummons.Count);

            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * summonRadius;
                Vector3 spawnPos = transform.position + (Vector3)offset;

                var zombieObj = ObjectPool.Instance.Get(summonPrefab, spawnPos, Quaternion.identity);
                var zombieEnemy = zombieObj.GetComponent<Enemy>();
                zombieEnemy.Initialize(summonData, enemy.Target, 1f);

                activeSummons.Add(zombieObj);
                OnSummon?.Invoke(zombieObj);
            }

            if (animator != null)
            {
                if (summonAnimRoutine != null) StopCoroutine(summonAnimRoutine);
                summonAnimRoutine = StartCoroutine(PlaySummonAnimation());
            }
        }

        private IEnumerator PlaySummonAnimation()
        {
            animator.Play(SummonState, 0, 0f);
            yield return new WaitForSeconds(SummonAnimDuration);

            if (!enemy.IsDead)
            {
                animator.Play(WalkState, 0, 0f);
            }

            summonAnimRoutine = null;
        }

        private void HandleDeath()
        {
            if (healthBar != null) healthBar.Hide();
        }
    }
}
