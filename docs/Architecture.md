# Architecture — 스크립트/시스템 구조

## 원칙

- **UI 오브젝트는 스크립트가 생성하지 않는다.** Canvas 이하 모든 UI는 씬 하이어라키에 직접 배치되어 있으며, 스크립트는 `[SerializeField]`로 그 오브젝트들을 참조하여 텍스트/이미지 값만 갱신한다. (매핑은 `docs/UI.md` 참고)
- 데이터(무기/좀비/강화 수치)는 코드에 하드코딩하지 않고 **ScriptableObject**로 분리한다.
- 성능이 중요한 반복 생성 오브젝트(좀비/투사체/경험치 젬)는 **오브젝트 풀링**을 사용한다.
- 시스템 간 결합은 이벤트(`GameEvents`)를 통해 느슨하게 연결한다.

## 폴더 구조

```
Assets/
  Scripts/
    Core/        GameManager, GameEvents, ObjectPool
    Player/      PlayerController, PlayerStats, PlayerHealth, PlayerLevel, CameraFollow
    Combat/      WeaponController, Weapon, Projectile
    Enemies/     Enemy, EnemySpawner
    Progression/ ExpGem, LevelUpManager
    UI/          HUDController, LevelUpUI, VirtualJoystick
    Data/        WeaponData, EnemyData, UpgradeData (ScriptableObject 정의)
  Data/          위 SO의 실제 인스턴스 에셋 (.asset)
  Prefabs/       Player, Zombie, Projectile, ExpGem
  Art/Placeholder/  임시 스프라이트
```

## 시스템별 책임

### Core
- **GameManager** (싱글턴): 게임 상태(Playing/LevelUp/GameOver) 관리, 생존 시간 경과 카운트(기록용, 승리 판정 없음), 레벨업 시 `Time.timeScale = 0` 처리, 패배 시 결과 처리 진입점. **별도의 승리(Win) 상태는 존재하지 않으며 난이도는 무한히 상승한다.**
- **GameEvents**: 정적 이벤트 허브. `OnExpChanged`, `OnLevelUp`, `OnHealthChanged`, `OnKillCountChanged`, `OnTimerChanged`, `OnGameOver` 등. Player/Enemy가 발행, UI가 구독.
- **ObjectPool**: `GameObject` 프리팹별 풀 관리(Get/Return). 좀비 스포너/투사체/경험치 젬이 사용.

### Player
- **PlayerController**: Input System(또는 VirtualJoystick 출력)을 읽어 Rigidbody2D 이동. 바라보는 방향은 회전이 아니라 `SpriteRenderer.flipX`로 좌우 반전만 처리(상하 회전 없음) — 공격 대상이 있으면 그 방향, 없으면 이동 방향의 x부호를 따른다.
- **PlayerStats**: 런타임 스탯 컨테이너(이동속도, 최대체력, 픽업범위, 데미지배율, 공속배율). 강화 적용 시 이 값들이 수정됨.
- **PlayerHealth**: 현재체력, 피격 무적시간 처리, 0 이하 시 `GameEvents.OnGameOver` 발행.
- **PlayerLevel**: XP 누적/레벨 계산, 레벨업 임계치 도달 시 `LevelUpManager.TriggerLevelUp()` 호출.
- **CameraFollow**: 메인 카메라가 플레이어를 따라가도록 처리(단순 Lerp).
- **걷기 애니메이션**: `Assets/character/Player/walk_1~8.png` 8프레임을 `Assets/Animations/PlayerWalk.anim`(10fps, 루프)으로 만들고 `Assets/Animations/PlayerAnimator.controller`(단일 Walk 스테이트)로 재생. `PlayerController`가 이동 입력이 있을 때만 `Animator.speed=1`, 정지 시 `0`으로 설정해 멈추면 걷기 동작도 함께 멈춘다.

### Combat
- **WeaponController**: 플레이어에 부착. 보유 무기 리스트를 순회하며 각 무기의 발사 타이머 갱신.
- **Weapon**: `WeaponData` 참조, 타겟팅(가장 가까운 좀비 탐색), 발사 시 `ObjectPool`에서 Projectile을 꺼내 발사.
- **Projectile**: 지정된 방향/타겟으로 이동, 좀비와 충돌 시 데미지 적용 후 풀 반환.

### Enemies
- **Enemy**: `EnemyData` 기반 체력/속도, 플레이어를 향해 이동, 접촉 시 주기적 데미지, 사망 시 ExpGem 드롭 + `GameEvents.OnKillCountChanged`. 좀비 콜라이더는 **Trigger**로 설정(`OnTriggerStay2D`로 접촉 데미지 판정)한다 — 다수의 좀비가 밀집할 때 Non-Trigger 강체 충돌로 인한 물리 폭발(넉백 폭주)을 방지하기 위함.
- **EnemySpawner**: 화면 밖 원형 반경에서 주기적으로 풀에서 좀비를 꺼내 배치. 경과 시간에 따라 스폰 간격 감소 및 좀비 체력 배율 증가(AnimationCurve 또는 간단 수식).

### Progression
- **ExpGem**: 플레이어의 픽업범위 진입 시 플레이어 방향으로 이동(자석 효과) 후 흡수 시 XP 지급.
- **LevelUpManager**: `UpgradeData` 목록에서 무작위 3종 추첨(중복 방지), `LevelUpUI`에 전달하여 표시. 사용자가 카드를 선택하면 해당 강화 효과를 `PlayerStats`/`WeaponController`에 적용하고 게임 재개.

