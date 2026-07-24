struct FoamMaterialTopologySample
{
    float majorSupport;
    float connectorSupport;
    float negativeAgingPressure;
    float pressureSupport;
    float leeSupport;
    float shoreSupport;
    float combinedAnchoredSupport;
    float obstacleFootprint;
    float validFluid;
};


FoamMaterialTopologySample FoamResolveMaterialTopology(
    float4 topology,
    float4 anchoredSources,
    float boundaryCoverage,
    float canonicalObstacleFootprint)
{
    FoamMaterialTopologySample sample;
    sample.majorSupport = saturate(topology.r);
    sample.connectorSupport = saturate(topology.g);
    sample.negativeAgingPressure = saturate(topology.b);
    sample.pressureSupport = saturate(anchoredSources.r);
    sample.leeSupport = saturate(anchoredSources.g);
    sample.shoreSupport = saturate(anchoredSources.b);
    sample.combinedAnchoredSupport = max(
        max(sample.pressureSupport, sample.leeSupport),
        sample.shoreSupport);

    sample.obstacleFootprint = saturate(canonicalObstacleFootprint);
    sample.validFluid = saturate(boundaryCoverage) *
        (1.0 - sample.obstacleFootprint);
    return sample;
}


float FoamCombinedMaterialSupport(FoamMaterialTopologySample sample)
{
    return max(
        max(sample.majorSupport, sample.connectorSupport),
        sample.combinedAnchoredSupport);
}


float FoamShapeSupportAgingInfluence(float value)
{
    float fullSupportedAgingAt =
        _FoamFullSupportedAgingAt >= 0.15
            ? min(_FoamFullSupportedAgingAt, 1.0)
            : 0.92;
    return smoothstep(
        0.08,
        fullSupportedAgingAt,
        saturate(value));
}


float FoamShapeNegativeAgingInfluence(float value)
{
    // Negative Aging Pressure keeps its accepted fixed response. The authored
    // support-saturation control must not change hostile-water behavior.
    return smoothstep(0.08, 0.92, saturate(value));
}


float FoamResolveLocalAgeRate(FoamMaterialTopologySample materialTopology)
{
    float shapedSupport = FoamShapeSupportAgingInfluence(
        FoamCombinedMaterialSupport(materialTopology));
    float shapedNegative = FoamShapeNegativeAgingInfluence(
        materialTopology.negativeAgingPressure);

    // Negative Aging Pressure should mean hostile water, not merely a
    // multiplicative counterweight that can still leave full support aging
    // slower than neutral. Suppress the preservation influence first, then
    // apply the negative aging multiplier. This keeps overlap edges blended
    // while making negative cores actually consume material.
    float effectiveSupport = shapedSupport * (1.0 - shapedNegative);

    float positiveAgeFactor = lerp(
        1.0,
        max(0.05, _FoamPositiveAgeMultiplier),
        effectiveSupport);
    float negativeAgeFactor = lerp(
        1.0,
        max(1.0, _FoamNegativeAgeMultiplier),
        shapedNegative);
    return positiveAgeFactor * negativeAgeFactor;
}


bool IsObstacleIntervalHeightInside(
    float4 intervals,
    float waterHeight)
{
    bool insideFirst =
        intervals.y > intervals.x &&
        waterHeight >= intervals.x &&
        waterHeight <= intervals.y;
    bool insideSecond =
        intervals.w > intervals.z &&
        waterHeight >= intervals.z &&
        waterHeight <= intervals.w;
    return insideFirst || insideSecond;
}


bool IsObstacleIntervalSampleInside(
    int sampleOffset,
    int sampleIndex)
{
    FoamObstacleSample sample =
        _FoamObstacleSamples[sampleOffset + sampleIndex];
    float globalDistance = sample.waterParameters.x;
    float lateralMetres = sample.waterParameters.y;
    float visibleHalfWidth = max(0.01, sample.waterParameters.z);
    float surfaceHalfWidth = max(0.01, sample.waterParameters.w);
    float liquidFactor = 1.0 - saturate(_FoamFreezeAmount);
    float bankMask;
    float dynamicHeight = RiverWaterEvaluateSurfaceHeight(
        globalDistance,
        lateralMetres,
        visibleHalfWidth,
        surfaceHalfWidth,
        _FoamTopologyEvaluationTime,
        _FoamMotionFlowSpeed,
        _FoamMotionWaveHeight,
        _FoamMotionWaveLength,
        _FoamMotionWaveSteepness,
        _FoamMotionTurbulence,
        _FoamShoreMotion,
        _FoamShoreMotionWidth,
        _FoamShoreWaveHeightScale,
        _FoamShoreWaveLengthScale,
        _FoamShoreWaveSpacingScale,
        _FoamShoreWaveReach,
        _FoamShoreWaveTransitionLength,
        _FoamShoreWaveSizeVariation,
        _FoamShoreWaveSideAsymmetry,
        _FoamShoreWaveProfileVariation,
        _FoamShoreWaveProfileEvolutionStrength,
        _FoamShoreWaveProfileEvolutionDuration,
        liquidFactor,
        _FoamSeed,
        bankMask);

    return IsObstacleIntervalHeightInside(
        sample.intervals,
        dynamicHeight);
}
