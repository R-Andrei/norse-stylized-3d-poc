using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    public static class TreeGenerator
    {
        public const int CurrentGeneratorVersion = 3;
        private const float GoldenAngleDegrees = 137.507764f;
        private const float TwoPi = Mathf.PI * 2f;
        private const float Epsilon = 0.00001f;

        private sealed class GenerationContext
        {
            internal TreeGenerationRecipe Recipe;
            internal TreeFamilyProfile Profile;
            internal TreeReferenceCalibrationPreset Calibration;
            internal TreeMaterialPalette Palette;
            internal TreeGenerationOverrides InstanceOverrides;
            internal int MasterSeed;
            internal TreeSeedSet Seeds;
            internal TreeResolvedParameters Parameters;
            internal List<TreeBranchDefinition> Branches =
                new List<TreeBranchDefinition>();
            internal List<TreeFoliageClusterDefinition> FoliageClusters =
                new List<TreeFoliageClusterDefinition>();
            internal List<string> Warnings = new List<string>();
            internal int RejectedBranches;
        }

        private struct ParentFrame
        {
            internal Vector3 Position;
            internal Vector3 Tangent;
            internal Vector3 Normal;
            internal Vector3 Binormal;
            internal float Radius;
        }

        public static TreeGenerationResult Generate(
            TreeGenerationRecipe recipe,
            TreeGenerationOverrides instanceOverrides,
            int masterSeed)
        {
            return GenerateInternal(recipe, instanceOverrides, masterSeed, true);
        }

        public static string RunDeterminismAndDependencyValidation(
            TreeGenerationRecipe recipe,
            TreeGenerationOverrides instanceOverrides,
            int masterSeed)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var report = new StringBuilder(16384);
            report.AppendLine("[TREE-GEN.2C Determinism and Selective-Regeneration Validation]");
            report.Append("Generated: ").AppendLine(timestamp);
            report.AppendLine(
                "Scope: structural definitions, independent seed streams, dependency fingerprints, and palette/foliage isolation; no meshes are generated");
            report.AppendLine();

            TreeGenerationResult baselineA =
                GenerateInternal(recipe, instanceOverrides, masterSeed, false);
            TreeGenerationResult baselineB =
                GenerateInternal(recipe, instanceOverrides, masterSeed, false);

            if (!baselineA.Passed || !baselineB.Passed)
            {
                report.AppendLine("FAIL | Baseline generation did not pass.");
                report.AppendLine(baselineA.Report ?? string.Empty);
                report.AppendLine(baselineB.Report ?? string.Empty);
                report.AppendLine("Status: FAIL");
                return report.ToString();
            }

            TreeDefinition baseline = baselineA.Definition;
            bool passed = true;
            passed &= AppendTest(
                report,
                "Same complete inputs reproduce the structural fingerprint",
                baseline.StructuralFingerprint == baselineB.Definition.StructuralFingerprint,
                baseline.StructuralFingerprint,
                baselineB.Definition.StructuralFingerprint);

            TreeGenerationOverrides colourOverrides = CloneOverrides(instanceOverrides);
            Color changedColour = baseline.ResolvedParameters.FoliageBaseColor;
            changedColour.r = Mathf.Repeat(changedColour.r + 0.173f, 1f);
            colourOverrides.SetFoliageColorForTest(changedColour);
            TreeGenerationResult colourResult = GenerateInternal(
                recipe,
                colourOverrides,
                masterSeed,
                false);
            passed &= AppendIsolationTest(
                report,
                "Foliage colour changes preserve structural and foliage-geometry fingerprints",
                colourResult,
                baseline,
                requireTrunkSame: true,
                requireBranchesSame: true,
                requireFoliageSame: true,
                requirePaletteSame: false);

            TreeGenerationOverrides volumeOverrides = CloneOverrides(instanceOverrides);
            float changedVolume = ChooseDifferentValue(
                baseline.ResolvedParameters.CrownVolume,
                recipe.FamilyProfile.OverallForm.CrownVolume);
            volumeOverrides.SetCrownVolumeForTest(changedVolume);
            TreeGenerationResult volumeResult = GenerateInternal(
                recipe,
                volumeOverrides,
                masterSeed,
                false);
            passed &= AppendIsolationTest(
                report,
                "Foliage-volume changes preserve trunk and branch fingerprints",
                volumeResult,
                baseline,
                requireTrunkSame: true,
                requireBranchesSame: true,
                requireFoliageSame: false,
                requirePaletteSame: true);

            if (recipe.FamilyProfile.Foliage.ClusterCount.Maximum >
                recipe.FamilyProfile.Foliage.ClusterCount.Minimum)
            {
                TreeGenerationOverrides densityOverrides = CloneOverrides(instanceOverrides);
                int changedClusterCount = ChooseDifferentValue(
                    baseline.ResolvedParameters.FoliageClusterCount,
                    recipe.FamilyProfile.Foliage.ClusterCount);
                densityOverrides.SetFoliageClusterCountForTest(changedClusterCount);
                TreeGenerationResult densityResult = GenerateInternal(
                    recipe,
                    densityOverrides,
                    masterSeed,
                    false);
                passed &= AppendIsolationTest(
                    report,
                    "Foliage-density changes preserve trunk and branch fingerprints",
                    densityResult,
                    baseline,
                    requireTrunkSame: true,
                    requireBranchesSame: true,
                    requireFoliageSame: false,
                    requirePaletteSame: true);
            }
            else
            {
                report.AppendLine(
                    "PASS | Foliage-density isolation is not applicable because this profile locks cluster count to one value.");
            }

            bool branchIsolation = TryGenerateBranchCountIsolationCandidate(
                recipe,
                instanceOverrides,
                masterSeed,
                baseline,
                out int requestedBranchCount,
                out TreeGenerationResult branchResult);
            passed &= AppendTest(
                report,
                "Primary-branch count changes preserve trunk and palette while changing branches",
                branchIsolation,
                "count=" + baseline.ResolvedParameters.PrimaryBranchCount +
                    " | trunk=" + baseline.TrunkFingerprint +
                    " | branches=" + baseline.BranchFingerprint +
                    " | palette=" + baseline.PaletteFingerprint,
                branchResult != null && branchResult.Passed
                    ? "requested=" + requestedBranchCount +
                      " | resolved=" +
                      branchResult.Definition.ResolvedParameters.PrimaryBranchCount +
                      " | trunk=" + branchResult.Definition.TrunkFingerprint +
                      " | branches=" + branchResult.Definition.BranchFingerprint +
                      " | palette=" + branchResult.Definition.PaletteFingerprint
                    : "No valid alternate count produced a generated result");

            TreeGenerationOverrides trunkOverrides = CloneOverrides(instanceOverrides);
            float changedCurvature = ChooseDifferentValue(
                baseline.ResolvedParameters.TrunkCurvature,
                recipe.FamilyProfile.Trunk.Curvature);
            trunkOverrides.SetTrunkCurvatureForTest(changedCurvature);
            TreeGenerationResult trunkResult = GenerateInternal(
                recipe,
                trunkOverrides,
                masterSeed,
                false);
            bool trunkInvalidation =
                trunkResult.Passed &&
                trunkResult.Definition.TrunkFingerprint != baseline.TrunkFingerprint &&
                trunkResult.Definition.BranchFingerprint != baseline.BranchFingerprint;
            passed &= AppendTest(
                report,
                "Trunk-curvature changes invalidate trunk and descendant structure",
                trunkInvalidation,
                baseline.TrunkFingerprint + " / " + baseline.BranchFingerprint,
                trunkResult.Passed
                    ? trunkResult.Definition.TrunkFingerprint + " / " +
                      trunkResult.Definition.BranchFingerprint
                    : "Generation failed");

            TreeGenerationOverrides archOverrides = CloneOverrides(instanceOverrides);
            float changedArch = ChooseDifferentValue(
                baseline.ResolvedParameters.BranchArchStrength,
                recipe.FamilyProfile.PrimaryBranches.ArchStrength);
            archOverrides.SetBranchArchStrengthForTest(changedArch);
            TreeGenerationResult archResult = GenerateInternal(
                recipe, archOverrides, masterSeed, false);
            passed &= AppendBranchChangeTest(
                report,
                "Branch-arch changes preserve trunk and palette while changing branches",
                archResult,
                baseline);

            TreeGenerationOverrides startHeightOverrides = CloneOverrides(instanceOverrides);
            float changedStartHeight = ChooseDifferentValue(
                baseline.ResolvedParameters.PrimaryBranchStartHeight,
                recipe.FamilyProfile.PrimaryBranches.StartHeight);
            startHeightOverrides.SetPrimaryBranchStartHeightForTest(changedStartHeight);
            TreeGenerationResult startHeightResult = GenerateInternal(
                recipe, startHeightOverrides, masterSeed, false);
            passed &= AppendBranchChangeTest(
                report,
                "Branch-start-height changes preserve trunk and palette while changing branches",
                startHeightResult,
                baseline);

            TreeGenerationOverrides symmetryOverrides = CloneOverrides(instanceOverrides);
            float changedSymmetry = ChooseDifferentValue(
                baseline.ResolvedParameters.AzimuthSymmetry,
                recipe.FamilyProfile.PrimaryBranches.AzimuthSymmetry);
            symmetryOverrides.SetAzimuthSymmetryForTest(changedSymmetry);
            TreeGenerationResult symmetryResult = GenerateInternal(
                recipe, symmetryOverrides, masterSeed, false);
            passed &= AppendBranchChangeTest(
                report,
                "Azimuth-symmetry changes preserve trunk and palette while changing branches",
                symmetryResult,
                baseline);

            TreeGenerationOverrides biasOverrides = CloneOverrides(instanceOverrides);
            float changedBias = ChooseDifferentValue(
                baseline.ResolvedParameters.DirectionalBiasStrength,
                recipe.FamilyProfile.PrimaryBranches.DirectionalBiasStrength);
            biasOverrides.SetDirectionalBiasStrengthForTest(changedBias);
            TreeGenerationResult biasResult = GenerateInternal(
                recipe, biasOverrides, masterSeed, false);
            passed &= AppendBranchChangeTest(
                report,
                "Directional-bias changes preserve trunk and palette while changing branches",
                biasResult,
                baseline);

            TreeBarkMeshSettings barkSettings =
                TreeBarkMeshSettings.CreateVerticalSliceDefaults(
                    baseline.ResolvedParameters.Family);
            string baselineBarkInput =
                TreeBarkMeshGenerator.CalculateInputFingerprint(
                    baseline,
                    barkSettings);

            TreeGenerationOverrides twistOverrides = CloneOverrides(instanceOverrides);
            float changedTwist = ChooseDifferentValue(
                baseline.ResolvedParameters.TrunkSurfaceTorsionDegrees,
                recipe.FamilyProfile.Trunk.SurfaceTorsionDegrees);
            twistOverrides.SetTrunkTwistDegreesForTest(changedTwist);
            TreeGenerationResult twistResult = GenerateInternal(
                recipe, twistOverrides, masterSeed, false);
            passed &= AppendTwistChangeTest(
                report,
                twistResult,
                baseline,
                baselineBarkInput,
                barkSettings);

            TreeGenerationOverrides ridgeOverrides = CloneOverrides(instanceOverrides);
            float changedRidgeDepth = ChooseDifferentValue(
                baseline.ResolvedParameters.TrunkTwistRidgeDepth,
                recipe.FamilyProfile.Trunk.TwistRidgeDepth);
            ridgeOverrides.SetTrunkTwistRidgeDepthForTest(changedRidgeDepth);
            TreeGenerationResult ridgeResult = GenerateInternal(
                recipe, ridgeOverrides, masterSeed, false);
            passed &= AppendBarkOnlyChangeTest(
                report,
                "Trunk-ridge changes preserve structure while changing bark input",
                ridgeResult,
                baseline,
                baselineBarkInput,
                barkSettings);

            TreeGenerationOverrides buttressOverrides = CloneOverrides(instanceOverrides);
            float changedButtress = ChooseDifferentValue(
                baseline.ResolvedParameters.RootButtressStrength,
                recipe.FamilyProfile.Trunk.RootButtressStrength);
            buttressOverrides.SetRootButtressStrengthForTest(changedButtress);
            TreeGenerationResult buttressResult = GenerateInternal(
                recipe, buttressOverrides, masterSeed, false);
            passed &= AppendBarkOnlyChangeTest(
                report,
                "Root-buttress changes preserve structure while changing bark input",
                buttressResult,
                baseline,
                baselineBarkInput,
                barkSettings);

            TreeGenerationOverrides spiralOverrides = CloneOverrides(instanceOverrides);
            float changedSpiral = ChooseDifferentValue(
                baseline.ResolvedParameters.TrunkSpiralStrength,
                recipe.FamilyProfile.Trunk.SpiralStrength);
            spiralOverrides.SetTrunkSpiralStrengthForTest(changedSpiral);
            TreeGenerationResult spiralResult = GenerateInternal(
                recipe, spiralOverrides, masterSeed, false);
            bool spiralInvalidation =
                spiralResult.Passed &&
                spiralResult.Definition.TrunkFingerprint != baseline.TrunkFingerprint &&
                spiralResult.Definition.BranchFingerprint != baseline.BranchFingerprint &&
                spiralResult.Definition.PaletteFingerprint == baseline.PaletteFingerprint;
            passed &= AppendTest(
                report,
                "Trunk-spiral changes invalidate trunk and descendants while preserving palette",
                spiralInvalidation,
                baseline.TrunkFingerprint + " / " + baseline.BranchFingerprint,
                spiralResult.Passed
                    ? spiralResult.Definition.TrunkFingerprint + " / " +
                      spiralResult.Definition.BranchFingerprint
                    : "Generation failed");

            TreeGenerationOverrides barkTintOverrides = CloneOverrides(instanceOverrides);
            Color changedBark = baseline.ResolvedParameters.BarkTint;
            changedBark.g = Mathf.Repeat(changedBark.g + 0.137f, 1f);
            barkTintOverrides.SetBarkTintForTest(changedBark);
            TreeGenerationResult barkTintResult = GenerateInternal(
                recipe, barkTintOverrides, masterSeed, false);
            passed &= AppendIsolationTest(
                report,
                "Bark-tint changes preserve all structure and foliage geometry",
                barkTintResult,
                baseline,
                requireTrunkSame: true,
                requireBranchesSame: true,
                requireFoliageSame: true,
                requirePaletteSame: false);

            passed &= ValidateLockedSeedIsolation(
                report,
                recipe,
                masterSeed,
                baseline.SeedSet);

            report.AppendLine();
            report.Append("Status: ").AppendLine(passed ? "PASS" : "FAIL");
            return report.ToString();
        }

        private static TreeGenerationResult GenerateInternal(
            TreeGenerationRecipe recipe,
            TreeGenerationOverrides instanceOverrides,
            int masterSeed,
            bool includeFullReport)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var failures = new List<string>();
            if (recipe == null)
            {
                failures.Add("No TreeGenerationRecipe was supplied.");
                return CreateFailure(timestamp, failures, includeFullReport);
            }

            recipe.ValidateRecipe(failures);
            if (failures.Count > 0)
            {
                return CreateFailure(timestamp, failures, includeFullReport);
            }

            var context = new GenerationContext
            {
                Recipe = recipe,
                Profile = recipe.FamilyProfile,
                Calibration = recipe.ReferenceCalibration,
                Palette = recipe.ResolvePalette(),
                InstanceOverrides = instanceOverrides ?? new TreeGenerationOverrides(),
                MasterSeed = masterSeed
            };

            context.Seeds = BuildSeedSet(context);
            context.Parameters = ResolveParameters(context, failures);
            ValidateResolvedParameters(context, failures);
            if (failures.Count > 0)
            {
                return CreateFailure(timestamp, failures, includeFullReport);
            }

            var stopwatch = Stopwatch.StartNew();
            GenerateStructure(context);
            FitStructureToReferenceWidthAndDepth(context);
            ReanchorBranchRootsAfterReferenceFit(context);
            stopwatch.Stop();

            ValidateStructure(context, failures);
            TreeGenerationMetrics metrics = CalculateMetrics(
                context,
                stopwatch.Elapsed.Ticks);
            Bounds bounds = CalculateBounds(context.Branches);
            Vector2 footprint = CalculateFootprint(bounds);
            string dependencyHash = CalculateDependencyFingerprint(context);
            string trunkHash = CalculateTrunkFingerprint(context.Branches);
            string branchHash = CalculateBranchFingerprint(context.Branches);
            string foliageHash = CalculateFoliageFingerprint(context);
            string paletteHash = CalculatePaletteFingerprint(context);
            string structuralHash = CalculateStructuralFingerprint(
                trunkHash,
                branchHash);

            if (context.Calibration != null &&
                !metrics.CalibrationWithinTolerance)
            {
                failures.Add(
                    "Generated reference-calibrated bounds exceeded the preset dimension tolerance.");
            }

            bool passed = failures.Count == 0;
            TreeDefinition definition = null;
            if (passed)
            {
                definition = new TreeDefinition();
                definition.Initialize(
                    context.Profile.Family,
                    recipe.StableIdentity,
                    CurrentGeneratorVersion,
                    masterSeed,
                    0,
                    context.Branches,
                    context.FoliageClusters,
                    bounds,
                    footprint,
                    context.Seeds,
                    context.Parameters,
                    metrics,
                    dependencyHash,
                    trunkHash,
                    branchHash,
                    foliageHash,
                    paletteHash,
                    structuralHash,
                    context.Warnings);
            }

            string report = includeFullReport
                ? BuildGenerationReport(
                    timestamp,
                    context,
                    definition,
                    metrics,
                    failures)
                : failures.Count > 0
                    ? BuildFailureSummary(timestamp, failures)
                    : string.Empty;

            return new TreeGenerationResult
            {
                Passed = passed,
                Definition = definition,
                Report = report,
                Timestamp = timestamp
            };
        }

        private static TreeSeedSet BuildSeedSet(GenerationContext context)
        {
            var records = new List<TreeSeedRecord>();
            Array streams = Enum.GetValues(typeof(TreeSeedStream));
            for (int index = 0; index < streams.Length; index++)
            {
                var stream = (TreeSeedStream)streams.GetValue(index);
                bool locked = context.Recipe.TryGetLockedSeed(
                    stream,
                    out int lockedSeed);
                int seed = locked
                    ? lockedSeed
                    : TreeDeterministicUtility.DeriveSeed(
                        context.MasterSeed,
                        context.Profile.Family,
                        context.Profile.StableIdentity,
                        context.Profile.ProfileVersion,
                        context.Calibration != null
                            ? context.Calibration.StableIdentity
                            : string.Empty,
                        context.Calibration != null
                            ? context.Calibration.CalibrationVersion
                            : 0,
                        context.Recipe.StableIdentity,
                        context.Recipe.RecipeVersion,
                        CurrentGeneratorVersion,
                        stream);
                records.Add(new TreeSeedRecord(stream, seed, locked));
            }

            var set = new TreeSeedSet();
            set.SetRecords(records);
            return set;
        }

        private static TreeResolvedParameters ResolveParameters(
            GenerationContext context,
            List<string> failures)
        {
            TreeFamilyProfile profile = context.Profile;
            TreeSeedSet seeds = context.Seeds;
            var parameters = new TreeResolvedParameters
            {
                Family = profile.Family,
                Height = Sample(profile.OverallForm.Height, seeds, TreeSeedStream.TrunkShape, "height"),
                TrunkBaseRadius = Sample(profile.OverallForm.TrunkBaseRadius, seeds, TreeSeedStream.TrunkShape, "trunk-base-radius"),
                CrownStartHeight = Sample(profile.OverallForm.CrownStartHeight, seeds, TreeSeedStream.FoliageClusterPlacement, "crown-start"),
                CrownVolume = Sample(profile.OverallForm.CrownVolume, seeds, TreeSeedStream.FoliageClusterShape, "crown-volume"),
                CrownWidthScale = Sample(profile.OverallForm.CrownWidthScale, seeds, TreeSeedStream.FoliageClusterShape, "crown-width"),
                CrownHeightScale = Sample(profile.OverallForm.CrownHeightScale, seeds, TreeSeedStream.FoliageClusterShape, "crown-height"),
                CrownFill = Sample(profile.OverallForm.CrownFill, seeds, TreeSeedStream.FoliageClusterPlacement, "crown-fill"),
                CrownAsymmetry = Sample(profile.OverallForm.CrownAsymmetry, seeds, TreeSeedStream.FoliageClusterPlacement, "crown-asymmetry"),
                CrownLobeCount = Sample(profile.OverallForm.CrownLobeCount, seeds, TreeSeedStream.FoliageClusterPlacement, "crown-lobes"),
                CrownLobeRadius = Sample(profile.OverallForm.CrownLobeRadius, seeds, TreeSeedStream.FoliageClusterShape, "crown-lobe-radius"),
                TrunkControlPointCount = Sample(profile.Trunk.ControlPointCount, seeds, TreeSeedStream.TrunkShape, "trunk-control-points"),
                TrunkCurvature = Sample(profile.Trunk.Curvature, seeds, TreeSeedStream.TrunkShape, "trunk-curvature"),
                TrunkBendCount = Sample(profile.Trunk.BendCount, seeds, TreeSeedStream.TrunkShape, "trunk-bend-count"),
                TrunkDirectionalDrift = Sample(profile.Trunk.DirectionalDrift, seeds, TreeSeedStream.TrunkShape, "trunk-drift"),
                TrunkLeanStrength = Sample(profile.Trunk.LeanStrength, seeds, TreeSeedStream.TrunkShape, "trunk-lean"),
                TrunkLeanDirectionDegrees = Sample(profile.Trunk.LeanDirectionDegrees, seeds, TreeSeedStream.TrunkShape, "trunk-lean-yaw"),
                TrunkSurfaceTorsionDegrees = Sample(profile.Trunk.SurfaceTorsionDegrees, seeds, TreeSeedStream.TrunkShape, "trunk-surface-torsion"),
                TrunkTwistRidgeCount = Sample(profile.Trunk.TwistRidgeCount, seeds, TreeSeedStream.TrunkShape, "trunk-twist-ridge-count"),
                TrunkTwistRidgeDepth = Sample(profile.Trunk.TwistRidgeDepth, seeds, TreeSeedStream.TrunkShape, "trunk-twist-ridge-depth"),
                RootButtressStrength = Sample(profile.Trunk.RootButtressStrength, seeds, TreeSeedStream.TrunkShape, "root-buttress-strength"),
                RootButtressHeight = Sample(profile.Trunk.RootButtressHeight, seeds, TreeSeedStream.TrunkShape, "root-buttress-height"),
                RootFlareScale = Sample(profile.Trunk.RootFlareScale, seeds, TreeSeedStream.TrunkShape, "root-flare-scale"),
                TrunkSpiralStrength = Sample(profile.Trunk.SpiralStrength, seeds, TreeSeedStream.TrunkShape, "trunk-spiral-strength"),
                TrunkSpiralTurns = Sample(profile.Trunk.SpiralTurns, seeds, TreeSeedStream.TrunkShape, "trunk-spiral-turns"),
                TrunkSpiralDirection = ResolveSpiralDirection(profile.Trunk.SpiralDirection, seeds.GetSeed(TreeSeedStream.TrunkShape), "trunk-spiral-direction"),
                TrunkIrregularity = Sample(profile.Trunk.Irregularity, seeds, TreeSeedStream.TrunkShape, "trunk-irregularity"),
                TrunkTaper = Sample(profile.Trunk.Taper, seeds, TreeSeedStream.TrunkShape, "trunk-taper"),
                TrunkForkProbability = Sample(profile.Trunk.ForkProbability, seeds, TreeSeedStream.TrunkForks, "trunk-fork-probability"),
                TrunkForkHeight = Sample(profile.Trunk.ForkHeight, seeds, TreeSeedStream.TrunkForks, "trunk-fork-height"),
                PrimaryBranchCount = Sample(profile.PrimaryBranches.Count, seeds, TreeSeedStream.PrimaryBranchLayout, "primary-count"),
                PrimaryBranchStartHeight = Sample(profile.PrimaryBranches.StartHeight, seeds, TreeSeedStream.PrimaryBranchLayout, "primary-start-height"),
                PrimaryBranchEndHeight = Sample(profile.PrimaryBranches.EndHeight, seeds, TreeSeedStream.PrimaryBranchLayout, "primary-end-height"),
                InitialBranchElevationDegrees = Sample(profile.PrimaryBranches.InitialElevationDegrees, seeds, TreeSeedStream.PrimaryBranchLayout, "primary-initial-elevation"),
                BranchArchDirection = Sample(profile.PrimaryBranches.ArchDirection, seeds, TreeSeedStream.BranchCurvature, "primary-arch-direction"),
                BranchArchStrength = Sample(profile.PrimaryBranches.ArchStrength, seeds, TreeSeedStream.BranchCurvature, "primary-arch-strength"),
                LateBranchSag = Sample(profile.PrimaryBranches.LateSag, seeds, TreeSeedStream.BranchCurvature, "primary-late-sag"),
                AzimuthSymmetry = Sample(profile.PrimaryBranches.AzimuthSymmetry, seeds, TreeSeedStream.PrimaryBranchLayout, "primary-azimuth-symmetry"),
                DirectionalBiasAngleDegrees = Sample(profile.PrimaryBranches.DirectionalBiasAngleDegrees, seeds, TreeSeedStream.PrimaryBranchLayout, "primary-directional-bias-angle"),
                DirectionalBiasStrength = Sample(profile.PrimaryBranches.DirectionalBiasStrength, seeds, TreeSeedStream.PrimaryBranchLayout, "primary-directional-bias-strength"),
                PrimaryAttachmentMinimum = profile.PrimaryBranches.AttachmentHeight.Minimum,
                PrimaryAttachmentMaximum = profile.PrimaryBranches.AttachmentHeight.Maximum,
                PrimaryBranchCurvature = Sample(profile.PrimaryBranches.Curvature, seeds, TreeSeedStream.BranchCurvature, "primary-curvature"),
                PrimaryBranchSideSweep = Sample(profile.PrimaryBranches.SideSweep, seeds, TreeSeedStream.BranchCurvature, "primary-sweep"),
                PrimaryBranchTwistDegrees = Sample(profile.PrimaryBranches.TwistDegrees, seeds, TreeSeedStream.BranchCurvature, "primary-twist"),
                PrimaryBranchIrregularity = Sample(profile.PrimaryBranches.Irregularity, seeds, TreeSeedStream.BranchCurvature, "primary-irregularity"),
                PrimaryBranchEndCurl = Sample(profile.PrimaryBranches.EndCurl, seeds, TreeSeedStream.BranchCurvature, "primary-end-curl"),
                PrimaryBranchLengthRatio = Sample(profile.PrimaryBranches.LengthRatio, seeds, TreeSeedStream.PrimaryBranchLayout, "primary-length"),
                PrimaryBranchRadiusRatio = Sample(profile.PrimaryBranches.RadiusRatio, seeds, TreeSeedStream.PrimaryBranchLayout, "primary-radius"),
                SecondaryBranchesPerPrimary = Sample(profile.SecondaryBranches.Count, seeds, TreeSeedStream.SecondaryBranchLayout, "secondary-count"),
                TertiaryBranchesPerSecondary = Sample(profile.TertiaryBranches.Count, seeds, TreeSeedStream.TertiaryBranchLayout, "tertiary-count"),
                MaximumBranchOrder = Sample(profile.MaximumBranchOrder, seeds, TreeSeedStream.PrimaryBranchLayout, "max-order"),
                SecondaryLengthRatio = Sample(profile.SecondaryBranches.LengthRatio, seeds, TreeSeedStream.SecondaryBranchLayout, "secondary-length"),
                TertiaryLengthRatio = Sample(profile.TertiaryBranches.LengthRatio, seeds, TreeSeedStream.TertiaryBranchLayout, "tertiary-length"),
                HigherOrderCurvatureScale = Mathf.Max(0.1f, Sample(profile.SecondaryBranches.Curvature, seeds, TreeSeedStream.BranchCurvature, "higher-order-curvature") / Mathf.Max(0.001f, profile.PrimaryBranches.Curvature.Midpoint)),
                ClusterWidthScale = Sample(profile.Foliage.ClusterWidthScale, seeds, TreeSeedStream.FoliageClusterShape, "cluster-width"),
                ClusterHeightScale = Sample(profile.Foliage.ClusterHeightScale, seeds, TreeSeedStream.FoliageClusterShape, "cluster-height"),
                ClusterLengthScale = Sample(profile.Foliage.ClusterLengthScale, seeds, TreeSeedStream.FoliageClusterShape, "cluster-length"),
                ClusterRadialSpread = Sample(profile.Foliage.ClusterRadialSpread, seeds, TreeSeedStream.FoliageClusterShape, "cluster-spread"),
                CardSizeScale = Sample(profile.Foliage.CardSizeScale, seeds, TreeSeedStream.FoliageCardShape, "card-size"),
                FoliageClusterCount = Sample(profile.Foliage.ClusterCount, seeds, TreeSeedStream.FoliageClusterPlacement, "cluster-count"),
                CardsPerCluster = Sample(profile.Foliage.CardsPerCluster, seeds, TreeSeedStream.FoliageCardPlacement, "cards-per-cluster"),
                FoliageEligibility = Sample(profile.Foliage.Eligibility, seeds, TreeSeedStream.FoliageClusterPlacement, "foliage-eligibility"),
                ClusterOccupancy = Sample(profile.Foliage.Occupancy, seeds, TreeSeedStream.FoliageClusterPlacement, "cluster-occupancy"),
                TerminalFoliageProbability = Sample(profile.Foliage.TerminalProbability, seeds, TreeSeedStream.FoliageClusterPlacement, "terminal-foliage"),
                CardRetentionFraction = Sample(profile.Foliage.Retention, seeds, TreeSeedStream.FoliageCardPlacement, "card-retention"),
                MissingBranchProbability = Sample(profile.Damage.MissingBranchProbability, seeds, TreeSeedStream.StructuralDamage, "missing-branch"),
                DeadBranchProbability = Sample(profile.Damage.DeadBranchProbability, seeds, TreeSeedStream.StructuralDamage, "dead-branch"),
                BreakProbability = Sample(profile.Damage.BreakProbability, seeds, TreeSeedStream.StructuralDamage, "break-branch")
            };

            TreeMaterialPalette palette = context.Palette;
            parameters.BarkTint = palette != null ? palette.BarkTint : Color.white;
            parameters.FoliageBaseColor = palette != null ? palette.FoliageBaseColor : Color.white;
            parameters.FoliageHighlightColor = palette != null ? palette.FoliageHighlightColor : Color.white;
            parameters.FoliageShadowColor = palette != null ? palette.FoliageShadowColor : Color.black;
            parameters.AddOwnership("Family profile: " + profile.StableIdentity);

            if (context.Calibration != null)
            {
                ApplyOverrides(
                    parameters,
                    context.Calibration.ParameterOverrides,
                    context.Seeds,
                    "Reference calibration " + context.Calibration.StableIdentity);
            }

            ApplyOverrides(
                parameters,
                context.Recipe.Overrides,
                context.Seeds,
                "Recipe " + context.Recipe.StableIdentity);
            ApplyOverrides(
                parameters,
                context.InstanceOverrides,
                context.Seeds,
                "Instance overrides");

            parameters.PrimaryAttachmentMinimum = parameters.PrimaryBranchStartHeight;
            parameters.PrimaryAttachmentMaximum = parameters.PrimaryBranchEndHeight;

            if (context.Profile.Family == TreeFamily.Dead)
            {
                parameters.FoliageClusterCount = 0;
                parameters.CardsPerCluster = 0;
                parameters.FoliageEligibility = 0f;
                parameters.CardRetentionFraction = 0f;
                parameters.AddOwnership("Dead family suppresses living foliage in the current bark-only generation phase.");
            }

            return parameters;
        }

        private static void ApplyOverrides(
            TreeResolvedParameters p,
            TreeGenerationOverrides o,
            TreeSeedSet seeds,
            string owner)
        {
            if (o == null || !o.HasAnyOverride)
            {
                return;
            }

            int trunkSeed = seeds.GetSeed(TreeSeedStream.TrunkShape);
            int primarySeed = seeds.GetSeed(TreeSeedStream.PrimaryBranchLayout);
            int curvatureSeed = seeds.GetSeed(TreeSeedStream.BranchCurvature);
            int secondarySeed = seeds.GetSeed(TreeSeedStream.SecondaryBranchLayout);
            int foliageShapeSeed = seeds.GetSeed(TreeSeedStream.FoliageClusterShape);
            int foliagePlacementSeed = seeds.GetSeed(TreeSeedStream.FoliageClusterPlacement);
            int cardSeed = seeds.GetSeed(TreeSeedStream.FoliageCardPlacement);
            int damageSeed = seeds.GetSeed(TreeSeedStream.StructuralDamage);

            p.Height = o.Height.Resolve(p.Height, trunkSeed, owner + ".height");
            p.TrunkBaseRadius = o.TrunkBaseRadius.Resolve(p.TrunkBaseRadius, trunkSeed, owner + ".radius");
            p.CrownStartHeight = o.CrownStartHeight.Resolve(p.CrownStartHeight, foliagePlacementSeed, owner + ".crown-start");
            p.CrownVolume = o.CrownVolume.Resolve(p.CrownVolume, foliageShapeSeed, owner + ".crown-volume");
            p.CrownWidthScale = o.CrownWidthScale.Resolve(p.CrownWidthScale, foliageShapeSeed, owner + ".crown-width");
            p.CrownHeightScale = o.CrownHeightScale.Resolve(p.CrownHeightScale, foliageShapeSeed, owner + ".crown-height");
            p.CrownFill = o.CrownFill.Resolve(p.CrownFill, foliagePlacementSeed, owner + ".crown-fill");
            p.CrownAsymmetry = o.CrownAsymmetry.Resolve(p.CrownAsymmetry, foliagePlacementSeed, owner + ".crown-asymmetry");
            p.CrownLobeCount = o.CrownLobeCount.Resolve(p.CrownLobeCount, foliagePlacementSeed, owner + ".crown-lobes");
            p.CrownLobeRadius = o.CrownLobeRadius.Resolve(p.CrownLobeRadius, foliageShapeSeed, owner + ".crown-lobe-radius");
            p.TrunkControlPointCount = o.TrunkControlPointCount.Resolve(p.TrunkControlPointCount, trunkSeed, owner + ".trunk-points");
            p.TrunkCurvature = o.TrunkCurvature.Resolve(p.TrunkCurvature, trunkSeed, owner + ".trunk-curvature");
            p.TrunkBendCount = o.TrunkBendCount.Resolve(p.TrunkBendCount, trunkSeed, owner + ".trunk-bends");
            p.TrunkDirectionalDrift = o.TrunkDirectionalDrift.Resolve(p.TrunkDirectionalDrift, trunkSeed, owner + ".trunk-drift");
            p.TrunkLeanStrength = o.TrunkLeanStrength.Resolve(p.TrunkLeanStrength, trunkSeed, owner + ".trunk-lean");
            p.TrunkLeanDirectionDegrees = o.TrunkLeanDirectionDegrees.Resolve(p.TrunkLeanDirectionDegrees, trunkSeed, owner + ".trunk-lean-yaw");
            p.TrunkSurfaceTorsionDegrees = o.TrunkSurfaceTorsionDegrees.Resolve(p.TrunkSurfaceTorsionDegrees, trunkSeed, owner + ".trunk-surface-torsion");
            p.TrunkTwistRidgeCount = o.TrunkTwistRidgeCount.Resolve(p.TrunkTwistRidgeCount, trunkSeed, owner + ".trunk-twist-ridge-count");
            p.TrunkTwistRidgeDepth = o.TrunkTwistRidgeDepth.Resolve(p.TrunkTwistRidgeDepth, trunkSeed, owner + ".trunk-twist-ridge-depth");
            p.RootButtressStrength = o.RootButtressStrength.Resolve(p.RootButtressStrength, trunkSeed, owner + ".root-buttress-strength");
            p.RootButtressHeight = o.RootButtressHeight.Resolve(p.RootButtressHeight, trunkSeed, owner + ".root-buttress-height");
            p.RootFlareScale = o.RootFlareScale.Resolve(p.RootFlareScale, trunkSeed, owner + ".root-flare-scale");
            p.TrunkSpiralStrength = o.TrunkSpiralStrength.Resolve(p.TrunkSpiralStrength, trunkSeed, owner + ".trunk-spiral-strength");
            p.TrunkSpiralTurns = o.TrunkSpiralTurns.Resolve(p.TrunkSpiralTurns, trunkSeed, owner + ".trunk-spiral-turns");
            p.TrunkSpiralDirection = ResolveOverrideDirection(o.TrunkSpiralDirection, p.TrunkSpiralDirection, trunkSeed, owner + ".trunk-spiral-direction");
            p.TrunkIrregularity = o.TrunkIrregularity.Resolve(p.TrunkIrregularity, trunkSeed, owner + ".trunk-irregularity");
            p.TrunkTaper = o.TrunkTaper.Resolve(p.TrunkTaper, trunkSeed, owner + ".trunk-taper");
            p.TrunkForkProbability = o.TrunkForkProbability.Resolve(p.TrunkForkProbability, trunkSeed, owner + ".trunk-fork-probability");
            p.TrunkForkHeight = o.TrunkForkHeight.Resolve(p.TrunkForkHeight, trunkSeed, owner + ".trunk-fork-height");
            p.PrimaryBranchCount = o.PrimaryBranchCount.Resolve(p.PrimaryBranchCount, primarySeed, owner + ".primary-count");
            p.PrimaryBranchStartHeight = o.PrimaryBranchStartHeight.Resolve(p.PrimaryBranchStartHeight, primarySeed, owner + ".primary-start-height");
            p.PrimaryBranchEndHeight = o.PrimaryBranchEndHeight.Resolve(p.PrimaryBranchEndHeight, primarySeed, owner + ".primary-end-height");
            p.InitialBranchElevationDegrees = o.InitialBranchElevationDegrees.Resolve(p.InitialBranchElevationDegrees, primarySeed, owner + ".primary-initial-elevation");
            p.BranchArchDirection = o.BranchArchDirection.Resolve(p.BranchArchDirection, curvatureSeed, owner + ".primary-arch-direction");
            p.BranchArchStrength = o.BranchArchStrength.Resolve(p.BranchArchStrength, curvatureSeed, owner + ".primary-arch-strength");
            p.LateBranchSag = o.LateBranchSag.Resolve(p.LateBranchSag, curvatureSeed, owner + ".primary-late-sag");
            p.AzimuthSymmetry = o.AzimuthSymmetry.Resolve(p.AzimuthSymmetry, primarySeed, owner + ".primary-azimuth-symmetry");
            p.DirectionalBiasAngleDegrees = o.DirectionalBiasAngleDegrees.Resolve(p.DirectionalBiasAngleDegrees, primarySeed, owner + ".primary-directional-bias-angle");
            p.DirectionalBiasStrength = o.DirectionalBiasStrength.Resolve(p.DirectionalBiasStrength, primarySeed, owner + ".primary-directional-bias-strength");
            p.PrimaryBranchCurvature = o.PrimaryBranchCurvature.Resolve(p.PrimaryBranchCurvature, curvatureSeed, owner + ".primary-curvature");
            p.PrimaryBranchSideSweep = o.PrimaryBranchSideSweep.Resolve(p.PrimaryBranchSideSweep, curvatureSeed, owner + ".primary-sweep");
            p.PrimaryBranchTwistDegrees = o.PrimaryBranchTwistDegrees.Resolve(p.PrimaryBranchTwistDegrees, curvatureSeed, owner + ".primary-twist");
            p.PrimaryBranchIrregularity = o.PrimaryBranchIrregularity.Resolve(p.PrimaryBranchIrregularity, curvatureSeed, owner + ".primary-irregularity");
            p.PrimaryBranchEndCurl = o.PrimaryBranchEndCurl.Resolve(p.PrimaryBranchEndCurl, curvatureSeed, owner + ".primary-end-curl");
            p.PrimaryBranchLengthRatio = o.PrimaryBranchLengthRatio.Resolve(p.PrimaryBranchLengthRatio, primarySeed, owner + ".primary-length");
            p.PrimaryBranchRadiusRatio = o.PrimaryBranchRadiusRatio.Resolve(p.PrimaryBranchRadiusRatio, primarySeed, owner + ".primary-radius");
            p.SecondaryBranchesPerPrimary = o.SecondaryBranchesPerPrimary.Resolve(p.SecondaryBranchesPerPrimary, secondarySeed, owner + ".secondary-count");
            p.TertiaryBranchesPerSecondary = o.TertiaryBranchesPerSecondary.Resolve(p.TertiaryBranchesPerSecondary, secondarySeed, owner + ".tertiary-count");
            p.MaximumBranchOrder = o.MaximumBranchOrder.Resolve(p.MaximumBranchOrder, primarySeed, owner + ".max-order");
            p.SecondaryLengthRatio = o.SecondaryLengthRatio.Resolve(p.SecondaryLengthRatio, secondarySeed, owner + ".secondary-length");
            p.TertiaryLengthRatio = o.TertiaryLengthRatio.Resolve(p.TertiaryLengthRatio, secondarySeed, owner + ".tertiary-length");
            p.HigherOrderCurvatureScale = o.HigherOrderCurvatureScale.Resolve(p.HigherOrderCurvatureScale, secondarySeed, owner + ".higher-curvature");
            p.ClusterWidthScale = o.ClusterWidthScale.Resolve(p.ClusterWidthScale, foliageShapeSeed, owner + ".cluster-width");
            p.ClusterHeightScale = o.ClusterHeightScale.Resolve(p.ClusterHeightScale, foliageShapeSeed, owner + ".cluster-height");
            p.ClusterLengthScale = o.ClusterLengthScale.Resolve(p.ClusterLengthScale, foliageShapeSeed, owner + ".cluster-length");
            p.ClusterRadialSpread = o.ClusterRadialSpread.Resolve(p.ClusterRadialSpread, foliageShapeSeed, owner + ".cluster-spread");
            p.CardSizeScale = o.CardSizeScale.Resolve(p.CardSizeScale, cardSeed, owner + ".card-size");
            p.FoliageClusterCount = o.FoliageClusterCount.Resolve(p.FoliageClusterCount, foliagePlacementSeed, owner + ".cluster-count");
            p.CardsPerCluster = o.CardsPerCluster.Resolve(p.CardsPerCluster, cardSeed, owner + ".cards-per-cluster");
            p.FoliageEligibility = o.FoliageEligibility.Resolve(p.FoliageEligibility, foliagePlacementSeed, owner + ".eligibility");
            p.ClusterOccupancy = o.ClusterOccupancy.Resolve(p.ClusterOccupancy, foliagePlacementSeed, owner + ".occupancy");
            p.TerminalFoliageProbability = o.TerminalFoliageProbability.Resolve(p.TerminalFoliageProbability, foliagePlacementSeed, owner + ".terminal-foliage");
            p.CardRetentionFraction = o.CardRetentionFraction.Resolve(p.CardRetentionFraction, cardSeed, owner + ".retention");
            p.MissingBranchProbability = o.MissingBranchProbability.Resolve(p.MissingBranchProbability, damageSeed, owner + ".missing");
            p.DeadBranchProbability = o.DeadBranchProbability.Resolve(p.DeadBranchProbability, damageSeed, owner + ".dead");
            p.BreakProbability = o.BreakProbability.Resolve(p.BreakProbability, damageSeed, owner + ".break");
            p.BarkTint = o.BarkTint.Resolve(p.BarkTint);
            p.FoliageBaseColor = o.FoliageBaseColor.Resolve(p.FoliageBaseColor);
            p.FoliageHighlightColor = o.FoliageHighlightColor.Resolve(p.FoliageHighlightColor);
            p.FoliageShadowColor = o.FoliageShadowColor.Resolve(p.FoliageShadowColor);
            p.AddOwnership(owner + " supplied explicit/ranged parameter overrides.");
        }

        private static void ValidateResolvedParameters(
            GenerationContext context,
            List<string> failures)
        {
            TreeResolvedParameters p = context.Parameters;
            TreeFamilyProfile profile = context.Profile;
            ValidateInside(p.Height, profile.OverallForm.Height, "Height", failures);
            ValidateInside(p.TrunkBaseRadius, profile.OverallForm.TrunkBaseRadius, "Trunk base radius", failures);
            ValidateInside(p.CrownStartHeight, profile.OverallForm.CrownStartHeight, "Crown start height", failures);
            ValidateInside(p.CrownVolume, profile.OverallForm.CrownVolume, "Crown volume", failures);
            ValidateInside(p.CrownWidthScale, profile.OverallForm.CrownWidthScale, "Crown width scale", failures);
            ValidateInside(p.CrownHeightScale, profile.OverallForm.CrownHeightScale, "Crown height scale", failures);
            ValidateInside(p.CrownFill, profile.OverallForm.CrownFill, "Crown fill", failures);
            ValidateInside(p.CrownAsymmetry, profile.OverallForm.CrownAsymmetry, "Crown asymmetry", failures);
            ValidateInside(p.CrownLobeCount, profile.OverallForm.CrownLobeCount, "Crown lobe count", failures);
            ValidateInside(p.CrownLobeRadius, profile.OverallForm.CrownLobeRadius, "Crown lobe radius", failures);
            ValidateInside(p.TrunkControlPointCount, profile.Trunk.ControlPointCount, "Trunk control-point count", failures);
            ValidateInside(p.TrunkCurvature, profile.Trunk.Curvature, "Trunk curvature", failures);
            ValidateInside(p.TrunkBendCount, profile.Trunk.BendCount, "Trunk bend count", failures);
            ValidateInside(p.TrunkDirectionalDrift, profile.Trunk.DirectionalDrift, "Trunk directional drift", failures);
            ValidateInside(p.TrunkLeanStrength, profile.Trunk.LeanStrength, "Trunk lean strength", failures);
            ValidateInside(p.TrunkLeanDirectionDegrees, profile.Trunk.LeanDirectionDegrees, "Trunk lean direction", failures);
            ValidateInside(p.TrunkSurfaceTorsionDegrees, profile.Trunk.SurfaceTorsionDegrees, "Trunk twist degrees", failures);
            ValidateInside(p.TrunkTwistRidgeCount, profile.Trunk.TwistRidgeCount, "Trunk twist ridge count", failures);
            ValidateInside(p.TrunkTwistRidgeDepth, profile.Trunk.TwistRidgeDepth, "Trunk twist ridge depth", failures);
            ValidateInside(p.RootButtressStrength, profile.Trunk.RootButtressStrength, "Root buttress strength", failures);
            ValidateInside(p.RootButtressHeight, profile.Trunk.RootButtressHeight, "Root buttress height", failures);
            ValidateInside(p.RootFlareScale, profile.Trunk.RootFlareScale, "Root flare scale", failures);
            ValidateInside(p.TrunkSpiralStrength, profile.Trunk.SpiralStrength, "Trunk path spiral strength", failures);
            ValidateInside(p.TrunkSpiralTurns, profile.Trunk.SpiralTurns, "Trunk spiral turns", failures);
            if (Mathf.Abs(p.TrunkSpiralDirection) < 0.5f)
            {
                failures.Add("Trunk spiral direction must resolve to clockwise or counter-clockwise.");
            }
            ValidateInside(p.TrunkIrregularity, profile.Trunk.Irregularity, "Trunk irregularity", failures);
            ValidateInside(p.TrunkTaper, profile.Trunk.Taper, "Trunk taper", failures);
            ValidateInside(p.TrunkForkProbability, profile.Trunk.ForkProbability, "Trunk fork probability", failures);
            ValidateInside(p.TrunkForkHeight, profile.Trunk.ForkHeight, "Trunk fork height", failures);
            ValidateInside(p.PrimaryBranchCount, profile.PrimaryBranches.Count, "Primary branch count", failures);
            ValidateInside(p.PrimaryBranchStartHeight, profile.PrimaryBranches.StartHeight, "Primary branch start height", failures);
            ValidateInside(p.PrimaryBranchEndHeight, profile.PrimaryBranches.EndHeight, "Primary branch end height", failures);
            ValidateInside(p.InitialBranchElevationDegrees, profile.PrimaryBranches.InitialElevationDegrees, "Initial branch elevation", failures);
            ValidateInside(p.BranchArchDirection, profile.PrimaryBranches.ArchDirection, "Branch arch direction", failures);
            ValidateInside(p.BranchArchStrength, profile.PrimaryBranches.ArchStrength, "Branch arch strength", failures);
            ValidateInside(p.LateBranchSag, profile.PrimaryBranches.LateSag, "Late branch sag", failures);
            ValidateInside(p.AzimuthSymmetry, profile.PrimaryBranches.AzimuthSymmetry, "Azimuth symmetry", failures);
            ValidateInside(p.DirectionalBiasAngleDegrees, profile.PrimaryBranches.DirectionalBiasAngleDegrees, "Directional bias angle", failures);
            ValidateInside(p.DirectionalBiasStrength, profile.PrimaryBranches.DirectionalBiasStrength, "Directional bias strength", failures);
            ValidateInside(p.PrimaryBranchCurvature, profile.PrimaryBranches.Curvature, "Primary branch curvature", failures);
            ValidateInside(p.PrimaryBranchSideSweep, profile.PrimaryBranches.SideSweep, "Primary branch side sweep", failures);
            ValidateInside(p.PrimaryBranchTwistDegrees, profile.PrimaryBranches.TwistDegrees, "Primary branch twist", failures);
            ValidateInside(p.PrimaryBranchIrregularity, profile.PrimaryBranches.Irregularity, "Primary branch irregularity", failures);
            ValidateInside(p.PrimaryBranchEndCurl, profile.PrimaryBranches.EndCurl, "Primary branch end curl", failures);
            ValidateInside(p.PrimaryBranchLengthRatio, profile.PrimaryBranches.LengthRatio, "Primary branch length ratio", failures);
            ValidateInside(p.PrimaryBranchRadiusRatio, profile.PrimaryBranches.RadiusRatio, "Primary branch radius ratio", failures);
            ValidateInside(p.SecondaryBranchesPerPrimary, profile.SecondaryBranches.Count, "Secondary branches per primary", failures);
            ValidateInside(p.TertiaryBranchesPerSecondary, profile.TertiaryBranches.Count, "Tertiary branches per secondary", failures);
            ValidateInside(p.MaximumBranchOrder, profile.MaximumBranchOrder, "Maximum branch order", failures);
            ValidateInside(p.SecondaryLengthRatio, profile.SecondaryBranches.LengthRatio, "Secondary length ratio", failures);
            ValidateInside(p.TertiaryLengthRatio, profile.TertiaryBranches.LengthRatio, "Tertiary length ratio", failures);
            if (!TreeDeterministicUtility.IsFinite(p.HigherOrderCurvatureScale) || p.HigherOrderCurvatureScale <= 0f)
            {
                failures.Add("Higher-order curvature scale must be finite and positive.");
            }
            ValidateInside(p.ClusterWidthScale, profile.Foliage.ClusterWidthScale, "Cluster width scale", failures);
            ValidateInside(p.ClusterHeightScale, profile.Foliage.ClusterHeightScale, "Cluster height scale", failures);
            ValidateInside(p.ClusterLengthScale, profile.Foliage.ClusterLengthScale, "Cluster length scale", failures);
            ValidateInside(p.ClusterRadialSpread, profile.Foliage.ClusterRadialSpread, "Cluster radial spread", failures);
            ValidateInside(p.CardSizeScale, profile.Foliage.CardSizeScale, "Card size scale", failures);
            ValidateInside(p.FoliageClusterCount, profile.Foliage.ClusterCount, "Foliage cluster count", failures);
            ValidateInside(p.CardsPerCluster, profile.Foliage.CardsPerCluster, "Cards per cluster", failures);
            ValidateInside(p.FoliageEligibility, profile.Foliage.Eligibility, "Foliage eligibility", failures);
            ValidateInside(p.ClusterOccupancy, profile.Foliage.Occupancy, "Cluster occupancy", failures);
            ValidateInside(p.TerminalFoliageProbability, profile.Foliage.TerminalProbability, "Terminal foliage probability", failures);
            ValidateInside(p.CardRetentionFraction, profile.Foliage.Retention, "Card retention fraction", failures);
            ValidateProbability(p.MissingBranchProbability, "Missing branch probability", failures);
            ValidateProbability(p.DeadBranchProbability, "Dead branch probability", failures);
            ValidateProbability(p.BreakProbability, "Break probability", failures);
            if (!IsFiniteColor(p.BarkTint) ||
                !IsFiniteColor(p.FoliageBaseColor) ||
                !IsFiniteColor(p.FoliageHighlightColor) ||
                !IsFiniteColor(p.FoliageShadowColor))
            {
                failures.Add("Resolved bark/foliage colours must be finite.");
            }

            if (p.TrunkControlPointCount < 3)
            {
                failures.Add("Resolved trunk control-point count is below 3.");
            }

            if (p.PrimaryBranchStartHeight < 0f ||
                p.PrimaryBranchEndHeight > 1f ||
                p.PrimaryBranchEndHeight < p.PrimaryBranchStartHeight ||
                p.PrimaryAttachmentMaximum < p.PrimaryAttachmentMinimum)
            {
                failures.Add("Resolved primary attachment-height interval is invalid.");
            }

            if (p.SecondaryBranchesPerPrimary < 0 ||
                p.TertiaryBranchesPerSecondary < 0)
            {
                failures.Add("Resolved higher-order branch counts cannot be negative.");
            }
        }

        private static void GenerateStructure(GenerationContext context)
        {
            CreateTrunk(context);
            CreatePrimaryBranches(context);
            if (context.Parameters.MaximumBranchOrder >= 2)
            {
                CreateHigherOrderBranches(context, 1, 2);
            }

            if (context.Parameters.MaximumBranchOrder >= 3)
            {
                CreateHigherOrderBranches(context, 2, 3);
            }
        }

        private static void CreateTrunk(GenerationContext context)
        {
            TreeResolvedParameters p = context.Parameters;
            int seed = context.Seeds.GetSeed(TreeSeedStream.TrunkShape);
            int pointCount = Mathf.Max(3, p.TrunkControlPointCount);
            var points = new List<Vector3>(pointCount);
            Vector2 leanDirection = DirectionFromDegrees(p.TrunkLeanDirectionDegrees);
            Vector2 primaryCurveDirection = TreeDeterministicUtility.DirectionXZ(seed, "trunk-primary-curve");
            Vector2 secondaryCurveDirection = new Vector2(-primaryCurveDirection.y, primaryCurveDirection.x);
            float spiralPhase = TreeDeterministicUtility.Sample01(seed, "trunk-spiral-phase") * Mathf.PI * 2f;
            Vector2 drift = Vector2.zero;

            for (int index = 0; index < pointCount; index++)
            {
                float t = index / (float)(pointCount - 1);
                float y = p.Height * t;
                float bendPhase = t * Mathf.PI * 2f * p.TrunkBendCount;
                float bendEnvelope = Mathf.Sin(Mathf.PI * t);
                float curveAmplitude = p.TrunkCurvature * p.Height * 0.12f;
                Vector2 curve =
                    primaryCurveDirection * Mathf.Sin(bendPhase) * curveAmplitude * bendEnvelope +
                    secondaryCurveDirection * Mathf.Sin(bendPhase * 0.53f + 1.17f) *
                    curveAmplitude * 0.45f * bendEnvelope;
                Vector2 jitterDirection = TreeDeterministicUtility.DirectionXZ(
                    seed,
                    "trunk-jitter-" + index);
                drift += jitterDirection *
                    p.TrunkDirectionalDrift * p.Height /
                    Mathf.Max(1, pointCount - 1) * t;
                Vector2 irregularity = jitterDirection *
                    p.TrunkIrregularity * p.Height * 0.035f * bendEnvelope;
                Vector2 lean = leanDirection *
                    p.TrunkLeanStrength * p.Height * Mathf.Pow(t, 1.35f);
                float spiralEnvelope = Mathf.SmoothStep(0f, 1f, t) *
                    Mathf.Lerp(0.7f, 1f, t);
                float spiralAngle = spiralPhase +
                    p.TrunkSpiralDirection * TwoPi * p.TrunkSpiralTurns * t;
                float spiralRadius = p.TrunkSpiralStrength * p.Height * 0.35f * spiralEnvelope;
                Vector2 spiral = new Vector2(
                    Mathf.Cos(spiralAngle),
                    Mathf.Sin(spiralAngle)) * spiralRadius;
                points.Add(new Vector3(
                    curve.x + drift.x + irregularity.x + lean.x + spiral.x,
                    y,
                    curve.y + drift.y + irregularity.y + lean.y + spiral.y));
            }

            ConstrainTrunkControlPoints(context, points);

            float endRadius = Mathf.Max(
                context.Profile.MinimumBranchRadius,
                p.TrunkBaseRadius * (1f - p.TrunkTaper));
            List<TreeCurveSample> samples = BuildCurveSamples(
                points,
                p.TrunkBaseRadius,
                endRadius,
                p.TrunkSurfaceTorsionDegrees,
                context.Profile.MaximumSamplesPerBranch,
                context.Warnings,
                "Trunk");
            var trunk = new TreeBranchDefinition();
            trunk.Initialize(
                StableBranchId(context, -1, 0, 0),
                -1,
                0,
                0f,
                Vector3.right,
                p.TrunkBaseRadius,
                endRadius,
                1f,
                TreeDeterministicUtility.Sample01(seed, "trunk-phase"),
                TreeBranchState.None,
                p.CrownStartHeight,
                1f,
                points,
                samples);
            context.Branches.Add(trunk);
        }

        private static void CreatePrimaryBranches(GenerationContext context)
        {
            TreeResolvedParameters p = context.Parameters;
            int layoutSeed = context.Seeds.GetSeed(TreeSeedStream.PrimaryBranchLayout);
            int damageSeed = context.Seeds.GetSeed(TreeSeedStream.StructuralDamage);
            int requestedCount = Mathf.Max(0, p.PrimaryBranchCount);
            int tiers = context.Profile.PrimaryBranches.TierCount.Sample(
                layoutSeed,
                "resolved-tier-count");
            int perTier = context.Profile.PrimaryBranches.BranchesPerTier.Sample(
                layoutSeed,
                "resolved-branches-per-tier");
            if (tiers > 0 && perTier > 0)
            {
                tiers = Mathf.Max(
                    tiers,
                    Mathf.CeilToInt(requestedCount / (float)perTier));
            }

            for (int index = 0; index < requestedCount; index++)
            {
                if (context.Branches.Count >= context.Profile.MaximumBranchCount)
                {
                    context.RejectedBranches += requestedCount - index;
                    context.Warnings.Add("Maximum branch-count budget stopped primary generation.");
                    break;
                }

                int branchId = StableBranchId(context, 0, 1, index);
                if (ShouldRejectBranch(context, damageSeed, branchId))
                {
                    context.RejectedBranches++;
                    continue;
                }

                float attachment = ResolvePrimaryAttachment(
                    context,
                    index,
                    requestedCount,
                    tiers,
                    perTier,
                    layoutSeed);
                float yaw = ResolvePrimaryYaw(
                    context,
                    index,
                    tiers,
                    perTier,
                    layoutSeed);
                ParentFrame frame = EvaluateBranchFrame(
                    context.Branches[0],
                    attachment);
                float lowerWeight = 1f - attachment;
                float length = p.Height * p.PrimaryBranchLengthRatio *
                    Mathf.Lerp(0.72f, 1.18f, lowerWeight) *
                    Mathf.Lerp(
                        0.86f,
                        1.14f,
                        TreeDeterministicUtility.Sample01(
                            layoutSeed,
                            "primary-length-" + index));
                float radius = Mathf.Max(
                    context.Profile.MinimumBranchRadius,
                    frame.Radius * p.PrimaryBranchRadiusRatio);
                TreeBranchState state = ResolveBranchState(
                    p,
                    damageSeed,
                    branchId);
                if ((state & TreeBranchState.Broken) != 0)
                {
                    length *= Mathf.Lerp(
                        0.42f,
                        0.78f,
                        TreeDeterministicUtility.Sample01(
                            damageSeed,
                            "break-length-" + branchId));
                }

                Vector3 direction = ResolvePrimaryDirection(
                    context,
                    frame,
                    yaw,
                    index);
                TreeBranchDefinition branch = CreateBranch(
                    context,
                    branchId,
                    0,
                    1,
                    attachment,
                    frame,
                    direction,
                    length,
                    radius,
                    Mathf.Max(
                        context.Profile.MinimumBranchRadius,
                        radius * 0.16f),
                    p.PrimaryBranchCurvature,
                    p.BranchArchDirection,
                    p.BranchArchStrength,
                    p.LateBranchSag,
                    p.PrimaryBranchSideSweep,
                    p.PrimaryBranchIrregularity,
                    p.PrimaryBranchEndCurl,
                    p.PrimaryBranchTwistDegrees,
                    state,
                    0.42f,
                    1f,
                    "primary-" + index);
                if (branch != null)
                {
                    context.Branches.Add(branch);
                }
            }

            if (p.TrunkForkProbability > 0f &&
                TreeDeterministicUtility.Sample01(
                    context.Seeds.GetSeed(TreeSeedStream.TrunkForks),
                    "trunk-fork-accept") < p.TrunkForkProbability &&
                context.Branches.Count < context.Profile.MaximumBranchCount)
            {
                CreateForkBranch(context);
            }
        }

        private static void CreateForkBranch(GenerationContext context)
        {
            TreeResolvedParameters p = context.Parameters;
            int forkSeed = context.Seeds.GetSeed(TreeSeedStream.TrunkForks);
            int branchId = StableBranchId(context, 0, 1, 100000);
            float attachment = p.TrunkForkHeight;
            ParentFrame frame = EvaluateBranchFrame(context.Branches[0], attachment);
            float yaw = TreeDeterministicUtility.Sample01(forkSeed, "fork-yaw") * 360f;
            float yawRadians = yaw * Mathf.Deg2Rad;
            Vector3 radial = (
                frame.Normal * Mathf.Cos(yawRadians) +
                frame.Binormal * Mathf.Sin(yawRadians)).normalized;
            Vector3 direction = (
                radial * 0.45f +
                frame.Tangent * 0.89f).normalized;
            float length = p.Height * (1f - attachment) * 0.9f;
            float radius = Mathf.Max(
                context.Profile.MinimumBranchRadius,
                frame.Radius * 0.72f);
            TreeBranchDefinition branch = CreateBranch(
                context,
                branchId,
                0,
                1,
                attachment,
                frame,
                direction,
                length,
                radius,
                Mathf.Max(context.Profile.MinimumBranchRadius, radius * 0.22f),
                p.TrunkCurvature * 0.7f,
                p.BranchArchDirection * 0.5f,
                p.BranchArchStrength * 0.45f,
                p.LateBranchSag * 0.25f,
                p.PrimaryBranchSideSweep * 0.5f,
                p.TrunkIrregularity,
                0.05f,
                p.TrunkSurfaceTorsionDegrees * 0.5f,
                TreeBranchState.None,
                0.3f,
                1f,
                "trunk-fork");
            if (branch != null)
            {
                context.Branches.Add(branch);
            }
        }

        private static void CreateHigherOrderBranches(
            GenerationContext context,
            int parentOrder,
            int childOrder)
        {
            TreeResolvedParameters p = context.Parameters;
            int layoutSeed = context.Seeds.GetSeed(
                childOrder == 2
                    ? TreeSeedStream.SecondaryBranchLayout
                    : TreeSeedStream.TertiaryBranchLayout);
            int damageSeed = context.Seeds.GetSeed(TreeSeedStream.StructuralDamage);
            int childCount = childOrder == 2
                ? p.SecondaryBranchesPerPrimary
                : p.TertiaryBranchesPerSecondary;
            float lengthRatio = childOrder == 2
                ? p.SecondaryLengthRatio
                : p.TertiaryLengthRatio;
            int initialCount = context.Branches.Count;

            for (int parentIndex = 1; parentIndex < initialCount; parentIndex++)
            {
                TreeBranchDefinition parent = context.Branches[parentIndex];
                if (parent.BranchOrder != parentOrder || parent.IsBroken)
                {
                    continue;
                }

                float parentLength = CalculateBranchLength(parent);
                for (int childIndex = 0; childIndex < childCount; childIndex++)
                {
                    if (context.Branches.Count >= context.Profile.MaximumBranchCount)
                    {
                        context.RejectedBranches++;
                        context.Warnings.Add(
                            "Maximum branch-count budget stopped order " + childOrder + " generation.");
                        return;
                    }

                    int branchId = StableBranchId(
                        context,
                        parent.StableBranchId,
                        childOrder,
                        childIndex);
                    if (ShouldRejectBranch(context, damageSeed, branchId))
                    {
                        context.RejectedBranches++;
                        continue;
                    }

                    float survivalProbability = childOrder == 2
                        ? context.Profile.StructuralConstraints.SecondarySurvivalProbability
                        : context.Profile.StructuralConstraints.TertiarySurvivalProbability;
                    if (TreeDeterministicUtility.Sample01(
                            layoutSeed,
                            "higher-survival-" + branchId) >= survivalProbability)
                    {
                        context.RejectedBranches++;
                        continue;
                    }

                    float attachment = Mathf.Lerp(
                        0.28f,
                        0.92f,
                        (childIndex + 1f) / (childCount + 1f));
                    attachment += TreeDeterministicUtility.SampleSigned(
                        layoutSeed,
                        "higher-attachment-" + branchId) * 0.06f;
                    attachment = Mathf.Clamp(attachment, 0.15f, 0.96f);
                    ParentFrame frame = EvaluateBranchFrame(parent, attachment);
                    float yaw =
                        childIndex * GoldenAngleDegrees +
                        TreeDeterministicUtility.SampleSigned(
                            layoutSeed,
                            "higher-yaw-" + branchId) * 28f;
                    float radians = yaw * Mathf.Deg2Rad;
                    Vector3 radial = (
                        frame.Normal * Mathf.Cos(radians) +
                        frame.Binormal * Mathf.Sin(radians)).normalized;
                    float upward = Mathf.Lerp(
                        0.05f,
                        0.34f,
                        TreeDeterministicUtility.Sample01(
                            layoutSeed,
                            "higher-upward-" + branchId));
                    float parentInheritance = childOrder == 2 ? 0.34f : 0.48f;
                    Vector3 direction = (
                        radial * (1f - parentInheritance) +
                        frame.Tangent * parentInheritance +
                        Vector3.up * upward).normalized;
                    float length = parentLength * lengthRatio * Mathf.Lerp(
                        0.78f,
                        1.16f,
                        TreeDeterministicUtility.Sample01(
                            layoutSeed,
                            "higher-length-" + branchId));
                    float radius = Mathf.Max(
                        context.Profile.MinimumBranchRadius,
                        frame.Radius * (childOrder == 2 ? 0.42f : 0.36f));
                    TreeBranchState state = ResolveBranchState(
                        p,
                        damageSeed,
                        branchId);
                    if ((state & TreeBranchState.Broken) != 0)
                    {
                        length *= 0.58f;
                    }

                    TreeBranchDefinition branch = CreateBranch(
                        context,
                        branchId,
                        parentIndex,
                        childOrder,
                        attachment,
                        frame,
                        direction,
                        length,
                        radius,
                        Mathf.Max(
                            context.Profile.MinimumBranchRadius,
                            radius * 0.14f),
                        p.PrimaryBranchCurvature *
                            p.HigherOrderCurvatureScale *
                            (childOrder == 2 ? 0.72f : 0.52f),
                        p.BranchArchDirection,
                        p.BranchArchStrength * (childOrder == 2 ? 0.55f : 0.35f),
                        p.LateBranchSag * (childOrder == 2 ? 0.65f : 0.45f),
                        p.PrimaryBranchSideSweep * (childOrder == 2 ? 0.72f : 0.5f),
                        p.PrimaryBranchIrregularity * (childOrder == 2 ? 0.72f : 0.5f),
                        p.PrimaryBranchEndCurl * (childOrder == 2 ? 0.65f : 0.42f),
                        p.PrimaryBranchTwistDegrees * (childOrder == 2 ? 0.7f : 0.45f),
                        state,
                        0.25f,
                        1f,
                        "order-" + childOrder + "-" + branchId);
                    if (branch != null)
                    {
                        context.Branches.Add(branch);
                    }
                }
            }
        }

        private static TreeBranchDefinition CreateBranch(
            GenerationContext context,
            int stableId,
            int parentIndex,
            int order,
            float attachment,
            ParentFrame parentFrame,
            Vector3 initialDirection,
            float length,
            float startRadius,
            float endRadius,
            float curvature,
            float archDirection,
            float archStrength,
            float lateSag,
            float sideSweep,
            float irregularity,
            float endCurl,
            float twistDegrees,
            TreeBranchState state,
            float foliageStart,
            float foliageEnd,
            string key)
        {
            length = Mathf.Max(
                context.Profile.MinimumBranchLength * 1.05f,
                length);
            int curveSeed = context.Seeds.GetSeed(TreeSeedStream.BranchCurvature);
            int pointCount = Mathf.Clamp(4 + order, 4, 7);
            var controlPoints = new List<Vector3>(pointCount);
            Vector3 direction = initialDirection.sqrMagnitude > Epsilon
                ? initialDirection.normalized
                : parentFrame.Tangent;
            Vector3 rootPosition = CalculateEmbeddedBranchRoot(
                parentFrame,
                direction,
                startRadius,
                context,
                stableId);
            Vector3 sideAxis = Vector3.Cross(direction, parentFrame.Tangent);
            if (sideAxis.sqrMagnitude < Epsilon)
            {
                sideAxis = parentFrame.Binormal;
            }
            sideAxis.Normalize();
            Vector3 curveAxis = Vector3.Cross(sideAxis, direction).normalized;
            float curvePhase = TreeDeterministicUtility.Sample01(
                curveSeed,
                key + "-curve-phase") * Mathf.PI * 2f;

            for (int index = 0; index < pointCount; index++)
            {
                float t = index / (float)(pointCount - 1);
                Vector3 position = rootPosition + direction * length * t;
                float envelope = Mathf.Sin(Mathf.PI * t);
                float curveWave = Mathf.Sin(t * Mathf.PI * 1.35f + curvePhase);
                position += curveAxis * curveWave * curvature * length * 0.22f * envelope;
                float archEnvelope = 4f * t * (1f - t);
                position += parentFrame.Tangent *
                    archDirection * archStrength * length * archEnvelope;
                position += sideAxis * sideSweep * length * Mathf.Sin(Mathf.PI * t);
                float sagT = Mathf.Clamp01((t - 0.45f) / 0.55f);
                sagT = sagT * sagT * (3f - 2f * sagT);
                position += Vector3.down * lateSag * length * sagT * sagT;
                position += Vector3.up * endCurl * length * t * t * t;
                Vector2 jitter = TreeDeterministicUtility.DirectionXZ(
                    curveSeed,
                    key + "-jitter-" + index);
                position += new Vector3(jitter.x, 0f, jitter.y) *
                    irregularity * length * 0.045f * envelope;
                controlPoints.Add(position);
            }

            if ((state & TreeBranchState.Broken) != 0 && controlPoints.Count > 2)
            {
                controlPoints.RemoveRange(
                    Mathf.Max(2, controlPoints.Count - 2),
                    Mathf.Min(2, controlPoints.Count - 2));
            }

            ConstrainBranchControlPoints(
                context,
                controlPoints,
                rootPosition,
                direction,
                order,
                length);

            RemoveCollapsedControlPoints(
                controlPoints,
                context.Profile.MinimumBranchLength * 0.025f);
            if (controlPoints.Count < 2 ||
                CalculatePolylineLength(controlPoints) <
                context.Profile.MinimumBranchLength)
            {
                RejectCollapsedBranch(
                    context,
                    stableId,
                    "constrained control polyline was below the profile minimum");
                return null;
            }

            var branchWarnings = new List<string>();
            List<TreeCurveSample> samples = BuildCurveSamples(
                controlPoints,
                startRadius,
                endRadius,
                twistDegrees,
                context.Profile.MaximumSamplesPerBranch,
                branchWarnings,
                "Branch " + stableId);
            float sampledLength = CalculateSampleLength(samples);
            bool containedCollapsedTangent = branchWarnings.Exists(
                warning => warning.IndexOf(
                    "zero-length tangent",
                    StringComparison.Ordinal) >= 0);
            if (samples.Count < 2 ||
                sampledLength < context.Profile.MinimumBranchLength ||
                containedCollapsedTangent)
            {
                RejectCollapsedBranch(
                    context,
                    stableId,
                    containedCollapsedTangent
                        ? "curve sampling produced a zero-length tangent"
                        : "sampled curve was below the profile minimum");
                return null;
            }

            context.Warnings.AddRange(branchWarnings);
            float stiffness = Mathf.Clamp01(1f - order * 0.22f);
            if ((state & TreeBranchState.Dead) != 0)
            {
                stiffness = Mathf.Min(1f, stiffness + 0.18f);
            }

            var branch = new TreeBranchDefinition();
            branch.Initialize(
                stableId,
                parentIndex,
                order,
                attachment,
                SafeProjectDirectionOnParentPlane(direction, parentFrame),
                startRadius,
                endRadius,
                stiffness,
                TreeDeterministicUtility.Sample01(curveSeed, key + "-phase"),
                state,
                foliageStart,
                foliageEnd,
                controlPoints,
                samples);
            return branch;
        }

        private static void ConstrainTrunkControlPoints(
            GenerationContext context,
            List<Vector3> points)
        {
            if (points == null || points.Count < 2)
            {
                return;
            }

            TreeStructuralConstraintSettings constraints =
                context.Profile.StructuralConstraints;
            float maximumHorizontal =
                context.Parameters.Height *
                constraints.MaximumTrunkHorizontalDisplacementRatio;
            for (int index = 1; index < points.Count; index++)
            {
                Vector2 horizontal = new Vector2(
                    points[index].x,
                    points[index].z);
                if (horizontal.magnitude > maximumHorizontal)
                {
                    horizontal = horizontal.normalized * maximumHorizontal;
                    points[index] = new Vector3(
                        horizontal.x,
                        points[index].y,
                        horizontal.y);
                }

                if (points[index].y <= points[index - 1].y)
                {
                    points[index] = new Vector3(
                        points[index].x,
                        points[index - 1].y + 0.001f,
                        points[index].z);
                }
            }

            ConstrainPolylineTurns(
                points,
                Vector3.up,
                constraints.MaximumTrunkSegmentTurnDegrees,
                constraints.MaximumPrimaryAccumulatedTurnDegrees,
                0.75f,
                0f,
                context.Parameters.Height);
            ClampControlPointsToReferenceEnvelope(context, points);
        }

        private static void ConstrainBranchControlPoints(
            GenerationContext context,
            List<Vector3> points,
            Vector3 origin,
            Vector3 initialDirection,
            int order,
            float intendedLength)
        {
            if (points == null || points.Count < 2)
            {
                return;
            }

            TreeStructuralConstraintSettings constraints =
                context.Profile.StructuralConstraints;
            float maximumAccumulated = order <= 1
                ? constraints.MaximumPrimaryAccumulatedTurnDegrees
                : constraints.MaximumHigherOrderAccumulatedTurnDegrees;
            float maximumArcChord = order <= 1
                ? constraints.MaximumPrimaryArcChordRatio
                : constraints.MaximumHigherOrderArcChordRatio;

            ConstrainPolylineTurns(
                points,
                initialDirection,
                constraints.MaximumBranchSegmentTurnDegrees,
                maximumAccumulated,
                constraints.MinimumForwardProgress,
                constraints.MaximumParentReturnFraction,
                intendedLength);

            float arcLength = CalculatePolylineLength(points);
            float chordLength = Vector3.Distance(
                points[0],
                points[points.Count - 1]);
            float ratio = chordLength > Epsilon
                ? arcLength / chordLength
                : float.PositiveInfinity;
            if (ratio > maximumArcChord && chordLength > Epsilon)
            {
                float blend = Mathf.Clamp01(
                    (ratio - maximumArcChord) /
                    Mathf.Max(0.001f, ratio - 1f));
                Vector3 start = points[0];
                Vector3 end = points[points.Count - 1];
                for (int index = 1; index < points.Count - 1; index++)
                {
                    float t = index / (float)(points.Count - 1);
                    Vector3 direct = Vector3.Lerp(start, end, t);
                    points[index] = Vector3.Lerp(
                        points[index],
                        direct,
                        blend);
                }

                ConstrainPolylineTurns(
                    points,
                    initialDirection,
                    constraints.MaximumBranchSegmentTurnDegrees,
                    maximumAccumulated,
                    constraints.MinimumForwardProgress,
                    constraints.MaximumParentReturnFraction,
                    intendedLength);
            }

            ClampControlPointsToReferenceEnvelope(context, points);
        }

        private static void ClampControlPointsToReferenceEnvelope(
            GenerationContext context,
            List<Vector3> points)
        {
            TreeReferenceCalibrationPreset calibration =
                context.Calibration;
            if (calibration == null ||
                calibration.TargetVisibleWidth <= 0f ||
                calibration.TargetVisibleDepth <= 0f)
            {
                return;
            }

            float radiusX = Mathf.Max(
                0.1f,
                calibration.TargetVisibleWidth * 0.5f *
                (1f - context.Profile.StructuralConstraints.CrownEnvelopeOvershoot * 0.35f));
            float radiusZ = Mathf.Max(
                0.1f,
                calibration.TargetVisibleDepth * 0.5f *
                (1f - context.Profile.StructuralConstraints.CrownEnvelopeOvershoot * 0.35f));
            float maximumY = Mathf.Max(
                0.1f,
                calibration.TargetVisibleHeight -
                context.Profile.MinimumBranchRadius);

            for (int index = 1; index < points.Count; index++)
            {
                Vector3 point = points[index];
                point.y = Mathf.Clamp(point.y, 0f, maximumY);
                float normalized =
                    (point.x * point.x) / (radiusX * radiusX) +
                    (point.z * point.z) / (radiusZ * radiusZ);
                if (normalized > 1f)
                {
                    float scale = 1f / Mathf.Sqrt(normalized);
                    point.x *= scale;
                    point.z *= scale;
                }

                points[index] = point;
            }
        }

        private static void ConstrainPolylineTurns(
            List<Vector3> points,
            Vector3 initialDirection,
            float maximumSegmentTurnDegrees,
            float maximumAccumulatedTurnDegrees,
            float minimumForwardProgress,
            float maximumReturnFraction,
            float intendedLength)
        {
            Vector3 forward = initialDirection.sqrMagnitude > Epsilon
                ? initialDirection.normalized
                : Vector3.up;
            Vector3 origin = points[0];
            Vector3 previousDirection = forward;
            float accumulatedTurn = 0f;
            float previousDistance = 0f;
            float nominalStep = Mathf.Max(
                0.001f,
                intendedLength / Mathf.Max(1, points.Count - 1));

            for (int index = 1; index < points.Count; index++)
            {
                Vector3 rawSegment = points[index] - points[index - 1];
                float segmentLength = Mathf.Max(
                    nominalStep * 0.35f,
                    rawSegment.magnitude);
                Vector3 desiredDirection = rawSegment.sqrMagnitude > Epsilon
                    ? rawSegment.normalized
                    : previousDirection;
                float remainingTurn = Mathf.Max(
                    0f,
                    maximumAccumulatedTurnDegrees - accumulatedTurn);
                float allowedTurn = Mathf.Min(
                    maximumSegmentTurnDegrees,
                    remainingTurn);
                Vector3 constrainedDirection = Vector3.RotateTowards(
                    previousDirection,
                    desiredDirection,
                    allowedTurn * Mathf.Deg2Rad,
                    0f).normalized;

                Vector3 candidate =
                    points[index - 1] +
                    constrainedDirection * segmentLength;
                float normalizedIndex =
                    index / (float)(points.Count - 1);
                float minimumForward =
                    intendedLength *
                    normalizedIndex *
                    minimumForwardProgress;
                float currentForward = Vector3.Dot(
                    candidate - origin,
                    forward);
                if (currentForward < minimumForward)
                {
                    candidate += forward *
                        (minimumForward - currentForward);
                }

                float distance = Vector3.Distance(origin, candidate);
                float minimumDistance = Mathf.Max(
                    0f,
                    previousDistance -
                    nominalStep * maximumReturnFraction);
                if (distance < minimumDistance &&
                    distance > Epsilon)
                {
                    candidate = origin +
                        (candidate - origin).normalized * minimumDistance;
                    distance = minimumDistance;
                }

                points[index] = candidate;
                accumulatedTurn += Vector3.Angle(
                    previousDirection,
                    constrainedDirection);
                previousDirection = constrainedDirection;
                previousDistance = distance;
            }
        }

        private static float CalculatePolylineLength(
            IReadOnlyList<Vector3> points)
        {
            float length = 0f;
            if (points == null)
            {
                return length;
            }

            for (int index = 1; index < points.Count; index++)
            {
                length += Vector3.Distance(
                    points[index - 1],
                    points[index]);
            }

            return length;
        }

        private static float CalculateSampleLength(
            IReadOnlyList<TreeCurveSample> samples)
        {
            float length = 0f;
            if (samples == null)
            {
                return length;
            }

            for (int index = 1; index < samples.Count; index++)
            {
                length += Vector3.Distance(
                    samples[index - 1].Position,
                    samples[index].Position);
            }

            return length;
        }

        private static void RemoveCollapsedControlPoints(
            List<Vector3> points,
            float minimumSeparation)
        {
            if (points == null || points.Count < 2)
            {
                return;
            }

            float separation = Mathf.Max(0.00001f, minimumSeparation);
            for (int index = points.Count - 1; index > 0; index--)
            {
                if (Vector3.Distance(points[index - 1], points[index]) < separation)
                {
                    points.RemoveAt(index);
                }
            }
        }

        private static void RejectCollapsedBranch(
            GenerationContext context,
            int stableId,
            string reason)
        {
            context.RejectedBranches++;
            context.Warnings.Add(
                "Branch " + stableId + " was rejected because its " + reason + ".");
        }

        private static void FitStructureToReferenceWidthAndDepth(
            GenerationContext context)
        {
            TreeReferenceCalibrationPreset calibration = context.Calibration;
            if (calibration == null ||
                calibration.TargetVisibleWidth <= 0f ||
                calibration.TargetVisibleDepth <= 0f ||
                context.Branches.Count < 2)
            {
                return;
            }

            TreeBranchDefinition trunk = context.Branches[0];
            if (trunk.Samples == null || trunk.Samples.Count < 2)
            {
                return;
            }

            const int maximumIterations = 3;
            for (int iteration = 0; iteration < maximumIterations; iteration++)
            {
                Bounds bounds = CalculateBounds(context.Branches);
                float widthRatio = calibration.TargetVisibleWidth /
                    Mathf.Max(0.001f, bounds.size.x);
                float depthRatio = calibration.TargetVisibleDepth /
                    Mathf.Max(0.001f, bounds.size.z);
                if (Mathf.Abs(widthRatio - 1f) <= 0.025f &&
                    Mathf.Abs(depthRatio - 1f) <= 0.025f)
                {
                    break;
                }

                float scaleX = Mathf.Clamp(widthRatio, 0.55f, 1.8f);
                float scaleZ = Mathf.Clamp(depthRatio, 0.55f, 1.8f);
                for (int branchIndex = 1;
                     branchIndex < context.Branches.Count;
                     branchIndex++)
                {
                    TreeBranchDefinition branch = context.Branches[branchIndex];
                    var controlPoints = new List<Vector3>(
                        branch.ControlPoints.Count);
                    for (int pointIndex = 0;
                         pointIndex < branch.ControlPoints.Count;
                         pointIndex++)
                    {
                        controlPoints.Add(ScaleAroundTrunkAxis(
                            branch.ControlPoints[pointIndex],
                            trunk.Samples,
                            scaleX,
                            scaleZ));
                    }

                    var positions = new List<Vector3>(branch.Samples.Count);
                    for (int sampleIndex = 0;
                         sampleIndex < branch.Samples.Count;
                         sampleIndex++)
                    {
                        positions.Add(ScaleAroundTrunkAxis(
                            branch.Samples[sampleIndex].Position,
                            trunk.Samples,
                            scaleX,
                            scaleZ));
                    }

                    var samples = new List<TreeCurveSample>(
                        branch.Samples.Count);
                    Vector3 previousNormal = Vector3.zero;
                    for (int sampleIndex = 0;
                         sampleIndex < branch.Samples.Count;
                         sampleIndex++)
                    {
                        TreeCurveSample source = branch.Samples[sampleIndex];
                        Vector3 tangent = CalculateTangent(
                            positions,
                            sampleIndex);
                        if (tangent.sqrMagnitude < Epsilon)
                        {
                            tangent = source.Tangent.sqrMagnitude > Epsilon
                                ? source.Tangent.normalized
                                : Vector3.up;
                        }
                        else
                        {
                            tangent.Normalize();
                        }

                        Vector3 normal = new Vector3(
                            source.Normal.x / Mathf.Max(0.001f, scaleX),
                            source.Normal.y,
                            source.Normal.z / Mathf.Max(0.001f, scaleZ));
                        normal = Vector3.ProjectOnPlane(normal, tangent);
                        if (normal.sqrMagnitude < Epsilon)
                        {
                            normal = ChooseInitialNormal(tangent);
                        }
                        else
                        {
                            normal.Normalize();
                        }

                        if (previousNormal.sqrMagnitude > Epsilon &&
                            Vector3.Dot(previousNormal, normal) < 0f)
                        {
                            normal = -normal;
                        }

                        Vector3 binormal = Vector3.Cross(
                            tangent,
                            normal);
                        if (binormal.sqrMagnitude < Epsilon)
                        {
                            normal = ChooseInitialNormal(tangent);
                            binormal = Vector3.Cross(tangent, normal);
                        }
                        binormal.Normalize();
                        normal = Vector3.Cross(binormal, tangent).normalized;
                        previousNormal = normal;

                        samples.Add(new TreeCurveSample(
                            positions[sampleIndex],
                            tangent,
                            normal,
                            binormal,
                            source.Radius,
                            source.NormalizedDistance));
                    }

                    branch.ReplaceGeometry(
                        controlPoints,
                        samples,
                        branch.BaseRadius,
                        branch.EndRadius);
                }
            }
        }

        private static void ReanchorBranchRootsAfterReferenceFit(
            GenerationContext context)
        {
            for (int branchIndex = 1;
                 branchIndex < context.Branches.Count;
                 branchIndex++)
            {
                TreeBranchDefinition branch = context.Branches[branchIndex];
                if (branch.ParentBranchIndex < 0 ||
                    branch.ParentBranchIndex >= context.Branches.Count ||
                    branch.Samples == null ||
                    branch.Samples.Count < 2)
                {
                    continue;
                }

                TreeBranchDefinition parent =
                    context.Branches[branch.ParentBranchIndex];
                ParentFrame parentFrame = EvaluateBranchFrame(
                    parent,
                    branch.ParentAttachmentDistance);
                Vector3 direction = branch.Samples[1].Position -
                    branch.Samples[0].Position;
                if (direction.sqrMagnitude <= Epsilon)
                {
                    direction = branch.Samples[0].Tangent;
                }
                direction = direction.sqrMagnitude > Epsilon
                    ? direction.normalized
                    : parentFrame.Normal;

                Vector3 targetRoot = CalculateEmbeddedBranchRoot(
                    parentFrame,
                    direction,
                    branch.BaseRadius,
                    context,
                    branch.StableBranchId);
                Vector3 offset = targetRoot - branch.Samples[0].Position;
                if (offset.sqrMagnitude > Epsilon)
                {
                    var controlPoints = new List<Vector3>(
                        branch.ControlPoints.Count);
                    for (int pointIndex = 0;
                         pointIndex < branch.ControlPoints.Count;
                         pointIndex++)
                    {
                        controlPoints.Add(
                            branch.ControlPoints[pointIndex] + offset);
                    }

                    var samples = new List<TreeCurveSample>(
                        branch.Samples.Count);
                    for (int sampleIndex = 0;
                         sampleIndex < branch.Samples.Count;
                         sampleIndex++)
                    {
                        TreeCurveSample source = branch.Samples[sampleIndex];
                        samples.Add(new TreeCurveSample(
                            source.Position + offset,
                            source.Tangent,
                            source.Normal,
                            source.Binormal,
                            source.Radius,
                            source.NormalizedDistance));
                    }

                    branch.ReplaceGeometry(
                        controlPoints,
                        samples,
                        branch.BaseRadius,
                        branch.EndRadius);
                }

                branch.ReplaceLocalReferenceAxis(
                    SafeProjectDirectionOnParentPlane(
                        direction,
                        parentFrame));
            }
        }

        private static Vector3 ScaleAroundTrunkAxis(
            Vector3 position,
            IReadOnlyList<TreeCurveSample> trunkSamples,
            float scaleX,
            float scaleZ)
        {
            Vector3 trunkCenter = SampleTrunkCenterAtHeight(
                trunkSamples,
                position.y);
            Vector3 offset = position - trunkCenter;
            return new Vector3(
                trunkCenter.x + offset.x * scaleX,
                position.y,
                trunkCenter.z + offset.z * scaleZ);
        }

        private static Vector3 SampleTrunkCenterAtHeight(
            IReadOnlyList<TreeCurveSample> samples,
            float height)
        {
            if (samples == null || samples.Count == 0)
            {
                return new Vector3(0f, height, 0f);
            }

            if (height <= samples[0].Position.y)
            {
                Vector3 first = samples[0].Position;
                return new Vector3(first.x, height, first.z);
            }

            for (int index = 1; index < samples.Count; index++)
            {
                Vector3 previous = samples[index - 1].Position;
                Vector3 current = samples[index].Position;
                float minimumY = Mathf.Min(previous.y, current.y);
                float maximumY = Mathf.Max(previous.y, current.y);
                if (height < minimumY || height > maximumY)
                {
                    continue;
                }

                float denominator = current.y - previous.y;
                float t = Mathf.Abs(denominator) > Epsilon
                    ? Mathf.Clamp01((height - previous.y) / denominator)
                    : 0f;
                Vector3 center = Vector3.Lerp(previous, current, t);
                return new Vector3(center.x, height, center.z);
            }

            Vector3 last = samples[samples.Count - 1].Position;
            return new Vector3(last.x, height, last.z);
        }

        private static List<TreeCurveSample> BuildCurveSamples(
            List<Vector3> controlPoints,
            float startRadius,
            float endRadius,
            float twistDegrees,
            int maximumSamples,
            List<string> warnings,
            string label)
        {
            int sampleCount = Mathf.Clamp(
                Mathf.Max(6, controlPoints.Count * 4),
                4,
                maximumSamples);
            var positions = new List<Vector3>(sampleCount);
            for (int index = 0; index < sampleCount; index++)
            {
                float t = index / (float)(sampleCount - 1);
                positions.Add(EvaluateCatmullRom(controlPoints, t));
            }

            var samples = new List<TreeCurveSample>(sampleCount);
            Vector3 previousTangent = CalculateTangent(positions, 0);
            Vector3 normal = ChooseInitialNormal(previousTangent);
            float previousTwist = 0f;

            for (int index = 0; index < positions.Count; index++)
            {
                float t = index / (float)(positions.Count - 1);
                Vector3 tangent = CalculateTangent(positions, index);
                if (tangent.sqrMagnitude < Epsilon)
                {
                    tangent = previousTangent.sqrMagnitude > Epsilon
                        ? previousTangent
                        : Vector3.up;
                    warnings.Add(label + " contained a zero-length tangent; previous tangent was reused.");
                }
                tangent.Normalize();

                if (index > 0)
                {
                    Quaternion transport = Quaternion.FromToRotation(
                        previousTangent,
                        tangent);
                    normal = transport * normal;
                    normal = Vector3.ProjectOnPlane(normal, tangent);
                    if (normal.sqrMagnitude < Epsilon)
                    {
                        normal = ChooseInitialNormal(tangent);
                        warnings.Add(label + " required a transported-frame recovery.");
                    }
                    normal.Normalize();
                }

                float totalTwist = twistDegrees * Mathf.Deg2Rad * t;
                float twistDelta = totalTwist - previousTwist;
                if (Mathf.Abs(twistDelta) > Epsilon)
                {
                    normal = Quaternion.AngleAxis(
                        twistDelta * Mathf.Rad2Deg,
                        tangent) * normal;
                    normal = Vector3.ProjectOnPlane(normal, tangent).normalized;
                }

                Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
                normal = Vector3.Cross(binormal, tangent).normalized;
                float radius = Mathf.Lerp(startRadius, endRadius, t);
                samples.Add(new TreeCurveSample(
                    positions[index],
                    tangent,
                    normal,
                    binormal,
                    radius,
                    t));
                previousTangent = tangent;
                previousTwist = totalTwist;
            }

            return samples;
        }

        private static Vector3 EvaluateCatmullRom(
            List<Vector3> controlPoints,
            float normalizedDistance)
        {
            if (controlPoints == null || controlPoints.Count == 0)
            {
                return Vector3.zero;
            }

            if (controlPoints.Count == 1)
            {
                return controlPoints[0];
            }

            float scaled = Mathf.Clamp01(normalizedDistance) *
                (controlPoints.Count - 1);
            int segment = Mathf.Min(
                controlPoints.Count - 2,
                Mathf.FloorToInt(scaled));
            float t = scaled - segment;
            Vector3 p0 = controlPoints[Mathf.Max(0, segment - 1)];
            Vector3 p1 = controlPoints[segment];
            Vector3 p2 = controlPoints[segment + 1];
            Vector3 p3 = controlPoints[Mathf.Min(
                controlPoints.Count - 1,
                segment + 2)];
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        private static Vector3 CalculateTangent(
            List<Vector3> positions,
            int index)
        {
            if (positions.Count < 2)
            {
                return Vector3.up;
            }

            if (index <= 0)
            {
                return positions[1] - positions[0];
            }

            if (index >= positions.Count - 1)
            {
                return positions[positions.Count - 1] -
                    positions[positions.Count - 2];
            }

            return positions[index + 1] - positions[index - 1];
        }

        private static Vector3 ChooseInitialNormal(Vector3 tangent)
        {
            Vector3 axis = Mathf.Abs(Vector3.Dot(tangent.normalized, Vector3.right)) < 0.85f
                ? Vector3.right
                : Vector3.forward;
            Vector3 normal = Vector3.ProjectOnPlane(axis, tangent);
            return normal.sqrMagnitude > Epsilon ? normal.normalized : Vector3.up;
        }

        private static ParentFrame EvaluateBranchFrame(
            TreeBranchDefinition branch,
            float normalizedDistance)
        {
            IReadOnlyList<TreeCurveSample> samples = branch.Samples;
            if (samples == null || samples.Count == 0)
            {
                return new ParentFrame
                {
                    Position = Vector3.zero,
                    Tangent = Vector3.up,
                    Normal = Vector3.right,
                    Binormal = Vector3.forward,
                    Radius = branch.BaseRadius
                };
            }

            float scaled = Mathf.Clamp01(normalizedDistance) * (samples.Count - 1);
            int lower = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, samples.Count - 1);
            int upper = Mathf.Min(samples.Count - 1, lower + 1);
            float t = scaled - lower;
            TreeCurveSample a = samples[lower];
            TreeCurveSample b = samples[upper];
            Vector3 tangent = Vector3.Slerp(a.Tangent, b.Tangent, t).normalized;
            Vector3 normal = Vector3.Slerp(a.Normal, b.Normal, t);
            normal = Vector3.ProjectOnPlane(normal, tangent).normalized;
            Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
            return new ParentFrame
            {
                Position = Vector3.Lerp(a.Position, b.Position, t),
                Tangent = tangent,
                Normal = normal,
                Binormal = binormal,
                Radius = Mathf.Lerp(a.Radius, b.Radius, t)
            };
        }

        private static float ResolvePrimaryAttachment(
            GenerationContext context,
            int index,
            int count,
            int tierCount,
            int branchesPerTier,
            int seed)
        {
            TreeResolvedParameters p = context.Parameters;
            float normalized;
            if (tierCount > 0 && branchesPerTier > 0)
            {
                int tier = index / branchesPerTier;
                int effectiveTiers = Mathf.Max(1, tierCount);
                normalized = effectiveTiers <= 1
                    ? 0.5f
                    : tier / (float)(effectiveTiers - 1);
                normalized += TreeDeterministicUtility.SampleSigned(
                    seed,
                    "primary-tier-height-" + index) *
                    context.Profile.PrimaryBranches.TierIrregularity.Midpoint;
            }
            else
            {
                normalized = count <= 1 ? 0.5f : index / (float)(count - 1);
                normalized += TreeDeterministicUtility.SampleSigned(
                    seed,
                    "primary-height-" + index) * 0.055f;
            }

            return Mathf.Lerp(
                p.PrimaryBranchStartHeight,
                p.PrimaryBranchEndHeight,
                Mathf.Clamp01(normalized));
        }

        private static float ResolvePrimaryYaw(
            GenerationContext context,
            int index,
            int tierCount,
            int branchesPerTier,
            int seed)
        {
            float evenYaw;
            if (tierCount > 0 && branchesPerTier > 0)
            {
                int tier = index / branchesPerTier;
                int inTier = index % branchesPerTier;
                float tierRotation = TreeDeterministicUtility.SampleSigned(
                    seed,
                    "primary-tier-rotation-" + tier) * 28f;
                evenYaw = inTier * (360f / Mathf.Max(1, branchesPerTier)) +
                    tierRotation;
            }
            else
            {
                evenYaw = index * GoldenAngleDegrees;
            }

            float randomYaw = TreeDeterministicUtility.Sample01(
                seed,
                "primary-random-yaw-" + index) * 360f;
            float yaw = Mathf.LerpAngle(
                randomYaw,
                evenYaw,
                Mathf.Clamp01(context.Parameters.AzimuthSymmetry));
            float remainingJitter = 1f - Mathf.Clamp01(
                context.Parameters.AzimuthSymmetry);
            yaw += TreeDeterministicUtility.SampleSigned(
                seed,
                "primary-yaw-jitter-" + index) * 22f * remainingJitter;
            return yaw;
        }

        private static Vector3 ResolvePrimaryDirection(
            GenerationContext context,
            ParentFrame frame,
            float yawDegrees,
            int index)
        {
            float resolvedYaw = yawDegrees;
            float biasStrength = Mathf.Clamp01(
                context.Parameters.DirectionalBiasStrength);
            if (biasStrength > 0f)
            {
                float biasRadians =
                    context.Parameters.DirectionalBiasAngleDegrees *
                    Mathf.Deg2Rad;
                Vector3 treeLocalBias = new Vector3(
                    Mathf.Cos(biasRadians),
                    0f,
                    Mathf.Sin(biasRadians));
                Vector3 projectedBias = Vector3.ProjectOnPlane(
                    treeLocalBias,
                    frame.Tangent);
                if (projectedBias.sqrMagnitude > Epsilon)
                {
                    projectedBias.Normalize();
                    float biasYaw = Mathf.Atan2(
                        Vector3.Dot(projectedBias, frame.Binormal),
                        Vector3.Dot(projectedBias, frame.Normal)) *
                        Mathf.Rad2Deg;
                    resolvedYaw = Mathf.LerpAngle(
                        resolvedYaw,
                        biasYaw,
                        biasStrength);
                }
            }

            float radians = resolvedYaw * Mathf.Deg2Rad;
            Vector3 radial = (
                frame.Normal * Mathf.Cos(radians) +
                frame.Binormal * Mathf.Sin(radians)).normalized;
            float elevation = context.Parameters.InitialBranchElevationDegrees;
            elevation += TreeDeterministicUtility.SampleSigned(
                context.Seeds.GetSeed(TreeSeedStream.PrimaryBranchLayout),
                "primary-elevation-jitter-" + index) * 4f;
            float elevationRadians = elevation * Mathf.Deg2Rad;
            Vector3 direction =
                radial * Mathf.Cos(elevationRadians) +
                frame.Tangent * Mathf.Sin(elevationRadians);
            return direction.sqrMagnitude > Epsilon
                ? direction.normalized
                : radial;
        }

        private static Vector3 CalculateEmbeddedBranchRoot(
            ParentFrame parentFrame,
            Vector3 direction,
            float childRadius,
            GenerationContext context,
            int stableId)
        {
            Vector3 radialDirection = Vector3.ProjectOnPlane(
                direction,
                parentFrame.Tangent);
            float radialMagnitude = radialDirection.magnitude;
            if (radialMagnitude < 0.2f)
            {
                context.Warnings.Add(
                    "Branch " + stableId +
                    " launch was nearly parallel to its parent; root intersection used the safe radial floor.");
                radialMagnitude = 0.2f;
            }

            float safeParentRadius = Mathf.Max(
                context.Profile.MinimumBranchRadius,
                parentFrame.Radius);
            float safeChildRadius = Mathf.Min(
                safeParentRadius * 0.9f,
                Mathf.Max(context.Profile.MinimumBranchRadius, childRadius));
            float rootDistance = Mathf.Max(
                0f,
                (safeParentRadius - safeChildRadius * 1.08f) / radialMagnitude);
            return parentFrame.Position + direction * rootDistance;
        }

        private static Vector3 SafeProjectDirectionOnParentPlane(
            Vector3 direction,
            ParentFrame parentFrame)
        {
            Vector3 radial = Vector3.ProjectOnPlane(
                direction,
                parentFrame.Tangent);
            return radial.sqrMagnitude > Epsilon
                ? radial.normalized
                : parentFrame.Normal;
        }

        private static bool ShouldRejectBranch(
            GenerationContext context,
            int damageSeed,
            int branchId)
        {
            return TreeDeterministicUtility.Sample01(
                damageSeed,
                "missing-" + branchId) <
                context.Parameters.MissingBranchProbability;
        }

        private static TreeBranchState ResolveBranchState(
            TreeResolvedParameters parameters,
            int damageSeed,
            int branchId)
        {
            TreeBranchState state = TreeBranchState.None;
            if (TreeDeterministicUtility.Sample01(
                    damageSeed,
                    "dead-" + branchId) < parameters.DeadBranchProbability)
            {
                state |= TreeBranchState.Dead;
            }

            if (TreeDeterministicUtility.Sample01(
                    damageSeed,
                    "broken-" + branchId) < parameters.BreakProbability)
            {
                state |= TreeBranchState.Broken;
            }

            return state;
        }

        private static void ValidateStructure(
            GenerationContext context,
            List<string> failures)
        {
            if (context.Branches.Count == 0)
            {
                failures.Add("Generator emitted no branches.");
                return;
            }

            var ids = new HashSet<int>();
            for (int index = 0; index < context.Branches.Count; index++)
            {
                TreeBranchDefinition branch = context.Branches[index];
                if (!ids.Add(branch.StableBranchId))
                {
                    failures.Add("Duplicate stable branch ID: " + branch.StableBranchId + ".");
                }

                if (index == 0)
                {
                    if (branch.ParentBranchIndex != -1 || branch.BranchOrder != 0)
                    {
                        failures.Add("Trunk branch has invalid parent/order metadata.");
                    }
                }
                else if (branch.ParentBranchIndex < 0 ||
                         branch.ParentBranchIndex >= index)
                {
                    failures.Add(
                        "Branch " + branch.StableBranchId +
                        " has an invalid or forward parent index.");
                }

                if (branch.ParentAttachmentDistance < 0f ||
                    branch.ParentAttachmentDistance > 1f)
                {
                    failures.Add("Branch attachment distance is outside [0,1].");
                }

                if (branch.Samples == null || branch.Samples.Count < 2)
                {
                    failures.Add("Branch " + branch.StableBranchId + " has fewer than two curve samples.");
                    continue;
                }

                float length = 0f;
                for (int sampleIndex = 0; sampleIndex < branch.Samples.Count; sampleIndex++)
                {
                    TreeCurveSample sample = branch.Samples[sampleIndex];
                    if (!IsFinite(sample.Position) ||
                        !IsFinite(sample.Tangent) ||
                        !IsFinite(sample.Normal) ||
                        !IsFinite(sample.Binormal) ||
                        !TreeDeterministicUtility.IsFinite(sample.Radius))
                    {
                        failures.Add("Branch " + branch.StableBranchId + " contains non-finite sample data.");
                        break;
                    }

                    if (sample.Radius <= 0f)
                    {
                        failures.Add("Branch " + branch.StableBranchId + " contains a non-positive radius.");
                    }

                    if (Mathf.Abs(Vector3.Dot(sample.Tangent, sample.Normal)) > 0.01f ||
                        Mathf.Abs(Vector3.Dot(sample.Tangent, sample.Binormal)) > 0.01f)
                    {
                        failures.Add("Branch " + branch.StableBranchId + " contains a non-orthogonal frame.");
                    }

                    if (sampleIndex > 0)
                    {
                        length += Vector3.Distance(
                            branch.Samples[sampleIndex - 1].Position,
                            sample.Position);
                    }
                }

                if (length < context.Profile.MinimumBranchLength)
                {
                    failures.Add(
                        "Branch " + branch.StableBranchId +
                        " length is below the profile minimum.");
                }
            }
        }

        private static TreeGenerationMetrics CalculateMetrics(
            GenerationContext context,
            long elapsedTicks)
        {
            int primary = 0;
            int secondary = 0;
            int tertiary = 0;
            int dead = 0;
            int broken = 0;
            int foliageEligible = 0;
            int controls = 0;
            int samples = 0;
            int backwardViolations = 0;
            int envelopeViolationBranches = 0;
            float length = 0f;
            float minimumRadius = float.PositiveInfinity;
            float maximumRadius = 0f;
            float maximumSegmentTurn = 0f;
            float maximumAccumulatedTurn = 0f;
            float maximumArcChord = 1f;
            float maximumEnvelopeOvershoot = 0f;

            Bounds generatedBounds = CalculateBounds(context.Branches);
            float crownBase =
                context.Parameters.Height *
                context.Parameters.CrownStartHeight;
            float crownTop = Mathf.Max(
                crownBase + 0.1f,
                generatedBounds.max.y);
            float crownRadiusX = context.Calibration != null &&
                context.Calibration.TargetVisibleWidth > 0f
                    ? context.Calibration.TargetVisibleWidth * 0.5f
                    : Mathf.Max(0.1f, generatedBounds.extents.x);
            float crownRadiusZ = context.Calibration != null &&
                context.Calibration.TargetVisibleDepth > 0f
                    ? context.Calibration.TargetVisibleDepth * 0.5f
                    : Mathf.Max(0.1f, generatedBounds.extents.z);
            float crownRadiusY = Mathf.Max(
                0.1f,
                (crownTop - crownBase) * 0.5f);
            Vector3 crownCenter = new Vector3(
                generatedBounds.center.x,
                crownBase + crownRadiusY,
                generatedBounds.center.z);
            float allowedEnvelope =
                1f +
                context.Profile.StructuralConstraints.CrownEnvelopeOvershoot;

            for (int index = 0; index < context.Branches.Count; index++)
            {
                TreeBranchDefinition branch = context.Branches[index];
                switch (branch.BranchOrder)
                {
                    case 1:
                        primary++;
                        break;
                    case 2:
                        secondary++;
                        break;
                    case 3:
                        tertiary++;
                        break;
                }

                dead += branch.IsDead ? 1 : 0;
                broken += branch.IsBroken ? 1 : 0;
                bool isFoliageEligible =
                    !branch.IsDead &&
                    !branch.IsBroken &&
                    branch.FoliageEligibilityEnd >
                        branch.FoliageEligibilityStart &&
                    branch.BranchOrder > 0;
                if (isFoliageEligible)
                {
                    foliageEligible++;
                }

                controls += branch.ControlPoints != null
                    ? branch.ControlPoints.Count
                    : 0;
                samples += branch.Samples != null
                    ? branch.Samples.Count
                    : 0;
                float branchLength = CalculateBranchLength(branch);
                length += branchLength;
                minimumRadius = Mathf.Min(
                    minimumRadius,
                    branch.EndRadius,
                    branch.BaseRadius);
                maximumRadius = Mathf.Max(
                    maximumRadius,
                    branch.EndRadius,
                    branch.BaseRadius);

                IReadOnlyList<TreeCurveSample> curve = branch.Samples;
                if (curve == null || curve.Count < 2)
                {
                    continue;
                }

                Vector3 initialDirection =
                    (curve[1].Position - curve[0].Position).normalized;
                float accumulatedTurn = 0f;
                float arcLength = 0f;
                bool envelopeViolation = false;
                for (int sampleIndex = 1;
                     sampleIndex < curve.Count;
                     sampleIndex++)
                {
                    Vector3 segment =
                        curve[sampleIndex].Position -
                        curve[sampleIndex - 1].Position;
                    arcLength += segment.magnitude;
                    if (segment.sqrMagnitude > Epsilon)
                    {
                        if (Vector3.Dot(
                                segment.normalized,
                                initialDirection) < -0.02f)
                        {
                            backwardViolations++;
                        }

                        float turn = Vector3.Angle(
                            curve[sampleIndex - 1].Tangent,
                            curve[sampleIndex].Tangent);
                        maximumSegmentTurn = Mathf.Max(
                            maximumSegmentTurn,
                            turn);
                        accumulatedTurn += turn;
                    }

                    if (isFoliageEligible &&
                        curve[sampleIndex].NormalizedDistance >=
                            branch.FoliageEligibilityStart)
                    {
                        Vector3 offset =
                            curve[sampleIndex].Position -
                            crownCenter;
                        float normalizedEnvelope = Mathf.Sqrt(
                            (offset.x * offset.x) /
                                (crownRadiusX * crownRadiusX) +
                            (offset.y * offset.y) /
                                (crownRadiusY * crownRadiusY) +
                            (offset.z * offset.z) /
                                (crownRadiusZ * crownRadiusZ));
                        maximumEnvelopeOvershoot = Mathf.Max(
                            maximumEnvelopeOvershoot,
                            Mathf.Max(0f, normalizedEnvelope - 1f));
                        if (normalizedEnvelope > allowedEnvelope)
                        {
                            envelopeViolation = true;
                        }
                    }
                }

                maximumAccumulatedTurn = Mathf.Max(
                    maximumAccumulatedTurn,
                    accumulatedTurn);
                float chordLength = Vector3.Distance(
                    curve[0].Position,
                    curve[curve.Count - 1].Position);
                if (chordLength > Epsilon)
                {
                    maximumArcChord = Mathf.Max(
                        maximumArcChord,
                        arcLength / chordLength);
                }

                if (envelopeViolation)
                {
                    envelopeViolationBranches++;
                }
            }

            if (float.IsPositiveInfinity(minimumRadius))
            {
                minimumRadius = 0f;
            }

            float heightRatio = 1f;
            float widthRatio = 1f;
            float depthRatio = 1f;
            bool withinTolerance = true;
            if (context.Calibration != null &&
                context.Calibration.TargetVisibleHeight > 0f &&
                context.Calibration.TargetVisibleWidth > 0f &&
                context.Calibration.TargetVisibleDepth > 0f)
            {
                heightRatio = generatedBounds.size.y /
                    context.Calibration.TargetVisibleHeight;
                widthRatio = generatedBounds.size.x /
                    context.Calibration.TargetVisibleWidth;
                depthRatio = generatedBounds.size.z /
                    context.Calibration.TargetVisibleDepth;
                float tolerance =
                    context.Calibration.DimensionTolerance;
                withinTolerance =
                    Mathf.Abs(heightRatio - 1f) <= tolerance &&
                    Mathf.Abs(widthRatio - 1f) <= tolerance &&
                    Mathf.Abs(depthRatio - 1f) <= tolerance;
            }

            var metrics = new TreeGenerationMetrics();
            metrics.Initialize(
                context.Branches.Count,
                primary,
                secondary,
                tertiary,
                context.RejectedBranches,
                dead,
                broken,
                foliageEligible,
                controls,
                samples,
                length,
                minimumRadius,
                maximumRadius,
                maximumSegmentTurn,
                maximumAccumulatedTurn,
                maximumArcChord,
                backwardViolations,
                envelopeViolationBranches,
                maximumEnvelopeOvershoot,
                heightRatio,
                widthRatio,
                depthRatio,
                withinTolerance,
                elapsedTicks);
            return metrics;
        }

        private static Bounds CalculateBounds(List<TreeBranchDefinition> branches)
        {
            bool initialized = false;
            Bounds bounds = default;
            for (int branchIndex = 0; branchIndex < branches.Count; branchIndex++)
            {
                IReadOnlyList<TreeCurveSample> samples = branches[branchIndex].Samples;
                for (int sampleIndex = 0; sampleIndex < samples.Count; sampleIndex++)
                {
                    TreeCurveSample sample = samples[sampleIndex];
                    Vector3 extent = Vector3.one * sample.Radius;
                    Bounds sampleBounds = new Bounds(sample.Position, extent * 2f);
                    if (!initialized)
                    {
                        bounds = sampleBounds;
                        initialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(sampleBounds.min);
                        bounds.Encapsulate(sampleBounds.max);
                    }
                }
            }

            return initialized ? bounds : new Bounds(Vector3.zero, Vector3.zero);
        }

        private static Vector2 CalculateFootprint(Bounds bounds)
        {
            return new Vector2(
                Mathf.Max(Mathf.Abs(bounds.min.x), Mathf.Abs(bounds.max.x)),
                Mathf.Max(Mathf.Abs(bounds.min.z), Mathf.Abs(bounds.max.z)));
        }

        private static string CalculateDependencyFingerprint(GenerationContext context)
        {
            ulong hash = TreeDeterministicUtility.BeginHash();
            TreeDeterministicUtility.Append(ref hash, CurrentGeneratorVersion);
            TreeDeterministicUtility.Append(ref hash, context.Profile.StableIdentity);
            TreeDeterministicUtility.Append(ref hash, context.Profile.ProfileVersion);
            TreeDeterministicUtility.Append(ref hash, context.Recipe.StableIdentity);
            TreeDeterministicUtility.Append(ref hash, context.Recipe.RecipeVersion);
            TreeDeterministicUtility.Append(ref hash, context.MasterSeed);
            if (context.Calibration != null)
            {
                TreeDeterministicUtility.Append(
                    ref hash,
                    context.Calibration.StableIdentity);
                TreeDeterministicUtility.Append(
                    ref hash,
                    context.Calibration.CalibrationVersion);
                TreeDeterministicUtility.Append(
                    ref hash,
                    context.Calibration.TargetVisibleHeight);
                TreeDeterministicUtility.Append(
                    ref hash,
                    context.Calibration.TargetVisibleWidth);
                TreeDeterministicUtility.Append(
                    ref hash,
                    context.Calibration.TargetVisibleDepth);
                TreeDeterministicUtility.Append(
                    ref hash,
                    context.Calibration.DimensionTolerance);
            }

            TreeStructuralConstraintSettings constraints =
                context.Profile.StructuralConstraints;
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.MaximumTrunkHorizontalDisplacementRatio);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.MaximumTrunkSegmentTurnDegrees);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.MaximumBranchSegmentTurnDegrees);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.MaximumPrimaryAccumulatedTurnDegrees);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.MaximumHigherOrderAccumulatedTurnDegrees);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.MaximumPrimaryArcChordRatio);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.MaximumHigherOrderArcChordRatio);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.MinimumForwardProgress);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.MaximumParentReturnFraction);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.SecondarySurvivalProbability);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.TertiarySurvivalProbability);
            TreeDeterministicUtility.Append(
                ref hash,
                constraints.CrownEnvelopeOvershoot);
            AppendResolvedParameters(ref hash, context.Parameters, includePalette: true);
            for (int index = 0; index < context.Seeds.Records.Count; index++)
            {
                TreeSeedRecord record = context.Seeds.Records[index];
                TreeDeterministicUtility.Append(ref hash, (int)record.Stream);
                TreeDeterministicUtility.Append(ref hash, record.Seed);
                TreeDeterministicUtility.Append(ref hash, record.Locked);
            }
            return TreeDeterministicUtility.FormatHash(hash);
        }

        private static string CalculateTrunkFingerprint(
            List<TreeBranchDefinition> branches)
        {
            ulong hash = TreeDeterministicUtility.BeginHash();
            if (branches.Count > 0)
            {
                AppendBranch(ref hash, branches[0]);
            }
            return TreeDeterministicUtility.FormatHash(hash);
        }

        private static string CalculateBranchFingerprint(
            List<TreeBranchDefinition> branches)
        {
            ulong hash = TreeDeterministicUtility.BeginHash();
            TreeDeterministicUtility.Append(ref hash, branches.Count);
            for (int index = 0; index < branches.Count; index++)
            {
                AppendBranch(ref hash, branches[index]);
            }
            return TreeDeterministicUtility.FormatHash(hash);
        }

        private static string CalculateFoliageFingerprint(GenerationContext context)
        {
            ulong hash = TreeDeterministicUtility.BeginHash();
            TreeResolvedParameters p = context.Parameters;
            TreeDeterministicUtility.Append(ref hash, p.CrownStartHeight);
            TreeDeterministicUtility.Append(ref hash, p.CrownVolume);
            TreeDeterministicUtility.Append(ref hash, p.CrownWidthScale);
            TreeDeterministicUtility.Append(ref hash, p.CrownHeightScale);
            TreeDeterministicUtility.Append(ref hash, p.CrownFill);
            TreeDeterministicUtility.Append(ref hash, p.CrownAsymmetry);
            TreeDeterministicUtility.Append(ref hash, p.CrownLobeCount);
            TreeDeterministicUtility.Append(ref hash, p.CrownLobeRadius);
            TreeDeterministicUtility.Append(ref hash, p.ClusterWidthScale);
            TreeDeterministicUtility.Append(ref hash, p.ClusterHeightScale);
            TreeDeterministicUtility.Append(ref hash, p.ClusterLengthScale);
            TreeDeterministicUtility.Append(ref hash, p.ClusterRadialSpread);
            TreeDeterministicUtility.Append(ref hash, p.CardSizeScale);
            TreeDeterministicUtility.Append(ref hash, p.FoliageClusterCount);
            TreeDeterministicUtility.Append(ref hash, p.CardsPerCluster);
            TreeDeterministicUtility.Append(ref hash, p.FoliageEligibility);
            TreeDeterministicUtility.Append(ref hash, p.ClusterOccupancy);
            TreeDeterministicUtility.Append(ref hash, p.TerminalFoliageProbability);
            TreeDeterministicUtility.Append(ref hash, p.CardRetentionFraction);
            for (int index = 0; index < context.Branches.Count; index++)
            {
                TreeBranchDefinition branch = context.Branches[index];
                if (!branch.IsDead && !branch.IsBroken && branch.BranchOrder > 0)
                {
                    TreeDeterministicUtility.Append(ref hash, branch.StableBranchId);
                    TreeDeterministicUtility.Append(ref hash, branch.FoliageEligibilityStart);
                    TreeDeterministicUtility.Append(ref hash, branch.FoliageEligibilityEnd);
                }
            }
            return TreeDeterministicUtility.FormatHash(hash);
        }

        private static string CalculatePaletteFingerprint(GenerationContext context)
        {
            ulong hash = TreeDeterministicUtility.BeginHash();
            TreeDeterministicUtility.Append(
                ref hash,
                context.Palette != null ? context.Palette.StableIdentity : string.Empty);
            TreeDeterministicUtility.Append(
                ref hash,
                context.Palette != null ? context.Palette.PaletteVersion : 0);
            TreeDeterministicUtility.Append(ref hash, context.Parameters.BarkTint);
            TreeDeterministicUtility.Append(ref hash, context.Parameters.FoliageBaseColor);
            TreeDeterministicUtility.Append(ref hash, context.Parameters.FoliageHighlightColor);
            TreeDeterministicUtility.Append(ref hash, context.Parameters.FoliageShadowColor);
            return TreeDeterministicUtility.FormatHash(hash);
        }

        private static string CalculateStructuralFingerprint(
            string trunkHash,
            string branchHash)
        {
            ulong hash = TreeDeterministicUtility.BeginHash();
            TreeDeterministicUtility.Append(ref hash, CurrentGeneratorVersion);
            TreeDeterministicUtility.Append(ref hash, trunkHash);
            TreeDeterministicUtility.Append(ref hash, branchHash);
            return TreeDeterministicUtility.FormatHash(hash);
        }

        private static void AppendResolvedParameters(
            ref ulong hash,
            TreeResolvedParameters p,
            bool includePalette)
        {
            TreeDeterministicUtility.Append(ref hash, (int)p.Family);
            TreeDeterministicUtility.Append(ref hash, p.Height);
            TreeDeterministicUtility.Append(ref hash, p.TrunkBaseRadius);
            TreeDeterministicUtility.Append(ref hash, p.CrownStartHeight);
            TreeDeterministicUtility.Append(ref hash, p.CrownVolume);
            TreeDeterministicUtility.Append(ref hash, p.CrownWidthScale);
            TreeDeterministicUtility.Append(ref hash, p.CrownHeightScale);
            TreeDeterministicUtility.Append(ref hash, p.CrownFill);
            TreeDeterministicUtility.Append(ref hash, p.CrownAsymmetry);
            TreeDeterministicUtility.Append(ref hash, p.CrownLobeCount);
            TreeDeterministicUtility.Append(ref hash, p.CrownLobeRadius);
            TreeDeterministicUtility.Append(ref hash, p.TrunkControlPointCount);
            TreeDeterministicUtility.Append(ref hash, p.TrunkCurvature);
            TreeDeterministicUtility.Append(ref hash, p.TrunkBendCount);
            TreeDeterministicUtility.Append(ref hash, p.TrunkDirectionalDrift);
            TreeDeterministicUtility.Append(ref hash, p.TrunkLeanStrength);
            TreeDeterministicUtility.Append(ref hash, p.TrunkLeanDirectionDegrees);
            TreeDeterministicUtility.Append(ref hash, p.TrunkSurfaceTorsionDegrees);
            TreeDeterministicUtility.Append(ref hash, p.TrunkTwistRidgeCount);
            TreeDeterministicUtility.Append(ref hash, p.TrunkTwistRidgeDepth);
            TreeDeterministicUtility.Append(ref hash, p.RootButtressStrength);
            TreeDeterministicUtility.Append(ref hash, p.RootButtressHeight);
            TreeDeterministicUtility.Append(ref hash, p.RootFlareScale);
            TreeDeterministicUtility.Append(ref hash, p.TrunkSpiralStrength);
            TreeDeterministicUtility.Append(ref hash, p.TrunkSpiralTurns);
            TreeDeterministicUtility.Append(ref hash, p.TrunkSpiralDirection);
            TreeDeterministicUtility.Append(ref hash, p.TrunkIrregularity);
            TreeDeterministicUtility.Append(ref hash, p.TrunkTaper);
            TreeDeterministicUtility.Append(ref hash, p.TrunkForkProbability);
            TreeDeterministicUtility.Append(ref hash, p.TrunkForkHeight);
            TreeDeterministicUtility.Append(ref hash, p.PrimaryBranchCount);
            TreeDeterministicUtility.Append(ref hash, p.PrimaryBranchStartHeight);
            TreeDeterministicUtility.Append(ref hash, p.PrimaryBranchEndHeight);
            TreeDeterministicUtility.Append(ref hash, p.InitialBranchElevationDegrees);
            TreeDeterministicUtility.Append(ref hash, p.BranchArchDirection);
            TreeDeterministicUtility.Append(ref hash, p.BranchArchStrength);
            TreeDeterministicUtility.Append(ref hash, p.LateBranchSag);
            TreeDeterministicUtility.Append(ref hash, p.AzimuthSymmetry);
            TreeDeterministicUtility.Append(ref hash, p.DirectionalBiasAngleDegrees);
            TreeDeterministicUtility.Append(ref hash, p.DirectionalBiasStrength);
            TreeDeterministicUtility.Append(ref hash, p.PrimaryBranchCurvature);
            TreeDeterministicUtility.Append(ref hash, p.PrimaryBranchSideSweep);
            TreeDeterministicUtility.Append(ref hash, p.PrimaryBranchTwistDegrees);
            TreeDeterministicUtility.Append(ref hash, p.PrimaryBranchIrregularity);
            TreeDeterministicUtility.Append(ref hash, p.PrimaryBranchEndCurl);
            TreeDeterministicUtility.Append(ref hash, p.PrimaryBranchLengthRatio);
            TreeDeterministicUtility.Append(ref hash, p.PrimaryBranchRadiusRatio);
            TreeDeterministicUtility.Append(ref hash, p.SecondaryBranchesPerPrimary);
            TreeDeterministicUtility.Append(ref hash, p.TertiaryBranchesPerSecondary);
            TreeDeterministicUtility.Append(ref hash, p.MaximumBranchOrder);
            TreeDeterministicUtility.Append(ref hash, p.SecondaryLengthRatio);
            TreeDeterministicUtility.Append(ref hash, p.TertiaryLengthRatio);
            TreeDeterministicUtility.Append(ref hash, p.HigherOrderCurvatureScale);
            TreeDeterministicUtility.Append(ref hash, p.ClusterWidthScale);
            TreeDeterministicUtility.Append(ref hash, p.ClusterHeightScale);
            TreeDeterministicUtility.Append(ref hash, p.ClusterLengthScale);
            TreeDeterministicUtility.Append(ref hash, p.ClusterRadialSpread);
            TreeDeterministicUtility.Append(ref hash, p.CardSizeScale);
            TreeDeterministicUtility.Append(ref hash, p.FoliageClusterCount);
            TreeDeterministicUtility.Append(ref hash, p.CardsPerCluster);
            TreeDeterministicUtility.Append(ref hash, p.FoliageEligibility);
            TreeDeterministicUtility.Append(ref hash, p.ClusterOccupancy);
            TreeDeterministicUtility.Append(ref hash, p.TerminalFoliageProbability);
            TreeDeterministicUtility.Append(ref hash, p.CardRetentionFraction);
            TreeDeterministicUtility.Append(ref hash, p.MissingBranchProbability);
            TreeDeterministicUtility.Append(ref hash, p.DeadBranchProbability);
            TreeDeterministicUtility.Append(ref hash, p.BreakProbability);
            if (includePalette)
            {
                TreeDeterministicUtility.Append(ref hash, p.BarkTint);
                TreeDeterministicUtility.Append(ref hash, p.FoliageBaseColor);
                TreeDeterministicUtility.Append(ref hash, p.FoliageHighlightColor);
                TreeDeterministicUtility.Append(ref hash, p.FoliageShadowColor);
            }
        }

        private static void AppendBranch(
            ref ulong hash,
            TreeBranchDefinition branch)
        {
            TreeDeterministicUtility.Append(ref hash, branch.StableBranchId);
            TreeDeterministicUtility.Append(ref hash, branch.ParentBranchIndex);
            TreeDeterministicUtility.Append(ref hash, branch.BranchOrder);
            TreeDeterministicUtility.Append(ref hash, branch.ParentAttachmentDistance);
            TreeDeterministicUtility.Append(ref hash, branch.LocalReferenceAxis);
            TreeDeterministicUtility.Append(ref hash, branch.BaseRadius);
            TreeDeterministicUtility.Append(ref hash, branch.EndRadius);
            TreeDeterministicUtility.Append(ref hash, branch.Stiffness);
            TreeDeterministicUtility.Append(ref hash, branch.Phase);
            TreeDeterministicUtility.Append(ref hash, (int)branch.State);
            TreeDeterministicUtility.Append(ref hash, branch.Samples.Count);
            for (int index = 0; index < branch.Samples.Count; index++)
            {
                TreeCurveSample sample = branch.Samples[index];
                TreeDeterministicUtility.Append(ref hash, sample.Position);
                TreeDeterministicUtility.Append(ref hash, sample.Tangent);
                TreeDeterministicUtility.Append(ref hash, sample.Normal);
                TreeDeterministicUtility.Append(ref hash, sample.Binormal);
                TreeDeterministicUtility.Append(ref hash, sample.Radius);
            }
        }

        private static string BuildGenerationReport(
            string timestamp,
            GenerationContext context,
            TreeDefinition definition,
            TreeGenerationMetrics metrics,
            List<string> failures)
        {
            var report = new StringBuilder(24576);
            report.AppendLine("[TREE-GEN.2C Calibrated Deterministic Structural Generation]");
            report.Append("Generated: ").AppendLine(timestamp);
            report.Append("Generator version: ").AppendLine(CurrentGeneratorVersion.ToString());
            report.AppendLine(
                "Output: branch graph, transported curve frames, resolved authoring parameters, fingerprints, and diagnostics only; no bark/foliage meshes are created");
            report.AppendLine();

            report.AppendLine("[Authoring Inputs]");
            report.Append("Family profile: ").AppendLine(context.Profile.StableIdentity);
            report.Append("Family: ").AppendLine(context.Profile.Family.ToString());
            report.Append("Calibration: ").AppendLine(
                context.Calibration != null
                    ? context.Calibration.StableIdentity
                    : "None");
            report.Append("Recipe: ").AppendLine(context.Recipe.StableIdentity);
            report.Append("Age class: ").AppendLine(context.Recipe.AgeClass.ToString());
            report.Append("Master seed: ").AppendLine(context.MasterSeed.ToString());
            report.Append("Instance overrides: ").AppendLine(
                context.InstanceOverrides != null && context.InstanceOverrides.HasAnyOverride
                    ? "Present"
                    : "None");
            report.AppendLine();

            report.AppendLine("[Independent Seed Streams]");
            for (int index = 0; index < context.Seeds.Records.Count; index++)
            {
                TreeSeedRecord record = context.Seeds.Records[index];
                report.Append(record.Locked ? "LOCKED | " : "DERIVED | ")
                    .Append(record.Stream)
                    .Append(" | ")
                    .AppendLine(record.Seed.ToString());
            }
            report.AppendLine();

            AppendResolvedReport(report, context.Parameters);
            report.AppendLine();
            report.AppendLine("[Structure Metrics]");
            report.Append("Branches total/order 1/2/3: ")
                .Append(metrics.BranchCount).Append(" / ")
                .Append(metrics.PrimaryBranchCount).Append(" / ")
                .Append(metrics.SecondaryBranchCount).Append(" / ")
                .AppendLine(metrics.TertiaryBranchCount.ToString());
            report.Append("Rejected/dead/broken/foliage-eligible: ")
                .Append(metrics.RejectedBranchCount).Append(" / ")
                .Append(metrics.DeadBranchCount).Append(" / ")
                .Append(metrics.BrokenBranchCount).Append(" / ")
                .AppendLine(metrics.FoliageEligibleBranchCount.ToString());
            report.Append("Control points / curve samples: ")
                .Append(metrics.ControlPointCount).Append(" / ")
                .AppendLine(metrics.CurveSampleCount.ToString());
            report.Append("Total branch length: ")
                .Append(metrics.TotalBranchLength.ToString("F3"))
                .AppendLine(" m");
            report.Append("Radius range: ")
                .Append(metrics.MinimumRadius.ToString("F4"))
                .Append(" .. ")
                .Append(metrics.MaximumRadius.ToString("F4"))
                .AppendLine(" m");
            report.Append("Generation time: ")
                .Append(metrics.GenerationMilliseconds.ToString("F3"))
                .AppendLine(" ms");
            report.Append("Curve constraints: maxSegmentTurn=")
                .Append(metrics.MaximumSegmentTurnDegrees.ToString("F2"))
                .Append(" maxAccumulatedTurn=")
                .Append(metrics.MaximumAccumulatedTurnDegrees.ToString("F2"))
                .Append(" maxArc/Chord=")
                .Append(metrics.MaximumArcChordRatio.ToString("F3"))
                .Append(" backwardViolations=")
                .AppendLine(metrics.BackwardProgressViolationCount.ToString());
            report.Append("Crown envelope: violatingBranches=")
                .Append(metrics.CrownEnvelopeViolationBranchCount)
                .Append(" maximumOvershoot=")
                .AppendLine(metrics.MaximumCrownEnvelopeOvershoot.ToString("F3"));
            if (context.Branches.Count > 0)
            {
                Bounds reportBounds = definition != null
                    ? definition.LocalBounds
                    : CalculateBounds(context.Branches);
                Vector2 reportFootprint = definition != null
                    ? definition.FootprintExtents
                    : CalculateFootprint(reportBounds);
                report.Append("Local bounds: center=")
                    .Append(reportBounds.center.ToString("F3"))
                    .Append(" size=")
                    .AppendLine(reportBounds.size.ToString("F3"));
                report.Append("Footprint extents X/Z: ")
                    .Append(reportFootprint.x.ToString("F3"))
                    .Append(" / ")
                    .AppendLine(reportFootprint.y.ToString("F3"));
                if (context.Calibration != null)
                {
                    report.Append("Reference target H/W/D: ")
                        .Append(context.Calibration.TargetVisibleHeight.ToString("F3"))
                        .Append(" / ")
                        .Append(context.Calibration.TargetVisibleWidth.ToString("F3"))
                        .Append(" / ")
                        .AppendLine(context.Calibration.TargetVisibleDepth.ToString("F3"));
                    report.Append("Generated/reference H/W/D ratios: ")
                        .Append(metrics.CalibrationHeightRatio.ToString("F3"))
                        .Append(" / ")
                        .Append(metrics.CalibrationWidthRatio.ToString("F3"))
                        .Append(" / ")
                        .Append(metrics.CalibrationDepthRatio.ToString("F3"))
                        .Append(" | tolerance=±")
                        .Append((context.Calibration.DimensionTolerance * 100f).ToString("F1"))
                        .Append("% | ")
                        .AppendLine(metrics.CalibrationWithinTolerance ? "PASS" : "FAIL");
                }
            }
            report.AppendLine();

            report.AppendLine("[Fingerprints]");
            if (definition != null)
            {
                report.Append("Dependency: ").AppendLine(definition.DependencyFingerprint);
                report.Append("Trunk: ").AppendLine(definition.TrunkFingerprint);
                report.Append("Branches: ").AppendLine(definition.BranchFingerprint);
                report.Append("Foliage geometry intent: ").AppendLine(definition.FoliageGeometryFingerprint);
                report.Append("Palette: ").AppendLine(definition.PaletteFingerprint);
                report.Append("Structural: ").AppendLine(definition.StructuralFingerprint);
            }
            report.AppendLine();

            report.AppendLine("[Validation]");
            if (failures.Count == 0)
            {
                report.AppendLine("PASS | Parent indices are backward-only and acyclic.");
                report.AppendLine("PASS | Attachments, lengths, radii, and frames are finite and valid.");
                report.AppendLine("PASS | Stable branch IDs are unique.");
                report.AppendLine("PASS | Structure remained inside profile branch/sample budgets.");
                if (context.Calibration != null)
                {
                    report.AppendLine("PASS | Reference calibration stayed inside its dimension tolerance.");
                }
                else
                {
                    report.AppendLine("PASS | Reference calibration not applicable to this recipe.");
                }
                report.AppendLine("PASS | Curve progression and transported-frame diagnostics were recorded.");
            }
            else
            {
                for (int index = 0; index < failures.Count; index++)
                {
                    report.Append("FAIL | ").AppendLine(failures[index]);
                }
            }

            for (int index = 0; index < context.Warnings.Count; index++)
            {
                report.Append("WARNING | ").AppendLine(context.Warnings[index]);
            }

            report.AppendLine();
            report.Append("Status: ").AppendLine(failures.Count == 0 ? "PASS" : "FAIL");
            return report.ToString();
        }

        private static void AppendResolvedReport(
            StringBuilder report,
            TreeResolvedParameters p)
        {
            report.AppendLine("[Resolved Parameters]");
            report.Append("Height / trunk radius: ")
                .Append(p.Height.ToString("F3")).Append(" / ")
                .AppendLine(p.TrunkBaseRadius.ToString("F3"));
            report.Append("Trunk controls: points=")
                .Append(p.TrunkControlPointCount)
                .Append(" curvature=").Append(p.TrunkCurvature.ToString("F3"))
                .Append(" bends=").Append(p.TrunkBendCount.ToString("F3"))
                .Append(" drift=").Append(p.TrunkDirectionalDrift.ToString("F3"))
                .Append(" lean=").Append(p.TrunkLeanStrength.ToString("F3"))
                .Append(" yaw=").Append(p.TrunkLeanDirectionDegrees.ToString("F1"))
                .Append(" twistDegrees=").Append(p.TrunkSurfaceTorsionDegrees.ToString("F1"))
                .Append(" ridges=").Append(p.TrunkTwistRidgeCount)
                .Append(" ridgeDepth=").Append(p.TrunkTwistRidgeDepth.ToString("F3"))
                .Append(" buttress=").Append(p.RootButtressStrength.ToString("F3"))
                .Append(" buttressHeight=").Append(p.RootButtressHeight.ToString("F3"))
                .Append(" rootFlare=").Append(p.RootFlareScale.ToString("F3"))
                .Append(" pathSpiral=").Append(p.TrunkSpiralStrength.ToString("F3"))
                .Append(" turns=").Append(p.TrunkSpiralTurns.ToString("F2"))
                .Append(" direction=").AppendLine(p.TrunkSpiralDirection > 0f ? "CCW" : "CW");
            report.Append("Primary branches: count=")
                .Append(p.PrimaryBranchCount)
                .Append(" start/end=").Append(p.PrimaryBranchStartHeight.ToString("F3"))
                .Append("/").Append(p.PrimaryBranchEndHeight.ToString("F3"))
                .Append(" elevation=").Append(p.InitialBranchElevationDegrees.ToString("F1"))
                .Append(" arch=").Append(p.BranchArchDirection.ToString("F2"))
                .Append("x").Append(p.BranchArchStrength.ToString("F3"))
                .Append(" lateSag=").Append(p.LateBranchSag.ToString("F3"))
                .Append(" symmetry=").Append(p.AzimuthSymmetry.ToString("F3"))
                .Append(" bias=").Append(p.DirectionalBiasAngleDegrees.ToString("F1"))
                .Append("@").Append(p.DirectionalBiasStrength.ToString("F3"))
                .Append(" curvature=").Append(p.PrimaryBranchCurvature.ToString("F3"))
                .Append(" lengthRatio=").AppendLine(p.PrimaryBranchLengthRatio.ToString("F3"));
            report.Append("Higher orders: max=")
                .Append(p.MaximumBranchOrder)
                .Append(" secondary/primary=").Append(p.SecondaryBranchesPerPrimary)
                .Append(" tertiary/secondary=").AppendLine(p.TertiaryBranchesPerSecondary.ToString());
            report.Append("Crown volume: overall=")
                .Append(p.CrownVolume.ToString("F3"))
                .Append(" width=").Append(p.CrownWidthScale.ToString("F3"))
                .Append(" height=").Append(p.CrownHeightScale.ToString("F3"))
                .Append(" fill=").Append(p.CrownFill.ToString("F3"))
                .Append(" lobes=").AppendLine(p.CrownLobeCount.ToString());
            report.Append("Foliage density: clusters=")
                .Append(p.FoliageClusterCount)
                .Append(" cards/cluster=").Append(p.CardsPerCluster)
                .Append(" eligibility=").Append(p.FoliageEligibility.ToString("F3"))
                .Append(" occupancy=").AppendLine(p.ClusterOccupancy.ToString("F3"));
            report.Append("Bark tint: ").AppendLine(p.BarkTint.ToString());
            report.Append("Foliage base/highlight/shadow: ")
                .Append(p.FoliageBaseColor).Append(" / ")
                .Append(p.FoliageHighlightColor).Append(" / ")
                .AppendLine(p.FoliageShadowColor.ToString());
            report.AppendLine("Ownership trace:");
            for (int index = 0; index < p.OwnershipTrace.Count; index++)
            {
                report.Append("  - ").AppendLine(p.OwnershipTrace[index]);
            }
        }

        private static TreeGenerationResult CreateFailure(
            string timestamp,
            List<string> failures,
            bool includeFullReport)
        {
            return new TreeGenerationResult
            {
                Passed = false,
                Definition = null,
                Timestamp = timestamp,
                Report = includeFullReport
                    ? BuildFailureSummary(timestamp, failures)
                    : string.Join("\n", failures)
            };
        }

        private static string BuildFailureSummary(
            string timestamp,
            List<string> failures)
        {
            var report = new StringBuilder(4096);
            report.AppendLine("[TREE-GEN.2C Calibrated Deterministic Structural Generation]");
            report.Append("Generated: ").AppendLine(timestamp);
            report.AppendLine("[Validation]");
            for (int index = 0; index < failures.Count; index++)
            {
                report.Append("FAIL | ").AppendLine(failures[index]);
            }
            report.AppendLine("Status: FAIL");
            return report.ToString();
        }

        private static int StableBranchId(
            GenerationContext context,
            int parentStableId,
            int order,
            int ordinal)
        {
            return TreeDeterministicUtility.DeriveSeed(
                context.MasterSeed,
                context.Profile.Family,
                parentStableId,
                order,
                ordinal,
                CurrentGeneratorVersion);
        }

        private static float CalculateBranchLength(TreeBranchDefinition branch)
        {
            float length = 0f;
            for (int index = 1; index < branch.Samples.Count; index++)
            {
                length += Vector3.Distance(
                    branch.Samples[index - 1].Position,
                    branch.Samples[index].Position);
            }
            return length;
        }

        private static bool IsFinite(Vector3 value)
        {
            return TreeDeterministicUtility.IsFinite(value.x) &&
                TreeDeterministicUtility.IsFinite(value.y) &&
                TreeDeterministicUtility.IsFinite(value.z);
        }

        private static bool IsFiniteColor(Color value)
        {
            return TreeDeterministicUtility.IsFinite(value.r) &&
                TreeDeterministicUtility.IsFinite(value.g) &&
                TreeDeterministicUtility.IsFinite(value.b) &&
                TreeDeterministicUtility.IsFinite(value.a);
        }

        private static Vector2 DirectionFromDegrees(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private static float ResolveSpiralDirection(
            TreeFloatRange range,
            int seed,
            string key)
        {
            float sampled = range.Sample(seed, key);
            if (Mathf.Abs(sampled) >= 0.5f)
            {
                return Mathf.Sign(sampled);
            }

            return TreeDeterministicUtility.Sample01(seed, key + "-handedness") < 0.5f
                ? -1f
                : 1f;
        }

        private static float ResolveOverrideDirection(
            TreeFloatOverride value,
            float inherited,
            int seed,
            string key)
        {
            if (!value.IsSet)
            {
                return inherited;
            }

            float resolved = value.Resolve(inherited, seed, key);
            if (Mathf.Abs(resolved) >= 0.5f)
            {
                return Mathf.Sign(resolved);
            }

            return TreeDeterministicUtility.Sample01(seed, key + "-handedness") < 0.5f
                ? -1f
                : 1f;
        }

        private static float Sample(
            TreeFloatRange range,
            TreeSeedSet seeds,
            TreeSeedStream stream,
            string key)
        {
            return range.Sample(seeds.GetSeed(stream), key);
        }

        private static int Sample(
            TreeIntRange range,
            TreeSeedSet seeds,
            TreeSeedStream stream,
            string key)
        {
            return range.Sample(seeds.GetSeed(stream), key);
        }

        private static void ValidateInside(
            float value,
            TreeFloatRange range,
            string label,
            List<string> failures)
        {
            if (!TreeDeterministicUtility.IsFinite(value) ||
                value < range.Minimum - 0.0001f ||
                value > range.Maximum + 0.0001f)
            {
                failures.Add(
                    label + "=" + value.ToString("F4") +
                    " is outside profile range [" +
                    range.Minimum.ToString("F4") + ", " +
                    range.Maximum.ToString("F4") + "].");
            }
        }

        private static void ValidateInside(
            int value,
            TreeIntRange range,
            string label,
            List<string> failures)
        {
            if (value < range.Minimum || value > range.Maximum)
            {
                failures.Add(
                    label + "=" + value +
                    " is outside profile range [" +
                    range.Minimum + ", " + range.Maximum + "].");
            }
        }

        private static void ValidateProbability(
            float value,
            string label,
            List<string> failures)
        {
            if (!TreeDeterministicUtility.IsFinite(value) || value < 0f || value > 1f)
            {
                failures.Add(label + " must remain inside [0,1].");
            }
        }

        private static TreeGenerationOverrides CloneOverrides(
            TreeGenerationOverrides source)
        {
            return source != null ? source.Clone() : new TreeGenerationOverrides();
        }

        private static bool TryGenerateBranchCountIsolationCandidate(
            TreeGenerationRecipe recipe,
            TreeGenerationOverrides instanceOverrides,
            int masterSeed,
            TreeDefinition baseline,
            out int requestedBranchCount,
            out TreeGenerationResult selectedResult)
        {
            requestedBranchCount =
                baseline.ResolvedParameters.PrimaryBranchCount;
            selectedResult = null;
            TreeIntRange allowed =
                recipe.FamilyProfile.PrimaryBranches.Count;
            int current = baseline.ResolvedParameters.PrimaryBranchCount;
            int maximumDistance = Mathf.Max(
                current - allowed.Minimum,
                allowed.Maximum - current);

            // Removing accepted branch requests is tested before adding new
            // requests because structural-damage rejection may make a +1
            // request produce the same accepted branch graph.
            for (int distance = 1;
                 distance <= maximumDistance;
                 distance++)
            {
                int lower = current - distance;
                if (lower >= allowed.Minimum)
                {
                    requestedBranchCount = lower;
                }
                if (lower >= allowed.Minimum &&
                    TryBranchCountCandidate(
                        recipe,
                        instanceOverrides,
                        masterSeed,
                        baseline,
                        lower,
                        out selectedResult))
                {
                    requestedBranchCount = lower;
                    return true;
                }

                int upper = current + distance;
                if (upper <= allowed.Maximum)
                {
                    requestedBranchCount = upper;
                }
                if (upper <= allowed.Maximum &&
                    TryBranchCountCandidate(
                        recipe,
                        instanceOverrides,
                        masterSeed,
                        baseline,
                        upper,
                        out selectedResult))
                {
                    requestedBranchCount = upper;
                    return true;
                }
            }

            return false;
        }

        private static bool TryBranchCountCandidate(
            TreeGenerationRecipe recipe,
            TreeGenerationOverrides instanceOverrides,
            int masterSeed,
            TreeDefinition baseline,
            int candidateCount,
            out TreeGenerationResult result)
        {
            TreeGenerationOverrides branchOverrides =
                CloneOverrides(instanceOverrides);
            branchOverrides.SetPrimaryBranchCountForTest(candidateCount);
            result = GenerateInternal(
                recipe,
                branchOverrides,
                masterSeed,
                false);
            return
                result.Passed &&
                result.Definition.ResolvedParameters.PrimaryBranchCount !=
                    baseline.ResolvedParameters.PrimaryBranchCount &&
                result.Definition.TrunkFingerprint ==
                    baseline.TrunkFingerprint &&
                result.Definition.BranchFingerprint !=
                    baseline.BranchFingerprint &&
                result.Definition.PaletteFingerprint ==
                    baseline.PaletteFingerprint;
        }

        private static float ChooseDifferentValue(
            float current,
            TreeFloatRange allowed)
        {
            float span = allowed.Maximum - allowed.Minimum;
            if (span <= 0.0001f)
            {
                return current;
            }

            float candidate = current + span * 0.2f;
            if (candidate > allowed.Maximum)
            {
                candidate = current - span * 0.2f;
            }
            return Mathf.Clamp(candidate, allowed.Minimum, allowed.Maximum);
        }

        private static int ChooseDifferentValue(
            int current,
            TreeIntRange allowed)
        {
            if (allowed.Maximum <= allowed.Minimum)
            {
                return current;
            }

            return current < allowed.Maximum ? current + 1 : current - 1;
        }

        private static bool AppendBranchChangeTest(
            StringBuilder report,
            string label,
            TreeGenerationResult result,
            TreeDefinition baseline)
        {
            bool passed =
                result.Passed &&
                result.Definition.TrunkFingerprint == baseline.TrunkFingerprint &&
                result.Definition.BranchFingerprint != baseline.BranchFingerprint &&
                result.Definition.PaletteFingerprint == baseline.PaletteFingerprint;
            return AppendTest(
                report,
                label,
                passed,
                baseline.TrunkFingerprint + " / " + baseline.BranchFingerprint +
                    " / " + baseline.PaletteFingerprint,
                result.Passed
                    ? result.Definition.TrunkFingerprint + " / " +
                      result.Definition.BranchFingerprint + " / " +
                      result.Definition.PaletteFingerprint
                    : "Generation failed");
        }

        private static bool AppendTwistChangeTest(
            StringBuilder report,
            TreeGenerationResult result,
            TreeDefinition baseline,
            string baselineBarkInput,
            TreeBarkMeshSettings settings)
        {
            bool passed = result != null &&
                result.Passed &&
                result.Definition.StructuralFingerprint !=
                    baseline.StructuralFingerprint &&
                result.Definition.FoliageGeometryFingerprint ==
                    baseline.FoliageGeometryFingerprint &&
                result.Definition.PaletteFingerprint ==
                    baseline.PaletteFingerprint;
            string changedBarkInput = result != null && result.Passed
                ? TreeBarkMeshGenerator.CalculateInputFingerprint(
                    result.Definition,
                    settings)
                : "Generation failed";
            passed &= changedBarkInput != baselineBarkInput;
            return AppendTest(
                report,
                "Trunk-twist changes retain the existing structural-frame response and change bark input",
                passed,
                baseline.StructuralFingerprint + " / bark=" + baselineBarkInput,
                result != null && result.Passed
                    ? result.Definition.StructuralFingerprint +
                      " / bark=" + changedBarkInput
                    : "Generation failed");
        }

        private static bool AppendBarkOnlyChangeTest(
            StringBuilder report,
            string label,
            TreeGenerationResult result,
            TreeDefinition baseline,
            string baselineBarkInput,
            TreeBarkMeshSettings settings)
        {
            bool passed = result != null &&
                result.Passed &&
                result.Definition.TrunkFingerprint == baseline.TrunkFingerprint &&
                result.Definition.BranchFingerprint == baseline.BranchFingerprint &&
                result.Definition.FoliageGeometryFingerprint == baseline.FoliageGeometryFingerprint &&
                result.Definition.PaletteFingerprint == baseline.PaletteFingerprint;
            string changedBarkInput = result != null && result.Passed
                ? TreeBarkMeshGenerator.CalculateInputFingerprint(
                    result.Definition,
                    settings)
                : "Generation failed";
            passed &= changedBarkInput != baselineBarkInput;
            return AppendTest(
                report,
                label,
                passed,
                baseline.StructuralFingerprint + " / bark=" + baselineBarkInput,
                result != null && result.Passed
                    ? result.Definition.StructuralFingerprint + " / bark=" + changedBarkInput
                    : "Generation failed");
        }

        private static bool AppendIsolationTest(
            StringBuilder report,
            string label,
            TreeGenerationResult result,
            TreeDefinition baseline,
            bool requireTrunkSame,
            bool requireBranchesSame,
            bool requireFoliageSame,
            bool requirePaletteSame)
        {
            bool passed = result.Passed;
            if (passed)
            {
                TreeDefinition changed = result.Definition;
                passed &= !requireTrunkSame || changed.TrunkFingerprint == baseline.TrunkFingerprint;
                passed &= !requireBranchesSame || changed.BranchFingerprint == baseline.BranchFingerprint;
                passed &= !requireFoliageSame || changed.FoliageGeometryFingerprint == baseline.FoliageGeometryFingerprint;
                passed &= !requirePaletteSame || changed.PaletteFingerprint == baseline.PaletteFingerprint;
                if (!requireFoliageSame)
                {
                    passed &= changed.FoliageGeometryFingerprint != baseline.FoliageGeometryFingerprint;
                }
                if (!requirePaletteSame)
                {
                    passed &= changed.PaletteFingerprint != baseline.PaletteFingerprint;
                }
            }

            return AppendTest(
                report,
                label,
                passed,
                baseline.StructuralFingerprint,
                result.Passed ? result.Definition.StructuralFingerprint : "Generation failed");
        }

        private static bool AppendTest(
            StringBuilder report,
            string label,
            bool passed,
            string expected,
            string actual)
        {
            report.Append(passed ? "PASS | " : "FAIL | ")
                .AppendLine(label);
            if (!passed)
            {
                report.Append("  baseline: ").AppendLine(expected ?? string.Empty);
                report.Append("  changed:  ").AppendLine(actual ?? string.Empty);
            }
            return passed;
        }

        private static bool ValidateLockedSeedIsolation(
            StringBuilder report,
            TreeGenerationRecipe recipe,
            int masterSeed,
            TreeSeedSet baselineSeeds)
        {
            bool foundLocked = false;
            TreeSeedStream lockedStream = TreeSeedStream.TrunkShape;
            int lockedValue = 0;
            for (int index = 0; index < recipe.SeedLocks.Count; index++)
            {
                TreeSeedLock candidate = recipe.SeedLocks[index];
                if (!candidate.Locked)
                {
                    continue;
                }

                foundLocked = true;
                lockedStream = candidate.Stream;
                lockedValue = candidate.Seed;
                break;
            }

            if (!foundLocked)
            {
                report.AppendLine(
                    "PASS | Locked-seed isolation test not applicable: recipe has no locked streams; all streams still use independent derivation keys.");
                return true;
            }

            bool foundUnlocked = false;
            TreeSeedStream comparisonStream = TreeSeedStream.PrimaryBranchLayout;
            Array streams = Enum.GetValues(typeof(TreeSeedStream));
            for (int index = 0; index < streams.Length; index++)
            {
                var candidate = (TreeSeedStream)streams.GetValue(index);
                if (candidate == lockedStream ||
                    recipe.TryGetLockedSeed(candidate, out _))
                {
                    continue;
                }

                comparisonStream = candidate;
                foundUnlocked = true;
                break;
            }

            if (!foundUnlocked)
            {
                report.AppendLine(
                    "PASS | Locked-seed isolation has no unlocked comparison stream because every stream is locked.");
                return true;
            }

            var context = new GenerationContext
            {
                Recipe = recipe,
                Profile = recipe.FamilyProfile,
                Calibration = recipe.ReferenceCalibration,
                Palette = recipe.ResolvePalette(),
                MasterSeed = unchecked(masterSeed + 104729)
            };
            TreeSeedSet changedSeeds = BuildSeedSet(context);
            bool lockedSame = changedSeeds.GetSeed(lockedStream) == lockedValue &&
                baselineSeeds.GetSeed(lockedStream) == lockedValue;
            bool unlockedChanged = changedSeeds.GetSeed(comparisonStream) !=
                baselineSeeds.GetSeed(comparisonStream);
            return AppendTest(
                report,
                "Locked subsystem seed remains stable while an unlocked subsystem changes",
                lockedSame && unlockedChanged,
                lockedStream + "=" + lockedValue,
                lockedStream + "=" + changedSeeds.GetSeed(lockedStream) +
                "; " + comparisonStream + " changed=" + unlockedChanged);
        }

    }
}
