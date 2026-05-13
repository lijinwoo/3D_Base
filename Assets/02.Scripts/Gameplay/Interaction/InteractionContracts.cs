using UnityEngine;

namespace SystemicOverload.Gameplay.Interaction
{
    /// <summary>
    /// 상호작용 시 전달되는 실행 컨텍스트입니다.
    /// </summary>
    public readonly struct InteractionContext
    {
        public InteractionContext(GameObject interactorObject, Transform interactorTransform, Vector3 hitPoint, float hitDistance)
        {
            InteractorObject = interactorObject;
            InteractorTransform = interactorTransform;
            HitPoint = hitPoint;
            HitDistance = hitDistance;
        }

        public GameObject InteractorObject { get; }
        public Transform InteractorTransform { get; }
        public Vector3 HitPoint { get; }
        public float HitDistance { get; }
    }

    /// <summary>
    /// 플레이어가 감지 및 실행 가능한 상호작용 계약입니다.
    /// </summary>
    public interface IInteractable
    {
        string InteractionLabel { get; }
        bool CanInteract(in InteractionContext interactionContext);
        void Interact(in InteractionContext interactionContext);
    }
}
