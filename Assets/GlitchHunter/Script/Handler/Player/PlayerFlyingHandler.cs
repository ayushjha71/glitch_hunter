using GlitchHunter.Constant;
using UnityEngine;

namespace GlitchHunter.Handler.Player
{
    [RequireComponent(typeof(PlayerMovementHandler))]
    public class PlayerFlyingHandler : MonoBehaviour
    {
        [Header("Flying Settings")]
        [SerializeField] private float FlySpeed = 8.0f;
        [SerializeField] private float FlySprintSpeed = 15.0f;
        [SerializeField] private float FlyAcceleration = 10.0f;
        [SerializeField] private float FlyDeceleration = 15.0f;
        [SerializeField] private float VerticalFlySpeed = 6.0f;
        [SerializeField] private float FlyRotationSmoothTime = 0.08f;

        [Header("Transition Settings")]
        [SerializeField] private float MinFlyHeight = 1.0f;
        [SerializeField] private float FlyActivationDelay = 0.5f;
        [SerializeField] private float AscentAcceleration = 5.0f;
        [SerializeField] private float DescentAcceleration = 8.0f;
        [SerializeField] private float MaxAscentSpeed = 12.0f;
        [SerializeField] private float MaxDescentSpeed = 8.0f;

        private CharacterController mController;
        private UnityEngine.Camera mPlayerCamera;
        private PlayerMovementHandler mMovementHandler;

        private bool mIsFlying = false;
        private bool mWantsToFly = false;
        private float mFlyActivationTimer = 0f;
        private Vector3 mFlyVelocity = Vector3.zero;
        private float mFlyRotationVelocity;
        private float mTargetFlyRotation = 0f;
        private float mCurrentVerticalSpeed = 0f;
        private float mCurrentHorizontalSpeed = 0f;

        // Input variables
        private Vector2 mMoveInput;
        private bool mSprintInput;
        private bool mJumpInput;
        private bool mJumpHeld = false;

        private void Awake()
        {
            mController = GetComponent<CharacterController>();
            mMovementHandler = GetComponent<PlayerMovementHandler>();
            mPlayerCamera = UnityEngine.Camera.main;
        }

        private void Start()
        {
            GlitchHunterConstant.OnMoveInput += OnReceivedMoveInput;
            GlitchHunterConstant.OnSprintInput += OnReceivedSprintInput;
            GlitchHunterConstant.OnJumpInput += OnReceivedJumpInput;
        }

        private void Update()
        {
            HandleFlyingInput();

            // Only handle flying if we're actually flying
            if (mIsFlying)
            {
                HandleFlying();
            }
            else if (mWantsToFly)
            {
                HandleFlyActivation();
            }

            // If we were flying but released space and aren't grounded, ensure gravity is applied
            if (!mJumpHeld && !mIsFlying && !mMovementHandler.IsGrounded())
            {
                mMovementHandler.SetFlying(false);
            }
        }

        private void HandleFlyingInput()
        {
            bool wantsToStartFlying = mJumpInput && !mJumpHeld;
            bool wantsToContinueFlying = mJumpInput && mJumpHeld;

            if (wantsToStartFlying)
            {
                mJumpHeld = true;
                if (!mIsFlying)
                {
                    mWantsToFly = true;
                    mFlyActivationTimer = 0f;
                }
            }
            else if (!mJumpInput && mJumpHeld)
            {
                mJumpHeld = false;
                if (mIsFlying)
                {
                    StopFlying();
                }
                mWantsToFly = false;
            }

            // Allow flying to continue while holding space, but don't block other inputs
            if (mIsFlying && wantsToContinueFlying)
            {
                // Keep flying
            }
        }

        private void HandleFlyActivation()
        {
            mFlyActivationTimer += Time.deltaTime;
            if (mFlyActivationTimer >= FlyActivationDelay)
            {
                StartFlying();
            }
        }

        private void StartFlying()
        {
            mIsFlying = true;
            mWantsToFly = false;
            mFlyActivationTimer = 0f;

            // Initialize with current horizontal velocity
            Vector3 currentVelocity = mController.velocity;
            mFlyVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            mCurrentHorizontalSpeed = mFlyVelocity.magnitude;
            mCurrentVerticalSpeed = 0f;
        }

