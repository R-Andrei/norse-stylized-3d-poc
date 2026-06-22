#ifndef PS3D_RIVER_WATER_BODY_INCLUDED
#define PS3D_RIVER_WATER_BODY_INCLUDED

struct RiverWaterBodyResult
{
    float3 colour;
    float coverage;
    float volumeCoverage;
    float surfaceCoverage;
};

RiverWaterBodyResult RiverWaterComposeBody(
    float3 sceneColour,
    float3 shallowColour,
    float3 deepColour,
    RiverWaterDepthData depthData,
    float waterTintStrength,
    float surfacePresence,
    float viewFacing)
{
    RiverWaterBodyResult result;

    float tintStrength = saturate(waterTintStrength);
    float presence = saturate(surfacePresence);

    float3 waterTint = lerp(
        shallowColour,
        deepColour,
        depthData.depthBlend);

    // The volume term only describes depth-dependent colour absorption.
    // Keeping it independent from the surface term allows clear water to
    // retain a readable air-water boundary without becoming artificially
    // murky.
    float depthCoverage = 1.0 - depthData.transmission;
    result.volumeCoverage = saturate(
        depthCoverage * tintStrength * 1.35);

    float3 volumeColour = lerp(
        sceneColour,
        waterTint,
        result.volumeCoverage);

    // A mostly constant surface layer is essential for an elevated
    // isometric-style camera. A restrained grazing-angle boost helps low
    // views without making Fresnel the primary source of water readability.
    float grazing = pow(saturate(1.0 - viewFacing), 2.0);
    float surfaceResponse = lerp(0.55, 1.75, grazing);
    result.surfaceCoverage = saturate(
        1.0 - exp2(-presence * surfaceResponse));

    float3 surfaceTint = lerp(
        shallowColour,
        waterTint,
        depthData.depthBlend * 0.20);
    surfaceTint = saturate(surfaceTint * 1.04 + 0.015);

    result.colour = lerp(
        volumeColour,
        surfaceTint,
        result.surfaceCoverage);

    result.coverage = 1.0 -
        (1.0 - result.volumeCoverage) *
        (1.0 - result.surfaceCoverage);

    return result;
}

float3 RiverWaterApplyReservedIntegration(
    float3 bodyColour,
    RiverWaterIntegrationInputs integration)
{
    float3 result = lerp(
        bodyColour,
        integration.reflectionColour,
        saturate(integration.reflectionWeight));
    result = lerp(result, 1.0.xxx, saturate(integration.foamMask));
    return result;
}

#endif
