
float SampleMajorMaskBilinear(
    int sliceIndex,
    float2 cellPosition)
{
    int2 dimensions = _FoamMajorMaskDimensions;
    int2 p0;
    int2 p1;
    float2 blend;
    if (!ResolveFoamCellBilinearCoordinates(
            cellPosition,
            dimensions,
            p0,
            p1,
            blend))
    {
        return 0.0;
    }
    float a = _FoamMajorMasks.Load(int4(p0, sliceIndex, 0)).r;
    float b = _FoamMajorMasks.Load(
        int4(int2(p1.x, p0.y), sliceIndex, 0)).r;
    float c = _FoamMajorMasks.Load(
        int4(int2(p0.x, p1.y), sliceIndex, 0)).r;
    float d = _FoamMajorMasks.Load(int4(p1, sliceIndex, 0)).r;
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}


float SampleHostedNegativeMaskBilinear(
    int sliceIndex,
    float2 cellPosition)
{
    int2 dimensions = _FoamHostedNegativeMaskDimensions;
    int2 p0;
    int2 p1;
    float2 blend;
    if (!ResolveFoamCellBilinearCoordinates(
            cellPosition,
            dimensions,
            p0,
            p1,
            blend))
    {
        return 0.0;
    }
    float a = _FoamHostedNegativeMasks.Load(
        int4(p0, sliceIndex, 0)).r;
    float b = _FoamHostedNegativeMasks.Load(
        int4(int2(p1.x, p0.y), sliceIndex, 0)).r;
    float c = _FoamHostedNegativeMasks.Load(
        int4(int2(p0.x, p1.y), sliceIndex, 0)).r;
    float d = _FoamHostedNegativeMasks.Load(
        int4(p1, sliceIndex, 0)).r;
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}


float SampleFreeWaterNegativeMaskBilinear(
    int sliceIndex,
    float2 cellPosition)
{
    int2 dimensions = _FoamFreeWaterMaskDimensions;
    int2 p0;
    int2 p1;
    float2 blend;
    if (!ResolveFoamCellBilinearCoordinates(
            cellPosition,
            dimensions,
            p0,
            p1,
            blend))
    {
        return 0.0;
    }
    float a = _FoamFreeWaterNegativeMasks.Load(
        int4(p0, sliceIndex, 0)).r;
    float b = _FoamFreeWaterNegativeMasks.Load(
        int4(int2(p1.x, p0.y), sliceIndex, 0)).r;
    float c = _FoamFreeWaterNegativeMasks.Load(
        int4(int2(p0.x, p1.y), sliceIndex, 0)).r;
    float d = _FoamFreeWaterNegativeMasks.Load(
        int4(p1, sliceIndex, 0)).r;
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}


bool ResolveMajorCandidatePosition(
    FoamMajorEvolutionData region,
    FoamMetricRow metric,
    float localDistance,
    float lateralMetres,
    bool applyMajorSupportExtentCull,
    out float2 candidatePosition)
{
    float centreAcrossNormalized = clamp(
        region.centreAndPlacement.y,
        -1.0,
        1.0);
    float centreLateralMetres = centreAcrossNormalized < 0.0
        ? centreAcrossNormalized * max(0.01, metric.shoreData.x)
        : centreAcrossNormalized * max(0.01, metric.shoreData.y);
    float2 delta = float2(
        localDistance - region.centreAndPlacement.x,
        lateralMetres - centreLateralMetres);
    float orientation = region.centreAndPlacement.z;
    float orientationCosine = cos(orientation);
    float orientationSine = sin(orientation);
    float principalMajorMetres =
        orientationCosine * delta.x +
        orientationSine * delta.y;
    float principalMinorMetres =
        -orientationSine * delta.x +
        orientationCosine * delta.y;

    float metresPerCell = max(
        0.0001,
        region.centreAndPlacement.w);
    float sourceMajor = principalMajorMetres /
        max(0.0001, metresPerCell * region.morph.x);
    float sourceMinor = principalMinorMetres /
        max(0.0001, metresPerCell * region.morph.y);
    sourceMajor -= sourceMinor * region.morph.z;

    float majorExtent = max(0.5, region.candidateExtents.x);
    float minorExtent = max(0.5, region.candidateExtents.y);
    float conservativeMajorExtent = majorExtent *
        (1.0 + abs(region.warp.x) * 1.55) + 3.0;
    float conservativeMinorExtent = minorExtent *
        (1.0 + abs(region.warp.y) * 1.55) + 3.0;
    if (applyMajorSupportExtentCull &&
        (abs(sourceMajor) > conservativeMajorExtent ||
         abs(sourceMinor) > conservativeMinorExtent))
    {
        candidatePosition = 0.0.xx;
        return false;
    }

    float normalMajor = sourceMajor / majorExtent;
    float normalMinor = sourceMinor / minorExtent;
    sourceMajor +=
        sin(normalMinor * 3.35 + region.warp.z) *
            region.warp.x * majorExtent +
        sin((normalMajor + normalMinor) * 1.85 + region.warp.w) *
            region.warp.x * majorExtent * 0.42;
    sourceMinor +=
        sin(normalMajor * 2.80 + region.warp.w) *
            region.warp.y * minorExtent +
        sin((normalMajor - normalMinor) * 2.10 + region.warp.z) *
            region.warp.y * minorExtent * 0.36;

    float principalAngle = region.candidateShape.z;
    float principalCosine = cos(principalAngle);
    float principalSine = sin(principalAngle);
    candidatePosition = float2(
        region.candidateShape.x +
            principalCosine * sourceMajor -
            principalSine * sourceMinor,
        region.candidateShape.y +
            principalSine * sourceMajor +
            principalCosine * sourceMinor);
    return true;
}


