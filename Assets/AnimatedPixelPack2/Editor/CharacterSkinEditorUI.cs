using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    [CustomEditor(typeof(Character), true)]
    [CanEditMultipleObjects]
    public class CharacterSkinEditorUI : Editor
    {
        // Editor Properties
        public Texture2D Skin;

        // Members
        private struct CharacterSprite
        {
            public Sprite Skin;
            public Sprite Outline;
        }
        private struct SkinPart
        {
            public int TextureIndex;
            public int OrderInLayer;
        }
        private static Dictionary<string, SkinPart> skinParts = new Dictionary<string, SkinPart>
        {
            {"Head", new SkinPart{TextureIndex = 0, OrderInLayer = -4 } },
            {"Helm", new SkinPart{TextureIndex = 1, OrderInLayer = 0 } },
            {"Body", new SkinPart{TextureIndex = 2, OrderInLayer = 0 } },
            {"Legs", new SkinPart{TextureIndex = 3, OrderInLayer = 0 } },
            {"BodyPantsMask", new SkinPart{TextureIndex = 3, OrderInLayer = 0 } },
            {"HeadBack", new SkinPart{TextureIndex = 4, OrderInLayer = -4 } },
            {"HelmBack", new SkinPart{TextureIndex = 5, OrderInLayer = 0 } },
            {"BodyBack", new SkinPart{TextureIndex = 6, OrderInLayer = 2 } },
            {"ElbowOff", new SkinPart{TextureIndex = 7, OrderInLayer = 0 } },
            {"HandOff", new SkinPart{TextureIndex = 8, OrderInLayer = 0 } },
            {"HandMain", new SkinPart{TextureIndex = 8, OrderInLayer = -8 } },
            {"FootOff", new SkinPart{TextureIndex = 9, OrderInLayer = -10 } },
            {"FootMain", new SkinPart{TextureIndex = 9, OrderInLayer = -10 } },
            {"ShoulderOff", new SkinPart{TextureIndex = 10, OrderInLayer = -2 } },
            {"ShoulderMain", new SkinPart{TextureIndex = 11, OrderInLayer = -10 } },
            {"HeldItemMain", new SkinPart{TextureIndex = 12, OrderInLayer = -6 } },
            {"HeldItemMainCollider", new SkinPart{TextureIndex = 12, OrderInLayer = -6 } },
            {"HeldItemOff", new SkinPart{TextureIndex = 13, OrderInLayer = 6 } },
            {"HeldItemMainFX", new SkinPart{TextureIndex = 14, OrderInLayer = -5 } },
            {"HeadDead", new SkinPart{TextureIndex = 15, OrderInLayer = -3 } }
        };

        private static GetWidthAndHeight getWidthAndHeightDelegate;
        private delegate void GetWidthAndHeight(TextureImporter importer, ref int width, ref int height);
        private string[] sortingLayerNames;

        public override void OnInspectorGUI()
        {
            Character c = (Character)target;

            GUI.changed = false;

            DrawDefaultInspector();

            EditorGUILayout.Separator();

            if (c.GetComponentInChildren<SpriteRenderer>() != null && PrefabUtility.GetCorrespondingObjectFromSource(c) != null)
            {
                EditorGUILayout.LabelField("Skin Editor", EditorStyles.boldLabel);
                this.Skin = (Texture2D)EditorGUILayout.ObjectField("Skin", this.Skin, typeof(Texture2D), false, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));

                // Add the apply skin button that will update all the sprites on the character with the skin
                if (GUILayout.Button("Apply Skin"))
                {
                    this.ApplySkin(c, this.Skin, 0);
                }

                // Add the button that updates the weapon collision shape with that of the current main hand weapon sprite
                if (GUILayout.Button("Apply Weapon Shape"))
                {
                    this.ApplyWeaponShape(c);
                }

                EditorGUILayout.Separator();

                // Add a button that will regenerate all outlines from the skin textures
                if (GUILayout.Button("Regenerate All Outlines"))
                {
                    this.GenerateAllOutlines(c);
                }

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(c);
                }
            }
        }

        public void ApplySkin(Character c, Texture2D skin, int characterOrderIndex)
        {
            if (skin == null)
            {
                return;
            }

            // Create the outline renderers
            this.EnsureOutlineRenderers(c, skin, characterOrderIndex);

            // Create the outline image
            Texture2D outline = this.GenerateOutline(skin);

            // Apply the sprites to the renderers
            this.SetSprites(c, skin, outline, characterOrderIndex);
        }

        private void EnsureOutlineRenderers(Character c, Texture2D skin, int characterOrderIndex)
        {
            // Loop through all the sprite renderers
            SpriteRenderer[] renderers = c.GetComponentsInChildren<SpriteRenderer>(/*includeInactive=*/ true);
            for (int i = 0; i < renderers.Length; i++)
            {
                // Check to see if this is a non-outline one
                SpriteRenderer sr = renderers[i];
                GameObject go = sr.gameObject;
                if (!go.name.EndsWith("-Outline"))
                {
                    // For non-outlines, search the children to see if there is already a matching outline
                    SpriteRenderer outline = null;
                    SpriteRenderer[] children = go.GetComponentsInChildren<SpriteRenderer>(/*includeInactive=*/ true);
                    foreach (var child in children)
                    {
                        if (child.gameObject.GetInstanceID() != go.GetInstanceID() &&
                            child.gameObject.name == go.name + "-Outline")
                        {
                            outline = child;
                            break;
                        }
                    }

                    // If we don't find a matching outline, create one
                    if (outline == null)
                    {
                        GameObject newGO = new GameObject(go.name + "-Outline");
                        newGO.transform.SetParent(go.transform);
                        outline = newGO.AddComponent<SpriteRenderer>();
                    }

                    outline.transform.localPosition = Vector3.zero;
                    outline.transform.localScale = Vector3.one;
                }
            }
        }

        private void SetSprites(Character c, Texture2D skin, Texture2D outline, int characterOrderIndex)
        {
            // Grab all the sprites out of this texture
            Sprite[] skinSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(skin)).OfType<Sprite>().ToArray();
            Sprite[] outlineSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(outline)).OfType<Sprite>().ToArray();

            Dictionary<int, CharacterSprite> spritesAtFrame = new Dictionary<int, CharacterSprite>();
            for (int i = 0; i < skinSprites.Length; i++)
            {
                Sprite s = skinSprites[i];

                int textureWidth = s.texture.width;
                int textureHeight = s.texture.height;

                // In case we are using a texture atlas, find the real original texture size
                if (s.rect != s.textureRect)
                {
                    if (getWidthAndHeightDelegate == null)
                    {
                        var method = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                        getWidthAndHeightDelegate = Delegate.CreateDelegate(typeof(GetWidthAndHeight), null, method) as GetWidthAndHeight;
                    }

                    string path = AssetDatabase.GetAssetPath(s);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null)
                    {
                        getWidthAndHeightDelegate(importer, ref textureWidth, ref textureHeight);
                    }
                }

                // Set the sprite information
                int x = (int)(s.rect.x / s.rect.height);
                int y = (int)((textureHeight - (s.rect.y + s.rect.height)) / s.rect.width);
                int index = (int)(y * (textureWidth / s.rect.width)) + x;
                spritesAtFrame[index] = new CharacterSprite { Skin = s, Outline = outlineSprites[i] };
            }

            bool hasWeapon = false;
            bool hasShield = false;

            // Loop through all the transforms so we don't miss a disabled sprite renderer component
            Transform[] objects = c.GetComponentsInChildren<Transform>(/*includeInactive=*/ true);
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject go = objects[i].gameObject;

                // Determine the sprite type based on the name
                bool isSkin = true;
                string partName = go.name;
                if (go.name.Contains("Sprite"))
                {
                    isSkin = !go.name.EndsWith("Sprite-Outline");
                    if (isSkin)
                    {
                        partName = go.name.Replace("Sprite", "");
                    }
                    else
                    {
                        partName = go.name.Replace("Sprite-Outline", "");
                    }
                }
                else if (go.name.EndsWith("Mask"))
                {
                    partName = go.name;
                    isSkin = true;
                }
                else
                {
                    continue;
                }

                // Update the sprite with the correct one from the skin texture
                if (skinParts.ContainsKey(partName))
                {
                    SkinPart part = skinParts[partName];
                    Renderer r = go.GetComponent<Renderer>();

                    int index = part.TextureIndex;
                    if (spritesAtFrame.ContainsKey(index))
                    {
                        // Set the correct sprite
                        Sprite sprite = (isSkin ? spritesAtFrame[index].Skin : spritesAtFrame[index].Outline);
                        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                        if (sr == null)
                        {
                            SpriteMask sm = go.GetComponent<SpriteMask>();
                            sm.sprite = sprite;
                            r.enabled = false;
                        }
                        else
                        {
                            sr.sprite = sprite;
                            r.enabled = true;
                        }

                        // Hide the special sprites that get turned on only in animations
                        if (go.name.Contains("Back") || go.name.Contains("Dead") || go.name.Contains("FX"))
                        {
                            r.gameObject.SetActive(false);
                        }

                        // Update the outline sprite order
                        if (!isSkin)
                        {
                            r.sortingOrder = -100;
                        }
                    }
                    else
                    {
                        // Hide this renderer
                        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                        if (sr == null)
                        {
                            SpriteMask sm = go.GetComponent<SpriteMask>();
                            sm.sprite = null;
                        }
                        else
                        {
                            sr.sprite = null;
                        }
                        r.enabled = false;
                    }

                    // Create the weapon collider shape if we find one
                    if (partName == "HeldItemMainCollider" && isSkin)
                    {
                        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                        if (sr.sprite != null)
                        {
                            hasWeapon = true;
                        }

                        PolygonCollider2D pc = go.GetComponent<PolygonCollider2D>();
                        if (pc != null)
                        {
                            if (sr.sprite != null)
                            {
                                // Creating a new component will auto set the collider to the shape of the sprite
                                PolygonCollider2D newShape = sr.gameObject.AddComponent<PolygonCollider2D>();

                                // Copy the points to the original collider so we don't make a new component and mess up prefab hierarchy
                                pc.SetPath(0, newShape.points);
                                pc.enabled = true;
                                pc.isTrigger = true;
                                sr.enabled = false;
                                c.WeaponObject = pc;
                                DestroyImmediate(newShape);
                            }
                            else
                            {
                                pc.enabled = false;
                            }
                        }
                    }
                }
            }

            // Change the character if we don't find a weapon sprite
            if (!hasWeapon)
            {
                c.EquippedWeaponType = Character.WeaponType.None;
                c.ThrowMainProjectile = null;
            }

            // Update based on the shield
            c.IsBlockEnabled = hasShield;

            // Set the tag
            c.gameObject.tag = "Player";
        }

        private void ApplyWeaponShape(Character c)
        {
            bool hasWeapon = false;
            bool hasShield = false;

            // Find the weapon sprites
            SpriteRenderer[] renderers = c.GetComponentsInChildren<SpriteRenderer>(/*includeInactive=*/ true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                GameObject go = sr.gameObject;
                if (go.name == "HeldItemMainCollider")
                {
                    if (sr.sprite != null)
                    {
                        if (go.name == "HeldItemMainCollider")
                        {
                            hasWeapon = true;
                        }
                    }

                    PolygonCollider2D pc = go.GetComponent<PolygonCollider2D>();
                    if (pc != null)
                    {
                        if (sr.sprite != null)
                        {
                            // Creating a new component will auto set the collider to the shape of the sprite
                            PolygonCollider2D newShape = sr.gameObject.AddComponent<PolygonCollider2D>();

                            // Copy the points to the original collider so we don't make a new component and mess up prefab hierarchy
                            pc.SetPath(0, newShape.points);
                            pc.enabled = true;
                            pc.isTrigger = true;
                            sr.enabled = false;
                            c.WeaponObject = pc;
                            DestroyImmediate(newShape);

                        }
                        else
                        {
                            pc.enabled = false;
                        }
                    }
                }
            }

            // Change the character if we don't find a weapon sprite
            if (!hasWeapon)
            {
                c.EquippedWeaponType = Character.WeaponType.None;
                c.ThrowMainProjectile = null;
            }

            // Update based on the shield
            c.IsBlockEnabled = hasShield;
        }


        private void GenerateAllOutlines(Character c)
        {
            // Find the character skins path
            SpriteRenderer sr = c.GetComponentInChildren<SpriteRenderer>();
            string texturePath = AssetDatabase.GetAssetPath(sr.sprite.texture);

            string prefix = Application.dataPath.Substring(0, Application.dataPath.Length - 6);

            string path = prefix + texturePath;
            path = Path.GetDirectoryName(path.Replace("/", "\\"));
            string[] spriteFiles = Directory.GetFiles(path, "*.png");

            // Generate an outline for each skin
            for (int i = 0; i < spriteFiles.Length; i++)
            {
                string file = spriteFiles[i].Replace("\\", "/").Replace(prefix, "");
                Texture2D skin = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                GenerateOutline(skin);
            }
        }

        private Texture2D GenerateOutline(Texture2D skin)
        {
            // Create a new texture that we will fill with outlines
            Texture2D texture = new Texture2D(skin.width, skin.height, TextureFormat.ARGB32, false);

            // Loop through the original character skin and look for non-transparent pixels
            Color[] pixels = skin.GetPixels(0, 0, texture.height, texture.width);
            Color[] newPixels = new Color[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % texture.width;
                int y = i / texture.width;

                if (pixels[i].a > 0.9f)
                {
                    // Fill in the outline with a while pixel surrounding the one we just found
                    newPixels[i] = Color.white;
                    if (x > 1)
                        newPixels[i - 1] = Color.white;
                    if (x < texture.width - 1)
                        newPixels[i + 1] = Color.white;
                    if (y > 1)
                        newPixels[i + texture.width] = Color.white;
                    if (y < texture.height - 1)
                        newPixels[i - texture.width] = Color.white;
                }
                else
                {
                    // Remove the pixel from the outline texture
                    if (newPixels[i].a < 0.1f)
                    {
                        newPixels[i] = Color.clear;
                    }
                }
            }

            // Update the texture with the new pixels
            texture.SetPixels(newPixels);

            // Create the output folder
            string folder = "Outlines";
            string skinPath = AssetDatabase.GetAssetPath(skin);
            string path = skinPath.Substring(0, skinPath.LastIndexOf("/"));
            if (!AssetDatabase.IsValidFolder(path + "/" + folder))
            {
                AssetDatabase.CreateFolder(path, folder);
            }

            string outPath = path + "/" + folder + "/";

            // Write out the new texture
            int index = skinPath.LastIndexOf("/") + 1;
            string file = outPath + skinPath.Substring(index, skinPath.LastIndexOf(".") - index) + "-Outline.png";
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(file, bytes);

            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(file);
        }

        private static void SetBoxColliderFromSprite(BoxCollider2D bc, Sprite sprite)
        {
            Rect from = sprite.textureRect;
            from.y = (sprite.texture.height - from.y) - from.height;

            // Since the sprite texture is probably not read/write enabled,
            // Let's just copy it to a temporary texture that we can read from.
            RenderTexture rt = new RenderTexture(sprite.texture.width, sprite.texture.height, 0);
            Graphics.Blit(sprite.texture, rt);
            Graphics.SetRenderTarget(rt);
            Texture2D maskTexture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            maskTexture.ReadPixels(from, 0, 0, false);
            maskTexture.Apply();

            int w = (int)sprite.textureRect.width;
            int h = (int)sprite.textureRect.height;

            // Compute the mask from the texture data
            Color[] pixels = maskTexture.GetPixels(0, 0, w, h);
            bool[] mask = new bool[pixels.Length];
            for (int i = 0; i < mask.Length; i++)
            {
                mask[i] = pixels[i].a > 0;
            }

            Vector2 start = new Vector2(w, h);
            Vector2 end = new Vector2(0, 0);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int n = (y * w) + x;
                    if (mask[n])
                    {
                        if (x < start.x) start.x = x;
                        if (y < start.y) start.y = y;
                        if (x > end.x) end.x = x;
                        if (y > end.y) end.y = y;
                    }
                }
            }

            end.x += 1;
            end.y += 1;

            float centerX = start.x + ((end.x - start.x) * 0.5f) - (sprite.pivot.x);
            float centerY = start.y + ((end.y - start.y) * 0.5f) - (sprite.pivot.y);

            Vector2 offset = new Vector2(centerX, centerY);
            Vector2 size = new Vector2((end.x - start.x), (end.y - start.y));
            bc.offset = offset * (1 / sprite.pixelsPerUnit);
            bc.size = size * (1 / sprite.pixelsPerUnit);
        }

        private string[] GetSortingLayerNames()
        {
            System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);

            this.sortingLayerNames = (string[])sortingLayersProperty.GetValue(null, new object[0]);
            return this.sortingLayerNames;
        }
    }
}