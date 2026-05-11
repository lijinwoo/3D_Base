# Game Vision — Hybrid Single-player RPG

## 한 줄
`Systemic Overload`는 **3D 탑다운 액션 전투**를 축으로, **탐험·퀘스트·영속 진행(세이브)**을 포함하는 **Hybrid Single-player RPG**입니다.

## Pillars
1. **Combat clarity** — 입력·조준·피드백이 즉시 이해되는 전투 체감
2. **Progression you can trust** — 퀘스트·월드 플래그·처치 수가 세이브로 보존됨
3. **Content scalability** — `ScriptableObject`로 인카운터·무기·스탯을 데이터 주도로 확장
4. **Technical vertical slice** — 검증 씬으로 회귀 테스트와 프로파일링을 반복

## 비범위 (현 MVP)
- Roguelite 메타 루프(런 리셋 중심 설계)
- 멀티플레이
- 복잡한 경제/인벤 전체(단계적 도입)

## 관련 구현
- `EncounterDirector` / `EncounterSpawnData` — 인카운터 스폰
- `QuestService` / `QuestDefinition` — 최소 퀘스트 목표
- `SaveLoadService` — JSON 세이브
- `WorldStateService` — 정수 플래그 저장
- `PlayerTargetProvider` — AI 타겟 탐색의 단일 진입점
