using UnityEngine;

namespace SystemicOverload.Data
{
    /// <summary>
    /// 한국어 주석: 싱글 RPG에서 사용할 퀘스트 정의(목표 수·식별자)의 최소 단위입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestDefinition", menuName = "Systemic Overload/Data/Quest Definition", order = 3)]
    public sealed class QuestDefinition : ScriptableObject
    {
        [SerializeField] private string questId = "demo_clear_pooled_enemies";
        [SerializeField] private int targetPooledEnemyKills = 4;

        public string QuestId => questId;
        public int TargetPooledEnemyKills => Mathf.Max(1, targetPooledEnemyKills);

        private void OnValidate()
        {
            targetPooledEnemyKills = Mathf.Max(1, targetPooledEnemyKills);
        }
    }
}
