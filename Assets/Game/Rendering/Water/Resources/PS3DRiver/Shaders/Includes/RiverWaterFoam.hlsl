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
    float strandWarp;
    float strandGroup;
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
    // Scale selects feature size, not effective breakup authority. The broad
    // composite has a compressed centre-weighted distribution, so normalize
    // each source band before interpolation. This keeps Scale 1 broader and
    // sparser without making it silently weaker than Scale 0.
    float mediumChipPattern = saturate(
        (mid - 0.5) * 1.35 + 0.5);
    float broadChipPattern = saturate(
        (broadField - 0.5) * 2.0 + 0.5);
    float fineFrayPattern = saturate(
        (fine - 0.5) * 1.20 + 0.5);
    float mediumFrayPattern = mediumChipPattern;

    fields.chip = lerp(
        mediumChipPattern,
        broadChipPattern,
        scale);
    fields.fray = lerp(
        fineFrayPattern,
        mediumFrayPattern,
        scale);

    // Strands are a separate authoring feature. Keep their stable broad warp
    // and grouping sources independent from Chip/Fray Scale so changing edge
    // breakup granularity cannot reseed or reorient strand families.
    fields.strandWarp = saturate(
        broad * 0.70 +
        diagonal * 0.30);
    fields.strandGroup = saturate(
        broad * 0.28 +
        diagonal * 0.52 +
        mid * 0.20);
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
    out float softVisibility,
    out float4 breakupField)
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
    breakupField = float4(
        patternFields.chip,
        patternFields.fray,
        patternFields.strandWarp,
        patternFields.strandGroup);

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
    softVisibility = saturate(visible);
    float hardVisible = smoothstep(0.22, 0.58, softVisibility);
    float fringe = smoothstep(0.06, 0.34, softVisibility) * 0.34;
    return saturate(max(hardVisible, fringe));
}

