using SystemicOverload.Combat;
using UnityEngine;

namespace SystemicOverload.Pooling
{
    /// <summary>
    /// 한국어 주석: 풀에서 재활성화될 때 체력을 최대로 맞춥니다.
    /// <see cref="IPooledObject"/>로 풀 라이프사이클에 연결되며, OnEnable 경로도 비풀 스폰과 호환됩니다.
    /// </summary>
    public sealed class PooledHealthReset : MonoBehaviour, IPooledObject
    {
        [SerializeField] private HealthComponent healthComponent;

        private void Awake()
        {
            healthComponent ??= GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            // 한국어 주석: 풀 외 경로(에디터 테스트, 직접 활성화)에서도 안전하게 초기화합니다.
            ApplyFullHealth();
        }

        public void OnSpawnedFromPool()
        {
            ApplyFullHealth();
        }

        public void OnReturnedToPool()
        {
            // 한국어 주석: 반환 시 별도 정리가 필요하면 여기에 추가합니다.
        }

        /// <summary>
        /// 한국어 주석: 외부 스폰 로직에서 명시적으로 호출할 수 있습니다.
        /// </summary>
        public void ApplyFullHealth()
        {
            if (healthComponent == null)
            {
                return;
            }

            healthComponent.ResetHealthToFull();
        }
    }
}
