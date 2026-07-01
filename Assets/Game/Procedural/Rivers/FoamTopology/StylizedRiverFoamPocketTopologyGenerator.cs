using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Deterministic CPU proof generator for the Major-hosted Negative Aging
    /// Pressure classes. Interior Pockets preserve a closed positive rim. Edge
    /// Cavities remain associated with a broad Major host but deliberately bias
    /// toward and breach one selected side while preserving a useful positive
    /// remainder. Both classes avoid important Connector cores and obstacle
    /// context and remain independent from positive support. Authoritative live
    /// Pressure, Lee, and Shore protection is applied during GPU composition.
    /// </summary>
    public static class StylizedRiverFoamPocketTopologyGenerator
    {
        private const float MajorInteriorThreshold = 0.30f;
        private const float ConnectorCoreThreshold = 0.18f;
        private const float ConnectorProtectionRadiusMetres = 0.22f;
        private const float MinimumInteriorRadiusMetres = 0.48f;
        private const float ExtendedInteriorRadiusMetres = 0.38f;
        private const float MinimumCavityHostRadiusMetres = 0.58f;
        private const float PositiveRimWidthMetres = 0.22f;
        private const float RimFeatherMetres = 0.10f;
        private const float MinimumPocketRadiusMetres = 0.24f;
        private const float MaximumPocketRadiusMetres = 1.20f;
        private const float MinimumPocketSpacingMetres = 0.62f;
        private const float MinimumCavitySpacingMetres = 0.54f;
        private const float CavityOutwardLimitMetres = 0.36f;
        private const float MaximumCavityHostCoverage = 0.54f;
        private const float MinimumHostRemainderRatio = 0.28f;
        private const float PocketCoverageThreshold = 0.15f;
        private const int MaximumInteriorPocketsPerHost = 3;
        private const int MaximumCavitiesPerHost = 2;
        private const uint InvalidHostId = uint.MaxValue;

        public static StylizedRiverFoamPocketTopology Generate(
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            StylizedRiverQuality quality,
            float shoreMotion,
            int seed,
            float interiorPocketAmount,
            float edgeCavityAmount,
            float[] obstacleMask,
            StylizedRiverFoamMajorTopology majorTopology,
            StylizedRiverFoamConnectorTopology connectorTopology)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int cellCount = Mathf.Max(0, width * height);
            float[] pressure = new float[cellCount];
            int reasonCount = Enum.GetValues(
                typeof(StylizedRiverFoamPocketRejectionReason)).Length;
            int[] rejectionCounts = new int[reasonCount];

            if (domain == null || !domain.IsValid ||
                majorTopology == null || connectorTopology == null ||
                width < 2 || height < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f ||
                majorTopology.Width != width || majorTopology.Height != height ||
                connectorTopology.Width != width ||
                connectorTopology.Height != height)
            {
                stopwatch.Stop();
                return CreateEmpty(
                    width,
                    height,
                    stopwatch.Elapsed.TotalMilliseconds,
                    pressure,
                    rejectionCounts);
            }

            float[] majorSupport = majorTopology.SupportData;
            float[] connectorSupport = connectorTopology.SupportData;
            if (majorSupport == null || majorSupport.Length < cellCount ||
                connectorSupport == null || connectorSupport.Length < cellCount)
            {
                stopwatch.Stop();
                return CreateEmpty(
                    width,
                    height,
                    stopwatch.Elapsed.TotalMilliseconds,
                    pressure,
                    rejectionCounts);
            }

            seed = Mathf.Max(0, seed);
            interiorPocketAmount = Mathf.Clamp01(interiorPocketAmount);
            edgeCavityAmount = Mathf.Clamp01(edgeCavityAmount);

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
            float[] connectorDistance = BuildDistanceFromSources(
                width,
                height,
                metricPositions,
                index => connectorSupport[index] >= ConnectorCoreThreshold,
                null);

            bool[] majorHostInterior = new bool[cellCount];
            bool[] pocketInterior = new bool[cellCount];
            for (int index = 0; index < cellCount; index++)
            {
                majorHostInterior[index] =
                    validCells[index] &&
                    fluidCoverage[index] > 0.35f &&
                    majorSupport[index] >= MajorInteriorThreshold;
                pocketInterior[index] =
                    majorHostInterior[index] &&
                    connectorDistance[index] >
                        ConnectorProtectionRadiusMetres;
            }

            float[] interiorDistance = BuildDistanceFromSources(
                width,
                height,
                metricPositions,
                index => !pocketInterior[index],
                pocketInterior);
            List<HostMetric> hosts = BuildHostMetrics(
                majorTopology.Regions,
                domain);
            uint[] hostByCell = BuildHostAssignments(
                majorHostInterior,
                metricPositions,
                hosts);
            Dictionary<uint, int> hostCellCounts = CountHostCells(
                majorHostInterior,
                hostByCell);
            Dictionary<uint, List<HostBoundaryPoint>> hostBoundaries =
                BuildHostBoundaries(
                    width,
                    height,
                    majorHostInterior,
                    hostByCell,
                    connectorDistance,
                    metricPositions);

            List<PocketCandidate> allCandidates = FindCandidateCentres(
                width,
                height,
                domain,
                seed,
                pocketInterior,
                interiorDistance,
                metricPositions,
                hosts);

            float[] candidateRaster = new float[cellCount];
            List<PreparedNegativeRegion> feasibleInteriorRegions = new();
            if (interiorPocketAmount > 0.0001f)
            {
                List<PocketCandidate> orderedInteriorCandidates =
                    OrderInteriorCandidates(allCandidates);
                Dictionary<uint, int> feasibleInteriorByHost = new();
                List<AcceptedNegativeRegion> acceptedInteriorSpacing = new();

                for (int candidateIndex = 0;
                     candidateIndex < orderedInteriorCandidates.Count;
                     candidateIndex++)
                {
                    PocketCandidate candidate =
                        orderedInteriorCandidates[candidateIndex];
                    feasibleInteriorByHost.TryGetValue(
                        candidate.HostRegionId,
                        out int hostPocketCount);
                    if (hostPocketCount >= MaximumInteriorPocketsPerHost)
                    {
                        rejectionCounts[(int)
                            StylizedRiverFoamPocketRejectionReason.Spacing]++;
                        continue;
                    }

                    float availableRadius = candidate.InteriorRadius -
                        PositiveRimWidthMetres;
                    if (availableRadius < MinimumPocketRadiusMetres)
                    {
                        rejectionCounts[(int)
                            StylizedRiverFoamPocketRejectionReason
                                .HostTooNarrow]++;
                        continue;
                    }

                    float radiusSelector = Hash01(candidate.StableId, 11u);
                    float baseRadius = Mathf.Clamp(
                        availableRadius * Mathf.Lerp(
                            0.62f,
                            0.88f,
                            radiusSelector),
                        MinimumPocketRadiusMetres,
                        MaximumPocketRadiusMetres);
                    float aspectSelector = Hash01(candidate.StableId, 12u);
                    float alongRadius = baseRadius * Mathf.Lerp(
                        0.82f,
                        1.24f,
                        aspectSelector);
                    float acrossRadius = baseRadius * Mathf.Lerp(
                        1.12f,
                        0.72f,
                        aspectSelector);
                    float largestRadius = Mathf.Max(
                        alongRadius,
                        acrossRadius);

                    if (ViolatesSpacing(
                        candidate.Position,
                        largestRadius,
                        StylizedRiverFoamNegativeRegionClass.InteriorPocket,
                        acceptedInteriorSpacing))
                    {
                        rejectionCounts[(int)
                            StylizedRiverFoamPocketRejectionReason.Spacing]++;
                        continue;
                    }

                    float orientation = Mathf.Lerp(
                        -Mathf.PI,
                        Mathf.PI,
                        Hash01(candidate.StableId, 13u));
                    Array.Clear(candidateRaster, 0, candidateRaster.Length);
                    int rasterCoverage = RasterizeInteriorPocket(
                        candidate,
                        orientation,
                        alongRadius,
                        acrossRadius,
                        width,
                        height,
                        validCells,
                        majorSupport,
                        connectorSupport,
                        connectorDistance,
                        interiorDistance,
                        metricPositions,
                        candidateRaster);
                    if (rasterCoverage <= 0)
                    {
                        rejectionCounts[(int)
                            StylizedRiverFoamPocketRejectionReason
                                .NoRasterCoverage]++;
                        continue;
                    }

                    feasibleInteriorByHost[candidate.HostRegionId] =
                        hostPocketCount + 1;
                    acceptedInteriorSpacing.Add(new AcceptedNegativeRegion(
                        StylizedRiverFoamNegativeRegionClass.InteriorPocket,
                        candidate.Position,
                        largestRadius));
                    feasibleInteriorRegions.Add(new PreparedNegativeRegion(
                        StylizedRiverFoamNegativeRegionClass.InteriorPocket,
                        candidate.StableId,
                        candidate.HostRegionId,
                        candidate.Position,
                        candidate.AcrossNormalized,
                        orientation,
                        alongRadius,
                        acrossRadius,
                        Vector2.zero,
                        CaptureRaster(candidateRaster)));
                }
            }

            List<PreparedNegativeRegion> feasibleCavityRegions = new();
            int unusableBoundaryCount = 0;
            if (edgeCavityAmount > 0.0001f)
            {
                List<CavityCandidate> cavityCandidates =
                    BuildCavityCandidates(
                        allCandidates,
                        hostBoundaries,
                        out unusableBoundaryCount);
                rejectionCounts[(int)
                    StylizedRiverFoamPocketRejectionReason
                        .NoUsableBoundary] += unusableBoundaryCount;
                cavityCandidates = OrderCavityCandidates(cavityCandidates);

                Dictionary<uint, int> feasibleCavitiesByHost = new();
                List<AcceptedNegativeRegion> acceptedCavitySpacing = new();
                float[] acceptedCavityPressure = new float[cellCount];
                for (int candidateIndex = 0;
                     candidateIndex < cavityCandidates.Count;
                     candidateIndex++)
                {
                    CavityCandidate candidate =
                        cavityCandidates[candidateIndex];
                    feasibleCavitiesByHost.TryGetValue(
                        candidate.HostRegionId,
                        out int hostCavityCount);
                    if (hostCavityCount >= MaximumCavitiesPerHost)
                    {
                        rejectionCounts[(int)
                            StylizedRiverFoamPocketRejectionReason.Spacing]++;
                        continue;
                    }

                    float radiusSelector = Hash01(candidate.StableId, 31u);
                    float baseRadius = Mathf.Clamp(
                        candidate.InteriorRadius * Mathf.Lerp(
                            0.52f,
                            0.78f,
                            radiusSelector),
                        MinimumPocketRadiusMetres,
                        MaximumPocketRadiusMetres);
                    float tangentSelector = Hash01(candidate.StableId, 32u);
                    float alongRadius = baseRadius * Mathf.Lerp(
                        0.72f,
                        0.96f,
                        tangentSelector);
                    float acrossRadius = baseRadius * Mathf.Lerp(
                        0.92f,
                        1.28f,
                        tangentSelector);
                    float largestRadius = Mathf.Max(
                        alongRadius,
                        acrossRadius);

                    if (ViolatesSpacing(
                        candidate.Position,
                        largestRadius,
                        StylizedRiverFoamNegativeRegionClass.EdgeCavity,
                        acceptedCavitySpacing))
                    {
                        rejectionCounts[(int)
                            StylizedRiverFoamPocketRejectionReason.Spacing]++;
                        continue;
                    }

                    float orientation = Mathf.Atan2(
                        candidate.BreachDirection.y,
                        candidate.BreachDirection.x);
                    Array.Clear(candidateRaster, 0, candidateRaster.Length);
                    CavityRasterResult rasterResult = RasterizeEdgeCavity(
                        candidate,
                        orientation,
                        alongRadius,
                        acrossRadius,
                        width,
                        height,
                        validCells,
                        majorSupport,
                        connectorSupport,
                        connectorDistance,
                        hostByCell,
                        metricPositions,
                        candidateRaster);
                    if (rasterResult.TotalCoverage <= 0)
                    {
                        rejectionCounts[(int)
                            StylizedRiverFoamPocketRejectionReason
                                .NoRasterCoverage]++;
                        continue;
                    }

                    if (rasterResult.InsideHostCoverage <= 0 ||
                        rasterResult.OutsideHostCoverage <= 0)
                    {
                        rejectionCounts[(int)
                            StylizedRiverFoamPocketRejectionReason
                                .NoEdgeBreach]++;
                        continue;
                    }

                    if (WouldCollapseHost(
                        candidate.HostRegionId,
                        width,
                        height,
                        hostByCell,
                        hostCellCounts,
                        acceptedCavityPressure,
                        candidateRaster))
                    {
                        rejectionCounts[(int)
                            StylizedRiverFoamPocketRejectionReason
                                .HostWouldCollapse]++;
                        continue;
                    }

                    MergeMaximum(acceptedCavityPressure, candidateRaster);
                    feasibleCavitiesByHost[candidate.HostRegionId] =
                        hostCavityCount + 1;
                    acceptedCavitySpacing.Add(new AcceptedNegativeRegion(
                        StylizedRiverFoamNegativeRegionClass.EdgeCavity,
                        candidate.Position,
                        largestRadius));
                    feasibleCavityRegions.Add(new PreparedNegativeRegion(
                        StylizedRiverFoamNegativeRegionClass.EdgeCavity,
                        candidate.StableId,
                        candidate.HostRegionId,
                        candidate.Position,
                        ResolveAcrossNormalized(domain, candidate.Position),
                        orientation,
                        alongRadius,
                        acrossRadius,
                        candidate.BreachDirection,
                        CaptureRaster(candidateRaster)));
                }
            }

            int selectedInteriorCount = ResolveSelectedCount(
                interiorPocketAmount,
                feasibleInteriorRegions.Count);
            int selectedCavityCount = ResolveSelectedCount(
                edgeCavityAmount,
                feasibleCavityRegions.Count);
            List<StylizedRiverFoamPocketRegion> regions = new(
                selectedInteriorCount + selectedCavityCount);
            ApplyPreparedPrefix(
                feasibleInteriorRegions,
                selectedInteriorCount,
                domain,
                pressure,
                regions);
            ApplyPreparedPrefix(
                feasibleCavityRegions,
                selectedCavityCount,
                domain,
                pressure,
                regions);

            int interiorEligibleHostCount = CountPreparedHosts(
                feasibleInteriorRegions);
            int cavityEligibleHostCount = CountPreparedHosts(
                feasibleCavityRegions);

            int coveredCellCount = 0;
            for (int index = 0; index < pressure.Length; index++)
            {
                if (pressure[index] >= PocketCoverageThreshold)
                {
                    coveredCellCount++;
                }
            }

            stopwatch.Stop();
            return new StylizedRiverFoamPocketTopology(
                width,
                height,
                interiorEligibleHostCount,
                feasibleInteriorRegions.Count,
                selectedInteriorCount,
                cavityEligibleHostCount,
                feasibleCavityRegions.Count,
                selectedCavityCount,
                coveredCellCount,
                stopwatch.Elapsed.TotalMilliseconds,
                pressure,
                regions.ToArray(),
                rejectionCounts);
        }

        private static StylizedRiverFoamPocketTopology CreateEmpty(
            int width,
            int height,
            double milliseconds,
            float[] pressure,
            int[] rejectionCounts)
        {
            return new StylizedRiverFoamPocketTopology(
                width,
                height,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                milliseconds,
                pressure,
                Array.Empty<StylizedRiverFoamPocketRegion>(),
                rejectionCounts);
        }

        private static List<HostMetric> BuildHostMetrics(
            IReadOnlyList<StylizedRiverFoamMajorRegion> regions,
            RiverDomainSnapshot domain)
        {
            List<HostMetric> hosts = new();
            if (regions == null)
            {
                return hosts;
            }

            for (int index = 0; index < regions.Count; index++)
            {
                StylizedRiverFoamMajorRegion region = regions[index];
                float localDistance = Mathf.Clamp(
                    region.CentreGlobalDistance -
                        domain.GlobalDistanceMinimum,
                    0f,
                    domain.LocalLength);
                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(localDistance);
                float lateral = SignedNormalizedToMetres(
                    region.CentreAcrossNormalized,
                    Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth),
                    Mathf.Max(0.05f, sample.RightSurfaceHalfWidth));
                hosts.Add(new HostMetric(
                    region.StableId,
                    new Vector2(localDistance, lateral)));
            }

            return hosts;
        }

        private static List<PocketCandidate> FindCandidateCentres(
            int width,
            int height,
            RiverDomainSnapshot domain,
            int seed,
            bool[] interior,
            float[] distance,
            Vector2[] positions,
            List<HostMetric> hosts)
        {
            List<PocketCandidate> candidates = new();
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int index = x + y * width;
                    float radius = distance[index];
                    if (!interior[index] ||
                        radius < ExtendedInteriorRadiusMetres ||
                        !IsLocalMaximum(index, width, distance))
                    {
                        continue;
                    }

                    uint hostId = ResolveNearestHost(
                        positions[index],
                        hosts);
                    uint stableId = MixBits(
                        (uint)(seed + 1) * 0x9E3779B9u ^
                        hostId ^
                        (uint)(index + 1) * 0x85EBCA6Bu);
                    float jitter = Hash01(stableId, 4u) * 0.035f;
                    float acrossNormalized = ResolveAcrossNormalized(
                        domain,
                        positions[index]);
                    candidates.Add(new PocketCandidate(
                        stableId,
                        hostId,
                        index,
                        positions[index],
                        acrossNormalized,
                        radius,
                        radius + jitter));
                }
            }

            return candidates;
        }

        private static List<PocketCandidate> OrderInteriorCandidates(
            List<PocketCandidate> candidates)
        {
            List<PocketCandidate> ordered = new(candidates);
            ordered.Sort((a, b) =>
            {
                int scoreComparison = b.Score.CompareTo(a.Score);
                return scoreComparison != 0
                    ? scoreComparison
                    : a.StableId.CompareTo(b.StableId);
            });

            Dictionary<uint, int> nextHostRank = new();
            Dictionary<uint, int> hostRankByStableId = new();
            for (int index = 0; index < ordered.Count; index++)
            {
                PocketCandidate candidate = ordered[index];
                nextHostRank.TryGetValue(
                    candidate.HostRegionId,
                    out int hostRank);
                hostRankByStableId[candidate.StableId] = hostRank;
                nextHostRank[candidate.HostRegionId] = hostRank + 1;
            }

            ordered.Sort((a, b) =>
            {
                float aBaselineBonus = a.InteriorRadius >=
                    MinimumInteriorRadiusMetres ? 0.10f : 0f;
                float bBaselineBonus = b.InteriorRadius >=
                    MinimumInteriorRadiusMetres ? 0.10f : 0f;
                float aAdjustedScore = a.Score + aBaselineBonus -
                    hostRankByStableId[a.StableId] * 0.12f;
                float bAdjustedScore = b.Score + bBaselineBonus -
                    hostRankByStableId[b.StableId] * 0.12f;
                int scoreComparison =
                    bAdjustedScore.CompareTo(aAdjustedScore);
                return scoreComparison != 0
                    ? scoreComparison
                    : a.StableId.CompareTo(b.StableId);
            });
            return ordered;
        }

        private static bool IsLocalMaximum(
            int index,
            int width,
            float[] distance)
        {
            float value = distance[index];
            int[] offsets =
            {
                -width - 1, -width, -width + 1,
                -1, 1,
                width - 1, width, width + 1
            };

            bool strictlyGreaterThanOne = false;
            for (int offsetIndex = 0;
                 offsetIndex < offsets.Length;
                 offsetIndex++)
            {
                float neighbour = distance[index + offsets[offsetIndex]];
                if (neighbour > value + 0.0001f)
                {
                    return false;
                }

                if (value > neighbour + 0.0001f)
                {
                    strictlyGreaterThanOne = true;
                }
            }

            return strictlyGreaterThanOne;
        }

        private static int RasterizeInteriorPocket(
            PocketCandidate candidate,
            float orientation,
            float alongRadius,
            float acrossRadius,
            int width,
            int height,
            bool[] validCells,
            float[] majorSupport,
            float[] connectorSupport,
            float[] connectorDistance,
            float[] interiorDistance,
            Vector2[] positions,
            float[] destination)
        {
            float cos = Mathf.Cos(orientation);
            float sin = Mathf.Sin(orientation);
            float outerRadius = Mathf.Max(alongRadius, acrossRadius) * 1.35f;
            int written = 0;
            uint noiseSeed = MixBits(candidate.StableId ^ 0xA511E9B3u);

            for (int index = 0; index < destination.Length; index++)
            {
                if (!validCells[index] ||
                    majorSupport[index] < MajorInteriorThreshold ||
                    connectorSupport[index] >= ConnectorCoreThreshold ||
                    connectorDistance[index] <=
                        ConnectorProtectionRadiusMetres ||
                    interiorDistance[index] <= PositiveRimWidthMetres)
                {
                    continue;
                }

                Vector2 delta = positions[index] - candidate.Position;
                if (delta.sqrMagnitude > outerRadius * outerRadius)
                {
                    continue;
                }

                float localX = cos * delta.x + sin * delta.y;
                float localY = -sin * delta.x + cos * delta.y;
                float normalizedX = localX / Mathf.Max(0.001f, alongRadius);
                float normalizedY = localY / Mathf.Max(0.001f, acrossRadius);

                float shape = EvaluateIrregularShape(
                    normalizedX,
                    normalizedY,
                    noiseSeed);
                if (shape <= 0.001f)
                {
                    continue;
                }

                float rimGate = SmoothStep(
                    PositiveRimWidthMetres,
                    PositiveRimWidthMetres + RimFeatherMetres,
                    interiorDistance[index]);
                float connectorGate = SmoothStep(
                    ConnectorProtectionRadiusMetres,
                    ConnectorProtectionRadiusMetres + 0.12f,
                    connectorDistance[index]);
                float value = Mathf.Clamp01(
                    shape * rimGate * connectorGate);
                destination[index] = Mathf.Max(destination[index], value);
                if (value >= PocketCoverageThreshold)
                {
                    written++;
                }
            }

            return written;
        }

        private static CavityRasterResult RasterizeEdgeCavity(
            CavityCandidate candidate,
            float orientation,
            float alongRadius,
            float acrossRadius,
            int width,
            int height,
            bool[] validCells,
            float[] majorSupport,
            float[] connectorSupport,
            float[] connectorDistance,
            uint[] hostByCell,
            Vector2[] positions,
            float[] destination)
        {
            float cos = Mathf.Cos(orientation);
            float sin = Mathf.Sin(orientation);
            float outerRadius = Mathf.Max(alongRadius, acrossRadius) * 1.42f;
            uint noiseSeed = MixBits(candidate.StableId ^ 0xF1357AE5u);
            int total = 0;
            int inside = 0;
            int outside = 0;

            for (int index = 0; index < destination.Length; index++)
            {
                if (!validCells[index] ||
                    connectorSupport[index] >= ConnectorCoreThreshold ||
                    connectorDistance[index] <= ConnectorProtectionRadiusMetres)
                {
                    continue;
                }

                bool insideMajor = majorSupport[index] >=
                    MajorInteriorThreshold;
                if (insideMajor && hostByCell[index] != candidate.HostRegionId)
                {
                    continue;
                }

                Vector2 delta = positions[index] - candidate.Position;
                if (delta.sqrMagnitude > outerRadius * outerRadius)
                {
                    continue;
                }

                float outwardFromBoundary = Vector2.Dot(
                    positions[index] - candidate.BoundaryPosition,
                    candidate.BreachDirection);
                if (outwardFromBoundary > CavityOutwardLimitMetres)
                {
                    continue;
                }

                float localX = cos * delta.x + sin * delta.y;
                float localY = -sin * delta.x + cos * delta.y;
                float normalizedX = localX / Mathf.Max(0.001f, alongRadius);
                float normalizedY = localY / Mathf.Max(0.001f, acrossRadius);
                float shape = EvaluateIrregularShape(
                    normalizedX,
                    normalizedY,
                    noiseSeed);
                if (shape <= 0.001f)
                {
                    continue;
                }

                // Keep the negative influence biased toward the selected side
                // of the host instead of allowing a centred closed pocket.
                float sideProjection = Vector2.Dot(
                    positions[index] - candidate.InteriorPosition,
                    candidate.BreachDirection);
                float sideGate = SmoothStep(
                    -alongRadius * 0.44f,
                    alongRadius * 0.16f,
                    sideProjection);
                float connectorGate = SmoothStep(
                    ConnectorProtectionRadiusMetres,
                    ConnectorProtectionRadiusMetres + 0.12f,
                    connectorDistance[index]);
                float value = Mathf.Clamp01(
                    shape * sideGate * connectorGate);
                destination[index] = Mathf.Max(destination[index], value);
                if (value < PocketCoverageThreshold)
                {
                    continue;
                }

                total++;
                if (insideMajor)
                {
                    inside++;
                }
                else
                {
                    outside++;
                }
            }

            return new CavityRasterResult(total, inside, outside);
        }

        private static float EvaluateIrregularShape(
            float normalizedX,
            float normalizedY,
            uint noiseSeed)
        {
            float noiseX = normalizedX * 1.75f + 13.7f;
            float noiseY = normalizedY * 1.75f - 8.3f;
            float warpX = (ValueNoise(noiseX, noiseY, noiseSeed) - 0.5f) *
                0.22f;
            float warpY = (ValueNoise(
                noiseX + 5.17f,
                noiseY - 3.43f,
                noiseSeed ^ 0x68BC21EBu) - 0.5f) * 0.22f;
            float warpedX = normalizedX + warpX;
            float warpedY = normalizedY + warpY;
            float radial = Mathf.Sqrt(
                warpedX * warpedX + warpedY * warpedY);
            float broadNoise = ValueNoise(
                normalizedX * 2.35f + 21.1f,
                normalizedY * 2.35f + 4.7f,
                noiseSeed ^ 0xC2B2AE35u);
            float shapeDistance = radial + (broadNoise - 0.5f) * 0.24f;
            return 1f - SmoothStep(0.70f, 1.02f, shapeDistance);
        }

        private static uint[] BuildHostAssignments(
            bool[] interior,
            Vector2[] positions,
            List<HostMetric> hosts)
        {
            uint[] result = new uint[interior.Length];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = InvalidHostId;
            }

            for (int index = 0; index < interior.Length; index++)
            {
                if (interior[index])
                {
                    result[index] = ResolveNearestHost(
                        positions[index],
                        hosts);
                }
            }

            return result;
        }

        private static Dictionary<uint, int> CountHostCells(
            bool[] interior,
            uint[] hostByCell)
        {
            Dictionary<uint, int> counts = new();
            for (int index = 0; index < hostByCell.Length; index++)
            {
                uint hostId = hostByCell[index];
                if (!interior[index] || hostId == InvalidHostId)
                {
                    continue;
                }

                counts.TryGetValue(hostId, out int count);
                counts[hostId] = count + 1;
            }

            return counts;
        }

        private static Dictionary<uint, List<HostBoundaryPoint>>
            BuildHostBoundaries(
                int width,
                int height,
                bool[] interior,
                uint[] hostByCell,
                float[] connectorDistance,
                Vector2[] positions)
        {
            Dictionary<uint, List<HostBoundaryPoint>> result = new();
            int[] neighbourX = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] neighbourY = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int index = x + y * width;
                    uint hostId = hostByCell[index];
                    if (!interior[index] || hostId == InvalidHostId ||
                        connectorDistance[index] <=
                            ConnectorProtectionRadiusMetres)
                    {
                        continue;
                    }

                    bool boundary = false;
                    for (int neighbourIndex = 0;
                         neighbourIndex < neighbourX.Length;
                         neighbourIndex++)
                    {
                        int nx = x + neighbourX[neighbourIndex];
                        int ny = y + neighbourY[neighbourIndex];
                        int next = nx + ny * width;
                        if (!interior[next])
                        {
                            boundary = true;
                            break;
                        }
                    }

                    if (!boundary)
                    {
                        continue;
                    }

                    if (!result.TryGetValue(
                        hostId,
                        out List<HostBoundaryPoint> points))
                    {
                        points = new List<HostBoundaryPoint>();
                        result.Add(hostId, points);
                    }

                    points.Add(new HostBoundaryPoint(index, positions[index]));
                }
            }

            return result;
        }

        private static List<CavityCandidate> BuildCavityCandidates(
            List<PocketCandidate> baseCandidates,
            Dictionary<uint, List<HostBoundaryPoint>> hostBoundaries,
            out int unusableBoundaryCount)
        {
            List<CavityCandidate> result = new();
            unusableBoundaryCount = 0;

            for (int index = 0; index < baseCandidates.Count; index++)
            {
                PocketCandidate source = baseCandidates[index];
                if (source.InteriorRadius < MinimumCavityHostRadiusMetres)
                {
                    continue;
                }

                uint stableId = MixBits(source.StableId ^ 0xB5297A4Du);
                if (!hostBoundaries.TryGetValue(
                    source.HostRegionId,
                    out List<HostBoundaryPoint> boundaries) ||
                    !TrySelectBoundary(
                        source,
                        stableId,
                        boundaries,
                        out HostBoundaryPoint boundary,
                        out Vector2 breachDirection,
                        out float boundaryScore))
                {
                    unusableBoundaryCount++;
                    continue;
                }

                Vector2 position = Vector2.Lerp(
                    source.Position,
                    boundary.Position,
                    0.70f);
                result.Add(new CavityCandidate(
                    stableId,
                    source.HostRegionId,
                    source.Position,
                    position,
                    boundary.Position,
                    breachDirection,
                    source.InteriorRadius,
                    source.Score + boundaryScore * 0.12f));
            }

            return result;
        }

        private static List<CavityCandidate> OrderCavityCandidates(
            List<CavityCandidate> candidates)
        {
            List<CavityCandidate> ordered = new(candidates);
            ordered.Sort((a, b) =>
            {
                int scoreComparison = b.Score.CompareTo(a.Score);
                return scoreComparison != 0
                    ? scoreComparison
                    : a.StableId.CompareTo(b.StableId);
            });

            Dictionary<uint, int> nextHostRank = new();
            Dictionary<uint, int> hostRankByStableId = new();
            for (int index = 0; index < ordered.Count; index++)
            {
                CavityCandidate candidate = ordered[index];
                nextHostRank.TryGetValue(
                    candidate.HostRegionId,
                    out int hostRank);
                hostRankByStableId[candidate.StableId] = hostRank;
                nextHostRank[candidate.HostRegionId] = hostRank + 1;
            }

            ordered.Sort((a, b) =>
            {
                float aAdjustedScore = a.Score -
                    hostRankByStableId[a.StableId] * 0.14f;
                float bAdjustedScore = b.Score -
                    hostRankByStableId[b.StableId] * 0.14f;
                int scoreComparison =
                    bAdjustedScore.CompareTo(aAdjustedScore);
                return scoreComparison != 0
                    ? scoreComparison
                    : a.StableId.CompareTo(b.StableId);
            });
            return ordered;
        }

        private static bool TrySelectBoundary(
            PocketCandidate source,
            uint stableId,
            List<HostBoundaryPoint> boundaries,
            out HostBoundaryPoint selected,
            out Vector2 breachDirection,
            out float selectedScore)
        {
            selected = default;
            breachDirection = Vector2.zero;
            selectedScore = float.NegativeInfinity;
            if (boundaries == null || boundaries.Count == 0)
            {
                return false;
            }

            float desiredAngle = Mathf.Lerp(
                -Mathf.PI,
                Mathf.PI,
                Hash01(stableId, 42u));
            Vector2 desiredDirection = new Vector2(
                Mathf.Cos(desiredAngle),
                Mathf.Sin(desiredAngle));
            float maximumDistance = Mathf.Max(
                1.0f,
                source.InteriorRadius * 2.5f);

            for (int index = 0; index < boundaries.Count; index++)
            {
                HostBoundaryPoint boundary = boundaries[index];
                Vector2 delta = boundary.Position - source.Position;
                float distance = delta.magnitude;
                if (distance < 0.12f || distance > maximumDistance)
                {
                    continue;
                }

                Vector2 direction = delta / distance;
                float angularFit = Vector2.Dot(direction, desiredDirection) *
                    0.5f + 0.5f;
                float distanceFit = 1f - Mathf.Clamp01(
                    Mathf.Abs(distance - source.InteriorRadius) /
                    Mathf.Max(0.15f, source.InteriorRadius));
                float tieBreak = Hash01(
                    stableId ^ (uint)(boundary.CellIndex + 1),
                    43u) * 0.025f;
                float score = angularFit * 0.72f +
                    distanceFit * 0.28f + tieBreak;
                if (score <= selectedScore)
                {
                    continue;
                }

                selectedScore = score;
                selected = boundary;
                breachDirection = direction;
            }

            return selectedScore > float.NegativeInfinity &&
                breachDirection.sqrMagnitude > 0.000001f;
        }

        private static bool ViolatesSpacing(
            Vector2 position,
            float largestRadius,
            StylizedRiverFoamNegativeRegionClass regionClass,
            List<AcceptedNegativeRegion> accepted)
        {
            for (int index = 0; index < accepted.Count; index++)
            {
                AcceptedNegativeRegion existing = accepted[index];
                bool sameClass = existing.RegionClass == regionClass;
                if (!sameClass)
                {
                    continue;
                }

                float minimum = regionClass ==
                    StylizedRiverFoamNegativeRegionClass.InteriorPocket
                        ? MinimumPocketSpacingMetres
                        : MinimumCavitySpacingMetres;
                float scale = 0.72f;
                float requiredSpacing = Mathf.Max(
                    minimum,
                    (largestRadius + existing.LargestRadius) * scale);
                if (Vector2.Distance(position, existing.Position) <
                    requiredSpacing)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool WouldCollapseHost(
            uint hostId,
            int width,
            int height,
            uint[] hostByCell,
            Dictionary<uint, int> hostCellCounts,
            float[] acceptedCavityPressure,
            float[] candidateRaster)
        {
            if (!hostCellCounts.TryGetValue(hostId, out int hostCellCount) ||
                hostCellCount <= 0)
            {
                return true;
            }

            bool[] remainder = new bool[hostByCell.Length];
            int affected = 0;
            int remainderCount = 0;
            for (int index = 0; index < hostByCell.Length; index++)
            {
                if (hostByCell[index] != hostId)
                {
                    continue;
                }

                float combinedPressure = Mathf.Max(
                    acceptedCavityPressure[index],
                    candidateRaster[index]);
                if (combinedPressure >= PocketCoverageThreshold)
                {
                    affected++;
                    continue;
                }

                remainder[index] = true;
                remainderCount++;
            }

            float affectedRatio = affected / (float)hostCellCount;
            float remainderRatio = remainderCount / (float)hostCellCount;
            if (affectedRatio > MaximumCavityHostCoverage ||
                remainderRatio < MinimumHostRemainderRatio)
            {
                return true;
            }

            int largestComponent = ResolveLargestComponentSize(
                width,
                height,
                remainder);
            int requiredConnectedRemainder = Mathf.Max(
                4,
                Mathf.CeilToInt(remainderCount * 0.84f));
            return largestComponent < requiredConnectedRemainder;
        }

        private static int ResolveLargestComponentSize(
            int width,
            int height,
            bool[] mask)
        {
            bool[] visited = new bool[mask.Length];
            int[] queue = new int[mask.Length];
            int largest = 0;
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            for (int start = 0; start < mask.Length; start++)
            {
                if (!mask[start] || visited[start])
                {
                    continue;
                }

                int head = 0;
                int tail = 0;
                queue[tail++] = start;
                visited[start] = true;
                int count = 0;

                while (head < tail)
                {
                    int index = queue[head++];
                    count++;
                    int x = index % width;
                    int y = index / width;
                    for (int direction = 0; direction < dx.Length; direction++)
                    {
                        int nx = x + dx[direction];
                        int ny = y + dy[direction];
                        if (nx < 0 || nx >= width ||
                            ny < 0 || ny >= height)
                        {
                            continue;
                        }

                        int next = nx + ny * width;
                        if (!mask[next] || visited[next])
                        {
                            continue;
                        }

                        visited[next] = true;
                        queue[tail++] = next;
                    }
                }

                largest = Mathf.Max(largest, count);
            }

            return largest;
        }

        private static void MergeMaximum(
            float[] destination,
            float[] source)
        {
            for (int index = 0; index < destination.Length; index++)
            {
                destination[index] = Mathf.Max(
                    destination[index],
                    source[index]);
            }
        }

        private static void AddRegionMetadata(
            List<StylizedRiverFoamPocketRegion> regions,
            StylizedRiverFoamNegativeRegionClass regionClass,
            uint stableId,
            uint hostId,
            RiverDomainSnapshot domain,
            Vector2 position,
            float acrossNormalized,
            float orientation,
            float alongRadius,
            float acrossRadius,
            Vector2 breachDirection)
        {
            uint evolutionSeed = MixBits(stableId ^ 0xD1B54A35u);
            regions.Add(new StylizedRiverFoamPocketRegion(
                regionClass,
                stableId,
                hostId,
                domain.GlobalDistanceMinimum + position.x,
                acrossNormalized,
                orientation,
                alongRadius,
                acrossRadius,
                breachDirection,
                evolutionSeed,
                Hash01(evolutionSeed, 21u),
                Hash01(evolutionSeed, 22u),
                Hash01(evolutionSeed, 23u),
                Hash01(evolutionSeed, 24u),
                Hash01(evolutionSeed, 25u),
                Hash01(evolutionSeed, 26u),
                Hash01(evolutionSeed, 27u)));
        }

        private static int ResolveSelectedCount(
            float amount,
            int feasibleCount)
        {
            feasibleCount = Mathf.Max(0, feasibleCount);
            amount = Mathf.Clamp01(amount);
            if (feasibleCount == 0 || amount <= 0.0001f)
            {
                return 0;
            }

            if (amount >= 0.9999f)
            {
                return feasibleCount;
            }

            return Mathf.Clamp(
                Mathf.FloorToInt(amount * feasibleCount + 0.5f),
                0,
                feasibleCount);
        }

        private static RasterSample[] CaptureRaster(float[] source)
        {
            List<RasterSample> samples = new();
            for (int index = 0; index < source.Length; index++)
            {
                float value = source[index];
                if (value > 0.0001f)
                {
                    samples.Add(new RasterSample(index, value));
                }
            }

            return samples.ToArray();
        }

        private static void ApplyPreparedPrefix(
            List<PreparedNegativeRegion> prepared,
            int selectedCount,
            RiverDomainSnapshot domain,
            float[] pressure,
            List<StylizedRiverFoamPocketRegion> regions)
        {
            selectedCount = Mathf.Clamp(
                selectedCount,
                0,
                prepared != null ? prepared.Count : 0);
            for (int index = 0; index < selectedCount; index++)
            {
                PreparedNegativeRegion region = prepared[index];
                RasterSample[] samples = region.Samples;
                for (int sampleIndex = 0;
                     sampleIndex < samples.Length;
                     sampleIndex++)
                {
                    RasterSample sample = samples[sampleIndex];
                    pressure[sample.Index] = Mathf.Max(
                        pressure[sample.Index],
                        sample.Value);
                }

                AddRegionMetadata(
                    regions,
                    region.RegionClass,
                    region.StableId,
                    region.HostRegionId,
                    domain,
                    region.Position,
                    region.AcrossNormalized,
                    region.Orientation,
                    region.AlongRadius,
                    region.AcrossRadius,
                    region.BreachDirection);
            }
        }

        private static int CountPreparedHosts(
            List<PreparedNegativeRegion> prepared)
        {
            if (prepared == null || prepared.Count == 0)
            {
                return 0;
            }

            HashSet<uint> hosts = new();
            for (int index = 0; index < prepared.Count; index++)
            {
                hosts.Add(prepared[index].HostRegionId);
            }

            return hosts.Count;
        }

        private static float[] BuildDistanceFromSources(
            int width,
            int height,
            Vector2[] positions,
            Func<int, bool> isSource,
            bool[] traversable)
        {
            int cellCount = width * height;
            float[] distance = new float[cellCount];
            MinHeap heap = new MinHeap(cellCount);
            for (int index = 0; index < cellCount; index++)
            {
                if (isSource(index))
                {
                    distance[index] = 0f;
                    heap.Push(index, 0f);
                }
                else
                {
                    distance[index] = float.PositiveInfinity;
                }
            }

            int[] neighbourX = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] neighbourY = { -1, -1, -1, 0, 0, 1, 1, 1 };
            while (heap.Count > 0)
            {
                heap.Pop(out int index, out float currentDistance);
                if (currentDistance > distance[index] + 0.0001f)
                {
                    continue;
                }

                int x = index % width;
                int y = index / width;
                for (int neighbourIndex = 0;
                     neighbourIndex < neighbourX.Length;
                     neighbourIndex++)
                {
                    int nx = x + neighbourX[neighbourIndex];
                    int ny = y + neighbourY[neighbourIndex];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    {
                        continue;
                    }

                    int next = nx + ny * width;
                    if (traversable != null && !traversable[next])
                    {
                        continue;
                    }

                    float step = Vector2.Distance(
                        positions[index],
                        positions[next]);
                    float proposed = currentDistance + step;
                    if (proposed + 0.0001f >= distance[next])
                    {
                        continue;
                    }

                    distance[next] = proposed;
                    heap.Push(next, proposed);
                }
            }

            return distance;
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

        private static uint ResolveNearestHost(
            Vector2 position,
            List<HostMetric> hosts)
        {
            if (hosts == null || hosts.Count == 0)
            {
                return MixBits(
                    (uint)Mathf.Max(1, Mathf.RoundToInt(position.x * 97f)) ^
                    (uint)Mathf.Max(1, Mathf.RoundToInt(
                        (position.y + 32f) * 131f)));
            }

            int bestIndex = 0;
            float bestDistance = float.PositiveInfinity;
            for (int index = 0; index < hosts.Count; index++)
            {
                float distance = (position - hosts[index].Position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = index;
                }
            }

            return hosts[bestIndex].StableId;
        }

        private static float ResolveAcrossNormalized(
            RiverDomainSnapshot domain,
            Vector2 position)
        {
            StylizedRiverSplineSample sample =
                domain.SampleAtOrientedDistance(Mathf.Clamp(
                    position.x,
                    0f,
                    domain.LocalLength));
            float left = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
            float right = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
            return position.y < 0f
                ? Mathf.Clamp(position.y / left, -1f, 0f)
                : Mathf.Clamp(position.y / right, 0f, 1f);
        }

        private static float Across01ToMetres(
            float across01,
            float leftWidth,
            float rightWidth)
        {
            if (across01 <= 0.5f)
            {
                return Mathf.Lerp(-leftWidth, 0f, across01 * 2f);
            }

            return Mathf.Lerp(0f, rightWidth, (across01 - 0.5f) * 2f);
        }

        private static float SignedNormalizedToMetres(
            float acrossNormalized,
            float leftWidth,
            float rightWidth)
        {
            return acrossNormalized < 0f
                ? acrossNormalized * leftWidth
                : acrossNormalized * rightWidth;
        }

        private static float ValueNoise(
            float x,
            float y,
            uint seed)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float tx = Smooth01(x - x0);
            float ty = Smooth01(y - y0);
            float a = HashLattice(x0, y0, seed);
            float b = HashLattice(x0 + 1, y0, seed);
            float c = HashLattice(x0, y0 + 1, seed);
            float d = HashLattice(x0 + 1, y0 + 1, seed);
            return Mathf.Lerp(
                Mathf.Lerp(a, b, tx),
                Mathf.Lerp(c, d, tx),
                ty);
        }

        private static float HashLattice(int x, int y, uint seed)
        {
            uint value = seed ^
                (uint)x * 0x8DA6B343u ^
                (uint)y * 0xD8163841u;
            return Hash01(MixBits(value), 0xCB1AB31Fu);
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            if (edge1 <= edge0 + 0.000001f)
            {
                return value >= edge1 ? 1f : 0f;
            }

            return Smooth01((value - edge0) / (edge1 - edge0));
        }

        private static float Hash01(uint value, uint salt)
        {
            uint mixed = MixBits(value ^ salt * 0x9E3779B9u);
            return (mixed & 0x00FFFFFFu) / 16777215f;
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

        private readonly struct HostMetric
        {
            public HostMetric(uint stableId, Vector2 position)
            {
                StableId = stableId;
                Position = position;
            }

            public uint StableId { get; }
            public Vector2 Position { get; }
        }

        private readonly struct PocketCandidate
        {
            public PocketCandidate(
                uint stableId,
                uint hostRegionId,
                int cellIndex,
                Vector2 position,
                float acrossNormalized,
                float interiorRadius,
                float score)
            {
                StableId = stableId;
                HostRegionId = hostRegionId;
                CellIndex = cellIndex;
                Position = position;
                AcrossNormalized = acrossNormalized;
                InteriorRadius = interiorRadius;
                Score = score;
            }

            public uint StableId { get; }
            public uint HostRegionId { get; }
            public int CellIndex { get; }
            public Vector2 Position { get; }
            public float AcrossNormalized { get; }
            public float InteriorRadius { get; }
            public float Score { get; }
        }

        private readonly struct HostBoundaryPoint
        {
            public HostBoundaryPoint(int cellIndex, Vector2 position)
            {
                CellIndex = cellIndex;
                Position = position;
            }

            public int CellIndex { get; }
            public Vector2 Position { get; }
        }

        private readonly struct CavityCandidate
        {
            public CavityCandidate(
                uint stableId,
                uint hostRegionId,
                Vector2 interiorPosition,
                Vector2 position,
                Vector2 boundaryPosition,
                Vector2 breachDirection,
                float interiorRadius,
                float score)
            {
                StableId = stableId;
                HostRegionId = hostRegionId;
                InteriorPosition = interiorPosition;
                Position = position;
                BoundaryPosition = boundaryPosition;
                BreachDirection = breachDirection.sqrMagnitude > 0.000001f
                    ? breachDirection.normalized
                    : Vector2.zero;
                InteriorRadius = interiorRadius;
                Score = score;
            }

            public uint StableId { get; }
            public uint HostRegionId { get; }
            public Vector2 InteriorPosition { get; }
            public Vector2 Position { get; }
            public Vector2 BoundaryPosition { get; }
            public Vector2 BreachDirection { get; }
            public float InteriorRadius { get; }
            public float Score { get; }
        }

        private readonly struct RasterSample
        {
            public RasterSample(int index, float value)
            {
                Index = index;
                Value = Mathf.Clamp01(value);
            }

            public int Index { get; }
            public float Value { get; }
        }

        private readonly struct PreparedNegativeRegion
        {
            public PreparedNegativeRegion(
                StylizedRiverFoamNegativeRegionClass regionClass,
                uint stableId,
                uint hostRegionId,
                Vector2 position,
                float acrossNormalized,
                float orientation,
                float alongRadius,
                float acrossRadius,
                Vector2 breachDirection,
                RasterSample[] samples)
            {
                RegionClass = regionClass;
                StableId = stableId;
                HostRegionId = hostRegionId;
                Position = position;
                AcrossNormalized = acrossNormalized;
                Orientation = orientation;
                AlongRadius = alongRadius;
                AcrossRadius = acrossRadius;
                BreachDirection = breachDirection;
                Samples = samples ?? Array.Empty<RasterSample>();
            }

            public StylizedRiverFoamNegativeRegionClass RegionClass { get; }
            public uint StableId { get; }
            public uint HostRegionId { get; }
            public Vector2 Position { get; }
            public float AcrossNormalized { get; }
            public float Orientation { get; }
            public float AlongRadius { get; }
            public float AcrossRadius { get; }
            public Vector2 BreachDirection { get; }
            public RasterSample[] Samples { get; }
        }

        private readonly struct AcceptedNegativeRegion
        {
            public AcceptedNegativeRegion(
                StylizedRiverFoamNegativeRegionClass regionClass,
                Vector2 position,
                float largestRadius)
            {
                RegionClass = regionClass;
                Position = position;
                LargestRadius = largestRadius;
            }

            public StylizedRiverFoamNegativeRegionClass RegionClass { get; }
            public Vector2 Position { get; }
            public float LargestRadius { get; }
        }

        private readonly struct CavityRasterResult
        {
            public CavityRasterResult(
                int totalCoverage,
                int insideHostCoverage,
                int outsideHostCoverage)
            {
                TotalCoverage = Mathf.Max(0, totalCoverage);
                InsideHostCoverage = Mathf.Max(0, insideHostCoverage);
                OutsideHostCoverage = Mathf.Max(0, outsideHostCoverage);
            }

            public int TotalCoverage { get; }
            public int InsideHostCoverage { get; }
            public int OutsideHostCoverage { get; }
        }

        private sealed class MinHeap
        {
            private int[] indices;
            private float[] priorities;

            public MinHeap(int capacity)
            {
                capacity = Mathf.Max(4, capacity);
                indices = new int[capacity];
                priorities = new float[capacity];
            }

            public int Count { get; private set; }

            public void Push(int index, float priority)
            {
                EnsureCapacity(Count + 1);
                int position = Count++;
                while (position > 0)
                {
                    int parent = (position - 1) >> 1;
                    if (priorities[parent] <= priority)
                    {
                        break;
                    }

                    indices[position] = indices[parent];
                    priorities[position] = priorities[parent];
                    position = parent;
                }

                indices[position] = index;
                priorities[position] = priority;
            }

            public void Pop(out int index, out float priority)
            {
                index = indices[0];
                priority = priorities[0];
                Count--;
                if (Count <= 0)
                {
                    return;
                }

                int tailIndex = indices[Count];
                float tailPriority = priorities[Count];
                int position = 0;
                while (true)
                {
                    int left = position * 2 + 1;
                    if (left >= Count)
                    {
                        break;
                    }

                    int right = left + 1;
                    int child = right < Count &&
                        priorities[right] < priorities[left]
                            ? right
                            : left;
                    if (priorities[child] >= tailPriority)
                    {
                        break;
                    }

                    indices[position] = indices[child];
                    priorities[position] = priorities[child];
                    position = child;
                }

                indices[position] = tailIndex;
                priorities[position] = tailPriority;
            }

            private void EnsureCapacity(int required)
            {
                if (required <= indices.Length)
                {
                    return;
                }

                int next = Mathf.Max(required, indices.Length * 2);
                Array.Resize(ref indices, next);
                Array.Resize(ref priorities, next);
            }
        }
    }
}
