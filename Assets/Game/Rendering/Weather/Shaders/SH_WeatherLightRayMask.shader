Shader "Hidden/PS3D/Weather LightRay Mask"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "WeatherLightRayContinuousBeamMask"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Off
            ZWrite Off
            ZTest Always
            Blend One OneMinusSrcColor
            ColorMask R

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #define WEATHER_LIGHT_RAY_ENABLE_BEAM_BUFFER 1
            #include "Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 beamVariation : TEXCOORD2;
                float3 beamBaseWS : TEXCOORD3;
                float beamWidth : TEXCOORD4;
                float4 beamProfile0 : TEXCOORD5;
                float2 beamProfile1 : TEXCOORD6;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float height = max(
                    0.001,
                    _WeatherLightRayBaseCentreHeight.w);
                float3 rayDirection = normalize(
                    _WeatherLightRayDirectionAreaDiameter.xyz);
                float3 upwardAxis = -rayDirection;
                float3 contactAxis = WeatherLightRayGetGroundContactAxis();
                uint localVertex = input.vertexID % 6u;
                float beamIndex = (float)(input.vertexID / 6u);
                float2 quadUv;
                if (localVertex == 0u)
                {
                    quadUv = float2(0.0, 0.0);
                }
                else if (localVertex == 1u)
                {
                    quadUv = float2(0.0, 1.0);
                }
                else if (localVertex == 2u || localVertex == 4u)
                {
                    quadUv = float2(1.0, 1.0);
                }
                else if (localVertex == 3u)
                {
                    quadUv = float2(0.0, 0.0);
                }
                else
                {
                    quadUv = float2(1.0, 0.0);
                }
                float positionOSX = lerp(-1.0, 1.0, quadUv.x);

                float centreOffset;
                float beamWidth;
                WeatherLightRayGetBeamLayout(
                    beamIndex,
                    centreOffset,
                    beamWidth);
                float beamIntensity;
                float beamPhase;
                float upperFadeScale;
                float groundFadeScale;
                float leftSoftness;
                float rightSoftness;
                float peakBias;
                float leftTransmission;
                float rightTransmission;
                float contactOpacityScale;
                WeatherLightRayGetBeamVariation(
                    beamIndex,
                    beamIntensity,
                    beamPhase,
                    upperFadeScale,
                    groundFadeScale,
                    leftSoftness,
                    rightSoftness,
                    peakBias,
                    leftTransmission,
                    rightTransmission,
                    contactOpacityScale);
                float3 beamBase =
                    _WeatherLightRayBaseCentreHeight.xyz +
                    contactAxis * centreOffset;
                float resolvedGroundFadeFraction = clamp(
                    _WeatherLightRayBeamShape1.y * groundFadeScale,
                    0.001,
                    0.49);
                float belowContactExtensionFraction =
                    resolvedGroundFadeFraction * 0.35;
                float longitudinalFraction = lerp(
                    -belowContactExtensionFraction,
                    1.0,
                    quadUv.y);
                float3 positionWS = beamBase +
                    upwardAxis * (longitudinalFraction * height) +
                    contactAxis *
                        (positionOSX * beamWidth * 0.5);

                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.uv = float2(quadUv.x, longitudinalFraction);
                output.beamVariation = float4(
                    beamIntensity,
                    beamPhase,
                    upperFadeScale,
                    groundFadeScale);
                output.beamBaseWS = beamBase;
                output.beamWidth = beamWidth;
                output.beamProfile0 = float4(
                    leftSoftness,
                    rightSoftness,
                    peakBias,
                    leftTransmission);
                output.beamProfile1 = float2(
                    rightTransmission,
                    contactOpacityScale);
                return output;
            }

            float Frag(Varyings input) : SV_Target
            {
                float2 screenUV = GetNormalizedScreenSpaceUV(
                    input.positionCS);
                float groundFadeLength = clamp(
                    _WeatherLightRayBeamShape1.y *
                        input.beamVariation.w,
                    0.001,
                    0.49);
                float aboveContactFadeLength = max(
                    0.00065,
                    groundFadeLength * 0.65);
                float belowContactFadeLength = max(
                    0.00035,
                    groundFadeLength * 0.35);
                float contactPlaneOpacity = saturate(
                    WeatherLightRayGetContactPlaneOpacity() *
                    input.beamProfile1.y);
                float depthFade = 1.0;
                float rawDepth = SampleSceneDepth(screenUV);
                if (WeatherLightRayRawDepthIsValid(rawDepth) > 0.5)
                {
                    float3 scenePositionWS =
                        WeatherLightRayReconstructWorldPosition(
                            screenUV,
                            rawDepth);
                    float3 cameraPositionWS = GetCameraPositionWS();
                    float sceneDistance = distance(
                        cameraPositionWS,
                        scenePositionWS);
                    float ribbonDistance = distance(
                        cameraPositionWS,
                        input.positionWS);
                    float foregroundSeparation =
                        ribbonDistance - sceneDistance;
                    float fadeRange = max(
                        0.18,
                        input.beamWidth * 0.65);
                    float depthTestFade = 1.0 - smoothstep(
                        0.015,
                        fadeRange,
                        foregroundSeparation);

                    float3 normalCross = cross(
                        ddx(scenePositionWS),
                        ddy(scenePositionWS));
                    float normalLengthSquared = dot(
                        normalCross,
                        normalCross);
                    float3 sceneNormalWS = normalLengthSquared > 1e-8
                        ? normalCross * rsqrt(normalLengthSquared)
                        : float3(0.0, 1.0, 0.0);
                    float groundFacing = smoothstep(
                        0.45,
                        0.78,
                        abs(sceneNormalWS.y));
                    float heightAboveContact =
                        scenePositionWS.y - input.beamBaseWS.y;
                    float nearContactPlane = 1.0 - smoothstep(
                        0.12,
                        0.40,
                        heightAboveContact);
                    float lowGroundReceiver = groundFacing *
                        (1.0 - smoothstep(
                            0.40,
                            1.25,
                            max(0.0, heightAboveContact)));
                    float averageBeamWidth =
                        WeatherLightRayGetAverageBeamWidth();
                    float axialHeightAboveBase = input.uv.y *
                        _WeatherLightRayBaseCentreHeight.w;
                    float contactPreserveHeight = max(
                        0.75,
                        averageBeamWidth * 2.0);
                    float contactPreserveWeight = 1.0 - smoothstep(
                        0.0,
                        contactPreserveHeight,
                        axialHeightAboveBase);
                    float belowContactVisibility = smoothstep(
                        -belowContactFadeLength,
                        0.0,
                        input.uv.y);
                    float receiverWeight = saturate(max(
                        max(nearContactPlane, lowGroundReceiver),
                        contactPreserveWeight)) *
                        belowContactVisibility;
                    float occluderWeight = 1.0 - receiverWeight;
                    depthFade = lerp(
                        1.0,
                        depthTestFade,
                        occluderWeight);
                }

                float acrossU = saturate(input.uv.x);
                float densityBreathing = 1.0;

                float leftFeather = clamp(
                    input.beamProfile0.x,
                    0.015,
                    0.46);
                float rightFeather = clamp(
                    input.beamProfile0.y,
                    0.015,
                    0.46);
                float leftEdge = smoothstep(
                    0.0,
                    leftFeather,
                    acrossU);
                float rightEdge = 1.0 - smoothstep(
                    1.0 - rightFeather,
                    1.0,
                    acrossU);
                float edgeSupport = leftEdge * rightEdge;

                float peakU = clamp(
                    0.5 + input.beamProfile0.z,
                    0.16,
                    0.84);
                float profileDistance = acrossU < peakU
                    ? (peakU - acrossU) / max(0.08, peakU)
                    : (acrossU - peakU) /
                        max(0.08, 1.0 - peakU);
                profileDistance = saturate(profileDistance);
                float coreExponent = lerp(
                    2.8,
                    1.2,
                    saturate((leftFeather + rightFeather) * 1.5)) /
                    densityBreathing;
                float coreDensity = exp2(
                    -profileDistance * profileDistance *
                    coreExponent);
                float sideTransmission = lerp(
                    input.beamProfile0.w,
                    input.beamProfile1.x,
                    smoothstep(0.0, 1.0, acrossU));
                float widthProfile = saturate(
                    edgeSupport * coreDensity * sideTransmission);

                float upperFadeLength = clamp(
                    _WeatherLightRayBeamShape1.x *
                        input.beamVariation.z,
                    0.001,
                    0.49);
                float crossFadeScale = lerp(
                    0.72,
                    1.35,
                    profileDistance);
                float aboveContactT = smoothstep(
                    0.0,
                    aboveContactFadeLength,
                    max(0.0, input.uv.y));
                float aboveContactFade = lerp(
                    contactPlaneOpacity,
                    1.0,
                    aboveContactT);
                float belowContactT = smoothstep(
                    -belowContactFadeLength,
                    0.0,
                    input.uv.y);
                float belowContactFade =
                    contactPlaneOpacity * belowContactT;
                float groundFade = input.uv.y >= 0.0
                    ? aboveContactFade
                    : belowContactFade;
                float upperFade = smoothstep(
                    0.0,
                    upperFadeLength * crossFadeScale,
                    1.0 - input.uv.y);

                float fluctuation = 1.0;
                float longitudinalA = 0.5 + 0.5 * sin(
                    input.uv.y * WEATHER_LIGHT_RAY_PI * 2.3 +
                    input.beamVariation.y * 0.73);
                float longitudinalB = 0.5 + 0.5 * sin(
                    input.uv.y * WEATHER_LIGHT_RAY_PI * 4.7 -
                    input.beamVariation.y * 1.91);
                float longitudinal = lerp(
                    0.94,
                    1.0,
                    longitudinalA * 0.68 + longitudinalB * 0.32);

                float3 beamTopWS = input.beamBaseWS -
                    normalize(_WeatherLightRayDirectionAreaDiameter.xyz) *
                    _WeatherLightRayBaseCentreHeight.w;
                float cameraDistanceToBeam =
                    WeatherLightRayDistanceToSegment(
                        GetCameraPositionWS(),
                        input.beamBaseWS,
                        beamTopWS);
                float cameraClearance = smoothstep(
                    input.beamWidth * 0.55,
                    input.beamWidth * 2.5 + 0.05,
                    cameraDistanceToBeam);
                float cameraFade = lerp(
                    1.0,
                    cameraClearance,
                    saturate(_WeatherLightRayIntensity.z));

                float atmosphere = widthProfile *
                    groundFade *
                    upperFade *
                    input.beamVariation.x *
                    fluctuation *
                    longitudinal *
                    cameraFade *
                    depthFade *
                    saturate(_WeatherLightRayIntensity.x);
                return saturate(atmosphere);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
