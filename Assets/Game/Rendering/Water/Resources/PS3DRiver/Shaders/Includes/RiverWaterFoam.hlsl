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


struct RiverWaterFoamSelectionDiagnostics
{
    float chipCandidateField;
    float chipActivatedCandidates;
    float chipEdgeEligibility;
    float chipFinalSelection;
    float chipMaterialGate;
    float chipProductionSelection;
    float frayPermittedBand;
    float frayPatternPreview;
};

float RiverWaterFoamSoftIrregularChip(
    float2 deltaMetres,
    float outerRadiusMetres,
    float antialiasMetres,
    float2 contourAxis,
    float shapeIrregularity,
    float3 contourCoefficients)
{
    float outerRadius = max(0.001, outerRadiusMetres);
    float aa = max(0.0005, antialiasMetres);
    float distanceToCentre = length(deltaMetres);
    float2 direction = distanceToCentre > 0.00001
        ? deltaMetres / distanceToCentre
        : contourAxis;
    float2 perpendicularAxis = float2(
        -contourAxis.y,
        contourAxis.x);
    float localX = dot(direction, contourAxis);
    float localY = dot(direction, perpendicularAxis);

    // One connected star-shaped contour replaces the former union of three
    // circles. Low-order directional harmonics create broad asymmetry without
    // introducing detached lobes or a rigid orbiting cluster. A bounded
    // normalization and outer cap keep Candidate Radius authoritative.
    float harmonic1 = localX;
    float harmonic2 = localX * localX - localY * localY;
    float harmonic3 = localX * (
        localX * localX - 3.0 * localY * localY);
    float irregularity = saturate(shapeIrregularity);
    float contourDelta =
        contourCoefficients.x * harmonic1 +
        contourCoefficients.y * harmonic2 +
        contourCoefficients.z * harmonic3;
    float contourEnvelope = 1.0 + irregularity * 0.30 * (
        abs(contourCoefficients.x) +
        abs(contourCoefficients.y) +
        abs(contourCoefficients.z));
    float radialScale = saturate(
        max(0.24, 1.0 + irregularity * contourDelta) /
        max(1.0, contourEnvelope));
    float localRadius = outerRadius * radialScale;

    return 1.0 - smoothstep(
        localRadius - aa,
        localRadius + aa,
        distanceToCentre);
}

float RiverWaterFoamSmoothPeriodicWave(float phase)
{
    float cycle = frac(phase);
    float triangleWave = 1.0 - abs(cycle * 2.0 - 1.0);
    return triangleWave * triangleWave * (3.0 - 2.0 * triangleWave);
}

void RiverWaterFoamResolveShapePreservingChipBasis(
    float2 sourceDx,
    float2 sourceDy,
    float2 evolvedDx,
    float2 evolvedDy,
    out float2 sourceFromEvolvedX,
    out float2 sourceFromEvolvedY)
{
    // The animated coordinate field controls candidate lookup and centre
    // motion. It must not also shear the local chip contour. Build one local
    // evolved-to-source metric transform per fragment; every candidate reuses
    // it, so shape correction does not repeat matrix inversion in the 3x3 loop.
    float sourceDeterminant =
        sourceDx.x * sourceDy.y - sourceDy.x * sourceDx.y;
    float evolvedDeterminant =
        evolvedDx.x * evolvedDy.y - evolvedDy.x * evolvedDx.y;
    float determinantSign = evolvedDeterminant < 0.0 ? -1.0 : 1.0;
    float minimumDeterminant = max(
        abs(sourceDeterminant) * 0.08,
        0.00000001);
    float safeDeterminant = determinantSign * max(
        abs(evolvedDeterminant),
        minimumDeterminant);

    sourceFromEvolvedX = (
        sourceDx * evolvedDy.y -
        sourceDy * evolvedDx.y) / safeDeterminant;
    sourceFromEvolvedY = (
        -sourceDx * evolvedDy.x +
        sourceDy * evolvedDx.x) / safeDeterminant;
}

float2 RiverWaterFoamApplyShapePreservingChipBasis(
    float2 evolvedDeltaMetres,
    float2 sourceFromEvolvedX,
    float2 sourceFromEvolvedY)
{
    float2 sourceDeltaMetres =
        sourceFromEvolvedX * evolvedDeltaMetres.x +
        sourceFromEvolvedY * evolvedDeltaMetres.y;

    // Near a fold, a raw inverse can become arbitrarily large. Cap only the
    // correction magnitude; direction and local aspect correction remain.
    float evolvedLength = max(length(evolvedDeltaMetres), 0.000001);
    float correctedLength = length(sourceDeltaMetres);
    float maximumCorrectedLength = max(evolvedLength * 6.0, 0.001);
    sourceDeltaMetres *= min(
        1.0,
        maximumCorrectedLength / max(correctedLength, 0.000001));
    return sourceDeltaMetres;
}

