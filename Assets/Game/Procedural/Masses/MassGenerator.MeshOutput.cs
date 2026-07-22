using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Shared transformation and mesh output

        private static Vector3 ResolveDimensions(MassRecipe recipe)
        {
            System.Random random =
                CreateRandom(recipe.ShapeSeed, 0x165667B1);

            Vector3 dimensions = GetBaseDimensions(recipe.Archetype);

            float variation = recipe.ShapeDiversity switch
            {
                ShapeDiversity.Restrained => 0.08f,
                ShapeDiversity.Broad => 0.25f,
                ShapeDiversity.Wild => 0.45f,
                _ => 0.25f
            };

            Vector3 proportionFactors = new Vector3(
                RandomRange(random, 1f - variation, 1f + variation),
                RandomRange(random, 1f - variation, 1f + variation),
                RandomRange(random, 1f - variation, 1f + variation));

            float averageFactor =
                (proportionFactors.x +
                 proportionFactors.y +
                 proportionFactors.z) / 3f;

            proportionFactors /= Mathf.Max(0.001f, averageFactor);

            dimensions = Vector3.Scale(dimensions, proportionFactors);
            dimensions = Vector3.Scale(
                dimensions,
                new Vector3(
                    recipe.WidthBias,
                    recipe.HeightBias,
                    recipe.DepthBias));

            dimensions *=
                GetSizeMultiplier(recipe.Size) *
                recipe.FineScale;

            ConstrainArchetypeDimensions(
                recipe.Archetype,
                ref dimensions);

            dimensions.x = Mathf.Max(0.1f, dimensions.x);
            dimensions.y = Mathf.Max(0.1f, dimensions.y);
            dimensions.z = Mathf.Max(0.1f, dimensions.z);

            return dimensions;
        }

        private static void ConstrainArchetypeDimensions(
            MassArchetype archetype,
            ref Vector3 dimensions)
        {
            float maximumHorizontal = Mathf.Max(dimensions.x, dimensions.z);
            float minimumHorizontal = Mathf.Min(dimensions.x, dimensions.z);

            switch (archetype)
            {
                case MassArchetype.StandingStone:
                    dimensions.y = Mathf.Max(
                        dimensions.y,
                        maximumHorizontal * 1.55f);
                    break;

                case MassArchetype.FlatSlab:
                    dimensions.y = Mathf.Min(
                        dimensions.y,
                        minimumHorizontal * 0.42f);
                    break;

                case MassArchetype.LayeredStone:
                    dimensions.y = Mathf.Min(
                        dimensions.y,
                        minimumHorizontal * 0.56f);
                    break;

                case MassArchetype.CarvedMarkerStone:
                    dimensions.y = Mathf.Max(
                        dimensions.y,
                        dimensions.x * 1.95f);
                    dimensions.z = Mathf.Min(
                        dimensions.z,
                        dimensions.x * 0.58f);
                    dimensions.z = Mathf.Max(
                        dimensions.z,
                        dimensions.x * 0.28f);
                    break;

                case MassArchetype.FracturedPillar:
                    dimensions.y = Mathf.Max(
                        dimensions.y,
                        maximumHorizontal * 2.18f);
                    dimensions.x = Mathf.Min(
                        dimensions.x,
                        dimensions.y * 0.46f);
                    dimensions.z = Mathf.Min(
                        dimensions.z,
                        dimensions.y * 0.38f);
                    break;

                case MassArchetype.SquatBoulder:
                    dimensions.y = Mathf.Min(
                        dimensions.y,
                        maximumHorizontal * 0.68f);
                    break;
            }
        }

        private static void ApplyDimensions(
            List<Vector3> positions,
            Vector3 dimensions)
        {
            Vector3 halfDimensions = dimensions * 0.5f;

            for (int i = 0; i < positions.Count; i++)
            {
                positions[i] = Vector3.Scale(
                    positions[i],
                    halfDimensions);
            }
        }

        private struct MassPlacementFrame
        {
            public int ReferenceVertexCount;
            public float LeanMinimumY;
            public float LeanHeight;
            public Vector3 LeanDirection;
            public float LeanDistance;
            public float GroundingMinimumY;
            public float GroundingHeight;
            public float GroundingTop;
            public float GroundingFlatteningStrength;
            public float GroundingBroadeningStrength;
            public float RecenterMinimumY;
            public float ContactBand;
            public Vector2 ContactCentre;
            public Vector3 RecenterOffset;
        }

        private static MassPlacementFrame ResolveAndApplyMassPlacementFrame(
            List<Vector3> referencePositions,
            LeanStyle lean,
            int shapeSeed,
            GroundingStyle grounding)
        {
            MassPlacementFrame frame = default;
            frame.ReferenceVertexCount = referencePositions == null
                ? 0
                : referencePositions.Count;
            if (referencePositions == null ||
                referencePositions.Count == 0)
            {
                return frame;
            }

            ResolveLeanPlacementFrame(
                referencePositions,
                lean,
                shapeSeed,
                ref frame);
            ApplyLean(referencePositions, frame);

            ResolveGroundingPlacementFrame(
                referencePositions,
                grounding,
                ref frame);
            ApplyGrounding(referencePositions, frame);

            ResolveGroundRecenterPlacementFrame(
                referencePositions,
                ref frame);
            ApplyGroundRecenter(referencePositions, frame);

            return frame;
        }

        private static void ResolveLeanPlacementFrame(
            List<Vector3> referencePositions,
            LeanStyle lean,
            int shapeSeed,
            ref MassPlacementFrame frame)
        {
            GetVerticalRange(
                referencePositions,
                out float minimumY,
                out float maximumY);
            frame.LeanMinimumY = minimumY;
            frame.LeanHeight = Mathf.Max(0.001f, maximumY - minimumY);

            float leanAmount = lean switch
            {
                LeanStyle.None => 0f,
                LeanStyle.Subtle => 0.055f,
                LeanStyle.Pronounced => 0.14f,
                _ => 0f
            };
            if (leanAmount <= 0f)
            {
                frame.LeanDirection = Vector3.zero;
                frame.LeanDistance = 0f;
                return;
            }

            System.Random random =
                CreateRandom(shapeSeed, 0x5F3759DF);
            frame.LeanDirection = RandomHorizontalDirection(random);
            Bounds bounds = CalculateBounds(referencePositions);
            frame.LeanDistance = leanAmount *
                Mathf.Max(bounds.size.x, bounds.size.z);
        }

        private static void ResolveGroundingPlacementFrame(
            List<Vector3> referencePositions,
            GroundingStyle grounding,
            ref MassPlacementFrame frame)
        {
            GetGroundingSettings(
                grounding,
                out float bandFraction,
                out float flatteningStrength,
                out float broadeningStrength);
            GetVerticalRange(
                referencePositions,
                out float minimumY,
                out float maximumY);

            frame.GroundingMinimumY = minimumY;
            frame.GroundingHeight = Mathf.Max(
                0.001f,
                maximumY - minimumY);
            frame.GroundingTop = minimumY +
                frame.GroundingHeight * bandFraction;
            frame.GroundingFlatteningStrength = flatteningStrength;
            frame.GroundingBroadeningStrength = broadeningStrength;
        }

        private static void ResolveGroundRecenterPlacementFrame(
            List<Vector3> referencePositions,
            ref MassPlacementFrame frame)
        {
            GetVerticalRange(
                referencePositions,
                out float minimumY,
                out float maximumY);
            float height = Mathf.Max(0.001f, maximumY - minimumY);
            float contactBand = minimumY + height * 0.08f;
            Vector2 contactCentre = Vector2.zero;
            int contactCount = 0;

            for (int positionIndex = 0;
                 positionIndex < referencePositions.Count;
                 positionIndex++)
            {
                Vector3 position = referencePositions[positionIndex];
                if (position.y > contactBand)
                {
                    continue;
                }
                contactCentre += new Vector2(position.x, position.z);
                contactCount++;
            }
            if (contactCount > 0)
            {
                contactCentre /= contactCount;
            }

            frame.RecenterMinimumY = minimumY;
            frame.ContactBand = contactBand;
            frame.ContactCentre = contactCentre;
            frame.RecenterOffset = new Vector3(
                -contactCentre.x,
                -minimumY,
                -contactCentre.y);
        }

        private static void ApplyMassPlacementFrame(
            List<Vector3> positions,
            MassPlacementFrame frame)
        {
            if (positions == null || positions.Count == 0)
            {
                return;
            }
            ApplyLean(positions, frame);
            ApplyGrounding(positions, frame);
            ApplyGroundRecenter(positions, frame);
        }

        private static void ApplyLean(
            List<Vector3> positions,
            MassPlacementFrame frame)
        {
            if (frame.LeanDistance <= 0f)
            {
                return;
            }
            for (int positionIndex = 0;
                 positionIndex < positions.Count;
                 positionIndex++)
            {
                Vector3 position = positions[positionIndex];
                float influence =
                    (position.y - frame.LeanMinimumY) /
                    frame.LeanHeight;
                position += frame.LeanDirection *
                    frame.LeanDistance * influence;
                positions[positionIndex] = position;
            }
        }

        private static void ApplyGrounding(
            List<Vector3> positions,
            MassPlacementFrame frame)
        {
            for (int positionIndex = 0;
                 positionIndex < positions.Count;
                 positionIndex++)
            {
                Vector3 position = positions[positionIndex];
                if (position.y >= frame.GroundingTop)
                {
                    continue;
                }
                float influence = 1f - Mathf.InverseLerp(
                    frame.GroundingMinimumY,
                    frame.GroundingTop,
                    position.y);
                influence = Mathf.SmoothStep(0f, 1f, influence);
                position.y = Mathf.Lerp(
                    position.y,
                    frame.GroundingMinimumY,
                    frame.GroundingFlatteningStrength * influence);
                float broadening = 1f +
                    frame.GroundingBroadeningStrength * influence;
                position.x *= broadening;
                position.z *= broadening;
                positions[positionIndex] = position;
            }
        }

        private static void ApplyGroundRecenter(
            List<Vector3> positions,
            MassPlacementFrame frame)
        {
            for (int positionIndex = 0;
                 positionIndex < positions.Count;
                 positionIndex++)
            {
                positions[positionIndex] += frame.RecenterOffset;
            }
        }

        private const float AuthoredSurfaceNormalScoreTieEpsilon =
            0.000001f;

        private enum AuthoredSurfaceNormalCandidateKind
        {
            AreaWeighted,
            Triangle,
            PairBisector,
            TripleEqualAngle
        }

        private readonly struct AuthoredSurfaceNormalTriangleEvidence
        {
            public readonly int TriangleIndex;
            public readonly Vector3 Normal;
            public readonly double AreaWeight;

            public AuthoredSurfaceNormalTriangleEvidence(
                int triangleIndex,
                Vector3 normal,
                double areaWeight)
            {
                TriangleIndex = triangleIndex;
                Normal = normal;
                AreaWeight = areaWeight;
            }
        }

        private struct AuthoredSurfaceNormalAccumulator
        {
            public double X;
            public double Y;
            public double Z;
            public double TotalAreaWeight;
            public int TriangleCount;

            public void Add(
                Vector3 areaWeightedNormal,
                double areaWeight)
            {
                X += areaWeightedNormal.x;
                Y += areaWeightedNormal.y;
                Z += areaWeightedNormal.z;
                TotalAreaWeight += areaWeight;
                TriangleCount++;
            }

            public Vector3 ResolveSum()
            {
                return new Vector3(
                    (float)X,
                    (float)Y,
                    (float)Z);
            }
        }

        private sealed class AuthoredSurfaceNormalGroupEvidence
        {
            public readonly int SurfaceGroup;
            public readonly List<AuthoredSurfaceNormalTriangleEvidence>
                Triangles =
                    new List<AuthoredSurfaceNormalTriangleEvidence>();
            public AuthoredSurfaceNormalAccumulator Accumulator;
            public bool HasOriginalAuthoredNormal;
            public Vector3 OriginalAuthoredNormal;

            public AuthoredSurfaceNormalGroupEvidence(int surfaceGroup)
            {
                SurfaceGroup = surfaceGroup;
            }
        }

        private readonly struct AuthoredSurfaceNormalCandidate
        {
            public readonly bool Valid;
            public readonly AuthoredSurfaceNormalCandidateKind Kind;
            public readonly Vector3 Normal;
            public readonly float MinimumDot;
            public readonly double AreaWeightedAverageDot;
            public readonly int WorstTriangleIndex;
            public readonly int DefiningTriangleA;
            public readonly int DefiningTriangleB;
            public readonly int DefiningTriangleC;

            public AuthoredSurfaceNormalCandidate(
                AuthoredSurfaceNormalCandidateKind kind,
                Vector3 normal,
                float minimumDot,
                double areaWeightedAverageDot,
                int worstTriangleIndex,
                int definingTriangleA,
                int definingTriangleB,
                int definingTriangleC)
            {
                Valid = true;
                Kind = kind;
                Normal = normal;
                MinimumDot = minimumDot;
                AreaWeightedAverageDot = areaWeightedAverageDot;
                WorstTriangleIndex = worstTriangleIndex;
                DefiningTriangleA = definingTriangleA;
                DefiningTriangleB = definingTriangleB;
                DefiningTriangleC = definingTriangleC;
            }
        }

        private static Dictionary<int, Vector3>
            ResolveTransformedAuthoredSurfaceNormals(
                TriangleSoup soup)
        {
            Dictionary<int, AuthoredSurfaceNormalGroupEvidence> groups =
                new Dictionary<int, AuthoredSurfaceNormalGroupEvidence>();

            for (int triangleOffset = 0;
                 triangleOffset < soup.Positions.Count;
                 triangleOffset += 3)
            {
                if (!soup.TryResolveAuthoredSurfaceGroup(
                        triangleOffset,
                        out int surfaceGroup))
                {
                    continue;
                }

                int triangleIndex = triangleOffset / 3;
                if (!soup.TryResolveAuthoredSurfaceNormal(
                        triangleOffset,
                        out Vector3 authoredSurfaceNormal) ||
                    !TryNormalizeMassVector(
                        authoredSurfaceNormal,
                        out Vector3 normalizedAuthoredSurfaceNormal))
                {
                    throw new InvalidOperationException(
                        "Generated mass authored surface group " +
                        FormatAuthoredSurfaceGroupEvidence(surfaceGroup) +
                        " contains an invalid source normal at triangle " +
                        triangleIndex + ".");
                }

                Vector3 a = soup.Positions[triangleOffset];
                Vector3 b = soup.Positions[triangleOffset + 1];
                Vector3 c = soup.Positions[triangleOffset + 2];
                Vector3 geometricNormal = Vector3.Cross(
                    b - a,
                    c - a);
                if (!TryNormalizeMassVector(
                        geometricNormal,
                        out Vector3 normalizedGeometricNormal))
                {
                    throw new InvalidOperationException(
                        "Generated mass authored surface group " +
                        FormatAuthoredSurfaceGroupEvidence(surfaceGroup) +
                        " contains a triangle with no finite final " +
                        "geometric normal at triangle " +
                        triangleIndex + ".");
                }

                double geometricX = geometricNormal.x;
                double geometricY = geometricNormal.y;
                double geometricZ = geometricNormal.z;
                double areaWeightSqr =
                    geometricX * geometricX +
                    geometricY * geometricY +
                    geometricZ * geometricZ;
                double areaWeight = Math.Sqrt(areaWeightSqr);
                if (!(areaWeight > 0.0) ||
                    double.IsNaN(areaWeight) ||
                    double.IsInfinity(areaWeight))
                {
                    throw new InvalidOperationException(
                        "Generated mass authored surface group " +
                        FormatAuthoredSurfaceGroupEvidence(surfaceGroup) +
                        " contains an invalid final triangle area at " +
                        "triangle " + triangleIndex + ".");
                }

                if (Vector3.Dot(
                        normalizedGeometricNormal,
                        normalizedAuthoredSurfaceNormal) < 0f)
                {
                    geometricNormal = -geometricNormal;
                    normalizedGeometricNormal =
                        -normalizedGeometricNormal;
                }

                if (!groups.TryGetValue(
                        surfaceGroup,
                        out AuthoredSurfaceNormalGroupEvidence group))
                {
                    group = new AuthoredSurfaceNormalGroupEvidence(
                        surfaceGroup);
                    groups.Add(surfaceGroup, group);
                }

                if (!group.HasOriginalAuthoredNormal)
                {
                    group.HasOriginalAuthoredNormal = true;
                    group.OriginalAuthoredNormal =
                        normalizedAuthoredSurfaceNormal;
                }

                group.Accumulator.Add(
                    geometricNormal,
                    areaWeight);
                group.Triangles.Add(
                    new AuthoredSurfaceNormalTriangleEvidence(
                        triangleIndex,
                        normalizedGeometricNormal,
                        areaWeight));
            }

            Dictionary<int, Vector3> resolvedNormals =
                new Dictionary<int, Vector3>(groups.Count);
            foreach (KeyValuePair<int, AuthoredSurfaceNormalGroupEvidence>
                     entry in groups)
            {
                AuthoredSurfaceNormalGroupEvidence group = entry.Value;
                if (!TryResolveBestAuthoredSurfaceNormalCandidate(
                        group,
                        out AuthoredSurfaceNormalCandidate areaWeighted,
                        out AuthoredSurfaceNormalCandidate best))
                {
                    throw new InvalidOperationException(
                        "Generated mass authored surface group " +
                        FormatAuthoredSurfaceGroupEvidence(entry.Key) +
                        " cannot produce any finite shared final render " +
                        "normal from " +
                        group.Triangles.Count +
                        " transformed triangles.");
                }

                if (best.MinimumDot < 0.5f)
                {
                    throw CreateAuthoredSurfaceNormalInfeasibility(
                        group,
                        areaWeighted,
                        best);
                }

                resolvedNormals.Add(entry.Key, best.Normal);
            }

            return resolvedNormals;
        }

        private static bool TryResolveBestAuthoredSurfaceNormalCandidate(
            AuthoredSurfaceNormalGroupEvidence group,
            out AuthoredSurfaceNormalCandidate areaWeighted,
            out AuthoredSurfaceNormalCandidate best)
        {
            areaWeighted = default;
            best = default;
            if (group == null ||
                group.Triangles.Count <= 0 ||
                group.Accumulator.TriangleCount != group.Triangles.Count ||
                !(group.Accumulator.TotalAreaWeight > 0.0))
            {
                return false;
            }

            Vector3 accumulatedNormal = group.Accumulator.ResolveSum();
            if (TryEvaluateAuthoredSurfaceNormalCandidate(
                    group,
                    accumulatedNormal,
                    AuthoredSurfaceNormalCandidateKind.AreaWeighted,
                    int.MaxValue,
                    int.MaxValue,
                    int.MaxValue,
                    out AuthoredSurfaceNormalCandidate areaCandidate))
            {
                areaWeighted = areaCandidate;
                best = areaCandidate;
            }

            for (int firstIndex = 0;
                 firstIndex < group.Triangles.Count;
                 firstIndex++)
            {
                AuthoredSurfaceNormalTriangleEvidence first =
                    group.Triangles[firstIndex];
                ConsiderAuthoredSurfaceNormalCandidate(
                    group,
                    first.Normal,
                    AuthoredSurfaceNormalCandidateKind.Triangle,
                    first.TriangleIndex,
                    int.MaxValue,
                    int.MaxValue,
                    ref best);
            }

            for (int firstIndex = 0;
                 firstIndex < group.Triangles.Count;
                 firstIndex++)
            {
                AuthoredSurfaceNormalTriangleEvidence first =
                    group.Triangles[firstIndex];
                for (int secondIndex = firstIndex + 1;
                     secondIndex < group.Triangles.Count;
                     secondIndex++)
                {
                    AuthoredSurfaceNormalTriangleEvidence second =
                        group.Triangles[secondIndex];
                    ConsiderAuthoredSurfaceNormalCandidate(
                        group,
                        first.Normal + second.Normal,
                        AuthoredSurfaceNormalCandidateKind.PairBisector,
                        first.TriangleIndex,
                        second.TriangleIndex,
                        int.MaxValue,
                        ref best);
                }
            }

            for (int firstIndex = 0;
                 firstIndex < group.Triangles.Count;
                 firstIndex++)
            {
                AuthoredSurfaceNormalTriangleEvidence first =
                    group.Triangles[firstIndex];
                for (int secondIndex = firstIndex + 1;
                     secondIndex < group.Triangles.Count;
                     secondIndex++)
                {
                    AuthoredSurfaceNormalTriangleEvidence second =
                        group.Triangles[secondIndex];
                    for (int thirdIndex = secondIndex + 1;
                         thirdIndex < group.Triangles.Count;
                         thirdIndex++)
                    {
                        AuthoredSurfaceNormalTriangleEvidence third =
                            group.Triangles[thirdIndex];
                        Vector3 equalAngleAxis = Vector3.Cross(
                            first.Normal - second.Normal,
                            first.Normal - third.Normal);
                        ConsiderAuthoredSurfaceNormalCandidate(
                            group,
                            equalAngleAxis,
                            AuthoredSurfaceNormalCandidateKind
                                .TripleEqualAngle,
                            first.TriangleIndex,
                            second.TriangleIndex,
                            third.TriangleIndex,
                            ref best);
                        ConsiderAuthoredSurfaceNormalCandidate(
                            group,
                            -equalAngleAxis,
                            AuthoredSurfaceNormalCandidateKind
                                .TripleEqualAngle,
                            first.TriangleIndex,
                            second.TriangleIndex,
                            third.TriangleIndex,
                            ref best);
                    }
                }
            }

            return best.Valid;
        }

        private static void ConsiderAuthoredSurfaceNormalCandidate(
            AuthoredSurfaceNormalGroupEvidence group,
            Vector3 candidateNormal,
            AuthoredSurfaceNormalCandidateKind kind,
            int definingTriangleA,
            int definingTriangleB,
            int definingTriangleC,
            ref AuthoredSurfaceNormalCandidate best)
        {
            if (!TryEvaluateAuthoredSurfaceNormalCandidate(
                    group,
                    candidateNormal,
                    kind,
                    definingTriangleA,
                    definingTriangleB,
                    definingTriangleC,
                    out AuthoredSurfaceNormalCandidate candidate))
            {
                return;
            }

            if (IsBetterAuthoredSurfaceNormalCandidate(
                    candidate,
                    best))
            {
                best = candidate;
            }
        }

        private static bool TryEvaluateAuthoredSurfaceNormalCandidate(
            AuthoredSurfaceNormalGroupEvidence group,
            Vector3 candidateNormal,
            AuthoredSurfaceNormalCandidateKind kind,
            int definingTriangleA,
            int definingTriangleB,
            int definingTriangleC,
            out AuthoredSurfaceNormalCandidate candidate)
        {
            candidate = default;
            if (group == null ||
                group.Triangles.Count <= 0 ||
                !TryNormalizeMassVector(
                    candidateNormal,
                    out Vector3 normalizedCandidate))
            {
                return false;
            }

            float minimumDot = float.PositiveInfinity;
            int worstTriangleIndex = int.MaxValue;
            double weightedDotSum = 0.0;
            double totalAreaWeight = 0.0;
            for (int triangleIndex = 0;
                 triangleIndex < group.Triangles.Count;
                 triangleIndex++)
            {
                AuthoredSurfaceNormalTriangleEvidence triangle =
                    group.Triangles[triangleIndex];
                float dot = Vector3.Dot(
                    normalizedCandidate,
                    triangle.Normal);
                if (!IsFiniteMassValue(dot) ||
                    !(triangle.AreaWeight > 0.0) ||
                    double.IsNaN(triangle.AreaWeight) ||
                    double.IsInfinity(triangle.AreaWeight))
                {
                    return false;
                }

                dot = Mathf.Clamp(dot, -1f, 1f);
                if (dot < minimumDot)
                {
                    minimumDot = dot;
                    worstTriangleIndex = triangle.TriangleIndex;
                }
                else if (Mathf.Abs(dot - minimumDot) <=
                             AuthoredSurfaceNormalScoreTieEpsilon &&
                         triangle.TriangleIndex < worstTriangleIndex)
                {
                    worstTriangleIndex = triangle.TriangleIndex;
                }

                weightedDotSum += triangle.AreaWeight * dot;
                totalAreaWeight += triangle.AreaWeight;
            }

            if (!IsFiniteMassValue(minimumDot) ||
                worstTriangleIndex == int.MaxValue ||
                !(totalAreaWeight > 0.0))
            {
                return false;
            }

            double areaWeightedAverageDot =
                weightedDotSum / totalAreaWeight;
            if (double.IsNaN(areaWeightedAverageDot) ||
                double.IsInfinity(areaWeightedAverageDot))
            {
                return false;
            }

            candidate = new AuthoredSurfaceNormalCandidate(
                kind,
                normalizedCandidate,
                minimumDot,
                areaWeightedAverageDot,
                worstTriangleIndex,
                definingTriangleA,
                definingTriangleB,
                definingTriangleC);
            return true;
        }

        private static bool IsBetterAuthoredSurfaceNormalCandidate(
            AuthoredSurfaceNormalCandidate candidate,
            AuthoredSurfaceNormalCandidate best)
        {
            if (!candidate.Valid)
            {
                return false;
            }
            if (!best.Valid)
            {
                return true;
            }

            if (candidate.MinimumDot > best.MinimumDot)
            {
                return true;
            }
            if (candidate.MinimumDot < best.MinimumDot)
            {
                return false;
            }

            if (candidate.AreaWeightedAverageDot >
                best.AreaWeightedAverageDot)
            {
                return true;
            }
            if (candidate.AreaWeightedAverageDot <
                best.AreaWeightedAverageDot)
            {
                return false;
            }

            int definingComparison = CompareAuthoredSurfaceDefinition(
                candidate.DefiningTriangleA,
                candidate.DefiningTriangleB,
                candidate.DefiningTriangleC,
                best.DefiningTriangleA,
                best.DefiningTriangleB,
                best.DefiningTriangleC);
            if (definingComparison != 0)
            {
                return definingComparison < 0;
            }

            return (int)candidate.Kind < (int)best.Kind;
        }

        private static int CompareAuthoredSurfaceDefinition(
            int candidateA,
            int candidateB,
            int candidateC,
            int bestA,
            int bestB,
            int bestC)
        {
            int comparison = candidateA.CompareTo(bestA);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = candidateB.CompareTo(bestB);
            return comparison != 0
                ? comparison
                : candidateC.CompareTo(bestC);
        }

        private static InvalidOperationException
            CreateAuthoredSurfaceNormalInfeasibility(
                AuthoredSurfaceNormalGroupEvidence group,
                AuthoredSurfaceNormalCandidate areaWeighted,
                AuthoredSurfaceNormalCandidate best)
        {
            System.Text.StringBuilder builder =
                new System.Text.StringBuilder();
            builder.Append(
                "Generated mass authored surface group ");
            builder.Append(
                FormatAuthoredSurfaceGroupEvidence(group.SurfaceGroup));
            builder.Append(
                " has no shared final render normal satisfying the " +
                "required 0.5 agreement. triangleCount=");
            builder.Append(group.Triangles.Count);
            builder.Append(",originalAuthoredNormal=");
            builder.Append(
                FormatMassVectorEvidence(group.OriginalAuthoredNormal));
            builder.Append(",areaWeighted=");
            AppendAuthoredSurfaceNormalCandidateEvidence(
                builder,
                areaWeighted);
            builder.Append(",bestFeasibilityCandidate=");
            AppendAuthoredSurfaceNormalCandidateEvidence(
                builder,
                best);
            builder.Append(",triangles=[");
            for (int triangleIndex = 0;
                 triangleIndex < group.Triangles.Count;
                 triangleIndex++)
            {
                if (triangleIndex > 0)
                {
                    builder.Append(';');
                }

                AuthoredSurfaceNormalTriangleEvidence triangle =
                    group.Triangles[triangleIndex];
                builder.Append("index:");
                builder.Append(triangle.TriangleIndex);
                builder.Append(",normal:");
                builder.Append(
                    FormatMassVectorEvidence(triangle.Normal));
                builder.Append(",areaWeight:");
                builder.Append(
                    FormatMassDoubleEvidence(triangle.AreaWeight));
            }
            builder.Append("].");
            return new InvalidOperationException(builder.ToString());
        }

        private static void AppendAuthoredSurfaceNormalCandidateEvidence(
            System.Text.StringBuilder builder,
            AuthoredSurfaceNormalCandidate candidate)
        {
            if (!candidate.Valid)
            {
                builder.Append("none");
                return;
            }

            builder.Append("{kind:");
            builder.Append(candidate.Kind);
            builder.Append(",definition:");
            AppendAuthoredSurfaceDefinitionEvidence(
                builder,
                candidate.DefiningTriangleA,
                candidate.DefiningTriangleB,
                candidate.DefiningTriangleC);
            builder.Append(",minimumDot:");
            builder.Append(
                FormatMassFloatEvidence(candidate.MinimumDot));
            builder.Append(",averageDot:");
            builder.Append(
                FormatMassDoubleEvidence(
                    candidate.AreaWeightedAverageDot));
            builder.Append(",worstTriangle:");
            builder.Append(candidate.WorstTriangleIndex);
            builder.Append(",normal:");
            builder.Append(
                FormatMassVectorEvidence(candidate.Normal));
            builder.Append('}');
        }

        private static void AppendAuthoredSurfaceDefinitionEvidence(
            System.Text.StringBuilder builder,
            int triangleA,
            int triangleB,
            int triangleC)
        {
            builder.Append('[');
            if (triangleA != int.MaxValue)
            {
                builder.Append(triangleA);
            }
            if (triangleB != int.MaxValue)
            {
                builder.Append('/');
                builder.Append(triangleB);
            }
            if (triangleC != int.MaxValue)
            {
                builder.Append('/');
                builder.Append(triangleC);
            }
            builder.Append(']');
        }

        private static string FormatAuthoredSurfaceGroupEvidence(
            int surfaceGroup)
        {
            string encoded = surfaceGroup.ToString(
                "X8",
                System.Globalization.CultureInfo.InvariantCulture);
            if (TryDecodeAuthoredSurfaceGroup(
                    surfaceGroup,
                    0x3A710000,
                    "ordinary",
                    out string ordinaryEvidence))
            {
                return surfaceGroup +
                    "(0x" + encoded + ")," +
                    ordinaryEvidence;
            }
            if (TryDecodeAuthoredSurfaceGroup(
                    surfaceGroup,
                    0x4B1D0000,
                    "bevel",
                    out string bevelEvidence))
            {
                return surfaceGroup +
                    "(0x" + encoded + ")," +
                    bevelEvidence;
            }

            return surfaceGroup + "(0x" + encoded + ")";
        }

        private static bool TryDecodeAuthoredSurfaceGroup(
            int surfaceGroup,
            int prefix,
            string groupClass,
            out string evidence)
        {
            evidence = string.Empty;
            int payload = surfaceGroup ^ prefix;
            int provenanceValue = payload >> 20;
            int maximumProvenance =
                (int)PolygonFaceProvenanceKind.CornerDamageCap;
            if (provenanceValue < 0 ||
                provenanceValue > maximumProvenance)
            {
                return false;
            }

            int identity = payload & 0x000FFFFF;
            evidence = "groupClass=" + groupClass +
                ",provenance=" +
                (PolygonFaceProvenanceKind)provenanceValue +
                ':' + identity;
            return true;
        }

        private static string FormatMassDoubleEvidence(double value)
        {
            return value.ToString(
                "R",
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private static void ValidateTransformedAuthoredSurfaceTriangle(
            int triangleIndex,
            int surfaceGroup,
            Vector3 originalAuthoredNormal,
            Vector3 rebuiltSurfaceNormal,
            Vector3 geometricNormal,
            Vector3 a,
            Vector3 b,
            Vector3 c)
        {
            if (!TryNormalizeMassVector(
                    geometricNormal,
                    out Vector3 normalizedGeometricNormal))
            {
                throw new InvalidOperationException(
                    "Generated mass authored surface group " +
                    surfaceGroup +
                    " triangle " +
                    triangleIndex +
                    " cannot produce a finite final geometric normal. " +
                    "originalAuthoredNormal=" +
                    FormatMassVectorEvidence(originalAuthoredNormal) +
                    ",rebuiltSurfaceNormal=" +
                    FormatMassVectorEvidence(rebuiltSurfaceNormal) +
                    ",a=" +
                    FormatMassVectorEvidence(a) +
                    ",b=" +
                    FormatMassVectorEvidence(b) +
                    ",c=" +
                    FormatMassVectorEvidence(c) +
                    ".");
            }

            float normalDot = Vector3.Dot(
                normalizedGeometricNormal,
                rebuiltSurfaceNormal);
            if (!IsFiniteMassValue(normalDot) || normalDot < 0.5f)
            {
                throw new InvalidOperationException(
                    "Generated mass authored surface group " +
                    surfaceGroup +
                    " triangle " +
                    triangleIndex +
                    " contains a final render normal that disagrees " +
                    "with its winding. normalDot=" +
                    FormatMassFloatEvidence(normalDot) +
                    ",originalAuthoredNormal=" +
                    FormatMassVectorEvidence(originalAuthoredNormal) +
                    ",rebuiltSurfaceNormal=" +
                    FormatMassVectorEvidence(rebuiltSurfaceNormal) +
                    ",geometricNormal=" +
                    FormatMassVectorEvidence(
                        normalizedGeometricNormal) +
                    ",a=" +
                    FormatMassVectorEvidence(a) +
                    ",b=" +
                    FormatMassVectorEvidence(b) +
                    ",c=" +
                    FormatMassVectorEvidence(c) +
                    ".");
            }
        }

        private static string FormatMassFloatEvidence(float value)
        {
            return FormattableString.Invariant($"{value:R}");
        }

        private static string FormatMassVectorEvidence(Vector3 value)
        {
            return FormattableString.Invariant(
                $"({value.x:R}/{value.y:R}/{value.z:R})");
        }

        private static MeshData BuildMeshData(
            TriangleSoup soup,
            MassRecipe recipe)
        {
            if (soup.Positions.Count < 3 ||
                soup.Positions.Count % 3 != 0)
            {
                throw new InvalidOperationException(
                    "Generated mass triangle soup is invalid.");
            }

            MeshData meshData = new MeshData();
            Vector3 centre = CalculateAverage(soup.Positions);
            Bounds bounds = CalculateBounds(soup.Positions);
            float safeWidth = Mathf.Max(0.001f, bounds.size.x);
            float safeHeight = Mathf.Max(0.001f, bounds.size.y);
            float safeDepth = Mathf.Max(0.001f, bounds.size.z);
            Dictionary<int, Vector3> transformedAuthoredSurfaceNormals =
                ResolveTransformedAuthoredSurfaceNormals(soup);

            for (int i = 0; i < soup.Positions.Count; i += 3)
            {
                int faceIndex = i / 3;
                Vector3 a = soup.Positions[i];
                Vector3 b = soup.Positions[i + 1];
                Vector3 c = soup.Positions[i + 2];

                Vector3 normal = Vector3.Cross(b - a, c - a);
                bool hasAuthoredSurfaceNormal =
                    soup.TryResolveAuthoredSurfaceNormal(
                        i,
                        out Vector3 authoredSurfaceNormal);
                bool hasAuthoredSurfaceGroup =
                    soup.TryResolveAuthoredSurfaceGroup(
                        i,
                        out int authoredSurfaceGroup);
                Vector3 faceNormal;
                if (hasAuthoredSurfaceGroup)
                {
                    if (!hasAuthoredSurfaceNormal)
                    {
                        throw new InvalidOperationException(
                            "Generated mass authored surface group " +
                            authoredSurfaceGroup +
                            " is missing its source render normal at " +
                            "triangle " + faceIndex + ".");
                    }
                    if (!transformedAuthoredSurfaceNormals.TryGetValue(
                            authoredSurfaceGroup,
                            out faceNormal))
                    {
                        throw new InvalidOperationException(
                            "Generated mass authored surface group " +
                            authoredSurfaceGroup +
                            " has no rebuilt final render normal at " +
                            "triangle " + faceIndex + ".");
                    }
                    if (Vector3.Dot(normal, faceNormal) < 0f)
                    {
                        Vector3 temporary = b;
                        b = c;
                        c = temporary;
                        normal = -normal;
                    }

                    ValidateTransformedAuthoredSurfaceTriangle(
                        faceIndex,
                        authoredSurfaceGroup,
                        authoredSurfaceNormal,
                        faceNormal,
                        normal,
                        a,
                        b,
                        c);
                }
                else if (hasAuthoredSurfaceNormal)
                {
                    if (!TryNormalizeMassVector(
                            authoredSurfaceNormal,
                            out faceNormal))
                    {
                        throw new InvalidOperationException(
                            "Generated mass face " + faceIndex +
                            " contains an invalid authored render normal.");
                    }
                    if (Vector3.Dot(normal, faceNormal) < 0f)
                    {
                        Vector3 temporary = b;
                        b = c;
                        c = temporary;
                        normal = -normal;
                    }
                }
                else
                {
                    Vector3 faceCentre = (a + b + c) / 3f;
                    if (Vector3.Dot(normal, faceCentre - centre) < 0f)
                    {
                        Vector3 temporary = b;
                        b = c;
                        c = temporary;
                        normal = -normal;
                    }

                    if (!TryNormalizeMassVector(normal, out faceNormal))
                    {
                        throw new InvalidOperationException(
                            "Generated mass face " + faceIndex +
                            " cannot produce a finite unit render normal " +
                            "from its accepted triangle geometry.");
                    }
                }
                PolygonFaceFeature faceFeature = soup.ResolveFeature(i);
                float faceFeatureStrength = soup.ResolveFeatureStrength(i);

                int indexA = AddRenderedVertex(
                    meshData,
                    a,
                    i,
                    0,
                    faceIndex,
                    bounds,
                    safeWidth,
                    safeHeight,
                    safeDepth,
                    faceNormal,
                    hasAuthoredSurfaceGroup,
                    authoredSurfaceGroup,
                    recipe,
                    faceFeature,
                    faceFeatureStrength);

                int indexB = AddRenderedVertex(
                    meshData,
                    b,
                    i + 1,
                    1,
                    faceIndex,
                    bounds,
                    safeWidth,
                    safeHeight,
                    safeDepth,
                    faceNormal,
                    hasAuthoredSurfaceGroup,
                    authoredSurfaceGroup,
                    recipe,
                    faceFeature,
                    faceFeatureStrength);

                int indexC = AddRenderedVertex(
                    meshData,
                    c,
                    i + 2,
                    2,
                    faceIndex,
                    bounds,
                    safeWidth,
                    safeHeight,
                    safeDepth,
                    faceNormal,
                    hasAuthoredSurfaceGroup,
                    authoredSurfaceGroup,
                    recipe,
                    faceFeature,
                    faceFeatureStrength);

                meshData.AddTriangle(indexA, indexB, indexC);
            }

            ValidateGeneratedMassMeshData(meshData);
            return meshData;
        }

        private static void ValidateGeneratedMassMeshData(
            MeshData meshData)
        {
            if (meshData == null)
            {
                throw new ArgumentNullException(nameof(meshData));
            }
            if (meshData.Vertices.Count < 3 ||
                meshData.Triangles.Count == 0 ||
                meshData.Triangles.Count % 3 != 0 ||
                meshData.Normals.Count != meshData.Vertices.Count ||
                meshData.UV0.Count != meshData.Vertices.Count ||
                meshData.Colors.Count != meshData.Vertices.Count ||
                meshData.UV2.Count != meshData.Vertices.Count)
            {
                throw new InvalidOperationException(
                    "Generated mass render channels are incomplete.");
            }

            for (int vertexIndex = 0;
                 vertexIndex < meshData.Vertices.Count;
                 vertexIndex++)
            {
                Vector3 position = meshData.Vertices[vertexIndex];
                Vector3 normal = meshData.Normals[vertexIndex];
                Vector2 uv0 = meshData.UV0[vertexIndex];
                Color color = meshData.Colors[vertexIndex];
                Vector4 uv2 = meshData.UV2[vertexIndex];
                if (!IsFiniteMassVector(position) ||
                    !TryNormalizeMassVector(normal, out _) ||
                    !IsFiniteMassValue(uv0.x) ||
                    !IsFiniteMassValue(uv0.y) ||
                    !IsFiniteMassValue(color.r) ||
                    !IsFiniteMassValue(color.g) ||
                    !IsFiniteMassValue(color.b) ||
                    !IsFiniteMassValue(color.a) ||
                    !IsFiniteMassValue(uv2.x) ||
                    !IsFiniteMassValue(uv2.y) ||
                    !IsFiniteMassValue(uv2.z) ||
                    !IsFiniteMassValue(uv2.w))
                {
                    throw new InvalidOperationException(
                        "Generated mass render channel is invalid at " +
                        "vertex " + vertexIndex + ".");
                }
            }

            for (int triangleOffset = 0;
                 triangleOffset < meshData.Triangles.Count;
                 triangleOffset += 3)
            {
                int indexA = meshData.Triangles[triangleOffset];
                int indexB = meshData.Triangles[triangleOffset + 1];
                int indexC = meshData.Triangles[triangleOffset + 2];
                if (indexA < 0 || indexA >= meshData.Vertices.Count ||
                    indexB < 0 || indexB >= meshData.Vertices.Count ||
                    indexC < 0 || indexC >= meshData.Vertices.Count)
                {
                    throw new InvalidOperationException(
                        "Generated mass triangle contains an invalid " +
                        "vertex index.");
                }

                Vector3 geometricNormal = Vector3.Cross(
                    meshData.Vertices[indexB] -
                        meshData.Vertices[indexA],
                    meshData.Vertices[indexC] -
                        meshData.Vertices[indexA]);
                if (!TryNormalizeMassVector(
                        geometricNormal,
                        out Vector3 normalizedGeometricNormal))
                {
                    throw new InvalidOperationException(
                        "Generated mass triangle " +
                        (triangleOffset / 3) +
                        " cannot produce a finite geometric normal.");
                }

                float minimumNormalDot = Mathf.Min(
                    Vector3.Dot(
                        normalizedGeometricNormal,
                        meshData.Normals[indexA]),
                    Mathf.Min(
                        Vector3.Dot(
                            normalizedGeometricNormal,
                            meshData.Normals[indexB]),
                        Vector3.Dot(
                            normalizedGeometricNormal,
                            meshData.Normals[indexC])));
                if (!IsFiniteMassValue(minimumNormalDot) ||
                    minimumNormalDot < 0.5f)
                {
                    throw new InvalidOperationException(
                        "Generated mass triangle " +
                        (triangleOffset / 3) +
                        " contains a render normal that disagrees with " +
                        "its winding.");
                }
            }
        }

        private static int AddRenderedVertex(
            MeshData meshData,
            Vector3 position,
            int vertexIndex,
            int cornerIndex,
            int faceIndex,
            Bounds bounds,
            float width,
            float height,
            float depth,
            Vector3 faceNormal,
            bool hasAuthoredSurfaceGroup,
            int authoredSurfaceGroup,
            MassRecipe recipe,
            PolygonFaceFeature faceFeature,
            float faceFeatureStrength)
        {
            Vector2 uv = new Vector2(
                (position.x - bounds.min.x) / width,
                (position.z - bounds.min.z) / depth);

            int surfaceVariationIndex = hasAuthoredSurfaceGroup
                ? authoredSurfaceGroup
                : vertexIndex;
            float randomValue = Hash01(
                recipe.SurfaceSeed,
                surfaceVariationIndex);

            float red = Mathf.Clamp01(
                0.5f +
                (randomValue - 0.5f) *
                recipe.SurfaceVariation);

            float vertical01 = Mathf.Clamp01(
                (position.y - bounds.min.y) / height);

            float green = ResolveExposureMask(
                faceNormal,
                vertical01,
                randomValue);

            float blue = ResolveCreviceMask(
                faceNormal,
                vertical01,
                green,
                randomValue);

            // Geometry-first edge wear writes only actual generated bevel faces.
            // Broad interpolated masks remain safe here, but line-like edge wear
            // must not be reconstructed from vertex gradients or packed atlases.
            float edgeWear = faceFeature == PolygonFaceFeature.ConvexEdgeWear
                ? Mathf.Clamp01(faceFeatureStrength)
                : 0f;
            float concaveCrease = 0f;

            float dirtDeposit = ResolveDirtDepositMask(
                position,
                vertical01,
                green,
                blue,
                recipe,
                0f);

            Vector4 materialMasks = new Vector4(
                concaveCrease,
                dirtDeposit,
                edgeWear,
                0f);

            int renderedVertex = meshData.AddVertex(
                position,
                uv,
                new Color(red, green, blue, edgeWear),
                materialMasks);
            meshData.Normals.Add(faceNormal);
            return renderedVertex;
        }

        // Vertex colour material contract:
        // R = existing deterministic surface variation.
        // G = upward/flat exposure mask for lighter worn or frosted planes.
        // B = base/side/occlusion mask for darker crevice-like broad grounding.
        // A = generated convex edge-wear strength on actual bevel/chamfer faces only.
        //     It mirrors UV2.z for inspection/backward compatibility.
        //
        // UV2 material contract:
        // X = reserved for future concave crease or selected crack-darkening strength.
        // Y = dirty deposit / mineral stain area mask.
        // Z = generated convex edge-wear strength on actual bevel/chamfer faces.
        // W = reserved for future concave crease localization data.
        //
        // Line-like features need a later line/overlay or per-edge representation.
        // Broad area features remain safe as vertex/interpolated masks.
        private static float ResolveExposureMask(
            Vector3 faceNormal,
            float vertical01,
            float randomValue)
        {
            float upward = Mathf.Clamp01(faceNormal.y);
            float flatness = Mathf.Pow(upward, 1.65f);
            float upperSurface = Mathf.SmoothStep(0.08f, 0.82f, vertical01);
            float surfaceBreakup = (randomValue - 0.5f) * 0.08f;

            return Mathf.Clamp01(
                flatness *
                Mathf.Lerp(0.45f, 1f, upperSurface) +
                surfaceBreakup);
        }

        private static float ResolveCreviceMask(
            Vector3 faceNormal,
            float vertical01,
            float exposure,
            float randomValue)
        {
            // Broad area mask only. Patch 12C.2 deliberately hard-clamps this
            // to the base/lower sheltered portion of the mass; the previous
            // versions left too much baseline value and debugged as a pale wash
            // across the whole rock. This channel is broad grounding/contact
            // darkness, not cracks.
            float upperKill = Mathf.Pow(
                1f - Mathf.SmoothStep(0.22f, 0.50f, vertical01),
                1.85f);

            float baseContact = Mathf.Pow(
                1f - Mathf.SmoothStep(0.000f, 0.125f, vertical01),
                2.40f);

            float sideAmount = Mathf.SmoothStep(
                0.30f,
                0.88f,
                1f - Mathf.Abs(faceNormal.y));

            float lowerSide = sideAmount * Mathf.Pow(
                1f - Mathf.SmoothStep(0.045f, 0.285f, vertical01),
                2.20f);

            float shelteredSurface =
                Mathf.Clamp01(1f - exposure) *
                sideAmount *
                Mathf.Pow(
                    1f - Mathf.SmoothStep(0.030f, 0.265f, vertical01),
                    2.55f);

            float underside = Mathf.Clamp01(-faceNormal.y) * Mathf.Pow(
                1f - Mathf.SmoothStep(0.015f, 0.220f, vertical01),
                2.10f);

            float surfaceBreakup = (randomValue - 0.5f) * 0.006f;

            float mask =
                baseContact * 0.92f +
                lowerSide * 0.24f +
                shelteredSurface * 0.16f +
                underside * 0.30f +
                surfaceBreakup;

            return Mathf.Clamp01(mask * upperKill);
        }

        private static float ResolveDirtDepositMask(
            Vector3 position,
            float vertical01,
            float exposure,
            float crevice,
            MassRecipe recipe,
            float authoredDepositBoost)
        {
            // Environmental area mask. Patch 12C.2 makes this primarily a
            // lower-rim / lower-side buildup mask. It may crawl upward in
            // broad irregular patches, but exposed upper and mid-body faces
            // should remain mostly neutral in debug.
            float upperKill = Mathf.Pow(
                1f - Mathf.SmoothStep(0.245f, 0.470f, vertical01),
                1.65f);

            float baseBand = Mathf.Pow(
                1f - Mathf.SmoothStep(0.000f, 0.185f, vertical01),
                1.95f);

            float crawlWindow =
                Mathf.SmoothStep(0.030f, 0.125f, vertical01) *
                (1f - Mathf.SmoothStep(0.170f, 0.355f, vertical01));

            float sideShelter = Mathf.Clamp01(1f - exposure);

            float broadNoiseA = HashPosition01(
                unchecked(recipe.SurfaceSeed ^ 0x51A7E),
                new Vector3(
                    position.x * 0.58f,
                    position.y * 0.24f,
                    position.z * 0.58f));

            float broadNoiseB = HashPosition01(
                unchecked(recipe.SurfaceSeed ^ 0x6D3B1),
                new Vector3(
                    position.x * 1.05f + 2.17f,
                    position.y * 0.42f - 1.31f,
                    position.z * 1.05f + 0.43f));

            float broadPatch = Mathf.SmoothStep(
                0.46f,
                0.86f,
                broadNoiseA * 0.64f + broadNoiseB * 0.36f);

            float fineBreakup = Mathf.Lerp(
                0.55f,
                1.08f,
                HashPosition01(
                    unchecked(recipe.SurfaceSeed ^ 0xD147),
                    position * 2.05f));

            float baseDeposit = baseBand * Mathf.Lerp(0.48f, 1.08f, broadPatch);
            float upwardPatch = crawlWindow * Mathf.Pow(sideShelter, 1.25f) * broadPatch;
            float creviceCatch = crevice * sideShelter * Mathf.Lerp(0.18f, 0.48f, broadPatch);

            float depositCore =
                baseDeposit * 0.86f +
                upwardPatch * 0.64f +
                creviceCatch +
                authoredDepositBoost * 0.0f;

            return Mathf.Clamp01(depositCore * fineBreakup * upperKill);
        }

        #endregion
    }
}
