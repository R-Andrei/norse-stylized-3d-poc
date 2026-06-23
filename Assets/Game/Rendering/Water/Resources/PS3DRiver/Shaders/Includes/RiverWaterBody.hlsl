#ifndef PS3D_RIVER_WATER_BODY_INCLUDED
#define PS3D_RIVER_WATER_BODY_INCLUDED

struct RiverWaterBodyResult
{
    float3 colour;
    float3 liquidColour;
    float3 frozenColour;
    float coverage;
    float volumeCoverage;
    float surfaceCoverage;
    float freezeAmount;
};

float RiverWaterResolveSurfaceCoverage(
    float presence,
    float viewFacing,
    float baseResponse,
    float grazingResponse)
{
    float grazing = pow(saturate(1.0 - viewFacing), 2.0);
    float response = lerp(
        max(0.0, baseResponse),
        max(0.0, grazingResponse),
        grazing);

    return saturate(
        1.0 - exp2(-saturate(presence) * response));
}

RiverWaterBodyResult RiverWaterComposeBody(
    float3 sceneColour,
    float3 shallowColour,
    float3 deepColour,
    RiverWaterDepthData depthData,
    float waterTintStrength,
    float surfacePresence,
    float viewFacing,
    float freezeAmount,
    float3 iceColour,
    float iceTransmission,
    float iceThickness,
    float iceCloudiness,
    float iceSurfacePresence,
    float iceScattering,
    float3 bodyLighting)
{
    RiverWaterBodyResult result;

    float tintStrength = saturate(waterTintStrength);
    float liquidPresence = saturate(surfacePresence);
    float freeze = saturate(freezeAmount);

    float3 waterTint = lerp(
        shallowColour,
        deepColour,
        depthData.depthBlend);

    float depthCoverage = 1.0 - depthData.transmission;
    float liquidVolumeCoverage = saturate(
        depthCoverage * tintStrength * 1.35);

    float3 litWaterTint =
        max(waterTint, 0.0) *
        max(bodyLighting, 0.0);

    float3 liquidVolumeColour = lerp(
        sceneColour,
        litWaterTint,
        liquidVolumeCoverage);

    float liquidSurfaceCoverage =
        RiverWaterResolveSurfaceCoverage(
            liquidPresence,
            viewFacing,
            0.55,
            1.75);

    float3 liquidSurfaceTint = lerp(
        shallowColour,
        waterTint,
        depthData.depthBlend * 0.20);

    liquidSurfaceTint =
        max(liquidSurfaceTint * 1.04 + 0.015, 0.0) *
        max(bodyLighting, 0.0);

    result.liquidColour = lerp(
        liquidVolumeColour,
        liquidSurfaceTint,
        liquidSurfaceCoverage);

    float thickness = saturate(iceThickness);
    float cloudiness = saturate(iceCloudiness);
    float transmission = saturate(iceTransmission);

    // Ice transmission is intentionally separate from water depth. Thickness
    // and cloudiness describe the frozen sheet itself, while a restrained
    // depth term keeps very deep channels from looking unnaturally clear.
    float frozenSceneTransmission =
        transmission *
        lerp(1.0, 0.18, thickness) *
        lerp(1.0, 0.20, cloudiness) *
        lerp(1.0, depthData.transmission, 0.15);

    float frozenVolumeCoverage =
        saturate(1.0 - frozenSceneTransmission);

    float3 cloudyIceColour = lerp(
        iceColour,
        1.0.xxx,
        cloudiness * 0.35);

    float scatterBoost = lerp(
        1.0,
        1.45,
        saturate(iceScattering) * cloudiness);

    float3 litIceColour =
        max(cloudyIceColour, 0.0) *
        max(bodyLighting, 0.0) *
        scatterBoost;

    float3 frozenVolumeColour = lerp(
        sceneColour,
        litIceColour,
        frozenVolumeCoverage);

    float frozenSurfaceCoverage =
        RiverWaterResolveSurfaceCoverage(
            iceSurfacePresence,
            viewFacing,
            1.10,
            1.70);

    float3 frozenSurfaceTint = lerp(
        iceColour,
        1.0.xxx,
        cloudiness * 0.22);

    frozenSurfaceTint =
        max(frozenSurfaceTint, 0.0) *
        max(bodyLighting, 0.0) *
        lerp(
            1.0,
            1.25,
            saturate(iceScattering));

    result.frozenColour = lerp(
        frozenVolumeColour,
        frozenSurfaceTint,
        frozenSurfaceCoverage);

    result.colour = lerp(
        result.liquidColour,
        result.frozenColour,
        freeze);

    result.volumeCoverage = lerp(
        liquidVolumeCoverage,
        frozenVolumeCoverage,
        freeze);

    result.surfaceCoverage = lerp(
        liquidSurfaceCoverage,
        frozenSurfaceCoverage,
        freeze);

    result.coverage = 1.0 -
        (1.0 - result.volumeCoverage) *
        (1.0 - result.surfaceCoverage);

    result.freezeAmount = freeze;

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