float RiverWaterFoamResolveChipMaterialGate(
    float materialPattern)
{
    // Material Pattern is transported with Layer C. Two smooth harmonics turn
    // it into a broad, stable eligibility signal without another texture or a
    // time-varying reseed. World-space candidates therefore become active only
    // where eligible material is currently passing through them.
    float phase = saturate(materialPattern);
    float broad = sin(phase * 12.5663706144 + 1.37) * 0.5 + 0.5;
    float secondary = sin(phase * 21.9911485751 + 4.11) * 0.5 + 0.5;
    float signal = saturate(broad * 0.68 + secondary * 0.32);
    return smoothstep(0.34, 0.68, signal);
}

RiverWaterFoamSelectionDiagnostics
RiverWaterFoamEvaluateSelectionDiagnostics(
    float materialPattern,
    float storedGlobalDistance,
    float lateralMetres,
    float materialEdgeDepth,
    float evaluateChipSelection,
    float evaluateChipCandidates,
    float evaluateCandidatesOutsideMaterial,
    float evaluateFrayDiagnostics,
    float chipActivation,
    float chipCandidateSpacing,
    float chipDistributionIrregularity,
    float chipRadiusRatio,
    float chipSizeIrregularity,
    float chipShapeIrregularity,
    float chipSelectionDepth,
    float chipFieldSpeed,
    float chipEvolutionRate,
    float chipEvolutionAmount,
    float chipEvolutionTime,
    float fraySelectionDepth,
    float frayWavelength,
    float frayDepth)
{
    RiverWaterFoamSelectionDiagnostics result;
    result.chipCandidateField = 0.0;
    result.chipActivatedCandidates = 0.0;
    result.chipEdgeEligibility = 0.0;
    result.chipFinalSelection = 0.0;
    result.chipMaterialGate = 0.0;
    result.chipProductionSelection = 0.0;
    result.frayPermittedBand = 0.0;
    result.frayPatternPreview = 0.0;

    [branch]
    if (max(evaluateChipSelection, evaluateFrayDiagnostics) <= 0.5)
    {
        return result;
    }

    float2 pointMetres = float2(
        storedGlobalDistance,
        lateralMetres);
    float evolutionTime = max(0.0, chipEvolutionTime);
    float2 chipPointMetres = float2(
        storedGlobalDistance -
            max(0.0, chipFieldSpeed) * evolutionTime,
        lateralMetres);
    float edgeDepth = saturate(materialEdgeDepth);
    float edgeDepthAA = max(
        fwidth(edgeDepth),
        0.001);
    float materialBody = smoothstep(
        0.015 - edgeDepthAA,
        0.10 + edgeDepthAA,
        edgeDepth);
    float spacingForEvolution = max(0.10, chipCandidateSpacing);
    float evolutionAmountForField = saturate(chipEvolutionAmount);
    float evolutionRateForField = max(0.0, chipEvolutionRate);
    float2 evolvedChipPointMetres = chipPointMetres;
    float candidateFieldRequired =
        (evaluateChipCandidates > 0.5 &&
            (materialBody > 0.0001 ||
                evaluateCandidatesOutsideMaterial > 0.5))
        ? 1.0
        : 0.0;

    [branch]
    if (evolutionAmountForField > 0.0001 &&
        candidateFieldRequired > 0.5)
    {
        // B.2B replaces tiny candidate-local centre offsets with a continuous
        // animated coordinate field. A broad and a finer independently moving
        // value-noise layer produce multi-spacing downstream/lateral travel,
        // compression, and release. Because candidate lookup occurs after the
        // warp, the fixed 3x3 lattice search remains sufficient regardless of
        // the displacement magnitude.
        float fieldClock = evolutionTime * evolutionRateForField;
        float2 coarseDomain =
            chipPointMetres / (spacingForEvolution * 4.10);
        float2 fineDomain =
            chipPointMetres / (spacingForEvolution * 1.65);
        float coarseDownstream =
            RiverWaterFoamValueNoise(
                coarseDomain +
                float2(fieldClock * 0.31, -fieldClock * 0.23)) *
            2.0 - 1.0;
        float coarseLateral =
            RiverWaterFoamValueNoise(
                float2(-coarseDomain.y, coarseDomain.x) +
                float2(-fieldClock * 0.27, fieldClock * 0.37) +
                float2(19.73, 19.73)) *
            2.0 - 1.0;
        float fineDownstream =
            RiverWaterFoamValueNoise(
                fineDomain +
                float2(-fieldClock * 0.53, fieldClock * 0.41) +
                float2(47.19, 47.19)) *
            2.0 - 1.0;
        float fineLateral =
            RiverWaterFoamValueNoise(
                float2(fineDomain.y, -fineDomain.x) +
                float2(fieldClock * 0.47, fieldClock * 0.59) +
                float2(83.61, 83.61)) *
            2.0 - 1.0;
        float2 fieldDisplacementSpacings = float2(
            coarseDownstream * 2.75 + fineDownstream * 1.15,
            coarseLateral * 4.60 + fineLateral * 1.90);
        evolvedChipPointMetres +=
            fieldDisplacementSpacings *
            spacingForEvolution *
            evolutionAmountForField;
    }

    [branch]
    if (evaluateChipCandidates > 0.5)
    {
        float2 sourcePointDx = ddx(chipPointMetres);
        float2 sourcePointDy = ddy(chipPointMetres);
        float2 evolvedPointDx = ddx(evolvedChipPointMetres);
        float2 evolvedPointDy = ddy(evolvedChipPointMetres);
        float2 sourceFromEvolvedX = float2(1.0, 0.0);
        float2 sourceFromEvolvedY = float2(0.0, 1.0);
        [branch]
        if (evolutionAmountForField > 0.0001)
        {
            RiverWaterFoamResolveShapePreservingChipBasis(
                sourcePointDx,
                sourcePointDy,
                evolvedPointDx,
                evolvedPointDy,
                sourceFromEvolvedX,
                sourceFromEvolvedY);
        }

        float2 pointFootprint = max(
            fwidth(chipPointMetres),
            float2(0.0001, 0.0001));
        float antialiasMetres = max(
            pointFootprint.x,
            pointFootprint.y);

        [branch]
        if (materialBody > 0.0001 ||
            evaluateCandidatesOutsideMaterial > 0.5)
        {
            float spacing = max(0.10, chipCandidateSpacing);
            float radiusRatio = clamp(chipRadiusRatio, 0.05, 0.65);
            float nominalRadius = spacing * radiusRatio;
            float distributionIrregularity = saturate(
                chipDistributionIrregularity);
            float sizeIrregularity = saturate(
                chipSizeIrregularity);
            float shapeIrregularity = saturate(
                chipShapeIrregularity);
            float evolutionAmount = saturate(chipEvolutionAmount);
            float evolutionRate = max(0.0, chipEvolutionRate);
            float candidateClock = evolutionTime * evolutionRate;
            float2 baseCell = floor(evolvedChipPointMetres / spacing);

            [unroll]
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                [unroll]
                for (int offsetY = -1; offsetY <= 1; offsetY++)
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

                    float turnoverEnvelope = 1.0;
                    float geometricTurnoverScale = 1.0;
                    float radiusPulseScale = 1.0;
                    float contourMorph = 0.0;
                    [branch]
                    if (evolutionAmount > 0.0001)
                    {
                        // Candidate lifecycle now changes geometry, not merely
                        // opacity. Every cell owns independent turnover and
                        // morph rates, producing visible growth/shrink and
                        // preventing neighbouring candidates from remaining
                        // synchronized after the coordinate field brings them
                        // together.
                        float turnoverRate = lerp(
                            0.43,
                            1.79,
                            activationHash);
                        float turnoverPhase = frac(
                            candidateClock * turnoverRate +
                            activationHash * 5.17 +
                            tertiaryHash * 0.73);
                        float fadeInEnd = lerp(
                            0.10,
                            0.23,
                            secondaryHash);
                        float fadeOutStart = lerp(
                            0.63,
                            0.86,
                            centreHashX);
                        float lifecyclePresence =
                            smoothstep(0.0, fadeInEnd, turnoverPhase) *
                            (1.0 - smoothstep(
                                fadeOutStart,
                                1.0,
                                turnoverPhase));
                        float lifecycleGrowth = pow(
                            saturate(lifecyclePresence),
                            0.72);
                        float targetTurnoverScale = lerp(
                            0.13,
                            1.08,
                            lifecycleGrowth);
                        geometricTurnoverScale = lerp(
                            1.0,
                            targetTurnoverScale,
                            evolutionAmount);
                        turnoverEnvelope = lerp(
                            1.0,
                            smoothstep(
                                0.025,
                                0.28,
                                lifecyclePresence),
                            evolutionAmount);

                        float radiusPulse =
                            RiverWaterFoamSmoothPeriodicWave(
                                candidateClock * lerp(
                                    0.61,
                                    1.91,
                                    radiusHash) +
                                radiusHash * 6.37 +
                                centreHashY * 1.11) *
                            2.0 - 1.0;
                        radiusPulseScale = 1.0 +
                            evolutionAmount * radiusPulse * 0.08;
                        contourMorph =
                            RiverWaterFoamSmoothPeriodicWave(
                                candidateClock * lerp(
                                    0.49,
                                    1.57,
                                    secondaryHash) +
                                secondaryHash * 7.13 +
                                tertiaryHash * 1.29);
                    }

                    // Static authoring controls remain independent. Large-scale
                    // motion belongs to evolvedChipPointMetres, so candidate
                    // centres do not orbit or oscillate around their own cells.
                    float angle = angleHash * 6.28318530718;
                    float2 contourAxis = float2(
                        cos(angle),
                        sin(angle));
                    float2 fullJitter = (float2(
                        centreHashX,
                        centreHashY) - 0.5) * 0.78;
                    float2 candidateCentre =
                        (cell + 0.5 +
                            fullJitter * distributionIrregularity) * spacing;

                    float fullRadiusVariation = lerp(
                        0.58,
                        1.42,
                        radiusHash);
                    float candidateOuterRadius = nominalRadius * lerp(
                        1.0,
                        fullRadiusVariation,
                        sizeIrregularity);
                    candidateOuterRadius *=
                        geometricTurnoverScale *
                        radiusPulseScale;
                    // The 3x3 search remains correct even at maximum authored
                    // radius, size variation, lifecycle growth, and pulse.
                    candidateOuterRadius = min(
                        candidateOuterRadius,
                        spacing * 1.02);

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
                    float3 contourSetB = float3(
                        -contourSignB * lerp(
                            0.28,
                            0.50,
                            centreHashX),
                        contourSignC * lerp(
                            0.21,
                            0.39,
                            secondaryHash),
                        -contourSignA * lerp(
                            0.15,
                            0.31,
                            tertiaryHash));
                    float3 evolvedContour = lerp(
                        contourSetA,
                        contourSetB,
                        contourMorph);
                    float3 contourCoefficients = lerp(
                        contourSetA,
                        evolvedContour,
                        evolutionAmount);

                    float2 candidateDeltaMetres =
                        evolvedChipPointMetres - candidateCentre;
                    [branch]
                    if (evolutionAmount > 0.0001)
                    {
                        candidateDeltaMetres =
                            RiverWaterFoamApplyShapePreservingChipBasis(
                                candidateDeltaMetres,
                                sourceFromEvolvedX,
                                sourceFromEvolvedY);
                    }

                    float candidate = RiverWaterFoamSoftIrregularChip(
                        candidateDeltaMetres,
                        candidateOuterRadius,
                        antialiasMetres,
                        contourAxis,
                        shapeIrregularity,
                        contourCoefficients);
                    result.chipCandidateField = max(
                        result.chipCandidateField,
                        candidate);

                    float activation = saturate(chipActivation);
                    float active = activation > 0.0001
                        ? step(activationHash, activation)
                        : 0.0;
                    result.chipActivatedCandidates = max(
                        result.chipActivatedCandidates,
                        candidate * active * turnoverEnvelope);
                }
            }
        }
    }

    [branch]
    if (evaluateChipSelection > 0.5)
    {
        float chipLimit = saturate(chipSelectionDepth);
        float chipBand = 1.0 - smoothstep(
            chipLimit - edgeDepthAA,
            chipLimit + edgeDepthAA,
            edgeDepth);
        result.chipEdgeEligibility = saturate(
            materialBody * chipBand);
        result.chipFinalSelection = saturate(
            result.chipActivatedCandidates *
            result.chipEdgeEligibility);
        result.chipMaterialGate = saturate(
            materialBody *
            RiverWaterFoamResolveChipMaterialGate(materialPattern));
        result.chipProductionSelection = saturate(
            result.chipFinalSelection *
            result.chipMaterialGate);
    }

    [branch]
    if (evaluateFrayDiagnostics > 0.5)
    {
        float frayLimit = saturate(fraySelectionDepth);
        float frayBand = 1.0 - smoothstep(
            frayLimit - edgeDepthAA,
            frayLimit + edgeDepthAA,
            edgeDepth);
        result.frayPermittedBand = saturate(
            materialBody * frayBand);

        float wavelength = max(0.01, frayWavelength);
        float seed = saturate(materialPattern) * 31.17 + 7.93;
        float2 frayCoordinate = float2(
            pointMetres.x / wavelength +
                pointMetres.y / wavelength * 0.37,
            pointMetres.y / wavelength * 1.41 -
                pointMetres.x / wavelength * 0.23) + seed;
        float finePattern = RiverWaterFoamValueNoise(
            frayCoordinate);
        float selectedFray = smoothstep(
            0.48,
            0.78,
            finePattern);
        result.frayPatternPreview = saturate(
            result.frayPermittedBand *
            selectedFray *
            saturate(frayDepth));
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
    return smoothstep(0.02, 0.16, saturate(presence));
}

