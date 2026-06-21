using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Lighting
{
    [Serializable]
    public sealed class LightingModifierSlot
    {
        public LightingModifierProfile profile;

        [Range(0f, 1f)]
        public float weight;

        public void Apply(ref TimeOfDayLightingState state)
        {
            if (profile != null && weight > 0f)
            {
                profile.Apply(ref state, weight);
            }
        }
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class TimeOfDayController : MonoBehaviour
    {
        private static readonly int SkyTintId =
            Shader.PropertyToID("_SkyTint");
        private static readonly int SkyGroundColorId =
            Shader.PropertyToID("_GroundColor");
        private static readonly int SkyExposureId =
            Shader.PropertyToID("_Exposure");
        private static readonly int AtmosphereThicknessId =
            Shader.PropertyToID("_AtmosphereThickness");

        [Header("Time")]
        [SerializeField]
        private TimeOfDayProfile profile;

        [Tooltip("Real-world seconds required for one complete 24-hour cycle.")]
        [Min(1f)]
        [SerializeField]
        private float secondsPerFullDay = 180f;

        [Range(0f, 23.999f)]
        [SerializeField]
        private float currentHour = 12f;

        [SerializeField]
        private bool paused;

        [Tooltip("Additional multiplier for the passage of time.")]
        [Min(0f)]
        [SerializeField]
        private float timeScale = 1f;

        [SerializeField]
        private bool useUnscaledTime = true;

        [Tooltip(
            "When enabled, changing Current Hour outside Play Mode previews " +
            "the lighting directly in the Scene and Game views.")]
        [SerializeField]
        private bool previewInEditMode = true;

        [Header("Celestial rig")]
        [SerializeField]
        private Transform celestialRig;

        [SerializeField]
        private Light sunLight;

        [Tooltip("Rotates the entire sun path around the world's Y axis.")]
        [Range(-180f, 180f)]
        [SerializeField]
        private float sunPathYawDegrees = -35f;

        [Tooltip(
            "Leans the sun path so midday does not need to pass directly " +
            "overhead. This is an artistic control, not an astronomy model.")]
        [Range(-89f, 89f)]
        [SerializeField]
        private float sunPathTiltDegrees = 15f;

        [Header("Skybox")]
        [Tooltip(
            "Assign a material using the Skybox/Procedural shader. " +
            "The controller clones it during Play Mode.")]
        [SerializeField]
        private Material proceduralSkyboxTemplate;

        [Header("Optional lighting modifiers")]
        [SerializeField]
        private LightingModifierSlot regionModifier =
            new LightingModifierSlot();

        [SerializeField]
        private LightingModifierSlot weatherModifier =
            new LightingModifierSlot();

        [SerializeField]
        private LightingModifierSlot additionalModifier =
            new LightingModifierSlot();

        [Header("Optional environment reflection refresh")]
        [Tooltip(
            "Leave disabled while authoring unless changing skybox-based " +
            "reflections is visibly important. Ambient light is controlled " +
            "directly and does not require this.")]
        [SerializeField]
        private bool updateDynamicEnvironment;

        [Min(0.1f)]
        [SerializeField]
        private float environmentUpdateInterval = 1f;

        private Material runtimeSkybox;
        private float environmentUpdateTimer;

        public float CurrentHour => currentHour;
        public float NormalizedTime => currentHour / 24f;
        public bool Paused => paused;

        public event Action<float> TimeChanged;

        private void OnEnable()
        {
            EnsureSkyboxMaterial();
            ApplyCurrentState();

            if (Application.isPlaying && updateDynamicEnvironment)
            {
                DynamicGI.UpdateEnvironment();
            }
        }

        private void OnDisable()
        {
            CleanUpRuntimeSkybox();
        }

        private void OnValidate()
        {
            currentHour = Mathf.Clamp(currentHour, 0f, 23.999f);
            secondsPerFullDay = Mathf.Max(1f, secondsPerFullDay);
            timeScale = Mathf.Max(0f, timeScale);
            environmentUpdateInterval =
                Mathf.Max(0.1f, environmentUpdateInterval);

            if (!Application.isPlaying &&
                previewInEditMode &&
                isActiveAndEnabled)
            {
                EnsureSkyboxMaterial();
                ApplyCurrentState();
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                AdvanceTime();
                ApplyCurrentState();
                UpdateDynamicEnvironmentIfNeeded();
            }
            else if (previewInEditMode)
            {
                ApplyCurrentState();
            }
        }

        public void SetCurrentHour(float hour)
        {
            currentHour = Mathf.Repeat(hour, 24f);
            ApplyCurrentState();
        }

        public void SetPaused(bool shouldPause)
        {
            paused = shouldPause;
        }

        [ContextMenu("Apply Current Time")]
        private void ApplyCurrentTimeFromContextMenu()
        {
            ApplyCurrentState();
        }

        [ContextMenu("Set Time/Deep Night - 00:00")]
        private void SetDeepNight()
        {
            SetCurrentHour(0f);
        }

        [ContextMenu("Set Time/Midday - 12:00")]
        private void SetMidday()
        {
            SetCurrentHour(12f);
        }

        private void AdvanceTime()
        {
            if (paused || profile == null || timeScale <= 0f)
            {
                return;
            }

            float deltaTime = useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            float hoursPerSecond = 24f / secondsPerFullDay;

            currentHour = Mathf.Repeat(
                currentHour + deltaTime * hoursPerSecond * timeScale,
                24f);
        }

        private void ApplyCurrentState()
        {
            if (profile == null)
            {
                return;
            }

            TimeOfDayLightingState state = profile.Evaluate(currentHour);

            // Deliberate order: broad region changes first, weather second,
            // and temporary scene/event overrides last.
            regionModifier.Apply(ref state);
            weatherModifier.Apply(ref state);
            additionalModifier.Apply(ref state);

            ApplySun(state);
            ApplyAmbientAndReflections(state);
            ApplySkybox(state);
            ApplyFog(state);

            if (Application.isPlaying)
            {
                TimeChanged?.Invoke(currentHour);
            }
        }

        private void ApplySun(TimeOfDayLightingState state)
        {
            if (celestialRig != null)
            {
                float orbitAngle =
                    currentHour / 24f * 360f - 90f;

                Quaternion orbit =
                    Quaternion.AngleAxis(orbitAngle, Vector3.right);
                Quaternion tilt =
                    Quaternion.AngleAxis(
                        sunPathTiltDegrees,
                        Vector3.forward);
                Quaternion yaw =
                    Quaternion.AngleAxis(
                        sunPathYawDegrees,
                        Vector3.up);

                celestialRig.localRotation = yaw * tilt * orbit;
            }

            if (sunLight == null)
            {
                return;
            }

            sunLight.color = state.sunColor;
            sunLight.intensity = state.sunIntensity;
            RenderSettings.sun = sunLight;
        }

        private static void ApplyAmbientAndReflections(
            TimeOfDayLightingState state)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = state.ambientColor;
            RenderSettings.ambientIntensity = state.ambientIntensity;
            RenderSettings.reflectionIntensity =
                state.reflectionIntensity;
        }

        private void ApplySkybox(TimeOfDayLightingState state)
        {
            Material skybox = EnsureSkyboxMaterial();

            if (skybox == null)
            {
                return;
            }

            SetColorIfPresent(skybox, SkyTintId, state.skyTint);
            SetColorIfPresent(
                skybox,
                SkyGroundColorId,
                state.skyGroundColor);
            SetFloatIfPresent(
                skybox,
                AtmosphereThicknessId,
                state.skyAtmosphereThickness);
            SetFloatIfPresent(
                skybox,
                SkyExposureId,
                state.skyExposure);

            if (RenderSettings.skybox != skybox)
            {
                RenderSettings.skybox = skybox;
            }
        }

        private static void ApplyFog(TimeOfDayLightingState state)
        {
            bool fogEnabled = state.fogDensity > 0.00001f;

            RenderSettings.fog = fogEnabled;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = state.fogColor;
            RenderSettings.fogDensity =
                Mathf.Max(0f, state.fogDensity);
        }

        private Material EnsureSkyboxMaterial()
        {
            if (proceduralSkyboxTemplate == null)
            {
                return null;
            }

            if (!Application.isPlaying)
            {
                if (RenderSettings.skybox != proceduralSkyboxTemplate)
                {
                    RenderSettings.skybox = proceduralSkyboxTemplate;
                }

                return proceduralSkyboxTemplate;
            }

            if (runtimeSkybox == null)
            {
                runtimeSkybox = new Material(proceduralSkyboxTemplate)
                {
                    name =
                        proceduralSkyboxTemplate.name +
                        " (Runtime Instance)"
                };
            }

            if (RenderSettings.skybox != runtimeSkybox)
            {
                RenderSettings.skybox = runtimeSkybox;
            }

            return runtimeSkybox;
        }

        private void CleanUpRuntimeSkybox()
        {
            if (runtimeSkybox == null)
            {
                return;
            }

            if (RenderSettings.skybox == runtimeSkybox)
            {
                RenderSettings.skybox = proceduralSkyboxTemplate;
            }

            if (Application.isPlaying)
            {
                Destroy(runtimeSkybox);
            }
            else
            {
                DestroyImmediate(runtimeSkybox);
            }

            runtimeSkybox = null;
        }

        private void UpdateDynamicEnvironmentIfNeeded()
        {
            if (!updateDynamicEnvironment)
            {
                return;
            }

            environmentUpdateTimer += Time.unscaledDeltaTime;

            if (environmentUpdateTimer < environmentUpdateInterval)
            {
                return;
            }

            environmentUpdateTimer = 0f;
            DynamicGI.UpdateEnvironment();
        }

        private static void SetColorIfPresent(
            Material material,
            int propertyId,
            Color value)
        {
            if (material.HasProperty(propertyId))
            {
                material.SetColor(propertyId, value);
            }
        }

        private static void SetFloatIfPresent(
            Material material,
            int propertyId,
            float value)
        {
            if (material.HasProperty(propertyId))
            {
                material.SetFloat(propertyId, value);
            }
        }
    }
}