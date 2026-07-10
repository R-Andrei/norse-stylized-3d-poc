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
            if (!surfaceFeatures.HasValue || faces == null || faces.Count < 4)
            {
                return;
            }

            MassSurfaceFeatureSettings settings = surfaceFeatures.Value;
            float amount01 = Mathf.Clamp01(settings.EdgeWearAmount * 0.5f);
            if (amount01 <= 0.0001f)
            {
                return;
            }

            Bounds bounds = CalculateFaceBounds(faces);
            float maximumDimension = Mathf.Max(
                0.0001f,
                Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)));

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
                LogChamferReadiness(
                    new ChamferReadinessStats(0, 0),
                    false,
                    "no convex edge-wear candidates");
                return;
            }

            candidates.Sort((left, right) => right.Score.CompareTo(left.Score));

            float coverage01 = Mathf.Clamp01(settings.EdgeWearCoverage * 0.5f);
            int selectedCount = Mathf.Clamp(
                Mathf.CeilToInt(candidates.Count * coverage01),
                0,
                candidates.Count);
            if (selectedCount <= 0)
            {
                return;
            }

            float minimumStableEdgeLength = maximumDimension * 0.0012f;
            ChamferReadinessStats stats = new ChamferReadinessStats(
                candidates.Count,
                selectedCount);

            bool ready = TryBuildChamferTopologyContext(
                faces,
                candidates,
                selectedCount,
                minimumStableEdgeLength,
                ref stats,
                out ChamferTopologyContext context,
                out string blocker);

            LogChamferReadiness(stats, ready, blocker);

            if (ready)
            {
                float requestedWidth = maximumDimension * Mathf.Lerp(
                    0.006f,
                    0.028f,
                    Mathf.InverseLerp(0.25f, 2f, settings.EdgeWearWidth));
                HashSet<int> forcedDeferredEdges = new HashSet<int>();
                ChamferCornerStats cornerStats = default;
                ChamferEmissionStats emissionStats = default;
                bool cornersReady = false;
                bool emissionReady = false;
                string cornerBlocker = string.Empty;
                string emissionBlocker = string.Empty;

                const int MaximumCompatibilityPasses = 8;
                for (int compatibilityPass = 0;
                     compatibilityPass < MaximumCompatibilityPasses;
                     compatibilityPass++)
                {
                    cornerStats = new ChamferCornerStats();
                    cornersReady = AuditExplicitChamferCornerSolution(
                        faces,
                        context,
                        requestedWidth,
                        minimumStableEdgeLength,
                        maximumDimension * maximumDimension * 0.000001f,
                        forcedDeferredEdges,
                        ref cornerStats,
                        out ChamferCornerSolution cornerSolution,
                        out cornerBlocker);
                    if (!cornersReady)
                    {
                        break;
                    }

                    emissionStats = new ChamferEmissionStats();
                    emissionStats.CompatibilityPassCount = compatibilityPass + 1;
                    emissionReady = AuditProvisionalChamferEmission(
                        faces,
                        context,
                        cornerSolution,
                        minimumStableEdgeLength,
                        maximumDimension * maximumDimension * 0.000001f,
                        ref emissionStats,
                        out HashSet<int> conflictingEdges,
                        out emissionBlocker);
                    emissionStats.ConflictDeferredEdgeCount =
                        forcedDeferredEdges.Count + conflictingEdges.Count;
                    if (emissionReady || conflictingEdges.Count == 0)
                    {
                        break;
                    }

                    int previousDeferredCount = forcedDeferredEdges.Count;
                    forcedDeferredEdges.UnionWith(conflictingEdges);
                    if (forcedDeferredEdges.Count == previousDeferredCount)
                    {
                        emissionBlocker =
                            "active vertex-boundary conflicts did not produce a new deterministic deferral";
                        break;
                    }
                }

                LogChamferCornerAudit(
                    cornerStats,
                    cornersReady,
                    cornerBlocker);
                if (cornersReady)
                {
                    LogChamferEmissionAudit(
                        emissionStats,
                        emissionReady,
                        emissionBlocker);
                }
            }

            // EW-C2R rebuilds the active positive-width edge network, defers
            // deterministic duplicate-boundary conflicts, audits provisional
            // replacement faces and one-strip quads, then discards them. The
            // original PolygonFace list remains rendered until vertex patches pass.
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

        private static bool TryBuildChamferTopologyContext(
            List<PolygonFace> sourceFaces,
            List<EdgeWearBevelCandidate> candidates,
            int selectedCount,
            float minimumStableEdgeLength,
            ref ChamferReadinessStats stats,
            out ChamferTopologyContext context,
            out string blocker)
        {
            context = null;
            blocker = string.Empty;
            if (!TryBuildEdgeWearTopologyGraph(
                    sourceFaces,
                    out EdgeWearTopologyGraph graph,
                    out EdgeWearGraphBuildStats graphStats))
            {
                stats.ApplyGraphStats(graphStats);
                stats.Blocked = 1;
                blocker = "source topology graph failed validation";
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
                stats.Blocked = 1;
                blocker = "selected candidates did not map cleanly to source graph edges";
                return false;
            }

            stats.ApplyGraphStats(graphStats);
            stats.SourceHalfEdgeCount = 0;
            stats.SelectedManifoldEdgeCount = 0;
            stats.SelectedBoundaryEdgeCount = 0;
            stats.SelectedNonManifoldEdgeCount = 0;

            List<ChamferHalfEdge> halfEdges = BuildChamferHalfEdges(graph);
            stats.SourceHalfEdgeCount = halfEdges.Count;

            for (int i = 0; i < selectedEdges.Count; i++)
            {
                EdgeWearGraphEdge edge = graph.Edges[selectedEdges[i].GraphEdgeIndex];
                if (edge.ExtraFaceCount > 0)
                {
                    stats.SelectedNonManifoldEdgeCount++;
                }
                else if (edge.FaceA < 0 || edge.FaceB < 0)
                {
                    stats.SelectedBoundaryEdgeCount++;
                }
                else
                {
                    stats.SelectedManifoldEdgeCount++;
                }
            }

            TraceChamferBoundaryLoops(
                graph,
                halfEdges,
                ref stats);
            AuditChamferVertexFans(
                graph,
                halfEdges,
                ref stats);

            EdgeWearTopologyStats sourceTopology = AuditEdgeWearTopology(
                sourceFaces,
                minimumStableEdgeLength);
            stats.SourceOpenEdgeCount = sourceTopology.OpenEdgeCount;
            stats.SourceNonManifoldEdgeCount = sourceTopology.NonManifoldEdgeCount;
            stats.SourceTJunctionCount = sourceTopology.TJunctionCount;

            bool ready =
                graphStats.InvalidFaceCount == 0 &&
                graphStats.InvalidEdgeCount == 0 &&
                graphStats.MissingSelectedGraphEdgeCount == 0 &&
                graphStats.MismatchedSelectedGraphFaceCount == 0 &&
                graphStats.DuplicateSelectedGraphEdgeCount == 0 &&
                stats.SourceNonManifoldEdgeCount == 0 &&
                stats.SourceTJunctionCount == 0 &&
                stats.SelectedBoundaryEdgeCount == 0 &&
                stats.SelectedNonManifoldEdgeCount == 0 &&
                stats.BoundaryTraceFailureCount == 0 &&
                stats.DisconnectedVertexFanCount == 0;

            stats.Ready = ready ? 1 : 0;
            stats.Blocked = ready ? 0 : 1;
            if (!ready && string.IsNullOrEmpty(blocker))
            {
                blocker = "one or more EW-C topology readiness invariants failed";
            }

            if (ready)
            {
                context = new ChamferTopologyContext(
                    graph,
                    selectedEdges,
                    halfEdges);
            }

            return ready;
        }

        private static List<ChamferHalfEdge> BuildChamferHalfEdges(
            EdgeWearTopologyGraph graph)
        {
            List<ChamferHalfEdge> halfEdges = new List<ChamferHalfEdge>();
            Dictionary<long, int> directedByPair = new Dictionary<long, int>();

            for (int faceIndex = 0; faceIndex < graph.Faces.Count; faceIndex++)
            {
                EdgeWearGraphFace face = graph.Faces[faceIndex];
                int count = face.VertexIndices.Count;
                int firstHalfEdge = halfEdges.Count;
                for (int i = 0; i < count; i++)
                {
                    int origin = face.VertexIndices[i];
                    int destination = face.VertexIndices[(i + 1) % count];
                    ChamferHalfEdge halfEdge = new ChamferHalfEdge
                    {
                        Index = halfEdges.Count,
                        OriginVertex = origin,
                        DestinationVertex = destination,
                        FaceIndex = faceIndex,
                        SourceEdgeIndex = face.EdgeIndices[i],
                        Next = firstHalfEdge + ((i + 1) % count),
                        Previous = firstHalfEdge + ((i + count - 1) % count),
                        Opposite = -1,
                        IsSelected = graph.Edges[face.EdgeIndices[i]].Selected
                    };
                    halfEdges.Add(halfEdge);
                    directedByPair[PackDirectedVertexPair(origin, destination)] = halfEdge.Index;
                }
            }

            for (int i = 0; i < halfEdges.Count; i++)
            {
                ChamferHalfEdge halfEdge = halfEdges[i];
                if (directedByPair.TryGetValue(
                        PackDirectedVertexPair(
                            halfEdge.DestinationVertex,
                            halfEdge.OriginVertex),
                        out int opposite))
                {
                    halfEdge.Opposite = opposite;
                }
            }

            return halfEdges;
        }

        private static long PackDirectedVertexPair(int origin, int destination)
        {
            return ((long)origin << 32) | (uint)destination;
        }

        private static void TraceChamferBoundaryLoops(
            EdgeWearTopologyGraph graph,
            List<ChamferHalfEdge> halfEdges,
            ref ChamferReadinessStats stats)
        {
            Dictionary<int, List<int>> outgoingBoundaryByVertex =
                new Dictionary<int, List<int>>();
            for (int i = 0; i < halfEdges.Count; i++)
            {
                ChamferHalfEdge halfEdge = halfEdges[i];
                if (halfEdge.Opposite >= 0)
                {
                    continue;
                }

                if (!outgoingBoundaryByVertex.TryGetValue(
                        halfEdge.OriginVertex,
                        out List<int> outgoing))
                {
                    outgoing = new List<int>();
                    outgoingBoundaryByVertex.Add(halfEdge.OriginVertex, outgoing);
                }
                outgoing.Add(i);
            }

            HashSet<int> visited = new HashSet<int>();
            for (int i = 0; i < halfEdges.Count; i++)
            {
                if (halfEdges[i].Opposite >= 0 || visited.Contains(i))
                {
                    continue;
                }

                stats.SourceBoundaryLoopCount++;
                int current = i;
                int guard = 0;
                while (guard++ <= halfEdges.Count)
                {
                    if (!visited.Add(current))
                    {
                        if (current != i)
                        {
                            stats.BoundaryTraceFailureCount++;
                        }
                        break;
                    }

                    int destination = halfEdges[current].DestinationVertex;
                    if (!outgoingBoundaryByVertex.TryGetValue(
                            destination,
                            out List<int> nextCandidates) ||
                        nextCandidates.Count != 1)
                    {
                        stats.BoundaryTraceFailureCount++;
                        break;
                    }

                    current = nextCandidates[0];
                    if (current == i)
                    {
                        break;
                    }
                }

                if (guard > halfEdges.Count)
                {
                    stats.BoundaryTraceFailureCount++;
                }
            }
        }

        private static void AuditChamferVertexFans(
            EdgeWearTopologyGraph graph,
            List<ChamferHalfEdge> halfEdges,
            ref ChamferReadinessStats stats)
        {
            List<List<int>> outgoingByVertex = new List<List<int>>(graph.Vertices.Count);
            for (int i = 0; i < graph.Vertices.Count; i++)
            {
                outgoingByVertex.Add(new List<int>());
            }
            for (int i = 0; i < halfEdges.Count; i++)
            {
                outgoingByVertex[halfEdges[i].OriginVertex].Add(i);
            }

            for (int vertexIndex = 0; vertexIndex < graph.Vertices.Count; vertexIndex++)
            {
                List<int> outgoing = outgoingByVertex[vertexIndex];
                if (outgoing.Count == 0)
                {
                    continue;
                }

                bool affected = false;
                for (int i = 0; i < outgoing.Count; i++)
                {
                    affected |= halfEdges[outgoing[i]].IsSelected;
                }
                if (!affected)
                {
                    continue;
                }

                stats.AffectedVertexCount++;

                int start = -1;
                bool openFan = false;
                for (int i = 0; i < outgoing.Count; i++)
                {
                    ChamferHalfEdge candidate = halfEdges[outgoing[i]];
                    int previousOpposite = halfEdges[candidate.Previous].Opposite;
                    if (previousOpposite < 0)
                    {
                        start = candidate.Index;
                        openFan = true;
                        break;
                    }
                }
                if (start < 0)
                {
                    start = outgoing[0];
                }

                List<int> ordered = new List<int>(outgoing.Count);
                HashSet<int> visited = new HashSet<int>();
                int current = start;
                int guard = 0;
                while (current >= 0 && guard++ <= outgoing.Count)
                {
                    if (!visited.Add(current))
                    {
                        break;
                    }
                    ordered.Add(current);

                    ChamferHalfEdge currentHalfEdge = halfEdges[current];
                    int next = currentHalfEdge.Opposite >= 0
                        ? halfEdges[currentHalfEdge.Opposite].Next
                        : -1;
                    if (next < 0 || next == start)
                    {
                        break;
                    }
                    if (halfEdges[next].OriginVertex != vertexIndex)
                    {
                        stats.DisconnectedVertexFanCount++;
                        break;
                    }
                    current = next;
                }

                if (ordered.Count != outgoing.Count)
                {
                    stats.DisconnectedVertexFanCount++;
                    continue;
                }

                if (openFan)
                {
                    stats.OpenVertexFanCount++;
                }
                else
                {
                    stats.ClosedVertexFanCount++;
                }

                int selectedRunCount = CountChamferSelectedRuns(
                    ordered,
                    halfEdges,
                    openFan);
                stats.SelectedRunCount += selectedRunCount;
                if (selectedRunCount > 1)
                {
                    stats.MultipleSelectedRunVertexCount++;
                }
            }
        }

        private static int CountChamferSelectedRuns(
            List<int> orderedHalfEdges,
            List<ChamferHalfEdge> halfEdges,
            bool openFan)
        {
            if (orderedHalfEdges == null || orderedHalfEdges.Count == 0)
            {
                return 0;
            }

            int selectedCount = 0;
            int runCount = 0;
            bool previousSelected = openFan
                ? false
                : halfEdges[orderedHalfEdges[orderedHalfEdges.Count - 1]].IsSelected;

            for (int i = 0; i < orderedHalfEdges.Count; i++)
            {
                bool selected = halfEdges[orderedHalfEdges[i]].IsSelected;
                if (selected)
                {
                    selectedCount++;
                    if (!previousSelected)
                    {
                        runCount++;
                    }
                }
                previousSelected = selected;
            }

            if (!openFan && selectedCount == orderedHalfEdges.Count)
            {
                return 1;
            }

            return runCount;
        }


        private static bool AuditExplicitChamferCornerSolution(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            float requestedWidth,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            HashSet<int> forcedDeferredEdges,
            ref ChamferCornerStats stats,
            out ChamferCornerSolution solution,
            out string blocker)
        {
            solution = null;
            blocker = string.Empty;
            stats.SourceFaceCount = context.Graph.Faces.Count;
            stats.ExpectedCornerCount = context.HalfEdges.Count;
            stats.SelectedEdgeCount = context.SelectedSourceEdges.Count;
            stats.RequestedWidth = requestedWidth;

            Dictionary<int, float> widthByEdge =
                new Dictionary<int, float>(context.SelectedSourceEdges.Count);
            foreach (int edgeIndex in context.SelectedSourceEdges)
            {
                if (forcedDeferredEdges != null &&
                    forcedDeferredEdges.Contains(edgeIndex))
                {
                    widthByEdge.Add(edgeIndex, 0f);
                    continue;
                }

                float solvedWidth = CalculateChamferEdgeWidth(
                    context.Graph,
                    edgeIndex,
                    requestedWidth,
                    minimumStableEdgeLength,
                    out bool clamped);
                if (solvedWidth < minimumStableEdgeLength ||
                    float.IsNaN(solvedWidth) ||
                    float.IsInfinity(solvedWidth))
                {
                    stats.WidthSolveFailures++;
                    blocker = "one or more selected edges have no stable chamfer width";
                    return false;
                }

                widthByEdge.Add(edgeIndex, solvedWidth);
                if (clamped)
                {
                    stats.WidthClampedEdges++;
                }
            }

            if (!TrySolveCornerAwareChamferWidths(
                    sourceFaces,
                    context,
                    requestedWidth,
                    minimumStableEdgeLength,
                    widthByEdge,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            stats.MinimumSolvedWidth = float.PositiveInfinity;
            stats.MaximumSolvedWidth = 0f;
            foreach (KeyValuePair<int, float> pair in widthByEdge)
            {
                if (pair.Value <= PointMergeDistance)
                {
                    stats.DeferredSelectedEdgeCount++;
                    continue;
                }

                stats.ActiveSelectedEdgeCount++;
                stats.MinimumSolvedWidth = Mathf.Min(
                    stats.MinimumSolvedWidth,
                    pair.Value);
                stats.MaximumSolvedWidth = Mathf.Max(
                    stats.MaximumSolvedWidth,
                    pair.Value);
            }

            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners =
                new Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner>(
                    stats.ExpectedCornerCount);

            for (int faceIndex = 0;
                 faceIndex < context.Graph.Faces.Count;
                 faceIndex++)
            {
                EdgeWearGraphFace graphFace = context.Graph.Faces[faceIndex];
                PolygonFace sourceFace = sourceFaces[graphFace.SourceFaceIndex];
                Vector3 faceCentre = CalculateAverage(sourceFace.Vertices);
                int count = graphFace.VertexIndices.Count;

                for (int localIndex = 0; localIndex < count; localIndex++)
                {
                    int sourceVertexIndex = graphFace.VertexIndices[localIndex];
                    int previousEdgeIndex = graphFace.EdgeIndices[
                        (localIndex + count - 1) % count];
                    int nextEdgeIndex = graphFace.EdgeIndices[localIndex];
                    float previousWidthValue = 0f;
                    float nextWidthValue = 0f;
                    bool previousSelected =
                        context.SelectedSourceEdges.Contains(previousEdgeIndex) &&
                        widthByEdge.TryGetValue(previousEdgeIndex, out previousWidthValue) &&
                        previousWidthValue > PointMergeDistance;
                    bool nextSelected =
                        context.SelectedSourceEdges.Contains(nextEdgeIndex) &&
                        widthByEdge.TryGetValue(nextEdgeIndex, out nextWidthValue) &&
                        nextWidthValue > PointMergeDistance;
                    float previousWidth = previousSelected
                        ? previousWidthValue
                        : 0f;
                    float nextWidth = nextSelected
                        ? nextWidthValue
                        : 0f;

                    if (!TryBuildChamferFaceLine(
                            context.Graph,
                            previousEdgeIndex,
                            sourceFace.Normal,
                            faceCentre,
                            previousWidth,
                            out ChamferFaceLine previousLine) ||
                        !TryBuildChamferFaceLine(
                            context.Graph,
                            nextEdgeIndex,
                            sourceFace.Normal,
                            faceCentre,
                            nextWidth,
                            out ChamferFaceLine nextLine))
                    {
                        stats.CornerSolveFailures++;
                        blocker = "failed to build a stable face-edge support line";
                        return false;
                    }

                    Vector3 sourceVertex =
                        context.Graph.Vertices[sourceVertexIndex].Position;
                    if (!TrySolveChamferFaceCorner(
                            sourceVertex,
                            previousLine,
                            nextLine,
                            sourceFace.Normal,
                            minimumStableEdgeLength * 0.001f,
                            out Vector3 solved))
                    {
                        stats.CornerSolveFailures++;
                        blocker = "one or more face corners have parallel or unstable offset lines";
                        return false;
                    }

                    if (!IsFinite(solved))
                    {
                        stats.NonFiniteCornerCount++;
                        blocker = "one or more solved face corners are non-finite";
                        return false;
                    }

                    float previousLength = GetGraphEdgeLength(
                        context.Graph,
                        previousEdgeIndex);
                    float nextLength = GetGraphEdgeLength(
                        context.Graph,
                        nextEdgeIndex);
                    float localLimit = CalculateChamferCornerDisplacementLimit(
                        requestedWidth,
                        minimumStableEdgeLength,
                        previousLength,
                        nextLength);
                    float displacement = (solved - sourceVertex).magnitude;
                    UpdateChamferFinalWorstCorner(
                        faceIndex,
                        sourceVertexIndex,
                        previousEdgeIndex,
                        nextEdgeIndex,
                        displacement,
                        localLimit,
                        ref stats);
                    if (displacement > localLimit + PointMergeDistance)
                    {
                        stats.ExcessiveDisplacementCornerCount++;
                        blocker = "one or more solved corners still exceed the conservative local displacement limit after width solving";
                        return false;
                    }

                    if (!previousSelected && !nextSelected)
                    {
                        stats.PreservedCornerCount++;
                    }
                    else if (previousSelected && nextSelected)
                    {
                        stats.DoubleSelectedCornerCount++;
                    }
                    else
                    {
                        stats.SingleSelectedCornerCount++;
                    }

                    corners.Add(
                        new ChamferFaceCornerKey(faceIndex, sourceVertexIndex),
                        new ChamferSolvedCorner(
                            solved,
                            faceIndex,
                            sourceVertexIndex,
                            previousEdgeIndex,
                            nextEdgeIndex,
                            previousSelected,
                            nextSelected));
                    stats.SolvedCornerCount++;
                }
            }

            if (!TryReconcileChamferUnselectedInternalEdges(
                    context,
                    corners,
                    widthByEdge,
                    minimumStableEdgeLength,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            if (!AuditChamferReplacementFaces(
                    sourceFaces,
                    context,
                    corners,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            if (!AuditChamferSelectedRails(
                    context,
                    corners,
                    widthByEdge,
                    minimumStableEdgeLength,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            if (!AuditChamferSolvedBoundary(
                    context,
                    corners,
                    minimumStableEdgeLength,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            if (float.IsPositiveInfinity(stats.MinimumSolvedWidth))
            {
                stats.MinimumSolvedWidth = 0f;
            }
            solution = new ChamferCornerSolution(corners, widthByEdge);
            stats.ReadyForEmission = 1;
            return true;
        }

        private static bool TrySolveCornerAwareChamferWidths(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            float requestedWidth,
            float minimumStableEdgeLength,
            Dictionary<int, float> widthByEdge,
            ref ChamferCornerStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            const int MaximumPasses = 12;
            const float SafetyScale = 0.95f;
            HashSet<int> cornerClampedEdges = new HashSet<int>();
            HashSet<int> sharedEdgeClampedEdges = new HashSet<int>();
            stats.MinimumCornerWidthScale = 1f;
            stats.MinimumSharedEdgeWidthScale = 1f;

            for (int pass = 0; pass < MaximumPasses; pass++)
            {
                if (!TryBuildChamferCornerTable(
                        sourceFaces,
                        context,
                        widthByEdge,
                        minimumStableEdgeLength,
                        out Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> passCorners,
                        out blocker))
                {
                    stats.CornerWidthConvergenceFailures++;
                    return false;
                }

                bool changed = false;
                foreach (ChamferSolvedCorner corner in passCorners.Values)
                {
                    EdgeWearGraphFace graphFace = context.Graph.Faces[corner.FaceIndex];
                    PolygonFace sourceFace = sourceFaces[graphFace.SourceFaceIndex];
                    Vector3 sourceVertex =
                        context.Graph.Vertices[corner.SourceVertexIndex].Position;
                    float previousLength = GetGraphEdgeLength(
                        context.Graph,
                        corner.PreviousSourceEdgeIndex);
                    float nextLength = GetGraphEdgeLength(
                        context.Graph,
                        corner.NextSourceEdgeIndex);
                    float localLimit = CalculateChamferCornerDisplacementLimit(
                        requestedWidth,
                        minimumStableEdgeLength,
                        previousLength,
                        nextLength);
                    float displacement = (corner.Position - sourceVertex).magnitude;

                    if (pass == 0)
                    {
                        UpdateChamferInitialWorstCorner(
                            corner.FaceIndex,
                            corner.SourceVertexIndex,
                            corner.PreviousSourceEdgeIndex,
                            corner.NextSourceEdgeIndex,
                            displacement,
                            localLimit,
                            ref stats);
                    }

                    if (displacement <= localLimit + PointMergeDistance)
                    {
                        continue;
                    }

                    float scale = Mathf.Clamp01(
                        SafetyScale * localLimit / displacement);
                    bool cornerChanged = false;
                    if (corner.PreviousSelected &&
                        TryClampChamferEdgeWidth(
                            corner.PreviousSourceEdgeIndex,
                            scale,
                            requestedWidth,
                            minimumStableEdgeLength,
                            widthByEdge,
                            cornerClampedEdges,
                            ref stats))
                    {
                        cornerChanged = true;
                    }
                    if (corner.NextSelected &&
                        corner.NextSourceEdgeIndex != corner.PreviousSourceEdgeIndex &&
                        TryClampChamferEdgeWidth(
                            corner.NextSourceEdgeIndex,
                            scale,
                            requestedWidth,
                            minimumStableEdgeLength,
                            widthByEdge,
                            cornerClampedEdges,
                            ref stats))
                    {
                        cornerChanged = true;
                    }

                    if (!cornerChanged)
                    {
                        stats.CornerWidthBelowMinimumFailures++;
                        blocker = "a corner remains over its displacement limit at the minimum stable chamfer width";
                        return false;
                    }
                    changed = true;
                }

                if (changed)
                {
                    stats.CornerWidthSolvePasses = pass + 1;
                    continue;
                }

                for (int edgeIndex = 0;
                     edgeIndex < context.Graph.Edges.Count;
                     edgeIndex++)
                {
                    EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                    bool activeSelected = edge.Selected &&
                        widthByEdge.TryGetValue(edgeIndex, out float activeWidth) &&
                        activeWidth > PointMergeDistance;
                    if (activeSelected || edge.FaceA < 0 || edge.FaceB < 0)
                    {
                        continue;
                    }

                    if (HasStableChamferSharedInterval(
                            context,
                            edgeIndex,
                            passCorners,
                            minimumStableEdgeLength))
                    {
                        continue;
                    }

                    HashSet<int> participatingEdges =
                        CollectChamferSharedEdgeParticipatingSelectedEdges(
                            edge,
                            passCorners);
                    if (participatingEdges.Count == 0 ||
                        !TryFindChamferSharedEdgeWidthScale(
                            sourceFaces,
                            context,
                            edgeIndex,
                            participatingEdges,
                            widthByEdge,
                            minimumStableEdgeLength,
                            out float solvedScale,
                            out blocker))
                    {
                        stats.SharedEdgeWidthConvergenceFailures++;
                        if (string.IsNullOrEmpty(blocker))
                        {
                            blocker = "an unselected internal edge has no stable common interval even at zero adjacent chamfer width";
                        }
                        return false;
                    }

                    bool edgeChanged = false;
                    foreach (int selectedEdgeIndex in participatingEdges)
                    {
                        float oldWidth = widthByEdge[selectedEdgeIndex];
                        float scaledWidth = oldWidth * solvedScale;
                        float newWidth = scaledWidth < minimumStableEdgeLength
                            ? 0f
                            : scaledWidth;
                        if (newWidth >= oldWidth - PointMergeDistance)
                        {
                            continue;
                        }

                        widthByEdge[selectedEdgeIndex] = newWidth;
                        if (newWidth <= PointMergeDistance)
                        {
                            stats.SharedEdgeWidthDeferredEdges++;
                        }
                        sharedEdgeClampedEdges.Add(selectedEdgeIndex);
                        stats.SharedEdgeWidthClampApplications++;
                        float relativeScale = requestedWidth > PointMergeDistance
                            ? newWidth / requestedWidth
                            : 1f;
                        stats.MinimumSharedEdgeWidthScale = Mathf.Min(
                            stats.MinimumSharedEdgeWidthScale,
                            relativeScale);
                        edgeChanged = true;
                    }

                    if (!edgeChanged)
                    {
                        bool deferredAny = false;
                        foreach (int selectedEdgeIndex in participatingEdges)
                        {
                            if (!widthByEdge.TryGetValue(selectedEdgeIndex, out float currentWidth) ||
                                currentWidth <= PointMergeDistance)
                            {
                                continue;
                            }

                            widthByEdge[selectedEdgeIndex] = 0f;
                            sharedEdgeClampedEdges.Add(selectedEdgeIndex);
                            stats.SharedEdgeWidthDeferredEdges++;
                            deferredAny = true;
                        }

                        if (!deferredAny)
                        {
                            stats.SharedEdgeWidthBelowMinimumFailures++;
                            blocker = "an unselected internal edge remains unstable after all participating chamfers were deferred";
                            return false;
                        }
                    }
                    changed = true;
                }

                stats.CornerWidthSolvePasses = pass + 1;
                stats.CornerWidthClampedEdges = cornerClampedEdges.Count;
                stats.SharedEdgeWidthClampedEdges = sharedEdgeClampedEdges.Count;
                if (!changed)
                {
                    return true;
                }
            }

            stats.CornerWidthClampedEdges = cornerClampedEdges.Count;
            stats.SharedEdgeWidthClampedEdges = sharedEdgeClampedEdges.Count;
            stats.CornerWidthConvergenceFailures++;
            blocker = "unified corner and shared-edge width solving did not converge";
            return false;
        }

        private static bool TryBuildChamferCornerTable(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            Dictionary<int, float> widthByEdge,
            float minimumStableEdgeLength,
            out Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            out string blocker)
        {
            blocker = string.Empty;
            corners = new Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner>(
                context.HalfEdges.Count);

            for (int faceIndex = 0;
                 faceIndex < context.Graph.Faces.Count;
                 faceIndex++)
            {
                EdgeWearGraphFace graphFace = context.Graph.Faces[faceIndex];
                PolygonFace sourceFace = sourceFaces[graphFace.SourceFaceIndex];
                Vector3 faceCentre = CalculateAverage(sourceFace.Vertices);
                int count = graphFace.VertexIndices.Count;

                for (int localIndex = 0; localIndex < count; localIndex++)
                {
                    int sourceVertexIndex = graphFace.VertexIndices[localIndex];
                    int previousEdgeIndex = graphFace.EdgeIndices[
                        (localIndex + count - 1) % count];
                    int nextEdgeIndex = graphFace.EdgeIndices[localIndex];
                    float previousWidthValue = 0f;
                    float nextWidthValue = 0f;
                    bool previousSelected =
                        context.SelectedSourceEdges.Contains(previousEdgeIndex) &&
                        widthByEdge.TryGetValue(previousEdgeIndex, out previousWidthValue) &&
                        previousWidthValue > PointMergeDistance;
                    bool nextSelected =
                        context.SelectedSourceEdges.Contains(nextEdgeIndex) &&
                        widthByEdge.TryGetValue(nextEdgeIndex, out nextWidthValue) &&
                        nextWidthValue > PointMergeDistance;
                    float previousWidth = previousSelected
                        ? previousWidthValue
                        : 0f;
                    float nextWidth = nextSelected
                        ? nextWidthValue
                        : 0f;

                    if (!TryBuildChamferFaceLine(
                            context.Graph,
                            previousEdgeIndex,
                            sourceFace.Normal,
                            faceCentre,
                            previousWidth,
                            out ChamferFaceLine previousLine) ||
                        !TryBuildChamferFaceLine(
                            context.Graph,
                            nextEdgeIndex,
                            sourceFace.Normal,
                            faceCentre,
                            nextWidth,
                            out ChamferFaceLine nextLine))
                    {
                        blocker = "failed to build a stable support line during chamfer width solving";
                        return false;
                    }

                    Vector3 sourceVertex =
                        context.Graph.Vertices[sourceVertexIndex].Position;
                    if (!TrySolveChamferFaceCorner(
                            sourceVertex,
                            previousLine,
                            nextLine,
                            sourceFace.Normal,
                            minimumStableEdgeLength * 0.001f,
                            out Vector3 solved) ||
                        !IsFinite(solved))
                    {
                        blocker = "failed to solve a finite corner during chamfer width solving";
                        return false;
                    }

                    corners.Add(
                        new ChamferFaceCornerKey(faceIndex, sourceVertexIndex),
                        new ChamferSolvedCorner(
                            solved,
                            faceIndex,
                            sourceVertexIndex,
                            previousEdgeIndex,
                            nextEdgeIndex,
                            previousSelected,
                            nextSelected));
                }
            }

            return true;
        }

        private static bool HasStableChamferSharedInterval(
            ChamferTopologyContext context,
            int edgeIndex,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            float minimumStableEdgeLength)
        {
            EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
            if (edge.FaceA < 0 || edge.FaceB < 0)
            {
                return true;
            }

            Vector3 sourceA = context.Graph.Vertices[edge.VertexA].Position;
            Vector3 sourceB = context.Graph.Vertices[edge.VertexB].Position;
            Vector3 edgeVector = sourceB - sourceA;
            float edgeLength = edgeVector.magnitude;
            if (edgeLength <= PointMergeDistance)
            {
                return false;
            }
            Vector3 direction = edgeVector / edgeLength;

            ChamferSolvedCorner aA = corners[
                new ChamferFaceCornerKey(edge.FaceA, edge.VertexA)];
            ChamferSolvedCorner aB = corners[
                new ChamferFaceCornerKey(edge.FaceA, edge.VertexB)];
            ChamferSolvedCorner bA = corners[
                new ChamferFaceCornerKey(edge.FaceB, edge.VertexA)];
            ChamferSolvedCorner bB = corners[
                new ChamferFaceCornerKey(edge.FaceB, edge.VertexB)];

            float a0 = Vector3.Dot(aA.Position - sourceA, direction);
            float a1 = Vector3.Dot(aB.Position - sourceA, direction);
            float b0 = Vector3.Dot(bA.Position - sourceA, direction);
            float b1 = Vector3.Dot(bB.Position - sourceA, direction);
            float sharedStart = Mathf.Max(Mathf.Min(a0, a1), Mathf.Min(b0, b1));
            float sharedEnd = Mathf.Min(Mathf.Max(a0, a1), Mathf.Max(b0, b1));
            float requiredSharedLength = Mathf.Min(
                minimumStableEdgeLength,
                edgeLength);
            return sharedEnd - sharedStart + PointMergeDistance >=
                requiredSharedLength;
        }

        private static HashSet<int> CollectChamferSharedEdgeParticipatingSelectedEdges(
            EdgeWearGraphEdge edge,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners)
        {
            HashSet<int> selectedEdges = new HashSet<int>();
            ChamferSolvedCorner[] relatedCorners =
            {
                corners[new ChamferFaceCornerKey(edge.FaceA, edge.VertexA)],
                corners[new ChamferFaceCornerKey(edge.FaceA, edge.VertexB)],
                corners[new ChamferFaceCornerKey(edge.FaceB, edge.VertexA)],
                corners[new ChamferFaceCornerKey(edge.FaceB, edge.VertexB)]
            };

            for (int i = 0; i < relatedCorners.Length; i++)
            {
                ChamferSolvedCorner corner = relatedCorners[i];
                if (corner.PreviousSelected)
                {
                    selectedEdges.Add(corner.PreviousSourceEdgeIndex);
                }
                if (corner.NextSelected)
                {
                    selectedEdges.Add(corner.NextSourceEdgeIndex);
                }
            }
            return selectedEdges;
        }

        private static bool TryFindChamferSharedEdgeWidthScale(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            int unselectedEdgeIndex,
            HashSet<int> participatingEdges,
            Dictionary<int, float> widthByEdge,
            float minimumStableEdgeLength,
            out float solvedScale,
            out string blocker)
        {
            blocker = string.Empty;
            solvedScale = 0f;
            Dictionary<int, float> testWidths =
                new Dictionary<int, float>(widthByEdge);

            foreach (int edgeIndex in participatingEdges)
            {
                testWidths[edgeIndex] = 0f;
            }
            if (!TryBuildChamferCornerTable(
                    sourceFaces,
                    context,
                    testWidths,
                    minimumStableEdgeLength,
                    out Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> zeroCorners,
                    out blocker) ||
                !HasStableChamferSharedInterval(
                    context,
                    unselectedEdgeIndex,
                    zeroCorners,
                    minimumStableEdgeLength))
            {
                return false;
            }

            float low = 0f;
            float high = 1f;
            for (int iteration = 0; iteration < 12; iteration++)
            {
                float middle = (low + high) * 0.5f;
                testWidths.Clear();
                foreach (KeyValuePair<int, float> pair in widthByEdge)
                {
                    testWidths.Add(pair.Key, pair.Value);
                }
                foreach (int edgeIndex in participatingEdges)
                {
                    testWidths[edgeIndex] = widthByEdge[edgeIndex] * middle;
                }

                if (!TryBuildChamferCornerTable(
                        sourceFaces,
                        context,
                        testWidths,
                        minimumStableEdgeLength,
                        out Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> testCorners,
                        out blocker))
                {
                    return false;
                }

                if (HasStableChamferSharedInterval(
                        context,
                        unselectedEdgeIndex,
                        testCorners,
                        minimumStableEdgeLength))
                {
                    low = middle;
                }
                else
                {
                    high = middle;
                }
            }

            solvedScale = low * 0.95f;
            return solvedScale > 0f;
        }

        private static void UpdateChamferInitialWorstCorner(
            int faceIndex,
            int sourceVertexIndex,
            int previousEdgeIndex,
            int nextEdgeIndex,
            float displacement,
            float limit,
            ref ChamferCornerStats stats)
        {
            if (displacement <= stats.InitialMaximumCornerDisplacement)
            {
                return;
            }

            stats.InitialMaximumCornerDisplacement = displacement;
            stats.InitialMaximumCornerDisplacementLimit = limit;
            stats.InitialWorstCornerFace = faceIndex;
            stats.InitialWorstCornerVertex = sourceVertexIndex;
            stats.InitialWorstCornerPreviousEdge = previousEdgeIndex;
            stats.InitialWorstCornerNextEdge = nextEdgeIndex;
        }

        private static bool TryClampChamferEdgeWidth(
            int edgeIndex,
            float scale,
            float requestedWidth,
            float minimumStableEdgeLength,
            Dictionary<int, float> widthByEdge,
            HashSet<int> clampedEdges,
            ref ChamferCornerStats stats)
        {
            float oldWidth = widthByEdge[edgeIndex];
            float newWidth = Mathf.Max(
                minimumStableEdgeLength,
                oldWidth * scale);
            if (newWidth >= oldWidth - PointMergeDistance)
            {
                return false;
            }

            widthByEdge[edgeIndex] = newWidth;
            clampedEdges.Add(edgeIndex);
            stats.CornerWidthClampApplications++;
            float relativeScale = requestedWidth > PointMergeDistance
                ? newWidth / requestedWidth
                : 1f;
            stats.MinimumCornerWidthScale = Mathf.Min(
                stats.MinimumCornerWidthScale,
                relativeScale);
            return true;
        }

        private static float CalculateChamferCornerDisplacementLimit(
            float requestedWidth,
            float minimumStableEdgeLength,
            float previousLength,
            float nextLength)
        {
            return Mathf.Max(
                requestedWidth * 4f,
                Mathf.Max(
                    minimumStableEdgeLength,
                    Mathf.Min(previousLength, nextLength) * 0.45f));
        }

        private static void UpdateChamferFinalWorstCorner(
            int faceIndex,
            int sourceVertexIndex,
            int previousEdgeIndex,
            int nextEdgeIndex,
            float displacement,
            float limit,
            ref ChamferCornerStats stats)
        {
            if (displacement <= stats.FinalMaximumCornerDisplacement)
            {
                return;
            }

            stats.FinalMaximumCornerDisplacement = displacement;
            stats.FinalMaximumCornerDisplacementLimit = limit;
            stats.FinalWorstCornerFace = faceIndex;
            stats.FinalWorstCornerVertex = sourceVertexIndex;
            stats.FinalWorstCornerPreviousEdge = previousEdgeIndex;
            stats.FinalWorstCornerNextEdge = nextEdgeIndex;
        }

        private static float CalculateChamferEdgeWidth(
            EdgeWearTopologyGraph graph,
            int edgeIndex,
            float requestedWidth,
            float minimumStableEdgeLength,
            out bool clamped)
        {
            EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
            float maximumWidth = requestedWidth;
            AccumulateChamferEndpointWidthLimit(
                graph,
                edge.FaceA,
                edge.VertexA,
                edgeIndex,
                ref maximumWidth);
            AccumulateChamferEndpointWidthLimit(
                graph,
                edge.FaceA,
                edge.VertexB,
                edgeIndex,
                ref maximumWidth);
            AccumulateChamferEndpointWidthLimit(
                graph,
                edge.FaceB,
                edge.VertexA,
                edgeIndex,
                ref maximumWidth);
            AccumulateChamferEndpointWidthLimit(
                graph,
                edge.FaceB,
                edge.VertexB,
                edgeIndex,
                ref maximumWidth);

            clamped = maximumWidth < requestedWidth - PointMergeDistance;
            return Mathf.Max(minimumStableEdgeLength, maximumWidth);
        }

        private static void AccumulateChamferEndpointWidthLimit(
            EdgeWearTopologyGraph graph,
            int faceIndex,
            int vertexIndex,
            int selectedEdgeIndex,
            ref float maximumWidth)
        {
            if (faceIndex < 0 || faceIndex >= graph.Faces.Count)
            {
                return;
            }

            EdgeWearGraphFace face = graph.Faces[faceIndex];
            int localIndex = face.VertexIndices.IndexOf(vertexIndex);
            if (localIndex < 0)
            {
                return;
            }

            int count = face.VertexIndices.Count;
            int previousEdge = face.EdgeIndices[(localIndex + count - 1) % count];
            int nextEdge = face.EdgeIndices[localIndex];
            int adjacentEdge = previousEdge == selectedEdgeIndex
                ? nextEdge
                : previousEdge;
            maximumWidth = Mathf.Min(
                maximumWidth,
                GetGraphEdgeLength(graph, adjacentEdge) * 0.25f);
        }

        private static float GetGraphEdgeLength(
            EdgeWearTopologyGraph graph,
            int edgeIndex)
        {
            EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
            return Vector3.Distance(
                graph.Vertices[edge.VertexA].Position,
                graph.Vertices[edge.VertexB].Position);
        }

        private static bool TryBuildChamferFaceLine(
            EdgeWearTopologyGraph graph,
            int edgeIndex,
            Vector3 faceNormal,
            Vector3 faceCentre,
            float offset,
            out ChamferFaceLine line)
        {
            EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
            Vector3 start = graph.Vertices[edge.VertexA].Position;
            Vector3 end = graph.Vertices[edge.VertexB].Position;
            Vector3 edgeVector = end - start;
            float length = edgeVector.magnitude;
            if (length <= PointMergeDistance || !IsFinite(edgeVector))
            {
                line = default;
                return false;
            }

            Vector3 direction = edgeVector / length;
            Vector3 inward = Vector3.Cross(faceNormal, direction).normalized;
            Vector3 midpoint = (start + end) * 0.5f;
            if (Vector3.Dot(faceCentre - midpoint, inward) < 0f)
            {
                inward = -inward;
            }

            line = new ChamferFaceLine(
                start + inward * offset,
                direction,
                edgeIndex,
                offset);
            return IsFinite(line.Point) && IsFinite(line.Direction);
        }

        private static bool TrySolveChamferFaceCorner(
            Vector3 sourceVertex,
            ChamferFaceLine previousLine,
            ChamferFaceLine nextLine,
            Vector3 faceNormal,
            float parallelTolerance,
            out Vector3 solved)
        {
            if (previousLine.Offset <= 0f && nextLine.Offset <= 0f)
            {
                solved = sourceVertex;
                return true;
            }

            float denominator = Vector3.Dot(
                Vector3.Cross(previousLine.Direction, nextLine.Direction),
                faceNormal);
            if (Mathf.Abs(denominator) <= parallelTolerance)
            {
                solved = Vector3.zero;
                return false;
            }

            float t = Vector3.Dot(
                Vector3.Cross(
                    nextLine.Point - previousLine.Point,
                    nextLine.Direction),
                faceNormal) / denominator;
            solved = previousLine.Point + previousLine.Direction * t;
            return IsFinite(solved);
        }

        private static bool TryReconcileChamferUnselectedInternalEdges(
            ChamferTopologyContext context,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            Dictionary<int, float> widthByEdge,
            float minimumStableEdgeLength,
            ref ChamferCornerStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            for (int edgeIndex = 0;
                 edgeIndex < context.Graph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                bool activeSelected = edge.Selected &&
                    widthByEdge.TryGetValue(edgeIndex, out float activeWidth) &&
                    activeWidth > PointMergeDistance;
                if (activeSelected || edge.FaceA < 0 || edge.FaceB < 0)
                {
                    continue;
                }

                stats.UnselectedInternalEdgeCount++;
                Vector3 sourceA = context.Graph.Vertices[edge.VertexA].Position;
                Vector3 sourceB = context.Graph.Vertices[edge.VertexB].Position;
                Vector3 edgeVector = sourceB - sourceA;
                float edgeLength = edgeVector.magnitude;
                if (edgeLength <= PointMergeDistance)
                {
                    stats.SharedUnselectedEndpointFailureCount++;
                    blocker = "an unselected internal source edge is degenerate";
                    return false;
                }
                Vector3 direction = edgeVector / edgeLength;

                ChamferSolvedCorner aA = corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexA)];
                ChamferSolvedCorner aB = corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexB)];
                ChamferSolvedCorner bA = corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexA)];
                ChamferSolvedCorner bB = corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexB)];

                stats.SharedUnselectedEndpointsChecked += 2;
                bool exactA = new VertexKey(aA.Position).Equals(new VertexKey(bA.Position));
                bool exactB = new VertexKey(aB.Position).Equals(new VertexKey(bB.Position));
                if (exactA && exactB)
                {
                    stats.SharedUnselectedEndpointsExact += 2;
                    continue;
                }

                float a0 = Vector3.Dot(aA.Position - sourceA, direction);
                float a1 = Vector3.Dot(aB.Position - sourceA, direction);
                float b0 = Vector3.Dot(bA.Position - sourceA, direction);
                float b1 = Vector3.Dot(bB.Position - sourceA, direction);
                float sharedStart = Mathf.Max(Mathf.Min(a0, a1), Mathf.Min(b0, b1));
                float sharedEnd = Mathf.Min(Mathf.Max(a0, a1), Mathf.Max(b0, b1));
                float requiredSharedLength = Mathf.Min(
                    minimumStableEdgeLength,
                    edgeLength);
                if (sharedEnd - sharedStart + PointMergeDistance <
                    requiredSharedLength)
                {
                    stats.SharedUnselectedEndpointFailureCount++;
                    blocker = "incident faces have no stable common interval on an unselected edge";
                    return false;
                }

                Vector3 pointA = sourceA + direction * sharedStart;
                Vector3 pointB = sourceA + direction * sharedEnd;
                aA.Position = pointA;
                bA.Position = pointA;
                aB.Position = pointB;
                bB.Position = pointB;
                stats.SharedUnselectedEndpointsReconciled += 2;
            }
            return true;
        }

        private static bool AuditChamferReplacementFaces(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref ChamferCornerStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            for (int faceIndex = 0;
                 faceIndex < context.Graph.Faces.Count;
                 faceIndex++)
            {
                EdgeWearGraphFace graphFace = context.Graph.Faces[faceIndex];
                PolygonFace sourceFace = sourceFaces[graphFace.SourceFaceIndex];
                List<Vector3> solvedFace = new List<Vector3>(
                    graphFace.VertexIndices.Count);
                for (int i = 0; i < graphFace.VertexIndices.Count; i++)
                {
                    solvedFace.Add(corners[
                        new ChamferFaceCornerKey(
                            faceIndex,
                            graphFace.VertexIndices[i])].Position);
                }

                if (CalculatePolygonArea(solvedFace) <= minimumStableFaceArea)
                {
                    stats.ReplacementFaceAreaFailureCount++;
                    blocker = "one or more hypothetical replacement faces have insufficient area";
                    return false;
                }
                Vector3 normal = CalculatePolygonNormal(solvedFace);
                if (!IsFinite(normal) || Vector3.Dot(normal, sourceFace.Normal) <= 0.25f)
                {
                    stats.ReplacementFaceWindingFailureCount++;
                    blocker = "one or more hypothetical replacement faces invert or lose stable winding";
                    return false;
                }
                for (int i = 0; i < solvedFace.Count; i++)
                {
                    Vector3 start = solvedFace[i];
                    Vector3 end = solvedFace[(i + 1) % solvedFace.Count];
                    int sourceEdgeIndex = graphFace.EdgeIndices[i];
                    float sourceLength = GetGraphEdgeLength(
                        context.Graph,
                        sourceEdgeIndex);
                    if ((end - start).magnitude < minimumStableEdgeLength &&
                        sourceLength >= minimumStableEdgeLength)
                    {
                        stats.ReplacementEdgeCollapseFailureCount++;
                        blocker = "a previously stable source edge collapses in a replacement face";
                        return false;
                    }
                }
                stats.ReplacementFacesValid++;
            }
            return true;
        }

        private static bool AuditChamferSelectedRails(
            ChamferTopologyContext context,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            Dictionary<int, float> widthByEdge,
            float minimumStableEdgeLength,
            ref ChamferCornerStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            foreach (int edgeIndex in context.SelectedSourceEdges)
            {
                if (!widthByEdge.TryGetValue(edgeIndex, out float activeWidth) ||
                    activeWidth <= PointMergeDistance)
                {
                    continue;
                }

                EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                stats.SelectedRailsChecked++;
                Vector3 a0 = corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexA)].Position;
                Vector3 b0 = corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexB)].Position;
                Vector3 a1 = corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexA)].Position;
                Vector3 b1 = corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexB)].Position;

                if (Vector3.Distance(a0, a1) < minimumStableEdgeLength ||
                    Vector3.Distance(b0, b1) < minimumStableEdgeLength)
                {
                    stats.SelectedRailSpanFailureCount++;
                    blocker = "one or more selected edge strips have insufficient endpoint span";
                    return false;
                }
                if (Vector3.Distance(a0, b0) < minimumStableEdgeLength ||
                    Vector3.Distance(a1, b1) < minimumStableEdgeLength)
                {
                    stats.SelectedRailLengthFailureCount++;
                    blocker = "one or more selected edge strips have insufficient rail length";
                    return false;
                }
                stats.SelectedRailsValid++;
            }
            return true;
        }

        private static bool AuditChamferSolvedBoundary(
            ChamferTopologyContext context,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            float minimumStableEdgeLength,
            ref ChamferCornerStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            for (int edgeIndex = 0;
                 edgeIndex < context.Graph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                if (edge.FaceA >= 0 && edge.FaceB >= 0)
                {
                    continue;
                }
                int faceIndex = edge.FaceA >= 0 ? edge.FaceA : edge.FaceB;
                stats.SourceBoundaryEdgeCount++;
                Vector3 a = corners[
                    new ChamferFaceCornerKey(faceIndex, edge.VertexA)].Position;
                Vector3 b = corners[
                    new ChamferFaceCornerKey(faceIndex, edge.VertexB)].Position;
                float sourceLength = GetGraphEdgeLength(
                    context.Graph,
                    edgeIndex);
                if (Vector3.Distance(a, b) < minimumStableEdgeLength &&
                    sourceLength >= minimumStableEdgeLength)
                {
                    stats.SolvedBoundaryLoopFailureCount++;
                    blocker = "one or more preserved source-boundary edges collapse after corner solving";
                    return false;
                }
                stats.SolvedBoundaryEdgeCount++;
            }
            return true;
        }

        private static bool AuditProvisionalChamferEmission(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            ChamferCornerSolution solution,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref ChamferEmissionStats stats,
            out HashSet<int> conflictingEdges,
            out string blocker)
        {
            conflictingEdges = new HashSet<int>();
            blocker = string.Empty;
            stats.SourceFaceCount = context.Graph.Faces.Count;
            stats.CandidateSelectedEdgeCount = context.SelectedSourceEdges.Count;
            List<PolygonFace> provisionalFaces = new List<PolygonFace>(
                context.Graph.Faces.Count + context.SelectedSourceEdges.Count);
            HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges =
                new HashSet<TopologyEdgeKey>();
            List<ChamferStripEndpointBoundary> endpointBoundaries =
                new List<ChamferStripEndpointBoundary>(
                    context.SelectedSourceEdges.Count * 2);
            HashSet<TopologyEdgeKey> expectedVertexBoundaryEdges =
                new HashSet<TopologyEdgeKey>();

            for (int faceIndex = 0;
                 faceIndex < context.Graph.Faces.Count;
                 faceIndex++)
            {
                stats.ReplacementFacesAttempted++;
                EdgeWearGraphFace graphFace = context.Graph.Faces[faceIndex];
                PolygonFace sourceFace = sourceFaces[graphFace.SourceFaceIndex];
                List<Vector3> vertices = new List<Vector3>(
                    graphFace.VertexIndices.Count);
                for (int i = 0; i < graphFace.VertexIndices.Count; i++)
                {
                    ChamferFaceCornerKey key = new ChamferFaceCornerKey(
                        faceIndex,
                        graphFace.VertexIndices[i]);
                    if (!solution.Corners.TryGetValue(
                            key,
                            out ChamferSolvedCorner corner))
                    {
                        stats.ReplacementFaceFailureCount++;
                        blocker = "a replacement face is missing a solved corner";
                        return false;
                    }
                    vertices.Add(corner.Position);
                }

                if (CalculatePolygonArea(vertices) <= minimumStableFaceArea ||
                    !IsFinite(CalculatePolygonNormal(vertices)))
                {
                    stats.ReplacementFaceFailureCount++;
                    blocker = "a provisional replacement face is geometrically invalid";
                    return false;
                }

                Vector3 replacementNormal = CalculatePolygonNormal(vertices);
                if (Vector3.Dot(replacementNormal, sourceFace.Normal) <= 0.25f)
                {
                    stats.ReplacementFaceFailureCount++;
                    blocker = "a provisional replacement face has invalid winding";
                    return false;
                }

                provisionalFaces.Add(new PolygonFace(
                    vertices,
                    sourceFace.Normal,
                    sourceFace.Feature,
                    sourceFace.FeatureStrength));
                stats.ReplacementFacesBuilt++;
            }

            Dictionary<int, EdgeWearSelectedGraphEdge> selectedByGraphEdge =
                new Dictionary<int, EdgeWearSelectedGraphEdge>();
            for (int i = 0; i < context.SelectedEdges.Count; i++)
            {
                selectedByGraphEdge[context.SelectedEdges[i].GraphEdgeIndex] =
                    context.SelectedEdges[i];
            }

            foreach (int edgeIndex in context.SelectedSourceEdges)
            {
                if (!solution.WidthByEdge.TryGetValue(edgeIndex, out float width) ||
                    width <= PointMergeDistance)
                {
                    stats.DeferredSelectedEdgeCount++;
                    continue;
                }

                stats.ActiveSelectedEdgeCount++;
                stats.BevelStripsAttempted++;
                EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                if (edge.FaceA < 0 || edge.FaceB < 0 ||
                    !selectedByGraphEdge.TryGetValue(
                        edgeIndex,
                        out EdgeWearSelectedGraphEdge selected))
                {
                    stats.BevelStripFailureCount++;
                    blocker = "an active selected edge lacks two incident faces or candidate provenance";
                    return false;
                }

                Vector3 a0 = solution.Corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexA)].Position;
                Vector3 b0 = solution.Corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexB)].Position;
                Vector3 a1 = solution.Corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexA)].Position;
                Vector3 b1 = solution.Corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexB)].Position;

                List<Vector3> strip = new List<Vector3> { a0, b0, b1, a1 };
                Vector3 expectedNormal = selected.Candidate.BevelNormal;
                Vector3 stripNormal = CalculatePolygonNormal(strip);
                if (!IsFinite(stripNormal) || stripNormal.sqrMagnitude <= 0.00000001f)
                {
                    stats.BevelStripFailureCount++;
                    blocker = "an active selected edge produces an invalid bevel strip normal";
                    return false;
                }
                if (Vector3.Dot(stripNormal, expectedNormal) < 0f)
                {
                    strip.Reverse();
                    stripNormal = -stripNormal;
                }
                if (CalculatePolygonArea(strip) <= minimumStableFaceArea)
                {
                    stats.BevelStripFailureCount++;
                    blocker = "an active selected edge produces an insufficient bevel-strip area";
                    return false;
                }

                provisionalFaces.Add(new PolygonFace(
                    strip,
                    stripNormal,
                    PolygonFaceFeature.ConvexEdgeWear,
                    selected.Candidate.Strength));
                stats.BevelStripsBuilt++;
                stats.BevelStripQuadFaceCount++;
                stats.BevelStripTriangleEstimate += 2;

                TopologyEdgeKey boundaryAtA = new TopologyEdgeKey(
                    new VertexKey(a0),
                    new VertexKey(a1));
                TopologyEdgeKey boundaryAtB = new TopologyEdgeKey(
                    new VertexKey(b0),
                    new VertexKey(b1));
                endpointBoundaries.Add(new ChamferStripEndpointBoundary(
                    edge.VertexA,
                    edgeIndex,
                    edge.FaceA,
                    edge.FaceB,
                    boundaryAtA));
                endpointBoundaries.Add(new ChamferStripEndpointBoundary(
                    edge.VertexB,
                    edgeIndex,
                    edge.FaceA,
                    edge.FaceB,
                    boundaryAtB));
                expectedVertexBoundaryEdges.Add(boundaryAtA);
                expectedVertexBoundaryEdges.Add(boundaryAtB);
            }

            stats.StripEndpointBoundaryRegistrationCount =
                endpointBoundaries.Count;
            Dictionary<TopologyEdgeKey, List<ChamferStripEndpointBoundary>>
                endpointBoundariesByKey =
                    new Dictionary<TopologyEdgeKey, List<ChamferStripEndpointBoundary>>();
            for (int i = 0; i < endpointBoundaries.Count; i++)
            {
                ChamferStripEndpointBoundary boundary = endpointBoundaries[i];
                if (!endpointBoundariesByKey.TryGetValue(
                        boundary.Key,
                        out List<ChamferStripEndpointBoundary> owners))
                {
                    owners = new List<ChamferStripEndpointBoundary>();
                    endpointBoundariesByKey.Add(boundary.Key, owners);
                }
                owners.Add(boundary);
            }

            HashSet<int> duplicateBoundaryVertices = new HashSet<int>();
            foreach (KeyValuePair<TopologyEdgeKey, List<ChamferStripEndpointBoundary>> pair
                     in endpointBoundariesByKey)
            {
                List<ChamferStripEndpointBoundary> owners = pair.Value;
                if (owners.Count <= 1)
                {
                    continue;
                }

                stats.DuplicateStripEndpointBoundaryKeyCount++;
                stats.DuplicateStripEndpointBoundaryRegistrationCount +=
                    owners.Count - 1;
                for (int i = 0; i < owners.Count; i++)
                {
                    duplicateBoundaryVertices.Add(owners[i].SourceVertexIndex);
                }

                int keeperEdge = owners[0].SourceEdgeIndex;
                for (int i = 1; i < owners.Count; i++)
                {
                    keeperEdge = ChooseChamferBoundaryConflictKeeper(
                        context,
                        keeperEdge,
                        owners[i].SourceEdgeIndex);
                }
                for (int i = 0; i < owners.Count; i++)
                {
                    if (owners[i].SourceEdgeIndex != keeperEdge)
                    {
                        conflictingEdges.Add(owners[i].SourceEdgeIndex);
                    }
                }
            }
            stats.DuplicateBoundaryVertexCount = duplicateBoundaryVertices.Count;

            AuditActiveChamferRuns(
                context,
                solution.WidthByEdge,
                ref stats);

            if (conflictingEdges.Count > 0)
            {
                blocker =
                    "duplicate active strip-end boundaries require deterministic local edge deferral";
                return false;
            }

            for (int edgeIndex = 0;
                 edgeIndex < context.Graph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                if (edge.FaceA >= 0 && edge.FaceB >= 0)
                {
                    continue;
                }
                int faceIndex = edge.FaceA >= 0 ? edge.FaceA : edge.FaceB;
                Vector3 a = solution.Corners[
                    new ChamferFaceCornerKey(faceIndex, edge.VertexA)].Position;
                Vector3 b = solution.Corners[
                    new ChamferFaceCornerKey(faceIndex, edge.VertexB)].Position;
                expectedSourceBoundaryEdges.Add(new TopologyEdgeKey(
                    new VertexKey(a),
                    new VertexKey(b)));
            }

            stats.ExpectedSourceBoundaryEdgeCount =
                expectedSourceBoundaryEdges.Count;
            stats.ExpectedVertexBoundaryEdgeCount =
                expectedVertexBoundaryEdges.Count;

            Dictionary<TopologyEdgeKey, int> useCounts =
                new Dictionary<TopologyEdgeKey, int>();
            for (int faceIndex = 0; faceIndex < provisionalFaces.Count; faceIndex++)
            {
                List<Vector3> vertices = provisionalFaces[faceIndex].Vertices;
                for (int i = 0; i < vertices.Count; i++)
                {
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        new VertexKey(vertices[i]),
                        new VertexKey(vertices[(i + 1) % vertices.Count]));
                    useCounts.TryGetValue(key, out int useCount);
                    useCounts[key] = useCount + 1;
                }
            }

            HashSet<TopologyEdgeKey> actualOpenEdges =
                new HashSet<TopologyEdgeKey>();
            foreach (KeyValuePair<TopologyEdgeKey, int> pair in useCounts)
            {
                if (pair.Value == 1)
                {
                    actualOpenEdges.Add(pair.Key);
                }
                else if (pair.Value > 2)
                {
                    stats.ProvisionalNonManifoldEdgeCount++;
                }
            }
            stats.ProvisionalOpenEdgeCount = actualOpenEdges.Count;

            foreach (TopologyEdgeKey key in expectedSourceBoundaryEdges)
            {
                if (actualOpenEdges.Contains(key))
                {
                    stats.MatchedSourceBoundaryEdgeCount++;
                }
            }
            foreach (TopologyEdgeKey key in expectedVertexBoundaryEdges)
            {
                if (actualOpenEdges.Contains(key))
                {
                    stats.MatchedVertexBoundaryEdgeCount++;
                }
                else
                {
                    stats.MissingExpectedVertexBoundaryEdgeCount++;
                }
            }
            foreach (TopologyEdgeKey key in actualOpenEdges)
            {
                if (!expectedSourceBoundaryEdges.Contains(key) &&
                    !expectedVertexBoundaryEdges.Contains(key))
                {
                    stats.UnexpectedProvisionalOpenEdgeCount++;
                }
            }

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                provisionalFaces,
                minimumStableEdgeLength);
            stats.ProvisionalTJunctionCount = topology.TJunctionCount;
            stats.ProvisionalNonManifoldEdgeCount = Mathf.Max(
                stats.ProvisionalNonManifoldEdgeCount,
                topology.NonManifoldEdgeCount);

            if (stats.MatchedSourceBoundaryEdgeCount !=
                    stats.ExpectedSourceBoundaryEdgeCount ||
                stats.MissingExpectedVertexBoundaryEdgeCount > 0 ||
                stats.UnexpectedProvisionalOpenEdgeCount > 0 ||
                stats.ProvisionalNonManifoldEdgeCount > 0 ||
                stats.ProvisionalTJunctionCount > 0)
            {
                blocker = "provisional chamfer topology does not match the explicit source-boundary and vertex-patch boundary contract";
                return false;
            }

            stats.ReadyForVertexPatches = 1;
            return true;
        }

        private static int ChooseChamferBoundaryConflictKeeper(
            ChamferTopologyContext context,
            int firstEdgeIndex,
            int secondEdgeIndex)
        {
            EdgeWearSelectedGraphEdge first = default;
            EdgeWearSelectedGraphEdge second = default;
            bool foundFirst = false;
            bool foundSecond = false;
            for (int i = 0; i < context.SelectedEdges.Count; i++)
            {
                EdgeWearSelectedGraphEdge selected = context.SelectedEdges[i];
                if (selected.GraphEdgeIndex == firstEdgeIndex)
                {
                    first = selected;
                    foundFirst = true;
                }
                if (selected.GraphEdgeIndex == secondEdgeIndex)
                {
                    second = selected;
                    foundSecond = true;
                }
            }

            if (foundFirst && foundSecond)
            {
                if (!Mathf.Approximately(
                        first.Candidate.Strength,
                        second.Candidate.Strength))
                {
                    return first.Candidate.Strength > second.Candidate.Strength
                        ? firstEdgeIndex
                        : secondEdgeIndex;
                }

                float firstLength = GetGraphEdgeLength(
                    context.Graph,
                    firstEdgeIndex);
                float secondLength = GetGraphEdgeLength(
                    context.Graph,
                    secondEdgeIndex);
                if (!Mathf.Approximately(firstLength, secondLength))
                {
                    return firstLength > secondLength
                        ? firstEdgeIndex
                        : secondEdgeIndex;
                }
            }

            return Mathf.Min(firstEdgeIndex, secondEdgeIndex);
        }

        private static void AuditActiveChamferRuns(
            ChamferTopologyContext context,
            Dictionary<int, float> widthByEdge,
            ref ChamferEmissionStats stats)
        {
            List<List<int>> outgoingByVertex =
                new List<List<int>>(context.Graph.Vertices.Count);
            for (int i = 0; i < context.Graph.Vertices.Count; i++)
            {
                outgoingByVertex.Add(new List<int>());
            }
            for (int i = 0; i < context.HalfEdges.Count; i++)
            {
                outgoingByVertex[context.HalfEdges[i].OriginVertex].Add(i);
            }

            for (int vertexIndex = 0;
                 vertexIndex < outgoingByVertex.Count;
                 vertexIndex++)
            {
                List<int> outgoing = outgoingByVertex[vertexIndex];
                if (outgoing.Count == 0)
                {
                    continue;
                }

                int start = -1;
                bool openFan = false;
                for (int i = 0; i < outgoing.Count; i++)
                {
                    ChamferHalfEdge candidate = context.HalfEdges[outgoing[i]];
                    int previousOpposite =
                        context.HalfEdges[candidate.Previous].Opposite;
                    if (previousOpposite < 0)
                    {
                        start = candidate.Index;
                        openFan = true;
                        break;
                    }
                }
                if (start < 0)
                {
                    start = outgoing[0];
                }

                List<int> ordered = new List<int>(outgoing.Count);
                HashSet<int> visited = new HashSet<int>();
                int current = start;
                int guard = 0;
                while (current >= 0 && guard++ <= outgoing.Count)
                {
                    if (!visited.Add(current))
                    {
                        break;
                    }
                    ordered.Add(current);
                    ChamferHalfEdge halfEdge = context.HalfEdges[current];
                    int next = halfEdge.Opposite >= 0
                        ? context.HalfEdges[halfEdge.Opposite].Next
                        : -1;
                    if (next < 0 || next == start)
                    {
                        break;
                    }
                    current = next;
                }
                if (ordered.Count != outgoing.Count)
                {
                    continue;
                }

                bool anyActive = false;
                int runCount = 0;
                bool previousActive = openFan
                    ? false
                    : IsChamferHalfEdgeActive(
                        context.HalfEdges[ordered[ordered.Count - 1]],
                        widthByEdge);
                int activeCount = 0;
                for (int i = 0; i < ordered.Count; i++)
                {
                    bool active = IsChamferHalfEdgeActive(
                        context.HalfEdges[ordered[i]],
                        widthByEdge);
                    if (active)
                    {
                        anyActive = true;
                        activeCount++;
                        if (!previousActive)
                        {
                            runCount++;
                        }
                    }
                    previousActive = active;
                }
                if (!openFan && activeCount == ordered.Count && activeCount > 0)
                {
                    runCount = 1;
                }
                if (!anyActive)
                {
                    continue;
                }

                stats.ActiveAffectedVertexCount++;
                stats.ActiveSelectedRunCount += runCount;
                if (openFan)
                {
                    stats.ActiveOpenRunCount += runCount;
                }
                else
                {
                    stats.ActiveClosedRunCount += runCount;
                }
                if (runCount > 1)
                {
                    stats.ActiveMultipleRunVertexCount++;
                }
                if (activeCount == 1)
                {
                    stats.ActiveIsolatedEdgeRunCount++;
                }
            }
        }

        private static bool IsChamferHalfEdgeActive(
            ChamferHalfEdge halfEdge,
            Dictionary<int, float> widthByEdge)
        {
            return widthByEdge.TryGetValue(
                    halfEdge.SourceEdgeIndex,
                    out float width) &&
                width > PointMergeDistance;
        }

        private static void LogChamferEmissionAudit(
            ChamferEmissionStats stats,
            bool ready,
            string blocker)
        {
#if UNITY_EDITOR
            string message =
                "GeneratedMass edge wear provisional chamfer emission audit complete. " +
                stats.ToSummaryString() +
                ", geometryEmission=provisional, geometryCommit=disabled";
            if (!string.IsNullOrEmpty(blocker))
            {
                message += ", blocker=" + blocker;
            }
            if (ready)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogWarning(message);
            }
