using System.Collections;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Enemies;

namespace Zzinmak.UI
{
    // Plays a fixed-length alert when a monster wave starts: the siren icon blinks and
    // the screen-edge flash pulses red, both for exactly ringCount pulses, then hide -
    // independent of how long the wave itself actually takes to clear.
    [RequireComponent(typeof(AudioSource))]
    public class WaveAlertController : MonoBehaviour
    {
        [SerializeField] private GameObject sirenIcon;
        [SerializeField] private CanvasGroup screenEdgeFlash;
        [SerializeField] private int ringCount = 2;
        [Tooltip("Seconds per ring - matches the siren sprite's own blink-cycle length (16 frames at SirenBlinker's frame rate).")]
        [SerializeField] private float ringDuration = 1.3f;
        [SerializeField] private float maxFlashAlpha = 0.35f;
        [SerializeField] private AudioClip sirenClip;

        private AudioSource audioSource;
        private Coroutine activeRoutine;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }

        private void OnEnable()
        {
            MonsterWaveManager.OnWaveStarted += HandleWaveStarted;
            GameEvents.OnSfxVolumeChanged += HandleSfxVolumeChanged;
        }

        private void OnDisable()
        {
            MonsterWaveManager.OnWaveStarted -= HandleWaveStarted;
            GameEvents.OnSfxVolumeChanged -= HandleSfxVolumeChanged;
        }

        private void HandleSfxVolumeChanged(float volume)
        {
            audioSource.volume = volume;
        }

        private void HandleWaveStarted()
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(PlayAlert());
        }

        private IEnumerator PlayAlert()
        {
            if (sirenIcon != null) sirenIcon.SetActive(true);
            if (screenEdgeFlash != null) screenEdgeFlash.gameObject.SetActive(true);

            float totalDuration = ringCount * ringDuration;
            float elapsed = 0f;
            int lastRing = -1;

            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;

                int currentRing = (int)(elapsed / ringDuration);
                if (currentRing != lastRing)
                {
                    lastRing = currentRing;
                    if (sirenClip != null) audioSource.PlayOneShot(sirenClip);
                }

                if (screenEdgeFlash != null)
                {
                    float t = (elapsed % ringDuration) / ringDuration;
                    screenEdgeFlash.alpha = Mathf.Sin(t * Mathf.PI) * maxFlashAlpha;
                }

                yield return null;
            }

            if (sirenIcon != null) sirenIcon.SetActive(false);
            if (screenEdgeFlash != null) screenEdgeFlash.gameObject.SetActive(false);
            activeRoutine = null;
        }
    }
}
