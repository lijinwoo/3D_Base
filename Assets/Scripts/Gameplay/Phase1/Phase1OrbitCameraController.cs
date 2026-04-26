using System.Collections.Generic;
using UnityEngine;

namespace SystemicOverload.Phase1
{
    /// <summary>
    /// Phase 1 전용 Orbit/SpringArm 카메라 컨트롤러입니다.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class Phase1OrbitCameraController : MonoBehaviour
    {
        public enum AutoFollowMode
        {
            Always,
            MovingOnly,
            Manual
        }

        [Header("Target")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private InputProvider inputProvider;
        [SerializeField] private MovementComponent movementComponent;
        [SerializeField] private Vector3 pivotOffset = new Vector3(0.0f, 1.7f, 0.0f);

        [Header("Look")]
        [SerializeField] private float yawSensitivity = 0.16f;
        [SerializeField] private float pitchSensitivity = 0.11f;
        [SerializeField] private float minPitch = -35.0f;
        [SerializeField] private float maxPitch = 75.0f;

        [Header("Zoom")]
        [SerializeField] private float defaultZoomDistance = 7.0f;
        [SerializeField] private float maxZoomDistance = 14.0f;
        [SerializeField] private float zoomSpeed = 5.0f;
        [SerializeField] private float zoomSmoothing = 16.0f;
        [SerializeField] private float firstPersonThreshold = 0.05f;

        [Header("Auto Follow")]
        [SerializeField] private AutoFollowMode autoFollowMode = AutoFollowMode.MovingOnly;
        [SerializeField] private float autoFollowSharpness = 4.0f;

        [Header("Spring Arm Collision")]
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private float collisionSphereRadius = 0.2f;
        [SerializeField] private float collisionBuffer = 0.12f;

        [Header("Environment Transition")]
        [SerializeField] private float waterSurfaceHeight = -1000.0f;
        [SerializeField] private List<GameObject> underwaterEffectRoots = new List<GameObject>();

        private Camera cachedCamera;
        private float currentYaw;
        private float currentPitch = 22.0f;
        private float targetZoomDistance;
        private float currentZoomDistance;
        private bool wasInFirstPerson;
        private bool isUnderwaterActive;
        private Renderer[] cachedTargetRenderers;

        public Transform FollowTarget => followTarget;
        public float CurrentYaw => currentYaw;
        public Vector3 PivotPosition => followTarget == null ? transform.position : followTarget.position + pivotOffset;

        private void Awake()
        {
            cachedCamera = GetComponent<Camera>();
            targetZoomDistance = Mathf.Clamp(defaultZoomDistance, 0.0f, maxZoomDistance);
            currentZoomDistance = targetZoomDistance;

            if (followTarget != null)
            {
                currentYaw = followTarget.eulerAngles.y;
                CacheTargetRenderers();
            }
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                return;
            }

            ResolveReferences();
            HandleLookInput();
            HandleZoomInput();
            ApplyAutoFollow();
            UpdateCameraTransform();
            UpdateFirstPersonRendering();
            UpdateEnvironmentTransition();
        }

        private void ResolveReferences()
        {
            if (inputProvider == null)
            {
                inputProvider = followTarget.GetComponent<InputProvider>();
            }

            if (movementComponent == null)
            {
                movementComponent = followTarget.GetComponent<MovementComponent>();
            }
        }

        private void HandleLookInput()
        {
            if (inputProvider == null)
            {
                return;
            }

            Vector2 lookDelta = inputProvider.LookDelta;
            bool hasDragInput = inputProvider.IsLeftMouseHeld || inputProvider.IsRightMouseHeld;
            if (!hasDragInput)
            {
                return;
            }

            currentYaw += lookDelta.x * yawSensitivity;
            currentPitch = Mathf.Clamp(currentPitch - lookDelta.y * pitchSensitivity, minPitch, maxPitch);
        }

        private void HandleZoomInput()
        {
            if (inputProvider != null)
            {
                targetZoomDistance = Mathf.Clamp(targetZoomDistance - inputProvider.ZoomDelta * zoomSpeed, 0.0f, maxZoomDistance);
            }

            float zoomBlend = 1.0f - Mathf.Exp(-zoomSmoothing * Time.deltaTime);
            currentZoomDistance = Mathf.Lerp(currentZoomDistance, targetZoomDistance, zoomBlend);
        }

        private void ApplyAutoFollow()
        {
            if (inputProvider == null || followTarget == null)
            {
                return;
            }

            bool isDraggingCamera = inputProvider.IsLeftMouseHeld || inputProvider.IsRightMouseHeld;
            if (isDraggingCamera)
            {
                return;
            }

            bool shouldFollow = autoFollowMode == AutoFollowMode.Always;
            if (autoFollowMode == AutoFollowMode.MovingOnly && movementComponent != null)
            {
                shouldFollow = movementComponent.CurrentPlanarVelocity.sqrMagnitude > 0.0001f;
            }

            if (!shouldFollow)
            {
                return;
            }

            float targetYaw = followTarget.eulerAngles.y;
            float nextYaw = Mathf.LerpAngle(currentYaw, targetYaw, 1.0f - Mathf.Exp(-autoFollowSharpness * Time.deltaTime));
            currentYaw = nextYaw;
        }

        private void UpdateCameraTransform()
        {
            Vector3 pivotPosition = followTarget.position + pivotOffset;
            Quaternion lookRotation = Quaternion.Euler(currentPitch, currentYaw, 0.0f);
            Vector3 desiredDirection = lookRotation * Vector3.back;
            float resolvedDistance = ResolveCollisionDistance(pivotPosition, desiredDirection, currentZoomDistance);
            Vector3 cameraPosition = pivotPosition + desiredDirection * resolvedDistance;

            transform.position = cameraPosition;
            transform.rotation = lookRotation;
        }

        private float ResolveCollisionDistance(Vector3 pivotPosition, Vector3 desiredDirection, float desiredDistance)
        {
            if (desiredDistance <= 0.0001f)
            {
                return 0.0f;
            }

            if (Physics.SphereCast(pivotPosition, collisionSphereRadius, desiredDirection, out RaycastHit hitInfo, desiredDistance, collisionMask, QueryTriggerInteraction.Ignore))
            {
                float overrideDistance = Mathf.Max(hitInfo.distance - collisionBuffer, 0.0f);
                return overrideDistance;
            }

            return desiredDistance;
        }

        private void UpdateFirstPersonRendering()
        {
            bool shouldEnableFirstPerson = targetZoomDistance <= firstPersonThreshold;
            if (shouldEnableFirstPerson == wasInFirstPerson)
            {
                return;
            }

            wasInFirstPerson = shouldEnableFirstPerson;
            bool rendererEnabled = !shouldEnableFirstPerson;

            if (cachedTargetRenderers == null || cachedTargetRenderers.Length == 0)
            {
                CacheTargetRenderers();
            }

            foreach (Renderer targetRenderer in cachedTargetRenderers)
            {
                if (targetRenderer != null)
                {
                    targetRenderer.enabled = rendererEnabled;
                }
            }
        }

        private void UpdateEnvironmentTransition()
        {
            bool shouldActivateUnderwater = transform.position.y < waterSurfaceHeight;
            if (shouldActivateUnderwater == isUnderwaterActive)
            {
                return;
            }

            isUnderwaterActive = shouldActivateUnderwater;
            foreach (GameObject effectRoot in underwaterEffectRoots)
            {
                if (effectRoot != null)
                {
                    effectRoot.SetActive(shouldActivateUnderwater);
                }
            }
        }

        private void CacheTargetRenderers()
        {
            if (followTarget == null)
            {
                cachedTargetRenderers = new Renderer[0];
                return;
            }

            cachedTargetRenderers = followTarget.GetComponentsInChildren<Renderer>(true);
        }
    }
}
