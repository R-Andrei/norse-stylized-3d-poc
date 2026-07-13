using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Plane-cut mass construction

        private static TriangleSoup BuildPlaneCutMass(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures,
            EdgeWearEvaluationMode edgeWearEvaluationMode,
            int boundedEdgeOrdinal,
            out PlaneCutBevelPreviewStatus previewStatus,
            out BoundedEdgePreviewStatus boundedPreviewStatus)
        {
            previewStatus = default;
            boundedPreviewStatus = default;
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

            TriangleSoup planeCutPreviewSoup = null;
            if (edgeWearEvaluationMode != EdgeWearEvaluationMode.None)
            {
                planeCutPreviewSoup = ApplyGeneratedEdgeWearBevels(
                    faces,
                    recipe,
                    surfaceFeatures,
                    edgeWearEvaluationMode,
                    boundedEdgeOrdinal,
                    out previewStatus,
                    out boundedPreviewStatus);
            }
            if ((edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.PlaneCutPreview ||
                 edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.BoundedSingleEdgePreview) &&
                planeCutPreviewSoup != null)
            {
                return planeCutPreviewSoup;
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

        #endregion
    }
}
