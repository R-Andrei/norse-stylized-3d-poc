#ifndef PS3D_RIVER_WATER_FOAM_INCLUDED
#define PS3D_RIVER_WATER_FOAM_INCLUDED

struct RiverWaterFoamResult
{
    float presence;
    float remainingLife;
    float materialPattern;
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
    float sharpness,
    float freezeAmount)
{
    RiverWaterFoamResult result;
    result.presence = 0.0;
    result.remainingLife = 0.0;
    result.materialPattern = 0.0;
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

    float presence = saturate(state.x);
    float remainingLife = presence > 0.0001
        ? saturate(state.y / presence)
        : 0.0;
    float materialPattern = presence > 0.0001
        ? saturate(state.z / presence)
        : 0.0;

    // Presence is geometric coverage, not emitter strength. The proof
    // renderer extracts its central contour directly and deliberately leaves
    // Material Pattern visually inert until fracture work begins.
    float signedPresence = presence - 0.5;
    float derivativeWidth = max(fwidth(signedPresence), 0.0001);
    float edgeWidth = derivativeWidth * lerp(
        1.75,
        0.35,
        saturate(sharpness));
    float mask = smoothstep(-edgeWidth, edgeWidth, signedPresence);
    mask *= step(0.0001, remainingLife);
    mask *= 1.0 - saturate(freezeAmount);

    result.presence = presence;
    result.remainingLife = remainingLife;
    result.materialPattern = materialPattern;
    result.mask = saturate(mask);
    result.fieldUV = uv;
    return result;
}

float3 RiverWaterResolveFoamColour(
    float3 foamColour,
    float3 lighting,
    float minimumNightVisibility)
{
    float3 lit = max(
        float3(
            minimumNightVisibility,
            minimumNightVisibility,
            minimumNightVisibility),
        lighting);
    return max(0.0, foamColour * lit);
}

float3 RiverWaterFoamPatternDebugColour(float materialPattern)
{
    float value = saturate(materialPattern);
    float3 low = float3(0.08, 0.20, 0.78);
    float3 middle = float3(0.08, 0.92, 0.62);
    float3 high = float3(1.00, 0.82, 0.08);
    return value < 0.5
        ? lerp(low, middle, value * 2.0)
        : lerp(middle, high, (value - 0.5) * 2.0);
}

#endif
