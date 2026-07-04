using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static class MassGenerator
    {
        private const float PlaneEpsilon = 0.0001f;

        // Position welding tolerance in the normalized pre-scale mass.
        // Keep this small: larger values can collapse legitimate short cut edges.
        private const float PointMergeDistance = 0.00001f;
        private const float PointMergeDistanceSqr =
            PointMergeDistance * PointMergeDistance;

        // Dimensionless, scale-relative tests. These must not share PlaneEpsilon:
        // plane distance, edge length and triangle area use different units.
        private const float RelativeCollinearEpsilon = 0.0000000001f;
        private const float RelativeTriangleAreaEpsilon = 0.000000000001f;
        private const float MinimumEdgeLengthSqr = 0.000000000001f;
        private const float TinyFaceAreaEpsilon = 0.0000000001f;

        private static readonly Vector3[] BaseVertices =
        {
            new Vector3(-1f,  1.618034f,  0f),
            new Vector3( 1f,  1.618034f,  0f),
            new Vector3(-1f, -1.618034f,  0f),
            new Vector3( 1f, -1.618034f,  0f),
            new Vector3( 0f, -1f,  1.618034f),
            new Vector3( 0f,  1f,  1.618034f),
            new Vector3( 0f, -1f, -1.618034f),
            new Vector3( 0f,  1f, -1.618034f),
            new Vector3( 1.618034f,  0f, -1f),
            new Vector3( 1.618034f,  0f,  1f),
            new Vector3(-1.618034f,  0f, -1f),
            new Vector3(-1.618034f,  0f,  1f)
        };

        private static readonly int[] BaseTriangles =
        {
             0, 11,  5,
             0,  5,  1,
             0,  1,  7,
             0,  7, 10,
             0, 10, 11,
             1,  5,  9,
             5, 11,  4,
            11, 10,  2,
            10,  7,  6,
             7,  1,  8,
             3,  9,  4,
             3,  4,  2,
             3,  2,  6,
             3,  6,  8,
             3,  8,  9,
             4,  9,  5,
             2,  4, 11,
             6,  2, 10,
             8,  6,  7,
             9,  8,  1
        };

        public static MeshData Generate(MassRecipe recipe)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            Vector3 dimensions = ResolveDimensions(recipe);

            TriangleSoup soup = BuildMassSoup(recipe);

            ApplyDimensions(soup.Positions, dimensions);
            ApplyLean(soup.Positions, recipe.Lean, recipe.ShapeSeed);
            ApplyGrounding(soup.Positions, recipe.Grounding);
            RecenterOnGround(soup.Positions);

            return BuildMeshData(soup, recipe);
        }

        private static bool UsesRadialBuilder(MassArchetype archetype)
        {
            return archetype == MassArchetype.PolishedStone;
        }

        private static TriangleSoup BuildMassSoup(MassRecipe recipe)
        {
            return recipe.Archetype switch
            {
                MassArchetype.LayeredStone => BuildLayeredStoneMass(recipe),
                MassArchetype.CarvedMarkerStone => BuildCarvedMarkerMass(recipe),
                _ => UsesRadialBuilder(recipe.Archetype)
                    ? BuildRadialMass(recipe)
                    : BuildPlaneCutMass(recipe)
            };
        }

        #region Plane-cut mass

        private static TriangleSoup BuildPlaneCutMass(MassRecipe recipe)
        {
            System.Random shapeRandom =
                CreateRandom(recipe.ShapeSeed, 0x27101987);

            BoxExtents box = CreateBoxExtents(
                shapeRandom,
                recipe.ShapeDiversity,
                recipe.Archetype);

            List<PolygonFace> faces = CreateBoxFaces(box);

            MacroProfile profile = SelectMacroProfile(
                shapeRandom,
                recipe.Archetype,
                recipe.ShapeDiversity);

            ApplyProfileCuts(
                faces,
                box,
                profile,
                shapeRandom,
                recipe);

            if (recipe.Archetype == MassArchetype.FracturedPillar)
            {
                ApplyFracturedPillarCuts(
                    faces,
                    shapeRandom,
                    recipe);
            }

            GetMajorCutCountRange(
                recipe.FormComplexity,
                out int minimumCuts,
                out int maximumCuts);

            int majorCutCount = shapeRandom.Next(
                minimumCuts,
                maximumCuts + 1);

            GetCutDepthRange(
                recipe.ShapeDiversity,
                out float minimumDepth,
                out float maximumDepth);

            float archetypeDepthMultiplier =
                GetArchetypeCutDepthMultiplier(recipe.Archetype);

            float edgeDepthMultiplier =
                GetEdgeCutDepthMultiplier(recipe.EdgeCharacter);

            for (int i = 0; i < majorCutCount; i++)
            {
                CutRegion region = SelectCutRegion(i, shapeRandom);
                Vector3 normal = RandomCutNormal(shapeRandom, region);

                float depth = RandomRange(
                    shapeRandom,
                    minimumDepth,
                    maximumDepth);

                depth *= archetypeDepthMultiplier;
                depth *= edgeDepthMultiplier;
                depth = Mathf.Clamp(depth, 0.04f, 0.46f);

                ApplyCut(faces, normal, depth);
            }

            int chipCount = GetSecondaryChipCount(
                recipe.EdgeCharacter,
                recipe.FormComplexity);

            for (int i = 0; i < chipCount; i++)
            {
                Vector3 normal = RandomCutNormal(
                    shapeRandom,
                    CutRegion.Any);

                float depth = RandomRange(
                    shapeRandom,
                    0.035f,
                    0.09f);

                ApplyCut(faces, normal, depth);
            }

            return TriangulatePolyhedron(
                faces,
                recipe.SurfaceFacetDensity,
                recipe.EdgeCharacter,
                recipe.SurfaceSeed);
        }

        private static BoxExtents CreateBoxExtents(
            System.Random random,
            ShapeDiversity diversity,
            MassArchetype archetype)
        {
            float variation = diversity switch
            {
                ShapeDiversity.Restrained => 0.07f,
                ShapeDiversity.Broad => 0.16f,
                ShapeDiversity.Wild => 0.28f,
                _ => 0.16f
            };

            float positiveX = RandomRange(random, 1f - variation, 1f + variation);
            float negativeX = RandomRange(random, 1f - variation, 1f + variation);
            float positiveY = RandomRange(random, 0.82f, 1f + variation);
            float negativeY = RandomRange(random, 0.88f, 1.08f);
            float positiveZ = RandomRange(random, 1f - variation, 1f + variation);
            float negativeZ = RandomRange(random, 1f - variation, 1f + variation);

            if (archetype == MassArchetype.BrokenChunk)
            {
                positiveX *= RandomRange(random, 0.82f, 1.18f);
                positiveY *= RandomRange(random, 0.78f, 1.16f);
                negativeZ *= RandomRange(random, 0.82f, 1.18f);
            }

            return new BoxExtents(
                positiveX,
                negativeX,
                positiveY,
                negativeY,
                positiveZ,
                negativeZ);
        }

        private static TriangleSoup BuildLayeredStoneMass(MassRecipe recipe)
        {
            System.Random random =
                CreateRandom(recipe.ShapeSeed, 0x4C617965);

            TriangleSoup soup = new TriangleSoup();
            int layerCount = recipe.FormComplexity switch
            {
                FormComplexity.Primitive => 3,
                FormComplexity.Simple => 4,
                FormComplexity.Moderate => 5,
                _ => 6
            };

            List<RectRing> rings = new List<RectRing>();
            Vector2 centre = Vector2.zero;
            float width = RandomRange(random, 0.95f, 1.18f);
            float depth = RandomRange(random, 0.78f, 1.08f);
            rings.Add(new RectRing(-1f, centre, width, depth));

            for (int level = 1; level <= layerCount; level++)
            {
                float y = Mathf.Lerp(-1f, 1f, level / (float)layerCount);
                float taper = level / (float)layerCount;

                centre.x += RandomRange(random, -0.08f, 0.08f);
                centre.y += RandomRange(random, -0.06f, 0.06f);

                width *= Mathf.Lerp(0.92f, 0.74f, taper) *
                    RandomRange(random, 0.94f, 1.06f);

                depth *= Mathf.Lerp(0.94f, 0.78f, taper) *
                    RandomRange(random, 0.94f, 1.07f);

                rings.Add(new RectRing(y, centre, width, depth));
            }

            Vector3 solidCentre = Vector3.zero;
            AddRectRingCap(
                soup,
                rings[0],
                solidCentre);

            for (int i = 0; i < rings.Count - 1; i++)
            {
                AddRectRingBridge(
                    soup,
                    rings[i],
                    rings[i + 1],
                    solidCentre);
            }

            AddRectRingCap(
                soup,
                rings[rings.Count - 1],
                solidCentre);

            return soup;
        }

        private static TriangleSoup BuildCarvedMarkerMass(MassRecipe recipe)
        {
            System.Random random =
                CreateRandom(recipe.ShapeSeed, 0x4D41524B);

            TriangleSoup soup = new TriangleSoup();
            float frontSign = random.NextDouble() < 0.5 ? 1f : -1f;
            float back = -frontSign * 0.24f;
            float front = frontSign * 0.24f;
            float crownLean = RandomRange(random, -0.18f, 0.18f);

            Vector2[] silhouette =
            {
                new Vector2(-0.72f, -1.00f),
                new Vector2( 0.72f, -1.00f),
                new Vector2( 0.58f, -0.64f),
                new Vector2( 0.48f,  0.50f),
                new Vector2( 0.64f + crownLean,  0.86f),
                new Vector2( 0.12f + crownLean,  1.08f),
                new Vector2(-0.42f + crownLean,  1.02f),
                new Vector2(-0.70f,  0.38f),
                new Vector2(-0.60f, -0.62f)
            };

            AddExtrudedPolygon(
                soup,
                silhouette,
                Mathf.Min(back, front),
                Mathf.Max(back, front));

            return soup;
        }

        private static List<PolygonFace> CreateBoxFaces(BoxExtents box)
        {
            Vector3 nnn = new Vector3(-box.NegativeX, -box.NegativeY, -box.NegativeZ);
            Vector3 nnp = new Vector3(-box.NegativeX, -box.NegativeY,  box.PositiveZ);
            Vector3 npn = new Vector3(-box.NegativeX,  box.PositiveY, -box.NegativeZ);
            Vector3 npp = new Vector3(-box.NegativeX,  box.PositiveY,  box.PositiveZ);
            Vector3 pnn = new Vector3( box.PositiveX, -box.NegativeY, -box.NegativeZ);
            Vector3 pnp = new Vector3( box.PositiveX, -box.NegativeY,  box.PositiveZ);
            Vector3 ppn = new Vector3( box.PositiveX,  box.PositiveY, -box.NegativeZ);
            Vector3 ppp = new Vector3( box.PositiveX,  box.PositiveY,  box.PositiveZ);

            return new List<PolygonFace>
            {
                CreateOrientedFace(Vector3.right,   pnn, ppn, ppp, pnp),
                CreateOrientedFace(Vector3.left,    nnn, nnp, npp, npn),
                CreateOrientedFace(Vector3.up,      npn, npp, ppp, ppn),
                CreateOrientedFace(Vector3.down,    nnn, pnn, pnp, nnp),
                CreateOrientedFace(Vector3.forward, nnp, pnp, ppp, npp),
                CreateOrientedFace(Vector3.back,    nnn, npn, ppn, pnn)
            };
        }

        private static MacroProfile SelectMacroProfile(
            System.Random random,
            MassArchetype archetype,
            ShapeDiversity diversity)
        {
            if (archetype == MassArchetype.FlatSlab)
            {
                return random.NextDouble() < 0.5
                    ? MacroProfile.Wedge
                    : MacroProfile.Block;
            }

            if (archetype == MassArchetype.LayeredStone)
            {
                return random.NextDouble() < 0.65
                    ? MacroProfile.Block
                    : MacroProfile.Shoulder;
            }

            if (archetype == MassArchetype.StandingStone)
            {
                int value = random.Next(0, 3);
                return value switch
                {
                    0 => MacroProfile.Wedge,
                    1 => MacroProfile.Ridge,
                    _ => MacroProfile.Crown
                };
            }

            int maximumExclusive = diversity == ShapeDiversity.Restrained
                ? 3
                : 5;

            return (MacroProfile)random.Next(0, maximumExclusive);
        }

        private static void ApplyProfileCuts(
            List<PolygonFace> faces,
            BoxExtents box,
            MacroProfile profile,
            System.Random random,
            MassRecipe recipe)
        {
            Vector3 horizontal = RandomHorizontalDirection(random);
            float baseDepth = recipe.ShapeDiversity switch
            {
                ShapeDiversity.Restrained => 0.11f,
                ShapeDiversity.Broad => 0.19f,
                ShapeDiversity.Wild => 0.29f,
                _ => 0.19f
            };

            baseDepth *= GetArchetypeCutDepthMultiplier(recipe.Archetype);
            baseDepth *= GetEdgeCutDepthMultiplier(recipe.EdgeCharacter);
            baseDepth = Mathf.Clamp(baseDepth, 0.06f, 0.40f);

            switch (profile)
            {
                case MacroProfile.Block:
                    ApplyCut(
                        faces,
                        (Vector3.up + horizontal * 0.28f).normalized,
                        baseDepth * 0.55f);
                    break;

                case MacroProfile.Wedge:
                    ApplyCut(
                        faces,
                        (Vector3.up * 0.72f + horizontal * 0.70f).normalized,
                        baseDepth * 1.18f);
                    ApplyCut(
                        faces,
                        -horizontal,
                        baseDepth * 0.55f);
                    break;

                case MacroProfile.Shoulder:
                    ApplyCut(
                        faces,
                        horizontal,
                        baseDepth * 1.22f);
                    ApplyCut(
                        faces,
                        (Vector3.up * 0.78f - horizontal * 0.45f).normalized,
                        baseDepth * 0.68f);
                    break;

                case MacroProfile.Ridge:
                    ApplyCut(
                        faces,
                        (Vector3.up * 0.78f + horizontal * 0.52f).normalized,
                        baseDepth * 0.72f);
                    ApplyCut(
                        faces,
                        (Vector3.up * 0.78f - horizontal * 0.52f).normalized,
                        baseDepth * 0.72f);
                    break;

                case MacroProfile.Crown:
                    ApplyCut(
                        faces,
                        (Vector3.up + horizontal * 0.18f).normalized,
                        baseDepth * 0.78f);
                    ApplyCut(
                        faces,
                        (horizontal + Vector3.forward * 0.35f + Vector3.up * 0.30f).normalized,
                        baseDepth * 0.72f);
                    ApplyCut(
                        faces,
                        (-horizontal + Vector3.back * 0.35f + Vector3.up * 0.22f).normalized,
                        baseDepth * 0.62f);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(profile), profile, null);
            }

            if (box.PositiveY > box.NegativeY * 1.08f)
            {
                ApplyCut(
                    faces,
                    (Vector3.up + RandomHorizontalDirection(random) * 0.25f).normalized,
                    baseDepth * 0.45f);
            }
        }

        private static void ApplyFracturedPillarCuts(
            List<PolygonFace> faces,
            System.Random random,
            MassRecipe recipe)
        {
            Vector3 splitAxis = RandomHorizontalDirection(random);
            Vector3 sideAxis =
                Vector3.Cross(Vector3.up, splitAxis).normalized;

            float aggression = recipe.ShapeDiversity switch
            {
                ShapeDiversity.Restrained => 0.92f,
                ShapeDiversity.Broad => 1.08f,
                ShapeDiversity.Wild => 1.28f,
                _ => 1.08f
            };

            ApplyCut(
                faces,
                (splitAxis * 0.78f + Vector3.up * 0.62f).normalized,
                0.30f * aggression);

            ApplyCut(
                faces,
                (-splitAxis * 0.70f + Vector3.up * 0.42f).normalized,
                0.20f * aggression);

            ApplyCut(
                faces,
                (sideAxis * 0.95f + Vector3.up * 0.14f).normalized,
                0.17f * aggression);

            ApplyCut(
                faces,
                (-sideAxis * 0.95f + Vector3.up * 0.08f).normalized,
                0.13f * aggression);
        }

        private static CutRegion SelectCutRegion(
            int index,
            System.Random random)
        {
            if (index == 0)
            {
                return CutRegion.Top;
            }

            if (index == 1 || index == 2)
            {
                return CutRegion.Side;
            }

            double roll = random.NextDouble();

            if (roll < 0.28)
            {
                return CutRegion.Top;
            }

            if (roll < 0.78)
            {
                return CutRegion.Side;
            }

            return CutRegion.Any;
        }

        private static Vector3 RandomCutNormal(
            System.Random random,
            CutRegion region)
        {
            float angle = RandomRange(random, 0f, Mathf.PI * 2f);
            float y = region switch
            {
                CutRegion.Top => RandomRange(random, 0.48f, 0.94f),
                CutRegion.Side => RandomRange(random, -0.16f, 0.34f),
                CutRegion.Any => RandomRange(random, -0.25f, 0.88f),
                _ => 0f
            };

            float horizontalMagnitude =
                Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));

            return new Vector3(
                Mathf.Cos(angle) * horizontalMagnitude,
                y,
                Mathf.Sin(angle) * horizontalMagnitude).normalized;
        }

        private static void ApplyCut(
            List<PolygonFace> faces,
            Vector3 normal,
            float depthFraction)
        {
            normal.Normalize();

            float support = GetCurrentSupport(faces, normal);

            if (support <= PlaneEpsilon)
            {
                return;
            }

            float distance = support * (1f - Mathf.Clamp01(depthFraction));
            distance = Mathf.Max(distance, support * 0.42f);

            ClipPolyhedron(
                faces,
                new CutPlane(normal, distance));
        }

        private static float GetCurrentSupport(
            List<PolygonFace> faces,
            Vector3 normal)
        {
            float support = float.NegativeInfinity;

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;

                for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
                {
                    support = Mathf.Max(
                        support,
                        Vector3.Dot(normal, vertices[vertexIndex]));
                }
            }

            return support;
        }

        private static void ClipPolyhedron(
            List<PolygonFace> faces,
            CutPlane plane)
        {
            List<PolygonFace> clippedFaces = new List<PolygonFace>();
            List<Vector3> capPoints = new List<Vector3>();

            for (int i = 0; i < faces.Count; i++)
            {
                List<Vector3> clipped = ClipPolygon(
                    faces[i].Vertices,
                    plane,
                    capPoints);

                clipped = SanitizePolygon(
                    clipped,
                    faces[i].Normal);

                if (clipped.Count >= 3 &&
                    CalculatePolygonArea(clipped) > TinyFaceAreaEpsilon)
                {
                    clippedFaces.Add(
                        new PolygonFace(clipped, faces[i].Normal));
                }
            }

            List<Vector3> uniqueCapPoints =
                GetUniquePoints(capPoints);

            if (uniqueCapPoints.Count >= 3)
            {
                PolygonFace capFace = CreateOrientedFace(
                    plane.Normal,
                    uniqueCapPoints.ToArray());

                List<Vector3> sanitizedCap = SanitizePolygon(
                    capFace.Vertices,
                    capFace.Normal);

                if (sanitizedCap.Count >= 3 &&
                    CalculatePolygonArea(sanitizedCap) > TinyFaceAreaEpsilon)
                {
                    clippedFaces.Add(
                        new PolygonFace(
                            sanitizedCap,
                            capFace.Normal));
                }
            }

            WeldSharedVertices(clippedFaces);
            SanitizeAllFaces(clippedFaces);

            if (clippedFaces.Count >= 4)
            {
                faces.Clear();
                faces.AddRange(clippedFaces);
            }
        }

        private static List<Vector3> ClipPolygon(
            List<Vector3> vertices,
            CutPlane plane,
            List<Vector3> capPoints)
        {
            List<Vector3> result = new List<Vector3>();

            Vector3 previous = vertices[vertices.Count - 1];
            float previousDistance = plane.SignedDistance(previous);
            bool previousInside = previousDistance <= PlaneEpsilon;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                float currentDistance = plane.SignedDistance(current);
                bool currentInside = currentDistance <= PlaneEpsilon;

                if (previousInside && currentInside)
                {
                    AddPointIfDifferent(result, current);
                }
                else if (previousInside && !currentInside)
                {
                    Vector3 intersection = IntersectEdge(
                        previous,
                        current,
                        previousDistance,
                        currentDistance);

                    AddPointIfDifferent(result, intersection);
                    capPoints.Add(intersection);
                }
                else if (!previousInside && currentInside)
                {
                    Vector3 intersection = IntersectEdge(
                        previous,
                        current,
                        previousDistance,
                        currentDistance);

                    AddPointIfDifferent(result, intersection);
                    AddPointIfDifferent(result, current);
                    capPoints.Add(intersection);
                }

                previous = current;
                previousDistance = currentDistance;
                previousInside = currentInside;
            }

            RemoveClosingDuplicate(result);
            return result;
        }

        private static Vector3 IntersectEdge(
            Vector3 start,
            Vector3 end,
            float startDistance,
            float endDistance)
        {
            float denominator = startDistance - endDistance;

            if (Mathf.Abs(denominator) <= PlaneEpsilon)
            {
                return start;
            }

            float t = startDistance / denominator;
            return Vector3.LerpUnclamped(start, end, t);
        }

        private static PolygonFace CreateOrientedFace(
            Vector3 outwardNormal,
            params Vector3[] points)
        {
            List<Vector3> ordered = new List<Vector3>(points);
            Vector3 centre = CalculateAverage(ordered);

            Vector3 tangent = Mathf.Abs(outwardNormal.y) < 0.9f
                ? Vector3.Cross(outwardNormal, Vector3.up).normalized
                : Vector3.Cross(outwardNormal, Vector3.right).normalized;

            Vector3 bitangent =
                Vector3.Cross(outwardNormal, tangent).normalized;

            ordered.Sort((left, right) =>
            {
                Vector3 leftOffset = left - centre;
                Vector3 rightOffset = right - centre;

                float leftAngle = Mathf.Atan2(
                    Vector3.Dot(leftOffset, bitangent),
                    Vector3.Dot(leftOffset, tangent));

                float rightAngle = Mathf.Atan2(
                    Vector3.Dot(rightOffset, bitangent),
                    Vector3.Dot(rightOffset, tangent));

                return leftAngle.CompareTo(rightAngle);
            });

            Vector3 calculatedNormal = CalculatePolygonNormal(ordered);

            if (Vector3.Dot(calculatedNormal, outwardNormal) < 0f)
            {
                ordered.Reverse();
            }

            return new PolygonFace(ordered, outwardNormal.normalized);
        }

        private static Vector3 CalculatePolygonNormal(
            List<Vector3> vertices)
        {
            Vector3 normal = Vector3.zero;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 next = vertices[(i + 1) % vertices.Count];

                normal.x += (current.y - next.y) * (current.z + next.z);
                normal.y += (current.z - next.z) * (current.x + next.x);
                normal.z += (current.x - next.x) * (current.y + next.y);
            }

            float normalSqrMagnitude = normal.sqrMagnitude;

            return normalSqrMagnitude > MinimumEdgeLengthSqr
                ? normal / Mathf.Sqrt(normalSqrMagnitude)
                : Vector3.up;
        }

        private static TriangleSoup TriangulatePolyhedron(
            List<PolygonFace> faces,
            SurfaceFacetDensity density,
            EdgeCharacter edgeCharacter,
            int surfaceSeed)
        {
            TriangleSoup soup = new TriangleSoup();
            int edgeSegments = GetBoundarySegments(density);

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace sourceFace = faces[faceIndex];

                List<Vector3> cleanFaceVertices = SanitizePolygon(
                    sourceFace.Vertices,
                    sourceFace.Normal);

                if (cleanFaceVertices.Count < 3 ||
                    CalculatePolygonArea(cleanFaceVertices) <= TinyFaceAreaEpsilon)
                {
                    continue;
                }

                PolygonFace face = new PolygonFace(
                    cleanFaceVertices,
                    sourceFace.Normal);

                if (density == SurfaceFacetDensity.Sparse)
                {
                    for (int i = 1; i < face.Vertices.Count - 1; i++)
                    {
                        AddOrientedTriangle(
                            soup,
                            face.Vertices[0],
                            face.Vertices[i],
                            face.Vertices[i + 1],
                            face.Normal);
                    }

                    continue;
                }

                List<Vector3> boundary = BuildSegmentedBoundary(
                    face.Vertices,
                    edgeSegments);

                Vector3 centre = CalculateAverage(boundary);
                float faceRadius = CalculateAverageRadius(boundary, centre);

                float relief = GetSurfaceRelief(density) * faceRadius;
                relief *= GetReliefMultiplier(edgeCharacter);

                float signedVariation =
                    HashSigned(surfaceSeed, faceIndex);

                centre += face.Normal * relief * signedVariation;

                for (int i = 0; i < boundary.Count; i++)
                {
                    Vector3 current = boundary[i];
                    Vector3 next = boundary[(i + 1) % boundary.Count];

                    AddOrientedTriangle(
                        soup,
                        centre,
                        current,
                        next,
                        face.Normal);
                }
            }

            return soup;
        }

        private static List<Vector3> BuildSegmentedBoundary(
            List<Vector3> vertices,
            int segmentsPerEdge)
        {
            List<Vector3> result = new List<Vector3>();

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 start = vertices[i];
                Vector3 end = vertices[(i + 1) % vertices.Count];

                for (int segment = 0; segment < segmentsPerEdge; segment++)
                {
                    float t = segment / (float)segmentsPerEdge;
                    result.Add(Vector3.Lerp(start, end, t));
                }
            }

            return result;
        }

        #endregion

        #region Radial polished mass

        private static TriangleSoup BuildRadialMass(MassRecipe recipe)
        {
            int frequency = GetSurfaceFrequency(recipe.SurfaceFacetDensity);
            Topology topology = BuildGeodesicTopology(frequency);

            Quaternion samplingRotation = CreateSamplingRotation(recipe.SurfaceSeed);
            Vector3[] directions = new Vector3[topology.Directions.Count];

            for (int i = 0; i < directions.Length; i++)
            {
                directions[i] = samplingRotation * topology.Directions[i];
            }

            float[] radii = GenerateRadialRadii(
                directions,
                topology.Neighbours,
                recipe);

            TriangleSoup soup = new TriangleSoup();

            for (int i = 0; i < topology.Triangles.Count; i += 3)
            {
                int a = topology.Triangles[i];
                int b = topology.Triangles[i + 1];
                int c = topology.Triangles[i + 2];

                Vector3 positionA = directions[a] * radii[a];
                Vector3 positionB = directions[b] * radii[b];
                Vector3 positionC = directions[c] * radii[c];

                AddOutwardTriangle(
                    soup,
                    positionA,
                    positionB,
                    positionC);
            }

            return soup;
        }

        private static float[] GenerateRadialRadii(
            Vector3[] directions,
            List<int>[] neighbours,
            MassRecipe recipe)
        {
            System.Random random =
                CreateRandom(recipe.ShapeSeed, 0x6E624EB7);

            float amplitude = recipe.ShapeDiversity switch
            {
                ShapeDiversity.Restrained => 0.055f,
                ShapeDiversity.Broad => 0.11f,
                ShapeDiversity.Wild => 0.17f,
                _ => 0.11f
            };

            int lobeCount = recipe.FormComplexity switch
            {
                FormComplexity.Primitive => 3,
                FormComplexity.Simple => 4,
                FormComplexity.Moderate => 6,
                FormComplexity.Complex => 8,
                FormComplexity.HighlyComplex => 10,
                _ => 6
            };

            DeformationLobe[] lobes = new DeformationLobe[lobeCount];

            for (int i = 0; i < lobeCount; i++)
            {
                lobes[i] = new DeformationLobe(
                    RandomUnitVector(random),
                    RandomRange(random, -amplitude, amplitude),
                    RandomRange(random, -0.35f, 0.20f),
                    RandomRange(random, 1.2f, 2.6f));
            }

            float[] radii = new float[directions.Length];
            float total = 0f;

            for (int i = 0; i < directions.Length; i++)
            {
                float radius = 1f;

                for (int lobeIndex = 0; lobeIndex < lobes.Length; lobeIndex++)
                {
                    DeformationLobe lobe = lobes[lobeIndex];
                    float alignment = Vector3.Dot(directions[i], lobe.Direction);

                    float influence = Mathf.InverseLerp(
                        lobe.FalloffStart,
                        1f,
                        alignment);

                    influence = Mathf.Pow(
                        Mathf.Clamp01(influence),
                        lobe.Power);

                    radius += lobe.Strength * influence;
                }

                radius = Mathf.Clamp(radius, 0.72f, 1.28f);
                radii[i] = radius;
                total += radius;
            }

            float average = total / radii.Length;

            for (int i = 0; i < radii.Length; i++)
            {
                radii[i] /= average;
            }

            GetRadialRegularization(
                recipe.EdgeCharacter,
                out int passes,
                out float strength,
                out float localDifference);

            RelaxRadii(radii, neighbours, passes, strength);
            LimitLocalPointiness(radii, neighbours, localDifference);

            return radii;
        }

        private static void RelaxRadii(
            float[] radii,
            List<int>[] neighbours,
            int passCount,
            float strength)
        {
            float[] working = new float[radii.Length];

            for (int pass = 0; pass < passCount; pass++)
            {
                for (int i = 0; i < radii.Length; i++)
                {
                    float neighbourAverage =
                        CalculateNeighbourAverage(radii, neighbours[i]);

                    working[i] = Mathf.Lerp(
                        radii[i],
                        neighbourAverage,
                        strength);
                }

                Array.Copy(working, radii, radii.Length);
            }
        }

        private static void LimitLocalPointiness(
            float[] radii,
            List<int>[] neighbours,
            float maximumDifference)
        {
            float[] working = new float[radii.Length];

            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < radii.Length; i++)
                {
                    float neighbourAverage =
                        CalculateNeighbourAverage(radii, neighbours[i]);

                    working[i] = Mathf.Clamp(
                        radii[i],
                        neighbourAverage - maximumDifference,
                        neighbourAverage + maximumDifference);
                }

                Array.Copy(working, radii, radii.Length);
            }
        }

        private static float CalculateNeighbourAverage(
            float[] values,
            List<int> neighbours)
        {
            if (neighbours.Count == 0)
            {
                return 1f;
            }

            float total = 0f;

            for (int i = 0; i < neighbours.Count; i++)
            {
                total += values[neighbours[i]];
            }

            return total / neighbours.Count;
        }

        #endregion

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

        private static void ApplyLean(
            List<Vector3> positions,
            LeanStyle lean,
            int shapeSeed)
        {
            float leanAmount = lean switch
            {
                LeanStyle.None => 0f,
                LeanStyle.Subtle => 0.055f,
                LeanStyle.Pronounced => 0.14f,
                _ => 0f
            };

            if (leanAmount <= 0f)
            {
                return;
            }

            GetVerticalRange(
                positions,
                out float minimumY,
                out float maximumY);

            float height = Mathf.Max(0.001f, maximumY - minimumY);

            System.Random random =
                CreateRandom(shapeSeed, 0x5F3759DF);

            Vector3 direction = RandomHorizontalDirection(random);
            Bounds bounds = CalculateBounds(positions);
            float distance = leanAmount * Mathf.Max(bounds.size.x, bounds.size.z);

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 position = positions[i];
                float influence = (position.y - minimumY) / height;
                position += direction * distance * influence;
                positions[i] = position;
            }
        }

        private static void ApplyGrounding(
            List<Vector3> positions,
            GroundingStyle grounding)
        {
            GetGroundingSettings(
                grounding,
                out float bandFraction,
                out float flatteningStrength,
                out float broadeningStrength);

            GetVerticalRange(
                positions,
                out float minimumY,
                out float maximumY);

            float height = Mathf.Max(0.001f, maximumY - minimumY);
            float groundingTop = minimumY + height * bandFraction;

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 position = positions[i];

                if (position.y >= groundingTop)
                {
                    continue;
                }

                float influence = 1f - Mathf.InverseLerp(
                    minimumY,
                    groundingTop,
                    position.y);

                influence = Mathf.SmoothStep(0f, 1f, influence);

                position.y = Mathf.Lerp(
                    position.y,
                    minimumY,
                    flatteningStrength * influence);

                float broadening = 1f + broadeningStrength * influence;
                position.x *= broadening;
                position.z *= broadening;

                positions[i] = position;
            }
        }

        private static void RecenterOnGround(List<Vector3> positions)
        {
            GetVerticalRange(
                positions,
                out float minimumY,
                out float maximumY);

            float height = Mathf.Max(0.001f, maximumY - minimumY);
            float contactBand = minimumY + height * 0.08f;

            Vector2 contactCentre = Vector2.zero;
            int contactCount = 0;

            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i].y > contactBand)
                {
                    continue;
                }

                contactCentre += new Vector2(
                    positions[i].x,
                    positions[i].z);

                contactCount++;
            }

            if (contactCount > 0)
            {
                contactCentre /= contactCount;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 position = positions[i];
                position.x -= contactCentre.x;
                position.z -= contactCentre.y;
                position.y -= minimumY;
                positions[i] = position;
            }
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
            FaceMaterialMaskLookup materialMaskLookup =
                FaceMaterialMaskLookup.Build(
                    soup,
                    centre,
                    bounds,
                    recipe);

            float safeWidth = Mathf.Max(0.001f, bounds.size.x);
            float safeHeight = Mathf.Max(0.001f, bounds.size.y);
            float safeDepth = Mathf.Max(0.001f, bounds.size.z);

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

                Vector3 faceNormal = normal.sqrMagnitude > MinimumEdgeLengthSqr
                    ? normal.normalized
                    : Vector3.up;

                int indexA = AddRenderedVertex(
                    meshData,
                    a,
                    i,
                    faceIndex,
                    bounds,
                    safeWidth,
                    safeHeight,
                    safeDepth,
                    faceNormal,
                    materialMaskLookup,
                    recipe);

                int indexB = AddRenderedVertex(
                    meshData,
                    b,
                    i + 1,
                    faceIndex,
                    bounds,
                    safeWidth,
                    safeHeight,
                    safeDepth,
                    faceNormal,
                    materialMaskLookup,
                    recipe);

                int indexC = AddRenderedVertex(
                    meshData,
                    c,
                    i + 2,
                    faceIndex,
                    bounds,
                    safeWidth,
                    safeHeight,
                    safeDepth,
                    faceNormal,
                    materialMaskLookup,
                    recipe);

                meshData.AddTriangle(indexA, indexB, indexC);
            }

            return meshData;
        }

        private static int AddRenderedVertex(
            MeshData meshData,
            Vector3 position,
            int vertexIndex,
            int faceIndex,
            Bounds bounds,
            float width,
            float height,
            float depth,
            Vector3 faceNormal,
            FaceMaterialMaskLookup materialMaskLookup,
            MassRecipe recipe)
        {
            Vector2 uv = new Vector2(
                (position.x - bounds.min.x) / width,
                (position.z - bounds.min.z) / depth);

            float randomValue = Hash01(
                recipe.SurfaceSeed,
                vertexIndex);

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

            float edgeWear = materialMaskLookup.ResolveConvexEdgeWear(
                faceIndex,
                position);

            float concaveCrease = materialMaskLookup.ResolveConcaveCrease(
                faceIndex,
                position);

            float dirtDeposit = ResolveDirtDepositMask(
                vertical01,
                green,
                blue,
                randomValue,
                materialMaskLookup.ResolveDirtDepositBoost(
                    faceIndex,
                    position));

            Vector4 materialMasks = new Vector4(
                concaveCrease,
                dirtDeposit,
                0f,
                0f);

            return meshData.AddVertex(
                position,
                uv,
                new Color(red, green, blue, edgeWear),
                materialMasks);
        }

        // Vertex colour material contract:
        // R = existing deterministic surface variation.
        // G = upward/flat exposure mask for lighter worn or frosted planes.
        // B = base/side/occlusion mask for darker crevice-like response.
        // A = convex ridge/edge wear intensity.
        //
        // UV2 material contract:
        // X = concave crease or selected crack-darkening mask.
        // Y = dirty deposit / mineral stain mask.
        // ZW = reserved for future biome-specific material state.
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
            float baseContact =
                1f - Mathf.SmoothStep(0.01f, 0.20f, vertical01);

            float sideOcclusion =
                Mathf.SmoothStep(0.24f, 0.90f, 1f - Mathf.Abs(faceNormal.y)) *
                (1f - Mathf.SmoothStep(0.28f, 0.86f, vertical01));

            float shelteredSurface =
                (1f - exposure) *
                (1f - Mathf.SmoothStep(0.46f, 0.96f, vertical01));
            float surfaceBreakup = (randomValue - 0.5f) * 0.045f;

            return Mathf.Clamp01(
                baseContact * 0.58f +
                sideOcclusion * 0.24f +
                shelteredSurface * 0.12f +
                surfaceBreakup);
        }

        private static float ResolveDirtDepositMask(
            float vertical01,
            float exposure,
            float crevice,
            float randomValue,
            float authoredDepositBoost)
        {
            float lowerArea =
                1f - Mathf.SmoothStep(0.10f, 0.58f, vertical01);
            float lowerBand =
                Mathf.SmoothStep(0.02f, 0.18f, vertical01) *
                (1f - Mathf.SmoothStep(0.34f, 0.86f, vertical01));
            float sheltered =
                Mathf.Clamp01(crevice * 0.65f + (1f - exposure) * 0.35f);
            float depositCore =
                lowerArea * 0.48f +
                sheltered * 0.28f +
                crevice * 0.14f +
                authoredDepositBoost * 0.32f;
            float breakup = Mathf.Lerp(
                0.58f,
                1.18f,
                Mathf.SmoothStep(0.18f, 0.92f, randomValue));

            return Mathf.Clamp01(
                depositCore * breakup +
                lowerBand * authoredDepositBoost * 0.24f);
        }

        #endregion

        #region Geodesic topology

        private static Topology BuildGeodesicTopology(int frequency)
        {
            List<Vector3> directions = new List<Vector3>();
            List<int> triangles = new List<int>();
            Dictionary<VertexKey, int> lookup =
                new Dictionary<VertexKey, int>();

            for (int face = 0; face < BaseTriangles.Length; face += 3)
            {
                Vector3 a = BaseVertices[BaseTriangles[face]].normalized;
                Vector3 b = BaseVertices[BaseTriangles[face + 1]].normalized;
                Vector3 c = BaseVertices[BaseTriangles[face + 2]].normalized;

                int[,] localIndices =
                    new int[frequency + 1, frequency + 1];

                for (int row = 0; row <= frequency; row++)
                {
                    int maximumColumn = frequency - row;

                    for (int column = 0; column <= maximumColumn; column++)
                    {
                        float weightB = row / (float)frequency;
                        float weightC = column / (float)frequency;
                        float weightA = 1f - weightB - weightC;

                        Vector3 direction =
                            (a * weightA + b * weightB + c * weightC).normalized;

                        localIndices[row, column] = GetOrCreateVertex(
                            direction,
                            directions,
                            lookup);
                    }
                }

                for (int row = 0; row < frequency; row++)
                {
                    int cellCount = frequency - row;

                    for (int column = 0; column < cellCount; column++)
                    {
                        int v0 = localIndices[row, column];
                        int v1 = localIndices[row + 1, column];
                        int v2 = localIndices[row, column + 1];

                        AddOutwardTopologyTriangle(
                            directions,
                            triangles,
                            v0,
                            v1,
                            v2);

                        if (column >= cellCount - 1)
                        {
                            continue;
                        }

                        int v3 = localIndices[row + 1, column + 1];

                        AddOutwardTopologyTriangle(
                            directions,
                            triangles,
                            v1,
                            v3,
                            v2);
                    }
                }
            }

            List<int>[] neighbours = BuildNeighbourLists(
                directions.Count,
                triangles);

            return new Topology(directions, triangles, neighbours);
        }

        private static int GetOrCreateVertex(
            Vector3 direction,
            List<Vector3> directions,
            Dictionary<VertexKey, int> lookup)
        {
            VertexKey key = new VertexKey(direction);

            if (lookup.TryGetValue(key, out int existing))
            {
                return existing;
            }

            int index = directions.Count;
            directions.Add(direction);
            lookup.Add(key, index);
            return index;
        }

        private static void AddOutwardTopologyTriangle(
            List<Vector3> positions,
            List<int> triangles,
            int a,
            int b,
            int c)
        {
            Vector3 normal = Vector3.Cross(
                positions[b] - positions[a],
                positions[c] - positions[a]);

            Vector3 faceCentre =
                (positions[a] + positions[b] + positions[c]) / 3f;

            if (Vector3.Dot(normal, faceCentre) < 0f)
            {
                int temporary = b;
                b = c;
                c = temporary;
            }

            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        private static List<int>[] BuildNeighbourLists(
            int vertexCount,
            List<int> triangles)
        {
            HashSet<int>[] sets = new HashSet<int>[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                sets[i] = new HashSet<int>();
            }

            for (int i = 0; i < triangles.Count; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                AddNeighbourPair(sets, a, b);
                AddNeighbourPair(sets, b, c);
                AddNeighbourPair(sets, c, a);
            }

            List<int>[] neighbours = new List<int>[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                neighbours[i] = new List<int>(sets[i]);
            }

            return neighbours;
        }

        private static void AddNeighbourPair(
            HashSet<int>[] sets,
            int a,
            int b)
        {
            sets[a].Add(b);
            sets[b].Add(a);
        }

        #endregion

        #region Helpers and settings

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

            soup.Positions.Add(a);
            soup.Positions.Add(b);
            soup.Positions.Add(c);
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
            Vector3 expectedNormal)
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

            soup.Positions.Add(a);
            soup.Positions.Add(b);
            soup.Positions.Add(c);
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

        #endregion

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

        private sealed class TriangleSoup
        {
            public readonly List<Vector3> Positions = new List<Vector3>();
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

                foreach (EdgeMaterialAggregate edge in edges.Values)
                {
                    EdgeMaterialMask edgeMask = ResolveEdgeMaterialMask(
                        edge,
                        faces,
                        bounds,
                        maximumDimension,
                        recipe);

                    if (edgeMask.IsNeutral)
                    {
                        continue;
                    }

                    for (int i = 0; i < edge.FaceIndices.Count; i++)
                    {
                        int faceIndex = edge.FaceIndices[i];
                        AddFaceVertexMask(
                            masks,
                            faceIndex,
                            edge.Start,
                            edgeMask);
                        AddFaceVertexMask(
                            masks,
                            faceIndex,
                            edge.End,
                            edgeMask);
                    }
                }

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
                    edge = new EdgeMaterialAggregate(start, end);
                    edges.Add(key, edge);
                }

                edge.AddFace(faceIndex);
            }

            private static EdgeMaterialMask ResolveEdgeMaterialMask(
                EdgeMaterialAggregate edge,
                Dictionary<int, FaceMaskRecord> faces,
                Bounds bounds,
                float maximumDimension,
                MassRecipe recipe)
            {
                float edgeLength = (edge.End - edge.Start).magnitude;
                float edgeLength01 = edgeLength / maximumDimension;
                float readableLength = Mathf.SmoothStep(
                    0.035f,
                    0.16f,
                    edgeLength01);

                if (readableLength <= 0.001f)
                {
                    return default;
                }

                Vector3 midpoint = (edge.Start + edge.End) * 0.5f;
                float safeHeight = Mathf.Max(0.001f, bounds.size.y);
                float vertical01 = Mathf.Clamp01(
                    (midpoint.y - bounds.min.y) / safeHeight);
                float baseSuppression = Mathf.SmoothStep(
                    0.055f,
                    0.20f,
                    vertical01);
                float exposedHeight = Mathf.SmoothStep(
                    0.14f,
                    0.86f,
                    vertical01);
                float lowerDepositBand =
                    Mathf.SmoothStep(0.015f, 0.18f, vertical01) *
                    (1f - Mathf.SmoothStep(0.42f, 0.90f, vertical01));

                float convexCandidate = 0f;
                float concaveCandidate = 0f;
                float fractureCandidate = 0f;

                if (edge.FaceIndices.Count <= 1)
                {
                    convexCandidate = 0.38f * readableLength;
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
                                0.16f,
                                0.58f,
                                angleAmount);

                            if (angleScore <= 0.001f)
                            {
                                continue;
                            }

                            Vector3 centreDelta = second.Centre - first.Centre;

                            if (centreDelta.sqrMagnitude <= MinimumEdgeLengthSqr)
                            {
                                continue;
                            }

                            Vector3 direction = centreDelta.normalized;
                            float firstSide = Vector3.Dot(
                                direction,
                                first.Normal);
                            float secondSide = Vector3.Dot(
                                -direction,
                                second.Normal);
                            float convexness = Mathf.Clamp01(
                                (-firstSide - secondSide) * 0.5f);
                            float concaveness = Mathf.Clamp01(
                                (firstSide + secondSide) * 0.5f);

                            convexCandidate = Mathf.Max(
                                convexCandidate,
                                angleScore * convexness);
                            concaveCandidate = Mathf.Max(
                                concaveCandidate,
                                angleScore * concaveness);
                            fractureCandidate = Mathf.Max(
                                fractureCandidate,
                                angleScore);
                        }
                    }
                }

                int edgeHash = new EdgeKey(edge.Start, edge.End).GetHashCode();
                float wearBreakup = Mathf.Lerp(
                    0.72f,
                    1.10f,
                    Hash01(
                        unchecked(recipe.SurfaceSeed ^ 0x37A1D5),
                        edgeHash));
                float convexEdgeWear =
                    convexCandidate *
                    readableLength *
                    baseSuppression *
                    Mathf.Lerp(0.72f, 1.12f, exposedHeight) *
                    wearBreakup;
                convexEdgeWear = Mathf.SmoothStep(
                    0.16f,
                    0.78f,
                    convexEdgeWear);

                float selectionThreshold = GetCreaseSelectionThreshold(recipe);
                float creaseRandom = Hash01(
                    unchecked(recipe.SurfaceSeed ^ 0x5EED5EA),
                    edgeHash);
                float selectedFracture = creaseRandom <= selectionThreshold ? 1f : 0f;
                float longReadableEdge = Mathf.SmoothStep(
                    0.07f,
                    0.24f,
                    edgeLength01);
                float upperCutoff =
                    1f - Mathf.SmoothStep(0.88f, 1.0f, vertical01) * 0.55f;
                float concaveCrease =
                    concaveCandidate * 0.95f +
                    fractureCandidate *
                    longReadableEdge *
                    selectedFracture *
                    0.52f;
                concaveCrease *=
                    Mathf.SmoothStep(0.045f, 0.18f, vertical01) *
                    upperCutoff;
                concaveCrease = Mathf.SmoothStep(
                    0.18f,
                    0.76f,
                    concaveCrease);

                float dirtBreakup = Mathf.Lerp(
                    0.68f,
                    1.16f,
                    Hash01(
                        unchecked(recipe.SurfaceSeed ^ 0xD171),
                        edgeHash));
                float dirtDepositBoost = Mathf.Clamp01(
                    lowerDepositBand *
                    readableLength *
                    dirtBreakup *
                    (0.20f + concaveCrease * 0.42f +
                     (1f - convexEdgeWear) * 0.10f));

                return new EdgeMaterialMask(
                    convexEdgeWear,
                    concaveCrease,
                    dirtDepositBoost);
            }

            private static float GetCreaseSelectionThreshold(
                MassRecipe recipe)
            {
                float threshold = recipe.Archetype switch
                {
                    MassArchetype.BrokenChunk => 0.40f,
                    MassArchetype.FracturedPillar => 0.42f,
                    MassArchetype.CarvedMarkerStone => 0.34f,
                    MassArchetype.LayeredStone => 0.30f,
                    MassArchetype.StandingStone => 0.26f,
                    MassArchetype.FlatSlab => 0.24f,
                    MassArchetype.PolishedStone => 0.10f,
                    _ => 0.22f
                };

                threshold += recipe.FormComplexity switch
                {
                    FormComplexity.Primitive => -0.05f,
                    FormComplexity.Simple => -0.02f,
                    FormComplexity.Complex => 0.05f,
                    FormComplexity.HighlyComplex => 0.08f,
                    _ => 0f
                };

                threshold += recipe.EdgeCharacter switch
                {
                    EdgeCharacter.Chipped => 0.07f,
                    EdgeCharacter.Sharp => 0.04f,
                    EdgeCharacter.Worn => -0.03f,
                    EdgeCharacter.Polished => -0.08f,
                    _ => 0f
                };

                return Mathf.Clamp(threshold, 0.04f, 0.48f);
            }

            private static void AddFaceVertexMask(
                Dictionary<FaceVertexKey, FaceVertexMaterialMask> masks,
                int faceIndex,
                Vector3 position,
                EdgeMaterialMask edgeMask)
            {
                FaceVertexKey key = new FaceVertexKey(
                    faceIndex,
                    position);
                masks.TryGetValue(
                    key,
                    out FaceVertexMaterialMask existing);

                masks[key] = new FaceVertexMaterialMask(
                    Mathf.Max(existing.ConvexEdgeWear, edgeMask.ConvexEdgeWear),
                    Mathf.Max(existing.ConcaveCrease, edgeMask.ConcaveCrease),
                    Mathf.Max(existing.DirtDepositBoost, edgeMask.DirtDepositBoost));
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
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly List<int> FaceIndices = new List<int>(2);

            public EdgeMaterialAggregate(
                Vector3 start,
                Vector3 end)
            {
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

        private readonly struct EdgeMaterialMask
        {
            public readonly float ConvexEdgeWear;
            public readonly float ConcaveCrease;
            public readonly float DirtDepositBoost;

            public bool IsNeutral =>
                ConvexEdgeWear <= 0.0001f &&
                ConcaveCrease <= 0.0001f &&
                DirtDepositBoost <= 0.0001f;

            public EdgeMaterialMask(
                float convexEdgeWear,
                float concaveCrease,
                float dirtDepositBoost)
            {
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

            public FaceVertexMaterialMask(
                float convexEdgeWear,
                float concaveCrease,
                float dirtDepositBoost)
            {
                ConvexEdgeWear = convexEdgeWear;
                ConcaveCrease = concaveCrease;
                DirtDepositBoost = dirtDepositBoost;
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

            public PolygonFace(List<Vector3> vertices, Vector3 normal)
            {
                Vertices = vertices;
                Normal = normal.normalized;
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
    }
}
