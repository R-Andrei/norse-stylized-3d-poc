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

float RiverWaterFoamResolveBaseCoverage(float foamMask)
{
    // Keep production Chip support and normal Foam composition on the same
    // pre-Chip visibility threshold. This is arithmetic-only and does not add
    // a texture sample or persistent field.
    return smoothstep(0.08, 0.46, saturate(foamMask));
}

float RiverWaterFoamResolveLargestMetresPerPixel(float2 riverPointMetres)
{
    float2 riverDx = ddx(riverPointMetres);
    float2 riverDy = ddy(riverPointMetres);
    float g00 = dot(riverDx, riverDx);
    float g01 = dot(riverDx, riverDy);
    float g11 = dot(riverDy, riverDy);
    float discriminant = sqrt(max(
        0.0,
        (g00 - g11) * (g00 - g11) + 4.0 * g01 * g01));
    float largestEigenvalue = 0.5 * (
        g00 + g11 + discriminant);
    return sqrt(max(largestEigenvalue, 0.00000001));
}


struct RiverWaterFoamChipEligibility
{
    float visibleSupport;
    float edgeBand;
    float interiorRegion;
};

struct RiverWaterFoamSelectionDiagnostics
{
    float chipCandidateField;
    float chipEdgeEligibility;
    float chipInteriorEligibility;
    float chipProductionSelection;
};

RiverWaterFoamChipEligibility RiverWaterFoamResolveChipEligibility(
    float preChipSoftVisibility,
    float preChipMask,
    float edgeWidthPixels)
{
    RiverWaterFoamChipEligibility result;
    result.visibleSupport = RiverWaterFoamResolveBaseCoverage(preChipMask);
    result.edgeBand = 0.0;
    result.interiorRegion = result.visibleSupport;

    float widthPixels = max(0.0, edgeWidthPixels);
    [branch]
    if (widthPixels <= 0.0001 || result.visibleSupport <= 0.0001)
    {
        return result;
    }

    // softVisibility already follows the complete pre-Chip production path.
    // Dividing its inward scalar rise by the local screen derivative removes
    // the previous dependence on how quickly Presence happened to saturate.
    // This is a local projected edge coordinate, not a global distance field.
    float edgeSource = saturate(preChipSoftVisibility);
    float edgeGradientPerPixel = max(
        fwidth(edgeSource),
        0.001);
    static const float VisibleSoftEdgeStart = 0.06;
    float estimatedInwardPixels = max(
        0.0,
        edgeSource - VisibleSoftEdgeStart) / edgeGradientPerPixel;
    float edgeMembership = 1.0 - smoothstep(
        widthPixels - 0.5,
        widthPixels + 0.5,
        estimatedInwardPixels);
    edgeMembership = saturate(edgeMembership);

    result.edgeBand = result.visibleSupport * edgeMembership;
    result.interiorRegion = result.visibleSupport * (1.0 - edgeMembership);
    return result;
}

float RiverWaterFoamQuinticSmooth(float value)
{
    float x = saturate(value);
    return x * x * x * (x * (x * 6.0 - 15.0) + 10.0);
}

float RiverWaterFoamSmoothPeriodicWave(float phase)
{
    float cycle = frac(phase);
    float triangleWave = 1.0 - abs(cycle * 2.0 - 1.0);
    return triangleWave * triangleWave * (3.0 - 2.0 * triangleWave);
}

float RiverWaterFoamResolveChipSignedWave(
    float timeSeconds,
    float cyclesPerSecond,
    float phaseOffset)
{
    float speed = max(0.0, cyclesPerSecond);
    if (speed <= 0.0001)
    {
        return 0.0;
    }

    return RiverWaterFoamSmoothPeriodicWave(
        timeSeconds * speed + phaseOffset) * 2.0 - 1.0;
}

float2 RiverWaterFoamResolveChipMorphTrajectory(
    float timeSeconds,
    float changesPerSecond,
    float transitionTimeSeconds,
    float phaseOffset)
{
    float cadence = max(0.0, changesPerSecond);
    if (cadence <= 0.0001)
    {
        return float2(0.0, 0.0);
    }

    // Cadence selects a new deterministic target. Transition Time independently
    // controls how long the geometry takes to move there. Every target is a
    // fixed golden-angle step through the candidate's two-axis morph plane, so
    // consecutive transitions have the same coefficient-space distance rather
    // than occasionally producing a tiny move followed by a violent switch.
    float cadencePosition = max(0.0, timeSeconds) * cadence +
        saturate(phaseOffset);
    float targetIndex = floor(cadencePosition);
    float intervalPhase = frac(cadencePosition);
    float intervalDuration = rcp(cadence);
    float effectiveTransitionTime = min(
        max(0.001, transitionTimeSeconds),
        intervalDuration);
    float transitionFraction = saturate(
        effectiveTransitionTime * cadence);
    float transitionProgress = saturate(
        intervalPhase / max(transitionFraction, 0.0001));
    float easedProgress = RiverWaterFoamQuinticSmooth(
        transitionProgress);

    const float GoldenAngleRadians = 2.39996322973;
    float seedAngle = saturate(phaseOffset) * 6.28318530718;
    float trajectoryAngle = seedAngle +
        (targetIndex - 1.0 + easedProgress) * GoldenAngleRadians;
    float trajectorySin;
    float trajectoryCos;
    sincos(
        trajectoryAngle,
        trajectorySin,
        trajectoryCos);
    return float2(
        trajectoryCos,
        trajectorySin);
}

void RiverWaterFoamResolveChipLifecycle(
    float timeSeconds,
    float phaseOffset,
    float formationTime,
    float stableTime,
    float dissolveTime,
    float dormantTime,
    out float lifeScale,
    out float stableVariationAuthority)
{
    float formation = max(0.001, formationTime);
    float stable = max(0.001, stableTime);
    float dissolve = max(0.001, dissolveTime);
    float dormant = max(0.001, dormantTime);
    float totalDuration = formation + stable + dissolve + dormant;
    float cycleTime = frac(
        max(0.0, timeSeconds) / totalDuration +
        saturate(phaseOffset)) * totalDuration;

    float formationScale = RiverWaterFoamQuinticSmooth(
        cycleTime / formation);
    float dissolveScale = 1.0 - RiverWaterFoamQuinticSmooth(
        (cycleTime - formation - stable) / dissolve);
    lifeScale = saturate(min(formationScale, dissolveScale));

    // Living variation belongs to the established stage. It eases in and out
    // inside Stable Time so Formation remains monotonic and Dissolve returns
    // cleanly to the authored base contour before shrinking to zero.
    float stableBlendDuration = max(
        0.001,
        min(stable * 0.25, 0.75));
    float stableBlendIn = RiverWaterFoamQuinticSmooth(
        (cycleTime - formation) / stableBlendDuration);
    float stableBlendOut = 1.0 - RiverWaterFoamQuinticSmooth(
        (cycleTime - (formation + stable - stableBlendDuration)) /
        stableBlendDuration);
    stableVariationAuthority = saturate(
        stableBlendIn * stableBlendOut);
}

