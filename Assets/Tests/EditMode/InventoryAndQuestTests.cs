using System.Collections.Generic;
using Edenfall.Inventory;
using Edenfall.Quests;
using NUnit.Framework;
using UnityEngine;

namespace Edenfall.Tests.EditMode
{
    public class InventoryAndQuestTests
    {
        [Test]
        public void QuestDefinition_GetTitle_ReturnsLocalizedOrFallback()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.questId = "quest_spire";
            quest.titleVi = "Đánh Thức Vùng Đất Vỡ Vụn";
            quest.titleEn = "The Shattered Awakening";

            Assert.AreEqual("Đánh Thức Vùng Đất Vỡ Vụn", quest.GetTitle(true));
            Assert.AreEqual("The Shattered Awakening", quest.GetTitle(false));
        }

        [Test]
        public void QuestProgress_StepAdvancement_TracksCorrectly()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.questId = "quest_spire";
            quest.steps = new List<QuestStep>
            {
                new QuestStep { descriptionVi = "Đến Tòa Tháp Vỡ Vụn" },
                new QuestStep { descriptionVi = "Hòa nhịp vào Lõi Tinh Tú" }
            };

            var progress = new QuestProgress(quest);
            Assert.AreEqual(0, progress.currentStepIndex);
            Assert.IsFalse(progress.isCompleted);

            progress.currentStepIndex = 1;
            Assert.AreEqual(1, progress.currentStepIndex);
        }

        [Test]
        public void ItemDefinition_GetName_HandlesBilingualLookup()
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = "celestial_lily";
            item.nameVi = "Hoa Ly Tinh Tú";
            item.nameEn = "Dawn Celestial Lily";

            Assert.AreEqual("Hoa Ly Tinh Tú", item.GetName(true));
            Assert.AreEqual("Dawn Celestial Lily", item.GetName(false));
        }
    }
}
