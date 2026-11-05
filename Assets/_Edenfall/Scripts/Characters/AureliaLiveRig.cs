using System;
using System.Collections;
using System.Collections.Generic;
using Edenfall.World;
using UnityEngine;

namespace Edenfall.Characters
{
    public enum AureliaAnimationState
    {
        Idle_Normal,
        Idle_Shy,
        Idle_Happy,
        Talk_Normal,
        Talk_Gentle,
        Talk_Shy,
        Talk_Serious,
        React_Surprised,
        React_Embarrassed,
        React_Sad,
        React_Angry,
        React_Romantic,
        Gesture_Reaching,
        Gesture_Thinking,
        Gesture_Concerned
    }

    [Serializable]
    public struct Live2DParameters
    {
        [Range(-30f, 30f)] public float angleX;
        [Range(-30f, 30f)] public float angleY;
        [Range(-30f, 30f)] public float angleZ;

        [Range(0f, 1f)] public float eyeLOpen;
        [Range(0f, 1f)] public float eyeROpen;
        [Range(-1f, 1f)] public float eyeBallX;
        [Range(-1f, 1f)] public float eyeBallY;

        [Range(0f, 1f)] public float mouthOpenY;
        [Range(-1f, 1f)] public float mouthForm;

        [Range(-10f, 10f)] public float bodyAngleX;
        [Range(-10f, 10f)] public float bodyAngleZ;
        [Range(0f, 1f)] public float breath;

        [Range(0f, 1f)] public float blush;
        public float hairFront;
        public float hairSide;
        public float hairBack;
    }

    /// <summary>
    /// Phase 3.6 Production Live2D-Style Decoupled Character Rig & Animation State Machine.
    /// Emulates Cubism parameter blending, spring-mass physics, speech flap, and 15 distinct animation states.
    /// </summary>
    public class AureliaLiveRig : MonoBehaviour
    {
        [Header("Master Renderers & Layers")]
        [SerializeField] private SpriteRenderer primaryRenderer;
        [SerializeField] private SpriteRenderer faceExpressionRenderer;
        [SerializeField] private SpriteRenderer blushOverlayRenderer;
        [SerializeField] private SpriteRenderer talkRenderer;
        [SerializeField] private NPCStageAnchor stageAnchor;

        [Header("Canonical Artwork Resources")]
        [SerializeField] private Sprite halfBodyMasterSprite;
        [SerializeField] private Sprite fullBodyMasterSprite;
        [SerializeField] private List<Sprite> talkSprites = new(); // 5 frames
        [SerializeField] private List<Sprite> expressionPortraits = new();

        [Header("Live2D Runtime Parameters")]
        [SerializeField] private Live2DParameters currentParams;
        [SerializeField] private Live2DParameters targetParams;

        [Header("Animation State")]
        [SerializeField] private AureliaAnimationState currentState = AureliaAnimationState.Idle_Normal;
        [SerializeField] private float stateTransitionSpeed = 4.0f; // ~0.25s transition

        [Header("Eye & Blink Controller")]
        [SerializeField] private bool enableBlinking = true;
        [SerializeField] private float minBlinkInterval = 2.4f;
        [SerializeField] private float maxBlinkInterval = 5.2f;

        [Header("Speech Synchronization")]
        [SerializeField] private bool isTalking = false;
        [SerializeField] private float talkFlapSpeed = 10.0f;

        [Header("Secondary Spring Physics")]
        [SerializeField] private float hairSpringStiffness = 35f;
        [SerializeField] private float hairDamping = 4.5f;

        private Vector3 initialBasePos;
        private Vector3 initialBaseScale;
        private Quaternion initialBaseRot;
        private bool isBaseCaptured = false;

        private float hairFrontVelocity = 0f;
        private float hairSideVelocity = 0f;
        private float hairBackVelocity = 0f;

        private Coroutine blinkRoutine;
        private Coroutine stateRoutine;

        public AureliaAnimationState CurrentState => currentState;
        public bool IsTalking => isTalking;
        public Live2DParameters Parameters => currentParams;

        private void Awake()
        {
            if (primaryRenderer == null) primaryRenderer = GetComponent<SpriteRenderer>();
            if (stageAnchor == null) stageAnchor = GetComponent<NPCStageAnchor>();
            CaptureBaseTransform();
        }

        private void Start()
        {
            CaptureBaseTransform();
            SetAnimationState(AureliaAnimationState.Idle_Normal, true);
            if (enableBlinking) blinkRoutine = StartCoroutine(BlinkLoop());
        }

