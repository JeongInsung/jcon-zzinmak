using System;

namespace Zzinmak.Core
{
    public static class GameEvents
    {
        public static event Action<float, float> OnHealthChanged;
        public static event Action<float, float> OnExpChanged;
        public static event Action<int> OnLevelChanged;
        public static event Action<int> OnKillCountChanged;
        public static event Action<float> OnTimerChanged;
        public static event Action OnGameOver;

        public static void RaiseHealthChanged(float current, float max) => OnHealthChanged?.Invoke(current, max);
        public static void RaiseExpChanged(float current, float needed) => OnExpChanged?.Invoke(current, needed);
        public static void RaiseLevelChanged(int level) => OnLevelChanged?.Invoke(level);
        public static void RaiseKillCountChanged(int count) => OnKillCountChanged?.Invoke(count);
        public static void RaiseTimerChanged(float elapsed) => OnTimerChanged?.Invoke(elapsed);
        public static void RaiseGameOver() => OnGameOver?.Invoke();
    }
}
