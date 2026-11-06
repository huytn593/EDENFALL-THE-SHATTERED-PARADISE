using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Core;
using Edenfall.NPC.Behavior;
using Edenfall.NPC.Relationships;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.UI
{
    public class RelationshipJournalUI : MonoBehaviour
    {
        [Header("Character Selection")]
        [SerializeField] private List<NPCController> npcs = new();
        [SerializeField] private Text characterNameText;
        [SerializeField] private Text characterTitleText;
        [SerializeField] private Text characterBioText;
        [SerializeField] private Text stageBadgeText;

        [Header("6 Dimension Sliders")]
        [SerializeField] private Slider affectionSlider;
        [SerializeField] private Slider trustSlider;
        [SerializeField] private Slider familiaritySlider;
        [SerializeField] private Slider respectSlider;
        [SerializeField] private Slider comfortSlider;
        [SerializeField] private Slider conflictSlider;

        [SerializeField] private Text affectionValueText;
        [SerializeField] private Text trustValueText;
        [SerializeField] private Text familiarityValueText;
        [SerializeField] private Text respectValueText;
        [SerializeField] private Text comfortValueText;
        [SerializeField] private Text conflictValueText;

        [Header("Memories List")]
        [SerializeField] private Transform memoryContainer;
        [SerializeField] private GameObject memoryItemPrefab;

        private int selectedIndex = 0;

        private void OnEnable()
        {
            RefreshUI();
        }

        public void SelectCharacter(int index)
        {
            selectedIndex = Mathf.Clamp(index, 0, npcs.Count - 1);
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (npcs == null || npcs.Count == 0 || selectedIndex >= npcs.Count) return;

            var npc = npcs[selectedIndex];
            if (npc == null || npc.Definition == null || npc.Relationship == null) return;

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            if (characterNameText != null) characterNameText.text = npc.Definition.characterName;
            if (characterTitleText != null) characterTitleText.text = npc.Definition.title;
            if (characterBioText != null) characterBioText.text = npc.Definition.bio;

            var rel = npc.Relationship;
            if (stageBadgeText != null)
            {
                stageBadgeText.text = isVi ? GetStageNameVi(rel.currentStage) : rel.currentStage.ToString();
            }

            // Dimension stats
            SetSlider(affectionSlider, affectionValueText, rel.affection);
            SetSlider(trustSlider, trustValueText, rel.trust);
            SetSlider(familiaritySlider, familiarityValueText, rel.familiarity);
            SetSlider(respectSlider, respectValueText, rel.respect);
            SetSlider(comfortSlider, comfortValueText, rel.comfort);
            SetSlider(conflictSlider, conflictValueText, rel.conflict);

            // Populate memories
            if (memoryContainer != null && memoryItemPrefab != null)
            {
                foreach (Transform child in memoryContainer)
                {
                    Destroy(child.gameObject);
                }

                foreach (var mem in rel.memories)
                {
                    var item = Instantiate(memoryItemPrefab, memoryContainer);
                    var textComp = item.GetComponentInChildren<Text>();
                    if (textComp != null)
                    {
                        textComp.text = $"[{mem.timestamp}] ({mem.impact}) {mem.text}";
                    }
                }
            }
        }

        private void SetSlider(Slider s, Text t, int val)
        {
            if (s != null) s.value = val / 100f;
            if (t != null) t.text = $"{val}/100";
        }

        private string GetStageNameVi(RelationshipStage stage)
        {
            return stage switch
            {
                RelationshipStage.Stranger => "Người Xa Lạ",
                RelationshipStage.Acquaintance => "Người Quen Biết",
                RelationshipStage.TrustedCompanion => "Bạn Đồng Hành Tin Cậy",
                RelationshipStage.EmotionalConnection => "Thấu Cảm Tâm Hồn",
                RelationshipStage.RomanticPossibility => "Khởi Đầu Tình Cảm",
                RelationshipStage.EstablishedRelationship => "Gắn Kết Định Mệnh",
                RelationshipStage.StoryResolution => "Nguyện Ước Viên Mãn",
                _ => stage.ToString()
            };
        }
    }
}
