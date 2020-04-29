using System.Collections;
using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class SurvivalResource : MonoBehaviour
    {
        // Editor Properties
        public int MaxLife = 10;
        public SurvivalCollectable CollectablePrefab;
        public int AmountToSpawn = 3;

        // Members
        private int life;
        private Animator animator;
        private Animator cameraAnimator;

        void Awake()
        {
            // Setup the variables
            this.life = this.MaxLife;
            this.animator = this.GetComponent<Animator>();
            this.cameraAnimator = Camera.main.GetComponent<Animator>();
        }

        public void ApplyDamage(int damage)
        {
            // Apply the damage to the life
            this.life = Mathf.Clamp(this.life - damage, 0, this.MaxLife);
            if (this.life <= 0)
            {
                // Kill the tree
                this.animator.Play("Die", 0);
                StartCoroutine(this.DestroyAfter(0.25f, this.gameObject));

                // Spawn some logs
                for (int i = 0; i <this.AmountToSpawn; i++)
                {
                    var c = GameObject.Instantiate<SurvivalCollectable>(this.CollectablePrefab);
                    var pos = (Vector2)this.transform.position;
                    pos += Random.insideUnitCircle;
                    c.Destination = pos;
                    c.transform.position = this.transform.position;
                }
            }
            else
            {
                // Just play the hurt animation
                this.animator.Play("Damage", 0);
            }

            // Always shake the camera a bit
            this.cameraAnimator.Play("Shake", 0);
        }

        private IEnumerator DestroyAfter(float seconds, GameObject gameObject)
        {
            yield return new WaitForSeconds(seconds);

            GameObject.Destroy(gameObject);
        }
    }
}
