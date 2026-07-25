#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Zzinmak.EditorTools
{
    /// <summary>
    /// 에디터에서 Play를 누르면 현재 열린 씬과 상관없이 항상 Intro 씬부터 시작하게 한다.
    /// 메뉴에서 끌 수 있고, 설정은 EditorPrefs에 저장되어 에디터를 재시작해도 유지된다.
    /// </summary>
    [InitializeOnLoad]
    internal static class PlayFromIntroScene
    {
        private const string MenuPath = "Zzinmak/재생 시 항상 인트로부터 시작";
        private const string PrefKey = "jcon-zzinmak.PlayFromIntro";

        // 씬 이름이 바뀌어도 따라가도록 GUID를 우선 사용하고, 경로는 예비 수단으로 둔다.
        private const string IntroSceneGuid = "70e8ec7b33c8dda46b3c4016f6c8ead2";
        private const string IntroScenePathFallback = "Assets/Scenes/Intro.unity";

        private static bool Enabled
        {
            get => EditorPrefs.GetBool(PrefKey, true);
            set => EditorPrefs.SetBool(PrefKey, value);
        }

        static PlayFromIntroScene()
        {
            // 도메인 리로드 직후에는 AssetDatabase가 아직 준비되지 않았을 수 있으므로 한 프레임 미룬다.
            EditorApplication.delayCall += Apply;
        }

        [MenuItem(MenuPath, false, 100)]
        private static void Toggle()
        {
            Enabled = !Enabled;
            Apply();
        }

        [MenuItem(MenuPath, true, 100)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }

        private static void Apply()
        {
            if (!Enabled)
            {
                // 껐을 때는 반드시 명시적으로 비워야 한다. 그렇지 않으면 이전 설정이 세션 내내 남는다.
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(IntroSceneGuid);
            if (string.IsNullOrEmpty(path))
                path = IntroScenePathFallback;

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (sceneAsset == null)
            {
                Debug.LogWarning(
                    $"[PlayFromIntro] 인트로 씬을 찾을 수 없습니다 (guid={IntroSceneGuid}, path={path}). " +
                    "현재 열린 씬에서 그대로 재생합니다.");
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            if (EditorSceneManager.playModeStartScene != sceneAsset)
                EditorSceneManager.playModeStartScene = sceneAsset;
        }
    }
}
#endif
