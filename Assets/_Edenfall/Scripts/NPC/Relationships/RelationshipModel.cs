using System;
using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Core;
using UnityEngine;

namespace Edenfall.NPC.Relationships
{
    public enum RelationshipStage
    {
        Stranger,
        Acquaintance,
        TrustedCompanion,
        EmotionalConnection,
        RomanticPossibility,
        EstablishedRelationship,
        StoryResolution
    }

    [System.Serializable]
    public class NPCMemory
    {
        public string id;
        public string text;
        public string timestamp;
        public MemoryImpact impact;

        public enum MemoryImpact
        {
            Positive,
            Negative,
            Neutral,
            Romantic
        }
    }

    [System.Serializable]
    public class RelationshipModel
    {
        public string characterId;
        public CharacterEmotion currentEmotion = CharacterEmotion.Gentle;

        // 6 Independent dimensions per directive 5
        [Range(0, 100)] public int affection;
        [Range(0, 100)] public int trust;
        [Range(0, 100)] public int familiarity;
        [Range(0, 100)] public int respect;
        [Range(0, 100)] public int comfort;
        [Range(0, 100)] public int conflict;

        public RelationshipStage currentStage = RelationshipStage.Stranger;
        public List<NPCMemory> memories = new();

        public RelationshipModel(string id)
        {
            characterId = id;
            affection = 0;
            trust = 0;
            familiarity = 0;
            respect = 0;
            comfort = 0;
            conflict = 0;
            currentStage = RelationshipStage.Stranger;
        }

        public void ApplyDeltas(int aff, int tru, int fam, int res, int com, int con)
        {
            affection = Mathf.Clamp(affection + aff, 0, 100);
            trust = Mathf.Clamp(trust + tru, 0, 100);
            familiarity = Mathf.Clamp(familiarity + fam, 0, 100);
            respect = Mathf.Clamp(respect + res, 0, 100);
            comfort = Mathf.Clamp(comfort + com, 0, 100);
            conflict = Mathf.Clamp(conflict + con, 0, 100);

            EvaluateStage();

            if (aff != 0) GameEvents.TriggerRelationshipStatChanged(characterId, "affection", aff);
            if (tru != 0) GameEvents.TriggerRelationshipStatChanged(characterId, "trust", tru);
            if (res != 0) GameEvents.TriggerRelationshipStatChanged(characterId, "respect", res);
        }

        public void AddMemory(string text, NPCMemory.MemoryImpact impact, string dayTimeInfo)
        {
            var mem = new NPCMemory
            {
                id = Guid.NewGuid().ToString("N")[..8],
                text = text,
                timestamp = dayTimeInfo,
                impact = impact
            };
            memories.Add(mem);
        }

        public void EvaluateStage()
        {
            var oldStage = currentStage;

            // Multidimensional stage gating
            if (affection >= 85 && trust >= 80 && comfort >= 75)
            {
                currentStage = RelationshipStage.StoryResolution;
            }
            else if (affection >= 70 && trust >= 65 && comfort >= 60)
            {
                currentStage = RelationshipStage.EstablishedRelationship;
            }
            else if (affection >= 50 && trust >= 45 && comfort >= 40)
            {
                currentStage = RelationshipStage.RomanticPossibility;
            }
            else if (affection >= 35 && trust >= 30 && familiarity >= 25)
            {
                currentStage = RelationshipStage.EmotionalConnection;
            }
            else if (trust >= 25 && respect >= 30)
            {
                currentStage = RelationshipStage.TrustedCompanion;
            }
            else if (familiarity >= 10 || trust >= 10)
            {
                currentStage = RelationshipStage.Acquaintance;
            }
            else
            {
                currentStage = RelationshipStage.Stranger;
            }

            if (currentStage != oldStage)
            {
                GameEvents.TriggerRelationshipStageAdvanced(characterId, currentStage.ToString());
            }
        }
    }
}
