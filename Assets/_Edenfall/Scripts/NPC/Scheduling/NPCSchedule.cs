using System;
using System.Collections.Generic;
using Edenfall.Core;
using UnityEngine;

namespace Edenfall.NPC.Scheduling
{
    [System.Serializable]
    public class ScheduleEntry
    {
        public TimeOfDay timeOfDay;
        public string locationName;
        public Vector2 targetPosition;
        public string activityDescription;
        public string poseAnimation;
    }

    [CreateAssetMenu(fileName = "NPCSchedule", menuName = "Edenfall/NPC Schedule")]
    public class NPCSchedule : ScriptableObject
    {
        public List<ScheduleEntry> entries = new();

        public ScheduleEntry GetEntryForTime(TimeOfDay time)
        {
            foreach (var e in entries)
            {
                if (e.timeOfDay == time) return e;
            }
            return entries.Count > 0 ? entries[0] : null;
        }
    }
}
