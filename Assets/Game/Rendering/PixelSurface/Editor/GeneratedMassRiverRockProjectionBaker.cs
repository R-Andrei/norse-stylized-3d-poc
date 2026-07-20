using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Text;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;
using ProgrammaticStylized3D.Geometry.Masses;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    /// <summary>
    /// Editor-only frozen Terrain/Squat Generated Mass material refinement.
    /// It creates no runtime asset and adds no runtime behavior.
    /// </summary>
    internal static class GeneratedMassRiverRockProjectionBaker
    {
        internal const int AlgorithmVersion = 4;
        internal const int Resolution = 1536;
        internal const int CatalogColumns = 5;
        internal const int CatalogRows = 4;
        internal const int ExpectedRockCount = 18;
        internal const int ExpectedTerrainCount = 9;
        internal const int ExpectedSquatCount = 9;
        internal const int ExpectedFrozenCount = 18;

        private const int CellPadding = 12;
        private const int LabelStripHeight = 48;
        private const float MinimumTriangleArea = 0.00001f;
        private const float DepthEpsilon = 0.000001f;
        private const float StrongHeightFilterRangeSigma = 0.045f;
        private const float MildHeightFilterRangeSigma = 0.026f;
        private const int StrongHeightFilterPasses = 3;
        private const int MildHeightFilterPasses = 1;
        private const float StrongNormalStrength = 6.2f;
        private const float MildNormalStrength = 7.2f;
        private const float MildNormalBlend = 0.22f;
        private const float RawExposureContribution = 0.12f;
        private const int WearSilhouetteExclusionRadius = 3;

        private static readonly float[] BurialComparisonFractions =
        {
            0.08f,
            0.18f,
            0.28f,
            0.38f
        };

        private static readonly string[] BurialComparisonSourceIds =
        {
            "S-12", "S-14", "T-13", "T-15"
        };

        private static readonly Dictionary<char, string[]> BitmapFont =
            BuildBitmapFont();

        internal sealed class RockEvidence
        {
            internal int Index;
            internal string StableId;
            internal string ProfileCode;
            internal string ProfileName;
            internal MassArchetype Archetype;
            internal int ShapeSeed;
            internal int SurfaceSeed;
            internal float BurialFraction;
            internal float RotationDegrees;
            internal bool IsFrozen;
            internal FormComplexity FormComplexity;
            internal SurfaceFacetDensity FacetDensity;
            internal EdgeCharacter EdgeCharacter;
            internal ShapeDiversity ShapeDiversity;
            internal GroundingStyle Grounding;
            internal LeanStyle Lean;
            internal float WidthBias;
            internal float HeightBias;
            internal float DepthBias;
            internal float SurfaceVariation;
            internal float EdgeWearAmount;
            internal float EdgeWearWidth;
            internal int VertexCount;
            internal int TriangleCount;
            internal int OccupiedPixels;
            internal float HeightRange;
            internal float NormalVariance;
            internal float MeanVariation;
            internal float MeanExposure;
            internal float MeanCrevice;
            internal float MeanEdgeWear;
            internal float SilhouetteAspect;
            internal bool UsedFallbackMesh;
            internal string FallbackReason;
            internal string Fingerprint;
        }

        internal sealed class ProjectionResult
        {
            internal readonly List<RockEvidence> Rocks =
                new List<RockEvidence>();
            internal readonly List<string> BurialSourceIds =
                new List<string>();
            internal Color32[] Raw;
            internal Color32[] Neutral;
            internal Color32[] Processed;
            internal Color32[] Strong;
            internal Color32[] Height;
            internal Color32[] ProcessedHeight;
            internal Color32[] Normals;
            internal Color32[] ProcessedNormals;
            internal Color32[] Mask;
            internal Color32[] Variation;
            internal Color32[] ProcessedVariation;
            internal Color32[] Exposure;
            internal Color32[] ProcessedExposure;
            internal Color32[] Crevice;
            internal Color32[] ProcessedCrevice;
            internal Color32[] EdgeWear;
            internal Color32[] ProcessedEdgeWear;
            internal Color32[] BurialComparison;
            internal int BurialComparisonCellCount;
            internal int TotalVertices;
            internal int TotalTriangles;
            internal int FallbackCount;
            internal string Fingerprint;
            internal string Failure;

            internal bool Succeeded => string.IsNullOrEmpty(Failure);
        }

        private sealed class ProfileDefinition
        {
            internal string Code;
            internal string Name;
            internal FormComplexity FormComplexity;
            internal SurfaceFacetDensity FacetDensity;
            internal EdgeCharacter EdgeCharacter;
            internal ShapeDiversity ShapeDiversity;
            internal GroundingStyle TerrainGrounding;
            internal GroundingStyle SquatGrounding;
            internal LeanStyle TerrainLean;
            internal LeanStyle SquatLean;
            internal float WidthBias;
            internal float HeightBias;
            internal float DepthBias;
            internal float SurfaceVariation;
            internal float EdgeWearAmount;
            internal float EdgeWearWidth;
        }

        private enum MaterialResponseMode
        {
            Neutral = 0,
            Moderate = 1,
            Strong = 2
        }

        private sealed class FrozenRockContract
        {
            internal string StableId;
            internal MassArchetype Archetype;
            internal int ShapeSeed;
            internal int SurfaceSeed;
            internal float BurialFraction;
            internal float RotationDegrees;
        }

        private sealed class RockDefinition
        {
            internal int Index;
            internal string StableId;
            internal MassArchetype Archetype;
            internal ProfileDefinition Profile;
            internal int ShapeSeed;
            internal int SurfaceSeed;
            internal float BurialFraction;
            internal float RotationDegrees;
            internal bool IsFrozen;
        }

        private sealed class CatalogBuffers
        {
            internal readonly float[] Depth;
            internal readonly float[] Height;
            internal readonly float[] DetailHeight;
            internal readonly float[] Mask;
            internal readonly float[] Variation;
            internal readonly float[] Exposure;
            internal readonly float[] Crevice;
            internal readonly float[] EdgeWear;
            internal readonly Vector3[] Normals;
            internal readonly int[] RockIndex;

            internal CatalogBuffers(int resolution)
            {
                int pixelCount = resolution * resolution;
                Depth = new float[pixelCount];
                Height = new float[pixelCount];
                DetailHeight = new float[pixelCount];
                Mask = new float[pixelCount];
                Variation = new float[pixelCount];
                Exposure = new float[pixelCount];
                Crevice = new float[pixelCount];
                EdgeWear = new float[pixelCount];
                Normals = new Vector3[pixelCount];
                RockIndex = new int[pixelCount];
                for (int index = 0; index < pixelCount; index++)
                {
                    Depth[index] = float.NegativeInfinity;
                    Normals[index] = Vector3.up;
                    RockIndex[index] = -1;
                }
            }
        }

        private sealed class GeneratedRock
        {
            internal RockDefinition Definition;
            internal MassRecipe Recipe;
            internal MeshData Mesh;
            internal bool UsedFallback;
            internal string FallbackReason;
        }

        private static readonly ProfileDefinition UnevenBroadProfile =
            new ProfileDefinition
            {
                Code = "UB",
                Name = "Uneven Broad",
                FormComplexity = FormComplexity.Complex,
                FacetDensity = SurfaceFacetDensity.High,
                EdgeCharacter = EdgeCharacter.Chipped,
                ShapeDiversity = ShapeDiversity.Wild,
                TerrainGrounding = GroundingStyle.Stable,
                SquatGrounding = GroundingStyle.Embedded,
                TerrainLean = LeanStyle.Pronounced,
                SquatLean = LeanStyle.Subtle,
                WidthBias = 1.15f,
                HeightBias = 0.83f,
                DepthBias = 1.14f,
                SurfaceVariation = 0.68f,
                EdgeWearAmount = 1.12f,
                EdgeWearWidth = 0.72f
            };

        private static readonly FrozenRockContract[] FrozenLibrary =
        {
            new FrozenRockContract { StableId = "T-05", Archetype = MassArchetype.TerrainBoulder, ShapeSeed = 3187, SurfaceSeed = 4134, BurialFraction = 0.226f, RotationDegrees = 186f },
            new FrozenRockContract { StableId = "T-08", Archetype = MassArchetype.TerrainBoulder, ShapeSeed = 1291, SurfaceSeed = 1254, BurialFraction = 0.218f, RotationDegrees = 73f },
            new FrozenRockContract { StableId = "T-09", Archetype = MassArchetype.TerrainBoulder, ShapeSeed = 3473, SurfaceSeed = 6660, BurialFraction = 0.226f, RotationDegrees = 145f },
            new FrozenRockContract { StableId = "T-10", Archetype = MassArchetype.TerrainBoulder, ShapeSeed = 5237, SurfaceSeed = 9140, BurialFraction = 0.234f, RotationDegrees = 206f },
            new FrozenRockContract { StableId = "T-11", Archetype = MassArchetype.TerrainBoulder, ShapeSeed = 8123, SurfaceSeed = 9475, BurialFraction = 0.242f, RotationDegrees = 279f },
            new FrozenRockContract { StableId = "T-12", Archetype = MassArchetype.TerrainBoulder, ShapeSeed = 1579, SurfaceSeed = 2222, BurialFraction = 0.218f, RotationDegrees = 201f },
            new FrozenRockContract { StableId = "T-13", Archetype = MassArchetype.TerrainBoulder, ShapeSeed = 3821, SurfaceSeed = 8048, BurialFraction = 0.226f, RotationDegrees = 259f },
            new FrozenRockContract { StableId = "T-14", Archetype = MassArchetype.TerrainBoulder, ShapeSeed = 6173, SurfaceSeed = 4645, BurialFraction = 0.234f, RotationDegrees = 353f },
            new FrozenRockContract { StableId = "T-15", Archetype = MassArchetype.TerrainBoulder, ShapeSeed = 9431, SurfaceSeed = 7584, BurialFraction = 0.242f, RotationDegrees = 68f },
            new FrozenRockContract { StableId = "S-00", Archetype = MassArchetype.SquatBoulder, ShapeSeed = 5727, SurfaceSeed = 2238, BurialFraction = 0.218f, RotationDegrees = 246f },
            new FrozenRockContract { StableId = "S-03", Archetype = MassArchetype.SquatBoulder, ShapeSeed = 7319, SurfaceSeed = 3776, BurialFraction = 0.242f, RotationDegrees = 106f },
            new FrozenRockContract { StableId = "S-04", Archetype = MassArchetype.SquatBoulder, ShapeSeed = 1117, SurfaceSeed = 489, BurialFraction = 0.218f, RotationDegrees = 156f },
            new FrozenRockContract { StableId = "S-08", Archetype = MassArchetype.SquatBoulder, ShapeSeed = 1361, SurfaceSeed = 2721, BurialFraction = 0.218f, RotationDegrees = 110f },
            new FrozenRockContract { StableId = "S-09", Archetype = MassArchetype.SquatBoulder, ShapeSeed = 3593, SurfaceSeed = 8477, BurialFraction = 0.226f, RotationDegrees = 158f },
            new FrozenRockContract { StableId = "S-10", Archetype = MassArchetype.SquatBoulder, ShapeSeed = 5393, SurfaceSeed = 1210, BurialFraction = 0.234f, RotationDegrees = 255f },
            new FrozenRockContract { StableId = "S-12", Archetype = MassArchetype.SquatBoulder, ShapeSeed = 1693, SurfaceSeed = 3997, BurialFraction = 0.218f, RotationDegrees = 222f },
            new FrozenRockContract { StableId = "S-13", Archetype = MassArchetype.SquatBoulder, ShapeSeed = 4001, SurfaceSeed = 286, BurialFraction = 0.226f, RotationDegrees = 322f },
            new FrozenRockContract { StableId = "S-14", Archetype = MassArchetype.SquatBoulder, ShapeSeed = 6311, SurfaceSeed = 6588, BurialFraction = 0.234f, RotationDegrees = 35f }
        };

        internal static ProjectionResult Build()
        {
            ProjectionResult result = new ProjectionResult();
            try
            {
                List<RockDefinition> definitions = BuildRockDefinitions();
                if (definitions.Count != ExpectedRockCount)
                {
                    result.Failure = string.Format(
                        CultureInfo.InvariantCulture,
                        "Generated {0} rock definitions; expected {1}.",
                        definitions.Count,
                        ExpectedRockCount);
                    return result;
                }

                CatalogBuffers catalog = new CatalogBuffers(Resolution);
                for (int index = 0; index < definitions.Count; index++)
                {
                    RockDefinition definition = definitions[index];
                    GeneratedRock generated = GenerateRock(definition);
                    RockEvidence evidence = RasterizeRockIntoCell(
                        generated,
                        catalog,
                        Resolution,
                        CatalogColumns,
                        CatalogRows,
                        LabelStripHeight);
                    result.Rocks.Add(evidence);
                    result.TotalVertices += evidence.VertexCount;
                    result.TotalTriangles += evidence.TriangleCount;
                    if (evidence.UsedFallbackMesh)
                    {
                        result.FallbackCount++;
                    }
                }

                CatalogBuffers processed = BuildProcessedBuffers(
                    catalog,
                    result.Rocks,
                    CatalogColumns,
                    CatalogRows);
                BuildCatalogImages(catalog, processed, result);
                ApplyCatalogLabels(result);
                result.BurialComparison = BuildBurialComparison(
                    result.BurialSourceIds,
                    out int burialCellCount);
                result.BurialComparisonCellCount = burialCellCount;
                result.Fingerprint = CalculateFingerprint(result);
                return result;
            }
            catch (Exception exception)
            {
                result.Failure = exception.ToString();
                return result;
            }
        }

        private static List<RockDefinition> BuildRockDefinitions()
        {
            if (FrozenLibrary.Length != ExpectedRockCount)
            {
                throw new InvalidOperationException(
                    "Frozen river-rock library count does not match the " +
                    "expected M2.7C.5C contract.");
            }

            List<RockDefinition> definitions =
                new List<RockDefinition>(FrozenLibrary.Length);
            for (int index = 0; index < FrozenLibrary.Length; index++)
            {
                FrozenRockContract frozen = FrozenLibrary[index];
                definitions.Add(new RockDefinition
                {
                    Index = index,
                    StableId = frozen.StableId,
                    Archetype = frozen.Archetype,
                    Profile = UnevenBroadProfile,
                    ShapeSeed = frozen.ShapeSeed,
                    SurfaceSeed = frozen.SurfaceSeed,
                    BurialFraction = frozen.BurialFraction,
                    RotationDegrees = frozen.RotationDegrees,
                    IsFrozen = true
                });
            }

            return definitions;
        }

        private static GeneratedRock GenerateRock(
            RockDefinition definition)
        {
            MassRecipe recipe = CreateRecipe(definition);
            MassSurfaceFeatureSettings settings =
                CreateSurfaceFeatureSettings(definition);

            MeshData mesh;
            bool fallback = false;
            string fallbackReason = string.Empty;
            try
            {
                MassGenerator.UnifiedEdgeWearPreviewStatus previewStatus;
                mesh = MassGenerator.GenerateUnifiedEdgeWearPreview(
                    recipe,
                    settings,
                    out previewStatus);
                if (!previewStatus.PreviewApplied)
                {
                    throw new InvalidOperationException(
                        "Unified edge-wear preview was not applied: " +
                        previewStatus.Diagnostic);
                }

                ValidateMesh(mesh);
            }
            catch (Exception exception)
            {
                fallback = true;
                fallbackReason = exception.GetType().Name + ": " +
                    exception.Message;
                mesh = MassGenerator.Generate(recipe, settings);
                ValidateMesh(mesh);
            }

            return new GeneratedRock
            {
                Definition = definition,
                Recipe = recipe,
                Mesh = mesh,
                UsedFallback = fallback,
                FallbackReason = fallbackReason
            };
        }

        private static MassRecipe CreateRecipe(
            RockDefinition definition)
        {
            ProfileDefinition profile = definition.Profile;
            GroundingStyle grounding =
                definition.Archetype == MassArchetype.TerrainBoulder
                    ? profile.TerrainGrounding
                    : profile.SquatGrounding;
            LeanStyle lean =
                definition.Archetype == MassArchetype.TerrainBoulder
                    ? profile.TerrainLean
                    : profile.SquatLean;

            MassRecipe recipe = new MassRecipe();
            SetRecipeField(recipe, "archetype", definition.Archetype);
            recipe.ApplyArchetypeDefaults();
            SetRecipeField(
                recipe,
                "formComplexity",
                profile.FormComplexity);
            SetRecipeField(
                recipe,
                "surfaceFacetDensity",
                profile.FacetDensity);
            SetRecipeField(
                recipe,
                "edgeCharacter",
                profile.EdgeCharacter);
            SetRecipeField(
                recipe,
                "shapeDiversity",
                profile.ShapeDiversity);
            SetRecipeField(recipe, "grounding", grounding);
            SetRecipeField(recipe, "lean", lean);
            SetRecipeField(recipe, "fineScale", 1.0f);
            SetRecipeField(recipe, "widthBias", profile.WidthBias);
            SetRecipeField(recipe, "heightBias", profile.HeightBias);
            SetRecipeField(recipe, "depthBias", profile.DepthBias);
            SetRecipeField(
                recipe,
                "surfaceVariation",
                profile.SurfaceVariation);
            recipe.SetShapeSeed(definition.ShapeSeed);
            recipe.SetSurfaceSeed(definition.SurfaceSeed);
            return recipe;
        }

        private static void SetRecipeField<T>(
            MassRecipe recipe,
            string fieldName,
            T value)
        {
            FieldInfo field = typeof(MassRecipe).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(
                    typeof(MassRecipe).FullName,
                    fieldName);
            }

            Type expectedType = typeof(T);
            if (field.FieldType != expectedType)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "MassRecipe field '{0}' type mismatch. " +
                        "Expected {1}; actual {2}.",
                        fieldName,
                        expectedType.FullName,
                        field.FieldType.FullName));
            }

            field.SetValue(recipe, value);
        }

        private static MassSurfaceFeatureSettings
            CreateSurfaceFeatureSettings(RockDefinition definition)
        {
            ProfileDefinition profile = definition.Profile;
            return new MassSurfaceFeatureSettings(
                definition.Archetype,
                definition.SurfaceSeed,
                profile.EdgeWearAmount,
                profile.EdgeWearWidth,
                1.0f,
                0.72f,
                0.70f,
                0.32f,
                0f,
                1f,
                1f,
                0f);
        }

        private static void ValidateMesh(MeshData mesh)
        {
            if (mesh == null)
            {
                throw new InvalidOperationException(
                    "Generated Mass returned a null mesh.");
            }

            mesh.Validate();
            if (!mesh.HasNormals)
            {
                throw new InvalidOperationException(
                    "Generated Mass mesh has no complete normal channel.");
            }
        }

        private static RockEvidence RasterizeRockIntoCell(
            GeneratedRock generated,
            CatalogBuffers buffers,
            int resolution,
            int columns,
            int rows,
            int labelStripHeight)
        {
            RockDefinition definition = generated.Definition;
            int column = definition.Index % columns;
            int row = definition.Index / columns;
            int cellWidth = resolution / columns;
            int cellHeight = resolution / rows;
            RectInt cell = new RectInt(
                column * cellWidth,
                row * cellHeight,
                cellWidth,
                cellHeight);
            RectInt projectionCell = new RectInt(
                cell.xMin,
                cell.yMin + labelStripHeight,
                cell.width,
                Mathf.Max(4, cell.height - labelStripHeight));

            ProfileDefinition profile = definition.Profile;
            RockEvidence evidence = new RockEvidence
            {
                Index = definition.Index,
                StableId = definition.StableId,
                ProfileCode = profile.Code,
                ProfileName = profile.Name,
                Archetype = definition.Archetype,
                ShapeSeed = definition.ShapeSeed,
                SurfaceSeed = definition.SurfaceSeed,
                BurialFraction = definition.BurialFraction,
                RotationDegrees = definition.RotationDegrees,
                IsFrozen = definition.IsFrozen,
                FormComplexity = profile.FormComplexity,
                FacetDensity = profile.FacetDensity,
                EdgeCharacter = profile.EdgeCharacter,
                ShapeDiversity = profile.ShapeDiversity,
                Grounding = generated.Recipe.Grounding,
                Lean = generated.Recipe.Lean,
                WidthBias = profile.WidthBias,
                HeightBias = profile.HeightBias,
                DepthBias = profile.DepthBias,
                SurfaceVariation = profile.SurfaceVariation,
                EdgeWearAmount = profile.EdgeWearAmount,
                EdgeWearWidth = profile.EdgeWearWidth,
                VertexCount = generated.Mesh.VertexCount,
                TriangleCount = generated.Mesh.TriangleCount,
                UsedFallbackMesh = generated.UsedFallback,
                FallbackReason = generated.FallbackReason
            };

            RasterizeMesh(
                generated.Mesh,
                buffers,
                resolution,
                projectionCell,
                definition.BurialFraction,
                definition.RotationDegrees,
                evidence);
            evidence.Fingerprint = CalculateRockFingerprint(
                evidence,
                buffers,
                resolution,
                projectionCell);
            return evidence;
        }

        private static void RasterizeMesh(
            MeshData mesh,
            CatalogBuffers buffers,
            int resolution,
            RectInt cell,
            float burialFraction,
            float rotationDegrees,
            RockEvidence evidence)
        {
            int vertexCount = mesh.Vertices.Count;
            Vector3[] positions = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            float radians = rotationDegrees * Mathf.Deg2Rad;
            float cosine = Mathf.Cos(radians);
            float sine = Mathf.Sin(radians);

            float minimumX = float.PositiveInfinity;
            float maximumX = float.NegativeInfinity;
            float minimumY = float.PositiveInfinity;
            float maximumY = float.NegativeInfinity;
            float minimumZ = float.PositiveInfinity;
            float maximumZ = float.NegativeInfinity;
            for (int index = 0; index < vertexCount; index++)
            {
                Vector3 source = mesh.Vertices[index];
                Vector3 transformed = new Vector3(
                    source.x * cosine + source.z * sine,
                    source.y,
                    -source.x * sine + source.z * cosine);
                positions[index] = transformed;

                Vector3 sourceNormal = mesh.Normals[index];
                normals[index] = new Vector3(
                    sourceNormal.x * cosine + sourceNormal.z * sine,
                    sourceNormal.y,
                    -sourceNormal.x * sine + sourceNormal.z * cosine)
                    .normalized;

                minimumX = Mathf.Min(minimumX, transformed.x);
                maximumX = Mathf.Max(maximumX, transformed.x);
                minimumY = Mathf.Min(minimumY, transformed.y);
                maximumY = Mathf.Max(maximumY, transformed.y);
                minimumZ = Mathf.Min(minimumZ, transformed.z);
                maximumZ = Mathf.Max(maximumZ, transformed.z);
            }

            float width = Mathf.Max(0.0001f, maximumX - minimumX);
            float depth = Mathf.Max(0.0001f, maximumZ - minimumZ);
            float fullHeight = Mathf.Max(0.0001f, maximumY - minimumY);
            float burialY = minimumY +
                fullHeight * Mathf.Clamp(burialFraction, 0f, 0.75f);
            float visibleHeight = Mathf.Max(
                0.0001f,
                maximumY - burialY);
            float availableWidth = Mathf.Max(
                4f,
                cell.width - CellPadding * 2f);
            float availableHeight = Mathf.Max(
                4f,
                cell.height - CellPadding * 2f);
            float scale = Mathf.Min(
                availableWidth / width,
                availableHeight / depth);
            float centerX = (minimumX + maximumX) * 0.5f;
            float centerZ = (minimumZ + maximumZ) * 0.5f;
            float screenCenterX = cell.xMin + cell.width * 0.5f;
            float screenCenterY = cell.yMin + cell.height * 0.5f;

            Vector2[] projected = new Vector2[vertexCount];
            for (int index = 0; index < vertexCount; index++)
            {
                projected[index] = new Vector2(
                    screenCenterX +
                        (positions[index].x - centerX) * scale,
                    screenCenterY +
                        (positions[index].z - centerZ) * scale);
            }

            for (int triangleOffset = 0;
                 triangleOffset < mesh.Triangles.Count;
                 triangleOffset += 3)
            {
                RasterizeTriangle(
                    mesh,
                    positions,
                    normals,
                    projected,
                    mesh.Triangles[triangleOffset],
                    mesh.Triangles[triangleOffset + 1],
                    mesh.Triangles[triangleOffset + 2],
                    burialY,
                    visibleHeight,
                    buffers,
                    resolution,
                    cell,
                    evidence.Index);
            }

            MeasureCellEvidence(
                buffers,
                resolution,
                cell,
                evidence);
        }

        private static void RasterizeTriangle(
            MeshData mesh,
            Vector3[] positions,
            Vector3[] normals,
            Vector2[] projected,
            int indexA,
            int indexB,
            int indexC,
            float burialY,
            float visibleHeight,
            CatalogBuffers buffers,
            int resolution,
            RectInt cell,
            int rockIndex)
        {
            Vector2 pointA = projected[indexA];
            Vector2 pointB = projected[indexB];
            Vector2 pointC = projected[indexC];
            float area = Edge(pointA, pointB, pointC);
            if (Mathf.Abs(area) <= MinimumTriangleArea)
            {
                return;
            }

            int minimumX = Mathf.Clamp(
                Mathf.FloorToInt(Mathf.Min(
                    pointA.x,
                    Mathf.Min(pointB.x, pointC.x))),
                cell.xMin,
                cell.xMax - 1);
            int maximumX = Mathf.Clamp(
                Mathf.CeilToInt(Mathf.Max(
                    pointA.x,
                    Mathf.Max(pointB.x, pointC.x))),
                cell.xMin,
                cell.xMax - 1);
            int minimumY = Mathf.Clamp(
                Mathf.FloorToInt(Mathf.Min(
                    pointA.y,
                    Mathf.Min(pointB.y, pointC.y))),
                cell.yMin,
                cell.yMax - 1);
            int maximumY = Mathf.Clamp(
                Mathf.CeilToInt(Mathf.Max(
                    pointA.y,
                    Mathf.Max(pointB.y, pointC.y))),
                cell.yMin,
                cell.yMax - 1);

            Color colorA = ResolveColor(mesh, indexA);
            Color colorB = ResolveColor(mesh, indexB);
            Color colorC = ResolveColor(mesh, indexC);
            Vector4 uv2A = ResolveUv2(mesh, indexA);
            Vector4 uv2B = ResolveUv2(mesh, indexB);
            Vector4 uv2C = ResolveUv2(mesh, indexC);

            for (int y = minimumY; y <= maximumY; y++)
            {
                for (int x = minimumX; x <= maximumX; x++)
                {
                    Vector2 sample = new Vector2(
                        x + 0.5f,
                        y + 0.5f);
                    float weightA = Edge(pointB, pointC, sample) / area;
                    float weightB = Edge(pointC, pointA, sample) / area;
                    float weightC = 1f - weightA - weightB;
                    if (weightA < -0.0001f ||
                        weightB < -0.0001f ||
                        weightC < -0.0001f)
                    {
                        continue;
                    }

                    float worldY =
                        positions[indexA].y * weightA +
                        positions[indexB].y * weightB +
                        positions[indexC].y * weightC;
                    if (worldY <= burialY)
                    {
                        continue;
                    }

                    int destination = y * resolution + x;
                    if (worldY <= buffers.Depth[destination] + DepthEpsilon)
                    {
                        continue;
                    }

                    Vector3 normal = (
                        normals[indexA] * weightA +
                        normals[indexB] * weightB +
                        normals[indexC] * weightC).normalized;
                    Color color =
                        colorA * weightA +
                        colorB * weightB +
                        colorC * weightC;
                    Vector4 uv2 =
                        uv2A * weightA +
                        uv2B * weightB +
                        uv2C * weightC;

                    buffers.Depth[destination] = worldY;
                    buffers.Height[destination] = Mathf.Clamp01(
                        (worldY - burialY) / visibleHeight);
                    buffers.Mask[destination] = 1f;
                    buffers.Normals[destination] = normal;
                    buffers.Variation[destination] = Mathf.Clamp01(color.r);
                    buffers.Exposure[destination] = Mathf.Clamp01(color.g);
                    buffers.Crevice[destination] = Mathf.Clamp01(color.b);
                    buffers.EdgeWear[destination] = Mathf.Clamp01(
                        Mathf.Max(color.a, uv2.z));
                    buffers.RockIndex[destination] = rockIndex;
                }
            }
        }

        private static Color ResolveColor(MeshData mesh, int index)
        {
            return mesh.Colors.Count == mesh.Vertices.Count
                ? mesh.Colors[index]
                : new Color(0.5f, 0.5f, 0f, 0f);
        }

        private static Vector4 ResolveUv2(MeshData mesh, int index)
        {
            return mesh.UV2.Count == mesh.Vertices.Count
                ? mesh.UV2[index]
                : Vector4.zero;
        }

        private static float Edge(
            Vector2 pointA,
            Vector2 pointB,
            Vector2 point)
        {
            return (point.x - pointA.x) *
                    (pointB.y - pointA.y) -
                (point.y - pointA.y) *
                    (pointB.x - pointA.x);
        }

        private static void MeasureCellEvidence(
            CatalogBuffers buffers,
            int resolution,
            RectInt cell,
            RockEvidence evidence)
        {
            float minimumHeight = float.PositiveInfinity;
            float maximumHeight = float.NegativeInfinity;
            int minimumX = cell.xMax;
            int maximumX = cell.xMin;
            int minimumY = cell.yMax;
            int maximumY = cell.yMin;
            Vector3 meanNormal = Vector3.zero;
            double variation = 0.0;
            double exposure = 0.0;
            double crevice = 0.0;
            double edgeWear = 0.0;
            int count = 0;
            for (int y = cell.yMin; y < cell.yMax; y++)
            {
                for (int x = cell.xMin; x < cell.xMax; x++)
                {
                    int index = y * resolution + x;
                    if (buffers.Mask[index] <= 0.5f)
                    {
                        continue;
                    }

                    float height = buffers.Height[index];
                    minimumHeight = Mathf.Min(minimumHeight, height);
                    maximumHeight = Mathf.Max(maximumHeight, height);
                    minimumX = Mathf.Min(minimumX, x);
                    maximumX = Mathf.Max(maximumX, x);
                    minimumY = Mathf.Min(minimumY, y);
                    maximumY = Mathf.Max(maximumY, y);
                    meanNormal += buffers.Normals[index];
                    variation += buffers.Variation[index];
                    exposure += buffers.Exposure[index];
                    crevice += buffers.Crevice[index];
                    edgeWear += buffers.EdgeWear[index];
                    count++;
                }
            }

            evidence.OccupiedPixels = count;
            if (count <= 0)
            {
                evidence.HeightRange = 0f;
                evidence.NormalVariance = 0f;
                evidence.SilhouetteAspect = 0f;
                return;
            }

            meanNormal /= count;
            double variance = 0.0;
            for (int y = cell.yMin; y < cell.yMax; y++)
            {
                for (int x = cell.xMin; x < cell.xMax; x++)
                {
                    int index = y * resolution + x;
                    if (buffers.Mask[index] <= 0.5f)
                    {
                        continue;
                    }

                    Vector3 difference =
                        buffers.Normals[index] - meanNormal;
                    variance += difference.sqrMagnitude;
                }
            }

            float occupiedWidth = Mathf.Max(1f, maximumX - minimumX + 1f);
            float occupiedHeight = Mathf.Max(1f, maximumY - minimumY + 1f);
            evidence.HeightRange = Mathf.Max(
                0f,
                maximumHeight - minimumHeight);
            evidence.NormalVariance = (float)(variance / count);
            evidence.MeanVariation = (float)(variation / count);
            evidence.MeanExposure = (float)(exposure / count);
            evidence.MeanCrevice = (float)(crevice / count);
            evidence.MeanEdgeWear = (float)(edgeWear / count);
            evidence.SilhouetteAspect = Mathf.Max(
                occupiedWidth / occupiedHeight,
                occupiedHeight / occupiedWidth);
        }

        private static CatalogBuffers BuildProcessedBuffers(
            CatalogBuffers raw,
            IReadOnlyList<RockEvidence> rocks,
            int columns,
            int rows)
        {
            CatalogBuffers processed = new CatalogBuffers(Resolution);
            Array.Copy(raw.Mask, processed.Mask, raw.Mask.Length);
            Array.Copy(raw.Depth, processed.Depth, raw.Depth.Length);
            Array.Copy(raw.RockIndex, processed.RockIndex, raw.RockIndex.Length);

            float[] strongHeight = (float[])raw.Height.Clone();
            for (int pass = 0; pass < StrongHeightFilterPasses; pass++)
            {
                strongHeight = ApplyMaskedEdgeAwareHeightPass(
                    strongHeight,
                    raw.Mask,
                    StrongHeightFilterRangeSigma);
            }

            float[] mildHeight = (float[])raw.Height.Clone();
            for (int pass = 0; pass < MildHeightFilterPasses; pass++)
            {
                mildHeight = ApplyMaskedEdgeAwareHeightPass(
                    mildHeight,
                    raw.Mask,
                    MildHeightFilterRangeSigma);
            }

            Array.Copy(strongHeight, processed.Height, strongHeight.Length);
            Array.Copy(mildHeight, processed.DetailHeight, mildHeight.Length);
            Vector3[] volumeNormals = BuildHeightDerivedNormals(
                strongHeight,
                raw.Mask,
                StrongNormalStrength);
            Vector3[] planeNormals = BuildHeightDerivedNormals(
                mildHeight,
                raw.Mask,
                MildNormalStrength);

            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    int index = y * Resolution + x;
                    if (raw.Mask[index] <= 0.5f)
                    {
                        processed.Normals[index] = Vector3.up;
                        continue;
                    }

                    int rockIndex = raw.RockIndex[index];
                    RockEvidence rock = ResolveRockEvidence(rocks, rockIndex);
                    processed.Normals[index] = Vector3.Lerp(
                        volumeNormals[index],
                        planeNormals[index],
                        MildNormalBlend).normalized;
                    processed.Variation[index] = BuildMaterialVariation(
                        rock,
                        x,
                        y,
                        rockIndex,
                        columns,
                        rows);
                    float normalExposure = SmoothStep(
                        0.20f,
                        0.96f,
                        processed.Normals[index].y);
                    processed.Exposure[index] = Mathf.Lerp(
                        normalExposure,
                        raw.Exposure[index],
                        RawExposureContribution);
                    processed.Crevice[index] = BuildSelectiveRootDarkening(
                        raw,
                        processed,
                        rock,
                        x,
                        y,
                        index,
                        rockIndex,
                        columns,
                        rows);
                }
            }

            BuildProcessedEdgeWear(
                raw,
                processed,
                rocks,
                columns,
                rows);
            return processed;
        }

        private static RockEvidence ResolveRockEvidence(
            IReadOnlyList<RockEvidence> rocks,
            int rockIndex)
        {
            if (rocks == null ||
                rockIndex < 0 ||
                rockIndex >= rocks.Count)
            {
                throw new InvalidOperationException(
                    "Processed projection pixel has no valid frozen rock owner.");
            }

            return rocks[rockIndex];
        }

        private static float BuildMaterialVariation(
            RockEvidence rock,
            int x,
            int y,
            int rockIndex,
            int columns,
            int rows)
        {
            ResolveLocalRockCoordinates(
                x,
                y,
                rockIndex,
                columns,
                rows,
                out float localX,
                out float localY);
            int seed = rock.SurfaceSeed;
            float phaseA = Hash01(seed * 37 + 101) * Mathf.PI * 2f;
            float phaseB = Hash01(seed * 53 + 211) * Mathf.PI * 2f;
            float phaseC = Hash01(seed * 71 + 307) * Mathf.PI * 2f;
            float broadA = Mathf.Sin(
                (localX * 0.72f + localY * 0.31f) * Mathf.PI + phaseA);
            float broadB = Mathf.Sin(
                (-localX * 0.38f + localY * 0.81f) *
                Mathf.PI * 1.25f + phaseB);
            float grain = Mathf.Sin(
                (localX * 3.1f + localY * 2.6f) *
                Mathf.PI + phaseC) * 0.65f +
                Mathf.Sin(
                    (localX * 5.3f - localY * 4.1f) *
                    Mathf.PI + phaseA * 0.63f) * 0.35f;
            float rockOffset = (Hash01(seed * 97 + 401) - 0.5f) * 0.10f;
            return Mathf.Clamp01(
                0.50f +
                rockOffset +
                broadA * 0.070f +
                broadB * 0.045f +
                grain * 0.018f);
        }

        private static float BuildSelectiveRootDarkening(
            CatalogBuffers raw,
            CatalogBuffers processed,
            RockEvidence rock,
            int x,
            int y,
            int index,
            int rockIndex,
            int columns,
            int rows)
        {
            ResolveLocalRockCoordinates(
                x,
                y,
                rockIndex,
                columns,
                rows,
                out float localX,
                out float localY);
            float radius = Mathf.Sqrt(localX * localX + localY * localY);
            if (radius <= 0.0001f)
            {
                return 0f;
            }

            float angle = Mathf.Atan2(localY, localX);
            int seed = rock.SurfaceSeed;
            float contactAngle = Hash01(seed * 43 + 503) * Mathf.PI * 2f;
            Vector2 contactDirection = new Vector2(
                Mathf.Cos(contactAngle),
                Mathf.Sin(contactAngle));
            Vector2 radial = new Vector2(localX, localY).normalized;
            float sector = SmoothStep(
                -0.05f,
                0.68f,
                Vector2.Dot(radial, contactDirection));
            float wave = 0.5f + 0.5f * Mathf.Sin(
                angle * 3f +
                Hash01(seed * 59 + 601) * Mathf.PI * 2f +
                Mathf.Sin(angle * 2f + contactAngle) * 0.55f);
            float brokenContact = SmoothStep(0.48f, 0.72f, wave);
            float lowHeight = SmoothStep(
                0.30f,
                0.035f,
                processed.Height[index]);
            float sideFacing = SmoothStep(
                0.04f,
                0.58f,
                1f - processed.Normals[index].y);
            float rawSupport = Mathf.Lerp(
                0.78f,
                1f,
                Mathf.Clamp01(raw.Crevice[index]));
            return Mathf.Clamp01(
                lowHeight *
                sideFacing *
                sector *
                Mathf.Lerp(0.30f, 1f, brokenContact) *
                rawSupport);
        }

        private static void ResolveLocalRockCoordinates(
            int x,
            int y,
            int rockIndex,
            int columns,
            int rows,
            out float localX,
            out float localY)
        {
            int cellWidth = Resolution / columns;
            int cellHeight = Resolution / rows;
            int column = rockIndex % columns;
            int row = rockIndex / columns;
            float centerX = column * cellWidth + cellWidth * 0.5f;
            float projectionHeight = Mathf.Max(4f, cellHeight - LabelStripHeight);
            float centerY = row * cellHeight +
                LabelStripHeight + projectionHeight * 0.5f;
            localX = (x + 0.5f - centerX) /
                Mathf.Max(1f, cellWidth * 0.5f);
            localY = (y + 0.5f - centerY) /
                Mathf.Max(1f, projectionHeight * 0.5f);
        }

        private static float Hash01(int value)
        {
            unchecked
            {
                uint x = (uint)value;
                x ^= x >> 16;
                x *= 0x7FEB352Du;
                x ^= x >> 15;
                x *= 0x846CA68Bu;
                x ^= x >> 16;
                return (x & 0x00FFFFFFu) / 16777215f;
            }
        }

        private static float[] ApplyMaskedEdgeAwareHeightPass(
            float[] source,
            float[] mask,
            float rangeSigma)
        {
            float[] output = (float[])source.Clone();
            float inverseRange = 1f /
                Mathf.Max(0.000001f, 2f * rangeSigma * rangeSigma);
            for (int y = 1; y < Resolution - 1; y++)
            {
                for (int x = 1; x < Resolution - 1; x++)
                {
                    int index = y * Resolution + x;
                    if (mask[index] <= 0.5f)
                    {
                        continue;
                    }

                    float center = source[index];
                    float weighted = center * 1.5f;
                    float totalWeight = 1.5f;
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        for (int offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            if (offsetX == 0 && offsetY == 0)
                            {
                                continue;
                            }

                            int sample = (y + offsetY) * Resolution +
                                x + offsetX;
                            if (mask[sample] <= 0.5f)
                            {
                                continue;
                            }

                            float delta = source[sample] - center;
                            float rangeWeight = Mathf.Exp(
                                -(delta * delta) * inverseRange);
                            float spatialWeight = offsetX != 0 &&
                                offsetY != 0
                                    ? 0.70f
                                    : 1f;
                            float weight = rangeWeight * spatialWeight;
                            weighted += source[sample] * weight;
                            totalWeight += weight;
                        }
                    }

                    output[index] = weighted /
                        Mathf.Max(0.000001f, totalWeight);
                }
            }

            return output;
        }

        private static Vector3[] BuildHeightDerivedNormals(
            float[] height,
            float[] mask,
            float strength)
        {
            Vector3[] normals = new Vector3[height.Length];
            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    int index = y * Resolution + x;
                    if (mask[index] <= 0.5f)
                    {
                        normals[index] = Vector3.up;
                        continue;
                    }

                    float center = height[index];
                    float left = SampleMaskedHeight(
                        height,
                        mask,
                        x - 1,
                        y,
                        center);
                    float right = SampleMaskedHeight(
                        height,
                        mask,
                        x + 1,
                        y,
                        center);
                    float down = SampleMaskedHeight(
                        height,
                        mask,
                        x,
                        y - 1,
                        center);
                    float up = SampleMaskedHeight(
                        height,
                        mask,
                        x,
                        y + 1,
                        center);
                    float deltaX = (right - left) * strength;
                    float deltaZ = (up - down) * strength;
                    normals[index] = new Vector3(
                        -deltaX,
                        1f,
                        -deltaZ).normalized;
                }
            }

            return normals;
        }

        private static float SampleMaskedHeight(
            float[] height,
            float[] mask,
            int x,
            int y,
            float fallback)
        {
            if (x < 0 || x >= Resolution ||
                y < 0 || y >= Resolution)
            {
                return fallback;
            }

            int index = y * Resolution + x;
            return mask[index] > 0.5f
                ? height[index]
                : fallback;
        }

        private static void BuildProcessedEdgeWear(
            CatalogBuffers raw,
            CatalogBuffers processed,
            IReadOnlyList<RockEvidence> rocks,
            int columns,
            int rows)
        {
            for (int y = WearSilhouetteExclusionRadius;
                 y < Resolution - WearSilhouetteExclusionRadius;
                 y++)
            {
                for (int x = WearSilhouetteExclusionRadius;
                     x < Resolution - WearSilhouetteExclusionRadius;
                     x++)
                {
                    int index = y * Resolution + x;
                    if (raw.Mask[index] <= 0.5f ||
                        !IsInteriorAtRadius(
                            raw.Mask,
                            x,
                            y,
                            WearSilhouetteExclusionRadius))
                    {
                        continue;
                    }

                    int left = index - 1;
                    int right = index + 1;
                    int down = index - Resolution;
                    int up = index + Resolution;
                    float height = processed.DetailHeight[index];
                    float laplacian =
                        processed.DetailHeight[left] +
                        processed.DetailHeight[right] +
                        processed.DetailHeight[down] +
                        processed.DetailHeight[up] -
                        height * 4f;
                    float convex = Mathf.Clamp01(
                        Mathf.Max(0f, -laplacian) * 14f);
                    float normalBreak = 0f;
                    Vector3 normal = processed.Normals[index];
                    normalBreak += 1f - Mathf.Clamp01(
                        Vector3.Dot(normal, processed.Normals[left]));
                    normalBreak += 1f - Mathf.Clamp01(
                        Vector3.Dot(normal, processed.Normals[right]));
                    normalBreak += 1f - Mathf.Clamp01(
                        Vector3.Dot(normal, processed.Normals[down]));
                    normalBreak += 1f - Mathf.Clamp01(
                        Vector3.Dot(normal, processed.Normals[up]));
                    normalBreak = Mathf.Clamp01(normalBreak * 1.85f);

                    int rockIndex = raw.RockIndex[index];
                    RockEvidence rock = ResolveRockEvidence(rocks, rockIndex);
                    ResolveLocalRockCoordinates(
                        x,
                        y,
                        rockIndex,
                        columns,
                        rows,
                        out float localX,
                        out float localY);
                    float breakup = 0.5f + 0.5f * Mathf.Sin(
                        (localX * 4.7f + localY * 3.9f) * Mathf.PI +
                        Hash01(rock.SurfaceSeed * 83 + 709) *
                        Mathf.PI * 2f);
                    float intermittent = Mathf.Lerp(
                        0.35f,
                        1f,
                        SmoothStep(0.32f, 0.72f, breakup));
                    float heightSupport = SmoothStep(
                        0.12f,
                        0.30f,
                        processed.Height[index]);
                    float nativeWear = Mathf.Pow(
                        Mathf.Clamp01(raw.EdgeWear[index]),
                        0.78f) * 0.88f;
                    float fallbackWear = Mathf.Max(
                        convex,
                        normalBreak) * 0.46f;
                    processed.EdgeWear[index] = Mathf.Clamp01(
                        Mathf.Max(nativeWear, fallbackWear) *
                        intermittent *
                        heightSupport);
                }
            }
        }

        private static bool IsInteriorAtRadius(
            float[] mask,
            int x,
            int y,
            int radius)
        {
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                for (int offsetX = -radius;
                     offsetX <= radius;
                     offsetX++)
                {
                    if (Mathf.Abs(offsetX) + Mathf.Abs(offsetY) > radius)
                    {
                        continue;
                    }

                    int sampleX = x + offsetX;
                    int sampleY = y + offsetY;
                    if (sampleX < 0 || sampleX >= Resolution ||
                        sampleY < 0 || sampleY >= Resolution ||
                        mask[sampleY * Resolution + sampleX] <= 0.92f)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void BuildCatalogImages(
            CatalogBuffers raw,
            CatalogBuffers processed,
            ProjectionResult result)
        {
            int pixelCount = Resolution * Resolution;
            result.Raw = new Color32[pixelCount];
            result.Neutral = new Color32[pixelCount];
            result.Processed = new Color32[pixelCount];
            result.Strong = new Color32[pixelCount];
            result.Height = new Color32[pixelCount];
            result.ProcessedHeight = new Color32[pixelCount];
            result.Normals = new Color32[pixelCount];
            result.ProcessedNormals = new Color32[pixelCount];
            result.Mask = new Color32[pixelCount];
            result.Variation = new Color32[pixelCount];
            result.ProcessedVariation = new Color32[pixelCount];
            result.Exposure = new Color32[pixelCount];
            result.ProcessedExposure = new Color32[pixelCount];
            result.Crevice = new Color32[pixelCount];
            result.ProcessedCrevice = new Color32[pixelCount];
            result.EdgeWear = new Color32[pixelCount];
            result.ProcessedEdgeWear = new Color32[pixelCount];

            Vector3 lightDirection = new Vector3(
                -0.55f,
                0.72f,
                0.42f).normalized;
            Color backgroundA = new Color(0.16f, 0.14f, 0.11f, 1f);
            Color backgroundB = new Color(0.20f, 0.18f, 0.14f, 1f);
            Color rockDark = new Color(0.24f, 0.25f, 0.23f, 1f);
            Color rockLight = new Color(0.62f, 0.61f, 0.55f, 1f);

            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    int index = y * Resolution + x;
                    float checker = ((x / 32 + y / 32) & 1) == 0
                        ? 0f
                        : 1f;
                    Color background = Color.Lerp(
                        backgroundA,
                        backgroundB,
                        checker * 0.32f);
                    float mask = raw.Mask[index];
                    result.Mask[index] = Grayscale(ToByte(mask));
                    result.Height[index] = Grayscale(
                        ToByte(raw.Height[index]));
                    result.ProcessedHeight[index] = Grayscale(
                        ToByte(processed.Height[index]));
                    result.Variation[index] = Grayscale(
                        ToByte(raw.Variation[index]));
                    result.ProcessedVariation[index] = Grayscale(
                        ToByte(processed.Variation[index]));
                    result.Exposure[index] = Grayscale(
                        ToByte(raw.Exposure[index]));
                    result.ProcessedExposure[index] = Grayscale(
                        ToByte(processed.Exposure[index]));
                    result.Crevice[index] = Grayscale(
                        ToByte(raw.Crevice[index]));
                    result.ProcessedCrevice[index] = Grayscale(
                        ToByte(processed.Crevice[index]));
                    result.EdgeWear[index] = Grayscale(
                        ToByte(raw.EdgeWear[index]));
                    result.ProcessedEdgeWear[index] = Grayscale(
                        ToByte(processed.EdgeWear[index]));

                    if (mask <= 0.5f)
                    {
                        Color32 encodedUp = new Color32(
                            128,
                            255,
                            128,
                            255);
                        result.Normals[index] = encodedUp;
                        result.ProcessedNormals[index] = encodedUp;
                        Color32 backgroundPixel = (Color32)background;
                        result.Raw[index] = backgroundPixel;
                        result.Neutral[index] = backgroundPixel;
                        result.Processed[index] = backgroundPixel;
                        result.Strong[index] = backgroundPixel;
                        continue;
                    }

                    Vector3 rawNormal = raw.Normals[index].normalized;
                    Vector3 processedNormal =
                        processed.Normals[index].normalized;
                    result.Normals[index] = EncodeWorldNormal(rawNormal);
                    result.ProcessedNormals[index] =
                        EncodeWorldNormal(processedNormal);

                    float rawLight = Mathf.Clamp01(
                        Vector3.Dot(rawNormal, lightDirection) * 0.5f + 0.5f);
                    float rawValue = Mathf.Clamp01(
                        0.24f +
                        raw.Height[index] * 0.28f +
                        raw.Variation[index] * 0.18f +
                        raw.Exposure[index] * 0.14f +
                        rawLight * 0.16f);
                    Color rawColor = Color.Lerp(
                        rockDark,
                        rockLight,
                        rawValue);
                    rawColor = Color.Lerp(
                        rawColor,
                        rockLight * 1.03f,
                        raw.EdgeWear[index] * 0.22f);
                    rawColor = Color.Lerp(
                        rawColor,
                        rockDark * 0.72f,
                        raw.Crevice[index] * 0.28f);
                    result.Raw[index] = (Color32)rawColor;
                    result.Neutral[index] = (Color32)BuildProcessedMaterialColor(
                        processed,
                        index,
                        lightDirection,
                        rockDark,
                        rockLight,
                        MaterialResponseMode.Neutral);
                    result.Processed[index] = (Color32)BuildProcessedMaterialColor(
                        processed,
                        index,
                        lightDirection,
                        rockDark,
                        rockLight,
                        MaterialResponseMode.Moderate);
                    result.Strong[index] = (Color32)BuildProcessedMaterialColor(
                        processed,
                        index,
                        lightDirection,
                        rockDark,
                        rockLight,
                        MaterialResponseMode.Strong);
                }
            }
        }

        private static Color BuildProcessedMaterialColor(
            CatalogBuffers processed,
            int index,
            Vector3 lightDirection,
            Color rockDark,
            Color rockLight,
            MaterialResponseMode mode)
        {
            float minimumLighting;
            float maximumLighting;
            float contrastLow;
            float contrastHigh;
            float wearStrength;
            float rootStrength;
            switch (mode)
            {
                case MaterialResponseMode.Neutral:
                    minimumLighting = 0.80f;
                    maximumLighting = 1.10f;
                    contrastLow = 0.16f;
                    contrastHigh = 0.84f;
                    wearStrength = 0.12f;
                    rootStrength = 0.24f;
                    break;
                case MaterialResponseMode.Strong:
                    minimumLighting = 0.36f;
                    maximumLighting = 1.44f;
                    contrastLow = 0.33f;
                    contrastHigh = 0.67f;
                    wearStrength = 0.52f;
                    rootStrength = 0.72f;
                    break;
                default:
                    minimumLighting = 0.56f;
                    maximumLighting = 1.28f;
                    contrastLow = 0.25f;
                    contrastHigh = 0.75f;
                    wearStrength = 0.34f;
                    rootStrength = 0.54f;
                    break;
            }

            Vector3 normal = processed.Normals[index].normalized;
            float light = Mathf.Clamp01(
                Vector3.Dot(normal, lightDirection) * 0.5f + 0.5f);
            float contrastLight = SmoothStep(
                contrastLow,
                contrastHigh,
                light);
            float lighting = Mathf.Lerp(
                minimumLighting,
                maximumLighting,
                contrastLight);
            float materialValue = Mathf.Clamp01(
                0.34f +
                processed.Height[index] * 0.22f +
                (processed.Variation[index] - 0.5f) * 0.42f +
                processed.Exposure[index] * 0.12f);
            Color color = Color.Lerp(
                rockDark,
                rockLight,
                materialValue) * lighting;
            color = Color.Lerp(
                color,
                rockLight * 1.08f,
                processed.EdgeWear[index] * wearStrength);
            color = Color.Lerp(
                color,
                rockDark * 0.48f,
                processed.Crevice[index] * rootStrength);
            return color;
        }

        private static Color32[] BuildProcessedColorImage(
            CatalogBuffers raw,
            CatalogBuffers processed)
        {
            Color32[] output = new Color32[Resolution * Resolution];
            Vector3 lightDirection = new Vector3(
                -0.55f,
                0.72f,
                0.42f).normalized;
            Color backgroundA = new Color(0.16f, 0.14f, 0.11f, 1f);
            Color backgroundB = new Color(0.20f, 0.18f, 0.14f, 1f);
            Color rockDark = new Color(0.24f, 0.25f, 0.23f, 1f);
            Color rockLight = new Color(0.62f, 0.61f, 0.55f, 1f);
            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    int index = y * Resolution + x;
                    float checker = ((x / 32 + y / 32) & 1) == 0
                        ? 0f
                        : 1f;
                    Color background = Color.Lerp(
                        backgroundA,
                        backgroundB,
                        checker * 0.32f);
                    output[index] = raw.Mask[index] > 0.5f
                        ? (Color32)BuildProcessedMaterialColor(
                            processed,
                            index,
                            lightDirection,
                            rockDark,
                            rockLight,
                            MaterialResponseMode.Moderate)
                        : (Color32)background;
                }
            }

            return output;
        }

        private static void ApplyCatalogLabels(ProjectionResult result)
        {
            ApplyLabels(result.Raw, result.Rocks);
            ApplyLabels(result.Neutral, result.Rocks);
            ApplyLabels(result.Processed, result.Rocks);
            ApplyLabels(result.Strong, result.Rocks);
            ApplyLabels(result.Height, result.Rocks);
            ApplyLabels(result.ProcessedHeight, result.Rocks);
            ApplyLabels(result.Normals, result.Rocks);
            ApplyLabels(result.ProcessedNormals, result.Rocks);
            ApplyLabels(result.Mask, result.Rocks);
            ApplyLabels(result.Variation, result.Rocks);
            ApplyLabels(result.ProcessedVariation, result.Rocks);
            ApplyLabels(result.Exposure, result.Rocks);
            ApplyLabels(result.ProcessedExposure, result.Rocks);
            ApplyLabels(result.Crevice, result.Rocks);
            ApplyLabels(result.ProcessedCrevice, result.Rocks);
            ApplyLabels(result.EdgeWear, result.Rocks);
            ApplyLabels(result.ProcessedEdgeWear, result.Rocks);
        }

        private static void ApplyLabels(
            Color32[] pixels,
            IReadOnlyList<RockEvidence> rocks)
        {
            if (pixels == null)
            {
                return;
            }

            int cellWidth = Resolution / CatalogColumns;
            int cellHeight = Resolution / CatalogRows;
            Color32 background = new Color32(18, 18, 18, 255);
            Color32 text = new Color32(236, 236, 228, 255);
            Color32 frozenText = new Color32(255, 230, 148, 255);
            for (int index = 0; index < rocks.Count; index++)
            {
                RockEvidence rock = rocks[index];
                int column = index % CatalogColumns;
                int row = index / CatalogColumns;
                RectInt labelRect = new RectInt(
                    column * cellWidth,
                    row * cellHeight,
                    cellWidth,
                    LabelStripHeight);
                FillRect(pixels, Resolution, labelRect, background);
                string path = rock.UsedFallbackMesh ? "F" : "U";
                string line1 = rock.StableId + " " +
                    rock.ProfileCode + " " + path;
                string line2 = "SH" +
                    rock.ShapeSeed.ToString(
                        "0000",
                        CultureInfo.InvariantCulture) +
                    " SU" +
                    rock.SurfaceSeed.ToString(
                        "0000",
                        CultureInfo.InvariantCulture);
                string line3 = "B" +
                    Mathf.RoundToInt(rock.BurialFraction * 100f).ToString(
                        "00",
                        CultureInfo.InvariantCulture) +
                    "% EW" +
                    Mathf.RoundToInt(rock.EdgeWearWidth * 100f).ToString(
                        "000",
                        CultureInfo.InvariantCulture);
                Color32 lineColor = frozenText;
                DrawText(
                    pixels,
                    Resolution,
                    labelRect.xMin + 5,
                    labelRect.yMin + 31,
                    line1,
                    lineColor,
                    2);
                DrawText(
                    pixels,
                    Resolution,
                    labelRect.xMin + 5,
                    labelRect.yMin + 17,
                    line2,
                    text,
                    2);
                DrawText(
                    pixels,
                    Resolution,
                    labelRect.xMin + 5,
                    labelRect.yMin + 3,
                    line3,
                    text,
                    2);
            }
        }

        private static Color32[] BuildBurialComparison(
            ICollection<string> sourceIds,
            out int cellCount)
        {
            const int columns = 4;
            const int rows = 4;
            CatalogBuffers raw = new CatalogBuffers(Resolution);
            List<RockEvidence> labels = new List<RockEvidence>(16);
            List<RockDefinition> definitions = BuildRockDefinitions();
            int index = 0;
            for (int sourceIndex = 0;
                 sourceIndex < BurialComparisonSourceIds.Length;
                 sourceIndex++)
            {
                string sourceId = BurialComparisonSourceIds[sourceIndex];
                RockDefinition source = FindDefinition(
                    definitions,
                    sourceId);
                sourceIds.Add(sourceId);
                GeneratedRock generated = GenerateRock(source);
                for (int burialIndex = 0;
                     burialIndex < BurialComparisonFractions.Length;
                     burialIndex++)
                {
                    RockDefinition definition = new RockDefinition
                    {
                        Index = index,
                        StableId = source.StableId,
                        Archetype = source.Archetype,
                        Profile = source.Profile,
                        ShapeSeed = source.ShapeSeed,
                        SurfaceSeed = source.SurfaceSeed,
                        BurialFraction =
                            BurialComparisonFractions[burialIndex],
                        RotationDegrees = source.RotationDegrees,
                        IsFrozen = true
                    };
                    generated.Definition = definition;
                    RockEvidence evidence = RasterizeRockIntoCell(
                        generated,
                        raw,
                        Resolution,
                        columns,
                        rows,
                        LabelStripHeight);
                    labels.Add(evidence);
                    index++;
                }
            }

            CatalogBuffers processed = BuildProcessedBuffers(
                raw,
                labels,
                columns,
                rows);
            Color32[] output = BuildProcessedColorImage(raw, processed);
            ApplyBurialLabels(output, labels, columns, rows);
            cellCount = labels.Count;
            return output;
        }

        private static RockDefinition FindDefinition(
            IReadOnlyList<RockDefinition> definitions,
            string stableId)
        {
            for (int index = 0; index < definitions.Count; index++)
            {
                RockDefinition definition = definitions[index];
                if (string.Equals(
                        definition.StableId,
                        stableId,
                        StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            throw new InvalidOperationException(
                "Required burial-comparison source is absent: " +
                stableId + ".");
        }

        private static void ApplyBurialLabels(
            Color32[] pixels,
            IReadOnlyList<RockEvidence> rocks,
            int columns,
            int rows)
        {
            int cellWidth = Resolution / columns;
            int cellHeight = Resolution / rows;
            Color32 background = new Color32(18, 18, 18, 255);
            Color32 text = new Color32(246, 236, 202, 255);
            for (int index = 0; index < rocks.Count; index++)
            {
                RockEvidence rock = rocks[index];
                int column = index % columns;
                int row = index / columns;
                RectInt labelRect = new RectInt(
                    column * cellWidth,
                    row * cellHeight,
                    cellWidth,
                    LabelStripHeight);
                FillRect(pixels, Resolution, labelRect, background);
                string line = rock.StableId + " B" +
                    Mathf.RoundToInt(rock.BurialFraction * 100f).ToString(
                        "00",
                        CultureInfo.InvariantCulture) + "%";
                DrawText(
                    pixels,
                    Resolution,
                    labelRect.xMin + 8,
                    labelRect.yMin + 16,
                    line,
                    text,
                    2);
            }
        }

        private static string CalculateRockFingerprint(
            RockEvidence rock,
            CatalogBuffers buffers,
            int resolution,
            RectInt cell)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(AlgorithmVersion);
                writer.Write(cell.width);
                writer.Write(cell.height);
                for (int y = cell.yMin; y < cell.yMax; y++)
                {
                    for (int x = cell.xMin; x < cell.xMax; x++)
                    {
                        int index = y * resolution + x;
                        writer.Write(buffers.Mask[index]);
                        writer.Write(buffers.Height[index]);
                        writer.Write(buffers.Normals[index].x);
                        writer.Write(buffers.Normals[index].y);
                        writer.Write(buffers.Normals[index].z);
                        writer.Write(buffers.Variation[index]);
                        writer.Write(buffers.Exposure[index]);
                        writer.Write(buffers.Crevice[index]);
                        writer.Write(buffers.EdgeWear[index]);
                    }
                }

                writer.Flush();
                return CalculateSha256(stream.ToArray());
            }
        }

        private static string CalculateFingerprint(
            ProjectionResult result)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(AlgorithmVersion);
                writer.Write(result.Rocks.Count);
                for (int index = 0; index < result.Rocks.Count; index++)
                {
                    RockEvidence rock = result.Rocks[index];
                    writer.Write(rock.StableId ?? string.Empty);
                    writer.Write(rock.Fingerprint ?? string.Empty);
                    writer.Write(rock.UsedFallbackMesh);
                    writer.Write(rock.FallbackReason ?? string.Empty);
                }

                WritePixels(writer, result.Raw);
                WritePixels(writer, result.Neutral);
                WritePixels(writer, result.Processed);
                WritePixels(writer, result.Strong);
                WritePixels(writer, result.Height);
                WritePixels(writer, result.ProcessedHeight);
                WritePixels(writer, result.Normals);
                WritePixels(writer, result.ProcessedNormals);
                WritePixels(writer, result.Mask);
                WritePixels(writer, result.Variation);
                WritePixels(writer, result.ProcessedVariation);
                WritePixels(writer, result.Exposure);
                WritePixels(writer, result.ProcessedExposure);
                WritePixels(writer, result.Crevice);
                WritePixels(writer, result.ProcessedCrevice);
                WritePixels(writer, result.EdgeWear);
                WritePixels(writer, result.ProcessedEdgeWear);
                WritePixels(writer, result.BurialComparison);
                writer.Flush();
                return CalculateSha256(stream.ToArray());
            }
        }

        private static string CalculateSha256(byte[] bytes)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int index = 0; index < hash.Length; index++)
                {
                    builder.Append(hash[index].ToString(
                        "x2",
                        CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }

        private static void WritePixels(
            BinaryWriter writer,
            Color32[] pixels)
        {
            writer.Write(pixels != null ? pixels.Length : 0);
            if (pixels == null)
            {
                return;
            }

            for (int index = 0; index < pixels.Length; index++)
            {
                writer.Write(pixels[index].r);
                writer.Write(pixels[index].g);
                writer.Write(pixels[index].b);
                writer.Write(pixels[index].a);
            }
        }

        private static Color32 Grayscale(byte value)
        {
            return new Color32(value, value, value, 255);
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.RoundToInt(
                Mathf.Clamp01(value) * 255f);
        }

        private static Color32 EncodeWorldNormal(Vector3 normal)
        {
            return new Color32(
                ToByte(normal.x * 0.5f + 0.5f),
                ToByte(normal.y * 0.5f + 0.5f),
                ToByte(normal.z * 0.5f + 0.5f),
                255);
        }

        private static float SmoothStep(
            float edge0,
            float edge1,
            float value)
        {
            if (Mathf.Abs(edge1 - edge0) <= 0.000001f)
            {
                return value >= edge1 ? 1f : 0f;
            }

            float t = Mathf.Clamp01(
                (value - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static void FillRect(
            Color32[] pixels,
            int resolution,
            RectInt rect,
            Color32 color)
        {
            int minimumX = Mathf.Clamp(rect.xMin, 0, resolution);
            int maximumX = Mathf.Clamp(rect.xMax, 0, resolution);
            int minimumY = Mathf.Clamp(rect.yMin, 0, resolution);
            int maximumY = Mathf.Clamp(rect.yMax, 0, resolution);
            for (int y = minimumY; y < maximumY; y++)
            {
                for (int x = minimumX; x < maximumX; x++)
                {
                    pixels[y * resolution + x] = color;
                }
            }
        }

        private static void DrawText(
            Color32[] pixels,
            int resolution,
            int startX,
            int startY,
            string text,
            Color32 color,
            int scale)
        {
            int cursorX = startX;
            for (int characterIndex = 0;
                 characterIndex < text.Length;
                 characterIndex++)
            {
                char character = char.ToUpperInvariant(text[characterIndex]);
                string[] glyph;
                if (!BitmapFont.TryGetValue(character, out glyph))
                {
                    glyph = BitmapFont[' '];
                }

                for (int row = 0; row < glyph.Length; row++)
                {
                    for (int column = 0;
                         column < glyph[row].Length;
                         column++)
                    {
                        if (glyph[row][column] != '1')
                        {
                            continue;
                        }

                        int pixelX = cursorX + column * scale;
                        int pixelY = startY +
                            (glyph.Length - 1 - row) * scale;
                        for (int offsetY = 0; offsetY < scale; offsetY++)
                        {
                            for (int offsetX = 0;
                                 offsetX < scale;
                                 offsetX++)
                            {
                                int x = pixelX + offsetX;
                                int y = pixelY + offsetY;
                                if (x >= 0 && x < resolution &&
                                    y >= 0 && y < resolution)
                                {
                                    pixels[y * resolution + x] = color;
                                }
                            }
                        }
                    }
                }

                cursorX += 6 * scale;
            }
        }

        private static Dictionary<char, string[]> BuildBitmapFont()
        {
            Dictionary<char, string[]> font =
                new Dictionary<char, string[]>();
            font[' '] = Glyph("00000", "00000", "00000", "00000", "00000", "00000", "00000");
            font['-'] = Glyph("00000", "00000", "00000", "11111", "00000", "00000", "00000");
            font['%'] = Glyph("11001", "11010", "00100", "01000", "10110", "00110", "00000");
            font['0'] = Glyph("01110", "10001", "10011", "10101", "11001", "10001", "01110");
            font['1'] = Glyph("00100", "01100", "00100", "00100", "00100", "00100", "01110");
            font['2'] = Glyph("01110", "10001", "00001", "00010", "00100", "01000", "11111");
            font['3'] = Glyph("11110", "00001", "00001", "01110", "00001", "00001", "11110");
            font['4'] = Glyph("00010", "00110", "01010", "10010", "11111", "00010", "00010");
            font['5'] = Glyph("11111", "10000", "10000", "11110", "00001", "00001", "11110");
            font['6'] = Glyph("01110", "10000", "10000", "11110", "10001", "10001", "01110");
            font['7'] = Glyph("11111", "00001", "00010", "00100", "01000", "01000", "01000");
            font['8'] = Glyph("01110", "10001", "10001", "01110", "10001", "10001", "01110");
            font['9'] = Glyph("01110", "10001", "10001", "01111", "00001", "00001", "01110");
            font['A'] = Glyph("01110", "10001", "10001", "11111", "10001", "10001", "10001");
            font['B'] = Glyph("11110", "10001", "10001", "11110", "10001", "10001", "11110");
            font['C'] = Glyph("01111", "10000", "10000", "10000", "10000", "10000", "01111");
            font['E'] = Glyph("11111", "10000", "10000", "11110", "10000", "10000", "11111");
            font['F'] = Glyph("11111", "10000", "10000", "11110", "10000", "10000", "10000");
            font['H'] = Glyph("10001", "10001", "10001", "11111", "10001", "10001", "10001");
            font['L'] = Glyph("10000", "10000", "10000", "10000", "10000", "10000", "11111");
            font['R'] = Glyph("11110", "10001", "10001", "11110", "10100", "10010", "10001");
            font['S'] = Glyph("01111", "10000", "10000", "01110", "00001", "00001", "11110");
            font['T'] = Glyph("11111", "00100", "00100", "00100", "00100", "00100", "00100");
            font['U'] = Glyph("10001", "10001", "10001", "10001", "10001", "10001", "01110");
            font['W'] = Glyph("10001", "10001", "10001", "10101", "10101", "10101", "01010");
            return font;
        }

        private static string[] Glyph(
            string row0,
            string row1,
            string row2,
            string row3,
            string row4,
            string row5,
            string row6)
        {
            return new[]
            {
                row0,
                row1,
                row2,
                row3,
                row4,
                row5,
                row6
            };
        }
    }
}
