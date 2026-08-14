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
        private const string OrderKey = "ItemCollectionOrder";

        public static bool IsUnlocked(ItemData item)
        {
            if (item == null) return false;
            return PlayerPrefs.GetInt(KeyPrefix + item.name, 0) == 1;
        }

        public static void MarkCollected(ItemData item)
        {
            if (item == null) return;

            MarkCollectedName(item.name);
            PlayerPrefs.Save();
        }

        // 클라우드에서 내려온 획득 순서(이름 배열)를 로컬과 합친다. 이미 획득한 항목은
        // 건드리지 않고, 로컬에 없던 항목만 기존 순서 뒤에 추가된다.
        public static void ImportCollected(string[] itemAssetNames)
        {
            if (itemAssetNames == null) return;

            foreach (string name in itemAssetNames)
            {
                MarkCollectedName(name);
            }
            PlayerPrefs.Save();
        }

        private static void MarkCollectedName(string itemAssetName)
        {
            if (string.IsNullOrEmpty(itemAssetName)) return;

            string key = KeyPrefix + itemAssetName;
            if (PlayerPrefs.GetInt(key, 0) != 1)
            {
                PlayerPrefs.SetInt(key, 1);
                AppendToOrder(itemAssetName);
            }
        }

        private static void AppendToOrder(string itemAssetName)
        {
            string existing = PlayerPrefs.GetString(OrderKey, "");
            PlayerPrefs.SetString(OrderKey, string.IsNullOrEmpty(existing) ? itemAssetName : existing + "," + itemAssetName);
        }

        // Names of every collected item (asset name, e.g. "Item_LaserSight"), in the order
        // each was first picked up. Lets the codex fill slots in pickup order instead of a
        // fixed per-item slot, so it stays correct as bound items are added later.
        public static string[] GetCollectionOrder()
        {
            string existing = PlayerPrefs.GetString(OrderKey, "");
            return string.IsNullOrEmpty(existing) ? new string[0] : existing.Split(',');
        }
    }
}
