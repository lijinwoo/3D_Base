using SystemicOverload.Data;
using UnityEngine;

namespace SystemicOverload.Interaction
{
    /// <summary>
    /// ScriptableObject 정의를 읽어 상호작용 프롬프트와 실행 로그를 제공하는 런타임 컴포넌트입니다.
    /// </summary>
    public sealed class InteractableComponent : MonoBehaviour, IInteractable
    {
        [Header("Definition")]
        [SerializeField] private InteractableDefinitionSO interactableDefinition;
        [SerializeField] private bool useDefinitionData = true;

        [Header("Fallback")]
        [SerializeField] private string promptText = "[E] 상호작용";
        [SerializeField] private string interactionLogMessage = "상호작용 실행";

        public string GetPrompt()
        {
            if (useDefinitionData && interactableDefinition != null)
            {
                return interactableDefinition.PromptText;
            }

            return promptText;
        }

        public void Interact(GameObject actor)
        {
            string actorName = actor != null ? actor.name : "Unknown";
            string logMessage = interactionLogMessage;
            string interactionId = "interactable.local";
            if (useDefinitionData && interactableDefinition != null)
            {
                logMessage = interactableDefinition.InteractionLogMessage;
                interactionId = interactableDefinition.InteractableId;
            }

            Debug.Log($"[{interactionId}] {logMessage} / Actor: {actorName} / Target: {name}");
        }
    }
}
