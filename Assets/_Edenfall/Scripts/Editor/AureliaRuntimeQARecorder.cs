#if UNITY_EDITOR
using System.IO;
using Edenfall.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Edenfall.Editor
{
    public static class AureliaRuntimeQARecorder
    {
        private const string OutputDir = "Reports/QA_Captures_Phase364";
        private const string FramesDir = "Reports/QA_Captures_Phase364/Frames";
        private const string DiagnosticScenePath = "Assets/_Edenfall/Scenes/AureliaRigDiagnostic.unity";
        private const string SanctuaryScenePath = "Assets/_Edenfall/Scenes/DawnSanctuary/DawnSanctuary.unity";

        [MenuItem("Edenfall/Record True Runtime Deformable QA Suite")]
        public static void RecordDeformableRigQASuite()
        {
            if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);
            if (!Directory.Exists(FramesDir)) Directory.CreateDirectory(FramesDir);

            AssetDatabase.Refresh();
            AureliaRigBuilder.BuildRiggedPrefab();
            AureliaRigBuilder.CreateDiagnosticScene();

            var scene = EditorSceneManager.OpenScene(DiagnosticScenePath, OpenSceneMode.Single);
            GameObject charObj = GameObject.Find("Aurelia_Rigged_Character");
            if (charObj == null)
            {
                Debug.LogError("[AureliaRuntimeQARecorder] Aurelia_Rigged_Character not found!");
                return;
            }

            var rig = charObj.GetComponent<AureliaRiggedCharacter>();
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[AureliaRuntimeQARecorder] Main Camera not found!");
                return;
            }

            int width = 1280;
            int height = 720;

            Texture2D CaptureFrame(Camera activeCam, string relPath)
            {
                RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                activeCam.targetTexture = rt;
                activeCam.Render();
                RenderTexture.active = rt;

                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                activeCam.targetTexture = null;
                RenderTexture.active = null;
                Object.DestroyImmediate(rt);

                byte[] pngData = tex.EncodeToPNG();
                string fullPath = Path.Combine(OutputDir, relPath);
                File.WriteAllBytes(fullPath, pngData);
                return tex;
            }

            // --- 1. RECORD CONSECUTIVE FRAMES (300 frames = 10s at 30 FPS) ---
            cam.orthographicSize = 4.6f;
            cam.transform.position = new Vector3(0, 0.4f, -10f);

            rig.SetAnimationState(AureliaAnimationState.Idle_Normal, true);
            rig.SetEyesClosed(false);
            rig.SetTalking(false);

            int totalFrames = 300;
            float dt = 1f / 30f;

            Debug.Log($"[AureliaRuntimeQARecorder] Recording {totalFrames} consecutive animation frames...");

            for (int i = 0; i < totalFrames; i++)
            {
                float time = i * dt;

                // Timeline logic:
                // 0.0s - 3.0s: Idle breathing & secondary hair wave
                if (time < 3.0f)
                {
                    rig.SetAnimationState(AureliaAnimationState.Idle_Normal, false);
                    rig.SetEyesClosed(false);
                    rig.SetTalking(false);
                }
                // 3.0s - 5.0s: Eye blinking & iris gaze tracking
                else if (time < 5.0f)
                {
                    bool blink = (time >= 3.5f && time <= 3.7f) || (time >= 4.4f && time <= 4.6f);
                    rig.SetEyesClosed(blink);
                    float gazeX = Mathf.Sin((time - 3.0f) * Mathf.PI) * 0.15f;
                    rig.SetGazeTarget(new Vector3(gazeX, 1.4f, 0));
                }
                // 5.0s - 7.5s: Head tilt with S-curve deformable hair mesh bending
                else if (time < 7.5f)
                {
                    rig.SetEyesClosed(false);
                    rig.SetAnimationState(AureliaAnimationState.Idle_Shy, false);
                    // Manual head rotation to trigger S-curve spring physics
                    var headBone = rig.transform.Find("Bones/Bone_Root/Bone_Spine/Bone_Chest/Bone_Neck/Bone_Head");
                    if (headBone != null)
                    {
                        float tilt = Mathf.Sin((time - 5.0f) * 2f) * 8.0f;
                        headBone.localRotation = Quaternion.Euler(0, 0, tilt);
                    }
                }
                // 7.5s - 10.0s: Speech visemes & Romantic reaction
                else
                {
                    rig.SetAnimationState(AureliaAnimationState.React_Romantic, false);
                    int viseme = (int)(Mathf.PingPong((time - 7.5f) * 4f, 2.99f));
                    rig.SetMouthViseme(viseme);
                }

                // Simulate character rig step
                rig.SimulateStep(dt);

                // Save frame
                string frameName = $"Frames/frame_{i:D4}.png";
                CaptureFrame(cam, frameName);
            }

            Debug.Log("[AureliaRuntimeQARecorder] Frames recording complete. Capturing named QA milestones...");

            // --- 2. CAPTURE NAMED MILESTONE PNGs ---
            // QA-01: Rest pose perfect registration
            rig.SetAnimationState(AureliaAnimationState.Idle_Normal, true);
            rig.SetEyesClosed(false);
            rig.SetTalking(false);
            rig.ResetBonesToRestPose();
            CaptureFrame(cam, "qa_01_rest_pose_perfect_registration.png");

            // QA-02: Natural breathing locked ground
            rig.SetAnimationState(AureliaAnimationState.Idle_Normal, false);
            for (int s = 0; s < 45; s++) rig.SimulateStep(0.033f);
            CaptureFrame(cam, "qa_02_natural_breathing_locked_ground.png");

            // QA-03: Stochastic eye blink closed
            rig.SetEyesClosed(true);
            CaptureFrame(cam, "qa_03_stochastic_eye_blink_closed.png");

            // QA-04: Deformable hair mesh S-curve bend
            rig.SetEyesClosed(false);
            rig.SetAnimationState(AureliaAnimationState.Idle_Shy, false);
            var head = rig.transform.Find("Bones/Bone_Root/Bone_Spine/Bone_Chest/Bone_Neck/Bone_Head");
            if (head != null) head.localRotation = Quaternion.Euler(0, 0, -9.5f);
            for (int s = 0; s < 60; s++) rig.SimulateStep(0.033f);
            CaptureFrame(cam, "qa_04_deformable_hair_mesh_s_curve_bend.png");

            // QA-05: Independent gaze tracking
            rig.ResetBonesToRestPose();
            rig.SetGazeTarget(new Vector3(0.35f, 1.4f, 0));
            rig.SimulateStep(0.033f);
            CaptureFrame(cam, "qa_05_independent_gaze_tracking.png");

            // QA-06: Speech viseme lip sync open
            rig.SetMouthViseme(2); // Open
            CaptureFrame(cam, "qa_06_speech_viseme_lip_sync_open.png");

            // QA-07: Shy reaction tilt and blush
            rig.SetMouthViseme(0);
            rig.SetAnimationState(AureliaAnimationState.React_Embarrassed, true);
            for (int s = 0; s < 30; s++) rig.SimulateStep(0.033f);
            CaptureFrame(cam, "qa_07_shy_reaction_tilt_and_blush.png");

            // QA-08: Romantic seductive posture
            rig.SetAnimationState(AureliaAnimationState.React_Romantic, true);
            for (int s = 0; s < 30; s++) rig.SimulateStep(0.033f);
            CaptureFrame(cam, "qa_08_romantic_seductive_posture.png");

            // QA-09: Dawn Sanctuary deformable VN staging
            if (File.Exists(SanctuaryScenePath))
            {
                var sanctuaryScene = EditorSceneManager.OpenScene(SanctuaryScenePath, OpenSceneMode.Single);
                var vnCam = Camera.main;
                if (vnCam != null)
                {
                    CaptureFrame(vnCam, "qa_09_dawn_sanctuary_deformable_vn_staging.png");
                }
            }

            Debug.Log($"[AureliaRuntimeQARecorder] QA Suite recorded successfully to {OutputDir}!");
        }
    }
}
#endif
