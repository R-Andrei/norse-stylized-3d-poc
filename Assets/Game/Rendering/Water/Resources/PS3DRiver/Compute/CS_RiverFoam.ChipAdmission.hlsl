#ifndef PS3D_RIVER_FOAM_CHIP_ADMISSION_INCLUDED
#define PS3D_RIVER_FOAM_CHIP_ADMISSION_INCLUDED

Texture2D<float4> _FoamChipAdmissionPreviousStateRead;
Texture2D<float4> _FoamChipAdmissionCurrentStateRead;
RWTexture2D<float> _FoamChipStraddleAdmissionWrite;

int2 _FoamChipAdmissionDimensions;
int2 _FoamChipAdmissionOrigin;
int _FoamChipAdmissionRecordCount;
int _FoamChipAdmissionHistoryValid;
float _FoamChipAdmissionInterpolation;
float _FoamChipAdmissionEvolutionTime;
float _FoamChipAdmissionCandidateSpacing;
float _FoamChipAdmissionActivation;
float _FoamChipAdmissionSize;
float _FoamChipAdmissionIrregularity;
float _FoamChipAdmissionMaximumViewScale;
float _FoamChipAdmissionFieldSpeed;
float _FoamChipAdmissionFormationTime;
float _FoamChipAdmissionStableTime;
float _FoamChipAdmissionDissolveTime;
float _FoamChipAdmissionDormantTime;
float _FoamChipAdmissionLateralMotionAmount;
float _FoamChipAdmissionLateralMotionSpeed;
float _FoamChipAdmissionRotationAmountDegrees;
float _FoamChipAdmissionRotationSpeed;
float _FoamChipAdmissionSizePulseAmount;
float _FoamChipAdmissionSizePulseSpeed;
float _FoamChipAdmissionShapeChangeAmount;
float _FoamChipAdmissionShapeChangeSpeed;
float _FoamChipAdmissionShapeTransitionTime;
float _FoamChipAdmissionSharpness;
float _FoamChipAdmissionFinalVisibilityMode;
float _FoamChipAdmissionStrandStrength;
float _FoamChipAdmissionStrandScale;
float _FoamChipAdmissionStrandDensity;
float _FoamChipAdmissionStrandReach;
float _FoamChipAdmissionSupportFootprintMetres;

