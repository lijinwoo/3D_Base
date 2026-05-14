using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("무기 목록")]
    [SerializeField] private WeaponData[] weapons;
    [Header("플레이어 상태")]
    [SerializeField] private PlayerStatus playerStatus;
    [Header("Input (Input System)")]
    [SerializeField] private InputAction equipWeapon1Action = new InputAction("EquipWeapon1", InputActionType.Button, "<Keyboard>/1");
    [SerializeField] private InputAction equipWeapon2Action = new InputAction("EquipWeapon2", InputActionType.Button, "<Keyboard>/2");
    [SerializeField] private InputAction equipWeapon3Action = new InputAction("EquipWeapon3", InputActionType.Button, "<Keyboard>/3");
    [SerializeField] private InputAction attackAction = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
    [SerializeField] private InputAction reloadAction = new InputAction("Reload", InputActionType.Button, "<Keyboard>/r");

    private WeaponRuntime currentWeapon;
    private int currentWeaponIndex;
    private float nextAttackTime;

    // 무기 표시 이름을 안전하게 반환합니다.
    private static string GetWeaponLabel(WeaponData weaponData)
    {
        if (weaponData == null)
            return "(null)";

        return string.IsNullOrWhiteSpace(weaponData.weaponName) ? weaponData.name : weaponData.weaponName;
    }

    private void Awake()
    {
        EnsureInputActions();
    }

    private void Start()
    {
        if (playerStatus == null)
            playerStatus = GetComponent<PlayerStatus>();

        if (playerStatus == null)
        {
            Debug.LogWarning("PlayerWeaponController: PlayerStatus가 없어 직업 기반 장착 제한을 적용할 수 없습니다.");
            return;
        }

        EquipWeapon(0);
    }

    // 프리팹 직렬화 데이터가 비어 있어도 기본 키 바인딩을 보장합니다.
    private void EnsureInputActions()
    {
        if (equipWeapon1Action == null)
            equipWeapon1Action = new InputAction("EquipWeapon1", InputActionType.Button, "<Keyboard>/1");

        if (equipWeapon2Action == null)
            equipWeapon2Action = new InputAction("EquipWeapon2", InputActionType.Button, "<Keyboard>/2");

        if (equipWeapon3Action == null)
            equipWeapon3Action = new InputAction("EquipWeapon3", InputActionType.Button, "<Keyboard>/3");

        if (attackAction == null)
            attackAction = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");

        if (reloadAction == null)
            reloadAction = new InputAction("Reload", InputActionType.Button, "<Keyboard>/r");
    }

    private void OnEnable()
    {
        equipWeapon1Action.performed += OnEquipWeapon1Performed;
        equipWeapon2Action.performed += OnEquipWeapon2Performed;
        equipWeapon3Action.performed += OnEquipWeapon3Performed;
        attackAction.performed += OnAttackPerformed;
        reloadAction.performed += OnReloadPerformed;

        equipWeapon1Action.Enable();
        equipWeapon2Action.Enable();
        equipWeapon3Action.Enable();
        attackAction.Enable();
        reloadAction.Enable();
    }

    private void OnDisable()
    {
        reloadAction.Disable();
        attackAction.Disable();
        equipWeapon3Action.Disable();
        equipWeapon2Action.Disable();
        equipWeapon1Action.Disable();

        reloadAction.performed -= OnReloadPerformed;
        attackAction.performed -= OnAttackPerformed;
        equipWeapon3Action.performed -= OnEquipWeapon3Performed;
        equipWeapon2Action.performed -= OnEquipWeapon2Performed;
        equipWeapon1Action.performed -= OnEquipWeapon1Performed;
    }

    private void OnEquipWeapon1Performed(InputAction.CallbackContext context)
    {
        EquipWeapon(0);
    }

    private void OnEquipWeapon2Performed(InputAction.CallbackContext context)
    {
        EquipWeapon(1);
    }

    private void OnEquipWeapon3Performed(InputAction.CallbackContext context)
    {
        EquipWeapon(2);
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        TryAttack();
    }

    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        Reload();
    }

    private void EquipWeapon(int index)
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogWarning("PlayerWeaponController: weapons 배열이 비어 있습니다.");
            return;
        }

        if (index < 0 || index >= weapons.Length)
        {
            Debug.LogWarning($"PlayerWeaponController: 잘못된 무기 인덱스입니다. index={index}, length={weapons.Length}");
            return;
        }

        WeaponData nextWeaponData = weapons[index];
        if (nextWeaponData == null)
        {
            Debug.LogWarning($"PlayerWeaponController: {index}번 슬롯에 무기 데이터가 연결되지 않았습니다.");
            return;
        }

        // PlayerStatus에서 직업 기반 장착 제한을 판정합니다.
        if (!playerStatus.TryEquipWeapon(nextWeaponData))
            return;

        currentWeaponIndex = index;
        currentWeapon = new WeaponRuntime(nextWeaponData);

        Debug.Log($"무기 장착: {GetWeaponLabel(currentWeapon.data)}");
    }

    private void TryAttack()
    {
        if (currentWeapon == null)
            return;

        if (Time.time < nextAttackTime)
        {
            Debug.Log("공격 쿨타임 중입니다.");
            return;
        }

        if (!currentWeapon.HasAmmo())
        {
            Debug.Log("탄약이 없습니다. R 키로 재장전하세요.");
            return;
        }

        if (currentWeapon.data.attackRate <= 0f)
        {
            Debug.LogWarning($"공격 실패: attackRate가 0 이하입니다. 무기={GetWeaponLabel(currentWeapon.data)}");
            return;
        }

        nextAttackTime = Time.time + (1f / currentWeapon.data.attackRate);

        currentWeapon.ConsumeAmmo();

        int finalDamage = CalculateDamage();

        Debug.Log(
            $"{GetWeaponLabel(currentWeapon.data)} 공격 / " +
            $"Damage: {finalDamage}, " +
            $"Range: {currentWeapon.data.attackRange}, " +
            $"Ammo: {currentWeapon.currentAmmo}"
        );
    }

    private int CalculateDamage()
    {
        int damage = currentWeapon.data.damage;

        float randomValue = Random.value;
        if (randomValue <= currentWeapon.data.criticalChance)
        {
            damage = Mathf.RoundToInt(damage * currentWeapon.data.criticalMultiplier);
            Debug.Log("치명타 발생!");
        }

        return damage;
    }

    private void Reload()
    {
        if (currentWeapon == null)
            return;

        currentWeapon.Reload();

        Debug.Log(
            $"{GetWeaponLabel(currentWeapon.data)} 재장전 완료 / " +
            $"Ammo: {currentWeapon.currentAmmo}"
        );
    }
}