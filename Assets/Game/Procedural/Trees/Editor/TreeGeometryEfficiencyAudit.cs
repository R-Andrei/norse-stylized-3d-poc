using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeGeometryEfficiencyAudit
    {
        private const string OutputDirectory =
            "Library/PS3D/Trees/GeometryEfficiencyAudit";
        private const string CaptureDirectoryName = "Captures";
        private const string ReportFileName =
            "TreeGeometryEfficiencyAudit.md";
        private const string AggregateCsvFileName =
            "TreeGeometryEfficiencyAudit.csv";
        private const string BranchCsvFileName =
            "TreeGeometryEfficiencyBranches.csv";
        private const int ExpectedTreeCount = 20;
        private const int CapturePixelHeight = 288;
        private const int MaximumCaptureWidth = 1024;
        private const int KnownRecentHeadVertices = 18585;
        private const int KnownRecentHeadTriangles = 30734;

        private static readonly TreeBarkMeshEfficiencyPolicy[] Policies =
        {
            TreeBarkMeshEfficiencyPolicy.Current,
            TreeBarkMeshEfficiencyPolicy.LegacyCurrent,
            TreeBarkMeshEfficiencyPolicy.RadialAggressive
        };

        private sealed class Target
        {
            internal ProceduralTreeInstance Instance;
            internal string Name;
            internal TreeFamily Family;
            internal int Variant;
            internal TreeDefinition Definition;
            internal string DefinitionFailure;
            internal double MeasuredGenerationMilliseconds;
            internal bool FreshFingerprintMatched;
            internal int SerializedJsonBytes;
            internal long RawStructuralBytes;
            internal int RendererCount;
            internal int DrawCallEstimate;
            internal int ShadowCasterRendererCount;
            internal int ShadowCasterDrawCallEstimate;
            internal long ShadowCasterTriangleEstimate;
            internal int ExistingMeshVertices;
            internal int ExistingMeshTriangles;
        }

        private sealed class CaseResult
        {
            internal Target Target;
            internal TreeBarkMeshEfficiencyPolicy Policy;
            internal bool Passed;
            internal string Failure;
            internal TreeBarkMeshBuildResult Bark;
            internal string CapturePath;
            internal string CaptureFailure;
            internal float SilhouetteDeviation;
            internal bool SilhouetteMeasured;
            internal bool ExistingBaselineMeshMatched;
        }

        private sealed class PendingCapture
        {
            internal CaseResult Result;
            internal RenderTexture RenderTexture;
            internal AsyncGPUReadbackRequest Request;
            internal AsyncGPUReadbackRequest CompletedRequest;
            internal string CapturePath;
            internal int Width;
            internal int Height;
            internal bool CallbackCompleted;
            internal bool Abandoned;
            internal bool Released;
        }

        private sealed class Job
        {
            internal List<Target> Targets;
            internal readonly List<CaseResult> Results =
                new List<CaseResult>();
            internal readonly Dictionary<string, bool[]> BaselineSilhouettes =
                new Dictionary<string, bool[]>();
            internal int TargetIndex;
            internal int PolicyIndex;
            internal int CompletedCases;
            internal int PassedCases;
            internal int FailedCases;
            internal bool CancelRequested;
            internal string CancelReason;
            internal PendingCapture PendingCapture;
            internal DateTime StartedUtc;
            internal string ReportPath;
            internal string AggregateCsvPath;
            internal string BranchCsvPath;
            internal string CaptureDirectory;
            internal StreamWriter AggregateWriter;
            internal StreamWriter BranchWriter;
            internal Quaternion CaptureRotation;
            internal Matrix4x4 CaptureProjectionMatrix;
            internal bool CaptureOrthographic;
            internal float CaptureNearClipPlane;
            internal float CaptureFarClipPlane;
            internal float CaptureReferenceDistance;
            internal int CaptureWidth;
            internal int CaptureHeight;
            internal string CaptureCameraSource;
            internal bool CaptureAvailable;
        }

        private static Job activeJob;
        private static string lastReportPath = string.Empty;
        private static float currentProgress;
        private static string currentDetail = "Not running";
        private static string currentEta = string.Empty;

        internal static bool IsRunning => activeJob != null;
        internal static string LastReportPath => lastReportPath;
        internal static float CurrentProgress => currentProgress;
        internal static string CurrentDetail => currentDetail;
        internal static string CurrentEta => currentEta;

        internal static string ProgressLabel
        {
            get
            {
                if (activeJob == null)
                {
                    return "Not running";
                }

                return activeJob.CompletedCases + " / " +
                    (activeJob.Targets.Count * Policies.Length);
            }
        }

        internal static bool Start(ProceduralTreeInstance selected)
        {
            if (activeJob != null)
            {
                return false;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError(
                    "[TREE-GEOMETRY.3B] Run the geometry-efficiency audit in Edit Mode, not while entering or running Play Mode.");
                return false;
            }

            if (TreeControlResponseSuite.IsRunning ||
                TreeRootCollapseTournament.IsRunning ||
                TreeCuratedGalleryGenerationCoordinator.IsRunning)
            {
                Debug.LogError(
                    "[TREE-GEOMETRY.3B] Another tree generation or diagnostic job is running. Finish or cancel it before starting the geometry-efficiency audit.");
                return false;
            }

            TreeReferenceGallery gallery = selected != null
                ? selected.GetComponentInParent<TreeReferenceGallery>()
                : null;
            if (gallery == null)
            {
                Debug.LogError(
                    "[TREE-GEOMETRY.3B] Select a procedural tree beneath the Tree Reference Gallery before running the audit.");
                return false;
            }

            List<Target> targets = CollectTargets(gallery);
            if (targets.Count != ExpectedTreeCount)
            {
                Debug.LogError(
                    "[TREE-GEOMETRY.3B] The audit requires exactly twenty initialized procedural comparison-gallery slots beneath " +
                    TreeReferenceGalleryBuilder.CompleteGalleryRootName +
                    ". Found " + targets.Count +
                    ". Rebuild the complete comparison gallery first.",
                    gallery);
                return false;
            }

            Directory.CreateDirectory(OutputDirectory);
            string captureDirectory = Path.Combine(
                OutputDirectory,
                CaptureDirectoryName);
            if (Directory.Exists(captureDirectory))
            {
                Directory.Delete(captureDirectory, true);
            }
            Directory.CreateDirectory(captureDirectory);

            string reportPath = Path.Combine(OutputDirectory, ReportFileName);
            string aggregateCsvPath = Path.Combine(
                OutputDirectory,
                AggregateCsvFileName);
            string branchCsvPath = Path.Combine(
                OutputDirectory,
                BranchCsvFileName);
            var aggregateWriter = new StreamWriter(
                aggregateCsvPath,
                false,
                Encoding.UTF8);
            aggregateWriter.WriteLine(
                "Tree,Family,Variant,Policy,Status,Failure," +
                "GenerationMs,FreshFingerprintMatched,SerializedJsonBytes,RawStructuralBytes," +
                "Branches,PrimaryBranches,SecondaryBranches,TertiaryBranches,RejectedBranches,DeadBranches,BrokenBranches,ControlPoints,CurveSamples," +
                "Vertices,Triangles,EstimatedMeshBytes,GeometryBuildMs,TopologyAuditMs,MeshUploadMs,TotalBuildMs," +
                "TrunkMinRadialSegments,TrunkMaxRadialSegments,TrunkAverageRadialSegments,TrunkRadialTransitions,TrunkMixedResolutionStrips,TrunkStitchTriangles," +
                "TrunkVertices,TrunkTriangles,RootZoneVertices,RootZoneTriangles,RootLobeVertices,RootLobeTriangles,ButtressPersistenceVertices,ButtressPersistenceTriangles,OrdinaryTrunkVertices,OrdinaryTrunkTriangles," +
                "PrimaryVertices,PrimaryTriangles,SecondaryVertices,SecondaryTriangles,TertiaryVertices,TertiaryTriangles," +
                "CapVertices,CapTriangles,SeamDuplicateVertices,SourceSamples,RenderRings,InsertedRings,RootRefinementInsertedRings,TwistRefinementInsertedRings,AdaptiveShapeRefinementInsertedRings,RemovedRings,EfficiencyPolicyRemovedRings,TopologyRepairRemovedRings," +
                "PhaseAlignedRings,CurvatureRadiusClamps,CircularTopologyRemovedRings,TrunkTipRemovedRings,AlternateQuadDiagonals," +
                "AverageSegmentLength,MaximumSegmentLength,AverageTurnDegrees,MaximumTurnDegrees," +
                "RendererCount,DrawCallEstimate,ShadowCasterRendererCount,ShadowCasterDrawCallEstimate,ShadowCasterTriangleEstimate,ExistingMeshVertices,ExistingMeshTriangles,ExistingBaselineMeshMatched," +
                "SilhouetteMeasured,SilhouetteDeviation,CapturePath,CaptureFailure,TopologySummary");
            aggregateWriter.Flush();

            var branchWriter = new StreamWriter(
                branchCsvPath,
                false,
                Encoding.UTF8);
            branchWriter.WriteLine(
                "Tree,Family,Variant,Policy,StableBranchId,BranchOrder," +
                "SourceSamples,RenderRings,RadialSegments,MinimumRadialSegments,MaximumRadialSegments,AverageRadialSegments,RadialTransitions,MixedResolutionStrips,StitchTriangles,RootLobeAverageRadialSegments,ButtressPersistenceAverageRadialSegments,OrdinaryTrunkAverageRadialSegments,SideVertices,SideTriangles,CapVertices,CapTriangles,SeamDuplicateVertices," +
                "RootZoneRings,RootZoneIntervals,RootZoneVertices,RootZoneTriangles,RootLobeRings,RootLobeIntervals,RootLobeVertices,RootLobeTriangles,ButtressPersistenceRings,ButtressPersistenceIntervals,ButtressPersistenceVertices,ButtressPersistenceTriangles,OrdinaryTrunkVertices,OrdinaryTrunkTriangles," +
                "InsertedRings,RootRefinementInsertedRings,TwistRefinementInsertedRings,AdaptiveShapeRefinementInsertedRings,RemovedRings,EfficiencyPolicyRemovedRings,TopologyRepairRemovedRings,AverageSegmentLength,MaximumSegmentLength,AverageTurnDegrees,MaximumTurnDegrees");
            branchWriter.Flush();

            Camera mainCamera = Camera.main;
            float captureAspect = mainCamera != null
                ? Mathf.Max(0.1f, mainCamera.aspect)
                : 1f;
            int captureWidth = Mathf.Clamp(
                Mathf.RoundToInt(CapturePixelHeight * captureAspect),
                1,
                MaximumCaptureWidth);
            float captureReferenceDistance = mainCamera != null &&
                selected != null
                    ? Mathf.Abs(Vector3.Dot(
                        selected.transform.position -
                            mainCamera.transform.position,
                        mainCamera.transform.forward))
                    : 10f;
            captureReferenceDistance = Mathf.Max(
                0.1f,
                captureReferenceDistance);
            activeJob = new Job
            {
                Targets = targets,
                StartedUtc = DateTime.UtcNow,
                ReportPath = reportPath,
                AggregateCsvPath = aggregateCsvPath,
                BranchCsvPath = branchCsvPath,
                CaptureDirectory = captureDirectory,
                AggregateWriter = aggregateWriter,
                BranchWriter = branchWriter,
                CaptureAvailable = mainCamera != null,
                CaptureRotation = mainCamera != null
                    ? mainCamera.transform.rotation
                    : Quaternion.identity,
                CaptureProjectionMatrix = mainCamera != null
                    ? mainCamera.projectionMatrix
                    : Matrix4x4.identity,
                CaptureOrthographic = mainCamera != null &&
                    mainCamera.orthographic,
                CaptureNearClipPlane = mainCamera != null
                    ? mainCamera.nearClipPlane
                    : 0.01f,
                CaptureFarClipPlane = mainCamera != null
                    ? mainCamera.farClipPlane
                    : 1000f,
                CaptureReferenceDistance = captureReferenceDistance,
                CaptureWidth = captureWidth,
                CaptureHeight = CapturePixelHeight,
                CaptureCameraSource = mainCamera != null
                    ? mainCamera.name + " exact projection; isolated candidate recentered from " +
                        (selected != null ? selected.name : "unknown selection") +
                        "; " + captureWidth + "×" + CapturePixelHeight +
                        "; reference distance=" +
                        captureReferenceDistance.ToString(
                            "F3",
                            CultureInfo.InvariantCulture)
                    : "Unavailable: no enabled MainCamera-tagged Camera"
            };
            lastReportPath = reportPath;
            currentProgress = 0f;
            currentDetail = "Preparing first tree";
            currentEta = "ETA calculating";
            WriteCheckpoint(activeJob, "RUNNING");
            TreeGeometryEfficiencyAuditWindow.ShowWindow();
            EditorApplication.update += Tick;
            AssemblyReloadEvents.beforeAssemblyReload += AbortForReload;
            EditorApplication.quitting += AbortForQuit;
            Debug.Log(
                "[TREE-GEOMETRY.3B] Incremental geometry-efficiency audit started. Cases=" +
                (targets.Count * Policies.Length) + ". Output: " + reportPath,
                gallery);
            return true;
        }

        internal static void RequestCancel()
        {
            if (activeJob != null)
            {
                activeJob.CancelRequested = true;
            }
        }

        internal static void CopyLastReport()
        {
            if (!string.IsNullOrEmpty(lastReportPath) &&
                File.Exists(lastReportPath))
            {
                EditorGUIUtility.systemCopyBuffer =
                    File.ReadAllText(lastReportPath);
            }
        }

        internal static void OpenOutputFolder()
        {
            Directory.CreateDirectory(OutputDirectory);
            EditorUtility.RevealInFinder(Path.GetFullPath(OutputDirectory));
        }

        private static void AbortForReload()
        {
            if (activeJob != null)
            {
                Finish(
                    activeJob,
                    "CANCELLED",
                    "Assembly reload interrupted the audit after the partial Markdown and CSV checkpoints were preserved.");
            }
        }

        private static void AbortForQuit()
        {
            if (activeJob != null)
            {
                Finish(
                    activeJob,
                    "CANCELLED",
                    "Editor shutdown interrupted the audit after the partial Markdown and CSV checkpoints were preserved.");
            }
        }

        private static void Tick()
        {
            Job job = activeJob;
            if (job == null)
            {
                return;
            }

            try
            {
                string forcedCancelReason = null;
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    forcedCancelReason =
                        "Play Mode was requested while the geometry-efficiency audit was running. The completed checkpoints were preserved.";
                }
                else if (TreeControlResponseSuite.IsRunning ||
                    TreeRootCollapseTournament.IsRunning ||
                    TreeCuratedGalleryGenerationCoordinator.IsRunning)
                {
                    forcedCancelReason =
                        "Another tree generation or diagnostic job started while the geometry-efficiency audit was running. The completed checkpoints were preserved.";
                }

                if (!string.IsNullOrEmpty(forcedCancelReason))
                {
                    job.CancelRequested = true;
                    job.CancelReason = forcedCancelReason;
                }

                int totalCases = job.Targets.Count * Policies.Length;
                Target target = job.Targets[job.TargetIndex];
                TreeBarkMeshEfficiencyPolicy policy =
                    Policies[job.PolicyIndex];
                TimeSpan elapsed = DateTime.UtcNow - job.StartedUtc;
                double secondsPerCase = job.CompletedCases > 0
                    ? elapsed.TotalSeconds / job.CompletedCases
                    : 0.0;
                double etaSeconds = secondsPerCase *
                    (totalCases - job.CompletedCases);
                currentProgress = job.CompletedCases /
                    (float)Mathf.Max(1, totalCases);
                currentDetail = target.Name + " — " + policy;
                if (job.PendingCapture != null)
                {
                    currentDetail += " — awaiting asynchronous GPU capture";
                }
                currentEta = "Elapsed " +
                    FormatDuration(elapsed.TotalSeconds) +
                    " | ETA " + FormatDuration(etaSeconds);
                TreeGeometryEfficiencyAuditWindow.RepaintOpenWindow();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

                if (job.PendingCapture != null)
                {
                    if (!IsCaptureComplete(job.PendingCapture))
                    {
                        return;
                    }

                    CompletePendingCapture(job);
                    CompleteCaseAndAdvance(job);
                    return;
                }

                if (job.CancelRequested)
                {
                    Finish(job, "CANCELLED", job.CancelReason);
                    return;
                }

                bool completedSynchronously =
                    RunOneCase(job, target, policy);
                if (completedSynchronously)
                {
                    CompleteCaseAndAdvance(job);
                }
            }
            catch (Exception exception)
            {
                Finish(activeJob, "FAILED", exception.ToString());
            }
        }

        private static bool RunOneCase(
            Job job,
            Target target,
            TreeBarkMeshEfficiencyPolicy policy)
        {
            if (target.Definition == null &&
                string.IsNullOrEmpty(target.DefinitionFailure))
            {
                PrepareDefinition(target);
            }

            var result = new CaseResult
            {
                Target = target,
                Policy = policy
            };
            if (target.Definition == null)
            {
                result.Failure = target.DefinitionFailure;
                FinalizeCase(job, result);
                return true;
            }

            var mesh = new Mesh
            {
                name = "TREE-GEOMETRY.3B " + target.Name + " " + policy,
                hideFlags = HideFlags.HideAndDontSave
            };
            try
            {
                TreeBarkMeshSettings settings =
                    TreeBarkMeshSettings.CreateEfficiencyAuditDefaults(
                        target.Family,
                        target.Instance.UsesRecipeOnlyGeneration,
                        policy);
                result.Bark = TreeBarkMeshGenerator.Build(
                    target.Definition,
                    settings,
                    mesh);
                if (result.Bark == null || !result.Bark.Passed)
                {
                    result.Failure = result.Bark != null
                        ? result.Bark.Failure
                        : "Bark generation returned null.";
                }
                else
                {
                    result.Passed = true;
                    if (policy == TreeBarkMeshEfficiencyPolicy.Current &&
                        target.ExistingMeshVertices > 0)
                    {
                        result.ExistingBaselineMeshMatched =
                            target.ExistingMeshVertices ==
                                result.Bark.VertexCount &&
                            target.ExistingMeshTriangles ==
                                result.Bark.TriangleCount;
                    }
                    else
                    {
                        result.ExistingBaselineMeshMatched = false;
                    }

                    if (job.CaptureAvailable &&
                        BeginSilhouetteCapture(
                            job,
                            target,
                            policy,
                            mesh,
                            result))
                    {
                        return false;
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }

            FinalizeCase(job, result);
            return true;
        }

        private static void FinalizeCase(Job job, CaseResult result)
        {
            if (result.Passed)
            {
                job.PassedCases++;
            }
            else
            {
                job.FailedCases++;
            }
            job.Results.Add(result);
            WriteAggregateCase(job.AggregateWriter, result);
            WriteBranchCases(job.BranchWriter, result);
            job.AggregateWriter.Flush();
            job.BranchWriter.Flush();
        }

        private static void CompleteCaseAndAdvance(Job job)
        {
            job.CompletedCases++;
            WriteCheckpoint(job, "RUNNING");
            if (!Advance(job))
            {
                Finish(job, "COMPLETE", null);
                return;
            }

            if (job.CancelRequested)
            {
                Finish(job, "CANCELLED", job.CancelReason);
            }
        }

        private static void PrepareDefinition(Target target)
        {
            TreeDefinition serializedDefinition =
                target.Instance.GeneratedDefinition;
            if (serializedDefinition == null ||
                !serializedDefinition.IsValid)
            {
                target.DefinitionFailure =
                    "The comparison-gallery instance has no valid serialized generated definition.";
                return;
            }

            // Geometry-policy comparisons must hold structure constant. The
            // visible/generated bark asset was built from this serialized
            // definition, so Current parity is meaningful only when the audit
            // uses the same structure snapshot for all three policies.
            target.Definition = serializedDefinition;
            string json = JsonUtility.ToJson(serializedDefinition);
            target.SerializedJsonBytes = Encoding.UTF8.GetByteCount(json);
            target.RawStructuralBytes = EstimateRawStructuralBytes(
                serializedDefinition);

            // Deterministic regeneration remains a separate diagnostic. A
            // fingerprint mismatch is evidence of stale or edited structural
            // state, but it must not silently replace the structure under the
            // geometry tournament.
            var stopwatch = Stopwatch.StartNew();
            TreeGenerationResult generation;
            if (target.Instance.UsesRecipeOnlyGeneration)
            {
                string sourceIdentity = !string.IsNullOrEmpty(
                    target.Instance.ExactControlsSourceRecipeIdentity)
                        ? target.Instance.ExactControlsSourceRecipeIdentity
                        : target.Instance.Recipe != null
                            ? target.Instance.Recipe.StableIdentity
                            : "tree-geometry-audit-" + target.Name;
                generation = TreeGenerator.GenerateExactForValidation(
                    target.Instance.ExactControls,
                    target.Instance.MasterSeed,
                    sourceIdentity,
                    target.Instance.Family);
            }
            else if (target.Instance.Recipe != null)
            {
                generation = TreeGenerator.Generate(
                    target.Instance.Recipe,
                    target.Instance.InstanceOverrides,
                    target.Instance.MasterSeed);
            }
            else
            {
                generation = null;
            }
            stopwatch.Stop();
            target.MeasuredGenerationMilliseconds =
                stopwatch.Elapsed.TotalMilliseconds;
            target.FreshFingerprintMatched =
                generation != null &&
                generation.Passed &&
                generation.Definition != null &&
                generation.Definition.IsValid &&
                generation.Definition.StructuralFingerprint ==
                    serializedDefinition.StructuralFingerprint;
        }

        private static List<Target> CollectTargets(
            TreeReferenceGallery gallery)
        {
            var targets = new List<Target>(ExpectedTreeCount);
            Transform root = gallery != null
                ? gallery.transform.Find(
                    TreeReferenceGalleryBuilder.CompleteGalleryRootName)
                : null;
            if (root == null)
            {
                return targets;
            }

            ProceduralTreeInstance[] instances =
                root.GetComponentsInChildren<ProceduralTreeInstance>(true);
            Array.Sort(instances, CompareInstances);
            for (int index = 0; index < instances.Length; index++)
            {
                ProceduralTreeInstance instance = instances[index];
                if (instance == null || !instance.HasGeneratedDefinition)
                {
                    continue;
                }

                Renderer[] renderers =
                    instance.GetComponentsInChildren<Renderer>(true);
                int drawCallEstimate = 0;
                int shadowCasterRendererCount = 0;
                int shadowCasterDrawCallEstimate = 0;
                long shadowCasterTriangleEstimate = 0L;
                for (int rendererIndex = 0;
                     rendererIndex < renderers.Length;
                     rendererIndex++)
                {
                    Renderer renderer = renderers[rendererIndex];
                    if (renderer == null || !renderer.enabled ||
                        !renderer.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    Material[] sharedMaterials = renderer.sharedMaterials;
                    drawCallEstimate += sharedMaterials != null
                        ? sharedMaterials.Length
                        : 0;
                    if (renderer.shadowCastingMode != ShadowCastingMode.Off)
                    {
                        shadowCasterRendererCount++;
                        shadowCasterDrawCallEstimate += sharedMaterials != null
                            ? sharedMaterials.Length
                            : 0;
                        shadowCasterTriangleEstimate +=
                            EstimateRendererTriangleCount(renderer);
                    }
                }

                Mesh existingMesh = instance.GeneratedBarkMesh;
                targets.Add(new Target
                {
                    Instance = instance,
                    Name = instance.Family + " " +
                        instance.SourceVariantIndex,
                    Family = instance.Family,
                    Variant = instance.SourceVariantIndex,
                    RendererCount = renderers.Length,
                    DrawCallEstimate = drawCallEstimate,
                    ShadowCasterRendererCount =
                        shadowCasterRendererCount,
                    ShadowCasterDrawCallEstimate =
                        shadowCasterDrawCallEstimate,
                    ShadowCasterTriangleEstimate =
                        shadowCasterTriangleEstimate,
                    ExistingMeshVertices = existingMesh != null
                        ? existingMesh.vertexCount
                        : 0,
                    ExistingMeshTriangles = existingMesh != null &&
                        existingMesh.subMeshCount > 0
                            ? (int)(existingMesh.GetIndexCount(0) / 3)
                            : 0
                });
            }

            return targets;
        }

        private static long EstimateRendererTriangleCount(
            Renderer renderer)
        {
            if (renderer == null)
            {
                return 0L;
            }

            Mesh mesh = renderer is SkinnedMeshRenderer skinned
                ? skinned.sharedMesh
                : renderer.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null)
            {
                return 0L;
            }

            long triangleCount = 0L;
            for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
            {
                if (mesh.GetTopology(subMesh) == MeshTopology.Triangles)
                {
                    triangleCount += (long)mesh.GetIndexCount(subMesh) / 3L;
                }
            }
            return triangleCount;
        }

        private static int CompareInstances(
            ProceduralTreeInstance a,
            ProceduralTreeInstance b)
        {
            int family = a.Family.CompareTo(b.Family);
            return family != 0
                ? family
                : a.SourceVariantIndex.CompareTo(b.SourceVariantIndex);
        }

        private static bool Advance(Job job)
        {
            job.PolicyIndex++;
            if (job.PolicyIndex < Policies.Length)
            {
                return true;
            }

            job.PolicyIndex = 0;
            job.TargetIndex++;
            return job.TargetIndex < job.Targets.Count;
        }

        private static void Finish(
            Job job,
            string outcome,
            string fatalFailure)
        {
            if (job == null)
            {
                return;
            }

            EditorApplication.update -= Tick;
            AssemblyReloadEvents.beforeAssemblyReload -= AbortForReload;
            EditorApplication.quitting -= AbortForQuit;
            if (job.PendingCapture != null)
            {
                PendingCapture pending = job.PendingCapture;
                job.PendingCapture = null;
                if (IsCaptureComplete(pending))
                {
                    ReleasePendingCapture(pending);
                }
                else
                {
                    pending.Abandoned = true;
                }
            }
            try
            {
                job.AggregateWriter?.Flush();
                job.AggregateWriter?.Dispose();
                job.BranchWriter?.Flush();
                job.BranchWriter?.Dispose();
            }
            catch
            {
                // Preserve the primary report path and outcome.
            }

            WriteCheckpoint(job, outcome, fatalFailure);
            activeJob = null;
            currentProgress = outcome == "COMPLETE"
                ? 1f
                : currentProgress;
            currentDetail = "Audit " + outcome;
            currentEta = "Report checkpointed";
            TreeGeometryEfficiencyAuditWindow.RepaintOpenWindow();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            string message =
                "[TREE-GEOMETRY.3B] Geometry-efficiency audit " + outcome +
                ". Completed=" + job.CompletedCases +
                ", passed=" + job.PassedCases +
                ", failed=" + job.FailedCases +
                ". Report: " + job.ReportPath;
            if (outcome == "COMPLETE" && job.FailedCases == 0)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogWarning(message);
            }
        }

        private static void WriteCheckpoint(
            Job job,
            string outcome,
            string fatalFailure = null)
        {
            if (job == null)
            {
                return;
            }

            var report = new StringBuilder(65536);
            report.AppendLine("# TREE-GEOMETRY.3B — Procedural Tree Geometry Efficiency Audit");
            report.AppendLine();
            report.Append("- Generated UTC: ")
                .AppendLine(DateTime.UtcNow.ToString("O"));
            report.Append("- Outcome: ").AppendLine(outcome);
            report.Append("- Generator / bark version: ")
                .Append(TreeGenerator.CurrentGeneratorVersion)
                .Append(" / ")
                .AppendLine(TreeBarkMeshGenerator.BarkAlgorithmVersion.ToString());
            report.Append("- Completed / passed / failed cases: ")
                .Append(job.CompletedCases).Append(" / ")
                .Append(job.PassedCases).Append(" / ")
                .AppendLine(job.FailedCases.ToString());
            report.Append("- Trees / policies: ")
                .Append(job.Targets.Count).Append(" / ")
                .AppendLine(Policies.Length.ToString());
            report.Append("- Capture camera source: ")
                .AppendLine(job.CaptureCameraSource);
            report.Append("- Elapsed: ")
                .AppendLine(FormatDuration(
                    (DateTime.UtcNow - job.StartedUtc).TotalSeconds));
            report.AppendLine("- Traditional distance LOD: excluded.");
            report.AppendLine("- Production Current is the accepted Patch 1 axial plus contour-owned radial policy; Radial Aggressive remains diagnostic-only.");
            report.AppendLine("- Capture pipeline: isolated preview Scene plus polled AsyncGPUReadback; no synchronous GPU readback or wait.");
            if (!string.IsNullOrEmpty(fatalFailure))
            {
                report.AppendLine();
                report.AppendLine("## Fatal failure");
                report.AppendLine();
                report.AppendLine("```text");
                report.AppendLine(fatalFailure);
                report.AppendLine("```");
            }

            AppendPolicyDefinitions(report);
            AppendAggregateSummary(report, job);
            AppendStructureAndStorageSummary(report, job);
            AppendPerTreeSummary(report, job);
            AppendRecommendations(report, job, outcome);
            report.AppendLine();
            report.AppendLine("## Output files");
            report.AppendLine();
            report.Append("- Aggregate CSV: `")
                .Append(job.AggregateCsvPath).AppendLine("`");
            report.Append("- Per-branch CSV: `")
                .Append(job.BranchCsvPath).AppendLine("`");
            report.Append("- Captures: `")
                .Append(job.CaptureDirectory).AppendLine("`");
            File.WriteAllText(
                job.ReportPath,
                report.ToString(),
                Encoding.UTF8);
        }

        private static void AppendPolicyDefinitions(
            StringBuilder report)
        {
            report.AppendLine();
            report.AppendLine("## Diagnostic policy definitions");
            report.AppendLine();
            report.AppendLine("- **Production Current:** the accepted production representation: Patch 1 adaptive axial sampling plus contour-owned trunk radial tiers and radius-aware branch sides.");
            report.AppendLine("- **Legacy Pre-Patch-1:** exact pre-Patch-1 axial sampling with uniform Root Count × 10 trunk radial density and every non-trunk structural sample. It is retained only as the historical geometry baseline.");
            report.AppendLine("- **Radial Aggressive:** the production contour-owned architecture with lower samples-per-lobe, circular-trunk, and branch radial targets. It establishes the visible faceting boundary and remains diagnostic-only.");
            report.AppendLine("- Buttress Persistence is authoritative. If lobes remain active at Persistence 1.000, the production trunk remains lobe-owned to the tip and is never forcibly converted to a circular tube.");
            report.AppendLine("- Mixed-resolution strips use deterministic lobe-sector zipper stitching while both rings are lobe-owned, then deterministic angular zipper stitching after circular release.");
        }

        private static void AppendAggregateSummary(
            StringBuilder report,
            Job job)
        {
            report.AppendLine();
            report.AppendLine("## Aggregate policy results");
            report.AppendLine();
            report.AppendLine("| Policy | Complete trees | Topology passes | Vertices | Triangles | Mesh estimate | Vertex reduction vs Legacy | Triangle reduction vs Legacy | Mean silhouette deviation vs Production |");
            report.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|");
            TreeBarkMeshEfficiencyPolicy baselinePolicy =
                TreeBarkMeshEfficiencyPolicy.LegacyCurrent;
            long baselineVertices = SumPolicy(
                job, baselinePolicy, r => r.Bark.VertexCount);
            long baselineTriangles = SumPolicy(
                job, baselinePolicy, r => r.Bark.TriangleCount);
            int baselineResultCount = 0;
            int baselinePassCount = 0;
            for (int index = 0; index < job.Results.Count; index++)
            {
                CaseResult baseline = job.Results[index];
                if (baseline.Policy != baselinePolicy)
                {
                    continue;
                }
                baselineResultCount++;
                if (baseline.Passed)
                {
                    baselinePassCount++;
                }
            }

            for (int policyIndex = 0;
                 policyIndex < Policies.Length;
                 policyIndex++)
            {
                TreeBarkMeshEfficiencyPolicy policy = Policies[policyIndex];
                int resultCount = 0;
                int passCount = 0;
                long vertices = 0;
                long triangles = 0;
                long bytes = 0;
                float deviationTotal = 0f;
                int deviationCount = 0;
                for (int index = 0; index < job.Results.Count; index++)
                {
                    CaseResult item = job.Results[index];
                    if (item.Policy != policy)
                    {
                        continue;
                    }
                    resultCount++;
                    if (item.Passed && item.Bark != null)
                    {
                        passCount++;
                        vertices += item.Bark.VertexCount;
                        triangles += item.Bark.TriangleCount;
                        bytes += item.Bark.EstimatedMeshBytes;
                    }
                    if (item.SilhouetteMeasured)
                    {
                        deviationTotal += item.SilhouetteDeviation;
                        deviationCount++;
                    }
                }

                bool comparableWithBaseline =
                    baselineResultCount > 0 &&
                    baselinePassCount == baselineResultCount &&
                    resultCount == baselineResultCount &&
                    passCount == resultCount;
                report.Append("| ").Append(PolicyDisplayName(policy))
                    .Append(" | ").Append(resultCount)
                    .Append(" | ").Append(passCount)
                    .Append(" | ").Append(vertices)
                    .Append(" | ").Append(triangles)
                    .Append(" | ").Append(FormatBytes(bytes))
                    .Append(" | ")
                    .Append(comparableWithBaseline
                        ? FormatPercentReduction(baselineVertices, vertices)
                        : "n/a")
                    .Append(" | ")
                    .Append(comparableWithBaseline
                        ? FormatPercentReduction(baselineTriangles, triangles)
                        : "n/a")
                    .Append(" | ")
                    .Append(deviationCount > 0
                        ? (deviationTotal / deviationCount).ToString(
                            "P2", CultureInfo.InvariantCulture)
                        : "n/a")
                    .AppendLine(" |");
            }

            report.AppendLine();
            report.AppendLine("### Aggregate geometry categories");
            report.AppendLine();
            report.AppendLine("| Policy | Root lobe V/T | Persistence V/T | Ordinary trunk V/T | Primary V/T | Secondary V/T | Tertiary V/T | Caps V/T | Seam duplicates | Root/twist/shape inserted | Adaptive/topology removed |");
            report.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
            for (int policyIndex = 0; policyIndex < Policies.Length; policyIndex++)
            {
                TreeBarkMeshEfficiencyPolicy policy = Policies[policyIndex];
                GeometryTotals totals = AggregatePolicyGeometry(job, policy);
                report.Append("| ").Append(PolicyDisplayName(policy))
                    .Append(" | ").Append(totals.RootLobeVertices).Append('/').Append(totals.RootLobeTriangles)
                    .Append(" | ").Append(totals.PersistenceVertices).Append('/').Append(totals.PersistenceTriangles)
                    .Append(" | ").Append(totals.OrdinaryTrunkVertices).Append('/').Append(totals.OrdinaryTrunkTriangles)
                    .Append(" | ").Append(totals.PrimaryVertices).Append('/').Append(totals.PrimaryTriangles)
                    .Append(" | ").Append(totals.SecondaryVertices).Append('/').Append(totals.SecondaryTriangles)
                    .Append(" | ").Append(totals.TertiaryVertices).Append('/').Append(totals.TertiaryTriangles)
                    .Append(" | ").Append(totals.CapVertices).Append('/').Append(totals.CapTriangles)
                    .Append(" | ").Append(totals.SeamVertices)
                    .Append(" | ").Append(totals.RootRefinementInsertedRings).Append('/').Append(totals.TwistRefinementInsertedRings).Append('/').Append(totals.AdaptiveShapeRefinementInsertedRings)
                    .Append(" | ").Append(totals.EfficiencyPolicyRemovedRings).Append('/').Append(totals.TopologyRepairRemovedRings)
                    .AppendLine(" |");
            }

            report.AppendLine();
            report.AppendLine("### Radial transition accounting");
            report.AppendLine();
            report.AppendLine("| Policy | Trunk radial min–max | Mean trunk radial | Radial transitions | Mixed-resolution strips | Stitch triangles |");
            report.AppendLine("|---|---:|---:|---:|---:|---:|");
            for (int policyIndex = 0; policyIndex < Policies.Length; policyIndex++)
            {
                TreeBarkMeshEfficiencyPolicy policy = Policies[policyIndex];
                int minimum = int.MaxValue;
                int maximum = 0;
                double averageTotal = 0.0;
                int averageCount = 0;
                int transitions = 0;
                int mixedStrips = 0;
                int stitchTriangles = 0;
                for (int resultIndex = 0; resultIndex < job.Results.Count; resultIndex++)
                {
                    CaseResult item = job.Results[resultIndex];
                    if (item.Policy != policy || !item.Passed || item.Bark == null)
                    {
                        continue;
                    }
                    minimum = Mathf.Min(minimum, item.Bark.MinimumEffectiveTrunkRadialSegments);
                    maximum = Mathf.Max(maximum, item.Bark.MaximumEffectiveTrunkRadialSegments);
                    averageTotal += item.Bark.AverageEffectiveTrunkRadialSegments;
                    averageCount++;
                    transitions += item.Bark.TrunkRadialTransitionCount;
                    mixedStrips += item.Bark.TrunkMixedResolutionStripCount;
                    stitchTriangles += item.Bark.TrunkStitchTriangleCount;
                }
                report.Append("| ").Append(PolicyDisplayName(policy))
                    .Append(" | ").Append(minimum == int.MaxValue ? 0 : minimum).Append('–').Append(maximum)
                    .Append(" | ").Append(averageCount > 0 ? (averageTotal / averageCount).ToString("F2", CultureInfo.InvariantCulture) : "0")
                    .Append(" | ").Append(transitions)
                    .Append(" | ").Append(mixedStrips)
                    .Append(" | ").Append(stitchTriangles)
                    .AppendLine(" |");
            }

            long productionVertices = SumPolicy(
                job,
                TreeBarkMeshEfficiencyPolicy.Current,
                r => r.Bark.VertexCount);
            long productionTriangles = SumPolicy(
                job,
                TreeBarkMeshEfficiencyPolicy.Current,
                r => r.Bark.TriangleCount);
            if (PolicyPassedAll(job, TreeBarkMeshEfficiencyPolicy.Current) &&
                productionVertices > 0 && productionTriangles > 0)
            {
                report.AppendLine();
                report.Append("Known recent HEAD aggregate supplied by the handoff: **")
                    .Append(KnownRecentHeadVertices).Append(" vertices / ")
                    .Append(KnownRecentHeadTriangles)
                    .AppendLine(" triangles**. The audit cannot recreate that historical revision; this remains descriptive evidence only.");
                report.Append("Measured Production Current / known recent HEAD ratio: **")
                    .Append((productionVertices / (double)KnownRecentHeadVertices).ToString("F2", CultureInfo.InvariantCulture))
                    .Append("× vertices / ")
                    .Append((productionTriangles / (double)KnownRecentHeadTriangles).ToString("F2", CultureInfo.InvariantCulture))
                    .AppendLine("× triangles**.");
            }
        }

        private static void AppendStructureAndStorageSummary(
            StringBuilder report,
            Job job)
        {
            long serializedJsonBytes = 0L;
            long rawStructuralBytes = 0L;
            int branchCount = 0;
            int controlPointCount = 0;
            int curveSampleCount = 0;
            int freshFingerprintMatches = 0;
            int existingMeshComparisons = 0;
            int existingMeshMatches = 0;
            int rendererCount = 0;
            int drawCallEstimate = 0;
            int shadowCasterRendererCount = 0;
            int shadowCasterDrawCallEstimate = 0;
            long shadowCasterTriangleEstimate = 0L;
            for (int targetIndex = 0;
                 targetIndex < job.Targets.Count;
                 targetIndex++)
            {
                Target target = job.Targets[targetIndex];
                serializedJsonBytes += target.SerializedJsonBytes;
                rawStructuralBytes += target.RawStructuralBytes;
                if (target.Definition?.Metrics != null)
                {
                    branchCount += target.Definition.Metrics.BranchCount;
                    controlPointCount +=
                        target.Definition.Metrics.ControlPointCount;
                    curveSampleCount +=
                        target.Definition.Metrics.CurveSampleCount;
                }
                if (target.FreshFingerprintMatched)
                {
                    freshFingerprintMatches++;
                }
                rendererCount += target.RendererCount;
                drawCallEstimate += target.DrawCallEstimate;
                shadowCasterRendererCount +=
                    target.ShadowCasterRendererCount;
                shadowCasterDrawCallEstimate +=
                    target.ShadowCasterDrawCallEstimate;
                shadowCasterTriangleEstimate +=
                    target.ShadowCasterTriangleEstimate;
            }

            for (int resultIndex = 0;
                 resultIndex < job.Results.Count;
                 resultIndex++)
            {
                CaseResult result = job.Results[resultIndex];
                if (result.Policy !=
                        TreeBarkMeshEfficiencyPolicy.Current ||
                    result.Target.ExistingMeshVertices <= 0)
                {
                    continue;
                }
                existingMeshComparisons++;
                if (result.ExistingBaselineMeshMatched)
                {
                    existingMeshMatches++;
                }
            }

            report.AppendLine();
            report.AppendLine("## Structure, serialization, and renderer accounting");
            report.AppendLine();
            report.Append("- Structural branches / control points / transported samples: **")
                .Append(branchCount).Append(" / ")
                .Append(controlPointCount).Append(" / ")
                .Append(curveSampleCount).AppendLine("**.");
            report.Append("- Serialized `TreeDefinition` JSON payload estimate: **")
                .Append(FormatBytes(serializedJsonBytes))
                .AppendLine("**.");
            report.Append("- Raw field payload estimate before Unity serialization overhead: **")
                .Append(FormatBytes(rawStructuralBytes))
                .AppendLine("**.");
            report.Append("- Fresh deterministic structure fingerprints matching the serialized gallery definitions: **")
                .Append(freshFingerprintMatches).Append(" / ")
                .Append(job.Targets.Count).AppendLine("**.");
            report.Append("- Existing generated bark count parity for completed Production Current cases: **")
                .Append(existingMeshMatches).Append(" / ")
                .Append(existingMeshComparisons).AppendLine("**.");
            report.Append("- Renderers / estimated active material draws beneath the twenty instances: **")
                .Append(rendererCount).Append(" / ")
                .Append(drawCallEstimate).AppendLine("**.");
            report.Append("- Shadow-casting renderers / estimated shadow draws / source-mesh triangle estimate for those renderers: **")
                .Append(shadowCasterRendererCount).Append(" / ")
                .Append(shadowCasterDrawCallEstimate).Append(" / ")
                .Append(shadowCasterTriangleEstimate).AppendLine("**.");
            report.AppendLine("- Mesh-memory figures are vertex/index payload estimates and exclude Unity object, allocator, and driver overhead.");
        }

        private static void AppendPerTreeSummary(
            StringBuilder report,
            Job job)
        {
            report.AppendLine();
            report.AppendLine("## Completed cases");
            report.AppendLine();
            report.AppendLine("| Tree | Policy | Status | Vertices | Triangles | Trunk rings/sides | Root intervals | Build ms | Topology ms | Silhouette deviation | Capture |");
            report.AppendLine("|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|");
            for (int index = 0; index < job.Results.Count; index++)
            {
                CaseResult item = job.Results[index];
                TreeBarkMeshBuildResult bark = item.Bark;
                report.Append("| ").Append(item.Target.Name)
                    .Append(" | ").Append(item.Policy)
                    .Append(" | ").Append(item.Passed ? "PASS" : "FAIL")
                    .Append(" | ").Append(bark != null ? bark.VertexCount : 0)
                    .Append(" | ").Append(bark != null ? bark.TriangleCount : 0)
                    .Append(" | ").Append(bark != null
                        ? bark.EffectiveTrunkRingCount + "/" +
                            bark.MinimumEffectiveTrunkRadialSegments + "–" +
                            bark.MaximumEffectiveTrunkRadialSegments +
                            " (avg " + bark.AverageEffectiveTrunkRadialSegments.ToString(
                                "F1", CultureInfo.InvariantCulture) + ")"
                        : "0/0")
                    .Append(" | ").Append(bark != null
                        ? bark.RootZoneLongitudinalIntervals
                        : 0)
                    .Append(" | ").Append(bark != null
                        ? bark.TotalBuildMilliseconds.ToString(
                            "F2",
                            CultureInfo.InvariantCulture)
                        : "0")
                    .Append(" | ").Append(bark != null
                        ? bark.TopologyAuditMilliseconds.ToString(
                            "F2",
                            CultureInfo.InvariantCulture)
                        : "0")
                    .Append(" | ").Append(item.SilhouetteMeasured
                        ? item.SilhouetteDeviation.ToString(
                            "P2",
                            CultureInfo.InvariantCulture)
                        : "n/a")
                    .Append(" | ").Append(string.IsNullOrEmpty(item.CapturePath)
                        ? "n/a"
                        : "`" + item.CapturePath + "`")
                    .AppendLine(" |");
                if (!string.IsNullOrEmpty(item.CaptureFailure))
                {
                    report.AppendLine();
                    report.Append("**").Append(item.Target.Name)
                        .Append(" / ").Append(item.Policy)
                        .Append(" capture unavailable:** `")
                        .Append(FirstFailureLine(item.CaptureFailure))
                        .AppendLine("`");
                }
                if (!item.Passed && !string.IsNullOrEmpty(item.Failure))
                {
                    report.AppendLine();
                    report.Append("**").Append(item.Target.Name)
                        .Append(" / ").Append(item.Policy)
                        .Append(" failure:** `")
                        .Append(FirstFailureLine(item.Failure))
                        .AppendLine("`");
                }
            }
        }

        private static void AppendRecommendations(
            StringBuilder report,
            Job job,
            string outcome)
        {
            report.AppendLine();
            report.AppendLine("## Decision gates and recommendations");
            report.AppendLine();
            if (outcome != "COMPLETE")
            {
                report.AppendLine("The audit is incomplete. Do not interpret partial production or diagnostic totals.");
                return;
            }

            bool productionComplete = PolicyPassedAll(
                job,
                TreeBarkMeshEfficiencyPolicy.Current);
            bool legacyComplete = PolicyPassedAll(
                job,
                TreeBarkMeshEfficiencyPolicy.LegacyCurrent);
            bool aggressiveComplete = PolicyPassedAll(
                job,
                TreeBarkMeshEfficiencyPolicy.RadialAggressive);
            long productionVertices = SumPolicy(
                job,
                TreeBarkMeshEfficiencyPolicy.Current,
                r => r.Bark.VertexCount);
            long productionTriangles = SumPolicy(
                job,
                TreeBarkMeshEfficiencyPolicy.Current,
                r => r.Bark.TriangleCount);
            long legacyVertices = SumPolicy(
                job,
                TreeBarkMeshEfficiencyPolicy.LegacyCurrent,
                r => r.Bark.VertexCount);
            long legacyTriangles = SumPolicy(
                job,
                TreeBarkMeshEfficiencyPolicy.LegacyCurrent,
                r => r.Bark.TriangleCount);
            double vertexReduction = productionComplete && legacyComplete &&
                legacyVertices > 0
                    ? 1.0 - productionVertices / (double)legacyVertices
                    : double.NaN;
            double triangleReduction = productionComplete && legacyComplete &&
                legacyTriangles > 0
                    ? 1.0 - productionTriangles / (double)legacyTriangles
                    : double.NaN;

            report.Append("- Production Current topology across all twenty trees: **")
                .Append(productionComplete ? "PASS" : "FAIL")
                .AppendLine("**.");
            report.Append("- Legacy Pre-Patch-1 topology across all twenty trees: **")
                .Append(legacyComplete ? "PASS" : "FAIL")
                .AppendLine("**.");
            report.Append("- Radial Aggressive topology across all twenty trees: **")
                .Append(aggressiveComplete ? "PASS" : "FAIL")
                .AppendLine("**.");
            report.Append("- Production Current aggregate reduction versus Legacy: **")
                .Append(double.IsNaN(vertexReduction)
                    ? "not comparable"
                    : vertexReduction.ToString("P2", CultureInfo.InvariantCulture))
                .Append(" vertices / ")
                .Append(double.IsNaN(triangleReduction)
                    ? "not comparable"
                    : triangleReduction.ToString("P2", CultureInfo.InvariantCulture))
                .AppendLine(" triangles**.");

            int meshComparisons = 0;
            int meshMatches = 0;
            int productionSilhouettes = 0;
            int legacySilhouettes = 0;
            int aggressiveSilhouettes = 0;
            for (int index = 0; index < job.Results.Count; index++)
            {
                CaseResult result = job.Results[index];
                if (result.Policy == TreeBarkMeshEfficiencyPolicy.Current)
                {
                    if (result.Target.ExistingMeshVertices > 0)
                    {
                        meshComparisons++;
                        if (result.ExistingBaselineMeshMatched)
                        {
                            meshMatches++;
                        }
                    }
                    if (result.SilhouetteMeasured)
                    {
                        productionSilhouettes++;
                    }
                }
                else if (result.Policy == TreeBarkMeshEfficiencyPolicy.LegacyCurrent &&
                         result.SilhouetteMeasured)
                {
                    legacySilhouettes++;
                }
                else if (result.Policy == TreeBarkMeshEfficiencyPolicy.RadialAggressive &&
                         result.SilhouetteMeasured)
                {
                    aggressiveSilhouettes++;
                }
            }

            bool meshParityComplete = meshComparisons == job.Targets.Count &&
                meshMatches == meshComparisons;
            bool visualEvidenceComplete =
                productionSilhouettes == job.Targets.Count &&
                legacySilhouettes == job.Targets.Count &&
                aggressiveSilhouettes == job.Targets.Count;
            report.Append("- Production Current generated-mesh count parity: **")
                .Append(meshMatches).Append(" / ").Append(meshComparisons)
                .Append(" — ").Append(meshParityComplete ? "PASS" : "FAIL")
                .AppendLine("**.");
            report.Append("- Fixed-camera silhouette coverage for Production / Legacy / Aggressive: **")
                .Append(productionSilhouettes).Append(" / ")
                .Append(legacySilhouettes).Append(" / ")
                .Append(aggressiveSilhouettes).Append(" of ")
                .Append(job.Targets.Count).Append(" — ")
                .Append(visualEvidenceComplete ? "PASS" : "FAIL")
                .AppendLine("**.");
            if (!meshParityComplete)
            {
                report.AppendLine("- **Production mesh parity failed. Rebuild the gallery with ordinary production generation before using this audit as a regression baseline.**");
            }
            report.AppendLine("- Production Current is the accepted representation. Radial Aggressive remains diagnostic-only and must not be selected by ordinary production factories.");
            report.AppendLine("- Persistence 1.000 must remain lobe-owned wherever the authored contour remains visibly lobed; no height-based circular release is permitted.");
            report.AppendLine("- Further geometry reduction is lower priority than mesh storage-layout, deterministic regeneration, and spatial streaming work.");
        }

        private static string PolicyDisplayName(
            TreeBarkMeshEfficiencyPolicy policy)
        {
            switch (policy)
            {
                case TreeBarkMeshEfficiencyPolicy.Current:
                    return "Production Current";
                case TreeBarkMeshEfficiencyPolicy.LegacyCurrent:
                    return "Legacy Pre-Patch-1";
                case TreeBarkMeshEfficiencyPolicy.RadialConservative:
                    return "Accepted Radial Conservative Alias";
                case TreeBarkMeshEfficiencyPolicy.RadialAggressive:
                    return "Radial Aggressive";
                default:
                    return policy.ToString();
            }
        }

        private static void WriteAggregateCase(
            StreamWriter writer,
            CaseResult result)
        {
            Target target = result.Target;
            TreeGenerationMetrics metrics = target.Definition != null
                ? target.Definition.Metrics
                : null;
            AggregateGeometry(result.Bark, out GeometryTotals totals);
            WriteCsvRow(writer, new[]
            {
                target.Name,
                target.Family.ToString(),
                target.Variant.ToString(CultureInfo.InvariantCulture),
                result.Policy.ToString(),
                result.Passed ? "PASS" : "FAIL",
                result.Failure ?? string.Empty,
                F(target.MeasuredGenerationMilliseconds),
                target.FreshFingerprintMatched ? "YES" : "NO",
                target.SerializedJsonBytes.ToString(CultureInfo.InvariantCulture),
                target.RawStructuralBytes.ToString(CultureInfo.InvariantCulture),
                I(metrics?.BranchCount),
                I(metrics?.PrimaryBranchCount),
                I(metrics?.SecondaryBranchCount),
                I(metrics?.TertiaryBranchCount),
                I(metrics?.RejectedBranchCount),
                I(metrics?.DeadBranchCount),
                I(metrics?.BrokenBranchCount),
                I(metrics?.ControlPointCount),
                I(metrics?.CurveSampleCount),
                I(result.Bark?.VertexCount),
                I(result.Bark?.TriangleCount),
                L(result.Bark?.EstimatedMeshBytes),
                D(result.Bark?.GeometryBuildMilliseconds),
                D(result.Bark?.TopologyAuditMilliseconds),
                D(result.Bark?.MeshUploadMilliseconds),
                D(result.Bark?.TotalBuildMilliseconds),
                I(result.Bark?.MinimumEffectiveTrunkRadialSegments),
                I(result.Bark?.MaximumEffectiveTrunkRadialSegments),
                D(result.Bark?.AverageEffectiveTrunkRadialSegments),
                I(result.Bark?.TrunkRadialTransitionCount),
                I(result.Bark?.TrunkMixedResolutionStripCount),
                I(result.Bark?.TrunkStitchTriangleCount),
                totals.TrunkVertices.ToString(CultureInfo.InvariantCulture),
                totals.TrunkTriangles.ToString(CultureInfo.InvariantCulture),
                totals.RootVertices.ToString(CultureInfo.InvariantCulture),
                totals.RootTriangles.ToString(CultureInfo.InvariantCulture),
                totals.RootLobeVertices.ToString(CultureInfo.InvariantCulture),
                totals.RootLobeTriangles.ToString(CultureInfo.InvariantCulture),
                totals.PersistenceVertices.ToString(CultureInfo.InvariantCulture),
                totals.PersistenceTriangles.ToString(CultureInfo.InvariantCulture),
                totals.OrdinaryTrunkVertices.ToString(CultureInfo.InvariantCulture),
                totals.OrdinaryTrunkTriangles.ToString(CultureInfo.InvariantCulture),
                totals.PrimaryVertices.ToString(CultureInfo.InvariantCulture),
                totals.PrimaryTriangles.ToString(CultureInfo.InvariantCulture),
                totals.SecondaryVertices.ToString(CultureInfo.InvariantCulture),
                totals.SecondaryTriangles.ToString(CultureInfo.InvariantCulture),
                totals.TertiaryVertices.ToString(CultureInfo.InvariantCulture),
                totals.TertiaryTriangles.ToString(CultureInfo.InvariantCulture),
                totals.CapVertices.ToString(CultureInfo.InvariantCulture),
                totals.CapTriangles.ToString(CultureInfo.InvariantCulture),
                totals.SeamVertices.ToString(CultureInfo.InvariantCulture),
                totals.SourceSamples.ToString(CultureInfo.InvariantCulture),
                totals.RenderRings.ToString(CultureInfo.InvariantCulture),
                totals.InsertedRings.ToString(CultureInfo.InvariantCulture),
                totals.RootRefinementInsertedRings.ToString(CultureInfo.InvariantCulture),
                totals.TwistRefinementInsertedRings.ToString(CultureInfo.InvariantCulture),
                totals.AdaptiveShapeRefinementInsertedRings.ToString(CultureInfo.InvariantCulture),
                totals.RemovedRings.ToString(CultureInfo.InvariantCulture),
                totals.EfficiencyPolicyRemovedRings.ToString(CultureInfo.InvariantCulture),
                totals.TopologyRepairRemovedRings.ToString(CultureInfo.InvariantCulture),
                I(result.Bark?.PhaseAlignedRingCount),
                I(result.Bark?.CurvatureRadiusClampCount),
                I(result.Bark?.CircularBranchRingRemovalCount),
                I(result.Bark?.TrunkTipRemovedRingCount),
                I(result.Bark?.AlternateQuadDiagonalCount),
                F(totals.AverageSegmentLength),
                F(totals.MaximumSegmentLength),
                F(totals.AverageTurn),
                F(totals.MaximumTurn),
                target.RendererCount.ToString(CultureInfo.InvariantCulture),
                target.DrawCallEstimate.ToString(CultureInfo.InvariantCulture),
                target.ShadowCasterRendererCount.ToString(
                    CultureInfo.InvariantCulture),
                target.ShadowCasterDrawCallEstimate.ToString(
                    CultureInfo.InvariantCulture),
                target.ShadowCasterTriangleEstimate.ToString(
                    CultureInfo.InvariantCulture),
                target.ExistingMeshVertices.ToString(CultureInfo.InvariantCulture),
                target.ExistingMeshTriangles.ToString(CultureInfo.InvariantCulture),
                result.Policy == TreeBarkMeshEfficiencyPolicy.Current
                    ? (result.ExistingBaselineMeshMatched ? "YES" : "NO")
                    : string.Empty,
                result.SilhouetteMeasured ? "YES" : "NO",
                result.SilhouetteMeasured
                    ? F(result.SilhouetteDeviation)
                    : string.Empty,
                result.CapturePath ?? string.Empty,
                result.CaptureFailure ?? string.Empty,
                result.Bark?.TopologyAudit != null
                    ? FirstFailureLine(result.Bark.TopologyAudit.Report)
                    : string.Empty
            });
        }

        private static void WriteBranchCases(
            StreamWriter writer,
            CaseResult result)
        {
            if (result.Bark?.BranchGeometryAccounting == null)
            {
                return;
            }

            IReadOnlyList<TreeBarkMeshBranchGeometryAccounting> records =
                result.Bark.BranchGeometryAccounting;
            for (int index = 0; index < records.Count; index++)
            {
                TreeBarkMeshBranchGeometryAccounting item = records[index];
                WriteCsvRow(writer, new[]
                {
                    result.Target.Name,
                    result.Target.Family.ToString(),
                    result.Target.Variant.ToString(CultureInfo.InvariantCulture),
                    result.Policy.ToString(),
                    item.StableBranchId.ToString(CultureInfo.InvariantCulture),
                    item.BranchOrder.ToString(CultureInfo.InvariantCulture),
                    item.SourceSampleCount.ToString(CultureInfo.InvariantCulture),
                    item.RenderRingCount.ToString(CultureInfo.InvariantCulture),
                    item.RadialSegments.ToString(CultureInfo.InvariantCulture),
                    item.MinimumRadialSegments.ToString(CultureInfo.InvariantCulture),
                    item.MaximumRadialSegments.ToString(CultureInfo.InvariantCulture),
                    F(item.AverageRadialSegments),
                    item.RadialTransitionCount.ToString(CultureInfo.InvariantCulture),
                    item.MixedResolutionStripCount.ToString(CultureInfo.InvariantCulture),
                    item.StitchTriangleCount.ToString(CultureInfo.InvariantCulture),
                    F(item.RootLobeAverageRadialSegments),
                    F(item.ButtressPersistenceAverageRadialSegments),
                    F(item.OrdinaryTrunkAverageRadialSegments),
                    item.SideVertexCount.ToString(CultureInfo.InvariantCulture),
                    item.SideTriangleCount.ToString(CultureInfo.InvariantCulture),
                    item.CapVertexCount.ToString(CultureInfo.InvariantCulture),
                    item.CapTriangleCount.ToString(CultureInfo.InvariantCulture),
                    item.SeamDuplicateVertexCount.ToString(CultureInfo.InvariantCulture),
                    item.RootZoneRingCount.ToString(CultureInfo.InvariantCulture),
                    item.RootZoneIntervalCount.ToString(CultureInfo.InvariantCulture),
                    item.RootZoneVertexCount.ToString(CultureInfo.InvariantCulture),
                    item.RootZoneTriangleCount.ToString(CultureInfo.InvariantCulture),
                    item.RootLobeRingCount.ToString(CultureInfo.InvariantCulture),
                    item.RootLobeIntervalCount.ToString(CultureInfo.InvariantCulture),
                    item.RootLobeVertexCount.ToString(CultureInfo.InvariantCulture),
                    item.RootLobeTriangleCount.ToString(CultureInfo.InvariantCulture),
                    item.ButtressPersistenceRingCount.ToString(CultureInfo.InvariantCulture),
                    item.ButtressPersistenceIntervalCount.ToString(CultureInfo.InvariantCulture),
                    item.ButtressPersistenceVertexCount.ToString(CultureInfo.InvariantCulture),
                    item.ButtressPersistenceTriangleCount.ToString(CultureInfo.InvariantCulture),
                    item.OrdinaryTrunkVertexCount.ToString(CultureInfo.InvariantCulture),
                    item.OrdinaryTrunkTriangleCount.ToString(CultureInfo.InvariantCulture),
                    item.InsertedRenderRingCount.ToString(CultureInfo.InvariantCulture),
                    item.RootRefinementInsertedRingCount.ToString(CultureInfo.InvariantCulture),
                    item.TwistRefinementInsertedRingCount.ToString(CultureInfo.InvariantCulture),
                    item.AdaptiveShapeRefinementInsertedRingCount.ToString(CultureInfo.InvariantCulture),
                    item.RemovedRenderRingCount.ToString(CultureInfo.InvariantCulture),
                    item.EfficiencyPolicyRemovedRingCount.ToString(CultureInfo.InvariantCulture),
                    item.TopologyRepairRemovedRingCount.ToString(CultureInfo.InvariantCulture),
                    F(item.AverageSegmentLength),
                    F(item.MaximumSegmentLength),
                    F(item.AverageTurnDegrees),
                    F(item.MaximumTurnDegrees)
                });
            }
        }

        private struct GeometryTotals
        {
            internal int TrunkVertices;
            internal int TrunkTriangles;
            internal int RootVertices;
            internal int RootTriangles;
            internal int RootLobeVertices;
            internal int RootLobeTriangles;
            internal int PersistenceVertices;
            internal int PersistenceTriangles;
            internal int OrdinaryTrunkVertices;
            internal int OrdinaryTrunkTriangles;
            internal int PrimaryVertices;
            internal int PrimaryTriangles;
            internal int SecondaryVertices;
            internal int SecondaryTriangles;
            internal int TertiaryVertices;
            internal int TertiaryTriangles;
            internal int CapVertices;
            internal int CapTriangles;
            internal int SeamVertices;
            internal int SourceSamples;
            internal int RenderRings;
            internal int InsertedRings;
            internal int RootRefinementInsertedRings;
            internal int TwistRefinementInsertedRings;
            internal int AdaptiveShapeRefinementInsertedRings;
            internal int RemovedRings;
            internal int EfficiencyPolicyRemovedRings;
            internal int TopologyRepairRemovedRings;
            internal float AverageSegmentLength;
            internal float MaximumSegmentLength;
            internal float AverageTurn;
            internal float MaximumTurn;
        }

        private static void AggregateGeometry(
            TreeBarkMeshBuildResult bark,
            out GeometryTotals totals)
        {
            totals = default;
            if (bark?.BranchGeometryAccounting == null)
            {
                return;
            }

            int recordCount = 0;
            for (int index = 0;
                 index < bark.BranchGeometryAccounting.Count;
                 index++)
            {
                TreeBarkMeshBranchGeometryAccounting item =
                    bark.BranchGeometryAccounting[index];
                int sideVertices = item.SideVertexCount;
                int sideTriangles = item.SideTriangleCount;
                if (item.BranchOrder == 0)
                {
                    totals.TrunkVertices += sideVertices;
                    totals.TrunkTriangles += sideTriangles;
                    totals.RootVertices += item.RootZoneVertexCount;
                    totals.RootTriangles += item.RootZoneTriangleCount;
                    totals.RootLobeVertices += item.RootLobeVertexCount;
                    totals.RootLobeTriangles += item.RootLobeTriangleCount;
                    totals.PersistenceVertices +=
                        item.ButtressPersistenceVertexCount;
                    totals.PersistenceTriangles +=
                        item.ButtressPersistenceTriangleCount;
                    totals.OrdinaryTrunkVertices +=
                        item.OrdinaryTrunkVertexCount;
                    totals.OrdinaryTrunkTriangles +=
                        item.OrdinaryTrunkTriangleCount;
                }
                else if (item.BranchOrder == 1)
                {
                    totals.PrimaryVertices += sideVertices;
                    totals.PrimaryTriangles += sideTriangles;
                }
                else if (item.BranchOrder == 2)
                {
                    totals.SecondaryVertices += sideVertices;
                    totals.SecondaryTriangles += sideTriangles;
                }
                else
                {
                    totals.TertiaryVertices += sideVertices;
                    totals.TertiaryTriangles += sideTriangles;
                }

                totals.CapVertices += item.CapVertexCount;
                totals.CapTriangles += item.CapTriangleCount;
                totals.SeamVertices += item.SeamDuplicateVertexCount;
                totals.SourceSamples += item.SourceSampleCount;
                totals.RenderRings += item.RenderRingCount;
                totals.InsertedRings += item.InsertedRenderRingCount;
                totals.RootRefinementInsertedRings +=
                    item.RootRefinementInsertedRingCount;
                totals.TwistRefinementInsertedRings +=
                    item.TwistRefinementInsertedRingCount;
                totals.AdaptiveShapeRefinementInsertedRings +=
                    item.AdaptiveShapeRefinementInsertedRingCount;
                totals.RemovedRings += item.RemovedRenderRingCount;
                totals.EfficiencyPolicyRemovedRings +=
                    item.EfficiencyPolicyRemovedRingCount;
                totals.TopologyRepairRemovedRings +=
                    item.TopologyRepairRemovedRingCount;
                totals.AverageSegmentLength += item.AverageSegmentLength;
                totals.MaximumSegmentLength = Mathf.Max(
                    totals.MaximumSegmentLength,
                    item.MaximumSegmentLength);
                totals.AverageTurn += item.AverageTurnDegrees;
                totals.MaximumTurn = Mathf.Max(
                    totals.MaximumTurn,
                    item.MaximumTurnDegrees);
                recordCount++;
            }

            if (recordCount > 0)
            {
                totals.AverageSegmentLength /= recordCount;
                totals.AverageTurn /= recordCount;
            }
        }

        private static long EstimateRawStructuralBytes(TreeDefinition definition)
        {
            if (definition == null || definition.Branches == null)
            {
                return 0L;
            }

            long bytes = 0L;
            for (int index = 0; index < definition.Branches.Count; index++)
            {
                TreeBranchDefinition branch = definition.Branches[index];
                if (branch == null)
                {
                    continue;
                }
                bytes += 64L;
                bytes += branch.ControlPoints.Count * 12L;
                bytes += branch.Samples.Count * 56L;
            }
            return bytes;
        }

        private static bool BeginSilhouetteCapture(
            Job job,
            Target target,
            TreeBarkMeshEfficiencyPolicy policy,
            Mesh mesh,
            CaseResult result)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null)
            {
                result.CaptureFailure =
                    "Neither the URP Unlit nor fallback Unlit/Color capture shader was available.";
                return false;
            }

            Scene previewScene = default;
            bool previewSceneCreated = false;
            Material material = null;
            RenderTexture renderTexture = null;
            Camera camera = null;
            bool ownershipTransferred = false;
            try
            {
                previewScene = EditorSceneManager.NewPreviewScene();
                previewSceneCreated = true;
                var objectRoot = new GameObject(
                    "TREE-GEOMETRY.3B Capture Mesh")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                SceneManager.MoveGameObjectToScene(objectRoot, previewScene);
                MeshFilter filter = objectRoot.AddComponent<MeshFilter>();
                MeshRenderer renderer = objectRoot.AddComponent<MeshRenderer>();
                filter.sharedMesh = mesh;
                material = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", Color.white);
                }
                if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", Color.white);
                }
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                objectRoot.transform.SetPositionAndRotation(
                    Vector3.zero,
                    target.Instance.transform.rotation);
                objectRoot.transform.localScale =
                    target.Instance.transform.lossyScale;

                var cameraObject = new GameObject(
                    "TREE-GEOMETRY.3B Capture Camera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                SceneManager.MoveGameObjectToScene(
                    cameraObject,
                    previewScene);
                camera = cameraObject.AddComponent<Camera>();
                camera.scene = previewScene;
                camera.enabled = false;
                camera.orthographic = job.CaptureOrthographic;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.cullingMask = ~0;
                camera.aspect = job.CaptureWidth /
                    (float)Mathf.Max(1, job.CaptureHeight);
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.nearClipPlane = job.CaptureNearClipPlane;
                camera.farClipPlane = job.CaptureFarClipPlane;
                camera.transform.rotation = job.CaptureRotation;
                camera.projectionMatrix = job.CaptureProjectionMatrix;

                Bounds worldBounds = TransformBounds(
                    mesh.bounds,
                    objectRoot.transform.localToWorldMatrix);
                PositionFixedProjectionCamera(camera, worldBounds, job);
                renderTexture = new RenderTexture(
                    job.CaptureWidth,
                    job.CaptureHeight,
                    24,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear)
                {
                    name = "TREE-GEOMETRY.3B Async Capture",
                    hideFlags = HideFlags.HideAndDontSave,
                    antiAliasing = 1,
                    useMipMap = false,
                    autoGenerateMips = false
                };
                if (!renderTexture.Create())
                {
                    result.CaptureFailure =
                        "The asynchronous capture RenderTexture could not be created.";
                    return false;
                }

                camera.targetTexture = renderTexture;
                camera.Render();
                camera.targetTexture = null;

                string fileName = SanitizeFileName(target.Name) + "_" +
                    policy + ".png";
                var pending = new PendingCapture
                {
                    Result = result,
                    RenderTexture = renderTexture,
                    CapturePath = Path.Combine(
                        job.CaptureDirectory,
                        fileName),
                    Width = job.CaptureWidth,
                    Height = job.CaptureHeight
                };
                pending.Request = AsyncGPUReadback.Request(
                    renderTexture,
                    0,
                    TextureFormat.RGBA32,
                    completedRequest =>
                    {
                        pending.CompletedRequest = completedRequest;
                        pending.CallbackCompleted = true;
                        if (pending.Abandoned)
                        {
                            ReleasePendingCapture(pending);
                        }
                    });
                job.PendingCapture = pending;
                ownershipTransferred = true;
                return true;
            }
            catch (Exception exception)
            {
                result.CaptureFailure = exception.GetType().Name + ": " +
                    FirstFailureLine(exception.Message);
                result.CapturePath = string.Empty;
                return false;
            }
            finally
            {
                if (camera != null)
                {
                    camera.targetTexture = null;
                }
                if (!ownershipTransferred && renderTexture != null)
                {
                    renderTexture.Release();
                    UnityEngine.Object.DestroyImmediate(renderTexture);
                }
                if (material != null)
                {
                    UnityEngine.Object.DestroyImmediate(material);
                }
                if (previewSceneCreated && previewScene.IsValid())
                {
                    EditorSceneManager.ClosePreviewScene(previewScene);
                }
            }
        }

        private static bool IsCaptureComplete(PendingCapture pending)
        {
            return pending != null &&
                (pending.CallbackCompleted || pending.Request.done);
        }

        private static void CompletePendingCapture(Job job)
        {
            PendingCapture pending = job.PendingCapture;
            job.PendingCapture = null;
            CaseResult result = pending.Result;
            Texture2D texture = null;
            try
            {
                AsyncGPUReadbackRequest request = pending.CallbackCompleted
                    ? pending.CompletedRequest
                    : pending.Request;
                if (request.hasError)
                {
                    result.CaptureFailure =
                        "AsyncGPUReadback reported a GPU capture error.";
                    result.CapturePath = string.Empty;
                }
                else
                {
                    var data = request.GetData<Color32>();
                    int expectedPixels = pending.Width * pending.Height;
                    if (data.Length != expectedPixels)
                    {
                        result.CaptureFailure =
                            "AsyncGPUReadback returned " + data.Length +
                            " pixels; expected " + expectedPixels + ".";
                        result.CapturePath = string.Empty;
                    }
                    else
                    {
                        var pixels = new Color32[expectedPixels];
                        for (int index = 0; index < pixels.Length; index++)
                        {
                            pixels[index] = data[index];
                        }

                        bool[] silhouette = BuildSilhouette(
                            pixels,
                            pending.Width,
                            pending.Height,
                            out string silhouetteFailure);
                        result.CaptureFailure = silhouetteFailure;
                        texture = new Texture2D(
                            pending.Width,
                            pending.Height,
                            TextureFormat.RGBA32,
                            false,
                            true)
                        {
                            hideFlags = HideFlags.HideAndDontSave
                        };
                        texture.SetPixels32(pixels);
                        texture.Apply(false, false);
                        File.WriteAllBytes(
                            pending.CapturePath,
                            texture.EncodeToPNG());
                        result.CapturePath = pending.CapturePath;
                        ApplySilhouetteComparison(
                            job,
                            result,
                            silhouette);
                    }
                }
            }
            catch (Exception exception)
            {
                result.CaptureFailure = exception.GetType().Name + ": " +
                    FirstFailureLine(exception.Message);
                result.CapturePath = string.Empty;
            }
            finally
            {
                if (texture != null)
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
                ReleasePendingCapture(pending);
            }

            FinalizeCase(job, result);
        }

        private static bool[] BuildSilhouette(
            Color32[] pixels,
            int width,
            int height,
            out string failure)
        {
            failure = string.Empty;
            var silhouette = new bool[pixels.Length];
            int silhouettePixels = 0;
            bool touchesBorder = false;
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                int minimum = Math.Min(pixel.r, Math.Min(pixel.g, pixel.b));
                int maximum = Math.Max(pixel.r, Math.Max(pixel.g, pixel.b));
                bool occupied = minimum >= 96 && maximum - minimum <= 48;
                silhouette[index] = occupied;
                if (!occupied)
                {
                    continue;
                }

                silhouettePixels++;
                int x = index % width;
                int y = index / width;
                touchesBorder |= x == 0 || y == 0 || x == width - 1 || y == height - 1;
            }

            if (silhouettePixels == 0)
            {
                failure = "The isolated capture produced no white candidate silhouette pixels.";
                return null;
            }
            if (touchesBorder)
            {
                failure = "The white candidate silhouette clips the fixed game-camera capture border; deviation was not measured.";
                return null;
            }
            return silhouette;
        }

        private static void ApplySilhouetteComparison(
            Job job,
            CaseResult result,
            bool[] silhouette)
        {
            if (silhouette == null)
            {
                return;
            }

            string key = TargetKey(result.Target);
            if (result.Policy == TreeBarkMeshEfficiencyPolicy.Current)
            {
                job.BaselineSilhouettes[key] = silhouette;
                result.SilhouetteMeasured = true;
                result.SilhouetteDeviation = 0f;
            }
            else if (job.BaselineSilhouettes.TryGetValue(
                         key,
                         out bool[] baseline))
            {
                result.SilhouetteMeasured = true;
                result.SilhouetteDeviation =
                    CalculateSilhouetteDeviation(baseline, silhouette);
            }
        }

        private static void ReleasePendingCapture(PendingCapture pending)
        {
            if (pending == null || pending.Released)
            {
                return;
            }

            pending.Released = true;
            if (pending.RenderTexture != null)
            {
                pending.RenderTexture.Release();
                UnityEngine.Object.DestroyImmediate(pending.RenderTexture);
                pending.RenderTexture = null;
            }
        }

        private static Bounds TransformBounds(
            Bounds localBounds,
            Matrix4x4 localToWorld)
        {
            Vector3 center = localToWorld.MultiplyPoint3x4(localBounds.center);
            Vector3 extents = localBounds.extents;
            Vector3 axisX = localToWorld.MultiplyVector(
                new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = localToWorld.MultiplyVector(
                new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = localToWorld.MultiplyVector(
                new Vector3(0f, 0f, extents.z));
            extents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
            return new Bounds(center, extents * 2f);
        }

        private static void PositionFixedProjectionCamera(
            Camera camera,
            Bounds bounds,
            Job job)
        {
            Vector3 forward = camera.transform.forward;
            float boundsRadius = Mathf.Max(0.01f, bounds.extents.magnitude);
            float distance = job.CaptureOrthographic
                ? Mathf.Max(10f, boundsRadius * 4f)
                : job.CaptureReferenceDistance;
            distance = Mathf.Clamp(
                distance,
                job.CaptureNearClipPlane + boundsRadius + 0.01f,
                Mathf.Max(
                    job.CaptureNearClipPlane + boundsRadius + 0.02f,
                    job.CaptureFarClipPlane - boundsRadius - 0.01f));
            camera.transform.position = bounds.center - forward * distance;
        }

        private static float CalculateSilhouetteDeviation(
            bool[] baseline,
            bool[] candidate)
        {
            if (baseline == null || candidate == null ||
                baseline.Length != candidate.Length)
            {
                return float.NaN;
            }

            int union = 0;
            int different = 0;
            for (int index = 0; index < baseline.Length; index++)
            {
                if (baseline[index] || candidate[index])
                {
                    union++;
                    if (baseline[index] != candidate[index])
                    {
                        different++;
                    }
                }
            }
            return union > 0 ? different / (float)union : 0f;
        }

        private static GeometryTotals AggregatePolicyGeometry(
            Job job,
            TreeBarkMeshEfficiencyPolicy policy)
        {
            GeometryTotals aggregate = default;
            for (int index = 0; index < job.Results.Count; index++)
            {
                CaseResult result = job.Results[index];
                if (result.Policy != policy || !result.Passed ||
                    result.Bark == null)
                {
                    continue;
                }

                AggregateGeometry(result.Bark, out GeometryTotals item);
                aggregate.TrunkVertices += item.TrunkVertices;
                aggregate.TrunkTriangles += item.TrunkTriangles;
                aggregate.RootVertices += item.RootVertices;
                aggregate.RootTriangles += item.RootTriangles;
                aggregate.RootLobeVertices += item.RootLobeVertices;
                aggregate.RootLobeTriangles += item.RootLobeTriangles;
                aggregate.PersistenceVertices += item.PersistenceVertices;
                aggregate.PersistenceTriangles += item.PersistenceTriangles;
                aggregate.OrdinaryTrunkVertices +=
                    item.OrdinaryTrunkVertices;
                aggregate.OrdinaryTrunkTriangles +=
                    item.OrdinaryTrunkTriangles;
                aggregate.PrimaryVertices += item.PrimaryVertices;
                aggregate.PrimaryTriangles += item.PrimaryTriangles;
                aggregate.SecondaryVertices += item.SecondaryVertices;
                aggregate.SecondaryTriangles += item.SecondaryTriangles;
                aggregate.TertiaryVertices += item.TertiaryVertices;
                aggregate.TertiaryTriangles += item.TertiaryTriangles;
                aggregate.CapVertices += item.CapVertices;
                aggregate.CapTriangles += item.CapTriangles;
                aggregate.SeamVertices += item.SeamVertices;
                aggregate.SourceSamples += item.SourceSamples;
                aggregate.RenderRings += item.RenderRings;
                aggregate.InsertedRings += item.InsertedRings;
                aggregate.RootRefinementInsertedRings +=
                    item.RootRefinementInsertedRings;
                aggregate.TwistRefinementInsertedRings +=
                    item.TwistRefinementInsertedRings;
                aggregate.AdaptiveShapeRefinementInsertedRings +=
                    item.AdaptiveShapeRefinementInsertedRings;
                aggregate.RemovedRings += item.RemovedRings;
                aggregate.EfficiencyPolicyRemovedRings +=
                    item.EfficiencyPolicyRemovedRings;
                aggregate.TopologyRepairRemovedRings +=
                    item.TopologyRepairRemovedRings;
                aggregate.MaximumSegmentLength = Mathf.Max(
                    aggregate.MaximumSegmentLength,
                    item.MaximumSegmentLength);
                aggregate.MaximumTurn = Mathf.Max(
                    aggregate.MaximumTurn,
                    item.MaximumTurn);
            }
            return aggregate;
        }

        private static long SumPolicy(
            Job job,
            TreeBarkMeshEfficiencyPolicy policy,
            Func<CaseResult, int> selector)
        {
            long total = 0;
            for (int index = 0; index < job.Results.Count; index++)
            {
                CaseResult item = job.Results[index];
                if (item.Policy == policy && item.Passed &&
                    item.Bark != null)
                {
                    total += selector(item);
                }
            }
            return total;
        }

        private static bool PolicyPassedAll(
            Job job,
            TreeBarkMeshEfficiencyPolicy policy)
        {
            int count = 0;
            for (int index = 0; index < job.Results.Count; index++)
            {
                CaseResult item = job.Results[index];
                if (item.Policy != policy)
                {
                    continue;
                }
                count++;
                if (!item.Passed)
                {
                    return false;
                }
            }
            return count == ExpectedTreeCount;
        }

        private static float MeanSilhouetteDeviation(
            Job job,
            TreeBarkMeshEfficiencyPolicy policy)
        {
            float total = 0f;
            int count = 0;
            for (int index = 0; index < job.Results.Count; index++)
            {
                CaseResult item = job.Results[index];
                if (item.Policy == policy && item.SilhouetteMeasured)
                {
                    total += item.SilhouetteDeviation;
                    count++;
                }
            }
            return count > 0 ? total / count : float.NaN;
        }

        private static string TargetKey(Target target)
        {
            return target.Family + ":" + target.Variant;
        }

        private static string FormatPercentReduction(
            long baseline,
            long candidate)
        {
            if (baseline <= 0)
            {
                return "n/a";
            }
            return (1.0 - candidate / (double)baseline).ToString(
                "P2",
                CultureInfo.InvariantCulture);
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024L)
            {
                return (bytes / (1024.0 * 1024.0)).ToString(
                    "F2",
                    CultureInfo.InvariantCulture) + " MB";
            }
            if (bytes >= 1024L)
            {
                return (bytes / 1024.0).ToString(
                    "F2",
                    CultureInfo.InvariantCulture) + " KB";
            }
            return bytes + " B";
        }

        private static string FormatDuration(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) ||
                seconds < 0.0)
            {
                return "unknown";
            }
            TimeSpan duration = TimeSpan.FromSeconds(seconds);
            return duration.TotalHours >= 1.0
                ? duration.ToString(@"hh\:mm\:ss")
                : duration.ToString(@"mm\:ss");
        }

        private static string FirstFailureLine(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            int newline = value.IndexOfAny(new[] { '\r', '\n' });
            return newline >= 0 ? value.Substring(0, newline) : value;
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }
            return value.Replace(' ', '_');
        }

        private static string I(int? value)
        {
            return (value ?? 0).ToString(CultureInfo.InvariantCulture);
        }

        private static string L(long? value)
        {
            return (value ?? 0L).ToString(CultureInfo.InvariantCulture);
        }

        private static string D(double? value)
        {
            return (value ?? 0.0).ToString(
                "F6",
                CultureInfo.InvariantCulture);
        }

        private static string F(double value)
        {
            return value.ToString("F6", CultureInfo.InvariantCulture);
        }

        private static void WriteCsvRow(
            StreamWriter writer,
            IReadOnlyList<string> values)
        {
            for (int index = 0; index < values.Count; index++)
            {
                if (index > 0)
                {
                    writer.Write(',');
                }
                writer.Write(CsvEscape(values[index]));
            }
            writer.WriteLine();
        }

        private static string CsvEscape(string value)
        {
            value ??= string.Empty;
            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return value;
            }
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }

    internal sealed class TreeGeometryEfficiencyAuditWindow : EditorWindow
    {
        private static TreeGeometryEfficiencyAuditWindow openWindow;

        internal static void ShowWindow()
        {
            openWindow = GetWindow<TreeGeometryEfficiencyAuditWindow>(
                false,
                "Tree Geometry Audit",
                true);
            openWindow.minSize = new Vector2(500f, 190f);
            openWindow.Show();
        }

        internal static void RepaintOpenWindow()
        {
            if (openWindow != null)
            {
                openWindow.Repaint();
            }
        }

        private void OnEnable()
        {
            openWindow = this;
        }

        private void OnDisable()
        {
            if (openWindow == this)
            {
                openWindow = null;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "TREE-GEOMETRY.3B Efficiency Audit",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "The audit advances one tree/policy build per Editor update. It never commits candidate meshes or modifies the scene. Closing this window does not stop it.",
                MessageType.None);
            Rect progressRect = GUILayoutUtility.GetRect(
                10f,
                20f,
                GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(
                progressRect,
                TreeGeometryEfficiencyAudit.CurrentProgress,
                TreeGeometryEfficiencyAudit.ProgressLabel);
            EditorGUILayout.LabelField(
                "Current",
                TreeGeometryEfficiencyAudit.CurrentDetail);
            EditorGUILayout.LabelField(
                "Timing",
                TreeGeometryEfficiencyAudit.CurrentEta);

            if (TreeGeometryEfficiencyAudit.IsRunning)
            {
                if (GUILayout.Button("Cancel After Current Bounded Case"))
                {
                    TreeGeometryEfficiencyAudit.RequestCancel();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Audit is not running.");
            }
        }
    }
}
