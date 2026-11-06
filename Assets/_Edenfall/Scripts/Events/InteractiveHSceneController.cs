using System;
using System.Collections;
using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Cinematics;
using Edenfall.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.Events
{
    public enum HSceneStage
    {
        Idle = 1,       // Foreplay / Anticipation / Gentle Breathing
        Insert = 2,     // Initiation / First Contact / Breath Catch
        SlowRhythm = 3, // Passionate Rhythmic Motion
        FastRhythm = 4, // Climactic Intensity / Rapid Breathing
        Climax = 5      // Ecstatic Release & Afterglow
    }

    /// <summary>
    /// Interactive Multi-Stage Adult Visual Novel Presentation Controller.
    /// Adapted from the Godot enjoy-collection-dotnet reference architecture into Unity 6 URP 2D.
    /// Manages the 5-stage interactive state machine, procedural secondary deformation,
    /// dynamic speed/tempo control, and player HUD controls.
    /// </summary>
    public class InteractiveHSceneController : MonoBehaviour
    {
        public static InteractiveHSceneController Instance { get; private set; }

        [Header("Runtime State")]
        [SerializeField] private HSceneStage currentStage = HSceneStage.Idle;
        [SerializeField] private float playbackSpeed = 1.0f;
        [SerializeField] private bool autoAdvance = false;
        [SerializeField] private float autoAdvanceTimer = 0f;
        [SerializeField] private float stageDuration = 6.0f;

        [Header("UI Control Elements (Reference Toolbar)")]
        [SerializeField] private GameObject toolbarContainer;
        [SerializeField] private Button btnStageIdle;
        [SerializeField] private Button btnStageInsert;
        [SerializeField] private Button btnStageSlow;
        [SerializeField] private Button btnStageFast;
        [SerializeField] private Button btnStageClimax;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private Toggle autoAdvanceToggle;
        [SerializeField] private Button hideUIToggleButton;

        [Header("Display Elements")]
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private Image backgroundLayer;
        [SerializeField] private Image characterLayer;
        [SerializeField] private RectTransform characterTransform;
        [SerializeField] private Image fxOverlay;
        [SerializeField] private Text stageIndicatorText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text speakerText;

        [Header("Stage Sprites & Overlays")]
        [SerializeField] private Sprite[] stageCharacterSprites = new Sprite[5];
        [SerializeField] private AudioClip[] stageVoiceClips = new AudioClip[5];

        [Header("Procedural Secondary Motion Settings")]
        [SerializeField] private bool enableProceduralMotion = true;
        [SerializeField] private float idleBounceFrequency = 0.8f;
        [SerializeField] private float idleBounceAmplitude = 4.0f;
        [SerializeField] private float slowPistonFrequency = 1.5f;
        [SerializeField] private float slowPistonAmplitude = 12.0f;
        [SerializeField] private float fastPistonFrequency = 3.0f;
        [SerializeField] private float fastPistonAmplitude = 20.0f;

        private Vector2 baseCharacterPos;
        private Coroutine climaxEffectRoutine;
        private bool isUIHidden = false;

        public HSceneStage CurrentStage => currentStage;
        public float PlaybackSpeed => playbackSpeed;
        public bool IsAutoAdvance => autoAdvance;

        public event Action<HSceneStage> OnStageChanged;
        public event Action OnClimaxTriggered;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (characterTransform != null)
            {
                baseCharacterPos = characterTransform.anchoredPosition;
            }

            WireUIButtons();
        }

        private void Start()
        {
            SetStage(HSceneStage.Idle);
        }

        private void Update()
        {
            UpdateProceduralMotion();
            UpdateAutoAdvance();
            HandleHotkeys();
        }

        private void WireUIButtons()
        {
            if (btnStageIdle != null) btnStageIdle.onClick.AddListener(() => SetStage(HSceneStage.Idle));
            if (btnStageInsert != null) btnStageInsert.onClick.AddListener(() => SetStage(HSceneStage.Insert));
            if (btnStageSlow != null) btnStageSlow.onClick.AddListener(() => SetStage(HSceneStage.SlowRhythm));
            if (btnStageFast != null) btnStageFast.onClick.AddListener(() => SetStage(HSceneStage.FastRhythm));
            if (btnStageClimax != null) btnStageClimax.onClick.AddListener(() => SetStage(HSceneStage.Climax));

            if (speedSlider != null)
            {
                speedSlider.minValue = 0.5f;
                speedSlider.maxValue = 2.5f;
                speedSlider.value = playbackSpeed;
                speedSlider.onValueChanged.AddListener(SetPlaybackSpeed);
            }

            if (autoAdvanceToggle != null)
            {
                autoAdvanceToggle.isOn = autoAdvance;
                autoAdvanceToggle.onValueChanged.AddListener(ToggleAutoAdvance);
            }

            if (hideUIToggleButton != null)
            {
                hideUIToggleButton.onClick.AddListener(ToggleHideUI);
            }
        }

        public void SetStage(HSceneStage newStage)
        {
            currentStage = newStage;
            autoAdvanceTimer = 0f;

            // Update character sprite if available
            int spriteIndex = ((int)newStage) - 1;
            if (stageCharacterSprites != null && spriteIndex >= 0 && spriteIndex < stageCharacterSprites.Length)
            {
                if (characterLayer != null && stageCharacterSprites[spriteIndex] != null)
                {
                    characterLayer.sprite = stageCharacterSprites[spriteIndex];
                }
            }

            // Update stage indicator
            if (stageIndicatorText != null)
            {
                stageIndicatorText.text = GetStageDisplayName(newStage);
            }

            // Special climax trigger
            if (newStage == HSceneStage.Climax)
            {
                TriggerClimaxBurst();
            }

            OnStageChanged?.Invoke(newStage);
            Debug.Log($"[InteractiveHSceneController] Switched to stage: {newStage} (Speed: {playbackSpeed:F1}x)");
        }

        public void SetPlaybackSpeed(float speed)
        {
            playbackSpeed = Mathf.Clamp(speed, 0.5f, 3.0f);
        }

        public void ToggleAutoAdvance(bool enabled)
        {
            autoAdvance = enabled;
            autoAdvanceTimer = 0f;
        }

        public void ToggleHideUI()
        {
            isUIHidden = !isUIHidden;
            if (toolbarContainer != null) toolbarContainer.SetActive(!isUIHidden);
            if (subtitleText != null && subtitleText.transform.parent != null)
            {
                subtitleText.transform.parent.gameObject.SetActive(!isUIHidden);
            }
        }

        private void UpdateAutoAdvance()
        {
            if (!autoAdvance) return;

            autoAdvanceTimer += Time.deltaTime * playbackSpeed;
            if (autoAdvanceTimer >= stageDuration)
            {
                autoAdvanceTimer = 0f;
                int next = ((int)currentStage) + 1;
                if (next <= 5)
                {
                    SetStage((HSceneStage)next);
                }
                else
                {
                    SetStage(HSceneStage.Idle); // Loop back or finish
                }
            }
        }

        private void UpdateProceduralMotion()
        {
            if (!enableProceduralMotion || characterTransform == null) return;

            float time = Time.time * playbackSpeed;
            float offsetY = 0f;
            float offsetX = 0f;

            switch (currentStage)
            {
                case HSceneStage.Idle:
                    // Gentle breathing sine wave
                    offsetY = Mathf.Sin(time * idleBounceFrequency * Mathf.PI * 2f) * idleBounceAmplitude;
                    break;

                case HSceneStage.Insert:
                    // Subtle tension hold
                    offsetY = Mathf.Sin(time * idleBounceFrequency * Mathf.PI * 2f) * (idleBounceAmplitude * 0.5f);
                    break;

                case HSceneStage.SlowRhythm:
                    // Rhythmic passionate cadence (slight pelvic sway & chest rise)
                    offsetY = Mathf.Abs(Mathf.Sin(time * slowPistonFrequency * Mathf.PI)) * slowPistonAmplitude;
                    offsetX = Mathf.Cos(time * slowPistonFrequency * Mathf.PI * 0.5f) * 2.0f;
                    break;

                case HSceneStage.FastRhythm:
                    // Fast rhythmic tempo with sharp attack and recoil
                    float cycle = Mathf.Repeat(time * fastPistonFrequency, 1.0f);
                    offsetY = Mathf.Pow(cycle, 1.5f) * fastPistonAmplitude;
                    offsetX = (Mathf.PerlinNoise(time * 5f, 0f) - 0.5f) * 4.0f;
                    break;

                case HSceneStage.Climax:
                    // Ecstasy shudder followed by resting afterglow
                    offsetY = Mathf.Sin(time * 8.0f) * (idleBounceAmplitude * 1.5f);
                    break;
            }

            characterTransform.anchoredPosition = baseCharacterPos + new Vector2(offsetX, offsetY);
        }

        public void TriggerClimaxBurst()
        {
            if (climaxEffectRoutine != null) StopCoroutine(climaxEffectRoutine);
            climaxEffectRoutine = StartCoroutine(DoClimaxEffect());
            OnClimaxTriggered?.Invoke();
        }

        private IEnumerator DoClimaxEffect()
        {
            if (fxOverlay == null) yield break;

            fxOverlay.gameObject.SetActive(true);
            Color initialColor = new Color(1f, 1f, 1f, 0.85f); // Bright white-gold ecstasy flash
            fxOverlay.color = initialColor;

            float elapsed = 0f;
            float duration = 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.85f, 0f, elapsed / duration);
                fxOverlay.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                yield return null;
            }

            fxOverlay.gameObject.SetActive(false);
        }

        private void HandleHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetStage(HSceneStage.Idle);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetStage(HSceneStage.Insert);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetStage(HSceneStage.SlowRhythm);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetStage(HSceneStage.FastRhythm);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SetStage(HSceneStage.Climax);
            if (Input.GetKeyDown(KeyCode.H) || Input.GetMouseButtonDown(1)) ToggleHideUI();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                int next = ((int)currentStage) + 1;
                if (next > 5) next = 1;
                SetStage((HSceneStage)next);
            }
        }

        private string GetStageDisplayName(HSceneStage stage)
        {
            return stage switch
            {
                HSceneStage.Idle => "Stage 1: Caress (Dạo Đầu)",
                HSceneStage.Insert => "Stage 2: Initiate (Gắn Kết)",
                HSceneStage.SlowRhythm => "Stage 3: Passion (Nhịp Điệu)",
                HSceneStage.FastRhythm => "Stage 4: Intensity (Cao Trào)",
                HSceneStage.Climax => "Stage 5: Ecstasy (Viên Mãn)",
                _ => stage.ToString()
            };
        }
    }
}
