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
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Edenfall.Editor
{
    public static class EdenfallSceneBuilder
    {
        private const string ScenePath = "Assets/_Edenfall/Scenes/DawnSanctuary/DawnSanctuary.unity";
        private const string DataRoot = "Assets/_Edenfall/Data";

        [MenuItem("Edenfall/Build Dawn Sanctuary Scene")]
        public static void BuildDawnSanctuaryScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Clean existing scene root objects if any
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                Object.DestroyImmediate(root);
            }

            Debug.Log("[EdenfallSceneBuilder] Building Dawn Sanctuary production scene...");

            // 1. Camera & Lighting
            var (cameraObj, cam2D) = CreateCameraSetup();
            CreateLightingSetup();

            // 2. Managers Root
            CreateManagersSetup();

            // 3. Environment & Terrain Platforms
            CreateEnvironmentSetup();

            // 4. Player Character
            var playerObj = CreatePlayerSetup();
            if (cam2D != null && playerObj != null)
            {
                cam2D.SetTarget(playerObj.transform);
            }

            // 5. NPC Aurelia
            CreateAureliaSetup();

            // 6. Interactive Shrines & Props
            CreateInteractivePropsSetup();

            // 7. Full Production UI Canvas
            CreateUISetup();

            // Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[EdenfallSceneBuilder] Dawn Sanctuary scene built and saved successfully!");
        }

        private static (GameObject, CameraController2D) CreateCameraSetup()
        {
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0, 2f, -10f);

            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.5f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.15f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            var cam2D = camObj.AddComponent<CameraController2D>();
            cam2D.SetSpeed(4f);
            cam2D.SetBounds(new Vector2(-25f, -2f), new Vector2(25f, 15f));

            return (camObj, cam2D);
        }

        private static void CreateLightingSetup()
        {
            var lightObj = new GameObject("Celestial Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.intensity = 1.15f;
            lightObj.transform.rotation = Quaternion.Euler(45f, -30f, 0);

            lightObj.AddComponent<UniversalAdditionalLightData>();
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

            // Main Marble Sanctuary Platform
            var groundObj = new GameObject("Sanctuary_MainGround");
            groundObj.transform.SetParent(envRoot.transform);
            groundObj.transform.position = new Vector3(0, -1f, 0);

            var groundCol = groundObj.AddComponent<BoxCollider2D>();
            groundCol.size = new Vector2(50f, 2f);

            var groundSr = groundObj.AddComponent<SpriteRenderer>();
            groundSr.sprite = CreateBoxSprite(500, 20, new Color(0.92f, 0.90f, 0.85f, 1f));
            groundSr.sortingOrder = 0;

            // Upper Floating Balcony
            var balconyObj = new GameObject("Sanctuary_CelestialBalcony");
            balconyObj.transform.SetParent(envRoot.transform);
            balconyObj.transform.position = new Vector3(8f, 3.5f, 0);

            var balconyCol = balconyObj.AddComponent<BoxCollider2D>();
            balconyCol.size = new Vector2(12f, 0.8f);

            var balconySr = balconyObj.AddComponent<SpriteRenderer>();
            balconySr.sprite = CreateBoxSprite(120, 8, new Color(0.85f, 0.88f, 0.95f, 1f));
            balconySr.sortingOrder = 0;

            // Parallax Background Layers
            var parallaxRoot = new GameObject("Parallax_Backgrounds");
            parallaxRoot.transform.SetParent(envRoot.transform);

            // Far Sky (Scroll Speed 0.05)
            var farSky = new GameObject("Layer_FarSky");
            farSky.transform.SetParent(parallaxRoot.transform);
            farSky.transform.position = new Vector3(0, 5f, 5f);
            var p1 = farSky.AddComponent<ParallaxLayer>();
            p1.SetMultiplier(new Vector2(0.05f, 0.02f));
            var sr1 = farSky.AddComponent<SpriteRenderer>();
            sr1.sprite = CreateBoxSprite(80, 40, new Color(0.12f, 0.15f, 0.28f, 1f));
            sr1.sortingOrder = -20;

            // Distant Spire (Scroll Speed 0.2)
            var distantSpire = new GameObject("Layer_SolsticeSpire");
            distantSpire.transform.SetParent(parallaxRoot.transform);
            distantSpire.transform.position = new Vector3(-6f, 4f, 2f);
            var p2 = distantSpire.AddComponent<ParallaxLayer>();
            p2.SetMultiplier(new Vector2(0.2f, 0.1f));
            var sr2 = distantSpire.AddComponent<SpriteRenderer>();
            sr2.sprite = CreateBoxSprite(16, 60, new Color(0.24f, 0.28f, 0.44f, 0.85f));
            sr2.sortingOrder = -10;
        }

        private static GameObject CreatePlayerSetup()
        {
            var charRoot = GameObject.Find("--- CHARACTERS ---") ?? new GameObject("--- CHARACTERS ---");

            var player = new GameObject("Player");
            player.transform.SetParent(charRoot.transform);
            player.transform.position = new Vector3(-4f, 1f, 0);

            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            var col = player.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.8f);
            col.offset = new Vector2(0, 0.9f);

            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBoxSprite(8, 18, new Color(0.28f, 0.52f, 0.92f, 1f));
            sr.sortingOrder = 10;

            player.AddComponent<PlayerController2D>();

            return player;
        }

        private static void CreateAureliaSetup()
        {
            var charRoot = GameObject.Find("--- CHARACTERS ---") ?? new GameObject("--- CHARACTERS ---");

            var npcObj = new GameObject("NPC_Aurelia");
            npcObj.transform.SetParent(charRoot.transform);
            npcObj.transform.position = new Vector3(4f, 1f, 0);

            var col = npcObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 1.8f);
            col.offset = new Vector2(0, 0.9f);
            col.isTrigger = true;

            var sr = npcObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBoxSprite(8, 18, new Color(0.95f, 0.78f, 0.42f, 1f));
            sr.sortingOrder = 9;

            var npc = npcObj.AddComponent<NPCController>();

            // Load generated assets
            var aureliaDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>($"{DataRoot}/Characters/Character_Aurelia.asset");
            var aureliaDiag = AssetDatabase.LoadAssetAtPath<DialogueTree>($"{DataRoot}/Dialogue/DialogueTree_Aurelia.asset");
            var aureliaSchedule = AssetDatabase.LoadAssetAtPath<NPCSchedule>($"{DataRoot}/Schedules/Schedule_Aurelia.asset");

            npc.Initialize(aureliaDef, aureliaSchedule, aureliaDiag);
        }

        private static void CreateInteractivePropsSetup()
        {
            var propRoot = new GameObject("--- INTERACTIVE PROPS ---");

            var shrineObj = new GameObject("Prop_CelestialAltar");
            shrineObj.transform.SetParent(propRoot.transform);
            shrineObj.transform.position = new Vector3(10f, 4.5f, 0);

            var col = shrineObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 2f);
            col.isTrigger = true;

            var sr = shrineObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBoxSprite(20, 20, new Color(0.98f, 0.88f, 0.35f, 1f));
            sr.sortingOrder = 5;

            var prop = shrineObj.AddComponent<InteractiveProp>();

            // Wire reward item
            var lily = AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{DataRoot}/Items/Item_CelestialLily.asset");
            var propField = typeof(InteractiveProp).GetField("rewardItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propField != null)
            {
                propField.SetValue(prop, lily);
            }
        }

        private static void CreateUISetup()
        {
            var uiRoot = new GameObject("--- UI CANVASES ---");

            var canvasObj = new GameObject("MainCanvas");
            canvasObj.transform.SetParent(uiRoot.transform);

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
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
            cineObj.AddComponent<CinematicUI>();

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
        }

        private static Sprite CreateBoxSprite(int width, int height, Color color)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
#endif
