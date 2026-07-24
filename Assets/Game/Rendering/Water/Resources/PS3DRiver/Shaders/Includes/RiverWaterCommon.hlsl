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
// Profile Variation shapes one positive shore-wave packet internally through
// a slope-continuous start/middle/end curve. Size Variation gives successive
// travelling packet identities stable deterministic overall sizes. Transition
// Length shapes shoulders only inside each packet; it never changes packet
// support or the explicit inter-packet Gap. Both variation controls share the
// same left/right asymmetry contract and never reseed during runtime.

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

float RiverWaterResolveShoreEvolutionIdentityState(
    float waveIndex,
    float time,
    float duration,
    float sideSign,
    float seed,
    float sideAsymmetry)
{
    float phaseOffset = RiverWaterResolveShoreSideValue(
        waveIndex,
        sideSign,
        seed,
        sideAsymmetry,
        271.0);
    float cycle = frac(
        time / max(1.0, duration) +
        phaseOffset);
    float triangleWave = 1.0 - abs(cycle * 2.0 - 1.0);
    float smoothCycle = triangleWave * triangleWave *
        (3.0 - 2.0 * triangleWave);
    return smoothCycle * 2.0 - 1.0;
}

struct RiverWaterShorePacketState
{
    float packetIndex;
    float localT;
    float localDistance;
    float packetLength;
    float gapLength;
    float period;
    float envelope;
};

RiverWaterShorePacketState RiverWaterResolveShorePacketState(
    float globalDistance,
    float time,
    float flowSpeed,
    float shoreWaveLength,
    float shoreWaveGap,
    float transitionLength,
    float seed)
{
    RiverWaterShorePacketState state;
    state.packetLength = max(0.25, shoreWaveLength);
    state.gapLength = max(0.0, shoreWaveGap);
    state.period = max(0.25, state.packetLength + state.gapLength);

    float travelledDistance =
        globalDistance -
        time * flowSpeed +
        frac(seed * 0.01371) * state.period;
    float packetCoordinate = travelledDistance / state.period;
    state.packetIndex = floor(packetCoordinate);
    state.localDistance = frac(packetCoordinate) * state.period;
    state.localT = saturate(
        state.localDistance / max(0.0001, state.packetLength));

    // Packet support is exact. Length owns the complete positive-wave region
    // and Gap owns the complete calm region. No transition fade is allowed to
    // shorten the packet or enlarge the authored gap. The positive lobe itself
    // supplies zero value and zero slope at both packet boundaries.
    state.envelope = 1.0 - step(
        state.packetLength,
        state.localDistance);
    return state;
}

float RiverWaterResolvePacketProfileValue(
    float packetIndex,
    float localT,
    float sideSign,
    float seed,
    float sideAsymmetry,
    float variation,
    float salt,
    float minimumValue,
    float maximumValue)
{
    if (variation <= 0.0001)
    {
        return 1.0;
    }

    float startValue = RiverWaterResolveShoreSideValue(
        packetIndex * 3.0,
        sideSign,
        seed,
        sideAsymmetry,
        salt);
    float middleValue = RiverWaterResolveShoreSideValue(
        packetIndex * 3.0 + 1.0,
        sideSign,
        seed,
        sideAsymmetry,
        salt);
    float endValue = RiverWaterResolveShoreSideValue(
        packetIndex * 3.0 + 2.0,
        sideSign,
        seed,
        sideAsymmetry,
        salt);

    float firstHalf = smoothstep(0.0, 1.0, saturate(localT * 2.0));
    float secondHalf = smoothstep(
        0.0,
        1.0,
        saturate((localT - 0.5) * 2.0));
    float rawValue = localT < 0.5
        ? lerp(startValue, middleValue, firstHalf)
        : lerp(middleValue, endValue, secondHalf);
    float shapedValue = lerp(minimumValue, maximumValue, rawValue);
    return lerp(1.0, shapedValue, saturate(variation));
}

float RiverWaterResolvePacketSizeValue(
    float packetIndex,
    float sideSign,
    float seed,
    float sideAsymmetry,
    float variation,
    float salt,
    float minimumValue,
    float maximumValue)
{
    if (variation <= 0.0001)
    {
        return 1.0;
    }

    float rawValue = RiverWaterResolveShoreSideValue(
        packetIndex,
        sideSign,
        seed,
        sideAsymmetry,
        salt);
    float shapedValue = lerp(minimumValue, maximumValue, rawValue);
    return lerp(1.0, shapedValue, saturate(variation));
}

