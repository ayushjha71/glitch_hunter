using GlitchHunter.Constant;
using GlitchHunter.Enum;
using GlitchHunter.Handler.Player;
using GlitchHunter.Manager;
using UnityEngine;


namespace GlitchHunter.Handler.Camera
{
    public class CameraMovementHandler : MonoBehaviour
    {
        #region [SerializeField]
        [SerializeField]
        private Transform cameraFollowTarget;
        [SerializeField]
        private Vector3 camOffset = new Vector3(0, 0, -4);
        [SerializeField]
        private float smooth = 10f;
        [SerializeField]
        private float maxVerticalAngle = 30f;
        [SerializeField]
        private float minVerticalAngle = -60f;
        [SerializeField]
        private Vector3 minCameraOffset = new Vector3(0.0f, 0.0f, -3.0f);
        [SerializeField]
        private PlayerMovementHandler playerMovementHandler;
        [SerializeField]
        private LayerMask layerMask;
        #endregion[SerializeField]



        #region Properties
        #region Private Properties
        private float mAngleHorizontal = 0;
        private float mAngleVertical = 0;
        private Transform mCamera;
        private Vector3 pivotOffset = new Vector3(0, 0, 0);
        private Vector3 mSmoothPivotOffset;
        private Vector3 mSmoothCamOffset;
        private Vector3 mTargetPivotOffset;
        private Vector3 mTargetCamOffset;
        private float mDefaultFOV;
        private float mTargetFOV;
        private float mTargetMaxVerticalAngle;
        private bool mIsCustomOffset;

        private const float HORIZONTAL_SPEED = 3f;
        private const float VERTICAL_SPEED = 3f;
        //Input
        private Vector2 mLookVector;
        #endregion Private Properities

        #region Public Properties
        public float AngleHorizontal
        {
            get => mAngleHorizontal;
        }
        public Vector3 TargetCameraPositionOffset
        {
            get => mTargetCamOffset;
            set => mTargetCamOffset = value;
        }
        public float GetHorizontalAngle
        {
            get
            {
                return mAngleHorizontal;
            }
        }
        #endregion Public Properities
        #endregion Properties



        #region Unity's LifeCycle Callbacks
        void Awake()
        {
            mCamera = transform;
            mCamera.position = cameraFollowTarget.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
            mCamera.rotation = Quaternion.identity;
            mSmoothPivotOffset = pivotOffset;
            mSmoothCamOffset = camOffset;
            mDefaultFOV = mCamera.GetComponent<UnityEngine.Camera>().fieldOfView;
            ResetTargetOffsets();
            ResetFOV();
            ResetMaxVerticalAngle();
        }

        private void OnEnable()
        {
            GlitchHunterConstant.OnLookInput += OnReceivedInput;
            GlitchHunterConstant.OnSwitchController += SwitchControllerType;
        }

        private void LateUpdate()
        {
            if (!GameManager.Instance.IsGameStarted)
            {
                return;
            }

            if (GlitchHunterConstant.CurrentControllerType == ControllerType.FIRST_PERSON)
            {
                FirstPersonMoveCamera();
            }
            else
            {
                ThirdPersonMoveCamera();
            }

        }

        private void OnDestroy()
        {
            GlitchHunterConstant.OnLookInput -= OnReceivedInput;
            GlitchHunterConstant.OnSwitchController -= SwitchControllerType;
        }

        #endregion Unity's LifeCycle Callbacks

        #region Methods
        #region Private Methods
        private void OnReceivedInput(Vector2 lookInput)
        {
            mLookVector = lookInput;
        }

        private void FirstPersonMoveCamera()
        {
            mAngleHorizontal += Mathf.Clamp(mLookVector.x, -1, 1) * HORIZONTAL_SPEED;
            mAngleVertical += Mathf.Clamp(mLookVector.y, -1, 1) * -VERTICAL_SPEED;

            // Clamp the vertical angle to avoid flipping
            mAngleVertical = Mathf.Clamp(mAngleVertical, -45, 45);

            // Apply the rotation to the camera
            transform.localRotation = Quaternion.Euler(mAngleVertical, mAngleHorizontal, 0);
            this.transform.position = cameraFollowTarget.position;
        }

