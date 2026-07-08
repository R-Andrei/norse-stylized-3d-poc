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
            return Generate(recipe, null);
        }

        public static MeshData Generate(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            Vector3 dimensions = ResolveDimensions(recipe);

            TriangleSoup soup = BuildMassSoup(recipe, surfaceFeatures);

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

        private static TriangleSoup BuildMassSoup(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures)
        {
            return recipe.Archetype switch
            {
                MassArchetype.LayeredStone => BuildLayeredStoneMass(recipe),
                MassArchetype.CarvedMarkerStone => BuildCarvedMarkerMass(recipe),
                _ => UsesRadialBuilder(recipe.Archetype)
                    ? BuildRadialMass(recipe)
                    : BuildPlaneCutMass(recipe, surfaceFeatures)
            };
        }

        #region Plane-cut mass

        private static TriangleSoup BuildPlaneCutMass(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures)
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

            ApplyGeneratedEdgeWearBevels(
                faces,
                recipe,
                surfaceFeatures);

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

        private static void ApplyGeneratedEdgeWearBevels(
            List<PolygonFace> faces,
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures)
        {
            if (!surfaceFeatures.HasValue || faces.Count < 4)
            {
                return;
            }

            MassSurfaceFeatureSettings settings = surfaceFeatures.Value;
            float amount01 = Mathf.Clamp01(settings.EdgeWearAmount * 0.5f);
            if (amount01 <= 0.001f)
            {
                return;
            }

            Bounds bounds = CalculateFaceBounds(faces);
            float maximumDimension = Mathf.Max(
                0.0001f,
                Mathf.Max(
                    bounds.size.x,
                    Mathf.Max(bounds.size.y, bounds.size.z)));

            // EW-4D0.3 keeps the EW-4A.1 control contract: Width owns
            // physical bevel depth, Amount owns generated material strength,
            // and Softness is reserved for material/normal response. The active
            // construction path is being moved to a topology-first graph: this
            // step preflights graph mapping, per-face selected-edge clipping,
            // shared rail extraction, and sampled profile-grid preparation
            // before the older half-space fallback runs.
            float width01 = Mathf.InverseLerp(0.25f, 2f, settings.EdgeWearWidth);
            float bevelDepth = maximumDimension * Mathf.Lerp(0.0035f, 0.0115f, width01);
            bevelDepth = Mathf.Clamp(
                bevelDepth,
                maximumDimension * 0.0025f,
                maximumDimension * 0.016f);

            List<EdgeWearBevelCandidate> candidates =
                BuildEdgeWearBevelCandidates(
                    faces,
                    bounds,
                    maximumDimension,
                    recipe,
                    settings,
                    amount01);
            if (candidates.Count == 0)
            {
                WarnEdgeWearBevelFailure(new EdgeWearBevelBuildStats(0, 0));
                return;
            }

            candidates.Sort((left, right) => right.Score.CompareTo(left.Score));

            float coverage01 = Mathf.Clamp01(settings.EdgeWearCoverage * 0.5f);
            float coverageFraction = Mathf.Lerp(0.0f, 1.0f, coverage01);
            int selectedCount = Mathf.Clamp(
                Mathf.CeilToInt(candidates.Count * coverageFraction),
                0,
                candidates.Count);

            if (selectedCount <= 0)
            {
                return;
            }

            float minimumStableFaceArea = Mathf.Max(
                TinyFaceAreaEpsilon * 12f,
                maximumDimension * maximumDimension * 0.0000007f);
            float minimumStableEdgeLength = maximumDimension * 0.0012f;

            EdgeWearBevelBuildStats stats = new EdgeWearBevelBuildStats(
                candidates.Count,
                selectedCount);

            if (!TryPreflightTopologyBevelGraph(
                    faces,
                    candidates,
                    selectedCount,
                    bevelDepth,
                    minimumStableFaceArea,
                    minimumStableEdgeLength,
                    ref stats))
            {
                // EW-4D0.4 fail-closed: if selected edge candidates cannot be
                // mapped into a real topology graph, if affected faces cannot
                // produce shared clipped rails, if selected edges cannot
                // produce stable sampled profile grids, or if temporary
                // clipped base-face replacements cannot be built, later bevel
                // ribbons would be operating on untrusted data.
                WarnEdgeWearBevelFailure(stats);
                return;
            }

            if (!TryApplyHalfSpaceEdgeWearBevels(
                    faces,
                    candidates,
                    selectedCount,
                    bevelDepth,
                    bounds,
                    minimumStableFaceArea,
                    minimumStableEdgeLength,
                    ref stats))
            {
                // Fail closed. Edge wear is optional; broken bevel topology is not.
                WarnEdgeWearBevelFailure(stats);
                return;
            }
        }

        private static List<EdgeWearBevelCandidate> BuildEdgeWearBevelCandidates(
            List<PolygonFace> faces,
            Bounds bounds,
            float maximumDimension,
            MassRecipe recipe,
            MassSurfaceFeatureSettings settings,
            float amount01)
        {
            Dictionary<EdgeKey, EdgeWearEdgeAggregate> edges =
                new Dictionary<EdgeKey, EdgeWearEdgeAggregate>();

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.Feature != PolygonFaceFeature.Base)
                {
                    continue;
                }

                for (int vertexIndex = 0; vertexIndex < face.Vertices.Count; vertexIndex++)
                {
                    Vector3 start = face.Vertices[vertexIndex];
                    Vector3 end = face.Vertices[(vertexIndex + 1) % face.Vertices.Count];
                    if ((end - start).sqrMagnitude <= MinimumEdgeLengthSqr)
                    {
                        continue;
                    }

                    EdgeKey key = new EdgeKey(start, end);
                    if (!edges.TryGetValue(key, out EdgeWearEdgeAggregate edge))
                    {
                        edge = new EdgeWearEdgeAggregate(start, end);
                        edges.Add(key, edge);
                    }

                    edge.AddFace(faceIndex);
                }
            }

            List<EdgeWearBevelCandidate> candidates =
                new List<EdgeWearBevelCandidate>(edges.Count);
            int candidateIndex = 0;

            foreach (EdgeWearEdgeAggregate edge in edges.Values)
            {
                if (edge.FaceIndices.Count != 2)
                {
                    continue;
                }

                int faceA = edge.FaceIndices[0];
                int faceB = edge.FaceIndices[1];
                PolygonFace first = faces[faceA];
                PolygonFace second = faces[faceB];
                Vector3 normalSum = first.Normal + second.Normal;
                if (normalSum.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    continue;
                }

                Vector3 bevelNormal = normalSum.normalized;
                Vector3 edgeVector = edge.End - edge.Start;
                float length = edgeVector.magnitude;
                if (length <= Mathf.Max(0.0001f, maximumDimension * 0.015f))
                {
                    continue;
                }

                float angleScore = Mathf.Clamp01(
                    (1f - Vector3.Dot(first.Normal, second.Normal)) * 0.72f);
                if (angleScore <= 0.035f)
                {
                    continue;
                }

                Vector3 midpoint = (edge.Start + edge.End) * 0.5f;
                float vertical01 = Mathf.InverseLerp(
                    bounds.min.y,
                    bounds.max.y,
                    midpoint.y);
                float baseSuppression = Mathf.SmoothStep(0.06f, 0.20f, vertical01);
                if (baseSuppression <= 0.001f)
                {
                    continue;
                }

                float lengthScore = Mathf.Clamp01(
                    length / Mathf.Max(0.0001f, maximumDimension * 0.34f));
                float upwardEdgeBoost = Mathf.Lerp(
                    0.82f,
                    1.08f,
                    Mathf.Clamp01((first.Normal.y + second.Normal.y) * 0.5f + 0.5f));
                float characterBoost = recipe.EdgeCharacter switch
                {
                    EdgeCharacter.Sharp => 1.08f,
                    EdgeCharacter.Chipped => 1.22f,
                    EdgeCharacter.Worn => 0.86f,
                    EdgeCharacter.Polished => 0.62f,
                    _ => 1f
                };
                float random = HashPosition01(
                    settings.SurfaceSeed + 0x4A17,
                    midpoint + bevelNormal * 0.173f);
                float score =
                    (angleScore * 0.58f + lengthScore * 0.27f + random * 0.15f) *
                    baseSuppression *
                    upwardEdgeBoost *
                    characterBoost;

                // Amount controls generated worn-face material strength, not
                // physical bevel depth. Macro/Micro remain reserved controls;
                // this deterministic variation keeps selected edges from looking
                // cloned without exposing it as final Macro behaviour yet.
                float deterministicVariation = Mathf.Lerp(
                    0.90f,
                    1.08f,
                    Hash01(settings.SurfaceSeed + 0x29AF, candidateIndex));
                float strength = Mathf.Clamp01(
                    amount01 *
                    Mathf.Lerp(0.86f, 1.06f, random) *
                    deterministicVariation);
                float depthMultiplier = Mathf.Clamp(
                    Mathf.Lerp(0.88f, 1.08f, random) *
                    Mathf.Lerp(0.96f, 1.04f, angleScore),
                    0.78f,
                    1.15f);

                candidates.Add(
                    new EdgeWearBevelCandidate(
                        candidateIndex,
                        edge.Start,
                        edge.End,
                        faceA,
                        faceB,
                        first.Normal,
                        second.Normal,
                        midpoint,
                        bevelNormal,
                        score,
                        strength,
                        depthMultiplier));
                candidateIndex++;
            }

            return candidates;
        }

        private static bool TryPreflightTopologyBevelGraph(
            List<PolygonFace> faces,
            List<EdgeWearBevelCandidate> candidates,
            int selectedCount,
            float bevelDepth,
            float minimumStableFaceArea,
            float minimumStableEdgeLength,
            ref EdgeWearBevelBuildStats stats)
        {
            if (!TryBuildEdgeWearTopologyGraph(
                    faces,
                    out EdgeWearTopologyGraph graph,
                    out EdgeWearGraphBuildStats graphStats))
            {
                stats.ApplyGraphStats(graphStats);
                stats.RegisterReject(EdgeWearBevelRejectReason.ValidationGlobal);
                return false;
            }

            if (!TryMapSelectedCandidatesToGraph(
                    graph,
                    candidates,
                    selectedCount,
                    out List<EdgeWearSelectedGraphEdge> selectedEdges,
                    ref graphStats))
            {
                stats.ApplyGraphStats(graphStats);
                stats.RegisterReject(EdgeWearBevelRejectReason.ValidationGlobal);
                return false;
            }

            if (!TryPreflightSelectedFaceClips(
                    faces,
                    graph,
                    selectedEdges,
                    bevelDepth,
                    minimumStableFaceArea,
                    minimumStableEdgeLength,
                    ref graphStats))
            {
                stats.ApplyGraphStats(graphStats);
                if (graphStats.FaceClipFailedFaceCount > 0)
                {
                    stats.RegisterReject(EdgeWearBevelRejectReason.FaceClip);
                }
                else if (graphStats.BaseFaceValidationFailureCount > 0)
                {
                    stats.RegisterReject(EdgeWearBevelRejectReason.ValidationBaseFace);
                }
                else if (graphStats.FragmentedRailCount > 0)
                {
                    stats.RegisterReject(EdgeWearBevelRejectReason.RailFragmented);
                }
                else
                {
                    stats.RegisterReject(EdgeWearBevelRejectReason.RailExtraction);
                }

                return false;
            }

            stats.ApplyGraphStats(graphStats);
            return selectedEdges.Count == selectedCount;
        }

        private static bool TryBuildEdgeWearTopologyGraph(
            List<PolygonFace> faces,
            out EdgeWearTopologyGraph graph,
            out EdgeWearGraphBuildStats stats)
        {
            graph = new EdgeWearTopologyGraph();
            stats = new EdgeWearGraphBuildStats();

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.Feature != PolygonFaceFeature.Base)
                {
                    continue;
                }

                List<int> vertexIndices = new List<int>(face.Vertices.Count);
                for (int vertexIndex = 0; vertexIndex < face.Vertices.Count; vertexIndex++)
                {
                    int graphVertexIndex = GetOrAddEdgeWearGraphVertex(
                        graph,
                        face.Vertices[vertexIndex]);
                    vertexIndices.Add(graphVertexIndex);
                }

                if (GetUniqueGraphVertexCount(vertexIndices) < 3)
                {
                    stats.InvalidFaceCount++;
                    continue;
                }

                List<int> edgeIndices = new List<int>(face.Vertices.Count);
                for (int vertexIndex = 0; vertexIndex < face.Vertices.Count; vertexIndex++)
                {
                    int startVertex = vertexIndices[vertexIndex];
                    int endVertex = vertexIndices[(vertexIndex + 1) % vertexIndices.Count];
                    if (startVertex == endVertex)
                    {
                        stats.InvalidEdgeCount++;
                        continue;
                    }

                    int edgeIndex = GetOrAddEdgeWearGraphEdge(
                        graph,
                        startVertex,
                        endVertex,
                        face.Vertices[vertexIndex],
                        face.Vertices[(vertexIndex + 1) % face.Vertices.Count]);
                    EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                    edge.TryAddFace(faceIndex);

                    edgeIndices.Add(edgeIndex);
                    graph.Vertices[startVertex].AddFace(faceIndex);
                    graph.Vertices[endVertex].AddFace(faceIndex);
                }

                graph.Faces.Add(
                    new EdgeWearGraphFace(
                        faceIndex,
                        face,
                        vertexIndices,
                        edgeIndices));
            }

            for (int edgeIndex = 0; edgeIndex < graph.Edges.Count; edgeIndex++)
            {
                EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                if (edge.FaceA < 0 || edge.FaceB < 0)
                {
                    stats.GraphBoundaryEdgeCount++;
                }

                if (edge.ExtraFaceCount > 0)
                {
                    stats.GraphNonManifoldEdgeCount = CountGraphNonManifoldEdges(graph);
                }
            }

            stats.GraphVertexCount = graph.Vertices.Count;
            stats.GraphEdgeCount = graph.Edges.Count;
            stats.GraphFaceCount = graph.Faces.Count;

            return stats.GraphFaceCount > 0 &&
                stats.GraphNonManifoldEdgeCount == 0 &&
                stats.InvalidFaceCount == 0 &&
                stats.InvalidEdgeCount == 0;
        }

        private static bool TryMapSelectedCandidatesToGraph(
            EdgeWearTopologyGraph graph,
            List<EdgeWearBevelCandidate> candidates,
            int selectedCount,
            out List<EdgeWearSelectedGraphEdge> selectedEdges,
            ref EdgeWearGraphBuildStats stats)
        {
            selectedEdges = new List<EdgeWearSelectedGraphEdge>(selectedCount);
            int limit = Mathf.Clamp(selectedCount, 0, candidates.Count);
            for (int i = 0; i < limit; i++)
            {
                EdgeWearBevelCandidate candidate = candidates[i];
                EdgeKey key = new EdgeKey(candidate.Start, candidate.End);
                if (!graph.EdgeByKey.TryGetValue(key, out int graphEdgeIndex))
                {
                    stats.MissingSelectedGraphEdgeCount++;
                    continue;
                }

                EdgeWearGraphEdge edge = graph.Edges[graphEdgeIndex];
                if (!GraphEdgeMatchesCandidateFaces(edge, candidate))
                {
                    stats.MismatchedSelectedGraphFaceCount++;
                    continue;
                }

                if (edge.Selected)
                {
                    stats.DuplicateSelectedGraphEdgeCount++;
                    continue;
                }

                edge.Selected = true;
                edge.CandidateIndex = candidate.CandidateIndex;
                selectedEdges.Add(
                    new EdgeWearSelectedGraphEdge(
                        graphEdgeIndex,
                        candidate.CandidateIndex,
                        candidate));
                stats.SelectedGraphEdgeCount++;
            }

            return stats.SelectedGraphEdgeCount == limit &&
                stats.MissingSelectedGraphEdgeCount == 0 &&
                stats.MismatchedSelectedGraphFaceCount == 0 &&
                stats.DuplicateSelectedGraphEdgeCount == 0;
        }

        private static bool TryPreflightSelectedFaceClips(
            List<PolygonFace> sourceFaces,
            EdgeWearTopologyGraph graph,
            List<EdgeWearSelectedGraphEdge> selectedEdges,
            float bevelDepth,
            float minimumStableFaceArea,
            float minimumStableEdgeLength,
            ref EdgeWearGraphBuildStats stats)
        {
            if (selectedEdges.Count == 0)
            {
                return false;
            }

            Dictionary<int, List<FaceInsetCut>> cutsByFace =
                new Dictionary<int, List<FaceInsetCut>>();
            Dictionary<long, FaceInsetCut> cutsByCandidateFace =
                new Dictionary<long, FaceInsetCut>(selectedEdges.Count * 2);

            for (int i = 0; i < selectedEdges.Count; i++)
            {
                EdgeWearBevelCandidate candidate = selectedEdges[i].Candidate;
                float candidateDepth = bevelDepth * candidate.DepthMultiplier;

                if (!TryBuildFaceInsetCut(
                        candidate,
                        candidate.FaceA,
                        sourceFaces[candidate.FaceA],
                        candidateDepth,
                        out FaceInsetCut cutA))
                {
                    stats.FaceClipFailedFaceCount++;
                    continue;
                }

                if (!TryBuildFaceInsetCut(
                        candidate,
                        candidate.FaceB,
                        sourceFaces[candidate.FaceB],
                        candidateDepth,
                        out FaceInsetCut cutB))
                {
                    stats.FaceClipFailedFaceCount++;
                    continue;
                }

                AddFaceInsetCut(cutsByFace, cutA);
                AddFaceInsetCut(cutsByFace, cutB);
                cutsByCandidateFace[MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceA)] = cutA;
                cutsByCandidateFace[MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceB)] = cutB;
                stats.SelectedFaceEdgeCount += 2;
            }

            Dictionary<int, List<Vector3>> clippedPolygonsByFace =
                new Dictionary<int, List<Vector3>>(cutsByFace.Count);
            float localBaseMinEdgeLength = Mathf.Max(
                PlaneEpsilon * 8f,
                minimumStableEdgeLength * 0.35f);
            float localBaseFaceMinArea = Mathf.Max(
                TinyFaceAreaEpsilon * 4f,
                minimumStableFaceArea * 0.2f);

            for (int faceIndex = 0; faceIndex < graph.Faces.Count; faceIndex++)
            {
                EdgeWearGraphFace graphFace = graph.Faces[faceIndex];
                int sourceFaceIndex = graphFace.SourceFaceIndex;
                if (!cutsByFace.TryGetValue(sourceFaceIndex, out List<FaceInsetCut> faceCuts))
                {
                    continue;
                }

                stats.FaceClipAffectedFaceCount++;
                PolygonFace sourceFace = graphFace.SourceFace;
                List<Vector3> polygon = new List<Vector3>(sourceFace.Vertices);
                bool failed = false;

                for (int cutIndex = 0; cutIndex < faceCuts.Count; cutIndex++)
                {
                    polygon = ClipPolygon(
                        polygon,
                        faceCuts[cutIndex].Plane,
                        new List<Vector3>());
                    polygon = SanitizePolygon(polygon, sourceFace.Normal);

                    if (!ValidateLocalEdgeWearFace(
                            polygon,
                            localBaseFaceMinArea,
                            localBaseMinEdgeLength))
                    {
                        failed = true;
                        break;
                    }
                }

                if (failed)
                {
                    stats.FaceClipFailedFaceCount++;
                    continue;
                }

                stats.FaceClipSucceededFaceCount++;
                clippedPolygonsByFace[sourceFaceIndex] = polygon;
            }

            stats.ExpectedRailCount = selectedEdges.Count * 2;
            float railTolerance = Mathf.Max(
                PlaneEpsilon * 18f,
                minimumStableEdgeLength * 0.8f);
            Dictionary<long, BevelRail> railsByCandidateFace =
                new Dictionary<long, BevelRail>(stats.ExpectedRailCount);

            for (int i = 0; i < selectedEdges.Count; i++)
            {
                EdgeWearBevelCandidate candidate = selectedEdges[i].Candidate;
                TryPreflightRailForCandidateFace(
                    clippedPolygonsByFace,
                    cutsByCandidateFace,
                    railsByCandidateFace,
                    candidate,
                    candidate.FaceA,
                    railTolerance,
                    minimumStableEdgeLength,
                    ref stats);

                TryPreflightRailForCandidateFace(
                    clippedPolygonsByFace,
                    cutsByCandidateFace,
                    railsByCandidateFace,
                    candidate,
                    candidate.FaceB,
                    railTolerance,
                    minimumStableEdgeLength,
                    ref stats);
            }

            if (stats.FaceClipFailedFaceCount != 0 ||
                stats.MissingRailCount != 0 ||
                stats.FragmentedRailCount != 0 ||
                stats.ShortRailCount != 0 ||
                stats.ExtractedRailCount != stats.ExpectedRailCount)
            {
                return false;
            }

            if (!TryBuildSampledProfileGrids(
                    selectedEdges,
                    railsByCandidateFace,
                    bevelDepth,
                    minimumStableEdgeLength,
                    out Dictionary<int, EdgeWearProfileGrid> profileGridsByCandidate,
                    ref stats))
            {
                return false;
            }

            return TryBuildClippedBaseFaceWorkspace(
                sourceFaces,
                graph,
                clippedPolygonsByFace,
                profileGridsByCandidate,
                localBaseFaceMinArea,
                localBaseMinEdgeLength,
                minimumStableEdgeLength,
                ref stats);
        }

        private static bool TryPreflightRailForCandidateFace(
            Dictionary<int, List<Vector3>> clippedPolygonsByFace,
            Dictionary<long, FaceInsetCut> cutsByCandidateFace,
            Dictionary<long, BevelRail> railsByCandidateFace,
            EdgeWearBevelCandidate candidate,
            int faceIndex,
            float railTolerance,
            float minimumStableEdgeLength,
            ref EdgeWearGraphBuildStats stats)
        {
            long key = MakeCandidateFaceKey(candidate.CandidateIndex, faceIndex);
            if (!clippedPolygonsByFace.TryGetValue(faceIndex, out List<Vector3> polygon) ||
                !cutsByCandidateFace.TryGetValue(
                    key,
                    out FaceInsetCut cut))
            {
                stats.MissingRailCount++;
                return false;
            }

            if (!TryExtractCutRail(
                    polygon,
                    cut,
                    railTolerance,
                    minimumStableEdgeLength,
                    out BevelRail rail,
                    out bool fragmented))
            {
                if (fragmented)
                {
                    stats.FragmentedRailCount++;
                }
                else
                {
                    stats.MissingRailCount++;
                }

                return false;
            }

            float minimumLength = Mathf.Max(
                minimumStableEdgeLength,
                railTolerance * 0.45f);
            if ((rail.End - rail.Start).magnitude <= minimumLength)
            {
                stats.ShortRailCount++;
                return false;
            }

            railsByCandidateFace[key] = rail;
            stats.ExtractedRailCount++;
            return true;
        }

        private static bool TryBuildSampledProfileGrids(
            List<EdgeWearSelectedGraphEdge> selectedEdges,
            Dictionary<long, BevelRail> railsByCandidateFace,
            float bevelDepth,
            float minimumStableEdgeLength,
            out Dictionary<int, EdgeWearProfileGrid> profileGridsByCandidate,
            ref EdgeWearGraphBuildStats stats)
        {
            profileGridsByCandidate = new Dictionary<int, EdgeWearProfileGrid>(
                selectedEdges.Count);

            if (selectedEdges.Count == 0)
            {
                return false;
            }

            const int profileSegments = 3;
            stats.ProfileSegmentCount = profileSegments;
            stats.ProfileMinSampleCount = int.MaxValue;

            for (int edgeIndex = 0; edgeIndex < selectedEdges.Count; edgeIndex++)
            {
                EdgeWearBevelCandidate candidate = selectedEdges[edgeIndex].Candidate;
                if (!railsByCandidateFace.TryGetValue(
                        MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceA),
                        out BevelRail railA) ||
                    !railsByCandidateFace.TryGetValue(
                        MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceB),
                        out BevelRail railB))
                {
                    stats.ProfileEdgesFailedCount++;
                    continue;
                }

                float edgeLength = (candidate.End - candidate.Start).magnitude;
                float railALength = (railA.End - railA.Start).magnitude;
                float railBLength = (railB.End - railB.Start).magnitude;
                float minimumProfileWidth = Mathf.Max(
                    minimumStableEdgeLength * 0.18f,
                    bevelDepth * 0.08f);

                if (edgeLength <= minimumStableEdgeLength ||
                    railALength <= minimumStableEdgeLength ||
                    railBLength <= minimumStableEdgeLength)
                {
                    stats.ProfileEdgesFailedCount++;
                    continue;
                }

                int sampleCount = CalculateEdgeWearProfileSampleCount(
                    edgeLength,
                    bevelDepth,
                    minimumStableEdgeLength);
                Vector3[,] points = new Vector3[sampleCount, profileSegments + 1];
                int edgeGridPointCount = 0;
                bool failed = false;

                for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
                {
                    float t = sampleCount <= 1
                        ? 0f
                        : sampleIndex / (float)(sampleCount - 1);
                    Vector3 edgePoint = Vector3.Lerp(candidate.Start, candidate.End, t);
                    Vector3 railPointA = Vector3.Lerp(railA.Start, railA.End, t);
                    Vector3 railPointB = Vector3.Lerp(railB.Start, railB.End, t);
                    float profileWidth = (railPointB - railPointA).magnitude;
                    if (profileWidth <= minimumProfileWidth)
                    {
                        stats.ProfileZeroWidthCount++;
                        failed = true;
                        continue;
                    }

                    for (int segmentIndex = 0; segmentIndex <= profileSegments; segmentIndex++)
                    {
                        float u = segmentIndex / (float)profileSegments;
                        Vector3 point = EvaluateEdgeWearProfilePoint(
                            railPointA,
                            edgePoint,
                            railPointB,
                            u);
                        if (!IsFinite(point))
                        {
                            stats.ProfileInvalidPointCount++;
                            failed = true;
                            continue;
                        }

                        points[sampleIndex, segmentIndex] = point;
                        edgeGridPointCount++;
                    }
                }

                if (failed)
                {
                    stats.ProfileEdgesFailedCount++;
                    continue;
                }

                profileGridsByCandidate[candidate.CandidateIndex] =
                    new EdgeWearProfileGrid(
                        candidate,
                        railA,
                        railB,
                        sampleCount,
                        profileSegments,
                        points);

                stats.ProfileEdgesPreparedCount++;
                stats.ProfileSampleCount += sampleCount;
                stats.ProfileGridPointCount += edgeGridPointCount;
                stats.ProfileMinSampleCount = Mathf.Min(
                    stats.ProfileMinSampleCount,
                    sampleCount);
                stats.ProfileMaxSampleCount = Mathf.Max(
                    stats.ProfileMaxSampleCount,
                    sampleCount);
            }

            if (stats.ProfileMinSampleCount == int.MaxValue)
            {
                stats.ProfileMinSampleCount = 0;
            }

            return profileGridsByCandidate.Count == selectedEdges.Count &&
                stats.ProfileEdgesPreparedCount == selectedEdges.Count &&
                stats.ProfileEdgesFailedCount == 0 &&
                stats.ProfileInvalidPointCount == 0 &&
                stats.ProfileZeroWidthCount == 0 &&
                stats.ProfileGridPointCount > 0;
        }

        private static bool TryBuildClippedBaseFaceWorkspace(
            List<PolygonFace> sourceFaces,
            EdgeWearTopologyGraph graph,
            Dictionary<int, List<Vector3>> clippedPolygonsByFace,
            Dictionary<int, EdgeWearProfileGrid> profileGridsByCandidate,
            float localBaseFaceMinArea,
            float localBaseMinEdgeLength,
            float minimumStableEdgeLength,
            ref EdgeWearGraphBuildStats stats)
        {
            EdgeWearTopologyRebuildWorkspace workspace =
                new EdgeWearTopologyRebuildWorkspace();
            Dictionary<int, List<EdgeWearFaceRailSamples>> railSamplesByFace =
                BuildEdgeWearFaceRailSamples(profileGridsByCandidate);
            float railSampleTolerance = Mathf.Max(
                PlaneEpsilon * 20f,
                minimumStableEdgeLength * 0.9f);
            float railSampleMinEdgeLength = Mathf.Max(
                PlaneEpsilon * 6f,
                minimumStableEdgeLength * 0.05f);

            for (int faceIndex = 0; faceIndex < sourceFaces.Count; faceIndex++)
            {
                PolygonFace sourceFace = sourceFaces[faceIndex];
                if (sourceFace.Feature != PolygonFaceFeature.Base)
                {
                    workspace.RebuiltFaces.Add(
                        new PolygonFace(
                            new List<Vector3>(sourceFace.Vertices),
                            sourceFace.Normal,
                            sourceFace.Feature,
                            sourceFace.FeatureStrength));
                    continue;
                }

                if (clippedPolygonsByFace.TryGetValue(
                        faceIndex,
                        out List<Vector3> clippedPolygon))
                {
                    stats.AffectedBaseFaceCount++;
                    List<Vector3> clean = SanitizePolygon(
                        new List<Vector3>(clippedPolygon),
                        sourceFace.Normal);

                    if (!ValidateLocalEdgeWearFace(
                            clean,
                            localBaseFaceMinArea,
                            localBaseMinEdgeLength))
                    {
                        stats.BaseFaceValidationFailureCount++;
                        continue;
                    }

                    if (!railSamplesByFace.TryGetValue(
                            faceIndex,
                            out List<EdgeWearFaceRailSamples> faceRailSamples) ||
                        faceRailSamples.Count == 0)
                    {
                        stats.RailSampleInsertionFailureCount++;
                        continue;
                    }

                    if (!TryInsertRailSamplesIntoBaseFace(
                            clean,
                            faceRailSamples,
                            railSampleTolerance,
                            out List<Vector3> sampledBaseFace,
                            ref stats))
                    {
                        stats.BaseFaceValidationFailureCount++;
                        continue;
                    }

                    if (!ValidateRailSampledBaseFace(
                            sampledBaseFace,
                            localBaseFaceMinArea,
                            railSampleMinEdgeLength))
                    {
                        stats.BaseFaceValidationFailureCount++;
                        continue;
                    }

                    PolygonFace clippedFace = new PolygonFace(
                        sampledBaseFace,
                        sourceFace.Normal,
                        PolygonFaceFeature.Base,
                        sourceFace.FeatureStrength);
                    workspace.AffectedSourceFaces.Add(faceIndex);
                    workspace.ClippedBaseFaceBySourceFace[faceIndex] = clippedFace;
                    workspace.RebuiltFaces.Add(clippedFace);
                    stats.ReplacedBaseFaceCount++;
                    stats.RailSampledBaseFaceCount++;
                    continue;
                }

                workspace.RebuiltFaces.Add(
                    new PolygonFace(
                        new List<Vector3>(sourceFace.Vertices),
                        sourceFace.Normal,
                        sourceFace.Feature,
                        sourceFace.FeatureStrength));
                stats.PreservedBaseFaceCount++;
            }

            stats.WorkspaceFaceCount = workspace.RebuiltFaces.Count;
            stats.WorkspaceBaseFaceCount = CountBaseFaces(workspace.RebuiltFaces);

            EdgeWearTopologyStats workspaceTopology = AuditEdgeWearTopology(
                workspace.RebuiltFaces,
                minimumStableEdgeLength);
            stats.WorkspaceTemporaryOpenEdgeCount = workspaceTopology.OpenEdgeCount;

            bool baseWorkspaceValid = stats.BaseFaceValidationFailureCount == 0 &&
                stats.RailSampleInsertionFailureCount == 0 &&
                stats.AffectedBaseFaceCount == stats.FaceClipAffectedFaceCount &&
                stats.ReplacedBaseFaceCount == stats.FaceClipSucceededFaceCount &&
                stats.WorkspaceBaseFaceCount >= graph.Faces.Count;
            if (!baseWorkspaceValid)
            {
                return false;
            }

            if (!TryAppendEdgeWearRibbonFaces(
                    workspace,
                    profileGridsByCandidate,
                    localBaseFaceMinArea,
                    railSampleMinEdgeLength,
                    minimumStableEdgeLength,
                    ref stats))
            {
                return false;
            }

            return true;
        }

        private static Dictionary<int, List<EdgeWearFaceRailSamples>> BuildEdgeWearFaceRailSamples(
            Dictionary<int, EdgeWearProfileGrid> profileGridsByCandidate)
        {
            Dictionary<int, List<EdgeWearFaceRailSamples>> samplesByFace =
                new Dictionary<int, List<EdgeWearFaceRailSamples>>();

            foreach (EdgeWearProfileGrid grid in profileGridsByCandidate.Values)
            {
                AddEdgeWearFaceRailSamples(
                    samplesByFace,
                    grid.Candidate.FaceA,
                    grid.Candidate.CandidateIndex,
                    grid.RailA,
                    grid.GetBoundarySamples(0));
                AddEdgeWearFaceRailSamples(
                    samplesByFace,
                    grid.Candidate.FaceB,
                    grid.Candidate.CandidateIndex,
                    grid.RailB,
                    grid.GetBoundarySamples(grid.ProfileSegments));
            }

            return samplesByFace;
        }

        private static void AddEdgeWearFaceRailSamples(
            Dictionary<int, List<EdgeWearFaceRailSamples>> samplesByFace,
            int faceIndex,
            int candidateIndex,
            BevelRail rail,
            Vector3[] samples)
        {
            if (!samplesByFace.TryGetValue(
                    faceIndex,
                    out List<EdgeWearFaceRailSamples> faceSamples))
            {
                faceSamples = new List<EdgeWearFaceRailSamples>();
                samplesByFace.Add(faceIndex, faceSamples);
            }

            faceSamples.Add(
                new EdgeWearFaceRailSamples(
                    faceIndex,
                    candidateIndex,
                    rail,
                    samples));
        }

        private static bool TryInsertRailSamplesIntoBaseFace(
            List<Vector3> polygon,
            List<EdgeWearFaceRailSamples> faceRailSamples,
            float tolerance,
            out List<Vector3> sampledPolygon,
            ref EdgeWearGraphBuildStats stats)
        {
            sampledPolygon = new List<Vector3>();
            if (polygon.Count < 3 || faceRailSamples.Count == 0)
            {
                stats.RailSampleInsertionFailureCount++;
                return false;
            }

            bool[] matchedRails = new bool[faceRailSamples.Count];
            for (int vertexIndex = 0; vertexIndex < polygon.Count; vertexIndex++)
            {
                Vector3 start = polygon[vertexIndex];
                Vector3 end = polygon[(vertexIndex + 1) % polygon.Count];
                AddPointIfDifferent(sampledPolygon, start);

                int railIndex = FindMatchingRailSampleSegment(
                    faceRailSamples,
                    matchedRails,
                    start,
                    end,
                    tolerance,
                    out bool reversed);
                if (railIndex < 0)
                {
                    continue;
                }

                EdgeWearFaceRailSamples railSamples = faceRailSamples[railIndex];
                matchedRails[railIndex] = true;
                stats.RailSampledBoundarySegmentCount++;
                stats.RailSamplesPreservedCount += railSamples.Samples.Length;

                if (!reversed)
                {
                    for (int sampleIndex = 1; sampleIndex < railSamples.Samples.Length - 1; sampleIndex++)
                    {
                        AddPointIfDifferent(sampledPolygon, railSamples.Samples[sampleIndex]);
                        stats.RailSamplesInsertedCount++;
                    }
                }
                else
                {
                    for (int sampleIndex = railSamples.Samples.Length - 2; sampleIndex >= 1; sampleIndex--)
                    {
                        AddPointIfDifferent(sampledPolygon, railSamples.Samples[sampleIndex]);
                        stats.RailSamplesInsertedCount++;
                    }
                }
            }

            RemoveClosingDuplicate(sampledPolygon);

            int matchedCount = 0;
            for (int i = 0; i < matchedRails.Length; i++)
            {
                if (matchedRails[i])
                {
                    matchedCount++;
                }
            }

            if (matchedCount != faceRailSamples.Count)
            {
                stats.RailSampleInsertionFailureCount += faceRailSamples.Count - matchedCount;
                return false;
            }

            return sampledPolygon.Count >= polygon.Count;
        }

        private static int FindMatchingRailSampleSegment(
            List<EdgeWearFaceRailSamples> faceRailSamples,
            bool[] matchedRails,
            Vector3 segmentStart,
            Vector3 segmentEnd,
            float tolerance,
            out bool reversed)
        {
            reversed = false;
            for (int i = 0; i < faceRailSamples.Count; i++)
            {
                if (matchedRails[i])
                {
                    continue;
                }

                EdgeWearFaceRailSamples samples = faceRailSamples[i];
                if (AreWithinDistance(segmentStart, samples.Start, tolerance) &&
                    AreWithinDistance(segmentEnd, samples.End, tolerance))
                {
                    reversed = false;
                    return i;
                }

                if (AreWithinDistance(segmentStart, samples.End, tolerance) &&
                    AreWithinDistance(segmentEnd, samples.Start, tolerance))
                {
                    reversed = true;
                    return i;
                }
            }

            return -1;
        }

        private static bool TryAppendEdgeWearRibbonFaces(
            EdgeWearTopologyRebuildWorkspace workspace,
            Dictionary<int, EdgeWearProfileGrid> profileGridsByCandidate,
            float minimumStableFaceArea,
            float minimumStableEdgeLength,
            float topologyMinimumStableEdgeLength,
            ref EdgeWearGraphBuildStats stats)
        {
            foreach (EdgeWearProfileGrid grid in profileGridsByCandidate.Values)
            {
                int expectedForEdge = Mathf.Max(0, grid.SampleCount - 1) * grid.ProfileSegments;
                stats.RibbonFacesExpectedCount += expectedForEdge;

                if (grid.SampleCount < 2 || grid.ProfileSegments < 1)
                {
                    stats.RibbonEdgesFailedCount++;
                    continue;
                }

                int builtForEdge = 0;
                bool failed = false;
                for (int sampleIndex = 0; sampleIndex < grid.SampleCount - 1; sampleIndex++)
                {
                    for (int profileIndex = 0; profileIndex < grid.ProfileSegments; profileIndex++)
                    {
                        Vector3 p00 = grid.Points[sampleIndex, profileIndex];
                        Vector3 p10 = grid.Points[sampleIndex + 1, profileIndex];
                        Vector3 p11 = grid.Points[sampleIndex + 1, profileIndex + 1];
                        Vector3 p01 = grid.Points[sampleIndex, profileIndex + 1];

                        if (HasDegenerateQuad(p00, p10, p11, p01, minimumStableEdgeLength))
                        {
                            stats.RibbonDegenerateFaceCount++;
                            failed = true;
                            continue;
                        }

                        Vector3 normal = Vector3.Cross(p10 - p00, p01 - p00);
                        if (normal.sqrMagnitude <= MinimumEdgeLengthSqr)
                        {
                            normal = grid.Candidate.BevelNormal;
                        }
                        else
                        {
                            normal.Normalize();
                            if (Vector3.Dot(normal, grid.Candidate.BevelNormal) < 0f)
                            {
                                normal = -normal;
                            }
                        }

                        PolygonFace ribbonFace = CreateOrientedFace(
                            normal,
                            PolygonFaceFeature.ConvexEdgeWear,
                            grid.Candidate.Strength,
                            p00,
                            p10,
                            p11,
                            p01);

                        if (!ValidateLocalEdgeWearFace(
                                ribbonFace.Vertices,
                                minimumStableFaceArea * 0.08f,
                                minimumStableEdgeLength))
                        {
                            stats.RibbonInvalidFaceCount++;
                            failed = true;
                            continue;
                        }

                        workspace.RebuiltFaces.Add(ribbonFace);
                        stats.RibbonFacesBuiltCount++;
                        builtForEdge++;
                    }
                }

                if (failed || builtForEdge != expectedForEdge)
                {
                    stats.RibbonEdgesFailedCount++;
                }
                else
                {
                    stats.RibbonEdgesPreparedCount++;
                }
            }

            stats.WorkspaceConvexEdgeWearFaceCount = CountFeatureFaces(
                workspace.RebuiltFaces,
                PolygonFaceFeature.ConvexEdgeWear);
            stats.WorkspaceFacesAfterRibbonsCount = workspace.RebuiltFaces.Count;

            EdgeWearTopologyStats ribbonTopology = AuditEdgeWearTopology(
                workspace.RebuiltFaces,
                topologyMinimumStableEdgeLength);
            stats.WorkspaceOpenEdgesAfterRibbonsCount = ribbonTopology.OpenEdgeCount;

            return stats.RibbonEdgesPreparedCount == profileGridsByCandidate.Count &&
                stats.RibbonEdgesFailedCount == 0 &&
                stats.RibbonFacesBuiltCount == stats.RibbonFacesExpectedCount &&
                stats.RibbonDegenerateFaceCount == 0 &&
                stats.RibbonInvalidFaceCount == 0 &&
                stats.WorkspaceConvexEdgeWearFaceCount > 0;
        }

        private static bool ValidateRailSampledBaseFace(
            List<Vector3> vertices,
            float minimumStableFaceArea,
            float minimumStableEdgeLength)
        {
            if (vertices.Count < 3 ||
                CalculatePolygonArea(vertices) <= minimumStableFaceArea)
            {
                return false;
            }

            float minimumEdgeLengthSqr = minimumStableEdgeLength * minimumStableEdgeLength;
            for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
            {
                Vector3 start = vertices[vertexIndex];
                Vector3 end = vertices[(vertexIndex + 1) % vertices.Count];
                if (!IsFinite(start) ||
                    !IsFinite(end) ||
                    (end - start).sqrMagnitude <= minimumEdgeLengthSqr)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasDegenerateQuad(
            Vector3 p00,
            Vector3 p10,
            Vector3 p11,
            Vector3 p01,
            float minimumStableEdgeLength)
        {
            float minimumEdgeLengthSqr = minimumStableEdgeLength * minimumStableEdgeLength;
            return
                !IsFinite(p00) ||
                !IsFinite(p10) ||
                !IsFinite(p11) ||
                !IsFinite(p01) ||
                (p10 - p00).sqrMagnitude <= minimumEdgeLengthSqr ||
                (p11 - p10).sqrMagnitude <= minimumEdgeLengthSqr ||
                (p01 - p11).sqrMagnitude <= minimumEdgeLengthSqr ||
                (p00 - p01).sqrMagnitude <= minimumEdgeLengthSqr;
        }

        private static bool AreWithinDistance(
            Vector3 left,
            Vector3 right,
            float distance)
        {
            return (left - right).sqrMagnitude <= distance * distance;
        }

        private static int CountBaseFaces(List<PolygonFace> faces)
        {
            int count = 0;
            for (int i = 0; i < faces.Count; i++)
            {
                if (faces[i].Feature == PolygonFaceFeature.Base)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CalculateEdgeWearProfileSampleCount(
            float edgeLength,
            float bevelDepth,
            float minimumStableEdgeLength)
        {
            float spacing = Mathf.Max(
                minimumStableEdgeLength * 5.0f,
                Mathf.Max(bevelDepth * 1.35f, PlaneEpsilon * 12f));
            int sampleCount = Mathf.CeilToInt(edgeLength / Mathf.Max(PlaneEpsilon, spacing)) + 1;
            return Mathf.Clamp(sampleCount, 2, 8);
        }

        private static Vector3 EvaluateEdgeWearProfilePoint(
            Vector3 railPointA,
            Vector3 edgePoint,
            Vector3 railPointB,
            float u)
        {
            u = Mathf.Clamp01(u);
            float inverse = 1f - u;
            return
                inverse * inverse * railPointA +
                2f * inverse * u * edgePoint +
                u * u * railPointB;
        }

        private static int GetOrAddEdgeWearGraphVertex(
            EdgeWearTopologyGraph graph,
            Vector3 position)
        {
            VertexKey key = new VertexKey(position);
            if (graph.VertexByKey.TryGetValue(key, out int index))
            {
                return index;
            }

            index = graph.Vertices.Count;
            graph.VertexByKey.Add(key, index);
            graph.Vertices.Add(new EdgeWearGraphVertex(position, key));
            return index;
        }

        private static int GetOrAddEdgeWearGraphEdge(
            EdgeWearTopologyGraph graph,
            int vertexA,
            int vertexB,
            Vector3 start,
            Vector3 end)
        {
            EdgeKey key = new EdgeKey(start, end);
            if (graph.EdgeByKey.TryGetValue(key, out int index))
            {
                return index;
            }

            index = graph.Edges.Count;
            graph.EdgeByKey.Add(key, index);
            graph.Edges.Add(new EdgeWearGraphEdge(vertexA, vertexB));
            graph.Vertices[vertexA].AddEdge(index);
            graph.Vertices[vertexB].AddEdge(index);
            return index;
        }

        private static int GetUniqueGraphVertexCount(List<int> vertexIndices)
        {
            HashSet<int> unique = new HashSet<int>();
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                unique.Add(vertexIndices[i]);
            }

            return unique.Count;
        }

        private static bool GraphEdgeMatchesCandidateFaces(
            EdgeWearGraphEdge edge,
            EdgeWearBevelCandidate candidate)
        {
            return
                (edge.FaceA == candidate.FaceA && edge.FaceB == candidate.FaceB) ||
                (edge.FaceA == candidate.FaceB && edge.FaceB == candidate.FaceA);
        }

        private static int CountGraphNonManifoldEdges(EdgeWearTopologyGraph graph)
        {
            int count = 0;
            for (int i = 0; i < graph.Edges.Count; i++)
            {
                if (graph.Edges[i].ExtraFaceCount > 0)
                {
                    count++;
                }
            }

            return count;
        }


        private static bool TryApplyHalfSpaceEdgeWearBevels(
            List<PolygonFace> faces,
            List<EdgeWearBevelCandidate> candidates,
            int selectedCount,
            float bevelDepth,
            Bounds bounds,
            float minimumStableFaceArea,
            float minimumStableEdgeLength,
            ref EdgeWearBevelBuildStats stats)
        {
            if (selectedCount <= 0)
            {
                return false;
            }

            List<EdgeWearBevelCandidate> selected =
                new List<EdgeWearBevelCandidate>(selectedCount);
            for (int i = 0; i < selectedCount; i++)
            {
                selected.Add(candidates[i]);
            }

            EdgeWearTopologyStats sourceTopologyStats = AuditEdgeWearTopology(
                faces,
                minimumStableEdgeLength);

            // EW-4C.0: candidates are no longer individually sacrificed to
            // repair topology. All authored selected candidates become bevel
            // support planes; if the requested width cannot fit, the whole
            // profile narrows and solves again from the same candidate set.
            float[] depthScales =
            {
                1.0f,
                0.85f,
                0.70f,
                0.55f,
                0.40f,
                0.30f,
                0.22f
            };

            for (int scaleIndex = 0; scaleIndex < depthScales.Length; scaleIndex++)
            {
                float depthScale = depthScales[scaleIndex];
                if (!TryBuildHalfSpaceEdgeWearSupportPlanes(
                        faces,
                        selected,
                        bevelDepth * depthScale,
                        out List<EdgeWearSupportPlane> supportPlanes,
                        out int bevelPlaneCount,
                        out EdgeWearBevelRejectReason supportRejectReason))
                {
                    stats.RegisterReject(supportRejectReason);
                    return false;
                }

                if (!TryRebuildConvexPolyhedronFromSupportPlanes(
                        supportPlanes,
                        minimumStableFaceArea,
                        minimumStableEdgeLength,
                        out List<PolygonFace> rebuiltFaces,
                        out int producedBevelFaceCount))
                {
                    stats.RegisterReject(EdgeWearBevelRejectReason.ValidationGlobal);
                    continue;
                }

                if (producedBevelFaceCount < selected.Count)
                {
                    stats.RegisterReject(EdgeWearBevelRejectReason.ValidationBevelFace);
                    continue;
                }

                EdgeWearTopologyStats topologyStats = AuditEdgeWearTopology(
                    rebuiltFaces,
                    minimumStableEdgeLength);

                if (topologyStats.OpenEdgeCount > sourceTopologyStats.OpenEdgeCount)
                {
                    stats.RegisterReject(EdgeWearBevelRejectReason.ValidationOpenEdge);
                    continue;
                }

                if (topologyStats.NonManifoldEdgeCount >
                    sourceTopologyStats.NonManifoldEdgeCount)
                {
                    stats.RegisterReject(EdgeWearBevelRejectReason.ValidationNonManifoldEdge);
                    continue;
                }

                if (topologyStats.TJunctionCount > sourceTopologyStats.TJunctionCount)
                {
                    stats.RegisterReject(EdgeWearBevelRejectReason.ValidationTJunction);
                    continue;
                }

                faces.Clear();
                faces.AddRange(rebuiltFaces);

                stats.AcceptedCount = selected.Count;
                stats.ConstructedBevelPlaneCount = bevelPlaneCount;
                stats.ProducedBevelFaceCount = producedBevelFaceCount;
                stats.BevelDepthScalePercent = Mathf.RoundToInt(depthScale * 100f);
                stats.TopologyOpenEdges = topologyStats.OpenEdgeCount;
                stats.TopologyNonManifoldEdges = topologyStats.NonManifoldEdgeCount;
                stats.TopologyTJunctions = topologyStats.TJunctionCount;
                return true;
            }

            return false;
        }

        private static bool TryBuildHalfSpaceEdgeWearSupportPlanes(
            List<PolygonFace> sourceFaces,
            List<EdgeWearBevelCandidate> selected,
            float bevelDepth,
            out List<EdgeWearSupportPlane> supportPlanes,
            out int bevelPlaneCount,
            out EdgeWearBevelRejectReason rejectReason)
        {
            supportPlanes = new List<EdgeWearSupportPlane>(
                sourceFaces.Count + selected.Count);
            bevelPlaneCount = 0;
            rejectReason = EdgeWearBevelRejectReason.None;

            Vector3 interiorPoint = CalculatePolyhedronInteriorPoint(sourceFaces);

            for (int faceIndex = 0; faceIndex < sourceFaces.Count; faceIndex++)
            {
                PolygonFace face = sourceFaces[faceIndex];
                if (face.Vertices.Count < 3)
                {
                    rejectReason = EdgeWearBevelRejectReason.ValidationBaseFace;
                    return false;
                }

                float distance = AveragePlaneDistance(
                    face.Normal,
                    face.Vertices);
                supportPlanes.Add(
                    new EdgeWearSupportPlane(
                        face.Normal,
                        distance,
                        face.Feature,
                        face.FeatureStrength,
                        -1));
            }

            for (int i = 0; i < selected.Count; i++)
            {
                EdgeWearBevelCandidate candidate = selected[i];
                if (!TryBuildHalfSpaceBevelPlane(
                        candidate,
                        sourceFaces,
                        bevelDepth * candidate.DepthMultiplier,
                        interiorPoint,
                        out EdgeWearSupportPlane bevelPlane))
                {
                    rejectReason = EdgeWearBevelRejectReason.InsetCut;
                    return false;
                }

                supportPlanes.Add(bevelPlane);
                bevelPlaneCount++;
            }

            return true;
        }

        private static bool TryBuildHalfSpaceBevelPlane(
            EdgeWearBevelCandidate candidate,
            List<PolygonFace> sourceFaces,
            float candidateDepth,
            Vector3 interiorPoint,
            out EdgeWearSupportPlane bevelPlane)
        {
            bevelPlane = default;

            if (!TryBuildFaceInsetCut(
                    candidate,
                    candidate.FaceA,
                    sourceFaces[candidate.FaceA],
                    candidateDepth,
                    out FaceInsetCut cutA) ||
                !TryBuildFaceInsetCut(
                    candidate,
                    candidate.FaceB,
                    sourceFaces[candidate.FaceB],
                    candidateDepth,
                    out FaceInsetCut cutB))
            {
                return false;
            }

            Vector3 edge = candidate.End - candidate.Start;
            float edgeLength = edge.magnitude;
            if (edgeLength <= PlaneEpsilon)
            {
                return false;
            }

            Vector3 edgeDirection = edge / edgeLength;
            float insetAStart = Mathf.Abs(cutA.Plane.SignedDistance(candidate.Start));
            float insetAEnd = Mathf.Abs(cutA.Plane.SignedDistance(candidate.End));
            float insetBStart = Mathf.Abs(cutB.Plane.SignedDistance(candidate.Start));
            float insetBEnd = Mathf.Abs(cutB.Plane.SignedDistance(candidate.End));

            Vector3 railAStart = candidate.Start + cutA.Inward * insetAStart;
            Vector3 railAEnd = candidate.End + cutA.Inward * insetAEnd;
            Vector3 railBStart = candidate.Start + cutB.Inward * insetBStart;
            Vector3 railBEnd = candidate.End + cutB.Inward * insetBEnd;

            Vector3 across = ((railBStart - railAStart) +
                (railBEnd - railAEnd)) * 0.5f;
            if (across.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            Vector3 normal = Vector3.Cross(edgeDirection, across);
            if (normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                normal = Vector3.Cross(edgeDirection, railBEnd - railAStart);
            }

            if (normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            normal.Normalize();
            if (Vector3.Dot(normal, candidate.BevelNormal) < 0f)
            {
                normal = -normal;
            }

            float distance =
                (Vector3.Dot(normal, railAStart) +
                 Vector3.Dot(normal, railAEnd) +
                 Vector3.Dot(normal, railBStart) +
                 Vector3.Dot(normal, railBEnd)) * 0.25f;

            float edgeDistance = Vector3.Dot(normal, candidate.Midpoint);
            if (edgeDistance <= distance + PlaneEpsilon)
            {
                distance = edgeDistance - Mathf.Max(
                    PlaneEpsilon * 12f,
                    candidateDepth * 0.15f);
            }

            if (Vector3.Dot(normal, interiorPoint) > distance - PlaneEpsilon)
            {
                return false;
            }

            bevelPlane = new EdgeWearSupportPlane(
                normal,
                distance,
                PolygonFaceFeature.ConvexEdgeWear,
                candidate.Strength,
                candidate.CandidateIndex);
            return true;
        }

        private static bool TryRebuildConvexPolyhedronFromSupportPlanes(
            List<EdgeWearSupportPlane> supportPlanes,
            float minimumStableFaceArea,
            float minimumStableEdgeLength,
            out List<PolygonFace> rebuiltFaces,
            out int producedBevelFaceCount)
        {
            rebuiltFaces = null;
            producedBevelFaceCount = 0;

            if (supportPlanes.Count < 4)
            {
                return false;
            }

            float insideTolerance = Mathf.Max(
                PlaneEpsilon * 12f,
                minimumStableEdgeLength * 0.45f);
            Dictionary<VertexKey, Vector3> uniqueVertices =
                new Dictionary<VertexKey, Vector3>();

            for (int i = 0; i < supportPlanes.Count - 2; i++)
            {
                for (int j = i + 1; j < supportPlanes.Count - 1; j++)
                {
                    for (int k = j + 1; k < supportPlanes.Count; k++)
                    {
                        if (!TryIntersectSupportPlanes(
                                supportPlanes[i],
                                supportPlanes[j],
                                supportPlanes[k],
                                out Vector3 point))
                        {
                            continue;
                        }

                        if (!IsInsideSupportPlanes(
                                point,
                                supportPlanes,
                                insideTolerance))
                        {
                            continue;
                        }

                        VertexKey key = new VertexKey(point);
                        if (!uniqueVertices.ContainsKey(key))
                        {
                            uniqueVertices.Add(key, point);
                        }
                    }
                }
            }

            if (uniqueVertices.Count < 4)
            {
                return false;
            }

            List<Vector3> vertices = new List<Vector3>(uniqueVertices.Values);
            List<PolygonFace> faces =
                new List<PolygonFace>(supportPlanes.Count);
            float faceTolerance = Mathf.Max(
                PlaneEpsilon * 16f,
                minimumStableEdgeLength * 0.55f);

            for (int planeIndex = 0;
                 planeIndex < supportPlanes.Count;
                 planeIndex++)
            {
                EdgeWearSupportPlane plane = supportPlanes[planeIndex];
                List<Vector3> faceVertices = new List<Vector3>();
                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    float distance = Mathf.Abs(
                        Vector3.Dot(plane.Normal, vertices[vertexIndex]) -
                        plane.Distance);
                    if (distance <= faceTolerance)
                    {
                        faceVertices.Add(vertices[vertexIndex]);
                    }
                }

                faceVertices = GetUniquePoints(faceVertices);
                if (faceVertices.Count < 3)
                {
                    continue;
                }

                PolygonFace oriented = CreateOrientedFace(
                    plane.Normal,
                    plane.Feature,
                    plane.FeatureStrength,
                    faceVertices.ToArray());
                List<Vector3> clean = SanitizePolygon(
                    oriented.Vertices,
                    oriented.Normal);
                if (!ValidateLocalEdgeWearFace(
                        clean,
                        minimumStableFaceArea,
                        minimumStableEdgeLength * 0.25f))
                {
                    continue;
                }

                faces.Add(
                    new PolygonFace(
                        clean,
                        plane.Normal,
                        plane.Feature,
                        plane.FeatureStrength));
                if (plane.Feature == PolygonFaceFeature.ConvexEdgeWear)
                {
                    producedBevelFaceCount++;
                }
            }

            if (faces.Count < 4 || producedBevelFaceCount <= 0)
            {
                return false;
            }

            WeldSharedVertices(faces);
            SanitizeAllFaces(faces);
            producedBevelFaceCount = CountFeatureFaces(
                faces,
                PolygonFaceFeature.ConvexEdgeWear);
            if (!ValidatePolyhedronFaces(
                    faces,
                    minimumStableFaceArea,
                    minimumStableEdgeLength * 0.20f))
            {
                return false;
            }

            rebuiltFaces = faces;
            return true;
        }

        private static bool TryIntersectSupportPlanes(
            EdgeWearSupportPlane first,
            EdgeWearSupportPlane second,
            EdgeWearSupportPlane third,
            out Vector3 point)
        {
            Vector3 n1 = first.Normal;
            Vector3 n2 = second.Normal;
            Vector3 n3 = third.Normal;
            Vector3 n2CrossN3 = Vector3.Cross(n2, n3);
            float denominator = Vector3.Dot(n1, n2CrossN3);

            if (Mathf.Abs(denominator) <= PlaneEpsilon * 0.05f)
            {
                point = Vector3.zero;
                return false;
            }

            point =
                (first.Distance * n2CrossN3 +
                 second.Distance * Vector3.Cross(n3, n1) +
                 third.Distance * Vector3.Cross(n1, n2)) /
                denominator;
            return IsFinite(point);
        }

        private static bool IsInsideSupportPlanes(
            Vector3 point,
            List<EdgeWearSupportPlane> supportPlanes,
            float tolerance)
        {
            for (int i = 0; i < supportPlanes.Count; i++)
            {
                if (Vector3.Dot(supportPlanes[i].Normal, point) -
                    supportPlanes[i].Distance > tolerance)
                {
                    return false;
                }
            }

            return true;
        }

        private static Vector3 CalculatePolyhedronInteriorPoint(
            List<PolygonFace> faces)
        {
            List<Vector3> points = new List<Vector3>();
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    points.Add(face.Vertices[vertexIndex]);
                }
            }

            return points.Count > 0
                ? CalculateAverage(points)
                : Vector3.zero;
        }

        private static float AveragePlaneDistance(
            Vector3 normal,
            List<Vector3> vertices)
        {
            float total = 0f;
            for (int i = 0; i < vertices.Count; i++)
            {
                total += Vector3.Dot(normal, vertices[i]);
            }

            return total / Mathf.Max(1, vertices.Count);
        }

        private static bool IsFinite(Vector3 value)
        {
            return !(float.IsNaN(value.x) ||
                float.IsNaN(value.y) ||
                float.IsNaN(value.z) ||
                float.IsInfinity(value.x) ||
                float.IsInfinity(value.y) ||
                float.IsInfinity(value.z));
        }

        private static bool TryApplyLocalEdgeWearBevels(
            List<PolygonFace> faces,
            List<EdgeWearBevelCandidate> candidates,
            int selectedCount,
            float bevelDepth,
            Bounds bounds,
            float minimumStableFaceArea,
            float minimumStableEdgeLength,
            ref EdgeWearBevelBuildStats stats)
        {
            // EW-4B.1: local bevels must be tolerant of individual bad
            // candidates. A single difficult corner/edge should not collapse the
            // entire optional edge-wear pass back to an un-bevelled mesh. Build
            // cumulatively from the original face set and keep only candidates
            // that produce valid local topology.
            List<EdgeWearBevelCandidate> accepted =
                new List<EdgeWearBevelCandidate>(selectedCount);
            List<PolygonFace> bestFaces = null;

            for (int i = 0; i < selectedCount; i++)
            {
                accepted.Add(candidates[i]);

                if (TryBuildLocalEdgeWearBevelFaces(
                        faces,
                        accepted,
                        bevelDepth,
                        bounds,
                        minimumStableFaceArea,
                        minimumStableEdgeLength,
                        out List<PolygonFace> rebuiltFaces,
                        out EdgeWearCornerClosureStats cornerClosureStats,
                        out EdgeWearTopologyStats topologyStats,
                        out EdgeWearBevelRejectReason rejectReason))
                {
                    bestFaces = rebuiltFaces;
                    stats.AcceptedCount = accepted.Count;
                    stats.CornerClosureCount = cornerClosureStats.CreatedCount;
                    stats.SkippedCornerClosureCount = cornerClosureStats.SkippedCount;
                    stats.TopologyOpenEdges = topologyStats.OpenEdgeCount;
                    stats.TopologyNonManifoldEdges = topologyStats.NonManifoldEdgeCount;
                    stats.TopologyTJunctions = topologyStats.TJunctionCount;
                }
                else
                {
                    stats.RegisterReject(rejectReason);
                    accepted.RemoveAt(accepted.Count - 1);
                }
            }

            if (accepted.Count == 0 || bestFaces == null)
            {
                return false;
            }

            faces.Clear();
            faces.AddRange(bestFaces);

            if (stats.SkippedCornerClosureCount > 0)
            {
                WarnEdgeWearCornerClosureGaps(stats);
            }

            if (stats.HasTopologyRejects)
            {
                WarnEdgeWearTopologyRejects(stats);
            }

            return true;
        }

        private static void WarnEdgeWearBevelFailure(EdgeWearBevelBuildStats stats)
        {
#if UNITY_EDITOR
            Debug.LogWarning(
                "GeneratedMass edge wear generated no valid topology-graph bevel result. " +
                stats.ToSummaryString());
#endif
        }

        private static void WarnEdgeWearCornerClosureGaps(EdgeWearBevelBuildStats stats)
        {
#if UNITY_EDITOR
            Debug.LogWarning(
                "GeneratedMass edge wear skipped one or more corner closures. " +
                stats.ToSummaryString());
#endif
        }

        private static void WarnEdgeWearTopologyRejects(EdgeWearBevelBuildStats stats)
        {
#if UNITY_EDITOR
            Debug.LogWarning(
                "GeneratedMass edge wear rejected one or more topology-unsafe bevel candidates. " +
                stats.ToSummaryString());
#endif
        }

        private static bool TryBuildLocalEdgeWearBevelFaces(
            List<PolygonFace> sourceFaces,
            List<EdgeWearBevelCandidate> selected,
            float bevelDepth,
            Bounds bounds,
            float minimumStableFaceArea,
            float minimumStableEdgeLength,
            out List<PolygonFace> rebuiltFaces,
            out EdgeWearCornerClosureStats cornerClosureStats,
            out EdgeWearTopologyStats topologyStats,
            out EdgeWearBevelRejectReason rejectReason)
        {
            rebuiltFaces = null;
            cornerClosureStats = new EdgeWearCornerClosureStats();
            topologyStats = EdgeWearTopologyStats.Empty;
            rejectReason = EdgeWearBevelRejectReason.None;

            Dictionary<int, List<FaceInsetCut>> cutsByFace =
                new Dictionary<int, List<FaceInsetCut>>();
            Dictionary<long, FaceInsetCut> cutsByCandidateFace =
                new Dictionary<long, FaceInsetCut>();

            for (int i = 0; i < selected.Count; i++)
            {
                EdgeWearBevelCandidate candidate = selected[i];
                float candidateDepth = bevelDepth * candidate.DepthMultiplier;

                if (!TryBuildFaceInsetCut(
                        candidate,
                        candidate.FaceA,
                        sourceFaces[candidate.FaceA],
                        candidateDepth,
                        out FaceInsetCut cutA) ||
                    !TryBuildFaceInsetCut(
                        candidate,
                        candidate.FaceB,
                        sourceFaces[candidate.FaceB],
                        candidateDepth,
                        out FaceInsetCut cutB))
                {
                    rejectReason = EdgeWearBevelRejectReason.InsetCut;
                    return false;
                }

                AddFaceInsetCut(cutsByFace, cutA);
                AddFaceInsetCut(cutsByFace, cutB);
                cutsByCandidateFace[MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceA)] = cutA;
                cutsByCandidateFace[MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceB)] = cutB;
            }

            if (selected.Count == 0)
            {
                rejectReason = EdgeWearBevelRejectReason.ValidationGlobal;
                return false;
            }

            EdgeWearTopologyStats sourceTopologyStats = AuditEdgeWearTopology(
                sourceFaces,
                minimumStableEdgeLength);

            float localBaseFaceMinArea = Mathf.Max(
                TinyFaceAreaEpsilon * 4f,
                minimumStableFaceArea * 0.2f);
            float localBevelFaceMinArea = Mathf.Max(
                TinyFaceAreaEpsilon * 4f,
                minimumStableFaceArea * 0.12f);
            float localCapFaceMinArea = Mathf.Max(
                TinyFaceAreaEpsilon * 2f,
                minimumStableFaceArea * 0.05f);
            float localBaseMinEdgeLength = Mathf.Max(
                PlaneEpsilon * 8f,
                minimumStableEdgeLength * 0.35f);
            float localBevelMinEdgeLength = Mathf.Max(
                PlaneEpsilon * 8f,
                minimumStableEdgeLength * 0.25f);
            float localCapMinEdgeLength = Mathf.Max(
                PlaneEpsilon * 4f,
                minimumStableEdgeLength * 0.15f);

            List<PolygonFace> localRebuiltFaces =
                new List<PolygonFace>(sourceFaces.Count + selected.Count * 5);
            Dictionary<int, List<Vector3>> rebuiltBasePolygons =
                new Dictionary<int, List<Vector3>>(sourceFaces.Count);

            for (int faceIndex = 0; faceIndex < sourceFaces.Count; faceIndex++)
            {
                PolygonFace face = sourceFaces[faceIndex];
                List<Vector3> polygon = new List<Vector3>(face.Vertices);

                if (cutsByFace.TryGetValue(faceIndex, out List<FaceInsetCut> faceCuts))
                {
                    for (int cutIndex = 0; cutIndex < faceCuts.Count; cutIndex++)
                    {
                        polygon = ClipPolygon(
                            polygon,
                            faceCuts[cutIndex].Plane,
                            new List<Vector3>());
                        polygon = SanitizePolygon(polygon, face.Normal);

                        if (polygon.Count < 3 ||
                            CalculatePolygonArea(polygon) <= minimumStableFaceArea)
                        {
                            rejectReason = EdgeWearBevelRejectReason.FaceClip;
                            return false;
                        }
                    }
                }

                polygon = SanitizePolygon(polygon, face.Normal);
                if (polygon.Count < 3 ||
                    CalculatePolygonArea(polygon) <= minimumStableFaceArea)
                {
                    rejectReason = EdgeWearBevelRejectReason.FaceClip;
                    return false;
                }

                if (cutsByFace.ContainsKey(faceIndex) &&
                    !ValidateLocalEdgeWearFace(
                        polygon,
                        localBaseFaceMinArea,
                        localBaseMinEdgeLength))
                {
                    rejectReason = EdgeWearBevelRejectReason.ValidationBaseFace;
                    return false;
                }

                rebuiltBasePolygons[faceIndex] = polygon;
                localRebuiltFaces.Add(
                    new PolygonFace(
                        polygon,
                        face.Normal,
                        face.Feature,
                        face.FeatureStrength));
            }

            Dictionary<long, BevelRail> railsByCandidateFace =
                new Dictionary<long, BevelRail>(selected.Count * 2);
            float railTolerance = Mathf.Max(
                PlaneEpsilon * 8f,
                minimumStableEdgeLength * 0.55f);

            for (int i = 0; i < selected.Count; i++)
            {
                EdgeWearBevelCandidate candidate = selected[i];

                if (!cutsByCandidateFace.TryGetValue(
                        MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceA),
                        out FaceInsetCut cutA) ||
                    !cutsByCandidateFace.TryGetValue(
                        MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceB),
                        out FaceInsetCut cutB) ||
                    !rebuiltBasePolygons.TryGetValue(candidate.FaceA, out List<Vector3> polygonA) ||
                    !rebuiltBasePolygons.TryGetValue(candidate.FaceB, out List<Vector3> polygonB))
                {
                    rejectReason = EdgeWearBevelRejectReason.RailExtraction;
                    return false;
                }

                if (!TryExtractCutRail(
                        polygonA,
                        cutA,
                        railTolerance,
                        minimumStableEdgeLength,
                        out BevelRail railA,
                        out bool railAFragmented))
                {
                    rejectReason = railAFragmented
                        ? EdgeWearBevelRejectReason.RailFragmented
                        : EdgeWearBevelRejectReason.RailExtraction;
                    return false;
                }

                if (!TryExtractCutRail(
                        polygonB,
                        cutB,
                        railTolerance,
                        minimumStableEdgeLength,
                        out BevelRail railB,
                        out bool railBFragmented))
                {
                    rejectReason = railBFragmented
                        ? EdgeWearBevelRejectReason.RailFragmented
                        : EdgeWearBevelRejectReason.RailExtraction;
                    return false;
                }

                railsByCandidateFace[MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceA)] = railA;
                railsByCandidateFace[MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceB)] = railB;
            }

            Dictionary<VertexKey, EndpointCapAccumulator> cornerClosures =
                new Dictionary<VertexKey, EndpointCapAccumulator>(selected.Count * 2);

            for (int i = 0; i < selected.Count; i++)
            {
                EdgeWearBevelCandidate candidate = selected[i];
                BevelRail railA = railsByCandidateFace[
                    MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceA)];
                BevelRail railB = railsByCandidateFace[
                    MakeCandidateFaceKey(candidate.CandidateIndex, candidate.FaceB)];

                PolygonFace bevelFace = CreateOrientedFace(
                    candidate.BevelNormal,
                    PolygonFaceFeature.ConvexEdgeWear,
                    candidate.Strength,
                    railA.Start,
                    railA.End,
                    railB.End,
                    railB.Start);
                List<Vector3> cleanBevel = SanitizePolygon(
                    bevelFace.Vertices,
                    bevelFace.Normal);
                if (cleanBevel.Count < 3 ||
                    CalculatePolygonArea(cleanBevel) <= minimumStableFaceArea)
                {
                    rejectReason = EdgeWearBevelRejectReason.BevelFace;
                    return false;
                }

                if (!ValidateLocalEdgeWearFace(
                        cleanBevel,
                        localBevelFaceMinArea,
                        localBevelMinEdgeLength) ||
                    Vector3.Dot(
                        CalculatePolygonNormal(cleanBevel),
                        candidate.BevelNormal) < 0.35f)
                {
                    rejectReason = EdgeWearBevelRejectReason.ValidationBevelFace;
                    return false;
                }

                localRebuiltFaces.Add(
                    new PolygonFace(
                        cleanBevel,
                        bevelFace.Normal,
                        bevelFace.Feature,
                        bevelFace.FeatureStrength));

                AddCornerClosurePoints(
                    cornerClosures,
                    candidate.Start,
                    candidate.Strength,
                    railA.Start,
                    railB.Start);
                AddCornerClosurePoints(
                    cornerClosures,
                    candidate.End,
                    candidate.Strength,
                    railA.End,
                    railB.End);
            }

            AddCornerClosureFaces(
                localRebuiltFaces,
                cornerClosures,
                bounds,
                localCapFaceMinArea,
                localCapMinEdgeLength,
                ref cornerClosureStats);

            if (cornerClosureStats.SkippedCount > 0)
            {
                rejectReason = EdgeWearBevelRejectReason.ValidationCapFace;
                return false;
            }

            WeldSharedVertices(localRebuiltFaces);
            SanitizeAllFaces(localRebuiltFaces);

            if (CountFeatureFaces(
                    localRebuiltFaces,
                    PolygonFaceFeature.ConvexEdgeWear) < selected.Count)
            {
                rejectReason = EdgeWearBevelRejectReason.ValidationBevelFace;
                return false;
            }

            topologyStats = AuditEdgeWearTopology(
                localRebuiltFaces,
                minimumStableEdgeLength);
            if (topologyStats.OpenEdgeCount > sourceTopologyStats.OpenEdgeCount)
            {
                rejectReason = EdgeWearBevelRejectReason.ValidationOpenEdge;
                return false;
            }

            if (topologyStats.NonManifoldEdgeCount >
                sourceTopologyStats.NonManifoldEdgeCount)
            {
                rejectReason = EdgeWearBevelRejectReason.ValidationNonManifoldEdge;
                return false;
            }

            if (topologyStats.TJunctionCount > sourceTopologyStats.TJunctionCount)
            {
                rejectReason = EdgeWearBevelRejectReason.ValidationTJunction;
                return false;
            }

            rebuiltFaces = localRebuiltFaces;
            return true;
        }

        private static bool AddEndpointCapFace(
            List<PolygonFace> rebuiltFaces,
            Bounds bounds,
            Vector3 origin,
            float strength,
            Vector3 firstRailPoint,
            Vector3 secondRailPoint,
            float minimumStableFaceArea,
            float minimumStableEdgeLength)
        {
            if (AreSamePoint(origin, firstRailPoint) ||
                AreSamePoint(origin, secondRailPoint) ||
                AreSamePoint(firstRailPoint, secondRailPoint))
            {
                return false;
            }

            Vector3 normal = Vector3.Cross(
                firstRailPoint - origin,
                secondRailPoint - origin);
            if (normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            normal.Normalize();
            Vector3 outward = origin - bounds.center;
            if (outward.sqrMagnitude > MinimumEdgeLengthSqr &&
                Vector3.Dot(normal, outward) < 0f)
            {
                normal = -normal;
            }

            PolygonFace capFace = CreateOrientedFace(
                normal,
                PolygonFaceFeature.ConvexEdgeWear,
                strength,
                origin,
                firstRailPoint,
                secondRailPoint);
            List<Vector3> cleanCap = SanitizePolygon(
                capFace.Vertices,
                capFace.Normal);

            if (ValidateLocalEdgeWearFace(
                    cleanCap,
                    minimumStableFaceArea,
                    minimumStableEdgeLength))
            {
                rebuiltFaces.Add(
                    new PolygonFace(
                        cleanCap,
                        capFace.Normal,
                        capFace.Feature,
                        capFace.FeatureStrength));
                return true;
            }

            return false;
        }

        private static void AddCornerClosureFaces(
            List<PolygonFace> rebuiltFaces,
            Dictionary<VertexKey, EndpointCapAccumulator> cornerClosures,
            Bounds bounds,
            float minimumStableFaceArea,
            float minimumStableEdgeLength,
            ref EdgeWearCornerClosureStats stats)
        {
            foreach (EndpointCapAccumulator closure in cornerClosures.Values)
            {
                if (TryAddCornerClosureFace(
                        rebuiltFaces,
                        bounds,
                        closure,
                        minimumStableFaceArea,
                        minimumStableEdgeLength))
                {
                    stats.RegisterCreated();
                }
                else
                {
                    stats.RegisterSkipped();
                }
            }
        }

        private static bool TryAddCornerClosureFace(
            List<PolygonFace> rebuiltFaces,
            Bounds bounds,
            EndpointCapAccumulator closure,
            float minimumStableFaceArea,
            float minimumStableEdgeLength)
        {
            List<Vector3> uniqueRailPoints = GetUniquePoints(closure.Points);
            if (uniqueRailPoints.Count < 2)
            {
                return false;
            }

            if (closure.RailPairCount <= 1 || uniqueRailPoints.Count < 3)
            {
                return AddEndpointCapFace(
                    rebuiltFaces,
                    bounds,
                    closure.Origin,
                    closure.AverageStrength,
                    uniqueRailPoints[0],
                    uniqueRailPoints[1],
                    minimumStableFaceArea,
                    minimumStableEdgeLength);
            }

            Vector3 outward = closure.Origin - bounds.center;
            if (outward.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                outward = CalculatePolygonNormal(uniqueRailPoints);
            }

            PolygonFace cornerFace = CreateOrientedFace(
                outward,
                PolygonFaceFeature.ConvexEdgeWear,
                closure.AverageStrength,
                uniqueRailPoints.ToArray());
            List<Vector3> cleanCorner = SanitizePolygon(
                cornerFace.Vertices,
                cornerFace.Normal);

            if (ValidateLocalEdgeWearFace(
                    cleanCorner,
                    minimumStableFaceArea,
                    minimumStableEdgeLength))
            {
                rebuiltFaces.Add(
                    new PolygonFace(
                        cleanCorner,
                        cornerFace.Normal,
                        cornerFace.Feature,
                        cornerFace.FeatureStrength));
                return true;
            }

            return TryAddCornerClosureFan(
                rebuiltFaces,
                cornerFace.Normal,
                closure.AverageStrength,
                cleanCorner,
                minimumStableFaceArea,
                minimumStableEdgeLength);
        }

        private static bool TryAddCornerClosureFan(
            List<PolygonFace> rebuiltFaces,
            Vector3 outwardNormal,
            float strength,
            List<Vector3> orderedRailPoints,
            float minimumStableFaceArea,
            float minimumStableEdgeLength)
        {
            if (orderedRailPoints.Count < 3)
            {
                return false;
            }

            Vector3 centre = CalculateAverage(orderedRailPoints);
            int addedCount = 0;
            for (int i = 0; i < orderedRailPoints.Count; i++)
            {
                Vector3 start = orderedRailPoints[i];
                Vector3 end = orderedRailPoints[(i + 1) % orderedRailPoints.Count];
                if (AreSamePoint(start, end) ||
                    AreSamePoint(start, centre) ||
                    AreSamePoint(end, centre))
                {
                    continue;
                }

                PolygonFace fanFace = CreateOrientedFace(
                    outwardNormal,
                    PolygonFaceFeature.ConvexEdgeWear,
                    strength,
                    start,
                    end,
                    centre);
                List<Vector3> cleanFan = SanitizePolygon(
                    fanFace.Vertices,
                    fanFace.Normal);

                if (!ValidateLocalEdgeWearFace(
                        cleanFan,
                        minimumStableFaceArea,
                        minimumStableEdgeLength))
                {
                    continue;
                }

                rebuiltFaces.Add(
                    new PolygonFace(
                        cleanFan,
                        fanFace.Normal,
                        fanFace.Feature,
                        fanFace.FeatureStrength));
                addedCount++;
            }

            return addedCount > 0;
        }

        private static void AddFaceInsetCut(
            Dictionary<int, List<FaceInsetCut>> cutsByFace,
            FaceInsetCut cut)
        {
            if (!cutsByFace.TryGetValue(cut.FaceIndex, out List<FaceInsetCut> cuts))
            {
                cuts = new List<FaceInsetCut>();
                cutsByFace.Add(cut.FaceIndex, cuts);
            }

            cuts.Add(cut);
        }

        private static bool TryBuildFaceInsetCut(
            EdgeWearBevelCandidate candidate,
            int faceIndex,
            PolygonFace face,
            float candidateDepth,
            out FaceInsetCut cut)
        {
            cut = default;

            if (!TryResolveFaceEdge(
                    face,
                    candidate.Start,
                    candidate.End,
                    out Vector3 orientedStart,
                    out Vector3 orientedEnd))
            {
                return false;
            }

            Vector3 orientedDirection = orientedEnd - orientedStart;
            float edgeLength = orientedDirection.magnitude;
            if (edgeLength <= PlaneEpsilon)
            {
                return false;
            }

            orientedDirection /= edgeLength;
            Vector3 canonicalDirection = candidate.End - candidate.Start;
            float canonicalLength = canonicalDirection.magnitude;
            if (canonicalLength <= PlaneEpsilon)
            {
                return false;
            }

            canonicalDirection /= canonicalLength;
            Vector3 inward = Vector3.Cross(face.Normal, orientedDirection);
            if (inward.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            inward.Normalize();
            Vector3 faceCentre = CalculateAverage(face.Vertices);
            Vector3 edgeMidpoint = (orientedStart + orientedEnd) * 0.5f;
            if (Vector3.Dot(inward, faceCentre - edgeMidpoint) < 0f)
            {
                inward = -inward;
            }

            float projection = Mathf.Abs(Vector3.Dot(candidate.BevelNormal, inward));
            float faceInset = candidateDepth / Mathf.Max(0.22f, projection);
            faceInset = Mathf.Clamp(
                faceInset,
                candidateDepth * 0.55f,
                candidateDepth * 3.5f);

            Vector3 cutPoint = edgeMidpoint + inward * faceInset;
            Vector3 keepNormal = -inward;
            CutPlane plane = new CutPlane(
                keepNormal,
                Vector3.Dot(keepNormal, cutPoint));

            cut = new FaceInsetCut(
                candidate.CandidateIndex,
                faceIndex,
                candidate.Start,
                candidate.End,
                canonicalDirection,
                inward,
                plane);
            return true;
        }

        private static bool TryResolveFaceEdge(
            PolygonFace face,
            Vector3 edgeStart,
            Vector3 edgeEnd,
            out Vector3 orientedStart,
            out Vector3 orientedEnd)
        {
            for (int i = 0; i < face.Vertices.Count; i++)
            {
                Vector3 current = face.Vertices[i];
                Vector3 next = face.Vertices[(i + 1) % face.Vertices.Count];

                if (AreSamePoint(current, edgeStart) &&
                    AreSamePoint(next, edgeEnd))
                {
                    orientedStart = current;
                    orientedEnd = next;
                    return true;
                }

                if (AreSamePoint(current, edgeEnd) &&
                    AreSamePoint(next, edgeStart))
                {
                    orientedStart = current;
                    orientedEnd = next;
                    return true;
                }
            }

            orientedStart = Vector3.zero;
            orientedEnd = Vector3.zero;
            return false;
        }

        private static bool TryExtractCutRail(
            List<Vector3> polygon,
            FaceInsetCut cut,
            float tolerance,
            float minimumStableEdgeLength,
            out BevelRail rail,
            out bool fragmented)
        {
            rail = default;
            fragmented = false;

            // Prefer the actual clipped polygon edge created by this inset cut.
            // EW-4B's original version gathered every vertex near the cut plane
            // and picked min/max along the source edge. Around corners or faces
            // with several cuts that can accidentally choose unrelated points.
            Vector3 bestStart = Vector3.zero;
            Vector3 bestEnd = Vector3.zero;
            float bestScore = 0f;
            int alignedSegmentCount = 0;
            float minimumLength = Mathf.Max(
                minimumStableEdgeLength,
                tolerance * 0.45f);
            float originalLength = (cut.OriginalEnd - cut.OriginalStart).magnitude;
            float alongTolerance = Mathf.Max(tolerance * 2f, minimumStableEdgeLength * 0.75f);

            for (int i = 0; i < polygon.Count; i++)
            {
                Vector3 start = polygon[i];
                Vector3 end = polygon[(i + 1) % polygon.Count];
                float startDistance = Mathf.Abs(cut.Plane.SignedDistance(start));
                float endDistance = Mathf.Abs(cut.Plane.SignedDistance(end));

                if (startDistance > tolerance || endDistance > tolerance)
                {
                    continue;
                }

                Vector3 edge = end - start;
                float length = edge.magnitude;
                if (length <= minimumLength)
                {
                    continue;
                }

                Vector3 direction = edge / length;
                float alignment = Mathf.Abs(Vector3.Dot(direction, cut.EdgeDirection));
                if (alignment < 0.72f)
                {
                    continue;
                }

                float startAlong = Vector3.Dot(start - cut.OriginalStart, cut.EdgeDirection);
                float endAlong = Vector3.Dot(end - cut.OriginalStart, cut.EdgeDirection);
                float minAlong = Mathf.Min(startAlong, endAlong);
                float maxAlong = Mathf.Max(startAlong, endAlong);
                if (maxAlong < -alongTolerance ||
                    minAlong > originalLength + alongTolerance)
                {
                    continue;
                }

                alignedSegmentCount++;
                float score = length * alignment;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestStart = startAlong <= endAlong ? start : end;
                    bestEnd = startAlong <= endAlong ? end : start;
                }
            }

            if (alignedSegmentCount > 1)
            {
                fragmented = true;
                return false;
            }

            if (bestScore > 0f)
            {
                rail = new BevelRail(bestStart, bestEnd);
                return true;
            }

            // Fallback: use near-plane points only if an aligned rail edge was not
            // present. This keeps thin/edge-case bevels from disappearing while
            // still filtering points outside the source-edge interval.
            List<Vector3> railPoints = new List<Vector3>(2);
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector3 start = polygon[i];
                Vector3 end = polygon[(i + 1) % polygon.Count];
                float startDistance = Mathf.Abs(cut.Plane.SignedDistance(start));
                float endDistance = Mathf.Abs(cut.Plane.SignedDistance(end));

                if (startDistance <= tolerance)
                {
                    float along = Vector3.Dot(start - cut.OriginalStart, cut.EdgeDirection);
                    if (along >= -alongTolerance &&
                        along <= originalLength + alongTolerance)
                    {
                        AddPointIfDifferent(railPoints, start);
                    }
                }

                if (endDistance <= tolerance)
                {
                    float along = Vector3.Dot(end - cut.OriginalStart, cut.EdgeDirection);
                    if (along >= -alongTolerance &&
                        along <= originalLength + alongTolerance)
                    {
                        AddPointIfDifferent(railPoints, end);
                    }
                }
            }

            railPoints = GetUniquePoints(railPoints);
            if (railPoints.Count < 2)
            {
                return false;
            }

            if (railPoints.Count > 2)
            {
                fragmented = true;
                return false;
            }

            Vector3 startPoint = railPoints[0];
            Vector3 endPoint = railPoints[0];
            float minPointAlong = Vector3.Dot(startPoint - cut.OriginalStart, cut.EdgeDirection);
            float maxPointAlong = minPointAlong;

            for (int i = 1; i < railPoints.Count; i++)
            {
                float along = Vector3.Dot(railPoints[i] - cut.OriginalStart, cut.EdgeDirection);
                if (along < minPointAlong)
                {
                    minPointAlong = along;
                    startPoint = railPoints[i];
                }

                if (along > maxPointAlong)
                {
                    maxPointAlong = along;
                    endPoint = railPoints[i];
                }
            }

            if ((endPoint - startPoint).magnitude <= minimumLength)
            {
                return false;
            }

            rail = new BevelRail(startPoint, endPoint);
            return true;
        }

        private static void AddCornerClosurePoints(
            Dictionary<VertexKey, EndpointCapAccumulator> cornerClosures,
            Vector3 origin,
            float strength,
            Vector3 firstRailPoint,
            Vector3 secondRailPoint)
        {
            VertexKey key = new VertexKey(origin);
            if (!cornerClosures.TryGetValue(key, out EndpointCapAccumulator cap))
            {
                cap = new EndpointCapAccumulator(origin);
                cornerClosures.Add(key, cap);
            }

            cap.AddRailPair(firstRailPoint, secondRailPoint, strength);
        }

        private static EdgeWearTopologyStats AuditEdgeWearTopology(
            List<PolygonFace> faces,
            float minimumStableEdgeLength)
        {
            if (faces == null || faces.Count == 0)
            {
                return EdgeWearTopologyStats.Empty;
            }

            Dictionary<TopologyEdgeKey, int> edgeUseCounts =
                new Dictionary<TopologyEdgeKey, int>();
            List<TopologyEdgeSegment> edgeSegments =
                new List<TopologyEdgeSegment>();
            Dictionary<VertexKey, Vector3> uniqueVertices =
                new Dictionary<VertexKey, Vector3>();

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                if (vertices == null || vertices.Count < 3)
                {
                    continue;
                }

                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    Vector3 start = vertices[vertexIndex];
                    Vector3 end = vertices[(vertexIndex + 1) % vertices.Count];

                    if (AreSamePoint(start, end))
                    {
                        continue;
                    }

                    VertexKey startKey = new VertexKey(start);
                    VertexKey endKey = new VertexKey(end);
                    if (!uniqueVertices.ContainsKey(startKey))
                    {
                        uniqueVertices.Add(startKey, start);
                    }

                    if (!uniqueVertices.ContainsKey(endKey))
                    {
                        uniqueVertices.Add(endKey, end);
                    }

                    TopologyEdgeKey edgeKey = new TopologyEdgeKey(
                        startKey,
                        endKey);
                    edgeUseCounts.TryGetValue(edgeKey, out int useCount);
                    edgeUseCounts[edgeKey] = useCount + 1;
                    edgeSegments.Add(
                        new TopologyEdgeSegment(
                            start,
                            end,
                            startKey,
                            endKey));
                }
            }

            int openEdges = 0;
            int nonManifoldEdges = 0;
            foreach (int useCount in edgeUseCounts.Values)
            {
                if (useCount == 1)
                {
                    openEdges++;
                }
                else if (useCount > 2)
                {
                    nonManifoldEdges++;
                }
            }

            int tJunctions = CountTopologyTJunctions(
                uniqueVertices,
                edgeSegments,
                minimumStableEdgeLength);

            return new EdgeWearTopologyStats(
                openEdges,
                nonManifoldEdges,
                tJunctions);
        }

        private static int CountTopologyTJunctions(
            Dictionary<VertexKey, Vector3> uniqueVertices,
            List<TopologyEdgeSegment> edgeSegments,
            float minimumStableEdgeLength)
        {
            if (uniqueVertices.Count == 0 || edgeSegments.Count == 0)
            {
                return 0;
            }

            float tolerance = Mathf.Max(
                PointMergeDistance * 4f,
                minimumStableEdgeLength * 0.04f);
            float toleranceSqr = tolerance * tolerance;
            int count = 0;

            foreach (KeyValuePair<VertexKey, Vector3> vertex in uniqueVertices)
            {
                for (int edgeIndex = 0;
                     edgeIndex < edgeSegments.Count;
                     edgeIndex++)
                {
                    TopologyEdgeSegment edge = edgeSegments[edgeIndex];
                    if (vertex.Key.Equals(edge.StartKey) ||
                        vertex.Key.Equals(edge.EndKey))
                    {
                        continue;
                    }

                    if (IsPointOnSegmentInterior(
                            vertex.Value,
                            edge.Start,
                            edge.End,
                            toleranceSqr))
                    {
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        private static bool IsPointOnSegmentInterior(
            Vector3 point,
            Vector3 start,
            Vector3 end,
            float toleranceSqr)
        {
            Vector3 segment = end - start;
            float segmentLengthSqr = segment.sqrMagnitude;
            if (segmentLengthSqr <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            if ((point - start).sqrMagnitude <= toleranceSqr ||
                (point - end).sqrMagnitude <= toleranceSqr)
            {
                return false;
            }

            float t = Vector3.Dot(point - start, segment) / segmentLengthSqr;
            if (t <= 0f || t >= 1f)
            {
                return false;
            }

            Vector3 closest = start + segment * t;
            return (point - closest).sqrMagnitude <= toleranceSqr;
        }

        private static bool AreSamePoint(Vector3 left, Vector3 right)
        {
            return (left - right).sqrMagnitude <= PointMergeDistanceSqr;
        }

        private static long MakeCandidateFaceKey(int candidateIndex, int faceIndex)
        {
            return ((long)candidateIndex << 32) ^ (uint)faceIndex;
        }

        private static Bounds CalculateFaceBounds(List<PolygonFace> faces)
        {
            if (faces == null || faces.Count == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            bool initialized = false;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
                {
                    if (!initialized)
                    {
                        bounds = new Bounds(vertices[vertexIndex], Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(vertices[vertexIndex]);
                    }
                }
            }

            return initialized
                ? bounds
                : new Bounds(Vector3.zero, Vector3.one);
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
            CutPlane plane,
            PolygonFaceFeature capFeature = PolygonFaceFeature.Base,
            float capFeatureStrength = 0f)
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
                        new PolygonFace(
                            clipped,
                            faces[i].Normal,
                            faces[i].Feature,
                            faces[i].FeatureStrength));
                }
            }

            List<Vector3> uniqueCapPoints =
                GetUniquePoints(capPoints);

            if (uniqueCapPoints.Count >= 3)
            {
                PolygonFace capFace = CreateOrientedFace(
                    plane.Normal,
                    capFeature,
                    capFeatureStrength,
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
                            capFace.Normal,
                            capFace.Feature,
                            capFace.FeatureStrength));
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

        private static bool TryClipPolyhedron(
            List<PolygonFace> faces,
            CutPlane plane,
            PolygonFaceFeature capFeature,
            float capFeatureStrength,
            float minimumStableFaceArea,
            float minimumStableEdgeLength)
        {
            if (faces.Count < 4)
            {
                return false;
            }

            List<PolygonFace> snapshot = ClonePolygonFaces(faces);
            int previousFeatureFaceCount = CountFeatureFaces(faces, capFeature);

            ClipPolyhedron(
                faces,
                plane,
                capFeature,
                capFeatureStrength);

            bool accepted =
                CountFeatureFaces(faces, capFeature) > previousFeatureFaceCount &&
                ValidatePolyhedronFaces(
                    faces,
                    minimumStableFaceArea,
                    minimumStableEdgeLength);

            if (accepted)
            {
                return true;
            }

            faces.Clear();
            faces.AddRange(snapshot);
            return false;
        }

        private static List<PolygonFace> ClonePolygonFaces(
            List<PolygonFace> faces)
        {
            List<PolygonFace> clone = new List<PolygonFace>(faces.Count);
            for (int i = 0; i < faces.Count; i++)
            {
                clone.Add(
                    new PolygonFace(
                        new List<Vector3>(faces[i].Vertices),
                        faces[i].Normal,
                        faces[i].Feature,
                        faces[i].FeatureStrength));
            }

            return clone;
        }

        private static int CountFeatureFaces(
            List<PolygonFace> faces,
            PolygonFaceFeature feature)
        {
            int count = 0;
            for (int i = 0; i < faces.Count; i++)
            {
                if (faces[i].Feature == feature)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool ValidateLocalEdgeWearFace(
            List<Vector3> vertices,
            float minimumStableFaceArea,
            float minimumStableEdgeLength)
        {
            if (vertices.Count < 3 ||
                CalculatePolygonArea(vertices) <= minimumStableFaceArea)
            {
                return false;
            }

            float minimumEdgeLengthSqr =
                minimumStableEdgeLength * minimumStableEdgeLength;
            for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
            {
                Vector3 start = vertices[vertexIndex];
                Vector3 end = vertices[(vertexIndex + 1) % vertices.Count];
                if ((end - start).sqrMagnitude <= minimumEdgeLengthSqr)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ValidatePolyhedronFaces(
            List<PolygonFace> faces,
            float minimumStableFaceArea,
            float minimumStableEdgeLength)
        {
            if (faces.Count < 4)
            {
                return false;
            }

            float minimumEdgeLengthSqr = minimumStableEdgeLength * minimumStableEdgeLength;
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                if (vertices.Count < 3 ||
                    CalculatePolygonArea(vertices) <= minimumStableFaceArea)
                {
                    return false;
                }

                for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
                {
                    Vector3 start = vertices[vertexIndex];
                    Vector3 end = vertices[(vertexIndex + 1) % vertices.Count];
                    if ((end - start).sqrMagnitude <= minimumEdgeLengthSqr)
                    {
                        return false;
                    }
                }
            }

            return true;
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
            return CreateOrientedFace(
                outwardNormal,
                PolygonFaceFeature.Base,
                0f,
                points);
        }

        private static PolygonFace CreateOrientedFace(
            Vector3 outwardNormal,
            PolygonFaceFeature feature,
            float featureStrength,
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

            return new PolygonFace(
                ordered,
                outwardNormal.normalized,
                feature,
                featureStrength);
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
                    sourceFace.Normal,
                    sourceFace.Feature,
                    sourceFace.FeatureStrength);

                if (density == SurfaceFacetDensity.Sparse ||
                    face.Feature == PolygonFaceFeature.ConvexEdgeWear)
                {
                    for (int i = 1; i < face.Vertices.Count - 1; i++)
                    {
                        AddOrientedTriangle(
                            soup,
                            face.Vertices[0],
                            face.Vertices[i],
                            face.Vertices[i + 1],
                            face.Normal,
                            face.Feature,
                            face.FeatureStrength);
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
                        face.Normal,
                        face.Feature,
                        face.FeatureStrength);
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
                    recipe,
                    faceFeature,
                    faceFeatureStrength);

                meshData.AddTriangle(indexA, indexB, indexC);
            }

            return meshData;
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
            MassRecipe recipe,
            PolygonFaceFeature faceFeature,
            float faceFeatureStrength)
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

            return meshData.AddVertex(
                position,
                uv,
                new Color(red, green, blue, edgeWear),
                materialMasks);
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

        private sealed class EdgeWearEdgeAggregate
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly List<int> FaceIndices = new List<int>(2);

            public EdgeWearEdgeAggregate(Vector3 start, Vector3 end)
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

        private enum EdgeWearBevelRejectReason
        {
            None,
            InsetCut,
            FaceClip,
            RailExtraction,
            RailFragmented,
            BevelFace,
            ValidationBaseFace,
            ValidationBevelFace,
            ValidationCapFace,
            ValidationOpenEdge,
            ValidationNonManifoldEdge,
            ValidationTJunction,
            ValidationGlobal
        }

        private readonly struct EdgeWearSupportPlane
        {
            public readonly Vector3 Normal;
            public readonly float Distance;
            public readonly PolygonFaceFeature Feature;
            public readonly float FeatureStrength;
            public readonly int CandidateIndex;

            public EdgeWearSupportPlane(
                Vector3 normal,
                float distance,
                PolygonFaceFeature feature,
                float featureStrength,
                int candidateIndex)
            {
                Normal = normal.normalized;
                Distance = distance;
                Feature = feature;
                FeatureStrength = Mathf.Clamp01(featureStrength);
                CandidateIndex = candidateIndex;
            }
        }

        private readonly struct EdgeWearTopologyStats
        {
            public static readonly EdgeWearTopologyStats Empty =
                new EdgeWearTopologyStats(0, 0, 0);

            public readonly int OpenEdgeCount;
            public readonly int NonManifoldEdgeCount;
            public readonly int TJunctionCount;

            public EdgeWearTopologyStats(
                int openEdgeCount,
                int nonManifoldEdgeCount,
                int tJunctionCount)
            {
                OpenEdgeCount = openEdgeCount;
                NonManifoldEdgeCount = nonManifoldEdgeCount;
                TJunctionCount = tJunctionCount;
            }
        }

        private readonly struct TopologyEdgeKey : IEquatable<TopologyEdgeKey>
        {
            private readonly VertexKey first;
            private readonly VertexKey second;

            public TopologyEdgeKey(VertexKey start, VertexKey end)
            {
                if (start.CompareTo(end) <= 0)
                {
                    first = start;
                    second = end;
                }
                else
                {
                    first = end;
                    second = start;
                }
            }

            public bool Equals(TopologyEdgeKey other)
            {
                return first.Equals(other.first) &&
                    second.Equals(other.second);
            }

            public override bool Equals(object obj)
            {
                return obj is TopologyEdgeKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (first.GetHashCode() * 397) ^
                        second.GetHashCode();
                }
            }
        }

        private readonly struct TopologyEdgeSegment
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly VertexKey StartKey;
            public readonly VertexKey EndKey;

            public TopologyEdgeSegment(
                Vector3 start,
                Vector3 end,
                VertexKey startKey,
                VertexKey endKey)
            {
                Start = start;
                End = end;
                StartKey = startKey;
                EndKey = endKey;
            }
        }

        private sealed class EdgeWearTopologyGraph
        {
            public readonly List<EdgeWearGraphVertex> Vertices =
                new List<EdgeWearGraphVertex>();
            public readonly List<EdgeWearGraphEdge> Edges =
                new List<EdgeWearGraphEdge>();
            public readonly List<EdgeWearGraphFace> Faces =
                new List<EdgeWearGraphFace>();
            public readonly Dictionary<VertexKey, int> VertexByKey =
                new Dictionary<VertexKey, int>();
            public readonly Dictionary<EdgeKey, int> EdgeByKey =
                new Dictionary<EdgeKey, int>();
        }

        private sealed class EdgeWearTopologyRebuildWorkspace
        {
            public readonly List<PolygonFace> RebuiltFaces =
                new List<PolygonFace>();

            public readonly Dictionary<int, PolygonFace> ClippedBaseFaceBySourceFace =
                new Dictionary<int, PolygonFace>();

            public readonly HashSet<int> AffectedSourceFaces =
                new HashSet<int>();
        }

        private sealed class EdgeWearGraphVertex
        {
            public readonly Vector3 Position;
            public readonly VertexKey Key;
            public readonly List<int> EdgeIndices = new List<int>();
            public readonly List<int> FaceIndices = new List<int>();

            public EdgeWearGraphVertex(Vector3 position, VertexKey key)
            {
                Position = position;
                Key = key;
            }

            public void AddEdge(int edgeIndex)
            {
                if (!EdgeIndices.Contains(edgeIndex))
                {
                    EdgeIndices.Add(edgeIndex);
                }
            }

            public void AddFace(int faceIndex)
            {
                if (!FaceIndices.Contains(faceIndex))
                {
                    FaceIndices.Add(faceIndex);
                }
            }
        }

        private sealed class EdgeWearGraphEdge
        {
            public readonly int VertexA;
            public readonly int VertexB;
            public int FaceA = -1;
            public int FaceB = -1;
            public int ExtraFaceCount;
            public int CandidateIndex = -1;
            public bool Selected;

            public EdgeWearGraphEdge(int vertexA, int vertexB)
            {
                VertexA = vertexA;
                VertexB = vertexB;
            }

            public bool TryAddFace(int faceIndex)
            {
                if (FaceA == faceIndex || FaceB == faceIndex)
                {
                    return true;
                }

                if (FaceA < 0)
                {
                    FaceA = faceIndex;
                    return true;
                }

                if (FaceB < 0)
                {
                    FaceB = faceIndex;
                    return true;
                }

                ExtraFaceCount++;
                return false;
            }
        }

        private sealed class EdgeWearGraphFace
        {
            public readonly int SourceFaceIndex;
            public readonly PolygonFace SourceFace;
            public readonly List<int> VertexIndices;
            public readonly List<int> EdgeIndices;

            public EdgeWearGraphFace(
                int sourceFaceIndex,
                PolygonFace sourceFace,
                List<int> vertexIndices,
                List<int> edgeIndices)
            {
                SourceFaceIndex = sourceFaceIndex;
                SourceFace = sourceFace;
                VertexIndices = vertexIndices;
                EdgeIndices = edgeIndices;
            }
        }

        private readonly struct EdgeWearSelectedGraphEdge
        {
            public readonly int GraphEdgeIndex;
            public readonly int CandidateIndex;
            public readonly EdgeWearBevelCandidate Candidate;

            public EdgeWearSelectedGraphEdge(
                int graphEdgeIndex,
                int candidateIndex,
                EdgeWearBevelCandidate candidate)
            {
                GraphEdgeIndex = graphEdgeIndex;
                CandidateIndex = candidateIndex;
                Candidate = candidate;
            }
        }

        private sealed class EdgeWearProfileGrid
        {
            public readonly EdgeWearBevelCandidate Candidate;
            public readonly BevelRail RailA;
            public readonly BevelRail RailB;
            public readonly int SampleCount;
            public readonly int ProfileSegments;
            public readonly Vector3[,] Points;

            public EdgeWearProfileGrid(
                EdgeWearBevelCandidate candidate,
                BevelRail railA,
                BevelRail railB,
                int sampleCount,
                int profileSegments,
                Vector3[,] points)
            {
                Candidate = candidate;
                RailA = railA;
                RailB = railB;
                SampleCount = sampleCount;
                ProfileSegments = profileSegments;
                Points = points;
            }

            public Vector3[] GetBoundarySamples(int profileIndex)
            {
                Vector3[] samples = new Vector3[SampleCount];
                int clampedProfileIndex = Mathf.Clamp(
                    profileIndex,
                    0,
                    ProfileSegments);
                for (int sampleIndex = 0; sampleIndex < SampleCount; sampleIndex++)
                {
                    samples[sampleIndex] = Points[sampleIndex, clampedProfileIndex];
                }

                return samples;
            }
        }

        private readonly struct EdgeWearFaceRailSamples
        {
            public readonly int FaceIndex;
            public readonly int CandidateIndex;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly Vector3[] Samples;

            public EdgeWearFaceRailSamples(
                int faceIndex,
                int candidateIndex,
                BevelRail rail,
                Vector3[] samples)
            {
                FaceIndex = faceIndex;
                CandidateIndex = candidateIndex;
                Start = rail.Start;
                End = rail.End;
                Samples = samples;
            }
        }

        private struct EdgeWearGraphBuildStats
        {
            public int GraphVertexCount;
            public int GraphEdgeCount;
            public int GraphFaceCount;
            public int GraphBoundaryEdgeCount;
            public int GraphNonManifoldEdgeCount;
            public int SelectedGraphEdgeCount;
            public int MissingSelectedGraphEdgeCount;
            public int MismatchedSelectedGraphFaceCount;
            public int DuplicateSelectedGraphEdgeCount;
            public int InvalidFaceCount;
            public int InvalidEdgeCount;
            public int FaceClipAffectedFaceCount;
            public int FaceClipSucceededFaceCount;
            public int FaceClipFailedFaceCount;
            public int SelectedFaceEdgeCount;
            public int ExpectedRailCount;
            public int ExtractedRailCount;
            public int MissingRailCount;
            public int ShortRailCount;
            public int FragmentedRailCount;
            public int ProfileEdgesPreparedCount;
            public int ProfileEdgesFailedCount;
            public int ProfileSampleCount;
            public int ProfileSegmentCount;
            public int ProfileGridPointCount;
            public int ProfileMinSampleCount;
            public int ProfileMaxSampleCount;
            public int ProfileInvalidPointCount;
            public int ProfileZeroWidthCount;
            public int PreservedBaseFaceCount;
            public int AffectedBaseFaceCount;
            public int ReplacedBaseFaceCount;
            public int BaseFaceValidationFailureCount;
            public int WorkspaceFaceCount;
            public int WorkspaceBaseFaceCount;
            public int WorkspaceTemporaryOpenEdgeCount;
            public int RailSampledBaseFaceCount;
            public int RailSampledBoundarySegmentCount;
            public int RailSamplesInsertedCount;
            public int RailSamplesPreservedCount;
            public int RailSampleInsertionFailureCount;
            public int RibbonEdgesPreparedCount;
            public int RibbonEdgesFailedCount;
            public int RibbonFacesExpectedCount;
            public int RibbonFacesBuiltCount;
            public int RibbonDegenerateFaceCount;
            public int RibbonInvalidFaceCount;
            public int WorkspaceConvexEdgeWearFaceCount;
            public int WorkspaceFacesAfterRibbonsCount;
            public int WorkspaceOpenEdgesAfterRibbonsCount;
        }

        private struct EdgeWearCornerClosureStats
        {
            public int CreatedCount;
            public int SkippedCount;

            public void RegisterCreated()
            {
                CreatedCount++;
            }

            public void RegisterSkipped()
            {
                SkippedCount++;
            }
        }

        private struct EdgeWearBevelBuildStats
        {
            public readonly int CandidateCount;
            public readonly int SelectedCount;
            public int AcceptedCount;
            public int RejectedInsetCut;
            public int RejectedFaceClip;
            public int RejectedRailExtraction;
            public int RejectedRailFragmented;
            public int RejectedBevelFace;
            public int RejectedValidationBaseFace;
            public int RejectedValidationBevelFace;
            public int RejectedValidationCapFace;
            public int RejectedValidationOpenEdge;
            public int RejectedValidationNonManifoldEdge;
            public int RejectedValidationTJunction;
            public int RejectedValidationGlobal;
            public int CornerClosureCount;
            public int SkippedCornerClosureCount;
            public int ConstructedBevelPlaneCount;
            public int ProducedBevelFaceCount;
            public int BevelDepthScalePercent;
            public int GraphVertexCount;
            public int GraphEdgeCount;
            public int GraphFaceCount;
            public int GraphBoundaryEdgeCount;
            public int GraphNonManifoldEdgeCount;
            public int SelectedGraphEdgeCount;
            public int MissingSelectedGraphEdgeCount;
            public int MismatchedSelectedGraphFaceCount;
            public int DuplicateSelectedGraphEdgeCount;
            public int InvalidGraphFaceCount;
            public int InvalidGraphEdgeCount;
            public int FaceClipAffectedFaceCount;
            public int FaceClipSucceededFaceCount;
            public int FaceClipFailedFaceCount;
            public int SelectedFaceEdgeCount;
            public int ExpectedRailCount;
            public int ExtractedRailCount;
            public int MissingRailCount;
            public int ShortRailCount;
            public int FragmentedRailCount;
            public int ProfileEdgesPreparedCount;
            public int ProfileEdgesFailedCount;
            public int ProfileSampleCount;
            public int ProfileSegmentCount;
            public int ProfileGridPointCount;
            public int ProfileMinSampleCount;
            public int ProfileMaxSampleCount;
            public int ProfileInvalidPointCount;
            public int ProfileZeroWidthCount;
            public int PreservedBaseFaceCount;
            public int AffectedBaseFaceCount;
            public int ReplacedBaseFaceCount;
            public int BaseFaceValidationFailureCount;
            public int WorkspaceFaceCount;
            public int WorkspaceBaseFaceCount;
            public int WorkspaceTemporaryOpenEdgeCount;
            public int RailSampledBaseFaceCount;
            public int RailSampledBoundarySegmentCount;
            public int RailSamplesInsertedCount;
            public int RailSamplesPreservedCount;
            public int RailSampleInsertionFailureCount;
            public int RibbonEdgesPreparedCount;
            public int RibbonEdgesFailedCount;
            public int RibbonFacesExpectedCount;
            public int RibbonFacesBuiltCount;
            public int RibbonDegenerateFaceCount;
            public int RibbonInvalidFaceCount;
            public int WorkspaceConvexEdgeWearFaceCount;
            public int WorkspaceFacesAfterRibbonsCount;
            public int WorkspaceOpenEdgesAfterRibbonsCount;
            public int TopologyOpenEdges;
            public int TopologyNonManifoldEdges;
            public int TopologyTJunctions;
            public int RejectedUnknown;

            public EdgeWearBevelBuildStats(int candidateCount, int selectedCount)
            {
                CandidateCount = candidateCount;
                SelectedCount = selectedCount;
                AcceptedCount = 0;
                RejectedInsetCut = 0;
                RejectedFaceClip = 0;
                RejectedRailExtraction = 0;
                RejectedRailFragmented = 0;
                RejectedBevelFace = 0;
                RejectedValidationBaseFace = 0;
                RejectedValidationBevelFace = 0;
                RejectedValidationCapFace = 0;
                RejectedValidationOpenEdge = 0;
                RejectedValidationNonManifoldEdge = 0;
                RejectedValidationTJunction = 0;
                RejectedValidationGlobal = 0;
                CornerClosureCount = 0;
                SkippedCornerClosureCount = 0;
                ConstructedBevelPlaneCount = 0;
                ProducedBevelFaceCount = 0;
                BevelDepthScalePercent = 0;
                GraphVertexCount = 0;
                GraphEdgeCount = 0;
                GraphFaceCount = 0;
                GraphBoundaryEdgeCount = 0;
                GraphNonManifoldEdgeCount = 0;
                SelectedGraphEdgeCount = 0;
                MissingSelectedGraphEdgeCount = 0;
                MismatchedSelectedGraphFaceCount = 0;
                DuplicateSelectedGraphEdgeCount = 0;
                InvalidGraphFaceCount = 0;
                InvalidGraphEdgeCount = 0;
                FaceClipAffectedFaceCount = 0;
                FaceClipSucceededFaceCount = 0;
                FaceClipFailedFaceCount = 0;
                SelectedFaceEdgeCount = 0;
                ExpectedRailCount = 0;
                ExtractedRailCount = 0;
                MissingRailCount = 0;
                ShortRailCount = 0;
                FragmentedRailCount = 0;
                ProfileEdgesPreparedCount = 0;
                ProfileEdgesFailedCount = 0;
                ProfileSampleCount = 0;
                ProfileSegmentCount = 0;
                ProfileGridPointCount = 0;
                ProfileMinSampleCount = 0;
                ProfileMaxSampleCount = 0;
                ProfileInvalidPointCount = 0;
                ProfileZeroWidthCount = 0;
                PreservedBaseFaceCount = 0;
                AffectedBaseFaceCount = 0;
                ReplacedBaseFaceCount = 0;
                BaseFaceValidationFailureCount = 0;
                WorkspaceFaceCount = 0;
                WorkspaceBaseFaceCount = 0;
                WorkspaceTemporaryOpenEdgeCount = 0;
                RailSampledBaseFaceCount = 0;
                RailSampledBoundarySegmentCount = 0;
                RailSamplesInsertedCount = 0;
                RailSamplesPreservedCount = 0;
                RailSampleInsertionFailureCount = 0;
                RibbonEdgesPreparedCount = 0;
                RibbonEdgesFailedCount = 0;
                RibbonFacesExpectedCount = 0;
                RibbonFacesBuiltCount = 0;
                RibbonDegenerateFaceCount = 0;
                RibbonInvalidFaceCount = 0;
                WorkspaceConvexEdgeWearFaceCount = 0;
                WorkspaceFacesAfterRibbonsCount = 0;
                WorkspaceOpenEdgesAfterRibbonsCount = 0;
                TopologyOpenEdges = 0;
                TopologyNonManifoldEdges = 0;
                TopologyTJunctions = 0;
                RejectedUnknown = 0;
            }

            public void ApplyGraphStats(EdgeWearGraphBuildStats graphStats)
            {
                GraphVertexCount = graphStats.GraphVertexCount;
                GraphEdgeCount = graphStats.GraphEdgeCount;
                GraphFaceCount = graphStats.GraphFaceCount;
                GraphBoundaryEdgeCount = graphStats.GraphBoundaryEdgeCount;
                GraphNonManifoldEdgeCount = graphStats.GraphNonManifoldEdgeCount;
                SelectedGraphEdgeCount = graphStats.SelectedGraphEdgeCount;
                MissingSelectedGraphEdgeCount = graphStats.MissingSelectedGraphEdgeCount;
                MismatchedSelectedGraphFaceCount = graphStats.MismatchedSelectedGraphFaceCount;
                DuplicateSelectedGraphEdgeCount = graphStats.DuplicateSelectedGraphEdgeCount;
                InvalidGraphFaceCount = graphStats.InvalidFaceCount;
                InvalidGraphEdgeCount = graphStats.InvalidEdgeCount;
                FaceClipAffectedFaceCount = graphStats.FaceClipAffectedFaceCount;
                FaceClipSucceededFaceCount = graphStats.FaceClipSucceededFaceCount;
                FaceClipFailedFaceCount = graphStats.FaceClipFailedFaceCount;
                SelectedFaceEdgeCount = graphStats.SelectedFaceEdgeCount;
                ExpectedRailCount = graphStats.ExpectedRailCount;
                ExtractedRailCount = graphStats.ExtractedRailCount;
                MissingRailCount = graphStats.MissingRailCount;
                ShortRailCount = graphStats.ShortRailCount;
                FragmentedRailCount = graphStats.FragmentedRailCount;
                ProfileEdgesPreparedCount = graphStats.ProfileEdgesPreparedCount;
                ProfileEdgesFailedCount = graphStats.ProfileEdgesFailedCount;
                ProfileSampleCount = graphStats.ProfileSampleCount;
                ProfileSegmentCount = graphStats.ProfileSegmentCount;
                ProfileGridPointCount = graphStats.ProfileGridPointCount;
                ProfileMinSampleCount = graphStats.ProfileMinSampleCount;
                ProfileMaxSampleCount = graphStats.ProfileMaxSampleCount;
                ProfileInvalidPointCount = graphStats.ProfileInvalidPointCount;
                ProfileZeroWidthCount = graphStats.ProfileZeroWidthCount;
                PreservedBaseFaceCount = graphStats.PreservedBaseFaceCount;
                AffectedBaseFaceCount = graphStats.AffectedBaseFaceCount;
                ReplacedBaseFaceCount = graphStats.ReplacedBaseFaceCount;
                BaseFaceValidationFailureCount = graphStats.BaseFaceValidationFailureCount;
                WorkspaceFaceCount = graphStats.WorkspaceFaceCount;
                WorkspaceBaseFaceCount = graphStats.WorkspaceBaseFaceCount;
                WorkspaceTemporaryOpenEdgeCount = graphStats.WorkspaceTemporaryOpenEdgeCount;
                RailSampledBaseFaceCount = graphStats.RailSampledBaseFaceCount;
                RailSampledBoundarySegmentCount = graphStats.RailSampledBoundarySegmentCount;
                RailSamplesInsertedCount = graphStats.RailSamplesInsertedCount;
                RailSamplesPreservedCount = graphStats.RailSamplesPreservedCount;
                RailSampleInsertionFailureCount = graphStats.RailSampleInsertionFailureCount;
                RibbonEdgesPreparedCount = graphStats.RibbonEdgesPreparedCount;
                RibbonEdgesFailedCount = graphStats.RibbonEdgesFailedCount;
                RibbonFacesExpectedCount = graphStats.RibbonFacesExpectedCount;
                RibbonFacesBuiltCount = graphStats.RibbonFacesBuiltCount;
                RibbonDegenerateFaceCount = graphStats.RibbonDegenerateFaceCount;
                RibbonInvalidFaceCount = graphStats.RibbonInvalidFaceCount;
                WorkspaceConvexEdgeWearFaceCount = graphStats.WorkspaceConvexEdgeWearFaceCount;
                WorkspaceFacesAfterRibbonsCount = graphStats.WorkspaceFacesAfterRibbonsCount;
                WorkspaceOpenEdgesAfterRibbonsCount = graphStats.WorkspaceOpenEdgesAfterRibbonsCount;
            }

            public void RegisterReject(EdgeWearBevelRejectReason reason)
            {
                switch (reason)
                {
                    case EdgeWearBevelRejectReason.InsetCut:
                        RejectedInsetCut++;
                        break;
                    case EdgeWearBevelRejectReason.FaceClip:
                        RejectedFaceClip++;
                        break;
                    case EdgeWearBevelRejectReason.RailExtraction:
                        RejectedRailExtraction++;
                        break;
                    case EdgeWearBevelRejectReason.RailFragmented:
                        RejectedRailFragmented++;
                        break;
                    case EdgeWearBevelRejectReason.BevelFace:
                        RejectedBevelFace++;
                        break;
                    case EdgeWearBevelRejectReason.ValidationBaseFace:
                        RejectedValidationBaseFace++;
                        break;
                    case EdgeWearBevelRejectReason.ValidationBevelFace:
                        RejectedValidationBevelFace++;
                        break;
                    case EdgeWearBevelRejectReason.ValidationCapFace:
                        RejectedValidationCapFace++;
                        break;
                    case EdgeWearBevelRejectReason.ValidationOpenEdge:
                        RejectedValidationOpenEdge++;
                        break;
                    case EdgeWearBevelRejectReason.ValidationNonManifoldEdge:
                        RejectedValidationNonManifoldEdge++;
                        break;
                    case EdgeWearBevelRejectReason.ValidationTJunction:
                        RejectedValidationTJunction++;
                        break;
                    case EdgeWearBevelRejectReason.ValidationGlobal:
                        RejectedValidationGlobal++;
                        break;
                    default:
                        RejectedUnknown++;
                        break;
                }
            }

            public bool HasTopologyRejects =>
                RejectedValidationOpenEdge > 0 ||
                RejectedValidationNonManifoldEdge > 0 ||
                RejectedValidationTJunction > 0;

            public string ToSummaryString()
            {
                return
                    $"candidates={CandidateCount}, selected={SelectedCount}, accepted={AcceptedCount}, " +
                    $"rejectedInsetCut={RejectedInsetCut}, rejectedFaceClip={RejectedFaceClip}, " +
                    $"rejectedRailExtraction={RejectedRailExtraction}, " +
                    $"rejectedRailFragmented={RejectedRailFragmented}, " +
                    $"rejectedBevelFace={RejectedBevelFace}, " +
                    $"rejectedValidationBaseFace={RejectedValidationBaseFace}, " +
                    $"rejectedValidationBevelFace={RejectedValidationBevelFace}, " +
                    $"rejectedValidationCapFace={RejectedValidationCapFace}, " +
                    $"rejectedValidationOpenEdge={RejectedValidationOpenEdge}, " +
                    $"rejectedValidationNonManifoldEdge={RejectedValidationNonManifoldEdge}, " +
                    $"rejectedValidationTJunction={RejectedValidationTJunction}, " +
                    $"rejectedValidationGlobal={RejectedValidationGlobal}, " +
                    $"cornerClosures={CornerClosureCount}, " +
                    $"skippedCornerClosures={SkippedCornerClosureCount}, " +
                    $"constructedBevelPlanes={ConstructedBevelPlaneCount}, " +
                    $"producedBevelFaces={ProducedBevelFaceCount}, " +
                    $"bevelDepthScalePercent={BevelDepthScalePercent}, " +
                    $"graphVertices={GraphVertexCount}, " +
                    $"graphEdges={GraphEdgeCount}, " +
                    $"graphFaces={GraphFaceCount}, " +
                    $"graphBoundaryEdges={GraphBoundaryEdgeCount}, " +
                    $"graphNonManifoldEdges={GraphNonManifoldEdgeCount}, " +
                    $"selectedGraphEdges={SelectedGraphEdgeCount}, " +
                    $"missingSelectedGraphEdges={MissingSelectedGraphEdgeCount}, " +
                    $"mismatchedSelectedGraphFaces={MismatchedSelectedGraphFaceCount}, " +
                    $"duplicateSelectedGraphEdges={DuplicateSelectedGraphEdgeCount}, " +
                    $"invalidGraphFaces={InvalidGraphFaceCount}, " +
                    $"invalidGraphEdges={InvalidGraphEdgeCount}, " +
                    $"faceClipAffectedFaces={FaceClipAffectedFaceCount}, " +
                    $"faceClipSucceededFaces={FaceClipSucceededFaceCount}, " +
                    $"faceClipFailedFaces={FaceClipFailedFaceCount}, " +
                    $"selectedFaceEdges={SelectedFaceEdgeCount}, " +
                    $"expectedRails={ExpectedRailCount}, " +
                    $"extractedRails={ExtractedRailCount}, " +
                    $"missingRails={MissingRailCount}, " +
                    $"shortRails={ShortRailCount}, " +
                    $"fragmentedRails={FragmentedRailCount}, " +
                    $"profileEdgesPrepared={ProfileEdgesPreparedCount}, " +
                    $"profileEdgesFailed={ProfileEdgesFailedCount}, " +
                    $"profileSamples={ProfileSampleCount}, " +
                    $"profileSegments={ProfileSegmentCount}, " +
                    $"profileGridPoints={ProfileGridPointCount}, " +
                    $"profileMinSamples={ProfileMinSampleCount}, " +
                    $"profileMaxSamples={ProfileMaxSampleCount}, " +
                    $"profileInvalidPoints={ProfileInvalidPointCount}, " +
                    $"profileZeroWidth={ProfileZeroWidthCount}, " +
                    $"preservedBaseFaces={PreservedBaseFaceCount}, " +
                    $"affectedBaseFaces={AffectedBaseFaceCount}, " +
                    $"replacedBaseFaces={ReplacedBaseFaceCount}, " +
                    $"baseFaceValidationFailures={BaseFaceValidationFailureCount}, " +
                    $"workspaceFaces={WorkspaceFaceCount}, " +
                    $"workspaceBaseFaces={WorkspaceBaseFaceCount}, " +
                    $"workspaceTemporaryOpenEdges={WorkspaceTemporaryOpenEdgeCount}, " +
                    $"railSampledBaseFaces={RailSampledBaseFaceCount}, " +
                    $"railSampledBoundarySegments={RailSampledBoundarySegmentCount}, " +
                    $"railSamplesInserted={RailSamplesInsertedCount}, " +
                    $"railSamplesPreserved={RailSamplesPreservedCount}, " +
                    $"railSampleInsertionFailures={RailSampleInsertionFailureCount}, " +
                    $"ribbonEdgesPrepared={RibbonEdgesPreparedCount}, " +
                    $"ribbonEdgesFailed={RibbonEdgesFailedCount}, " +
                    $"ribbonFacesExpected={RibbonFacesExpectedCount}, " +
                    $"ribbonFacesBuilt={RibbonFacesBuiltCount}, " +
                    $"ribbonDegenerateFaces={RibbonDegenerateFaceCount}, " +
                    $"ribbonInvalidFaces={RibbonInvalidFaceCount}, " +
                    $"workspaceConvexEdgeWearFaces={WorkspaceConvexEdgeWearFaceCount}, " +
                    $"workspaceFacesAfterRibbons={WorkspaceFacesAfterRibbonsCount}, " +
                    $"workspaceOpenEdgesAfterRibbons={WorkspaceOpenEdgesAfterRibbonsCount}, " +
                    $"topologyOpenEdges={TopologyOpenEdges}, " +
                    $"topologyNonManifoldEdges={TopologyNonManifoldEdges}, " +
                    $"topologyTJunctions={TopologyTJunctions}, " +
                    $"rejectedUnknown={RejectedUnknown}.";
            }
        }

        private readonly struct EdgeWearBevelCandidate
        {
            public readonly int CandidateIndex;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly int FaceA;
            public readonly int FaceB;
            public readonly Vector3 NormalA;
            public readonly Vector3 NormalB;
            public readonly Vector3 Midpoint;
            public readonly Vector3 BevelNormal;
            public readonly float Score;
            public readonly float Strength;
            public readonly float DepthMultiplier;

            public EdgeWearBevelCandidate(
                int candidateIndex,
                Vector3 start,
                Vector3 end,
                int faceA,
                int faceB,
                Vector3 normalA,
                Vector3 normalB,
                Vector3 midpoint,
                Vector3 bevelNormal,
                float score,
                float strength,
                float depthMultiplier)
            {
                CandidateIndex = candidateIndex;
                Start = start;
                End = end;
                FaceA = faceA;
                FaceB = faceB;
                NormalA = normalA;
                NormalB = normalB;
                Midpoint = midpoint;
                BevelNormal = bevelNormal;
                Score = score;
                Strength = strength;
                DepthMultiplier = depthMultiplier;
            }
        }

        private readonly struct FaceInsetCut
        {
            public readonly int CandidateIndex;
            public readonly int FaceIndex;
            public readonly Vector3 OriginalStart;
            public readonly Vector3 OriginalEnd;
            public readonly Vector3 EdgeDirection;
            public readonly Vector3 Inward;
            public readonly CutPlane Plane;

            public FaceInsetCut(
                int candidateIndex,
                int faceIndex,
                Vector3 originalStart,
                Vector3 originalEnd,
                Vector3 edgeDirection,
                Vector3 inward,
                CutPlane plane)
            {
                CandidateIndex = candidateIndex;
                FaceIndex = faceIndex;
                OriginalStart = originalStart;
                OriginalEnd = originalEnd;
                EdgeDirection = edgeDirection.normalized;
                Inward = inward.normalized;
                Plane = plane;
            }
        }

        private readonly struct BevelRail
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;

            public BevelRail(Vector3 start, Vector3 end)
            {
                Start = start;
                End = end;
            }
        }

        private sealed class EndpointCapAccumulator
        {
            public readonly Vector3 Origin;
            public readonly List<Vector3> Points = new List<Vector3>();
            private float strengthSum;
            private int strengthCount;

            public EndpointCapAccumulator(Vector3 origin)
            {
                Origin = origin;
            }

            public int RailPairCount { get; private set; }

            public float AverageStrength => strengthCount > 0
                ? Mathf.Clamp01(strengthSum / strengthCount)
                : 0f;

            public void AddRailPair(
                Vector3 firstRailPoint,
                Vector3 secondRailPoint,
                float strength)
            {
                AddPointIfDifferent(Points, firstRailPoint);
                AddPointIfDifferent(Points, secondRailPoint);
                strengthSum += Mathf.Clamp01(strength);
                strengthCount++;
                RailPairCount++;
            }
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