float2 RiverWaterResolveShoreProfileEvolution(
    float globalDistance,
    float time,
    float flowSpeed,
    float shoreWaveLength,
    float shoreWaveGap,
    float sideSign,
    float transitionLength,
    float sideAsymmetry,
    float evolutionStrength,
    float evolutionDuration,
    float seed)
{
    float strength = saturate(evolutionStrength);
    if (strength <= 0.0001)
    {
        return float2(0.0, 0.0);
    }

    RiverWaterShorePacketState packet = RiverWaterResolveShorePacketState(
        globalDistance,
        time,
        flowSpeed,
        shoreWaveLength,
        shoreWaveGap,
        transitionLength,
        seed);
    float state = RiverWaterResolveShoreEvolutionIdentityState(
        packet.packetIndex,
        time,
        evolutionDuration,
        sideSign,
        seed,
        sideAsymmetry);

    // Evolution belongs to the packet identity, not the gap or period. The
    // packet-local carrier applies these coefficients uniformly for the packet.
    float roundness = clamp(state, -1.0, 1.0) * strength;
    float shoulder = clamp(
        2.0 * state * state - 1.0,
        -1.0,
        1.0) * strength;
    return float2(roundness, shoulder);
}

float RiverWaterResolveShoreOverflowUsageProfile(
    float globalDistance,
    float time,
    float flowSpeed,
    float shoreWaveLength,
    float shoreWaveGap,
    float sideSign,
    float transitionLength,
    float sizeVariation,
    float sideAsymmetry,
    float profileVariation,
    float seed)
{
    RiverWaterShorePacketState packet = RiverWaterResolveShorePacketState(
        globalDistance,
        time,
        flowSpeed,
        shoreWaveLength,
        shoreWaveGap,
        transitionLength,
        seed);
    if (packet.envelope <= 0.0001)
    {
        return 0.0;
    }

    float sizeUsage = RiverWaterResolvePacketSizeValue(
        packet.packetIndex,
        sideSign,
        seed,
        sideAsymmetry,
        sizeVariation,
        197.0,
        0.35,
        1.50);
    float profileUsage = RiverWaterResolvePacketProfileValue(
        packet.packetIndex,
        packet.localT,
        sideSign,
        seed,
        sideAsymmetry,
        profileVariation,
        223.0,
        0.50,
        1.30);
    return saturate(sizeUsage * profileUsage * packet.envelope);
}

void RiverWaterResolveShoreWaveProfiles(
    float globalDistance,
    float time,
    float flowSpeed,
    float shoreWaveLength,
    float shoreWaveGap,
    float sideSign,
    float transitionLength,
    float sizeVariation,
    float sideAsymmetry,
    float profileVariation,
    float seed,
    out float heightProfile,
    out float reachProfile)
{
    RiverWaterShorePacketState packet = RiverWaterResolveShorePacketState(
        globalDistance,
        time,
        flowSpeed,
        shoreWaveLength,
        shoreWaveGap,
        transitionLength,
        seed);
    if (packet.envelope <= 0.0001)
    {
        heightProfile = 0.0;
        reachProfile = 0.0;
        return;
    }

    float heightShape = RiverWaterResolvePacketProfileValue(
        packet.packetIndex,
        packet.localT,
        sideSign,
        seed,
        sideAsymmetry,
        profileVariation,
        11.0,
        0.45,
        1.55);
    float reachShape = RiverWaterResolvePacketProfileValue(
        packet.packetIndex,
        packet.localT,
        sideSign,
        seed,
        sideAsymmetry,
        profileVariation,
        53.0,
        0.35,
        1.15);
    float heightWaveSize = RiverWaterResolvePacketSizeValue(
        packet.packetIndex,
        sideSign,
        seed,
        sideAsymmetry,
        sizeVariation,
        101.0,
        0.60,
        1.40);
    float independentReachWaveSize = RiverWaterResolvePacketSizeValue(
        packet.packetIndex,
        sideSign,
        seed,
        sideAsymmetry,
        sizeVariation,
        149.0,
        0.65,
        1.35);
    float reachWaveSize = lerp(
        heightWaveSize,
        independentReachWaveSize,
        0.55);

    heightProfile = max(0.0, heightShape * heightWaveSize);
    reachProfile = max(0.0, reachShape * reachWaveSize);
}

