using UnityEngine;

namespace SystemicOverload.PhysicsQuery
{
    /// <summary>
    /// AOE 계열 스킬의 거리 기반 데미지 연산을 담당합니다.
    /// </summary>
    public static class TpsAoeComputation
    {
        public static float ComputeRadialDamage(float distance, float radius, float maxDamage)
        {
            float safeRadius = Mathf.Max(0.0001f, radius);
            float damageRatio = 1.0f - Mathf.Clamp01(distance / safeRadius);
            return Mathf.Max(0.0f, maxDamage * damageRatio);
        }
    }
}
