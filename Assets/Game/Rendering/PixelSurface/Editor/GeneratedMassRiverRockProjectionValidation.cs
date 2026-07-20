using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using ProgrammaticStylized3D.Geometry.Masses;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    internal static class GeneratedMassRiverRockProjectionValidation
    {
        private const string OutputDirectory =
            "Library/SurfaceMaterialDiagnostics/" +
            "GeneratedMassRiverRockProjection";
        private const string ReportPath = OutputDirectory +
            "/GeneratedMassRiverRockMaterialRefinementReport.txt";

        private sealed class FrozenContract
        {
            internal string StableId;
            internal MassArchetype Archetype;
            internal int ShapeSeed;
            internal int SurfaceSeed;
            internal float BurialFraction;
            internal float RotationDegrees;
        }

        private static readonly FrozenContract[] FrozenLibrary =
        {
            Contract("T-05", MassArchetype.TerrainBoulder, 3187, 4134, 0.226f, 186f),
            Contract("T-08", MassArchetype.TerrainBoulder, 1291, 1254, 0.218f, 73f),
            Contract("T-09", MassArchetype.TerrainBoulder, 3473, 6660, 0.226f, 145f),
            Contract("T-10", MassArchetype.TerrainBoulder, 5237, 9140, 0.234f, 206f),
            Contract("T-11", MassArchetype.TerrainBoulder, 8123, 9475, 0.242f, 279f),
            Contract("T-12", MassArchetype.TerrainBoulder, 1579, 2222, 0.218f, 201f),
            Contract("T-13", MassArchetype.TerrainBoulder, 3821, 8048, 0.226f, 259f),
            Contract("T-14", MassArchetype.TerrainBoulder, 6173, 4645, 0.234f, 353f),
            Contract("T-15", MassArchetype.TerrainBoulder, 9431, 7584, 0.242f, 68f),
            Contract("S-00", MassArchetype.SquatBoulder, 5727, 2238, 0.218f, 246f),
            Contract("S-03", MassArchetype.SquatBoulder, 7319, 3776, 0.242f, 106f),
            Contract("S-04", MassArchetype.SquatBoulder, 1117, 489, 0.218f, 156f),
            Contract("S-08", MassArchetype.SquatBoulder, 1361, 2721, 0.218f, 110f),
            Contract("S-09", MassArchetype.SquatBoulder, 3593, 8477, 0.226f, 158f),
            Contract("S-10", MassArchetype.SquatBoulder, 5393, 1210, 0.234f, 255f),
            Contract("S-12", MassArchetype.SquatBoulder, 1693, 3997, 0.218f, 222f),
            Contract("S-13", MassArchetype.SquatBoulder, 4001, 286, 0.226f, 322f),
            Contract("S-14", MassArchetype.SquatBoulder, 6311, 6588, 0.234f, 35f)
        };

        private static readonly string[] RequiredBurialSources =
        {
            "S-12", "S-14", "T-13", "T-15"
        };

        private static readonly string[] LegacyEvidenceFiles =
        {
            "GeneratedMassRiverRockProjectionReport.txt",
            "GeneratedMassRiverRockFamilySweepReport.txt",
            "RockCatalog_Color.png",
            "RockCatalog_Height.png",
            "RockCatalog_Normals.png",
            "RockCatalog_Mask.png",
            "RockCatalog_Variation.png",
            "RockCatalog_Exposure.png",
            "RockCatalog_Crevice.png",
            "RockCatalog_EdgeWear.png",
            "RockCatalog_BurialComparison.png",
            "RockFamilySweep_Neutral.png",
            "RockFamilySweep_Stylized.png",
            "RockFamilySweep_Raw.png",
            "RockFamilySweep_Processed.png",
            "RockFamilySweep_Height.png",
            "RockFamilySweep_ProcessedHeight.png",
            "RockFamilySweep_Normals.png",
            "RockFamilySweep_ProcessedNormals.png",
            "RockFamilySweep_Mask.png",
            "RockFamilySweep_Variation.png",
            "RockFamilySweep_ProcessedVariation.png",
            "RockFamilySweep_Exposure.png",
            "RockFamilySweep_ProcessedExposure.png",
            "RockFamilySweep_Crevice.png",
            "RockFamilySweep_ProcessedCrevice.png",
            "RockFamilySweep_EdgeWear.png",
            "RockFamilySweep_ProcessedEdgeWear.png",
            "RockFamilySweep_BurialComparison.png"
        };

        [MenuItem(
            "Tools/PS3D/Run Generated Mass River-Rock Material Refinement")]
        private static void RunMenuAction()
        {
            Directory.CreateDirectory(OutputDirectory);
            DeleteLegacyEvidence();
            GeneratedMassRiverRockProjectionBaker.ProjectionResult first =
                GeneratedMassRiverRockProjectionBaker.Build();
            GeneratedMassRiverRockProjectionBaker.ProjectionResult second =
                GeneratedMassRiverRockProjectionBaker.Build();
            List<string> failures = new List<string>();

            if (!first.Succeeded)
            {
                failures.Add("First material-refinement build failed: " +
                    first.Failure);
            }

            if (!second.Succeeded)
            {
                failures.Add("Repeated material-refinement build failed: " +
                    second.Failure);
            }

            bool deterministic = first.Succeeded && second.Succeeded &&
                string.Equals(
                    first.Fingerprint,
                    second.Fingerprint,
                    StringComparison.Ordinal);
            if (!deterministic)
            {
                failures.Add(
                    "Repeated material refinement produced a different " +
                    "catalog fingerprint.");
            }

            if (first.Succeeded)
            {
                ValidateResult(first, second, failures);
                WriteEvidence(first);
            }

            string report = BuildReport(
                first,
                second,
                deterministic,
                failures);
            File.WriteAllText(ReportPath, report, Encoding.UTF8);
            EditorGUIUtility.systemCopyBuffer = report;

            if (failures.Count == 0)
            {
                Debug.Log(
                    "[GSU-M2.7C.5C] Generated Mass frozen river-rock " +
                    "material refinement passed mechanical validation. " +
                    "Report written to " + ReportPath +
                    " and copied to the clipboard. Visual material and " +
                    "burial acceptance remain pending.");
            }
            else
            {
                Debug.LogError(
                    "[GSU-M2.7C.5C] Generated Mass frozen river-rock " +
                    "material refinement failed " + failures.Count +
                    " check(s). Report written to " + ReportPath +
                    " and copied to the clipboard.");
            }
        }

        private static void ValidateResult(
            GeneratedMassRiverRockProjectionBaker.ProjectionResult first,
            GeneratedMassRiverRockProjectionBaker.ProjectionResult second,
            ICollection<string> failures)
        {
            int expectedCount =
                GeneratedMassRiverRockProjectionBaker.ExpectedRockCount;
            if (first.Rocks.Count != expectedCount)
            {
                failures.Add(
                    "Expected " + expectedCount +
                    " frozen rocks; received " + first.Rocks.Count + ".");
            }

            if (second.Rocks.Count != first.Rocks.Count)
            {
                failures.Add(
                    "Repeated material refinement returned a different " +
                    "rock count.");
            }

            ValidateImages(first, failures);
            ValidateBurialComparison(first, failures);

            int terrainCount = 0;
            int squatCount = 0;
            int frozenCount = 0;
            HashSet<string> stableIds = new HashSet<string>(
                StringComparer.Ordinal);
            HashSet<string> fingerprints = new HashSet<string>(
                StringComparer.Ordinal);
            Dictionary<string,
                GeneratedMassRiverRockProjectionBaker.RockEvidence> byId =
                new Dictionary<string,
                    GeneratedMassRiverRockProjectionBaker.RockEvidence>(
                    StringComparer.Ordinal);

            for (int index = 0; index < first.Rocks.Count; index++)
            {
                GeneratedMassRiverRockProjectionBaker.RockEvidence rock =
                    first.Rocks[index];
                if (rock.Archetype == MassArchetype.TerrainBoulder)
                {
                    terrainCount++;
                }
                else if (rock.Archetype == MassArchetype.SquatBoulder)
                {
                    squatCount++;
                }
                else
                {
                    failures.Add(
                        FormatRock(rock) +
                        ": excluded archetype entered the frozen library.");
                }

                if (!string.Equals(
                        rock.ProfileCode,
                        "UB",
                        StringComparison.Ordinal))
                {
                    failures.Add(
                        FormatRock(rock) +
                        ": profile is not UB / Uneven Broad.");
                }

                if (!rock.IsFrozen)
                {
                    failures.Add(
                        FormatRock(rock) +
                        ": entry is not marked frozen.");
                }
                else
                {
                    frozenCount++;
                }

                string stableId = rock.StableId ?? string.Empty;
                if (!stableIds.Add(stableId))
                {
                    failures.Add(
                        FormatRock(rock) + ": duplicate stable ID.");
                }
                else
                {
                    byId.Add(stableId, rock);
                }

                if (string.IsNullOrEmpty(rock.Fingerprint))
                {
                    failures.Add(
                        FormatRock(rock) +
                        ": missing per-rock fingerprint.");
                }
                else if (!fingerprints.Add(rock.Fingerprint))
                {
                    failures.Add(
                        FormatRock(rock) +
                        ": duplicate per-rock raw output fingerprint.");
                }

                if (rock.VertexCount < 3 || rock.TriangleCount < 1)
                {
                    failures.Add(
                        FormatRock(rock) +
                        ": generated mesh is empty or invalid.");
                }

                if (rock.OccupiedPixels <= 0 || rock.HeightRange <= 0f)
                {
                    failures.Add(
                        FormatRock(rock) +
                        ": projected geometry has no visible volume.");
                }

                if (float.IsNaN(rock.NormalVariance) ||
                    float.IsInfinity(rock.NormalVariance))
                {
                    failures.Add(
                        FormatRock(rock) +
                        ": normal-variance metric is not finite.");
                }
            }

            if (terrainCount !=
                GeneratedMassRiverRockProjectionBaker.ExpectedTerrainCount)
            {
                failures.Add(
                    "Frozen Terrain count is " + terrainCount +
                    "; expected " +
                    GeneratedMassRiverRockProjectionBaker
                        .ExpectedTerrainCount + ".");
            }

            if (squatCount !=
                GeneratedMassRiverRockProjectionBaker.ExpectedSquatCount)
            {
                failures.Add(
                    "Frozen Squat count is " + squatCount +
                    "; expected " +
                    GeneratedMassRiverRockProjectionBaker
                        .ExpectedSquatCount + ".");
            }

            if (frozenCount !=
                GeneratedMassRiverRockProjectionBaker.ExpectedFrozenCount)
            {
                failures.Add(
                    "Frozen marker count is " + frozenCount +
                    "; expected " +
                    GeneratedMassRiverRockProjectionBaker
                        .ExpectedFrozenCount + ".");
            }

            ValidateFrozenLibrary(byId, failures);
            ValidateRepeatedRockFingerprints(first, second, failures);
        }

        private static void ValidateFrozenLibrary(
            IReadOnlyDictionary<string,
                GeneratedMassRiverRockProjectionBaker.RockEvidence> byId,
            ICollection<string> failures)
        {
            if (FrozenLibrary.Length !=
                GeneratedMassRiverRockProjectionBaker.ExpectedFrozenCount)
            {
                failures.Add(
                    "Validator frozen-library contract has an invalid count.");
            }

            for (int index = 0; index < FrozenLibrary.Length; index++)
            {
                FrozenContract contract = FrozenLibrary[index];
                if (!byId.TryGetValue(
                        contract.StableId,
                        out GeneratedMassRiverRockProjectionBaker.RockEvidence
                            rock))
                {
                    failures.Add(
                        "Frozen source is missing: " + contract.StableId + ".");
                    continue;
                }

                if (rock.Archetype != contract.Archetype ||
                    rock.ShapeSeed != contract.ShapeSeed ||
                    rock.SurfaceSeed != contract.SurfaceSeed ||
                    !Approximately(
                        rock.BurialFraction,
                        contract.BurialFraction) ||
                    !Approximately(
                        rock.RotationDegrees,
                        contract.RotationDegrees))
                {
                    failures.Add(
                        contract.StableId +
                        ": frozen generation/projection settings changed.");
                }
            }

            if (byId.Count != FrozenLibrary.Length)
            {
                failures.Add(
                    "Frozen catalog contains an unexpected stable ID.");
            }
        }

        private static void ValidateRepeatedRockFingerprints(
            GeneratedMassRiverRockProjectionBaker.ProjectionResult first,
            GeneratedMassRiverRockProjectionBaker.ProjectionResult second,
            ICollection<string> failures)
        {
            Dictionary<string, string> repeated =
                new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index < second.Rocks.Count; index++)
            {
                GeneratedMassRiverRockProjectionBaker.RockEvidence rock =
                    second.Rocks[index];
                repeated[rock.StableId ?? string.Empty] =
                    rock.Fingerprint ?? string.Empty;
            }

            for (int index = 0; index < first.Rocks.Count; index++)
            {
                GeneratedMassRiverRockProjectionBaker.RockEvidence rock =
                    first.Rocks[index];
                if (!repeated.TryGetValue(
                        rock.StableId ?? string.Empty,
                        out string fingerprint) ||
                    !string.Equals(
                        rock.Fingerprint,
                        fingerprint,
                        StringComparison.Ordinal))
                {
                    failures.Add(
                        FormatRock(rock) +
                        ": repeated per-rock fingerprint changed.");
                }
            }
        }

        private static void ValidateBurialComparison(
            GeneratedMassRiverRockProjectionBaker.ProjectionResult result,
            ICollection<string> failures)
        {
            if (result.BurialComparisonCellCount != 16)
            {
                failures.Add(
                    "Burial comparison contains " +
                    result.BurialComparisonCellCount +
                    " cells; expected 16.");
            }

            if (result.BurialSourceIds.Count !=
                RequiredBurialSources.Length)
            {
                failures.Add(
                    "Burial comparison source count changed.");
                return;
            }

            for (int index = 0; index < RequiredBurialSources.Length; index++)
            {
                if (!string.Equals(
                        result.BurialSourceIds[index],
                        RequiredBurialSources[index],
                        StringComparison.Ordinal))
                {
                    failures.Add(
                        "Burial comparison source order changed at index " +
                        index + ".");
                }
            }
        }

        private static void ValidateImages(
            GeneratedMassRiverRockProjectionBaker.ProjectionResult result,
            ICollection<string> failures)
        {
            int expectedPixels =
                GeneratedMassRiverRockProjectionBaker.Resolution *
                GeneratedMassRiverRockProjectionBaker.Resolution;
            ValidatePixelCount("RawGeometry", result.Raw, expectedPixels, failures);
            ValidatePixelCount("Neutral", result.Neutral, expectedPixels, failures);
            ValidatePixelCount("Moderate", result.Processed, expectedPixels, failures);
            ValidatePixelCount("Strong", result.Strong, expectedPixels, failures);
            ValidatePixelCount("RawHeight", result.Height, expectedPixels, failures);
            ValidatePixelCount(
                "ProcessedHeight",
                result.ProcessedHeight,
                expectedPixels,
                failures);
            ValidatePixelCount("RawNormals", result.Normals, expectedPixels, failures);
            ValidatePixelCount(
                "ProcessedNormals",
                result.ProcessedNormals,
                expectedPixels,
                failures);
            ValidatePixelCount("Mask", result.Mask, expectedPixels, failures);
            ValidatePixelCount(
                "RawVariation",
                result.Variation,
                expectedPixels,
                failures);
            ValidatePixelCount(
                "MaterialVariation",
                result.ProcessedVariation,
                expectedPixels,
                failures);
            ValidatePixelCount(
                "RawExposure",
                result.Exposure,
                expectedPixels,
                failures);
            ValidatePixelCount(
                "ProcessedExposure",
                result.ProcessedExposure,
                expectedPixels,
                failures);
            ValidatePixelCount(
                "RawCrevice",
                result.Crevice,
                expectedPixels,
                failures);
            ValidatePixelCount(
                "RootDarkening",
                result.ProcessedCrevice,
                expectedPixels,
                failures);
            ValidatePixelCount(
                "RawEdgeWear",
                result.EdgeWear,
                expectedPixels,
                failures);
            ValidatePixelCount(
                "ProcessedEdgeWear",
                result.ProcessedEdgeWear,
                expectedPixels,
                failures);
            ValidatePixelCount(
                "BurialComparison",
                result.BurialComparison,
                expectedPixels,
                failures);
        }

        private static void ValidatePixelCount(
            string label,
            Color32[] pixels,
            int expected,
            ICollection<string> failures)
        {
            int actual = pixels != null ? pixels.Length : 0;
            if (actual != expected)
            {
                failures.Add(
                    label + " pixel count is " + actual +
                    "; expected " + expected + ".");
            }
        }

        private static string BuildReport(
            GeneratedMassRiverRockProjectionBaker.ProjectionResult first,
            GeneratedMassRiverRockProjectionBaker.ProjectionResult second,
            bool deterministic,
            IReadOnlyCollection<string> failures)
        {
            StringBuilder builder = new StringBuilder(32768);
            builder.AppendLine(
                "GENERATED MASS FROZEN RIVER-ROCK MATERIAL REFINEMENT — " +
                "GSU-M2.7C.5C");
            builder.AppendLine(
                "Generated UTC: " +
                DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            builder.AppendLine("Unity: " + Application.unityVersion);
            builder.AppendLine(
                "Algorithm version: " +
                GeneratedMassRiverRockProjectionBaker.AlgorithmVersion);
            builder.AppendLine(
                "Catalog resolution: " +
                GeneratedMassRiverRockProjectionBaker.Resolution + " x " +
                GeneratedMassRiverRockProjectionBaker.Resolution);
            builder.AppendLine(
                "Runtime integration: None — all outputs remain local under " +
                "Library.");
            builder.AppendLine(
                "Source generation: frozen 18-rock Terrain/Squat Uneven Broad " +
                "library; no seed exploration.");
            builder.AppendLine();

            builder.AppendLine("DETERMINISM");
            builder.AppendLine(
                "First catalog fingerprint: " +
                (first.Fingerprint ?? "FAIL"));
            builder.AppendLine(
                "Second catalog fingerprint: " +
                (second.Fingerprint ?? "FAIL"));
            builder.AppendLine(
                "Repeated catalog identical: " +
                (deterministic ? "Yes" : "No"));
            builder.AppendLine();

            if (!first.Succeeded)
            {
                builder.AppendLine("FIRST BUILD FAILURE");
                builder.AppendLine(first.Failure ?? "Unknown failure.");
                builder.AppendLine();
            }
            else
            {
                AppendSummary(builder, first);
                builder.AppendLine("ROCK ENTRIES");
                for (int index = 0; index < first.Rocks.Count; index++)
                {
                    AppendRock(builder, first.Rocks[index]);
                }

                builder.AppendLine();
            }

            builder.AppendLine("OUTPUTS");
            builder.AppendLine("Report: " + ReportPath);
            builder.AppendLine("RockLibrary_RawGeometry.png");
            builder.AppendLine("RockLibrary_Neutral.png");
            builder.AppendLine("RockLibrary_Moderate.png");
            builder.AppendLine("RockLibrary_Strong.png");
            builder.AppendLine("RockLibrary_RawHeight.png");
            builder.AppendLine("RockLibrary_ProcessedHeight.png");
            builder.AppendLine("RockLibrary_RawNormals.png");
            builder.AppendLine("RockLibrary_ProcessedNormals.png");
            builder.AppendLine("RockLibrary_Mask.png");
            builder.AppendLine("RockLibrary_RawVariation.png");
            builder.AppendLine("RockLibrary_MaterialVariation.png");
            builder.AppendLine("RockLibrary_RawExposure.png");
            builder.AppendLine("RockLibrary_ProcessedExposure.png");
            builder.AppendLine("RockLibrary_RawCrevice.png");
            builder.AppendLine("RockLibrary_RootDarkening.png");
            builder.AppendLine("RockLibrary_RawEdgeWear.png");
            builder.AppendLine("RockLibrary_ProcessedEdgeWear.png");
            builder.AppendLine("RockLibrary_BurialComparison.png");
            builder.AppendLine();

            builder.AppendLine("RECOMMENDED PROGRESSION");
            builder.AppendLine(
                "M2.7C.5C — accept one material-response level and burial " +
                "range for the frozen library.");
            builder.AppendLine(
                "M2.7C.5D — assemble the accepted frozen rocks into a seamless " +
                "sparse riverbed tile.");
            builder.AppendLine(
                "M2.7C.5E — integrate an accepted tile through the ordinary " +
                "Ground material architecture.");
            builder.AppendLine();

            builder.AppendLine("SUMMARY");
            if (failures.Count == 0)
            {
                builder.AppendLine(
                    "VERDICT: PASS — deterministic frozen 18-rock generation, " +
                    "two-scale processed normals, non-triangular material " +
                    "variation, processed-normal exposure, selective root " +
                    "darkening, interior wear, three response previews, burial " +
                    "comparison, and evidence generation passed.");
            }
            else
            {
                builder.AppendLine(
                    "VERDICT: FAIL — " + failures.Count +
                    " issue(s) detected.");
                foreach (string failure in failures)
                {
                    builder.AppendLine("- " + failure);
                }
            }

            builder.AppendLine();
            builder.AppendLine(
                "PENDING GATE: inspect Neutral, Moderate, Strong, processed " +
                "Normals/Height, MaterialVariation, ProcessedExposure, " +
                "RootDarkening, ProcessedEdgeWear, Mask, and BurialComparison. " +
                "No seamless tile or runtime integration is authorized yet.");
            return builder.ToString();
        }

        private static void AppendSummary(
            StringBuilder builder,
            GeneratedMassRiverRockProjectionBaker.ProjectionResult result)
        {
            int terrain = 0;
            int squat = 0;
            int frozen = 0;
            for (int index = 0; index < result.Rocks.Count; index++)
            {
                GeneratedMassRiverRockProjectionBaker.RockEvidence rock =
                    result.Rocks[index];
                terrain += rock.Archetype == MassArchetype.TerrainBoulder
                    ? 1
                    : 0;
                squat += rock.Archetype == MassArchetype.SquatBoulder
                    ? 1
                    : 0;
                frozen += rock.IsFrozen ? 1 : 0;
            }

            builder.AppendLine("CATALOG SUMMARY");
            builder.AppendLine("Rock count: " + result.Rocks.Count);
            builder.AppendLine("Terrain / Squat: " + terrain + " / " + squat);
            builder.AppendLine("Frozen entries: " + frozen);
            builder.AppendLine("Profile: UB / Uneven Broad only");
            builder.AppendLine(
                "Burial comparison sources: " +
                string.Join("/", result.BurialSourceIds));
            builder.AppendLine(
                "Processing: three-pass volume height plus one-pass plane " +
                "height, blended height-derived normals, procedural per-rock " +
                "material variation, normal-derived exposure, directional " +
                "broken root darkening, and silhouette-excluded interior wear.");
            builder.AppendLine(
                "Total generated vertices: " + result.TotalVertices);
            builder.AppendLine(
                "Total generated triangles: " + result.TotalTriangles);
            builder.AppendLine(
                "Unified edge-wear preview fallbacks: " +
                result.FallbackCount);
            builder.AppendLine();
        }

        private static void AppendRock(
            StringBuilder builder,
            GeneratedMassRiverRockProjectionBaker.RockEvidence rock)
        {
            builder.AppendLine(
                "[" + rock.StableId + "] " + rock.Archetype +
                " profile=" + rock.ProfileCode +
                " (" + rock.ProfileName + ") FROZEN");
            builder.AppendLine(
                "    shape/surface: " + rock.ShapeSeed +
                " / " + rock.SurfaceSeed);
            builder.AppendLine(
                "    burial/rotation: " +
                FormatPercent(rock.BurialFraction) +
                " / " + Format(rock.RotationDegrees) + "deg");
            builder.AppendLine(
                "    recipe: complexity=" + rock.FormComplexity +
                ", facets=" + rock.FacetDensity +
                ", edge=" + rock.EdgeCharacter +
                ", diversity=" + rock.ShapeDiversity +
                ", grounding=" + rock.Grounding +
                ", lean=" + rock.Lean);
            builder.AppendLine(
                "    biases W/H/D and source variation: " +
                Format(rock.WidthBias) + " / " +
                Format(rock.HeightBias) + " / " +
                Format(rock.DepthBias) + " / " +
                Format(rock.SurfaceVariation));
            builder.AppendLine(
                "    edge wear amount/width: " +
                Format(rock.EdgeWearAmount) + " / " +
                Format(rock.EdgeWearWidth));
            builder.AppendLine(
                "    mesh vertices/triangles: " +
                rock.VertexCount + " / " + rock.TriangleCount);
            builder.AppendLine(
                "    occupied/aspect/height/normal variance: " +
                rock.OccupiedPixels + " / " +
                Format(rock.SilhouetteAspect) + " / " +
                Format(rock.HeightRange) + " / " +
                Format(rock.NormalVariance));
            builder.AppendLine(
                "    source mean variation/exposure/crevice/edge-wear: " +
                Format(rock.MeanVariation) + " / " +
                Format(rock.MeanExposure) + " / " +
                Format(rock.MeanCrevice) + " / " +
                Format(rock.MeanEdgeWear));
            builder.AppendLine(
                "    mesh path: " +
                (rock.UsedFallbackMesh
                    ? "ordinary Generated Mass fallback"
                    : "unified edge-wear preview geometry"));
            if (rock.UsedFallbackMesh)
            {
                builder.AppendLine(
                    "    fallback reason: " + rock.FallbackReason);
            }

            builder.AppendLine(
                "    fingerprint: " + rock.Fingerprint);
        }

        private static void WriteEvidence(
            GeneratedMassRiverRockProjectionBaker.ProjectionResult result)
        {
            int resolution =
                GeneratedMassRiverRockProjectionBaker.Resolution;
            WritePng(OutputDirectory + "/RockLibrary_RawGeometry.png", result.Raw, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_Neutral.png", result.Neutral, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_Moderate.png", result.Processed, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_Strong.png", result.Strong, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_RawHeight.png", result.Height, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_ProcessedHeight.png", result.ProcessedHeight, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_RawNormals.png", result.Normals, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_ProcessedNormals.png", result.ProcessedNormals, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_Mask.png", result.Mask, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_RawVariation.png", result.Variation, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_MaterialVariation.png", result.ProcessedVariation, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_RawExposure.png", result.Exposure, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_ProcessedExposure.png", result.ProcessedExposure, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_RawCrevice.png", result.Crevice, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_RootDarkening.png", result.ProcessedCrevice, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_RawEdgeWear.png", result.EdgeWear, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_ProcessedEdgeWear.png", result.ProcessedEdgeWear, resolution, resolution);
            WritePng(OutputDirectory + "/RockLibrary_BurialComparison.png", result.BurialComparison, resolution, resolution);
        }

        private static void DeleteLegacyEvidence()
        {
            for (int index = 0; index < LegacyEvidenceFiles.Length; index++)
            {
                string path = Path.Combine(
                    OutputDirectory,
                    LegacyEvidenceFiles[index]);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private static void WritePng(
            string path,
            Color32[] pixels,
            int width,
            int height)
        {
            Texture2D texture = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false,
                true)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static FrozenContract Contract(
            string stableId,
            MassArchetype archetype,
            int shapeSeed,
            int surfaceSeed,
            float burialFraction,
            float rotationDegrees)
        {
            return new FrozenContract
            {
                StableId = stableId,
                Archetype = archetype,
                ShapeSeed = shapeSeed,
                SurfaceSeed = surfaceSeed,
                BurialFraction = burialFraction,
                RotationDegrees = rotationDegrees
            };
        }

        private static bool Approximately(float first, float second)
        {
            return Mathf.Abs(first - second) <= 0.0001f;
        }

        private static string FormatRock(
            GeneratedMassRiverRockProjectionBaker.RockEvidence rock)
        {
            return (rock.StableId ?? "<missing-id>") + " " +
                rock.Archetype + " shape " + rock.ShapeSeed;
        }

        private static string Format(float value)
        {
            return value.ToString(
                "0.0000",
                CultureInfo.InvariantCulture);
        }

        private static string FormatPercent(float value)
        {
            return value.ToString(
                "P2",
                CultureInfo.InvariantCulture);
        }
    }
}
