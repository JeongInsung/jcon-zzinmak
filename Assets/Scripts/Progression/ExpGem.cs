using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Player;

namespace Zzinmak.Progression
{
    public class ExpGem : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float absorbDistance = 0.2f;
        [SerializeField] private float lifetime = 5f;

        private Transform player;
        private PlayerStats playerStats;
        private PlayerLevel playerLevel;
        private int expValue;
        private float lifeTimer;

        public void SetValue(int value)
        {
            expValue = value;
        }

        private void OnEnable()
        {
            lifeTimer = lifetime;

            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    playerStats = playerObj.GetComponent<PlayerStats>();
                    playerLevel = playerObj.GetComponent<PlayerLevel>();
                }
            }
        }

        private void Update()
        {
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                ObjectPool.Instance.Return(gameObject);
                return;
            }

            if (player == null || playerStats == null) return;

            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= playerStats.pickupRange)
            {
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }

            if (distance <= absorbDistance)
            {
                playerLevel.AddExp(expValue);
                ObjectPool.Instance.Return(gameObject);
            }
        }
    }
}
