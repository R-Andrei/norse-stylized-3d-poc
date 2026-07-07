#ifndef PS3D_RIVER_WATER_LIGHTING_INCLUDED
#define PS3D_RIVER_WATER_LIGHTING_INCLUDED

struct RiverWaterLightingResult
{
    float3 ambient;
    float3 sun;
    float3 sunUnshadowed;
    float3 localLights;
    float3 combined;
    float mainShadowAttenuation;
};

float RiverWaterLuminance(float3 colour)
{
    return dot(colour, float3(0.2126, 0.7152, 0.0722));
}

float3 RiverWaterApplyLightColourInfluence(
    float3 lightColour,
    float colourInfluence)
{
    float luminance = RiverWaterLuminance(lightColour);
    return lerp(
        luminance.xxx,
        lightColour,
        saturate(colourInfluence));
}

float RiverWaterWrappedDiffuse(
    float3 normalWS,
    float3 lightDirectionWS,
    float diffuseWrap)
{
    float wrap = max(0.0, diffuseWrap);
    return saturate(
        (dot(normalize(normalWS), normalize(lightDirectionWS)) + wrap) /
        (1.0 + wrap));
}

float3 RiverWaterEvaluateDirectLight(
    float3 normalWS,
    Light light,
    float response,
    float lightColourInfluence,
    float shadowResponse,
    float diffuseWrap)
{
    float diffuse = RiverWaterWrappedDiffuse(
        normalWS,
        light.direction,
        diffuseWrap);

    float shadow = lerp(
        1.0,
        light.shadowAttenuation,
        saturate(shadowResponse));

    float attenuation =
        light.distanceAttenuation *
        shadow *
        diffuse *
        max(0.0, response);

    return
        RiverWaterApplyLightColourInfluence(
            max(light.color, 0.0),
            lightColourInfluence) *
        attenuation;
}

RiverWaterLightingResult RiverWaterEvaluateLighting(
    InputData inputData,
    float ambientResponse,
    float sunResponse,
    float localLightResponse,
    float lightColourInfluence,
    float shadowResponse,
    float diffuseWrap)
{
    RiverWaterLightingResult result;

    float3 normalWS = normalize(inputData.normalWS);

    result.ambient =
        RiverWaterApplyLightColourInfluence(
            max(SampleSH(normalWS), 0.0),
            lightColourInfluence) *
        max(0.0, ambientResponse);

    Light mainLight = GetMainLight(
        inputData.shadowCoord,
        inputData.positionWS,
        half4(1.0, 1.0, 1.0, 1.0));

    result.mainShadowAttenuation =
        mainLight.shadowAttenuation;

    result.sunUnshadowed = RiverWaterEvaluateDirectLight(
        normalWS,
        mainLight,
        sunResponse,
        lightColourInfluence,
        0.0,
        diffuseWrap);

    result.sun = result.sunUnshadowed * lerp(
        1.0,
        mainLight.shadowAttenuation,
        saturate(shadowResponse));

    result.localLights = 0.0;

    #if defined(_ADDITIONAL_LIGHTS)
        #if USE_FORWARD_PLUS
            UNITY_LOOP for (
                uint lightIndex = 0;
                lightIndex <
                    min(
                        URP_FP_DIRECTIONAL_LIGHTS_COUNT,
                        MAX_VISIBLE_LIGHTS);
                lightIndex++)
            {
                Light additionalLight = GetAdditionalLight(
                    lightIndex,
                    inputData.positionWS,
                    half4(1.0, 1.0, 1.0, 1.0));

                result.localLights += RiverWaterEvaluateDirectLight(
                    normalWS,
                    additionalLight,
                    localLightResponse,
                    lightColourInfluence,
                    shadowResponse,
                    diffuseWrap);
            }
        #endif
        

        uint pixelLightCount = GetAdditionalLightsCount();

        LIGHT_LOOP_BEGIN(pixelLightCount)
            Light additionalLight = GetAdditionalLight(
                lightIndex,
                inputData.positionWS,
                half4(1.0, 1.0, 1.0, 1.0));

            result.localLights += RiverWaterEvaluateDirectLight(
                normalWS,
                additionalLight,
                localLightResponse,
                lightColourInfluence,
                shadowResponse,
                diffuseWrap);
        LIGHT_LOOP_END
    #endif

    result.combined =
        result.ambient +
        result.sun +
        result.localLights;

    return result;
}

float3 RiverWaterResolveBodyLightingWithMainShadowPolicy(
    RiverWaterLightingResult lighting,
    float lightDependence,
    float minimumNightVisibility,
    float mainShadowResponse)
{
    float resolvedMainShadow = lerp(
        1.0,
        lighting.mainShadowAttenuation,
        saturate(mainShadowResponse));

    float3 policyLighting =
        lighting.ambient +
        lighting.sunUnshadowed * resolvedMainShadow +
        lighting.localLights;

    float3 responsiveLighting =
        max(
            policyLighting,
            max(0.0, minimumNightVisibility).xxx);

    return lerp(
        1.0.xxx,
        responsiveLighting,
        saturate(lightDependence));
}

#endif
