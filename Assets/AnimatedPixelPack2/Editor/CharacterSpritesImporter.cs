using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class CharacterSpritesImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
        }

        public void OnPostprocessTexture(Texture2D texture)
        {
            // Ensure we only alter textures under our own path to prevent issues with other sprites
            string path = assetPath.ToLower();
            if (path.IndexOf("/animatedpixelpack2/") == -1)
            {
                return;
            }

            Object asset = AssetDatabase.LoadAssetAtPath(this.assetImporter.assetPath, typeof(Texture2D));
            if (asset)
            {
                EditorUtility.SetDirty(asset);
            }
            else
            {
                this.SetDefaults(texture, path);
                assetImporter.SaveAndReimport();
            }
        }

        public void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
        }

        private void SetDefaults(Texture2D texture, string path)
        {
            // Check the type of sprite we are processing
            bool isSpritesheet = path.IndexOf("/spritesheets/") > -1;
            bool isSkin = path.IndexOf("/characters/") > -1 && !isSpritesheet;

            // Set the texture import settings
            TextureImporter textureImporter = (TextureImporter)this.assetImporter;
            textureImporter.maxTextureSize = isSkin ? 64 : 1024;
            textureImporter.isReadable = isSkin ? true : false;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.spritePixelsPerUnit = 16;
            textureImporter.mipmapEnabled = false;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.spritePackingTag = "AnimatedPixelPack2";

            // Slice the sprites into the correct size
            int spriteSize = isSpritesheet ? 48 : 16;
            int xCount = texture.width / spriteSize;
            int yCount = texture.height / spriteSize;

            List<SpriteMetaData> metas = new List<SpriteMetaData>();
            for (int y = 0; y < yCount; y++)
            {
                for (int x = 0; x < xCount; x++)
                {
                    SpriteMetaData meta = new SpriteMetaData();
                    meta.rect = new Rect(x * spriteSize, texture.height - ((y + 1) * spriteSize), spriteSize, spriteSize);
                    meta.name = "Sprite_" + (x + (y * xCount));
                    metas.Add(meta);
                }
            }

            textureImporter.spritesheet = metas.ToArray();
        }
    }
}
