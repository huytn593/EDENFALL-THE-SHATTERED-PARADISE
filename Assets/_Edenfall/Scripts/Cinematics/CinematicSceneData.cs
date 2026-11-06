using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Core;
using UnityEngine;

namespace Edenfall.Cinematics
{
    public enum CameraEffect
    {
        PanUp,
        ZoomIn,
        SoftGlow,
        Radiance,
        FadeBlack
    }

    [System.Serializable]
    public class CinematicSlide
    {
        public CharacterEmotion characterEmotion;
        [TextArea(2, 4)] public string narrationVi;
        [TextArea(2, 4)] public string narrationEn;
        [TextArea(2, 4)] public string dialogueVi;
        [TextArea(2, 4)] public string dialogueEn;
        public CameraEffect cameraEffect;
        public Sprite cgSprite;

        public string GetNarration(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(narrationVi) ? narrationEn : narrationVi)
                              : (string.IsNullOrEmpty(narrationEn) ? narrationVi : narrationEn);
        }

        public string GetDialogue(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(dialogueVi) ? dialogueEn : dialogueVi)
                              : (string.IsNullOrEmpty(dialogueEn) ? dialogueVi : dialogueEn);
        }
    }

    [CreateAssetMenu(fileName = "CinematicScene", menuName = "Edenfall/Cinematic Scene")]
    public class CinematicSceneData : ScriptableObject
    {
        public string sceneId;
        public string titleVi;
        public string titleEn;
        public string characterId;
        public string characterName;
        [TextArea(2, 4)] public string descriptionVi;
        [TextArea(2, 4)] public string descriptionEn;

        [Header("Unlock Conditions")]
        public int minAffection = 0;
        public int minTrust = 0;
        public bool hasTimeRequirement = false;
        public TimeOfDay requiredTimeOfDay = TimeOfDay.Evening;
        public string requiredLocationId = "";

        [Header("Cinematic Content")]
        public List<CinematicSlide> slides = new();

        [Header("Rewards upon Completion")]
        public int bonusAffection = 15;
        public int bonusTrust = 15;
        public string completionMemoryVi = "";
        public string completionMemoryEn = "";

        public string GetTitle(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(titleVi) ? titleEn : titleVi)
                              : (string.IsNullOrEmpty(titleEn) ? titleVi : titleEn);
        }

        public string GetDescription(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(descriptionVi) ? descriptionEn : descriptionVi)
                              : (string.IsNullOrEmpty(descriptionEn) ? descriptionVi : descriptionEn);
        }
    }
}