        private void OnEnable()
        {
            CaptureBaseTransform();
            if (enableBlinking && blinkRoutine == null) blinkRoutine = StartCoroutine(BlinkLoop());
        }

        private void OnDisable()
        {
            if (blinkRoutine != null) { StopCoroutine(blinkRoutine); blinkRoutine = null; }
            if (stateRoutine != null) { StopCoroutine(stateRoutine); stateRoutine = null; }
        }

        public void CaptureBaseTransform()
        {
            initialBasePos = transform.position;
            initialBaseScale = transform.localScale;
            initialBaseRot = transform.localRotation;
            isBaseCaptured = true;
        }

        private void Update()
        {
            if (!isBaseCaptured) CaptureBaseTransform();

            float dt = Time.deltaTime;
            float time = Time.time;

            // 1. Natural Breathing Loop (~3.6s cycle)
            float breathCycle = (Mathf.Sin(time * 1.74f) + 1.0f) * 0.5f; // [0, 1]
            currentParams.breath = breathCycle;

            // 2. Speech Synchronization
            if (isTalking)
            {
                // Smooth sinusoidal speech loop with accented peaks
                float talkPhase = Mathf.PingPong(time * talkFlapSpeed, 1.0f);
                currentParams.mouthOpenY = talkPhase;

                if (talkSprites.Count >= 5 && talkRenderer != null)
                {
                    int frameIndex = Mathf.Clamp((int)(talkPhase * 4.99f), 0, talkSprites.Count - 1);
                    talkRenderer.sprite = talkSprites[frameIndex];
                    if (!talkRenderer.gameObject.activeSelf) talkRenderer.gameObject.SetActive(true);
                }
            }
            else
            {
                currentParams.mouthOpenY = Mathf.MoveTowards(currentParams.mouthOpenY, 0f, dt * 6f);
                if (talkRenderer != null && talkRenderer.gameObject.activeSelf)
                {
                    talkRenderer.gameObject.SetActive(false);
                }
            }

            // 3. Smooth Parameter Lerp (No abrupt snapping!)
            LerpParameters(dt);

            // 4. Secondary Spring-Mass Hair Physics
            UpdateSpringPhysics(dt);

            // 5. Apply Rig Deformations to Transform
            ApplyRigTransform();
        }

        private void LerpParameters(float dt)
        {
            float t = dt * stateTransitionSpeed;
            currentParams.angleX = Mathf.Lerp(currentParams.angleX, targetParams.angleX, t);
            currentParams.angleY = Mathf.Lerp(currentParams.angleY, targetParams.angleY, t);
            currentParams.angleZ = Mathf.Lerp(currentParams.angleZ, targetParams.angleZ, t);

            currentParams.bodyAngleX = Mathf.Lerp(currentParams.bodyAngleX, targetParams.bodyAngleX, t);
            currentParams.bodyAngleZ = Mathf.Lerp(currentParams.bodyAngleZ, targetParams.bodyAngleZ, t);
            currentParams.blush = Mathf.Lerp(currentParams.blush, targetParams.blush, t);

            if (blushOverlayRenderer != null)
            {
                Color c = blushOverlayRenderer.color;
                c.a = currentParams.blush;
                blushOverlayRenderer.color = c;
            }
        }

        private void UpdateSpringPhysics(float dt)
        {
            float targetHair = -currentParams.angleZ * 0.45f + Mathf.Sin(Time.time * 2.2f) * 0.8f;

            // Front Hair Spring
            float fForce = (targetHair - currentParams.hairFront) * hairSpringStiffness - hairFrontVelocity * hairDamping;
            hairFrontVelocity += fForce * dt;
            currentParams.hairFront += hairFrontVelocity * dt;

            // Side Hair / Locks Spring
            float sForce = (targetHair * 1.2f - currentParams.hairSide) * (hairSpringStiffness * 0.85f) - hairSideVelocity * hairDamping;
            hairSideVelocity += sForce * dt;
            currentParams.hairSide += hairSideVelocity * dt;

            // Back Hair Spring
            float bForce = (targetHair * 0.7f - currentParams.hairBack) * (hairSpringStiffness * 1.1f) - hairBackVelocity * hairDamping;
            hairBackVelocity += bForce * dt;
            currentParams.hairBack += hairBackVelocity * dt;
        }

