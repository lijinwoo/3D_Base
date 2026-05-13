using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 전방 근접 판정을 위한 SphereCast 기반 공격 모드입니다.
    /// </summary>
    public sealed class ShapeCastAttackMode : MonoBehaviour, ICombatAttackMode
    {
        [SerializeField] private float castRadius = 0.8f;
        [SerializeField] private float damageMultiplier = 0.85f;
        [SerializeField] private bool drawDebugLines = true;
        [SerializeField] private float debugDrawDuration = 0.08f;
        [SerializeField] private Color hitColor = new Color(1.0f, 0.45f, 0.1f, 1.0f);
        [SerializeField] private Color missColor = Color.gray;
        [SerializeField] private Color gizmoColor = new Color(1.0f, 0.7f, 0.2f, 1.0f);

        public string ModeName => "ShapeCast";

        public bool TryAttack(in AttackExecutionContext attackExecutionContext)
        {
            RaycastHit[] sphereCastHits = Physics.SphereCastAll(
                attackExecutionContext.Origin,
                castRadius,
                attackExecutionContext.Direction,
                attackExecutionContext.MaxDistance,
                attackExecutionContext.TargetLayerMask,
                QueryTriggerInteraction.Ignore);

            if (sphereCastHits.Length <= 0)
            {
                DrawMiss(in attackExecutionContext);
                return false;
            }

            System.Array.Sort(sphereCastHits, (left, right) => left.distance.CompareTo(right.distance));
            for (int index = 0; index < sphereCastHits.Length; index++)
            {
                RaycastHit hitInfo = sphereCastHits[index];
                if (hitInfo.collider != null && hitInfo.collider.transform.IsChildOf(attackExecutionContext.Owner))
                {
                    continue;
                }

                if (!TryResolveDamageable(hitInfo.collider, out IDamageable damageable) || !damageable.IsAlive)
                {
                    continue;
                }

                DamagePayload payload = new DamagePayload
                {
                    Amount = attackExecutionContext.BaseDamage * Mathf.Max(0.0f, damageMultiplier),
                    Attacker = attackExecutionContext.Owner
                };
                damageable.ApplyDamage(in payload);
                DrawHit(attackExecutionContext.Origin, hitInfo.point);
                return true;
            }

            DrawMiss(in attackExecutionContext);
            return false;
        }

        private static bool TryResolveDamageable(Collider hitCollider, out IDamageable damageable)
        {
            damageable = null;
            if (hitCollider == null)
            {
                return false;
            }

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
            castRadius = Mathf.Max(0.05f, castRadius);
            damageMultiplier = Mathf.Max(0.0f, damageMultiplier);
            debugDrawDuration = Mathf.Max(0.0f, debugDrawDuration);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Vector3 origin = transform.position + Vector3.up * 1.3f;
            Vector3 direction = transform.forward;
            Gizmos.DrawWireSphere(origin, castRadius);
            Gizmos.DrawWireSphere(origin + direction * 2.0f, castRadius);
            Gizmos.DrawLine(origin, origin + direction * 2.0f);
        }

        private void DrawHit(Vector3 origin, Vector3 hitPoint)
        {
            if (!drawDebugLines)
            {
                return;
            }

            Debug.DrawLine(origin, hitPoint, hitColor, debugDrawDuration);
        }

        private void DrawMiss(in AttackExecutionContext attackExecutionContext)
        {
            if (!drawDebugLines)
            {
                return;
            }

            Vector3 endPoint = attackExecutionContext.Origin + attackExecutionContext.Direction * attackExecutionContext.MaxDistance;
            Debug.DrawLine(attackExecutionContext.Origin, endPoint, missColor, debugDrawDuration);
        }
    }
}
