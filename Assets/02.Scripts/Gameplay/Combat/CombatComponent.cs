using SystemicOverload.Phase1;
using SystemicOverload.PhysicsQuery;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 기본 원거리(레이캐스트) 공격과 발사 간격을 처리합니다. Animator가 있으면 Attack 트리거를 전달합니다.
    /// </summary>
    [RequireComponent(typeof(InputProvider))]
    public sealed class CombatComponent : MonoBehaviour
    {
        private const string AttackTriggerParameterName = "AttackTrig";

        [Header("Weapon")]
        [SerializeField] private float damage = 12.0f;
        [SerializeField] private float shotsPerSecond = 4.0f;
        [SerializeField] private float maxRange = 40.0f;
        [SerializeField] private float rayOriginHeight = 1.0f;
        [SerializeField] private float rayStartForwardOffset = 0.35f;
        [SerializeField] private LayerMask hitLayerMask = ~0;
        [SerializeField] private float cameraAimRayMaxDistance = 200.0f;
        [SerializeField] private float muzzleAimPaddingDistance = 0.1f;
        [SerializeField] private bool drawDebugRay = true;

        [Header("References")]
        [SerializeField] private MovementComponent movementComponent;
        [SerializeField] private Animator animator;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private TpsPhysicsQueryService physicsQueryService;

        private InputProvider inputProvider;
        private float nextAllowedShotTime;

        private static readonly int AttackTriggerHash = Animator.StringToHash(AttackTriggerParameterName);

        private void Awake()
        {
            inputProvider = GetComponent<InputProvider>();
            movementComponent ??= GetComponent<MovementComponent>();
            physicsQueryService ??= GetComponent<TpsPhysicsQueryService>();
            if (physicsQueryService == null)
            {
                physicsQueryService = gameObject.AddComponent<TpsPhysicsQueryService>();
            }
        }

        private void OnValidate()
        {
            damage = Mathf.Max(0.0f, damage);
            shotsPerSecond = Mathf.Max(0.01f, shotsPerSecond);
            maxRange = Mathf.Max(0.1f, maxRange);
            cameraAimRayMaxDistance = Mathf.Max(0.1f, cameraAimRayMaxDistance);
            muzzleAimPaddingDistance = Mathf.Max(0.0f, muzzleAimPaddingDistance);
        }

        private void Update()
        {
            if (!inputProvider.WasAttackPressedThisFrame)
            {
                return;
            }

            if (Time.time < nextAllowedShotTime)
            {
                return;
            }

            float interval = 1.0f / shotsPerSecond;
            nextAllowedShotTime = Time.time + interval;

            TryFireHitScan();
            TrySetAttackTrigger();
        }

        /// <summary>
        /// TPS 정석 2-Step Raycast를 수행합니다.
        /// 1) Camera center Ray로 조준점을 확정하고
        /// 2) 실제 발사점(Muzzle)에서 조준점 방향으로 다시 Raycast해 최종 피격을 결정합니다.
        /// </summary>
        private void TryFireHitScan()
        {
            if (!TryResolveCameraRay(out Ray cameraRay))
            {
                return;
            }

            Vector3 step1AimPoint = ResolveCameraAimPoint(cameraRay);
            Vector3 muzzleOrigin = ResolveMuzzleOrigin();
            TpsAimComputation.BuildMuzzleCast(
                muzzleOrigin,
                step1AimPoint,
                transform.forward,
                maxRange,
                muzzleAimPaddingDistance,
                out Vector3 step2Direction,
                out float step2Distance);

            if (drawDebugRay)
            {
                Debug.DrawRay(cameraRay.origin, cameraRay.direction * maxRange, Color.yellow, 0.2f);
                Debug.DrawRay(muzzleOrigin, step2Direction * step2Distance, Color.cyan, 0.2f);
            }

            if (!physicsQueryService.TryRaycast(
                    muzzleOrigin,
                    step2Direction,
                    step2Distance,
                    hitLayerMask,
                    out RaycastHit hitInfo,
                    QueryTriggerInteraction.Ignore,
                    transform))
            {
                return;
            }

            IDamageable damageable = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            DamagePayload payload = new DamagePayload
            {
                Amount = damage,
                Attacker = transform
            };
            damageable.ApplyDamage(in payload);

            if (drawDebugRay)
            {
                Debug.DrawLine(muzzleOrigin, hitInfo.point, Color.green, 0.25f);
            }
        }

        private bool TryResolveCameraRay(out Ray cameraRay)
        {
            cameraRay = default;
            Camera targetCamera = aimCamera;
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                return false;
            }

            return TpsAimComputation.TryBuildCenterRay(targetCamera, out cameraRay);
        }

        private Vector3 ResolveCameraAimPoint(Ray cameraRay)
        {
            if (physicsQueryService.TryRaycast(
                    cameraRay,
                    cameraAimRayMaxDistance,
                    hitLayerMask,
                    out RaycastHit cameraHit,
                    QueryTriggerInteraction.Ignore,
                    transform))
            {
                return cameraHit.point;
            }

            return TpsAimComputation.ResolveFallbackAimPoint(cameraRay, maxRange);
        }

        private Vector3 ResolveMuzzleOrigin()
        {
            if (muzzlePoint != null)
            {
                return muzzlePoint.position;
            }

            return transform.position + Vector3.up * rayOriginHeight + transform.forward * rayStartForwardOffset;
        }

        private void TrySetAttackTrigger()
        {
            if (animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == AttackTriggerParameterName && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(AttackTriggerHash);
                    return;
                }
            }
        }
    }
}
