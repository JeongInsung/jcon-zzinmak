using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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

        [Header("안전장치")]
        [Tooltip("영상 준비/재생이 아예 진행되지 않을 때 강제로 전환하기까지의 최대 대기 시간(초).")]
        [SerializeField] private float safetyTimeout = 15f;

        [Tooltip("영상 길이를 알아낸 뒤, 종료 신호를 기다려 줄 여유 시간(초).")]
        [SerializeField] private float endMargin = 2f;

        private bool isTransitioning;

        private void Awake()
        {
            // 씬에 저장된 값과 무관하게 항상 투명하게 시작한다.
            if (fadeImage != null)
            {
                Color color = fadeImage.color;
                color.a = 0f;
                fadeImage.color = color;
            }
        }

        private void OnEnable()
        {
            if (videoPlayer == null) return;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.errorReceived += OnVideoError;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }

        private void OnDisable()
        {
            if (videoPlayer == null) return;
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }

        private void Start()
        {
            if (videoPlayer == null)
            {
                Debug.LogWarning("[IntroSequence] videoPlayer가 연결되지 않았습니다. 즉시 전환합니다.");
                BeginTransition();
                return;
            }

            // playOnAwake로 바로 재생하면 디코더 초기화 중에 첫 프레임이 끊기는 느낌이 생긴다.
            // 미리 Prepare()로 디코딩을 끝내놓고, 준비가 끝난 뒤에 Play()해서 첫 프레임부터 매끄럽게 시작한다.
            videoPlayer.playOnAwake = false;
            videoPlayer.Prepare();

            StartCoroutine(WatchdogRoutine());
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            source.Play();
        }

        private void Update()
        {
            if (!isTransitioning && IsSkipInputDown())
            {
                BeginTransition();
            }
        }

        // 터치/클릭/키보드 중 아무거나 눌리면 스킵한다.
        private static bool IsSkipInputDown()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                return true;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;

            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                return true;

            return false;
        }

        // 영상이 정상적으로 끝났을 때 호출된다. 이게 정상 경로.
        private void OnVideoFinished(VideoPlayer source)
        {
            BeginTransition();
        }

        private void OnVideoError(VideoPlayer source, string message)
        {
            Debug.LogError($"[IntroSequence] 영상 재생 오류: {message}. 인트로를 건너뜁니다.");
            BeginTransition();
        }

        /// <summary>
        /// 영상이 끝나지 않아도 씬이 영원히 멈추지 않도록 보장한다.
        /// 처음에는 safetyTimeout을 상한으로 두고, 영상 준비가 끝나 실제 길이를 알게 되면
        /// (길이 + endMargin)으로 상한을 좁힌다.
        /// </summary>
        private IEnumerator WatchdogRoutine()
        {
            float deadline = Time.realtimeSinceStartup + safetyTimeout;
            bool refined = false;

            while (!isTransitioning)
            {
                if (!refined && videoPlayer.isPrepared && videoPlayer.length > 0d)
                {
                    float speed = Mathf.Max(0.01f, videoPlayer.playbackSpeed);
                    float expected = (float)videoPlayer.length / speed + endMargin;
                    deadline = Mathf.Min(deadline, Time.realtimeSinceStartup + expected);
                    refined = true;
                }

                if (Time.realtimeSinceStartup >= deadline)
                {
                    Debug.LogWarning(
                        "[IntroSequence] 영상 종료 신호를 받지 못해 강제로 전환합니다. " +
                        $"(isPrepared={videoPlayer.isPrepared}, isPlaying={videoPlayer.isPlaying}, " +
                        $"time={videoPlayer.time:F2}/{videoPlayer.length:F2})");
                    BeginTransition();
                    yield break;
                }

                yield return null;
            }
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
                    elapsed += Time.unscaledDeltaTime;
                    color.a = Mathf.Clamp01(elapsed / fadeDuration);
                    fadeImage.color = color;
                    yield return null;
                }
                color.a = 1f;
                fadeImage.color = color;
            }

            if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                Debug.LogError(
                    $"[IntroSequence] '{nextSceneName}' 씬을 로드할 수 없습니다. " +
                    "Build Settings에 포함되어 있는지 확인하세요.");
                yield break;
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
