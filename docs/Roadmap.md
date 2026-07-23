# Roadmap

## MVP 체크리스트 (핵심 루프)

- [x] 기획 문서(GDD/Architecture/UI/Roadmap) 작성
- [x] 프로젝트 세팅: 1080x2340 해상도, 레이어/태그, Physics2D, 카메라
- [x] 플레이스홀더 스프라이트(플레이어/좀비/투사체/젬/조이스틱/카드아이콘)
- [x] Core: GameManager, GameEvents, ObjectPool
- [x] Player: PlayerController(조이스틱 이동), PlayerStats, PlayerHealth, PlayerLevel, CameraFollow
- [x] Combat: WeaponController, Weapon(소총), Projectile
- [x] Enemies: Enemy(일반 좀비), EnemySpawner(무한 난이도 상승)
- [x] Progression: ExpGem, LevelUpManager(3택 강화)
- [x] 프리팹: Player/Zombie/Projectile/ExpGem
- [x] 데이터: WeaponData/EnemyData/UpgradeData ScriptableObject
- [x] UI 하이어라키: Canvas(HUD, LevelUpPanel) 직접 배치 + 연결 스크립트(HUDController/LevelUpUI/VirtualJoystick)
- [x] Play 모드 골든패스 검증 및 밸런스 1차 조정 — 이동/자동공격/좀비스폰/처치/경험치/레벨업 카드선택/강화적용까지 실동작 확인 완료
- [x] 유한 야외 맵(40x40, 사방 벽) + 랜덤 건물 미션(입장→인터리어 좀비 N마리 전멸→인질 구출→복귀→새 건물 생성) 구현 및 전체 루프 실동작 확인

## 확정된 설계 결정

- **승리(엔딩) 조건 없음 — 무한 생존.** 난이도는 시간에 따라 끝없이 상승하며, 플레이어 사망 시에만 게임오버로 종료된다. (변경 이력: 최초 계획은 생존 타이머 완료 시 승리였으나, 사용자 지시로 무한 생존 방식으로 변경됨)
- 모든 UI 오브젝트는 스크립트 생성 없이 하이어라키에서 직접 구성.
- 아트는 플레이스홀더(단색 도형 스프라이트)로 우선 개발.
- **좀비 콜라이더는 Trigger.** 다수가 밀집할 때 Non-Trigger 강체 충돌이 만드는 물리 폭발(플레이어/좀비가 맵 밖으로 튕겨나가는 버그)을 플레이테스트 중 발견해 Trigger + `OnTriggerStay2D` 방식으로 수정함. 좀비끼리는 자유롭게 겹친다(별도 분리 로직 없음).
- **맵은 무한 타일링 → 유한 맵+랜덤 건물 미션으로 최종 변경.** 처음엔 카메라를 따라다니는 무한 타일링 바닥이었고, 그다음엔 동서남북 포털로 연결되는 무한 그리드였으나, 최종적으로 유한한 야외 맵 + 랜덤 위치에 생성되는 건물(좀비 전멸→인질 구출 미니 미션) 구조로 확정. `InfiniteGround.cs`/`Portal.cs`는 각각 제거됨.

## 알려진 이슈

- ~~한글 TMP 폰트 미지원~~ → **해결됨.** `Assets/Fonts/Dalmoori.ttf`(달무리 폰트)로 `Assets/Fonts/Dalmoori SDF.asset` TMP Font Asset을 생성(Dynamic Atlas 모드)하여 모든 TMP_Text와 TMP Settings 기본 폰트에 적용. 한글 텍스트가 정상 렌더링됨을 스크린샷으로 확인.

## 이후 확장 백로그 (MVP 이후)

- 무기/좀비 다양화, 무기 진화(조합), 시너지 강화
- 보스/특수 웨이브(예: N분마다 강화 개체 등장)
- 메타 성장(런 종료 후 영구 재화/강화), 캐릭터 선택
- 건물 내부 전용 타일맵/아트(현재는 플레이스홀더 바닥+벽), 건물 종류 다양화(상점/함정 등), 인터리어 난이도 스케일링
- 배경 데코 오브젝트(건물 미션과 별개로 지형에 랜덤 배치되는 나무/잔해 등 장식), 파쿠르 장애물
- 사운드/BGM/이펙트, 피격/사망 VFX, 화면 흔들림
- UI 확장: 메인 메뉴(타이틀), 결과창(생존시간/킬수 요약, 재시작), 설정(사운드/조작), 일시정지 메뉴
- 실제 아트로 플레이스홀더 교체
- 리더보드/최고 생존시간 기록(로컬 저장)
- 모바일 빌드 최적화(배터리/발열/프레임), 광고/수익화(필요 시)
