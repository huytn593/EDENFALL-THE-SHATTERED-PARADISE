using UnityEngine;

namespace Edenfall.Core
{
    public enum GameMode
    {
        Exploration,
        Dialogue,
        Cinematic,
        MenuPaused
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GameMode currentMode = GameMode.Exploration;
        [SerializeField] private TimeOfDay currentTime = TimeOfDay.Morning;
        [SerializeField] private WeatherType currentWeather = WeatherType.CelestialClear;
        [SerializeField] private int currentDay = 1;
        [SerializeField] [Range(0f, 100f)] private float sanctuaryRestoration = 10f;
        [SerializeField] private VisionLayer activeVisionLayer = VisionLayer.Normal;

        public GameMode CurrentMode => currentMode;
        public TimeOfDay CurrentTime => currentTime;
        public WeatherType CurrentWeather => currentWeather;
        public int CurrentDay => currentDay;
        public float SanctuaryRestoration => sanctuaryRestoration;
        public VisionLayer ActiveVisionLayer => activeVisionLayer;

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

        private void Start()
        {
            // Initial broadcasts
            GameEvents.TriggerTimeOfDayChanged(currentTime);
            GameEvents.TriggerWeatherChanged(currentWeather);
            GameEvents.TriggerDayChanged(currentDay);
            GameEvents.TriggerSanctuaryRestorationChanged(sanctuaryRestoration);
            GameEvents.TriggerVisionLayerChanged(activeVisionLayer);
        }

        public void SetGameMode(GameMode mode)
        {
            currentMode = mode;
            Time.timeScale = (mode == GameMode.MenuPaused) ? 0f : 1f;
        }

        public void AdvanceTime()
        {
            currentTime = currentTime.Next();
            if (currentTime == TimeOfDay.Morning)
            {
                currentDay++;
                GameEvents.TriggerDayChanged(currentDay);
            }
            GameEvents.TriggerTimeOfDayChanged(currentTime);
        }

        public void SetWeather(WeatherType weather)
        {
            currentWeather = weather;
            GameEvents.TriggerWeatherChanged(weather);
        }

        public void AddSanctuaryRestoration(float delta)
        {
            sanctuaryRestoration = Mathf.Clamp(sanctuaryRestoration + delta, 0f, 100f);
            GameEvents.TriggerSanctuaryRestorationChanged(sanctuaryRestoration);
        }

        public void CycleVisionLayer()
        {
            activeVisionLayer = activeVisionLayer switch
            {
                VisionLayer.Normal => VisionLayer.MagicalAura,
                VisionLayer.MagicalAura => VisionLayer.EnergyPathways,
                VisionLayer.EnergyPathways => VisionLayer.CelestialSilhouette,
                VisionLayer.CelestialSilhouette => VisionLayer.MiasmaSight,
                VisionLayer.MiasmaSight => VisionLayer.Normal,
                _ => VisionLayer.Normal
            };

            GameEvents.TriggerVisionLayerChanged(activeVisionLayer);
        }

        public void SetVisionLayer(VisionLayer layer)
        {
            activeVisionLayer = layer;
            GameEvents.TriggerVisionLayerChanged(activeVisionLayer);
        }
    }
}
