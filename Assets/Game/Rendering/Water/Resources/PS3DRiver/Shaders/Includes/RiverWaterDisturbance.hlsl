#ifndef PS3D_RIVER_WATER_DISTURBANCE_INCLUDED
#define PS3D_RIVER_WATER_DISTURBANCE_INCLUDED

#define PS3D_DISTURBANCE_TWO_PI 6.28318530718

struct RiverWaterDisturbanceResult
{
    float height;
    float velocity;
    float downstreamGradient;
    float lateralGradient;
    float intensity;
    float bankMask;
    float2 fieldUV;
};

struct RiverWaterWakeResult
{
    float energy;
    float downstreamGradient;
    float lateralGradient;
    float intensity;
    float2 fieldUV;
};

float RiverWaterResolveDisturbanceBankMask(
    float lateralMetres,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float shoreInteraction)
{
    float lateral = abs(lateralMetres);
    float visible = max(0.001, visibleHalfWidth);
    float surface = max(visible + 0.001, surfaceHalfWidth);
    float retainedAtShore = saturate(shoreInteraction);

    if (lateral <= visible)
    {
        float interiorBand = max(0.12, visible * 0.20);
        float interiorDistance = visible - lateral;
        return lerp(
            retainedAtShore,
            1.0,
            smoothstep(0.0, interiorBand, interiorDistance));
    }

    float hiddenWidth = max(0.001, surface - visible);
    float hiddenRemaining = saturate((surface - lateral) / hiddenWidth);
    return retainedAtShore * smoothstep(0.0, 1.0, hiddenRemaining);
}

float2 RiverWaterResolveDisturbanceUV(
    float globalDistance,
    float lateralMetres,
    float surfaceHalfWidth,
    float globalStart,
    float fieldLength)
{
    float signedAcross =
        lateralMetres / max(0.001, surfaceHalfWidth);
    return float2(
        saturate((globalDistance - globalStart) / fieldLength),
        saturate(signedAcross * 0.5 + 0.5));
}

void RiverWaterDecodeStaticDynamics(
    float packedValue,
    out float phase,
    out float waveResponse)
{
    float packed = round(max(0.0, packedValue));
    float phaseCode = fmod(packed, 16.0);
    float waveResponseCode = floor(packed / 16.0);
    phase = ((phaseCode + 0.5) / 16.0) *
            PS3D_DISTURBANCE_TWO_PI;
    waveResponse = saturate(waveResponseCode / 31.0) * 2.0;
}

