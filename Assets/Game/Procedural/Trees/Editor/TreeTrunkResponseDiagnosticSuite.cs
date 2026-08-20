using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeTrunkResponseDiagnosticSuite
    {
        private const string OutputDirectory =
            "Library/PS3D/Trees/TrunkResponseDiagnostics";
        private const string ReportFileName =
            "TreeTrunkResponseDiagnosticReport.txt";
        private const string SummaryCsvFileName =
            "TreeTrunkResponseDiagnosticCases.csv";
        private const string SampleCsvFileName =
            "TreeTrunkResponseDiagnosticSamples.csv";
        private const float DefaultBendFrequency = 1.5f;
        private const float PositionContractTolerance = 0.001f;
        private const float TangentContractToleranceDegrees = 0.1f;

        private sealed class CaseDefinition
        {
            internal string Group;
            internal string Name;
            internal float Lean;
            internal float Bend;
            internal float BendFrequency = DefaultBendFrequency;
            internal float Persistence = 0.5f;
            internal float? RootHeight;
            internal float? RootReach;
            internal float? RootThickness;
            internal float Twist;
            internal float SpiralRadius;
            internal float SpiralTurns;
        }

        private sealed class CaseResult
        {
            internal CaseDefinition Definition;
            internal bool Passed;
            internal bool TopologyPassed;
            internal string Failure;
            internal int VertexCount;
            internal int TriangleCount;
            internal float MaximumTangentMismatch;
            internal float MaximumTangentMismatchT;
            internal float MeanLowerQuarterMismatch;
            internal float MeanLowerHalfMismatch;
            internal float StableBelowFiveT;
            internal float StableBelowOneT;
            internal float TipHorizontalDisplacement;
            internal float ExpectedLeanTipDisplacement;
            internal float LeanTipDisplacementError;
            internal float FinalStructuralY;
            internal float AuthoredHeightError;
            internal float? BendOnlyEndpointError;
            internal float? LeanBendEndpointError;
            internal float EarliestTransitionTangentMismatch;
            internal string InvariantFailure;
            internal float MaximumHorizontalDisplacement;
            internal float MaximumHorizontalDisplacementT;
            internal float TreeHeight;
            internal int RootCount;
            internal float RootHeight;
            internal float RootReach;
            internal float RootThickness;
            internal float MaximumStructuralTurn;
            internal float MaximumSurfaceTurn;
            internal float RootGroundPlateauEnd;
            internal float RootCollapseEnd;
            internal float EarliestRootTransition;
            internal float EffectiveRootTransition;
            internal float EffectiveButtressBodyEnd;
        }

        private sealed class Job
        {
            internal ProceduralTreeInstance Source;
            internal TreeResolvedControls Baseline;
            internal List<CaseDefinition> Cases;
            internal List<CaseResult> Results = new List<CaseResult>();
            internal int Index;
            internal int Passed;
            internal int Failed;
            internal bool CancelRequested;
            internal DateTime StartedUtc;
            internal string ReportPath;
            internal string SummaryCsvPath;
            internal string SampleCsvPath;
            internal StreamWriter SummaryWriter;
            internal StreamWriter SampleWriter;
        }

        private static readonly float[] FixedSampleDistances =
        {
            0f,
            0.025f,
            0.05f,
            0.10f,
            0.20f,
            0.35f,
            0.50f,
            0.75f,
            1f
        };

        private static Job activeJob;
        private static string lastReportPath = string.Empty;
        private static string lastSummaryCsvPath = string.Empty;
        private static string lastSampleCsvPath = string.Empty;
        private static string currentDetail = "Not running";
        private static string currentEta = string.Empty;
        private static float currentProgress;

        internal static bool IsRunning => activeJob != null;
        internal static string CurrentDetail => currentDetail;
        internal static string CurrentEta => currentEta;
        internal static float CurrentProgress => currentProgress;
        internal static string LastReportPath => lastReportPath;
        internal static string LastSummaryCsvPath => lastSummaryCsvPath;
        internal static string LastSampleCsvPath => lastSampleCsvPath;

        internal static string ProgressLabel => activeJob == null
            ? "Not running"
            : activeJob.Index + " / " + activeJob.Cases.Count;

        internal static bool Start(ProceduralTreeInstance selected)
        {
            if (activeJob != null || selected == null ||
                !selected.HasExactControls || selected.ExactControls == null)
            {
                return false;
            }

            Directory.CreateDirectory(OutputDirectory);
            string reportPath = Path.Combine(OutputDirectory, ReportFileName);
            string summaryPath = Path.Combine(
                OutputDirectory,
                SummaryCsvFileName);
            string samplePath = Path.Combine(
                OutputDirectory,
                SampleCsvFileName);
            var summaryWriter = new StreamWriter(
                summaryPath,
                false,
                Encoding.UTF8);
            var sampleWriter = new StreamWriter(
                samplePath,
                false,
                Encoding.UTF8);
            summaryWriter.WriteLine(
                "Group,Case,Status,Lean,Bend,BendFrequency,Persistence," +
                "TreeHeight,RootCount,RootHeight,RootReach,RootThickness," +
                "Twist,SpiralRadius," +
                "SpiralTurns,Vertices,Triangles,MaxTangentMismatchDegrees," +
                "MaxMismatchT,MeanLower25MismatchDegrees," +
                "MeanLower50MismatchDegrees,StableBelow5T,StableBelow1T," +
                "TipHorizontalDisplacement,ExpectedLeanTipDisplacement," +
                "LeanTipDisplacementError,FinalStructuralY,AuthoredHeightError," +
                "BendOnlyEndpointError,LeanBendEndpointError," +
                "EarliestTransitionTangentMismatchDegrees,InvariantStatus," +
                "MaxHorizontalDisplacement," +
                "MaxHorizontalDisplacementT,MaxStructuralTurnDegrees," +
                "MaxSurfaceTurnDegrees,GroundPlateauT,RootCollapseT," +
                "EarliestRootTransitionT,EffectiveRootTransitionT," +
                "EffectiveButtressBodyEndT,Failure");
            sampleWriter.WriteLine(
                "Group,Case,T,Landmark,StructuralX,StructuralY,StructuralZ," +
                "StructuralTangentX,StructuralTangentY,StructuralTangentZ," +
                "SurfaceTangentX,SurfaceTangentY,SurfaceTangentZ," +
                "TangentMismatchDegrees,BaseAzimuthMismatchDegrees," +
                "RolledNormalMismatchDegrees,TangentSafetyEnvelope," +
                "RootFrameEnvelope,BodyEnvelope,FootShapeEnvelope," +
                "FootAnchorEnvelope," +
                "AuthoredRollDegrees,HorizontalDisplacement," +
                "StructuralTurnDegrees,SurfaceTurnDegrees");
            summaryWriter.Flush();
            sampleWriter.Flush();

            activeJob = new Job
            {
                Source = selected,
                Baseline = CloneControls(selected.ExactControls),
                Cases = BuildCases(),
                StartedUtc = DateTime.UtcNow,
                ReportPath = reportPath,
                SummaryCsvPath = summaryPath,
                SampleCsvPath = samplePath,
                SummaryWriter = summaryWriter,
                SampleWriter = sampleWriter
            };
            lastReportPath = reportPath;
            lastSummaryCsvPath = summaryPath;
            lastSampleCsvPath = samplePath;
            currentDetail = "Preparing first Lean/Bend interaction case";
            currentEta = "ETA calculating";
            currentProgress = 0f;
            WriteReport(activeJob, "RUNNING", null);
            EditorApplication.update += Tick;
            AssemblyReloadEvents.beforeAssemblyReload += AbortForReload;
            EditorApplication.quitting += AbortForQuit;
            Debug.Log(
                "[TREE-TRUNK.2] Lean/Bend contract verification started. " +
                "Cases=" + activeJob.Cases.Count +
                ". Output: " + reportPath,
                selected);
            return true;
        }

        internal static void RequestCancel()
        {
            if (activeJob != null)
            {
                activeJob.CancelRequested = true;
            }
        }

        internal static void OpenOutputFolder()
        {
            Directory.CreateDirectory(OutputDirectory);
            EditorUtility.RevealInFinder(Path.GetFullPath(OutputDirectory));
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

        private static void AbortForReload()
        {
            if (activeJob != null)
            {
                Finish(
                    activeJob,
                    "CANCELLED",
                    "Assembly reload interrupted the suite after partial outputs were preserved.");
            }
        }

        private static void AbortForQuit()
        {
            if (activeJob != null)
            {
                Finish(
                    activeJob,
                    "CANCELLED",
                    "Editor shutdown interrupted the suite after partial outputs were preserved.");
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
                if (job.CancelRequested)
                {
                    Finish(job, "CANCELLED", null);
                    return;
                }

                if (job.Index >= job.Cases.Count)
                {
                    Finish(job, "COMPLETE", null);
                    return;
                }

                TimeSpan elapsed = DateTime.UtcNow - job.StartedUtc;
                double secondsPerCase = job.Index > 0
                    ? elapsed.TotalSeconds / job.Index
                    : 0.0;
                double remainingSeconds = secondsPerCase *
                    (job.Cases.Count - job.Index);
                CaseDefinition definition = job.Cases[job.Index];
                currentProgress = job.Index /
                    (float)Mathf.Max(1, job.Cases.Count);
                currentDetail = definition.Group + " — " + definition.Name;
                currentEta = "Elapsed " + FormatDuration(elapsed.TotalSeconds) +
                    " | ETA " + FormatDuration(remainingSeconds);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

                CaseResult result = RunCase(job, definition);
                job.Results.Add(result);
                if (result.Passed)
                {
                    job.Passed++;
                }
                else
                {
                    job.Failed++;
                }
                job.Index++;
                currentProgress = job.Index /
                    (float)Mathf.Max(1, job.Cases.Count);
                WriteSummaryCsv(job.SummaryWriter, result);
                job.SummaryWriter.Flush();
                job.SampleWriter.Flush();
                WriteReport(job, "RUNNING", null);
            }
            catch (Exception exception)
            {
                Finish(job, "FAILED", exception.ToString());
            }
        }

        private static CaseResult RunCase(
            Job job,
            CaseDefinition definition)
        {
            TreeResolvedControls controls = CloneControls(job.Baseline);
            PrepareIsolatedTrunkControls(controls);
            SetFloat(controls, "leanAmount", definition.Lean);
            SetFloat(controls, "bendAmount", definition.Bend);
            SetFloat(controls, "bendFrequency", definition.BendFrequency);
            SetFloat(controls, "buttressTransition", definition.Persistence);
            SetFloat(controls, "axialTwist", definition.Twist);
            SetFloat(controls, "pathSpiralRadius", definition.SpiralRadius);
            SetFloat(
                controls,
                "signedPathSpiralTurns",
                definition.SpiralRadius > 0f ? definition.SpiralTurns : 0f);
            if (definition.RootHeight.HasValue)
            {
                SetFloat(controls, "rootHeight", definition.RootHeight.Value);
            }
            if (definition.RootReach.HasValue)
            {
                SetFloat(controls, "rootReach", definition.RootReach.Value);
            }
            if (definition.RootThickness.HasValue)
            {
                SetFloat(
                    controls,
                    "rootThickness",
                    definition.RootThickness.Value);
            }
            controls.ValidateAndClamp();

            var result = new CaseResult
            {
                Definition = definition,
                StableBelowFiveT = -1f,
                StableBelowOneT = -1f,
                TreeHeight = controls.Height,
                RootCount = controls.RootCount,
                RootHeight = controls.RootHeight,
                RootReach = controls.RootReach,
                RootThickness = controls.RootThickness
            };
            TreeGenerationResult generation =
                TreeGenerator.GenerateExactForValidation(
                    controls,
                    job.Source.MasterSeed,
                    string.IsNullOrEmpty(
                        job.Source.ExactControlsSourceRecipeIdentity)
                            ? "tree-trunk-response-diagnostic"
                            : job.Source.ExactControlsSourceRecipeIdentity,
                    job.Source.Family);
            if (generation == null || !generation.Passed ||
                generation.Definition == null ||
                !generation.Definition.IsValid)
            {
                result.Failure = generation != null
                    ? FirstFailureLine(generation.Report)
                    : "Tree generation returned null.";
                return result;
            }

            TreeDefinition tree = generation.Definition;
            if (tree.TrunkBranchIndex < 0 ||
                tree.TrunkBranchIndex >= tree.Branches.Count)
            {
                result.Failure = "Generated definition has no valid trunk branch.";
                return result;
            }
            TreeBranchDefinition trunk = tree.Branches[tree.TrunkBranchIndex];
            if (trunk.Samples == null || trunk.Samples.Count < 2)
            {
                result.Failure = "Generated trunk has fewer than two curve samples.";
                return result;
            }

            var mesh = new Mesh
            {
                name = "TREE-TRUNK.2 Diagnostic Candidate"
            };
            TreeBarkMeshBuildResult bark;
            try
            {
                bark = TreeBarkMeshGenerator.Build(
                    tree,
                    TreeBarkMeshSettings.CreateRecipeOnlyDefaults(),
                    mesh);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
            if (bark == null || !bark.Passed ||
                bark.TopologyAudit == null || !bark.TopologyAudit.Passed)
            {
                string barkFailure = bark != null &&
                    !string.IsNullOrEmpty(bark.Failure)
                        ? bark.Failure
                        : bark?.TopologyAudit?.Report;
                result.Failure = bark != null
                    ? string.IsNullOrWhiteSpace(barkFailure)
                        ? "Bark build failed without a diagnostic report."
                        : barkFailure.Trim()
                    : "Bark build returned null.";
                return result;
            }

            result.TopologyPassed = true;
            result.VertexCount = bark.VertexCount;
            result.TriangleCount = bark.TriangleCount;
            MeasureCase(
                job.SampleWriter,
                definition,
                tree,
                trunk,
                result);
            result.InvariantFailure = ResolveInvariantFailure(result);
            if (!string.IsNullOrEmpty(result.InvariantFailure))
            {
                result.Failure = result.InvariantFailure;
                return result;
            }
            result.Passed = true;
            return result;
        }

        private static void MeasureCase(
            StreamWriter sampleWriter,
            CaseDefinition definition,
            TreeDefinition tree,
            TreeBranchDefinition trunk,
            CaseResult result)
        {
            IReadOnlyList<TreeCurveSample> samples = trunk.Samples;
            TreeResolvedParameters parameters = tree.ResolvedParameters;
            var sampleDiagnostics = new List<TreeBarkTrunkFrameDiagnostic>(
                samples.Count);
            float mismatchSum25 = 0f;
            int mismatchCount25 = 0;
            float mismatchSum50 = 0f;
            int mismatchCount50 = 0;
            result.MaximumTangentMismatch = 0f;
            result.MaximumTangentMismatchT = 0f;
            result.MaximumHorizontalDisplacement = 0f;
            result.MaximumHorizontalDisplacementT = 0f;
            result.MaximumStructuralTurn = 0f;
            result.MaximumSurfaceTurn = 0f;
            Vector3 basePosition = samples[0].Position;

            for (int index = 0; index < samples.Count; index++)
            {
                TreeBarkTrunkFrameDiagnostic diagnostic =
                    TreeBarkMeshGenerator.EvaluateTrunkFrameForDiagnostics(
                        parameters,
                        samples[index]);
                sampleDiagnostics.Add(diagnostic);
                float mismatch = Vector3.Angle(
                    diagnostic.StructuralTangent,
                    diagnostic.BaseSurfaceTangent);
                if (mismatch > result.MaximumTangentMismatch)
                {
                    result.MaximumTangentMismatch = mismatch;
                    result.MaximumTangentMismatchT =
                        samples[index].NormalizedDistance;
                }
                Vector3 structuralDelta = samples[index].Position - basePosition;
                float horizontalDisplacement = new Vector2(
                    structuralDelta.x,
                    structuralDelta.z).magnitude;
                if (horizontalDisplacement > result.MaximumHorizontalDisplacement)
                {
                    result.MaximumHorizontalDisplacement = horizontalDisplacement;
                    result.MaximumHorizontalDisplacementT =
                        samples[index].NormalizedDistance;
                }
                if (samples[index].NormalizedDistance <= 0.25f)
                {
                    mismatchSum25 += mismatch;
                    mismatchCount25++;
                }
                if (samples[index].NormalizedDistance <= 0.50f)
                {
                    mismatchSum50 += mismatch;
                    mismatchCount50++;
                }
                if (index > 0)
                {
                    result.MaximumStructuralTurn = Mathf.Max(
                        result.MaximumStructuralTurn,
                        Vector3.Angle(
                            samples[index - 1].Tangent,
                            samples[index].Tangent));
                    result.MaximumSurfaceTurn = Mathf.Max(
                        result.MaximumSurfaceTurn,
                        Vector3.Angle(
                            sampleDiagnostics[index - 1].BaseSurfaceTangent,
                            diagnostic.BaseSurfaceTangent));
                }
            }

            result.MeanLowerQuarterMismatch = mismatchCount25 > 0
                ? mismatchSum25 / mismatchCount25
                : 0f;
            result.MeanLowerHalfMismatch = mismatchCount50 > 0
                ? mismatchSum50 / mismatchCount50
                : 0f;
            result.StableBelowFiveT = ResolveStableBelowThreshold(
                samples,
                sampleDiagnostics,
                5f);
            result.StableBelowOneT = ResolveStableBelowThreshold(
                samples,
                sampleDiagnostics,
                1f);
            Vector3 tipDelta = samples[samples.Count - 1].Position - basePosition;
            result.TipHorizontalDisplacement = new Vector2(
                tipDelta.x,
                tipDelta.z).magnitude;
            result.ExpectedLeanTipDisplacement =
                definition.Lean * result.TreeHeight;
            result.LeanTipDisplacementError =
                result.TipHorizontalDisplacement -
                result.ExpectedLeanTipDisplacement;
            result.FinalStructuralY = samples[samples.Count - 1].Position.y;
            result.AuthoredHeightError =
                result.FinalStructuralY - result.TreeHeight;
            bool nonSpiral = definition.SpiralRadius <= 0.000001f;
            if (nonSpiral && definition.Lean <= 0.000001f &&
                definition.Bend > 0.000001f)
            {
                result.BendOnlyEndpointError =
                    result.TipHorizontalDisplacement;
            }
            if (nonSpiral && definition.Lean > 0.000001f &&
                definition.Bend > 0.000001f)
            {
                result.LeanBendEndpointError = Mathf.Abs(
                    result.TipHorizontalDisplacement -
                    result.ExpectedLeanTipDisplacement);
            }

            TreeBarkTrunkFrameDiagnostic rootDiagnostic =
                sampleDiagnostics[0];
            result.RootGroundPlateauEnd = rootDiagnostic.GroundPlateauEnd;
            result.RootCollapseEnd = rootDiagnostic.RootCollapseEnd;
            result.EarliestRootTransition =
                rootDiagnostic.EarliestRootTransition;
            result.EffectiveRootTransition =
                rootDiagnostic.EffectiveRootTransition;
            result.EffectiveButtressBodyEnd =
                rootDiagnostic.EffectiveButtressBodyEnd;
            TreeCurveSample earliestSample = EvaluateSample(
                trunk,
                result.EarliestRootTransition);
            TreeBarkTrunkFrameDiagnostic earliestDiagnostic =
                TreeBarkMeshGenerator.EvaluateTrunkFrameForDiagnostics(
                    parameters,
                    earliestSample);
            result.EarliestTransitionTangentMismatch = Vector3.Angle(
                earliestDiagnostic.StructuralTangent,
                earliestDiagnostic.BaseSurfaceTangent);

            var distances = new List<float>(FixedSampleDistances.Length + 5);
            for (int index = 0; index < FixedSampleDistances.Length; index++)
            {
                AddUniqueDistance(distances, FixedSampleDistances[index]);
            }
            AddUniqueDistance(distances, result.RootGroundPlateauEnd);
            AddUniqueDistance(distances, result.RootCollapseEnd);
            AddUniqueDistance(distances, result.EarliestRootTransition);
            AddUniqueDistance(distances, result.EffectiveRootTransition);
            AddUniqueDistance(distances, result.EffectiveButtressBodyEnd);
            distances.Sort();

            for (int index = 0; index < distances.Count; index++)
            {
                float t = distances[index];
                TreeCurveSample source = EvaluateSample(trunk, t);
                TreeBarkTrunkFrameDiagnostic diagnostic =
                    TreeBarkMeshGenerator.EvaluateTrunkFrameForDiagnostics(
                        parameters,
                        source);
                float structuralTurn = EvaluateLocalTurn(trunk, t, false);
                float surfaceTurn = EvaluateLocalTurn(trunk, t, true, parameters);
                Vector3 displacement = source.Position - basePosition;
                Vector3 projectedStructuralNormal = Vector3.ProjectOnPlane(
                    source.Normal,
                    diagnostic.BaseSurfaceTangent);
                if (projectedStructuralNormal.sqrMagnitude <= 0.000001f)
                {
                    projectedStructuralNormal = diagnostic.BaseSurfaceNormal;
                }
                projectedStructuralNormal.Normalize();
                WriteSampleCsv(
                    sampleWriter,
                    definition,
                    diagnostic,
                    ResolveLandmark(result, t),
                    Vector3.Angle(
                        diagnostic.StructuralTangent,
                        diagnostic.BaseSurfaceTangent),
                    Vector3.Angle(
                        projectedStructuralNormal,
                        diagnostic.BaseSurfaceNormal),
                    Vector3.Angle(
                        projectedStructuralNormal,
                        diagnostic.RolledSurfaceNormal),
                    new Vector2(displacement.x, displacement.z).magnitude,
                    structuralTurn,
                    surfaceTurn);
            }
        }

        private static string ResolveInvariantFailure(CaseResult result)
        {
            var failures = new List<string>();
            if (Mathf.Abs(result.AuthoredHeightError) >
                PositionContractTolerance)
            {
                failures.Add(
                    "authored height error=" + F(result.AuthoredHeightError));
            }
            if (result.BendOnlyEndpointError.HasValue &&
                result.BendOnlyEndpointError.Value >
                PositionContractTolerance)
            {
                failures.Add(
                    "Bend-only endpoint error=" +
                    F(result.BendOnlyEndpointError));
            }
            if (result.LeanBendEndpointError.HasValue &&
                result.LeanBendEndpointError.Value >
                PositionContractTolerance)
            {
                failures.Add(
                    "Lean+Bend endpoint error=" +
                    F(result.LeanBendEndpointError));
            }
            if (result.EarliestTransitionTangentMismatch >
                TangentContractToleranceDegrees)
            {
                failures.Add(
                    "earliest-transition tangent mismatch=" +
                    F(result.EarliestTransitionTangentMismatch) + " deg");
            }
            return failures.Count > 0
                ? "TREE-TRUNK.2 invariant failure: " +
                    string.Join("; ", failures)
                : string.Empty;
        }

        private static float ResolveStableBelowThreshold(
            IReadOnlyList<TreeCurveSample> samples,
            IReadOnlyList<TreeBarkTrunkFrameDiagnostic> diagnostics,
            float threshold)
        {
            int lastAbove = -1;
            for (int index = 0; index < samples.Count; index++)
            {
                float mismatch = Vector3.Angle(
                    diagnostics[index].StructuralTangent,
                    diagnostics[index].BaseSurfaceTangent);
                if (mismatch > threshold)
                {
                    lastAbove = index;
                }
            }
            if (lastAbove < 0)
            {
                return 0f;
            }
            if (lastAbove + 1 >= samples.Count)
            {
                return -1f;
            }
            return samples[lastAbove + 1].NormalizedDistance;
        }

        private static TreeCurveSample EvaluateSample(
            TreeBranchDefinition trunk,
            float normalizedDistance)
        {
            IReadOnlyList<TreeCurveSample> samples = trunk.Samples;
            float target = Mathf.Clamp01(normalizedDistance);
            if (target <= samples[0].NormalizedDistance)
            {
                return samples[0];
            }
            int last = samples.Count - 1;
            if (target >= samples[last].NormalizedDistance)
            {
                return samples[last];
            }

            int lower = 0;
            int upper = last;
            while (upper - lower > 1)
            {
                int middle = (lower + upper) >> 1;
                if (samples[middle].NormalizedDistance <= target)
                {
                    lower = middle;
                }
                else
                {
                    upper = middle;
                }
            }

            TreeCurveSample a = samples[lower];
            TreeCurveSample b = samples[upper];
            float denominator = Mathf.Max(
                0.000001f,
                b.NormalizedDistance - a.NormalizedDistance);
            float blend = Mathf.Clamp01(
                (target - a.NormalizedDistance) / denominator);
            Vector3 tangent = Vector3.Slerp(
                a.Tangent,
                b.Tangent,
                blend).normalized;
            Vector3 normal = Vector3.Slerp(a.Normal, b.Normal, blend);
            normal = Vector3.ProjectOnPlane(normal, tangent);
            if (normal.sqrMagnitude <= 0.000001f)
            {
                normal = Vector3.ProjectOnPlane(Vector3.right, tangent);
            }
            if (normal.sqrMagnitude <= 0.000001f)
            {
                normal = Vector3.ProjectOnPlane(Vector3.forward, tangent);
            }
            normal.Normalize();
            Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
            normal = Vector3.Cross(binormal, tangent).normalized;
            return new TreeCurveSample(
                Vector3.Lerp(a.Position, b.Position, blend),
                tangent,
                normal,
                binormal,
                Mathf.Lerp(a.Radius, b.Radius, blend),
                target);
        }

        private static float EvaluateLocalTurn(
            TreeBranchDefinition trunk,
            float normalizedDistance,
            bool surface,
            TreeResolvedParameters parameters = null)
        {
            float delta = 1f / Mathf.Max(2f, trunk.Samples.Count - 1f);
            TreeCurveSample a = EvaluateSample(
                trunk,
                Mathf.Clamp01(normalizedDistance - delta));
            TreeCurveSample b = EvaluateSample(
                trunk,
                Mathf.Clamp01(normalizedDistance + delta));
            if (!surface)
            {
                return Vector3.Angle(a.Tangent, b.Tangent);
            }
            Vector3 tangentA =
                TreeBarkMeshGenerator.EvaluateTrunkFrameForDiagnostics(
                    parameters,
                    a).BaseSurfaceTangent;
            Vector3 tangentB =
                TreeBarkMeshGenerator.EvaluateTrunkFrameForDiagnostics(
                    parameters,
                    b).BaseSurfaceTangent;
            return Vector3.Angle(tangentA, tangentB);
        }

        private static void AddUniqueDistance(List<float> values, float value)
        {
            float clamped = Mathf.Clamp01(value);
            for (int index = 0; index < values.Count; index++)
            {
                if (Mathf.Abs(values[index] - clamped) <= 0.00001f)
                {
                    return;
                }
            }
            values.Add(clamped);
        }

        private static string ResolveLandmark(CaseResult result, float t)
        {
            var labels = new List<string>();
            for (int index = 0; index < FixedSampleDistances.Length; index++)
            {
                if (Mathf.Abs(t - FixedSampleDistances[index]) <= 0.00001f)
                {
                    labels.Add("fixed");
                    break;
                }
            }
            AppendLandmark(
                labels,
                "ground-plateau-end",
                t,
                result.RootGroundPlateauEnd);
            AppendLandmark(
                labels,
                "root-collapse-end",
                t,
                result.RootCollapseEnd);
            AppendLandmark(
                labels,
                "earliest-root-transition",
                t,
                result.EarliestRootTransition);
            AppendLandmark(
                labels,
                "effective-root-transition",
                t,
                result.EffectiveRootTransition);
            AppendLandmark(
                labels,
                "buttress-body-end",
                t,
                result.EffectiveButtressBodyEnd);
            return string.Join("+", labels);
        }

        private static void AppendLandmark(
            List<string> labels,
            string label,
            float actual,
            float target)
        {
            if (Mathf.Abs(actual - target) <= 0.00001f)
            {
                labels.Add(label);
            }
        }

        private static void WriteSampleCsv(
            StreamWriter writer,
            CaseDefinition definition,
            TreeBarkTrunkFrameDiagnostic diagnostic,
            string landmark,
            float tangentMismatch,
            float baseAzimuthMismatch,
            float rolledNormalMismatch,
            float horizontalDisplacement,
            float structuralTurn,
            float surfaceTurn)
        {
            writer.Write(Csv(definition.Group)); writer.Write(',');
            writer.Write(Csv(definition.Name)); writer.Write(',');
            writer.Write(F(diagnostic.NormalizedDistance)); writer.Write(',');
            writer.Write(Csv(landmark)); writer.Write(',');
            writer.Write(F(diagnostic.StructuralPosition.x)); writer.Write(',');
            writer.Write(F(diagnostic.StructuralPosition.y)); writer.Write(',');
            writer.Write(F(diagnostic.StructuralPosition.z)); writer.Write(',');
            writer.Write(F(diagnostic.StructuralTangent.x)); writer.Write(',');
            writer.Write(F(diagnostic.StructuralTangent.y)); writer.Write(',');
            writer.Write(F(diagnostic.StructuralTangent.z)); writer.Write(',');
            writer.Write(F(diagnostic.BaseSurfaceTangent.x)); writer.Write(',');
            writer.Write(F(diagnostic.BaseSurfaceTangent.y)); writer.Write(',');
            writer.Write(F(diagnostic.BaseSurfaceTangent.z)); writer.Write(',');
            writer.Write(F(tangentMismatch)); writer.Write(',');
            writer.Write(F(baseAzimuthMismatch)); writer.Write(',');
            writer.Write(F(rolledNormalMismatch)); writer.Write(',');
            writer.Write(F(diagnostic.TangentSafetyEnvelope)); writer.Write(',');
            writer.Write(F(diagnostic.RootFrameEnvelope)); writer.Write(',');
            writer.Write(F(diagnostic.BodyEnvelope)); writer.Write(',');
            writer.Write(F(diagnostic.FootShapeEnvelope)); writer.Write(',');
            writer.Write(F(diagnostic.FootAnchorEnvelope)); writer.Write(',');
            writer.Write(F(diagnostic.AuthoredRollDegrees)); writer.Write(',');
            writer.Write(F(horizontalDisplacement)); writer.Write(',');
            writer.Write(F(structuralTurn)); writer.Write(',');
            writer.WriteLine(F(surfaceTurn));
        }

        private static void WriteSummaryCsv(
            StreamWriter writer,
            CaseResult result)
        {
            CaseDefinition d = result.Definition;
            writer.Write(Csv(d.Group)); writer.Write(',');
            writer.Write(Csv(d.Name)); writer.Write(',');
            writer.Write(result.Passed ? "PASS" : "FAIL"); writer.Write(',');
            writer.Write(F(d.Lean)); writer.Write(',');
            writer.Write(F(d.Bend)); writer.Write(',');
            writer.Write(F(d.BendFrequency)); writer.Write(',');
            writer.Write(F(d.Persistence)); writer.Write(',');
            writer.Write(F(result.TreeHeight)); writer.Write(',');
            writer.Write(result.RootCount); writer.Write(',');
            writer.Write(F(result.RootHeight)); writer.Write(',');
            writer.Write(F(result.RootReach)); writer.Write(',');
            writer.Write(F(result.RootThickness)); writer.Write(',');
            writer.Write(F(d.Twist)); writer.Write(',');
            writer.Write(F(d.SpiralRadius)); writer.Write(',');
            writer.Write(F(d.SpiralTurns)); writer.Write(',');
            writer.Write(result.VertexCount); writer.Write(',');
            writer.Write(result.TriangleCount); writer.Write(',');
            writer.Write(F(result.MaximumTangentMismatch)); writer.Write(',');
            writer.Write(F(result.MaximumTangentMismatchT)); writer.Write(',');
            writer.Write(F(result.MeanLowerQuarterMismatch)); writer.Write(',');
            writer.Write(F(result.MeanLowerHalfMismatch)); writer.Write(',');
            writer.Write(F(result.StableBelowFiveT)); writer.Write(',');
            writer.Write(F(result.StableBelowOneT)); writer.Write(',');
            writer.Write(F(result.TipHorizontalDisplacement)); writer.Write(',');
            writer.Write(F(result.ExpectedLeanTipDisplacement)); writer.Write(',');
            writer.Write(F(result.LeanTipDisplacementError)); writer.Write(',');
            writer.Write(F(result.FinalStructuralY)); writer.Write(',');
            writer.Write(F(result.AuthoredHeightError)); writer.Write(',');
            writer.Write(F(result.BendOnlyEndpointError)); writer.Write(',');
            writer.Write(F(result.LeanBendEndpointError)); writer.Write(',');
            writer.Write(F(result.EarliestTransitionTangentMismatch)); writer.Write(',');
            writer.Write(Csv(!result.TopologyPassed
                ? "NOT_RUN"
                : string.IsNullOrEmpty(result.InvariantFailure)
                    ? "PASS"
                    : result.InvariantFailure)); writer.Write(',');
            writer.Write(F(result.MaximumHorizontalDisplacement)); writer.Write(',');
            writer.Write(F(result.MaximumHorizontalDisplacementT)); writer.Write(',');
            writer.Write(F(result.MaximumStructuralTurn)); writer.Write(',');
            writer.Write(F(result.MaximumSurfaceTurn)); writer.Write(',');
            writer.Write(F(result.RootGroundPlateauEnd)); writer.Write(',');
            writer.Write(F(result.RootCollapseEnd)); writer.Write(',');
            writer.Write(F(result.EarliestRootTransition)); writer.Write(',');
            writer.Write(F(result.EffectiveRootTransition)); writer.Write(',');
            writer.Write(F(result.EffectiveButtressBodyEnd)); writer.Write(',');
            writer.WriteLine(Csv(result.Failure));
        }

        private static void WriteReport(
            Job job,
            string status,
            string failure)
        {
            var report = new StringBuilder(16384);
            report.AppendLine(
                "[TREE-TRUNK.2 Accepted Lean/Bend Contract Verification]");
            report.Append("Status: ").AppendLine(status);
            report.Append("Source: ").AppendLine(
                job.Source != null
                    ? job.Source.name
                    : "missing");
            report.Append("Cases completed/pass/fail: ")
                .Append(job.Index).Append(" / ")
                .Append(job.Passed).Append(" / ")
                .AppendLine(job.Failed.ToString());
            report.Append("Cases total: ").AppendLine(job.Cases.Count.ToString());
            report.AppendLine(
                "Contract: verifies the accepted Generator 8 / Bark 31 TREE-TRUNK.2 endpoint, authored-height, tangent-release, and production topology contracts while retaining detailed root-frame failure evidence. Lean Direction remains removed; Lean uses canonical tree-local +X.");
            report.AppendLine(
                "Topology: every completed case builds a temporary Production Current bark mesh and requires the existing topology audit to pass.");
            report.AppendLine();
            report.AppendLine("[Completed Cases]");
            for (int index = 0; index < job.Results.Count; index++)
            {
                CaseResult result = job.Results[index];
                report.Append(result.Passed ? "PASS | " : "FAIL | ")
                    .Append(result.Definition.Group).Append(" | ")
                    .Append(result.Definition.Name)
                    .Append(" | lean=").Append(F(result.Definition.Lean))
                    .Append(" bend=").Append(F(result.Definition.Bend))
                    .Append(" freq=").Append(F(result.Definition.BendFrequency))
                    .Append(" persistence=").Append(F(result.Definition.Persistence))
                    .Append(" | maxTangentMismatch=")
                    .Append(F(result.MaximumTangentMismatch))
                    .Append("@t=").Append(F(result.MaximumTangentMismatchT))
                    .Append(" meanLower25=")
                    .Append(F(result.MeanLowerQuarterMismatch))
                    .Append(" meanLower50=")
                    .Append(F(result.MeanLowerHalfMismatch))
                    .Append(" stable<5=").Append(F(result.StableBelowFiveT))
                    .Append(" stable<1=").Append(F(result.StableBelowOneT))
                    .Append(" | tipXZ=")
                    .Append(F(result.TipHorizontalDisplacement))
                    .Append(" expectedLeanTip=")
                    .Append(F(result.ExpectedLeanTipDisplacement))
                    .Append(" leanTipError=")
                    .Append(F(result.LeanTipDisplacementError))
                    .Append(" finalY=")
                    .Append(F(result.FinalStructuralY))
                    .Append(" heightError=")
                    .Append(F(result.AuthoredHeightError))
                    .Append(" bendOnlyEndpointError=")
                    .Append(F(result.BendOnlyEndpointError))
                    .Append(" leanBendEndpointError=")
                    .Append(F(result.LeanBendEndpointError))
                    .Append(" earliestTransitionTangentMismatch=")
                    .Append(F(result.EarliestTransitionTangentMismatch))
                    .Append(" maxXZ=")
                    .Append(F(result.MaximumHorizontalDisplacement))
                    .Append("@t=")
                    .Append(F(result.MaximumHorizontalDisplacementT))
                    .Append(" | topology=")
                    .Append(result.TopologyPassed ? "PASS" : "FAIL")
                    .Append(" contract=")
                    .Append(!result.TopologyPassed
                        ? "NOT_RUN"
                        : string.IsNullOrEmpty(result.InvariantFailure)
                            ? "PASS"
                            : "FAIL");
                if (!string.IsNullOrEmpty(result.Failure))
                {
                    report.Append(" | ").Append(result.Failure);
                }
                report.AppendLine();
            }
            if (!string.IsNullOrEmpty(failure))
            {
                report.AppendLine();
                report.AppendLine("[Runner Failure]");
                report.AppendLine(failure);
            }
            report.AppendLine();
            report.Append("Summary CSV: ").AppendLine(job.SummaryCsvPath);
            report.Append("Sample CSV: ").AppendLine(job.SampleCsvPath);
            File.WriteAllText(job.ReportPath, report.ToString(), Encoding.UTF8);
        }

        private static void Finish(
            Job job,
            string status,
            string failure)
        {
            if (job == null)
            {
                return;
            }
            WriteReport(job, status, failure);
            job.SummaryWriter?.Flush();
            job.SampleWriter?.Flush();
            job.SummaryWriter?.Dispose();
            job.SampleWriter?.Dispose();
            EditorApplication.update -= Tick;
            AssemblyReloadEvents.beforeAssemblyReload -= AbortForReload;
            EditorApplication.quitting -= AbortForQuit;
            activeJob = null;
            currentProgress = status == "COMPLETE" ? 1f : currentProgress;
            currentDetail = status;
            currentEta = string.Empty;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            string message =
                "[TREE-TRUNK.2] Lean/Bend contract verification " +
                status + ". Completed=" + job.Index + "/" +
                job.Cases.Count + ", pass=" + job.Passed +
                ", fail=" + job.Failed + ". Report: " + job.ReportPath;
            if (status == "COMPLETE" && job.Failed == 0)
            {
                Debug.Log(message, job.Source);
            }
            else
            {
                Debug.LogWarning(message, job.Source);
            }
        }

        private static List<CaseDefinition> BuildCases()
        {
            var cases = new List<CaseDefinition>();
            float[] persistence = { 0f, 0.5f, 1f };
            for (int index = 0; index < persistence.Length; index++)
            {
                cases.Add(Case(
                    "Neutral x Persistence",
                    "Neutral / P" + persistence[index].ToString("0.0", CultureInfo.InvariantCulture),
                    0f, 0f, persistence[index]));
            }

            float[] lean = { 0.15f, 0.30f, 0.60f };
            for (int leanIndex = 0; leanIndex < lean.Length; leanIndex++)
            {
                for (int persistenceIndex = 0;
                    persistenceIndex < persistence.Length;
                    persistenceIndex++)
                {
                    cases.Add(Case(
                        "Lean x Persistence",
                        "Lean " + lean[leanIndex].ToString("0.00", CultureInfo.InvariantCulture) +
                            " / P" + persistence[persistenceIndex].ToString("0.0", CultureInfo.InvariantCulture),
                        lean[leanIndex],
                        0f,
                        persistence[persistenceIndex]));
                }
            }

            float[] bend = { 0.50f, 1f };
            for (int bendIndex = 0; bendIndex < bend.Length; bendIndex++)
            {
                for (int persistenceIndex = 0;
                    persistenceIndex < persistence.Length;
                    persistenceIndex++)
                {
                    cases.Add(Case(
                        "Bend x Persistence",
                        "Bend " + bend[bendIndex].ToString("0.00", CultureInfo.InvariantCulture) +
                            " / P" + persistence[persistenceIndex].ToString("0.0", CultureInfo.InvariantCulture),
                        0f,
                        bend[bendIndex],
                        persistence[persistenceIndex]));
                }
            }
            CaseDefinition zeroFrequency = Case(
                "Bend Frequency Dependency",
                "Bend 1.00 / Frequency 0",
                0f,
                1f,
                0.5f);
            zeroFrequency.BendFrequency = 0f;
            cases.Add(zeroFrequency);

            Vector2[] combined =
            {
                new Vector2(0.30f, 0.50f),
                new Vector2(0.60f, 1f)
            };
            for (int comboIndex = 0; comboIndex < combined.Length; comboIndex++)
            {
                for (int persistenceIndex = 0;
                    persistenceIndex < persistence.Length;
                    persistenceIndex++)
                {
                    cases.Add(Case(
                        "Lean + Bend x Persistence",
                        "Lean " + combined[comboIndex].x.ToString("0.00", CultureInfo.InvariantCulture) +
                            " / Bend " + combined[comboIndex].y.ToString("0.00", CultureInfo.InvariantCulture) +
                            " / P" + persistence[persistenceIndex].ToString("0.0", CultureInfo.InvariantCulture),
                        combined[comboIndex].x,
                        combined[comboIndex].y,
                        persistence[persistenceIndex]));
                }
            }

            CaseDefinition lightRoot = Case(
                "Root Interaction",
                "Light roots / strong Lean+Bend",
                0.60f,
                1f,
                1f);
            lightRoot.RootHeight = 0.05f;
            lightRoot.RootReach = 0.40f;
            lightRoot.RootThickness = 0.50f;
            cases.Add(lightRoot);
            cases.Add(Case(
                "Root Interaction",
                "Authored roots / strong Lean+Bend",
                0.60f,
                1f,
                1f));
            CaseDefinition heavyRoot = Case(
                "Root Interaction",
                "Heavy roots / strong Lean+Bend",
                0.60f,
                1f,
                1f);
            heavyRoot.RootHeight = 0.40f;
            heavyRoot.RootReach = 2f;
            heavyRoot.RootThickness = 2f;
            cases.Add(heavyRoot);

            float[] twists = { 0f, 400f, -400f };
            for (int index = 0; index < twists.Length; index++)
            {
                CaseDefinition twist = Case(
                    "Axial Twist Interaction",
                    "Twist " + twists[index].ToString("0", CultureInfo.InvariantCulture),
                    0.30f,
                    0.50f,
                    1f);
                twist.Twist = twists[index];
                cases.Add(twist);
            }

            cases.Add(Case(
                "Path Spiral Interaction",
                "No Path Spiral",
                0.30f,
                0.50f,
                0.5f));
            float[] turns = { 1f, 2f, -2f };
            for (int index = 0; index < turns.Length; index++)
            {
                CaseDefinition spiral = Case(
                    "Path Spiral Interaction",
                    "Radius 0.25 / Turns " + turns[index].ToString("0", CultureInfo.InvariantCulture),
                    0.30f,
                    0.50f,
                    0.5f);
                spiral.SpiralRadius = 0.25f;
                spiral.SpiralTurns = turns[index];
                cases.Add(spiral);
            }
            return cases;
        }

        private static CaseDefinition Case(
            string group,
            string name,
            float lean,
            float bend,
            float persistence)
        {
            return new CaseDefinition
            {
                Group = group,
                Name = name,
                Lean = lean,
                Bend = bend,
                Persistence = persistence
            };
        }

        private static void PrepareIsolatedTrunkControls(
            TreeResolvedControls controls)
        {
            SetFloat(controls, "missingBranchChance", 0f);
            SetFloat(controls, "deadBranchChance", 0f);
            SetFloat(controls, "brokenBranchChance", 0f);
            SetInt(controls, "primaryBranchCount", 0);
            SetInt(controls, "maximumBranchOrder", 1);
            SetFloat(controls, "secondaryDensity", 0f);
            SetFloat(controls, "tertiaryDensity", 0f);
            SetFloat(controls, "forkChance", 0f);
            SetFloat(controls, "trunkDrift", 0f);
            SetFloat(controls, "trunkRoughness", 0f);
            SetFloat(controls, "leanAmount", 0f);
            SetFloat(controls, "bendAmount", 0f);
            SetFloat(controls, "bendFrequency", DefaultBendFrequency);
            SetFloat(controls, "pathSpiralRadius", 0f);
            SetFloat(controls, "signedPathSpiralTurns", 0f);
            SetFloat(controls, "axialTwist", 0f);
            controls.ValidateAndClamp();
        }

        private static TreeResolvedControls CloneControls(
            TreeResolvedControls source)
        {
            return source == null
                ? null
                : JsonUtility.FromJson<TreeResolvedControls>(
                    JsonUtility.ToJson(source));
        }

        private static void SetFloat(
            TreeResolvedControls controls,
            string fieldName,
            float value)
        {
            SetField(controls, fieldName, value);
        }

        private static void SetInt(
            TreeResolvedControls controls,
            string fieldName,
            int value)
        {
            SetField(controls, fieldName, value);
        }

        private static void SetField(
            TreeResolvedControls controls,
            string fieldName,
            object value)
        {
            FieldInfo field = typeof(TreeResolvedControls).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(
                    typeof(TreeResolvedControls).FullName,
                    fieldName);
            }
            field.SetValue(controls, value);
        }

        private static string FirstFailureLine(string report)
        {
            if (string.IsNullOrEmpty(report))
            {
                return "Unknown failure.";
            }
            string[] lines = report.Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index].Trim();
                if (line.StartsWith("FAIL", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("Failure", StringComparison.OrdinalIgnoreCase))
                {
                    return line;
                }
            }
            return lines.Length > 0 ? lines[0].Trim() : "Unknown failure.";
        }

        private static string F(float? value)
        {
            return value.HasValue
                ? value.Value.ToString("0.######", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string F(float value)
        {
            return value.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private static string Csv(string value)
        {
            value ??= string.Empty;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static string FormatDuration(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) ||
                seconds < 0.0)
            {
                return "--";
            }
            TimeSpan span = TimeSpan.FromSeconds(seconds);
            return span.TotalHours >= 1.0
                ? span.ToString(@"hh\:mm\:ss")
                : span.ToString(@"mm\:ss");
        }
    }
}
