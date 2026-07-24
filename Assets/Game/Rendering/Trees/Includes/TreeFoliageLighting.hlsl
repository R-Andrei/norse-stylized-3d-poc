#ifndef PS3D_TREE_FOLIAGE_LIGHTING_INCLUDED
#define PS3D_TREE_FOLIAGE_LIGHTING_INCLUDED

struct TreeFoliageLightingResult
{
    float3 ambient;
    float3 mainDirect;
    float3 additionalDirect;
    float3 combined;
    float directResponse;
    float rawShadowAttenuation;
    float resolvedShadowAttenuation;
    float cookieAttenuation;
};

float TreeFoliageLuminance(float3 colour)
{
    return dot(colour, float3(0.2126, 0.7152, 0.0722));
}

float TreeFoliageWrappedDiffuse(
    float3 normalWS,
    float3 lightDirectionWS,
    float diffuseWrap)
{
    float wrap = max(0.0, diffuseWrap);
    float twoSidedNdotL = abs(dot(
        normalize(normalWS),
        normalize(lightDirectionWS)));
    return saturate((twoSidedNdotL + wrap) / (1.0 + wrap));
}

float TreeFoliageResolveShadowAttenuation(
    float rawShadowAttenuation,
    float receiveStrength,
    float shadowFloor)
{
    float flooredShadow = max(
        saturate(rawShadowAttenuation),
        saturate(shadowFloor));
    return lerp(
        1.0,
        flooredShadow,
        saturate(receiveStrength));
}

float TreeFoliageEstimateCookieAttenuation(Light mainLight)
{
    float unmodulatedLuminance = max(
        0.0001,
        TreeFoliageLuminance(max(_MainLightColor.rgb, 0.0)));
    float resolvedLuminance = TreeFoliageLuminance(
        max(mainLight.color, 0.0));
    return saturate(resolvedLuminance / unmodulatedLuminance);
}

float3 TreeFoliageEvaluateSpecular(
    float3 normalWS,
    float3 viewDirectionWS,
    Light light,
    float shadowAttenuation,
    float3 specularColour,
    float smoothness)
{
    float3 halfDirection = SafeNormalize(
        normalize(light.direction) + normalize(viewDirectionWS));
    float exponent = lerp(8.0, 64.0, saturate(smoothness));
    float highlight = pow(
        saturate(abs(dot(normalize(normalWS), halfDirection))),
        exponent);
    return max(light.color, 0.0) *
        max(0.0, light.distanceAttenuation) *
        shadowAttenuation *
        highlight *
        max(specularColour, 0.0) *
        0.25;
}

