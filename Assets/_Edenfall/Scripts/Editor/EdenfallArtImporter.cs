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
            Debug.Log("[EdenfallArtImporter] Starting production texture import configuration (Phase 3.6.2)...");
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
                if (importer == null) continue;

                bool changed = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }

                // 4K Max Resolution for GTX 1650 4GB support
                if (importer.maxTextureSize != 4096)
                {
                    importer.maxTextureSize = 4096;
                    changed = true;
                }

                // High Quality Desktop Compression (BC7 / ASTC / CompressedHQ)
                if (importer.textureCompression != TextureImporterCompression.CompressedHQ)
                {
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    changed = true;
                }

                // Anchor pivot: Bottom-Center (0.5, 0.0) for Character Master & Rig layers
                bool isCharacterBody = path.Contains("Aurelia_HalfBody") || path.Contains("Aurelia_FullBody") ||
                                       path.Contains("Aurelia_VN_Portrait") || path.Contains("Aurelia_VN_FullBody") ||
                                       path.Contains("Layer_Body") || path.Contains("Layer_Cape") || path.Contains("Layer_Skirt");

                if (isCharacterBody)
                {
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    if (settings.spriteAlignment != (int)SpriteAlignment.BottomCenter)
                    {
                        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                        importer.SetTextureSettings(settings);
                        changed = true;
                    }
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

                if (changed)
                {
                    importer.SaveAndReimport();
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

            // Link 16 canonical anime portraits
            AddPortrait(aurelia, CharacterEmotion.Gentle, $"{pDir}/Portrait_Normal.png");
            AddPortrait(aurelia, CharacterEmotion.Smiling, $"{pDir}/Portrait_Smile.png");
            AddPortrait(aurelia, CharacterEmotion.Happy, $"{pDir}/Portrait_Happy.png");
            AddPortrait(aurelia, CharacterEmotion.Excited, $"{pDir}/Portrait_Excited.png");
            AddPortrait(aurelia, CharacterEmotion.Surprised, $"{pDir}/Portrait_Surprised.png");
            AddPortrait(aurelia, CharacterEmotion.Confused, $"{pDir}/Portrait_Confused.png");
            AddPortrait(aurelia, CharacterEmotion.Thinking, $"{pDir}/Portrait_Thinking.png");
            AddPortrait(aurelia, CharacterEmotion.Concerned, $"{pDir}/Portrait_Concerned.png");
            AddPortrait(aurelia, CharacterEmotion.Flustered, $"{pDir}/Portrait_Shy.png");
            AddPortrait(aurelia, CharacterEmotion.Blushing, $"{pDir}/Portrait_Blush.png");
            AddPortrait(aurelia, CharacterEmotion.Seductive, $"{pDir}/Portrait_Seductive.png");
            AddPortrait(aurelia, CharacterEmotion.Sad, $"{pDir}/Portrait_Sad.png");
            AddPortrait(aurelia, CharacterEmotion.Angry, $"{pDir}/Portrait_Angry.png");
            AddPortrait(aurelia, CharacterEmotion.Hurt, $"{pDir}/Portrait_Hurt.png");
            AddPortrait(aurelia, CharacterEmotion.Satisfied, $"{pDir}/Portrait_Satisfied.png");

            EditorUtility.SetDirty(aurelia);
            Debug.Log($"[EdenfallArtImporter] Linked {aurelia.portraits.Count} canonical portraits to Character_Aurelia.");
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
            string cgDir = $"{ArtRoot}/Cinematics";
            string cgPath = $"{cgDir}/CG_01_FirstMeeting.png";
            var cgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(cgPath);
            if (cgSprite == null) cgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{cgDir}/CG_SolsticeVow.png");

            string scenePath = $"{DataRoot}/Cinematics/Cinematic_GardenSolace.asset";
            var sceneData = AssetDatabase.LoadAssetAtPath<CinematicSceneData>(scenePath);
            if (sceneData != null && cgSprite != null)
            {
                foreach (var slide in sceneData.slides)
                {
                    slide.cgSprite = cgSprite;
                }
                EditorUtility.SetDirty(sceneData);
                Debug.Log("[EdenfallArtImporter] Linked CG to Cinematic_GardenSolace slides.");
            }
        }
    }
}
#endif
