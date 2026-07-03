
float FoamHash11(float value)
{
    return frac(sin(value * 127.1 + 311.7) * 43758.5453123);
}


float2 FoamHash22(float2 value)
{
    float2 result;
    result.x = frac(sin(dot(value, float2(127.1, 311.7))) * 43758.5453123);
    result.y = frac(sin(dot(value, float2(269.5, 183.3))) * 43758.5453123);
    return result;
}


float FoamSourceFillValueNoise(float2 position, float seed)
{
    float2 baseCell = floor(position);
    float2 local = frac(position);
    local = local * local * (3.0 - 2.0 * local);

    float seedOffset = seed * 0.071;
    float a = FoamHash11(
        dot(baseCell, float2(37.17, 91.73)) + seedOffset);
    float b = FoamHash11(
        dot(baseCell + float2(1.0, 0.0), float2(37.17, 91.73)) +
        seedOffset);
    float c = FoamHash11(
        dot(baseCell + float2(0.0, 1.0), float2(37.17, 91.73)) +
        seedOffset);
    float d = FoamHash11(
        dot(baseCell + float2(1.0, 1.0), float2(37.17, 91.73)) +
        seedOffset);
    return lerp(lerp(a, b, local.x), lerp(c, d, local.x), local.y);
}


float EvaluateFoamSourceFillField(
    float2 physicalPosition,
    float sourceFillSeed,
    float featureSize)
{
    float safeFeatureSize = max(0.05, featureSize);
    float2 seedOffset = float2(
        FoamHash11(sourceFillSeed + 17.13),
        FoamHash11(sourceFillSeed + 43.71)) * 23.0;
    float2 basePosition = physicalPosition / safeFeatureSize + seedOffset;

    float broad = FoamSourceFillValueNoise(
        basePosition * 0.72,
        sourceFillSeed + 11.0);
    float secondary = FoamSourceFillValueNoise(
        mul(float2x2(0.82, -0.37, 0.37, 0.82), basePosition) * 1.43 +
        float2(7.3, 19.1),
        sourceFillSeed + 29.0);
    float combined = saturate(broad * 0.78 + secondary * 0.22);
    // Stretch the naturally centre-heavy value-noise distribution so Amount
    // changes produce useful area differences across the full 0-1 range.
    return smoothstep(0.12, 0.88, combined);
}


float EvaluateFoamSourceFillCoverage(
    float2 physicalPosition,
    float sourceAmount,
    float sourceFillSeed,
    float requestedFeatureSize,
    float2 physicalCellSpacing)
{
    float amount = saturate(sourceAmount);
    if (amount <= 0.0001)
    {
        return 0.0;
    }
    if (amount >= 0.9999)
    {
        return 1.0;
    }

    // Keep coherent source islands at least a few structural texels wide on
    // every quality tier. This is source rasterization only, not persistent
    // material evolution.
    float maximumCellSpacing = max(
        max(0.01, physicalCellSpacing.x),
        max(0.01, physicalCellSpacing.y));
    float featureSize = max(
        requestedFeatureSize,
        maximumCellSpacing * 2.5);
    float fillValue = EvaluateFoamSourceFillField(
        physicalPosition,
        sourceFillSeed,
        featureSize);
    float transitionWidth = clamp(
        maximumCellSpacing / featureSize * 0.30,
        0.025,
        0.12);

    // Thresholding one fixed field makes every higher Amount a nested
    // superset of every lower Amount for the same source and coordinates.
    return smoothstep(
        fillValue - transitionWidth,
        fillValue + transitionWidth,
        amount);
}


float PhaseDistance(float a, float b)
{
    float difference = abs(frac(a - b + 0.5) - 0.5);
    return difference * 2.0;
}


float MixPhaseShortest(float fromPhase, float toPhase, float blend)
{
    float delta = frac(toPhase - fromPhase + 0.5) - 0.5;
    return frac(fromPhase + delta * saturate(blend) + 1.0);
}


float FoamEllipseMask(float2 samplePosition, float2 centre, float2 radius)
{
    float2 safeRadius = max(radius, float2(0.04, 0.04));
    float distanceValue = length((samplePosition - centre) / safeRadius);
    return 1.0 - smoothstep(0.78, 1.0, distanceValue);
}


