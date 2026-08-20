static const float FoamMaterialStateEpsilon = 0.0001;
static const int FoamMaterialContractCoverageLife = 1;

bool FoamMaterialContractUsesCoverageLife()
{
    return _FoamMaterialContract == FoamMaterialContractCoverageLife;
}

struct FoamMaterialState
{
    float coverage;
    float presence;
    float remainingLife;
    float materialPattern;
};

FoamMaterialState FoamDecodeMaterialState(float4 packed)
{
    FoamMaterialState state = (FoamMaterialState)0;
    if (FoamMaterialContractUsesCoverageLife())
    {
        float storedCoverage = saturate(packed.w);
        if (storedCoverage <= 0.00000001 && packed.x > 0.0)
        {
            storedCoverage = saturate(packed.x);
        }

        float lifeMoment = clamp(packed.y, 0.0, storedCoverage);
        float patternMoment = clamp(packed.z, 0.0, storedCoverage);
        state.coverage = storedCoverage;
        if (storedCoverage > FoamMaterialStateEpsilon &&
            lifeMoment > 0.0)
        {
            state.presence = 1.0;
            state.remainingLife = saturate(
                lifeMoment / max(storedCoverage, 0.00000001));
            state.materialPattern = saturate(
                patternMoment / max(storedCoverage, 0.00000001));
        }
        return state;
    }

    float materialAmount = saturate(packed.x);
    float storedCoverage = saturate(packed.w);

    // P13A writes explicit Coverage to alpha. Preserve a transient legacy
    // fallback for a positive pre-P13 RGB state whose alpha is still zero:
    // the former Presence amount becomes Coverage and intrinsic Presence is
    // one, preserving the visible material amount until the state is replaced.
    bool legacyPackedState =
        storedCoverage <= 0.00000001 &&
        materialAmount > 0.0;
    state.coverage = legacyPackedState
        ? materialAmount
        : storedCoverage;

    if (state.coverage > FoamMaterialStateEpsilon &&
        materialAmount > 0.0)
    {
        state.presence = legacyPackedState
            ? 1.0
            : saturate(materialAmount / max(state.coverage, 0.00000001));
        state.remainingLife = saturate(packed.y / max(materialAmount, 0.00000001));
        state.materialPattern = saturate(packed.z / max(materialAmount, 0.00000001));
    }
    else
    {
        state.presence = 0.0;
        state.remainingLife = 0.0;
        state.materialPattern = 0.0;
    }

    return state;
}

float4 FoamEncodeMaterialState(FoamMaterialState state)
{
    if (FoamMaterialContractUsesCoverageLife())
    {
        float coverage = saturate(state.coverage);
        float life = saturate(state.remainingLife);
        if (coverage <= FoamMaterialStateEpsilon ||
            life <= FoamMaterialStateEpsilon)
        {
            return 0.0.xxxx;
        }

        float pattern = saturate(state.materialPattern);
        return float4(
            coverage,
            coverage * life,
            coverage * pattern,
            coverage);
    }

    state.coverage = saturate(state.coverage);
    state.presence = saturate(state.presence);
    state.remainingLife = saturate(state.remainingLife);
    state.materialPattern = saturate(state.materialPattern);
    if (state.coverage <= FoamMaterialStateEpsilon ||
        state.presence <= 0.0 ||
        state.remainingLife <= 0.0)
    {
        return 0.0.xxxx;
    }

    float materialAmount = state.coverage * state.presence;
    return float4(
        materialAmount,
        materialAmount * state.remainingLife,
        materialAmount * state.materialPattern,
        state.coverage);
}

