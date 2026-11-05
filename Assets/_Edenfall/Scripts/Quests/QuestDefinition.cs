using System.Collections.Generic;
using UnityEngine;

namespace Edenfall.Quests
{
    public enum QuestCategory
    {
        Main,
        Character,
        Exploration,
        Sanctuary
    }

    [System.Serializable]
    public class QuestStep
    {
        [TextArea(1, 3)] public string descriptionVi;
        [TextArea(1, 3)] public string descriptionEn;
        public string targetLocationId;
        public bool isCompleted;

        public string GetDescription(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(descriptionVi) ? descriptionEn : descriptionVi)
                              : (string.IsNullOrEmpty(descriptionEn) ? descriptionVi : descriptionEn);
        }
    }

    [CreateAssetMenu(fileName = "QuestDefinition", menuName = "Edenfall/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        public string questId;
        public string titleVi;
        public string titleEn;
        public QuestCategory category;

        [TextArea(2, 4)] public string summaryVi;
        [TextArea(2, 4)] public string summaryEn;

        public List<QuestStep> steps = new();
        public string rewardTextVi;
        public string rewardTextEn;
        public float rewardResonance = 15f;

        public string GetTitle(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(titleVi) ? titleEn : titleVi)
                              : (string.IsNullOrEmpty(titleEn) ? titleVi : titleEn);
        }

        public string GetSummary(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(summaryVi) ? summaryEn : summaryVi)
                              : (string.IsNullOrEmpty(summaryEn) ? summaryVi : summaryEn);
        }
    }
}