float RiverWaterFoamApplyEdgeBreakup(
    float hardenedShape,
    float softVisibility,
    float materialPresence,
    float materialPattern,
    float4 breakupField,
    float globalDistance,
    float lateralMetres,
    float chipStrength,
    float frayStrength,
    float strandStrength,
    float strandSpacing,
    float strandWidth,
    float strandCurvature,
    float fragmentationStrength,
    float fragmentSize,
    float fragmentReach)
{
    float shape = saturate(hardenedShape);
    float softShape = saturate(softVisibility);
    float presence = saturate(materialPresence);
    float chip = saturate(chipStrength);
    float fray = saturate(frayStrength);
    float strand = saturate(strandStrength);
    float fragmentation = saturate(fragmentationStrength);

    // Neutral authoring values must reproduce the accepted 5.17A.1 hardened
    // silhouette exactly. Scale, spacing, width, curvature, size, and reach
    // have no visual authority while their owning feature strengths are zero.
    [branch]
    if (shape <= 0.0001 ||
        max(max(max(chip, fray), strand), fragmentation) <= 0.0001)
    {
        return shape;
    }

    if (softShape <= 0.0001)
    {
        return 0.0;
    }

    float chipPattern = saturate(breakupField.x);
    float frayPattern = saturate(breakupField.y);
    float visibilityAA = max(
        fwidth(softShape),
        0.001);
    float exactCore = step(0.999, softShape);

    // Medium coherent chip regions now cut against the pre-hardening signal.
    // A selected bite can therefore reach true zero coverage instead of only
    // weakening the antialiased rim of an already-binary mask.
    float chipSelection = smoothstep(
        0.50,
        0.76,
        chipPattern);
    float chipAuthority = saturate(
        chip *
        chipSelection);
    float chipThreshold = lerp(
        0.16,
        0.98,
        chipAuthority);
    float chipCut = smoothstep(
        chipThreshold - visibilityAA,
        chipThreshold + visibilityAA,
        softShape);
    float chipKeep = lerp(
        1.0,
        chipCut,
        smoothstep(0.001, 0.08, chipAuthority));
    chipKeep = max(chipKeep, exactCore);

    // Fray uses the same binary survival model but a shallower maximum depth,
    // producing serrated weak edges without competing with the principal chip
    // path for the established body.
    float fraySelection = smoothstep(
        0.42,
        0.70,
        frayPattern);
    float frayAuthority = saturate(
        fray *
        fraySelection);
    float frayThreshold = lerp(
        0.04,
        0.72,
        frayAuthority);
    float frayCut = smoothstep(
        frayThreshold - visibilityAA,
        frayThreshold + visibilityAA,
        softShape);
    float frayKeep = lerp(
        1.0,
        frayCut,
        smoothstep(0.001, 0.08, frayAuthority));
    frayKeep = max(frayKeep, exactCore);

    // Foam Strands are independent from Chip/Fray. The old chip-owned periodic
    // crack comb was removed because it could expose many adjacent subpixel
    // lanes. This version keeps the useful pulled-strip look while enforcing
    // stable grouping, non-adjacent candidate lanes, broad coherent curvature,
    // and screen-space density protection.
    float spacing = saturate(strandSpacing);
    float width = saturate(strandWidth);
    float curvature = saturate(strandCurvature);
    float strandWarp = saturate(breakupField.z);
    float strandGroup = saturate(breakupField.w);

    float strandFrequency = lerp(
        6.0,
        2.2,
        spacing);
    float rawStrandPhase =
        lateralMetres * strandFrequency +
        globalDistance * strandFrequency * 0.16 +
        (strandWarp - 0.5) * lerp(0.0, 1.40, curvature) +
        materialPattern * 2.31;
    float strandPhase = frac(rawStrandPhase);
    float laneIndex = floor(rawStrandPhase);
    float laneParity = 1.0 - step(
        0.5,
        frac(laneIndex * 0.5));
    float laneSelectionNoise = RiverWaterFoamHash21(
        float2(laneIndex, 17.0));
    float laneSelection = laneParity * smoothstep(
        0.30,
        0.64,
        laneSelectionNoise);

    float phaseFootprint = max(
        fwidth(rawStrandPhase),
        0.001);
    float densityKeep = 1.0 - smoothstep(
        0.22,
        0.40,
        phaseFootprint);
    float authoredHalfWidth = lerp(
        0.040,
        0.125,
        width);
    float resolvedHalfWidth = max(
        authoredHalfWidth,
        phaseFootprint * 0.45);
    float strandLine = 1.0 - smoothstep(
        resolvedHalfWidth,
        resolvedHalfWidth + phaseFootprint,
        abs(strandPhase - 0.5));
    float groupEnvelope = smoothstep(
        0.38,
        0.64,
        strandGroup);
    float edgeReach = 1.0 - smoothstep(
        0.90,
        0.995,
        softShape);
    float strandAuthority = saturate(
        strand *
        strandLine *
        laneSelection *
        groupEnvelope *
        densityKeep *
        edgeReach);
    float strandThreshold = lerp(
        0.08,
        0.96,
        strandAuthority);
    float strandCut = smoothstep(
        strandThreshold - visibilityAA,
        strandThreshold + visibilityAA,
        softShape);
    float strandKeep = lerp(
        1.0,
        strandCut,
        smoothstep(0.001, 0.08, strandAuthority));
    strandKeep = max(strandKeep, exactCore);

    // Regional fragmentation owns a different size band from Chip, Fray, and
    // Strands. It uses the existing stable broad/diagonal/mid signals to cut
    // coherent portions of weak and partial-presence edge material. Fragment
    // Size progressively removes subdivision detail from the same broad zones
    // instead of crossfading to an unrelated pattern identity.
    float size = saturate(fragmentSize);
    float reach = saturate(fragmentReach);
    float fragmentFoundation = saturate(
        (breakupField.z * 0.62 + breakupField.w * 0.38 - 0.5) * 1.55 +
        0.5);
    float fragmentSubdivision = saturate(
        ((breakupField.w - breakupField.z) * 1.80) + 0.5);
    float fragmentSignal = saturate(
        fragmentFoundation +
        (fragmentSubdivision - 0.5) * 0.18 * (1.0 - size));
    float regionalSelection = smoothstep(
        0.46,
        0.68,
        fragmentSignal);

    // Material Presence identifies the weak/transitional population observed
    // by the Material Presence diagnostic. Soft visibility supplies the actual
    // cuttable rendered edge band. Reach broadens both gates inward, but the
    // exact saturated core remains protected below.
    float meaningfulPresence = smoothstep(
        0.02,
        0.16,
        presence);
    float presenceCoreStart = lerp(
        0.56,
        0.72,
        reach);
    float presenceCoreEnd = lerp(
        0.80,
        0.96,
        reach);
    float partialPresenceBand = meaningfulPresence *
        (1.0 - smoothstep(
            presenceCoreStart,
            presenceCoreEnd,
            presence));
    float softBandEnd = lerp(
        0.80,
        0.995,
        reach);
    float visualEdgeBand = smoothstep(
        0.04,
        0.18,
        softShape) *
        (1.0 - smoothstep(
            softBandEnd - 0.12,
            softBandEnd,
            softShape));
    float fragmentAuthority = saturate(
        fragmentation *
        regionalSelection *
        partialPresenceBand *
        visualEdgeBand);
    float fragmentMaximumThreshold = lerp(
        0.72,
        0.985,
        reach);
    float fragmentThreshold = lerp(
        0.08,
        fragmentMaximumThreshold,
        fragmentAuthority);
    float fragmentCut = smoothstep(
        fragmentThreshold - visibilityAA,
        fragmentThreshold + visibilityAA,
        softShape);
    float fragmentKeep = lerp(
        1.0,
        fragmentCut,
        smoothstep(0.001, 0.10, fragmentAuthority));
    fragmentKeep = max(fragmentKeep, exactCore);

    float breakupKeep = min(
        min(chipKeep, frayKeep),
        min(strandKeep, fragmentKeep));
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
    out float softVisibility,
    out float presence,
    out float remainingLife,
    out float materialPattern,
    out float4 breakupField)
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
        softVisibility,
        breakupField);
}

