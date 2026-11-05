using System;
using System.Collections.Generic;
using Edenfall.Core;
using Edenfall.Dialogue;
using Edenfall.Events;
using Edenfall.World;
using UnityEngine;

namespace Edenfall.NPC.Scheduling
{
    public class NPCScheduleController : MonoBehaviour
    {
        public static NPCScheduleController Instance { get; private set; }

        [Header("NPC Schedule Data")]
        [SerializeField] private NPCSchedule aureliaSchedule;

        [Header("Locking Flags")]
        [SerializeField] private bool isDialogueLocked = false;
        [SerializeField] private bool isCinematicLocked = false;
        [SerializeField] private bool isEventLocked = false;

        [Header("Pending Relocation Queue")]
        private bool hasPendingRelocation = false;
        private TimeOfDay pendingTimeSlot;

        public bool IsLocked => isDialogueLocked || isCinematicLocked || isEventLocked;

        public event Action<string, string> OnNPCLocationChanged; // npcId, newLocationId

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            GameEvents.OnTimeOfDayChanged += HandleTimeOfDayChanged;

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed += HandleDialogueStarted;
                DialogueManager.Instance.OnDialogueFinished += HandleDialogueFinished;
            }

            if (MatureEventManager.Instance != null)
            {
                MatureEventManager.Instance.OnMatureEventStarted += HandleMatureEventStarted;
                MatureEventManager.Instance.OnMatureEventCompleted += HandleMatureEventCompleted;
            }
        }

        private void OnDisable()
        {
            GameEvents.OnTimeOfDayChanged -= HandleTimeOfDayChanged;

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed -= HandleDialogueStarted;
                DialogueManager.Instance.OnDialogueFinished -= HandleDialogueFinished;
            }

            if (MatureEventManager.Instance != null)
            {
                MatureEventManager.Instance.OnMatureEventStarted -= HandleMatureEventStarted;
                MatureEventManager.Instance.OnMatureEventCompleted -= HandleMatureEventCompleted;
            }
        }

        public void SetCinematicLock(bool locked)
        {
            isCinematicLocked = locked;
            if (!locked && hasPendingRelocation)
            {
                ProcessPendingRelocation();
            }
        }

        private void HandleDialogueStarted(Characters.CharacterDefinition speaker, DialogueNode node, Sprite portrait)
        {
            isDialogueLocked = true;
        }

        private void HandleDialogueFinished()
        {
            isDialogueLocked = false;
            if (hasPendingRelocation)
            {
                ProcessPendingRelocation();
            }
        }

        private void HandleMatureEventStarted(MatureEventDefinition evt)
        {
            isEventLocked = true;
        }

        private void HandleMatureEventCompleted(MatureEventDefinition evt)
        {
            isEventLocked = false;
            if (hasPendingRelocation)
            {
                ProcessPendingRelocation();
            }
        }

        private void HandleTimeOfDayChanged(TimeOfDay newTime)
        {
            if (IsLocked)
            {
                // Defer relocation until current interaction finishes safely
                hasPendingRelocation = true;
                pendingTimeSlot = newTime;
                Debug.Log($"[NPCScheduleController] Schedule transition to {newTime} deferred (interaction locked).");
                return;
            }

            ApplyScheduleSlot(newTime);
        }

        private void ProcessPendingRelocation()
        {
            if (!IsLocked && hasPendingRelocation)
            {
                hasPendingRelocation = false;
                ApplyScheduleSlot(pendingTimeSlot);
                Debug.Log($"[NPCScheduleController] Deferred schedule transition applied for {pendingTimeSlot}.");
            }
        }

        public void ApplyScheduleSlot(TimeOfDay slot)
        {
            string aureliaTargetLocation = GetAssignedLocationForTime("aurelia", slot);
            OnNPCLocationChanged?.Invoke("aurelia", aureliaTargetLocation);
        }

        public string GetAssignedLocationForTime(string npcId, TimeOfDay slot)
        {
            if (npcId == "aurelia")
            {
                return slot switch
                {
                    TimeOfDay.Morning => "loc_dawn_garden",
                    TimeOfDay.Afternoon => "loc_celestial_altar",
                    TimeOfDay.Evening => "loc_solstice_balcony",
                    TimeOfDay.Night => "loc_aurelia_chamber",
                    _ => "loc_dawn_garden"
                };
            }

            return "loc_dawn_garden";
        }

        public bool IsNPCAvailable(string npcId, string currentLocationId, TimeOfDay currentTime)
        {
            string assignedLoc = GetAssignedLocationForTime(npcId, currentTime);
            return assignedLoc == currentLocationId;
        }

        public string GetAvailabilityStatusDescription(string npcId, TimeOfDay currentTime, bool isVi = true)
        {
            string assignedLoc = GetAssignedLocationForTime(npcId, currentTime);
            if (LocationManager.Instance != null)
            {
                var loc = LocationManager.Instance.GetLocationById(assignedLoc);
                string locName = loc != null ? loc.GetName(isVi) : assignedLoc;

                return isVi
                    ? $"Hiện đang có mặt tại: {locName}"
                    : $"Currently present at: {locName}";
            }

            return assignedLoc;
        }
    }
}
