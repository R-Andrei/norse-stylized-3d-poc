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

float RiverWaterResolveMotionBankMask(
    float lateralMetres,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float shoreMotion,
    float shoreMotionWidth)
{
    float lateral = abs(lateralMetres);
    float visible = max(0.001, visibleHalfWidth);
    float surface = max(visible + 0.001, surfaceHalfWidth);
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

    float hiddenWidth = max(0.001, surface - visible);
    float hiddenRemaining = saturate((surface - lateral) / hiddenWidth);
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
    float liquidFactor,
    float seed)
{
    float macroHeight = RiverWaterEvaluateMacroHeight(
        globalDistance,
        lateralMetres,
        time,
        flowSpeed,
        waveHeight,
        waveLength,
        steepness,
        turbulence,
        seed);
    float bankMask = RiverWaterResolveMotionBankMask(
        lateralMetres,
        visibleHalfWidth,
        surfaceHalfWidth,
        shoreMotion,
        shoreMotionWidth);
    return macroHeight * bankMask * saturate(liquidFactor);
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

    [unroll]
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
