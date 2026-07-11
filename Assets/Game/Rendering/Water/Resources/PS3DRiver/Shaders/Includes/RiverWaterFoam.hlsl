#ifndef PS3D_RIVER_WATER_FOAM_INCLUDED
#define PS3D_RIVER_WATER_FOAM_INCLUDED


float RiverWaterFoamHash21(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float RiverWaterFoamValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);

    float a = RiverWaterFoamHash21(i + float2(0.0, 0.0));
    float b = RiverWaterFoamHash21(i + float2(1.0, 0.0));
    float c = RiverWaterFoamHash21(i + float2(0.0, 1.0));
    float d = RiverWaterFoamHash21(i + float2(1.0, 1.0));

    return lerp(
        lerp(a, b, u.x),
        lerp(c, d, u.x),
        u.y);
}

float RiverWaterFoamSharpenCoverage(
    float presence,
    float sharpness)
{
    float s = saturate(sharpness);
    float low = lerp(0.105, 0.185, s);
    float high = lerp(0.365, 0.575, s);
    float shaped = smoothstep(low, high, presence);

    // The visual contract is now deliberately closer to ink/paint coverage
    // than translucent smoke: the surviving body should stay readable and
    // foam-coloured. Softness belongs mostly to a narrow edge fringe, not the
    // whole patch.
    float hard = smoothstep(0.18, 0.82, shaped);
    hard = pow(max(0.0, hard), lerp(1.65, 2.15, s));
    return saturate(hard);
}

float RiverWaterFoamStablePattern(
    float materialPattern,
    float storedGlobalDistance,
    float lateralMetres)
{
    float seed = materialPattern * 43.731 + 11.17;
    float2 p = float2(storedGlobalDistance, lateralMetres);

    // Use several differently-oriented layers so the stored ribbon footprint
    // is not simply displayed as long parallel strokes. These coordinates are
    // storage-space metres, so the breakup rides with the material instead of
    // swimming in world space.
    float broad = RiverWaterFoamValueNoise(
        p * float2(0.62, 1.75) + seed);
    float diagonal = RiverWaterFoamValueNoise(
        float2(
            p.x * 1.18 + p.y * 1.45,
            p.y * 2.80 - p.x * 0.34) + seed * 1.37 + 17.0);
    float mid = RiverWaterFoamValueNoise(
        float2(
            p.x * 2.65 - p.y * 0.70,
            p.y * 4.60 + p.x * 0.52) + seed * 1.93 + 29.0);
    float fine = RiverWaterFoamValueNoise(
        p * float2(5.80, 7.40) + seed * 2.71 + 41.0);

    return saturate(
        materialPattern * 0.32 +
        broad * 0.24 +
        diagonal * 0.22 +
        mid * 0.16 +
        fine * 0.06);
}

