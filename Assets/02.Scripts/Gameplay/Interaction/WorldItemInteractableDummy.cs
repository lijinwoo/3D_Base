using UnityEngine;

namespace SystemicOverload.Gameplay.Interaction
{
    /// <summary>
    /// 월드 아이템 상호작용(획득)을 검증하기 위한 더미 구현체입니다.
    /// </summary>
    public sealed class WorldItemInteractableDummy : MonoBehaviour, IInteractable
    {
        [SerializeField] private string itemName = "InteractableItem_HealthPotion_Dummy";
        [SerializeField] private bool destroyOnPickup = true;

        private bool hasBeenPickedUp;

        public string InteractionLabel => hasBeenPickedUp ? $"{itemName} (Picked)" : $"Pick Up: {itemName}";

        public bool CanInteract(in InteractionContext interactionContext)
        {
            return !hasBeenPickedUp;
        }

        public void Interact(in InteractionContext interactionContext)
        {
            if (hasBeenPickedUp)
            {
                return;
            }

            hasBeenPickedUp = true;
            Debug.Log($"[WorldItemInteractableDummy] 아이템 획득: {itemName} by {interactionContext.InteractorObject.name}");

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