        private void ThirdPersonMoveCamera()
        {
            mAngleHorizontal += Mathf.Clamp(mLookVector.x, -1, 1) * HORIZONTAL_SPEED;
            mAngleVertical += Mathf.Clamp(mLookVector.y, -1, 1) * -VERTICAL_SPEED;

            mAngleVertical = Mathf.Clamp(mAngleVertical, minVerticalAngle, mTargetMaxVerticalAngle);

            Quaternion camYRotation = Quaternion.Euler(0, mAngleHorizontal, 0);
            Quaternion aimRotation = Quaternion.Euler(-mAngleVertical, mAngleHorizontal, 0);
            mCamera.rotation = aimRotation;

            mCamera.GetComponent<UnityEngine.Camera>().fieldOfView = Mathf.Lerp(mCamera.GetComponent<UnityEngine.Camera>().fieldOfView, mTargetFOV, Time.deltaTime);

            Vector3 baseTempPosition = cameraFollowTarget.position + camYRotation * mTargetPivotOffset;
            Vector3 noCollisionOffset = mTargetCamOffset;
            while (noCollisionOffset.magnitude >= 0.5f)
            {
                if (DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset))
                    break;
                noCollisionOffset -= noCollisionOffset.normalized * 0.5f;
            }
            if (noCollisionOffset.magnitude < 0.5f)
            {
                noCollisionOffset = minCameraOffset;
            }

            bool customOffsetCollision = mIsCustomOffset && noCollisionOffset.sqrMagnitude < mTargetCamOffset.sqrMagnitude;

            mSmoothPivotOffset = Vector3.Lerp(mSmoothPivotOffset, customOffsetCollision ? pivotOffset : mTargetPivotOffset, smooth * Time.deltaTime);
            mSmoothCamOffset = Vector3.Lerp(mSmoothCamOffset, customOffsetCollision ? minCameraOffset : noCollisionOffset, smooth * Time.deltaTime);

            mCamera.position = cameraFollowTarget.position + camYRotation * mSmoothPivotOffset + aimRotation * mSmoothCamOffset;
        }

        private void ResetTargetOffsets()
        {
            mTargetPivotOffset = pivotOffset;
            mTargetCamOffset = camOffset;
            mIsCustomOffset = false;
        }

        private void ResetFOV()
        {
            this.mTargetFOV = mDefaultFOV;
        }

        private void ResetMaxVerticalAngle()
        {
            this.mTargetMaxVerticalAngle = maxVerticalAngle;
        }

        private bool DoubleViewingPosCheck(Vector3 checkPos)
        {
            return ViewingPosCheck(checkPos) && ReverseViewingPosCheck(checkPos);
        }

        private bool ViewingPosCheck(Vector3 checkPos)
        {
            Vector3 target = cameraFollowTarget.position + pivotOffset;
            Vector3 direction = target - checkPos;
            if (Physics.SphereCast(checkPos, 0.2f, direction, out RaycastHit hit, direction.magnitude, layerMask))
            {
                Collider collider = hit.transform.GetComponent<Collider>();
                if (collider == null)
                {
                    return true;
                }
                if (hit.transform != cameraFollowTarget && !collider.isTrigger)
                {
                    return false;
                }
            }
            return true;
        }

        private bool ReverseViewingPosCheck(Vector3 checkPos)
        {
            Vector3 origin = cameraFollowTarget.position + pivotOffset;
            Vector3 direction = checkPos - origin;
            if (Physics.SphereCast(origin, 0.2f, direction, out RaycastHit hit, direction.magnitude, layerMask))
            {
                Collider collider = hit.transform.GetComponent<Collider>();
                if (collider == null)
                {
                    return true;
                }

                if (hit.transform != cameraFollowTarget && hit.transform != transform && !collider.isTrigger)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion Private Methods

        #region Public Methods
        public void SwitchControllerType()
        {
            if (GlitchHunterConstant.CurrentControllerType == ControllerType.THIRD_PERSON)
            {
                pivotOffset = new Vector3(0, 0, .8f);
                mCamera.GetComponent<UnityEngine.Camera>().nearClipPlane = 0.1f;
                GlitchHunterConstant.CurrentControllerType = ControllerType.THIRD_PERSON;
                playerMovementHandler.SetAvatarLayer(LayerMask.NameToLayer("Player"));
            }

            else if (GlitchHunterConstant.CurrentControllerType == ControllerType.FIRST_PERSON)
            {
                pivotOffset = new Vector3(0, 0, 0);
                mCamera.GetComponent<UnityEngine.Camera>().nearClipPlane = 0.3f;
                GlitchHunterConstant.CurrentControllerType = ControllerType.FIRST_PERSON;
                playerMovementHandler.SetAvatarLayer(LayerMask.NameToLayer("LocalPlayer"));
            }
        }
        #endregion Public Methods
        #endregion Methods
    }
}
