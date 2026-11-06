#if UNITY_EDITOR
using System.IO;
using Edenfall.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Edenfall.Editor
{
    public static class AureliaRigQAViewer
    {
        private const string OutputDir = "Temp/qa_captures";
        private const string ScenePath = "Assets/_Edenfall/Scenes/AureliaRigDiagnostic.unity";

        [MenuItem("Edenfall/Capture All Rig QA Evidence")]
        public static void CaptureAllQAEvidence()
        {
            if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);

            AssetDatabase.Refresh();
            AureliaRigBuilder.BuildRiggedPrefab();
            AureliaRigBuilder.CreateDiagnosticScene();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject charObj = GameObject.Find("Aurelia_Rigged_Character");
            if (charObj == null)
            {
                Debug.LogError("[AureliaRigQAViewer] Aurelia_Rigged_Character not found in scene!");
                return;
            }

            var rig = charObj.GetComponent<AureliaRiggedCharacter>();
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[AureliaRigQAViewer] Main Camera not found!");
                return;
            }

            // Set fixed 1920x1080 resolution
            int w = 1920;
            int h = 1080;

            Texture2D Capture(Camera activeCam, string filename)
            {
                if (activeCam == null) activeCam = Camera.main;
                RenderTexture rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
                activeCam.targetTexture = rt;
                activeCam.Render();
                RenderTexture.active = rt;

                Texture2D screenShot = new Texture2D(w, h, TextureFormat.RGB24, false);
                screenShot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                screenShot.Apply();

                activeCam.targetTexture = null;
                RenderTexture.active = null;
                Object.DestroyImmediate(rt);

                byte[] bytes = screenShot.EncodeToPNG();
                string path = Path.Combine(OutputDir, filename);
                File.WriteAllBytes(path, bytes);
                Debug.Log($"[AureliaRigQAViewer] Saved QA capture: {path}");
                return screenShot;
            }

            // 1. Static Half-Body Dialogue Framing (Neutral)
            rig.SetAnimationState(AureliaAnimationState.Idle_Normal, true);
            rig.SetEyesClosed(false);
            rig.SetTalking(false);
            cam.orthographicSize = 5.0f;
            cam.transform.position = new Vector3(0, 0, -10f);
            Capture(cam, "qa_01_static_halfbody_neutral.png");

            // 2. Eyes Open vs Closed (Face Close-Up)
            cam.orthographicSize = 2.4f;
            cam.transform.position = new Vector3(0.05f, 1.4f, -10f); // Zoom in on face

            rig.SetEyesClosed(false);
            Capture(cam, "qa_02_face_eyes_open.png");

            rig.SetEyesClosed(true);
            Capture(cam, "qa_03_face_eyes_closed.png");

            // 3. Mouth Closed vs Open vs Smile vs Wide
            rig.SetEyesClosed(false);
            rig.SetTalking(false);
            rig.SetMouthViseme(0);
            Capture(cam, "qa_04_face_mouth_closed.png");

            rig.SetMouthViseme(2);
            Capture(cam, "qa_05_face_mouth_open.png");

            rig.SetMouthViseme(0);

            // 4. Head Neutral vs Tilted (Shy & Romantic)
            cam.orthographicSize = 4.8f;
            cam.transform.position = new Vector3(0, 0.2f, -10f);

            rig.SetTalking(false);
            rig.SetAnimationState(AureliaAnimationState.Idle_Normal, true);
            Capture(cam, "qa_06_head_neutral.png");

            rig.SetAnimationState(AureliaAnimationState.Idle_Shy, true);
            Capture(cam, "qa_07_head_tilted_shy.png");

            rig.SetAnimationState(AureliaAnimationState.React_Romantic, true);
            Capture(cam, "qa_08_head_tilted_romantic.png");

            // 5. Surprised / Excited Reaction
            rig.SetAnimationState(AureliaAnimationState.React_Surprised, true);
            Capture(cam, "qa_09_react_surprised.png");

            // Reset to Neutral
            rig.SetAnimationState(AureliaAnimationState.Idle_Normal, true);

            // 6. Visual Novel Dialogue Staging in Dawn Sanctuary (Matching User Reference)
            string sanctuaryPath = "Assets/_Edenfall/Scenes/DawnSanctuary/DawnSanctuary.unity";
            if (File.Exists(sanctuaryPath))
            {
                var sanctuaryScene = EditorSceneManager.OpenScene(sanctuaryPath, OpenSceneMode.Single);
                var vnCam = Camera.main;
                if (vnCam != null)
                {
                    // Find MainCanvas specifically (avoid secondary inactive canvases)
                    var canvasObj = GameObject.Find("MainCanvas");
                    if (canvasObj == null)
                    {
                        var uiRoot = GameObject.Find("--- UI CANVASES ---");
                        if (uiRoot != null) canvasObj = uiRoot.transform.Find("MainCanvas")?.gameObject;
                    }
                    var canvas = canvasObj != null ? canvasObj.GetComponent<Canvas>() : Object.FindFirstObjectByType<Canvas>();
                    var prevRenderMode = canvas != null ? canvas.renderMode : RenderMode.ScreenSpaceOverlay;
                    int prevSortingOrder = canvas != null ? canvas.sortingOrder : 0;
                    if (canvas != null)
                    {
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        canvas.worldCamera = vnCam;
                        canvas.planeDistance = 1.0f;
                        canvas.sortingOrder = 100;
                        Canvas.ForceUpdateCanvases();
                    }

                    // Activate and configure dialogue box for reference capture
                    var diagPanel = canvasObj != null ? canvasObj.transform.Find("DialoguePanel")?.gameObject : null;
                    if (diagPanel != null) diagPanel.SetActive(true);
                    var diagBox = canvasObj != null ? canvasObj.transform.Find("DialoguePanel/DialogueBoxFrame")?.gameObject : null;
                    if (diagBox != null)
                    {
                        diagBox.SetActive(true);
                        var spkText = diagBox.transform.Find("NameBadge/SpeakerName")?.GetComponent<UnityEngine.UI.Text>();
                        if (spkText != null) spkText.text = "Aurelia";
                        var bodyText = diagBox.transform.Find("DialogueBody")?.GetComponent<UnityEngine.UI.Text>();
                        if (bodyText != null)
                        {
                            bodyText.text = "Good morning, my sweet. Are you talking to yourself? You finally decided to wake up and give me some attention.";
                        }
                    }

                    // Set character state to gentle smile
                    var dsChar = GameObject.Find("Heroine_Aurelia")?.GetComponent<AureliaRiggedCharacter>();
                    if (dsChar != null)
                    {
                        dsChar.SetAnimationState(AureliaAnimationState.Idle_Happy, true);
                    }

                    Canvas.ForceUpdateCanvases();
                    Capture(vnCam, "qa_10_visual_novel_dialogue_staging.png");

                    // 7. Full-Screen Cinematic CG Event (First Meeting)
                    var cineOverlay = canvasObj != null ? canvasObj.transform.Find("CinematicOverlay")?.gameObject : null;
                    if (cineOverlay != null)
                    {
                        if (diagBox != null) diagBox.SetActive(false);
                        cineOverlay.SetActive(true);
                        var cgObj = cineOverlay.transform.Find("FullScreenCG")?.gameObject;
                        if (cgObj != null)
                        {
                            cgObj.SetActive(true);
                            var cgImg = cgObj.GetComponent<UnityEngine.UI.Image>();
                            var cgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Edenfall/Art/Cinematics/CG_01_FirstMeeting.png");
                            if (cgSprite != null) cgImg.sprite = cgSprite;
                        }

                        var spk = cineOverlay.transform.Find("Subtitles/SpeakerText")?.GetComponent<UnityEngine.UI.Text>();
                        if (spk != null) spk.text = "Aurelia";
                        var sub = cineOverlay.transform.Find("Subtitles/SubtitleText")?.GetComponent<UnityEngine.UI.Text>();
                        if (sub != null) sub.text = "“Đến đây nào... Em đã chờ anh lâu lắm rồi...”";

                        Canvas.ForceUpdateCanvases();
                        Capture(vnCam, "qa_11_cinematic_cg_first_meeting.png");
                        cineOverlay.SetActive(false);
                    }
                    if (canvas != null)
                    {
                        canvas.sortingOrder = prevSortingOrder;
                        canvas.renderMode = prevRenderMode;
                        Canvas.ForceUpdateCanvases();
                    }
                    if (diagBox != null) diagBox.SetActive(false);
                }
            }

            Debug.Log("[AureliaRigQAViewer] All 11 high-resolution QA captures recorded successfully!");
        }
    }
}
#endif
