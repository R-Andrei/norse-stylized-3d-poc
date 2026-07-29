#ifndef PS3D_TREE_WIND_RESPONSE_INCLUDED
#define PS3D_TREE_WIND_RESPONSE_INCLUDED

#include "../../Weather/Includes/WeatherWindField.hlsl"
#include "TreeCommon.hlsl"

struct TreeWindVertexResult
{
    float3 positionWS;
    float3 normalWS;
    float3 tangentWS;
    float heightMask;
    float windMask;
    float flutterPhase;
    float weatherActive;
};

TreeWindVertexResult ApplyTreeWindResponse(
    float3 positionOS,
    float3 positionWS,
    float3 normalWS,
    float3 tangentWS,
    float4 vertexColour,
    float3 rootWorldPosition,
    float boundsMinY,
    float boundsHeight,
    float windEnabled,
    float maskMode,
    float stiffness,
    float macroWindStrength,
    float foliageFlutterStrength,
    float instancePhase,
    float isFoliage)
{
    TreeWindVertexResult result;
    result.positionWS = positionWS;
    result.normalWS = normalize(normalWS);
    result.tangentWS = normalize(tangentWS);
    result.heightMask = TreeResolveHeightMask(
        positionOS.y,
        boundsMinY,
        boundsHeight);
    result.windMask = TreeResolveWindMask(
        result.heightMask,
        vertexColour,
        maskMode);
    result.flutterPhase = instancePhase * PS3D_TREE_TWO_PI;
    result.weatherActive = 0.0;

    if (windEnabled < 0.5)
    {
        return result;
    }

    WeatherWindResponseSample weather =
        SampleWeatherWindResponse(rootWorldPosition);
    result.weatherActive = weather.active;
    if (weather.active < 0.5)
    {
        return result;
    }

    float deadMask = isFoliage < 0.5
        ? saturate(vertexColour.a)
        : 0.0;
    float branchStiffness = saturate(vertexColour.b);
    float effectiveStiffness = lerp(
        saturate(stiffness),
        max(saturate(stiffness), branchStiffness),
        deadMask);
    float response = saturate(1.0 - effectiveStiffness);
    float2 bend = weather.bend;
    float bendMagnitude = length(bend);
    float2 bendDirection = bendMagnitude > 0.0001
        ? bend / bendMagnitude
        : normalize(weather.velocity + float2(0.0001, 0.0001));
    float3 rotationAxis = normalize(float3(
        -bendDirection.y,
        0.0,
        bendDirection.x));
    float bendAngle =
        bendMagnitude *
        max(0.0, macroWindStrength) *
        response *
        0.08 *
        result.windMask;

    float3 rootRelativePosition = positionWS - rootWorldPosition;
    result.positionWS = rootWorldPosition + TreeRotateAroundAxis(
        rootRelativePosition,
        rotationAxis,
        bendAngle);
    result.normalWS = normalize(TreeRotateAroundAxis(
        result.normalWS,
        rotationAxis,
        bendAngle));
    result.tangentWS = normalize(TreeRotateAroundAxis(
        result.tangentWS,
        rotationAxis,
        bendAngle));

    if (isFoliage < 0.5 || foliageFlutterStrength <= 0.00001)
    {
        return result;
    }

    float spatialPhase = TreeHash31(
        floor(positionOS * 1.75) + instancePhase * 19.0);
    float continuousTime = max(
        0.0,
        _WeatherWindFieldTiming.x + _WeatherWindFieldTiming.y);
    float flutterFrequency = lerp(2.1, 3.6, spatialPhase);
    result.flutterPhase =
        continuousTime * PS3D_TREE_TWO_PI * flutterFrequency +
        instancePhase * PS3D_TREE_TWO_PI +
        dot(positionOS, float3(0.71, 0.29, -0.53));

    float weatherEnergy = saturate(
        bendMagnitude * 1.25 +
        length(weather.velocity) * 0.30);
    float flutter =
        sin(result.flutterPhase) *
        max(0.0, foliageFlutterStrength) *
        weatherEnergy *
        response *
        result.windMask;
    float sideFlutter =
        sin(result.flutterPhase * 0.73 + 1.57079632679) *
        flutter *
        0.45;

    result.positionWS +=
        result.normalWS * flutter +
        result.tangentWS * sideFlutter;
    return result;
}

#endif
