using System.Collections.Generic;
using UnityEngine;

public class WeaponTester : MonoBehaviour
{
    [Header("테스트 대상 플레이어 상태")]
    [SerializeField] private PlayerStatus playerStatus;

    [Header("테스트할 무기 데이터 목록")]
    [SerializeField] private List<WeaponData> weaponSet = new List<WeaponData>();

    private void Start()
    {
        if (playerStatus == null)
        {
            Debug.LogWarning("WeaponTester: PlayerStatus가 연결되지 않았습니다.");
            return;
        }

        if (weaponSet == null || weaponSet.Count == 0)
        {
            Debug.LogWarning("WeaponTester: 테스트할 무기 데이터가 비어 있습니다.");
            return;
        }

        Debug.Log($"===== Weapon Test Start / Player Class: {playerStatus.CurrentJobClass} =====");

        for (int weaponIndex = 0; weaponIndex < weaponSet.Count; weaponIndex++)
        {
            WeaponData weaponData = weaponSet[weaponIndex];
            if (weaponData == null)
            {
                Debug.LogWarning($"[{weaponIndex}] 무기 데이터가 비어 있습니다.");
                continue;
            }

            bool canEquip = playerStatus.CanEquipWeapon(weaponData);

            // 과제 검증을 위해 무기별 핵심 속성과 장착 가능 여부를 한 번에 출력합니다.
            Debug.Log(
                $"[{weaponIndex}] " +
                $"Name: {weaponData.weaponName}, " +
                $"Job: {weaponData.jobClassType}, " +
                $"Damage: {weaponData.damage}, " +
                $"Range: {weaponData.attackRange}, " +
                $"AttackRate: {weaponData.attackRate}, " +
                $"UseAmmo: {weaponData.useAmmo}, " +
                $"IsMelee: {weaponData.isMelee}, " +
                $"CanEquip: {canEquip}"
            );
        }

        Debug.Log("===== Weapon Test End =====");
    }
}
