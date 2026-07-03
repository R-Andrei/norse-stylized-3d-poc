void ResolveFoamDirectionalNeighbourhood(
    int x,
    int y,
    float globalDistance,
    float lateralMetres,
    float phase,
    float amount,
    float agitation,
    out float directionalAverage,
    out float edgeExposure,
    out float neckness,
    out float tipness,
    out float phaseStress,
    out float broadAmount,
    out float enclosedSupport,
    out float neighbourPhase)
{
    float baseAngle = phase * 6.2831853 +
        (FoamFbm(float2(globalDistance * 0.19, lateralMetres * 0.41) + _FoamSeed * 0.0017) - 0.5) * 2.6;
    float radius = lerp(1.15, 1.70, agitation);
    float2 d0 = float2(cos(baseAngle), sin(baseAngle));
    float2 d1 = float2(cos(baseAngle + 0.7853982), sin(baseAngle + 0.7853982));
    float2 d2 = float2(cos(baseAngle + 1.5707963), sin(baseAngle + 1.5707963));
    float2 d3 = float2(cos(baseAngle + 2.3561945), sin(baseAngle + 2.3561945));

    float4 s0 = SampleAdvectedBilinear(float2(x, y) + d0 * radius);
    float4 s1 = SampleAdvectedBilinear(float2(x, y) + d1 * radius);
    float4 s2 = SampleAdvectedBilinear(float2(x, y) + d2 * radius);
    float4 s3 = SampleAdvectedBilinear(float2(x, y) + d3 * radius);
    float4 s4 = SampleAdvectedBilinear(float2(x, y) - d0 * radius);
    float4 s5 = SampleAdvectedBilinear(float2(x, y) - d1 * radius);
    float4 s6 = SampleAdvectedBilinear(float2(x, y) - d2 * radius);
    float4 s7 = SampleAdvectedBilinear(float2(x, y) - d3 * radius);

    directionalAverage =
        (s0.x + s1.x + s2.x + s3.x + s4.x + s5.x + s6.x + s7.x) * 0.125;
    float pair0 = min(s0.x, s4.x);
    float pair1 = min(s1.x, s5.x);
    float pair2 = min(s2.x, s6.x);
    float pair3 = min(s3.x, s7.x);
    float strongestPair = max(max(pair0, pair1), max(pair2, pair3));
    float weakestPair = min(min(pair0, pair1), min(pair2, pair3));
    edgeExposure = saturate(
        1.0 - directionalAverage / max(0.035, amount) +
        max(0.0, amount - directionalAverage) * 0.72);
    neckness =
        smoothstep(0.09, 0.38, strongestPair) *
        (1.0 - smoothstep(0.055, 0.24, weakestPair)) *
        smoothstep(0.045, 0.40, amount);
    tipness =
        smoothstep(0.035, 0.28, amount) *
        (1.0 - smoothstep(0.08, 0.34, directionalAverage));

    float directionalWeight =
        s0.x + s1.x + s2.x + s3.x + s4.x + s5.x + s6.x + s7.x;
    neighbourPhase = phase;
    if (directionalWeight > 0.001)
    {
        float weightedSin =
            sin(s0.w * 6.2831853) * s0.x + sin(s1.w * 6.2831853) * s1.x +
            sin(s2.w * 6.2831853) * s2.x + sin(s3.w * 6.2831853) * s3.x +
            sin(s4.w * 6.2831853) * s4.x + sin(s5.w * 6.2831853) * s5.x +
            sin(s6.w * 6.2831853) * s6.x + sin(s7.w * 6.2831853) * s7.x;
        float weightedCos =
            cos(s0.w * 6.2831853) * s0.x + cos(s1.w * 6.2831853) * s1.x +
            cos(s2.w * 6.2831853) * s2.x + cos(s3.w * 6.2831853) * s3.x +
            cos(s4.w * 6.2831853) * s4.x + cos(s5.w * 6.2831853) * s5.x +
            cos(s6.w * 6.2831853) * s6.x + cos(s7.w * 6.2831853) * s7.x;
        neighbourPhase = frac(atan2(weightedSin, weightedCos) / 6.2831853 + 1.0);
    }

    phaseStress =
        (PhaseDistance(s0.w, phase) * s0.x + PhaseDistance(s1.w, phase) * s1.x +
         PhaseDistance(s2.w, phase) * s2.x + PhaseDistance(s3.w, phase) * s3.x +
         PhaseDistance(s4.w, phase) * s4.x + PhaseDistance(s5.w, phase) * s5.x +
         PhaseDistance(s6.w, phase) * s6.x + PhaseDistance(s7.w, phase) * s7.x) /
        max(0.001, directionalWeight);

    float broadRadius = lerp(2.5, 3.5, agitation);
    // Four phase-rotated broad samples are sufficient for source suppression;
    // the fracture field now owns coherent multi-directional stress. Avoid a
    // second eight-sample neighbourhood in every full-resolution cell.
    broadAmount =
        (SampleAdvectedBilinear(float2(x, y) + d0 * broadRadius).x +
         SampleAdvectedBilinear(float2(x, y) - d0 * broadRadius).x +
         SampleAdvectedBilinear(float2(x, y) + d2 * broadRadius).x +
         SampleAdvectedBilinear(float2(x, y) - d2 * broadRadius).x) * 0.25;

    enclosedSupport =
        step(_FoamVisibleThreshold * 0.80, s0.x) + step(_FoamVisibleThreshold * 0.80, s1.x) +
        step(_FoamVisibleThreshold * 0.80, s2.x) + step(_FoamVisibleThreshold * 0.80, s3.x) +
        step(_FoamVisibleThreshold * 0.80, s4.x) + step(_FoamVisibleThreshold * 0.80, s5.x) +
        step(_FoamVisibleThreshold * 0.80, s6.x) + step(_FoamVisibleThreshold * 0.80, s7.x);
}

