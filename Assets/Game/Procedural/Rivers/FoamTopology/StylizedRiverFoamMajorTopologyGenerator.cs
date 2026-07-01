using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Deterministic CPU proof generator for static whole-river Major Support.
    /// Expensive candidate generation and composition are intentionally kept in
    /// the controlled initialization/rebuild path until the accepted result is
    /// moved into the production topology cache.
    /// </summary>
    public static class StylizedRiverFoamMajorTopologyGenerator
    {
        private const int LateralOpportunityCount = 3;
        private const int MaximumLongitudinalOpportunityCount = 48;
        private const int PlacementVariantCount = 3;
        private const float MajorCoverageThreshold = 0.15f;
        private const float GoldenRatioConjugate = 0.61803398875f;
        private const int MaximumRecycleAnchorCount = 6;
        private const int RecycleAnchorAttemptCount = 15;
        private const float MinimumRecycleRunwayMetres = 3f;
        private const float MinimumRecycleRunwayFraction = 0.15f;
        private static readonly float[] RecycleLateralOffsets =
            { 0f, -0.22f, 0.22f };

        public static StylizedRiverFoamMajorTopology Generate(
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            StylizedRiverQuality quality,
            float shoreMotion,
            float amount,
            float size,
            float sizeVariation,
            float recycleTerritoryDeviationPercent,
            int seed,
            float[] obstacleMask)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int cellCount = Mathf.Max(0, width * height);
            float[] support = new float[cellCount];
            int rejectionReasonCount = Enum.GetValues(
                typeof(StylizedRiverFoamMajorPlacementRejectionReason)).Length;
            int[] rejectionCounts = new int[rejectionReasonCount];

            if (domain == null || !domain.IsValid ||
                width < 2 || height < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                stopwatch.Stop();
                return new StylizedRiverFoamMajorTopology(
                    width,
                    height,
                    0,
                    0,
                    0,
                    0,
                    0,
                    stopwatch.Elapsed.TotalMilliseconds,
                    support,
                    Array.Empty<StylizedRiverFoamMajorRegion>(),
                    Array.Empty<StylizedRiverFoamPreparedMajorRegion>(),
                    0,
                    0,
                    rejectionCounts);
            }

            amount = Mathf.Clamp01(amount);
            size = Mathf.Clamp01(size);
            sizeVariation = Mathf.Clamp01(sizeVariation);
            recycleTerritoryDeviationPercent = Mathf.Clamp(
                recycleTerritoryDeviationPercent,
                0f,
                10f);
            seed = Mathf.Max(0, seed);
            float[] fluidCoverage = new float[cellCount];
            bool[] validCells = new bool[cellCount];
            int validCellCount = BuildFluidContext(
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

            List<MajorOpportunity> opportunities = BuildOpportunities(
                validFieldLength,
                seed);
            opportunities.Sort((a, b) =>
            {
                int rankComparison = a.ActivationRank.CompareTo(
                    b.ActivationRank);
                return rankComparison != 0
                    ? rankComparison
                    : a.StableId.CompareTo(b.StableId);
            });

            int activeOpportunityCount = ResolveActiveOpportunityCount(
                opportunities.Count,
                amount);
            float maximumCoverage = Mathf.Lerp(
                0.07f,
                0.42f,
                Mathf.Pow(amount, 0.82f));
            int maximumCoveredCells = Mathf.CeilToInt(
                validCellCount * maximumCoverage);
            float averageVisibleWidth = ResolveAverageVisibleWidth(domain);

            List<StylizedRiverFoamMajorRegion> regions = new();
            List<StylizedRiverFoamPreparedMajorRegion> preparedRegions = new();
            List<AcceptedPlacement> acceptedPlacements = new();
            int preparedRecycleAnchorCount = 0;
            int recycleFallbackCount = 0;
            int[] lateralPopulation = new int[3];
            float[] variantField = new float[cellCount];
            float[] bestField = new float[cellCount];
            int coveredCellCount = 0;

            for (int activationOrder = 0;
                 activationOrder < activeOpportunityCount;
                 activationOrder++)
            {
                MajorOpportunity opportunity = opportunities[activationOrder];
                StylizedRiverFoamMajorCandidate candidate =
                    StylizedRiverFoamMajorCandidateGenerator.Generate(
                        opportunity.CandidateSeed);
                if (candidate == null || !candidate.Accepted)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamMajorPlacementRejectionReason
                            .CandidateRejected]++;
                    continue;
                }

                CandidateShape shape = AnalyseCandidate(candidate);
                if (!shape.IsValid)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamMajorPlacementRejectionReason
                            .CandidateRejected]++;
                    continue;
                }

                float sizeQuantile = Fractional(
                    opportunity.SizeSequenceOffset +
                    activationOrder * GoldenRatioConjugate);
                float patch2HierarchyScale = Mathf.Lerp(
                    0.62f,
                    1.44f,
                    sizeQuantile);
                float hierarchySpread = sizeVariation * 2f;
                float hierarchyScale = Mathf.Clamp(
                    1f + (patch2HierarchyScale - 1f) * hierarchySpread,
                    0.42f,
                    1.82f);
                float requestedDiameter = Mathf.Lerp(
                    1.15f,
                    4.85f,
                    size) * hierarchyScale;
                float baseOrientation = ResolveOrientationRadians(
                    opportunity.StableId);
                float orientationLateralness = Mathf.Abs(
                    Mathf.Sin(baseOrientation));
                float baseAcrossNormalized = Mathf.Lerp(
                    opportunity.AcrossNormalized,
                    opportunity.AcrossNormalized * 0.76f,
                    Mathf.SmoothStep(0.55f, 0.95f, orientationLateralness));

                CandidateEvaluation bestEvaluation = default;
                bool hasBestEvaluation = false;
                Array.Clear(bestField, 0, bestField.Length);

                for (int variantIndex = 0;
                     variantIndex < PlacementVariantCount;
                     variantIndex++)
                {
                    float variantSign = Hash01(
                        opportunity.StableId,
                        (uint)(30 + variantIndex)) < 0.5f
                            ? -1f
                            : 1f;
                    float lateralOffset = variantIndex switch
                    {
                        0 => 0f,
                        1 => 0.10f * variantSign,
                        _ => -0.10f * variantSign
                    };
                    float orientationOffset = variantIndex switch
                    {
                        0 => 0f,
                        1 => Mathf.Deg2Rad * 13f * variantSign,
                        _ => -Mathf.Deg2Rad * 13f * variantSign
                    };
                    float centreAcrossNormalized = Mathf.Clamp(
                        baseAcrossNormalized + lateralOffset,
                        -0.82f,
                        0.82f);
                    float orientation = WrapHalfTurn(
                        baseOrientation + orientationOffset);

                    Array.Clear(variantField, 0, variantField.Length);
                    CandidateEvaluation evaluation = EvaluatePlacement(
                        candidate,
                        shape,
                        opportunity,
                        centreAcrossNormalized,
                        orientation,
                        requestedDiameter,
                        domain,
                        width,
                        height,
                        fieldLength,
                        validFieldLength,
                        fluidCoverage,
                        validCells,
                        obstacleMask,
                        support,
                        variantField,
                        acceptedPlacements,
                        lateralPopulation,
                        averageVisibleWidth,
                        coveredCellCount,
                        maximumCoveredCells);

                    bool evaluationIsBetter = !hasBestEvaluation ||
                        (evaluation.Accepted && !bestEvaluation.Accepted) ||
                        (evaluation.Accepted == bestEvaluation.Accepted &&
                         evaluation.Score > bestEvaluation.Score);
                    if (evaluationIsBetter)
                    {
                        hasBestEvaluation = true;
                        bestEvaluation = evaluation;
                        Buffer.BlockCopy(
                            variantField,
                            0,
                            bestField,
                            0,
                            variantField.Length * sizeof(float));
                    }
                }

                if (!hasBestEvaluation || !bestEvaluation.Accepted)
                {
                    StylizedRiverFoamMajorPlacementRejectionReason reason =
                        hasBestEvaluation
                            ? bestEvaluation.RejectionReason
                            : StylizedRiverFoamMajorPlacementRejectionReason
                                .NoRasterCoverage;
                    rejectionCounts[(int)reason]++;
                    continue;
                }

                for (int index = 0; index < support.Length; index++)
                {
                    support[index] = Mathf.Max(
                        support[index],
                        bestField[index]);
                }

                coveredCellCount = bestEvaluation.CoveredCellCountAfter;
                int lateralBin = ResolveLateralBin(
                    bestEvaluation.CentreAcrossNormalized);
                lateralPopulation[lateralBin]++;

                uint evolutionSeed = MixBits(
                    opportunity.StableId ^ 0x27D4EB2Fu);
                float driftRateSelector = Hash01(evolutionSeed, 70u);
                float phaseOffset = Hash01(evolutionSeed, 71u);
                float fadeInSelector = Hash01(evolutionSeed, 72u);
                float fadeOutSelector = Hash01(evolutionSeed, 73u);
                float movementSpanSelector = Hash01(evolutionSeed, 74u);
                float recycleSelector = Hash01(evolutionSeed, 75u);

                float[] localSupport = new float[
                    candidate.Resolution * candidate.Resolution];
                candidate.CopyFinalSupportTo(localSupport);
                StylizedRiverFoamMajorRecycleAnchor[] recycleAnchors =
                    BuildRecycleAnchors(
                        candidate,
                        shape,
                        bestEvaluation,
                        opportunity.StableId,
                        domain,
                        width,
                        height,
                        fieldLength,
                        validFieldLength,
                        recycleTerritoryDeviationPercent,
                        fluidCoverage,
                        obstacleMask,
                        out bool usedFallbackAnchor);
                preparedRecycleAnchorCount += recycleAnchors.Length;
                if (usedFallbackAnchor)
                {
                    recycleFallbackCount++;
                }

                preparedRegions.Add(
                    new StylizedRiverFoamPreparedMajorRegion(
                        opportunity.StableId,
                        candidate.Resolution,
                        localSupport,
                        shape.Centroid,
                        shape.PrincipalAngleRadians,
                        shape.MajorHalfExtentCells,
                        shape.MinorHalfExtentCells,
                        recycleAnchors));

                regions.Add(new StylizedRiverFoamMajorRegion(
                    opportunity.StableId,
                    opportunity.LongitudinalIndex,
                    opportunity.LateralIndex,
                    activationOrder,
                    opportunity.ActivationRank,
                    opportunity.CandidateSeed,
                    domain.GlobalDistanceMinimum +
                        bestEvaluation.CentreLocalDistance,
                    bestEvaluation.CentreAcrossNormalized,
                    bestEvaluation.OrientationRadians,
                    bestEvaluation.MetresPerCandidateCell,
                    evolutionSeed,
                    driftRateSelector,
                    phaseOffset,
                    fadeInSelector,
                    fadeOutSelector,
                    movementSpanSelector,
                    recycleSelector,
                    bestEvaluation.AnchoringStrength));
                acceptedPlacements.Add(new AcceptedPlacement(
                    bestEvaluation.CentreLocalDistance,
                    bestEvaluation.CentreAcrossNormalized,
                    bestEvaluation.AlongRadius,
                    bestEvaluation.CrossRadius,
                    bestEvaluation.OrientationRadians,
                    shape.AspectRatio));
            }

            stopwatch.Stop();
            int rejectedRegionCount = activeOpportunityCount - regions.Count;
            return new StylizedRiverFoamMajorTopology(
                width,
                height,
                activeOpportunityCount,
                regions.Count,
                rejectedRegionCount,
                validCellCount,
                coveredCellCount,
                stopwatch.Elapsed.TotalMilliseconds,
                support,
                regions.ToArray(),
                preparedRegions.ToArray(),
                preparedRecycleAnchorCount,
                recycleFallbackCount,
                rejectionCounts);
        }

        private static StylizedRiverFoamMajorRecycleAnchor[]
            BuildRecycleAnchors(
                StylizedRiverFoamMajorCandidate candidate,
                CandidateShape shape,
                CandidateEvaluation acceptedPlacement,
                uint stableId,
                RiverDomainSnapshot domain,
                int width,
                int height,
                float fieldLength,
                float validFieldLength,
                float recycleTerritoryDeviationPercent,
                float[] fluidCoverage,
                float[] obstacleMask,
                out bool usedFallback)
        {
            usedFallback = false;
            List<StylizedRiverFoamMajorRecycleAnchor> anchors = new(
                MaximumRecycleAnchorCount);
            float deviationMetres = validFieldLength *
                Mathf.Clamp(recycleTerritoryDeviationPercent, 0f, 10f) *
                0.01f;
            float minimumRunway = Mathf.Min(
                validFieldLength,
                Mathf.Max(
                    MinimumRecycleRunwayMetres,
                    validFieldLength * MinimumRecycleRunwayFraction));
            float egressStart =
                StylizedRiverFoamMajorTopology
                    .ResolveEvolutionEgressStart(validFieldLength);
            float maximumSafeHomeDistance = Mathf.Max(
                0f,
                egressStart - minimumRunway);
            float homeDistance = Mathf.Min(
                acceptedPlacement.CentreLocalDistance,
                maximumSafeHomeDistance);
            float territoryStart = Mathf.Clamp(
                homeDistance - deviationMetres,
                0f,
                maximumSafeHomeDistance);
            float territoryEnd = Mathf.Clamp(
                homeDistance + deviationMetres,
                territoryStart,
                maximumSafeHomeDistance);
            for (int attempt = 0;
                 attempt < RecycleAnchorAttemptCount &&
                 anchors.Count < MaximumRecycleAnchorCount;
                 attempt++)
            {
                uint attemptSeed = MixBits(
                    stableId ^
                    ((uint)(attempt + 1) * 0x9E3779B9u) ^
                    0xA511E9B3u);
                float alongSelector = Fractional(
                    Hash01(attemptSeed, 1u) +
                    attempt * GoldenRatioConjugate);
                float centreLocalDistance = Mathf.Lerp(
                    territoryStart,
                    territoryEnd,
                    alongSelector);
                int laneIndex = attempt % RecycleLateralOffsets.Length;
                float laneJitter = Mathf.Lerp(
                    -0.08f,
                    0.08f,
                    Hash01(attemptSeed, 2u));
                float centreAcrossNormalized = Mathf.Clamp(
                    acceptedPlacement.CentreAcrossNormalized +
                        RecycleLateralOffsets[laneIndex] + laneJitter,
                    -0.76f,
                    0.76f);
                float orientation = WrapHalfTurn(
                    acceptedPlacement.OrientationRadians +
                    Mathf.Lerp(-0.24f, 0.24f, Hash01(attemptSeed, 3u)));
                float metresPerCell =
                    acceptedPlacement.MetresPerCandidateCell *
                    Mathf.Lerp(0.82f, 1.00f, Hash01(attemptSeed, 4u));

                EvolutionAnchorEvaluation evaluation =
                    EvaluateRecycleAnchor(
                        candidate,
                        shape,
                        centreLocalDistance,
                        centreAcrossNormalized,
                        orientation,
                        metresPerCell,
                        domain,
                        width,
                        height,
                        fieldLength,
                        validFieldLength,
                        fluidCoverage,
                        obstacleMask);
                if (!evaluation.Accepted ||
                    IsNearExistingRecycleAnchor(
                        anchors,
                        centreLocalDistance,
                        centreAcrossNormalized))
                {
                    continue;
                }

                anchors.Add(new StylizedRiverFoamMajorRecycleAnchor(
                    centreLocalDistance,
                    centreAcrossNormalized,
                    orientation,
                    metresPerCell,
                    false));
            }

            if (anchors.Count > 0)
            {
                return anchors.ToArray();
            }

            usedFallback = true;
            float fallbackAcross = Mathf.Clamp(
                acceptedPlacement.CentreAcrossNormalized,
                -0.76f,
                0.76f);
            float fallbackScale =
                acceptedPlacement.MetresPerCandidateCell * 0.72f;
            EvolutionAnchorEvaluation safeHomeFallback =
                EvaluateRecycleAnchor(
                    candidate,
                    shape,
                    homeDistance,
                    fallbackAcross,
                    acceptedPlacement.OrientationRadians,
                    fallbackScale,
                    domain,
                    width,
                    height,
                    fieldLength,
                    validFieldLength,
                    fluidCoverage,
                    obstacleMask);
            if (safeHomeFallback.Accepted)
            {
                return new[]
                {
                    new StylizedRiverFoamMajorRecycleAnchor(
                        homeDistance,
                        fallbackAcross,
                        acceptedPlacement.OrientationRadians,
                        fallbackScale,
                        true)
                };
            }

            // The original accepted placement is the final fail-safe because
            // it has already passed the authoritative static placement gates.
            return new[]
            {
                new StylizedRiverFoamMajorRecycleAnchor(
                    acceptedPlacement.CentreLocalDistance,
                    acceptedPlacement.CentreAcrossNormalized,
                    acceptedPlacement.OrientationRadians,
                    acceptedPlacement.MetresPerCandidateCell,
                    true)
            };
        }

        private static bool IsNearExistingRecycleAnchor(
            List<StylizedRiverFoamMajorRecycleAnchor> anchors,
            float localDistance,
            float acrossNormalized)
        {
            for (int index = 0; index < anchors.Count; index++)
            {
                StylizedRiverFoamMajorRecycleAnchor anchor = anchors[index];
                if (Mathf.Abs(anchor.CentreLocalDistance - localDistance) <
                        0.34f &&
                    Mathf.Abs(
                        anchor.CentreAcrossNormalized - acrossNormalized) <
                        0.16f)
                {
                    return true;
                }
            }

            return false;
        }

        private static EvolutionAnchorEvaluation EvaluateRecycleAnchor(
            StylizedRiverFoamMajorCandidate candidate,
            CandidateShape shape,
            float centreLocalDistance,
            float centreAcrossNormalized,
            float orientationRadians,
            float metresPerCandidateCell,
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            float[] fluidCoverage,
            float[] obstacleMask)
        {
            float cosineOrientation = Mathf.Cos(orientationRadians);
            float sineOrientation = Mathf.Sin(orientationRadians);
            float alongRadius = metresPerCandidateCell * (
                Mathf.Abs(cosineOrientation) * shape.MajorHalfExtentCells +
                Mathf.Abs(sineOrientation) * shape.MinorHalfExtentCells);
            int minimumX = Mathf.Clamp(
                Mathf.FloorToInt(
                    (centreLocalDistance - alongRadius) /
                    Mathf.Max(0.0001f, fieldLength) * width) - 1,
                0,
                width - 1);
            int maximumX = Mathf.Clamp(
                Mathf.CeilToInt(
                    (centreLocalDistance + alongRadius) /
                    Mathf.Max(0.0001f, fieldLength) * width) + 1,
                0,
                width - 1);

            float totalWeight = 0f;
            float effectiveWeight = 0f;
            float bankClippedWeight = 0f;
            float obstacleWeight = 0f;
            float outsideDomainWeight = 0f;
            float candidatePrincipalCosine = Mathf.Cos(
                shape.PrincipalAngleRadians);
            float candidatePrincipalSine = Mathf.Sin(
                shape.PrincipalAngleRadians);

            for (int x = minimumX; x <= maximumX; x++)
            {
                float localDistance = (x + 0.5f) /
                    Mathf.Max(1f, width) * fieldLength;
                float clampedDistance = Mathf.Clamp(
                    localDistance,
                    0f,
                    validFieldLength);
                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(clampedDistance);
                float centreAcrossMetres = SignedNormalizedToMetres(
                    centreAcrossNormalized,
                    sample.LeftHalfWidth,
                    sample.RightHalfWidth);
                float deltaAlong = localDistance - centreLocalDistance;

                for (int y = 0; y < height; y++)
                {
                    float across01 = (y + 0.5f) /
                        Mathf.Max(1f, height);
                    float acrossMetres = Across01ToMetres(
                        across01,
                        sample.LeftSurfaceHalfWidth,
                        sample.RightSurfaceHalfWidth);
                    float deltaAcross = acrossMetres - centreAcrossMetres;
                    float principalMajorMetres =
                        cosineOrientation * deltaAlong +
                        sineOrientation * deltaAcross;
                    float principalMinorMetres =
                        -sineOrientation * deltaAlong +
                        cosineOrientation * deltaAcross;
                    float sourceMajor = principalMajorMetres /
                        Mathf.Max(0.0001f, metresPerCandidateCell);
                    float sourceMinor = principalMinorMetres /
                        Mathf.Max(0.0001f, metresPerCandidateCell);
                    float sourceX = shape.Centroid.x +
                        candidatePrincipalCosine * sourceMajor -
                        candidatePrincipalSine * sourceMinor;
                    float sourceY = shape.Centroid.y +
                        candidatePrincipalSine * sourceMajor +
                        candidatePrincipalCosine * sourceMinor;
                    float support = candidate.SampleFinalSupportBilinear(
                        sourceX,
                        sourceY);
                    if (support <= 0.001f)
                    {
                        continue;
                    }

                    int cellIndex = x + y * width;
                    float fluid = fluidCoverage[cellIndex];
                    float obstacle = SampleObstacle(
                        obstacleMask,
                        cellIndex);
                    totalWeight += support;
                    effectiveWeight += support * fluid * (1f - obstacle);
                    bankClippedWeight += support * (1f - fluid);
                    obstacleWeight += support * obstacle;
                    if (localDistance < 0f ||
                        localDistance > validFieldLength)
                    {
                        outsideDomainWeight += support;
                    }
                }
            }

            if (totalWeight <= 0.001f)
            {
                return new EvolutionAnchorEvaluation(
                    false,
                    float.NegativeInfinity,
                    centreLocalDistance,
                    centreAcrossNormalized,
                    orientationRadians,
                    metresPerCandidateCell);
            }

            float bankRatio = bankClippedWeight / totalWeight;
            float obstacleRatio = obstacleWeight / totalWeight;
            float outsideRatio = outsideDomainWeight / totalWeight;
            float effectiveRatio = effectiveWeight / totalWeight;
            bool accepted = outsideRatio <= 0.10f &&
                obstacleRatio <= 0.055f &&
                bankRatio <= 0.22f &&
                effectiveRatio >= 0.68f;
            float score = effectiveRatio -
                bankRatio * 0.7f -
                obstacleRatio * 2.2f -
                outsideRatio * 3f;
            return new EvolutionAnchorEvaluation(
                accepted,
                score,
                centreLocalDistance,
                centreAcrossNormalized,
                orientationRadians,
                metresPerCandidateCell);
        }

        private static List<MajorOpportunity> BuildOpportunities(
            float validFieldLength,
            int seed)
        {
            int longitudinalCount = Mathf.Clamp(
                Mathf.CeilToInt(validFieldLength / 2.75f),
                validFieldLength < 4f ? 1 : 2,
                MaximumLongitudinalOpportunityCount);
            float rowSpacing = validFieldLength /
                Mathf.Max(1, longitudinalCount);
            float[] lateralLanes = { -0.62f, 0f, 0.62f };
            List<MajorOpportunity> opportunities = new(
                longitudinalCount * LateralOpportunityCount);

            for (int longitudinalIndex = 0;
                 longitudinalIndex < longitudinalCount;
                 longitudinalIndex++)
            {
                for (int lateralIndex = 0;
                     lateralIndex < LateralOpportunityCount;
                     lateralIndex++)
                {
                    uint stableId = MixBits(
                        (uint)seed ^
                        ((uint)(longitudinalIndex + 1) * 0x9E3779B9u) ^
                        ((uint)(lateralIndex + 1) * 0x85EBCA6Bu));
                    float longitudinalJitter = Mathf.Lerp(
                        -0.22f,
                        0.22f,
                        Hash01(stableId, 1u)) * rowSpacing;
                    float centreLocalDistance = Mathf.Clamp(
                        (longitudinalIndex + 0.5f) * rowSpacing +
                            longitudinalJitter,
                        rowSpacing * 0.18f,
                        Mathf.Max(
                            rowSpacing * 0.18f,
                            validFieldLength - rowSpacing * 0.18f));
                    float acrossJitter = lateralIndex == 1
                        ? Mathf.Lerp(-0.18f, 0.18f, Hash01(stableId, 2u))
                        : Mathf.Lerp(-0.11f, 0.11f, Hash01(stableId, 2u));
                    float acrossNormalized = Mathf.Clamp(
                        lateralLanes[lateralIndex] + acrossJitter,
                        -0.78f,
                        0.78f);
                    float activationRank = Hash01(stableId, 3u);
                    int candidateSeed = unchecked((int)(
                        MixBits(stableId ^ 0xC2B2AE35u) & 0x7FFFFFFFu));
                    float sizeSequenceOffset = Hash01(
                        MixBits((uint)seed),
                        4u);

                    opportunities.Add(new MajorOpportunity(
                        stableId,
                        longitudinalIndex,
                        lateralIndex,
                        centreLocalDistance,
                        acrossNormalized,
                        activationRank,
                        candidateSeed,
                        sizeSequenceOffset));
                }
            }

            return opportunities;
        }

        private static int ResolveActiveOpportunityCount(
            int opportunityCount,
            float amount)
        {
            if (opportunityCount <= 0 || amount <= 0.0001f)
            {
                return 0;
            }

            float activeFraction =
                amount * 0.18f + Mathf.Pow(amount, 1.35f) * 0.82f;
            return Mathf.Clamp(
                Mathf.CeilToInt(opportunityCount * activeFraction),
                1,
                opportunityCount);
        }

        private static CandidateShape AnalyseCandidate(
            StylizedRiverFoamMajorCandidate candidate)
        {
            int resolution = candidate.Resolution;
            byte[] mask = new byte[resolution * resolution];
            candidate.CopyCleanedMaskTo(mask);
            int occupied = 0;
            Vector2 centroid = Vector2.zero;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    if (mask[x + y * resolution] == 0)
                    {
                        continue;
                    }

                    occupied++;
                    centroid += new Vector2(x + 0.5f, y + 0.5f);
                }
            }

            if (occupied <= 0)
            {
                return default;
            }

            centroid /= occupied;
            float covarianceX = 0f;
            float covarianceY = 0f;
            float covarianceXY = 0f;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    if (mask[x + y * resolution] == 0)
                    {
                        continue;
                    }

                    float dx = x + 0.5f - centroid.x;
                    float dy = y + 0.5f - centroid.y;
                    covarianceX += dx * dx;
                    covarianceY += dy * dy;
                    covarianceXY += dx * dy;
                }
            }

            float principalAngle = 0.5f * Mathf.Atan2(
                2f * covarianceXY,
                covarianceX - covarianceY);
            float cosine = Mathf.Cos(principalAngle);
            float sine = Mathf.Sin(principalAngle);
            float majorHalfExtent = 0f;
            float minorHalfExtent = 0f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    if (mask[x + y * resolution] == 0)
                    {
                        continue;
                    }

                    float dx = x + 0.5f - centroid.x;
                    float dy = y + 0.5f - centroid.y;
                    float major = cosine * dx + sine * dy;
                    float minor = -sine * dx + cosine * dy;
                    majorHalfExtent = Mathf.Max(
                        majorHalfExtent,
                        Mathf.Abs(major));
                    minorHalfExtent = Mathf.Max(
                        minorHalfExtent,
                        Mathf.Abs(minor));
                }
            }

            majorHalfExtent += 3f;
            minorHalfExtent += 3f;
            if (minorHalfExtent > majorHalfExtent)
            {
                (majorHalfExtent, minorHalfExtent) =
                    (minorHalfExtent, majorHalfExtent);
                principalAngle = WrapHalfTurn(
                    principalAngle + Mathf.PI * 0.5f);
            }

            return new CandidateShape(
                centroid,
                principalAngle,
                majorHalfExtent,
                minorHalfExtent);
        }

        private static CandidateEvaluation EvaluatePlacement(
            StylizedRiverFoamMajorCandidate candidate,
            CandidateShape shape,
            MajorOpportunity opportunity,
            float centreAcrossNormalized,
            float orientationRadians,
            float requestedDiameter,
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            float[] fluidCoverage,
            bool[] validCells,
            float[] obstacleMask,
            float[] accumulatedSupport,
            float[] candidateField,
            List<AcceptedPlacement> acceptedPlacements,
            int[] lateralPopulation,
            float averageVisibleWidth,
            int coveredCellCount,
            int maximumCoveredCells)
        {
            StylizedRiverSplineSample centreSample =
                domain.SampleAtOrientedDistance(
                    opportunity.CentreLocalDistance);
            float localVisibleWidth = Mathf.Max(
                0.2f,
                centreSample.LeftHalfWidth + centreSample.RightHalfWidth);
            if (localVisibleWidth < 0.65f)
            {
                return CandidateEvaluation.Rejected(
                    StylizedRiverFoamMajorPlacementRejectionReason
                        .WidthCapacity);
            }

            float metresPerCandidateCell = requestedDiameter /
                Mathf.Max(1f, shape.MajorHalfExtentCells * 2f);
            float cosineOrientation = Mathf.Cos(orientationRadians);
            float sineOrientation = Mathf.Sin(orientationRadians);
            float crossRadius = metresPerCandidateCell * (
                Mathf.Abs(sineOrientation) * shape.MajorHalfExtentCells +
                Mathf.Abs(cosineOrientation) * shape.MinorHalfExtentCells);
            float maximumCrossRadius = localVisibleWidth * 0.42f;
            if (crossRadius > maximumCrossRadius)
            {
                metresPerCandidateCell *= maximumCrossRadius /
                    Mathf.Max(0.001f, crossRadius);
                crossRadius = maximumCrossRadius;
            }

            float resolvedDiameter = metresPerCandidateCell *
                shape.MajorHalfExtentCells * 2f;
            if (resolvedDiameter < 0.62f)
            {
                return CandidateEvaluation.Rejected(
                    StylizedRiverFoamMajorPlacementRejectionReason
                        .WidthCapacity);
            }

            float alongRadius = metresPerCandidateCell * (
                Mathf.Abs(cosineOrientation) * shape.MajorHalfExtentCells +
                Mathf.Abs(sineOrientation) * shape.MinorHalfExtentCells);
            float minimumLocalDistance =
                opportunity.CentreLocalDistance - alongRadius;
            float maximumLocalDistance =
                opportunity.CentreLocalDistance + alongRadius;
            int minimumX = Mathf.Clamp(
                Mathf.FloorToInt(
                    minimumLocalDistance / fieldLength * width) - 1,
                0,
                width - 1);
            int maximumX = Mathf.Clamp(
                Mathf.CeilToInt(
                    maximumLocalDistance / fieldLength * width) + 1,
                0,
                width - 1);

            float totalWeight = 0f;
            float effectiveWeight = 0f;
            float bankClippedWeight = 0f;
            float obstacleWeight = 0f;
            float outsideDomainWeight = 0f;
            float overlapWeight = 0f;
            int newCoveredCells = 0;
            float candidatePrincipalCosine = Mathf.Cos(
                shape.PrincipalAngleRadians);
            float candidatePrincipalSine = Mathf.Sin(
                shape.PrincipalAngleRadians);

            for (int x = minimumX; x <= maximumX; x++)
            {
                float localDistance = (x + 0.5f) /
                    Mathf.Max(1f, width) * fieldLength;
                float clampedLocalDistance = Mathf.Clamp(
                    localDistance,
                    0f,
                    validFieldLength);
                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(clampedLocalDistance);
                float centreAcrossMetres = SignedNormalizedToMetres(
                    centreAcrossNormalized,
                    sample.LeftHalfWidth,
                    sample.RightHalfWidth);
                float deltaAlong = localDistance -
                    opportunity.CentreLocalDistance;

                for (int y = 0; y < height; y++)
                {
                    float across01 = (y + 0.5f) /
                        Mathf.Max(1f, height);
                    float acrossMetres = Across01ToMetres(
                        across01,
                        sample.LeftSurfaceHalfWidth,
                        sample.RightSurfaceHalfWidth);
                    float deltaAcross = acrossMetres - centreAcrossMetres;
                    float principalMajorMetres =
                        cosineOrientation * deltaAlong +
                        sineOrientation * deltaAcross;
                    float principalMinorMetres =
                        -sineOrientation * deltaAlong +
                        cosineOrientation * deltaAcross;
                    float sourceMajor = principalMajorMetres /
                        Mathf.Max(0.0001f, metresPerCandidateCell);
                    float sourceMinor = principalMinorMetres /
                        Mathf.Max(0.0001f, metresPerCandidateCell);
                    float sourceX = shape.Centroid.x +
                        candidatePrincipalCosine * sourceMajor -
                        candidatePrincipalSine * sourceMinor;
                    float sourceY = shape.Centroid.y +
                        candidatePrincipalSine * sourceMajor +
                        candidatePrincipalCosine * sourceMinor;
                    float candidateSupport =
                        candidate.SampleFinalSupportBilinear(
                            sourceX,
                            sourceY);
                    if (candidateSupport <= 0.001f)
                    {
                        continue;
                    }

                    int index = x + y * width;
                    float fluid = fluidCoverage[index];
                    float obstacle = SampleObstacle(obstacleMask, index);
                    totalWeight += candidateSupport;
                    bankClippedWeight += candidateSupport * (1f - fluid);
                    obstacleWeight += candidateSupport * obstacle;
                    if (localDistance < 0f ||
                        localDistance > validFieldLength)
                    {
                        outsideDomainWeight += candidateSupport;
                    }

                    float effective = candidateSupport * fluid *
                        (1f - obstacle);
                    if (effective <= 0.001f)
                    {
                        continue;
                    }

                    candidateField[index] = effective;
                    effectiveWeight += effective;
                    overlapWeight += effective *
                        accumulatedSupport[index];
                    if (validCells[index] &&
                        accumulatedSupport[index] < MajorCoverageThreshold &&
                        effective >= MajorCoverageThreshold)
                    {
                        newCoveredCells++;
                    }
                }
            }

            if (totalWeight <= 0.001f || effectiveWeight <= 0.001f)
            {
                return CandidateEvaluation.Rejected(
                    StylizedRiverFoamMajorPlacementRejectionReason
                        .NoRasterCoverage);
            }

            float bankClippingRatio = bankClippedWeight / totalWeight;
            float obstacleRatio = obstacleWeight / totalWeight;
            float outsideDomainRatio = outsideDomainWeight / totalWeight;
            float overlapRatio = overlapWeight / effectiveWeight;
            int coveredCellCountAfter = coveredCellCount + newCoveredCells;
            float spacingScore = EvaluateSpacing(
                opportunity.CentreLocalDistance,
                centreAcrossNormalized,
                alongRadius,
                crossRadius,
                domain,
                acceptedPlacements,
                out bool crowdingFailure);

            StylizedRiverFoamMajorPlacementRejectionReason rejectionReason =
                StylizedRiverFoamMajorPlacementRejectionReason.None;
            if (outsideDomainRatio > 0.10f)
            {
                rejectionReason =
                    StylizedRiverFoamMajorPlacementRejectionReason
                        .OutsideValidWater;
            }
            else if (obstacleRatio > 0.055f)
            {
                rejectionReason =
                    StylizedRiverFoamMajorPlacementRejectionReason
                        .ObstacleOverlap;
            }
            else if (bankClippingRatio > 0.22f ||
                     effectiveWeight / totalWeight < 0.68f)
            {
                rejectionReason =
                    StylizedRiverFoamMajorPlacementRejectionReason
                        .BankClipping;
            }
            else if (crowdingFailure || overlapRatio > 0.34f ||
                     newCoveredCells < 4)
            {
                rejectionReason =
                    StylizedRiverFoamMajorPlacementRejectionReason
                        .Crowding;
            }
            else if (coveredCellCountAfter > maximumCoveredCells)
            {
                rejectionReason =
                    StylizedRiverFoamMajorPlacementRejectionReason
                        .OpenWaterLimit;
            }

            int lateralBin = ResolveLateralBin(centreAcrossNormalized);
            int minimumPopulation = Mathf.Min(
                lateralPopulation[0],
                Mathf.Min(lateralPopulation[1], lateralPopulation[2]));
            float lateralBalanceScore = Mathf.Clamp(
                minimumPopulation + 1 - lateralPopulation[lateralBin],
                -3,
                3) * 0.045f;
            float bankContext = 1f - Mathf.Clamp01(
                Mathf.Abs(Mathf.Abs(centreAcrossNormalized) - 0.48f) /
                0.48f);
            float constrictionContext = Mathf.Clamp(
                (averageVisibleWidth - localVisibleWidth) /
                Mathf.Max(0.25f, averageVisibleWidth),
                -0.5f,
                0.7f);
            float leeContext = HasUpstreamObstacle(
                opportunity.CentreLocalDistance,
                centreAcrossNormalized,
                width,
                height,
                fieldLength,
                validFieldLength,
                obstacleMask)
                    ? 1f
                    : 0f;
            float orientationPenalty = EvaluateOrientationRepetition(
                opportunity.CentreLocalDistance,
                orientationRadians,
                shape.AspectRatio,
                acceptedPlacements);
            float score =
                newCoveredCells * 0.01f +
                spacingScore * 0.18f +
                lateralBalanceScore +
                bankContext * 0.055f +
                constrictionContext * 0.08f +
                leeContext * 0.12f -
                overlapRatio * 0.42f -
                bankClippingRatio * 0.32f -
                obstacleRatio * 0.65f -
                orientationPenalty;

            float anchoringStrength = Mathf.Clamp01(
                bankContext * 0.35f +
                Mathf.Max(0f, constrictionContext) * 0.25f +
                leeContext * 0.55f);
            return new CandidateEvaluation(
                rejectionReason ==
                    StylizedRiverFoamMajorPlacementRejectionReason.None,
                rejectionReason,
                score,
                opportunity.CentreLocalDistance,
                centreAcrossNormalized,
                orientationRadians,
                metresPerCandidateCell,
                alongRadius,
                crossRadius,
                coveredCellCountAfter,
                anchoringStrength);
        }

        private static float EvaluateSpacing(
            float centreLocalDistance,
            float centreAcrossNormalized,
            float alongRadius,
            float crossRadius,
            RiverDomainSnapshot domain,
            List<AcceptedPlacement> acceptedPlacements,
            out bool crowdingFailure)
        {
            crowdingFailure = false;
            float minimumNormalisedDistance = 4f;
            StylizedRiverSplineSample sample =
                domain.SampleAtOrientedDistance(centreLocalDistance);
            float acrossMetres = SignedNormalizedToMetres(
                centreAcrossNormalized,
                sample.LeftHalfWidth,
                sample.RightHalfWidth);

            for (int index = 0; index < acceptedPlacements.Count; index++)
            {
                AcceptedPlacement accepted = acceptedPlacements[index];
                StylizedRiverSplineSample acceptedSample =
                    domain.SampleAtOrientedDistance(
                        accepted.CentreLocalDistance);
                float acceptedAcrossMetres = SignedNormalizedToMetres(
                    accepted.CentreAcrossNormalized,
                    acceptedSample.LeftHalfWidth,
                    acceptedSample.RightHalfWidth);
                float alongDistance = Mathf.Abs(
                    centreLocalDistance - accepted.CentreLocalDistance) /
                    Mathf.Max(
                        0.25f,
                        (alongRadius + accepted.AlongRadius) * 0.82f);
                float acrossDistance = Mathf.Abs(
                    acrossMetres - acceptedAcrossMetres) /
                    Mathf.Max(
                        0.2f,
                        (crossRadius + accepted.CrossRadius) * 0.82f);
                float normalisedDistance = Mathf.Sqrt(
                    alongDistance * alongDistance +
                    acrossDistance * acrossDistance);
                minimumNormalisedDistance = Mathf.Min(
                    minimumNormalisedDistance,
                    normalisedDistance);
            }

            crowdingFailure = minimumNormalisedDistance < 0.72f;
            return Mathf.Clamp01(
                Mathf.InverseLerp(0.72f, 1.65f, minimumNormalisedDistance));
        }

        private static float EvaluateOrientationRepetition(
            float centreLocalDistance,
            float orientationRadians,
            float aspectRatio,
            List<AcceptedPlacement> acceptedPlacements)
        {
            float penalty = 0f;
            for (int index = 0; index < acceptedPlacements.Count; index++)
            {
                AcceptedPlacement accepted = acceptedPlacements[index];
                float alongDistance = Mathf.Abs(
                    centreLocalDistance - accepted.CentreLocalDistance);
                if (alongDistance > 8f)
                {
                    continue;
                }

                float angleDifference = HalfTurnAngleDifference(
                    orientationRadians,
                    accepted.OrientationRadians);
                float angleSimilarity = 1f - Mathf.Clamp01(
                    angleDifference / (Mathf.Deg2Rad * 24f));
                float aspectSimilarity = 1f - Mathf.Clamp01(
                    Mathf.Abs(aspectRatio - accepted.AspectRatio) / 0.55f);
                penalty += angleSimilarity * aspectSimilarity * 0.045f;
            }

            return Mathf.Min(0.18f, penalty);
        }

        private static bool HasUpstreamObstacle(
            float centreLocalDistance,
            float centreAcrossNormalized,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            float[] obstacleMask)
        {
            if (obstacleMask == null || obstacleMask.Length != width * height)
            {
                return false;
            }

            int centreX = Mathf.Clamp(
                Mathf.FloorToInt(
                    centreLocalDistance / fieldLength * width),
                0,
                width - 1);
            int centreY = Mathf.Clamp(
                Mathf.RoundToInt(
                    (centreAcrossNormalized * 0.5f + 0.5f) *
                    (height - 1)),
                0,
                height - 1);
            int upstreamCells = Mathf.Clamp(
                Mathf.CeilToInt(3.2f / fieldLength * width),
                1,
                width);
            int immediateCells = Mathf.Clamp(
                Mathf.FloorToInt(0.35f / fieldLength * width),
                0,
                upstreamCells);
            int lateralCells = Mathf.Max(2, Mathf.RoundToInt(height * 0.08f));

            for (int x = Mathf.Max(0, centreX - upstreamCells);
                 x <= Mathf.Max(0, centreX - immediateCells);
                 x++)
            {
                float localDistance = (x + 0.5f) /
                    Mathf.Max(1f, width) * fieldLength;
                if (localDistance > validFieldLength)
                {
                    continue;
                }

                for (int y = Mathf.Max(0, centreY - lateralCells);
                     y <= Mathf.Min(height - 1, centreY + lateralCells);
                     y++)
                {
                    if (SampleObstacle(obstacleMask, x + y * width) > 0.5f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static int BuildFluidContext(
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            StylizedRiverQuality quality,
            float shoreMotion,
            float[] obstacleMask,
            float[] fluidCoverage,
            bool[] validCells)
        {
            float edgeCells = quality switch
            {
                StylizedRiverQuality.Low => 1.5f,
                StylizedRiverQuality.Medium => 2f,
                StylizedRiverQuality.High => 2.5f,
                _ => 2f
            };
            float animatedEnvelope = Mathf.Lerp(
                0.25f,
                0.90f,
                Mathf.Clamp01(shoreMotion));
            int validCellCount = 0;

            for (int x = 0; x < width; x++)
            {
                float localDistance = x /
                    (float)Mathf.Max(1, width - 1) * fieldLength;
                if (localDistance > validFieldLength + 0.0001f)
                {
                    continue;
                }

                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(
                        Mathf.Min(localDistance, validFieldLength));
                float leftSurface = Mathf.Max(
                    0.05f,
                    sample.LeftSurfaceHalfWidth);
                float rightSurface = Mathf.Max(
                    0.05f,
                    sample.RightSurfaceHalfWidth);
                float leftVisible = Mathf.Max(0.01f, sample.LeftHalfWidth);
                float rightVisible = Mathf.Max(0.01f, sample.RightHalfWidth);
                float leftReach = Mathf.Lerp(
                    leftVisible,
                    leftSurface,
                    animatedEnvelope);
                float rightReach = Mathf.Lerp(
                    rightVisible,
                    rightSurface,
                    animatedEnvelope);
                float edgeWidth = Mathf.Max(
                    0.05f,
                    (leftSurface + rightSurface) /
                    Mathf.Max(1, height - 1) * edgeCells);

                for (int y = 0; y < height; y++)
                {
                    float across01 = y /
                        (float)Mathf.Max(1, height - 1);
                    float lateral = Across01ToMetres(
                        across01,
                        leftSurface,
                        rightSurface);
                    float reach = lateral < 0f ? leftReach : rightReach;
                    float coverage = Mathf.Clamp01(
                        (reach - Mathf.Abs(lateral)) / edgeWidth);
                    int index = x + y * width;
                    fluidCoverage[index] = coverage;
                    bool valid = coverage > 0.001f &&
                        SampleObstacle(obstacleMask, index) < 0.5f;
                    validCells[index] = valid;
                    if (valid)
                    {
                        validCellCount++;
                    }
                }
            }

            return validCellCount;
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

        private static float ResolveOrientationRadians(uint stableId)
        {
            float family = Hash01(stableId, 8u);
            float sign = Hash01(stableId, 9u) < 0.5f ? -1f : 1f;
            float degrees;
            if (family < 0.28f)
            {
                degrees = Mathf.Lerp(-20f, 20f, Hash01(stableId, 10u));
            }
            else if (family < 0.58f)
            {
                degrees = sign * Mathf.Lerp(
                    30f,
                    58f,
                    Hash01(stableId, 10u));
            }
            else if (family < 0.82f)
            {
                degrees = sign * Mathf.Lerp(
                    66f,
                    88f,
                    Hash01(stableId, 10u));
            }
            else
            {
                degrees = Mathf.Lerp(-82f, 82f, Hash01(stableId, 10u));
            }

            return degrees * Mathf.Deg2Rad;
        }

        private static int ResolveLateralBin(float acrossNormalized)
        {
            return acrossNormalized < -0.24f
                ? 0
                : acrossNormalized > 0.24f
                    ? 2
                    : 1;
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

        private static float SignedNormalizedToMetres(
            float acrossNormalized,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            return acrossNormalized < 0f
                ? acrossNormalized * leftHalfWidth
                : acrossNormalized * rightHalfWidth;
        }

        private static float SampleObstacle(float[] obstacleMask, int index)
        {
            return obstacleMask != null &&
                index >= 0 && index < obstacleMask.Length
                    ? Mathf.Clamp01(obstacleMask[index])
                    : 0f;
        }

        private static float HalfTurnAngleDifference(float a, float b)
        {
            float difference = Mathf.Abs(WrapHalfTurn(a - b));
            return Mathf.Min(difference, Mathf.PI - difference);
        }

        private static float WrapHalfTurn(float angle)
        {
            while (angle > Mathf.PI * 0.5f)
            {
                angle -= Mathf.PI;
            }
            while (angle < -Mathf.PI * 0.5f)
            {
                angle += Mathf.PI;
            }

            return angle;
        }

        private static float Fractional(float value)
        {
            return value - Mathf.Floor(value);
        }

        private static float Hash01(uint seed, uint stream)
        {
            return (MixBits(seed ^ MixBits(stream + 0xC2B2AE35u)) &
                0x00FFFFFFu) / 16777216f;
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

        private readonly struct EvolutionAnchorEvaluation
        {
            public EvolutionAnchorEvaluation(
                bool accepted,
                float score,
                float centreLocalDistance,
                float centreAcrossNormalized,
                float orientationRadians,
                float metresPerCandidateCell)
            {
                Accepted = accepted;
                Score = score;
                CentreLocalDistance = centreLocalDistance;
                CentreAcrossNormalized = centreAcrossNormalized;
                OrientationRadians = orientationRadians;
                MetresPerCandidateCell = metresPerCandidateCell;
            }

            public bool Accepted { get; }
            public float Score { get; }
            public float CentreLocalDistance { get; }
            public float CentreAcrossNormalized { get; }
            public float OrientationRadians { get; }
            public float MetresPerCandidateCell { get; }
        }

        private readonly struct MajorOpportunity
        {
            public MajorOpportunity(
                uint stableId,
                int longitudinalIndex,
                int lateralIndex,
                float centreLocalDistance,
                float acrossNormalized,
                float activationRank,
                int candidateSeed,
                float sizeSequenceOffset)
            {
                StableId = stableId;
                LongitudinalIndex = longitudinalIndex;
                LateralIndex = lateralIndex;
                CentreLocalDistance = centreLocalDistance;
                AcrossNormalized = acrossNormalized;
                ActivationRank = activationRank;
                CandidateSeed = candidateSeed;
                SizeSequenceOffset = sizeSequenceOffset;
            }

            public uint StableId { get; }
            public int LongitudinalIndex { get; }
            public int LateralIndex { get; }
            public float CentreLocalDistance { get; }
            public float AcrossNormalized { get; }
            public float ActivationRank { get; }
            public int CandidateSeed { get; }
            public float SizeSequenceOffset { get; }
        }

        private readonly struct CandidateShape
        {
            public CandidateShape(
                Vector2 centroid,
                float principalAngleRadians,
                float majorHalfExtentCells,
                float minorHalfExtentCells)
            {
                Centroid = centroid;
                PrincipalAngleRadians = principalAngleRadians;
                MajorHalfExtentCells = majorHalfExtentCells;
                MinorHalfExtentCells = minorHalfExtentCells;
            }

            public Vector2 Centroid { get; }
            public float PrincipalAngleRadians { get; }
            public float MajorHalfExtentCells { get; }
            public float MinorHalfExtentCells { get; }
            public bool IsValid => MajorHalfExtentCells > 0.5f &&
                MinorHalfExtentCells > 0.5f;
            public float AspectRatio => MajorHalfExtentCells /
                Mathf.Max(0.5f, MinorHalfExtentCells);
        }

        private readonly struct AcceptedPlacement
        {
            public AcceptedPlacement(
                float centreLocalDistance,
                float centreAcrossNormalized,
                float alongRadius,
                float crossRadius,
                float orientationRadians,
                float aspectRatio)
            {
                CentreLocalDistance = centreLocalDistance;
                CentreAcrossNormalized = centreAcrossNormalized;
                AlongRadius = alongRadius;
                CrossRadius = crossRadius;
                OrientationRadians = orientationRadians;
                AspectRatio = aspectRatio;
            }

            public float CentreLocalDistance { get; }
            public float CentreAcrossNormalized { get; }
            public float AlongRadius { get; }
            public float CrossRadius { get; }
            public float OrientationRadians { get; }
            public float AspectRatio { get; }
        }

        private readonly struct CandidateEvaluation
        {
            public CandidateEvaluation(
                bool accepted,
                StylizedRiverFoamMajorPlacementRejectionReason rejectionReason,
                float score,
                float centreLocalDistance,
                float centreAcrossNormalized,
                float orientationRadians,
                float metresPerCandidateCell,
                float alongRadius,
                float crossRadius,
                int coveredCellCountAfter,
                float anchoringStrength)
            {
                Accepted = accepted;
                RejectionReason = rejectionReason;
                Score = score;
                CentreLocalDistance = centreLocalDistance;
                CentreAcrossNormalized = centreAcrossNormalized;
                OrientationRadians = orientationRadians;
                MetresPerCandidateCell = metresPerCandidateCell;
                AlongRadius = alongRadius;
                CrossRadius = crossRadius;
                CoveredCellCountAfter = coveredCellCountAfter;
                AnchoringStrength = anchoringStrength;
            }

            public bool Accepted { get; }
            public StylizedRiverFoamMajorPlacementRejectionReason
                RejectionReason { get; }
            public float Score { get; }
            public float CentreLocalDistance { get; }
            public float CentreAcrossNormalized { get; }
            public float OrientationRadians { get; }
            public float MetresPerCandidateCell { get; }
            public float AlongRadius { get; }
            public float CrossRadius { get; }
            public int CoveredCellCountAfter { get; }
            public float AnchoringStrength { get; }

            public static CandidateEvaluation Rejected(
                StylizedRiverFoamMajorPlacementRejectionReason reason)
            {
                return new CandidateEvaluation(
                    false,
                    reason,
                    float.NegativeInfinity,
                    0f,
                    0f,
                    0f,
                    0f,
                    0f,
                    0f,
                    0,
                    0f);
            }
        }
    }
}
