using UnityEngine;
using UnityEngine.InputSystem;

namespace SystemicOverload.Gameplay.Interaction
{
    /// <summary>
    /// 감지된 상호작용 대상과 입력(E/F) 실행을 연결합니다.
    /// </summary>
    [RequireComponent(typeof(InteractionDetector))]
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerInteractionController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string interactActionName = "Interact";

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = true;

        private InteractionDetector interactionDetector;
        private IInteractable lastTargetInteractable;
        private Collider lastTargetCollider;
        private InteractionHit currentHit;
        private InputAction interactAction;

        private void Awake()
        {
            interactionDetector = GetComponent<InteractionDetector>();
            playerInput ??= GetComponent<PlayerInput>();
            ResolveInputAction();
        }

        private void OnEnable()
        {
            ResolveInputAction();
        }

        private void Update()
        {
            if (interactAction == null)
            {
                ResolveInputAction();
            }

            bool hasTarget = interactionDetector.TryDetect(out currentHit);
            HandleTargetChanged(hasTarget ? currentHit.Interactable : null, hasTarget ? currentHit.HitCollider : null);

            if (!hasTarget)
            {
                return;
            }

            if (!WasInteractPressedThisFrame())
            {
                return;
            }

            InteractionContext interactionContext = new InteractionContext(gameObject, transform, currentHit.HitPoint, currentHit.HitDistance);
            if (!currentHit.Interactable.CanInteract(in interactionContext))
            {
                return;
            }

            currentHit.Interactable.Interact(in interactionContext);
        }

        private void HandleTargetChanged(IInteractable nextInteractable, Collider nextCollider)
        {
            if (ReferenceEquals(lastTargetInteractable, nextInteractable) && lastTargetCollider == nextCollider)
            {
                return;
            }

            lastTargetInteractable = nextInteractable;
            lastTargetCollider = nextCollider;

            if (!enableDebugLog)
            {
                return;
            }

            if (nextInteractable == null)
            {
                Debug.Log("[PlayerInteractionController] 상호작용 대상이 없습니다.");
                return;
            }

            string colliderName = nextCollider != null ? nextCollider.name : "UnknownCollider";
            Debug.Log($"[PlayerInteractionController] 대상 감지: {nextInteractable.InteractionLabel} ({colliderName})");
        }

        private bool WasInteractPressedThisFrame()
        {
            return interactAction != null && interactAction.WasPressedThisFrame();
        }

        private void ResolveInputAction()
        {
            if (playerInput == null || playerInput.actions == null)
            {
                interactAction = null;
                return;
            }

            InputActionMap targetMap = playerInput.actions.FindActionMap(actionMapName, false);
            interactAction = targetMap?.FindAction(interactActionName, false);
        }
    }
}