void RiverWaterFoamResolveChipMorphBasis(
    float3 rawBasisU,
    float3 rawBasisV,
    out float3 basisU,
    out float3 basisV)
{
    float rawULengthSq = dot(rawBasisU, rawBasisU);
    basisU = rawULengthSq > 0.0001
        ? rawBasisU * rsqrt(rawULengthSq)
        : float3(1.0, 0.0, 0.0);

    float3 rejectedV = rawBasisV - basisU * dot(rawBasisV, basisU);
    float rejectedLengthSq = dot(rejectedV, rejectedV);

    // The source hashes are almost never collinear, but keep a deterministic
    // orthogonal fallback so every candidate owns a valid two-axis morph plane.
    float3 fallbackAxis = abs(basisU.x) < 0.75
        ? float3(1.0, 0.0, 0.0)
        : float3(0.0, 1.0, 0.0);
    float3 fallbackV = cross(basisU, fallbackAxis);
    fallbackV *= rsqrt(max(dot(fallbackV, fallbackV), 0.0001));

    basisV = rejectedLengthSq > 0.0001
        ? rejectedV * rsqrt(rejectedLengthSq)
        : fallbackV;
}

float RiverWaterFoamResolveStaticChipRadialScale(
    float3 cosineHarmonics,
    float shapeIrregularity,
    float3 baseCosineCoefficients)
{
    float irregularity = saturate(shapeIrregularity);
    float contourDelta = dot(
        baseCosineCoefficients,
        cosineHarmonics);
    float staticEnvelope = 1.0 + irregularity * 0.30 * (
        abs(baseCosineCoefficients.x) +
        abs(baseCosineCoefficients.y) +
        abs(baseCosineCoefficients.z));
    return saturate(
        max(0.24, 1.0 + irregularity * contourDelta) /
        max(1.0, staticEnvelope));
}

float RiverWaterFoamResolveMultiAxisChipRadialScale(
    float2 direction,
    float2 contourAxis,
    float shapeIrregularity,
    float3 baseCosineCoefficients,
    float3 morphBasisU,
    float3 morphBasisV,
    float2 morphTrajectory,
    float shapeChangeAmount)
{
    float2 perpendicularAxis = float2(
        -contourAxis.y,
        contourAxis.x);
    float localX = dot(direction, contourAxis);
    float localY = dot(direction, perpendicularAxis);

    float3 cosineHarmonics = float3(
        localX,
        localX * localX - localY * localY,
        localX * (localX * localX - 3.0 * localY * localY));
    float3 sineHarmonics = float3(
        localY,
        2.0 * localX * localY,
        localY * (3.0 * localX * localX - localY * localY));

    float staticRadialScale = RiverWaterFoamResolveStaticChipRadialScale(
        cosineHarmonics,
        shapeIrregularity,
        baseCosineCoefficients);
    float authority = saturate(shapeChangeAmount);
    if (authority <= 0.0001 ||
        dot(morphTrajectory, morphTrajectory) <= 0.0001)
    {
        return staticRadialScale;
    }

    float3 temporalDirection =
        morphBasisU * morphTrajectory.x +
        morphBasisV * morphTrajectory.y;

    // Use a constant L1 coefficient budget. The raw temporal contour is then
    // bounded to [0.45, 1.55] before area normalization, so it remains positive
    // and connected without a time-varying safety clamp.
    float temporalL1 = max(
        abs(temporalDirection.x) +
        abs(temporalDirection.y) +
        abs(temporalDirection.z),
        0.0001);
    float3 temporalSineCoefficients = temporalDirection *
        (0.55 / temporalL1);

    float irregularity = saturate(shapeIrregularity);
    float3 staticCosineCoefficients =
        baseCosineCoefficients * irregularity;
    float staticEnvelope = 1.0 + irregularity * 0.30 * (
        abs(baseCosineCoefficients.x) +
        abs(baseCosineCoefficients.y) +
        abs(baseCosineCoefficients.z));

    // The temporal target is normalized in squared-radius space. Fourier
    // orthogonality gives an exact angular mean, so every point along the
    // morph trajectory has the same enclosed radial area. Shape Change Amount
    // blends squared radii, preserving that area instead of becoming a hidden
    // Size Pulse. The target may extend a lobe beyond the area-equivalent
    // Candidate Radius; the adaptive candidate search includes the proven
    // 1.52x maximum geometry reach.
    float staticAreaProxy =
        (1.0 + 0.5 * dot(
            staticCosineCoefficients,
            staticCosineCoefficients)) /
        max(staticEnvelope * staticEnvelope, 0.0001);
    float temporalEnergy = 1.0 + 0.5 * dot(
        temporalSineCoefficients,
        temporalSineCoefficients);
    float temporalRawScale = 1.0 + dot(
        temporalSineCoefficients,
        sineHarmonics);
    float temporalRadialScale = sqrt(
        max(0.0, staticAreaProxy / temporalEnergy)) *
        temporalRawScale;

    float blendedSquaredRadius = lerp(
        staticRadialScale * staticRadialScale,
        temporalRadialScale * temporalRadialScale,
        authority);
    return sqrt(max(0.0, blendedSquaredRadius));
}

float RiverWaterFoamSoftIrregularChip(
    float2 deltaMetres,
    float outerRadiusMetres,
    float antialiasMetres,
    float2 contourAxis,
    float shapeIrregularity,
    float3 baseCosineCoefficients,
    float3 morphBasisU,
    float3 morphBasisV,
    float2 morphTrajectory,
    float shapeChangeAmount)
{
    float outerRadius = max(0.0, outerRadiusMetres);
    if (outerRadius <= 0.000001)
    {
        return 0.0;
    }

    float aa = max(0.0005, antialiasMetres);
    float distanceToCentre = length(deltaMetres);
    float2 direction = distanceToCentre > 0.00001
        ? deltaMetres / distanceToCentre
        : contourAxis;
    float radialScale = RiverWaterFoamResolveMultiAxisChipRadialScale(
        direction,
        contourAxis,
        shapeIrregularity,
        baseCosineCoefficients,
        morphBasisU,
        morphBasisV,
        morphTrajectory,
        shapeChangeAmount);
    float localRadius = outerRadius * radialScale;

    return 1.0 - smoothstep(
        localRadius - aa,
        localRadius + aa,
        distanceToCentre);
}

