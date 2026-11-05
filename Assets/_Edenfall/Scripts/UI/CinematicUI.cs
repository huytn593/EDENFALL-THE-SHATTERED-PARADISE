using Edenfall.Cinematics;
using Edenfall.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.UI
{
    public class CinematicUI : MonoBehaviour
    {
        [Header("Root Panel")]
        [SerializeField] private GameObject cinematicPanel;

        [Header("CG Display")]
        [SerializeField] private Image cgDisplayImage;
        [SerializeField] private CanvasGroup fadeOverlayGroup;

        [Header("Text Displays")]
        [SerializeField] private Text sceneTitleText;
        [SerializeField] private Text narrationText;
        [SerializeField] private Text speakerDialogueText;
        [SerializeField] private Text progressIndicatorText;

        [Header("Controls")]
        [SerializeField] private Button advanceButton;

        private void OnEnable()
        {
            if (CinematicManager.Instance != null)
            {
                CinematicManager.Instance.OnCinematicStarted += HandleCinematicStarted;
                CinematicManager.Instance.OnSlideChanged += HandleSlideChanged;
                CinematicManager.Instance.OnCinematicFinished += HandleCinematicFinished;
            }

            if (advanceButton != null)
            {
                advanceButton.onClick.AddListener(OnAdvanceClicked);
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

            if (advanceButton != null)
            {
                advanceButton.onClick.RemoveListener(OnAdvanceClicked);
            }
        }

        private void HandleCinematicStarted(CinematicSceneData scene)
        {
            cinematicPanel.SetActive(true);
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            if (sceneTitleText != null)
            {
                sceneTitleText.text = scene.GetTitle(isVi);
            }
        }

        private void HandleSlideChanged(CinematicSceneData scene, CinematicSlide slide, int currentIndex, int totalCount)
        {
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            if (cgDisplayImage != null && slide.cgSprite != null)
            {
                cgDisplayImage.sprite = slide.cgSprite;
            }

            if (narrationText != null)
            {
                narrationText.text = slide.GetNarration(isVi);
            }

            if (speakerDialogueText != null)
            {
                string spoken = slide.GetDialogue(isVi);
                speakerDialogueText.gameObject.SetActive(!string.IsNullOrEmpty(spoken));
                if (!string.IsNullOrEmpty(spoken))
                {
                    speakerDialogueText.text = $"“{spoken}”";
                }
            }

            if (progressIndicatorText != null)
            {
                progressIndicatorText.text = $"{currentIndex + 1} / {totalCount}";
            }

            // Camera / Visual effect presentation
            if (fadeOverlayGroup != null)
            {
                fadeOverlayGroup.alpha = (slide.cameraEffect == CameraEffect.FadeBlack) ? 0.75f : 0f;
            }
        }

        private void OnAdvanceClicked()
        {
            if (CinematicManager.Instance != null)
            {
                CinematicManager.Instance.AdvanceSlide();
            }
        }

        private void HandleCinematicFinished(CinematicSceneData scene)
        {
            cinematicPanel.SetActive(false);
        }
    }
}
