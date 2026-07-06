static const float FoamMaterialStateEpsilon = 0.0001;

struct FoamMaterialState
{
    float presence;
    float remainingLife;
    float materialPattern;
};

FoamMaterialState FoamDecodeMaterialState(float4 packed)
{
    FoamMaterialState state;
    state.presence = saturate(packed.x);
    if (state.presence > FoamMaterialStateEpsilon)
    {
        state.remainingLife = saturate(packed.y / state.presence);
        state.materialPattern = saturate(packed.z / state.presence);
    }
    else
    {
        state.remainingLife = 0.0;
        state.materialPattern = 0.0;
    }

    return state;
}

float4 FoamEncodeMaterialState(FoamMaterialState state)
{
    state.presence = saturate(state.presence);
    state.remainingLife = saturate(state.remainingLife);
    state.materialPattern = saturate(state.materialPattern);
    if (state.presence <= FoamMaterialStateEpsilon ||
        state.remainingLife <= 0.0)
    {
        return 0.0.xxxx;
    }

    return float4(
        state.presence,
        state.presence * state.remainingLife,
        state.presence * state.materialPattern,
        0.0);
}

float4 FoamClampPackedMaterialState(float4 packed)
{
    float presence = saturate(packed.x);
    float lifeMoment = clamp(packed.y, 0.0, presence);
    float patternMoment = clamp(packed.z, 0.0, presence);
    if (presence <= FoamMaterialStateEpsilon || lifeMoment <= 0.0)
    {
        return 0.0.xxxx;
    }

    return float4(presence, lifeMoment, patternMoment, 0.0);
}

float4 FoamMergeBornPresence(float4 existingPacked, float4 sourcePacked)
{
    FoamMaterialState existing = FoamDecodeMaterialState(existingPacked);
    FoamMaterialState source = FoamDecodeMaterialState(sourcePacked);
    float addedPresence = max(
        0.0,
        source.presence - existing.presence);
    float combinedPresence = max(
        existing.presence,
        source.presence);
    if (combinedPresence <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxxx;
    }

    FoamMaterialState combined;
    combined.presence = combinedPresence;
    combined.remainingLife = saturate(
        (existing.presence * existing.remainingLife +
         addedPresence * source.remainingLife) /
        combinedPresence);
    combined.materialPattern = saturate(
        (existing.presence * existing.materialPattern +
         addedPresence * source.materialPattern) /
        combinedPresence);
    return FoamEncodeMaterialState(combined);
}

float4 FoamClipPackedToValidFluid(float4 packed, float validFluid)
{
    FoamMaterialState state = FoamDecodeMaterialState(packed);
    state.presence = min(state.presence, saturate(validFluid));
    return FoamEncodeMaterialState(state);
}


float FoamLoadMaterialPresence(int2 coordinate)
{
    if (coordinate.x < 0 || coordinate.x >= _FoamDimensions.x ||
        coordinate.y < 0 || coordinate.y >= _FoamDimensions.y ||
        !IsFoamColumnInsideSimulation(coordinate.x))
    {
        return 0.0;
    }

    float4 packed = FoamClipPackedToValidFluid(
        _FoamStateRead.Load(int3(coordinate, 0)),
        FoamValidFluidAt(coordinate));
    return FoamDecodeMaterialState(packed).presence;
}

float4 FoamSamplePackedMaterialBilinear(float2 pixelCoordinate)
{
    return FoamClampPackedMaterialState(SampleStateBilinear(pixelCoordinate));
}

float FoamSignedMorphNoise(float2 position, float seed)
{
    return FoamSourceFillValueNoise(position, seed) * 2.0 - 1.0;
}

float FoamResolveNeighbourExposure(int2 coordinate, float currentPresence)
{
    if (currentPresence <= FoamMaterialStateEpsilon)
    {
        return 0.0;
    }

    float left = FoamLoadMaterialPresence(coordinate + int2(-1, 0));
    float right = FoamLoadMaterialPresence(coordinate + int2(1, 0));
    float down = FoamLoadMaterialPresence(coordinate + int2(0, -1));
    float up = FoamLoadMaterialPresence(coordinate + int2(0, 1));
    float neighbourAverage = (left + right + down + up) * 0.25;
    float emptyNeighbourRatio =
        (1.0 - step(FoamMaterialStateEpsilon, left) +
         1.0 - step(FoamMaterialStateEpsilon, right) +
         1.0 - step(FoamMaterialStateEpsilon, down) +
         1.0 - step(FoamMaterialStateEpsilon, up)) * 0.25;

    float exposedByGradient = saturate(
        (currentPresence - neighbourAverage) * 1.45);
    return saturate(exposedByGradient + emptyNeighbourRatio * 0.55);
}

