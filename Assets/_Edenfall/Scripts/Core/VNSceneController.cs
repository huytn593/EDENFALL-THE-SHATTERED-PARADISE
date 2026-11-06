using System.Collections;
using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Cinematics;
using Edenfall.Core;
using Edenfall.Dialogue;
using Edenfall.Events;
using Edenfall.Gameplay;
using Edenfall.NPC.Behavior;
using Edenfall.NPC.Relationships;
using Edenfall.NPC.Scheduling;
using Edenfall.UI;
using Edenfall.World;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.Core
{
    public class VNSceneController : MonoBehaviour
    {
        public static VNSceneController Instance { get; private set; }

        [Header("Camera & Viewport (First-Person POV)")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private CameraController2D cameraController;
        [SerializeField] private float standardOrthoSize = 4.2f;
        [SerializeField] private float intimateZoomOrthoSize = 3.2f;
        [SerializeField] private float wideSceneOrthoSize = 5.2f;

        [Header("Environment & Background Staging")]
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private SpriteRenderer distantSpireRenderer;
        [SerializeField] private Transform environmentVFXRoot;

        [Header("Heroine Staging (Aurelia)")]
        [SerializeField] private GameObject aureliaRoot;
        [SerializeField] private CharacterVisualRig aureliaRig;
        [SerializeField] private AureliaLiveRig aureliaLiveRig;
        [SerializeField] private AureliaRigController aureliaRigController;
        [SerializeField] private AureliaRiggedCharacter aureliaRiggedCharacter;
        [SerializeField] private NPCSchedule aureliaSchedule;
        [SerializeField] private DialogueTree aureliaDialogueTree;
        [SerializeField] private CharacterDefinition aureliaDef;

        [Header("Staging Positions")]
        [SerializeField] private Vector3 firstPersonCenterPos = new Vector3(0.5f, -1.8f, 0f);
        [SerializeField] private Vector3 firstPersonScale = new Vector3(0.85f, 0.85f, 1f);

        [Header("Hotspot UI Canvas")]
        [SerializeField] private GameObject hotspotCanvas;
        [SerializeField] private Transform hotspotButtonsContainer;

        [Header("Relationship Model")]
        private RelationshipModel aureliaRelationship;

        public RelationshipModel AureliaRelationship => aureliaRelationship;

        private Coroutine cameraZoomRoutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Initialize 6-dimension relationship model for Aurelia
            aureliaRelationship = new RelationshipModel("aurelia");
            if (aureliaDef != null)
            {
                aureliaRelationship.affection = aureliaDef.startingAffection;
                aureliaRelationship.trust = aureliaDef.startingTrust;
                aureliaRelationship.familiarity = aureliaDef.startingFamiliarity;
                aureliaRelationship.respect = aureliaDef.startingRespect;
                aureliaRelationship.comfort = aureliaDef.startingComfort;
                aureliaRelationship.conflict = aureliaDef.startingConflict;
                aureliaRelationship.EvaluateStage();
            }
        }

        private void Start()
        {
            if (LocationManager.Instance != null)
            {
                LocationManager.Instance.OnLocationChanged += HandleLocationChanged;
                if (LocationManager.Instance.CurrentLocation != null)
                {
                    HandleLocationChanged(LocationManager.Instance.CurrentLocation);
                }
            }

            GameEvents.OnTimeOfDayChanged += HandleTimeOfDayChanged;

            // Stage Aurelia using mathematical NPCStageAnchor
            if (aureliaLiveRig != null)
            {
                aureliaLiveRig.SetStagingMode(StagingMode.DialoguePortrait, mainCamera);
            }
            else if (aureliaRig != null)
            {
                aureliaRig.SetStagingMode(StagingMode.DialoguePortrait, mainCamera);
            }
            else if (aureliaRoot != null)
            {
                var anchor = aureliaRoot.GetComponent<NPCStageAnchor>();
                var sr = aureliaRoot.GetComponent<SpriteRenderer>();
                if (anchor != null && sr != null)
                {
                    anchor.AlignCharacter(aureliaRoot.transform, sr, StagingMode.DialoguePortrait, mainCamera);
                }
            }
        }

        private void OnDestroy()
        {
            if (LocationManager.Instance != null)
            {
                LocationManager.Instance.OnLocationChanged -= HandleLocationChanged;
            }
            GameEvents.OnTimeOfDayChanged -= HandleTimeOfDayChanged;
        }

        private void HandleLocationChanged(LocationDefinition location)
        {
            if (location == null) return;

            // 1. Update background
            if (backgroundRenderer != null && location.backgroundSprite != null)
            {
                backgroundRenderer.sprite = location.backgroundSprite;
            }

            // 2. Update distant spire layer if present
            if (distantSpireRenderer != null)
            {
                distantSpireRenderer.gameObject.SetActive(location.distantSpireSprite != null);
                if (location.distantSpireSprite != null)
                {
                    distantSpireRenderer.sprite = location.distantSpireSprite;
                }
            }

            // 3. Update NPC presence based on schedule
            UpdateNPCPresence(location);

            // 4. Refresh interactive hotspots
            RefreshHotspots(location);

            // 5. Check unlocks and mature events
            if (LocationManager.Instance != null)
            {
                LocationManager.Instance.CheckUnlocksForRelationship(aureliaRelationship);
            }

            CheckForLocationEvents(location);
        }

        private void HandleTimeOfDayChanged(TimeOfDay time)
        {
            if (LocationManager.Instance != null && LocationManager.Instance.CurrentLocation != null)
            {
                UpdateNPCPresence(LocationManager.Instance.CurrentLocation);
                RefreshHotspots(LocationManager.Instance.CurrentLocation);
                CheckForLocationEvents(LocationManager.Instance.CurrentLocation);
            }
        }

        private void UpdateNPCPresence(LocationDefinition location)
        {
            if (aureliaRoot == null) return;

            var currentTime = GameManager.Instance != null ? GameManager.Instance.CurrentTime : TimeOfDay.Morning;
            bool isPresent = false;

            if (aureliaSchedule != null)
            {
                var entry = aureliaSchedule.GetEntryForTime(currentTime);
                if (entry != null)
                {
                    isPresent = location.locationId.Contains(entry.locationName.ToLower()) ||
                                location.nameEn.ToLower().Contains(entry.locationName.ToLower()) ||
                                location.locationId == "loc_dawn_garden"; // Always present in garden default
                }
            }
            else
            {
                isPresent = true;
            }

            aureliaRoot.SetActive(isPresent);

            if (isPresent)
            {
                CharacterEmotion targetEmotion = aureliaRelationship != null && aureliaRelationship.affection >= 50
                    ? CharacterEmotion.Smiling
                    : CharacterEmotion.Gentle;

                if (aureliaRig != null)
                {
                    aureliaRig.SetFacing(true);
                    aureliaRig.SetExpression(targetEmotion);
                }
                if (aureliaRiggedCharacter != null)
                {
                    aureliaRiggedCharacter.SetAnimationState(targetEmotion == CharacterEmotion.Smiling ? AureliaAnimationState.Idle_Happy : AureliaAnimationState.Idle_Normal);
                }
            }
        }

        public void StartConversationWithAurelia()
        {
            if (DialogueManager.Instance != null && aureliaDef != null && aureliaDialogueTree != null)
            {
                if (aureliaLiveRig != null)
                {
                    aureliaLiveRig.SetStagingMode(StagingMode.DialoguePortrait, mainCamera);
                    aureliaLiveRig.SetAnimationState(AureliaAnimationState.Talk_Normal);
                    aureliaLiveRig.SetTalking(true);
                }
                if (aureliaRiggedCharacter != null)
                {
                    aureliaRiggedCharacter.SetAnimationState(AureliaAnimationState.Talk_Normal);
                    aureliaRiggedCharacter.SetTalking(true);
                }
                if (aureliaRig != null)
                {
                    aureliaRig.SetStagingMode(StagingMode.DialoguePortrait, mainCamera);
                }
                SetCameraFocus(true); // Zoom in on dialogue
                DialogueManager.Instance.StartDialogue(aureliaDef, aureliaDialogueTree, aureliaRelationship);

                // Listen to finished event to restore camera zoom
                DialogueManager.Instance.OnDialogueFinished += HandleDialogueFinished;
            }
        }

        private void HandleDialogueFinished()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueFinished -= HandleDialogueFinished;
            }
            if (aureliaLiveRig != null)
            {
                aureliaLiveRig.SetTalking(false);
                aureliaLiveRig.SetAnimationState(AureliaAnimationState.Idle_Normal);
            }
            if (aureliaRiggedCharacter != null)
            {
                aureliaRiggedCharacter.SetTalking(false);
                aureliaRiggedCharacter.SetAnimationState(AureliaAnimationState.Idle_Normal);
            }
            SetCameraFocus(false);
        }

        public void SetCameraFocus(bool isIntimate)
        {
            float targetSize = isIntimate ? intimateZoomOrthoSize : standardOrthoSize;
            if (cameraZoomRoutine != null) StopCoroutine(cameraZoomRoutine);
            cameraZoomRoutine = StartCoroutine(DoCameraZoom(targetSize, 0.7f));
        }

        private IEnumerator DoCameraZoom(float targetSize, float duration)
        {
            if (mainCamera == null) yield break;

            float startSize = mainCamera.orthographicSize;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
                yield return null;
            }
            mainCamera.orthographicSize = targetSize;
        }

        private void RefreshHotspots(LocationDefinition location)
        {
            if (hotspotButtonsContainer == null) return;

            foreach (Transform child in hotspotButtonsContainer)
            {
                Destroy(child.gameObject);
            }

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            // 1. If Aurelia is present, add primary "Talk" hotspot
            if (aureliaRoot != null && aureliaRoot.activeSelf)
            {
                string talkLabel = isVi ? "💬 Trò Chuyện Cùng Aurelia" : "💬 Talk with Aurelia";
                var talkBtn = CreateHotspotButton(talkLabel, StartConversationWithAurelia, new Color(0.95f, 0.75f, 0.35f));
                talkBtn.transform.SetParent(hotspotButtonsContainer, false);
            }

            // 2. Location-specific hotspots
            foreach (var spot in location.hotspots)
            {
                string title = spot.GetTitle(isVi);
                var btn = CreateHotspotButton(title, () => HandleHotspotAction(spot), new Color(0.35f, 0.65f, 0.95f));
                btn.transform.SetParent(hotspotButtonsContainer, false);
            }

            // 3. Rest / Advance Time hotspot
            string restLabel = isVi ? "⏳ Nghỉ Ngơi / Đợi Thời Khắc Tiếp Theo" : "⏳ Rest / Advance Time";
            var restBtn = CreateHotspotButton(restLabel, AdvanceTimeToNext, new Color(0.55f, 0.45f, 0.85f));
            restBtn.transform.SetParent(hotspotButtonsContainer, false);
        }

        private void HandleHotspotAction(LocationHotspot spot)
        {
            switch (spot.actionType)
            {
                case "talk":
                    StartConversationWithAurelia();
                    break;
                case "inspect":
                    Debug.Log($"[VNSceneController] Inspected: {spot.hotspotId}");
                    break;
                case "event":
                    if (MatureEventManager.Instance != null)
                    {
                        var evt = MatureEventManager.Instance.GetAvailableEvent(LocationManager.Instance.CurrentLocation.locationId, GameManager.Instance.CurrentTime, aureliaRelationship);
                        if (evt != null) MatureEventManager.Instance.PlayMatureEvent(evt);
                    }
                    break;
            }
        }

        public void AdvanceTimeToNext()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AdvanceTime();
            }
        }

        private void CheckForLocationEvents(LocationDefinition location)
        {
            if (MatureEventManager.Instance == null || aureliaRelationship == null) return;

            var currentTime = GameManager.Instance != null ? GameManager.Instance.CurrentTime : TimeOfDay.Morning;
            var availableEvent = MatureEventManager.Instance.GetAvailableEvent(location.locationId, currentTime, aureliaRelationship);
            if (availableEvent != null)
            {
                Debug.Log($"[VNSceneController] Triggering mature romantic event: {availableEvent.titleEn}");
                MatureEventManager.Instance.PlayMatureEvent(availableEvent);
            }
        }

        private GameObject CreateHotspotButton(string label, UnityEngine.Events.UnityAction onClick, Color accentColor)
        {
            var btnObj = new GameObject("HotspotButton");
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.08f, 0.12f, 0.22f, 0.88f);

            var btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            var txt = txtObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 18;
            txt.color = accentColor;
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter;

            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(12, 6);
            txtRt.offsetMax = new Vector2(-12, -6);

            var btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(360, 44);

            return btnObj;
        }
    }
}
