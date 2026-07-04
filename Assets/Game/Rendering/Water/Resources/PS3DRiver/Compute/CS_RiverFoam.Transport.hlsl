static const float FoamThincBeta = 2.3;
static const float FoamThincTanhBeta = 0.9800963963;
static const float FoamThincInverseCoshBeta = 0.1985221751;
static const float FoamThincInterfaceEpsilon = 0.00001;

float FoamValidFluidAt(int2 coordinate)
{
    if (coordinate.x < 0 || coordinate.x >= _FoamDimensions.x ||
        coordinate.y < 0 || coordinate.y >= _FoamDimensions.y ||
        !IsFoamColumnInsideSimulation(coordinate.x))
    {
        return 0.0;
    }

    float boundaryCoverage = LoadBoundaryCoverage(coordinate);
    float obstacleFootprint = LoadObstacleExclusionCell(coordinate);
    return saturate(boundaryCoverage * (1.0 - obstacleFootprint));
}


bool FoamIsOutsideLongitudinalSimulation(int2 coordinate)
{
    if (coordinate.y < 0 || coordinate.y >= _FoamDimensions.y)
    {
        return false;
    }

    return coordinate.x < 0 || coordinate.x >= _FoamDimensions.x ||
        !IsFoamColumnInsideSimulation(coordinate.x);
}

float FoamResolveTransportFaceAperture(
    float validCentre,
    float validNeighbour,
    int2 neighbourCoordinate,
    bool longitudinalFace)
{
    if (validNeighbour > FoamMaterialStateEpsilon)
    {
        return min(validCentre, validNeighbour);
    }

    // Banks and canonical obstacles are impermeable. The two longitudinal
    // simulation ends are open: material may leave the allocated river field,
    // while an outside upwind state remains empty so no material can enter.
    return longitudinalFace &&
        FoamIsOutsideLongitudinalSimulation(neighbourCoordinate)
        ? validCentre
        : 0.0;
}

bool FoamIsInsideCurrentTransportRange(int2 coordinate)
{
    return coordinate.x >= _FoamRangeStart &&
        coordinate.x < _FoamRangeStart + _FoamRangeCount &&
        coordinate.y >= 0 && coordinate.y < _FoamDimensions.y;
}

float3 FoamLoadStateTransportValue(int2 coordinate)
{
    if (!FoamIsInsideCurrentTransportRange(coordinate) ||
        FoamValidFluidAt(coordinate) <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxx;
    }

    return FoamClampPackedMaterialState(
        _FoamStateRead.Load(int3(coordinate, 0))).xyz;
}

float3 FoamLoadPredictorTransportValue(int2 coordinate)
{
    // Predictor textures are written only for the current contiguous active
    // range. Never sample outside that range: those texels intentionally keep
    // no current-stage value and may still contain an older dispatch result.
    if (!FoamIsInsideCurrentTransportRange(coordinate) ||
        FoamValidFluidAt(coordinate) <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxx;
    }

    return FoamClampPackedMaterialState(
        _FoamTransportPredictorRead.Load(int3(coordinate, 0))).xyz;
}

float FoamMinmodScalar(float a, float b)
{
    return 0.5 * (sign(a) + sign(b)) * min(abs(a), abs(b));
}

float FoamMonotonizedCentralSlopeScalar(
    float previous,
    float centre,
    float next)
{
    float centred = 0.5 * (next - previous);
    float backward = 2.0 * (centre - previous);
    float forward = 2.0 * (next - centre);
    return FoamMinmodScalar(
        centred,
        FoamMinmodScalar(backward, forward));
}

float FoamResolveLimitedFacePresence(
    float previous,
    float centre,
    float next,
    bool rightFace)
{
    float slope = FoamMonotonizedCentralSlopeScalar(
        previous,
        centre,
        next);
    float value = centre + (rightFace ? 0.5 : -0.5) * slope;
    float neighbour = rightFace ? next : previous;
    return clamp(value, min(centre, neighbour), max(centre, neighbour));
}

