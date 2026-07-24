#ifndef PS3D_WEATHER_LIGHT_RAY_COMMON_INCLUDED
#define PS3D_WEATHER_LIGHT_RAY_COMMON_INCLUDED

float4 _WeatherLightRayBaseCentreHeight;
float4 _WeatherLightRayDirectionBaseRadius;
float4 _WeatherLightRayTopShape;
float4 _WeatherLightRayColour;
float4 _WeatherLightRayIntensity;
float4 _WeatherLightRayCloudParameters;
float4 _WeatherLightRayStrandShape0;
float4 _WeatherLightRayStrandShape1;
float4 _WeatherLightRayStrandShape2;
float4 _WeatherLightRayEvolution0;
float4 _WeatherLightRayEvolution1;
float4 _WeatherLightRaySurfaceShape;
float4 _WeatherLightRayIllumination;
float4 _WeatherLightRayScatterDirection;
float4 _WeatherLightRayScatterParameters;
float _WeatherLightRayDebugMode;

static const float WEATHER_LIGHT_RAY_PI = 3.14159265359;
static const int WEATHER_LIGHT_RAY_MAX_STRANDS = 8;

float WeatherLightRayRawDepthIsValid(float rawDepth)
{
    #if UNITY_REVERSED_Z
        return step(0.00001, rawDepth);
    #else
        return 1.0 - step(0.99999, rawDepth);
    #endif
}

float3 WeatherLightRayReconstructWorldPosition(
    float2 screenUV,
    float rawDepth)
{
    float deviceDepth = rawDepth;
    #if !UNITY_REVERSED_Z
        deviceDepth = lerp(
            UNITY_NEAR_CLIP_VALUE,
            1.0,
            rawDepth);
    #endif

    return ComputeWorldSpacePosition(
        screenUV,
        deviceDepth,
        UNITY_MATRIX_I_VP);
}

float WeatherLightRayHash11(float value)
{
    return frac(sin(value * 12.9898 + 78.233) * 43758.5453);
}

float2 WeatherLightRayHash21(float value)
{
    return frac(sin(float2(
        value * 12.9898 + 17.17,
        value * 78.233 + 41.73)) * 43758.5453);
}

void WeatherLightRayBuildBasis(
    float3 upwardAxis,
    out float3 tangent,
    out float3 bitangent)
{
    float3 referenceAxis = abs(upwardAxis.y) < 0.92
        ? float3(0.0, 1.0, 0.0)
        : float3(1.0, 0.0, 0.0);
    tangent = normalize(cross(referenceAxis, upwardAxis));
    bitangent = normalize(cross(upwardAxis, tangent));
}

void WeatherLightRayGetLocalCoordinates(
    float3 positionWS,
    out float axial01,
    out float2 radialCoordinates,
    out float localRadius,
    out float radial01)
{
    float3 baseCentre = _WeatherLightRayBaseCentreHeight.xyz;
    float height = max(0.001, _WeatherLightRayBaseCentreHeight.w);
    float3 upwardAxis = normalize(
        -_WeatherLightRayDirectionBaseRadius.xyz);
    float3 tangent;
    float3 bitangent;
    WeatherLightRayBuildBasis(upwardAxis, tangent, bitangent);

    float3 relative = positionWS - baseCentre;
    float axialDistance = dot(relative, upwardAxis);
    axial01 = axialDistance / height;
    float baseRadius = max(
        0.001,
        _WeatherLightRayDirectionBaseRadius.w);
    float topRadius = max(0.001, _WeatherLightRayTopShape.x);
    localRadius = lerp(
        baseRadius,
        topRadius,
        saturate(axial01));
    float3 radialVector = relative - upwardAxis * axialDistance;
    radialCoordinates = float2(
        dot(radialVector, tangent),
        dot(radialVector, bitangent));
    radial01 = length(radialCoordinates) / max(0.001, localRadius);
}

float WeatherLightRaySoftRadialMask(
    float radial01,
    float edgeSoftness)
{
    float softness = max(0.001, edgeSoftness);
    return 1.0 - smoothstep(
        max(0.0, 1.0 - softness),
        1.0,
        radial01);
}

