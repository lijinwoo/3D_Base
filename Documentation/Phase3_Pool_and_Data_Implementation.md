# Phase 3 — Object Pool 및 ScriptableObject 데이터 (구현 요약)

## 목적
전투 수치와 웨이브 구성을 데이터(`ScriptableObject`)로 분리하고, 적 스폰은 `GameObjectPool`로 재사용합니다.

## 핵심 타입

| 경로 | 설명 |
|------|------|
| `Assets/Scripts/Gameplay/Data/StatData.cs` | 최대 체력·피해 배율 |
| `Assets/Scripts/Gameplay/Data/WeaponData.cs` | 히트 스캔 수치·LayerMask |
<<<<<<< HEAD
| `Assets/Scripts/Gameplay/Data/EncounterSpawnData.cs` | 인카운터 스폰 개수·간격·반경 |
| `Assets/Scripts/Gameplay/Pooling/GameObjectPool.cs` | 단일 프리팹 풀 + Animator `Rebind` 옵션 |
| `Assets/Scripts/Gameplay/Encounter/EncounterDirector.cs` | `EncounterSpawnData` 기반 스폰 코루틴 |
| `Assets/Scripts/Gameplay/Pooling/PooledHealthReset.cs` | 풀 재사용 시 체력 리셋 |
| `Assets/Scripts/Gameplay/Pooling/PooledEnemyReleaseOnDeath.cs` | 사망 시 풀 반환 + 퀘스트 알림 |
=======
| `Assets/Scripts/Gameplay/Data/WaveData.cs` | 스폰 개수·간격·반경 |
| `Assets/Scripts/Gameplay/Pooling/GameObjectPool.cs` | 단일 프리팹 풀 + Animator `Rebind` 옵션 |
| `Assets/Scripts/Gameplay/Wave/WavePoolDirector.cs` | `WaveData` 기반 스폰 코루틴 |
| `Assets/Scripts/Gameplay/Wave/HealthResetOnSpawn.cs` | 풀 재사용 시 체력 리셋 |
>>>>>>> 29fbfa50cf419c0d688f39bdbd0021df496917ec

## Combat / Health 연동

- `CombatComponent`는 선택적 `WeaponData`를 적용합니다.
- `HealthComponent`는 선택적 `StatData`를 적용합니다.

## Validation Scene

에디터 메뉴: `Tools/Systemic Overload/Phase Validation/Build Phase 3 Combat Data Scene`  
생성 경로: `Assets/01.Scenes/PhaseValidation/Phase_03_CombatDataValidation.unity`  
<<<<<<< HEAD
샘플 데이터: `Assets/Data/PhaseValidation/` (`Stat_*`, `Weapon_*`, `Encounter_*`, `Quest_*`)  
=======
샘플 데이터: `Assets/Data/PhaseValidation/` (`Stat_*`, `Weapon_*`, `Wave_*`)  
>>>>>>> 29fbfa50cf419c0d688f39bdbd0021df496917ec
풀용 프리팹: `Assets/01.Scenes/PhaseValidation/Prefabs/PoolEnemy.prefab`

## Animator 재사용 정책

`GameObjectPool.resetAnimatorOnSpawn`가 켜져 있으면 스폰 시 `Animator.Rebind()` + `Update(0)`을 호출합니다.