RiverWaterFoamSelectionDiagnostics
RiverWaterFoamEvaluateSelectionDiagnostics(
    float storedGlobalDistance,
    float lateralMetres,
    float preChipSoftVisibility,
    float preChipMask,
    float evaluateChipSelection,
    float evaluateChipCandidates,
    float evaluateCandidatesOutsideMaterial,
    float chipActivation,
    float chipCandidateSpacing,
    float chipSize,
    float chipIrregularity,
    float chipStableScreenRadiusPixels,
    float chipMaximumViewScale,
    float chipEdgeWidthPixels,
    float chipInteriorAccess,
    float chipDownstreamSpeed,
    float chipFormationTime,
    float chipStableTime,
    float chipDissolveTime,
    float chipDormantTime,
    float chipLateralMotionAmount,
    float chipLateralMotionSpeed,
    float chipRotationAmountDegrees,
    float chipRotationSpeed,
    float chipSizePulseAmount,
    float chipSizePulseSpeed,
    float chipShapeChangeAmount,
    float chipShapeChangeSpeed,
    float chipShapeTransitionTime,
    float chipEvolutionTime)
{
    RiverWaterFoamSelectionDiagnostics result;
    result.chipCandidateField = 0.0;
    result.chipEdgeEligibility = 0.0;
    result.chipInteriorEligibility = 0.0;
    result.chipProductionSelection = 0.0;

    [branch]
    if (evaluateChipSelection <= 0.5)
    {
        return result;
    }

    float evolutionTime = max(0.0, chipEvolutionTime);
    float2 chipPointMetres = float2(
        storedGlobalDistance -
            max(0.0, chipDownstreamSpeed) * evolutionTime,
        lateralMetres);
    RiverWaterFoamChipEligibility chipEligibility =
        RiverWaterFoamResolveChipEligibility(
            preChipSoftVisibility,
            preChipMask,
            chipEdgeWidthPixels);
    float activation = saturate(chipActivation);
    float interiorAccess = saturate(chipInteriorAccess);
    float chipInteriorCandidates = 0.0;
    float productionPermissionEnabled =
        (max(0.0, chipEdgeWidthPixels) > 0.0001 ||
            interiorAccess > 0.0001)
        ? 1.0
        : 0.0;
    float candidateFieldRequired =
        (evaluateChipCandidates > 0.5 &&
            activation > 0.0001 &&
            ((productionPermissionEnabled > 0.5 &&
                chipEligibility.visibleSupport > 0.0001) ||
                evaluateCandidatesOutsideMaterial > 0.5))
        ? 1.0
        : 0.0;

    [branch]
    if (candidateFieldRequired > 0.5)
    {
        float metresPerPixel =
            RiverWaterFoamResolveLargestMetresPerPixel(chipPointMetres);
        float antialiasMetres = max(metresPerPixel, 0.0001);
        float spacing = max(0.10, chipCandidateSpacing);
        // Chip Size is one artist-facing bounded control. The internal
        // radius-to-spacing mapping retains the proven adaptive search budget.
        float radiusRatio = lerp(
            0.05,
            0.65,
            saturate(chipSize));
        float nominalRadius = spacing * radiusRatio;
        float stableRadiusPixels = clamp(
            chipStableScreenRadiusPixels,
            0.0,
            16.0);
        float maximumViewScale = clamp(
            chipMaximumViewScale,
            1.0,
            2.5);
        float targetStableRadiusMetres =
            stableRadiusPixels * metresPerPixel;
        float fullStabilizationRadiusMetres =
            targetStableRadiusMetres * 0.75;
        // One static Irregularity control owns deterministic centre jitter,
        // candidate-to-candidate radius variance, and connected contour shape.
        float irregularity = saturate(chipIrregularity);
        float distributionIrregularity = irregularity;
        float sizeIrregularity = irregularity;
        float shapeIrregularity = irregularity;
        float lateralAmount = clamp(
            chipLateralMotionAmount,
            0.0,
            2.5);
        float sizePulseAmount = clamp(
            chipSizePulseAmount,
            0.0,
            0.45);
        float maximumRadiusScale =
            lerp(1.0, 1.40, sizeIrregularity) *
            (1.0 + sizePulseAmount);
        float viewScaleCeiling = stableRadiusPixels > 0.0001
            ? maximumViewScale
            : 1.0;
        float maximumStabilizedRadiusRatio = min(
            0.65,
            radiusRatio * viewScaleCeiling);
        // Multi-axis shape morphing preserves radial area but can redistribute
        // it into a lobe up to 1.52x the area-equivalent Candidate Radius.
        float maximumShapeReachScale = sqrt(lerp(
            1.0,
            1.52 * 1.52,
            saturate(chipShapeChangeAmount)));
        float maximumRadiusReachInSpacings =
            maximumStabilizedRadiusRatio *
            maximumRadiusScale *
            maximumShapeReachScale;
        float maximumLateralReachInSpacings =
            maximumRadiusReachInSpacings + lateralAmount;

        // A source cell can approach the current fragment by half a cell plus
        // the authored centre jitter (0.39 spacing at full irregularity). Use
        // that exact bound to choose the smallest rectangular search that can
        // contain every rigidly translated candidate at the current settings.
        float cellCentreReach = 0.5 +
            0.39 * distributionIrregularity;
        int requiredDownstreamOffset = clamp(
            (int)floor(
                maximumRadiusReachInSpacings + cellCentreReach + 0.0001),
            1,
            2);
        int requiredLateralOffset = clamp(
            (int)floor(
                maximumLateralReachInSpacings + cellCentreReach + 0.0001),
            1,
            5);
        float2 baseCell = floor(chipPointMetres / spacing);

        // Keep the authored reach exact while exposing one candidate body to
        // the shader compiler. The bounds are uniform per material and select
        // the smallest required rectangle for the current settings at runtime.
        [loop]
        for (
            int offsetX = -requiredDownstreamOffset;
            offsetX <= requiredDownstreamOffset;
            offsetX++)
        {
            [loop]
            for (
                int offsetY = -requiredLateralOffset;
                offsetY <= requiredLateralOffset;
                offsetY++)
            {
                float2 cell = baseCell + float2(offsetX, offsetY);
                float centreHashX = RiverWaterFoamHash21(
                    cell + float2(13.17, 41.73));
                float centreHashY = RiverWaterFoamHash21(
                    cell + float2(71.31, 19.47));
                float angleHash = RiverWaterFoamHash21(
                    cell + float2(37.91, 83.11));
                float radiusHash = RiverWaterFoamHash21(
                    cell + float2(97.53, 23.69));
                float secondaryHash = RiverWaterFoamHash21(
                    cell + float2(29.47, 91.13));
                float tertiaryHash = RiverWaterFoamHash21(
                    cell + float2(81.37, 47.59));
                float activationHash = RiverWaterFoamHash21(
                    cell + float2(53.27, 67.19));
                float lifecycleHash = RiverWaterFoamHash21(
                    cell + float2(17.41, 59.83));

                float lifecycleScale;
                float stableVariationAuthority;
                RiverWaterFoamResolveChipLifecycle(
                    evolutionTime,
                    lifecycleHash,
                    chipFormationTime,
                    chipStableTime,
                    chipDissolveTime,
                    chipDormantTime,
                    lifecycleScale,
                    stableVariationAuthority);

                float lateralWave = RiverWaterFoamResolveChipSignedWave(
                    evolutionTime,
                    chipLateralMotionSpeed,
                    secondaryHash * 0.73 + tertiaryHash * 0.27);
                float rotationWave = RiverWaterFoamResolveChipSignedWave(
                    evolutionTime,
                    chipRotationSpeed,
                    tertiaryHash * 0.61 + centreHashX * 0.39);
                float sizePulseWave = RiverWaterFoamResolveChipSignedWave(
                    evolutionTime,
                    chipSizePulseSpeed,
                    radiusHash * 0.67 + centreHashY * 0.33);
                float2 shapeMorphTrajectory =
                    RiverWaterFoamResolveChipMorphTrajectory(
                        evolutionTime,
                        chipShapeChangeSpeed,
                        chipShapeTransitionTime,
                        secondaryHash * 0.57 + centreHashY * 0.43);

                float2 fullJitter = (float2(
                    centreHashX,
                    centreHashY) - 0.5) * 0.78;
                float2 candidateCentre =
                    (cell + 0.5 +
                        fullJitter * distributionIrregularity) * spacing;
                candidateCentre.y +=
                    spacing * lateralAmount * lateralWave;

                float angle =
                    angleHash * 6.28318530718 +
                    clamp(chipRotationAmountDegrees, 0.0, 180.0) *
                    0.01745329252 * rotationWave;
                float2 contourAxis = float2(
                    cos(angle),
                    sin(angle));

                // D.1C removes the tiny-size tail while retaining clear
                // candidate-to-candidate variation. The range is deliberately
                // biased toward medium and large readable bites.
                float fullRadiusVariation = lerp(
                    0.80,
                    1.40,
                    radiusHash);
                float candidateSizeMultiplier = lerp(
                    1.0,
                    fullRadiusVariation,
                    sizeIrregularity);
                float staticCandidateRadius =
                    nominalRadius * candidateSizeMultiplier;

                [branch]
                if (stableRadiusPixels > 0.0001 &&
                    maximumViewScale > 1.0001)
                {
                    float stabilizationWeight = 1.0 - smoothstep(
                        fullStabilizationRadiusMetres,
                        max(
                            targetStableRadiusMetres,
                            fullStabilizationRadiusMetres + 0.000001),
                        staticCandidateRadius);
                    float requiredViewScale = clamp(
                        targetStableRadiusMetres /
                            max(staticCandidateRadius, 0.000001),
                        1.0,
                        maximumViewScale);
                    float candidateViewScale = lerp(
                        1.0,
                        requiredViewScale,
                        stabilizationWeight);
                    float maximumStaticCandidateRadius =
                        spacing * 0.65 * candidateSizeMultiplier;
                    staticCandidateRadius = min(
                        staticCandidateRadius * candidateViewScale,
                        maximumStaticCandidateRadius);
                }

                // Readability admission is resolved from the fully formed,
                // view-stabilized radius before pulse and lifecycle. Candidates
                // that still cannot approach the authored screen-space target
                // fade out instead of surviving as distant pixel dirt. Zero
                // preserves the previous pure world-space behavior exactly.
                float readabilityVisibility = 1.0;
                [branch]
                if (stableRadiusPixels > 0.0001)
                {
                    float readableRadiusLowMetres =
                        targetStableRadiusMetres * 0.65;
                    readabilityVisibility = smoothstep(
                        readableRadiusLowMetres,
                        max(
                            targetStableRadiusMetres,
                            readableRadiusLowMetres + 0.000001),
                        staticCandidateRadius);
                }

                float radiusPulseScale = 1.0 +
                    sizePulseAmount *
                    sizePulseWave *
                    stableVariationAuthority;
                float candidateOuterRadius =
                    staticCandidateRadius *
                    lifecycleScale *
                    radiusPulseScale;
                // The adaptive rectangular search covers the complete authored
                // radius, the bounded 1.52x multi-axis geometry reach, and up to
                // 2.5 spacing of rigid lateral travel without increasing the
                // downstream search unnecessarily.
                candidateOuterRadius = min(
                    candidateOuterRadius,
                    spacing * 1.34);

                float contourSignA = secondaryHash < 0.5
                    ? -1.0
                    : 1.0;
                float contourSignB = tertiaryHash < 0.5
                    ? -1.0
                    : 1.0;
                float contourSignC = centreHashY < 0.5
                    ? -1.0
                    : 1.0;
                float contourMagnitudeA = lerp(
                    0.30,
                    0.52,
                    abs(secondaryHash * 2.0 - 1.0));
                float contourMagnitudeB = lerp(
                    0.19,
                    0.36,
                    abs(tertiaryHash * 2.0 - 1.0));
                float contourMagnitudeC = lerp(
                    0.13,
                    0.28,
                    abs(centreHashY * 2.0 - 1.0));
                float3 contourSetA = float3(
                    contourSignA * contourMagnitudeA,
                    contourSignB * contourMagnitudeB,
                    contourSignC * contourMagnitudeC);

                // Candidate-specific sine-harmonic directions form a genuine
                // two-dimensional geometry plane. They use existing hashes,
                // so meaningful morphing adds no texture reads or new identity
                // field and remains deterministic for the candidate lifetime.
                float3 rawMorphBasisU = float3(
                    secondaryHash * 2.0 - 1.0,
                    tertiaryHash * 2.0 - 1.0,
                    centreHashX * 2.0 - 1.0);
                float3 rawMorphBasisV = float3(
                    centreHashY * 2.0 - 1.0,
                    radiusHash * 2.0 - 1.0,
                    angleHash * 2.0 - 1.0);
                float3 morphBasisU;
                float3 morphBasisV;
                RiverWaterFoamResolveChipMorphBasis(
                    rawMorphBasisU,
                    rawMorphBasisV,
                    morphBasisU,
                    morphBasisV);

                float2 candidateDeltaMetres =
                    chipPointMetres - candidateCentre;
                float effectiveShapeChangeAmount =
                    saturate(chipShapeChangeAmount) *
                    stableVariationAuthority;
                float candidate = RiverWaterFoamSoftIrregularChip(
                    candidateDeltaMetres,
                    candidateOuterRadius,
                    antialiasMetres,
                    contourAxis,
                    shapeIrregularity,
                    contourSetA,
                    morphBasisU,
                    morphBasisV,
                    shapeMorphTrajectory,
                    effectiveShapeChangeAmount);

                // View stabilization is applied to each static candidate radius,
                // before pulse and lifecycle. Formation/Dissolve therefore keep
                // exact zero endpoints. Only the genuinely subpixel tail fades
                // to suppress unresolved birth/death sparks.
                float projectedRadiusPixels = candidateOuterRadius /
                    max(metresPerPixel, 0.000001);
                float subpixelVisibility = smoothstep(
                    0.25,
                    0.75,
                    projectedRadiusPixels);
                candidate *= readabilityVisibility *
                    subpixelVisibility *
                    step(0.000001, lifecycleScale);
                float active = step(activationHash, activation);
                float activeCandidate = candidate * active;
                // The single retained candidate diagnostic is the exact
                // activated field used by production before material permission.
                result.chipCandidateField = max(
                    result.chipCandidateField,
                    activeCandidate);

                // Interior Access is commonly zero, so its identity hash and
                // admission work are skipped entirely unless that optional
                // permission is authored. Admitted candidates retain their
                // complete connected contour instead of becoming pixel noise.
                [branch]
                if (interiorAccess > 0.0001)
                {
                    float interiorHash = RiverWaterFoamHash21(
                        cell + float2(11.89, 73.43));
                    float interiorAdmitted = lerp(
                        step(interiorHash, interiorAccess),
                        1.0,
                        step(0.9999, interiorAccess));
                    chipInteriorCandidates = max(
                        chipInteriorCandidates,
                        activeCandidate * interiorAdmitted);
                }
            }
        }
    }

    [branch]
    if (evaluateChipSelection > 0.5)
    {
        float edgeSelection = saturate(
            result.chipCandidateField * chipEligibility.edgeBand);
        float interiorSelection = saturate(
            chipInteriorCandidates * chipEligibility.interiorRegion);

        // One canonical material-permission model owns both territories.
        // Edge Width defines the projected boundary band; Interior Access may
        // admit complete deterministic candidates only in its complementary
        // established-body region. No independent material-depth threshold
        // remains.
        result.chipEdgeEligibility = saturate(
            chipEligibility.edgeBand);
        result.chipInteriorEligibility = saturate(
            chipEligibility.interiorRegion * interiorAccess);
        result.chipProductionSelection = saturate(max(
            edgeSelection,
            interiorSelection));
    }


    return result;
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
    return smoothstep(0.02, 0.10, saturate(presence));
}

