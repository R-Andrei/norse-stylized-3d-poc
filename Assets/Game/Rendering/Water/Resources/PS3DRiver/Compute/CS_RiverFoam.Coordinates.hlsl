// Canonical Stage 6 topology-field contract. UV spans the complete field
// rectangle, texel centres are (index + 0.5) / count, UV-to-sample mapping is
// uv * count - 0.5, and local mask cell positions place centres at
// 0.5, 1.5, ... count - 0.5. Keep these formulas in lockstep with
// StylizedRiverFoamTopologyFieldSpace.cs.

FoamGridDescriptorData LoadFoamGridDescriptor()
{
    FoamGridDescriptorData descriptor;
    descriptor.contract = _FoamGridDescriptorContract;
    descriptor.spacing = _FoamGridDescriptorSpacing;
    descriptor.lateral = _FoamGridDescriptorLateral;
    descriptor.longitudinal = _FoamGridDescriptorLongitudinal;
    descriptor.extent = _FoamGridDescriptorExtent;
    return descriptor;
}

bool FoamGridUsesFixedMetricLattice()
{
    return (int)round(_FoamGridDescriptorContract.z) == 1;
}

float FoamTexelCentreUV1D(int index, int count)
{
    return ((float)index + 0.5) / max(1.0, (float)count);
}


float2 FoamTexelCentreUV(uint2 coordinate, int2 dimensions)
{
    return ((float2)coordinate + 0.5) /
        max(float2(1.0, 1.0), (float2)dimensions);
}


void FoamResolveFilmStructuralRange1D(
    int filmIndex,
    int structuralCount,
    out int structuralStart,
    out int representedCount)
{
    structuralStart = max(0, filmIndex * 2);
    representedCount = clamp(
        structuralCount - structuralStart,
        0,
        2);
}


float FoamFilmTexelCentreFieldUV1D(
    int filmIndex,
    int structuralCount)
{
    int structuralStart;
    int representedCount;
    FoamResolveFilmStructuralRange1D(
        filmIndex,
        structuralCount,
        structuralStart,
        representedCount);
    return ((float)structuralStart +
        0.5 * max(1.0, (float)representedCount)) /
        max(1.0, (float)structuralCount);
}


float2 FoamFilmTexelCentreFieldUV(int2 filmCoordinate)
{
    return float2(
        FoamFilmTexelCentreFieldUV1D(
            filmCoordinate.x,
            _FoamDimensions.x),
        FoamFilmTexelCentreFieldUV1D(
            filmCoordinate.y,
            _FoamDimensions.y));
}


float FoamFieldUVToFilmUV1D(
    float fieldUV,
    int structuralCount,
    int filmCount)
{
    int safeStructuralCount = max(1, structuralCount);
    int safeFilmCount = max(1, filmCount);
    float structuralPosition =
        saturate(fieldUV) * (float)safeStructuralCount;
    int filmIndex = min(
        (int)floor(structuralPosition * 0.5),
        safeFilmCount - 1);
    int structuralStart;
    int representedCount;
    FoamResolveFilmStructuralRange1D(
        filmIndex,
        safeStructuralCount,
        structuralStart,
        representedCount);
    float localPosition = saturate(
        (structuralPosition - (float)structuralStart) /
        max(1.0, (float)representedCount));
    return ((float)filmIndex + localPosition) /
        (float)safeFilmCount;
}


float2 FoamFieldUVToFilmUV(float2 fieldUV)
{
    if (!FoamGridUsesFixedMetricLattice())
    {
        return saturate(fieldUV);
    }

    return float2(
        FoamFieldUVToFilmUV1D(
            fieldUV.x,
            _FoamDimensions.x,
            _FoamFilmDimensions.x),
        FoamFieldUVToFilmUV1D(
            fieldUV.y,
            _FoamDimensions.y,
            _FoamFilmDimensions.y));
}


float FoamGridLocalDistanceAtUV(float u)
{
    if (FoamGridUsesFixedMetricLattice())
    {
        return _FoamGridDescriptorLongitudinal.x +
            saturate(u) * _FoamGridDescriptorLongitudinal.y;
    }

    return saturate(u) * _FoamFieldLength;
}


float FoamGridLocalDistanceAtTexel(int x)
{
    if (FoamGridUsesFixedMetricLattice())
    {
        return _FoamGridDescriptorLongitudinal.x +
            ((float)x + 0.5) * _FoamGridDescriptorSpacing.z;
    }

    return FoamTexelCentreUV1D(x, _FoamDimensions.x) *
        _FoamFieldLength;
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


bool IsFoamGridColumnInsideDomain(int x)
{
    float localDistance = FoamGridLocalDistanceAtTexel(x);
    return localDistance <=
        _FoamGridDescriptorLongitudinal.x +
        _FoamValidLength + 0.0001;
}


bool IsFoamUInsideDomain(float u)
{
    return saturate(u) * _FoamFieldLength <=
        _FoamValidLength + 0.0001;
}


bool IsFoamGridUInsideDomain(float u)
{
    return FoamGridLocalDistanceAtUV(u) <=
        _FoamGridDescriptorLongitudinal.x +
        _FoamValidLength + 0.0001;
}


bool IsFoamColumnInsideSimulation(int x)
{
    float localDistance = FoamLocalDistanceAtTexel(x);
    return localDistance <= _FoamSimulationLength + 0.0001;
}

bool IsFoamGridColumnInsideSimulation(int x)
{
    if (!FoamGridUsesFixedMetricLattice())
    {
        return IsFoamColumnInsideSimulation(x);
    }

    float localDistance = FoamGridLocalDistanceAtTexel(x);
    return localDistance <=
        _FoamGridDescriptorLongitudinal.x +
        _FoamSimulationLength + 0.0001;
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


float FoamLateralMetresAtUV(
    float v,
    FoamMetricRow metric)
{
    if (FoamGridUsesFixedMetricLattice())
    {
        float localY = saturate(v) * _FoamGridDescriptorLateral.z - 0.5;
        float globalY = _FoamGridDescriptorLateral.y + localY;
        return _FoamGridDescriptorLateral.x +
            globalY * _FoamGridDescriptorSpacing.w;
    }

    return FoamAcross01ToMetres(
        saturate(v),
        metric.widthsAndSpacing.x,
        metric.widthsAndSpacing.y);
}


float FoamLateralMetresAtTexel(
    int y,
    FoamMetricRow metric)
{
    return FoamLateralMetresAtUV(
        FoamTexelCentreUV1D(y, _FoamDimensions.y),
        metric);
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


float2 ResolveFoamExternalFieldUV(float2 foamUV)
{
    float2 legacyUV = saturate(foamUV);
    if (!FoamGridUsesFixedMetricLattice())
    {
        return legacyUV;
    }

    int metricX = FoamUVToContainingTexel(
        legacyUV.x,
        _FoamDimensions.x);
    FoamMetricRow metric = _FoamMetricRows[metricX];
    float localDistanceMetres = FoamGridLocalDistanceAtUV(legacyUV.x);
    float lateralMetres = FoamLateralMetresAtUV(legacyUV.y, metric);
    float externalU =
        (localDistanceMetres - _FoamGridDescriptorLongitudinal.x) /
        max(0.0001, _FoamGridDescriptorLongitudinal.y);
    float externalV = FoamMetresToAcross01(
        lateralMetres,
        metric.widthsAndSpacing.x,
        metric.widthsAndSpacing.y);
    return saturate(float2(externalU, externalV));
}
