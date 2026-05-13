using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 단일 대상 정밀 타격용 Raycast 공격 모드입니다.
    /// </summary>
    public sealed class RaycastAttackMode : MonoBehaviour, ICombatAttackMode
    {
        [SerializeField] private float damageMultiplier = 1.0f;
        [SerializeField] private bool drawDebugLines = true;
        [SerializeField] private float debugDrawDuration = 0.08f;
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private Color missColor = Color.gray;

        public string ModeName => "Raycast";

        public bool TryAttack(in AttackExecutionContext attackExecutionContext)
        {
            RaycastHit[] raycastHits = Physics.RaycastAll(
                attackExecutionContext.Origin,
                attackExecutionContext.Direction,
                attackExecutionContext.MaxDistance,
                attackExecutionContext.TargetLayerMask,
                QueryTriggerInteraction.Ignore);

            if (raycastHits.Length <= 0)
            {
                DrawMiss(attackExecutionContext);
                return false;
            }

            System.Array.Sort(raycastHits, (left, right) => left.distance.CompareTo(right.distance));
            for (int index = 0; index < raycastHits.Length; index++)
            {
                RaycastHit hitInfo = raycastHits[index];
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

            DrawMiss(attackExecutionContext);
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