float4 FoamClampPackedMaterialState(float4 packed)
{
    if (FoamMaterialContractUsesCoverageLife())
    {
        float rawCoverage = max(0.0, packed.w);
        if (rawCoverage <= 0.00000001 && packed.x > 0.0)
        {
            rawCoverage = max(0.0, packed.x);
        }
        float rawLifeMoment = clamp(packed.y, 0.0, rawCoverage);
        float rawPatternMoment = clamp(packed.z, 0.0, rawCoverage);
        if (rawCoverage <= FoamMaterialStateEpsilon ||
            rawLifeMoment <= 0.0)
        {
            return 0.0.xxxx;
        }

        float coverage = saturate(rawCoverage);
        float life = saturate(
            rawLifeMoment / max(rawCoverage, 0.00000001));
        float pattern = saturate(
            rawPatternMoment / max(rawCoverage, 0.00000001));
        return float4(
            coverage,
            coverage * life,
            coverage * pattern,
            coverage);
    }

    // Clamp capacity coherently. Transport may temporarily converge more than
    // one cell of Coverage, but capacity resolution must not independently
    // reshape the material moments and thereby invent new Presence or Life.
    float rawMaterialAmount = max(0.0, packed.x);
    float rawCoverage = max(0.0, packed.w);
    if (rawCoverage <= 0.00000001 &&
        rawMaterialAmount > 0.0)
    {
        rawCoverage = rawMaterialAmount;
    }

    rawMaterialAmount = min(rawMaterialAmount, rawCoverage);
    float rawLifeMoment = clamp(packed.y, 0.0, rawMaterialAmount);
    float rawPatternMoment = clamp(packed.z, 0.0, rawMaterialAmount);
    if (rawCoverage <= FoamMaterialStateEpsilon ||
        rawMaterialAmount <= 0.0 ||
        rawLifeMoment <= 0.0)
    {
        return 0.0.xxxx;
    }

    FoamMaterialState state;
    state.coverage = saturate(rawCoverage);
    state.presence = saturate(
        rawMaterialAmount / max(rawCoverage, 0.00000001));
    state.remainingLife = saturate(
        rawLifeMoment / max(rawMaterialAmount, 0.00000001));
    state.materialPattern = saturate(
        rawPatternMoment / max(rawMaterialAmount, 0.00000001));
    return FoamEncodeMaterialState(state);
}

float4 FoamMergeBornMaterial(float4 existingPacked, float4 sourcePacked)
{
    FoamMaterialState existing = FoamDecodeMaterialState(existingPacked);
    FoamMaterialState source = FoamDecodeMaterialState(sourcePacked);
    if (source.coverage <= FoamMaterialStateEpsilon ||
        source.presence <= 0.0 ||
        source.remainingLife <= 0.0)
    {
        return FoamEncodeMaterialState(existing);
    }
    if (existing.coverage <= FoamMaterialStateEpsilon ||
        existing.presence <= 0.0 ||
        existing.remainingLife <= 0.0)
    {
        return FoamEncodeMaterialState(source);
    }

    // D7 packet-independence contract: a birth may affect only the fraction
    // of the cell that it genuinely adds. Repeated overlap over an already
    // occupied fraction must not reset Presence, Remaining Life, or Pattern.
    float addedCoverage = max(
        0.0,
        source.coverage - existing.coverage);
    if (addedCoverage <= FoamMaterialStateEpsilon)
    {
        return FoamEncodeMaterialState(existing);
    }

    float existingAmount =
        existing.coverage * existing.presence;
    float addedAmount =
        addedCoverage * source.presence;
    float combinedCoverage =
        existing.coverage + addedCoverage;
    float combinedAmount =
        existingAmount + addedAmount;

    // The packed moments are additive over only the newly occupied fraction.
    // Returning them directly avoids decode/re-encode divisions in the birth
    // raster hot path while preserving CP/CPL/CPM/C invariants.
    return float4(
        saturate(combinedAmount),
        saturate(
            existingAmount * existing.remainingLife +
            addedAmount * source.remainingLife),
        saturate(
            existingAmount * existing.materialPattern +
            addedAmount * source.materialPattern),
        saturate(combinedCoverage));
}

float4 FoamClipPackedToValidFluid(float4 packed, float validFluid)
{
    FoamMaterialState state = FoamDecodeMaterialState(packed);
    state.coverage = min(state.coverage, saturate(validFluid));
    return FoamEncodeMaterialState(state);
}


// Persistent material simulation is not a morphology authority. The caller's
// conservative finite-volume step owns spatial movement; this helper only
// clamps and clips an already transported packed state before lifecycle aging.


