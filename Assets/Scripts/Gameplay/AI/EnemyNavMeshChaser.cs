using UnityEngine;
using UnityEngine.AI;

namespace SystemicOverload.AI
{
    /// <summary>
    /// 한국어 주석: NavMeshAgent로 플레이어를 추격하는 단순 AI입니다.
    /// 우선 <see cref="T:SystemicOverload.Rpg.PlayerTargetProvider"/>를 사용하고, 없을 때만 이름 기반 탐색으로 폴백합니다.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyAiBlackboard))]
    public sealed class EnemyNavMeshChaser : MonoBehaviour
    {
        [SerializeField] private string playerObjectName = "Player";
        [SerializeField] private float repathIntervalSeconds = 0.35f;
        [SerializeField] private float stoppingDistance = 1.25f;

        private NavMeshAgent navMeshAgent;
        private EnemyAiBlackboard blackboard;
        private float nextRepathTime;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            blackboard = GetComponent<EnemyAiBlackboard>();
            navMeshAgent.stoppingDistance = stoppingDistance;
        }

        private void Start()
        {
            global::SystemicOverload.Rpg.PlayerTargetProvider activePlayerTarget =
                global::SystemicOverload.Rpg.PlayerTargetProvider.Active;
            if (activePlayerTarget != null)
            {
                blackboard.BindChaseTarget(activePlayerTarget.TargetTransform);
                return;
            }

            GameObject playerObject = GameObject.Find(playerObjectName);
            if (playerObject != null)
            {
                blackboard.BindChaseTarget(playerObject.transform);
            }
            else
            {
                Debug.LogWarning($"[EnemyNavMeshChaser] PlayerTargetProvider가 없고 '{playerObjectName}' 오브젝트도 찾지 못했습니다.", this);
            }
        }

        private void Update()
        {
            if (blackboard == null || navMeshAgent == null || !blackboard.HasChaseTarget)
            {
                return;
            }

            if (Time.time < nextRepathTime)
            {
                return;
            }

            nextRepathTime = Time.time + Mathf.Max(0.05f, repathIntervalSeconds);
            navMeshAgent.SetDestination(blackboard.LastKnownTargetPosition);
        }
    }
}
