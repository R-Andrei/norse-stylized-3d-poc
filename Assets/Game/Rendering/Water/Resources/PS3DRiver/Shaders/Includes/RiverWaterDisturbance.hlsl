#ifndef PS3D_RIVER_WATER_DISTURBANCE_INCLUDED
#define PS3D_RIVER_WATER_DISTURBANCE_INCLUDED

struct RiverWaterDisturbanceResult
{
    float height;
    float velocity;
    float downstreamGradient;
    float lateralGradient;
    float intensity;
    float bankMask;
    float2 fieldUV;
};

float RiverWaterResolveDisturbanceBankMask(
    float lateralMetres,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float shoreInteraction)
{
    float lateral = abs(lateralMetres);
    float visible = max(0.001, visibleHalfWidth);
    float surface = max(visible + 0.001, surfaceHalfWidth);
    float retainedAtShore = saturate(shoreInteraction);

    if (lateral <= visible)
    {
        float interiorBand = max(0.12, visible * 0.20);
        float interiorDistance = visible - lateral;
        return lerp(
            retainedAtShore,
            1.0,
            smoothstep(0.0, interiorBand, interiorDistance));
    }

    float hiddenWidth = max(0.001, surface - visible);
    float hiddenRemaining = saturate((surface - lateral) / hiddenWidth);
    return retainedAtShore * smoothstep(0.0, 1.0, hiddenRemaining);
}

RiverWaterDisturbanceResult RiverWaterEvaluateDisturbance(
    TEXTURE2D_PARAM(previousField, previousSampler),
    TEXTURE2D_PARAM(currentField, currentSampler),
    float enabled,
    float globalDistance,
    float lateralMetres,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float globalStart,
    float fieldLength,
    float interpolation,
    float geometryStrength,
    float shoreInteraction,
    float maximumHeight,
    float freezeAmount)
{
    RiverWaterDisturbanceResult result;
    result.height = 0.0;
    result.velocity = 0.0;
    result.downstreamGradient = 0.0;
    result.lateralGradient = 0.0;
    result.intensity = 0.0;
    result.bankMask = 0.0;
    result.fieldUV = 0.0;

    if (enabled < 0.5 || fieldLength <= 0.0001)
    {
        return result;
    }

    float signedAcross =
        lateralMetres / max(0.001, surfaceHalfWidth);
    float2 uv = float2(
        saturate((globalDistance - globalStart) / fieldLength),
        saturate(signedAcross * 0.5 + 0.5));

    float4 previousState = SAMPLE_TEXTURE2D_LOD(
        previousField,
        previousSampler,
        uv,
        0.0);
    float4 currentState = SAMPLE_TEXTURE2D_LOD(
        currentField,
        currentSampler,
        uv,
        0.0);
    float4 state = lerp(
        previousState,
        currentState,
        saturate(interpolation));

    float liquidFactor = 1.0 - saturate(freezeAmount);
    float bankMask = RiverWaterResolveDisturbanceBankMask(
        lateralMetres,
        visibleHalfWidth,
        surfaceHalfWidth,
        shoreInteraction);

    result.height =
        clamp(state.r, -maximumHeight, maximumHeight) *
        max(0.0, geometryStrength) *
        bankMask *
        liquidFactor;
    result.velocity = state.g * bankMask * liquidFactor;
    result.downstreamGradient = state.b * bankMask * liquidFactor;
    result.lateralGradient = state.a * bankMask * liquidFactor;
    result.intensity = saturate(
        abs(state.r) / max(0.001, maximumHeight) * 0.45 +
        abs(state.g) * 0.10 +
        length(state.ba) * 0.35);
    result.bankMask = bankMask;
    result.fieldUV = uv;
    return result;
}

float3 RiverWaterApplyDisturbanceNormal(
    float3 baseNormalWS,
    float3 tangentWS,
    float3 sideWS,
    float downstreamGradient,
    float lateralGradient,
    float normalStrength)
{
    return normalize(
        baseNormalWS -
        tangentWS * downstreamGradient * max(0.0, normalStrength) -
        sideWS * lateralGradient * max(0.0, normalStrength));
}

#endif
