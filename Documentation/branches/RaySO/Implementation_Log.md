# Branch Log - RaySO

## Goal
- Dummy Enemy/Chest/NPC를 `ScriptableObject` 기반으로 전환
- `Physics Query`의 판정과 연산/비즈니스 책임 분리

## Implemented
- `EnemyDefinitionSO`, `ChestDefinitionSO`, `NpcDefinitionSO`, `InteractableDefinitionSO` 추가
- `InteractableComponent` 도입 및 Dummy Interactable Prefab 연결 전환
- `HealthComponent`에 Enemy Definition 기반 초기화 바인딩 추가
- `IPhysicsQueryService`, `TpsPhysicsQueryService`, `TpsAimComputation`, `TpsAoeComputation` 추가
- `Combat`, `Interaction`, `Melee`, `MagicSphereCast`, `GroundAoe`, `Dash`, `Movement`, `OrbitCamera`에 Query 분리 패턴 확장

## Validation Plan
- Dummy Prefab 3종의 Definition 참조 정상 로드 확인
- 공격/상호작용/대시/카메라 충돌 동작 회귀 테스트
- `Documentation/`이 git 비공유 상태인지 `git status`로 확인

## Pending
- 플레이모드에서 실제 씬 기반 기능 검증
- 필요한 경우 Query 전략(우선순위/필터 규칙) 문서 세분화
