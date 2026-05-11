using NUnit.Framework;
using SystemicOverload.Data;
using SystemicOverload.Rpg;
using UnityEditor;
using UnityEngine;

namespace SystemicOverload.Tests.EditMode
{
    /// <summary>
    /// 한국어 주석: 싱글 RPG 퀘스트 진행(풀 적 처치 카운트)의 최소 회귀를 EditMode에서 검증합니다.
    /// </summary>
    public sealed class QuestServiceEditModeTests
    {
        private GameObject rootGameObject;
        private QuestDefinition questDefinitionAsset;

        [SetUp]
        public void Setup()
        {
            rootGameObject = new GameObject("QuestServiceTestRoot");
            rootGameObject.AddComponent<WorldStateService>();
            QuestService questService = rootGameObject.AddComponent<QuestService>();

            questDefinitionAsset = ScriptableObject.CreateInstance<QuestDefinition>();
            SerializedObject questSerialized = new SerializedObject(questDefinitionAsset);
            questSerialized.FindProperty("targetPooledEnemyKills").intValue = 3;
            questSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serviceSerialized = new SerializedObject(questService);
            serviceSerialized.FindProperty("demoQuest").objectReferenceValue = questDefinitionAsset;
            serviceSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Teardown()
        {
            if (questDefinitionAsset != null)
            {
                Object.DestroyImmediate(questDefinitionAsset);
            }

            if (rootGameObject != null)
            {
                Object.DestroyImmediate(rootGameObject);
            }
        }

        [Test]
        public void NotifyPooledEnemyKilled_IncrementsCounter()
        {
            QuestService.NotifyPooledEnemyKilled();
            QuestService questService = rootGameObject.GetComponent<QuestService>();

            Assert.AreEqual(1, questService.PooledEnemyKillCount);
        }

        [Test]
        public void KillCount_ReachesTarget_MarksDemoComplete()
        {
            QuestService.NotifyPooledEnemyKilled();
            QuestService.NotifyPooledEnemyKilled();
            QuestService.NotifyPooledEnemyKilled();
            QuestService questService = rootGameObject.GetComponent<QuestService>();

            Assert.IsTrue(questService.IsDemoQuestComplete);
        }
    }
}
