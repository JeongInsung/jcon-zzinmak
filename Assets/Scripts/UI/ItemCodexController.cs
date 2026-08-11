using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zzinmak.Data;

namespace Zzinmak.UI
{
    // 메인메뉴의 아이템 도감. `items`는 도감에 등록될 수 있는 귀속 아이템 "풀"이고,
    // 실제로 어느 타일에 뜨는지는 고정 인덱스가 아니라 ItemCollection이 기록한 "먹은 순서"로
    // 정해진다 - 첫 번째로 먹은 귀속 아이템이 타일 0, 두 번째가 타일 1... 아직 안 먹은 나머지
    // 타일은 전부 빈 칸+"???". 나중에 귀속 아이템을 추가할 땐 `items`에 넣기만 하면 된다.
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

        private readonly List<ItemData> displayedItems = new List<ItemData>();

        // 메뉴로 돌아올 때마다(게임 한 판 하고 온 뒤에도) 최신 수집 상태를 반영한다.
        private void OnEnable()
        {
            displayedItems.Clear();
            foreach (string collectedName in ItemCollection.GetCollectionOrder())
            {
                foreach (var candidate in items)
                {
                    if (candidate != null && candidate.name == collectedName)
                    {
                        displayedItems.Add(candidate);
                        break;
                    }
                }
            }

            int tileCount = Mathf.Min(Mathf.Min(tileButtons.Length, tileIcons.Length), tileLabels.Length);
            for (int i = 0; i < tileCount; i++)
            {
                var item = i < displayedItems.Count ? displayedItems[i] : null;
                bool unlocked = item != null;

                if (tileIcons[i] != null)
                {
                    // 미획득 칸은 실루엣(그림자)을 보여주지 않고 아이콘 자체를 꺼서,
                    // 타일 배경의 검은 자리가 그대로 빈 칸처럼 보이게 한다.
                    tileIcons[i].sprite = unlocked ? item.icon : null;
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
            if (index < 0 || index >= displayedItems.Count) return;

            var item = displayedItems[index];
            if (item == null) return;

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
