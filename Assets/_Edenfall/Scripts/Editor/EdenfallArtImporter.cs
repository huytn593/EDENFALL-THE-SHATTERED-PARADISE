#if UNITY_EDITOR
using System.IO;
using Edenfall.Characters;
using Edenfall.Cinematics;
using UnityEditor;
using UnityEngine;

namespace Edenfall.Editor
{
    public static class EdenfallArtImporter
    {
        private const string ArtRoot = "Assets/_Edenfall/Art";
        private const string DataRoot = "Assets/_Edenfall/Data";

        [MenuItem("Edenfall/Import and Configure Art Assets")]
        public static void ImportAndConfigureAll()
        {
            Debug.Log("[EdenfallArtImporter] Starting texture import configuration...");
            ConfigureTexturesUnder(ArtRoot);
            AssetDatabase.Refresh();

            LinkAureliaPortraits();
            LinkCinematicCG();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EdenfallArtImporter] All textures configured and linked successfully!");
        }

        public static void ConfigureTexturesUnder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool changed = false;

                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        changed = true;
                    }

                    if (!importer.alphaIsTransparency)
                    {
                        importer.alphaIsTransparency = true;
                        changed = true;
                    }

                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        changed = true;
                    }

                    if (importer.filterMode != FilterMode.Bilinear)
                    {
                        importer.filterMode = FilterMode.Bilinear;
                        changed = true;
                    }

                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        private static void LinkAureliaPortraits()
        {
            string charPath = $"{DataRoot}/Characters/Character_Aurelia.asset";
            var aurelia = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(charPath);
            if (aurelia == null)
            {
                Debug.LogWarning("[EdenfallArtImporter] Character_Aurelia.asset not found at: " + charPath);
                return;
            }

            string pDir = $"{ArtRoot}/Characters/Aurelia/Portraits";
            aurelia.portraits.Clear();

            AddPortrait(aurelia, CharacterEmotion.Neutral, $"{pDir}/Portrait_Aurelia_Neutral.png");
            AddPortrait(aurelia, CharacterEmotion.Happy, $"{pDir}/Portrait_Aurelia_Smile.png");
            AddPortrait(aurelia, CharacterEmotion.Thoughtful, $"{pDir}/Portrait_Aurelia_Regal.png");
            AddPortrait(aurelia, CharacterEmotion.Melancholy, $"{pDir}/Portrait_Aurelia_Sorrow.png");
            AddPortrait(aurelia, CharacterEmotion.Surprised, $"{pDir}/Portrait_Aurelia_Surprise.png");
            AddPortrait(aurelia, CharacterEmotion.Introspective, $"{pDir}/Portrait_Aurelia_Blush.png");

            EditorUtility.SetDirty(aurelia);
            Debug.Log($"[EdenfallArtImporter] Linked {aurelia.portraits.Count} portraits to Character_Aurelia.");
        }

        private static void AddPortrait(CharacterDefinition def, CharacterEmotion emotion, string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                def.portraits.Add(new EmotionPortrait
                {
                    emotion = emotion,
                    portraitSprite = sprite
                });
            }
            else
            {
                Debug.LogWarning("[EdenfallArtImporter] Portrait sprite missing: " + path);
            }
        }

        private static void LinkCinematicCG()
        {
            string cgPath = $"{ArtRoot}/Cinematics/CG_SolsticeVow.png";
            var cgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(cgPath);
            if (cgSprite == null)
            {
                Debug.LogWarning("[EdenfallArtImporter] CG Sprite not found at: " + cgPath);
                return;
            }

            string scenePath = $"{DataRoot}/Cinematics/Cinematic_GardenSolace.asset";
            var sceneData = AssetDatabase.LoadAssetAtPath<CinematicSceneData>(scenePath);
            if (sceneData != null)
            {
                foreach (var slide in sceneData.slides)
                {
                    slide.cgSprite = cgSprite;
                }
                EditorUtility.SetDirty(sceneData);
                Debug.Log("[EdenfallArtImporter] Linked CG_SolsticeVow to Cinematic_GardenSolace slides.");
            }
        }
    }
}
#endif
