struct FoamMotionFieldSample
{
    float lateralCells;
    float obstacleInfluence;
    float laneValue;
    float obstacleValue;
};

int FoamWrapMotionLaneX(int x)
{
    int width = max(1, _FoamDimensions.x);

    // _FoamMotionLaneScrollCells is wrapped on the CPU into [0, width),
    // so the smooth lane sampler only ever asks for x within one wrap
    // beyond the texture bounds. Avoid integer modulus in the SimulateFoam
    // hot path because D3D11 warns that signed integer modulo can be slow.
    if (x >= width)
    {
        x -= width;
    }
    else if (x < 0)
    {
        x += width;
    }

    return clamp(x, 0, width - 1);
}

float FoamLoadMotionLaneCell(int x, int y)
{
    int2 coordinate = int2(
        FoamWrapMotionLaneX(x),
        ClampY(y));
    return clamp(
        _FoamMotionLaneRead.Load(int3(coordinate, 0)),
        -1.0,
        1.0);
}

float FoamSampleMotionLaneSmooth(float2 coordinate)
{
    float scrolledX = coordinate.x - _FoamMotionLaneScrollCells;
    int x0 = (int)floor(scrolledX);
    int x1 = x0 + 1;
    float blend = frac(scrolledX);
    int y = ClampY((int)floor(coordinate.y));
    float a = FoamLoadMotionLaneCell(x0, y);
    float b = FoamLoadMotionLaneCell(x1, y);
    return lerp(a, b, blend);
}

float2 FoamLoadObstacleRoutingCell(float2 coordinate)
{
    int2 texel = int2(floor(coordinate));
    if (texel.x < 0 || texel.x >= _FoamDimensions.x ||
        texel.y < 0 || texel.y >= _FoamDimensions.y)
    {
        return 0.0.xx;
    }

    float2 routing = _FoamObstacleRoutingRead.Load(int3(texel, 0));
    routing.x = clamp(routing.x, -1.0, 1.0);
    routing.y = saturate(routing.y);
    return routing;
}

FoamMotionFieldSample FoamResolveMotionFieldSample(
    float2 motionSampleCoordinate,
    float validFluid)
{
    FoamMotionFieldSample sample;
    sample.lateralCells = 0.0;
    sample.obstacleInfluence = 0.0;
    sample.laneValue = 0.0;
    sample.obstacleValue = 0.0;

    float strength = clamp(_FoamMotionFieldStrength, 0.0, 4.0);
    if (strength <= 0.0001 || validFluid <= 0.0001)
    {
        return sample;
    }

    float lane = FoamSampleMotionLaneSmooth(motionSampleCoordinate);
    float2 obstacle = FoamLoadObstacleRoutingCell(motionSampleCoordinate);
    float obstacleInfluence = saturate(obstacle.y);
    float resolved = lerp(lane, obstacle.x, obstacleInfluence);

    sample.laneValue = lane;
    sample.obstacleValue = obstacle.x;
    sample.obstacleInfluence = obstacleInfluence;
    sample.lateralCells = clamp(resolved * strength, -1.25, 1.25);
    return sample;
}
