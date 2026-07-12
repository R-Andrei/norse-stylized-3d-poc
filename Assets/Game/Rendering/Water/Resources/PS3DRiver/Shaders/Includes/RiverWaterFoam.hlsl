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

float RiverWaterFoamResolveMeaningfulPresenceFootprint(
    float presence)
{
    // Match the accepted material diagnostic footprint. Lifecycle-Faithful
    // rendering still requires meaningful material, but it does not require a
    // dense local concentration before Remaining Life can remain visible.
    return smoothstep(0.02, 0.16, saturate(presence));
}

struct RiverWaterFoamPatternFields
{
    float combined;
    float chip;
    float fray;
};

RiverWaterFoamPatternFields RiverWaterFoamStablePatternFields(
    float materialPattern,
    float storedGlobalDistance,
    float lateralMetres,
    float breakupScale)
{
    RiverWaterFoamPatternFields fields;
    float seed = materialPattern * 43.731 + 11.17;
    float2 p = float2(storedGlobalDistance, lateralMetres);

    // Use several differently-oriented layers so the stored ribbon footprint
    // is not simply displayed as long parallel strokes. These coordinates are
    // storage-space metres, so the breakup rides with the material instead of
    // swimming in screen space. The accepted combined visibility pattern is
    // preserved exactly; Chip and Fray only expose transient reuse signals.
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

    float broadField = saturate(
        broad * 0.58 +
        diagonal * 0.42);
    float scale = saturate(breakupScale);

    fields.combined = saturate(
        materialPattern * 0.32 +
        broad * 0.24 +
        diagonal * 0.22 +
        mid * 0.16 +
        fine * 0.06);
    fields.chip = lerp(mid, broadField, scale);
    fields.fray = lerp(fine, mid, scale);
    return fields;
}

