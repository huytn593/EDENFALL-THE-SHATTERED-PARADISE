#if UNITY_EDITOR
using System.IO;
using Edenfall.Characters;
using Edenfall.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;

namespace Edenfall.Editor
{
    /// <summary>
    /// Phase 3.6.4 Production Builder for Aurelia Deformable 2D Character Rig.
    /// Builds an authentic 2D skeletal hierarchy where:
    /// - Facial layers and accessories are parented directly to anatomical bones (Bone_Head, Bone_EyeL/R, Bone_Mouth)
    /// - Hair layers are driven by genuine 2D Animation SpriteSkin mesh deformation with multi-bone chains
    /// - Zero detached visual layers; 100% bone-driven articulation
    /// </summary>
    public static class AureliaRigBuilder
    {
        private const string RigLayersPath = "Assets/_Edenfall/Art/Characters/Aurelia/RigLayers";
        private const string PrefabDir = "Assets/_Edenfall/Prefabs/Characters";
        private const string PrefabPath = "Assets/_Edenfall/Prefabs/Characters/AureliaRiggedPrefab.prefab";
        private const string DiagnosticScenePath = "Assets/_Edenfall/Scenes/AureliaRigDiagnostic.unity";

        [MenuItem("Edenfall/Build Aurelia Rigged Character Prefab")]
        public static void BuildRiggedPrefab()
        {
            Debug.Log("[AureliaRigBuilder] Step 1: Baking 2D deformable hair meshes and bone weights...");
            AureliaDeformableMeshBuilder.BakeAllDeformableMeshes();

            Debug.Log("[AureliaRigBuilder] Step 2: Configuring 21 character layer texture import settings...");
            ConfigureRigTextures();

            Debug.Log("[AureliaRigBuilder] Step 3: Constructing Aurelia Deformable Character Rig GameObject...");
            GameObject rigRoot = CreateRigGameObject();

            if (!Directory.Exists(PrefabDir))
            {
                Directory.CreateDirectory(PrefabDir);
            }

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rigRoot, PrefabPath);
            Object.DestroyImmediate(rigRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AureliaRigBuilder] Aurelia Deformable Prefab successfully created at: {PrefabPath}");
        }

        [MenuItem("Edenfall/Create Diagnostic Rig Scene")]
        public static void CreateDiagnosticScene()
        {
            Debug.Log("[AureliaRigBuilder] Creating dedicated diagnostic rig testing scene...");
            BuildRiggedPrefab();

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 5.0f; // 10.0 vertical units view (-5 to +5)
                cam.transform.position = new Vector3(0, 0, -10f);
                cam.backgroundColor = new Color(0.08f, 0.06f, 0.12f, 1.0f);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = "Aurelia_Rigged_Character";
                instance.transform.position = new Vector3(0, -4.95f, 0);
                instance.transform.localScale = new Vector3(0.293f, 0.293f, 1f);
            }

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

            // 1. SKELETON BONE HIERARCHY (In character units, 100 PPU, pivot at bottom-center)
            GameObject boneRoot = new GameObject("Bone_Root");
            boneRoot.transform.SetParent(root.transform, false);
            boneRoot.transform.localPosition = Vector3.zero;

            GameObject bonePelvis = new GameObject("Bone_Pelvis");
            bonePelvis.transform.SetParent(boneRoot.transform, false);
            bonePelvis.transform.localPosition = new Vector3(0, 4.0f, 0);

            GameObject boneTorso = new GameObject("Bone_Torso");
            boneTorso.transform.SetParent(bonePelvis.transform, false);
            boneTorso.transform.localPosition = new Vector3(0, 7.0f, 0); // world Y = 11.0

            GameObject boneChest = new GameObject("Bone_Chest");
            boneChest.transform.SetParent(boneTorso.transform, false);
            boneChest.transform.localPosition = new Vector3(0, 5.0f, 0); // world Y = 16.0

            GameObject boneArmR = new GameObject("Bone_ArmR");
            boneArmR.transform.SetParent(boneChest.transform, false);
            boneArmR.transform.localPosition = new Vector3(1.8f, 0.5f, 0); // world (1.8, 16.5)

            GameObject boneNeck = new GameObject("Bone_Neck");
            boneNeck.transform.SetParent(boneChest.transform, false);
            boneNeck.transform.localPosition = new Vector3(0, 3.5f, 0); // world Y = 19.5

            GameObject boneHead = new GameObject("Bone_Head");
            boneHead.transform.SetParent(boneNeck.transform, false);
            boneHead.transform.localPosition = new Vector3(0, 2.5f, 0); // world Y = 22.0

