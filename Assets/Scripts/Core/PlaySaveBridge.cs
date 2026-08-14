using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zzinmak.Data;
using Zzinmak.Progression;

namespace Zzinmak.Core
{
    // PlayerPrefs <-> CloudSaveData 변환과 병합만 담당한다. GPGS 의존성이 전혀 없어
    // 플러그인 설치 여부와 무관하게 에디터에서 그대로 호출/검증할 수 있다.
    public static class PlaySaveBridge
    {
        public static CloudSaveData ReadLocal()
        {
            return new CloudSaveData
            {
                bestTimes = SurvivalRecords.GetAll(),
                collectedItemNames = ItemCollection.GetCollectionOrder(),
                laserSightUnlocked = PlayerPrefs.GetInt(ItemSelectionManager.LaserSightUnlockedKey, 0) == 1,
                nightVisionUnlocked = PlayerPrefs.GetInt(ItemSelectionManager.NightVisionUnlockedKey, 0) == 1,
                dungeonLocatorUnlocked = PlayerPrefs.GetInt(ItemSelectionManager.DungeonLocatorUnlockedKey, 0) == 1,
            };
        }

        public static void WriteLocal(CloudSaveData data)
        {
            SurvivalRecords.SetAll(data.bestTimes);
            ItemCollection.ImportCollected(data.collectedItemNames);

            if (data.laserSightUnlocked) PlayerPrefs.SetInt(ItemSelectionManager.LaserSightUnlockedKey, 1);
            if (data.nightVisionUnlocked) PlayerPrefs.SetInt(ItemSelectionManager.NightVisionUnlockedKey, 1);
            if (data.dungeonLocatorUnlocked) PlayerPrefs.SetInt(ItemSelectionManager.DungeonLocatorUnlockedKey, 1);
            PlayerPrefs.Save();
        }

        // 어느 쪽이 최신인지 판단할 필요가 없도록, 두 스냅샷을 합치기만 하고 절대 지우지
        // 않는다 - 이러면 Saved Games 충돌 콜백에서도 이 함수를 그대로 재사용할 수 있다.
        public static CloudSaveData Merge(CloudSaveData a, CloudSaveData b)
        {
            var mergedTimes = new List<float>(a.bestTimes);
            mergedTimes.AddRange(b.bestTimes);
            mergedTimes.Sort((x, y) => y.CompareTo(x));

            var topTimes = mergedTimes.GetRange(0, Mathf.Min(SurvivalRecords.Count, mergedTimes.Count)).ToArray();

            var order = new List<string>(a.collectedItemNames);
            foreach (string name in b.collectedItemNames)
            {
                if (!order.Contains(name)) order.Add(name);
            }

            return new CloudSaveData
            {
                bestTimes = topTimes,
                collectedItemNames = order.ToArray(),
                laserSightUnlocked = a.laserSightUnlocked || b.laserSightUnlocked,
                nightVisionUnlocked = a.nightVisionUnlocked || b.nightVisionUnlocked,
                dungeonLocatorUnlocked = a.dungeonLocatorUnlocked || b.dungeonLocatorUnlocked,
            };
        }

        public static byte[] Serialize(CloudSaveData data)
        {
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        }

        public static CloudSaveData Deserialize(byte[] bytes)
        {
            return JsonUtility.FromJson<CloudSaveData>(Encoding.UTF8.GetString(bytes));
        }
    }
}
