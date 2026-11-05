using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Cinematics;
using Edenfall.NPC.Behavior;
using Edenfall.World;
using NUnit.Framework;
using UnityEngine;

namespace Edenfall.Tests.EditMode
{
    public class AureliaLiveRigTests
    {
        private GameObject characterObj;
        private AureliaLiveRig liveRig;
        private AureliaRigController rigController;
        private NPCStageAnchor anchor;
        private SpriteRenderer spriteRenderer;

        [SetUp]
        public void SetUp()
        {
            characterObj = new GameObject("Test_AureliaLiveRig");
            spriteRenderer = characterObj.AddComponent<SpriteRenderer>();
            anchor = characterObj.AddComponent<NPCStageAnchor>();
            liveRig = characterObj.AddComponent<AureliaLiveRig>();
            rigController = characterObj.AddComponent<AureliaRigController>();

            // Setup a dummy sprite with bottom-center pivot
            Texture2D tex = new Texture2D(100, 200);
            Sprite dummySprite = Sprite.Create(tex, new Rect(0, 0, 100, 200), new Vector2(0.5f, 0.0f), 100f);
            spriteRenderer.sprite = dummySprite;

            liveRig.ConfigureMasterSprites(dummySprite, dummySprite, new List<Sprite>(), new List<Sprite>(), spriteRenderer, null, anchor);
        }

        [TearDown]
        public void TearDown()
        {
            if (characterObj != null) Object.DestroyImmediate(characterObj);
        }

        [Test]
        public void All15AnimationStates_ConfigureTargetParametersCorrectly()
        {
            var allStates = (AureliaAnimationState[])System.Enum.GetValues(typeof(AureliaAnimationState));
            Assert.AreEqual(15, allStates.Length, "Must have exactly 15 animation states as requested in Stage 5");

            foreach (var state in allStates)
            {
                liveRig.SetAnimationState(state, immediate: true);
                Assert.AreEqual(state, liveRig.CurrentState, $"Current state should update to {state}");
                var p = liveRig.Parameters;

                // Verify parameters are not NaN and within valid Live2D ranges
                Assert.IsFalse(float.IsNaN(p.angleX), $"{state}: angleX is NaN");
                Assert.IsFalse(float.IsNaN(p.angleY), $"{state}: angleY is NaN");
                Assert.IsFalse(float.IsNaN(p.angleZ), $"{state}: angleZ is NaN");
                Assert.GreaterOrEqual(p.blush, 0f, $"{state}: blush should be >= 0");
                Assert.LessOrEqual(p.blush, 1f, $"{state}: blush should be <= 1");
            }
        }

        [Test]
        public void EmotionMapping_MapsEveryEmotionToValidState()
        {
            var emotions = (CharacterEmotion[])System.Enum.GetValues(typeof(CharacterEmotion));
            foreach (var emotion in emotions)
            {
                var idleState = rigController.MapEmotionToAnimationState(emotion, false);
                var talkState = rigController.MapEmotionToAnimationState(emotion, true);

                Assert.IsTrue(System.Enum.IsDefined(typeof(AureliaAnimationState), idleState));
                Assert.IsTrue(System.Enum.IsDefined(typeof(AureliaAnimationState), talkState));
            }
        }

        [Test]
        public void GroundContact_RemainsLockedDuringUpwardBreathing()
        {
            var camObj = new GameObject("Test_Cam");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.2f;

            anchor.AlignCharacter(characterObj.transform, spriteRenderer, StagingMode.DialoguePortrait, cam);
            liveRig.CaptureBaseTransform();

            float initialWorldY = characterObj.transform.position.y;
            Assert.AreEqual(anchor.PortraitBaselineY, initialWorldY, 0.001f);

            // Simulate breathing at peak inhalation
            liveRig.SetAnimationState(AureliaAnimationState.Idle_Normal, true);
            Vector3 clampedPos = anchor.ClampIdleMotion(characterObj.transform.position, Vector3.zero);
            Assert.AreEqual(initialWorldY, clampedPos.y, 0.001f, "Contact point Y must never shift during breathing");

            Object.DestroyImmediate(camObj);
        }
    }
}
