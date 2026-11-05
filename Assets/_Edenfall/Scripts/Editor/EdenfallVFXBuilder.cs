#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Edenfall.Editor
{
    public static class EdenfallVFXBuilder
    {
        private const string ProfilePath = "Assets/_Edenfall/Settings/DawnSanctuary_VolumeProfile.asset";

        [MenuItem("Edenfall/Build VFX and PostProcessing Assets")]
        public static void BuildAllVFX()
        {
            EnsureProfile();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EdenfallVFXBuilder] VFX profiles and settings built successfully!");
        }

        public static VolumeProfile EnsureProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            // 1. Bloom
            if (!profile.TryGet<Bloom>(out var bloom))
            {
                bloom = profile.Add<Bloom>(true);
            }
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 1.35f;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.85f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.72f;

            // 2. Color Adjustments
            if (!profile.TryGet<ColorAdjustments>(out var colorAdj))
            {
                colorAdj = profile.Add<ColorAdjustments>(true);
            }
            colorAdj.postExposure.overrideState = true;
            colorAdj.postExposure.value = 0.25f;
            colorAdj.contrast.overrideState = true;
            colorAdj.contrast.value = 18f;
            colorAdj.saturation.overrideState = true;
            colorAdj.saturation.value = 20f;

            // 3. Vignette
            if (!profile.TryGet<Vignette>(out var vignette))
            {
                vignette = profile.Add<Vignette>(true);
            }
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.24f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.45f;
            vignette.color.overrideState = true;
            vignette.color.value = new Color(0.04f, 0.05f, 0.12f, 1f);

            // 4. Tonemapping
            if (!profile.TryGet<Tonemapping>(out var tone))
            {
                tone = profile.Add<Tonemapping>(true);
            }
            tone.mode.overrideState = true;
            tone.mode.value = TonemappingMode.ACES;

            EditorUtility.SetDirty(profile);
            return profile;
        }

        public static GameObject CreateCelestialSparks(Transform parent)
        {
            var vfxObj = new GameObject("VFX_CelestialSparks");
            vfxObj.transform.SetParent(parent);
            vfxObj.transform.position = new Vector3(0, 0f, 0);

            var ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3.5f, 5.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            main.startColor = new Color(1.0f, 0.92f, 0.55f, 0.85f);
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 14f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(35f, 2f, 0.5f);

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.95f, 0.7f), 0f), new GradientColorKey(new Color(1f, 0.85f, 0.4f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.85f, 0.25f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLife.color = gradient;

            var renderer = vfxObj.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 15;
            renderer.sharedMaterial = GetOrCreateParticleMaterial("VFX_CelestialSparks", "Particle_Spark.png", true, new Color(1.0f, 0.92f, 0.55f, 0.9f));

            return vfxObj;
        }

        public static GameObject CreateFallingPetals(Transform parent)
        {
            var vfxObj = new GameObject("VFX_FallingPetals");
            vfxObj.transform.SetParent(parent);
            vfxObj.transform.position = new Vector3(5f, 9f, 0);

            var ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4.5f, 7.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.24f);
            main.startColor = new Color(1.0f, 0.88f, 0.75f, 0.9f);
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 6f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(25f, 1f, 0.5f);
            shape.rotation = new Vector3(180f, 0, 0);

            var rotOverLife = ps.rotationOverLifetime;
            rotOverLife.enabled = true;
            rotOverLife.z = new ParticleSystem.MinMaxCurve(-90f, 90f);

            var renderer = vfxObj.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 16;
            renderer.sharedMaterial = GetOrCreateParticleMaterial("VFX_FallingPetals", "Particle_Petal.png", false, new Color(1.0f, 0.88f, 0.85f, 0.85f));

            return vfxObj;
        }

        public static GameObject CreateAltarAura(Transform parent)
        {
            var vfxObj = new GameObject("VFX_AltarLeylineAura");
            vfxObj.transform.SetParent(parent);
            vfxObj.transform.position = new Vector3(10f, 4.5f, 0);

            var ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.0f, 3.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
            main.startColor = new Color(0.65f, 0.92f, 1.0f, 0.9f);
            main.maxParticles = 40;

            var emission = ps.emission;
            emission.rateOverTime = 12f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f;
            shape.radius = 0.5f;

            var renderer = vfxObj.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 14;
            renderer.sharedMaterial = GetOrCreateParticleMaterial("VFX_AltarLeylineAura", "Particle_Glow.png", true, new Color(0.70f, 0.85f, 1.0f, 0.8f));

            return vfxObj;
        }

        public static Material GetOrCreateParticleMaterial(string matName, string texName, bool additive, Color tint)
        {
            string matPath = $"Assets/_Edenfall/Materials/{matName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            if (mat == null)
            {
                mat = new Material(shader);
                if (!Directory.Exists("Assets/_Edenfall/Materials"))
                {
                    Directory.CreateDirectory("Assets/_Edenfall/Materials");
                }
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/_Edenfall/Art/VFX/{texName}");
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);

            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // Transparent
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", additive ? 1f : 0f);

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
#endif
