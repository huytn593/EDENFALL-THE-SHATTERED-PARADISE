#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Edenfall.Characters;
using Edenfall.Cinematics;
using Edenfall.Core;
using Edenfall.Dialogue;
using Edenfall.Events;
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

            Debug.Log("[EdenfallSceneBuilder] Building First-Person Adult Visual Novel Scene...");

            // 1. First-Person POV Camera & URP Volume
            var (cameraObj, cam2D) = CreatePOVCameraSetup();
            CreateLightingAndVFXSetup();

            // 2. Managers Root (Location, Dialogue, MatureEvent, etc.)
            var (locMgr, matureMgr) = CreateManagersSetup();

            // 3. Environment & Background Staging
            var (bgRenderer, spireRenderer) = CreateEnvironmentSetup();

            // 4. Heroine Staging (Aurelia - Large Center First-Person Scale)
            var (aureliaObj, aureliaRig, aureliaRigCtrl) = CreateAureliaSetup(cameraObj.GetComponent<Camera>());

            // 5. Full Production UI Canvas (Visual Novel Layout)
            var (canvasObj, cinOverlay, cinCGImg, subText, spkText, skipBtn, hotspotContainer, mapModal) = CreateUISetup();

            // 6. Wire Mature Event Manager UI
            WireMatureEventUI(matureMgr, canvasObj);

            // 7. VN Scene Controller
            CreateVNSceneControllerSetup(
                cameraObj.GetComponent<Camera>(),
                cam2D,
                bgRenderer,
                spireRenderer,
                aureliaObj,
                aureliaRig,
                aureliaRigCtrl,
                hotspotContainer
            );

            // 8. Solstice Vow Cinematic Sequence (First-Person POV)
            CreateCinematicSequenceSetup(cam2D, aureliaRig, cinOverlay, cinCGImg, subText, spkText, skipBtn);

            // Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[EdenfallSceneBuilder] First-Person Adult Visual Novel Scene built successfully!");
        }

        private static (GameObject, CameraController2D) CreatePOVCameraSetup()
        {
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0, 0f, -10f);

            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.2f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.16f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            camObj.AddComponent<AudioListener>();
            var camData = camObj.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;

            var cam2D = camObj.AddComponent<CameraController2D>();
            cam2D.SetSpeed(4f);
            cam2D.SetBounds(new Vector2(-15f, -5f), new Vector2(15f, 10f));

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

            // 4. Directional Key Light
            var dirObj = new GameObject("Directional Key Light");
            dirObj.transform.SetParent(lightRoot.transform);
            var dirLight = dirObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(1.0f, 0.88f, 0.70f);
            dirLight.intensity = 1.1f;
            dirObj.transform.rotation = Quaternion.Euler(45f, -30f, 0);
            dirObj.AddComponent<UniversalAdditionalLightData>();

            // 5. Environmental VFX
            var vfxRoot = new GameObject("--- ENVIRONMENTAL VFX ---");
            vfxRoot.transform.SetParent(lightRoot.transform);
            EdenfallVFXBuilder.CreateCelestialSparks(vfxRoot.transform);
            EdenfallVFXBuilder.CreateFallingPetals(vfxRoot.transform);
            EdenfallVFXBuilder.CreateAltarAura(vfxRoot.transform);
        }

        private static (LocationManager, MatureEventManager) CreateManagersSetup()
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

            var logObj = new GameObject("DialogueLogManager");
            logObj.transform.SetParent(mgrRoot.transform);
            logObj.AddComponent<DialogueLogManager>();

            var cineObj = new GameObject("CinematicManager");
            cineObj.transform.SetParent(mgrRoot.transform);
            cineObj.AddComponent<CinematicManager>();

            var galObj = new GameObject("GalleryManager");
            galObj.transform.SetParent(mgrRoot.transform);
            galObj.AddComponent<GalleryManager>();

            // Location Manager
            var lmObj = new GameObject("LocationManager");
            lmObj.transform.SetParent(mgrRoot.transform);
            var locMgr = lmObj.AddComponent<LocationManager>();

            var locGarden = AssetDatabase.LoadAssetAtPath<LocationDefinition>($"{DataRoot}/Locations/Location_DawnGarden.asset");
            var locAltar = AssetDatabase.LoadAssetAtPath<LocationDefinition>($"{DataRoot}/Locations/Location_CelestialAltar.asset");
            var locBalcony = AssetDatabase.LoadAssetAtPath<LocationDefinition>($"{DataRoot}/Locations/Location_SolsticeBalcony.asset");
            var locChamber = AssetDatabase.LoadAssetAtPath<LocationDefinition>($"{DataRoot}/Locations/Location_AureliaChamber.asset");

            var allLocs = new List<LocationDefinition> { locGarden, locAltar, locBalcony, locChamber };
            SetPrivateField(locMgr, "allLocations", allLocs);
            SetPrivateField(locMgr, "currentLocation", locGarden);

            // Mature Event Manager
            var memObj = new GameObject("MatureEventManager");
            memObj.transform.SetParent(mgrRoot.transform);
            var matureMgr = memObj.AddComponent<MatureEventManager>();

            var evtIntimacy = AssetDatabase.LoadAssetAtPath<MatureEventDefinition>($"{DataRoot}/Events/Event_MorningIntimacy.asset");
            if (evtIntimacy != null)
            {
                SetPrivateField(matureMgr, "allEvents", new List<MatureEventDefinition> { evtIntimacy });
            }

            // NPC Schedule Controller (Phase 3.5 Discrete Slot Scheduling & Interaction Locking)
            var schedObj = new GameObject("NPCScheduleController");
            schedObj.transform.SetParent(mgrRoot.transform);
            var schedCtrl = schedObj.AddComponent<NPCScheduleController>();
            var aureliaSched = AssetDatabase.LoadAssetAtPath<NPCSchedule>($"{DataRoot}/Schedules/Schedule_Aurelia.asset");
            SetPrivateField(schedCtrl, "aureliaSchedule", aureliaSched);

            return (locMgr, matureMgr);
        }

        private static (SpriteRenderer, SpriteRenderer) CreateEnvironmentSetup()
        {
            var envRoot = new GameObject("--- ENVIRONMENT ---");

            // Illustrated Full-Screen Background
            var bgObj = new GameObject("VN_Background");
            bgObj.transform.SetParent(envRoot.transform);
            bgObj.transform.position = new Vector3(0, 0f, 5f);
            bgObj.transform.localScale = new Vector3(3.2f, 3.2f, 1f);

            var srBg = bgObj.AddComponent<SpriteRenderer>();
            var gardenSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/Environment/Sanctuary_MarblePlatform.png");
            if (gardenSprite != null) srBg.sprite = gardenSprite;
            srBg.sortingOrder = -20;

            // Optional Distant Spire
            var spireObj = new GameObject("VN_DistantSpire");
            spireObj.transform.SetParent(envRoot.transform);
            spireObj.transform.position = new Vector3(-3f, 1.5f, 10f);
            spireObj.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

            var srSpire = spireObj.AddComponent<SpriteRenderer>();
            var spireSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/Environment/Spire_SolsticeDistant.png");
            if (spireSprite != null) srSpire.sprite = spireSprite;
            srSpire.sortingOrder = -25;

            return (srBg, srSpire);
        }

        private static (GameObject, CharacterVisualRig, AureliaRigController) CreateAureliaSetup(Camera mainCam)
        {
            var charRoot = GameObject.Find("--- HEROINE STAGING ---") ?? new GameObject("--- HEROINE STAGING ---");

            var aureliaObj = new GameObject("Heroine_Aurelia");
            aureliaObj.transform.SetParent(charRoot.transform);

            // Master Production Visual Novel Sprites (Zero Frankenstein collage pieces!)
            string charRootArt = $"{ArtRoot}/Characters/Aurelia";
            var portraitSp = AssetDatabase.LoadAssetAtPath<Sprite>($"{charRootArt}/Aurelia_VN_Portrait.png");
            var fullBodySp = AssetDatabase.LoadAssetAtPath<Sprite>($"{charRootArt}/Aurelia_VN_FullBody.png");

            // Base Body Renderer
            var baseR = aureliaObj.AddComponent<SpriteRenderer>();
            baseR.sprite = portraitSp != null ? portraitSp : fullBodySp;
            baseR.sortingOrder = 8;

            // Anchor & Alignment System (Phase 3.5 Grounding & Anti-Floating Remediation)
            var anchor = aureliaObj.AddComponent<NPCStageAnchor>();

            // Setup CharacterVisualRig
            var rig = aureliaObj.AddComponent<CharacterVisualRig>();
            rig.SetMasterSprites(portraitSp, fullBodySp, baseR, anchor);

            // Align mathematically to stage baseline
            anchor.AlignCharacter(aureliaObj.transform, baseR, StagingMode.DialoguePortrait, mainCam);
            rig.CaptureBaseTransform();

            // Setup AureliaRigController
            var rigCtrl = aureliaObj.AddComponent<AureliaRigController>();

            // Setup NPC Controller
            var npc = aureliaObj.AddComponent<NPCController>();
            var aureliaDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>($"{DataRoot}/Characters/Character_Aurelia.asset");
            var aureliaDiag = AssetDatabase.LoadAssetAtPath<DialogueTree>($"{DataRoot}/Dialogue/DialogueTree_Aurelia.asset");
            var aureliaSchedule = AssetDatabase.LoadAssetAtPath<NPCSchedule>($"{DataRoot}/Schedules/Schedule_Aurelia.asset");

            npc.Initialize(aureliaDef, aureliaSchedule, aureliaDiag);

            return (aureliaObj, rig, rigCtrl);
        }

        private static (GameObject, GameObject, Image, Text, Text, Button, Transform, GameObject) CreateUISetup()
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

            // 1. Top HUD Bar (Celestial Navigation & Status)
            var hudObj = new GameObject("HUD");
            hudObj.transform.SetParent(canvasObj.transform, false);
            var hud = hudObj.AddComponent<HUDController>();

            // 2. First-Person Visual Novel Dialogue Panel (Matching Reference Image)
            var diagObj = new GameObject("DialoguePanel");
            diagObj.transform.SetParent(canvasObj.transform, false);
            var diagCg = diagObj.AddComponent<CanvasGroup>();
            var diagUI = diagObj.AddComponent<DialogueUI>();

            // Bottom dialogue box frame
            var boxObj = new GameObject("DialogueBoxFrame");
            boxObj.transform.SetParent(diagObj.transform, false);
            var boxImg = boxObj.AddComponent<Image>();
            boxImg.color = new Color(0.04f, 0.06f, 0.12f, 0.88f);
            var boxBtn = boxObj.AddComponent<Button>();
            boxBtn.onClick.AddListener(diagUI.OnClickDialogueBox);

            var boxRt = boxObj.GetComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.06f, 0.03f);
            boxRt.anchorMax = new Vector2(0.94f, 0.28f);
            boxRt.offsetMin = Vector2.zero;
            boxRt.offsetMax = Vector2.zero;

            // Nameplate Badge
            var badgeObj = new GameObject("NameBadge");
            badgeObj.transform.SetParent(boxObj.transform, false);
            var badgeImg = badgeObj.AddComponent<Image>();
            badgeImg.color = new Color(0.12f, 0.16f, 0.28f, 0.95f);
            var badgeRt = badgeObj.GetComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0.02f, 0.74f);
            badgeRt.anchorMax = new Vector2(0.24f, 0.96f);
            badgeRt.offsetMin = Vector2.zero;
            badgeRt.offsetMax = Vector2.zero;

            var nameTextObj = new GameObject("SpeakerName");
            nameTextObj.transform.SetParent(badgeObj.transform, false);
            var spkNameText = nameTextObj.AddComponent<Text>();
            spkNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            spkNameText.fontSize = 22;
            spkNameText.fontStyle = FontStyle.Bold;
            spkNameText.color = new Color(1.0f, 0.88f, 0.45f);
            spkNameText.alignment = TextAnchor.MiddleCenter;
            var nameRt = nameTextObj.GetComponent<RectTransform>();
            nameRt.anchorMin = Vector2.zero;
            nameRt.anchorMax = Vector2.one;
            nameRt.offsetMin = Vector2.zero;
            nameRt.offsetMax = Vector2.zero;

            // Dialogue Body Text
            var bodyTextObj = new GameObject("DialogueBody");
            bodyTextObj.transform.SetParent(boxObj.transform, false);
            var diagBodyText = bodyTextObj.AddComponent<Text>();
            diagBodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            diagBodyText.fontSize = 22;
            diagBodyText.lineSpacing = 1.25f;
            diagBodyText.color = new Color(0.95f, 0.96f, 1.0f);
            var bodyRt = bodyTextObj.GetComponent<RectTransform>();
            bodyRt.anchorMin = new Vector2(0.03f, 0.10f);
            bodyRt.anchorMax = new Vector2(0.78f, 0.70f);
            bodyRt.offsetMin = Vector2.zero;
            bodyRt.offsetMax = Vector2.zero;

            // Action Buttons in Dialogue Box (Skip, Auto, Log, Hide)
            var (autoBtn, autoTxt) = CreateVNActionButton(boxObj.transform, "Auto", new Vector2(0.80f, 0.12f), new Vector2(0.88f, 0.36f));
            var (skipBtnUI, skipTxt) = CreateVNActionButton(boxObj.transform, "Skip", new Vector2(0.89f, 0.12f), new Vector2(0.98f, 0.36f));
            var (logBtn, logTxt) = CreateVNActionButton(boxObj.transform, "Log", new Vector2(0.80f, 0.42f), new Vector2(0.88f, 0.66f));
            var (hideBtn, hideTxt) = CreateVNActionButton(boxObj.transform, "Hide", new Vector2(0.89f, 0.42f), new Vector2(0.98f, 0.66f));

            // Choices Container
            var choicesContainer = new GameObject("ChoicesContainer");
            choicesContainer.transform.SetParent(diagObj.transform, false);
            var cRt = choicesContainer.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.20f, 0.32f);
            cRt.anchorMax = new Vector2(0.80f, 0.68f);
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;
            var vlg = choicesContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandHeight = false;

            // Wire DialogueUI fields
            SetPrivateField(diagUI, "dialoguePanel", diagObj);
            SetPrivateField(diagUI, "panelCanvasGroup", diagCg);
            SetPrivateField(diagUI, "speakerNameText", spkNameText);
            SetPrivateField(diagUI, "nameBadgeObj", badgeObj);
            SetPrivateField(diagUI, "dialogueBodyText", diagBodyText);
            SetPrivateField(diagUI, "choicesContainer", choicesContainer.transform);
            SetPrivateField(diagUI, "autoPlayButton", autoBtn);
            SetPrivateField(diagUI, "autoPlayText", autoTxt);
            SetPrivateField(diagUI, "skipButton", skipBtnUI);
            SetPrivateField(diagUI, "skipText", skipTxt);
            SetPrivateField(diagUI, "logButton", logBtn);
            SetPrivateField(diagUI, "hideUIButton", hideBtn);

            diagObj.SetActive(false);

            // 3. Hotspots Container (Left-floating interaction actions)
            var hotspotCanvas = new GameObject("HotspotsCanvas");
            hotspotCanvas.transform.SetParent(canvasObj.transform, false);
            var hRt = hotspotCanvas.AddComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0.04f, 0.35f);
            hRt.anchorMax = new Vector2(0.32f, 0.72f);
            hRt.offsetMin = Vector2.zero;
            hRt.offsetMax = Vector2.zero;
            var hVlg = hotspotCanvas.AddComponent<VerticalLayoutGroup>();
            hVlg.spacing = 12f;
            hVlg.childAlignment = TextAnchor.UpperLeft;
            hVlg.childForceExpandHeight = false;

            // 4. Location Map Modal
            var mapModal = CreateLocationMapModal(canvasObj.transform);
            SetPrivateField(hud, "locationMapModal", mapModal);

            // 5. Cinematic Overlay Canvas & Subtitles
            var cinCanvas = new GameObject("CinematicOverlay");
            cinCanvas.transform.SetParent(canvasObj.transform, false);
            var cinRt = cinCanvas.AddComponent<RectTransform>();
            cinRt.anchorMin = Vector2.zero;
            cinRt.anchorMax = Vector2.one;
            cinRt.offsetMin = Vector2.zero;
            cinRt.offsetMax = Vector2.zero;

            var cgImgObj = new GameObject("FullScreenCG");
            cgImgObj.transform.SetParent(cinCanvas.transform, false);
            var cinCGImg = cgImgObj.AddComponent<Image>();
            var cgRt = cgImgObj.GetComponent<RectTransform>();
            cgRt.anchorMin = Vector2.zero;
            cgRt.anchorMax = Vector2.one;
            cgRt.offsetMin = Vector2.zero;
            cgRt.offsetMax = Vector2.zero;
            cgImgObj.SetActive(false);

            var subCanvas = new GameObject("Subtitles");
            subCanvas.transform.SetParent(cinCanvas.transform, false);
            var subBg = subCanvas.AddComponent<Image>();
            subBg.color = new Color(0.04f, 0.05f, 0.10f, 0.85f);
            var subRt = subCanvas.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.08f, 0.04f);
            subRt.anchorMax = new Vector2(0.92f, 0.22f);
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
            var sTxtRt = txtObj.GetComponent<RectTransform>();
            sTxtRt.anchorMin = new Vector2(0.04f, 0.10f);
            sTxtRt.anchorMax = new Vector2(0.85f, 0.68f);
            sTxtRt.offsetMin = Vector2.zero;
            sTxtRt.offsetMax = Vector2.zero;

            var (cinSkipBtn, _) = CreateVNActionButton(subCanvas.transform, "SKIP >>", new Vector2(0.86f, 0.65f), new Vector2(0.98f, 0.92f));
            cinCanvas.SetActive(false);

            // 6. Modals
            var relModal = CreateSimpleModal(canvasObj.transform, "RelationshipModal", "NHẬT KÝ TÌNH CẢM (RELATIONSHIPS)");
            relModal.AddComponent<RelationshipJournalUI>();
            SetPrivateField(hud, "relationshipModal", relModal);

            var questModal = CreateSimpleModal(canvasObj.transform, "QuestModal", "SỔ TAY NHIỆM VỤ (QUESTS)");
            questModal.AddComponent<QuestJournalUI>();
            SetPrivateField(hud, "questModal", questModal);

            var invModal = CreateSimpleModal(canvasObj.transform, "InventoryModal", "TÚI ĐỒ (INVENTORY)");
            invModal.AddComponent<InventoryUI>();
            SetPrivateField(hud, "inventoryModal", invModal);

            var galModal = CreateSimpleModal(canvasObj.transform, "GalleryModal", "BỘ SƯU TẬP HOẠT CẢNH (GALLERY)");
            galModal.AddComponent<GalleryUI>();
            SetPrivateField(hud, "galleryModal", galModal);

            var saveModal = CreateSimpleModal(canvasObj.transform, "SaveLoadModal", "LƯU TRỮ TIẾN TRÌNH (SAVE / LOAD)");
            saveModal.AddComponent<SaveLoadUI>();
            SetPrivateField(hud, "saveLoadModal", saveModal);

            // 7. Character Directory Modal (Phase 3.5 NPC Discovery & 1-Click Direct Navigation)
            var charDirModal = CreateSimpleModal(canvasObj.transform, "CharacterDirectoryModal", "DANH BẠ NHÂN VẬT (CHARACTER DIRECTORY)");
            var dirUI = charDirModal.AddComponent<CharacterDirectoryUI>();

            var cardsContainer = new GameObject("DirectoryCardsContainer");
            cardsContainer.transform.SetParent(charDirModal.transform, false);
            var dcRt = cardsContainer.AddComponent<RectTransform>();
            dcRt.anchorMin = new Vector2(0.10f, 0.12f);
            dcRt.anchorMax = new Vector2(0.90f, 0.84f);
            dcRt.offsetMin = Vector2.zero;
            dcRt.offsetMax = Vector2.zero;

            var dVlg = cardsContainer.AddComponent<VerticalLayoutGroup>();
            dVlg.spacing = 15f;
            dVlg.childAlignment = TextAnchor.UpperCenter;
            dVlg.childForceExpandHeight = false;

            var dirCloseBtnObj = charDirModal.transform.Find("CloseButton")?.gameObject;
            var dirCloseBtn = dirCloseBtnObj != null ? dirCloseBtnObj.GetComponent<Button>() : null;

            var aureliaCharDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>($"{DataRoot}/Characters/Character_Aurelia.asset");
            var charList = new List<CharacterDefinition> { aureliaCharDef };

            SetPrivateField(dirUI, "directoryModal", charDirModal);
            SetPrivateField(dirUI, "directoryCardsContainer", cardsContainer.transform);
            SetPrivateField(dirUI, "closeButton", dirCloseBtn);
            SetPrivateField(dirUI, "characters", charList);

            charDirModal.SetActive(false);
            SetPrivateField(hud, "characterDirectoryModal", charDirModal);

            return (canvasObj, cinCanvas, cinCGImg, subText, spkText, cinSkipBtn, hotspotCanvas.transform, mapModal);
        }

        private static void WireMatureEventUI(MatureEventManager mem, GameObject canvasObj)
        {
            if (mem == null || canvasObj == null) return;

            var evtCanvas = new GameObject("MatureEventCanvas");
            evtCanvas.transform.SetParent(canvasObj.transform, false);
            var eRt = evtCanvas.AddComponent<RectTransform>();
            eRt.anchorMin = Vector2.zero;
            eRt.anchorMax = Vector2.one;
            eRt.offsetMin = Vector2.zero;
            eRt.offsetMax = Vector2.zero;

            // Fullscreen CG
            var cgObj = new GameObject("CGImage");
            cgObj.transform.SetParent(evtCanvas.transform, false);
            var cgImg = cgObj.AddComponent<Image>();
            var cgRt = cgObj.GetComponent<RectTransform>();
            cgRt.anchorMin = Vector2.zero;
            cgRt.anchorMax = Vector2.one;
            cgRt.offsetMin = Vector2.zero;
            cgRt.offsetMax = Vector2.zero;

            // Fade Overlay
            var fadeObj = new GameObject("FadeOverlay");
            fadeObj.transform.SetParent(evtCanvas.transform, false);
            var fadeImg = fadeObj.AddComponent<Image>();
            fadeImg.color = new Color(0, 0, 0, 0);
            var fRt = fadeObj.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.offsetMin = Vector2.zero;
            fRt.offsetMax = Vector2.zero;

            // Dialogue Box
            var boxObj = new GameObject("EventBox");
            boxObj.transform.SetParent(evtCanvas.transform, false);
            var boxImg = boxObj.AddComponent<Image>();
            boxImg.color = new Color(0.04f, 0.05f, 0.10f, 0.90f);
            var bRt = boxObj.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.08f, 0.04f);
            bRt.anchorMax = new Vector2(0.92f, 0.24f);
            bRt.offsetMin = Vector2.zero;
            bRt.offsetMax = Vector2.zero;

            var spkObj = new GameObject("Speaker");
            spkObj.transform.SetParent(boxObj.transform, false);
            var spkTxt = spkObj.AddComponent<Text>();
            spkTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            spkTxt.fontSize = 22;
            spkTxt.fontStyle = FontStyle.Bold;
            spkTxt.color = new Color(1.0f, 0.88f, 0.45f);
            var sRt = spkObj.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.03f, 0.72f);
            sRt.anchorMax = new Vector2(0.40f, 0.95f);
            sRt.offsetMin = Vector2.zero;
            sRt.offsetMax = Vector2.zero;

            var diaObj = new GameObject("Dialogue");
            diaObj.transform.SetParent(boxObj.transform, false);
            var diaTxt = diaObj.AddComponent<Text>();
            diaTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            diaTxt.fontSize = 20;
            diaTxt.color = Color.white;
            var dRt = diaObj.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.03f, 0.10f);
            dRt.anchorMax = new Vector2(0.85f, 0.68f);
            dRt.offsetMin = Vector2.zero;
            dRt.offsetMax = Vector2.zero;

            var (nextBtn, _) = CreateVNActionButton(boxObj.transform, "TIẾP >>", new Vector2(0.86f, 0.55f), new Vector2(0.98f, 0.90f));
            var (skipBtn, _) = CreateVNActionButton(boxObj.transform, "SKIP", new Vector2(0.86f, 0.12f), new Vector2(0.98f, 0.48f));

            SetPrivateField(mem, "eventCanvas", evtCanvas);
            SetPrivateField(mem, "fullScreenCGImage", cgImg);
            SetPrivateField(mem, "fadeOverlay", fadeImg);
            SetPrivateField(mem, "eventSpeakerText", spkTxt);
            SetPrivateField(mem, "eventDialogueText", diaTxt);
            SetPrivateField(mem, "nextSlideButton", nextBtn);
            SetPrivateField(mem, "skipEventButton", skipBtn);

            evtCanvas.SetActive(false);
        }

        private static void CreateVNSceneControllerSetup(
            Camera mainCam,
            CameraController2D camCtrl,
            SpriteRenderer bgR,
            SpriteRenderer spireR,
            GameObject aureliaObj,
            CharacterVisualRig rig,
            AureliaRigController rigCtrl,
            Transform hotspotContainer)
        {
            var vnRoot = new GameObject("--- VN SCENE CONTROLLER ---");
            var vnCtrl = vnRoot.AddComponent<VNSceneController>();

            var aureliaDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>($"{DataRoot}/Characters/Character_Aurelia.asset");
            var aureliaTree = AssetDatabase.LoadAssetAtPath<DialogueTree>($"{DataRoot}/Dialogue/DialogueTree_Aurelia.asset");
            var aureliaSched = AssetDatabase.LoadAssetAtPath<NPCSchedule>($"{DataRoot}/Schedules/Schedule_Aurelia.asset");

            SetPrivateField(vnCtrl, "mainCamera", mainCam);
            SetPrivateField(vnCtrl, "cameraController", camCtrl);
            SetPrivateField(vnCtrl, "backgroundRenderer", bgR);
            SetPrivateField(vnCtrl, "distantSpireRenderer", spireR);
            SetPrivateField(vnCtrl, "aureliaRoot", aureliaObj);
            SetPrivateField(vnCtrl, "aureliaRig", rig);
            SetPrivateField(vnCtrl, "aureliaRigController", rigCtrl);
            SetPrivateField(vnCtrl, "aureliaDef", aureliaDef);
            SetPrivateField(vnCtrl, "aureliaDialogueTree", aureliaTree);
            SetPrivateField(vnCtrl, "aureliaSchedule", aureliaSched);
            SetPrivateField(vnCtrl, "hotspotButtonsContainer", hotspotContainer);
        }

        private static void CreateCinematicSequenceSetup(
            CameraController2D camCtrl,
            CharacterVisualRig aureliaRig,
            GameObject overlayCanvas,
            Image cgImg,
            Text subText,
            Text spkText,
            Button skipBtn)
        {
            var cineRoot = new GameObject("--- CINEMATICS SEQUENCE ---");

            var wideAnchor = new GameObject("Anchor_POV_WideShot");
            wideAnchor.transform.SetParent(cineRoot.transform);
            wideAnchor.transform.position = new Vector3(0, 0f, -10f);

            var closeAnchor = new GameObject("Anchor_POV_CloseUp");
            closeAnchor.transform.SetParent(cineRoot.transform);
            closeAnchor.transform.position = new Vector3(0.4f, 0.2f, -10f);

            var intimateAnchor = new GameObject("Anchor_POV_Intimate");
            intimateAnchor.transform.SetParent(cineRoot.transform);
            intimateAnchor.transform.position = new Vector3(0.4f, 0.4f, -10f);

            var seqObj = new GameObject("SolsticeCinematicSequence");
            seqObj.transform.SetParent(cineRoot.transform);
            var seq = seqObj.AddComponent<SolsticeCinematicSequence>();

            var cgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/Cinematics/CG_SolsticeVow.png");

            seq.ConfigureSequence(
                camCtrl,
                wideAnchor.transform,
                closeAnchor.transform,
                intimateAnchor.transform,
                aureliaRig,
                overlayCanvas,
                cgImg,
                subText,
                spkText,
                skipBtn,
                cgSprite
            );
        }

        private static GameObject CreateLocationMapModal(Transform canvasTransform)
        {
            var modal = CreateSimpleModal(canvasTransform, "LocationMapModal", "BẢN ĐỒ THÁNH ĐỊA (LOCATION MAP)");
            var mapUI = modal.AddComponent<LocationMapUI>();

            // Location cards container
            var container = new GameObject("CardsContainer");
            container.transform.SetParent(modal.transform, false);
            var cRt = container.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.12f, 0.12f);
            cRt.anchorMax = new Vector2(0.88f, 0.82f);
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;

            var vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandHeight = false;

            var closeBtnObj = modal.transform.Find("CloseButton")?.gameObject;
            var closeBtn = closeBtnObj != null ? closeBtnObj.GetComponent<Button>() : null;

            var sched = AssetDatabase.LoadAssetAtPath<NPCSchedule>($"{DataRoot}/Schedules/Schedule_Aurelia.asset");

            SetPrivateField(mapUI, "mapModal", modal);
            SetPrivateField(mapUI, "locationCardsContainer", container.transform);
            SetPrivateField(mapUI, "closeButton", closeBtn);
            SetPrivateField(mapUI, "aureliaSchedule", sched);

            return modal;
        }

        private static GameObject CreateSimpleModal(Transform parent, string name, string title)
        {
            var modalObj = new GameObject(name);
            modalObj.transform.SetParent(parent, false);

            var bg = modalObj.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.14f, 0.94f);

            var rt = modalObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.15f, 0.10f);
            rt.anchorMax = new Vector2(0.85f, 0.90f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Title
            var titleObj = new GameObject("HeaderTitle");
            titleObj.transform.SetParent(modalObj.transform, false);
            var titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.text = title;
            titleTxt.fontSize = 24;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(1f, 0.88f, 0.45f);
            titleTxt.alignment = TextAnchor.MiddleCenter;

            var tRt = titleObj.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.05f, 0.88f);
            tRt.anchorMax = new Vector2(0.95f, 0.98f);
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;

            // Close Button
            var closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(modalObj.transform, false);
            var closeImg = closeObj.AddComponent<Image>();
            closeImg.color = new Color(0.35f, 0.15f, 0.15f, 0.9f);
            var closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => modalObj.SetActive(false));

            var cRt = closeObj.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.92f, 0.90f);
            cRt.anchorMax = new Vector2(0.98f, 0.98f);
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;

            var xObj = new GameObject("XText");
            xObj.transform.SetParent(closeObj.transform, false);
            var xTxt = xObj.AddComponent<Text>();
            xTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            xTxt.text = "X";
            xTxt.fontSize = 20;
            xTxt.color = Color.white;
            xTxt.alignment = TextAnchor.MiddleCenter;
            var xRt = xObj.GetComponent<RectTransform>();
            xRt.anchorMin = Vector2.zero;
            xRt.anchorMax = Vector2.one;
            xRt.offsetMin = Vector2.zero;
            xRt.offsetMax = Vector2.zero;

            modalObj.SetActive(false);
            return modalObj;
        }

        private static (Button, Text) CreateVNActionButton(Transform parent, string label, Vector2 min, Vector2 max)
        {
            var btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.14f, 0.18f, 0.30f, 0.85f);

            var btn = btnObj.AddComponent<Button>();
            var rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            var txt = txtObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = label;
            txt.fontSize = 17;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            var tRt = txtObj.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;

            return (btn, txt);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null) return;
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[EdenfallSceneBuilder] Field '{fieldName}' not found on {target.GetType().Name}");
            }
        }
    }
}
#endif