float FoamResolveThincFacePresence(
    float previous,
    float centre,
    float next,
    bool rightFace)
{
    previous = saturate(previous);
    centre = saturate(centre);
    next = saturate(next);

    float backwardDifference = centre - previous;
    float forwardDifference = next - centre;
    float minimumValue = min(previous, next);
    float maximumValue = max(previous, next);
    float valueRange = maximumValue - minimumValue;

    // THINC is used only for a resolved monotone interface. Plateaus, local
    // extrema, and nearly uniform regions use the bounded MC reconstruction
    // instead. This prevents an artificial interface from being invented in
    // smooth or constant material while keeping real Presence edges sharp.
    bool resolvedMonotoneInterface =
        valueRange > FoamThincInterfaceEpsilon &&
        backwardDifference * forwardDifference > 0.0 &&
        centre > minimumValue + FoamThincInterfaceEpsilon &&
        centre < maximumValue - FoamThincInterfaceEpsilon;
    if (!resolvedMonotoneInterface)
    {
        return FoamResolveLimitedFacePresence(
            previous,
            centre,
            next,
            rightFace);
    }

    float orientation = next >= previous ? 1.0 : -1.0;
    float normalizedAverage = clamp(
        (centre - minimumValue + FoamThincInterfaceEpsilon) /
        (valueRange + 2.0 * FoamThincInterfaceEpsilon),
        FoamThincInterfaceEpsilon,
        1.0 - FoamThincInterfaceEpsilon);
    float exponentialAverage = exp(
        orientation * FoamThincBeta *
        (2.0 * normalizedAverage - 1.0));
    float interfaceOffset = clamp(
        (exponentialAverage * FoamThincInverseCoshBeta - 1.0) /
        FoamThincTanhBeta,
        -1.0 + FoamThincInterfaceEpsilon,
        1.0 - FoamThincInterfaceEpsilon);

    float mappedFace = interfaceOffset;
    if (rightFace)
    {
        mappedFace =
            (FoamThincTanhBeta + interfaceOffset) /
            max(
                FoamThincInterfaceEpsilon,
                1.0 + FoamThincTanhBeta * interfaceOffset);
    }

    float value = minimumValue + 0.5 * valueRange *
        (1.0 + orientation * mappedFace);
    return clamp(value, minimumValue, maximumValue);
}

float2 FoamResolveTransportAttributes(float3 packed)
{
    float presence = saturate(packed.x);
    if (presence <= FoamMaterialStateEpsilon)
    {
        return 0.0.xx;
    }

    return saturate(packed.yz / presence);
}

float3 FoamResolveCellFacePackedState(
    float3 previous,
    float3 centre,
    float3 next,
    bool rightFace)
{
    float facePresence = FoamResolveThincFacePresence(
        previous.x,
        centre.x,
        next.x,
        rightFace);
    if (facePresence <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxx;
    }

    // Remaining Life and Material Pattern are attributes of the occupied
    // material, not independent scalars. The upwind cell's normalized
    // attributes therefore travel with the same sharp, conservative Presence
    // flux. This preserves the packed-state invariants at every face.
    float2 attributes = FoamResolveTransportAttributes(centre);
    return float3(
        facePresence,
        facePresence * attributes.x,
        facePresence * attributes.y);
}

float3 FoamResolveUpwindFaceValue(
    float3 farLeft,
    float3 left,
    float3 right,
    float3 farRight,
    float velocity)
{
    return velocity >= 0.0
        ? FoamResolveCellFacePackedState(
            farLeft,
            left,
            right,
            true)
        : FoamResolveCellFacePackedState(
            left,
            right,
            farRight,
            false);
}

