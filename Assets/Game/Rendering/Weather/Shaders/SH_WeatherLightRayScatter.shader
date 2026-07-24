Shader "Hidden/PS3D/Weather LightRay Scatter"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Overlay"
        }

        Pass
        {
            Name "WeatherLightRayScatter"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl"

            float4 Frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.texcoord;
                float centreValue = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    screenUV).r;
                float rawCentreDepth = SampleSceneDepth(screenUV);
                float centreValid = WeatherLightRayRawDepthIsValid(
                    rawCentreDepth);
                float centreEyeDepth = centreValid > 0.5
                    ? LinearEyeDepth(rawCentreDepth, _ZBufferParams)
                    : 1e20;
                float scatterLength = max(
                    0.0,
                    _WeatherLightRayScatterParameters.x);
                float scatterSoftness = saturate(
                    _WeatherLightRayScatterParameters.y);

                float accumulated = 0.0;
                float accumulatedWeight = 0.0;
                [unroll]
                for (int tapIndex = 0; tapIndex < 7; tapIndex++)
                {
                    float signedTap = tapIndex - 3.0;
                    float2 tapUV = screenUV +
                        _WeatherLightRayScatterDirection.xy *
                        _WeatherLightRayScatterDirection.zw *
                        signedTap * scatterLength;
                    float sampleValue = SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_LinearClamp,
                        tapUV).r;
                    float rawTapDepth = SampleSceneDepth(tapUV);
                    float tapValid = WeatherLightRayRawDepthIsValid(
                        rawTapDepth);
                    float depthWeight = 1.0;
                    if (centreValid > 0.5 && tapValid > 0.5)
                    {
                        float tapEyeDepth = LinearEyeDepth(
                            rawTapDepth,
                            _ZBufferParams);
                        float threshold = max(
                            0.35,
                            centreEyeDepth * 0.02);
                        depthWeight = exp2(
                            -abs(tapEyeDepth - centreEyeDepth) /
                            threshold * 4.0);
                    }

                    float tapWeight = (1.0 - abs(signedTap) / 4.0) *
                        depthWeight;
                    accumulated += sampleValue * tapWeight;
                    accumulatedWeight += tapWeight;
                }

                float filtered = accumulated /
                    max(0.0001, accumulatedWeight);
                float scattered = max(
                    centreValue,
                    lerp(centreValue, filtered, scatterSoftness));
                return float4(scattered, 0.0, 0.0, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
