using UnityEngine;

namespace SystemicOverload.Comparison
{
    /// <summary>
    /// ScriptableObject 기반 공유 스탯 데이터입니다.
    /// 여러 적 인스턴스가 동일 에셋을 참조할 때 값이 함께 변하는 특성을 관찰하는 데 사용합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "SystemicOverload/Gameplay/Comparison/Enemy Config", fileName = "ComparisonEnemyConfig")]
    public sealed class ComparisonEnemyConfigSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyId = "enemy.goblin.shared";

        [Header("Base Stats")]
        [SerializeField] private float maxHealth = 100.0f;
        [SerializeField] private float attackPower = 10.0f;

        public string EnemyId => enemyId;
        public float MaxHealth => maxHealth;
        public float AttackPower => attackPower;

        private void OnValidate()
        {
            enemyId = string.IsNullOrWhiteSpace(enemyId) ? "enemy.undefined" : enemyId.Trim();
            maxHealth = Mathf.Max(1.0f, maxHealth);
            attackPower = Mathf.Max(0.0f, attackPower);
        }

        /// <summary>
        /// 데모 목적의 런타임 원본 수정 API입니다.
        /// 공유 에셋 값을 바꾸면 이를 참조하는 모든 객체의 기준값이 동시에 변합니다.
        /// </summary>
        public void SetMaxHealthForDemo(float newMaxHealth)
        {
            maxHealth = Mathf.Max(1.0f, newMaxHealth);
        }
    }
}
