using UnityEngine;

namespace SystemicOverload.Pooling
{
    /// <summary>
    /// 한국어 주석: 풀에서 생성된 인스턴스가 자신을 소유한 <see cref="GameObjectPool"/>을 참조합니다.
    /// 사망 시 반환 등 런타임에서 풀 참조가 필요할 때 사용합니다.
    /// </summary>
    public sealed class PooledInstanceLink : MonoBehaviour
    {
        [SerializeField] private GameObjectPool ownerPool;

        public GameObjectPool OwnerPool => ownerPool;

        /// <summary>
        /// 한국어 주석: <see cref="GameObjectPool"/>이 인스턴스를 만들 때 한 번 호출합니다.
        /// </summary>
        public void Initialize(GameObjectPool pool)
        {
            ownerPool = pool;
        }
    }
}
