using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;

namespace Zzinmak.Enemies
{
    public class BossSpawner : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private EnemyData bossData;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private int levelInterval = 10;
        [SerializeField] private float spawnRadius = 9f;
        [SerializeField] private float healthGrowthPerTier = 0.6f;
        [SerializeField] private float damageGrowthPerTier = 0.3f;
        [SerializeField] private int highHealthLevelThreshold = 15;
        [SerializeField] private float highHealthMultiplier = 2f;

        private int lastSpawnedTier;

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
            if (levelInterval <= 0 || level % levelInterval != 0) return;

            int tier = level / levelInterval;
            if (tier <= lastSpawnedTier) return;

            lastSpawnedTier = tier;
            SpawnBoss(tier, level);
        }

        private void SpawnBoss(int tier, int level)
        {
            if (player == null || bossPrefab == null || bossData == null) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = player.position + (Vector3)offset;

            float healthMultiplier = 1f + (tier - 1) * healthGrowthPerTier;
            if (level > highHealthLevelThreshold) healthMultiplier *= highHealthMultiplier;

            float damageMultiplier = 1f + (tier - 1) * damageGrowthPerTier;

            var bossObj = ObjectPool.Instance.Get(bossPrefab, spawnPos, Quaternion.identity);
            var enemy = bossObj.GetComponent<Enemy>();
            enemy.Initialize(bossData, player, healthMultiplier);

            var boss = bossObj.GetComponent<BossZombie>();
            if (boss != null)
            {
                boss.ApplyDamageMultiplier(damageMultiplier);
            }
        }
    }
}
