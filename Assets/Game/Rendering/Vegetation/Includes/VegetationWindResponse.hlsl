#ifndef PS3D_VEGETATION_WIND_RESPONSE_INCLUDED
#define PS3D_VEGETATION_WIND_RESPONSE_INCLUDED

#include "../../Weather/Includes/WeatherWindField.hlsl"

#define PS3D_VEGETATION_TWO_PI 6.28318530718
#define PS3D_VEGETATION_CALM_DETAIL_ENERGY 0.078

float3 ApplyVegetationWindResponse(
    float3 worldPosition,
    float rootToTipWeight,
    float stiffness,
    float instancePhase,
    float bladeVariation,
    out float2 fullTipDisplacementXZ)
{
    fullTipDisplacementXZ = 0.0;

    WeatherWindResponseSample weather = SampleWeatherWindResponse(worldPosition);
    if (weather.active < 0.5)
    {
        return worldPosition;
    }

    float response = saturate(1.0 - stiffness);
    response *= lerp(0.90, 1.10, saturate(bladeVariation));

    float2 macroBend = weather.bend;
    float bendMagnitude = length(macroBend);
    float2 bendDirection = bendMagnitude > 0.0001
        ? macroBend / bendMagnitude
        : normalize(weather.velocity + float2(0.0001, 0.0001));
    float2 perpendicular = float2(-bendDirection.y, bendDirection.x);

    float windDrivenDetailEnergy = saturate(bendMagnitude * 1.35);
    float detailEnergy = max(
        PS3D_VEGETATION_CALM_DETAIL_ENERGY,
        windDrivenDetailEnergy);
    float detailFrequency = lerp(1.75, 2.55, saturate(bladeVariation));
    float continuousWindTime = max(
        0.0,
        _WeatherWindFieldTiming.x + _WeatherWindFieldTiming.y);
    float detailPhase =
        continuousWindTime * PS3D_VEGETATION_TWO_PI * detailFrequency +
        instancePhase * PS3D_VEGETATION_TWO_PI +
        dot(worldPosition.xz, float2(0.17, -0.11));
    float longitudinalDetail = sin(detailPhase) * detailEnergy * 0.035;
    float lateralDetail =
        sin(detailPhase * 0.73 + 1.57079632679) * detailEnergy * 0.018;

    float2 fullTipWind =
        macroBend +
        bendDirection * longitudinalDetail +
        perpendicular * lateralDetail;
    fullTipDisplacementXZ = fullTipWind * response;

    float bendWeight = rootToTipWeight * rootToTipWeight;
    float2 windDisplacement = fullTipWind * (response * bendWeight);

    worldPosition.xz += windDisplacement;
    float displacementMagnitude = length(windDisplacement);
    worldPosition.y -= displacementMagnitude * displacementMagnitude * 0.12;
    return worldPosition;
}

#endif
