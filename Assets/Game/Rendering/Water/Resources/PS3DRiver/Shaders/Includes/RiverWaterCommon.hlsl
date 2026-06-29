#ifndef PS3D_RIVER_WATER_COMMON_INCLUDED
#define PS3D_RIVER_WATER_COMMON_INCLUDED

#define PS3D_RIVER_TWO_PI 6.28318530718
#define PS3D_RIVER_SHORE_SEARCH_STEPS 24

struct RiverWaterSurfaceInputs
{
    float3 positionWS;
    float3 baseNormalWS;
    float localDistance;
    float globalDistance;
    float lateralMetres;
};

struct RiverWaterIntegrationInputs
{
    float3 surfaceNormalWS;
    float2 refractionOffset;
    float foamMask;
    float3 reflectionColour;
    float reflectionWeight;

    // Stage 5 owns these values. Stage 3 keeps them neutral.
    float disturbanceHeight;
    float3 disturbanceNormalWS;
};

RiverWaterIntegrationInputs RiverWaterCreateEmptyIntegration(
    float3 baseNormalWS)
{
    RiverWaterIntegrationInputs inputs;
    inputs.surfaceNormalWS = normalize(baseNormalWS);
    inputs.refractionOffset = 0.0;
    inputs.foamMask = 0.0;
    inputs.reflectionColour = 0.0;
    inputs.reflectionWeight = 0.0;
    inputs.disturbanceHeight = 0.0;
    inputs.disturbanceNormalWS = 0.0;
    return inputs;
}