float EvaluateCompoundInjectionShape(float alongDistance, float acrossDistance)
{
    float variety = saturate(_FoamInjectionShapeVariety);
    float seed = _FoamInjectionShapeSeed * 97.31 + _FoamSeed * 0.017;
    float2 samplePosition = float2(alongDistance, acrossDistance);

    float bend = (FoamHash11(seed + 1.0) - 0.5) * 0.72 * variety;
    float skew = (FoamHash11(seed + 2.0) - 0.5) * 0.34 * variety;
    float taperDirection = FoamHash11(seed + 3.0) < 0.5 ? -1.0 : 1.0;
    float along01 = saturate(alongDistance * 0.5 * taperDirection + 0.5);
    float centreLine = bend * (alongDistance * alongDistance - 0.28) +
        skew * alongDistance;
    float mainHeadWidth = lerp(0.30, 0.68, FoamHash11(seed + 4.0));
    float mainTailWidth = mainHeadWidth *
        lerp(0.22, 0.52, FoamHash11(seed + 4.5));
    float mainWidth = lerp(mainHeadWidth, mainTailWidth, along01);
    float mainAlong = 1.0 - smoothstep(0.76, 1.04, abs(alongDistance));
    float mainAcross = 1.0 - smoothstep(
        mainWidth * 0.70,
        mainWidth,
        abs(acrossDistance - centreLine));
    float mainTongue = saturate(mainAlong * mainAcross);

    float secondarySide = FoamHash11(seed + 5.0) < 0.5 ? -1.0 : 1.0;
    float2 secondaryCentre = float2(
        lerp(-0.30, 0.38, FoamHash11(seed + 6.0)),
        secondarySide * lerp(0.20, 0.48, FoamHash11(seed + 7.0)) * variety);
    float2 secondaryRadius = float2(
        lerp(0.24, 0.62, FoamHash11(seed + 8.0)),
        lerp(0.10, 0.28, FoamHash11(seed + 9.0)));
    float secondaryTongue = FoamEllipseMask(
        samplePosition,
        secondaryCentre,
        secondaryRadius) * variety;

    float tailAlong =
        taperDirection * lerp(0.58, 0.84, FoamHash11(seed + 10.0));
    float tailCentreLine =
        bend * (tailAlong * tailAlong - 0.28) + skew * tailAlong;
    float2 tailCentre = float2(
        tailAlong,
        tailCentreLine * 0.45 +
        (FoamHash11(seed + 11.0) - 0.5) * 0.28 * variety);
    float tail = FoamEllipseMask(
        samplePosition,
        tailCentre,
        float2(
            lerp(0.14, 0.32, FoamHash11(seed + 12.0)),
            lerp(0.07, 0.18, FoamHash11(seed + 13.0)))) *
        lerp(0.35, 1.0, variety);

    float satelliteEnabled = step(0.48, FoamHash11(seed + 14.0)) * variety;
    float2 satelliteCentre = float2(
        lerp(-0.68, 0.68, FoamHash11(seed + 15.0)),
        lerp(-0.72, 0.72, FoamHash11(seed + 16.0)));
    float satellite = FoamEllipseMask(
        samplePosition,
        satelliteCentre,
        float2(
            lerp(0.12, 0.25, FoamHash11(seed + 17.0)),
            lerp(0.10, 0.22, FoamHash11(seed + 18.0)))) *
        satelliteEnabled;

    float combined = 1.0 -
        (1.0 - mainTongue) *
        (1.0 - secondaryTongue) *
        (1.0 - tail) *
        (1.0 - satellite);

    float2 cutCentre = float2(
        lerp(-0.42, 0.42, FoamHash11(seed + 19.0)),
        lerp(-0.28, 0.28, FoamHash11(seed + 20.0)));
    float cut = FoamEllipseMask(
        samplePosition,
        cutCentre,
        float2(
            lerp(0.14, 0.30, FoamHash11(seed + 21.0)),
            lerp(0.12, 0.26, FoamHash11(seed + 22.0))));
    combined = saturate(combined - cut * variety * 0.58);
    return combined;
}



float FoamValueNoise(float2 position)
{
    float2 baseCell = floor(position);
    float2 local = frac(position);
    local = local * local * (3.0 - 2.0 * local);

    float a = FoamHash11(dot(baseCell, float2(17.17, 61.73)) + _FoamSeed * 0.011);
    float b = FoamHash11(dot(baseCell + float2(1.0, 0.0), float2(17.17, 61.73)) + _FoamSeed * 0.011);
    float c = FoamHash11(dot(baseCell + float2(0.0, 1.0), float2(17.17, 61.73)) + _FoamSeed * 0.011);
    float d = FoamHash11(dot(baseCell + float2(1.0, 1.0), float2(17.17, 61.73)) + _FoamSeed * 0.011);
    return lerp(lerp(a, b, local.x), lerp(c, d, local.x), local.y);
}


float FoamFbm(float2 position)
{
    float value = 0.0;
    float amplitude = 0.55;
    float2 samplePosition = position;

    [unroll]
    for (int octave = 0; octave < 4; octave++)
    {
        value += FoamValueNoise(samplePosition) * amplitude;
        samplePosition = mul(
            float2x2(1.37, -1.11, 1.11, 1.37),
            samplePosition) + float2(13.7, 29.3);
        amplitude *= 0.48;
    }

    return saturate(value);
}


float VoronoiEdgeDistance(float2 position, float timeValue, float seedValue)
{
    float2 baseCell = floor(position);
    float2 local = frac(position);
    float nearest = 1000.0;
    float secondNearest = 1000.0;

    [unroll]
    for (int offsetY = -1; offsetY <= 1; offsetY++)
    {
        [unroll]
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            float2 offset = float2((float)offsetX, (float)offsetY);
            float2 hashValue = FoamHash22(baseCell + offset + seedValue);
            float regionalPhase = FoamHash11(
                dot(baseCell + offset, float2(23.71, 47.13)) +
                seedValue * 0.91);
            float2 centre = 0.5 + (hashValue - 0.5) * 0.78;
            centre += float2(
                sin(timeValue * lerp(0.19, 0.43, regionalPhase) +
                    hashValue.x * 6.2831853),
                cos(timeValue * lerp(0.17, 0.39, 1.0 - regionalPhase) +
                    hashValue.y * 6.2831853)) *
                lerp(0.055, 0.14, regionalPhase);
            float2 delta = offset + centre - local;
            delta.x *= 0.78;
            float distanceSquared = dot(delta, delta);

            if (distanceSquared < nearest)
            {
                secondNearest = nearest;
                nearest = distanceSquared;
            }
            else if (distanceSquared < secondNearest)
            {
                secondNearest = distanceSquared;
            }
        }
    }

    return max(
        0.0,
        sqrt(max(0.0, secondNearest)) - sqrt(max(0.0, nearest)));
}
