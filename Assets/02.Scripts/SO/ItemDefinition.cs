using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Data/Item Definition")]
public class ItemDefinition : ScriptableObject,IIdentifiable
{
   [SerializeField] private string itemId;
   [SerializeField] private string itemName;
   [SerializeField] private ItemType itemType;
   [SerializeField] private Sprite icon;
    
    public string Id => itemId; 
    public string Name => itemName;
    public ItemType ItemType => itemType;
    public Sprite Icon => icon;

    private void Reset()
    {
        if (string.IsNullOrEmpty(itemId))
        {
            itemId = Guid.NewGuid().ToString();
        }
    }
}


