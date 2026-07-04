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

struct RiverWaterFoamResult
{
    float presence;
    float remainingLife;
    float mask;
    float2 fieldUV;
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
    float renderTravelMetres,
    float sharpness,
    float freezeAmount)
{
    RiverWaterFoamResult result;
    result.presence = 0.0;
    result.remainingLife = 0.0;
    result.mask = 0.0;
    result.fieldUV = 0.0;

    if (enabled < 0.5 || fieldLength <= 0.0001)
    {
        return result;
    }

    float2 fieldUV = float2(
        saturate((globalDistance - globalStart) / fieldLength),
        saturate(lateralMetres / max(0.001, surfaceHalfWidth) * 0.5 + 0.5));
    float2 foamUV = float2(
        saturate(((globalDistance - renderTravelMetres) - globalStart) / fieldLength),
        fieldUV.y);

    float blend = saturate(interpolation);
    float4 currentState = SAMPLE_TEXTURE2D_LOD(
        currentFoam,
        currentFoamSampler,
        foamUV,
        0.0);
    float4 state = currentState;
    if (blend < 0.999)
    {
        float4 previousState = SAMPLE_TEXTURE2D_LOD(
            previousFoam,
            previousFoamSampler,
            foamUV,
            0.0);
        state = lerp(
            previousState,
            currentState,
            blend);
    }

    float presence = saturate(state.x);
    float remainingLife = presence > 0.0001
        ? saturate(state.y / presence)
        : 0.0;
    float materialPattern = presence > 0.0001
        ? saturate(state.z / presence)
        : 0.0;

    // Presence is geometric coverage, not emitter strength. The final mask is
    // intentionally sharper than the raw transported coverage because the
    // phase-transport model makes any broad bilinear softness read as a blurry
    // static decal. Material Pattern finally participates here: it breaks the
    // born footprint into a stable internal identity, while a slow threshold
    // drift makes only the rendered silhouette/holes breathe over time.
    float baseMask = RiverWaterFoamSharpenCoverage(
        presence,
        sharpness);
    float storedGlobalDistance = globalDistance - renderTravelMetres;
    float mask = RiverWaterFoamPatternedMask(
        baseMask,
        presence,
        remainingLife,
        materialPattern,
        storedGlobalDistance,
        lateralMetres,
        sharpness);
    mask *= 1.0 - saturate(freezeAmount);

    result.presence = presence;
    result.remainingLife = remainingLife;
    result.mask = saturate(mask);
    result.fieldUV = fieldUV;
    return result;
}

float3 RiverWaterResolveFoamColour(
    float3 foamColour,
    float3 lighting,
    float minimumNightVisibility)
{
    float3 lit = max(
        float3(
            minimumNightVisibility,
            minimumNightVisibility,
            minimumNightVisibility),
        lighting);
    return max(0.0, foamColour * lit);
}

#endif