float RiverWaterFoamPatternedMask(
    float baseMask,
    float presence,
    float remainingLife,
    float materialPattern,
    float storedGlobalDistance,
    float lateralMetres,
    float sharpness)
{
    float s = saturate(sharpness);
    float life = saturate(remainingLife);
    float damage = 1.0 - life;

    float seed = materialPattern * 43.731 + 11.17;
    float pattern = RiverWaterFoamStablePattern(
        materialPattern,
        storedGlobalDistance,
        lateralMetres);

    float2 p = float2(storedGlobalDistance, lateralMetres);
    float slowA = sin(_Time.y * 0.31 + seed * 0.43 + pattern * 5.1) * 0.5 + 0.5;
    float slowB = sin(_Time.y * 0.57 + seed * 0.79 + p.x * 0.37 - p.y * 0.91) * 0.5 + 0.5;
    float morph = slowA * 0.55 + slowB * 0.45;

    float edgeExposure = 1.0 - smoothstep(0.38, 0.76, presence);
    float weakInterior = 1.0 - smoothstep(0.54, 0.88, presence);

    // Remaining Life is not opacity. It raises the erosion threshold so older
    // material loses weak edge/fringe pieces first. The fragments that survive
    // still render as opaque foam rather than fading into blue/teal water.
    float erosionDrive = pattern + (morph - 0.5) * 0.16;
    erosionDrive += (1.0 - edgeExposure) * 0.18;
    erosionDrive += baseMask * 0.22;

    float edgeThreshold = lerp(0.18, 0.30, s) + damage * lerp(0.20, 0.38, edgeExposure);
    float interiorThreshold = lerp(0.09, 0.19, s) + damage * lerp(0.05, 0.16, weakInterior);

    float edgeKeep = smoothstep(
        edgeThreshold - 0.09,
        edgeThreshold + 0.12,
        erosionDrive);
    float interiorKeep = smoothstep(
        interiorThreshold - 0.08,
        interiorThreshold + 0.16,
        erosionDrive + (1.0 - weakInterior) * 0.15);

    float keep = lerp(interiorKeep, edgeKeep, edgeExposure);

    // Extra band breaker: the manual/progressive source can be born as long
    // visible ribbons. This only removes parts inside the stored footprint; it
    // never grows material laterally or downstream.
    float bandBreaker = RiverWaterFoamValueNoise(
        float2(
            p.x * 1.85 + p.y * 3.25,
            p.y * 6.20 - p.x * 0.48) + seed * 2.19 + morph * 0.35);
    float bandKeep = smoothstep(
        0.20 + damage * 0.08,
        0.52 + damage * 0.12,
        bandBreaker + pattern * 0.38 + baseMask * 0.24);
    keep *= lerp(0.74 + bandKeep * 0.26, 1.0, smoothstep(0.72, 0.94, presence));

    float visible = baseMask * keep;

    // Preserve only compact, pattern-supported cores. The previous patch kept
    // the whole high-presence body, which protected the line-shaped source too
    // much and made it move like a static stamp.
    float compactCore = smoothstep(0.66, 0.91, presence) *
        smoothstep(0.22 + damage * 0.16, 0.58 + damage * 0.12, pattern + morph * 0.10);
    visible = max(visible, compactCore * lerp(0.72, 0.92, s));

    // Near-zero Remaining Life may disappear, but ordinary aging does not
    // globally fade the whole patch. It erodes the mask through the thresholds
    // above.
    float lifeGate = smoothstep(0.015, 0.070, life);
    visible *= lifeGate;

    // Make the surviving body much less blurry. Keep a very narrow soft edge,
    // but drive most surviving fragments toward solid coverage so water colour
    // does not leak through as a false teal aging signal.
    float hardVisible = smoothstep(0.22, 0.58, visible);
    float fringe = smoothstep(0.06, 0.34, visible) * 0.34;
    return saturate(max(hardVisible, fringe));
}

float RiverWaterFoamEvaluateShaderLocalDetailProbe(
    float baseShape,
    float presence,
    float remainingLife,
    float materialPattern,
    float2 detailUV,
    float globalDistance,
    float lateralMetres,
    float renderAdvectionSeconds,
    float sharpness,
    float surfaceEnergy)
{
    baseShape = saturate(baseShape);
    if (baseShape <= 0.0001)
    {
        return 0.0;
    }

    float s = saturate(sharpness);
    float life = saturate(remainingLife);
    float damage = 1.0 - life;
    float seed = materialPattern * 61.37 +
        RiverWaterFoamHash21(detailUV * float2(193.0, 257.0)) * 19.0 +
        7.13;

    // Use stable river metres, not foam-grid cells or residual material UVs.
    // This keeps the probe at the rendered-pixel/detail layer and prevents
    // Layer E diagnostics from inheriting material-cell phase snap. Future
    // final streak/detail motion should use its own smooth shader motion, not
    // Layer C's residual transport coordinate.
    float2 p = float2(globalDistance, lateralMetres);
    float slowTime = _Time.y * 0.055;

    float broad = RiverWaterFoamValueNoise(
        float2(
            p.x * 2.20 + p.y * 0.85,
            p.y * 4.40 - p.x * 0.31) + seed + slowTime);
    float mid = RiverWaterFoamValueNoise(
        float2(
            p.x * 5.90 - p.y * 1.35,
            p.y * 8.70 + p.x * 0.74) + seed * 1.71 - slowTime * 1.37);
    float fine = RiverWaterFoamValueNoise(
        float2(
            p.x * 13.20 + p.y * 3.10,
            p.y * 16.60 - p.x * 1.80) + seed * 2.47 + slowTime * 2.10);
    float grain = RiverWaterFoamValueNoise(
        float2(
            p.x * 24.00 - p.y * 5.80,
            p.y * 28.00 + p.x * 3.60) + seed * 3.63 - slowTime * 2.80);

    float localField = saturate(
        broad * 0.16 +
        mid * 0.29 +
        fine * 0.33 +
        grain * 0.22);

    // Limit the diagnostic to edges and weak/old fringe material. Broad, high
    // coverage interiors should remain readable so this probe tests micro detail
    // instead of macro shape ownership.
    float edgeBand = smoothstep(0.035, 0.42, baseShape) *
        (1.0 - smoothstep(0.62, 0.94, baseShape));
    float weakCoverage = 1.0 - smoothstep(0.52, 0.88, baseShape);
    float detailInfluence = saturate(
        edgeBand * (0.88 + surfaceEnergy * 0.22) +
        weakCoverage * damage * 0.20);

    float threshold = lerp(0.24, 0.38, s) +
        damage * 0.12 -
        surfaceEnergy * 0.035;
    float keep = smoothstep(
        threshold - 0.12,
        threshold + 0.18,
        localField + broad * 0.10);

    // A narrow scratch signal removes tiny local slivers in the debug probe. It
    // is deliberately bounded by detailInfluence so it cannot become a broad
    // structural split/merge system.
    float scratchPhase = frac(
        p.y * 7.30 +
        p.x * 0.46 +
        mid * 1.70 +
        seed * 0.11);
    float scratch = 1.0 - smoothstep(0.010, 0.085, abs(scratchPhase - 0.5));
    float scratchKeep = lerp(
        1.0,
        0.54 + keep * 0.46,
        scratch * edgeBand * (0.28 + damage * 0.18));

    float detailed = baseShape * lerp(1.0, keep, detailInfluence);
    detailed *= scratchKeep;
    return saturate(detailed);
}


