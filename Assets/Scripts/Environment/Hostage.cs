using UnityEngine;

namespace Zzinmak.Environment
{
    public class Hostage : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (MissionManager.Instance == null) return;

            MissionManager.Instance.TryRescue();
        }
    }
}