struct RiverWaterFoamPatternFields
{
    float combined;
    // Raw normalized scale bands are retained transiently so dedicated Strand
    // shaping can simplify unresolved detail before any survival threshold is
    // evaluated.
    float2 scaleBands;
    // Stable broad fields provide low-frequency curvature modulation without a
    // second noise evaluation or a new pattern identity.
    float2 curvatureBands;
};

RiverWaterFoamPatternFields RiverWaterFoamStablePatternFields(
    float materialPattern,
    float storedGlobalDistance,
    float lateralMetres)
{
    RiverWaterFoamPatternFields fields;
    float seed = materialPattern * 43.731 + 11.17;
    float2 p = float2(storedGlobalDistance, lateralMetres);

    // Use several differently-oriented layers so the stored ribbon footprint
    // is not simply displayed as long parallel strokes. These coordinates are
    // storage-space metres, so the pattern rides with the material instead of
    // swimming in screen space. The coherent body uses broad/medium structure.
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

    // The coherent family deliberately stops at medium scale.
    fields.combined = saturate(
        materialPattern * 0.32 +
        broad * 0.27 +
        diagonal * 0.24 +
        mid * 0.17);

    // Normalize the broad and medium bands before Strand Scale interpolates
    // between them, keeping scale changes distinct from overall authority.
    float broadField = saturate(
        broad * 0.58 +
        diagonal * 0.42);
    float mediumPattern = saturate(
        (mid - 0.5) * 1.35 + 0.5);
    float broadPattern = saturate(
        (broadField - 0.5) * 2.0 + 0.5);
    fields.scaleBands = float2(
        mediumPattern,
        broadPattern);
    fields.curvatureBands = float2(
        broad,
        diagonal);

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
    float strandStrength,
    float strandScale,
    float strandReach,
    float2 projectedMetreFootprint,
    float projectedPatternSeedFootprint,
    out float coherentSoftVisibility,
    out float strandSoftVisibility,
    out float2 strandPattern,
    out float strandResolution)
{
    float s = saturate(sharpness);
    float life = saturate(remainingLife);
    float damage = 1.0 - life;

    float seed = materialPattern * 43.731 + 11.17;
    RiverWaterFoamPatternFields patternFields =
        RiverWaterFoamStablePatternFields(
            materialPattern,
            storedGlobalDistance,
            lateralMetres);
    float pattern = patternFields.combined;

    float2 p = float2(storedGlobalDistance, lateralMetres);
    float2 footprint = max(
        projectedMetreFootprint,
        float2(0.0001, 0.0001));
    float seedFootprint = max(
        projectedPatternSeedFootprint,
        0.0);

    // Strand resolution must include both river-coordinate variation and the
    // transported Material Pattern phase. The latter is multiplied by the same
    // seed coefficients used by each procedural source.
    float broadSpatialFootprint = max(
        footprint.x * 0.62,
        footprint.y * 1.75);
    float diagonalSpatialFootprint = max(
        footprint.x * 1.18 + footprint.y * 1.45,
        footprint.x * 0.34 + footprint.y * 2.80);
    float broadFootprint = max(
        broadSpatialFootprint + seedFootprint,
        diagonalSpatialFootprint + seedFootprint * 1.37);
    float midFootprint = max(
        footprint.x * 2.65 + footprint.y * 0.70,
        footprint.x * 0.52 + footprint.y * 4.60) +
        seedFootprint * 1.93;
    float bandFootprint = max(
        footprint.x * 1.85 + footprint.y * 3.25,
        footprint.x * 0.48 + footprint.y * 6.20) +
        seedFootprint * 2.19;

    float broadResolved = 1.0 - smoothstep(
        0.48,
        1.00,
        broadFootprint);
    float midResolved = 1.0 - smoothstep(
        0.38,
        0.82,
        midFootprint);
    float bandResolved = 1.0 - smoothstep(
        0.36,
        0.80,
        bandFootprint);


    // Strand Scale owns a broad-to-medium structural hierarchy.
    float strandDetail = 1.0 - saturate(strandScale);
    float mediumAuthority = strandDetail * midResolved;
    strandPattern = float2(
        saturate(lerp(
            patternFields.scaleBands.y,
            patternFields.scaleBands.x,
            mediumAuthority)),
        0.0);
    strandResolution = saturate(broadResolved);

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

    float coherentKeep = lerp(interiorKeep, edgeKeep, edgeExposure);

    // The anisotropic band family belongs exclusively to Strands.
    float2 bandLocal = float2(
        p.x * 1.85 + p.y * 3.25,
        p.y * 6.20 - p.x * 0.48);
    float2 bandCoordinate = bandLocal + seed * 2.19;
    float bandBreaker = RiverWaterFoamValueNoise(
        bandCoordinate);

    float strandLineifiedKeep = coherentKeep;
    [branch]
    if (saturate(strandStrength) > 0.0001)
    {
        // The anisotropic family is retained, but unresolved band detail falls
        // back to the same stable broad/diagonal organization. Reach changes
        // attenuation and eligible inward depth; it does not alter candidate
        // pattern density or scale.
        float broadBandFallback = saturate(
            patternFields.curvatureBands.x * 0.58 +
            patternFields.curvatureBands.y * 0.42);
        float resolvedStrandBand = lerp(
            broadBandFallback,
            bandBreaker,
            bandResolved);
        float strandBandKeep = smoothstep(
            0.20 + damage * 0.08,
            0.52 + damage * 0.12,
            resolvedStrandBand + pattern * 0.38 + baseMask * 0.24);

        float reach = saturate(strandReach);
        float attenuationFloor = lerp(
            0.90,
            0.66,
            reach);
        float presenceGuardLow = lerp(
            0.54,
            0.72,
            reach);
        float presenceGuardHigh = lerp(
            0.78,
            0.96,
            reach);
        float rawStrandLineifiedKeep = coherentKeep * lerp(
            attenuationFloor +
                strandBandKeep * (1.0 - attenuationFloor),
            1.0,
            smoothstep(
                presenceGuardLow,
                presenceGuardHigh,
                presence));
        strandLineifiedKeep = lerp(
            coherentKeep,
            rawStrandLineifiedKeep,
            strandResolution);
    }
    else
    {
        strandPattern = float2(0.0, 0.0);
        strandResolution = 0.0;
    }

    float coherentVisible = baseMask * coherentKeep;
    float strandVisible = baseMask * strandLineifiedKeep;

    // Preserve only compact, pattern-supported cores. All transient signals
    // share the same core and lifecycle authority; they differ only in their
    // explicit render-only deterioration vocabulary.
    float compactCore = smoothstep(0.66, 0.91, presence) *
        smoothstep(0.22 + damage * 0.16, 0.58 + damage * 0.12, pattern + morph * 0.10);
    float protectedCore = compactCore * lerp(0.72, 0.92, s);
    coherentVisible = max(coherentVisible, protectedCore);
    strandVisible = max(strandVisible, protectedCore);

    // Near-zero Remaining Life may disappear, but ordinary aging does not
    // globally fade the whole patch. It erodes through the thresholds above.
    float lifeGate = smoothstep(0.015, 0.070, life);
    coherentVisible *= lifeGate;
    strandVisible *= lifeGate;

    coherentSoftVisibility = saturate(coherentVisible);
    strandSoftVisibility = saturate(strandVisible);
    float hardVisible = smoothstep(0.22, 0.58, coherentSoftVisibility);
    float fringe = smoothstep(0.06, 0.34, coherentSoftVisibility) * 0.34;
    return saturate(max(hardVisible, fringe));
}

