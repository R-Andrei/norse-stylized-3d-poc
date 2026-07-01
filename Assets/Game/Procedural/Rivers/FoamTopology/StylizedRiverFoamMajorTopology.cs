using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverFoamMajorPlacementRejectionReason
    {
        None,
        CandidateRejected,
        WidthCapacity,
        OutsideValidWater,
        BankClipping,
        ObstacleOverlap,
        Crowding,
        OpenWaterLimit,
        NoRasterCoverage
    }

    /// <summary>
    /// Stable identity and cheap future-evolution metadata for one accepted
    /// whole-river Major Support region. The current topology uses only the
    /// static transform;
    /// the retained normalized selectors defer physical timing and speed
    /// decisions to their later gated runtime-evolution step.
    /// </summary>
    public readonly struct StylizedRiverFoamMajorRegion
    {
        public StylizedRiverFoamMajorRegion(
            uint stableId,
            int longitudinalOpportunityIndex,
            int lateralOpportunityIndex,
            int activationOrder,
            float activationRank,
            int candidateSeed,
            float centreGlobalDistance,
            float centreAcrossNormalized,
            float orientationRadians,
            float metresPerCandidateCell,
            uint evolutionSeed,
            float driftRateSelector,
            float phaseOffset,
            float fadeInSelector,
            float fadeOutSelector,
            float movementSpanSelector,
            float recycleSelector,
            float anchoringStrength)
        {
            StableId = stableId;
            LongitudinalOpportunityIndex = longitudinalOpportunityIndex;
            LateralOpportunityIndex = lateralOpportunityIndex;
            ActivationOrder = activationOrder;
            ActivationRank = activationRank;
            CandidateSeed = candidateSeed;
            CentreGlobalDistance = centreGlobalDistance;
            CentreAcrossNormalized = centreAcrossNormalized;
            OrientationRadians = orientationRadians;
            MetresPerCandidateCell = metresPerCandidateCell;
            EvolutionSeed = evolutionSeed;
            DriftRateSelector = Mathf.Clamp01(driftRateSelector);
            PhaseOffset = Mathf.Repeat(phaseOffset, 1f);
            FadeInSelector = Mathf.Clamp01(fadeInSelector);
            FadeOutSelector = Mathf.Clamp01(fadeOutSelector);
            MovementSpanSelector = Mathf.Clamp01(movementSpanSelector);
            RecycleSelector = Mathf.Clamp01(recycleSelector);
            AnchoringStrength = Mathf.Clamp01(anchoringStrength);
        }

        public uint StableId { get; }
        public int LongitudinalOpportunityIndex { get; }
        public int LateralOpportunityIndex { get; }
        public int ActivationOrder { get; }
        public float ActivationRank { get; }
        public int CandidateSeed { get; }
        public float CentreGlobalDistance { get; }
        public float CentreAcrossNormalized { get; }
        public float OrientationRadians { get; }
        public float MetresPerCandidateCell { get; }
        public uint EvolutionSeed { get; }
        public float DriftRateSelector { get; }
        public float PhaseOffset { get; }
        public float FadeInSelector { get; }
        public float FadeOutSelector { get; }
        public float MovementSpanSelector { get; }
        public float RecycleSelector { get; }
        public float AnchoringStrength { get; }
    }

    /// <summary>
    /// Immutable output of one deterministic whole-river Major Support build.
    /// It contains the uploaded scalar field, accepted stable identities, and
    /// only the concise telemetry required by the topology implementation plan.
    /// </summary>
    public sealed class StylizedRiverFoamMajorTopology
    {
        private readonly float[] support;
        private readonly StylizedRiverFoamMajorRegion[] regions;
        private readonly int[] rejectionCounts;

        internal StylizedRiverFoamMajorTopology(
            int width,
            int height,
            int attemptedOpportunityCount,
            int acceptedRegionCount,
            int rejectedRegionCount,
            int validCellCount,
            int coveredCellCount,
            double generationMilliseconds,
            float[] support,
            StylizedRiverFoamMajorRegion[] regions,
            int[] rejectionCounts)
        {
            Width = Mathf.Max(0, width);
            Height = Mathf.Max(0, height);
            AttemptedOpportunityCount = Mathf.Max(
                0,
                attemptedOpportunityCount);
            AcceptedRegionCount = Mathf.Max(0, acceptedRegionCount);
            RejectedRegionCount = Mathf.Max(0, rejectedRegionCount);
            ValidCellCount = Mathf.Max(0, validCellCount);
            CoveredCellCount = Mathf.Max(0, coveredCellCount);
            GenerationMilliseconds = Math.Max(0.0, generationMilliseconds);
            this.support = support ?? Array.Empty<float>();
            this.regions = regions ?? Array.Empty<StylizedRiverFoamMajorRegion>();
            this.rejectionCounts = rejectionCounts ??
                new int[Enum.GetValues(
                    typeof(StylizedRiverFoamMajorPlacementRejectionReason)).Length];
        }

        public int Width { get; }
        public int Height { get; }
        public int AttemptedOpportunityCount { get; }
        public int AcceptedRegionCount { get; }
        public int RejectedRegionCount { get; }
        public int ValidCellCount { get; }
        public int CoveredCellCount { get; }
        public double GenerationMilliseconds { get; }
        public float Coverage => ValidCellCount > 0
            ? CoveredCellCount / (float)ValidCellCount
            : 0f;
        public IReadOnlyList<StylizedRiverFoamMajorRegion> Regions => regions;

        // The accepted scalar field remains immutable by convention and is
        // exposed only inside the topology assembly so later prepared layers
        // can derive relationships without reconstructing Major Support.
        internal float[] SupportData => support;

        public int GetRejectionCount(
            StylizedRiverFoamMajorPlacementRejectionReason reason)
        {
            int index = (int)reason;
            return index >= 0 && index < rejectionCounts.Length
                ? rejectionCounts[index]
                : 0;
        }

        public string GetTopRejectionSummary(int maximumReasons = 3)
        {
            maximumReasons = Mathf.Clamp(maximumReasons, 1, 8);
            List<(StylizedRiverFoamMajorPlacementRejectionReason reason,
                int count)> ranked = new();

            for (int index = 1; index < rejectionCounts.Length; index++)
            {
                int count = rejectionCounts[index];
                if (count <= 0)
                {
                    continue;
                }

                ranked.Add((
                    (StylizedRiverFoamMajorPlacementRejectionReason)index,
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

        internal void FillUploadPixels(Color[] destination)
        {
            int cellCount = Width * Height;
            if (destination == null || destination.Length < cellCount)
            {
                throw new ArgumentException(
                    $"Major topology upload requires at least {cellCount} pixels.",
                    nameof(destination));
            }

            for (int index = 0; index < cellCount; index++)
            {
                destination[index] = new Color(
                    Mathf.Clamp01(support[index]),
                    0f,
                    0f,
                    0f);
            }
        }
    }
}
