using UnityEngine;
using System.Collections;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class HorizontalProjectile : WeaponProjectile
    {
        // Editor Properties
        [Header("Projectile")]
        [Tooltip("The horizontal speed to move the projectile")]
        public int Speed = 500;
        [Tooltip("The speed to rotate the projectile")]
        public int RotationSpeed = 0;
        [Tooltip("Restrict rotation to 90 degree intervals")]
        public bool RestrictRotation90 = true;
        [Tooltip("Transform to rotate")]
        public Transform RotateTransform;

        // Members
        protected Vector3 realRotation;

        protected override void Start()
        {
            base.Start();

            // Get the slider joint that prevents any Y movement
            SliderJoint2D joint = this.GetComponent<SliderJoint2D>();
            if (joint != null)
            {
                joint.anchor = new Vector2(this.transform.position.x, -this.transform.position.y);
            }

            // Give it some velocity
            float x = DirectionX * this.Speed;
            Rigidbody2D body = this.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.AddForce(new Vector2(x, 0));
            }

            this.realRotation = this.RotateTransform.eulerAngles;
        }

        protected override void Update()
        {
            base.Update();

            // If we are a rotating projectile, then rotate the rendering part
            if (this.RotationSpeed != 0 && this.RotateTransform != null && !this.isStopped)
            {
                this.RotateTransform.eulerAngles = this.realRotation;
                this.RotateTransform.Rotate(Vector3.forward, Time.deltaTime * -this.RotationSpeed * this.DirectionX);
                this.realRotation = this.RotateTransform.eulerAngles;

                if (this.RestrictRotation90)
                {
                    var vec = this.RotateTransform.eulerAngles;
                    vec.x = Mathf.Round(vec.x / 90) * 90;
                    vec.y = Mathf.Round(vec.y / 90) * 90;
                    vec.z = Mathf.Round(vec.z / 90) * 90;
                    this.RotateTransform.eulerAngles = vec;
                }
            }
        }
    }
}