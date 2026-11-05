#if UNITY_EDITOR
using System.IO;
using Edenfall.Characters;
using Edenfall.Cinematics;
using Edenfall.Core;
using Edenfall.Dialogue;
using Edenfall.Gameplay;
using Edenfall.Inventory;
using Edenfall.NPC.Behavior;
using Edenfall.NPC.Scheduling;
using Edenfall.Quests;
using Edenfall.UI;
using Edenfall.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Edenfall.Editor
{
    public static class EdenfallSceneBuilder
    {
        private const string ScenePath = "Assets/_Edenfall/Scenes/DawnSanctuary/DawnSanctuary.unity";
        private const string DataRoot = "Assets/_Edenfall/Data";
        private const string ArtRoot = "Assets/_Edenfall/Art";

        [MenuItem("Edenfall/Build Dawn Sanctuary Scene")]
        public static void BuildDawnSanctuaryScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Clean existing scene root objects
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                Object.DestroyImmediate(root);
            }

            Debug.Log("[EdenfallSceneBuilder] Building Dawn Sanctuary production anime visual scene...");

            // 1. Camera & URP Volume
            var (cameraObj, cam2D) = CreateCameraSetup();
            CreateLightingAndVFXSetup();

            // 2. Managers Root
            CreateManagersSetup();

            // 3. Environment & Terrain Platforms & Parallax
            CreateEnvironmentSetup();

            // 4. Player Character (Echo)
            var (playerObj, echoRig) = CreatePlayerSetup();
            if (cam2D != null && playerObj != null)
            {
                cam2D.SetTarget(playerObj.transform);
            }

            // 5. NPC Aurelia (Full Layered Visual Rig)
            var (aureliaObj, aureliaRig) = CreateAureliaSetup();

            // 6. Interactive Shrines & Props
            var altarObj = CreateInteractivePropsSetup();

            // 7. Full Production UI Canvas
            var (canvasObj, cinOverlay, subText, spkText, skipBtn) = CreateUISetup();

            // 8. Cinematic Sequence Setup
            CreateCinematicSequenceSetup(cam2D, aureliaRig, echoRig, cinOverlay, subText, spkText, skipBtn);

            // Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[EdenfallSceneBuilder] Dawn Sanctuary scene built and saved successfully with full anime visuals!");
        }

        private static (GameObject, CameraController2D) CreateCameraSetup()
        {
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0, 1.8f, -10f);

            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.2f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.16f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            camObj.AddComponent<AudioListener>();
            var camData = camObj.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;

            var cam2D = camObj.AddComponent<CameraController2D>();
            cam2D.SetSpeed(4f);
            cam2D.SetBounds(new Vector2(-20f, -2f), new Vector2(20f, 12f));

            return (camObj, cam2D);
        }

        private static void CreateLightingAndVFXSetup()
        {
            var lightRoot = new GameObject("--- LIGHTING & POST PROCESSING ---");

            // 1. Post Processing Volume
            var volObj = new GameObject("Global PostProcessing Volume");
            volObj.transform.SetParent(lightRoot.transform);
            var vol = volObj.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 1f;

            var profile = EdenfallVFXBuilder.EnsureProfile();
            vol.profile = profile;

            // 2. 2D Global Light
            var globalLightObj = new GameObject("Global Light 2D");
            globalLightObj.transform.SetParent(lightRoot.transform);
            var light2D = globalLightObj.AddComponent<Light2D>();
            light2D.lightType = Light2D.LightType.Global;
            light2D.color = new Color(1.0f, 0.92f, 0.78f, 1f);
            light2D.intensity = 1.15f;

            // 3. Time of Day Controller
            var todLighting = globalLightObj.AddComponent<DawnSanctuaryLighting>();

            // 4. Directional Key Light for 3D/depth
            var dirObj = new GameObject("Directional Key Light");
            dirObj.transform.SetParent(lightRoot.transform);
            var dirLight = dirObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(1.0f, 0.88f, 0.70f);
            dirLight.intensity = 1.1f;
            dirObj.transform.rotation = Quaternion.Euler(45f, -30f, 0);
            dirObj.AddComponent<UniversalAdditionalLightData>();

            // Wire to DawnSanctuaryLighting
            var globalLightField = typeof(DawnSanctuaryLighting).GetField("globalLight2D", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (globalLightField != null) globalLightField.SetValue(todLighting, light2D);

            var dirLightField = typeof(DawnSanctuaryLighting).GetField("directionalKeyLight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dirLightField != null) dirLightField.SetValue(todLighting, dirLight);

            var volField = typeof(DawnSanctuaryLighting).GetField("globalVolume", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (volField != null) volField.SetValue(todLighting, vol);

            // 5. Environmental VFX
            var vfxRoot = new GameObject("--- ENVIRONMENTAL VFX ---");
            vfxRoot.transform.SetParent(lightRoot.transform);
            EdenfallVFXBuilder.CreateCelestialSparks(vfxRoot.transform);
            EdenfallVFXBuilder.CreateFallingPetals(vfxRoot.transform);
            EdenfallVFXBuilder.CreateAltarAura(vfxRoot.transform);
        }

        private static void CreateManagersSetup()
        {
            var mgrRoot = new GameObject("--- MANAGERS ---");

            var gmObj = new GameObject("GameManager");
            gmObj.transform.SetParent(mgrRoot.transform);
            gmObj.AddComponent<GameManager>();

            var locObj = new GameObject("LocalizationManager");
            locObj.transform.SetParent(mgrRoot.transform);
            locObj.AddComponent<LocalizationManager>();

            var invObj = new GameObject("InventoryManager");
            invObj.transform.SetParent(mgrRoot.transform);
            invObj.AddComponent<InventoryManager>();

            var questObj = new GameObject("QuestManager");
            questObj.transform.SetParent(mgrRoot.transform);
            questObj.AddComponent<QuestManager>();

            var diagObj = new GameObject("DialogueManager");
            diagObj.transform.SetParent(mgrRoot.transform);
            diagObj.AddComponent<DialogueManager>();

            var cineObj = new GameObject("CinematicManager");
            cineObj.transform.SetParent(mgrRoot.transform);
            cineObj.AddComponent<CinematicManager>();

            var galObj = new GameObject("GalleryManager");
            galObj.transform.SetParent(mgrRoot.transform);
            galObj.AddComponent<GalleryManager>();
        }

        private static void CreateEnvironmentSetup()
        {
            var envRoot = new GameObject("--- ENVIRONMENT ---");

            // 1. Far Sky Parallax
            var skyObj = new GameObject("Layer_FarSky");
            skyObj.transform.SetParent(envRoot.transform);
            skyObj.transform.position = new Vector3(0, 3.8f, 20f);
            var p1 = skyObj.AddComponent<ParallaxLayer>();
            p1.SetMultiplier(new Vector2(0.02f, 0.01f));
            var srSky = skyObj.AddComponent<SpriteRenderer>();
            var skySprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/Environment/Sky_CelestialDawn.png");
            if (skySprite != null) srSky.sprite = skySprite;
            srSky.sortingOrder = -30;
            skyObj.transform.localScale = new Vector3(2.8f, 2.8f, 1f);

            // 2. Solstice Spire Parallax
            var spireObj = new GameObject("Layer_SolsticeSpire");
            spireObj.transform.SetParent(envRoot.transform);
            spireObj.transform.position = new Vector3(-3.5f, 3.2f, 10f);
            var p2 = spireObj.AddComponent<ParallaxLayer>();
            p2.SetMultiplier(new Vector2(0.10f, 0.04f));
            var srSpire = spireObj.AddComponent<SpriteRenderer>();
            var spireSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/Environment/Spire_SolsticeDistant.png");
            if (spireSprite != null) srSpire.sprite = spireSprite;
            srSpire.sortingOrder = -20;
            spireObj.transform.localScale = new Vector3(2.0f, 2.0f, 1f);

            // 3. Main Marble Platform (Walkable Ground)
            var groundObj = new GameObject("Sanctuary_MainGround");
            groundObj.transform.SetParent(envRoot.transform);
            groundObj.transform.position = new Vector3(0, -1.8f, 0);

            var groundCol = groundObj.AddComponent<BoxCollider2D>();
            groundCol.size = new Vector2(35f, 2f);
            groundCol.offset = new Vector2(0, -0.6f);

            var srGround = groundObj.AddComponent<SpriteRenderer>();
            var groundSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/Environment/Sanctuary_MarblePlatform.png");
            if (groundSprite != null) srGround.sprite = groundSprite;
            srGround.sortingOrder = -5;
            groundObj.transform.localScale = new Vector3(2.4f, 2.0f, 1f);
        }

        private static (GameObject, CharacterVisualRig) CreatePlayerSetup()
        {
            var charRoot = GameObject.Find("--- CHARACTERS ---") ?? new GameObject("--- CHARACTERS ---");

            var player = new GameObject("Player");
            player.transform.SetParent(charRoot.transform);
            player.transform.position = new Vector3(-3.5f, -0.5f, 0);

            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            var col = player.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 2.2f);
            col.offset = new Vector2(0, 1.1f);

            var sr = player.AddComponent<SpriteRenderer>();
            var echoSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/Characters/Echo/Echo_FullBody.png");
            if (echoSprite != null) sr.sprite = echoSprite;
            sr.sortingOrder = 10;
            player.transform.localScale = new Vector3(0.24f, 0.24f, 1f);

            player.AddComponent<PlayerController2D>();

            // Rig for procedural breathing
            var rig = player.AddComponent<CharacterVisualRig>();
            rig.ConfigureRig(player.transform, player.transform, null, null, null, null,
                sr, null, null, null, null, null, null);

            return (player, rig);
        }

        private static (GameObject, CharacterVisualRig) CreateAureliaSetup()
        {
            var charRoot = GameObject.Find("--- CHARACTERS ---") ?? new GameObject("--- CHARACTERS ---");

            var aureliaObj = new GameObject("NPC_Aurelia");
            aureliaObj.transform.SetParent(charRoot.transform);
            aureliaObj.transform.position = new Vector3(3.8f, -0.6f, 0);
            aureliaObj.transform.localScale = new Vector3(0.38f, 0.38f, 1f);

            var col = aureliaObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 2.4f);
            col.offset = new Vector2(0, 1.2f);
            col.isTrigger = true;

            // Load Aurelia Sprites
            string sDir = $"{ArtRoot}/Characters/Aurelia/Sprites";
            var bodySp = AssetDatabase.LoadAssetAtPath<Sprite>($"{sDir}/Aurelia_FullBody.png");
            var headSp = AssetDatabase.LoadAssetAtPath<Sprite>($"{sDir}/Aurelia_Rig_Head.png");
            var torsoSp = AssetDatabase.LoadAssetAtPath<Sprite>($"{sDir}/Aurelia_Rig_Torso.png");
            var skirtSp = AssetDatabase.LoadAssetAtPath<Sprite>($"{sDir}/Aurelia_Rig_Skirt.png");
            var hairBackSp = AssetDatabase.LoadAssetAtPath<Sprite>($"{sDir}/Aurelia_Rig_HairBack.png");
            var hairFrontSp = AssetDatabase.LoadAssetAtPath<Sprite>($"{sDir}/Aurelia_Rig_HairFront.png");
            var eyeOpenSp = AssetDatabase.LoadAssetAtPath<Sprite>($"{sDir}/Aurelia_Rig_EyesOpen.png");
            var eyeBlinkSp = AssetDatabase.LoadAssetAtPath<Sprite>($"{sDir}/Aurelia_Rig_EyesClosed.png");
            var mouthClosedSp = AssetDatabase.LoadAssetAtPath<Sprite>($"{sDir}/Aurelia_Rig_MouthClosed.png");
            var mouthOpenSp = AssetDatabase.LoadAssetAtPath<Sprite>($"{sDir}/Aurelia_Rig_MouthOpen.png");

            // Base Body Renderer
            var baseR = aureliaObj.AddComponent<SpriteRenderer>();
            baseR.sprite = bodySp;
            baseR.sortingOrder = 8;

            // Child HairBack
            var hairBackObj = new GameObject("Rig_HairBack");
            hairBackObj.transform.SetParent(aureliaObj.transform, false);
            var hairBackR = hairBackObj.AddComponent<SpriteRenderer>();
            hairBackR.sprite = hairBackSp;
            hairBackR.sortingOrder = 7;

            // Child Torso
            var torsoObj = new GameObject("Rig_Torso");
            torsoObj.transform.SetParent(aureliaObj.transform, false);
            var torsoR = torsoObj.AddComponent<SpriteRenderer>();
            torsoR.sprite = torsoSp;
            torsoR.sortingOrder = 9;

            // Child Skirt
            var skirtObj = new GameObject("Rig_Skirt");
            skirtObj.transform.SetParent(aureliaObj.transform, false);
            var skirtR = skirtObj.AddComponent<SpriteRenderer>();
            skirtR.sprite = skirtSp;
            skirtR.sortingOrder = 8;

            // Child Head
            var headObj = new GameObject("Rig_Head");
            headObj.transform.SetParent(aureliaObj.transform, false);
            headObj.transform.localPosition = new Vector3(0, 0.05f, 0);
            var headR = headObj.AddComponent<SpriteRenderer>();
            headR.sprite = headSp;
            headR.sortingOrder = 10;

            // Child HairFront
            var hairFrontObj = new GameObject("Rig_HairFront");
            hairFrontObj.transform.SetParent(headObj.transform, false);
            var hairFrontR = hairFrontObj.AddComponent<SpriteRenderer>();
            hairFrontR.sprite = hairFrontSp;
            hairFrontR.sortingOrder = 13;

            // Child Eyes
            var eyesObj = new GameObject("Rig_Eyes");
            eyesObj.transform.SetParent(headObj.transform, false);
            var eyesR = eyesObj.AddComponent<SpriteRenderer>();
            eyesR.sprite = eyeOpenSp;
            eyesR.sortingOrder = 11;

            // Child Mouth
            var mouthObj = new GameObject("Rig_Mouth");
            mouthObj.transform.SetParent(headObj.transform, false);
            var mouthR = mouthObj.AddComponent<SpriteRenderer>();
            mouthR.sprite = mouthClosedSp;
            mouthR.sortingOrder = 12;

            // Setup CharacterVisualRig
            var rig = aureliaObj.AddComponent<CharacterVisualRig>();
            rig.ConfigureRig(
                aureliaObj.transform, torsoObj.transform, headObj.transform, hairBackObj.transform, hairFrontObj.transform, skirtObj.transform,
                baseR, headR, eyesR, mouthR, hairBackR, hairFrontR, skirtR
            );
            rig.AssignSprites(bodySp, headSp, eyeOpenSp, eyeBlinkSp, mouthClosedSp, mouthOpenSp, hairBackSp, hairFrontSp, skirtSp);

            // Setup AureliaRigController
            var rigCtrl = aureliaObj.AddComponent<AureliaRigController>();
            rigCtrl.ConfigurePresets(
                rig,
                eyeOpenSp, eyeOpenSp, eyeOpenSp, eyeBlinkSp, eyeBlinkSp, eyeOpenSp,
                mouthClosedSp, mouthOpenSp, mouthOpenSp
            );

            // Setup NPC Controller
            var npc = aureliaObj.AddComponent<NPCController>();
            var aureliaDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>($"{DataRoot}/Characters/Character_Aurelia.asset");
            var aureliaDiag = AssetDatabase.LoadAssetAtPath<DialogueTree>($"{DataRoot}/Dialogue/DialogueTree_Aurelia.asset");
            var aureliaSchedule = AssetDatabase.LoadAssetAtPath<NPCSchedule>($"{DataRoot}/Schedules/Schedule_Aurelia.asset");

            npc.Initialize(aureliaDef, aureliaSchedule, aureliaDiag);

            return (aureliaObj, rig);
        }

        private static GameObject CreateInteractivePropsSetup()
        {
            var propRoot = new GameObject("--- INTERACTIVE PROPS ---");

            var shrineObj = new GameObject("Prop_CelestialAltar");
            shrineObj.transform.SetParent(propRoot.transform);
            shrineObj.transform.position = new Vector3(8.5f, 0.5f, 0);
            shrineObj.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

            var col = shrineObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 3f);
            col.isTrigger = true;

            var sr = shrineObj.AddComponent<SpriteRenderer>();
            var altarSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/Props/Prop_CelestialAltar.png");
            if (altarSprite != null) sr.sprite = altarSprite;
            sr.sortingOrder = 2;

            var prop = shrineObj.AddComponent<InteractiveProp>();

            // 2D Point Light on the Altar
            var altarLightObj = new GameObject("Altar Point Light 2D");
            altarLightObj.transform.SetParent(shrineObj.transform, false);
            altarLightObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            var pointLight = altarLightObj.AddComponent<Light2D>();
            pointLight.lightType = Light2D.LightType.Point;
            pointLight.color = new Color(1.0f, 0.92f, 0.65f, 1f);
            pointLight.intensity = 1.45f;
            pointLight.pointLightOuterRadius = 6.5f;
            pointLight.pointLightInnerRadius = 0.5f;

            // Wire reward item
            var lily = AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{DataRoot}/Items/Item_CelestialLily.asset");
            var propField = typeof(InteractiveProp).GetField("rewardItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propField != null)
            {
                propField.SetValue(prop, lily);
            }

            return shrineObj;
        }

        private static (GameObject, GameObject, Text, Text, Button) CreateUISetup()
        {
            var uiRoot = new GameObject("--- UI CANVASES ---");

            var canvasObj = new GameObject("MainCanvas");
            canvasObj.transform.SetParent(uiRoot.transform);

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var esObj = new GameObject("EventSystem");
                esObj.transform.SetParent(uiRoot.transform);
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // HUD
            var hudObj = new GameObject("HUD");
            hudObj.transform.SetParent(canvasObj.transform, false);
            hudObj.AddComponent<HUDController>();

            // Dialogue UI
            var diagObj = new GameObject("DialoguePanel");
            diagObj.transform.SetParent(canvasObj.transform, false);
            diagObj.AddComponent<DialogueUI>();

            // Cinematic UI
            var cineObj = new GameObject("CinematicPanel");
            cineObj.transform.SetParent(canvasObj.transform, false);
            var cinUI = cineObj.AddComponent<CinematicUI>();

            // Subtitle elements for Solstice sequence
            var subCanvas = new GameObject("Solstice_Subtitles");
            subCanvas.transform.SetParent(canvasObj.transform, false);
            var subBg = subCanvas.AddComponent<Image>();
            subBg.color = new Color(0.04f, 0.05f, 0.10f, 0.75f);
            var subRt = subCanvas.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.1f, 0.04f);
            subRt.anchorMax = new Vector2(0.9f, 0.22f);
            subRt.offsetMin = Vector2.zero;
            subRt.offsetMax = Vector2.zero;

            var spkObj = new GameObject("SpeakerText");
            spkObj.transform.SetParent(subCanvas.transform, false);
            var spkText = spkObj.AddComponent<Text>();
            spkText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            spkText.fontSize = 24;
            spkText.fontStyle = FontStyle.Bold;
            spkText.color = new Color(1.0f, 0.88f, 0.45f);
            var spkRt = spkObj.GetComponent<RectTransform>();
            spkRt.anchorMin = new Vector2(0.04f, 0.70f);
            spkRt.anchorMax = new Vector2(0.50f, 0.95f);
            spkRt.offsetMin = Vector2.zero;
            spkRt.offsetMax = Vector2.zero;

            var txtObj = new GameObject("DialogueText");
            txtObj.transform.SetParent(subCanvas.transform, false);
            var subText = txtObj.AddComponent<Text>();
            subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subText.fontSize = 20;
            subText.color = Color.white;
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0.04f, 0.10f);
            txtRt.anchorMax = new Vector2(0.96f, 0.68f);
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            // Skip Button
            var skipObj = new GameObject("SkipButton");
            skipObj.transform.SetParent(subCanvas.transform, false);
            var skipImg = skipObj.AddComponent<Image>();
            skipImg.color = new Color(0.2f, 0.25f, 0.4f, 0.85f);
            var skipBtn = skipObj.AddComponent<Button>();
            var skipRt = skipObj.GetComponent<RectTransform>();
            skipRt.anchorMin = new Vector2(0.85f, 0.72f);
            skipRt.anchorMax = new Vector2(0.98f, 0.95f);
            skipRt.offsetMin = Vector2.zero;
            skipRt.offsetMax = Vector2.zero;

            var skipTxtObj = new GameObject("SkipText");
            skipTxtObj.transform.SetParent(skipObj.transform, false);
            var skipTxt = skipTxtObj.AddComponent<Text>();
            skipTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            skipTxt.text = "SKIP >>";
            skipTxt.alignment = TextAnchor.MiddleCenter;
            skipTxt.fontSize = 16;
            skipTxt.color = Color.white;
            var sTxtRt = skipTxtObj.GetComponent<RectTransform>();
            sTxtRt.anchorMin = Vector2.zero;
            sTxtRt.anchorMax = Vector2.one;
            sTxtRt.offsetMin = Vector2.zero;
            sTxtRt.offsetMax = Vector2.zero;

            subCanvas.SetActive(false);

            // Relationship Journal UI
            var relObj = new GameObject("RelationshipJournalPanel");
            relObj.transform.SetParent(canvasObj.transform, false);
            relObj.AddComponent<RelationshipJournalUI>();

            // Quest Journal UI
            var questObj = new GameObject("QuestJournalPanel");
            questObj.transform.SetParent(canvasObj.transform, false);
            questObj.AddComponent<QuestJournalUI>();

            // Inventory UI
            var invObj = new GameObject("InventoryPanel");
            invObj.transform.SetParent(canvasObj.transform, false);
            invObj.AddComponent<InventoryUI>();

            // Gallery UI
            var galObj = new GameObject("GalleryPanel");
            galObj.transform.SetParent(canvasObj.transform, false);
            galObj.AddComponent<GalleryUI>();

            // Save Load UI
            var saveObj = new GameObject("SaveLoadPanel");
            saveObj.transform.SetParent(canvasObj.transform, false);
            saveObj.AddComponent<SaveLoadUI>();

            return (canvasObj, subCanvas, subText, spkText, skipBtn);
        }

        private static void CreateCinematicSequenceSetup(
            CameraController2D camCtrl,
            CharacterVisualRig aureliaRig,
            CharacterVisualRig echoRig,
            GameObject overlayCanvas,
            Text subText,
            Text spkText,
            Button skipBtn)
        {
            var cineRoot = new GameObject("--- CINEMATICS SEQUENCE ---");

            var wideAnchor = new GameObject("Anchor_WideShot");
            wideAnchor.transform.SetParent(cineRoot.transform);
            wideAnchor.transform.position = new Vector3(0, 1.8f, -10f);

            var closeAnchor = new GameObject("Anchor_AureliaCloseUp");
            closeAnchor.transform.SetParent(cineRoot.transform);
            closeAnchor.transform.position = new Vector3(3.8f, 0.4f, -10f);

            var twoAnchor = new GameObject("Anchor_TwoShot");
            twoAnchor.transform.SetParent(cineRoot.transform);
            twoAnchor.transform.position = new Vector3(0.5f, 0.2f, -10f);

            var seqObj = new GameObject("SolsticeCinematicSequence");
            seqObj.transform.SetParent(cineRoot.transform);
            var seq = seqObj.AddComponent<SolsticeCinematicSequence>();

            var cgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/Cinematics/CG_SolsticeVow.png");

            seq.ConfigureSequence(
                camCtrl,
                wideAnchor.transform,
                closeAnchor.transform,
                twoAnchor.transform,
                aureliaRig,
                echoRig,
                overlayCanvas,
                subText,
                spkText,
                skipBtn,
                cgSprite
            );
        }
    }
}
#endif
