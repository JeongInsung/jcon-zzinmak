using System;

namespace Zzinmak.Core
{
    // Play Games Saved Games에 그대로 직렬화되는 스냅샷. 로컬 PlayerPrefs와 클라우드 양쪽을
    // 같은 모양으로 다뤄서 PlaySaveBridge가 둘을 대칭적으로 읽고/합칠 수 있게 한다.
    [Serializable]
    public class CloudSaveData
    {
        public float[] bestTimes = new float[SurvivalRecords.Count];

        // 먹은 순서 그대로가 곧 도감 순서 정보라, 순서를 담는 필드를 따로 두지 않는다.
        public string[] collectedItemNames = new string[0];

        public bool laserSightUnlocked;
        public bool nightVisionUnlocked;
        public bool dungeonLocatorUnlocked;
    }
}
