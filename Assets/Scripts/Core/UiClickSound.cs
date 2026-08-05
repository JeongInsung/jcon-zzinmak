using UnityEngine;

namespace Zzinmak.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class UiClickSound : MonoBehaviour
    {
        public static UiClickSound Instance { get; private set; }

        [SerializeField] private AudioClip clickClip;

        private AudioSource audioSource;

        private void Awake()
        {
            Instance = this;

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }

        private void OnEnable()
        {
            GameEvents.OnSfxVolumeChanged += HandleSfxVolumeChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnSfxVolumeChanged -= HandleSfxVolumeChanged;
        }

        private void HandleSfxVolumeChanged(float volume)
        {
            audioSource.volume = volume;
        }

        public void Play()
        {
            if (clickClip != null)
            {
                audioSource.PlayOneShot(clickClip);
            }
        }
    }
}
