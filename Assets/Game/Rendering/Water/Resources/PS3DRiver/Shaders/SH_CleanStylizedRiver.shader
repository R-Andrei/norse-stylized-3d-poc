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

        [HideInInspector]
        _DomainFallbackDepth("Domain Fallback Depth", Float) = 1.1

        [Header(Body Validation)]
        _BodyDebugView("Body Debug View", Range(0, 7)) = 0
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Includes/RiverWaterCommon.hlsl"
            #include "Includes/RiverWaterDepth.hlsl"
            #include "Includes/RiverWaterBody.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                float _Clarity;
                float _BodyDepthRange;
                float _BodyDepthContrast;
                float _WaterTintStrength;
                float _SurfacePresence;
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

                RiverWaterBodyResult body = RiverWaterComposeBody(
                    sceneColour,
                    _ShallowColor.rgb,
                    _DeepColor.rgb,
                    depthData,
                    _WaterTintStrength,
                    _SurfacePresence,
                    viewFacing);

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

                return half4(saturate(finalColour), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