struct RiverWaterFoamPatternFields
{
    float combined;
    float chip;
    float fray;
    // Raw normalized scale bands are retained transiently so dedicated Strand
    // shaping can simplify unresolved detail before any survival threshold is
    // evaluated. The legacy Chip/Fray pair above remains unchanged.
    float4 scaleBands;
    // Stable broad fields provide low-frequency curvature modulation without a
    // second noise evaluation or a new pattern identity.
    float2 curvatureBands;
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

    // Chip and Fray now own distinct stable scale hierarchies. Chip is
    // anchored by the broad field and receives medium subdivision at lower
    // Breakup Scale. Fray is anchored by the medium field and receives fine
    // subdivision at lower Breakup Scale. The broad organization therefore
    // remains related instead of crossfading between unrelated pattern maps.
    float breakupDetail = 1.0 - scale;
    fields.chip = saturate(
        broadChipPattern +
        (mediumChipPattern - 0.5) * 0.68 * breakupDetail);
    fields.fray = saturate(
        mediumFrayPattern +
        (fineFrayPattern - 0.5) * 0.60 *
            breakupDetail * breakupDetail);
    fields.scaleBands = float4(
        mediumChipPattern,
        broadChipPattern,
        fineFrayPattern,
        mediumFrayPattern);
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
    float breakupScale,
    float strandStrength,
    float strandScale,
    float strandReach,
    float2 projectedMetreFootprint,
    float projectedPatternSeedFootprint,
    out float coherentSoftVisibility,
    out float materialEdgeDepth,
    out float strandSoftVisibility,
    out float4 breakupField,
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
            lateralMetres,
            breakupScale);
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
    float fineFootprint = max(
        footprint.x * 5.80,
        footprint.y * 7.40) +
        seedFootprint * 2.71;
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
    float fineResolved = 1.0 - smoothstep(
        0.34,
        0.76,
        fineFootprint);
    float bandResolved = 1.0 - smoothstep(
        0.36,
        0.80,
        bandFootprint);

    // Real Chip and Fray use the coherent Foam body rather than the hidden
    // lineified signal. Their candidate patterns are resolved hierarchically:
    // broad-to-medium for Chip and medium-to-fine for Fray. Unresolved fine
    // contribution disappears first; broad Chip survives longer than Fray.
    float resolvedChipPattern = saturate(lerp(
        patternFields.scaleBands.y,
        patternFields.chip,
        midResolved));
    float resolvedFrayPattern = saturate(lerp(
        patternFields.scaleBands.w,
        patternFields.fray,
        fineResolved));
    breakupField = float4(
        resolvedChipPattern,
        resolvedFrayPattern,
        saturate(broadResolved),
        saturate(midResolved));

    // Strand Scale owns a truthful hierarchy. Broad organization is always the
    // anchor; lower Scale progressively adds medium and fine subdivision only
    // while those source bands remain screen-resolved.
    float strandDetail = 1.0 - saturate(strandScale);
    float mediumAuthority = strandDetail * midResolved;
    float fineAuthority = strandDetail * strandDetail * fineResolved;
    strandPattern = float2(
        saturate(lerp(
            patternFields.scaleBands.y,
            patternFields.scaleBands.x,
            mediumAuthority)),
        saturate(lerp(
            patternFields.scaleBands.w,
            patternFields.scaleBands.z,
            fineAuthority)));
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

    // The anisotropic band family now belongs exclusively to Strands. Chip and
    // Fray no longer consume this hidden lineified signal.
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
    // Base material coverage is the no-new-sample edge-depth coordinate for
    // Chip and Fray. Unlike coherent visibility it contains no procedural
    // morphology valleys, anisotropic banding, or surface-break modulation.
    materialEdgeDepth = saturate(baseMask);
    strandSoftVisibility = saturate(strandVisible);
    float hardVisible = smoothstep(0.22, 0.58, coherentSoftVisibility);
    float fringe = smoothstep(0.06, 0.34, coherentSoftVisibility) * 0.34;
    return saturate(max(hardVisible, fringe));
}

