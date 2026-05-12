using UnityEngine;
using StarterAssets;

namespace SystemicOverload.Raycasting
{
    /// <summary>
    /// 플레이어 기준으로 Raycast 또는 SphereCast를 수행하는 "센서" 컴포넌트입니다.
    ///
    /// [Raycast 학습 포인트]
    /// - Raycast는 "시작점(origin) + 방향(direction) + 거리(distance)"로 가상의 선을 쏘고,
    ///   그 선이 Collider에 닿았는지 검사합니다.
    /// - SphereCast는 Raycast와 비슷하지만, 얇은 선이 아니라 "반지름이 있는 구"를 굴리듯 검사합니다.
    ///   TPS RPG에서는 조준 보정, 근거리 상호작용, 넓은 판정의 공격에 유용합니다.
    /// - LayerMask를 사용하면 Enemy, Interactable, Ground 같은 원하는 레이어만 감지할 수 있습니다.
    ///
    /// [TPS RPG 활용 예]
    /// - 공격: 카메라 정면 또는 캐릭터 전방에 있는 적을 감지
    /// - 상호작용: 플레이어 앞의 NPC, 상자, 문을 감지
    /// - 타겟 탐지: 락온 후보 또는 조준 대상 찾기
    /// - 디버깅: Gizmo로 실제 Ray 방향과 충돌 지점을 확인
    /// </summary>
    public sealed class PlayerRaycastSensor : MonoBehaviour
    {
        /// <summary>
        /// 어떤 형태로 충돌 검사를 할지 선택합니다.
        /// Ray    : 얇은 선 하나로 검사합니다. 정확하지만 판정이 좁습니다.
        /// Sphere : 반지름이 있는 구 형태로 검사합니다. TPS 조준 보정이나 근접 상호작용에 적합합니다.
        /// </summary>
        public enum CastShape
        {
            Ray,
            Sphere
        }

        [Header("Cast Setup")]

        // Ray/SphereCast가 시작되는 위치입니다.
        // 예: 캐릭터 눈 위치, 카메라 위치, 무기 총구 위치, 상호작용 센서 위치.
        // 값이 비어 있으면 Awake()에서 이 GameObject의 transform으로 자동 대체됩니다.
        [SerializeField] private Transform rayOrigin;

        // Ray/SphereCast가 향하는 방향을 제공하는 Transform입니다.
        // 보통 TPS에서는 카메라의 forward를 쓰거나, 캐릭터의 forward를 사용합니다.
        // rayOrigin과 rayDirectionSource를 분리하면 "총구 위치에서 시작하되 카메라 방향으로 쏘기" 같은 구조가 가능합니다.
        [SerializeField] private Transform rayDirectionSource;

        // Ray와 SphereCast 중 어떤 방식으로 검사할지 선택합니다.
        // 수업에서는 Ray로 시작한 뒤 Sphere로 바꾸어 판정 차이를 비교시키면 좋습니다.
        [SerializeField] private CastShape castShape = CastShape.Ray;

        // Ray/SphereCast가 도달할 수 있는 최대 거리입니다.
        // 예: 상호작용은 2~3m, 근접 공격은 1~2m, 원거리 조준은 30~100m처럼 상황별로 다르게 설정합니다.
        [SerializeField] private float maxDistance = 3.0f;

        // SphereCast일 때만 의미가 있는 반지름입니다.
        // 값이 클수록 조준 판정이 넓어지지만, 너무 크면 의도하지 않은 대상까지 맞을 수 있습니다.
        [SerializeField] private float sphereRadius = 0.2f;

        // Ray/SphereCast가 감지할 Layer를 제한합니다.
        // ~0은 모든 비트를 1로 둔 값이므로, 기본값은 "모든 Layer 감지"입니다.
        // 수업 예: Enemy와 Interactable 레이어만 체크하면 바닥/벽/UI 같은 불필요한 충돌을 줄일 수 있습니다.
        [SerializeField] private LayerMask hitLayerMask = ~0;

