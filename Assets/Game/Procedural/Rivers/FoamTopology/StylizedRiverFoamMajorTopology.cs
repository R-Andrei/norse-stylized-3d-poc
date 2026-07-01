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
    /// Stable identity and cheap evolution metadata for one accepted
    /// whole-river Major Support region.
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
    /// One prevalidated local-territory placement used when an evolving Major recycles.
    /// Coordinates remain in the canonical local river domain.
    /// </summary>
    internal readonly struct StylizedRiverFoamMajorRecycleAnchor
    {
        public StylizedRiverFoamMajorRecycleAnchor(
            float centreLocalDistance,
            float centreAcrossNormalized,
            float orientationRadians,
            float metresPerCandidateCell,
            bool isFallback)
        {
            CentreLocalDistance = Mathf.Max(0f, centreLocalDistance);
            CentreAcrossNormalized = Mathf.Clamp(
                centreAcrossNormalized,
                -0.82f,
                0.82f);
            OrientationRadians = orientationRadians;
            MetresPerCandidateCell = Mathf.Max(
                0.0001f,
                metresPerCandidateCell);
            IsFallback = isFallback;
        }

        public float CentreLocalDistance { get; }
        public float CentreAcrossNormalized { get; }
        public float OrientationRadians { get; }
        public float MetresPerCandidateCell { get; }
        public bool IsFallback { get; }
    }

    /// <summary>
    /// Immutable local mask and geometry retained for the cheap Patch 4.6
    /// movement/morph path. It is prepared once and never regenerated during
    /// ordinary gameplay.
    /// </summary>
    internal sealed class StylizedRiverFoamPreparedMajorRegion
    {
        private readonly float[] localSupport;
        private readonly StylizedRiverFoamMajorRecycleAnchor[] recycleAnchors;

        public StylizedRiverFoamPreparedMajorRegion(
            uint stableId,
            int maskResolution,
            float[] localSupport,
            Vector2 centroidCells,
            float principalAngleRadians,
            float majorHalfExtentCells,
            float minorHalfExtentCells,
            StylizedRiverFoamMajorRecycleAnchor[] recycleAnchors)
        {
            StableId = stableId;
            MaskResolution = Mathf.Max(1, maskResolution);
            this.localSupport = localSupport ?? Array.Empty<float>();
            CentroidCells = centroidCells;
            PrincipalAngleRadians = principalAngleRadians;
            MajorHalfExtentCells = Mathf.Max(0.5f, majorHalfExtentCells);
            MinorHalfExtentCells = Mathf.Max(0.5f, minorHalfExtentCells);
            this.recycleAnchors = recycleAnchors ??
                Array.Empty<StylizedRiverFoamMajorRecycleAnchor>();
        }

        public uint StableId { get; }
        public int MaskResolution { get; }
        public Vector2 CentroidCells { get; }
        public float PrincipalAngleRadians { get; }
        public float MajorHalfExtentCells { get; }
        public float MinorHalfExtentCells { get; }
        public IReadOnlyList<StylizedRiverFoamMajorRecycleAnchor>
            RecycleAnchors => recycleAnchors;
        internal float[] LocalSupportData => localSupport;
    }

    /// <summary>
    /// Immutable output of one deterministic whole-river Major Support build.
    /// It contains the accepted scalar field, stable identities, and prepared
    /// local data required by the low-cost runtime evolution proof.
    /// </summary>
    public sealed class StylizedRiverFoamMajorTopology
    {
        internal const float EvolutionEgressFraction = 0.10f;
        internal const float EvolutionMinimumEgressMetres = 0.75f;
        internal const float EvolutionMaximumEgressMetres = 3.0f;

        internal static float ResolveEvolutionEgressStart(
            float validFieldLength)
        {
            validFieldLength = Mathf.Max(0f, validFieldLength);
            float egressSpan = Mathf.Clamp(
                validFieldLength * EvolutionEgressFraction,
                EvolutionMinimumEgressMetres,
                EvolutionMaximumEgressMetres);
            egressSpan = Mathf.Min(
                egressSpan,
                validFieldLength * 0.35f);
            return Mathf.Max(
                validFieldLength * 0.55f,
                validFieldLength - egressSpan);
        }

        private readonly float[] support;
        private readonly StylizedRiverFoamMajorRegion[] regions;
        private readonly StylizedRiverFoamPreparedMajorRegion[] preparedRegions;
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
            StylizedRiverFoamPreparedMajorRegion[] preparedRegions,
            int preparedRecycleAnchorCount,
            int recycleFallbackCount,
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
            PreparedRecycleAnchorCount = Mathf.Max(
                0,
                preparedRecycleAnchorCount);
            RecycleFallbackCount = Mathf.Max(0, recycleFallbackCount);
            this.support = support ?? Array.Empty<float>();
            this.regions = regions ?? Array.Empty<StylizedRiverFoamMajorRegion>();
            this.preparedRegions = preparedRegions ??
                Array.Empty<StylizedRiverFoamPreparedMajorRegion>();
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
        public int PreparedRecycleAnchorCount { get; }
        public int RecycleFallbackCount { get; }
        public double GenerationMilliseconds { get; }
        public float Coverage => ValidCellCount > 0
            ? CoveredCellCount / (float)ValidCellCount
            : 0f;
        public IReadOnlyList<StylizedRiverFoamMajorRegion> Regions => regions;

        internal float[] SupportData => support;
        internal IReadOnlyList<StylizedRiverFoamPreparedMajorRegion>
            PreparedRegions => preparedRegions;

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