            // Facial bones under Bone_Head
            GameObject boneEyeL = new GameObject("Bone_EyeL");
            boneEyeL.transform.SetParent(boneHead.transform, false);
            boneEyeL.transform.localPosition = new Vector3(-1.35f, -0.35f, 0); // world (-1.35, 21.65)

            GameObject boneIrisL = new GameObject("Bone_IrisL");
            boneIrisL.transform.SetParent(boneEyeL.transform, false);
            boneIrisL.transform.localPosition = Vector3.zero;

            GameObject boneEyeR = new GameObject("Bone_EyeR");
            boneEyeR.transform.SetParent(boneHead.transform, false);
            boneEyeR.transform.localPosition = new Vector3(2.08f, 1.08f, 0); // world (2.08, 23.08)

            GameObject boneIrisR = new GameObject("Bone_IrisR");
            boneIrisR.transform.SetParent(boneEyeR.transform, false);
            boneIrisR.transform.localPosition = Vector3.zero;

            GameObject boneEyebrows = new GameObject("Bone_Eyebrows");
            boneEyebrows.transform.SetParent(boneHead.transform, false);
            boneEyebrows.transform.localPosition = new Vector3(0.36f, 0.50f, 0); // world (0.36, 22.50)

            GameObject boneMouth = new GameObject("Bone_Mouth");
            boneMouth.transform.SetParent(boneHead.transform, false);
            boneMouth.transform.localPosition = new Vector3(1.50f, -1.95f, 0); // world (1.50, 20.05)

            GameObject boneLilyOrnament = new GameObject("Bone_LilyOrnament");
            boneLilyOrnament.transform.SetParent(boneHead.transform, false);
            boneLilyOrnament.transform.localPosition = new Vector3(-2.80f, 2.50f, 0); // world (-2.80, 24.50)

            // Deformable Multi-Bone Hair Chains
            // Side Hair Left (Under Bone_Head)
            GameObject boneHairSideL_01 = new GameObject("Bone_HairSideL_01");
            boneHairSideL_01.transform.SetParent(boneHead.transform, false);
            boneHairSideL_01.transform.localPosition = new Vector3(-2.03f, 0.42f, 0); // world (-2.03, 22.42)

            GameObject boneHairSideL_02 = new GameObject("Bone_HairSideL_02");
            boneHairSideL_02.transform.SetParent(boneHairSideL_01.transform, false);
            boneHairSideL_02.transform.localPosition = new Vector3(-0.25f, -2.74f, 0);

            GameObject boneHairSideL_03 = new GameObject("Bone_HairSideL_03");
            boneHairSideL_03.transform.SetParent(boneHairSideL_02.transform, false);
            boneHairSideL_03.transform.localPosition = new Vector3(-0.41f, -2.62f, 0);

            // Side Hair Right (Under Bone_Head)
            GameObject boneHairSideR_01 = new GameObject("Bone_HairSideR_01");
            boneHairSideR_01.transform.SetParent(boneHead.transform, false);
            boneHairSideR_01.transform.localPosition = new Vector3(4.02f, 0.14f, 0); // world (4.02, 22.14)

            GameObject boneHairSideR_02 = new GameObject("Bone_HairSideR_02");
            boneHairSideR_02.transform.SetParent(boneHairSideR_01.transform, false);
            boneHairSideR_02.transform.localPosition = new Vector3(0.32f, -2.74f, 0);

            GameObject boneHairSideR_03 = new GameObject("Bone_HairSideR_03");
            boneHairSideR_03.transform.SetParent(boneHairSideR_02.transform, false);
            boneHairSideR_03.transform.localPosition = new Vector3(0.83f, -2.85f, 0);

            // Front Bangs (Under Bone_Head)
            GameObject boneHairFront_01 = new GameObject("Bone_HairFront_01");
            boneHairFront_01.transform.SetParent(boneHead.transform, false);
            boneHairFront_01.transform.localPosition = new Vector3(0.87f, 3.23f, 0); // world (0.87, 25.23)

            GameObject boneHairFront_02 = new GameObject("Bone_HairFront_02");
            boneHairFront_02.transform.SetParent(boneHairFront_01.transform, false);
            boneHairFront_02.transform.localPosition = new Vector3(-0.39f, -1.28f, 0);

            GameObject boneHairFront_03 = new GameObject("Bone_HairFront_03");
            boneHairFront_03.transform.SetParent(boneHairFront_02.transform, false);
            boneHairFront_03.transform.localPosition = new Vector3(-0.31f, -1.48f, 0);

            // Back Flowing Hair (Under Bone_Torso)
            GameObject boneHairBack_01 = new GameObject("Bone_HairBack_01");
            boneHairBack_01.transform.SetParent(boneTorso.transform, false);
            boneHairBack_01.transform.localPosition = new Vector3(0.25f, 13.13f, 0); // world (0.25, 24.13)

