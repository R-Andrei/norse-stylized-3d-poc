#ifndef PS3D_VEGETATION_LIGHTING_INCLUDED
#define PS3D_VEGETATION_LIGHTING_INCLUDED

// WEATHER VEGETATION ACCENT CONTRACT — PROTECTED SHADER BOUNDARY.
//
// Body lighting always uses URP's real punctual Light.direction, attenuation,
// colour, and cone. Weather blade-edge selection MUST use the per-Light
// horizontal source direction in the indexed sidecar; never reconstruct it
// from Spot position and never fall back to the radial Spot direction.
//
// Coverage is a stable whole-card participation threshold only. It must not
// scale radiance or apply a radial/spatial mask. Softness shapes only the
// selected blade-edge profile. It must not change card participation,
// directional eligibility, attenuation, or the LightRay footprint.
//
// This two-float4 layout is mirrored exactly by
// WeatherLightRayRendererFeature.VegetationAccentGpuRecord. Any layout change
// must update both files together.
struct VegetationAdditionalLightAccentData
{
    // x = preset-resolved radiance scale
    // y = stable whole-card coverage
    // z = selected blade-edge profile softness
    // w = explicit Weather override active
    float4 parameters;

    // xyz = normalized horizontal direction from receiver toward source
    // w = valid direction flag
    float4 sourceDirectionWS;
};

float4 _WeatherLightRayVegetationAccentDirectionWS;
float _WeatherLightRayAccentLineIntensity;
float _WeatherLightRayAccentLineResolvedScale;
float _WeatherLightRayVegetationAccentCoverage;
// Legacy diagnostic globals remain for report compatibility. Production
// Weather accent selection uses the indexed additional-light sidecar below.
float4 _WeatherLightRayVegetationAccentSpotPositionWS;
StructuredBuffer<VegetationAdditionalLightAccentData>
    _VegetationAdditionalLightAccentData;
int _VegetationAdditionalLightAccentDataCount;
float _WeatherLightRayVegetationDiagnosticMode;


struct VegetationDirectLightingResult
{
    float3 body;
    float3 edge;
    float lightRaySpotMatch;
    float publishedDirectionActive;
    float accentOverrideSelected;
};

struct VegetationLightingResult
{
    float3 ambient;
    float3 sun;
    float3 localLights;
    float3 edgeAccent;
    float3 combined;
    float lightLayersVariantActive;
    float publishedSpotActive;
    float anyAdditionalLightSeen;
    float lightRaySpotMatchSeen;
    float lightRayLayerMatch;
    float publishedDirectionActive;
    float accentOverrideSelected;
    float lightRayBodyLuminance;
    float lightRayEdgeLuminance;
};

uint VegetationHashU32(uint value)
{
    value ^= value >> 16;
    value *= 0x7feb352du;
    value ^= value >> 15;
    value *= 0x846ca68bu;
    value ^= value >> 16;
    return value;
}

float VegetationStableAccentCandidate01(
    uint instanceId,
    uint cardIndex)
{
    // One deterministic decision owns the entire crossed card. Every vertex
    // of a card supplies the same discrete card index, so no triangle or
    // longitudinal blade segment can be selected independently.
    uint seed = VegetationHashU32(instanceId + 0x9e3779b9u);
    seed ^= VegetationHashU32(cardIndex + 0x85ebca6bu);
    return (VegetationHashU32(seed) & 0x00ffffffu) / 16777216.0;
}

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


bool VegetationLightMatchesRenderingLayer(
    Light light,
    uint meshRenderingLayers)
{
    #if defined(_LIGHT_LAYERS)
        return IsMatchingLightLayer(
            light.layerMask,
            meshRenderingLayers);
    #else
        return true;
    #endif
}

bool VegetationHasPublishedLightRaySpot()
{
    return _WeatherLightRayVegetationAccentDirectionWS.w > 0.5;
}

bool VegetationShouldEvaluateWeatherLightRayAccent()
{
    bool productionAccentActive =
        _WeatherLightRayAccentLineResolvedScale > 0.0;
    bool diagnosticActive =
        _WeatherLightRayVegetationDiagnosticMode > 0.5;
    return productionAccentActive || diagnosticActive;
}

