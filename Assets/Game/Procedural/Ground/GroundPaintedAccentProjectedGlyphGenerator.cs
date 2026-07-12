using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Rivers;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundPaintedAccentProjectedGlyphRejectionReason
    {
        None = 0,
        Sampling = 1,
        River = 2,
        ModifierExclusion = 3,
        BroadSlope = 4,
        LocalGrade = 5
    }

    public readonly struct GroundPaintedAccentProjectedGlyph
    {
        public GroundPaintedAccentProjectedGlyph(
            Vector3[] localSurfacePoints,
            float[] halfWidths,
            int seed,
            float crestT,
            float crestPeakHeight,
            float crownPeakHeight,
            float combinedPeakHeight,
            float maximumNorthDisplacementError,
            float maximumCrossAxisDrift)
        {
            LocalSurfacePoints =
                localSurfacePoints ?? Array.Empty<Vector3>();
            HalfWidths = halfWidths ?? Array.Empty<float>();
            Seed = seed;
            CrestT = Mathf.Clamp01(crestT);
            CrestPeakHeight = Mathf.Max(0f, crestPeakHeight);
            CrownPeakHeight = Mathf.Max(0f, crownPeakHeight);
            CombinedPeakHeight = Mathf.Max(0f, combinedPeakHeight);
            MaximumNorthDisplacementError =
                Mathf.Max(0f, maximumNorthDisplacementError);
            MaximumCrossAxisDrift = Mathf.Max(0f, maximumCrossAxisDrift);
        }

        public Vector3[] LocalSurfacePoints { get; }
        public float[] HalfWidths { get; }
        public int Seed { get; }
        public float CrestT { get; }
        public float CrestPeakHeight { get; }
        public float CrownPeakHeight { get; }
        public float CombinedPeakHeight { get; }
        public float MaximumNorthDisplacementError { get; }
        public float MaximumCrossAxisDrift { get; }

        public bool IsValid =>
            LocalSurfacePoints != null &&
            HalfWidths != null &&
            LocalSurfacePoints.Length >= 2 &&
            HalfWidths.Length == LocalSurfacePoints.Length;
    }

    public readonly struct GroundPaintedAccentProjectedGlyphRejectionDebugPoint
    {
        public GroundPaintedAccentProjectedGlyphRejectionDebugPoint(
            Vector3 localPosition,
            GroundPaintedAccentProjectedGlyphRejectionReason reason)
        {
            LocalPosition = localPosition;
            Reason = reason;
        }

        public Vector3 LocalPosition { get; }
        public GroundPaintedAccentProjectedGlyphRejectionReason Reason { get; }
    }

    public readonly struct GroundPaintedAccentProjectedGlyphDiagnostics
    {
        public GroundPaintedAccentProjectedGlyphDiagnostics(
            int acceptedBaseDescriptors,
            int projectedGlyphsAccepted,
            int projectedGlyphsRejectedSampling,
            int projectedGlyphsRejectedRiver,
            int projectedGlyphsRejectedModifier,
            int projectedGlyphsRejectedBroadSlope,
            int projectedGlyphsRejectedLocalGrade,
            Vector2 projectionLocalDirection,
            int projectedPointCountMin,
            float projectedPointCountMean,
            int projectedPointCountMax,
            float projectedCrestTMin,
            float projectedCrestTMean,
            float projectedCrestTMax,
            float crestPeakHeightMin,
            float crestPeakHeightMean,
            float crestPeakHeightMax,
            float crownPeakHeightMin,
            float crownPeakHeightMean,
            float crownPeakHeightMax,
            float combinedPeakHeightMin,
            float combinedPeakHeightMean,
            float combinedPeakHeightMax,
            float maximumNorthDisplacementError,
            float maximumCrossAxisDrift)
        {
            AcceptedBaseDescriptors = Mathf.Max(0, acceptedBaseDescriptors);
            ProjectedGlyphsAccepted = Mathf.Max(0, projectedGlyphsAccepted);
            ProjectedGlyphsRejectedSampling =
                Mathf.Max(0, projectedGlyphsRejectedSampling);
            ProjectedGlyphsRejectedRiver =
                Mathf.Max(0, projectedGlyphsRejectedRiver);
            ProjectedGlyphsRejectedModifier =
                Mathf.Max(0, projectedGlyphsRejectedModifier);
            ProjectedGlyphsRejectedBroadSlope =
                Mathf.Max(0, projectedGlyphsRejectedBroadSlope);
            ProjectedGlyphsRejectedLocalGrade =
                Mathf.Max(0, projectedGlyphsRejectedLocalGrade);
            ProjectionLocalDirection =
                projectionLocalDirection.sqrMagnitude > 0.000001f
                    ? projectionLocalDirection.normalized
                    : Vector2.up;
            ProjectedPointCountMin = Mathf.Max(0, projectedPointCountMin);
            ProjectedPointCountMean = Mathf.Max(0f, projectedPointCountMean);
            ProjectedPointCountMax = Mathf.Max(0, projectedPointCountMax);
            ProjectedCrestTMin = Mathf.Clamp01(projectedCrestTMin);
            ProjectedCrestTMean = Mathf.Clamp01(projectedCrestTMean);
            ProjectedCrestTMax = Mathf.Clamp01(projectedCrestTMax);
            CrestPeakHeightMin = Mathf.Max(0f, crestPeakHeightMin);
            CrestPeakHeightMean = Mathf.Max(0f, crestPeakHeightMean);
            CrestPeakHeightMax = Mathf.Max(0f, crestPeakHeightMax);
            CrownPeakHeightMin = Mathf.Max(0f, crownPeakHeightMin);
            CrownPeakHeightMean = Mathf.Max(0f, crownPeakHeightMean);
            CrownPeakHeightMax = Mathf.Max(0f, crownPeakHeightMax);
            CombinedPeakHeightMin = Mathf.Max(0f, combinedPeakHeightMin);
            CombinedPeakHeightMean = Mathf.Max(0f, combinedPeakHeightMean);
            CombinedPeakHeightMax = Mathf.Max(0f, combinedPeakHeightMax);
            MaximumNorthDisplacementError =
                Mathf.Max(0f, maximumNorthDisplacementError);
            MaximumCrossAxisDrift = Mathf.Max(0f, maximumCrossAxisDrift);
        }

        public int AcceptedBaseDescriptors { get; }
        public int ProjectedGlyphsAccepted { get; }
        public int ProjectedGlyphsRejectedSampling { get; }
        public int ProjectedGlyphsRejectedRiver { get; }
        public int ProjectedGlyphsRejectedModifier { get; }
        public int ProjectedGlyphsRejectedBroadSlope { get; }
        public int ProjectedGlyphsRejectedLocalGrade { get; }
        public Vector2 ProjectionLocalDirection { get; }
        public int ProjectedPointCountMin { get; }
        public float ProjectedPointCountMean { get; }
        public int ProjectedPointCountMax { get; }
        public float ProjectedCrestTMin { get; }
        public float ProjectedCrestTMean { get; }
        public float ProjectedCrestTMax { get; }
        public float CrestPeakHeightMin { get; }
        public float CrestPeakHeightMean { get; }
        public float CrestPeakHeightMax { get; }
        public float CrownPeakHeightMin { get; }
        public float CrownPeakHeightMean { get; }
        public float CrownPeakHeightMax { get; }
        public float CombinedPeakHeightMin { get; }
        public float CombinedPeakHeightMean { get; }
        public float CombinedPeakHeightMax { get; }
        public float MaximumNorthDisplacementError { get; }
        public float MaximumCrossAxisDrift { get; }

        public int ProjectedGlyphsRejectedTotal =>
            ProjectedGlyphsRejectedSampling +
            ProjectedGlyphsRejectedRiver +
            ProjectedGlyphsRejectedModifier +
            ProjectedGlyphsRejectedBroadSlope +
            ProjectedGlyphsRejectedLocalGrade;

        public static GroundPaintedAccentProjectedGlyphDiagnostics Empty =>
            default;
    }

    public readonly struct GroundPaintedAccentProjectedGlyphDebugSnapshot
    {
        public GroundPaintedAccentProjectedGlyphDebugSnapshot(
            GroundPaintedAccentProjectedGlyph[] glyphs,
            GroundPaintedAccentProjectedGlyphRejectionDebugPoint[] rejections,
            GroundPaintedAccentProjectedGlyphDiagnostics diagnostics)
        {
            Glyphs = glyphs ?? Array.Empty<GroundPaintedAccentProjectedGlyph>();
            Rejections =
                rejections ??
                Array.Empty<GroundPaintedAccentProjectedGlyphRejectionDebugPoint>();
            Diagnostics = diagnostics;
            isValid = true;
        }

        public GroundPaintedAccentProjectedGlyph[] Glyphs { get; }
        public GroundPaintedAccentProjectedGlyphRejectionDebugPoint[] Rejections
        {
            get;
        }
        public GroundPaintedAccentProjectedGlyphDiagnostics Diagnostics { get; }

        private readonly bool isValid;

        public bool IsValid =>
            isValid && Glyphs != null && Rejections != null;

        public static GroundPaintedAccentProjectedGlyphDebugSnapshot Empty =>
            default;
    }

    internal static class GroundPaintedAccentProjectedGlyphGenerator
    {
        private const float RiverSafetyClearance = 0.15f;
        private const float MaximumBroadSlopeDegrees = 45f;
        private const float MaximumLocalGradeDegrees = 40f;
        private const float RejectionMarkerSurfaceOffset = 0.04f;

        public static GroundPaintedAccentProjectedGlyphDebugSnapshot Build(
            IReadOnlyList<GroundPaintedAccentSurfaceStroke> baseStrokes,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 projectionLocalDirection)
        {
            if (baseStrokes == null ||
                baseSurface == null ||
                !baseSurface.IsValid ||
                feature == null)
            {
                return GroundPaintedAccentProjectedGlyphDebugSnapshot.Empty;
            }

            Vector2 localNorth =
                projectionLocalDirection.sqrMagnitude > 0.000001f
                    ? projectionLocalDirection.normalized
                    : Vector2.up;
            List<GroundPaintedAccentProjectedGlyph> glyphs =
                new List<GroundPaintedAccentProjectedGlyph>(baseStrokes.Count);
            List<GroundPaintedAccentProjectedGlyphRejectionDebugPoint> rejections =
                new List<GroundPaintedAccentProjectedGlyphRejectionDebugPoint>();

            int rejectedSampling = 0;
            int rejectedRiver = 0;
            int rejectedModifier = 0;
            int rejectedBroadSlope = 0;
            int rejectedLocalGrade = 0;
            int pointCountMinimum = int.MaxValue;
            int pointCountMaximum = 0;
            double pointCountTotal = 0.0;
            float crestMinimum = float.PositiveInfinity;
            float crestMaximum = 0f;
            double crestTotal = 0.0;
            float crestHeightMinimum = float.PositiveInfinity;
            float crestHeightMaximum = 0f;
            double crestHeightTotal = 0.0;
            float crownHeightMinimum = float.PositiveInfinity;
            float crownHeightMaximum = 0f;
            double crownHeightTotal = 0.0;
            float combinedHeightMinimum = float.PositiveInfinity;
            float combinedHeightMaximum = 0f;
            double combinedHeightTotal = 0.0;
            float maximumNorthError = 0f;
            float maximumCrossDrift = 0f;

            for (int strokeIndex = 0;
                 strokeIndex < baseStrokes.Count;
                 strokeIndex++)
            {
                GroundPaintedAccentSurfaceStroke stroke = baseStrokes[strokeIndex];
                if (!stroke.IsValid)
                {
                    rejectedSampling++;
                    continue;
                }

                if (TryCreateProjectedGlyph(
                        stroke,
                        baseSurface,
                        feature,
                        rivers,
                        modifiers,
                        localNorth,
                        out GroundPaintedAccentProjectedGlyph glyph,
                        out GroundPaintedAccentProjectedGlyphRejectionReason reason))
                {
                    glyphs.Add(glyph);
                    int pointCount = glyph.LocalSurfacePoints.Length;
                    pointCountMinimum = Mathf.Min(pointCountMinimum, pointCount);
                    pointCountMaximum = Mathf.Max(pointCountMaximum, pointCount);
                    pointCountTotal += pointCount;
                    crestMinimum = Mathf.Min(crestMinimum, glyph.CrestT);
                    crestMaximum = Mathf.Max(crestMaximum, glyph.CrestT);
                    crestTotal += glyph.CrestT;
                    crestHeightMinimum =
                        Mathf.Min(crestHeightMinimum, glyph.CrestPeakHeight);
                    crestHeightMaximum =
                        Mathf.Max(crestHeightMaximum, glyph.CrestPeakHeight);
                    crestHeightTotal += glyph.CrestPeakHeight;
                    crownHeightMinimum =
                        Mathf.Min(crownHeightMinimum, glyph.CrownPeakHeight);
                    crownHeightMaximum =
                        Mathf.Max(crownHeightMaximum, glyph.CrownPeakHeight);
                    crownHeightTotal += glyph.CrownPeakHeight;
                    combinedHeightMinimum =
                        Mathf.Min(combinedHeightMinimum, glyph.CombinedPeakHeight);
                    combinedHeightMaximum =
                        Mathf.Max(combinedHeightMaximum, glyph.CombinedPeakHeight);
                    combinedHeightTotal += glyph.CombinedPeakHeight;
                    maximumNorthError =
                        Mathf.Max(
                            maximumNorthError,
                            glyph.MaximumNorthDisplacementError);
                    maximumCrossDrift =
                        Mathf.Max(maximumCrossDrift, glyph.MaximumCrossAxisDrift);
                    continue;
                }

                switch (reason)
                {
                    case GroundPaintedAccentProjectedGlyphRejectionReason.River:
                        rejectedRiver++;
                        break;
                    case GroundPaintedAccentProjectedGlyphRejectionReason.ModifierExclusion:
                        rejectedModifier++;
                        break;
                    case GroundPaintedAccentProjectedGlyphRejectionReason.BroadSlope:
                        rejectedBroadSlope++;
                        break;
                    case GroundPaintedAccentProjectedGlyphRejectionReason.LocalGrade:
                        rejectedLocalGrade++;
                        break;
                    case GroundPaintedAccentProjectedGlyphRejectionReason.Sampling:
                    default:
                        rejectedSampling++;
                        break;
                }

                Vector3 markerPosition =
                    stroke.LocalPoints[stroke.LocalPoints.Length / 2];
                markerPosition.y += RejectionMarkerSurfaceOffset;
                rejections.Add(
                    new GroundPaintedAccentProjectedGlyphRejectionDebugPoint(
                        markerPosition,
                        reason));
            }

            int acceptedCount = glyphs.Count;
            float divisor = Mathf.Max(1, acceptedCount);
            GroundPaintedAccentProjectedGlyphDiagnostics diagnostics =
                new GroundPaintedAccentProjectedGlyphDiagnostics(
                    baseStrokes.Count,
                    acceptedCount,
                    rejectedSampling,
                    rejectedRiver,
                    rejectedModifier,
                    rejectedBroadSlope,
                    rejectedLocalGrade,
                    localNorth,
                    acceptedCount > 0 ? pointCountMinimum : 0,
                    (float)(pointCountTotal / divisor),
                    pointCountMaximum,
                    acceptedCount > 0 ? crestMinimum : 0f,
                    (float)(crestTotal / divisor),
                    acceptedCount > 0 ? crestMaximum : 0f,
                    acceptedCount > 0 ? crestHeightMinimum : 0f,
                    (float)(crestHeightTotal / divisor),
                    acceptedCount > 0 ? crestHeightMaximum : 0f,
                    acceptedCount > 0 ? crownHeightMinimum : 0f,
                    (float)(crownHeightTotal / divisor),
                    acceptedCount > 0 ? crownHeightMaximum : 0f,
                    acceptedCount > 0 ? combinedHeightMinimum : 0f,
                    (float)(combinedHeightTotal / divisor),
                    acceptedCount > 0 ? combinedHeightMaximum : 0f,
                    maximumNorthError,
                    maximumCrossDrift);

            return new GroundPaintedAccentProjectedGlyphDebugSnapshot(
                glyphs.ToArray(),
                rejections.ToArray(),
                diagnostics);
        }

        private static bool TryCreateProjectedGlyph(
            GroundPaintedAccentSurfaceStroke stroke,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 localNorth,
            out GroundPaintedAccentProjectedGlyph glyph,
            out GroundPaintedAccentProjectedGlyphRejectionReason rejectionReason)
        {
            glyph = default;
            rejectionReason =
                GroundPaintedAccentProjectedGlyphRejectionReason.None;

            if (!GroundPaintedAccentLongitudinalProfileGenerator.TryBuild(
                    stroke,
                    feature,
                    out GroundPaintedAccentLongitudinalProfile profile))
            {
                rejectionReason =
                    GroundPaintedAccentProjectedGlyphRejectionReason.Sampling;
                return false;
            }

            GroundPaintedAccentLongitudinalProfileSample[] samples =
                profile.Samples;
            Vector2[] projectedPoints = new Vector2[samples.Length];
            float[] halfWidths = new float[samples.Length];
            Vector2 localCross = new Vector2(-localNorth.y, localNorth.x);
            float maximumNorthError = 0f;
            float maximumCrossDrift = 0f;

            for (int pointIndex = 0;
                 pointIndex < samples.Length;
                 pointIndex++)
            {
                GroundPaintedAccentLongitudinalProfileSample sample =
                    samples[pointIndex];
                Vector2 baseline =
                    new Vector2(
                        sample.LocalBaselinePoint.x,
                        sample.LocalBaselinePoint.z);
                Vector2 projected =
                    baseline + localNorth * sample.CombinedHeight;
                Vector2 displacement = projected - baseline;
                float northDistance = Vector2.Dot(displacement, localNorth);
                float crossDistance = Vector2.Dot(displacement, localCross);

                maximumNorthError =
                    Mathf.Max(
                        maximumNorthError,
                        Mathf.Abs(northDistance - sample.CombinedHeight));
                maximumCrossDrift =
                    Mathf.Max(maximumCrossDrift, Mathf.Abs(crossDistance));
                projectedPoints[pointIndex] = projected;
                halfWidths[pointIndex] = sample.HalfWidth;
            }

            if (!TryValidateAndSampleProjectedGlyph(
                    projectedPoints,
                    halfWidths,
                    baseSurface,
                    rivers,
                    modifiers,
                    out Vector3[] localSurfacePoints,
                    out rejectionReason))
            {
                return false;
            }

            float crestT =
                samples[Mathf.Clamp(
                    profile.PeakSampleIndex,
                    0,
                    samples.Length - 1)].T;
            glyph =
                new GroundPaintedAccentProjectedGlyph(
                    localSurfacePoints,
                    halfWidths,
                    stroke.Seed,
                    crestT,
                    profile.CrestPeakHeight,
                    profile.CrownPeakHeight,
                    profile.CombinedPeakHeight,
                    maximumNorthError,
                    maximumCrossDrift);
            return glyph.IsValid;
        }

        private static bool TryValidateAndSampleProjectedGlyph(
            IReadOnlyList<Vector2> projectedPoints,
            IReadOnlyList<float> halfWidths,
            GroundHeightFieldSnapshot baseSurface,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            out Vector3[] localSurfacePoints,
            out GroundPaintedAccentProjectedGlyphRejectionReason rejectionReason)
        {
            localSurfacePoints = Array.Empty<Vector3>();
            rejectionReason =
                GroundPaintedAccentProjectedGlyphRejectionReason.None;

            if (projectedPoints == null ||
                halfWidths == null ||
                projectedPoints.Count < 2 ||
                projectedPoints.Count != halfWidths.Count)
            {
                rejectionReason =
                    GroundPaintedAccentProjectedGlyphRejectionReason.Sampling;
                return false;
            }

            Vector3[] sampledPoints = new Vector3[projectedPoints.Count];
            bool hasPreviousCenter = false;
            Vector2 previousCenterXZ = Vector2.zero;
            float previousCenterHeight = 0f;

            for (int pointIndex = 0;
                 pointIndex < projectedPoints.Count;
                 pointIndex++)
            {
                Vector2 centerXZ = projectedPoints[pointIndex];
                Vector2 tangent = ResolveTangent(projectedPoints, pointIndex);
                Vector2 sideAxis = new Vector2(-tangent.y, tangent.x);
                float halfWidth = Mathf.Max(0.0005f, halfWidths[pointIndex]);
                Vector2 leftXZ = centerXZ - sideAxis * halfWidth;
                Vector2 rightXZ = centerXZ + sideAxis * halfWidth;

                if (!baseSurface.TrySample(
                        centerXZ,
                        out GroundSurfaceSample centerSample) ||
                    !baseSurface.TrySample(
                        leftXZ,
                        out GroundSurfaceSample leftSample) ||
                    !baseSurface.TrySample(
                        rightXZ,
                        out GroundSurfaceSample rightSample))
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.Sampling;
                    return false;
                }

                if (ExceedsBroadSlope(centerSample) ||
                    ExceedsBroadSlope(leftSample) ||
                    ExceedsBroadSlope(rightSample))
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.BroadSlope;
                    return false;
                }

                if (IsExcludedByRiver(
                        centerXZ,
                        rivers,
                        halfWidth + RiverSafetyClearance) ||
                    IsExcludedByRiver(
                        leftXZ,
                        rivers,
                        RiverSafetyClearance) ||
                    IsExcludedByRiver(
                        rightXZ,
                        rivers,
                        RiverSafetyClearance))
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.River;
                    return false;
                }

                if (IsExcludedByModifier(centerXZ, modifiers) ||
                    IsExcludedByModifier(leftXZ, modifiers) ||
                    IsExcludedByModifier(rightXZ, modifiers))
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.ModifierExclusion;
                    return false;
                }

                if (ExceedsGrade(
                        leftSample.Height,
                        centerSample.Height,
                        Vector2.Distance(leftXZ, centerXZ)) ||
                    ExceedsGrade(
                        centerSample.Height,
                        rightSample.Height,
                        Vector2.Distance(centerXZ, rightXZ)))
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.LocalGrade;
                    return false;
                }

                if (hasPreviousCenter &&
                    ExceedsGrade(
                        previousCenterHeight,
                        centerSample.Height,
                        Vector2.Distance(previousCenterXZ, centerXZ)))
                {
                    rejectionReason =
                        GroundPaintedAccentProjectedGlyphRejectionReason.LocalGrade;
                    return false;
                }

                sampledPoints[pointIndex] =
                    new Vector3(centerXZ.x, centerSample.Height, centerXZ.y);
                hasPreviousCenter = true;
                previousCenterXZ = centerXZ;
                previousCenterHeight = centerSample.Height;
            }

            localSurfacePoints = sampledPoints;
            return true;
        }

        private static Vector2 ResolveTangent(
            IReadOnlyList<Vector2> points,
            int pointIndex)
        {
            int beforeIndex = Mathf.Max(0, pointIndex - 1);
            int afterIndex = Mathf.Min(points.Count - 1, pointIndex + 1);
            Vector2 tangent = points[afterIndex] - points[beforeIndex];

            if (tangent.sqrMagnitude > 0.000001f)
            {
                return tangent.normalized;
            }

            return Vector2.right;
        }

        private static bool ExceedsBroadSlope(GroundSurfaceSample sample)
        {
            Vector3 normal =
                sample.RenderNormal.sqrMagnitude > 0.000001f
                    ? sample.RenderNormal.normalized
                    : Vector3.up;
            return Vector3.Angle(normal, Vector3.up) > MaximumBroadSlopeDegrees;
        }

        private static bool ExceedsGrade(
            float heightA,
            float heightB,
            float horizontalDistance)
        {
            if (horizontalDistance <= 0.00001f)
            {
                return Mathf.Abs(heightB - heightA) > 0.00001f;
            }

            float gradeDegrees =
                Mathf.Atan2(
                    Mathf.Abs(heightB - heightA),
                    horizontalDistance) *
                Mathf.Rad2Deg;
            return gradeDegrees > MaximumLocalGradeDegrees;
        }

        private static bool IsExcludedByRiver(
            Vector2 point,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            float clearance)
        {
            if (rivers == null)
            {
                return false;
            }

            for (int riverIndex = 0;
                 riverIndex < rivers.Count;
                 riverIndex++)
            {
                StylizedRiverGroundSnapshot river = rivers[riverIndex];
                if (!river.IsValid ||
                    !river.TryEvaluate(
                        point,
                        out float distance,
                        out _,
                        out _,
                        out float surfaceHalfWidth))
                {
                    continue;
                }

                if (distance <=
                    river.ResolveHandoffHalfWidth(surfaceHalfWidth) +
                    Mathf.Max(0f, clearance))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsExcludedByModifier(
            Vector2 point,
            IReadOnlyList<GroundModifierSnapshot> modifiers)
        {
            if (modifiers == null)
            {
                return false;
            }

            for (int modifierIndex = 0;
                 modifierIndex < modifiers.Count;
                 modifierIndex++)
            {
                GroundModifierSnapshot modifier = modifiers[modifierIndex];
                if (!modifier.Excludes(
                        GroundSurfaceFeatureExclusionFlags.PaintedAccentLines))
                {
                    continue;
                }

                if (modifier.EvaluateWeight(point) > 0.0001f)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
