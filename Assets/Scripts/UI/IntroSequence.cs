using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Zzinmak.UI
{
    public class IntroSequence : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private Image fadeImage;
        [SerializeField] private string nextSceneName = "2 SCENE";
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float fadeStartDelay = 1.8f;

        private bool isTransitioning;

        private void Start()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached += OnVideoFinished;
            }

            StartCoroutine(TriggerFadeAfterDelay());
        }

        private IEnumerator TriggerFadeAfterDelay()
        {
            yield return new WaitForSeconds(fadeStartDelay);
            BeginTransition();
        }

        private void OnVideoFinished(VideoPlayer source)
        {
            BeginTransition();
        }

        public void BeginTransition()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            StartCoroutine(FadeAndLoad());
        }

        private IEnumerator FadeAndLoad()
        {
            if (fadeImage != null)
            {
                float elapsed = 0f;
                Color color = fadeImage.color;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    color.a = Mathf.Clamp01(elapsed / fadeDuration);
                    fadeImage.color = color;
                    yield return null;
                }
                color.a = 1f;
                fadeImage.color = color;
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