float4 FoamPreservePersistentMaterialState(
    float4 currentPacked,
    float validFluid)
{
    // No spatial source sampling here. The previous persistent morph system
    // sampled neighbouring material and wrote that mixture back into storage,
    // which made it a second hidden transport path. Keep this function boring
    // and explicit: clamp current material and clip it to valid fluid only.
    return FoamClipPackedToValidFluid(
        FoamClampPackedMaterialState(currentPacked),
        validFluid);
}



// Conservative packed-state transport shared by both material contracts.
// Bulk Phase removes the shared downstream speed and the retained bounded
// Superbee TVD reconstruction resolves interior faces for residual/lateral
// advection. Baseline transports C×P/C×P×L/C×P×M/C; Coverage + Life reduces
// those coherent moments to C/C×L/C×Mvisual/C.

bool FoamTransportInsideGrid(int2 coordinate)
{
    return coordinate.x >= 0 && coordinate.x < _FoamDimensions.x &&
        coordinate.y >= 0 && coordinate.y < _FoamDimensions.y;
}

bool FoamTransportInsideSimulation(int2 coordinate)
{
    return FoamTransportInsideGrid(coordinate) &&
        IsFoamGridColumnInsideSimulation(coordinate.x);
}

float4 FoamLoadTransportPacked(int2 coordinate, out float validFluid)
{
    validFluid = FoamValidFluidAt(coordinate);
    if (validFluid <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxxx;
    }

    int2 sampleCoordinate = coordinate;
    sampleCoordinate.x -= _FoamBulkTransportIntegerShift;
    if (!FoamTransportInsideSimulation(sampleCoordinate))
    {
        return 0.0.xxxx;
    }

    return FoamClipPackedToValidFluid(
        FoamClampPackedMaterialState(
            _FoamStateRead.Load(int3(sampleCoordinate, 0))),
        validFluid);
}

float2 FoamResolveGridVelocity(int2 coordinate, float validFluid)
{
    float2 motionCoordinate = (float2)coordinate + 0.5;
    motionCoordinate.x += _FoamBulkTransportPhaseCells;
    RiverWaterFoamResolvedVelocity resolved = FoamResolveVelocity(
        motionCoordinate,
        validFluid);
    float flowSign = _FoamFlowDirection >= 0.0 ? 1.0 : -1.0;
    float downstream = resolved.velocityMetresPerSecond.x * flowSign;
    downstream -= _FoamBulkTransportSpeed * flowSign *
        saturate(validFluid);
    return float2(downstream, resolved.velocityMetresPerSecond.y);
}

float FoamTransportLongitudinalSpacing(int x)
{
    int safeX = clamp(x, 0, max(0, _FoamDimensions.x - 1));
    return max(0.0001, _FoamMetricRows[safeX].widthsAndSpacing.z);
}

float FoamTransportLateralSpacing(int x)
{
    int safeX = clamp(x, 0, max(0, _FoamDimensions.x - 1));
    return max(0.0001, _FoamMetricRows[safeX].widthsAndSpacing.w);
}

static const float FoamTransportMinimumCurvatureJacobian = 0.25;

float FoamTransportRawCurvatureJacobian(
    int x,
    float lateralMetres)
{
    if (!FoamGridUsesFixedMetricLattice())
    {
        return 1.0;
    }

    int safeX = clamp(x, 0, max(0, _FoamDimensions.x - 1));
    FoamMetricRow metric = _FoamMetricRows[safeX];
    return 1.0 - metric.topologyData.x * lateralMetres;
}

float FoamTransportCurvatureJacobian(
    int x,
    float lateralMetres)
{
    return max(
        FoamTransportMinimumCurvatureJacobian,
        FoamTransportRawCurvatureJacobian(x, lateralMetres));
}