float RiverWaterFoamHardenSoftVisibility(
    float softVisibility)
{
    float softShape = saturate(softVisibility);
    float hardVisible = smoothstep(
        0.22,
        0.58,
        softShape);
    float fringe = smoothstep(
        0.06,
        0.34,
        softShape) * 0.34;
    return saturate(max(
        hardVisible,
        fringe));
}


float RiverWaterFoamResolveStructuralStrandKeep(
    float softShape,
    float chipPattern,
    float strandStrength,
    float strandDensity,
    float strandReach,
    float visibilityAA,
    float exactCore)
{
    float pattern = saturate(chipPattern);
    float patternAA = max(
        fwidth(pattern),
        0.0015);
    float density = saturate(strandDensity);
    float selectionLow = lerp(
        0.68,
        0.42,
        density);
    float selectionHigh = selectionLow + 0.18;
    float selection = smoothstep(
        selectionLow - patternAA,
        selectionHigh + patternAA,
        pattern);
    float authority = saturate(
        saturate(strandStrength) * selection);
    float maximumDepth = lerp(
        0.52,
        0.98,
        saturate(strandReach));
    float threshold = lerp(
        0.16,
        maximumDepth,
        authority);
    float cut = smoothstep(
        threshold - visibilityAA,
        threshold + visibilityAA,
        softShape);
    float keep = lerp(
        1.0,
        cut,
        smoothstep(0.001, 0.08, authority));
    return max(keep, exactCore);
}

