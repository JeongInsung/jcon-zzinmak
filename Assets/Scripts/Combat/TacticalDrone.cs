using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Player;

namespace Zzinmak.Combat
{
    public class TacticalDrone : MonoBehaviour
    {
        [SerializeField] private GameObject dronePrefab;
        [SerializeField] private float damage = 4f;
        [SerializeField] private int levelsPerBonusDrone = 3;
        [SerializeField] private int maxDroneCount = 3;
        [SerializeField] private float hoverRadius = 1.5f;

        private readonly List<Transform> drones = new List<Transform>();
        private PlayerStats stats;
        private bool active;
        private int currentPlayerLevel = 1;
        private int levelAtActivation;
        private int bonusDronesGranted;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

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

            if (!active || levelsPerBonusDrone <= 0) return;

            int levelsSinceActivation = currentPlayerLevel - levelAtActivation;
            int targetBonusDrones = levelsSinceActivation / levelsPerBonusDrone;

            while (bonusDronesGranted < targetBonusDrones && drones.Count < maxDroneCount)
            {
                SpawnDrone();
                bonusDronesGranted++;
            }
        }

        public void Activate()
        {
            if (active) return;
            active = true;
            levelAtActivation = currentPlayerLevel;

            SpawnDrone();
        }

        private void SpawnDrone()
        {
            var droneObj = Instantiate(dronePrefab, transform.position, Quaternion.identity);
            var controller = droneObj.GetComponent<DroneController>();
            if (controller != null)
            {
                float angle = (360f / maxDroneCount) * drones.Count;
                Vector2 offset = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * hoverRadius;
                controller.Initialize(this, transform, offset);
            }

            drones.Add(droneObj.transform);
        }

        public float GetDamage()
        {
            return damage * (stats != null ? stats.damageMultiplier : 1f);
        }

        public int GetExtraProjectiles()
        {
            return stats != null ? stats.extraProjectiles : 0;
        }
    }
}
