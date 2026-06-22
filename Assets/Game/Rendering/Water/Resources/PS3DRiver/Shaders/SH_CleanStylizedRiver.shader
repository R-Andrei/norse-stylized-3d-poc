Shader "PS3D/Clean Stylized River"
{
    Properties
    {
        [Header(Palette)]
        [HDR] _ShallowColor("Shallow Color", Color) = (0.42, 0.73, 0.73, 1)
        [HDR] _DeepColor("Deep Color", Color) = (0.12, 0.42, 0.48, 1)
        [HDR] _FlowTint("Flow Tint", Color) = (0.72, 0.92, 0.88, 1)
        _Opacity("Opacity", Range(0.15, 1)) = 0.72

        [Header(Flow)]
        _FlowTex("Flow Texture", 2D) = "gray" {}
        _DetailTex("Detail Texture", 2D) = "gray" {}
        _FlowScale("Flow Scale", Float) = 4.5
        _FlowStrength("Flow Strength", Range(0, 1)) = 0.32
        _DetailScale("Detail Scale", Float) = 0.85
        _DetailStrength("Detail Strength", Range(0, 1)) = 0.38
        _WaveHeight("Wave Height", Range(0, 0.16)) = 0.035
        _BankLight("Bank Light", Range(0, 1)) = 0.35
        _LightingSteps("Lighting Steps", Range(1, 6)) = 3

        [HideInInspector] _FlowDistance("Flow Distance", Float) = 0
        [HideInInspector] _RiverTime("River Time", Float) = 0
        [HideInInspector] _VisualSeed("Visual Seed", Float) = 1731
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardRiver"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_FlowTex);
            SAMPLER(sampler_FlowTex);

            TEXTURE2D(_DetailTex);
            SAMPLER(sampler_DetailTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _FlowTint;
                float _Opacity;

                float _FlowScale;
                float _FlowStrength;
                float _DetailScale;
                float _DetailStrength;
                float _WaveHeight;
                float _BankLight;
                float _LightingSteps;

                float _FlowDistance;
                float _RiverTime;
                float _VisualSeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float2 uv : TEXCOORD4;
                float4 color : TEXCOORD5;
                half fogFactor : TEXCOORD6;
            };

            float SampleDetailHeight(
                float2 riverUv)
            {
                float safeScale =
                    max(
                        0.05,
                        _DetailScale);

                float2 uv =
                    float2(
                        riverUv.x * 2.25 +
                            _VisualSeed * 0.00031,
                        (riverUv.y -
                         _FlowDistance * 0.52) /
                            safeScale +
                            _VisualSeed * 0.00017);

                float first =
                    SAMPLE_TEXTURE2D_LOD(
                        _DetailTex,
                        sampler_DetailTex,
                        uv,
                        0).r;

                float second =
                    SAMPLE_TEXTURE2D_LOD(
                        _DetailTex,
                        sampler_DetailTex,
                        uv *
                            float2(
                                1.83,
                                1.29) +
                            float2(
                                0.37,
                                0.19),
                        0).r;

                return
                    first * 0.66 +
                    second * 0.34;
            }

            Varyings Vert(
                Attributes input)
            {
                Varyings output;

                float3 positionOS =
                    input.positionOS.xyz;

                float bankFade =
                    saturate(
                        1.0 -
                        abs(
                            input.uv.x *
                                2.0 -
                            1.0));

                bankFade =
                    smoothstep(
                        0.0,
                        0.35,
                        bankFade);

                float detailHeight =
                    SampleDetailHeight(
                        input.uv);

                float slowUndulation =
                    sin(
                        input.uv.y * 0.62 -
                        _RiverTime * 0.75 +
                        input.uv.x * 2.7 +
                        _VisualSeed * 0.011) *
                    0.5 +
                    0.5;

                float displacement =
                    ((detailHeight - 0.5) * 0.72 +
                     (slowUndulation - 0.5) * 0.28) *
                    _WaveHeight *
                    bankFade;

                positionOS.y += displacement;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(
                        positionOS);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(
                        input.normalOS,
                        input.tangentOS);

                output.positionHCS =
                    positionInputs.positionCS;

                output.positionWS =
                    positionInputs.positionWS;

                output.normalWS =
                    normalInputs.normalWS;

                output.tangentWS =
                    normalInputs.tangentWS;

                output.bitangentWS =
                    normalInputs.bitangentWS;

                output.uv = input.uv;
                output.color = input.color;

                output.fogFactor =
                    ComputeFogFactor(
                        positionInputs.positionCS.z);

                return output;
            }

            half4 Frag(
                Varyings input) : SV_Target
            {
                float safeFlowScale =
                    max(
                        0.05,
                        _FlowScale);

                float safeDetailScale =
                    max(
                        0.05,
                        _DetailScale);

                float acrossSigned =
                    input.uv.x * 2.0 - 1.0;

                float centreAmount =
                    pow(
                        saturate(
                            1.0 -
                            abs(acrossSigned)),
                        0.72);

                float bankAmount =
                    1.0 -
                    centreAmount;

                float boundedWarp =
                    sin(
                        input.uv.y * 0.19 +
                        _VisualSeed * 0.013) *
                    0.055;

                // Provisional Body Flow sampling: the current source texture repeats along local cumulative
                // spline distance, and that repetition is a known visual limitation. The future replacement
                // must sample a fixed-world-scale procedural field using global connected-river distance.
                // Do not normalize by total river length, and do not generate one unrelated mask per chunk.
                float2 flowUvA =
                    float2(
                        input.uv.x * 1.55 +
                            boundedWarp,
                        (input.uv.y -
                         _FlowDistance) /
                            safeFlowScale);

                float2 flowUvB =
                    float2(
                        input.uv.x * 2.35 -
                            boundedWarp * 0.7 +
                            0.31,
                        (input.uv.y -
                         _FlowDistance * 0.74) /
                            (safeFlowScale * 0.63) +
                            0.17);

                float flowA =
                    SAMPLE_TEXTURE2D(
                        _FlowTex,
                        sampler_FlowTex,
                        flowUvA).r;

                float flowB =
                    SAMPLE_TEXTURE2D(
                        _FlowTex,
                        sampler_FlowTex,
                        flowUvB).r;

                float flowPattern =
                    saturate(
                        flowA * 0.68 +
                        flowB * 0.32);

                float2 detailUv =
                    float2(
                        input.uv.x * 2.4 +
                            _VisualSeed * 0.00023,
                        (input.uv.y -
                         _FlowDistance * 0.52) /
                            safeDetailScale);

                float detail =
                    SAMPLE_TEXTURE2D(
                        _DetailTex,
                        sampler_DetailTex,
                        detailUv).r;

                float2 texel =
                    float2(
                        0.008,
                        0.014);

                float detailLeft =
                    SAMPLE_TEXTURE2D(
                        _DetailTex,
                        sampler_DetailTex,
                        detailUv -
                            float2(
                                texel.x,
                                0)).r;

                float detailRight =
                    SAMPLE_TEXTURE2D(
                        _DetailTex,
                        sampler_DetailTex,
                        detailUv +
                            float2(
                                texel.x,
                                0)).r;

                float detailDown =
                    SAMPLE_TEXTURE2D(
                        _DetailTex,
                        sampler_DetailTex,
                        detailUv -
                            float2(
                                0,
                                texel.y)).r;

                float detailUp =
                    SAMPLE_TEXTURE2D(
                        _DetailTex,
                        sampler_DetailTex,
                        detailUv +
                            float2(
                                0,
                                texel.y)).r;

                float3 detailNormalTS =
                    normalize(
                        float3(
                            (detailLeft -
                             detailRight) *
                                2.8 *
                                _DetailStrength,
                            (detailDown -
                             detailUp) *
                                2.8 *
                                _DetailStrength,
                            1.0));

                half3 normalWS =
                    normalize(
                        input.tangentWS *
                            detailNormalTS.x +
                        input.bitangentWS *
                            detailNormalTS.y +
                        input.normalWS *
                            detailNormalTS.z);

                half3 baseColor =
                    lerp(
                        _ShallowColor.rgb,
                        _DeepColor.rgb,
                        centreAmount);

                baseColor =
                    lerp(
                        baseColor,
                        baseColor +
                            _FlowTint.rgb *
                            0.34,
                        bankAmount *
                        _BankLight);

                float signedFlow =
                    (flowPattern - 0.5) *
                    2.0;

                half3 color =
                    baseColor +
                    _FlowTint.rgb *
                    signedFlow *
                    _FlowStrength *
                    0.36;

                color +=
                    _FlowTint.rgb *
                    (detail - 0.5) *
                    _DetailStrength *
                    0.12;

                Light mainLight =
                    GetMainLight();

                half wrappedLight =
                    saturate(
                        (dot(
                            normalWS,
                            mainLight.direction) +
                         0.42) /
                        1.42);

                float steps =
                    max(
                        1.0,
                        _LightingSteps);

                half quantizedLight =
                    floor(
                        wrappedLight *
                            steps +
                        0.5) /
                    steps;

                half3 ambient =
                    max(
                        SampleSH(
                            normalWS),
                        half3(
                            0.03,
                            0.03,
                            0.03));

                color *=
                    ambient * 0.62 +
                    mainLight.color *
                    quantizedLight *
                    0.78 +
                    half3(
                        0.20,
                        0.20,
                        0.20);

                half3 viewDirectionWS =
                    GetWorldSpaceNormalizeViewDir(
                        input.positionWS);

                half fresnel =
                    pow(
                        saturate(
                            1.0 -
                            dot(
                                normalWS,
                                viewDirectionWS)),
                        3.0);

                color +=
                    _FlowTint.rgb *
                    fresnel *
                    0.10;

                color =
                    MixFog(
                        color,
                        input.fogFactor);

                half alpha =
                    _Opacity *
                    lerp(
                        0.86,
                        1.0,
                        centreAmount);

                return
                    half4(
                        color,
                        alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
