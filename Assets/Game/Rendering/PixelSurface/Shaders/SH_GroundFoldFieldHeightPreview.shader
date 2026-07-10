Shader "Hidden/PS3D/Ground Fold Field Height Preview"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.82, 0.82, 0.72, 1)
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
            Name "FoldFieldHeightPreview"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS =
                    TransformObjectToHClip(input.positionOS);
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half height = saturate(input.color.r);
                half3 lowColor = half3(0.16, 0.26, 0.34);
                half3 midColor = half3(0.48, 0.55, 0.42);
                half3 highColor = half3(0.94, 0.82, 0.36);

                half3 color =
                    lerp(
                        lowColor,
                        midColor,
                        saturate(height * 2.0));
                color =
                    lerp(
                        color,
                        highColor,
                        saturate((height - 0.5) * 2.0));

                color =
                    lerp(
                        color,
                        _BaseColor.rgb,
                        0.18);

                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
