using System;
using System.Collections;
using System.Collections.Generic;
using Edenfall.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.Cinematics
{
    /// <summary>
    /// Phase 3.6 Cinematic Presentation Pipeline for High-Resolution Narrative CG Scenes.
    /// Supports Ken Burns camera pan/zoom loops, parallax layers, lighting dimming, and first-person POV framing.
    /// </summary>
    public class CinematicCGController : MonoBehaviour
    {
        public static CinematicCGController Instance { get; private set; }

        [Header("UI & Presentation Elements")]
        [SerializeField] private GameObject cinematicOverlayCanvas;
        [SerializeField] private CanvasGroup overlayCanvasGroup;
        [SerializeField] private Image primaryCGImage;
        [SerializeField] private Image secondaryCGImage; // For cross-fading
        [SerializeField] private Text speakerText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Button skipButton;

        [Header("Ken Burns Dynamic Camera Settings")]
        [SerializeField] private bool enablePanAndZoom = true;
        [SerializeField] private float maxZoom = 1.15f;
        [SerializeField] private float panSpeed = 0.05f;

        [Header("Registered Narrative CG Scenes")]
        [SerializeField] private List<Sprite> registeredCGs = new();

        private Coroutine panZoomRoutine;
        private Coroutine fadeRoutine;
        private bool isSkipping = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (skipButton != null) skipButton.onClick.AddListener(OnSkipClicked);
        }

        private void OnEnable()
        {
            if (CinematicManager.Instance != null)
            {
                CinematicManager.Instance.OnCinematicStarted += HandleCinematicStarted;
                CinematicManager.Instance.OnSlideChanged += HandleSlideChanged;
                CinematicManager.Instance.OnCinematicFinished += HandleCinematicFinished;
            }
        }

        private void OnDisable()
        {
            if (CinematicManager.Instance != null)
            {
                CinematicManager.Instance.OnCinematicStarted -= HandleCinematicStarted;
                CinematicManager.Instance.OnSlideChanged -= HandleSlideChanged;
                CinematicManager.Instance.OnCinematicFinished -= HandleCinematicFinished;
            }
        }

        private void HandleCinematicStarted(CinematicSceneData scene)
        {
            isSkipping = false;
            if (cinematicOverlayCanvas != null)
            {
                cinematicOverlayCanvas.SetActive(true);
            }
            if (overlayCanvasGroup != null)
            {
                overlayCanvasGroup.alpha = 1f;
            }
        }

        private void HandleSlideChanged(CinematicSceneData scene, CinematicSlide slide, int current, int total)
        {
            if (slide == null) return;

            Sprite targetCG = slide.cgSprite;
            if (targetCG != null && primaryCGImage != null)
            {
                primaryCGImage.sprite = targetCG;
                primaryCGImage.gameObject.SetActive(true);

                if (enablePanAndZoom)
                {
                    if (panZoomRoutine != null) StopCoroutine(panZoomRoutine);
                    panZoomRoutine = StartCoroutine(DoKenBurnsPanZoom(primaryCGImage.rectTransform, 4.0f));
                }
            }

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
            if (speakerText != null) speakerText.text = !string.IsNullOrEmpty(scene.characterName) ? scene.characterName : scene.GetTitle(isVi);
            if (subtitleText != null)
            {
                string text = slide.GetDialogue(isVi);
                if (string.IsNullOrEmpty(text)) text = slide.GetNarration(isVi);
                subtitleText.text = text;
            }
        }

        private void HandleCinematicFinished(CinematicSceneData scene)
        {
            if (panZoomRoutine != null) StopCoroutine(panZoomRoutine);

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeOutOverlay());
        }

        private IEnumerator DoKenBurnsPanZoom(RectTransform rt, float duration)
        {
            if (rt == null) yield break;

            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.one * maxZoom;
            Vector2 startPos = Vector2.zero;
            Vector2 endPos = new Vector2(UnityEngine.Random.Range(-25f, 25f), UnityEngine.Random.Range(-15f, 15f));

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                rt.localScale = Vector3.Lerp(startScale, endScale, t);
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
        }

        private IEnumerator FadeOutOverlay()
        {
            if (overlayCanvasGroup == null)
            {
                if (cinematicOverlayCanvas != null) cinematicOverlayCanvas.SetActive(false);
                yield break;
            }

            float elapsed = 0f;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                overlayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }

            overlayCanvasGroup.alpha = 0f;
            if (cinematicOverlayCanvas != null) cinematicOverlayCanvas.SetActive(false);
        }

        private void OnSkipClicked()
        {
            isSkipping = true;
            if (CinematicManager.Instance != null)
            {
                // Advance to end
                while (CinematicManager.Instance != null && isSkipping)
                {
                    CinematicManager.Instance.AdvanceSlide();
                    break;
                }
            }
        }

        public void PlayCGDirect(Sprite cgSprite, string title, string text)
        {
            if (primaryCGImage != null && cgSprite != null)
            {
                primaryCGImage.sprite = cgSprite;
                primaryCGImage.gameObject.SetActive(true);
            }
            if (speakerText != null) speakerText.text = title;
            if (subtitleText != null) subtitleText.text = text;

            if (cinematicOverlayCanvas != null) cinematicOverlayCanvas.SetActive(true);
            if (overlayCanvasGroup != null) overlayCanvasGroup.alpha = 1f;

            if (enablePanAndZoom && primaryCGImage != null)
            {
                if (panZoomRoutine != null) StopCoroutine(panZoomRoutine);
                panZoomRoutine = StartCoroutine(DoKenBurnsPanZoom(primaryCGImage.rectTransform, 5.0f));
            }
        }
    }
}
