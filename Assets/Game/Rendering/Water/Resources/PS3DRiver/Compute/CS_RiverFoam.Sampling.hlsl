
float FoamValidFluidAt(int2 coordinate)
{
    if (coordinate.x < 0 || coordinate.x >= _FoamDimensions.x ||
        coordinate.y < 0 || coordinate.y >= _FoamDimensions.y ||
        !IsFoamColumnInsideSimulation(coordinate.x))
    {
        return 0.0;
    }

    float boundaryCoverage = LoadBoundaryCoverage(coordinate);
    float obstacleFootprint = LoadObstacleExclusionCell(coordinate);
    return saturate(boundaryCoverage * (1.0 - obstacleFootprint));
}


float4 SampleStateBilinear(float2 pixelCoordinate)
{
    float2 clamped = clamp(
        pixelCoordinate,
        float2(0.0, 0.0),
        float2(
            (float)(_FoamDimensions.x - 1),
            (float)(_FoamDimensions.y - 1)));
    int2 baseCoordinate = int2(floor(clamped));
    int2 nextCoordinate = min(
        baseCoordinate + int2(1, 1),
        _FoamDimensions - int2(1, 1));
    float2 blend = frac(clamped);

    float4 a = LoadState(baseCoordinate);
    float4 b = LoadState(int2(nextCoordinate.x, baseCoordinate.y));
    float4 c = LoadState(int2(baseCoordinate.x, nextCoordinate.y));
    float4 d = LoadState(nextCoordinate);
    float4 state = lerp(
        lerp(a, b, blend.x),
        lerp(c, d, blend.x),
        blend.y);

    state.w = 0.0;
    return state;
}



float SampleBoundaryCoverageBilinear(float2 pixelCoordinate)
{
    float2 clamped = clamp(
        pixelCoordinate,
        float2(0.0, 0.0),
        float2(
            (float)(_FoamDimensions.x - 1),
            (float)(_FoamDimensions.y - 1)));
    int2 baseCoordinate = int2(floor(clamped));
    int2 nextCoordinate = min(
        baseCoordinate + int2(1, 1),
        _FoamDimensions - int2(1, 1));
    float2 blend = frac(clamped);
    float a = LoadBoundaryCoverage(baseCoordinate);
    float b = LoadBoundaryCoverage(
        int2(nextCoordinate.x, baseCoordinate.y));
    float c = LoadBoundaryCoverage(
        int2(baseCoordinate.x, nextCoordinate.y));
    float d = LoadBoundaryCoverage(nextCoordinate);
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}


float4 SampleStateAtUV(float2 uv)
{
    return SampleStateBilinear(
        FoamUVToTexelCoordinate(uv, _FoamDimensions));
}


float4 SampleTopologyBilinear(float2 pixelCoordinate)
{
    float2 clamped = clamp(
        pixelCoordinate,
        float2(0.0, 0.0),
        float2(
            (float)(_FoamTopologyDimensions.x - 1),
            (float)(_FoamTopologyDimensions.y - 1)));
    int2 baseCoordinate = int2(floor(clamped));
    int2 nextCoordinate = min(
        baseCoordinate + int2(1, 1),
        _FoamTopologyDimensions - int2(1, 1));
    float2 blend = frac(clamped);

    float4 a = _FoamTopologyRead.Load(int3(baseCoordinate, 0));
    float4 b = _FoamTopologyRead.Load(
        int3(nextCoordinate.x, baseCoordinate.y, 0));
    float4 c = _FoamTopologyRead.Load(
        int3(baseCoordinate.x, nextCoordinate.y, 0));
    float4 d = _FoamTopologyRead.Load(int3(nextCoordinate, 0));
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}

float4 SampleTopologySourcesBilinear(float2 pixelCoordinate)
{
    float2 clamped = clamp(
        pixelCoordinate,
        float2(0.0, 0.0),
        float2(
            (float)(_FoamTopologyDimensions.x - 1),
            (float)(_FoamTopologyDimensions.y - 1)));
    int2 baseCoordinate = int2(floor(clamped));
    int2 nextCoordinate = min(
        baseCoordinate + int2(1, 1),
        _FoamTopologyDimensions - int2(1, 1));
    float2 blend = frac(clamped);

    float4 a = _FoamTopologySourcesRead.Load(int3(baseCoordinate, 0));
    float4 b = _FoamTopologySourcesRead.Load(
        int3(nextCoordinate.x, baseCoordinate.y, 0));
    float4 c = _FoamTopologySourcesRead.Load(
        int3(baseCoordinate.x, nextCoordinate.y, 0));
    float4 d = _FoamTopologySourcesRead.Load(int3(nextCoordinate, 0));
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}

