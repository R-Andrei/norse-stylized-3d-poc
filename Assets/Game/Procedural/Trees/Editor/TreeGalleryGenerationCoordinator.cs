using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal sealed class TreeUnifiedGalleryBuildResult
    {
        internal bool Passed;
        internal int GeneratedTreeCount;
        internal string Timestamp;
        internal string Report;
    }

    internal static class TreeGalleryGenerationCoordinator
    {
        internal static TreeUnifiedGalleryBuildResult Rebuild(
            TreeReferenceGallery gallery)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var report = new StringBuilder(16384);
            report.AppendLine(
                "[TREE-GEN.2C.3H5R1 Exact H5 Regression Revert Build]");
            report.Append("Generated: ").AppendLine(timestamp);
            report.AppendLine(
                "Workflow: one action audits and repairs source imports, rebuilds all twenty imported references and procedural slots, migrates exact H5 managed Common/Twisted values back to the live-passed H4 targets, restores the H4 rounded body/quadratic-foot geometry, retains H4 shoulder narrowing and ten-sample root lobes, and validates deterministic repeatability and selective regeneration.");
            report.AppendLine(
                "Output: all twenty procedural slots receive ProceduralTreeInstance structural previews; Common 1, Pine 1, Twisted 1, and Dead 1 receive the H4 bark geometry and managed root targets. H5 smootherstep body/compact-foot envelopes and H5 Common/Twisted mass values are removed. Branch visual work remains pending; foliage meshes remain TREE-GEN.3 work.");
            report.AppendLine();

            if (gallery == null)
            {
                return Failure(report, timestamp, "Tree reference gallery is null.");
            }

            if (gallery.ReferenceGround == null &&
                !TreeReferenceGalleryBuilder.AssignClosestGround(
                    gallery,
                    out string groundAssignment))
            {
                return Failure(report, timestamp, groundAssignment);
            }

            TreeSourceAuditResult audit = TreeSourceAssetAudit.Run(gallery);
            Undo.RecordObject(gallery, "Record Unified Tree Source Audit");
            gallery.RecordSourceAudit(
                audit.Passed,
                audit.SourceFolderAvailable,
                audit.FoundModelCount,
                audit.FoundTextureCount,
                audit.Timestamp,
                audit.Report);
            EditorUtility.SetDirty(gallery);
            report.AppendLine("[Source Audit]");
            report.Append(audit.Passed ? "PASS" : "FAIL")
                .Append(" | models=")
                .Append(audit.FoundModelCount)
                .Append("/20 | textures=")
                .Append(audit.FoundTextureCount)
                .AppendLine("/12");
            if (!audit.Passed)
            {
                return Failure(
                    report,
                    timestamp,
                    "Complete tree source audit failed. Use the copied report for details.");
            }

            bool existingGallery =
                TreeReferenceGalleryBuilder.HasCompleteGallery(gallery);
            TreeGalleryBuildResult galleryResult =
                TreeReferenceGalleryBuilder.BuildCompleteGallery(
                    gallery,
                    existingGallery);
            gallery.RecordCompleteGalleryBuild(
                galleryResult.Passed,
                galleryResult.SpecimenCount,
                galleryResult.Timestamp,
                galleryResult.Report);
            EditorUtility.SetDirty(gallery);
            report.AppendLine();
            report.AppendLine("[Reference Gallery]");
            report.Append(galleryResult.Passed ? "PASS" : "FAIL")
                .Append(" | mode=")
                .Append(existingGallery ? "Rebuild" : "Build")
                .Append(" | specimens/slots=")
                .Append(galleryResult.SpecimenCount)
                .AppendLine();
            if (!galleryResult.Passed)
            {
                return Failure(
                    report,
                    timestamp,
                    "Complete imported reference gallery build failed.");
            }

            Transform completeRoot = gallery.transform.Find(
                TreeReferenceGalleryBuilder.CompleteGalleryRootName);
            if (completeRoot == null)
            {
                return Failure(
                    report,
                    timestamp,
                    "Complete gallery root was not found after a passing rebuild.");
            }

            List<TreeReferenceSpecimen> slots = CollectProceduralSlots(
                completeRoot);
            if (!TreeGenerationLibraryBuilder.EnsureLibrary(
                    gallery,
                    slots,
                    report,
                    out TreeGenerationLibrary library,
                    out string libraryFailure))
            {
                return Failure(report, timestamp, libraryFailure);
            }

            report.AppendLine();
            report.AppendLine("[Procedural Slot Generation]");
            int generatedCount = 0;
            int deterministicCount = 0;
            var barkRepresentatives = new List<ProceduralTreeInstance>(4);
            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                TreeReferenceSpecimen slot = slots[slotIndex];
                TreeGenerationLibraryVariant variant = library.FindVariant(
                    slot.Family,
                    slot.SourceVariantIndex);
                if (variant == null || variant.Recipe == null)
                {
                    return Failure(
                        report,
                        timestamp,
                        "No managed recipe exists for " + slot.Family + " " +
                        slot.SourceVariantIndex + ".");
                }

                ProceduralTreeInstance instance =
                    slot.GetComponent<ProceduralTreeInstance>();
                if (instance == null)
                {
                    instance = Undo.AddComponent<ProceduralTreeInstance>(
                        slot.gameObject);
                }
                else
                {
                    Undo.RecordObject(
                        instance,
                        "Rebuild Procedural Tree Structure");
                }

                instance.ConfigureManagedBinding(library, variant);
                instance.SetPreviewSettings(
                    gallery.ShowGeneratedStructuralPreviews,
                    gallery.GeneratedPreviewScope,
                    gallery.ShowGeneratedTrunk,
                    gallery.ShowGeneratedPrimaryBranches,
                    gallery.ShowGeneratedHigherOrderBranches,
                    gallery.ShowGeneratedAttachmentPoints,
                    gallery.ShowGeneratedBounds,
                    gallery.ShowGeneratedTransportedFrames);
                TreeGenerationResult generation = instance.GenerateStructure();
                EditorUtility.SetDirty(instance);
                if (!generation.Passed ||
                    generation.Definition == null ||
                    !generation.Definition.IsValid)
                {
                    report.Append("FAIL | ")
                        .Append(slot.Family)
                        .Append(" ")
                        .Append(slot.SourceVariantIndex)
                        .AppendLine(" | structural generation failed.");
                    report.AppendLine(generation.Report ?? string.Empty);
                    return Failure(
                        report,
                        timestamp,
                        "Structural generation failed for " + slot.Family +
                        " " + slot.SourceVariantIndex + ".");
                }

                TreeGenerationResult repeat = TreeGenerator.Generate(
                    variant.Recipe,
                    instance.InstanceOverrides,
                    instance.MasterSeed);
                bool deterministic = repeat.Passed &&
                    repeat.Definition != null &&
                    repeat.Definition.StructuralFingerprint ==
                    generation.Definition.StructuralFingerprint;
                if (deterministic)
                {
                    deterministicCount++;
                }

                generatedCount++;
                report.Append(deterministic ? "PASS" : "FAIL")
                    .Append(" | ")
                    .Append(slot.Family)
                    .Append(" ")
                    .Append(slot.SourceVariantIndex)
                    .Append(" | seed=")
                    .Append(instance.MasterSeed)
                    .Append(" | branches=")
                    .Append(generation.Definition.Metrics.BranchCount)
                    .Append(" | bounds=")
                    .Append(generation.Definition.LocalBounds.size.ToString("F3"))
                    .Append(" | refRatioHWD=")
                    .Append(generation.Definition.Metrics.CalibrationHeightRatio.ToString("F3"))
                    .Append("/")
                    .Append(generation.Definition.Metrics.CalibrationWidthRatio.ToString("F3"))
                    .Append("/")
                    .Append(generation.Definition.Metrics.CalibrationDepthRatio.ToString("F3"))
                    .Append(" | curve=")
                    .Append(generation.Definition.Metrics.MaximumArcChordRatio.ToString("F3"))
                    .Append(" | structural=")
                    .Append(generation.Definition.StructuralFingerprint)
                    .AppendLine();
                if (!deterministic)
                {
                    return Failure(
                        report,
                        timestamp,
                        "Repeat generation fingerprint mismatch for " +
                        slot.Family + " " + slot.SourceVariantIndex + ".");
                }

                if (slot.SourceVariantIndex == 1)
                {
                    barkRepresentatives.Add(instance);
                }
            }

            report.AppendLine();
            report.AppendLine("[H5 Regression Revert — H4 Buttress Shoulder Vertical Slice]");
            int barkMeshCount = 0;
            var aggregateFailures = new List<string>();
            var pendingBranchDiagnostics = new List<string>();
            for (int index = 0; index < barkRepresentatives.Count; index++)
            {
                ProceduralTreeInstance representative =
                    barkRepresentatives[index];
                if (!TreeBarkMeshAssetBuilder.BuildOrUpdate(
                        gallery,
                        library,
                        representative,
                        out TreeBarkMeshBuildResult barkResult,
                        out string barkReport,
                        out string barkFailure))
                {
                    report.Append("FAIL | ")
                        .Append(representative.Family)
                        .Append(" 1 | ")
                        .AppendLine(barkFailure);
                    if (!string.IsNullOrEmpty(barkReport))
                    {
                        report.AppendLine(barkReport);
                    }

                    aggregateFailures.Add(
                        "Bark mesh generation failed for " +
                        representative.Family + " 1.");
                    continue;
                }

                barkMeshCount++;
                report.Append("PASS | ")
                    .Append(representative.Family)
                    .Append(" 1 | branches=")
                    .Append(barkResult.MeshedBranchCount)
                    .Append(" | vertices=")
                    .Append(barkResult.VertexCount)
                    .Append(" | triangles=")
                    .Append(barkResult.TriangleCount)
                    .Append(" | caps=")
                    .Append(barkResult.TipCapCount)
                    .Append(" | altDiagonals=")
                    .Append(barkResult.AlternateQuadDiagonalCount)
                    .Append(" | phaseRings=")
                    .Append(barkResult.PhaseAlignedRingCount)
                    .Append(" | radiusClamps=")
                    .Append(barkResult.CurvatureRadiusClampCount)
                    .Append(" | collapsedRings=")
                    .Append(barkResult.CircularBranchRingRemovalCount)
                    .Append(" | trunkSegments=")
                    .Append(barkResult.EffectiveTrunkRadialSegments)
                    .Append(" | trunkRings=")
                    .Append(barkResult.EffectiveTrunkRingCount)
                    .Append(" | rootIntervals=")
                    .Append(barkResult.RootZoneLongitudinalIntervals)
                    .Append(" | buttresses=")
                    .Append(representative.GeneratedDefinition.ResolvedParameters.RootButtressCount)
                    .Append(" | samplesPerLobe=")
                    .Append(barkResult.ButtressSamplesPerLobe.ToString("F2"))
                    .Append(" | pathSpiral=")
                    .Append(barkResult.PathSpiralStrength.ToString("F3"))
                    .Append("x")
                    .Append(barkResult.PathSpiralTurns.ToString("F2"))
                    .Append(barkResult.PathSpiralDirection < 0f ? "CW" : "CCW")
                    .Append("@")
                    .Append(barkResult.MaximumPathSpiralRadius.ToString("F3"))
                    .Append(" | crossSection=")
                    .Append(barkResult.MaximumCrossSectionMultiplier.ToString("F3"))
                    .Append(" | rootProfile=")
                    .Append(barkResult.GroundButtressCrestMultiplier.ToString("F3"))
                    .Append("/")
                    .Append(barkResult.MinimumGroundCrossSectionMultiplier.ToString("F3"))
                    .Append("/")
                    .Append(barkResult.HalfHeightRootExtensionRatio.ToString("F3"))
                    .Append("/")
                    .Append(barkResult.HalfHeightButtressAngularWidthScale.ToString("F3"))
                    .Append("/")
                    .Append(barkResult.MaximumGroundButtressCrestTurnDegrees.ToString("F2"))
                    .Append(" | rootWD=")
                    .Append(barkResult.GeneratedRootWidth.ToString("F3"))
                    .Append("/")
                    .Append(barkResult.GeneratedRootDepth.ToString("F3"))
                    .Append(" | twistReq/Measured/Error=")
                    .Append(barkResult.RequestedAxialTwistDegrees.ToString("F2"))
                    .Append("/")
                    .Append(barkResult.MeasuredAxialTwistDegrees.ToString("F2"))
                    .Append("/")
                    .Append(barkResult.AxialTwistErrorDegrees.ToString("F3"))
                    .Append(" | branchTurn=")
                    .Append(CalculateMaximumNonTrunkSegmentTurn(
                        representative.GeneratedDefinition).ToString("F2"))
                    .Append(" | bounds=")
                    .Append(barkResult.LocalBounds.size.ToString("F3"))
                    .Append(" | repeat=")
                    .Append(barkResult.RepeatabilityPassed ? "PASS" : "FAIL")
                    .Append(" | exposedLoops=")
                    .Append(barkResult.TopologyAudit != null
                        ? barkResult.TopologyAudit.UnexpectedExposedBoundaryLoopCount
                        : -1)
                    .Append(" | inward=")
                    .Append(barkResult.TopologyAudit != null
                        ? barkResult.TopologyAudit.InwardSideTriangleCount
                        : -1)
                    .Append(" | mesh=")
                    .AppendLine(barkResult.GeometryFingerprint);
                report.AppendLine(barkReport);

                bool rootBearingRepresentative =
                    representative.Family == TreeFamily.Common ||
                    representative.Family == TreeFamily.Pine ||
                    representative.Family == TreeFamily.Twisted ||
                    representative.Family == TreeFamily.Dead;
                if (rootBearingRepresentative &&
                    barkResult.ButtressSamplesPerLobe + 0.0001f < 10f)
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 emitted only " +
                        barkResult.ButtressSamplesPerLobe.ToString("F2") +
                        " buttress samples per lobe; required at least 10.00.");
                }

                if (rootBearingRepresentative &&
                    barkResult.RootZoneLongitudinalIntervals < 10)
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 emitted only " +
                        barkResult.RootZoneLongitudinalIntervals +
                        " root-zone longitudinal intervals; required at least 10.");
                }

                if (rootBearingRepresentative &&
                    (barkResult.MinimumGroundCrossSectionMultiplier < 0.995f ||
                     barkResult.MinimumGroundCrossSectionMultiplier > 1.005f))
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 ground valley multiplier reached " +
                        barkResult.MinimumGroundCrossSectionMultiplier.ToString("F3") +
                        "; valley-safe root reconstruction requires 0.995 through 1.005.");
                }

                if (rootBearingRepresentative &&
                    barkResult.RootTopRootOnlyMultiplier > 0.005f)
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 retained " +
                        barkResult.RootTopRootOnlyMultiplier.ToString("F4") +
                        " root-only contribution at Root Buttress Height; maximum is 0.0050.");
                }

                if (rootBearingRepresentative &&
                    barkResult.MaximumGroundButtressCrestTurnDegrees > 22.001f)
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 ground buttress crest turn reached " +
                        barkResult.MaximumGroundButtressCrestTurnDegrees.ToString("F2") +
                        " degrees; rounded-crest maximum is 22.00.");
                }

                if (rootBearingRepresentative &&
                    (barkResult.HalfHeightRootExtensionRatio < 0.2699f ||
                     barkResult.HalfHeightRootExtensionRatio > 0.3501f))
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 half-height/ground root-extension ratio reached " +
                        barkResult.HalfHeightRootExtensionRatio.ToString("F3") +
                        "; required range is 0.27 through 0.35.");
                }

                if (rootBearingRepresentative &&
                    (barkResult.HalfHeightButtressAngularWidthScale < 0.7999f ||
                     barkResult.HalfHeightButtressAngularWidthScale > 0.8001f))
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 half-height buttress angular-width scale reached " +
                        barkResult.HalfHeightButtressAngularWidthScale.ToString("F4") +
                        "; H5R1 restores the H4 requirement of 0.8000.");
                }

                float minimumGroundCrest = 0f;
                float maximumGroundCrest = float.PositiveInfinity;
                if (representative.Family == TreeFamily.Common)
                {
                    minimumGroundCrest = 1.72f;
                    maximumGroundCrest = 1.80f;
                }
                else if (representative.Family == TreeFamily.Pine)
                {
                    minimumGroundCrest = 1.30f;
                    maximumGroundCrest = 1.36f;
                }
                else if (representative.Family == TreeFamily.Twisted)
                {
                    minimumGroundCrest = 1.90f;
                    maximumGroundCrest = 2.05f;
                }
                else if (representative.Family == TreeFamily.Dead)
                {
                    minimumGroundCrest = 1.84f;
                    maximumGroundCrest = 1.98f;
                }

                if (rootBearingRepresentative &&
                    (barkResult.GroundButtressCrestMultiplier < minimumGroundCrest ||
                     barkResult.GroundButtressCrestMultiplier > maximumGroundCrest))
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 ground buttress crest multiplier reached " +
                        barkResult.GroundButtressCrestMultiplier.ToString("F3") +
                        "; required range is " +
                        minimumGroundCrest.ToString("F2") +
                        " through " +
                        maximumGroundCrest.ToString("F2") + ".");
                }

                int resolvedButtressCount = representative.GeneratedDefinition
                    .ResolvedParameters.RootButtressCount;
                if (resolvedButtressCount < 3 || resolvedButtressCount > 8)
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 resolved invalid root buttress count " +
                        resolvedButtressCount + "; required 3 through 8.");
                }

                int expectedButtressCount = representative.Family == TreeFamily.Common
                    ? 5
                    : representative.Family == TreeFamily.Pine
                        ? 5
                        : representative.Family == TreeFamily.Twisted
                            ? 5
                            : representative.Family == TreeFamily.Dead
                                ? 6
                                : resolvedButtressCount;
                if (resolvedButtressCount != expectedButtressCount)
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 resolved " + resolvedButtressCount +
                        " root buttresses; expected managed comparison value " +
                        expectedButtressCount + ".");
                }

                TreeResolvedParameters branchParameters = representative
                    .GeneratedDefinition.ResolvedParameters;
                if (representative.Family == TreeFamily.Twisted &&
                    (branchParameters.PrimaryBranchCount < 10 ||
                     branchParameters.SecondaryBranchesPerPrimary < 3 ||
                     branchParameters.InitialBranchElevationDegrees < 14f ||
                     branchParameters.AzimuthSymmetry < 0.80f ||
                     branchParameters.DirectionalBiasStrength > 0.1801f ||
                     branchParameters.PrimaryBranchLengthRatio > 0.3801f))
                {
                    pendingBranchDiagnostics.Add(
                        "Twisted 1 branch distribution remains pending: primary=" +
                        branchParameters.PrimaryBranchCount +
                        ", secondaryPerPrimary=" +
                        branchParameters.SecondaryBranchesPerPrimary +
                        ", elevation=" +
                        branchParameters.InitialBranchElevationDegrees.ToString("F1") +
                        ", symmetry=" +
                        branchParameters.AzimuthSymmetry.ToString("F3") +
                        ", bias=" +
                        branchParameters.DirectionalBiasStrength.ToString("F3") +
                        ", length=" +
                        branchParameters.PrimaryBranchLengthRatio.ToString("F3") + ".");
                }
                else if (representative.Family == TreeFamily.Dead &&
                    (branchParameters.PrimaryBranchCount < 14 ||
                     branchParameters.SecondaryBranchesPerPrimary < 3 ||
                     branchParameters.PrimaryBranchStartHeight < 0.30f ||
                     branchParameters.InitialBranchElevationDegrees < 14f ||
                     branchParameters.AzimuthSymmetry < 0.70f ||
                     branchParameters.DirectionalBiasStrength > 0.2001f ||
                     branchParameters.PrimaryBranchLengthRatio > 0.2601f))
                {
                    pendingBranchDiagnostics.Add(
                        "Dead 1 branch placement/density remains pending: primary=" +
                        branchParameters.PrimaryBranchCount +
                        ", secondaryPerPrimary=" +
                        branchParameters.SecondaryBranchesPerPrimary +
                        ", start=" +
                        branchParameters.PrimaryBranchStartHeight.ToString("F3") +
                        ", elevation=" +
                        branchParameters.InitialBranchElevationDegrees.ToString("F1") +
                        ", symmetry=" +
                        branchParameters.AzimuthSymmetry.ToString("F3") +
                        ", bias=" +
                        branchParameters.DirectionalBiasStrength.ToString("F3") +
                        ", length=" +
                        branchParameters.PrimaryBranchLengthRatio.ToString("F3") + ".");
                }

                if ((representative.Family == TreeFamily.Twisted ||
                     representative.Family == TreeFamily.Dead) &&
                    representative.Recipe != null &&
                    representative.Recipe.FamilyProfile != null)
                {
                    float sampledTurnLimit = Mathf.Max(
                        4f,
                        representative.Recipe.FamilyProfile
                            .StructuralConstraints
                            .MaximumBranchSegmentTurnDegrees * 0.45f);
                    float measuredBranchTurn =
                        CalculateMaximumNonTrunkSegmentTurn(
                            representative.GeneratedDefinition);
                    if (measuredBranchTurn > sampledTurnLimit + 0.01f)
                    {
                        pendingBranchDiagnostics.Add(
                            representative.Family +
                            " 1 sampled branch turn remains pending at " +
                            measuredBranchTurn.ToString("F2") +
                            " degrees versus " +
                            sampledTurnLimit.ToString("F2") + ".");
                    }
                }

                float maximumAverageTwistStep =
                    barkResult.EffectiveTrunkRingCount > 1
                        ? Mathf.Abs(barkResult.RequestedAxialTwistDegrees) /
                          (barkResult.EffectiveTrunkRingCount - 1)
                        : float.PositiveInfinity;
                if (maximumAverageTwistStep > 12.001f)
                {
                    aggregateFailures.Add(
                        representative.Family +
                        " 1 averages " +
                        maximumAverageTwistStep.ToString("F2") +
                        " degrees of twist per trunk ring; maximum is 12.00.");
                }

                if (representative.Family == TreeFamily.Twisted &&
                    (barkResult.PathSpiralStrength < 0.1799f ||
                     barkResult.PathSpiralTurns < 0.999f ||
                     barkResult.PathSpiralDirection >= 0f))
                {
                    aggregateFailures.Add(
                        "Twisted 1 did not retain the managed 0.18 strength, 1.00 turn, clockwise path spiral.");
                }

                if (representative.Family == TreeFamily.Dead &&
                    (barkResult.PathSpiralStrength < 0.0999f ||
                     barkResult.PathSpiralTurns < 0.749f ||
                     barkResult.PathSpiralDirection >= 0f))
                {
                    aggregateFailures.Add(
                        "Dead 1 did not retain the managed 0.10 strength, 0.75 turn, clockwise path spiral.");
                }
            }

            if (barkMeshCount != 4)
            {
                aggregateFailures.Add(
                    "Four-family rounded-root-profile vertical slice expected four meshes but built " +
                    barkMeshCount + ".");
            }

            report.AppendLine();
            report.AppendLine("[Family Dependency Validation]");
            for (int familyIndex = 0; familyIndex < 4; familyIndex++)
            {
                TreeFamily family = (TreeFamily)familyIndex;
                TreeGenerationLibraryVariant variant =
                    library.FindVariant(family, 1);
                if (variant == null || variant.Recipe == null)
                {
                    report.Append("FAIL | ")
                        .Append(family)
                        .AppendLine(" | missing family validation recipe");
                    aggregateFailures.Add(
                        "Missing family validation recipe for " + family + ".");
                    continue;
                }

                string validation =
                    TreeGenerator.RunDeterminismAndDependencyValidation(
                        variant.Recipe,
                        new TreeGenerationOverrides(),
                        variant.Recipe.MasterSeed);
                bool passed = ReportPassed(validation);
                report.Append(passed ? "PASS" : "FAIL")
                    .Append(" | ")
                    .Append(family)
                    .AppendLine(" | deterministic dependency suite");
                if (!passed)
                {
                    report.AppendLine(validation);
                    aggregateFailures.Add(
                        "Determinism/dependency validation failed for " + family + ".");
                }
            }

            MarkSceneDirty(gallery);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = gallery.gameObject;
            EditorGUIUtility.PingObject(gallery.gameObject);
            SceneView.RepaintAll();
            bool passedBuild = aggregateFailures.Count == 0;
            report.AppendLine();
            report.AppendLine("[Summary]");
            report.Append("Status: ")
                .AppendLine(passedBuild ? "PASS" : "FAIL");
            report.Append("Imported references: 20\nProcedural slots: 20\nGenerated structures: ")
                .Append(generatedCount)
                .Append("\nGenerated bark meshes: ")
                .Append(barkMeshCount)
                .Append(" / 4\nDeterministic repeat checks: ")
                .Append(deterministicCount)
                .AppendLine(" / 20");
            report.Append("Managed library: ")
                .AppendLine(TreeGenerationLibraryBuilder.LibraryAssetPath);
            report.AppendLine(
                "Normal workflow: select Tree Reference Gallery and use Rebuild Complete Tree Comparison Gallery. Common 1, Pine 1, Twisted 1, and Dead 1 carry the TREE-GEN.2C.3H5R1 exact regression revert, restoring H4 buttress geometry and values. Branch visual calibration remains pending and non-blocking.");
            if (!passedBuild)
            {
                report.AppendLine("Failures:");
                for (int failureIndex = 0;
                     failureIndex < aggregateFailures.Count;
                     failureIndex++)
                {
                    report.Append("- ")
                        .AppendLine(aggregateFailures[failureIndex]);
                }
            }
            if (pendingBranchDiagnostics.Count > 0)
            {
                report.AppendLine("Pending branch diagnostics (non-blocking):");
                for (int diagnosticIndex = 0;
                     diagnosticIndex < pendingBranchDiagnostics.Count;
                     diagnosticIndex++)
                {
                    report.Append("- ")
                        .AppendLine(pendingBranchDiagnostics[diagnosticIndex]);
                }
            }

            return new TreeUnifiedGalleryBuildResult
            {
                Passed = passedBuild,
                GeneratedTreeCount = generatedCount,
                Timestamp = timestamp,
                Report = report.ToString()
            };
        }

        private static float CalculateMaximumNonTrunkSegmentTurn(
            TreeDefinition definition)
        {
            float maximumTurn = 0f;
            if (definition == null || definition.Branches == null)
            {
                return maximumTurn;
            }

            IReadOnlyList<TreeBranchDefinition> branches = definition.Branches;
            for (int branchIndex = 0;
                 branchIndex < branches.Count;
                 branchIndex++)
            {
                TreeBranchDefinition branch = branches[branchIndex];
                if (branch == null || branch.BranchOrder == 0 ||
                    branch.Samples == null || branch.Samples.Count < 3)
                {
                    continue;
                }

                Vector3 previousDirection =
                    branch.Samples[1].Position -
                    branch.Samples[0].Position;
                if (previousDirection.sqrMagnitude <= 0.000001f)
                {
                    continue;
                }
                previousDirection.Normalize();

                for (int sampleIndex = 2;
                     sampleIndex < branch.Samples.Count;
                     sampleIndex++)
                {
                    Vector3 segment =
                        branch.Samples[sampleIndex].Position -
                        branch.Samples[sampleIndex - 1].Position;
                    if (segment.sqrMagnitude <= 0.000001f)
                    {
                        continue;
                    }

                    Vector3 direction = segment.normalized;
                    maximumTurn = Mathf.Max(
                        maximumTurn,
                        Vector3.Angle(previousDirection, direction));
                    previousDirection = direction;
                }
            }

            return maximumTurn;
        }

        internal static TreeUnifiedGalleryBuildResult RemoveGeneratedOutputs(
            TreeReferenceGallery gallery)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var report = new StringBuilder(2048);
            report.AppendLine("[TREE-GEN.2C.3H5R1 Remove Generated Tree Outputs]");
            report.Append("Generated: ").AppendLine(timestamp);
            if (gallery == null)
            {
                return Failure(report, timestamp, "Tree reference gallery is null.");
            }

            ProceduralTreeInstance[] instances =
                gallery.GetComponentsInChildren<ProceduralTreeInstance>(true);
            int removed = 0;
            for (int index = 0; index < instances.Length; index++)
            {
                if (instances[index] == null)
                {
                    continue;
                }

                TreeBarkMeshAssetBuilder.RemoveSceneOutput(instances[index]);
                Undo.DestroyObjectImmediate(instances[index]);
                removed++;
            }

            MarkSceneDirty(gallery);
            Selection.activeGameObject = gallery.gameObject;
            EditorGUIUtility.PingObject(gallery.gameObject);
            SceneView.RepaintAll();
            report.Append("PASS | Removed ProceduralTreeInstance outputs: ")
                .AppendLine(removed.ToString());
            report.AppendLine(
                "The imported references, procedural slot GameObjects, managed generation library, authored recipes, and reusable managed mesh sub-assets were retained; scene bark renderers were removed.");
            report.AppendLine();
            report.AppendLine("[Summary]");
            report.AppendLine("Status: PASS");
            return new TreeUnifiedGalleryBuildResult
            {
                Passed = true,
                GeneratedTreeCount = 0,
                Timestamp = timestamp,
                Report = report.ToString()
            };
        }

        internal static int CountGeneratedInstances(TreeReferenceGallery gallery)
        {
            return gallery != null
                ? gallery.GetComponentsInChildren<ProceduralTreeInstance>(true).Length
                : 0;
        }

        private static List<TreeReferenceSpecimen> CollectProceduralSlots(
            Transform completeRoot)
        {
            TreeReferenceSpecimen[] specimens =
                completeRoot.GetComponentsInChildren<TreeReferenceSpecimen>(true);
            var slots = new List<TreeReferenceSpecimen>(20);
            for (int index = 0; index < specimens.Length; index++)
            {
                TreeReferenceSpecimen specimen = specimens[index];
                if (specimen != null &&
                    specimen.Role == TreeReferenceRole.ProceduralComparison)
                {
                    slots.Add(specimen);
                }
            }

            slots.Sort(CompareSlots);
            return slots;
        }

        private static int CompareSlots(
            TreeReferenceSpecimen left,
            TreeReferenceSpecimen right)
        {
            int familyComparison = left.Family.CompareTo(right.Family);
            return familyComparison != 0
                ? familyComparison
                : left.SourceVariantIndex.CompareTo(right.SourceVariantIndex);
        }

        private static bool ReportPassed(string report)
        {
            return !string.IsNullOrWhiteSpace(report) &&
                report.TrimEnd().EndsWith(
                    "Status: PASS",
                    StringComparison.Ordinal);
        }

        private static TreeUnifiedGalleryBuildResult Failure(
            StringBuilder report,
            string timestamp,
            string message)
        {
            report.AppendLine();
            report.Append("FAIL | ").AppendLine(message);
            report.AppendLine();
            report.AppendLine("[Summary]");
            report.AppendLine("Status: FAIL");
            return new TreeUnifiedGalleryBuildResult
            {
                Passed = false,
                GeneratedTreeCount = 0,
                Timestamp = timestamp,
                Report = report.ToString()
            };
        }

        private static void MarkSceneDirty(TreeReferenceGallery gallery)
        {
            if (gallery != null && gallery.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gallery.gameObject.scene);
            }
        }
    }
}
