#ifndef PS3D_RIVER_WATER_COMMON_INCLUDED
#define PS3D_RIVER_WATER_COMMON_INCLUDED

struct RiverWaterSurfaceInputs
{
    float3 positionWS;
    float3 baseNormalWS;
    float localDistance;
    float globalDistance;
    float lateralMetres;
};

struct RiverWaterIntegrationInputs
{
    float3 surfaceNormalWS;
    float2 refractionOffset;
    float foamMask;
    float3 reflectionColour;
    float reflectionWeight;
};

RiverWaterIntegrationInputs RiverWaterCreateEmptyIntegration(
    float3 baseNormalWS)
{
    RiverWaterIntegrationInputs inputs;
    inputs.surfaceNormalWS = normalize(baseNormalWS);
    inputs.refractionOffset = 0.0;
    inputs.foamMask = 0.0;
    inputs.reflectionColour = 0.0;
    inputs.reflectionWeight = 0.0;
    return inputs;
}

#endif