float FoamTransportCellArea(int x, int y)
{
    int safeX = clamp(x, 0, max(0, _FoamDimensions.x - 1));
    FoamMetricRow metric = _FoamMetricRows[safeX];
    float lateralMetres = FoamLateralMetresAtTexel(y, metric);
    return FoamTransportLongitudinalSpacing(x) *
        FoamTransportLateralSpacing(x) *
        FoamTransportCurvatureJacobian(x, lateralMetres);
}


float FoamTransportLateralFaceLength(int x, int lowerY)
{
    float longitudinalSpacing = FoamTransportLongitudinalSpacing(x);
    if (!FoamGridUsesFixedMetricLattice())
    {
        return longitudinalSpacing;
    }

    int safeX = clamp(x, 0, max(0, _FoamDimensions.x - 1));
    FoamMetricRow metric = _FoamMetricRows[safeX];
    float lowerLateral = FoamLateralMetresAtTexel(lowerY, metric);
    float upperLateral = FoamLateralMetresAtTexel(lowerY + 1, metric);
    float faceLateral = 0.5 * (lowerLateral + upperLateral);
    return longitudinalSpacing *
        FoamTransportCurvatureJacobian(x, faceLateral);
}

float FoamTransportSuperbeeSlopeComponent(
    float backwardDifference,
    float forwardDifference)
{
    float slope = 0.0;
    if (backwardDifference * forwardDifference > 0.0)
    {
        float direction = backwardDifference >= 0.0 ? 1.0 : -1.0;
        float backwardMagnitude = abs(backwardDifference);
        float forwardMagnitude = abs(forwardDifference);
        float candidateA = min(
            backwardMagnitude * 2.0,
            forwardMagnitude);
        float candidateB = min(
            backwardMagnitude,
            forwardMagnitude * 2.0);
        slope = direction * max(candidateA, candidateB);
    }

    return slope;
}

float FoamTransportCoverage(float4 packed)
{
    return FoamDecodeMaterialState(packed).coverage;
}

float4 FoamLoadTransportPackedOrFallback(
    int2 coordinate,
    float4 fallbackPacked)
{
    if (!FoamTransportInsideSimulation(coordinate))
    {
        return fallbackPacked;
    }

    float validFluid;
    float4 packed = FoamLoadTransportPacked(coordinate, validFluid);
    return validFluid > FoamMaterialStateEpsilon
        ? packed
        : fallbackPacked;
}

float4 FoamResolveInteriorFaceDonor(
    int2 negativeCoordinate,
    int2 positiveCoordinate,
    int2 axis,
    float faceVelocity,
    float4 negativePacked,
    float4 positivePacked)
{
    FoamMaterialState negativeState = FoamDecodeMaterialState(
        negativePacked);
    FoamMaterialState positiveState = FoamDecodeMaterialState(
        positivePacked);
    FoamMaterialState donorState;
    if (faceVelocity >= 0.0)
    {
        donorState.coverage = negativeState.coverage;
        donorState.presence = negativeState.presence;
        donorState.remainingLife = negativeState.remainingLife;
        donorState.materialPattern = negativeState.materialPattern;
    }
    else
    {
        donorState.coverage = positiveState.coverage;
        donorState.presence = positiveState.presence;
        donorState.remainingLife = positiveState.remainingLife;
        donorState.materialPattern = positiveState.materialPattern;
    }
    float reconstructionScale = 0.5 * (1.0 - saturate(
        _FoamTransportReconstructionCourant));

    float reconstructedCoverage = donorState.coverage;
    if (faceVelocity >= 0.0)
    {
        float4 previousPacked = FoamLoadTransportPackedOrFallback(
            negativeCoordinate - axis,
            negativePacked);
        float previousCoverage = FoamTransportCoverage(previousPacked);
        float slope = FoamTransportSuperbeeSlopeComponent(
            negativeState.coverage - previousCoverage,
            positiveState.coverage - negativeState.coverage);
        reconstructedCoverage =
            negativeState.coverage + slope * reconstructionScale;
    }
    else
    {
        float4 nextPacked = FoamLoadTransportPackedOrFallback(
            positiveCoordinate + axis,
            positivePacked);
        float nextCoverage = FoamTransportCoverage(nextPacked);
        float slope = FoamTransportSuperbeeSlopeComponent(
            positiveState.coverage - negativeState.coverage,
            nextCoverage - positiveState.coverage);
        reconstructedCoverage =
            positiveState.coverage - slope * reconstructionScale;
    }

    donorState.coverage = clamp(
        reconstructedCoverage,
        min(negativeState.coverage, positiveState.coverage),
        max(negativeState.coverage, positiveState.coverage));
    return FoamEncodeMaterialState(donorState);
}

