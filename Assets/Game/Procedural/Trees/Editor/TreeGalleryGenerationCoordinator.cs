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
                "[TREE-GEN.2C Four-Family Trunk Grammar Build]");
            report.Append("Generated: ").AppendLine(timestamp);
            report.AppendLine(
                "Workflow: one action audits and repairs source imports, rebuilds all twenty imported references and procedural slots, upgrades the managed generation library, generates calibrated constrained structures, builds the four bark representatives with compact buttress/ridge/twist grammar, and validates deterministic repeatability and selective regeneration.");
            report.AppendLine(
                "Output: all twenty procedural slots receive ProceduralTreeInstance structural previews; Common 1, Pine 1, Twisted 1, and Dead 1 receive persistent bark meshes with non-circular trunk cross-sections and root buttresses. Foliage meshes remain TREE-GEN.3 work.");
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
            report.AppendLine("[Trunk Grammar Bark Vertical Slice]");
            int barkMeshCount = 0;
            var aggregateFailures = new List<string>();
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
                    .Append(" | trunkSegments=")
                    .Append(barkResult.EffectiveTrunkRadialSegments)
                    .Append(" | crossSection=")
                    .Append(barkResult.MaximumCrossSectionMultiplier.ToString("F3"))
                    .Append(" | rootWD=")
                    .Append(barkResult.GeneratedRootWidth.ToString("F3"))
                    .Append("/")
                    .Append(barkResult.GeneratedRootDepth.ToString("F3"))
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
            }

            if (barkMeshCount != 4)
            {
                aggregateFailures.Add(
                    "Four-family trunk-grammar vertical slice expected four meshes but built " +
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
                "Normal workflow: select Tree Reference Gallery and use Rebuild Complete Tree Comparison Gallery. Structural previews default to Selected Tree; Common 1, Pine 1, Twisted 1, and Dead 1 carry the TREE-GEN.2C buttress/ridge/twist bark vertical slice.");
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

            return new TreeUnifiedGalleryBuildResult
            {
                Passed = passedBuild,
                GeneratedTreeCount = generatedCount,
                Timestamp = timestamp,
                Report = report.ToString()
            };
        }

        internal static TreeUnifiedGalleryBuildResult RemoveGeneratedOutputs(
            TreeReferenceGallery gallery)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var report = new StringBuilder(2048);
            report.AppendLine("[TREE-GEN.2C Remove Generated Tree Outputs]");
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
