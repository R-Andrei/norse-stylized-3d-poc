Shader "PS3D/Weather/Weather Wind Trails"
{
    Properties
    {
        [MainColor] _TrailColor("Trail Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [Toggle] _UniformBodyOpacity("Uniform Body Opacity", Float) = 1
        _EdgeSoftness("Edge Softness", Range(0.01, 1.0)) = 0.15
        _StrengthOpacityInfluence("Strength Opacity Influence", Range(0, 1)) = 0
        _VariationOpacityInfluence("Variation Opacity Influence", Range(0, 1)) = 0
        [HideInInspector] _TrailPresentationTime("Trail Presentation Time", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "WindTrails"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _TrailColor;
                float _UniformBodyOpacity;
                float _EdgeSoftness;
                float _StrengthOpacityInfluence;
                float _VariationOpacityInfluence;
                float _TrailPresentationTime;
            CBUFFER_END

            struct Attributes
            {
                float3 positionWS : POSITION;
                float3 tangentWS : NORMAL;
                float2 signedHalfWidthAndDistance : TEXCOORD0;
                float4 lifecycleMotion : TEXCOORD1;
                float4 lifecycleTiming : TEXCOORD2;
                float4 presentation : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float ribbonSide : TEXCOORD0;
                float distanceMetres : TEXCOORD1;
                float4 lifecycleMotion : TEXCOORD2;
                float4 lifecycleTiming : TEXCOORD3;
                float4 presentation : TEXCOORD4;
            };

            float3 SafeNormalize(float3 value, float3 fallbackValue)
            {
                float magnitudeSquared = dot(value, value);
                return magnitudeSquared > 1e-8
                    ? value * rsqrt(magnitudeSquared)
                    : fallbackValue;
            }

            void ResolveVisibleInterval(
                float age,
                float4 lifecycleMotion,
                float4 lifecycleTiming,
                out float tailDistance,
                out float headDistance)
            {
                float travelSpeed = lifecycleMotion.y;
                float bodyLength = lifecycleMotion.z;
                float aliveDuration = lifecycleMotion.w;
                float spawnDuration = max(0.0001, lifecycleTiming.x);
                float despawnDuration = max(0.0001, lifecycleTiming.y);

                if (age < spawnDuration)
                {
                    float spawn01 = saturate(age / spawnDuration);
                    tailDistance = 0.0;
                    headDistance = bodyLength * spawn01;
                    return;
                }

                float aliveAge = age - spawnDuration;
                if (aliveAge < aliveDuration)
                {
                    tailDistance = travelSpeed * aliveAge;
                    headDistance = tailDistance + bodyLength;
                    return;
                }

                float despawnAge = min(
                    max(0.0, aliveAge - aliveDuration),
                    despawnDuration);
                float tipAllowance = bodyLength / (2.0 * despawnDuration);
                float aliveTravel = travelSpeed * aliveDuration;
                tailDistance = aliveTravel +
                    (travelSpeed + tipAllowance) * despawnAge;
                headDistance = bodyLength + aliveTravel +
                    (travelSpeed - tipAllowance) * despawnAge;
            }

            float EndpointTaper(
                float distanceMetres,
                float tailDistance,
                float headDistance,
                float pointedEndLength)
            {
                float visibleLength = max(0.0, headDistance - tailDistance);
                float effectiveTaperLength = max(
                    0.0001,
                    min(pointedEndLength, visibleLength * 0.5));
                float tailTaper = smoothstep(
                    0.0,
                    effectiveTaperLength,
                    distanceMetres - tailDistance);
                float headTaper = smoothstep(
                    0.0,
                    effectiveTaperLength,
                    headDistance - distanceMetres);
                return tailTaper * headTaper;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 tangentWS = SafeNormalize(
                    input.tangentWS,
                    float3(0.0, 0.0, 1.0));
                float3 viewDirectionWS = SafeNormalize(
                    GetCameraPositionWS() - input.positionWS,
                    float3(0.0, 1.0, 0.0));
                float3 sideWS = cross(tangentWS, viewDirectionWS);
                if (dot(sideWS, sideWS) <= 1e-8)
                {
                    float3 fallbackAxis = abs(tangentWS.y) < 0.95
                        ? float3(0.0, 1.0, 0.0)
                        : float3(1.0, 0.0, 0.0);
                    sideWS = cross(tangentWS, fallbackAxis);
                }

                sideWS = SafeNormalize(sideWS, float3(1.0, 0.0, 0.0));

                float age = max(
                    0.0,
                    _TrailPresentationTime - input.lifecycleMotion.x);
                float tailDistance;
                float headDistance;
                ResolveVisibleInterval(
                    age,
                    input.lifecycleMotion,
                    input.lifecycleTiming,
                    tailDistance,
                    headDistance);
                float clampedDistance = clamp(
                    input.signedHalfWidthAndDistance.y,
                    tailDistance,
                    headDistance);
                float widthScale = EndpointTaper(
                    clampedDistance,
                    tailDistance,
                    headDistance,
                    input.lifecycleTiming.z);
                float3 endpointClampedCentreWS = input.positionWS +
                    tangentWS * (
                        clampedDistance -
                        input.signedHalfWidthAndDistance.y);
                float3 expandedPositionWS = endpointClampedCentreWS +
                    sideWS * input.signedHalfWidthAndDistance.x * widthScale;

                output.positionCS = TransformWorldToHClip(expandedPositionWS);
                output.ribbonSide = sign(input.signedHalfWidthAndDistance.x);
                output.distanceMetres = clampedDistance;
                output.lifecycleMotion = input.lifecycleMotion;
                output.lifecycleTiming = input.lifecycleTiming;
                output.presentation = input.presentation;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                clip(input.presentation.a - 0.001);

                float age = _TrailPresentationTime - input.lifecycleMotion.x;
                clip(age);
                clip(input.lifecycleTiming.w - age);

                float tailDistance;
                float headDistance;
                ResolveVisibleInterval(
                    age,
                    input.lifecycleMotion,
                    input.lifecycleTiming,
                    tailDistance,
                    headDistance);
                clip(headDistance - input.distanceMetres);
                clip(input.distanceMetres - tailDistance);

                float endpointTaper = EndpointTaper(
                    input.distanceMetres,
                    tailDistance,
                    headDistance,
                    input.lifecycleTiming.z);
                float edgeCoordinate = abs(input.ribbonSide);
                float edgeAntialias = max(fwidth(edgeCoordinate), 0.0001);
                float edgeStart = saturate(1.0 - _EdgeSoftness);
                float softEdgeMask = 1.0 - smoothstep(
                    max(0.0, edgeStart - edgeAntialias),
                    1.0,
                    edgeCoordinate);
                float uniformEdgeMask = 1.0 - smoothstep(
                    max(0.0, 1.0 - edgeAntialias * 1.5),
                    1.0,
                    edgeCoordinate);
                float uniformMode = step(0.5, _UniformBodyOpacity);
                float spatialMask = lerp(
                    endpointTaper * softEdgeMask,
                    uniformEdgeMask,
                    uniformMode);

                float strengthFactor = lerp(
                    1.0,
                    saturate(input.presentation.g),
                    saturate(_StrengthOpacityInfluence));
                float variationInfluence = saturate(_VariationOpacityInfluence);
                float variationFactor = lerp(
                    1.0 - variationInfluence,
                    1.0 + variationInfluence,
                    saturate(input.presentation.b));

                float alpha = saturate(
                    _TrailColor.a *
                    input.presentation.r *
                    spatialMask *
                    strengthFactor *
                    variationFactor);
                float3 premultipliedColor = _TrailColor.rgb * alpha;
                return half4(premultipliedColor, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
