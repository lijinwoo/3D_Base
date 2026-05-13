using UnityEngine;

namespace SystemicOverload.Data
{
    /// <summary>
    /// 상자형 상호작용 오브젝트 데이터입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "SystemicOverload/Gameplay/Chest Definition", fileName = "ChestDefinition")]
    public sealed class ChestDefinitionSO : InteractableDefinitionSO
    {
        protected override string DefaultId => "interactable.chest.undefined";
    }
}
