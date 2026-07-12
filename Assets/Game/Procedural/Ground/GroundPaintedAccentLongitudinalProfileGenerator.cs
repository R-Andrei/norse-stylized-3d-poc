using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    internal readonly struct GroundPaintedAccentLongitudinalProfileSample
    {
        public GroundPaintedAccentLongitudinalProfileSample(
            float t,
            Vector3 localBaselinePoint,
            Vector3 localBaselineNormal,
            float endEnvelope,
            float widthScale,
            float halfWidth,
            float normalizedCrestHeight,
            float crestHeight,
            float crownHeight)
        {
            T = t;
            LocalBaselinePoint = localBaselinePoint;
            LocalBaselineNormal =
                localBaselineNormal.sqrMagnitude > 0.000001f
                    ? localBaselineNormal
                    : Vector3.up;
            EndEnvelope = endEnvelope;
            WidthScale = widthScale;
            HalfWidth = halfWidth;
            NormalizedCrestHeight = normalizedCrestHeight;
            CrestHeight = crestHeight;
            CrownHeight = crownHeight;
        }

        public float T { get; }
        public Vector3 LocalBaselinePoint { get; }
        public Vector3 LocalBaselineNormal { get; }
        public float EndEnvelope { get; }
        public float WidthScale { get; }
        public float HalfWidth { get; }
        public float NormalizedCrestHeight { get; }
        public float CrestHeight { get; }
        public float CrownHeight { get; }
        public float CombinedHeight => CrestHeight + CrownHeight;
    }

    internal readonly struct GroundPaintedAccentLongitudinalProfile
    {
        public GroundPaintedAccentLongitudinalProfile(
            GroundPaintedAccentLongitudinalProfileSample[] samples,
            float planarLength,
            int sourceKnotCount,
            int peakSampleIndex,
            float crestPeakHeight,
            float crownPeakHeight,
            float combinedPeakHeight,
            float moundPeakTarget,
            float rawCharacterRetention,
            float positiveFloorFraction,
            float rawPlateauSpan,
            float roundedCrestSpan,
            bool plateauSuppressed,
            bool apexSoftened,
            float leftEndpointAngleRequested,
            float leftEndpointAngleApplied,
            float rightEndpointAngleRequested,
            float rightEndpointAngleApplied,
            int signedDetailControlCount,
            int negativeDetailControlCount,
            int floorCorrectionSampleCount,
            float minimumProfileMinusFloor,
            int samplesBelowPositiveFloor,
            int dominantPeakViolations,
            float maximumSplineTangentDiscontinuity,
            float maximumSampledTurnDegrees)
        {
            Samples = samples ?? Array.Empty<GroundPaintedAccentLongitudinalProfileSample>();
            PlanarLength = planarLength;
            SourceKnotCount = sourceKnotCount;
            PeakSampleIndex = Samples.Length > 0 ? peakSampleIndex : 0;
            CrestPeakHeight = crestPeakHeight;
            CrownPeakHeight = crownPeakHeight;
            CombinedPeakHeight = combinedPeakHeight;
            MoundPeakTarget = moundPeakTarget;
            RawCharacterRetention = rawCharacterRetention;
            PositiveFloorFraction = positiveFloorFraction;
            RawPlateauSpan = rawPlateauSpan;
            RoundedCrestSpan = roundedCrestSpan;
            PlateauSuppressed = plateauSuppressed;
            ApexSoftened = apexSoftened;
            LeftEndpointAngleRequested = leftEndpointAngleRequested;
            LeftEndpointAngleApplied = leftEndpointAngleApplied;
            RightEndpointAngleRequested = rightEndpointAngleRequested;
            RightEndpointAngleApplied = rightEndpointAngleApplied;
            SignedDetailControlCount = signedDetailControlCount;
            NegativeDetailControlCount = negativeDetailControlCount;
            FloorCorrectionSampleCount = floorCorrectionSampleCount;
            MinimumProfileMinusFloor = minimumProfileMinusFloor;
            SamplesBelowPositiveFloor = samplesBelowPositiveFloor;
            DominantPeakViolations = dominantPeakViolations;
            MaximumSplineTangentDiscontinuity = maximumSplineTangentDiscontinuity;
            MaximumSampledTurnDegrees = maximumSampledTurnDegrees;
        }

        public GroundPaintedAccentLongitudinalProfileSample[] Samples { get; }
        public float PlanarLength { get; }
        public int SourceKnotCount { get; }
        public int FinalSampleCount => Samples != null ? Samples.Length : 0;
        public int PeakSampleIndex { get; }
        public float CrestPeakHeight { get; }
        public float CrownPeakHeight { get; }
        public float CombinedPeakHeight { get; }
        public float MoundPeakTarget { get; }
        public float RawCharacterRetention { get; }
        public float PositiveFloorFraction { get; }
        public float RawPlateauSpan { get; }
        public float RoundedCrestSpan { get; }
        public bool PlateauSuppressed { get; }
        public bool ApexSoftened { get; }
        public float LeftEndpointAngleRequested { get; }
        public float LeftEndpointAngleApplied { get; }
        public float RightEndpointAngleRequested { get; }
        public float RightEndpointAngleApplied { get; }
        public int SignedDetailControlCount { get; }
        public int NegativeDetailControlCount { get; }
        public int FloorCorrectionSampleCount { get; }
        public float MinimumProfileMinusFloor { get; }
        public int SamplesBelowPositiveFloor { get; }
        public int DominantPeakViolations { get; }
        public float MaximumSplineTangentDiscontinuity { get; }
        public float MaximumSampledTurnDegrees { get; }

        public bool IsValid => Samples != null && Samples.Length >= 3;
    }

    internal static class GroundPaintedAccentLongitudinalProfileGenerator
    {
        internal const float MinimumEndWidthScale = 0.12f;
        internal const int CrestSearchSampleCount = 5;
        internal const int MinimumSourceKnotCount = 17;
        internal const int MaximumSourceKnotCount = 25;
        internal const float TargetSourceKnotSpacing = 0.09f;
        internal const int FinalSampleMultiplier = 4;

        // Existing legacy mound-knot shaping remains the source of broad seeded
        // character. A6 converts those knots into one C1-continuous profile.
        internal const float MoundGuideBaseBlend = 0.34f;
        internal const float MoundGuideSpanBlend = 0.15f;
        internal const float MoundGuidePlateauBlend = 0.34f;
        internal const float MoundGuideIrregularityRetention = 0.05f;
        internal const float MoundPeakTargetBase = 0.94f;
        internal const float MoundPeakTargetSpanPromotion = 0.12f;
        internal const float MoundPeakTargetJitter = 0.035f;
        internal const float MoundPlateauThresholdFraction = 0.86f;
        internal const float MoundPlateauStartSpan = 0.18f;
        internal const float MoundPlateauFullSpan = 0.42f;
        internal const float MoundBaseSharpness = 1.15f;
        internal const float MoundSpanSharpness = 0.50f;
        internal const float MoundPlateauSharpness = 0.50f;
        internal const float RoundedCrestMinimumHalfSpan = 0.10f;
        internal const float RoundedCrestMaximumHalfSpan = 0.16f;
        internal const float RoundedCrestBlend = 0.72f;
        internal const float RoundedCrestFalloffPower = 1.65f;
        internal const float RoundedCrestAsymmetry = 0.18f;
        internal const float MoundIrregularityAsymmetry = 0.35f;
        internal const float ValleyThresholdFraction = 0.08f;
        internal const float ValleyRepairStrength = 0.60f;
        internal const float LegCrownSupport = 0.45f;
        internal const float CrownEndRampFraction = 0.12f;
        internal const float EnvelopeTransitionWidth = 0.08f;

        internal const float RawCharacterRetentionMinimum = 0.45f;
        internal const float RawCharacterRetentionMaximum = 0.90f;
        internal const float PositiveFloorFractionMinimum = 0.82f;
        internal const float PositiveFloorFractionMaximum = 0.92f;
        internal const float EndpointAngleMinimumDegrees = 12f;
        internal const float EndpointAngleMaximumDegrees = 68f;
        internal const float EndpointAngleSoftMaximumDegrees = 27f;
        internal const float EndpointAngleSteepMinimumDegrees = 47f;
        internal const int SignedDetailMinimumInteriorControlCount = 3;
        internal const int SignedDetailMaximumInteriorControlCount = 5;
        internal const float SignedDetailBroadAmplitudeMinimum = 0.060f;
        internal const float SignedDetailBroadAmplitudeMaximum = 0.180f;
        internal const float SignedDetailFineAmplitudeMinimum = 0.020f;
        internal const float SignedDetailFineAmplitudeMaximum = 0.070f;
        internal const float SignedDetailCrestProtectionFraction = 0.13f;
        internal const float SignedDetailEndpointProtectionFraction = 0.04f;
        internal const float NonDominantPeakCeiling = 0.985f;
        internal const float PositiveFloorTolerance = 0.00001f;
        internal const float SmoothFloorTransitionFraction = 0.012f;

        internal static bool TryBuild(
            GroundPaintedAccentSurfaceStroke stroke,
            GroundSurfaceFeatureRecipe feature,
            out GroundPaintedAccentLongitudinalProfile profile)
        {
            profile = default;
            if (!stroke.IsValid)
            {
                return false;
            }

            Vector3[] points = stroke.LocalPoints;
            Vector3[] normals = stroke.LocalNormals;
            float foldHeight =
                feature != null ? feature.PaintedAccentFoldHeight : 0.018f;
            float crestCrownHeight =
                feature != null ? feature.PaintedAccentCrestCrownHeight : 0.02f;
            float foldIrregularity =
                feature != null ? feature.PaintedAccentFoldIrregularity : 0.55f;
            float foldEndTaper =
                feature != null ? feature.PaintedAccentFoldEndTaper : 0.65f;
            float authoredStrokeLengthMin =
                feature != null ? feature.PaintedAccentStrokeLengthMin : 0.55f;
            float authoredStrokeLengthMax =
                feature != null ? feature.PaintedAccentStrokeLengthMax : 1.55f;
            float authoredStrokeWidth =
                feature != null ? feature.PaintedAccentStrokeWidth : 0.12f;

            float strokePlanarLength =
                ResolvePaintedAccentStrokePlanarLength(points);
            int desiredSourceKnotCount =
                Mathf.CeilToInt(
                    strokePlanarLength / TargetSourceKnotSpacing) + 1;
            int sourceKnotCount =
                Mathf.Clamp(
                    Mathf.Max(
                        MinimumSourceKnotCount,
                        Mathf.Max(points.Length, desiredSourceKnotCount)),
                    MinimumSourceKnotCount,
                    MaximumSourceKnotCount);
            int finalSampleCount =
                (sourceKnotCount - 1) *
                FinalSampleMultiplier + 1;
            float sourceStep = 1f / Mathf.Max(1, sourceKnotCount - 1);

            float strokeLengthFactor =
                Mathf.InverseLerp(
                    authoredStrokeLengthMin,
                    Mathf.Max(
                        authoredStrokeLengthMin + 0.001f,
                        authoredStrokeLengthMax),
                    strokePlanarLength);
            float strokeWidthRatio =
                stroke.Width / Mathf.Max(0.001f, authoredStrokeWidth);
            float strokeWidthFactor =
                Mathf.InverseLerp(0.84f, 1.18f, strokeWidthRatio);
            float moundSpanFactor =
                Mathf.Clamp01(
                    strokeLengthFactor * 0.65f +
                    strokeWidthFactor * 0.35f);
            PaintedAccentFoldProfileBasis[] profileBases =
                BuildPaintedAccentFoldProfileBases(
                    stroke.Seed,
                    foldIrregularity,
                    out float profileNormalization);
            float halfWidth = stroke.Width * 0.5f;
            float strokeHeight =
                foldHeight * Mathf.Lerp(0.94f, 1f, stroke.Strength);

            float[] sourceEndEnvelopes = new float[sourceKnotCount];
            float[] legacyKnots = new float[sourceKnotCount];
            for (int index = 0; index < sourceKnotCount; index++)
            {
                float t = index * sourceStep;
                float endEnvelope =
                    ResolvePaintedAccentFoldEndEnvelope(
                        t,
                        stroke.Seed,
                        foldEndTaper);
                sourceEndEnvelopes[index] = endEnvelope;

                float normalizedCrestHeight = 0f;
                for (int sampleIndex = 0;
                     sampleIndex < CrestSearchSampleCount;
                     sampleIndex++)
                {
                    float sample01 =
                        sampleIndex /
                        (float)(CrestSearchSampleCount - 1);
                    float u = sample01 * 2f - 1f;
                    normalizedCrestHeight =
                        Mathf.Max(
                            normalizedCrestHeight,
                            ResolvePaintedAccentFoldProfileHeight(
                                t,
                                u,
                                stroke.Seed,
                                profileBases,
                                profileNormalization,
                                foldIrregularity,
                                endEnvelope));
                }

                legacyKnots[index] = normalizedCrestHeight;
            }

            ShapePaintedAccentSingleMoundProfile(
                legacyKnots,
                sourceEndEnvelopes,
                stroke.Seed,
                foldIrregularity,
                moundSpanFactor,
                out float moundPeakTarget,
                out float ignoredMoundGuideBlend,
                out float rawPlateauSpan,
                out float roundedCrestSpan,
                out bool plateauSuppressed,
                out bool apexSoftened);

            int sourcePeakIndex = ResolvePeakIndex(legacyKnots);
            float peakT = sourcePeakIndex * sourceStep;
            peakT = Mathf.Clamp(
                peakT,
                sourceStep,
                1f - sourceStep);

            float leftRequestedAngle =
                ResolveEndpointAngleDegrees(stroke.Seed, 701u);
            float rightRequestedAngle =
                ResolveEndpointAngleDegrees(stroke.Seed, 709u);
            float leftRequestedDerivative =
                ResolveNormalizedEndpointDerivative(
                    leftRequestedAngle,
                    strokePlanarLength,
                    strokeHeight);
            float rightRequestedDerivative =
                ResolveNormalizedEndpointDerivative(
                    rightRequestedAngle,
                    strokePlanarLength,
                    strokeHeight);
            float leftGuideDerivative =
                Mathf.Min(
                    leftRequestedDerivative,
                    3f * moundPeakTarget /
                    Mathf.Max(0.001f, peakT));
            float rightGuideDerivative =
                Mathf.Min(
                    rightRequestedDerivative,
                    3f * moundPeakTarget /
                    Mathf.Max(0.001f, 1f - peakT));

            float rawCharacterRetention =
                Mathf.Lerp(
                    RawCharacterRetentionMinimum,
                    RawCharacterRetentionMaximum,
                    Mathf.Clamp01(
                        foldIrregularity * 0.78f +
                        ResolvePaintedAccentPreviewHash01(
                            stroke.Seed,
                            719u) * 0.22f));
            float positiveFloorFraction =
                Mathf.Lerp(
                    PositiveFloorFractionMinimum,
                    PositiveFloorFractionMaximum,
                    ResolvePaintedAccentPreviewHash01(
                        stroke.Seed,
                        727u));

            float[] sourceGuide = new float[sourceKnotCount];
            float[] sourceFloor = new float[sourceKnotCount];
            float[] sourceDetail = new float[sourceKnotCount];
            float[] sourceProfile = new float[sourceKnotCount];
            BuildSignedDetailSignal(
                sourceDetail,
                sourcePeakIndex,
                stroke.Seed,
                foldIrregularity,
                out int signedDetailControlCount,
                out int negativeDetailControlCount);

            for (int index = 0; index < sourceKnotCount; index++)
            {
                float t = index * sourceStep;
                float guide =
                    ResolvePositiveGuide(
                        t,
                        peakT,
                        moundPeakTarget,
                        leftGuideDerivative,
                        rightGuideDerivative);
                float floor = guide * positiveFloorFraction;
                float rawResidual = legacyKnots[index] - guide;
                float detail = sourceDetail[index] * moundPeakTarget;
                float candidate =
                    guide +
                    rawResidual * rawCharacterRetention +
                    detail;

                if (index == 0 || index == sourceKnotCount - 1)
                {
                    guide = 0f;
                    floor = 0f;
                    candidate = 0f;
                }
                else if (index == sourcePeakIndex)
                {
                    guide = moundPeakTarget;
                    floor = moundPeakTarget * positiveFloorFraction;
                    candidate = moundPeakTarget;
                }
                else
                {
                    candidate =
                        Mathf.Clamp(
                            candidate,
                            floor,
                            moundPeakTarget * NonDominantPeakCeiling);
                }

                sourceGuide[index] = guide;
                sourceFloor[index] = floor;
                sourceProfile[index] = candidate;
            }

            float[] sourceTangents =
                BuildShapePreservingTangents(
                    sourceProfile,
                    sourceStep,
                    sourcePeakIndex,
                    leftGuideDerivative,
                    rightGuideDerivative);
            float leftAppliedDerivative = sourceTangents[0];
            float rightAppliedDerivative = -sourceTangents[sourceKnotCount - 1];
            float leftAppliedAngle =
                ResolvePhysicalEndpointAngleDegrees(
                    leftAppliedDerivative,
                    strokePlanarLength,
                    strokeHeight);
            float rightAppliedAngle =
                ResolvePhysicalEndpointAngleDegrees(
                    rightAppliedDerivative,
                    strokePlanarLength,
                    strokeHeight);

            GroundPaintedAccentLongitudinalProfileSample[] samples =
                new GroundPaintedAccentLongitudinalProfileSample[finalSampleCount];
            float crestPeakHeight = 0f;
            float crownPeakHeight = 0f;
            float combinedPeakHeight = 0f;
            int peakSampleIndex = 0;
            int floorCorrectionSampleCount = 0;
            float minimumProfileMinusFloor = float.PositiveInfinity;
            int samplesBelowPositiveFloor = 0;
            int dominantPeakViolations = 0;
            Vector2[] physicalProfilePoints = new Vector2[finalSampleCount];

            for (int index = 0; index < finalSampleCount; index++)
            {
                float t =
                    finalSampleCount <= 1
                        ? 0f
                        : index / (float)(finalSampleCount - 1);
                float normalizedHeight =
                    EvaluateProfileSpline(
                        sourceProfile,
                        sourceTangents,
                        sourceStep,
                        t);
                float guide =
                    ResolvePositiveGuide(
                        t,
                        peakT,
                        moundPeakTarget,
                        leftGuideDerivative,
                        rightGuideDerivative);
                float floor = guide * positiveFloorFraction;
                if (normalizedHeight < floor - PositiveFloorTolerance)
                {
                    floorCorrectionSampleCount++;
                }

                if (index == 0 || index == finalSampleCount - 1)
                {
                    normalizedHeight = 0f;
                }
                else
                {
                    normalizedHeight =
                        ResolveSmoothMaximum(
                            normalizedHeight,
                            floor,
                            moundPeakTarget *
                            SmoothFloorTransitionFraction);
                    normalizedHeight =
                        Mathf.Clamp(normalizedHeight, 0f, 1.15f);
                }

                float finalFloorDifference = normalizedHeight - floor;
                minimumProfileMinusFloor =
                    Mathf.Min(
                        minimumProfileMinusFloor,
                        finalFloorDifference);
                if (finalFloorDifference < -PositiveFloorTolerance)
                {
                    samplesBelowPositiveFloor++;
                }

                float endEnvelope =
                    ResolvePaintedAccentFoldEndEnvelope(
                        t,
                        stroke.Seed,
                        foldEndTaper);
                float widthScale =
                    Mathf.Lerp(MinimumEndWidthScale, 1f, endEnvelope);
                float effectiveHalfWidth = halfWidth * widthScale;
                ResolvePaintedAccentRidgeLongitudinalSample(
                    points,
                    normals,
                    t,
                    out Vector3 centerlinePoint,
                    out Vector3 normal);
                float crestHeight = strokeHeight * normalizedHeight;
                float crownEndEnvelope =
                    ResolvePaintedAccentCrownEndEnvelope(t, endEnvelope);
                float crownHeight = crestCrownHeight * crownEndEnvelope;
                float combinedHeight = crestHeight + crownHeight;

                if (normalizedHeight >
                    moundPeakTarget + PositiveFloorTolerance &&
                    Mathf.Abs(t - peakT) > sourceStep)
                {
                    dominantPeakViolations++;
                }

                crestPeakHeight = Mathf.Max(crestPeakHeight, crestHeight);
                crownPeakHeight = Mathf.Max(crownPeakHeight, crownHeight);
                if (combinedHeight > combinedPeakHeight)
                {
                    combinedPeakHeight = combinedHeight;
                    peakSampleIndex = index;
                }

                physicalProfilePoints[index] =
                    new Vector2(
                        t * strokePlanarLength,
                        combinedHeight);
                samples[index] =
                    new GroundPaintedAccentLongitudinalProfileSample(
                        t,
                        centerlinePoint,
                        normal,
                        endEnvelope,
                        widthScale,
                        effectiveHalfWidth,
                        normalizedHeight,
                        crestHeight,
                        crownHeight);
            }

            if (float.IsPositiveInfinity(minimumProfileMinusFloor))
            {
                minimumProfileMinusFloor = 0f;
            }

            leftAppliedAngle =
                ResolveSampledEndpointAngleDegrees(
                    physicalProfilePoints,
                    true,
                    leftAppliedAngle);
            rightAppliedAngle =
                ResolveSampledEndpointAngleDegrees(
                    physicalProfilePoints,
                    false,
                    rightAppliedAngle);
            float maximumSampledTurnDegrees =
                ResolveMaximumSampledTurnDegrees(physicalProfilePoints);
            profile =
                new GroundPaintedAccentLongitudinalProfile(
                    samples,
                    strokePlanarLength,
                    sourceKnotCount,
                    peakSampleIndex,
                    crestPeakHeight,
                    crownPeakHeight,
                    combinedPeakHeight,
                    moundPeakTarget,
                    rawCharacterRetention,
                    positiveFloorFraction,
                    rawPlateauSpan,
                    roundedCrestSpan,
                    plateauSuppressed,
                    apexSoftened,
                    leftRequestedAngle,
                    leftAppliedAngle,
                    rightRequestedAngle,
                    rightAppliedAngle,
                    signedDetailControlCount,
                    negativeDetailControlCount,
                    floorCorrectionSampleCount,
                    minimumProfileMinusFloor,
                    samplesBelowPositiveFloor,
                    dominantPeakViolations,
                    0f,
                    maximumSampledTurnDegrees);
            return profile.IsValid;
        }

        private static int ResolvePeakIndex(float[] values)
        {
            int peakIndex = 1;
            float peakValue = float.NegativeInfinity;
            for (int index = 1; index < values.Length - 1; index++)
            {
                if (values[index] > peakValue)
                {
                    peakValue = values[index];
                    peakIndex = index;
                }
            }

            return peakIndex;
        }

        private static float ResolveEndpointAngleDegrees(
            int seed,
            uint salt)
        {
            float value = ResolvePaintedAccentPreviewHash01(seed, salt);
            if (value < 0.15f)
            {
                return Mathf.Lerp(
                    EndpointAngleMinimumDegrees,
                    EndpointAngleSoftMaximumDegrees,
                    value / 0.15f);
            }

            if (value < 0.65f)
            {
                return Mathf.Lerp(
                    EndpointAngleSoftMaximumDegrees,
                    EndpointAngleSteepMinimumDegrees,
                    (value - 0.15f) / 0.50f);
            }

            return Mathf.Lerp(
                EndpointAngleSteepMinimumDegrees,
                EndpointAngleMaximumDegrees,
                (value - 0.65f) / 0.35f);
        }

        private static float ResolveNormalizedEndpointDerivative(
            float angleDegrees,
            float planarLength,
            float strokeHeight)
        {
            float tangent =
                Mathf.Tan(
                    Mathf.Clamp(
                        angleDegrees,
                        0f,
                        85f) *
                    Mathf.Deg2Rad);
            return tangent *
                Mathf.Max(0.001f, planarLength) /
                Mathf.Max(0.001f, strokeHeight);
        }

        private static float ResolvePhysicalEndpointAngleDegrees(
            float normalizedDerivative,
            float planarLength,
            float strokeHeight)
        {
            float physicalSlope =
                Mathf.Max(0f, normalizedDerivative) *
                Mathf.Max(0.001f, strokeHeight) /
                Mathf.Max(0.001f, planarLength);
            return Mathf.Atan(physicalSlope) * Mathf.Rad2Deg;
        }

        private static float ResolvePositiveGuide(
            float t,
            float peakT,
            float peakHeight,
            float leftDerivative,
            float rightDerivative)
        {
            t = Mathf.Clamp01(t);
            peakT = Mathf.Clamp(peakT, 0.001f, 0.999f);
            if (t <= peakT)
            {
                float u = t / peakT;
                return EvaluateHermite(
                    0f,
                    peakHeight,
                    leftDerivative * peakT,
                    0f,
                    u);
            }

            float rightSpan = 1f - peakT;
            float rightU = (t - peakT) / rightSpan;
            return EvaluateHermite(
                peakHeight,
                0f,
                0f,
                -rightDerivative * rightSpan,
                rightU);
        }

        private static float EvaluateHermite(
            float firstValue,
            float secondValue,
            float firstDerivative,
            float secondDerivative,
            float u)
        {
            u = Mathf.Clamp01(u);
            float u2 = u * u;
            float u3 = u2 * u;
            float firstBasis = 2f * u3 - 3f * u2 + 1f;
            float firstDerivativeBasis = u3 - 2f * u2 + u;
            float secondBasis = -2f * u3 + 3f * u2;
            float secondDerivativeBasis = u3 - u2;
            return
                firstBasis * firstValue +
                firstDerivativeBasis * firstDerivative +
                secondBasis * secondValue +
                secondDerivativeBasis * secondDerivative;
        }

        private static void BuildSignedDetailSignal(
            float[] destination,
            int peakIndex,
            int seed,
            float irregularity,
            out int controlCount,
            out int negativeControlCount)
        {
            Array.Clear(destination, 0, destination.Length);
            controlCount = 0;
            negativeControlCount = 0;
            BuildSignedDetailLeg(
                destination,
                peakIndex,
                true,
                seed,
                irregularity,
                811u,
                ref controlCount,
                ref negativeControlCount);
            BuildSignedDetailLeg(
                destination,
                peakIndex,
                false,
                seed,
                irregularity,
                907u,
                ref controlCount,
                ref negativeControlCount);
        }

        private static void BuildSignedDetailLeg(
            float[] destination,
            int peakIndex,
            bool isLeft,
            int seed,
            float irregularity,
            uint salt,
            ref int totalControlCount,
            ref int negativeControlCount)
        {
            int lastIndex = destination.Length - 1;
            int startIndex = isLeft ? 0 : peakIndex;
            int finishIndex = isLeft ? peakIndex : lastIndex;
            int span = finishIndex - startIndex;
            if (span < 4)
            {
                return;
            }

            int interiorControlCount =
                Mathf.Clamp(
                    Mathf.RoundToInt(
                        Mathf.Lerp(
                            SignedDetailMinimumInteriorControlCount,
                            SignedDetailMaximumInteriorControlCount,
                            Mathf.Clamp01(
                                irregularity * 0.72f +
                                ResolvePaintedAccentPreviewHash01(
                                    seed,
                                    salt) * 0.28f))),
                    SignedDetailMinimumInteriorControlCount,
                    SignedDetailMaximumInteriorControlCount);
            int controlArrayCount = interiorControlCount + 2;
            float[] controlPositions = new float[controlArrayCount];
            float[] controlValues = new float[controlArrayCount];
            controlPositions[0] = 0f;
            controlPositions[controlArrayCount - 1] = 1f;
            controlValues[0] = 0f;
            controlValues[controlArrayCount - 1] = 0f;

            for (int controlIndex = 1;
                 controlIndex <= interiorControlCount;
                 controlIndex++)
            {
                float basePosition =
                    controlIndex / (float)(interiorControlCount + 1);
                float jitter =
                    ResolvePaintedAccentPreviewSignedHash(
                        seed,
                        salt + (uint)(controlIndex * 17)) *
                    (0.20f / (interiorControlCount + 1));
                float position =
                    Mathf.Clamp(
                        basePosition + jitter,
                        SignedDetailEndpointProtectionFraction,
                        1f - SignedDetailCrestProtectionFraction);
                float broadMix =
                    ResolvePaintedAccentPreviewHash01(
                        seed,
                        salt + (uint)(controlIndex * 23 + 3));
                float minimumAmplitude =
                    broadMix > 0.52f
                        ? SignedDetailBroadAmplitudeMinimum
                        : SignedDetailFineAmplitudeMinimum;
                float maximumAmplitude =
                    broadMix > 0.52f
                        ? SignedDetailBroadAmplitudeMaximum
                        : SignedDetailFineAmplitudeMaximum;
                float amplitude =
                    Mathf.Lerp(
                        minimumAmplitude,
                        maximumAmplitude,
                        ResolvePaintedAccentPreviewHash01(
                            seed,
                            salt + (uint)(controlIndex * 29 + 5)));
                float signValue =
                    ResolvePaintedAccentPreviewSignedHash(
                        seed,
                        salt + (uint)(controlIndex * 31 + 7));
                float value =
                    signValue *
                    amplitude *
                    Mathf.Lerp(0.40f, 1f, Mathf.Clamp01(irregularity));
                controlPositions[controlIndex] = position;
                controlValues[controlIndex] = value;
                totalControlCount++;
                if (value < 0f)
                {
                    negativeControlCount++;
                }
            }

            float[] controlTangents =
                BuildControlTangents(controlPositions, controlValues);
            for (int localIndex = 1; localIndex < span; localIndex++)
            {
                float u = localIndex / (float)span;
                float value =
                    EvaluateControlSpline(
                        controlPositions,
                        controlValues,
                        controlTangents,
                        u);
                float endpointEnvelope =
                    Mathf.Pow(
                        Mathf.Max(0f, Mathf.Sin(Mathf.PI * u)),
                        1.15f);
                int destinationIndex =
                    isLeft
                        ? startIndex + localIndex
                        : finishIndex - localIndex;
                destination[destinationIndex] +=
                    value * endpointEnvelope;
            }
        }

        private static float[] BuildControlTangents(
            float[] positions,
            float[] values)
        {
            float[] tangents = new float[values.Length];
            tangents[0] = 0f;
            tangents[values.Length - 1] = 0f;
            for (int index = 1; index < values.Length - 1; index++)
            {
                float span =
                    Mathf.Max(
                        0.001f,
                        positions[index + 1] -
                        positions[index - 1]);
                tangents[index] =
                    (values[index + 1] - values[index - 1]) /
                    span;
            }

            return tangents;
        }

        private static float EvaluateControlSpline(
            float[] positions,
            float[] values,
            float[] tangents,
            float u)
        {
            u = Mathf.Clamp01(u);
            int segment = 0;
            while (segment < positions.Length - 2 &&
                   u > positions[segment + 1])
            {
                segment++;
            }

            float span =
                Mathf.Max(
                    0.001f,
                    positions[segment + 1] -
                    positions[segment]);
            float localU =
                (u - positions[segment]) / span;
            float value =
                EvaluateHermite(
                    values[segment],
                    values[segment + 1],
                    tangents[segment] * span,
                    tangents[segment + 1] * span,
                    localU);
            float minimum =
                Mathf.Min(values[segment], values[segment + 1]) - 0.015f;
            float maximum =
                Mathf.Max(values[segment], values[segment + 1]) + 0.015f;
            return Mathf.Clamp(value, minimum, maximum);
        }

        private static float[] BuildShapePreservingTangents(
            float[] values,
            float step,
            int peakIndex,
            float leftEndpointDerivative,
            float rightEndpointDerivative)
        {
            int count = values.Length;
            float[] tangents = new float[count];
            float[] slopes = new float[count - 1];
            for (int index = 0; index < slopes.Length; index++)
            {
                slopes[index] =
                    (values[index + 1] - values[index]) /
                    Mathf.Max(0.0001f, step);
            }

            for (int index = 1; index < count - 1; index++)
            {
                float previousSlope = slopes[index - 1];
                float nextSlope = slopes[index];
                if (previousSlope * nextSlope <= 0f)
                {
                    tangents[index] = 0f;
                }
                else
                {
                    tangents[index] =
                        2f * previousSlope * nextSlope /
                        (previousSlope + nextSlope);
                }
            }

            tangents[0] =
                Mathf.Clamp(
                    leftEndpointDerivative,
                    0f,
                    3f * Mathf.Max(0f, slopes[0]));
            tangents[count - 1] =
                -Mathf.Clamp(
                    rightEndpointDerivative,
                    0f,
                    3f * Mathf.Max(0f, -slopes[slopes.Length - 1]));
            tangents[Mathf.Clamp(peakIndex, 1, count - 2)] = 0f;
            return tangents;
        }

        private static float EvaluateProfileSpline(
            float[] values,
            float[] tangents,
            float step,
            float t)
        {
            t = Mathf.Clamp01(t);
            float scaled = t / Mathf.Max(0.0001f, step);
            int firstIndex =
                Mathf.Clamp(
                    Mathf.FloorToInt(scaled),
                    0,
                    values.Length - 2);
            int secondIndex = firstIndex + 1;
            float localU = scaled - firstIndex;
            return EvaluateHermite(
                values[firstIndex],
                values[secondIndex],
                tangents[firstIndex] * step,
                tangents[secondIndex] * step,
                localU);
        }

        private static float ResolveSmoothMaximum(
            float first,
            float second,
            float transitionWidth)
        {
            transitionWidth = Mathf.Max(0.00001f, transitionWidth);
            float blend =
                Mathf.Clamp01(
                    0.5f +
                    0.5f *
                    (first - second) /
                    transitionWidth);
            return
                Mathf.Lerp(second, first, blend) +
                transitionWidth * blend * (1f - blend);
        }

        private static float ResolveSampledEndpointAngleDegrees(
            Vector2[] points,
            bool isStart,
            float fallbackAngle)
        {
            if (points == null || points.Length < 2)
            {
                return fallbackAngle;
            }

            Vector2 first =
                isStart ? points[0] : points[points.Length - 1];
            Vector2 second =
                isStart ? points[1] : points[points.Length - 2];
            Vector2 direction = second - first;
            if (direction.sqrMagnitude <= 0.0000001f)
            {
                return fallbackAngle;
            }

            return
                Mathf.Atan2(
                    Mathf.Abs(direction.y),
                    Mathf.Abs(direction.x)) *
                Mathf.Rad2Deg;
        }

        private static float ResolveMaximumSampledTurnDegrees(
            Vector2[] points)
        {
            float maximumTurn = 0f;
            if (points == null || points.Length < 3)
            {
                return maximumTurn;
            }

            for (int index = 1; index < points.Length - 1; index++)
            {
                Vector2 incoming = points[index] - points[index - 1];
                Vector2 outgoing = points[index + 1] - points[index];
                if (incoming.sqrMagnitude <= 0.0000001f ||
                    outgoing.sqrMagnitude <= 0.0000001f)
                {
                    continue;
                }

                float turn =
                    Vector2.Angle(
                        incoming.normalized,
                        outgoing.normalized);
                maximumTurn = Mathf.Max(maximumTurn, turn);
            }

            return maximumTurn;
        }

        private readonly struct PaintedAccentFoldProfileBasis
        {
            public PaintedAccentFoldProfileBasis(
                float center,
                float width,
                float amplitude,
                float centerDrift,
                float widthVariation,
                float amplitudeVariation,
                float phase,
                float frequency)
            {
                Center = center;
                Width = Mathf.Max(0.04f, width);
                Amplitude =
                    Mathf.Abs(amplitude) <= 0.001f
                        ? (amplitude < 0f ? -0.001f : 0.001f)
                        : amplitude;
                CenterDrift = centerDrift;
                WidthVariation = Mathf.Max(0f, widthVariation);
                AmplitudeVariation = Mathf.Max(0f, amplitudeVariation);
                Phase = phase;
                Frequency = Mathf.Max(0.05f, frequency);
            }

            public float Center { get; }
            public float Width { get; }
            public float Amplitude { get; }
            public float CenterDrift { get; }
            public float WidthVariation { get; }
            public float AmplitudeVariation { get; }
            public float Phase { get; }
            public float Frequency { get; }
        }
        private static void ResolvePaintedAccentRidgeLongitudinalSample(
            Vector3[] points,
            Vector3[] normals,
            float t,
            out Vector3 point,
            out Vector3 normal)
        {
            t = Mathf.Clamp01(t);
            float scaledIndex = t * (points.Length - 1);
            int lowerIndex =
                Mathf.Clamp(
                    Mathf.FloorToInt(scaledIndex),
                    0,
                    points.Length - 1);
            int upperIndex =
                Mathf.Min(points.Length - 1, lowerIndex + 1);
            float interpolation = scaledIndex - lowerIndex;

            point =
                Vector3.Lerp(
                    points[lowerIndex],
                    points[upperIndex],
                    interpolation);

            Vector3 lowerNormal =
                lowerIndex < normals.Length &&
                normals[lowerIndex].sqrMagnitude > 0.000001f
                    ? normals[lowerIndex].normalized
                    : Vector3.up;
            Vector3 upperNormal =
                upperIndex < normals.Length &&
                normals[upperIndex].sqrMagnitude > 0.000001f
                    ? normals[upperIndex].normalized
                    : lowerNormal;
            normal =
                Vector3.Lerp(
                    lowerNormal,
                    upperNormal,
                    interpolation);
            if (normal.sqrMagnitude <= 0.000001f)
            {
                normal = Vector3.up;
            }
            else
            {
                normal.Normalize();
            }
        }
        private static float ResolvePaintedAccentStrokePlanarLength(
            Vector3[] points)
        {
            if (points == null || points.Length < 2)
            {
                return 0f;
            }

            float length = 0f;
            for (int index = 1; index < points.Length; index++)
            {
                Vector2 previous =
                    new Vector2(
                        points[index - 1].x,
                        points[index - 1].z);
                Vector2 current =
                    new Vector2(
                        points[index].x,
                        points[index].z);
                length += Vector2.Distance(previous, current);
            }

            return length;
        }

        private static void ShapePaintedAccentSingleMoundProfile(
            float[] normalizedCrestHeights,
            float[] endEnvelopes,
            int strokeSeed,
            float irregularity,
            float moundSpanFactor,
            out float moundPeakTarget,
            out float moundGuideBlend,
            out float rawPlateauSpan,
            out float roundedCrestSpan,
            out bool plateauSuppressed,
            out bool apexSoftened)
        {
            moundPeakTarget = 0f;
            moundGuideBlend = 0f;
            rawPlateauSpan = 0f;
            roundedCrestSpan = 0f;
            plateauSuppressed = false;
            apexSoftened = false;

            if (normalizedCrestHeights == null ||
                endEnvelopes == null ||
                normalizedCrestHeights.Length < 3 ||
                normalizedCrestHeights.Length != endEnvelopes.Length)
            {
                return;
            }

            irregularity = Mathf.Clamp01(irregularity);
            moundSpanFactor = Mathf.Clamp01(moundSpanFactor);
            int lastIndex = normalizedCrestHeights.Length - 1;
            float rawPeakHeight = 0f;
            int highestSampleIndex = 1;

            for (int index = 1; index < lastIndex; index++)
            {
                float candidateHeight =
                    Mathf.Max(0f, normalizedCrestHeights[index]);
                if (candidateHeight > rawPeakHeight)
                {
                    rawPeakHeight = candidateHeight;
                    highestSampleIndex = index;
                }
            }

            if (rawPeakHeight <= 0.000001f)
            {
                normalizedCrestHeights[0] = 0f;
                normalizedCrestHeights[lastIndex] = 0f;
                return;
            }

            // If the raw profile already has a broad high shelf, choose the
            // dominant crest near that shelf's weighted centre rather than at
            // whichever almost-equal sample happened to win the maximum test.
            float weightedPeakIndex = 0f;
            float weightedPeakTotal = 0f;
            float weightedPeakThreshold = rawPeakHeight * 0.90f;
            for (int index = 1; index < lastIndex; index++)
            {
                float weight =
                    Mathf.Max(
                        0f,
                        normalizedCrestHeights[index] -
                        weightedPeakThreshold);
                weightedPeakIndex += index * weight;
                weightedPeakTotal += weight;
            }

            int peakIndex =
                weightedPeakTotal > 0.000001f
                    ? Mathf.RoundToInt(
                          weightedPeakIndex / weightedPeakTotal)
                    : highestSampleIndex;
            peakIndex = Mathf.Clamp(peakIndex, 1, lastIndex - 1);

            float plateauThreshold =
                rawPeakHeight * MoundPlateauThresholdFraction;
            int plateauStartIndex = peakIndex;
            while (plateauStartIndex > 1 &&
                   normalizedCrestHeights[plateauStartIndex - 1] >=
                       plateauThreshold)
            {
                plateauStartIndex--;
            }

            int plateauFinishIndex = peakIndex;
            while (plateauFinishIndex < lastIndex - 1 &&
                   normalizedCrestHeights[plateauFinishIndex + 1] >=
                       plateauThreshold)
            {
                plateauFinishIndex++;
            }

            rawPlateauSpan =
                (plateauFinishIndex - plateauStartIndex) /
                (float)Mathf.Max(1, lastIndex);
            float plateauFactor =
                Mathf.InverseLerp(
                    MoundPlateauStartSpan,
                    MoundPlateauFullSpan,
                    rawPlateauSpan);
            plateauSuppressed = plateauFactor > 0.001f;

            float peakTargetJitter =
                ResolvePaintedAccentPreviewSignedHash(
                    strokeSeed,
                    401u) *
                MoundPeakTargetJitter *
                Mathf.Lerp(0.35f, 1f, irregularity);
            moundPeakTarget =
                Mathf.Clamp(
                    MoundPeakTargetBase +
                    MoundPeakTargetSpanPromotion *
                        moundSpanFactor +
                    peakTargetJitter,
                    0.90f,
                    1.10f);

            moundGuideBlend =
                Mathf.Clamp(
                    MoundGuideBaseBlend +
                    MoundGuideSpanBlend *
                        moundSpanFactor +
                    MoundGuidePlateauBlend *
                        plateauFactor -
                    MoundGuideIrregularityRetention *
                        irregularity,
                    0.30f,
                    0.82f);

            float moundSharpness =
                MoundBaseSharpness +
                MoundSpanSharpness * moundSpanFactor +
                MoundPlateauSharpness * plateauFactor;
            float moundAsymmetry =
                ResolvePaintedAccentPreviewSignedHash(
                    strokeSeed,
                    409u) *
                MoundIrregularityAsymmetry *
                irregularity;
            float leftSharpness =
                Mathf.Clamp(
                    moundSharpness * (1f + moundAsymmetry),
                    0.90f,
                    3.20f);
            float rightSharpness =
                Mathf.Clamp(
                    moundSharpness * (1f - moundAsymmetry),
                    0.90f,
                    3.20f);
            float peakEndEnvelope =
                Mathf.Max(0.001f, endEnvelopes[peakIndex]);

            for (int index = 0; index <= lastIndex; index++)
            {
                float hillProgress;
                float sideSharpness;
                if (index <= peakIndex)
                {
                    hillProgress =
                        peakIndex > 0
                            ? index / (float)peakIndex
                            : 1f;
                    sideSharpness = leftSharpness;
                }
                else
                {
                    int finishSpan = lastIndex - peakIndex;
                    hillProgress =
                        finishSpan > 0
                            ? (lastIndex - index) /
                              (float)finishSpan
                            : 1f;
                    sideSharpness = rightSharpness;
                }

                float moundEnvelope =
                    Mathf.Pow(
                        Mathf.Clamp01(hillProgress),
                        sideSharpness);
                float relativeEndEnvelope =
                    Mathf.Clamp01(
                        Mathf.Max(0f, endEnvelopes[index]) /
                        peakEndEnvelope);
                float targetHeight =
                    rawPeakHeight *
                    Mathf.Min(
                        moundEnvelope,
                        relativeEndEnvelope);
                float rawHeight =
                    Mathf.Max(0f, normalizedCrestHeights[index]);
                normalizedCrestHeights[index] =
                    Mathf.Lerp(
                        rawHeight,
                        targetHeight,
                        moundGuideBlend);
            }

            normalizedCrestHeights[0] = 0f;
            normalizedCrestHeights[lastIndex] = 0f;

            // Preserve seeded asymmetry and smaller changes. Only substantial
            // one-row valleys are lifted so the stronger mound guide cannot
            // reintroduce the earlier double-hill failure.
            float[] valleySource =
                (float[])normalizedCrestHeights.Clone();
            float valleyThreshold =
                rawPeakHeight * ValleyThresholdFraction;

            for (int index = 1; index < lastIndex; index++)
            {
                float currentHeight =
                    Mathf.Max(0f, valleySource[index]);
                float lowerNeighbourHeight =
                    Mathf.Min(
                        Mathf.Max(0f, valleySource[index - 1]),
                        Mathf.Max(0f, valleySource[index + 1]));
                float valleyDepth =
                    lowerNeighbourHeight - currentHeight;
                if (valleyDepth <= valleyThreshold)
                {
                    continue;
                }

                normalizedCrestHeights[index] =
                    Mathf.Lerp(
                        currentHeight,
                        lowerNeighbourHeight,
                        ValleyRepairStrength);
            }

            float shapedPeakHeight = 0f;
            int shapedPeakIndex = 1;
            for (int index = 1; index < lastIndex; index++)
            {
                float shapedHeight =
                    Mathf.Max(0f, normalizedCrestHeights[index]);
                if (shapedHeight > shapedPeakHeight)
                {
                    shapedPeakHeight = shapedHeight;
                    shapedPeakIndex = index;
                }
            }

            if (shapedPeakHeight > 0.000001f)
            {
                float roundedHalfSpanFraction =
                    Mathf.Lerp(
                        RoundedCrestMinimumHalfSpan,
                        RoundedCrestMaximumHalfSpan,
                        ResolvePaintedAccentPreviewHash01(
                            strokeSeed,
                            421u));
                float crestAsymmetry =
                    ResolvePaintedAccentPreviewSignedHash(
                        strokeSeed,
                        431u) *
                    RoundedCrestAsymmetry *
                    irregularity;
                int maximumLeftRadius =
                    Mathf.Max(1, shapedPeakIndex - 1);
                int maximumRightRadius =
                    Mathf.Max(1, lastIndex - shapedPeakIndex - 1);
                int leftRadius =
                    Mathf.Clamp(
                        Mathf.RoundToInt(
                            lastIndex *
                            roundedHalfSpanFraction *
                            (1f + crestAsymmetry)),
                        Mathf.Min(2, maximumLeftRadius),
                        maximumLeftRadius);
                int rightRadius =
                    Mathf.Clamp(
                        Mathf.RoundToInt(
                            lastIndex *
                            roundedHalfSpanFraction *
                            (1f - crestAsymmetry)),
                        Mathf.Min(2, maximumRightRadius),
                        maximumRightRadius);
                int leftBoundaryIndex =
                    Mathf.Max(1, shapedPeakIndex - leftRadius);
                int rightBoundaryIndex =
                    Mathf.Min(lastIndex - 1, shapedPeakIndex + rightRadius);
                leftRadius = shapedPeakIndex - leftBoundaryIndex;
                rightRadius = rightBoundaryIndex - shapedPeakIndex;
                roundedCrestSpan =
                    (leftRadius + rightRadius) /
                    (float)Mathf.Max(1, lastIndex);

                float[] roundedSource =
                    (float[])normalizedCrestHeights.Clone();
                for (int index = leftBoundaryIndex;
                     index <= rightBoundaryIndex;
                     index++)
                {
                    if (index == shapedPeakIndex)
                    {
                        continue;
                    }

                    bool isLeft = index < shapedPeakIndex;
                    int sideRadius = isLeft ? leftRadius : rightRadius;
                    if (sideRadius <= 0)
                    {
                        continue;
                    }

                    int boundaryIndex =
                        isLeft ? leftBoundaryIndex : rightBoundaryIndex;
                    float distanceFromPeak =
                        Mathf.Abs(index - shapedPeakIndex) /
                        (float)sideRadius;
                    float roundedProgress =
                        Mathf.Pow(
                            Mathf.SmoothStep(
                                0f,
                                1f,
                                Mathf.Clamp01(distanceFromPeak)),
                            RoundedCrestFalloffPower);
                    float boundaryHeight =
                        Mathf.Max(0f, roundedSource[boundaryIndex]);
                    float roundedTarget =
                        Mathf.Lerp(
                            shapedPeakHeight,
                            boundaryHeight,
                            roundedProgress);
                    float sourceHeight =
                        Mathf.Max(0f, roundedSource[index]);
                    float localBlend =
                        roundedTarget >= sourceHeight
                            ? RoundedCrestBlend
                            : RoundedCrestBlend * 0.35f;
                    normalizedCrestHeights[index] =
                        Mathf.Lerp(
                            sourceHeight,
                            roundedTarget,
                            localBlend);
                }

                apexSoftened =
                    leftRadius > 0 || rightRadius > 0;

                shapedPeakHeight = 0f;
                for (int index = 1; index < lastIndex; index++)
                {
                    shapedPeakHeight =
                        Mathf.Max(
                            shapedPeakHeight,
                            normalizedCrestHeights[index]);
                }
            }

            if (shapedPeakHeight > 0.000001f)
            {
                float peakScale =
                    moundPeakTarget / shapedPeakHeight;
                for (int index = 1; index < lastIndex; index++)
                {
                    normalizedCrestHeights[index] =
                        Mathf.Clamp(
                            normalizedCrestHeights[index] * peakScale,
                            0f,
                            1.15f);
                }
            }

            normalizedCrestHeights[0] = 0f;
            normalizedCrestHeights[lastIndex] = 0f;
        }

        private static PaintedAccentFoldProfileBasis[]
            BuildPaintedAccentFoldProfileBases(
                int strokeSeed,
                float irregularity,
                out float normalization)
        {
            irregularity = Mathf.Clamp01(irregularity);
            int maximumAdditionalBases =
                irregularity <= 0.001f
                    ? 0
                    : Mathf.Clamp(
                          Mathf.CeilToInt(irregularity * 3f),
                          1,
                          3);
            int additionalBases = 0;
            if (maximumAdditionalBases > 0)
            {
                additionalBases =
                    1 +
                    Mathf.Min(
                        maximumAdditionalBases - 1,
                        Mathf.FloorToInt(
                            ResolvePaintedAccentPreviewHash01(
                                strokeSeed,
                                101u) *
                            maximumAdditionalBases));
            }

            int basisCount = 1 + additionalBases;
            PaintedAccentFoldProfileBasis[] bases =
                new PaintedAccentFoldProfileBasis[basisCount];
            normalization = 1f;
            float primaryWidth =
                Mathf.Lerp(
                    0.34f,
                    0.50f,
                    ResolvePaintedAccentPreviewHash01(
                        strokeSeed,
                        109u));

            for (int basisIndex = 0; basisIndex < basisCount; basisIndex++)
            {
                uint salt = (uint)(basisIndex * 47 + 137);
                bool isPrimary = basisIndex == 0;
                float center =
                    isPrimary
                        ? ResolvePaintedAccentPreviewSignedHash(
                              strokeSeed,
                              salt + 1u) *
                          0.20f * irregularity
                        : ResolvePaintedAccentPreviewSignedHash(
                              strokeSeed,
                              salt + 1u) *
                          Mathf.Lerp(
                              0.22f,
                              0.62f,
                              ResolvePaintedAccentPreviewHash01(
                                  strokeSeed,
                                  salt + 3u));
                float width =
                    primaryWidth *
                    (isPrimary
                        ? Mathf.Lerp(
                              0.84f,
                              1.14f,
                              ResolvePaintedAccentPreviewHash01(
                                  strokeSeed,
                                  salt + 5u))
                        : Mathf.Lerp(
                              0.38f,
                              0.82f,
                              ResolvePaintedAccentPreviewHash01(
                                  strokeSeed,
                                  salt + 5u)));
                float amplitude = 1f;
                if (!isPrimary)
                {
                    float amplitudeMagnitude =
                        Mathf.Lerp(
                            0.14f,
                            0.56f,
                            ResolvePaintedAccentPreviewHash01(
                                strokeSeed,
                                salt + 7u)) *
                        irregularity;
                    float amplitudeSign =
                        ResolvePaintedAccentPreviewHash01(
                            strokeSeed,
                            salt + 9u) < 0.33f
                            ? -1f
                            : 1f;
                    amplitude = amplitudeMagnitude * amplitudeSign;
                }
                float centerDrift =
                    ResolvePaintedAccentPreviewSignedHash(
                        strokeSeed,
                        salt + 11u) *
                    0.18f * irregularity;
                float widthVariation =
                    Mathf.Lerp(
                        0.04f,
                        0.24f,
                        ResolvePaintedAccentPreviewHash01(
                            strokeSeed,
                            salt + 13u)) *
                    irregularity;
                float amplitudeVariation =
                    Mathf.Lerp(
                        0.07f,
                        0.32f,
                        ResolvePaintedAccentPreviewHash01(
                            strokeSeed,
                            salt + 17u)) *
                    irregularity;
                float phase =
                    ResolvePaintedAccentPreviewHash01(
                        strokeSeed,
                        salt + 19u) *
                    Mathf.PI * 2f;
                float frequency =
                    Mathf.Lerp(
                        0.55f,
                        1.35f,
                        ResolvePaintedAccentPreviewHash01(
                            strokeSeed,
                            salt + 23u));

                bases[basisIndex] =
                    new PaintedAccentFoldProfileBasis(
                        center,
                        width,
                        amplitude,
                        centerDrift,
                        widthVariation,
                        amplitudeVariation,
                        phase,
                        frequency);
                if (!isPrimary && amplitude > 0f)
                {
                    normalization += amplitude * 0.35f;
                }
            }

            normalization = Mathf.Max(0.001f, normalization);
            return bases;
        }

        private static float ResolvePaintedAccentFoldProfileHeight(
            float t,
            float u,
            int strokeSeed,
            PaintedAccentFoldProfileBasis[] profileBases,
            float profileNormalization,
            float irregularity,
            float endEnvelope)
        {
            float edgePower =
                Mathf.Lerp(
                    1.15f,
                    1.65f,
                    ResolvePaintedAccentPreviewHash01(
                        strokeSeed,
                        307u));
            float edgeEnvelope =
                Mathf.Pow(
                    Mathf.Max(0f, 1f - u * u),
                    edgePower);
            if (edgeEnvelope <= 0f)
            {
                return 0f;
            }

            float profile = 0f;
            for (int basisIndex = 0;
                 basisIndex < profileBases.Length;
                 basisIndex++)
            {
                PaintedAccentFoldProfileBasis basis =
                    profileBases[basisIndex];
                float angle =
                    t * Mathf.PI * 2f * basis.Frequency +
                    basis.Phase;
                float center =
                    basis.Center +
                    basis.CenterDrift * Mathf.Sin(angle);
                float width =
                    basis.Width *
                    (1f +
                     basis.WidthVariation *
                     Mathf.Sin(angle * 0.83f + basis.Phase * 0.47f));
                width = Mathf.Max(0.04f, width);
                float amplitude =
                    basis.Amplitude *
                    (1f +
                     basis.AmplitudeVariation *
                     Mathf.Sin(angle * 1.11f + basis.Phase * 0.71f));
                float normalizedDistance =
                    (u - center) /
                    width;
                float gaussian =
                    Mathf.Exp(
                        -0.5f *
                        normalizedDistance *
                        normalizedDistance);
                profile += amplitude * gaussian;
            }

            profile /= Mathf.Max(0.001f, profileNormalization);

            float crossPhaseA =
                ResolvePaintedAccentPreviewHash01(
                    strokeSeed,
                    313u) *
                Mathf.PI * 2f;
            float crossPhaseB =
                ResolvePaintedAccentPreviewHash01(
                    strokeSeed,
                    317u) *
                Mathf.PI * 2f;
            float crossVariation =
                1f +
                irregularity *
                (Mathf.Sin(
                     u * Mathf.PI * 1.25f +
                     t * Mathf.PI * 0.55f +
                     crossPhaseA) * 0.24f +
                 Mathf.Sin(
                     u * Mathf.PI * 2.35f -
                     t * Mathf.PI * 0.70f +
                     crossPhaseB) * 0.14f);
            profile =
                Mathf.Max(
                    0f,
                    profile * Mathf.Max(0.40f, crossVariation));

            float phaseA =
                ResolvePaintedAccentPreviewHash01(
                    strokeSeed,
                    271u) *
                Mathf.PI * 2f;
            float phaseB =
                ResolvePaintedAccentPreviewHash01(
                    strokeSeed,
                    277u) *
                Mathf.PI * 2f;
            float alongVariation =
                1f +
                irregularity *
                (Mathf.Sin(t * Mathf.PI * 1.55f + phaseA) * 0.30f +
                 Mathf.Sin(t * Mathf.PI * 3.10f + phaseB) * 0.18f);
            alongVariation = Mathf.Clamp(alongVariation, 0.45f, 1.50f);

            return Mathf.Clamp(
                profile *
                edgeEnvelope *
                alongVariation *
                endEnvelope,
                0f,
                1.55f);
        }

        private static float ResolveSmoothMaximum01(
            float first,
            float second,
            float transitionWidth)
        {
            first = Mathf.Clamp01(first);
            second = Mathf.Clamp01(second);
            transitionWidth = Mathf.Max(0.0001f, transitionWidth);
            float firstWeight =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.InverseLerp(
                        -transitionWidth,
                        transitionWidth,
                        first - second));
            return Mathf.Lerp(second, first, firstWeight);
        }

        private static float ResolvePaintedAccentCrownEndEnvelope(
            float t,
            float foldEndEnvelope)
        {
            float shortRampEnvelope =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(
                        t /
                        Mathf.Max(
                            0.001f,
                            CrownEndRampFraction))) *
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(
                        (1f - t) /
                        Mathf.Max(
                            0.001f,
                            CrownEndRampFraction)));

            return ResolveSmoothMaximum01(
                Mathf.Clamp01(foldEndEnvelope),
                shortRampEnvelope * LegCrownSupport,
                EnvelopeTransitionWidth);
        }

        private static float ResolvePaintedAccentFoldEndEnvelope(
            float t,
            int strokeSeed,
            float endTaper)
        {
            float taperFraction =
                Mathf.Lerp(0.025f, 0.35f, Mathf.Clamp01(endTaper));
            float startTaper =
                taperFraction *
                Mathf.Lerp(
                    0.84f,
                    1.16f,
                    ResolvePaintedAccentPreviewHash01(
                        strokeSeed,
                        283u));
            float finishTaper =
                taperFraction *
                Mathf.Lerp(
                    0.84f,
                    1.16f,
                    ResolvePaintedAccentPreviewHash01(
                        strokeSeed,
                        293u));

            return
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(t / Mathf.Max(0.001f, startTaper))) *
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01((1f - t) / Mathf.Max(0.001f, finishTaper)));
        }

        private static float ResolvePaintedAccentPreviewSignedHash(
            int seed,
            uint salt)
        {
            return ResolvePaintedAccentPreviewHash01(seed, salt) * 2f - 1f;
        }

        private static float ResolvePaintedAccentPreviewHash01(
            int seed,
            uint salt)
        {
            unchecked
            {
                uint value = (uint)seed ^ (salt * 0x9E3779B9u);
                value ^= value >> 16;
                value *= 0x7FEB352Du;
                value ^= value >> 15;
                value *= 0x846CA68Bu;
                value ^= value >> 16;
                return (value & 0x00FFFFFFu) / 16777215f;
            }
        }
    }
}