float RiverWaterEvaluateShorePacketHeightShaped(
    float globalDistance,
    float lateralMetres,
    float time,
    float flowSpeed,
    float waveHeight,
    float shoreWaveLength,
    float shoreWaveGap,
    float steepness,
    float turbulence,
    float seed,
    float2 profileEvolution,
    float transitionLength)
{
    RiverWaterShorePacketState packet = RiverWaterResolveShorePacketState(
        globalDistance,
        time,
        flowSpeed,
        shoreWaveLength,
        shoreWaveGap,
        transitionLength,
        seed);
    if (packet.envelope <= 0.0001 || waveHeight <= 0.0001)
    {
        return 0.0;
    }

    // One packet is one visible positive lapping event. The former full 2PI
    // signed cycle made the negative half of Length appear as extra calm gap.
    // sin^2(PI*t) spans the complete authored Length, stays nonnegative, and
    // reaches both packet boundaries with zero value and zero slope.
    float baseLobe = sin(3.14159265359 * packet.localT);
    baseLobe *= baseLobe;

    float effectiveSteepness = saturate(steepness);
    float roundness = clamp(profileEvolution.x, -1.0, 1.0);
    float shoulder = clamp(profileEvolution.y, -1.0, 1.0);

    // Transition Length now shapes shoulders only inside the authored packet.
    // It cannot change packet support or the authored inter-packet gap.
    float transitionRatio = saturate(
        max(0.0, transitionLength) /
        max(0.0001, packet.packetLength * 0.5));
    float lobeExponent = lerp(1.55, 0.75, transitionRatio);
    lobeExponent *= lerp(1.15, 0.78, effectiveSteepness);
    lobeExponent *= lerp(1.22, 0.82, roundness * 0.5 + 0.5);
    lobeExponent = clamp(lobeExponent, 0.80, 2.20);

    float shapedLobe = pow(baseLobe, lobeExponent);
    shapedLobe = saturate(
        shapedLobe +
        shoulder * 0.30 * shapedLobe * (1.0 - shapedLobe));

    // Turbulence and cross-river variation modulate the positive lobe without
    // moving its boundaries, producing negative values, or creating an internal
    // calm interval that would contaminate the authored Gap control.
    float crossPhase =
        lateralMetres * PS3D_RIVER_TWO_PI /
        max(0.75, packet.packetLength * 1.35);
    float2 noiseCoordinate = float2(
        globalDistance / max(1.0, packet.packetLength * 1.8),
        lateralMetres / max(1.0, packet.packetLength * 0.8));
    float evolvingNoise = RiverWaterValueNoise(
        noiseCoordinate +
        float2(
            -time * flowSpeed / max(1.0, packet.packetLength * 5.0),
            time * 0.035));
    float packetSeedPhase = RiverWaterResolveShoreProfileKnot(
        packet.packetIndex,
        seed,
        307.0) * PS3D_RIVER_TWO_PI;
    float secondary = sin(
        packet.localT * 3.14159265359 * 3.0 -
        crossPhase * 0.42 +
        packetSeedPhase * 0.31 +
        time * 0.21);
    float modulation = 1.0 +
        baseLobe * saturate(turbulence) *
        (0.18 * sin(crossPhase) +
         0.18 * (evolvingNoise * 2.0 - 1.0) +
         0.12 * secondary);
    float crest = saturate(shapedLobe * max(0.10, modulation));
    return crest * max(0.0, waveHeight);
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

// Negative displacement is valid in the river interior but must return to the
// static waterline before the exact visible shoreline. The generated corridor
// reaches static water height at that edge, so retaining a trough there makes
// terrain occlude the water and produces false width loss.
float RiverWaterResolveShoreTroughMask(
    float lateralMetres,
    float visibleHalfWidth,
    float shoreMotionWidth)
{
    float interiorDistance = max(
        0.0,
        max(0.001, visibleHalfWidth) - abs(lateralMetres));
    return smoothstep(
        0.0,
        max(0.001, shoreMotionWidth),
        interiorDistance);
}

float RiverWaterResolvePositiveShoreReach(
    float shoreHeight,
    float shoreAmplitude,
    float authoredReach,
    float reachProfile)
{
    float crest01 = saturate(
        max(0.0, shoreHeight) /
        max(0.0001, shoreAmplitude));
    float crestEnvelope = smoothstep(0.0, 1.0, crest01);
    return saturate(
        saturate(authoredReach) *
        max(0.0, reachProfile) *
        crestEnvelope);
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

float RiverWaterEvaluateMacroHeightShaped(
    float globalDistance,
    float lateralMetres,
    float time,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float steepness,
    float turbulence,
    float seed,
    float2 profileEvolution)
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
    float effectiveSteepness = saturate(steepness);

    if (max(abs(profileEvolution.x), abs(profileEvolution.y)) > 0.0001)
    {
        float shoulderAmount =
            clamp(profileEvolution.y, -1.0, 1.0) * 0.38;
        combined = clamp(
            combined +
            shoulderAmount * combined * (1.0 - abs(combined)),
            -1.0,
            1.0);
        effectiveSteepness = saturate(
            effectiveSteepness +
            clamp(profileEvolution.x, -1.0, 1.0) * 0.45);
    }

    float crest = sign(combined) *
        pow(abs(combined), lerp(1.0, 0.58, effectiveSteepness));
    return crest * max(0.0, waveHeight);
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
    return RiverWaterEvaluateMacroHeightShaped(
        globalDistance,
        lateralMetres,
        time,
        flowSpeed,
        waveHeight,
        waveLength,
        steepness,
        turbulence,
        seed,
        float2(0.0, 0.0));
}

float RiverWaterEvaluateBlendedMacroHeightDetailed(
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
    float shoreWaveSpacingScale,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float2 shoreProfileEvolution,
    float seed,
    out float shoreHeight,
    out float shoreAmplitude,
    out float reachProfile)
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
    float shoreGap =
        waveLength * max(0.0, shoreWaveSpacingScale);
    float sideSign = lateralMetres < 0.0 ? -1.0 : 1.0;
    float heightProfile;
    RiverWaterResolveShoreWaveProfiles(
        globalDistance,
        time,
        flowSpeed,
        shoreLength,
        shoreGap,
        sideSign,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        seed,
        heightProfile,
        reachProfile);

    shoreAmplitude =
        waveHeight * max(0.0, shoreWaveHeightScale) * heightProfile;
    float visible = max(0.001, visibleHalfWidth);
    float shoreEvaluationLateral = clamp(
        lateralMetres,
        -visible,
        visible);
    shoreHeight = RiverWaterEvaluateShorePacketHeightShaped(
        globalDistance,
        shoreEvaluationLateral,
        time,
        flowSpeed,
        shoreAmplitude,
        shoreLength,
        shoreGap,
        steepness,
        turbulence,
        seed,
        shoreProfileEvolution,
        shoreWaveTransitionLength);
    float shoreBlend = RiverWaterResolveShoreBlend(
        lateralMetres,
        visibleHalfWidth,
        shoreMotionWidth);
    return lerp(baseHeight, shoreHeight, shoreBlend);
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
    float shoreWaveSpacingScale,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float shoreWaveProfileEvolutionStrength,
    float shoreWaveProfileEvolutionDuration,
    float seed)
{
    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float shoreGap =
        waveLength * max(0.0, shoreWaveSpacingScale);
    float sideSign = lateralMetres < 0.0 ? -1.0 : 1.0;
    float2 shoreProfileEvolution =
        RiverWaterResolveShoreProfileEvolution(
            globalDistance,
            time,
            flowSpeed,
            shoreLength,
            shoreGap,
            sideSign,
            shoreWaveTransitionLength,
            shoreWaveSideAsymmetry,
            shoreWaveProfileEvolutionStrength,
            shoreWaveProfileEvolutionDuration,
            seed);

    float shoreHeight;
    float shoreAmplitude;
    float reachProfile;
    return RiverWaterEvaluateBlendedMacroHeightDetailed(
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
        shoreWaveSpacingScale,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
        seed,
        shoreHeight,
        shoreAmplitude,
        reachProfile);
}

