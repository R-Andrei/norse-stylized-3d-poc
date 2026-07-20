using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    internal static class SparseRiverbedCandidateSynthesisValidation
    {
        private const string OutputDirectory =
            "Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates";
        private const string ReportPath =
            OutputDirectory + "/SparseRiverbedCandidateSynthesis.txt";
        private const int RepeatCount = 3;
        private const int MipTileSize = 176;
        private const int MipTilePadding = 8;

        [MenuItem("Tools/PS3D/Run Sparse Riverbed Candidate Synthesis")]
        private static void RunMenuAction()
        {
            Directory.CreateDirectory(OutputDirectory);
            SparseRiverbedCandidateSynthesizer.SynthesisResult first =
                SparseRiverbedCandidateSynthesizer.SynthesizeAll();
            SparseRiverbedCandidateSynthesizer.SynthesisResult second =
                SparseRiverbedCandidateSynthesizer.SynthesizeAll();
            List<string> failures = new List<string>();

            if (!first.Succeeded)
            {
                failures.Add("First synthesis failed: " + first.Failure);
            }

            if (!second.Succeeded)
            {
                failures.Add("Repeated synthesis failed: " + second.Failure);
            }

            bool deterministic = first.Succeeded && second.Succeeded &&
                string.Equals(
                    first.CombinedFingerprint,
                    second.CombinedFingerprint,
                    StringComparison.Ordinal) &&
                string.Equals(
                    first.MotifCatalogFingerprint,
                    second.MotifCatalogFingerprint,
                    StringComparison.Ordinal);
            if (!deterministic)
            {
                failures.Add(
                    "Repeated synthesis produced a different motif catalog or combined fingerprint.");
            }

            if (first.ExtractedDonorPlacementCount != 0 ||
                second.ExtractedDonorPlacementCount != 0)
            {
                failures.Add(
                    "One or more donor placements were used; M2.7C.4 requires zero donor contribution.");
            }

            if (first.Succeeded)
            {
                ValidateMotifCatalog(first, second, failures);
                ValidateCandidateSet(first, second, failures);
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
                    $"[GSU-M2.7C.4] Natural-rock sparse riverbed candidate evidence passed. Report written to {ReportPath} and copied to the clipboard. Visual candidate acceptance remains pending.");
            }
            else
            {
                Debug.LogError(
                    $"[GSU-M2.7C.4] Natural-rock sparse riverbed synthesis failed {failures.Count} check(s). Report written to {ReportPath} and copied to the clipboard.");
            }
        }

        private static void ValidateMotifCatalog(
            SparseRiverbedCandidateSynthesizer.SynthesisResult first,
            SparseRiverbedCandidateSynthesizer.SynthesisResult second,
            ICollection<string> failures)
        {
            if (first.Motifs.Count !=
                SparseRiverbedCandidateSynthesizer.ProceduralMotifCount)
            {
                failures.Add(
                    $"Expected {SparseRiverbedCandidateSynthesizer.ProceduralMotifCount} procedural motifs; received {first.Motifs.Count}.");
            }

            if (first.Motifs.Count != second.Motifs.Count)
            {
                failures.Add(
                    "Repeated synthesis produced a different motif count.");
            }

            if (!string.Equals(
                    first.MotifCatalogFingerprint,
                    second.MotifCatalogFingerprint,
                    StringComparison.Ordinal))
            {
                failures.Add(
                    "Repeated synthesis produced a different motif catalog fingerprint.");
            }

            if (first.MinimumExponent < 2.0f)
            {
                failures.Add(
                    $"Minimum superellipse exponent {Format(first.MinimumExponent)} is below 2.0000.");
            }

            if (first.MaximumAspect > 1.85f)
            {
                failures.Add(
                    $"Maximum motif aspect {Format(first.MaximumAspect)} exceeds 1.8500.");
            }

            if (first.MinimumRadialScale < 0.78f)
            {
                failures.Add(
                    $"Minimum motif radial scale {Format(first.MinimumRadialScale)} is below 0.7800.");
            }

            if (first.MaximumBoundaryPerturbation > 0.22f)
            {
                failures.Add(
                    $"Maximum boundary perturbation {Format(first.MaximumBoundaryPerturbation)} exceeds 0.2200.");
            }

            if (first.MinimumFeatureResidualRms <
                SparseRiverbedCandidateSynthesizer.FeatureResidualRmsLimit)
            {
                failures.Add(
                    $"Minimum motif feature residual RMS {Format(first.MinimumFeatureResidualRms)} is below {Format(SparseRiverbedCandidateSynthesizer.FeatureResidualRmsLimit)}.");
            }

            if (first.MinimumHighCurvatureFraction <
                SparseRiverbedCandidateSynthesizer.HighCurvatureFractionLimit)
            {
                failures.Add(
                    $"Minimum motif high-curvature fraction {Format(first.MinimumHighCurvatureFraction)} is below {Format(SparseRiverbedCandidateSynthesizer.HighCurvatureFractionLimit)}.");
            }

            RequireAllPositive(
                first.MotifFamilyCounts,
                "silhouette family",
                failures);
            RequireAllPositive(
                first.CrownProfileCounts,
                "crown profile",
                failures);
            RequireAllPositive(
                first.EdgeProfileCounts,
                "edge profile",
                failures);
            RequireAllPositive(
                first.BurialProfileCounts,
                "burial profile",
                failures);
            RequireAllPositive(
                first.FeatureTypeCounts,
                "local feature type",
                failures);

            if (first.ModifierCountCounts.Length < 4 ||
                first.ModifierCountCounts[0] != 0 ||
                first.ModifierCountCounts[1] <= 0 ||
                first.ModifierCountCounts[2] <= 0 ||
                first.ModifierCountCounts[3] <= 0)
            {
                failures.Add(
                    "Modifier-count distribution must contain one-, two-, and three-modifier motifs and no zero-modifier motif.");
            }
        }

        private static void ValidateCandidateSet(
            SparseRiverbedCandidateSynthesizer.SynthesisResult first,
            SparseRiverbedCandidateSynthesizer.SynthesisResult second,
            ICollection<string> failures)
        {
            if (first.Candidates.Count != 3)
            {
                failures.Add(
                    $"Expected three candidates; received {first.Candidates.Count}.");
            }

            if (first.Candidates.Count != second.Candidates.Count)
            {
                failures.Add(
                    "Repeated synthesis returned a different candidate count.");
                return;
            }

            for (int index = 0; index < first.Candidates.Count; index++)
            {
                SparseRiverbedCandidateSynthesizer.CandidateResult candidate =
                    first.Candidates[index];
                SparseRiverbedCandidateSynthesizer.CandidateResult repeated =
                    second.Candidates[index];
                if (!candidate.Succeeded)
                {
                    failures.Add(
                        candidate.Definition.DisplayName + ": " +
                        candidate.Failure);
                    continue;
                }

                if (candidate.ActualCoverage <
                        candidate.Definition.MinimumCoverage ||
                    candidate.ActualCoverage >
                        candidate.Definition.MaximumCoverage)
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        $": actual coverage {FormatPercent(candidate.ActualCoverage)} is outside {FormatPercent(candidate.Definition.MinimumCoverage)}–{FormatPercent(candidate.Definition.MaximumCoverage)}.");
                }

                if (candidate.QuietBlockFraction <
                    candidate.Definition.MinimumQuietBlockFraction)
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        $": quiet {SparseRiverbedCandidateSynthesizer.QuietMacroBlockSize}×{SparseRiverbedCandidateSynthesizer.QuietMacroBlockSize} block fraction {FormatPercent(candidate.QuietBlockFraction)} is below {FormatPercent(candidate.Definition.MinimumQuietBlockFraction)}.");
                }

                if (candidate.OccupiedMacroBlocks >
                    candidate.Definition.MaximumOccupiedMacroBlocks)
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        $": occupied macro blocks {candidate.OccupiedMacroBlocks} exceed budget {candidate.Definition.MaximumOccupiedMacroBlocks}.");
                }

                int measuredOccupiedBlocks = Mathf.RoundToInt(
                    (1f - candidate.QuietBlockFraction) *
                    SparseRiverbedCandidateSynthesizer.QuietMacroBlockCount);
                if (measuredOccupiedBlocks != candidate.OccupiedMacroBlocks)
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        $": committed macro-block count {candidate.OccupiedMacroBlocks} differs from final measured count {measuredOccupiedBlocks}.");
                }

                if (!string.Equals(
                        candidate.Fingerprint,
                        repeated.Fingerprint,
                        StringComparison.Ordinal))
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        ": repeated fingerprint differs.");
                }

                if (candidate.Placements.Count != repeated.Placements.Count)
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        ": repeated placement count differs.");
                }

                if (!SparseRiverbedCandidateSynthesizer.SeamMetricsPass(
                        candidate.Seams))
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        ": seam metrics exceed limits. " +
                        FormatSeams(candidate.Seams));
                }

                if (candidate.FinalPlacedFeatureResidualRms <
                    SparseRiverbedCandidateSynthesizer.FinalPlacedFeatureResidualRmsLimit)
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        $": final placed-stone feature residual RMS {Format(candidate.FinalPlacedFeatureResidualRms)} is below {Format(SparseRiverbedCandidateSynthesizer.FinalPlacedFeatureResidualRmsLimit)}.");
                }

                if (candidate.FinalPlacedHighCurvatureFraction <
                    SparseRiverbedCandidateSynthesizer.FinalPlacedHighCurvatureFractionLimit)
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        $": final placed-stone high-curvature fraction {FormatPercent(candidate.FinalPlacedHighCurvatureFraction)} is below {FormatPercent(SparseRiverbedCandidateSynthesizer.FinalPlacedHighCurvatureFractionLimit)}.");
                }

                int requiredFamilies = index == 0 ? 2 : 3;
                if (CountPositive(candidate.FamilyPlacements) <
                    requiredFamilies)
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        $": insufficient silhouette-family participation; expected at least {requiredFamilies}.");
                }

                if (CountPositive(candidate.CrownPlacements) < 3 ||
                    CountPositive(candidate.EdgePlacements) < 3 ||
                    CountPositive(candidate.BurialPlacements) < 2 ||
                    CountPositive(candidate.FeaturePlacements) < 4)
                {
                    failures.Add(
                        candidate.Definition.DisplayName +
                        ": insufficient profile or local-feature participation.");
                }
            }
        }

        private static void RequireAllPositive(
            int[] values,
            string label,
            ICollection<string> failures)
        {
            for (int index = 0; index < values.Length; index++)
            {
                if (values[index] <= 0)
                {
                    failures.Add(
                        $"Procedural motif catalog has zero participation for {label} index {index}.");
                }
            }
        }

        private static string BuildReport(
            SparseRiverbedCandidateSynthesizer.SynthesisResult first,
            SparseRiverbedCandidateSynthesizer.SynthesisResult second,
            bool deterministic,
            IReadOnlyList<string> failures)
        {
            StringBuilder builder = new StringBuilder(32768);
            builder.AppendLine(
                "NATURAL-ROCK SPARSE RIVERBED SYNTHESIS — GSU-M2.7C.4");
            builder.AppendLine($"Generated UTC: {DateTime.UtcNow:O}");
            builder.AppendLine($"Unity: {Application.unityVersion}");
            builder.AppendLine(
                $"Algorithm version: {SparseRiverbedCandidateSynthesizer.AlgorithmVersion}");
            builder.AppendLine(
                $"Synthesis resolution: {SparseRiverbedCandidateSynthesizer.Resolution} x {SparseRiverbedCandidateSynthesizer.Resolution}");
            builder.AppendLine(
                "Runtime integration: None — all outputs remain local under Library.");
            builder.AppendLine();

            builder.AppendLine("STONE SOURCE POLICY");
            builder.AppendLine(
                "Source: deterministic natural-rock rounded motifs only.");
            builder.AppendLine(
                "Extracted donor placements: " +
                first.ExtractedDonorPlacementCount);
            builder.AppendLine("Purchased donor pixels sampled: 0");
            builder.AppendLine();

            builder.AppendLine("DETERMINISM");
            builder.AppendLine(
                "First motif catalog fingerprint: " +
                (first.MotifCatalogFingerprint ?? "FAIL"));
            builder.AppendLine(
                "Second motif catalog fingerprint: " +
                (second.MotifCatalogFingerprint ?? "FAIL"));
            builder.AppendLine(
                "First combined fingerprint: " +
                (first.CombinedFingerprint ?? "FAIL"));
            builder.AppendLine(
                "Second combined fingerprint: " +
                (second.CombinedFingerprint ?? "FAIL"));
            builder.AppendLine(
                "Repeated synthesis identical: " +
                (deterministic ? "Yes" : "No"));
            builder.AppendLine();

            if (first.Succeeded)
            {
                builder.AppendLine("PROCEDURAL MOTIF CATALOG");
                builder.AppendLine(
                    "Motif count: " + first.Motifs.Count);
                builder.AppendLine(
                    "Silhouette family counts rounded/oval/slab/soft-angular/chip: " +
                    JoinIntegers(first.MotifFamilyCounts));
                builder.AppendLine(
                    "Crown profile counts dome/flat/offset/twin/one-side/slab: " +
                    JoinIntegers(first.CrownProfileCounts));
                builder.AppendLine(
                    "Edge profile counts soft/mixed/buried/shoulder/chip/flat: " +
                    JoinIntegers(first.EdgeProfileCounts));
                builder.AppendLine(
                    "Burial profile counts light/half/one-side/slab/sink: " +
                    JoinIntegers(first.BurialProfileCounts));
                builder.AppendLine(
                    "Feature type counts facet/ridge/crease/depression/lobe/notch/buried-cut: " +
                    JoinIntegers(first.FeatureTypeCounts));
                builder.AppendLine(
                    "Modifier count distribution 0/1/2/3: " +
                    JoinIntegers(first.ModifierCountCounts));
                builder.AppendLine(
                    "Minimum superellipse exponent: " +
                    Format(first.MinimumExponent));
                builder.AppendLine(
                    "Maximum aspect: " +
                    Format(first.MaximumAspect));
                builder.AppendLine(
                    "Minimum radial scale: " +
                    Format(first.MinimumRadialScale));
                builder.AppendLine(
                    "Maximum boundary perturbation: " +
                    Format(first.MaximumBoundaryPerturbation));
                builder.AppendLine(
                    "Feature residual RMS min/max: " +
                    Format(first.MinimumFeatureResidualRms) + " / " +
                    Format(first.MaximumFeatureResidualRms));
                builder.AppendLine(
                    "High-curvature fraction min/max: " +
                    FormatPercent(first.MinimumHighCurvatureFraction) +
                    " / " +
                    FormatPercent(first.MaximumHighCurvatureFraction));
                builder.AppendLine(
                    "Catalog fingerprint: " +
                    first.MotifCatalogFingerprint);
                builder.AppendLine();

                for (int index = 0; index < first.Candidates.Count; index++)
                {
                    AppendCandidateReport(
                        builder,
                        first.Candidates[index]);
                }
            }
            else
            {
                builder.AppendLine("FIRST RUN FAILURE");
                builder.AppendLine(first.Failure ?? "Unknown failure.");
                builder.AppendLine();
            }

            builder.AppendLine("SEAM LIMITS");
            builder.AppendLine(
                "Boundary mean <= " +
                Format(SparseRiverbedCandidateSynthesizer.SeamMeanLimit));
            builder.AppendLine(
                "Boundary p95 <= " +
                Format(SparseRiverbedCandidateSynthesizer.SeamP95Limit));
            builder.AppendLine(
                "Local excess mean <= " +
                Format(
                    SparseRiverbedCandidateSynthesizer
                    .SeamLocalExcessMeanLimit));
            builder.AppendLine();

            builder.AppendLine("OUTPUTS");
            builder.AppendLine("Report: " + ReportPath);
            builder.AppendLine(
                "Per candidate: ColorPreview, ColorPreview_3x3, StoneMask, Height, Cavity, Normals, Roughness, FinalStructureDebug, MipContactSheet, PlacementDebug, MotifCatalog, MotifNormalCatalog.");
            builder.AppendLine(
                "FinalStructureDebug channels: R = normalized local curvature, G = normalized local residual, B = normalized placed-stone height.");
            builder.AppendLine("All outputs are local under Library.");
            builder.AppendLine();

            builder.AppendLine("SUMMARY");
            if (failures.Count == 0)
            {
                builder.AppendLine(
                    "VERDICT: PASS — deterministic natural-rock sparse candidate synthesis, enforced quiet composition, final-field structure validation, and evidence generation passed. Visual candidate acceptance is still required before runtime integration.");
            }
            else
            {
                builder.AppendLine(
                    $"VERDICT: FAIL — {failures.Count} issue(s) detected.");
                for (int index = 0; index < failures.Count; index++)
                {
                    builder.AppendLine("- " + failures[index]);
                }
            }

            builder.AppendLine();
            builder.AppendLine(
                "PENDING GATE: inspect every candidate's color preview, 3x3 repeat, motif height catalog, motif normal catalog, mask, height/cavity, normals, final structure debug, placement debug, and mip contact sheet. Runtime integration remains blocked until one candidate is visually accepted.");
            return builder.ToString();
        }

        private static void AppendCandidateReport(
            StringBuilder builder,
            SparseRiverbedCandidateSynthesizer.CandidateResult candidate)
        {
            builder.AppendLine(
                "CANDIDATE — " + candidate.Definition.DisplayName);
            builder.AppendLine(
                "Stable evidence id: " + candidate.Definition.StableId);
            if (!candidate.Succeeded)
            {
                builder.AppendLine("STATUS: FAIL — " + candidate.Failure);
                builder.AppendLine();
                return;
            }

            builder.AppendLine("Seed: " + candidate.Definition.Seed);
            builder.AppendLine(
                "Target stone coverage: " +
                FormatPercent(candidate.Definition.TargetCoverage));
            builder.AppendLine(
                "Accepted coverage range: " +
                FormatPercent(candidate.Definition.MinimumCoverage) + "–" +
                FormatPercent(candidate.Definition.MaximumCoverage));
            builder.AppendLine(
                "Actual stone coverage: " +
                FormatPercent(candidate.ActualCoverage));
            builder.AppendLine(
                $"Minimum quiet {SparseRiverbedCandidateSynthesizer.QuietMacroBlockSize}x{SparseRiverbedCandidateSynthesizer.QuietMacroBlockSize} blocks: " +
                FormatPercent(candidate.Definition.MinimumQuietBlockFraction));
            builder.AppendLine(
                $"Actual quiet {SparseRiverbedCandidateSynthesizer.QuietMacroBlockSize}x{SparseRiverbedCandidateSynthesizer.QuietMacroBlockSize} blocks: " +
                FormatPercent(candidate.QuietBlockFraction));
            builder.AppendLine(
                "Occupied macro blocks / budget: " +
                candidate.OccupiedMacroBlocks + " / " +
                candidate.Definition.MaximumOccupiedMacroBlocks);
            builder.AppendLine(
                "Placements: " + candidate.Placements.Count);
            builder.AppendLine(
                "Placement size buckets small/medium/large: " +
                JoinIntegers(candidate.SizeBucketPlacements));
            builder.AppendLine(
                "Placement silhouette families rounded/oval/slab/soft-angular/chip: " +
                JoinIntegers(candidate.FamilyPlacements));
            builder.AppendLine(
                "Placement crowns dome/flat/offset/twin/one-side/slab: " +
                JoinIntegers(candidate.CrownPlacements));
            builder.AppendLine(
                "Placement edges soft/mixed/buried/shoulder/chip/flat: " +
                JoinIntegers(candidate.EdgePlacements));
            builder.AppendLine(
                "Placement burials light/half/one-side/slab/sink: " +
                JoinIntegers(candidate.BurialPlacements));
            builder.AppendLine(
                "Placement local features facet/ridge/crease/depression/lobe/notch/buried-cut: " +
                JoinIntegers(candidate.FeaturePlacements));
            builder.AppendLine(
                "Average modifier count: " +
                Format(candidate.AverageModifierCount));
            builder.AppendLine(
                "Selected source-motif residual RMS average: " +
                Format(candidate.AverageFeatureResidualRms));
            builder.AppendLine(
                "Selected source-motif high-curvature average: " +
                FormatPercent(candidate.AverageHighCurvatureFraction));
            builder.AppendLine(
                "Final placed-stone feature residual RMS: " +
                Format(candidate.FinalPlacedFeatureResidualRms));
            builder.AppendLine(
                "Final placed-stone high-curvature fraction: " +
                FormatPercent(candidate.FinalPlacedHighCurvatureFraction));
            builder.AppendLine(
                "Largest wrapped connected stone-mask region: " +
                candidate.LargestConnectedStonePixels + " pixels");
            builder.AppendLine(
                "Seams: " + FormatSeams(candidate.Seams));
            builder.AppendLine(
                "Mip occupied fractions 0–4: " +
                JoinPercentages(candidate.MipOccupiedFractions));
            builder.AppendLine(
                $"Proposals / density / spacing / empty / overlap / coverage / quiet-budget rejections: {candidate.ProposalCount} / {candidate.DensityRejected} / {candidate.SpacingRejected} / {candidate.EmptyStampRejected} / {candidate.OverlapRejected} / {candidate.CoverageRejected} / {candidate.QuietBlockRejected}");
            builder.AppendLine(
                "Output fingerprint: " + candidate.Fingerprint);
            builder.AppendLine();
        }

        private static void WriteEvidence(
            SparseRiverbedCandidateSynthesizer.SynthesisResult synthesis)
        {
            for (int index = 0; index < synthesis.Candidates.Count; index++)
            {
                SparseRiverbedCandidateSynthesizer.CandidateResult candidate =
                    synthesis.Candidates[index];
                if (!candidate.Succeeded)
                {
                    continue;
                }

                string prefix = OutputDirectory + "/" +
                    Sanitize(candidate.Definition.DisplayName);
                WritePng(
                    prefix + "_ColorPreview.png",
                    candidate.ColorPreview,
                    SparseRiverbedCandidateSynthesizer.Resolution,
                    SparseRiverbedCandidateSynthesizer.Resolution);
                WritePng(
                    prefix + "_ColorPreview_3x3.png",
                    Tile3x3(
                        candidate.ColorPreview,
                        SparseRiverbedCandidateSynthesizer.Resolution,
                        SparseRiverbedCandidateSynthesizer.Resolution),
                    SparseRiverbedCandidateSynthesizer.Resolution * RepeatCount,
                    SparseRiverbedCandidateSynthesizer.Resolution * RepeatCount);
                WritePng(
                    prefix + "_StoneMask.png",
                    ToGrayscale(candidate.StoneMask),
                    SparseRiverbedCandidateSynthesizer.Resolution,
                    SparseRiverbedCandidateSynthesizer.Resolution);
                WritePng(
                    prefix + "_Height.png",
                    ToNormalizedGrayscale(candidate.Height),
                    SparseRiverbedCandidateSynthesizer.Resolution,
                    SparseRiverbedCandidateSynthesizer.Resolution);
                WritePng(
                    prefix + "_Cavity.png",
                    ToGrayscale(candidate.Cavity),
                    SparseRiverbedCandidateSynthesizer.Resolution,
                    SparseRiverbedCandidateSynthesizer.Resolution);
                WritePng(
                    prefix + "_Normals.png",
                    candidate.Normals,
                    SparseRiverbedCandidateSynthesizer.Resolution,
                    SparseRiverbedCandidateSynthesizer.Resolution);
                WritePng(
                    prefix + "_Roughness.png",
                    ToGrayscale(candidate.Roughness),
                    SparseRiverbedCandidateSynthesizer.Resolution,
                    SparseRiverbedCandidateSynthesizer.Resolution);
                WritePng(
                    prefix + "_FinalStructureDebug.png",
                    candidate.FinalStructureDebug,
                    SparseRiverbedCandidateSynthesizer.Resolution,
                    SparseRiverbedCandidateSynthesizer.Resolution);
                WritePng(
                    prefix + "_PlacementDebug.png",
                    candidate.PlacementDebug,
                    SparseRiverbedCandidateSynthesizer.Resolution,
                    SparseRiverbedCandidateSynthesizer.Resolution);
                WritePng(
                    prefix + "_MotifCatalog.png",
                    candidate.MotifCatalogPreview,
                    SparseRiverbedCandidateSynthesizer.Resolution,
                    SparseRiverbedCandidateSynthesizer.Resolution);
                WritePng(
                    prefix + "_MotifNormalCatalog.png",
                    candidate.MotifNormalCatalogPreview,
                    SparseRiverbedCandidateSynthesizer.Resolution,
                    SparseRiverbedCandidateSynthesizer.Resolution);
                WritePng(
                    prefix + "_MipContactSheet.png",
                    BuildMipContactSheet(candidate.ColorPreview),
                    MipTileSize * 5,
                    MipTileSize);
            }
        }

        private static Color32[] BuildMipContactSheet(Color32[] mipZero)
        {
            int sheetWidth = MipTileSize * 5;
            Color32[] sheet = new Color32[sheetWidth * MipTileSize];
            Fill(sheet, new Color32(16, 16, 16, 255));
            Color32[] current = mipZero;
            int currentWidth = SparseRiverbedCandidateSynthesizer.Resolution;
            int currentHeight = SparseRiverbedCandidateSynthesizer.Resolution;
            for (int mip = 0; mip < 5; mip++)
            {
                DrawScaled(
                    current,
                    currentWidth,
                    currentHeight,
                    sheet,
                    sheetWidth,
                    MipTileSize,
                    mip * MipTileSize + MipTilePadding,
                    MipTilePadding,
                    MipTileSize - MipTilePadding * 2,
                    MipTileSize - MipTilePadding * 2);
                if (mip < 4)
                {
                    current = SparseRiverbedCandidateSynthesizer.BuildWrappedMip(
                        current,
                        currentWidth,
                        currentHeight,
                        out currentWidth,
                        out currentHeight);
                }
            }

            return sheet;
        }

        private static Color32[] Tile3x3(
            Color32[] source,
            int width,
            int height)
        {
            int outputWidth = width * RepeatCount;
            int outputHeight = height * RepeatCount;
            Color32[] output = new Color32[outputWidth * outputHeight];
            for (int y = 0; y < outputHeight; y++)
            {
                int sourceY = y % height;
                for (int x = 0; x < outputWidth; x++)
                {
                    output[y * outputWidth + x] =
                        source[sourceY * width + x % width];
                }
            }

            return output;
        }

        private static Color32[] ToGrayscale(float[] values)
        {
            Color32[] pixels = new Color32[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                byte value = (byte)Mathf.RoundToInt(
                    Mathf.Clamp01(values[index]) * 255f);
                pixels[index] = new Color32(value, value, value, 255);
            }

            return pixels;
        }

        private static Color32[] ToNormalizedGrayscale(float[] values)
        {
            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int index = 0; index < values.Length; index++)
            {
                minimum = Mathf.Min(minimum, values[index]);
                maximum = Mathf.Max(maximum, values[index]);
            }

            float inverse = 1f / Mathf.Max(0.000001f, maximum - minimum);
            Color32[] pixels = new Color32[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                byte value = (byte)Mathf.RoundToInt(
                    Mathf.Clamp01((values[index] - minimum) * inverse) *
                    255f);
                pixels[index] = new Color32(value, value, value, 255);
            }

            return pixels;
        }

        private static void DrawScaled(
            Color32[] source,
            int sourceWidth,
            int sourceHeight,
            Color32[] destination,
            int destinationWidth,
            int destinationHeight,
            int offsetX,
            int offsetY,
            int drawnWidth,
            int drawnHeight)
        {
            for (int y = 0; y < drawnHeight; y++)
            {
                int sourceY = Mathf.Clamp(
                    y * sourceHeight / Mathf.Max(1, drawnHeight),
                    0,
                    sourceHeight - 1);
                for (int x = 0; x < drawnWidth; x++)
                {
                    int sourceX = Mathf.Clamp(
                        x * sourceWidth / Mathf.Max(1, drawnWidth),
                        0,
                        sourceWidth - 1);
                    int destinationX = offsetX + x;
                    int destinationY = offsetY + y;
                    if (destinationX >= 0 &&
                        destinationX < destinationWidth &&
                        destinationY >= 0 &&
                        destinationY < destinationHeight)
                    {
                        destination[destinationY * destinationWidth +
                            destinationX] =
                            source[sourceY * sourceWidth + sourceX];
                    }
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

        private static void Fill(Color32[] pixels, Color32 color)
        {
            for (int index = 0; index < pixels.Length; index++)
            {
                pixels[index] = color;
            }
        }

        private static int CountPositive(int[] values)
        {
            int count = 0;
            for (int index = 0; index < values.Length; index++)
            {
                if (values[index] > 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static string JoinIntegers(int[] values)
        {
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < values.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(" / ");
                }

                builder.Append(values[index]);
            }

            return builder.ToString();
        }

        private static string JoinPercentages(float[] values)
        {
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < values.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(" / ");
                }

                builder.Append(FormatPercent(values[index]));
            }

            return builder.ToString();
        }

        private static string FormatSeams(
            SparseRiverbedCandidateSynthesizer.SeamMetrics seams)
        {
            if (seams == null)
            {
                return "missing";
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "H mean/p95/excess {0:0.0000}/{1:0.0000}/{2:0.0000}, V mean/p95/excess {3:0.0000}/{4:0.0000}/{5:0.0000}",
                seams.HorizontalMean,
                seams.HorizontalP95,
                seams.HorizontalLocalExcessMean,
                seams.VerticalMean,
                seams.VerticalP95,
                seams.VerticalLocalExcessMean);
        }

        private static string Sanitize(string value)
        {
            StringBuilder builder = new StringBuilder(value.Length);
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                builder.Append(char.IsLetterOrDigit(character)
                    ? character
                    : '_');
            }

            return builder.ToString();
        }

        private static string Format(float value)
        {
            return value.ToString("0.0000", CultureInfo.InvariantCulture);
        }

        private static string FormatPercent(float value)
        {
            return (value * 100f).ToString(
                "0.00",
                CultureInfo.InvariantCulture) + "%";
        }
    }
}
