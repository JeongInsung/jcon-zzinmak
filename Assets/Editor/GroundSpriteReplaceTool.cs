#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Zzinmak.EditorTools
{
    /// <summary>
    /// Map.prefab의 "바닥"(땅) 그룹 안 모든 타일의 스프라이트를 Assets/Map/바닥/땅.png의
    /// 스프라이트로 일괄 교체한다.
    ///
    /// 이 텍스처(1254x1254px)는 기존 dirt 타일(320x320px)보다 훨씬 커서, 스프라이트만 바꾸면
    /// 각 타일이 화면에서 갑자기 4배 가까이 커져 서로 심하게 겹친다. 그래서 교체와 동시에 각
    /// 타일의 localScale을 "새 스프라이트 기준으로 화면상 크기가 지금과 똑같이 유지"되도록
    /// 비율 계산해서 같이 조정한다(사용자 확인: 기존 타일 크기에 맞춰 축소하는 쪽을 선택함).
    /// 비율은 (기존 스프라이트 월드 크기) / (새 스프라이트 월드 크기)로 계산하므로, 이미 스케일이
    /// 조금씩 다른 타일(예: 24장이 1.0226562였던 것)도 그 차이를 그대로 유지한 채 축소된다.
    ///
    /// "바닥" 그룹이 없으면(MapHierarchyOrganizerTool을 아직 안 돌렸으면) dirt*로 시작하는
    /// 텍스처를 쓰는 모든 SpriteRenderer를 대신 찾는다.
    ///
    /// 반드시 Map.prefab을 프리팹 모드로 연 상태에서 실행할 것.
    /// </summary>
    internal static class GroundSpriteReplaceTool
    {
        private const string GroundGroupName = "바닥";
        private const string NewSpritePath = "Assets/Map/바닥/땅.png";

        [MenuItem("Zzinmak/바닥 스프라이트를 땅.png로 교체")]
        private static void Replace()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null)
            {
                Debug.LogError("[GroundSpriteReplaceTool] Map.prefab을 더블클릭해서 프리팹 모드로 연 뒤 실행하세요.");
                return;
            }

            Sprite newSprite = LoadSingleSprite(NewSpritePath);
            if (newSprite == null)
            {
                Debug.LogError($"[GroundSpriteReplaceTool] '{NewSpritePath}'에서 스프라이트를 찾지 못했습니다.");
                return;
            }

            float newW = newSprite.rect.width / newSprite.pixelsPerUnit;
            float newH = newSprite.rect.height / newSprite.pixelsPerUnit;

            Transform mapRoot = stage.prefabContentsRoot.transform;
            List<SpriteRenderer> targets = CollectTargets(mapRoot);

            if (targets.Count == 0)
            {
                Debug.LogWarning("[GroundSpriteReplaceTool] 대상 바닥 타일을 찾지 못했습니다.");
                return;
            }

            int changed = 0;
            foreach (var sr in targets)
            {
                if (sr.sprite == null) continue;

                float oldW = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
                float oldH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
                float ratioX = oldW / newW;
                float ratioY = oldH / newH;

                Undo.RecordObject(sr, "바닥 스프라이트 교체");
                Undo.RecordObject(sr.transform, "바닥 스프라이트 교체");

                sr.sprite = newSprite;
                Vector3 scale = sr.transform.localScale;
                sr.transform.localScale = new Vector3(scale.x * ratioX, scale.y * ratioY, scale.z);
                changed++;
            }

            EditorSceneManager.MarkSceneDirty(stage.scene);
            Debug.Log($"[GroundSpriteReplaceTool] {changed}개 타일의 스프라이트를 '땅.png'로 교체하고 화면상 크기를 그대로 유지하도록 스케일을 조정했습니다. 프리팹 모드에서 Ctrl+S로 저장하세요.");
        }

        private static List<SpriteRenderer> CollectTargets(Transform mapRoot)
        {
            var result = new List<SpriteRenderer>();
            Transform groundGroup = mapRoot.Find(GroundGroupName);
            if (groundGroup != null)
            {
                foreach (Transform child in groundGroup)
                {
                    var sr = child.GetComponent<SpriteRenderer>();
                    if (sr != null) result.Add(sr);
                }
                return result;
            }

            foreach (var sr in mapRoot.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr.sprite != null && sr.sprite.texture != null && sr.sprite.texture.name.StartsWith("dirt"))
                    result.Add(sr);
            }
            return result;
        }

        private static Sprite LoadSingleSprite(string path)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is Sprite sprite) return sprite;
            return null;
        }
    }
}
#endif
