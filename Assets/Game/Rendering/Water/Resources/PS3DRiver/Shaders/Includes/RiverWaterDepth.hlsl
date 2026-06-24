#ifndef PS3D_RIVER_WATER_DEPTH_INCLUDED
#define PS3D_RIVER_WATER_DEPTH_INCLUDED

struct RiverWaterDepthData
{
    float verticalDepth;
    float normalizedDepth;
    float depthBlend;
    float transmission;
    float validSceneDepth;
};

float RiverWaterRawDepthIsValid(float rawDepth)
{
    #if UNITY_REVERSED_Z
        return step(0.00001, rawDepth);
    #else
        return 1.0 - step(0.99999, rawDepth);
    #endif
}

float RiverWaterShapeDepth(float normalizedDepth, float contrast)
{
    float exponentValue = lerp(0.65, 2.5, saturate(contrast));
    return pow(saturate(normalizedDepth), exponentValue);
}

RiverWaterDepthData RiverWaterEvaluateDepth(
    float2 screenUV,
    float3 waterPositionWS,
    float fallbackDepth,
    float depthRange,
    float depthContrast,
    float clarity)
{
    RiverWaterDepthData data;

    float rawSceneDepth = SampleSceneDepth(screenUV);
    float rawDepthValid = RiverWaterRawDepthIsValid(rawSceneDepth);

    float deviceDepth = rawSceneDepth;
    #if !UNITY_REVERSED_Z
        deviceDepth = lerp(
            UNITY_NEAR_CLIP_VALUE,
            1.0,
            rawSceneDepth);
    #endif

    float3 scenePositionWS = ComputeWorldSpacePosition(
        screenUV,
        deviceDepth,
        UNITY_MATRIX_I_VP);

    float sceneEyeDepth = LinearEyeDepth(
        rawSceneDepth,
        _ZBufferParams);
    float waterEyeDepth = -TransformWorldToView(waterPositionWS).z;

    float verticalDepth = waterPositionWS.y - scenePositionWS.y;
    float sceneBehindWater = step(
        waterEyeDepth + 0.001,
        sceneEyeDepth);
    float sceneBelowSurface = step(-0.001, verticalDepth);
    float validSceneDepth =
        rawDepthValid * sceneBehindWater * sceneBelowSurface;

    data.verticalDepth = lerp(
        max(0.01, fallbackDepth),
        max(0.0, verticalDepth),
        validSceneDepth);

    data.normalizedDepth = saturate(
        data.verticalDepth / max(0.01, depthRange));
    data.depthBlend = RiverWaterShapeDepth(
        data.normalizedDepth,
        depthContrast);

    float absorption = lerp(3.5, 0.12, saturate(clarity));
    data.transmission = exp2(
        -data.verticalDepth * absorption);
    data.validSceneDepth = validSceneDepth;

    return data;
}


#endif
