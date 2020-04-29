using UnityEngine;
using System.Collections;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public abstract class WeaponFX : MonoBehaviour
    {
        // Members
        private static Color? mainBackgroundColor;
        private static int backgroundColorChangers;
        protected Transform Target { get; private set; }
        protected bool IsFXStarted { get; private set; }
        protected int DirectionX { get; private set; }

        /// <summary>
        /// Instantiate a new instance of the WeaponEffect class using the supplied parameters
        /// </summary>
        /// <param name="instance">The instance to use as the base</param>
        /// <param name="target">The position that is being targeted</param>
        /// <returns>The new WeaponEffect</returns>
        public static WeaponFX Create(WeaponFX instance, Transform target, int directionX)
        {
            WeaponFX effect = GameObject.Instantiate<WeaponFX>(instance);
            effect.Target = target;
            effect.DirectionX = directionX;

            return effect;
        }

        /// <summary>
        /// Set the background color of the main camera.
        /// The color will be reset once all the effects that have changed it have stopped.
        /// </summary>
        /// <param name="newColor">The new background color to use</param>
        public static void SetBackgroundColor(Color newColor)
        {
            // Something is changing the environment color
            WeaponFX.backgroundColorChangers++;

            // Set the new color
            Camera.main.backgroundColor = newColor;
        }

        private static void ResetBackgroundColor()
        {
            // Something stopped changing the color
            WeaponFX.backgroundColorChangers--;

            if (WeaponFX.backgroundColorChangers <= 0 && WeaponFX.mainBackgroundColor.HasValue)
            {
                // Reset the color now that nothing is changing it
                Camera.main.backgroundColor = WeaponFX.mainBackgroundColor.GetValueOrDefault();
            }
        }

        void Awake()
        {
            // Store the starting background color in case we change it later
            if (!WeaponFX.mainBackgroundColor.HasValue)
            {
                WeaponFX.mainBackgroundColor = Camera.main.backgroundColor;
            }
        }

        protected virtual void Start()
        {
            if (this.Target != null)
            {
                this.transform.position = this.Target.position;
            }
        }

        protected virtual void Update()
        {
        }

        /// <summary>
        /// Start the effect
        /// </summary>
        public virtual void BeginFX()
        {
            this.IsFXStarted = true;
        }

        /// <summary>
        /// Stop the effect
        /// </summary>
        public virtual void EndFX()
        {
            // Get rid of the object
            GameObject.Destroy(this.gameObject);

            // Remove any color effect we performed
            WeaponFX.ResetBackgroundColor();
        }
    }
}
