Shader "Hidden/PS3D/Weather LightRay Composite"
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
            Name "WeatherLightRayComposite"
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

            TEXTURE2D_X(_WeatherLightRayMaskTexture);
            TEXTURE2D_X(_WeatherLightRayScatterTexture);

            float4 WeatherLightRaySampleMaskDepthAware(float2 screenUV)
            {
                float rawCentreDepth = SampleSceneDepth(screenUV);
                float centreValid = WeatherLightRayRawDepthIsValid(
                    rawCentreDepth);
                float centreEyeDepth = centreValid > 0.5
                    ? LinearEyeDepth(rawCentreDepth, _ZBufferParams)
                    : 1e20;
                float2 texel = _WeatherLightRayScatterDirection.zw;
                float4 accumulated = 0.0;
                float accumulatedWeight = 0.0;
                [unroll]
                for (int index = 0; index < 4; index++)
                {
                    float2 sampleOffset = float2(
                        (index & 1) != 0 ? 0.5 : -0.5,
                        (index & 2) != 0 ? 0.5 : -0.5);
                    float2 sampleUV = screenUV + sampleOffset * texel;
                    float4 mask = SAMPLE_TEXTURE2D_X(
                        _WeatherLightRayMaskTexture,
                        sampler_LinearClamp,
                        sampleUV);
                    float rawSampleDepth = SampleSceneDepth(sampleUV);
                    float sampleValid = WeatherLightRayRawDepthIsValid(
                        rawSampleDepth);
                    float depthWeight = 1.0;
                    if (centreValid > 0.5 && sampleValid > 0.5)
                    {
                        float sampleEyeDepth = LinearEyeDepth(
                            rawSampleDepth,
                            _ZBufferParams);
                        float threshold = max(
                            0.35,
                            centreEyeDepth * 0.02);
                        depthWeight = exp2(
                            -abs(sampleEyeDepth - centreEyeDepth) /
                            threshold * 4.0);
                    }
                    accumulated += mask * depthWeight;
                    accumulatedWeight += depthWeight;
                }

                return accumulated / max(0.0001, accumulatedWeight);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.texcoord;
                float4 source = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    screenUV);
                float4 mask = WeatherLightRaySampleMaskDepthAware(screenUV);
                float scatter = SAMPLE_TEXTURE2D_X(
                    _WeatherLightRayScatterTexture,
                    sampler_LinearClamp,
                    screenUV).r;

                if (_WeatherLightRayDebugMode > 0.5 &&
                    _WeatherLightRayDebugMode < 1.5)
                {
                    return float4(mask.rrr, 1.0);
                }
                if (_WeatherLightRayDebugMode > 1.5 &&
                    _WeatherLightRayDebugMode < 2.5)
                {
                    return float4(0.0, mask.b, 0.0, 1.0);
                }
                if (_WeatherLightRayDebugMode > 2.5 &&
                    _WeatherLightRayDebugMode < 3.5)
                {
                    return float4(mask.a, 0.0, mask.a, 1.0);
                }
                if (_WeatherLightRayDebugMode > 3.5 &&
                    _WeatherLightRayDebugMode < 4.5)
                {
                    return float4(scatter.xxx, 1.0);
                }
                if (_WeatherLightRayDebugMode > 4.5)
                {
                    return float4(mask.ggg, 1.0);
                }

                float3 rayColour = max(
                    0.0,
                    _WeatherLightRayColour.rgb);
                float structuredAtmosphere = max(mask.r, scatter);
                float3 contribution = rayColour * (
                    structuredAtmosphere * _WeatherLightRayIntensity.y +
                    mask.g * _WeatherLightRayIntensity.z +
                    mask.b * _WeatherLightRayIntensity.w +
                    mask.a * _WeatherLightRayCloudParameters.w);
                return float4(source.rgb + contribution, source.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
