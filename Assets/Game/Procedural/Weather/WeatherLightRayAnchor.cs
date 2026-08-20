using UnityEngine;
using UnityEngine.Serialization;

namespace ProgrammaticStylized3D.Weather
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Weather/Weather LightRay Anchor")]
    public sealed class WeatherLightRayAnchor : MonoBehaviour
    {
        [Header("Binding and Policy")]
        [SerializeField]
        private WeatherLightRayController controllerOverride;

        [SerializeField]
        private WeatherLightRayPreset presetOverride;

        [SerializeField]
        private bool previewInEditMode = true;

        [SerializeField]
        private WeatherLightRaySourceKind sourceKind =
            WeatherLightRaySourceKind.Sun;

        [SerializeField]
        private WeatherLightRayCloudPolicy cloudPolicy =
            WeatherLightRayCloudPolicy.IgnoreClouds;

        [SerializeField]
        private WeatherLightRaySourceGatePolicy sourceGatePolicy =
            WeatherLightRaySourceGatePolicy.RequireActiveSource;

        [Header("Lifecycle")]
        [SerializeField]
        private WeatherLightRayLifetimePolicy lifetimePolicy =
            WeatherLightRayLifetimePolicy.Permanent;

        [SerializeField, Min(0f)]
        private float fadeInDurationSeconds = 1.1f;

        [SerializeField, Min(0f)]
        private float holdDurationSeconds = 5f;

        [SerializeField, Min(0f)]
        private float fadeOutDurationSeconds = 1.4f;

        [SerializeField]
        private bool externallyControlledVisible = true;

        [Header("Continuous Beam Shape")]
        [SerializeField, Min(0.5f)]
        private float heightMetres = 18f;

        [SerializeField, Range(0f, 75f)]
        private float maximumVisualLeanDegrees = 25f;

        [SerializeField, Min(
            WeatherLightRayAreaLayout.MinimumDiameterMetres)]
        private float areaDiameterMetres = 4.8f;

        [SerializeField, Range(
            WeatherLightRayAreaLayout.MinimumBeamSpacingMetres,
            WeatherLightRayAreaLayout.MaximumBeamSpacingMetres),
            FormerlySerializedAs("legacyBeamSpacingMetres")]
        private float beamSpacingMetres =
            WeatherLightRayAreaLayout.DefaultBeamSpacingMetres;

        [SerializeField, Min(1)]
        private int variationSeed = 7319;

        [Header("Local Instance Overrides")]
        [SerializeField, Min(0f)]
        private float localIntensityMultiplier = 1f;

        [SerializeField]
        private bool overridePresetBeamSpacing;

        private WeatherLightRayController registeredController;
        private WeatherLightRayHandle handle;
        private string lastError = string.Empty;
        private uint lifecycleRevision = 1u;

        public WeatherLightRayController ControllerOverride =>
            controllerOverride;
        public WeatherLightRayPreset PresetOverride => presetOverride;
        public bool PreviewInEditMode => previewInEditMode;
        public WeatherLightRaySourceKind SourceKind => sourceKind;
        public WeatherLightRayCloudPolicy CloudPolicy => cloudPolicy;
        public WeatherLightRaySourceGatePolicy SourceGatePolicy =>
            sourceGatePolicy;
        public WeatherLightRayLifetimePolicy LifetimePolicy =>
            lifetimePolicy;
        public bool ExternallyControlledVisible =>
            externallyControlledVisible;
        public uint LifecycleRevision => lifecycleRevision;
        public float HeightMetres => heightMetres;
        public float MaximumVisualLeanDegrees =>
            Mathf.Clamp(maximumVisualLeanDegrees, 0f, 75f);
        public float AreaDiameterMetres => areaDiameterMetres;
        public float BeamSpacingMetres => beamSpacingMetres;
        public bool OverridePresetBeamSpacing => overridePresetBeamSpacing;
        public float LocalIntensityMultiplier =>
            Mathf.Max(0f, localIntensityMultiplier);
        public float FadeInDurationSeconds => fadeInDurationSeconds;
        public float HoldDurationSeconds => holdDurationSeconds;
        public float FadeOutDurationSeconds => fadeOutDurationSeconds;
        public uint VariationSeed => (uint)Mathf.Max(1, variationSeed);
        public WeatherLightRayController RegisteredController =>
            registeredController;
        public WeatherLightRayHandle Handle => handle;
        public string LastError => lastError;

        private void OnEnable()
        {
            RefreshRegistration();
        }

        private void OnDisable()
        {
            ReleaseRegistration();
        }

        private void OnDestroy()
        {
            ReleaseRegistration();
        }

        private void OnValidate()
        {
            heightMetres = Mathf.Max(0.5f, heightMetres);
            maximumVisualLeanDegrees = Mathf.Clamp(
                maximumVisualLeanDegrees,
                0f,
                75f);
            areaDiameterMetres = Mathf.Max(
                WeatherLightRayAreaLayout.MinimumDiameterMetres,
                !float.IsNaN(areaDiameterMetres) &&
                    !float.IsInfinity(areaDiameterMetres)
                    ? areaDiameterMetres
                    : WeatherLightRayAreaLayout.MinimumDiameterMetres);
            beamSpacingMetres = Mathf.Clamp(
                !float.IsNaN(beamSpacingMetres) &&
                    !float.IsInfinity(beamSpacingMetres)
                    ? beamSpacingMetres
                    : WeatherLightRayAreaLayout.DefaultBeamSpacingMetres,
                WeatherLightRayAreaLayout.MinimumBeamSpacingMetres,
                WeatherLightRayAreaLayout.MaximumBeamSpacingMetres);
            fadeInDurationSeconds = Mathf.Max(
                0f,
                fadeInDurationSeconds);
            holdDurationSeconds = Mathf.Max(
                0f,
                holdDurationSeconds);
            fadeOutDurationSeconds = Mathf.Max(
                0f,
                fadeOutDurationSeconds);
            variationSeed = Mathf.Max(1, variationSeed);
            localIntensityMultiplier = Mathf.Max(
                0f,
                localIntensityMultiplier);

            if (lifetimePolicy == WeatherLightRayLifetimePolicy.Timed)
            {
                lifecycleRevision = NextRevision(lifecycleRevision);
            }

            if (isActiveAndEnabled)
            {
                RefreshRegistration();
            }
        }

        private void Update()
        {
            if (Application.isPlaying || previewInEditMode)
            {
                RefreshRegistration();
            }
        }

        /// <summary>
        /// Builds request-local state only. Shared visual presentation is
        /// applied by the Controller from the ray's resolved preset before
        /// the descriptor may become active.
        /// </summary>
        public WeatherLightRayDescriptor BuildLocalDescriptor()
        {
            return new WeatherLightRayDescriptor(
                sourceKind,
                WeatherLightRayOriginKind.Authored,
                cloudPolicy,
                lifetimePolicy,
                sourceGatePolicy,
                WeatherLightRayMovementPolicy.Static,
                heightMetres,
                maximumVisualLeanDegrees,
                areaDiameterMetres,
                beamSpacingMetres,
                Vector2.one,
                0f,
                0.5f,
                0f,
                0.1f,
                0.1f,
                0f,
                Color.white,
                0f,
                1f,
                0f,
                0f,
                0f,
                0f,
                1f,
                0f,
                0f,
                0.5f,
                WeatherLightRayEvolutionPreset.Static,
                0f,
                0f,
                fadeInDurationSeconds,
                holdDurationSeconds,
                fadeOutDurationSeconds,
                0,
                VariationSeed);
        }

        public void RestartTimedLifecycle()
        {
            lifecycleRevision = NextRevision(lifecycleRevision);
            RefreshRegistration();
        }

        public void SetExternallyControlledVisible(bool visible)
        {
            if (externallyControlledVisible == visible)
            {
                return;
            }

            externallyControlledVisible = visible;
            RefreshRegistration();
        }

        public void RefreshRegistration()
        {
            WeatherLightRayController desiredController =
                controllerOverride != null
                    ? controllerOverride
                    : WeatherLightRayController.PublishedController;

            if (registeredController != desiredController)
            {
                ReleaseRegistration();
                registeredController = desiredController;
            }

            if (registeredController == null)
            {
                handle = default;
                lastError =
                    "No published Weather LightRay Controller is available.";
                return;
            }

            if (!registeredController.TryRegisterOrUpdateAuthoredRay(
                    this,
                    ref handle,
                    out string error))
            {
                lastError = error;
                return;
            }

            lastError = string.Empty;
        }

        private void ReleaseRegistration()
        {
            if (registeredController != null && handle.IsValid)
            {
                registeredController.ReleaseAuthoredRay(this, handle);
            }

            registeredController = null;
            handle = default;
        }

        private static uint NextRevision(uint revision)
        {
            revision++;
            return revision == 0u ? 1u : revision;
        }
    }
}