float3 FoamEvaluateConservativeTransportDerivative(
    int2 coordinate,
    float3 xMinusTwo,
    float3 xMinusOne,
    float3 centre,
    float3 xPlusOne,
    float3 xPlusTwo,
    float3 yMinusTwo,
    float3 yMinusOne,
    float3 yPlusOne,
    float3 yPlusTwo)
{
    FoamMetricRow centreMetric = _FoamMetricRows[ClampX(coordinate.x)];
    FoamMetricRow leftMetric = _FoamMetricRows[ClampX(coordinate.x - 1)];
    FoamMetricRow rightMetric = _FoamMetricRows[ClampX(coordinate.x + 1)];

    float dx = max(0.0001, centreMetric.widthsAndSpacing.z);
    float dy = max(0.0001, centreMetric.widthsAndSpacing.w);
    float leftDx = max(0.0001, leftMetric.widthsAndSpacing.z);
    float rightDx = max(0.0001, rightMetric.widthsAndSpacing.z);
    float leftDy = max(0.0001, leftMetric.widthsAndSpacing.w);
    float rightDy = max(0.0001, rightMetric.widthsAndSpacing.w);
    float cellArea = max(0.000001, dx * dy);

    float validCentre = FoamValidFluidAt(coordinate);
    if (validCentre <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxx;
    }

    float validLeft = FoamValidFluidAt(coordinate + int2(-1, 0));
    float validRight = FoamValidFluidAt(coordinate + int2(1, 0));
    float validDown = FoamValidFluidAt(coordinate + int2(0, -1));
    float validUp = FoamValidFluidAt(coordinate + int2(0, 1));

    float2 rightVelocity = ResolveMaterialVelocity(
        FoamPixelCoordinateToUV(
            float2(coordinate) + float2(0.5, 0.0)));
    float2 leftVelocity = ResolveMaterialVelocity(
        FoamPixelCoordinateToUV(
            float2(coordinate) + float2(-0.5, 0.0)));
    float2 upVelocity = ResolveMaterialVelocity(
        FoamPixelCoordinateToUV(
            float2(coordinate) + float2(0.0, 0.5)));
    float2 downVelocity = ResolveMaterialVelocity(
        FoamPixelCoordinateToUV(
            float2(coordinate) + float2(0.0, -0.5)));

    float uRight = FoamClampFaceVelocity(
        rightVelocity.x,
        min(dx, rightDx));
    float uLeft = FoamClampFaceVelocity(
        leftVelocity.x,
        min(leftDx, dx));
    float vUp = FoamClampFaceVelocity(upVelocity.y, dy);
    float vDown = FoamClampFaceVelocity(downVelocity.y, dy);

    float rightAperture = FoamResolveTransportFaceAperture(
        validCentre,
        validRight,
        coordinate + int2(1, 0),
        true);
    float leftAperture = FoamResolveTransportFaceAperture(
        validCentre,
        validLeft,
        coordinate + int2(-1, 0),
        true);
    float upAperture = FoamResolveTransportFaceAperture(
        validCentre,
        validUp,
        coordinate + int2(0, 1),
        false);
    float downAperture = FoamResolveTransportFaceAperture(
        validCentre,
        validDown,
        coordinate + int2(0, -1),
        false);

    float3 rightFace = FoamResolveUpwindFaceValue(
        xMinusOne,
        centre,
        xPlusOne,
        xPlusTwo,
        uRight);
    float3 leftFace = FoamResolveUpwindFaceValue(
        xMinusTwo,
        xMinusOne,
        centre,
        xPlusOne,
        uLeft);
    float3 upFace = FoamResolveUpwindFaceValue(
        yMinusOne,
        centre,
        yPlusOne,
        yPlusTwo,
        vUp);
    float3 downFace = FoamResolveUpwindFaceValue(
        yMinusTwo,
        yMinusOne,
        centre,
        yPlusOne,
        vDown);

    float rightFaceLength = 0.5 * (dy + rightDy);
    float leftFaceLength = 0.5 * (leftDy + dy);

    float3 rightFlux =
        uRight * rightFaceLength * rightAperture * rightFace;
    float3 leftFlux =
        uLeft * leftFaceLength * leftAperture * leftFace;
    float3 upFlux = vUp * dx * upAperture * upFace;
    float3 downFlux = vDown * dx * downAperture * downFace;

    return (leftFlux - rightFlux + downFlux - upFlux) / cellArea;
}

float3 FoamStateTransportDerivative(int2 coordinate)
{
    return FoamEvaluateConservativeTransportDerivative(
        coordinate,
        FoamLoadStateTransportValue(coordinate + int2(-2, 0)),
        FoamLoadStateTransportValue(coordinate + int2(-1, 0)),
        FoamLoadStateTransportValue(coordinate),
        FoamLoadStateTransportValue(coordinate + int2(1, 0)),
        FoamLoadStateTransportValue(coordinate + int2(2, 0)),
        FoamLoadStateTransportValue(coordinate + int2(0, -2)),
        FoamLoadStateTransportValue(coordinate + int2(0, -1)),
        FoamLoadStateTransportValue(coordinate + int2(0, 1)),
        FoamLoadStateTransportValue(coordinate + int2(0, 2)));
}

