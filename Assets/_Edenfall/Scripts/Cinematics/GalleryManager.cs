using System;
using System.Collections.Generic;
using UnityEngine;

namespace Edenfall.Cinematics
{
    public class GalleryManager : MonoBehaviour
    {
        public static GalleryManager Instance { get; private set; }

        [SerializeField] private List<string> unlockedSceneIds = new();
        public IReadOnlyList<string> UnlockedSceneIds => unlockedSceneIds;

        public event Action<string> OnSceneUnlocked;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool IsUnlocked(string sceneId)
        {
            return unlockedSceneIds.Contains(sceneId);
        }

        public void UnlockScene(string sceneId)
        {
            if (!unlockedSceneIds.Contains(sceneId))
            {
                unlockedSceneIds.Add(sceneId);
                OnSceneUnlocked?.Invoke(sceneId);
            }
        }

        public void LoadUnlockedList(List<string> loadedIds)
        {
            unlockedSceneIds = new List<string>(loadedIds);
        }
    }
}
