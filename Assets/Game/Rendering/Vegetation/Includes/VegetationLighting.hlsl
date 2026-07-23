#ifndef PS3D_VEGETATION_LIGHTING_INCLUDED
#define PS3D_VEGETATION_LIGHTING_INCLUDED

struct VegetationDirectLightingResult
{
    float3 body;
    float3 edge;
};

struct VegetationLightingResult
{
    float3 ambient;
    float3 sun;
    float3 localLights;
    float3 edgeAccent;
    float3 combined;
};

float VegetationLuminance(float3 colour)
{
    return dot(colour, float3(0.2126, 0.7152, 0.0722));
}

float3 VegetationApplyLightColourInfluence(
    float3 lightColour,
    float colourInfluence)
{
    float luminance = VegetationLuminance(lightColour);
    return lerp(
        luminance.xxx,
        lightColour,
        saturate(colourInfluence));
}

float3 VegetationWhitenLightColour(
    float3 lightColour,
    float whiteness)
{
    float peakIntensity = max(
        max(lightColour.r, lightColour.g),
        lightColour.b);
    if (peakIntensity <= 0.000001)
    {
        return 0.0;
    }

    float3 chroma = lightColour / peakIntensity;
    return lerp(
        chroma,
        float3(1.0, 1.0, 1.0),
        saturate(whiteness)) * peakIntensity;
}

