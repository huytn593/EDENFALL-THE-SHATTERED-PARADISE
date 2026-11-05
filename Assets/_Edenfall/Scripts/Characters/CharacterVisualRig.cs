using System.Collections;
using Edenfall.Characters;
using UnityEngine;

namespace Edenfall.Characters
{
    public class CharacterVisualRig : MonoBehaviour
    {
        [Header("Hierarchy Transforms")]
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform torsoTransform;
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform hairBackTransform;
        [SerializeField] private Transform hairFrontTransform;
        [SerializeField] private Transform leftArmTransform;
        [SerializeField] private Transform rightArmTransform;
        [SerializeField] private Transform skirtTransform;

        [Header("Layered Sprite Renderers")]
        [SerializeField] private SpriteRenderer baseBodyRenderer;
        [SerializeField] private SpriteRenderer faceRenderer;
        [SerializeField] private SpriteRenderer eyesRenderer;
        [SerializeField] private SpriteRenderer mouthRenderer;
        [SerializeField] private SpriteRenderer hairBackRenderer;
        [SerializeField] private SpriteRenderer hairFrontRenderer;
        [SerializeField] private SpriteRenderer skirtRenderer;

        [Header("Procedural Breathing")]
        [SerializeField] private bool enableBreathing = true;
        [SerializeField] private float breathingSpeed = 1.8f;
        [SerializeField] private float torsoYScaleAmplitude = 0.025f;
        [SerializeField] private float headYOffsetAmplitude = 0.015f;

        [Header("Secondary Hair & Cloth Motion")]
        [SerializeField] private bool enableSecondaryMotion = true;
        [SerializeField] private float hairSwaySpeed = 2.2f;
        [SerializeField] private float hairMaxAngle = 4.0f;
        [SerializeField] private float skirtSwaySpeed = 1.6f;
        [SerializeField] private float skirtMaxAngle = 3.0f;

        [Header("Eye Blinking")]
        [SerializeField] private bool enableBlinking = true;
        [SerializeField] private Sprite eyeOpenSprite;
        [SerializeField] private Sprite eyeBlinkSprite;
        [SerializeField] private float minBlinkInterval = 2.5f;
        [SerializeField] private float maxBlinkInterval = 5.5f;
        [SerializeField] private float blinkDuration = 0.12f;

        [Header("Mouth & Speech")]
        [SerializeField] private Sprite mouthClosedSprite;
        [SerializeField] private Sprite mouthOpenSprite;
        [SerializeField] private float talkFlapInterval = 0.14f;

        [Header("Runtime State")]
        [SerializeField] private CharacterEmotion currentEmotion = CharacterEmotion.Neutral;
        [SerializeField] private bool isTalking = false;

        private Vector3 initialTorsoScale;
        private Vector3 initialHeadPos;
        private Quaternion initialHairBackRot;
        private Quaternion initialSkirtRot;
        private Coroutine blinkCoroutine;
        private Coroutine talkCoroutine;

        public CharacterEmotion CurrentEmotion => currentEmotion;
        public bool IsTalking => isTalking;

        private void Awake()
        {
            if (rootTransform == null) rootTransform = transform;
            if (torsoTransform != null) initialTorsoScale = torsoTransform.localScale;
            if (headTransform != null) initialHeadPos = headTransform.localPosition;
            if (hairBackTransform != null) initialHairBackRot = hairBackTransform.localRotation;
            if (skirtTransform != null) initialSkirtRot = skirtTransform.localRotation;
        }

        private void OnEnable()
        {
            if (enableBlinking)
            {
                blinkCoroutine = StartCoroutine(BlinkRoutine());
            }
        }

        private void OnDisable()
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }

