#ifndef PS3D_RIVER_WATER_MOTION_INCLUDED
#define PS3D_RIVER_WATER_MOTION_INCLUDED

#define PS3D_RIVER_TWO_PI 6.28318530718

struct RiverWaterMotionInputs
{
    float3 positionWS;
    float3 baseNormalWS;
    float3 tangentWS;
    float3 sideWS;
    float globalDistance;
    float lateralMetres;
    float visibleHalfWidth;
    float surfaceHalfWidth;
    float time;
    float freezeAmount;
};

struct RiverWaterMotionResult
{
    float3 displacementWS;
    float3 surfaceNormalWS;
    float bankMask;
    float macroHeight;
    float currentAccent;
    float liquidFactor;
    float disturbanceHeight;
    float3 disturbanceNormalWS;
};

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

float3 RiverWaterEvaluateMacroNormal(
    RiverWaterMotionInputs input,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float steepness,
    float turbulence,
    float seed)
{
    float epsilon = max(0.04, min(0.18, waveLength * 0.035));
    float hForward = RiverWaterEvaluateMacroHeight(
        input.globalDistance + epsilon, input.lateralMetres, input.time,
        flowSpeed, waveHeight, waveLength, steepness, turbulence, seed);
    float hBack = RiverWaterEvaluateMacroHeight(
        input.globalDistance - epsilon, input.lateralMetres, input.time,
        flowSpeed, waveHeight, waveLength, steepness, turbulence, seed);
    float hRight = RiverWaterEvaluateMacroHeight(
        input.globalDistance, input.lateralMetres + epsilon, input.time,
        flowSpeed, waveHeight, waveLength, steepness, turbulence, seed);
    float hLeft = RiverWaterEvaluateMacroHeight(
        input.globalDistance, input.lateralMetres - epsilon, input.time,
        flowSpeed, waveHeight, waveLength, steepness, turbulence, seed);
    float downstreamSlope = (hForward - hBack) / (2.0 * epsilon);
    float lateralSlope = (hRight - hLeft) / (2.0 * epsilon);
    return normalize(
        input.baseNormalWS -
        input.tangentWS * downstreamSlope -
        input.sideWS * lateralSlope);
}

float3 RiverWaterEvaluateDetailNormal(
    TEXTURE2D_PARAM(detailTexture, detailSampler),
    RiverWaterMotionInputs input,
    float detailScale,
    float detailStrength,
    float flowSpeed,
    float turbulence,
    float seed)
{
    float scale = max(0.05, detailScale);
    float downstreamPhase = input.time * flowSpeed / scale;
    float2 baseUV = float2(
        input.lateralMetres / scale,
        input.globalDistance / scale - downstreamPhase);
    float2 warpUV =
        baseUV * 0.37 +
        float2(frac(seed * 0.017), frac(seed * 0.029) + input.time * 0.021);
    float2 warpSample =
        SAMPLE_TEXTURE2D(detailTexture, detailSampler, warpUV).rg * 2.0 - 1.0;
    float2 finalUV = baseUV + warpSample * saturate(turbulence) * 0.22;
    float3 tangentNormal = UnpackNormalScale(
        SAMPLE_TEXTURE2D(detailTexture, detailSampler, finalUV),
        max(0.0, detailStrength));
    return normalize(
        input.sideWS * tangentNormal.x +
        input.tangentWS * tangentNormal.y +
        input.baseNormalWS * tangentNormal.z);
}

RiverWaterMotionResult RiverWaterEvaluateMotionVertex(
    RiverWaterMotionInputs input,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float waveSteepness,
    float turbulence,
    float shoreMotion,
    float shoreMotionWidth,
    float seed)
{
    RiverWaterMotionResult result;
    result.liquidFactor = 1.0 - saturate(input.freezeAmount);
    result.bankMask = RiverWaterResolveMotionBankMask(
        input.lateralMetres,
        input.visibleHalfWidth,
        input.surfaceHalfWidth,
        shoreMotion,
        shoreMotionWidth);
    float baseHeight = RiverWaterEvaluateMacroHeight(
        input.globalDistance, input.lateralMetres, input.time,
        flowSpeed, waveHeight, waveLength, waveSteepness, turbulence, seed);
    result.macroHeight = baseHeight * result.bankMask * result.liquidFactor;
    result.displacementWS = input.baseNormalWS * result.macroHeight;
    result.surfaceNormalWS = RiverWaterEvaluateMacroNormal(
        input, flowSpeed,
        waveHeight * result.bankMask * result.liquidFactor,
        waveLength, waveSteepness, turbulence, seed);
    result.disturbanceHeight = 0.0;
    result.disturbanceNormalWS = 0.0;
    result.currentAccent = 0.0;
    return result;
}

RiverWaterMotionResult RiverWaterEvaluateMotionFragment(
    TEXTURE2D_PARAM(detailTexture, detailSampler),
    RiverWaterMotionInputs input,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float waveSteepness,
    float detailStrength,
    float detailScale,
    float turbulence,
    float currentAccentStrength,
    float currentAccentScale,
    float shoreMotion,
    float shoreMotionWidth,
    float seed)
{
    RiverWaterMotionResult result = RiverWaterEvaluateMotionVertex(
        input, flowSpeed, waveHeight, waveLength, waveSteepness,
        turbulence, shoreMotion, shoreMotionWidth, seed);
    float3 detailNormal = RiverWaterEvaluateDetailNormal(
        TEXTURE2D_ARGS(detailTexture, detailSampler),
        input, detailScale,
        detailStrength * result.bankMask * result.liquidFactor,
        flowSpeed, turbulence, seed);
    result.surfaceNormalWS = normalize(lerp(
        result.surfaceNormalWS,
        detailNormal,
        saturate(detailStrength) * result.bankMask * result.liquidFactor));
    float accentScale = max(0.25, currentAccentScale);
    float accentPhase =
        input.globalDistance * PS3D_RIVER_TWO_PI / accentScale -
        input.time * flowSpeed * PS3D_RIVER_TWO_PI / accentScale +
        input.lateralMetres * 0.37 +
        frac(seed * 0.011) * PS3D_RIVER_TWO_PI;
    float brokenAccent =
        sin(accentPhase) * 0.65 +
        sin(accentPhase * 1.91 + result.macroHeight * 5.0) * 0.35;
    result.currentAccent =
        smoothstep(0.35, 0.9, brokenAccent * 0.5 + 0.5) *
        saturate(currentAccentStrength) *
        result.bankMask *
        result.liquidFactor;
    return result;
}

#endif
