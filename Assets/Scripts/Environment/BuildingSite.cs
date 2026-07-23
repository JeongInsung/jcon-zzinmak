using UnityEngine;

namespace Zzinmak.Environment
{
    public class BuildingSite : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (MissionManager.Instance == null) return;

            MissionManager.Instance.EnterBuilding();
        }
    }
}
