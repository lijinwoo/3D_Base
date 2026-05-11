using UnityEngine;

namespace SystemicOverload.Rpg
{
    /// <summary>
    /// 한국어 주석: 플레이어 Transform을 전역으로 등록해 AI 등이 이름 검색 없이 접근합니다.
    /// </summary>
    public sealed class PlayerTargetProvider : MonoBehaviour
    {
        public static PlayerTargetProvider Active { get; private set; }

        public Transform TargetTransform => transform;

        private void OnEnable()
        {
            Active = this;
        }

        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
            }
        }
    }
}