float VegetationTwoSidedWrappedDiffuse(
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

float VegetationLightFacingEdge(
    float signedSilhouettePosition,
    float3 bladeLateralWS,
    float3 lightDirectionWS,
    out float lateralAlignment)
{
    lateralAlignment = 0.0;

    float lateralMagnitudeSquared = dot(bladeLateralWS, bladeLateralWS);
    if (lateralMagnitudeSquared <= 0.000001)
    {
        return 0.0;
    }

    float3 resolvedLateralWS =
        bladeLateralWS * rsqrt(lateralMagnitudeSquared);
    float lateralLightDirection = dot(
        resolvedLateralWS,
        normalize(lightDirectionWS));
    lateralAlignment = abs(lateralLightDirection);

    // VEG-V1C.4 uses a strict directional dead zone. The opposite edge and
    // nearly perpendicular light/blade orientations receive exactly zero.
    float orientationGate = smoothstep(
        0.15,
        0.45,
        lateralAlignment);
    float orientedSide =
        clamp(signedSilhouettePosition, -1.0, 1.0) *
        lateralLightDirection;
    float selectedSide = smoothstep(
        0.05,
        0.35,
        orientedSide);
    return selectedSide * orientationGate;
}

float VegetationPerLightEdgeStability(
    float edgeWidth,
    float pixelsPerSignedUnit,
    float minimumStableAccentPixels,
    float lateralAlignment)
{
    float resolvedEdgeWidth = clamp(edgeWidth, 0.01, 0.50);
    float authoredBandStart = 1.0 - resolvedEdgeWidth;

    // The selected-side smoothstep spans 0.05..0.35, so its midpoint is
    // 0.20. Measure only the band that remains at least half-strength after
    // the punctual light's lateral alignment narrows the selected side.
    float halfWeightSidePosition =
        0.20 / max(lateralAlignment, 0.0001);
    float effectiveBandStart = max(
        authoredBandStart,
        halfWeightSidePosition);
    float effectiveBandWidth = max(
        0.0,
        1.0 - effectiveBandStart);
    float effectiveBandPixels =
        effectiveBandWidth * max(0.0, pixelsPerSignedUnit);

    float minimumStablePixels = clamp(
        minimumStableAccentPixels,
        0.5,
        2.0);
    return smoothstep(
        minimumStablePixels,
        minimumStablePixels + 0.20,
        effectiveBandPixels);
}

VegetationDirectLightingResult VegetationEvaluateDirectLight(
    float3 normalWS,
    float3 bladeLateralWS,
    float signedSilhouettePosition,
    float edgeMask,
    float edgeWidth,
    float pixelsPerSignedUnit,
    float minimumStableAccentPixels,
    Light light,
    float response,
    float lightColourInfluence,
    float diffuseWrap,
    float stylizedEdgeAccent,
    float edgeHighlightWhiteness,
    float localEdgeFalloffPower,
    float allowEdgeAccent,
    float localEdgeActivationThreshold)
{
    VegetationDirectLightingResult result;

    float diffuse = VegetationTwoSidedWrappedDiffuse(
        normalWS,
        light.direction,
        diffuseWrap);

    float resolvedResponse = max(0.0, response);
    float resolvedAccent = saturate(stylizedEdgeAccent);
    float edgeEligibility = saturate(allowEdgeAccent);
    float directAttenuation =
        light.distanceAttenuation *
        resolvedResponse;
    float normalizedEdgeAttenuation = saturate(
        max(0.0, light.distanceAttenuation));
    float shapedEdgeAttenuation =
        pow(
            normalizedEdgeAttenuation,
            clamp(localEdgeFalloffPower, 1.0, 8.0)) *
        resolvedResponse;
    float edgeActivationAttenuation =
        normalizedEdgeAttenuation *
        resolvedResponse;

    // The master remains a single 0..1 control. Low and medium settings use
    // a gentler response, while 1.0 retains the established maximum gain.
    float accentResponse =
        resolvedAccent *
        lerp(0.125, 1.0, resolvedAccent);

    // Directional lights pass edge eligibility 0 and therefore preserve the
    // exact VEG-V1C broad direct-light term. Only eligible punctual lights
    // receive the coupled body-fill restraint.
    float broadFillScale = lerp(
        1.0,
        0.75,
        accentResponse * edgeEligibility);
    float broadContribution =
        directAttenuation *
        diffuse *
        broadFillScale;

    float3 influencedLightColour =
        VegetationApplyLightColourInfluence(
            max(light.color, 0.0),
            lightColourInfluence);
    result.body = influencedLightColour * broadContribution;

    float lateralAlignment;
    float facingEdge = VegetationLightFacingEdge(
        signedSilhouettePosition,
        bladeLateralWS,
        light.direction,
        lateralAlignment);
    float pixelStability = 0.0;
    if (edgeEligibility > 0.0)
    {
        pixelStability = VegetationPerLightEdgeStability(
            edgeWidth,
            pixelsPerSignedUnit,
            minimumStableAccentPixels,
            lateralAlignment);
    }
    float edgeDiffuseSupport = lerp(0.65, 1.0, diffuse);

    // VEG-V1C.6 preserves the established 4.0 maximum while making low and
    // medium master values materially easier to tune.
    float edgeGain = 4.0 * accentResponse;

    // Activation uses unpowered normalized attenuation. Falloff Power shapes
    // final edge radiance once instead of compounding the eligibility curve.
    float localEnergy =
        VegetationLuminance(max(light.color, 0.0)) *
        edgeActivationAttenuation *
        edgeDiffuseSupport;
    float activationThreshold = max(
        0.0,
        localEdgeActivationThreshold);
    float activationSoftness = max(
        0.08,
        activationThreshold * 0.35);
    float localEdgeActivation =
        edgeEligibility *
        smoothstep(
            activationThreshold,
            activationThreshold + activationSoftness,
            localEnergy);

    float3 whitenedLightColour = VegetationWhitenLightColour(
        influencedLightColour,
        edgeHighlightWhiteness);
    result.edge =
        whitenedLightColour *
        shapedEdgeAttenuation *
        localEdgeActivation *
        saturate(edgeMask) *
        facingEdge *
        pixelStability *
        edgeDiffuseSupport *
        edgeGain;

    return result;
}

VegetationLightingResult VegetationEvaluateLighting(
    InputData inputData,
    float3 bladeLateralWS,
    float signedSilhouettePosition,
    float edgeMask,
    float edgeWidth,
    float pixelsPerSignedUnit,
    float minimumStableAccentPixels,
    float ambientResponse,
    float sunResponse,
    float localLightResponse,
    float lightColourInfluence,
    float diffuseWrap,
    float stylizedEdgeAccent,
    float edgeHighlightWhiteness,
    float localEdgeFalloffPower,
    float localEdgeActivationThreshold)
{
    VegetationLightingResult result;

    float3 resolvedNormalWS = normalize(inputData.normalWS);
    result.ambient =
        VegetationApplyLightColourInfluence(
            max(SampleSH(resolvedNormalWS), 0.0),
            lightColourInfluence) *
        max(0.0, ambientResponse);

    Light mainLight = GetMainLight(
        float4(0.0, 0.0, 0.0, 0.0),
        inputData.positionWS,
        half4(1.0, 1.0, 1.0, 1.0));
    VegetationDirectLightingResult mainResult =
        VegetationEvaluateDirectLight(
            resolvedNormalWS,
            bladeLateralWS,
            signedSilhouettePosition,
            edgeMask,
            edgeWidth,
            pixelsPerSignedUnit,
            minimumStableAccentPixels,
            mainLight,
            sunResponse,
            lightColourInfluence,
            diffuseWrap,
            stylizedEdgeAccent,
            edgeHighlightWhiteness,
            localEdgeFalloffPower,
            0.0,
            localEdgeActivationThreshold);
    result.sun = mainResult.body;
    result.edgeAccent = 0.0;

    result.localLights = 0.0;

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
                    half4(1.0, 1.0, 1.0, 1.0));

                VegetationDirectLightingResult additionalResult =
                    VegetationEvaluateDirectLight(
                        resolvedNormalWS,
                        bladeLateralWS,
                        signedSilhouettePosition,
                        edgeMask,
                        edgeWidth,
                        pixelsPerSignedUnit,
                        minimumStableAccentPixels,
                        additionalLight,
                        localLightResponse,
                        lightColourInfluence,
                        diffuseWrap,
                        stylizedEdgeAccent,
                        edgeHighlightWhiteness,
                        localEdgeFalloffPower,
                        0.0,
                        localEdgeActivationThreshold);
                result.localLights += additionalResult.body;
            }
        #endif

        uint pixelLightCount = GetAdditionalLightsCount();

        LIGHT_LOOP_BEGIN(pixelLightCount)
            Light additionalLight = GetAdditionalLight(
                lightIndex,
                inputData.positionWS,
                half4(1.0, 1.0, 1.0, 1.0));

            VegetationDirectLightingResult additionalResult =
                VegetationEvaluateDirectLight(
                    resolvedNormalWS,
                    bladeLateralWS,
                    signedSilhouettePosition,
                    edgeMask,
                    edgeWidth,
                    pixelsPerSignedUnit,
                    minimumStableAccentPixels,
                    additionalLight,
                    localLightResponse,
                    lightColourInfluence,
                    diffuseWrap,
                    stylizedEdgeAccent,
                    edgeHighlightWhiteness,
                    localEdgeFalloffPower,
                    1.0,
                    localEdgeActivationThreshold);
            result.localLights += additionalResult.body;
            result.edgeAccent += additionalResult.edge;
        LIGHT_LOOP_END
    #endif

    result.combined =
        result.ambient +
        result.sun +
        result.localLights;

    return result;
}

float3 VegetationResolveLighting(
    VegetationLightingResult lighting,
    float minimumNightVisibility)
{
    return max(
        lighting.combined,
        max(0.0, minimumNightVisibility).xxx);
}

#endif
