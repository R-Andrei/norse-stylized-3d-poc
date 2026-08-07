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
        _PixelBroadVariation("Broad Variation", Range(0, 0.25)) = 0.022
        _PixelWarpStrength("Cell Warp Strength", Range(0, 2)) = 0.18

        [Header(Semantic Surface Response)]
        _ExposureTintStrength("Exposure Brighten Strength", Range(0, 0.5)) = 0.04
        _CreviceDarkenStrength("Crevice Darken Strength", Range(0, 0.75)) = 0.075
        _BaseDarkenStrength("Base Darken Strength", Range(0, 0.75)) = 0.04

        [Header(Generated Stone Mask Response)]
        _StoneDirtResponse("Stone Dirt/Deposit Response", Range(0, 1)) = 0.25
        _StoneDirtTint("Stone Dirt/Deposit Tint", Color) = (0.42, 0.40, 0.36, 1)

        [Header(Generated Stone Surface Breakup)]
        _StoneMottleStrength("Stone Mottle Strength", Range(0, 1)) = 0.25
        _StoneMottleScale("Stone Mottle Scale", Range(0.2, 2.5)) = 0.9
        _StoneMottleSoftness("Stone Mottle Softness", Range(0, 1)) = 0.6
        _StoneMottleShelterBias("Stone Mottle Shelter Bias", Range(0, 1)) = 0.45

        [Header(Generated Stone Mask Tinting)]
        _GeneratedMassExposureTint("Exposure Tint", Color) = (0.50, 0.50, 0.50, 1)
        _GeneratedMassExposureTintStrength("Exposure Tint Strength", Range(0, 1)) = 0
        _GeneratedMassCreviceTint("Crevice Tint", Color) = (0.50, 0.50, 0.50, 1)
        _GeneratedMassCreviceTintStrength("Crevice Tint Strength", Range(0, 1)) = 0
        _GeneratedMassBaseTint("Base Tint", Color) = (0.50, 0.50, 0.50, 1)
        _GeneratedMassBaseTintStrength("Base Tint Strength", Range(0, 1)) = 0
        _GeneratedMassDirtDepositTint("Dirt Deposit Tint", Color) = (0.50, 0.50, 0.50, 1)
        _GeneratedMassDirtDepositTintStrength("Dirt Deposit Tint Strength", Range(0, 1)) = 0

        [Header(Generated Stone Colour Authority)]
        _GeneratedMassOverallRockTint("Overall Rock Tint", Color) = (0.50, 0.50, 0.50, 1)
        _GeneratedMassOverallRockTintStrength("Overall Rock Tint Strength", Range(0, 1)) = 0
        _GeneratedMassLightingTintInfluence("Lighting Tint Influence", Range(0, 1)) = 0.35
        [HideInInspector] _GeneratedMassSurfaceNormalStrength("Generated Mass Surface Normal Strength", Float) = 0.18
        [HideInInspector] _GeneratedMassSurfaceNormalScale("Generated Mass Surface Normal Scale", Float) = 1.6

        _StoneEdgeWearResponse("Stone Edge Wear Response", Range(0, 1)) = 0.5
        _StoneEdgeWearTint("Stone Edge Wear Tint", Color) = (0.76, 0.74, 0.62, 1)
        _StoneCreaseResponse("Stone Crease Response", Range(0, 1)) = 0.65
        _StoneCreaseTint("Stone Crease Tint", Color) = (0.09, 0.085, 0.075, 1)

        [Enum(ProgrammaticStylized3D.Rendering.PixelSurfaceMaskDebugMode)]
        _MaskDebugMode("Mask Debug Mode", Float) = 0
        [HideInInspector] _SurfaceCausalityMode("Surface Causality Mode", Float) = 0
        [HideInInspector] _SurfaceCausalityLightScale("Surface Causality Light Scale", Float) = 1
        [Enum(GeneratedMass,0,Ground,1)]
        _SurfaceContract("Surface Contract", Float) = 0
        _GroundSnowResponse("Ground Snow Response", Range(0, 2)) = 1
        _GroundDampResponse("Ground Damp/Deposit Response", Range(0, 2)) = 1
        _GroundVegetationResponse("Ground Vegetation Response", Range(0, 2)) = 0.25
        _GroundRockyDryResponse("Ground Rocky/Dry Response", Range(0, 2)) = 0.5
        _GroundShoreDampStrength("Ground Shore Damp Strength", Range(0, 2)) = 1
        [HideInInspector] _GeneratedMassLocalMinY("Generated Mass Local Min Y", Float) = 0
        [HideInInspector] _GeneratedMassLocalHeight("Generated Mass Local Height", Float) = 1
        [HideInInspector] _GeneratedMassMaskSeed("Generated Mass Mask Seed", Float) = 0
        [HideInInspector] _GeneratedMassLocalXZScale("Generated Mass Local XZ Scale", Float) = 1
        [HideInInspector] _GeneratedMassMaskBaseLift("Generated Mass Mask Base Lift", Float) = 0
        [HideInInspector] _GeneratedMassCreviceReach("Generated Mass Crevice Reach", Float) = 1
        [HideInInspector] _GeneratedMassCreviceSmoothness("Generated Mass Crevice Smoothness", Float) = 1
        [HideInInspector] _GeneratedMassCreviceBreakup("Generated Mass Crevice Breakup", Float) = 1
        [HideInInspector] _GeneratedMassDirtCrawlReach("Generated Mass Dirt Crawl Reach", Float) = 1
        [HideInInspector] _GeneratedMassDirtCoverage("Generated Mass Dirt Coverage", Float) = 1
        [HideInInspector] _GeneratedMassExposureResponse("Generated Mass Exposure Response", Float) = 1
        [HideInInspector] _GeneratedMassCreviceResponse("Generated Mass Crevice Response", Float) = 1
        [HideInInspector] _GeneratedMassBaseResponse("Generated Mass Base Response", Float) = 1
        [HideInInspector] _GeneratedMassDirtDepositResponse("Generated Mass Dirt Deposit Response", Float) = 1
        [HideInInspector] _GeneratedMassFeatureAtlas0("Generated Mass Feature Atlas 0", 2D) = "black" {}
        [HideInInspector] _GeneratedMassFeatureAtlas0Enabled("Generated Mass Feature Atlas 0 Enabled", Float) = 0
        [HideInInspector] _GeneratedMassFeatureAtlas1("Generated Mass Feature Atlas 1", 2D) = "black" {}
        [HideInInspector] _GeneratedMassFeatureAtlas1Enabled("Generated Mass Feature Atlas 1 Enabled", Float) = 0
        [HideInInspector] _GeneratedMassFeatureAtlasQuality("Generated Mass Feature Atlas Quality", Float) = 1
        [HideInInspector] _GeneratedMassGeometryEdgeWearEnabled("Generated Mass Geometry Edge Wear Enabled", Float) = 0
        [HideInInspector] _GeneratedMassEdgeWearCoverage("Generated Mass Edge Wear Coverage", Float) = 1
        [HideInInspector] _GeneratedMassEdgeWearSoftness("Generated Mass Edge Wear Softness", Float) = 0.45
        [HideInInspector] _GeneratedMassEdgeWearResponseStrength("Generated Mass Edge Wear Response Strength", Float) = 0
        [HideInInspector] _GeneratedMassEdgeWearBrightnessLift("Generated Mass Edge Wear Brightness Lift", Float) = 0.25
        [HideInInspector] _GeneratedMassEdgeWearTint("Generated Mass Edge Wear Tint", Color) = (0.70, 0.69, 0.62, 1)
        [HideInInspector] _GeneratedMassEdgeWearTintStrength("Generated Mass Edge Wear Tint Strength", Float) = 0
        [HideInInspector] _GeneratedMassEdgeWearMacroVariation("Generated Mass Edge Wear Macro Variation", Float) = 0
        [HideInInspector] _GeneratedMassCreaseLength("Generated Mass Crease Length", Float) = 1
        [HideInInspector] _GeneratedMassCreaseBranching("Generated Mass Crease Branching", Float) = 1
        [HideInInspector] _GeneratedMassCreaseSoftness("Generated Mass Crease Softness", Float) = 0.35

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
            #pragma shader_feature_local_fragment _SURFACE_CAUSALITY_AUDIT
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _LIGHT_COOKIES

            #define _SPECULAR_SETUP 1

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
                float _PixelBroadVariation;
                float _PixelWarpStrength;
                float _ExposureTintStrength;
                float _CreviceDarkenStrength;
                float _BaseDarkenStrength;
                float _StoneDirtResponse;
                half4 _StoneDirtTint;
                float _StoneMottleStrength;
                float _StoneMottleScale;
                float _StoneMottleSoftness;
                float _StoneMottleShelterBias;
                half4 _GeneratedMassExposureTint;
                float _GeneratedMassExposureTintStrength;
                half4 _GeneratedMassCreviceTint;
                float _GeneratedMassCreviceTintStrength;
                half4 _GeneratedMassBaseTint;
                float _GeneratedMassBaseTintStrength;
                half4 _GeneratedMassDirtDepositTint;
                float _GeneratedMassDirtDepositTintStrength;
                half4 _GeneratedMassOverallRockTint;
                float _GeneratedMassOverallRockTintStrength;
                float _GeneratedMassLightingTintInfluence;
                float _GeneratedMassSurfaceNormalStrength;
                float _GeneratedMassSurfaceNormalScale;
                float _StoneEdgeWearResponse;
                half4 _StoneEdgeWearTint;
                float _StoneCreaseResponse;
                half4 _StoneCreaseTint;
                float _MaskDebugMode;
                float _SurfaceCausalityMode;
                float _SurfaceCausalityLightScale;
                float _SurfaceContract;
                float _GroundSnowResponse;
                float _GroundDampResponse;
                float _GroundVegetationResponse;
                float _GroundRockyDryResponse;
                float _GroundShoreDampStrength;
                float _GeneratedMassLocalMinY;
                float _GeneratedMassLocalHeight;
                float _GeneratedMassMaskSeed;
                float _GeneratedMassLocalXZScale;
                float _GeneratedMassMaskBaseLift;
                float _GeneratedMassCreviceReach;
                float _GeneratedMassCreviceSmoothness;
                float _GeneratedMassCreviceBreakup;
                float _GeneratedMassDirtCrawlReach;
                float _GeneratedMassDirtCoverage;
                float _GeneratedMassExposureResponse;
                float _GeneratedMassCreviceResponse;
                float _GeneratedMassBaseResponse;
                float _GeneratedMassDirtDepositResponse;
                float _GeneratedMassFeatureAtlas0Enabled;
                float _GeneratedMassFeatureAtlas1Enabled;
                float _GeneratedMassFeatureAtlasQuality;
                float _GeneratedMassGeometryEdgeWearEnabled;
                float _GeneratedMassEdgeWearCoverage;
                float _GeneratedMassEdgeWearSoftness;
                float _GeneratedMassEdgeWearResponseStrength;
                float _GeneratedMassEdgeWearBrightnessLift;
                half4 _GeneratedMassEdgeWearTint;
                float _GeneratedMassEdgeWearTintStrength;
                float _GeneratedMassEdgeWearMacroVariation;
                float _GeneratedMassCreaseLength;
                float _GeneratedMassCreaseBranching;
                float _GeneratedMassCreaseSoftness;
                float _HighlightCompressStrength;
                float _HighlightCompressStart;
                float _BottomDarkenStrength;
                float _BottomDarkenHeight;
                float _EdgeDarkenStrength;
                float _EdgeDarkenPower;
                float _ProfileContrast;
                float _ProfilePixelContrast;
                float _Wetness;
                float _WetDarkenStrength;
                float _WetPixelSoftening;
                float _WetSmoothnessBoost;
                float _FrostStrength;
                float _FrostCoverage;
                float _FrostContrast;
                float _FrostCreviceDarken;
                half4 _FrostColor;
                float _MonolithicFlatten;
                float _MonolithicSmoothnessBoost;
                float _Smoothness;
                float _SpecularStrength;
                float _AmbientStrength;
                float _DirectStrength;
                float _DiffuseWrap;
                float _ShadowAmbientStrength;
                float _FlatNormalStrength;
                float _ReceiveShadows;
                float _Cull;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_GeneratedMassFeatureAtlas0);
            SAMPLER(sampler_GeneratedMassFeatureAtlas0);
            TEXTURE2D(_GeneratedMassFeatureAtlas1);
            SAMPLER(sampler_GeneratedMassFeatureAtlas1);

            #include "../Includes/PixelSurfaceForwardTypes.hlsl"
            #include "../Includes/PixelSurfaceGeneratedMassCore.hlsl"
            #include "../Includes/PixelSurfaceGroundResponse.hlsl"
            #include "../Includes/PixelSurfaceColorUtility.hlsl"
            #include "../Includes/PixelSurfaceGeneratedMassFeatures.hlsl"
            #include "../Includes/PixelSurfaceMaskDebug.hlsl"
            #include "../Includes/PixelSurfaceForwardPass.hlsl"
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
                float _PixelBroadVariation;
                float _PixelWarpStrength;
                float _ExposureTintStrength;
                float _CreviceDarkenStrength;
                float _BaseDarkenStrength;
                float _StoneDirtResponse;
                half4 _StoneDirtTint;
                float _StoneMottleStrength;
                float _StoneMottleScale;
                float _StoneMottleSoftness;
                float _StoneMottleShelterBias;
                float _StoneEdgeWearResponse;
                half4 _StoneEdgeWearTint;
                float _StoneCreaseResponse;
                half4 _StoneCreaseTint;
                float _MaskDebugMode;
                float _SurfaceCausalityMode;
                float _SurfaceCausalityLightScale;
                float _SurfaceContract;
                float _GroundSnowResponse;
                float _GroundDampResponse;
                float _GroundVegetationResponse;
                float _GroundRockyDryResponse;
                float _GroundShoreDampStrength;
                float _GeneratedMassLocalMinY;
                float _GeneratedMassLocalHeight;
                float _GeneratedMassMaskSeed;
                float _GeneratedMassLocalXZScale;
                float _GeneratedMassMaskBaseLift;
                float _GeneratedMassCreviceReach;
                float _GeneratedMassCreviceSmoothness;
                float _GeneratedMassCreviceBreakup;
                float _GeneratedMassDirtCrawlReach;
                float _GeneratedMassDirtCoverage;
                float _GeneratedMassFeatureAtlas0Enabled;
                float _GeneratedMassFeatureAtlas1Enabled;
                float _GeneratedMassFeatureAtlasQuality;
                float _GeneratedMassGeometryEdgeWearEnabled;
                float _GeneratedMassEdgeWearCoverage;
                float _GeneratedMassEdgeWearSoftness;
                float _GeneratedMassEdgeWearResponseStrength;
                float _GeneratedMassEdgeWearBrightnessLift;
                half4 _GeneratedMassEdgeWearTint;
                float _GeneratedMassEdgeWearTintStrength;
                float _GeneratedMassEdgeWearMacroVariation;
                float _GeneratedMassCreaseLength;
                float _GeneratedMassCreaseBranching;
                float _GeneratedMassCreaseSoftness;
                float _HighlightCompressStrength;
                float _HighlightCompressStart;
                float _BottomDarkenStrength;
                float _BottomDarkenHeight;
                float _EdgeDarkenStrength;
                float _EdgeDarkenPower;
                float _ProfileContrast;
                float _ProfilePixelContrast;
                float _Wetness;
                float _WetDarkenStrength;
                float _WetPixelSoftening;
                float _WetSmoothnessBoost;
                float _FrostStrength;
                float _FrostCoverage;
                float _FrostContrast;
                float _FrostCreviceDarken;
                half4 _FrostColor;
                float _MonolithicFlatten;
                float _MonolithicSmoothnessBoost;
                float _Smoothness;
                float _SpecularStrength;
                float _AmbientStrength;
                float _DirectStrength;
                float _DiffuseWrap;
                float _ShadowAmbientStrength;
                float _FlatNormalStrength;
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
                float _PixelBroadVariation;
                float _PixelWarpStrength;
                float _ExposureTintStrength;
                float _CreviceDarkenStrength;
                float _BaseDarkenStrength;
                float _StoneDirtResponse;
                half4 _StoneDirtTint;
                float _StoneMottleStrength;
                float _StoneMottleScale;
                float _StoneMottleSoftness;
                float _StoneMottleShelterBias;
                float _StoneEdgeWearResponse;
                half4 _StoneEdgeWearTint;
                float _StoneCreaseResponse;
                half4 _StoneCreaseTint;
                float _MaskDebugMode;
                float _SurfaceCausalityMode;
                float _SurfaceCausalityLightScale;
                float _SurfaceContract;
                float _GroundSnowResponse;
                float _GroundDampResponse;
                float _GroundVegetationResponse;
                float _GroundRockyDryResponse;
                float _GroundShoreDampStrength;
                float _GeneratedMassLocalMinY;
                float _GeneratedMassLocalHeight;
                float _GeneratedMassMaskSeed;
                float _GeneratedMassLocalXZScale;
                float _GeneratedMassMaskBaseLift;
                float _GeneratedMassCreviceReach;
                float _GeneratedMassCreviceSmoothness;
                float _GeneratedMassCreviceBreakup;
                float _GeneratedMassDirtCrawlReach;
                float _GeneratedMassDirtCoverage;
                float _GeneratedMassFeatureAtlas0Enabled;
                float _GeneratedMassFeatureAtlas1Enabled;
                float _GeneratedMassFeatureAtlasQuality;
                float _GeneratedMassGeometryEdgeWearEnabled;
                float _GeneratedMassEdgeWearCoverage;
                float _GeneratedMassEdgeWearSoftness;
                float _GeneratedMassEdgeWearResponseStrength;
                float _GeneratedMassEdgeWearBrightnessLift;
                half4 _GeneratedMassEdgeWearTint;
                float _GeneratedMassEdgeWearTintStrength;
                float _GeneratedMassEdgeWearMacroVariation;
                float _GeneratedMassCreaseLength;
                float _GeneratedMassCreaseBranching;
                float _GeneratedMassCreaseSoftness;
                float _HighlightCompressStrength;
                float _HighlightCompressStart;
                float _BottomDarkenStrength;
                float _BottomDarkenHeight;
                float _EdgeDarkenStrength;
                float _EdgeDarkenPower;
                float _ProfileContrast;
                float _ProfilePixelContrast;
                float _Wetness;
                float _WetDarkenStrength;
                float _WetPixelSoftening;
                float _WetSmoothnessBoost;
                float _FrostStrength;
                float _FrostCoverage;
                float _FrostContrast;
                float _FrostCreviceDarken;
                half4 _FrostColor;
                float _MonolithicFlatten;
                float _MonolithicSmoothnessBoost;
                float _Smoothness;
                float _SpecularStrength;
                float _AmbientStrength;
                float _DirectStrength;
                float _DiffuseWrap;
                float _ShadowAmbientStrength;
                float _FlatNormalStrength;
                float _ReceiveShadows;
                float _Cull;
            CBUFFER_END

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
                float _PixelBroadVariation;
                float _PixelWarpStrength;
                float _ExposureTintStrength;
                float _CreviceDarkenStrength;
                float _BaseDarkenStrength;
                float _StoneDirtResponse;
                half4 _StoneDirtTint;
                float _StoneMottleStrength;
                float _StoneMottleScale;
                float _StoneMottleSoftness;
                float _StoneMottleShelterBias;
                float _StoneEdgeWearResponse;
                half4 _StoneEdgeWearTint;
                float _StoneCreaseResponse;
                half4 _StoneCreaseTint;
                float _MaskDebugMode;
                float _SurfaceCausalityMode;
                float _SurfaceCausalityLightScale;
                float _SurfaceContract;
                float _GroundSnowResponse;
                float _GroundDampResponse;
                float _GroundVegetationResponse;
                float _GroundRockyDryResponse;
                float _GroundShoreDampStrength;
                float _GeneratedMassLocalMinY;
                float _GeneratedMassLocalHeight;
                float _GeneratedMassMaskSeed;
                float _GeneratedMassLocalXZScale;
                float _GeneratedMassMaskBaseLift;
                float _GeneratedMassCreviceReach;
                float _GeneratedMassCreviceSmoothness;
                float _GeneratedMassCreviceBreakup;
                float _GeneratedMassDirtCrawlReach;
                float _GeneratedMassDirtCoverage;
                float _GeneratedMassFeatureAtlas0Enabled;
                float _GeneratedMassFeatureAtlas1Enabled;
                float _GeneratedMassFeatureAtlasQuality;
                float _GeneratedMassGeometryEdgeWearEnabled;
                float _GeneratedMassEdgeWearCoverage;
                float _GeneratedMassEdgeWearSoftness;
                float _GeneratedMassEdgeWearResponseStrength;
                float _GeneratedMassEdgeWearBrightnessLift;
                half4 _GeneratedMassEdgeWearTint;
                float _GeneratedMassEdgeWearTintStrength;
                float _GeneratedMassEdgeWearMacroVariation;
                float _GeneratedMassCreaseLength;
                float _GeneratedMassCreaseBranching;
                float _GeneratedMassCreaseSoftness;
                float _HighlightCompressStrength;
                float _HighlightCompressStart;
                float _BottomDarkenStrength;
                float _BottomDarkenHeight;
                float _EdgeDarkenStrength;
                float _EdgeDarkenPower;
                float _ProfileContrast;
                float _ProfilePixelContrast;
                float _Wetness;
                float _WetDarkenStrength;
                float _WetPixelSoftening;
                float _WetSmoothnessBoost;
                float _FrostStrength;
                float _FrostCoverage;
                float _FrostContrast;
                float _FrostCreviceDarken;
                half4 _FrostColor;
                float _MonolithicFlatten;
                float _MonolithicSmoothnessBoost;
                float _Smoothness;
                float _SpecularStrength;
                float _AmbientStrength;
                float _DirectStrength;
                float _DiffuseWrap;
                float _ShadowAmbientStrength;
                float _FlatNormalStrength;
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
