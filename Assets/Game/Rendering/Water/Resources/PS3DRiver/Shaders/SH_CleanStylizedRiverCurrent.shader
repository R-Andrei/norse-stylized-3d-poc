Shader "PS3D/Clean Stylized River Current"
{
    Properties
    {
        [HDR] _AccentColor("Accent Color", Color) = (1, 1, 1, 1)
        _AccentIntensity("Accent Intensity", Range(0, 2)) = 1
        _AccentOpacity("Accent Opacity", Range(0, 1)) = 0.92
        _EdgeSoftness("Edge Softness", Range(0, 1)) = 0.25

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
            Name "ForwardCurrent"

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

            CBUFFER_START(UnityPerMaterial)
                half4 _AccentColor;
                float _AccentIntensity;
                float _AccentOpacity;
                float _EdgeSoftness;
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
                float2 uv : TEXCOORD0;
                float4 color : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                output.positionHCS = positionInputs.positionCS;
                output.uv = input.uv;
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float edgeWidth = lerp(0.5, 0.05, saturate(_EdgeSoftness));

                float leftFade = smoothstep(0.0, edgeWidth, input.uv.x);
                float rightFade = smoothstep(0.0, edgeWidth, 1.0 - input.uv.x);
                float edgeFade = saturate(leftFade * rightFade);

                float endFade = saturate(input.color.b);

                float seed = input.color.r;
                float along = input.uv.y;

                float shimmer = sin(along * 34.0 + _RiverTime * 1.8 + seed * 13.37) * 0.5 + 0.5;
                float shimmerB = sin(along * 19.0 - _RiverTime * 1.1 + seed * 21.19) * 0.5 + 0.5;
                float breakup = lerp(0.82, 1.0, shimmer * 0.6 + shimmerB * 0.4);

                half3 color = _AccentColor.rgb * (_AccentIntensity * breakup);
                color = MixFog(color, input.fogFactor);

                half alpha = _AccentOpacity * edgeFade * endFade;
                return half4(color, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
