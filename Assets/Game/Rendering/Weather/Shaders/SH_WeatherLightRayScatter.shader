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
            Name "WeatherLightRaySecondaryHalo"
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
            #include "Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl"

            float4 Frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.texcoord;
                float2 acrossTexel =
                    _WeatherLightRaySofteningDirection.xy;
                if (dot(acrossTexel, acrossTexel) <= 1e-12)
                {
                    acrossTexel = float2(
                        _WeatherLightRaySofteningDirection.z,
                        0.0);
                }

                float softeningRadius = clamp(
                    _WeatherLightRaySofteningParameters.y,
                    1.5,
                    8.0);
                float2 nearOffset = acrossTexel *
                    (softeningRadius * 0.45);
                float2 farOffset = acrossTexel *
                    softeningRadius;
                float centre = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    screenUV).r;
                float negativeNear = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    screenUV - nearOffset).r;
                float positiveNear = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    screenUV + nearOffset).r;
                float negativeFar = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    screenUV - farOffset).r;
                float positiveFar = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    screenUV + farOffset).r;

                float negativeSource = max(
                    negativeNear,
                    negativeFar * 0.72);
                float positiveSource = max(
                    positiveNear,
                    positiveFar * 0.72);
                float dominantSource = max(
                    negativeSource,
                    positiveSource);
                float secondarySource = min(
                    negativeSource,
                    positiveSource);
                float dominantHalo = max(
                    0.0,
                    dominantSource - centre);
                float secondaryHalo = max(
                    0.0,
                    secondarySource - centre);
                float directionality = saturate(
                    abs(negativeSource - positiveSource) * 2.0);
                float halo = dominantHalo +
                    secondaryHalo * lerp(0.28, 0.08, directionality);
                float softeningStrength = saturate(
                    _WeatherLightRaySofteningParameters.x);
                float haloGain = lerp(
                    0.32,
                    0.78,
                    softeningStrength);
                float softened = saturate(
                    centre + halo * haloGain * softeningStrength);
                return float4(softened, 0.0, 0.0, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