float3 FoamPredictorTransportDerivative(int2 coordinate)
{
    return FoamEvaluateConservativeTransportDerivative(
        coordinate,
        FoamLoadPredictorTransportValue(coordinate + int2(-2, 0)),
        FoamLoadPredictorTransportValue(coordinate + int2(-1, 0)),
        FoamLoadPredictorTransportValue(coordinate),
        FoamLoadPredictorTransportValue(coordinate + int2(1, 0)),
        FoamLoadPredictorTransportValue(coordinate + int2(2, 0)),
        FoamLoadPredictorTransportValue(coordinate + int2(0, -2)),
        FoamLoadPredictorTransportValue(coordinate + int2(0, -1)),
        FoamLoadPredictorTransportValue(coordinate + int2(0, 1)),
        FoamLoadPredictorTransportValue(coordinate + int2(0, 2)));
}

float3 FoamResolvePackedTransferMoment(float3 packed, float presenceDelta)
{
    float presence = saturate(packed.x);
    if (presence <= FoamMaterialStateEpsilon || presenceDelta <= 0.0)
    {
        return 0.0.xxx;
    }

    float2 attributes = saturate(packed.yz / presence);
    return float3(
        presenceDelta,
        presenceDelta * attributes.x,
        presenceDelta * attributes.y);
}

float4 FoamResolveCompressedInterfacePairValue(
    int2 coordinateA,
    int2 coordinateB,
    bool returnA)
{
    float validA = FoamValidFluidAt(coordinateA);
    float4 packedA = FoamClipPackedToValidFluid(
        _FoamCompressionStateRead.Load(int3(coordinateA, 0)),
        validA);

    bool hasB = FoamIsInsideCurrentTransportRange(coordinateB);
    float validB = hasB ? FoamValidFluidAt(coordinateB) : 0.0;
    float4 packedB = hasB
        ? FoamClipPackedToValidFluid(
            _FoamCompressionStateRead.Load(int3(coordinateB, 0)),
            validB)
        : 0.0.xxxx;

    if (!hasB || validA <= FoamMaterialStateEpsilon ||
        validB <= FoamMaterialStateEpsilon)
    {
        return returnA ? packedA : packedB;
    }

    float presenceA = saturate(packedA.x);
    float presenceB = saturate(packedB.x);
    float difference = abs(presenceA - presenceB);
    if (difference <= 0.0005)
    {
        return returnA ? packedA : packedB;
    }

    bool aIsReceiver = presenceA > presenceB;
    float3 receiver = aIsReceiver ? packedA.xyz : packedB.xyz;
    float3 donor = aIsReceiver ? packedB.xyz : packedA.xyz;
    float receiverPresence = saturate(receiver.x);
    float donorPresence = saturate(donor.x);

    float conservativeLimit = min(
        donorPresence,
        1.0 - receiverPresence);
    float interfaceWeight = saturate((difference - 0.006) / 0.220);
    float transferPresence = min(
        conservativeLimit,
        difference * 0.72) * interfaceWeight;

    if (transferPresence > FoamMaterialStateEpsilon)
    {
        float3 transfer = FoamResolvePackedTransferMoment(
            donor,
            transferPresence);
        donor = FoamClampPackedMaterialState(
            float4(donor - transfer, 0.0)).xyz;
        receiver = FoamClampPackedMaterialState(
            float4(receiver + transfer, 0.0)).xyz;
    }

    float4 resultA = aIsReceiver
        ? FoamClipPackedToValidFluid(float4(receiver, 0.0), validA)
        : FoamClipPackedToValidFluid(float4(donor, 0.0), validA);
    float4 resultB = aIsReceiver
        ? FoamClipPackedToValidFluid(float4(donor, 0.0), validB)
        : FoamClipPackedToValidFluid(float4(receiver, 0.0), validB);

    return returnA ? resultA : resultB;
}
