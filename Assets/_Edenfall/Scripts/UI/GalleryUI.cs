using System.Collections.Generic;
using Edenfall.Cinematics;
using Edenfall.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.UI
{
    public class GalleryUI : MonoBehaviour
    {
        [Header("CG Scene Definitions")]
        [SerializeField] private List<CinematicSceneData> scenes = new();

        [Header("Container")]
        [SerializeField] private Transform galleryGridContainer;
        [SerializeField] private GameObject galleryItemPrefab;

        private void OnEnable()
        {
            RefreshGallery();
        }

        public void RefreshGallery()
        {
            if (galleryGridContainer == null || galleryItemPrefab == null) return;

            foreach (Transform child in galleryGridContainer) Destroy(child.gameObject);

            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            foreach (var scene in scenes)
            {
                bool unlocked = GalleryManager.Instance != null && GalleryManager.Instance.IsUnlocked(scene.sceneId);

                var obj = Instantiate(galleryItemPrefab, galleryGridContainer);
                var textComp = obj.GetComponentInChildren<Text>();
                if (textComp != null)
                {
                    textComp.text = unlocked
                        ? $"✦ {scene.GetTitle(isVi)}"
                        : (isVi ? "🔒 [Chưa Mở Khóa]" : "🔒 [Locked]");
                }

                var btn = obj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = unlocked;
                    if (unlocked)
                    {
                        var capturedId = scene.sceneId;
                        btn.onClick.AddListener(() =>
                        {
                            gameObject.SetActive(false);
                            if (CinematicManager.Instance != null)
                            {
                                CinematicManager.Instance.PlaySceneById(capturedId);
                            }
                        });
                    }
                }
            }
        }
    }
}
