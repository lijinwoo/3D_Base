# Technical Design Specification
## Project: Systemic Overload

## 1. Project Overview

### 1.1 Genre and Core Experience
- Genre: 3D Top-down Action / Hybrid Single-player RPG
- Core Experience: 탐험·퀘스트·성장(영속 진행)과 전투를 결합한 루프 — `Skill`, `Positioning`, `Cooldown`으로 인카운터를 처리하고 보상으로 캐릭터를 강화
- Session Goal: 세이브 기반 캠페인 진행(챕터/목표 단위)과 월드 상태 유지

### 1.2 Design Philosophy
- `Decoupling`
  - 직접 참조를 최소화하고 `Interface`와 Event 중심으로 시스템을 연결합니다.
  - 시스템 교체 비용(예: 무기 시스템 확장, AI 교체)을 낮춥니다.
- `Scalability`
  - 기획 데이터는 `ScriptableObject`로 분리해 코드 수정 없이 콘텐츠를 확장합니다.
  - Enemy/Weapon/EncounterSpawn 추가 시 재컴파일 없이 데이터만 추가 가능해야 합니다.
- `Performance`
  - `Object Pooling`으로 `Instantiate/Destroy`를 억제합니다.
  - `Addressables`로 필요한 순간에만 로드하고 즉시 해제해 메모리 피크를 관리합니다.

### 1.3 Technical Scope (MVP)
- 포함:
  - Player 이동/회전, 기본 전투, Enemy 추적/공격, 인카운터(풀 기반 스폰) + 최소 퀘스트/세이브(JSON)
  - `ScriptableObject` 기반 Stat/Weapon/EncounterSpawn/QuestDefinition 데이터
  - `WorldStateService` / `QuestService` / `SaveLoadService` 기반 RPG 진행
  - `Addressables` 기반 리소스 로드
  - `VFX Graph`/`Shuriken`/`Cinemachine` 기반 피드백
- 제외:
  - 멀티플레이, 서비스형 라이브 운영, 복잡한 스킬 트리 전체(단계적 확장)

## 2. System Architecture Summary

### 2.1 Composition First
거대 상속 계층 대신 독립적인 `Component`를 조합합니다.

| Component | Responsibility | 주요 입력 | 주요 출력 |
|---|---|---|---|
| `HealthComponent` | HP, Damage 처리, 사망 상태 전환 | `IDamageable.ApplyDamage()` | `Damaged`, `Died` Event |
| `MovementComponent` | 이동/가속/감속/회전 보간 | 이동 벡터, 커서 월드 위치 | 현재 이동 상태, 회전 결과 |
| `CombatComponent` | 공격 실행, 쿨타임/연사 제어 | 공격 입력, WeaponData | Projectile Spawn, Hit 처리 |
| `InputProvider` | `Input System` 입력 수집/정규화 | Player Input Action | Move/Aim/Attack Command |

### 2.2 Interface Messaging
- `IDamageable`
  - 대상이 무엇인지(Player/Enemy/Prop)와 무관하게 동일 계약으로 Damage 전달
- `IInteractable`
  - Item, Lever 등 상호작용 오브젝트의 통신 규격 통일

### 2.3 Runtime Data Flow
1. `InputProvider`가 입력을 표준 Command로 변환
2. `MovementComponent`/`CombatComponent`가 Command를 소비
3. Projectile 또는 Hit Scan이 `IDamageable`에 Damage 전달
4. `HealthComponent`가 체력/사망 Event를 발행
5. AI/Spawner/UI가 해당 Event를 구독해 반응

## 3. Core Gameplay Systems (MVP Definition)

### 3.1 Movement and Physics
- 입력:
  - `WASD`: 평면 이동
  - Mouse Cursor: 월드 기준 조준/회전 방향
- 구현 원칙:
  - 카메라 Raycast로 지면 교차 지점을 계산합니다.
  - 캐릭터 회전은 `Quaternion.Slerp`로 보간합니다.
  - 이동 속도 변화는 `Vector3.Lerp` 또는 가속 모델로 보간합니다.
- 목표:
  - 입력 지연 체감 최소화, 미끄러짐 과다 방지, 회전 안정성 확보

