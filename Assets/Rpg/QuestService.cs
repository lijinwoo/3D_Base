using SystemicOverload.Data;
using UnityEngine;

namespace SystemicOverload.Rpg
{
    /// <summary>
    /// 한국어 주석: 풀 기반 적 처치 등 최소 퀘스트 진행을 추적하고 세이브와 동기화합니다.
    /// </summary>
    public sealed class QuestService : MonoBehaviour
    {
        public static QuestService Instance { get; private set; }

        [SerializeField] private QuestDefinition demoQuest;

        private int pooledEnemyKillCount;

        public int PooledEnemyKillCount => pooledEnemyKillCount;

        public bool IsDemoQuestComplete =>
            demoQuest != null && pooledEnemyKillCount >= demoQuest.TargetPooledEnemyKills;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ReloadFromDisk();
        }

        /// <summary>
        /// 한국어 주석: 디스크 세이브를 읽어 퀘스트·월드 상태를 복원합니다. 런타임 Start와 에디터 테스트에서 공통 사용합니다.
        /// </summary>
        public void ReloadFromDisk()
        {
            SaveLoadService.GameSaveData loaded = SaveLoadService.LoadOrCreate();
            pooledEnemyKillCount = loaded.pooledEnemyKillCount;
            if (WorldStateService.Instance != null)
            {
                WorldStateService.Instance.ReplaceAllFromArrays(loaded.worldFlagKeys, loaded.worldFlagValues);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 한국어 주석: 풀링된 적이 사망 처리될 때 호출합니다(정적 진입점).
        /// </summary>
        public static void NotifyPooledEnemyKilled()
        {
            if (Instance == null)
            {
                return;
            }

            Instance.pooledEnemyKillCount++;
            Instance.Persist();
        }

        private void Persist()
        {
            string[] worldKeys = System.Array.Empty<string>();
            int[] worldValues = System.Array.Empty<int>();
            if (WorldStateService.Instance != null)
            {
                WorldStateService.Instance.CopyToArrays(out worldKeys, out worldValues);
            }

            var data = new SaveLoadService.GameSaveData
            {
                pooledEnemyKillCount = pooledEnemyKillCount,
                worldFlagKeys = worldKeys,
                worldFlagValues = worldValues,
            };

            SaveLoadService.Save(data);
        }
    }
}
