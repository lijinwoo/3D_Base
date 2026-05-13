using System.Collections.Generic;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 범위형 타격을 위한 OverlapSphereNonAlloc 기반 공격 모드입니다.
    /// </summary>
    public sealed class OverlapAttackMode : MonoBehaviour, ICombatAttackMode
    {
        [SerializeField] private float overlapRadius = 2.0f;
        [SerializeField] private float overlapForwardOffset = 1.3f;
        [SerializeField] private float damageMultiplier = 0.6f;
        [SerializeField] private bool drawDebugLines = true;
        [SerializeField] private float debugDrawDuration = 0.08f;
        [SerializeField] private Color hitColor = new Color(0.9f, 0.1f, 0.95f, 1.0f);
        [SerializeField] private Color missColor = Color.gray;
        [SerializeField] private Color gizmoColor = new Color(0.8f, 0.25f, 1.0f, 1.0f);

        private readonly Collider[] overlapResults = new Collider[32];
        private readonly HashSet<IDamageable> uniqueTargets = new HashSet<IDamageable>();

        public string ModeName => "Overlap";

        public bool TryAttack(in AttackExecutionContext attackExecutionContext)
        {
            uniqueTargets.Clear();
            Vector3 overlapCenter = attackExecutionContext.Origin + attackExecutionContext.Direction * overlapForwardOffset;
            int hitCount = Physics.OverlapSphereNonAlloc(
                overlapCenter,
                overlapRadius,
                overlapResults,
                attackExecutionContext.TargetLayerMask,
                QueryTriggerInteraction.Ignore);

            if (hitCount <= 0)
            {
                DrawMiss(in attackExecutionContext, overlapCenter);
                return false;
            }

            bool didHit = false;
            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider = overlapResults[index];
                if (hitCollider == null || hitCollider.transform.IsChildOf(attackExecutionContext.Owner))
                {
                    continue;
                }

                if (!TryResolveDamageable(hitCollider, out IDamageable damageable) || !damageable.IsAlive)
                {
                    continue;
                }

                if (!uniqueTargets.Add(damageable))
                {
                    continue;
                }

                DamagePayload payload = new DamagePayload
                {
                    Amount = attackExecutionContext.BaseDamage * Mathf.Max(0.0f, damageMultiplier),
                    Attacker = attackExecutionContext.Owner
                };
                damageable.ApplyDamage(in payload);
                didHit = true;
                DrawHit(attackExecutionContext.Origin, hitCollider.ClosestPoint(attackExecutionContext.Origin));
            }

            if (!didHit)
            {
                DrawMiss(in attackExecutionContext, overlapCenter);
            }

            return didHit;
        }

        private static bool TryResolveDamageable(Collider hitCollider, out IDamageable damageable)
        {
            damageable = null;
            MonoBehaviour[] targetComponents = hitCollider.GetComponentsInParent<MonoBehaviour>(true);
            for (int index = 0; index < targetComponents.Length; index++)
            {
                if (targetComponents[index] is not IDamageable foundDamageable)
                {
                    continue;
                }

                damageable = foundDamageable;
                return true;
            }

            return false;
        }

        private void OnValidate()
        {
            overlapRadius = Mathf.Max(0.2f, overlapRadius);
            overlapForwardOffset = Mathf.Max(0.0f, overlapForwardOffset);
            damageMultiplier = Mathf.Max(0.0f, damageMultiplier);
            debugDrawDuration = Mathf.Max(0.0f, debugDrawDuration);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Vector3 origin = transform.position + Vector3.up * 1.3f;
            Vector3 center = origin + transform.forward * overlapForwardOffset;
            Gizmos.DrawLine(origin, center);
            Gizmos.DrawWireSphere(center, overlapRadius);
        }

        private void DrawHit(Vector3 origin, Vector3 hitPoint)
        {
            if (!drawDebugLines)
            {
                return;
            }

            Debug.DrawLine(origin, hitPoint, hitColor, debugDrawDuration);
        }

        private void DrawMiss(in AttackExecutionContext attackExecutionContext, Vector3 overlapCenter)
        {
            if (!drawDebugLines)
            {
                return;
            }

            Debug.DrawLine(attackExecutionContext.Origin, overlapCenter, missColor, debugDrawDuration);
        }
    }
}
