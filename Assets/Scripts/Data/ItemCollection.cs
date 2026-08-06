using UnityEngine;

namespace Zzinmak.Data
{
    // 아이템을 실제로 한 번이라도 획득했는지를 세션 간 저장한다. 게임 씬(ItemSelectionManager)이
    // 기록하고 메인메뉴(도감)가 읽는다. 프로젝트에 세이브 시스템이 없어 PlayerPrefs만 사용한다.
    // 야간투시경/레이저사이트/던전탐지기처럼 이미 영구 효과가 있는 아이템도 이 플래그를 별도로 갖는다 —
    // 이건 "효과가 유지되는가"가 아니라 "도감에 한 번 등록됐는가"를 나타내는 순수 기록용 플래그다.
    public static class ItemCollection
    {
        private const string KeyPrefix = "ItemCollected_";

        public static bool IsUnlocked(ItemData item)
        {
            if (item == null) return false;
            return PlayerPrefs.GetInt(KeyPrefix + item.name, 0) == 1;
        }

        public static void MarkCollected(ItemData item)
        {
            if (item == null) return;
            PlayerPrefs.SetInt(KeyPrefix + item.name, 1);
            PlayerPrefs.Save();
        }
    }
}