float RiverWaterFoamPatternedMask(
    float baseMask,
    float presence,
    float remainingLife,
    float materialPattern,
    float storedGlobalDistance,
    float lateralMetres,
    float sharpness,
    float breakupScale,
    out float2 breakupField)
{
    float s = saturate(sharpness);
    float life = saturate(remainingLife);
    float damage = 1.0 - life;

    float seed = materialPattern * 43.731 + 11.17;
    RiverWaterFoamPatternFields patternFields =
        RiverWaterFoamStablePatternFields(
            materialPattern,
            storedGlobalDistance,
            lateralMetres,
            breakupScale);
    float pattern = patternFields.combined;
    breakupField = float2(
        patternFields.chip,
        patternFields.fray);

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

float RiverWaterFoamApplyEdgeBreakup(
    float baseShape,
    float materialPattern,
    float2 breakupField,
    float globalDistance,
    float lateralMetres,
    float chipStrength,
    float frayStrength,
    float breakupScale)
{
    float shape = saturate(baseShape);
    float chip = saturate(chipStrength);
    float fray = saturate(frayStrength);

    // Chip and Fray are uniform authoring controls. Their shared neutral branch
    // preserves the accepted 5.17A.1 silhouette exactly and avoids unnecessary
    // breakup arithmetic in coherent empty or disabled regions.
    [branch]
    if (shape <= 0.0001 || max(chip, fray) <= 0.0001)
    {
        return shape;
    }

    float chipField = saturate(breakupField.x);
    float frayField = saturate(breakupField.y);
    float scale = saturate(breakupScale);

    // 5.17B.1 deliberately makes the top end an exaggerated stress test.
    // Scale now changes activation as well as field selection: low values keep
    // smaller frequent breakup, while high values admit broader, sparser bites.
    // The survival functions remain monotone in incoming shape and preserve a
    // fully established core at shape == 1.
    float chipSignal = smoothstep(
        lerp(0.40, 0.30, scale),
        lerp(0.74, 0.62, scale),
        chipField);
    float chipDepth = saturate(
        chip * chipSignal);
    float chipCut = smoothstep(
        0.10 + chipDepth * 0.62,
        0.26 + chipDepth * 0.72,
        shape);
    float chipKeep = lerp(
        1.0,
        chipCut,
        smoothstep(0.01, 0.08, chipDepth));

    // Fray now reaches visibly into the rendered edge instead of editing only
    // antialiased fringe pixels. It still fades before the fully established
    // core and cannot independently create an isolated interior hole.
    float fringeZone =
        1.0 - smoothstep(
            lerp(0.40, 0.46, scale),
            lerp(0.82, 0.86, scale),
            shape);
    float fraySignal = smoothstep(
        lerp(0.26, 0.22, scale),
        lerp(0.62, 0.52, scale),
        frayField);
    float frayAuthority = saturate(
        fray *
        fringeZone *
        fraySignal);
    float frayCut = smoothstep(
        0.02 + frayAuthority * 0.26,
        0.14 + frayAuthority * 0.56,
        shape);
    float frayKeep = lerp(
        1.0,
        frayCut,
        smoothstep(0.01, 0.08, frayAuthority));

    // Short cuts remain derived from Chip Strength. The calibration broadens
    // their line width, lowers the stable anchors, and lets maximum authority
    // cut through opaque edge coverage while still preserving shape == 1.
    float crackFrequency = lerp(
        10.0,
        3.5,
        scale);
    float crackPhase = frac(
        lateralMetres * crackFrequency +
        globalDistance * crackFrequency * 0.18 +
        chipField * 1.70 +
        materialPattern * 2.31);
    float crackHalfWidth = lerp(
        0.045,
        0.100,
        scale);
    float crackLine =
        1.0 - smoothstep(
            crackHalfWidth * 0.25,
            crackHalfWidth,
            abs(crackPhase - 0.5));
    float crackAnchor =
        smoothstep(
            lerp(0.52, 0.42, scale),
            lerp(0.76, 0.64, scale),
            chipField) *
        smoothstep(
            lerp(0.28, 0.22, scale),
            lerp(0.58, 0.48, scale),
            frayField);
    float crackAuthority = saturate(
        chip *
        crackLine *
        crackAnchor);
    float crackCut = smoothstep(
        0.16 + crackAuthority * 0.56,
        0.32 + crackAuthority * 0.64,
        shape);
    float crackKeep = lerp(
        1.0,
        crackCut,
        crackAuthority);

    float breakupKeep = min(
        chipKeep,
        min(frayKeep, crackKeep));
    return saturate(
        shape * breakupKeep);
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
    float finalVisibilityMode,
    float breakupScale,
    out float presence,
    out float remainingLife,
    out float materialPattern,
    out float2 breakupField)
{
    RiverWaterFoamDecodeMaterialState(
        state,
        presence,
        remainingLife,
        materialPattern);
    float baseMask;
    float patternedPresence;
    [branch]
    if (finalVisibilityMode > 0.5)
    {
        // Presence defines only the meaningful material footprint in this mode.
        // Once inside that footprint, Remaining Life and the stable material
        // pattern own deterioration instead of a second high-concentration gate.
        float lifecycleFootprint =
            RiverWaterFoamResolveMeaningfulPresenceFootprint(presence);
        baseMask = lifecycleFootprint;
        patternedPresence = lifecycleFootprint;
    }
    else
    {
        // Preserve the accepted legacy result exactly for the default A/B side.
        baseMask = RiverWaterFoamSharpenCoverage(
            presence,
            sharpness);
        patternedPresence = presence;
    }

    return RiverWaterFoamPatternedMask(
        baseMask,
        patternedPresence,
        remainingLife,
        materialPattern,
        storedGlobalDistance,
        lateralMetres,
        sharpness,
        breakupScale,
        breakupField);
}

struct RiverWaterFoamResult
{
    float presence;
    float remainingLife;
    float materialPattern;
    float mask;
    float surfaceEnergy;
    float2 breakupField;
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
    float sharpness,
    float finalVisibilityMode,
    float breakupScale,
    float freezeAmount,
    RiverWaterFoamSurfaceInfluence surfaceInfluence)
{
    RiverWaterFoamResult result;
    result.presence = 0.0;
    result.remainingLife = 0.0;
    result.materialPattern = 0.0;
    result.mask = 0.0;
    result.surfaceEnergy = 0.0;
    result.breakupField = 0.0;
    result.fieldUV = 0.0;
    result.materialUV = 0.0;

    if (enabled < 0.5 || fieldLength <= 0.0001)
    {
        return result;
    }

    float2 fieldUV = float2(
        saturate((globalDistance - globalStart) / fieldLength),
        saturate(lateralMetres / max(0.001, surfaceHalfWidth) * 0.5 + 0.5));
    // The current committed Layer C state is the production presentation
    // authority. Point-velocity residual backtracing was retired after Unity
    // validation proved that it oscillated around conservative closed faces.
    float storedGlobalDistance = globalDistance;
    float storedLateralMetres = lateralMetres;
    float2 foamUV = fieldUV;

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
    float2 storedBreakupField;
    float storedMask = RiverWaterFoamResolveStateMask(
        storedState,
        storedGlobalDistance,
        storedLateralMetres,
        sharpness,
        finalVisibilityMode,
        breakupScale,
        storedPresence,
        storedRemainingLife,
        storedMaterialPattern,
        storedBreakupField);

    // Normal rendering and raw Layer C diagnostics now share the committed
    // field coordinate. Surface warp below remains visual-only and bounded.
    result.presence = storedPresence;
    result.remainingLife = storedRemainingLife;
    result.materialPattern = storedMaterialPattern;
    result.breakupField = storedBreakupField;
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
    float2 visualBreakupField;
    float visualMask = RiverWaterFoamResolveStateMask(
        visualState,
        storedGlobalDistance - warpMetres.x,
        storedLateralMetres - warpMetres.y,
        sharpness,
        finalVisibilityMode,
        breakupScale,
        visualPresence,
        visualRemainingLife,
        visualMaterialPattern,
        visualBreakupField);

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
        float2 leadBreakupField;
        float leadMask = RiverWaterFoamResolveStateMask(
            leadState,
            storedGlobalDistance - warpMetres.x - stretchDirection.x * stretchMetres,
            storedLateralMetres - warpMetres.y - stretchDirection.y * stretchMetres,
            sharpness,
            finalVisibilityMode,
            breakupScale,
            leadPresence,
            leadLife,
            leadPattern,
            leadBreakupField);
        float trailPresence;
        float trailLife;
        float trailPattern;
        float2 trailBreakupField;
        float trailMask = RiverWaterFoamResolveStateMask(
            trailState,
            storedGlobalDistance - warpMetres.x + stretchDirection.x * stretchMetres,
            storedLateralMetres - warpMetres.y + stretchDirection.y * stretchMetres,
            sharpness,
            finalVisibilityMode,
            breakupScale,
            trailPresence,
            trailLife,
            trailPattern,
            trailBreakupField);

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
    float minimumNightVisibility,
    float edgeContrast)
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
        float3(
            max(minimumNightVisibility, luminance),
            max(minimumNightVisibility, luminance),
            max(minimumNightVisibility, luminance)),
        0.20);

    float interior = smoothstep(0.42, 0.82, saturate(foamMask));
    float strongSurfaceFeature = smoothstep(
        0.32,
        0.78,
        saturate(surfaceEnergy));
    float detailAllowance = lerp(0.10, 0.34, strongSurfaceFeature);
    float3 filteredInteriorLighting = lerp(
        flatLighting,
        safeLighting,
        detailAllowance);

    // Zero preserves the pre-5.17A lighting exactly. Negative Edge Contrast
    // suppresses the existing bright rim by moving edge lighting toward the
    // filtered interior response. Positive values visibly intensify it. The
    // established body remains on the same filtered lighting path.
    float suppressEdge = saturate(-edgeContrast);
    float intensifyEdge = saturate(edgeContrast);
    float3 controlledEdgeLighting = lerp(
        safeLighting,
        filteredInteriorLighting,
        suppressEdge);
    controlledEdgeLighting *= 1.0 + intensifyEdge * 0.50;

    return lerp(
        controlledEdgeLighting,
        filteredInteriorLighting,
        interior);
}

