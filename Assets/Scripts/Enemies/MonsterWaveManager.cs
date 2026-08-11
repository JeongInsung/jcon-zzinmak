using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Environment;

namespace Zzinmak.Enemies
{
    // Recurring "monster wave" event: every waveLevelInterval levels starting at
    // firstWaveLevel, spawns a burst of regular zombies (composition/count from `tiers`)
    // plus one random boss. Mirrors BossSpawner's OnLevelChanged-driven trigger pattern.
    public class MonsterWaveManager : MonoBehaviour
    {
        private const int MaxSpawnPositionAttempts = 8;
        private static readonly Collider2D[] SpawnOverlapBuffer = new Collider2D[8];

        // EnemySpawner checks this to pause its own trickle-spawning while a wave is
        // out, so the wave's monsters don't get lost in a flood of regular spawns.
        public static bool IsWaveActive { get; private set; }

        // Fired once per wave, right when it starts. UI alerts (siren/screen flash) hook
        // this instead of polling IsWaveActive, since they should only play briefly at
        // the start rather than for the wave's whole (much longer) duration.
        public static event System.Action OnWaveStarted;

        private readonly List<GameObject> activeWaveEnemies = new List<GameObject>();

        [System.Serializable]
        public class WaveVariant
        {
            public GameObject prefab;
            public EnemyData data;
        }

        [System.Serializable]
        public class WaveTier
        {
            public int monsterCount;
            [Tooltip("Enemy types this tier's monsters are randomly drawn from.")]
            public WaveVariant[] variants;
        }

        [SerializeField] private Transform player;
        [SerializeField] private int firstWaveLevel = 10;
        [SerializeField] private int waveLevelInterval = 5;
        [Tooltip("One entry per wave in order (index 0 = firstWaveLevel, index 1 = firstWaveLevel + interval, etc). Once the level goes past the last configured entry, composition stays the same as the last one but the monster count keeps growing.")]
        [SerializeField] private WaveTier[] tiers;
        [SerializeField] private int countIncreasePerTierAfterTable = 5;
        [SerializeField] private WaveVariant[] bossVariants;
        [SerializeField] private float spawnRadius = 9f;

        private void OnEnable()
        {
            GameEvents.OnLevelChanged += HandleLevelChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelChanged -= HandleLevelChanged;
        }

        private void Update()
        {
            if (!IsWaveActive) return;

            activeWaveEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);
            if (activeWaveEnemies.Count == 0)
            {
                IsWaveActive = false;
            }
        }

        private void HandleLevelChanged(int level)
        {
            if (player == null || tiers == null || tiers.Length == 0) return;
            if (level < firstWaveLevel) return;
            if ((level - firstWaveLevel) % waveLevelInterval != 0) return;

            SpawnWave((level - firstWaveLevel) / waveLevelInterval);
        }

        private void SpawnWave(int tierIndex)
        {
            WaveTier tier = tiers[Mathf.Min(tierIndex, tiers.Length - 1)];
            if (tier.variants == null || tier.variants.Length == 0) return;

            int count = tier.monsterCount;
            int overflowTiers = tierIndex - (tiers.Length - 1);
            if (overflowTiers > 0)
            {
                count += overflowTiers * countIncreasePerTierAfterTable;
            }

            activeWaveEnemies.Clear();

            for (int i = 0; i < count; i++)
            {
                var variant = tier.variants[Random.Range(0, tier.variants.Length)];
                if (variant.prefab == null || variant.data == null) continue;

                activeWaveEnemies.Add(SpawnEnemy(variant.prefab, variant.data));
            }

            if (bossVariants != null && bossVariants.Length > 0)
            {
                var boss = bossVariants[Random.Range(0, bossVariants.Length)];
                if (boss.prefab != null && boss.data != null)
                {
                    activeWaveEnemies.Add(SpawnEnemy(boss.prefab, boss.data));
                }
            }

            IsWaveActive = activeWaveEnemies.Count > 0;
            if (IsWaveActive) OnWaveStarted?.Invoke();
        }

        private GameObject SpawnEnemy(GameObject prefab, EnemyData data)
        {
            Vector3 spawnPos = FindSpawnPosition(prefab);
            var obj = ObjectPool.Instance.Get(prefab, spawnPos, Quaternion.identity);
            var enemy = obj.GetComponent<Enemy>();
            enemy.Initialize(data, player, 1f);
            return obj;
        }

        // Same obstacle-avoidance approach as EnemySpawner/BossSpawner: retry a few times
        // to avoid landing inside a building/decoration collider.
        private Vector3 FindSpawnPosition(GameObject prefab)
        {
            float checkRadius = GetObstacleCheckRadius(prefab);
            Vector2 candidate = player.position;
            for (int attempt = 0; attempt < MaxSpawnPositionAttempts; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
                Vector2 rawPos = (Vector2)player.position + offset;
                candidate = MapBounds.Instance != null ? MapBounds.Instance.ClampToInterior(rawPos) : rawPos;

                if (!BuildingCollision.OverlapsSolidObstacle(candidate, checkRadius, SpawnOverlapBuffer)) break;
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
