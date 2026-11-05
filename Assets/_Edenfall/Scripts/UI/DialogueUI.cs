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

        [Header("Speaker Elements")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text speakerTitleText;
        [SerializeField] private Text dialogueBodyText;

        [Header("Choices")]
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Typewriter Settings")]
        [SerializeField] private float charsPerSecond = 35f;

        private Coroutine typewriterRoutine;
        private string fullTextTarget = "";
        private bool isTyping = false;

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

        private void HandleNodeDisplayed(CharacterDefinition speaker, DialogueNode node, Sprite portrait)
        {
            dialoguePanel.SetActive(true);

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            if (speakerNameText != null)
            {
                speakerNameText.text = speaker != null ? speaker.characterName : node.speakerName;
            }

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
            typewriterRoutine = StartCoroutine(TypewriterEffect(fullTextTarget));
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
        }

        private void HandleChoicesAvailable(List<DialogueChoice> choices)
        {
            // Clear old buttons
            if (choicesContainer != null)
            {
                foreach (Transform child in choicesContainer)
                {
                    Destroy(child.gameObject);
                }

                bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

                foreach (var choice in choices)
                {
                    var btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
                    var textComp = btnObj.GetComponentInChildren<Text>();
                    if (textComp != null)
                    {
                        string toneBadge = choice.tone switch
                        {
                            DialogueTone.Romantic => isVi ? "[Tình Cảm] " : "[Romantic] ",
                            DialogueTone.Inquisitive => isVi ? "[Tìm Hiểu] " : "[Inquire] ",
                            DialogueTone.Compassionate => isVi ? "[Thấu Cảm] " : "[Compassion] ",
                            DialogueTone.Defiant => isVi ? "[Kiên Quyết] " : "[Defiant] ",
                            DialogueTone.Flirtatious => isVi ? "[Tán Tỉnh] " : "[Flirt] ",
                            DialogueTone.Mystical => isVi ? "[Huyền Bí] " : "[Mystical] ",
                            _ => ""
                        };
                        textComp.text = toneBadge + choice.GetText(isVi);
                    }

                    var btn = btnObj.GetComponent<Button>();
                    if (btn != null)
                    {
                        var captured = choice;
                        btn.onClick.AddListener(() =>
                        {
                            if (isTyping)
                            {
                                // Fast-forward text
                                if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
                                dialogueBodyText.text = fullTextTarget;
                                isTyping = false;
                            }
                            DialogueManager.Instance.SelectChoice(captured);
                        });
                    }
                }
            }
        }

        private void HandleDialogueFinished()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }
    }
}
