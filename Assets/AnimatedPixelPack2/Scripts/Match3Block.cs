using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class Match3Block : MonoBehaviour, IPointerClickHandler
    {
        // Editor Properties
        public SpriteRenderer Selection;

        // Script Properties
        public int BoardX { get; private set; }
        public int BoardY { get; private set; }
        public bool IsEmpty
        {
            get { return this.spriteRenderer.sprite == null; }
        }
        private static Match3Block currentSelection;
        private static bool isPerformingSwap;
        private Match3Block otherSwappingBlock;
        private Vector3 swapPosition1;
        private Vector3 swapPosition2;
        private float swapTime;
        private float swapDuration = 2;
        private float swapSpeed = 5;

        // Members
        private SpriteRenderer spriteRenderer;
        private bool isSelected;

        public static Match3Block Create(Match3Block prefab, int x, int y, Vector3 position)
        {
            // Create a new block and setup the params
            Match3Block block = GameObject.Instantiate<Match3Block>(prefab, position, prefab.transform.rotation);
            block.BoardX = x;
            block.BoardY = y;
            return block;
        }

        void Awake()
        {
            // Get the components
            this.spriteRenderer = this.GetComponent<SpriteRenderer>();

            // Make sure nothing is selected yet
            this.Unselect();
        }

        void Update()
        {
            if (this.otherSwappingBlock != null)
            {
                this.swapTime += Time.deltaTime * this.swapSpeed;
                this.transform.position = Vector3.Lerp(this.swapPosition1, this.swapPosition2, this.swapTime / this.swapDuration);
                this.otherSwappingBlock.transform.position = Vector3.Lerp(this.swapPosition2, this.swapPosition1, this.swapTime / this.swapDuration);

                if (this.swapTime >= this.swapDuration)
                {
                    this.EndSwap();
                }
            }
        }

        public bool IsMatchTo(Match3Block other)
        {
            // Do the blocks have the same sprite?
            return !this.IsEmpty && other.spriteRenderer.sprite == this.spriteRenderer.sprite;
        }

        public bool IsNextTo(Match3Block block)
        {
            // Check up, down, left, and right
            if ((this.BoardX == block.BoardX && (this.BoardY == block.BoardY - 1 || this.BoardY == block.BoardY + 1)) ||
                (this.BoardY == block.BoardY && (this.BoardX == block.BoardX - 1 || this.BoardX == block.BoardX + 1)))
            {
                return true;
            }

            return false;
        }

        public void SetSprite(Sprite newSprite)
        {
            // Update the sprite
            this.spriteRenderer.sprite = newSprite;
        }

        public void MoveSpriteFrom(Match3Block source)
        {
            // Cut and paste the sprite
            this.spriteRenderer.sprite = source.spriteRenderer.sprite;
            source.spriteRenderer.sprite = null;
        }

        public void Clear()
        {
            this.spriteRenderer.sprite = null;
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            // Ignore clicks on empty blocks, and when the board is busy
            if (this.IsEmpty || DemoMatch3.Instance.IsRunningBoardChange || Match3Block.isPerformingSwap)
            {
                return;
            }

            if (this.isSelected)
            {
                // Unselect a selected block
                this.Unselect();
            }
            else
            {
                if (Match3Block.currentSelection == null)
                {
                    // Select this block if it is the first one
                    this.Select();
                }
                else
                {
                    if (this.IsNextTo(Match3Block.currentSelection))
                    {
                        // Swap adjacent blocks
                        this.BeginSwap(Match3Block.currentSelection);
                        Match3Block.currentSelection.Unselect();
                    }
                    else
                    {
                        // Move selection to be the block that was just clicked
                        Match3Block.currentSelection.Unselect();
                        this.Select();
                    }
                }
            }
        }

        private void BeginSwap(Match3Block other)
        {
            // Ignore matching blocks since a swap would do nothing
            if (this.IsMatchTo(other))
            {
                return;
            }

            Match3Block.isPerformingSwap = true;

            this.otherSwappingBlock = other;
            this.swapTime = 0;
            this.swapPosition1 = this.transform.position;
            this.swapPosition2 = other.transform.position;

        }

        private void EndSwap()
        {
            Match3Block.isPerformingSwap = false;

            Match3Block other = this.otherSwappingBlock;
            this.otherSwappingBlock = null;

            // Swap the sprites
            Sprite temp = other.spriteRenderer.sprite;
            other.spriteRenderer.sprite = this.spriteRenderer.sprite;
            this.spriteRenderer.sprite = temp;

            this.transform.position = this.swapPosition1;
            other.transform.position = this.swapPosition2;

            // Look for 3's in a row
            DemoMatch3.Instance.StartFindingMatchesForSwap(this, other);
        }

        private void Select()
        {
            // Show the selection ui
            this.isSelected = true;
            Match3Block.currentSelection = this;
            this.Selection.gameObject.SetActive(true);
        }

        private void Unselect()
        {
            // Hide the selection ui
            this.isSelected = false;
            Match3Block.currentSelection = null;
            this.Selection.gameObject.SetActive(false);
        }
    }
}