struct FoamSurfaceMorphInfluence
{
    float agitation;
    float downstreamBias;
    float lateralBias;
    float edgeBoost;
    float strength;
};

FoamSurfaceMorphInfluence FoamResolveSurfaceMorphInfluence(
    float2 surfaceUv,
    FoamMaterialTopologySample materialTopology,
    float edgeExposure)
{
    FoamSurfaceMorphInfluence influence;
    influence.agitation = 0.0;
    influence.downstreamBias = 0.0;
    influence.lateralBias = 0.0;
    influence.edgeBoost = 0.0;
    influence.strength = clamp(_FoamSurfaceMorphStrength, 0.0, 5.0);

    if (_FoamDisturbanceEnabled <= 0.5 || influence.strength <= 0.0001)
    {
        return influence;
    }

    float4 ripple = SampleRippleBilinear(surfaceUv);
    float4 wake = SampleWakeBilinear(surfaceUv);
    float4 staticWake = SampleStaticWakeBilinear(surfaceUv);
    float4 staticPressure = SampleStaticPressureBilinear(surfaceUv);

    float2 rippleGradient = ripple.ba;
    float2 wakeGradient = wake.ba;
    float2 pressureGradient = staticPressure.gb;

    // 5.7b proved the coupling path works, but the response was still too
    // timid: a value of 5 read more like a merely strong authored effect than
    // an overdriven stress test. This curve treats 1 as the normal readable
    // authored response. It lifts low/mid disturbance values, favours wake/lee
    // and pressure gradients, then clamps the result so debug-strength fields
    // cannot fling stored material across the simulation or paint new Foam.
    float rippleAgitation = saturate(
        abs(ripple.r) * 3.20 +
        abs(ripple.g) * 0.56 +
        length(rippleGradient) * 0.46);
    float wakeAgitation = saturate(
        wake.r * 0.78 +
        length(wakeGradient) * 0.72 +
        wake.g * 0.28);
    float pressureAgitation = saturate(
        abs(staticPressure.r) * 1.70 +
        length(pressureGradient) * 0.56);
    float leeAgitation = saturate(
        staticWake.g * 1.35 +
        staticWake.r * 0.34 +
        staticWake.b * 0.12);

    float topologyContact = saturate(
        materialTopology.pressureSupport * 0.34 +
        materialTopology.leeSupport * 0.38 +
        materialTopology.shoreSupport * 0.14);

    float rawAgitation = saturate(
        rippleAgitation * 0.36 +
        wakeAgitation * 0.42 +
        pressureAgitation * 0.34 +
        leeAgitation * 0.46 +
        topologyContact * 0.10);

    float activeStrength = saturate(influence.strength);
    float overdrive = max(0.0, influence.strength - 1.0);
    float overdrive01 = saturate(overdrive * 0.50);
    float midLift = 1.0 - (1.0 - rawAgitation) * (1.0 - rawAgitation);
    float perceptualAgitation = sqrt(max(0.0, rawAgitation));
    float readableAgitation = lerp(
        midLift,
        perceptualAgitation,
        0.42 + overdrive01 * 0.20);
    influence.agitation = saturate(
        readableAgitation * activeStrength *
        (1.0 + overdrive * 0.40));

    // Surface gradients bias the already area-balanced wobble, not net
    // transport. Low/mid gradients are shaped upward so Material Presence shows
    // visible edge motion near wakes, lee depressions, and pressure ridges at
    // strength 1.0, while the final clamps keep overdrive values bounded.
    float biasGain = activeStrength * (1.0 + overdrive * 0.34);
    float2 combinedGradient = clamp(
        (rippleGradient * 0.40 +
         wakeGradient * 0.72 +
         pressureGradient * 0.58) * biasGain,
        float2(-1.0, -1.0),
        float2(1.0, 1.0));

    float2 shapedGradient = sign(combinedGradient) *
        sqrt(abs(combinedGradient));
    combinedGradient = lerp(
        combinedGradient,
        shapedGradient,
        0.46 + overdrive01 * 0.20);

    influence.downstreamBias = clamp(combinedGradient.x, -0.96, 0.96);
    influence.lateralBias = clamp(combinedGradient.y, -1.0, 1.0);
    float edgeFocus = smoothstep(0.04, 0.72, edgeExposure);
    influence.edgeBoost = saturate(
        influence.agitation * lerp(0.54, 1.12, edgeFocus) *
        (1.0 + overdrive * 0.18));
    return influence;
}

