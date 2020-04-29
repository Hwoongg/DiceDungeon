using UnityEngine;
using System.Collections;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class CastFX : WeaponFX
    {
        // EditorProperties
        [Header("Cast")]
        [Tooltip("Should we start the animation when the cast begins")]
        public bool PlayOnStart = false;
        [Tooltip("How long to leave the FX running after the cast is complete")]
        public float KillDelay = 0;
        [Tooltip("Should the fx be mirrored when travelling the opposite direction")]
        public bool ShouldFlipDirection = true;

        // Members
        private Animator animator;

        protected override void Start()
        {
            base.Start();

            // Flip the sprite if necessary
            if (this.ShouldFlipDirection && this.DirectionX < 0)
            {
                Vector3 rotation = this.transform.localRotation.eulerAngles;
                rotation.y -= 180;
                this.transform.localEulerAngles = rotation;
            }

            // Start the animation if requested
            if (this.PlayOnStart)
            {
                this.animator = this.GetComponent<Animator>();
                this.animator.Play("Cast");
            }
        }

        public override void BeginFX()
        {
            base.BeginFX();

            // Start the animation if we haven't already
            if (!this.PlayOnStart)
            {
                this.animator = this.GetComponent<Animator>();
                this.animator.Play("Cast");
            }
        }

        public override void EndFX()
        {
            // Kill the effect after its lifetime
            StartCoroutine(this.StopAfter(this.KillDelay));
        }

        private IEnumerator StopAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            base.EndFX();
            this.animator.Play("Stopped");
        }
    }
}