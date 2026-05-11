using System;
using SystemicOverload.Data;
using SystemicOverload.Phase1;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 기본 원거리(레이캐스트) 공격과 발사 간격을 처리합니다. Animator가 있으면 Attack 트리거를 전달합니다.
    /// <para>한국어 주석: 레이는 <see cref="hitLayerMask"/>만 통과하므로, 플레이어/환경/적 Layer를 명시하는 것이 안전합니다.</para>
    /// <para>한국어 주석: 자기 자신(루트 동일)/자식 콜라이더는 거리 정렬된 다중 hit 중에서 자동으로 skip되어, 다음 유효 타겟을 탐색합니다.</para>
    /// </summary>
    [RequireComponent(typeof(InputProvider))]
    public sealed class CombatComponent : MonoBehaviour
    {
        private const string AttackTriggerParameterName = "AttackTrig";
        // 한국어 주석: RaycastNonAlloc 버퍼 크기. 한 발 안에서 가능한 hit 수 상한이며, 너무 작으면 뒤쪽 적이 누락될 수 있습니다.
        private const int HitScanBufferSize = 16;

        [Header("Weapon")]
        [Tooltip("설정 시 인스펙터의 숫자 값보다 ScriptableObject 값이 우선합니다(런타임 초기화).")]
        [SerializeField] private WeaponData weaponData;

        [SerializeField] private float damage = 12.0f;
        [SerializeField] private float shotsPerSecond = 4.0f;
        [SerializeField] private float maxRange = 40.0f;
        [SerializeField] private float rayOriginHeight = 1.0f;
        [SerializeField] private float rayStartForwardOffset = 0.35f;
        [SerializeField] private LayerMask hitLayerMask = ~0;

        [Header("References")]
        [SerializeField] private MovementComponent movementComponent;
        [SerializeField] private Animator animator;

        private InputProvider inputProvider;
        private float nextAllowedShotTime;
        private bool loggedMissingInputProvider;
        // 한국어 주석: 매 발사마다 GC 할당이 발생하지 않도록 RaycastHit 버퍼를 재사용합니다.
        private readonly RaycastHit[] hitScanBuffer = new RaycastHit[HitScanBufferSize];

        private static readonly int AttackTriggerHash = Animator.StringToHash(AttackTriggerParameterName);

        private void Awake()
        {
            inputProvider = GetComponent<InputProvider>();
            movementComponent ??= GetComponent<MovementComponent>();
            ApplyWeaponDataIfPresent();
        }

        private void OnValidate()
        {
            // 한국어 주석: 에디터에서 SO를 연결하면 즉시 반영되도록 합니다.
            ApplyWeaponDataIfPresent();
            damage = Mathf.Max(0.0f, damage);
            shotsPerSecond = Mathf.Max(0.01f, shotsPerSecond);
            maxRange = Mathf.Max(0.1f, maxRange);
        }

        private void Update()
        {
            // 한국어 주석: RequireComponent이지만 에디터 직렬화/런타임 교체 등으로 null이 될 수 있어 방어합니다.
            if (inputProvider == null)
            {
                inputProvider = GetComponent<InputProvider>();
                if (inputProvider == null)
                {
                    if (!loggedMissingInputProvider)
                    {
                        loggedMissingInputProvider = true;
                        Debug.LogError("[CombatComponent] InputProvider가 없습니다. Attack 입력을 처리할 수 없습니다.", this);
                    }

                    return;
                }
            }

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
        /// 히트 스캔 한 발을 수행하고, 맞은 대상에 <see cref="IDamageable"/> 데미지를 적용합니다.
        /// <para>한국어 주석: RaycastNonAlloc + 거리 정렬로 자기 자신(루트 동일)을 skip하고, 첫 유효 IDamageable에 데미지를 적용합니다.</para>
        /// </summary>
        private void TryFireHitScan()
        {
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight + transform.forward * rayStartForwardOffset;
            Vector3 direction = ResolveFireDirection();
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = transform.forward;
            }
            else
            {
                direction.Normalize();
            }

            int hitCount = Physics.RaycastNonAlloc(origin, direction, hitScanBuffer, maxRange, hitLayerMask, QueryTriggerInteraction.Ignore);
            if (hitCount <= 0)
            {
                return;
            }

            // 한국어 주석: RaycastNonAlloc는 거리 정렬을 보장하지 않으므로, 가까운 hit부터 검사하기 위해 정렬합니다.
            Array.Sort(hitScanBuffer, 0, hitCount, RaycastHitDistanceComparer.Instance);

            Transform attackerRoot = transform.root;
            for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
            {
                RaycastHit hitInfo = hitScanBuffer[hitIndex];
                Collider hitCollider = hitInfo.collider;
                if (hitCollider == null)
                {
                    continue;
                }

                // 한국어 주석: 자기 자신(루트 동일)이면 다음 hit를 검사합니다. IsChildOf보다 강한 필터로 동일 GameObject self-hit도 제외합니다.
                if (hitCollider.transform.root == attackerRoot)
                {
                    continue;
                }

                IDamageable damageable = hitCollider.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive)
                {
                    // 한국어 주석: 데미지 가능 대상이 아니면 환경/장애물로 보고, 더 뒤쪽 적은 가리는 것이 자연스러우므로 종료합니다.
                    return;
                }

                DamagePayload payload = new DamagePayload
                {
                    Amount = damage,
                    Attacker = transform
                };
                damageable.ApplyDamage(in payload);
                return;
            }
        }

        /// <summary>
        /// 한국어 주석: RaycastHit 배열을 거리 오름차순으로 정렬할 때 재사용하는 비교자입니다(Array.Sort GC 0).
        /// </summary>
        private sealed class RaycastHitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly RaycastHitDistanceComparer Instance = new RaycastHitDistanceComparer();

            public int Compare(RaycastHit lhs, RaycastHit rhs)
            {
                return lhs.distance.CompareTo(rhs.distance);
            }
        }

        private Vector3 ResolveFireDirection()
        {
            if (movementComponent != null)
            {
                Vector3 toAim = movementComponent.LastAimPoint - transform.position;
                toAim.y = 0.0f;
                if (toAim.sqrMagnitude > 0.0001f)
                {
                    return toAim.normalized;
                }
            }

            return transform.forward;
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

        /// <summary>
        /// 한국어 주석: <see cref="weaponData"/>가 있으면 히트 스캔 수치를 SO 기준으로 덮어씁니다.
        /// </summary>
        private void ApplyWeaponDataIfPresent()
        {
            if (weaponData == null)
            {
                return;
            }

            damage = Mathf.Max(0.0f, weaponData.HitScanDamage);
            shotsPerSecond = Mathf.Max(0.01f, weaponData.ShotsPerSecond);
            maxRange = Mathf.Max(0.1f, weaponData.MaxRange);
            rayOriginHeight = Mathf.Max(0.0f, weaponData.RayOriginHeight);
            rayStartForwardOffset = weaponData.RayStartForwardOffset;
            hitLayerMask = weaponData.HitLayerMask;
        }
    }
}