RiverWaterDisturbanceResult RiverWaterEvaluateDisturbance(
    TEXTURE2D_PARAM(previousField, previousSampler),
    TEXTURE2D_PARAM(currentField, currentSampler),
    TEXTURE2D_PARAM(staticPressureField, staticPressureSampler),
    float enabled,
    float globalDistance,
    float lateralMetres,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float globalStart,
    float fieldLength,
    float interpolation,
    float geometryStrength,
    float shoreInteraction,
    float maximumHeight,
    float staticMaximumHeight,
    float freezeAmount,
    float motionTime,
    float macroHeight,
    float motionWaveHeight,
    float motionFlowSpeed,
    float motionWaveLength,
    float motionWaveSteepness,
    float motionTurbulence,
    float motionSeed)
{
    RiverWaterDisturbanceResult result;
    result.height = 0.0;
    result.velocity = 0.0;
    result.downstreamGradient = 0.0;
    result.lateralGradient = 0.0;
    result.intensity = 0.0;
    result.bankMask = 0.0;
    result.fieldUV = 0.0;

    if (enabled < 0.5 || fieldLength <= 0.0001)
    {
        return result;
    }

    float2 uv = RiverWaterResolveDisturbanceUV(
        globalDistance,
        lateralMetres,
        surfaceHalfWidth,
        globalStart,
        fieldLength);
    float4 previousState = SAMPLE_TEXTURE2D_LOD(
        previousField,
        previousSampler,
        uv,
        0.0);
    float4 currentState = SAMPLE_TEXTURE2D_LOD(
        currentField,
        currentSampler,
        uv,
        0.0);
    float4 rippleState = lerp(
        previousState,
        currentState,
        saturate(interpolation));
    float4 staticPressure = SAMPLE_TEXTURE2D_LOD(
        staticPressureField,
        staticPressureSampler,
        uv,
        0.0);

    float liquidFactor = 1.0 - saturate(freezeAmount);
    float bankMask = RiverWaterResolveDisturbanceBankMask(
        lateralMetres,
        visibleHalfWidth,
        surfaceHalfWidth,
        shoreInteraction);

    float phase;
    float waveResponse;
    RiverWaterDecodeStaticDynamics(
        staticPressure.a,
        phase,
        waveResponse);

    float staticBaseHeight = clamp(
        staticPressure.r,
        0.0,
        staticMaximumHeight);
    float staticModulation = 1.0;
    float staticLateralModulationGradient = 0.0;

    // Static pressure remains geometrically cached. Only the height modulation
    // is animated, and only while the analytical Stage 3 water level is
    // changing at this point. This avoids constant artificial breathing.
    [branch]
    if (staticBaseHeight > 0.0001 && motionWaveHeight > 0.0001)
    {
        const float activitySampleDelta = 0.075;
        float previousMacroHeight = RiverWaterEvaluateMacroHeight(
            globalDistance,
            lateralMetres,
            motionTime - activitySampleDelta,
            motionFlowSpeed,
            motionWaveHeight,
            motionWaveLength,
            motionWaveSteepness,
            motionTurbulence,
            motionSeed) * bankMask * liquidFactor;
        float levelChange = abs(macroHeight - previousMacroHeight) /
            max(0.001, motionWaveHeight);
        float waveActivity = saturate(levelChange * 2.35);
        float modulationAmplitude =
            0.26 * saturate(waveResponse * 0.5);
        float ridgeScale = max(0.55, motionWaveLength * 0.55);
        float flowRate = max(0.20, abs(motionFlowSpeed));
        float frequencyA = PS3D_DISTURBANCE_TWO_PI / ridgeScale;
        float frequencyB = PS3D_DISTURBANCE_TWO_PI /
            max(0.35, ridgeScale * 0.58);
        float phaseA =
            lateralMetres * frequencyA +
            motionTime * flowRate * 0.58 +
            phase;
        float phaseB =
            lateralMetres * frequencyB -
            motionTime * flowRate * 0.33 +
            phase * 1.71;
        float varyingSignal =
            sin(phaseA) * 0.67 +
            sin(phaseB) * 0.33;
        staticModulation = clamp(
            1.0 +
            waveActivity *
            modulationAmplitude *
            varyingSignal,
            0.84,
            1.16);
        staticLateralModulationGradient =
            waveActivity *
            modulationAmplitude *
            (cos(phaseA) * frequencyA * 0.67 +
             cos(phaseB) * frequencyB * 0.33);
    }

    float rippleHeight = clamp(
        rippleState.r,
        -maximumHeight,
        maximumHeight) * max(0.0, geometryStrength);
    float staticHeight = clamp(
        staticBaseHeight * staticModulation,
        0.0,
        staticMaximumHeight);
    float2 combinedGradient =
        rippleState.ba * max(0.0, geometryStrength) +
        staticPressure.gb * staticModulation;
    combinedGradient.y +=
        staticBaseHeight * staticLateralModulationGradient;

    result.height =
        (rippleHeight + staticHeight) *
        bankMask *
        liquidFactor;
    result.velocity = rippleState.g * bankMask * liquidFactor;
    result.downstreamGradient =
        combinedGradient.x * bankMask * liquidFactor;
    result.lateralGradient =
        combinedGradient.y * bankMask * liquidFactor;
    result.intensity = saturate(
        abs(rippleHeight) / max(0.001, maximumHeight) * 0.34 +
        staticHeight / max(0.001, staticMaximumHeight) * 0.42 +
        abs(rippleState.g) * 0.08 +
        length(combinedGradient) * 0.26);
    result.bankMask = bankMask;
    result.fieldUV = uv;
    return result;
}

RiverWaterWakeResult RiverWaterEvaluateWake(
    TEXTURE2D_PARAM(previousWake, previousWakeSampler),
    TEXTURE2D_PARAM(currentWake, currentWakeSampler),
    float enabled,
    float globalDistance,
    float lateralMetres,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float globalStart,
    float fieldLength,
    float interpolation,
    float shoreInteraction,
    float freezeAmount)
{
    RiverWaterWakeResult result;
    result.energy = 0.0;
    result.downstreamGradient = 0.0;
    result.lateralGradient = 0.0;
    result.intensity = 0.0;
    result.fieldUV = 0.0;

    if (enabled < 0.5 || fieldLength <= 0.0001)
    {
        return result;
    }

    float2 uv = RiverWaterResolveDisturbanceUV(
        globalDistance,
        lateralMetres,
        surfaceHalfWidth,
        globalStart,
        fieldLength);
    float4 previousState = SAMPLE_TEXTURE2D_LOD(
        previousWake,
        previousWakeSampler,
        uv,
        0.0);
    float4 currentState = SAMPLE_TEXTURE2D_LOD(
        currentWake,
        currentWakeSampler,
        uv,
        0.0);
    float4 wakeState = lerp(
        previousState,
        currentState,
        saturate(interpolation));
    float bankMask = RiverWaterResolveDisturbanceBankMask(
        lateralMetres,
        visibleHalfWidth,
        surfaceHalfWidth,
        shoreInteraction);
    float liquidFactor = 1.0 - saturate(freezeAmount);
    float mask = bankMask * liquidFactor;

    result.energy = max(0.0, wakeState.r) * mask;
    result.downstreamGradient = wakeState.b * mask;
    result.lateralGradient = wakeState.a * mask;
    result.intensity = saturate(
        result.energy * 0.32 +
        length(float2(
            result.downstreamGradient,
            result.lateralGradient)) * 0.42);
    result.fieldUV = uv;
    return result;
}

float3 RiverWaterApplyDisturbanceNormal(
    float3 baseNormalWS,
    float3 tangentWS,
    float3 sideWS,
    float downstreamGradient,
    float lateralGradient,
    float normalStrength)
{
    return normalize(
        baseNormalWS -
        tangentWS * downstreamGradient * max(0.0, normalStrength) -
        sideWS * lateralGradient * max(0.0, normalStrength));
}

#endif
