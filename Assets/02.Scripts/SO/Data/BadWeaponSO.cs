using UnityEngine;


[CreateAssetMenu(
    fileName = "Weapon_New",
    menuName = "Lab/Bad Weapon")]
public class BadWeaponSO : ScriptableObject
{
    [SerializeField]
    private string weaponId;
    [SerializeField]
    private int baseDamage;
    [SerializeField]
    private int maxAmmo;
  
    public string WeaponId => weaponId;
    public int BaseDamage => baseDamage;
    public int MaxAmmo => maxAmmo;

}