float FoamChipAdmissionHash21(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float FoamChipAdmissionValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = FoamChipAdmissionHash21(i);
    float b = FoamChipAdmissionHash21(i + float2(1.0, 0.0));
    float c = FoamChipAdmissionHash21(i + float2(0.0, 1.0));
    float d = FoamChipAdmissionHash21(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float FoamChipAdmissionQuinticSmooth(float value)
{
    float x = saturate(value);
    return x * x * x * (x * (x * 6.0 - 15.0) + 10.0);
}

float FoamChipAdmissionSmoothPeriodicWave(float phase)
{
    float cycle = frac(phase);
    float triangleWave = 1.0 - abs(cycle * 2.0 - 1.0);
    return triangleWave * triangleWave * (3.0 - 2.0 * triangleWave);
}

float FoamChipAdmissionSignedWave(
    float timeSeconds,
    float cyclesPerSecond,
    float phaseOffset)
{
    float speed = max(0.0, cyclesPerSecond);
    if (speed <= 0.0001)
    {
        return 0.0;
    }

    return FoamChipAdmissionSmoothPeriodicWave(
        timeSeconds * speed + phaseOffset) * 2.0 - 1.0;
}

float2 FoamChipAdmissionMorphTrajectory(
    float timeSeconds,
    float changesPerSecond,
    float transitionTimeSeconds,
    float phaseOffset)
{
    float cadence = max(0.0, changesPerSecond);
    if (cadence <= 0.0001)
    {
        return float2(0.0, 0.0);
    }

    float cadencePosition = max(0.0, timeSeconds) * cadence +
        saturate(phaseOffset);
    float targetIndex = floor(cadencePosition);
    float intervalPhase = frac(cadencePosition);
    float intervalDuration = rcp(cadence);
    float effectiveTransitionTime = min(
        max(0.001, transitionTimeSeconds),
        intervalDuration);
    float transitionFraction = saturate(
        effectiveTransitionTime * cadence);
    float transitionProgress = saturate(
        intervalPhase / max(transitionFraction, 0.0001));
    float easedProgress = FoamChipAdmissionQuinticSmooth(
        transitionProgress);
    const float GoldenAngleRadians = 2.39996322973;
    float seedAngle = saturate(phaseOffset) * 6.28318530718;
    float trajectoryAngle = seedAngle +
        (targetIndex - 1.0 + easedProgress) * GoldenAngleRadians;
    float trajectorySin;
    float trajectoryCos;
    sincos(trajectoryAngle, trajectorySin, trajectoryCos);
    return float2(trajectoryCos, trajectorySin);
}

void FoamChipAdmissionLifecycle(
    float timeSeconds,
    float phaseOffset,
    out float lifeScale,
    out float stableVariationAuthority)
{
    float formation = max(0.001, _FoamChipAdmissionFormationTime);
    float stable = max(0.001, _FoamChipAdmissionStableTime);
    float dissolve = max(0.001, _FoamChipAdmissionDissolveTime);
    float dormant = max(0.001, _FoamChipAdmissionDormantTime);
    float totalDuration = formation + stable + dissolve + dormant;
    float cycleTime = frac(
        max(0.0, timeSeconds) / totalDuration + saturate(phaseOffset)) *
        totalDuration;
    float formationScale = FoamChipAdmissionQuinticSmooth(
        cycleTime / formation);
    float dissolveScale = 1.0 - FoamChipAdmissionQuinticSmooth(
        (cycleTime - formation - stable) / dissolve);
    lifeScale = saturate(min(formationScale, dissolveScale));
    float stableBlendDuration = max(0.001, min(stable * 0.25, 0.75));
    float stableBlendIn = FoamChipAdmissionQuinticSmooth(
        (cycleTime - formation) / stableBlendDuration);
    float stableBlendOut = 1.0 - FoamChipAdmissionQuinticSmooth(
        (cycleTime - (formation + stable - stableBlendDuration)) /
        stableBlendDuration);
    stableVariationAuthority = saturate(stableBlendIn * stableBlendOut);
}

void FoamChipAdmissionMorphBasis(
    float3 rawBasisU,
    float3 rawBasisV,
    out float3 basisU,
    out float3 basisV)
{
    float rawULengthSq = dot(rawBasisU, rawBasisU);
    basisU = rawULengthSq > 0.0001
        ? rawBasisU * rsqrt(rawULengthSq)
        : float3(1.0, 0.0, 0.0);
    float3 rejectedV = rawBasisV - basisU * dot(rawBasisV, basisU);
    float rejectedLengthSq = dot(rejectedV, rejectedV);
    float3 fallbackAxis = abs(basisU.x) < 0.75
        ? float3(1.0, 0.0, 0.0)
        : float3(0.0, 1.0, 0.0);
    float3 fallbackV = cross(basisU, fallbackAxis);
    fallbackV *= rsqrt(max(dot(fallbackV, fallbackV), 0.0001));
    basisV = rejectedLengthSq > 0.0001
        ? rejectedV * rsqrt(rejectedLengthSq)
        : fallbackV;
}

float FoamChipAdmissionStaticRadialScale(
    float3 cosineHarmonics,
    float shapeIrregularity,
    float3 baseCosineCoefficients)
{
    float irregularity = saturate(shapeIrregularity);
    float contourDelta = dot(baseCosineCoefficients, cosineHarmonics);
    float staticEnvelope = 1.0 + irregularity * 0.30 * (
        abs(baseCosineCoefficients.x) +
        abs(baseCosineCoefficients.y) +
        abs(baseCosineCoefficients.z));
    return saturate(
        max(0.24, 1.0 + irregularity * contourDelta) /
        max(1.0, staticEnvelope));
}

float FoamChipAdmissionRadialScale(
    float2 direction,
    float2 contourAxis,
    float shapeIrregularity,
    float3 baseCosineCoefficients,
    float3 morphBasisU,
    float3 morphBasisV,
    float2 morphTrajectory,
    float shapeChangeAmount)
{
    float2 perpendicularAxis = float2(-contourAxis.y, contourAxis.x);
    float localX = dot(direction, contourAxis);
    float localY = dot(direction, perpendicularAxis);
    float3 cosineHarmonics = float3(
        localX,
        localX * localX - localY * localY,
        localX * (localX * localX - 3.0 * localY * localY));
    float3 sineHarmonics = float3(
        localY,
        2.0 * localX * localY,
        localY * (3.0 * localX * localX - localY * localY));
    float staticRadialScale = FoamChipAdmissionStaticRadialScale(
        cosineHarmonics,
        shapeIrregularity,
        baseCosineCoefficients);
    float authority = saturate(shapeChangeAmount);
    if (authority <= 0.0001 ||
        dot(morphTrajectory, morphTrajectory) <= 0.0001)
    {
        return staticRadialScale;
    }

    float3 temporalDirection =
        morphBasisU * morphTrajectory.x +
        morphBasisV * morphTrajectory.y;
    float temporalL1 = max(
        abs(temporalDirection.x) +
        abs(temporalDirection.y) +
        abs(temporalDirection.z),
        0.0001);
    float3 temporalSineCoefficients = temporalDirection *
        (0.55 / temporalL1);
    float irregularity = saturate(shapeIrregularity);
    float3 staticCosineCoefficients =
        baseCosineCoefficients * irregularity;
    float staticEnvelope = 1.0 + irregularity * 0.30 * (
        abs(baseCosineCoefficients.x) +
        abs(baseCosineCoefficients.y) +
        abs(baseCosineCoefficients.z));
    float staticAreaProxy =
        (1.0 + 0.5 * dot(
            staticCosineCoefficients,
            staticCosineCoefficients)) /
        max(staticEnvelope * staticEnvelope, 0.0001);
    float temporalEnergy = 1.0 + 0.5 * dot(
        temporalSineCoefficients,
        temporalSineCoefficients);
    float temporalRawScale = 1.0 + dot(
        temporalSineCoefficients,
        sineHarmonics);
    float temporalRadialScale = sqrt(
        max(0.0, staticAreaProxy / temporalEnergy)) *
        temporalRawScale;
    return sqrt(max(0.0, lerp(
        staticRadialScale * staticRadialScale,
        temporalRadialScale * temporalRadialScale,
        authority)));
}

bool FoamChipAdmissionPointInsideField(float2 pointMetres)
{
    float localDistance = pointMetres.x - _FoamGlobalStart;
    if (localDistance < _FoamGridDescriptorLongitudinal.x - 0.0001 ||
        localDistance >
            _FoamGridDescriptorLongitudinal.x +
            _FoamGridDescriptorLongitudinal.z + 0.0001)
    {
        return false;
    }

    return pointMetres.y >= _FoamGridDescriptorExtent.x - 0.0001 &&
        pointMetres.y <= _FoamGridDescriptorExtent.y + 0.0001;
}

float2 FoamChipAdmissionPointToUV(float2 pointMetres)
{
    float localDistance = pointMetres.x - _FoamGlobalStart;
    if ((int)round(_FoamGridDescriptorContract.z) == 1)
    {
        float globalY =
            (pointMetres.y - _FoamGridDescriptorLateral.x) /
            max(0.0001, _FoamGridDescriptorSpacing.w);
        return saturate(float2(
            (localDistance - _FoamGridDescriptorLongitudinal.x) /
                max(0.0001, _FoamGridDescriptorLongitudinal.y),
            (globalY - _FoamGridDescriptorLateral.y + 0.5) /
                max(1.0, _FoamGridDescriptorLateral.z)));
    }

    return saturate(float2(
        localDistance / max(0.001, _FoamFieldLength),
        (pointMetres.y - _FoamGridDescriptorExtent.x) /
            max(
                0.001,
                _FoamGridDescriptorExtent.y -
                _FoamGridDescriptorExtent.x)));
}

void FoamChipAdmissionResolveBilinearCoordinates(
    float2 uv,
    out int2 p0,
    out int2 p1,
    out float2 blend)
{
    float2 pixel = clamp(
        saturate(uv) * (float2)_FoamDimensions - 0.5,
        float2(0.0, 0.0),
        max(float2(0.0, 0.0), (float2)_FoamDimensions - 1.0));
    p0 = int2(floor(pixel));
    p1 = min(p0 + int2(1, 1), _FoamDimensions - int2(1, 1));
    blend = frac(pixel);
}

float4 FoamChipAdmissionSamplePreviousState(float2 uv)
{
    int2 p0;
    int2 p1;
    float2 blend;
    FoamChipAdmissionResolveBilinearCoordinates(uv, p0, p1, blend);
    float4 a = _FoamChipAdmissionPreviousStateRead.Load(int3(p0, 0));
    float4 b = _FoamChipAdmissionPreviousStateRead.Load(
        int3(p1.x, p0.y, 0));
    float4 c = _FoamChipAdmissionPreviousStateRead.Load(
        int3(p0.x, p1.y, 0));
    float4 d = _FoamChipAdmissionPreviousStateRead.Load(int3(p1, 0));
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}

float4 FoamChipAdmissionSampleCurrentState(float2 uv)
{
    int2 p0;
    int2 p1;
    float2 blend;
    FoamChipAdmissionResolveBilinearCoordinates(uv, p0, p1, blend);
    float4 a = _FoamChipAdmissionCurrentStateRead.Load(int3(p0, 0));
    float4 b = _FoamChipAdmissionCurrentStateRead.Load(
        int3(p1.x, p0.y, 0));
    float4 c = _FoamChipAdmissionCurrentStateRead.Load(
        int3(p0.x, p1.y, 0));
    float4 d = _FoamChipAdmissionCurrentStateRead.Load(int3(p1, 0));
    return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
}

float FoamChipAdmissionSharpenCoverage(float presence)
{
    float s = saturate(_FoamChipAdmissionSharpness);
    float shaped = smoothstep(
        lerp(0.105, 0.185, s),
        lerp(0.365, 0.575, s),
        presence);
    float hard = smoothstep(0.18, 0.82, shaped);
    return saturate(pow(max(0.0, hard), lerp(1.65, 2.15, s)));
}

float FoamChipAdmissionPreChipMask(float2 pointMetres)
{
    if (!FoamChipAdmissionPointInsideField(pointMetres))
    {
        return 0.0;
    }

    float2 uv = FoamChipAdmissionPointToUV(pointMetres);
    float4 previousState = FoamChipAdmissionSamplePreviousState(uv);
    float4 currentState = FoamChipAdmissionSampleCurrentState(uv);
    float4 packedState = lerp(
        previousState,
        currentState,
        saturate(_FoamChipAdmissionInterpolation));
    float presence = saturate(packedState.x);
    if (presence <= 0.0001)
    {
        return 0.0;
    }

    float remainingLife = saturate(packedState.y / presence);
    float materialPattern = saturate(packedState.z / presence);
    float baseMask = _FoamChipAdmissionFinalVisibilityMode > 0.5
        ? smoothstep(0.02, 0.10, presence)
        : FoamChipAdmissionSharpenCoverage(presence);
    // Candidate Straddle is only active for Presence-Amplitude. Keep the
    // support evaluator on that exact footprint contract.
    baseMask = min(baseMask, presence);

    float seed = materialPattern * 43.731 + 11.17;
    float2 p = pointMetres;
    float broad = FoamChipAdmissionValueNoise(
        p * float2(0.62, 1.75) + seed);
    float diagonal = FoamChipAdmissionValueNoise(
        float2(
            p.x * 1.18 + p.y * 1.45,
            p.y * 2.80 - p.x * 0.34) + seed * 1.37 + 17.0);
    float mid = FoamChipAdmissionValueNoise(
        float2(
            p.x * 2.65 - p.y * 0.70,
            p.y * 4.60 + p.x * 0.52) + seed * 1.93 + 29.0);
    float pattern = saturate(
        materialPattern * 0.32 +
        broad * 0.27 +
        diagonal * 0.24 +
        mid * 0.17);
    float broadField = saturate(broad * 0.58 + diagonal * 0.42);
    float mediumPattern = saturate((mid - 0.5) * 1.35 + 0.5);
    float broadPattern = saturate((broadField - 0.5) * 2.0 + 0.5);

    // The fragment route derives these policies from screen-space derivatives.
    // The cache is deliberately camera-independent, so it evaluates the same
    // formulas with a fixed small world-space footprint instead of ddx/ddy.
    float2 footprint = max(
        _FoamChipAdmissionSupportFootprintMetres.xx,
        float2(0.0001, 0.0001));
    float broadSpatialFootprint = max(
        footprint.x * 0.62,
        footprint.y * 1.75);
    float diagonalSpatialFootprint = max(
        footprint.x * 1.18 + footprint.y * 1.45,
        footprint.x * 0.34 + footprint.y * 2.80);
    float broadFootprint = max(
        broadSpatialFootprint,
        diagonalSpatialFootprint);
    float midFootprint = max(
        footprint.x * 2.65 + footprint.y * 0.70,
        footprint.x * 0.52 + footprint.y * 4.60);
    float bandFootprint = max(
        footprint.x * 1.85 + footprint.y * 3.25,
        footprint.x * 0.48 + footprint.y * 6.20);
    float broadResolved = 1.0 - smoothstep(
        0.48,
        1.00,
        broadFootprint);
    float midResolved = 1.0 - smoothstep(
        0.38,
        0.82,
        midFootprint);
    float bandResolved = 1.0 - smoothstep(
        0.36,
        0.80,
        bandFootprint);
    float strandDetail = 1.0 - saturate(_FoamChipAdmissionStrandScale);
    float mediumAuthority = strandDetail * midResolved;
    float strandPattern = saturate(lerp(
        broadPattern,
        mediumPattern,
        mediumAuthority));
    float strandResolution = saturate(broadResolved);

    float damage = 1.0 - remainingLife;
    float slowA = sin(
        _FoamChipAdmissionEvolutionTime * 0.31 +
        seed * 0.43 + pattern * 5.1) * 0.5 + 0.5;
    float slowB = sin(
        _FoamChipAdmissionEvolutionTime * 0.57 +
        seed * 0.79 + p.x * 0.37 - p.y * 0.91) * 0.5 + 0.5;
    float morph = slowA * 0.55 + slowB * 0.45;
    float edgeExposure = 1.0 - smoothstep(0.38, 0.76, presence);
    float weakInterior = 1.0 - smoothstep(0.54, 0.88, presence);
    float erosionDrive = pattern + (morph - 0.5) * 0.16;
    erosionDrive += (1.0 - edgeExposure) * 0.18;
    erosionDrive += baseMask * 0.22;
    float sharpness = saturate(_FoamChipAdmissionSharpness);
    float edgeThreshold = lerp(0.18, 0.30, sharpness) +
        damage * lerp(0.20, 0.38, edgeExposure);
    float interiorThreshold = lerp(0.09, 0.19, sharpness) +
        damage * lerp(0.05, 0.16, weakInterior);
    float edgeKeep = smoothstep(
        edgeThreshold - 0.09,
        edgeThreshold + 0.12,
        erosionDrive);
    float interiorKeep = smoothstep(
        interiorThreshold - 0.08,
        interiorThreshold + 0.16,
        erosionDrive + (1.0 - weakInterior) * 0.15);
    float coherentKeep = lerp(interiorKeep, edgeKeep, edgeExposure);

    float2 bandCoordinate = float2(
        p.x * 1.85 + p.y * 3.25,
        p.y * 6.20 - p.x * 0.48) + seed * 2.19;
    float bandBreaker = FoamChipAdmissionValueNoise(bandCoordinate);
    float strandLineifiedKeep = coherentKeep;
    if (saturate(_FoamChipAdmissionStrandStrength) > 0.0001)
    {
        float broadBandFallback = saturate(
            broad * 0.58 + diagonal * 0.42);
        float resolvedStrandBand = lerp(
            broadBandFallback,
            bandBreaker,
            bandResolved);
        float strandBandKeep = smoothstep(
            0.20 + damage * 0.08,
            0.52 + damage * 0.12,
            resolvedStrandBand + pattern * 0.38 + baseMask * 0.24);
        float reach = saturate(_FoamChipAdmissionStrandReach);
        float attenuationFloor = lerp(0.90, 0.66, reach);
        float presenceGuardLow = lerp(0.54, 0.72, reach);
        float presenceGuardHigh = lerp(0.78, 0.96, reach);
        float rawStrandLineifiedKeep = coherentKeep * lerp(
            attenuationFloor +
                strandBandKeep * (1.0 - attenuationFloor),
            1.0,
            smoothstep(
                presenceGuardLow,
                presenceGuardHigh,
                presence));
        strandLineifiedKeep = lerp(
            coherentKeep,
            rawStrandLineifiedKeep,
            strandResolution);
    }
    else
    {
        strandPattern = 0.0;
        strandResolution = 0.0;
    }

    float compactCore = smoothstep(0.66, 0.91, presence) *
        smoothstep(
            0.22 + damage * 0.16,
            0.58 + damage * 0.12,
            pattern + morph * 0.10);
    float protectedCore = compactCore * lerp(0.72, 0.92, sharpness);
    float lifeGate = smoothstep(0.015, 0.070, remainingLife);
    float coherentSoft = max(baseMask * coherentKeep, protectedCore) *
        lifeGate;
    float strandSoft = max(
        baseMask * strandLineifiedKeep,
        protectedCore) * lifeGate;
    float hardVisible = smoothstep(0.22, 0.58, coherentSoft);
    float fringe = smoothstep(0.06, 0.34, coherentSoft) * 0.34;
    float hardenedShape = saturate(max(hardVisible, fringe));
    if (hardenedShape <= 0.0001 || coherentSoft <= 0.0001)
    {
        return 0.0;
    }

    float exactCore = step(0.999, coherentSoft);
    float density = saturate(_FoamChipAdmissionStrandDensity);
    float selectionLow = lerp(0.68, 0.42, density);
    float selectionHigh = selectionLow + 0.18;
    float patternAA = max(
        0.0015,
        _FoamChipAdmissionSupportFootprintMetres * 0.06);
    float selection = smoothstep(
        selectionLow - patternAA,
        selectionHigh + patternAA,
        strandPattern);
    float authority = saturate(
        _FoamChipAdmissionStrandStrength *
        strandResolution *
        selection);
    float maximumDepth = lerp(
        0.52,
        0.98,
        saturate(_FoamChipAdmissionStrandReach));
    float threshold = lerp(0.16, maximumDepth, authority);
    float visibilityAA = max(
        0.001,
        _FoamChipAdmissionSupportFootprintMetres * 0.04);
    float cut = smoothstep(
        threshold - visibilityAA,
        threshold + visibilityAA,
        strandSoft);
    float strandKeep = lerp(
        1.0,
        cut,
        smoothstep(0.001, 0.08, authority));
    strandKeep = max(strandKeep, exactCore);
    return saturate(hardenedShape * strandKeep);
}

[numthreads(64, 1, 1)]
void BuildFoamChipStraddleAdmission(uint3 dispatchId : SV_DispatchThreadID)
{
    uint recordIndex = dispatchId.x;
    if (recordIndex >= (uint)_FoamChipAdmissionRecordCount)
    {
        return;
    }

    int width = max(1, _FoamChipAdmissionDimensions.x);
    int2 localCell = int2(
        (int)(recordIndex % (uint)width),
        (int)(recordIndex / (uint)width));
    int2 cell = _FoamChipAdmissionOrigin + localCell;
    bool previouslyAdmitted = false;
    [branch]
    if (_FoamChipAdmissionHistoryValid != 0)
    {
        previouslyAdmitted =
            _FoamChipStraddleAdmissionWrite[localCell] > 0.5;
    }

    float2 cellFloat = (float2)cell;
    float activationHash = FoamChipAdmissionHash21(
        cellFloat + float2(53.27, 67.19));
    float lifecycleHash = FoamChipAdmissionHash21(
        cellFloat + float2(17.41, 59.83));
    float lifeScale;
    float stableVariationAuthority;
    FoamChipAdmissionLifecycle(
        _FoamChipAdmissionEvolutionTime,
        lifecycleHash,
        lifeScale,
        stableVariationAuthority);
    if (activationHash > saturate(_FoamChipAdmissionActivation) ||
        lifeScale <= 0.000001)
    {
        _FoamChipStraddleAdmissionWrite[localCell] = 0.0;
        return;
    }

    float centreHashX = FoamChipAdmissionHash21(
        cellFloat + float2(13.17, 41.73));
    float centreHashY = FoamChipAdmissionHash21(
        cellFloat + float2(71.31, 19.47));
    float angleHash = FoamChipAdmissionHash21(
        cellFloat + float2(37.91, 83.11));
    float radiusHash = FoamChipAdmissionHash21(
        cellFloat + float2(97.53, 23.69));
    float secondaryHash = FoamChipAdmissionHash21(
        cellFloat + float2(29.47, 91.13));
    float tertiaryHash = FoamChipAdmissionHash21(
        cellFloat + float2(81.37, 47.59));

    float spacing = max(0.10, _FoamChipAdmissionCandidateSpacing);
    float irregularity = saturate(_FoamChipAdmissionIrregularity);
    float2 fullJitter = (float2(centreHashX, centreHashY) - 0.5) * 0.78;
    float2 candidateCentre =
        (cellFloat + 0.5 + fullJitter * irregularity) * spacing;
    float lateralWave = FoamChipAdmissionSignedWave(
        _FoamChipAdmissionEvolutionTime,
        _FoamChipAdmissionLateralMotionSpeed,
        secondaryHash * 0.73 + tertiaryHash * 0.27);
    candidateCentre.y += spacing *
        clamp(_FoamChipAdmissionLateralMotionAmount, 0.0, 2.5) *
        lateralWave;
    candidateCentre.x += max(0.0, _FoamChipAdmissionFieldSpeed) *
        _FoamChipAdmissionEvolutionTime;

    float rotationWave = FoamChipAdmissionSignedWave(
        _FoamChipAdmissionEvolutionTime,
        _FoamChipAdmissionRotationSpeed,
        tertiaryHash * 0.61 + centreHashX * 0.39);
    float angle = angleHash * 6.28318530718 +
        clamp(_FoamChipAdmissionRotationAmountDegrees, 0.0, 180.0) *
        0.01745329252 * rotationWave;
    float2 contourAxis = float2(cos(angle), sin(angle));

    float radiusRatio = lerp(0.05, 0.65, saturate(_FoamChipAdmissionSize));
    float fullRadiusVariation = lerp(0.80, 1.40, radiusHash);
    float candidateSizeMultiplier = lerp(
        1.0,
        fullRadiusVariation,
        irregularity);
    float staticCandidateRadius = spacing *
        radiusRatio * candidateSizeMultiplier;
    float maximumStaticCandidateRadius =
        spacing * 0.65 * candidateSizeMultiplier;
    staticCandidateRadius = min(
        staticCandidateRadius * clamp(
            _FoamChipAdmissionMaximumViewScale,
            1.0,
            2.5),
        maximumStaticCandidateRadius);

    float sizePulseWave = FoamChipAdmissionSignedWave(
        _FoamChipAdmissionEvolutionTime,
        _FoamChipAdmissionSizePulseSpeed,
        radiusHash * 0.67 + centreHashY * 0.33);
    float candidateOuterRadius = staticCandidateRadius * lifeScale *
        (1.0 + clamp(
            _FoamChipAdmissionSizePulseAmount,
            0.0,
            0.45) * sizePulseWave * stableVariationAuthority);
    candidateOuterRadius = min(candidateOuterRadius, spacing * 1.34);
    if (candidateOuterRadius <= 0.000001)
    {
        _FoamChipStraddleAdmissionWrite[localCell] = 0.0;
        return;
    }

    float contourSignA = secondaryHash < 0.5 ? -1.0 : 1.0;
    float contourSignB = tertiaryHash < 0.5 ? -1.0 : 1.0;
    float contourSignC = centreHashY < 0.5 ? -1.0 : 1.0;
    float3 contourSetA = float3(
        contourSignA * lerp(
            0.30,
            0.52,
            abs(secondaryHash * 2.0 - 1.0)),
        contourSignB * lerp(
            0.19,
            0.36,
            abs(tertiaryHash * 2.0 - 1.0)),
        contourSignC * lerp(
            0.13,
            0.28,
            abs(centreHashY * 2.0 - 1.0)));
    float3 morphBasisU;
    float3 morphBasisV;
    FoamChipAdmissionMorphBasis(
        float3(
            secondaryHash * 2.0 - 1.0,
            tertiaryHash * 2.0 - 1.0,
            centreHashX * 2.0 - 1.0),
        float3(
            centreHashY * 2.0 - 1.0,
            radiusHash * 2.0 - 1.0,
            angleHash * 2.0 - 1.0),
        morphBasisU,
        morphBasisV);
    float2 morphTrajectory = FoamChipAdmissionMorphTrajectory(
        _FoamChipAdmissionEvolutionTime,
        _FoamChipAdmissionShapeChangeSpeed,
        _FoamChipAdmissionShapeTransitionTime,
        secondaryHash * 0.57 + centreHashY * 0.43);
    float effectiveShapeChange = saturate(
        _FoamChipAdmissionShapeChangeAmount) *
        stableVariationAuthority;

    float centreSupport = FoamChipAdmissionPreChipMask(candidateCentre);
    bool centreRejected = previouslyAdmitted
        ? centreSupport >= 0.46
        : centreSupport > 0.08;
    if (centreRejected)
    {
        // This is algebraically equivalent to the full test: a new candidate
        // cannot enter with an occupied centre, and hysteresis cannot retain a
        // candidate whose centre has become convincingly interior. Reject
        // before the eight expensive perimeter support evaluations.
        _FoamChipStraddleAdmissionWrite[localCell] = 0.0;
        return;
    }

    uint requiredInsideCount = previouslyAdmitted ? 1u : 2u;
    uint perimeterInsideCount = 0u;
    [loop]
    for (uint sampleIndex = 0u; sampleIndex < 8u; sampleIndex++)
    {
        float sampleAngle = ((float)sampleIndex + 0.5) *
            0.78539816339;
        float2 sampleDirection = float2(
            cos(sampleAngle),
            sin(sampleAngle));
        float radialScale = FoamChipAdmissionRadialScale(
            sampleDirection,
            contourAxis,
            irregularity,
            contourSetA,
            morphBasisU,
            morphBasisV,
            morphTrajectory,
            effectiveShapeChange);
        float2 samplePoint = candidateCentre +
            sampleDirection * candidateOuterRadius * radialScale;
        perimeterInsideCount +=
            FoamChipAdmissionPreChipMask(samplePoint) > 0.08
                ? 1u
                : 0u;
        if (perimeterInsideCount >= requiredInsideCount)
        {
            // A new candidate needs two independent perimeter contacts; a
            // previously admitted candidate needs one. Remaining samples
            // cannot change the binary result, so stop immediately.
            break;
        }
    }

    bool admitted = perimeterInsideCount >= requiredInsideCount;
    _FoamChipStraddleAdmissionWrite[localCell] = admitted ? 1.0 : 0.0;
}

#endif
