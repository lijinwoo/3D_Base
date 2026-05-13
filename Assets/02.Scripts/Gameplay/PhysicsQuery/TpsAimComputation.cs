using UnityEngine;

namespace SystemicOverload.PhysicsQuery
{
    /// <summary>
    /// 조준과 발사에 필요한 순수 계산 유틸리티입니다.
    /// </summary>
    public static class TpsAimComputation
    {
        public static bool TryBuildCenterRay(Camera sourceCamera, out Ray centerRay)
        {
            centerRay = default;
            if (sourceCamera == null)
            {
                return false;
            }

            centerRay = sourceCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            return true;
        }

        public static Vector3 ResolveFallbackAimPoint(Ray centerRay, float fallbackDistance)
        {
            return centerRay.origin + centerRay.direction * Mathf.Max(0.0f, fallbackDistance);
        }

        public static void BuildMuzzleCast(
            Vector3 muzzleOrigin,
            Vector3 aimPoint,
            Vector3 fallbackForward,
            float maxRange,
            float paddingDistance,
            out Vector3 castDirection,
            out float castDistance)
        {
            Vector3 muzzleToAim = aimPoint - muzzleOrigin;
            if (muzzleToAim.sqrMagnitude < 0.0001f)
            {
                muzzleToAim = fallbackForward;
            }

            castDirection = muzzleToAim.normalized;
            castDistance = Mathf.Min(maxRange, muzzleToAim.magnitude + paddingDistance);
            if (castDistance <= 0.0001f)
            {
                castDistance = maxRange;
            }
        }
    }
}