        // Trigger Collider를 Raycast 결과에 포함할지 결정합니다.
        // Ignore   : Trigger 무시. 실제 벽, 적, 오브젝트 같은 물리 충돌체만 검사할 때 적합합니다.
        // Collide  : Trigger 포함. 상호작용 영역, 감지 범위, 퀘스트 범위 같은 트리거 감지에 적합합니다.
        // UseGlobal: Project Settings > Physics의 전역 설정을 따릅니다.
        [SerializeField] private QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        // 플레이어 자신의 Collider를 맞추지 않도록 할지 여부입니다.
        // TPS에서는 카메라/센서가 플레이어 몸 근처에 있어서 자기 Collider를 먼저 맞추는 문제가 자주 발생합니다.
        [SerializeField] private bool ignoreSelfColliders = true;

        [Header("Input Trigger")]

        // Starter Assets에서 입력 상태를 관리하는 컴포넌트입니다.
        // 이 스크립트는 입력 자체를 직접 정의하지 않고, StarterAssetsInputs의 클릭 상태만 읽습니다.
        [SerializeField] private StarterAssetsInputs starterAssetsInputs;

        // true면 Primary Click이 눌린 프레임에 자동으로 TryCast()를 실행합니다.
        // false로 두면 다른 스크립트가 TryCast()를 직접 호출하는 "센서 전용 모드"로 사용할 수 있습니다.
        [SerializeField] private bool castOnPrimaryClick = true;

        // true면 Cast 결과를 Console에 출력합니다.
        // 학생 실습에서는 true로 켜 두면 origin, hit point, distance를 확인하기 좋습니다.
        [SerializeField] private bool printDebugOnCast = true;

        [Header("Gizmos")]

        // Scene 뷰에서 Ray/SphereCast의 방향과 충돌 위치를 시각화할지 여부입니다.
        // Raycast는 Game 뷰에서 보이지 않으므로, 학습 단계에서는 Gizmo가 매우 중요합니다.
        [SerializeField] private bool drawGizmos = true;

        // true : 오브젝트를 선택했을 때만 Gizmo 표시.
        // false: 선택하지 않아도 Scene 뷰에 항상 표시.
        [SerializeField] private bool drawOnlyWhenSelected = true;

        // 아무것도 맞추지 못했을 때 표시할 색상입니다.
        [SerializeField] private Color missColor = new Color(0.2f, 0.75f, 1.0f, 0.9f);

        // 무언가를 맞췄을 때 표시할 색상입니다.
        [SerializeField] private Color hitColor = new Color(1.0f, 0.3f, 0.2f, 0.95f);

        [Header("Debug Visualization")]

        // CenterRaycastShooter의 Debug.DrawLine처럼 Cast 시점에 Game 뷰 라인을 출력할지 결정합니다.
        [SerializeField] private bool drawDebugLineOnCast = true;

        // Debug.DrawLine이 화면에 유지되는 시간(초)입니다.
        [SerializeField] private float debugLineDuration = 0.6f;

        // 마지막 충돌 지점의 노멀 벡터를 Gizmo로 표시할지 여부입니다.
        [SerializeField] private bool drawHitNormalGizmo = true;

        // Ray 시작점과 방향 확인을 위해 origin sphere/forward ray를 추가 표시합니다.
        [SerializeField] private bool drawOriginAndDirectionGizmo = true;

        [SerializeField] private float originGizmoRadius = 0.04f;
        [SerializeField] private float directionPreviewLength = 0.5f;
        [SerializeField] private Color directionPreviewColor = new Color(1.0f, 0.9f, 0.2f, 0.95f);

        // 플레이어 자신에게 속한 모든 Collider를 저장합니다.
        // Raycast가 자기 몸을 먼저 맞췄는지 검사할 때 사용합니다.
        private Collider[] selfColliders = System.Array.Empty<Collider>();

        /// <summary>
        /// 마지막 Cast가 Collider를 맞췄는지 여부입니다.
        /// UI 표시, 디버그 출력, Gizmo 시각화에 사용할 수 있습니다.
        /// </summary>
        public bool HasHitLastCast { get; private set; }

        /// <summary>
        /// 마지막 Cast의 충돌 정보입니다.
        /// RaycastHit에는 collider, point, normal, distance 같은 핵심 정보가 들어 있습니다.
        /// </summary>
        public RaycastHit LastHitInfo { get; private set; }

