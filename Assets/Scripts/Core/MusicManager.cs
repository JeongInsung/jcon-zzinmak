using UnityEngine;

namespace Zzinmak.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        [SerializeField] private AudioClip outdoorBgm;
        [SerializeField] private AudioClip dungeonBgm;

        private AudioSource audioSource;

        private void Awake()
        {
            Instance = this;

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        }

        private void OnEnable()
        {
            GameEvents.OnBgmVolumeChanged += HandleBgmVolumeChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnBgmVolumeChanged -= HandleBgmVolumeChanged;
        }

        private void Start()
        {
            PlayOutdoor();
        }

        private void HandleBgmVolumeChanged(float volume)
        {
            audioSource.volume = volume;
        }

        public void PlayOutdoor()
        {
            PlayClip(outdoorBgm);
        }

        public void PlayDungeon()
        {
            PlayClip(dungeonBgm);
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || audioSource.clip == clip) return;

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
