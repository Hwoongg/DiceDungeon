using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class Character : MonoBehaviour
    {
        // Editor Properties
        [Header("Basic Properties")]
        [Tooltip("Health of the character")]
        public int MaxHealth = 100;
        [Tooltip("Speed of the character when running")]
        public float RunSpeed = 250;
        [Tooltip("Factor of run speed to lose(-)/gain(+) when pressing the modifier. Use for run boost or sneak.")]
        [Range(-1, 1)]
        public float RunModifierFactor = -0.75f;
        [Tooltip("Force applied to character when pressing jump")]
        public float JumpPower = 380;
        [Tooltip("Factor of velocity to lose(-)/gain(+) when blocking (if enabled)")]
        [Range(-1, 1)]
        public float BlockingMoveFactor = -0.75f;
        [Tooltip("Use head bobbing while walking")]
        public bool UseWalkBob = true;
        [Tooltip("Is the character a zombie")]
        public bool IsZombified = false;

        [Header("Control Options")]
        [Tooltip("Should the character ignore all the mecanim animation states and remain static (useful for character select screens)")]
        public bool IgnoreAnimationStates = false;
        [Tooltip("Should the character fire projectiles and effects when playing animations")]
        public bool CastProjectiles = true;
        [Tooltip("Should the character be allowed to control direction while in the air")]
        public bool AllowAirControl = true;
        [Tooltip("Should the character be allowed to double jump")]
        public bool AllowDoubleJump = true;
        [Tooltip("Allow the character to jump down oneway platforms (containing a PlatformEffector2D with oneway set to true)")]
        public bool AllowJumpDownPlatforms = true;
        [Tooltip("Should the character be allowed to jump while sliding down a wall")]
        public bool AllowWallJump = true;
        [Tooltip("Should the character be allowed to jump higer if they hold down the jump key")]
        public bool AllowBiggerJumpOnHold = true;
        [Tooltip("Allow the character to slide on ice trigger colliders (named or tagged 'ice')")]
        public bool UseIce = true;
        [Tooltip("Allow the character to climb ladder trigger colliders (named or tagged 'ladder')")]
        public bool UseLadders = true;
        [Tooltip("Allow the character to move slower in water trigger colliders (named or tagged 'water')")]
        public bool UseWater = true;
        [Tooltip("Lock the character's body parts to pixel positions")]
        public bool UsePixelPositioning = true;

        [Header("Advanced Properties")]
        [Tooltip("The velocity the character must be travelling to create a dust cloud when landing")]
        public float DustCloudThreshold = -10;
        [Tooltip("Amount of friction to use for ice")]
        public float IceFriction = 1f;
        [Tooltip("The time to wait after jumping down before re-enabling the platform")]
        public float JumpDownTimeout = 1;
        [Tooltip("Gravity scale applied when jumping to prevent the jump feeling 'floaty'")]
        public float JumpGravityScale = 3;
        [Tooltip("Gravity scale applied when quickly pressing the jump button rather than a long press")]
        public float JumpQuickGravityScale = 1.5f;
        [Tooltip("Force applied to character horizontally when jumping off a wall")]
        public float WallJumpHorizontalPower = 100;
        [Tooltip("Factor of velocity to lose(-)/gain(+) when sliding down a wall")]
        [Range(-1, 1)]
        public float WallSlideFactor = -0.25f;
        [Tooltip("Factor of velocity to lose(-)/gain(+) when moving in water")]
        [Range(-1, 1)]
        public float WaterMoveFactor = -0.75f;
        [Tooltip("The number of pixels per unit, used for pixel positioning")]
        public int PixelsPerUnit = 16;

        [Header("Weapon")]
        [Tooltip("Type of weapon character is carrying. Used in animations.")]
        public WeaponType EquippedWeaponType;
        [Tooltip("Can the character block?")]
        public bool IsBlockEnabled = false;
        [Tooltip("Transform position used as the spawn point for projectiles")]
        public Transform LaunchPoint;
        [Tooltip("Projectile to spawn when casting")]
        public WeaponProjectile CastProjectile;
        [Tooltip("Projectile to spawn when quick casting")]
        public WeaponProjectile CastQuickProjectile;
        [Tooltip("Projectile to spawn when throwing from main hand")]
        public WeaponProjectile ThrowMainProjectile;
        [Tooltip("Projectile to spawn when throwing from off hand")]
        public WeaponProjectile ThrowOffProjectile;
        [Tooltip("Transform position used to spawn an effect")]
        public Transform CastFXPoint;
        [Tooltip("Type of effect to spawn during cast")]
        public WeaponFX CastFX;

        [Header("Objects")]
        [Tooltip("Transform used to check if the character is touching the ground")]
        public Transform GroundChecker;
        [Tooltip("Layer that contains all the 'ground' colliders")]
        public LayerMask GroundLayer;
        [Tooltip("Transform used to check if the character is sliding down a wall forwards")]
        public Transform WallCheckerFront;
        [Tooltip("Transform used to check if the character is sliding down a wall backwards")]
        public Transform WallCheckerBack;
        [Tooltip("Layer that contains all the 'wall' colliders")]
        public LayerMask WallLayer;
        [Tooltip("Transform used to position bubbles when under water")]
        public Transform BubblePoint;
        [Tooltip("The collider that represents the characters weapon (for ignoring collisions)")]
        public Collider2D WeaponObject;
        [Tooltip("The transform parent to all the body parts used for pixel positioning")]
        public Transform Skeleton;
        [Tooltip("A particle system to spawn when entering water")]
        public ParticleSystem SplashEmitter;
        [Tooltip("A particle system to spawn when underwater")]
        public ParticleSystem BubbleEmitter;
        [Tooltip("A particle system to spawn when jumping")]
        public ParticleSystem DustEmitter;

        // Script Properties
        public int CurrentHealth { get; private set; }
        public bool IsDead { get { return this.CurrentHealth <= 0; } }
        public Direction CurrentDirection { get; private set; }
        public float ModifiedSpeed
        {
            get
            {
                return this.RunSpeed * this.GetMultiplier(this.RunModifierFactor);
            }
        }
        public bool IsAttacking
        {
            get
            {
                AnimatorStateInfo state = this.animatorObject.GetCurrentAnimatorStateInfo(3);
                return state.IsName("Attack") || state.IsName("QuickAttack");
            }
        }

        public enum WeaponType
        {
            None = 0,
            Staff = 1,
            Sword = 2,
            Bow = 3,
            Gun = 4
        }

        public enum Direction
        {
            Left = -1,
            Right = 1
        }

        [System.Flags]
        public enum Action
        {
            Jump = 1,
            RunModified = 2,
            QuickAttack = 4,
            Attack = 8,
            Cast = 16,
            ThrowOff = 32,
            ThromMain = 64,
            Consume = 128,
            Block = 256,
            Hurt = 512,
            JumpDown = 1024,
            Crouch = 2048
        }

        // Members
        private Animator animatorObject;
        private Rigidbody2D body;
        private CapsuleCollider2D bodyCollider;
        private ContactFilter2D groundCheckFilter;
        private ContactFilter2D wallCheckFilter;
        private Collider2D[] layerCheckResults;
        private Collider2D ladderCollider;
        private bool isGrounded = true;
        private bool isOnWall = false;
        private bool isOnWallFront = false;
        private bool isOnLadder = false;
        private bool isInWater = false;
        private bool isOnIce = false;
        private bool isBlocking = false;
        private bool isCrouching = false;
        private bool isJumpPressed = false;
        private bool isReadyForDust = false;
        private bool isRunningNormal = false;
        private int jumpCount = 0;
        private float groundRadius = 0.1f;
        private float wallDecayX = 0.006f;
        private float wallJumpX = 0;
        private WeaponFX activeFX;
        private Direction startDirection = Direction.Right;
        private Vector3 pixelPosition = new Vector3();
        private float pixelsPerUnitFactor;
        private List<Transform> pixelBodyParts;
        private ParticleSystem bubblesSystem;

        void Awake()
        {
            // Setup the ground and wall check variables
            this.groundCheckFilter = new ContactFilter2D();
            this.groundCheckFilter.layerMask = this.GroundLayer;
            this.groundCheckFilter.useLayerMask = true;

            this.wallCheckFilter = new ContactFilter2D();
            this.wallCheckFilter.layerMask = this.WallLayer;
            this.wallCheckFilter.useLayerMask = true;

            this.layerCheckResults = new Collider2D[2];

            // Grab the editor objects
            this.body = this.GetComponent<Rigidbody2D>();
            this.bodyCollider = this.GetComponent<CapsuleCollider2D>();
            this.animatorObject = this.GetComponent<Animator>();

            // Setup the character
            this.CurrentHealth = this.MaxHealth;
            this.ApplyDamage(0);
            if (this.startDirection != Direction.Right)
            {
                this.ChangeDirection(this.startDirection);
            }
            else
            {
                this.CurrentDirection = this.startDirection;
            }

            this.pixelsPerUnitFactor = 1f / this.PixelsPerUnit;
            this.pixelBodyParts = new List<Transform>();
            var parts = this.Skeleton.GetComponentsInChildren<Transform>();
            foreach (var p in parts)
            {
                if (p.GetComponents<Component>().Length == 1)
                {
                    this.pixelBodyParts.Add(p);
                }
            }
        }

        void Start()
        {
            // Perform an initial ground check
            this.isGrounded = this.CheckGround();

            if (this.IgnoreAnimationStates)
            {
                this.isGrounded = true;
                this.animatorObject.SetBool("IsGrounded", this.isGrounded);
                return;
            }
        }

        void FixedUpdate()
        {
            // Check if we are touching the ground using the rigidbody
            this.isGrounded = this.CheckGround();

            // Check if we are touching a wall when wall jump is allowed
            bool isOnWallFront = false;
            bool isOnWallBack = false;
            if (this.AllowWallJump && !this.isGrounded && this.body.velocity.y <= 0)
            {
                isOnWallFront = this.CheckWall(WallCheckerFront.position);
                isOnWallBack = this.CheckWall(WallCheckerBack.position);
            }

            this.isOnWall = (isOnWallFront || isOnWallBack);
            this.isOnWallFront = (this.isOnWall && isOnWallFront);

            // Prevent the player from moving down past the bottom of a ladder
            if (this.isOnLadder && this.isGrounded && this.body.velocity.y < 0)
            {
                // Look below the character to see if we will still be on the ladder if we move down
                bool isSafe = this.CheckLadder(0.15f);
                if (!isSafe)
                {
                    // If it isn't safe, prevent the character moving down
                    this.body.velocity = this.body.velocity * Vector3.up * 0;
                }
            }
        }

        void Update()
        {
            // Add gravity to any jump so that it doesn't feel so 'floaty'
            if (this.body.velocity.y < 0)
            {
                this.body.velocity += Vector2.up * Physics2D.gravity * this.JumpGravityScale * Time.deltaTime;
            }
            else if (this.body.velocity.y > 0 && (!this.AllowBiggerJumpOnHold || !Input.GetButton("Jump")))
            {
                this.body.velocity += Vector2.up * Physics2D.gravity * this.JumpQuickGravityScale * Time.deltaTime;
            }
        }

        void LateUpdate()
        {
            if (this.UsePixelPositioning)
            {
                // Round each transform position to a pixel offset to get spritesheet like animations
                foreach (var p in this.pixelBodyParts)
                {
                    pixelPosition = p.localPosition;
                    pixelPosition.x = Mathf.Round(pixelPosition.x * this.PixelsPerUnit) * this.pixelsPerUnitFactor;
                    pixelPosition.y = Mathf.Round(pixelPosition.y * this.PixelsPerUnit) * this.pixelsPerUnitFactor;
                    pixelPosition.z = Mathf.Round(pixelPosition.z * this.PixelsPerUnit) * this.pixelsPerUnitFactor;
                    p.localPosition = pixelPosition;
                }
            }
        }

        void OnCollisionEnter2D(Collision2D c)
        {
            // Check if we hit the ground (going downwards)
            if ((this.GroundLayer.value & 1 << c.gameObject.layer) != 0 &&
                c.contacts[0].normal.y > 0.8f)
            {
                // If we did, check to see if we either just jumped or are falling fast enough for dust
                if (this.isReadyForDust || this.body.velocity.y < this.DustCloudThreshold)
                {
                    this.CreateDustCloud(c.contacts[0].point);
                }

                // We hit the ground after jumping
                this.isReadyForDust = false;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Check if they are splashing into water
            if (this.UseWater && this.IsTrigger(other.gameObject, "water"))
            {
                Vector2 pos = this.GroundChecker.position;
                pos.y = other.bounds.center.y + other.bounds.extents.y;
                this.CreateSplash(pos);

                // Play bubbles
                if (this.BubbleEmitter != null && !this.BubbleEmitter.isPlaying)
                {
                    if (this.bubblesSystem == null && this.BubblePoint != null)
                    {
                        // Instansiate the bubbles
                        this.bubblesSystem = GameObject.Instantiate<ParticleSystem>(this.BubbleEmitter);
                        this.bubblesSystem.gameObject.transform.SetParent(this.BubblePoint, false);
                        this.BubbleEmitter = this.bubblesSystem;
                    }

                    this.BubbleEmitter.gameObject.SetActive(true);
                    this.BubbleEmitter.Play();
                }
            }

            // Check if we collided with a main item weapon
            if (this.IsTrigger(other.gameObject, this.WeaponObject.gameObject.name))
            {
                // Take some damage if we are attacked
                Character hurtBy = other.GetComponentInParent<Character>();
                if (hurtBy != null && hurtBy != this && hurtBy.IsAttacking)
                {
                    // Apply damage to this character
                    float direction = other.transform.position.x - this.transform.position.x;
                    this.ApplyDamage(100, direction);
                }
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            // Check for ladders (but only if the character isn't running past it)
            if (this.UseLadders && this.IsTrigger(other.gameObject, "ladder") && Mathf.Approximately(this.body.velocity.x, 0) && this.bodyCollider.IsTouching(other))
            {
                this.ladderCollider = other;
                this.body.isKinematic = true;
                this.isOnLadder = true;

                // Only show the ladder animation if we are moving on it
                bool showAnimation = (Mathf.Abs(this.body.velocity.x) > 0 || Mathf.Abs(this.body.velocity.y) > 0);
                if (!showAnimation)
                {
                    // Or if we are stood on it (this prevents showing the animation when we are stood at the top)
                    showAnimation = this.CheckLadder(this.groundRadius * -1.5f);
                }
                this.animatorObject.SetBool("IsOnLadder", showAnimation);
                if (this.isOnLadder)
                {
                    this.ChangeDirection(Direction.Right);
                }
            }

            // Check for water
            if (this.UseWater && this.IsTrigger(other.gameObject, "water"))
            {
                this.isInWater = true;
                this.animatorObject.SetBool("IsInWater", this.isInWater);
            }

            // Check for ice
            if (this.UseIce && this.IsTrigger(other.gameObject, "ice"))
            {
                this.isOnIce = true;
                this.animatorObject.SetBool("IsOnIce", this.isOnIce);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            // Check for ladders
            if (this.UseLadders && this.IsTrigger(other.gameObject, "ladder") && !this.bodyCollider.IsTouching(other))
            {
                this.body.isKinematic = false;
                this.isOnLadder = false;
                this.animatorObject.SetBool("IsOnLadder", this.isOnLadder);
            }

            // Check for water
            if (this.UseWater && this.IsTrigger(other.gameObject, "water"))
            {
                this.isInWater = false;
                this.animatorObject.SetBool("IsInWater", this.isInWater);

                if (this.BubbleEmitter != null && this.BubbleEmitter.isPlaying)
                {
                    this.BubbleEmitter.Stop();
                    this.BubbleEmitter.gameObject.SetActive(false);
                }
            }

            // Check for ice
            if (this.UseIce && this.IsTrigger(other.gameObject, "ice"))
            {
                this.isOnIce = false;
                this.animatorObject.SetBool("IsOnIce", this.isOnIce);
            }
        }

        /// <summary>
        /// Perform movement for the character
        /// This should be called from the FixedUpdate() method as it makes changes to physics properties
        /// </summary>
        /// <param name="axis">The x and y values used for running and climbing ladders</param>
        /// <param name="isHorizontalStillPressed">True if the user is still actively pressing the horizontal axis</param>
        public void Move(Vector2 axis, bool isHorizontalStillPressed)
        {
            // Quit early if dead
            if (this.IsDead)
            {
                this.body.velocity = new Vector2(0, this.body.velocity.y);
                return;
            }

            if (this.IgnoreAnimationStates)
            {
                this.isGrounded = true;
                this.animatorObject.SetBool("IsGrounded", this.isGrounded);
                return;
            }

            // Get the input and speed
            if (this.isGrounded || this.AllowAirControl)
            {
                float horizontal = axis.x;

                // Check if we are jumping off a wall when using AirControl
                if (this.AllowWallJump && !this.isGrounded && !Mathf.Approximately(this.wallJumpX, 0))
                {
                    if (!isHorizontalStillPressed)
                    {
                        // Wall jumps with AirControl emulate a user pressing the direction,
                        // so that they jump off at an angle. You can't just rely on the 
                        // rigidbody force since we overwrite it with the user input.
                        this.wallJumpX = Mathf.Lerp(this.wallJumpX, 0, Time.deltaTime);
                        horizontal = this.wallJumpX;
                    }
                    else
                    {
                        this.wallJumpX = 0;
                    }
                }

                // Set the new velocity for the character based on the run modifier
                float speed = (this.isRunningNormal ? this.RunSpeed : this.ModifiedSpeed);
                Vector2 newVelocity = new Vector2(horizontal * speed * Time.deltaTime, this.body.velocity.y);

                if (this.isOnIce)
                {
                    // If on ice we should slide
                    newVelocity.x = Mathf.Lerp(this.body.velocity.x, newVelocity.x, this.IceFriction * Time.deltaTime);
                }

                this.body.velocity = newVelocity;
            }

            // If they pressed jump, then add some Y velocity
            if (this.isJumpPressed)
            {
                float xPower = 0;
                if (this.AllowWallJump && this.isOnWall)
                {
                    // Add horizontal power for wall jumps
                    xPower = this.WallJumpHorizontalPower * (int)this.CurrentDirection;
                    this.wallJumpX = xPower * this.wallDecayX;

                    // Show the dust cloud when we jump off a wall
                    this.CreateDustCloud(this.GroundChecker.position);
                }
                else
                {
                    this.wallJumpX = 0;
                }

                this.body.velocity = new Vector2(this.body.velocity.x, 0);
                this.body.AddForce(new Vector2(xPower, this.JumpPower));
                this.isJumpPressed = false;
                this.isReadyForDust = true;

                // If we are double jumping, play an extra rolling animation,
                // so that the jump looks cooler.
                if (this.jumpCount == 2)
                {
                    this.animatorObject.Play("Roll", LayerMask.NameToLayer("FX"));
                }
            }
            else if (this.isOnLadder)
            {
                // Ladder climbing means we use the Y axis
                float vertical = axis.y;
                this.body.velocity = new Vector2(this.body.velocity.x, vertical * this.RunSpeed * Time.deltaTime);
            }
            else if (this.isOnWall && !this.isGrounded)
            {
                // If they are sliding down a wall, slow them down
                if (this.body.velocity.y < 0 &&
                    (this.body.velocity.x >= 0 && this.CurrentDirection < 0 ||
                     this.body.velocity.x <= 0 && this.CurrentDirection > 0))
                {
                    this.body.velocity = new Vector2(this.body.velocity.x, this.body.velocity.y * this.GetMultiplier(this.WallSlideFactor));
                }
            }
            else if (this.isBlocking)
            {
                // Blocking changes the speed of the character
                this.body.velocity = new Vector2(this.body.velocity.x * this.GetMultiplier(this.BlockingMoveFactor), this.body.velocity.y);
            }

            if (this.isInWater)
            {
                // Water also changes the speed of the character
                float waterFactor = this.GetMultiplier(this.WaterMoveFactor);
                this.body.velocity = new Vector2(this.body.velocity.x * waterFactor, this.body.velocity.y);
            }

            // Update the animator
            this.animatorObject.SetBool("IsGrounded", this.isGrounded);
            this.animatorObject.SetBool("IsOnWall", this.isOnWall);
            this.animatorObject.SetInteger("WeaponType", (int)this.EquippedWeaponType);
            this.animatorObject.SetBool("IsWalkBobFree", !this.UseWalkBob);
            this.animatorObject.SetBool("IsZombified", this.IsZombified);
            this.animatorObject.SetFloat("AbsY", Mathf.Abs(this.body.velocity.y));
            this.animatorObject.SetFloat("VelocityY", this.body.velocity.y);
            this.animatorObject.SetFloat("VelocityX", Mathf.Abs(this.body.velocity.x));
            this.animatorObject.SetBool("HasMoveInput", isHorizontalStillPressed);
            this.animatorObject.SetBool("IsCrouching", this.isCrouching && this.isGrounded);
            this.animatorObject.SetFloat("CrouchModifier", (this.isCrouching || this.isBlocking || this.isInWater ? Mathf.Abs(this.RunModifierFactor) * (this.isCrouching ? 0.8f : 1) : 1));

            // Flip the sprites if necessary
            if (this.isOnWall)
            {
                if (this.isOnWallFront && !this.isOnLadder)
                {
                    this.ChangeDirection(this.CurrentDirection == Direction.Left ? Direction.Right : Direction.Left);
                }
            }
            else if (this.body.velocity.x != 0)
            {
                this.ChangeDirection(this.body.velocity.x < 0 ? Direction.Left : Direction.Right);
            }

            this.FixedUpdate();
        }

        /// <summary>
        /// Perform the specified actions for the character
        /// </summary>
        /// <param name="action">A combined set of flags for all the actions the character should perform</param>
        public void Perform(Action action)
        {
            // Quit early if dead or on a ladder (since we can't do any actions either way)
            if (this.IsDead || this.isOnLadder)
            {
                return;
            }

            // Check if we are blocking
            this.isBlocking = IsAction(action, Action.Block) && this.IsBlockEnabled;

            // Check if we are crouching
            this.isCrouching = IsAction(action, Action.Crouch);

            // Check for the running modifier key
            this.isRunningNormal = !IsAction(action, Action.RunModified);

            // Reset the jump count if we are on the ground
            if (this.isGrounded)
            {
                this.jumpCount = 0;
            }

            // Check for jumping down since we need to remove the regular jump flag if we are
            if (IsAction(action, Action.JumpDown))
            {
                Collider2D ground = this.CheckGround();
                if (ground != null)
                {
                    PlatformEffector2D fx = ground.GetComponent<PlatformEffector2D>();
                    if (fx != null && fx.useOneWay)
                    {
                        ground.enabled = false;
                        action &= ~Action.Jump;

                        StartCoroutine(this.EnableAfter(this.JumpDownTimeout, ground));
                    }
                }
            }

            // Now check the rest of the keys for actions
            if (IsAction(action, Action.Jump) && !this.isJumpPressed)
            {
                // Prevent them jumping on ladders
                if (!this.isOnLadder)
                {
                    if (this.isGrounded || (this.AllowWallJump && this.isOnWall))
                    {
                        this.isJumpPressed = true;
                        this.jumpCount = 1;
                    }
                    else if (this.AllowDoubleJump && this.jumpCount <= 1)
                    {
                        this.isJumpPressed = true;
                        this.jumpCount = 2;
                    }
                }
            }
            else if (IsAction(action, Action.QuickAttack))
            {
                this.TriggerAction("TriggerQuickAttack");
            }
            else if (IsAction(action, Action.Attack))
            {
                this.TriggerAction("TriggerAttack");
            }
            else if (IsAction(action, Action.Cast))
            {
                this.TriggerAction("TriggerCast");
            }
            else if (IsAction(action, Action.ThrowOff))
            {
                this.TriggerAction("TriggerThrowOff");
            }
            else if (IsAction(action, Action.ThromMain))
            {
                this.TriggerAction("TriggerThrowMain");
            }
            else if (IsAction(action, Action.Consume))
            {
                this.TriggerAction("TriggerConsume");
            }
            else if (this.isBlocking && !this.animatorObject.GetBool("IsBlocking"))
            {
                this.TriggerAction("TriggerBlock");
            }
            else if (IsAction(action, Action.Hurt))
            {
                // Apply some damage to test the animation,
                // In your code will likely want to call ApplyDamage directly.
                this.ApplyDamage(34);
            }

            // Reset the blocking animation if they let go of the block button
            if (!this.isBlocking)
            {
                this.animatorObject.SetBool("IsBlocking", this.isBlocking);
            }
        }

        /// <summary>
        /// Reduce the health of the character by the specified amount
        /// </summary>
        /// <param name="damage">The amount of damage to apply</param>
        /// <param name="direction">The direction that the damage came from (left < 0 > right)</param>
        /// <returns>True if the character dies from this damage, False if it remains alive</returns>
        public bool ApplyDamage(int damage, float direction = 0)
        {
            if (!this.IsDead)
            {
                this.animatorObject.SetFloat("LastHitDirection", direction * (int)this.CurrentDirection);

                // Update the health
                this.CurrentHealth = Mathf.Clamp(this.CurrentHealth - damage, 0, this.MaxHealth);
                this.animatorObject.SetInteger("Health", this.CurrentHealth);

                if (damage != 0)
                {
                    // Show the hurt animation
                    this.TriggerAction("TriggerHurt", false);
                }

                if (this.CurrentHealth <= 0)
                {
                    // Since the player is dead, remove the corpse
                    StartCoroutine(this.DestroyAfter(1.5f, this.gameObject));
                }
            }

            return this.IsDead;
        }

        /// <summary>
        /// Resets the polygon collider on the 'MainItemPart' gameobject
        /// </summary>
        public void UpdateWeaponCollision()
        {
            SpriteRenderer[] srs = this.gameObject.transform.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in srs)
            {
                if (string.Equals(sr.gameObject.name, "MainItemSprite", System.StringComparison.OrdinalIgnoreCase))
                {
                    PolygonCollider2D pc = sr.gameObject.GetComponent<PolygonCollider2D>();
                    if (pc != null)
                    {
                        Destroy(pc);
                    }

                    if (sr.sprite != null)
                    {
                        pc = sr.gameObject.AddComponent<PolygonCollider2D>();
                        pc.isTrigger = true;
                        pc.gameObject.layer = LayerMask.NameToLayer("CharacterWeapons");
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Force the character to face a specific direction
        /// </summary>
        /// <param name="newDirection">Use -1 for left and 1 for right</param>
        public void ForceDirection(Direction newDirection)
        {
            this.ChangeDirection(newDirection);
        }

        private void TriggerAction(string action, bool isCombatAction = true)
        {
            // Update the animator object
            this.animatorObject.SetTrigger(action);
            this.animatorObject.SetBool("IsBlocking", this.isBlocking);

            if (isCombatAction)
            {
                // Combat actions also trigger an additional parameter to move correctly through states
                this.animatorObject.SetTrigger("TriggerCombatAction");
            }
        }

        private void ChangeDirection(Direction newDirection)
        {
            // Quit early if we are already facing the new direction
            if (this.CurrentDirection == newDirection)
            {
                return;
            }

            // Swap the direction of the sprites
            this.transform.localScale = new Vector3(newDirection == Direction.Left ? -1 : 1, 1, 1);
            this.CurrentDirection = newDirection;
        }

        private void OnCastStart()
        {
            // If we have an effect start it now
            if (this.CastFX != null && this.CastProjectiles)
            {
                this.activeFX = WeaponFX.Create(
                    this.CastFX,
                    this.CastFXPoint,
                    (this.CurrentDirection == Direction.Left ? -1 : 1));
            }
        }

        private void OnCastFX()
        {
            // If we have an effect start it now
            if (this.activeFX != null && this.CastProjectiles)
            {
                this.activeFX.BeginFX();
            }
        }

        private void OnCastFXStop()
        {
            // If we have an effect stop it now
            if (this.activeFX != null)
            {
                this.activeFX.EndFX();
                this.activeFX = null;
            }
        }

        private void OnCastComplete()
        {
            // Stop the active effect once we cast
            this.OnCastFXStop();

            // Create the projectile
            this.LaunchProjectile(this.CastProjectile);
        }

        private void OnCastQuickComplete()
        {
            // Stop the active effect once we cast
            this.OnCastFXStop();

            // Create the projectile
            this.LaunchProjectile(this.CastQuickProjectile);
        }

        private void OnThrowMainComplete()
        {
            // Create the projectile for the main hand
            this.LaunchProjectile(this.ThrowMainProjectile);
        }

        private void OnThrowOffComplete()
        {
            // Create the projectile for the off hand
            this.LaunchProjectile(this.ThrowOffProjectile);
        }

        private void LaunchProjectile(WeaponProjectile projectile)
        {
            // Create the projectile
            if (projectile != null && this.CastProjectiles)
            {
                WeaponProjectile.Create(
                    projectile,
                    this,
                    this.LaunchPoint,
                    (this.CurrentDirection == Direction.Left ? -1 : 1));
            }
        }

        private void CreateSplash(Vector2 point)
        {
            if (this.SplashEmitter != null)
            {
                // Create a water effect that will die after a while
                ParticleSystem splash = GameObject.Instantiate<ParticleSystem>(this.SplashEmitter);
                TemporaryFX temp = splash.gameObject.AddComponent<TemporaryFX>();
                temp.LifeTime = splash.main.duration;
                splash.transform.position = point;
                splash.Play();
            }
        }

        private void CreateDustCloud(Vector2 point)
        {
            if (this.DustEmitter != null)
            {
                // Create a cloud of dust that will die after a while
                ParticleSystem dust = GameObject.Instantiate<ParticleSystem>(this.DustEmitter);
                TemporaryFX temp = dust.gameObject.AddComponent<TemporaryFX>();
                temp.LifeTime = dust.main.duration;
                dust.transform.position = point;
                dust.Play();
            }
        }

        private bool CheckLadder(float offset)
        {
            var colliders = Physics2D.OverlapCircleAll(this.GroundChecker.position + Vector3.down * offset, this.groundRadius);
            foreach (var c in colliders)
            {
                if (c != null && c.gameObject == this.ladderCollider.gameObject)
                {
                    // Found the ladder
                    return true;
                }
            }

            return false;
        }

        private Collider2D CheckGround()
        {
            return CheckLayer(this.GroundChecker.position, this.groundRadius, this.groundCheckFilter);
        }

        private Collider2D CheckWall(Vector3 position)
        {
            return CheckLayer(position, this.groundRadius, this.wallCheckFilter);
        }

        private Collider2D CheckLayer(Vector3 position, float radius, ContactFilter2D filter)
        {
            // Check if we are touching the ground using the rigidbody
            int count = Physics2D.OverlapCircle(position, radius, filter, this.layerCheckResults);
            for (int i = 0; i < count; i++)
            {
                if (this.layerCheckResults[i].gameObject != this.gameObject &&
                    this.layerCheckResults[i].gameObject != this.WeaponObject.gameObject)
                {
                    var data = this.layerCheckResults[i].GetComponent<DataTag>();
                    if (data != null && data.Tag == "Weapon")
                    {
                        var weapon = data.GetComponentInParent<WeaponProjectile>();
                        if (weapon != null && weapon.Owner == this)
                        {
                            continue;
                        }
                    }

                    return this.layerCheckResults[i];
                }
            }

            return null;
        }

        private bool IsTrigger(GameObject other, string name)
        {
            name = name.ToLower();

            if ((other.tag != null && other.tag.ToLower() == name) ||
                (other.name != null && other.name.ToLower() == name))
            {
                return true;
            }

            return false;
        }

        private bool IsAction(Action value, Action flag)
        {
            return (value & flag) != 0;
        }

        private float GetMultiplier(float factor)
        {
            if (Mathf.Sign(factor) < 0)
            {
                return 1 + factor;
            }
            else
            {
                return factor;
            }
        }

        private IEnumerator DestroyAfter(float seconds, GameObject gameObject)
        {
            yield return new WaitForSeconds(seconds);

            GameObject.Destroy(gameObject);
        }

        private IEnumerator EnableAfter(float seconds, Behaviour obj)
        {
            yield return new WaitForSeconds(seconds);

            obj.enabled = true;
        }
    }
}