        /// <summary>
        /// 현재 센서가 사용하는 최대 탐지 거리입니다.
        /// 다른 스크립트가 읽을 수는 있지만, 직접 수정하지는 못하게 private set 구조로 관리합니다.
        /// </summary>
        public float MaxDistance => maxDistance;

        private void Awake()
        {
            // true를 넣으면 비활성화된 자식 오브젝트의 Collider까지 가져옵니다.
            // 플레이어 모델, 장비, 무기, 히트박스가 자식에 있을 수 있으므로 전체를 수집합니다.
            selfColliders = GetComponentsInChildren<Collider>(true);

            // rayOrigin을 인스펙터에 넣지 않은 경우, 최소한 이 오브젝트 위치에서 쏘도록 기본값을 잡습니다.
            // 이렇게 하면 NullReferenceException을 줄이고, 초보자가 빠르게 테스트할 수 있습니다.
            if (rayOrigin == null)
            {
                rayOrigin = transform;
            }

            // 방향 기준이 따로 없으면 시작점의 forward를 방향으로 사용합니다.
            if (rayDirectionSource == null)
            {
                rayDirectionSource = rayOrigin;
            }

            // StarterAssetsInputs를 인스펙터에 직접 연결하지 않아도,
            // 같은 GameObject에 붙어 있으면 자동으로 찾아옵니다.
            if (starterAssetsInputs == null)
            {
                starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            }
        }

        private void OnValidate()
        {
            // OnValidate는 인스펙터 값이 바뀔 때 에디터에서 호출됩니다.
            // 거리나 반지름이 0 이하가 되면 Raycast/SphereCast 의미가 없어지므로 최소값을 보정합니다.
            maxDistance = Mathf.Max(0.05f, maxDistance);
            sphereRadius = Mathf.Max(0.01f, sphereRadius);
            debugLineDuration = Mathf.Max(0.0f, debugLineDuration);
            originGizmoRadius = Mathf.Max(0.005f, originGizmoRadius);
            directionPreviewLength = Mathf.Max(0.05f, directionPreviewLength);
        }

        private void Update()
        {
            // 자동 Cast가 꺼져 있거나 입력 컴포넌트가 없으면 Update에서 아무것도 하지 않습니다.
            // 이 경우 외부 스크립트가 TryCast()를 직접 호출하는 구조로 사용할 수 있습니다.
            if (!castOnPrimaryClick || starterAssetsInputs == null)
            {
                return;
            }

            // WasPrimaryClickPressedThisFrame()은 "이번 프레임에 막 눌렸는가"를 검사한다고 보면 됩니다.
            // GetMouseButton처럼 계속 누르는 동안 매 프레임 실행되는 방식과 구분해서 설명하면 좋습니다.
            if (!starterAssetsInputs.WasPrimaryClickPressedThisFrame())
            {
                return;
            }

            // out RaycastHit hitInfo:
            // TryCast가 true를 반환하면 hitInfo 안에 충돌한 대상 정보가 채워집니다.
            // TryCast가 false를 반환하면 hitInfo는 유효한 충돌 정보가 아니라고 보면 됩니다.
            bool hasHit = TryCast(out RaycastHit hitInfo);

            // 디버그 출력이 꺼져 있으면 Cast만 수행하고 로그는 남기지 않습니다.
            if (!printDebugOnCast)
            {
                return;
            }

            if (!hasHit)
            {
                // Miss 로그에서는 Ray가 어디에서 시작했고, 얼마만큼 검사했는지 확인합니다.
                Debug.Log($"[PlayerRaycastSensor] Cast miss | Origin={GetCurrentOrigin().position} | Distance={maxDistance:F2}", this);
                return;
            }

            // Hit 로그에서는 충돌한 Collider 이름, 충돌 지점, 충돌 거리 확인이 핵심입니다.
            // hitInfo.distance는 origin에서 hit point까지의 실제 거리입니다.
            Debug.Log(
                $"[PlayerRaycastSensor] Hit={hitInfo.collider.name} | Point={hitInfo.point} | Distance={hitInfo.distance:F2}",
                hitInfo.collider);
        }