struct RiverWaterFoamSurfaceInfluence
{
    float macroHeight;
    float currentAccent;
    float disturbanceHeight;
    float downstreamGradient;
    float lateralGradient;
    float disturbanceVelocity;
    float wakeEnergy;
    float wakeIntensity;
    float wakeDownstreamGradient;
    float wakeLateralGradient;
};

RiverWaterFoamSurfaceInfluence RiverWaterCreateFoamSurfaceInfluence()
{
    RiverWaterFoamSurfaceInfluence influence;
    influence.macroHeight = 0.0;
    influence.currentAccent = 0.0;
    influence.disturbanceHeight = 0.0;
    influence.downstreamGradient = 0.0;
    influence.lateralGradient = 0.0;
    influence.disturbanceVelocity = 0.0;
    influence.wakeEnergy = 0.0;
    influence.wakeIntensity = 0.0;
    influence.wakeDownstreamGradient = 0.0;
    influence.wakeLateralGradient = 0.0;
    return influence;
}

float RiverWaterFoamResolveSurfaceEnergy(
    RiverWaterFoamSurfaceInfluence surface)
{
    float2 totalGradient = float2(
        surface.downstreamGradient + surface.wakeDownstreamGradient * 0.70,
        surface.lateralGradient + surface.wakeLateralGradient * 0.70);
    float gradientEnergy = saturate(length(totalGradient) * 1.10);
    float heightEnergy = saturate(
        abs(surface.disturbanceHeight) * 2.40 +
        abs(surface.macroHeight) * 0.80);
    float wakeEnergy = saturate(
        surface.wakeEnergy * 0.30 +
        surface.wakeIntensity * 0.72);
    float velocityEnergy = saturate(abs(surface.disturbanceVelocity) * 0.55);
    float currentEnergy = saturate(abs(surface.currentAccent) * 0.35);

    return saturate(
        max(max(gradientEnergy, heightEnergy), max(wakeEnergy, velocityEnergy)) +
        currentEnergy * 0.35);
}

float2 RiverWaterFoamResolveSurfaceWarpMetres(
    RiverWaterFoamSurfaceInfluence surface,
    float globalDistance,
    float lateralMetres,
    float surfaceHalfWidth,
    float materialPattern)
{
    float surfaceEnergy = RiverWaterFoamResolveSurfaceEnergy(surface);
    float2 totalGradient = float2(
        surface.downstreamGradient + surface.wakeDownstreamGradient * 0.70,
        surface.lateralGradient + surface.wakeLateralGradient * 0.70);

    float seed = materialPattern * 37.17 + 9.41;
    float waveA = sin(
        _Time.y * 1.21 +
        globalDistance * 0.37 +
        lateralMetres * 0.82 +
        seed);
    float waveB = sin(
        _Time.y * 1.73 -
        globalDistance * 0.21 +
        lateralMetres * 1.46 +
        seed * 1.63);

    // This is a render-space backtrace offset, not stored material motion.
    // Gradients pull the visible edge along the already-rendered surface slope;
    // opposed waves stop the result from becoming a one-way smear.
    float downstream =
        -totalGradient.x * 0.18 +
        surface.disturbanceVelocity * 0.045 +
        surface.wakeEnergy * 0.035 +
        waveA * (0.035 + surfaceEnergy * 0.060);
    float lateral =
        -totalGradient.y * 0.24 +
        waveB * (0.035 + surfaceEnergy * 0.075) +
        surface.wakeLateralGradient * 0.070;

    float shoreDistance01 = saturate(
        (max(0.0, surfaceHalfWidth - abs(lateralMetres))) /
        max(0.001, surfaceHalfWidth));
    float shoreGuard = lerp(0.55, 1.0, smoothstep(0.02, 0.18, shoreDistance01));

    float strength = surfaceEnergy * shoreGuard;
    return float2(
        clamp(downstream * strength, -0.38, 0.38),
        clamp(lateral * strength, -0.34, 0.34));
}