            if (talkCoroutine != null)
            {
                StopCoroutine(talkCoroutine);
                talkCoroutine = null;
            }
        }

        private void Update()
        {
            float time = Time.time;

            // 1. Procedural Breathing
            if (enableBreathing)
            {
                float breathSin = Mathf.Sin(time * breathingSpeed);

                if (torsoTransform != null)
                {
                    torsoTransform.localScale = initialTorsoScale + new Vector3(0, breathSin * torsoYScaleAmplitude, 0);
                }

                if (headTransform != null)
                {
                    headTransform.localPosition = initialHeadPos + new Vector3(0, breathSin * headYOffsetAmplitude, 0);
                }
            }

            // 2. Secondary Hair & Gown Motion
            if (enableSecondaryMotion)
            {
                if (hairBackTransform != null)
                {
                    float hairAngle = Mathf.Sin(time * hairSwaySpeed) * hairMaxAngle;
                    hairBackTransform.localRotation = initialHairBackRot * Quaternion.Euler(0, 0, hairAngle);
                }

                if (hairFrontTransform != null)
                {
                    float frontHairAngle = Mathf.Sin(time * (hairSwaySpeed * 1.15f) + 0.5f) * (hairMaxAngle * 0.6f);
                    hairFrontTransform.localRotation = Quaternion.Euler(0, 0, frontHairAngle);
                }

                if (skirtTransform != null)
                {
                    float skirtAngle = Mathf.Sin(time * skirtSwaySpeed + 0.8f) * skirtMaxAngle;
                    skirtTransform.localRotation = initialSkirtRot * Quaternion.Euler(0, 0, skirtAngle);
                }
            }
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                float waitTime = Random.Range(minBlinkInterval, maxBlinkInterval);
                yield return new WaitForSeconds(waitTime);

                if (eyesRenderer != null && eyeBlinkSprite != null)
                {
                    eyesRenderer.sprite = eyeBlinkSprite;
                    yield return new WaitForSeconds(blinkDuration);
                    eyesRenderer.sprite = eyeOpenSprite;
                }
            }
        }

        public void SetTalking(bool talking)
        {
            if (isTalking == talking) return;
            isTalking = talking;

            if (isTalking)
            {
                if (talkCoroutine != null) StopCoroutine(talkCoroutine);
                if (gameObject.activeInHierarchy)
                {
                    talkCoroutine = StartCoroutine(TalkRoutine());
                }
            }
            else
            {
                if (talkCoroutine != null)
                {
                    StopCoroutine(talkCoroutine);
                    talkCoroutine = null;
                }
                if (mouthRenderer != null && mouthClosedSprite != null)
                {
                    mouthRenderer.sprite = mouthClosedSprite;
                }
            }
        }

        private IEnumerator TalkRoutine()
        {
            bool toggle = false;
            while (isTalking)
            {
                if (mouthRenderer != null)
                {
                    mouthRenderer.sprite = toggle && mouthOpenSprite != null ? mouthOpenSprite : mouthClosedSprite;
                }
                toggle = !toggle;
                yield return new WaitForSeconds(talkFlapInterval);
            }
        }

        public void SetExpression(CharacterEmotion emotion, Sprite customEyes = null, Sprite customMouth = null)
        {
            currentEmotion = emotion;

            if (customEyes != null)
            {
                eyeOpenSprite = customEyes;
                if (eyesRenderer != null) eyesRenderer.sprite = eyeOpenSprite;
            }

            if (customMouth != null)
            {
                mouthClosedSprite = customMouth;
                if (mouthRenderer != null && !isTalking) mouthRenderer.sprite = mouthClosedSprite;
            }
        }

        public void SetFacing(bool faceRight)
        {
            if (rootTransform != null)
            {
                Vector3 scale = rootTransform.localScale;
                scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
                rootTransform.localScale = scale;
            }
        }

        public void ConfigureRig(
            Transform root, Transform torso, Transform head, Transform hairBack, Transform hairFront, Transform skirt,
            SpriteRenderer bodyR, SpriteRenderer faceR, SpriteRenderer eyesR, SpriteRenderer mouthR,
            SpriteRenderer hairBackR, SpriteRenderer hairFrontR, SpriteRenderer skirtR)
        {
            rootTransform = root != null ? root : transform;
            torsoTransform = torso;
            headTransform = head;
            hairBackTransform = hairBack;
            hairFrontTransform = hairFront;
            skirtTransform = skirt;

            baseBodyRenderer = bodyR;
            faceRenderer = faceR;
            eyesRenderer = eyesR;
            mouthRenderer = mouthR;
            hairBackRenderer = hairBackR;
            hairFrontRenderer = hairFrontR;
            skirtRenderer = skirtR;

            if (torsoTransform != null) initialTorsoScale = torsoTransform.localScale;
            if (headTransform != null) initialHeadPos = headTransform.localPosition;
            if (hairBackTransform != null) initialHairBackRot = hairBackTransform.localRotation;
            if (skirtTransform != null) initialSkirtRot = skirtTransform.localRotation;
        }

        public void AssignSprites(
            Sprite body,
            Sprite face,
            Sprite eyesOpen,
            Sprite eyesBlink,
            Sprite mouthClosed,
            Sprite mouthOpen,
            Sprite hairBack,
            Sprite hairFront,
            Sprite skirt)
        {
            if (baseBodyRenderer != null && body != null) baseBodyRenderer.sprite = body;
            if (faceRenderer != null && face != null) faceRenderer.sprite = face;
            if (hairBackRenderer != null && hairBack != null) hairBackRenderer.sprite = hairBack;
            if (hairFrontRenderer != null && hairFront != null) hairFrontRenderer.sprite = hairFront;
            if (skirtRenderer != null && skirt != null) skirtRenderer.sprite = skirt;

            eyeOpenSprite = eyesOpen;
            eyeBlinkSprite = eyesBlink;
            mouthClosedSprite = mouthClosed;
            mouthOpenSprite = mouthOpen;

            if (eyesRenderer != null && eyeOpenSprite != null) eyesRenderer.sprite = eyeOpenSprite;
            if (mouthRenderer != null && mouthClosedSprite != null) mouthRenderer.sprite = mouthClosedSprite;
        }
    }
}