TreeFoliageLightingResult TreeEvaluateFoliageLighting(
    InputData inputData,
    float3 normalWS,
    float diffuseWrap,
    float orientationContrast,
    float shadowReceiveStrength,
    float shadowFloor,
    float3 specularColour,
    float smoothness)
{
    TreeFoliageLightingResult result;
    float3 resolvedNormalWS = normalize(normalWS);
    float3 viewDirectionWS = normalize(inputData.viewDirectionWS);

    result.ambient = max(SampleSH(resolvedNormalWS), 0.0);
    result.mainDirect = 0.0;
    result.additionalDirect = 0.0;
    result.directResponse = 0.0;
    result.rawShadowAttenuation = 1.0;
    result.resolvedShadowAttenuation = 1.0;
    result.cookieAttenuation = 1.0;

    Light mainLight = GetMainLight(
        inputData.shadowCoord,
        inputData.positionWS,
        inputData.shadowMask);
    float mainDiffuse = TreeFoliageWrappedDiffuse(
        resolvedNormalWS,
        mainLight.direction,
        diffuseWrap);
    float shapedMainDiffuse = lerp(
        1.0,
        mainDiffuse,
        saturate(orientationContrast));
    result.rawShadowAttenuation = saturate(
        mainLight.shadowAttenuation);
    result.resolvedShadowAttenuation =
        TreeFoliageResolveShadowAttenuation(
            result.rawShadowAttenuation,
            shadowReceiveStrength,
            shadowFloor);
    result.cookieAttenuation =
        TreeFoliageEstimateCookieAttenuation(mainLight);
    result.directResponse = shapedMainDiffuse;
    result.mainDirect = max(mainLight.color, 0.0) *
        max(0.0, mainLight.distanceAttenuation) *
        result.resolvedShadowAttenuation *
        shapedMainDiffuse;
    result.mainDirect += TreeFoliageEvaluateSpecular(
        resolvedNormalWS,
        viewDirectionWS,
        mainLight,
        result.resolvedShadowAttenuation,
        specularColour,
        smoothness);

    #if defined(_ADDITIONAL_LIGHTS)
        #if USE_CLUSTER_LIGHT_LOOP
            UNITY_LOOP for (
                uint lightIndex = 0;
                lightIndex < min(
                    URP_FP_DIRECTIONAL_LIGHTS_COUNT,
                    MAX_VISIBLE_LIGHTS);
                lightIndex++)
            {
                Light additionalLight = GetAdditionalLight(
                    lightIndex,
                    inputData.positionWS,
                    inputData.shadowMask);
                float additionalDiffuse = TreeFoliageWrappedDiffuse(
                    resolvedNormalWS,
                    additionalLight.direction,
                    diffuseWrap);
                float shapedAdditionalDiffuse = lerp(
                    1.0,
                    additionalDiffuse,
                    saturate(orientationContrast));
                float additionalShadow =
                    TreeFoliageResolveShadowAttenuation(
                        additionalLight.shadowAttenuation,
                        shadowReceiveStrength,
                        shadowFloor);
                result.additionalDirect += max(
                    additionalLight.color,
                    0.0) *
                    max(0.0, additionalLight.distanceAttenuation) *
                    additionalShadow *
                    shapedAdditionalDiffuse;
                result.additionalDirect +=
                    TreeFoliageEvaluateSpecular(
                        resolvedNormalWS,
                        viewDirectionWS,
                        additionalLight,
                        additionalShadow,
                        specularColour,
                        smoothness);
            }
        #endif

        uint pixelLightCount = GetAdditionalLightsCount();
        LIGHT_LOOP_BEGIN(pixelLightCount)
            Light additionalLight = GetAdditionalLight(
                lightIndex,
                inputData.positionWS,
                inputData.shadowMask);
            float additionalDiffuse = TreeFoliageWrappedDiffuse(
                resolvedNormalWS,
                additionalLight.direction,
                diffuseWrap);
            float shapedAdditionalDiffuse = lerp(
                1.0,
                additionalDiffuse,
                saturate(orientationContrast));
            float additionalShadow =
                TreeFoliageResolveShadowAttenuation(
                    additionalLight.shadowAttenuation,
                    shadowReceiveStrength,
                    shadowFloor);
            result.additionalDirect += max(
                additionalLight.color,
                0.0) *
                max(0.0, additionalLight.distanceAttenuation) *
                additionalShadow *
                shapedAdditionalDiffuse;
            result.additionalDirect += TreeFoliageEvaluateSpecular(
                resolvedNormalWS,
                viewDirectionWS,
                additionalLight,
                additionalShadow,
                specularColour,
                smoothness);
        LIGHT_LOOP_END
    #endif

    result.combined =
        result.ambient +
        result.mainDirect +
        result.additionalDirect;
    return result;
}

float TreeResolveFoliageCanopyFactor(
    float heightMask,
    float strength,
    float power)
{
    float shapedHeight = pow(
        saturate(heightMask),
        max(0.05, power));
    return lerp(
        1.0 - saturate(strength),
        1.0,
        shapedHeight);
}

float TreeResolveFoliageOrientationFactor(
    float3 rawNormalWS,
    float strength)
{
    float orientation = saturate(
        normalize(rawNormalWS).y * 0.5 + 0.5);
    float resolvedStrength = saturate(strength);
    return lerp(
        1.0 - 0.35 * resolvedStrength,
        1.0 + 0.15 * resolvedStrength,
        orientation);
}

float TreeResolveFoliageUndersideFactor(
    float faceSign,
    float undersideDarkening)
{
    return faceSign >= 0.0
        ? 1.0
        : 1.0 - saturate(undersideDarkening);
}

float TreeResolveFoliageClusterFactor(
    float clusterVariation,
    float strength)
{
    float signedVariation = saturate(clusterVariation) * 2.0 - 1.0;
    return max(
        0.0,
        1.0 + signedVariation * saturate(strength));
}

float3 TreeResolveFoliageDebugColour(
    float debugMode,
    float3 sourceAlbedo,
    float alpha,
    float faceSign,
    float heightMask,
    float clusterVariation,
    float orientationFactor,
    TreeFoliageLightingResult lighting)
{
    if (debugMode < 0.5)
    {
        return -1.0;
    }

    if (debugMode < 1.5)
    {
        return saturate(sourceAlbedo);
    }

    if (debugMode < 2.5)
    {
        return alpha.xxx;
    }

    if (debugMode < 3.5)
    {
        return faceSign >= 0.0
            ? float3(0.20, 0.85, 0.35)
            : float3(0.85, 0.25, 0.20);
    }

    if (debugMode < 4.5)
    {
        return saturate(heightMask).xxx;
    }

    if (debugMode < 5.5)
    {
        return saturate(clusterVariation).xxx;
    }

    if (debugMode < 6.5)
    {
        return saturate(orientationFactor * 0.75).xxx;
    }

    if (debugMode < 7.5)
    {
        return lighting.rawShadowAttenuation.xxx;
    }

    if (debugMode < 8.5)
    {
        return lighting.cookieAttenuation.xxx;
    }

    if (debugMode < 9.5)
    {
        return saturate(lighting.directResponse).xxx;
    }

    return saturate(lighting.combined);
}

#endif