        /// <summary>
        /// 인스펙터에 설정된 maxDistance로 Cast를 수행합니다.
        /// 가장 기본적으로 사용할 수 있는 버전입니다.
        /// </summary>
        public bool TryCast(out RaycastHit hitInfo)
        {
            // 오버로드를 활용해서 실제 구현은 TryCast(float, out RaycastHit)에 맡깁니다.
            // 같은 로직을 여러 곳에 복사하지 않는 구조입니다.
            return TryCast(maxDistance, out hitInfo);
        }

        /// <summary>
        /// 거리만 임시로 바꿔서 Cast를 수행합니다.
        /// 예: 평소 상호작용 거리는 3m, 특수 스킬 탐지 거리는 8m처럼 상황별 호출이 가능합니다.
        /// </summary>
        public bool TryCast(float distanceOverride, out RaycastHit hitInfo)
        {
            // 인스펙터 값이 비어 있어도 안전하게 동작하도록 fallback을 둡니다.
            Transform originTransform = rayOrigin != null ? rayOrigin : transform;
            Transform directionTransform = rayDirectionSource != null ? rayDirectionSource : originTransform;

            // Raycast의 3요소 중 "시작점"입니다.
            Vector3 castOrigin = originTransform.position;

            // Raycast의 3요소 중 "방향"입니다.
            // Transform.forward는 월드 공간 기준의 파란색 Z축 방향이라고 이해하면 됩니다.
            Vector3 castDirection = directionTransform.forward;

            // 방향 벡터가 0에 가까우면 normalized가 불안정해질 수 있습니다.
            // 이런 경우에는 이 GameObject의 forward를 예비 방향으로 사용합니다.
            if (castDirection.sqrMagnitude < 0.0001f)
            {
                castDirection = transform.forward;
            }

            // Physics.Raycast는 방향 벡터가 정규화되어 있지 않아도 동작하지만,
            // 학습과 디버깅에서는 "방향은 길이 1의 벡터"로 맞추는 습관이 좋습니다.
            castDirection.Normalize();

            // 실제 Physics.Raycast / Physics.SphereCast 호출은 아래 오버로드에 위임합니다.
            return TryCast(castOrigin, castDirection, distanceOverride, out hitInfo);
        }

        /// <summary>
        /// origin/direction/distance를 외부에서 직접 지정해 Cast를 수행합니다.
        ///
        /// [활용 예]
        /// - 카메라 중앙 조준 Ray: origin = camera.transform.position, direction = camera.transform.forward
        /// - 무기 총구 보정 Ray: origin = muzzle.position, direction = targetPoint - muzzle.position
        /// - 적 AI 시야 검사: origin = enemyEye.position, direction = player.position - enemyEye.position
        /// </summary>
        public bool TryCast(Vector3 castOrigin, Vector3 castDirection, float castDistance, out RaycastHit hitInfo)
        {
            // 음수나 0에 가까운 거리가 들어와도 최소 거리로 보정합니다.
            castDistance = Mathf.Max(0.05f, castDistance);

            // direction은 "어디로 쏠 것인가"만 의미해야 하므로 정규화합니다.
            // sqrMagnitude를 먼저 쓰는 이유는 magnitude보다 sqrt 계산이 없어 조금 더 가볍기 때문입니다.
            castDirection = castDirection.sqrMagnitude > 0.0001f ? castDirection.normalized : transform.forward;

            bool hasHit;

            if (castShape == CastShape.Sphere)
            {
                // SphereCast:
                // - origin에서 시작해 sphereRadius 크기의 구를 direction 방향으로 굴리듯 검사합니다.
                // - 얇은 Ray보다 판정이 넓기 때문에, TPS에서 조준이 약간 빗나가도 적을 잡아내는 "보정"에 유용합니다.
                // - 단, radius가 너무 크면 벽 뒤 적이나 옆 대상까지 맞는 것처럼 느껴질 수 있습니다.
                hasHit = Physics.SphereCast(
                    castOrigin,
                    sphereRadius,
                    castDirection,
                    out hitInfo,
                    castDistance,
                    hitLayerMask,
                    queryTriggerInteraction);
            }
            else
            {
                // Raycast:
                // - origin에서 direction 방향으로 castDistance만큼 보이지 않는 선을 쏩니다.
                // - out hitInfo에는 충돌한 Collider, 충돌 위치(point), 표면 방향(normal), 거리(distance)가 들어갑니다.
                // - hitLayerMask는 감지할 Layer를 제한하고, queryTriggerInteraction은 Trigger 포함 여부를 결정합니다.
                hasHit = Physics.Raycast(
                    castOrigin,
                    castDirection,
                    out hitInfo,
                    castDistance,
                    hitLayerMask,
                    queryTriggerInteraction);
            }

            // TPS에서 흔한 문제:
            // 카메라나 센서가 플레이어 몸 안쪽/근처에 있으면 Ray가 자신의 Collider를 먼저 맞을 수 있습니다.
            // 이 옵션이 켜져 있으면 그런 자기 자신에 대한 hit를 무효 처리합니다.
            //
            // 주의:
            // 이 코드는 "첫 번째로 맞은 대상이 자기 자신이면 실패 처리"하는 단순 학습용 구조입니다.
            // 실제 게임에서는 RaycastAll 또는 RaycastNonAlloc으로 여러 hit를 받아 거리순으로 정렬한 뒤,
            // 자기 자신을 건너뛰고 다음 hit를 선택하는 방식으로 확장할 수 있습니다.
            if (hasHit && ignoreSelfColliders && IsSelfCollider(hitInfo.collider))
            {
                hasHit = false;
                hitInfo = default;
            }

            // 마지막 Cast 결과를 저장해 두면 다른 스크립트와 Gizmo가 같은 결과를 참조할 수 있습니다.
            HasHitLastCast = hasHit;
            if (hasHit)
            {
                LastHitInfo = hitInfo;
            }
            else
            {
                LastHitInfo = default;
            }

            DrawCastDebugLine(castOrigin, castDirection, castDistance, hasHit, hitInfo);

            return hasHit;
        }