void FoamResolveLongitudinalFaceFlux(
    int leftX,
    int y,
    out float4 resolvedFlux,
    out float4 boundaryOutflow)
{
    resolvedFlux = 0.0.xxxx;
    boundaryOutflow = 0.0.xxxx;
    int2 leftCoordinate = int2(leftX, y);
    int2 rightCoordinate = int2(leftX + 1, y);
    bool leftInside = FoamTransportInsideSimulation(leftCoordinate);
    bool rightInside = FoamTransportInsideSimulation(rightCoordinate);

    if (leftInside && rightInside)
    {
        float leftValid;
        float rightValid;
        float4 leftPacked = FoamLoadTransportPacked(
            leftCoordinate,
            leftValid);
        float4 rightPacked = FoamLoadTransportPacked(
            rightCoordinate,
            rightValid);

        // Banks, obstacle footprints, and invalid padded cells are closed
        // faces. Partial boundary coverage remains represented by the
        // canonical velocity validity and packed-state capacity.
        if (leftValid <= FoamMaterialStateEpsilon ||
            rightValid <= FoamMaterialStateEpsilon)
        {
            return;
        }

        float leftVelocity = FoamResolveGridVelocity(
            leftCoordinate,
            leftValid).x;
        float rightVelocity = FoamResolveGridVelocity(
            rightCoordinate,
            rightValid).x;
        float faceVelocity = 0.5 * (leftVelocity + rightVelocity);
        float faceLength = 0.5 * (
            FoamTransportLateralSpacing(leftX) +
            FoamTransportLateralSpacing(leftX + 1));
        float4 donor = FoamResolveInteriorFaceDonor(
            leftCoordinate,
            rightCoordinate,
            int2(1, 0),
            faceVelocity,
            leftPacked,
            rightPacked);
        resolvedFlux = faceVelocity * faceLength * donor;
        return;
    }

    // Only the physical longitudinal endpoint is open. There is never
    // external inflow: positive grid velocity may leave the right endpoint,
    // and negative grid velocity may leave the left endpoint. Reversing river
    // flow changes the signed grid velocity and therefore swaps the outlet.
    if (leftInside && !rightInside)
    {
        float leftValid;
        float4 leftPacked = FoamLoadTransportPacked(
            leftCoordinate,
            leftValid);
        if (leftValid <= FoamMaterialStateEpsilon)
        {
            return;
        }

        float faceVelocity = FoamResolveGridVelocity(
            leftCoordinate,
            leftValid).x;
        if (faceVelocity <= 0.0)
        {
            return;
        }

        resolvedFlux = faceVelocity *
            FoamTransportLateralSpacing(leftX) * leftPacked;
        boundaryOutflow = max(0.0.xxxx, resolvedFlux);
        return;
    }

    if (!leftInside && rightInside)
    {
        float rightValid;
        float4 rightPacked = FoamLoadTransportPacked(
            rightCoordinate,
            rightValid);
        if (rightValid <= FoamMaterialStateEpsilon)
        {
            return;
        }

        float faceVelocity = FoamResolveGridVelocity(
            rightCoordinate,
            rightValid).x;
        if (faceVelocity >= 0.0)
        {
            return;
        }

        resolvedFlux = faceVelocity *
            FoamTransportLateralSpacing(rightCoordinate.x) * rightPacked;
        boundaryOutflow = max(0.0.xxxx, -resolvedFlux);
        return;
    }
}

