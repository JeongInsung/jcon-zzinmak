using System.Collections.Generic;
using UnityEngine;

namespace Zzinmak.Enemies
{
    /// <summary>Self-registering marker so UI (e.g. a direction arrow) can find the
    /// nearest active boss without caring whether it spawned outdoors or in a building.</summary>
    public class BossMarker : MonoBehaviour
    {
        private static readonly List<BossMarker> active = new List<BossMarker>();

        public static Transform GetNearestActiveBoss(Vector3 from)
        {
            Transform nearest = null;
            float nearestDistSqr = float.MaxValue;

            foreach (var marker in active)
            {
                if (marker == null || !marker.isActiveAndEnabled) continue;

                float distSqr = (marker.transform.position - from).sqrMagnitude;
                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearest = marker.transform;
                }
            }

            return nearest;
        }

        private void OnEnable()
        {
            active.Add(this);
        }

        private void OnDisable()
        {
            active.Remove(this);
        }
    }
}
