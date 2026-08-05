using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zzinmak.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "Game";

        [Tooltip("할당하면 검정 페이드아웃 후 씬을 로드한다. 비워두면 clickSoundDelay만큼 기다린 뒤 바로 로드.")]
        [SerializeField] private ScreenFader screenFader;

        [Tooltip("할당하면 씬 전환 시 배경음악도 화면과 함께 페이드아웃한다.")]
        [SerializeField] private BgmPlayer bgmPlayer;

        [Tooltip("클릭 사운드가 잘리지 않도록 씬 전환/종료를 이만큼 지연시킨다. (button02b.mp3 길이 0.79초 기준)")]
        [SerializeField] private float clickSoundDelay = 0.85f;

        private bool isLeaving;

        public void LoadGameScene()
        {
            if (isLeaving) return;   // 연타로 두 번 로드되는 것 방지
            isLeaving = true;

            if (bgmPlayer != null) bgmPlayer.FadeOut();   // 화면 페이드아웃과 동시에 조용해짐

            if (screenFader != null)
            {
                screenFader.FadeOutAndLoad(gameSceneName);
                return;
            }

            StartCoroutine(LoadGameSceneRoutine());
        }

        public void QuitGame()
        {
            if (isLeaving) return;
            isLeaving = true;
            StartCoroutine(QuitGameRoutine());
        }

        private IEnumerator LoadGameSceneRoutine()
        {
            yield return new WaitForSecondsRealtime(clickSoundDelay);
            SceneManager.LoadScene(gameSceneName);
        }

        private IEnumerator QuitGameRoutine()
        {
            yield return new WaitForSecondsRealtime(clickSoundDelay);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
