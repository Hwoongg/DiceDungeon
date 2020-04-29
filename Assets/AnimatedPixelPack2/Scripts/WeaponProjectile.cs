using UnityEngine;
using System.Collections;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public abstract class WeaponProjectile : MonoBehaviour
    {
        // Editor Properties
        [Header("Weapon")]
        [Tooltip("The amount of damage to apply to the Character it hits")]
        public int Damage = 50;
        [Tooltip("How many seconds should the projectile live for (it is destroyed after this time)")]
        public float LifeTime = 3;
        [Tooltip("Should the projectile collide with the ground layer (if not it will fly straight through)")]
        public bool ShouldCollideWithGround;
        [Tooltip("Should the projectile fall to the ground when it hits a wall")]
        public bool ShouldFallWhenCollided = false;
        [Tooltip("Should the projectile destroy itself when it hits a wall")]
        public bool ShouldGetDestroyedWhenCollided = false;
        [Tooltip("Layer that contains all the 'ground' colliders (if left as nothing it will be set from the character)")]
        public LayerMask OverrideGroundLayer;
        [Tooltip("A ParticleSystem to play when a collision occurs")]
        public ParticleSystem CollideEmitter;
        [Tooltip("Should the projectile be mirrored when travelling the opposite direction")]
        public bool ShouldFlipDirection = true;

        // Members
        public Character Owner { get; private set; }
        protected int DirectionX { get; private set; }
        protected Animator animatorObject;
        protected bool isStopped;
        protected Transform launchPoint;

        /// <summary>
        /// Instantiate a new instance of the WeaponProjectile class using the supplied parameters
        /// </summary>
        /// <param name="instance">The instance to use as the base</param>
        /// <param name="owner">The character that owns this projectile</param>
        /// <param name="launchPoint">Where to spawn the projectile</param>
        /// <param name="directionX">The direction to move</param>
        /// <returns>The new projectile</returns>
        public static WeaponProjectile Create(WeaponProjectile instance, Character owner, Transform launchPoint, int directionX)
        {
            WeaponProjectile projectile = GameObject.Instantiate<WeaponProjectile>(instance);
            projectile.Owner = owner;

            // Set the start position
            projectile.launchPoint = launchPoint;
            projectile.transform.position = projectile.launchPoint.position;
            projectile.DirectionX = directionX;

            // Make sure we can't collide with the person who shot this projectile
            WeaponProjectile.IgnoreOwnerCollisions(projectile, owner);

            // Flip the sprite if necessary
            if (projectile.ShouldFlipDirection && directionX < 0)
            {
                Vector3 rotation = projectile.transform.localRotation.eulerAngles;
                rotation.y -= 180;
                projectile.transform.localEulerAngles = rotation;
            }

            return projectile;
        }

        protected static void IgnoreOwnerCollisions(WeaponProjectile projectile, Character owner)
        {
            // Prevent hitting the player who cast it
            if (owner != null)
            {
                Collider2D[] colliders = owner.GetComponentsInChildren<Collider2D>();
                Collider2D[] projectileColliders = projectile.GetComponentsInChildren<Collider2D>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    for (int j = 0; j < projectileColliders.Length; j++)
                    {
                        Physics2D.IgnoreCollision(colliders[i], projectileColliders[j]);
                    }
                }
            }
        }

        protected virtual void Start()
        {
            this.animatorObject = this.GetComponentInChildren<Animator>();

            // Get rid of the projectile after a while if it doesn't hit anything
            StartCoroutine(this.DestroyAfter(this.LifeTime));
        }

        protected virtual void Update()
        {
        }

        protected virtual void OnCollisionEnter2D(Collision2D c)
        {
            CheckRealCollision(c, false);
        }

        protected bool CheckRealCollision(Collision2D c, bool preCheck)
        {
            bool destroy = false;

            if (!this.isStopped)
            {
                LayerMask ground = (this.OverrideGroundLayer == 0 && this.Owner != null ? this.Owner.GroundLayer : this.OverrideGroundLayer);
                bool isOnGround = (ground & (1 << c.gameObject.layer)) != 0;

                if (isOnGround)
                {
                    if (this.ShouldCollideWithGround)
                    {
                        if (preCheck)
                        {
                            // Quit early since this is just a pre-check
                            return true;
                        }

                        if (this.animatorObject != null)
                        {
                            this.animatorObject.Play("Hit");
                        }

                        if (this.ShouldGetDestroyedWhenCollided)
                        {
                            GameObject.Destroy(this.gameObject);
                        }
                        else if (this.ShouldFallWhenCollided)
                        {
                            SliderJoint2D joint = this.GetComponent<SliderJoint2D>();
                            if (joint != null)
                            {
                                // joint.enabled = false;
                                joint.anchor = new Vector2(0,0);
                                joint.autoConfigureConnectedAnchor = true;
                                joint.angle = 90;
                            }
                        }
                        else
                        {
                            Rigidbody2D body = this.GetComponentInChildren<Rigidbody2D>();
                            if (body != null)
                            {
                                body.simulated = false;
                            }

                            Collider2D[] colliders = this.GetComponentsInChildren<Collider2D>();
                            for (int i = 0; i < colliders.Length; i++)
                            {
                                colliders[i].enabled = false;
                            }
                        }

                        if (this.CollideEmitter != null)
                        {
                            this.CollideEmitter.Play();
                        }

                        this.isStopped = true;
                    }
                    else
                    {
                        IgnoreCollisions(c.collider, true);
                    }
                }
                else
                {
                    destroy = true;
                }
            }

            // Apply damage to any character hit by this projectile
            Character character = c.transform.GetComponent<Character>();
            if (character != null)
            {
                float direction = c.contacts[0].point.x - character.transform.position.x;
                character.ApplyDamage(this.Damage, direction);
                destroy = true;
                this.isStopped = true;
            }

            if (destroy)
            {
                GameObject.Destroy(this.gameObject);
            }

            return this.isStopped;
        }

        private void IgnoreCollisions(Collider2D collider, bool ignore)
        {
            Collider2D[] projectileColliders = this.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < projectileColliders.Length; i++)
            {
                Physics2D.IgnoreCollision(projectileColliders[i], collider, ignore);
            }
        }

        protected IEnumerator DestroyAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            GameObject.Destroy(this.gameObject);
        }
    }
}
