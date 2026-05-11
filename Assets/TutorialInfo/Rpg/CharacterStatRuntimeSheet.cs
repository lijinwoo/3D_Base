using SystemicOverload.Data;
using UnityEngine;

namespace SystemicOverload.Rpg
{
    /// <summary>
    /// 한국어 주석: <see cref="StatData"/> 템플릿을 기준으로 런타임 레벨·보정값을 누적하는 최소 성장 시트입니다.
    /// 장비·버프 확장 시 이 클래스에 modifier 스택을 추가합니다.
    /// </summary>
    public sealed class CharacterStatRuntimeSheet : MonoBehaviour
    {
        [SerializeField] private StatData baseStatTemplate;
        [SerializeField] private int characterLevel = 1;

        public int CharacterLevel => Mathf.Max(1, characterLevel);

        /// <summary>
        /// 한국어 주석: 세이브 등에서 레벨을 복원합니다.
        /// </summary>
        public void SetCharacterLevel(int level)
        {
            characterLevel = Mathf.Max(1, level);
        }

        /// <summary>
        /// 한국어 주석: 최대 체력 계산(현재는 템플릿 그대로, 향후 레벨 계수 적용).
        /// </summary>
        public float ComputeMaxHealth()
        {
            if (baseStatTemplate == null)
            {
                return 100.0f;
            }

            return baseStatTemplate.MaxHealth;
        }
    }
}
