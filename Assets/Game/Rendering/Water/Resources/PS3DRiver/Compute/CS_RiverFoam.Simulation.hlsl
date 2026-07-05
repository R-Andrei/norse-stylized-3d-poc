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

float2 FoamResolveMacroMaterialDeformationCells(
    int2 coordinate,
    float2 physicalPosition,
    float materialPattern,
    float edgeExposure,
    float support,
    float negative,
    float flowCells)
{
    float2 cellPosition = float2(coordinate);
    float patternSeed =
        materialPattern * 997.31 +
        _FoamSeed * 0.043 +
        physicalPosition.x * 0.013 +
        physicalPosition.y * 0.071;

    // Low-frequency, river-space intrinsic deformation. This is not river
    // disturbance coupling yet; it is the material's own slow film wobble so
    // stored Presence can bend and stretch instead of remaining a rigid stamp.
    float slowPhase = _FoamTime * 0.115;
    float2 broadDomain = cellPosition / float2(18.0, 11.0) +
        float2(slowPhase, -slowPhase * 0.73);
    float2 detailDomain = cellPosition / float2(7.0, 5.0) +
        float2(-slowPhase * 1.37, slowPhase * 0.91);

    float broadLateral = FoamSignedMorphNoise(
        broadDomain,
        patternSeed + 31.0);
    float broadLongitudinal = FoamSignedMorphNoise(
        broadDomain + float2(12.7, 4.3),
        patternSeed + 79.0);
    float detailLateral = FoamSignedMorphNoise(
        detailDomain + float2(3.1, 17.4),
        patternSeed + 137.0);

    float mobility =
        lerp(0.62, 1.42, edgeExposure) *
        lerp(1.0, 1.34, negative) *
        lerp(1.0, 0.78, support);

    float lateralCells =
        (broadLateral * 1.18 + detailLateral * 0.34) * mobility;
    float longitudinalCells =
        broadLongitudinal * lerp(0.28, 1.04, mobility) +
        flowCells * 0.42;

    return float2(longitudinalCells, lateralCells);
}

float4 FoamApplyPersistentMaterialMorph(
    float4 currentPacked,
    int2 coordinate,
    float2 physicalPosition,
    float2 physicalCellSpacing,
    FoamMaterialTopologySample materialTopology,
    float validFluid)
{
    if (_FoamDebugAbsoluteLifeProbeActive > 0.5 ||
        _FoamDeltaTime <= 0.00001)
    {
        return currentPacked;
    }

    FoamMaterialState currentState = FoamDecodeMaterialState(currentPacked);
    float flowSign = _FoamFlowDirection >= 0.0 ? 1.0 : -1.0;
    float longitudinalCellSize = max(0.01, physicalCellSpacing.x);
    float flowCells = saturate(
        abs(_FoamFlowSpeed) * _FoamDeltaTime / longitudinalCellSize);

    float support = FoamShapeAgingInfluence(
        FoamCombinedMaterialSupport(materialTopology));
    float negative = FoamShapeAgingInfluence(
        materialTopology.negativeAgingPressure);
    float edgeExposure = FoamResolveNeighbourExposure(
        coordinate,
        currentState.presence);

    float2 macroCells = FoamResolveMacroMaterialDeformationCells(
        coordinate,
        physicalPosition,
        currentState.materialPattern,
        edgeExposure,
        support,
        negative,
        flowCells);

    float2 currentPixel = float2(coordinate);

    // Backtrace from several deliberately different local material velocities.
    // The offsets are large enough to deform broad silhouettes over seconds,
    // while the weighted gather keeps the event one persistent material body
    // rather than spawning new hidden Foam.
    float forwardCells = clamp(
        0.42 + flowCells * 1.15 + macroCells.x,
        -0.65,
        2.25);
    float lateralCells = clamp(macroCells.y, -1.85, 1.85);
    float2 primaryPixel = currentPixel -
        float2(flowSign * forwardCells, lateralCells);

    float lagCells = clamp(
        -0.48 + macroCells.x * 0.58,
        -1.35,
        0.95);
    float2 lagPixel = currentPixel -
        float2(flowSign * lagCells,
            -lateralCells * 0.62 + macroCells.x * 0.28);

    float sideDirection = macroCells.y >= 0.0 ? 1.0 : -1.0;
    float2 sidePixel = currentPixel -
        float2(flowSign * (forwardCells * 0.28 - macroCells.x * 0.18),
            lateralCells + sideDirection * lerp(0.35, 1.10, edgeExposure));

    float4 primaryPacked = FoamSamplePackedMaterialBilinear(primaryPixel);
    float4 lagPacked = FoamSamplePackedMaterialBilinear(lagPixel);
    float4 sidePacked = FoamSamplePackedMaterialBilinear(sidePixel);

    float primaryPresence = FoamDecodeMaterialState(primaryPacked).presence;
    float lagPresence = FoamDecodeMaterialState(lagPacked).presence;
    float sidePresence = FoamDecodeMaterialState(sidePacked).presence;
    float nearbyPresence = max(
        currentState.presence,
        max(primaryPresence, max(lagPresence, sidePresence)));
    if (nearbyPresence <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxxx;
    }

    float activity = smoothstep(0.01, 0.18, nearbyPresence);
    float materialAge = saturate(1.0 - currentState.remainingLife);
    float mobility =
        lerp(0.78, 1.34, edgeExposure) *
        lerp(0.92, 1.22, materialAge) *
        lerp(1.0, 1.32, negative) *
        lerp(1.0, 0.82, support);

    // 5.5 was intentionally conservative and only roughened edges. This pass
    // uses a much stronger stored-state deformation weight so Material Presence
    // itself visibly bends/stretches over time. Erosion remains separate below.
    float morphWeight = saturate(_FoamDeltaTime * 4.35 * activity * mobility);
    float currentWeight = 1.0 - morphWeight;
    float primaryWeight = morphWeight * 0.52;
    float lagWeight = morphWeight * 0.30;
    float sideWeight = morphWeight * 0.18;

    float4 mixedPacked =
        currentPacked * currentWeight +
        primaryPacked * primaryWeight +
        lagPacked * lagWeight +
        sidePacked * sideWeight;

    FoamMaterialState mixedState = FoamDecodeMaterialState(
        FoamClampPackedMaterialState(mixedPacked));

    // Lifecycle authority repair: morphing may redistribute and locally extend
    // stored material, but it may not become an independent death path.
    // Existing material cells keep at least their current Presence until the
    // Remaining Life equation removes them. This keeps lifespan controlled only
    // by Neutral Lifetime, support, and negative-aging topology.
    float preservedExistingPresence = currentState.presence > FoamMaterialStateEpsilon
        ? currentState.presence
        : 0.0;
    mixedState.presence = max(mixedState.presence, preservedExistingPresence);

    // Prevent deformation gather from turning interpolation haze into permanent
    // full-strength material while still allowing nearby material to create
    // visible local widening/reconfiguration.
    mixedState.presence = min(
        mixedState.presence,
        max(nearbyPresence, currentState.presence) + 0.08);

    return FoamClipPackedToValidFluid(
        FoamEncodeMaterialState(mixedState),
        validFluid);
}
