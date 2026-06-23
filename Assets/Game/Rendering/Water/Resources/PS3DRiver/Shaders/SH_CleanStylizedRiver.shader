Shader "PS3D/Stylized River Water"
{
    Properties
    {
        [Header(Water Body)]
        _ShallowColor("Shallow Colour", Color) = (0.458, 0.802, 0.798, 1)
        _DeepColor("Deep Colour", Color) = (0.0, 0.310, 0.594, 1)
        _Clarity("Clarity", Range(0, 1)) = 0.62
        _BodyDepthRange("Depth Range", Range(0.1, 8)) = 1.4
        _BodyDepthContrast("Depth Contrast", Range(0, 1)) = 0.5
        _WaterTintStrength("Water Tint Strength", Range(0, 1)) = 0.72
        _SurfacePresence("Surface Presence", Range(0, 1)) = 0.46

        [Header(Surface State)]
        _FreezeAmount("Freeze Amount", Range(0, 1)) = 0

        [Header(Frozen Body)]
        _IceColor("Ice Colour", Color) = (0.56, 0.78, 0.90, 1)
        _IceTransmission("Ice Transmission", Range(0, 1)) = 0.16
        _IceThickness("Ice Thickness", Range(0, 1)) = 0.72
        _IceCloudiness("Ice Cloudiness", Range(0, 1)) = 0.58
        _IceSurfacePresence("Ice Surface Presence", Range(0, 1)) = 0.86
        _IceScattering("Ice Scattering", Range(0, 1)) = 0.68

        [Header(Lighting Response)]
        _LightDependence("Light Dependence", Range(0, 1)) = 1
        _AmbientResponse("Ambient Response", Range(0, 2)) = 1
        _SunResponse("Sun Response", Range(0, 2)) = 1
        _LocalLightResponse("Local Light Response", Range(0, 3)) = 1
        _LightColorInfluence("Light Colour Influence", Range(0, 1)) = 0.8
        _MinimumNightVisibility("Minimum Night Visibility", Range(0, 0.5)) = 0.025
        _ShadowResponse("Shadow Response", Range(0, 1)) = 1
        _DiffuseWrap("Diffuse Wrap", Range(0, 1)) = 0.22

        [HideInInspector]
        _DomainFallbackDepth("Domain Fallback Depth", Float) = 1.1

        [Header(Body Validation)]
        _BodyDebugView("Body Debug View", Range(0, 12)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Transparent-10"
        }

        Pass
        {
            Name "ForwardWaterBody"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On
            ZTest LEqual
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Includes/RiverWaterCommon.hlsl"
            #include "Includes/RiverWaterDepth.hlsl"
            #include "Includes/RiverWaterLighting.hlsl"
            #include "Includes/RiverWaterBody.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                float _Clarity;
                float _BodyDepthRange;
                float _BodyDepthContrast;
                float _WaterTintStrength;
                float _SurfacePresence;

                float _FreezeAmount;
                half4 _IceColor;
                float _IceTransmission;
                float _IceThickness;
                float _IceCloudiness;
                float _IceSurfacePresence;
                float _IceScattering;

                float _LightDependence;
                float _AmbientResponse;
                float _SunResponse;
                float _LocalLightResponse;
                float _LightColorInfluence;
                float _MinimumNightVisibility;
                float _ShadowResponse;
                float _DiffuseWrap;

                float _DomainFallbackDepth;
                float _BodyDebugView;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 baseNormalWS : TEXCOORD1;
                float localDistance : TEXCOORD2;
                float globalDistance : TEXCOORD3;
                float lateralMetres : TEXCOORD4;
                half fogFactor : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.baseNormalWS = normalize(
                    TransformObjectToWorldNormal(input.normalOS));
                output.localDistance = input.uv0.y;
                output.globalDistance = input.uv1.x;
                output.lateralMetres = input.uv1.y;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUV = GetNormalizedScreenSpaceUV(
                    input.positionCS);

                RiverWaterSurfaceInputs surfaceInputs;
                surfaceInputs.positionWS = input.positionWS;
                surfaceInputs.baseNormalWS = normalize(input.baseNormalWS);
                surfaceInputs.localDistance = input.localDistance;
                surfaceInputs.globalDistance = input.globalDistance;
                surfaceInputs.lateralMetres = input.lateralMetres;

                // Later stages populate this structure. Stage 2 deliberately
                // supplies neutral values so the body contract does not need
                // to be refactored when motion, refraction, foam, and
                // reflections are introduced.
                RiverWaterIntegrationInputs integration =
                    RiverWaterCreateEmptyIntegration(
                        surfaceInputs.baseNormalWS);

                float2 backgroundUV = saturate(
                    screenUV + integration.refractionOffset);
                float3 sceneColour = SampleSceneColor(backgroundUV);

                RiverWaterDepthData depthData = RiverWaterEvaluateDepth(
                    screenUV,
                    surfaceInputs.positionWS,
                    _DomainFallbackDepth,
                    _BodyDepthRange,
                    _BodyDepthContrast,
                    _Clarity);

                float3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(
                    surfaceInputs.positionWS);
                float viewFacing = saturate(dot(
                    surfaceInputs.baseNormalWS,
                    viewDirectionWS));

                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = surfaceInputs.positionWS;
                lightingInput.normalWS = integration.surfaceNormalWS;
                lightingInput.viewDirectionWS = viewDirectionWS;
                lightingInput.shadowCoord = TransformWorldToShadowCoord(
                    surfaceInputs.positionWS);
                lightingInput.normalizedScreenSpaceUV = screenUV;

                RiverWaterLightingResult lighting =
                    RiverWaterEvaluateLighting(
                        lightingInput,
                        _AmbientResponse,
                        _SunResponse,
                        _LocalLightResponse,
                        _LightColorInfluence,
                        _ShadowResponse,
                        _DiffuseWrap);

                float3 bodyLighting = RiverWaterResolveBodyLighting(
                    lighting,
                    _LightDependence,
                    _MinimumNightVisibility);

                RiverWaterBodyResult body = RiverWaterComposeBody(
                    sceneColour,
                    _ShallowColor.rgb,
                    _DeepColor.rgb,
                    depthData,
                    _WaterTintStrength,
                    _SurfacePresence,
                    viewFacing,
                    _FreezeAmount,
                    _IceColor.rgb,
                    _IceTransmission,
                    _IceThickness,
                    _IceCloudiness,
                    _IceSurfacePresence,
                    _IceScattering,
                    bodyLighting);

                float3 finalColour = RiverWaterApplyReservedIntegration(
                    body.colour,
                    integration);
                finalColour = MixFog(finalColour, input.fogFactor);

                int debugMode = (int)round(_BodyDebugView);

                if (debugMode == 1)
                {
                    return half4(depthData.normalizedDepth.xxx, 1.0);
                }

                if (debugMode == 2)
                {
                    return half4(depthData.depthBlend.xxx, 1.0);
                }

                if (debugMode == 3)
                {
                    return half4(depthData.transmission.xxx, 1.0);
                }

                if (debugMode == 4)
                {
                    return half4(body.coverage.xxx, 1.0);
                }

                if (debugMode == 5)
                {
                    return half4(sceneColour, 1.0);
                }

                if (debugMode == 6)
                {
                    return half4(depthData.validSceneDepth.xxx, 1.0);
                }

                if (debugMode == 7)
                {
                    return half4(body.surfaceCoverage.xxx, 1.0);
                }

                if (debugMode == 8)
                {
                    return half4(lighting.combined, 1.0);
                }

                if (debugMode == 9)
                {
                    return half4(lighting.ambient, 1.0);
                }

                if (debugMode == 10)
                {
                    return half4(lighting.sun, 1.0);
                }

                if (debugMode == 11)
                {
                    return half4(lighting.localLights, 1.0);
                }

                if (debugMode == 12)
                {
                    return half4(body.freezeAmount.xxx, 1.0);
                }

                return half4(max(finalColour, 0.0), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