float RiverWaterEvaluateSurfaceHeightCore(
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
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float2 shoreProfileEvolution,
    float liquidFactor,
    float seed,
    out float bankMask,
    out float shoreHeight,
    out float resolvedReach)
{
    float shoreAmplitude;
    float reachProfile;
    float blendedHeight = RiverWaterEvaluateBlendedMacroHeightDetailed(
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
        shoreWaveSpacingScale,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
        seed,
        shoreHeight,
        shoreAmplitude,
        reachProfile);

    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float shoreGap =
        waveLength * max(0.0, shoreWaveSpacingScale);
    resolvedReach = RiverWaterResolvePositiveShoreReach(
        shoreHeight,
        shoreAmplitude,
        shoreWaveReach,
        reachProfile);
    float positiveBankMask = RiverWaterResolveMotionBankMask(
        lateralMetres,
        visibleHalfWidth,
        surfaceHalfWidth,
        shoreMotion,
        shoreMotionWidth,
        resolvedReach);
    float troughMask = RiverWaterResolveShoreTroughMask(
        lateralMetres,
        visibleHalfWidth,
        shoreMotionWidth);

    // The output mask is the stable visible-water/detail authority. It must
    // not change when the signed macro height crosses zero because fragment
    // detail normals, current accents, and refraction all consume it.
    bankMask = positiveBankMask;

    // Trough restoration is displacement-only. Positive crests retain the
    // existing shoreline/overflow mask while negative displacement returns to
    // the static waterline through the dedicated trough mask.
    float positiveHeight = max(0.0, blendedHeight) * positiveBankMask;
    float negativeHeight = min(0.0, blendedHeight) * troughMask;
    return (positiveHeight + negativeHeight) * saturate(liquidFactor);
}

