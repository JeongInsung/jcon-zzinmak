using TMPro;
using UnityEngine;
using Zzinmak.Core;

namespace Zzinmak.UI
{
    /// <summary>
    /// 최고 기록 TOP5는 GameOverController가 SurvivalRecords.TrySubmit으로 갱신한다.
    /// 지금까지는 게임오버 화면에서만 보여줬는데, 메인메뉴에도 있어야 다시 플레이할 동기가 생긴다.
    /// 메인메뉴에서는 버튼 라벨(1위 요약)과, 그 버튼을 눌러 여는 팝업(TOP5 상세) 두 곳에 반영한다.
    /// </summary>
    public class MainMenuInfo : MonoBehaviour
    {
        [SerializeField] private TMP_Text bestRecordButtonLabel;
        [Tooltip("1위부터 5위까지 순서대로 배치")]
        [SerializeField] private TMP_Text[] bestRecordRows;
        [SerializeField] private TMP_Text versionText;

        // 게임 플레이 후 메뉴로 돌아왔을 때도 최신 기록이 반영되도록 OnEnable에서 매번 읽는다.
        // 클라우드 동기화가 메뉴 표시보다 늦게 끝나는 경우(로그인/병합이 비동기라)를 위해
        // OnDataSynced가 오면 한 번 더 다시 읽는다.
        private void OnEnable()
        {
            PlayServices.OnDataSynced += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            PlayServices.OnDataSynced -= Refresh;
        }

        private void Refresh()
        {
            float[] records = SurvivalRecords.GetAll();

            if (bestRecordButtonLabel != null)
            {
                bestRecordButtonLabel.text = "최고 기록  " + SurvivalRecords.Format(records[0]);
            }

            if (bestRecordRows != null)
            {
                for (int i = 0; i < bestRecordRows.Length; i++)
                {
                    if (bestRecordRows[i] == null) continue;

                    float time = i < records.Length ? records[i] : 0f;
                    bestRecordRows[i].text = (i + 1) + "위  " + SurvivalRecords.Format(time);
                }
            }

            if (versionText != null) versionText.text = "v" + Application.version;
        }
    }
}
