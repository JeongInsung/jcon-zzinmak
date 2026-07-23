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
        [SerializeField] private int zombieCountPerBuilding = 5;
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] private EnemyData zombieData;
        [SerializeField] private GameObject hostagePrefab;

        public static MissionManager Instance { get; private set; }

        private GameObject currentBuilding;
        private GameObject currentHostage;
        private readonly List<GameObject> interiorZombies = new List<GameObject>();
        private bool playerInBuilding;
        private bool interiorCleared;
        private Vector3 outdoorReturnPosition;

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
        }

        public void TryRescue()
        {
            if (!playerInBuilding || !interiorCleared) return;

            ExitBuilding();
        }

        private void ExitBuilding()
        {
            playerInBuilding = false;

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
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                if (enemy.activeInHierarchy)
                {
                    ObjectPool.Instance.Return(enemy);
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

        private void SpawnNewBuilding()
        {
            Vector3 spawnPos = mapRoomCenter.position;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 offset = new Vector2(
                    Random.Range(-mapHalfExtent, mapHalfExtent),
                    Random.Range(-mapHalfExtent, mapHalfExtent));
                Vector3 candidate = mapRoomCenter.position + (Vector3)offset;

                if (Vector3.Distance(candidate, player.position) > minSpawnDistanceFromPlayer)
                {
                    spawnPos = candidate;
                    break;
                }
            }

            currentBuilding = Instantiate(buildingPrefab, spawnPos, Quaternion.identity);
        }
    }
}
