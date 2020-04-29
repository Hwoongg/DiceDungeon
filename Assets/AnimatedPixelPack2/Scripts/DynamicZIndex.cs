using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class DynamicZIndex : MonoBehaviour
    {
        // Members
        private Vector3 pos;

        void Update()
        {
            // Move the z position so that it matches the y level.
            // This is so sprites will be ordered correctly on a 2d survival type camera.
            this.pos = this.transform.position;
            this.pos.z = this.transform.position.y;

            this.transform.position = this.pos;
        }
    }
}