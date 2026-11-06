using System;
using System.Collections;
using System.Collections.Generic;
using Edenfall.Dialogue;
using Edenfall.World;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Edenfall.Characters
{
    /// <summary>
    /// Phase 3.6.4 Production Deformable 2D Character Rig Controller for Aurelia.
    /// Drives 21 registered character layers with:
    /// - True anatomical bone hierarchy where face & eyes are attached to Bone_Head
    /// - Genuine Unity 2D Animation SpriteSkin mesh deformation on multi-bone hair chains
    /// - Multi-segment secondary spring-mass physics with S-curve wave curvature
    /// - Independent gaze tracking with eye-socket clamped irises
    /// - Stochastic blinking and speech viseme lip-sync
    /// - Pure torso breathing deformation with locked ground contact
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public class AureliaRiggedCharacter : MonoBehaviour
    {
        [Header("Skeleton Hierarchy")]
        [SerializeField] private Transform boneRoot;
        [SerializeField] private Transform bonePelvis;
        [SerializeField] private Transform boneTorso;
        [SerializeField] private Transform boneChest;
        [SerializeField] private Transform boneNeck;
        [SerializeField] private Transform boneHead;
        [SerializeField] private Transform boneEyeL;
        [SerializeField] private Transform boneIrisL;
        [SerializeField] private Transform boneEyeR;
        [SerializeField] private Transform boneIrisR;
        [SerializeField] private Transform boneEyebrows;
        [SerializeField] private Transform boneMouth;
        [SerializeField] private Transform boneLilyOrnament;
        [SerializeField] private Transform boneArmR;

        [Header("Deformable Hair Multi-Bone Chains")]
        [SerializeField] private Transform boneHairSideL_01;
        [SerializeField] private Transform boneHairSideL_02;
        [SerializeField] private Transform boneHairSideL_03;

        [SerializeField] private Transform boneHairSideR_01;
        [SerializeField] private Transform boneHairSideR_02;
        [SerializeField] private Transform boneHairSideR_03;

        [SerializeField] private Transform boneHairFront_01;
        [SerializeField] private Transform boneHairFront_02;
        [SerializeField] private Transform boneHairFront_03;

        [SerializeField] private Transform boneHairBack_01;
        [SerializeField] private Transform boneHairBack_02;
        [SerializeField] private Transform boneHairBack_03;

        [Header("SpriteSkin Deformers")]
        [SerializeField] private SpriteSkin skinHairSideL;
        [SerializeField] private SpriteSkin skinHairSideR;
        [SerializeField] private SpriteSkin skinHairFront;
        [SerializeField] private SpriteSkin skinHairBack;

        [Header("Layer SpriteRenderers")]
        [SerializeField] private SpriteRenderer rendHairBack;
        [SerializeField] private SpriteRenderer rendBodyTorso;
        [SerializeField] private SpriteRenderer rendHeadFaceBase;
        [SerializeField] private SpriteRenderer rendEyeLOpen;
        [SerializeField] private SpriteRenderer rendEyeLBlink;
        [SerializeField] private SpriteRenderer rendIrisL;
        [SerializeField] private SpriteRenderer rendEyeROpen;
        [SerializeField] private SpriteRenderer rendEyeRBlink;
        [SerializeField] private SpriteRenderer rendIrisR;
        [SerializeField] private SpriteRenderer rendEyebrows;
        [SerializeField] private SpriteRenderer rendMouth;
        [SerializeField] private SpriteRenderer rendHairSideL;
        [SerializeField] private SpriteRenderer rendHairSideR;
        [SerializeField] private SpriteRenderer rendHairFront;
        [SerializeField] private SpriteRenderer rendLilyRibbon;
        [SerializeField] private SpriteRenderer rendBreathingChest;
        [SerializeField] private SpriteRenderer rendArmR;

        [Header("Mouth Viseme Sprites")]
        [SerializeField] private Sprite mouthClosed;
        [SerializeField] private Sprite mouthHalfOpen;
        [SerializeField] private Sprite mouthOpen;
        [SerializeField] private Sprite mouthWide;
        [SerializeField] private Sprite mouthSmile;

        [Header("Animation State")]
        [SerializeField] private AureliaAnimationState currentState = AureliaAnimationState.Idle_Normal;
        [SerializeField] private float stateTransitionSpeed = 5.0f;

        [Header("Breathing Settings")]
        [SerializeField] private bool enableBreathing = true;
        [SerializeField] private float breathCycleSpeed = 1.6f;
        [SerializeField] private float breathDisplacementY = 0.025f;
        [SerializeField] private float breathScaleY = 0.035f;

        [Header("Blink Settings")]
        [SerializeField] private bool enableBlinking = true;
        [SerializeField] private float minBlinkInterval = 2.5f;
        [SerializeField] private float maxBlinkInterval = 5.5f;
        [SerializeField] private float blinkDuration = 0.12f;

        [Header("Speech Synchronization")]
        [SerializeField] private bool isTalking = false;
        [SerializeField] private float speechFlapSpeed = 11.0f;

        [Header("Gaze Tracking")]
        [SerializeField] private bool enableGazeTracking = true;
        [SerializeField] private float maxGazeOffset = 0.035f;
        [SerializeField] private float gazeFollowSpeed = 6.0f;

        [Header("Deformable Secondary Spring Physics")]
        [SerializeField] private float springStiffness = 48f;
        [SerializeField] private float springDamping = 6.0f;

        // Internal rest transforms
        private Vector3 restHeadLocalPos;
        private Quaternion restHeadLocalRot;
        private Vector3 restChestLocalPos;
        private Vector3 restArmRLocalPos;
        private Quaternion restArmRLocalRot;
        private Vector3 restEyebrowLocalPos;
        private Vector3 restIrisLLocalPos;
        private Vector3 restIrisRLocalPos;
        private Vector3 restLilyLocalPos;
        private Quaternion restLilyLocalRot;

        // Multi-segment hair spring states (Angle & Velocity)
        private float hairSideL_MidAngle, hairSideL_MidVel;
        private float hairSideL_TipAngle, hairSideL_TipVel;
        private float hairSideR_MidAngle, hairSideR_MidVel;
        private float hairSideR_TipAngle, hairSideR_TipVel;
        private float hairFront_MidAngle, hairFront_MidVel;
        private float hairFront_TipAngle, hairFront_TipVel;
        private float hairBack_MidAngle, hairBack_MidVel;
        private float hairBack_TipAngle, hairBack_TipVel;
        private float lilyAngle, lilyVel;

        // Target state parameters
        private float targetHeadTiltZ = 0f;
        private float targetHeadOffsetY = 0f;
        private float targetEyebrowOffsetY = 0f;
        private float targetEyebrowRotZ = 0f;
        private float targetArmRRotZ = 0f;
        private float currentHeadTiltZ = 0f;
        private float currentHeadOffsetY = 0f;
        private float currentEyebrowOffsetY = 0f;
        private float currentEyebrowRotZ = 0f;
        private float currentArmRRotZ = 0f;

        private float lastHeadTiltZ = 0f;
        private Coroutine blinkCoroutine;
        private Camera mainCamera;
        private Vector3 currentGazeOffset = Vector3.zero;

        public AureliaAnimationState CurrentState => currentState;
        public bool IsTalking => isTalking;

        // Public accessors for Editor validation
        public Transform BoneHead => boneHead;
        public Transform BoneEyeL => boneEyeL;
        public Transform BoneIrisL => boneIrisL;
        public Transform BoneEyeR => boneEyeR;
        public Transform BoneIrisR => boneIrisR;
        public Transform BoneEyebrows => boneEyebrows;
        public Transform BoneMouth => boneMouth;
        public Transform BoneHairSideL_01 => boneHairSideL_01;
        public Transform BoneHairSideL_02 => boneHairSideL_02;
        public Transform BoneHairSideL_03 => boneHairSideL_03;
        public SpriteSkin SkinHairSideL => skinHairSideL;
        public SpriteSkin SkinHairSideR => skinHairSideR;
        public SpriteSkin SkinHairFront => skinHairFront;
        public SpriteSkin SkinHairBack => skinHairBack;

        private void Awake()
        {
            mainCamera = Camera.main;
            CaptureRestTransforms();
        }

        private void OnEnable()
        {
            CaptureRestTransforms();
            if (Application.isPlaying && enableBlinking && blinkCoroutine == null)
            {
                blinkCoroutine = StartCoroutine(BlinkRoutine());
            }

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed += HandleNodeDisplayed;
                DialogueManager.Instance.OnDialogueFinished += HandleDialogueFinished;
            }
        }

        private void OnDisable()
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed -= HandleNodeDisplayed;
                DialogueManager.Instance.OnDialogueFinished -= HandleDialogueFinished;
            }
        }

        private void Start()
        {
            SetAnimationState(currentState, true);
        }

        public void CaptureRestTransforms()
        {
            if (boneHead != null)
            {
                restHeadLocalPos = boneHead.localPosition;
                restHeadLocalRot = boneHead.localRotation;
            }
            if (boneChest != null)
            {
                restChestLocalPos = boneChest.localPosition;
            }
            if (boneArmR != null)
            {
                restArmRLocalPos = boneArmR.localPosition;
                restArmRLocalRot = boneArmR.localRotation;
            }
            if (boneEyebrows != null)
            {
                restEyebrowLocalPos = boneEyebrows.localPosition;
            }
            if (boneIrisL != null)
            {
                restIrisLLocalPos = boneIrisL.localPosition;
            }
            if (boneIrisR != null)
            {
                restIrisRLocalPos = boneIrisR.localPosition;
            }
            if (boneLilyOrnament != null)
            {
                restLilyLocalPos = boneLilyOrnament.localPosition;
                restLilyLocalRot = boneLilyOrnament.localRotation;
            }
        }

        private Vector3 manualGazeTarget = Vector3.zero;
        private bool hasManualGaze = false;
        private float internalSimTime = 0f;

        public void SetGazeTarget(Vector3 targetWorldPos)
        {
            hasManualGaze = true;
            manualGazeTarget = new Vector3(
                Mathf.Clamp(targetWorldPos.x * maxGazeOffset, -maxGazeOffset, maxGazeOffset),
                Mathf.Clamp(targetWorldPos.y * maxGazeOffset * 0.6f, -maxGazeOffset * 0.6f, maxGazeOffset * 0.6f),
                0
            );
        }

        public void ResetBonesToRestPose()
        {
            CaptureRestTransforms();
            if (boneHead != null)
            {
                boneHead.localPosition = restHeadLocalPos;
                boneHead.localRotation = restHeadLocalRot;
            }
            if (boneChest != null) boneChest.localPosition = restChestLocalPos;
            if (boneArmR != null)
            {
                boneArmR.localPosition = restArmRLocalPos;
                boneArmR.localRotation = restArmRLocalRot;
            }
            if (boneIrisL != null) boneIrisL.localPosition = restIrisLLocalPos;
            if (boneIrisR != null) boneIrisR.localPosition = restIrisRLocalPos;
            if (boneHairSideL_02 != null) boneHairSideL_02.localRotation = Quaternion.identity;
            if (boneHairSideL_03 != null) boneHairSideL_03.localRotation = Quaternion.identity;
            if (boneHairSideR_02 != null) boneHairSideR_02.localRotation = Quaternion.identity;
            if (boneHairSideR_03 != null) boneHairSideR_03.localRotation = Quaternion.identity;
            if (boneHairFront_02 != null) boneHairFront_02.localRotation = Quaternion.identity;
            if (boneHairFront_03 != null) boneHairFront_03.localRotation = Quaternion.identity;
            if (boneHairBack_02 != null) boneHairBack_02.localRotation = Quaternion.identity;
            if (boneHairBack_03 != null) boneHairBack_03.localRotation = Quaternion.identity;
            currentHeadTiltZ = 0;
            currentHeadOffsetY = 0;
            currentEyebrowOffsetY = 0;
            currentEyebrowRotZ = 0;
            currentArmRRotZ = 0;
            hasManualGaze = false;
        }

        public void SimulateStep(float dt)
        {
            internalSimTime += dt;
            StepSimulation(dt, internalSimTime);
        }

        private void Update()
        {
            float dt = Mathf.Clamp(Time.deltaTime, 0.001f, 0.033f);
            float t = Time.time;
            StepSimulation(dt, t);
        }

        private void StepSimulation(float dt, float t)
        {

            // 1. Natural Sinusoidal Breathing (Chest Moves Upward, Ground Contact Locked)
            if (enableBreathing && boneChest != null)
            {
                float breathPhase = (Mathf.Sin(t * breathCycleSpeed) + 1.0f) * 0.5f;
                boneChest.localPosition = restChestLocalPos + new Vector3(0, breathPhase * breathDisplacementY, 0);
                if (rendBreathingChest != null)
                {
                    rendBreathingChest.transform.localScale = new Vector3(1f, 1f + breathPhase * breathScaleY, 1f);
                }
            }

            // 2. Smooth State Parameter Blending
            float blendSpeed = dt * stateTransitionSpeed;
            currentHeadTiltZ = Mathf.Lerp(currentHeadTiltZ, targetHeadTiltZ, blendSpeed);
            currentHeadOffsetY = Mathf.Lerp(currentHeadOffsetY, targetHeadOffsetY, blendSpeed);
            currentEyebrowOffsetY = Mathf.Lerp(currentEyebrowOffsetY, targetEyebrowOffsetY, blendSpeed);
            currentEyebrowRotZ = Mathf.Lerp(currentEyebrowRotZ, targetEyebrowRotZ, blendSpeed);
            currentArmRRotZ = Mathf.Lerp(currentArmRRotZ, targetArmRRotZ, blendSpeed);

            // Apply Head Motion (Face, Eyes, Eyebrows, Mouth, and Hair roots follow 100% cleanly)
            if (boneHead != null)
            {
                boneHead.localPosition = restHeadLocalPos + new Vector3(0, currentHeadOffsetY, 0);
                boneHead.localRotation = restHeadLocalRot * Quaternion.Euler(0, 0, currentHeadTiltZ);
            }

            // Apply Eyebrow Emotion
            if (boneEyebrows != null)
            {
                boneEyebrows.localPosition = restEyebrowLocalPos + new Vector3(0, currentEyebrowOffsetY, 0);
                boneEyebrows.localRotation = Quaternion.Euler(0, 0, currentEyebrowRotZ);
            }

            // Apply Arm Pose
            if (boneArmR != null)
            {
                boneArmR.localRotation = restArmRLocalRot * Quaternion.Euler(0, 0, currentArmRRotZ);
            }

            // 3. Speech Viseme Lip-Sync
            UpdateSpeechMouth(dt, t);

            // 4. Multi-Segment Deformable Hair Physics
            UpdateDeformableHairPhysics(dt, t);

            // 5. Gaze Tracking
            UpdateGazeTracking(dt);

            lastHeadTiltZ = currentHeadTiltZ;
        }

        private void UpdateSpeechMouth(float dt, float t)
        {
            if (rendMouth == null) return;

            if (isTalking)
            {
                float speechPhase = Mathf.PingPong(t * speechFlapSpeed, 1.0f);
                if (speechPhase < 0.22f)
                {
                    rendMouth.sprite = mouthHalfOpen != null ? mouthHalfOpen : mouthClosed;
                }
                else if (speechPhase < 0.65f)
                {
                    rendMouth.sprite = mouthOpen != null ? mouthOpen : mouthClosed;
                }
                else if (speechPhase < 0.88f)
                {
                    rendMouth.sprite = mouthWide != null ? mouthWide : mouthOpen;
                }
                else
                {
                    rendMouth.sprite = mouthHalfOpen != null ? mouthHalfOpen : mouthClosed;
                }
            }
            else
            {
                // Emotional resting mouth
                if (currentState == AureliaAnimationState.Idle_Happy || currentState == AureliaAnimationState.React_Romantic)
                {
                    rendMouth.sprite = mouthSmile != null ? mouthSmile : mouthClosed;
                }
                else
                {
                    rendMouth.sprite = mouthClosed;
                }
            }
        }

        private void UpdateDeformableHairPhysics(float dt, float t)
        {
            // Head angular acceleration drives inertial drag on hair
            float headAngVel = (currentHeadTiltZ - lastHeadTiltZ) / dt;
            float inertialForce = -headAngVel * 0.45f;

            // Atmospheric breeze
            float windSway = Mathf.Sin(t * 2.2f) * 1.5f + Mathf.Cos(t * 1.4f) * 0.8f;

            // 1. Left Side Hair (Mid + Tip wave physics)
            SimulateSpring(ref hairSideL_MidAngle, ref hairSideL_MidVel, inertialForce + windSway, springStiffness, springDamping, dt);
            SimulateSpring(ref hairSideL_TipAngle, ref hairSideL_TipVel, hairSideL_MidAngle * 1.35f + windSway * 1.2f, springStiffness * 0.85f, springDamping * 0.9f, dt);
            if (boneHairSideL_02 != null) boneHairSideL_02.localRotation = Quaternion.Euler(0, 0, hairSideL_MidAngle);
            if (boneHairSideL_03 != null) boneHairSideL_03.localRotation = Quaternion.Euler(0, 0, hairSideL_TipAngle);

            // 2. Right Side Hair
            SimulateSpring(ref hairSideR_MidAngle, ref hairSideR_MidVel, inertialForce + windSway * 0.9f, springStiffness, springDamping, dt);
            SimulateSpring(ref hairSideR_TipAngle, ref hairSideR_TipVel, hairSideR_MidAngle * 1.35f + windSway * 1.1f, springStiffness * 0.85f, springDamping * 0.9f, dt);
            if (boneHairSideR_02 != null) boneHairSideR_02.localRotation = Quaternion.Euler(0, 0, hairSideR_MidAngle);
            if (boneHairSideR_03 != null) boneHairSideR_03.localRotation = Quaternion.Euler(0, 0, hairSideR_TipAngle);

            // 3. Front Bangs
            SimulateSpring(ref hairFront_MidAngle, ref hairFront_MidVel, inertialForce * 0.6f + windSway * 0.6f, springStiffness * 1.4f, springDamping * 1.2f, dt);
            SimulateSpring(ref hairFront_TipAngle, ref hairFront_TipVel, hairFront_MidAngle * 1.25f, springStiffness * 1.2f, springDamping * 1.1f, dt);
            if (boneHairFront_02 != null) boneHairFront_02.localRotation = Quaternion.Euler(0, 0, hairFront_MidAngle);
            if (boneHairFront_03 != null) boneHairFront_03.localRotation = Quaternion.Euler(0, 0, hairFront_TipAngle);

            // 4. Back Flowing Hair
            SimulateSpring(ref hairBack_MidAngle, ref hairBack_MidVel, windSway * 0.7f, springStiffness * 0.7f, springDamping * 0.8f, dt);
            SimulateSpring(ref hairBack_TipAngle, ref hairBack_TipVel, hairBack_MidAngle * 1.4f + windSway * 0.9f, springStiffness * 0.6f, springDamping * 0.8f, dt);
            if (boneHairBack_02 != null) boneHairBack_02.localRotation = Quaternion.Euler(0, 0, hairBack_MidAngle);
            if (boneHairBack_03 != null) boneHairBack_03.localRotation = Quaternion.Euler(0, 0, hairBack_TipAngle);

            // 5. Lily Ribbon Secondary Spring
            SimulateSpring(ref lilyAngle, ref lilyVel, inertialForce * 0.7f + windSway * 0.5f, springStiffness * 1.5f, springDamping * 1.3f, dt);
            if (boneLilyOrnament != null) boneLilyOrnament.localRotation = restLilyLocalRot * Quaternion.Euler(0, 0, lilyAngle);
        }

        private static void SimulateSpring(ref float pos, ref float vel, float target, float k, float d, float dt)
        {
            float force = -k * (pos - target) - d * vel;
            vel += force * dt;
            vel = Mathf.Clamp(vel, -80f, 80f);
            pos += vel * dt;
            pos = Mathf.Clamp(pos, -45f, 45f);
        }

        private void UpdateGazeTracking(float dt)
        {
            if (!enableGazeTracking) return;

            Vector3 targetGaze = Vector3.zero;
            if (hasManualGaze)
            {
                targetGaze = manualGazeTarget;
            }
            else if (Application.isPlaying && mainCamera != null)
            {
                Vector3 mouseScreen = Input.mousePosition;
                Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
                Vector3 delta = (mouseScreen - screenCenter) / (Screen.height * 0.5f);

                targetGaze = new Vector3(
                    Mathf.Clamp(delta.x * maxGazeOffset, -maxGazeOffset, maxGazeOffset),
                    Mathf.Clamp(delta.y * maxGazeOffset * 0.6f, -maxGazeOffset * 0.6f, maxGazeOffset * 0.6f),
                    0
                );
            }

            currentGazeOffset = Vector3.Lerp(currentGazeOffset, targetGaze, dt * gazeFollowSpeed);

            if (boneIrisL != null)
            {
                boneIrisL.localPosition = restIrisLLocalPos + currentGazeOffset;
            }
            if (boneIrisR != null)
            {
                boneIrisR.localPosition = restIrisRLocalPos + currentGazeOffset;
            }
        }

        public void SetAnimationState(AureliaAnimationState state, bool immediate = false)
        {
            currentState = state;

            switch (state)
            {
                case AureliaAnimationState.Idle_Normal:
                    targetHeadTiltZ = 0f;
                    targetHeadOffsetY = 0f;
                    targetEyebrowOffsetY = 0f;
                    targetEyebrowRotZ = 0f;
                    targetArmRRotZ = 0f;
                    break;

                case AureliaAnimationState.Idle_Happy:
                    targetHeadTiltZ = -1.5f;
                    targetHeadOffsetY = 0.01f;
                    targetEyebrowOffsetY = 0.015f;
                    targetEyebrowRotZ = -1.0f;
                    targetArmRRotZ = 1.5f;
                    break;

                case AureliaAnimationState.React_Embarrassed:
                case AureliaAnimationState.Idle_Shy:
                    targetHeadTiltZ = 3.5f;
                    targetHeadOffsetY = -0.02f;
                    targetEyebrowOffsetY = -0.01f;
                    targetEyebrowRotZ = 2.5f;
                    targetArmRRotZ = -2.0f;
                    break;

                case AureliaAnimationState.React_Romantic:
                    targetHeadTiltZ = -3.0f;
                    targetHeadOffsetY = -0.01f;
                    targetEyebrowOffsetY = 0.005f;
                    targetEyebrowRotZ = -1.5f;
                    targetArmRRotZ = 3.0f;
                    break;

                case AureliaAnimationState.React_Surprised:
                    targetHeadTiltZ = 2.0f;
                    targetHeadOffsetY = 0.035f;
                    targetEyebrowOffsetY = 0.04f;
                    targetEyebrowRotZ = 4.0f;
                    targetArmRRotZ = -3.5f;
                    break;

                case AureliaAnimationState.React_Angry:
                    targetHeadTiltZ = -1.8f;
                    targetHeadOffsetY = 0.0f;
                    targetEyebrowOffsetY = -0.025f;
                    targetEyebrowRotZ = -4.0f;
                    targetArmRRotZ = 2.0f;
                    break;

                case AureliaAnimationState.Talk_Normal:
                case AureliaAnimationState.Talk_Gentle:
                case AureliaAnimationState.Talk_Shy:
                case AureliaAnimationState.Talk_Serious:
                    isTalking = true;
                    break;

                default:
                    targetHeadTiltZ = 0f;
                    targetHeadOffsetY = 0f;
                    break;
            }

            if (immediate)
            {
                currentHeadTiltZ = targetHeadTiltZ;
                currentHeadOffsetY = targetHeadOffsetY;
                currentEyebrowOffsetY = targetEyebrowOffsetY;
                currentEyebrowRotZ = targetEyebrowRotZ;
                currentArmRRotZ = targetArmRRotZ;

                if (boneHead != null)
                {
                    boneHead.localPosition = restHeadLocalPos + new Vector3(0, currentHeadOffsetY, 0);
                    boneHead.localRotation = restHeadLocalRot * Quaternion.Euler(0, 0, currentHeadTiltZ);
                }
            }
        }

        public void TriggerBlink()
        {
            if (isActiveAndEnabled)
            {
                StartCoroutine(SingleBlinkCycle());
            }
        }

        private IEnumerator SingleBlinkCycle()
        {
            if (rendEyeLOpen != null) rendEyeLOpen.gameObject.SetActive(false);
            if (rendEyeLBlink != null) rendEyeLBlink.gameObject.SetActive(true);
            if (rendEyeROpen != null) rendEyeROpen.gameObject.SetActive(false);
            if (rendEyeRBlink != null) rendEyeRBlink.gameObject.SetActive(true);

            yield return new WaitForSeconds(blinkDuration);

            if (rendEyeLOpen != null) rendEyeLOpen.gameObject.SetActive(true);
            if (rendEyeLBlink != null) rendEyeLBlink.gameObject.SetActive(false);
            if (rendEyeROpen != null) rendEyeROpen.gameObject.SetActive(true);
            if (rendEyeRBlink != null) rendEyeRBlink.gameObject.SetActive(false);
        }

        private IEnumerator BlinkRoutine()
        {
            while (enableBlinking)
            {
                float waitTime = UnityEngine.Random.Range(minBlinkInterval, maxBlinkInterval);
                yield return new WaitForSeconds(waitTime);
                yield return SingleBlinkCycle();
            }
        }

        public void SetTalking(bool talking)
        {
            isTalking = talking;
        }

        public void SetEyesClosed(bool closed)
        {
            if (rendEyeLOpen != null) rendEyeLOpen.gameObject.SetActive(!closed);
            if (rendEyeLBlink != null) rendEyeLBlink.gameObject.SetActive(closed);
            if (rendEyeROpen != null) rendEyeROpen.gameObject.SetActive(!closed);
            if (rendEyeRBlink != null) rendEyeRBlink.gameObject.SetActive(closed);
        }

        public void SetMouthViseme(int index)
        {
            if (rendMouth == null) return;
            switch (index)
            {
                case 0: rendMouth.sprite = mouthClosed; break;
                case 1: rendMouth.sprite = mouthHalfOpen != null ? mouthHalfOpen : mouthClosed; break;
                case 2: rendMouth.sprite = mouthOpen != null ? mouthOpen : mouthClosed; break;
                case 3: rendMouth.sprite = mouthWide != null ? mouthWide : mouthOpen; break;
                case 4: rendMouth.sprite = mouthSmile != null ? mouthSmile : mouthClosed; break;
                default: rendMouth.sprite = mouthClosed; break;
            }
        }

        private void HandleNodeDisplayed(CharacterDefinition speaker, DialogueNode node, Sprite portrait)
        {
            if (speaker != null && (speaker.characterId.Equals("aurelia", StringComparison.OrdinalIgnoreCase) || speaker.characterName.Equals("Aurelia", StringComparison.OrdinalIgnoreCase)))
            {
                SetTalking(true);
            }
            else
            {
                SetTalking(false);
            }
        }

        private void HandleDialogueFinished()
        {
            SetTalking(false);
        }
    }
}
