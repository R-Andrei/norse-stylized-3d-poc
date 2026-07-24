#ifndef PS3D_RIVER_WATER_MOTION_INCLUDED
#define PS3D_RIVER_WATER_MOTION_INCLUDED

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
    float currentShoreHalfWidth;
    float currentAccent;
    float liquidFactor;
    float disturbanceHeight;
    float3 disturbanceNormalWS;
};


float3 RiverWaterEvaluateSurfaceNormal(
    RiverWaterMotionInputs input,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float steepness,
    float turbulence,
    float shoreMotion,
    float shoreMotionWidth,
    float shoreWaveHeightScale,
    float shoreWaveLengthScale,
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float shoreWaveProfileEvolutionStrength,
    float shoreWaveProfileEvolutionDuration,
    float2 shoreProfileEvolution,
    float seed)
{
    float epsilon = max(0.04, min(0.18, waveLength * 0.035));
    float liquidFactor = 1.0 - saturate(input.freezeAmount);
    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float shoreGap =
        waveLength * max(0.0, shoreWaveSpacingScale);
    float sideSign = input.lateralMetres < 0.0 ? -1.0 : 1.0;
    float2 forwardEvolution = shoreProfileEvolution;
    float2 backEvolution = shoreProfileEvolution;
    if (shoreWaveProfileEvolutionStrength > 0.0001)
    {
        forwardEvolution = RiverWaterResolveShoreProfileEvolution(
            input.globalDistance + epsilon,
            input.time,
            flowSpeed,
            shoreLength,
            shoreGap,
            sideSign,
            shoreWaveTransitionLength,
            shoreWaveSideAsymmetry,
            shoreWaveProfileEvolutionStrength,
            shoreWaveProfileEvolutionDuration,
            seed);
        backEvolution = RiverWaterResolveShoreProfileEvolution(
            input.globalDistance - epsilon,
            input.time,
            flowSpeed,
            shoreLength,
            shoreGap,
            sideSign,
            shoreWaveTransitionLength,
            shoreWaveSideAsymmetry,
            shoreWaveProfileEvolutionStrength,
            shoreWaveProfileEvolutionDuration,
            seed);
    }
    float sampleBankMask;
    float hForward = RiverWaterEvaluateSurfaceHeightWithEvolution(
        input.globalDistance + epsilon,
        input.lateralMetres,
        input.visibleHalfWidth,
        input.surfaceHalfWidth,
        input.time,
        flowSpeed,
        waveHeight,
        waveLength,
        steepness,
        turbulence,
        shoreMotion,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        forwardEvolution,
        liquidFactor,
        seed,
        sampleBankMask);
    float hBack = RiverWaterEvaluateSurfaceHeightWithEvolution(
        input.globalDistance - epsilon,
        input.lateralMetres,
        input.visibleHalfWidth,
        input.surfaceHalfWidth,
        input.time,
        flowSpeed,
        waveHeight,
        waveLength,
        steepness,
        turbulence,
        shoreMotion,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        backEvolution,
        liquidFactor,
        seed,
        sampleBankMask);
    float hRight = RiverWaterEvaluateSurfaceHeightWithEvolution(
        input.globalDistance,
        input.lateralMetres + epsilon,
        input.visibleHalfWidth,
        input.surfaceHalfWidth,
        input.time,
        flowSpeed,
        waveHeight,
        waveLength,
        steepness,
        turbulence,
        shoreMotion,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
        liquidFactor,
        seed,
        sampleBankMask);
    float hLeft = RiverWaterEvaluateSurfaceHeightWithEvolution(
        input.globalDistance,
        input.lateralMetres - epsilon,
        input.visibleHalfWidth,
        input.surfaceHalfWidth,
        input.time,
        flowSpeed,
        waveHeight,
        waveLength,
        steepness,
        turbulence,
        shoreMotion,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
        liquidFactor,
        seed,
        sampleBankMask);
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
    float shoreWaveHeightScale,
    float shoreWaveLengthScale,
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float shoreWaveProfileEvolutionStrength,
    float shoreWaveProfileEvolutionDuration,
    float shorelineBankCover,
    float seed)
{
    RiverWaterMotionResult result;
    result.liquidFactor = 1.0 - saturate(input.freezeAmount);
    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float shoreGap =
        waveLength * max(0.0, shoreWaveSpacingScale);
    float sideSign = input.lateralMetres < 0.0 ? -1.0 : 1.0;
    float2 shoreProfileEvolution =
        RiverWaterResolveShoreProfileEvolution(
            input.globalDistance,
            input.time,
            flowSpeed,
            shoreLength,
            shoreGap,
            sideSign,
            shoreWaveTransitionLength,
            shoreWaveSideAsymmetry,
            shoreWaveProfileEvolutionStrength,
            shoreWaveProfileEvolutionDuration,
            seed);
    result.macroHeight =
        RiverWaterEvaluateSurfaceHeightAndShorelineWithEvolution(
            input.globalDistance,
            input.lateralMetres,
            input.visibleHalfWidth,
            input.surfaceHalfWidth,
            input.time,
            flowSpeed,
            waveHeight,
            waveLength,
            waveSteepness,
            turbulence,
            shoreMotion,
            shoreMotionWidth,
            shoreWaveHeightScale,
            shoreWaveLengthScale,
            shoreWaveSpacingScale,
            shoreWaveReach,
            shoreWaveTransitionLength,
            shoreWaveSizeVariation,
            shoreWaveSideAsymmetry,
            shoreWaveProfileVariation,
            shoreProfileEvolution,
            shorelineBankCover,
            result.liquidFactor,
            seed,
            result.bankMask,
            result.currentShoreHalfWidth);
    result.displacementWS = input.baseNormalWS * result.macroHeight;
    result.surfaceNormalWS = RiverWaterEvaluateSurfaceNormal(
        input,
        flowSpeed,
        waveHeight,
        waveLength,
        waveSteepness,
        turbulence,
        shoreMotion,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreWaveProfileEvolutionStrength,
        shoreWaveProfileEvolutionDuration,
        shoreProfileEvolution,
        seed);
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
    float shoreWaveHeightScale,
    float shoreWaveLengthScale,
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float shoreWaveProfileEvolutionStrength,
    float shoreWaveProfileEvolutionDuration,
    float shorelineBankCover,
    float seed)
{
    RiverWaterMotionResult result = RiverWaterEvaluateMotionVertex(
        input,
        flowSpeed,
        waveHeight,
        waveLength,
        waveSteepness,
        turbulence,
        shoreMotion,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreWaveProfileEvolutionStrength,
        shoreWaveProfileEvolutionDuration,
        shorelineBankCover,
        seed);
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
