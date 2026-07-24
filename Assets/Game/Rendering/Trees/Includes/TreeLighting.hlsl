#ifndef PS3D_TREE_LIGHTING_INCLUDED
#define PS3D_TREE_LIGHTING_INCLUDED

InputData BuildTreeInputData(
    float4 positionCS,
    float3 positionWS,
    float3 normalWS,
    float fogFactor)
{
    InputData inputData = (InputData)0;
    inputData.positionWS = positionWS;
    inputData.positionCS = positionCS;
    inputData.normalWS = normalize(normalWS);
    inputData.viewDirectionWS =
        GetWorldSpaceNormalizeViewDir(positionWS);
    inputData.shadowCoord =
        TransformWorldToShadowCoord(positionWS);
    inputData.fogCoord = fogFactor;
    inputData.vertexLighting = half3(0.0h, 0.0h, 0.0h);
    inputData.bakedGI = SampleSH(inputData.normalWS);
    inputData.normalizedScreenSpaceUV =
        GetNormalizedScreenSpaceUV(positionCS);
    inputData.shadowMask = half4(1.0h, 1.0h, 1.0h, 1.0h);
    return inputData;
}

half4 ShadeTreeSurface(
    InputData inputData,
    half3 albedo,
    half3 specular,
    half smoothness,
    half alpha)
{
    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo = albedo;
    surfaceData.specular = specular;
    surfaceData.metallic = 0.0h;
    surfaceData.smoothness = saturate(smoothness);
    surfaceData.normalTS = half3(0.0h, 0.0h, 1.0h);
    surfaceData.emission = half3(0.0h, 0.0h, 0.0h);
    surfaceData.occlusion = 1.0h;
    surfaceData.alpha = alpha;
    surfaceData.clearCoatMask = 0.0h;
    surfaceData.clearCoatSmoothness = 0.0h;
    return UniversalFragmentPBR(inputData, surfaceData);
}

#endif
