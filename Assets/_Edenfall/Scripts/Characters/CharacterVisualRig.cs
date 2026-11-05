using System.Collections;
using Edenfall.Characters;
using Edenfall.World;
using UnityEngine;

namespace Edenfall.Characters
{
    public class CharacterVisualRig : MonoBehaviour
    {
        [Header("Master Visual Novel Sprites")]
        [SerializeField] private SpriteRenderer primaryRenderer;
        [SerializeField] private Sprite portraitSprite;
        [SerializeField] private Sprite fullBodySprite;
        [SerializeField] private NPCStageAnchor stageAnchor;
        [SerializeField] private StagingMode currentStagingMode = StagingMode.DialoguePortrait;

        [Header("Procedural Breathing (Grounded)")]
        [SerializeField] private bool enableBreathing = true;
        [SerializeField] private float breathingSpeed = 1.8f;
        [SerializeField] private float breathingScaleAmplitude = 0.02f; // Subtle 2% upward expansion

        [Header("Idle Sway (Grounded)")]
        [SerializeField] private bool enableIdleSway = true;
        [SerializeField] private float swaySpeed = 1.2f;
        [SerializeField] private float swayMaxAngle = 0.35f; // Rotation around feet anchor

        [Header("Legacy / Layered Overlays (Optional)")]
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform torsoTransform;
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform hairBackTransform;
        [SerializeField] private Transform hairFrontTransform;
        [SerializeField] private Transform skirtTransform;

        [SerializeField] private SpriteRenderer baseBodyRenderer;
        [SerializeField] private SpriteRenderer faceRenderer;
        [SerializeField] private SpriteRenderer eyesRenderer;
        [SerializeField] private SpriteRenderer mouthRenderer;
        [SerializeField] private SpriteRenderer hairBackRenderer;
        [SerializeField] private SpriteRenderer hairFrontRenderer;
        [SerializeField] private SpriteRenderer skirtRenderer;

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

        private Vector3 basePosition;
        private Vector3 baseScale;
        private Quaternion baseRotation;
        private bool isBaseCaptured = false;

        private Coroutine blinkCoroutine;
        private Coroutine talkCoroutine;

        public CharacterEmotion CurrentEmotion => currentEmotion;
        public bool IsTalking => isTalking;
        public StagingMode CurrentStagingMode => currentStagingMode;
        public SpriteRenderer PrimaryRenderer => primaryRenderer;

        private void Awake()
        {
            if (rootTransform == null) rootTransform = transform;
            if (primaryRenderer == null) primaryRenderer = GetComponent<SpriteRenderer>();
            if (stageAnchor == null) stageAnchor = GetComponent<NPCStageAnchor>();

            CaptureBaseTransform();
        }

        private void Start()
        {
            CaptureBaseTransform();
            if (stageAnchor != null && primaryRenderer != null)
            {
                SetStagingMode(currentStagingMode);
            }
        }

        private void OnEnable()
        {
            CaptureBaseTransform();
            if (enableBlinking && eyesRenderer != null && eyeOpenSprite != null && eyeBlinkSprite != null)
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

        public void CaptureBaseTransform()
        {
            basePosition = transform.position;
            baseScale = transform.localScale;
            baseRotation = transform.localRotation;
            isBaseCaptured = true;
        }

        private void Update()
        {
            if (!isBaseCaptured) CaptureBaseTransform();

            float time = Time.time;

            // 1. Procedural Grounded Breathing
            // Because sprite pivot is anchored at feet (0.5, 0.0), scaling Y expands strictly UPWARDS (+y).
            // Feet remain locked at world ground contact position without dipping or floating.
            if (enableBreathing)
            {
                float breathSin = Mathf.Sin(time * breathingSpeed);
                float upwardStretch = Mathf.Max(0f, breathSin) * breathingScaleAmplitude;
                transform.localScale = new Vector3(baseScale.x, baseScale.y * (1f + upwardStretch), baseScale.z);
            }

            // 2. Subtle Grounded Sway
            // Rotates gently around feet pivot (Z axis)
            if (enableIdleSway)
            {
                float sway = Mathf.Sin(time * swaySpeed) * swayMaxAngle;
                transform.localRotation = baseRotation * Quaternion.Euler(0, 0, sway);
            }

            // 3. Anchor Grounding Enforcement
            if (stageAnchor != null)
            {
                transform.position = stageAnchor.ClampIdleMotion(basePosition, Vector3.zero);
            }
        }

        public void SetMasterSprites(Sprite portrait, Sprite fullBody, SpriteRenderer renderer = null, NPCStageAnchor anchor = null)
        {
            portraitSprite = portrait;
            fullBodySprite = fullBody;
            if (renderer != null) primaryRenderer = renderer;
            if (anchor != null) stageAnchor = anchor;
        }

        public void SetStagingMode(StagingMode newMode, Camera cam = null)
        {
            currentStagingMode = newMode;

            if (primaryRenderer != null)
            {
                if (newMode == StagingMode.DialoguePortrait && portraitSprite != null)
                {
                    primaryRenderer.sprite = portraitSprite;
                }
                else if (newMode == StagingMode.FullBodyGrounded && fullBodySprite != null)
                {
                    primaryRenderer.sprite = fullBodySprite;
                }
            }

            if (stageAnchor != null)
            {
                Camera targetCam = cam != null ? cam : Camera.main;
                stageAnchor.AlignCharacter(transform, primaryRenderer, newMode, targetCam);
                CaptureBaseTransform();
            }
        }

        private IEnumerator BlinkRoutine()
        {
            while (enableBlinking && eyesRenderer != null)
            {
                float wait = Random.Range(minBlinkInterval, maxBlinkInterval);
                yield return new WaitForSeconds(wait);

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
                if (gameObject.activeInHierarchy && mouthRenderer != null)
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
                baseScale.x = Mathf.Abs(baseScale.x) * (faceRight ? 1f : -1f);
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

            if (primaryRenderer == null) primaryRenderer = bodyR;
            CaptureBaseTransform();
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
