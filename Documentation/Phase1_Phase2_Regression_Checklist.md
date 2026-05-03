# Phase 1·2 회귀 테스트 체크리스트 (완료 기준)

## Phase 1 — 이동·카메라·입력·로코모션

| 항목 | 기대 동작 |
|------|-----------|
| Validation Scene 생성 | `Tools/Systemic Overload/Phase Validation/Build Phase 1 Movement Scene` 성공, Build Settings에 씬 등록 |
| Animator Controller | `Create Phase1 Locomotion Placeholder Controller` 선행 시 `Speed` / `IsGrounded` 반응 |
| WASD 이동 | `CharacterController` 기반 이동, 지면 인식 |
| Orbit 카메라 | 줌·피벗·충돌 마스크 정상 |
| 입력 충돌 | `Attack`은 **Space / 게임패드 South** (LMB/RMB와 분리 유지) |

## Phase 2 — 전투 루프

| 항목 | 기대 동작 |
|------|-----------|
| Validation Scene | `Build Phase 2 Damage Weapon Scene` 후 `TrainingDummy` 레이 히트 |
| 자기 자신 무시 | 플레이어 레이가 본인 콜라이더에 맞아도 데미지 없음 |
| `HealthComponent` | `currentHealth` 감소, 0 이하 시 사망 처리 |
| `AttackTrig` | Animator에 해당 Trigger 파라미터가 있을 때만 발동 |

## Known issues (문서화용)

- `hitLayerMask`를 `Everything`으로 두면 의도치 않은 레이어까지 맞을 수 있음 → 프로덕션에서는 `Enemy` 등 전용 Layer 권장.
- `WeaponData` 연동은 Phase 3에서 `CombatComponent`에 선택적으로 적용됩니다.

## Done 기준 (Phase 1·2 하드닝)

위 체크리스트를 **스모크 1회 이상** 통과하고, 본 저장소의 Validation Scene 도구로 씬을 재생성할 수 있으면 Phase 1·2 하드닝 완료로 간주합니다.
