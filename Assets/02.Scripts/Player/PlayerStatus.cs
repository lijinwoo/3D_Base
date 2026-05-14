using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Class")]
    [SerializeField] private JobClassType currentJobClass;

    [Header("HP")]
    public int maxHp = 100;
    public int currentHp = 50;

    [Header("MP")]
    public int maxMp = 50;
    public int currentMp = 10;

    [Header("Combat")]
    public int attack = 10;
    public int defense = 5;

    [Header("Currency")]
    public int gold = 0;

    [Header("Weapon")]
    [SerializeField] private WeaponData equippedWeapon;

    public JobClassType CurrentJobClass => currentJobClass;
    public WeaponData EquippedWeapon => equippedWeapon;

    // 무기 표시 이름을 안전하게 반환합니다.
    private static string GetWeaponLabel(WeaponData weaponData)
    {
        if (weaponData == null)
            return "(null)";

        return string.IsNullOrWhiteSpace(weaponData.weaponName) ? weaponData.name : weaponData.weaponName;
    }

    // 과제 기준 필수 데이터가 채워졌는지 검사합니다.
    private static bool IsWeaponDataConfigured(WeaponData weaponData)
    {
        return !string.IsNullOrWhiteSpace(weaponData.weaponName)
               && weaponData.damage > 0
               && weaponData.attackRate > 0f;
    }

    // 직업 조건을 만족하는 무기인지 검사합니다.
    public bool CanEquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogWarning("장착할 무기 데이터가 없습니다.");
            return false;
        }

        if (!IsWeaponDataConfigured(weaponData))
        {
            Debug.LogWarning(
                $"장착 실패: 무기 데이터가 미설정 상태입니다. Asset={weaponData.name}, " +
                $"weaponName='{weaponData.weaponName}', damage={weaponData.damage}, attackRate={weaponData.attackRate}"
            );
            return false;
        }

        if (weaponData.jobClassType != currentJobClass)
        {
            Debug.LogWarning(
                $"장착 실패: 현재 직업={currentJobClass}, 무기 직업={weaponData.jobClassType}, 무기={GetWeaponLabel(weaponData)}"
            );
            return false;
        }

        return true;
    }

    // 직업 규칙에 맞는 경우에만 무기 장착을 허용합니다.
    public bool TryEquipWeapon(WeaponData weaponData)
    {
        if (!CanEquipWeapon(weaponData))
            return false;

        equippedWeapon = weaponData;
        Debug.Log($"장착 성공: {currentJobClass} 직업이 {GetWeaponLabel(equippedWeapon)}을(를) 장착했습니다.");
        return true;
    }

    public void HealHp(int amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
        Debug.Log($"HP 회복: +{amount}, 현재 HP: {currentHp}/{maxHp}");
    }

    public void HealMp(int amount)
    {
        currentMp = Mathf.Min(currentMp + amount, maxMp);
        Debug.Log($"MP 회복: +{amount}, 현재 MP: {currentMp}/{maxMp}");
    }

    public void IncreaseAttack(int amount)
    {
        attack += amount;
        Debug.Log($"공격력 증가: +{amount}, 현재 공격력: {attack}");
    }

    public void IncreaseDefense(int amount)
    {
        defense += amount;
        Debug.Log($"방어력 증가: +{amount}, 현재 방어력: {defense}");
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"골드 획득: +{amount}, 현재 골드: {gold}");
    }
}

