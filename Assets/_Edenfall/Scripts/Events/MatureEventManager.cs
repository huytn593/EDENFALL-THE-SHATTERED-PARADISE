using System;
using System.Collections;
using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Cinematics;
using Edenfall.Core;
using Edenfall.Dialogue;
using Edenfall.NPC.Relationships;
using Edenfall.World;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.Events
{
    public class MatureEventManager : MonoBehaviour
    {
        public static MatureEventManager Instance { get; private set; }

        [Header("Event Database")]
        [SerializeField] private List<MatureEventDefinition> allEvents = new();

        [Header("UI References")]
        [SerializeField] private GameObject eventCanvas;
        [SerializeField] private Image fullScreenCGImage;
        [SerializeField] private Image fadeOverlay;
        [SerializeField] private Text eventSpeakerText;
        [SerializeField] private Text eventDialogueText;
        [SerializeField] private Text eventNarrationText;
        [SerializeField] private Button nextSlideButton;
        [SerializeField] private Button skipEventButton;

        [Header("Runtime State")]
        private MatureEventDefinition currentEvent;
        private int currentSlideIndex = 0;
        private bool isEventRunning = false;
        private Coroutine fadeRoutine;

        public bool IsEventRunning => isEventRunning;

        public event Action<MatureEventDefinition> OnMatureEventStarted;
        public event Action<MatureEventDefinition> OnMatureEventCompleted;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (nextSlideButton != null) nextSlideButton.onClick.AddListener(NextSlide);
            if (skipEventButton != null) skipEventButton.onClick.AddListener(FinishEvent);
            if (eventCanvas != null) eventCanvas.SetActive(false);
        }

        public MatureEventDefinition GetAvailableEvent(string locationId, TimeOfDay time, RelationshipModel relationship)
        {
            if (relationship == null) return null;

            foreach (var evt in allEvents)
            {
                if (evt == null) continue;

                if (GalleryManager.Instance != null && GalleryManager.Instance.IsUnlocked(evt.eventId))
                {
                    // Already unlocked/completed, can still replay via gallery
                    continue;
                }

                if (evt.requiredLocationId == locationId &&
                    (evt.anyTimeOfDay || evt.requiredTimeOfDay == time) &&
                    relationship.affection >= evt.minAffection &&
                    relationship.trust >= evt.minTrust &&
                    relationship.comfort >= evt.minComfort)
                {
                    return evt;
                }
            }
            return null;
        }

        public void PlayMatureEvent(MatureEventDefinition evt)
        {
            if (evt == null || isEventRunning) return;

            currentEvent = evt;
            currentSlideIndex = 0;
            isEventRunning = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.Cinematic);
            }

            if (eventCanvas != null) eventCanvas.SetActive(true);
            OnMatureEventStarted?.Invoke(currentEvent);

            ShowSlide(currentSlideIndex);
        }

        public void NextSlide()
        {
            if (!isEventRunning || currentEvent == null) return;

            currentSlideIndex++;
            if (currentSlideIndex >= currentEvent.slides.Count)
            {
                FinishEvent();
            }
            else
            {
                ShowSlide(currentSlideIndex);
            }
        }

        private void ShowSlide(int index)
        {
            if (currentEvent == null || index < 0 || index >= currentEvent.slides.Count) return;

            var slide = currentEvent.slides[index];
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            // CG Image
            Sprite targetCG = slide.slideCG != null ? slide.slideCG : currentEvent.mainCGSprite;
            if (fullScreenCGImage != null)
            {
                fullScreenCGImage.gameObject.SetActive(targetCG != null);
                if (targetCG != null) fullScreenCGImage.sprite = targetCG;
            }

            // Fade handling
            if (slide.isFadeToBlack)
            {
                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                fadeRoutine = StartCoroutine(DoFade(0f, 0.95f, 0.8f));
            }
            else
            {
                if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 0);
            }

            // Dialogue & Narration
            if (eventSpeakerText != null)
            {
                string speaker = !string.IsNullOrEmpty(slide.GetDialogue(isVi)) ? "Aurelia" : "";
                eventSpeakerText.text = speaker;
            }

            if (eventDialogueText != null)
            {
                eventDialogueText.text = slide.GetDialogue(isVi);
                eventDialogueText.gameObject.SetActive(!string.IsNullOrEmpty(eventDialogueText.text));
            }

            if (eventNarrationText != null)
            {
                eventNarrationText.text = slide.GetNarration(isVi);
                eventNarrationText.gameObject.SetActive(!string.IsNullOrEmpty(eventNarrationText.text));
            }
        }

        private IEnumerator DoFade(float startA, float targetA, float duration)
        {
            if (fadeOverlay == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(startA, targetA, elapsed / duration);
                fadeOverlay.color = new Color(0, 0, 0, a);
                yield return null;
            }
            fadeOverlay.color = new Color(0, 0, 0, targetA);
        }

        private void FinishEvent()
        {
            if (!isEventRunning || currentEvent == null) return;

            // Apply rewards to Aurelia's relationship if available
            var aureliaDef = Resources.Load<CharacterDefinition>("Data/Characters/Character_Aurelia");
            // Also log memory
            string timeInfo = GameManager.Instance != null ? GameManager.Instance.CurrentTime.ToString() : "Night";
            // Unlock in Gallery
            if (GalleryManager.Instance != null)
            {
                GalleryManager.Instance.UnlockScene(currentEvent.eventId);
            }

            if (eventCanvas != null) eventCanvas.SetActive(false);
            if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 0);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.Exploration);
            }

            isEventRunning = false;
            OnMatureEventCompleted?.Invoke(currentEvent);
            currentEvent = null;
        }
    }
}
