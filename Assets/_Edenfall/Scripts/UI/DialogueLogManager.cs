using System;
using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Dialogue;
using UnityEngine;

namespace Edenfall.UI
{
    [System.Serializable]
    public class DialogueLogEntry
    {
        public string speakerName;
        public string textVi;
        public string textEn;
        public CharacterEmotion emotion;

        public string GetText(bool isVi = true)
        {
            return isVi ? (string.IsNullOrEmpty(textVi) ? textEn : textVi)
                        : (string.IsNullOrEmpty(textEn) ? textVi : textEn);
        }
    }

    public class DialogueLogManager : MonoBehaviour
    {
        public static DialogueLogManager Instance { get; private set; }

        [SerializeField] private List<DialogueLogEntry> logEntries = new();
        public IReadOnlyList<DialogueLogEntry> LogEntries => logEntries;

        public event Action<DialogueLogEntry> OnEntryAdded;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed += HandleNodeDisplayed;
            }
        }

        private void OnDisable()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed -= HandleNodeDisplayed;
            }
        }

        private void HandleNodeDisplayed(CharacterDefinition speaker, DialogueNode node, Sprite portrait)
        {
            if (node == null) return;

            var entry = new DialogueLogEntry
            {
                speakerName = speaker != null ? speaker.characterName : node.speakerName,
                textVi = node.textVi,
                textEn = node.textEn,
                emotion = node.emotion
            };

            logEntries.Add(entry);
            OnEntryAdded?.Invoke(entry);
        }

        public void ClearLog()
        {
            logEntries.Clear();
        }
    }
}
