using System.Collections;
using UnityEngine;
using Zzinmak.Data;

namespace Zzinmak.Combat
{
    public class MineSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject minePrefab;
        [Tooltip("Used to compute mine damage as a fraction of this enemy's base health. Assign Enemy_BossZombie.")]
        [SerializeField] private EnemyData bossZombieData;
        [SerializeField] private float baseInterval = 15f;
        [SerializeField] private float intervalReductionPerLevel = 3f;
        [SerializeField] private float minInterval = 6f;

        private int level;
        private Coroutine spawnLoop;

        public void Activate()
        {
            level++;

            if (spawnLoop == null)
            {
                spawnLoop = StartCoroutine(SpawnLoop());
            }
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(CurrentInterval());
                SpawnMine();
            }
        }

        private float CurrentInterval()
        {
            return Mathf.Max(baseInterval - (level - 1) * intervalReductionPerLevel, minInterval);
        }

        private void SpawnMine()
        {
            if (minePrefab == null) return;

            var mineObj = Instantiate(minePrefab, transform.position, Quaternion.identity);
            var mine = mineObj.GetComponent<Mine>();
            if (mine != null)
            {
                mine.Initialize(MineDamage());
            }
        }

        private float MineDamage()
        {
            return bossZombieData != null ? bossZombieData.baseHealth * 0.25f : 0f;
        }
    }
}
