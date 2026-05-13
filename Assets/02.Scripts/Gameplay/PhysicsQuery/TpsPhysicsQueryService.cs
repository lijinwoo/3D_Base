using System;
using UnityEngine;

namespace SystemicOverload.PhysicsQuery
{
    /// <summary>
    /// 판정 로직만 전담하는 기본 Physics Query 서비스 구현입니다.
    /// </summary>
    public sealed class TpsPhysicsQueryService : MonoBehaviour, IPhysicsQueryService
    {
        public bool TryRaycast(
            Ray ray,
            float maxDistance,
            LayerMask layerMask,
            out RaycastHit hitInfo,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore,
            Transform ignoredRoot = null)
        {
            if (ignoredRoot == null)
            {
                return Physics.Raycast(ray, out hitInfo, maxDistance, layerMask, triggerInteraction);
            }

            RaycastHit[] allHits = Physics.RaycastAll(ray, maxDistance, layerMask, triggerInteraction);
            return TryResolveNearestValidHit(allHits, ignoredRoot, out hitInfo);
        }

        public bool TryRaycast(
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            LayerMask layerMask,
            out RaycastHit hitInfo,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore,
            Transform ignoredRoot = null)
        {
            if (ignoredRoot == null)
            {
                return Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask, triggerInteraction);
            }

            RaycastHit[] allHits = Physics.RaycastAll(origin, direction, maxDistance, layerMask, triggerInteraction);
            return TryResolveNearestValidHit(allHits, ignoredRoot, out hitInfo);
        }

        public bool TrySphereCast(
            Ray ray,
            float radius,
            float maxDistance,
            LayerMask layerMask,
            out RaycastHit hitInfo,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore,
            Transform ignoredRoot = null)
        {
            if (ignoredRoot == null)
            {
                return Physics.SphereCast(ray, radius, out hitInfo, maxDistance, layerMask, triggerInteraction);
            }

            RaycastHit[] allHits = Physics.SphereCastAll(ray, radius, maxDistance, layerMask, triggerInteraction);
            return TryResolveNearestValidHit(allHits, ignoredRoot, out hitInfo);
        }

        public bool TryCapsuleCast(
            Vector3 point1,
            Vector3 point2,
            float radius,
            Vector3 direction,
            float maxDistance,
            LayerMask layerMask,
            out RaycastHit hitInfo,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore,
            Transform ignoredRoot = null)
        {
            if (ignoredRoot == null)
            {
                return Physics.CapsuleCast(point1, point2, radius, direction, out hitInfo, maxDistance, layerMask, triggerInteraction);
            }

            RaycastHit[] allHits = Physics.CapsuleCastAll(
                point1,
                point2,
                radius,
                direction,
                maxDistance,
                layerMask,
                triggerInteraction);
            return TryResolveNearestValidHit(allHits, ignoredRoot, out hitInfo);
        }

        public Collider[] OverlapSphere(
            Vector3 position,
            float radius,
            LayerMask layerMask,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            return Physics.OverlapSphere(position, radius, layerMask, triggerInteraction);
        }

        public int OverlapSphereNonAlloc(
            Vector3 position,
            float radius,
            Collider[] results,
            LayerMask layerMask,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            return Physics.OverlapSphereNonAlloc(position, radius, results, layerMask, triggerInteraction);
        }

        /// <summary>
        /// 자신 계층을 제외하고 가장 가까운 유효 히트를 선택합니다.
        /// </summary>
        private static bool TryResolveNearestValidHit(RaycastHit[] candidates, Transform ignoredRoot, out RaycastHit hitInfo)
        {
            hitInfo = default;
            if (candidates == null || candidates.Length == 0)
            {
                return false;
            }

            Array.Sort(candidates, (left, right) => left.distance.CompareTo(right.distance));
            for (int index = 0; index < candidates.Length; index++)
            {
                RaycastHit candidate = candidates[index];
                if (candidate.collider == null)
                {
                    continue;
                }

                if (ignoredRoot != null && candidate.collider.transform.IsChildOf(ignoredRoot))
                {
                    continue;
                }

                hitInfo = candidate;
                return true;
            }

            return false;
        }
    }
}
