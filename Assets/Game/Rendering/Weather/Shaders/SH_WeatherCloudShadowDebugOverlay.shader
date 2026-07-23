Shader "Hidden/PS3D/Weather Cloud Shadow Debug Overlay"
{
    Properties
    {
        _CloudDebugMode("Debug Mode", Float) = 2
        _CloudDebugOpacity("Overlay Opacity", Range(0, 1)) = 0.55
        _CloudDebugCloudColor("Cloud Color", Color) = (1, 0, 0.75, 1)
        _CloudDebugOpeningColor("Opening Color", Color) = (0, 0.85, 1, 1)
        _CloudDebugShadedTransmission("Shaded Transmission", Range(0, 1)) = 0.62
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay+20"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "WeatherCloudShadowDebugOverlay"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fragment _ _LIGHT_COOKIES

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _CloudDebugMode;
                half _CloudDebugOpacity;
                half4 _CloudDebugCloudColor;
                half4 _CloudDebugOpeningColor;
                half _CloudDebugShadedTransmission;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half transmission = 1.0h;
                #if defined(_LIGHT_COOKIES)
                    transmission =
                        SampleMainLightCookie(input.positionWS).r;
                #endif

                half transmissionRange = max(
                    1.0h - _CloudDebugShadedTransmission,
                    0.0001h);
                half cloudAmount = saturate(
                    (1.0h - transmission) / transmissionRange);

                if (_CloudDebugMode < 1.5h)
                {
                    half alpha = cloudAmount * _CloudDebugOpacity;
                    clip(alpha - 0.001h);
                    return half4(_CloudDebugCloudColor.rgb, alpha);
                }

                half3 colour = lerp(
                    _CloudDebugOpeningColor.rgb,
                    _CloudDebugCloudColor.rgb,
                    cloudAmount);
                return half4(colour, _CloudDebugOpacity);
            }
            ENDHLSL
        }
    }
}