float3 RiverWaterResolveFoamColourFiltered(
    float3 foamColour,
    float3 lighting,
    float foamMask,
    float surfaceEnergy,
    float minimumNightVisibility,
    float edgeContrast)
{
    return max(
        0.0,
        foamColour * RiverWaterResolveFoamInteriorLighting(
            lighting,
            foamMask,
            surfaceEnergy,
            minimumNightVisibility,
            edgeContrast));
}

struct RiverWaterFoamComposition
{
    float3 colour;
    float opacity;
};

RiverWaterFoamComposition RiverWaterResolveFoamComposition(
    float3 foamBaseTint,
    float foamBaseOpacity,
    float foamMask,
    float interiorOpacityFloor,
    float edgeContrast,
    float3 lighting,
    float surfaceEnergy,
    float minimumNightVisibility)
{
    RiverWaterFoamComposition result;

    // Preserve the accepted pre-5.17A blend exactly at Floor 0 / Contrast 0.
    // The absolute floor applies only to an established body, so it cannot
    // create Foam in weak fringe or outside the incoming silhouette.
    float safeFoamMask = saturate(foamMask);
    float baseCoverage = smoothstep(0.08, 0.46, safeFoamMask);
    float establishedBody = smoothstep(0.42, 0.82, safeFoamMask);
    float baseOpacity = baseCoverage * saturate(foamBaseOpacity);
    float floorOpacity =
        establishedBody * saturate(interiorOpacityFloor);

    result.colour = RiverWaterResolveFoamColourFiltered(
        foamBaseTint,
        lighting,
        safeFoamMask,
        surfaceEnergy,
        minimumNightVisibility,
        edgeContrast);
    result.opacity = saturate(max(baseOpacity, floorOpacity));
    return result;
}

#endif
