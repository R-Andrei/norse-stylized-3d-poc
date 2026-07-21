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
    internal static class GeneratedMassSparseRiverbedTileAssemblyValidation
    {
        private const string OutputDirectory =
            "Library/SurfaceMaterialDiagnostics/" +
            "GeneratedMassSparseRiverbedAssembly";
        private const string ReportPath = OutputDirectory +
            "/GeneratedMassSparseRiverbedAssemblyReport.txt";

        private sealed class AcceptedSourceContract
        {
            internal string StableId;
            internal MassArchetype Archetype;
            internal int ShapeSeed;
            internal int SurfaceSeed;
            internal float BurialFraction;
            internal float RotationDegrees;
        }

        private static readonly AcceptedSourceContract[] AcceptedSources =
        {
            Source("T-05", MassArchetype.TerrainBoulder, 3187, 4134, 0.226f, 186f),
            Source("T-08", MassArchetype.TerrainBoulder, 1291, 1254, 0.218f, 73f),
            Source("T-09", MassArchetype.TerrainBoulder, 3473, 6660, 0.226f, 145f),
            Source("T-10", MassArchetype.TerrainBoulder, 5237, 9140, 0.234f, 206f),
            Source("T-11", MassArchetype.TerrainBoulder, 8123, 9475, 0.242f, 279f),
            Source("T-12", MassArchetype.TerrainBoulder, 1579, 2222, 0.218f, 201f),
            Source("T-13", MassArchetype.TerrainBoulder, 3821, 8048, 0.226f, 259f),
            Source("T-14", MassArchetype.TerrainBoulder, 6173, 4645, 0.234f, 353f),
            Source("T-15", MassArchetype.TerrainBoulder, 9431, 7584, 0.242f, 68f),
            Source("S-00", MassArchetype.SquatBoulder, 5727, 2238, 0.218f, 246f),
            Source("S-03", MassArchetype.SquatBoulder, 7319, 3776, 0.242f, 106f),
            Source("S-04", MassArchetype.SquatBoulder, 1117, 489, 0.218f, 156f),
            Source("S-08", MassArchetype.SquatBoulder, 1361, 2721, 0.218f, 110f),
            Source("S-09", MassArchetype.SquatBoulder, 3593, 8477, 0.226f, 158f),
            Source("S-10", MassArchetype.SquatBoulder, 5393, 1210, 0.234f, 255f),
            Source("S-12", MassArchetype.SquatBoulder, 1693, 3997, 0.218f, 222f),
            Source("S-13", MassArchetype.SquatBoulder, 4001, 286, 0.226f, 322f),
            Source("S-14", MassArchetype.SquatBoulder, 6311, 6588, 0.234f, 35f)
        };

        [MenuItem(
            "Tools/PS3D/Run Generated Mass Sparse Riverbed Assembly Proof")]
        private static void RunMenuAction()
        {
            Directory.CreateDirectory(OutputDirectory);
            DeletePreviousEvidence();
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();
            ValidateCurrentSourceSnapshot(warnings, failures);

            GeneratedMassSparseRiverbedTileAssembler.SuiteResult first =
                GeneratedMassSparseRiverbedTileAssembler.BuildSuite();
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult second =
                GeneratedMassSparseRiverbedTileAssembler.BuildSuite(false);

            if (!first.Succeeded)
            {
                failures.Add("First assembly suite failed: " + first.Failure);
            }

            if (!second.Succeeded)
            {
                failures.Add(
                    "Repeated assembly suite failed: " + second.Failure);
            }

            bool deterministic = first.Succeeded && second.Succeeded &&
                string.Equals(
                    first.Fingerprint,
                    second.Fingerprint,
                    StringComparison.Ordinal);
            if (!deterministic)
            {
                failures.Add(
                    "Repeated assembly suite produced a different fingerprint.");
            }

            if (first.Succeeded)
            {
                ValidateSuite(first, second, failures);
                WriteEvidence(first);
            }

            string report = BuildReport(
                first,
                second,
                deterministic,
                failures,
                warnings);
            File.WriteAllText(ReportPath, report, Encoding.UTF8);
            EditorGUIUtility.systemCopyBuffer = report;

            if (failures.Count > 0)
            {
                Debug.LogError(
                    "[GSU-M2.7C.5D.1] Generated Mass sparse riverbed " +
                    "assembly proof failed " + failures.Count +
                    " check(s). Report written to " + ReportPath +
                    " and copied to the clipboard.");
            }
            else if (warnings.Count > 0)
            {
                Debug.LogWarning(
                    "[GSU-M2.7C.5D.1] Generated Mass sparse riverbed " +
                    "assembly proof passed with " + warnings.Count +
                    " source-drift warning(s). Report written to " +
                    ReportPath + " and copied to the clipboard.");
            }
            else
            {
                Debug.Log(
                    "[GSU-M2.7C.5D.1] Generated Mass sparse riverbed " +
                    "assembly proof passed mechanical validation. Report " +
                    "written to " + ReportPath +
                    " and copied to the clipboard. Visual candidate " +
                    "selection remains pending.");
            }
        }

        private static void ValidateCurrentSourceSnapshot(
            ICollection<string> warnings,
            ICollection<string> failures)
        {
            try
            {
                IReadOnlyList<GeneratedMassRiverRockProjectionBaker
                    .FrozenSourceDefinition> definitions =
                    GeneratedMassRiverRockProjectionBaker
                        .GetFrozenSourceDefinitions();
                IReadOnlyDictionary<string, string> current =
                    GeneratedMassRiverRockProjectionBaker
                        .BuildCurrentRawFingerprintSnapshot();
                if (definitions.Count !=
                    GeneratedMassSparseRiverbedTileAssembler
                        .ExpectedSourceCount ||
                    AcceptedSources.Length != definitions.Count)
                {
                    failures.Add(
                        "Frozen source-definition count is " +
                        definitions.Count + "; expected " +
                        GeneratedMassSparseRiverbedTileAssembler
                            .ExpectedSourceCount + ".");
                    return;
                }

                for (int index = 0; index < definitions.Count; index++)
                {
                    GeneratedMassRiverRockProjectionBaker
                        .FrozenSourceDefinition definition =
                            definitions[index];
                    AcceptedSourceContract accepted = AcceptedSources[index];
                    if (!string.Equals(
                            definition.StableId,
                            accepted.StableId,
                            StringComparison.Ordinal) ||
                        definition.Archetype != accepted.Archetype ||
                        definition.ShapeSeed != accepted.ShapeSeed ||
                        definition.SurfaceSeed != accepted.SurfaceSeed ||
                        !Approximately(
                            definition.DefaultBurialFraction,
                            accepted.BurialFraction) ||
                        !Approximately(
                            definition.CatalogRotationDegrees,
                            accepted.RotationDegrees))
                    {
                        failures.Add(
                            "Frozen source definition changed at index " +
                            index + ".");
                    }
                }

                for (int index = 0; index < definitions.Count; index++)
                {
                    GeneratedMassRiverRockProjectionBaker
                        .FrozenSourceDefinition definition =
                            definitions[index];
                    string fingerprint;
                    if (!current.TryGetValue(
                            definition.StableId,
                            out fingerprint))
                    {
                        failures.Add(
                            "Current source snapshot is missing " +
                            definition.StableId + ".");
                        continue;
                    }

                    if (!string.Equals(
                            fingerprint,
                            definition.AcceptedRawFingerprint,
                            StringComparison.Ordinal))
                    {
                        warnings.Add(
                            definition.StableId +
                            ": current raw geometry fingerprint differs " +
                            "from the accepted algorithm-8 M2.7C.5D source " +
                            "snapshot.");
                    }
                }
            }
            catch (Exception exception)
            {
                failures.Add(
                    "Frozen source snapshot validation failed: " +
                    exception.Message);
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private static void ValidateSuite(
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult first,
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult second,
            ICollection<string> failures)
        {
            if (first.Candidates.Count !=
                GeneratedMassSparseRiverbedTileAssembler.CandidateCount)
            {
                failures.Add(
                    "Candidate count is " + first.Candidates.Count +
                    "; expected " +
                    GeneratedMassSparseRiverbedTileAssembler.CandidateCount +
                    ".");
            }

            if (second.Candidates.Count != first.Candidates.Count)
            {
                failures.Add(
                    "Repeated suite returned a different candidate count.");
                return;
            }

            for (int index = 0; index < first.Candidates.Count; index++)
            {
                GeneratedMassSparseRiverbedTileAssembler.CandidateResult
                    candidate = first.Candidates[index];
                GeneratedMassSparseRiverbedTileAssembler.CandidateResult
                    repeated = second.Candidates[index];
                ValidateCandidate(candidate, failures);
                if (!string.Equals(
                        candidate.Definition.StableId,
                        repeated.Definition.StableId,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        candidate.Fingerprint,
                        repeated.Fingerprint,
                        StringComparison.Ordinal))
                {
                    failures.Add(
                        candidate.Definition.StableId +
                        ": repeated candidate fingerprint changed.");
                }
            }
        }

        private static void ValidateCandidate(
            GeneratedMassSparseRiverbedTileAssembler.CandidateResult candidate,
            ICollection<string> failures)
        {
            if (!candidate.Succeeded)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": candidate build failed: " + candidate.Failure);
                return;
            }

            if (candidate.Coverage <
                    candidate.Definition.MinimumCoverage ||
                candidate.Coverage >
                    candidate.Definition.MaximumCoverage)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": coverage is " +
                    FormatPercent(candidate.Coverage) +
                    "; accepted range is " +
                    FormatPercent(candidate.Definition.MinimumCoverage) +
                    "–" +
                    FormatPercent(candidate.Definition.MaximumCoverage) +
                    ".");
            }

            if (candidate.QuietBlockFraction <
                candidate.Definition.MinimumQuietFraction)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": quiet 32x32 block fraction is " +
                    FormatPercent(candidate.QuietBlockFraction) +
                    "; minimum is " +
                    FormatPercent(
                        candidate.Definition.MinimumQuietFraction) + ".");
            }

            if (candidate.UniqueSourceCount <
                GeneratedMassSparseRiverbedTileAssembler
                    .MinimumSourceDiversity)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": source diversity is " +
                    candidate.UniqueSourceCount + "; minimum is " +
                    GeneratedMassSparseRiverbedTileAssembler
                        .MinimumSourceDiversity + ".");
            }

            if (candidate.MaximumObservedSourceShare >
                GeneratedMassSparseRiverbedTileAssembler
                    .MaximumSourceShare + 0.0001f)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": maximum source share is " +
                    FormatPercent(
                        candidate.MaximumObservedSourceShare) +
                    "; maximum is " +
                    FormatPercent(
                        GeneratedMassSparseRiverbedTileAssembler
                            .MaximumSourceShare) + ".");
            }

            if (candidate.Placements.Count <
                GeneratedMassSparseRiverbedTileAssembler
                    .MinimumSourceDiversity)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": placement count is too low for the diversity contract.");
            }

            for (int index = 0; index < candidate.Placements.Count; index++)
            {
                GeneratedMassSparseRiverbedTileAssembler.PlacementEvidence
                    placement = candidate.Placements[index];
                if (placement.RootContactPixels <= 0)
                {
                    failures.Add(
                        candidate.Definition.StableId + "/" +
                        placement.Index +
                        ": root contact is empty.");
                }
                else if (placement.RootPerimeterAffectedFraction >= 0.65f)
                {
                    failures.Add(
                        candidate.Definition.StableId + "/" +
                        placement.Index +
                        ": root contact affects " +
                        FormatPercent(
                            placement.RootPerimeterAffectedFraction) +
                        " of the perimeter; expected below 65%.");
                }
            }

            if (candidate.Seams == null || !candidate.Seams.Passed)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": periodic seam metrics exceeded tolerance.");
            }

            ValidatePixels(
                candidate.Definition.StableId + " Moderate",
                candidate.Moderate,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                candidate.Definition.StableId + " PlacementDebug",
                candidate.PlacementDebug,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                candidate.Definition.StableId + " StableIdDebug",
                candidate.StableIdDebug,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                candidate.Definition.StableId + " Mask",
                candidate.Mask,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                candidate.Definition.StableId + " Height",
                candidate.Height,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                candidate.Definition.StableId + " Normals",
                candidate.Normals,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                candidate.Definition.StableId + " Variation",
                candidate.Variation,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                candidate.Definition.StableId + " RootDarkening",
                candidate.RootDarkening,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                candidate.Definition.StableId + " EdgeWear",
                candidate.EdgeWear,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                candidate.Definition.StableId + " MipContactSheet",
                candidate.MipContactSheet,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
        }

        private static void ValidatePixels(
            string label,
            Color32[] pixels,
            int width,
            int height,
            ICollection<string> failures)
        {
            int expected = width * height;
            int actual = pixels != null ? pixels.Length : 0;
            if (actual != expected)
            {
                failures.Add(
                    label + " pixel count is " + actual +
                    "; expected " + expected + ".");
            }
        }

        private static string BuildReport(
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult first,
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult second,
            bool deterministic,
            IReadOnlyCollection<string> failures,
            IReadOnlyCollection<string> warnings)
        {
            StringBuilder builder = new StringBuilder(32768);
            builder.AppendLine(
                "GENERATED MASS SPARSE RIVERBED ASSEMBLY PROOF — " +
                "GSU-M2.7C.5D.1");
            builder.AppendLine(
                "Generated UTC: " +
                DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            builder.AppendLine("Unity: " + Application.unityVersion);
            builder.AppendLine(
                "Assembler algorithm version: " +
                GeneratedMassSparseRiverbedTileAssembler.AlgorithmVersion);
            builder.AppendLine(
                "Final / working resolution: " +
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution +
                " / " +
                GeneratedMassSparseRiverbedTileAssembler.WorkResolution);
            builder.AppendLine(
                "Runtime integration: None — all outputs remain local under " +
                "Library.");
            builder.AppendLine(
                "Source contract: frozen 18-rock algorithm-8 library, unified " +
                "wear 0.52, fallback wear 0.56, Moderate response.");
            builder.AppendLine();

            builder.AppendLine("DETERMINISM");
            builder.AppendLine(
                "First suite fingerprint: " +
                (first.Fingerprint ?? "FAIL"));
            builder.AppendLine(
                "Second suite fingerprint: " +
                (second.Fingerprint ?? "FAIL"));
            builder.AppendLine(
                "Repeated suite identical: " +
                (deterministic ? "Yes" : "No"));
            builder.AppendLine();

            if (first.Succeeded)
            {
                builder.AppendLine("CANDIDATE RESULTS");
                for (int index = 0; index < first.Candidates.Count; index++)
                {
                    AppendCandidate(builder, first.Candidates[index]);
                }
            }
            else
            {
                builder.AppendLine("FIRST SUITE FAILURE");
                builder.AppendLine(first.Failure ?? "Unknown failure.");
                builder.AppendLine();
            }

            builder.AppendLine("OUTPUTS");
            builder.AppendLine("Report: " + ReportPath);
            if (first.Succeeded)
            {
                for (int index = 0; index < first.Candidates.Count; index++)
                {
                    string prefix = first.Candidates[index]
                        .Definition.StableId;
                    builder.AppendLine(prefix + "_Moderate.png");
                    builder.AppendLine(prefix + "_3x3.png");
                    builder.AppendLine(prefix + "_PlacementDebug.png");
                    builder.AppendLine(prefix + "_StableIdDebug.png");
                    builder.AppendLine(prefix + "_Mask.png");
                    builder.AppendLine(prefix + "_Height.png");
                    builder.AppendLine(prefix + "_Normals.png");
                    builder.AppendLine(prefix + "_Variation.png");
                    builder.AppendLine(prefix + "_RootDarkening.png");
                    builder.AppendLine(prefix + "_EdgeWear.png");
                    builder.AppendLine(prefix + "_MipContactSheet.png");
                }
            }

            builder.AppendLine();
            builder.AppendLine("SUMMARY");
            if (failures.Count > 0)
            {
                builder.AppendLine(
                    "VERDICT: FAIL — " + failures.Count +
                    " issue(s) detected.");
                foreach (string failure in failures)
                {
                    builder.AppendLine("- " + failure);
                }
            }
            else if (warnings.Count > 0)
            {
                builder.AppendLine(
                    "VERDICT: PASS WITH SOURCE GEOMETRY DRIFT WARNING — " +
                    warnings.Count + " warning(s) detected.");
                foreach (string warning in warnings)
                {
                    builder.AppendLine("- WARNING: " + warning);
                }
            }
            else
            {
                builder.AppendLine(
                    "VERDICT: PASS — deterministic three-candidate direct-mesh " +
                    "assembly, pre-commit quiet composition, source diversity, " +
                    "toroidal seams, frozen Moderate response, mip evidence and " +
                    "complete output generation passed.");
            }

            builder.AppendLine();
            builder.AppendLine(
                "PENDING GATE: visually select one complete candidate after " +
                "reviewing Moderate, 3x3, PlacementDebug, StableIdDebug, " +
                "channels and mip evidence. Runtime integration remains blocked.");
            return builder.ToString();
        }

        private static void AppendCandidate(
            StringBuilder builder,
            GeneratedMassSparseRiverbedTileAssembler.CandidateResult candidate)
        {
            builder.AppendLine(
                "[" + candidate.Definition.StableId + "] " +
                candidate.Definition.DisplayName);
            builder.AppendLine(
                "    coverage target/actual/range: " +
                FormatPercent(candidate.Definition.TargetCoverage) + " / " +
                FormatPercent(candidate.Coverage) + " / " +
                FormatPercent(candidate.Definition.MinimumCoverage) + "–" +
                FormatPercent(candidate.Definition.MaximumCoverage));
            builder.AppendLine(
                "    quiet blocks: " +
                FormatPercent(candidate.QuietBlockFraction) +
                " (minimum " +
                FormatPercent(
                    candidate.Definition.MinimumQuietFraction) + ", occupied " +
                candidate.OccupiedQuietBlocks + ")");
            builder.AppendLine(
                "    placements / unique sources / maximum source share: " +
                candidate.Placements.Count + " / " +
                candidate.UniqueSourceCount + " / " +
                FormatPercent(candidate.MaximumObservedSourceShare));
            builder.AppendLine(
                "    maximum root perimeter affected: " +
                FormatPercent(
                    candidate.MaximumRootPerimeterAffectedFraction));
            builder.AppendLine(
                "    rejection counts spacing/overlap/quiet/coverage/repeat: " +
                candidate.RejectedForSpacing + " / " +
                candidate.RejectedForOverlap + " / " +
                candidate.RejectedForQuietBudget + " / " +
                candidate.RejectedForCoverage + " / " +
                candidate.RejectedForLocalRepeat);
            if (candidate.Seams != null)
            {
                builder.AppendLine(
                    "    seam means mask/height/normal/variation/root/wear/preview: " +
                    FormatFloat(candidate.Seams.MaskMean) + " / " +
                    FormatFloat(candidate.Seams.HeightMean) + " / " +
                    FormatFloat(candidate.Seams.NormalMean) + " / " +
                    FormatFloat(candidate.Seams.VariationMean) + " / " +
                    FormatFloat(candidate.Seams.RootMean) + " / " +
                    FormatFloat(candidate.Seams.WearMean) + " / " +
                    FormatFloat(candidate.Seams.PreviewMean));
            }

            builder.AppendLine("    source usage:");
            for (int index = 0; index < candidate.SourceUsage.Count; index++)
            {
                GeneratedMassSparseRiverbedTileAssembler.SourceUsage usage =
                    candidate.SourceUsage[index];
                if (usage.Count <= 0)
                {
                    continue;
                }

                builder.AppendLine(
                    "      " + usage.StableId + " = " + usage.Count +
                    (usage.UsedFallbackMesh ? " (fallback)" : " (unified)"));
            }

            builder.AppendLine(
                "    fingerprint: " + candidate.Fingerprint);
            builder.AppendLine();
        }

        private static void WriteEvidence(
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult suite)
        {
            for (int index = 0; index < suite.Candidates.Count; index++)
            {
                GeneratedMassSparseRiverbedTileAssembler.CandidateResult
                    candidate = suite.Candidates[index];
                string prefix = Path.Combine(
                    OutputDirectory,
                    candidate.Definition.StableId);
                WritePng(
                    prefix + "_Moderate.png",
                    candidate.Moderate,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                Color32[] threeByThree =
                    GeneratedMassSparseRiverbedTileAssembler
                        .BuildThreeByThreeEvidence(candidate.Moderate);
                WritePng(
                    prefix + "_3x3.png",
                    threeByThree,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution * 3,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution * 3);
                WritePng(
                    prefix + "_PlacementDebug.png",
                    candidate.PlacementDebug,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_StableIdDebug.png",
                    candidate.StableIdDebug,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_Mask.png",
                    candidate.Mask,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_Height.png",
                    candidate.Height,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_Normals.png",
                    candidate.Normals,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_Variation.png",
                    candidate.Variation,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_RootDarkening.png",
                    candidate.RootDarkening,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_EdgeWear.png",
                    candidate.EdgeWear,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_MipContactSheet.png",
                    candidate.MipContactSheet,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
            }
        }

        private static void DeletePreviousEvidence()
        {
            if (!Directory.Exists(OutputDirectory))
            {
                return;
            }

            string[] files = Directory.GetFiles(OutputDirectory);
            for (int index = 0; index < files.Length; index++)
            {
                File.Delete(files[index]);
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

        private static AcceptedSourceContract Source(
            string stableId,
            MassArchetype archetype,
            int shapeSeed,
            int surfaceSeed,
            float burialFraction,
            float rotationDegrees)
        {
            return new AcceptedSourceContract
            {
                StableId = stableId,
                Archetype = archetype,
                ShapeSeed = shapeSeed,
                SurfaceSeed = surfaceSeed,
                BurialFraction = burialFraction,
                RotationDegrees = rotationDegrees
            };
        }

        private static bool Approximately(float a, float b)
        {
            return Mathf.Abs(a - b) <= 0.00001f;
        }

        private static string FormatPercent(float value)
        {
            return value.ToString("P2", CultureInfo.InvariantCulture);
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("F5", CultureInfo.InvariantCulture);
        }
    }
}
