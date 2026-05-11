using NUnit.Framework;
using SystemicOverload.Rpg;
using UnityEngine;

namespace SystemicOverload.Tests.EditMode
{
    /// <summary>
    /// 한국어 주석: 세이브 DTO의 JSON 직렬화가 싱글 RPG 필드를 유실 없이 보존하는지 검증합니다.
    /// </summary>
    public sealed class SaveLoadDtoRoundtripEditModeTests
    {
        [Test]
        public void GameSaveData_JsonUtility_Roundtrip_PreservesFields()
        {
            var original = new SaveLoadService.GameSaveData
            {
                pooledEnemyKillCount = 7,
                worldFlagKeys = new[] { "gate_open", "boss_defeated" },
                worldFlagValues = new[] { 1, 0 },
            };

            string json = JsonUtility.ToJson(original);
            SaveLoadService.GameSaveData restored = JsonUtility.FromJson<SaveLoadService.GameSaveData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(7, restored.pooledEnemyKillCount);
            Assert.AreEqual(2, restored.worldFlagKeys.Length);
            Assert.AreEqual("gate_open", restored.worldFlagKeys[0]);
            Assert.AreEqual(1, restored.worldFlagValues[0]);
        }
    }
}
