using UnityEngine;

namespace SystemicOverload.Phase1
{
    /// <summary>
    /// Phase 1 검증을 위한 간단한 Top-down 카메라 추적 컴포넌트입니다.
    /// </summary>
    public sealed class TopDownCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 followOffset = new Vector3(0.0f, 14.0f, -8.0f);
        [SerializeField] private float positionSharpness = 10.0f;
        [SerializeField] private bool lockRotation = true;
        [SerializeField] private Vector3 lockedEulerAngles = new Vector3(60.0f, 0.0f, 0.0f);

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                return;
            }

            // 카메라 이동을 보간해 화면 흔들림 없이 타겟을 안정적으로 추적합니다.
            Vector3 targetPosition = followTarget.position + followOffset;
            float blendFactor = 1.0f - Mathf.Exp(-positionSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPosition, blendFactor);

            if (lockRotation)
            {
                transform.rotation = Quaternion.Euler(lockedEulerAngles);
            }
            else
            {
                transform.LookAt(followTarget.position);
            }
        }
    }
}
