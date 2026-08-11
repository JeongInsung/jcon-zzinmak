using UnityEngine;

namespace Zzinmak.Core
{
    // Index 0 keeps the original "BestSurvivalTime" PlayerPrefs key so existing saves'
    // single best score slots in as rank 1 without any migration step.
    public static class SurvivalRecords
    {
        private const string KeyPrefix = "BestSurvivalTime";
        public const int Count = 5;

        public static float[] GetAll()
        {
            var records = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                records[i] = PlayerPrefs.GetFloat(KeyFor(i), 0f);
            }

            return records;
        }

        // Inserts a new survival time into the sorted (best-first) top-5 list if it
        // qualifies, pushing lower entries down and dropping the last one. Returns
        // true if it became the new #1 record.
        public static bool TrySubmit(float time)
        {
            float[] records = GetAll();

            int insertIndex = -1;
            for (int i = 0; i < Count; i++)
            {
                if (time > records[i])
                {
                    insertIndex = i;
                    break;
                }
            }

            if (insertIndex < 0) return false;

            for (int i = Count - 1; i > insertIndex; i--)
            {
                records[i] = records[i - 1];
            }
            records[insertIndex] = time;

            for (int i = 0; i < Count; i++)
            {
                PlayerPrefs.SetFloat(KeyFor(i), records[i]);
            }

            return insertIndex == 0;
        }

        public static string Format(float time)
        {
            if (time <= 0f) return "--:--";

            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        private static string KeyFor(int index)
        {
            return index == 0 ? KeyPrefix : KeyPrefix + index;
        }
    }
}
