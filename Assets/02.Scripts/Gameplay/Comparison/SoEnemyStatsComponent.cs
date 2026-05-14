using UnityEngine;

namespace SystemicOverload.Comparison
{
    /// <summary>
    /// ScriptableObject를 기준 데이터로 참조하는 데모용 컴포넌트입니다.
    /// 최대 체력/공격력은 공유 에셋에서 읽고, 현재 체력만 인스턴스별로 보관합니다.
    /// </summary>
    public sealed class SoEnemyStatsComponent : MonoBehaviour
    {
        [Header("Shared Config")]
        [SerializeField] private ComparisonEnemyConfigSO sharedConfig;
        [SerializeField] private string enemyLabel = "SO Enemy";

        [Header("Runtime State")]
        [SerializeField] private float currentHealth = 100.0f;

        public string EnemyLabel => enemyLabel;
        public ComparisonEnemyConfigSO SharedConfig => sharedConfig;
        public float MaxHealth => sharedConfig != null ? sharedConfig.MaxHealth : 1.0f;
        public float AttackPower => sharedConfig != null ? sharedConfig.AttackPower : 0.0f;
        public float CurrentHealth => currentHealth;

        private void Awake()
        {
            ResetRuntimeState();
        }

        private void OnValidate()
        {
            enemyLabel = string.IsNullOrWhiteSpace(enemyLabel) ? "SO Enemy" : enemyLabel.Trim();
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, MaxHealth);
        }

        /// <summary>
        /// 데모 초기 배치를 위해 공유 에셋과 라벨을 설정합니다.
        /// </summary>
        public void Configure(ComparisonEnemyConfigSO config, string label)
        {
            sharedConfig = config;
            enemyLabel = string.IsNullOrWhiteSpace(label) ? "SO Enemy" : label.Trim();
            ResetRuntimeState();
        }

        /// <summary>
        /// 현재 체력을 감소시킵니다.
        /// </summary>
        public void ApplyDamage(float damageAmount)
        {
            float sanitizedDamage = Mathf.Max(0.0f, damageAmount);
            currentHealth = Mathf.Max(0.0f, currentHealth - sanitizedDamage);
        }

        /// <summary>
        /// 공유 기준 최대 체력에 맞춰 현재 체력을 재설정합니다.
        /// </summary>
        public void ResetRuntimeState()
        {
            currentHealth = MaxHealth;
        }

        /// <summary>
        /// 공유 최대 체력 변경 이후 현재 체력이 초과하지 않도록 보정합니다.
        /// </summary>
        public void ClampCurrentHealthToSharedMax()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, MaxHealth);
        }
    }
}
