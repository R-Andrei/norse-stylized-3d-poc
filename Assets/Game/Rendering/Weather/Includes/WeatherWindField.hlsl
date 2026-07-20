#ifndef PS3D_WEATHER_WIND_FIELD_INCLUDED
#define PS3D_WEATHER_WIND_FIELD_INCLUDED

TEXTURE2D(_WeatherWindTargetField);
SAMPLER(sampler_WeatherWindTargetField);
TEXTURE2D(_WeatherWindResponseField);
SAMPLER(sampler_WeatherWindResponseField);

float4 _WeatherWindFieldOriginCellSize;
float4 _WeatherWindFieldResolutionOffset;
float4 _WeatherWindFieldTiming;

struct WeatherWindResponseSample
{
    float2 bend;
    float2 velocity;
    float active;
};

float2 WeatherWindLogicalCellToUv(float2 logicalCell)
{
    float2 resolution = max(_WeatherWindFieldResolutionOffset.xy, 1.0);
    float2 clampedLogical = clamp(logicalCell, 0.0, resolution - 1.0);
    float2 physicalCell = clampedLogical + _WeatherWindFieldResolutionOffset.zw;
    return frac((physicalCell + 0.5) / resolution);
}

float WeatherWindFieldContains(float2 logicalCell)
{
    float2 resolution = _WeatherWindFieldResolutionOffset.xy;
    float2 minimumCheck = step(0.0, logicalCell);
    float2 maximumCheck = step(logicalCell, resolution - 1.0);
    return minimumCheck.x * minimumCheck.y * maximumCheck.x * maximumCheck.y;
}

float2 WeatherWindWorldToLogicalCell(float3 worldPosition)
{
    float cellSize = max(0.0001, _WeatherWindFieldOriginCellSize.z);
    return (worldPosition.xz - _WeatherWindFieldOriginCellSize.xy) / cellSize - 0.5;
}

WeatherWindResponseSample SampleWeatherWindResponse(float3 worldPosition)
{
    WeatherWindResponseSample sample;
    sample.bend = 0.0;
    sample.velocity = 0.0;
    sample.active = 0.0;

    if (_WeatherWindFieldOriginCellSize.w < 0.5)
    {
        return sample;
    }

    float2 logicalCell = WeatherWindWorldToLogicalCell(worldPosition);
    float inside = WeatherWindFieldContains(logicalCell);
    float2 uv = WeatherWindLogicalCellToUv(logicalCell);
    float4 state = SAMPLE_TEXTURE2D_LOD(
        _WeatherWindResponseField,
        sampler_WeatherWindResponseField,
        uv,
        0.0);

    float predictionTime = min(
        max(0.0, _WeatherWindFieldTiming.y),
        max(0.0, _WeatherWindFieldTiming.z));
    float2 predictedBend = state.xy + state.zw * predictionTime;
    float maximumBend = max(0.001, _WeatherWindFieldTiming.w);
    float predictedMagnitude = length(predictedBend);
    if (predictedMagnitude > maximumBend)
    {
        predictedBend *= maximumBend / predictedMagnitude;
    }

    sample.bend = predictedBend * inside;
    sample.velocity = state.zw * inside;
    sample.active = inside;
    return sample;
}

float2 SampleWeatherTargetWind(float3 worldPosition)
{
    if (_WeatherWindFieldOriginCellSize.w < 0.5)
    {
        return 0.0;
    }

    float2 logicalCell = WeatherWindWorldToLogicalCell(worldPosition);
    float inside = WeatherWindFieldContains(logicalCell);
    float2 uv = WeatherWindLogicalCellToUv(logicalCell);
    return SAMPLE_TEXTURE2D_LOD(
        _WeatherWindTargetField,
        sampler_WeatherWindTargetField,
        uv,
        0.0).xy * inside;
}

#endif
