using System;
using System.Collections.Generic;
using Edenfall.Core;
using UnityEngine;

namespace Edenfall.Quests
{
    [System.Serializable]
    public class QuestProgress
    {
        public QuestDefinition definition;
        public int currentStepIndex = 0;
        public bool isCompleted = false;

        public QuestProgress(QuestDefinition def)
        {
            definition = def;
            currentStepIndex = 0;
            isCompleted = false;
        }
    }

    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("Starting Quests")]
        [SerializeField] private List<QuestDefinition> startingQuests = new();
        [SerializeField] private List<QuestProgress> activeQuests = new();
        [SerializeField] private List<QuestProgress> completedQuests = new();

        public IReadOnlyList<QuestProgress> ActiveQuests => activeQuests;
        public IReadOnlyList<QuestProgress> CompletedQuests => completedQuests;

        public event Action OnQuestsUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                InitializeQuests();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnQuestStarted += HandleQuestStarted;
            GameEvents.OnQuestStepUpdated += HandleQuestStepUpdated;
        }

        private void OnDisable()
        {
            GameEvents.OnQuestStarted -= HandleQuestStarted;
            GameEvents.OnQuestStepUpdated -= HandleQuestStepUpdated;
        }

        private void InitializeQuests()
        {
            foreach (var q in startingQuests)
            {
                activeQuests.Add(new QuestProgress(q));
            }
        }

        private void HandleQuestStarted(string questId)
        {
            if (activeQuests.Exists(q => q.definition.questId == questId) ||
                completedQuests.Exists(q => q.definition.questId == questId))
            {
                return;
            }

            var def = startingQuests.Find(q => q.questId == questId);
            if (def != null)
            {
                activeQuests.Add(new QuestProgress(def));
                OnQuestsUpdated?.Invoke();
            }
        }

        private void HandleQuestStepUpdated(string questId, int stepIndex)
        {
            var q = activeQuests.Find(x => x.definition.questId == questId);
            if (q != null)
            {
                q.currentStepIndex = Mathf.Clamp(stepIndex, 0, q.definition.steps.Count);
                if (q.currentStepIndex >= q.definition.steps.Count)
                {
                    CompleteQuest(questId);
                }
                else
                {
                    OnQuestsUpdated?.Invoke();
                }
            }
        }

        public void CompleteQuest(string questId)
        {
            var q = activeQuests.Find(x => x.definition.questId == questId);
            if (q != null)
            {
                q.isCompleted = true;
                activeQuests.Remove(q);
                completedQuests.Add(q);

                if (q.definition.rewardResonance > 0 && GameManager.Instance != null)
                {
                    GameManager.Instance.AddSanctuaryRestoration(q.definition.rewardResonance);
                }

                GameEvents.TriggerQuestCompleted(questId);
                OnQuestsUpdated?.Invoke();
            }
        }
    }
}
