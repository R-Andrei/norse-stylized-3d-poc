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

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #define WEATHER_LIGHT_RAY_ENABLE_DEPTH_EVALUATION 1
        #include "Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl"

        TEXTURE2D_X(_WeatherLightRayMaskTexture);
        TEXTURE2D_X(_WeatherLightRaySoftenedTexture);
        float WeatherLightRaySampleRaw(float2 screenUV)
        {
            return SAMPLE_TEXTURE2D_X(
                _WeatherLightRayMaskTexture,
                sampler_PointClamp,
                screenUV).r;
        }

        float WeatherLightRaySampleSoftened(float2 screenUV)
        {
            return SAMPLE_TEXTURE2D_X(
                _WeatherLightRaySoftenedTexture,
                sampler_LinearClamp,
                screenUV).r;
        }

        ENDHLSL

        Pass
        {
            Name "WeatherLightRayContinuousBeamComposite"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment FragComposite

            float4 FragComposite(Varyings input) : SV_Target
            {
                float2 screenUV = input.texcoord;
                float4 source = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    screenUV);
                float rawBeam =
                    WeatherLightRaySampleRaw(screenUV);
                float softenedBeam =
                    WeatherLightRaySampleSoftened(screenUV);
                float surfaceInfluence =
                    WeatherLightRayEvaluateSurfaceInfluence(screenUV);

                if (_WeatherLightRayDebugMode > 0.5 &&
                    _WeatherLightRayDebugMode < 1.5)
                {
                    return float4(rawBeam.xxx, 1.0);
                }
                if (_WeatherLightRayDebugMode > 1.5 &&
                    _WeatherLightRayDebugMode < 2.5)
                {
                    float boundaryMarker;
                    float diameterMarker;
                    float endpointMarker;
                    float centreMarker;
                    WeatherLightRayEvaluateFootprintMarkers(
                        screenUV,
                        boundaryMarker,
                        diameterMarker,
                        endpointMarker,
                        centreMarker);
                    float3 footprintColour = float3(
                        0.0,
                        surfaceInfluence,
                        surfaceInfluence * 0.65);
                    float3 markerColour = max(
                        float3(
                            max(boundaryMarker, diameterMarker),
                            0.0,
                            centreMarker),
                        float3(
                            endpointMarker,
                            endpointMarker,
                            0.0));
                    return float4(
                        max(footprintColour, markerColour),
                        1.0);
                }
                if (_WeatherLightRayDebugMode > 3.5 &&
                    _WeatherLightRayDebugMode < 4.5)
                {
                    return float4(softenedBeam.xxx, 1.0);
                }

                float3 rayColour = max(
                    0.0,
                    _WeatherLightRayColour.rgb);
                float3 atmosphereContribution = rayColour *
                    softenedBeam *
                    max(0.0, _WeatherLightRayIntensity.y);
                float3 positiveRayColour = max(
                    0.0,
                    rayColour);
                float maximumRayChannel = max(
                    positiveRayColour.r,
                    max(positiveRayColour.g, positiveRayColour.b));
                float3 normalizedRayColour = maximumRayChannel > 0.0001
                    ? positiveRayColour / maximumRayChannel
                    : 0.0;
                float3 boundedSceneColour = saturate(source.rgb);
                float3 fullPowerSurfaceTarget = 1.0 -
                    (1.0 - boundedSceneColour) *
                    (1.0 - normalizedRayColour * 0.28);
                float3 boundedSurfaceLift = max(
                    0.0,
                    fullPowerSurfaceTarget - boundedSceneColour);
                float3 surfaceLitScene = source.rgb +
                    boundedSurfaceLift * saturate(surfaceInfluence);
                return float4(
                    surfaceLitScene + atmosphereContribution,
                    source.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
