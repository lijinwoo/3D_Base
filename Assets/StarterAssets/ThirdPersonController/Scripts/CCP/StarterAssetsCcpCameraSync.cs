using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    /// <summary>
    /// 한국어 주석: StarterAssets Look 입력으로 Cinemachine 타깃 회전을 갱신합니다.
    /// </summary>
    [AddComponentMenu("Starter Assets/CCP/Starter Assets CCP Camera Sync")]
    public sealed class StarterAssetsCcpCameraSync : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private StarterAssetsInputs starterAssetsInputs;

        [SerializeField]
        private Transform cameraTarget;

#if ENABLE_INPUT_SYSTEM
        [SerializeField]
        private PlayerInput playerInput;
#endif

        [Header("Camera Angles")]
        [SerializeField]
        private float topClamp = 70.0f;

        [SerializeField]
        private float bottomClamp = -30.0f;

        [SerializeField]
        private float cameraAngleOverride;

        [SerializeField]
        private bool lockCameraPosition;

        private const float LookThreshold = 0.01f;

        private float yaw;
        private float pitch;

        private void Awake()
        {
            if (starterAssetsInputs == null)
            {
                starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            }

            if (cameraTarget != null)
            {
                yaw = cameraTarget.rotation.eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (starterAssetsInputs == null || cameraTarget == null)
            {
                return;
            }

            if (starterAssetsInputs.look.sqrMagnitude >= LookThreshold && !lockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse() ? 1.0f : Time.deltaTime;
                yaw += starterAssetsInputs.look.x * deltaTimeMultiplier;
                pitch += starterAssetsInputs.look.y * deltaTimeMultiplier;
            }

            yaw = ClampAngle(yaw, float.MinValue, float.MaxValue);
            pitch = ClampAngle(pitch, bottomClamp, topClamp);

            cameraTarget.rotation = Quaternion.Euler(pitch + cameraAngleOverride, yaw, 0.0f);
        }

        public void Configure(StarterAssetsInputs configuredInputs, Transform configuredCameraTarget)
        {
            starterAssetsInputs = configuredInputs;
            cameraTarget = configuredCameraTarget;
            if (cameraTarget != null)
            {
                yaw = cameraTarget.rotation.eulerAngles.y;
            }
        }

        private bool IsCurrentDeviceMouse()
        {
#if ENABLE_INPUT_SYSTEM
            return playerInput != null && playerInput.currentControlScheme == "KeyboardMouse";
#else
            return false;
#endif
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
            {
                angle += 360f;
            }

            if (angle > 360f)
            {
                angle -= 360f;
            }

            return Mathf.Clamp(angle, min, max);
        }
    }
}