float ResolveFoamSourceNeed(int x, float targetCoverage)
{
    float chunkPosition =
        ((float)x + 0.5) / max(1.0, (float)_FoamResolutionPerChunk) - 0.5;
    int chunkBase = (int)floor(chunkPosition);
    int chunkA = clamp(chunkBase, 0, max(0, _FoamChunkCount - 1));
    int chunkB = clamp(chunkBase + 1, 0, max(0, _FoamChunkCount - 1));
    float chunkBlend = frac(chunkPosition);
    uint byteOffsetA = (uint)chunkA * 32u;
    uint byteOffsetB = (uint)chunkB * 32u;
    uint4 metricsA0 = _FoamPopulationMetrics.Load4(byteOffsetA);
    uint4 metricsB0 = _FoamPopulationMetrics.Load4(byteOffsetB);
    uint4 metricsA1 = _FoamPopulationMetrics.Load4(byteOffsetA + 16u);
    uint4 metricsB1 = _FoamPopulationMetrics.Load4(byteOffsetB + 16u);
    float validA = max(1.0, (float)metricsA0.z);
    float validB = max(1.0, (float)metricsB0.z);
    float laneValidA = max(1.0, (float)metricsA0.w);
    float laneValidB = max(1.0, (float)metricsB0.w);
    float visibleCoverage = lerp((float)metricsA0.y / validA, (float)metricsB0.y / validB, chunkBlend);
    float perimeterCoverage = lerp((float)metricsA1.x / validA, (float)metricsB1.x / validB, chunkBlend);
    float interiorCoverage = lerp((float)metricsA1.y / validA, (float)metricsB1.y / validB, chunkBlend);
    float laneCoverage = lerp((float)metricsA1.w / laneValidA, (float)metricsB1.w / laneValidB, chunkBlend);
    float perimeterRatio = perimeterCoverage / max(0.001, visibleCoverage);

    float populationDeficit = targetCoverage > 0.0001
        ? saturate((targetCoverage - visibleCoverage) / max(0.025, targetCoverage))
        : 0.0;
    float laneTarget = lerp(0.44, 0.72, saturate(targetCoverage / 0.30));
    float laneDeficit = saturate((laneTarget - laneCoverage) / max(0.12, laneTarget));
    float interiorLimit = max(0.014, targetCoverage * 0.28);
    float interiorPenalty = saturate(1.0 - interiorCoverage / interiorLimit);
    float complexityNeed = saturate((0.66 - perimeterRatio) / 0.42);
    float sourceNeed = max(laneDeficit * 0.92, populationDeficit * 0.58);
    sourceNeed *= lerp(0.68, 1.0, complexityNeed);
    sourceNeed *= lerp(0.18, 1.0, interiorPenalty);
    return sourceNeed;
}

