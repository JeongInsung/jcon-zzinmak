using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;

namespace Zzinmak.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        public class EnemyVariant
        {
            public EnemyData data;
            public GameObject prefab;
            public float spawnWeight = 1f;
            public int minPlayerLevel = 1;
        }

        [SerializeField] private Transform player;
        [SerializeField] private EnemyVariant[] enemyVariants;
        [SerializeField] private float spawnRadius = 9f;
        [SerializeField] private float initialSpawnInterval = 1.5f;
        [SerializeField] private float minSpawnInterval = 0.2f;
        [SerializeField] private float intervalDecreasePerMinute = 0.15f;
        [SerializeField] private float healthGrowthPerMinute = 0.15f;

        private float spawnTimer;
        private int currentPlayerLevel = 1;

        private void OnEnable()
        {
            GameEvents.OnLevelChanged += HandleLevelChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelChanged -= HandleLevelChanged;
        }

        private void HandleLevelChanged(int level)
        {
            currentPlayerLevel = level;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = CurrentSpawnInterval();
                SpawnEnemy();
            }
        }

        private float CurrentSpawnInterval()
        {
            float minutes = GameManager.Instance != null ? GameManager.Instance.ElapsedTime / 60f : 0f;
            float interval = initialSpawnInterval - minutes * intervalDecreasePerMinute;
            return Mathf.Max(interval, minSpawnInterval);
        }

        private float CurrentHealthMultiplier()
        {
            float minutes = GameManager.Instance != null ? GameManager.Instance.ElapsedTime / 60f : 0f;
            return 1f + minutes * healthGrowthPerMinute;
        }

        private void SpawnEnemy()
        {
            if (player == null) return;

            var variant = PickVariant();
            if (variant == null || variant.prefab == null || variant.data == null) return;

            Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = player.position + (Vector3)offset;

            var enemyObj = ObjectPool.Instance.Get(variant.prefab, spawnPos, Quaternion.identity);
            var enemy = enemyObj.GetComponent<Enemy>();
            enemy.Initialize(variant.data, player, CurrentHealthMultiplier());
        }

        private EnemyVariant PickVariant()
        {
            if (enemyVariants == null || enemyVariants.Length == 0) return null;

            float totalWeight = 0f;
            EnemyVariant lastEligible = null;
            foreach (var variant in enemyVariants)
            {
                if (currentPlayerLevel < variant.minPlayerLevel) continue;

                totalWeight += variant.spawnWeight;
                lastEligible = variant;
            }

            if (totalWeight <= 0f) return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (var variant in enemyVariants)
            {
                if (currentPlayerLevel < variant.minPlayerLevel) continue;

                cumulative += variant.spawnWeight;
                if (roll <= cumulative)
                {
                    return variant;
                }
            }

            return lastEligible;
        }
    }
}
