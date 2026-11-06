using System;
using System.Collections.Generic;
using Edenfall.Characters;
using UnityEngine;

namespace Edenfall.Dialogue
{
    [System.Serializable]
    public class DialogueNode
    {
        public string nodeId;
        public string speakerName;
        public CharacterEmotion emotion = CharacterEmotion.Gentle;
        [TextArea(2, 5)] public string textVi;
        [TextArea(2, 5)] public string textEn;
        public List<DialogueChoice> choices = new();

        public string GetText(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(textVi) ? textEn : textVi)
                              : (string.IsNullOrEmpty(textEn) ? textVi : textEn);
        }
    }
}