float2 RiverWaterFoamMetresToFieldUV(
    float2 metres,
    float fieldLength,
    float surfaceHalfWidth)
{
    return float2(
        metres.x / max(0.001, fieldLength),
        metres.y / max(0.001, surfaceHalfWidth * 2.0));
}

float4 RiverWaterFoamSampleInterpolatedState(
    TEXTURE2D_PARAM(previousFoam, previousFoamSampler),
    TEXTURE2D_PARAM(currentFoam, currentFoamSampler),
    float2 foamUV,
    float interpolation)
{
    float4 currentState = SAMPLE_TEXTURE2D_LOD(
        currentFoam,
        currentFoamSampler,
        foamUV,
        0.0);

    if (interpolation >= 0.999)
    {
        return currentState;
    }

    float4 previousState = SAMPLE_TEXTURE2D_LOD(
        previousFoam,
        previousFoamSampler,
        foamUV,
        0.0);
    return lerp(
        previousState,
        currentState,
        saturate(interpolation));
}

void RiverWaterFoamDecodeMaterialState(
    float4 state,
    out float presence,
    out float remainingLife,
    out float materialPattern)
{
    presence = saturate(state.x);
    remainingLife = presence > 0.0001
        ? saturate(state.y / presence)
        : 0.0;
    materialPattern = presence > 0.0001
        ? saturate(state.z / presence)
        : 0.0;
}

float RiverWaterFoamResolveStateMask(
    float4 state,
    float storedGlobalDistance,
    float lateralMetres,
    float sharpness,
    out float presence,
    out float remainingLife,
    out float materialPattern)
{
    RiverWaterFoamDecodeMaterialState(
        state,
        presence,
        remainingLife,
        materialPattern);
    float baseMask = RiverWaterFoamSharpenCoverage(
        presence,
        sharpness);
    return RiverWaterFoamPatternedMask(
        baseMask,
        presence,
        remainingLife,
        materialPattern,
        storedGlobalDistance,
        lateralMetres,
        sharpness);
}

struct RiverWaterFoamResult
{
    float presence;
    float remainingLife;
    float materialPattern;
    float mask;
    float surfaceEnergy;
    float2 fieldUV;
    float2 materialUV;
};

