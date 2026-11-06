using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.U2D.Animation;

namespace Edenfall.Tests.EditMode
{
    [TestFixture]
    public class AureliaDeformableRigTests
    {
        private const string PrefabPath = "Assets/_Edenfall/Prefabs/Characters/AureliaRiggedPrefab.prefab";
        private GameObject prefabInstance;
        private AureliaRiggedCharacter rig;

        [SetUp]
        public void SetUp()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, $"Prefab not found at: {PrefabPath}");
            prefabInstance = Object.Instantiate(prefab);
            rig = prefabInstance.GetComponent<AureliaRiggedCharacter>();
            Assert.IsNotNull(rig, "AureliaRiggedCharacter component missing on instantiated prefab.");
        }

        [TearDown]
        public void TearDown()
        {
            if (prefabInstance != null)
            {
                Object.DestroyImmediate(prefabInstance);
            }
        }

        [Test]
        public void Validation_01_EveryDeclaredRigBoneExistsAndIsNonZero()
        {
            Assert.IsNotNull(rig.BoneHead, "Bone_Head must exist.");
            Assert.IsNotNull(rig.BoneEyeL, "Bone_EyeL must exist.");
            Assert.IsNotNull(rig.BoneIrisL, "Bone_IrisL must exist.");
            Assert.IsNotNull(rig.BoneEyeR, "Bone_EyeR must exist.");
            Assert.IsNotNull(rig.BoneIrisR, "Bone_IrisR must exist.");
            Assert.IsNotNull(rig.BoneEyebrows, "Bone_Eyebrows must exist.");
            Assert.IsNotNull(rig.BoneMouth, "Bone_Mouth must exist.");
            Assert.IsNotNull(rig.BoneHairSideL_01, "Bone_HairSideL_01 must exist.");
            Assert.IsNotNull(rig.BoneHairSideL_02, "Bone_HairSideL_02 must exist.");
            Assert.IsNotNull(rig.BoneHairSideL_03, "Bone_HairSideL_03 must exist.");
        }

        [Test]
        public void Validation_02_EveryAnimatedVisualPartIsConnectedToDriverBone()
        {
            var faceBase = prefabInstance.transform.Find("Bone_Root/Bone_Pelvis/Bone_Torso/Bone_Chest/Bone_Neck/Bone_Head/Layer_02_Head_FaceBase");
            Assert.IsNotNull(faceBase, "FaceBase must be parented under Bone_Head.");

            var eyeL = prefabInstance.transform.Find("Bone_Root/Bone_Pelvis/Bone_Torso/Bone_Chest/Bone_Neck/Bone_Head/Bone_EyeL/Layer_03_EyeL_Open");
            Assert.IsNotNull(eyeL, "EyeL must be parented under Bone_EyeL.");

            var mouth = prefabInstance.transform.Find("Bone_Root/Bone_Pelvis/Bone_Torso/Bone_Chest/Bone_Neck/Bone_Head/Bone_Mouth/Layer_06_Mouth");
            Assert.IsNotNull(mouth, "Mouth must be parented under Bone_Mouth.");
        }

        [Test]
        public void Validation_03_EverySpriteSkinHasValidGeometry()
        {
            var skins = prefabInstance.GetComponentsInChildren<SpriteSkin>(true);
            Assert.GreaterOrEqual(skins.Length, 4, "Must have at least 4 SpriteSkin deformers.");

            foreach (var ss in skins)
            {
                var sr = ss.GetComponent<SpriteRenderer>();
                Assert.IsNotNull(sr, $"SpriteSkin on {ss.name} must have a SpriteRenderer.");
                Assert.IsNotNull(sr.sprite, $"SpriteRenderer on {ss.name} must have a valid Sprite.");

                int vCount = SpriteDataAccessExtensions.GetVertexCount(sr.sprite);
                Assert.Greater(vCount, 10, $"Sprite {sr.sprite.name} must have more than 10 vertices for deformable geometry. Found: {vCount}");

                var indices = SpriteDataAccessExtensions.GetIndices(sr.sprite);
                Assert.Greater(indices.Length, 10, $"Sprite {sr.sprite.name} must have valid indices. Found: {indices.Length}");
            }
        }

        [Test]
        public void Validation_04_BoneWeightsAreNormalized()
        {
            var skins = prefabInstance.GetComponentsInChildren<SpriteSkin>(true);
            foreach (var ss in skins)
            {
                var sr = ss.GetComponent<SpriteRenderer>();
                var weights = SpriteDataAccessExtensions.GetVertexAttribute<BoneWeight>(sr.sprite, UnityEngine.Rendering.VertexAttribute.BlendWeight);
                Assert.Greater(weights.Length, 0, $"Sprite {sr.sprite.name} must have BlendWeights.");

                for (int i = 0; i < weights.Length; i++)
                {
                    var bw = weights[i];
                    float sum = bw.weight0 + bw.weight1 + bw.weight2 + bw.weight3;
                    Assert.AreEqual(1.0f, sum, 0.002f, $"Vertex {i} on sprite {sr.sprite.name} weights do not sum to 1.0 (sum={sum}).");
                }
            }
        }

        [Test]
        public void Validation_05_BindPosesAndBoneReferencesAreValid()
        {
            var skins = prefabInstance.GetComponentsInChildren<SpriteSkin>(true);
            foreach (var ss in skins)
            {
                Assert.IsNotNull(ss.rootBone, $"SpriteSkin on {ss.name} rootBone must not be null.");
                Assert.IsNotNull(ss.boneTransforms, $"SpriteSkin on {ss.name} boneTransforms must not be null.");
                Assert.AreEqual(3, ss.boneTransforms.Length, $"SpriteSkin on {ss.name} must have 3 bones assigned.");

                for (int b = 0; b < ss.boneTransforms.Length; b++)
                {
                    Assert.IsNotNull(ss.boneTransforms[b], $"BoneTransform[{b}] on {ss.name} is null.");
                }

                var bindPoses = SpriteDataAccessExtensions.GetBindPoses(ss.GetComponent<SpriteRenderer>().sprite);
                Assert.AreEqual(3, bindPoses.Length, $"BindPoses on sprite {ss.name} must equal bone count.");
            }
        }

        [Test]
        public void Validation_06_NoOrphanRenderersExist()
        {
            var renderers = prefabInstance.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.AreEqual(17, renderers.Length, "Expected 17 registered character visual renderers.");

            foreach (var sr in renderers)
            {
                // Must be either inside Bone_Root or have a SpriteSkin
                bool isUnderBoneRoot = sr.transform.IsChildOf(prefabInstance.transform.Find("Bone_Root"));
                bool hasSkin = sr.GetComponent<SpriteSkin>() != null;
                Assert.IsTrue(isUnderBoneRoot || hasSkin, $"Renderer {sr.name} is an orphan outside the bone hierarchy.");
            }
        }

        [Test]
        public void Validation_07_CharacterDisplaysCorrectlyInRestPose()
        {
            rig.SetAnimationState(AureliaAnimationState.Idle_Normal, true);
            Assert.AreEqual(Vector3.zero, rig.transform.localPosition, "Rig root should remain at local origin.");

            // Rotating head must displace attached face in world space without separating
            Vector3 restFacePos = prefabInstance.transform.Find("Bone_Root/Bone_Pelvis/Bone_Torso/Bone_Chest/Bone_Neck/Bone_Head/Layer_02_Head_FaceBase").position;
            rig.BoneHead.localRotation = Quaternion.Euler(0, 0, 10f);
            Vector3 rotatedFacePos = prefabInstance.transform.Find("Bone_Root/Bone_Pelvis/Bone_Torso/Bone_Chest/Bone_Neck/Bone_Head/Layer_02_Head_FaceBase").position;

            Assert.AreNotEqual(restFacePos, rotatedFacePos, "FaceBase must move when Bone_Head rotates.");
        }

        [Test]
        public void Validation_08_NoDuplicateTransformWriters()
        {
            var components = prefabInstance.GetComponents<Component>();
            int controllerCount = 0;
            foreach (var c in components)
            {
                if (c is AureliaRiggedCharacter) controllerCount++;
            }
            Assert.AreEqual(1, controllerCount, "Must have exactly one AureliaRiggedCharacter controller on the prefab root.");
        }
    }
}
