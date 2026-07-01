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
    /// name remains the low-level aggregate container for compatibility while
    /// all four static negative classes share one prepared aggregate field.
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
    /// Free-Water Negative Events use no positive host. Patch 4.4 still uses only
    /// the static aggregate field; normalized selectors defer physical timing,
    /// drift, growth, and respawn decisions to the later runtime-evolution gate.
    /// Edge cavities additionally retain their deliberate breach direction.
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
    /// Immutable result of one deterministic prepared Negative Aging Pressure
    /// build. The aggregate field remains independent from
    /// positive support and therefore never destructively edits Major or
    /// Connector topology.
    /// </summary>
    public sealed class StylizedRiverFoamPocketTopology
    {
        private readonly float[] pressure;
        private readonly StylizedRiverFoamPocketRegion[] regions;
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
            StylizedRiverFoamPocketRegion[] regions,
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
            this.regions = regions ??
                Array.Empty<StylizedRiverFoamPocketRegion>();
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

        internal void AddToUploadPixels(Color[] destination)
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
                value.b = Mathf.Clamp01(pressure[index]);
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
