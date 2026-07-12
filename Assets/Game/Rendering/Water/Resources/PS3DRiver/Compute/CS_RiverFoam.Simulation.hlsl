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



// Patch 4.11C.5.16B — first-order conservative donor-cell transport.
// Every flux carries the complete packed material vector so Presence,
// Presence*RemainingLife, and Presence*Pattern remain attached.

bool FoamTransportInsideGrid(int2 coordinate)
{
    return coordinate.x >= 0 && coordinate.x < _FoamDimensions.x &&
        coordinate.y >= 0 && coordinate.y < _FoamDimensions.y;
}

bool FoamTransportInsideSimulation(int2 coordinate)
{
    return FoamTransportInsideGrid(coordinate) &&
        IsFoamColumnInsideSimulation(coordinate.x);
}

float4 FoamLoadTransportPacked(int2 coordinate, out float validFluid)
{
    validFluid = FoamValidFluidAt(coordinate);
    if (validFluid <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxxx;
    }

    return FoamClipPackedToValidFluid(
        FoamClampPackedMaterialState(
            _FoamStateRead.Load(int3(coordinate, 0))),
        validFluid);
}

float2 FoamResolveGridVelocity(int2 coordinate, float validFluid)
{
    RiverWaterFoamResolvedVelocity resolved = FoamResolveVelocity(
        (float2)coordinate + 0.5,
        validFluid);
    float flowSign = _FoamFlowDirection >= 0.0 ? 1.0 : -1.0;
    return float2(
        resolved.velocityMetresPerSecond.x * flowSign,
        resolved.velocityMetresPerSecond.y);
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

float FoamTransportCellArea(int x)
{
    return FoamTransportLongitudinalSpacing(x) *
        FoamTransportLateralSpacing(x);
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
        float4 donor = faceVelocity >= 0.0
            ? leftPacked
            : rightPacked;
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

float4 FoamResolveLateralFaceFlux(int x, int lowerY)
{
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
    float faceVelocity = 0.5 * (lowerVelocity + upperVelocity);
    float4 donor = faceVelocity >= 0.0
        ? lowerPacked
        : upperPacked;
    return faceVelocity * FoamTransportLongitudinalSpacing(x) * donor;
}

uint FoamTransportFixedPoint(float value)
{
    float scaled = max(0.0, value) *
        max(1.0, _FoamTransportMetricFixedPointScale);
    return (uint)min(4294967040.0, floor(scaled + 0.5));
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
    float rawTransportedPresence,
    float finalStoredPresence,
    float validFluid,
    float cellArea,
    float totalPresenceLossArea)
{
    if (_FoamTransportMetricsEnabled == 0)
    {
        return;
    }

    float positiveRawPresence = max(0.0, rawTransportedPresence);
    float unitLimitedPresence = saturate(rawTransportedPresence);
    float boundaryCapacity = LoadBoundaryCoverage(coordinate);
    float boundaryLimitedPresence = min(
        unitLimitedPresence,
        boundaryCapacity);
    float obstacleLimitedPresence = min(
        boundaryLimitedPresence,
        saturate(validFluid));

    float unitCapacityLoss = max(
        0.0,
        positiveRawPresence - unitLimitedPresence);
    float boundaryCapacityLoss = max(
        0.0,
        unitLimitedPresence - boundaryLimitedPresence);
    float obstacleCapacityLoss = max(
        0.0,
        boundaryLimitedPresence - obstacleLimitedPresence);

    bool minimumCutoff =
        obstacleLimitedPresence > 0.0 &&
        obstacleLimitedPresence <= FoamMaterialStateEpsilon;
    float minimumCutoffLoss = minimumCutoff
        ? obstacleLimitedPresence
        : 0.0;
    float stateValidityLoss = max(
        0.0,
        obstacleLimitedPresence -
        saturate(finalStoredPresence) -
        minimumCutoffLoss);

    bool unitCapacityHit = unitCapacityLoss > 0.0;
    bool boundaryCapacityHit = boundaryCapacityLoss > 0.0;
    bool obstacleCapacityHit = obstacleCapacityLoss > 0.0;
    bool anyCapacityHit =
        unitCapacityHit ||
        boundaryCapacityHit ||
        obstacleCapacityHit;

    float maximumLocalCapacityExcess = max(
        0.0,
        positiveRawPresence - saturate(validFluid));

    float attributedLoss =
        unitCapacityLoss +
        boundaryCapacityLoss +
        obstacleCapacityLoss +
        stateValidityLoss +
        minimumCutoffLoss;
    float attributedLossArea = attributedLoss * cellArea;
    float reconciliationTolerance = max(
        0.0000001,
        totalPresenceLossArea * 0.00001);
    bool floatAttributionReconciles =
        attributedLoss > 0.0 &&
        abs(attributedLossArea - totalPresenceLossArea) <=
            reconciliationTolerance;

    uint unitCapacityLossFixed;
    uint boundaryCapacityLossFixed;
    uint obstacleCapacityLossFixed;
    uint stateValidityLossFixed;
    uint minimumCutoffLossFixed;
    if (floatAttributionReconciles)
    {
        uint totalLossFixed = FoamTransportFixedPoint(
            totalPresenceLossArea);
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
        FoamTransportFixedPoint(positiveRawPresence),
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
