using Edenfall.Core;
using Edenfall.Quests;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.UI
{
    public class QuestJournalUI : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private Transform activeQuestsContainer;
        [SerializeField] private Transform completedQuestsContainer;
        [SerializeField] private GameObject questItemPrefab;

        private void OnEnable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestsUpdated += RefreshUI;
            }
            RefreshUI();
        }

        private void OnDisable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestsUpdated -= RefreshUI;
            }
        }

        public void RefreshUI()
        {
            if (QuestManager.Instance == null || questItemPrefab == null) return;

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            // Active
            if (activeQuestsContainer != null)
            {
                foreach (Transform child in activeQuestsContainer) Destroy(child.gameObject);

                foreach (var q in QuestManager.Instance.ActiveQuests)
                {
                    var obj = Instantiate(questItemPrefab, activeQuestsContainer);
                    var textComp = obj.GetComponentInChildren<Text>();
                    if (textComp != null)
                    {
                        string currentStepDesc = q.currentStepIndex < q.definition.steps.Count
                            ? q.definition.steps[q.currentStepIndex].GetDescription(isVi)
                            : (isVi ? "Hoàn tất các bước!" : "All steps finished!");

                        textComp.text = $"★ {q.definition.GetTitle(isVi)}\n  → {currentStepDesc}\n  [{q.definition.GetSummary(isVi)}]";
                    }
                }
            }

            // Completed
            if (completedQuestsContainer != null)
            {
                foreach (Transform child in completedQuestsContainer) Destroy(child.gameObject);

                foreach (var q in QuestManager.Instance.CompletedQuests)
                {
                    var obj = Instantiate(questItemPrefab, completedQuestsContainer);
                    var textComp = obj.GetComponentInChildren<Text>();
                    if (textComp != null)
                    {
                        textComp.text = $"✔ {q.definition.GetTitle(isVi)} ({(isVi ? "Đã Hoàn Thành" : "Completed")})";
                    }
                }
            }
        }
    }
}
