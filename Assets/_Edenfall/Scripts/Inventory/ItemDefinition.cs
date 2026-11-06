using UnityEngine;

namespace Edenfall.Inventory
{
    public enum ItemCategory
    {
        Relic,
        Material,
        Consumable,
        KeyItem,
        Gift
    }

    public enum ItemRarity
    {
        Common,
        Rare,
        Celestial,
        Ancient
    }

    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Edenfall/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        public string itemId;
        public string nameVi;
        public string nameEn;
        public ItemCategory category;
        public ItemRarity rarity;

        [TextArea(2, 4)] public string descriptionVi;
        [TextArea(2, 4)] public string descriptionEn;
        [TextArea(2, 4)] public string loreVi;
        [TextArea(2, 4)] public string loreEn;

        public Sprite icon;
        public bool isUsable;
        public string targetGiftCharacterId;

        [Header("Use Effects")]
        public float staminaRestore = 0f;
        public float sanctuaryRestorationBonus = 0f;

        public string GetName(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(nameVi) ? nameEn : nameVi)
                              : (string.IsNullOrEmpty(nameEn) ? nameVi : nameEn);
        }

        public string GetDescription(bool vietnamese = true)
        {
            return vietnamese ? (string.IsNullOrEmpty(descriptionVi) ? descriptionEn : descriptionVi)
                              : (string.IsNullOrEmpty(descriptionEn) ? descriptionVi : descriptionEn);
        }
    }
}
