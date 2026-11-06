using System;
using System.Collections.Generic;
using Edenfall.Core;
using Edenfall.NPC.Relationships;
using UnityEngine;

namespace Edenfall.SaveSystem
{
    [System.Serializable]
    public class SavedRelationship
    {
        public string characterId;
        public int affection;
        public int trust;
        public int familiarity;
        public int respect;
        public int comfort;
        public int conflict;
        public RelationshipStage stage;
        public List<NPCMemory> memories = new();
    }

    [System.Serializable]
    public class SavedItem
    {
        public string itemId;
        public int quantity;
    }

    [System.Serializable]
    public class SavedQuest
    {
        public string questId;
        public int currentStep;
        public bool isCompleted;
    }

    [System.Serializable]
    public class SaveData
    {
        public string version = "1.0.0";
        public string timestamp;
        public int slotIndex;

        [Header("Player & World")]
        public float playerPosX;
        public float playerPosY;
        public TimeOfDay timeOfDay;
        public int currentDay;
        public WeatherType weather;
        public float sanctuaryRestoration;

        [Header("Relationships")]
        public List<SavedRelationship> relationships = new();

        [Header("Inventory")]
        public List<SavedItem> inventory = new();

        [Header("Quests")]
        public List<SavedQuest> quests = new();

        [Header("Gallery")]
        public List<string> unlockedCGs = new();
    }
}
