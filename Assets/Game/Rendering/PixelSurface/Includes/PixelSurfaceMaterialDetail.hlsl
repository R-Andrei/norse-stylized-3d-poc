#ifndef PS3D_PIXELSURFACEMATERIALDETAIL_HLSL
#define PS3D_PIXELSURFACEMATERIALDETAIL_HLSL

struct PS3D_StylizedSurfaceDetail
{
    float2 slope;
    float cavity;
    float cavityCore;
    float formSigned;
    float finishSigned;
    float textureFormStrength;
    float sceneLightingResponse;
    float roughness;
    float roughnessVariationStrength;
    float textureFormPayload;
    float featureTextureFormPayload;
    float substrateFormSigned;
    float substrateRoughness;
    float featureMask;
};

PS3D_StylizedSurfaceDetail PS3D_ZeroStylizedSurfaceDetail()
{
    PS3D_StylizedSurfaceDetail result;
    result.slope = float2(0.0, 0.0);
    result.cavity = 0.0;
    result.cavityCore = 0.0;
    result.formSigned = 0.0;
    result.finishSigned = 0.0;
    result.textureFormStrength = 0.0;
    result.sceneLightingResponse = 1.0;
    result.roughness = 0.5;
    result.roughnessVariationStrength = 0.0;
    result.textureFormPayload = 0.0;
    result.featureTextureFormPayload = 0.0;
    result.substrateFormSigned = 0.0;
    result.substrateRoughness = 0.5;
    result.featureMask = 0.0;
    return result;
}

PS3D_StylizedSurfaceDetail PS3D_DecodeStylizedSurfaceDetail(
    float4 packedSample,
    float4 detailA,
    float4 detailB,
    float4 detailC)
{
    PS3D_StylizedSurfaceDetail result =
        PS3D_ZeroStylizedSurfaceDetail();
    result.slope =
        (packedSample.rg * 2.0 - 1.0) * max(0.0, detailA.w);

    float cavityBias = saturate(detailB.y);
    float cavityRaw = saturate(
        (packedSample.b - cavityBias) /
        max(0.001, 1.0 - cavityBias));
    float cavityStrength = max(0.0, detailB.x);

    // One packed cavity channel carries two visual bands. The broader shoulder
    // produces restrained contact shadow around material elements, while the
    // upper channel range produces a narrower deep core.
    result.cavity =
        smoothstep(0.0, 0.82, cavityRaw) * cavityStrength;
    result.cavityCore =
        smoothstep(0.66, 0.98, cavityRaw) * cavityStrength;

    result.textureFormPayload = step(0.5, detailC.z);
    result.featureTextureFormPayload = step(1.5, detailC.z);
    float packedVariation = packedSample.a * 2.0 - 1.0;
    result.formSigned =
        packedVariation * max(0.0, detailB.z) *
        (1.0 - result.textureFormPayload);
    result.finishSigned =
        packedVariation * max(0.0, detailC.x) *
        (1.0 - result.textureFormPayload);
    result.roughness = saturate(packedSample.a);
    result.roughnessVariationStrength =
        saturate(detailC.w) * result.textureFormPayload;
    return result;
}

PS3D_StylizedSurfaceDetail PS3D_AssignStylizedSurfaceTextureForm(
    PS3D_StylizedSurfaceDetail detail,
    float4 formSample,
    float4 formA)
{
    float enabled = step(0.5, formA.x);
    float strength = saturate(formA.z) * enabled;
    float normalizedForm = saturate(formSample.r);
    detail.formSigned =
        (normalizedForm * 2.0 - 1.0) * strength;
    detail.substrateFormSigned =
        (saturate(formSample.g) * 2.0 - 1.0) * strength;
    detail.substrateRoughness = saturate(formSample.b);
    detail.featureMask =
        saturate(formSample.a) * detail.featureTextureFormPayload;
    detail.textureFormStrength = strength;
    detail.sceneLightingResponse = saturate(formA.w);
    return detail;
}

PS3D_StylizedSurfaceDetail PS3D_ApplyStylizedSurfaceFeatureRetention(
    PS3D_StylizedSurfaceDetail detail,
    float edgeRetention)
{
    float retention = saturate(edgeRetention);
    detail.formSigned = lerp(
        detail.substrateFormSigned,
        detail.formSigned,
        retention);
    detail.slope *= retention;
    detail.cavity *= retention;
    detail.cavityCore *= retention;
    detail.roughness = lerp(
        detail.substrateRoughness,
        detail.roughness,
        retention);
    return detail;
}

float3 PS3D_ApplyWorldXZStylizedSurfaceNormal(
    float3 baseNormalWS,
    float2 combinedSlope)
{
    float3 safeNormal = normalize(baseNormalWS);
    float safeY = max(0.15, abs(safeNormal.y));
    float3 tangentX = normalize(
        float3(1.0, -safeNormal.x / safeY, 0.0));
    float3 tangentZ = normalize(
        float3(0.0, -safeNormal.z / safeY, 1.0));

    return normalize(
        safeNormal +
        tangentX * combinedSlope.x +
        tangentZ * combinedSlope.y);
}

half3 PS3D_ResolveStylizedSurfacePalette(
    half3 baseColor,
    half3 darkColor,
    half3 lightColor,
    half3 cavityColor,
    float signedVariation,
    float formHighlightStrength,
    PS3D_StylizedSurfaceDetail detail)
{
    float combinedVariation = clamp(
        signedVariation + detail.formSigned,
        -1.0,
        1.0);
    float positiveVariation = saturate(combinedVariation);
    positiveVariation = saturate(
        positiveVariation *
        (1.0 + max(0.0, formHighlightStrength)));

    half3 palette = combinedVariation < 0.0
        ? lerp(
            baseColor,
            darkColor,
            (half)(-combinedVariation))
        : lerp(
            baseColor,
            lightColor,
            (half)positiveVariation);

    half3 contactShadowPalette = lerp(
        palette,
        darkColor,
        (half)saturate(detail.cavity * 0.42));

    return lerp(
        contactShadowPalette,
        cavityColor,
        (half)saturate(detail.cavityCore));
}

half PS3D_ResolveStylizedSurfaceDrySmoothness(
    half profileSmoothness,
    PS3D_StylizedSurfaceDetail detail)
{
    half roughnessVariation =
        (0.5h - (half)detail.roughness) *
        0.5h *
        (half)detail.roughnessVariationStrength;
    return saturate(
        profileSmoothness +
        (half)detail.finishSigned +
        roughnessVariation -
        (half)detail.cavity * 0.08h);
}

#endif // PS3D_PIXELSURFACEMATERIALDETAIL_HLSL
