using System.Collections;
using SystemicOverload.Data;
using SystemicOverload.Pooling;
using UnityEngine;
using UnityEngine.Serialization;

namespace SystemicOverload.Encounter
{
    /// <summary>
    /// 한국어 주석: <see cref="EncounterSpawnData"/>에 따라 풀에서 적을 꺼내 배치합니다.
    /// 퀘스트·월드 상태는 <see cref="Rpg.QuestService"/> 등에서 별도로 구독/저장하며, 기본은 씬 시작 시 자동 실행입니다.
    /// </summary>
    public sealed class EncounterDirector : MonoBehaviour
    {
        [FormerlySerializedAs("waveData")]
        [SerializeField]
        private EncounterSpawnData encounterSpawnData;

        [SerializeField] private GameObjectPool enemyPool;
        [SerializeField] private Transform spawnAnchor;

        private Coroutine runningEncounterRoutine;

        private void OnValidate()
        {
            if (spawnAnchor == null)
            {
                spawnAnchor = transform;
            }
        }

        private void Start()
        {
            if (encounterSpawnData == null || enemyPool == null)
            {
                Debug.LogWarning("[EncounterDirector] EncounterSpawnData 또는 GameObjectPool이 비어 있습니다.", this);
                return;
            }

            if (runningEncounterRoutine != null)
            {
                StopCoroutine(runningEncounterRoutine);
            }

            runningEncounterRoutine = StartCoroutine(RunEncounterRoutine());
        }

        private IEnumerator RunEncounterRoutine()
        {
            Vector3 center = (spawnAnchor != null ? spawnAnchor.position : transform.position) + encounterSpawnData.SpawnCenterWorldOffset;
            for (int spawnIndex = 0; spawnIndex < encounterSpawnData.EnemyCount; spawnIndex++)
            {
                Vector2 disk = Random.insideUnitCircle * encounterSpawnData.SpawnRadius;
                Vector3 spawnPosition = center + new Vector3(disk.x, 0.0f, disk.y);
                Quaternion spawnRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                GameObject spawned = enemyPool.Get(spawnPosition, spawnRotation);
                if (spawned != null)
                {
                    PooledHealthReset healthReset = spawned.GetComponent<PooledHealthReset>();
                    healthReset?.ApplyFullHealth();
                }

                yield return new WaitForSeconds(encounterSpawnData.SpawnIntervalSeconds);
            }
        }
    }
}