        public void EndFlying()
        {
            mIsFlying = false;
            mWantsToFly = false;
            mJumpHeld = false;
            mMovementHandler.SetFlying(false); // Tell movement handler to resume normal gravity
        }

        private void StopFlying()
        {
            EndFlying();

            // Apply a small downward force to start falling
            if (!mMovementHandler.IsGrounded())
            {
                mMovementHandler.SetVerticalVelocity(-2f); // Small downward push
            }
        }

        private void HandleFlying()
        {
            if (!mJumpHeld && mIsFlying)
            {
                // If we're flying but not holding jump, start falling
                StopFlying();
                return;
            }

            // Calculate target horizontal speed
            float targetHorizontalSpeed = mSprintInput ? FlySprintSpeed : FlySpeed;
            if (mMoveInput == Vector2.zero) targetHorizontalSpeed = 0f;

            // Smooth horizontal speed transition
            float acceleration = targetHorizontalSpeed > mCurrentHorizontalSpeed ?
                FlyAcceleration : FlyDeceleration;
            mCurrentHorizontalSpeed = Mathf.Lerp(
                mCurrentHorizontalSpeed,
                targetHorizontalSpeed,
                Time.deltaTime * acceleration
            );

            // Calculate movement direction
            Vector3 moveDirection = Vector3.zero;
            if (mMoveInput != Vector2.zero)
            {
                Vector3 forward = mPlayerCamera.transform.forward;
                Vector3 right = mPlayerCamera.transform.right;
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                moveDirection = (forward * mMoveInput.y + right * mMoveInput.x).normalized;

                // Handle rotation
                if (moveDirection != Vector3.zero)
                {
                    mTargetFlyRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                    float rotation = Mathf.SmoothDampAngle(
                        transform.eulerAngles.y,
                        mTargetFlyRotation,
                        ref mFlyRotationVelocity,
                        FlyRotationSmoothTime
                    );
                    transform.rotation = Quaternion.Euler(0f, rotation, 0f);
                }
            }

            // Handle vertical movement
            float targetVerticalSpeed = 0f;

            if (mJumpHeld) // Ascend while jump is held
            {
                targetVerticalSpeed = MaxAscentSpeed;
            }
            else // Descend when not holding jump
            {
                targetVerticalSpeed = -MaxDescentSpeed;
            }

            // Apply vertical acceleration
            float verticalAcceleration = mJumpHeld ? AscentAcceleration : DescentAcceleration;
            mCurrentVerticalSpeed = Mathf.Lerp(
                mCurrentVerticalSpeed,
                targetVerticalSpeed,
                Time.deltaTime * verticalAcceleration
            );

            // Apply movement
            Vector3 horizontalMovement = moveDirection * mCurrentHorizontalSpeed;
            Vector3 verticalMovement = Vector3.up * mCurrentVerticalSpeed;
            mFlyVelocity = horizontalMovement + verticalMovement;

            // Apply movement to controller
            mController.Move(mFlyVelocity * Time.deltaTime);

            // Prevent going below min height
            if (transform.position.y < MinFlyHeight)
            {
                Vector3 pos = transform.position;
                pos.y = MinFlyHeight;
                transform.position = pos;
                mCurrentVerticalSpeed = Mathf.Max(0f, mCurrentVerticalSpeed);
            }
        }

        public bool IsFlying() => mIsFlying;
        public bool WantsToFly() => mWantsToFly;

        public void ForceStopFlying()
        {
            StopFlying();
            mWantsToFly = false;
            mJumpHeld = false;
        }

        public bool ShouldHandleMovement() => !mIsFlying;

        #region Input Handlers
        private void OnReceivedMoveInput(Vector2 input) => mMoveInput = input;
        private void OnReceivedSprintInput(bool input) => mSprintInput = input;
        private void OnReceivedJumpInput(bool input) => mJumpInput = input;
        #endregion

        private void OnDestroy()
        {
            GlitchHunterConstant.OnMoveInput -= OnReceivedMoveInput;
            GlitchHunterConstant.OnSprintInput -= OnReceivedSprintInput;
            GlitchHunterConstant.OnJumpInput -= OnReceivedJumpInput;
        }
    }
}