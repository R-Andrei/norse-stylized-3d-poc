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

        _StoneEdgeWearResponse("Stone Edge Wear Response", Range(0, 1)) = 0.5
        _StoneEdgeWearTint("Stone Edge Wear Tint", Color) = (0.76, 0.74, 0.62, 1)
        _StoneCreaseResponse("Stone Crease Response", Range(0, 1)) = 0.65
        _StoneCreaseTint("Stone Crease Tint", Color) = (0.09, 0.085, 0.075, 1)

        [Enum(ProgrammaticStylized3D.Rendering.PixelSurfaceMaskDebugMode)]
        _MaskDebugMode("Mask Debug Mode", Float) = 0
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
        [HideInInspector] _GeneratedMassEdgeWearCoverage("Generated Mass Edge Wear Coverage", Float) = 1
        [HideInInspector] _GeneratedMassEdgeWearSoftness("Generated Mass Edge Wear Softness", Float) = 0.45
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
                float _StoneEdgeWearResponse;
                half4 _StoneEdgeWearTint;
                float _StoneCreaseResponse;
                half4 _StoneCreaseTint;
                float _MaskDebugMode;
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
                float _GeneratedMassEdgeWearCoverage;
                float _GeneratedMassEdgeWearSoftness;
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

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD2;
                float2 featureAtlasUV : TEXCOORD3;
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
                float3 positionOS : TEXCOORD4;
                float4 materialMasks : TEXCOORD5;
                half3 normalOS : TEXCOORD6;
                float2 featureAtlasUV : TEXCOORD7;
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
                output.positionOS = input.positionOS.xyz;
                output.materialMasks = input.uv2;
                output.normalOS = normalize(input.normalOS);
                output.featureAtlasUV = input.featureAtlasUV;
                return output;
            }

            float ResolveGeneratedMassHeight01(Varyings input)
            {
                float height = max(0.0001, _GeneratedMassLocalHeight);
                float rawHeight01 = saturate(
                    (input.positionOS.y - _GeneratedMassLocalMinY) /
                    height);
                float baseLift = saturate(_GeneratedMassMaskBaseLift);
                return saturate(
                    (rawHeight01 - baseLift) /
                    max(0.0001, 1.0 - baseLift));
            }

            float ResolveNotUpwardMask(Varyings input)
            {
                float normalY = normalize((float3)input.normalOS).y;
                return 1.0 - smoothstep(0.18, 0.78, normalY);
            }

            float3 ResolveGeneratedMassMaskCoordinate(
                Varyings input,
                float scale,
                float offset)
            {
                float height = max(0.0001, _GeneratedMassLocalHeight);
                float xzScale = max(0.0001, _GeneratedMassLocalXZScale);
                float3 normalizedPosition =
                    float3(
                        input.positionOS.x / xzScale,
                        (input.positionOS.y - _GeneratedMassLocalMinY) / height,
                        input.positionOS.z / xzScale);

                return normalizedPosition * scale +
                    float3(
                        _GeneratedMassMaskSeed * 0.017 + offset,
                        _GeneratedMassMaskSeed * 0.011 - offset * 0.37,
                        _GeneratedMassMaskSeed * 0.019 + offset * 0.61);
            }

            float ResolveGeneratedMassPatchNoise(
                Varyings input,
                float scale,
                float offset)
            {
                float3 coordinate =
                    ResolveGeneratedMassMaskCoordinate(input, scale, offset);

                float broad = PS3D_ValueNoise31(coordinate);
                float detail = PS3D_ValueNoise31(coordinate * 2.23 + 17.31);
                return saturate(broad * 0.68 + detail * 0.32);
            }

            float ResolveGeneratedMassSoftPatchNoise(
                Varyings input,
                float scale,
                float offset)
            {
                float3 coordinate =
                    ResolveGeneratedMassMaskCoordinate(input, scale, offset);

                float a = PS3D_ValueNoise31(coordinate);
                float b = PS3D_ValueNoise31(coordinate * 1.71 + 9.73);
                float c = PS3D_ValueNoise31(coordinate * 3.11 + 27.19);
                return saturate(a * 0.52 + b * 0.33 + c * 0.15);
            }

            float ResolveGeneratedMassTallnessFactor()
            {
                float height = max(0.0001, _GeneratedMassLocalHeight);
                float xzScale = max(0.0001, _GeneratedMassLocalXZScale);
                return saturate((height / xzScale - 0.65) * 0.58);
            }

            float ResolveGeneratedMassSizeFactor()
            {
                return saturate((_GeneratedMassLocalHeight - 0.75) * 0.16);
            }

            float ResolveGeneratedMassOrganicLowerFade(Varyings input)
            {
                float height01 = ResolveGeneratedMassHeight01(input);
                float3 normalOS = normalize((float3)input.normalOS);
                float tallness = ResolveGeneratedMassTallnessFactor();
                float sizeFactor = ResolveGeneratedMassSizeFactor();
                float creviceReach = max(0.05, _GeneratedMassCreviceReach);
                float creviceSmoothness = max(0.05, _GeneratedMassCreviceSmoothness);
                float creviceBreakup = max(0.05, _GeneratedMassCreviceBreakup);
                float reach01 = saturate((creviceReach - 0.25) / 1.75);
                float smoothness01 = saturate((creviceSmoothness - 0.25) / 1.75);
                float breakup01 = saturate((creviceBreakup - 0.25) / 1.75);
                float xzScale = max(0.0001, _GeneratedMassLocalXZScale);
                float2 normalizedXZ = input.positionOS.xz / xzScale;
                float seed = _GeneratedMassMaskSeed;

                // Footprint lobes are useful for broad object-level variation, but
                // they cannot be the only field; on large side faces they can leave
                // a whole face inside one inactive footprint island.
                float footprintWarpA = PS3D_ValueNoise31(float3(
                    normalizedXZ * 0.45 + float2(seed * 0.007, seed * -0.013),
                    seed * 0.019));
                float footprintWarpB = PS3D_ValueNoise31(float3(
                    normalizedXZ.yx * 0.49 + float2(seed * -0.011, seed * 0.017),
                    seed * 0.023 + 11.7));
                float2 warpedXZ = normalizedXZ +
                    (float2(footprintWarpA, footprintWarpB) - 0.5) *
                    lerp(0.12, 0.42, breakup01);

                float footprintWaveA = sin(warpedXZ.x * 2.35 + seed * 0.071);
                float footprintWaveB = sin(warpedXZ.y * 2.05 + seed * 0.053 + 1.37);
                float footprintWaveC = sin(
                    (warpedXZ.x * 0.62 + warpedXZ.y * 0.48) * 3.35 +
                    seed * 0.093 - 0.48);
                float footprintWave =
                    (footprintWaveA * 0.36 +
                     footprintWaveB * 0.34 +
                     footprintWaveC * 0.30) * 0.5 + 0.5;

                float broadNoise =
                    ResolveGeneratedMassSoftPatchNoise(input, 0.42, 47.0);
                float lobeNoise =
                    ResolveGeneratedMassSoftPatchNoise(input, 0.72, 83.0);
                float patchNoise =
                    ResolveGeneratedMassSoftPatchNoise(input, 1.18, 119.0);
                float planeNoise =
                    ResolveGeneratedMassPatchNoise(input, 1.82, 29.0);
                float facetNoise = saturate(
                    (float)input.color.r * 0.28 +
                    broadNoise * 0.24 +
                    lobeNoise * 0.22 +
                    patchNoise * 0.16 +
                    planeNoise * 0.10);

                // Side-surface lobes: choose the useful width coordinate for the
                // current side face. Z-facing faces vary along local X; X-facing
                // faces vary along local Z. Each dominant side receives a separate
                // phase so the back/side faces do not mirror or disappear together.
                float dominantX = step(abs(normalOS.z), abs(normalOS.x));
                float sideWidthCoord = lerp(normalizedXZ.x, normalizedXZ.y, dominantX);
                float positiveSide = lerp(step(0.0, normalOS.z), step(0.0, normalOS.x), dominantX);
                float sidePhase = lerp(
                    lerp(23.7, 41.9, positiveSide),
                    lerp(67.3, 89.1, positiveSide),
                    dominantX);
                float sideWarp = PS3D_ValueNoise31(float3(
                    sideWidthCoord * 0.96 + sidePhase * 0.019,
                    height01 * 0.62 + seed * 0.017,
                    seed * 0.031 + sidePhase));
                float sideCoord = sideWidthCoord +
                    (sideWarp - 0.5) * lerp(0.18, 0.78, breakup01);

                float sideWaveA = sin(sideCoord * 5.6 + seed * 0.061 + sidePhase);
                float sideWaveB = sin(sideCoord * 10.4 + seed * 0.089 + sidePhase * 1.37);
                float sideWaveC = sin(sideCoord * 15.7 + seed * 0.047 - sidePhase * 0.73);
                float sideNoiseA = PS3D_ValueNoise31(float3(
                    sideCoord * 1.65 + sidePhase * 0.027,
                    seed * 0.021,
                    height01 * 0.42));
                float sideNoiseB = PS3D_ValueNoise31(float3(
                    sideCoord * 3.25 - sidePhase * 0.013,
                    seed * 0.037 + 9.1,
                    height01 * 0.85));
                float rawSideLobe = saturate(
                    (sideWaveA * 0.28 +
                     sideWaveB * 0.19 +
                     sideWaveC * 0.12) * 0.5 + 0.5);
                rawSideLobe = saturate(
                    rawSideLobe * 0.48 +
                    sideNoiseA * 0.32 +
                    sideNoiseB * 0.20);

                // Breakup should increase contrast and local relief, not simply
                // lower the threshold and behave like a second Reach slider.
                float sideLobe =
                    saturate((rawSideLobe - 0.5) *
                        lerp(1.35, 3.15, breakup01) + 0.5);
                float footprintLobe =
                    saturate((footprintWave * 0.44 +
                              broadNoise * 0.26 +
                              lobeNoise * 0.20 +
                              facetNoise * 0.10 - 0.5) *
                        lerp(1.15, 2.25, breakup01) + 0.5);
                float patchRelief =
                    saturate((patchNoise * 0.36 +
                              planeNoise * 0.24 +
                              facetNoise * 0.24 +
                              broadNoise * 0.16 - 0.5) *
                        lerp(1.05, 2.05, breakup01) + 0.5);

                float lobeHeightDriver = saturate(
                    sideLobe * 0.58 +
                    footprintLobe * 0.25 +
                    patchRelief * 0.17);
                float lobePresenceDriver = saturate(
                    sideLobe * 0.66 +
                    footprintLobe * 0.18 +
                    patchRelief * 0.16);

                // Every side point needs a low crawl floor. Breakup should make
                // areas crawl low or high; it should not create fully empty
                // vertical strips. Keep the floor in crawl height, not in mask
                // intensity, so it does not rebuild the old continuous skirt.
                float lobePresence = saturate(
                    lerp(0.18, 0.12, breakup01) +
                    smoothstep(0.30, 0.76, lobePresenceDriver) *
                    lerp(0.72, 0.86, breakup01));

                // Preserve approximate average height while increasing the low/high
                // spread as Breakup rises.
                float lobeHeightContrast =
                    saturate((lobeHeightDriver - 0.5) *
                        lerp(1.10, 2.80, breakup01) + 0.5);

                // Reach controls average crawl height. The default is intentionally
                // visible again, but local side lobes decide how much extra height
                // each area gets above the guaranteed low crawl floor.
                float averageCrawlHeight =
                    (0.078 + tallness * 0.030 + sizeFactor * 0.018) *
                    lerp(0.56, 1.44, reach01);
                float minimumCrawlHeight =
                    averageCrawlHeight *
                    lerp(0.20, 0.15, breakup01);
                float extraCrawlHeight =
                    averageCrawlHeight *
                    lerp(0.00, 2.24, lobeHeightContrast);
                float localCrawlHeight =
                    minimumCrawlHeight + extraCrawlHeight;

                // Smoothness controls the vertical dissolve length independently.
                float fadeLength =
                    (0.125 + tallness * 0.048 + sizeFactor * 0.028) *
                    lerp(1.10, 3.05, smoothness01) *
                    lerp(0.92, 1.32, saturate(broadNoise * 0.55 + sideNoiseA * 0.45));

                float verticalNoise = PS3D_ValueNoise31(float3(
                    sideCoord * 0.86 + sidePhase * 0.017,
                    warpedXZ.x * 0.38 + seed * 0.011,
                    height01 * 1.95 + seed * 0.029));
                float heightJitter =
                    (verticalNoise - 0.5) * lerp(0.050, 0.135, breakup01) +
                    (patchNoise - 0.5) * lerp(0.030, 0.082, breakup01) +
                    (planeNoise - 0.5) * lerp(0.018, 0.055, breakup01) +
                    (facetNoise - 0.5) * 0.028;
                float shiftedHeight = max(0.0, height01 + heightJitter);

                // Long-tail fade: avoid a single smoothstep contour. Higher local
                // crawl shifts the strongest part upward; smoothness widens the
                // dissolve without increasing crawl height.
                float fadeStart = localCrawlHeight * 0.16;
                float fadeDenominator = max(0.035, localCrawlHeight * 0.62 + fadeLength);
                float distanceT = max(0.0, shiftedHeight - fadeStart) / fadeDenominator;
                float falloffShape = lerp(1.05, 0.56, smoothness01);
                float falloffRate = lerp(4.25, 1.55, smoothness01);
                float lowerFade =
                    exp2(-pow(max(0.0, distanceT), falloffShape) * falloffRate);

                lowerFade *= lobePresence;
                lowerFade *= 1.0 - smoothstep(0.68, 0.94, height01);

                float contactAnchor =
                    (1.0 - smoothstep(0.0, 0.010 + tallness * 0.003, height01)) *
                    lerp(0.034, 0.056, lobePresence);

                return saturate(max(lowerFade, contactAnchor));
            }

            float ResolveGeneratedMassMottleNoise(Varyings input)
            {
                float scale = max(0.05, _StoneMottleScale);
                float broad =
                    ResolveGeneratedMassSoftPatchNoise(input, scale * 0.48, 151.0);
                float middle =
                    ResolveGeneratedMassSoftPatchNoise(input, scale * 0.94, 197.0);
                float small =
                    ResolveGeneratedMassPatchNoise(input, scale * 1.82, 233.0);
                float raw = saturate(
                    broad * 0.56 +
                    middle * 0.32 +
                    small * 0.12);

                // Higher softness keeps the mottle as broad material variation;
                // lower softness exaggerates the same field for validation.
                float contrast = lerp(1.90, 0.78, saturate(_StoneMottleSoftness));
                return saturate((raw - 0.5) * contrast + 0.5);
            }

            half3 ApplyGeneratedMassSurfaceMottle(
                half3 albedo,
                Varyings input,
                float generatedMassSurface,
                float exposureVisual,
                float creviceVisual,
                float baseVisual,
                float dirtDepositVisual,
                float wetness,
                float frostStrength,
                float monolithicFlatten)
            {
                float strength =
                    saturate(_StoneMottleStrength) *
                    saturate(generatedMassSurface) *
                    // Frost and monolithic profiles should reduce mottle, not
                    // erase it completely. Patch 13B keeps some broad stone
                    // breakup visible so these profiles do not collapse into
                    // smooth artificial material slabs.
                    lerp(1.0, 0.68, saturate(frostStrength)) *
                    lerp(1.0, 0.58, saturate(monolithicFlatten));

                if (strength <= 0.0001)
                {
                    return albedo;
                }

                float mottle = ResolveGeneratedMassMottleNoise(input);
                float signedMottle = (mottle - 0.5) * 2.0;
                float shelterBias = saturate(_StoneMottleShelterBias);
                float shelterMask = saturate(
                    creviceVisual * 0.36 +
                    baseVisual * 0.28 +
                    dirtDepositVisual * 0.52 +
                    (1.0 - exposureVisual) * 0.14);

                // Broad face breakup should remain value-based so neutral grey
                // rocks do not regain unwanted hue drift. Shelter bias only
                // increases the darker gathered component in existing semantic
                // dirt/base/crevice zones.
                float broadValueScale =
                    1.0 +
                    signedMottle *
                    strength *
                    lerp(0.070, 0.045, shelterBias);
                float gatheredDarkMask = saturate(
                    (1.0 - mottle) *
                    lerp(0.26, 0.54 + shelterMask * 0.78, shelterBias));
                float gatheredDarken =
                    gatheredDarkMask *
                    strength *
                    lerp(0.055, 0.185, shelterBias) *
                    lerp(1.0, 1.22, saturate(wetness));

                float valueScale = max(0.0, broadValueScale - gatheredDarken);
                return albedo * (half)valueScale;
            }

            float ResolveGeneratedMassOrganicBottomMask(Varyings input)
            {
                // Keep value shaping from rebuilding a separate horizontal band.
                // This only lightly follows the same side-aware lobe field.
                return saturate(ResolveGeneratedMassOrganicLowerFade(input) * 0.20);
            }

            float ResolveShaderCreviceBaseMask(Varyings input)
            {
                float height01 = ResolveGeneratedMassHeight01(input);
                float normalY = normalize((float3)input.normalOS).y;
                float up = saturate(normalY);
                float downward = saturate(-normalY * 1.10);
                float sideFacing = 1.0 - smoothstep(0.16, 0.92, abs(normalY));
                float notUpward = 1.0 - smoothstep(0.10, 0.56, up);
                float tallness = ResolveGeneratedMassTallnessFactor();

                float broadNoise = ResolveGeneratedMassSoftPatchNoise(input, 0.92, 19.0);
                float patchNoise = ResolveGeneratedMassSoftPatchNoise(input, 1.42, 31.0);
                float planeNoise = ResolveGeneratedMassPatchNoise(input, 2.05, 37.0);
                float facetNoise = saturate(
                    (float)input.color.r * 0.46 +
                    broadNoise * 0.20 +
                    patchNoise * 0.17 +
                    planeNoise * 0.17);

                float lowerFade = ResolveGeneratedMassOrganicLowerFade(input);
                float shelter = saturate(sideFacing * 0.50 + notUpward * 0.14 + downward * 0.14);
                float shelterBlend = lerp(0.66, 1.00, smoothstep(0.16, 0.82, shelter));
                float facetAttenuation = lerp(0.58, 1.02, saturate(facetNoise * 0.72 + broadNoise * 0.28));
                float contactAccent =
                    (1.0 - smoothstep(0.0, 0.012 + tallness * 0.003, height01)) * 0.045;

                float mask = lowerFade * shelterBlend * facetAttenuation;
                mask = max(mask, contactAccent);
                return saturate(mask);
            }

            float ResolveShaderDirtDepositMask(Varyings input)
            {
                float height01 = ResolveGeneratedMassHeight01(input);
                float normalY = normalize((float3)input.normalOS).y;
                float up = saturate(normalY);
                float downward = saturate(-normalY * 1.22);
                float sideFacing = 1.0 - smoothstep(0.24, 0.92, abs(normalY));
                float notUpward = 1.0 - smoothstep(0.09, 0.60, up);
                float depositShelter = saturate(
                    sideFacing * 0.74 +
                    downward * 0.24 +
                    notUpward * 0.18);

                float tallness = ResolveGeneratedMassTallnessFactor();
                float dirtReach = max(0.05, _GeneratedMassDirtCrawlReach);
                float dirtCoverage = max(0.05, _GeneratedMassDirtCoverage);
                float dirtCoverageDelta = clamp(dirtCoverage - 1.0, -0.75, 1.0);
                float dirtCoverageMultiplier = clamp(dirtCoverage, 0.35, 1.45);
                float xzScale = max(0.0001, _GeneratedMassLocalXZScale);
                float2 normalizedXZ = input.positionOS.xz / xzScale;
                float seed = _GeneratedMassMaskSeed;

                float lowNoise =
                    ResolveGeneratedMassSoftPatchNoise(input, 0.88, 47.0);
                float mediumNoise =
                    ResolveGeneratedMassPatchNoise(input, 2.80, 71.0);
                float highNoise =
                    ResolveGeneratedMassPatchNoise(input, 7.60, 113.0);

                float skeletonWaveA =
                    1.0 - smoothstep(
                        0.18,
                        0.56,
                        abs(sin(normalizedXZ.x * 10.5 + lowNoise * 2.4 + seed * 0.041)));
                float skeletonWaveB =
                    1.0 - smoothstep(
                        0.14,
                        0.52,
                        abs(sin((normalizedXZ.x * 0.62 + normalizedXZ.y * 0.91) * 8.8 + mediumNoise * 1.8 + seed * 0.067)));
                float crawlSkeleton = saturate(max(skeletonWaveA * 0.82, skeletonWaveB * 0.70));
                crawlSkeleton *= smoothstep(0.30, 0.82, lowNoise * 0.56 + mediumNoise * 0.44);

                float crawlHeight =
                    (0.070 + 0.305 * pow(lowNoise, 1.42) +
                    crawlSkeleton * 0.070) *
                    dirtReach;
                crawlHeight = min(
                    crawlHeight + tallness * 0.040 * dirtReach,
                    clamp(0.48 * dirtReach, 0.12, 0.86));
                float connectedCrawl =
                    1.0 - smoothstep(
                        crawlHeight,
                        crawlHeight + 0.086,
                        height01);

                float baseConnection =
                    1.0 - smoothstep(
                        0.0,
                        0.070 + tallness * 0.010,
                        height01);
                float heightTaper =
                    1.0 - smoothstep(
                        crawlHeight * 0.62,
                        crawlHeight + 0.055,
                        height01);

                float erosion =
                    smoothstep(
                        saturate(0.34 - dirtCoverageDelta * 0.12),
                        saturate(0.72 - dirtCoverageDelta * 0.10),
                        mediumNoise * 0.50 + highNoise * 0.34 + lowNoise * 0.16);
                float fineBreakup = lerp(0.62, 1.06, highNoise);
                float skeletonCoverage = lerp(0.42, 1.00, crawlSkeleton);

                float rimCore =
                    1.0 - smoothstep(0.0, 0.050 + tallness * 0.010, height01);
                float rimBreakup = smoothstep(
                    0.30,
                    0.68,
                    mediumNoise * 0.50 + highNoise * 0.30 + lowNoise * 0.20);
                float brokenRim =
                    rimCore * rimBreakup * lerp(0.46, 0.96, depositShelter);

                float crawlDeposit =
                    connectedCrawl *
                    heightTaper *
                    erosion *
                    fineBreakup *
                    skeletonCoverage *
                    depositShelter;

                float upperSuppress = smoothstep(0.46, 0.66, height01);
                float mask = max(
                    baseConnection * rimBreakup * 0.30 * dirtCoverageMultiplier,
                    brokenRim * 0.54 * dirtCoverageMultiplier);
                mask = max(mask, crawlDeposit * 0.76 * dirtCoverageMultiplier);
                mask *= 1.0 - upperSuppress;
                return saturate(pow(mask, 1.06));
            }

            float ResolveSurfaceContractIsGround()
            {
                return step(0.5, _SurfaceContract);
            }

            float ResolveGroundTonalMask(Varyings input)
            {
                return saturate((float)input.color.r);
            }

            float ResolveGroundExposureMask(Varyings input)
            {
                return saturate((float)input.color.g);
            }

            float ResolveGroundDampDepositMask(Varyings input)
            {
                return saturate((float)input.color.b);
            }

            float ResolveGroundVegetationMask(Varyings input)
            {
                return saturate((float)input.color.a);
            }

            float ResolveGroundCompactionMask(Varyings input)
            {
                return saturate(input.materialMasks.x);
            }

            float ResolveGroundShoreMask(Varyings input)
            {
                return saturate(input.materialMasks.y);
            }

            float ResolveGroundRockyDryMask(Varyings input)
            {
                return saturate(input.materialMasks.z);
            }

            half PS3D_MaskTintLuminance(half3 color)
            {
                return dot(color, half3(0.2126h, 0.7152h, 0.0722h));
            }

            half3 PS3D_ApplyValuePreservingTint(
                half3 neutralTarget,
                half3 tintColor,
                float tintStrength)
            {
                half strength = saturate((half)tintStrength);
                half targetLum = max(0.001h, PS3D_MaskTintLuminance(neutralTarget));
                half tintLum = max(0.001h, PS3D_MaskTintLuminance(tintColor));
                half3 hueTarget = tintColor * (targetLum / tintLum);
                return lerp(neutralTarget, hueTarget, strength);
            }

            float4 ResolveGeneratedMassFeatureAtlas0(Varyings input)
            {
                if (_GeneratedMassFeatureAtlas0Enabled < 0.5)
                {
                    return float4(0.0, 0.0, 0.0, 0.0);
                }

                float2 atlasUV = saturate(input.featureAtlasUV);
                return saturate(
                    SAMPLE_TEXTURE2D(
                        _GeneratedMassFeatureAtlas0,
                        sampler_GeneratedMassFeatureAtlas0,
                        atlasUV));
            }

            float ResolveGeneratedMassAtlasEdgeWearMask(Varyings input)
            {
                float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                return saturate(atlas0.r * atlas0.g);
            }

            float ResolveGeneratedMassAtlasCreaseMask(Varyings input)
            {
                float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                return saturate(atlas0.b * atlas0.a);
            }

            half3 ResolveMaskDebugColor(Varyings input)
            {
                int mode = (int)round(_MaskDebugMode);

                if (mode <= 0)
                {
                    return half3(-1.0h, -1.0h, -1.0h);
                }

                if (mode == 4)
                {
                    float mask = ResolveGeneratedMassAtlasEdgeWearMask(input);

                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(1.0, 0.92, 0.55),
                        mask);
                }

                if (mode == 5)
                {
                    float mask = ResolveGeneratedMassAtlasCreaseMask(input);

                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(0.20, 0.36, 1.0),
                        mask);
                }

                float mask = 0.0;
                if (mode == 1)
                {
                    mask = saturate((float)input.color.r);
                }
                else if (mode == 2)
                {
                    mask = saturate((float)input.color.g);
                }
                else if (mode == 3)
                {
                    mask = ResolveShaderCreviceBaseMask(input);
                }
                else if (mode == 6)
                {
                    mask = ResolveShaderDirtDepositMask(input);
                }
                else if (mode == 7)
                {
                    mask = ResolveGroundTonalMask(input);
                }
                else if (mode == 8)
                {
                    mask = ResolveGroundExposureMask(input);
                }
                else if (mode == 9)
                {
                    mask = ResolveGroundDampDepositMask(input);
                }
                else if (mode == 10)
                {
                    mask = ResolveGroundVegetationMask(input);
                }
                else if (mode == 11)
                {
                    mask = ResolveGroundCompactionMask(input);
                }
                else if (mode == 12)
                {
                    mask = ResolveGroundShoreMask(input);
                }
                else if (mode == 13)
                {
                    mask = ResolveGroundRockyDryMask(input);
                }
                else if (mode == 14)
                {
                    float exposure = ResolveGroundExposureMask(input);
                    float damp = saturate(
                        ResolveGroundDampDepositMask(input) * 0.75 +
                        ResolveGroundShoreMask(input) * 0.45);
                    float vegetationOrDry = max(
                        ResolveGroundVegetationMask(input),
                        ResolveGroundRockyDryMask(input));

                    return (half3)float3(
                        exposure,
                        damp,
                        vegetationOrDry);
                }

                return (half3)lerp(
                    float3(0.025, 0.025, 0.035),
                    float3(1.0, 0.92, 0.55),
                    mask);
            }

            half3 ResolvePixelSurfaceColor(Varyings input)
            {
                half4 baseSample =
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                float broadCellSize = max(_PixelCellSize * 8.0, 0.0001);
                float3 broadCoordinate =
                    input.positionWS / broadCellSize + _PixelSeed * 0.013;
                float3 warp =
                    float3(
                        PS3D_ValueNoise31(broadCoordinate + 11.17),
                        PS3D_ValueNoise31(broadCoordinate + 23.31),
                        PS3D_ValueNoise31(broadCoordinate + 37.47)) *
                    2.0 -
                    1.0;
                float3 pixelPositionWS =
                    input.positionWS +
                    warp * _PixelCellSize * _PixelWarpStrength;

                float pixelVariation;
                PixelCellVariation_float(
                    pixelPositionWS,
                    _PixelCellSize,
                    _PixelSeed,
                    _PixelToneCount,
                    _PixelClusterStrength,
                    pixelVariation);

                float broadValue =
                    PS3D_ValueNoise31(broadCoordinate + 53.29) * 2.0 - 1.0;
                float contractMask =
                    1.0 -
                    step(
                        0.995,
                        min(
                            min((float)input.color.r, (float)input.color.g),
                            (float)input.color.b));
                float vertexVariation =
                    ((float)input.color.r - 0.5) * 2.0 * contractMask;
                float pixelProfileContrast =
                    max(0.0, _ProfilePixelContrast) *
                    lerp(1.0, 1.0 - saturate(_WetPixelSoftening), saturate(_Wetness)) *
                    lerp(1.0, max(0.0, _FrostContrast), saturate(_FrostStrength)) *
                    lerp(1.0, 0.25, saturate(_MonolithicFlatten));
                float tonalOffset =
                    (pixelVariation * _PixelVariation +
                     vertexVariation * _PixelVertexVariation +
                     broadValue * _PixelBroadVariation) *
                    pixelProfileContrast;
                half tonalScale =
                    (half)max(0.0, 1.0 + tonalOffset * _PixelEffectStrength);

                float isGroundSurface = ResolveSurfaceContractIsGround();
                float exposureMask =
                    saturate((float)input.color.g) * contractMask;
                float massCreviceMask =
                    ResolveShaderCreviceBaseMask(input) * contractMask;
                float creviceMask =
                    lerp(massCreviceMask, 0.0, isGroundSurface);
                float massDirtDepositMask =
                    ResolveShaderDirtDepositMask(input) * contractMask;
                float dirtDepositMask =
                    lerp(massDirtDepositMask, 0.0, isGroundSurface);
                float baseMask = creviceMask * (1.0 - exposureMask);
                float groundDampDeposit = ResolveGroundDampDepositMask(input);
                float groundShore = ResolveGroundShoreMask(input);
                float groundRockyDry = ResolveGroundRockyDryMask(input);
                float groundVegetation = ResolveGroundVegetationMask(input);
                float groundDampVisual = saturate(
                    (groundDampDeposit * 0.78 +
                     groundShore * 0.52 * max(0.0, _GroundShoreDampStrength)) *
                    max(0.0, _GroundDampResponse));
                float groundSnowVisual = saturate(
                    exposureMask * max(0.0, _GroundSnowResponse) *
                    (1.0 - groundDampVisual * 0.42));
                float groundRockyDryVisual = saturate(
                    groundRockyDry * max(0.0, _GroundRockyDryResponse));
                float groundVegetationVisual = saturate(
                    groundVegetation * max(0.0, _GroundVegetationResponse));
                float profileContrast =
                    max(0.0, _ProfileContrast) *
                    lerp(1.0, max(0.0, _FrostContrast), saturate(_FrostStrength));
                float generatedMassExposureResponse =
                    max(0.0, _GeneratedMassExposureResponse);
                float generatedMassCreviceResponse =
                    max(0.0, _GeneratedMassCreviceResponse);
                float generatedMassBaseResponse =
                    max(0.0, _GeneratedMassBaseResponse);
                float generatedMassDirtDepositResponse =
                    max(0.0, _GeneratedMassDirtDepositResponse);

                float wetness = saturate(_Wetness);
                float frostStrength = saturate(_FrostStrength);
                float monolithicFlatten = saturate(_MonolithicFlatten);
                float generatedMassSurface = 1.0 - isGroundSurface;

                float exposureVisual =
                    pow(saturate(exposureMask), 0.72);
                float creviceVisual =
                    pow(saturate(creviceMask), 0.58) *
                    (1.0 - exposureVisual * 0.22);
                float baseVisual =
                    pow(saturate(baseMask), 0.78) *
                    (1.0 - exposureVisual * 0.18);
                float dirtDepositVisual =
                    pow(saturate(dirtDepositMask), 0.70);

                // Exposure remains the only generated-mass mask that primarily
                // shifts the pre-layer value scale. Crevice, base, and dirt are
                // handled below as independent material layers so their response
                // controls do not collapse into one shared lower-region multiplier.
                float generatedMassSemanticScale =
                    1.0 +
                    exposureVisual *
                    _ExposureTintStrength *
                    1.72 *
                    generatedMassExposureResponse *
                    profileContrast;
                float groundSemanticScale =
                    1.0 +
                    (groundSnowVisual * 0.11 -
                     groundDampVisual * 0.18 -
                     groundRockyDryVisual * 0.035 +
                     groundVegetationVisual * 0.025) *
                    profileContrast;
                float semanticScale = lerp(
                    generatedMassSemanticScale,
                    groundSemanticScale,
                    isGroundSurface);

                half3 albedo =
                    baseSample.rgb *
                    _BaseColor.rgb *
                    tonalScale *
                    (half)max(0.0, semanticScale);

                half3 groundAlbedo = albedo;
                groundAlbedo = lerp(
                    groundAlbedo,
                    _FrostColor.rgb,
                    (half)(groundSnowVisual * 0.34));
                groundAlbedo *=
                    (half)max(0.0, 1.0 - groundDampVisual * 0.24);
                groundAlbedo = lerp(
                    groundAlbedo,
                    groundAlbedo * half3(0.88h, 0.90h, 0.93h),
                    (half)(groundRockyDryVisual * 0.22));
                groundAlbedo = lerp(
                    groundAlbedo,
                    groundAlbedo * half3(0.94h, 1.00h, 0.90h),
                    (half)(groundVegetationVisual * 0.18));
                albedo = lerp(
                    albedo,
                    groundAlbedo,
                    (half)isGroundSurface);

                albedo = ApplyGeneratedMassSurfaceMottle(
                    albedo,
                    input,
                    generatedMassSurface,
                    exposureVisual,
                    creviceVisual,
                    baseVisual,
                    dirtDepositVisual,
                    wetness,
                    frostStrength,
                    monolithicFlatten);

                half3 exposureTintTarget =
                    PS3D_ApplyValuePreservingTint(
                        albedo,
                        _GeneratedMassExposureTint.rgb,
                        _GeneratedMassExposureTintStrength);
                float exposureTintOpacity =
                    exposureVisual *
                    generatedMassExposureResponse *
                    generatedMassSurface *
                    saturate(_GeneratedMassExposureTintStrength);
                albedo = lerp(
                    albedo,
                    exposureTintTarget,
                    (half)saturate(exposureTintOpacity));

                // Dedicated crevice layer: profile-aware depth/occlusion.
                // Response 1.0 is intentionally stronger than H2L response 2.0.
                half3 creviceNeutralTarget =
                    albedo *
                    (half)lerp(0.48, 0.38, wetness);
                half3 creviceTarget =
                    PS3D_ApplyValuePreservingTint(
                        creviceNeutralTarget,
                        _GeneratedMassCreviceTint.rgb,
                        _GeneratedMassCreviceTintStrength);
                creviceTarget = lerp(
                    creviceTarget,
                    _BaseColor.rgb * (half)0.46,
                    (half)(monolithicFlatten * 0.82));
                float creviceOpacity =
                    (1.0 - exp2(
                        -creviceVisual *
                        (2.80 +
                         _CreviceDarkenStrength * 14.50 +
                         wetness * 1.05 +
                         frostStrength * 0.62) *
                        generatedMassCreviceResponse *
                        profileContrast)) *
                    generatedMassSurface *
                    lerp(1.0, 0.72, frostStrength) *
                    lerp(1.0, 0.66, monolithicFlatten);
                albedo = lerp(
                    albedo,
                    creviceTarget,
                    (half)saturate(creviceOpacity));

                // Dedicated base/contact layer: broader grounding, less deep
                // than crevice, controlled only by Base Response.
                half3 baseNeutralTarget =
                    albedo *
                    (half)lerp(0.70, 0.62, wetness);
                half3 baseTarget =
                    PS3D_ApplyValuePreservingTint(
                        baseNeutralTarget,
                        _GeneratedMassBaseTint.rgb,
                        _GeneratedMassBaseTintStrength);
                baseTarget = lerp(
                    baseTarget,
                    _BaseColor.rgb * (half)0.62,
                    (half)(monolithicFlatten * 0.70));
                float baseOpacity =
                    (1.0 - exp2(
                        -baseVisual *
                        (1.25 +
                         _BaseDarkenStrength * 9.50 +
                         wetness * 0.42) *
                        generatedMassBaseResponse *
                        profileContrast)) *
                    generatedMassSurface *
                    lerp(1.0, 0.70, frostStrength) *
                    lerp(1.0, 0.62, monolithicFlatten);
                albedo = lerp(
                    albedo,
                    baseTarget,
                    (half)saturate(baseOpacity));

                // Dedicated dirt/deposit layer. Response 1.0 is calibrated to
                // land near the previous H2L response 2.0 visual strength, but
                // the exponential opacity curve keeps high values from becoming
                // flat paint too quickly.
                half3 dirtNeutralTarget =
                    albedo *
                    (half)lerp(0.88, 0.70, wetness);
                half3 dirtTintTarget =
                    PS3D_ApplyValuePreservingTint(
                        dirtNeutralTarget,
                        _GeneratedMassDirtDepositTint.rgb,
                        _GeneratedMassDirtDepositTintStrength);
                half3 dirtTarget = lerp(
                    dirtTintTarget,
                    dirtTintTarget *
                        (half)lerp(0.92, 0.62, saturate(_WetDarkenStrength)),
                    (half)wetness);
                float dirtOpacity =
                    (1.0 - exp2(
                        -dirtDepositVisual *
                        (1.75 + saturate(_StoneDirtResponse) * 2.65) *
                        generatedMassDirtDepositResponse *
                        lerp(1.0, 1.36, wetness))) *
                    generatedMassSurface *
                    lerp(1.0, 0.18, frostStrength) *
                    lerp(1.0, 0.09, monolithicFlatten);
                albedo = lerp(
                    albedo,
                    dirtTarget,
                    (half)saturate(dirtOpacity));

                float dampGatherMask =
                    saturate(
                        dirtDepositVisual * generatedMassDirtDepositResponse * 0.82 +
                        baseVisual * generatedMassBaseResponse * 0.20 +
                        creviceVisual * generatedMassCreviceResponse * 0.14 -
                        exposureVisual * generatedMassExposureResponse * 0.16);
                half3 wetDampNeutralTarget =
                    albedo *
                    (half)lerp(0.88, 0.58, saturate(_WetDarkenStrength));
                half3 wetDampTarget =
                    PS3D_ApplyValuePreservingTint(
                        wetDampNeutralTarget,
                        _GeneratedMassDirtDepositTint.rgb,
                        _GeneratedMassDirtDepositTintStrength);
                float wetDampStrength =
                    dampGatherMask *
                    wetness *
                    saturate(_WetDarkenStrength * 1.65) *
                    generatedMassSurface;
                albedo = lerp(
                    albedo,
                    wetDampTarget,
                    (half)saturate(wetDampStrength));

                float frostNoise =
                    PS3D_ValueNoise31(broadCoordinate * 1.7 + 71.31);
                float frostPattern =
                    saturate(
                        (frostNoise - (1.0 - saturate(_FrostCoverage))) /
                        max(0.001, saturate(_FrostCoverage)));
                float frostPatternSoft =
                    smoothstep(0.12, 0.88, frostPattern);

                // Patch 13B: frost should read as a coherent pale material
                // layer, not as a high-contrast triangle/facet visualizer.
                // Keep exposure important, but soften its authority and use the
                // procedural frost field as a low-frequency breakup term.
                float frostExposure =
                    saturate(exposureVisual * 0.72 + broadValue * 0.08);
                float frostMask =
                    saturate(
                        frostExposure * (0.84 * generatedMassExposureResponse) +
                        frostPatternSoft * 0.22 -
                        creviceVisual * (0.10 * generatedMassCreviceResponse) -
                        dirtDepositVisual *
                            (0.10 * generatedMassDirtDepositResponse)) *
                    frostStrength *
                    generatedMassSurface;
                albedo = lerp(
                    albedo,
                    _FrostColor.rgb,
                    (half)(frostMask * 0.62));

                float wetGlobalDarken =
                    wetness * saturate(_WetDarkenStrength) * 0.36;
                albedo *= (half)max(0.0, 1.0 - wetGlobalDarken);

                float monolithicRelief =
                    broadValue * 0.028 +
                    exposureVisual * (0.078 * generatedMassExposureResponse) -
                    creviceVisual * (0.110 * generatedMassCreviceResponse) -
                    baseVisual * (0.052 * generatedMassBaseResponse);
                half3 monolithicTarget =
                    _BaseColor.rgb * (half)max(0.0, 1.0 + monolithicRelief);
                albedo = lerp(
                    albedo,
                    monolithicTarget,
                    (half)monolithicFlatten);

                return albedo;
            }

            half3 ApplyStylizedValueShaping(
                half3 albedo,
                Varyings input,
                half3 normalWS)
            {
                Light mainLight = GetMainLight();
                half litMask = saturate(dot(normalWS, mainLight.direction));
                half highlightMask = saturate(
                    (litMask - (half)_HighlightCompressStart) /
                    max(0.001h, 1.0h - (half)_HighlightCompressStart));
                half highlightScale =
                    1.0h -
                    highlightMask *
                    saturate((half)_HighlightCompressStrength);

                float isGroundSurface = ResolveSurfaceContractIsGround();
                half generatedMassBottomMask =
                    (half)ResolveGeneratedMassOrganicBottomMask(input);
                half defaultBottomMask =
                    1.0h -
                    smoothstep(
                        0.0h,
                        max(0.001h, (half)_BottomDarkenHeight),
                        (half)max(0.0, input.positionOS.y));
                half bottomMask = lerp(
                    generatedMassBottomMask,
                    defaultBottomMask,
                    (half)isGroundSurface);
                half sideMask = pow(
                    saturate(1.0h - abs(normalWS.y)),
                    max(0.5h, (half)_EdgeDarkenPower));
                half generatedMassBaseResponse =
                    (half)max(0.0, _GeneratedMassBaseResponse);
                half bottomResponseScale =
                    lerp(generatedMassBaseResponse, 1.0h, (half)isGroundSurface);
                half bottomDarken =
                    bottomMask *
                    saturate((half)_BottomDarkenStrength) *
                    bottomResponseScale;
                half broadEdgeDarken =
                    bottomMask *
                    sideMask *
                    saturate((half)_EdgeDarkenStrength) *
                    bottomResponseScale;
                half valueScale =
                    highlightScale *
                    (1.0h - saturate(bottomDarken + broadEdgeDarken));

                return albedo * valueScale;
            }

            half ResolveProfileSmoothness()
            {
                return saturate(
                    (half)_Smoothness +
                    (half)_Wetness * (half)_WetSmoothnessBoost +
                    (half)_MonolithicFlatten *
                    (half)_MonolithicSmoothnessBoost -
                    (half)_FrostStrength * 0.06h);
            }

            InputData BuildInputData(Varyings input, half3 normalWS)
            {
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS =
                    SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                inputData.shadowCoord =
                    TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.vertexLighting = half3(0.0h, 0.0h, 0.0h);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV =
                    GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1.0h, 1.0h, 1.0h, 1.0h);
                return inputData;
            }

            SurfaceData BuildSurfaceData(half3 albedo)
            {
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.specular =
                    (half3)_SpecularStrength *
                    // Wet and monolithic stone can have controlled highlights,
                    // but the previous amplification pushed profiles toward
                    // polished metal/glass. Keep profile identity without
                    // overwhelming stone roughness.
                    lerp(
                        1.0h,
                        1.25h,
                        saturate((half)_Wetness)) *
                    lerp(
                        1.0h,
                        1.10h,
                        saturate((half)_MonolithicFlatten));
                surfaceData.metallic = 0.0h;
                surfaceData.smoothness = ResolveProfileSmoothness();
                surfaceData.normalTS = half3(0.0h, 0.0h, 1.0h);
                surfaceData.emission = half3(0.0h, 0.0h, 0.0h);
                surfaceData.occlusion = 1.0h;
                surfaceData.alpha = _BaseColor.a;
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;
                return surfaceData;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half3 normalWS = normalize(input.normalWS);
                half flatNormalStrength =
                    saturate((half)_FlatNormalStrength);
                if (flatNormalStrength > 0.001h)
                {
                    half3 viewDirectionWS =
                        SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                    half3 flatNormalWS = normalize(
                        cross(
                            ddy(input.positionWS),
                            ddx(input.positionWS)));
                    flatNormalWS = faceforward(
                        flatNormalWS,
                        -viewDirectionWS,
                        flatNormalWS);
                    normalWS = normalize(
                        lerp(
                            normalWS,
                            flatNormalWS,
                            flatNormalStrength));
                }
                half3 debugColor = ResolveMaskDebugColor(input);
                if (debugColor.r >= 0.0h)
                {
                    return half4(debugColor, 1.0h);
                }

                half3 albedo = ResolvePixelSurfaceColor(input);
                albedo = ApplyStylizedValueShaping(albedo, input, normalWS);
                albedo = PS3D_ApplyValuePreservingTint(
                    albedo,
                    _GeneratedMassOverallRockTint.rgb,
                    _GeneratedMassOverallRockTintStrength);

                InputData inputData = BuildInputData(input, normalWS);
                SurfaceData surfaceData = BuildSurfaceData(albedo);
                half4 pbrColor = UniversalFragmentPBR(inputData, surfaceData);

                // Keep URP/PBR lighting, shadows, local lights and specular,
                // but reduce how much RGB light colour can override the rock's
                // chosen material hue. This preserves brightness/form from PBR
                // while letting light tint remain an adjustable influence.
                half3 safeAlbedo = max(albedo, half3(0.001h, 0.001h, 0.001h));
                half3 pbrLightingRatio = pbrColor.rgb / safeAlbedo;
                half lightingLuma =
                    dot(
                        pbrLightingRatio,
                        half3(0.2126h, 0.7152h, 0.0722h));
                half3 neutralLitColor =
                    albedo * max(0.0h, lightingLuma);

                half lightingTintInfluence =
                    saturate((half)_GeneratedMassLightingTintInfluence);
                half3 finalRgb =
                    lerp(
                        neutralLitColor,
                        pbrColor.rgb,
                        lightingTintInfluence);

                finalRgb = MixFog(finalRgb, inputData.fogCoord);
                return half4(finalRgb, pbrColor.a);
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
                float _GeneratedMassEdgeWearCoverage;
                float _GeneratedMassEdgeWearSoftness;
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
                float _GeneratedMassEdgeWearCoverage;
                float _GeneratedMassEdgeWearSoftness;
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
                float _GeneratedMassEdgeWearCoverage;
                float _GeneratedMassEdgeWearSoftness;
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