#endif
        }

        private static void LogChamferCornerAudit(
            ChamferCornerStats stats,
            bool ready,
            string blocker)
        {
#if UNITY_EDITOR
            string message =
                "GeneratedMass edge wear chamfer corner audit complete. " +
                stats.ToSummaryString() +
                ", geometryEmission=disabled";
            if (!string.IsNullOrEmpty(blocker))
            {
                message += ", blocker=" + blocker;
            }
            if (ready)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogWarning(message);
            }
#endif
        }

private static void LogChamferReadiness(
            ChamferReadinessStats stats,
            bool ready,
            string blocker)
        {
#if UNITY_EDITOR
            string message =
                "GeneratedMass edge wear chamfer readiness audit complete. " +
                stats.ToSummaryString() +
                ", geometryEmission=disabled";
            if (!string.IsNullOrEmpty(blocker))
            {
                message += ", blocker=" + blocker;
            }

            if (ready)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogWarning(message);
            }
#endif
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
        private static bool TryCalculateRawPolygonNormal(
            List<Vector3> vertices,
            float minimumStableEdgeLength,
            out Vector3 normal)
        {
            normal = Vector3.zero;
            if (vertices == null || vertices.Count < 3)
            {
                return false;
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 next = vertices[(i + 1) % vertices.Count];

                normal.x += (current.y - next.y) * (current.z + next.z);
                normal.y += (current.z - next.z) * (current.x + next.x);
                normal.z += (current.x - next.x) * (current.y + next.y);
            }

            float normalSqr = normal.sqrMagnitude;
            float minimumNormalSqr = minimumStableEdgeLength * minimumStableEdgeLength;
            if (normalSqr <= minimumNormalSqr || !IsFinite(normal))
            {
                normal = Vector3.zero;
                return false;
            }

            normal /= Mathf.Sqrt(normalSqr);
            return true;
        }

        private static bool AreWithinDistance(
            Vector3 left,
            Vector3 right,
            float distance)
        {
            return (left - right).sqrMagnitude <= distance * distance;
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
        private static bool IsFinite(Vector3 value)
        {
            return !(float.IsNaN(value.x) ||
                float.IsNaN(value.y) ||
                float.IsNaN(value.z) ||
                float.IsInfinity(value.x) ||
                float.IsInfinity(value.y) ||
                float.IsInfinity(value.z));
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

            float tolerance = CalculateTopologyTJunctionTolerance(
                minimumStableEdgeLength);
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

        private static float CalculateTopologyTJunctionTolerance(
            float minimumStableEdgeLength)
        {
            return Mathf.Max(
                PointMergeDistance * 4f,
                minimumStableEdgeLength * 0.04f);
        }

        private static bool AreSamePoint(Vector3 left, Vector3 right)
        {
            return (left - right).sqrMagnitude <= PointMergeDistanceSqr;
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
        private sealed class ChamferTopologyContext
        {
            public readonly EdgeWearTopologyGraph Graph;
            public readonly List<EdgeWearSelectedGraphEdge> SelectedEdges;
            public readonly List<ChamferHalfEdge> HalfEdges;
            public readonly HashSet<int> SelectedSourceEdges;

            public ChamferTopologyContext(
                EdgeWearTopologyGraph graph,
                List<EdgeWearSelectedGraphEdge> selectedEdges,
                List<ChamferHalfEdge> halfEdges)
            {
                Graph = graph;
                SelectedEdges = selectedEdges;
                HalfEdges = halfEdges;
                SelectedSourceEdges = new HashSet<int>();
                for (int i = 0; i < selectedEdges.Count; i++)
                {
                    SelectedSourceEdges.Add(selectedEdges[i].GraphEdgeIndex);
                }
            }
        }

        private readonly struct ChamferFaceCornerKey :
            IEquatable<ChamferFaceCornerKey>
        {
            public readonly int FaceIndex;
            public readonly int SourceVertexIndex;

            public ChamferFaceCornerKey(int faceIndex, int sourceVertexIndex)
            {
                FaceIndex = faceIndex;
                SourceVertexIndex = sourceVertexIndex;
            }

            public bool Equals(ChamferFaceCornerKey other)
            {
                return FaceIndex == other.FaceIndex &&
                    SourceVertexIndex == other.SourceVertexIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is ChamferFaceCornerKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (FaceIndex * 397) ^ SourceVertexIndex;
                }
            }
        }

        private sealed class ChamferSolvedCorner
        {
            public Vector3 Position;
            public readonly int FaceIndex;
            public readonly int SourceVertexIndex;
            public readonly int PreviousSourceEdgeIndex;
            public readonly int NextSourceEdgeIndex;
            public readonly bool PreviousSelected;
            public readonly bool NextSelected;

            public ChamferSolvedCorner(
                Vector3 position,
                int faceIndex,
                int sourceVertexIndex,
                int previousSourceEdgeIndex,
                int nextSourceEdgeIndex,
                bool previousSelected,
                bool nextSelected)
            {
                Position = position;
                FaceIndex = faceIndex;
                SourceVertexIndex = sourceVertexIndex;
                PreviousSourceEdgeIndex = previousSourceEdgeIndex;
                NextSourceEdgeIndex = nextSourceEdgeIndex;
                PreviousSelected = previousSelected;
                NextSelected = nextSelected;
            }
        }

        private sealed class ChamferCornerSolution
        {
            public readonly Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> Corners;
            public readonly Dictionary<int, float> WidthByEdge;

            public ChamferCornerSolution(
                Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
                Dictionary<int, float> widthByEdge)
            {
                Corners = corners;
                WidthByEdge = widthByEdge;
            }
        }

        private readonly struct ChamferFaceLine
        {
            public readonly Vector3 Point;
            public readonly Vector3 Direction;
            public readonly int SourceEdgeIndex;
            public readonly float Offset;

            public ChamferFaceLine(
                Vector3 point,
                Vector3 direction,
                int sourceEdgeIndex,
                float offset)
            {
                Point = point;
                Direction = direction;
                SourceEdgeIndex = sourceEdgeIndex;
                Offset = offset;
            }
        }

        private readonly struct ChamferStripEndpointBoundary
        {
            public readonly int SourceVertexIndex;
            public readonly int SourceEdgeIndex;
            public readonly int FaceA;
            public readonly int FaceB;
            public readonly TopologyEdgeKey Key;

            public ChamferStripEndpointBoundary(
                int sourceVertexIndex,
                int sourceEdgeIndex,
                int faceA,
                int faceB,
                TopologyEdgeKey key)
            {
                SourceVertexIndex = sourceVertexIndex;
                SourceEdgeIndex = sourceEdgeIndex;
                FaceA = faceA;
                FaceB = faceB;
                Key = key;
            }
        }

        private struct ChamferEmissionStats
        {
            public int SourceFaceCount;
            public int ReplacementFacesAttempted;
            public int ReplacementFacesBuilt;
            public int ReplacementFaceFailureCount;
            public int CandidateSelectedEdgeCount;
            public int ActiveSelectedEdgeCount;
            public int DeferredSelectedEdgeCount;
            public int BevelStripsAttempted;
            public int BevelStripsBuilt;
            public int BevelStripFailureCount;
            public int BevelStripQuadFaceCount;
            public int BevelStripTriangleEstimate;
            public int ExpectedSourceBoundaryEdgeCount;
            public int MatchedSourceBoundaryEdgeCount;
            public int ExpectedVertexBoundaryEdgeCount;
            public int MatchedVertexBoundaryEdgeCount;
            public int ProvisionalOpenEdgeCount;
            public int UnexpectedProvisionalOpenEdgeCount;
            public int MissingExpectedVertexBoundaryEdgeCount;
            public int ProvisionalNonManifoldEdgeCount;
            public int ProvisionalTJunctionCount;
            public int StripEndpointBoundaryRegistrationCount;
            public int DuplicateStripEndpointBoundaryKeyCount;
            public int DuplicateStripEndpointBoundaryRegistrationCount;
            public int DuplicateBoundaryVertexCount;
            public int ConflictDeferredEdgeCount;
            public int CompatibilityPassCount;
            public int ActiveAffectedVertexCount;
            public int ActiveSelectedRunCount;
            public int ActiveClosedRunCount;
            public int ActiveOpenRunCount;
            public int ActiveMultipleRunVertexCount;
            public int ActiveIsolatedEdgeRunCount;
            public int ReadyForVertexPatches;

            public string ToSummaryString()
            {
                return
                    "sourceFaces=" + SourceFaceCount +
                    ", replacementFacesAttempted=" + ReplacementFacesAttempted +
                    ", replacementFacesBuilt=" + ReplacementFacesBuilt +
                    ", replacementFaceFailures=" + ReplacementFaceFailureCount +
                    ", candidateSelectedEdges=" + CandidateSelectedEdgeCount +
                    ", activeSelectedEdges=" + ActiveSelectedEdgeCount +
                    ", deferredSelectedEdges=" + DeferredSelectedEdgeCount +
                    ", bevelStripsAttempted=" + BevelStripsAttempted +
                    ", bevelStripsBuilt=" + BevelStripsBuilt +
                    ", bevelStripFailures=" + BevelStripFailureCount +
                    ", bevelStripQuadFaces=" + BevelStripQuadFaceCount +
                    ", bevelStripTriangleEstimate=" + BevelStripTriangleEstimate +
                    ", expectedSourceBoundaryEdges=" + ExpectedSourceBoundaryEdgeCount +
                    ", matchedSourceBoundaryEdges=" + MatchedSourceBoundaryEdgeCount +
                    ", expectedVertexBoundaryEdges=" + ExpectedVertexBoundaryEdgeCount +
                    ", matchedVertexBoundaryEdges=" + MatchedVertexBoundaryEdgeCount +
                    ", provisionalOpenEdges=" + ProvisionalOpenEdgeCount +
                    ", unexpectedProvisionalOpenEdges=" + UnexpectedProvisionalOpenEdgeCount +
                    ", missingExpectedVertexBoundaryEdges=" + MissingExpectedVertexBoundaryEdgeCount +
                    ", provisionalNonManifoldEdges=" + ProvisionalNonManifoldEdgeCount +
                    ", provisionalTJunctions=" + ProvisionalTJunctionCount +
                    ", stripEndpointBoundaryRegistrations=" + StripEndpointBoundaryRegistrationCount +
                    ", duplicateStripEndpointBoundaryKeys=" + DuplicateStripEndpointBoundaryKeyCount +
                    ", duplicateStripEndpointBoundaryRegistrations=" + DuplicateStripEndpointBoundaryRegistrationCount +
                    ", duplicateBoundaryVertices=" + DuplicateBoundaryVertexCount +
                    ", conflictDeferredEdges=" + ConflictDeferredEdgeCount +
                    ", compatibilityPasses=" + CompatibilityPassCount +
                    ", activeAffectedVertices=" + ActiveAffectedVertexCount +
                    ", activeSelectedRuns=" + ActiveSelectedRunCount +
                    ", activeClosedRuns=" + ActiveClosedRunCount +
                    ", activeOpenRuns=" + ActiveOpenRunCount +
                    ", activeMultipleRunVertices=" + ActiveMultipleRunVertexCount +
                    ", activeIsolatedEdgeRuns=" + ActiveIsolatedEdgeRunCount +
                    ", readyForVertexPatches=" + ReadyForVertexPatches;
            }
        }

        private struct ChamferCornerStats
        {
            public int SourceFaceCount;
            public int ExpectedCornerCount;
            public int SolvedCornerCount;
            public int PreservedCornerCount;
            public int SingleSelectedCornerCount;
            public int DoubleSelectedCornerCount;
            public int SelectedEdgeCount;
            public int ActiveSelectedEdgeCount;
            public int DeferredSelectedEdgeCount;
            public float RequestedWidth;
            public float MinimumSolvedWidth;
            public float MaximumSolvedWidth;
            public int WidthClampedEdges;
            public int WidthSolveFailures;
            public int CornerSolveFailures;
            public int NonFiniteCornerCount;
            public int ExcessiveDisplacementCornerCount;
            public int CornerWidthSolvePasses;
            public int CornerWidthClampApplications;
            public int CornerWidthClampedEdges;
            public float MinimumCornerWidthScale;
            public float InitialMaximumCornerDisplacement;
            public float InitialMaximumCornerDisplacementLimit;
            public int InitialWorstCornerFace;
            public int InitialWorstCornerVertex;
            public int InitialWorstCornerPreviousEdge;
            public int InitialWorstCornerNextEdge;
            public float FinalMaximumCornerDisplacement;
            public float FinalMaximumCornerDisplacementLimit;
            public int FinalWorstCornerFace;
            public int FinalWorstCornerVertex;
            public int FinalWorstCornerPreviousEdge;
            public int FinalWorstCornerNextEdge;
            public int CornerWidthConvergenceFailures;
            public int CornerWidthBelowMinimumFailures;
            public int SharedEdgeWidthClampApplications;
            public int SharedEdgeWidthClampedEdges;
            public float MinimumSharedEdgeWidthScale;
            public int SharedEdgeWidthConvergenceFailures;
            public int SharedEdgeWidthBelowMinimumFailures;
            public int SharedEdgeWidthDeferredEdges;
            public int ReplacementFacesValid;
            public int ReplacementFaceAreaFailureCount;
            public int ReplacementFaceWindingFailureCount;
            public int ReplacementEdgeCollapseFailureCount;
            public int UnselectedInternalEdgeCount;
            public int SharedUnselectedEndpointsChecked;
            public int SharedUnselectedEndpointsExact;
            public int SharedUnselectedEndpointsReconciled;
            public int SharedUnselectedEndpointFailureCount;
            public int SelectedRailsChecked;
            public int SelectedRailsValid;
            public int SelectedRailSpanFailureCount;
            public int SelectedRailLengthFailureCount;
            public int SourceBoundaryEdgeCount;
            public int SolvedBoundaryEdgeCount;
            public int SolvedBoundaryLoopFailureCount;
            public int ReadyForEmission;

            public string ToSummaryString()
            {
                return
                    "sourceFaces=" + SourceFaceCount +
                    ", expectedCorners=" + ExpectedCornerCount +
                    ", solvedCorners=" + SolvedCornerCount +
                    ", preservedCorners=" + PreservedCornerCount +
                    ", singleSelectedCorners=" + SingleSelectedCornerCount +
                    ", doubleSelectedCorners=" + DoubleSelectedCornerCount +
                    ", selectedEdges=" + SelectedEdgeCount +
                    ", activeSelectedEdges=" + ActiveSelectedEdgeCount +
                    ", deferredSelectedEdges=" + DeferredSelectedEdgeCount +
                    ", requestedWidth=" + RequestedWidth.ToString("F6") +
                    ", minimumSolvedWidth=" + MinimumSolvedWidth.ToString("F6") +
                    ", maximumSolvedWidth=" + MaximumSolvedWidth.ToString("F6") +
                    ", widthClampedEdges=" + WidthClampedEdges +
                    ", widthSolveFailures=" + WidthSolveFailures +
                    ", cornerSolveFailures=" + CornerSolveFailures +
                    ", nonFiniteCorners=" + NonFiniteCornerCount +
                    ", excessiveDisplacementCorners=" + ExcessiveDisplacementCornerCount +
                    ", cornerWidthSolvePasses=" + CornerWidthSolvePasses +
                    ", cornerWidthClampApplications=" + CornerWidthClampApplications +
                    ", cornerWidthClampedEdges=" + CornerWidthClampedEdges +
                    ", minimumCornerWidthScale=" + MinimumCornerWidthScale.ToString("F6") +
                    ", initialMaximumCornerDisplacement=" + InitialMaximumCornerDisplacement.ToString("F6") +
                    ", initialMaximumCornerDisplacementLimit=" + InitialMaximumCornerDisplacementLimit.ToString("F6") +
                    ", initialWorstCornerFace=" + InitialWorstCornerFace +
                    ", initialWorstCornerVertex=" + InitialWorstCornerVertex +
                    ", initialWorstCornerPreviousEdge=" + InitialWorstCornerPreviousEdge +
                    ", initialWorstCornerNextEdge=" + InitialWorstCornerNextEdge +
                    ", finalMaximumCornerDisplacement=" + FinalMaximumCornerDisplacement.ToString("F6") +
                    ", finalMaximumCornerDisplacementLimit=" + FinalMaximumCornerDisplacementLimit.ToString("F6") +
                    ", finalWorstCornerFace=" + FinalWorstCornerFace +
                    ", finalWorstCornerVertex=" + FinalWorstCornerVertex +
                    ", finalWorstCornerPreviousEdge=" + FinalWorstCornerPreviousEdge +
                    ", finalWorstCornerNextEdge=" + FinalWorstCornerNextEdge +
                    ", cornerWidthConvergenceFailures=" + CornerWidthConvergenceFailures +
                    ", cornerWidthBelowMinimumFailures=" + CornerWidthBelowMinimumFailures +
                    ", sharedEdgeWidthClampApplications=" + SharedEdgeWidthClampApplications +
                    ", sharedEdgeWidthClampedEdges=" + SharedEdgeWidthClampedEdges +
                    ", minimumSharedEdgeWidthScale=" + MinimumSharedEdgeWidthScale.ToString("F6") +
                    ", sharedEdgeWidthConvergenceFailures=" + SharedEdgeWidthConvergenceFailures +
                    ", sharedEdgeWidthBelowMinimumFailures=" + SharedEdgeWidthBelowMinimumFailures +
                    ", sharedEdgeWidthDeferredEdges=" + SharedEdgeWidthDeferredEdges +
                    ", replacementFacesValid=" + ReplacementFacesValid +
                    ", replacementFaceAreaFailures=" + ReplacementFaceAreaFailureCount +
                    ", replacementFaceWindingFailures=" + ReplacementFaceWindingFailureCount +
                    ", replacementEdgeCollapseFailures=" + ReplacementEdgeCollapseFailureCount +
                    ", unselectedInternalEdges=" + UnselectedInternalEdgeCount +
                    ", sharedUnselectedEndpointsChecked=" + SharedUnselectedEndpointsChecked +
                    ", sharedUnselectedEndpointsExact=" + SharedUnselectedEndpointsExact +
                    ", sharedUnselectedEndpointsReconciled=" + SharedUnselectedEndpointsReconciled +
                    ", sharedUnselectedEndpointFailures=" + SharedUnselectedEndpointFailureCount +
                    ", selectedRailsChecked=" + SelectedRailsChecked +
                    ", selectedRailsValid=" + SelectedRailsValid +
                    ", selectedRailSpanFailures=" + SelectedRailSpanFailureCount +
                    ", selectedRailLengthFailures=" + SelectedRailLengthFailureCount +
                    ", sourceBoundaryEdges=" + SourceBoundaryEdgeCount +
                    ", solvedBoundaryEdges=" + SolvedBoundaryEdgeCount +
                    ", solvedBoundaryLoopFailures=" + SolvedBoundaryLoopFailureCount +
                    ", readyForChamferEmission=" + ReadyForEmission;
            }
        }

        private sealed class ChamferHalfEdge
        {
            public int Index;
            public int OriginVertex;
            public int DestinationVertex;
            public int FaceIndex;
            public int SourceEdgeIndex;
            public int Next = -1;
            public int Previous = -1;
            public int Opposite = -1;
            public bool IsSelected;
        }

        private struct ChamferReadinessStats
        {
            public int CandidateCount;
            public int SelectedCount;
            public int SourceFaceCount;
            public int SourceVertexCount;
            public int SourceEdgeCount;
            public int SourceHalfEdgeCount;
            public int SourceOpenEdgeCount;
            public int SourceBoundaryLoopCount;
            public int SourceNonManifoldEdgeCount;
            public int SourceTJunctionCount;
            public int BoundaryTraceFailureCount;
            public int SelectedGraphEdgeCount;
            public int SelectedManifoldEdgeCount;
            public int SelectedBoundaryEdgeCount;
            public int SelectedNonManifoldEdgeCount;
            public int MissingSelectedGraphEdgeCount;
            public int MismatchedSelectedGraphFaceCount;
            public int DuplicateSelectedGraphEdgeCount;
            public int InvalidGraphFaceCount;
            public int InvalidGraphEdgeCount;
            public int AffectedVertexCount;
            public int ClosedVertexFanCount;
            public int OpenVertexFanCount;
            public int DisconnectedVertexFanCount;
            public int SelectedRunCount;
            public int MultipleSelectedRunVertexCount;
            public int Ready;
            public int Blocked;

            public ChamferReadinessStats(int candidateCount, int selectedCount)
            {
                this = default;
                CandidateCount = candidateCount;
                SelectedCount = selectedCount;
            }

            public void ApplyGraphStats(EdgeWearGraphBuildStats stats)
            {
                SourceFaceCount = stats.GraphFaceCount;
                SourceVertexCount = stats.GraphVertexCount;
                SourceEdgeCount = stats.GraphEdgeCount;
                SourceOpenEdgeCount = stats.GraphBoundaryEdgeCount;
                SourceNonManifoldEdgeCount = stats.GraphNonManifoldEdgeCount;
                SelectedGraphEdgeCount = stats.SelectedGraphEdgeCount;
                MissingSelectedGraphEdgeCount = stats.MissingSelectedGraphEdgeCount;
                MismatchedSelectedGraphFaceCount = stats.MismatchedSelectedGraphFaceCount;
                DuplicateSelectedGraphEdgeCount = stats.DuplicateSelectedGraphEdgeCount;
                InvalidGraphFaceCount = stats.InvalidFaceCount;
                InvalidGraphEdgeCount = stats.InvalidEdgeCount;
            }

            public string ToSummaryString()
            {
                return
                    "candidates=" + CandidateCount +
                    ", selected=" + SelectedCount +
                    ", sourceFaces=" + SourceFaceCount +
                    ", sourceVertices=" + SourceVertexCount +
                    ", sourceEdges=" + SourceEdgeCount +
                    ", halfEdges=" + SourceHalfEdgeCount +
                    ", sourceBoundaryEdges=" + SourceOpenEdgeCount +
                    ", sourceBoundaryLoops=" + SourceBoundaryLoopCount +
                    ", boundaryTraceFailures=" + BoundaryTraceFailureCount +
                    ", sourceNonManifoldEdges=" + SourceNonManifoldEdgeCount +
                    ", sourceTJunctions=" + SourceTJunctionCount +
                    ", selectedGraphEdges=" + SelectedGraphEdgeCount +
                    ", selectedManifoldEdges=" + SelectedManifoldEdgeCount +
                    ", selectedBoundaryEdges=" + SelectedBoundaryEdgeCount +
                    ", selectedNonManifoldEdges=" + SelectedNonManifoldEdgeCount +
                    ", missingSelectedGraphEdges=" + MissingSelectedGraphEdgeCount +
                    ", mismatchedSelectedGraphFaces=" + MismatchedSelectedGraphFaceCount +
                    ", duplicateSelectedGraphEdges=" + DuplicateSelectedGraphEdgeCount +
                    ", invalidGraphFaces=" + InvalidGraphFaceCount +
                    ", invalidGraphEdges=" + InvalidGraphEdgeCount +
                    ", affectedVertices=" + AffectedVertexCount +
                    ", closedVertexFans=" + ClosedVertexFanCount +
                    ", openVertexFans=" + OpenVertexFanCount +
                    ", disconnectedVertexFans=" + DisconnectedVertexFanCount +
                    ", selectedRuns=" + SelectedRunCount +
                    ", multipleSelectedRunVertices=" + MultipleSelectedRunVertexCount +
                    ", readyForChamferKernel=" + Ready +
                    ", blocked=" + Blocked;
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
