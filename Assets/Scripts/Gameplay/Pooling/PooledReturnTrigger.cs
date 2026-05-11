using UnityEngine;

namespace SystemicOverload.Pooling
{
    /// <summary>
    /// 한국어 주석: 특정 조건(수명 종료)에서 자동으로 풀에 반환하는 보조 컴포넌트입니다. VFX/프로젝타일 검증에 사용합니다.
    /// GameObjectPool.Get 단계에서 풀 참조를 자동 주입할 수 있도록 AttachOwner 진입점을 제공합니다.
    /// </summary>
    public sealed class PooledReturnTrigger : MonoBehaviour
    {
        [SerializeField] private GameObjectPool ownerPool;
        [SerializeField] private float lifetimeSeconds = 2.0f;

        private float despawnTime;
        // 한국어 주석: OnEnable 시점 풀 반환을 한 번으로 보장합니다(중복 Release 차단 보조).
        private bool hasReleasedThisActivation;

        public GameObjectPool OwnerPool => ownerPool;
        public float LifetimeSeconds => lifetimeSeconds;

        private void OnEnable()
        {
            despawnTime = Time.time + Mathf.Max(0.05f, lifetimeSeconds);
            hasReleasedThisActivation = false;
        }

        private void Update()
        {
            if (hasReleasedThisActivation || Time.time < despawnTime)
            {
                return;
            }

            hasReleasedThisActivation = true;
            if (ownerPool != null)
            {
                ownerPool.Release(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 한국어 주석: 스폰 직후 풀 참조를 주입합니다(런타임). lifetime도 함께 갱신합니다.
        /// </summary>
        public void Configure(GameObjectPool pool, float lifetime)
        {
            ownerPool = pool;
            lifetimeSeconds = Mathf.Max(0.05f, lifetime);
            despawnTime = Time.time + lifetimeSeconds;
            hasReleasedThisActivation = false;
        }

        /// <summary>
        /// 한국어 주석: GameObjectPool.Get에서 자동 호출합니다. lifetime 인스펙터 값은 보존합니다.
        /// </summary>
        public void AttachOwner(GameObjectPool pool)
        {
            ownerPool = pool;
            // 한국어 주석: 자동 주입 시 lifetime은 사용자 설정을 유지하며, 타이머는 OnEnable 기준으로 동작합니다.
        }
    }
}