float RiverWaterFoamResolveChipFrayEdgeKeep(
    float materialEdgeDepth,
    float chipPattern,
    float frayPattern,
    float chipStrength,
    float frayStrength,
    float edgeAA)
{
    float chip = saturate(chipStrength);
    float fray = saturate(frayStrength);
    if (max(chip, fray) <= 0.0001)
    {
        return 1.0;
    }

    float resolvedChipPattern = saturate(chipPattern);
    float chipPatternAA = max(
        fwidth(resolvedChipPattern),
        0.0015);
    float chipSelectionLow = lerp(
        0.74,
        0.52,
        chip);
    float chipSelectionHigh = chipSelectionLow + 0.18;
    float chipSelection = smoothstep(
        chipSelectionLow - chipPatternAA,
        chipSelectionHigh + chipPatternAA,
        resolvedChipPattern);
    float chipAuthority = saturate(
        chip * chipSelection);

    float resolvedFrayPattern = saturate(frayPattern);
    float frayPatternAA = max(
        fwidth(resolvedFrayPattern),
        0.0015);
    float fraySelectionLow = lerp(
        0.70,
        0.46,
        fray);
    float fraySelectionHigh = fraySelectionLow + 0.20;
    float fraySelection = smoothstep(
        fraySelectionLow - frayPatternAA,
        fraySelectionHigh + frayPatternAA,
        resolvedFrayPattern);
    float frayAuthority = saturate(
        fray * fraySelection);

    // Chip raises the required material depth in coherent medium regions. Fray
    // adds only a shallow fine perturbation to that same edge-depth requirement,
    // so it roughens both the original perimeter and the rims of Chip notches.
    float chipMaximumDepth = lerp(
        0.30,
        0.78,
        chip);
    float frayMaximumDepth = lerp(
        0.035,
        0.16,
        fray);
    float requiredDepth = saturate(
        chipAuthority * chipMaximumDepth +
        frayAuthority * frayMaximumDepth);
    float activeAuthority = saturate(max(
        chipAuthority,
        frayAuthority));
    float depthKeep = smoothstep(
        requiredDepth - edgeAA,
        requiredDepth + edgeAA,
        saturate(materialEdgeDepth));
    float keep = lerp(
        1.0,
        depthKeep,
        smoothstep(0.001, 0.08, activeAuthority));

    // Material that has reached the fully established base footprint remains a
    // protected core. This protection is based on material depth, not on the
    // procedurally eroded coherent-visibility scalar.
    float exactMaterialCore = step(
        0.999,
        saturate(materialEdgeDepth));
    return max(keep, exactMaterialCore);
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


float RiverWaterFoamResolveStrandChipKeep(
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

float RiverWaterFoamResolveStrandFrayKeep(
    float softShape,
    float frayPattern,
    float strandStrength,
    float strandDensity,
    float strandReach,
    float visibilityAA,
    float exactCore)
{
    float pattern = saturate(frayPattern);
    float patternAA = max(
        fwidth(pattern),
        0.0015);
    float density = saturate(strandDensity);
    float selectionLow = lerp(
        0.62,
        0.34,
        density);
    float selectionHigh = selectionLow + 0.18;
    float selection = smoothstep(
        selectionLow - patternAA,
        selectionHigh + patternAA,
        pattern);
    float authority = saturate(
        saturate(strandStrength) * selection);
    float maximumDepth = lerp(
        0.32,
        0.78,
        saturate(strandReach));
    float threshold = lerp(
        0.04,
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


float RiverWaterFoamApplyEdgeBreakup(
    float hardenedShape,
    float coherentSoftVisibility,
    float materialEdgeDepth,
    float strandSoftVisibility,
    float4 breakupField,
    float2 strandPattern,
    float strandResolution,
    float productionChipSelection,
    float frayStrength,
    float strandStrength,
    float strandDensity,
    float strandReach,
    out float productionChipRemovedMask)
{
    float shape = saturate(hardenedShape);
    float coherentSoftShape = saturate(coherentSoftVisibility);
    float strandSoftShape = saturate(strandSoftVisibility);
    float productionChip = saturate(productionChipSelection);
    float fray = saturate(frayStrength) * saturate(breakupField.w);
    float strand = saturate(strandStrength);
    productionChipRemovedMask = 0.0;

    [branch]
    if (shape <= 0.0001 ||
        max(max(productionChip, fray), strand) <= 0.0001)
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

    // Production Chip is the divorced analytical candidate selection gated by
    // transported Material Pattern. It changes the soft body before Strands,
    // then reconstructs the accepted coupled hardened mask through a ratio so
    // neutral regions remain byte-for-byte equivalent to the previous result.
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
        productionChipRemovedMask = saturate(
            shape - postChipMask);
    }

    // Legacy Fray remains temporarily intact until the dedicated final-edge
    // Fray patch. Legacy Chip authority is deliberately absent, preventing a
    // second hidden chip pass after the new production selection.
    float postFrayMask = postChipMask;
    [branch]
    if (fray > 0.0001)
    {
        float edgeDepth = saturate(materialEdgeDepth);
        float edgeAA = max(
            fwidth(edgeDepth),
            0.001);
        float edgeKeep = RiverWaterFoamResolveChipFrayEdgeKeep(
            edgeDepth,
            0.0,
            breakupField.y,
            0.0,
            fray,
            edgeAA);
        float postFraySoftShape = saturate(
            postChipSoftShape * edgeKeep);
        float baselineSoftMask = RiverWaterFoamHardenSoftVisibility(
            postChipSoftShape);
        float modifiedSoftMask = RiverWaterFoamHardenSoftVisibility(
            postFraySoftShape);
        float reconstructedRatio = baselineSoftMask > 0.0001
            ? saturate(modifiedSoftMask / baselineSoftMask)
            : 1.0;
        postFrayMask = saturate(
            postChipMask * reconstructedRatio);
    }

    // Accepted D1D Strands remain unchanged and continue to own lineification.
    float resolvedStrandStrength = strand * saturate(strandResolution);
    float strandAA = max(
        fwidth(strandSoftShape),
        0.001);
    float strandChipKeep = RiverWaterFoamResolveStrandChipKeep(
        strandSoftShape,
        strandPattern.x,
        resolvedStrandStrength,
        strandDensity,
        strandReach,
        strandAA,
        exactCore);
    float strandFrayKeep = RiverWaterFoamResolveStrandFrayKeep(
        strandSoftShape,
        strandPattern.y,
        resolvedStrandStrength,
        strandDensity,
        strandReach,
        strandAA,
        exactCore);
    float strandKeep = min(
        strandChipKeep,
        strandFrayKeep);

    return saturate(
        postFrayMask * strandKeep);
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
    float strandStrength,
    float strandScale,
    float strandReach,
    float2 projectedMetreFootprint,
    float projectedPatternSeedFootprint,
    out float coherentSoftVisibility,
    out float materialEdgeDepth,
    out float strandSoftVisibility,
    out float presence,
    out float remainingLife,
    out float materialPattern,
    out float4 breakupField,
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
        breakupScale,
        strandStrength,
        strandScale,
        strandReach,
        projectedMetreFootprint,
        projectedPatternSeedFootprint,
        coherentSoftVisibility,
        materialEdgeDepth,
        strandSoftVisibility,
        breakupField,
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
    float materialEdgeDepth;
    float strandSoftVisibility;
    float surfaceEnergy;
    float4 breakupField;
    float2 strandPattern;
    float strandResolution;
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
    float strandStrength,
    float strandScale,
    float strandReach,
    float freezeAmount,
    RiverWaterFoamSurfaceInfluence surfaceInfluence)
{
    RiverWaterFoamResult result;
    result.presence = 0.0;
    result.remainingLife = 0.0;
    result.materialPattern = 0.0;
    result.mask = 0.0;
    result.softVisibility = 0.0;
    result.materialEdgeDepth = 0.0;
    result.strandSoftVisibility = 0.0;
    result.surfaceEnergy = 0.0;
    result.breakupField = 0.0;
    result.strandPattern = 0.0;
    result.strandResolution = 0.0;
    result.fieldUV = 0.0;
    result.materialUV = 0.0;

    if (enabled < 0.5 || fieldLength <= 0.0001)
    {
        return result;
    }

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
    float storedMaterialEdgeDepth;
    float storedStrandSoftVisibility;
    float storedPresence;
    float storedRemainingLife;
    float storedMaterialPattern;
    float4 storedBreakupField;
    float2 storedStrandPattern;
    float storedStrandResolution;
    float storedMask = RiverWaterFoamResolveStateMask(
        storedState,
        storedGlobalDistance,
        storedLateralMetres,
        sharpness,
        finalVisibilityMode,
        breakupScale,
        strandStrength,
        strandScale,
        strandReach,
        projectedMetreFootprint,
        projectedPatternSeedFootprint,
        storedSoftVisibility,
        storedMaterialEdgeDepth,
        storedStrandSoftVisibility,
        storedPresence,
        storedRemainingLife,
        storedMaterialPattern,
        storedBreakupField,
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
    float visualMaterialEdgeDepth;
    float visualStrandSoftVisibility;
    float visualPresence;
    float visualRemainingLife;
    float visualMaterialPattern;
    float4 visualBreakupField;
    float2 visualStrandPattern;
    float visualStrandResolution;
    float visualMask = RiverWaterFoamResolveStateMask(
        visualState,
        storedGlobalDistance - warpMetres.x,
        storedLateralMetres - warpMetres.y,
        sharpness,
        finalVisibilityMode,
        breakupScale,
        strandStrength,
        strandScale,
        strandReach,
        projectedMetreFootprint,
        projectedPatternSeedFootprint,
        visualSoftVisibility,
        visualMaterialEdgeDepth,
        visualStrandSoftVisibility,
        visualPresence,
        visualRemainingLife,
        visualMaterialPattern,
        visualBreakupField,
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
    float coupledMaterialEdgeDepth = lerp(
        storedMaterialEdgeDepth,
        visualMaterialEdgeDepth,
        surfaceCoupling);
    float4 coupledBreakupField = lerp(
        storedBreakupField,
        visualBreakupField,
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
        float leadMaterialEdgeDepth;
        float leadStrandSoftVisibility;
        float leadPresence;
        float leadLife;
        float leadPattern;
        float4 leadBreakupField;
        float2 leadStrandPattern;
        float leadStrandResolution;
        float leadMask = RiverWaterFoamResolveStateMask(
            leadState,
            storedGlobalDistance - warpMetres.x - stretchDirection.x * stretchMetres,
            storedLateralMetres - warpMetres.y - stretchDirection.y * stretchMetres,
            sharpness,
            finalVisibilityMode,
            breakupScale,
            strandStrength,
            strandScale,
            strandReach,
            projectedMetreFootprint,
            projectedPatternSeedFootprint,
            leadSoftVisibility,
            leadMaterialEdgeDepth,
            leadStrandSoftVisibility,
            leadPresence,
            leadLife,
            leadPattern,
            leadBreakupField,
            leadStrandPattern,
            leadStrandResolution);
        float trailSoftVisibility;
        float trailMaterialEdgeDepth;
        float trailStrandSoftVisibility;
        float trailPresence;
        float trailLife;
        float trailPattern;
        float4 trailBreakupField;
        float2 trailStrandPattern;
        float trailStrandResolution;
        float trailMask = RiverWaterFoamResolveStateMask(
            trailState,
            storedGlobalDistance - warpMetres.x + stretchDirection.x * stretchMetres,
            storedLateralMetres - warpMetres.y + stretchDirection.y * stretchMetres,
            sharpness,
            finalVisibilityMode,
            breakupScale,
            strandStrength,
            strandScale,
            strandReach,
            projectedMetreFootprint,
            projectedPatternSeedFootprint,
            trailSoftVisibility,
            trailMaterialEdgeDepth,
            trailStrandSoftVisibility,
            trailPresence,
            trailLife,
            trailPattern,
            trailBreakupField,
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
        float leadScaledSoft = leadSoftVisibility * stretchScale;
        float trailScaledSoft = trailSoftVisibility * stretchScale;
        float trailOwnsBreakupStretch = step(
            leadScaledSoft,
            trailScaledSoft);
        float dominantBreakupSoft = max(
            leadScaledSoft,
            trailScaledSoft);
        float4 dominantBreakupField = lerp(
            leadBreakupField,
            trailBreakupField,
            trailOwnsBreakupStretch);
        float dominantMaterialEdgeDepth = lerp(
            leadMaterialEdgeDepth,
            trailMaterialEdgeDepth,
            trailOwnsBreakupStretch);
        float stretchOwnsBreakup = step(
            coupledSoftVisibility,
            dominantBreakupSoft);
        float4 stretchedBreakupField = lerp(
            coupledBreakupField,
            dominantBreakupField,
            stretchOwnsBreakup);
        float stretchedMaterialEdgeDepth = lerp(
            coupledMaterialEdgeDepth,
            dominantMaterialEdgeDepth,
            stretchOwnsBreakup);
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
        coupledBreakupField = lerp(
            coupledBreakupField,
            stretchedBreakupField,
            stretchWeight);
        coupledMaterialEdgeDepth = lerp(
            coupledMaterialEdgeDepth,
            stretchedMaterialEdgeDepth,
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
    float storedOwnsRetainedBreakup = step(
        coupledSoftVisibility,
        retainedStoredSoft);
    coupledSoftVisibility = max(
        coupledSoftVisibility,
        retainedStoredSoft);
    coupledBreakupField = lerp(
        coupledBreakupField,
        storedBreakupField,
        storedOwnsRetainedBreakup);
    coupledMaterialEdgeDepth = lerp(
        coupledMaterialEdgeDepth,
        storedMaterialEdgeDepth,
        storedOwnsRetainedBreakup);
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
    result.materialEdgeDepth = saturate(coupledMaterialEdgeDepth);
    result.breakupField = saturate(coupledBreakupField);
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
