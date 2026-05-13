using UnityEngine;

namespace SystemicOverload.Gameplay.Interaction
{
    /// <summary>
    /// RPG의 NPC 상호작용 흐름을 검증하기 위한 더미 구현체입니다.
    /// </summary>
    public sealed class NpcInteractableDummy : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "NPC_Villager_Dummy";
        [SerializeField] private string dialogueLine = "안녕하세요, 여행자님. 오늘도 안전한 모험 되세요.";

        public string InteractionLabel => $"Talk: {npcName}";

        public bool CanInteract(in InteractionContext interactionContext)
        {
            return true;
        }

        public void Interact(in InteractionContext interactionContext)
        {
            Debug.Log($"[NpcInteractableDummy] {npcName}: {dialogueLine}");
        }
    }
}
