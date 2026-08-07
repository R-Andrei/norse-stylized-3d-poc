Shader "PS3D/Vegetation/Stylized Vegetation Benchmark"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.22, 0.34, 0.12, 1)
        _RootColor("Root Color", Color) = (0.08, 0.12, 0.045, 1)
        _TipColor("Tip Color", Color) = (0.38, 0.52, 0.20, 1)
        _GrassPatchDarkening("Grass Dark Patch Strength", Range(0, 0.5)) = 0.12
        _GrassPatchBrightening("Grass Light Patch Strength", Range(0, 0.5)) = 0.08
        _AmbientResponse("Ambient Response", Range(0, 2)) = 1.0
        _SunResponse("Sun Response", Range(0, 2)) = 1.0
        _LocalLightResponse("Local Light Response", Range(0, 2)) = 1.0
        _MinimumNightVisibility("Minimum Night Visibility", Range(0, 0.5)) = 0.04
        _DiffuseWrap("Diffuse Wrap", Range(0, 1)) = 0.35
        _NormalUpBias("Normal Up Bias", Range(0, 1)) = 0.42
        _WindNormalResponse("Wind Normal Response", Range(0, 4)) = 0.70
        _WindBendShadingResponse("Wind Bend Shading Response", Range(0, 2)) = 1.0
        _LightColourInfluence("Light Colour Influence", Range(0, 1)) = 1.0
        _StylizedEdgeAccent("Stylized Edge Accent", Range(0, 1)) = 0.22
        _EdgeAccentWidth("Edge Accent Width", Range(0.01, 0.50)) = 0.10
        _MinimumStableAccentPixels("Minimum Stable Accent Pixels", Range(0.5, 2)) = 1.0
        _EdgeHighlightWhiteness("Edge Highlight Whiteness", Range(0, 1)) = 0.75
        _LocalEdgeFalloffPower("Local Edge Falloff Power", Range(1, 8)) = 3.0
        _LocalEdgeActivationThreshold("Local Edge Activation Threshold", Range(0, 2)) = 0.35
        _AlphaCutoff("Card Alpha Cutoff", Range(0, 1)) = 0.42
        _TipWidthRatio("Tip Width Ratio", Range(0, 0.5)) = 0.12
        _TaperStart("Taper Start", Range(0, 0.95)) = 0.68
        _WidthStabilizationEnabled("Width Stabilization Enabled", Float) = 1
        _WidthStabilizationStartDistance("Width Stabilization Start Distance", Float) = 18
        _WidthStabilizationMaximumMultiplier("Width Stabilization Maximum Multiplier", Float) = 1.2
        _InteractionBendResponse("Interaction Bend Response", Range(0, 2)) = 1
        _InteractionFlattenResponse("Interaction Flatten Response", Range(0, 2)) = 1
        _InteractionHeightExponent("Interaction Height Exponent", Range(0.25, 4)) = 1.5
        _InteractionMaximumBend("Interaction Maximum Bend", Range(0, 3)) = 0.65
        _InteractionNormalResponse("Interaction Normal Response", Range(0, 4)) = 1
        _WindInfluenceOnDisplacedGrass("Wind Influence On Displaced Grass", Range(0, 1)) = 1
        _TrampleBendResponse("Trample Bend Response", Range(0, 2)) = 1
        _TrampleFlattenResponse("Trample Flatten Response", Range(0, 2)) = 1
        _TrampleHeightExponent("Trample Height Exponent", Range(0.25, 4)) = 1.25
        _TrampleMaximumBend("Trample Maximum Bend", Range(0, 3)) = 0.8
        _TrampleNormalResponse("Trample Normal Response", Range(0, 4)) = 1
        _WindInfluenceOnTrampledGrass("Wind Influence On Trampled Grass", Range(0, 1)) = 0.25
        [HideInInspector] _VegetationTramplePreviousField("Vegetation Trample Previous Field", 2D) = "black" {}
        [HideInInspector] _VegetationTrampleCurrentField("Vegetation Trample Current Field", 2D) = "black" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On
            ZTest LEqual
            Blend One Zero

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"
            #include "../Includes/VegetationCommon.hlsl"
            #include "../Includes/VegetationWindResponse.hlsl"
            #include "../Includes/VegetationInteractionField.hlsl"
            #include "../Includes/VegetationTrampleField.hlsl"
            #include "../Includes/VegetationLighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RootColor;
                float4 _TipColor;
                float _GrassPatchDarkening;
                float _GrassPatchBrightening;
                float _AmbientResponse;
                float _SunResponse;
                float _LocalLightResponse;
                float _MinimumNightVisibility;
                float _DiffuseWrap;
                float _NormalUpBias;
                float _WindNormalResponse;
                float _WindBendShadingResponse;
                float _LightColourInfluence;
                float _StylizedEdgeAccent;
                float _EdgeAccentWidth;
                float _MinimumStableAccentPixels;
                float _EdgeHighlightWhiteness;
                float _LocalEdgeFalloffPower;
                float _LocalEdgeActivationThreshold;
                float _AlphaCutoff;
                float _TipWidthRatio;
                float _TaperStart;
                float _WidthStabilizationEnabled;
                float _WidthStabilizationStartDistance;
                float _WidthStabilizationMaximumMultiplier;
                float _InteractionBendResponse;
                float _InteractionFlattenResponse;
                float _InteractionHeightExponent;
                float _InteractionMaximumBend;
                float _InteractionNormalResponse;
                float _WindInfluenceOnDisplacedGrass;
                float _TrampleBendResponse;
                float _TrampleFlattenResponse;
                float _TrampleHeightExponent;
                float _TrampleMaximumBend;
                float _TrampleNormalResponse;
                float _WindInfluenceOnTrampledGrass;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 centerXZ : TEXCOORD1;
                float4 color : COLOR;
                uint instanceId : SV_InstanceID;
                uint vertexId : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float rootToTip : TEXCOORD2;
                float colorScale : TEXCOORD3;
                float fogFactor : TEXCOORD4;
                float3 positionWS : TEXCOORD5;
                float signedNormalBendRatio : TEXCOORD6;
                nointerpolation float lightRayAccentCandidate01 : TEXCOORD7;
            };

            Varyings Vert(Attributes input)
            {
                InitIndirectDrawArgs(0);
                uint instanceId = GetIndirectInstanceID(input.instanceId);

                float3 instanceLocalPosition;
                float yaw;
                float2 scale;
                float stiffness;
                float phase;
                float colorVariation;
                float bladeVariation;
                float macroPatch;
                DecodeVegetationInstance(
                    instanceId,
                    instanceLocalPosition,
                    yaw,
                    scale,
                    stiffness,
                    phase,
                    colorVariation,
                    bladeVariation,
                    macroPatch);

                float darkPatchMask = max(-macroPatch, 0.0);
                float lightPatchMask = max(macroPatch, 0.0);
                float macroColorScale =
                    1.0 - darkPatchMask *
                        clamp(_GrassPatchDarkening, 0.0, 0.5) +
                    lightPatchMask *
                        clamp(_GrassPatchBrightening, 0.0, 0.5);
                float microColorScale = lerp(
                    0.90,
                    1.10,
                    saturate(colorVariation));
                float colorScale = microColorScale * macroColorScale;

                float rootToTip = saturate(input.color.r);
                float3 worldPosition = TransformVegetationVertexToWorldStabilized(
                    input.positionOS,
                    input.centerXZ,
                    instanceLocalPosition,
                    yaw,
                    scale,
                    _WidthStabilizationEnabled,
                    _WidthStabilizationStartDistance,
                    _WidthStabilizationMaximumMultiplier);
                float3 interactionSamplePosition = mul(
                    _VegetationLocalToWorld,
                    float4(instanceLocalPosition, 1.0)).xyz;
                VegetationInteractionSample interaction =
                    SampleVegetationInteraction(interactionSamplePosition);
                VegetationTrampleSample trample =
                    SampleVegetationTrample(interactionSamplePosition);
                float effectiveInteractionStrength =
                    VegetationInteractionEffectiveStrength(
                        interaction,
                        _InteractionBendResponse,
                        _InteractionFlattenResponse);
                float effectiveTrampleStrength =
                    VegetationTrampleEffectiveStrength(
                        trample,
                        _TrampleBendResponse,
                        _TrampleFlattenResponse);
                float immediateWindRetention = lerp(
                    1.0,
                    saturate(_WindInfluenceOnDisplacedGrass),
                    effectiveInteractionStrength);
                float trampleWindRetention = lerp(
                    1.0,
                    saturate(_WindInfluenceOnTrampledGrass),
                    effectiveTrampleStrength);
                float retainedWindInfluence =
                    immediateWindRetention * trampleWindRetention;

                float3 preWindWorldPosition = worldPosition;
                float2 fullTipWindDisplacementXZ;
                float3 windDeformedWorldPosition =
                    ApplyVegetationWindResponse(
                        worldPosition,
                        rootToTip,
                        stiffness,
                        phase,
                        bladeVariation,
                        fullTipWindDisplacementXZ);
                worldPosition = lerp(
                    preWindWorldPosition,
                    windDeformedWorldPosition,
                    retainedWindInfluence);
                fullTipWindDisplacementXZ *= retainedWindInfluence;

                float scaledVertexHeight =
                    input.positionOS.y * scale.y;
                float2 fullTipInteractionDisplacementXZ;
                float fullTipInteractionFlatten;
                worldPosition = ApplyVegetationInteractionResponse(
                    worldPosition,
                    interaction,
                    rootToTip,
                    scaledVertexHeight,
                    _InteractionBendResponse,
                    _InteractionFlattenResponse,
                    _InteractionHeightExponent,
                    _InteractionMaximumBend,
                    fullTipInteractionDisplacementXZ,
                    fullTipInteractionFlatten);
                float2 fullTipTrampleDisplacementXZ;
                float fullTipTrampleFlatten;
                worldPosition = ApplyVegetationTrampleResponse(
                    worldPosition,
                    trample,
                    rootToTip,
                    scaledVertexHeight,
                    _TrampleBendResponse,
                    _TrampleFlattenResponse,
                    _TrampleHeightExponent,
                    _TrampleMaximumBend,
                    fullTipTrampleDisplacementXZ,
                    fullTipTrampleFlatten);

                float3 baseNormalWS =
                    TransformVegetationNormalToWorld(input.normalOS, yaw);
                float3 bladeLateralWS = cross(
                    baseNormalWS,
                    float3(0.0, 1.0, 0.0));
                float inverseBladeHeight =
                    rootToTip / max(scaledVertexHeight, 0.0001);
                float commonSlopeScale =
                    2.0 * rootToTip * inverseBladeHeight;
                float interactionNormalAmount =
                    clamp(_InteractionNormalResponse, 0.0, 4.0);
                float trampleNormalAmount =
                    clamp(_TrampleNormalResponse, 0.0, 4.0);
                float2 deformationSlopeXZ =
                    fullTipWindDisplacementXZ *
                        (commonSlopeScale *
                         clamp(_WindNormalResponse, 0.0, 4.0)) +
                    fullTipInteractionDisplacementXZ *
                        (commonSlopeScale * interactionNormalAmount) +
                    fullTipTrampleDisplacementXZ *
                        (commonSlopeScale * trampleNormalAmount);
                float combinedFlatten = saturate(
                    fullTipInteractionFlatten * interactionNormalAmount +
                    fullTipTrampleFlatten * trampleNormalAmount);
                float interactionVerticalTangentScale = max(
                    0.05,
                    1.0 - combinedFlatten);
                float3 deformedCenterlineTangentWS = float3(
                    deformationSlopeXZ.x,
                    interactionVerticalTangentScale,
                    deformationSlopeXZ.y);
                float3 windDeformedNormalWS = cross(
                    deformedCenterlineTangentWS,
                    bladeLateralWS);
                float2 combinedTipDisplacementXZ =
                    fullTipWindDisplacementXZ +
                    fullTipInteractionDisplacementXZ +
                    fullTipTrampleDisplacementXZ;
                float signedNormalBendRatio = dot(
                    baseNormalWS.xz,
                    combinedTipDisplacementXZ) * inverseBladeHeight;

                Varyings output;
                output.positionCS = TransformWorldToHClip(worldPosition);
                output.normalWS = windDeformedNormalWS;
                output.uv = input.uv;
                output.rootToTip = rootToTip;
                output.colorScale = colorScale;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                output.positionWS = worldPosition;
                output.signedNormalBendRatio = signedNormalBendRatio;
                // VegetationClusterMeshBuilder emits exactly six vertices per
                // crossed card: three rows with two vertices per row.
                // SV_VertexID therefore provides a stable logical card index
                // shared by every triangle and segment of that card.
                const uint verticesPerCard = 6u;
                uint cardIndex = input.vertexId / verticesPerCard;
                output.lightRayAccentCandidate01 =
                    VegetationStableAccentCandidate01(
                        instanceId,
                        cardIndex);
                return output;
            }

            half4 Frag(Varyings input, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                float rawSignedSilhouettePosition = input.uv.x * 2.0 - 1.0;
                float taperRange = max(0.0001, 1.0 - _TaperStart);
                float taperT = saturate((input.uv.y - _TaperStart) / taperRange);
                taperT *= taperT * (3.0 - 2.0 * taperT);
                float allowedWidth = lerp(
                    0.92,
                    max(0.02, _TipWidthRatio * 0.92),
                    taperT);
                float visibleHalfWidth = max(
                    0.0001,
                    allowedWidth - _AlphaCutoff * 0.08);

                float signedSilhouettePosition =
                    rawSignedSilhouettePosition / visibleHalfWidth;

                // Differentiate the unsaturated normalized blade coordinate
                // before clip so projected width cannot depend on subpixel
                // placement relative to the discarded card silhouette.
                float2 signedSilhouetteGradient = float2(
                    ddx(signedSilhouettePosition),
                    ddy(signedSilhouettePosition));
                float signedGradientSquared = max(
                    dot(
                        signedSilhouetteGradient,
                        signedSilhouetteGradient),
                    0.00000025);
                float pixelsPerSignedUnit = rsqrt(signedGradientSquared);

                clip(
                    visibleHalfWidth -
                    abs(rawSignedSilhouettePosition));

                float edgeDistance =
                    1.0 - saturate(abs(signedSilhouettePosition));
                float edgeDerivativeAA = max(
                    abs(signedSilhouetteGradient.x) +
                    abs(signedSilhouetteGradient.y),
                    0.0005);
                float edgeWidth = clamp(_EdgeAccentWidth, 0.01, 0.50);
                float edgeAntialias = clamp(
                    edgeDerivativeAA * 0.35,
                    0.0005,
                    0.10);
                float edgeMask = 1.0 - smoothstep(
                    max(0.0, edgeWidth - edgeAntialias),
                    min(1.0, edgeWidth + edgeAntialias),
                    edgeDistance);

                float3 unflippedNormalWS = normalize(input.normalWS);
                float3 bladeLateralWS = cross(
                    unflippedNormalWS,
                    float3(0.0, 1.0, 0.0));

                float3 normalWS = unflippedNormalWS;
                normalWS *= isFrontFace ? 1.0 : -1.0;
                normalWS = normalize(lerp(
                    normalWS,
                    float3(0.0, 1.0, 0.0),
                    saturate(_NormalUpBias)));

                float4 heightColor = lerp(
                    _RootColor,
                    _BaseColor,
                    smoothstep(0.0, 0.55, input.rootToTip));
                heightColor = lerp(
                    heightColor,
                    _TipColor,
                    smoothstep(0.68, 1.0, input.rootToTip));
                heightColor.rgb *= input.colorScale;

                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS =
                    GetWorldSpaceNormalizeViewDir(input.positionWS);
                lightingInput.normalizedScreenSpaceUV =
                    GetNormalizedScreenSpaceUV(input.positionCS);

                VegetationLightingResult vegetationLighting =
                    VegetationEvaluateLighting(
                        lightingInput,
                        bladeLateralWS,
                        signedSilhouettePosition,
                        edgeMask,
                        edgeWidth,
                        pixelsPerSignedUnit,
                        _MinimumStableAccentPixels,
                        input.lightRayAccentCandidate01,
                        _AmbientResponse,
                        _SunResponse,
                        _LocalLightResponse,
                        _LightColourInfluence,
                        _DiffuseWrap,
                        _StylizedEdgeAccent,
                        _EdgeHighlightWhiteness,
                        _LocalEdgeFalloffPower,
                        _LocalEdgeActivationThreshold);
                float3 lighting = VegetationResolveLighting(
                    vegetationLighting,
                    _MinimumNightVisibility);

                float faceSign = isFrontFace ? 1.0 : -1.0;
                float renderedSignedBend =
                    input.signedNormalBendRatio * faceSign;
                float bendActivation = smoothstep(
                    0.03,
                    0.30,
                    abs(renderedSignedBend));
                float heightResponse =
                    input.rootToTip * input.rootToTip *
                    (3.0 - 2.0 * input.rootToTip);
                float bendShade =
                    clamp(_WindBendShadingResponse, 0.0, 2.0) *
                    bendActivation *
                    heightResponse;
                float concaveWeight = step(0.0, renderedSignedBend);
                float convexWeight = 1.0 - concaveWeight;
                float bendBodyMultiplier =
                    1.0 - 0.30 * concaveWeight * bendShade +
                    0.12 * convexWeight * bendShade;

                float3 finalColor =
                    heightColor.rgb * lighting * bendBodyMultiplier +
                    vegetationLighting.edgeAccent;
                finalColor = MixFog(finalColor, input.fogFactor);
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
