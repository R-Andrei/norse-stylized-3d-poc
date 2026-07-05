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

float2 FoamResolveAreaBalancedWobbleCells(
    int2 coordinate,
    float2 physicalPosition,
    float materialPattern,
    float edgeExposure,
    float support,
    float negative)
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

    float mobility =
        lerp(0.78, 1.55, edgeExposure) *
        lerp(1.0, 1.28, negative) *
        lerp(1.0, 0.76, support);

    // The two axes use different phase mixtures so a strip alternates between
    // bowing, relaxing, and bowing the other way. These offsets are sampled as
    // paired/opposed gathers below, which keeps average area stable.
    float lateralCells =
        (broadA * 0.82 + waveA * 0.66 + detail * 0.22) * mobility;
    float longitudinalCells =
        (broadB * 0.58 - waveB * 0.42 + broadA * 0.18) * mobility;

    return float2(
        clamp(longitudinalCells, -1.45, 1.45),
        clamp(lateralCells, -2.10, 2.10));
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
    float support = FoamShapeAgingInfluence(
        FoamCombinedMaterialSupport(materialTopology));
    float negative = FoamShapeAgingInfluence(
        materialTopology.negativeAgingPressure);
    float edgeExposure = FoamResolveNeighbourExposure(
        coordinate,
        currentState.presence);

    float2 macroCells = FoamResolveAreaBalancedWobbleCells(
        coordinate,
        physicalPosition,
        currentState.materialPattern,
        edgeExposure,
        support,
        negative);

    float2 currentPixel = float2(coordinate);
    float2 primaryOffset = macroCells;
    float2 counterOffset = -macroCells * lerp(0.58, 0.82, edgeExposure);
    float2 crossOffset = float2(
        -macroCells.y * 0.26,
        macroCells.x * 0.42);

    float4 primaryPacked = FoamSamplePackedMaterialBilinear(
        currentPixel - primaryOffset);
    float4 counterPacked = FoamSamplePackedMaterialBilinear(
        currentPixel - counterOffset);
    float4 crossPacked = FoamSamplePackedMaterialBilinear(
        currentPixel - crossOffset);

    float primaryPresence = FoamDecodeMaterialState(primaryPacked).presence;
    float counterPresence = FoamDecodeMaterialState(counterPacked).presence;
    float crossPresence = FoamDecodeMaterialState(crossPacked).presence;
    float nearbyPresence = max(
        currentState.presence,
        max(primaryPresence, max(counterPresence, crossPresence)));
    if (nearbyPresence <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxxx;
    }

    float activity = smoothstep(0.01, 0.16, nearbyPresence);
    float materialAge = saturate(1.0 - currentState.remainingLife);
    float mobility =
        lerp(0.86, 1.34, edgeExposure) *
        lerp(0.96, 1.14, materialAge) *
        lerp(1.0, 1.18, negative) *
        lerp(1.0, 0.84, support);

    // Area-balanced wobble: use opposed samples and normalized weights. Unlike
    // the 5.5c lifecycle repair, this is not a max/current union; material can
    // locally move away from a cell while another nearby cell gains it. That
    // produces visible back-and-forth body motion without making Presence an
    // independent death authority or an ever-growing smear.
    float morphWeight = saturate(_FoamDeltaTime * 5.65 * activity * mobility);
    float currentWeight = 1.0 - morphWeight;
    float primaryWeight = morphWeight * 0.46;
    float counterWeight = morphWeight * 0.34;
    float crossWeight = morphWeight * 0.20;
    float totalWeight = max(
        0.0001,
        currentWeight + primaryWeight + counterWeight + crossWeight);

    float4 mixedPacked =
        (currentPacked * currentWeight +
         primaryPacked * primaryWeight +
         counterPacked * counterWeight +
         crossPacked * crossWeight) /
        totalWeight;

    FoamMaterialState mixedState = FoamDecodeMaterialState(
        FoamClampPackedMaterialState(mixedPacked));

    // Do not allow interpolation haze to accumulate into new full material.
    // Existing cells are not forcibly preserved at full strength; that previous
    // union behavior caused monotonic area growth. Lifespan remains governed by
    // Remaining Life because this pass contains no explicit Presence erosion.
    mixedState.presence = min(
        mixedState.presence,
        nearbyPresence + 0.025);

    return FoamClipPackedToValidFluid(
        FoamEncodeMaterialState(mixedState),
        validFluid);
}

