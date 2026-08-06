using UnityEngine;
using Zzinmak.Progression;

namespace Zzinmak.Environment
{
    public class BossBag : MonoBehaviour
    {
        private bool opened;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (opened) return;
            if (!other.CompareTag("Player")) return;
            if (ItemSelectionManager.Instance == null) return;

            opened = true;
            ItemSelectionManager.Instance.Open();
            Destroy(gameObject);
        }
    }
}
