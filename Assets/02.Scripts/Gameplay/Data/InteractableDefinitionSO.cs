using UnityEngine;

namespace SystemicOverload.Data
{
    /// <summary>
    /// 상호작용 오브젝트 공통 정의 데이터입니다.
    /// </summary>
    public abstract class InteractableDefinitionSO : ScriptableObject
    {
        [SerializeField] private string interactableId = "interactable.undefined";
        [SerializeField] private string promptText = "[E] Interact";
        [SerializeField] private string interactionLogMessage = "Interaction executed";

        public string InteractableId => interactableId;
        public string PromptText => promptText;
        public string InteractionLogMessage => interactionLogMessage;

        protected virtual string DefaultId => "interactable.undefined";

        protected virtual void OnValidate()
        {
            interactableId = string.IsNullOrWhiteSpace(interactableId) ? DefaultId : interactableId.Trim();
            promptText = string.IsNullOrWhiteSpace(promptText) ? "[E] Interact" : promptText.Trim();
            interactionLogMessage = string.IsNullOrWhiteSpace(interactionLogMessage)
                ? "Interaction executed"
                : interactionLogMessage.Trim();
        }
    }
}
