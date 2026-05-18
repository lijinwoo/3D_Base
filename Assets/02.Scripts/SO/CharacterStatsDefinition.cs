using UnityEngine;

[CreateAssetMenu(
    fileName = "CHR_Stats_New",
    menuName = "RPG Data/Character Stats")]
public class CharacterStatsDefinition : ScriptableObject
{
    [Header("Base Stats")] public string characterId;
    public int maxHp = 100;
    public int attack = 10;
    public int defense = 3;

    [Header("Combat")] public float moveSpeed = 4.5f;
    public float attackInterval = 0.7f;
}