#ifndef PS3D_RIVER_WATER_FOAM_INCLUDED
#define PS3D_RIVER_WATER_FOAM_INCLUDED


float RiverFoamHash(float value)
{
    return frac(sin(value * 127.1 + 311.7) * 43758.5453123);
}

float RiverFoamValueNoise(float2 position, float seed)
{
    float2 cell = floor(position);
    float2 local = frac(position);
    local = local * local * (3.0 - 2.0 * local);

    float a = RiverFoamHash(dot(cell, float2(17.17, 61.73)) + seed);
    float b = RiverFoamHash(dot(cell + float2(1.0, 0.0), float2(17.17, 61.73)) + seed);
    float c = RiverFoamHash(dot(cell + float2(0.0, 1.0), float2(17.17, 61.73)) + seed);
    float d = RiverFoamHash(dot(cell + float2(1.0, 1.0), float2(17.17, 61.73)) + seed);
    return lerp(lerp(a, b, local.x), lerp(c, d, local.x), local.y);
}

float RiverFoamFbm(float2 position, float seed)
{
    float value = 0.0;
    float amplitude = 0.56;
    float2 samplePosition = position;

    [unroll]
    for (int octave = 0; octave < 3; octave++)
    {
        value += RiverFoamValueNoise(samplePosition, seed) * amplitude;
        samplePosition = mul(
            float2x2(1.31, -1.07, 1.07, 1.31),
            samplePosition) + float2(11.7, 23.9);
        amplitude *= 0.48;
    }

    return saturate(value);
}

struct RiverWaterFoamResult
{
    float amount;
    float freshness;
    float integrity;
    float phase;
    float mask;
    float2 fieldUV;
};

RiverWaterFoamResult RiverWaterEvaluateFoam(
    TEXTURE2D_PARAM(previousFoam, previousFoamSampler),
    TEXTURE2D_PARAM(currentFoam, currentFoamSampler),
    float enabled,
    float globalDistance,
    float lateralMetres,
    float surfaceHalfWidth,
    float globalStart,
    float fieldLength,
    float interpolation,
    float strength,
    float coverage,
    float sharpness,
    float detailScale,
    float detailStrength,
    float seed,
    float freezeAmount)
{
    RiverWaterFoamResult result;
    result.amount = 0.0;
    result.freshness = 0.0;
    result.integrity = 0.0;
    result.phase = 0.0;
    result.mask = 0.0;
    result.fieldUV = 0.0;

    if (enabled < 0.5 || fieldLength <= 0.0001)
    {
        return result;
    }

    float2 uv = float2(
        saturate((globalDistance - globalStart) / fieldLength),
        saturate(lateralMetres / max(0.001, surfaceHalfWidth) * 0.5 + 0.5));
    float4 previousState = SAMPLE_TEXTURE2D_LOD(
        previousFoam,
        previousFoamSampler,
        uv,
        0.0);
    float4 currentState = SAMPLE_TEXTURE2D_LOD(
        currentFoam,
        currentFoamSampler,
        uv,
        0.0);
    float4 state = lerp(
        previousState,
        currentState,
        saturate(interpolation));

    float amount = saturate(state.x);
    float freshness = saturate(state.y);
    float integrity = saturate(state.z);
    float phase = saturate(state.w);
    float resolvedStrength = max(0.0, strength);
    float resolvedCoverage = saturate(coverage);
    float resolvedSharpness = saturate(sharpness);

    // Macro and micro topology come from the persistent simulation. Rendering
    // is deliberately temporally stable: it may roughen an existing contour,
    // but it may not animate individual pixels across the visibility threshold.
    float structuralPresence = lerp(0.82, 1.08, integrity);
    float density = saturate(amount * resolvedStrength * structuralPresence);
    float threshold = lerp(0.31, 0.105, resolvedCoverage);
    threshold += (1.0 - integrity) * 0.020;

    float signedDensity = density - threshold;
    float detailFrequency = lerp(
        5.5,
        12.5,
        saturate(1.0 - detailScale));
    float2 detailPosition = float2(
        globalDistance * detailFrequency,
        lateralMetres * detailFrequency * 1.19) +
        float2(phase * 13.7, -phase * 9.1);
    float stableDetail = RiverFoamFbm(
        detailPosition,
        seed * 0.013 + phase * 41.0);
    float edgeBand = 1.0 - smoothstep(
        0.015,
        0.085,
        abs(signedDensity));
    float weakness = saturate(1.0 - integrity);
    float irregularOffset =
        (stableDetail - 0.5) * 0.008;
    signedDensity += irregularOffset *
        saturate(detailStrength) *
        edgeBand *
        lerp(0.55, 0.90, weakness);

    float derivativeWidth = max(fwidth(signedDensity), 0.0001);
    float edgeWidth = derivativeWidth * lerp(2.00, 0.28, resolvedSharpness);
    float mask = smoothstep(-edgeWidth, edgeWidth, signedDensity);
    mask *= 1.0 - saturate(freezeAmount);

    result.amount = amount;
    result.freshness = freshness;
    result.integrity = integrity;
    result.phase = phase;
    result.mask = saturate(mask);
    result.fieldUV = uv;
    return result;
}

float3 RiverWaterResolveFoamColour(
    float3 foamColour,
    float3 lighting,
    float minimumNightVisibility,
    float freshness,
    float integrity)
{
    float3 lit = max(
        float3(
            minimumNightVisibility,
            minimumNightVisibility,
            minimumNightVisibility),
        lighting);
    float freshnessLift = lerp(0.86, 1.07, saturate(freshness));
    float integrityLift = lerp(0.92, 1.03, saturate(integrity));
    return max(0.0, foamColour * lit * freshnessLift * integrityLift);
}

float3 RiverWaterFoamPhaseDebugColour(float phase)
{
    float angle = saturate(phase) * 6.2831853;
    return saturate(
        0.5 + 0.5 * cos(angle + float3(0.0, 4.1887902, 2.0943951)));
}

#endif
