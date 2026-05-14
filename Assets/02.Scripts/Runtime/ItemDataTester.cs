using System;
using UnityEngine;

public class ItemDataTester : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    private void Start()
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData가 연결되지 않았습니다.");
            return;
        }
        Debug.Log($"Item Id: {itemData.itemId}");
        Debug.Log($"Item Name: {itemData.name}");
        Debug.Log($"Item Type: {itemData.itemType}");
        Debug.Log($"BuyPrice: {itemData.buyPrice}");
        Debug.Log($"Sell Price: {itemData.sellPrice}");
        Debug.Log($"Stackable   : {itemData.canStack}");
    }
}