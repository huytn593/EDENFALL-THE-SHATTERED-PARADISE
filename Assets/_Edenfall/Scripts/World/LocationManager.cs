using System;
using System.Collections.Generic;
using Edenfall.Core;
using Edenfall.NPC.Relationships;
using UnityEngine;

namespace Edenfall.World
{
    public class LocationManager : MonoBehaviour
    {
        public static LocationManager Instance { get; private set; }

        [Header("Database")]
        [SerializeField] private List<LocationDefinition> allLocations = new();

        [Header("Runtime State")]
        [SerializeField] private LocationDefinition currentLocation;
        [SerializeField] private HashSet<string> unlockedLocationIds = new();

        public LocationDefinition CurrentLocation => currentLocation;
        public IReadOnlyList<LocationDefinition> AllLocations => allLocations;

        public event Action<LocationDefinition> OnLocationChanged;
        public event Action<LocationDefinition> OnLocationUnlocked;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeUnlockedLocations();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeUnlockedLocations()
        {
            unlockedLocationIds.Clear();
            foreach (var loc in allLocations)
            {
                if (loc != null && loc.isUnlockedByDefault)
                {
                    unlockedLocationIds.Add(loc.locationId);
                }
            }
        }

        public bool IsLocationUnlocked(string locationId)
        {
            return unlockedLocationIds.Contains(locationId);
        }

        public void UnlockLocation(string locationId)
        {
            if (!unlockedLocationIds.Contains(locationId))
            {
                unlockedLocationIds.Add(locationId);
                var loc = GetLocationById(locationId);
                if (loc != null)
                {
                    OnLocationUnlocked?.Invoke(loc);
                    Debug.Log($"[LocationManager] Unlocked location: {loc.nameEn} ({locationId})");
                }
            }
        }

        public LocationDefinition GetLocationById(string locationId)
        {
            return allLocations.Find(l => l != null && l.locationId == locationId);
        }

        public bool TravelTo(string locationId)
        {
            var loc = GetLocationById(locationId);
            if (loc == null)
            {
                Debug.LogWarning($"[LocationManager] Target location not found: {locationId}");
                return false;
            }

            if (!unlockedLocationIds.Contains(locationId))
            {
                Debug.LogWarning($"[LocationManager] Location {locationId} is locked!");
                return false;
            }

            currentLocation = loc;
            OnLocationChanged?.Invoke(currentLocation);
            Debug.Log($"[LocationManager] Traveled to: {loc.nameEn}");
            return true;
        }

        public void CheckUnlocksForRelationship(RelationshipModel relationship)
        {
            if (relationship == null) return;

            foreach (var loc in allLocations)
            {
                if (loc == null || unlockedLocationIds.Contains(loc.locationId)) continue;

                if (loc.requiredAffectionToUnlock > 0 && relationship.affection >= loc.requiredAffectionToUnlock)
                {
                    UnlockLocation(loc.locationId);
                }
            }
        }
    }
}