RiverWaterFoamResult RiverWaterEvaluateFoam(
    TEXTURE2D_PARAM(previousFoam, previousFoamSampler),
    TEXTURE2D_PARAM(currentFoam, currentFoamSampler),
    float enabled,
    float globalDistance,
    float lateralMetres,
    float surfaceHalfWidth,
    float globalStart,
    float fieldLength,
    float interpolation,
    float renderAdvectionSeconds,
    float2 resolvedVelocityMetresPerSecond,
    float obstacleInfluence,
    float flowDirection,
    float sharpness,
    float freezeAmount,
    RiverWaterFoamSurfaceInfluence surfaceInfluence)
{
    RiverWaterFoamResult result;
    result.presence = 0.0;
    result.remainingLife = 0.0;
    result.materialPattern = 0.0;
    result.mask = 0.0;
    result.surfaceEnergy = 0.0;
    result.fieldUV = 0.0;
    result.materialUV = 0.0;

    if (enabled < 0.5 || fieldLength <= 0.0001)
    {
        return result;
    }

    float2 fieldUV = float2(
        saturate((globalDistance - globalStart) / fieldLength),
        saturate(lateralMetres / max(0.001, surfaceHalfWidth) * 0.5 + 0.5));
    float flowSign = flowDirection >= 0.0 ? 1.0 : -1.0;
    // Point-velocity extrapolation cannot reproduce the conservative solver's
    // closed obstacle faces. Fade it out before obstacle routing becomes strong
    // so a material tick cannot invalidate a predicted cross-face displacement
    // and create a repeated advance/snap-back sawtooth.
    float renderPredictionConfidence = 1.0 - smoothstep(
        0.05,
        0.35,
        saturate(obstacleInfluence));
    float2 residualTravelMetres = float2(
        resolvedVelocityMetresPerSecond.x * flowSign,
        resolvedVelocityMetresPerSecond.y) *
        max(0.0, renderAdvectionSeconds) *
        renderPredictionConfidence;
    float storedGlobalDistance = globalDistance - residualTravelMetres.x;
    float storedLateralMetres = lateralMetres - residualTravelMetres.y;
    float2 foamUV = float2(
        saturate((storedGlobalDistance - globalStart) / fieldLength),
        saturate(storedLateralMetres /
            max(0.001, surfaceHalfWidth) * 0.5 + 0.5));

    float blend = saturate(interpolation);
    float4 storedState = RiverWaterFoamSampleInterpolatedState(
        TEXTURE2D_ARGS(
            previousFoam,
            previousFoamSampler),
        TEXTURE2D_ARGS(
            currentFoam,
            currentFoamSampler),
        foamUV,
        blend);

    float storedPresence;
    float storedRemainingLife;
    float storedMaterialPattern;
    float storedMask = RiverWaterFoamResolveStateMask(
        storedState,
        storedGlobalDistance,
        storedLateralMetres,
        sharpness,
        storedPresence,
        storedRemainingLife,
        storedMaterialPattern);

    // This is the residual-predicted state used by normal rendering. Raw Layer C
    // diagnostics sample the committed current texture directly at fieldUV so
    // presentation extrapolation cannot masquerade as stored material motion.
    result.presence = storedPresence;
    result.remainingLife = storedRemainingLife;
    result.materialPattern = storedMaterialPattern;
    result.fieldUV = fieldUV;
    result.materialUV = foamUV;

    float liquidFactor = 1.0 - saturate(freezeAmount);
    float surfaceEnergy = RiverWaterFoamResolveSurfaceEnergy(
        surfaceInfluence) * liquidFactor;

    float2 warpMetres = RiverWaterFoamResolveSurfaceWarpMetres(
        surfaceInfluence,
        globalDistance,
        lateralMetres,
        surfaceHalfWidth,
        storedMaterialPattern);
    float2 warpUV = RiverWaterFoamMetresToFieldUV(
        warpMetres,
        fieldLength,
        surfaceHalfWidth);
    float2 visualFoamUV = saturate(foamUV - warpUV);

    float4 visualState = RiverWaterFoamSampleInterpolatedState(
        TEXTURE2D_ARGS(
            previousFoam,
            previousFoamSampler),
        TEXTURE2D_ARGS(
            currentFoam,
            currentFoamSampler),
        visualFoamUV,
        blend);

    float visualPresence;
    float visualRemainingLife;
    float visualMaterialPattern;
    float visualMask = RiverWaterFoamResolveStateMask(
        visualState,
        storedGlobalDistance - warpMetres.x,
        storedLateralMetres - warpMetres.y,
        sharpness,
        visualPresence,
        visualRemainingLife,
        visualMaterialPattern);

    float coupledMask = lerp(
        storedMask,
        visualMask,
        saturate(surfaceEnergy * 0.72));

    // Wake and lee regions should not spawn Foam, but they may visually stretch
    // or compress already-nearby material. This extra pair of render samples is
    // bounded and only contributes near an existing stored/warped body.
    [branch]
    if (surfaceEnergy > 0.015)
    {
        float2 stretchDirection = float2(
            0.82 + abs(surfaceInfluence.disturbanceVelocity) * 0.16 +
            surfaceInfluence.wakeEnergy * 0.20,
            surfaceInfluence.lateralGradient +
            surfaceInfluence.wakeLateralGradient * 0.42);
        float stretchLength = length(stretchDirection);
        if (stretchLength > 0.0001)
        {
            stretchDirection /= stretchLength;
        }
        else
        {
            stretchDirection = float2(1.0, 0.0);
        }
        float stretchMetres = surfaceEnergy *
            (0.035 + surfaceInfluence.wakeIntensity * 0.125);
        float2 stretchUV = RiverWaterFoamMetresToFieldUV(
            stretchDirection * stretchMetres,
            fieldLength,
            surfaceHalfWidth);

        float4 leadState = RiverWaterFoamSampleInterpolatedState(
            TEXTURE2D_ARGS(
                previousFoam,
                previousFoamSampler),
            TEXTURE2D_ARGS(
                currentFoam,
                currentFoamSampler),
            saturate(visualFoamUV - stretchUV),
            blend);
        float4 trailState = RiverWaterFoamSampleInterpolatedState(
            TEXTURE2D_ARGS(
                previousFoam,
                previousFoamSampler),
            TEXTURE2D_ARGS(
                currentFoam,
                currentFoamSampler),
            saturate(visualFoamUV + stretchUV),
            blend);

        float leadPresence;
        float leadLife;
        float leadPattern;
        float leadMask = RiverWaterFoamResolveStateMask(
            leadState,
            storedGlobalDistance - warpMetres.x - stretchDirection.x * stretchMetres,
            storedLateralMetres - warpMetres.y - stretchDirection.y * stretchMetres,
            sharpness,
            leadPresence,
            leadLife,
            leadPattern);
        float trailPresence;
        float trailLife;
        float trailPattern;
        float trailMask = RiverWaterFoamResolveStateMask(
            trailState,
            storedGlobalDistance - warpMetres.x + stretchDirection.x * stretchMetres,
            storedLateralMetres - warpMetres.y + stretchDirection.y * stretchMetres,
            sharpness,
            trailPresence,
            trailLife,
            trailPattern);

        float nearMaterial = saturate(max(
            max(storedMask, visualMask),
            max(leadMask, trailMask)));
        float stretchedMask = max(
            coupledMask,
            max(leadMask, trailMask) * (0.42 + surfaceEnergy * 0.30));
        coupledMask = lerp(
            coupledMask,
            stretchedMask,
            saturate(nearMaterial * surfaceEnergy));
    }

    float edgeExposure = 1.0 - smoothstep(0.36, 0.82, max(storedPresence, visualPresence));
    float contactWave = sin(
        _Time.y * (1.10 + surfaceEnergy * 0.75) +
        globalDistance * 2.15 +
        lateralMetres * 5.30 +
        storedMaterialPattern * 5.70) * 0.5 + 0.5;
    float surfaceBreak = lerp(
        0.92,
        1.10,
        contactWave);
    coupledMask *= lerp(
        1.0,
        surfaceBreak,
        saturate(edgeExposure * surfaceEnergy * 0.85));

    // Do not allow render coupling to erase coherent stored material. It may
    // visually bend/thin edges, but lifecycle remains in the material field.
    coupledMask = max(
        coupledMask,
        storedMask * lerp(0.72, 0.58, saturate(surfaceEnergy)));
    coupledMask *= liquidFactor;

    result.mask = saturate(coupledMask);
    result.surfaceEnergy = surfaceEnergy;
    return result;
}

