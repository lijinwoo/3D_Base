using UnityEngine;

namespace SystemicOverload.Phase1
{
    /// <summary>
    /// 이동과 회전을 담당하는 Phase 1용 기본 Movement 컴포넌트입니다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(InputProvider))]
    public sealed class MovementComponent : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6.0f;
        [SerializeField] private float accelerationSharpness = 14.0f;
        [SerializeField] private float decelerationSharpness = 18.0f;
        [SerializeField] private float gravity = -25.0f;

        [Header("Rotation")]
        [SerializeField] private float rotationSharpness = 16.0f;
        [SerializeField] private bool useMouseRaycastRotation = false;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private LayerMask groundLayerMask = ~0;
        [SerializeField] private float aimRayMaxDistance = 300.0f;
        [SerializeField] private Phase1OrbitCameraController orbitCameraController;

        private CharacterController characterController;
        private InputProvider inputProvider;
        private Vector3 currentPlanarVelocity;
        private float verticalVelocity;

        public Vector3 CurrentPlanarVelocity => currentPlanarVelocity;
        public Vector3 LastAimPoint { get; private set; }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputProvider = GetComponent<InputProvider>();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0.0f)
            {
                return;
            }

            UpdatePlanarMovement(deltaTime);
            UpdateVerticalMovement(deltaTime);
            UpdateRotation(deltaTime);
        }

        private void UpdatePlanarMovement(float deltaTime)
        {
            Camera targetCamera = ResolveAimCamera();
            if (targetCamera == null)
            {
                return;
            }

            Vector2 moveInput = inputProvider.MoveInput;

            // 카메라 기준 평면 축으로 이동 벡터를 계산해 조작 일관성을 유지합니다.
            Vector3 cameraForwardOnPlane = Vector3.ProjectOnPlane(targetCamera.transform.forward, Vector3.up).normalized;
            Vector3 cameraRightOnPlane = Vector3.ProjectOnPlane(targetCamera.transform.right, Vector3.up).normalized;
            Vector3 desiredPlanarDirection = cameraForwardOnPlane * moveInput.y + cameraRightOnPlane * moveInput.x;
            Vector3 desiredPlanarVelocity = desiredPlanarDirection * moveSpeed;

            float smoothingSharpness = desiredPlanarVelocity.sqrMagnitude > 0.0001f
                ? accelerationSharpness
                : decelerationSharpness;
            float blendFactor = 1.0f - Mathf.Exp(-smoothingSharpness * deltaTime);
            currentPlanarVelocity = Vector3.Lerp(currentPlanarVelocity, desiredPlanarVelocity, blendFactor);
        }

        private void UpdateVerticalMovement(float deltaTime)
        {
            if (characterController.isGrounded && verticalVelocity < 0.0f)
            {
                // 지면 접지 상태에서 미세하게 아래로 유지해 들뜸 현상을 방지합니다.
                verticalVelocity = -2.0f;
            }
            else
            {
                verticalVelocity += gravity * deltaTime;
            }

            Vector3 frameVelocity = currentPlanarVelocity + Vector3.up * verticalVelocity;
            characterController.Move(frameVelocity * deltaTime);
        }

        private void UpdateRotation(float deltaTime)
        {
            if (inputProvider == null)
            {
                return;
            }

            // RMB 입력 중에는 카메라 Yaw와 캐릭터 Yaw를 동기화합니다.
            if (inputProvider.IsRightMouseHeld && TryRotateTowardCameraYaw(deltaTime))
            {
                return;
            }

            // Free Look(LMB) 중에는 캐릭터 방향을 변경하지 않습니다.
            if (inputProvider.IsLeftMouseHeld)
            {
                return;
            }

            if (useMouseRaycastRotation)
            {
                RotateTowardPointer(deltaTime);
            }
        }

        private bool TryRotateTowardCameraYaw(float deltaTime)
        {
            if (orbitCameraController == null)
            {
                orbitCameraController = FindFirstObjectByType<Phase1OrbitCameraController>();
            }

            Camera targetCamera = ResolveAimCamera();
            if (targetCamera == null)
            {
                return false;
            }

            float targetYaw = orbitCameraController != null
                ? orbitCameraController.CurrentYaw
                : targetCamera.transform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0.0f, targetYaw, 0.0f);
            float blendFactor = 1.0f - Mathf.Exp(-rotationSharpness * deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, blendFactor);
            return true;
        }

        private void RotateTowardPointer(float deltaTime)
        {
            Camera targetCamera = ResolveAimCamera();
            if (targetCamera == null)
            {
                return;
            }

            Ray aimRay = targetCamera.ScreenPointToRay(inputProvider.PointerScreenPosition);
            if (!Physics.Raycast(aimRay, out RaycastHit hitInfo, aimRayMaxDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            LastAimPoint = hitInfo.point;

            Vector3 lookDirection = hitInfo.point - transform.position;
            lookDirection.y = 0.0f;
            if (lookDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            float blendFactor = 1.0f - Mathf.Exp(-rotationSharpness * deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, blendFactor);
        }

        private Camera ResolveAimCamera()
        {
            if (aimCamera != null)
            {
                return aimCamera;
            }

            aimCamera = Camera.main;
            return aimCamera;
        }
    }
}
