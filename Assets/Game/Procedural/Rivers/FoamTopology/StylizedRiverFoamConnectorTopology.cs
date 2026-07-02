using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverFoamConnectorRejectionReason
    {
        None,
        TooClose,
        TooFar,
        SearchExhausted,
        PathTooLong,
        CrossesOtherMajor,
        InsufficientUnsupportedSpan,
        ExistingConnectorOverlap,
        NoRasterCoverage
    }

    internal enum StylizedRiverFoamConnectorPathVariantAvailability
    {
        Available,
        EndpointOwnershipUnavailable,
        InvalidEndpointGate,
        OutsideValidWater,
        ObstacleOverlap,
        ExcessiveStretch,
        NoUsablePolyline
    }

    /// <summary>
    /// Stable relationship identity and cheap future-evolution metadata for one
    /// accepted Connector Support path. Patch 3 uses only the static path field;
    /// normalized selectors intentionally defer physical timing decisions to the
    /// later runtime-evolution gate.
    /// </summary>
    public readonly struct StylizedRiverFoamConnectorRelationship
    {
        public StylizedRiverFoamConnectorRelationship(
            uint stableId,
            uint startComponentId,
            uint endComponentId,
            uint startEndpointId,
            uint endEndpointId,
            float startGlobalDistance,
            float startAcrossNormalized,
            float endGlobalDistance,
            float endAcrossNormalized,
            float pathLengthMetres,
            uint evolutionSeed,
            float driftRateSelector,
            float phaseOffset,
            float fadeInSelector,
            float fadeOutSelector,
            float movementSpanSelector,
            float recycleSelector,
            float fragilitySelector)
        {
            StableId = stableId;
            StartComponentId = startComponentId;
            EndComponentId = endComponentId;
            StartEndpointId = startEndpointId;
            EndEndpointId = endEndpointId;
            StartGlobalDistance = startGlobalDistance;
            StartAcrossNormalized = Mathf.Clamp(startAcrossNormalized, -1f, 1f);
            EndGlobalDistance = endGlobalDistance;
            EndAcrossNormalized = Mathf.Clamp(endAcrossNormalized, -1f, 1f);
            PathLengthMetres = Mathf.Max(0f, pathLengthMetres);
            EvolutionSeed = evolutionSeed;
            DriftRateSelector = Mathf.Clamp01(driftRateSelector);
            PhaseOffset = Mathf.Repeat(phaseOffset, 1f);
            FadeInSelector = Mathf.Clamp01(fadeInSelector);
            FadeOutSelector = Mathf.Clamp01(fadeOutSelector);
            MovementSpanSelector = Mathf.Clamp01(movementSpanSelector);
            RecycleSelector = Mathf.Clamp01(recycleSelector);
            FragilitySelector = Mathf.Clamp01(fragilitySelector);
        }

        public uint StableId { get; }
        public uint StartComponentId { get; }
        public uint EndComponentId { get; }
        public uint StartEndpointId { get; }
        public uint EndEndpointId { get; }
        public float StartGlobalDistance { get; }
        public float StartAcrossNormalized { get; }
        public float EndGlobalDistance { get; }
        public float EndAcrossNormalized { get; }
        public float PathLengthMetres { get; }
        public uint EvolutionSeed { get; }
        public float DriftRateSelector { get; }
        public float PhaseOffset { get; }
        public float FadeInSelector { get; }
        public float FadeOutSelector { get; }
        public float MovementSpanSelector { get; }
        public float RecycleSelector { get; }
        public float FragilitySelector { get; }
    }

    /// <summary>
    /// Preparation-time binding between one accepted Connector gate and the
    /// individual Major slot whose mask contributes most strongly at that gate.
    /// The local offset is expressed in the Major candidate's principal-axis
    /// cell frame so later endpoint movement never needs a gameplay host search.
    /// </summary>
    internal readonly struct StylizedRiverFoamConnectorEndpointBinding
    {
        public StylizedRiverFoamConnectorEndpointBinding(
            bool isAvailable,
            uint majorStableId,
            int majorPreparedIndex,
            Vector2 majorLocalOffsetCells,
            Vector2 acceptedMetricPosition,
            float acceptedAcrossNormalized,
            float ownershipSupport)
        {
            IsAvailable = isAvailable && majorPreparedIndex >= 0;
            MajorStableId = IsAvailable ? majorStableId : 0u;
            MajorPreparedIndex = IsAvailable ? majorPreparedIndex : -1;
            MajorLocalOffsetCells = IsAvailable
                ? majorLocalOffsetCells
                : Vector2.zero;
            AcceptedMetricPosition = acceptedMetricPosition;
            AcceptedAcrossNormalized = Mathf.Clamp(
                acceptedAcrossNormalized,
                -1f,
                1f);
            OwnershipSupport = Mathf.Clamp01(ownershipSupport);
        }

        public bool IsAvailable { get; }
        public uint MajorStableId { get; }
        public int MajorPreparedIndex { get; }
        public Vector2 MajorLocalOffsetCells { get; }
        public Vector2 AcceptedMetricPosition { get; }
        public float AcceptedAcrossNormalized { get; }
        public float OwnershipSupport { get; }
    }

    /// <summary>
    /// One strictly bounded preparation-time Connector alternative associated
    /// with a source/destination Major recycle-anchor combination. Unavailable
    /// alternatives are retained with a reason so later runtime code can choose
    /// temporary absence without pathfinding, retries, or hidden fallbacks.
    /// Anchor index -1 means the accepted identity placement for that endpoint.
    /// </summary>
    internal sealed class StylizedRiverFoamConnectorPathVariant
    {
        private readonly Vector2[] metricPoints;
        private readonly float[] normalizedCumulativeLength;

        public StylizedRiverFoamConnectorPathVariant(
            int startAnchorIndex,
            int endAnchorIndex,
            StylizedRiverFoamConnectorPathVariantAvailability availability,
            Vector2[] metricPoints,
            float[] normalizedCumulativeLength)
        {
            StartAnchorIndex = startAnchorIndex;
            EndAnchorIndex = endAnchorIndex;
            Availability = availability;
            this.metricPoints = availability ==
                    StylizedRiverFoamConnectorPathVariantAvailability.Available &&
                metricPoints != null
                    ? (Vector2[])metricPoints.Clone()
                    : Array.Empty<Vector2>();
            this.normalizedCumulativeLength = availability ==
                    StylizedRiverFoamConnectorPathVariantAvailability.Available &&
                normalizedCumulativeLength != null
                    ? (float[])normalizedCumulativeLength.Clone()
                    : Array.Empty<float>();
        }

        public int StartAnchorIndex { get; }
        public int EndAnchorIndex { get; }
        public StylizedRiverFoamConnectorPathVariantAvailability Availability
            { get; }
        public bool IsAvailable => Availability ==
            StylizedRiverFoamConnectorPathVariantAvailability.Available;
        internal Vector2[] MetricPointData => metricPoints;
        internal float[] NormalizedCumulativeLengthData =>
            normalizedCumulativeLength;
    }

    /// <summary>
    /// Immutable prepared metric polyline for one accepted Connector
    /// relationship. The original accepted points remain authoritative for the
    /// current static raster. Patch 4.7C.1 additionally retains a bounded runtime
    /// polyline, normalized cumulative arc length, individual endpoint ownership,
    /// and bounded recycle-anchor alternatives without enabling movement yet.
    /// </summary>
    public sealed class StylizedRiverFoamConnectorPath
    {
        private readonly Vector2[] metricPoints;
        private readonly Vector2[] preparedMetricPoints;
        private readonly float[] normalizedCumulativeLength;
        private readonly StylizedRiverFoamConnectorPathVariant[] pathVariants;

        internal StylizedRiverFoamConnectorPath(
            uint stableId,
            Vector2[] metricPoints,
            Vector2[] preparedMetricPoints,
            float[] normalizedCumulativeLength,
            StylizedRiverFoamConnectorEndpointBinding startEndpointBinding,
            StylizedRiverFoamConnectorEndpointBinding endEndpointBinding,
            StylizedRiverFoamConnectorPathVariant[] pathVariants)
        {
            StableId = stableId;
            this.metricPoints = metricPoints != null
                ? (Vector2[])metricPoints.Clone()
                : Array.Empty<Vector2>();
            this.preparedMetricPoints = preparedMetricPoints != null
                ? (Vector2[])preparedMetricPoints.Clone()
                : Array.Empty<Vector2>();
            this.normalizedCumulativeLength = normalizedCumulativeLength != null
                ? (float[])normalizedCumulativeLength.Clone()
                : Array.Empty<float>();
            StartEndpointBinding = startEndpointBinding;
            EndEndpointBinding = endEndpointBinding;
            this.pathVariants = pathVariants != null
                ? (StylizedRiverFoamConnectorPathVariant[])pathVariants.Clone()
                : Array.Empty<StylizedRiverFoamConnectorPathVariant>();
        }

        public uint StableId { get; }
        public IReadOnlyList<Vector2> MetricPoints => metricPoints;
        internal StylizedRiverFoamConnectorEndpointBinding StartEndpointBinding
            { get; }
        internal StylizedRiverFoamConnectorEndpointBinding EndEndpointBinding
            { get; }
        internal bool PreparationAvailable =>
            StartEndpointBinding.IsAvailable &&
            EndEndpointBinding.IsAvailable &&
            preparedMetricPoints.Length >= 2 &&
            normalizedCumulativeLength.Length == preparedMetricPoints.Length;
        internal Vector2[] MetricPointData => metricPoints;
        internal Vector2[] PreparedMetricPointData => preparedMetricPoints;
        internal float[] NormalizedCumulativeLengthData =>
            normalizedCumulativeLength;
        internal IReadOnlyList<StylizedRiverFoamConnectorPathVariant>
            PathVariants => pathVariants;
    }

    /// <summary>
    /// Immutable result of one deterministic prepared Connector Support build.
    /// The result contains the accepted scalar field, stable relationship
    /// metadata, and concise preparation diagnostics. Patch 4.7C.1 does not
    /// change the authoritative static raster or enable Connector movement.
    /// </summary>
    public sealed class StylizedRiverFoamConnectorTopology
    {
        private readonly float[] support;
        private readonly StylizedRiverFoamConnectorRelationship[] relationships;
        private readonly StylizedRiverFoamConnectorPath[] paths;
        private readonly int[] rejectionCounts;

        internal StylizedRiverFoamConnectorTopology(
            int width,
            int height,
            int eligibleEndpointCount,
            int pathAttemptCount,
            int acceptedConnectorCount,
            int coveredCellCount,
            double generationMilliseconds,
            float[] support,
            StylizedRiverFoamConnectorRelationship[] relationships,
            StylizedRiverFoamConnectorPath[] paths,
            int[] rejectionCounts)
        {
            Width = Mathf.Max(0, width);
            Height = Mathf.Max(0, height);
            EligibleEndpointCount = Mathf.Max(0, eligibleEndpointCount);
            PathAttemptCount = Mathf.Max(0, pathAttemptCount);
            AcceptedConnectorCount = Mathf.Max(0, acceptedConnectorCount);
            CoveredCellCount = Mathf.Max(0, coveredCellCount);
            GenerationMilliseconds = Math.Max(0.0, generationMilliseconds);
            this.support = support ?? Array.Empty<float>();
            this.relationships = relationships ??
                Array.Empty<StylizedRiverFoamConnectorRelationship>();
            this.paths = paths ??
                Array.Empty<StylizedRiverFoamConnectorPath>();
            this.rejectionCounts = rejectionCounts ??
                new int[Enum.GetValues(
                    typeof(StylizedRiverFoamConnectorRejectionReason)).Length];

            int preparedConnectorCount = 0;
            int preparedEndpointCount = 0;
            int preparedPathPointCount = 0;
            int availableVariantCount = 0;
            int unavailableVariantCount = 0;
            for (int pathIndex = 0; pathIndex < this.paths.Length; pathIndex++)
            {
                StylizedRiverFoamConnectorPath path = this.paths[pathIndex];
                if (path == null)
                {
                    continue;
                }

                if (path.StartEndpointBinding.IsAvailable)
                {
                    preparedEndpointCount++;
                }
                if (path.EndEndpointBinding.IsAvailable)
                {
                    preparedEndpointCount++;
                }
                if (path.PreparationAvailable)
                {
                    preparedConnectorCount++;
                    preparedPathPointCount +=
                        path.PreparedMetricPointData.Length;
                }

                IReadOnlyList<StylizedRiverFoamConnectorPathVariant> variants =
                    path.PathVariants;
                for (int variantIndex = 0;
                     variantIndex < variants.Count;
                     variantIndex++)
                {
                    if (variants[variantIndex].IsAvailable)
                    {
                        availableVariantCount++;
                    }
                    else
                    {
                        unavailableVariantCount++;
                    }
                }
            }

            PreparedConnectorCount = preparedConnectorCount;
            PreparedEndpointCount = preparedEndpointCount;
            PreparedPathPointCount = preparedPathPointCount;
            PreparedPathVariantCount = availableVariantCount;
            UnavailablePathVariantCount = unavailableVariantCount;
        }

        public int Width { get; }
        public int Height { get; }
        public int EligibleEndpointCount { get; }
        public int PathAttemptCount { get; }
        public int AcceptedConnectorCount { get; }
        public int CoveredCellCount { get; }
        public int PreparedConnectorCount { get; }
        public int UnavailableConnectorCount => Mathf.Max(
            0,
            AcceptedConnectorCount - PreparedConnectorCount);
        public int PreparedEndpointCount { get; }
        public int UnresolvedEndpointCount => Mathf.Max(
            0,
            AcceptedConnectorCount * 2 - PreparedEndpointCount);
        public int PreparedPathPointCount { get; }
        public int PreparedPathVariantCount { get; }
        public int UnavailablePathVariantCount { get; }
        public double GenerationMilliseconds { get; }
        public IReadOnlyList<StylizedRiverFoamConnectorRelationship>
            Relationships => relationships;
        public IReadOnlyList<StylizedRiverFoamConnectorPath> Paths => paths;

        // The accepted scalar field and path geometry remain immutable by
        // convention and are exposed only inside prepared topology assembly.
        // Weak Span generation consumes the retained accepted paths rather than
        // inferring relationship geometry from the flattened support field.
        internal IReadOnlyList<StylizedRiverFoamConnectorPath> PreparedPaths =>
            paths;

        // The accepted scalar field remains immutable by convention and is
        // exposed only inside prepared topology assembly so Pocket generation
        // can protect important Connector cores without reconstructing paths.
        internal float[] SupportData => support;

        public int GetRejectionCount(
            StylizedRiverFoamConnectorRejectionReason reason)
        {
            int index = (int)reason;
            return index >= 0 && index < rejectionCounts.Length
                ? rejectionCounts[index]
                : 0;
        }

        public string GetTopRejectionSummary(int maximumReasons = 1)
        {
            maximumReasons = Mathf.Clamp(maximumReasons, 1, 4);
            List<(StylizedRiverFoamConnectorRejectionReason reason,
                int count)> ranked = new();

            for (int index = 1; index < rejectionCounts.Length; index++)
            {
                int count = rejectionCounts[index];
                if (count <= 0)
                {
                    continue;
                }

                ranked.Add((
                    (StylizedRiverFoamConnectorRejectionReason)index,
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
                    $"Connector topology upload requires at least {cellCount} pixels.",
                    nameof(destination));
            }

            for (int index = 0; index < cellCount; index++)
            {
                Color value = destination[index];
                value.g = Mathf.Clamp01(support[index]);
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
