using UnityEngine;
using UnityEngine.InputSystem;

namespace SystemicOverload.Phase1
{
    /// <summary>
    /// 플레이어 입력을 수집해 다른 컴포넌트가 소비하기 쉬운 형태로 제공합니다.
    /// </summary>
    public sealed class InputProvider : MonoBehaviour
    {
        [SerializeField] private bool normalizeDiagonalInput = true;
        [SerializeField] private bool enableDualMouseForwardMove = true;
        [SerializeField] private float dualMouseForwardAmount = 1.0f;

        public Vector2 MoveInput { get; private set; }
        public Vector2 PointerScreenPosition { get; private set; }
        public Vector2 LookDelta { get; private set; }
        public float ZoomDelta { get; private set; }
        public bool IsLeftMouseHeld { get; private set; }
        public bool IsRightMouseHeld { get; private set; }
        public bool IsDualMouseForwardHeld => IsLeftMouseHeld && IsRightMouseHeld;

        private void Update()
        {
            RefreshMouseState();
            MoveInput = ReadMoveInput();
            PointerScreenPosition = ReadPointerPosition();
            LookDelta = ReadLookDelta();
            ZoomDelta = ReadZoomDelta();
        }

        private void RefreshMouseState()
        {
            if (Mouse.current == null)
            {
                IsLeftMouseHeld = false;
                IsRightMouseHeld = false;
                return;
            }

            IsLeftMouseHeld = Mouse.current.leftButton.isPressed;
            IsRightMouseHeld = Mouse.current.rightButton.isPressed;
        }

        private Vector2 ReadMoveInput()
        {
            if (Keyboard.current == null)
            {
                return ApplyDualMouseForward(Vector2.zero);
            }

            float horizontal = 0.0f;
            float vertical = 0.0f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontal -= 1.0f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontal += 1.0f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                vertical -= 1.0f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                vertical += 1.0f;
            }

            Vector2 moveInput = new Vector2(horizontal, vertical);

            // 대각선 이동 시 축 합산으로 속도가 증가하지 않도록 정규화합니다.
            if (normalizeDiagonalInput && moveInput.sqrMagnitude > 1.0f)
            {
                moveInput.Normalize();
            }

            return ApplyDualMouseForward(moveInput);
        }

        private Vector2 ApplyDualMouseForward(Vector2 sourceMoveInput)
        {
            if (!enableDualMouseForwardMove || !IsDualMouseForwardHeld)
            {
                return sourceMoveInput;
            }

            Vector2 composedInput = sourceMoveInput + Vector2.up * dualMouseForwardAmount;
            if (normalizeDiagonalInput && composedInput.sqrMagnitude > 1.0f)
            {
                composedInput.Normalize();
            }

            return composedInput;
        }

        private Vector2 ReadPointerPosition()
        {
            if (Mouse.current == null)
            {
                return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }

            return Mouse.current.position.ReadValue();
        }

        private Vector2 ReadLookDelta()
        {
            if (Mouse.current == null)
            {
                return Vector2.zero;
            }

            return Mouse.current.delta.ReadValue();
        }

        private float ReadZoomDelta()
        {
            if (Mouse.current == null)
            {
                return 0.0f;
            }

            return Mouse.current.scroll.ReadValue().y * 0.01f;
        }
    }
}