### UI (연결 전용, 생성 없음)
- **HUDController**: `GameEvents` 구독 → 체력바/경험치바 `Image.fillAmount`, 타이머/킬카운트 `TMP_Text.text` 갱신.
- **LevelUpUI**: `LevelUpManager`가 넘긴 강화 후보 3개를 카드(아이콘/이름/설명)에 반영, 버튼 클릭 시 콜백으로 선택 결과 전달.
- **VirtualJoystick**: `IDragHandler`/`IPointerUpHandler` 구현, Handle 오브젝트 위치를 드래그에 따라 제한 이동시키고 정규화된 방향 벡터를 `PlayerController`가 읽을 수 있도록 프로퍼티로 노출.

## 데이터 흐름 예시 (좀비 처치 → 레벨업)

1. `Projectile`이 `Enemy.TakeDamage()` 호출
2. 체력 0 → `Enemy`가 `ExpGem`을 스폰(풀에서 Get) 후 자신은 풀로 반환, `GameEvents.RaiseKillCountChanged()`
3. 플레이어가 젬 픽업범위 진입 → `ExpGem`이 플레이어에게 흡수 → `PlayerLevel.AddExp()`
4. 임계치 도달 → `PlayerLevel`이 `LevelUpManager.TriggerLevelUp()` 호출 → `GameManager` 상태를 LevelUp으로 전환(`Time.timeScale = 0`)
5. `LevelUpManager`가 후보 3종 추첨 → `LevelUpUI.Show(candidates)` (하이어라키의 `LevelUpPanel` 활성화)
6. 사용자가 카드 선택 → `LevelUpUI` → `LevelUpManager.ApplyUpgrade()` → `PlayerStats`/`WeaponController` 갱신 → 패널 비활성화, `GameManager` 상태를 Playing으로 복귀

## 환경(맵) — 유한한 야외 맵 + 랜덤 건물 미션

**설계 변경 이력**: 최초엔 동서남북 포털로 연결되는 무한 그리드 구조였으나, 사용자 지시로 폐기하고 **"랜덤 위치에 건물이 하나 생성되고, 그 건물에 들어가 좀비를 전멸시킨 뒤 인질을 구출하면 다시 밖으로 나오는"** 미션 루프로 교체했다. `Portal.cs`와 4방향 포털 오브젝트는 삭제됨.

### 야외 맵 — MapRoom (씬 루트)
- 유한한 크기(40x40)의 맵. 경계는 `North/South/East/WestWall`(`BoxCollider2D`, Non-Trigger, Rigidbody 없음=Static)로 막혀 있어 플레이어가 밖으로 나갈 수 없다.
- **Ground**: `SpriteRenderer`(Tiled 모드, 40x40, sortingOrder=-100)로 `Assets/tile/O5NY920.jpg`(심리스 반복 타일)를 깔아 바닥 표현.

### 건물 미션 — MissionManager (씬 루트)
게임 시작 시 야외 맵의 랜덤 위치에 건물 하나를 생성하고, 인질 구출을 완료하면 그 건물을 지우고 다른 랜덤 위치에 새 건물을 생성하는 것을 반복한다. 실제 "건물 내부"는 씬 안의 고정된 오프셋 위치(`InteriorRoom`, 원점에서 (300,0) 떨어진 20x20 벽으로 막힌 룸)를 계속 재사용한다.

- **BuildingSite.cs**: `Building` 프리팹(건물 스프라이트 + Trigger)에 부착. 플레이어가 닿으면 `MissionManager.Instance.EnterBuilding()` 호출.
- **MissionManager.cs**(`Assets/Scripts/Environment/`, 싱글턴):
  - `Start()`에서 야외 맵 랜덤 위치(플레이어와 최소 거리 확보, 최대 10회 재시도)에 `Building` 프리팹을 스폰.
  - `EnterBuilding()`: 야외 `EnemySpawner` 비활성화 + 현재 활성 좀비 전부 풀로 반환(야외를 "정지") → 플레이어를 `InteriorRoom`의 `EntryPoint`로 순간이동 → `HostageSpot`에 인질(`Hostage` 프리팹) 활성화/배치 → 인터리어 중심 주변 랜덤 위치에 좀비 N마리(`zombieCountPerBuilding`, 기본 5) 스폰.
  - `Update()`: 인터리어 좀비 리스트에서 죽거나 비활성화된 항목을 제거, 0마리가 되면 `interiorCleared = true`.
  - `TryRescue()`(인질이 호출): `interiorCleared`가 true일 때만 `ExitBuilding()` 실행 — 인질 비활성화, 플레이어를 야외 원래 위치로 복귀, 야외 스포너 재활성화, 기존 건물 파괴 후 새 건물을 다른 랜덤 위치에 스폰.
- **Hostage.cs**: 인질 프리팹에 부착된 Trigger. 플레이어가 닿으면 `MissionManager.Instance.TryRescue()` 호출(좀비가 남아있으면 아무 일도 일어나지 않음).
- 인터리어 좀비는 야외와 동일한 `Zombie` 프리팹/`Enemy_Zombie` 데이터를 재사용하되 체력 배율은 1로 고정(시간 경과에 따른 야외 난이도 스케일링과 무관한 고정 난이도 미니 전투).
- 야외 좀비는 항상 플레이어를 추적하므로(스폰도 플레이어 기준 반경) 건물에서 나오면 스포너가 재활성화되며 자연스럽게 새 위치 기준으로 다시 스폰된다.

## 오브젝트 풀링 대상

- Zombie(일반/러너), Projectile, ExpGem — 스폰/소멸이 빈번하여 `Instantiate/Destroy` 대신 풀 사용.
