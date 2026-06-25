using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry
{
    public enum GeneratedRiverInteractionParticipation
    {
        Automatic,
        Disabled
    }

    // Retained only for serialized/API compatibility with the former combined
    // profile. The inspector and runtime now author each feature separately.
    public enum GeneratedRiverResponseProfile
    {
        InheritRiver,
        Subtle,
        Standard,
        Strong,
        Custom
    }

    public enum GeneratedRiverFeatureMode
    {
        Inherit,
        Disabled,
        Custom
    }

    [Serializable]
    public sealed class GeneratedRiverInteractionSettings
    {
        private const float MinimumProfileChangeInterval = 0.5f;
        private const float MaximumProfileChangeInterval = 3f;
        private const float DefaultProfileChangeIntervalMin = 0.75f;
        private const float DefaultProfileChangeIntervalMax = 1.75f;
        [Tooltip("Controls whether this generated solid may be discovered automatically as a static river obstruction.")]
        [SerializeField]
        private GeneratedRiverInteractionParticipation participation =
            GeneratedRiverInteractionParticipation.Automatic;

        [Header("Static Pressure")]
        [Tooltip("Inherit uses the detected river's Static Pressure defaults. Disabled removes only the pressure ridge. Custom uses the values below.")]
        [SerializeField]
        private GeneratedRiverFeatureMode staticPressureMode =
            GeneratedRiverFeatureMode.Inherit;

        [Tooltip("For Custom, selects where pressure sits between the computed minimum and maximum feasible heights. Zero uses the safe lower bound; one uses the full computed ceiling.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float staticPressureStrength = 0.65f;

        [Tooltip("For Custom, controls how abruptly the pressure ridge descends away from the object. Higher values produce a steeper contact face without changing the computed maximum height.")]
        [Range(0.5f, 4f)]
        [SerializeField]
        private float staticPressureContactSharpness = 2.8f;

        [Tooltip("For Custom, controls how strongly the pressure ridge redistributes its supported height laterally. Zero keeps the cached geometry-derived profile stable.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float staticPressureWaveResponse = 1f;

        [Tooltip("For Custom, shortest randomized time in seconds between lateral pressure-profile changes.")]
        [Range(MinimumProfileChangeInterval, MaximumProfileChangeInterval)]
        [SerializeField]
        private float staticPressureProfileChangeIntervalMin =
            DefaultProfileChangeIntervalMin;

        [Tooltip("For Custom, longest randomized time in seconds between lateral pressure-profile changes. The morph duration scales with the selected interval and completes before the next change.")]
        [Range(MinimumProfileChangeInterval, MaximumProfileChangeInterval)]
        [SerializeField]
        private float staticPressureProfileChangeIntervalMax =
            DefaultProfileChangeIntervalMax;

        [Header("Obstruction Wake")]
        [Tooltip("Inherit uses the detected river's Obstruction Wake defaults. Disabled removes only the downstream obstruction wake. Custom uses the values below.")]
        [SerializeField]
        private GeneratedRiverFeatureMode obstructionWakeMode =
            GeneratedRiverFeatureMode.Inherit;

        [Tooltip("For Custom, controls obstruction-wake energy injected downstream.")]
        [Range(0f, 3f)]
        [SerializeField]
        private float obstructionWakeStrength = 1.50f;

        [Tooltip("For Custom, controls how far the obstruction wake remains active downstream.")]
        [Range(0.25f, 3f)]
        [SerializeField]
        private float obstructionWakeReach = 1f;

        [Tooltip("For Custom, controls the lateral width of the obstruction wake around the object outlets.")]
        [Range(0.5f, 2f)]
        [SerializeField]
        private float obstructionWakeSpread = 1f;

        [Tooltip("For Custom, controls spatial lee-profile variation and independent left/right release trajectory variation. Timing remains authored by the detected river.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float obstructionWakeVariation = 0.35f;

        // Hidden legacy data permits existing generated masses to migrate from
        // the former combined profile without exposing the removed controls.
        [HideInInspector, SerializeField]
        private GeneratedRiverResponseProfile responseProfile =
            GeneratedRiverResponseProfile.InheritRiver;
        [HideInInspector, SerializeField] private float pressureStrength = 0.55f;
        [HideInInspector, SerializeField] private float strengthMultiplier = 1f;
        [HideInInspector, SerializeField] private float geometryAmplitude = 1f;
        [HideInInspector, SerializeField] private float surfaceDetail = 1f;
        [HideInInspector, SerializeField] private float responseStiffness = 1f;
        [HideInInspector, SerializeField] private float wakeLength = 1f;
        [HideInInspector, SerializeField] private float unsteadiness = 1f;
        [HideInInspector, SerializeField] private float footprintPadding = 0.12f;
        [HideInInspector, SerializeField] private bool featureModelInitialized;

        public GeneratedRiverInteractionParticipation Participation =>
            participation;
        public GeneratedRiverFeatureMode StaticPressureMode =>
            staticPressureMode;
        public float StaticPressureStrength =>
            Mathf.Clamp01(staticPressureStrength);
        public float StaticPressureContactSharpness =>
            Mathf.Clamp(staticPressureContactSharpness, 0.5f, 4f);
        public float StaticPressureProfileVariation =>
            Mathf.Clamp(staticPressureWaveResponse, 0f, 2f);
        public float StaticPressureProfileChangeIntervalMin =>
            Mathf.Clamp(
                staticPressureProfileChangeIntervalMin,
                MinimumProfileChangeInterval,
                MaximumProfileChangeInterval);
        public float StaticPressureProfileChangeIntervalMax =>
            Mathf.Clamp(
                staticPressureProfileChangeIntervalMax,
                MinimumProfileChangeInterval,
                MaximumProfileChangeInterval);

        // Compatibility alias for integrations compiled against the previous
        // wave-triggered profile implementation.
        public float StaticPressureWaveResponse =>
            StaticPressureProfileVariation;

        public GeneratedRiverFeatureMode ObstructionWakeMode =>
            obstructionWakeMode;
        public float ObstructionWakeStrength =>
            Mathf.Clamp(obstructionWakeStrength, 0f, 3f);
        public float ObstructionWakeReach =>
            Mathf.Clamp(obstructionWakeReach, 0.25f, 3f);
        public float ObstructionWakeSpread =>
            Mathf.Clamp(obstructionWakeSpread, 0.5f, 2f);
        public float ObstructionWakeVariation =>
            Mathf.Clamp01(obstructionWakeVariation);

        // Compatibility surface for code compiled against the former profile.
        public GeneratedRiverResponseProfile ResponseProfile =>
            responseProfile;
        public float FootprintPadding => Mathf.Max(0f, footprintPadding);

        public ResolvedGeneratedRiverInteraction Resolve()
        {
            EnsureFeatureModel();
            return new ResolvedGeneratedRiverInteraction(
                staticPressureMode != GeneratedRiverFeatureMode.Disabled,
                StaticPressureStrength,
                StaticPressureContactSharpness,
                StaticPressureProfileVariation,
                StaticPressureProfileChangeIntervalMin,
                StaticPressureProfileChangeIntervalMax,
                obstructionWakeMode != GeneratedRiverFeatureMode.Disabled,
                ObstructionWakeStrength,
                ObstructionWakeReach,
                ObstructionWakeSpread,
                ObstructionWakeVariation);
        }

        public void Validate()
        {
            EnsureFeatureModel();
            staticPressureStrength = Mathf.Clamp01(staticPressureStrength);
            staticPressureContactSharpness = Mathf.Clamp(
                staticPressureContactSharpness,
                0.5f,
                4f);
            staticPressureWaveResponse = Mathf.Clamp(
                staticPressureWaveResponse,
                0f,
                2f);

            if (staticPressureProfileChangeIntervalMin <= 0f &&
                staticPressureProfileChangeIntervalMax <= 0f)
            {
                staticPressureProfileChangeIntervalMin =
                    DefaultProfileChangeIntervalMin;
                staticPressureProfileChangeIntervalMax =
                    DefaultProfileChangeIntervalMax;
            }

            staticPressureProfileChangeIntervalMin = Mathf.Clamp(
                staticPressureProfileChangeIntervalMin,
                MinimumProfileChangeInterval,
                MaximumProfileChangeInterval);
            staticPressureProfileChangeIntervalMax = Mathf.Clamp(
                staticPressureProfileChangeIntervalMax,
                MinimumProfileChangeInterval,
                MaximumProfileChangeInterval);

            if (staticPressureProfileChangeIntervalMin >
                staticPressureProfileChangeIntervalMax)
            {
                (staticPressureProfileChangeIntervalMin,
                    staticPressureProfileChangeIntervalMax) =
                    (staticPressureProfileChangeIntervalMax,
                        staticPressureProfileChangeIntervalMin);
            }

            obstructionWakeStrength = Mathf.Clamp(
                obstructionWakeStrength,
                0f,
                3f);
            obstructionWakeReach = Mathf.Clamp(
                obstructionWakeReach,
                0.25f,
                3f);
            obstructionWakeSpread = Mathf.Clamp(
                obstructionWakeSpread,
                0.5f,
                2f);
            obstructionWakeVariation = Mathf.Clamp01(
                obstructionWakeVariation);
            footprintPadding = Mathf.Clamp(footprintPadding, 0f, 2f);
        }

        private void EnsureFeatureModel()
        {
            if (featureModelInitialized)
            {
                return;
            }

            switch (responseProfile)
            {
                case GeneratedRiverResponseProfile.Subtle:
                    SetMigratedCustomValues(
                        0f,
                        2.2f,
                        0.6f,
                        0.90f,
                        0.75f,
                        0.85f);
                    break;

                case GeneratedRiverResponseProfile.Standard:
                    SetMigratedCustomValues(
                        0.65f,
                        2.8f,
                        1f,
                        1.50f,
                        1f,
                        1f);
                    break;

                case GeneratedRiverResponseProfile.Strong:
                    SetMigratedCustomValues(
                        1f,
                        3.2f,
                        1.35f,
                        2.25f,
                        1.35f,
                        1.25f);
                    break;

                case GeneratedRiverResponseProfile.Custom:
                    SetMigratedCustomValues(
                        Mathf.Clamp01(pressureStrength),
                        Mathf.Lerp(
                            0.5f,
                            4f,
                            Mathf.Clamp01(responseStiffness * 0.5f)),
                        Mathf.Clamp(unsteadiness, 0f, 2f),
                        Mathf.Clamp(
                            strengthMultiplier * surfaceDetail,
                            0f,
                            3f),
                        Mathf.Clamp(wakeLength, 0.25f, 3f),
                        Mathf.Lerp(
                            0.5f,
                            2f,
                            Mathf.Clamp01(geometryAmplitude * 0.5f)));
                    break;

                default:
                    staticPressureMode = GeneratedRiverFeatureMode.Inherit;
                    obstructionWakeMode = GeneratedRiverFeatureMode.Inherit;
                    break;
            }

            featureModelInitialized = true;
        }

        private void SetMigratedCustomValues(
            float pressure,
            float contactSharpness,
            float waveResponse,
            float wakeStrength,
            float wakeReach,
            float wakeSpread)
        {
            staticPressureMode = GeneratedRiverFeatureMode.Custom;
            staticPressureStrength = pressure;
            staticPressureContactSharpness = contactSharpness;
            staticPressureWaveResponse = waveResponse;
            staticPressureProfileChangeIntervalMin =
                DefaultProfileChangeIntervalMin;
            staticPressureProfileChangeIntervalMax =
                DefaultProfileChangeIntervalMax;
            obstructionWakeMode = GeneratedRiverFeatureMode.Custom;
            obstructionWakeStrength = wakeStrength;
            obstructionWakeReach = wakeReach;
            obstructionWakeSpread = wakeSpread;
            obstructionWakeVariation = 0.35f;
        }
    }

    public readonly struct ResolvedGeneratedRiverInteraction
    {
        public ResolvedGeneratedRiverInteraction(
            bool staticPressureEnabled,
            float staticPressureStrength,
            float staticPressureContactSharpness,
            float staticPressureProfileVariation,
            float staticPressureProfileChangeIntervalMin,
            float staticPressureProfileChangeIntervalMax,
            bool obstructionWakeEnabled,
            float obstructionWakeStrength,
            float obstructionWakeReach,
            float obstructionWakeSpread,
            float obstructionWakeVariation)
        {
            StaticPressureEnabled = staticPressureEnabled;
            StaticPressureStrength = Mathf.Clamp01(staticPressureStrength);
            StaticPressureContactSharpness = Mathf.Clamp(
                staticPressureContactSharpness,
                0.5f,
                4f);
            StaticPressureProfileVariation = Mathf.Clamp(
                staticPressureProfileVariation,
                0f,
                2f);

            float clampedIntervalMin = Mathf.Clamp(
                staticPressureProfileChangeIntervalMin,
                0.5f,
                3f);
            float clampedIntervalMax = Mathf.Clamp(
                staticPressureProfileChangeIntervalMax,
                0.5f,
                3f);
            StaticPressureProfileChangeIntervalMin = Mathf.Min(
                clampedIntervalMin,
                clampedIntervalMax);
            StaticPressureProfileChangeIntervalMax = Mathf.Max(
                clampedIntervalMin,
                clampedIntervalMax);

            ObstructionWakeEnabled = obstructionWakeEnabled;
            ObstructionWakeStrength = Mathf.Clamp(
                obstructionWakeStrength,
                0f,
                3f);
            ObstructionWakeReach = Mathf.Clamp(
                obstructionWakeReach,
                0.25f,
                3f);
            ObstructionWakeSpread = Mathf.Clamp(
                obstructionWakeSpread,
                0.5f,
                2f);
            ObstructionWakeVariation = Mathf.Clamp01(
                obstructionWakeVariation);
            compatibilityFootprintPadding = 0.12f;
        }

        public ResolvedGeneratedRiverInteraction(
            bool staticPressureEnabled,
            float staticPressureStrength,
            float staticPressureContactSharpness,
            float staticPressureProfileVariation,
            float staticPressureProfileChangeIntervalMin,
            float staticPressureProfileChangeIntervalMax,
            bool obstructionWakeEnabled,
            float obstructionWakeStrength,
            float obstructionWakeReach,
            float obstructionWakeSpread)
            : this(
                staticPressureEnabled,
                staticPressureStrength,
                staticPressureContactSharpness,
                staticPressureProfileVariation,
                staticPressureProfileChangeIntervalMin,
                staticPressureProfileChangeIntervalMax,
                obstructionWakeEnabled,
                obstructionWakeStrength,
                obstructionWakeReach,
                obstructionWakeSpread,
                0.35f)
        {
        }

        // Former per-feature constructor retained for source compatibility.
        public ResolvedGeneratedRiverInteraction(
            bool staticPressureEnabled,
            float staticPressureStrength,
            float staticPressureContactSharpness,
            float staticPressureWaveResponse,
            bool obstructionWakeEnabled,
            float obstructionWakeStrength,
            float obstructionWakeReach,
            float obstructionWakeSpread)
            : this(
                staticPressureEnabled,
                staticPressureStrength,
                staticPressureContactSharpness,
                staticPressureWaveResponse,
                0.75f,
                1.75f,
                obstructionWakeEnabled,
                obstructionWakeStrength,
                obstructionWakeReach,
                obstructionWakeSpread)
        {
        }

        // Former constructor retained so unrelated callers are not broken by
        // the per-feature authorship migration.
        public ResolvedGeneratedRiverInteraction(
            float pressureStrength,
            float strengthMultiplier,
            float geometryAmplitude,
            float surfaceDetail,
            float responseStiffness,
            float wakeLength,
            float unsteadiness,
            float footprintPadding)
        {
            StaticPressureEnabled = true;
            StaticPressureStrength = Mathf.Clamp01(pressureStrength);
            StaticPressureContactSharpness = Mathf.Lerp(
                0.5f,
                4f,
                Mathf.Clamp01(responseStiffness * 0.5f));
            StaticPressureProfileVariation = Mathf.Clamp(
                unsteadiness,
                0f,
                2f);
            StaticPressureProfileChangeIntervalMin = 0.75f;
            StaticPressureProfileChangeIntervalMax = 1.75f;
            ObstructionWakeEnabled = true;
            ObstructionWakeStrength = Mathf.Clamp(
                strengthMultiplier * surfaceDetail,
                0f,
                3f);
            ObstructionWakeReach = Mathf.Clamp(
                wakeLength,
                0.25f,
                3f);
            ObstructionWakeSpread = Mathf.Lerp(
                0.5f,
                2f,
                Mathf.Clamp01(geometryAmplitude * 0.5f));
            ObstructionWakeVariation = 0.35f;
            compatibilityFootprintPadding = Mathf.Max(
                0f,
                footprintPadding);
        }

        public bool StaticPressureEnabled { get; }
        public float StaticPressureStrength { get; }
        public float StaticPressureContactSharpness { get; }
        public float StaticPressureProfileVariation { get; }
        public float StaticPressureProfileChangeIntervalMin { get; }
        public float StaticPressureProfileChangeIntervalMax { get; }

        // Compatibility alias for integrations compiled against the previous
        // wave-triggered profile implementation.
        public float StaticPressureWaveResponse =>
            StaticPressureProfileVariation;

        public bool ObstructionWakeEnabled { get; }
        public float ObstructionWakeStrength { get; }
        public float ObstructionWakeReach { get; }
        public float ObstructionWakeSpread { get; }
        public float ObstructionWakeVariation { get; }

        // Former property names retained as non-authoritative aliases.
        public float PressureStrength => StaticPressureStrength;
        public float StrengthMultiplier => ObstructionWakeStrength;
        public float GeometryAmplitude => 1f;
        public float SurfaceDetail => ObstructionWakeStrength;
        public float ResponseStiffness => StaticPressureContactSharpness;
        public float WakeLength => ObstructionWakeReach;
        public float Unsteadiness => StaticPressureWaveResponse;
        public float FootprintPadding => compatibilityFootprintPadding;
        private readonly float compatibilityFootprintPadding;
    }

    /// <summary>
    /// Optional generated-object contract for object-specific river authorship.
    /// Geometry sources that do not implement it inherit the detected river's
    /// static-pressure and obstruction-wake defaults.
    /// </summary>
    public interface IGeneratedRiverInteractionSource
    {
        GeneratedRiverInteractionSettings RiverInteractionSettings { get; }
    }
}
