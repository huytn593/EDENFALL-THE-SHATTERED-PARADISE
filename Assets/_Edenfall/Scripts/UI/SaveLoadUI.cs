using Edenfall.Core;
using Edenfall.SaveSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.UI
{
    public class SaveLoadUI : MonoBehaviour
    {
        [System.Serializable]
        public struct SlotDisplay
        {
            public Text infoText;
            public Button saveButton;
            public Button loadButton;
        }

        [SerializeField] private SlotDisplay[] slots = new SlotDisplay[3];
        [SerializeField] private Transform playerTransform;

        private void OnEnable()
        {
            RefreshSlots();
        }

        public void RefreshSlots()
        {
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            for (int i = 0; i < slots.Length; i++)
            {
                int slotIndex = i;
                bool exists = Edenfall.SaveSystem.SaveSystem.DoesSaveExist(slotIndex);

                if (exists)
                {
                    var data = Edenfall.SaveSystem.SaveSystem.Load(slotIndex);
                    if (data != null && slots[i].infoText != null)
                    {
                        slots[i].infoText.text = isVi
                            ? $"Ô {slotIndex + 1}: Ngày {data.currentDay} ({data.timeOfDay}) - {data.timestamp}"
                            : $"Slot {slotIndex + 1}: Day {data.currentDay} ({data.timeOfDay}) - {data.timestamp}";
                    }
                }
                else
                {
                    if (slots[i].infoText != null)
                    {
                        slots[i].infoText.text = isVi ? $"Ô {slotIndex + 1}: [Trống]" : $"Slot {slotIndex + 1}: [Empty]";
                    }
                }

                if (slots[i].saveButton != null)
                {
                    slots[i].saveButton.onClick.RemoveAllListeners();
                    slots[i].saveButton.onClick.AddListener(() => OnClickSave(slotIndex));
                }

                if (slots[i].loadButton != null)
                {
                    slots[i].loadButton.interactable = exists;
                    slots[i].loadButton.onClick.RemoveAllListeners();
                    slots[i].loadButton.onClick.AddListener(() => OnClickLoad(slotIndex));
                }
            }
        }

        private void OnClickSave(int slot)
        {
            Edenfall.SaveSystem.SaveSystem.Save(slot, playerTransform);
            RefreshSlots();
        }

        private void OnClickLoad(int slot)
        {
            var data = Edenfall.SaveSystem.SaveSystem.Load(slot);
            if (data != null)
            {
                if (playerTransform != null)
                {
                    playerTransform.position = new Vector3(data.playerPosX, data.playerPosY, 0);
                }
                // Reload world values
                RefreshSlots();
                gameObject.SetActive(false);
            }
        }
    }
}
