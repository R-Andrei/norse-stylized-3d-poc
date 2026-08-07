using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        /// <summary>
        /// Canonical, scale-relative final-triangle condition used by generation
        /// diagnostics and editor audits. It deliberately separates literal
        /// structural invalidity from poor numerical conditioning. No caller
        /// should infer degeneracy from Vector3.normalized returning zero.
        /// </summary>
        public enum FinalTriangleCondition
        {
            Valid = 0,
            NonFinite = 1,
            DuplicateIndices = 2,
            CoincidentPositions = 3,
            ExactDegenerate = 4,
            WindingInvalid = 5,
            NumericallyUnderResolved = 6,
            ExtremeSliver = 7,
            IndexOutOfRange = 8
        }

        public readonly struct FinalTriangleQuality
        {
            public readonly FinalTriangleCondition PrimaryCondition;
            public readonly bool IsFinite;
            public readonly bool HasValidIndexRange;
            public readonly bool HasDistinctIndices;
            public readonly bool HasDistinctPositions;
            public readonly bool IsExactDegenerate;
            public readonly bool IsNumericallyUnderResolved;
            public readonly bool IsExtremeSliver;
            public readonly bool IsWindingInvalid;
            public readonly bool IsNormalizationSafe;
            public readonly double DoubleArea;
            public readonly double NormalizedDoubleArea;
            public readonly double LongestEdge;
            public readonly double ShortestAltitude;
            public readonly double AspectRatio;
            public readonly double MinimumAngleDegrees;
            public readonly double WindingDot;
            public readonly Vector3 GeometricNormal;

            public bool IsStructurallyInvalid =>
                !IsFinite ||
                !HasValidIndexRange ||
                !HasDistinctIndices ||
                !HasDistinctPositions ||
                IsExactDegenerate ||
                IsWindingInvalid;

            public bool IsConditionedForDifferentialAnalysis =>
                !IsStructurallyInvalid &&
                !IsNumericallyUnderResolved &&
                !IsExtremeSliver;

            internal FinalTriangleQuality(
                FinalTriangleCondition primaryCondition,
                bool isFinite,
                bool hasValidIndexRange,
                bool hasDistinctIndices,
                bool hasDistinctPositions,
                bool isExactDegenerate,
                bool isNumericallyUnderResolved,
                bool isExtremeSliver,
                bool isWindingInvalid,
                bool isNormalizationSafe,
                double doubleArea,
                double normalizedDoubleArea,
                double longestEdge,
                double shortestAltitude,
                double aspectRatio,
                double minimumAngleDegrees,
                double windingDot,
                Vector3 geometricNormal)
            {
                PrimaryCondition = primaryCondition;
                IsFinite = isFinite;
                HasValidIndexRange = hasValidIndexRange;
                HasDistinctIndices = hasDistinctIndices;
                HasDistinctPositions = hasDistinctPositions;
                IsExactDegenerate = isExactDegenerate;
                IsNumericallyUnderResolved = isNumericallyUnderResolved;
                IsExtremeSliver = isExtremeSliver;
                IsWindingInvalid = isWindingInvalid;
                IsNormalizationSafe = isNormalizationSafe;
                DoubleArea = doubleArea;
                NormalizedDoubleArea = normalizedDoubleArea;
                LongestEdge = longestEdge;
                ShortestAltitude = shortestAltitude;
                AspectRatio = aspectRatio;
                MinimumAngleDegrees = minimumAngleDegrees;
                WindingDot = windingDot;
                GeometricNormal = geometricNormal;
            }
        }

        // These are diagnostic-contract constants, not artist controls.
        private const double FinalTriangleExactAreaScale = 1e-12;
        private const double FinalTriangleCoincidentEdgeScale = 1e-7;
        private const double FinalTriangleUnderResolvedAreaScale = 1e-5;
        private const double FinalTriangleUnderResolvedAltitudeScale = 1e-5;
        private const double FinalTriangleExtremeAspectRatio = 100.0;
        private const double FinalTriangleExtremeMinimumAngleDegrees = 1.0;

        public static FinalTriangleQuality EvaluateFinalTriangleQuality(
            IReadOnlyList<Vector3> vertices,
            int indexA,
            int indexB,
            int indexC,
            Vector3 expectedNormal = default)
        {
            bool validRange =
                vertices != null &&
                indexA >= 0 && indexA < vertices.Count &&
                indexB >= 0 && indexB < vertices.Count &&
                indexC >= 0 && indexC < vertices.Count;
            if (!validRange)
            {
                return new FinalTriangleQuality(
                    FinalTriangleCondition.IndexOutOfRange,
                    false,
                    false,
                    indexA != indexB && indexB != indexC && indexC != indexA,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    double.PositiveInfinity,
                    0.0,
                    0.0,
                    Vector3.zero);
            }
            return EvaluateFinalTriangleQuality(
                vertices[indexA],
                vertices[indexB],
                vertices[indexC],
                indexA,
                indexB,
                indexC,
                expectedNormal);
        }

        public static FinalTriangleQuality EvaluateFinalTriangleQuality(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            int indexA = -1,
            int indexB = -1,
            int indexC = -1,
            Vector3 expectedNormal = default)
        {
            bool finite = IsTriangleQualityFinite(a) &&
                IsTriangleQualityFinite(b) &&
                IsTriangleQualityFinite(c);
            bool distinctIndices =
                indexA < 0 || indexB < 0 || indexC < 0 ||
                (indexA != indexB && indexB != indexC && indexC != indexA);

            if (!finite)
            {
                return new FinalTriangleQuality(
                    FinalTriangleCondition.NonFinite,
                    false,
                    true,
                    distinctIndices,
                    false,
                    true,
                    true,
                    true,
                    false,
                    false,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    double.PositiveInfinity,
                    0.0,
                    0.0,
                    Vector3.zero);
            }

            double abx = (double)b.x - a.x;
            double aby = (double)b.y - a.y;
            double abz = (double)b.z - a.z;
            double bcx = (double)c.x - b.x;
            double bcy = (double)c.y - b.y;
            double bcz = (double)c.z - b.z;
            double cax = (double)a.x - c.x;
            double cay = (double)a.y - c.y;
            double caz = (double)a.z - c.z;

            double abSq = abx * abx + aby * aby + abz * abz;
            double bcSq = bcx * bcx + bcy * bcy + bcz * bcz;
            double caSq = cax * cax + cay * cay + caz * caz;
            double longestSq = Math.Max(abSq, Math.Max(bcSq, caSq));
            double longest = Math.Sqrt(Math.Max(0.0, longestSq));
            double positionTolerance = Math.Max(
                1e-8,
                longest * FinalTriangleCoincidentEdgeScale);
            double positionToleranceSq = positionTolerance * positionTolerance;
            bool distinctPositions =
                abSq > positionToleranceSq &&
                bcSq > positionToleranceSq &&
                caSq > positionToleranceSq;

            double acx = (double)c.x - a.x;
            double acy = (double)c.y - a.y;
            double acz = (double)c.z - a.z;
            double crossX = aby * acz - abz * acy;
            double crossY = abz * acx - abx * acz;
            double crossZ = abx * acy - aby * acx;
            double doubleArea = Math.Sqrt(
                Math.Max(
                    0.0,
                    crossX * crossX +
                    crossY * crossY +
                    crossZ * crossZ));
            double scaleArea = Math.Max(longestSq, 1e-30);
            double normalizedDoubleArea = doubleArea / scaleArea;
            double exactAreaFloor = Math.Max(1e-15, scaleArea * FinalTriangleExactAreaScale);
            bool exactDegenerate =
                !distinctPositions ||
                doubleArea <= exactAreaFloor;

            double shortestAltitude =
                longest > 1e-30 ? doubleArea / longest : 0.0;
            double aspectRatio =
                shortestAltitude > 1e-30
                    ? longest / shortestAltitude
                    : double.PositiveInfinity;
            double minimumAngle = CalculateMinimumTriangleAngleDegrees(
                abSq,
                bcSq,
                caSq);
            bool underResolved =
                !exactDegenerate &&
                (normalizedDoubleArea < FinalTriangleUnderResolvedAreaScale ||
                 shortestAltitude < Math.Max(
                     1e-8,
                     longest * FinalTriangleUnderResolvedAltitudeScale));
            bool extremeSliver =
                !exactDegenerate &&
                (aspectRatio > FinalTriangleExtremeAspectRatio ||
                 minimumAngle < FinalTriangleExtremeMinimumAngleDegrees);

            Vector3 geometricNormal = Vector3.zero;
            bool normalizationSafe = !exactDegenerate && doubleArea > 1e-30;
            if (normalizationSafe)
            {
                double inverse = 1.0 / doubleArea;
                geometricNormal = new Vector3(
                    (float)(crossX * inverse),
                    (float)(crossY * inverse),
                    (float)(crossZ * inverse));
            }

            double expectedMagnitude = Math.Sqrt(
                (double)expectedNormal.x * expectedNormal.x +
                (double)expectedNormal.y * expectedNormal.y +
                (double)expectedNormal.z * expectedNormal.z);
            double windingDot = 1.0;
            bool windingInvalid = false;
            if (normalizationSafe && expectedMagnitude > 1e-12)
            {
                double inverseExpected = 1.0 / expectedMagnitude;
                windingDot =
                    geometricNormal.x * expectedNormal.x * inverseExpected +
                    geometricNormal.y * expectedNormal.y * inverseExpected +
                    geometricNormal.z * expectedNormal.z * inverseExpected;
                // Under-resolved triangles retain a diagnostic winding dot,
                // but their orientation is not promoted into structural
                // invalidity until the canonical geometry is conditioned
                // enough for a reliable sign decision.
                windingInvalid = !underResolved && windingDot <= 0.0;
            }

            FinalTriangleCondition condition;
            if (!distinctIndices)
            {
                condition = FinalTriangleCondition.DuplicateIndices;
            }
            else if (!distinctPositions)
            {
                condition = FinalTriangleCondition.CoincidentPositions;
            }
            else if (exactDegenerate)
            {
                condition = FinalTriangleCondition.ExactDegenerate;
            }
            else if (windingInvalid)
            {
                condition = FinalTriangleCondition.WindingInvalid;
            }
            else if (underResolved)
            {
                condition = FinalTriangleCondition.NumericallyUnderResolved;
            }
            else if (extremeSliver)
            {
                condition = FinalTriangleCondition.ExtremeSliver;
            }
            else
            {
                condition = FinalTriangleCondition.Valid;
            }

            return new FinalTriangleQuality(
                condition,
                true,
                true,
                distinctIndices,
                distinctPositions,
                exactDegenerate,
                underResolved,
                extremeSliver,
                windingInvalid,
                normalizationSafe,
                doubleArea,
                normalizedDoubleArea,
                longest,
                shortestAltitude,
                aspectRatio,
                minimumAngle,
                windingDot,
                geometricNormal);
        }

        private static double CalculateMinimumTriangleAngleDegrees(
            double abSq,
            double bcSq,
            double caSq)
        {
            double ab = Math.Sqrt(Math.Max(0.0, abSq));
            double bc = Math.Sqrt(Math.Max(0.0, bcSq));
            double ca = Math.Sqrt(Math.Max(0.0, caSq));
            if (ab <= 1e-30 || bc <= 1e-30 || ca <= 1e-30)
            {
                return 0.0;
            }

            double angleA = SafeAcosDegrees(
                (abSq + caSq - bcSq) / (2.0 * ab * ca));
            double angleB = SafeAcosDegrees(
                (abSq + bcSq - caSq) / (2.0 * ab * bc));
            double angleC = 180.0 - angleA - angleB;
            return Math.Max(0.0, Math.Min(angleA, Math.Min(angleB, angleC)));
        }

        private static double SafeAcosDegrees(double value)
        {
            value = Math.Max(-1.0, Math.Min(1.0, value));
            return Math.Acos(value) * (180.0 / Math.PI);
        }

        private static bool IsTriangleQualityFinite(Vector3 value)
        {
            return
                IsTriangleQualityFinite(value.x) &&
                IsTriangleQualityFinite(value.y) &&
                IsTriangleQualityFinite(value.z);
        }

        private static bool IsTriangleQualityFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
