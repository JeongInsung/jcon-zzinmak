using System.Collections;
using UnityEngine;

namespace Zzinmak.Combat
{
    public class TurretSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject turretPrefab;
        [SerializeField] private float respawnDelay = 5f;

        private WeaponController weaponController;
        private int level;
        private Coroutine respawnLoop;

        private void Awake()
        {
            weaponController = GetComponent<WeaponController>();
        }

        public void PlaceTurret(float duration)
        {
            if (turretPrefab == null) return;

            level++;

            if (respawnLoop == null)
            {
                respawnLoop = StartCoroutine(RespawnLoop(duration));
            }
        }

        private IEnumerator RespawnLoop(float duration)
        {
            while (true)
            {
                SpawnTurret(duration);
                yield return new WaitForSeconds(duration);
                yield return new WaitForSeconds(respawnDelay);
            }
        }

        private void SpawnTurret(float duration)
        {
            var turretObj = Instantiate(turretPrefab, transform.position, Quaternion.identity);
            var controller = turretObj.GetComponent<TurretController>();
            if (controller != null)
            {
                float damage = weaponController != null ? weaponController.GetCurrentDamage() : 0f;
                controller.Initialize(level, damage, duration);
            }
        }
    }
}
