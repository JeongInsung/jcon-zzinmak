using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Enemies;

namespace Zzinmak.Environment
{
    public class MissionManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private EnemySpawner outdoorSpawner;

        [Header("Building (outdoor)")]
        [SerializeField] private GameObject buildingPrefab;
        [SerializeField] private Transform mapRoomCenter;
        [SerializeField] private float mapHalfExtent = 16f;
        [SerializeField] private float minSpawnDistanceFromPlayer = 5f;

        [Header("Building interior")]
        [SerializeField] private Transform interiorRoomCenter;
        [SerializeField] private Transform interiorEntryPoint;
        [SerializeField] private Transform interiorHostageSpot;
        [SerializeField] private float interiorSpawnRadius = 9f;
        [SerializeField] private int zombieCountPerBuilding = 3;
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] private EnemyData zombieData;
        [SerializeField] private GameObject hostagePrefab;

        [Header("Building interior - boss")]
        [SerializeField] private BossOption[] bossOptions = new BossOption[0];

        [System.Serializable]
        private class BossOption
        {
            public GameObject prefab;
            public EnemyData data;
        }

        public static MissionManager Instance { get; private set; }

        private GameObject currentBuilding;
        private GameObject currentHostage;
        private readonly List<GameObject> interiorZombies = new List<GameObject>();
        private bool playerInBuilding;
        private bool interiorCleared;
        private Vector3 outdoorReturnPosition;
        private int lastBossOptionIndex = -1;
        private BossBroodmother currentBroodmother;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SpawnNewBuilding();
        }

        private void Update()
        {
            if (!playerInBuilding || interiorCleared) return;

            interiorZombies.RemoveAll(zombie => zombie == null || !zombie.activeInHierarchy);
            if (interiorZombies.Count == 0)
            {
                interiorCleared = true;
            }
        }

        public void EnterBuilding()
        {
            if (playerInBuilding) return;

            playerInBuilding = true;
            interiorCleared = false;
            outdoorReturnPosition = player.position;

            if (outdoorSpawner != null)
            {
                outdoorSpawner.enabled = false;
            }

            ClearOutdoorZombies();

            player.position = interiorEntryPoint.position;

            SpawnHostage();
            SpawnInteriorZombies();
            SpawnInteriorBoss();
        }

        public void TryRescue()
        {
            if (!playerInBuilding || !interiorCleared) return;

            ExitBuilding();
        }

        private void ExitBuilding()
        {
            playerInBuilding = false;

            if (currentBroodmother != null)
            {
                currentBroodmother.OnSummon -= HandleZombieSummoned;
                currentBroodmother = null;
            }

            if (currentHostage != null)
            {
                currentHostage.SetActive(false);
            }

            player.position = outdoorReturnPosition;

            if (outdoorSpawner != null)
            {
                outdoorSpawner.enabled = true;
            }

            if (currentBuilding != null)
            {
                Destroy(currentBuilding);
            }

            SpawnNewBuilding();
        }

        private void ClearOutdoorZombies()
        {
            // Snapshot first: returning an enemy to the pool disables it, which
            // unregisters it from EnemyTracker.Active and would mutate the list
            // out from under a direct foreach over it.
            var enemies = new List<Enemy>(EnemyTracker.Active);
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    ObjectPool.Instance.Return(enemy.gameObject);
                }
            }
        }

        private void SpawnHostage()
        {
            if (currentHostage == null)
            {
                currentHostage = Instantiate(hostagePrefab, interiorHostageSpot.position, Quaternion.identity);
            }
            else
            {
                currentHostage.transform.position = interiorHostageSpot.position;
                currentHostage.SetActive(true);
            }
        }

        private void SpawnInteriorZombies()
        {
            interiorZombies.Clear();

            for (int i = 0; i < zombieCountPerBuilding; i++)
            {
                Vector2 offset = Random.insideUnitCircle * interiorSpawnRadius;
                Vector3 spawnPos = interiorRoomCenter.position + (Vector3)offset;

                var zombieObj = ObjectPool.Instance.Get(zombiePrefab, spawnPos, Quaternion.identity);
                var enemy = zombieObj.GetComponent<Enemy>();
                enemy.Initialize(zombieData, player, 1f);
                interiorZombies.Add(zombieObj);
            }
        }

        private void SpawnInteriorBoss()
        {
            if (bossOptions == null || bossOptions.Length == 0) return;

            int index = Random.Range(0, bossOptions.Length);
            if (bossOptions.Length > 1)
            {
                while (index == lastBossOptionIndex)
                {
                    index = Random.Range(0, bossOptions.Length);
                }
            }
            lastBossOptionIndex = index;

            var option = bossOptions[index];
            if (option.prefab == null || option.data == null) return;

            var bossObj = ObjectPool.Instance.Get(option.prefab, interiorRoomCenter.position, Quaternion.identity);
            var enemy = bossObj.GetComponent<Enemy>();
            enemy.Initialize(option.data, player, 1f);
            interiorZombies.Add(bossObj);

            var broodmother = bossObj.GetComponent<BossBroodmother>();
            if (broodmother != null)
            {
                currentBroodmother = broodmother;
                currentBroodmother.OnSummon += HandleZombieSummoned;
            }
        }

        private void HandleZombieSummoned(GameObject zombieObj)
        {
            interiorZombies.Add(zombieObj);
        }

        private void SpawnNewBuilding()
        {
            Vector3 spawnPos = mapRoomCenter.position;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector3 candidate;
                if (MapBounds.Instance != null)
                {
                    Vector2 point = MapBounds.Instance.RandomPointInInterior();
                    candidate = new Vector3(point.x, point.y, mapRoomCenter.position.z);
                }
                else
                {
                    Vector2 offset = new Vector2(
                        Random.Range(-mapHalfExtent, mapHalfExtent),
                        Random.Range(-mapHalfExtent, mapHalfExtent));
                    candidate = mapRoomCenter.position + (Vector3)offset;
                }

                if (Vector3.Distance(candidate, player.position) > minSpawnDistanceFromPlayer)
                {
                    spawnPos = candidate;
                    break;
                }
            }

            // Final safety clamp - guarantees the building never lands past the walls
            // even if every retry above happened to fail the distance-from-player check.
            if (MapBounds.Instance != null)
            {
                Vector2 clamped = MapBounds.Instance.ClampToInterior(spawnPos);
                spawnPos = new Vector3(clamped.x, clamped.y, spawnPos.z);
            }

            currentBuilding = Instantiate(buildingPrefab, spawnPos, Quaternion.identity);
        }
    }
}
