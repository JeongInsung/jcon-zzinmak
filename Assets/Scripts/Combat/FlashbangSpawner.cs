using System.Collections;
using UnityEngine;
using Zzinmak.Player;

namespace Zzinmak.Combat
{
    public class FlashbangSpawner : MonoBehaviour
    {
        [SerializeField] private float interval = 30f;
        [Tooltip("How far ahead of the player, along their current aim direction, the flashbang lands.")]
        [SerializeField] private float throwDistance = 4f;
        [SerializeField] private GameObject grenadePrefab;

        private PlayerController playerController;
        private Coroutine loop;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        public void Activate()
        {
            if (loop == null)
            {
                loop = StartCoroutine(FlashbangLoop());
            }
        }

        private IEnumerator FlashbangLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                ThrowFlashbang();
            }
        }

        private void ThrowFlashbang()
        {
            if (grenadePrefab == null) return;

            Vector2 aimDir = playerController != null ? playerController.AimDirection : Vector2.right;
            Vector3 landingPosition = transform.position + (Vector3)(aimDir * throwDistance);

            var grenadeObj = Instantiate(grenadePrefab, transform.position, Quaternion.identity);
            var grenade = grenadeObj.GetComponent<FlashbangGrenade>();
            if (grenade != null)
            {
                grenade.Throw(landingPosition);
            }
        }
    }
}