float SampleObstacleExclusionBilinear(float2 pixelCoordinate)
{
    float2 clamped = clamp(
        pixelCoordinate,
        float2(0.0, 0.0),
        float2(
            (float)(_FoamDimensions.x - 1),
            (float)(_FoamDimensions.y - 1)));
    int2 baseCoordinate = int2(floor(clamped));
    int2 nextCoordinate = min(
        baseCoordinate + int2(1, 1),
        _FoamDimensions - int2(1, 1));
    float2 blend = frac(clamped);

    float a = LoadObstacleExclusionCell(baseCoordinate);
    float b = LoadObstacleExclusionCell(
        int2(nextCoordinate.x, baseCoordinate.y));
    float c = LoadObstacleExclusionCell(
        int2(baseCoordinate.x, nextCoordinate.y));
    float d = LoadObstacleExclusionCell(nextCoordinate);
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}



void ResolveExternalBilinearCoordinates(
    int2 dimensions,
    float2 uv,
    out int2 baseCoordinate,
    out int2 nextCoordinate,
    out float2 blend)
{
    int2 safeDimensions = max(dimensions, int2(1, 1));
    float2 coordinate = saturate(uv) *
        (float2)(safeDimensions - int2(1, 1));
    baseCoordinate = int2(floor(coordinate));
    nextCoordinate = min(
        baseCoordinate + int2(1, 1),
        safeDimensions - int2(1, 1));
    blend = frac(coordinate);
}


float4 SampleStaticWakeBilinear(float2 uv)
{
    int2 baseCoordinate;
    int2 nextCoordinate;
    float2 blend;
    ResolveExternalBilinearCoordinates(
        _FoamStaticWakeDimensions,
        uv,
        baseCoordinate,
        nextCoordinate,
        blend);
    float4 a = _FoamStaticWakeField.Load(int3(baseCoordinate, 0));
    float4 b = _FoamStaticWakeField.Load(
        int3(nextCoordinate.x, baseCoordinate.y, 0));
    float4 c = _FoamStaticWakeField.Load(
        int3(baseCoordinate.x, nextCoordinate.y, 0));
    float4 d = _FoamStaticWakeField.Load(int3(nextCoordinate, 0));
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}



float4 SampleRippleBilinear(float2 uv)
{
    int2 baseCoordinate;
    int2 nextCoordinate;
    float2 blend;
    ResolveExternalBilinearCoordinates(
        _FoamRippleDimensions,
        uv,
        baseCoordinate,
        nextCoordinate,
        blend);
    float4 a = _FoamRippleField.Load(int3(baseCoordinate, 0));
    float4 b = _FoamRippleField.Load(
        int3(nextCoordinate.x, baseCoordinate.y, 0));
    float4 c = _FoamRippleField.Load(
        int3(baseCoordinate.x, nextCoordinate.y, 0));
    float4 d = _FoamRippleField.Load(int3(nextCoordinate, 0));
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}


float4 SampleWakeBilinear(float2 uv)
{
    int2 baseCoordinate;
    int2 nextCoordinate;
    float2 blend;
    ResolveExternalBilinearCoordinates(
        _FoamWakeDimensions,
        uv,
        baseCoordinate,
        nextCoordinate,
        blend);
    float4 a = _FoamWakeField.Load(int3(baseCoordinate, 0));
    float4 b = _FoamWakeField.Load(
        int3(nextCoordinate.x, baseCoordinate.y, 0));
    float4 c = _FoamWakeField.Load(
        int3(baseCoordinate.x, nextCoordinate.y, 0));
    float4 d = _FoamWakeField.Load(int3(nextCoordinate, 0));
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}


float4 SampleStaticPressureBilinear(float2 uv)
{
    int2 baseCoordinate;
    int2 nextCoordinate;
    float2 blend;
    ResolveExternalBilinearCoordinates(
        _FoamStaticPressureDimensions,
        uv,
        baseCoordinate,
        nextCoordinate,
        blend);
    float4 a = _FoamStaticPressureField.Load(int3(baseCoordinate, 0));
    float4 b = _FoamStaticPressureField.Load(
        int3(nextCoordinate.x, baseCoordinate.y, 0));
    float4 c = _FoamStaticPressureField.Load(
        int3(baseCoordinate.x, nextCoordinate.y, 0));
    float4 d = _FoamStaticPressureField.Load(int3(nextCoordinate, 0));
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}
