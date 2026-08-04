using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Player;

namespace Zzinmak.Combat
{
    public class TacticalDrone : MonoBehaviour
    {
        [SerializeField] private GameObject dronePrefab;
        [SerializeField] private float damage = 4f;
        [SerializeField] private int maxDroneCount = 3;
        [SerializeField] private float hoverRadius = 1.5f;

        private readonly List<Transform> drones = new List<Transform>();
        private PlayerStats stats;
        private int bonusProjectiles;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        public void Activate()
        {
            if (drones.Count < maxDroneCount)
            {
                SpawnDrone();
            }
            else
            {
                bonusProjectiles++;
            }
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

        public int GetBonusProjectiles()
        {
            return bonusProjectiles;
        }
    }
}
