using System.Collections;
using Edenfall.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Edenfall.World
{
    public class DawnSanctuaryLighting : MonoBehaviour
    {
        [Header("Lights")]
        [SerializeField] private Light2D globalLight2D;
        [SerializeField] private Light2D altarPointLight2D;
        [SerializeField] private Light directionalKeyLight;

        [Header("Volume Post Processing")]
        [SerializeField] private Volume globalVolume;

        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 1.5f;

        [System.Serializable]
        public struct TimeLightingProfile
        {
            public TimeOfDay time;
            public Color globalLightColor;
            public float globalIntensity;
            public Color directionalColor;
            public float directionalIntensity;
            public Color skyAmbientColor;
        }

        [Header("Lighting Profiles")]
        [SerializeField] private TimeLightingProfile[] profiles = new TimeLightingProfile[]
        {
            new TimeLightingProfile
            {
                time = TimeOfDay.Morning,
                globalLightColor = new Color(1.0f, 0.91f, 0.74f),
                globalIntensity = 1.15f,
                directionalColor = new Color(1.0f, 0.85f, 0.60f),
                directionalIntensity = 1.2f,
                skyAmbientColor = new Color(0.22f, 0.20f, 0.35f)
            },
            new TimeLightingProfile
            {
                time = TimeOfDay.Afternoon,
                globalLightColor = new Color(1.0f, 0.98f, 0.94f),
                globalIntensity = 1.35f,
                directionalColor = new Color(1.0f, 0.97f, 0.90f),
                directionalIntensity = 1.4f,
                skyAmbientColor = new Color(0.35f, 0.45f, 0.65f)
            },
            new TimeLightingProfile
            {
                time = TimeOfDay.Evening,
                globalLightColor = new Color(0.70f, 0.47f, 0.66f),
                globalIntensity = 0.85f,
                directionalColor = new Color(0.85f, 0.40f, 0.55f),
                directionalIntensity = 0.9f,
                skyAmbientColor = new Color(0.20f, 0.15f, 0.30f)
            },
            new TimeLightingProfile
            {
                time = TimeOfDay.Night,
                globalLightColor = new Color(0.25f, 0.33f, 0.54f),
                globalIntensity = 0.65f,
                directionalColor = new Color(0.35f, 0.45f, 0.75f),
                directionalIntensity = 0.6f,
                skyAmbientColor = new Color(0.06f, 0.08f, 0.18f)
            }
        };

        private Coroutine transitionRoutine;

        private void OnEnable()
        {
            GameEvents.OnTimeOfDayChanged += HandleTimeChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnTimeOfDayChanged -= HandleTimeChanged;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                ApplyInstant(GameManager.Instance.CurrentTime);
            }
            else
            {
                ApplyInstant(TimeOfDay.Morning);
            }
        }

        private void Update()
        {
            // Subtle Altar Holy Pulsing
            if (altarPointLight2D != null)
            {
                float pulse = 1f + 0.18f * Mathf.Sin(Time.time * 2.2f);
                altarPointLight2D.intensity = 1.4f * pulse;
            }
        }

        private void HandleTimeChanged(TimeOfDay time)
        {
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = StartCoroutine(TransitionTo(time));
        }

        private void ApplyInstant(TimeOfDay time)
        {
            var p = GetProfile(time);
            if (globalLight2D != null)
            {
                globalLight2D.color = p.globalLightColor;
                globalLight2D.intensity = p.globalIntensity;
            }

            if (directionalKeyLight != null)
            {
                directionalKeyLight.color = p.directionalColor;
                directionalKeyLight.intensity = p.directionalIntensity;
            }

            RenderSettings.ambientSkyColor = p.skyAmbientColor;
        }

        private IEnumerator TransitionTo(TimeOfDay targetTime)
        {
            var target = GetProfile(targetTime);

            Color startGlobalCol = globalLight2D != null ? globalLight2D.color : Color.white;
            float startGlobalInt = globalLight2D != null ? globalLight2D.intensity : 1f;

            Color startDirCol = directionalKeyLight != null ? directionalKeyLight.color : Color.white;
            float startDirInt = directionalKeyLight != null ? directionalKeyLight.intensity : 1f;

            Color startSkyCol = RenderSettings.ambientSkyColor;

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

                if (globalLight2D != null)
                {
                    globalLight2D.color = Color.Lerp(startGlobalCol, target.globalLightColor, t);
                    globalLight2D.intensity = Mathf.Lerp(startGlobalInt, target.globalIntensity, t);
                }

                if (directionalKeyLight != null)
                {
                    directionalKeyLight.color = Color.Lerp(startDirCol, target.directionalColor, t);
                    directionalKeyLight.intensity = Mathf.Lerp(startDirInt, target.directionalIntensity, t);
                }

                RenderSettings.ambientSkyColor = Color.Lerp(startSkyCol, target.skyAmbientColor, t);
                yield return null;
            }

            ApplyInstant(targetTime);
            transitionRoutine = null;
        }

        private TimeLightingProfile GetProfile(TimeOfDay time)
        {
            foreach (var p in profiles)
            {
                if (p.time == time) return p;
            }
            return profiles[0];
        }
    }
}
