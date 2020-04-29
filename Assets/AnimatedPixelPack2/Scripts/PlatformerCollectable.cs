using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class PlatformerCollectable : MonoBehaviour
    {
        // Editor Properties
        public FloatingUIText CollectTextPrefab;
        public float FloatSpeed = 0.5f;
        public float FloatDistance = 0.5f;
        public int ScoreValue = 10;

        // Script Properties
        public Vector2 Destination;

        // Members
        private Vector3 pos;
        private Vector3 startPosition;
        private float moveTime;
        private static Canvas uiCanvas;

        void Awake()
        {
            this.startPosition = this.transform.position;
            this.pos = this.transform.position;
        }

        void Update()
        {
            this.moveTime += Time.deltaTime * this.FloatSpeed;
            float y = Mathf.Sin(moveTime * Mathf.PI) * this.FloatDistance;
            this.pos.y = this.startPosition.y + y;
            this.transform.localPosition = pos;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Just destroy the object when collected to give you an idea of what sort of features you could add to a game like this.
            Character c = other.GetComponent<Character>();
            if (c != null)
            {
                // Find the canvas the first time we need it and cache it for all future uses
                if (PlatformerCollectable.uiCanvas == null)
                {
                    PlatformerCollectable.uiCanvas = FindObjectOfType<Canvas>();
                }

                // Add some floating text to show the item name
                Vector3 titlePosition = c.transform.position + (Vector3.up * 1.5f);
                string text = "+" + this.ScoreValue;
                var ft = FloatingUIText.Create(this.CollectTextPrefab, PlatformerCollectable.uiCanvas, titlePosition, text);
                ft.WaveDistance = 0;

                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