### 3.2 AI and Navigation
- `NavMeshAgent`로 추격/회피 경로 계산
- Behavior 구조:
  - `Selector`: 공격 가능 여부를 우선 평가
  - `Sequence`: `타겟확인 -> 접근 -> 공격`의 절차 실행
  - `Blackboard`: `Target`, `LastKnownPosition`, `AggroRange` 공유
- 목표:
  - 근접/원거리 Enemy 공통 행동 프레임 유지
  - 장애물 환경에서도 추격 실패율 최소화

### 3.3 Combat and Object Pooling
- Projectile, Hit VFX, Impact Decal은 Pool로 관리
- 공격 속도/상태이상 지속은 `Coroutine`으로 시간 관리
- 목표:
  - 연사 상황에서도 GC Spike 억제
  - Projectile 수 증가 시에도 프레임 안정성 확보

## 4. Data and Resource Management

### 4.1 ScriptableObject Data-Driven Policy
- `StatData`: MaxHP, MoveSpeed, Defense, CritRate
- `WeaponData`: Damage, FireRate, ProjectileRef, VFX/SFXRef
- `EncounterSpawnData`: 풀 기반 인카운터 스폰 개수·간격·반경(구역 이벤트·퀘스트와 연동 가능)
- `QuestDefinition`: 퀘스트 ID, 목표 처치 수 등 최소 목표 정의
- 규칙:
  - 템플릿 SO와 런타임 시트(`CharacterStatRuntimeSheet` 등)를 분리합니다.
  - 밸런싱은 SO 에셋 변경만으로 가능해야 합니다.

### 4.2 Addressables Policy
- 비동기 로딩:
  - 스테이지/던전 진입 시 필요한 Enemy/Weapon/VFX를 선로드
  - 인카운터 또는 다음 구역 진입 전 보조 에셋을 백그라운드 로딩
- 해제:
  - Stage 종료/씬 전환 시 `Addressables.Release` 일괄 수행
- 목표:
  - 불필요한 상주시 메모리 제거
  - 런타임 Hitch 최소화

## 5. Visual Feedback Design

### 5.1 VFX Split Policy
- `Shuriken`
  - 피격 스파크, 먼지, 짧은 로컬 이펙트
- `VFX Graph`
  - 범위 스킬, 포탈, 대량 입자 연산
- `Cinemachine Impulse`
  - 타격 순간 카메라 반응으로 피드백 강화

### 5.2 Feedback Rule
- 공격 입력 -> 발사 -> 타격 -> 카메라 반응을 0.2초 이내 체감 가능한 시퀀스로 유지
- 고빈도 전투에서도 과도한 Screen Shake 누적 방지(쿨다운/감쇠 적용)

## 6. MVP Success Criteria
- 전투 루프가 10분 이상 안정적으로 유지됩니다.
- 인카운터·전투 구간에서 프레임 드랍이 반복적으로 발생하지 않습니다.
- Enemy/Weapon/Encounter 콘텐츠 추가 시 코드 변경 없이 데이터 확장이 가능합니다.
- 세이브/로드가 퀘스트·월드 플래그·처치 수 등 핵심 RPG 상태를 유실 없이 보존합니다.
- 주요 리소스 로딩/해제가 `Addressables` 정책에 맞게 동작합니다.

## 7. Risk and Mitigation
- Risk: Pool 크기 과소 설정으로 런타임 재할당 발생
  - Mitigation: 최대 동시 발사량 기반 초기 Pool 산정 + 런타임 모니터링
- Risk: Addressables 의존성 누락으로 로드 실패
  - Mitigation: Label 규칙, Build 전 Validate 체크
- Risk: AI Behavior 조건 충돌로 비정상 상태 고착
  - Mitigation: Blackboard 상태 전이 로그와 디버그 뷰 유지

## 8. Glossary
- `Composition`: 상속 대신 컴포넌트를 조립해 기능 확장
- `Decoupling`: 시스템 간 직접 의존 제거
- `A* Algorithm`: 최단 경로 탐색 기반 알고리즘(`NavMesh` 내부 경로 계산의 기반 개념)
- `GC Spike`: Garbage Collection 시점의 일시적 프레임 드랍
