using System;
using System.Collections.Generic;
using UnityEngine;

namespace Edenfall.Core
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        public enum Language
        {
            Vietnamese,
            English
        }

        [SerializeField] private Language currentLanguage = Language.Vietnamese;
        public Language CurrentLanguage => currentLanguage;

        public event Action OnLanguageChanged;

        private readonly Dictionary<string, string> viTable = new();
        private readonly Dictionary<string, string> enTable = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                PopulateDefaultTranslations();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetLanguage(Language newLanguage)
        {
            if (currentLanguage != newLanguage)
            {
                currentLanguage = newLanguage;
                OnLanguageChanged?.Invoke();
            }
        }

        public string Get(string key, string fallback = "")
        {
            var table = currentLanguage == Language.Vietnamese ? viTable : enTable;
            if (table.TryGetValue(key, out var val))
            {
                return val;
            }

            // Fallback to English if Vietnamese missing, or vice versa
            var backupTable = currentLanguage == Language.Vietnamese ? enTable : viTable;
            if (backupTable.TryGetValue(key, out var backupVal))
            {
                return backupVal;
            }

            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }

        private void PopulateDefaultTranslations()
        {
            // UI Strings - VI
            viTable["ui.game_title"] = "EDENFALL: THIÊN ĐƯỜNG VỠ VỤN";
            viTable["ui.start_game"] = "Bắt Đầu Hành Trình";
            viTable["ui.continue"] = "Tiếp Tục";
            viTable["ui.save_load"] = "Lưu & Tải Game";
            viTable["ui.settings"] = "Cài Đặt";
            viTable["ui.quest_journal"] = "Nhật Ký Nhiệm Vụ";
            viTable["ui.relationship_journal"] = "Hồ Sơ Quan Hệ";
            viTable["ui.inventory"] = "Túi Đồ";
            viTable["ui.gallery"] = "Phòng Trưng Bày CG";
            viTable["ui.interact_prompt"] = "[E] Tương Tác";
            viTable["ui.vision_layer_prompt"] = "[Tab] Đổi Thị Giác";
            viTable["ui.stamina"] = "Thể Lực";
            viTable["ui.resonance"] = "Cộng Hưởng Tinh Tú";
            viTable["ui.restoration"] = "Khôi Phục Thánh Địa";

            // UI Strings - EN
            enTable["ui.game_title"] = "EDENFALL: THE SHATTERED PARADISE";
            enTable["ui.start_game"] = "Begin Journey";
            enTable["ui.continue"] = "Continue";
            enTable["ui.save_load"] = "Save & Load";
            enTable["ui.settings"] = "Settings";
            enTable["ui.quest_journal"] = "Quest Journal";
            enTable["ui.relationship_journal"] = "Relationship Journal";
            enTable["ui.inventory"] = "Inventory";
            enTable["ui.gallery"] = "CG Gallery";
            enTable["ui.interact_prompt"] = "[E] Interact";
            enTable["ui.vision_layer_prompt"] = "[Tab] Switch Vision";
            enTable["ui.stamina"] = "Stamina";
            enTable["ui.resonance"] = "Starlight Resonance";
            enTable["ui.restoration"] = "Sanctuary Restoration";

            // Character Names - VI
            viTable["char.aurelia.name"] = "Aurelia";
            viTable["char.aurelia.title"] = "Hậu Duệ Nhật Thực";
            viTable["char.teresa.name"] = "Chỉ Huy Teresa";
            viTable["char.teresa.title"] = "Thiết Vệ Tiền Tuyến";
            viTable["char.vaelen.name"] = "Học Giả Vaelen";
            viTable["char.vaelen.title"] = "Người Gác Thư Viện Tinh Tú";

            // Character Names - EN
            enTable["char.aurelia.name"] = "Aurelia";
            enTable["char.aurelia.title"] = "The Solstice Heir";
            enTable["char.teresa.name"] = "Commander Teresa";
            enTable["char.teresa.title"] = "Iron Warden of the Vanguard";
            enTable["char.vaelen.name"] = "Archivist Vaelen";
            enTable["char.vaelen.title"] = "Curator of Celestial Vaults";
        }
    }
}
