using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Cinematics;
using Edenfall.Core;
using UnityEngine;

namespace Edenfall.Events
{
    [System.Serializable]
    public class MatureEventSlide
    {
        public CharacterEmotion characterEmotion = CharacterEmotion.Gentle;
        [TextArea(2, 4)] public string narrationVi;
        [TextArea(2, 4)] public string narrationEn;
        [TextArea(2, 4)] public string dialogueVi;
        [TextArea(2, 4)] public string dialogueEn;
        public CameraEffect cameraEffect = CameraEffect.ZoomIn;
        public bool isFadeToBlack = false;
        public Sprite slideCG;

        public string GetNarration(bool isVi = true)
        {
            return isVi ? (string.IsNullOrEmpty(narrationVi) ? narrationEn : narrationVi)
                        : (string.IsNullOrEmpty(narrationEn) ? narrationVi : narrationEn);
        }

        public string GetDialogue(bool isVi = true)
        {
            return isVi ? (string.IsNullOrEmpty(dialogueVi) ? dialogueEn : dialogueVi)
                        : (string.IsNullOrEmpty(dialogueEn) ? dialogueVi : dialogueEn);
        }
    }

    [CreateAssetMenu(fileName = "MatureEventDefinition", menuName = "Edenfall/Mature Event Definition")]
    public class MatureEventDefinition : ScriptableObject
    {
        [Header("Event Identification")]
        public string eventId;
        public string titleVi;
        public string titleEn;
        public string characterId = "aurelia";
        [TextArea(2, 4)] public string summaryVi;
        [TextArea(2, 4)] public string summaryEn;

        [Header("Prerequisites (18+ Cast Only)")]
        [Range(0, 100)] public int minAffection = 40;
        [Range(0, 100)] public int minTrust = 35;
        [Range(0, 100)] public int minComfort = 30;
        public string requiredLocationId = "loc_aurelia_chamber";
        public TimeOfDay requiredTimeOfDay = TimeOfDay.Night;
        public bool anyTimeOfDay = false;

        [Header("Rewards & Progression")]
        public int bonusAffection = 20;
        public int bonusTrust = 15;
        public int bonusComfort = 20;
        public string memoryRecordVi = "Đã trải qua khoảnh khắc thân mật riêng tư cùng Aurelia trong sương mai.";
        public string memoryRecordEn = "Shared an intimate private morning memory with Aurelia in her chamber.";

        [Header("Visuals & Slides")]
        public Sprite mainCGSprite;
        public List<MatureEventSlide> slides = new();

        public string GetTitle(bool isVi = true)
        {
            return isVi ? (string.IsNullOrEmpty(titleVi) ? titleEn : titleVi)
                        : (string.IsNullOrEmpty(titleEn) ? titleVi : titleEn);
        }
    }
}
