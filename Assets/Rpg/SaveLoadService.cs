using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SystemicOverload.Rpg
{
    /// <summary>
    /// 한국어 주석: 싱글 RPG 진행 상태를 JSON 파일로 저장·로드합니다.
    /// </summary>
    public static class SaveLoadService
    {
        private const string SaveFileName = "systemic_overload_rpg_save.json";

        public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        /// <summary>
        /// 한국어 주석: 디스크에 저장할 DTO입니다.
        /// </summary>
        [Serializable]
        public sealed class GameSaveData
        {
            public int pooledEnemyKillCount;
            public string[] worldFlagKeys = System.Array.Empty<string>();
            public int[] worldFlagValues = System.Array.Empty<int>();
        }

        public static GameSaveData LoadOrCreate()
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                {
                    return new GameSaveData();
                }

                string json = File.ReadAllText(SaveFilePath);
                GameSaveData loaded = JsonUtility.FromJson<GameSaveData>(json);
                return loaded ?? new GameSaveData();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveLoadService] 로드 실패, 새 세이브로 시작합니다: {exception.Message}");
                return new GameSaveData();
            }
        }

        public static void Save(GameSaveData data)
        {
            if (data == null)
            {
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveLoadService] 저장 실패: {exception.Message}");
            }
        }
    }
}
