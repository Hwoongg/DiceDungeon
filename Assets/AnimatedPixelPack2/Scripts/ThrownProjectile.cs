using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class ThrownProjectile : HorizontalProjectile
    {
        // Editor Properties
        [Header("Thrown")]
        [Tooltip("Should the sprite be generated from the MainItem (false will use OffItem)")]
        public bool IsMainItem = true;
        [Tooltip("The starting angle to rotate the sprite when thrown")]
        public float StartRotation = 0f;
        [Tooltip("The angle to rotate the sprite when it has landed")]
        public float EndRotation = 0f;
        [Tooltip("The empty gameobject to use as the holder for colliders")]
        public Transform CollisionHolder;

        // Members
        private bool isEndRotationSet;
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer outlineSpriteRenderer;

        protected override void Start()
        {
            base.Start();

            this.spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
            this.outlineSpriteRenderer = this.spriteRenderer.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>();

            // Get the sprite from the hand of the character
            GameObject weaponPart = null;
            Sprite weaponSprite = this.spriteRenderer.sprite;
            Sprite weaponOutlineSprite = this.outlineSpriteRenderer.sprite;

            // Search the character for the appropriate weapon sprite
            Collider2D weaponBox = this.GetComponentInChildren<Collider2D>();
            if (this.Owner != null)
            {
                SpriteRenderer[] parts = this.Owner.GetComponentsInChildren<SpriteRenderer>();
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].name == (this.IsMainItem ? "HeldItemMainSprite" : "HeldItemOffSprite"))
                    {
                        weaponPart = parts[i].gameObject;
                        weaponSprite = parts[i].sprite;

                        // Now search this weapon's children for the outline sprite
                        for (int j = 0; j < parts[i].transform.childCount; j++)
                        {
                            var child = parts[i].transform.GetChild(j);
                            if (child.name.EndsWith("-Outline"))
                            {
                                weaponOutlineSprite = child.GetComponentInChildren<SpriteRenderer>().sprite;
                                break;
                            }
                        }
                        weaponBox = null;
                        break;
                    }
                }
            }

            // Update our sprite to match the one we are throwing
            if (weaponSprite != null)
            {
                if (this.spriteRenderer != null)
                {
                    this.spriteRenderer.sprite = weaponSprite;
                    this.outlineSpriteRenderer.sprite = weaponOutlineSprite;
                    this.outlineSpriteRenderer.color = this.Owner.GetComponent<PixelPartsOutline>().OutlineTint;
                    this.outlineSpriteRenderer.enabled = this.Owner.GetComponent<PixelPartsOutline>().UseOutline;
                }
            }

            // If a collision box is not found, we generate one using PolygonCollider2D so that it fits the shape of the sprite
            if (weaponBox == null)
            {
                // Create a new collider that will take the shape of the sprite
                var spriteCollider = this.spriteRenderer.gameObject.AddComponent<PolygonCollider2D>();
                spriteCollider.enabled = false;

                // Now copy it into different gameobjects that will not rotate
                var collisionO = new GameObject("CollisionOrigin");
                collisionO.transform.SetParent(this.CollisionHolder, false);
                var cO = collisionO.gameObject.AddComponent<PolygonCollider2D>();
                cO.points = spriteCollider.points;
                cO.enabled = true;

                var collisionR = new GameObject("CollisionRotated");
                collisionR.transform.SetParent(this.CollisionHolder, false);
                var cR = collisionR.gameObject.AddComponent<PolygonCollider2D>();
                cR.points = spriteCollider.points;
                cR.transform.localEulerAngles = new Vector3(0, 0, 90);
                cR.enabled = true;

                // Make the generated colliders a bit smaller
                this.CollisionHolder.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                // Center the sprite based on the new collider
                Rigidbody2D body = this.GetComponent<Rigidbody2D>();
                if (body != null && weaponPart != null)
                {
                    Vector3 dirScale = new Vector3(this.DirectionX, 1, 1);
                    this.spriteRenderer.transform.localPosition = Vector3.Scale(-body.centerOfMass, dirScale);
                    this.RotateTransform.localEulerAngles = new Vector3(0, 0, this.StartRotation);

                    // If we are going to rotate, adjust the start position so we don't just hit the ground immediately
                    if (this.RotationSpeed != 0 || this.StartRotation != 0)
                    {
                        var minY = Mathf.Min(cO.bounds.extents.y, cR.bounds.extents.y) * 2;
                        var maxY = Mathf.Max(cO.bounds.extents.y, cR.bounds.extents.y) * 2;

                        if (this.RotationSpeed != 0 && this.launchPoint.localPosition.y < maxY)
                        {
                            this.transform.localPosition += Vector3.up * (maxY - this.launchPoint.localPosition.y);
                        }
                        else if (this.RotationSpeed == 0 && this.launchPoint.localPosition.y < minY)
                        {
                            this.transform.localPosition += Vector3.up * (minY - this.launchPoint.localPosition.y);
                        }
                        else if (this.RotationSpeed == 0)
                        {
                            if (this.StartRotation != 0)
                            {
                                if (this.launchPoint.localPosition.y < cR.bounds.extents.y * 2)
                                {
                                    this.transform.localPosition += Vector3.up * (cR.bounds.extents.y * 2 - this.launchPoint.localPosition.y);
                                }
                                cO.enabled = false;
                            }
                            else if (this.StartRotation == 0)
                            {
                                if (this.launchPoint.localPosition.y < cO.bounds.extents.y * 2)
                                {
                                    this.transform.localPosition += Vector3.up * (cO.bounds.extents.y * 2 - this.launchPoint.localPosition.y);
                                }
                                cR.enabled = false;
                            }
                        }

                        // Reset the slider joint that prevents any Y movement
                        SliderJoint2D joint = this.GetComponent<SliderJoint2D>();
                        if (joint != null)
                        {
                            joint.anchor = new Vector2(this.transform.position.x, -this.transform.position.y);
                        }
                    }
                }

                // Since we updated the collider we need to re-ignore the collisions
                WeaponProjectile.IgnoreOwnerCollisions(this, this.Owner);
            }
        }

        protected override void OnCollisionEnter2D(Collision2D c)
        {
            base.OnCollisionEnter2D(c);

            // If we hit our first ground collision, set the end rotation
            if (this.isStopped && !this.isEndRotationSet)
            {
                this.isEndRotationSet = true;

                // Update our rotation to the final one
                Vector3 rot = this.RotateTransform.localEulerAngles;
                rot.z = this.EndRotation;
                this.RotateTransform.localEulerAngles = rot;
            }
        }
    }
}
