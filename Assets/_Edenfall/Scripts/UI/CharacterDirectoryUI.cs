using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Core;
using Edenfall.NPC.Relationships;
using Edenfall.NPC.Scheduling;
using Edenfall.World;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.UI
{
    public class CharacterDirectoryUI : MonoBehaviour
    {
        [Header("Root Panel")]
        [SerializeField] private GameObject directoryModal;
        [SerializeField] private Transform directoryCardsContainer;
        [SerializeField] private Button closeButton;

        [Header("Characters Database")]
        [SerializeField] private List<CharacterDefinition> characters = new();

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseDirectory);
        }

        public void ToggleDirectory()
        {
            if (directoryModal == null) return;
            bool active = !directoryModal.activeSelf;
            directoryModal.SetActive(active);
            if (active) RefreshDirectory();
        }

        public void CloseDirectory()
        {
            if (directoryModal != null) directoryModal.SetActive(false);
        }

        public void RefreshDirectory()
        {
            if (directoryCardsContainer == null) return;

            foreach (Transform child in directoryCardsContainer)
            {
                Destroy(child.gameObject);
            }

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
            var currentTime = GameManager.Instance != null ? GameManager.Instance.CurrentTime : TimeOfDay.Morning;

            foreach (var ch in characters)
            {
                if (ch == null) continue;
                var card = CreateDirectoryCard(ch, currentTime, isVi);
                card.transform.SetParent(directoryCardsContainer, false);
            }
        }

        private GameObject CreateDirectoryCard(CharacterDefinition def, TimeOfDay time, bool isVi)
        {
            var cardObj = new GameObject($"Card_{def.characterId}");
            var img = cardObj.AddComponent<Image>();
            img.color = new Color(0.08f, 0.12f, 0.22f, 0.94f);

            var cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(580, 140);

            // Thumbnail
            var thumbObj = new GameObject("Thumbnail");
            thumbObj.transform.SetParent(cardObj.transform, false);
            var thumbImg = thumbObj.AddComponent<Image>();
            var portrait = def.GetPortrait(CharacterEmotion.Gentle);
            if (portrait != null) thumbImg.sprite = portrait;
            var thRt = thumbObj.GetComponent<RectTransform>();
            thRt.anchorMin = new Vector2(0.02f, 0.10f);
            thRt.anchorMax = new Vector2(0.24f, 0.90f);
            thRt.offsetMin = Vector2.zero;
            thRt.offsetMax = Vector2.zero;

            // Details Container
            var nameObj = new GameObject("NameAndTitle");
            nameObj.transform.SetParent(cardObj.transform, false);
            var nameTxt = nameObj.AddComponent<Text>();
            nameTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameTxt.fontSize = 20;
            nameTxt.fontStyle = FontStyle.Bold;
            nameTxt.color = new Color(1.0f, 0.88f, 0.45f);
            nameTxt.text = $"{def.characterName} — <size=15><color=#A8C0E0>{def.title}</color></size>";
            var nRt = nameObj.GetComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0.27f, 0.65f);
            nRt.anchorMax = new Vector2(0.96f, 0.92f);
            nRt.offsetMin = Vector2.zero;
            nRt.offsetMax = Vector2.zero;

            // Location & Schedule Status
            string targetLocId = NPCScheduleController.Instance != null
                ? NPCScheduleController.Instance.GetAssignedLocationForTime(def.characterId, time)
                : "loc_dawn_garden";

            var locDef = LocationManager.Instance != null ? LocationManager.Instance.GetLocationById(targetLocId) : null;
            string locName = locDef != null ? locDef.GetName(isVi) : targetLocId;

            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(cardObj.transform, false);
            var statusTxt = statusObj.AddComponent<Text>();
            statusTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusTxt.fontSize = 15;
            statusTxt.color = new Color(0.85f, 0.90f, 1.0f);
            statusTxt.text = isVi ? $"📍 Vị trí hiện tại: <b><color=#7DF9FF>{locName}</color></b>"
                                 : $"📍 Current Location: <b><color=#7DF9FF>{locName}</color></b>";
            var sRt = statusObj.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.27f, 0.38f);
            sRt.anchorMax = new Vector2(0.68f, 0.62f);
            sRt.offsetMin = Vector2.zero;
            sRt.offsetMax = Vector2.zero;

            // Direct Navigation Button
            var navBtnObj = new GameObject("DirectNavButton");
            navBtnObj.transform.SetParent(cardObj.transform, false);
            var navImg = navBtnObj.AddComponent<Image>();
            navImg.color = new Color(0.18f, 0.35f, 0.65f, 0.95f);
            var navBtn = navBtnObj.AddComponent<Button>();
            navBtn.onClick.AddListener(() =>
            {
                if (LocationManager.Instance != null)
                {
                    LocationManager.Instance.TravelTo(targetLocId);
                    CloseDirectory();
                }
            });

            var btnRt = navBtnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.70f, 0.15f);
            btnRt.anchorMax = new Vector2(0.96f, 0.55f);
            btnRt.offsetMin = Vector2.zero;
            btnRt.offsetMax = Vector2.zero;

            var btnTxtObj = new GameObject("BtnText");
            btnTxtObj.transform.SetParent(navBtnObj.transform, false);
            var btnTxt = btnTxtObj.AddComponent<Text>();
            btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnTxt.fontSize = 16;
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.color = Color.white;
            btnTxt.text = isVi ? "Tìm Gặp >>" : "Visit >>";
            btnTxt.alignment = TextAnchor.MiddleCenter;
            var btRt = btnTxtObj.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.offsetMin = Vector2.zero;
            btRt.offsetMax = Vector2.zero;

            return cardObj;
        }
    }
}
