using SystemicOverload.Phase1;
using SystemicOverload.PhysicsQuery;
using UnityEngine;

namespace SystemicOverload.Interaction
{
    /// <summary>
    /// 카메라 중앙 Raycast로 상호작용 대상을 탐색하고 입력 시 실행합니다.
    /// </summary>
    [RequireComponent(typeof(InputProvider))]
    public sealed class TpsRayInteractor : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float interactDistance = 4.0f;
        [SerializeField] private LayerMask interactMask = ~0;
        [SerializeField] private bool drawDebugRay = true;
        [SerializeField] private TpsPhysicsQueryService physicsQueryService;

        private InputProvider inputProvider;
        private IInteractable currentTarget;
        private string lastPrompt;

        public string CurrentPrompt => currentTarget?.GetPrompt() ?? string.Empty;

        private void Awake()
        {
            inputProvider = GetComponent<InputProvider>();
            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }

            physicsQueryService ??= GetComponent<TpsPhysicsQueryService>();
            if (physicsQueryService == null)
            {
                physicsQueryService = gameObject.AddComponent<TpsPhysicsQueryService>();
            }
        }

        private void OnValidate()
        {
            interactDistance = Mathf.Max(0.2f, interactDistance);
        }

        private void Update()
        {
            currentTarget = ScanInteractable();
            UpdatePromptState();
            if (currentTarget == null)
            {
                return;
            }

            if (inputProvider.WasInteractPressedThisFrame)
            {
                currentTarget.Interact(gameObject);
            }
        }

        /// <summary>
        /// 판정 전용: 화면 중앙 Raycast로 상호작용 가능 대상을 반환합니다.
        /// </summary>
        private IInteractable ScanInteractable()
        {
            if (aimCamera == null)
            {
                aimCamera = Camera.main;
                if (aimCamera == null)
                {
                    return null;
                }
            }

            if (!TpsAimComputation.TryBuildCenterRay(aimCamera, out Ray centerRay))
            {
                return null;
            }

            if (physicsQueryService.TryRaycast(
                    centerRay,
                    interactDistance,
                    interactMask,
                    out RaycastHit raycastHit,
                    QueryTriggerInteraction.Ignore,
                    transform))
            {
                if (drawDebugRay)
                {
                    Debug.DrawRay(centerRay.origin, centerRay.direction * raycastHit.distance, Color.yellow);
                }

                return raycastHit.collider.GetComponentInParent<IInteractable>();
            }

            if (drawDebugRay)
            {
                Debug.DrawRay(centerRay.origin, centerRay.direction * interactDistance, Color.white);
            }

            return null;
        }

        /// <summary>
        /// 연산/표시 전용: 프롬프트 변경을 감지해 출력 상태를 갱신합니다.
        /// </summary>
        private void UpdatePromptState()
        {
            if (currentTarget == null)
            {
                lastPrompt = string.Empty;
                return;
            }

            string prompt = currentTarget.GetPrompt();
            if (!string.IsNullOrEmpty(prompt) && prompt != lastPrompt)
            {
                Debug.Log(prompt);
            }

            lastPrompt = prompt;
        }
    }
}