// Shared Stage 3 motion primitives. These functions are intentionally kept in
// RiverWaterCommon so both the visible-water shader and compute consumers can
// evaluate the same macro wave and shore attenuation without maintaining a
// second approximation.
float RiverWaterHash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float RiverWaterValueNoise(float2 p)
{
    float2 cell = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = RiverWaterHash21(cell);
    float b = RiverWaterHash21(cell + float2(1.0, 0.0));
    float c = RiverWaterHash21(cell + float2(0.0, 1.0));
    float d = RiverWaterHash21(cell + 1.0);
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// Intermediate Stage 3 shore-wave profile.
//
// Profile Variation shapes one wave internally through a slope-continuous
// half-wave knot curve. Size Variation gives successive travelling wave
// identities stable deterministic overall sizes. Transition Length controls
// the world-space blend around the boundary between those wave identities.
// Both controls share the same left/right asymmetry contract and never reseed
// during runtime.

float RiverWaterHermiteZeroToLinear(float t)
{
    t = saturate(t);
    return t * t * (2.0 - t);
}

float RiverWaterHermiteLinearToZero(float t)
{
    t = saturate(t);
    return t * (1.0 + t - t * t);
}

// Keeps the original value through the middle of the unit interval while
// forcing zero derivative at both hard bounds. This removes longitudinal
// shoreline corners caused by max/saturate activation at the normal shore or
// at the outer hidden-water allowance.
float RiverWaterResolveZeroSlopeBoundedReach(
    float rawReach,
    float transitionLength,
    float wavelength)
{
    float reach = saturate(rawReach);
    float transitionFraction = clamp(
        max(0.0, transitionLength) / max(0.25, wavelength),
        0.0005,
        0.49);

    if (reach < transitionFraction)
    {
        float t = reach / transitionFraction;
        return transitionFraction * RiverWaterHermiteZeroToLinear(t);
    }

    if (reach > 1.0 - transitionFraction)
    {
        float t = (reach - (1.0 - transitionFraction)) /
            transitionFraction;
        return (1.0 - transitionFraction) +
            transitionFraction * RiverWaterHermiteLinearToZero(t);
    }

    return reach;
}

// Applies a zero-slope activation around shore-wave zero crossings while
// preserving the original signed height once it leaves the transition band.
// The transition is derived from the authored world-space Transition Length,
// so the visible shoreline can leave and rejoin the normal edge without a
// tangent discontinuity.
float RiverWaterResolveZeroSlopeShoreHeight(
    float signedHeight,
    float maximumAmplitude,
    float transitionLength,
    float wavelength)
{
    float amplitude = max(0.0001, maximumAmplitude);
    float transitionFraction = clamp(
        max(0.0, transitionLength) / max(0.25, wavelength),
        0.0005,
        0.49);
    float normalizedThreshold = max(
        0.001,
        sin(3.14159265359 * transitionFraction));
    float threshold = amplitude * normalizedThreshold;
    float magnitude = abs(signedHeight);

    if (magnitude >= threshold)
    {
        return signedHeight;
    }

    float t = magnitude / threshold;
    float smoothedMagnitude =
        threshold * RiverWaterHermiteZeroToLinear(t);
    return signedHeight < 0.0 ? -smoothedMagnitude : smoothedMagnitude;
}

float RiverWaterResolveShoreProfileKnot(
    float knotIndex,
    float seed,
    float salt)
{
    return RiverWaterHash21(float2(
        knotIndex + seed * 0.00173,
        salt + seed * 0.00091));
}

float RiverWaterResolveShoreSideValue(
    float sampleIndex,
    float sideSign,
    float seed,
    float sideAsymmetry,
    float salt)
{
    float sharedValue = RiverWaterResolveShoreProfileKnot(
        sampleIndex,
        seed,
        salt);
    float asymmetry = saturate(sideAsymmetry);
    if (asymmetry <= 0.0001)
    {
        return sharedValue;
    }

    float sideSalt = sideSign < 0.0 ? salt + 17.0 : salt + 31.0;
    float sideValue = RiverWaterResolveShoreProfileKnot(
        sampleIndex,
        seed,
        sideSalt);
    return lerp(sharedValue, sideValue, asymmetry);
}

float RiverWaterCatmullRomScalar(
    float p0,
    float p1,
    float p2,
    float p3,
    float t)
{
    float t2 = t * t;
    float t3 = t2 * t;
    return 0.5 * (
        2.0 * p1 +
        (-p0 + p2) * t +
        (2.0 * p0 - 5.0 * p1 + 4.0 * p2 - p3) * t2 +
        (-p0 + 3.0 * p1 - 3.0 * p2 + p3) * t3);
}

float RiverWaterBSplineScalar(
    float p0,
    float p1,
    float p2,
    float p3,
    float t)
{
    float t2 = t * t;
    float t3 = t2 * t;
    return (
        p0 * (1.0 - 3.0 * t + 3.0 * t2 - t3) +
        p1 * (4.0 - 6.0 * t2 + 3.0 * t3) +
        p2 * (1.0 + 3.0 * t + 3.0 * t2 - 3.0 * t3) +
        p3 * t3) / 6.0;
}

float RiverWaterResolveShoreProfileValue(
    float waveCoordinate,
    float sideSign,
    float seed,
    float sideAsymmetry,
    float variation,
    float transitionLength,
    float wavelength,
    float salt,
    float minimumValue,
    float maximumValue)
{
    if (variation <= 0.0001)
    {
        return 1.0;
    }

    // One sample every half wave gives each cycle a distinct start, middle,
    // and end while cubic interpolation preserves continuous slope.
    float knotCoordinate = waveCoordinate * 2.0;
    float knotIndex = floor(knotCoordinate);
    float knotT = frac(knotCoordinate);

    float p0 = RiverWaterResolveShoreSideValue(
        (knotIndex - 1.0) * 0.5,
        sideSign,
        seed,
        sideAsymmetry,
        salt);
    float p1 = RiverWaterResolveShoreSideValue(
        knotIndex * 0.5,
        sideSign,
        seed,
        sideAsymmetry,
        salt);
    float p2 = RiverWaterResolveShoreSideValue(
        (knotIndex + 1.0) * 0.5,
        sideSign,
        seed,
        sideAsymmetry,
        salt);
    float p3 = RiverWaterResolveShoreSideValue(
        (knotIndex + 2.0) * 0.5,
        sideSign,
        seed,
        sideAsymmetry,
        salt);

    float catmullProfile = RiverWaterCatmullRomScalar(
        p0,
        p1,
        p2,
        p3,
        knotT);
    float splineProfile = RiverWaterBSplineScalar(
        p0,
        p1,
        p2,
        p3,
        knotT);
    float smoothing = saturate(
        max(0.0, transitionLength) /
        max(0.25, wavelength * 0.5));
    float rawProfile = saturate(lerp(
        catmullProfile,
        splineProfile,
        smoothing));
    float shapedProfile = lerp(minimumValue, maximumValue, rawProfile);
    return lerp(1.0, shapedProfile, saturate(variation));
}

float RiverWaterResolveShoreWaveSizeValue(
    float waveCoordinate,
    float sideSign,
    float seed,
    float sideAsymmetry,
    float sizeVariation,
    float transitionLength,
    float wavelength,
    float salt,
    float minimumValue,
    float maximumValue)
{
    if (sizeVariation <= 0.0001)
    {
        return 1.0;
    }

    float waveIndex = floor(waveCoordinate);
    float waveT = frac(waveCoordinate);

    float previousValue = RiverWaterResolveShoreSideValue(
        waveIndex - 1.0,
        sideSign,
        seed,
        sideAsymmetry,
        salt);
    float currentValue = RiverWaterResolveShoreSideValue(
        waveIndex,
        sideSign,
        seed,
        sideAsymmetry,
        salt);
    float nextValue = RiverWaterResolveShoreSideValue(
        waveIndex + 1.0,
        sideSign,
        seed,
        sideAsymmetry,
        salt);

    // Transition Length is the complete world-space blend span centred on a
    // wave boundary. Both adjacent cycles evaluate the same half-transition
    // at that boundary, so position and slope remain continuous.
    float transitionHalfT = clamp(
        max(0.0, transitionLength) /
            max(0.25, wavelength) * 0.5,
        0.0005,
        0.49);

    float rawSize = currentValue;
    if (waveT < transitionHalfT)
    {
        float blend = smoothstep(
            -transitionHalfT,
            transitionHalfT,
            waveT);
        rawSize = lerp(previousValue, currentValue, blend);
    }
    else if (waveT > 1.0 - transitionHalfT)
    {
        float blend = smoothstep(
            -transitionHalfT,
            transitionHalfT,
            waveT - 1.0);
        rawSize = lerp(currentValue, nextValue, blend);
    }

    float shapedSize = lerp(minimumValue, maximumValue, rawSize);
    return lerp(1.0, shapedSize, saturate(sizeVariation));
}

void RiverWaterResolveShoreWaveProfiles(
    float globalDistance,
    float time,
    float flowSpeed,
    float shoreWaveLength,
    float sideSign,
    float transitionLength,
    float sizeVariation,
    float sideAsymmetry,
    float profileVariation,
    float seed,
    out float heightProfile,
    out float reachProfile)
{
    float wavelength = max(0.25, shoreWaveLength);
    float waveCoordinate =
        globalDistance / wavelength -
        time * flowSpeed / wavelength +
        frac(seed * 0.01371);

    float heightShape = RiverWaterResolveShoreProfileValue(
        waveCoordinate,
        sideSign,
        seed,
        sideAsymmetry,
        profileVariation,
        transitionLength,
        wavelength,
        11.0,
        0.45,
        1.55);
    float reachShape = RiverWaterResolveShoreProfileValue(
        waveCoordinate,
        sideSign,
        seed,
        sideAsymmetry,
        profileVariation,
        transitionLength,
        wavelength,
        53.0,
        0.35,
        1.15);

    float waveSize = RiverWaterResolveShoreWaveSizeValue(
        waveCoordinate,
        sideSign,
        seed,
        sideAsymmetry,
        sizeVariation,
        transitionLength,
        wavelength,
        101.0,
        0.60,
        1.40);

    heightProfile = max(0.0, heightShape * waveSize);
    reachProfile = max(0.0, reachShape * waveSize);
}

float RiverWaterResolveShoreBlend(
    float lateralMetres,
    float visibleHalfWidth,
    float shoreMotionWidth)
{
    float interiorDistance =
        max(0.0, visibleHalfWidth - abs(lateralMetres));
    return 1.0 - smoothstep(
        0.0,
        max(0.001, shoreMotionWidth),
        interiorDistance);
}

float RiverWaterResolveMotionBankMask(
    float lateralMetres,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float shoreMotion,
    float shoreMotionWidth,
    float shoreWaveReach)
{
    float lateral = abs(lateralMetres);
    float visible = max(0.001, visibleHalfWidth);
    float generatedSurface = max(visible + 0.001, surfaceHalfWidth);
    float effectiveSurface = lerp(
        visible,
        generatedSurface,
        saturate(shoreWaveReach));
    float retainedAtShore = saturate(shoreMotion);

    if (lateral <= visible)
    {
        float interiorDistance = visible - lateral;
        float interiorBlend = smoothstep(
            0.0,
            max(0.001, shoreMotionWidth),
            interiorDistance);
        return lerp(retainedAtShore, 1.0, interiorBlend);
    }

    if (lateral >= effectiveSurface || effectiveSurface <= visible + 0.0001)
    {
        return 0.0;
    }

    float hiddenWidth = max(0.001, effectiveSurface - visible);
    float hiddenRemaining = saturate(
        (effectiveSurface - lateral) / hiddenWidth);
    return retainedAtShore * smoothstep(0.0, 1.0, hiddenRemaining);
}

float RiverWaterEvaluateMacroHeight(
    float globalDistance,
    float lateralMetres,
    float time,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float steepness,
    float turbulence,
    float seed)
{
    float wavelength = max(0.25, waveLength);
    float phaseSpeed = flowSpeed * PS3D_RIVER_TWO_PI / wavelength;
    float seedPhase = frac(seed * 0.01371) * PS3D_RIVER_TWO_PI;
    float2 noiseCoordinate = float2(
        globalDistance / max(1.0, wavelength * 1.8),
        lateralMetres / max(1.0, wavelength * 0.8));
    float evolvingNoise = RiverWaterValueNoise(
        noiseCoordinate +
        float2(-time * flowSpeed / max(1.0, wavelength * 5.0),
               time * 0.035));
    float distortion =
        (evolvingNoise * 2.0 - 1.0) *
        saturate(turbulence) *
        1.65;
    float phase =
        globalDistance * PS3D_RIVER_TWO_PI / wavelength -
        time * phaseSpeed +
        seedPhase +
        distortion;
    float crossPhase =
        lateralMetres * PS3D_RIVER_TWO_PI /
        max(0.75, wavelength * 1.35);
    float primary = sin(phase + sin(crossPhase) * turbulence * 0.55);
    float secondary = sin(
        phase * 1.73 -
        crossPhase * 0.42 +
        seedPhase * 0.31 +
        time * phaseSpeed * 0.21);
    float combined = primary * 0.72 + secondary * 0.28;
    float crest = sign(combined) *
        pow(abs(combined), lerp(1.0, 0.58, saturate(steepness)));
    return crest * max(0.0, waveHeight);
}

float RiverWaterEvaluateBlendedMacroHeight(
    float globalDistance,
    float lateralMetres,
    float visibleHalfWidth,
    float time,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float steepness,
    float turbulence,
    float shoreMotionWidth,
    float shoreWaveHeightScale,
    float shoreWaveLengthScale,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float seed)
{
    float baseHeight = RiverWaterEvaluateMacroHeight(
        globalDistance,
        lateralMetres,
        time,
        flowSpeed,
        waveHeight,
        waveLength,
        steepness,
        turbulence,
        seed);

    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float sideSign = lateralMetres < 0.0 ? -1.0 : 1.0;
    float heightProfile;
    float unusedReachProfile;
    RiverWaterResolveShoreWaveProfiles(
        globalDistance,
        time,
        flowSpeed,
        shoreLength,
        sideSign,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        seed,
        heightProfile,
        unusedReachProfile);

    float shoreAmplitude =
        waveHeight * max(0.0, shoreWaveHeightScale) * heightProfile;
    float shoreHeight = RiverWaterEvaluateMacroHeight(
        globalDistance,
        lateralMetres,
        time,
        flowSpeed,
        shoreAmplitude,
        shoreLength,
        steepness,
        turbulence,
        seed);
    shoreHeight = RiverWaterResolveZeroSlopeShoreHeight(
        shoreHeight,
        shoreAmplitude,
        shoreWaveTransitionLength,
        shoreLength);
    float shoreBlend = RiverWaterResolveShoreBlend(
        lateralMetres,
        visibleHalfWidth,
        shoreMotionWidth);
    return lerp(baseHeight, shoreHeight, shoreBlend);
}

float RiverWaterEvaluateSurfaceHeight(
    float globalDistance,
    float lateralMetres,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float time,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float steepness,
    float turbulence,
    float shoreMotion,
    float shoreMotionWidth,
    float shoreWaveHeightScale,
    float shoreWaveLengthScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float liquidFactor,
    float seed,
    out float bankMask)
{
    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float sideSign = lateralMetres < 0.0 ? -1.0 : 1.0;
    float unusedHeightProfile;
    float reachProfile;
    RiverWaterResolveShoreWaveProfiles(
        globalDistance,
        time,
        flowSpeed,
        shoreLength,
        sideSign,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        seed,
        unusedHeightProfile,
        reachProfile);

    float resolvedReach = RiverWaterResolveZeroSlopeBoundedReach(
        saturate(shoreWaveReach) * reachProfile,
        shoreWaveTransitionLength,
        shoreLength);
    bankMask = RiverWaterResolveMotionBankMask(
        lateralMetres,
        visibleHalfWidth,
        surfaceHalfWidth,
        shoreMotion,
        shoreMotionWidth,
        resolvedReach);

    float blendedHeight = RiverWaterEvaluateBlendedMacroHeight(
        globalDistance,
        lateralMetres,
        visibleHalfWidth,
        time,
        flowSpeed,
        waveHeight,
        waveLength,
        steepness,
        turbulence,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        seed);
    return blendedHeight * bankMask * saturate(liquidFactor);
}

float RiverWaterResolveHiddenBankCoverOffset(
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float bankCover,
    float lateralAbsolute)
{
    float hiddenWidth = max(0.001, surfaceHalfWidth - visibleHalfWidth);
    float hiddenT = saturate(
        (lateralAbsolute - visibleHalfWidth) / hiddenWidth);
    return max(0.0, bankCover) * smoothstep(0.0, 1.0, hiddenT);
}

float RiverWaterEvaluateShoreSurfaceOffset(
    float globalDistance,
    float lateralMetres,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float time,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float steepness,
    float turbulence,
    float shoreMotion,
    float shoreMotionWidth,
    float shoreWaveHeightScale,
    float shoreWaveLengthScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float liquidFactor,
    float seed)
{
    float bankMask;
    return RiverWaterEvaluateSurfaceHeight(
        globalDistance,
        lateralMetres,
        visibleHalfWidth,
        surfaceHalfWidth,
        time,
        flowSpeed,
        waveHeight,
        waveLength,
        steepness,
        turbulence,
        shoreMotion,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        liquidFactor,
        seed,
        bankMask);
}

bool RiverWaterIsHiddenShoreSampleVisible(
    float globalDistance,
    float sideSign,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float hiddenT,
    float time,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float steepness,
    float turbulence,
    float shoreMotion,
    float shoreMotionWidth,
    float shoreWaveHeightScale,
    float shoreWaveLengthScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float bankCover,
    float liquidFactor,
    float seed)
{
    float lateralAbsolute = lerp(
        visibleHalfWidth,
        surfaceHalfWidth,
        saturate(hiddenT));
    float lateralMetres = sideSign * lateralAbsolute;
    float surfaceOffset = RiverWaterEvaluateShoreSurfaceOffset(
        globalDistance,
        lateralMetres,
        visibleHalfWidth,
        surfaceHalfWidth,
        time,
        flowSpeed,
        waveHeight,
        waveLength,
        steepness,
        turbulence,
        shoreMotion,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        liquidFactor,
        seed);
    float coverOffset = RiverWaterResolveHiddenBankCoverOffset(
        visibleHalfWidth,
        surfaceHalfWidth,
        bankCover,
        lateralAbsolute);
    return surfaceOffset > coverOffset;
}

// Resolves the current outermost visible water edge on one side of the river.
// The normal visible half-width is the minimum edge. Positive Stage 3 macro
// displacement may extend that edge through the hidden shoreline allowance by
// overtopping the corridor's mandatory bank-cover profile. The same macro-wave
// and shore attenuation functions used by the water shader are evaluated here.
float RiverWaterResolveCurrentVisibleShoreHalfWidth(
    float globalDistance,
    float sideSign,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float time,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float steepness,
    float turbulence,
    float shoreMotion,
    float shoreMotionWidth,
    float shoreWaveHeightScale,
    float shoreWaveLengthScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float bankCover,
    float liquidFactor,
    float seed)
{
    float visible = max(0.001, visibleHalfWidth);
    float surface = max(visible, surfaceHalfWidth);
    if (surface <= visible + 0.0001 ||
        waveHeight <= 0.0001 ||
        liquidFactor <= 0.0001 ||
        shoreMotion <= 0.0001)
    {
        return visible;
    }

    float farthestWetT = 0.0;

    // This search calls the complete shared shore-wave evaluator. Forcing all
    // samples to unroll can exceed D3D11 compiler limits as that evaluator grows.
    // Shader Model 5 compute supports this bounded runtime loop directly.
    [loop]
    for (int sampleIndex = 1;
         sampleIndex <= PS3D_RIVER_SHORE_SEARCH_STEPS;
         sampleIndex++)
    {
        float hiddenT = sampleIndex /
            (float)PS3D_RIVER_SHORE_SEARCH_STEPS;
        if (RiverWaterIsHiddenShoreSampleVisible(
                globalDistance,
                sideSign,
                visible,
                surface,
                hiddenT,
                time,
                flowSpeed,
                waveHeight,
                waveLength,
                steepness,
                turbulence,
                shoreMotion,
                shoreMotionWidth,
                shoreWaveHeightScale,
                shoreWaveLengthScale,
                shoreWaveReach,
                shoreWaveTransitionLength,
                shoreWaveSizeVariation,
                shoreWaveSideAsymmetry,
                shoreWaveProfileVariation,
                bankCover,
                liquidFactor,
                seed))
        {
            farthestWetT = hiddenT;
        }
    }

    if (farthestWetT <= 0.0)
    {
        return visible;
    }

    float lowerT = farthestWetT;
    float upperT = min(
        1.0,
        farthestWetT + 1.0 / (float)PS3D_RIVER_SHORE_SEARCH_STEPS);

    [unroll]
    for (int refinement = 0; refinement < 4; refinement++)
    {
        float middleT = (lowerT + upperT) * 0.5;
        if (RiverWaterIsHiddenShoreSampleVisible(
                globalDistance,
                sideSign,
                visible,
                surface,
                middleT,
                time,
                flowSpeed,
                waveHeight,
                waveLength,
                steepness,
                turbulence,
                shoreMotion,
                shoreMotionWidth,
                shoreWaveHeightScale,
                shoreWaveLengthScale,
                shoreWaveReach,
                shoreWaveTransitionLength,
                shoreWaveSizeVariation,
                shoreWaveSideAsymmetry,
                shoreWaveProfileVariation,
                bankCover,
                liquidFactor,
                seed))
        {
            lowerT = middleT;
        }
        else
        {
            upperT = middleT;
        }
    }

    return lerp(visible, surface, saturate(lowerT));
}

#endif
