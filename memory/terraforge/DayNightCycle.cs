using System;
using UnityEngine;
using UnityEngine.Events;

namespace TerraForge
{
    /// <summary>
    /// Manages day/night cycle with time progression,
    /// sun/moon positioning, lighting transitions, and dawn/dusk events.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Time Settings")]
        [Tooltip("Seconds for a full day in-game")]
        [SerializeField] private float dayLengthSeconds = 300f;
        
        [Tooltip("Current time of day (0-1, where 0=midnight, 0.5=noon)")]
        [Range(0f, 1f)]
        [SerializeField] private float timeOfDay = 0.25f;
        
        [Tooltip("Paused at time? Useful for testing")]
        [SerializeField] private bool freezeTime = false;
        
        [Header("Celestial Bodies")]
        [Tooltip("Transform for the sun (rotates for day light)")]
        [SerializeField] private Transform sunTransform;
        
        [Tooltip("Transform for the moon (opposite to sun)")]
        [SerializeField] private Transform moonTransform;
        
        [Tooltip("Sun light source")]
        [SerializeField] private Light sunLight;
        
        [Tooltip("Moon light source")]
        [SerializeField] private Light moonLight;
        
        [Tooltip("Ambient light intensity at noon")]
        [SerializeField] private float maxAmbientIntensity = 1f;
        
        [Tooltip("Ambient light intensity at midnight")]
        [SerializeField] private float minAmbientIntensity = 0.1f;
        
        [Header("Lighting Colors")]
        [Tooltip("Sun color at noon")]
        [SerializeField] private Color noonSunColor = new Color(1f, 0.98f, 0.9f);
        
        [Tooltip("Sun color during sunrise/sunset")]
        [SerializeField] private Color dawnDuskSunColor = new Color(1f, 0.6f, 0.3f);
        
        [Tooltip("Sun color at night (should be dark)")]
        [SerializeField] private Color nightSunColor = new Color(0.2f, 0.2f, 0.4f);
        
        [Tooltip("Sky color at noon")]
        [SerializeField] private Color noonSkyColor = new Color(0.53f, 0.81f, 0.92f);
        
        [Tooltip("Sky color at night")]
        [SerializeField] private Color nightSkyColor = new Color(0.05f, 0.05f, 0.15f);
        
        [Header("Events")]
        [Tooltip("Event fired when dawn begins (sunrise)")]
        public UnityEvent OnDawn;
        
        [Tooltip("Event fired when dusk begins (sunset)")]
        public UnityEvent OnDusk;
        
        [Tooltip("Event fired when day reaches noon")]
        public UnityEvent OnNoon;
        
        [Tooltip("Event fired when night reaches midnight")]
        public UnityEvent OnMidnight;
        
        [Tooltip("Event fired every tick with current time (0-1)")]
        public UnityEvent<float> OnTimeChanged;
        
        // Internal state
        private float previousTimeOfDay;
        private bool dawnTriggered = false;
        private bool duskTriggered = false;
        private bool noonTriggered = false;
        private bool midnightTriggered = false;
        
        // Time thresholds
        private const float DAWN_START = 0.2f;
        private const float DAWN_END = 0.3f;
        private const float NOON_TIME = 0.5f;
        private const float DUSK_START = 0.7f;
        private const float DUSK_END = 0.8f;
        private const float MIDNIGHT_TIME = 0f;
        
        // Public properties
        public float TimeOfDay => timeOfDay;
        public float DayLengthSeconds => dayLengthSeconds;
        public bool IsDaytime => timeOfDay > DAWN_END && timeOfDay < DUSK_START;
        public bool IsNighttime => timeOfDay < DAWN_START || timeOfDay > DUSK_END;
        public bool IsDawn => timeOfDay >= DAWN_START && timeOfDay <= DAWN_END;
        public bool IsDusk => timeOfDay >= DUSK_START && timeOfDay <= DUSK_END;
        
        /// <summary>
        /// Get hours in 24-hour format (0-23.99)
        /// </summary>
        public float GetHourDecimal() => timeOfDay * 24f;
        
        /// <summary>
        /// Get formatted time string (HH:MM)
        /// </summary>
        public string GetTimeString()
        {
            float totalHours = timeOfDay * 24f;
            int hours = Mathf.FloorToInt(totalHours);
            int minutes = Mathf.FloorToInt((totalHours - hours) * 60f);
            return $"{hours:D2}:{minutes:D2}";
        }
        
        private void Update()
        {
            if (!freezeTime)
                UpdateTime();
            
            UpdateCelestialBodies();
            UpdateLighting();
            CheckEvents();
            
            previousTimeOfDay = timeOfDay;
        }
        
        private void UpdateTime()
        {
            timeOfDay += Time.deltaTime / dayLengthSeconds;
            timeOfDay %= 1f;
            OnTimeChanged?.Invoke(timeOfDay);
        }
        
        private void UpdateCelestialBodies()
        {
            float sunAngle = (timeOfDay - 0.25f) * 360f;
            
            if (sunTransform != null)
                sunTransform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);
            