float4 FoamResolveLateralFaceFlux(
    int x,
    int lowerY,
    out float faceVelocity,
    out float donorPresence,
    out float faceLength)
{
    faceVelocity = 0.0;
    donorPresence = 0.0;
    faceLength = 0.0;
    int2 lowerCoordinate = int2(x, lowerY);
    int2 upperCoordinate = int2(x, lowerY + 1);
    if (!FoamTransportInsideSimulation(lowerCoordinate) ||
        !FoamTransportInsideSimulation(upperCoordinate))
    {
        return 0.0.xxxx;
    }

    float lowerValid;
    float upperValid;
    float4 lowerPacked = FoamLoadTransportPacked(
        lowerCoordinate,
        lowerValid);
    float4 upperPacked = FoamLoadTransportPacked(
        upperCoordinate,
        upperValid);
    if (lowerValid <= FoamMaterialStateEpsilon ||
        upperValid <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxxx;
    }

    float lowerVelocity = FoamResolveGridVelocity(
        lowerCoordinate,
        lowerValid).y;
    float upperVelocity = FoamResolveGridVelocity(
        upperCoordinate,
        upperValid).y;
    faceVelocity = 0.5 * (lowerVelocity + upperVelocity);
    float4 donor = FoamResolveInteriorFaceDonor(
        lowerCoordinate,
        upperCoordinate,
        int2(0, 1),
        faceVelocity,
        lowerPacked,
        upperPacked);
    donorPresence = donor.x;
    faceLength = FoamTransportLateralFaceLength(x, lowerY);
    return faceVelocity * faceLength * donor;
}

float4 FoamResolveLateralFaceFlux(int x, int lowerY)
{
    float ignoredVelocity;
    float ignoredPresence;
    float ignoredLength;
    return FoamResolveLateralFaceFlux(
        x,
        lowerY,
        ignoredVelocity,
        ignoredPresence,
        ignoredLength);
}



uint FoamTransportFixedPoint(float value)
{
    float scaled = max(0.0, value) *
        max(1.0, _FoamTransportMetricFixedPointScale);
    return (uint)min(4294967040.0, floor(scaled + 0.5));
}

void FoamAccumulateLateralTransportEvidence(
    float faceVelocity,
    float donorPresence,
    float faceLength,
    float presenceFlux)
{
    if (_FoamTransportMetricsEnabled == 0 ||
        donorPresence <= FoamMaterialStateEpsilon ||
        faceLength <= 0.0)
    {
        return;
    }

    float weight = donorPresence * faceLength;
    float movement = _FoamDeltaTime * abs(presenceFlux);
    uint ignored;
    _FoamTransportMetrics.InterlockedAdd(
        FoamTransportLateralWeightedSpeedNumeratorOffset,
        FoamTransportFixedPoint(abs(faceVelocity) * weight),
        ignored);
    _FoamTransportMetrics.InterlockedAdd(
        FoamTransportLateralWeightedSpeedWeightOffset,
        FoamTransportFixedPoint(weight),
        ignored);
    _FoamTransportMetrics.InterlockedAdd(
        presenceFlux >= 0.0
            ? FoamTransportLateralPositiveMovementOffset
            : FoamTransportLateralNegativeMovementOffset,
        FoamTransportFixedPoint(movement),
        ignored);
}

void FoamAccumulateTransportTriplet(
    uint presenceOffset,
    uint lifeOffset,
    uint patternOffset,
    float4 areaWeightedPacked)
{
    if (_FoamTransportMetricsEnabled == 0)
    {
        return;
    }

    uint originalValue;
    _FoamTransportMetrics.InterlockedAdd(
        presenceOffset,
        FoamTransportFixedPoint(areaWeightedPacked.x),
        originalValue);
    _FoamTransportMetrics.InterlockedAdd(
        lifeOffset,
        FoamTransportFixedPoint(areaWeightedPacked.y),
        originalValue);
    _FoamTransportMetrics.InterlockedAdd(
        patternOffset,
        FoamTransportFixedPoint(areaWeightedPacked.z),
        originalValue);
}

