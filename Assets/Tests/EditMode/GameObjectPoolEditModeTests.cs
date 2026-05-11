using NUnit.Framework;
using SystemicOverload.Pooling;
using UnityEngine;

namespace SystemicOverload.Tests.EditMode
{
    /// <summary>
    /// 한국어 주석: GameObjectPool의 ownership·중복 반환 방지·자동 prewarm 동작을 EditMode에서 검증합니다.
    /// </summary>
    public sealed class GameObjectPoolEditModeTests
    {
        private GameObject poolHostGameObject;
        private GameObjectPool poolUnderTest;
        private GameObject samplePrefab;

        [SetUp]
        public void Setup()
        {
            samplePrefab = new GameObject("PoolPrefabSample");
            // 한국어 주석: 풀 호스트는 별도의 GameObject로 두어 인스턴스 부모/자식 분리를 명확히 합니다.
            poolHostGameObject = new GameObject("PoolHost");
            poolUnderTest = poolHostGameObject.AddComponent<GameObjectPool>();
            // 한국어 주석: 인스펙터 연결을 시뮬레이션하기 위해 SerializedObject 대신 reflection을 사용합니다(테스트 한정).
            typeof(GameObjectPool)
                .GetField("prefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(poolUnderTest, samplePrefab);
            typeof(GameObjectPool)
                .GetField("prewarmCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(poolUnderTest, 0);
        }

        [TearDown]
        public void Teardown()
        {
            if (poolHostGameObject != null)
            {
                Object.DestroyImmediate(poolHostGameObject);
            }

            if (samplePrefab != null)
            {
                Object.DestroyImmediate(samplePrefab);
            }
        }

        [Test]
        public void Get_ReturnsActiveInstance_OwnedByPool()
        {
            GameObject spawned = poolUnderTest.Get(Vector3.zero, Quaternion.identity);

            Assert.IsNotNull(spawned, "Get은 유효한 인스턴스를 반환해야 합니다.");
            Assert.IsTrue(spawned.activeSelf, "Get 직후 인스턴스는 활성화 상태여야 합니다.");
            Assert.AreEqual(1, poolUnderTest.OwnedCount, "OwnedCount는 1이어야 합니다.");
            Assert.AreEqual(0, poolUnderTest.AvailableCount, "활성 중인 인스턴스는 풀 큐에 없어야 합니다.");
        }

        [Test]
        public void Release_TwiceOnSameInstance_LogsWarningAndKeepsSingleEntry()
        {
            GameObject spawned = poolUnderTest.Get(Vector3.zero, Quaternion.identity);

            poolUnderTest.Release(spawned);
            // 한국어 주석: 두 번째 Release는 풀 오염을 일으키지 않아야 합니다.
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("중복 Release"));
            poolUnderTest.Release(spawned);

            Assert.AreEqual(1, poolUnderTest.AvailableCount, "중복 Release는 큐에 항목을 추가하지 않아야 합니다.");
        }

        [Test]
        public void Release_WithForeignInstance_IsRejected()
        {
            GameObject foreignInstance = new GameObject("ForeignInstance");

            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("이 풀이 만들지 않은"));
            poolUnderTest.Release(foreignInstance);

            Assert.AreEqual(0, poolUnderTest.AvailableCount, "외부 인스턴스 반환은 큐에 영향을 주지 않아야 합니다.");
            Object.DestroyImmediate(foreignInstance);
        }

        [Test]
        public void GetAfterRelease_ReusesQueuedInstance()
        {
            GameObject firstSpawn = poolUnderTest.Get(Vector3.zero, Quaternion.identity);
            poolUnderTest.Release(firstSpawn);
            GameObject secondSpawn = poolUnderTest.Get(Vector3.zero, Quaternion.identity);

            Assert.AreSame(firstSpawn, secondSpawn, "재사용된 인스턴스는 동일 reference여야 합니다.");
            Assert.AreEqual(1, poolUnderTest.OwnedCount, "재사용은 새 인스턴스를 만들지 않아야 합니다.");
        }

        [Test]
        public void Get_AttachesPooledInstanceLinkPointingToOwnerPool()
        {
            GameObject spawned = poolUnderTest.Get(Vector3.zero, Quaternion.identity);
            PooledInstanceLink link = spawned.GetComponent<PooledInstanceLink>();

            Assert.IsNotNull(link, "풀에서 꺼낸 인스턴스에는 PooledInstanceLink가 있어야 합니다.");
            Assert.AreSame(poolUnderTest, link.OwnerPool, "OwnerPool은 이 풀을 가리켜야 합니다.");
        }
    }
}
