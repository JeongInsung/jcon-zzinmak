using UnityEngine;
using Zzinmak.Enemies;

namespace Zzinmak.UI
{
    public class BossDirectionIndicator : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private RectTransform arrow;

        private void Update()
        {
            if (player == null || arrow == null) return;

            var boss = BossMarker.GetNearestActiveBoss(player.position);
            if (boss == null)
            {
                arrow.gameObject.SetActive(false);
                return;
            }

            arrow.gameObject.SetActive(true);

            Vector2 dir = (Vector2)boss.position - (Vector2)player.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrow.localEulerAngles = new Vector3(0f, 0f, angle);
        }
    }
}
