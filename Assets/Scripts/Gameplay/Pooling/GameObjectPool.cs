using System.Collections.Generic;
using UnityEngine;

namespace SystemicOverload.Pooling
{
    /// <summary>
    /// 한국어 주석: 단일 프리팹 기준의 간단한 GameObject Pool입니다. 인카운터·VFX 등 싱글 RPG 런타임에서 재사용합니다.
    /// 중복 Release/외부 인스턴스 반환을 ownership 추적으로 차단하고, IPooledObject 훅을 통해 재사용 초기화를 일관화합니다.
    /// </summary>
    public sealed class GameObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int prewarmCount = 4;
        [SerializeField] private Transform poolRoot;
        [SerializeField] private bool resetAnimatorOnSpawn = true;
        [Tooltip("스폰 시 PooledReturnTrigger가 있으면 자동으로 풀 참조를 주입합니다. lifetime은 트리거 컴포넌트의 기본값을 사용합니다.")]
        [SerializeField] private bool autoConfigurePooledReturnTrigger = true;

        private readonly Queue<GameObject> availableInstances = new Queue<GameObject>();
        // 한국어 주석: 이 풀이 만든 인스턴스만 추적합니다(외부 GameObject 오반환 차단).
        private readonly HashSet<GameObject> ownedInstances = new HashSet<GameObject>();
        // 한국어 주석: 현재 풀 큐 안에 들어 있는 인스턴스(중복 Release 차단용).
        private readonly HashSet<GameObject> instancesInPool = new HashSet<GameObject>();
        private bool warmedUp;

        public GameObject Prefab => prefab;
        public int AvailableCount => availableInstances.Count;
        public int OwnedCount => ownedInstances.Count;

        private void Awake()
        {
            if (poolRoot == null)
            {
                GameObject rootObject = new GameObject($"{name}_PoolRoot");
                rootObject.transform.SetParent(transform, false);
                poolRoot = rootObject.transform;
            }
        }

        private void Start()
        {
            PrewarmIfNeeded();
        }

        /// <summary>
        /// 한국어 주석: 미리 인스턴스를 만들어 두어 첫 스폰 시 GC 스파이크를 줄입니다.
        /// </summary>
        public void PrewarmIfNeeded()
        {
            if (warmedUp || prefab == null)
            {
                return;
            }

            warmedUp = true;
            for (int index = 0; index < prewarmCount; index++)
            {
                GameObject instance = CreateNewInstance();
                instance.SetActive(false);
                availableInstances.Enqueue(instance);
                instancesInPool.Add(instance);
            }
        }

        /// <summary>
        /// 한국어 주석: 풀에서 꺼내거나 새로 생성해 활성화합니다.
        /// </summary>
        public GameObject Get(Vector3 worldPosition, Quaternion worldRotation)
        {
            PrewarmIfNeeded();
            GameObject instance = TryDequeueExisting();
            if (instance == null)
            {
                if (prefab == null)
                {
                    Debug.LogError("[GameObjectPool] Prefab이 비어 있습니다.", this);
                    return null;
                }

                instance = CreateNewInstance();
            }

            instance.transform.SetPositionAndRotation(worldPosition, worldRotation);
            instance.SetActive(true);
            ApplyAnimatorResetIfNeeded(instance);
            ConfigurePooledReturnTriggerIfNeeded(instance);
            NotifySpawned(instance);
            return instance;
        }

        /// <summary>
        /// 한국어 주석: 인스턴스를 비활성화하고 풀에 반환합니다. 중복 반환과 외부 인스턴스 반환을 방지합니다.
        /// </summary>
        public void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            // 한국어 주석: 이 풀이 만든 인스턴스가 아니면 받지 않습니다(풀 오염 방지).
            if (!ownedInstances.Contains(instance))
            {
                Debug.LogWarning($"[GameObjectPool] 이 풀이 만들지 않은 인스턴스 반환 시도를 무시합니다: {instance.name}", this);
                return;
            }

            // 한국어 주석: 이미 풀에 들어 있는 인스턴스의 중복 Release를 차단합니다.
            if (instancesInPool.Contains(instance))
            {
                Debug.LogWarning($"[GameObjectPool] 중복 Release를 차단했습니다: {instance.name}", this);
                return;
            }

            NotifyReturned(instance);
            instance.SetActive(false);
            instance.transform.SetParent(poolRoot, false);
            availableInstances.Enqueue(instance);
            instancesInPool.Add(instance);
        }

        private GameObject CreateNewInstance()
        {
            GameObject instance = Instantiate(prefab, poolRoot);
            instance.name = $"{prefab.name}_Pooled";
            ownedInstances.Add(instance);
            PooledInstanceLink link = instance.GetComponent<PooledInstanceLink>();
            if (link == null)
            {
                link = instance.AddComponent<PooledInstanceLink>();
            }

            link.Initialize(this);
            return instance;
        }

        private GameObject TryDequeueExisting()
        {
            while (availableInstances.Count > 0)
            {
                GameObject candidate = availableInstances.Dequeue();
                if (candidate == null)
                {
                    // 한국어 주석: 외부 파괴된 인스턴스는 ownership과 in-pool 집합에서도 정리합니다.
                    continue;
                }

                instancesInPool.Remove(candidate);
                return candidate;
            }

            return null;
        }

        private void ApplyAnimatorResetIfNeeded(GameObject instance)
        {
            if (!resetAnimatorOnSpawn || instance == null)
            {
                return;
            }

            Animator animator = instance.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                return;
            }

            // 한국어 주석: 재사용 시 상태가 고착되지 않도록 바인딩을 초기화합니다.
            animator.Rebind();
            animator.Update(0.0f);
        }

        private void ConfigurePooledReturnTriggerIfNeeded(GameObject instance)
        {
            if (!autoConfigurePooledReturnTrigger || instance == null)
            {
                return;
            }

            PooledReturnTrigger trigger = instance.GetComponent<PooledReturnTrigger>();
            if (trigger == null)
            {
                return;
            }

            // 한국어 주석: 수동 연결 누락을 막기 위해 풀 참조를 자동 주입합니다. lifetime은 트리거 인스펙터 값을 그대로 사용합니다.
            trigger.AttachOwner(this);
        }

        private static void NotifySpawned(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            // 한국어 주석: 단일 GameObject 트리에 여러 IPooledObject가 있어도 모두 호출합니다.
            IPooledObject[] handlers = instance.GetComponentsInChildren<IPooledObject>(true);
            for (int handlerIndex = 0; handlerIndex < handlers.Length; handlerIndex++)
            {
                handlers[handlerIndex].OnSpawnedFromPool();
            }
        }

        private static void NotifyReturned(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            IPooledObject[] handlers = instance.GetComponentsInChildren<IPooledObject>(true);
            for (int handlerIndex = 0; handlerIndex < handlers.Length; handlerIndex++)
            {
                handlers[handlerIndex].OnReturnedToPool();
            }
        }
    }
}
