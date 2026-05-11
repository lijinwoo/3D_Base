using UnityEngine;

namespace SystemicOverload.Data
{
    /// <summary>
    /// 한국어 주석: 인카운터(구역 이벤트·퀘스트 연동 등)에서 풀 기반으로 적을 배치할 때 사용하는 스폰 파라미터를 정의합니다.
    /// Roguelite의 연속 Wave 개념 대신, 싱글 RPG에서 재사용 가능한 스폰 테이블 역할을 합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "EncounterSpawnData", menuName = "Systemic Overload/Data/Encounter Spawn Data", order = 2)]
    public sealed class EncounterSpawnData : ScriptableObject
    {
        [Header("Spawn")]
        [SerializeField] private GameObject pooledEnemyPrefab;
        [SerializeField] private int enemyCount = 5;
        [SerializeField] private float spawnIntervalSeconds = 1.25f;
        [SerializeField] private float spawnRadius = 8.0f;
        [SerializeField] private Vector3 spawnCenterWorldOffset = new Vector3(0.0f, 0.0f, 12.0f);

        public GameObject PooledEnemyPrefab => pooledEnemyPrefab;
        public int EnemyCount => Mathf.Max(0, enemyCount);
        public float SpawnIntervalSeconds => Mathf.Max(0.05f, spawnIntervalSeconds);
        public float SpawnRadius => Mathf.Max(0.5f, spawnRadius);
        public Vector3 SpawnCenterWorldOffset => spawnCenterWorldOffset;

        private void OnValidate()
        {
            enemyCount = Mathf.Max(0, enemyCount);
            spawnIntervalSeconds = Mathf.Max(0.05f, spawnIntervalSeconds);
            spawnRadius = Mathf.Max(0.5f, spawnRadius);
        }
    }
}
