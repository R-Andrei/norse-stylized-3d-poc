using UnityEngine;

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

        [Header("Shape")]
        [SerializeField, Min(0.1f)]
        private float radiusMetres = 3f;

        [SerializeField, Range(0.05f, 2f)]
        private float topRadiusScale = 0.8f;

        [SerializeField, Min(0.5f)]
        private float heightMetres = 18f;

        [SerializeField, Range(0.1f, 2f)]
        private float visualEnvelopeRadiusScale = 1f;

        [SerializeField, Range(0.01f, 1f)]
        private float visualEnvelopeEdgeSoftness = 0.65f;

        [SerializeField, Range(0f, 75f)]
        private float maximumVisualLeanDegrees = 25f;

        [Header("Internal Ray Structure")]
        [SerializeField, Range(1, 8)]
        private int strandCount = 5;

        [SerializeField]
        private Vector2 strandWidthRange = new Vector2(0.07f, 0.16f);

        [SerializeField, Range(0f, 1f)]
        private float strandSpread = 0.72f;

        [SerializeField, Range(0f, 1f)]
        private float strandPositionVariation = 0.45f;

        [SerializeField, Range(0f, 1f)]
        private float strandIntensityVariation = 0.32f;

        [SerializeField, Range(0f, 1f)]
        private float strandLengthVariation = 0.28f;

        [SerializeField, Range(0f, 1f)]
        private float strandTaper = 0.35f;

        [SerializeField, Range(0.01f, 1f)]
        private float strandEdgeSoftness = 0.42f;

        [SerializeField, Range(0f, 1f)]
        private float strandClusterBias = 0.38f;

        [Header("Atmospheric Appearance")]
        [SerializeField]
        [ColorUsage(false, true)]
        private Color colourMultiplier = Color.white;

        [SerializeField, Range(0f, 1f)]
        private float warmthContribution = 0.35f;

        [SerializeField, Min(0f)]
        private float shaftIntensity = 0.28f;

        [SerializeField, Min(0f)]
        private float envelopeHazeIntensity = 0.025f;

        [SerializeField, Range(0f, 8f)]
        private float scatterLength = 2.2f;

        [SerializeField, Range(0f, 1f)]
        private float scatterSoftness = 0.35f;

        [SerializeField, Range(0.001f, 0.49f)]
        private float heightFade = 0.08f;

        [SerializeField, Range(0f, 1f)]
        private float cameraIntersectionFade = 0.9f;

        [Header("Surface Illumination")]
        [SerializeField, Min(0f)]
        private float groundLightIntensity = 0.42f;

        [SerializeField, Min(0f)]
        private float surfaceLightIntensity = 0.28f;

        [SerializeField, Min(0f)]
        private float cloudCompensationIntensity = 0.45f;

        [SerializeField, Range(0.01f, 1f)]
        private float edgeSoftness = 0.42f;

        [SerializeField, Range(0f, 1f)]
        private float footprintIrregularity = 0.2f;

        [SerializeField, Min(0f)]
        private float coreEmphasis = 0.2f;

        [Header("Subtle Evolution")]
        [SerializeField, Range(0f, 0.5f)]
        private float fluctuationStrength = 0.06f;

        [SerializeField, Min(0f)]
        private float fluctuationSpeed = 0.12f;

        [SerializeField, Range(0f, 0.35f)]
        private float widthBreathingStrength = 0.035f;

        [SerializeField, Range(0f, 0.25f)]
        private float lateralDriftStrength = 0.025f;

        [SerializeField, Min(0f)]
        private float patternEvolutionSpeed = 0.08f;

        [SerializeField, Range(0f, 1f)]
        private float perStrandPhaseVariation = 0.8f;

        [SerializeField, Min(1)]
        private int variationSeed = 7319;

        private WeatherLightRayController registeredController;
        private WeatherLightRayHandle handle;
        private string lastError = string.Empty;
        private uint lifecycleRevision = 1u;

        public WeatherLightRayController ControllerOverride =>
            controllerOverride;
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
        public float RadiusMetres => radiusMetres;
        public float TopRadiusScale => topRadiusScale;
        public float HeightMetres => heightMetres;
        public float VisualEnvelopeRadiusScale =>
            visualEnvelopeRadiusScale;
        public float VisualEnvelopeEdgeSoftness =>
            visualEnvelopeEdgeSoftness;
        public float MaximumVisualLeanDegrees =>
            maximumVisualLeanDegrees;
        public Color ColourMultiplier => colourMultiplier;
        public float WarmthContribution => warmthContribution;
        public float ShaftIntensity => shaftIntensity;
        public float EnvelopeHazeIntensity => envelopeHazeIntensity;
        public float GroundLightIntensity => groundLightIntensity;
        public float SurfaceLightIntensity => surfaceLightIntensity;
        public float CloudCompensationIntensity =>
            cloudCompensationIntensity;
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
            radiusMetres = Mathf.Max(0.1f, radiusMetres);
            topRadiusScale = Mathf.Clamp(topRadiusScale, 0.05f, 2f);
            heightMetres = Mathf.Max(0.5f, heightMetres);
            visualEnvelopeRadiusScale = Mathf.Clamp(
                visualEnvelopeRadiusScale,
                0.1f,
                2f);
            visualEnvelopeEdgeSoftness = Mathf.Clamp(
                visualEnvelopeEdgeSoftness,
                0.01f,
                1f);
            maximumVisualLeanDegrees = Mathf.Clamp(
                maximumVisualLeanDegrees,
                0f,
                75f);
            strandCount = Mathf.Clamp(strandCount, 1, 8);
            strandWidthRange = NormalizeRange(
                strandWidthRange,
                0.01f,
                0.5f);
            strandSpread = Mathf.Clamp01(strandSpread);
            strandPositionVariation = Mathf.Clamp01(
                strandPositionVariation);
            strandIntensityVariation = Mathf.Clamp01(
                strandIntensityVariation);
            strandLengthVariation = Mathf.Clamp01(
                strandLengthVariation);
            strandTaper = Mathf.Clamp01(strandTaper);
            strandEdgeSoftness = Mathf.Clamp(
                strandEdgeSoftness,
                0.01f,
                1f);
            strandClusterBias = Mathf.Clamp01(strandClusterBias);
            warmthContribution = Mathf.Clamp01(warmthContribution);
            shaftIntensity = Mathf.Max(0f, shaftIntensity);
            envelopeHazeIntensity = Mathf.Max(
                0f,
                envelopeHazeIntensity);
            scatterLength = Mathf.Clamp(scatterLength, 0f, 8f);
            scatterSoftness = Mathf.Clamp01(scatterSoftness);
            heightFade = Mathf.Clamp(heightFade, 0.001f, 0.49f);
            cameraIntersectionFade = Mathf.Clamp01(
                cameraIntersectionFade);
            groundLightIntensity = Mathf.Max(
                0f,
                groundLightIntensity);
            surfaceLightIntensity = Mathf.Max(
                0f,
                surfaceLightIntensity);
            cloudCompensationIntensity = Mathf.Max(
                0f,
                cloudCompensationIntensity);
            edgeSoftness = Mathf.Clamp(edgeSoftness, 0.01f, 1f);
            footprintIrregularity = Mathf.Clamp01(
                footprintIrregularity);
            coreEmphasis = Mathf.Max(0f, coreEmphasis);
            fluctuationStrength = Mathf.Clamp(
                fluctuationStrength,
                0f,
                0.5f);
            fluctuationSpeed = Mathf.Max(0f, fluctuationSpeed);
            widthBreathingStrength = Mathf.Clamp(
                widthBreathingStrength,
                0f,
                0.35f);
            lateralDriftStrength = Mathf.Clamp(
                lateralDriftStrength,
                0f,
                0.25f);
            patternEvolutionSpeed = Mathf.Max(
                0f,
                patternEvolutionSpeed);
            perStrandPhaseVariation = Mathf.Clamp01(
                perStrandPhaseVariation);
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

        public WeatherLightRayDescriptor BuildDescriptor()
        {
            float baseRadius = Mathf.Max(0.1f, radiusMetres);
            float topRadius = baseRadius * Mathf.Max(
                0.05f,
                topRadiusScale);
            return new WeatherLightRayDescriptor(
                sourceKind,
                WeatherLightRayOriginKind.Authored,
                cloudPolicy,
                lifetimePolicy,
                sourceGatePolicy,
                WeatherLightRayMovementPolicy.Static,
                heightMetres,
                new Vector2(baseRadius, baseRadius),
                new Vector2(topRadius, topRadius),
                visualEnvelopeRadiusScale,
                visualEnvelopeEdgeSoftness,
                maximumVisualLeanDegrees,
                strandCount,
                strandWidthRange,
                strandSpread,
                strandPositionVariation,
                strandIntensityVariation,
                strandLengthVariation,
                strandTaper,
                strandEdgeSoftness,
                strandClusterBias,
                colourMultiplier,
                warmthContribution,
                shaftIntensity,
                envelopeHazeIntensity,
                scatterLength,
                scatterSoftness,
                heightFade,
                cameraIntersectionFade,
                groundLightIntensity,
                surfaceLightIntensity,
                cloudCompensationIntensity,
                edgeSoftness,
                footprintIrregularity,
                coreEmphasis,
                fluctuationStrength,
                fluctuationSpeed,
                widthBreathingStrength,
                lateralDriftStrength,
                patternEvolutionSpeed,
                perStrandPhaseVariation,
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

        private static Vector2 NormalizeRange(
            Vector2 range,
            float minimum,
            float maximum)
        {
            float min = Mathf.Clamp(
                Mathf.Min(range.x, range.y),
                minimum,
                maximum);
            float max = Mathf.Clamp(
                Mathf.Max(range.x, range.y),
                min,
                maximum);
            return new Vector2(min, max);
        }

        private static uint NextRevision(uint revision)
        {
            revision++;
            return revision == 0u ? 1u : revision;
        }
    }
}
