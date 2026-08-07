Shader "Hidden/PS3D/Generated Mass Triangle Identity Audit"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "TriangleIdentityAudit"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual
            Blend One Zero
            ColorMask RGB

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 identity : COLOR0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.identity = input.color;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                return float4(input.identity.rgb, 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
