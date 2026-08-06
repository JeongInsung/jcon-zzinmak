using UnityEngine;
using Zzinmak.Environment;

namespace Zzinmak.UI
{
    public class DungeonDirectionIndicator : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private float orbitRadius = 1.5f;

        private Transform arrowInstance;
        private bool unlocked;

        public void Unlock()
        {
            unlocked = true;
        }

        private void Update()
        {
            bool insideBuilding = MissionManager.Instance != null && MissionManager.Instance.PlayerInBuilding;
            var building = unlocked && !insideBuilding && MissionManager.Instance != null ? MissionManager.Instance.CurrentBuilding : null;
            bool shouldShow = building != null && player != null;

            if (shouldShow && arrowInstance == null && arrowPrefab != null)
            {
                arrowInstance = Instantiate(arrowPrefab).transform;
            }

            if (arrowInstance == null) return;

            arrowInstance.gameObject.SetActive(shouldShow);
            if (!shouldShow) return;

            Vector2 dir = (Vector2)building.position - (Vector2)player.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            arrowInstance.position = player.position + (Vector3)(dir.normalized * orbitRadius);
            arrowInstance.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