VegetationAdditionalLightAccentData VegetationEmptyAccentData()
{
    VegetationAdditionalLightAccentData data;
    data.parameters = 0.0;
    data.sourceDirectionWS = 0.0;
    return data;
}

VegetationAdditionalLightAccentData VegetationGetAdditionalLightAccentData(
    uint loopLightIndex)
{
    int dataIndex;
    #if USE_CLUSTER_LIGHT_LOOP
        dataIndex = (int)loopLightIndex;
    #else
        dataIndex = GetPerObjectLightIndex(loopLightIndex);
    #endif

    if (dataIndex < 0 ||
        dataIndex >= _VegetationAdditionalLightAccentDataCount)
    {
        return VegetationEmptyAccentData();
    }

    return _VegetationAdditionalLightAccentData[dataIndex];
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
    float3 positionWS,
    float3 bladeLateralWS,
    float signedSilhouettePosition,
    float edgeMask,
    float edgeWidth,
    float pixelsPerSignedUnit,
    float minimumStableAccentPixels,
    float lightRayAccentCandidate01,
    Light light,
    VegetationAdditionalLightAccentData vegetationAccentData,
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
    float accentOverrideWeight =
        saturate(vegetationAccentData.parameters.w);
    result.lightRaySpotMatch = accentOverrideWeight;
    result.publishedDirectionActive = 0.0;
    result.accentOverrideSelected = accentOverrideWeight;

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

    // DIRECTION OWNERSHIP CONTRACT:
    // - real punctual direction remains authoritative for body lighting above;
    // - only an explicit Weather override may substitute the horizontal
    //   celestial/LightRay direction for the stylized edge-side selector;
    // - an invalid Weather direction disables Weather edge output rather than
    //   falling back to the radial Spot direction and recreating a rim mask.
    float3 edgeLightDirectionWS = light.direction;
    if (accentOverrideWeight > 0.5)
    {
        float3 sourceDirectionWS =
            vegetationAccentData.sourceDirectionWS.xyz;
        float sourceDirectionLengthSquared = dot(
            sourceDirectionWS,
            sourceDirectionWS);
        bool validSourceDirection =
            vegetationAccentData.sourceDirectionWS.w > 0.5 &&
            sourceDirectionLengthSquared > 0.000001;
        if (validSourceDirection)
        {
            edgeLightDirectionWS = sourceDirectionWS *
                rsqrt(sourceDirectionLengthSquared);
            result.publishedDirectionActive = 1.0;
        }
        else
        {
            edgeEligibility = 0.0;
        }
    }

    float lateralAlignment;
    float facingEdge = VegetationLightFacingEdge(
        signedSilhouettePosition,
        bladeLateralWS,
        edgeLightDirectionWS,
        lateralAlignment);

    // Preserve the established ordinary-punctual directional profile exactly.
    // Weather Softness is deliberately NOT applied here because changing this
    // term changes which cards/orientations qualify instead of softening the
    // selected line itself.
    facingEdge = pow(saturate(facingEdge), 1.125);
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

    // Ordinary punctual lights retain the established vegetation-local gain.
    // For a registered LightRay Spot, first capture the former
    // AF5D full-scale result, then apply a deliberately wide exponential
    // controller range. AH preserves AF5G's 40%-of-AF5F response and
    // uses the indexed sidecar only when an explicit Weather override is present: 0.10 is about 0.20x the former AF5D
    // maximum, 0.20 about 0.60x, 0.50 about 6.13x, and 1.0 is 200x. The former
    // min(4, ...) LightRay ceiling remains intentionally removed; the controller
    // is the explicit artistic authority.
    float edgeGain = 4.0 * accentResponse;
    if (result.accentOverrideSelected > 0.5)
    {
        float formerAf5dMaximumGain = min(4.0, edgeGain * 12.0);
        float relativeAccentScale =
            max(0.0, vegetationAccentData.parameters.x);
        edgeGain = formerAf5dMaximumGain * relativeAccentScale;
    }

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

    float lightRayParticipation = 1.0;
    if (result.accentOverrideSelected > 0.5 &&
        _WeatherLightRayVegetationDiagnosticMode <= 0.5)
    {
        float coverage =
            saturate(vegetationAccentData.parameters.y);
        lightRayParticipation = coverage <= 0.0
            ? 0.0
            : (coverage >= 1.0
                ? 1.0
                : step(
                    saturate(lightRayAccentCandidate01),
                    coverage));
    }

    float resolvedEdgeProfile = saturate(edgeMask);
    if (result.accentOverrideSelected > 0.5)
    {
        // SOFTNESS CONTRACT:
        // 0 = crisp/narrow selected edge profile
        // 0.5 = preserve the authored vegetation edge mask
        // 1 = broad/soft selected edge profile
        // This transformation occurs after card participation and direction
        // selection, so sweeping Softness cannot change which cards are chosen
        // or create a radial centre exclusion.
        float softness =
            saturate(vegetationAccentData.parameters.z);
        float profileExponent = exp2(2.0 - 4.0 * softness);
        resolvedEdgeProfile = pow(
            resolvedEdgeProfile,
            profileExponent);
    }

    float3 whitenedLightColour = VegetationWhitenLightColour(
        influencedLightColour,
        edgeHighlightWhiteness);
    result.edge =
        whitenedLightColour *
        shapedEdgeAttenuation *
        localEdgeActivation *
        resolvedEdgeProfile *
        facingEdge *
        pixelStability *
        edgeDiffuseSupport *
        edgeGain *
        lightRayParticipation;

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
    float lightRayAccentCandidate01,
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
    bool vegetationDiagnosticActive =
        _WeatherLightRayVegetationDiagnosticMode > 0.5;
    #if defined(_LIGHT_LAYERS)
        result.lightLayersVariantActive = 1.0;
    #else
        result.lightLayersVariantActive = 0.0;
    #endif
    result.publishedSpotActive =
        vegetationDiagnosticActive &&
        _VegetationAdditionalLightAccentDataCount > 0
            ? 1.0
            : 0.0;
    result.anyAdditionalLightSeen = 0.0;
    result.lightRaySpotMatchSeen = 0.0;
    result.lightRayLayerMatch = 0.0;
    result.publishedDirectionActive = 0.0;
    result.accentOverrideSelected = 0.0;
    result.lightRayBodyLuminance = 0.0;
    result.lightRayEdgeLuminance = 0.0;

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
            inputData.positionWS,
            bladeLateralWS,
            signedSilhouettePosition,
            edgeMask,
            edgeWidth,
            pixelsPerSignedUnit,
            minimumStableAccentPixels,
            lightRayAccentCandidate01,
            mainLight,
            VegetationEmptyAccentData(),
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
        uint meshRenderingLayers = GetMeshRenderingLayer();
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
                VegetationAdditionalLightAccentData additionalAccentData =
                    VegetationGetAdditionalLightAccentData(lightIndex);

                bool isWeatherLightRay = false;
                if (vegetationDiagnosticActive)
                {
                    result.anyAdditionalLightSeen = 1.0;
                    isWeatherLightRay =
                        additionalAccentData.parameters.w > 0.5;
                    if (isWeatherLightRay)
                    {
                        result.lightRaySpotMatchSeen = 1.0;
                    }
                }

                bool matchesRenderingLayer =
                    VegetationLightMatchesRenderingLayer(
                        additionalLight,
                        meshRenderingLayers);
                if (vegetationDiagnosticActive &&
                    isWeatherLightRay &&
                    matchesRenderingLayer)
                {
                    result.lightRayLayerMatch = 1.0;
                }

                if (matchesRenderingLayer)
                {
                    VegetationDirectLightingResult additionalResult =
                        VegetationEvaluateDirectLight(
                            resolvedNormalWS,
                            inputData.positionWS,
                            bladeLateralWS,
                            signedSilhouettePosition,
                            edgeMask,
                            edgeWidth,
                            pixelsPerSignedUnit,
                            minimumStableAccentPixels,
                            lightRayAccentCandidate01,
                            additionalLight,
                            additionalAccentData,
                            localLightResponse,
                            lightColourInfluence,
                            diffuseWrap,
                            stylizedEdgeAccent,
                            edgeHighlightWhiteness,
                            localEdgeFalloffPower,
                            0.0,
                            localEdgeActivationThreshold);
                    result.localLights += additionalResult.body;
                    if (vegetationDiagnosticActive &&
                        isWeatherLightRay)
                    {
                        result.lightRayBodyLuminance = max(
                            result.lightRayBodyLuminance,
                            VegetationLuminance(additionalResult.body));
                    }
                }
            }
        #endif

        uint pixelLightCount = GetAdditionalLightsCount();

        LIGHT_LOOP_BEGIN(pixelLightCount)
            Light additionalLight = GetAdditionalLight(
                lightIndex,
                inputData.positionWS,
                half4(1.0, 1.0, 1.0, 1.0));
            VegetationAdditionalLightAccentData additionalAccentData =
                VegetationGetAdditionalLightAccentData(lightIndex);

            if (vegetationDiagnosticActive)
            {
                result.anyAdditionalLightSeen = 1.0;
            }

            bool matchesRenderingLayer =
                VegetationLightMatchesRenderingLayer(
                    additionalLight,
                    meshRenderingLayers);

            if (matchesRenderingLayer)
            {
                VegetationDirectLightingResult additionalResult =
                    VegetationEvaluateDirectLight(
                        resolvedNormalWS,
                        inputData.positionWS,
                        bladeLateralWS,
                        signedSilhouettePosition,
                        edgeMask,
                        edgeWidth,
                        pixelsPerSignedUnit,
                        minimumStableAccentPixels,
                        lightRayAccentCandidate01,
                        additionalLight,
                        additionalAccentData,
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
                if (vegetationDiagnosticActive &&
                    additionalResult.lightRaySpotMatch > 0.5)
                {
                    result.lightRaySpotMatchSeen = 1.0;
                    result.lightRayLayerMatch = 1.0;
                    result.publishedDirectionActive = max(
                        result.publishedDirectionActive,
                        additionalResult.publishedDirectionActive);
                    result.accentOverrideSelected = max(
                        result.accentOverrideSelected,
                        additionalResult.accentOverrideSelected);
                    result.lightRayBodyLuminance = max(
                        result.lightRayBodyLuminance,
                        VegetationLuminance(additionalResult.body));
                    result.lightRayEdgeLuminance = max(
                        result.lightRayEdgeLuminance,
                        VegetationLuminance(additionalResult.edge));
                }
            }
        LIGHT_LOOP_END
    #endif

    result.combined =
        result.ambient +
        result.sun +
        result.localLights;

    return result;
}

float3 VegetationResolveLightRayDiagnosticColour(
    VegetationLightingResult lighting)
{
    if (lighting.publishedSpotActive < 0.5)
    {
        return float3(1.0, 0.0, 1.0);
    }

    if (lighting.anyAdditionalLightSeen < 0.5)
    {
        return float3(1.0, 0.0, 0.0);
    }

    if (lighting.lightRaySpotMatchSeen < 0.5)
    {
        return float3(1.0, 0.25, 0.0);
    }

    if (lighting.lightRayLayerMatch < 0.5)
    {
        return float3(0.65, 0.0, 1.0);
    }

    if (lighting.publishedDirectionActive < 0.5)
    {
        return float3(1.0, 1.0, 0.0);
    }

    if (lighting.accentOverrideSelected < 0.5)
    {
        return float3(0.0, 1.0, 1.0);
    }

    if (lighting.lightRayBodyLuminance <= 0.00001)
    {
        return float3(0.0, 0.08, 0.35);
    }

    if (lighting.lightRayEdgeLuminance <= 0.00001)
    {
        return float3(0.0, 0.20, 1.0);
    }

    return float3(0.0, 1.0, 0.0);
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
