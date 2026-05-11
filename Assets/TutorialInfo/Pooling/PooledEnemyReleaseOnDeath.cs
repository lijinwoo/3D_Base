using SystemicOverload.Combat;
using UnityEngine;

namespace SystemicOverload.Pooling
{
    /// <summary>
    /// 한국어 주석: 풀링된 적이 사망하면 소유 풀로 반환하고, RPG 진행(<see cref="T:SystemicOverload.Rpg.QuestService"/>)에 알립니다.
    /// </summary>
    public sealed class PooledEnemyReleaseOnDeath : MonoBehaviour
    {
        private HealthComponent healthComponent;
        private PooledInstanceLink pooledLink;

        private void Awake()
        {
            healthComponent = GetComponent<HealthComponent>();
            pooledLink = GetComponent<PooledInstanceLink>();
        }

        private void OnEnable()
        {
            if (healthComponent != null)
            {
                healthComponent.Died += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (healthComponent != null)
            {
                healthComponent.Died -= HandleDied;
            }
        }

        private void HandleDied()
        {
            global::SystemicOverload.Rpg.QuestService.NotifyPooledEnemyKilled();

            if (pooledLink != null && pooledLink.OwnerPool != null)
            {
                pooledLink.OwnerPool.Release(gameObject);
            }
        }
    }
}
