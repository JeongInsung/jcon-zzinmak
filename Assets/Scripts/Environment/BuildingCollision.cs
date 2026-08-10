using UnityEngine;

namespace Zzinmak.Environment
{
    // Buildings aren't on their own physics layer, so blocking colliders are identified
    // by name: BuildingColliderTool.cs generates a non-trigger "fw" ("footwall") child
    // under each building for exactly this purpose. MapHierarchyOrganizerTool.cs
    // (Assets/Editor) later renamed those to "<건물이름>_충돌" during the 2026-08-09
    // hierarchy cleanup, and buildings also carry a separate "<건물이름>_콜라이더"
    // collider that must block zombies the same way, so all three forms are recognized here.
    // When a building has more than one wall segment, AssignSuffixedNames only gives the
    // first one the plain "_충돌"/"_콜라이더" suffix - the rest get " 2", " 3", etc. appended
    // (e.g. "건물 5_충돌 2"), so a trailing " <number>" is stripped before matching or those
    // extra segments silently stop blocking zombies/projectiles/spawn checks.
    public static class BuildingCollision
    {
        public static bool IsFootCollider(Collider2D col)
        {
            string name = StripTrailingIndex(col.gameObject.name);
            return name.EndsWith("_충돌") || name.EndsWith("_콜라이더")
                || name == "fw" || name.StartsWith("fw (") || name.StartsWith("fw(");
        }

        private static string StripTrailingIndex(string name)
        {
            int lastSpace = name.LastIndexOf(' ');
            if (lastSpace < 0) return name;

            string suffix = name.Substring(lastSpace + 1);
            return int.TryParse(suffix, out _) ? name.Substring(0, lastSpace) : name;
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
