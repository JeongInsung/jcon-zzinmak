using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zzinmak.UI
{
    /// <summary>
    /// 화면 전체를 덮는 CanvasGroup을 검정 <-> 투명으로 페이드한다.
    /// 씬 시작 시 자동으로 검정에서 투명으로 페이드인하고, 필요하면 검정으로 페이드아웃한 뒤 다음 씬을 로드할 수도 있다.
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeInDuration = 0.6f;
        [SerializeField] private float fadeOutDuration = 0.6f;

        [Tooltip("씬 시작 시 자동으로 검정에서 투명으로 페이드인할지 여부.")]
        [SerializeField] private bool fadeInOnStart = true;

        private bool isFading;

        private void Awake()
        {
            if (fadeGroup != null)
            {
                fadeGroup.alpha = 1f;
                fadeGroup.blocksRaycasts = true;
            }
        }

        private void Start()
        {
            if (fadeInOnStart)
            {
                FadeIn();
            }
        }

        public void FadeIn()
        {
            if (fadeGroup == null || isFading) return;
            StartCoroutine(FadeRoutine(fadeGroup.alpha, 0f, fadeInDuration, blockRaycastsAfter: false));
        }

        /// <summary>
        /// 검정으로 페이드아웃한 뒤 지정한 씬을 로드한다. StartButton 등에 직접 연결해서 쓸 수 있다.
        /// </summary>
        public void FadeOutAndLoad(string sceneName)
        {
            if (isFading) return;
            StartCoroutine(FadeOutAndLoadRoutine(sceneName));
        }

        private IEnumerator FadeOutAndLoadRoutine(string sceneName)
        {
            if (fadeGroup != null)
            {
                yield return FadeRoutine(fadeGroup.alpha, 1f, fadeOutDuration, blockRaycastsAfter: true);
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[ScreenFader] '{sceneName}' 씬을 로드할 수 없습니다. Build Settings에 포함되어 있는지 확인하세요.");
                yield break;
            }

            SceneManager.LoadScene(sceneName);
        }

        private IEnumerator FadeRoutine(float from, float to, float duration, bool blockRaycastsAfter)
        {
            isFading = true;
            fadeGroup.blocksRaycasts = true;

            float elapsed = 0f;
            duration = Mathf.Max(0.0001f, duration);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            fadeGroup.alpha = to;
            fadeGroup.blocksRaycasts = blockRaycastsAfter;
            isFading = false;
        }
    }
}
