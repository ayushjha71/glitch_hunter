using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mayaaverse.Core.Handler
{
    public class VRUIFollower : MonoBehaviour
    {
        enum FollowType { DontFollow, StrictFollow, LazyFollow }

        [SerializeField] private FollowType followType = FollowType.StrictFollow;

        //[Space] [Tooltip("Axis that will be tracked for the object")]
        //[SerializeField] private FollowAxis followAxis;

        [Tooltip("Override the follow distance by the starting distance.")]
        [SerializeField] private bool autoFollowDistance = true;

        [Tooltip("Set a default forward offset from the head")]
        public float followDistance;

        [Tooltip("Additional offset given to the object, while being tracked.")]
        public Vector3 offset;
        public bool faceCamera;
        public bool followHeadRotation;

        [Header("Lazy Follow")]
        public float followSpeed = 4f;

        [SerializeField]
        private Camera vRCamera;

        Transform HeadObject
        {
            get
            {
                if(vRCamera == null)
                {
                    vRCamera = Camera.main;
                }
                if(vRCamera == null)
                {
                    return gameObject.transform;
                }
                return vRCamera.gameObject.transform;
            }
        }

        private void Start()
        {
            if (autoFollowDistance) offset = transform.position;
        }

        void LateUpdate()
        {
            if (followType == FollowType.StrictFollow)
            {
                transform.position = HeadObject.forward * (followDistance + offset.z) + HeadObject.right * offset.x +
                                     HeadObject.up * offset.y;
            }
            else if (followType == FollowType.LazyFollow)
            {
                transform.position = Vector3.Lerp(transform.position,
                    HeadObject.forward * (followDistance + offset.z) + HeadObject.right * offset.x + HeadObject.up * offset.y,
                    followSpeed / 360);
            }
            else { /*dont follow */ }

            if (faceCamera)
            {
                transform.LookAt(HeadObject);
                transform.Rotate(0, 180, 0);
            }
            if (followHeadRotation) transform.Rotate(0, 0, HeadObject.eulerAngles.z);
        }
    }
}
