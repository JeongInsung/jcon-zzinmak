using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zzinmak.Data;

namespace Zzinmak.UI
{
    // 메인메뉴의 아이템 도감. 9개 타일은 순서가 고정이라 배열 인덱스로 직접 다룬다.
    // ItemCollection이 기록한 "최초 획득 여부"를 읽어서 아이콘/이름을 실물 또는 검은 빈 칸+"???"로 보여주고,
    // 타일을 클릭하면 같은 정보를 상세 팝업에 크게 띄운다.
    public class ItemCodexController : MonoBehaviour
    {
        [SerializeField] private ItemData[] items = new ItemData[9];
        [SerializeField] private Button[] tileButtons = new Button[9];
        [SerializeField] private Image[] tileIcons = new Image[9];
        [SerializeField] private TMP_Text[] tileLabels = new TMP_Text[9];

        [SerializeField] private GameObject detailPopup;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TMP_Text detailTitle;
        [SerializeField] private TMP_Text detailDescription;

        private const string LockedLabel = "???";

        // 메뉴로 돌아올 때마다(게임 한 판 하고 온 뒤에도) 최신 수집 상태를 반영한다.
        private void OnEnable()
        {
            int count = Mathf.Min(Mathf.Min(items.Length, tileButtons.Length), Mathf.Min(tileIcons.Length, tileLabels.Length));
            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                if (item == null) continue;

                bool unlocked = ItemCollection.IsUnlocked(item);

                if (tileIcons[i] != null)
                {
                    // 미획득 아이템은 실루엣(그림자)을 보여주지 않고 아이콘 자체를 꺼서,
                    // 타일 배경의 검은 자리가 그대로 빈 칸처럼 보이게 한다.
                    tileIcons[i].sprite = item.icon;
                    tileIcons[i].enabled = unlocked;
                }

                if (tileLabels[i] != null)
                {
                    tileLabels[i].text = unlocked ? item.itemName : LockedLabel;
                }

                if (tileButtons[i] != null)
                {
                    tileButtons[i].interactable = unlocked;
                }
            }
        }

        private void ShowDetail(int index)
        {
            if (index < 0 || index >= items.Length) return;

            var item = items[index];
            if (item == null || !ItemCollection.IsUnlocked(item)) return;

            if (detailIcon != null) detailIcon.sprite = item.icon;
            if (detailTitle != null) detailTitle.text = item.itemName;
            if (detailDescription != null) detailDescription.text = item.description;
            if (detailPopup != null) detailPopup.SetActive(true);
        }

        // 버튼 onClick은 인자 없는 메서드만 안전하게 연결할 수 있어(기존 프로젝트의 모든 버튼이
        // 이 방식이라 검증됨), 인덱스별로 얇은 래퍼를 둔다. 9개뿐이라 스위치보다 이게 더 명확하다.
        public void ShowDetail0() => ShowDetail(0);
        public void ShowDetail1() => ShowDetail(1);
        public void ShowDetail2() => ShowDetail(2);
        public void ShowDetail3() => ShowDetail(3);
        public void ShowDetail4() => ShowDetail(4);
        public void ShowDetail5() => ShowDetail(5);
        public void ShowDetail6() => ShowDetail(6);
        public void ShowDetail7() => ShowDetail(7);
        public void ShowDetail8() => ShowDetail(8);
    }
}
