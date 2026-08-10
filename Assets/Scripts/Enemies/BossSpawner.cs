using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Environment;

namespace Zzinmak.Enemies
{
    public class BossSpawner : MonoBehaviour
    {
        private const int MaxSpawnPositionAttempts = 8;
        private static readonly Collider2D[] SpawnOverlapBuffer = new Collider2D[8];

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

            Vector3 spawnPos = FindSpawnPosition(bossPrefab);

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

        // Retries a few times to avoid landing inside a building's "fw" collider - spawning
        // there would leave the boss wedged in place, unable to move on any axis. Checked
        // with the same collider-radius overlap test Enemy.ResolveMovement uses (not just a
        // point), so a position that passes here doesn't immediately block the boss's own
        // body the instant it spawns.
        private Vector3 FindSpawnPosition(GameObject prefab)
        {
            float checkRadius = GetObstacleCheckRadius(prefab);
            Vector2 candidate = player.position;
            for (int attempt = 0; attempt < MaxSpawnPositionAttempts; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
                candidate = (Vector2)player.position + offset;

                if (!BuildingCollision.OverlapsBuilding(candidate, checkRadius, SpawnOverlapBuffer)) break;
            }

            return new Vector3(candidate.x, candidate.y, player.position.z);
        }

        private static float GetObstacleCheckRadius(GameObject prefab)
        {
            var collider = prefab.GetComponent<CircleCollider2D>();
            return collider != null ? collider.radius * prefab.transform.lossyScale.x : 0.3f;
        }
    }
}