float RiverWaterFoamApplyChipAndStrands(
    float hardenedShape,
    float coherentSoftVisibility,
    float strandSoftVisibility,
    float2 strandPattern,
    float strandResolution,
    float productionChipSelection,
    float strandStrength,
    float strandDensity,
    float strandReach,
    out float productionChipRemovedMask)
{
    float shape = saturate(hardenedShape);
    float coherentSoftShape = saturate(coherentSoftVisibility);
    float strandSoftShape = saturate(strandSoftVisibility);
    float productionChip = saturate(productionChipSelection);
    float strand = saturate(strandStrength);
    productionChipRemovedMask = 0.0;

    [branch]
    if (shape <= 0.0001)
    {
        return shape;
    }

    if (coherentSoftShape <= 0.0001)
    {
        return 0.0;
    }

    float exactCore = step(0.999, coherentSoftShape);
    float postChipSoftShape = coherentSoftShape;
    float postChipMask = shape;

    // Production Chip changes the soft body before Strands, then reconstructs
    // the accepted hardened mask through a ratio so neutral regions remain
    // exactly equivalent to the accepted baseline.
    [branch]
    if (productionChip > 0.0001)
    {
        postChipSoftShape = saturate(
            coherentSoftShape * (1.0 - productionChip));
        float baselineSoftMask = RiverWaterFoamHardenSoftVisibility(
            coherentSoftShape);
        float modifiedSoftMask = RiverWaterFoamHardenSoftVisibility(
            postChipSoftShape);
        float reconstructedRatio = baselineSoftMask > 0.0001
            ? saturate(modifiedSoftMask / baselineSoftMask)
            : 1.0;
        postChipMask = saturate(shape * reconstructedRatio);
        productionChipRemovedMask = saturate(shape - postChipMask);
    }

    // Strands own structural anisotropic lineification after Chipping.
    float resolvedStrandStrength = strand * saturate(strandResolution);
    float strandAA = max(fwidth(strandSoftShape), 0.001);
    float strandKeep = RiverWaterFoamResolveStructuralStrandKeep(
        strandSoftShape,
        strandPattern.x,
        resolvedStrandStrength,
        strandDensity,
        strandReach,
        strandAA,
        exactCore);

    return saturate(postChipMask * strandKeep);
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

bool RiverWaterFoamUsesFixedMetricLattice(float4 gridContract)
{
    return (int)round(gridContract.z) == 1;
}

float2 RiverWaterFoamResolveFieldUV(
    float globalDistance,
    float lateralMetres,
    float surfaceHalfWidth,
    float globalStart,
    float fieldLength,
    float4 gridContract,
    float4 gridSpacing,
    float4 gridLateral,
    float4 gridLongitudinal)
{
    if (!RiverWaterFoamUsesFixedMetricLattice(gridContract))
    {
        return float2(
            saturate((globalDistance - globalStart) /
                max(0.001, fieldLength)),
            saturate(lateralMetres /
                max(0.001, surfaceHalfWidth) * 0.5 + 0.5));
    }

    float localDistanceMetres = globalDistance - globalStart;
    float globalY = (lateralMetres - gridLateral.x) /
        max(0.0001, gridSpacing.w);
    return saturate(float2(
        (localDistanceMetres - gridLongitudinal.x) /
            max(0.0001, gridLongitudinal.y),
        (globalY - gridLateral.y + 0.5) /
            max(1.0, gridLateral.z)));
}

bool RiverWaterFoamPointInsideValidField(
    float globalDistance,
    float lateralMetres,
    float globalStart,
    float4 gridContract,
    float4 gridLongitudinal,
    float4 gridExtent)
{
    if (!RiverWaterFoamUsesFixedMetricLattice(gridContract))
    {
        return true;
    }

    float localDistanceMetres = globalDistance - globalStart;
    float longitudinalMinimum = gridLongitudinal.x;
    float longitudinalMaximum = longitudinalMinimum +
        max(0.0, gridLongitudinal.z);
    return localDistanceMetres >= longitudinalMinimum - 0.0001 &&
        localDistanceMetres <= longitudinalMaximum + 0.0001 &&
        lateralMetres >= gridExtent.x - 0.0001 &&
        lateralMetres <= gridExtent.y + 0.0001;
}

bool RiverWaterFoamFieldUVInsideValidSample(
    float2 fieldUV,
    float4 gridContract,
    float4 gridLongitudinal)
{
    if (!RiverWaterFoamUsesFixedMetricLattice(gridContract))
    {
        return true;
    }

    float validU = max(0.0, gridLongitudinal.z) /
        max(0.0001, gridLongitudinal.y);
    return fieldUV.x >= -0.0001 &&
        fieldUV.x <= validU + 0.0001 &&
        fieldUV.y >= -0.0001 &&
        fieldUV.y <= 1.0001;
}


float RiverWaterFoamFieldUVToFilmUV1D(
    float fieldUV,
    int structuralCount,
    int filmCount)
{
    int safeStructuralCount = max(1, structuralCount);
    int safeFilmCount = max(1, filmCount);
    float structuralPosition =
        saturate(fieldUV) * (float)safeStructuralCount;
    int filmIndex = min(
        (int)floor(structuralPosition * 0.5),
        safeFilmCount - 1);
    int structuralStart = max(0, filmIndex * 2);
    int representedCount = clamp(
        safeStructuralCount - structuralStart,
        1,
        2);
    float localPosition = saturate(
        (structuralPosition - (float)structuralStart) /
        (float)representedCount);
    return ((float)filmIndex + localPosition) /
        (float)safeFilmCount;
}

float2 RiverWaterFoamFieldUVToFilmUV(
    float2 fieldUV,
    float2 structuralDimensions,
    float2 filmDimensions,
    float4 gridContract)
{
    if (!RiverWaterFoamUsesFixedMetricLattice(gridContract))
    {
        return saturate(fieldUV);
    }

    int2 structural = max(int2(1, 1), (int2)round(structuralDimensions));
    int2 film = max(int2(1, 1), (int2)round(filmDimensions));
    return float2(
        RiverWaterFoamFieldUVToFilmUV1D(
            fieldUV.x,
            structural.x,
            film.x),
        RiverWaterFoamFieldUVToFilmUV1D(
            fieldUV.y,
            structural.y,
            film.y));
}

float2 RiverWaterFoamMetresToFieldUV(
    float2 metres,
    float fieldLength,
    float surfaceHalfWidth,
    float4 gridContract,
    float4 gridSpacing,
    float4 gridLateral,
    float4 gridLongitudinal)
{
    if (!RiverWaterFoamUsesFixedMetricLattice(gridContract))
    {
        return float2(
            metres.x / max(0.001, fieldLength),
            metres.y / max(0.001, surfaceHalfWidth * 2.0));
    }

    return float2(
        metres.x / max(0.0001, gridLongitudinal.y),
        metres.y / max(0.0001, gridLateral.z * gridSpacing.w));
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
    float strandStrength,
    float strandScale,
    float strandReach,
    float2 projectedMetreFootprint,
    float projectedPatternSeedFootprint,
    out float coherentSoftVisibility,
    out float strandSoftVisibility,
    out float presence,
    out float remainingLife,
    out float materialPattern,
    out float2 strandPattern,
    out float strandResolution)
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
        strandStrength,
        strandScale,
        strandReach,
        projectedMetreFootprint,
        projectedPatternSeedFootprint,
        coherentSoftVisibility,
        strandSoftVisibility,
        strandPattern,
        strandResolution);
}

