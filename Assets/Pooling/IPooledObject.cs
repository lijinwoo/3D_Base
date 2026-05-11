namespace SystemicOverload.Pooling
{
    /// <summary>
    /// 한국어 주석: 풀에서 스폰/반환되는 시점에 상태를 초기화하기 위한 공통 훅 인터페이스입니다.
    /// <see cref="PooledHealthReset"/>처럼 OnEnable에 의존하지 않고도 명시적으로 라이프사이클을 제어할 수 있게 해줍니다.
    /// </summary>
    public interface IPooledObject
    {
        /// <summary>
        /// 한국어 주석: 풀이 인스턴스를 활성화한 직후 호출됩니다(Get).
        /// </summary>
        void OnSpawnedFromPool();

        /// <summary>
        /// 한국어 주석: 풀이 인스턴스를 비활성화하기 직전 호출됩니다(Release).
        /// </summary>
        void OnReturnedToPool();
    }
}
