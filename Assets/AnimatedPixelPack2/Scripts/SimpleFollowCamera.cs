using UnityEngine;
using System.Collections;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class SimpleFollowCamera : MonoBehaviour
    {
        // Editor Properties
        [Tooltip("The target to follow around")]
        public Transform FollowTarget;
        [Tooltip("The offset to apply to the follow target")]
        public Vector3 Offset;

        [Header("Speed Settings")]
        public float SmoothFactor = 0.2f;
        public float LookAheadDistance = 1;
        public float LookAheadSpeed = 0.5f;
        public float LookAheadThreshold = 0.1f;

        [Header("Tracking Settings")]
        [Tooltip("The area in which the target can move without moving the camera")]
        public Vector2 DeadZone = new Vector2(2,1);
        public float DeactivateThreshold = 0.05f;

        // Members
        private Vector3 previousFollowTargetPosition;
        private Vector3 dampVelocity;
        private Vector3 lookAtPosition;
        private bool isTracking = false;

        void Start()
        {
            this.previousFollowTargetPosition = this.transform.position;

            // If we don't have a follow target, find the first available player
            if (this.FollowTarget == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag("Player");
                if (go != null)
                {
                    this.FollowTarget = go.transform;
                }
            }
        }

        void Update()
        {
            if (this.FollowTarget != null)
            {
                if (!this.isTracking)
                {
                    // Check if the target it outside the deadzone
                    var diff = this.transform.position - this.GetTargetPosition();
                    if (Mathf.Abs(diff.x) > this.DeadZone.x || Mathf.Abs(diff.y) > this.DeadZone.y)
                    {
                        this.isTracking = true;
                    }
                }

                if (this.isTracking)
                {
                    // Check if the player has changed direction
                    Vector2 movement = (this.GetTargetPosition() - this.previousFollowTargetPosition);
                    if (Mathf.Abs(movement.x) > this.LookAheadThreshold)
                    {
                        // If so we need to look ahead in the other direction
                        this.lookAtPosition = this.LookAheadDistance * Vector3.right * Mathf.Sign(movement.x);
                    }
                    else
                    {
                        // Otherwise just gently focus on the look ahead position
                        this.lookAtPosition = Vector3.MoveTowards(this.lookAtPosition, Vector3.zero, Time.deltaTime * this.LookAheadSpeed);
                    }

                    // Move the camera towards the target position
                    Vector3 target = this.GetTargetPosition() + this.lookAtPosition;
                    Vector3 newPos = Vector3.SmoothDamp(this.transform.position, target, ref this.dampVelocity, this.SmoothFactor);

                    // Remember to keep the camera on the starting z axis value
                    newPos.z = this.transform.position.z;

                    // Move the camera to the new position
                    this.transform.position = newPos;

                    // Store the new follow target position so we can check if it changed in the next update call
                    this.previousFollowTargetPosition = this.GetTargetPosition();

                    // Check if we should trun off tracking
                    if (Mathf.Abs(movement.x) < this.DeactivateThreshold &&
                        Mathf.Abs(movement.y) < this.DeactivateThreshold)
                    {
                        Vector2 trackingDifference = (this.transform.position - this.GetTargetPosition());

                        if (Mathf.Abs(trackingDifference.x) < this.DeactivateThreshold &&
                            Mathf.Abs(trackingDifference.y) < this.DeactivateThreshold)
                        {
                            this.isTracking = false;
                        }
                    }
                }
            }
        }

        private Vector3 GetTargetPosition()
        {
            return this.FollowTarget.position + this.Offset;
        }
    }
}
