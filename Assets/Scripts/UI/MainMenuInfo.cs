using TMPro;
using UnityEngine;

namespace Zzinmak.UI
{
    /// <summary>
    /// 최고 기록은 GameOverController가 PlayerPrefs["BestSurvivalTime"]에 초 단위로 저장한다.
    /// 지금까지는 게임오버 화면에서만 보여줬는데, 메인메뉴에도 있어야 다시 플레이할 동기가 생긴다.
    /// 메인메뉴에서는 버튼 라벨(요약)과, 그 버튼을 눌러 여는 팝업(상세) 두 곳에 같은 값을 반영한다.
    /// </summary>
    public class MainMenuInfo : MonoBehaviour
    {
        private const string BestTimeKey = "BestSurvivalTime";

        [SerializeField] private TMP_Text bestRecordButtonLabel;
        [SerializeField] private TMP_Text bestRecordPopupText;
        [SerializeField] private TMP_Text versionText;

        // 게임 플레이 후 메뉴로 돌아왔을 때도 최신 기록이 반영되도록 OnEnable에서 매번 읽는다.
        private void OnEnable()
        {
            string formatted = FormatBestRecord();

            if (bestRecordButtonLabel != null) bestRecordButtonLabel.text = "최고 기록  " + formatted;
            if (bestRecordPopupText != null) bestRecordPopupText.text = formatted;

            if (versionText != null) versionText.text = "v" + Application.version;
        }

        private static string FormatBestRecord()
        {
            float best = PlayerPrefs.GetFloat(BestTimeKey, 0f);
            if (best <= 0f) return "--:--";

            int minutes = Mathf.FloorToInt(best / 60f);
            int seconds = Mathf.FloorToInt(best % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
