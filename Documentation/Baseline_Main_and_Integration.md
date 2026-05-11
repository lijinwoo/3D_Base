# `main` 기준선 및 통합 메모 (Baseline Freeze)

## 목적
전체 Phase 로드맵 실행 시 **어느 커밋이 기준선인지**와, `main`과 작업 브랜치 간 **통합 순서**를 고정합니다.

## 현재 스냅샷 (로컬 확인 시점 기준)

| 브랜치 | 역할 | 비고 |
|--------|------|------|
| `main` | 안정 릴리즈 기준선 | Phase 1~6 본 구현이 통합되기 전까지는 `Phase1_Movement` 등 작업 브랜치가 앞설 수 있음 |
| `Phase1_Movement` | Phase 1·2 및 이후 Phase 작업 통합 | Validation Scene, Combat, Animator Factory 등 포함 |
| `phase/phase-1-movement` … `phase/phase-6-*` | Phase별 통합 브랜치 | `main`에 머지하기 전 단계별 정리용 |

## Phase 1~2 보강 범위 (재구현 제외)

다음은 **이미 구현된 것으로 간주**하고, 이후 Phase는 **회귀 테스트만**으로 보강합니다.

- Phase 1: `InputProvider`, `MovementComponent`, `Phase1OrbitCameraController`, `LocomotionAnimatorDriver`, Phase 1 Validation Scene 도구
- Phase 2: `IDamageable`, `HealthComponent`, `CombatComponent`, `Attack` 입력, Phase 2 Validation Scene 도구

## 권장 통합 순서

1. `Phase1_Movement`(또는 `phase/phase-2-damage-weapon`)에서 Phase 3~6 작업 완료
2. `develop`이 있다면 `phase/*` → `develop`
3. QA 후 `main` 병합
4. 태그 `mvp-phase1` … `mvp-1.0.0` 적용 ([MVP_Roadmap_Phase_and_Release_Management.md](MVP_Roadmap_Phase_and_Release_Management.md))

## 운영 원칙

- Phase 시작 전: 본 문서와 로드맵의 **Done 기준**을 다시 확인
- Phase 종료 시: `Done` / `Known Issue` / `Out of Scope`를 로드맵 또는 Phase 전용 문서에 기록
