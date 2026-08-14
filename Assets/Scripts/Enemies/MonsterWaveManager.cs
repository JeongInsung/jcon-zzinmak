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

        // MissionManager checks this for the wave's whole duration (blocks entering
        // buildings until every wave monster is down). EnemySpawner instead checks
        // IsBlockingRegularSpawns below, which only holds for the first
        // regularSpawnResumeDelay seconds - an unfinished wave shouldn't starve the
        // player of any zombies to fight for the rest of its run.
        public static bool IsWaveActive { get; private set; }

        private static float waveStartElapsedTime;
        private static float spawnBlockDuration;

        public static bool IsBlockingRegularSpawns
        {
            get
            {
                if (!IsWaveActive) return false;
                float elapsed = GameManager.Instance != null ? GameManager.Instance.ElapsedTime : 0f;
                return elapsed - waveStartElapsedTime < spawnBlockDuration;
            }
        }

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
        [Tooltip("웨이브 좀비를 다 못 잡아도, 웨이브 시작 후 이 시간(초)이 지나면 필드 좀비 스폰을 재개한다.")]
        [SerializeField] private float regularSpawnResumeDelay = 20f;

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
            if (IsWaveActive)
            {
                waveStartElapsedTime = GameManager.Instance != null ? GameManager.Instance.ElapsedTime : 0f;
                spawnBlockDuration = regularSpawnResumeDelay;
                OnWaveStarted?.Invoke();
            }
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
