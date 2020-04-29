using UnityEngine;
using UnityEngine.UI;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class FloatingUIText : MonoBehaviour
    {
        // Editor Properties
        public float TimeToLive = 5;
        public Vector2 Velocity = new Vector2(1, 1);
        public float WaveDistance = 1;
        public float WorldScale = 0.01f;

        // Members
        private Text textComponent;
        private float lifeTime;
        private float startX;

        /// <summary>
        /// Instantiate a new instance of the floating text class using the supplied parameters
        /// </summary>
        /// <param name="prefab">The prefab to use as the base</param>
        /// <param name="canvas">The UI canvas to use for parenting</param>
        /// <param name="start">The start location</param>
        /// <param name="text">The text to display</param>
        /// <returns>The new floating text instance</returns>
        public static FloatingUIText Create(FloatingUIText prefab, Canvas canvas, Vector3 start, string text)
        {
            FloatingUIText ft = GameObject.Instantiate<FloatingUIText>(prefab);
            ft.transform.SetParent(canvas.transform, false);
            ft.transform.position = start;
            ft.textComponent = ft.GetComponent<Text>();
            ft.textComponent.text = text;

            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                ft.transform.localScale *= ft.WorldScale;
            }

            return ft;
        }

        void Awake()
        {
            this.textComponent = this.GetComponent<Text>();
        }

        void Start()
        {
            this.startX = this.transform.position.x;
        }

        void Update()
        {
            this.lifeTime += Time.deltaTime;
            float life = (this.lifeTime / this.TimeToLive);

            // Fade out the color
            Color color = this.textComponent.color;
            color.a = 1 - life;
            this.textComponent.color = color;

            // Move in a sine wave pattern
            float x = this.startX + Mathf.Sin(life * this.WaveDistance * Mathf.PI) * this.Velocity.x;
            float y = this.transform.position.y + (this.Velocity.y * Time.deltaTime);

            this.transform.position = new Vector3(x, y, this.transform.position.z);

            if (this.lifeTime >= this.TimeToLive)
            {
                // End of life
                Destroy(this.gameObject);
            }
        }
    }
}