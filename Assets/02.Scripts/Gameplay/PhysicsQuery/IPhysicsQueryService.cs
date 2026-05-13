using UnityEngine;

namespace SystemicOverload.PhysicsQuery
{
    /// <summary>
    /// Physics 판정만 담당하는 Query 서비스 계약입니다.
    /// </summary>
    public interface IPhysicsQueryService
    {
        bool TryRaycast(
            Ray ray,
            float maxDistance,
            LayerMask layerMask,
            out RaycastHit hitInfo,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore,
            Transform ignoredRoot = null);

        bool TryRaycast(
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            LayerMask layerMask,
            out RaycastHit hitInfo,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore,
            Transform ignoredRoot = null);

        bool TrySphereCast(
            Ray ray,
            float radius,
            float maxDistance,
            LayerMask layerMask,
            out RaycastHit hitInfo,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore,
            Transform ignoredRoot = null);

        bool TryCapsuleCast(
            Vector3 point1,
            Vector3 point2,
            float radius,
            Vector3 direction,
            float maxDistance,
            LayerMask layerMask,
            out RaycastHit hitInfo,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore,
            Transform ignoredRoot = null);

        Collider[] OverlapSphere(
            Vector3 position,
            float radius,
            LayerMask layerMask,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore);

        int OverlapSphereNonAlloc(
            Vector3 position,
            float radius,
            Collider[] results,
            LayerMask layerMask,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore);
    }
}