float2 FoamResolveAreaBalancedWobbleCells(
    int2 coordinate,
    float2 physicalPosition,
    float materialPattern,
    float edgeExposure,
    float support,
    float negative,
    FoamSurfaceMorphInfluence surfaceInfluence)
{
    float2 cellPosition = float2(coordinate);
    float patternSeed =
        materialPattern * 997.31 +
        _FoamSeed * 0.043 +
        physicalPosition.x * 0.013 +
        physicalPosition.y * 0.071;

    // Intrinsic material wobble only. Phase transport already supplies net
    // downstream travel, so this field must be approximately zero-mean: it
    // bends, expands, and compresses the stored material back and forth instead
    // of continually pushing the silhouette outward.
    float slowPhase = _FoamTime * 0.72 + patternSeed * 0.017;
    float counterPhase = _FoamTime * 0.43 + patternSeed * 0.031;
    float2 broadDomain = cellPosition / float2(21.0, 13.0);
    float2 detailDomain = cellPosition / float2(9.0, 6.0);

    float broadA = FoamSignedMorphNoise(
        broadDomain + float2(slowPhase, -slowPhase * 0.61),
        patternSeed + 31.0);
    float broadB = FoamSignedMorphNoise(
        broadDomain + float2(-counterPhase * 0.83, counterPhase),
        patternSeed + 79.0);
    float detail = FoamSignedMorphNoise(
        detailDomain + float2(counterPhase * 1.17, -slowPhase * 0.94),
        patternSeed + 137.0);

    float waveA = sin(
        slowPhase +
        cellPosition.x * 0.073 +
        cellPosition.y * 0.119 +
        materialPattern * 6.28318);
    float waveB = cos(
        counterPhase +
        cellPosition.x * 0.052 -
        cellPosition.y * 0.087 +
        materialPattern * 4.71);

    float surfaceAgitation = surfaceInfluence.agitation;
    float surfaceCalibration = saturate(
        surfaceAgitation * (0.62 + surfaceInfluence.strength * 0.12));
    float mobility =
        lerp(0.78, 1.55, edgeExposure) *
        lerp(1.0, 1.28, negative) *
        lerp(1.0, 0.76, support) *
        lerp(1.0, lerp(1.54, 2.18, surfaceCalibration),
            surfaceAgitation);

    // The two axes use different phase mixtures so a strip alternates between
    // bowing, relaxing, and bowing the other way. Surface data does not replace
    // this intrinsic zero-mean wobble; it makes disturbed edges more mobile and
    // pushes the balanced sample direction toward local ripple/wake/pressure
    // slopes. Strength 1 is now intentionally readable instead of merely subtle.
    float lateralCells =
        (broadA * 0.82 + waveA * 0.66 + detail * 0.22) * mobility +
        surfaceInfluence.lateralBias *
        (0.26 + surfaceAgitation * lerp(0.78, 1.32, surfaceCalibration));
    float longitudinalCells =
        (broadB * 0.58 - waveB * 0.42 + broadA * 0.18) * mobility +
        surfaceInfluence.downstreamBias *
        (0.20 + surfaceAgitation * lerp(0.58, 1.04, surfaceCalibration));

    return float2(
        clamp(longitudinalCells, -lerp(1.70, 3.10, surfaceCalibration),
            lerp(1.70, 3.10, surfaceCalibration)),
        clamp(lateralCells, -lerp(2.45, 4.05, surfaceCalibration),
            lerp(2.45, 4.05, surfaceCalibration)));
}

