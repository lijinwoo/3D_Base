using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// 한국어 주석: CCP 상태 값을 StarterAssets Animator 파라미터로 브리지합니다.
    /// </summary>
    [AddComponentMenu("Starter Assets/CCP/Starter Assets CCP Animator Bridge")]
    public sealed class StarterAssetsCcpAnimatorBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private CharacterActor characterActor;

        [SerializeField]
        private CharacterBrain characterBrain;

        [SerializeField]
        private Animator animator;

        [Header("Blend")]
        [SerializeField]
        private float speedBlendRate = 10.0f;

        [Header("Parameters")]
        [SerializeField]
        private string speedParameter = "Speed";

        [SerializeField]
        private string groundedParameter = "Grounded";

        [SerializeField]
        private string jumpParameter = "Jump";

        [SerializeField]
        private string freeFallParameter = "FreeFall";

        [SerializeField]
        private string motionSpeedParameter = "MotionSpeed";

        private int speedId;
        private int groundedId;
        private int jumpId;
        private int freeFallId;
        private int motionSpeedId;

        private float animationSpeed;

        private void Awake()
        {
            if (characterActor == null)
            {
                characterActor = GetComponentInChildren<CharacterActor>();
            }

            if (characterBrain == null)
            {
                characterBrain = GetComponentInChildren<CharacterBrain>();
            }

            if (animator == null && characterActor != null)
            {
                animator = characterActor.Animator;
            }

            speedId = Animator.StringToHash(speedParameter);
            groundedId = Animator.StringToHash(groundedParameter);
            jumpId = Animator.StringToHash(jumpParameter);
            freeFallId = Animator.StringToHash(freeFallParameter);
            motionSpeedId = Animator.StringToHash(motionSpeedParameter);
        }

        private void Update()
        {
            if (animator == null || characterActor == null || characterBrain == null)
            {
                return;
            }

            bool isGrounded = characterActor.IsGrounded;
            bool isAscending = characterActor.LocalVelocity.y > 0.05f;
            bool isFreeFall = !isGrounded && characterActor.IsFalling;

            float planarSpeed = characterActor.PlanarVelocity.magnitude;
            float inputMagnitude = Mathf.Clamp01(characterBrain.CharacterActions.movement.value.magnitude);

            animationSpeed = Mathf.Lerp(animationSpeed, planarSpeed, Time.deltaTime * speedBlendRate);

            animator.SetBool(groundedId, isGrounded);
            animator.SetBool(jumpId, !isGrounded && isAscending);
            animator.SetBool(freeFallId, isFreeFall);
            animator.SetFloat(speedId, animationSpeed);
            animator.SetFloat(motionSpeedId, inputMagnitude);
        }

        public void Configure(CharacterActor configuredActor, CharacterBrain configuredBrain, Animator configuredAnimator)
        {
            characterActor = configuredActor;
            characterBrain = configuredBrain;
            animator = configuredAnimator;
        }
    }
}
