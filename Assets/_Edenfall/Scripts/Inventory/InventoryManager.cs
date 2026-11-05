using System;
using System.Collections.Generic;
using Edenfall.Core;
using UnityEngine;

namespace Edenfall.Inventory
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemDefinition item;
        public int quantity;

        public InventorySlot(ItemDefinition it, int qty)
        {
            item = it;
            quantity = qty;
        }
    }

    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Starting Items")]
        [SerializeField] private List<InventorySlot> slots = new();
        public IReadOnlyList<InventorySlot> Slots => slots;

        public event Action OnInventoryUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddItem(ItemDefinition item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return;

            var existing = slots.Find(s => s.item.itemId == item.itemId);
            if (existing != null)
            {
                existing.quantity += quantity;
            }
            else
            {
                slots.Add(new InventorySlot(item, quantity));
            }

            GameEvents.TriggerItemAdded(item.itemId, quantity);
            OnInventoryUpdated?.Invoke();
        }

        public bool RemoveItem(string itemId, int quantity = 1)
        {
            var existing = slots.Find(s => s.item.itemId == itemId);
            if (existing == null || existing.quantity < quantity)
            {
                return false;
            }

            existing.quantity -= quantity;
            if (existing.quantity <= 0)
            {
                slots.Remove(existing);
            }

            GameEvents.TriggerItemRemoved(itemId, quantity);
            OnInventoryUpdated?.Invoke();
            return true;
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            var existing = slots.Find(s => s.item.itemId == itemId);
            return existing != null && existing.quantity >= quantity;
        }

        public int GetQuantity(string itemId)
        {
            var existing = slots.Find(s => s.item.itemId == itemId);
            return existing != null ? existing.quantity : 0;
        }

        public bool UseItem(string itemId)
        {
            var slot = slots.Find(s => s.item.itemId == itemId);
            if (slot == null || !slot.item.isUsable) return false;

            if (slot.item.sanctuaryRestorationBonus > 0 && GameManager.Instance != null)
            {
                GameManager.Instance.AddSanctuaryRestoration(slot.item.sanctuaryRestorationBonus);
            }

            RemoveItem(itemId, 1);
            return true;
        }
    }
}