        /// <summary>
        /// Cast로 맞춘 Collider에서 원하는 컴포넌트를 가져옵니다.
        ///
        /// [왜 GetComponentInParent를 쓰는가?]
        /// - 적 캐릭터는 보통 루트 오브젝트에 EnemyHealth, IDamageable 같은 스크립트가 있고,
        ///   실제 Collider는 자식 Bone/HitBox 오브젝트에 있는 경우가 많습니다.
        /// - hitInfo.collider.GetComponent<T>()만 쓰면 자식 Collider에서 컴포넌트를 못 찾을 수 있습니다.
        /// - GetComponentInParent<T>()는 Collider 자신부터 부모 방향으로 올라가며 컴포넌트를 찾습니다.
        /// </summary>
        public bool TryGetHitComponent<TComponent>(out TComponent component, out RaycastHit hitInfo) where TComponent : Component
        {
            component = null;

            // 먼저 Cast를 실행합니다. 아무것도 맞지 않았다면 컴포넌트도 찾을 수 없습니다.
            if (!TryCast(out hitInfo))
            {
                return false;
            }

            // 예: TryGetHitComponent<EnemyHealth>(out var enemy, out var hit)
            // 예: TryGetHitComponent<InteractableChest>(out var chest, out var hit)
            component = hitInfo.collider.GetComponentInParent<TComponent>();
            return component != null;
        }

        private bool IsSelfCollider(Collider candidateCollider)
        {
            // Null 안전 처리입니다.
            if (candidateCollider == null)
            {
                return false;
            }

            // Awake()에서 수집한 자기 자신/자식 Collider 목록과 비교합니다.
            for (int index = 0; index < selfColliders.Length; index++)
            {
                if (selfColliders[index] == candidateCollider)
                {
                    return true;
                }
            }

            // selfColliders 배열에 없더라도 Transform 계층상 내 자식이면 자기 자신으로 간주합니다.
            // 장비나 임시 생성된 히트박스가 나중에 붙는 경우를 보완합니다.
            return candidateCollider.transform.IsChildOf(transform);
        }

        private void OnDrawGizmos()
        {
            // 선택하지 않았을 때도 항상 그리고 싶은 경우에만 실행됩니다.
            if (!drawGizmos || drawOnlyWhenSelected)
            {
                return;
            }

            DrawCastGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            // 오브젝트를 선택했을 때만 그리고 싶은 경우에 실행됩니다.
            if (!drawGizmos || !drawOnlyWhenSelected)
            {
                return;
            }

            DrawCastGizmos();
        }