float FoamUnitySmoothStep(float fromValue, float toValue, float t)
{
    t = saturate(t);
    t = t * t * (3.0 - 2.0 * t);
    return lerp(fromValue, toValue, t);
}


float FoamEdgeSmoothStep(float edge0, float edge1, float value)
{
    if (edge1 <= edge0 + 0.000001)
    {
        return value >= edge1 ? 1.0 : 0.0;
    }

    float t = saturate((value - edge0) / (edge1 - edge0));
    return t * t * (3.0 - 2.0 * t);
}


float FoamDistancePointToSegment(
    float2 samplePosition,
    float2 segmentStart,
    float2 segmentEnd)
{
    float2 segment = segmentEnd - segmentStart;
    float denominator = dot(segment, segment);
    if (denominator <= 0.000001)
    {
        return length(samplePosition - segmentStart);
    }

    float t = saturate(
        dot(samplePosition - segmentStart, segment) / denominator);
    return length(samplePosition - (segmentStart + segment * t));
}


uint FoamIdentityMixBits(uint value)
{
    value ^= value >> 16;
    value *= 0x7FEB352Du;
    value ^= value >> 15;
    value *= 0x846CA68Bu;
    value ^= value >> 16;
    return value;
}


float FoamPocketHash01(uint value, uint salt)
{
    uint mixed = FoamIdentityMixBits(value ^ salt * 0x9E3779B9u);
    return (float)(mixed & 0x00FFFFFFu) / 16777215.0;
}


float FoamWeakSpanHashLattice(int x, int y, uint seed)
{
    uint value = seed ^
        (uint)x * 0x8DA6B343u ^
        (uint)y * 0xD8163841u;
    return FoamPocketHash01(
        FoamIdentityMixBits(value),
        0xCB1AB31Fu);
}


float FoamWeakSpanValueNoise(float2 position, uint seed)
{
    int2 baseCell = (int2)floor(position);
    float2 fraction = position - (float2)baseCell;
    fraction = saturate(fraction);
    fraction = fraction * fraction * (3.0 - 2.0 * fraction);
    float a = FoamWeakSpanHashLattice(baseCell.x, baseCell.y, seed);
    float b = FoamWeakSpanHashLattice(baseCell.x + 1, baseCell.y, seed);
    float c = FoamWeakSpanHashLattice(baseCell.x, baseCell.y + 1, seed);
    float d = FoamWeakSpanHashLattice(
        baseCell.x + 1,
        baseCell.y + 1,
        seed);
    return lerp(lerp(a, b, fraction.x), lerp(c, d, fraction.x), fraction.y);
}


float EvaluateConnectorIdentitySupport(
    float2 metricPosition,
    float currentMajorSupport)
{
    float support = 0.0;
    [loop]
    for (int connectorIndex = 0;
         connectorIndex < _FoamConnectorIdentityCount;
         connectorIndex++)
    {
        FoamConnectorIdentityData connector =
            _FoamConnectorIdentityRecords[connectorIndex];
        int pointOffset = max(
            0,
            (int)round(connector.pointRangeAndRadii.x));
        int pointCount = max(
            0,
            (int)round(connector.pointRangeAndRadii.y));
        if (pointCount < 2)
        {
            continue;
        }

        float minimumDistance = 1.0e20;
        [loop]
        for (int pointIndex = 1;
             pointIndex < pointCount;
             pointIndex++)
        {
            float2 segmentStart = _FoamConnectorPathPoints[
                pointOffset + pointIndex - 1].xy;
            float2 segmentEnd = _FoamConnectorPathPoints[
                pointOffset + pointIndex].xy;
            minimumDistance = min(
                minimumDistance,
                FoamDistancePointToSegment(
                    metricPosition,
                    segmentStart,
                    segmentEnd));
        }

        float outerRadius = max(0.0001, connector.pointRangeAndRadii.z);
        if (minimumDistance > outerRadius || currentMajorSupport >= 0.42)
        {
            continue;
        }

        // Preserve the accepted CPU raster exactly. Unity's Mathf.SmoothStep
        // treats the distance as interpolation t; it is not an edge-based
        // smoothstep call.
        float coreRadius = connector.pointRangeAndRadii.w;
        float value = 1.0 - FoamUnitySmoothStep(
            coreRadius,
            outerRadius,
            minimumDistance);
        float majorSuppression = 1.0 - FoamUnitySmoothStep(
            0.16,
            0.42,
            saturate(currentMajorSupport));
        support = max(support, value * majorSuppression);
    }

    return saturate(support);
}


