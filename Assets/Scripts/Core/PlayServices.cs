using System;
using UnityEngine;
#if ZZINMAK_GPGS && UNITY_ANDROID && !UNITY_EDITOR
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif

namespace Zzinmak.Core
{
    // Google Play 게임즈 서비스 연동 진입점. GPGS 플러그인을 설치하고 Player Settings의
    // Scripting Define Symbols에 ZZINMAK_GPGS를 추가하기 전까지는 이 클래스의 모든 public
    // 메서드가 빈 몸통이라 게임은 지금과 완전히 동일하게 로컬(PlayerPrefs)로만 동작한다.
    //
    // 로컬이 항상 SSOT이고 클라우드 세이브는 그 미러다 - 로그인 실패/거부 시에도 게임이
    // 정상 동작해야 하므로, 병합은 어느 쪽이 최신인지 볼 필요 없는 합집합 정책
    // (PlaySaveBridge.Merge)으로 설계되어 있다.
    public class PlayServices : MonoBehaviour
    {
        public static PlayServices Instance { get; private set; }

        [Tooltip("GPGS Setup 창이 생성하는 GPGSIds 상수 클래스의 리더보드 ID 필드를 넣는다.")]
        [SerializeField] private string leaderboardId = "";

        private const string SaveFileName = "zzinmak_save";

        public bool IsSignedIn { get; private set; }
        public static event Action OnDataSynced;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

#if ZZINMAK_GPGS && UNITY_ANDROID && !UNITY_EDITOR
            PlayGamesPlatform.Instance.Authenticate(OnAuthenticated);
#endif
        }

        // 자동 로그인이 실패했거나 사용자가 이전에 거부한 경우, 설정 화면의 로그인 버튼에서
        // 이 메서드를 호출해 다시 시도한다. v2 플러그인에는 SignOut이 없어 로그아웃 버튼은
        // 만들지 않는다.
        public void SignIn()
        {
#if ZZINMAK_GPGS && UNITY_ANDROID && !UNITY_EDITOR
            PlayGamesPlatform.Instance.ManuallyAuthenticate(OnAuthenticated);
#endif
        }

        public void SubmitBestTime(float seconds)
        {
#if ZZINMAK_GPGS && UNITY_ANDROID && !UNITY_EDITOR
            if (!IsSignedIn || string.IsNullOrEmpty(leaderboardId)) return;

            // TIME 포맷 리더보드는 밀리초 단위로 제출한다.
            long milliseconds = (long)(seconds * 1000f);
            PlayGamesPlatform.Instance.ReportScore(milliseconds, leaderboardId, success =>
            {
                if (!success) Debug.LogWarning("[PlayServices] Leaderboard score submission failed.");
            });
#endif
        }

        public void SaveToCloud()
        {
#if ZZINMAK_GPGS && UNITY_ANDROID && !UNITY_EDITOR
            if (!IsSignedIn) return;

            PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
                SaveFileName, DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime, CommitLocalState);
#endif
        }

#if ZZINMAK_GPGS && UNITY_ANDROID && !UNITY_EDITOR
        private void OnAuthenticated(SignInStatus status)
        {
            IsSignedIn = status == SignInStatus.Success;

            if (IsSignedIn)
            {
                LoadFromCloudAndMerge();
            }
            else
            {
                OnDataSynced?.Invoke();
            }
        }

        private void LoadFromCloudAndMerge()
        {
            PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
                SaveFileName, DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime, (openStatus, game) =>
                {
                    if (openStatus != SavedGameRequestStatus.Success)
                    {
                        Debug.LogWarning("[PlayServices] Failed to open saved game: " + openStatus);
                        OnDataSynced?.Invoke();
                        return;
                    }

                    PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, (readStatus, bytes) =>
                    {
                        if (readStatus == SavedGameRequestStatus.Success && bytes != null && bytes.Length > 0)
                        {
                            CloudSaveData cloud = PlaySaveBridge.Deserialize(bytes);
                            CloudSaveData merged = PlaySaveBridge.Merge(PlaySaveBridge.ReadLocal(), cloud);
                            PlaySaveBridge.WriteLocal(merged);
                        }

                        SaveToCloud();
                        OnDataSynced?.Invoke();
                    });
                });
        }

        private void CommitLocalState(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            if (status != SavedGameRequestStatus.Success)
            {
                Debug.LogWarning("[PlayServices] Failed to open saved game for write: " + status);
                return;
            }

            byte[] data = PlaySaveBridge.Serialize(PlaySaveBridge.ReadLocal());
            var update = new SavedGameMetadataUpdate.Builder().Build();
            PlayGamesPlatform.Instance.SavedGame.CommitUpdate(game, update, data, (commitStatus, _) =>
            {
                if (commitStatus != SavedGameRequestStatus.Success)
                {
                    Debug.LogWarning("[PlayServices] Cloud save commit failed: " + commitStatus);
                }
            });
        }
#endif
    }
}
