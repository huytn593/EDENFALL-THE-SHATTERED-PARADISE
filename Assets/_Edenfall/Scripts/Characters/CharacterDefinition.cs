using System;
using System.Collections.Generic;
using UnityEngine;

namespace Edenfall.Characters
{
    [System.Serializable]
    public struct EmotionPortrait
    {
        public CharacterEmotion emotion;
        public Sprite portraitSprite;
    }

    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Edenfall/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [Header("Identity (18+ Adult Cast)")]
        public string characterId = "aurelia";
        public string characterName = "Aurelia";
        public string title = "The Solstice Heir";
        public string role = "Luminary Seeker & Architect of the Dawn";
        [Range(18, 999)] public int age = 20;
        [TextArea(3, 6)] public string bio;
        public string defaultLocation = "Dawn Garden";

        [Header("Gifting")]
        public List<string> favoriteGiftItemIds = new();

        [Header("Initial Relationship Values")]
        [Range(0, 100)] public int startingAffection = 35;
        [Range(0, 100)] public int startingTrust = 40;
        [Range(0, 100)] public int startingFamiliarity = 30;
        [Range(0, 100)] public int startingRespect = 45;
        [Range(0, 100)] public int startingComfort = 35;
        [Range(0, 100)] public int startingConflict = 5;

        [Header("Portraits")]
        public List<EmotionPortrait> portraits = new();

        public Sprite GetPortrait(CharacterEmotion emotion)
        {
            foreach (var item in portraits)
            {
                if (item.emotion == emotion) return item.portraitSprite;
            }
            return portraits.Count > 0 ? portraits[0].portraitSprite : null;
        }
    }
}
