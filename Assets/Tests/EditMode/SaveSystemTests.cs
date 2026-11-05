using System.IO;
using Edenfall.SaveSystem;
using NUnit.Framework;
using UnityEngine;

namespace Edenfall.Tests.EditMode
{
    public class SaveSystemTests
    {
        [Test]
        public void SaveData_Serialization_PreservesAllFields()
        {
            var data = new SaveData
            {
                slotIndex = 0,
                currentDay = 3,
                sanctuaryRestoration = 45.5f,
                unlockedCGs = new System.Collections.Generic.List<string> { "cg_aurelia_garden" }
            };

            string json = JsonUtility.ToJson(data);
            var loaded = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(0, loaded.slotIndex);
            Assert.AreEqual(3, loaded.currentDay);
            Assert.AreEqual(45.5f, loaded.sanctuaryRestoration, 0.01f);
            Assert.Contains("cg_aurelia_garden", loaded.unlockedCGs);
        }
    }
}
