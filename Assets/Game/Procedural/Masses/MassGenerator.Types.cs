using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Core mass-generator types

        private enum MacroProfile
        {
            Block,
            Wedge,
            Shoulder,
            Ridge,
            Crown
        }

        private enum CutRegion
        {
            Top,
            Side,
            Any
        }


        private enum MassSurfaceFeatureType
        {
            None,
            ConvexBoundary,
            ConcaveBoundary,
            CornerChipCap,
            Fracture,
            ImpactDent,
            MaterialSeam
        }

        private enum MassSurfaceFeatureResponseRole
        {
            None,
            StructuralTransition,
            MajorDamage,
            ConvexConcaveStructure,
            SurfaceAccumulation
        }

        private readonly struct MassSurfaceFeatureContribution
        {
            public static readonly MassSurfaceFeatureContribution None =
                new MassSurfaceFeatureContribution(
                    MassSurfaceFeatureType.None,
                    MassSurfaceFeatureResponseRole.None,
                    -1,
                    0f,
                    Vector3.zero,
                    0);

            public readonly MassSurfaceFeatureType Type;
            public readonly MassSurfaceFeatureResponseRole Role;
            public readonly int FeatureId;
            public readonly float Strength;
            public readonly Vector3 Direction;
            public readonly int Priority;

            public bool IsValid => Type != MassSurfaceFeatureType.None;

            public MassSurfaceFeatureContribution(
                MassSurfaceFeatureType type,
                MassSurfaceFeatureResponseRole role,
                int featureId,
                float strength,
                Vector3 direction,
                int priority)
            {
                Type = type;
                Role = role;
                FeatureId = featureId;
                Strength = Mathf.Clamp01(strength);
                Direction = TryNormalizeMassVector(direction, out Vector3 normalized)
                    ? normalized
                    : Vector3.zero;
                Priority = priority;
            }
        }

        private enum PolygonFaceFeature
        {
            Base,
            ConvexEdgeWear
        }

        private enum PolygonFaceProvenanceKind
        {
            None,
            SourceFace,
            EdgeBevelPlane,
            VertexJunctionPlane,
            BoundedEdgeBevel,
            BoundedEndpointCap,
            CornerDamageCap
        }

        private sealed class TriangleSoup
        {
            public readonly List<Vector3> Positions = new List<Vector3>();
            private readonly List<PolygonFaceFeature> features =
                new List<PolygonFaceFeature>();
            private readonly List<float> featureStrengths = new List<float>();
            private readonly List<Vector3> authoredSurfaceNormals =
                new List<Vector3>();
            private readonly List<int> authoredSurfaceGroups =
                new List<int>();
            private readonly List<PolygonFaceProvenanceKind> provenanceKinds =
                new List<PolygonFaceProvenanceKind>();
            private readonly List<int> provenanceIndices =
                new List<int>();
            private readonly List<MassSurfaceFeatureContribution>
                primarySurfaceContributions =
                    new List<MassSurfaceFeatureContribution>();
            private readonly List<MassSurfaceFeatureContribution>
                secondarySurfaceContributions =
                    new List<MassSurfaceFeatureContribution>();

            public void AddTriangle(
                Vector3 a,
                Vector3 b,
                Vector3 c,
                PolygonFaceFeature feature,
                float featureStrength)
            {
                AddTriangle(
                    a,
                    b,
                    c,
                    feature,
                    featureStrength,
                    Vector3.zero,
                    -1);
            }

            public void AddTriangle(
                Vector3 a,
                Vector3 b,
                Vector3 c,
                PolygonFaceFeature feature,
                float featureStrength,
                Vector3 authoredSurfaceNormal)
            {
                AddTriangle(
                    a,
                    b,
                    c,
                    feature,
                    featureStrength,
                    authoredSurfaceNormal,
                    -1);
            }

            public void AddTriangle(
                Vector3 a,
                Vector3 b,
                Vector3 c,
                PolygonFaceFeature feature,
                float featureStrength,
                Vector3 authoredSurfaceNormal,
                int authoredSurfaceGroup,
                PolygonFaceProvenanceKind provenanceKind =
                    PolygonFaceProvenanceKind.None,
                int provenanceIndex = -1)
            {
                Positions.Add(a);
                Positions.Add(b);
                Positions.Add(c);

                feature = feature == PolygonFaceFeature.ConvexEdgeWear
                    ? PolygonFaceFeature.ConvexEdgeWear
                    : PolygonFaceFeature.Base;
                featureStrength = Mathf.Clamp01(featureStrength);

                features.Add(feature);
                features.Add(feature);
                features.Add(feature);

                featureStrengths.Add(featureStrength);
                featureStrengths.Add(featureStrength);
                featureStrengths.Add(featureStrength);

                TryNormalizeMassVector(
                    authoredSurfaceNormal,
                    out Vector3 storedNormal);
                authoredSurfaceNormals.Add(storedNormal);
                authoredSurfaceNormals.Add(storedNormal);
                authoredSurfaceNormals.Add(storedNormal);

                authoredSurfaceGroups.Add(authoredSurfaceGroup);
                authoredSurfaceGroups.Add(authoredSurfaceGroup);
                authoredSurfaceGroups.Add(authoredSurfaceGroup);
                provenanceKinds.Add(provenanceKind);
                provenanceKinds.Add(provenanceKind);
                provenanceKinds.Add(provenanceKind);
                provenanceIndices.Add(provenanceIndex);
                provenanceIndices.Add(provenanceIndex);
                provenanceIndices.Add(provenanceIndex);

                ResolveSurfaceFeatureContributions(
                    feature,
                    featureStrength,
                    authoredSurfaceNormal,
                    provenanceKind,
                    provenanceIndex,
                    out MassSurfaceFeatureContribution primary,
                    out MassSurfaceFeatureContribution secondary);
                primarySurfaceContributions.Add(primary);
                primarySurfaceContributions.Add(primary);
                primarySurfaceContributions.Add(primary);
                secondarySurfaceContributions.Add(secondary);
                secondarySurfaceContributions.Add(secondary);
                secondarySurfaceContributions.Add(secondary);
            }

            public void AddTriangleWithSurfaceContributions(
                Vector3 a,
                Vector3 b,
                Vector3 c,
                PolygonFaceFeature feature,
                float featureStrength,
                Vector3 authoredSurfaceNormal,
                int authoredSurfaceGroup,
                MassSurfaceFeatureContribution primary,
                MassSurfaceFeatureContribution secondary,
                PolygonFaceProvenanceKind provenanceKind =
                    PolygonFaceProvenanceKind.None,
                int provenanceIndex = -1)
            {
                AddTriangle(
                    a,
                    b,
                    c,
                    feature,
                    featureStrength,
                    authoredSurfaceNormal,
                    authoredSurfaceGroup,
                    provenanceKind,
                    provenanceIndex);

                int lastIndex = primarySurfaceContributions.Count - 1;
                for (int offset = 0; offset < 3; offset++)
                {
                    primarySurfaceContributions[lastIndex - offset] = primary;
                    secondarySurfaceContributions[lastIndex - offset] = secondary;
                }
            }

            public MassSurfaceFeatureContribution ResolvePrimarySurfaceContribution(
                int vertexIndex)
            {
                return vertexIndex >= 0 &&
                    vertexIndex < primarySurfaceContributions.Count
                        ? primarySurfaceContributions[vertexIndex]
                        : MassSurfaceFeatureContribution.None;
            }

            public MassSurfaceFeatureContribution ResolveSecondarySurfaceContribution(
                int vertexIndex)
            {
                return vertexIndex >= 0 &&
                    vertexIndex < secondarySurfaceContributions.Count
                        ? secondarySurfaceContributions[vertexIndex]
                        : MassSurfaceFeatureContribution.None;
            }

            public PolygonFaceFeature ResolveFeature(int vertexIndex)
            {
                return vertexIndex >= 0 && vertexIndex < features.Count
                    ? features[vertexIndex]
                    : PolygonFaceFeature.Base;
            }

            public float ResolveFeatureStrength(int vertexIndex)
            {
                return vertexIndex >= 0 && vertexIndex < featureStrengths.Count
                    ? featureStrengths[vertexIndex]
                    : 0f;
            }

            public bool TryResolveAuthoredSurfaceGroup(
                int vertexIndex,
                out int surfaceGroup)
            {
                surfaceGroup = -1;
                if (vertexIndex < 0 ||
                    vertexIndex >= authoredSurfaceGroups.Count)
                {
                    return false;
                }

                surfaceGroup = authoredSurfaceGroups[vertexIndex];
                return surfaceGroup >= 0;
            }


            public bool TryResolveProvenance(
                int vertexIndex,
                out PolygonFaceProvenanceKind kind,
                out int index)
            {
                kind = PolygonFaceProvenanceKind.None;
                index = -1;
                if (vertexIndex < 0 || vertexIndex >= provenanceKinds.Count)
                {
                    return false;
                }
                kind = provenanceKinds[vertexIndex];
                index = provenanceIndices[vertexIndex];
                return kind != PolygonFaceProvenanceKind.None || index >= 0;
            }

            public bool TryResolveAuthoredSurfaceNormal(
                int vertexIndex,
                out Vector3 normal)
            {
                normal = Vector3.zero;
                if (vertexIndex < 0 ||
                    vertexIndex >= authoredSurfaceNormals.Count)
                {
                    return false;
                }

                normal = authoredSurfaceNormals[vertexIndex];
                return normal.sqrMagnitude > MinimumEdgeLengthSqr;
            }
        }

        private static void ResolveSurfaceFeatureContributions(
            PolygonFaceFeature feature,
            float featureStrength,
            Vector3 authoredSurfaceNormal,
            PolygonFaceProvenanceKind provenanceKind,
            int provenanceIndex,
            out MassSurfaceFeatureContribution primary,
            out MassSurfaceFeatureContribution secondary)
        {
            primary = MassSurfaceFeatureContribution.None;
            secondary = MassSurfaceFeatureContribution.None;

            MassSurfaceFeatureContribution convex =
                MassSurfaceFeatureContribution.None;
            MassSurfaceFeatureContribution corner =
                MassSurfaceFeatureContribution.None;

            bool isConvex =
                feature == PolygonFaceFeature.ConvexEdgeWear ||
                provenanceKind == PolygonFaceProvenanceKind.EdgeBevelPlane ||
                provenanceKind == PolygonFaceProvenanceKind.BoundedEdgeBevel ||
                provenanceKind == PolygonFaceProvenanceKind.VertexJunctionPlane;
            if (isConvex)
            {
                convex = new MassSurfaceFeatureContribution(
                    MassSurfaceFeatureType.ConvexBoundary,
                    MassSurfaceFeatureResponseRole.ConvexConcaveStructure,
                    provenanceIndex,
                    featureStrength > 0f ? featureStrength : 1f,
                    authoredSurfaceNormal,
                    200);
            }

            if (provenanceKind == PolygonFaceProvenanceKind.CornerDamageCap)
            {
                corner = new MassSurfaceFeatureContribution(
                    MassSurfaceFeatureType.CornerChipCap,
                    MassSurfaceFeatureResponseRole.MajorDamage,
                    provenanceIndex,
                    featureStrength > 0f ? featureStrength : 1f,
                    authoredSurfaceNormal,
                    300);
            }

            AddResolvedSurfaceContribution(
                convex,
                ref primary,
                ref secondary);
            AddResolvedSurfaceContribution(
                corner,
                ref primary,
                ref secondary);
        }

        private static void AddResolvedSurfaceContribution(
            MassSurfaceFeatureContribution candidate,
            ref MassSurfaceFeatureContribution primary,
            ref MassSurfaceFeatureContribution secondary)
        {
            if (!candidate.IsValid)
            {
                return;
            }

            if (!primary.IsValid ||
                candidate.Priority > primary.Priority ||
                (candidate.Priority == primary.Priority &&
                 CompareSurfaceContributionIdentity(candidate, primary) < 0))
            {
                secondary = primary;
                primary = candidate;
                return;
            }

            if (!secondary.IsValid ||
                candidate.Priority > secondary.Priority ||
                (candidate.Priority == secondary.Priority &&
                 CompareSurfaceContributionIdentity(candidate, secondary) < 0))
            {
                secondary = candidate;
            }
        }

        private static int CompareSurfaceContributionIdentity(
            MassSurfaceFeatureContribution left,
            MassSurfaceFeatureContribution right)
        {
            int typeCompare = ((int)left.Type).CompareTo((int)right.Type);
            return typeCompare != 0
                ? typeCompare
                : left.FeatureId.CompareTo(right.FeatureId);
        }

        private static bool IsFiniteMassValue(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFiniteMassVector(Vector3 value)
        {
            return IsFiniteMassValue(value.x) &&
                IsFiniteMassValue(value.y) &&
                IsFiniteMassValue(value.z);
        }

        private static bool TryNormalizeMassVector(
            Vector3 value,
            out Vector3 normalized)
        {
            normalized = Vector3.zero;
            if (!IsFiniteMassVector(value))
            {
                return false;
            }

            double x = value.x;
            double y = value.y;
            double z = value.z;
            double magnitudeSqr = x * x + y * y + z * z;
            if (!(magnitudeSqr > 0.0) ||
                double.IsNaN(magnitudeSqr) ||
                double.IsInfinity(magnitudeSqr))
            {
                return false;
            }

            double inverseMagnitude = 1.0 / Math.Sqrt(magnitudeSqr);
            normalized = new Vector3(
                (float)(x * inverseMagnitude),
                (float)(y * inverseMagnitude),
                (float)(z * inverseMagnitude));
            float normalizedMagnitudeSqr = normalized.sqrMagnitude;
            if (!IsFiniteMassVector(normalized) ||
                !IsFiniteMassValue(normalizedMagnitudeSqr) ||
                normalizedMagnitudeSqr < 0.999f ||
                normalizedMagnitudeSqr > 1.001f)
            {
                normalized = Vector3.zero;
                return false;
            }

            return true;
        }

        private readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            private readonly VertexKey first;
            private readonly VertexKey second;

            public EdgeKey(
                Vector3 start,
                Vector3 end)
            {
                VertexKey startKey = new VertexKey(start);
                VertexKey endKey = new VertexKey(end);

                if (startKey.CompareTo(endKey) <= 0)
                {
                    first = startKey;
                    second = endKey;
                }
                else
                {
                    first = endKey;
                    second = startKey;
                }
            }

            public bool Equals(EdgeKey other)
            {
                return first.Equals(other.first) &&
                    second.Equals(other.second);
            }

            public override bool Equals(object obj)
            {
                return obj is EdgeKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (first.GetHashCode() * 397) ^ second.GetHashCode();
                }
            }
        }

        private readonly struct RectRing
        {
            public readonly float Y;
            public readonly Vector2 Centre;
            public readonly float HalfWidth;
            public readonly float HalfDepth;

            public RectRing(
                float y,
                Vector2 centre,
                float halfWidth,
                float halfDepth)
            {
                Y = y;
                Centre = centre;
                HalfWidth = halfWidth;
                HalfDepth = halfDepth;
            }
        }

        private sealed class PolygonFace
        {
            public readonly List<Vector3> Vertices;
            public readonly Vector3 Normal;
            public readonly PolygonFaceFeature Feature;
            public readonly float FeatureStrength;
            public readonly PolygonFaceProvenanceKind ProvenanceKind;
            public readonly int ProvenanceIndex;

            public PolygonFace(
                List<Vector3> vertices,
                Vector3 normal,
                PolygonFaceFeature feature = PolygonFaceFeature.Base,
                float featureStrength = 0f,
                PolygonFaceProvenanceKind provenanceKind =
                    PolygonFaceProvenanceKind.None,
                int provenanceIndex = -1)
            {
                Vertices = vertices;
                Normal = normal.normalized;
                Feature = feature == PolygonFaceFeature.ConvexEdgeWear
                    ? PolygonFaceFeature.ConvexEdgeWear
                    : PolygonFaceFeature.Base;
                FeatureStrength = Mathf.Clamp01(featureStrength);
                ProvenanceKind = provenanceKind;
                ProvenanceIndex = provenanceIndex;
            }
        }

        private readonly struct CutPlane
        {
            public readonly Vector3 Normal;
            public readonly float Distance;

            public CutPlane(Vector3 normal, float distance)
            {
                Normal = normal.normalized;
                Distance = distance;
            }

            public float SignedDistance(Vector3 point)
            {
                return Vector3.Dot(Normal, point) - Distance;
            }
        }

        private readonly struct BoxExtents
        {
            public readonly float PositiveX;
            public readonly float NegativeX;
            public readonly float PositiveY;
            public readonly float NegativeY;
            public readonly float PositiveZ;
            public readonly float NegativeZ;

            public BoxExtents(
                float positiveX,
                float negativeX,
                float positiveY,
                float negativeY,
                float positiveZ,
                float negativeZ)
            {
                PositiveX = positiveX;
                NegativeX = negativeX;
                PositiveY = positiveY;
                NegativeY = negativeY;
                PositiveZ = positiveZ;
                NegativeZ = negativeZ;
            }
        }

        private sealed class Topology
        {
            public readonly List<Vector3> Directions;
            public readonly List<int> Triangles;
            public readonly List<int>[] Neighbours;

            public Topology(
                List<Vector3> directions,
                List<int> triangles,
                List<int>[] neighbours)
            {
                Directions = directions;
                Triangles = triangles;
                Neighbours = neighbours;
            }
        }

        private readonly struct DeformationLobe
        {
            public readonly Vector3 Direction;
            public readonly float Strength;
            public readonly float FalloffStart;
            public readonly float Power;

            public DeformationLobe(
                Vector3 direction,
                float strength,
                float falloffStart,
                float power)
            {
                Direction = direction;
                Strength = strength;
                FalloffStart = falloffStart;
                Power = power;
            }
        }

        private readonly struct VertexKey : IEquatable<VertexKey>
        {
            private const float Quantization = 100000f;

            private readonly int x;
            private readonly int y;
            private readonly int z;

            public VertexKey(Vector3 position)
            {
                x = Mathf.RoundToInt(position.x * Quantization);
                y = Mathf.RoundToInt(position.y * Quantization);
                z = Mathf.RoundToInt(position.z * Quantization);
            }

            public int CompareTo(VertexKey other)
            {
                int xComparison = x.CompareTo(other.x);

                if (xComparison != 0)
                {
                    return xComparison;
                }

                int yComparison = y.CompareTo(other.y);

                if (yComparison != 0)
                {
                    return yComparison;
                }

                return z.CompareTo(other.z);
            }

            public bool Equals(VertexKey other)
            {
                return x == other.x && y == other.y && z == other.z;
            }

            public override bool Equals(object obj)
            {
                return obj is VertexKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = x;
                    hash = hash * 397 ^ y;
                    hash = hash * 397 ^ z;
                    return hash;
                }
            }
        }

        #endregion
    }
}
