using System.Collections.Generic;
using UnityEngine;

namespace Zzinmak.Enemies
{
    /// <summary>Self-maintained list of currently active enemies. Lets systems find
    /// the nearest enemy in O(active enemy count) without the per-frame allocation
    /// and full-scene scan of GameObject.FindGameObjectsWithTag.</summary>
    public static class EnemyTracker
    {
        private static readonly List<Enemy> active = new List<Enemy>();

        public static IReadOnlyList<Enemy> Active => active;

        public static void Register(Enemy enemy)
        {
            active.Add(enemy);
        }

        public static void Unregister(Enemy enemy)
        {
            active.Remove(enemy);
        }

        public static Transform GetNearest(Vector3 from, float maxRange)
        {
            Transform nearest = null;
            float nearestDistSqr = maxRange * maxRange;

            for (int i = 0; i < active.Count; i++)
            {
                var enemy = active[i];
                if (enemy == null || !enemy.isActiveAndEnabled || enemy.IsDead) continue;

                float distSqr = (enemy.transform.position - from).sqrMagnitude;
                if (distSqr <= nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearest = enemy.transform;
                }
            }

            return nearest;
        }
    }
}
