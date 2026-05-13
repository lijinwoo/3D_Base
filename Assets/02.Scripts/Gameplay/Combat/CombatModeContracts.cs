using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 공격 모듈이 공통으로 받는 실행 데이터입니다.
    /// </summary>
    public readonly struct AttackExecutionContext
    {
        public AttackExecutionContext(Transform owner, Vector3 origin, Vector3 direction, float maxDistance, float baseDamage, LayerMask targetLayerMask)
        {
            Owner = owner;
            Origin = origin;
            Direction = direction;
            MaxDistance = maxDistance;
            BaseDamage = baseDamage;
            TargetLayerMask = targetLayerMask;
        }

        public Transform Owner { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float MaxDistance { get; }
        public float BaseDamage { get; }
        public LayerMask TargetLayerMask { get; }
    }

    /// <summary>
    /// 공격 방식을 독립 컴포넌트로 조합하기 위한 계약입니다.
    /// </summary>
    public interface ICombatAttackMode
    {
        string ModeName { get; }
        bool TryAttack(in AttackExecutionContext attackExecutionContext);
    }
}
