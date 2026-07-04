// Canonical Stage 6 topology-field contract. UV spans the complete field
// rectangle, texel centres are (index + 0.5) / count, UV-to-sample mapping is
// uv * count - 0.5, and local mask cell positions place centres at
// 0.5, 1.5, ... count - 0.5. Keep these formulas in lockstep with
// StylizedRiverFoamTopologyFieldSpace.cs.

float FoamTexelCentreUV1D(int index, int count)
{
    return ((float)index + 0.5) / max(1.0, (float)count);
}


float2 FoamTexelCentreUV(uint2 coordinate, int2 dimensions)
{
    return ((float2)coordinate + 0.5) /
        max(float2(1.0, 1.0), (float2)dimensions);
}


float FoamLocalDistanceAtTexel(int x)
{
    return FoamTexelCentreUV1D(x, _FoamDimensions.x) *
        _FoamFieldLength;
}


float FoamAcross01AtTexel(int y)
{
    return FoamTexelCentreUV1D(y, _FoamDimensions.y);
}


float2 FoamUVToTexelCoordinate(float2 uv, int2 dimensions)
{
    return clamp(
        saturate(uv) * (float2)dimensions - 0.5,
        float2(0.0, 0.0),
        max(float2(0.0, 0.0), (float2)dimensions - 1.0));
}


int FoamUVToContainingTexel(float u, int count)
{
    return clamp(
        (int)floor(saturate(u) * max(1.0, (float)count)),
        0,
        max(0, count - 1));
}


bool ResolveFoamCellBilinearCoordinates(
    float2 cellPosition,
    int2 dimensions,
    out int2 p0,
    out int2 p1,
    out float2 blend)
{
    p0 = int2(0, 0);
    p1 = int2(0, 0);
    blend = float2(0.0, 0.0);
    if (any(cellPosition < 0.0) ||
        any(cellPosition > (float2)dimensions))
    {
        return false;
    }

    float2 coordinate = clamp(
        cellPosition - 0.5,
        float2(0.0, 0.0),
        max(float2(0.0, 0.0), (float2)dimensions - 1.0));
    p0 = (int2)floor(coordinate);
    p1 = min(p0 + 1, dimensions - 1);
    blend = coordinate - p0;
    return true;
}


bool IsFoamColumnInsideDomain(int x)
{
    float localDistance = FoamLocalDistanceAtTexel(x);
    return localDistance <= _FoamValidLength + 0.0001;
}


bool IsFoamUInsideDomain(float u)
{
    return saturate(u) * _FoamFieldLength <=
        _FoamValidLength + 0.0001;
}


bool IsFoamColumnInsideSimulation(int x)
{
    float localDistance = FoamLocalDistanceAtTexel(x);
    return localDistance <= _FoamSimulationLength + 0.0001;
}


int ClampX(int x)
{
    return clamp(x, 0, _FoamDimensions.x - 1);
}


int ClampY(int y)
{
    return clamp(y, 0, _FoamDimensions.y - 1);
}


float4 LoadState(int2 coordinate)
{
    return _FoamStateRead.Load(
        int3(ClampX(coordinate.x), ClampY(coordinate.y), 0));
}


float LoadBoundaryCoverage(int2 coordinate)
{
    return saturate(_FoamBoundary.Load(
        int3(ClampX(coordinate.x), ClampY(coordinate.y), 0)).r);
}


float LoadObstacleExclusionCell(int2 coordinate)
{
    if (coordinate.x < 0 || coordinate.x >= _FoamDimensions.x ||
        coordinate.y < 0 || coordinate.y >= _FoamDimensions.y)
    {
        return 0.0;
    }

    return saturate(_FoamObstacleExclusionRead.Load(
        int3(coordinate, 0)));
}


float FoamAcross01ToMetres(
    float across01,
    float leftHalfWidth,
    float rightHalfWidth)
{
    if (across01 <= 0.5)
    {
        return -leftHalfWidth * (1.0 - across01 * 2.0);
    }

    return rightHalfWidth * (across01 * 2.0 - 1.0);
}


float FoamMetresToAcross01(
    float lateralMetres,
    float leftHalfWidth,
    float rightHalfWidth)
{
    if (lateralMetres <= 0.0)
    {
        return 0.5 * (1.0 + lateralMetres /
            max(0.0001, leftHalfWidth));
    }

    return 0.5 * (1.0 + lateralMetres /
        max(0.0001, rightHalfWidth));
}
