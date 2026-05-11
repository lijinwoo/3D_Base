# Systemic Overload Documentation

## 문서 목적
이 폴더는 `Systemic Overload`의 MVP 구현과 운영을 위한 단일 참조 소스입니다.  
설계 철학(`Decoupling`, `Scalability`, `Performance`)을 실제 Unity 3D 개발 흐름에 맞춰 문서화했습니다.

## 권장 읽기 순서
0. [Game Vision — Hybrid Single-player RPG](Game_Vision_Hybrid_Single_RPG.md)
1. [Technical Design Specification](Technical_Design_Specification_Systemic_Overload.md)
2. [Architecture Component and Interface](Architecture_Component_and_Interface.md)
3. [Gameplay Systems Movement AI Combat](Gameplay_Systems_Movement_AI_Combat.md)
4. [Data and Resource Management](Data_and_Resource_Management.md)
5. [VFX and Feedback Pipeline](VFX_and_Feedback_Pipeline.md)
6. [MVP Roadmap Phase and Release Management](MVP_Roadmap_Phase_and_Release_Management.md)

## 문서 목록
- [Technical Design Specification](Technical_Design_Specification_Systemic_Overload.md)
  - 프로젝트 비전, 시스템 목표, 기술 스택 기준 문서
- [Architecture Component and Interface](Architecture_Component_and_Interface.md)
  - Composition 기반 설계, Interface Messaging 규칙, 의존성 경계
- [Gameplay Systems Movement AI Combat](Gameplay_Systems_Movement_AI_Combat.md)
  - 이동, 전투, AI의 런타임 동작/구현 기준
- [Data and Resource Management](Data_and_Resource_Management.md)
  - `ScriptableObject`, `Addressables`, 메모리 관리 정책
- [VFX and Feedback Pipeline](VFX_and_Feedback_Pipeline.md)
  - `Shuriken`, `VFX Graph`, `Cinemachine Impulse` 적용 정책
- [MVP Roadmap Phase and Release Management](MVP_Roadmap_Phase_and_Release_Management.md)
  - MVP Phase 운영, 브랜치 전략, 릴리즈/태그 기준, Phase별 `Animation` 산출물 및 `Animation Pipeline` 공통 규칙
- [Phase1 vs Phase2 Change Log](Phase1_vs_Phase2_Change_Log.md) — Phase 2 도입 시 Phase 1 입력/애니/전투 연동 변경 요약
- [Phase2 Implementation Guide](Phase2_Implementation_Guide_and_Source.md) — Phase 2 구현 참고(코드는 `Assets`에 반영됨)
- [Unity 3D Course — Notion & Git](Unity3D_Course_Notion_and_Git.md) — 수업용 Notion 페이지 링크 및 `phase/*` 브랜치 정리
- [Baseline — main and integration](Baseline_Main_and_Integration.md) — `main` 기준선·통합 순서·Phase 1~2 보강 범위 고정
- [Phase 1·2 Regression Checklist](Phase1_Phase2_Regression_Checklist.md) — Phase 1·2 회귀·완료 기준
- [Phase 6 Profiling Checklist](Phase6_Profiling_Checklist.md) — CPU/GPU/Memory/GC 점검 항목
- [Notion sync — Phase 3~6](Notion_Sync_Phase3_to_6.md) — Notion·태그·릴리즈 노트 동기화 절차

## 현재 패키지 기준
`Packages/manifest.json` 기준 핵심 패키지:
- `Input System`
- `AI Navigation`
- `Addressables`
- `Cinemachine`
- `VFX Graph`
- `URP`
- `Test Framework`

> 미사용으로 정리한 패키지: `com.unity.visualscripting`, `com.unity.multiplayer.center`. 향후 추가 시 사용 사례를 먼저 정의하고 manifest에 등록합니다.

## 어셈블리 / 테스트 구조
- 런타임 코드는 `SystemicOverload.Gameplay` (`Assets/Scripts/Gameplay`) 어셈블리로 분리되어 있습니다.
- 에디터 도구는 `SystemicOverload.EditorTools` (`Assets/Editor`) 어셈블리이며, Editor 플랫폼 전용입니다.
- 테스트는 `Assets/Tests/EditMode`, `Assets/Tests/PlayMode`에 각각 asmdef와 함께 위치합니다. 필드 변경 회귀, 풀·PooledInstanceLink, 전투 self-hit skip, 세이브 DTO 직렬화, 퀘스트 처치 카운트, 검증 씬 경로·BuildSettings 정합성을 smoke level로 검증합니다.

## 인코딩 규칙
- 모든 문서와 소스는 **UTF-8 (BOM 없음)** 으로 저장합니다.
- Windows PowerShell에서 신규 파일 생성 시 인코딩이 UTF-16/CP949로 변경되지 않도록 주의합니다.

## Phase 운영 원칙
- 개발 단위는 `feature/<phase>-<topic>` 브랜치에서 시작합니다.
- 각 기능은 해당 `phase/*` 브랜치로 PR 후 통합합니다.
- Phase 완료 후 `develop`으로 병합하고 마일스톤 태그를 생성합니다.
- 최종 MVP는 `release/mvp-1.0.0`에서 QA 후 `main`으로 병합합니다.
- 각 Phase는 `Assets/01.Scenes/PhaseValidation/` 아래 RPG 수직 슬라이스용 검증 씬을 유지합니다(에디터 메뉴: `Tools/Systemic Overload/RPG Vertical Slice/`).

## 문서 유지보수 규칙
- 새로운 시스템 도입 시, 관련 문서를 먼저 업데이트한 뒤 구현을 진행합니다.
- 런타임 성능 관련 변경(`Object Pool`, `Addressables`, `VFX`)은 체크리스트를 반드시 갱신합니다.
- Phase 종료 시 `Done/Out of Scope/Risk`를 문서에 기록합니다.
