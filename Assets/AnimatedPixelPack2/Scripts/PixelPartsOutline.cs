using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    [ExecuteInEditMode]
    public class PixelPartsOutline : MonoBehaviour
    {
        // Editor Properties
        [Header("Display")]
        [Tooltip("Should we display the character outline sprites")]
        public bool UseOutline = false;
        [Tooltip("The outline color")]
        public Color OutlineTint = Color.black;

        // Members
        private bool lastUseOutline;
        private Color lastOutlineColor;
        private SpriteRenderer[] outlines;

        void Awake()
        {
            // Setup the outline
            this.lastOutlineColor = Color.white;
            SpriteRenderer[] renderers = this.GetComponentsInChildren<SpriteRenderer>(true);
            List<SpriteRenderer> foundOutlines = new List<SpriteRenderer>();
            foreach (var r in renderers)
            {
                if (r.gameObject.name.EndsWith("Sprite-Outline"))
                {
                    foundOutlines.Add(r);
                    r.gameObject.SetActive(this.UseOutline);
                }
            }
            this.outlines = foundOutlines.ToArray();
        }
        
        void Update()
        {
            // Update the outline color if it has changed
            if (this.lastUseOutline != this.UseOutline || this.lastOutlineColor != this.OutlineTint)
            {
                foreach (var o in this.outlines)
                {
                    o.color = this.OutlineTint;
                    o.gameObject.SetActive(this.UseOutline);
                }
                this.lastOutlineColor = this.OutlineTint;
                this.lastUseOutline = this.UseOutline;
            }
        }
    }
}
