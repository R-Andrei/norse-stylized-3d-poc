using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverFoamPocketRejectionReason
    {
        None,
        HostTooNarrow,
        Spacing,
        NoRasterCoverage,
        NoUsableBoundary,
        NoEdgeBreach,
        HostWouldCollapse,
        ConnectorTooShort,
        NoUsableConnectorSpan,
        ConnectorSpanSpacing,
        NoUsableFreeWater,
        FreeWaterSpacing
    }

    /// <summary>
    /// The implemented Negative Aging Pressure classes. The legacy Pocket type
    /// name remains the low-level aggregate container for compatibility. Patch
    /// 4.7A retains Major-hosted classes separately; Patch 4.7B/4.7B.1 retains
    /// independently evolving Free-Water masks while Connector Weak Spans stay
    /// on the accepted static field until Patch 4.7C.2.
    /// </summary>
    public enum StylizedRiverFoamNegativeRegionClass
    {
        InteriorPocket,
        EdgeCavity,
        ConnectorWeakSpan,
        FreeWaterNegativeEvent
    }

    /// <summary>
    /// Stable host identity and cheap future-evolution metadata for one accepted
    /// Negative Aging Pressure region. Major-hosted classes use a Major region
    /// identity; Connector Weak Spans use their accepted Connector identity;
    /// Free-Water Negative Events use no positive host. Patch 4.7A evolves the
    /// two Major-hosted classes through their host frame; Patch 4.7B/4.7B.1
    /// evolves Free-Water Events independently, while Weak Spans remain static
    /// until their Connector-owned runtime slice. Edge cavities additionally
    /// retain their deliberate breach direction.
    /// </summary>
    public readonly struct StylizedRiverFoamPocketRegion
    {
        public StylizedRiverFoamPocketRegion(
            StylizedRiverFoamNegativeRegionClass regionClass,
            uint stableId,
            uint hostRegionId,
            float centreGlobalDistance,
            float centreAcrossNormalized,
            float orientationRadians,
            float alongRadiusMetres,
            float acrossRadiusMetres,
            Vector2 breachDirection,
            uint evolutionSeed,
            float driftRateSelector,
            float phaseOffset,
            float fadeInSelector,
            float fadeOutSelector,
            float movementSpanSelector,
            float recycleSelector,
            float growthSelector)
        {
            RegionClass = regionClass;
            StableId = stableId;
            HostRegionId = hostRegionId;
            CentreGlobalDistance = centreGlobalDistance;
            CentreAcrossNormalized = Mathf.Clamp(
                centreAcrossNormalized,
                -1f,
                1f);
            OrientationRadians = orientationRadians;
            AlongRadiusMetres = Mathf.Max(0f, alongRadiusMetres);
            AcrossRadiusMetres = Mathf.Max(0f, acrossRadiusMetres);
            BreachDirection = breachDirection.sqrMagnitude > 0.000001f
                ? breachDirection.normalized
                : Vector2.zero;
            EvolutionSeed = evolutionSeed;
            DriftRateSelector = Mathf.Clamp01(driftRateSelector);
            PhaseOffset = Mathf.Repeat(phaseOffset, 1f);
            FadeInSelector = Mathf.Clamp01(fadeInSelector);
            FadeOutSelector = Mathf.Clamp01(fadeOutSelector);
            MovementSpanSelector = Mathf.Clamp01(movementSpanSelector);
            RecycleSelector = Mathf.Clamp01(recycleSelector);
            GrowthSelector = Mathf.Clamp01(growthSelector);
        }

        public StylizedRiverFoamNegativeRegionClass RegionClass { get; }
        public uint StableId { get; }
        public uint HostRegionId { get; }
        public float CentreGlobalDistance { get; }
        public float CentreAcrossNormalized { get; }
        public float OrientationRadians { get; }
        public float AlongRadiusMetres { get; }
        public float AcrossRadiusMetres { get; }
        public Vector2 BreachDirection { get; }
        public uint EvolutionSeed { get; }
        public float DriftRateSelector { get; }
        public float PhaseOffset { get; }
        public float FadeInSelector { get; }
        public float FadeOutSelector { get; }
        public float MovementSpanSelector { get; }
        public float RecycleSelector { get; }
        public float GrowthSelector { get; }
    }


    /// <summary>
    /// One preparation-time validated local transform for a Major-hosted
    /// negative mask. Runtime chooses only from these bounded states, avoiding
    /// gameplay containment or boundary searches.
    /// </summary>
    internal readonly struct StylizedRiverFoamHostedNegativeVariant
    {
        public StylizedRiverFoamHostedNegativeVariant(
            Vector2 offsetCells,
            float rotationRadians,
            float scaleAlong,
            float scaleAcross)
        {
            OffsetCells = offsetCells;
            RotationRadians = rotationRadians;
            ScaleAlong = Mathf.Max(0.05f, scaleAlong);
            ScaleAcross = Mathf.Max(0.05f, scaleAcross);
        }

        public Vector2 OffsetCells { get; }
        public float RotationRadians { get; }
        public float ScaleAlong { get; }
        public float ScaleAcross { get; }
    }

    /// <summary>
    /// One accepted Major-hosted negative mask retained in the host Major's
    /// prepared candidate coordinate frame. Ordinary runtime evolution samples
    /// this immutable mask through the host transform instead of searching for
    /// or regenerating a valid Pocket/Cavity placement during gameplay.
    /// </summary>
    internal sealed class StylizedRiverFoamPreparedHostedNegativeRegion
    {
        private readonly float[] localPressure;
        private readonly StylizedRiverFoamHostedNegativeVariant[] variants;

        public StylizedRiverFoamPreparedHostedNegativeRegion(
            StylizedRiverFoamNegativeRegionClass regionClass,
            uint stableId,
            uint hostRegionId,
            int hostPreparedIndex,
            int maskResolution,
            float[] localPressure,
            Vector2 centreCandidateCells,
            StylizedRiverFoamHostedNegativeVariant[] variants)
        {
            RegionClass = regionClass;
            StableId = stableId;
            HostRegionId = hostRegionId;
            HostPreparedIndex = Mathf.Max(0, hostPreparedIndex);
            MaskResolution = Mathf.Max(1, maskResolution);
            this.localPressure = localPressure ?? Array.Empty<float>();
            CentreCandidateCells = centreCandidateCells;
            this.variants = variants ??
                Array.Empty<StylizedRiverFoamHostedNegativeVariant>();
        }

        public StylizedRiverFoamNegativeRegionClass RegionClass { get; }
        public uint StableId { get; }
        public uint HostRegionId { get; }
        public int HostPreparedIndex { get; }
        public int MaskResolution { get; }
        public Vector2 CentreCandidateCells { get; }
        public IReadOnlyList<StylizedRiverFoamHostedNegativeVariant>
            Variants => variants;
        internal float[] LocalPressureData => localPressure;
    }

    /// <summary>
    /// One preparation-time validated upstream placement used when an
    /// independently evolving Free-Water Negative Event reaches its finite
    /// lifetime or downstream egress. Runtime selects only from these bounded
    /// anchors and never searches for a replacement placement during play.
    /// </summary>
    internal readonly struct StylizedRiverFoamFreeWaterRecycleAnchor
    {
        public StylizedRiverFoamFreeWaterRecycleAnchor(
            float centreLocalDistance,
            float centreAcrossNormalized,
            float orientationRadians,
            bool isFallback)
        {
            CentreLocalDistance = Mathf.Max(0f, centreLocalDistance);
            CentreAcrossNormalized = Mathf.Clamp(
                centreAcrossNormalized,
                -0.88f,
                0.88f);
            OrientationRadians = orientationRadians;
            IsFallback = isFallback;
        }

        public float CentreLocalDistance { get; }
        public float CentreAcrossNormalized { get; }
        public float OrientationRadians { get; }
        public bool IsFallback { get; }
    }

    /// <summary>
    /// One immutable Free-Water Negative Event mask retained in its own local
    /// metric frame. Runtime evolution samples this mask through a slow bounded
    /// single-instance pose and instantly recycles it through a preparation-time
    /// validated upstream anchor at lifetime or downstream egress.
    /// </summary>
    internal sealed class StylizedRiverFoamPreparedFreeWaterRegion
    {
        private readonly float[] localPressure;
        private readonly StylizedRiverFoamFreeWaterRecycleAnchor[]
            recycleAnchors;

        public StylizedRiverFoamPreparedFreeWaterRegion(
            uint stableId,
            int maskResolution,
            float[] localPressure,
            float centreLocalDistance,
            float centreAcrossNormalized,
            float orientationRadians,
            float metresPerCell,
            StylizedRiverFoamFreeWaterRecycleAnchor[] recycleAnchors)
        {
            StableId = stableId;
            MaskResolution = Mathf.Max(1, maskResolution);
            this.localPressure = localPressure ?? Array.Empty<float>();
            CentreLocalDistance = Mathf.Max(0f, centreLocalDistance);
            CentreAcrossNormalized = Mathf.Clamp(
                centreAcrossNormalized,
                -1f,
                1f);
            OrientationRadians = orientationRadians;
            MetresPerCell = Mathf.Max(0.0001f, metresPerCell);
            this.recycleAnchors = recycleAnchors ??
                Array.Empty<StylizedRiverFoamFreeWaterRecycleAnchor>();
        }

        public uint StableId { get; }
        public int MaskResolution { get; }
        public float CentreLocalDistance { get; }
        public float CentreAcrossNormalized { get; }
        public float OrientationRadians { get; }
        public float MetresPerCell { get; }
        public IReadOnlyList<StylizedRiverFoamFreeWaterRecycleAnchor>
            RecycleAnchors => recycleAnchors;
        internal float[] LocalPressureData => localPressure;
    }

    /// <summary>
    /// Immutable result of one deterministic prepared Negative Aging Pressure
    /// build. The aggregate field remains independent from
    /// positive support and therefore never destructively edits Major or
    /// Connector topology.
    /// </summary>
    public sealed class StylizedRiverFoamPocketTopology
    {
        private readonly float[] pressure;
        private readonly float[] hostedPressure;
        private readonly float[] hostedFallbackPressure;
        private readonly float[] independentPressure;
        private readonly float[] staticIndependentPressure;
        private readonly float[] freeWaterPressure;
        private readonly StylizedRiverFoamPocketRegion[] regions;
        private readonly StylizedRiverFoamPreparedHostedNegativeRegion[]
            preparedHostedRegions;
        private readonly StylizedRiverFoamPreparedFreeWaterRegion[]
            preparedFreeWaterRegions;
        private readonly int[] rejectionCounts;

        internal StylizedRiverFoamPocketTopology(
            int width,
            int height,
            int interiorEligibleHostCount,
            int interiorCandidateCount,
            int acceptedInteriorPocketCount,
            int cavityEligibleHostCount,
            int cavityCandidateCount,
            int acceptedEdgeCavityCount,
            int weakSpanEligibleConnectorCount,
            int weakSpanCandidateCount,
            int acceptedConnectorWeakSpanCount,
            int freeWaterOpportunityCount,
            int freeWaterCandidateCount,
            int acceptedFreeWaterEventCount,
            int coveredCellCount,
            double generationMilliseconds,
            float[] pressure,
            float[] hostedPressure,
            float[] hostedFallbackPressure,
            float[] independentPressure,
            float[] staticIndependentPressure,
            float[] freeWaterPressure,
            StylizedRiverFoamPocketRegion[] regions,
            StylizedRiverFoamPreparedHostedNegativeRegion[]
                preparedHostedRegions,
            StylizedRiverFoamPreparedFreeWaterRegion[]
                preparedFreeWaterRegions,
            int[] rejectionCounts)
        {
            Width = Mathf.Max(0, width);
            Height = Mathf.Max(0, height);
            InteriorEligibleHostCount = Mathf.Max(
                0,
                interiorEligibleHostCount);
            InteriorCandidateCount = Mathf.Max(0, interiorCandidateCount);
            AcceptedInteriorPocketCount = Mathf.Max(
                0,
                acceptedInteriorPocketCount);
            CavityEligibleHostCount = Mathf.Max(0, cavityEligibleHostCount);
            CavityCandidateCount = Mathf.Max(0, cavityCandidateCount);
            AcceptedEdgeCavityCount = Mathf.Max(
                0,
                acceptedEdgeCavityCount);
            WeakSpanEligibleConnectorCount = Mathf.Max(
                0,
                weakSpanEligibleConnectorCount);
            WeakSpanCandidateCount = Mathf.Max(0, weakSpanCandidateCount);
            AcceptedConnectorWeakSpanCount = Mathf.Max(
                0,
                acceptedConnectorWeakSpanCount);
            FreeWaterOpportunityCount = Mathf.Max(
                0,
                freeWaterOpportunityCount);
            FreeWaterCandidateCount = Mathf.Max(0, freeWaterCandidateCount);
            AcceptedFreeWaterEventCount = Mathf.Max(
                0,
                acceptedFreeWaterEventCount);
            CoveredCellCount = Mathf.Max(0, coveredCellCount);
            GenerationMilliseconds = Math.Max(0.0, generationMilliseconds);
            this.pressure = pressure ?? Array.Empty<float>();
            this.hostedPressure = hostedPressure ?? Array.Empty<float>();
            this.hostedFallbackPressure = hostedFallbackPressure ??
                Array.Empty<float>();
            this.independentPressure = independentPressure ?? Array.Empty<float>();
            this.staticIndependentPressure =
                staticIndependentPressure ?? Array.Empty<float>();
            this.freeWaterPressure = freeWaterPressure ?? Array.Empty<float>();
            this.regions = regions ??
                Array.Empty<StylizedRiverFoamPocketRegion>();
            this.preparedHostedRegions = preparedHostedRegions ??
                Array.Empty<StylizedRiverFoamPreparedHostedNegativeRegion>();
            this.preparedFreeWaterRegions = preparedFreeWaterRegions ??
                Array.Empty<StylizedRiverFoamPreparedFreeWaterRegion>();
            this.rejectionCounts = rejectionCounts ??
                new int[Enum.GetValues(
                    typeof(StylizedRiverFoamPocketRejectionReason)).Length];
        }

        public int Width { get; }
        public int Height { get; }
        public int InteriorEligibleHostCount { get; }
        public int InteriorCandidateCount { get; }
        public int AcceptedInteriorPocketCount { get; }
        public int CavityEligibleHostCount { get; }
        public int CavityCandidateCount { get; }
        public int AcceptedEdgeCavityCount { get; }
        public int WeakSpanEligibleConnectorCount { get; }
        public int WeakSpanCandidateCount { get; }
        public int AcceptedConnectorWeakSpanCount { get; }
        public int FreeWaterOpportunityCount { get; }
        public int FreeWaterCandidateCount { get; }
        public int AcceptedFreeWaterEventCount { get; }
        public int AcceptedHostedRegionCount =>
            AcceptedInteriorPocketCount + AcceptedEdgeCavityCount;
        public int PreparedHostedRegionCount => preparedHostedRegions.Length;
        public int PreparedFreeWaterRegionCount =>
            preparedFreeWaterRegions.Length;
        public int PreparedFreeWaterRecycleAnchorCount
        {
            get
            {
                int count = 0;
                for (int regionIndex = 0;
                     regionIndex < preparedFreeWaterRegions.Length;
                     regionIndex++)
                {
                    count += preparedFreeWaterRegions[regionIndex]
                        .RecycleAnchors.Count;
                }

                return count;
            }
        }
        public int FreeWaterRecycleFallbackCount
        {
            get
            {
                int count = 0;
                for (int regionIndex = 0;
                     regionIndex < preparedFreeWaterRegions.Length;
                     regionIndex++)
                {
                    IReadOnlyList<StylizedRiverFoamFreeWaterRecycleAnchor>
                        anchors = preparedFreeWaterRegions[regionIndex]
                            .RecycleAnchors;
                    for (int anchorIndex = 0;
                         anchorIndex < anchors.Count;
                         anchorIndex++)
                    {
                        if (anchors[anchorIndex].IsFallback)
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
        }
        public int HostedFallbackRegionCount => Mathf.Max(
            0,
            AcceptedHostedRegionCount - PreparedHostedRegionCount);
        public int CoveredCellCount { get; }
        public double GenerationMilliseconds { get; }

        // Compatibility totals retained for the existing concise runtime
        // telemetry while the implemented classes also remain inspectable
        // separately.
        public int EligibleHostCount =>
            InteriorEligibleHostCount + CavityEligibleHostCount +
            WeakSpanEligibleConnectorCount + FreeWaterOpportunityCount;
        public int CandidateCentreCount =>
            InteriorCandidateCount + CavityCandidateCount +
            WeakSpanCandidateCount + FreeWaterCandidateCount;
        public int AcceptedPocketCount =>
            AcceptedInteriorPocketCount + AcceptedEdgeCavityCount +
            AcceptedConnectorWeakSpanCount + AcceptedFreeWaterEventCount;

        public IReadOnlyList<StylizedRiverFoamPocketRegion> Regions => regions;

        internal float[] PressureData => pressure;
        internal float[] HostedPressureData => hostedPressure;
        internal float[] HostedFallbackPressureData => hostedFallbackPressure;
        internal float[] IndependentPressureData => independentPressure;
        internal float[] StaticIndependentPressureData =>
            staticIndependentPressure;
        internal float[] FreeWaterPressureData => freeWaterPressure;
        internal IReadOnlyList<StylizedRiverFoamPreparedHostedNegativeRegion>
            PreparedHostedRegions => preparedHostedRegions;
        internal IReadOnlyList<StylizedRiverFoamPreparedFreeWaterRegion>
            PreparedFreeWaterRegions => preparedFreeWaterRegions;

        public int GetRejectionCount(
            StylizedRiverFoamPocketRejectionReason reason)
        {
            int index = (int)reason;
            return index >= 0 && index < rejectionCounts.Length
                ? rejectionCounts[index]
                : 0;
        }

        public string GetTopRejectionSummary(int maximumReasons = 2)
        {
            maximumReasons = Mathf.Clamp(maximumReasons, 1, 6);
            List<(StylizedRiverFoamPocketRejectionReason reason,
                int count)> ranked = new();

            for (int index = 1; index < rejectionCounts.Length; index++)
            {
                int count = rejectionCounts[index];
                if (count <= 0)
                {
                    continue;
                }

                ranked.Add((
                    (StylizedRiverFoamPocketRejectionReason)index,
                    count));
            }

            ranked.Sort((a, b) =>
            {
                int countComparison = b.count.CompareTo(a.count);
                return countComparison != 0
                    ? countComparison
                    : ((int)a.reason).CompareTo((int)b.reason);
            });

            if (ranked.Count == 0)
            {
                return "None";
            }

            StringBuilder builder = new StringBuilder();
            int shown = Mathf.Min(maximumReasons, ranked.Count);
            for (int index = 0; index < shown; index++)
            {
                if (index > 0)
                {
                    builder.Append(" · ");
                }

                builder.Append(Nicify(ranked[index].reason.ToString()));
                builder.Append(' ');
                builder.Append(ranked[index].count);
            }

            return builder.ToString();
        }

        internal void AddToUploadPixels(
            Color[] destination,
            bool hostedEvolutionAvailable,
            bool freeWaterEvolutionAvailable)
        {
            int cellCount = Width * Height;
            if (destination == null || destination.Length < cellCount)
            {
                throw new ArgumentException(
                    $"Negative topology upload requires at least {cellCount} pixels.",
                    nameof(destination));
            }

            for (int index = 0; index < cellCount; index++)
            {
                Color value = destination[index];
                // Blue remains the static independent-negative channel. While
                // Free-Water evolution is active, Free-Water leaves this static
                // channel and is supplied by the evolving runtime field instead;
                // Connector Weak Spans remain static here.
                value.b = Mathf.Clamp01(
                    freeWaterEvolutionAvailable
                        ? staticIndependentPressure[index]
                        : independentPressure[index]);
                value.a = Mathf.Clamp01(
                    hostedEvolutionAvailable
                        ? hostedFallbackPressure[index]
                        : hostedPressure[index]);
                destination[index] = value;
            }
        }

        private static string Nicify(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length + 8);
            builder.Append(value[0]);
            for (int index = 1; index < value.Length; index++)
            {
                char current = value[index];
                char previous = value[index - 1];
                if (char.IsUpper(current) && !char.IsUpper(previous))
                {
                    builder.Append(' ');
                }
                builder.Append(current);
            }

            return builder.ToString();
        }
    }
}
