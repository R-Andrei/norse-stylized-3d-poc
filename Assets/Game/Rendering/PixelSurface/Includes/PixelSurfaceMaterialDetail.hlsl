#ifndef PS3D_PIXELSURFACEMATERIALDETAIL_HLSL
#define PS3D_PIXELSURFACEMATERIALDETAIL_HLSL

struct PS3D_StylizedSurfaceDetail
{
    float2 slope;
    float cavity;
    float cavityCore;
    float formSigned;
    float finishSigned;
};

PS3D_StylizedSurfaceDetail PS3D_ZeroStylizedSurfaceDetail()
{
    PS3D_StylizedSurfaceDetail result;
    result.slope = float2(0.0, 0.0);
    result.cavity = 0.0;
    result.cavityCore = 0.0;
    result.formSigned = 0.0;
    result.finishSigned = 0.0;
    return result;
}

PS3D_StylizedSurfaceDetail PS3D_DecodeStylizedSurfaceDetail(
    float4 packedSample,
    float4 detailA,
    float4 detailB,
    float4 detailC)
{
    PS3D_StylizedSurfaceDetail result;
    result.slope =
        (packedSample.rg * 2.0 - 1.0) * max(0.0, detailA.w);

    float cavityBias = saturate(detailB.y);
    float cavityRaw = saturate(
        (packedSample.b - cavityBias) /
        max(0.001, 1.0 - cavityBias));
    float cavityStrength = max(0.0, detailB.x);

    // One packed cavity channel carries two visual bands. The broader shoulder
    // produces restrained contact shadow around material elements, while the
    // upper channel range produces a narrower deep core. This avoids turning
    // every boundary into one uniformly black graphic outline.
    result.cavity =
        smoothstep(0.0, 0.82, cavityRaw) * cavityStrength;
    result.cavityCore =
        smoothstep(0.66, 0.98, cavityRaw) * cavityStrength;

    float authoredVariation = packedSample.a * 2.0 - 1.0;
    result.formSigned = authoredVariation * max(0.0, detailB.z);
    result.finishSigned =
        authoredVariation * max(0.0, detailC.x);
    return result;
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

#endif // PS3D_PIXELSURFACEMATERIALDETAIL_HLSL
