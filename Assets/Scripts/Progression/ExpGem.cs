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
        [SerializeField] private AudioClip pickupClip;
        [Tooltip("Boss gem: grants exactly one level regardless of the current exp curve, instead of adding expValue as exp.")]
        [SerializeField] private bool guaranteedLevelUp = false;

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
                if (pickupClip != null)
                {
                    // Plain PlayClipAtPoint defaults to a fully 3D source, so it fades out
                    // with camera distance and can end up nearly inaudible. Force 2D so the
                    // pickup cue always plays at full, consistent volume.
                    var soundObj = new GameObject("ExpGemPickupSound");
                    var soundSource = soundObj.AddComponent<AudioSource>();
                    soundSource.clip = pickupClip;
                    soundSource.spatialBlend = 0f;
                    soundSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
                    soundSource.Play();
                    Destroy(soundObj, pickupClip.length);
                }

                if (guaranteedLevelUp)
                {
                    playerLevel.ForceLevelUp();
                }
                else
                {
                    playerLevel.AddExp(expValue);
                }
                ObjectPool.Instance.Return(gameObject);
            }
        }
    }
}
