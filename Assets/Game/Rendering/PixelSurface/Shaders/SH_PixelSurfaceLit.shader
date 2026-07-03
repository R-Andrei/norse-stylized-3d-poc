Shader "PS3D/Pixel Surface Lit"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.334, 0.341, 0.349, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        [Header(Pixel Surface)]
        _PixelCellSize("Pixel Cell Size", Range(0.005, 0.5)) = 0.058
        _PixelSeed("Pixel Seed", Float) = 906
        _PixelToneCount("Pixel Tone Count", Range(2, 8)) = 3.89
        _PixelClusterStrength("Pixel Cluster Strength", Range(0, 1)) = 0.591
        _PixelVariation("Pixel Variation", Range(0, 0.25)) = 0.057
        _PixelVertexVariation("Vertex Variation", Range(0, 0.25)) = 0.09
        _PixelEffectStrength("Pixel Effect Strength", Range(0, 2)) = 1

        [Header(Lighting)]
        _Smoothness("Smoothness", Range(0, 1)) = 0.2
        _SpecularStrength("Specular Strength", Range(0, 1)) = 0.16
        _AmbientStrength("Ambient Strength", Range(0, 2)) = 1
        [Toggle] _ReceiveShadows("Receive Shadows", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
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

            Cull [_Cull]
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
            #include "../Includes/PixelCellVariation.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _PixelCellSize;
                float _PixelSeed;
                float _PixelToneCount;
                float _PixelClusterStrength;
                float _PixelVariation;
                float _PixelVertexVariation;
                float _PixelEffectStrength;
                float _Smoothness;
                float _SpecularStrength;
                float _AmbientStrength;
                float _ReceiveShadows;
                float _Cull;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half4 color : COLOR;
                half fogFactor : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalize(normalInputs.normalWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half3 ResolvePixelSurfaceColor(Varyings input)
            {
                half4 baseSample =
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                float pixelVariation;
                PixelCellVariation_float(
                    input.positionWS,
                    _PixelCellSize,
                    _PixelSeed,
                    _PixelToneCount,
                    _PixelClusterStrength,
                    pixelVariation);

                float vertexVariation = ((float)input.color.r - 0.5) * 2.0;
                float tonalOffset =
                    pixelVariation * _PixelVariation +
                    vertexVariation * _PixelVertexVariation;
                half tonalScale =
                    (half)max(0.0, 1.0 + tonalOffset * _PixelEffectStrength);

                return baseSample.rgb * _BaseColor.rgb * tonalScale;
            }

            half3 ResolveLighting(
                half3 albedo,
                half3 normalWS,
                float3 positionWS)
            {
                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half mainShadow = lerp(
                    (half)1.0,
                    mainLight.shadowAttenuation,
                    saturate((half)_ReceiveShadows));
                half mainNdotL = saturate(dot(normalWS, mainLight.direction));

                half3 lighting =
                    SampleSH(normalWS) * (half)_AmbientStrength;
                lighting +=
                    mainLight.color *
                    mainNdotL *
                    mainLight.distanceAttenuation *
                    mainShadow;

                half specularPower =
                    lerp((half)8.0, (half)96.0, saturate((half)_Smoothness));
                half3 viewDirectionWS =
                    SafeNormalize(GetWorldSpaceViewDir(positionWS));
                half3 halfDirection =
                    SafeNormalize(mainLight.direction + viewDirectionWS);
                half specular =
                    pow(saturate(dot(normalWS, halfDirection)), specularPower) *
                    (half)_SpecularStrength *
                    mainLight.distanceAttenuation *
                    mainShadow;

                #if defined(_ADDITIONAL_LIGHTS)
                uint lightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(lightCount)
                    Light light = GetAdditionalLight(
                        lightIndex,
                        positionWS,
                        half4(1.0, 1.0, 1.0, 1.0));
                    half shadow = lerp(
                        (half)1.0,
                        light.shadowAttenuation,
                        saturate((half)_ReceiveShadows));
                    half ndotl = saturate(dot(normalWS, light.direction));
                    lighting +=
                        light.color *
                        ndotl *
                        light.distanceAttenuation *
                        shadow;

                    half3 additionalHalfDirection =
                        SafeNormalize(light.direction + viewDirectionWS);
                    specular +=
                        pow(
                            saturate(dot(normalWS, additionalHalfDirection)),
                            specularPower) *
                        (half)_SpecularStrength *
                        light.distanceAttenuation *
                        shadow;
                LIGHT_LOOP_END
                #endif

                return albedo * lighting + specular;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half3 normalWS = normalize(input.normalWS);
                half3 albedo = ResolvePixelSurfaceColor(input);
                half3 color =
                    ResolveLighting(albedo, normalWS, input.positionWS);
                color = MixFog(color, input.fogFactor);

                return half4(color, _BaseColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _PixelCellSize;
                float _PixelSeed;
                float _PixelToneCount;
                float _PixelClusterStrength;
                float _PixelVariation;
                float _PixelVertexVariation;
                float _PixelEffectStrength;
                float _Smoothness;
                float _SpecularStrength;
                float _AmbientStrength;
                float _ReceiveShadows;
                float _Cull;
            CBUFFER_END

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS =
                    TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS =
                    TransformObjectToWorldNormal(input.normalOS);

                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 lightDirectionWS =
                        SafeNormalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(
                    ApplyShadowBias(
                        positionWS,
                        normalWS,
                        lightDirectionWS));

                #if UNITY_REVERSED_Z
                    positionCS.z =
                        min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z =
                        max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _PixelCellSize;
                float _PixelSeed;
                float _PixelToneCount;
                float _PixelClusterStrength;
                float _PixelVariation;
                float _PixelVertexVariation;
                float _PixelEffectStrength;
                float _Smoothness;
                float _SpecularStrength;
                float _AmbientStrength;
                float _ReceiveShadows;
                float _Cull;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
