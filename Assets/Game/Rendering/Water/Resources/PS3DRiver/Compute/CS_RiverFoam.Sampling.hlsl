
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



float4 LoadAdvected(int2 coordinate)
{
    return _FoamAdvectedRead.Load(
        int3(ClampX(coordinate.x), ClampY(coordinate.y), 0));
}


float4 LoadReverse(int2 coordinate)
{
    return _FoamReverseRead.Load(
        int3(ClampX(coordinate.x), ClampY(coordinate.y), 0));
}


float4 SampleAdvectedBilinear(float2 pixelCoordinate)
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

    float4 a = LoadAdvected(baseCoordinate);
    float4 b = LoadAdvected(int2(nextCoordinate.x, baseCoordinate.y));
    float4 c = LoadAdvected(int2(baseCoordinate.x, nextCoordinate.y));
    float4 d = LoadAdvected(nextCoordinate);
    float4 state = lerp(
        lerp(a, b, blend.x),
        lerp(c, d, blend.x),
        blend.y);

    state.w = 0.0;
    return state;
}



float4 SampleBoundaryClassesBilinear(float2 pixelCoordinate)
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
    float4 a = LoadBoundaryClasses(baseCoordinate);
    float4 b = LoadBoundaryClasses(
        int2(nextCoordinate.x, baseCoordinate.y));
    float4 c = LoadBoundaryClasses(
        int2(baseCoordinate.x, nextCoordinate.y));
    float4 d = LoadBoundaryClasses(nextCoordinate);
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}


float2 SampleBoundaryBilinear(float2 pixelCoordinate)
{
    return SampleBoundaryClassesBilinear(pixelCoordinate).rg;
}


float4 SampleGuidanceBilinear(float2 uv)
{
    float2 coordinate = FoamUVToTexelCoordinate(
        uv,
        _FoamGuidanceDimensions);
    int2 baseCoordinate = int2(floor(coordinate));
    int2 nextCoordinate = min(
        baseCoordinate + int2(1, 1),
        _FoamGuidanceDimensions - int2(1, 1));
    float2 blend = frac(coordinate);
    float4 a = _FoamGuidanceRead.Load(int3(baseCoordinate, 0));
    float4 b = _FoamGuidanceRead.Load(
        int3(nextCoordinate.x, baseCoordinate.y, 0));
    float4 c = _FoamGuidanceRead.Load(
        int3(baseCoordinate.x, nextCoordinate.y, 0));
    float4 d = _FoamGuidanceRead.Load(int3(nextCoordinate, 0));
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}



float4 SampleStateAtUV(float2 uv)
{
    return SampleStateBilinear(
        FoamUVToTexelCoordinate(uv, _FoamDimensions));
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
