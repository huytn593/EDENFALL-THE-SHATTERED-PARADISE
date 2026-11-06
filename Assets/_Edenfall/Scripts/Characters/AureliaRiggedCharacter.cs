using System;
using System.Collections;
using System.Collections.Generic;
using Edenfall.Dialogue;
using Edenfall.World;
using UnityEngine;

namespace Edenfall.Characters
{
    /// <summary>
    /// Phase 3.6.3 Production Decoupled 2D Character Rig Controller for Aurelia.
    /// Drives 21 registered character layers with true independent bone hierarchy,
    /// procedural eye blinking, gaze tracking, speech viseme flap, spring-mass hair physics,
    /// and chest breathing deformation without modifying the stationary root transform.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public class AureliaRiggedCharacter : MonoBehaviour
    {
        [Header("Bone Hierarchy References")]
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
        [SerializeField] private Transform boneHairFront;
        [SerializeField] private Transform boneHairSideL;
        [SerializeField] private Transform boneHairSideR;
        [SerializeField] private Transform boneHairBack;
        [SerializeField] private Transform boneLilyOrnament;
        [SerializeField] private Transform boneArmR;

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
        [SerializeField] private float breathCycleSpeed = 1.6f; // ~3.9s per full breath
        [SerializeField] private float breathDisplacementY = 0.025f; // Local upward chest motion
        [SerializeField] private float breathScaleY = 0.035f;

        [Header("Blink Settings")]
        [SerializeField] private bool enableBlinking = true;
        [SerializeField] private float minBlinkInterval = 2.5f;
        [SerializeField] private float maxBlinkInterval = 5.5f;
        [SerializeField] private float blinkDuration = 0.14f;

        [Header("Speech Synchronization")]
        [SerializeField] private bool isTalking = false;
        [SerializeField] private float speechFlapSpeed = 11.0f;

        [Header("Gaze Tracking")]
        [SerializeField] private bool enableGazeTracking = true;
        [SerializeField] private float maxGazeOffset = 0.035f;
        [SerializeField] private float gazeFollowSpeed = 6.0f;

        [Header("Secondary Spring-Mass Hair Physics")]
        [SerializeField] private float springStiffness = 45f;
        [SerializeField] private float springDamping = 5.5f;

        // Internal rest transforms
        private Vector3 restHeadLocalPos;
        private Quaternion restHeadLocalRot;
        private Vector3 restChestLocalPos;
        private Vector3 restChestLocalScale;
        private Vector3 restArmRLocalPos;
        private Quaternion restArmRLocalRot;
        private Vector3 restEyebrowLocalPos;
        private Vector3 restIrisLLocalPos;
        private Vector3 restIrisRLocalPos;

        // Spring velocities
        private float hairFrontVel, hairFrontAngle;
        private float hairSideLVel, hairSideLAngle;
        private float hairSideRVel, hairSideRAngle;
        private float hairBackVel, hairBackAngle;
        private float lilyVel, lilyAngle;

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

        private Coroutine blinkCoroutine;
        private Camera mainCamera;
        private Vector3 currentGazeOffset = Vector3.zero;

        public AureliaAnimationState CurrentState => currentState;
        public bool IsTalking => isTalking;

        private void Awake()
        {
            mainCamera = Camera.main;
            CaptureRestTransforms();
        }

        private void OnEnable()
        {
            CaptureRestTransforms();
            if (enableBlinking && blinkCoroutine == null)
            {
                blinkCoroutine = StartCoroutine(BlinkRoutine());
            }

            // Hook DialogueManager events
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

        private void CaptureRestTransforms()
        {
            if (boneHead != null)
            {
                restHeadLocalPos = boneHead.localPosition;
                restHeadLocalRot = boneHead.localRotation;
            }
            if (boneChest != null)
            {
                restChestLocalPos = boneChest.localPosition;
                restChestLocalScale = boneChest.localScale;
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
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float t = Time.time;

            // 1. Natural Sinusoidal Breathing (Affects Chest & Bust Layer Only, Root Stays Flat)
            if (enableBreathing && boneChest != null)
            {
                float breathPhase = (Mathf.Sin(t * breathCycleSpeed) + 1.0f) * 0.5f; // [0, 1]
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

            // Apply Head Motion
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

            // 3. Speech Viseme Flap
            UpdateSpeechMouth(dt, t);

            // 4. Secondary Spring-Mass Hair Physics
            UpdateHairPhysics(dt, t);

            // 5. Gaze Tracking
            UpdateGazeTracking(dt);
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
                // Idle or emotional resting mouth
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

        private void UpdateHairPhysics(float dt, float t)
        {
            // Driving acceleration from head tilt and natural atmospheric breeze
            float windSway = Mathf.Sin(t * 2.2f) * 1.2f + Mathf.Cos(t * 1.4f) * 0.6f;
            float targetHairAngle = -currentHeadTiltZ * 0.6f + windSway;

            // Front Bangs Spring
            float fForce = (targetHairAngle * 0.8f - hairFrontAngle) * springStiffness - hairFrontVel * springDamping;
            hairFrontVel += fForce * dt;
            hairFrontAngle += hairFrontVel * dt;
            if (boneHairFront != null) boneHairFront.localRotation = Quaternion.Euler(0, 0, hairFrontAngle);

            // Side Lock Left Spring
            float slForce = (targetHairAngle * 1.3f - hairSideLAngle) * (springStiffness * 0.85f) - hairSideLVel * springDamping;
            hairSideLVel += slForce * dt;
            hairSideLAngle += hairSideLVel * dt;
            if (boneHairSideL != null) boneHairSideL.localRotation = Quaternion.Euler(0, 0, hairSideLAngle);

            // Side Lock Right Spring
            float srForce = (targetHairAngle * 1.1f - hairSideRAngle) * (springStiffness * 0.9f) - hairSideRVel * springDamping;
            hairSideRVel += srForce * dt;
            hairSideRAngle += hairSideRVel * dt;
            if (boneHairSideR != null) boneHairSideR.localRotation = Quaternion.Euler(0, 0, hairSideRAngle);

            // Hair Back Spring
            float bForce = (targetHairAngle * 0.5f - hairBackAngle) * (springStiffness * 1.2f) - hairBackVel * springDamping;
            hairBackVel += bForce * dt;
            hairBackAngle += hairBackVel * dt;
            if (boneHairBack != null) boneHairBack.localRotation = Quaternion.Euler(0, 0, hairBackAngle);

            // Lily Blossom Spring
            float lForce = (targetHairAngle * 0.4f - lilyAngle) * (springStiffness * 1.5f) - lilyVel * (springDamping * 1.2f);
            lilyVel += lForce * dt;
            lilyAngle += lilyVel * dt;
            if (boneLilyOrnament != null) boneLilyOrnament.localRotation = Quaternion.Euler(0, 0, lilyAngle);
        }

        private void UpdateGazeTracking(float dt)
        {
            if (!enableGazeTracking || mainCamera == null) return;

            Vector3 mouseScreen = Input.mousePosition;
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 10f));
            Vector3 headPos = boneHead != null ? boneHead.position : transform.position;

            Vector3 dir = (mouseWorld - headPos).normalized;
            Vector3 targetOffset = new Vector3(
                Mathf.Clamp(dir.x * maxGazeOffset, -maxGazeOffset, maxGazeOffset),
                Mathf.Clamp(dir.y * (maxGazeOffset * 0.6f), -maxGazeOffset * 0.5f, maxGazeOffset * 0.5f),
                0f
            );

            currentGazeOffset = Vector3.Lerp(currentGazeOffset, targetOffset, dt * gazeFollowSpeed);

            if (boneIrisL != null) boneIrisL.localPosition = restIrisLLocalPos + currentGazeOffset;
            if (boneIrisR != null) boneIrisR.localPosition = restIrisRLocalPos + currentGazeOffset;
        }

        private IEnumerator BlinkRoutine()
        {
            while (enableBlinking)
            {
                float wait = UnityEngine.Random.Range(minBlinkInterval, maxBlinkInterval);
                yield return new WaitForSeconds(wait);

                // Quick realistic blink
                SetEyesClosed(true);
                yield return new WaitForSeconds(blinkDuration);
                SetEyesClosed(false);

                // Occasional cute double-blink (20% chance)
                if (UnityEngine.Random.value < 0.20f)
                {
                    yield return new WaitForSeconds(0.08f);
                    SetEyesClosed(true);
                    yield return new WaitForSeconds(blinkDuration * 0.8f);
                    SetEyesClosed(false);
                }
            }
        }

        public void SetEyesClosed(bool closed)
        {
            if (rendEyeLOpen != null) rendEyeLOpen.gameObject.SetActive(!closed);
            if (rendEyeROpen != null) rendEyeROpen.gameObject.SetActive(!closed);
            if (rendEyeLBlink != null) rendEyeLBlink.gameObject.SetActive(closed);
            if (rendEyeRBlink != null) rendEyeRBlink.gameObject.SetActive(closed);
            if (rendIrisL != null) rendIrisL.gameObject.SetActive(!closed);
            if (rendIrisR != null) rendIrisR.gameObject.SetActive(!closed);
        }

        public void SetTalking(bool talking)
        {
            isTalking = talking;
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

                case AureliaAnimationState.Idle_Shy:
                    targetHeadTiltZ = -3.5f; // Cute head duck
                    targetHeadOffsetY = -0.015f;
                    targetEyebrowOffsetY = -0.01f;
                    targetEyebrowRotZ = -3f;
                    targetArmRRotZ = 8f; // Hand moves closer to chin/chest
                    break;

                case AureliaAnimationState.Idle_Happy:
                    targetHeadTiltZ = 2.8f;
                    targetHeadOffsetY = 0.01f;
                    targetEyebrowOffsetY = 0.015f;
                    targetEyebrowRotZ = 2.5f;
                    targetArmRRotZ = -3f;
                    break;

                case AureliaAnimationState.Talk_Normal:
                case AureliaAnimationState.Talk_Gentle:
                    targetHeadTiltZ = 1.2f;
                    targetHeadOffsetY = 0f;
                    targetEyebrowOffsetY = 0.005f;
                    targetEyebrowRotZ = 1f;
                    targetArmRRotZ = 0f;
                    break;

                case AureliaAnimationState.Talk_Shy:
                    targetHeadTiltZ = -4.0f;
                    targetHeadOffsetY = -0.02f;
                    targetEyebrowOffsetY = -0.015f;
                    targetEyebrowRotZ = -4f;
                    targetArmRRotZ = 10f;
                    break;

                case AureliaAnimationState.Talk_Serious:
                    targetHeadTiltZ = 0f;
                    targetHeadOffsetY = 0f;
                    targetEyebrowOffsetY = -0.02f;
                    targetEyebrowRotZ = -2f;
                    targetArmRRotZ = -5f;
                    break;

                case AureliaAnimationState.React_Surprised:
                    targetHeadTiltZ = -1.5f;
                    targetHeadOffsetY = 0.03f;
                    targetEyebrowOffsetY = 0.035f;
                    targetEyebrowRotZ = 4.0f;
                    targetArmRRotZ = 5f;
                    break;

                case AureliaAnimationState.React_Embarrassed:
                    targetHeadTiltZ = -5.0f;
                    targetHeadOffsetY = -0.025f;
                    targetEyebrowOffsetY = -0.02f;
                    targetEyebrowRotZ = -5.0f;
                    targetArmRRotZ = 14f;
                    break;

                case AureliaAnimationState.React_Romantic:
                    targetHeadTiltZ = 3.5f; // Affectionate head tilt
                    targetHeadOffsetY = -0.005f;
                    targetEyebrowOffsetY = 0.01f;
                    targetEyebrowRotZ = 2.0f;
                    targetArmRRotZ = 6f;
                    break;

                case AureliaAnimationState.Gesture_Reaching:
                    targetHeadTiltZ = 2.0f;
                    targetHeadOffsetY = 0.01f;
                    targetArmRRotZ = -15f;
                    break;

                default:
                    targetHeadTiltZ = 0f;
                    targetHeadOffsetY = 0f;
                    targetEyebrowOffsetY = 0f;
                    targetEyebrowRotZ = 0f;
                    targetArmRRotZ = 0f;
                    break;
            }

            if (immediate)
            {
                currentHeadTiltZ = targetHeadTiltZ;
                currentHeadOffsetY = targetHeadOffsetY;
                currentEyebrowOffsetY = targetEyebrowOffsetY;
                currentEyebrowRotZ = targetEyebrowRotZ;
                currentArmRRotZ = targetArmRRotZ;
            }
        }

        // Dialogue integration
        private void HandleNodeDisplayed(CharacterDefinition speaker, DialogueNode node, Sprite portrait)
        {
            if (speaker != null && speaker.characterId == "aurelia")
            {
                SetTalking(true);
                // Also trigger emotion based on node if present
                if (node != null)
                {
                    switch (node.emotion)
                    {
                        case CharacterEmotion.Flustered:
                            SetAnimationState(AureliaAnimationState.Talk_Shy);
                            break;
                        case CharacterEmotion.Smiling:
                        case CharacterEmotion.Radiant:
                            SetAnimationState(AureliaAnimationState.Idle_Happy);
                            break;
                        case CharacterEmotion.Surprised:
                            SetAnimationState(AureliaAnimationState.React_Surprised);
                            break;
                        case CharacterEmotion.Intimate:
                            SetAnimationState(AureliaAnimationState.React_Romantic);
                            break;
                        default:
                            SetAnimationState(AureliaAnimationState.Talk_Normal);
                            break;
                    }
                }
            }
            else
            {
                SetTalking(false);
            }
        }

        private void HandleDialogueFinished()
        {
            SetTalking(false);
            SetAnimationState(AureliaAnimationState.Idle_Normal);
        }
    }
}
