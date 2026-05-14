using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
   [Header("HP")]
   public int maxHp = 100;
   public int currentHp = 50;
   
   [Header("MP")]
   public int maxMp = 50;
   public int currentMp = 10;
   
   [Header("Combat")]
   public int attack = 10;
   public int defense = 5;

   [Header("Currency")] public int gold = 0;

   public void HealHp(int amount)
   {
      currentHp = Mathf.Min( currentHp + amount,maxHp);
      Debug.Log($"HP 회복: + {amount}, 현재 HP: {currentHp/maxHp}");
   }
   
   public void HealMp(int amount)
   {
      currentHp = Mathf.Min( currentMp + amount,maxMp);
      Debug.Log($"MP 회복: + {amount}, 현재 MP: {currentMp/maxMp}");
   }

   public void IncreaseAttack(int amount)
   {
      attack += amount;
      Debug.Log($"공격력 증가: + {amount}, 현재 공격력: {attack}");
   }

   public void IncreaseDefence(int amount)
   {
      defense += amount;
      Debug.Log($"방어력 증가: + {amount}, 현재 방어력: {defense}");
   }
   
   public void AddGold(int amount)
   {
      gold += amount;
      Debug.Log($"골드 획득: + {amount}, 현재 골드: {gold}");
   }
   
   
   
}