void ApplyFoamDistributedSupply(
    int x,
    int y,
    float2 fieldUV,
    float globalDistance,
    float lateralMetres,
    float sourceNeed,
    float broadAmount,
    float enclosedSupport,
    float coverage,
    bool insidePhysicalDomain,
    float agitation,
    float downstreamSign,
    FoamMotionSample motion,
    inout float amount,
    inout float freshness,
    inout float integrity,
    inout float phase)
{
    float targetCoverage = saturate(_FoamTargetCoverage);
    float sourceNoise = FoamFbm(
        float2(
            globalDistance * 0.33 + _FoamTime * lerp(0.018, 0.062, agitation),
            lateralMetres * 0.61 - _FoamTime * lerp(0.015, 0.052, agitation)) +
        _FoamSeed * 0.007);
    float sourcePulse = smoothstep(0.34, 0.70, sourceNoise);
    float laneBirth = saturate(
        smoothstep(0.15, 0.56, motion.lane) * lerp(0.78, 1.0, sourcePulse) +
        motion.junction * 0.26);

    float enclosedHole =
        (1.0 - smoothstep(0.02, 0.12, amount)) *
        smoothstep(4.5, 7.0, enclosedSupport);
    float localCapacity = saturate(1.0 - broadAmount / lerp(0.13, 0.22, targetCoverage));
    localCapacity *= 1.0 - enclosedHole * 0.99;
    localCapacity *= 1.0 - smoothstep(0.24, 0.50, amount);

    // Prevent distributed supply from appearing immediately upstream of an
    // existing branch and masquerading as reverse-flow merging.
    float downstreamSupport = SampleAdvectedBilinear(
        float2(x, y) + float2(downstreamSign * 2.6, 0.0)).x;
    float upstreamMergeSuppression =
        (1.0 - smoothstep(0.02, 0.10, amount)) *
        smoothstep(_FoamVisibleThreshold * 0.70, _FoamVisibleThreshold * 1.25, downstreamSupport);
    localCapacity *= 1.0 - upstreamMergeSuppression * 0.96;

    float sourceRate =
        max(0.0, _FoamSupplyRate) * sourceNeed * laneBirth *
        localCapacity * coverage * (insidePhysicalDomain ? 1.0 : 0.0);
    float bornAmount = sourceRate * _FoamDeltaTime;
    if (bornAmount > 0.000001)
    {
        float previousAmount = amount;
        amount = saturate(amount + bornAmount * (1.0 - amount));
        float added = max(0.0, amount - previousAmount);
        float sourceFreshness = lerp(0.50, 0.78, sourcePulse);
        float sourceIntegrity = lerp(0.62, 0.86, sourcePulse);
        if (amount > 0.0001)
        {
            freshness = saturate((freshness * previousAmount + sourceFreshness * added) / amount);
            integrity = saturate((integrity * previousAmount + sourceIntegrity * added) / amount);
        }
        float sourcePhase = frac(
            sourceNoise * 0.73 +
            FoamHash11(floor(globalDistance * 0.15) * 31.7 + floor(fieldUV.y * 17.0) * 17.3 + _FoamSeed * 0.019) * 0.61);
        phase = previousAmount > 0.002
            ? MixPhaseShortest(phase, sourcePhase, saturate(added / max(0.001, amount)))
            : sourcePhase;
    }
}
