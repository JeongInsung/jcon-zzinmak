# 최고 기록 TOP5

## 작업 일자
2026-08-11

## 배경
메인 메뉴에는 이미 `BestRecordButton`(타이틀 하단, "최고 기록 mm:ss")을 눌러 여는 `BestRecordPopup`이 있었지만, `게임오버결과창.md`의 TODO에 남아있던 대로 **1위 기록 하나만** 보여줬다(`BestRecordTime` 텍스트 하나). 이번 작업으로 팝업을 TOP5 리스트로 확장했다. 이 버튼/팝업 자체는 기존 `메인메뉴.md` 문서에는 반영되지 않은 채 이미 씬에 구현돼 있었음 — 문서 드리프트였다.

## 데이터 변경
- **신규**: `Assets/Scripts/Core/SurvivalRecords.cs` — TOP5 저장/조회를 전담하는 정적 헬퍼.
  - `GetAll()`: `PlayerPrefs`에서 5개 기록을 배열로(1위→5위 순) 읽어온다.
  - `TrySubmit(time)`: 정렬된 삽입으로 새 기록을 끼워넣고(더 낮은 순위는 한 칸씩 밀려나며 5위 밖은 버려짐) 1위 갱신 여부를 반환한다.
  - `Format(time)`: `mm:ss` 포맷, 기록 없으면 `--:--`.
  - **키 호환성**: 1위는 기존 `"BestSurvivalTime"` 키를 그대로 재사용 — 기존 세이브의 단일 최고 기록이 마이그레이션 없이 자동으로 TOP5의 1위 자리에 들어간다. 2~5위는 `"BestSurvivalTime1"`~`"BestSurvivalTime4"` 신규 키(기본값 0 = 미기록).
- **`Assets/Scripts/UI/GameOverController.cs`** — 기존에 `PlayerPrefs.GetFloat/SetFloat("BestSurvivalTime", ...)`를 직접 하던 것을 `SurvivalRecords.TrySubmit`/`GetAll`/`Format` 호출로 교체. "NEW!" 표기 조건(1위 갱신 여부)은 동일하게 유지.
- **`Assets/Scripts/UI/MainMenuInfo.cs`** — `bestRecordPopupText`(단일 `TMP_Text`) 필드를 `bestRecordRows`(`TMP_Text[]`, 1위~5위 순서) 배열로 교체. `OnEnable`에서 `SurvivalRecords.GetAll()`로 5개 값을 읽어 각 행에 `"N위  mm:ss"`(미기록은 `"N위  --:--"`) 형식으로 채운다.

## Hierarchy 변경 (`Assets/Scenes/2 SCENE.unity`)
기존 팝업 구조는 그대로 두고 `BestRecordTime` 한 줄만 5줄 리스트로 교체:

```
Canvas
├── BestRecordButton                  (기존, 변경 없음)
├── BestRecordPopup                   (기존, 기본 비활성)
│   └── BestRecordCard
│       ├── BestRecordTitle           (기존, "최고 생존 기록")
│       ├── RecordRows                (신규 — VerticalLayoutGroup, 기존 BestRecordTime 자리를 대체)
│       │   ├── BestRecordRow1        (TMP, "1위  --:--", fontSize 34 Bold)
│       │   ├── BestRecordRow2        (TMP, "2위  --:--", fontSize 28)
│       │   ├── BestRecordRow3        (TMP, "3위  --:--", fontSize 28)
│       │   ├── BestRecordRow4        (TMP, "4위  --:--", fontSize 28)
│       │   └── BestRecordRow5        (TMP, "5위  --:--", fontSize 28)
│       └── BestRecordCloseButton     (기존, 변경 없음)
```

- `RecordRows`는 이전 `BestRecordTime`과 같은 자리(anchoredPosition 0,10 / sizeDelta 500x200)를 차지해 위(제목)·아래(닫기 버튼)와의 간격이 그대로 유지된다.
- `VerticalLayoutGroup`: spacing 5, childAlignment MiddleCenter, childControlWidth 1(폭은 부모에 맞춤) / childControlHeight 0(높이는 각 행이 직접 지정, 36) — `MenuButtons`에 이미 쓰이던 것과 동일한 설정 패턴.
- 5개 행 모두 `MainMenuInfo.bestRecordRows` 배열에 순서대로(1위→5위) 연결됨.
- 색상/스타일은 변경 최소화를 위해 흰색 유지, 1위만 Bold+약간 큰 폰트(34)로 구분. 골드 강조색 등은 적용하지 않음 — 필요하면 후속 작업.

## 확인 필요 (Unity 미실행 상태로 작업함)
- 이 세션은 Unity 에디터를 직접 실행할 수 없어 씬 파일을 텍스트로 직접 편집했다. **Unity에서 `2 SCENE`을 열어 다음을 반드시 육안 확인할 것**:
  - `BestRecordButton` → 팝업 → 5줄이 카드 안에 올바르게 배치되는지(겹침/잘림 없는지)
  - `VerticalLayoutGroup`이 즉시 반영되는지(레이아웃 리빌드가 자동으로 일어나야 하는데, 혹시 안 되면 해당 GameObject를 클릭하거나 씬을 한 번 저장/리로드)
  - 실제 플레이 후 게임오버 → 메인으로 복귀 시 TOP5가 올바른 순서로 갱신되는지

## 손대지 않은 것
- 팝업을 여닫는 로직(`UIPanelToggle`), `BestRecordButton`/`BestRecordCloseButton`의 onClick 배선 — 기존 그대로.
- 골드 그라디언트 등 순위별 추가 스타일링.
- 기록 리셋(디버그용) 기능.

## 관련 문서
[[게임오버결과창]] — "리더보드(다중 기록 저장)" TODO 항목이 이번 작업으로 해소됨.
[[메인메뉴]] — `BestRecordButton`/`BestRecordPopup`이 이 문서에는 반영되지 않은 채 이미 씬에 있었음(문서 드리프트, 별도 정리 필요).
