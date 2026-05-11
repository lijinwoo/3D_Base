# Phase 6 — 빌드 안정화 및 프로파일링 체크리스트

## CPU

- **PlayerLoop**: `Update`/`FixedUpdate` 상위 비용 스크립트 식별
- **Animator**: 레이어 수, `Cull` 모드, 불필요한 파라미터 전이
- **Physics**: Raycast 빈도, LayerMask 범위 축소

## GPU

- **URP Renderer**: draw calls, overdraw, shadow distance
- **VFX Graph / Particle**: max particles, burst 동시성

## Memory / GC

- **Managed Heap**: 스폰 루프에서 임시 할당(allocation) 제거
- **Addressables**: `Release`/`ReleaseInstance` 누락 여부
- **Object Pool**: 풀 반환 누락으로 인한 인스턴스 누적

## 빌드

- **Development Build**로 로그 수준 확인 후 **Release** 전환
- **Script Debugging** 비활성화
- 타깃 플랫폼별 입력·해상도 스모크

## Unity 에디터 메뉴

`Tools/Systemic Overload/Phase 6/Open Unity Profiler Window` — Profiler 창을 엽니다.
`Tools/Systemic Overload/Phase Validation/Build Phase 6 Final Profiling Scene` — Phase 1~6 통합 Validation Scene을 생성합니다.

## 통합 검증 범위 (Final Scene)

- **Phase 1**: Input + Movement + Orbit Camera
- **Phase 2**: HitScan Combat + Training Dummy
- **Phase 3**: GameObjectPool + EncounterDirector + EncounterSpawnData + Quest/Save SO
- **Phase 4**: NavMeshSurface + EnemyNavMeshChaser
- **Phase 5**: AddressablesWarmupUtility + AddressablesOneShotVfx
- **Phase 6**: Profiling Marker 배치 및 단일 scene 스모크 검증

## Done 기준 (Phase 6 스냅샷)

- 위 항목을 1회 이상 기록(스크린샷 또는 수치)하고, Critical 이슈 0건이면 Phase 6 스냅샷 완료로 간주합니다.
