using UnityEngine;

namespace SystemicOverload.Data
{
    /// <summary>
    /// Enemy의 기본 전투 데이터를 정의하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "SystemicOverload/Gameplay/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinitionSO : ScriptableObject
    {
        [SerializeField] private string enemyId = "enemy.dummy";
        [SerializeField] private float maxHealth = 100.0f;
        [SerializeField] private float initialHealth = 100.0f;

        public string EnemyId => enemyId;
        public float MaxHealth => maxHealth;
        public float InitialHealth => Mathf.Clamp(initialHealth, 0.0f, maxHealth);

        private void OnValidate()
        {
            enemyId = string.IsNullOrWhiteSpace(enemyId) ? "enemy.undefined" : enemyId.Trim();
            maxHealth = Mathf.Max(1.0f, maxHealth);
            initialHealth = Mathf.Clamp(initialHealth, 0.0f, maxHealth);
        }
    }
}
