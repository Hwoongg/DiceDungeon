using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class SurvivalCollectable : MonoBehaviour
    {
        // Editor Properties
        public FloatingUIText CollectTextPrefab;

        // Script Properties
        public Vector2 Destination;

        // Members
        private Vector3 pos;
        private static Canvas uiCanvas;

        void Awake()
        {
            // Find the world canvas the first time we need it and cache it for all future uses
            if (SurvivalCollectable.uiCanvas == null)
            {
                var canvases = FindObjectsOfType<Canvas>();
                foreach (var c in canvases)
                {
                    if (c.renderMode == RenderMode.WorldSpace)
                    {
                        SurvivalCollectable.uiCanvas = c;
                        break;
                    }
                }
            }
        }

        void Update()
        {
            // Move the logs a bit, ideally this would use a nicer effect
            float z = this.transform.position.z;
            this.pos = Vector2.Lerp(this.transform.position, this.Destination, Time.deltaTime * 5);
            this.pos.z = z;
            this.transform.position = pos;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Just destroy the object when collected to give you an idea of what sort of features you could add to a game like this.
            Character c = other.GetComponent<Character>();
            if (c != null)
            {
                // For screen overlay camera's we need to calculate the new posision
                Vector3 titlePosition = c.transform.position + (Vector3.up * 1.5f);
                if (SurvivalCollectable.uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    titlePosition = Camera.main.WorldToScreenPoint(titlePosition);
                }

                // Add some floating text to show the item name
                string text = "Log x1";
                var ft = FloatingUIText.Create(this.CollectTextPrefab, SurvivalCollectable.uiCanvas, titlePosition, text);
                ft.WaveDistance = 0;

                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
