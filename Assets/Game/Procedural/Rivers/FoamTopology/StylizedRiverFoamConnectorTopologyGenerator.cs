using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Deterministic CPU proof generator for sparse prepared Connector Support.
    /// Patch 3.5 connects disconnected Major components only, blocks
    /// Major clearance halos outside narrow endpoint gates, rejects excessive
    /// detours, softly favours wider component and longitudinal participation,
    /// and exposes bounded Amount,
    /// Directness, and Length Preference controls. It does
    /// not approximate the authoritative live Pressure, Lee, or Shore fields
    /// on the CPU, and performs no work during ordinary gameplay.
    /// </summary>
    public static class StylizedRiverFoamConnectorTopologyGenerator
    {
        private const float MajorComponentThreshold = 0.18f;
        private const float MajorRoutingInteriorThreshold = 0.42f;
        private const float MajorRoutingHaloThreshold = 0.08f;
        private const float MajorRasterSuppressionStart = 0.16f;
        private const float EndpointGateRadiusMetres = 0.46f;
        private const int MinimumComponentCellCount = 5;
        private const int EndpointSectorCount = 12;
        private const int MaximumEndpointsPerComponent = 10;
        private const int BaselineMaximumPairAttempts = 48;
        private const int BaselineMaximumComponentDegree = 2;
        private const float MinimumGapMetres = 0.28f;
        private const float MinimumUnsupportedSpanMetres = 0.24f;
        private const float ConnectorCoverageThreshold = 0.15f;
        private const int LongitudinalDistributionSectionCount = 6;
        private const float UnconnectedComponentScoreBonus = 0.045f;
        private const float RepeatedComponentScorePenalty = 0.040f;
        private const float LongitudinalSectionLoadPenalty = 0.020f;

        public static StylizedRiverFoamConnectorTopology Generate(
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            StylizedRiverQuality quality,
            float shoreMotion,
            int seed,
            float connectorAmount,
            float connectorDirectness,
            float connectorLengthPreference,
            float[] obstacleMask,
            StylizedRiverFoamMajorTopology majorTopology)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int cellCount = Mathf.Max(0, width * height);
            float[] connectorSupport = new float[cellCount];
            int reasonCount = Enum.GetValues(
                typeof(StylizedRiverFoamConnectorRejectionReason)).Length;
            int[] rejectionCounts = new int[reasonCount];

            if (domain == null || !domain.IsValid ||
                majorTopology == null ||
                width < 2 || height < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f ||
                majorTopology.Width != width || majorTopology.Height != height)
            {
                stopwatch.Stop();
                return CreateEmpty(
                    width,
                    height,
                    stopwatch.Elapsed.TotalMilliseconds,
                    connectorSupport,
                    rejectionCounts);
            }

            float[] majorSupport = majorTopology.SupportData;
            if (majorSupport == null || majorSupport.Length < cellCount)
            {
                stopwatch.Stop();
                return CreateEmpty(
                    width,
                    height,
                    stopwatch.Elapsed.TotalMilliseconds,
                    connectorSupport,
                    rejectionCounts);
            }

            connectorAmount = Mathf.Clamp01(connectorAmount);
            connectorDirectness = Mathf.Clamp01(connectorDirectness);
            connectorLengthPreference = Mathf.Clamp01(
                connectorLengthPreference);

            float[] fluidCoverage = new float[cellCount];
            bool[] validCells = new bool[cellCount];
            StylizedRiverFoamMajorTopologyGenerator.BuildFluidContext(
                domain,
                width,
                height,
                fieldLength,
                validFieldLength,
                quality,
                shoreMotion,
                obstacleMask,
                fluidCoverage,
                validCells);

            Vector2[] metricPositions = BuildMetricPositions(
                domain,
                width,
                height,
                fieldLength,
                validFieldLength);
            int[] componentLabels = new int[cellCount];
            for (int index = 0; index < componentLabels.Length; index++)
            {
                componentLabels[index] = -1;
            }

            List<MajorComponent> components = ExtractMajorComponents(
                width,
                height,
                majorSupport,
                validCells,
                metricPositions,
                componentLabels);
            AssignStableComponentIdentity(
                components,
                componentLabels,
                width,
                height,
                fieldLength,
                domain,
                majorTopology.Regions);

            List<ConnectorEndpoint> endpoints = ExtractEndpoints(
                components,
                width,
                height,
                metricPositions,
                majorSupport);
            List<ComponentPair> pairs = BuildCandidatePairs(
                components,
                domain,
                seed,
                connectorDirectness,
                connectorLengthPreference,
                rejectionCounts);

            int[] componentDegree = new int[components.Count];
            DisjointSet relationships = new DisjointSet(components.Count);
            List<StylizedRiverFoamConnectorRelationship>
                acceptedRelationships = new();
            List<StylizedRiverFoamConnectorPath> acceptedPaths = new();
            int pathAttemptCount = 0;
            int maximumPairAttempts = Mathf.Max(
                1,
                Mathf.RoundToInt(Mathf.Lerp(
                    BaselineMaximumPairAttempts - 36f,
                    BaselineMaximumPairAttempts + 36f,
                    connectorAmount)));
            int maximumComponentDegree = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Lerp(
                    BaselineMaximumComponentDegree - 1f,
                    BaselineMaximumComponentDegree + 1f,
                    connectorAmount)),
                1,
                3);
            int cycleBudget = connectorAmount > 0.5f
                ? Mathf.CeilToInt(
                    (connectorAmount - 0.5f) * 2f *
                    Mathf.Max(1f, components.Count * 0.35f))
                : 0;
            int maximumRelationshipCap = components.Count > 1
                ? Mathf.Max(
                    components.Count - 1,
                    components.Count - 1 + cycleBudget)
                : 0;
            int maximumAccepted = components.Count > 1
                ? Mathf.Clamp(
                    Mathf.CeilToInt(
                        components.Count * Mathf.Lerp(
                            0.10f,
                            1.06f,
                            connectorAmount)),
                    1,
                    maximumRelationshipCap)
                : 0;
            float maximumOverlapRatio = Mathf.Lerp(
                0.12f,
                0.56f,
                connectorAmount);
            int acceptedCycleCount = 0;
            bool[] evaluatedPairs = new bool[pairs.Count];
            int[] acceptedRelationshipsBySection =
                new int[LongitudinalDistributionSectionCount];

            float[] candidateRaster = new float[cellCount];
            while (pathAttemptCount < maximumPairAttempts &&
                   acceptedRelationships.Count < maximumAccepted)
            {
                int pairIndex = SelectNextPairIndex(
                    pairs,
                    evaluatedPairs,
                    componentDegree,
                    relationships,
                    acceptedCycleCount,
                    cycleBudget,
                    maximumComponentDegree,
                    acceptedRelationshipsBySection,
                    acceptedRelationships.Count,
                    validFieldLength);
                if (pairIndex < 0)
                {
                    break;
                }

                evaluatedPairs[pairIndex] = true;
                ComponentPair pair = pairs[pairIndex];
                bool createsCycle =
                    relationships.Find(pair.StartComponentIndex) ==
                    relationships.Find(pair.EndComponentIndex);

                pathAttemptCount++;
                if (!TryFindPath(
                        pair,
                        width,
                        height,
                        validCells,
                        componentLabels,
                        fluidCoverage,
                        majorSupport,
                        connectorSupport,
                        metricPositions,
                        connectorDirectness,
                        out List<int> path,
                        out Vector2 curvatureWaypoint,
                        out StylizedRiverFoamConnectorRejectionReason reason))
                {
                    rejectionCounts[(int)reason]++;
                    continue;
                }

                List<int> simplifiedPath = SimplifyPath(
                    path,
                    width,
                    height,
                    validCells,
                    componentLabels,
                    majorSupport,
                    pair.StartComponentIndex,
                    pair.EndComponentIndex,
                    pair.StartEndpoint.CellIndex,
                    pair.EndEndpoint.CellIndex,
                    connectorDirectness,
                    curvatureWaypoint,
                    metricPositions);
                float pathLength = MeasurePathLength(
                    simplifiedPath,
                    metricPositions);
                // Curved routes may be longer than the direct endpoint gap,
                // but a Connector must still read as a bridge rather than an
                // orbit around either Major region. The bend allowance remains
                // broad enough for the approved single-waypoint grammar and
                // sensible obstacle avoidance.
                float relativePathLimit = Mathf.Lerp(
                    1.85f,
                    1.65f,
                    connectorDirectness);
                float pathPadding = Mathf.Lerp(
                    0.58f,
                    0.38f,
                    connectorDirectness);
                if (pathLength >
                        pair.DistanceMetres * relativePathLimit + pathPadding ||
                    pathLength > pair.MaximumPathLengthMetres)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamConnectorRejectionReason.PathTooLong]++;
                    continue;
                }

                if (PathCrossesBlockedMajorHalo(
                        path,
                        validCells,
                        componentLabels,
                        majorSupport,
                        metricPositions,
                        pair.StartComponentIndex,
                        pair.EndComponentIndex,
                        pair.StartEndpoint.CellIndex,
                        pair.EndEndpoint.CellIndex))
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamConnectorRejectionReason
                            .CrossesOtherMajor]++;
                    continue;
                }

                float unsupportedSpan = MeasureUnsupportedSpan(
                    path,
                    majorSupport,
                    metricPositions);
                if (unsupportedSpan < MinimumUnsupportedSpanMetres)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamConnectorRejectionReason
                            .InsufficientUnsupportedSpan]++;
                    continue;
                }

                Array.Clear(candidateRaster, 0, candidateRaster.Length);
                uint relationshipId = ResolveRelationshipId(pair);
                RasterizeConnector(
                    simplifiedPath,
                    relationshipId,
                    width,
                    height,
                    validCells,
                    majorSupport,
                    metricPositions,
                    candidateRaster);

                int newCoverage = 0;
                int rasterCoverage = 0;
                int existingOverlap = 0;
                for (int index = 0; index < cellCount; index++)
                {
                    if (candidateRaster[index] < ConnectorCoverageThreshold)
                    {
                        continue;
                    }

                    rasterCoverage++;
                    if (connectorSupport[index] >= ConnectorCoverageThreshold)
                    {
                        existingOverlap++;
                    }
                    else
                    {
                        newCoverage++;
                    }
                }

                if (rasterCoverage == 0 || newCoverage < 2)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamConnectorRejectionReason
                            .NoRasterCoverage]++;
                    continue;
                }

                if (existingOverlap >
                    rasterCoverage * maximumOverlapRatio)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamConnectorRejectionReason
                            .ExistingConnectorOverlap]++;
                    continue;
                }

                for (int index = 0; index < cellCount; index++)
                {
                    connectorSupport[index] = Mathf.Max(
                        connectorSupport[index],
                        candidateRaster[index]);
                }

                componentDegree[pair.StartComponentIndex]++;
                componentDegree[pair.EndComponentIndex]++;
                if (createsCycle)
                {
                    acceptedCycleCount++;
                }
                else
                {
                    relationships.Union(
                        pair.StartComponentIndex,
                        pair.EndComponentIndex);
                }

                Vector2[] acceptedMetricPath = new Vector2[
                    simplifiedPath.Count];
                for (int pointIndex = 0;
                     pointIndex < simplifiedPath.Count;
                     pointIndex++)
                {
                    acceptedMetricPath[pointIndex] =
                        metricPositions[simplifiedPath[pointIndex]];
                }
                acceptedPaths.Add(new StylizedRiverFoamConnectorPath(
                    relationshipId,
                    acceptedMetricPath));

                uint evolutionSeed = MixBits(relationshipId ^ 0xD1B54A35u);
                int acceptedSection = ResolveLongitudinalSection(
                    pair,
                    validFieldLength,
                    acceptedRelationshipsBySection.Length);
                acceptedRelationshipsBySection[acceptedSection]++;

                acceptedRelationships.Add(
                    new StylizedRiverFoamConnectorRelationship(
                        relationshipId,
                        pair.StartComponentId,
                        pair.EndComponentId,
                        pair.StartEndpoint.StableId,
                        pair.EndEndpoint.StableId,
                        domain.GlobalDistanceMinimum +
                            pair.StartEndpoint.Position.x,
                        ResolveAcrossNormalized(
                            pair.StartEndpoint.CellIndex,
                            width,
                            height),
                        domain.GlobalDistanceMinimum +
                            pair.EndEndpoint.Position.x,
                        ResolveAcrossNormalized(
                            pair.EndEndpoint.CellIndex,
                            width,
                            height),
                        pathLength,
                        evolutionSeed,
                        Hash01(evolutionSeed, 40u),
                        Hash01(evolutionSeed, 41u),
                        Hash01(evolutionSeed, 42u),
                        Hash01(evolutionSeed, 43u),
                        Hash01(evolutionSeed, 44u),
                        Hash01(evolutionSeed, 45u),
                        Hash01(evolutionSeed, 46u)));
            }

            int coveredCellCount = 0;
            for (int index = 0; index < connectorSupport.Length; index++)
            {
                if (connectorSupport[index] >= ConnectorCoverageThreshold)
                {
                    coveredCellCount++;
                }
            }

            stopwatch.Stop();
            return new StylizedRiverFoamConnectorTopology(
                width,
                height,
                endpoints.Count,
                pathAttemptCount,
                acceptedRelationships.Count,
                coveredCellCount,
                stopwatch.Elapsed.TotalMilliseconds,
                connectorSupport,
                acceptedRelationships.ToArray(),
                acceptedPaths.ToArray(),
                rejectionCounts);
        }

        private static int SelectNextPairIndex(
            IReadOnlyList<ComponentPair> pairs,
            bool[] evaluatedPairs,
            int[] componentDegree,
            DisjointSet relationships,
            int acceptedCycleCount,
            int cycleBudget,
            int maximumComponentDegree,
            int[] acceptedRelationshipsBySection,
            int acceptedRelationshipCount,
            float validFieldLength)
        {
            int bestIndex = -1;
            float bestScore = float.PositiveInfinity;

            for (int index = 0; index < pairs.Count; index++)
            {
                if (evaluatedPairs[index])
                {
                    continue;
                }

                ComponentPair pair = pairs[index];
                int startDegree = componentDegree[
                    pair.StartComponentIndex];
                int endDegree = componentDegree[
                    pair.EndComponentIndex];
                if (startDegree >= maximumComponentDegree ||
                    endDegree >= maximumComponentDegree)
                {
                    continue;
                }

                bool createsCycle =
                    relationships.Find(pair.StartComponentIndex) ==
                    relationships.Find(pair.EndComponentIndex);
                if (createsCycle && acceptedCycleCount >= cycleBudget)
                {
                    continue;
                }

                // These are deliberately small ordering biases. The
                // relationship geometry remains the primary score, and a
                // strong repeated relationship may still outrank a weak first
                // connection elsewhere.
                float dynamicScore = pair.Score +
                    ResolveComponentDegreeScoreAdjustment(startDegree) +
                    ResolveComponentDegreeScoreAdjustment(endDegree) +
                    ResolveLongitudinalSectionScoreAdjustment(
                        pair,
                        validFieldLength,
                        acceptedRelationshipsBySection,
                        acceptedRelationshipCount);

                if (dynamicScore < bestScore - 0.000001f ||
                    (Mathf.Abs(dynamicScore - bestScore) <= 0.000001f &&
                     (bestIndex < 0 || index < bestIndex)))
                {
                    bestIndex = index;
                    bestScore = dynamicScore;
                }
            }

            return bestIndex;
        }

        private static float ResolveComponentDegreeScoreAdjustment(
            int degree)
        {
            if (degree <= 0)
            {
                return -UnconnectedComponentScoreBonus;
            }

            if (degree == 1)
            {
                return 0f;
            }

            return (degree - 1) * RepeatedComponentScorePenalty;
        }

        private static float ResolveLongitudinalSectionScoreAdjustment(
            ComponentPair pair,
            float validFieldLength,
            int[] acceptedRelationshipsBySection,
            int acceptedRelationshipCount)
        {
            if (acceptedRelationshipsBySection == null ||
                acceptedRelationshipsBySection.Length == 0 ||
                acceptedRelationshipCount <= 0)
            {
                return 0f;
            }

            int section = ResolveLongitudinalSection(
                pair,
                validFieldLength,
                acceptedRelationshipsBySection.Length);
            float averageLoad = acceptedRelationshipCount /
                (float)acceptedRelationshipsBySection.Length;
            float relativeLoad =
                acceptedRelationshipsBySection[section] - averageLoad;
            return relativeLoad * LongitudinalSectionLoadPenalty;
        }

        private static int ResolveLongitudinalSection(
            ComponentPair pair,
            float validFieldLength,
            int sectionCount)
        {
            if (sectionCount <= 1)
            {
                return 0;
            }

            float midpointDistance =
                (pair.StartEndpoint.Position.x +
                 pair.EndEndpoint.Position.x) * 0.5f;
            float normalizedDistance = Mathf.Clamp01(
                midpointDistance / Mathf.Max(0.0001f, validFieldLength));
            return Mathf.Min(
                sectionCount - 1,
                Mathf.FloorToInt(normalizedDistance * sectionCount));
        }

        private static StylizedRiverFoamConnectorTopology CreateEmpty(
            int width,
            int height,
            double milliseconds,
            float[] support,
            int[] rejectionCounts)
        {
            return new StylizedRiverFoamConnectorTopology(
                width,
                height,
                0,
                0,
                0,
                0,
                milliseconds,
                support,
                Array.Empty<StylizedRiverFoamConnectorRelationship>(),
                Array.Empty<StylizedRiverFoamConnectorPath>(),
                rejectionCounts);
        }

        private static Vector2[] BuildMetricPositions(
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength)
        {
            Vector2[] positions = new Vector2[width * height];
            for (int x = 0; x < width; x++)
            {
                float localDistance = x /
                    (float)Mathf.Max(1, width - 1) * fieldLength;
                float clampedDistance = Mathf.Min(
                    localDistance,
                    validFieldLength);
                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(clampedDistance);
                float leftSurface = Mathf.Max(
                    0.05f,
                    sample.LeftSurfaceHalfWidth);
                float rightSurface = Mathf.Max(
                    0.05f,
                    sample.RightSurfaceHalfWidth);

                for (int y = 0; y < height; y++)
                {
                    float across01 = y /
                        (float)Mathf.Max(1, height - 1);
                    positions[x + y * width] = new Vector2(
                        localDistance,
                        Across01ToMetres(
                            across01,
                            leftSurface,
                            rightSurface));
                }
            }

            return positions;
        }

        private static List<MajorComponent> ExtractMajorComponents(
            int width,
            int height,
            float[] majorSupport,
            bool[] validCells,
            Vector2[] metricPositions,
            int[] labels)
        {
            int cellCount = width * height;
            int[] queue = new int[cellCount];
            List<MajorComponent> components = new();

            for (int seedIndex = 0; seedIndex < cellCount; seedIndex++)
            {
                if (labels[seedIndex] != -1 ||
                    !validCells[seedIndex] ||
                    majorSupport[seedIndex] < MajorComponentThreshold)
                {
                    continue;
                }

                int head = 0;
                int tail = 0;
                queue[tail++] = seedIndex;
                labels[seedIndex] = -2;
                List<int> cells = new();
                Vector2 positionSum = Vector2.zero;

                while (head < tail)
                {
                    int index = queue[head++];
                    cells.Add(index);
                    positionSum += metricPositions[index];
                    int x = index % width;
                    int y = index / width;
                    TryQueue(x - 1, y);
                    TryQueue(x + 1, y);
                    TryQueue(x, y - 1);
                    TryQueue(x, y + 1);
                }

                if (cells.Count < MinimumComponentCellCount)
                {
                    for (int index = 0; index < cells.Count; index++)
                    {
                        labels[cells[index]] = -3;
                    }
                    continue;
                }

                int componentIndex = components.Count;
                for (int index = 0; index < cells.Count; index++)
                {
                    labels[cells[index]] = componentIndex;
                }

                components.Add(new MajorComponent(
                    cells,
                    positionSum / cells.Count));

                void TryQueue(int x, int y)
                {
                    if (x < 0 || x >= width || y < 0 || y >= height)
                    {
                        return;
                    }

                    int neighbour = x + y * width;
                    if (labels[neighbour] != -1 ||
                        !validCells[neighbour] ||
                        majorSupport[neighbour] < MajorComponentThreshold)
                    {
                        return;
                    }

                    labels[neighbour] = -2;
                    queue[tail++] = neighbour;
                }
            }

            return components;
        }

        private static void AssignStableComponentIdentity(
            List<MajorComponent> components,
            int[] componentLabels,
            int width,
            int height,
            float fieldLength,
            RiverDomainSnapshot domain,
            IReadOnlyList<StylizedRiverFoamMajorRegion> regions)
        {
            for (int regionIndex = 0; regionIndex < regions.Count; regionIndex++)
            {
                StylizedRiverFoamMajorRegion region = regions[regionIndex];
                float localDistance = region.CentreGlobalDistance -
                    domain.GlobalDistanceMinimum;
                int x = Mathf.Clamp(
                    Mathf.RoundToInt(
                        localDistance /
                        Mathf.Max(0.0001f, fieldLength) *
                        (width - 1)),
                    0,
                    width - 1);
                int y = Mathf.Clamp(
                    Mathf.RoundToInt(
                        (region.CentreAcrossNormalized * 0.5f + 0.5f) *
                        (height - 1)),
                    0,
                    height - 1);
                int componentIndex = FindNearestComponentLabel(
                    x,
                    y,
                    width,
                    height,
                    componentLabels,
                    10);
                if (componentIndex >= 0 && componentIndex < components.Count)
                {
                    components[componentIndex].SourceRegionIds.Add(
                        region.StableId);
                }
            }

            for (int index = 0; index < components.Count; index++)
            {
                MajorComponent component = components[index];
                component.SourceRegionIds.Sort();
                uint stableId = 0x9E3779B9u;
                if (component.SourceRegionIds.Count > 0)
                {
                    for (int sourceIndex = 0;
                         sourceIndex < component.SourceRegionIds.Count;
                         sourceIndex++)
                    {
                        stableId = MixBits(
                            stableId ^
                            component.SourceRegionIds[sourceIndex] ^
                            (uint)(sourceIndex + 1) * 0x85EBCA6Bu);
                    }
                }
                else
                {
                    int firstCell = component.Cells.Count > 0
                        ? component.Cells[0]
                        : index;
                    stableId = MixBits(
                        (uint)(firstCell + 1) ^
                        (uint)component.Cells.Count * 0xC2B2AE35u);
                }

                component.StableId = stableId;
            }
        }

        private static int FindNearestComponentLabel(
            int centreX,
            int centreY,
            int width,
            int height,
            int[] labels,
            int maximumRadius)
        {
            int direct = labels[centreX + centreY * width];
            if (direct >= 0)
            {
                return direct;
            }

            for (int radius = 1; radius <= maximumRadius; radius++)
            {
                for (int y = centreY - radius; y <= centreY + radius; y++)
                {
                    if (y < 0 || y >= height)
                    {
                        continue;
                    }

                    for (int x = centreX - radius;
                         x <= centreX + radius;
                         x++)
                    {
                        if (x < 0 || x >= width ||
                            (Mathf.Abs(x - centreX) != radius &&
                             Mathf.Abs(y - centreY) != radius))
                        {
                            continue;
                        }

                        int label = labels[x + y * width];
                        if (label >= 0)
                        {
                            return label;
                        }
                    }
                }
            }

            return -1;
        }

        private static List<ConnectorEndpoint> ExtractEndpoints(
            List<MajorComponent> components,
            int width,
            int height,
            Vector2[] metricPositions,
            float[] majorSupport)
        {
            List<ConnectorEndpoint> endpoints = new();
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                MajorComponent component = components[componentIndex];
                EndpointCandidate[] sectors =
                    new EndpointCandidate[EndpointSectorCount];

                for (int cellListIndex = 0;
                     cellListIndex < component.Cells.Count;
                     cellListIndex++)
                {
                    int cellIndex = component.Cells[cellListIndex];
                    if (!IsBoundaryCell(
                            cellIndex,
                            width,
                            height,
                            majorSupport))
                    {
                        continue;
                    }

                    Vector2 delta = metricPositions[cellIndex] -
                        component.Centroid;
                    float angle = Mathf.Atan2(delta.y, delta.x);
                    int sector = Mathf.Clamp(
                        Mathf.FloorToInt(
                            (angle + Mathf.PI) /
                            (Mathf.PI * 2f) * EndpointSectorCount),
                        0,
                        EndpointSectorCount - 1);
                    float score = delta.sqrMagnitude +
                        (1f - Mathf.Clamp01(majorSupport[cellIndex])) * 0.02f;
                    if (!sectors[sector].Valid ||
                        score > sectors[sector].Score)
                    {
                        sectors[sector] = new EndpointCandidate(
                            true,
                            cellIndex,
                            score,
                            sector);
                    }
                }

                List<EndpointCandidate> selected = new();
                for (int sector = 0; sector < sectors.Length; sector++)
                {
                    if (sectors[sector].Valid)
                    {
                        selected.Add(sectors[sector]);
                    }
                }

                selected.Sort((a, b) => b.Score.CompareTo(a.Score));
                int endpointCount = Mathf.Min(
                    MaximumEndpointsPerComponent,
                    selected.Count);
                HashSet<int> usedCells = new();
                for (int endpointIndex = 0;
                     endpointIndex < endpointCount;
                     endpointIndex++)
                {
                    EndpointCandidate candidate = selected[endpointIndex];
                    if (!usedCells.Add(candidate.CellIndex))
                    {
                        continue;
                    }

                    uint endpointId = MixBits(
                        component.StableId ^
                        (uint)(candidate.Sector + 1) * 0x27D4EB2Fu);
                    ConnectorEndpoint endpoint = new ConnectorEndpoint(
                        endpointId,
                        candidate.CellIndex,
                        metricPositions[candidate.CellIndex]);
                    component.Endpoints.Add(endpoint);
                    endpoints.Add(endpoint);
                }
            }

            return endpoints;
        }

        private static bool IsBoundaryCell(
            int cellIndex,
            int width,
            int height,
            float[] majorSupport)
        {
            if (majorSupport[cellIndex] < MajorComponentThreshold)
            {
                return false;
            }

            int x = cellIndex % width;
            int y = cellIndex / width;
            return x == 0 || x == width - 1 || y == 0 || y == height - 1 ||
                majorSupport[cellIndex - 1] < MajorComponentThreshold ||
                majorSupport[cellIndex + 1] < MajorComponentThreshold ||
                majorSupport[cellIndex - width] < MajorComponentThreshold ||
                majorSupport[cellIndex + width] < MajorComponentThreshold;
        }

        private static List<ComponentPair> BuildCandidatePairs(
            List<MajorComponent> components,
            RiverDomainSnapshot domain,
            int seed,
            float directness,
            float lengthPreference,
            int[] rejectionCounts)
        {
            int alternativesPerComponentPair = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Lerp(8f, 3f, directness)),
                3,
                8);
            List<ComponentPair> pairs = new();
            float averageVisibleWidth = ResolveAverageVisibleWidth(domain);
            float baselineMaximumGapMetres = Mathf.Clamp(
                averageVisibleWidth * 0.74f,
                1.55f,
                5.25f);

            // Length Preference ranks relationships inside one fixed safe
            // envelope. It does not change the absolute distance at which a
            // connection becomes unsafe or implausible.
            float maximumGapMetres = baselineMaximumGapMetres * 1.55f;
            float baselineMaximumPathLength = Mathf.Clamp(
                baselineMaximumGapMetres * 1.85f,
                2.5f,
                8.5f);
            float maximumPathLength = baselineMaximumPathLength * 1.55f;
            float centredLengthPreference =
                (Mathf.Clamp01(lengthPreference) - 0.5f) * 2f;
            float lengthPreferenceStrength = Mathf.Abs(
                centredLengthPreference);
            bool foundAnySeparatedComponents = false;

            for (int startIndex = 0;
                 startIndex < components.Count;
                 startIndex++)
            {
                MajorComponent start = components[startIndex];
                for (int endIndex = startIndex + 1;
                     endIndex < components.Count;
                     endIndex++)
                {
                    MajorComponent end = components[endIndex];
                    List<EndpointPairCandidate> endpointPairs =
                        ResolveEndpointPairs(
                            start,
                            end,
                            alternativesPerComponentPair,
                            directness,
                            seed);
                    if (endpointPairs.Count == 0)
                    {
                        continue;
                    }

                    foundAnySeparatedComponents = true;
                    float relationshipDistance = Vector2.Distance(
                        start.Centroid,
                        end.Centroid);
                    float normalizedRelationshipLength = Mathf.InverseLerp(
                        MinimumGapMetres,
                        maximumGapMetres,
                        relationshipDistance);
                    float lengthPreferenceScore =
                        -centredLengthPreference *
                        normalizedRelationshipLength *
                        Mathf.Lerp(
                            0f,
                            1.45f,
                            lengthPreferenceStrength);

                    for (int alternativeIndex = 0;
                         alternativeIndex < endpointPairs.Count;
                         alternativeIndex++)
                    {
                        EndpointPairCandidate endpointPair =
                            endpointPairs[alternativeIndex];
                        float distance = endpointPair.DistanceMetres;
                        if (distance < MinimumGapMetres)
                        {
                            rejectionCounts[(int)
                                StylizedRiverFoamConnectorRejectionReason
                                    .TooClose]++;
                            continue;
                        }

                        if (distance > maximumGapMetres)
                        {
                            continue;
                        }

                        float lateralDifference = Mathf.Abs(
                            endpointPair.Start.Position.y -
                            endpointPair.End.Position.y);
                        float longitudinalDifference = Mathf.Abs(
                            endpointPair.Start.Position.x -
                            endpointPair.End.Position.x);
                        uint stablePairSeed = MixBits(
                            start.StableId ^
                            RotateLeft(end.StableId, 13) ^
                            (uint)seed ^
                            (uint)(alternativeIndex + 1) * 0x9E3779B9u);
                        float deterministicTie = Hash01(
                            stablePairSeed,
                            12u);

                        // Endpoint selection is owned by Directness. The
                        // relationship-length bias is shared by all endpoint
                        // alternatives of the same component pair so Length
                        // Preference cannot silently undo that choice.
                        float score =
                            endpointPair.SelectionScore * 0.82f +
                            lateralDifference * 0.035f +
                            Mathf.Max(
                                0f,
                                lateralDifference -
                                longitudinalDifference * 2.0f) * 0.075f +
                            alternativeIndex * 0.012f +
                            deterministicTie * 0.008f +
                            lengthPreferenceScore;

                        pairs.Add(new ComponentPair(
                            startIndex,
                            endIndex,
                            start.StableId,
                            end.StableId,
                            endpointPair.Start,
                            endpointPair.End,
                            distance,
                            maximumPathLength,
                            score));
                    }
                }
            }

            if (pairs.Count == 0 && foundAnySeparatedComponents)
            {
                rejectionCounts[(int)
                    StylizedRiverFoamConnectorRejectionReason.TooFar]++;
            }

            pairs.Sort((a, b) =>
            {
                int scoreComparison = a.Score.CompareTo(b.Score);
                if (scoreComparison != 0)
                {
                    return scoreComparison;
                }

                int startComparison = a.StartComponentId.CompareTo(
                    b.StartComponentId);
                if (startComparison != 0)
                {
                    return startComparison;
                }

                int endComparison = a.EndComponentId.CompareTo(
                    b.EndComponentId);
                return endComparison != 0
                    ? endComparison
                    : a.StartEndpoint.StableId.CompareTo(
                        b.StartEndpoint.StableId);
            });
            return pairs;
        }

        private static List<EndpointPairCandidate>
            ResolveEndpointPairs(
                MajorComponent start,
                MajorComponent end,
                int maximumPairs,
                float directness,
                int seed)
        {
            List<EndpointPairCandidate> candidates = new();
            float minimumDistance = float.PositiveInfinity;
            float maximumDistance = 0f;

            for (int startIndex = 0;
                 startIndex < start.Endpoints.Count;
                 startIndex++)
            {
                ConnectorEndpoint a = start.Endpoints[startIndex];
                for (int endIndex = 0;
                     endIndex < end.Endpoints.Count;
                     endIndex++)
                {
                    ConnectorEndpoint b = end.Endpoints[endIndex];
                    float distance = Vector2.Distance(a.Position, b.Position);
                    minimumDistance = Mathf.Min(minimumDistance, distance);
                    maximumDistance = Mathf.Max(maximumDistance, distance);
                    uint choiceSeed = MixBits(
                        start.StableId ^
                        RotateLeft(end.StableId, 11) ^
                        RotateLeft(a.StableId, 19) ^
                        RotateLeft(b.StableId, 25) ^
                        (uint)seed);
                    candidates.Add(new EndpointPairCandidate(
                        a,
                        b,
                        distance,
                        Hash01(choiceSeed, 16u),
                        0f));
                }
            }

            float indirectness = 1f - Mathf.Clamp01(directness);
            uint componentPairSeed = MixBits(
                start.StableId ^
                RotateLeft(end.StableId, 17) ^
                (uint)seed ^
                0xA511E9B3u);
            float targetNormalizedDistance = indirectness * Mathf.Lerp(
                0.56f,
                0.84f,
                Hash01(componentPairSeed, 17u));
            float distanceRange = Mathf.Max(
                0.0001f,
                maximumDistance - minimumDistance);

            for (int index = 0; index < candidates.Count; index++)
            {
                EndpointPairCandidate candidate = candidates[index];
                float normalizedDistance = Mathf.Clamp01(
                    (candidate.DistanceMetres - minimumDistance) /
                    distanceRange);
                float targetError = Mathf.Abs(
                    normalizedDistance - targetNormalizedDistance);
                float selectionScore = targetError +
                    candidate.SelectionBias * Mathf.Lerp(
                        0.11f,
                        0.0005f,
                        directness);
                candidates[index] = new EndpointPairCandidate(
                    candidate.Start,
                    candidate.End,
                    candidate.DistanceMetres,
                    candidate.SelectionBias,
                    selectionScore);
            }

            candidates.Sort((a, b) =>
            {
                int selectionComparison = a.SelectionScore.CompareTo(
                    b.SelectionScore);
                if (selectionComparison != 0)
                {
                    return selectionComparison;
                }

                int distanceComparison = a.DistanceMetres.CompareTo(
                    b.DistanceMetres);
                if (distanceComparison != 0)
                {
                    return distanceComparison;
                }

                int startComparison = a.Start.StableId.CompareTo(
                    b.Start.StableId);
                return startComparison != 0
                    ? startComparison
                    : a.End.StableId.CompareTo(b.End.StableId);
            });

            if (candidates.Count <= maximumPairs)
            {
                return candidates;
            }

            return candidates.GetRange(0, maximumPairs);
        }

        private static bool TryFindPath(
            ComponentPair pair,
            int width,
            int height,
            bool[] validCells,
            int[] componentLabels,
            float[] fluidCoverage,
            float[] majorSupport,
            float[] connectorSupport,
            Vector2[] metricPositions,
            float directness,
            out List<int> path,
            out Vector2 curvatureWaypoint,
            out StylizedRiverFoamConnectorRejectionReason rejectionReason)
        {
            int start = pair.StartEndpoint.CellIndex;
            int goal = pair.EndEndpoint.CellIndex;
            Vector2 directStart = metricPositions[start];
            Vector2 directEnd = metricPositions[goal];
            curvatureWaypoint = (directStart + directEnd) * 0.5f;
            uint pathSeed = ResolveRelationshipId(pair);
            float indirectness = 1f - Mathf.Clamp01(directness);
            int baselineMaximumExpansions = Mathf.Clamp(
                width * height / 2,
                768,
                8192);
            int maximumExpansions = Mathf.Clamp(
                Mathf.RoundToInt(
                    baselineMaximumExpansions * Mathf.Lerp(
                        1f,
                        1.35f,
                        indirectness)),
                768,
                12288);

            if (indirectness <= 0.0001f)
            {
                bool foundDirect = TryFindPathSegment(
                    start,
                    goal,
                    directStart,
                    directEnd,
                    pathSeed,
                    maximumExpansions,
                    0.16f,
                    0.035f,
                    width,
                    height,
                    validCells,
                    componentLabels,
                    fluidCoverage,
                    majorSupport,
                    connectorSupport,
                    metricPositions,
                    pair.StartComponentIndex,
                    pair.EndComponentIndex,
                    start,
                    goal,
                    out path);
                rejectionReason = foundDirect
                    ? StylizedRiverFoamConnectorRejectionReason.None
                    : StylizedRiverFoamConnectorRejectionReason.SearchExhausted;
                return foundDirect;
            }

            float preferredSide = Hash01(pathSeed, 23u) < 0.5f
                ? -1f
                : 1f;
            for (int sideAttempt = 0; sideAttempt < 2; sideAttempt++)
            {
                float side = sideAttempt == 0
                    ? preferredSide
                    : -preferredSide;
                if (!TryResolveCurvatureWaypoint(
                        directStart,
                        directEnd,
                        pathSeed,
                        directness,
                        side,
                        validCells,
                        fluidCoverage,
                        majorSupport,
                        metricPositions,
                        out int waypointCell,
                        out Vector2 waypointPosition))
                {
                    continue;
                }

                if (!TryFindPathSegment(
                        start,
                        waypointCell,
                        directStart,
                        waypointPosition,
                        pathSeed ^ 0x68BC21EBu,
                        maximumExpansions,
                        0.20f,
                        0.028f,
                        width,
                        height,
                        validCells,
                        componentLabels,
                        fluidCoverage,
                        majorSupport,
                        connectorSupport,
                        metricPositions,
                        pair.StartComponentIndex,
                        pair.EndComponentIndex,
                        start,
                        goal,
                        out List<int> firstLeg))
                {
                    continue;
                }

                if (!TryFindPathSegment(
                        waypointCell,
                        goal,
                        waypointPosition,
                        directEnd,
                        pathSeed ^ 0x02E5BE93u,
                        maximumExpansions,
                        0.20f,
                        0.028f,
                        width,
                        height,
                        validCells,
                        componentLabels,
                        fluidCoverage,
                        majorSupport,
                        connectorSupport,
                        metricPositions,
                        pair.StartComponentIndex,
                        pair.EndComponentIndex,
                        start,
                        goal,
                        out List<int> secondLeg))
                {
                    continue;
                }

                path = firstLeg;
                for (int index = 1; index < secondLeg.Count; index++)
                {
                    path.Add(secondLeg[index]);
                }
                curvatureWaypoint = waypointPosition;
                rejectionReason =
                    StylizedRiverFoamConnectorRejectionReason.None;
                return path.Count >= 3;
            }

            path = new List<int>();
            rejectionReason =
                StylizedRiverFoamConnectorRejectionReason.SearchExhausted;
            return false;
        }

        private static bool TryFindPathSegment(
            int start,
            int goal,
            Vector2 corridorStart,
            Vector2 corridorEnd,
            uint pathSeed,
            int maximumExpansions,
            float corridorPenaltyScale,
            float noiseAmplitude,
            int width,
            int height,
            bool[] validCells,
            int[] componentLabels,
            float[] fluidCoverage,
            float[] majorSupport,
            float[] connectorSupport,
            Vector2[] metricPositions,
            int startComponent,
            int endComponent,
            int startEndpointCell,
            int endEndpointCell,
            out List<int> path)
        {
            int cellCount = width * height;
            float[] cost = new float[cellCount];
            int[] previous = new int[cellCount];
            bool[] closed = new bool[cellCount];
            for (int index = 0; index < cellCount; index++)
            {
                cost[index] = float.PositiveInfinity;
                previous[index] = -1;
            }

            MinHeap open = new MinHeap();
            cost[start] = 0f;
            open.Push(new HeapEntry(
                start,
                Vector2.Distance(
                    metricPositions[start],
                    metricPositions[goal]),
                0u));

            int expansions = 0;
            int[] neighbourX = { -1, 1, 0, 0, -1, -1, 1, 1 };
            int[] neighbourY = { 0, 0, -1, 1, -1, 1, -1, 1 };

            while (open.Count > 0 && expansions < maximumExpansions)
            {
                HeapEntry currentEntry = open.Pop();
                int current = currentEntry.CellIndex;
                if (closed[current])
                {
                    continue;
                }

                if (current == goal)
                {
                    path = ReconstructPath(previous, start, goal);
                    return path.Count >= 2;
                }

                closed[current] = true;
                expansions++;
                int currentX = current % width;
                int currentY = current / width;

                for (int neighbourIndex = 0;
                     neighbourIndex < neighbourX.Length;
                     neighbourIndex++)
                {
                    int x = currentX + neighbourX[neighbourIndex];
                    int y = currentY + neighbourY[neighbourIndex];
                    if (x < 0 || x >= width || y < 0 || y >= height)
                    {
                        continue;
                    }

                    int next = x + y * width;
                    if (closed[next] ||
                        !IsConnectorTraversalCellAllowed(
                            next,
                            validCells,
                            componentLabels,
                            majorSupport,
                            metricPositions,
                            startComponent,
                            endComponent,
                            startEndpointCell,
                            endEndpointCell))
                    {
                        continue;
                    }

                    bool diagonal = neighbourX[neighbourIndex] != 0 &&
                        neighbourY[neighbourIndex] != 0;
                    if (diagonal)
                    {
                        int sideA = currentX + neighbourX[neighbourIndex] +
                            currentY * width;
                        int sideB = currentX +
                            (currentY + neighbourY[neighbourIndex]) * width;
                        if (!IsConnectorTraversalCellAllowed(
                                sideA,
                                validCells,
                                componentLabels,
                                majorSupport,
                                metricPositions,
                                startComponent,
                                endComponent,
                                startEndpointCell,
                                endEndpointCell) ||
                            !IsConnectorTraversalCellAllowed(
                                sideB,
                                validCells,
                                componentLabels,
                                majorSupport,
                                metricPositions,
                                startComponent,
                                endComponent,
                                startEndpointCell,
                                endEndpointCell))
                        {
                            continue;
                        }
                    }

                    float physicalStep = Vector2.Distance(
                        metricPositions[current],
                        metricPositions[next]);
                    float major = Mathf.Clamp01(majorSupport[next]);
                    float fluid = Mathf.Clamp01(fluidCoverage[next]);
                    float generated = Mathf.Clamp01(connectorSupport[next]);
                    float traversalMultiplier = Mathf.Lerp(
                        1f,
                        0.20f,
                        Mathf.SmoothStep(0.10f, 0.72f, major));
                    traversalMultiplier *= Mathf.Lerp(
                        1.85f,
                        1f,
                        fluid);
                    traversalMultiplier *= Mathf.Lerp(
                        1f,
                        1.70f,
                        generated);

                    float corridorDeviation = DistancePointToSegment(
                        metricPositions[next],
                        corridorStart,
                        corridorEnd);
                    float corridorPenalty = corridorDeviation *
                        corridorPenaltyScale;
                    float noise = (Hash01(
                        pathSeed ^ (uint)(next + 1),
                        19u) - 0.5f) * noiseAmplitude;
                    float tentative = cost[current] +
                        physicalStep * traversalMultiplier +
                        corridorPenalty * physicalStep +
                        noise * physicalStep;
                    if (tentative >= cost[next])
                    {
                        continue;
                    }

                    previous[next] = current;
                    cost[next] = tentative;
                    float heuristic = Vector2.Distance(
                        metricPositions[next],
                        metricPositions[goal]) * 0.88f;
                    uint tie = MixBits(
                        pathSeed ^
                        (uint)(next + 1) * 0x9E3779B9u);
                    open.Push(new HeapEntry(
                        next,
                        tentative + heuristic,
                        tie));
                }
            }

            path = new List<int>();
            return false;
        }

        private static bool TryResolveCurvatureWaypoint(
            Vector2 start,
            Vector2 end,
            uint pathSeed,
            float directness,
            float side,
            bool[] validCells,
            float[] fluidCoverage,
            float[] majorSupport,
            Vector2[] metricPositions,
            out int waypointCell,
            out Vector2 waypointPosition)
        {
            waypointCell = -1;
            waypointPosition = (start + end) * 0.5f;
            float indirectness = 1f - Mathf.Clamp01(directness);
            Vector2 segment = end - start;
            float segmentLength = segment.magnitude;
            if (indirectness <= 0.0001f || segmentLength <= 0.0001f)
            {
                return false;
            }

            Vector2 direction = segment / segmentLength;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            float bendEnvelope = Mathf.SmoothStep(0f, 1f, indirectness);
            float offsetScale = Mathf.Lerp(
                0.24f,
                0.42f,
                Hash01(pathSeed, 24u));
            float targetOffset = segmentLength * offsetScale * bendEnvelope;
            float targetAlong = Mathf.Lerp(
                0.42f,
                0.58f,
                Hash01(pathSeed, 25u));
            Vector2 desired = start + segment * targetAlong +
                perpendicular * (side * targetOffset);
            float minimumSignedDeviation = targetOffset * 0.48f;
            float bestScore = float.PositiveInfinity;

            for (int index = 0; index < validCells.Length; index++)
            {
                if (!validCells[index] ||
                    majorSupport[index] >= MajorRoutingHaloThreshold)
                {
                    continue;
                }

                Vector2 position = metricPositions[index];
                float along = Vector2.Dot(position - start, direction) /
                    segmentLength;
                if (along < 0.24f || along > 0.76f)
                {
                    continue;
                }

                Vector2 projected = start + segment * along;
                float signedDeviation = Vector2.Dot(
                    position - projected,
                    perpendicular) * side;
                if (signedDeviation < minimumSignedDeviation)
                {
                    continue;
                }

                float score = Vector2.SqrMagnitude(position - desired);
                score += (1f - Mathf.Clamp01(fluidCoverage[index])) * 0.18f;
                score += Mathf.Clamp01(majorSupport[index]) * 0.14f;
                if (score < bestScore)
                {
                    bestScore = score;
                    waypointCell = index;
                    waypointPosition = position;
                }
            }

            return waypointCell >= 0;
        }

        private static List<int> ReconstructPath(
            int[] previous,
            int start,
            int goal)
        {
            List<int> reversed = new();
            int current = goal;
            int guard = previous.Length + 1;
            while (current >= 0 && guard-- > 0)
            {
                reversed.Add(current);
                if (current == start)
                {
                    break;
                }
                current = previous[current];
            }

            reversed.Reverse();
            return reversed;
        }

        private static List<int> SimplifyPath(
            List<int> path,
            int width,
            int height,
            bool[] validCells,
            int[] componentLabels,
            float[] majorSupport,
            int startComponent,
            int endComponent,
            int startEndpointCell,
            int endEndpointCell,
            float directness,
            Vector2 curvatureWaypoint,
            Vector2[] metricPositions)
        {
            if (path.Count <= 2)
            {
                return path;
            }

            int maximumProbeDistance = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Lerp(10f, 18f, directness)),
                10,
                18);
            List<int> simplified = new() { path[0] };

            if (directness >= 0.9999f)
            {
                AppendSimplifiedRange(
                    path,
                    0,
                    path.Count - 1,
                    maximumProbeDistance,
                    width,
                    height,
                    validCells,
                    componentLabels,
                    majorSupport,
                    metricPositions,
                    startComponent,
                    endComponent,
                    startEndpointCell,
                    endEndpointCell,
                    simplified);
                return simplified;
            }

            int pivotPathIndex = 1;
            float pivotDistance = float.PositiveInfinity;
            for (int index = 1; index < path.Count - 1; index++)
            {
                float distance = Vector2.SqrMagnitude(
                    metricPositions[path[index]] - curvatureWaypoint);
                if (distance < pivotDistance)
                {
                    pivotDistance = distance;
                    pivotPathIndex = index;
                }
            }

            AppendSimplifiedRange(
                path,
                0,
                pivotPathIndex,
                maximumProbeDistance,
                width,
                height,
                validCells,
                componentLabels,
                majorSupport,
                metricPositions,
                startComponent,
                endComponent,
                startEndpointCell,
                endEndpointCell,
                simplified);
            AppendSimplifiedRange(
                path,
                pivotPathIndex,
                path.Count - 1,
                maximumProbeDistance,
                width,
                height,
                validCells,
                componentLabels,
                majorSupport,
                metricPositions,
                startComponent,
                endComponent,
                startEndpointCell,
                endEndpointCell,
                simplified);
            return simplified;
        }

        private static void AppendSimplifiedRange(
            List<int> path,
            int startPathIndex,
            int endPathIndex,
            int maximumProbeDistance,
            int width,
            int height,
            bool[] validCells,
            int[] componentLabels,
            float[] majorSupport,
            Vector2[] metricPositions,
            int startComponent,
            int endComponent,
            int startEndpointCell,
            int endEndpointCell,
            List<int> destination)
        {
            int anchor = startPathIndex;
            while (anchor < endPathIndex)
            {
                int furthest = anchor + 1;
                int maximumProbe = Mathf.Min(
                    endPathIndex,
                    anchor + maximumProbeDistance);
                for (int candidate = anchor + 2;
                     candidate <= maximumProbe;
                     candidate++)
                {
                    if (!HasGridLineOfSight(
                            path[anchor],
                            path[candidate],
                            width,
                            height,
                            validCells,
                            componentLabels,
                            majorSupport,
                            metricPositions,
                            startComponent,
                            endComponent,
                            startEndpointCell,
                            endEndpointCell))
                    {
                        break;
                    }

                    furthest = candidate;
                }

                int cellIndex = path[furthest];
                if (destination.Count == 0 ||
                    destination[destination.Count - 1] != cellIndex)
                {
                    destination.Add(cellIndex);
                }
                anchor = furthest;
            }
        }

        private static bool HasGridLineOfSight(
            int start,
            int end,
            int width,
            int height,
            bool[] validCells,
            int[] componentLabels,
            float[] majorSupport,
            Vector2[] metricPositions,
            int startComponent,
            int endComponent,
            int startEndpointCell,
            int endEndpointCell)
        {
            int x0 = start % width;
            int y0 = start / width;
            int x1 = end % width;
            int y1 = end / width;
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int error = dx - dy;

            while (true)
            {
                if (x0 < 0 || x0 >= width || y0 < 0 || y0 >= height)
                {
                    return false;
                }

                int cellIndex = x0 + y0 * width;
                if (!IsConnectorTraversalCellAllowed(
                        cellIndex,
                        validCells,
                        componentLabels,
                        majorSupport,
                        metricPositions,
                        startComponent,
                        endComponent,
                        startEndpointCell,
                        endEndpointCell))
                {
                    return false;
                }

                if (x0 == x1 && y0 == y1)
                {
                    return true;
                }

                int doubled = error * 2;
                bool moveX = doubled > -dy;
                bool moveY = doubled < dx;
                if (moveX && moveY)
                {
                    int sideX = x0 + sx + y0 * width;
                    int sideY = x0 + (y0 + sy) * width;
                    if (!IsConnectorTraversalCellAllowed(
                            sideX,
                            validCells,
                            componentLabels,
                            majorSupport,
                            metricPositions,
                            startComponent,
                            endComponent,
                            startEndpointCell,
                            endEndpointCell) ||
                        !IsConnectorTraversalCellAllowed(
                            sideY,
                            validCells,
                            componentLabels,
                            majorSupport,
                            metricPositions,
                            startComponent,
                            endComponent,
                            startEndpointCell,
                            endEndpointCell))
                    {
                        return false;
                    }
                }

                if (moveX)
                {
                    error -= dy;
                    x0 += sx;
                }
                if (moveY)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        private static bool IsConnectorTraversalCellAllowed(
            int cellIndex,
            bool[] validCells,
            int[] componentLabels,
            float[] majorSupport,
            Vector2[] metricPositions,
            int startComponent,
            int endComponent,
            int startEndpointCell,
            int endEndpointCell)
        {
            if (cellIndex < 0 || cellIndex >= validCells.Length ||
                !validCells[cellIndex])
            {
                return false;
            }

            float major = majorSupport[cellIndex];
            if (major < MajorRoutingHaloThreshold)
            {
                return true;
            }

            int label = componentLabels[cellIndex];
            if (label >= 0 &&
                label != startComponent &&
                label != endComponent)
            {
                return false;
            }

            // Major Support forms a blocked clearance halo. Only compact gates
            // around the selected source and destination endpoints are open.
            // This prevents the pathfinder from travelling around a region's
            // perimeter merely to reach an unlikely corner.
            bool insideStartGate = Vector2.Distance(
                metricPositions[cellIndex],
                metricPositions[startEndpointCell]) <=
                EndpointGateRadiusMetres;
            bool insideEndGate = Vector2.Distance(
                metricPositions[cellIndex],
                metricPositions[endEndpointCell]) <=
                EndpointGateRadiusMetres;

            if (label == startComponent)
            {
                return insideStartGate;
            }

            if (label == endComponent)
            {
                return insideEndGate;
            }

            // Soft support outside the labelled component interior belongs to
            // the same clearance halo. It is traversable only through one of
            // the two endpoint gates.
            return insideStartGate || insideEndGate;
        }

        private static bool PathCrossesBlockedMajorHalo(
            List<int> path,
            bool[] validCells,
            int[] componentLabels,
            float[] majorSupport,
            Vector2[] metricPositions,
            int startComponent,
            int endComponent,
            int startEndpointCell,
            int endEndpointCell)
        {
            for (int index = 0; index < path.Count; index++)
            {
                if (!IsConnectorTraversalCellAllowed(
                        path[index],
                        validCells,
                        componentLabels,
                        majorSupport,
                        metricPositions,
                        startComponent,
                        endComponent,
                        startEndpointCell,
                        endEndpointCell))
                {
                    return true;
                }
            }

            return false;
        }

        private static float MeasureUnsupportedSpan(
            List<int> path,
            float[] majorSupport,
            Vector2[] metricPositions)
        {
            float span = 0f;
            for (int index = 1; index < path.Count; index++)
            {
                int a = path[index - 1];
                int b = path[index];
                float major = Mathf.Max(
                    majorSupport[a],
                    majorSupport[b]);
                if (major < 0.56f)
                {
                    span += Vector2.Distance(
                        metricPositions[a],
                        metricPositions[b]);
                }
            }

            return span;
        }

        private static float MeasurePathLength(
            List<int> path,
            Vector2[] metricPositions)
        {
            float length = 0f;
            for (int index = 1; index < path.Count; index++)
            {
                length += Vector2.Distance(
                    metricPositions[path[index - 1]],
                    metricPositions[path[index]]);
            }
            return length;
        }

        private static void RasterizeConnector(
            List<int> simplifiedPath,
            uint stableId,
            int width,
            int height,
            bool[] validCells,
            float[] majorSupport,
            Vector2[] metricPositions,
            float[] destination)
        {
            float outerRadius = Mathf.Lerp(
                0.17f,
                0.27f,
                Hash01(stableId, 31u));
            float coreRadius = outerRadius * Mathf.Lerp(
                0.20f,
                0.36f,
                Hash01(stableId, 32u));

            for (int cellIndex = 0;
                 cellIndex < destination.Length;
                 cellIndex++)
            {
                if (!validCells[cellIndex])
                {
                    continue;
                }

                Vector2 position = metricPositions[cellIndex];
                float minimumDistance = float.PositiveInfinity;
                for (int segmentIndex = 1;
                     segmentIndex < simplifiedPath.Count;
                     segmentIndex++)
                {
                    float distance = DistancePointToSegment(
                        position,
                        metricPositions[simplifiedPath[segmentIndex - 1]],
                        metricPositions[simplifiedPath[segmentIndex]]);
                    minimumDistance = Mathf.Min(minimumDistance, distance);
                }

                if (minimumDistance > outerRadius)
                {
                    continue;
                }

                float major = Mathf.Clamp01(majorSupport[cellIndex]);
                if (major >= MajorRoutingInteriorThreshold)
                {
                    continue;
                }

                float value = 1f - Mathf.SmoothStep(
                    coreRadius,
                    outerRadius,
                    minimumDistance);
                float majorSuppression = 1f - Mathf.SmoothStep(
                    MajorRasterSuppressionStart,
                    MajorRoutingInteriorThreshold,
                    major);
                destination[cellIndex] = Mathf.Max(
                    destination[cellIndex],
                    value * majorSuppression);
            }
        }

        private static uint ResolveRelationshipId(ComponentPair pair)
        {
            uint lowComponent = pair.StartComponentId < pair.EndComponentId
                ? pair.StartComponentId
                : pair.EndComponentId;
            uint highComponent = pair.StartComponentId < pair.EndComponentId
                ? pair.EndComponentId
                : pair.StartComponentId;
            uint lowEndpoint = pair.StartEndpoint.StableId <
                pair.EndEndpoint.StableId
                    ? pair.StartEndpoint.StableId
                    : pair.EndEndpoint.StableId;
            uint highEndpoint = pair.StartEndpoint.StableId <
                pair.EndEndpoint.StableId
                    ? pair.EndEndpoint.StableId
                    : pair.StartEndpoint.StableId;
            return MixBits(
                lowComponent ^
                RotateLeft(highComponent, 7) ^
                RotateLeft(lowEndpoint, 17) ^
                RotateLeft(highEndpoint, 23));
        }

        private static float ResolveAcrossNormalized(
            int cellIndex,
            int width,
            int height)
        {
            int y = cellIndex / width;
            return y /
                (float)Mathf.Max(1, height - 1) * 2f - 1f;
        }

        private static float ResolveAverageVisibleWidth(
            RiverDomainSnapshot domain)
        {
            const int sampleCount = 24;
            float total = 0f;
            for (int index = 0; index < sampleCount; index++)
            {
                float distance = domain.LocalLength *
                    (index + 0.5f) / sampleCount;
                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(distance);
                total += sample.LeftHalfWidth + sample.RightHalfWidth;
            }
            return total / sampleCount;
        }

        private static float Across01ToMetres(
            float across01,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            if (across01 <= 0.5f)
            {
                return -leftHalfWidth * (1f - across01 * 2f);
            }
            return rightHalfWidth * (across01 * 2f - 1f);
        }

        private static float DistancePointToSegment(
            Vector2 point,
            Vector2 start,
            Vector2 end)
        {
            Vector2 segment = end - start;
            float denominator = segment.sqrMagnitude;
            if (denominator <= 0.000001f)
            {
                return Vector2.Distance(point, start);
            }

            float t = Mathf.Clamp01(
                Vector2.Dot(point - start, segment) / denominator);
            return Vector2.Distance(point, start + segment * t);
        }

        private static float Hash01(uint seed, uint stream)
        {
            return (MixBits(seed ^ MixBits(stream + 0xC2B2AE35u)) &
                0x00FFFFFFu) / 16777216f;
        }

        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        private static uint MixBits(uint value)
        {
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return value;
        }

        private sealed class MajorComponent
        {
            public MajorComponent(
                List<int> cells,
                Vector2 centroid)
            {
                Cells = cells;
                Centroid = centroid;
            }

            public List<int> Cells { get; }
            public Vector2 Centroid { get; }
            public List<uint> SourceRegionIds { get; } = new();
            public List<ConnectorEndpoint> Endpoints { get; } = new();
            public uint StableId { get; set; }
        }

        private readonly struct EndpointCandidate
        {
            public EndpointCandidate(
                bool valid,
                int cellIndex,
                float score,
                int sector)
            {
                Valid = valid;
                CellIndex = cellIndex;
                Score = score;
                Sector = sector;
            }

            public bool Valid { get; }
            public int CellIndex { get; }
            public float Score { get; }
            public int Sector { get; }
        }

        private readonly struct ConnectorEndpoint
        {
            public ConnectorEndpoint(
                uint stableId,
                int cellIndex,
                Vector2 position)
            {
                StableId = stableId;
                CellIndex = cellIndex;
                Position = position;
            }

            public uint StableId { get; }
            public int CellIndex { get; }
            public Vector2 Position { get; }
        }

        private readonly struct EndpointPairCandidate
        {
            public EndpointPairCandidate(
                ConnectorEndpoint start,
                ConnectorEndpoint end,
                float distanceMetres,
                float selectionBias,
                float selectionScore)
            {
                Start = start;
                End = end;
                DistanceMetres = distanceMetres;
                SelectionBias = selectionBias;
                SelectionScore = selectionScore;
            }

            public ConnectorEndpoint Start { get; }
            public ConnectorEndpoint End { get; }
            public float DistanceMetres { get; }
            public float SelectionBias { get; }
            public float SelectionScore { get; }
        }

        private readonly struct ComponentPair
        {
            public ComponentPair(
                int startComponentIndex,
                int endComponentIndex,
                uint startComponentId,
                uint endComponentId,
                ConnectorEndpoint startEndpoint,
                ConnectorEndpoint endEndpoint,
                float distanceMetres,
                float maximumPathLengthMetres,
                float score)
            {
                StartComponentIndex = startComponentIndex;
                EndComponentIndex = endComponentIndex;
                StartComponentId = startComponentId;
                EndComponentId = endComponentId;
                StartEndpoint = startEndpoint;
                EndEndpoint = endEndpoint;
                DistanceMetres = distanceMetres;
                MaximumPathLengthMetres = maximumPathLengthMetres;
                Score = score;
            }

            public int StartComponentIndex { get; }
            public int EndComponentIndex { get; }
            public uint StartComponentId { get; }
            public uint EndComponentId { get; }
            public ConnectorEndpoint StartEndpoint { get; }
            public ConnectorEndpoint EndEndpoint { get; }
            public float DistanceMetres { get; }
            public float MaximumPathLengthMetres { get; }
            public float Score { get; }
        }

        private readonly struct HeapEntry
        {
            public HeapEntry(int cellIndex, float priority, uint tie)
            {
                CellIndex = cellIndex;
                Priority = priority;
                Tie = tie;
            }

            public int CellIndex { get; }
            public float Priority { get; }
            public uint Tie { get; }
        }

        private sealed class MinHeap
        {
            private readonly List<HeapEntry> entries = new();

            public int Count => entries.Count;

            public void Push(HeapEntry entry)
            {
                entries.Add(entry);
                int index = entries.Count - 1;
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (!IsLess(entries[index], entries[parent]))
                    {
                        break;
                    }
                    (entries[index], entries[parent]) =
                        (entries[parent], entries[index]);
                    index = parent;
                }
            }

            public HeapEntry Pop()
            {
                HeapEntry root = entries[0];
                int lastIndex = entries.Count - 1;
                entries[0] = entries[lastIndex];
                entries.RemoveAt(lastIndex);
                int index = 0;

                while (true)
                {
                    int left = index * 2 + 1;
                    if (left >= entries.Count)
                    {
                        break;
                    }

                    int right = left + 1;
                    int smallest = right < entries.Count &&
                        IsLess(entries[right], entries[left])
                            ? right
                            : left;
                    if (!IsLess(entries[smallest], entries[index]))
                    {
                        break;
                    }

                    (entries[index], entries[smallest]) =
                        (entries[smallest], entries[index]);
                    index = smallest;
                }

                return root;
            }

            private static bool IsLess(HeapEntry a, HeapEntry b)
            {
                int priorityComparison = a.Priority.CompareTo(b.Priority);
                return priorityComparison != 0
                    ? priorityComparison < 0
                    : a.Tie < b.Tie;
            }
        }

        private sealed class DisjointSet
        {
            private readonly int[] parent;
            private readonly byte[] rank;

            public DisjointSet(int count)
            {
                parent = new int[count];
                rank = new byte[count];
                for (int index = 0; index < count; index++)
                {
                    parent[index] = index;
                }
            }

            public int Find(int value)
            {
                int root = value;
                while (parent[root] != root)
                {
                    root = parent[root];
                }

                while (parent[value] != value)
                {
                    int next = parent[value];
                    parent[value] = root;
                    value = next;
                }

                return root;
            }

            public void Union(int a, int b)
            {
                int rootA = Find(a);
                int rootB = Find(b);
                if (rootA == rootB)
                {
                    return;
                }

                if (rank[rootA] < rank[rootB])
                {
                    parent[rootA] = rootB;
                }
                else if (rank[rootA] > rank[rootB])
                {
                    parent[rootB] = rootA;
                }
                else
                {
                    parent[rootB] = rootA;
                    rank[rootA]++;
                }
            }
        }
    }
}