float4 FoamApplyPersistentMaterialMorph(
    float4 currentPacked,
    int2 coordinate,
    float2 motionSampleCoordinate,
    float2 physicalPosition,
    float2 physicalCellSpacing,
    float2 surfaceUv,
    FoamMaterialTopologySample materialTopology,
    float validFluid)
{
    if (_FoamDebugAbsoluteLifeProbeActive > 0.5 ||
        _FoamDeltaTime <= 0.00001)
    {
        return currentPacked;
    }

    FoamMaterialState currentState = FoamDecodeMaterialState(currentPacked);
    float support = FoamShapeAgingInfluence(
        FoamCombinedMaterialSupport(materialTopology));
    float negative = FoamShapeAgingInfluence(
        materialTopology.negativeAgingPressure);
    float edgeExposure = FoamResolveNeighbourExposure(
        coordinate,
        currentState.presence);
    FoamSurfaceMorphInfluence surfaceInfluence =
        FoamResolveSurfaceMorphInfluence(
            surfaceUv,
            materialTopology,
            edgeExposure);
    FoamMotionFieldSample motionField = FoamResolveMotionFieldSample(
        motionSampleCoordinate,
        validFluid);

    float2 morphCells = FoamResolveAreaBalancedWobbleCells(
        coordinate,
        physicalPosition,
        currentState.materialPattern,
        edgeExposure,
        support,
        negative,
        surfaceInfluence);

    float2 currentPixel = float2(coordinate);

    // 4.11C.5.9: macro lateral body transport now comes from the explicit
    // dense Foam Motion Field. Downstream transport remains owned by phase
    // transport; this pass contributes lateral stored-material motion only.
    float2 macroBase = currentPixel - float2(0.0, motionField.lateralCells);
    float2 mesoBase = macroBase;
    float2 edgeOffset = 0.0.xx;

    float2 primaryOffset = morphCells;
    float counterBalance = lerp(
        0.60,
        0.78 + surfaceInfluence.edgeBoost * 0.06,
        edgeExposure);
    float2 counterOffset = -morphCells * counterBalance;
    float crossScale = lerp(0.92, 1.08, surfaceInfluence.agitation);
    float2 crossOffset = float2(
        -morphCells.y * 0.22 * crossScale,
        morphCells.x * 0.34 * crossScale);

    float4 advectedBasePacked = FoamSamplePackedMaterialBilinear(
        macroBase);
    float4 primaryPacked = FoamSamplePackedMaterialBilinear(
        mesoBase - primaryOffset - edgeOffset);
    float4 counterPacked = FoamSamplePackedMaterialBilinear(
        mesoBase - counterOffset + edgeOffset * 0.42);
    float4 crossPacked = FoamSamplePackedMaterialBilinear(
        mesoBase - crossOffset - float2(edgeOffset.y * 0.22, -edgeOffset.x * 0.16));

    float advectedBasePresence = FoamDecodeMaterialState(advectedBasePacked).presence;
    float primaryPresence = FoamDecodeMaterialState(primaryPacked).presence;
    float counterPresence = FoamDecodeMaterialState(counterPacked).presence;
    float crossPresence = FoamDecodeMaterialState(crossPacked).presence;
    float nearbyPresence = max(
        advectedBasePresence,
        max(primaryPresence, max(counterPresence, crossPresence)));
    if (nearbyPresence <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxxx;
    }

    float activity = smoothstep(0.01, 0.16, nearbyPresence);
    float materialAge = saturate(1.0 - currentState.remainingLife);
    float surfaceCalibration = saturate(
        surfaceInfluence.agitation *
        (0.58 + surfaceInfluence.edgeBoost * 0.36));
    float mobility =
        lerp(0.92, 1.04, edgeExposure) *
        lerp(0.96, 1.10, materialAge) *
        lerp(1.0, 1.10, negative) *
        lerp(1.0, 0.88, support) *
        lerp(1.0, lerp(1.20, 1.50, surfaceCalibration),
            surfaceInfluence.agitation) *
        lerp(1.0, lerp(1.04, 1.16, surfaceCalibration),
            surfaceInfluence.edgeBoost);

    // Area-balanced wobble remains meso deformation around the field-advected
    // body. It remains normalized and lifecycle-neutral: this pass still
    // contains no explicit Presence erosion or Remaining Life adjustment.
    float surfaceRate = lerp(
        5.35,
        6.75,
        surfaceInfluence.agitation *
        (0.45 + surfaceCalibration * 0.55));
    float morphWeight = saturate(surfaceRate * _FoamDeltaTime * activity * mobility);

    // Material transport should be source-sampled, not union-preserved.
    // The previous current/advected blend left source cells behind while also
    // filling the destination, which read as stretching/growth instead of
    // actual lateral movement. With zero lateral motion, macroBase equals the
    // current pixel, so advectedBasePacked naturally preserves stationary foam.
    float4 basePacked = advectedBasePacked;

    float baseWeight = 1.0 - morphWeight * 0.66;
    float primaryWeight = morphWeight * 0.25;
    float counterWeight = morphWeight * 0.23;
    float crossWeight = morphWeight * 0.13;
    float totalWeight = max(
        0.0001,
        baseWeight + primaryWeight + counterWeight + crossWeight);

    float4 mixedPacked =
        (basePacked * baseWeight +
         primaryPacked * primaryWeight +
         counterPacked * counterWeight +
         crossPacked * crossWeight) /
        totalWeight;

    FoamMaterialState mixedState = FoamDecodeMaterialState(
        FoamClampPackedMaterialState(mixedPacked));

    // Do not allow interpolation haze to accumulate into new full material.
    // Current-cell material no longer gets a free preservation vote during
    // lateral transport; output is sourced from the field-advected samples.
    // Lifespan remains governed by Remaining Life because this pass contains no
    // explicit Presence erosion.
    mixedState.presence = min(
        mixedState.presence,
        nearbyPresence + 0.025);

    return FoamClipPackedToValidFluid(
        FoamEncodeMaterialState(mixedState),
        validFluid);
}

