using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class SnappingCursor : MonoBehaviour
    {
        // Members
        private Vector2 pos = new Vector2();

        void Start()
        {
            // Hide the real cursor
            Cursor.visible = false;
        }

        void Update()
        {
            // Move the cursor to the position under the mouse, but snapped to whole units
            this.pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.pos.x = Mathf.Round(this.pos.x * 2) / 2f;
            this.pos.y = Mathf.Round(this.pos.y * 2) / 2f;
            this.transform.position = this.pos;
        }
    }
}