            if (moonTransform != null)
                moonTransform.rotation = Quaternion.Euler(sunAngle + 180f, 0f, 0f);
        }
        
        private void UpdateLighting()
        {
            if (sunLight == null) return;
            
            float sunIntensity;
            Color sunColor;
            
            if (timeOfDay >= DAWN_START && timeOfDay <= DAWN_END)
            {
                float t = Mathf.InverseLerp(DAWN_START, DAWN_END, timeOfDay);
                sunIntensity = Mathf.Lerp(0f, 1f, t);
                sunColor = Color.Lerp(nightSunColor, dawnDuskSunColor, t);
            }
            else if (timeOfDay > DAWN_END && timeOfDay < DUSK_START)
            {
                sunIntensity = 1f;
                float distFromNoon = Mathf.Abs(timeOfDay - NOON_TIME);
                float dayProgress = 1f - (distFromNoon / (NOON_TIME - DAWN_END));
                sunColor = Color.Lerp(dawnDuskSunColor, noonSunColor, dayProgress);
            }
            else if (timeOfDay >= DUSK_START && timeOfDay <= DUSK_END)
            {
                float t = Mathf.InverseLerp(DUSK_START, DUSK_END, timeOfDay);
                sunIntensity = Mathf.Lerp(1f, 0f, t);
                sunColor = Color.Lerp(dawnDuskSunColor, nightSunColor, t);
            }
            else
            {
                sunIntensity = 0f;
                sunColor = nightSunColor;
            }
            
            sunLight.intensity = sunIntensity;
            sunLight.color = sunColor;
            
            if (moonLight != null)
                moonLight.intensity = 1f - sunIntensity;
            
            // Ambient lighting
            float ambientIntensity = Mathf.Lerp(minAmbientIntensity, maxAmbientIntensity, sunIntensity);
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.ambientLight = Color.Lerp(nightSkyColor, noonSkyColor, sunIntensity);
        }
        
        private void CheckEvents()
        {
            // Dawn: crossing DAWN_START threshold
            if (!dawnTriggered && previousTimeOfDay < DAWN_START && timeOfDay >= DAWN_START)
            {
                OnDawn?.Invoke();
                dawnTriggered = true;
            }
            else if (timeOfDay > DAWN_END)
            {
                dawnTriggered = false;
            }
            
            // Dusk: crossing DUSK_START threshold
            if (!duskTriggered && previousTimeOfDay < DUSK_START && timeOfDay >= DUSK_START)
            {
                OnDusk?.Invoke();
                duskTriggered = true;
            }
            else if (timeOfDay > DUSK_END)
            {
                duskTriggered = false;
            }
            
            // Noon: crossing NOON_TIME
            if (!noonTriggered && previousTimeOfDay < NOON_TIME && timeOfDay >= NOON_TIME)
            {
                OnNoon?.Invoke();
                noonTriggered = true;
            }
            else if (timeOfDay > NOON_TIME + 0.01f)
            {
                noonTriggered = false;
            }
            
            // Midnight: wrapping around or crossing 0
            if (!midnightTriggered)
            {
                bool crossedMidnight = previousTimeOfDay > 0.9f && timeOfDay < 0.1f;
                bool crossingZero = previousTimeOfDay < timeOfDay && timeOfDay >= MIDNIGHT_TIME && previousTimeOfDay < 0.01f;
                
                if (crossedMidnight || crossingZero)
                {
                    OnMidnight?.Invoke();
                    midnightTriggered = true;
                }
            }
            else if (timeOfDay > 0.1f)
            {
                midnightTriggered = false;
            }
        }
        
        /// <summary> Set time directly (0-1 range) </summary>
        public void SetTime(float newTime) => timeOfDay = Mathf.Clamp01(newTime);
        
        /// <summary> Set time from hour (0-24) </summary>
        public void SetTimeFromHour(float hour) => timeOfDay = Mathf.Clamp01(hour / 24f);
        
        /// <summary> Skip to a specific time instantly </summary>
        public void SkipToTime(float targetTime)
        {
            timeOfDay = Mathf.Clamp01(targetTime);
            UpdateCelestialBodies();
            UpdateLighting();
        }
        
        /// <summary> Pause time progression </summary>
        public void Pause() => freezeTime = true;
        
        /// <summary> Resume time progression </summary>
        public void Resume() => freezeTime = false;
        
        /// <summary> Toggle pause state </summary>
        public void TogglePause() => freezeTime = !freezeTime;
        
        /// <summary> Advance time by specific amount </summary>
        public void AdvanceTime(float amount) => timeOfDay = (timeOfDay + amount) % 1f;
        
        /// <summary> Get current phase of day </summary>
        public DayPhase GetCurrentPhase()
        {
            if (IsDawn) return DayPhase.Dawn;
            if (IsDaytime) return DayPhase.Day;
            if (IsDusk) return DayPhase.Dusk;
            return DayPhase.Night;
        }
        
        /// <summary> Set day length in real-time seconds </summary>
        public void SetDayLength(float seconds) => dayLengthSeconds = Mathf.Max(1f, seconds);
    }
    
    /// <summary> Phases of the day </summary>
    public enum DayPhase
    {
        Dawn,
        Day,
        Dusk,
        Night
    }
}
