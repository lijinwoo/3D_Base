using SystemicOverload.Gameplay.Interaction;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// Enemy 더미의 상태 로그와 상호작용 표시를 담당합니다.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public sealed class EnemyDummyController : MonoBehaviour, IInteractable
    {
        [SerializeField] private string enemyName = "Enemy_Goblin_Dummy";
        [SerializeField] private bool destroyOnDeath = false;

        private HealthComponent healthComponent;
        private bool subscribed;

        public string InteractionLabel => $"Inspect: {enemyName}";

        private void Awake()
        {
            healthComponent = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            if (healthComponent == null || subscribed)
            {
                return;
            }

            healthComponent.Damaged += HandleDamaged;
            healthComponent.Died += HandleDied;
            subscribed = true;
        }

        private void OnDisable()
        {
            if (healthComponent == null || !subscribed)
            {
                return;
            }

            healthComponent.Damaged -= HandleDamaged;
            healthComponent.Died -= HandleDied;
            subscribed = false;
        }

        public bool CanInteract(in InteractionContext interactionContext)
        {
            return true;
        }

        public void Interact(in InteractionContext interactionContext)
        {
            Debug.Log($"[EnemyDummyController] {enemyName} HP: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");
        }

        private void HandleDamaged(float damageAmount, float currentHealth)
        {
            Debug.Log($"[EnemyDummyController] {enemyName} 피격: -{damageAmount}, 잔여 HP={currentHealth}");
        }

        private void HandleDied()
        {
            Debug.Log($"[EnemyDummyController] {enemyName} 사망");
            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
