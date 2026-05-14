using UnityEngine;

namespace SystemicOverload.Comparison
{
    /// <summary>
    /// MonoBehaviour 내부에 개별 데이터를 직접 저장하는 데모용 컴포넌트입니다.
    /// 각 인스턴스가 독립적인 기준값을 가지므로 값 변경이 다른 객체에 전파되지 않습니다.
    /// </summary>
    public sealed class MonoEnemyStatsComponent : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string enemyLabel = "Mono Enemy";

        [Header("Local Base Stats")]
        [SerializeField] private float maxHealth = 100.0f;
        [SerializeField] private float attackPower = 10.0f;

        [Header("Runtime State")]
        [SerializeField] private float currentHealth = 100.0f;

        public string EnemyLabel => enemyLabel;
        public float MaxHealth => maxHealth;
        public float AttackPower => attackPower;
        public float CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
        }

        private void OnValidate()
        {
            enemyLabel = string.IsNullOrWhiteSpace(enemyLabel) ? "Mono Enemy" : enemyLabel.Trim();
            maxHealth = Mathf.Max(1.0f, maxHealth);
            attackPower = Mathf.Max(0.0f, attackPower);
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
        }

        /// <summary>
        /// 데모 초기 배치를 위해 로컬 기준 스탯을 설정합니다.
        /// </summary>
        public void ConfigureLocalBaseStats(string label, float configuredMaxHealth, float configuredAttackPower)
        {
            enemyLabel = string.IsNullOrWhiteSpace(label) ? "Mono Enemy" : label.Trim();
            maxHealth = Mathf.Max(1.0f, configuredMaxHealth);
            attackPower = Mathf.Max(0.0f, configuredAttackPower);
            currentHealth = maxHealth;
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
        /// 현재 체력을 기준 최대 체력으로 복구합니다.
        /// </summary>
        public void ResetRuntimeState()
        {
            currentHealth = maxHealth;
        }

        /// <summary>
        /// 개별 인스턴스의 로컬 최대 체력만 변경합니다.
        /// </summary>
        public void SetLocalMaxHealth(float newMaxHealth)
        {
            maxHealth = Mathf.Max(1.0f, newMaxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
        }
    }
}
