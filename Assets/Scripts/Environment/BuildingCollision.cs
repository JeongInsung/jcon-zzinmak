using UnityEngine;

namespace Zzinmak.Environment
{
    // Buildings aren't on their own physics layer, so blocking colliders are identified
    // by name: BuildingColliderTool.cs generates a non-trigger "fw" ("footwall") child
    // under each building for exactly this purpose. MapHierarchyOrganizerTool.cs
    // (Assets/Editor) later renamed those to "<건물이름>_충돌" during the 2026-08-09
    // hierarchy cleanup, so both forms are recognized here.
    public static class BuildingCollision
    {
        public static bool IsFootCollider(Collider2D col)
        {
            string name = col.gameObject.name;
            return name.EndsWith("_충돌")
                || name == "fw" || name.StartsWith("fw (") || name.StartsWith("fw(");
        }

        public static bool OverlapsBuilding(Vector2 point)
        {
            var hits = Physics2D.OverlapPointAll(point);
            foreach (var hit in hits)
            {
                if (hit.isTrigger) continue;
                if (IsFootCollider(hit)) return true;
            }

            return false;
        }

        public static bool OverlapsBuilding(Vector2 position, float radius, Collider2D[] buffer)
        {
            int count = Physics2D.OverlapCircleNonAlloc(position, radius, buffer);
            for (int i = 0; i < count; i++)
            {
                var col = buffer[i];
                if (col == null || col.isTrigger) continue;
                if (IsFootCollider(col)) return true;
            }

            return false;
        }
    }
}
