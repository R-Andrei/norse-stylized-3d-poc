using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Shared helpers and settings

        private static void AddOutwardTriangle(
            TriangleSoup soup,
            Vector3 a,
            Vector3 b,
            Vector3 c)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            Vector3 faceCentre = (a + b + c) / 3f;

            if (Vector3.Dot(normal, faceCentre) < 0f)
            {
                Vector3 temporary = b;
                b = c;
                c = temporary;
            }

            soup.AddTriangle(
                a,
                b,
                c,
                PolygonFaceFeature.Base,
                0f);
        }

        private static void AddRectRingCap(
            TriangleSoup soup,
            RectRing ring,
            Vector3 solidCentre)
        {
            Vector3[] corners = ResolveRectRingCorners(ring);

            AddQuadAroundCenter(
                soup,
                corners[0],
                corners[1],
                corners[2],
                corners[3],
                solidCentre);
        }

        private static void AddRectRingBridge(
            TriangleSoup soup,
            RectRing lower,
            RectRing upper,
            Vector3 solidCentre)
        {
            Vector3[] lowerCorners = ResolveRectRingCorners(lower);
            Vector3[] upperCorners = ResolveRectRingCorners(upper);
            bool isShelf =
                Mathf.Abs(lower.Y - upper.Y) <= PointMergeDistance;

            for (int i = 0; i < lowerCorners.Length; i++)
            {
                int next = (i + 1) % lowerCorners.Length;

                if (isShelf)
                {
                    AddOrientedTriangle(
                        soup,
                        lowerCorners[i],
                        lowerCorners[next],
                        upperCorners[next],
                        Vector3.up);

                    AddOrientedTriangle(
                        soup,
                        lowerCorners[i],
                        upperCorners[next],
                        upperCorners[i],
                        Vector3.up);

                    continue;
                }

                AddQuadAroundCenter(
                    soup,
                    lowerCorners[i],
                    lowerCorners[next],
                    upperCorners[next],
                    upperCorners[i],
                    solidCentre);
            }
        }

        private static Vector3[] ResolveRectRingCorners(RectRing ring)
        {
            return new[]
            {
                new Vector3(
                    ring.Centre.x - ring.HalfWidth,
                    ring.Y,
                    ring.Centre.y - ring.HalfDepth),
                new Vector3(
                    ring.Centre.x + ring.HalfWidth,
                    ring.Y,
                    ring.Centre.y - ring.HalfDepth),
                new Vector3(
                    ring.Centre.x + ring.HalfWidth,
                    ring.Y,
                    ring.Centre.y + ring.HalfDepth),
                new Vector3(
                    ring.Centre.x - ring.HalfWidth,
                    ring.Y,
                    ring.Centre.y + ring.HalfDepth)
            };
        }

        private static void AddExtrudedPolygon(
            TriangleSoup soup,
            Vector2[] points,
            float minimumZ,
            float maximumZ)
        {
            if (points == null || points.Length < 3)
            {
                return;
            }

            Vector2 centre2D = Vector2.zero;
            for (int i = 0; i < points.Length; i++)
            {
                centre2D += points[i];
            }

            centre2D /= points.Length;

            Vector3 solidCentre = new Vector3(
                centre2D.x,
                centre2D.y,
                (minimumZ + maximumZ) * 0.5f);

            Vector3 frontCentre = new Vector3(
                centre2D.x,
                centre2D.y,
                maximumZ);

            Vector3 backCentre = new Vector3(
                centre2D.x,
                centre2D.y,
                minimumZ);

            for (int i = 0; i < points.Length; i++)
            {
                int next = (i + 1) % points.Length;

                Vector3 frontA = new Vector3(
                    points[i].x,
                    points[i].y,
                    maximumZ);

                Vector3 frontB = new Vector3(
                    points[next].x,
                    points[next].y,
                    maximumZ);

                Vector3 backA = new Vector3(
                    points[i].x,
                    points[i].y,
                    minimumZ);

                Vector3 backB = new Vector3(
                    points[next].x,
                    points[next].y,
                    minimumZ);

                AddTriangleAroundCenter(
                    soup,
                    frontCentre,
                    frontA,
                    frontB,
                    solidCentre);

                AddTriangleAroundCenter(
                    soup,
                    backCentre,
                    backB,
                    backA,
                    solidCentre);

                AddQuadAroundCenter(
                    soup,
                    backA,
                    backB,
                    frontB,
                    frontA,
                    solidCentre);
            }
        }

        private static void AddQuadAroundCenter(
            TriangleSoup soup,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            Vector3 solidCentre)
        {
            AddTriangleAroundCenter(soup, a, b, c, solidCentre);
            AddTriangleAroundCenter(soup, a, c, d, solidCentre);
        }

        private static void AddTriangleAroundCenter(
            TriangleSoup soup,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 solidCentre)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            Vector3 faceCentre = (a + b + c) / 3f;

            if (Vector3.Dot(normal, faceCentre - solidCentre) < 0f)
            {
                Vector3 temporary = b;
                b = c;
                c = temporary;
            }

            AddOrientedTriangle(
                soup,
                a,
                b,
                c,
                Vector3.Cross(b - a, c - a));
        }

        private static void AddOrientedTriangle(
            TriangleSoup soup,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 expectedNormal,
            PolygonFaceFeature feature = PolygonFaceFeature.Base,
            float featureStrength = 0f)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 bc = c - b;

            float maximumEdgeLengthSqr = Mathf.Max(
                ab.sqrMagnitude,
                Mathf.Max(ac.sqrMagnitude, bc.sqrMagnitude));

            if (maximumEdgeLengthSqr <= MinimumEdgeLengthSqr)
            {
                return;
            }

            Vector3 normal = Vector3.Cross(ab, ac);

            // Cross-product squared has length^4 units. Compare it against an
            // edge-length^4 reference, not against the linear plane epsilon.
            // This preserves small but valid triangles on heavily cut forms.
            float relativeAreaThreshold =
                maximumEdgeLengthSqr *
                maximumEdgeLengthSqr *
                RelativeTriangleAreaEpsilon;

            if (normal.sqrMagnitude <= relativeAreaThreshold)
            {
                return;
            }

            if (Vector3.Dot(normal, expectedNormal) < 0f)
            {
                Vector3 temporary = b;
                b = c;
                c = temporary;
            }

            soup.AddTriangle(
                a,
                b,
                c,
                feature,
                featureStrength);
        }

        private static void AddPointIfDifferent(
            List<Vector3> points,
            Vector3 point)
        {
            if (points.Count == 0 ||
                (points[points.Count - 1] - point).sqrMagnitude > PointMergeDistanceSqr)
            {
                points.Add(point);
            }
        }

        private static void RemoveClosingDuplicate(List<Vector3> points)
        {
            if (points.Count < 2)
            {
                return;
            }

            if ((points[0] - points[points.Count - 1]).sqrMagnitude <=
                PointMergeDistanceSqr)
            {
                points.RemoveAt(points.Count - 1);
            }
        }

        private static List<Vector3> GetUniquePoints(List<Vector3> points)
        {
            List<Vector3> unique = new List<Vector3>();

            for (int i = 0; i < points.Count; i++)
            {
                bool duplicate = false;

                for (int j = 0; j < unique.Count; j++)
                {
                    if ((points[i] - unique[j]).sqrMagnitude <=
                        PointMergeDistanceSqr)
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                {
                    unique.Add(points[i]);
                }
            }

            return unique;
        }

        private static void WeldSharedVertices(
            List<PolygonFace> faces)
        {
            Dictionary<VertexKey, Vector3> canonical =
                new Dictionary<VertexKey, Vector3>();

            for (int faceIndex = 0;
                 faceIndex < faces.Count;
                 faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;

                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    VertexKey key = new VertexKey(vertices[vertexIndex]);

                    if (canonical.TryGetValue(
                            key,
                            out Vector3 snapped))
                    {
                        vertices[vertexIndex] = snapped;
                    }
                    else
                    {
                        canonical.Add(key, vertices[vertexIndex]);
                    }
                }
            }
        }

        private static void SanitizeAllFaces(
            List<PolygonFace> faces)
        {
            for (int i = faces.Count - 1; i >= 0; i--)
            {
                List<Vector3> cleaned = SanitizePolygon(
                    faces[i].Vertices,
                    faces[i].Normal);

                if (cleaned.Count < 3 ||
                    CalculatePolygonArea(cleaned) <= TinyFaceAreaEpsilon)
                {
                    faces.RemoveAt(i);
                    continue;
                }

                faces[i].Vertices.Clear();
                faces[i].Vertices.AddRange(cleaned);
            }
        }

        private static List<Vector3> SanitizePolygon(
            List<Vector3> vertices,
            Vector3 normal)
        {
            List<Vector3> result =
                new List<Vector3>(vertices);

            bool changed = true;
            int safetyPass = 0;

            while (changed &&
                   result.Count >= 3 &&
                   safetyPass < 12)
            {
                changed = false;
                safetyPass++;

                RemoveClosingDuplicate(result);

                for (int i = result.Count - 1;
                     i >= 0 && result.Count >= 3;
                     i--)
                {
                    int previousIndex =
                        (i - 1 + result.Count) % result.Count;

                    int nextIndex =
                        (i + 1) % result.Count;

                    Vector3 previous = result[previousIndex];
                    Vector3 current = result[i];
                    Vector3 next = result[nextIndex];

                    Vector3 previousEdge = current - previous;
                    Vector3 nextEdge = next - current;

                    float previousEdgeLengthSqr =
                        previousEdge.sqrMagnitude;

                    float nextEdgeLengthSqr =
                        nextEdge.sqrMagnitude;

                    if (previousEdgeLengthSqr <=
                            PointMergeDistanceSqr ||
                        nextEdgeLengthSqr <=
                            PointMergeDistanceSqr)
                    {
                        result.RemoveAt(i);
                        changed = true;
                        continue;
                    }

                    float maximumAdjacentEdgeLengthSqr = Mathf.Max(
                        previousEdgeLengthSqr,
                        nextEdgeLengthSqr);

                    float turnAreaSqr = Vector3.Cross(
                        previousEdge,
                        nextEdge).sqrMagnitude;

                    float relativeCollinearThreshold =
                        maximumAdjacentEdgeLengthSqr *
                        maximumAdjacentEdgeLengthSqr *
                        RelativeCollinearEpsilon;

                    if (turnAreaSqr <= relativeCollinearThreshold)
                    {
                        result.RemoveAt(i);
                        changed = true;
                    }
                }
            }

            RemoveClosingDuplicate(result);

            if (result.Count >= 3)
            {
                Vector3 calculatedNormal =
                    CalculatePolygonNormal(result);

                if (Vector3.Dot(calculatedNormal, normal) < 0f)
                {
                    result.Reverse();
                }
            }

            return result;
        }

        private static float CalculatePolygonArea(
            List<Vector3> vertices)
        {
            if (vertices.Count < 3)
            {
                return 0f;
            }

            Vector3 origin = vertices[0];
            float totalArea = 0f;

            for (int i = 1; i < vertices.Count - 1; i++)
            {
                Vector3 first = vertices[i] - origin;
                Vector3 second = vertices[i + 1] - origin;

                totalArea +=
                    Vector3.Cross(first, second).magnitude * 0.5f;
            }

            return totalArea;
        }

        private static Vector3 CalculateAverage(List<Vector3> values)
        {
            Vector3 total = Vector3.zero;

            for (int i = 0; i < values.Count; i++)
            {
                total += values[i];
            }

            return values.Count > 0
                ? total / values.Count
                : Vector3.zero;
        }

        private static float CalculateAverageRadius(
            List<Vector3> points,
            Vector3 centre)
        {
            float total = 0f;

            for (int i = 0; i < points.Count; i++)
            {
                total += Vector3.Distance(points[i], centre);
            }

            return points.Count > 0
                ? total / points.Count
                : 0f;
        }

        private static Bounds CalculateBounds(List<Vector3> positions)
        {
            Bounds bounds = new Bounds(positions[0], Vector3.zero);

            for (int i = 1; i < positions.Count; i++)
            {
                bounds.Encapsulate(positions[i]);
            }

            return bounds;
        }

        private static void GetVerticalRange(
            List<Vector3> positions,
            out float minimumY,
            out float maximumY)
        {
            minimumY = float.PositiveInfinity;
            maximumY = float.NegativeInfinity;

            for (int i = 0; i < positions.Count; i++)
            {
                minimumY = Mathf.Min(minimumY, positions[i].y);
                maximumY = Mathf.Max(maximumY, positions[i].y);
            }
        }

        private static Vector3 GetBaseDimensions(MassArchetype archetype)
        {
            return archetype switch
            {
                MassArchetype.TerrainBoulder => new Vector3(2f, 1.45f, 1.7f),
                MassArchetype.SquatBoulder => new Vector3(2.35f, 0.95f, 2f),
                MassArchetype.StandingStone => new Vector3(1.1f, 2.8f, 0.95f),
                MassArchetype.FlatSlab => new Vector3(2.4f, 0.62f, 1.7f),
                MassArchetype.BrokenChunk => new Vector3(1.75f, 1.55f, 1.45f),
                MassArchetype.PolishedStone => new Vector3(2f, 1.35f, 1.7f),
                MassArchetype.LayeredStone => new Vector3(2.25f, 0.86f, 1.75f),
                MassArchetype.CarvedMarkerStone => new Vector3(1.28f, 3.05f, 0.62f),
                MassArchetype.FracturedPillar => new Vector3(0.96f, 3.15f, 0.82f),
                _ => Vector3.one
            };
        }

        private static float GetSizeMultiplier(MassScaleStep size)
        {
            return size switch
            {
                MassScaleStep.XS => 0.25f,
                MassScaleStep.S => 0.45f,
                MassScaleStep.M => 0.70f,
                MassScaleStep.L => 1f,
                MassScaleStep.XL => 1.45f,
                MassScaleStep.XXL => 2.10f,
                MassScaleStep.Monumental => 3.20f,
                _ => 1f
            };
        }

        private static void GetMajorCutCountRange(
            FormComplexity complexity,
            out int minimum,
            out int maximum)
        {
            switch (complexity)
            {
                case FormComplexity.Primitive:
                    minimum = 2;
                    maximum = 3;
                    break;
                case FormComplexity.Simple:
                    minimum = 4;
                    maximum = 6;
                    break;
                case FormComplexity.Moderate:
                    minimum = 7;
                    maximum = 10;
                    break;
                case FormComplexity.Complex:
                    minimum = 11;
                    maximum = 15;
                    break;
                case FormComplexity.HighlyComplex:
                    minimum = 16;
                    maximum = 22;
                    break;
                default:
                    minimum = 7;
                    maximum = 10;
                    break;
            }
        }

        private static void GetCutDepthRange(
            ShapeDiversity diversity,
            out float minimum,
            out float maximum)
        {
            switch (diversity)
            {
                case ShapeDiversity.Restrained:
                    minimum = 0.07f;
                    maximum = 0.16f;
                    break;
                case ShapeDiversity.Broad:
                    minimum = 0.10f;
                    maximum = 0.29f;
                    break;
                case ShapeDiversity.Wild:
                    minimum = 0.13f;
                    maximum = 0.42f;
                    break;
                default:
                    minimum = 0.10f;
                    maximum = 0.29f;
                    break;
            }
        }

        private static float GetArchetypeCutDepthMultiplier(
            MassArchetype archetype)
        {
            return archetype switch
            {
                MassArchetype.TerrainBoulder => 1f,
                MassArchetype.SquatBoulder => 0.95f,
                MassArchetype.StandingStone => 0.82f,
                MassArchetype.FlatSlab => 0.74f,
                MassArchetype.BrokenChunk => 1.16f,
                MassArchetype.LayeredStone => 0.68f,
                MassArchetype.CarvedMarkerStone => 1.25f,
                MassArchetype.FracturedPillar => 1.34f,
                _ => 1f
            };
        }

        private static float GetEdgeCutDepthMultiplier(
            EdgeCharacter edgeCharacter)
        {
            return edgeCharacter switch
            {
                EdgeCharacter.Sharp => 1.02f,
                EdgeCharacter.Chipped => 1.08f,
                EdgeCharacter.Natural => 0.92f,
                EdgeCharacter.Worn => 0.76f,
                EdgeCharacter.Polished => 0.58f,
                _ => 0.92f
            };
        }

        private static int GetSecondaryChipCount(
            EdgeCharacter edgeCharacter,
            FormComplexity complexity)
        {
            int complexityIndex = (int)complexity;

            return edgeCharacter switch
            {
                EdgeCharacter.Chipped => 2 + complexityIndex,
                EdgeCharacter.Natural => complexityIndex >= 2 ? 1 : 0,
                _ => 0
            };
        }


        private static int GetBoundarySegments(SurfaceFacetDensity density)
        {
            return density switch
            {
                SurfaceFacetDensity.Sparse => 1,
                SurfaceFacetDensity.Low => 1,
                SurfaceFacetDensity.Medium => 2,
                SurfaceFacetDensity.High => 3,
                SurfaceFacetDensity.VeryHigh => 4,
                _ => 2
            };
        }

        private static float GetSurfaceRelief(SurfaceFacetDensity density)
        {
            return density switch
            {
                SurfaceFacetDensity.Sparse => 0f,
                SurfaceFacetDensity.Low => 0f,
                SurfaceFacetDensity.Medium => 0.018f,
                SurfaceFacetDensity.High => 0.028f,
                SurfaceFacetDensity.VeryHigh => 0.040f,
                _ => 0.018f
            };
        }

        private static float GetReliefMultiplier(EdgeCharacter edgeCharacter)
        {
            return edgeCharacter switch
            {
                EdgeCharacter.Sharp => 0.60f,
                EdgeCharacter.Chipped => 1.20f,
                EdgeCharacter.Natural => 1f,
                EdgeCharacter.Worn => 0.55f,
                EdgeCharacter.Polished => 0.20f,
                _ => 1f
            };
        }

        private static int GetSurfaceFrequency(SurfaceFacetDensity density)
        {
            return density switch
            {
                SurfaceFacetDensity.Sparse => 1,
                SurfaceFacetDensity.Low => 2,
                SurfaceFacetDensity.Medium => 3,
                SurfaceFacetDensity.High => 4,
                SurfaceFacetDensity.VeryHigh => 5,
                _ => 3
            };
        }

        private static void GetRadialRegularization(
            EdgeCharacter edgeCharacter,
            out int passes,
            out float strength,
            out float localDifference)
        {
            switch (edgeCharacter)
            {
                case EdgeCharacter.Sharp:
                    passes = 1;
                    strength = 0.18f;
                    localDifference = 0.16f;
                    break;
                case EdgeCharacter.Chipped:
                    passes = 1;
                    strength = 0.24f;
                    localDifference = 0.14f;
                    break;
                case EdgeCharacter.Natural:
                    passes = 2;
                    strength = 0.36f;
                    localDifference = 0.11f;
                    break;
                case EdgeCharacter.Worn:
                    passes = 3;
                    strength = 0.48f;
                    localDifference = 0.085f;
                    break;
                case EdgeCharacter.Polished:
                    passes = 4;
                    strength = 0.60f;
                    localDifference = 0.06f;
                    break;
                default:
                    passes = 2;
                    strength = 0.36f;
                    localDifference = 0.11f;
                    break;
            }
        }

        private static void GetGroundingSettings(
            GroundingStyle grounding,
            out float bandFraction,
            out float flatteningStrength,
            out float broadeningStrength)
        {
            switch (grounding)
            {
                case GroundingStyle.Light:
                    bandFraction = 0.08f;
                    flatteningStrength = 0.25f;
                    broadeningStrength = 0f;
                    break;
                case GroundingStyle.Stable:
                    bandFraction = 0.16f;
                    flatteningStrength = 0.58f;
                    broadeningStrength = 0.035f;
                    break;
                case GroundingStyle.Embedded:
                    bandFraction = 0.25f;
                    flatteningStrength = 0.78f;
                    broadeningStrength = 0.07f;
                    break;
                default:
                    bandFraction = 0.16f;
                    flatteningStrength = 0.58f;
                    broadeningStrength = 0.035f;
                    break;
            }
        }

        private static Quaternion CreateSamplingRotation(int surfaceSeed)
        {
            System.Random random = CreateRandom(surfaceSeed, 0x2C1B3C6D);

            return Quaternion.Euler(
                RandomRange(random, -18f, 18f),
                RandomRange(random, 0f, 360f),
                RandomRange(random, -18f, 18f));
        }

        private static Vector3 RandomHorizontalDirection(System.Random random)
        {
            float angle = RandomRange(random, 0f, Mathf.PI * 2f);
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }

        private static Vector3 RandomUnitVector(System.Random random)
        {
            float y = RandomRange(random, -1f, 1f);
            float angle = RandomRange(random, 0f, Mathf.PI * 2f);
            float horizontal = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));

            return new Vector3(
                horizontal * Mathf.Cos(angle),
                y,
                horizontal * Mathf.Sin(angle));
        }

        private static System.Random CreateRandom(int seed, int salt)
        {
            return new System.Random(unchecked(seed * 486187739 + salt));
        }

        private static float RandomRange(
            System.Random random,
            float minimum,
            float maximum)
        {
            return Mathf.Lerp(
                minimum,
                maximum,
                (float)random.NextDouble());
        }

        private static float Hash01(int seed, int index)
        {
            unchecked
            {
                uint value =
                    (uint)seed * 374761393u +
                    (uint)index * 668265263u;

                value = (value ^ (value >> 13)) * 1274126177u;
                value ^= value >> 16;

                return (value & 0x00FFFFFFu) / 16777215f;
            }
        }

        private static float HashSigned(int seed, int index)
        {
            return Hash01(seed, index) * 2f - 1f;
        }

        private static float HashPosition01(int seed, Vector3 position)
        {
            unchecked
            {
                int x = Mathf.RoundToInt(position.x * 127.0f);
                int y = Mathf.RoundToInt(position.y * 127.0f);
                int z = Mathf.RoundToInt(position.z * 127.0f);
                int index = x * 73856093 ^ y * 19349663 ^ z * 83492791;
                return Hash01(seed, index);
            }
        }

        #endregion
    }
}