float WeatherLightRayEnvelope(
    float3 positionWS,
    float edgeSoftness,
    out float axial01,
    out float radial01)
{
    float2 radialCoordinates;
    float localRadius;
    WeatherLightRayGetLocalCoordinates(
        positionWS,
        axial01,
        radialCoordinates,
        localRadius,
        radial01);
    float envelopeScale = max(0.1, _WeatherLightRayTopShape.y);
    float envelopeRadial = radial01 / envelopeScale;
    float axialMask = step(0.0, axial01) * step(axial01, 1.0);
    return axialMask * WeatherLightRaySoftRadialMask(
        envelopeRadial,
        edgeSoftness);
}

float WeatherLightRayAtmosphericHeightFade(float axial01)
{
    float fade = clamp(
        _WeatherLightRaySurfaceShape.z,
        0.001,
        0.49);
    return smoothstep(0.0, fade, axial01) *
        smoothstep(0.0, fade, 1.0 - axial01);
}

float WeatherLightRayEvaluateStrands(
    float3 positionWS,
    float evolutionAmount,
    out float envelope,
    out float axial01,
    out float radial01)
{
    float2 radialCoordinates;
    float localRadius;
    WeatherLightRayGetLocalCoordinates(
        positionWS,
        axial01,
        radialCoordinates,
        localRadius,
        radial01);

    float axialMask = step(0.0, axial01) * step(axial01, 1.0);
    envelope = axialMask * WeatherLightRaySoftRadialMask(
        radial01 / max(0.1, _WeatherLightRayTopShape.y),
        max(0.01, _WeatherLightRayTopShape.z));
    if (axialMask <= 0.0)
    {
        return 0.0;
    }

    int strandCount = clamp(
        (int)round(_WeatherLightRayStrandShape0.x),
        1,
        WEATHER_LIGHT_RAY_MAX_STRANDS);
    float minimumWidth = max(0.01, _WeatherLightRayStrandShape0.y);
    float maximumWidth = max(
        minimumWidth,
        _WeatherLightRayStrandShape0.z);
    float spread = saturate(_WeatherLightRayStrandShape0.w);
    float positionVariation = saturate(
        _WeatherLightRayStrandShape1.x);
    float intensityVariation = saturate(
        _WeatherLightRayStrandShape1.y);
    float lengthVariation = saturate(
        _WeatherLightRayStrandShape1.z);
    float strandTaper = saturate(_WeatherLightRayStrandShape1.w);
    float strandEdgeSoftness = max(
        0.01,
        _WeatherLightRayStrandShape2.x);
    float clusterBias = saturate(_WeatherLightRayStrandShape2.y);
    float phaseVariation = saturate(_WeatherLightRayStrandShape2.z);
    float seedBase = _WeatherLightRayStrandShape2.w * 8191.0;

    float intensityFluctuation = saturate(
        _WeatherLightRayEvolution0.x) * evolutionAmount;
    float intensitySpeed = max(0.0, _WeatherLightRayEvolution0.y);
    float widthBreathing = saturate(
        _WeatherLightRayEvolution0.z) * evolutionAmount;
    float lateralDrift = saturate(
        _WeatherLightRayEvolution0.w) * evolutionAmount;
    float patternSpeed = max(0.0, _WeatherLightRayEvolution1.x);
    float basePhase = _WeatherLightRayEvolution1.y;
    float presentationTime = _WeatherLightRayEvolution1.z;

    float maximumDensity = 0.0;
    float limitedDensitySum = 0.0;
    [unroll]
    for (int strandIndex = 0;
        strandIndex < WEATHER_LIGHT_RAY_MAX_STRANDS;
        strandIndex++)
    {
        if (strandIndex >= strandCount)
        {
            continue;
        }

        float strandSeed = seedBase + strandIndex * 37.719;
        float2 randomA = WeatherLightRayHash21(strandSeed + 3.1);
        float2 randomB = WeatherLightRayHash21(strandSeed + 11.7);
        float2 randomC = WeatherLightRayHash21(strandSeed + 29.3);
        float peripheralCount = max(1.0, (float)(strandCount - 1));
        float peripheralIndex = max(0.0, (float)(strandIndex - 1));
        float normalizedIndex = peripheralIndex / peripheralCount;
        float angularCell = 2.0 * WEATHER_LIGHT_RAY_PI /
            peripheralCount;
        float angle = normalizedIndex * 2.0 * WEATHER_LIGHT_RAY_PI +
            (randomA.x - 0.5) * angularCell * positionVariation;
        float radialDistribution = pow(
            saturate(randomA.y),
            lerp(0.75, 2.5, clusterBias));
        float minimumPeripheralRadius = lerp(
            0.42,
            0.12,
            clusterBias);
        radialDistribution = lerp(
            minimumPeripheralRadius,
            1.0,
            radialDistribution);
        float radialDistance = strandIndex == 0
            ? 0.0
            : spread * radialDistribution * localRadius;
        float2 strandDirection = float2(cos(angle), sin(angle));
        float2 strandCentre = strandDirection * radialDistance;

        float strandPhase = basePhase + randomB.x *
            phaseVariation * 2.0 * WEATHER_LIGHT_RAY_PI;
        float evolutionPhase = strandPhase +
            presentationTime * patternSpeed * 2.0 * WEATHER_LIGHT_RAY_PI +
            axial01 * lerp(1.2, 3.4, randomB.y);
        float2 driftDirection = float2(
            -strandDirection.y,
            strandDirection.x);
        strandCentre += driftDirection *
            sin(evolutionPhase) * lateralDrift * localRadius;

        float widthFraction = lerp(
            minimumWidth,
            maximumWidth,
            randomC.x);
        float taperScale = lerp(
            1.0 + strandTaper * 0.22,
            1.0 - strandTaper * 0.22,
            axial01);
        float breathing = 1.0 + sin(
            evolutionPhase * 1.173 + 1.31) * widthBreathing;
        float strandRadius = max(
            0.001,
            localRadius * widthFraction * taperScale * breathing);

        float lowerCut = strandIndex == 0
            ? 0.0
            : lengthVariation * randomB.y * 0.24;
        float upperCut = 1.0 -
            lengthVariation * randomC.y * 0.18;
        float lengthSoftness = 0.035;
        float lengthMask = smoothstep(
            lowerCut,
            lowerCut + lengthSoftness,
            axial01) *
            (1.0 - smoothstep(
                upperCut - lengthSoftness,
                upperCut,
                axial01));

        float strandDistance = length(
            radialCoordinates - strandCentre) / strandRadius;
        float strandMask = WeatherLightRaySoftRadialMask(
            strandDistance,
            strandEdgeSoftness) * lengthMask;
        float staticIntensity = lerp(
            1.0 - intensityVariation,
            1.0 + intensityVariation,
            randomC.y);
        float evolvingIntensity = 1.0 + sin(
            strandPhase +
            presentationTime * intensitySpeed * 2.0 * WEATHER_LIGHT_RAY_PI +
            axial01 * lerp(1.0, 2.8, randomA.x)) *
            intensityFluctuation;
        float density = max(
            0.0,
            strandMask * staticIntensity * evolvingIntensity);
        maximumDensity = max(maximumDensity, density);
        limitedDensitySum += density;
    }

    return saturate(maximumDensity + limitedDensitySum * 0.08);
}

