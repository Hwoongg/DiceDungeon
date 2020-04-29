using System.Collections;
using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class TemporaryFX : MonoBehaviour
    {
        // Editor Properties
        public float LifeTime = 5;

        void Start()
        {
            StartCoroutine(this.DestroyAfter(this.LifeTime));
        }

        private IEnumerator DestroyAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            GameObject.Destroy(this.gameObject);
        }
    }
}
