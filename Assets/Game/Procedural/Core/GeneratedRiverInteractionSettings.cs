using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry
{
    public enum GeneratedRiverInteractionParticipation
    {
        Automatic,
        Disabled
    }

    public enum GeneratedRiverResponseProfile
    {
        InheritRiver,
        Subtle,
        Standard,
        Strong,
        Custom
    }
    

    [Serializable]
    public sealed class GeneratedRiverInteractionSettings
    {
        [Tooltip("Controls whether this generated solid may be discovered automatically as a static river obstruction.")]
        [SerializeField]
        private GeneratedRiverInteractionParticipation participation =
            GeneratedRiverInteractionParticipation.Automatic;

        [Tooltip("Selects an artistic response profile. Inherit River uses the automatic river baseline without an object-specific preset.")]
        [SerializeField]
        private GeneratedRiverResponseProfile responseProfile =
            GeneratedRiverResponseProfile.InheritRiver;

        [Tooltip("For Custom, selects where static pressure sits between the computed minimum and maximum feasible heights. Zero uses the lower bound; one uses the upper bound.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float pressureStrength = 0.55f;

        [Tooltip("Overall artistic multiplier for wake-energy and other non-pressure response. Static pressure height is selected by Pressure Strength.")]
        [Range(0f, 4f)]
        [SerializeField]
        private float strengthMultiplier = 1f;

        [Tooltip("Additional geometric-response multiplier retained for non-pressure and future dynamic interaction paths. Static generated pressure uses Pressure Strength.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float geometryAmplitude = 1f;

        [Tooltip("Multiplier for downstream wake-energy injection and local surface detail. It does not paint a permanent trail.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float surfaceDetail = 1f;

        [Tooltip("How firmly the cached pressure ridge remains readable while Stage 3 waves pass through it.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float responseStiffness = 1f;

        [Tooltip("Multiplier for wake-energy persistence and expected downstream reach. It does not stretch a permanent trail shape.")]
        [Range(0.25f, 3f)]
        [SerializeField]
        private float wakeLength = 1f;

        [Tooltip("Controls wave-triggered variation along the static pressure ridge and variation in transient wake-pulse timing and strength.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float unsteadiness = 1f;

        [Tooltip("Minimum outward expansion beyond the detected waterline footprint, in metres. The river may raise this to a resolution-safe minimum.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float footprintPadding = 0.12f;

        public GeneratedRiverInteractionParticipation Participation =>
            participation;
        public GeneratedRiverResponseProfile ResponseProfile =>
            responseProfile;
        public float FootprintPadding => Mathf.Max(0f, footprintPadding);

        public ResolvedGeneratedRiverInteraction Resolve()
        {
            return responseProfile switch
            {
                GeneratedRiverResponseProfile.Subtle =>
                    new ResolvedGeneratedRiverInteraction(
                        0.25f,
                        0.65f,
                        0.65f,
                        0.75f,
                        0.72f,
                        0.82f,
                        0.68f,
                        FootprintPadding),

                GeneratedRiverResponseProfile.Standard =>
                    new ResolvedGeneratedRiverInteraction(
                        0.55f,
                        1f,
                        1f,
                        1f,
                        1f,
                        1f,
                        1f,
                        FootprintPadding),

                GeneratedRiverResponseProfile.Strong =>
                    new ResolvedGeneratedRiverInteraction(
                        0.85f,
                        1.35f,
                        1.40f,
                        1.30f,
                        1.35f,
                        1.18f,
                        1.20f,
                        FootprintPadding),

                GeneratedRiverResponseProfile.Custom =>
                    new ResolvedGeneratedRiverInteraction(
                        Mathf.Clamp01(pressureStrength),
                        Mathf.Max(0f, strengthMultiplier),
                        Mathf.Clamp(geometryAmplitude, 0f, 2f),
                        Mathf.Clamp(surfaceDetail, 0f, 2f),
                        Mathf.Clamp(responseStiffness, 0f, 2f),
                        Mathf.Clamp(wakeLength, 0.25f, 3f),
                        Mathf.Clamp(unsteadiness, 0f, 2f),
                        FootprintPadding),

                _ =>
                    new ResolvedGeneratedRiverInteraction(
                        0.50f,
                        1f,
                        1f,
                        1f,
                        1f,
                        1f,
                        1f,
                        FootprintPadding)
            };
        }

        public void Validate()
        {
            pressureStrength = Mathf.Clamp01(pressureStrength);
            strengthMultiplier = Mathf.Clamp(strengthMultiplier, 0f, 4f);
            geometryAmplitude = Mathf.Clamp(geometryAmplitude, 0f, 2f);
            surfaceDetail = Mathf.Clamp(surfaceDetail, 0f, 2f);
            responseStiffness = Mathf.Clamp(responseStiffness, 0f, 2f);
            wakeLength = Mathf.Clamp(wakeLength, 0.25f, 3f);
            unsteadiness = Mathf.Clamp(unsteadiness, 0f, 2f);
            footprintPadding = Mathf.Clamp(footprintPadding, 0f, 2f);
        }
    }

    public readonly struct ResolvedGeneratedRiverInteraction
    {
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
            PressureStrength = Mathf.Clamp01(pressureStrength);
            StrengthMultiplier = strengthMultiplier;
            GeometryAmplitude = geometryAmplitude;
            SurfaceDetail = surfaceDetail;
            ResponseStiffness = responseStiffness;
            WakeLength = wakeLength;
            Unsteadiness = unsteadiness;
            FootprintPadding = footprintPadding;
        }

        public float PressureStrength { get; }
        public float StrengthMultiplier { get; }
        public float GeometryAmplitude { get; }
        public float SurfaceDetail { get; }
        public float ResponseStiffness { get; }
        public float WakeLength { get; }
        public float Unsteadiness { get; }
        public float FootprintPadding { get; }
    }

    /// <summary>
    /// Optional generated-object contract for object-specific river authorship.
    /// Geometry sources that do not implement it use the neutral automatic
    /// river baseline.
    /// </summary>
    public interface IGeneratedRiverInteractionSource
    {
        GeneratedRiverInteractionSettings RiverInteractionSettings { get; }
    }
}
