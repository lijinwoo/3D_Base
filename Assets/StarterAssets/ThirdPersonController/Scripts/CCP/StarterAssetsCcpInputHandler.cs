using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// 한국어 주석: StarterAssets 입력값을 CCP InputHandler 인터페이스로 변환합니다.
    /// </summary>
    [AddComponentMenu("Starter Assets/CCP/Starter Assets CCP Input Handler")]
    public sealed class StarterAssetsCcpInputHandler : InputHandler
    {
        [Header("References")]
        [SerializeField]
        private StarterAssetsInputs starterAssetsInputs;

        [Header("Look Mapping")]
        [SerializeField]
        private bool invertPitch;

        [SerializeField]
        private float lookToPitchScale = 1.0f;

        [SerializeField]
        private float lookToRollScale = 1.0f;

        [Header("Unsupported Action Defaults")]
        [SerializeField]
        private bool defaultInteractValue;

        [SerializeField]
        private bool defaultJetPackValue;

        [SerializeField]
        private bool defaultDashValue;

        [SerializeField]
        private bool defaultCrouchValue;

        public StarterAssetsInputs StarterAssetsInputs
        {
            get => starterAssetsInputs;
            set => starterAssetsInputs = value;
        }

        private void Awake()
        {
            if (starterAssetsInputs == null)
            {
                starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            }
        }

        public override bool GetBool(string actionName)
        {
            if (starterAssetsInputs == null)
            {
                return false;
            }

            switch (actionName)
            {
                case "Jump":
                    return starterAssetsInputs.jump;
                case "Run":
                    return starterAssetsInputs.sprint;
                case "Interact":
                    return defaultInteractValue;
                case "Jet Pack":
                    return defaultJetPackValue;
                case "Dash":
                    return defaultDashValue;
                case "Crouch":
                    return defaultCrouchValue;
                default:
                    return false;
            }
        }

        public override float GetFloat(string actionName)
        {
            if (starterAssetsInputs == null)
            {
                return 0.0f;
            }

            float pitchSign = invertPitch ? -1.0f : 1.0f;

            switch (actionName)
            {
                case "Pitch":
                    return starterAssetsInputs.look.y * pitchSign * lookToPitchScale;
                case "Roll":
                    return starterAssetsInputs.look.x * lookToRollScale;
                default:
                    return 0.0f;
            }
        }

        public override Vector2 GetVector2(string actionName)
        {
            if (starterAssetsInputs == null)
            {
                return Vector2.zero;
            }

            if (actionName == "Movement")
            {
                return starterAssetsInputs.move;
            }

            return Vector2.zero;
        }
    }
}
