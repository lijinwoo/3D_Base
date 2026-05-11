using System.Collections;
using NUnit.Framework;
using SystemicOverload.Combat;
using SystemicOverload.Phase1;
using UnityEngine;
using UnityEngine.TestTools;

namespace SystemicOverload.Tests.PlayMode
{
    /// <summary>
    /// 한국어 주석: CombatComponent의 self-hit skip이 자식 collider/동일 root 상황에서 의도대로 동작하는지 검증합니다.
    /// </summary>
    public sealed class CombatComponentSelfHitPlayModeTests
    {
        private GameObject attackerGameObject;
        private GameObject targetGameObject;
        private CombatComponent combatUnderTest;
        private HealthComponent targetHealthComponent;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            attackerGameObject = new GameObject("Attacker");
            // 한국어 주석: 자기 자신을 가리도록 self collider를 자식에 배치합니다(무기 hitbox 모사).
            GameObject selfHitboxChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
            selfHitboxChild.name = "SelfHitbox";
            selfHitboxChild.transform.SetParent(attackerGameObject.transform, false);
            selfHitboxChild.transform.localPosition = new Vector3(0.0f, 1.0f, 0.5f);
            selfHitboxChild.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            attackerGameObject.AddComponent<InputProvider>();
            combatUnderTest = attackerGameObject.AddComponent<CombatComponent>();
            // 한국어 주석: 데미지 1.0, 짧은 사거리, 모든 레이어. 인스펙터 값을 reflection으로 직접 주입합니다.
            SetPrivateField(combatUnderTest, "damage", 1.0f);
            SetPrivateField(combatUnderTest, "shotsPerSecond", 100.0f);
            SetPrivateField(combatUnderTest, "maxRange", 50.0f);
            SetPrivateField(combatUnderTest, "rayOriginHeight", 1.0f);
            SetPrivateField(combatUnderTest, "rayStartForwardOffset", 0.0f);
            SetPrivateField(combatUnderTest, "hitLayerMask", (LayerMask)~0);

            targetGameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            targetGameObject.name = "TargetDummy";
            targetGameObject.transform.position = new Vector3(0.0f, 1.0f, 5.0f);
            targetHealthComponent = targetGameObject.AddComponent<HealthComponent>();
            SetPrivateField(targetHealthComponent, "maxHealth", 10.0f);
            SetPrivateField(targetHealthComponent, "currentHealth", 10.0f);

            // 한국어 주석: 어태커가 +Z 방향을 향하도록 forward를 명시합니다.
            attackerGameObject.transform.position = Vector3.zero;
            attackerGameObject.transform.rotation = Quaternion.identity;

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            if (attackerGameObject != null)
            {
                Object.DestroyImmediate(attackerGameObject);
            }

            if (targetGameObject != null)
            {
                Object.DestroyImmediate(targetGameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator TryFireHitScan_SkipsSelfChild_AndDamagesTarget()
        {
            // 한국어 주석: private TryFireHitScan을 reflection으로 호출해 입력 의존성을 우회합니다.
            System.Reflection.MethodInfo fireMethod = typeof(CombatComponent).GetMethod(
                "TryFireHitScan",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(fireMethod, "TryFireHitScan 메서드를 찾지 못했습니다.");

            float healthBeforeShot = targetHealthComponent.CurrentHealth;
            fireMethod.Invoke(combatUnderTest, null);
            yield return null;

            Assert.Less(targetHealthComponent.CurrentHealth, healthBeforeShot,
                "self collider가 앞에 있어도 다음 hit인 적에게 데미지를 적용해야 합니다.");
        }

        private static void SetPrivateField(object targetObject, string fieldName, object value)
        {
            System.Reflection.FieldInfo fieldInfo = targetObject.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(fieldInfo, $"필드를 찾지 못했습니다: {fieldName}");
            fieldInfo.SetValue(targetObject, value);
        }
    }
}
