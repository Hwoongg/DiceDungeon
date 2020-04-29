using UnityEngine;
using UnityEngine.Rendering;

namespace BlankSourceCode.AnimatedPixelPack2
{
    [RequireComponent(typeof(SortingGroup))]
    [ExecuteAlways]
    public class AnimatedSortingGroup : MonoBehaviour 
	{
        // Editor Properties
        public float AnimatedOrderInLayer;

        // Members
        private SortingGroup group;
        private float previous;

        private void Awake()
        {
            this.group = this.GetComponent<SortingGroup>();
        }

        private void Update()
        {
            if (this.AnimatedOrderInLayer != this.previous)
            {
                this.previous = this.AnimatedOrderInLayer;
                this.group.sortingOrder = Mathf.RoundToInt(this.AnimatedOrderInLayer);
            }
        }
    }
}