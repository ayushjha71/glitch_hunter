using UnityEngine;
using GlitchHunter.Constant;
using GlitchHunter.Manager;
using UnityEngine.Windows;

namespace GlitchHunter.Handler.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerMovementHandler : MonoBehaviour
    {
        [SerializeField]
        private float MoveSpeed = 2.0f;
        [SerializeField]
        private float SprintSpeed = 5.335f;
        [SerializeField]
        private float RotationSmoothTime = 0.12f;
        [SerializeField]
        private float SpeedChangeRate = 10.0f;
        [SerializeField]
        private AudioClip LandingAudioClip;
        [SerializeField]
        private AudioClip[] FootstepAudioClips;
        [Range(0, 1)]
        [SerializeField]
        private float FootstepAudioVolume = 0.5f;
        [Space(10)]
        [SerializeField]
        private float JumpHeight = 1.2f;
        [SerializeField]
        private float Gravity = -15.0f;
        [Space(10)]
        [SerializeField]
        private float JumpTimeout = 0.50f;
        [SerializeField]
        private float FallTimeout = 0.15f;
        [SerializeField]
        private bool Grounded = true;
        [SerializeField]
        private float GroundedOffset = -0.14f;
        [SerializeField]
        private float GroundedRadius = 0.28f;
        [SerializeField]
        private LayerMask GroundLayers;
        [SerializeField]
        private GameObject avatarPreview;

        // player
        private float mSpeed;
        private float mAnimationBlend;
        private float mTargetRotation = 0.0f;
        private float mRotationVelocity;
        private float mVerticalVelocity;
        private float mTerminalVelocity = 53.0f;

        // timeout deltatime
        private float mJumpTimeoutDelta;
        private float mFallTimeoutDelta;

        // animation IDs
        private int mAnimIDSpeed;
        private int mAnimIDGrounded;
        private int mAnimIDJump;
        private int mAnimIDFreeFall;
        private int mAnimIDMotionSpeed;

        private Animator mAnimator;
        private CharacterController mController;

        private GameObject mPlayerCamera;

        private const float THRESHOLD = 0.01f;

        private bool mHasAnimator;

        //Input
        private Vector2 mMoveVector;
        private Vector2 mKeyboardLookVector;
        private bool mAnalogMovement;
        private bool mIsFlying = false;
        private bool mCanJump;
        private bool mCanSprint;

        // Flying integration
        private PlayerFlyingHandler mFlyingHandler;

        private void Awake()
        {
            // get a reference to our main camera
            if (mPlayerCamera == null)
            {
                mPlayerCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            // Get flying handler reference
            mFlyingHandler = GetComponent<PlayerFlyingHandler>();
            mFlyingHandler.enabled = false;
        }

        private void Start()
        {
            mHasAnimator = TryGetComponent(out mAnimator);
            mController = GetComponent<CharacterController>();
            AssignAnimationIDs();

            // reset our timeouts on start
            mJumpTimeoutDelta = JumpTimeout;
            mFallTimeoutDelta = FallTimeout;

            GlitchHunterConstant.OnMoveInput += OnReceivedMoveInput;
            GlitchHunterConstant.OnLookInput += OnReceivedKeyboardLookInput;
            GlitchHunterConstant.OnSprintInput += OnReceivedSprintInput;
            GlitchHunterConstant.OnJumpInput += OnReceivedJumpInput;
            GlitchHunterConstant.OnActivateFlying += OnActiveFlying;
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGameStarted)
            {
                return;
            }

            mHasAnimator = TryGetComponent(out mAnimator);

            // Always check grounded state
            GroundedCheck();

            // Only handle movement if not flying
            if (mFlyingHandler == null || !mFlyingHandler.IsFlying())
            {
                JumpAndGravity();
                Move();
            }
            else
            {
                // While flying, maintain some vertical velocity control
                if (Grounded && mVerticalVelocity < 0)
                {
                    mVerticalVelocity = -2f;
                }
            }
        }

        private void OnDestroy()
        {
            GlitchHunterConstant.OnMoveInput -= OnReceivedMoveInput;
            GlitchHunterConstant.OnLookInput -= OnReceivedKeyboardLookInput;
            GlitchHunterConstant.OnSprintInput -= OnReceivedSprintInput;
            GlitchHunterConstant.OnJumpInput -= OnReceivedJumpInput;
            GlitchHunterConstant.OnActivateFlying -= OnActiveFlying;
        }

        private void AssignAnimationIDs()
        {
            mAnimIDSpeed = Animator.StringToHash("Speed");
            mAnimIDGrounded = Animator.StringToHash("Grounded");
            mAnimIDJump = Animator.StringToHash("Jump");
            mAnimIDFreeFall = Animator.StringToHash("FreeFall");
            mAnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (mHasAnimator)
            {
                mAnimator.SetBool(mAnimIDGrounded, Grounded);
            }
        }

        private void OnReceivedMoveInput(Vector2 input)
        {
            mMoveVector = input;
        }

        private void OnReceivedKeyboardLookInput(Vector2 input)
        {
            mKeyboardLookVector = input;
        }

        private void OnReceivedSprintInput(bool input)
        {
            mCanSprint = input;
        }

        private void CanStartControls(bool active)
        {
            GlitchHunterConstant.SetCursorState(true);
        }

        private void OnActiveFlying()
        {
            mFlyingHandler.enabled = true;
        }

        public void SetFlying(bool isFlying)
        {
            mIsFlying = isFlying;

            // Reset vertical velocity when starting/stopping flying
            if (isFlying)
            {
                mVerticalVelocity = 0f;
            }
        }

        private void OnReceivedJumpInput(bool input)
        {
            // Only handle jumping for ground-based movement
            // Let flying handler handle its own jump input
            if (mFlyingHandler != null && (mFlyingHandler.IsFlying() || mFlyingHandler.WantsToFly()))
            {
                return;
            }

            // Only allow jumping when grounded and not flying
            if (!Grounded)
            {
                return;
            }

            // For ground jumping, we want the button press, not hold
            if (input && !mCanJump)
            {
                mCanJump = true;
            }
            else if (!input)
            {
                mCanJump = false;
            }
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = mCanSprint ? SprintSpeed : MoveSpeed;
            if (mMoveVector == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }
            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(mController.velocity.x, 0.0f, mController.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = mAnalogMovement ? mMoveVector.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                mSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                mSpeed = Mathf.Round(mSpeed * 1000f) / 1000f;
            }
            else
            {
                mSpeed = targetSpeed;
            }

            if (targetSpeed != 0)
            {
                RotatePlayer();
            }
            else
            {
                if (mKeyboardLookVector != Vector2.zero)
                {
                    RotatePlayer();
                }
            }
            mAnimationBlend = Mathf.Lerp(mAnimationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (mAnimationBlend < 0.01f)
            {
                mAnimationBlend = 0f;
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, mTargetRotation, 0.0f) * Vector3.forward;

            // move the player
            mController.Move(targetDirection.normalized * (mSpeed * Time.deltaTime) + new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (mHasAnimator)
            {
                mAnimator.SetFloat(mAnimIDSpeed, mAnimationBlend);
                mAnimator.SetFloat(mAnimIDMotionSpeed, inputMagnitude);
            }
        }

        private void RotatePlayer()
        {
            Vector3 dir = new Vector3(mMoveVector.x, 0.0f, mMoveVector.y).normalized;
            mTargetRotation = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + mPlayerCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, mTargetRotation, ref mRotationVelocity, RotationSmoothTime);
            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        private void JumpAndGravity()
        {
            if (mIsFlying) return;

            if (Grounded)
            {
                // reset the fall timeout timer
                mFallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (mHasAnimator)
                {
                    mAnimator.SetBool(mAnimIDJump, false);
                    mAnimator.SetBool(mAnimIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (mVerticalVelocity < 0.0f)
                {
                    mVerticalVelocity = -2f;
                }

                // Jump - trigger on button press
                if (mCanJump && mJumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    mVerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (mHasAnimator)
                    {
                        mAnimator.SetBool(mAnimIDJump, true);
                    }

                    // Reset jump flag after jumping
                    mCanJump = false;
                }

                // jump timeout
                if (mJumpTimeoutDelta >= 0.0f)
                {
                    mJumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                mJumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (mFallTimeoutDelta >= 0.0f)
                {
                    mFallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (mHasAnimator)
                    {
                        mAnimator.SetBool(mAnimIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                mCanJump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (mVerticalVelocity < mTerminalVelocity)
            {
                mVerticalVelocity += Gravity * Time.deltaTime;
            }
        }

        public void SetAvatarLayer(int newLayer)
        {
            SetLayerRecursively(avatarPreview, newLayer);
            Debug.LogError("Set Avatar Layer");
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(mController.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(mController.center), FootstepAudioVolume);
            }
        }

        // Public methods for flying integration
        public bool IsGrounded()
        {
            return Grounded;
        }

        public float GetVerticalVelocity()
        {
            return mVerticalVelocity;
        }

        public void SetVerticalVelocity(float velocity)
        {
            mVerticalVelocity = velocity;
        }
    }
}