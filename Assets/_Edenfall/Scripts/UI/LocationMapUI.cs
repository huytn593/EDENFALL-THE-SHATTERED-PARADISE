using System.Collections.Generic;
using Edenfall.Core;
using Edenfall.NPC.Scheduling;
using Edenfall.World;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.UI
{
    public class LocationMapUI : MonoBehaviour
    {
        [Header("Root Panel")]
        [SerializeField] private GameObject mapModal;
        [SerializeField] private Transform locationCardsContainer;
        [SerializeField] private Button closeButton;

        [Header("NPC Schedule Source")]
        [SerializeField] private NPCSchedule aureliaSchedule;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseMap);
        }

        private void OnEnable()
        {
            if (LocationManager.Instance != null)
            {
                LocationManager.Instance.OnLocationChanged += HandleLocationChanged;
            }
        }

        private void OnDisable()
        {
            if (LocationManager.Instance != null)
            {
                LocationManager.Instance.OnLocationChanged -= HandleLocationChanged;
            }
        }

        public void ToggleMap()
        {
            if (mapModal == null) return;
            bool active = !mapModal.activeSelf;
            mapModal.SetActive(active);
            if (active) RefreshLocationCards();
        }

        public void CloseMap()
        {
            if (mapModal != null) mapModal.SetActive(false);
        }

        private void HandleLocationChanged(LocationDefinition loc)
        {
            if (mapModal != null && mapModal.activeSelf)
            {
                RefreshLocationCards();
            }
        }

        public void RefreshLocationCards()
        {
            if (locationCardsContainer == null || LocationManager.Instance == null) return;

            foreach (Transform child in locationCardsContainer)
            {
                Destroy(child.gameObject);
            }

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
            var currentTime = GameManager.Instance != null ? GameManager.Instance.CurrentTime : TimeOfDay.Morning;

            // Check where Aurelia is right now
            string aureliaCurrentLoc = "";
            if (aureliaSchedule != null)
            {
                var entry = aureliaSchedule.GetEntryForTime(currentTime);
                if (entry != null) aureliaCurrentLoc = entry.locationName;
            }

            foreach (var loc in LocationManager.Instance.AllLocations)
            {
                if (loc == null) continue;

                bool isUnlocked = LocationManager.Instance.IsLocationUnlocked(loc.locationId);
                bool isCurrent = LocationManager.Instance.CurrentLocation != null && LocationManager.Instance.CurrentLocation.locationId == loc.locationId;
                bool hasAurelia = !string.IsNullOrEmpty(aureliaCurrentLoc) &&
                    (loc.locationId.Contains(aureliaCurrentLoc.ToLower()) || loc.nameEn.ToLower().Contains(aureliaCurrentLoc.ToLower()));

                var card = CreateLocationCard(loc, isUnlocked, isCurrent, hasAurelia, isVi);
                card.transform.SetParent(locationCardsContainer, false);
            }
        }

        private GameObject CreateLocationCard(LocationDefinition loc, bool isUnlocked, bool isCurrent, bool hasAurelia, bool isVi)
        {
            var cardObj = new GameObject($"Card_{loc.locationId}");
            var img = cardObj.AddComponent<Image>();
            img.color = isCurrent
                ? new Color(0.18f, 0.28f, 0.45f, 0.95f)
                : (isUnlocked ? new Color(0.10f, 0.14f, 0.24f, 0.90f) : new Color(0.06f, 0.08f, 0.12f, 0.70f));

            var btn = cardObj.AddComponent<Button>();
            btn.interactable = isUnlocked && !isCurrent;
            btn.onClick.AddListener(() =>
            {
                LocationManager.Instance.TravelTo(loc.locationId);
                CloseMap();
            });

            var cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(480, 110);

            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(cardObj.transform, false);
            var titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = 20;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = isCurrent ? new Color(1.0f, 0.88f, 0.45f) : (isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.55f));
            titleTxt.text = loc.GetName(isVi) + (isCurrent ? (isVi ? " (Hiện Tại)" : " (Current)") : "");

            var tRt = titleObj.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.04f, 0.55f);
            tRt.anchorMax = new Vector2(0.96f, 0.92f);
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;

            // Description / Status
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(cardObj.transform, false);
            var descTxt = descObj.AddComponent<Text>();
            descTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descTxt.fontSize = 15;
            descTxt.color = hasAurelia ? new Color(1.0f, 0.75f, 0.85f) : new Color(0.75f, 0.80f, 0.90f);

            if (!isUnlocked)
            {
                descTxt.text = isVi ? "🔒 Chưa mở khóa (Tăng điểm tình cảm để mở)" : "🔒 Locked (Increase Affection to unlock)";
                descTxt.color = new Color(0.6f, 0.35f, 0.35f);
            }
            else if (hasAurelia)
            {
                descTxt.text = isVi ? "✨ Aurelia đang ở đây!" : "✨ Aurelia is here right now!";
            }
            else
            {
                descTxt.text = loc.GetDescription(isVi);
            }

            var dRt = descObj.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.04f, 0.08f);
            dRt.anchorMax = new Vector2(0.96f, 0.52f);
            dRt.offsetMin = Vector2.zero;
            dRt.offsetMax = Vector2.zero;

            return cardObj;
        }
    }
}
