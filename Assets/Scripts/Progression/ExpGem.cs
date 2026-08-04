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

            Transform magnetTarget = player;
            float magnetDistance = Vector2.Distance(transform.position, player.position);
            float magnetRange = playerStats.pickupRange;

            var dog = ExpDog.Instance;
            if (dog != null)
            {
                float dogDistance = Vector2.Distance(transform.position, dog.transform.position);
                if (dogDistance <= dog.PickupRange)
                {
                    magnetTarget = dog.transform;
                    magnetDistance = dogDistance;
                    magnetRange = dog.PickupRange;
                }
            }

            if (magnetDistance <= magnetRange)
            {
                transform.position = Vector3.MoveTowards(transform.position, magnetTarget.position, moveSpeed * Time.deltaTime);
            }

            if (magnetDistance <= absorbDistance)
            {
                playerLevel.AddExp(expValue);
                ObjectPool.Instance.Return(gameObject);
            }
        }
    }
}
