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
        private const float SummonAnimDuration = 0.8f;

        [Header("Summon")]
        [SerializeField] private GameObject summonPrefab;
        [SerializeField] private EnemyData summonData;
        [SerializeField] private float summonInterval = 8f;
        [SerializeField] private int summonCountMin = 2;
        [SerializeField] private int summonCountMax = 3;
        [SerializeField] private float summonRadius = 2.5f;
        [SerializeField] private int maxActiveSummons = 8;

        [Header("UI")]
        [SerializeField] private BossHealthBar healthBar;

        public event System.Action<GameObject> OnSummon;

        private Enemy enemy;
        private Animator animator;
        private float summonTimer;
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
            if (enemy.IsDead || summonPrefab == null || summonData == null) return;

            summonTimer -= Time.deltaTime;
            if (summonTimer > 0f) return;

            summonTimer = summonInterval;
            SummonWave();
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
