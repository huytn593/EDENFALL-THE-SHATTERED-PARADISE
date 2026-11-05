using Edenfall.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Edenfall.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("World Status Header")]
        [SerializeField] private Text timeText;
        [SerializeField] private Text weatherText;
        [SerializeField] private Text dayText;
        [SerializeField] private Text visionLayerText;
        [SerializeField] private Slider restorationSlider;
        [SerializeField] private Text restorationPercentText;

        [Header("Modals")]
        [SerializeField] private GameObject locationMapModal;
        [SerializeField] private GameObject relationshipModal;
        [SerializeField] private GameObject questModal;
        [SerializeField] private GameObject inventoryModal;
        [SerializeField] private GameObject galleryModal;
        [SerializeField] private GameObject saveLoadModal;

        [Header("Language Toggle Button")]
        [SerializeField] private Text languageButtonText;

        private void OnEnable()
        {
            GameEvents.OnTimeOfDayChanged += HandleTimeChanged;
            GameEvents.OnWeatherChanged += HandleWeatherChanged;
            GameEvents.OnDayChanged += HandleDayChanged;
            GameEvents.OnSanctuaryRestorationChanged += HandleRestorationChanged;
            GameEvents.OnVisionLayerChanged += HandleVisionLayerChanged;

            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += RefreshAllText;
            }
        }

        private void OnDisable()
        {
            GameEvents.OnTimeOfDayChanged -= HandleTimeChanged;
            GameEvents.OnWeatherChanged -= HandleWeatherChanged;
            GameEvents.OnDayChanged -= HandleDayChanged;
            GameEvents.OnSanctuaryRestorationChanged -= HandleRestorationChanged;
            GameEvents.OnVisionLayerChanged -= HandleVisionLayerChanged;

            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= RefreshAllText;
            }
        }

        private void Start()
        {
            RefreshAllText();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.mKey.wasPressedThisFrame) ToggleLocationMap();
                if (kb.jKey.wasPressedThisFrame) ToggleQuestModal();
                if (kb.rKey.wasPressedThisFrame) ToggleRelationshipModal();
                if (kb.iKey.wasPressedThisFrame) ToggleInventoryModal();
                if (kb.gKey.wasPressedThisFrame) ToggleGalleryModal();
                if (kb.escapeKey.wasPressedThisFrame) ToggleSaveLoadModal();
            }
        }

        public void RefreshAllText()
        {
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            if (GameManager.Instance != null)
            {
                HandleTimeChanged(GameManager.Instance.CurrentTime);
                HandleWeatherChanged(GameManager.Instance.CurrentWeather);
                HandleDayChanged(GameManager.Instance.CurrentDay);
                HandleRestorationChanged(GameManager.Instance.SanctuaryRestoration);
                HandleVisionLayerChanged(GameManager.Instance.ActiveVisionLayer);
            }

            if (languageButtonText != null)
            {
                languageButtonText.text = isVi ? "VI / EN" : "EN / VI";
            }
        }

        public void ToggleLanguage()
        {
            if (LocalizationManager.Instance != null)
            {
                var nextLang = LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese
                    ? LocalizationManager.Language.English
                    : LocalizationManager.Language.Vietnamese;

                LocalizationManager.Instance.SetLanguage(nextLang);
            }
        }

        private void HandleTimeChanged(TimeOfDay time)
        {
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
            if (timeText != null) timeText.text = time.ToDisplayString(isVi);
        }

        private void HandleWeatherChanged(WeatherType weather)
        {
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
            if (weatherText != null) weatherText.text = weather.ToDisplayString(isVi);
        }

        private void HandleDayChanged(int day)
        {
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
            if (dayText != null) dayText.text = isVi ? $"Ngày {day}" : $"Day {day}";
        }

        private void HandleRestorationChanged(float percent)
        {
            if (restorationSlider != null) restorationSlider.value = percent / 100f;
            if (restorationPercentText != null) restorationPercentText.text = $"{percent:F0}%";
        }

        private void HandleVisionLayerChanged(VisionLayer layer)
        {
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
            if (visionLayerText != null) visionLayerText.text = layer.ToDisplayString(isVi);
        }

        public void ToggleLocationMap() => ToggleModal(locationMapModal);
        public void ToggleRelationshipModal() => ToggleModal(relationshipModal);
        public void ToggleQuestModal() => ToggleModal(questModal);
        public void ToggleInventoryModal() => ToggleModal(inventoryModal);
        public void ToggleGalleryModal() => ToggleModal(galleryModal);
        public void ToggleSaveLoadModal() => ToggleModal(saveLoadModal);

        private void ToggleModal(GameObject modal)
        {
            if (modal == null) return;
            bool active = !modal.activeSelf;
            modal.SetActive(active);

            if (GameManager.Instance != null && GameManager.Instance.CurrentMode != GameMode.Dialogue && GameManager.Instance.CurrentMode != GameMode.Cinematic)
            {
                GameManager.Instance.SetGameMode(active ? GameMode.MenuPaused : GameMode.Exploration);
            }
        }
    }
}
