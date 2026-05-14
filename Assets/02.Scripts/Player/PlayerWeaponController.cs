using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("무기 목록")]
    [SerializeField] private WeaponData[] weapons;
    [Header("Input (Input System)")]
    [SerializeField] private InputAction equipWeapon1Action = new InputAction("EquipWeapon1", InputActionType.Button, "<Keyboard>/1");
    [SerializeField] private InputAction equipWeapon2Action = new InputAction("EquipWeapon2", InputActionType.Button, "<Keyboard>/2");
    [SerializeField] private InputAction equipWeapon3Action = new InputAction("EquipWeapon3", InputActionType.Button, "<Keyboard>/3");
    [SerializeField] private InputAction attackAction = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
    [SerializeField] private InputAction reloadAction = new InputAction("Reload", InputActionType.Button, "<Keyboard>/r");

    private WeaponRuntime currentWeapon;
    private int currentWeaponIndex;
    private float nextAttackTime;

    private void Start()
    {
        EquipWeapon(0);
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
            return;

        if (index < 0 || index >= weapons.Length)
            return;

        currentWeaponIndex = index;
        currentWeapon = new WeaponRuntime(weapons[index]);

        Debug.Log($"무기 장착: {currentWeapon.data.weaponName}");
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

        nextAttackTime = Time.time + (1f / currentWeapon.data.attackRate);

        currentWeapon.ConsumeAmmo();

        int finalDamage = CalculateDamage();

        Debug.Log(
            $"{currentWeapon.data.weaponName} 공격 / " +
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
            $"{currentWeapon.data.weaponName} 재장전 완료 / " +
            $"Ammo: {currentWeapon.currentAmmo}"
        );
    }
}