float RiverWaterEvaluateSurfaceHeightWithEvolution(
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
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float2 shoreProfileEvolution,
    float liquidFactor,
    float seed,
    out float bankMask)
{
    float shoreHeight;
    float resolvedReach;
    return RiverWaterEvaluateSurfaceHeightCore(
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
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
        liquidFactor,
        seed,
        bankMask,
        shoreHeight,
        resolvedReach);
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
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float shoreWaveProfileEvolutionStrength,
    float shoreWaveProfileEvolutionDuration,
    float liquidFactor,
    float seed,
    out float bankMask)
{
    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float shoreGap =
        waveLength * max(0.0, shoreWaveSpacingScale);
    float sideSign = lateralMetres < 0.0 ? -1.0 : 1.0;
    float2 shoreProfileEvolution =
        RiverWaterResolveShoreProfileEvolution(
            globalDistance,
            time,
            flowSpeed,
            shoreLength,
            shoreGap,
            sideSign,
            shoreWaveTransitionLength,
            shoreWaveSideAsymmetry,
            shoreWaveProfileEvolutionStrength,
            shoreWaveProfileEvolutionDuration,
            seed);
    return RiverWaterEvaluateSurfaceHeightWithEvolution(
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
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
        liquidFactor,
        seed,
        bankMask);
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

// Resolves the monotonic contact between the positive hidden-band water
// profile and the generated HiddenCover bank profile. This uses only the
// already-evaluated shore height and reach; it performs no additional wave,
// noise, texture, or full-surface evaluation.
float RiverWaterResolveRenderedShoreHalfWidth(
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float shoreHeight,
    float resolvedReach,
    float shoreMotion,
    float bankCover,
    float liquidFactor,
    float overflowUsageProfile)
{
    float visible = max(0.001, visibleHalfWidth);
    float surface = max(visible, surfaceHalfWidth);
    float reach = saturate(resolvedReach);
    float crestAtShore =
        max(0.0, shoreHeight) *
        saturate(shoreMotion) *
        saturate(liquidFactor);

    if (surface <= visible + 0.0001 ||
        reach <= 0.0001 ||
        crestAtShore <= 0.0001)
    {
        return visible;
    }

    float lowerT = 0.0;
    float upperT = reach;
    float safeReach = max(0.0001, reach);

    [unroll]
    for (int iteration = 0; iteration < 10; iteration++)
    {
        float middleT = (lowerT + upperT) * 0.5;
        float hiddenRemaining = saturate(
            (reach - middleT) / safeReach);
        float waterOffset = crestAtShore * smoothstep(
            0.0,
            1.0,
            hiddenRemaining);
        float coverOffset = max(0.0, bankCover) * smoothstep(
            0.0,
            1.0,
            middleT);

        if (waterOffset > coverOffset)
        {
            lowerT = middleT;
        }
        else
        {
            upperT = middleT;
        }
    }

    float variedContactT = saturate(lowerT) *
        saturate(overflowUsageProfile);
    return lerp(visible, surface, variedContactT);
}

float RiverWaterEvaluateSurfaceHeightAndShorelineWithEvolution(
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
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float2 shoreProfileEvolution,
    float bankCover,
    float liquidFactor,
    float seed,
    out float bankMask,
    out float currentShoreHalfWidth)
{
    float shoreHeight;
    float resolvedReach;
    float surfaceHeight = RiverWaterEvaluateSurfaceHeightCore(
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
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
        liquidFactor,
        seed,
        bankMask,
        shoreHeight,
        resolvedReach);

    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float shoreGap =
        waveLength * max(0.0, shoreWaveSpacingScale);
    float sideSign = lateralMetres < 0.0 ? -1.0 : 1.0;
    float overflowUsageProfile =
        RiverWaterResolveShoreOverflowUsageProfile(
            globalDistance,
            time,
            flowSpeed,
            shoreLength,
            shoreGap,
            sideSign,
            shoreWaveTransitionLength,
            shoreWaveSizeVariation,
            shoreWaveSideAsymmetry,
            shoreWaveProfileVariation,
            seed);
    currentShoreHalfWidth = RiverWaterResolveRenderedShoreHalfWidth(
        visibleHalfWidth,
        surfaceHalfWidth,
        shoreHeight,
        resolvedReach,
        shoreMotion,
        bankCover,
        liquidFactor,
        overflowUsageProfile);
    return surfaceHeight;
}

float RiverWaterEvaluateSurfaceHeightAndShoreline(
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
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float shoreWaveProfileEvolutionStrength,
    float shoreWaveProfileEvolutionDuration,
    float bankCover,
    float liquidFactor,
    float seed,
    out float bankMask,
    out float currentShoreHalfWidth)
{
    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float shoreGap =
        waveLength * max(0.0, shoreWaveSpacingScale);
    float sideSign = lateralMetres < 0.0 ? -1.0 : 1.0;
    float2 shoreProfileEvolution =
        RiverWaterResolveShoreProfileEvolution(
            globalDistance,
            time,
            flowSpeed,
            shoreLength,
            shoreGap,
            sideSign,
            shoreWaveTransitionLength,
            shoreWaveSideAsymmetry,
            shoreWaveProfileEvolutionStrength,
            shoreWaveProfileEvolutionDuration,
            seed);
    return RiverWaterEvaluateSurfaceHeightAndShorelineWithEvolution(
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
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
        bankCover,
        liquidFactor,
        seed,
        bankMask,
        currentShoreHalfWidth);
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
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float2 shoreProfileEvolution,
    float liquidFactor,
    float seed)
{
    float bankMask;
    return RiverWaterEvaluateSurfaceHeightWithEvolution(
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
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
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
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float2 shoreProfileEvolution,
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
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
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
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float shoreWaveProfileEvolutionStrength,
    float shoreWaveProfileEvolutionDuration,
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

    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float shoreGap =
        waveLength * max(0.0, shoreWaveSpacingScale);
    float2 shoreProfileEvolution =
        RiverWaterResolveShoreProfileEvolution(
            globalDistance,
            time,
            flowSpeed,
            shoreLength,
            shoreGap,
            sideSign,
            shoreWaveTransitionLength,
            shoreWaveSideAsymmetry,
            shoreWaveProfileEvolutionStrength,
            shoreWaveProfileEvolutionDuration,
            seed);
    float farthestWetT = 0.0;

    // This search calls the complete shared shore-wave evaluator. Forcing all
    // samples to unroll can exceed D3D11 compiler limits as that evaluator grows.
    // Shader Model 5 compute supports this bounded runtime loop directly.
    // Profile evolution is resolved once per side above and reused by every
    // coarse and refinement sample.
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
                shoreWaveSpacingScale,
                shoreWaveReach,
                shoreWaveTransitionLength,
                shoreWaveSizeVariation,
                shoreWaveSideAsymmetry,
                shoreWaveProfileVariation,
                shoreProfileEvolution,
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
                shoreWaveSpacingScale,
                shoreWaveReach,
                shoreWaveTransitionLength,
                shoreWaveSizeVariation,
                shoreWaveSideAsymmetry,
                shoreWaveProfileVariation,
                shoreProfileEvolution,
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

    float overflowUsageProfile =
        RiverWaterResolveShoreOverflowUsageProfile(
            globalDistance,
            time,
            flowSpeed,
            shoreLength,
            shoreGap,
            sideSign,
            shoreWaveTransitionLength,
            shoreWaveSizeVariation,
            shoreWaveSideAsymmetry,
            shoreWaveProfileVariation,
            seed);
    float variedContactT = saturate(lowerT) *
        saturate(overflowUsageProfile);
    return lerp(visible, surface, variedContactT);
}

#endif
