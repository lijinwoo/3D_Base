using System;
using System.Collections.Generic;
using UnityEngine;

namespace SystemicOverload.Rpg
{
    /// <summary>
    /// 한국어 주석: 월드 플래그·정수 상태를 보관합니다. 세이브 데이터와 동기화할 수 있습니다.
    /// </summary>
    public sealed class WorldStateService : MonoBehaviour
    {
        public static WorldStateService Instance { get; private set; }

        private readonly Dictionary<string, int> intFlags = new Dictionary<string, int>(StringComparer.Ordinal);

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return intFlags.TryGetValue(key, out int value) ? value : defaultValue;
        }

        public void SetInt(string key, int value)
        {
            intFlags[key] = value;
        }

        /// <summary>
        /// 한국어 주석: 세이브 로드 시 전체 맵을 교체합니다.
        /// </summary>
        public void ReplaceAllFromPairs(IReadOnlyList<string> keys, IReadOnlyList<int> values)
        {
            intFlags.Clear();
            if (keys == null || values == null)
            {
                return;
            }

            int count = Mathf.Min(keys.Count, values.Count);
            for (int index = 0; index < count; index++)
            {
                if (!string.IsNullOrEmpty(keys[index]))
                {
                    intFlags[keys[index]] = values[index];
                }
            }
        }

        /// <summary>
        /// 한국어 주석: JSON 세이브에서 복원할 때 배열 형태로 주입합니다.
        /// </summary>
        public void ReplaceAllFromArrays(string[] keys, int[] values)
        {
            intFlags.Clear();
            if (keys == null || values == null)
            {
                return;
            }

            int count = Mathf.Min(keys.Length, values.Length);
            for (int index = 0; index < count; index++)
            {
                if (!string.IsNullOrEmpty(keys[index]))
                {
                    intFlags[keys[index]] = values[index];
                }
            }
        }

        public void CopyToLists(List<string> keysOut, List<int> valuesOut)
        {
            keysOut.Clear();
            valuesOut.Clear();
            foreach (KeyValuePair<string, int> pair in intFlags)
            {
                keysOut.Add(pair.Key);
                valuesOut.Add(pair.Value);
            }
        }

        /// <summary>
        /// 한국어 주석: 세이브 직렬화용으로 배열로 복사합니다.
        /// </summary>
        public void CopyToArrays(out string[] keysOut, out int[] valuesOut)
        {
            int count = intFlags.Count;
            keysOut = new string[count];
            valuesOut = new int[count];
            int index = 0;
            foreach (KeyValuePair<string, int> pair in intFlags)
            {
                keysOut[index] = pair.Key;
                valuesOut[index] = pair.Value;
                index++;
            }
        }
    }
}
