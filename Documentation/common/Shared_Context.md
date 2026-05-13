# Shared Context

## 프로젝트 공통 원칙
- 데이터는 가능한 `ScriptableObject`로 분리해 콘텐츠 확장 비용을 낮춥니다.
- `Physics Query` 판정과 게임 로직 연산/적용은 계층을 분리합니다.
- 더미/프로토타입도 실제 확장 구조와 동일한 책임 경계를 따릅니다.

## 공통 체크리스트
- 신규 Enemy/NPC/Interactable은 Definition 에셋만으로 기초 동작이 가능한가?
- 판정 서비스(`IPhysicsQueryService`)를 우회한 직접 `Physics.*` 호출이 남아있는가?
- 런타임 상태와 원본 Definition 데이터가 분리되어 있는가?