        /// <summary>
        /// Scene 뷰에서 현재 Cast 방향과 마지막 충돌 지점을 시각화합니다.
        ///
        /// [학생 실습 체크]
        /// - 선이 예상한 방향으로 나가는가?
        /// - hitColor와 missColor가 상황에 맞게 바뀌는가?
        /// - Sphere 모드일 때 반지름이 너무 크거나 작지 않은가?
        /// </summary>
        private void DrawCastGizmos()
        {
            // 에디터에서 아직 Awake()가 호출되지 않은 상태일 수 있으므로 null fallback을 다시 처리합니다.
            Transform originTransform = rayOrigin != null ? rayOrigin : transform;
            Transform directionTransform = rayDirectionSource != null ? rayDirectionSource : originTransform;

            Vector3 castOrigin = originTransform.position;
            Vector3 castDirection = directionTransform.forward.sqrMagnitude > 0.0001f
                ? directionTransform.forward.normalized
                : transform.forward;

            float drawDistance = maxDistance;
            bool hasHit = HasHitLastCast;

            // 마지막으로 맞춘 대상이 있으면 충돌 지점까지만 선을 그립니다.
            // 맞춘 대상이 없으면 최대 거리까지 선을 그립니다.
            Vector3 endPoint = hasHit ? LastHitInfo.point : castOrigin + castDirection * drawDistance;

            Gizmos.color = hasHit ? hitColor : missColor;
            Gizmos.DrawLine(castOrigin, endPoint);

            // 끝점에 작은 구를 그려서 "Ray가 끝난 위치" 또는 "충돌 지점"을 명확히 보여줍니다.
            Gizmos.DrawWireSphere(endPoint, 0.06f);

            if (drawOriginAndDirectionGizmo)
            {
                // 한국어 주석: origin과 방향 프리뷰를 함께 그려 Scene 뷰에서 조준 축을 빠르게 확인합니다.
                Gizmos.DrawWireSphere(castOrigin, originGizmoRadius);
                Gizmos.color = directionPreviewColor;
                Gizmos.DrawRay(castOrigin, castDirection * directionPreviewLength);
                Gizmos.color = hasHit ? hitColor : missColor;
            }

            if (hasHit && drawHitNormalGizmo)
            {
                // 한국어 주석: 표면 노멀을 표시해 충돌면 방향(반사/충돌 반응 설계에 중요)을 확인합니다.
                Gizmos.color = directionPreviewColor;
                Gizmos.DrawRay(LastHitInfo.point, LastHitInfo.normal * directionPreviewLength);
                Gizmos.color = hasHit ? hitColor : missColor;
            }

            if (castShape == CastShape.Sphere)
            {
                // SphereCast의 시작 구와 끝 구를 같이 그리면,
                // 학생들이 "선이 아니라 부피가 있는 판정"이라는 점을 이해하기 쉽습니다.
                Gizmos.DrawWireSphere(castOrigin, sphereRadius);
                Gizmos.DrawWireSphere(castOrigin + castDirection * drawDistance, sphereRadius);
            }
        }

        private Transform GetCurrentOrigin()
        {
            // Debug.Log에서 현재 origin 위치를 출력하기 위한 작은 헬퍼 함수입니다.
            return rayOrigin != null ? rayOrigin : transform;
        }

        /// <summary>
        /// 한국어 주석: CenterRaycastShooter 방식처럼 Cast 결과를 Debug.DrawLine으로 즉시 시각화합니다.
        /// </summary>
        private void DrawCastDebugLine(Vector3 castOrigin, Vector3 castDirection, float castDistance, bool hasHit, RaycastHit hitInfo)
        {
            if (!drawDebugLineOnCast)
            {
                return;
            }

            if (hasHit)
            {
                Debug.DrawLine(castOrigin, hitInfo.point, hitColor, debugLineDuration);
            }
            else
            {
                Vector3 missEndPoint = castOrigin + castDirection * castDistance;
                Debug.DrawLine(castOrigin, missEndPoint, missColor, debugLineDuration);
            }
        }
    }
}
