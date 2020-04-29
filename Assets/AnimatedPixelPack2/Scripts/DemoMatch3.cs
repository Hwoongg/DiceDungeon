using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class DemoMatch3 : MonoBehaviour
    {
        // Editor Properties
        public List<Sprite> BlockSprites = new List<Sprite>();
        public Match3Block BlockPrefab;
        public int Width;
        public int Height;
        public Transform BoardPosition;
        public Transform EnemyPosition;
        public Text GameOverText;

        // Script Properties
        public static DemoMatch3 Instance { get; private set; }
        public bool IsRunningBoardChange { get; private set; }

        // Members
        private Match3Block[,] board;
        private DemoController controller;
        private Animator animator;
        private Rigidbody2D body;
        private Character player;
        private Character enemy;
        private int turnCount;

        void Awake()
        {
            // Set the singleton instance
            DemoMatch3.Instance = this;

            // Hide the gameover text
            this.GameOverText.enabled = false;

            // Hook up the event so we know when the player changes
            this.controller = this.GetComponent<DemoController>();
            this.controller.OnSelectedCharacterChanged += this.Controller_OnSelectedCharacterChanged;
        }

        void Start()
        {
            // Create a new board to store the blocks
            this.board = new Match3Block[this.Width, this.Height];

            // Calculate the start position to center the board
            Vector2 offset = BlockPrefab.GetComponent<SpriteRenderer>().bounds.size * 0.9f;
            float startX = this.BoardPosition.position.x - offset.x * this.Width * 0.5f + offset.x * 0.5f;
            float startY = this.BoardPosition.position.y - offset.y * this.Height * 0.5f + offset.y * 0.5f;

            Sprite[] previousLeft = new Sprite[this.Height];
            Sprite previousBelow = null;

            // Add each block making sure they don't immediately match
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    // Create the block
                    Vector3 position = new Vector3(startX + (offset.x * x), startY + (offset.y * y), 0);
                    Match3Block block = Match3Block.Create(this.BlockPrefab, x, y, position);

                    // Start out with all sprites available and remove matching ones
                    List<Sprite> possibleBlocks = new List<Sprite>();
                    possibleBlocks.AddRange(this.BlockSprites);
                    possibleBlocks.Remove(previousLeft[y]);
                    possibleBlocks.Remove(previousBelow);

                    // Set the new sprite
                    Sprite picked = possibleBlocks[Random.Range(0, possibleBlocks.Count)];
                    block.SetSprite(picked);

                    previousLeft[y] = picked;
                    previousBelow = picked;

                    // Store it in our board
                    this.board[x, y] = block;
                }
            }

            // Add an enemy
            this.SpawnEnemy();
        }

        /// <summary>
        /// Return the block at the specified position
        /// </summary>
        /// <param name="x">The x coord on the board</param>
        /// <param name="y">The y coord on the board</param>
        /// <returns>The block at that position</returns>
        public Match3Block GetBlockAt(int x, int y)
        {
            return this.board[x, y];
        }

        /// <summary>
        /// Begin the mechanism for find matches and updating the board
        /// </summary>
        /// <param name="start">One block that was just moved</param>
        /// <param name="other">The other block that was moved</param>
        public void StartFindingMatchesForSwap(Match3Block start, Match3Block other)
        {
            // Set the flag to prevent any other swaps from taking place until we are done
            this.IsRunningBoardChange = true;

            // Find matches with the first block
            List<Match3Block> found = new List<Match3Block>();
            int xCount;
            int yCount;
            this.FindMatchesFrom(start, ref found, out xCount, out yCount);
            if (xCount >= 2 || yCount >= 2)
            {
                found.Add(start);
                this.ClearMatches(found);
            }

            // Find matches with the second block
            found.Clear();
            this.FindMatchesFrom(other, ref found, out xCount, out yCount);
            if (xCount >= 2 || yCount >= 2)
            {
                found.Add(other);
                this.ClearMatches(found);
            }

            // Fill up the board by dropping blocks from the top
            int count = this.RefillBoard();
            if (count > 0)
            {
                this.Animate(this.player);
            }
            else
            {
                turnCount++;
                if (turnCount > 1)
                {
                    turnCount = 0;
                    this.Animate(this.enemy);
                }
            }

            // Clear the flag to allow swaps again
            this.IsRunningBoardChange = false;
        }

        private void FindMatchesFrom(Match3Block startBlock, ref List<Match3Block> found, out int xCount, out int yCount)
        {
            xCount = 0;
            yCount = 0;

            // Ignore blocks with no sprite
            if (startBlock.IsEmpty)
            {
                return;
            }

            // Loop through left and right looking for matches, quit when we reach the edge or a non-matching block
            for (int x = startBlock.BoardX - 1; x >= 0; x--)
            {
                if (!CheckPositionMatch(startBlock, x, startBlock.BoardY, ref xCount, ref found))
                {
                    break;
                }
            }
            for (int x = startBlock.BoardX + 1; x < this.Width; x++)
            {
                if (!CheckPositionMatch(startBlock, x, startBlock.BoardY, ref xCount, ref found))
                {
                    break;
                }
            }

            // Loop through up and down looking for matches, quit when we reach the edge or a non-matching block
            for (int y = startBlock.BoardY - 1; y >= 0; y--)
            {
                if (!CheckPositionMatch(startBlock, startBlock.BoardX, y, ref yCount, ref found))
                {
                    break;
                }
            }
            for (int y = startBlock.BoardY + 1; y < this.Height; y++)
            {
                if (!CheckPositionMatch(startBlock, startBlock.BoardX, y, ref yCount, ref found))
                {
                    break;
                }
            }
        }

        private bool CheckPositionMatch(Match3Block toCheck, int x, int y, ref int count, ref List<Match3Block> found)
        {
            // If they match add it to the list and return true
            var other = DemoMatch3.Instance.GetBlockAt(x, y);
            if (toCheck.IsMatchTo(other))
            {
                found.Add(other);
                count++;
                return true;
            }

            // Not a match
            return false;
        }

        private void ClearMatches(List<Match3Block> found)
        {
            // Just set each block's sprite to null
            foreach (var block in found)
            {
                block.Clear();
            }
        }

        private int RefillBoard()
        {
            int newBlockCount = 0;

            // Loop through each column
            for (int x = 0; x < this.Width; x++)
            {
                // Now loop through each row and move sprites into the first empty slot
                int emptyY = -1;
                for (int y = 0; y < this.Height; y++)
                {
                    if (emptyY < 0 && this.board[x, y].IsEmpty)
                    {
                        emptyY = y;
                    }
                    else if (emptyY >= 0 && !this.board[x, y].IsEmpty)
                    {
                        this.board[x, emptyY].MoveSpriteFrom(this.board[x, y]);
                        emptyY++;
                    }
                }

                // Loop one more time and fill in any still missing blocks with new sprites
                for (int y = this.Height - 1; y >= 0; y--)
                {
                    if (this.board[x, y].IsEmpty)
                    {
                        List<Sprite> possibleBlocks = new List<Sprite>();
                        possibleBlocks.AddRange(this.BlockSprites);

                        if (x > 0)
                        {
                            possibleBlocks.Remove(this.board[x - 1, y].GetComponent<SpriteRenderer>().sprite);
                        }
                        if (x < this.Width - 1)
                        {
                            possibleBlocks.Remove(this.board[x + 1, y].GetComponent<SpriteRenderer>().sprite);
                        }
                        if (y > 0)
                        {
                            possibleBlocks.Remove(this.board[x, y - 1].GetComponent<SpriteRenderer>().sprite);
                        }

                        this.board[x, y].SetSprite(possibleBlocks[Random.Range(0, possibleBlocks.Count)]);
                        newBlockCount++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return newBlockCount;
        }

        private void Controller_OnSelectedCharacterChanged(Character newCharacter)
        {
            // Set the new player
            this.player = newCharacter;
            this.animator = this.player.GetComponent<Animator>();
            this.body = this.player.GetComponent<Rigidbody2D>();

            // Get rid of the default input since we control it from here in this demo
            Destroy(this.player.GetComponent<CharacterInput>());

            // Remove the other platformer pieces
            this.player.IgnoreAnimationStates = true;
            this.body.bodyType = RigidbodyType2D.Kinematic;
            this.body.gravityScale = 0;
            this.animator.SetInteger("WeaponType", (int)this.player.EquippedWeaponType);
        }

        private void SpawnEnemy()
        {
            var enemy = GameObject.Instantiate<Character>(this.controller.Characters[Random.Range(0, this.controller.Characters.Length)], this.EnemyPosition.position, Quaternion.identity, null);

            // Get rid of the default input since we control it from here in this demo
            Destroy(enemy.GetComponent<CharacterInput>());

            // Remove the other platformer pieces
            enemy.IgnoreAnimationStates = true;
            enemy.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            enemy.GetComponent<Animator>().SetInteger("WeaponType", (int)enemy.EquippedWeaponType);
            enemy.ForceDirection(Character.Direction.Left);

            this.enemy = enemy;
        }

        private void Animate(Character c)
        {
            // Pick the correct animation action and play it
            Character.Action actionFlags = Character.Action.ThromMain;
            switch (c.EquippedWeaponType)
            {
                case Character.WeaponType.None:
                case Character.WeaponType.Bow:
                case Character.WeaponType.Gun:
                    actionFlags = Character.Action.Attack;
                    break;
            }
            c.Perform(actionFlags);

            // Wait for the animation to finish and see if the game is over,
            // Ideally we would have a callback on the character for when they died instead of polling like this.
            StartCoroutine(this.CheckGameState(3));
        }

        private IEnumerator CheckGameState(float afterSeconds)
        {
            yield return new WaitForSeconds(afterSeconds);

            // If the player is dead it is gameover
            if (this.player == null)
            {
                this.GameOverText.enabled = true;
                this.IsRunningBoardChange = true;
            }

            // Spawn a new enemy if needed
            if (this.enemy == null)
            {
                this.SpawnEnemy();
            }
        }
    }
}
