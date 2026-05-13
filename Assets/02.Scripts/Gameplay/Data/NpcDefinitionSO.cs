using UnityEngine;

namespace SystemicOverload.Data
{
    /// <summary>
    /// NPC형 상호작용 오브젝트 데이터입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "SystemicOverload/Gameplay/Npc Definition", fileName = "NpcDefinition")]
    public sealed class NpcDefinitionSO : InteractableDefinitionSO
    {
        protected override string DefaultId => "interactable.npc.undefined";
    }
}
