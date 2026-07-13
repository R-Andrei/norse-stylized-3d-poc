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
            BoundedEndpointCap
        }

        private sealed class TriangleSoup
        {
            public readonly List<Vector3> Positions = new List<Vector3>();
            private readonly List<PolygonFaceFeature> features =
                new List<PolygonFaceFeature>();
            private readonly List<float> featureStrengths = new List<float>();

            public void AddTriangle(
                Vector3 a,
                Vector3 b,
                Vector3 c,
                PolygonFaceFeature feature,
                float featureStrength)
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
