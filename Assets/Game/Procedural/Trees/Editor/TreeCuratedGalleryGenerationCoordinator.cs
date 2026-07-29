using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeCuratedGalleryGenerationCoordinator
    {
        internal const string ReportDirectory =
            "Library/PS3D/Trees/CuratedGallery";
        internal const string ReportPath =
            ReportDirectory + "/TreeCuratedGalleryGenerationReport.txt";

        private sealed class Job
        {
            internal TreeReferenceGallery Gallery;
            internal readonly List<TreeReferenceSpecimen> Slots =
                new List<TreeReferenceSpecimen>();
            internal readonly StringBuilder Report = new StringBuilder(32768);
            internal readonly Stopwatch Stopwatch = Stopwatch.StartNew();
            internal int Index;
            internal int ConfiguredSpawners;
            internal int RecipeAssignments;
            internal int SpawnedInstances;
            internal int InitializedControls;
            internal int ExactRepeatChecks;
            internal int GeneratedStructures;
            internal int DeterministicChecks;
            internal int BarkChecks;
            internal int LegacyReadChecks;
            internal int CompletePasses;
            internal int ReusedCheckpoints;
            internal int Failures;
            internal bool CancelRequested;
            internal string Current = "Preparing";
        }

        private static Job activeJob;

        internal static bool IsRunning => activeJob != null;
        internal static float Progress => activeJob == null || activeJob.Slots.Count == 0
            ? 0f
            : Mathf.Clamp01(activeJob.Index / (float)activeJob.Slots.Count);
        internal static string CurrentOperation => activeJob != null
            ? activeJob.Current
            : "Idle";
        internal static string Eta
        {
            get
            {
                if (activeJob == null || activeJob.Index <= 0)
                {
                    return "—";
                }

                double secondsPerSlot =
                    activeJob.Stopwatch.Elapsed.TotalSeconds / activeJob.Index;
                double remaining = secondsPerSlot *
                    Math.Max(0, activeJob.Slots.Count - activeJob.Index);
                return TimeSpan.FromSeconds(remaining).ToString(@"mm\:ss");
            }
        }

        internal static bool Start(
            TreeReferenceGallery gallery,
            out string message)
        {
            if (activeJob != null)
            {
                message = "A curated gallery generation job is already running.";
                return false;
            }

            if (gallery == null)
            {
                message = "Tree Reference Gallery is null.";
                return false;
            }

            Transform completeRoot = gallery.transform.Find(
                TreeReferenceGalleryBuilder.CompleteGalleryRootName);
            if (completeRoot == null)
            {
                message =
                    "The complete imported gallery does not exist. Build the complete reference gallery first.";
                return false;
            }

            if (!TreeCuratedGalleryUtility.TryResolveCatalog(
                    gallery,
                    out _,
                    out message))
            {
                return false;
            }

            if (gallery.GenerationLibrary == null)
            {
                message =
                    "The gallery has no bark-mesh storage library assigned. " +
                    "This asset is used only for generated mesh persistence; " +
                    "it contributes no recipe behavior.";
                return false;
            }

            var job = new Job { Gallery = gallery };
            TreeReferenceSpecimen[] specimens =
                completeRoot.GetComponentsInChildren<TreeReferenceSpecimen>(true);
            for (int index = 0; index < specimens.Length; index++)
            {
                TreeReferenceSpecimen specimen = specimens[index];
                if (specimen != null &&
                    specimen.Role == TreeReferenceRole.ProceduralComparison)
                {
                    job.Slots.Add(specimen);
                }
            }

            job.Slots.Sort(CompareSlots);
            if (job.Slots.Count != 20)
            {
                message =
                    "Expected 20 procedural comparison slots, found " +
                    job.Slots.Count + ".";
                return false;
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            job.Report.AppendLine(
                "[TREE-CONTROLS.4 Live Recipe Gallery Generation]");
            job.Report.Append("Generated: ").AppendLine(timestamp);
            job.Report.AppendLine(
                "Path: curated recipe intervals -> stable slot seed -> exact instance controls -> recipe-only generator -> bark mesh");
            job.Report.AppendLine(
                "Foliage: deferred; structural and bark validation only");
            job.Report.AppendLine(
                "Legacy behavioral family reads required: 0");
            job.Report.AppendLine(
                "Legacy behavioral calibration reads required: 0");
            job.Report.AppendLine();
            job.Report.AppendLine("[Curated Gallery Assignment]");

            activeJob = job;
            EditorApplication.update += Tick;
            message = "Curated recipe gallery generation started.";
            return true;
        }

        internal static int CountSpawnedInstances(
            TreeReferenceGallery gallery)
        {
            if (gallery == null)
            {
                return 0;
            }

            Transform completeRoot = gallery.transform.Find(
                TreeReferenceGalleryBuilder.CompleteGalleryRootName);
            if (completeRoot == null)
            {
                return 0;
            }

            TreeRecipeSpawner[] spawners =
                completeRoot.GetComponentsInChildren<TreeRecipeSpawner>(true);
            int count = 0;
            for (int index = 0; index < spawners.Length; index++)
            {
                if (spawners[index] != null &&
                    spawners[index].GeneratedInstance != null &&
                    spawners[index].GeneratedInstance.HasGeneratedDefinition)
                {
                    count++;
                }
            }

            return count;
        }

        internal static void Cancel()
        {
            if (activeJob != null)
            {
                activeJob.CancelRequested = true;
            }
        }

        internal static void OpenReportFolder()
        {
            Directory.CreateDirectory(ReportDirectory);
            EditorUtility.RevealInFinder(Path.GetFullPath(ReportDirectory));
        }

        private static void Tick()
        {
            Job job = activeJob;
            if (job == null)
            {
                EditorApplication.update -= Tick;
                return;
            }

            if (job.CancelRequested)
            {
                Finish(job, false, "CANCELLED");
                return;
            }

            if (job.Index >= job.Slots.Count)
            {
                Finish(job, job.Failures == 0, null);
                return;
            }

            TreeReferenceSpecimen slot = job.Slots[job.Index];
            job.Current =
                slot.Family + " " + slot.SourceVariantIndex +
                " (" + (job.Index + 1) + "/" + job.Slots.Count + ")";
            try
            {
                ProcessSlot(job, slot);
            }
            catch (Exception exception)
            {
                job.Failures++;
                job.Report.Append("FAIL | ")
                    .Append(slot.Family).Append(" ")
                    .Append(slot.SourceVariantIndex)
                    .Append(" | exception=")
                    .AppendLine(exception.ToString());
                UnityEngine.Debug.LogException(exception, slot);
            }

            job.Index++;
            WritePartialReport(job);
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        private static void ProcessSlot(
            Job job,
            TreeReferenceSpecimen slot)
        {
            if (!TreeCuratedGalleryUtility.TryConfigureSpawner(
                    job.Gallery,
                    slot,
                    out TreeRecipeSpawner spawner,
                    out string failure))
            {
                job.Failures++;
                job.Report.Append("FAIL | ")
                    .Append(slot.Family).Append(" ")
                    .Append(slot.SourceVariantIndex)
                    .Append(" | ").AppendLine(failure);
                return;
            }

            job.ConfiguredSpawners++;
            if (spawner.Recipe != null)
            {
                job.RecipeAssignments++;
            }

            ProceduralTreeInstance legacyDirectInstance =
                slot.GetComponent<ProceduralTreeInstance>();
            if (legacyDirectInstance != null)
            {
                GameObject legacyBark =
                    legacyDirectInstance.GeneratedBarkObject;
                if (legacyBark == null)
                {
                    Transform legacyBarkTransform = slot.transform.Find(
                        TreeBarkMeshAssetBuilder.GeneratedBarkChildName);
                    legacyBark = legacyBarkTransform != null
                        ? legacyBarkTransform.gameObject
                        : null;
                }

                if (legacyBark != null)
                {
                    Undo.DestroyObjectImmediate(legacyBark);
                }

                Undo.DestroyObjectImmediate(legacyDirectInstance);
            }

            ProceduralTreeInstance checkpointInstance =
                spawner.GeneratedInstance;
            if (CanReuseCheckpoint(spawner, checkpointInstance))
            {
                CountReusableCheckpoint(job);
                job.ReusedCheckpoints++;
                job.Report.Append("PASS | ")
                    .Append(slot.Family).Append(" ")
                    .Append(slot.SourceVariantIndex)
                    .Append(" | recipe=")
                    .Append(spawner.Recipe.RecipeDisplayName)
                    .Append(" | seed=")
                    .Append(spawner.SpawnSeed)
                    .AppendLine(" | checkpoint=REUSED | controls=42/42 | structureRepeat=PASS | bark=PASS | legacyReads=0");
                return;
            }

            ProceduralTreeInstance instance =
                TreeRecipeSpawnerEditor.EnsureGeneratedChild(spawner);
            if (instance != null)
            {
                job.SpawnedInstances++;
            }
            Undo.RecordObject(spawner, "Rebuild Curated Gallery Slot");
            Undo.RecordObject(instance, "Rebuild Curated Gallery Tree");
            spawner.AttachGeneratedInstance(instance);
            spawner.PrepareGeneratedInstance(job.Gallery.GenerationLibrary);

            bool exactReady = instance.HasExactControls &&
                instance.ExactControls != null;
            string exactFingerprint = exactReady
                ? instance.ExactControls.CalculateFingerprint()
                : string.Empty;
            var repeatedControls = new TreeResolvedControls();
            if (spawner.Recipe != null)
            {
                repeatedControls.ResolveFrom(
                    spawner.Recipe.ControlRanges,
                    spawner.SpawnSeed);
            }
            bool exactRepeat = exactReady &&
                exactFingerprint == repeatedControls.CalculateFingerprint();

            TreeGenerationResult generated = instance.GenerateStructure();
            TreeGenerationResult repeated = generated.Passed
                ? TreeGenerator.GenerateExactForValidation(
                    instance.ExactControls,
                    instance.MasterSeed,
                    instance.ExactControlsSourceRecipeIdentity,
                    instance.Family)
                : null;
            bool structureRepeat = generated.Passed &&
                generated.Definition != null &&
                repeated != null && repeated.Passed &&
                repeated.Definition != null &&
                generated.Definition.StructuralFingerprint ==
                    repeated.Definition.StructuralFingerprint;
            bool zeroLegacyReads = generated.Report != null &&
                generated.Report.IndexOf(
                    "Behavioral family reads: 0 | Behavioral calibration reads: 0",
                    StringComparison.Ordinal) >= 0;

            bool barkPassed = false;
            string barkFailure = string.Empty;
            if (generated.Passed && structureRepeat && exactRepeat)
            {
                barkPassed = TreeBarkMeshAssetBuilder.BuildOrUpdate(
                    job.Gallery,
                    job.Gallery.GenerationLibrary,
                    instance,
                    out _,
                    out _,
                    out barkFailure);
            }

            if (exactReady)
            {
                job.InitializedControls +=
                    TreeControlDescriptorRegistry.Controls.Count;
            }
            if (exactRepeat)
            {
                job.ExactRepeatChecks++;
            }
            if (generated != null && generated.Passed)
            {
                job.GeneratedStructures++;
            }
            if (structureRepeat)
            {
                job.DeterministicChecks++;
            }
            if (zeroLegacyReads)
            {
                job.LegacyReadChecks++;
            }
            if (barkPassed)
            {
                job.BarkChecks++;
            }

            bool passed = exactReady && exactRepeat &&
                generated.Passed && structureRepeat &&
                zeroLegacyReads && barkPassed;
            if (passed)
            {
                job.CompletePasses++;
            }
            else
            {
                job.Failures++;
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string slotReport = BuildSlotReport(
                slot,
                spawner,
                instance,
                exactReady,
                exactRepeat,
                generated,
                structureRepeat,
                zeroLegacyReads,
                barkPassed,
                barkFailure);
            spawner.RecordSpawn(passed, timestamp, slotReport);
            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(instance);
            if (slot.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(slot.gameObject.scene);
            }

            // Each completed slot is a real checkpoint: persistent generated
            // mesh subassets are saved before the job advances. Scene objects
            // remain marked dirty so cancellation never discards completed work.
            AssetDatabase.SaveAssets();

            job.Report.Append(passed ? "PASS | " : "FAIL | ")
                .Append(slot.Family).Append(" ")
                .Append(slot.SourceVariantIndex)
                .Append(" | recipe=")
                .Append(spawner.Recipe != null
                    ? spawner.Recipe.RecipeDisplayName
                    : "MISSING")
                .Append(" | seed=")
                .Append(spawner.SpawnSeed)
                .Append(" | controls=")
                .Append(exactReady && exactRepeat ? "42/42" : "FAIL")
                .Append(" | structureRepeat=")
                .Append(structureRepeat ? "PASS" : "FAIL")
                .Append(" | bark=")
                .Append(barkPassed
                    ? "PASS"
                    : "FAIL (" + FormatInlineFailure(barkFailure) + ")")
                .Append(" | legacyReads=")
                .AppendLine(zeroLegacyReads ? "0" : "FAIL");
        }

        private static void CountReusableCheckpoint(Job job)
        {
            job.SpawnedInstances++;
            job.InitializedControls +=
                TreeControlDescriptorRegistry.Controls.Count;
            job.ExactRepeatChecks++;
            job.GeneratedStructures++;
            job.DeterministicChecks++;
            job.BarkChecks++;
            job.LegacyReadChecks++;
            job.CompletePasses++;
        }

        private static bool CanReuseCheckpoint(
            TreeRecipeSpawner spawner,
            ProceduralTreeInstance instance)
        {
            if (spawner == null ||
                spawner.Recipe == null ||
                !spawner.LastSpawnPassed ||
                instance == null ||
                instance.Recipe != spawner.Recipe ||
                instance.MasterSeed != spawner.SpawnSeed ||
                !instance.UsesRecipeOnlyGeneration ||
                !instance.HasExactControls ||
                !instance.HasGeneratedDefinition ||
                !instance.HasGeneratedBarkMesh ||
                instance.GeneratedDefinition.RecipeIdentity !=
                    spawner.Recipe.StableIdentity ||
                instance.GeneratedDefinition.MasterSeed != spawner.SpawnSeed ||
                instance.GeneratedDefinition.GeneratorVersion !=
                    TreeGenerator.CurrentGeneratorVersion ||
                instance.GeneratedDefinition.ResolvedParameters == null ||
                !instance.GeneratedDefinition.ResolvedParameters
                    .RecipeOnlyControlSource ||
                string.IsNullOrEmpty(instance.LastGenerationReport) ||
                instance.LastGenerationReport.IndexOf(
                    "Behavioral family reads: 0 | Behavioral calibration reads: 0",
                    StringComparison.Ordinal) < 0 ||
                string.IsNullOrEmpty(instance.LastBarkMeshReport) ||
                !HasCompatibleBarkAlgorithmReport(
                    instance.LastBarkMeshReport))
            {
                return false;
            }

            var expected = new TreeResolvedControls();
            expected.ResolveFrom(
                spawner.Recipe.ControlRanges,
                spawner.SpawnSeed);
            return instance.ExactControls.CalculateFingerprint() ==
                expected.CalculateFingerprint();
        }

        private static string FormatInlineFailure(string failure)
        {
            return string.IsNullOrEmpty(failure)
                ? "unspecified bark failure"
                : failure.Replace("\r", string.Empty)
                    .Replace("\n", " | ");
        }

        private static bool HasCompatibleBarkAlgorithmReport(
            string report)
        {
            if (string.IsNullOrEmpty(report))
            {
                return false;
            }

            string current = "Bark algorithm version: " +
                TreeBarkMeshGenerator.BarkAlgorithmVersion;
            if (report.IndexOf(current, StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            // Generator 6 and bark algorithm 18 repair control contracts and
            // recipe-root geometry. Earlier checkpoints are stale and must be
            // rebuilt even when their previous topology audit passed.
            return false;
        }

        private static string BuildSlotReport(
            TreeReferenceSpecimen slot,
            TreeRecipeSpawner spawner,
            ProceduralTreeInstance instance,
            bool exactReady,
            bool exactRepeat,
            TreeGenerationResult generated,
            bool structureRepeat,
            bool zeroLegacyReads,
            bool barkPassed,
            string barkFailure)
        {
            var report = new StringBuilder(4096);
            report.AppendLine("[TREE-CONTROLS.4 Gallery Slot]");
            report.Append("Slot: ")
                .Append(slot.Family).Append(" ")
                .AppendLine(slot.SourceVariantIndex.ToString());
            report.Append("Recipe: ")
                .AppendLine(spawner.Recipe != null
                    ? spawner.Recipe.RecipeDisplayName
                    : "MISSING");
            report.Append("Stable slot identity: ")
                .AppendLine(spawner.StableSlotIdentity);
            report.Append("Seed: ").AppendLine(spawner.SpawnSeed.ToString());
            report.Append("Exact controls initialized: ")
                .AppendLine(exactReady ? "42/42" : "FAIL");
            report.Append("Same-seed exact resample: ")
                .AppendLine(exactRepeat ? "PASS" : "FAIL");
            report.Append("Structural generation: ")
                .AppendLine(generated != null && generated.Passed
                    ? "PASS" : "FAIL");
            report.Append("Structural repeatability: ")
                .AppendLine(structureRepeat ? "PASS" : "FAIL");
            report.Append("Behavioral family/calibration reads: ")
                .AppendLine(zeroLegacyReads ? "0 / 0" : "FAIL");
            report.Append("Bark generation/repeatability/topology: ")
                .AppendLine(barkPassed ? "PASS" : "FAIL | " + barkFailure);
            if (generated != null && generated.Definition != null)
            {
                report.Append("Structural fingerprint: ")
                    .AppendLine(generated.Definition.StructuralFingerprint);
            }
            report.Append("Status: ").AppendLine(
                exactReady && exactRepeat && generated != null &&
                generated.Passed && structureRepeat && zeroLegacyReads &&
                barkPassed ? "PASS" : "FAIL");
            return report.ToString();
        }

        private static void Finish(
            Job job,
            bool passed,
            string terminalStatus)
        {
            EditorApplication.update -= Tick;
            job.Stopwatch.Stop();
            job.Report.AppendLine();
            job.Report.AppendLine("[Summary]");
            job.Report.Append("Spawner slots: ")
                .Append(job.ConfiguredSpawners).AppendLine("/20");
            job.Report.Append("Recipe assignments: ")
                .Append(job.RecipeAssignments).AppendLine("/20");
            job.Report.Append("Spawned exact instances: ")
                .Append(job.SpawnedInstances).AppendLine("/20");
            job.Report.Append("Initialized exact controls: ")
                .Append(job.InitializedControls).AppendLine("/840");
            job.Report.Append("Same-seed exact resample checks: ")
                .Append(job.ExactRepeatChecks).AppendLine("/20");
            job.Report.Append("Generated structures: ")
                .Append(job.GeneratedStructures).AppendLine("/20");
            job.Report.Append("Deterministic repeat checks: ")
                .Append(job.DeterministicChecks).AppendLine("/20");
            job.Report.Append("Bark checks: ")
                .Append(job.BarkChecks).AppendLine("/20");
            job.Report.Append("Legacy-read zero checks: ")
                .Append(job.LegacyReadChecks).AppendLine("/20");
            job.Report.Append("Complete slot passes: ")
                .Append(job.CompletePasses).AppendLine("/20");
            job.Report.Append("Reused completed checkpoints: ")
                .AppendLine(job.ReusedCheckpoints.ToString());
            job.Report.Append("Legacy behavioral family reads: ")
                .AppendLine(job.LegacyReadChecks == 20 ? "0" : "UNVERIFIED");
            job.Report.Append("Legacy behavioral calibration reads: ")
                .AppendLine(job.LegacyReadChecks == 20 ? "0" : "UNVERIFIED");
            job.Report.Append("Operation failures: ")
                .AppendLine(job.Failures.ToString());
            job.Report.Append("Elapsed: ")
                .Append(job.Stopwatch.Elapsed.TotalSeconds.ToString("F2"))
                .AppendLine(" s");
            string status = terminalStatus ?? (passed ? "PASS" : "FAIL");
            job.Report.Append("Status: ").AppendLine(status);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            job.Gallery.RecordUnifiedGenerationBuild(
                passed,
                job.CompletePasses,
                timestamp,
                job.Report.ToString());
            EditorUtility.SetDirty(job.Gallery);
            if (job.Gallery.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(job.Gallery.gameObject.scene);
            }
            WritePartialReport(job);
            activeJob = null;
            SceneView.RepaintAll();
            UnityEngine.Debug.Log(
                "[TREE-CONTROLS.4] Curated gallery generation " + status +
                ". Report: " + ReportPath,
                job.Gallery);
        }

        private static void WritePartialReport(Job job)
        {
            Directory.CreateDirectory(ReportDirectory);
            File.WriteAllText(ReportPath, job.Report.ToString());
        }

        private static int CompareSlots(
            TreeReferenceSpecimen first,
            TreeReferenceSpecimen second)
        {
            int family = first.Family.CompareTo(second.Family);
            return family != 0
                ? family
                : first.SourceVariantIndex.CompareTo(
                    second.SourceVariantIndex);
        }
    }
}
