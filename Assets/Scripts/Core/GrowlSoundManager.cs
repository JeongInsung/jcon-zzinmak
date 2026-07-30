using System.Collections;
using UnityEngine;

namespace Zzinmak.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class GrowlSoundManager : MonoBehaviour
    {
        [SerializeField] private AudioClip[] growlClips;
        [SerializeField] private float minInterval = 5f;
        [SerializeField] private float maxInterval = 12f;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            GameEvents.OnGameOver += PlayRandomGrowl;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= PlayRandomGrowl;
        }

        private void Start()
        {
            PlayRandomGrowl();
            StartCoroutine(PeriodicGrowlRoutine());
        }

        private IEnumerator PeriodicGrowlRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
                PlayRandomGrowl();
            }
        }

        private void PlayRandomGrowl()
        {
            if (growlClips == null || growlClips.Length == 0) return;

            AudioClip clip = growlClips[Random.Range(0, growlClips.Length)];
            if (clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