        private void ApplyRigTransform()
        {
            // Combined Head and Body Tilt around feet/baseline
            float totalAngleZ = currentParams.angleZ * 0.4f + currentParams.bodyAngleZ * 0.6f + currentParams.hairFront * 0.15f;
            transform.localRotation = initialBaseRot * Quaternion.Euler(0, 0, totalAngleZ);

            // Upward-Only Breathing Expansion
            // Because sprite pivot is at bottom-center (0.5, 0.0), scaling Y expands strictly upward (+y).
            // Ground contact remains mathematically invariant (0 displacement).
            float breathScaleY = 1.0f + (currentParams.breath * 0.022f);
            transform.localScale = new Vector3(initialBaseScale.x, initialBaseScale.y * breathScaleY, initialBaseScale.z);

            // Ground Clamping
            if (stageAnchor != null)
            {
                transform.position = stageAnchor.ClampIdleMotion(initialBasePos, Vector3.zero);
            }
        }

        public void SetAnimationState(AureliaAnimationState state, bool immediate = false)
        {
            currentState = state;
            ConfigureStateParameters(state);

            if (immediate)
            {
                currentParams = targetParams;
            }
        }

        private void ConfigureStateParameters(AureliaAnimationState state)
        {
            switch (state)
            {
                case AureliaAnimationState.Idle_Normal:
                    targetParams.angleX = 0f; targetParams.angleY = 0f; targetParams.angleZ = 0.3f;
                    targetParams.bodyAngleX = 0f; targetParams.bodyAngleZ = 0.2f;
                    targetParams.blush = 0.0f;
                    break;

                case AureliaAnimationState.Idle_Shy:
                    targetParams.angleX = -3.5f; targetParams.angleY = -4.0f; targetParams.angleZ = -2.8f;
                    targetParams.bodyAngleX = -1.5f; targetParams.bodyAngleZ = -1.2f;
                    targetParams.blush = 0.65f;
                    break;

                case AureliaAnimationState.Idle_Happy:
                    targetParams.angleX = 2.0f; targetParams.angleY = 2.5f; targetParams.angleZ = 2.2f;
                    targetParams.bodyAngleX = 1.0f; targetParams.bodyAngleZ = 1.5f;
                    targetParams.blush = 0.25f;
                    break;

                case AureliaAnimationState.Talk_Normal:
                    targetParams.angleX = 0f; targetParams.angleY = 1.2f; targetParams.angleZ = 0.5f;
                    targetParams.blush = 0.0f;
                    break;

                case AureliaAnimationState.Talk_Gentle:
                    targetParams.angleX = 1.5f; targetParams.angleY = 0.5f; targetParams.angleZ = 1.8f;
                    targetParams.blush = 0.2f;
                    break;

                case AureliaAnimationState.Talk_Shy:
                    targetParams.angleX = -4.0f; targetParams.angleY = -3.5f; targetParams.angleZ = -3.0f;
                    targetParams.blush = 0.8f;
                    break;

                case AureliaAnimationState.Talk_Serious:
                    targetParams.angleX = 0f; targetParams.angleY = -1.5f; targetParams.angleZ = 0f;
                    targetParams.blush = 0.0f;
                    break;

                case AureliaAnimationState.React_Surprised:
                    targetParams.angleX = 0f; targetParams.angleY = 4.0f; targetParams.angleZ = -1.5f;
                    targetParams.bodyAngleX = -2.0f; targetParams.blush = 0.35f;
                    break;

                case AureliaAnimationState.React_Embarrassed:
                    targetParams.angleX = -6.0f; targetParams.angleY = -5.0f; targetParams.angleZ = -4.0f;
                    targetParams.bodyAngleX = -2.5f; targetParams.blush = 0.95f;
                    break;

                case AureliaAnimationState.React_Sad:
                    targetParams.angleX = -2.0f; targetParams.angleY = -4.5f; targetParams.angleZ = -3.5f;
                    targetParams.bodyAngleX = -1.5f; targetParams.blush = 0.1f;
                    break;

                case AureliaAnimationState.React_Angry:
                    targetParams.angleX = 3.0f; targetParams.angleY = 2.0f; targetParams.angleZ = 1.0f;
                    targetParams.bodyAngleX = 2.0f; targetParams.blush = 0.4f;
                    break;

                case AureliaAnimationState.React_Romantic:
                    targetParams.angleX = 2.5f; targetParams.angleY = -1.0f; targetParams.angleZ = 3.2f;
                    targetParams.bodyAngleX = 1.5f; targetParams.blush = 0.7f;
                    break;

                case AureliaAnimationState.Gesture_Reaching:
                    targetParams.angleX = 1.5f; targetParams.angleY = 1.0f; targetParams.angleZ = 1.2f;
                    targetParams.bodyAngleX = 2.2f; targetParams.blush = 0.3f;
                    break;

                case AureliaAnimationState.Gesture_Thinking:
                    targetParams.angleX = -3.0f; targetParams.angleY = 3.0f; targetParams.angleZ = -3.5f;
                    targetParams.bodyAngleX = -1.0f; targetParams.blush = 0.15f;
                    break;

                case AureliaAnimationState.Gesture_Concerned:
                    targetParams.angleX = 0.5f; targetParams.angleY = -2.5f; targetParams.angleZ = -2.0f;
                    targetParams.bodyAngleX = -0.5f; targetParams.blush = 0.25f;
                    break;
            }
        }

