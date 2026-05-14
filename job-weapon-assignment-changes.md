# 직업무기 과제 변경 내역

## 1) 과제 목표
- `ScriptableObject` 기반으로 직업별 무기 데이터를 구성하고, 플레이어 직업에 맞는 무기만 장착되도록 `equip flow`를 구현한다.
- 각 직업군(Archer, Warrior, Mage, Assassin, Gunner)별 무기 3개 이상을 준비해 과제 조건을 충족한다.

## 2) 코드 변경 파일

### `Assets/02.Scripts/SO/JobClassType.cs`
- 신규 `enum` 추가
  - `Archer`, `Warrior`, `Mage`, `Assassin`, `Gunner`
- 목적: 무기와 플레이어의 직업을 동일 타입으로 비교하기 위함

### `Assets/02.Scripts/SO/Data/WeaponData.cs`
- `WeaponData` 필드 확장
  - `jobClassType` 추가: 무기가 어떤 직업 전용인지 표시
  - `isMelee` 추가: 근접 무기 여부 표시
- 기존 필수 속성(`weaponName`, `damage`, `attackRange`, `attackRate`, `useAmmo`)과 함께 과제 요구사항을 충족하도록 구성

### `Assets/02.Scripts/Player/PlayerStatus.cs`
- 직업 상태 및 장착 상태 필드 추가
  - `currentJobClass`
  - `equippedWeapon`
- 무기 장착 검증 로직 추가
  - `CanEquipWeapon(WeaponData weaponData)`
  - `TryEquipWeapon(WeaponData weaponData)`
- 데이터 유효성 검증 추가
  - 빈 `weaponName`, `damage <= 0`, `attackRate <= 0`인 경우 장착 실패 처리
- 로그 개선
  - 실패 사유(직업 불일치/데이터 미설정) 명확화
  - 무기 이름이 비어있을 때 `asset name` fallback 사용

### `Assets/02.Scripts/Player/PlayerWeaponController.cs`
- `PlayerStatus` 연동 강화
  - 시작 시 `PlayerStatus` 확인
  - `EquipWeapon()`에서 `playerStatus.TryEquipWeapon()` 통과 시에만 장착
- 안정성 보강
  - `Awake()`에서 `EnsureInputActions()` 호출
  - `InputAction` 직렬화 값이 비어도 기본 키 바인딩 보장
- 방어 코드 및 로그 추가
  - 무기 배열 비어있음/인덱스 오류/슬롯 null 경고
  - `attackRate <= 0` 공격 차단
  - 장착/공격/재장전 로그에서 무기 이름 fallback 적용

## 3) Prefab 변경 파일

### `Assets/03.Prefab/JH_PlayerArmature.prefab`
- `PlayerStatus` 컴포넌트 부착
- `PlayerWeaponController` 컴포넌트 부착
- `PlayerWeaponController.playerStatus`를 동일 오브젝트의 `PlayerStatus`로 연결

> 결과적으로 `WeaponTester`가 아니라 `PlayerWeaponController` 중심의 장착 흐름으로 실행되도록 변경됨.

## 4) 무기 `ScriptableObject` 에셋 변경 (`Assets/04.Data/Weapons`)

### 기존 에셋 보정 (5개)
- `Weapon_HunterBow`
- `Weapon_IronSword`
- `Weapon_FireStaff`
- `Weapon_Dagger`
- `Weapon_IronRifle`

보정 내용:
- `weaponId`, `weaponName`, `jobClassType`
- `damage`, `attackRange`, `attackRate`
- `useAmmo`, `isMelee` 포함 과제 필드값 세팅

### 신규 에셋 추가 (10개)
- Archer
  - `Weapon_Archer_Crossbow`
  - `Weapon_Archer_HuntingKnife`
- Warrior
  - `Weapon_Warrior_GreatAxe`
  - `Weapon_Warrior_Spear`
- Mage
  - `Weapon_Mage_IceWand`
  - `Weapon_Mage_ManaOrb`
- Assassin
  - `Weapon_Assassin_TwinBlades`
  - `Weapon_Assassin_ThrowingKnife`
- Gunner
  - `Weapon_Gunner_Shotgun`
  - `Weapon_Gunner_CombatKnife`

최종 구성:
- 직업군 5종 x 각 3개 = 총 15개 무기 데이터

## 5) 과제 조건 충족 여부
- `ScriptableObject` 데이터 구조 설계: 충족
- 직업별 무기 에셋 3개 이상 생성: 충족
- 탄약 사용 무기 포함: 충족
- 근접 무기 포함: 충족
- 직업 불일치 시 장착 제한: 충족 (`PlayerStatus` 기반)
- `PlayerWeaponController` 중심 장착 flow: 충족

## 6) 실행 확인 방법
1. `JH_PlayerArmature.prefab`의 `PlayerStatus.currentJobClass` 설정
2. `PlayerWeaponController.weapons` 슬롯에 테스트할 무기 할당
3. Play 후 키 입력
   - `1`, `2`, `3`: 무기 장착 시도
   - 직업 일치: 장착 성공 로그
   - 직업 불일치/데이터 미설정: 장착 실패 로그
4. `Mouse Left`: 공격 로그 확인 (`damage`, `range`, `ammo`)
