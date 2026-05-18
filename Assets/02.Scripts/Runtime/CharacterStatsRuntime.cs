using UnityEngine;

public class CharacterStatsRuntime
{
    public CharacterStatsDefinition Definition { get; }
    public int MaxHp => Definition.maxHp;
    public int Attack => Definition.attack;
    public int Defense => Definition.defense;

    public int CurrentHp { get; private set; }

    public CharacterStatsRuntime(CharacterStatsDefinition definition)
    {
        Definition = definition;
        CurrentHp = definition.maxHp;
    }

    public void TakeDamage(int rawDamage)
    {
        int finalDamage = Mathf.Max(1, rawDamage - Defense);
        CurrentHp = Mathf.Max(0, CurrentHp - finalDamage);
    }
}