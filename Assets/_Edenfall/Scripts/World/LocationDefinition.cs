using System;
using System.Collections.Generic;
using Edenfall.Core;
using UnityEngine;

namespace Edenfall.World
{
    public enum LocationType
    {
        Garden,
        Altar,
        Balcony,
        PrivateChamber,
        Outpost
    }

    [System.Serializable]
    public class LocationHotspot
    {
        public string hotspotId;
        public string titleVi;
        public string titleEn;
        public Vector2 screenPosition; // Normalized 0..1 or anchor coordinate
        public string actionType; // "talk", "inspect", "gather", "event", "rest"
        public string targetParameter; // DialogueTree ID, Event ID, Item ID, etc.

        public string GetTitle(bool isVi = true)
        {
            return isVi ? (string.IsNullOrEmpty(titleVi) ? titleEn : titleVi)
                        : (string.IsNullOrEmpty(titleEn) ? titleVi : titleEn);
        }
    }

    [CreateAssetMenu(fileName = "LocationDefinition", menuName = "Edenfall/Location Definition")]
    public class LocationDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string locationId;
        public string nameVi;
        public string nameEn;
        [TextArea(2, 4)] public string descriptionVi;
        [TextArea(2, 4)] public string descriptionEn;
        public LocationType locationType = LocationType.Garden;

        [Header("Visuals & Atmosphere")]
        public Sprite backgroundSprite;
        public Sprite distantSpireSprite; // Optional parallax layer
        public Color ambientTint = Color.white;
        public bool hasFallingPetals = true;
        public bool hasCelestialSparks = true;

        [Header("Progression")]
        public bool isUnlockedByDefault = true;
        public int requiredAffectionToUnlock = 0;
        public string unlockRequirementDescriptionVi;
        public string unlockRequirementDescriptionEn;

        [Header("Interactive Hotspots")]
        public List<LocationHotspot> hotspots = new();

        public string GetName(bool isVi = true)
        {
            return isVi ? (string.IsNullOrEmpty(nameVi) ? nameEn : nameVi)
                        : (string.IsNullOrEmpty(nameEn) ? nameVi : nameEn);
        }

        public string GetDescription(bool isVi = true)
        {
            return isVi ? (string.IsNullOrEmpty(descriptionVi) ? descriptionEn : descriptionVi)
                        : (string.IsNullOrEmpty(descriptionEn) ? descriptionVi : descriptionEn);
        }
    }
}
