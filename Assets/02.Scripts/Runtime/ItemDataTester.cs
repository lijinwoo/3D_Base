using UnityEngine;

public class ItemDataTester : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    private void Start()
    {
        if (itemData == null)
        {
            Debug.LogWarning("ItemData가 연결되지 않았습니다.");
            return;
        }

        Debug.Log($"아이템 ID: {itemData.itemId}");
        Debug.Log($"아이템 이름: {itemData.itemName}");
        Debug.Log($"아이템 타입: {itemData.itemType}");
        Debug.Log($"구매 가격: {itemData.buyPrice}");
        Debug.Log($"판매 가격: {itemData.sellPrice}");
        Debug.Log($"스택 가능: {itemData.canStack}");
    }
}


