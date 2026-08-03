#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Zzinmak.EditorTools
{
    /// <summary>
    /// 건물 스프라이트 밑단에 "fw"라는 이름의 보이지 않는 BoxCollider2D 자식을 일괄로 만들어
    /// 플레이어/좀비가 건물을 통과하지 못하게 막는 툴.
    ///
    /// 기존에 손으로 넣은 fw 6개(씬 루트, 흰 사각형 스프라이트 + Transform 스케일로 크기 조절)를
    /// 그대로 참고해서 만들었다. 다만 씬 루트에 두면 MapRoom/Map의 트랜스폼이 바뀔 때마다
    /// 건물과 어긋나므로(7/30 사례), 이 툴이 만드는 fw는 항상 해당 건물의 "자식"으로 들어가고
    /// 크기는 Transform 스케일이 아니라 BoxCollider2D의 size/offset으로만 표현한다.
    ///
    /// 사용 순서: 측정 → (필요하면 비율 조정) → 기존 fw 이관 → 일괄 생성.
    /// 가능하면 Map.prefab을 프리팹 모드로 열어놓고 실행할 것 — 그래야 Game.unity와
    /// 2 SCENE.unity가 공유하는 원본 프리팹에 반영된다. 프리팹 모드가 아니면 현재 씬의
    /// PrefabInstance 오버라이드로만 저장된다는 경고를 띄운다.
    /// </summary>
    internal sealed class BuildingColliderTool : EditorWindow
    {
        private float widthRatio = 0.85f;
        private float thicknessRatio = 0.40f;
        private float minThickness = 0.7f;
        private float bottomOverlapRatio = 0.10f;
        private bool onlySelected;
        private bool overwriteExisting;

        [MenuItem("Zzinmak/건물 콜라이더 일괄 생성")]
        private static void Open()
        {
            GetWindow<BuildingColliderTool>("건물 콜라이더").minSize = new Vector2(320, 260);
        }

        /// <summary>
        /// 창을 띄우지 않고 커맨드라인에서 실행하기 위한 진입점.
        /// 예: Unity.exe -batchmode -executeMethod Zzinmak.EditorTools.BuildingColliderTool.BatchFixExistingScene -quit
        /// "2 SCENE.unity"만 열어서 측정 로그 → 없는 건물 생성 → 안 겹치는 fw 위로 올리기까지 수행하고 저장한다.
        /// </summary>
        public static void BatchFixExistingScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/2 SCENE.unity");

            var tool = ScriptableObject.CreateInstance<BuildingColliderTool>();
            tool.RunMeasure();
            tool.RunGenerate(previewOnly: false);
            tool.RunFixOverlap();
            DestroyImmediate(tool);

            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[BuildingColliderTool] 배치 완료");
        }

        private void OnGUI()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
                EditorGUILayout.HelpBox($"프리팹 모드: {stage.assetPath}", MessageType.Info);
            else
                EditorGUILayout.HelpBox(
                    "프리팹 모드가 아닙니다. Map.prefab을 더블클릭해서 프리팹 모드로 연 뒤 실행하면 " +
                    "Game.unity와 2 SCENE.unity가 함께 쓰는 원본에 반영됩니다.\n" +
                    "지금 실행하면 현재 열려 있는 씬의 오버라이드로만 저장됩니다.",
                    MessageType.Warning);

            EditorGUILayout.Space();
            widthRatio = EditorGUILayout.Slider("가로 비율 (건물 폭 대비)", widthRatio, 0.1f, 1f);
            thicknessRatio = EditorGUILayout.Slider("두께 비율 (건물 높이 대비)", thicknessRatio, 0.05f, 1f);
            minThickness = EditorGUILayout.FloatField("최소 두께 (월드 단위 근사)", minThickness);
            bottomOverlapRatio = EditorGUILayout.Slider(
                "밑변 겹침 비율 (건물 높이 대비, 스프라이트 안으로 파고드는 양)", bottomOverlapRatio, 0f, 0.5f);
            EditorGUILayout.Space();
            onlySelected = EditorGUILayout.ToggleLeft("선택한 오브젝트만 대상으로", onlySelected);
            overwriteExisting = EditorGUILayout.ToggleLeft("기존 fw 덮어쓰기", overwriteExisting);

            EditorGUILayout.Space();
            if (GUILayout.Button("1. 기존 fw 측정 (읽기 전용)"))
                RunMeasure();

            if (GUILayout.Button("2. 기존 fw를 건물 자식으로 이관"))
                RunMigrate();

            EditorGUILayout.Space();
            if (GUILayout.Button("3. 미리보기 (개수만 출력, 변경 없음)"))
                RunGenerate(previewOnly: true);

            if (GUILayout.Button("4. 건물 콜라이더 일괄 생성"))
                RunGenerate(previewOnly: false);

            EditorGUILayout.Space();
            if (GUILayout.Button("5. 기존 fw 위로 올리기 (부족한 것만)"))
                RunFixOverlap();
        }

        // ----- 모드 A: 측정 -----

        private void RunMeasure()
        {
            var roots = GetScanRoots();
            var buildings = CollectBuildings(roots);
            var fws = CollectFwObjects(roots);

            if (fws.Count == 0)
            {
                Debug.LogWarning("[BuildingColliderTool] fw 오브젝트를 찾지 못했습니다.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[BuildingColliderTool] 기존 fw 실측 결과 (이름 | 대응 건물 | widthRatio | thicknessRatio | topOffset)");
            foreach (var fw in fws)
            {
                var col = fw.GetComponent<BoxCollider2D>();
                if (col == null) continue;

                var building = FindNearestBuilding(fw, buildings);
                if (building == null)
                {
                    sb.AppendLine($"  {fw.name} | 매칭 실패");
                    continue;
                }

                Bounds fwBounds = col.bounds;
                Bounds bBounds = GetCombinedSpriteBounds(building);
                float wRatio = bBounds.size.x > 0.0001f ? fwBounds.size.x / bBounds.size.x : 0f;
                float tRatio = bBounds.size.y > 0.0001f ? fwBounds.size.y / bBounds.size.y : 0f;
                float top = fwBounds.max.y - bBounds.min.y;
                sb.AppendLine($"  {fw.name} | {building.name} | {wRatio:F3} | {tRatio:F3} | {top:F3}");
            }
            Debug.Log(sb.ToString());
        }

        // ----- 모드 B: 기존 fw를 건물 자식으로 이관 -----

        private void RunMigrate()
        {
            var roots = GetScanRoots();
            var buildings = CollectBuildings(roots);
            var fws = CollectFwObjects(roots);

            int migrated = 0, alreadyDone = 0, failed = 0;
            foreach (var fw in fws)
            {
                if (fw.parent != null)
                {
                    alreadyDone++;
                    continue;
                }

                var col = fw.GetComponent<BoxCollider2D>();
                if (col == null) continue;

                var building = FindNearestBuilding(fw, buildings);
                if (building == null)
                {
                    Debug.LogWarning($"[BuildingColliderTool] {fw.name}: 대응 건물을 찾지 못해 건너뜁니다.");
                    failed++;
                    continue;
                }

                Undo.RegisterFullObjectHierarchyUndo(fw.gameObject, "fw 이관");

                // 재부모화 전에 월드 공간 AABB를 먼저 기록해둔다.
                Vector3 worldMin = col.bounds.min;
                Vector3 worldMax = col.bounds.max;

                Undo.SetTransformParent(fw, building, "fw 이관");
                fw.localPosition = Vector3.zero;
                fw.localRotation = Quaternion.identity;
                fw.localScale = Vector3.one;

                // 트랜스폼을 정리한 뒤, 기록해둔 월드 AABB를 fw의 로컬 공간으로 역변환해서
                // 콜라이더 size/offset에 그대로 옮긴다. 이러면 월드상 위치·크기는 그대로 유지된다.
                Vector3 localMin = fw.InverseTransformPoint(worldMin);
                Vector3 localMax = fw.InverseTransformPoint(worldMax);
                col.size = new Vector2(Mathf.Abs(localMax.x - localMin.x), Mathf.Abs(localMax.y - localMin.y));
                col.offset = new Vector2((localMax.x + localMin.x) * 0.5f, (localMax.y + localMin.y) * 0.5f);
                col.isTrigger = false;

                var sr = fw.GetComponent<SpriteRenderer>();
                if (sr != null)
                    Undo.DestroyObjectImmediate(sr);

                fw.gameObject.name = "fw";
                EditorUtility.SetDirty(fw.gameObject);
                migrated++;
            }

            MarkScenesDirty(roots);
            Debug.Log($"[BuildingColliderTool] 이관 완료: {migrated}개 / 이미 자식이라 건너뜀: {alreadyDone}개 / 매칭 실패: {failed}개");
        }

        // ----- 모드 C: 일괄 생성 -----

        private void RunGenerate(bool previewOnly)
        {
            var roots = GetScanRoots();
            var buildings = CollectBuildings(roots);

            int created = 0, skipped = 0;
            foreach (var b in buildings)
            {
                var existingFw = FindFwChild(b);
                if (existingFw != null && !overwriteExisting)
                {
                    skipped++;
                    continue;
                }

                Bounds worldBounds = GetCombinedSpriteBounds(b);
                if (worldBounds.size.sqrMagnitude < 0.0001f)
                {
                    Debug.LogWarning($"[BuildingColliderTool] {b.name}: SpriteRenderer를 찾지 못해 건너뜁니다.");
                    skipped++;
                    continue;
                }

                if (previewOnly)
                {
                    created++;
                    continue;
                }

                if (existingFw != null && overwriteExisting)
                    Undo.DestroyObjectImmediate(existingFw.gameObject);

                Vector3 localMin = b.InverseTransformPoint(worldBounds.min);
                Vector3 localMax = b.InverseTransformPoint(worldBounds.max);
                float localWidth = Mathf.Abs(localMax.x - localMin.x);
                float localHeight = Mathf.Abs(localMax.y - localMin.y);
                float localCenterX = (localMax.x + localMin.x) * 0.5f;

                // minThickness는 월드 단위 기준 슬라이더값이라, 건물 자신의 스케일로 나눠서
                // 로컬 단위로 근사 환산한다(회전은 프로젝트 전역에서 쓰이지 않는다는 전제).
                float scaleY = Mathf.Max(0.0001f, Mathf.Abs(b.lossyScale.y));
                float minThicknessLocal = minThickness / scaleY;

                float sizeY = Mathf.Max(localHeight * thicknessRatio, minThicknessLocal);
                float sizeX = localWidth * widthRatio;

                // 콜라이더 윗변이 스프라이트 밑변(localMin.y)보다 조금 위로 파고들게 해서
                // 건물과 실제로 겹치게 한다. 겹침이 0이면 콜라이더 전체가 스프라이트 바깥(아래)
                // 빈 공간에 놓여 "건물 밖에 생긴" 것처럼 보인다 — 실제로 겪었던 문제.
                float bottomOverlap = localHeight * bottomOverlapRatio;
                float offsetY = localMin.y - sizeY * 0.5f + bottomOverlap;

                var go = new GameObject("fw");
                Undo.RegisterCreatedObjectUndo(go, "fw 콜라이더 생성");
                go.layer = b.gameObject.layer;
                go.tag = "Untagged";
                go.transform.SetParent(b, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = false;
                col.size = new Vector2(sizeX, sizeY);
                col.offset = new Vector2(localCenterX, offsetY);

                created++;
            }

            if (!previewOnly)
                MarkScenesDirty(roots);

            Debug.Log(previewOnly
                ? $"[BuildingColliderTool] 미리보기 — 생성 대상: {created}개, 스킵(이미 있음/스프라이트 없음): {skipped}개"
                : $"[BuildingColliderTool] 생성 완료: {created}개, 스킵: {skipped}개");
        }

        // ----- 모드 D: 이미 배치된 fw 중 안 겹치는 것만 위로 올리기 -----

        /// <summary>
        /// 예전 버그(겹침 0으로 생성)로 이미 건물 밖에 놓인 fw들을 되살린다.
        /// 새로 만들지 않고, 각 fw의 실제 월드 콜라이더 윗변이 목표 겹침선(건물 밑변 +
        /// bottomOverlapRatio*높이)보다 낮으면 부족한 만큼만 위로 올린다. X/크기는 손대지
        /// 않으므로, 이미 충분히 겹쳐 있는 것(손으로 이관한 것 포함)은 그대로 둔다.
        /// </summary>
        private void RunFixOverlap()
        {
            const float epsilon = 0.001f;
            var roots = GetScanRoots();
            var buildings = CollectBuildings(roots);

            int raised = 0, alreadyOk = 0, noFw = 0;
            foreach (var b in buildings)
            {
                var fw = FindFwChild(b);
                if (fw == null)
                {
                    noFw++;
                    continue;
                }

                var col = fw.GetComponent<BoxCollider2D>();
                if (col == null) continue;

                Bounds spriteBounds = GetCombinedSpriteBounds(b);
                float requiredOverlap = spriteBounds.size.y * bottomOverlapRatio;
                float desiredWorldTop = spriteBounds.min.y + requiredOverlap;
                float currentWorldTop = col.bounds.max.y;

                if (currentWorldTop >= desiredWorldTop - epsilon)
                {
                    alreadyOk++;
                    continue;
                }

                float deltaWorld = desiredWorldTop - currentWorldTop;
                float scaleY = Mathf.Max(0.0001f, Mathf.Abs(fw.lossyScale.y));
                float deltaLocal = deltaWorld / scaleY;

                Undo.RecordObject(col, "fw 위로 올리기");
                col.offset += new Vector2(0f, deltaLocal);
                EditorUtility.SetDirty(fw.gameObject);
                raised++;
            }

            MarkScenesDirty(roots);
            Debug.Log($"[BuildingColliderTool] 위로 올림: {raised}개 / 이미 충분히 겹침: {alreadyOk}개 / fw 없음: {noFw}개");
        }

        // ----- 공용 헬퍼 -----

        private GameObject[] GetScanRoots()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
                return new[] { stage.prefabContentsRoot };

            if (onlySelected && Selection.gameObjects.Length > 0)
                return Selection.gameObjects;

            var list = new List<GameObject>();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                list.AddRange(scene.GetRootGameObjects());
            }
            return list.ToArray();
        }

        private static void MarkScenesDirty(GameObject[] roots)
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                return; // 프리팹 모드는 EditorSceneManager.MarkSceneDirty 대상이 아니다.

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                    EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        private static List<Transform> CollectBuildings(GameObject[] roots)
        {
            var result = new List<Transform>();
            var seen = new HashSet<Transform>();
            foreach (var root in roots)
            {
                if (root == null) continue;
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!seen.Add(t)) continue;
                    if (IsBuildingName(t.name))
                        result.Add(t);
                }
            }
            return result;
        }

        private static List<Transform> CollectFwObjects(GameObject[] roots)
        {
            var result = new List<Transform>();
            var seen = new HashSet<Transform>();
            foreach (var root in roots)
            {
                if (root == null) continue;
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!seen.Add(t)) continue;
                    if (IsFwName(t.name))
                        result.Add(t);
                }
            }
            return result;
        }

        private static Transform FindFwChild(Transform building)
        {
            foreach (Transform child in building)
            {
                if (IsFwName(child.name))
                    return child;
            }
            return null;
        }

        private static bool IsBuildingName(string name)
        {
            if (!name.Contains("_건물_")) return false;
            if (name.Contains("_장식_") || name.Contains("_자동차_")) return false;
            return true;
        }

        private static bool IsFwName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            string lower = name.ToLowerInvariant();
            return lower == "fw" || lower.StartsWith("fw (") || lower.StartsWith("fw(");
        }

        private static Bounds GetCombinedSpriteBounds(Transform t)
        {
            var renderers = t.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0)
                return new Bounds(t.position, Vector3.zero);

            Bounds b = GetTightWorldBounds(renderers[0]);
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(GetTightWorldBounds(renderers[i]));
            return b;
        }

        // 스프라이트 원본(sprite.bounds/Renderer.bounds)은 텍스처 전체 사각형이라 투명 여백까지
        // 포함한다 — Photoroom류 배경제거 이미지는 여백이 커서 이 여백 기준으로 아무리 비율을
        // 조정해도 콜라이더가 건물 밖에 놓였다(실제로 겪은 문제). 그래서 픽셀 알파값을 직접 읽어
        // 불투명 영역만의 타이트한 바운드를 계산해 쓴다. 스프라이트 하나당 한 번만 계산해 캐시한다.
        private static readonly Dictionary<Sprite, Rect> tightLocalRectCache = new Dictionary<Sprite, Rect>();

        private static Bounds GetTightWorldBounds(SpriteRenderer sr)
        {
            if (sr.sprite == null)
                return sr.bounds;

            Rect local = GetTightLocalRect(sr.sprite);
            Transform t = sr.transform;
            Vector3 c0 = t.TransformPoint(new Vector3(local.xMin, local.yMin, 0));
            Vector3 c1 = t.TransformPoint(new Vector3(local.xMax, local.yMax, 0));
            Vector3 c2 = t.TransformPoint(new Vector3(local.xMin, local.yMax, 0));
            Vector3 c3 = t.TransformPoint(new Vector3(local.xMax, local.yMin, 0));

            Bounds b = new Bounds(c0, Vector3.zero);
            b.Encapsulate(c1);
            b.Encapsulate(c2);
            b.Encapsulate(c3);
            return b;
        }

        /// <summary>
        /// 스프라이트의 불투명 픽셀만의 바운딩 사각형을 (GameObject 로컬 유닛 공간으로) 계산한다.
        /// 텍스처가 Read/Write 꺼져 있으면 이번 계산 동안만 켰다가 즉시 원래대로 되돌린다.
        /// </summary>
        private static Rect GetTightLocalRect(Sprite sprite)
        {
            if (tightLocalRectCache.TryGetValue(sprite, out var cached))
                return cached;

            Rect fallback = new Rect(sprite.bounds.min, sprite.bounds.size);
            Rect result = fallback;

            Texture2D tex = sprite.texture;
            string path = tex != null ? AssetDatabase.GetAssetPath(tex) : null;
            var importer = string.IsNullOrEmpty(path) ? null : AssetImporter.GetAtPath(path) as TextureImporter;
            bool madeReadable = false;

            try
            {
                if (importer != null && !importer.isReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    madeReadable = true;
                }

                Rect texRect = sprite.rect; // 텍스처 안에서 이 스프라이트가 차지하는 픽셀 영역
                int rx = Mathf.RoundToInt(texRect.x);
                int ry = Mathf.RoundToInt(texRect.y);
                int rw = Mathf.RoundToInt(texRect.width);
                int rh = Mathf.RoundToInt(texRect.height);

                Color[] pixels = tex.GetPixels(rx, ry, rw, rh);

                const float alphaThreshold = 0.02f;
                int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
                for (int y = 0; y < rh; y++)
                {
                    int rowBase = y * rw;
                    for (int x = 0; x < rw; x++)
                    {
                        if (pixels[rowBase + x].a > alphaThreshold)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                }

                if (maxX >= minX && maxY >= minY)
                {
                    float ppu = sprite.pixelsPerUnit;
                    Vector2 pivotPx = sprite.pivot; // 스프라이트 rect 왼쪽 아래 기준 픽셀 좌표
                    float localMinX = (minX - pivotPx.x) / ppu;
                    float localMaxX = (maxX + 1 - pivotPx.x) / ppu;
                    float localMinY = (minY - pivotPx.y) / ppu;
                    float localMaxY = (maxY + 1 - pivotPx.y) / ppu;
                    result = Rect.MinMaxRect(localMinX, localMinY, localMaxX, localMaxY);
                }
                else
                {
                    Debug.LogWarning($"[BuildingColliderTool] {sprite.name}: 완전히 투명한 스프라이트라 전체 바운드로 대체합니다.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[BuildingColliderTool] {sprite.name}: 픽셀을 읽지 못해 전체 바운드로 대체합니다 ({e.Message}).");
            }
            finally
            {
                if (madeReadable && importer != null)
                {
                    importer.isReadable = false;
                    importer.SaveAndReimport();
                }
            }

            tightLocalRectCache[sprite] = result;
            return result;
        }

        /// <summary>
        /// fw 오브젝트의 월드 좌표가 어느 건물의 스프라이트 발밑에 있는지로 대응 건물을 찾는다.
        /// 건물의 가로 범위 안에 있는 후보를 우선하고, 그중 중심까지 거리가 가장 가까운 것을 고른다.
        /// </summary>
        private static Transform FindNearestBuilding(Transform fw, List<Transform> buildings)
        {
            Vector3 fwPos = fw.position;
            Transform best = null;
            float bestScore = float.MaxValue;

            foreach (var b in buildings)
            {
                Bounds wb = GetCombinedSpriteBounds(b);
                bool withinX = fwPos.x >= wb.min.x - 0.5f && fwPos.x <= wb.max.x + 0.5f;
                float dist = Vector2.Distance(fwPos, wb.center);
                float score = withinX ? dist : dist + 1000f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = b;
                }
            }
            return best;
        }
    }
}
#endif
