using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Environment;

namespace Zzinmak.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        private const int MaxSpawnPositionAttempts = 8;

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

        [Header("Level scaling")]
        [SerializeField] private int baseSpawnCount = 1;
        [SerializeField] private int levelsPerSpawnCountIncrease = 5;
        [SerializeField] private int spawnCountIncreasePerStep = 1;
        [SerializeField] private int maxSpawnCountPerTick = 8;
        [SerializeField] private int baseMaxAliveEnemies = 30;
        [SerializeField] private int levelsPerMaxAliveIncrease = 10;
        [SerializeField] private int maxAliveIncreasePerStep = 10;

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
            int count = CurrentSpawnCount();
            int maxAlive = CurrentMaxAliveEnemies();
            for (int i = 0; i < count; i++)
            {
                if (EnemyTracker.Active.Count >= maxAlive) return;
                SpawnSingleEnemy();
            }
        }

        private int CurrentSpawnCount()
        {
            int tier = (currentPlayerLevel - 1) / Mathf.Max(1, levelsPerSpawnCountIncrease);
            int count = baseSpawnCount + tier * spawnCountIncreasePerStep;
            return Mathf.Clamp(count, 1, maxSpawnCountPerTick);
        }

        private int CurrentMaxAliveEnemies()
        {
            int tier = (currentPlayerLevel - 1) / Mathf.Max(1, levelsPerMaxAliveIncrease);
            return baseMaxAliveEnemies + tier * maxAliveIncreasePerStep;
        }

        private void SpawnSingleEnemy()
        {
            if (player == null) return;

            var variant = PickVariant();
            if (variant == null || variant.prefab == null || variant.data == null) return;

            Vector3 spawnPos = FindSpawnPosition();

            var enemyObj = ObjectPool.Instance.Get(variant.prefab, spawnPos, Quaternion.identity);
            var enemy = enemyObj.GetComponent<Enemy>();
            enemy.Initialize(variant.data, player, CurrentHealthMultiplier());
        }

        // Retries a few times to avoid landing inside a building's "fw" collider - spawning
        // there would leave the enemy wedged in place, unable to move on any axis.
        private Vector3 FindSpawnPosition()
        {
            Vector2 candidate = player.position;
            for (int attempt = 0; attempt < MaxSpawnPositionAttempts; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
                Vector2 rawPos = (Vector2)player.position + offset;
                candidate = MapBounds.Instance != null ? MapBounds.Instance.ClampToInterior(rawPos) : rawPos;

                if (!BuildingCollision.OverlapsBuilding(candidate)) break;
            }

            return new Vector3(candidate.x, candidate.y, player.position.z);
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
