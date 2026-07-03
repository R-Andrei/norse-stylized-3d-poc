

float FoamCombinedAnchoredSupport(float4 anchoredSources)
{
    return max(
        max(anchoredSources.r, anchoredSources.g),
        anchoredSources.b);
}


float FoamCombinedNegativeInfluence(float4 topology)
{
    return max(topology.b, topology.a);
}

float FoamComposeLegacyNetSupport(
    float4 topology,
    float4 anchoredSources)
{
    float combinedSupport = max(
        max(topology.r, topology.g),
        FoamCombinedAnchoredSupport(anchoredSources));
    return saturate(
        combinedSupport *
        (1.0 - FoamCombinedNegativeInfluence(topology)));
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
        _FoamTime,
        _FoamMotionFlowSpeed,
        _FoamMotionWaveHeight,
        _FoamMotionWaveLength,
        _FoamMotionWaveSteepness,
        _FoamMotionTurbulence,
        _FoamShoreMotion,
        _FoamShoreMotionWidth,
        _FoamShoreWaveHeightScale,
        _FoamShoreWaveLengthScale,
        _FoamShoreWaveReach,
        _FoamShoreWaveTransitionLength,
        _FoamShoreWaveSizeVariation,
        _FoamShoreWaveSideAsymmetry,
        _FoamShoreWaveProfileVariation,
        liquidFactor,
        _FoamSeed,
        bankMask);

    return IsObstacleIntervalHeightInside(
        sample.intervals,
        dynamicHeight);
}
