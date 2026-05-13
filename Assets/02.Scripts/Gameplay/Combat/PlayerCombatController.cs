using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 공격 모드들을 합성으로 관리하고 입력/모드 전환을 라우팅합니다.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerCombatController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private PlayerInput playerInput;

        [Header("Attack")]
        [SerializeField] private float baseDamage = 15.0f;
        [SerializeField] private float attackRange = 16.0f;
        [SerializeField] private LayerMask attackTargetLayerMask = ~0;
        [SerializeField] private float attacksPerSecond = 3.0f;

        [Header("Input")]
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string attackActionName = "Fire";
        [SerializeField] private string mode1ActionName = "CombatMode1";
        [SerializeField] private string mode2ActionName = "CombatMode2";
        [SerializeField] private string mode3ActionName = "CombatMode3";

        private readonly List<ICombatAttackMode> attackModes = new List<ICombatAttackMode>();
        private int activeModeIndex;
        private float nextAllowedAttackTime;
        private InputAction attackAction;
        private InputAction mode1Action;
        private InputAction mode2Action;
        private InputAction mode3Action;

        public string ActiveModeName =>
            attackModes.Count > 0 ? attackModes[activeModeIndex].ModeName : "None";

        private void Awake()
        {
            CacheAttackModes();
            EnsureDefaultModes();
            playerInput ??= GetComponent<PlayerInput>();
            attackOrigin ??= transform;
            ResolveInputActions();
        }

        private void OnEnable()
        {
            ResolveInputActions();
        }

        private void Update()
        {
            if (attackAction == null)
            {
                ResolveInputActions();
            }

            HandleModeSwitchInput();
            HandleAttackInput();
        }

        private void CacheAttackModes()
        {
            attackModes.Clear();
            MonoBehaviour[] attachedComponents = GetComponents<MonoBehaviour>();
            for (int index = 0; index < attachedComponents.Length; index++)
            {
                if (attachedComponents[index] is ICombatAttackMode attackMode)
                {
                    attackModes.Add(attackMode);
                }
            }
        }

        private void EnsureDefaultModes()
        {
            // 합성 기반 동작을 위해 모드가 없으면 동일 오브젝트에 최소 모듈을 자동 부착합니다.
            if (GetComponent<RaycastAttackMode>() == null)
            {
                gameObject.AddComponent<RaycastAttackMode>();
            }

            if (GetComponent<ShapeCastAttackMode>() == null)
            {
                gameObject.AddComponent<ShapeCastAttackMode>();
            }

            if (GetComponent<OverlapAttackMode>() == null)
            {
                gameObject.AddComponent<OverlapAttackMode>();
            }

            CacheAttackModes();
            if (activeModeIndex >= attackModes.Count)
            {
                activeModeIndex = 0;
            }
        }

        private void HandleModeSwitchInput()
        {
            if (attackModes.Count <= 0)
            {
                return;
            }

            if (mode1Action != null && mode1Action.WasPressedThisFrame())
            {
                SetModeByIndex(0);
            }
            else if (mode2Action != null && mode2Action.WasPressedThisFrame())
            {
                SetModeByIndex(1);
            }
            else if (mode3Action != null && mode3Action.WasPressedThisFrame())
            {
                SetModeByIndex(2);
            }
        }

        private void HandleAttackInput()
        {
            bool wasAttackPressedThisFrame = attackAction != null && attackAction.WasPressedThisFrame();
            if (attackModes.Count <= 0 || !wasAttackPressedThisFrame)
            {
                return;
            }

            if (Time.time < nextAllowedAttackTime)
            {
                return;
            }

            nextAllowedAttackTime = Time.time + 1.0f / Mathf.Max(0.01f, attacksPerSecond);

            Transform originTransform = attackOrigin != null ? attackOrigin : transform;
            Vector3 attackCastOrigin = originTransform.position + Vector3.up * 1.3f;
            Vector3 attackDirection = transform.forward;
            attackDirection.y = 0.0f;
            if (attackDirection.sqrMagnitude <= 0.0001f)
            {
                attackDirection = originTransform.forward;
            }

            attackDirection.Normalize();

            AttackExecutionContext attackExecutionContext = new AttackExecutionContext(
                transform,
                attackCastOrigin,
                attackDirection,
                attackRange,
                baseDamage,
                attackTargetLayerMask);

            bool didHit = attackModes[activeModeIndex].TryAttack(in attackExecutionContext);
            Debug.Log($"[PlayerCombatController] Mode={ActiveModeName}, Hit={didHit}");
        }

        private void SetModeByIndex(int targetModeIndex)
        {
            if (targetModeIndex < 0 || targetModeIndex >= attackModes.Count)
            {
                return;
            }

            activeModeIndex = targetModeIndex;
            Debug.Log($"[PlayerCombatController] 공격 모드 전환: {ActiveModeName}");
        }

        private void OnValidate()
        {
            baseDamage = Mathf.Max(0.0f, baseDamage);
            attackRange = Mathf.Max(0.1f, attackRange);
            attacksPerSecond = Mathf.Max(0.01f, attacksPerSecond);
        }

        private void ResolveInputActions()
        {
            if (playerInput == null || playerInput.actions == null)
            {
                attackAction = null;
                mode1Action = null;
                mode2Action = null;
                mode3Action = null;
                return;
            }

            InputActionMap targetMap = playerInput.actions.FindActionMap(actionMapName, false);
            if (targetMap == null)
            {
                attackAction = null;
                mode1Action = null;
                mode2Action = null;
                mode3Action = null;
                return;
            }

            attackAction = targetMap.FindAction(attackActionName, false);
            mode1Action = targetMap.FindAction(mode1ActionName, false);
            mode2Action = targetMap.FindAction(mode2ActionName, false);
            mode3Action = targetMap.FindAction(mode3ActionName, false);
        }
    }
}
