using System;
using UnityEngine;

namespace Edenfall.Core
{
    public static class GameEvents
    {
        // World & Time
        public static event Action<TimeOfDay> OnTimeOfDayChanged;
        public static event Action<WeatherType> OnWeatherChanged;
        public static event Action<int> OnDayChanged;
        public static event Action<float> OnSanctuaryRestorationChanged;
        public static event Action<VisionLayer> OnVisionLayerChanged;

        // Dialogue
        public static event Action<string, string> OnDialogueStarted; // characterId, startNodeId
        public static event Action<string> OnDialogueEnded; // characterId
        public static event Action<string> OnDialogueNodeEntered; // nodeId

        // Relationship
        public static event Action<string, string, int> OnRelationshipStatChanged; // characterId, statName, delta
        public static event Action<string, string> OnRelationshipStageAdvanced; // characterId, newStage

        // Cinematics
        public static event Action<string> OnCinematicRequested; // sceneId
        public static event Action<string> OnCinematicCompleted; // sceneId

        // Quests & Inventory
        public static event Action<string> OnQuestStarted; // questId
        public static event Action<string, int> OnQuestStepUpdated; // questId, stepIndex
        public static event Action<string> OnQuestCompleted; // questId
        public static event Action<string, int> OnItemAdded; // itemId, qty
        public static event Action<string, int> OnItemRemoved; // itemId, qty

        // Invokers
        public static void TriggerTimeOfDayChanged(TimeOfDay time) => OnTimeOfDayChanged?.Invoke(time);
        public static void TriggerWeatherChanged(WeatherType weather) => OnWeatherChanged?.Invoke(weather);
        public static void TriggerDayChanged(int day) => OnDayChanged?.Invoke(day);
        public static void TriggerSanctuaryRestorationChanged(float percent) => OnSanctuaryRestorationChanged?.Invoke(percent);
        public static void TriggerVisionLayerChanged(VisionLayer layer) => OnVisionLayerChanged?.Invoke(layer);

        public static void TriggerDialogueStarted(string characterId, string nodeId) => OnDialogueStarted?.Invoke(characterId, nodeId);
        public static void TriggerDialogueEnded(string characterId) => OnDialogueEnded?.Invoke(characterId);
        public static void TriggerDialogueNodeEntered(string nodeId) => OnDialogueNodeEntered?.Invoke(nodeId);

        public static void TriggerRelationshipStatChanged(string characterId, string statName, int delta) =>
            OnRelationshipStatChanged?.Invoke(characterId, statName, delta);
        public static void TriggerRelationshipStageAdvanced(string characterId, string newStage) =>
            OnRelationshipStageAdvanced?.Invoke(characterId, newStage);

        public static void TriggerCinematicRequested(string sceneId) => OnCinematicRequested?.Invoke(sceneId);
        public static void TriggerCinematicCompleted(string sceneId) => OnCinematicCompleted?.Invoke(sceneId);

        public static void TriggerQuestStarted(string questId) => OnQuestStarted?.Invoke(questId);
        public static void TriggerQuestStepUpdated(string questId, int stepIndex) => OnQuestStepUpdated?.Invoke(questId, stepIndex);
        public static void TriggerQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
        public static void TriggerItemAdded(string itemId, int qty) => OnItemAdded?.Invoke(itemId, qty);
        public static void TriggerItemRemoved(string itemId, int qty) => OnItemRemoved?.Invoke(itemId, qty);
    }
}