bool SampleConnectorIdentityPath(
    int connectorIndex,
    float normalizedDistance,
    float fallbackOrientation,
    out float2 position,
    out float2 tangent)
{
    position = 0.0.xx;
    tangent = float2(cos(fallbackOrientation), sin(fallbackOrientation));
    if (connectorIndex < 0 ||
        connectorIndex >= _FoamConnectorIdentityCount)
    {
        return false;
    }

    FoamConnectorIdentityData connector =
        _FoamConnectorIdentityRecords[connectorIndex];
    int pointOffset = max(
        0,
        (int)round(connector.pointRangeAndRadii.x));
    int pointCount = max(
        0,
        (int)round(connector.pointRangeAndRadii.y));
    if (pointCount < 2)
    {
        return false;
    }

    float target = saturate(normalizedDistance);
    float4 firstPoint = _FoamConnectorPathPoints[pointOffset];
    position = firstPoint.xy;
    [loop]
    for (int pointIndex = 1;
         pointIndex < pointCount;
         pointIndex++)
    {
        float4 segmentStart = _FoamConnectorPathPoints[
            pointOffset + pointIndex - 1];
        float4 segmentEnd = _FoamConnectorPathPoints[
            pointOffset + pointIndex];
        float startDistance = saturate(segmentStart.z);
        float endDistance = saturate(segmentEnd.z);
        float2 segment = segmentEnd.xy - segmentStart.xy;
        float segmentLength = length(segment);
        if (segmentLength <= 0.0001)
        {
            continue;
        }
        if (target > endDistance && pointIndex < pointCount - 1)
        {
            continue;
        }

        float segmentRange = max(0.000001, endDistance - startDistance);
        float t = saturate((target - startDistance) / segmentRange);
        position = lerp(segmentStart.xy, segmentEnd.xy, t);
        tangent = segment / segmentLength;
        return true;
    }

    return false;
}


float EvaluateWeakSpanIdentityPressure(
    float2 metricPosition,
    float currentMajorSupport,
    float connectorSupport)
{
    if (currentMajorSupport >= 0.30 || connectorSupport < 0.045)
    {
        return 0.0;
    }

    float pressure = 0.0;
    [loop]
    for (int weakSpanIndex = 0;
         weakSpanIndex < _FoamWeakSpanIdentityCount;
         weakSpanIndex++)
    {
        FoamWeakSpanIdentityData weakSpan =
            _FoamWeakSpanIdentityRecords[weakSpanIndex];
        int connectorIndex = (int)round(weakSpan.connectorAndPath.x);
        float normalizedDistance = clamp(
            weakSpan.connectorAndPath.y,
            weakSpan.connectorAndPath.z,
            weakSpan.connectorAndPath.w);
        float2 centre;
        float2 pathTangent;
        if (!SampleConnectorIdentityPath(
                connectorIndex,
                normalizedDistance,
                weakSpan.shape.w,
                centre,
                pathTangent))
        {
            continue;
        }

        // Patch 4.7C.3 follows the current deformed Connector tangent. The
        // accepted orientation remains only the sampling fallback when a path
        // segment is degenerate.
        float2 tangent = pathTangent;
        float2 normal = float2(-tangent.y, tangent.x);
        float2 delta = metricPosition - centre;
        float along = dot(delta, tangent) /
            max(0.0001, weakSpan.shape.x);
        float across = dot(delta, normal) /
            max(0.0001, weakSpan.shape.y);
        float radial = sqrt(along * along + across * across);
        if (radial >= 1.0)
        {
            continue;
        }

        float noise = FoamWeakSpanValueNoise(
            metricPosition * 4.2,
            weakSpan.noiseAndFlags.x);
        radial += (noise - 0.5) * 0.16;
        float envelope = 1.0 - FoamEdgeSmoothStep(0.54, 1.0, radial);
        float connectorMask = FoamEdgeSmoothStep(
            0.045,
            0.20,
            connectorSupport);
        float majorProtection = 1.0 - FoamEdgeSmoothStep(
            0.10,
            0.30,
            currentMajorSupport);
        float value =
            envelope * weakSpan.shape.z * connectorMask * majorProtection;
        if (value > 0.0001)
        {
            pressure = max(pressure, value);
        }
    }

    return saturate(pressure);
}
