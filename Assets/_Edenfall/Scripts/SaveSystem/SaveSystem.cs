using System;
using System.Collections.Generic;
using System.IO;
using Edenfall.Cinematics;
using Edenfall.Core;
using Edenfall.Inventory;
using Edenfall.Quests;
using UnityEngine;

namespace Edenfall.SaveSystem
{
    public static class SaveSystem
    {
        private static string GetSavePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"edenfall_save_slot_{slot}.json");
        }

        public static bool DoesSaveExist(int slot)
        {
            return File.Exists(GetSavePath(slot));
        }

        public static bool Save(int slot, Transform playerTransform = null)
        {
            try
            {
                var data = new SaveData
                {
                    slotIndex = slot,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                if (playerTransform != null)
                {
                    data.playerPosX = playerTransform.position.x;
                    data.playerPosY = playerTransform.position.y;
                }

                if (GameManager.Instance != null)
                {
                    data.timeOfDay = GameManager.Instance.CurrentTime;
                    data.currentDay = GameManager.Instance.CurrentDay;
                    data.weather = GameManager.Instance.CurrentWeather;
                    data.sanctuaryRestoration = GameManager.Instance.SanctuaryRestoration;
                }

                // Inventory
                if (InventoryManager.Instance != null)
                {
                    foreach (var s in InventoryManager.Instance.Slots)
                    {
                        data.inventory.Add(new SavedItem
                        {
                            itemId = s.item.itemId,
                            quantity = s.quantity
                        });
                    }
                }

                // Quests
                if (QuestManager.Instance != null)
                {
                    foreach (var q in QuestManager.Instance.ActiveQuests)
                    {
                        data.quests.Add(new SavedQuest
                        {
                            questId = q.definition.questId,
                            currentStep = q.currentStepIndex,
                            isCompleted = false
                        });
                    }
                    foreach (var q in QuestManager.Instance.CompletedQuests)
                    {
                        data.quests.Add(new SavedQuest
                        {
                            questId = q.definition.questId,
                            currentStep = q.definition.steps.Count,
                            isCompleted = true
                        });
                    }
                }

                // Gallery
                if (GalleryManager.Instance != null)
                {
                    data.unlockedCGs = new List<string>(GalleryManager.Instance.UnlockedSceneIds);
                }

                string json = JsonUtility.ToJson(data, true);
                string filePath = GetSavePath(slot);
                string tempPath = filePath + ".tmp";

                // Atomic write
                File.WriteAllText(tempPath, json);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempPath, filePath);

                Debug.Log($"[SaveSystem] Successfully saved slot {slot} to {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to save slot {slot}: {ex.Message}");
                return false;
            }
        }

        public static SaveData Load(int slot)
        {
            string filePath = GetSavePath(slot);
            if (!File.Exists(filePath)) return null;

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to load slot {slot}: {ex.Message}");
                return null;
            }
        }
    }
}
