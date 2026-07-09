Shader "PS3D/Pixel Ground Surface Lit"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.807, 0.870, 0.906, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        [Header(Pixel Surface)]
        _PixelCellSize("Pixel Cell Size", Range(0.005, 0.5)) = 0.055
        _PixelSeed("Pixel Seed", Float) = 3276
        _PixelToneCount("Pixel Tone Count", Range(2, 8)) = 4.67
        _PixelClusterStrength("Pixel Cluster Strength", Range(0, 1)) = 0.576
        _PixelVariation("Pixel Variation", Range(0, 0.25)) = 0.047
        _PixelVertexVariation("Vertex Variation", Range(0, 0.25)) = 0.221
        _PixelEffectStrength("Pixel Effect Strength", Range(0, 2)) = 1
        _PixelBroadVariation("Broad Variation", Range(0, 0.25)) = 0.022
        _PixelWarpStrength("Cell Warp Strength", Range(0, 2)) = 0.18

        [Header(Ground Surface Response)]
        [Enum(ProgrammaticStylized3D.Rendering.PixelSurfaceMaskDebugMode)]
        _MaskDebugMode("Mask Debug Mode", Float) = 0
        [HideInInspector] _SurfaceContract("Surface Contract", Float) = 1
        _GroundSnowResponse("Ground Snow Response", Range(0, 2)) = 1
        _GroundDampResponse("Ground Damp/Deposit Response", Range(0, 2)) = 1
        _GroundVegetationResponse("Ground Vegetation Response", Range(0, 2)) = 0.25
        _GroundRockyDryResponse("Ground Rocky/Dry Response", Range(0, 2)) = 0.5
        _GroundShoreDampStrength("Ground Shore Damp Strength", Range(0, 2)) = 1
        _GroundPatchBlendStrength("Ground Patch Blend Strength", Range(0, 1)) = 0.55
        _GroundMacroPatchScale("Ground Macro Patch Scale", Range(0.5, 12)) = 4.5
        _GroundSnowTintStrength("Ground Snow Tint Strength", Range(0, 1)) = 0.58
        _GroundSnowBrightness("Ground Snow Brightness", Range(0, 0.5)) = 0.16
        _GroundDampDarkenStrength("Ground Damp Darken Strength", Range(0, 0.75)) = 0.34
        _GroundDampTint("Ground Damp Tint", Color) = (0.47, 0.42, 0.34, 1)
        _GroundDampTintStrength("Ground Damp Tint Strength", Range(0, 1)) = 0.2
        _GroundRockyDryTint("Ground Rocky/Dry Tint", Color) = (0.68, 0.70, 0.68, 1)
        _GroundRockyDryTintStrength("Ground Rocky/Dry Tint Strength", Range(0, 1)) = 0.18
        _GroundVegetationTint("Ground Vegetation Tint", Color) = (0.50, 0.58, 0.42, 1)
        _GroundVegetationTintStrength("Ground Vegetation Tint Strength", Range(0, 1)) = 0.1

        [Header(Ground Surface Features)]
        [HideInInspector] _GroundFeatureMode("Ground Feature Mode", Float) = 0
        [HideInInspector] _GroundFeatureStrength("Ground Feature Strength", Range(0, 1)) = 0
        [HideInInspector] _GroundFeatureScale("Ground Feature Scale", Range(0.1, 30)) = 5
        [HideInInspector] _GroundFeatureContrast("Ground Feature Contrast", Range(0, 1)) = 0.5
        [HideInInspector] _GroundFeatureMaskInfluence("Ground Feature Mask Influence", Range(0, 1)) = 0.5
        [HideInInspector] _GroundFeatureDirection("Ground Feature Direction", Vector) = (1, 0, 0, 0)
        [HideInInspector] _GroundFeatureSeed("Ground Feature Seed", Float) = 0
        [HideInInspector] _GroundDirectionalStreakStrength("Ground Directional Streak Strength", Range(0, 1)) = 0
        [HideInInspector] _GroundDirectionalStreakScale("Ground Directional Streak Scale", Range(0.1, 30)) = 5
        [HideInInspector] _GroundDirectionalStreakContrast("Ground Directional Streak Contrast", Range(0, 1)) = 0.5
        [HideInInspector] _GroundDirectionalStreakMaskInfluence("Ground Directional Streak Mask Influence", Range(0, 1)) = 0.5
        [HideInInspector] _GroundDirectionalStreakDirection("Ground Directional Streak Direction", Vector) = (1, 0, 0, 0)
        [HideInInspector] _GroundDirectionalStreakSeed("Ground Directional Streak Seed", Float) = 0
        [HideInInspector] _GroundPooledWetnessStrength("Ground Pooled Wetness Strength", Range(0, 1)) = 0
        [HideInInspector] _GroundPooledWetnessScale("Ground Pooled Wetness Scale", Range(0.1, 30)) = 5
        [HideInInspector] _GroundPooledWetnessContrast("Ground Pooled Wetness Contrast", Range(0, 1)) = 0.5
        [HideInInspector] _GroundPooledWetnessMaskInfluence("Ground Pooled Wetness Mask Influence", Range(0, 1)) = 0.5
        [HideInInspector] _GroundPooledWetnessDirection("Ground Pooled Wetness Direction", Vector) = (1, 0, 0, 0)
        [HideInInspector] _GroundPooledWetnessSeed("Ground Pooled Wetness Seed", Float) = 0
        [HideInInspector] _GroundTrampledWearStrength("Ground Trampled Wear Strength", Range(0, 1)) = 0
        [HideInInspector] _GroundTrampledWearScale("Ground Trampled Wear Scale", Range(0.1, 30)) = 5
        [HideInInspector] _GroundTrampledWearContrast("Ground Trampled Wear Contrast", Range(0, 1)) = 0.5
        [HideInInspector] _GroundTrampledWearMaskInfluence("Ground Trampled Wear Mask Influence", Range(0, 1)) = 0.5
        [HideInInspector] _GroundTrampledWearDirection("Ground Trampled Wear Direction", Vector) = (1, 0, 0, 0)
        [HideInInspector] _GroundTrampledWearSeed("Ground Trampled Wear Seed", Float) = 0
        [HideInInspector] _GroundPaintedAccentLineStrength("Ground Painted Accent Line Strength", Range(0, 1)) = 0
        [HideInInspector] _GroundPaintedAccentLineScale("Ground Painted Accent Line Scale", Range(0.1, 30)) = 5
        [HideInInspector] _GroundPaintedAccentLineContrast("Ground Painted Accent Line Contrast", Range(0, 1)) = 0.5
        [HideInInspector] _GroundPaintedAccentLineMaskInfluence("Ground Painted Accent Line Mask Influence", Range(0, 1)) = 0.5
        [HideInInspector] _GroundPaintedAccentLineDirection("Ground Painted Accent Line Direction", Vector) = (1, 0, 0, 0)
        [HideInInspector] _GroundPaintedAccentLineSeed("Ground Painted Accent Line Seed", Float) = 0
        [HideInInspector] _GroundPaintedAccentFoldField("Ground Painted Accent Fold Field", 2D) = "black" {}
        [HideInInspector] _GroundPaintedAccentFoldFieldEnabled("Ground Painted Accent Fold Field Enabled", Float) = 0
        [HideInInspector] _GroundPaintedAccentFoldFieldOriginSize("Ground Painted Accent Fold Field Origin Size", Vector) = (0, 0, 1, 1)
        [HideInInspector] _GroundPaintedAccentFoldFieldTexelSize("Ground Painted Accent Fold Field Texel Size", Vector) = (1, 1, 1, 1)

        [Header(Stylized Value Shaping)]
        _HighlightCompressStrength("Highlight Compress Strength", Range(0, 0.5)) = 0.08
        _HighlightCompressStart("Highlight Compress Start", Range(0, 1)) = 0.72
        _BottomDarkenStrength("Bottom Darken Strength", Range(0, 0.5)) = 0.1
        _BottomDarkenHeight("Bottom Darken Height", Range(0.01, 4)) = 0.55
        _EdgeDarkenStrength("Broad Edge Darken Strength", Range(0, 0.5)) = 0.05
        _EdgeDarkenPower("Broad Edge Darken Power", Range(0.5, 8)) = 2.5

        [Header(Material Profile)]
        _ProfileContrast("Profile Contrast", Range(0, 2)) = 1
        _ProfilePixelContrast("Profile Pixel Contrast", Range(0, 2)) = 1
        _Wetness("Wetness", Range(0, 1)) = 0
        _WetDarkenStrength("Wet Darken Strength", Range(0, 0.75)) = 0.22
        _WetPixelSoftening("Wet Pixel Softening", Range(0, 1)) = 0.55
        _WetSmoothnessBoost("Wet Smoothness Boost", Range(0, 1)) = 0.35
        _FrostStrength("Frost Strength", Range(0, 1)) = 0
        _FrostCoverage("Frost Coverage", Range(0, 1)) = 0.45
        _FrostContrast("Frost Contrast", Range(0, 2)) = 1
        _FrostCreviceDarken("Frost Crevice Darken", Range(0, 1)) = 0.22
        _FrostColor("Frost Color", Color) = (0.72, 0.82, 0.88, 1)
        _MonolithicFlatten("Monolithic Flatten", Range(0, 1)) = 0
        _MonolithicSmoothnessBoost("Monolithic Smoothness Boost", Range(0, 1)) = 0.18

        [Header(Lighting)]
        _Smoothness("Smoothness", Range(0, 1)) = 0.2
        _SpecularStrength("Specular Strength", Range(0, 1)) = 0.16
        _LightingTintInfluence("Lighting Tint Influence", Range(0, 1)) = 0.35
        _AmbientStrength("Ambient Strength", Range(0, 2)) = 0.95
        _DirectStrength("Direct Strength", Range(0, 2)) = 1.15
        _DiffuseWrap("Diffuse Wrap", Range(0, 1)) = 0.12
        _ShadowAmbientStrength("Shadow Ambient Strength", Range(0, 1)) = 0.42
        _FlatNormalStrength("Flat Normal Strength", Range(0, 1)) = 0
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
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            #define _SPECULAR_SETUP 1

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Includes/PixelCellVariation.hlsl"
            #include "../Includes/PixelSurfaceGroundMaterialProperties.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_GroundPaintedAccentFoldField);
            SAMPLER(sampler_GroundPaintedAccentFoldField);

            #include "../Includes/PixelSurfaceGroundForwardTypes.hlsl"
            #include "../Includes/PixelSurfaceGroundResponse.hlsl"
            #include "../Includes/PixelSurfaceColorUtility.hlsl"
            #include "../Includes/PixelSurfaceGroundMaskDebug.hlsl"
            #include "../Includes/PixelSurfaceGroundForwardPass.hlsl"
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
            #include "../Includes/PixelSurfaceGroundMaterialProperties.hlsl"

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
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../Includes/PixelSurfaceGroundMaterialProperties.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthNormalsVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS =
                    NormalizeNormalPerVertex(
                        TransformObjectToWorldNormal(input.normalOS));
                return output;
            }

            void DepthNormalsFragment(
                Varyings input,
                out half4 outNormalWS : SV_Target0
                #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
                #endif
            )
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                #if defined(_GBUFFER_NORMALS_OCT)
                    float3 normalWS = normalize(input.normalWS);
                    float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
                    float2 remappedOctNormalWS =
                        saturate(octNormalWS * 0.5 + 0.5);
                    half3 packedNormalWS =
                        PackFloat2To888(remappedOctNormalWS);
                    outNormalWS = half4(packedNormalWS, 0.0);
                #else
                    float3 normalWS = NormalizeNormalPerPixel(input.normalWS);
                    outNormalWS = half4(normalWS, 0.0);
                #endif

                #ifdef _WRITE_RENDERING_LAYERS
                    outRenderingLayers = EncodeMeshRenderingLayer();
                #endif
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
            #include "../Includes/PixelSurfaceGroundMaterialProperties.hlsl"

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

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