            GameObject boneHairBack_02 = new GameObject("Bone_HairBack_02");
            boneHairBack_02.transform.SetParent(boneHairBack_01.transform, false);
            boneHairBack_02.transform.localPosition = new Vector3(-1.26f, -5.15f, 0);

            GameObject boneHairBack_03 = new GameObject("Bone_HairBack_03");
            boneHairBack_03.transform.SetParent(boneHairBack_02.transform, false);
            boneHairBack_03.transform.localPosition = new Vector3(0.27f, -5.26f, 0);

            // 2. VISUAL LAYERS: Direct Bone Parenting & SpriteSkin Deformation
            SpriteRenderer CreateAttachedRenderer(string layerName, string spriteFileName, int order, float zDepth, Transform parentBone, Vector3 localOffset)
            {
                GameObject layerObj = new GameObject(layerName);
                layerObj.transform.SetParent(parentBone, false);
                layerObj.transform.localPosition = localOffset;

                var sr = layerObj.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Characters";
                sr.sortingOrder = order;

                string sPath = $"{RigLayersPath}/{spriteFileName}.png";
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(sPath);
                if (sp != null) sr.sprite = sp;

                return sr;
            }

            SpriteSkin SetupSpriteSkinRenderer(string layerName, string spriteFileName, int order, float zDepth, Transform rootBoneTransform, Transform[] boneTransforms)
            {
                GameObject layerObj = new GameObject(layerName);
                layerObj.transform.SetParent(boneRoot.transform, false);
                layerObj.transform.localPosition = new Vector3(0, 0, zDepth);

                var sr = layerObj.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Characters";
                sr.sortingOrder = order;

                string sPath = $"{RigLayersPath}/{spriteFileName}.png";
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(sPath);
                if (sp != null) sr.sprite = sp;

                var ss = layerObj.AddComponent<SpriteSkin>();
                ss.alwaysUpdate = true;
                ss.SetRootBone(rootBoneTransform);
                ss.SetBoneTransforms(boneTransforms);
                ss.ResetBindPose();

                return ss;
            }

            // A. Deformable Hair Layers (Using genuine SpriteSkin mesh deformation)
            var skinHairBack = SetupSpriteSkinRenderer("Layer_00_Hair_Back", "Layer_00_Hair_Back", 5, 0.20f,
                boneHairBack_01.transform,
                new Transform[] { boneHairBack_01.transform, boneHairBack_02.transform, boneHairBack_03.transform });
            var rendHairBack = skinHairBack.GetComponent<SpriteRenderer>();

            var skinHairSideL = SetupSpriteSkinRenderer("Layer_07_Hair_SideL", "Layer_07_Hair_SideL", 35, -0.045f,
                boneHairSideL_01.transform,
                new Transform[] { boneHairSideL_01.transform, boneHairSideL_02.transform, boneHairSideL_03.transform });
            var rendHairSideL = skinHairSideL.GetComponent<SpriteRenderer>();

            var skinHairSideR = SetupSpriteSkinRenderer("Layer_07_Hair_SideR", "Layer_07_Hair_SideR", 35, -0.045f,
                boneHairSideR_01.transform,
                new Transform[] { boneHairSideR_01.transform, boneHairSideR_02.transform, boneHairSideR_03.transform });
            var rendHairSideR = skinHairSideR.GetComponent<SpriteRenderer>();

            var skinHairFront = SetupSpriteSkinRenderer("Layer_08_Hair_Front_Bangs", "Layer_08_Hair_Front_Bangs", 40, -0.07f,
                boneHairFront_01.transform,
                new Transform[] { boneHairFront_01.transform, boneHairFront_02.transform, boneHairFront_03.transform });
            var rendHairFront = skinHairFront.GetComponent<SpriteRenderer>();

            // B. Anatomically Attached Rigid / Swapping Layers (Follow Bone_Head, Bone_Chest, Bone_Torso)
            var rendBodyTorso = CreateAttachedRenderer("Layer_01_Body_Torso", "Layer_01_Body_Torso", 10, 0.10f, boneTorso.transform, new Vector3(0, -11.0f, 0.10f));
            var rendBreathingChest = CreateAttachedRenderer("Layer_10_Body_Breathing_Chest", "Layer_10_Body_Breathing_Chest", 15, 0.05f, boneChest.transform, new Vector3(0, -16.0f, 0.05f));
            var rendArmR = CreateAttachedRenderer("Layer_11_Arm_Right", "Layer_11_Arm_Right", 38, -0.06f, boneArmR.transform, new Vector3(-1.8f, -16.5f, -0.06f));