        private IEnumerator BlinkLoop()
        {
            while (enableBlinking)
            {
                float wait = UnityEngine.Random.Range(minBlinkInterval, maxBlinkInterval);
                yield return new WaitForSeconds(wait);

                // Quick realistic blink: close over 0.06s, open over 0.08s
                float t = 0f;
                while (t < 0.06f)
                {
                    t += Time.deltaTime;
                    currentParams.eyeLOpen = Mathf.Lerp(1f, 0f, t / 0.06f);
                    currentParams.eyeROpen = currentParams.eyeLOpen;
                    yield return null;
                }

                yield return new WaitForSeconds(0.02f);

                t = 0f;
                while (t < 0.08f)
                {
                    t += Time.deltaTime;
                    currentParams.eyeLOpen = Mathf.Lerp(0f, 1f, t / 0.08f);
                    currentParams.eyeROpen = currentParams.eyeLOpen;
                    yield return null;
                }
                currentParams.eyeLOpen = 1f;
                currentParams.eyeROpen = 1f;

                // 25% chance of realistic double-blink
                if (UnityEngine.Random.value < 0.25f)
                {
                    yield return new WaitForSeconds(0.08f);
                    t = 0f;
                    while (t < 0.05f)
                    {
                        t += Time.deltaTime;
                        currentParams.eyeLOpen = Mathf.Lerp(1f, 0f, t / 0.05f);
                        currentParams.eyeROpen = currentParams.eyeLOpen;
                        yield return null;
                    }
                    t = 0f;
                    while (t < 0.07f)
                    {
                        t += Time.deltaTime;
                        currentParams.eyeLOpen = Mathf.Lerp(0f, 1f, t / 0.07f);
                        currentParams.eyeROpen = currentParams.eyeLOpen;
                        yield return null;
                    }
                    currentParams.eyeLOpen = 1f;
                    currentParams.eyeROpen = 1f;
                }
            }
        }

        public void SetTalking(bool talking)
        {
            isTalking = talking;
        }

        public void SetStagingMode(StagingMode mode, Camera cam = null)
        {
            if (primaryRenderer != null)
            {
                if (mode == StagingMode.DialoguePortrait && halfBodyMasterSprite != null)
                {
                    primaryRenderer.sprite = halfBodyMasterSprite;
                }
                else if (mode == StagingMode.FullBodyGrounded && fullBodyMasterSprite != null)
                {
                    primaryRenderer.sprite = fullBodyMasterSprite;
                }
            }

            if (stageAnchor != null)
            {
                Camera targetCam = cam != null ? cam : Camera.main;
                stageAnchor.AlignCharacter(transform, primaryRenderer, mode, targetCam);
                CaptureBaseTransform();
            }
        }

        public void ConfigureMasterSprites(
            Sprite halfBody,
            Sprite fullBody,
            List<Sprite> talkList,
            List<Sprite> portraitList,
            SpriteRenderer pRenderer,
            SpriteRenderer talkR = null,
            NPCStageAnchor anchor = null)
        {
            halfBodyMasterSprite = halfBody;
            fullBodyMasterSprite = fullBody;
            talkSprites = talkList ?? new();
            expressionPortraits = portraitList ?? new();
            if (pRenderer != null) primaryRenderer = pRenderer;
            if (talkR != null) talkRenderer = talkR;
            if (anchor != null) stageAnchor = anchor;
            CaptureBaseTransform();
        }
    }
}
