#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Zzinmak.EditorTools
{
    /// <summary>
    /// Map.prefab 하이러키 전체 정리. 이전 버전(첫 실행)은 (a) "_건물_" 류 이름 태그이거나
    /// (b) dirt/road 스프라이트인 것만 분류해서, 정크 이름을 가진 채 다른 스프라이트를 쓰는
    /// 오브젝트(비닐하우스/군용기지 등 서브폴더 130개)를 놓쳤고, 자식 오브젝트의 ChatGPT/Photoroom
    /// 이름은 아예 손대지 않았다. 이 버전은 그 문제를 고치고, 몇 번을 다시 실행해도 같은 결과가
    /// 나오도록(멱등) 다시 짰다.
    ///
    /// 분류 규칙(우선순위 순, 걸리면 재귀 중단):
    ///  1. 이미 "카테고리 N" 형태인 이름 → 그 카테고리 (재실행 대응)
    ///  2. 이름에 "_건물_"/"_장식_"/"_자동차_" 포함 → 해당 카테고리
    ///  3. 폴더 자체가 정확히 "건물"/"장식"/"바닥"/"도로"/"자동차"/"논"이면, 그 직속 자식 전부를
    ///     해당 카테고리 단위로 봄 — area 밑 "비닐하우스/건물", "군용기지2/바닥" 같은 걸 잡는 규칙.
    ///     (Map 루트 바로 아래의 카테고리 그룹 자체도 이 규칙에 걸려서, 이미 들어있는 것들을
    ///     "재수집"하는 것으로 동작 — 그래서 첫 실행 후 재실행해도 안전하다.)
    ///  4. SpriteRenderer 텍스처가 dirt*/road*로 시작 → 바닥/도로
    ///  5. 위 어디에도 안 걸리면 중간 폴더로 보고 재귀
    ///
    /// Transform의 position/rotation/scale은 절대 손대지 않는다 — SetParent(worldPositionStays:true)로
    /// 재부모화만 하므로 화면에 보이는 배치는 그대로 유지된다([[이동_버벅임_수정]]의 격자 스냅
    /// 시도에서 위치를 건드렸다가 유기적으로 배치된 바닥이 뒤죽박죽 보이는 부작용이 있었음 —
    /// 이번엔 위치를 완전히 그대로 둔다).
    ///
    /// 이름 재부여: 6개 카테고리(바닥/도로/건물/장식/자동차/논) 전부 화면 기준 왼쪽 위부터
    /// 오른쪽으로, 다 채우면 다음 줄로 내려가는 읽기 순서로 "카테고리 1"부터 새로 번호를 매긴다.
    /// 기존 "area1_건물_04 (ChatGPT Image ...)" 같은 이름은 통째로 버려진다.
    /// 그 자식들(스프라이트 조각/콜라이더)도 재귀적으로 "<부모이름>_1", "<부모이름>_충돌"(적을
    /// 막는 콜라이더 — 구 fw), "<부모이름>_콜라이더"(안 막는 콜라이더 — 구 비닐 콜라이더/DE)로
    /// 다시 이름 붙인다. 콜라이더 역할을 이름 하나로 합치지 않는 이유는
    /// Assets/Scripts/Environment/BuildingCollision.cs가 "fw" 이름으로 "적을 막는 벽"을
    /// 판별하기 때문 — 합치면 장식용 콜라이더까지 적을 막아버려 게임플레이가 바뀐다
    /// (그래서 BuildingCollision.cs도 "_충돌" 접미사를 인식하도록 같이 고쳤다).
    ///
    /// 콜라이더 정리: 건물/장식/자동차를 통째로 옮기므로 그 자식 콜라이더는 자동으로 같이
    /// 딸려간다. 그 외 혼자 떨어져 있는 Collider2D(예: Tilemap 바로 밑의 "fw (1)")는 스프라이트
    /// 바운드 중심이 가장 가까운 건물/장식의 자식으로 옮긴다.
    ///
    /// 반드시 Map.prefab을 프리팹 모드로 연 상태에서 실행할 것. PortalSpawner는 게임 로직이
    /// 자식 인덱스로 참조하므로(MissionManager.cs) 절대 건드리지 않는다.
    /// </summary>
    internal static class MapHierarchyOrganizerTool
    {
        private const string GroundGroupName = "바닥";
        private const string RoadGroupName = "도로";
        private const string BuildingGroupName = "건물";
        private const string DecorationGroupName = "장식";
        private const string CarGroupName = "자동차";
        private const string FieldGroupName = "논";
        private const string PortalSpawnerName = "PortalSpawner";

        private static readonly string[] AllCategories =
            { GroundGroupName, RoadGroupName, BuildingGroupName, DecorationGroupName, CarGroupName, FieldGroupName };

        [MenuItem("Zzinmak/맵 하이러키 정리 미리보기")]
        private static void Preview() => Run(apply: false);

        [MenuItem("Zzinmak/맵 하이러키 정리 적용")]
        private static void Apply() => Run(apply: true);

        private static void Run(bool apply)
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null)
            {
                Debug.LogError("[MapHierarchyOrganizerTool] Map.prefab을 더블클릭해서 프리팹 모드로 연 뒤 실행하세요.");
                return;
            }

            Transform mapRoot = stage.prefabContentsRoot.transform;

            var ground = new List<Transform>();
            var road = new List<Transform>();
            var buildings = new List<Transform>();
            var decorations = new List<Transform>();
            var cars = new List<Transform>();
            var fields = new List<Transform>();

            foreach (var child in SnapshotChildren(mapRoot))
            {
                if (child.name == PortalSpawnerName) continue;
                Classify(child, ground, road, buildings, decorations, cars, fields);
            }

            var sb = new StringBuilder();
            sb.AppendLine(apply
                ? "[MapHierarchyOrganizerTool] 적용 시작"
                : "[MapHierarchyOrganizerTool] 미리보기 (변경 없음)");
            sb.AppendLine($"  {GroundGroupName}: {ground.Count}개");
            sb.AppendLine($"  {RoadGroupName}: {road.Count}개");
            sb.AppendLine($"  {BuildingGroupName}: {buildings.Count}개");
            sb.AppendLine($"  {DecorationGroupName}: {decorations.Count}개");
            sb.AppendLine($"  {CarGroupName}: {cars.Count}개");
            sb.AppendLine($"  {FieldGroupName}: {fields.Count}개");

            if (!apply)
            {
                Debug.Log(sb.ToString());
                return;
            }

            Transform groundGroup = GetOrCreateGroup(mapRoot, GroundGroupName);
            Transform roadGroup = GetOrCreateGroup(mapRoot, RoadGroupName);
            Transform buildingGroup = GetOrCreateGroup(mapRoot, BuildingGroupName);
            Transform decorationGroup = GetOrCreateGroup(mapRoot, DecorationGroupName);
            Transform carGroup = GetOrCreateGroup(mapRoot, CarGroupName);
            Transform fieldGroup = GetOrCreateGroup(mapRoot, FieldGroupName);

            Reparent(ground, groundGroup);
            Reparent(road, roadGroup);
            Reparent(buildings, buildingGroup);
            Reparent(decorations, decorationGroup);
            Reparent(cars, carGroup);
            Reparent(fields, fieldGroup);

            // 이름을 매기기 전에 떨어진 콜라이더부터 건물/장식 밑으로 붙여야, 아래 자식 재이름
            // 패스에서 이 콜라이더도 "<부모>_충돌"/"<부모>_콜라이더"로 같이 정리된다.
            var allUnits = new HashSet<Transform>();
            foreach (var t in ground) allUnits.Add(t);
            foreach (var t in road) allUnits.Add(t);
            foreach (var t in buildings) allUnits.Add(t);
            foreach (var t in decorations) allUnits.Add(t);
            foreach (var t in cars) allUnits.Add(t);
            foreach (var t in fields) allUnits.Add(t);
            int reattached = FixOrphanColliders(mapRoot, buildings, decorations, allUnits, sb);

            Transform tilemap = mapRoot.Find("Tilemap");
            int removed = CleanupEmpty(tilemap);

            RenameCategory(ground, GroundGroupName);
            RenameCategory(road, RoadGroupName);
            RenameCategory(buildings, BuildingGroupName);
            RenameCategory(decorations, DecorationGroupName);
            RenameCategory(cars, CarGroupName);
            RenameCategory(fields, FieldGroupName);

            foreach (var t in buildings) RenameChildrenRecursive(t);
            foreach (var t in decorations) RenameChildrenRecursive(t);
            foreach (var t in cars) RenameChildrenRecursive(t);

            sb.AppendLine($"  빈 폴더 정리: {removed}개 삭제");
            sb.AppendLine($"  떨어져 있던 콜라이더 재배치: {reattached}개");
            if (tilemap != null && tilemap.childCount > 0)
                sb.AppendLine($"  ⚠ Tilemap 밑에 분류 못 한 오브젝트 {tilemap.childCount}개가 남아있습니다 — 직접 확인 필요.");

            EditorSceneManager.MarkSceneDirty(stage.scene);
            sb.AppendLine("[MapHierarchyOrganizerTool] 적용 완료. 프리팹 모드에서 Ctrl+S로 저장하세요.");
            Debug.Log(sb.ToString());
        }

        private static void RenameCategory(List<Transform> items, string categoryName)
        {
            var ordered = OrderReadingOrder(items);
            for (int i = 0; i < ordered.Count; i++)
                ordered[i].name = $"{categoryName} {i + 1}";
        }

        /// <summary>
        /// node의 자식들을 훑으며 이름/스프라이트/폴더명으로 분류한다. 분류에 걸린 노드는 그
        /// 자체(하위 트리 포함)를 하나의 단위로 옮기고 더 안 들어간다. node 자신이 정확히
        /// 카테고리 이름(건물/장식/바닥/도로/자동차/논)이면 그 직속 자식 전부를 그 카테고리로 본다
        /// (area 서브폴더나 Map 루트의 카테고리 그룹 자체를 다시 훑을 때 이 경로를 탄다).
        /// </summary>
        private static void Classify(Transform node, List<Transform> ground, List<Transform> road,
            List<Transform> buildings, List<Transform> decorations, List<Transform> cars, List<Transform> fields)
        {
            string folderCategory = CategoryForExactFolderName(node.name);
            if (folderCategory != null)
            {
                foreach (var c in SnapshotChildren(node))
                    AddToCategory(folderCategory, c, ground, road, buildings, decorations, cars, fields);
                return;
            }

            foreach (var child in SnapshotChildren(node))
            {
                string already = AlreadyOrganizedCategory(child.name);
                if (already != null) { AddToCategory(already, child, ground, road, buildings, decorations, cars, fields); continue; }

                if (child.name.Contains("_건물_")) { buildings.Add(child); continue; }
                if (child.name.Contains("_장식_")) { decorations.Add(child); continue; }
                if (child.name.Contains("_자동차_")) { cars.Add(child); continue; }

                var sr = child.GetComponent<SpriteRenderer>();
                string texName = sr != null && sr.sprite != null && sr.sprite.texture != null ? sr.sprite.texture.name : null;
                if (texName != null && texName.StartsWith("dirt")) { ground.Add(child); continue; }
                if (texName != null && texName.StartsWith("road")) { road.Add(child); continue; }

                Classify(child, ground, road, buildings, decorations, cars, fields);
            }
        }

        private static string CategoryForExactFolderName(string name)
        {
            foreach (var cat in AllCategories)
                if (name == cat) return cat;
            return null;
        }

        private static string AlreadyOrganizedCategory(string name)
        {
            foreach (var cat in AllCategories)
            {
                string prefix = cat + " ";
                if (name.StartsWith(prefix) && int.TryParse(name.Substring(prefix.Length), out _))
                    return cat;
            }
            return null;
        }

        private static void AddToCategory(string category, Transform t, List<Transform> ground, List<Transform> road,
            List<Transform> buildings, List<Transform> decorations, List<Transform> cars, List<Transform> fields)
        {
            if (category == GroundGroupName) ground.Add(t);
            else if (category == RoadGroupName) road.Add(t);
            else if (category == BuildingGroupName) buildings.Add(t);
            else if (category == DecorationGroupName) decorations.Add(t);
            else if (category == CarGroupName) cars.Add(t);
            else if (category == FieldGroupName) fields.Add(t);
        }

        private static Transform GetOrCreateGroup(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing;

            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "맵 그룹 생성");
            Undo.SetTransformParent(go.transform, parent, "맵 그룹 생성");
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        private static void Reparent(List<Transform> items, Transform target)
        {
            foreach (var t in items)
            {
                if (t.parent == target) continue;
                Undo.SetTransformParent(t, target, "맵 하이러키 정리");
            }
        }

        /// <summary>
        /// 자식이 없고 Transform 외 컴포넌트가 없는 빈 폴더를 하위부터(post-order) 지운다.
        /// </summary>
        private static int CleanupEmpty(Transform t)
        {
            if (t == null) return 0;

            var children = SnapshotChildren(t);
            int removed = 0;
            foreach (var c in children)
                removed += CleanupEmpty(c);

            if (t.childCount == 0 && t.GetComponents<Component>().Length <= 1)
            {
                Undo.DestroyObjectImmediate(t.gameObject);
                removed++;
            }
            return removed;
        }

        /// <summary>
        /// 왼쪽 위부터 오른쪽으로, 다 채우면 다음 줄(아래)로 내려가는 읽기 순서로 정렬한다.
        /// 완벽한 격자가 아니므로 스프라이트 높이의 중간값을 기준으로 "같은 줄" 허용 오차를 잡는다.
        /// </summary>
        private static List<Transform> OrderReadingOrder(List<Transform> items)
        {
            if (items.Count == 0) return items;

            var heights = new List<float>();
            foreach (var t in items)
            {
                float h = GetCombinedSpriteBounds(t).size.y;
                if (h > 0f) heights.Add(h);
            }
            heights.Sort();
            float medianHeight = heights.Count > 0 ? heights[heights.Count / 2] : 1f;
            float rowTolerance = Mathf.Max(0.01f, medianHeight * 0.6f);

            var sorted = items.OrderByDescending(t => t.position.y).ToList();
            var rows = new List<List<Transform>>();
            List<Transform> currentRow = null;
            float currentRowTopY = 0f;

            foreach (var t in sorted)
            {
                float y = t.position.y;
                if (currentRow == null || (currentRowTopY - y) > rowTolerance)
                {
                    currentRow = new List<Transform>();
                    rows.Add(currentRow);
                    currentRowTopY = y;
                }
                currentRow.Add(t);
            }

            foreach (var row in rows)
                row.Sort((a, b) => a.position.x.CompareTo(b.position.x));

            var result = new List<Transform>(items.Count);
            foreach (var row in rows)
                result.AddRange(row);
            return result;
        }

        /// <summary>
        /// 건물/장식/자동차 하나(이미 "카테고리 N"으로 재이름된 상태)의 자식들을 역할별로
        /// 재귀적으로 재이름한다: 적을 막는 콜라이더(구 fw) → "&lt;부모&gt;_충돌",
        /// 안 막는 콜라이더(구 DE/비닐 콜라이더) → "&lt;부모&gt;_콜라이더", 나머지(스프라이트 등)
        /// → "&lt;부모&gt;_1", "&lt;부모&gt;_2"... 처음 하나면 번호를 안 붙이고, 2개 이상이면
        /// 두 번째부터 " 2", " 3"...을 붙인다.
        /// </summary>
        private static void RenameChildrenRecursive(Transform parent)
        {
            var children = SnapshotChildren(parent);
            if (children.Count == 0) return;

            var hit = new List<Transform>();
            var collider = new List<Transform>();
            var plain = new List<Transform>();

            foreach (var child in children)
            {
                bool isCollider = child.GetComponent<Collider2D>() != null;
                if (isCollider && IsLegacyHitColliderName(child.name)) hit.Add(child);
                else if (isCollider) collider.Add(child);
                else plain.Add(child);
            }

            AssignSuffixedNames(parent.name, "_충돌", hit, alwaysNumber: false);
            AssignSuffixedNames(parent.name, "_콜라이더", collider, alwaysNumber: false);
            AssignSuffixedNames(parent.name, "_", plain, alwaysNumber: true);

            foreach (var child in children)
                RenameChildrenRecursive(child);
        }

        private static bool IsLegacyHitColliderName(string name)
        {
            return name == "fw" || name.StartsWith("fw (") || name.StartsWith("fw(") || name.EndsWith("_충돌");
        }

        private static void AssignSuffixedNames(string parentName, string suffix, List<Transform> items, bool alwaysNumber)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (alwaysNumber)
                    items[i].name = $"{parentName}{suffix}{i + 1}";
                else
                    items[i].name = i == 0 ? $"{parentName}{suffix}" : $"{parentName}{suffix} {i + 1}";
            }
        }

        /// <summary>
        /// 건물/장식 등에 붙지 못하고 떨어져 있는 Collider2D 오브젝트를 찾아, 스프라이트 바운드
        /// 중심이 가장 가까운 건물/장식의 자식으로 옮긴다. 이미 정리된 단위(카테고리 N, 또는
        /// 레거시 _건물_/_장식_/_자동차_ 태그) 내부는 그 자체로 완결된 것으로 보고 더 들어가지 않는다.
        /// </summary>
        private static int FixOrphanColliders(Transform mapRoot, List<Transform> buildings, List<Transform> decorations,
            HashSet<Transform> allUnits, StringBuilder log)
        {
            var targets = new List<Transform>();
            targets.AddRange(buildings);
            targets.AddRange(decorations);
            if (targets.Count == 0) return 0;

            var orphans = new List<Transform>();
            foreach (var child in SnapshotChildren(mapRoot))
            {
                if (child.name == PortalSpawnerName) continue;
                CollectOrphanColliders(child, allUnits, orphans);
            }

            int moved = 0;
            foreach (var orphan in orphans)
            {
                Transform nearest = FindNearestTarget(orphan, targets);
                if (nearest == null) continue;

                log.AppendLine($"    '{orphan.name}' → '{nearest.name}' 자식으로 이동");
                Undo.SetTransformParent(orphan, nearest, "콜라이더 재배치");
                moved++;
            }
            return moved;
        }

        /// <summary>
        /// allUnits(방금 분류된 6개 카테고리 단위)에 속하는 노드는 내부로 들어가지 않는다 —
        /// 아직 이름이 안 바뀐 상태라도(예: "비닐하우스/건물" 안에서 갓 분류된 건물) 이미
        /// 완결된 단위이므로, 그 자식인 fw/DE를 "떨어진 콜라이더"로 착각해 엉뚱한 곳으로
        /// 옮기면 안 된다. 레거시 이름 태그도 추가로 확인해 옛 상태에서도 안전하게 동작한다.
        /// </summary>
        private static void CollectOrphanColliders(Transform node, HashSet<Transform> allUnits, List<Transform> result)
        {
            foreach (var child in SnapshotChildren(node))
            {
                bool isUnit = allUnits.Contains(child)
                    || AlreadyOrganizedCategory(child.name) != null
                    || child.name.Contains("_건물_") || child.name.Contains("_장식_") || child.name.Contains("_자동차_");
                if (isUnit) continue; // 이미 완결된 단위 — 내부의 콜라이더는 건드리지 않는다.

                if (child.GetComponent<Collider2D>() != null)
                    result.Add(child);

                CollectOrphanColliders(child, allUnits, result);
            }
        }

        private static Transform FindNearestTarget(Transform orphan, List<Transform> targets)
        {
            Vector3 pos = orphan.position;
            Transform best = null;
            float bestDist = float.MaxValue;
            foreach (var t in targets)
            {
                float d = Vector3.Distance(pos, GetCombinedSpriteBounds(t).center);
                if (d < bestDist) { bestDist = d; best = t; }
            }
            return best;
        }

        private static Bounds GetCombinedSpriteBounds(Transform t)
        {
            var renderers = t.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0)
                return new Bounds(t.position, Vector3.zero);

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            return b;
        }

        private static List<Transform> SnapshotChildren(Transform t)
        {
            var list = new List<Transform>(t.childCount);
            for (int i = 0; i < t.childCount; i++) list.Add(t.GetChild(i));
            return list;
        }
    }
}
#endif
