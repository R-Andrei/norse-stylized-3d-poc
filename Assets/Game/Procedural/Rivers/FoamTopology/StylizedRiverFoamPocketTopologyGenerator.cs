using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Deterministic CPU proof generator for prepared Negative Aging Pressure.
    /// Interior Pockets preserve a closed Major rim. Edge Cavities breach one
    /// deliberate Major side while preserving a useful positive remainder.
    /// Connector Weak Spans remain bound to accepted Connector identities and
    /// locally weaken short path sections away from endpoint gates. Free-Water
    /// Negative Events use sparse metric-space opportunities without requiring a
    /// positive host. All classes remain independent from positive support.
    /// Authoritative live Pressure, Lee, and Shore protection is applied during
    /// GPU composition.
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
        private const int InteriorHostedVariantCount = 4;
        private const int CavityHostedVariantCount = 3;
        private const int HostedVariantAttemptMultiplier = 3;
        private const float HostedVariantPressureThreshold = 0.12f;
        private const float HostedInteriorMinimumContainment = 0.70f;
        private const float HostedInteriorContainmentTolerance = 0.02f;
        private const float HostedCavityOverlapTolerance = 0.10f;
        private const int FreeWaterEvolutionMaskResolution = 32;
        private const float FreeWaterEvolutionPadding = 1.55f;
        private const int FreeWaterRecycleAnchorAttemptCount = 18;
        private const int MaximumFreeWaterRecycleAnchorCount = 4;
        private const float FreeWaterRecycleTerritoryMetres = 0.48f;
        private const float FreeWaterMinimumRecycleRunwayMetres = 0.85f;
        private const float FreeWaterAnchorPressureThreshold = 0.01f;
        private const float GoldenRatioConjugate = 0.61803398875f;
        private static readonly float[] FreeWaterRecycleLateralOffsets =
        {
            0f, -0.14f, 0.14f, -0.27f, 0.27f
        };
        private const float MinimumWeakSpanConnectorLengthMetres = 0.72f;
        private const float MinimumWeakSpanUsableLengthMetres = 0.26f;
        private const float WeakSpanEndpointClearanceMetres = 0.34f;
        private const float SecondaryWeakSpanMinimumLengthMetres = 1.60f;
        private const float MinimumWeakSpanSpacingMetres = 0.44f;
        private const float WeakSpanConnectorThreshold = 0.045f;
        private const float WeakSpanMajorProtectionThreshold = 0.30f;
        private const float FreeWaterLatticeSpacingMetres = 1.24f;
        private const float MinimumFreeWaterSpacingMetres = 0.96f;
        private const float FreeWaterLongitudinalMarginMetres = 0.24f;
        private const float FreeWaterLateralMarginMetres = 0.10f;
        private const float FreeWaterNearestSampleRadiusMetres = 0.34f;
        private const float MinimumFreeWaterAlongRadiusMetres = 0.30f;
        private const float MaximumFreeWaterAlongRadiusMetres = 0.58f;
        private const float MinimumFreeWaterAcrossRadiusMetres = 0.18f;
        private const float MaximumFreeWaterAcrossRadiusMetres = 0.36f;
        private const float FreeWaterSmallScaleMinimum = 0.58f;
        private const float FreeWaterSmallScaleMaximum = 0.76f;
        private const float FreeWaterMediumScaleMinimum = 0.90f;
        private const float FreeWaterMediumScaleMaximum = 1.08f;
        private const float FreeWaterLargeScaleMinimum = 1.26f;
        private const float FreeWaterLargeScaleMaximum = 1.48f;
        private const float MaximumFreeWaterOrientationRadians = 0.24f;
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
            float connectorWeakSpanAmount,
            float freeWaterEventAmount,
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
            connectorWeakSpanAmount = Mathf.Clamp01(
                connectorWeakSpanAmount);
            freeWaterEventAmount = Mathf.Clamp01(freeWaterEventAmount);

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

            List<PreparedNegativeRegion> feasibleWeakSpanRegions = new();
            int weakSpanEligibleConnectorCount = 0;
            if (connectorWeakSpanAmount > 0.0001f)
            {
                feasibleWeakSpanRegions = BuildConnectorWeakSpanRegions(
                    connectorTopology.PreparedPaths,
                    domain,
                    seed,
                    validCells,
                    majorSupport,
                    connectorSupport,
                    metricPositions,
                    rejectionCounts,
                    out weakSpanEligibleConnectorCount);
            }

            List<PreparedNegativeRegion> feasibleFreeWaterRegions = new();
            int freeWaterOpportunityCount = 0;
            if (freeWaterEventAmount > 0.0001f)
            {
                feasibleFreeWaterRegions = BuildFreeWaterNegativeEventRegions(
                    domain,
                    width,
                    height,
                    fieldLength,
                    validFieldLength,
                    seed,
                    validCells,
                    fluidCoverage,
                    majorSupport,
                    connectorSupport,
                    metricPositions,
                    rejectionCounts,
                    out freeWaterOpportunityCount);
            }

            int selectedInteriorCount = ResolveSelectedCount(
                interiorPocketAmount,
                feasibleInteriorRegions.Count);
            int selectedCavityCount = ResolveSelectedCount(
                edgeCavityAmount,
                feasibleCavityRegions.Count);
            int selectedWeakSpanCount = ResolveSelectedCount(
                connectorWeakSpanAmount,
                feasibleWeakSpanRegions.Count);
            int selectedFreeWaterCount = ResolveSelectedCount(
                RemapFreeWaterAmount(freeWaterEventAmount),
                feasibleFreeWaterRegions.Count);

            float[] hostedPressure = new float[cellCount];
            float[] staticIndependentPressure = new float[cellCount];
            float[] freeWaterPressure = new float[cellCount];
            float[] independentPressure = new float[cellCount];
            MergePreparedPrefix(
                feasibleInteriorRegions,
                selectedInteriorCount,
                hostedPressure);
            MergePreparedPrefix(
                feasibleCavityRegions,
                selectedCavityCount,
                hostedPressure);
            MergePreparedPrefix(
                feasibleWeakSpanRegions,
                selectedWeakSpanCount,
                staticIndependentPressure);
            MergePreparedPrefix(
                feasibleFreeWaterRegions,
                selectedFreeWaterCount,
                freeWaterPressure);
            MergeMaximum(independentPressure, staticIndependentPressure);
            MergeMaximum(independentPressure, freeWaterPressure);
            StylizedRiverFoamPreparedHostedNegativeRegion[]
                preparedHostedRegions = BuildPreparedHostedRegions(
                    feasibleInteriorRegions,
                    selectedInteriorCount,
                    feasibleCavityRegions,
                    selectedCavityCount,
                    width,
                    height,
                    fieldLength,
                    validFieldLength,
                    domain,
                    majorTopology);
            StylizedRiverFoamPreparedFreeWaterRegion[]
                preparedFreeWaterRegions = BuildPreparedFreeWaterRegions(
                    feasibleFreeWaterRegions,
                    selectedFreeWaterCount,
                    width,
                    height,
                    fieldLength,
                    validFieldLength,
                    domain,
                    fluidCoverage,
                    obstacleMask);
            float[] hostedFallbackPressure = new float[cellCount];
            MergeUnpreparedHostedPrefix(
                feasibleInteriorRegions,
                selectedInteriorCount,
                preparedHostedRegions,
                hostedFallbackPressure);
            MergeUnpreparedHostedPrefix(
                feasibleCavityRegions,
                selectedCavityCount,
                preparedHostedRegions,
                hostedFallbackPressure);

            List<StylizedRiverFoamPocketRegion> regions = new(
                selectedInteriorCount + selectedCavityCount +
                selectedWeakSpanCount + selectedFreeWaterCount);
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
            ApplyPreparedPrefix(
                feasibleWeakSpanRegions,
                selectedWeakSpanCount,
                domain,
                pressure,
                regions);
            ApplyPreparedPrefix(
                feasibleFreeWaterRegions,
                selectedFreeWaterCount,
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
                weakSpanEligibleConnectorCount,
                feasibleWeakSpanRegions.Count,
                selectedWeakSpanCount,
                freeWaterOpportunityCount,
                feasibleFreeWaterRegions.Count,
                selectedFreeWaterCount,
                coveredCellCount,
                stopwatch.Elapsed.TotalMilliseconds,
                pressure,
                hostedPressure,
                hostedFallbackPressure,
                independentPressure,
                staticIndependentPressure,
                freeWaterPressure,
                regions.ToArray(),
                preparedHostedRegions,
                preparedFreeWaterRegions,
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
                0,
                0,
                0,
                0,
                0,
                0,
                milliseconds,
                pressure,
                new float[pressure != null ? pressure.Length : 0],
                new float[pressure != null ? pressure.Length : 0],
                new float[pressure != null ? pressure.Length : 0],
                new float[pressure != null ? pressure.Length : 0],
                new float[pressure != null ? pressure.Length : 0],
                Array.Empty<StylizedRiverFoamPocketRegion>(),
                Array.Empty<
                    StylizedRiverFoamPreparedHostedNegativeRegion>(),
                Array.Empty<StylizedRiverFoamPreparedFreeWaterRegion>(),
                rejectionCounts);
        }

        private static List<PreparedNegativeRegion>
            BuildConnectorWeakSpanRegions(
                IReadOnlyList<StylizedRiverFoamConnectorPath> paths,
                RiverDomainSnapshot domain,
                int seed,
                bool[] validCells,
                float[] majorSupport,
                float[] connectorSupport,
                Vector2[] metricPositions,
                int[] rejectionCounts,
                out int eligibleConnectorCount)
        {
            eligibleConnectorCount = 0;
            List<WeakSpanOpportunity> primary = new();
            List<WeakSpanOpportunity> secondary = new();
            if (paths == null)
            {
                return new List<PreparedNegativeRegion>();
            }

            for (int pathIndex = 0; pathIndex < paths.Count; pathIndex++)
            {
                StylizedRiverFoamConnectorPath path = paths[pathIndex];
                Vector2[] points = path?.MetricPointData;
                float length = MeasurePolyline(points);
                if (points == null || points.Length < 2 ||
                    length < MinimumWeakSpanConnectorLengthMetres)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason
                            .ConnectorTooShort]++;
                    continue;
                }

                float endpointClearance = Mathf.Min(
                    Mathf.Max(
                        WeakSpanEndpointClearanceMetres,
                        length * 0.16f),
                    length * 0.34f);
                float usableLength = length - endpointClearance * 2f;
                if (usableLength < MinimumWeakSpanUsableLengthMetres)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason
                            .NoUsableConnectorSpan]++;
                    continue;
                }

                eligibleConnectorCount++;
                uint primaryId = MixBits(
                    path.StableId ^ (uint)seed ^ 0xA24BAED5u);
                float primaryFraction = Mathf.Lerp(
                    0.40f,
                    0.60f,
                    Hash01(primaryId, 61u));
                primary.Add(new WeakSpanOpportunity(
                    primaryId,
                    path.StableId,
                    points,
                    length,
                    endpointClearance + usableLength * primaryFraction,
                    Hash01(primaryId, 62u)));

                if (length >= SecondaryWeakSpanMinimumLengthMetres &&
                    usableLength >= MinimumWeakSpanSpacingMetres * 2f)
                {
                    uint secondaryId = MixBits(
                        path.StableId ^ (uint)seed ^ 0x9FB21C65u);
                    bool placeUpstream = primaryFraction >= 0.5f;
                    float secondaryFraction = placeUpstream
                        ? Mathf.Lerp(0.16f, 0.31f, Hash01(secondaryId, 63u))
                        : Mathf.Lerp(0.69f, 0.84f, Hash01(secondaryId, 63u));
                    secondary.Add(new WeakSpanOpportunity(
                        secondaryId,
                        path.StableId,
                        points,
                        length,
                        endpointClearance + usableLength * secondaryFraction,
                        Hash01(secondaryId, 64u)));
                }
            }

            primary.Sort(CompareWeakSpanOpportunity);
            secondary.Sort(CompareWeakSpanOpportunity);
            List<WeakSpanOpportunity> opportunities = new(
                primary.Count + secondary.Count);
            opportunities.AddRange(primary);
            opportunities.AddRange(secondary);

            List<PreparedNegativeRegion> prepared = new();
            Dictionary<uint, List<AcceptedWeakSpan>> acceptedByConnector =
                new();
            float[] raster = new float[validCells.Length];
            for (int opportunityIndex = 0;
                 opportunityIndex < opportunities.Count;
                 opportunityIndex++)
            {
                WeakSpanOpportunity opportunity =
                    opportunities[opportunityIndex];
                if (!SamplePolyline(
                        opportunity.Points,
                        opportunity.DistanceAlongPath,
                        out Vector2 position,
                        out Vector2 tangent))
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason
                            .NoUsableConnectorSpan]++;
                    continue;
                }

                uint stableId = opportunity.StableId;
                float cutSelector = Hash01(stableId, 65u);
                float alongRadius = Mathf.Lerp(
                    0.17f,
                    0.34f,
                    Hash01(stableId, 66u));
                float acrossRadius = Mathf.Lerp(
                    0.17f,
                    0.29f,
                    cutSelector);
                float strength = Mathf.Lerp(0.52f, 1.0f, cutSelector);

                if (!acceptedByConnector.TryGetValue(
                        opportunity.ConnectorId,
                        out List<AcceptedWeakSpan> accepted))
                {
                    accepted = new List<AcceptedWeakSpan>();
                    acceptedByConnector.Add(
                        opportunity.ConnectorId,
                        accepted);
                }

                bool spacingViolation = false;
                for (int acceptedIndex = 0;
                     acceptedIndex < accepted.Count;
                     acceptedIndex++)
                {
                    AcceptedWeakSpan other = accepted[acceptedIndex];
                    float required = Mathf.Max(
                        MinimumWeakSpanSpacingMetres,
                        alongRadius + other.AlongRadius);
                    if (Mathf.Abs(
                            opportunity.DistanceAlongPath -
                            other.DistanceAlongPath) < required)
                    {
                        spacingViolation = true;
                        break;
                    }
                }
                if (spacingViolation)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason
                            .ConnectorSpanSpacing]++;
                    continue;
                }

                Array.Clear(raster, 0, raster.Length);
                int coverage = RasterizeConnectorWeakSpan(
                    stableId,
                    position,
                    tangent,
                    alongRadius,
                    acrossRadius,
                    strength,
                    validCells,
                    majorSupport,
                    connectorSupport,
                    metricPositions,
                    raster);
                if (coverage <= 0)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason
                            .NoRasterCoverage]++;
                    continue;
                }

                accepted.Add(new AcceptedWeakSpan(
                    opportunity.DistanceAlongPath,
                    alongRadius));
                prepared.Add(new PreparedNegativeRegion(
                    StylizedRiverFoamNegativeRegionClass.ConnectorWeakSpan,
                    stableId,
                    opportunity.ConnectorId,
                    position,
                    ResolveAcrossNormalized(domain, position),
                    Mathf.Atan2(tangent.y, tangent.x),
                    alongRadius,
                    acrossRadius,
                    Vector2.zero,
                    CaptureRaster(raster)));
            }

            return prepared;
        }

        private static List<PreparedNegativeRegion>
            BuildFreeWaterNegativeEventRegions(
                RiverDomainSnapshot domain,
                int width,
                int height,
                float fieldLength,
                float validFieldLength,
                int seed,
                bool[] validCells,
                float[] fluidCoverage,
                float[] majorSupport,
                float[] connectorSupport,
                Vector2[] metricPositions,
                int[] rejectionCounts,
                out int opportunityCount)
        {
            List<FreeWaterOpportunity> opportunities =
                BuildFreeWaterOpportunities(
                    domain,
                    width,
                    height,
                    fieldLength,
                    validFieldLength,
                    seed,
                    validCells,
                    fluidCoverage,
                    majorSupport,
                    connectorSupport,
                    metricPositions,
                    rejectionCounts);
            opportunityCount = opportunities.Count;
            opportunities.Sort(CompareFreeWaterOpportunity);

            List<PreparedNegativeRegion> prepared = new();
            List<AcceptedFreeWaterEvent> accepted = new();
            float[] raster = new float[validCells.Length];
            for (int opportunityIndex = 0;
                 opportunityIndex < opportunities.Count;
                 opportunityIndex++)
            {
                FreeWaterOpportunity opportunity =
                    opportunities[opportunityIndex];
                uint stableId = opportunity.StableId;
                float sizeSelector = Hash01(stableId, 75u);
                float sizeScale;
                if (sizeSelector < 0.34f)
                {
                    sizeScale = Mathf.Lerp(
                        FreeWaterSmallScaleMinimum,
                        FreeWaterSmallScaleMaximum,
                        sizeSelector / 0.34f);
                }
                else if (sizeSelector < 0.78f)
                {
                    sizeScale = Mathf.Lerp(
                        FreeWaterMediumScaleMinimum,
                        FreeWaterMediumScaleMaximum,
                        (sizeSelector - 0.34f) / 0.44f);
                }
                else
                {
                    sizeScale = Mathf.Lerp(
                        FreeWaterLargeScaleMinimum,
                        FreeWaterLargeScaleMaximum,
                        (sizeSelector - 0.78f) / 0.22f);
                }

                float alongRadius = Mathf.Lerp(
                    MinimumFreeWaterAlongRadiusMetres,
                    MaximumFreeWaterAlongRadiusMetres,
                    Hash01(stableId, 76u)) * sizeScale;
                float acrossRadius = Mathf.Lerp(
                    MinimumFreeWaterAcrossRadiusMetres,
                    MaximumFreeWaterAcrossRadiusMetres,
                    Hash01(stableId, 77u)) * sizeScale;
                float aspectSelector = Hash01(stableId, 78u);
                alongRadius *= Mathf.Lerp(0.80f, 1.24f, aspectSelector);
                acrossRadius *= Mathf.Lerp(1.18f, 0.72f, aspectSelector);
                alongRadius = Mathf.Clamp(alongRadius, 0.18f, 0.92f);
                acrossRadius = Mathf.Clamp(acrossRadius, 0.11f, 0.62f);
                acrossRadius = Mathf.Min(acrossRadius, alongRadius * 0.86f);
                float largestRadius = Mathf.Max(alongRadius, acrossRadius);

                bool spacingViolation = false;
                for (int acceptedIndex = 0;
                     acceptedIndex < accepted.Count;
                     acceptedIndex++)
                {
                    AcceptedFreeWaterEvent other = accepted[acceptedIndex];
                    float required = Mathf.Max(
                        MinimumFreeWaterSpacingMetres,
                        (largestRadius + other.LargestRadius) * 0.72f);
                    if (Vector2.Distance(
                            opportunity.Position,
                            other.Position) < required)
                    {
                        spacingViolation = true;
                        break;
                    }
                }
                if (spacingViolation)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason
                            .FreeWaterSpacing]++;
                    continue;
                }

                float orientation = Mathf.Lerp(
                    -MaximumFreeWaterOrientationRadians,
                    MaximumFreeWaterOrientationRadians,
                    Hash01(stableId, 79u));
                float strength = Mathf.Lerp(
                    0.58f,
                    0.92f,
                    Hash01(stableId, 80u));
                Array.Clear(raster, 0, raster.Length);
                int coverage = RasterizeFreeWaterNegativeEvent(
                    stableId,
                    opportunity.Position,
                    orientation,
                    alongRadius,
                    acrossRadius,
                    strength,
                    validCells,
                    metricPositions,
                    raster);
                if (coverage <= 0)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason
                            .NoRasterCoverage]++;
                    continue;
                }

                accepted.Add(new AcceptedFreeWaterEvent(
                    opportunity.Position,
                    largestRadius));
                prepared.Add(new PreparedNegativeRegion(
                    StylizedRiverFoamNegativeRegionClass
                        .FreeWaterNegativeEvent,
                    stableId,
                    InvalidHostId,
                    opportunity.Position,
                    ResolveAcrossNormalized(domain, opportunity.Position),
                    orientation,
                    alongRadius,
                    acrossRadius,
                    Vector2.zero,
                    CaptureRaster(raster)));
            }

            return prepared;
        }

        private static List<FreeWaterOpportunity> BuildFreeWaterOpportunities(
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            int seed,
            bool[] validCells,
            float[] fluidCoverage,
            float[] majorSupport,
            float[] connectorSupport,
            Vector2[] metricPositions,
            int[] rejectionCounts)
        {
            List<FreeWaterOpportunity> opportunities = new();
            if (domain == null || !domain.IsValid ||
                width < 2 || height < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                return opportunities;
            }

            float minimumDistance = Mathf.Min(
                FreeWaterLongitudinalMarginMetres,
                validFieldLength * 0.20f);
            float maximumDistance = Mathf.Max(
                minimumDistance,
                validFieldLength - minimumDistance);
            float alongOffset = (
                Hash01((uint)(seed + 1), 71u) - 0.5f) *
                FreeWaterLatticeSpacingMetres * 0.36f;
            List<float> alongDistances = new();
            for (int alongSlot = 0; alongSlot < 4096; alongSlot++)
            {
                float distance = minimumDistance +
                    FreeWaterLatticeSpacingMetres * 0.5f +
                    alongOffset +
                    alongSlot * FreeWaterLatticeSpacingMetres;
                if (distance > maximumDistance + 0.0001f)
                {
                    break;
                }

                if (distance >= minimumDistance - 0.0001f)
                {
                    alongDistances.Add(distance);
                }
            }

            if (alongDistances.Count == 0)
            {
                alongDistances.Add(validFieldLength * 0.5f);
            }

            for (int alongSlot = 0;
                 alongSlot < alongDistances.Count;
                 alongSlot++)
            {
                float distance = Mathf.Clamp(
                    alongDistances[alongSlot],
                    0f,
                    validFieldLength);
                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(distance);
                float leftWidth = Mathf.Max(
                    0.05f,
                    sample.LeftSurfaceHalfWidth);
                float rightWidth = Mathf.Max(
                    0.05f,
                    sample.RightSurfaceHalfWidth);
                float lateralMinimum = -leftWidth +
                    FreeWaterLateralMarginMetres;
                float lateralMaximum = rightWidth -
                    FreeWaterLateralMarginMetres;
                if (lateralMaximum < lateralMinimum)
                {
                    float centre = (rightWidth - leftWidth) * 0.5f;
                    lateralMinimum = centre;
                    lateralMaximum = centre;
                }

                uint rowId = MixBits(
                    (uint)(seed + 1) * 0x9E3779B9u ^
                    (uint)(alongSlot + 1) * 0x85EBCA6Bu);
                float lateralOffset = (
                    Hash01(rowId, 72u) - 0.5f) *
                    FreeWaterLatticeSpacingMetres * 0.34f;
                float firstLateral = lateralMinimum +
                    FreeWaterLatticeSpacingMetres * 0.5f +
                    lateralOffset;
                int rowOpportunityCount = 0;
                for (int acrossSlot = 0; acrossSlot < 1024; acrossSlot++)
                {
                    float lateral = firstLateral +
                        acrossSlot * FreeWaterLatticeSpacingMetres;
                    if (lateral > lateralMaximum + 0.0001f)
                    {
                        break;
                    }

                    TryAddFreeWaterOpportunity(
                        opportunities,
                        domain,
                        width,
                        height,
                        fieldLength,
                        seed,
                        alongSlot,
                        acrossSlot,
                        new Vector2(distance, lateral),
                        validCells,
                        fluidCoverage,
                        majorSupport,
                        connectorSupport,
                        metricPositions,
                        rejectionCounts);
                    rowOpportunityCount++;
                }

                if (rowOpportunityCount == 0)
                {
                    TryAddFreeWaterOpportunity(
                        opportunities,
                        domain,
                        width,
                        height,
                        fieldLength,
                        seed,
                        alongSlot,
                        0,
                        new Vector2(
                            distance,
                            Mathf.Lerp(lateralMinimum, lateralMaximum, 0.5f)),
                        validCells,
                        fluidCoverage,
                        majorSupport,
                        connectorSupport,
                        metricPositions,
                        rejectionCounts);
                }
            }

            return opportunities;
        }

        private static void TryAddFreeWaterOpportunity(
            List<FreeWaterOpportunity> opportunities,
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            int seed,
            int alongSlot,
            int acrossSlot,
            Vector2 proposedPosition,
            bool[] validCells,
            float[] fluidCoverage,
            float[] majorSupport,
            float[] connectorSupport,
            Vector2[] metricPositions,
            int[] rejectionCounts)
        {
            uint stableId = MixBits(
                (uint)(seed + 1) * 0xD1B54A35u ^
                (uint)(alongSlot + 1) * 0x94D049BBu ^
                (uint)(acrossSlot + 1) * 0x369DEA0Fu);
            Vector2 jitter = new Vector2(
                Hash01(stableId, 73u) - 0.5f,
                Hash01(stableId, 74u) - 0.5f) *
                (FreeWaterLatticeSpacingMetres * 0.30f);
            proposedPosition += jitter;
            proposedPosition.x = Mathf.Clamp(
                proposedPosition.x,
                0f,
                domain.LocalLength);

            StylizedRiverSplineSample sample =
                domain.SampleAtOrientedDistance(proposedPosition.x);
            float leftWidth = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
            float rightWidth = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
            float lateralMinimum = -leftWidth +
                FreeWaterLateralMarginMetres;
            float lateralMaximum = rightWidth -
                FreeWaterLateralMarginMetres;
            if (lateralMaximum < lateralMinimum)
            {
                float centre = (rightWidth - leftWidth) * 0.5f;
                lateralMinimum = centre;
                lateralMaximum = centre;
            }
            proposedPosition.y = Mathf.Clamp(
                proposedPosition.y,
                lateralMinimum,
                lateralMaximum);

            if (!TryResolveFreeWaterCell(
                    proposedPosition,
                    width,
                    height,
                    fieldLength,
                    leftWidth,
                    rightWidth,
                    validCells,
                    fluidCoverage,
                    metricPositions,
                    out int cellIndex,
                    out Vector2 resolvedPosition))
            {
                rejectionCounts[(int)
                    StylizedRiverFoamPocketRejectionReason
                        .NoUsableFreeWater]++;
                return;
            }

            float positiveSupport = Mathf.Max(
                majorSupport[cellIndex],
                connectorSupport[cellIndex]);
            float edgePenalty = 1f - Mathf.Clamp01(fluidCoverage[cellIndex]);
            float stableTieBreak = Hash01(stableId, 79u);
            float activationRank = Mathf.Clamp01(
                positiveSupport * 0.58f +
                edgePenalty * 0.24f +
                stableTieBreak * 0.18f);
            opportunities.Add(new FreeWaterOpportunity(
                stableId,
                resolvedPosition,
                activationRank));
        }

        private static bool TryResolveFreeWaterCell(
            Vector2 position,
            int width,
            int height,
            float fieldLength,
            float leftWidth,
            float rightWidth,
            bool[] validCells,
            float[] fluidCoverage,
            Vector2[] metricPositions,
            out int cellIndex,
            out Vector2 resolvedPosition)
        {
            cellIndex = -1;
            resolvedPosition = position;
            int approximateX = Mathf.Clamp(
                Mathf.RoundToInt(
                    position.x / Mathf.Max(0.0001f, fieldLength) *
                    (width - 1)),
                0,
                width - 1);
            float across01 = position.y < 0f
                ? Mathf.Lerp(
                    0.5f,
                    0f,
                    Mathf.Clamp01(-position.y / leftWidth))
                : Mathf.Lerp(
                    0.5f,
                    1f,
                    Mathf.Clamp01(position.y / rightWidth));
            int approximateY = Mathf.Clamp(
                Mathf.RoundToInt(across01 * (height - 1)),
                0,
                height - 1);
            int approximateIndex = approximateX + approximateY * width;
            if (validCells[approximateIndex])
            {
                cellIndex = approximateIndex;
                resolvedPosition = metricPositions[approximateIndex];
                return true;
            }

            // A fluid cell rejected at the nearest texel is inside the exact
            // obstacle mask. Do not slide the event around that obstacle.
            if (fluidCoverage[approximateIndex] > 0.001f)
            {
                return false;
            }

            float maximumDistanceSquared =
                FreeWaterNearestSampleRadiusMetres *
                FreeWaterNearestSampleRadiusMetres;
            float bestDistanceSquared = float.PositiveInfinity;
            const int searchRadius = 2;
            for (int offsetY = -searchRadius;
                 offsetY <= searchRadius;
                 offsetY++)
            {
                int y = approximateY + offsetY;
                if (y < 0 || y >= height)
                {
                    continue;
                }

                for (int offsetX = -searchRadius;
                     offsetX <= searchRadius;
                     offsetX++)
                {
                    int x = approximateX + offsetX;
                    if (x < 0 || x >= width)
                    {
                        continue;
                    }

                    int index = x + y * width;
                    if (!validCells[index])
                    {
                        continue;
                    }

                    float distanceSquared = (
                        metricPositions[index] - position).sqrMagnitude;
                    if (distanceSquared > maximumDistanceSquared ||
                        distanceSquared >= bestDistanceSquared)
                    {
                        continue;
                    }

                    bestDistanceSquared = distanceSquared;
                    cellIndex = index;
                    resolvedPosition = metricPositions[index];
                }
            }

            return cellIndex >= 0;
        }

        private static int CompareFreeWaterOpportunity(
            FreeWaterOpportunity a,
            FreeWaterOpportunity b)
        {
            int rank = a.ActivationRank.CompareTo(b.ActivationRank);
            return rank != 0 ? rank : a.StableId.CompareTo(b.StableId);
        }

        private static int RasterizeFreeWaterNegativeEvent(
            uint stableId,
            Vector2 centre,
            float orientation,
            float alongRadius,
            float acrossRadius,
            float strength,
            bool[] validCells,
            Vector2[] metricPositions,
            float[] destination)
        {
            float cos = Mathf.Cos(orientation);
            float sin = Mathf.Sin(orientation);
            float outerRadius = Mathf.Max(alongRadius, acrossRadius) * 1.45f;
            float outerRadiusSquared = outerRadius * outerRadius;
            uint noiseSeed = MixBits(stableId ^ 0xA6E8FEB9u);
            int covered = 0;
            for (int index = 0; index < destination.Length; index++)
            {
                if (!validCells[index])
                {
                    continue;
                }

                Vector2 delta = metricPositions[index] - centre;
                if (delta.sqrMagnitude > outerRadiusSquared)
                {
                    continue;
                }

                float localX = cos * delta.x + sin * delta.y;
                float localY = -sin * delta.x + cos * delta.y;
                float normalizedX = localX /
                    Mathf.Max(0.001f, alongRadius);
                float normalizedY = localY /
                    Mathf.Max(0.001f, acrossRadius);
                float shape = EvaluateIrregularShape(
                    normalizedX,
                    normalizedY,
                    noiseSeed);
                if (shape <= 0.001f)
                {
                    continue;
                }

                float downstreamBias = Mathf.Lerp(
                    0.84f,
                    1f,
                    SmoothStep(-0.9f, 0.9f, normalizedX));
                float value = Mathf.Clamp01(
                    shape * downstreamBias * strength);
                destination[index] = Mathf.Max(destination[index], value);
                if (value >= PocketCoverageThreshold)
                {
                    covered++;
                }
            }

            return covered;
        }

        private static int CompareWeakSpanOpportunity(
            WeakSpanOpportunity a,
            WeakSpanOpportunity b)
        {
            int rank = a.ActivationRank.CompareTo(b.ActivationRank);
            if (rank != 0)
            {
                return rank;
            }
            return a.StableId.CompareTo(b.StableId);
        }

        private static int RasterizeConnectorWeakSpan(
            uint stableId,
            Vector2 centre,
            Vector2 tangent,
            float alongRadius,
            float acrossRadius,
            float strength,
            bool[] validCells,
            float[] majorSupport,
            float[] connectorSupport,
            Vector2[] metricPositions,
            float[] destination)
        {
            Vector2 normal = new Vector2(-tangent.y, tangent.x);
            int covered = 0;
            for (int index = 0; index < destination.Length; index++)
            {
                if (!validCells[index] ||
                    majorSupport[index] >= WeakSpanMajorProtectionThreshold ||
                    connectorSupport[index] < WeakSpanConnectorThreshold)
                {
                    continue;
                }

                Vector2 delta = metricPositions[index] - centre;
                float along = Vector2.Dot(delta, tangent) /
                    Mathf.Max(0.0001f, alongRadius);
                float across = Vector2.Dot(delta, normal) /
                    Mathf.Max(0.0001f, acrossRadius);
                float radial = Mathf.Sqrt(along * along + across * across);
                if (radial >= 1f)
                {
                    continue;
                }

                Vector2 noisePosition = metricPositions[index] * 4.2f;
                float noise = ValueNoise(
                    noisePosition.x,
                    noisePosition.y,
                    stableId ^ 0x6C8E9CF5u);
                radial += (noise - 0.5f) * 0.16f;
                float envelope = 1f - SmoothStep(0.54f, 1f, radial);
                float connectorMask = SmoothStep(
                    WeakSpanConnectorThreshold,
                    0.20f,
                    connectorSupport[index]);
                float majorProtection = 1f - SmoothStep(
                    0.10f,
                    WeakSpanMajorProtectionThreshold,
                    majorSupport[index]);
                float value = envelope * strength * connectorMask *
                    majorProtection;
                if (value <= 0.0001f)
                {
                    continue;
                }

                destination[index] = Mathf.Max(destination[index], value);
                if (value >= PocketCoverageThreshold)
                {
                    covered++;
                }
            }

            return covered;
        }

        private static float MeasurePolyline(Vector2[] points)
        {
            if (points == null || points.Length < 2)
            {
                return 0f;
            }

            float length = 0f;
            for (int index = 1; index < points.Length; index++)
            {
                length += Vector2.Distance(points[index - 1], points[index]);
            }
            return length;
        }

        private static bool SamplePolyline(
            Vector2[] points,
            float distance,
            out Vector2 position,
            out Vector2 tangent)
        {
            position = Vector2.zero;
            tangent = Vector2.right;
            if (points == null || points.Length < 2)
            {
                return false;
            }

            float remaining = Mathf.Max(0f, distance);
            for (int index = 1; index < points.Length; index++)
            {
                Vector2 segment = points[index] - points[index - 1];
                float length = segment.magnitude;
                if (length <= 0.0001f)
                {
                    continue;
                }

                if (remaining <= length || index == points.Length - 1)
                {
                    float t = Mathf.Clamp01(remaining / length);
                    position = Vector2.Lerp(points[index - 1], points[index], t);
                    tangent = segment / length;
                    return true;
                }

                remaining -= length;
            }

            return false;
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

        private static float RemapFreeWaterAmount(float amount)
        {
            amount = Mathf.Clamp01(amount);
            if (amount <= 0.0001f)
            {
                return 0f;
            }

            if (amount >= 0.9999f)
            {
                return 1f;
            }

            // Free-Water Events should remain sparse and subordinate at the
            // default midpoint. The steeper remap keeps the low-to-mid range
            // restrained while preserving the full feasible population at 1.0.
            return Mathf.Pow(amount, 2.85f);
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

        private static void MergePreparedPrefix(
            List<PreparedNegativeRegion> prepared,
            int selectedCount,
            float[] destination)
        {
            selectedCount = Mathf.Clamp(
                selectedCount,
                0,
                prepared != null ? prepared.Count : 0);
            if (destination == null)
            {
                return;
            }

            for (int index = 0; index < selectedCount; index++)
            {
                RasterSample[] samples = prepared[index].Samples;
                for (int sampleIndex = 0;
                     sampleIndex < samples.Length;
                     sampleIndex++)
                {
                    RasterSample sample = samples[sampleIndex];
                    if (sample.Index < 0 || sample.Index >= destination.Length)
                    {
                        continue;
                    }

                    destination[sample.Index] = Mathf.Max(
                        destination[sample.Index],
                        sample.Value);
                }
            }
        }

        private static void MergePreparedRegion(
            PreparedNegativeRegion region,
            float[] destination)
        {
            if (destination == null)
            {
                return;
            }

            RasterSample[] samples = region.Samples;
            for (int sampleIndex = 0;
                 sampleIndex < samples.Length;
                 sampleIndex++)
            {
                RasterSample sample = samples[sampleIndex];
                if (sample.Index < 0 || sample.Index >= destination.Length)
                {
                    continue;
                }

                destination[sample.Index] = Mathf.Max(
                    destination[sample.Index],
                    sample.Value);
            }
        }

        private static StylizedRiverFoamPreparedHostedNegativeRegion[]
            BuildPreparedHostedRegions(
                List<PreparedNegativeRegion> interiorRegions,
                int selectedInteriorCount,
                List<PreparedNegativeRegion> cavityRegions,
                int selectedCavityCount,
                int width,
                int height,
                float fieldLength,
                float validFieldLength,
                RiverDomainSnapshot domain,
                StylizedRiverFoamMajorTopology majorTopology)
        {
            List<StylizedRiverFoamPreparedHostedNegativeRegion> result = new();
            if (majorTopology == null)
            {
                return result.ToArray();
            }

            AppendPreparedHostedRegions(
                result,
                interiorRegions,
                selectedInteriorCount,
                width,
                height,
                fieldLength,
                validFieldLength,
                domain,
                majorTopology);
            AppendPreparedHostedRegions(
                result,
                cavityRegions,
                selectedCavityCount,
                width,
                height,
                fieldLength,
                validFieldLength,
                domain,
                majorTopology);
            return result.ToArray();
        }

        private static void MergeUnpreparedHostedPrefix(
            List<PreparedNegativeRegion> prepared,
            int selectedCount,
            IReadOnlyList<StylizedRiverFoamPreparedHostedNegativeRegion>
                preparedHostedRegions,
            float[] destination)
        {
            selectedCount = Mathf.Clamp(
                selectedCount,
                0,
                prepared != null ? prepared.Count : 0);
            if (destination == null)
            {
                return;
            }

            for (int index = 0; index < selectedCount; index++)
            {
                PreparedNegativeRegion region = prepared[index];
                bool hasPreparedRuntimeRegion = false;
                if (preparedHostedRegions != null)
                {
                    for (int preparedIndex = 0;
                         preparedIndex < preparedHostedRegions.Count;
                         preparedIndex++)
                    {
                        StylizedRiverFoamPreparedHostedNegativeRegion candidate =
                            preparedHostedRegions[preparedIndex];
                        if (candidate.StableId == region.StableId &&
                            candidate.RegionClass == region.RegionClass)
                        {
                            hasPreparedRuntimeRegion = true;
                            break;
                        }
                    }
                }

                if (!hasPreparedRuntimeRegion)
                {
                    MergePreparedRegion(region, destination);
                }
            }
        }

        private static StylizedRiverFoamPreparedFreeWaterRegion[]
            BuildPreparedFreeWaterRegions(
                List<PreparedNegativeRegion> prepared,
                int selectedCount,
                int width,
                int height,
                float fieldLength,
                float validFieldLength,
                RiverDomainSnapshot domain,
                float[] fluidCoverage,
                float[] obstacleMask)
        {
            selectedCount = Mathf.Clamp(
                selectedCount,
                0,
                prepared != null ? prepared.Count : 0);
            List<StylizedRiverFoamPreparedFreeWaterRegion> result = new();
            if (selectedCount == 0 || width <= 1 || height <= 1 ||
                fieldLength <= 0.0001f || domain == null || !domain.IsValid)
            {
                return result.ToArray();
            }

            for (int index = 0; index < selectedCount; index++)
            {
                PreparedNegativeRegion region = prepared[index];
                if (region.RegionClass !=
                    StylizedRiverFoamNegativeRegionClass.FreeWaterNegativeEvent)
                {
                    continue;
                }

                int resolution = FreeWaterEvolutionMaskResolution;
                float[] localPressure = new float[resolution * resolution];
                float outerRadius = Mathf.Max(
                    region.AlongRadius,
                    region.AcrossRadius) * FreeWaterEvolutionPadding;
                float metresPerCell = Mathf.Max(
                    0.001f,
                    outerRadius * 2f / resolution);
                ResampleFreeWaterRegionToLocalMask(
                    region,
                    localPressure,
                    resolution,
                    metresPerCell,
                    width,
                    height,
                    fieldLength,
                    validFieldLength,
                    domain);
                if (!ContainsPreparedPressure(localPressure))
                {
                    continue;
                }

                StylizedRiverFoamFreeWaterRecycleAnchor[] recycleAnchors =
                    BuildFreeWaterRecycleAnchors(
                        region,
                        localPressure,
                        resolution,
                        metresPerCell,
                        width,
                        height,
                        fieldLength,
                        validFieldLength,
                        domain,
                        fluidCoverage,
                        obstacleMask);
                if (recycleAnchors.Length == 0)
                {
                    continue;
                }

                result.Add(new StylizedRiverFoamPreparedFreeWaterRegion(
                    region.StableId,
                    resolution,
                    localPressure,
                    region.Position.x,
                    region.AcrossNormalized,
                    region.Orientation,
                    metresPerCell,
                    recycleAnchors));
            }

            return result.ToArray();
        }

        private static StylizedRiverFoamFreeWaterRecycleAnchor[]
            BuildFreeWaterRecycleAnchors(
                PreparedNegativeRegion region,
                float[] localPressure,
                int resolution,
                float metresPerCell,
                int width,
                int height,
                float fieldLength,
                float validFieldLength,
                RiverDomainSnapshot domain,
                float[] fluidCoverage,
                float[] obstacleMask)
        {
            List<StylizedRiverFoamFreeWaterRecycleAnchor> anchors = new(
                MaximumFreeWaterRecycleAnchorCount);
            float egressStart = StylizedRiverFoamMajorTopology
                .ResolveEvolutionEgressStart(validFieldLength);
            float minimumRunway = Mathf.Min(
                FreeWaterMinimumRecycleRunwayMetres,
                Mathf.Max(0.20f, egressStart * 0.30f));
            float maximumSafeDistance = Mathf.Max(
                0f,
                egressStart - minimumRunway);
            float homeDistance = Mathf.Min(
                region.Position.x,
                maximumSafeDistance);
            float territoryStart = Mathf.Clamp(
                homeDistance - FreeWaterRecycleTerritoryMetres,
                0f,
                maximumSafeDistance);
            float territoryEnd = Mathf.Clamp(
                homeDistance + FreeWaterRecycleTerritoryMetres * 0.35f,
                territoryStart,
                maximumSafeDistance);

            for (int attempt = 0;
                 attempt < FreeWaterRecycleAnchorAttemptCount &&
                 anchors.Count < MaximumFreeWaterRecycleAnchorCount;
                 attempt++)
            {
                uint attemptSeed = MixBits(
                    region.StableId ^
                    ((uint)(attempt + 1) * 0x9E3779B9u) ^
                    0xD1B54A35u);
                float alongSelector = Mathf.Repeat(
                    Hash01(attemptSeed, 1u) +
                    attempt * GoldenRatioConjugate,
                    1f);
                float centreLocalDistance = Mathf.Lerp(
                    territoryStart,
                    territoryEnd,
                    alongSelector);
                int laneIndex = attempt %
                    FreeWaterRecycleLateralOffsets.Length;
                float centreAcrossNormalized = Mathf.Clamp(
                    region.AcrossNormalized +
                    FreeWaterRecycleLateralOffsets[laneIndex] +
                    Mathf.Lerp(
                        -0.055f,
                        0.055f,
                        Hash01(attemptSeed, 2u)),
                    -0.84f,
                    0.84f);
                float orientation = Mathf.Repeat(
                    region.Orientation +
                    Mathf.Lerp(
                        -0.18f,
                        0.18f,
                        Hash01(attemptSeed, 3u)) +
                    Mathf.PI * 0.5f,
                    Mathf.PI) - Mathf.PI * 0.5f;
                if (!IsFreeWaterRecycleAnchorValid(
                        localPressure,
                        resolution,
                        metresPerCell,
                        centreLocalDistance,
                        centreAcrossNormalized,
                        orientation,
                        width,
                        height,
                        fieldLength,
                        validFieldLength,
                        domain,
                        fluidCoverage,
                        obstacleMask) ||
                    IsNearExistingFreeWaterRecycleAnchor(
                        anchors,
                        centreLocalDistance,
                        centreAcrossNormalized))
                {
                    continue;
                }

                anchors.Add(new StylizedRiverFoamFreeWaterRecycleAnchor(
                    centreLocalDistance,
                    centreAcrossNormalized,
                    orientation,
                    false));
            }

            if (anchors.Count > 0)
            {
                return anchors.ToArray();
            }

            if (IsFreeWaterRecycleAnchorValid(
                    localPressure,
                    resolution,
                    metresPerCell,
                    homeDistance,
                    region.AcrossNormalized,
                    region.Orientation,
                    width,
                    height,
                    fieldLength,
                    validFieldLength,
                    domain,
                    fluidCoverage,
                    obstacleMask))
            {
                return new[]
                {
                    new StylizedRiverFoamFreeWaterRecycleAnchor(
                        homeDistance,
                        region.AcrossNormalized,
                        region.Orientation,
                        true)
                };
            }

            // Do not invent an unvalidated shifted fallback. The accepted
            // static event remains available through the existing unavailable-
            // evolution path when no safe upstream recycle anchor can be
            // prepared. Normal gameplay still performs no search or retry.
            return Array.Empty<
                StylizedRiverFoamFreeWaterRecycleAnchor>();
        }

        private static bool IsNearExistingFreeWaterRecycleAnchor(
            List<StylizedRiverFoamFreeWaterRecycleAnchor> anchors,
            float localDistance,
            float acrossNormalized)
        {
            for (int index = 0; index < anchors.Count; index++)
            {
                StylizedRiverFoamFreeWaterRecycleAnchor anchor =
                    anchors[index];
                if (Mathf.Abs(
                        anchor.CentreLocalDistance - localDistance) < 0.26f &&
                    Mathf.Abs(
                        anchor.CentreAcrossNormalized - acrossNormalized) <
                        0.12f)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFreeWaterRecycleAnchorValid(
            float[] localPressure,
            int resolution,
            float metresPerCell,
            float centreLocalDistance,
            float centreAcrossNormalized,
            float orientationRadians,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            RiverDomainSnapshot domain,
            float[] fluidCoverage,
            float[] obstacleMask)
        {
            if (localPressure == null ||
                localPressure.Length < resolution * resolution ||
                resolution <= 0 || metresPerCell <= 0.0001f ||
                domain == null || !domain.IsValid ||
                fluidCoverage == null ||
                fluidCoverage.Length < width * height)
            {
                return false;
            }

            float centre = resolution * 0.5f;
            float cosine = Mathf.Cos(orientationRadians);
            float sine = Mathf.Sin(orientationRadians);
            float totalWeight = 0f;
            float effectiveWeight = 0f;
            float bankWeight = 0f;
            float obstacleWeight = 0f;
            float outsideWeight = 0f;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float pressure = localPressure[x + y * resolution];
                    if (pressure <= FreeWaterAnchorPressureThreshold)
                    {
                        continue;
                    }

                    float localX = (x + 0.5f - centre) * metresPerCell;
                    float localY = (y + 0.5f - centre) * metresPerCell;
                    float deltaAlong =
                        cosine * localX - sine * localY;
                    float deltaAcross =
                        sine * localX + cosine * localY;
                    float localDistance = centreLocalDistance + deltaAlong;
                    totalWeight += pressure;
                    if (localDistance < 0f ||
                        localDistance > validFieldLength)
                    {
                        outsideWeight += pressure;
                        continue;
                    }

                    StylizedRiverSplineSample sample =
                        domain.SampleAtOrientedDistance(localDistance);
                    float centreAcrossMetres = SignedNormalizedToMetres(
                        centreAcrossNormalized,
                        Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth),
                        Mathf.Max(0.05f, sample.RightSurfaceHalfWidth));
                    Vector2 metricPosition = new Vector2(
                        localDistance,
                        centreAcrossMetres + deltaAcross);
                    float fluid = SampleRegionSourcePressure(
                        fluidCoverage,
                        width,
                        height,
                        fieldLength,
                        validFieldLength,
                        metricPosition,
                        domain);
                    float obstacle = obstacleMask != null &&
                        obstacleMask.Length >= width * height
                        ? SampleRegionSourcePressure(
                            obstacleMask,
                            width,
                            height,
                            fieldLength,
                            validFieldLength,
                            metricPosition,
                            domain)
                        : 0f;
                    effectiveWeight += pressure * fluid * (1f - obstacle);
                    bankWeight += pressure * (1f - fluid);
                    obstacleWeight += pressure * obstacle;
                }
            }

            if (totalWeight <= 0.0001f)
            {
                return false;
            }

            float effectiveRatio = effectiveWeight / totalWeight;
            float bankRatio = bankWeight / totalWeight;
            float obstacleRatio = obstacleWeight / totalWeight;
            float outsideRatio = outsideWeight / totalWeight;
            return effectiveRatio >= 0.70f &&
                bankRatio <= 0.22f &&
                obstacleRatio <= 0.055f &&
                outsideRatio <= 0.08f;
        }

        private static void AppendPreparedHostedRegions(
            List<StylizedRiverFoamPreparedHostedNegativeRegion> destination,
            List<PreparedNegativeRegion> prepared,
            int selectedCount,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            RiverDomainSnapshot domain,
            StylizedRiverFoamMajorTopology majorTopology)
        {
            selectedCount = Mathf.Clamp(
                selectedCount,
                0,
                prepared != null ? prepared.Count : 0);
            IReadOnlyList<StylizedRiverFoamMajorRegion> majorRegions =
                majorTopology.Regions;
            IReadOnlyList<StylizedRiverFoamPreparedMajorRegion> majorPrepared =
                majorTopology.PreparedRegions;

            for (int index = 0; index < selectedCount; index++)
            {
                PreparedNegativeRegion region = prepared[index];
                int hostIndex = FindMajorHostIndex(
                    region.HostRegionId,
                    majorRegions,
                    majorPrepared);
                if (hostIndex < 0)
                {
                    continue;
                }

                StylizedRiverFoamMajorRegion hostRegion = majorRegions[hostIndex];
                StylizedRiverFoamPreparedMajorRegion hostPrepared =
                    majorPrepared[hostIndex];
                int resolution = hostPrepared.MaskResolution;
                float[] localPressure = new float[resolution * resolution];
                ResampleHostedRegionToLocalMask(
                    region,
                    localPressure,
                    resolution,
                    width,
                    height,
                    fieldLength,
                    validFieldLength,
                    domain,
                    hostRegion,
                    hostPrepared);

                if (!ContainsPreparedPressure(localPressure))
                {
                    continue;
                }

                Vector2 centreCandidate = MapMetricToMajorCandidate(
                    region.Position,
                    hostRegion,
                    hostPrepared,
                    domain);
                Vector2 tangentCandidate = Vector2.zero;
                if (region.RegionClass ==
                    StylizedRiverFoamNegativeRegionClass.EdgeCavity)
                {
                    Vector2 tangentMetric = new Vector2(
                        -region.BreachDirection.y,
                        region.BreachDirection.x);
                    tangentCandidate = MapMetricDirectionToMajorCandidate(
                        tangentMetric,
                        hostRegion,
                        hostPrepared);
                }

                StylizedRiverFoamHostedNegativeVariant[] variants =
                    BuildHostedNegativeVariants(
                        region,
                        localPressure,
                        centreCandidate,
                        tangentCandidate,
                        hostPrepared);
                destination.Add(
                    new StylizedRiverFoamPreparedHostedNegativeRegion(
                        region.RegionClass,
                        region.StableId,
                        region.HostRegionId,
                        hostIndex,
                        resolution,
                        localPressure,
                        centreCandidate,
                        variants));
            }
        }

        private static StylizedRiverFoamHostedNegativeVariant[]
            BuildHostedNegativeVariants(
                PreparedNegativeRegion region,
                float[] localPressure,
                Vector2 centreCandidate,
                Vector2 cavityTangentCandidate,
                StylizedRiverFoamPreparedMajorRegion hostPrepared)
        {
            List<StylizedRiverFoamHostedNegativeVariant> variants = new();
            StylizedRiverFoamHostedNegativeVariant identity =
                new StylizedRiverFoamHostedNegativeVariant(
                    Vector2.zero,
                    0f,
                    1f,
                    1f);
            variants.Add(identity);

            int targetCount = region.RegionClass ==
                StylizedRiverFoamNegativeRegionClass.EdgeCavity
                ? CavityHostedVariantCount
                : InteriorHostedVariantCount;
            float baselineOverlap = ResolveHostedHostOverlap(
                localPressure,
                centreCandidate,
                identity,
                hostPrepared);
            int maximumAttempts = targetCount * HostedVariantAttemptMultiplier;
            for (int attempt = 0;
                 attempt < maximumAttempts && variants.Count < targetCount;
                 attempt++)
            {
                uint seed = MixBits(
                    region.StableId ^
                    ((uint)(attempt + 1) * 0x9E3779B9u) ^
                    0xA24BAED5u);
                StylizedRiverFoamHostedNegativeVariant candidate =
                    region.RegionClass ==
                        StylizedRiverFoamNegativeRegionClass.EdgeCavity
                        ? BuildCavityHostedVariant(
                            cavityTangentCandidate,
                            seed)
                        : BuildInteriorHostedVariant(seed);
                if (!IsHostedVariantDistinct(candidate, variants) ||
                    !IsHostedVariantValid(
                        region.RegionClass,
                        localPressure,
                        centreCandidate,
                        candidate,
                        hostPrepared,
                        baselineOverlap))
                {
                    continue;
                }

                variants.Add(candidate);
            }

            return variants.ToArray();
        }

        private static StylizedRiverFoamHostedNegativeVariant
            BuildInteriorHostedVariant(uint seed)
        {
            float angle = Hash01(seed, 1u) * Mathf.PI * 2f;
            float radius = Mathf.Sqrt(Hash01(seed, 2u)) * 0.72f;
            return new StylizedRiverFoamHostedNegativeVariant(
                new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius,
                Mathf.Lerp(-0.14f, 0.14f, Hash01(seed, 3u)),
                Mathf.Lerp(0.88f, 1.12f, Hash01(seed, 4u)),
                Mathf.Lerp(0.88f, 1.12f, Hash01(seed, 5u)));
        }

        private static StylizedRiverFoamHostedNegativeVariant
            BuildCavityHostedVariant(
                Vector2 tangentCandidate,
                uint seed)
        {
            float tangentOffset = Mathf.Lerp(
                -0.55f,
                0.55f,
                Hash01(seed, 1u));
            return new StylizedRiverFoamHostedNegativeVariant(
                tangentCandidate * tangentOffset,
                Mathf.Lerp(-0.07f, 0.07f, Hash01(seed, 2u)),
                Mathf.Lerp(0.90f, 1.12f, Hash01(seed, 3u)),
                Mathf.Lerp(0.94f, 1.08f, Hash01(seed, 4u)));
        }

        private static bool IsHostedVariantDistinct(
            StylizedRiverFoamHostedNegativeVariant candidate,
            List<StylizedRiverFoamHostedNegativeVariant> existing)
        {
            for (int index = 0; index < existing.Count; index++)
            {
                StylizedRiverFoamHostedNegativeVariant other = existing[index];
                if (Vector2.Distance(
                        candidate.OffsetCells,
                        other.OffsetCells) < 0.12f &&
                    Mathf.Abs(candidate.RotationRadians -
                        other.RotationRadians) < 0.025f &&
                    Mathf.Abs(candidate.ScaleAlong - other.ScaleAlong) < 0.03f &&
                    Mathf.Abs(candidate.ScaleAcross - other.ScaleAcross) < 0.03f)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsHostedVariantValid(
            StylizedRiverFoamNegativeRegionClass regionClass,
            float[] localPressure,
            Vector2 centreCandidate,
            StylizedRiverFoamHostedNegativeVariant candidate,
            StylizedRiverFoamPreparedMajorRegion hostPrepared,
            float baselineOverlap)
        {
            if (!TryResolveHostedHostOverlap(
                    localPressure,
                    centreCandidate,
                    candidate,
                    hostPrepared,
                    out float overlap))
            {
                return false;
            }

            if (regionClass ==
                StylizedRiverFoamNegativeRegionClass.InteriorPocket)
            {
                float minimumContainment = Mathf.Max(
                    HostedInteriorMinimumContainment,
                    baselineOverlap - HostedInteriorContainmentTolerance);
                return overlap >= minimumContainment;
            }

            return Mathf.Abs(overlap - baselineOverlap) <=
                HostedCavityOverlapTolerance;
        }

        private static float ResolveHostedHostOverlap(
            float[] localPressure,
            Vector2 centreCandidate,
            StylizedRiverFoamHostedNegativeVariant variant,
            StylizedRiverFoamPreparedMajorRegion hostPrepared)
        {
            return TryResolveHostedHostOverlap(
                localPressure,
                centreCandidate,
                variant,
                hostPrepared,
                out float overlap)
                ? overlap
                : 0f;
        }

        private static bool TryResolveHostedHostOverlap(
            float[] localPressure,
            Vector2 centreCandidate,
            StylizedRiverFoamHostedNegativeVariant variant,
            StylizedRiverFoamPreparedMajorRegion hostPrepared,
            out float overlap)
        {
            float[] hostSupport = hostPrepared.LocalSupportData;
            int resolution = hostPrepared.MaskResolution;
            float cosine = Mathf.Cos(variant.RotationRadians);
            float sine = Mathf.Sin(variant.RotationRadians);
            float totalWeight = 0f;
            float insideWeight = 0f;
            for (int index = 0; index < localPressure.Length; index++)
            {
                float pressure = localPressure[index];
                if (pressure < HostedVariantPressureThreshold)
                {
                    continue;
                }

                Vector2 source = new Vector2(
                    index % resolution + 0.5f,
                    index / resolution + 0.5f);
                Vector2 relative = source - centreCandidate;
                relative = new Vector2(
                    relative.x * variant.ScaleAlong,
                    relative.y * variant.ScaleAcross);
                Vector2 transformed = centreCandidate +
                    variant.OffsetCells +
                    new Vector2(
                        cosine * relative.x - sine * relative.y,
                        sine * relative.x + cosine * relative.y);
                if (transformed.x < 0.5f || transformed.y < 0.5f ||
                    transformed.x > resolution - 0.5f ||
                    transformed.y > resolution - 0.5f)
                {
                    overlap = 0f;
                    return false;
                }

                float hostValue = SamplePreparedFieldBilinear(
                    hostSupport,
                    resolution,
                    transformed);
                totalWeight += pressure;
                if (hostValue >= MajorInteriorThreshold)
                {
                    insideWeight += pressure;
                }
            }

            overlap = totalWeight > 0.0001f
                ? insideWeight / totalWeight
                : 0f;
            return totalWeight > 0.0001f;
        }

        private static float SamplePreparedFieldBilinear(
            float[] source,
            int resolution,
            Vector2 position)
        {
            float x = Mathf.Clamp(position.x - 0.5f, 0f, resolution - 1f);
            float y = Mathf.Clamp(position.y - 0.5f, 0f, resolution - 1f);
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = Mathf.Min(x0 + 1, resolution - 1);
            int y1 = Mathf.Min(y0 + 1, resolution - 1);
            float tx = x - x0;
            float ty = y - y0;
            float a = source[x0 + y0 * resolution];
            float b = source[x1 + y0 * resolution];
            float c = source[x0 + y1 * resolution];
            float d = source[x1 + y1 * resolution];
            return Mathf.Lerp(
                Mathf.Lerp(a, b, tx),
                Mathf.Lerp(c, d, tx),
                ty);
        }

        private static int FindMajorHostIndex(
            uint hostId,
            IReadOnlyList<StylizedRiverFoamMajorRegion> regions,
            IReadOnlyList<StylizedRiverFoamPreparedMajorRegion> prepared)
        {
            int count = Mathf.Min(
                regions != null ? regions.Count : 0,
                prepared != null ? prepared.Count : 0);
            for (int index = 0; index < count; index++)
            {
                if (regions[index].StableId == hostId &&
                    prepared[index].StableId == hostId)
                {
                    return index;
                }
            }

            return -1;
        }

        private static Vector2 MapMetricToMajorCandidate(
            Vector2 metricPosition,
            StylizedRiverFoamMajorRegion hostRegion,
            StylizedRiverFoamPreparedMajorRegion hostPrepared,
            RiverDomainSnapshot domain)
        {
            float hostLocalDistance = hostRegion.CentreGlobalDistance -
                domain.GlobalDistanceMinimum;
            // Match the runtime Major transform exactly: a normalized host
            // centre is converted with the river widths at the sampled row,
            // not only with the widths at the original host centre. This keeps
            // the retained Pocket/Cavity mask attached on tapered bends.
            StylizedRiverSplineSample metricSample =
                domain.SampleAtOrientedDistance(metricPosition.x);
            float hostLateralMetres = SignedNormalizedToMetres(
                ResolveRuntimeMajorAcrossNormalized(hostRegion),
                Mathf.Max(0.05f, metricSample.LeftHalfWidth),
                Mathf.Max(0.05f, metricSample.RightHalfWidth));
            Vector2 delta = new Vector2(
                metricPosition.x - hostLocalDistance,
                metricPosition.y - hostLateralMetres);
            float orientationCosine = Mathf.Cos(hostRegion.OrientationRadians);
            float orientationSine = Mathf.Sin(hostRegion.OrientationRadians);
            float sourceMajor = (
                orientationCosine * delta.x +
                orientationSine * delta.y) /
                Mathf.Max(0.0001f, hostRegion.MetresPerCandidateCell);
            float sourceMinor = (
                -orientationSine * delta.x +
                orientationCosine * delta.y) /
                Mathf.Max(0.0001f, hostRegion.MetresPerCandidateCell);
            float principalCosine = Mathf.Cos(
                hostPrepared.PrincipalAngleRadians);
            float principalSine = Mathf.Sin(
                hostPrepared.PrincipalAngleRadians);
            return new Vector2(
                hostPrepared.CentroidCells.x +
                    principalCosine * sourceMajor -
                    principalSine * sourceMinor,
                hostPrepared.CentroidCells.y +
                    principalSine * sourceMajor +
                    principalCosine * sourceMinor);
        }

        private static Vector2 MapMetricDirectionToMajorCandidate(
            Vector2 metricDirection,
            StylizedRiverFoamMajorRegion hostRegion,
            StylizedRiverFoamPreparedMajorRegion hostPrepared)
        {
            float orientationCosine = Mathf.Cos(hostRegion.OrientationRadians);
            float orientationSine = Mathf.Sin(hostRegion.OrientationRadians);
            float sourceMajor =
                orientationCosine * metricDirection.x +
                orientationSine * metricDirection.y;
            float sourceMinor =
                -orientationSine * metricDirection.x +
                orientationCosine * metricDirection.y;
            float principalCosine = Mathf.Cos(
                hostPrepared.PrincipalAngleRadians);
            float principalSine = Mathf.Sin(
                hostPrepared.PrincipalAngleRadians);
            Vector2 candidateDirection = new Vector2(
                principalCosine * sourceMajor -
                    principalSine * sourceMinor,
                principalSine * sourceMajor +
                    principalCosine * sourceMinor);
            return candidateDirection.sqrMagnitude > 0.000001f
                ? candidateDirection.normalized
                : Vector2.zero;
        }

        private static void ResampleHostedRegionToLocalMask(
            PreparedNegativeRegion region,
            float[] localPressure,
            int resolution,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            RiverDomainSnapshot domain,
            StylizedRiverFoamMajorRegion hostRegion,
            StylizedRiverFoamPreparedMajorRegion hostPrepared)
        {
            if (localPressure == null || localPressure.Length == 0 ||
                resolution <= 0 || width <= 1 || height <= 1 ||
                fieldLength <= 0.0001f || domain == null || !domain.IsValid)
            {
                return;
            }

            float[] sourcePressure = BuildRegionSourcePressure(
                region,
                width,
                height);
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector2 candidatePosition = new Vector2(
                        x + 0.5f,
                        y + 0.5f);
                    Vector2 metricPosition = MapMajorCandidateToMetric(
                        candidatePosition,
                        hostRegion,
                        hostPrepared,
                        validFieldLength,
                        domain);
                    float value = SampleRegionSourcePressure(
                        sourcePressure,
                        width,
                        height,
                        fieldLength,
                        validFieldLength,
                        metricPosition,
                        domain);
                    localPressure[x + y * resolution] = value;
                }
            }
        }

        private static float[] BuildRegionSourcePressure(
            PreparedNegativeRegion region,
            int width,
            int height)
        {
            float[] source = new float[Mathf.Max(0, width * height)];
            RasterSample[] samples = region.Samples;
            for (int index = 0; index < samples.Length; index++)
            {
                RasterSample sample = samples[index];
                if (sample.Index < 0 || sample.Index >= source.Length)
                {
                    continue;
                }

                source[sample.Index] = Mathf.Max(
                    source[sample.Index],
                    sample.Value);
            }

            return source;
        }

        private static void ResampleFreeWaterRegionToLocalMask(
            PreparedNegativeRegion region,
            float[] localPressure,
            int resolution,
            float metresPerCell,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            RiverDomainSnapshot domain)
        {
            if (localPressure == null || localPressure.Length == 0 ||
                resolution <= 0 || metresPerCell <= 0.0001f ||
                width <= 1 || height <= 1 ||
                fieldLength <= 0.0001f || domain == null || !domain.IsValid)
            {
                return;
            }

            float[] sourcePressure = BuildRegionSourcePressure(
                region,
                width,
                height);
            float centre = resolution * 0.5f;
            float cosine = Mathf.Cos(region.Orientation);
            float sine = Mathf.Sin(region.Orientation);
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float localX = (x + 0.5f - centre) * metresPerCell;
                    float localY = (y + 0.5f - centre) * metresPerCell;
                    Vector2 metricPosition = region.Position + new Vector2(
                        cosine * localX - sine * localY,
                        sine * localX + cosine * localY);
                    float value = SampleRegionSourcePressure(
                        sourcePressure,
                        width,
                        height,
                        fieldLength,
                        validFieldLength,
                        metricPosition,
                        domain);
                    localPressure[x + y * resolution] = value;
                }
            }
        }

        private static Vector2 MapMajorCandidateToMetric(
            Vector2 candidatePosition,
            StylizedRiverFoamMajorRegion hostRegion,
            StylizedRiverFoamPreparedMajorRegion hostPrepared,
            float validFieldLength,
            RiverDomainSnapshot domain)
        {
            Vector2 relative = candidatePosition - hostPrepared.CentroidCells;
            float principalCosine = Mathf.Cos(
                hostPrepared.PrincipalAngleRadians);
            float principalSine = Mathf.Sin(
                hostPrepared.PrincipalAngleRadians);
            float sourceMajor =
                principalCosine * relative.x + principalSine * relative.y;
            float sourceMinor =
                -principalSine * relative.x + principalCosine * relative.y;
            float majorMetres = sourceMajor *
                Mathf.Max(0.0001f, hostRegion.MetresPerCandidateCell);
            float minorMetres = sourceMinor *
                Mathf.Max(0.0001f, hostRegion.MetresPerCandidateCell);

            float orientationCosine = Mathf.Cos(hostRegion.OrientationRadians);
            float orientationSine = Mathf.Sin(hostRegion.OrientationRadians);
            float deltaAlong =
                orientationCosine * majorMetres -
                orientationSine * minorMetres;
            float deltaAcross =
                orientationSine * majorMetres +
                orientationCosine * minorMetres;

            float hostLocalDistance = hostRegion.CentreGlobalDistance -
                domain.GlobalDistanceMinimum;
            float localDistance = hostLocalDistance + deltaAlong;
            float widthSampleDistance = Mathf.Clamp(
                localDistance,
                0f,
                Mathf.Min(domain.LocalLength, validFieldLength));
            StylizedRiverSplineSample sample =
                domain.SampleAtOrientedDistance(widthSampleDistance);
            float hostLateralMetres = SignedNormalizedToMetres(
                ResolveRuntimeMajorAcrossNormalized(hostRegion),
                Mathf.Max(0.05f, sample.LeftHalfWidth),
                Mathf.Max(0.05f, sample.RightHalfWidth));
            return new Vector2(
                localDistance,
                hostLateralMetres + deltaAcross);
        }

        private static float ResolveRuntimeMajorAcrossNormalized(
            StylizedRiverFoamMajorRegion hostRegion)
        {
            return Mathf.Clamp(
                hostRegion.CentreAcrossNormalized,
                -0.82f,
                0.82f);
        }

        private static float SampleRegionSourcePressure(
            float[] source,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            Vector2 metricPosition,
            RiverDomainSnapshot domain)
        {
            if (source == null || source.Length < width * height ||
                metricPosition.x < 0f ||
                metricPosition.x > fieldLength + 0.0001f)
            {
                return 0f;
            }

            float clampedDistance = Mathf.Clamp(
                metricPosition.x,
                0f,
                Mathf.Min(domain.LocalLength, validFieldLength));
            StylizedRiverSplineSample sample =
                domain.SampleAtOrientedDistance(clampedDistance);
            float leftSurface = Mathf.Max(
                0.05f,
                sample.LeftSurfaceHalfWidth);
            float rightSurface = Mathf.Max(
                0.05f,
                sample.RightSurfaceHalfWidth);
            float across01 = AcrossMetresTo01(
                metricPosition.y,
                leftSurface,
                rightSurface);
            if (across01 < 0f || across01 > 1f)
            {
                return 0f;
            }

            float sourceX = metricPosition.x /
                Mathf.Max(0.0001f, fieldLength) *
                Mathf.Max(1, width - 1);
            float sourceY = across01 * Mathf.Max(1, height - 1);
            return SampleSourceFieldBilinear(
                source,
                width,
                height,
                sourceX,
                sourceY);
        }

        private static float SampleSourceFieldBilinear(
            float[] source,
            int width,
            int height,
            float x,
            float y)
        {
            if (x < 0f || y < 0f ||
                x > width - 1f || y > height - 1f)
            {
                return 0f;
            }

            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = Mathf.Min(x0 + 1, width - 1);
            int y1 = Mathf.Min(y0 + 1, height - 1);
            float tx = x - x0;
            float ty = y - y0;
            float a = source[x0 + y0 * width];
            float b = source[x1 + y0 * width];
            float c = source[x0 + y1 * width];
            float d = source[x1 + y1 * width];
            return Mathf.Clamp01(Mathf.Lerp(
                Mathf.Lerp(a, b, tx),
                Mathf.Lerp(c, d, tx),
                ty));
        }

        private static float AcrossMetresTo01(
            float metres,
            float leftWidth,
            float rightWidth)
        {
            return metres < 0f
                ? 0.5f + metres / Mathf.Max(0.0001f, leftWidth) * 0.5f
                : 0.5f + metres / Mathf.Max(0.0001f, rightWidth) * 0.5f;
        }

        private static bool ContainsPreparedPressure(float[] pressure)
        {
            if (pressure == null)
            {
                return false;
            }

            for (int index = 0; index < pressure.Length; index++)
            {
                if (pressure[index] > 0.0001f)
                {
                    return true;
                }
            }

            return false;
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

        private readonly struct FreeWaterOpportunity
        {
            public FreeWaterOpportunity(
                uint stableId,
                Vector2 position,
                float activationRank)
            {
                StableId = stableId;
                Position = position;
                ActivationRank = Mathf.Clamp01(activationRank);
            }

            public uint StableId { get; }
            public Vector2 Position { get; }
            public float ActivationRank { get; }
        }

        private readonly struct AcceptedFreeWaterEvent
        {
            public AcceptedFreeWaterEvent(
                Vector2 position,
                float largestRadius)
            {
                Position = position;
                LargestRadius = Mathf.Max(0f, largestRadius);
            }

            public Vector2 Position { get; }
            public float LargestRadius { get; }
        }

        private readonly struct WeakSpanOpportunity
        {
            public WeakSpanOpportunity(
                uint stableId,
                uint connectorId,
                Vector2[] points,
                float pathLength,
                float distanceAlongPath,
                float activationRank)
            {
                StableId = stableId;
                ConnectorId = connectorId;
                Points = points;
                DistanceAlongPath = Mathf.Clamp(
                    distanceAlongPath,
                    0f,
                    pathLength);
                ActivationRank = Mathf.Clamp01(activationRank);
            }

            public uint StableId { get; }
            public uint ConnectorId { get; }
            public Vector2[] Points { get; }
            public float DistanceAlongPath { get; }
            public float ActivationRank { get; }
        }

        private readonly struct AcceptedWeakSpan
        {
            public AcceptedWeakSpan(
                float distanceAlongPath,
                float alongRadius)
            {
                DistanceAlongPath = distanceAlongPath;
                AlongRadius = alongRadius;
            }

            public float DistanceAlongPath { get; }
            public float AlongRadius { get; }
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