void FoamAccumulateTransportPresenceAttribution(
    int2 coordinate,
    float rawTransportedCoverage,
    float rawTransportedMaterialAmount,
    float finalStoredMaterialAmount,
    float validFluid,
    float cellArea,
    float totalMaterialAmountLossArea)
{
    if (_FoamTransportMetricsEnabled == 0)
    {
        return;
    }

    float positiveRawCoverage = max(0.0, rawTransportedCoverage);
    float positiveRawMaterialAmount = max(
        0.0,
        rawTransportedMaterialAmount);
    if (positiveRawCoverage <= 0.00000001 &&
        positiveRawMaterialAmount > 0.0)
    {
        positiveRawCoverage = positiveRawMaterialAmount;
    }

    float intrinsicPresence = positiveRawCoverage > 0.00000001
        ? saturate(
            min(positiveRawMaterialAmount, positiveRawCoverage) /
            positiveRawCoverage)
        : 0.0;
    float unitLimitedCoverage = saturate(positiveRawCoverage);
    float boundaryCapacity = LoadBoundaryCoverage(coordinate);
    float boundaryLimitedCoverage = min(
        unitLimitedCoverage,
        boundaryCapacity);
    float obstacleLimitedCoverage = min(
        boundaryLimitedCoverage,
        saturate(validFluid));

    float unitLimitedMaterialAmount =
        unitLimitedCoverage * intrinsicPresence;
    float boundaryLimitedMaterialAmount =
        boundaryLimitedCoverage * intrinsicPresence;
    float obstacleLimitedMaterialAmount =
        obstacleLimitedCoverage * intrinsicPresence;

    float unitCapacityLoss = max(
        0.0,
        positiveRawMaterialAmount - unitLimitedMaterialAmount);
    float boundaryCapacityLoss = max(
        0.0,
        unitLimitedMaterialAmount - boundaryLimitedMaterialAmount);
    float obstacleCapacityLoss = max(
        0.0,
        boundaryLimitedMaterialAmount - obstacleLimitedMaterialAmount);

    bool minimumCutoff =
        obstacleLimitedCoverage > 0.0 &&
        obstacleLimitedCoverage <= FoamMaterialStateEpsilon;
    float minimumCutoffLoss = minimumCutoff
        ? obstacleLimitedMaterialAmount
        : 0.0;
    float stateValidityLoss = max(
        0.0,
        obstacleLimitedMaterialAmount -
        max(0.0, finalStoredMaterialAmount) -
        minimumCutoffLoss);

    bool unitCapacityHit =
        positiveRawCoverage > unitLimitedCoverage;
    bool boundaryCapacityHit =
        unitLimitedCoverage > boundaryLimitedCoverage;
    bool obstacleCapacityHit =
        boundaryLimitedCoverage > obstacleLimitedCoverage;
    bool anyCapacityHit =
        unitCapacityHit ||
        boundaryCapacityHit ||
        obstacleCapacityHit;

    float maximumLocalCapacityExcess = max(
        0.0,
        positiveRawCoverage - saturate(validFluid));

    float attributedLoss =
        unitCapacityLoss +
        boundaryCapacityLoss +
        obstacleCapacityLoss +
        stateValidityLoss +
        minimumCutoffLoss;
    float attributedLossArea = attributedLoss * cellArea;
    float reconciliationTolerance = max(
        0.0000001,
        totalMaterialAmountLossArea * 0.00001);
    bool floatAttributionReconciles =
        attributedLoss > 0.0 &&
        abs(attributedLossArea - totalMaterialAmountLossArea) <=
            reconciliationTolerance;

    uint unitCapacityLossFixed;
    uint boundaryCapacityLossFixed;
    uint obstacleCapacityLossFixed;
    uint stateValidityLossFixed;
    uint minimumCutoffLossFixed;
    if (floatAttributionReconciles)
    {
        uint totalLossFixed = FoamTransportFixedPoint(
            totalMaterialAmountLossArea);
        float inverseAttributedLoss = 1.0 / attributedLoss;
        uint unitEnd = (uint)min(
            (float)totalLossFixed,
            floor(
                (float)totalLossFixed *
                unitCapacityLoss *
                inverseAttributedLoss + 0.5));
        uint boundaryEnd = max(
            unitEnd,
            (uint)min(
                (float)totalLossFixed,
                floor(
                    (float)totalLossFixed *
                    (unitCapacityLoss + boundaryCapacityLoss) *
                    inverseAttributedLoss + 0.5)));
        uint obstacleEnd = max(
            boundaryEnd,
            (uint)min(
                (float)totalLossFixed,
                floor(
                    (float)totalLossFixed *
                    (unitCapacityLoss + boundaryCapacityLoss +
                     obstacleCapacityLoss) *
                    inverseAttributedLoss + 0.5)));
        uint validityEnd = max(
            obstacleEnd,
            (uint)min(
                (float)totalLossFixed,
                floor(
                    (float)totalLossFixed *
                    (unitCapacityLoss + boundaryCapacityLoss +
                     obstacleCapacityLoss + stateValidityLoss) *
                    inverseAttributedLoss + 0.5)));

        unitCapacityLossFixed = unitEnd;
        boundaryCapacityLossFixed = boundaryEnd - unitEnd;
        obstacleCapacityLossFixed = obstacleEnd - boundaryEnd;
        stateValidityLossFixed = validityEnd - obstacleEnd;
        minimumCutoffLossFixed = totalLossFixed - validityEnd;
    }
    else
    {
        // Keep a genuine unattributed path visible through the CPU residual
        // instead of forcing an incomplete category set to reconcile.
        unitCapacityLossFixed = FoamTransportFixedPoint(
            unitCapacityLoss * cellArea);
        boundaryCapacityLossFixed = FoamTransportFixedPoint(
            boundaryCapacityLoss * cellArea);
        obstacleCapacityLossFixed = FoamTransportFixedPoint(
            obstacleCapacityLoss * cellArea);
        stateValidityLossFixed = FoamTransportFixedPoint(
            stateValidityLoss * cellArea);
        minimumCutoffLossFixed = FoamTransportFixedPoint(
            minimumCutoffLoss * cellArea);
    }

    uint originalValue;
    _FoamTransportMetrics.InterlockedAdd(
        FoamTransportPresenceUnitCapacityLossOffset,
        unitCapacityLossFixed,
        originalValue);
    _FoamTransportMetrics.InterlockedAdd(
        FoamTransportPresenceBoundaryCapacityLossOffset,
        boundaryCapacityLossFixed,
        originalValue);
    _FoamTransportMetrics.InterlockedAdd(
        FoamTransportPresenceObstacleCapacityLossOffset,
        obstacleCapacityLossFixed,
        originalValue);
    _FoamTransportMetrics.InterlockedAdd(
        FoamTransportPresenceStateValidityLossOffset,
        stateValidityLossFixed,
        originalValue);
    _FoamTransportMetrics.InterlockedAdd(
        FoamTransportPresenceMinimumCutoffLossOffset,
        minimumCutoffLossFixed,
        originalValue);
    _FoamTransportMetrics.InterlockedMax(
        FoamTransportMaximumRawPresenceOffset,
        FoamTransportFixedPoint(positiveRawCoverage),
        originalValue);
    _FoamTransportMetrics.InterlockedMax(
        FoamTransportMaximumLocalCapacityExcessOffset,
        FoamTransportFixedPoint(maximumLocalCapacityExcess),
        originalValue);

    if (anyCapacityHit)
    {
        _FoamTransportMetrics.InterlockedAdd(
            FoamTransportTotalCapacityHitCountOffset,
            1u,
            originalValue);
    }
    if (unitCapacityHit)
    {
        _FoamTransportMetrics.InterlockedAdd(
            FoamTransportUnitCapacityHitCountOffset,
            1u,
            originalValue);
    }
    if (boundaryCapacityHit)
    {
        _FoamTransportMetrics.InterlockedAdd(
            FoamTransportBoundaryCapacityHitCountOffset,
            1u,
            originalValue);
    }
    if (obstacleCapacityHit)
    {
        _FoamTransportMetrics.InterlockedAdd(
            FoamTransportObstacleCapacityHitCountOffset,
            1u,
            originalValue);
    }
}
