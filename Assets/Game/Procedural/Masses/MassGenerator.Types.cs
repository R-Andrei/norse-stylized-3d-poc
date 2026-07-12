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

        private sealed class FaceMaterialMaskLookup
        {
            private readonly Dictionary<FaceVertexKey, FaceVertexMaterialMask>
                masks;

            private FaceMaterialMaskLookup(
                Dictionary<FaceVertexKey, FaceVertexMaterialMask> masks)
            {
                this.masks = masks;
            }

            public static FaceMaterialMaskLookup Build(
                TriangleSoup soup,
                Vector3 centre,
                Bounds bounds,
                MassRecipe recipe)
            {
                Dictionary<int, FaceMaskRecord> faces =
                    new Dictionary<int, FaceMaskRecord>(soup.Positions.Count / 3);
                Dictionary<EdgeKey, EdgeMaterialAggregate> edges =
                    new Dictionary<EdgeKey, EdgeMaterialAggregate>();

                for (int i = 0; i < soup.Positions.Count; i += 3)
                {
                    int faceIndex = i / 3;
                    Vector3 a = soup.Positions[i];
                    Vector3 b = soup.Positions[i + 1];
                    Vector3 c = soup.Positions[i + 2];
                    Vector3 normal = Vector3.Cross(b - a, c - a);
                    Vector3 faceCentre = (a + b + c) / 3f;

                    if (Vector3.Dot(normal, faceCentre - centre) < 0f)
                    {
                        Vector3 temporary = b;
                        b = c;
                        c = temporary;
                        normal = -normal;
                    }

                    if (normal.sqrMagnitude <= MinimumEdgeLengthSqr)
                    {
                        continue;
                    }

                    FaceMaskRecord face = new FaceMaskRecord(
                        faceIndex,
                        a,
                        b,
                        c,
                        normal.normalized,
                        faceCentre);
                    faces.Add(faceIndex, face);

                    AddEdge(edges, faceIndex, a, b);
                    AddEdge(edges, faceIndex, b, c);
                    AddEdge(edges, faceIndex, c, a);
                }

                Dictionary<FaceVertexKey, FaceVertexMaterialMask> masks =
                    new Dictionary<FaceVertexKey, FaceVertexMaterialMask>();
                float maximumDimension = Mathf.Max(
                    Mathf.Max(bounds.size.x, bounds.size.y),
                    bounds.size.z);
                maximumDimension = Mathf.Max(0.001f, maximumDimension);

                List<EdgeMaterialCandidate> candidates =
                    new List<EdgeMaterialCandidate>(edges.Count);

                foreach (EdgeMaterialAggregate edge in edges.Values)
                {
                    EdgeMaterialCandidate candidate = ResolveEdgeMaterialCandidate(
                        edge,
                        faces,
                        centre,
                        bounds,
                        maximumDimension,
                        recipe);

                    if (candidate.HasAnyMask)
                    {
                        candidates.Add(candidate);
                    }
                }

                ApplySelectedFeatureMasks(
                    candidates,
                    masks,
                    recipe);

                return new FaceMaterialMaskLookup(masks);
            }

            public float ResolveConvexEdgeWear(
                int faceIndex,
                Vector3 position)
            {
                return ResolveFaceVertexMask(faceIndex, position).ConvexEdgeWear;
            }

            public float ResolveConcaveCrease(
                int faceIndex,
                Vector3 position)
            {
                return ResolveFaceVertexMask(faceIndex, position).ConcaveCrease;
            }

            public float ResolveDirtDepositBoost(
                int faceIndex,
                Vector3 position)
            {
                return ResolveFaceVertexMask(faceIndex, position).DirtDepositBoost;
            }

            public float ResolveConvexEdgeLine(
                int faceIndex,
                Vector3 position)
            {
                return ResolveFaceVertexMask(faceIndex, position).ConvexEdgeLine;
            }

            public float ResolveConcaveCreaseLine(
                int faceIndex,
                Vector3 position)
            {
                return ResolveFaceVertexMask(faceIndex, position).ConcaveCreaseLine;
            }

            private FaceVertexMaterialMask ResolveFaceVertexMask(
                int faceIndex,
                Vector3 position)
            {
                FaceVertexKey key = new FaceVertexKey(
                    faceIndex,
                    position);

                if (masks.TryGetValue(
                        key,
                        out FaceVertexMaterialMask mask))
                {
                    return mask;
                }

                return default;
            }

            private static void AddEdge(
                Dictionary<EdgeKey, EdgeMaterialAggregate> edges,
                int faceIndex,
                Vector3 start,
                Vector3 end)
            {
                EdgeKey key = new EdgeKey(start, end);

                if (!edges.TryGetValue(
                        key,
                        out EdgeMaterialAggregate edge))
                {
                    edge = new EdgeMaterialAggregate(key, start, end);
                    edges.Add(key, edge);
                }

                edge.AddFace(faceIndex);
            }

            private static EdgeMaterialCandidate ResolveEdgeMaterialCandidate(
                EdgeMaterialAggregate edge,
                Dictionary<int, FaceMaskRecord> faces,
                Vector3 centre,
                Bounds bounds,
                float maximumDimension,
                MassRecipe recipe)
            {
                float edgeLength = (edge.End - edge.Start).magnitude;
                float edgeLength01 = edgeLength / maximumDimension;
                float readableLength = Mathf.SmoothStep(
                    0.09f,
                    0.28f,
                    edgeLength01);

                if (readableLength <= 0.001f)
                {
                    return default;
                }

                Vector3 midpoint = (edge.Start + edge.End) * 0.5f;
                Vector3 edgeDirection = (edge.End - edge.Start).normalized;
                float safeHeight = Mathf.Max(0.001f, bounds.size.y);
                float vertical01 = Mathf.Clamp01(
                    (midpoint.y - bounds.min.y) / safeHeight);
                float baseSuppression = Mathf.SmoothStep(
                    0.115f,
                    0.285f,
                    vertical01);
                float exposedHeight = Mathf.SmoothStep(
                    0.16f,
                    0.82f,
                    vertical01);
                float lowerDepositBand =
                    (1f - Mathf.SmoothStep(0.03f, 0.36f, vertical01)) *
                    Mathf.SmoothStep(0.005f, 0.10f, vertical01);

                float strongestAngleScore = 0f;
                float concaveTopologyScore = 0f;
                Vector3 averageNormal = Vector3.zero;

                for (int i = 0; i < edge.FaceIndices.Count; i++)
                {
                    if (faces.TryGetValue(
                            edge.FaceIndices[i],
                            out FaceMaskRecord face))
                    {
                        averageNormal += face.Normal;
                    }
                }

                if (averageNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    averageNormal = (midpoint - centre).sqrMagnitude > MinimumEdgeLengthSqr
                        ? (midpoint - centre).normalized
                        : Vector3.up;
                }
                else
                {
                    averageNormal.Normalize();
                }

                if (edge.FaceIndices.Count <= 1)
                {
                    strongestAngleScore = 0.64f;
                }
                else
                {
                    for (int i = 0; i < edge.FaceIndices.Count; i++)
                    {
                        FaceMaskRecord first = faces[edge.FaceIndices[i]];

                        for (int j = i + 1; j < edge.FaceIndices.Count; j++)
                        {
                            FaceMaskRecord second = faces[edge.FaceIndices[j]];
                            float normalDot = Mathf.Clamp(
                                Vector3.Dot(first.Normal, second.Normal),
                                -1f,
                                1f);
                            float angleAmount = 1f - normalDot;
                            float angleScore = Mathf.SmoothStep(
                                0.18f,
                                0.64f,
                                angleAmount);

                            strongestAngleScore = Mathf.Max(
                                strongestAngleScore,
                                angleScore);

                            Vector3 centreDelta = second.Centre - first.Centre;
                            if (centreDelta.sqrMagnitude > MinimumEdgeLengthSqr)
                            {
                                Vector3 direction = centreDelta.normalized;
                                float firstInset = Vector3.Dot(direction, first.Normal);
                                float secondInset = Vector3.Dot(-direction, second.Normal);
                                float concaveLike = Mathf.Clamp01(
                                    (firstInset + secondInset) * 0.5f);
                                concaveTopologyScore = Mathf.Max(
                                    concaveTopologyScore,
                                    angleScore * concaveLike);
                            }
                        }
                    }
                }

                if (strongestAngleScore <= 0.001f)
                {
                    return default;
                }

                int edgeHash = edge.Key.GetHashCode();
                float wearBreakup = Mathf.Lerp(
                    0.72f,
                    1.12f,
                    Hash01(
                        unchecked(recipe.SurfaceSeed ^ 0x37A1D5),
                        edgeHash));
                float outward = (midpoint - centre).sqrMagnitude > MinimumEdgeLengthSqr
                    ? Mathf.Clamp01(Vector3.Dot(averageNormal, (midpoint - centre).normalized) * 0.5f + 0.5f)
                    : 0.5f;
                float upwardSupport = Mathf.Clamp01(averageNormal.y * 0.45f + 0.55f);
                float convexScore =
                    strongestAngleScore *
                    readableLength *
                    baseSuppression *
                    Mathf.Lerp(0.76f, 1.22f, exposedHeight) *
                    Mathf.Lerp(0.74f, 1.08f, outward) *
                    Mathf.Lerp(0.88f, 1.12f, upwardSupport) *
                    wearBreakup;
                convexScore = Mathf.SmoothStep(0.12f, 0.58f, convexScore);

                float sideScore = 1f - Mathf.SmoothStep(
                    0.18f,
                    0.70f,
                    Mathf.Abs(averageNormal.y));
                float verticalOrDiagonal = Mathf.SmoothStep(
                    0.10f,
                    0.72f,
                    Mathf.Abs(edgeDirection.y));
                float midHeightBand =
                    Mathf.SmoothStep(0.12f, 0.30f, vertical01) *
                    (1f - Mathf.SmoothStep(0.84f, 0.98f, vertical01));
                float creaseRandom = Hash01(
                    unchecked(recipe.SurfaceSeed ^ 0x5EED5EA),
                    edgeHash);
                float selectedFracture = creaseRandom <= GetCreaseSelectionThreshold(recipe)
                    ? 1f
                    : 0f;
                float authoredFractureScore =
                    strongestAngleScore *
                    readableLength *
                    sideScore *
                    midHeightBand *
                    Mathf.Lerp(0.55f, 1.08f, verticalOrDiagonal) *
                    selectedFracture;
                float concaveScore = Mathf.Max(
                    concaveTopologyScore * readableLength * midHeightBand,
                    authoredFractureScore);
                concaveScore = Mathf.SmoothStep(0.10f, 0.48f, concaveScore);

                float dirtBreakup = Mathf.Lerp(
                    0.72f,
                    1.15f,
                    Hash01(
                        unchecked(recipe.SurfaceSeed ^ 0xD171),
                        edgeHash));
                float dirtDepositBoost = Mathf.Clamp01(
                    lowerDepositBand *
                    Mathf.Lerp(0.35f, 1.0f, sideScore) *
                    readableLength *
                    dirtBreakup *
                    (0.16f + concaveScore * 0.38f));

                return new EdgeMaterialCandidate(
                    edge,
                    convexScore,
                    concaveScore,
                    dirtDepositBoost);
            }

            private static void ApplySelectedFeatureMasks(
                List<EdgeMaterialCandidate> candidates,
                Dictionary<FaceVertexKey, FaceVertexMaterialMask> masks,
                MassRecipe recipe)
            {
                List<EdgeMaterialCandidate> convexCandidates =
                    new List<EdgeMaterialCandidate>(candidates.Count);
                List<EdgeMaterialCandidate> concaveCandidates =
                    new List<EdgeMaterialCandidate>(candidates.Count);

                for (int i = 0; i < candidates.Count; i++)
                {
                    EdgeMaterialCandidate candidate = candidates[i];

                    if (candidate.ConvexEdgeWear > 0.001f)
                    {
                        convexCandidates.Add(candidate);
                    }

                    if (candidate.ConcaveCrease > 0.001f)
                    {
                        concaveCandidates.Add(candidate);
                    }

                    if (candidate.DirtDepositBoost > 0.025f)
                    {
                        ApplyCandidateMask(
                            masks,
                            candidate,
                            0f,
                            0f,
                            candidate.DirtDepositBoost,
                            0f,
                            0f);
                    }
                }

                convexCandidates.Sort(
                    (left, right) => right.ConvexEdgeWear.CompareTo(left.ConvexEdgeWear));
                concaveCandidates.Sort(
                    (left, right) => right.ConcaveCrease.CompareTo(left.ConcaveCrease));

                HashSet<EdgeKey> convexSelected = new HashSet<EdgeKey>();
                int convexBudget = ResolveConvexFeatureBudget(
                    recipe,
                    convexCandidates.Count);
                int convexCount = 0;

                for (int i = 0; i < convexCandidates.Count && convexCount < convexBudget; i++)
                {
                    EdgeMaterialCandidate candidate = convexCandidates[i];

                    if (candidate.ConvexEdgeWear < 0.12f)
                    {
                        break;
                    }

                    convexSelected.Add(candidate.Edge.Key);
                    ApplyCandidateMask(
                        masks,
                        candidate,
                        candidate.ConvexEdgeWear,
                        0f,
                        0f,
                        1f,
                        0f);
                    convexCount++;
                }

                int concaveBudget = ResolveConcaveFeatureBudget(
                    recipe,
                    concaveCandidates.Count);
                int concaveCount = 0;

                for (int i = 0; i < concaveCandidates.Count && concaveCount < concaveBudget; i++)
                {
                    EdgeMaterialCandidate candidate = concaveCandidates[i];

                    if (candidate.ConcaveCrease < 0.10f)
                    {
                        break;
                    }

                    if (convexSelected.Contains(candidate.Edge.Key) &&
                        candidate.ConcaveCrease <= candidate.ConvexEdgeWear * 1.15f)
                    {
                        continue;
                    }

                    ApplyCandidateMask(
                        masks,
                        candidate,
                        0f,
                        candidate.ConcaveCrease,
                        0f,
                        0f,
                        1f);
                    concaveCount++;
                }
            }

            private static int ResolveConvexFeatureBudget(
                MassRecipe recipe,
                int candidateCount)
            {
                int budget = recipe.Archetype switch
                {
                    MassArchetype.BrokenChunk => 18,
                    MassArchetype.FracturedPillar => 16,
                    MassArchetype.LayeredStone => 14,
                    MassArchetype.CarvedMarkerStone => 12,
                    MassArchetype.StandingStone => 12,
                    MassArchetype.FlatSlab => 10,
                    MassArchetype.PolishedStone => 6,
                    _ => 12
                };

                budget += recipe.FormComplexity switch
                {
                    FormComplexity.Primitive => -5,
                    FormComplexity.Simple => -3,
                    FormComplexity.Complex => 4,
                    FormComplexity.HighlyComplex => 6,
                    _ => 0
                };

                budget += recipe.EdgeCharacter switch
                {
                    EdgeCharacter.Chipped => 4,
                    EdgeCharacter.Sharp => 2,
                    EdgeCharacter.Worn => -1,
                    EdgeCharacter.Polished => -3,
                    _ => 0
                };

                return Mathf.Clamp(budget, 3, Mathf.Max(3, candidateCount));
            }

            private static int ResolveConcaveFeatureBudget(
                MassRecipe recipe,
                int candidateCount)
            {
                int budget = recipe.Archetype switch
                {
                    MassArchetype.BrokenChunk => 8,
                    MassArchetype.FracturedPillar => 9,
                    MassArchetype.CarvedMarkerStone => 7,
                    MassArchetype.LayeredStone => 5,
                    MassArchetype.StandingStone => 5,
                    MassArchetype.FlatSlab => 4,
                    MassArchetype.PolishedStone => 1,
                    _ => 4
                };

                budget += recipe.FormComplexity switch
                {
                    FormComplexity.Primitive => -2,
                    FormComplexity.Simple => -1,
                    FormComplexity.Complex => 2,
                    FormComplexity.HighlyComplex => 3,
                    _ => 0
                };

                budget += recipe.EdgeCharacter switch
                {
                    EdgeCharacter.Chipped => 2,
                    EdgeCharacter.Sharp => 1,
                    EdgeCharacter.Worn => -1,
                    EdgeCharacter.Polished => -2,
                    _ => 0
                };

                return Mathf.Clamp(budget, 0, Mathf.Max(0, candidateCount));
            }

            private static float GetCreaseSelectionThreshold(
                MassRecipe recipe)
            {
                float threshold = recipe.Archetype switch
                {
                    MassArchetype.BrokenChunk => 0.34f,
                    MassArchetype.FracturedPillar => 0.36f,
                    MassArchetype.CarvedMarkerStone => 0.28f,
                    MassArchetype.LayeredStone => 0.24f,
                    MassArchetype.StandingStone => 0.22f,
                    MassArchetype.FlatSlab => 0.20f,
                    MassArchetype.PolishedStone => 0.06f,
                    _ => 0.18f
                };

                threshold += recipe.FormComplexity switch
                {
                    FormComplexity.Primitive => -0.06f,
                    FormComplexity.Simple => -0.03f,
                    FormComplexity.Complex => 0.04f,
                    FormComplexity.HighlyComplex => 0.06f,
                    _ => 0f
                };

                threshold += recipe.EdgeCharacter switch
                {
                    EdgeCharacter.Chipped => 0.06f,
                    EdgeCharacter.Sharp => 0.03f,
                    EdgeCharacter.Worn => -0.03f,
                    EdgeCharacter.Polished => -0.07f,
                    _ => 0f
                };

                return Mathf.Clamp(threshold, 0.02f, 0.42f);
            }

            private static void ApplyCandidateMask(
                Dictionary<FaceVertexKey, FaceVertexMaterialMask> masks,
                EdgeMaterialCandidate candidate,
                float convexEdgeWear,
                float concaveCrease,
                float dirtDepositBoost,
                float convexEdgeLine,
                float concaveCreaseLine)
            {
                for (int i = 0; i < candidate.Edge.FaceIndices.Count; i++)
                {
                    int faceIndex = candidate.Edge.FaceIndices[i];
                    AddFaceVertexMask(
                        masks,
                        faceIndex,
                        candidate.Edge.Start,
                        convexEdgeWear,
                        concaveCrease,
                        dirtDepositBoost,
                        convexEdgeLine,
                        concaveCreaseLine);
                    AddFaceVertexMask(
                        masks,
                        faceIndex,
                        candidate.Edge.End,
                        convexEdgeWear,
                        concaveCrease,
                        dirtDepositBoost,
                        convexEdgeLine,
                        concaveCreaseLine);
                }
            }

            private static void AddFaceVertexMask(
                Dictionary<FaceVertexKey, FaceVertexMaterialMask> masks,
                int faceIndex,
                Vector3 position,
                float convexEdgeWear,
                float concaveCrease,
                float dirtDepositBoost,
                float convexEdgeLine,
                float concaveCreaseLine)
            {
                FaceVertexKey key = new FaceVertexKey(
                    faceIndex,
                    position);
                masks.TryGetValue(
                    key,
                    out FaceVertexMaterialMask existing);

                masks[key] = new FaceVertexMaterialMask(
                    Mathf.Max(existing.ConvexEdgeWear, convexEdgeWear),
                    Mathf.Max(existing.ConcaveCrease, concaveCrease),
                    Mathf.Max(existing.DirtDepositBoost, dirtDepositBoost),
                    Mathf.Max(existing.ConvexEdgeLine, convexEdgeLine),
                    Mathf.Max(existing.ConcaveCreaseLine, concaveCreaseLine));
            }
        }

        private readonly struct FaceMaskRecord
        {
            public readonly int Index;
            public readonly Vector3 A;
            public readonly Vector3 B;
            public readonly Vector3 C;
            public readonly Vector3 Normal;
            public readonly Vector3 Centre;

            public FaceMaskRecord(
                int index,
                Vector3 a,
                Vector3 b,
                Vector3 c,
                Vector3 normal,
                Vector3 centre)
            {
                Index = index;
                A = a;
                B = b;
                C = c;
                Normal = normal;
                Centre = centre;
            }
        }

        private sealed class EdgeMaterialAggregate
        {
            public readonly EdgeKey Key;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly List<int> FaceIndices = new List<int>(2);

            public EdgeMaterialAggregate(
                EdgeKey key,
                Vector3 start,
                Vector3 end)
            {
                Key = key;
                Start = start;
                End = end;
            }

            public void AddFace(int faceIndex)
            {
                if (!FaceIndices.Contains(faceIndex))
                {
                    FaceIndices.Add(faceIndex);
                }
            }
        }

        private readonly struct EdgeMaterialCandidate
        {
            public readonly EdgeMaterialAggregate Edge;
            public readonly float ConvexEdgeWear;
            public readonly float ConcaveCrease;
            public readonly float DirtDepositBoost;

            public bool HasAnyMask =>
                ConvexEdgeWear > 0.0001f ||
                ConcaveCrease > 0.0001f ||
                DirtDepositBoost > 0.0001f;

            public EdgeMaterialCandidate(
                EdgeMaterialAggregate edge,
                float convexEdgeWear,
                float concaveCrease,
                float dirtDepositBoost)
            {
                Edge = edge;
                ConvexEdgeWear = convexEdgeWear;
                ConcaveCrease = concaveCrease;
                DirtDepositBoost = dirtDepositBoost;
            }
        }

        private readonly struct FaceVertexMaterialMask
        {
            public readonly float ConvexEdgeWear;
            public readonly float ConcaveCrease;
            public readonly float DirtDepositBoost;
            public readonly float ConvexEdgeLine;
            public readonly float ConcaveCreaseLine;

            public FaceVertexMaterialMask(
                float convexEdgeWear,
                float concaveCrease,
                float dirtDepositBoost,
                float convexEdgeLine,
                float concaveCreaseLine)
            {
                ConvexEdgeWear = convexEdgeWear;
                ConcaveCrease = concaveCrease;
                DirtDepositBoost = dirtDepositBoost;
                ConvexEdgeLine = convexEdgeLine;
                ConcaveCreaseLine = concaveCreaseLine;
            }
        }

        private readonly struct FaceVertexKey : IEquatable<FaceVertexKey>
        {
            private readonly int faceIndex;
            private readonly VertexKey vertexKey;

            public FaceVertexKey(
                int faceIndex,
                Vector3 position)
            {
                this.faceIndex = faceIndex;
                vertexKey = new VertexKey(position);
            }

            public bool Equals(FaceVertexKey other)
            {
                return faceIndex == other.faceIndex &&
                    vertexKey.Equals(other.vertexKey);
            }

            public override bool Equals(object obj)
            {
                return obj is FaceVertexKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (faceIndex * 397) ^ vertexKey.GetHashCode();
                }
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

            public PolygonFace(
                List<Vector3> vertices,
                Vector3 normal,
                PolygonFaceFeature feature = PolygonFaceFeature.Base,
                float featureStrength = 0f)
            {
                Vertices = vertices;
                Normal = normal.normalized;
                Feature = feature == PolygonFaceFeature.ConvexEdgeWear
                    ? PolygonFaceFeature.ConvexEdgeWear
                    : PolygonFaceFeature.Base;
                FeatureStrength = Mathf.Clamp01(featureStrength);
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

            public string ToDiagnosticString()
            {
                return x + ":" + y + ":" + z;
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
