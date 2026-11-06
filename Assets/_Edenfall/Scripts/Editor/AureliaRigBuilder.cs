#if UNITY_EDITOR
using System.IO;
using Edenfall.Characters;
using Edenfall.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Edenfall.Editor
{
    public static class AureliaRigBuilder
    {
        private const string RigLayersPath = "Assets/_Edenfall/Art/Characters/Aurelia/RigLayers";
        private const string PrefabDir = "Assets/_Edenfall/Prefabs/Characters";
        private const string PrefabPath = "Assets/_Edenfall/Prefabs/Characters/AureliaRiggedPrefab.prefab";
        private const string DiagnosticScenePath = "Assets/_Edenfall/Scenes/AureliaRigDiagnostic.unity";

        [MenuItem("Edenfall/Build Aurelia Rigged Character Prefab")]
        public static void BuildRiggedPrefab()
        {
            Debug.Log("[AureliaRigBuilder] Configuring 21 character layer textures...");
            ConfigureRigTextures();

            Debug.Log("[AureliaRigBuilder] Constructing Aurelia Rigged Character GameObject...");
            GameObject rigRoot = CreateRigGameObject();

            if (!Directory.Exists(PrefabDir))
            {
                Directory.CreateDirectory(PrefabDir);
            }

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rigRoot, PrefabPath);
            Object.DestroyImmediate(rigRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AureliaRigBuilder] Aurelia Rigged Prefab successfully created at: {PrefabPath}");
        }

        [MenuItem("Edenfall/Create Diagnostic Rig Scene")]
        public static void CreateDiagnosticScene()
        {
            Debug.Log("[AureliaRigBuilder] Creating dedicated diagnostic rig testing scene...");
            BuildRiggedPrefab();

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Configure Camera for 1080p Visual Novel Staging
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 5.0f; // 10.0 vertical units view (-5 to +5)
                cam.transform.position = new Vector3(0, 0, -10f);
                cam.backgroundColor = new Color(0.08f, 0.06f, 0.12f, 1.0f);
            }

            // Instantiate Rigged Prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = "Aurelia_Rigged_Character";
                var anchor = instance.GetComponent<NPCStageAnchor>();
                var primaryRend = instance.transform.Find("VisualLayers/Layer_01_Body_Torso")?.GetComponent<SpriteRenderer>();
                if (anchor != null && primaryRend != null && cam != null)
                {
                    anchor.AlignCharacter(instance.transform, primaryRend, StagingMode.DialoguePortrait, cam);
                }
                else
                {
                    instance.transform.position = new Vector3(0, -4.95f, 0);
                    instance.transform.localScale = new Vector3(0.293f, 0.293f, 1f);
                }
            }

            // Add simple directional light
            GameObject lightObj = new GameObject("Directional 2D Light");
            var l = lightObj.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1.0f, 0.95f, 0.90f);
            l.intensity = 1.0f;

            string sceneDir = Path.GetDirectoryName(DiagnosticScenePath);
            if (!Directory.Exists(sceneDir))
            {
                Directory.CreateDirectory(sceneDir);
            }

            EditorSceneManager.SaveScene(newScene, DiagnosticScenePath);
            Debug.Log($"[AureliaRigBuilder] Diagnostic scene saved at: {DiagnosticScenePath}");
        }

        public static void ConfigureRigTextures()
        {
            if (!Directory.Exists(RigLayersPath))
            {
                Debug.LogError($"[AureliaRigBuilder] Rig layers path not found: {RigLayersPath}");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { RigLayersPath });
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

                if (importer.spritePixelsPerUnit != 100f)
                {
                    importer.spritePixelsPerUnit = 100f;
                    changed = true;
                }

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                if (settings.spriteAlignment != (int)SpriteAlignment.BottomCenter)
                {
                    settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                    importer.SetTextureSettings(settings);
                    changed = true;
                }

                if (importer.maxTextureSize != 4096)
                {
                    importer.maxTextureSize = 4096;
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

                if (changed)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
            }

            AssetDatabase.Refresh();
        }

        public static GameObject CreateRigGameObject()
        {
            GameObject root = new GameObject("Aurelia_Rigged_Root");
            var anchor = root.AddComponent<NPCStageAnchor>();
            var rigCtrl = root.AddComponent<AureliaRiggedCharacter>();

            GameObject boneRoot = new GameObject("Bone_Root");
            boneRoot.transform.SetParent(root.transform, false);

            GameObject bonePelvis = new GameObject("Bone_Pelvis");
            bonePelvis.transform.SetParent(boneRoot.transform, false);
            bonePelvis.transform.localPosition = new Vector3(0, 0.4f, 0);

            GameObject boneTorso = new GameObject("Bone_Torso");
            boneTorso.transform.SetParent(bonePelvis.transform, false);
            boneTorso.transform.localPosition = new Vector3(0, 0.7f, 0);

            GameObject boneChest = new GameObject("Bone_Chest");
            boneChest.transform.SetParent(boneTorso.transform, false);
            boneChest.transform.localPosition = new Vector3(0, 0.4f, 0);

            GameObject boneArmR = new GameObject("Bone_ArmR");
            boneArmR.transform.SetParent(boneChest.transform, false);
            boneArmR.transform.localPosition = new Vector3(0.18f, 0.05f, 0);

            GameObject boneNeck = new GameObject("Bone_Neck");
            boneNeck.transform.SetParent(boneChest.transform, false);
            boneNeck.transform.localPosition = new Vector3(0, 0.4f, 0);

            GameObject boneHead = new GameObject("Bone_Head");
            boneHead.transform.SetParent(boneNeck.transform, false);
            boneHead.transform.localPosition = new Vector3(0, 0.3f, 0);

            // Facial bones under Head
            GameObject boneEyeL = new GameObject("Bone_EyeL");
            boneEyeL.transform.SetParent(boneHead.transform, false);
            boneEyeL.transform.localPosition = new Vector3(-0.22f, 0.05f, 0);

            GameObject boneIrisL = new GameObject("Bone_IrisL");
            boneIrisL.transform.SetParent(boneEyeL.transform, false);

            GameObject boneEyeR = new GameObject("Bone_EyeR");
            boneEyeR.transform.SetParent(boneHead.transform, false);
            boneEyeR.transform.localPosition = new Vector3(0.15f, 0.18f, 0);

            GameObject boneIrisR = new GameObject("Bone_IrisR");
            boneIrisR.transform.SetParent(boneEyeR.transform, false);

            GameObject boneEyebrows = new GameObject("Bone_Eyebrows");
            boneEyebrows.transform.SetParent(boneHead.transform, false);
            boneEyebrows.transform.localPosition = new Vector3(0, 0.12f, 0);

            GameObject boneMouth = new GameObject("Bone_Mouth");
            boneMouth.transform.SetParent(boneHead.transform, false);
            boneMouth.transform.localPosition = new Vector3(0.08f, -0.22f, 0);

            GameObject boneHairFront = new GameObject("Bone_HairFront");
            boneHairFront.transform.SetParent(boneHead.transform, false);
            boneHairFront.transform.localPosition = new Vector3(0, 0.25f, 0);

            GameObject boneHairSideL = new GameObject("Bone_HairSideL");
            boneHairSideL.transform.SetParent(boneHead.transform, false);
            boneHairSideL.transform.localPosition = new Vector3(-0.35f, -0.15f, 0);

            GameObject boneHairSideR = new GameObject("Bone_HairSideR");
            boneHairSideR.transform.SetParent(boneHead.transform, false);
            boneHairSideR.transform.localPosition = new Vector3(0.38f, -0.12f, 0);

            GameObject boneHairBack = new GameObject("Bone_HairBack");
            boneHairBack.transform.SetParent(boneTorso.transform, false);
            boneHairBack.transform.localPosition = new Vector3(0, 0.5f, 0);

            GameObject boneLilyOrnament = new GameObject("Bone_LilyOrnament");
            boneLilyOrnament.transform.SetParent(boneHead.transform, false);
            boneLilyOrnament.transform.localPosition = new Vector3(-0.32f, 0.35f, 0);

            // Visual Layers container
            GameObject visualLayers = new GameObject("VisualLayers");
            visualLayers.transform.SetParent(root.transform, false);

            SpriteRenderer CreateLayerRenderer(string layerName, string spriteFileName, int order, float zDepth, Transform parentBone)
            {
                GameObject layerObj = new GameObject(layerName);
                layerObj.transform.SetParent(visualLayers.transform, false);
                layerObj.transform.localPosition = new Vector3(0, 0, zDepth);

                var sr = layerObj.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Characters";
                sr.sortingOrder = order;

                string sPath = $"{RigLayersPath}/{spriteFileName}.png";
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(sPath);
                if (sp != null) sr.sprite = sp;

                return sr;
            }

            var rendHairBack = CreateLayerRenderer("Layer_00_Hair_Back", "Layer_00_Hair_Back", 5, 0.20f, boneHairBack.transform);
            var rendBodyTorso = CreateLayerRenderer("Layer_01_Body_Torso", "Layer_01_Body_Torso", 10, 0.10f, boneTorso.transform);
            var rendBreathingChest = CreateLayerRenderer("Layer_10_Body_Breathing_Chest", "Layer_10_Body_Breathing_Chest", 15, 0.05f, boneChest.transform);
            var rendHeadFaceBase = CreateLayerRenderer("Layer_02_Head_FaceBase", "Layer_02_Head_FaceBase", 20, 0.00f, boneHead.transform);
            var rendEyeLOpen = CreateLayerRenderer("Layer_03_EyeL_Open", "Layer_03_EyeL_Open", 25, -0.02f, boneEyeL.transform);
            var rendEyeLBlink = CreateLayerRenderer("Layer_03_EyeL_Blink", "Layer_03_EyeL_Blink", 25, -0.02f, boneEyeL.transform);
            rendEyeLBlink.gameObject.SetActive(false); // Closed initially

            var rendIrisL = CreateLayerRenderer("Layer_03_IrisL", "Layer_03_IrisL", 26, -0.025f, boneIrisL.transform);
            var rendEyeROpen = CreateLayerRenderer("Layer_04_EyeR_Open", "Layer_04_EyeR_Open", 25, -0.02f, boneEyeR.transform);
            var rendEyeRBlink = CreateLayerRenderer("Layer_04_EyeR_Blink", "Layer_04_EyeR_Blink", 25, -0.02f, boneEyeR.transform);
            rendEyeRBlink.gameObject.SetActive(false);

            var rendIrisR = CreateLayerRenderer("Layer_04_IrisR", "Layer_04_IrisR", 26, -0.025f, boneIrisR.transform);
            var rendEyebrows = CreateLayerRenderer("Layer_05_Eyebrows", "Layer_05_Eyebrows", 28, -0.03f, boneEyebrows.transform);
            var rendMouth = CreateLayerRenderer("Layer_06_Mouth", "Layer_06_Mouth_Closed", 30, -0.035f, boneMouth.transform);
            var rendHairSideL = CreateLayerRenderer("Layer_07_Hair_SideL", "Layer_07_Hair_SideL", 35, -0.045f, boneHairSideL.transform);
            var rendHairSideR = CreateLayerRenderer("Layer_07_Hair_SideR", "Layer_07_Hair_SideR", 35, -0.045f, boneHairSideR.transform);
            var rendArmR = CreateLayerRenderer("Layer_11_Arm_Right", "Layer_11_Arm_Right", 38, -0.06f, boneArmR.transform);
            var rendHairFront = CreateLayerRenderer("Layer_08_Hair_Front_Bangs", "Layer_08_Hair_Front_Bangs", 40, -0.07f, boneHairFront.transform);
            var rendLilyRibbon = CreateLayerRenderer("Layer_09_Accessory_Lily_Ribbon", "Layer_09_Accessory_Lily_Ribbon", 45, -0.085f, boneLilyOrnament.transform);

            // Wire up AureliaRiggedCharacter serialized fields using SerializedObject
            SerializedObject so = new SerializedObject(rigCtrl);

            // Bones
            so.FindProperty("boneRoot").objectReferenceValue = boneRoot.transform;
            so.FindProperty("bonePelvis").objectReferenceValue = bonePelvis.transform;
            so.FindProperty("boneTorso").objectReferenceValue = boneTorso.transform;
            so.FindProperty("boneChest").objectReferenceValue = boneChest.transform;
            so.FindProperty("boneNeck").objectReferenceValue = boneNeck.transform;
            so.FindProperty("boneHead").objectReferenceValue = boneHead.transform;
            so.FindProperty("boneEyeL").objectReferenceValue = boneEyeL.transform;
            so.FindProperty("boneIrisL").objectReferenceValue = boneIrisL.transform;
            so.FindProperty("boneEyeR").objectReferenceValue = boneEyeR.transform;
            so.FindProperty("boneIrisR").objectReferenceValue = boneIrisR.transform;
            so.FindProperty("boneEyebrows").objectReferenceValue = boneEyebrows.transform;
            so.FindProperty("boneMouth").objectReferenceValue = boneMouth.transform;
            so.FindProperty("boneHairFront").objectReferenceValue = boneHairFront.transform;
            so.FindProperty("boneHairSideL").objectReferenceValue = boneHairSideL.transform;
            so.FindProperty("boneHairSideR").objectReferenceValue = boneHairSideR.transform;
            so.FindProperty("boneHairBack").objectReferenceValue = boneHairBack.transform;
            so.FindProperty("boneLilyOrnament").objectReferenceValue = boneLilyOrnament.transform;
            so.FindProperty("boneArmR").objectReferenceValue = boneArmR.transform;

            // Renderers
            so.FindProperty("rendHairBack").objectReferenceValue = rendHairBack;
            so.FindProperty("rendBodyTorso").objectReferenceValue = rendBodyTorso;
            so.FindProperty("rendHeadFaceBase").objectReferenceValue = rendHeadFaceBase;
            so.FindProperty("rendEyeLOpen").objectReferenceValue = rendEyeLOpen;
            so.FindProperty("rendEyeLBlink").objectReferenceValue = rendEyeLBlink;
            so.FindProperty("rendIrisL").objectReferenceValue = rendIrisL;
            so.FindProperty("rendEyeROpen").objectReferenceValue = rendEyeROpen;
            so.FindProperty("rendEyeRBlink").objectReferenceValue = rendEyeRBlink;
            so.FindProperty("rendIrisR").objectReferenceValue = rendIrisR;
            so.FindProperty("rendEyebrows").objectReferenceValue = rendEyebrows;
            so.FindProperty("rendMouth").objectReferenceValue = rendMouth;
            so.FindProperty("rendHairSideL").objectReferenceValue = rendHairSideL;
            so.FindProperty("rendHairSideR").objectReferenceValue = rendHairSideR;
            so.FindProperty("rendHairFront").objectReferenceValue = rendHairFront;
            so.FindProperty("rendLilyRibbon").objectReferenceValue = rendLilyRibbon;
            so.FindProperty("rendBreathingChest").objectReferenceValue = rendBreathingChest;
            so.FindProperty("rendArmR").objectReferenceValue = rendArmR;

            // Mouth Visemes
            so.FindProperty("mouthClosed").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"{RigLayersPath}/Layer_06_Mouth_Closed.png");
            so.FindProperty("mouthHalfOpen").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"{RigLayersPath}/Layer_06_Mouth_HalfOpen.png");
            so.FindProperty("mouthOpen").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"{RigLayersPath}/Layer_06_Mouth_Open.png");
            so.FindProperty("mouthWide").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"{RigLayersPath}/Layer_06_Mouth_Wide.png");
            so.FindProperty("mouthSmile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"{RigLayersPath}/Layer_06_Mouth_Smile.png");

            so.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }
    }
}
#endif
