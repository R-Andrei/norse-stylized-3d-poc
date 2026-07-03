
float4 EvaluateNetworkSample(float2 uv)
{
    float agitation = saturate(_FoamEvolution / 1.8);
    int metricX = FoamUVToContainingTexel(uv.x, _FoamDimensions.x);
    FoamMetricRow metric = _FoamMetricRows[metricX];
    float globalDistance = _FoamGlobalStart + saturate(uv.x) * _FoamFieldLength;
    float lateralMetres = FoamAcross01ToMetres(
        saturate(uv.y),
        metric.widthsAndSpacing.x,
        metric.widthsAndSpacing.y);

    // Build the skeleton in metric river space. Previous normalized-UV cells
    // stretched with river length and produced long parallel lanes. Metric
    // cells preserve a stable pocket size through width changes and quality
    // tiers, with only a restrained downstream elongation.
    float2 world = float2(globalDistance, lateralMetres);
    float warpClock = _FoamTime * lerp(0.025, 0.090, agitation);
    float warpA = FoamFbm(
        world * float2(0.075, 0.12) +
        float2(warpClock, -warpClock * 0.63) +
        _FoamSeed * 0.0013);
    float warpB = FoamFbm(
        world.yx * float2(0.14, 0.065) +
        float2(-warpClock * 0.41, warpClock * 0.74) +
        31.7 + _FoamSeed * 0.0021);
    float2 warpedWorld = world + float2(
        (warpA - 0.5) * lerp(0.55, 1.45, agitation),
        (warpB - 0.5) * lerp(0.65, 1.65, agitation));

    float coarseSize = lerp(13.5, 10.5, agitation);
    float mediumSize = lerp(7.0, 5.0, agitation);
    float fineSize = lerp(3.8, 2.7, agitation);

    float coarseDistance = VoronoiEdgeDistance(
        warpedWorld / float2(coarseSize * 1.16, coarseSize),
        _FoamTime * lerp(0.055, 0.13, agitation),
        _FoamSeed * 0.013 + 7.1);
    float mediumDistance = VoronoiEdgeDistance(
        (warpedWorld + float2(2.7, -1.9)) /
            float2(mediumSize * 1.06, mediumSize),
        _FoamTime * lerp(0.085, 0.20, agitation),
        _FoamSeed * 0.029 + 19.7);
    float fineDistance = VoronoiEdgeDistance(
        (warpedWorld + float2(-1.4, 1.1)) /
            float2(fineSize * 0.98, fineSize),
        _FoamTime * lerp(0.12, 0.28, agitation),
        _FoamSeed * 0.047 + 41.3);

    float coarseLane = 1.0 - smoothstep(0.022, 0.085, coarseDistance);
    float mediumLane = 1.0 - smoothstep(0.014, 0.060, mediumDistance);
    float fineLane = 1.0 - smoothstep(0.010, 0.045, fineDistance);

    float fineGateNoise = FoamFbm(
        warpedWorld * float2(0.19, 0.27) +
        float2(
            _FoamTime * lerp(0.018, 0.060, agitation),
            -_FoamTime * lerp(0.015, 0.052, agitation)) +
        _FoamSeed * 0.0031);
    float fineGate = smoothstep(0.37, 0.62, fineGateNoise);
    fineLane *= fineGate;

    // Medium lanes are the dominant web. Coarse lanes provide only broad
    // structure and fine lanes provide incomplete connectors rather than a
    // second set of continuous parallel streaks.
    float lane = saturate(max(
        coarseLane * 0.50,
        max(mediumLane, fineLane * 0.86)));
    float junction = saturate(
        coarseLane * mediumLane * 0.62 +
        mediumLane * fineLane * 1.08 +
        coarseLane * fineLane * 0.42);
    float distanceField = min(
        coarseDistance * 1.45,
        min(mediumDistance, fineDistance * 0.88 + (1.0 - fineGate) * 0.20));

    return float4(distanceField, lane, junction, fineGate);
}


float EvaluateNetworkDistance(float2 uv)
{
    return EvaluateNetworkSample(uv).x;
}