float WeatherLightRayEvaluateSurface(
    float3 positionWS,
    out float strandInfluence,
    out float envelopeInfluence,
    out float axial01,
    out float radial01)
{
    strandInfluence = WeatherLightRayEvaluateStrands(
        positionWS,
        0.25,
        envelopeInfluence,
        axial01,
        radial01);
    float angle = atan2(positionWS.z - _WeatherLightRayBaseCentreHeight.z,
        positionWS.x - _WeatherLightRayBaseCentreHeight.x);
    float irregularity = saturate(_WeatherLightRaySurfaceShape.y);
    float seedPhase = _WeatherLightRayStrandShape2.w * 19.31;
    float edgeWarp = 1.0 + (
        sin(angle * 3.0 + seedPhase) * 0.62 +
        sin(angle * 5.0 - seedPhase * 1.7) * 0.38) *
        irregularity * 0.12;
    float footprint = WeatherLightRaySoftRadialMask(
        radial01 * edgeWarp,
        max(0.01, _WeatherLightRaySurfaceShape.x));
    float axialMask = step(0.0, axial01) * step(axial01, 1.0);
    footprint *= axialMask;
    float core = pow(
        saturate(1.0 - radial01),
        1.0 + max(0.0, _WeatherLightRayIllumination.w) * 2.0);
    float groundContact = pow(saturate(1.0 - axial01), 3.0);
    return saturate(
        footprint * 0.52 +
        strandInfluence * 0.78 +
        max(core * 0.2, groundContact * 0.16) * footprint);
}

float WeatherLightRayCameraFade(float3 cameraPositionWS)
{
    float axial01;
    float radial01;
    float envelope = WeatherLightRayEnvelope(
        cameraPositionWS,
        max(0.01, _WeatherLightRayTopShape.z),
        axial01,
        radial01);
    return 1.0 - envelope * saturate(
        _WeatherLightRaySurfaceShape.w);
}

