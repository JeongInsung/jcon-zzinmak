# UI — 하이어라키 구성 가이드

**원칙: 모든 UI 오브젝트는 스크립트가 아니라 씬 하이어라키에서 직접 생성/배치한다.** 아래 스크립트들은 하이어라키에 이미 존재하는 오브젝트를 `[SerializeField]`로 참조하여 값만 갱신하거나 입력만 받는다.

## Canvas 설정

- `Canvas` (Screen Space - Overlay)
  - `CanvasScaler`: UI Scale Mode = **Scale With Screen Size**, Reference Resolution = **1080 x 2340**, Match = 0.5
  - `GraphicRaycaster` 기본

## ① HUD (`Canvas/HUD`)

| 오브젝트 경로 | 컴포넌트 | 역할 | 참조 스크립트 |
|---|---|---|---|
| `Canvas/HUD/TopBar/ExpBar` | Image (Filled, Horizontal) | 경험치 진행률 | `HUDController.expBarFill` |
| `Canvas/HUD/TopBar/LevelText` | TMP_Text | 현재 레벨 표시 | `HUDController.levelText` |
| `Canvas/HUD/TopBar/TimerText` | TMP_Text | 생존 경과 시간(기록용, mm:ss) | `HUDController.timerText` |
| `Canvas/HUD/TopBar/ScoreText` | TMP_Text | 점수(좀비 1마리당 10점) | `HUDController.scoreText` |
| `Canvas/HUD/PlayerHealthBar` | Image (Filled, Horizontal) | 플레이어 체력 | `HUDController.healthBarFill` |
| `Canvas/HUD/JoystickTouchArea` | Image(투명, 화면 전체 스트레치, raycastTarget=true) | **플로팅 조이스틱 터치 감지 영역** | `VirtualJoystick` 부착 |
| `Canvas/HUD/Joystick` | Image(Background, 기본 비활성) + 자식 Image(Handle) | 조이스틱 비주얼(터치 시 해당 위치로 이동해 나타남) | `VirtualJoystick.background` / `.handle` 참조 대상 |

- `HUDController`는 씬의 `HUD` 루트 오브젝트에 부착하고 위 5개 필드를 인스펙터에서 드래그 연결.
- **조이스틱은 플로팅 방식.** `VirtualJoystick`은 화면 전체를 덮는 `JoystickTouchArea`(투명 Image)에 부착되어 터치를 감지하고, `background`/`handle` 필드로 `Joystick`/`Handle` 비주얼 오브젝트를 참조한다. 터치 시(`OnPointerDown`) `Joystick`이 터치 지점으로 이동하며 활성화되고, 손을 떼면(`OnPointerUp`) 다시 비활성화되어 사라진다. `Joystick`/`Handle`의 Image는 `raycastTarget=false`(터치 감지는 `JoystickTouchArea`가 전담). `PlayerController`는 `VirtualJoystick.Direction` 프로퍼티를 참조.

## ② 레벨업 카드 선택창 (`Canvas/LevelUpPanel`, 기본 비활성)

| 오브젝트 경로 | 컴포넌트 | 역할 |
|---|---|---|
| `Canvas/LevelUpPanel` | Image (Dim 배경, 반투명 검정) | 패널 루트, 평소 `SetActive(false)` |
| `Canvas/LevelUpPanel/Title` | TMP_Text | "레벨 업!" |
| `Canvas/LevelUpPanel/Cards/Card1` `Card2` `Card3` | Button + 자식(Icon: Image, NameText: TMP, DescText: TMP) | 강화 후보 카드 |

- `LevelUpUI`는 `LevelUpPanel` 루트에 부착. Card1~3 각각의 `Button`, `Icon`, `NameText`, `DescText` 참조를 배열/개별 필드로 인스펙터 연결.
- 카드 클릭 시 `LevelUpUI`가 인덱스를 `LevelUpManager.SelectUpgrade(index)`로 전달 → 패널 비활성화 + 게임 재개.

## ③ 게임오버 결과창 (`Canvas/GameOverPanel`, 기본 비활성)

상세 기획은 `Docs/기획서/게임오버결과창.md` 참고.

| 오브젝트 경로 | 컴포넌트 | 역할 |
|---|---|---|
| `Canvas/GameOverPanel` | Image (Dim 배경, 반투명 검정 a=0.75) | 패널 루트, 평소 `SetActive(false)` |
| `Canvas/GameOverPanel/Panel` | Image (카드 배경, 800x900) | 결과 카드 |
| `.../Panel/Title` | TMP_Text | "게임 오버" |
| `.../Panel/SurvivalTimeText` | TMP_Text | 생존 시간(mm:ss) |
| `.../Panel/KillCountText` | TMP_Text | 처치 수 |
| `.../Panel/BestTimeText` | TMP_Text | 최고 기록(mm:ss, 갱신 시 NEW!) |
| `.../Panel/RestartButton` | Button + Label(TMP) | 다시 시작 |
| `.../Panel/MainMenuButton` | Button + Label(TMP) | 메인으로 |

- `GameOverController`는 **패널이 아니라 `Canvas` 루트**에 부착한다(패널은 평소 비활성이라 그 위에 붙이면 `OnGameOver` 구독이 걸리지 않기 때문). `gameOverPanel`/`survivalTimeText`/`killCountText`/`bestTimeText` 필드를 인스펙터에서 드래그 연결.
- `RestartButton`/`MainMenuButton`의 `Button.onClick`에 각각 `GameOverController.Restart`/`.ReturnToMainMenu`를 퍼시스턴트 리스너로 연결.

## 이번 범위 제외 (Roadmap 참고)

- ④ 메인 메뉴(타이틀) 화면은 별도 씬(`2 SCENE.unity`)에 이미 구현됨(`Docs/기획서/메인메뉴.md` 참고)
- 일시정지 메뉴

## 하이어라키 편집 시 유의사항

- 모든 위치/크기는 **RectTransform 앵커 기반**으로 설정(세로 1080x2340 다양한 기기 대응).
- 새 UI 오브젝트 추가 시 이 문서의 표에 경로/역할/연결 스크립트를 함께 갱신한다.
- **Image의 Type=Filled(체력바/경험치바 등)는 반드시 Sprite를 지정해야 한다.** Sprite가 `None`이면 Unity가 `fillAmount`를 완전히 무시하고 항상 꽉 찬 사각형으로 렌더링한다(에디터에서도 동일). `ExpBar`/`PlayerHealthBar`는 단색 흰 사각형 스프라이트(`Assets/Art/Placeholder/ui_bar_fill.png`)를 지정해 두었으니, 색상만 `Image.Color`로 조정하면 된다 — Sprite 필드를 비우지 말 것.
