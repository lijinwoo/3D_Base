# CCP + StarterAssets Production Integration

## 개요

이 문서는 `Character Controller Pro`와 `StarterAssets`를 통합해 Production 환경에서 바로 사용할 수 있는 Player Framework를 생성하는 절차를 정리합니다.

핵심 원칙은 다음과 같습니다.

- 이동/물리의 단일 진실 소스는 `CCP(CharacterActor)`로 유지합니다.
- 입력/카메라/Mobile UI는 `StarterAssets` 계층을 재사용합니다.
- Prefab/Scene은 Editor Builder를 통해 반복 가능하게 생성합니다.

## 생성되는 결과물

- `Assets/StarterAssets/CCPProduction/Prefabs/CCP_StarterAssets_Player.prefab`
- `Assets/StarterAssets/CCPProduction/Scenes/CCP_StarterAssets_Validation.unity`

## 핵심 스크립트

- `Assets/StarterAssets/ThirdPersonController/Scripts/CCP/StarterAssetsCcpInputHandler.cs`
  - `StarterAssetsInputs`를 CCP `InputHandler` 인터페이스로 매핑합니다.
- `Assets/StarterAssets/ThirdPersonController/Scripts/CCP/StarterAssetsCcpCameraSync.cs`
  - `look` 입력으로 카메라 타깃 회전을 갱신합니다.
- `Assets/StarterAssets/ThirdPersonController/Scripts/CCP/StarterAssetsCcpAnimatorBridge.cs`
  - CCP 상태를 StarterAssets Animator 파라미터로 전달합니다.
- `Assets/Editor/StarterAssetsCcpProductionBuilder.cs`
  - 통합 Prefab/Validation Scene을 자동 생성합니다.

## 사용 방법

1. Unity Editor에서 메뉴를 실행합니다.
   - `Tools/Systemic Overload/CCP Integration/Build Production Player + Validation Scene`
2. 생성 완료 로그를 확인합니다.
3. `CCP_StarterAssets_Validation` 씬을 열고 입력/카메라/이동 동작을 검증합니다.

## 입력 매핑 규칙

- `Movement` <- `StarterAssetsInputs.move`
- `Jump` <- `StarterAssetsInputs.jump`
- `Run` <- `StarterAssetsInputs.sprint`
- `Pitch` <- `StarterAssetsInputs.look.y`
- `Roll` <- `StarterAssetsInputs.look.x`

아래 액션은 기본적으로 false를 반환합니다(필요 시 확장).

- `Interact`
- `Jet Pack`
- `Dash`
- `Crouch`

## 검증 체크리스트

- KeyboardMouse에서 이동/점프/스프린트 정상 동작
- Gamepad에서 이동/시점/점프 정상 동작
- Mobile UI(CanvasInputs)에서 이동/시점/점프/스프린트 동작
- 경사/계단에서 Grounded 전환 및 애니메이션 전환 정상
- 카메라 yaw 기준 이동 방향 일치
- Play Mode에서 GC alloc 스파이크 유무 확인

## 확장 가이드

- `Interact`, `Dash`, `Crouch`, `Jet Pack` 사용이 필요하면 `StarterAssetsCcpInputHandler`에 액션 소스를 추가합니다.
- 카메라 감도/클램프는 `StarterAssetsCcpCameraSync` 인스펙터에서 프로젝트 감각에 맞게 조정합니다.
- Animator 파라미터 이름이 다르면 `StarterAssetsCcpAnimatorBridge` 파라미터 문자열을 교체합니다.