struct RiverWaterFoamResult
{
    float presence;
    float remainingLife;
    float materialPattern;
    float mask;
    float softVisibility;
    float surfaceEnergy;
    float4 breakupField;
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
    result.softVisibility = 0.0;
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

    float storedSoftVisibility;
    float storedPresence;
    float storedRemainingLife;
    float storedMaterialPattern;
    float4 storedBreakupField;
    float storedMask = RiverWaterFoamResolveStateMask(
        storedState,
        storedGlobalDistance,
        storedLateralMetres,
        sharpness,
        finalVisibilityMode,
        breakupScale,
        storedSoftVisibility,
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

    float visualSoftVisibility;
    float visualPresence;
    float visualRemainingLife;
    float visualMaterialPattern;
    float4 visualBreakupField;
    float visualMask = RiverWaterFoamResolveStateMask(
        visualState,
        storedGlobalDistance - warpMetres.x,
        storedLateralMetres - warpMetres.y,
        sharpness,
        finalVisibilityMode,
        breakupScale,
        visualSoftVisibility,
        visualPresence,
        visualRemainingLife,
        visualMaterialPattern,
        visualBreakupField);

    float surfaceCoupling = saturate(surfaceEnergy * 0.72);
    float coupledMask = lerp(
        storedMask,
        visualMask,
        surfaceCoupling);
    float coupledSoftVisibility = lerp(
        storedSoftVisibility,
        visualSoftVisibility,
        surfaceCoupling);

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

        float leadSoftVisibility;
        float leadPresence;
        float leadLife;
        float leadPattern;
        float4 leadBreakupField;
        float leadMask = RiverWaterFoamResolveStateMask(
            leadState,
            storedGlobalDistance - warpMetres.x - stretchDirection.x * stretchMetres,
            storedLateralMetres - warpMetres.y - stretchDirection.y * stretchMetres,
            sharpness,
            finalVisibilityMode,
            breakupScale,
            leadSoftVisibility,
            leadPresence,
            leadLife,
            leadPattern,
            leadBreakupField);
        float trailSoftVisibility;
        float trailPresence;
        float trailLife;
        float trailPattern;
        float4 trailBreakupField;
        float trailMask = RiverWaterFoamResolveStateMask(
            trailState,
            storedGlobalDistance - warpMetres.x + stretchDirection.x * stretchMetres,
            storedLateralMetres - warpMetres.y + stretchDirection.y * stretchMetres,
            sharpness,
            finalVisibilityMode,
            breakupScale,
            trailSoftVisibility,
            trailPresence,
            trailLife,
            trailPattern,
            trailBreakupField);

        float nearMaterial = saturate(max(
            max(storedMask, visualMask),
            max(leadMask, trailMask)));
        float stretchWeight = saturate(
            nearMaterial * surfaceEnergy);
        float stretchScale = 0.42 + surfaceEnergy * 0.30;
        float stretchedMask = max(
            coupledMask,
            max(leadMask, trailMask) * stretchScale);
        float stretchedSoftVisibility = max(
            coupledSoftVisibility,
            max(leadSoftVisibility, trailSoftVisibility) * stretchScale);
        coupledMask = lerp(
            coupledMask,
            stretchedMask,
            stretchWeight);
        coupledSoftVisibility = lerp(
            coupledSoftVisibility,
            stretchedSoftVisibility,
            stretchWeight);
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
    float surfaceBreakWeight = saturate(
        edgeExposure * surfaceEnergy * 0.85);
    float surfaceBreakMultiplier = lerp(
        1.0,
        surfaceBreak,
        surfaceBreakWeight);
    coupledMask *= surfaceBreakMultiplier;
    coupledSoftVisibility *= surfaceBreakMultiplier;

    // Do not allow render coupling to erase coherent stored material. It may
    // visually bend/thin edges, but lifecycle remains in the material field.
    float storedRetention = lerp(
        0.72,
        0.58,
        saturate(surfaceEnergy));
    coupledMask = max(
        coupledMask,
        storedMask * storedRetention);
    coupledSoftVisibility = max(
        coupledSoftVisibility,
        storedSoftVisibility * storedRetention);
    coupledMask *= liquidFactor;
    coupledSoftVisibility *= liquidFactor;

    result.mask = saturate(coupledMask);
    result.softVisibility = saturate(coupledSoftVisibility);
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
