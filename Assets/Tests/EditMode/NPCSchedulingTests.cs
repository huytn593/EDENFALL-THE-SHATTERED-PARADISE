using System.Reflection;
using Edenfall.Core;
using Edenfall.NPC.Scheduling;
using Edenfall.World;
using NUnit.Framework;
using UnityEngine;

namespace Edenfall.Tests.EditMode
{
    public class NPCSchedulingTests
    {
        private GameObject controllerObj;
        private NPCScheduleController scheduleController;

        [SetUp]
        public void SetUp()
        {
            controllerObj = new GameObject("Test_NPCScheduleController");
            scheduleController = controllerObj.AddComponent<NPCScheduleController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (controllerObj != null) Object.DestroyImmediate(controllerObj);
        }

        [Test]
        public void DiscreteSlots_ReturnExpectedLocations()
        {
            // Verify all 4 discrete time slots return canonical sanctuary locations
            string morningLoc = scheduleController.GetAssignedLocationForTime("aurelia", TimeOfDay.Morning);
            string afternoonLoc = scheduleController.GetAssignedLocationForTime("aurelia", TimeOfDay.Afternoon);
            string eveningLoc = scheduleController.GetAssignedLocationForTime("aurelia", TimeOfDay.Evening);
            string nightLoc = scheduleController.GetAssignedLocationForTime("aurelia", TimeOfDay.Night);

            Assert.AreEqual("loc_dawn_garden", morningLoc, "Morning slot must assign Dawn Garden");
            Assert.AreEqual("loc_celestial_altar", afternoonLoc, "Afternoon slot must assign Celestial Altar");
            Assert.AreEqual("loc_solstice_balcony", eveningLoc, "Evening slot must assign Solstice Balcony");
            Assert.AreEqual("loc_aurelia_chamber", nightLoc, "Night slot must assign Aurelia Chamber");
        }

        [Test]
        public void CinematicLocking_DefersRelocationUntilUnlocked()
        {
            string lastNotifiedLocation = null;
            scheduleController.OnNPCLocationChanged += (npc, loc) => lastNotifiedLocation = loc;

            // 1. Lock cinematics
            scheduleController.SetCinematicLock(true);
            Assert.IsTrue(scheduleController.IsLocked, "Controller should report locked status");

            // 2. Invoke private HandleTimeOfDayChanged via reflection during lock
            MethodInfo handleTimeMethod = typeof(NPCScheduleController).GetMethod("HandleTimeOfDayChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(handleTimeMethod);

            handleTimeMethod.Invoke(scheduleController, new object[] { TimeOfDay.Afternoon });

            // Since it is locked, no location event should have fired immediately
            Assert.IsNull(lastNotifiedLocation, "Relocation must be deferred while cinematic lock is active");

            // 3. Unlock cinematic
            scheduleController.SetCinematicLock(false);

            // Now deferred relocation must have executed
            Assert.IsFalse(scheduleController.IsLocked, "Controller should be unlocked");
            Assert.AreEqual("loc_celestial_altar", lastNotifiedLocation, "Deferred relocation must apply upon unlock");
        }

        [Test]
        public void NPCStageAnchor_CalculatesExactScaleAndGrounding()
        {
            var charObj = new GameObject("Test_Heroine");
            var anchor = charObj.AddComponent<NPCStageAnchor>();
            var sr = charObj.AddComponent<SpriteRenderer>();

            // Create a test 100x200 sprite with pivot at bottom center (0.5, 0.0)
            Texture2D tex = new Texture2D(100, 200);
            Sprite testSprite = Sprite.Create(tex, new Rect(0, 0, 100, 200), new Vector2(0.5f, 0.0f), 100f);
            sr.sprite = testSprite;

            var camObj = new GameObject("Test_Cam");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.2f; // Total visible height = 8.4f

            // FullBody Grounded alignment
            anchor.AlignCharacter(charObj.transform, sr, StagingMode.FullBodyGrounded, cam);

            // Target height is 65% of 8.4 = 5.46f. Raw sprite height is 2.0f (200px / 100 PPU).
            // Calculated scale should be 5.46 / 2.0 = 2.73f.
            float expectedScale = (8.4f * 0.65f) / 2.0f;
            Assert.AreEqual(expectedScale, charObj.transform.localScale.y, 0.001f);

            // Since pivot.y is 0.0 (bottom), distanceToBottom is 0, so target Y must equal groundContactY (-2.10f)
            Assert.AreEqual(anchor.GroundContactY, charObj.transform.position.y, 0.001f);

            // Verify idle motion clamping never dips below ground
            Vector3 clampedDip = anchor.ClampIdleMotion(charObj.transform.position, new Vector3(0, -0.5f, 0));
            Assert.GreaterOrEqual(clampedDip.y, anchor.GroundContactY, "Idle motion must never dip below ground line");

            Object.DestroyImmediate(charObj);
            Object.DestroyImmediate(camObj);
            Object.DestroyImmediate(tex);
        }
    }
}
