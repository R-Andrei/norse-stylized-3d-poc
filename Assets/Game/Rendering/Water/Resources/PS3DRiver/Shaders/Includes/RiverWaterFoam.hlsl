#ifndef PS3D_RIVER_WATER_FOAM_INCLUDED
#define PS3D_RIVER_WATER_FOAM_INCLUDED

struct RiverWaterFoamResult
{
    float remainingLife;
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
    float renderTravelMetres,
    float sharpness,
    float freezeAmount)
{
    RiverWaterFoamResult result;
    result.remainingLife = 0.0;
    result.mask = 0.0;
    result.fieldUV = 0.0;

    if (enabled < 0.5 || fieldLength <= 0.0001)
    {
        return result;
    }

    float2 fieldUV = float2(
        saturate((globalDistance - globalStart) / fieldLength),
        saturate(lateralMetres / max(0.001, surfaceHalfWidth) * 0.5 + 0.5));
    float2 foamUV = float2(
        saturate(((globalDistance - renderTravelMetres) - globalStart) / fieldLength),
        fieldUV.y);

    float blend = saturate(interpolation);
    float4 currentState = SAMPLE_TEXTURE2D_LOD(
        currentFoam,
        currentFoamSampler,
        foamUV,
        0.0);
    float4 state = currentState;
    if (blend < 0.999)
    {
        float4 previousState = SAMPLE_TEXTURE2D_LOD(
            previousFoam,
            previousFoamSampler,
            foamUV,
            0.0);
        state = lerp(
            previousState,
            currentState,
            blend);
    }

    float presence = saturate(state.x);
    float remainingLife = presence > 0.0001
        ? saturate(state.y / presence)
        : 0.0;
    // Presence is geometric coverage, not emitter strength. Render the
    // transported coverage itself, but suppress the very low-coverage crumbs
    // that finite-volume transport can leave behind after the main footprint
    // has moved on. This is a visual residue floor only: it is far below the
    // useful body coverage of a resolved proof source and does not alter the
    // stored Remaining Life state.
    float contrast = lerp(1.15, 0.82, saturate(sharpness));
    float residueGate = smoothstep(0.025, 0.115, presence);
    float mask = pow(max(0.0, presence), contrast) * residueGate;
    mask *= step(0.0001, remainingLife);
    mask *= 1.0 - saturate(freezeAmount);

    result.remainingLife = remainingLife;
    result.mask = saturate(mask);
    result.fieldUV = fieldUV;
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

#endif
