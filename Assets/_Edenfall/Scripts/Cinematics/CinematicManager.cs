using System;
using System.Collections.Generic;
using Edenfall.Core;
using Edenfall.NPC.Relationships;
using UnityEngine;

namespace Edenfall.Cinematics
{
    public class CinematicManager : MonoBehaviour
    {
        public static CinematicManager Instance { get; private set; }

        [Header("Registered Scenes")]
        [SerializeField] private List<CinematicSceneData> allScenes = new();

        [Header("Runtime State")]
        private CinematicSceneData currentScene;
        private int currentSlideIndex = 0;

        public event Action<CinematicSceneData, CinematicSlide, int, int> OnSlideChanged;
        public event Action<CinematicSceneData> OnCinematicStarted;
        public event Action<CinematicSceneData> OnCinematicFinished;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnCinematicRequested += PlaySceneById;
        }

        private void OnDisable()
        {
            GameEvents.OnCinematicRequested -= PlaySceneById;
        }

        public void PlaySceneById(string sceneId)
        {
            var scene = allScenes.Find(s => s.sceneId == sceneId);
            if (scene != null)
            {
                PlayScene(scene);
            }
            else
            {
                Debug.LogWarning($"[CinematicManager] Scene {sceneId} not found in database.");
            }
        }

        public void PlayScene(CinematicSceneData scene)
        {
            currentScene = scene;
            currentSlideIndex = 0;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.Cinematic);
            }

            OnCinematicStarted?.Invoke(currentScene);
            DisplayCurrentSlide();
        }

        public void AdvanceSlide()
        {
            if (currentScene == null) return;

            currentSlideIndex++;
            if (currentSlideIndex >= currentScene.slides.Count)
            {
                CompleteCinematic();
            }
            else
            {
                DisplayCurrentSlide();
            }
        }

        private void DisplayCurrentSlide()
        {
            if (currentScene == null || currentSlideIndex >= currentScene.slides.Count) return;

            var slide = currentScene.slides[currentSlideIndex];
            OnSlideChanged?.Invoke(currentScene, slide, currentSlideIndex, currentScene.slides.Count);
        }

        private void CompleteCinematic()
        {
            if (currentScene == null) return;

            var completed = currentScene;
            currentScene = null;

            // Gallery unlock
            if (GalleryManager.Instance != null)
            {
                GalleryManager.Instance.UnlockScene(completed.sceneId);
            }

            // Relationship bonuses
            GameEvents.TriggerRelationshipStatChanged(completed.characterId, "affection", completed.bonusAffection);
            GameEvents.TriggerRelationshipStatChanged(completed.characterId, "trust", completed.bonusTrust);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.Exploration);
            }

            GameEvents.TriggerCinematicCompleted(completed.sceneId);
            OnCinematicFinished?.Invoke(completed);
        }
    }
}
