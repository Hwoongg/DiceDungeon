using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class SpritesheetGenerator : MonoBehaviour
    {
        // Editor Properties
        [Header("SpriteSheet")]
        public int PixelsPerUnit = 16;
        public int FrameWidth = 32;
        public int FrameHeight = 32;
        public int FrameOffsetX = 0;
        public int FrameOffsetY = -8;

        [Header("Animations")]
        public RuntimeAnimatorController Controller;
        public Character[] Prefabs;

        // We need all these collections as Unity can't serialize a complex dictionary structure.
        // Hide them in the inspector since they need to be populated by the Editor script.
        [HideInInspector]
        public List<AnimationClip> clips = new List<AnimationClip>();
        [HideInInspector]
        public List<int> clipFrameStarts = new List<int>();
        [HideInInspector]
        public List<int> clipFrameCounts = new List<int>();
        [HideInInspector]
        public List<float> frameTimes = new List<float>();
        [HideInInspector]
        public List<string> outFolderPaths = new List<string>();

        // Members
        private Camera mainCamera;
        private RenderTexture renderTexture;
        private Rect sourceRect;

        void Awake()
        {
            // Setup the camera that will record each keyframe
            this.mainCamera = Camera.main;
            this.mainCamera.orthographicSize = (Screen.height / (float)this.PixelsPerUnit) * 0.5f;

            // Create the output texture so that it matches the screen meaning we will get 1:1 pixel ratio in our screenshots
            this.renderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            this.renderTexture.useMipMap = false;
            this.renderTexture.filterMode = FilterMode.Point;
            this.renderTexture.anisoLevel = 0;
            this.renderTexture.antiAliasing = 1;
            this.renderTexture.wrapMode = TextureWrapMode.Clamp;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear, 1.0f);
            RenderTexture.active = null;

            // Set the rectangle that we will actually capture for the spritesheet
            this.sourceRect = new Rect(
                Mathf.RoundToInt((Screen.width * 0.5f) - (this.FrameWidth * 0.5f) + this.FrameOffsetX),
                Mathf.RoundToInt((Screen.height * 0.5f) - (this.FrameHeight * 0.5f) + this.FrameOffsetY),
                this.FrameWidth,
                this.FrameHeight);
        }

        void Start()
        {
            // Ensure the user has clicked the Collect Frame Info button
            if (this.frameTimes.Count == 0 || this.frameTimes.Count < this.clips.Count ||
                this.clips.Count != this.clipFrameStarts.Count ||
                this.clips.Count != this.clipFrameCounts.Count ||
                this.Prefabs.Length != this.outFolderPaths.Count)
            {
                throw new System.ArgumentException("No valid frame info found, did you forget to click Collect Frame Info?");
            }

            // And added some prefabs to convert
            if (this.Prefabs.Length == 0)
            {
                throw new System.ArgumentException("No prefabs found, did you forget to add some?");
            }

            // Start generating the spritesheet
            StartCoroutine(this.CaptureAnimation());
        }

        private IEnumerator CaptureAnimation()
        {
            // Repeat for all the prefabs
            for (int prefabIndex = 0; prefabIndex < this.Prefabs.Length; prefabIndex++)
            {
                // Create the charater that we will capture the spritesheet for
                Character character = GameObject.Instantiate<Character>(this.Prefabs[prefabIndex]);
                character.transform.position = Vector3.zero;
                character.IgnoreAnimationStates = true;
                character.CastProjectiles = false;
                character.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                Destroy(character.GetComponent<CharacterInput>());
                yield return new WaitForEndOfFrame();

                // Get all the animation clips
                Animator animator = character.GetComponent<Animator>();
                var clips = this.clips;

                // Capture the spritesheet for each animation clip
                for (int clipIndex = 0; clipIndex < clips.Count; clipIndex++)
                {
                    var clip = clips[clipIndex];

                    // Make the charater only use the current clip by overriding the state machine
                    AnimatorOverrideController aoc = new AnimatorOverrideController(this.Controller);
                    var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                    foreach (var a in aoc.animationClips)
                    {
                        anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, clip));
                    }
                    aoc.ApplyOverrides(anims);
                    animator.runtimeAnimatorController = aoc;

                    // Find the frame info for this clip
                    int x = 0;
                    int y = 0;
                    int clipToFrameIndex = this.clips.IndexOf(clip);
                    if (clipToFrameIndex == -1)
                    {
                        // No frame info, so skip the clip
                        continue;
                    }
                    int start = this.clipFrameStarts[clipToFrameIndex];
                    int count = this.clipFrameCounts[clipToFrameIndex];


                    // Generate the output texture
                    Texture2D spriteSheetTexture = new Texture2D(count * this.FrameWidth, this.FrameHeight, TextureFormat.ARGB32, false, false);

                    // Loop through each keyframe in the animation
                    this.mainCamera.targetTexture = this.renderTexture;
                    for (int i = start; i < start + count; i++)
                    {
                        x = (i - start) * this.FrameWidth;

                        // Move to the current keyframe
                        animator.speed = 0;
                        animator.Play("Default", 0, Mathf.Clamp01(this.frameTimes[i] / clip.length));
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForSeconds(0.1f);

                        CaptureFrame(x, y, spriteSheetTexture);
                    }

                    yield return new WaitForEndOfFrame();
                    yield return new WaitForSeconds(0.1f);

                    // Write out this spritesheet
                    string fileName = string.Format(
                        "{0}_{1}.png",
                        character.name.Replace(" Variant", "").Replace("(Clone)", ""),
                        clip.name.Replace("Humanoid_", ""));

                    string scenePath = this.outFolderPaths[prefabIndex];
                    scenePath = System.IO.Path.GetDirectoryName(scenePath);
                    string saveFilePath = System.IO.Path.Combine(scenePath, fileName);

                    byte[] textureBytes = spriteSheetTexture.EncodeToPNG();
                    System.IO.File.WriteAllBytes(saveFilePath, textureBytes);

                    yield return new WaitForEndOfFrame();
                }

                // Remove the character ready for the next prefab
                DestroyImmediate(character.gameObject);
                yield return new WaitForEndOfFrame();
            }
        }

        private void CaptureFrame(int x, int y, Texture2D spriteSheetTexture)
        {
            // Capture the frame into the rendertexture
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear, 1.0f);
            RenderTexture.active = null;
            this.mainCamera.Render();

            // Copy the rendertexture into the output spritesheet at the correct location
            RenderTexture.active = this.renderTexture;
            spriteSheetTexture.ReadPixels(this.sourceRect, x, y);
            RenderTexture.active = null;
        }
    }
}
