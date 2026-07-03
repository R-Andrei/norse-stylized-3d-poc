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
    float obstacleFootprintCopy;
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

    // _FoamObstacleExclusionRead is authoritative. topology.a remains only a
    // same-grid compatibility/debug copy and must not be multiplied with the
    // canonical footprint or applied as a second exclusion.
    sample.obstacleFootprint = saturate(canonicalObstacleFootprint);
    sample.obstacleFootprintCopy = saturate(topology.a);
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
