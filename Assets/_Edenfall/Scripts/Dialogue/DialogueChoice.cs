using System;
using UnityEngine;

namespace Edenfall.Dialogue
{
    public enum DialogueTone
    {
        Romantic,
        Inquisitive,
        Compassionate,
        Defiant,
        Mystical,
        Flirtatious,
        Cautious
    }

    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceId;
        [TextArea(1, 3)] public string textVi;
        [TextArea(1, 3)] public string textEn;
        public DialogueTone tone;

        [Header("Requirements")]
        public int reqAffection = 0;
        public int reqTrust = 0;
        public string reqItemId = "";

        [Header("Relationship Deltas")]
        public int affectionDelta = 0;
        public int trustDelta = 0;
        public int familiarityDelta = 2;
        public int respectDelta = 0;
        public int comfortDelta = 0;
        public int conflictDelta = 0;

        [Header("Memories & Triggers")]
        public string memoryTextVi = "";
        public string memoryTextEn = "";
        public string triggerCinematicId = "";
        public string triggerQuestId = "";
        public int triggerQuestStep = -1;

        public string nextNodeId = "";

        public string GetText(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(textVi) ? textEn : textVi)
                              : (string.IsNullOrEmpty(textEn) ? textVi : textEn);
        }

        public string GetMemoryText(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(memoryTextVi) ? memoryTextEn : memoryTextVi)
                              : (string.IsNullOrEmpty(memoryTextEn) ? memoryTextVi : memoryTextEn);
        }
    }
}
