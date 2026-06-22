Shader "PS3D/Clean Stylized River Foam"
{
    Properties
    {
        [HDR] _FoamColor("Foam Color", Color) = (0.72, 0.93, 0.88, 1)
        [HDR] _FoamHighlight("Foam Highlight", Color) = (0.92, 1.0, 0.96, 1)
        _FoamTex("Foam Texture", 2D) = "white" {}
        _Alpha("Alpha", Range(0, 1)) = 0.86
        _EdgeSoftness("Edge Softness", Range(0.01, 0.35)) = 0.12

        [HideInInspector] _FoamIntensity("Foam Intensity", Float) = 1
        [HideInInspector] _ContactFoam("Contact Foam", Range(0, 1)) = 0.82
        [HideInInspector] _WakeFoam("Wake Foam", Range(0, 1)) = 0.72
        [HideInInspector] _FoamBreakup("Foam Breakup", Range(0, 1)) = 0.58
        [HideInInspector] _FoamMotion("Foam Motion", Float) = 1
        [HideInInspector] _FlowDistance("Flow Distance", Float) = 0
        [HideInInspector] _RiverTime("River Time", Float) = 0
        [HideInInspector] _VisualSeed("Visual Seed", Float) = 1731
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+20"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardFoam"

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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_FoamTex);
            SAMPLER(sampler_FoamTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _FoamColor;
                half4 _FoamHighlight;
                float _Alpha;
                float _EdgeSoftness;

                float _FoamIntensity;
                float _ContactFoam;
                float _WakeFoam;
                float _FoamBreakup;
                float _FoamMotion;
                float _FlowDistance;
                float _RiverTime;
                float _VisualSeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 metadata : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            Varyings Vert(
                Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(
                        input.positionOS.xyz);

                output.positionHCS =
                    positionInputs.positionCS;

                output.positionWS =
                    positionInputs.positionWS;

                output.uv = input.uv;
                output.metadata = input.color;

                output.fogFactor =
                    ComputeFogFactor(
                        positionInputs.positionCS.z);

                return output;
            }

            half4 Frag(
                Varyings input) : SV_Target
            {
                float foamType =
                    saturate(
                        input.metadata.r);

                float randomValue =
                    input.metadata.g +
                    _VisualSeed * 0.00017;

                float interactionStrength =
                    saturate(
                        input.metadata.b);

                float acrossEdge =
                    smoothstep(
                        0.0,
                        max(
                            0.01,
                            _EdgeSoftness),
                        min(
                            input.uv.x,
                            1.0 -
                            input.uv.x));

                float endFade =
                    smoothstep(
                        0.0,
                        0.12,
                        input.uv.y) *
                    smoothstep(
                        0.0,
                        0.18,
                        2.0 -
                        input.uv.y);

                float edgeMask =
                    lerp(
                        acrossEdge,
                        acrossEdge *
                            endFade,
                        foamType);

                float movingAlong =
                    input.uv.y *
                        lerp(
                            2.4,
                            3.8,
                            foamType) -
                    _FlowDistance *
                        lerp(
                            0.16,
                            0.62,
                            foamType) *
                        _FoamMotion;

                float2 foamUvA =
                    float2(
                        input.uv.x * 2.2 +
                            randomValue * 7.0,
                        movingAlong +
                            _RiverTime *
                            0.16 *
                            _FoamMotion);

                float2 foamUvB =
                    float2(
                        input.uv.x * 4.1 +
                            randomValue * 11.0 +
                            0.37,
                        movingAlong * 1.47 -
                            _RiverTime *
                            0.11 *
                            _FoamMotion +
                            0.19);

                float foamA =
                    SAMPLE_TEXTURE2D(
                        _FoamTex,
                        sampler_FoamTex,
                        foamUvA).r;

                float foamB =
                    SAMPLE_TEXTURE2D(
                        _FoamTex,
                        sampler_FoamTex,
                        foamUvB).r;

                float foamPattern =
                    saturate(
                        foamA * 0.72 +
                        foamB * 0.28);

                float breakupMask =
                    smoothstep(
                        lerp(
                            0.03,
                            0.48,
                            _FoamBreakup),
                        lerp(
                            0.18,
                            0.68,
                            _FoamBreakup),
                        foamPattern);

                float typeStrength =
                    lerp(
                        _ContactFoam,
                        _WakeFoam,
                        foamType);

                float alpha =
                    _Alpha *
                    _FoamIntensity *
                    typeStrength *
                    interactionStrength *
                    edgeMask *
                    lerp(
                        1.0,
                        breakupMask,
                        _FoamBreakup);

                clip(
                    alpha -
                    0.01);

                float highlight =
                    saturate(
                        foamPattern * 0.75 +
                        sin(
                            _RiverTime * 2.1 +
                            randomValue * 6.28318 +
                            input.uv.y * 4.0) *
                            0.125 +
                        0.125);

                half3 color =
                    lerp(
                        _FoamColor.rgb,
                        _FoamHighlight.rgb,
                        highlight * 0.56);

                Light mainLight =
                    GetMainLight();

                half3 ambient =
                    max(
                        SampleSH(
                            half3(
                                0,
                                1,
                                0)),
                        half3(
                            0.08,
                            0.08,
                            0.08));

                color *=
                    ambient * 0.55 +
                    mainLight.color * 0.62 +
                    half3(
                        0.25,
                        0.25,
                        0.25);

                color =
                    MixFog(
                        color,
                        input.fogFactor);

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
