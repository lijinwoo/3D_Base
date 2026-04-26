# MVP Roadmap, Phase and Release Management

## 1. 목적
이 문서는 `Systemic Overload` MVP를 **큰 완성 단위(Phase)**로 관리하기 위한 실행 기준입니다.  
각 Phase의 기술 목표, 완료 기준, 브랜치 정책, QA 게이트를 정의합니다.

## 1.1 Phase Validation Scene 정책
- 각 Phase는 기능 검증 전용 Scene을 반드시 1개 이상 유지합니다.
- Validation Scene은 해당 Phase의 핵심 산출물을 단독 검증할 수 있어야 합니다.
- Validation Scene 경로 규칙:
  - `Assets/01.Scenes/PhaseValidation/Phase_0N_<Feature>Validation.unity`
- Phase 1 표준 Scene:
  - `Assets/01.Scenes/PhaseValidation/Phase_01_MovementValidation.unity`
- Scene 생성/정렬은 Unity Editor Tool 메뉴를 사용합니다.
  - `Tools/Systemic Overload/Phase Validation/Build Phase 1 Movement Scene`

## 2. Branch Strategy

## 2.1 Long-lived Branch
- `main`
  - 안정 릴리즈 기준선
- `develop`
  - 다음 MVP 통합 기준선

## 2.2 Phase Integration Branch
- `phase/phase-1-movement`
- `phase/phase-2-damage-weapon`
- `phase/phase-3-pool-so-combat`
- `phase/phase-4-ai-navmesh-behavior`
- `phase/phase-5-addressables-vfx`
- `phase/phase-6-build-profiling`

## 2.3 Working Branch Convention
- 기능 개발: `feature/<phase>-<topic>`
  - 예: `feature/phase1-player-rotation`
- 수정/버그: `fix/<phase>-<topic>`
  - 예: `fix/phase3-projectile-pool-reset`

## 2.4 Merge Rule
1. 작업 브랜치 -> 해당 `phase/*`로 PR
2. Phase 목표 충족 시 `phase/*` -> `develop`
3. 주요 마일스톤 태그 생성
4. MVP 종료 시 `release/mvp-1.0.0` 생성 후 QA
5. QA 통과 시 `main` 병합 + 최종 태그

## 3. Tag and Milestone Rule
- Phase 완료 태그:
  - `mvp-phase1`
  - `mvp-phase2`
  - `mvp-phase3`
  - `mvp-phase4`
  - `mvp-phase5`
  - `mvp-phase6`
- 최종 태그:
  - `mvp-1.0.0`

## 4. MVP Development Roadmap

## 4.1 Phase 1 - Movement Foundation
- 목표:
  - 캐릭터 기초 이동
  - 3D 수학 기반 회전 시스템
  - 마우스 기반 카메라/캐릭터 동기 조작 체계 구축
  - Spring Arm 충돌 보정 및 Zoom/FPP 전환
- 산출물:
  - `InputProvider` + `MovementComponent` 기본 루프
  - `Phase1OrbitCameraController`(LMB Free Look, RMB Sync Rotate, LMB+RMB Forward, Wheel Zoom)
  - Spring Arm Collision(Raycast/SphereCast 기반 거리 Override)
  - Auto-Follow(Always/MovingOnly/Manual)
  - Validation Scene: `Phase_01_MovementValidation.unity`
- Done 기준:
  - 이동/회전이 안정적으로 동작
  - 기본 카메라 추적에서 조작 불편 이슈 없음
  - 마우스 입력 매핑 4종이 충돌 없이 동작
  - Zoom 0 구간에서 FPP 전환(캐릭터 렌더링 비활성화) 확인
  - 환경 전환(수면 이하) 시 Post-Process/VFX 트리거 검증

## 4.2 Phase 2 - Damage and Weapon Base
- 목표:
  - `IDamageable` 기반 Damage 시스템
  - 기본 무기 발사 로직
- 산출물:
  - `HealthComponent`, `CombatComponent` 기본 동작
  - Validation Scene: `Phase_02_DamageWeaponValidation.unity`
- Done 기준:
  - Player/Enemy 공통 Damage 계약 적용
  - 기본 무기 1종으로 전투 루프 성립

## 4.3 Phase 3 - Combat Upgrade with Pool and SO
- 목표:
  - `Object Pool` 도입
  - `ScriptableObject` 기반 데이터 구조 정착
