Shader "Hidden/PS3D/Weather LightRay Mask"
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
            Name "WeatherLightRayMask"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite Off
            ZTest Always
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fragment _ _LIGHT_COOKIES

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            #include "Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl"

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

            float4 Frag(Varyings input) : SV_Target
            {
                float2 screenUV = GetNormalizedScreenSpaceUV(
                    input.positionCS);
                float rawDepth = SampleSceneDepth(screenUV);
                float validDepth = WeatherLightRayRawDepthIsValid(rawDepth);
                float3 cameraPositionWS = GetCameraPositionWS();
                float3 viewRay = normalize(
                    input.positionWS - cameraPositionWS);

                float enterDistance;
                float exitDistance;
                if (!WeatherLightRayIntersectFrustum(
                        cameraPositionWS,
                        viewRay,
                        enterDistance,
                        exitDistance))
                {
                    return 0.0;
                }

                float3 scenePositionWS = 0.0;
                float sceneDistance = 1e20;
                if (validDepth > 0.5)
                {
                    scenePositionWS = WeatherLightRayReconstructWorldPosition(
                        screenUV,
                        rawDepth);
                    sceneDistance = distance(
                        cameraPositionWS,
                        scenePositionWS);
                    exitDistance = min(exitDistance, sceneDistance);
                }

                float3 scenePositionDx = ddx(scenePositionWS);
                float3 scenePositionDy = ddy(scenePositionWS);

                if (exitDistance <= enterDistance)
                {
                    return 0.0;
                }

                const int sampleCount = 8;
                float segmentLength = exitDistance - enterDistance;
                float strandDensitySum = 0.0;
                float envelopeDensitySum = 0.0;
                float cameraFade = WeatherLightRayCameraFade(
                    cameraPositionWS);
                [unroll]
                for (int sampleIndex = 0;
                    sampleIndex < sampleCount;
                    sampleIndex++)
                {
                    float sample01 =
                        (sampleIndex + 0.5) / sampleCount;
                    float3 samplePositionWS = cameraPositionWS +
                        viewRay * lerp(
                            enterDistance,
                            exitDistance,
                            sample01);
                    float envelope;
                    float axial01;
                    float radial01;
                    float strands = WeatherLightRayEvaluateStrands(
                        samplePositionWS,
                        1.0,
                        envelope,
                        axial01,
                        radial01);
                    float heightFade =
                        WeatherLightRayAtmosphericHeightFade(axial01);
                    strandDensitySum += strands * heightFade * cameraFade;
                    envelopeDensitySum += envelope * heightFade * cameraFade;
                }

                float averageStrandDensity =
                    strandDensitySum / sampleCount;
                float averageEnvelopeDensity =
                    envelopeDensitySum / sampleCount;
                float strandAtmosphere = 1.0 - exp2(
                    -averageStrandDensity * segmentLength * 0.2);
                float envelopeHaze = 1.0 - exp2(
                    -averageEnvelopeDensity * segmentLength * 0.045);

                float surface = 0.0;
                float compensation = 0.0;
                if (validDepth > 0.5 &&
                    sceneDistance <= exitDistance + 0.01)
                {
                    float surfaceStrands;
                    float surfaceEnvelope;
                    float surfaceAxial01;
                    float surfaceRadial01;
                    float surfaceBase = WeatherLightRayEvaluateSurface(
                        scenePositionWS,
                        surfaceStrands,
                        surfaceEnvelope,
                        surfaceAxial01,
                        surfaceRadial01);
                    float3 normalVector = cross(
                        scenePositionDy,
                        scenePositionDx);
                    float groundAlignment = abs(normalVector.y) * rsqrt(
                        max(1e-8, dot(normalVector, normalVector)));
                    float groundWeight = smoothstep(
                        0.35,
                        0.8,
                        groundAlignment);
                    float illuminationStrength = lerp(
                        _WeatherLightRayIllumination.y,
                        _WeatherLightRayIllumination.x,
                        groundWeight);
                    surface = surfaceBase * illuminationStrength;

                    if (_WeatherLightRayTopShape.w > 0.5 &&
                        _WeatherLightRayCloudParameters.z > 0.5)
                    {
                        float transmission = 1.0;
                        #if defined(_LIGHT_COOKIES)
                            transmission =
                                SampleMainLightCookie(scenePositionWS).r;
                        #endif
                        float transmissionRange = max(
                            0.0001,
                            1.0 - _WeatherLightRayCloudParameters.x);
                        compensation = surfaceBase *
                            illuminationStrength *
                            _WeatherLightRayIllumination.z *
                            saturate(
                                (1.0 - transmission) /
                                transmissionRange);
                    }
                }

                float authoritative = saturate(
                    _WeatherLightRayIntensity.x);
                return float4(
                    saturate(strandAtmosphere * authoritative),
                    saturate(envelopeHaze * authoritative),
                    max(0.0, surface * authoritative),
                    max(0.0, compensation * authoritative));
            }
            ENDHLSL
        }
    }

    FallBack Off
}
