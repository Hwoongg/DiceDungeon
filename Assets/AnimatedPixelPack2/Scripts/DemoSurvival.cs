using System.Collections;
using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    [RequireComponent(typeof(DemoController))]
    public class DemoSurvival : MonoBehaviour
    {
        // Editor Properties
        public float Speed = 2;
        public Transform[] Waves;

        // Members
        private DemoController controller;
        private Animator animator;
        private Rigidbody2D body;
        private Vector2 axis;
        private Character player;
        private float waveTime = 0;

        void Awake()
        {
            // Hook up the event so we know when the player changes
            this.controller = this.GetComponent<DemoController>();
            this.controller.OnSelectedCharacterChanged += this.Controller_OnSelectedCharacterChanged;

            this.axis = new Vector2();
        }

        private void Controller_OnSelectedCharacterChanged(Character newCharacter)
        {
            // Set the new player
            this.player = newCharacter;
            this.animator = this.player.GetComponent<Animator>();
            this.body = this.player.GetComponent<Rigidbody2D>();
            
            // Get rid of the default input since we control it from here in this demo
            Destroy(this.player.GetComponent<CharacterInput>());

            // Remove the other platformer pieces
            this.player.IgnoreAnimationStates = true;
            this.body.bodyType = RigidbodyType2D.Dynamic;
            this.body.gravityScale = 0;
            this.animator.SetInteger("WeaponType", (int)this.player.EquippedWeaponType);

            // Add our script that makes the sprite sorting look correct for this topdown style camera
            this.player.gameObject.AddComponent<DynamicZIndex>();
        }

        void FixedUpdate()
        {
            // Move the character using the axis as the input
            this.body.velocity = this.axis * this.Speed;
        }

        void Update()
        {
            // Move the sea level a bit to make a nice effect
            this.waveTime += Time.deltaTime;
            for (int i = 0; i < this.Waves.Length; i++)
            {
                float y = Mathf.Sin(this.waveTime * Mathf.PI) * 0.003f;
                this.Waves[i].localPosition += Vector3.up * y;
            }

            if (this.player == null)
            {
                return;
            }

            // Get the player input for movement
            this.axis.x = Input.GetAxis("Horizontal");
            this.axis.y = Input.GetAxis("Vertical");
            this.axis.Normalize();

            // Set the animations and direction
            float movement = Mathf.Abs(this.axis.x) + Mathf.Abs(this.axis.y);
            this.animator.SetFloat("VelocityX", movement);
            this.animator.SetBool("HasMoveInput", movement > 0);
            if (this.axis.x != 0)
            {
                this.player.ForceDirection(this.axis.x < 0 ? Character.Direction.Left : Character.Direction.Right);
            }

            // Check for a chopping action
            Character.Action actionFlags = 0;
            actionFlags |= (Input.GetButtonDown("Fire1") ? Character.Action.QuickAttack : 0);
            this.player.Perform(actionFlags);

            if (actionFlags != 0)
            {
                StartCoroutine(this.ChopTree());
            }
        }

        private IEnumerator ChopTree()
        {
            // Wait until the chop animation is in the down position
            yield return new WaitForSeconds(0.15f);

            // See if we chop down a tree in this simplified demo
            var colliders = Physics2D.OverlapCircleAll(this.player.WallCheckerFront.position, 0.5f);
            foreach (var c in colliders)
            {
                var resource = c.GetComponent<SurvivalResource>();
                if (resource != null)
                {
                    resource.ApplyDamage(3);
                    break;
                }
            }
        }
    }
}
