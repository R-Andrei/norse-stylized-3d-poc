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

float FoamSampleMotionLaneSmooth(int2 coordinate)
{
    float scrolledX = (float)coordinate.x - _FoamMotionLaneScrollCells;
    int x0 = (int)floor(scrolledX);
    int x1 = x0 + 1;
    float blend = frac(scrolledX);
    float a = FoamLoadMotionLaneCell(x0, coordinate.y);
    float b = FoamLoadMotionLaneCell(x1, coordinate.y);
    return lerp(a, b, blend);
}

float2 FoamLoadObstacleRoutingCell(int2 coordinate)
{
    if (coordinate.x < 0 || coordinate.x >= _FoamDimensions.x ||
        coordinate.y < 0 || coordinate.y >= _FoamDimensions.y)
    {
        return 0.0.xx;
    }

    float2 routing = _FoamObstacleRoutingRead.Load(int3(coordinate, 0));
    routing.x = clamp(routing.x, -1.0, 1.0);
    routing.y = saturate(routing.y);
    return routing;
}

FoamMotionFieldSample FoamResolveMotionFieldSample(
    int2 coordinate,
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

    float lane = FoamSampleMotionLaneSmooth(coordinate);
    float2 obstacle = FoamLoadObstacleRoutingCell(coordinate);
    float obstacleInfluence = saturate(obstacle.y);
    float resolved = lerp(lane, obstacle.x, obstacleInfluence);

    sample.laneValue = lane;
    sample.obstacleValue = obstacle.x;
    sample.obstacleInfluence = obstacleInfluence;
    sample.lateralCells = clamp(resolved * strength, -1.25, 1.25);
    return sample;
}
