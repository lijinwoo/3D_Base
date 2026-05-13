using UnityEngine;

namespace SystemicOverload.Gameplay.Interaction
{
    public enum InteractionCastMode
    {
        Raycast = 0,
        SphereCast = 1,
        OverlapSphere = 2
    }

    /// <summary>
    /// Raycast/ShapeCast/Overlap 기반으로 상호작용 대상을 감지합니다.
    /// </summary>
    public sealed class InteractionDetector : MonoBehaviour
    {
        private const float MinDirectionEpsilon = 0.0001f;

        [Header("Cast")]
        [SerializeField] private InteractionCastMode castMode = InteractionCastMode.Raycast;
        [SerializeField] private Transform castOriginOverride;
        [SerializeField] private Transform ownerRoot;
        [SerializeField] private LayerMask detectLayerMask = ~0;
        [SerializeField] private float maxDistance = 4.0f;
        [SerializeField] private float sphereCastRadius = 0.5f;
        [SerializeField] private float overlapRadius = 2.0f;
        [SerializeField] private QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Debug View")]
        [SerializeField] private bool drawDebugLines = true;
        [SerializeField] private float debugDrawDuration = 0.08f;
        [SerializeField] private Color hitDebugColor = Color.green;
        [SerializeField] private Color missDebugColor = Color.yellow;
        [SerializeField] private Color gizmoColor = Color.cyan;

        private readonly Collider[] overlapResults = new Collider[24];

        public InteractionCastMode CastMode
        {
            get => castMode;
            set => castMode = value;
        }

        public bool TryDetect(out InteractionHit interactionHit)
        {
            ResolveCastPose(out Vector3 origin, out Vector3 direction);
            Vector3 overlapOrigin = ResolveOwnerOrigin();
            Vector3 overlapDirection = ResolveOwnerForward(direction);

            switch (castMode)
            {
                case InteractionCastMode.SphereCast:
                    return TrySphereCast(origin, direction, out interactionHit);
                case InteractionCastMode.OverlapSphere:
                    return TryOverlapSphere(overlapOrigin, overlapDirection, out interactionHit);
                default:
                    return TryRaycast(origin, direction, out interactionHit);
            }
        }

        private bool TryRaycast(Vector3 origin, Vector3 direction, out InteractionHit interactionHit)
        {
            interactionHit = default;
            RaycastHit[] raycastHits = Physics.RaycastAll(origin, direction, maxDistance, detectLayerMask, queryTriggerInteraction);
            if (raycastHits.Length <= 0)
            {
                DrawMissLine(origin, direction, maxDistance);
                return false;
            }

            System.Array.Sort(raycastHits, (left, right) => left.distance.CompareTo(right.distance));
            for (int index = 0; index < raycastHits.Length; index++)
            {
                RaycastHit hitInfo = raycastHits[index];
                if (ShouldIgnoreCollider(hitInfo.collider))
                {
                    continue;
                }

                if (!TryResolveInteractable(hitInfo.collider, out IInteractable interactable))
                {
                    continue;
                }

                interactionHit = new InteractionHit(interactable, hitInfo.collider, hitInfo.point, hitInfo.distance);
                DrawHitLine(origin, hitInfo.point);
                return true;
            }

            DrawMissLine(origin, direction, maxDistance);
            return false;
        }

        private bool TrySphereCast(Vector3 origin, Vector3 direction, out InteractionHit interactionHit)
        {
            interactionHit = default;
            RaycastHit[] sphereHits = Physics.SphereCastAll(origin, sphereCastRadius, direction, maxDistance, detectLayerMask, queryTriggerInteraction);
            if (sphereHits.Length <= 0)
            {
                DrawMissLine(origin, direction, maxDistance);
                return false;
            }

            System.Array.Sort(sphereHits, (left, right) => left.distance.CompareTo(right.distance));
            for (int index = 0; index < sphereHits.Length; index++)
            {
                RaycastHit hitInfo = sphereHits[index];
                if (ShouldIgnoreCollider(hitInfo.collider))
                {
                    continue;
                }

                if (!TryResolveInteractable(hitInfo.collider, out IInteractable interactable))
                {
                    continue;
                }

                interactionHit = new InteractionHit(interactable, hitInfo.collider, hitInfo.point, hitInfo.distance);
                DrawHitLine(origin, hitInfo.point);
                return true;
            }

            DrawMissLine(origin, direction, maxDistance);
            return false;
        }

        private bool TryOverlapSphere(Vector3 origin, Vector3 direction, out InteractionHit interactionHit)
        {
            interactionHit = default;
            Vector3 center = origin + direction * Mathf.Min(maxDistance, overlapRadius);
            int hitCount = Physics.OverlapSphereNonAlloc(
                center,
                overlapRadius,
                overlapResults,
                detectLayerMask,
                queryTriggerInteraction);

            if (hitCount <= 0)
            {
                DrawMissLine(origin, direction, maxDistance);
                return false;
            }

            float bestScore = float.MaxValue;
            bool hasCandidate = false;
            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider = overlapResults[index];
                if (hitCollider == null || ShouldIgnoreCollider(hitCollider) || !TryResolveInteractable(hitCollider, out IInteractable interactable))
                {
                    continue;
                }

                Vector3 closestPoint = hitCollider.ClosestPoint(origin);
                Vector3 toTarget = closestPoint - origin;
                float distanceScore = toTarget.magnitude;
                float directionScore = 1.0f - Mathf.Clamp01(Vector3.Dot(direction.normalized, toTarget.normalized));
                float score = distanceScore + directionScore * 2.0f;
                if (score >= bestScore)
                {
                    continue;
                }

                bestScore = score;
                interactionHit = new InteractionHit(interactable, hitCollider, closestPoint, distanceScore);
                hasCandidate = true;
            }

