using System.Collections;
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
        [SerializeField] private Transform portalSpawner;
        [SerializeField] private float minSpawnDistanceFromPlayer = 5f;
        [Tooltip("Delay after clearing a building (exiting via the boss bag) before a new one spawns. The very first building at game start is not delayed.")]
        [SerializeField] private float newBuildingSpawnDelay = 30f;

        [Header("Building interior")]
        [SerializeField] private Transform interiorRoomCenter;
        [SerializeField] private Transform interiorEntryPoint;
        [SerializeField] private float interiorSpawnRadius = 9f;
        [SerializeField] private MapBounds outdoorBounds;
        [SerializeField] private MapBounds indoorBounds;
        [SerializeField] private int zombieCountPerBuilding = 3;
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] private EnemyData zombieData;

        [Header("Building interior - boss")]
        [SerializeField] private BossOption[] bossOptions = new BossOption[0];
        [Tooltip("Dropped by the interior boss on death. Approaching it opens the item selection screen; picking an item exits the building.")]
        [SerializeField] private GameObject bossBagPrefab;

        [System.Serializable]
        private class BossOption
        {
            public GameObject prefab;
            public EnemyData data;
        }

        public static MissionManager Instance { get; private set; }

        public Transform CurrentBuilding => currentBuilding != null ? currentBuilding.transform : null;
        public bool PlayerInBuilding => playerInBuilding;

        private GameObject currentBuilding;
        private Enemy currentInteriorBoss;
        private readonly List<GameObject> interiorZombies = new List<GameObject>();
        private bool playerInBuilding;
        private bool interiorCleared;
        private Vector3 outdoorReturnPosition;
        private int lastBossOptionIndex = -1;
        private BossBroodmother currentBroodmother;
        private Rigidbody2D playerBody;

        private void Awake()
        {
            Instance = this;
            if (player != null) playerBody = player.GetComponent<Rigidbody2D>();
        }

        // The player's Rigidbody2D uses Interpolate. Writing Transform.position directly on an
        // interpolated body makes the interpolation pass smear the character across the map over
        // one fixed step instead of cutting instantly. Move the body itself and bounce the
        // interpolation mode so its interpolation history restarts at the new pose.
        private void TeleportPlayer(Vector3 destination)
        {
            if (playerBody == null)
            {
                player.position = destination;
                return;
            }

            var previous = playerBody.interpolation;
            playerBody.interpolation = RigidbodyInterpolation2D.None;
            playerBody.linearVelocity = Vector2.zero;
            playerBody.position = destination;
            player.position = destination;
            playerBody.interpolation = previous;
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
            if (playerInBuilding || MonsterWaveManager.IsWaveActive) return;

            playerInBuilding = true;
            interiorCleared = false;
            outdoorReturnPosition = player.position;

            if (outdoorSpawner != null)
            {
                outdoorSpawner.enabled = false;
            }

            ClearOutdoorZombies();

            TeleportPlayer(interiorEntryPoint.position);

            if (indoorBounds != null)
            {
                indoorBounds.MakeActive();
            }

            SpawnInteriorZombies();
            SpawnInteriorBoss();

            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayDungeon();
            }
        }

        // Called once the player picks an item from the boss's dropped bag - that's the
        // only way out of the building now that the hostage/rescue flow is gone.
        public void ExitBuilding()
        {
            if (!playerInBuilding) return;

            playerInBuilding = false;

            if (currentBroodmother != null)
            {
                currentBroodmother.OnSummon -= HandleZombieSummoned;
                currentBroodmother = null;
            }

            if (currentInteriorBoss != null)
            {
                currentInteriorBoss.OnDeath -= HandleInteriorBossDeath;
                currentInteriorBoss = null;
            }

            TeleportPlayer(outdoorReturnPosition);

            if (outdoorBounds != null)
            {
                outdoorBounds.MakeActive();
            }

            if (outdoorSpawner != null)
            {
                outdoorSpawner.enabled = true;
            }

            if (currentBuilding != null)
            {
                Destroy(currentBuilding);
            }

            StartCoroutine(SpawnNewBuildingAfterDelay(newBuildingSpawnDelay));

            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayOutdoor();
            }
        }

        private IEnumerator SpawnNewBuildingAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
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

            currentInteriorBoss = enemy;
            enemy.OnDeath += HandleInteriorBossDeath;

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

        private void HandleInteriorBossDeath()
        {
            if (currentInteriorBoss == null) return;

            Vector3 dropPosition = currentInteriorBoss.transform.position;
            currentInteriorBoss.OnDeath -= HandleInteriorBossDeath;
            currentInteriorBoss = null;

            if (bossBagPrefab != null)
            {
                Instantiate(bossBagPrefab, dropPosition, Quaternion.identity);
            }
        }

        private void SpawnNewBuilding()
        {
            Vector3 spawnPos = GetRandomPortalPosition();

            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector3 candidate = GetRandomPortalPosition();
                if (Vector3.Distance(candidate, player.position) > minSpawnDistanceFromPlayer)
                {
                    spawnPos = candidate;
                    break;
                }
            }

            currentBuilding = Instantiate(buildingPrefab, spawnPos, Quaternion.identity);
        }

        private Vector3 GetRandomPortalPosition()
        {
            if (portalSpawner == null || portalSpawner.childCount == 0)
            {
                return mapRoomCenter.position;
            }

            int index = Random.Range(0, portalSpawner.childCount);
            return portalSpawner.GetChild(index).position;
        }
    }
}