float3 RiverWaterResolveFoamInteriorLighting(
    float3 lighting,
    float foamMask,
    float surfaceEnergy,
    float minimumNightVisibility)
{
    float3 safeLighting = max(
        float3(
            minimumNightVisibility,
            minimumNightVisibility,
            minimumNightVisibility),
        lighting);

    // Foam is a clean stylized surface film, not bare water. The water normal
    // and small detail noise may influence the edge, but the interior should
    // not inherit every granular peak/valley from the liquid shader. Strong
    // waves/wakes/disturbances are still allowed to show through at a reduced
    // strength so Foam does not look detached from the river.
    float luminance = dot(
        safeLighting,
        float3(0.2126, 0.7152, 0.0722));
    float3 flatLighting = lerp(
        float3(1.0, 1.0, 1.0),
        float3(max(minimumNightVisibility, luminance), max(minimumNightVisibility, luminance), max(minimumNightVisibility, luminance)),
        0.20);

    float interior = smoothstep(0.42, 0.82, saturate(foamMask));
    float strongSurfaceFeature = smoothstep(0.32, 0.78, saturate(surfaceEnergy));
    float detailAllowance = lerp(0.10, 0.34, strongSurfaceFeature);
    float3 filteredInteriorLighting = lerp(
        flatLighting,
        safeLighting,
        detailAllowance);

    return lerp(
        safeLighting,
        filteredInteriorLighting,
        interior);
}

float3 RiverWaterResolveFoamColourFiltered(
    float3 foamColour,
    float3 lighting,
    float foamMask,
    float surfaceEnergy,
    float minimumNightVisibility)
{
    return max(
        0.0,
        foamColour * RiverWaterResolveFoamInteriorLighting(
            lighting,
            foamMask,
            surfaceEnergy,
            minimumNightVisibility));
}

#endif
