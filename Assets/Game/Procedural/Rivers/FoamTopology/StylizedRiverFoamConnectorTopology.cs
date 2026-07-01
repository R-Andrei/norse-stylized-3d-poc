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
    /// Immutable prepared metric polyline for one accepted Connector
    /// relationship. The Connector generator already computes this path for
    /// rasterization; retaining it allows later topology slices and runtime
    /// evolution to remain associated with the exact accepted relationship
    /// without reconstructing or pathfinding again.
    /// </summary>
    public sealed class StylizedRiverFoamConnectorPath
    {
        private readonly Vector2[] metricPoints;

        internal StylizedRiverFoamConnectorPath(
            uint stableId,
            Vector2[] metricPoints)
        {
            StableId = stableId;
            this.metricPoints = metricPoints != null
                ? (Vector2[])metricPoints.Clone()
                : Array.Empty<Vector2>();
        }

        public uint StableId { get; }
        public IReadOnlyList<Vector2> MetricPoints => metricPoints;

        internal Vector2[] MetricPointData => metricPoints;
    }

    /// <summary>
    /// Immutable result of one deterministic prepared Connector Support build.
    /// The result contains only the accepted scalar field, stable relationship
    /// metadata, and the concise diagnostics required by the topology plan.
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
        }

        public int Width { get; }
        public int Height { get; }
        public int EligibleEndpointCount { get; }
        public int PathAttemptCount { get; }
        public int AcceptedConnectorCount { get; }
        public int CoveredCellCount { get; }
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
