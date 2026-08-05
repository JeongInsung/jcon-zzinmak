using System.Collections;
using UnityEngine;

namespace Zzinmak.UI
{
    /// <summary>
    /// 메인 메뉴 배경음악을 루프 재생한다. 씬 시작 시 페이드인하고,
    /// 씬을 떠날 때(FadeOut 호출) 페이드아웃해서 씬 전환으로 인한 오디오 끊김을 막는다.
    /// 설정창의 BGMVolume(PlayerPrefs)을 매 프레임 반영해 슬라이더 조작이 바로 들리게 한다.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BgmPlayer : MonoBehaviour
    {
        private const string MasterVolumeKey = "MasterVolume";
        private const string BgmVolumeKey = "BGMVolume";

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clip;
        [Range(0f, 1f)]
        [SerializeField] private float baseVolume = 0.6f;
        [SerializeField] private bool respectBgmVolumePref = true;

        [SerializeField] private float fadeInDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 0.6f;
        [Tooltip("씬 시작 시 자동으로 페이드인하며 재생할지 여부.")]
        [SerializeField] private bool playOnStart = true;

        private float fadeLevel; // 0~1, 페이드 진행도. 볼륨 슬라이더 값과는 별개로 관리한다.
        private Coroutine fadeRoutine;

        private void Awake()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;
            audioSource.clip = clip;
            audioSource.volume = 0f;
        }

        private void Start()
        {
            if (playOnStart) Play();
        }

        private void Update()
        {
            float pref = respectBgmVolumePref ? PlayerPrefs.GetFloat(BgmVolumeKey, 1f) : 1f;
            float master = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            audioSource.volume = fadeLevel * baseVolume * pref * master;
        }

        public void Play()
        {
            if (audioSource.clip == null) return;
            audioSource.Play();
            FadeTo(1f, fadeInDuration);
        }

        /// <summary>
        /// 볼륨을 서서히 0으로 낮춘다. 씬 전환 직전(화면 페이드아웃과 동시에) 호출해서
        /// 씬 언로드로 AudioSource가 파괴될 때 소리가 뚝 끊기지 않게 한다.
        /// duration을 생략하면 fadeOutDuration을 쓴다.
        /// </summary>
        public void FadeOut(float duration = -1f)
        {
            FadeTo(0f, duration >= 0f ? duration : fadeOutDuration);
        }

        private void FadeTo(float target, float duration)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(fadeLevel, target, duration));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration)
        {
            float elapsed = 0f;
            duration = Mathf.Max(0.0001f, duration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeLevel = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            fadeLevel = to;
            fadeRoutine = null;
        }
    }
}