struct RiverWaterFoamResult
{
    float presence;
    float remainingLife;
    float materialPattern;
    float mask;
    float softVisibility;
    float strandSoftVisibility;
    float surfaceEnergy;
    float2 strandPattern;
    float strandResolution;
    float validField;
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
    float strandStrength,
    float strandScale,
    float strandReach,
    float freezeAmount,
    float4 gridContract,
    float4 gridSpacing,
    float4 gridLateral,
    float4 gridLongitudinal,
    float4 gridExtent,
    RiverWaterFoamSurfaceInfluence surfaceInfluence)
{
    RiverWaterFoamResult result;
    result.presence = 0.0;
    result.remainingLife = 0.0;
    result.materialPattern = 0.0;
    result.mask = 0.0;
    result.softVisibility = 0.0;
    result.strandSoftVisibility = 0.0;
    result.surfaceEnergy = 0.0;
    result.strandPattern = 0.0;
    result.strandResolution = 0.0;
    result.validField = 0.0;
    result.fieldUV = 0.0;
    result.materialUV = 0.0;

    if (enabled < 0.5 || fieldLength <= 0.0001)
    {
        return result;
    }
    if (!RiverWaterFoamPointInsideValidField(
            globalDistance,
            lateralMetres,
            globalStart,
            gridContract,
            gridLongitudinal,
            gridExtent))
    {
        return result;
    }

    result.validField = 1.0;

    float liquidFactor = 1.0 - saturate(freezeAmount);
    float surfaceEnergy = RiverWaterFoamResolveSurfaceEnergy(
        surfaceInfluence) * liquidFactor;

    // Derivatives are resolved once before any wake/lee branch and reused for
    // every stored, warped, lead, and trail evaluation. Strong surface coupling
    // can duplicate/compress linework, so it receives a conservative allowance
    // that simplifies Strands slightly earlier around wakes and obstacles.
    float2 projectedMetreFootprint = max(
        fwidth(float2(globalDistance, lateralMetres)),
        float2(0.0001, 0.0001));
    projectedMetreFootprint *= lerp(
        1.0,
        1.35,
        surfaceEnergy);

    float2 fieldUV = RiverWaterFoamResolveFieldUV(
        globalDistance,
        lateralMetres,
        surfaceHalfWidth,
        globalStart,
        fieldLength,
        gridContract,
        gridSpacing,
        gridLateral,
        gridLongitudinal);
    // The previous/current committed Layer C pair is the production
    // presentation authority. Ordinary fixed-step interpolation hides material
    // cadence without restoring point-velocity residual backtracing across
    // conservative closed faces.
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

    // Material Pattern participates directly in every procedural Strand phase.
    // Measure its screen-space variation once outside wake/lee branches so the
    // resolution policy does not incorrectly classify rapidly changing seed
    // phase as resolved merely because river coordinates are smooth.
    float previewPresence;
    float previewRemainingLife;
    float previewMaterialPattern;
    RiverWaterFoamDecodeMaterialState(
        storedState,
        previewPresence,
        previewRemainingLife,
        previewMaterialPattern);
    float projectedPatternSeedFootprint =
        fwidth(previewMaterialPattern) * 43.731 *
        smoothstep(0.02, 0.16, previewPresence) *
        lerp(1.0, 1.35, surfaceEnergy);

    float storedSoftVisibility;
    float storedStrandSoftVisibility;
    float storedPresence;
    float storedRemainingLife;
    float storedMaterialPattern;
    float2 storedStrandPattern;
    float storedStrandResolution;
    float storedMask = RiverWaterFoamResolveStateMask(
        storedState,
        storedGlobalDistance,
        storedLateralMetres,
        sharpness,
        finalVisibilityMode,
        strandStrength,
        strandScale,
        strandReach,
        projectedMetreFootprint,
        projectedPatternSeedFootprint,
        storedSoftVisibility,
        storedStrandSoftVisibility,
        storedPresence,
        storedRemainingLife,
        storedMaterialPattern,
        storedStrandPattern,
        storedStrandResolution);

    // Normal rendering and raw Layer C diagnostics now share the committed
    // field coordinate. Surface warp below remains visual-only and bounded.
    result.presence = storedPresence;
    result.remainingLife = storedRemainingLife;
    result.materialPattern = storedMaterialPattern;
    result.fieldUV = fieldUV;
    result.materialUV = foamUV;

    float2 warpMetres = RiverWaterFoamResolveSurfaceWarpMetres(
        surfaceInfluence,
        globalDistance,
        lateralMetres,
        surfaceHalfWidth,
        storedMaterialPattern);
    float2 warpUV = RiverWaterFoamMetresToFieldUV(
        warpMetres,
        fieldLength,
        surfaceHalfWidth,
        gridContract,
        gridSpacing,
        gridLateral,
        gridLongitudinal);
    float2 visualFoamUVRaw = foamUV - warpUV;
    bool visualFoamSampleValid = RiverWaterFoamFieldUVInsideValidSample(
        visualFoamUVRaw,
        gridContract,
        gridLongitudinal);
    float2 visualFoamUV = saturate(visualFoamUVRaw);

    float4 visualState = visualFoamSampleValid
        ? RiverWaterFoamSampleInterpolatedState(
            TEXTURE2D_ARGS(
                previousFoam,
                previousFoamSampler),
            TEXTURE2D_ARGS(
                currentFoam,
                currentFoamSampler),
            visualFoamUV,
            blend)
        : 0.0.xxxx;

    float visualSoftVisibility;
    float visualStrandSoftVisibility;
    float visualPresence;
    float visualRemainingLife;
    float visualMaterialPattern;
    float2 visualStrandPattern;
    float visualStrandResolution;
    float visualMask = RiverWaterFoamResolveStateMask(
        visualState,
        storedGlobalDistance - warpMetres.x,
        storedLateralMetres - warpMetres.y,
        sharpness,
        finalVisibilityMode,
        strandStrength,
        strandScale,
        strandReach,
        projectedMetreFootprint,
        projectedPatternSeedFootprint,
        visualSoftVisibility,
        visualStrandSoftVisibility,
        visualPresence,
        visualRemainingLife,
        visualMaterialPattern,
        visualStrandPattern,
        visualStrandResolution);

    float surfaceCoupling = saturate(surfaceEnergy * 0.72);
    float coupledMask = lerp(
        storedMask,
        visualMask,
        surfaceCoupling);
    float coupledSoftVisibility = lerp(
        storedSoftVisibility,
        visualSoftVisibility,
        surfaceCoupling);
    float coupledStrandSoftVisibility = lerp(
        storedStrandSoftVisibility,
        visualStrandSoftVisibility,
        surfaceCoupling);
    float2 coupledStrandPattern = lerp(
        storedStrandPattern,
        visualStrandPattern,
        surfaceCoupling);
    float coupledStrandResolution = lerp(
        storedStrandResolution,
        visualStrandResolution,
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
            surfaceHalfWidth,
            gridContract,
            gridSpacing,
            gridLateral,
            gridLongitudinal);

        float2 leadFoamUVRaw = visualFoamUVRaw - stretchUV;
        float2 trailFoamUVRaw = visualFoamUVRaw + stretchUV;
        bool leadSampleValid = RiverWaterFoamFieldUVInsideValidSample(
            leadFoamUVRaw,
            gridContract,
            gridLongitudinal);
        bool trailSampleValid = RiverWaterFoamFieldUVInsideValidSample(
            trailFoamUVRaw,
            gridContract,
            gridLongitudinal);
        float4 leadState = leadSampleValid
            ? RiverWaterFoamSampleInterpolatedState(
                TEXTURE2D_ARGS(
                    previousFoam,
                    previousFoamSampler),
                TEXTURE2D_ARGS(
                    currentFoam,
                    currentFoamSampler),
                saturate(leadFoamUVRaw),
                blend)
            : 0.0.xxxx;
        float4 trailState = trailSampleValid
            ? RiverWaterFoamSampleInterpolatedState(
                TEXTURE2D_ARGS(
                    previousFoam,
                    previousFoamSampler),
                TEXTURE2D_ARGS(
                    currentFoam,
                    currentFoamSampler),
                saturate(trailFoamUVRaw),
                blend)
            : 0.0.xxxx;

        float leadSoftVisibility;
        float leadStrandSoftVisibility;
        float leadPresence;
        float leadLife;
        float leadPattern;
        float2 leadStrandPattern;
        float leadStrandResolution;
        float leadMask = RiverWaterFoamResolveStateMask(
            leadState,
            storedGlobalDistance - warpMetres.x - stretchDirection.x * stretchMetres,
            storedLateralMetres - warpMetres.y - stretchDirection.y * stretchMetres,
            sharpness,
            finalVisibilityMode,
            strandStrength,
            strandScale,
            strandReach,
            projectedMetreFootprint,
            projectedPatternSeedFootprint,
            leadSoftVisibility,
            leadStrandSoftVisibility,
            leadPresence,
            leadLife,
            leadPattern,
            leadStrandPattern,
            leadStrandResolution);
        float trailSoftVisibility;
        float trailStrandSoftVisibility;
        float trailPresence;
        float trailLife;
        float trailPattern;
        float2 trailStrandPattern;
        float trailStrandResolution;
        float trailMask = RiverWaterFoamResolveStateMask(
            trailState,
            storedGlobalDistance - warpMetres.x + stretchDirection.x * stretchMetres,
            storedLateralMetres - warpMetres.y + stretchDirection.y * stretchMetres,
            sharpness,
            finalVisibilityMode,
            strandStrength,
            strandScale,
            strandReach,
            projectedMetreFootprint,
            projectedPatternSeedFootprint,
            trailSoftVisibility,
            trailStrandSoftVisibility,
            trailPresence,
            trailLife,
            trailPattern,
            trailStrandPattern,
            trailStrandResolution);

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
        float leadScaledStrandSoft =
            leadStrandSoftVisibility * stretchScale;
        float trailScaledStrandSoft =
            trailStrandSoftVisibility * stretchScale;
        float trailOwnsStretch = step(
            leadScaledStrandSoft,
            trailScaledStrandSoft);
        float dominantStretchSoft = max(
            leadScaledStrandSoft,
            trailScaledStrandSoft);
        float2 dominantStretchPattern = lerp(
            leadStrandPattern,
            trailStrandPattern,
            trailOwnsStretch);
        float dominantStretchResolution = lerp(
            leadStrandResolution,
            trailStrandResolution,
            trailOwnsStretch);
        float stretchOwnsStrand = step(
            coupledStrandSoftVisibility,
            dominantStretchSoft);
        float stretchedStrandSoftVisibility = max(
            coupledStrandSoftVisibility,
            dominantStretchSoft);
        float2 stretchedStrandPattern = lerp(
            coupledStrandPattern,
            dominantStretchPattern,
            stretchOwnsStrand);
        float stretchedStrandResolution = lerp(
            coupledStrandResolution,
            dominantStretchResolution,
            stretchOwnsStrand);
        coupledMask = lerp(
            coupledMask,
            stretchedMask,
            stretchWeight);
        coupledSoftVisibility = lerp(
            coupledSoftVisibility,
            stretchedSoftVisibility,
            stretchWeight);
        coupledStrandSoftVisibility = lerp(
            coupledStrandSoftVisibility,
            stretchedStrandSoftVisibility,
            stretchWeight);
        coupledStrandPattern = lerp(
            coupledStrandPattern,
            stretchedStrandPattern,
            stretchWeight);
        coupledStrandResolution = lerp(
            coupledStrandResolution,
            stretchedStrandResolution,
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
    coupledStrandSoftVisibility *= surfaceBreakMultiplier;

    // Do not allow render coupling to erase coherent stored material. It may
    // visually bend/thin edges, but lifecycle remains in the material field.
    float storedRetention = lerp(
        0.72,
        0.58,
        saturate(surfaceEnergy));
    coupledMask = max(
        coupledMask,
        storedMask * storedRetention);
    float retainedStoredSoft =
        storedSoftVisibility * storedRetention;
    coupledSoftVisibility = max(
        coupledSoftVisibility,
        retainedStoredSoft);
    float retainedStoredStrandSoft =
        storedStrandSoftVisibility * storedRetention;
    float storedOwnsRetainedStrand = step(
        coupledStrandSoftVisibility,
        retainedStoredStrandSoft);
    coupledStrandSoftVisibility = max(
        coupledStrandSoftVisibility,
        retainedStoredStrandSoft);
    coupledStrandPattern = lerp(
        coupledStrandPattern,
        storedStrandPattern,
        storedOwnsRetainedStrand);
    coupledStrandResolution = lerp(
        coupledStrandResolution,
        storedStrandResolution,
        storedOwnsRetainedStrand);
    coupledMask *= liquidFactor;
    coupledSoftVisibility *= liquidFactor;
    coupledStrandSoftVisibility *= liquidFactor;

    result.mask = saturate(coupledMask);
    result.softVisibility = saturate(coupledSoftVisibility);
    result.strandSoftVisibility = saturate(
        coupledStrandSoftVisibility);
    result.strandPattern = saturate(coupledStrandPattern);
    result.strandResolution = saturate(coupledStrandResolution);
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
    float baseCoverage = RiverWaterFoamResolveBaseCoverage(safeFoamMask);
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
