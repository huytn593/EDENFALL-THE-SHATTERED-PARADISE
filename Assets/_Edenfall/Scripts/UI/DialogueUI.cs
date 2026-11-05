using System.Collections;
using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Core;
using Edenfall.Dialogue;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("Root Panel")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Speaker Elements")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private Text speakerNameText;
        [SerializeField] private GameObject nameBadgeObj;
        [SerializeField] private Text speakerTitleText;
        [SerializeField] private Text dialogueBodyText;

        [Header("Choices")]
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("VN Action Buttons & Prompts")]
        [SerializeField] private Button autoPlayButton;
        [SerializeField] private Text autoPlayText;
        [SerializeField] private Button skipButton;
        [SerializeField] private Text skipText;
        [SerializeField] private Button logButton;
        [SerializeField] private Button hideUIButton;

        [Header("Dialogue Log Modal")]
        [SerializeField] private GameObject logModal;
        [SerializeField] private Transform logContentContainer;
        [SerializeField] private GameObject logEntryPrefab;
        [SerializeField] private Button closeLogButton;

        [Header("Typewriter Settings")]
        [SerializeField] private float charsPerSecond = 45f;
        [SerializeField] private float autoPlayDelay = 1.6f;

        private Coroutine typewriterRoutine;
        private Coroutine autoPlayRoutine;
        private string fullTextTarget = "";
        private bool isTyping = false;
        private bool isAutoPlay = false;
        private bool isSkipping = false;
        private bool isUIHidden = false;

        private void Awake()
        {
            if (autoPlayButton != null) autoPlayButton.onClick.AddListener(ToggleAutoPlay);
            if (skipButton != null) skipButton.onClick.AddListener(ToggleSkip);
            if (logButton != null) logButton.onClick.AddListener(OpenLogModal);
            if (hideUIButton != null) hideUIButton.onClick.AddListener(ToggleHideUI);
            if (closeLogButton != null) closeLogButton.onClick.AddListener(CloseLogModal);
        }

        private void OnEnable()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed += HandleNodeDisplayed;
                DialogueManager.Instance.OnChoicesAvailable += HandleChoicesAvailable;
                DialogueManager.Instance.OnDialogueFinished += HandleDialogueFinished;
            }
        }

        private void OnDisable()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed -= HandleNodeDisplayed;
                DialogueManager.Instance.OnChoicesAvailable -= HandleChoicesAvailable;
                DialogueManager.Instance.OnDialogueFinished -= HandleDialogueFinished;
            }
        }

        public void OnClickDialogueBox()
        {
            if (isUIHidden)
            {
                SetUIHidden(false);
                return;
            }

            if (isTyping)
            {
                // Instantly complete typewriter
                if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
                dialogueBodyText.text = fullTextTarget;
                isTyping = false;
            }
        }

        private void HandleNodeDisplayed(CharacterDefinition speaker, DialogueNode node, Sprite portrait)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            SetUIHidden(false);

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            string name = speaker != null ? speaker.characterName : node.speakerName;
            if (speakerNameText != null) speakerNameText.text = name;
            if (nameBadgeObj != null) nameBadgeObj.SetActive(!string.IsNullOrEmpty(name));

            if (speakerTitleText != null && speaker != null)
            {
                speakerTitleText.text = speaker.title;
            }

            if (portraitImage != null)
            {
                portraitImage.gameObject.SetActive(portrait != null);
                if (portrait != null) portraitImage.sprite = portrait;
            }

            fullTextTarget = node.GetText(isVi);

            if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
            if (autoPlayRoutine != null) StopCoroutine(autoPlayRoutine);

            if (isSkipping)
            {
                dialogueBodyText.text = fullTextTarget;
                isTyping = false;
            }
            else
            {
                typewriterRoutine = StartCoroutine(TypewriterEffect(fullTextTarget));
            }
        }

        private IEnumerator TypewriterEffect(string text)
        {
            isTyping = true;
            dialogueBodyText.text = "";

            float delay = 1f / charsPerSecond;
            foreach (char c in text)
            {
                dialogueBodyText.text += c;
                yield return new WaitForSeconds(delay);
            }

            isTyping = false;

            if (isAutoPlay)
            {
                autoPlayRoutine = StartCoroutine(AutoPlayWait());
            }
        }

        private IEnumerator AutoPlayWait()
        {
            yield return new WaitForSeconds(autoPlayDelay);
            // If choices aren't showing, advance if possible or wait
        }

        private void HandleChoicesAvailable(List<DialogueChoice> choices)
        {
            if (choicesContainer == null) return;

            foreach (Transform child in choicesContainer)
            {
                Destroy(child.gameObject);
            }

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            foreach (var choice in choices)
            {
                var choiceBtn = CreateChoiceButton(choice.GetText(isVi), () =>
                {
                    DialogueManager.Instance.SelectChoice(choice);
                });
                choiceBtn.transform.SetParent(choicesContainer, false);
            }
        }

        private void HandleDialogueFinished()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (autoPlayRoutine != null) StopCoroutine(autoPlayRoutine);
            if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
            isTyping = false;
        }

        public void ToggleAutoPlay()
        {
            isAutoPlay = !isAutoPlay;
            if (autoPlayText != null) autoPlayText.color = isAutoPlay ? new Color(1.0f, 0.85f, 0.4f) : Color.white;
            if (isAutoPlay && !isTyping)
            {
                autoPlayRoutine = StartCoroutine(AutoPlayWait());
            }
        }

        public void ToggleSkip()
        {
            isSkipping = !isSkipping;
            if (skipText != null) skipText.color = isSkipping ? new Color(1.0f, 0.85f, 0.4f) : Color.white;
            if (isSkipping && isTyping)
            {
                if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
                dialogueBodyText.text = fullTextTarget;
                isTyping = false;
            }
        }

        public void ToggleHideUI()
        {
            SetUIHidden(!isUIHidden);
        }

        public void SetUIHidden(bool hidden)
        {
            isUIHidden = hidden;
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = isUIHidden ? 0f : 1f;
                panelCanvasGroup.blocksRaycasts = !isUIHidden;
            }
        }

        public void OpenLogModal()
        {
            if (logModal == null) return;
            logModal.SetActive(true);
            PopulateLog();
        }

        public void CloseLogModal()
        {
            if (logModal != null) logModal.SetActive(false);
        }

        private void PopulateLog()
        {
            if (logContentContainer == null || DialogueLogManager.Instance == null) return;

            foreach (Transform child in logContentContainer)
            {
                Destroy(child.gameObject);
            }

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            foreach (var entry in DialogueLogManager.Instance.LogEntries)
            {
                var entryObj = new GameObject("LogEntry");
                entryObj.transform.SetParent(logContentContainer, false);
                var txt = entryObj.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 18;
                txt.color = Color.white;
                txt.text = $"<b><color=#FFD566>{entry.speakerName}:</color></b> {entry.GetText(isVi)}\n";
            }
        }

        private GameObject CreateChoiceButton(string label, UnityEngine.Events.UnityAction onClick)
        {
            var btnObj = new GameObject("ChoiceButton");
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.12f, 0.16f, 0.28f, 0.90f);

            var btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            var txt = txtObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = label;
            txt.fontSize = 20;
            txt.color = new Color(1.0f, 0.95f, 0.85f);
            txt.alignment = TextAnchor.MiddleCenter;

            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(15, 8);
            txtRt.offsetMax = new Vector2(-15, -8);

            var btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(650, 48);

            return btnObj;
        }
    }
}
