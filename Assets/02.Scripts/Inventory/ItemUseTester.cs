using UnityEngine;
using UnityEngine.InputSystem;

public class ItemUseTester : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private ItemData itemToUse;
    [SerializeField] private InputAction useItemAction = new InputAction("UseItem", InputActionType.Button, "<Keyboard>/u");

    private void OnEnable()
    {
        useItemAction.performed += OnUseItemPerformed;
        useItemAction.Enable();
    }

    private void OnDisable()
    {
        useItemAction.Disable();
        useItemAction.performed -= OnUseItemPerformed;
    }

    private void OnUseItemPerformed(InputAction.CallbackContext context)
    {
        UseItem(itemToUse);
    }

    private void UseItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("사용할 아이템이 없습니다.");
            return;
        }

        if (!itemData.canUse)
        {
            Debug.Log($"{itemData.itemName}은 사용할 수 없는 아이템입니다.");
            return;
        }

        Debug.Log($"아이템 사용: {itemData.itemName}");

        foreach (ItemEffect effect in itemData.effects)
        {
            ApplyEffect(effect);
        }
    }

    private void ApplyEffect(ItemEffect effect)
    {
        switch (effect.effectType)
        {
            case ItemEffectType.HealHp:
                playerStatus.HealHp(effect.value);
                break;

            case ItemEffectType.HealMp:
                playerStatus.HealMp(effect.value);
                break;

            case ItemEffectType.IncreaseAttack:
                playerStatus.IncreaseAttack(effect.value);
                break;

            case ItemEffectType.IncreaseDefense:
                playerStatus.IncreaseDefense(effect.value);
                break;

            case ItemEffectType.AddGold:
                playerStatus.AddGold(effect.value);
                break;
        }
    }
}