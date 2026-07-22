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
        private const float MinimumSubstrateMeanLuminance = 0.49f;
        private const float MaximumSubstrateMeanLuminance = 0.53f;
        private const float MinimumSubstrateFifthPercentile = 0.485f;
        private const float MaximumSubstrateFifthPercentile = 0.505f;
        private const float MinimumSubstrateNinetyFifthPercentile = 0.500f;
        private const float MaximumSubstrateNinetyFifthPercentile = 0.520f;
        private const float MinimumSubstratePercentileSpread = 0.008f;
        private const float MaximumSubstratePercentileSpread = 0.022f;
        private const float MinimumSubstrateRmsContrast = 0.0025f;
        private const float MaximumSubstrateRmsContrast = 0.0075f;
        private const float MaximumSubstrateEdgeDifference = 0.005f;
        private const float MaximumSubstrateBlockDeviation64 = 0.012f;
        private const float MaximumSubstrateBlockDeviation128 = 0.008f;
        private const float MaximumSubstrateBlockDeviation256 = 0.005f;
        private const float MaximumSubstrateBlockRmsDeviation64 = 0.0045f;
        private const float MaximumSubstrateBlockRmsDeviation128 = 0.0030f;
        private const float MaximumSubstrateBlockRmsDeviation256 = 0.0020f;
        private const float MinimumPaletteFormRange = 0.25f;
        private const float MinimumPaletteFormSubstrateMean = 0.56f;
        private const float MaximumPaletteFormSubstrateMean = 0.69f;
        private const float MinimumPaletteFormRockMean = 0.16f;
        private const float MaximumPaletteFormRockMean = 0.45f;
        private const float MinimumPaletteFormBandSeparation = 0.15f;
        private const float MaximumPackedSubstrateSlopeDeviation = 0.012f;
        private const float MaximumPackedSubstrateCavityMean = 0.001f;
        private const float MinimumPackedRockSlopeMagnitude = 0.05f;
        private const float MinimumPackedRockCavityMean = 0.003f;
        private const float MinimumPackedRockCavityMaximum = 0.20f;
        private const float MinimumNeutralToContrastDifference = 0.008f;
        private const float MinimumNeutralToAlternateDifference = 0.025f;
        private const float MinimumFractionalSilhouetteCoverageFraction =
            0.00005f;
        private const float MaximumAdjacentPaletteFormDifference = 0.45f;
        private const float MinimumFeatureMaskMaximum = 0.90f;
        private const float MinimumFeatureMaskMean = 0.001f;
        private const float MaximumFeatureMaskMean = 0.025f;
        private const float MinimumSubstrateOnlyFormMean = 0.54f;
        private const float MaximumSubstrateOnlyFormMean = 0.70f;
        private const float MinimumSubstrateOnlyRoughnessMean = 0.55f;
        private const float MaximumSubstrateOnlyRoughnessMean = 0.80f;

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

        private static readonly string[] ExpectedCandidateIds =
        {
            "Ultra_Sparse_Riverbed",
            "Very_Sparse_Riverbed",
            "Sparse_Riverbed"
        };

        private static readonly int[] ExpectedPlacementCounts =
        {
            6, 9, 12
        };

        private static readonly float[] ExpectedMinimumCoverage =
        {
            0.0025f, 0.0045f, 0.0065f
        };

        private static readonly float[] ExpectedMaximumCoverage =
        {
            0.0100f, 0.0140f, 0.0180f
        };

        private static readonly int[] ExpectedMaximumBroadCenterCount =
        {
            3, 4, 5
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

            GeneratedMassSparseRiverbedTileAssembler.SuiteResult comparison =
                GeneratedMassSparseRiverbedTileAssembler.BuildSuite(false);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult retained =
                GeneratedMassSparseRiverbedTileAssembler.BuildSuite(true);

            if (!comparison.Succeeded)
            {
                failures.Add(
                    "Non-retained comparison suite failed: " +
                    comparison.Failure);
            }

            if (!retained.Succeeded)
            {
                failures.Add(
                    "Retained evidence suite failed: " + retained.Failure);
            }

            bool deterministic = retained.Succeeded &&
                comparison.Succeeded &&
                string.Equals(
                    retained.Fingerprint,
                    comparison.Fingerprint,
                    StringComparison.Ordinal);
            if (!deterministic)
            {
                failures.Add(
                    "Repeated assembly suite produced a different fingerprint.");
            }

            if (retained.Succeeded)
            {
                ValidateSuite(retained, comparison, failures);
                WriteEvidence(retained);
            }

            string report = BuildReport(
                retained,
                comparison,
                deterministic,
                failures,
                warnings);
            File.WriteAllText(ReportPath, report, Encoding.UTF8);
            EditorGUIUtility.systemCopyBuffer = report;

            if (failures.Count > 0)
            {
                Debug.LogError(
                    "[GSU-M2.7C.5E.2.2] Generated Mass feature-payload " +
                    "ultra-sparse assembly proof failed " + failures.Count +
                    " check(s). Report written to " + ReportPath +
                    " and copied to the clipboard.");
            }
            else if (warnings.Count > 0)
            {
                Debug.LogWarning(
                    "[GSU-M2.7C.5E.2.2] Generated Mass feature-payload " +
                    "ultra-sparse assembly proof passed with " +
                    warnings.Count + " source-drift warning(s). Report " +
                    "written to " + ReportPath +
                    " and copied to the clipboard.");
            }
            else
            {
                Debug.Log(
                    "[GSU-M2.7C.5E.2.2] Generated Mass feature-payload " +
                    "ultra-sparse assembly proof passed mechanical " +
                    "validation. Report written to " + ReportPath +
                    " and copied to the clipboard. Visual substrate and " +
                    "candidate selection remain pending.");
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
                            "from the accepted algorithm-8 M2.7C.5D " +
                            "source snapshot.");
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
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult retained,
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult comparison,
            ICollection<string> failures)
        {
            if (retained.Candidates.Count !=
                GeneratedMassSparseRiverbedTileAssembler.CandidateCount)
            {
                failures.Add(
                    "Candidate count is " + retained.Candidates.Count +
                    "; expected " +
                    GeneratedMassSparseRiverbedTileAssembler.CandidateCount +
                    ".");
            }

            ValidateSubstrate(retained.Substrate, failures);
            if (comparison.Substrate == null ||
                retained.Substrate == null ||
                !string.Equals(
                    retained.Substrate.Fingerprint,
                    comparison.Substrate.Fingerprint,
                    StringComparison.Ordinal))
            {
                failures.Add(
                    "Repeated suite produced a different shared substrate " +
                    "fingerprint.");
            }

            if (comparison.Candidates.Count != retained.Candidates.Count)
            {
                failures.Add(
                    "Repeated suite returned a different candidate count.");
                return;
            }

            for (int index = 0;
                 index < retained.Candidates.Count;
                 index++)
            {
                GeneratedMassSparseRiverbedTileAssembler.CandidateResult
                    candidate = retained.Candidates[index];
                GeneratedMassSparseRiverbedTileAssembler.CandidateResult
                    repeated = comparison.Candidates[index];
                ValidateCandidateDefinition(
                    candidate.Definition,
                    index,
                    failures);
                ValidateCandidate(
                    candidate,
                    retained.Substrate,
                    failures);
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

                if (!string.Equals(
                        candidate.PalettePayloadFingerprint,
                        repeated.PalettePayloadFingerprint,
                        StringComparison.Ordinal))
                {
                    failures.Add(
                        candidate.Definition.StableId +
                        ": repeated palette-payload fingerprint changed.");
                }
            }

            ValidateNestedPlacementSequences(retained, failures);
        }

        private static void ValidateSubstrate(
            GeneratedMassSparseRiverbedTileAssembler.SubstrateResult substrate,
            ICollection<string> failures)
        {
            if (substrate == null)
            {
                failures.Add("Shared substrate result is missing.");
                return;
            }

            ValidatePixels(
                "SubstrateOnly",
                substrate.Color,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            int expectedVariation =
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution *
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution;
            if (substrate.Variation == null ||
                substrate.Variation.Length != expectedVariation)
            {
                failures.Add(
                    "Shared substrate variation count is " +
                    (substrate.Variation != null
                        ? substrate.Variation.Length
                        : 0) + "; expected " + expectedVariation + ".");
            }

            float spread = substrate.NinetyFifthPercentileLuminance -
                substrate.FifthPercentileLuminance;
            if (substrate.MeanLuminance <
                    MinimumSubstrateMeanLuminance ||
                substrate.MeanLuminance >
                    MaximumSubstrateMeanLuminance)
            {
                failures.Add(
                    "Shared substrate mean luminance is " +
                    FormatFloat(substrate.MeanLuminance) +
                    "; accepted range is " +
                    FormatFloat(MinimumSubstrateMeanLuminance) + "–" +
                    FormatFloat(MaximumSubstrateMeanLuminance) + ".");
            }

            if (substrate.FifthPercentileLuminance <
                    MinimumSubstrateFifthPercentile ||
                substrate.FifthPercentileLuminance >
                    MaximumSubstrateFifthPercentile ||
                substrate.NinetyFifthPercentileLuminance <
                    MinimumSubstrateNinetyFifthPercentile ||
                substrate.NinetyFifthPercentileLuminance >
                    MaximumSubstrateNinetyFifthPercentile ||
                spread < MinimumSubstratePercentileSpread ||
                spread > MaximumSubstratePercentileSpread)
            {
                failures.Add(
                    "Shared substrate luminance P05/P95/spread is " +
                    FormatFloat(substrate.FifthPercentileLuminance) + " / " +
                    FormatFloat(substrate.NinetyFifthPercentileLuminance) +
                    " / " + FormatFloat(spread) +
                    "; accepted P05 " +
                    FormatFloat(MinimumSubstrateFifthPercentile) + "–" +
                    FormatFloat(MaximumSubstrateFifthPercentile) +
                    ", P95 " +
                    FormatFloat(MinimumSubstrateNinetyFifthPercentile) + "–" +
                    FormatFloat(MaximumSubstrateNinetyFifthPercentile) +
                    ", spread " +
                    FormatFloat(MinimumSubstratePercentileSpread) + "–" +
                    FormatFloat(MaximumSubstratePercentileSpread) + ".");
            }

            if (substrate.RmsContrast < MinimumSubstrateRmsContrast ||
                substrate.RmsContrast > MaximumSubstrateRmsContrast)
            {
                failures.Add(
                    "Shared substrate RMS contrast is " +
                    FormatFloat(substrate.RmsContrast) +
                    "; accepted range is " +
                    FormatFloat(MinimumSubstrateRmsContrast) + "–" +
                    FormatFloat(MaximumSubstrateRmsContrast) + ".");
            }

            if (substrate.OppositeEdgeMeanDifference >
                MaximumSubstrateEdgeDifference)
            {
                failures.Add(
                    "Shared substrate opposite-edge mean difference is " +
                    FormatFloat(substrate.OppositeEdgeMeanDifference) +
                    "; maximum is " +
                    FormatFloat(MaximumSubstrateEdgeDifference) + ".");
            }

            if (substrate.MaximumBlockMeanDeviation64 >
                    MaximumSubstrateBlockDeviation64 ||
                substrate.MaximumBlockMeanDeviation128 >
                    MaximumSubstrateBlockDeviation128 ||
                substrate.MaximumBlockMeanDeviation256 >
                    MaximumSubstrateBlockDeviation256)
            {
                failures.Add(
                    "Shared substrate macro mean deviation 64/128/256 is " +
                    FormatFloat(substrate.MaximumBlockMeanDeviation64) + " / " +
                    FormatFloat(substrate.MaximumBlockMeanDeviation128) + " / " +
                    FormatFloat(substrate.MaximumBlockMeanDeviation256) +
                    "; maxima are " +
                    FormatFloat(MaximumSubstrateBlockDeviation64) + " / " +
                    FormatFloat(MaximumSubstrateBlockDeviation128) + " / " +
                    FormatFloat(MaximumSubstrateBlockDeviation256) + ".");
            }

            if (substrate.BlockMeanRmsDeviation64 >
                    MaximumSubstrateBlockRmsDeviation64 ||
                substrate.BlockMeanRmsDeviation128 >
                    MaximumSubstrateBlockRmsDeviation128 ||
                substrate.BlockMeanRmsDeviation256 >
                    MaximumSubstrateBlockRmsDeviation256)
            {
                failures.Add(
                    "Shared substrate macro RMS deviation 64/128/256 is " +
                    FormatFloat(substrate.BlockMeanRmsDeviation64) + " / " +
                    FormatFloat(substrate.BlockMeanRmsDeviation128) + " / " +
                    FormatFloat(substrate.BlockMeanRmsDeviation256) +
                    "; maxima are " +
                    FormatFloat(MaximumSubstrateBlockRmsDeviation64) + " / " +
                    FormatFloat(MaximumSubstrateBlockRmsDeviation128) + " / " +
                    FormatFloat(MaximumSubstrateBlockRmsDeviation256) + ".");
            }
        }

        private static void ValidateNestedPlacementSequences(
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult suite,
            ICollection<string> failures)
        {
            for (int candidateIndex = 1;
                 candidateIndex < suite.Candidates.Count;
                 candidateIndex++)
            {
                GeneratedMassSparseRiverbedTileAssembler.CandidateResult
                    previous = suite.Candidates[candidateIndex - 1];
                GeneratedMassSparseRiverbedTileAssembler.CandidateResult
                    current = suite.Candidates[candidateIndex];
                for (int placementIndex = 0;
                     placementIndex < previous.Placements.Count;
                     placementIndex++)
                {
                    if (!PlacementMatches(
                            previous.Placements[placementIndex],
                            current.Placements[placementIndex]))
                    {
                        failures.Add(
                            current.Definition.StableId +
                            ": placement prefix diverged at index " +
                            placementIndex + ".");
                        break;
                    }
                }
            }
        }

        private static bool PlacementMatches(
            GeneratedMassSparseRiverbedTileAssembler.PlacementEvidence a,
            GeneratedMassSparseRiverbedTileAssembler.PlacementEvidence b)
        {
            return string.Equals(
                    a.StableId,
                    b.StableId,
                    StringComparison.Ordinal) &&
                Approximately(a.CenterX, b.CenterX) &&
                Approximately(a.CenterY, b.CenterY) &&
                Approximately(a.Radius, b.Radius) &&
                Approximately(a.RotationDegrees, b.RotationDegrees) &&
                Approximately(a.UniformScale, b.UniformScale) &&
                Approximately(a.BurialFraction, b.BurialFraction) &&
                a.UsedFallbackMesh == b.UsedFallbackMesh;
        }

        private static void ValidateCandidateDefinition(
            GeneratedMassSparseRiverbedTileAssembler.CandidateDefinition definition,
            int index,
            ICollection<string> failures)
        {
            if (index < 0 || index >= ExpectedCandidateIds.Length)
            {
                failures.Add(
                    "Unexpected candidate-definition index " + index + ".");
                return;
            }

            if (!string.Equals(
                    definition.StableId,
                    ExpectedCandidateIds[index],
                    StringComparison.Ordinal) ||
                definition.ExactPlacementCount !=
                    ExpectedPlacementCounts[index] ||
                !Approximately(
                    definition.MinimumCoverage,
                    ExpectedMinimumCoverage[index]) ||
                !Approximately(
                    definition.MaximumCoverage,
                    ExpectedMaximumCoverage[index]) ||
                definition.MaximumBroadCenterCount !=
                    ExpectedMaximumBroadCenterCount[index])
            {
                failures.Add(
                    "Candidate definition changed at index " + index + ".");
            }
        }

        private static void ValidateCandidate(
            GeneratedMassSparseRiverbedTileAssembler.CandidateResult candidate,
            GeneratedMassSparseRiverbedTileAssembler.SubstrateResult substrate,
            ICollection<string> failures)
        {
            if (!candidate.Succeeded)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": candidate build failed: " + candidate.Failure);
                return;
            }

            if (candidate.Placements.Count !=
                candidate.Definition.ExactPlacementCount)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": placement count is " + candidate.Placements.Count +
                    "; expected exactly " +
                    candidate.Definition.ExactPlacementCount + ".");
            }

            if (candidate.Coverage <
                    candidate.Definition.MinimumCoverage ||
                candidate.Coverage >
                    candidate.Definition.MaximumCoverage)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": coverage is " + FormatPercent(candidate.Coverage) +
                    "; accepted range is " +
                    FormatPercent(candidate.Definition.MinimumCoverage) +
                    "–" +
                    FormatPercent(candidate.Definition.MaximumCoverage) +
                    ".");
            }

            if (candidate.UniqueSourceCount != candidate.Placements.Count)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": unique-source count is " +
                    candidate.UniqueSourceCount +
                    "; every placement must use a unique frozen source.");
            }

            for (int index = 0; index < candidate.SourceUsage.Count; index++)
            {
                if (candidate.SourceUsage[index].Count > 1)
                {
                    failures.Add(
                        candidate.Definition.StableId + "/" +
                        candidate.SourceUsage[index].StableId +
                        ": source is repeated " +
                        candidate.SourceUsage[index].Count + " times.");
                }
            }

            float smallFraction = candidate.Placements.Count > 0
                ? candidate.SmallScaleCount /
                    (float)candidate.Placements.Count
                : 0f;
            if (smallFraction <
                    GeneratedMassSparseRiverbedTileAssembler
                        .MinimumSmallPlacementFraction ||
                candidate.AccentScaleCount != 0)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": small-placement fraction / accent count is " +
                    FormatPercent(smallFraction) + " / " +
                    candidate.AccentScaleCount +
                    "; minimum small fraction is " +
                    FormatPercent(
                        GeneratedMassSparseRiverbedTileAssembler
                            .MinimumSmallPlacementFraction) +
                    " and accent count must be zero.");
            }

            if (candidate.MinimumObservedScale <
                    GeneratedMassSparseRiverbedTileAssembler
                        .MinimumPlacementScale - 0.0001f ||
                candidate.MaximumObservedScale >
                    GeneratedMassSparseRiverbedTileAssembler
                        .MaximumPlacementScale + 0.0001f)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": observed scale range is " +
                    FormatFloat(candidate.MinimumObservedScale) + "–" +
                    FormatFloat(candidate.MaximumObservedScale) +
                    "; approved range is " +
                    FormatFloat(
                        GeneratedMassSparseRiverbedTileAssembler
                            .MinimumPlacementScale) + "–" +
                    FormatFloat(
                        GeneratedMassSparseRiverbedTileAssembler
                            .MaximumPlacementScale) + ".");
            }

            if (candidate.MinimumNormalizedNeighbourSeparation + 0.0001f <
                GeneratedMassSparseRiverbedTileAssembler
                    .MinimumSpacingFactor)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": minimum normalized neighbour separation is " +
                    FormatFloat(
                        candidate.MinimumNormalizedNeighbourSeparation) +
                    "; minimum is " +
                    FormatFloat(
                        GeneratedMassSparseRiverbedTileAssembler
                            .MinimumSpacingFactor) + ".");
            }

            if (candidate.MaximumNearNeighbourCount >
                GeneratedMassSparseRiverbedTileAssembler
                    .MaximumNearNeighbourCount)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": maximum near-neighbour count is " +
                    candidate.MaximumNearNeighbourCount + "; maximum is " +
                    GeneratedMassSparseRiverbedTileAssembler
                        .MaximumNearNeighbourCount + ".");
            }

            if (candidate.MaximumBroadCenterCount >
                candidate.Definition.MaximumBroadCenterCount)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": maximum broad-radius centre count is " +
                    candidate.MaximumBroadCenterCount + "; maximum is " +
                    candidate.Definition.MaximumBroadCenterCount + ".");
            }

            if (substrate == null ||
                !string.Equals(
                    candidate.SubstrateFingerprint,
                    substrate.Fingerprint,
                    StringComparison.Ordinal))
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": candidate does not use the shared substrate " +
                    "fingerprint.");
            }

            for (int index = 0; index < candidate.Placements.Count; index++)
            {
                GeneratedMassSparseRiverbedTileAssembler.PlacementEvidence
                    placement = candidate.Placements[index];
                if (placement.UniformScale <
                        GeneratedMassSparseRiverbedTileAssembler
                            .MinimumPlacementScale - 0.0001f ||
                    placement.UniformScale >
                        GeneratedMassSparseRiverbedTileAssembler
                            .MaximumPlacementScale + 0.0001f)
                {
                    failures.Add(
                        candidate.Definition.StableId + "/" +
                        placement.Index +
                        ": placement scale is outside the approved range.");
                }

                if (placement.RootContactPixels <= 0)
                {
                    failures.Add(
                        candidate.Definition.StableId + "/" +
                        placement.Index + ": root contact is empty.");
                }
                else if (placement.RootPerimeterAffectedFraction >
                    GeneratedMassSparseRiverbedTileAssembler
                        .MaximumRootPerimeterFraction + 0.0001f)
                {
                    failures.Add(
                        candidate.Definition.StableId + "/" +
                        placement.Index + ": root contact affects " +
                        FormatPercent(
                            placement.RootPerimeterAffectedFraction) +
                        " of the perimeter; maximum is " +
                        FormatPercent(
                            GeneratedMassSparseRiverbedTileAssembler
                                .MaximumRootPerimeterFraction) + ".");
                }
            }

            if (candidate.Seams == null || !candidate.Seams.Passed)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": periodic seam metrics exceeded tolerance.");
            }

            ValidatePalettePayload(candidate, failures);
            ValidateCandidatePixels(candidate, failures);
        }

        private static void ValidatePalettePayload(
            GeneratedMassSparseRiverbedTileAssembler.CandidateResult candidate,
            ICollection<string> failures)
        {
            float formRange = candidate.PaletteFormMaximum -
                candidate.PaletteFormMinimum;
            if (formRange < MinimumPaletteFormRange)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": palette-form range is " + FormatFloat(formRange) +
                    "; minimum is " +
                    FormatFloat(MinimumPaletteFormRange) + ".");
            }

            if (candidate.PaletteFormSubstrateMean <
                    MinimumPaletteFormSubstrateMean ||
                candidate.PaletteFormSubstrateMean >
                    MaximumPaletteFormSubstrateMean)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": substrate palette-form mean is " +
                    FormatFloat(candidate.PaletteFormSubstrateMean) +
                    "; accepted range is " +
                    FormatFloat(MinimumPaletteFormSubstrateMean) + "–" +
                    FormatFloat(MaximumPaletteFormSubstrateMean) + ".");
            }

            if (candidate.PaletteFormRockMean <
                    MinimumPaletteFormRockMean ||
                candidate.PaletteFormRockMean >
                    MaximumPaletteFormRockMean)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": rock palette-form mean is " +
                    FormatFloat(candidate.PaletteFormRockMean) +
                    "; accepted range is " +
                    FormatFloat(MinimumPaletteFormRockMean) + "–" +
                    FormatFloat(MaximumPaletteFormRockMean) + ".");
            }

            float bandSeparation = candidate.PaletteFormSubstrateMean -
                candidate.PaletteFormRockMean;
            if (bandSeparation < MinimumPaletteFormBandSeparation)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": substrate/rock palette-form separation is " +
                    FormatFloat(bandSeparation) + "; minimum is " +
                    FormatFloat(MinimumPaletteFormBandSeparation) + ".");
            }

            if (candidate.PackedSubstrateSlopeDeviationMean >
                    MaximumPackedSubstrateSlopeDeviation ||
                candidate.PackedSubstrateCavityMean >
                    MaximumPackedSubstrateCavityMean)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": packed substrate slope/cavity means are " +
                    FormatFloat(
                        candidate.PackedSubstrateSlopeDeviationMean) + " / " +
                    FormatFloat(candidate.PackedSubstrateCavityMean) +
                    "; maxima are " +
                    FormatFloat(MaximumPackedSubstrateSlopeDeviation) +
                    " / " +
                    FormatFloat(MaximumPackedSubstrateCavityMean) + ".");
            }

            if (candidate.PackedRockSlopeMagnitudeMean <
                    MinimumPackedRockSlopeMagnitude ||
                candidate.PackedRockCavityMean <
                    MinimumPackedRockCavityMean ||
                candidate.PackedRockCavityMaximum <
                    MinimumPackedRockCavityMaximum)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": packed rock slope/cavity mean/max are " +
                    FormatFloat(candidate.PackedRockSlopeMagnitudeMean) +
                    " / " +
                    FormatFloat(candidate.PackedRockCavityMean) + " / " +
                    FormatFloat(candidate.PackedRockCavityMaximum) +
                    "; minima are " +
                    FormatFloat(MinimumPackedRockSlopeMagnitude) + " / " +
                    FormatFloat(MinimumPackedRockCavityMean) + " / " +
                    FormatFloat(MinimumPackedRockCavityMaximum) + ".");
            }

            if (candidate.NeutralToHigherContrastMeanDifference <
                    MinimumNeutralToContrastDifference ||
                candidate.NeutralToAlternateMeanDifference <
                    MinimumNeutralToAlternateDifference)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": neutral-to-contrast/alternate mean differences are " +
                    FormatFloat(
                        candidate.NeutralToHigherContrastMeanDifference) +
                    " / " +
                    FormatFloat(candidate.NeutralToAlternateMeanDifference) +
                    "; minima are " +
                    FormatFloat(MinimumNeutralToContrastDifference) + " / " +
                    FormatFloat(MinimumNeutralToAlternateDifference) + ".");
            }

            if (candidate.FractionalSilhouetteCoverageFraction <
                    MinimumFractionalSilhouetteCoverageFraction)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": fractional silhouette coverage is " +
                    FormatPercent(
                        candidate.FractionalSilhouetteCoverageFraction) +
                    "; minimum is " +
                    FormatPercent(
                        MinimumFractionalSilhouetteCoverageFraction) + ".");
            }

            if (candidate.MaximumAdjacentPaletteFormDifference >
                    MaximumAdjacentPaletteFormDifference)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": maximum adjacent Palette Form difference is " +
                    FormatFloat(
                        candidate.MaximumAdjacentPaletteFormDifference) +
                    "; maximum is " +
                    FormatFloat(MaximumAdjacentPaletteFormDifference) + ".");
            }

            if (candidate.FeatureMaskMaximum < MinimumFeatureMaskMaximum ||
                candidate.FeatureMaskMean < MinimumFeatureMaskMean ||
                candidate.FeatureMaskMean > MaximumFeatureMaskMean)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": feature-mask mean/maximum is " +
                    FormatFloat(candidate.FeatureMaskMean) + " / " +
                    FormatFloat(candidate.FeatureMaskMaximum) +
                    "; accepted mean " +
                    FormatFloat(MinimumFeatureMaskMean) + "–" +
                    FormatFloat(MaximumFeatureMaskMean) +
                    ", minimum maximum " +
                    FormatFloat(MinimumFeatureMaskMaximum) + ".");
            }

            if (candidate.SubstrateOnlyFormMean <
                    MinimumSubstrateOnlyFormMean ||
                candidate.SubstrateOnlyFormMean >
                    MaximumSubstrateOnlyFormMean ||
                candidate.SubstrateOnlyRoughnessMean <
                    MinimumSubstrateOnlyRoughnessMean ||
                candidate.SubstrateOnlyRoughnessMean >
                    MaximumSubstrateOnlyRoughnessMean)
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": substrate-only form/roughness means are " +
                    FormatFloat(candidate.SubstrateOnlyFormMean) + " / " +
                    FormatFloat(candidate.SubstrateOnlyRoughnessMean) +
                    "; accepted form " +
                    FormatFloat(MinimumSubstrateOnlyFormMean) + "–" +
                    FormatFloat(MaximumSubstrateOnlyFormMean) +
                    ", roughness " +
                    FormatFloat(MinimumSubstrateOnlyRoughnessMean) + "–" +
                    FormatFloat(MaximumSubstrateOnlyRoughnessMean) + ".");
            }

            if (string.IsNullOrEmpty(candidate.PalettePayloadFingerprint) ||
                string.IsNullOrEmpty(
                    candidate.PalettePreviewNeutralFingerprint) ||
                string.IsNullOrEmpty(
                    candidate.PalettePreviewHigherContrastFingerprint) ||
                string.IsNullOrEmpty(
                    candidate.PalettePreviewAlternateFingerprint))
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": palette payload or preview fingerprint is missing.");
            }
            else if (string.Equals(
                         candidate.PalettePreviewNeutralFingerprint,
                         candidate.PalettePreviewHigherContrastFingerprint,
                         StringComparison.Ordinal) ||
                     string.Equals(
                         candidate.PalettePreviewNeutralFingerprint,
                         candidate.PalettePreviewAlternateFingerprint,
                         StringComparison.Ordinal) ||
                     string.Equals(
                         candidate.PalettePreviewHigherContrastFingerprint,
                         candidate.PalettePreviewAlternateFingerprint,
                         StringComparison.Ordinal))
            {
                failures.Add(
                    candidate.Definition.StableId +
                    ": palette preview fingerprints are not distinct.");
            }
        }

        private static void ValidateCandidatePixels(
            GeneratedMassSparseRiverbedTileAssembler.CandidateResult candidate,
            ICollection<string> failures)
        {
            string prefix = candidate.Definition.StableId + " ";
            ValidatePixels(
                prefix + "Moderate",
                candidate.Moderate,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "PlacementDebug",
                candidate.PlacementDebug,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "StableIdDebug",
                candidate.StableIdDebug,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "Mask",
                candidate.Mask,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "Height",
                candidate.Height,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "Normals",
                candidate.Normals,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "Variation",
                candidate.Variation,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "RootDarkening",
                candidate.RootDarkening,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "EdgeWear",
                candidate.EdgeWear,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "MipContactSheet",
                candidate.MipContactSheet,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "PaletteForm",
                candidate.PaletteForm,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "RuntimePackedDetail",
                candidate.RuntimePackedDetail,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "PalettePreviewNeutral",
                candidate.PalettePreviewNeutral,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "PalettePreviewHigherContrast",
                candidate.PalettePreviewHigherContrast,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "PalettePreviewAlternate",
                candidate.PalettePreviewAlternate,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                failures);
            ValidatePixels(
                prefix + "PaletteComparison",
                candidate.PaletteComparison,
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
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult retained,
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult comparison,
            bool deterministic,
            IReadOnlyCollection<string> failures,
            IReadOnlyCollection<string> warnings)
        {
            StringBuilder builder = new StringBuilder(32768);
            builder.AppendLine(
                "GENERATED MASS FEATURE-AWARE PALETTE-NEUTRAL SPARSE RIVERBED " +
                "RUNTIME PAYLOAD PROOF — GSU-M2.7C.5E.2.2");
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
                "Source contract: frozen 18-rock algorithm-8 library, " +
                "unified wear 0.52, fallback wear 0.56, frozen Moderate " +
                "response; exact-count sparse composition plus paired " +
                "palette-form and packed-detail proof payload.");
            builder.AppendLine();

            builder.AppendLine("DETERMINISM");
            builder.AppendLine(
                "Non-retained comparison fingerprint: " +
                (comparison.Fingerprint ?? "FAIL"));
            builder.AppendLine(
                "Retained evidence fingerprint: " +
                (retained.Fingerprint ?? "FAIL"));
            builder.AppendLine(
                "Repeated suite identical: " +
                (deterministic ? "Yes" : "No"));
            builder.AppendLine();

            if (retained.Substrate != null)
            {
                AppendSubstrate(builder, retained.Substrate);
            }

            AppendPaletteDefinitions(builder);

            if (retained.Succeeded)
            {
                builder.AppendLine("CANDIDATE RESULTS");
                for (int index = 0;
                     index < retained.Candidates.Count;
                     index++)
                {
                    AppendCandidate(builder, retained.Candidates[index]);
                }
            }
            else
            {
                builder.AppendLine("RETAINED SUITE FAILURE");
                builder.AppendLine(retained.Failure ?? "Unknown failure.");
                builder.AppendLine();
            }

            builder.AppendLine("OUTPUTS");
            builder.AppendLine("Report: " + ReportPath);
            if (retained.Succeeded)
            {
                builder.AppendLine("SubstrateOnly.png");
                builder.AppendLine("SubstrateOnly_3x3.png");
                for (int index = 0;
                     index < retained.Candidates.Count;
                     index++)
                {
                    string prefix = retained.Candidates[index]
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
                    builder.AppendLine(prefix + "_PaletteForm.png");
                    builder.AppendLine(prefix + "_RuntimePackedDetail.png");
                    builder.AppendLine(prefix + "_PalettePreview_Neutral.png");
                    builder.AppendLine(
                        prefix + "_PalettePreview_HigherContrast.png");
                    builder.AppendLine(
                        prefix + "_PalettePreview_HigherContrast_3x3.png");
                    builder.AppendLine(
                        prefix + "_PalettePreview_Alternate.png");
                    builder.AppendLine(prefix + "_PaletteComparison.png");
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
                    "VERDICT: PASS — deterministic shared substrate, nested " +
                    "exact-count 6/9/12 placements, unique sources, " +
                    "radius-aware spacing, anti-hotspot limits, mostly-small " +
                    "scales, root-sector limits, toroidal seams, low-macro " +
                    "micro-noise substrate gates, feature-aware packed " +
                    "palette payload, distinct recolour previews, " +
                    "mip evidence and complete output generation passed.");
            }

            builder.AppendLine();
            builder.AppendLine(
                "PENDING GATE: inspect the feature-aware PaletteForm, " +
                "RuntimePackedDetail and HigherContrast 3x3 outputs, then " +
                "refresh the three installed candidates and validate Bank " +
                "and Riverbed feature-free edge clearances in scene.");
            return builder.ToString();
        }

        private static void AppendSubstrate(
            StringBuilder builder,
            GeneratedMassSparseRiverbedTileAssembler.SubstrateResult substrate)
        {
            builder.AppendLine("SHARED SUBSTRATE");
            builder.AppendLine(
                "    mean / P05 / P95 luminance: " +
                FormatFloat(substrate.MeanLuminance) + " / " +
                FormatFloat(substrate.FifthPercentileLuminance) + " / " +
                FormatFloat(substrate.NinetyFifthPercentileLuminance));
            builder.AppendLine(
                "    RMS contrast / opposite-edge mean difference: " +
                FormatFloat(substrate.RmsContrast) + " / " +
                FormatFloat(substrate.OppositeEdgeMeanDifference));
            builder.AppendLine(
                "    macro mean deviation 64/128/256: " +
                FormatFloat(substrate.MaximumBlockMeanDeviation64) + " / " +
                FormatFloat(substrate.MaximumBlockMeanDeviation128) + " / " +
                FormatFloat(substrate.MaximumBlockMeanDeviation256));
            builder.AppendLine(
                "    macro RMS deviation 64/128/256: " +
                FormatFloat(substrate.BlockMeanRmsDeviation64) + " / " +
                FormatFloat(substrate.BlockMeanRmsDeviation128) + " / " +
                FormatFloat(substrate.BlockMeanRmsDeviation256));
            builder.AppendLine(
                "    fingerprint: " + substrate.Fingerprint);
            builder.AppendLine();
        }

        private static void AppendPaletteDefinitions(
            StringBuilder builder)
        {
            builder.AppendLine("PALETTE PROOF");
            IReadOnlyList<GeneratedMassSparseRiverbedTileAssembler.PaletteDefinition>
                palettes = GeneratedMassSparseRiverbedTileAssembler
                    .GetPaletteDefinitions();
            for (int index = 0; index < palettes.Count; index++)
            {
                GeneratedMassSparseRiverbedTileAssembler.PaletteDefinition
                    palette = palettes[index];
                builder.AppendLine(
                    "    " + palette.DisplayName +
                    " base/dark/light/cavity: " +
                    FormatColor(palette.BaseColor) + " / " +
                    FormatColor(palette.DarkColor) + " / " +
                    FormatColor(palette.LightColor) + " / " +
                    FormatColor(palette.CavityColor));
            }

            builder.AppendLine(
                "    comparison layout: Neutral / Higher Contrast / " +
                "Alternate / Palette Form.");
            builder.AppendLine();
        }

        private static void AppendCandidate(
            StringBuilder builder,
            GeneratedMassSparseRiverbedTileAssembler.CandidateResult candidate)
        {
            builder.AppendLine(
                "[" + candidate.Definition.StableId + "] " +
                candidate.Definition.DisplayName);
            builder.AppendLine(
                "    exact placements / actual coverage / range: " +
                candidate.Definition.ExactPlacementCount + " / " +
                FormatPercent(candidate.Coverage) + " / " +
                FormatPercent(candidate.Definition.MinimumCoverage) + "–" +
                FormatPercent(candidate.Definition.MaximumCoverage));
            builder.AppendLine(
                "    descriptive quiet blocks: " +
                FormatPercent(candidate.QuietBlockFraction) +
                " (occupied " + candidate.OccupiedQuietBlocks + ")");
            builder.AppendLine(
                "    placements / unique sources / maximum source share: " +
                candidate.Placements.Count + " / " +
                candidate.UniqueSourceCount + " / " +
                FormatPercent(candidate.MaximumObservedSourceShare));
            builder.AppendLine(
                "    scale min/mean/max and classes S/M/L/A: " +
                FormatFloat(candidate.MinimumObservedScale) + " / " +
                FormatFloat(candidate.MeanObservedScale) + " / " +
                FormatFloat(candidate.MaximumObservedScale) + " — " +
                candidate.SmallScaleCount + "/" +
                candidate.MediumScaleCount + "/" +
                candidate.LargeScaleCount + "/" +
                candidate.AccentScaleCount);
            builder.AppendLine(
                "    minimum normalized separation / max near / max broad: " +
                FormatFloat(
                    candidate.MinimumNormalizedNeighbourSeparation) + " / " +
                candidate.MaximumNearNeighbourCount + " / " +
                candidate.MaximumBroadCenterCount);
            builder.AppendLine(
                "    maximum root perimeter affected: " +
                FormatPercent(
                    candidate.MaximumRootPerimeterAffectedFraction));
            builder.AppendLine(
                "    rejection counts spacing/hotspot/overlap/coverage: " +
                candidate.RejectedForSpacing + " / " +
                candidate.RejectedForHotspot + " / " +
                candidate.RejectedForOverlap + " / " +
                candidate.RejectedForCoverage);
            if (candidate.Seams != null)
            {
                builder.AppendLine(
                    "    seam means mask/height/normal/variation/root/wear/" +
                    "preview: " +
                    FormatFloat(candidate.Seams.MaskMean) + " / " +
                    FormatFloat(candidate.Seams.HeightMean) + " / " +
                    FormatFloat(candidate.Seams.NormalMean) + " / " +
                    FormatFloat(candidate.Seams.VariationMean) + " / " +
                    FormatFloat(candidate.Seams.RootMean) + " / " +
                    FormatFloat(candidate.Seams.WearMean) + " / " +
                    FormatFloat(candidate.Seams.PreviewMean));
                builder.AppendLine(
                    "    payload seams form/packed/higher-contrast preview: " +
                    FormatFloat(candidate.Seams.PaletteFormMean) + " / " +
                    FormatFloat(candidate.Seams.PackedDetailMean) + " / " +
                    FormatFloat(candidate.Seams.PalettePreviewMean));
            }

            builder.AppendLine(
                "    palette form min/max and substrate/rock means: " +
                FormatFloat(candidate.PaletteFormMinimum) + " / " +
                FormatFloat(candidate.PaletteFormMaximum) + " — " +
                FormatFloat(candidate.PaletteFormSubstrateMean) + " / " +
                FormatFloat(candidate.PaletteFormRockMean));
            builder.AppendLine(
                "    packed substrate slope/cavity and rock slope/cavity/max: " +
                FormatFloat(
                    candidate.PackedSubstrateSlopeDeviationMean) + " / " +
                FormatFloat(candidate.PackedSubstrateCavityMean) + " — " +
                FormatFloat(candidate.PackedRockSlopeMagnitudeMean) + " / " +
                FormatFloat(candidate.PackedRockCavityMean) + " / " +
                FormatFloat(candidate.PackedRockCavityMaximum));
            builder.AppendLine(
                "    neutral-to-higher-contrast / alternate mean difference: " +
                FormatFloat(
                    candidate.NeutralToHigherContrastMeanDifference) + " / " +
                FormatFloat(candidate.NeutralToAlternateMeanDifference));
            builder.AppendLine(
                "    fractional silhouette coverage / max adjacent form step: " +
                FormatPercent(
                    candidate.FractionalSilhouetteCoverageFraction) + " / " +
                FormatFloat(
                    candidate.MaximumAdjacentPaletteFormDifference));
            builder.AppendLine(
                "    feature mask mean/max and substrate-only form/roughness: " +
                FormatFloat(candidate.FeatureMaskMean) + " / " +
                FormatFloat(candidate.FeatureMaskMaximum) + " — " +
                FormatFloat(candidate.SubstrateOnlyFormMean) + " / " +
                FormatFloat(candidate.SubstrateOnlyRoughnessMean));
            builder.AppendLine(
                "    palette payload fingerprint: " +
                candidate.PalettePayloadFingerprint);
            builder.AppendLine(
                "    preview fingerprints neutral/contrast/alternate: " +
                candidate.PalettePreviewNeutralFingerprint + " / " +
                candidate.PalettePreviewHigherContrastFingerprint + " / " +
                candidate.PalettePreviewAlternateFingerprint);

            builder.AppendLine("    source usage:");
            for (int index = 0;
                 index < candidate.SourceUsage.Count;
                 index++)
            {
                GeneratedMassSparseRiverbedTileAssembler.SourceUsage usage =
                    candidate.SourceUsage[index];
                if (usage.Count <= 0)
                {
                    continue;
                }

                builder.AppendLine(
                    "      " + usage.StableId + " = " + usage.Count +
                    (usage.UsedFallbackMesh
                        ? " (fallback)"
                        : " (unified)"));
            }

            builder.AppendLine(
                "    substrate fingerprint: " +
                candidate.SubstrateFingerprint);
            builder.AppendLine(
                "    candidate fingerprint: " + candidate.Fingerprint);
            builder.AppendLine();
        }

        private static void WriteEvidence(
            GeneratedMassSparseRiverbedTileAssembler.SuiteResult suite)
        {
            WritePng(
                Path.Combine(OutputDirectory, "SubstrateOnly.png"),
                suite.Substrate.Color,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
            Color32[] substrateThreeByThree =
                GeneratedMassSparseRiverbedTileAssembler
                    .BuildThreeByThreeEvidence(suite.Substrate.Color);
            WritePng(
                Path.Combine(OutputDirectory, "SubstrateOnly_3x3.png"),
                substrateThreeByThree,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution * 3,
                GeneratedMassSparseRiverbedTileAssembler.FinalResolution * 3);
            substrateThreeByThree = null;

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
                threeByThree = null;
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
                WritePng(
                    prefix + "_PaletteForm.png",
                    candidate.PaletteForm,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_RuntimePackedDetail.png",
                    candidate.RuntimePackedDetail,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_PalettePreview_Neutral.png",
                    candidate.PalettePreviewNeutral,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_PalettePreview_HigherContrast.png",
                    candidate.PalettePreviewHigherContrast,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                Color32[] paletteThreeByThree =
                    GeneratedMassSparseRiverbedTileAssembler
                        .BuildThreeByThreeEvidence(
                            candidate.PalettePreviewHigherContrast);
                WritePng(
                    prefix + "_PalettePreview_HigherContrast_3x3.png",
                    paletteThreeByThree,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution * 3,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution * 3);
                paletteThreeByThree = null;
                WritePng(
                    prefix + "_PalettePreview_Alternate.png",
                    candidate.PalettePreviewAlternate,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution,
                    GeneratedMassSparseRiverbedTileAssembler.FinalResolution);
                WritePng(
                    prefix + "_PaletteComparison.png",
                    candidate.PaletteComparison,
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

        private static string FormatColor(Color color)
        {
            return "(" +
                color.r.ToString("F3", CultureInfo.InvariantCulture) + ", " +
                color.g.ToString("F3", CultureInfo.InvariantCulture) + ", " +
                color.b.ToString("F3", CultureInfo.InvariantCulture) + ")";
        }
    }
}
