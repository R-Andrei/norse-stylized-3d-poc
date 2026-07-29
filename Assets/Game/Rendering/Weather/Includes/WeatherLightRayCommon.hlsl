#ifndef PS3D_WEATHER_LIGHT_RAY_COMMON_INCLUDED
#define PS3D_WEATHER_LIGHT_RAY_COMMON_INCLUDED

float4 _WeatherLightRayBaseCentreHeight;
float4 _WeatherLightRayDirectionAreaDiameter;
float4 _WeatherLightRayGroundContactAxisWorld;
float4 _WeatherLightRayColour;
float4 _WeatherLightRayIntensity;
float4 _WeatherLightRayBeamShape0;
float4 _WeatherLightRayBeamShape1;
float4 _WeatherLightRayBeamShape2;
float4 _WeatherLightRaySofteningDirection;
float4 _WeatherLightRaySofteningParameters;
float4 _WeatherLightRaySurfaceParameters0;
float4 _WeatherLightRaySurfaceParameters1;
float4 _WeatherLightRaySurfaceScreenBounds;
float _WeatherLightRayDebugMode;

static const float WEATHER_LIGHT_RAY_PI = 3.14159265359;

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

void WeatherLightRayBuildStableBasis(
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

float3 WeatherLightRayGetGroundContactAxis()
{
    float3 axis = _WeatherLightRayGroundContactAxisWorld.xyz;
    float axisLengthSquared = dot(axis, axis);
    if (axisLengthSquared > 1e-6)
    {
        return axis * rsqrt(axisLengthSquared);
    }

    float3 fallbackAxis;
    float3 fallbackBitangent;
    WeatherLightRayBuildStableBasis(
        -normalize(_WeatherLightRayDirectionAreaDiameter.xyz),
        fallbackAxis,
        fallbackBitangent);
    return fallbackAxis;
}

#if defined(WEATHER_LIGHT_RAY_ENABLE_BEAM_BUFFER)
struct WeatherLightRayBeamRecord
{
    float4 A0;
    float4 A1;
    float4 A2;
    float4 B0;
    float4 B1;
    float4 B2;
};

StructuredBuffer<WeatherLightRayBeamRecord> _WeatherLightRayBeamBuffer;
StructuredBuffer<float4> _WeatherLightRayZoneBuffer;
int _WeatherLightRayZoneIndex;

float4 WeatherLightRayGetZoneData()
{
    return _WeatherLightRayZoneBuffer[_WeatherLightRayZoneIndex];
}

float WeatherLightRayGetBeamCount()
{
    return max(2.0, round(WeatherLightRayGetZoneData().y));
}

int WeatherLightRayGetBeamCountInt()
{
    return (int)WeatherLightRayGetBeamCount();
}

int WeatherLightRayGetBeamOffset()
{
    return max(0, (int)round(WeatherLightRayGetZoneData().x));
}

float WeatherLightRayGetEvolutionBlend()
{
    return saturate(WeatherLightRayGetZoneData().z);
}

float WeatherLightRayGetMinimumOverlapRatio()
{
    return clamp(_WeatherLightRayBeamShape2.z, 0.0, 0.49);
}

float WeatherLightRayGetMaximumOverlapRatio()
{
    return clamp(
        _WeatherLightRayBeamShape2.w,
        WeatherLightRayGetMinimumOverlapRatio(),
        0.60);
}

float WeatherLightRayGetContactPlaneOpacity()
{
    return saturate(WeatherLightRayGetZoneData().w);
}

float WeatherLightRayGetAverageBeamWidth()
{
    float beamCount = WeatherLightRayGetBeamCount();
    float areaDiameter = max(
        0.60,
        _WeatherLightRayDirectionAreaDiameter.w);
    float representativeOverlap =
        (WeatherLightRayGetMinimumOverlapRatio() +
            WeatherLightRayGetMaximumOverlapRatio()) * 0.5;
    float effectiveBeamUnits = max(
        1.0,
        beamCount - (beamCount - 1.0) * representativeOverlap);
    return areaDiameter / effectiveBeamUnits;
}

WeatherLightRayBeamRecord WeatherLightRayGetBeamRecord(int beamIndex)
{
    int clampedIndex = clamp(
        beamIndex,
        0,
        WeatherLightRayGetBeamCountInt() - 1);
    return _WeatherLightRayBeamBuffer[
        WeatherLightRayGetBeamOffset() + clampedIndex];
}

void WeatherLightRayGetBeamLayout(
    float beamIndexValue,
    out float centreOffset,
    out float beamWidth)
{
    WeatherLightRayBeamRecord record = WeatherLightRayGetBeamRecord(
        (int)round(beamIndexValue));
    float blend = WeatherLightRayGetEvolutionBlend();
    centreOffset = lerp(record.A0.x, record.B0.x, blend);
    beamWidth = lerp(record.A0.y, record.B0.y, blend);
}

void WeatherLightRayGetBeamVariation(
    float beamIndexValue,
    out float beamIntensity,
    out float beamPhase,
    out float upperFadeScale,
    out float groundFadeScale,
    out float leftSoftness,
    out float rightSoftness,
    out float peakBias,
    out float leftTransmission,
    out float rightTransmission,
    out float contactOpacityScale)
{
    WeatherLightRayBeamRecord record = WeatherLightRayGetBeamRecord(
        (int)round(beamIndexValue));
    float blend = WeatherLightRayGetEvolutionBlend();
    float4 value0 = lerp(record.A0, record.B0, blend);
    float4 value1 = lerp(record.A1, record.B1, blend);
    float4 value2 = lerp(record.A2, record.B2, blend);

    beamIntensity = value0.z;
    beamPhase = value0.w;
    upperFadeScale = value1.x;
    groundFadeScale = value1.y;
    leftSoftness = value1.z;
    rightSoftness = value1.w;
    peakBias = value2.x;
    leftTransmission = value2.y;
    rightTransmission = value2.z;
    contactOpacityScale = value2.w;
}

#endif // WEATHER_LIGHT_RAY_ENABLE_BEAM_BUFFER

float WeatherLightRayDistanceToSegment(
    float3 samplePosition,
    float3 segmentStart,
    float3 segmentEnd)
{
    float3 segment = segmentEnd - segmentStart;
    float segmentLengthSquared = max(1e-6, dot(segment, segment));
    float segmentT = saturate(
        dot(samplePosition - segmentStart, segment) /
        segmentLengthSquared);
    float3 closest = segmentStart + segment * segmentT;
    return distance(samplePosition, closest);
}



float WeatherLightRayScreenInsideSurfaceBounds(float2 screenUV)
{
    float2 lower = _WeatherLightRaySurfaceScreenBounds.xy;
    float2 upper = _WeatherLightRaySurfaceScreenBounds.zw;
    return step(lower.x, screenUV.x) *
        step(lower.y, screenUV.y) *
        step(screenUV.x, upper.x) *
        step(screenUV.y, upper.y);
}

float2 WeatherLightRayWorldToScreenUV(
    float3 worldPosition,
    out float clipW)
{
    float4 clipPosition = TransformWorldToHClip(worldPosition);
    clipW = clipPosition.w;
    float4 screenPosition = ComputeScreenPos(clipPosition);
    return screenPosition.xy / max(0.0001, screenPosition.w);
}

#if defined(WEATHER_LIGHT_RAY_ENABLE_DEPTH_EVALUATION)
void WeatherLightRayEvaluateFootprintMarkers(
    float2 screenUV,
    out float boundaryMarker,
    out float diameterMarker,
    out float endpointMarker,
    out float centreMarker)
{
    boundaryMarker = 0.0;
    diameterMarker = 0.0;
    endpointMarker = 0.0;
    centreMarker = 0.0;

    float rawDepth = SampleSceneDepth(screenUV);
    if (WeatherLightRayRawDepthIsValid(rawDepth) > 0.5)
    {
        float3 worldPosition = WeatherLightRayReconstructWorldPosition(
            screenUV,
            rawDepth);
        float footprintRadius = max(
            0.30,
            _WeatherLightRaySurfaceParameters1.x);
        float2 centreXZ = _WeatherLightRayBaseCentreHeight.xz;
        float2 deltaXZ = worldPosition.xz - centreXZ;
        float horizontalDistance = length(deltaXZ);
        float ringWidth = max(0.035, footprintRadius * 0.035);
        boundaryMarker = 1.0 - smoothstep(
            ringWidth,
            ringWidth * 2.5,
            abs(horizontalDistance - footprintRadius));

        float2 axisXZ = _WeatherLightRayGroundContactAxisWorld.xz;
        float axisLengthSquared = dot(axisXZ, axisXZ);
        axisXZ = axisLengthSquared > 1e-6
            ? axisXZ * rsqrt(axisLengthSquared)
            : float2(1.0, 0.0);
        float along = dot(deltaXZ, axisXZ);
        float perpendicular = abs(
            deltaXZ.x * axisXZ.y -
            deltaXZ.y * axisXZ.x);
        float lineWidth = max(0.025, footprintRadius * 0.018);
        diameterMarker = (1.0 - smoothstep(
            lineWidth,
            lineWidth * 2.5,
            perpendicular)) *
            (1.0 - step(footprintRadius, abs(along)));

        float2 endpointA = centreXZ - axisXZ * footprintRadius;
        float2 endpointB = centreXZ + axisXZ * footprintRadius;
        float endpointRadius = max(0.055, footprintRadius * 0.045);
        endpointMarker = max(
            1.0 - smoothstep(
                endpointRadius,
                endpointRadius * 2.0,
                distance(worldPosition.xz, endpointA)),
            1.0 - smoothstep(
                endpointRadius,
                endpointRadius * 2.0,
                distance(worldPosition.xz, endpointB)));
    }

    float centreClipW;
    float2 centreUV = WeatherLightRayWorldToScreenUV(
        _WeatherLightRayBaseCentreHeight.xyz,
        centreClipW);
    float centreDistancePixels = length(
        (screenUV - centreUV) * _ScreenParams.xy);
    centreMarker = (1.0 - smoothstep(
        6.0,
        14.0,
        centreDistancePixels)) * step(0.0001, centreClipW);
}

float WeatherLightRayEvaluateSurfaceInfluence(float2 screenUV)
{
    if (WeatherLightRayScreenInsideSurfaceBounds(screenUV) <= 0.5)
    {
        return 0.0;
    }

    float surfaceIntensity = saturate(
        _WeatherLightRaySurfaceParameters0.x);
    if (surfaceIntensity <= 0.0)
    {
        return 0.0;
    }

    float rawDepth = SampleSceneDepth(screenUV);
    if (WeatherLightRayRawDepthIsValid(rawDepth) <= 0.5)
    {
        return 0.0;
    }

    float3 worldPosition = WeatherLightRayReconstructWorldPosition(
        screenUV,
        rawDepth);
    float footprintRadius = max(
        0.1,
        _WeatherLightRaySurfaceParameters1.x);
    float horizontalDistance = distance(
        worldPosition.xz,
        _WeatherLightRayBaseCentreHeight.xz);

    float footprintSoftness = saturate(
        _WeatherLightRaySurfaceParameters0.y);
    float transitionHalfWidth =
        footprintRadius * 0.35 * footprintSoftness;
    float radial;
    if (footprintSoftness <= 0.0001)
    {
        radial = 1.0 - step(
            footprintRadius,
            horizontalDistance);
    }
    else
    {
        radial = 1.0 - smoothstep(
            footprintRadius - transitionHalfWidth,
            footprintRadius + transitionHalfWidth,
            horizontalDistance);
    }

    return saturate(
        radial *
        saturate(_WeatherLightRaySurfaceParameters0.z) *
        surfaceIntensity);
}
#endif // WEATHER_LIGHT_RAY_ENABLE_DEPTH_EVALUATION

#endif
