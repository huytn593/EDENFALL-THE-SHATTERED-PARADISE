using Edenfall.Core;
using Edenfall.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Grid Container")]
        [SerializeField] private Transform itemGridContainer;
        [SerializeField] private GameObject itemSlotPrefab;

        [Header("Item Detail Panel")]
        [SerializeField] private Image detailIcon;
        [SerializeField] private Text detailNameText;
        [SerializeField] private Text detailCategoryText;
        [SerializeField] private Text detailDescriptionText;
        [SerializeField] private Text detailLoreText;
        [SerializeField] private Button useButton;
        [SerializeField] private Text useButtonText;

        private InventorySlot selectedSlot;

        private void OnEnable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryUpdated += RefreshGrid;
            }
            RefreshGrid();
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryUpdated -= RefreshGrid;
            }
        }

        public void RefreshGrid()
        {
            if (InventoryManager.Instance == null || itemGridContainer == null || itemSlotPrefab == null) return;

            foreach (Transform child in itemGridContainer) Destroy(child.gameObject);

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            foreach (var slot in InventoryManager.Instance.Slots)
            {
                var obj = Instantiate(itemSlotPrefab, itemGridContainer);
                var textComp = obj.GetComponentInChildren<Text>();
                if (textComp != null)
                {
                    textComp.text = $"{slot.item.GetName(isVi)} x{slot.quantity}";
                }

                var btn = obj.GetComponent<Button>();
                if (btn != null)
                {
                    var captured = slot;
                    btn.onClick.AddListener(() => SelectItem(captured));
                }
            }

            if (InventoryManager.Instance.Slots.Count > 0)
            {
                SelectItem(InventoryManager.Instance.Slots[0]);
            }
            else
            {
                ClearDetails();
            }
        }

        public void SelectItem(InventorySlot slot)
        {
            selectedSlot = slot;
            if (slot == null || slot.item == null)
            {
                ClearDetails();
                return;
            }

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            if (detailNameText != null) detailNameText.text = $"{slot.item.GetName(isVi)} (x{slot.quantity})";
            if (detailCategoryText != null) detailCategoryText.text = $"[{slot.item.category}] - {slot.item.rarity}";
            if (detailDescriptionText != null) detailDescriptionText.text = slot.item.GetDescription(isVi);
            if (detailLoreText != null) detailLoreText.text = isVi ? slot.item.loreVi : slot.item.loreEn;

            if (detailIcon != null)
            {
                detailIcon.gameObject.SetActive(slot.item.icon != null);
                if (slot.item.icon != null) detailIcon.sprite = slot.item.icon;
            }

            if (useButton != null)
            {
                useButton.gameObject.SetActive(slot.item.isUsable);
                if (useButtonText != null) useButtonText.text = isVi ? "Sử Dụng" : "Use";
            }
        }

        public void OnClickUse()
        {
            if (selectedSlot != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.UseItem(selectedSlot.item.itemId);
            }
        }

        private void ClearDetails()
        {
            if (detailNameText != null) detailNameText.text = "";
            if (detailCategoryText != null) detailCategoryText.text = "";
            if (detailDescriptionText != null) detailDescriptionText.text = "";
            if (detailLoreText != null) detailLoreText.text = "";
            if (detailIcon != null) detailIcon.gameObject.SetActive(false);
            if (useButton != null) useButton.gameObject.SetActive(false);
        }
    }
}
