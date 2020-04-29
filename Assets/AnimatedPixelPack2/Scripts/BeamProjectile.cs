using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class BeamProjectile : WeaponProjectile
    {
        // Editor Properties
        [Header("Beam")]
        [Tooltip("The distance at which to cast the beam")]
        public float XDistance = 2;
        [Tooltip("Make sure the beam only collides with tilemap colliders")]
        public bool OnlyCollideWithTilemaps = true;

        // Members
        private Transform holder;
        private Vector3 scale = new Vector3(0, 1, 1);
        private float scaleSpeed = 5;
        private ContactFilter2D groundCheckFilter;
        private RaycastHit2D[] layerCheckResults;

        protected override void Start()
        {
            base.Start();

            // Setup a filter we can use to identify the ground
            this.layerCheckResults = new RaycastHit2D[5];
            this.groundCheckFilter = new ContactFilter2D();
            this.groundCheckFilter.layerMask = (this.OverrideGroundLayer == 0 && this.Owner != null ? this.Owner.GroundLayer : this.OverrideGroundLayer);
            this.groundCheckFilter.useLayerMask = true;

            this.holder = this.transform.GetChild(0);
            this.holder.localScale = this.scale;

            // Move to the X position away from the caster
            Vector3 pos = this.transform.position;
            pos.x += this.XDistance * this.DirectionX;

            // Find the ground at this x position
            int count = Physics2D.Raycast(pos, -Vector2.up, this.groundCheckFilter, this.layerCheckResults, 50);
            for (int i = 0; i < count; i++)
            {
                if (OnlyCollideWithTilemaps && this.layerCheckResults[i].collider.gameObject.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() == null)
                {
                    // Ignore non-tile entities
                    continue;
                }

                // Set the beam position to end at this ground position
                this.transform.position = new Vector3(pos.x, this.layerCheckResults[i].point.y, this.transform.position.z);
            }
        }

        protected override void Update()
        {
            base.Update();

            // Scale the beam outwards so it looks like it is growing
            this.scale.x = Mathf.Clamp01(this.scale.x + this.scaleSpeed * Time.deltaTime);
            this.holder.localScale = this.scale;
        }

        protected void OnTriggerEnter2D(Collider2D other)
        {            
            // Apply damage to any character hit by this beam
            Character character = other.transform.GetComponent<Character>();
            if (character != null)
            {
                float direction = (character.CurrentDirection == Character.Direction.Left ? -1 : 1);
                character.ApplyDamage(this.Damage, direction);
            }
        }
    }
}