void WeatherLightRayUpdateIntersection(
    float candidate,
    float valid,
    inout float minimumDistance,
    inout float maximumDistance,
    inout float hitCount)
{
    if (valid > 0.5)
    {
        minimumDistance = min(minimumDistance, candidate);
        maximumDistance = max(maximumDistance, candidate);
        hitCount += 1.0;
    }
}

bool WeatherLightRayIntersectFrustum(
    float3 rayOrigin,
    float3 rayDirection,
    out float enterDistance,
    out float exitDistance)
{
    float3 baseCentre = _WeatherLightRayBaseCentreHeight.xyz;
    float height = max(0.001, _WeatherLightRayBaseCentreHeight.w);
    float3 upwardAxis = normalize(
        -_WeatherLightRayDirectionBaseRadius.xyz);
    float envelopeScale = max(1.0, _WeatherLightRayTopShape.y);
    float baseRadius = max(
        0.001,
        _WeatherLightRayDirectionBaseRadius.w * envelopeScale);
    float topRadius = max(
        0.001,
        _WeatherLightRayTopShape.x * envelopeScale);
    float radiusSlope = (topRadius - baseRadius) / height;

    float3 originRelative = rayOrigin - baseCentre;
    float originAxis = dot(originRelative, upwardAxis);
    float directionAxis = dot(rayDirection, upwardAxis);
    float3 originRadial = originRelative - upwardAxis * originAxis;
    float3 directionRadial = rayDirection - upwardAxis * directionAxis;
    float radiusAtOrigin = baseRadius + radiusSlope * originAxis;

    float quadraticA = dot(directionRadial, directionRadial) -
        radiusSlope * radiusSlope * directionAxis * directionAxis;
    float quadraticB = 2.0 * (
        dot(originRadial, directionRadial) -
        radiusAtOrigin * radiusSlope * directionAxis);
    float quadraticC = dot(originRadial, originRadial) -
        radiusAtOrigin * radiusAtOrigin;

    float minimumDistance = 1e20;
    float maximumDistance = -1e20;
    float hitCount = 0.0;

    if (abs(quadraticA) > 1e-6)
    {
        float discriminant = quadraticB * quadraticB -
            4.0 * quadraticA * quadraticC;
        if (discriminant >= 0.0)
        {
            float root = sqrt(discriminant);
            float inverseDenominator = 0.5 / quadraticA;
            float t0 = (-quadraticB - root) * inverseDenominator;
            float t1 = (-quadraticB + root) * inverseDenominator;
            float y0 = originAxis + directionAxis * t0;
            float y1 = originAxis + directionAxis * t1;
            WeatherLightRayUpdateIntersection(
                t0,
                step(0.0, y0) * step(y0, height),
                minimumDistance,
                maximumDistance,
                hitCount);
            WeatherLightRayUpdateIntersection(
                t1,
                step(0.0, y1) * step(y1, height),
                minimumDistance,
                maximumDistance,
                hitCount);
        }
    }
    else if (abs(quadraticB) > 1e-6)
    {
        float t = -quadraticC / quadraticB;
        float y = originAxis + directionAxis * t;
        WeatherLightRayUpdateIntersection(
            t,
            step(0.0, y) * step(y, height),
            minimumDistance,
            maximumDistance,
            hitCount);
    }

    if (abs(directionAxis) > 1e-6)
    {
        float baseT = -originAxis / directionAxis;
        float3 basePoint = originRelative + rayDirection * baseT;
        float3 baseRadial = basePoint -
            upwardAxis * dot(basePoint, upwardAxis);
        WeatherLightRayUpdateIntersection(
            baseT,
            step(dot(baseRadial, baseRadial), baseRadius * baseRadius),
            minimumDistance,
            maximumDistance,
            hitCount);

        float topT = (height - originAxis) / directionAxis;
        float3 topPoint = originRelative + rayDirection * topT -
            upwardAxis * height;
        float3 topRadial = topPoint -
            upwardAxis * dot(topPoint, upwardAxis);
        WeatherLightRayUpdateIntersection(
            topT,
            step(dot(topRadial, topRadial), topRadius * topRadius),
            minimumDistance,
            maximumDistance,
            hitCount);
    }

    enterDistance = max(0.0, minimumDistance);
    exitDistance = maximumDistance;
    return hitCount >= 2.0 && exitDistance > enterDistance;
}

#endif