            if (hasCandidate)
            {
                DrawHitLine(origin, interactionHit.HitPoint);
            }
            else
            {
                DrawMissLine(origin, direction, maxDistance);
            }

            return hasCandidate;
        }

        private bool ShouldIgnoreCollider(Collider targetCollider)
        {
            if (targetCollider == null)
            {
                return true;
            }

            Transform ignoreRoot = ownerRoot != null ? ownerRoot : transform;
            return targetCollider.transform.IsChildOf(ignoreRoot);
        }

        private bool TryResolveInteractable(Collider targetCollider, out IInteractable interactable)
        {
            interactable = null;
            if (targetCollider == null)
            {
                return false;
            }

            MonoBehaviour[] ownerComponents = targetCollider.GetComponentsInParent<MonoBehaviour>(true);
            for (int index = 0; index < ownerComponents.Length; index++)
            {
                if (ownerComponents[index] is not IInteractable foundInteractable)
                {
                    continue;
                }

                interactable = foundInteractable;
                return true;
            }

            return false;
        }

        private void ResolveCastPose(out Vector3 origin, out Vector3 direction)
        {
            if (castOriginOverride != null)
            {
                origin = castOriginOverride.position;
                direction = castOriginOverride.forward;
                return;
            }

            origin = ResolveOwnerOrigin();
            direction = ResolveOwnerForward(ownerRoot != null ? ownerRoot.forward : transform.forward);
        }

        private Vector3 ResolveOwnerOrigin()
        {
            return (ownerRoot != null ? ownerRoot.position : transform.position) + Vector3.up * 1.3f;
        }

        private Vector3 ResolveOwnerForward(Vector3 preferredDirection)
        {
            Vector3 projectedDirection = Vector3.ProjectOnPlane(preferredDirection, Vector3.up);
            if (projectedDirection.sqrMagnitude <= MinDirectionEpsilon)
            {
                return ownerRoot != null ? ownerRoot.forward : transform.forward;
            }

            return projectedDirection.normalized;
        }

        private void OnValidate()
        {
            maxDistance = Mathf.Max(0.25f, maxDistance);
            sphereCastRadius = Mathf.Max(0.05f, sphereCastRadius);
            overlapRadius = Mathf.Max(0.1f, overlapRadius);
            debugDrawDuration = Mathf.Max(0.0f, debugDrawDuration);
        }

        private void OnDrawGizmosSelected()
        {
            ResolveCastPose(out Vector3 origin, out Vector3 direction);
            direction = direction.normalized;
            if (direction.sqrMagnitude <= MinDirectionEpsilon)
            {
                direction = transform.forward;
            }

            Gizmos.color = gizmoColor;
            switch (castMode)
            {
                case InteractionCastMode.SphereCast:
                    Gizmos.DrawWireSphere(origin, sphereCastRadius);
                    Gizmos.DrawWireSphere(origin + direction * maxDistance, sphereCastRadius);
                    Gizmos.DrawLine(origin, origin + direction * maxDistance);
                    break;
                case InteractionCastMode.OverlapSphere:
                {
                    Vector3 center = origin + direction * Mathf.Min(maxDistance, overlapRadius);
                    Gizmos.DrawWireSphere(center, overlapRadius);
                    Gizmos.DrawLine(origin, center);
                    break;
                }
                default:
                    Gizmos.DrawLine(origin, origin + direction * maxDistance);
                    break;
            }
        }

        private void DrawHitLine(Vector3 origin, Vector3 hitPoint)
        {
            if (!drawDebugLines)
            {
                return;
            }

            Debug.DrawLine(origin, hitPoint, hitDebugColor, debugDrawDuration);
        }

        private void DrawMissLine(Vector3 origin, Vector3 direction, float distance)
        {
            if (!drawDebugLines)
            {
                return;
            }

            Vector3 endPoint = origin + direction.normalized * Mathf.Max(0.0f, distance);
            Debug.DrawLine(origin, endPoint, missDebugColor, debugDrawDuration);
        }
    }

    /// <summary>
    /// 감지 결과를 후속 시스템이 안전하게 사용할 수 있는 형태로 캡슐화합니다.
    /// </summary>
    public readonly struct InteractionHit
    {
        public InteractionHit(IInteractable interactable, Collider hitCollider, Vector3 hitPoint, float hitDistance)
        {
            Interactable = interactable;
            HitCollider = hitCollider;
            HitPoint = hitPoint;
            HitDistance = hitDistance;
        }

        public IInteractable Interactable { get; }
        public Collider HitCollider { get; }
        public Vector3 HitPoint { get; }
        public float HitDistance { get; }
    }
}