            var rendHeadFaceBase = CreateAttachedRenderer("Layer_02_Head_FaceBase", "Layer_02_Head_FaceBase", 20, 0.00f, boneHead.transform, new Vector3(0, -22.0f, 0.00f));
            var rendEyeLOpen = CreateAttachedRenderer("Layer_03_EyeL_Open", "Layer_03_EyeL_Open", 25, -0.02f, boneEyeL.transform, new Vector3(1.35f, -21.65f, -0.02f));
            var rendEyeLBlink = CreateAttachedRenderer("Layer_03_EyeL_Blink", "Layer_03_EyeL_Blink", 25, -0.02f, boneEyeL.transform, new Vector3(1.35f, -21.65f, -0.02f));
            rendEyeLBlink.gameObject.SetActive(false);

            var rendIrisL = CreateAttachedRenderer("Layer_03_IrisL", "Layer_03_IrisL", 26, -0.025f, boneIrisL.transform, new Vector3(1.35f, -21.65f, -0.025f));
            var rendEyeROpen = CreateAttachedRenderer("Layer_04_EyeR_Open", "Layer_04_EyeR_Open", 25, -0.02f, boneEyeR.transform, new Vector3(-2.08f, -23.08f, -0.02f));
            var rendEyeRBlink = CreateAttachedRenderer("Layer_04_EyeR_Blink", "Layer_04_EyeR_Blink", 25, -0.02f, boneEyeR.transform, new Vector3(-2.08f, -23.08f, -0.02f));
            rendEyeRBlink.gameObject.SetActive(false);

            var rendIrisR = CreateAttachedRenderer("Layer_04_IrisR", "Layer_04_IrisR", 26, -0.025f, boneIrisR.transform, new Vector3(-2.08f, -23.08f, -0.025f));
            var rendEyebrows = CreateAttachedRenderer("Layer_05_Eyebrows", "Layer_05_Eyebrows", 28, -0.03f, boneEyebrows.transform, new Vector3(-0.36f, -22.50f, -0.03f));
            var rendMouth = CreateAttachedRenderer("Layer_06_Mouth", "Layer_06_Mouth_Closed", 30, -0.035f, boneMouth.transform, new Vector3(-1.50f, -20.05f, -0.035f));
            var rendLilyRibbon = CreateAttachedRenderer("Layer_09_Accessory_Lily_Ribbon", "Layer_09_Accessory_Lily_Ribbon", 45, -0.085f, boneLilyOrnament.transform, new Vector3(2.80f, -24.50f, -0.085f));

            // 3. WIRE UP CONTROLLER SERIALIZED FIELDS
            SerializedObject so = new SerializedObject(rigCtrl);

            // Skeleton Bones
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
            so.FindProperty("boneLilyOrnament").objectReferenceValue = boneLilyOrnament.transform;
            so.FindProperty("boneArmR").objectReferenceValue = boneArmR.transform;

            // Deformable Multi-Bone Hair Chains
            so.FindProperty("boneHairSideL_01").objectReferenceValue = boneHairSideL_01.transform;
            so.FindProperty("boneHairSideL_02").objectReferenceValue = boneHairSideL_02.transform;
            so.FindProperty("boneHairSideL_03").objectReferenceValue = boneHairSideL_03.transform;

            so.FindProperty("boneHairSideR_01").objectReferenceValue = boneHairSideR_01.transform;
            so.FindProperty("boneHairSideR_02").objectReferenceValue = boneHairSideR_02.transform;
            so.FindProperty("boneHairSideR_03").objectReferenceValue = boneHairSideR_03.transform;

            so.FindProperty("boneHairFront_01").objectReferenceValue = boneHairFront_01.transform;
            so.FindProperty("boneHairFront_02").objectReferenceValue = boneHairFront_02.transform;
            so.FindProperty("boneHairFront_03").objectReferenceValue = boneHairFront_03.transform;

            so.FindProperty("boneHairBack_01").objectReferenceValue = boneHairBack_01.transform;
            so.FindProperty("boneHairBack_02").objectReferenceValue = boneHairBack_02.transform;
            so.FindProperty("boneHairBack_03").objectReferenceValue = boneHairBack_03.transform;

            // SpriteSkin Deformers
            so.FindProperty("skinHairSideL").objectReferenceValue = skinHairSideL;
            so.FindProperty("skinHairSideR").objectReferenceValue = skinHairSideR;
            so.FindProperty("skinHairFront").objectReferenceValue = skinHairFront;
            so.FindProperty("skinHairBack").objectReferenceValue = skinHairBack;

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