- 산출물:
  - Projectile/VFX Pool
  - `StatData`, `WeaponData`, `WaveData`
  - Validation Scene: `Phase_03_CombatDataValidation.unity`
- Done 기준:
  - 연사 전투 중 GC Spike 현저히 감소
  - 밸런스 수정이 데이터 에셋 변경만으로 가능

## 4.4 Phase 4 - Enemy AI with NavMesh and Behavior
- 목표:
  - `NavMesh` + `Behavior` 연동 AI
- 산출물:
  - 추격/공격/순찰 전환 로직
  - Blackboard 기반 상태 관리
  - Validation Scene: `Phase_04_AINavigationValidation.unity`
- Done 기준:
  - Enemy가 장애물 환경에서 목표 추적 가능
  - 상태 전이가 논리적으로 안정적

## 4.5 Phase 5 - Addressables and VFX
- 목표:
  - `Addressables` 로드/해제
  - `VFX Graph` 기반 연출 반영
- 산출물:
  - Stage/Enemy/VFX 비동기 로딩 정책
  - Camera Feedback(`Cinemachine Impulse`)
  - Validation Scene: `Phase_05_AddressablesVFXValidation.unity`
- Done 기준:
  - Stage 전환 시 메모리 누수 없이 핸들 정리
  - 전투 피드백 품질 기준 충족

## 4.6 Phase 6 - Build, Profiling, Optimization
- 목표:
  - 최종 빌드 안정화
  - 프로파일링 기반 병목 제거
- 산출물:
  - 주요 병목 리포트
  - 품질 설정/최적화 결과
  - Validation Scene: `Phase_06_FinalProfilingValidation.unity`
- Done 기준:
  - 목표 플랫폼에서 프레임/메모리 기준 충족
  - 빌드 실패/치명 버그 없이 MVP 플레이 가능

## 5. QA Gate by Phase
- 기능 동작 확인(Functional)
- 회귀 테스트(Regression)
- 성능 검증(Performance)
- 데이터 무결성(Data Integrity)
- 릴리즈 노트 갱신(Release Notes)
- 해당 Phase Validation Scene에서 `Smoke Test`와 `Regression Test`를 통과해야 병합 가능합니다.

## 5.1 Validation Scene 최소 검증 규칙
- Smoke Test:
  - 해당 Phase 핵심 입력/상호작용이 최소 1회 이상 정상 동작해야 합니다.
- Regression Test:
  - 직전 Phase의 핵심 기능이 깨지지 않았는지 확인해야 합니다.
- Build Settings:
  - 현재 개발 중인 Phase Validation Scene은 Build Settings에 활성 상태로 등록합니다.

## 6. Profiling and Optimization Checklist

## 6.1 CPU
- `Update` 과다 호출 여부 확인
- AI 의사결정 주기 최적화
- 물리 쿼리(특히 Raycast) 빈도 관리

## 6.2 GPU
- `VFX Graph` 입자량/오버드로우 측정
- 포스트 프로세싱 비용 확인
- 카메라 이펙트 누적 비용 점검

## 6.3 Memory
- Addressable 핸들 누수 여부
- Pool Capacity 적정성
- Scene 전환 후 잔존 오브젝트 확인

## 6.4 GC
- 프레임당 Allocation 추적
- 문자열/임시 컬렉션 생성 감축
- 전투 피크 상황에서 GC Spike 빈도 검증

## 7. Release Cadence
- 각 Phase 종료 시:
  - 태그 생성
  - 변경점 요약
  - Known Issue 업데이트
- 최종 MVP 릴리즈 시:
  - `release/mvp-1.0.0` QA 완료
  - `main` 병합 후 `mvp-1.0.0` 태그

## 8. Phase 2~6 Scene 템플릿 규칙
- 네이밍:
  - `Phase_0N_<Feature>Validation.unity`
- 위치:
  - `Assets/01.Scenes/PhaseValidation/`
- 공통 구성 최소 기준:
  - `Ground`
  - `Main Camera`
  - 해당 Phase 핵심 시스템 오브젝트(Player/Enemy/Spawner 등)
- 운영 규칙:
  - Phase 완료 전 Validation Scene 검증 로그를 남겨야 합니다.
  - 통합 브랜치(`phase/*`) 병합 전 Validation Scene 통과 여부를 PR에 명시해야 합니다.
