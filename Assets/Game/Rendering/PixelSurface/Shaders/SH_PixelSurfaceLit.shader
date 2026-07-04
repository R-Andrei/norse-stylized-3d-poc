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
        [Enum(None,0,SurfaceVariation,1,Exposure,2,CreviceBase,3,ConvexEdgeWear,4,ConcaveCrease,5,DirtDeposit,6)]
        _MaskDebugMode("Mask Debug Mode", Float) = 0
        [HideInInspector] _GeneratedMassLocalMinY("Generated Mass Local Min Y", Float) = 0
        [HideInInspector] _GeneratedMassLocalHeight("Generated Mass Local Height", Float) = 1
        [HideInInspector] _GeneratedMassMaskSeed("Generated Mass Mask Seed", Float) = 0
        [HideInInspector] _GeneratedMassLocalXZScale("Generated Mass Local XZ Scale", Float) = 1
        [HideInInspector] _GeneratedMassMaskBaseLift("Generated Mass Mask Base Lift", Float) = 0
        [HideInInspector] _GeneratedMassCreviceReach("Generated Mass Crevice Reach", Float) = 1
        [HideInInspector] _GeneratedMassCreviceBreakup("Generated Mass Crevice Breakup", Float) = 1
        [HideInInspector] _GeneratedMassDirtCrawlReach("Generated Mass Dirt Crawl Reach", Float) = 1
        [HideInInspector] _GeneratedMassDirtCoverage("Generated Mass Dirt Coverage", Float) = 1

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
                float _MaskDebugMode;
                float _GeneratedMassLocalMinY;
                float _GeneratedMassLocalHeight;
                float _GeneratedMassMaskSeed;
                float _GeneratedMassLocalXZScale;
                float _GeneratedMassMaskBaseLift;
                float _GeneratedMassCreviceReach;
                float _GeneratedMassCreviceBreakup;
                float _GeneratedMassDirtCrawlReach;
                float _GeneratedMassDirtCoverage;
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

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD2;
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

            float ResolveShaderCreviceBaseMask(Varyings input)
            {
                float height01 = ResolveGeneratedMassHeight01(input);
                float normalY = normalize((float3)input.normalOS).y;
                float up = saturate(normalY);
                float downward = saturate(-normalY * 1.18);
                float sideFacing = 1.0 - smoothstep(0.18, 0.90, abs(normalY));
                float notUpward = 1.0 - smoothstep(0.08, 0.52, up);

                float tallness = ResolveGeneratedMassTallnessFactor();
                float sizeFactor = ResolveGeneratedMassSizeFactor();
                float creviceReach = max(0.05, _GeneratedMassCreviceReach);
                float creviceBreakup = max(0.05, _GeneratedMassCreviceBreakup);
                float creviceBreakupDelta = clamp(creviceBreakup - 1.0, -0.75, 1.0);
                float xzScale = max(0.0001, _GeneratedMassLocalXZScale);
                float2 normalizedXZ = input.positionOS.xz / xzScale;
                float seed = _GeneratedMassMaskSeed;

                float waveA = sin(normalizedXZ.x * 8.4 + seed * 0.113);
                float waveB = sin(normalizedXZ.y * 6.7 + seed * 0.071 + 1.73);
                float waveC = sin(
                    (normalizedXZ.x + normalizedXZ.y * 0.73) * 9.7 +
                    seed * 0.097 - 0.61);
                float irregularWave =
                    (waveA * 0.44 + waveB * 0.35 + waveC * 0.21) * 0.5 + 0.5;

                float broadNoise =
                    ResolveGeneratedMassSoftPatchNoise(input, 1.08, 19.0);
                float planeBreakNoise =
                    ResolveGeneratedMassPatchNoise(input, 1.95, 31.0);
                float facetNoise =
                    saturate((float)input.color.r * 0.50 + broadNoise * 0.28 + planeBreakNoise * 0.22);
                float boundaryWarp = saturate(irregularWave * 0.74 + broadNoise * 0.26);
                float patchField = saturate(
                    irregularWave * 0.38 +
                    broadNoise * 0.22 +
                    facetNoise * 0.18 +
                    planeBreakNoise * 0.22);

                float baseRise =
                    (0.078 + tallness * 0.026 + sizeFactor * 0.018) *
                    creviceReach;
                float localBoundary = baseRise * lerp(0.34, 1.78, boundaryWarp);
                float boundaryFeather =
                    (0.044 + tallness * 0.016) *
                    lerp(0.85, 1.15, saturate((creviceReach - 0.25) / 1.75));

                float contactCore =
                    1.0 - smoothstep(
                        0.0,
                        (0.032 + tallness * 0.006) *
                            lerp(0.85, 1.12, saturate((creviceReach - 0.25) / 1.75)),
                        height01);
                float lowerRegion =
                    1.0 - smoothstep(
                        localBoundary,
                        localBoundary + boundaryFeather,
                        height01);
                float lowerShoulder =
                    1.0 - smoothstep(
                        localBoundary + 0.018,
                        localBoundary + 0.088,
                        height01);

                float shelter = saturate(
                    sideFacing * 0.54 +
                    notUpward * 0.15 +
                    downward * 0.17);
                float shelterGate = smoothstep(0.33, 0.80, shelter);
                float facetBreakup = lerp(0.66, 1.00, facetNoise);
                float regionCoverage = smoothstep(
                    saturate(0.40 + creviceBreakupDelta * 0.08),
                    saturate(0.86 + creviceBreakupDelta * 0.04),
                    patchField);
                float interruption = lerp(
                    saturate(0.16 - creviceBreakupDelta * 0.08),
                    1.00,
                    regionCoverage);

                float lowerSideShelter =
                    lowerRegion *
                    shelterGate *
                    facetBreakup *
                    interruption;
                float sideMids = lowerShoulder * sideFacing * lerp(0.002, 0.018, broadNoise) * interruption;
                float mask = max(contactCore * 0.90, lowerSideShelter * 0.54);
                mask += sideMids;

                float upperSuppress = smoothstep(0.36, 0.56, height01);
                mask *= lerp(1.0, 0.02, upperSuppress);
                return saturate(pow(mask, 1.26));
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

            half3 ResolveMaskDebugColor(Varyings input)
            {
                int mode = (int)round(_MaskDebugMode);

                if (mode <= 0)
                {
                    return half3(-1.0h, -1.0h, -1.0h);
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
                else if (mode == 4)
                {
                    mask = saturate((float)input.color.a);
                }
                else if (mode == 5)
                {
                    mask = saturate((float)input.materialMasks.x);
                }
                else if (mode == 6)
                {
                    mask = ResolveShaderDirtDepositMask(input);
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

                float exposureMask =
                    saturate((float)input.color.g) * contractMask;
                float creviceMask =
                    ResolveShaderCreviceBaseMask(input) * contractMask;
                float baseMask = creviceMask * (1.0 - exposureMask);
                float profileContrast =
                    max(0.0, _ProfileContrast) *
                    lerp(1.0, max(0.0, _FrostContrast), saturate(_FrostStrength));
                float creviceDarken =
                    _CreviceDarkenStrength *
                    lerp(1.0, 0.4, saturate(_Wetness)) +
                    _FrostCreviceDarken * saturate(_FrostStrength);
                float baseDarken =
                    _BaseDarkenStrength *
                    lerp(1.0, 0.65, saturate(_Wetness));
                float semanticScale =
                    1.0 +
                    (exposureMask * _ExposureTintStrength -
                     creviceMask * creviceDarken -
                     baseMask * baseDarken) *
                    profileContrast;

                half3 albedo =
                    baseSample.rgb *
                    _BaseColor.rgb *
                    tonalScale *
                    (half)max(0.0, semanticScale);

                float frostNoise =
                    PS3D_ValueNoise31(broadCoordinate * 1.7 + 71.31);
                float frostPattern =
                    saturate(
                        (frostNoise - (1.0 - saturate(_FrostCoverage))) /
                        max(0.001, saturate(_FrostCoverage)));
                float frostMask =
                    saturate(
                        exposureMask * 0.85 +
                        frostPattern * 0.45 -
                        creviceMask * 0.35) *
                    saturate(_FrostStrength);
                albedo = lerp(
                    albedo,
                    _FrostColor.rgb,
                    (half)(frostMask * 0.62));

                albedo *= (half)max(
                    0.0,
                    1.0 - saturate(_Wetness) * _WetDarkenStrength);

                half3 monolithicTarget =
                    _BaseColor.rgb * (half)(1.0 + broadValue * 0.025);
                albedo = lerp(
                    albedo,
                    monolithicTarget,
                    (half)saturate(_MonolithicFlatten));

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

                half bottomMask =
                    1.0h -
                    smoothstep(
                        0.0h,
                        max(0.001h, (half)_BottomDarkenHeight),
                        (half)max(0.0, input.positionOS.y));
                half sideMask = pow(
                    saturate(1.0h - abs(normalWS.y)),
                    max(0.5h, (half)_EdgeDarkenPower));
                half bottomDarken =
                    bottomMask *
                    saturate((half)_BottomDarkenStrength);
                half broadEdgeDarken =
                    bottomMask *
                    sideMask *
                    saturate((half)_EdgeDarkenStrength);
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
                    lerp(
                        1.0h,
                        1.8h,
                        saturate((half)_Wetness)) *
                    lerp(
                        1.0h,
                        1.35h,
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
                half3 albedo = ResolvePixelSurfaceColor(input);
                half3 debugColor = ResolveMaskDebugColor(input);
                if (debugColor.r >= 0.0h)
                {
                    return half4(debugColor, 1.0h);
                }

                albedo = ApplyStylizedValueShaping(albedo, input, normalWS);
                InputData inputData = BuildInputData(input, normalWS);
                SurfaceData surfaceData = BuildSurfaceData(albedo);
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
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
                float _MaskDebugMode;
                float _GeneratedMassLocalMinY;
                float _GeneratedMassLocalHeight;
                float _GeneratedMassMaskSeed;
                float _GeneratedMassLocalXZScale;
                float _GeneratedMassMaskBaseLift;
                float _GeneratedMassCreviceReach;
                float _GeneratedMassCreviceBreakup;
                float _GeneratedMassDirtCrawlReach;
                float _GeneratedMassDirtCoverage;
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
                float _MaskDebugMode;
                float _GeneratedMassLocalMinY;
                float _GeneratedMassLocalHeight;
                float _GeneratedMassMaskSeed;
                float _GeneratedMassLocalXZScale;
                float _GeneratedMassMaskBaseLift;
                float _GeneratedMassCreviceReach;
                float _GeneratedMassCreviceBreakup;
                float _GeneratedMassDirtCrawlReach;
                float _GeneratedMassDirtCoverage;
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
                float _MaskDebugMode;
                float _GeneratedMassLocalMinY;
                float _GeneratedMassLocalHeight;
                float _GeneratedMassMaskSeed;
                float _GeneratedMassLocalXZScale;
                float _GeneratedMassMaskBaseLift;
                float _GeneratedMassCreviceReach;
                float _GeneratedMassCreviceBreakup;
                float _GeneratedMassDirtCrawlReach;
                float _GeneratedMassDirtCoverage;
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
