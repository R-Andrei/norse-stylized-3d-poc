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
    internal static class TreeControlResponseSuite
    {
        private const string OutputDirectory =
            "Library/PS3D/Trees/ControlResponse";
        private const string ReportFileName =
            "TreeControlResponseSuiteReport.txt";
        private const string CsvFileName =
            "TreeControlResponseSuite.csv";
        private const int SamplesPerControl = 4;
        private const float FloatTolerance = 0.0005f;

        private sealed class Representative
        {
            internal string Name;
            internal TreeFamily Family;
            internal int Seed;
            internal string SourceIdentity;
            internal TreeResolvedControls Controls;
        }

        private sealed class SampleResult
        {
            internal string Label;
            internal string AuthoredValue;
            internal bool Passed;
            internal string Failure;
            internal TreeDefinition Definition;
            internal TreeBarkMeshBuildResult Bark;
            internal string ExactFingerprint;
            internal float PrimaryAttachmentMinimum;
            internal float PrimaryAttachmentMaximum;
            internal int PrimaryAttachmentCount;
            internal int EmittedTrunkControlPointCount;
            internal int TrunkSampleCount;
            internal float TrunkTipHeight;
            internal float MaximumTrunkSampleTurnDegrees;
        }

        private sealed class ControlSummary
        {
            internal string Representative;
            internal string StableId;
            internal string Label;
            internal bool Passed;
            internal string Finding;
        }

        private sealed class Job
        {
            internal List<Representative> Representatives;
            internal IReadOnlyList<TreeControlDescriptor> Descriptors;
            internal List<ControlSummary> Summaries =
                new List<ControlSummary>();
            internal int RepresentativeIndex;
            internal int ControlIndex;
            internal int SampleStage;
            internal SampleResult Baseline;
            internal readonly List<SampleResult> Samples =
                new List<SampleResult>(3);
            internal DateTime StartedUtc;
            internal int CompletedCases;
            internal int PassedCases;
            internal int FailedCases;
            internal bool CancelRequested;
            internal string ReportPath;
            internal string CsvPath;
            internal StreamWriter CsvWriter;
        }

        private static Job activeJob;
        private static string lastReportPath = string.Empty;
        private static string lastCsvPath = string.Empty;
        private static string currentDetail = "Not running";
        private static string currentEta = "";
        private static float currentProgress;

        internal static bool IsRunning => activeJob != null;
        internal static string CurrentDetail => currentDetail;
        internal static string CurrentEta => currentEta;
        internal static float CurrentProgress => currentProgress;
        internal static string LastReportPath => lastReportPath;
        internal static string LastCsvPath => lastCsvPath;

        internal static string ProgressLabel
        {
            get
            {
                if (activeJob == null)
                {
                    return "Not running";
                }

                int total = Mathf.Max(
                    1,
                    activeJob.Representatives.Count *
                    activeJob.Descriptors.Count *
                    SamplesPerControl);
                return activeJob.CompletedCases + " / " + total;
            }
        }

        internal static bool Start(ProceduralTreeInstance selected)
        {
            if (activeJob != null)
            {
                return false;
            }

            List<Representative> representatives =
                CollectRepresentatives(selected);
            if (representatives.Count != 4)
            {
                Debug.LogError(
                    "[TREE-CONTROLS.4] The exhaustive response suite requires four initialized curated gallery representatives: Alder Standard, Norway Spruce Standard, Wych Elm Leaning, and Dead Alder. Found " +
                    representatives.Count + ". Rebuild the curated recipe gallery first.");
                return false;
            }

            Directory.CreateDirectory(OutputDirectory);
            string reportPath = Path.Combine(
                OutputDirectory,
                ReportFileName);
            string csvPath = Path.Combine(OutputDirectory, CsvFileName);
            var writer = new StreamWriter(csvPath, false, Encoding.UTF8);
            writer.WriteLine(
                "Representative,ControlId,Control,Sample,AuthoredValue,CaseStatus," +
                "ExactFingerprint,StructuralFingerprint,TrunkFingerprint,BranchFingerprint," +
                "PaletteFingerprint,BarkFingerprint,Height,TrunkControlPoints," +
                "EmittedTrunkControlPoints,TrunkSamples,TrunkTipY,MaximumTrunkTurnDegrees,Branches," +
                "Primary,Secondary,Tertiary,Dead,Broken,BoundsX,BoundsY,BoundsZ," +
                "PrimaryAttachmentMin,PrimaryAttachmentMax,RootCrest,RootHalfWidthDegrees," +
                "RootHalfChord,RequestedRootSupportDegrees,EmittedRootSupportDegrees," +
                "RootSupportClamped,EvaluatedRootThickness,GroundBaseMergeFactor," +
                "FootShapePlateauEnd,RootZoneIntervals,MeasuredAxialTwist,Failure");
            writer.Flush();

            activeJob = new Job
            {
                Representatives = representatives,
                Descriptors = TreeControlDescriptorRegistry.Controls,
                StartedUtc = DateTime.UtcNow,
                ReportPath = reportPath,
                CsvPath = csvPath,
                CsvWriter = writer
            };
            lastReportPath = reportPath;
            lastCsvPath = csvPath;
            WritePartialReport(activeJob, "RUNNING");
            currentDetail = "Preparing first control case";
            currentEta = "ETA calculating";
            currentProgress = 0f;
            TreeControlResponseSuiteWindow.ShowWindow();
            EditorApplication.update += Tick;
            AssemblyReloadEvents.beforeAssemblyReload += AbortForReload;
            EditorApplication.quitting += AbortForQuit;
            Debug.Log(
                "[TREE-CONTROLS.4] Incremental 42-control response suite started. " +
                "Representatives=" + representatives.Count +
                ". Output: " + reportPath);
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
            EditorUtility.RevealInFinder(
                Path.GetFullPath(OutputDirectory));
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
                    "Assembly reload interrupted the suite after the partial report and CSV checkpoint were preserved.");
            }
        }

        private static void AbortForQuit()
        {
            if (activeJob != null)
            {
                Finish(
                    activeJob,
                    "CANCELLED",
                    "Editor shutdown interrupted the suite after the partial report and CSV checkpoint were preserved.");
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
                int totalCases = Mathf.Max(
                    1,
                    job.Representatives.Count *
                    job.Descriptors.Count *
                    SamplesPerControl);
                float progress = job.CompletedCases / (float)totalCases;
                TimeSpan elapsed = DateTime.UtcNow - job.StartedUtc;
                double secondsPerCase = job.CompletedCases > 0
                    ? elapsed.TotalSeconds / job.CompletedCases
                    : 0.0;
                double etaSeconds = secondsPerCase *
                    (totalCases - job.CompletedCases);
                Representative representative =
                    job.Representatives[job.RepresentativeIndex];
                TreeControlDescriptor descriptor =
                    job.Descriptors[job.ControlIndex];
                string sampleLabel = GetSampleLabel(job.SampleStage);
                currentProgress = progress;
                currentDetail = representative.Name + " — " +
                    descriptor.Label + " — " + sampleLabel;
                currentEta = "Elapsed " +
                    FormatDuration(elapsed.TotalSeconds) +
                    " | ETA " + FormatDuration(etaSeconds);
                TreeControlResponseSuiteWindow.RepaintOpenWindow();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                if (job.CancelRequested)
                {
                    Finish(job, "CANCELLED", null);
                    return;
                }

                RunOneCase(job, representative, descriptor);
                job.CompletedCases++;
                WritePartialReport(job, "RUNNING");

                if (!Advance(job))
                {
                    Finish(job, "COMPLETE", null);
                }
            }
            catch (Exception exception)
            {
                Finish(activeJob, "FAILED", exception.ToString());
            }
        }

        private static void RunOneCase(
            Job job,
            Representative representative,
            TreeControlDescriptor descriptor)
        {
            TreeResolvedControls prepared = PrepareBaseline(
                representative.Controls,
                descriptor.PropertyName);
            string sampleLabel = GetSampleLabel(job.SampleStage);
            string authoredValue = "baseline";
            if (job.SampleStage > 0)
            {
                object value = GetSampleValue(
                    descriptor,
                    job.SampleStage - 1);
                SetControlValue(prepared, descriptor, value);
                authoredValue = FormatAuthoredValue(value);
            }

            prepared.ValidateAndClamp();
            SampleResult sample = GenerateSample(
                representative,
                descriptor,
                prepared,
                sampleLabel,
                authoredValue);
            if (sample.Passed)
            {
                job.PassedCases++;
            }
            else
            {
                job.FailedCases++;
            }

            WriteCsvCase(job.CsvWriter, representative, descriptor, sample);
            job.CsvWriter.Flush();

            if (job.SampleStage == 0)
            {
                job.Baseline = sample;
                job.Samples.Clear();
            }
            else
            {
                job.Samples.Add(sample);
                if (job.SampleStage == SamplesPerControl - 1)
                {
                    job.Summaries.Add(EvaluateControl(
                        representative,
                        descriptor,
                        job.Baseline,
                        job.Samples));
                }
            }
        }

        private static SampleResult GenerateSample(
            Representative representative,
            TreeControlDescriptor descriptor,
            TreeResolvedControls controls,
            string sampleLabel,
            string authoredValue)
        {
            var sample = new SampleResult
            {
                Label = sampleLabel,
                AuthoredValue = authoredValue,
                ExactFingerprint = controls.CalculateFingerprint()
            };
            TreeGenerationResult generation =
                TreeGenerator.GenerateExactForValidation(
                    controls,
                    representative.Seed,
                    representative.SourceIdentity,
                    representative.Family);
            if (generation == null || !generation.Passed ||
                generation.Definition == null ||
                !generation.Definition.IsValid)
            {
                sample.Failure = generation != null
                    ? FirstFailureLine(generation.Report)
                    : "Tree generation returned null.";
                return sample;
            }

            sample.Definition = generation.Definition;
            CalculatePrimaryAttachmentRange(
                sample.Definition,
                out sample.PrimaryAttachmentMinimum,
                out sample.PrimaryAttachmentMaximum,
                out sample.PrimaryAttachmentCount);
            MeasureTrunkCenterline(
                sample.Definition,
                out sample.EmittedTrunkControlPointCount,
                out sample.TrunkSampleCount,
                out sample.TrunkTipHeight,
                out sample.MaximumTrunkSampleTurnDegrees);

            if (RequiresBarkResponse(descriptor.PropertyName))
            {
                var mesh = new Mesh
                {
                    name = "TREE-CONTROLS.4 Response Candidate"
                };
                try
                {
                    sample.Bark = TreeBarkMeshGenerator.Build(
                        sample.Definition,
                        TreeBarkMeshSettings.CreateRecipeOnlyDefaults(),
                        mesh);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(mesh);
                }

                if (sample.Bark == null || !sample.Bark.Passed)
                {
                    sample.Failure = sample.Bark != null
                        ? sample.Bark.Failure
                        : "Bark generation returned null.";
                    return sample;
                }
            }

            sample.Passed = true;
            return sample;
        }

        private static ControlSummary EvaluateControl(
            Representative representative,
            TreeControlDescriptor descriptor,
            SampleResult baseline,
            List<SampleResult> samples)
        {
            var summary = new ControlSummary
            {
                Representative = representative.Name,
                StableId = descriptor.StableId,
                Label = descriptor.Label
            };
            if (baseline == null || !baseline.Passed)
            {
                summary.Finding = "Baseline failed: " +
                    (baseline != null ? baseline.Failure : "missing");
                return summary;
            }

            for (int index = 0; index < samples.Count; index++)
            {
                if (samples[index] == null || !samples[index].Passed)
                {
                    summary.Finding = samples[index] != null
                        ? samples[index].Label + " failed: " +
                            samples[index].Failure
                        : "A sample result was missing.";
                    return summary;
                }
            }

            if (!HasExpectedResponse(
                    descriptor.PropertyName,
                    samples[0],
                    samples[2]))
            {
                summary.Finding =
                    "Low and high anchors produced no measurable output difference.";
                return summary;
            }

            if (!ValidateControlSpecificInvariants(
                    descriptor.PropertyName,
                    baseline,
                    samples,
                    out string invariantFailure))
            {
                summary.Finding = invariantFailure;
                return summary;
            }

            summary.Passed = true;
            summary.Finding = "Measurable response and control-specific invariants passed.";
            return summary;
        }

        private static bool HasExpectedResponse(
            string propertyName,
            SampleResult baseline,
            SampleResult sample)
        {
            if (propertyName == "barkTint")
            {
                return baseline.Definition.PaletteFingerprint !=
                    sample.Definition.PaletteFingerprint;
            }

            if (RequiresBarkResponse(propertyName))
            {
                return baseline.Bark != null && sample.Bark != null &&
                    baseline.Bark.GeometryFingerprint !=
                    sample.Bark.GeometryFingerprint;
            }

            return baseline.Definition.StructuralFingerprint !=
                sample.Definition.StructuralFingerprint;
        }

        private static bool ValidateControlSpecificInvariants(
            string propertyName,
            SampleResult baseline,
            List<SampleResult> samples,
            out string failure)
        {
            failure = string.Empty;
            SampleResult minimum = samples[0];
            SampleResult neutral = samples[1];
            SampleResult maximum = samples[2];

            switch (propertyName)
            {
                case "height":
                    if (!SameInt(
                            minimum.Definition.ResolvedParameters
                                .TrunkControlPointCount,
                            neutral.Definition.ResolvedParameters
                                .TrunkControlPointCount,
                            maximum.Definition.ResolvedParameters
                                .TrunkControlPointCount))
                    {
                        failure = "Height changed trunk control-point count.";
                        return false;
                    }
                    if (CalculateNormalizedTrunkDifference(
                            minimum.Definition,
                            maximum.Definition) > 0.002f)
                    {
                        failure = "Height changed normalized trunk shape.";
                        return false;
                    }
                    break;

                case "bendFrequency":
                    if (!SameInt(
                            minimum.Definition.ResolvedParameters
                                .TrunkControlPointCount,
                            neutral.Definition.ResolvedParameters
                                .TrunkControlPointCount,
                            maximum.Definition.ResolvedParameters
                                .TrunkControlPointCount))
                    {
                        failure = "Bend Frequency changed trunk control-point count.";
                        return false;
                    }
                    break;

                case "pathSpiralRadius":
                case "signedPathSpiralTurns":
                    if (!ValidatePathSpiralHeightContract(
                            baseline,
                            out failure))
                    {
                        return false;
                    }
                    for (int index = 0; index < samples.Count; index++)
                    {
                        if (!ValidatePathSpiralHeightContract(
                                samples[index],
                                out failure))
                        {
                            return false;
                        }
                    }
                    if (propertyName == "signedPathSpiralTurns" &&
                        (minimum.EmittedTrunkControlPointCount !=
                            maximum.EmittedTrunkControlPointCount ||
                         minimum.TrunkSampleCount !=
                            maximum.TrunkSampleCount))
                    {
                        failure =
                            "Opposite Signed Path Spiral handedness values emitted different centreline resolutions.";
                        return false;
                    }
                    break;

                case "axialTwist":
                    if (minimum.Definition.TrunkFingerprint !=
                            maximum.Definition.TrunkFingerprint ||
                        minimum.Definition.BranchFingerprint !=
                            maximum.Definition.BranchFingerprint)
                    {
                        failure =
                            "Axial Twist altered structural trunk or branch geometry.";
                        return false;
                    }
                    if (minimum.Bark == null || maximum.Bark == null ||
                        Mathf.Abs(
                            maximum.Bark.MeasuredAxialTwistDegrees -
                            minimum.Bark.MeasuredAxialTwistDegrees) < 30f)
                    {
                        failure = "Axial Twist did not produce measurable bark roll.";
                        return false;
                    }
                    break;

                case "rootThickness":
                    if (minimum.Bark.GroundRootHalfExtensionAngularWidthDegrees +
                            FloatTolerance >=
                        neutral.Bark.GroundRootHalfExtensionAngularWidthDegrees)
                    {
                        failure =
                            "Root Thickness did not broaden individual half-extension width before support saturation.";
                        return false;
                    }
                    if (neutral.Bark.GroundRootBaseMergeFactor >
                            FloatTolerance)
                    {
                        failure =
                            "Root Thickness neutral H4 anchor unexpectedly merged the lower base.";
                        return false;
                    }
                    if (maximum.Bark.GroundRootBaseMergeFactor <=
                            neutral.Bark.GroundRootBaseMergeFactor +
                            FloatTolerance)
                    {
                        failure =
                            "Root Thickness did not increase shared lower-base merge after support saturation.";
                        return false;
                    }
                    if (maximum.Bark.RequestedRootSupportAngularWidthDegrees <=
                            maximum.Bark.EmittedRootSupportAngularWidthDegrees +
                            FloatTolerance ||
                        !maximum.Bark.RootSupportWidthClampedByCount)
                    {
                        failure =
                            "Root Thickness high sample did not enter the support-saturated shared-base stage.";
                        return false;
                    }
                    if (!Approximately(
                            minimum.Bark.GroundButtressCrestMultiplier,
                            neutral.Bark.GroundButtressCrestMultiplier,
                            0.002f) ||
                        !Approximately(
                            neutral.Bark.GroundButtressCrestMultiplier,
                            maximum.Bark.GroundButtressCrestMultiplier,
                            0.002f))
                    {
                        failure = "Root Thickness changed Root Reach crest amplitude.";
                        return false;
                    }
                    break;

                case "rootReach":
                    if (minimum.Bark.GroundButtressCrestMultiplier +
                            FloatTolerance >=
                        neutral.Bark.GroundButtressCrestMultiplier ||
                        neutral.Bark.GroundButtressCrestMultiplier +
                            FloatTolerance >=
                        maximum.Bark.GroundButtressCrestMultiplier)
                    {
                        failure = "Root Reach did not increase ground crest amplitude monotonically.";
                        return false;
                    }
                    // Zero reach has no measurable half-extension contour.
                    // Compare the non-zero neutral/high anchors instead.
                    if (!Approximately(
                            neutral.Bark.GroundRootHalfExtensionAngularWidthDegrees,
                            maximum.Bark.GroundRootHalfExtensionAngularWidthDegrees,
                            0.05f))
                    {
                        failure = "Root Reach changed angular thickness.";
                        return false;
                    }
                    break;

                case "rootHeight":
                    if (minimum.Bark.RootZoneLongitudinalIntervals >=
                        maximum.Bark.RootZoneLongitudinalIntervals)
                    {
                        failure = "Root Height did not increase root-zone intervals.";
                        return false;
                    }
                    if (!Approximately(
                            minimum.Bark.GroundButtressCrestMultiplier,
                            maximum.Bark.GroundButtressCrestMultiplier,
                            0.002f) ||
                        !Approximately(
                            minimum.Bark.GroundRootHalfExtensionAngularWidthDegrees,
                            maximum.Bark.GroundRootHalfExtensionAngularWidthDegrees,
                            0.05f))
                    {
                        failure = "Root Height changed ground reach or thickness.";
                        return false;
                    }
                    break;

                case "buttressTransition":
                    if (maximum.Bark.EffectiveRootTransitionHeightNormalized <=
                        minimum.Bark.EffectiveRootTransitionHeightNormalized +
                        FloatTolerance)
                    {
                        failure =
                            "Buttress Persistence did not extend the root-owned lobes higher as the value increased.";
                        return false;
                    }
                    if (!Approximately(
                            minimum.Bark.GroundButtressCrestMultiplier,
                            maximum.Bark.GroundButtressCrestMultiplier,
                            0.002f) ||
                        !Approximately(
                            minimum.Bark.GroundRootHalfExtensionAngularWidthDegrees,
                            maximum.Bark.GroundRootHalfExtensionAngularWidthDegrees,
                            0.05f) ||
                        !Approximately(
                            minimum.Bark.AuthoredRootHeightNormalized,
                            maximum.Bark.AuthoredRootHeightNormalized,
                            0.0001f))
                    {
                        failure =
                            "Buttress Persistence changed ground root reach, thickness or Root Height.";
                        return false;
                    }
                    break;

                case "rootCount":
                    if (!Approximately(
                            minimum.Bark.RequestedRootSupportAngularWidthDegrees,
                            maximum.Bark.RequestedRootSupportAngularWidthDegrees,
                            0.001f))
                    {
                        failure = "Root Count changed requested root width.";
                        return false;
                    }
                    break;

                case "trunkTaper":
                    if (minimum.Definition.Branches[
                            minimum.Definition.TrunkBranchIndex].EndRadius <=
                        maximum.Definition.Branches[
                            maximum.Definition.TrunkBranchIndex].EndRadius)
                    {
                        failure = "Trunk Taper was not monotonic.";
                        return false;
                    }
                    break;

                case "branchStartHeight":
                case "branchEndHeight":
                    if (minimum.Definition.ResolvedParameters
                            .PrimaryBranchEndHeight + FloatTolerance <
                        minimum.Definition.ResolvedParameters
                            .PrimaryBranchStartHeight ||
                        maximum.Definition.ResolvedParameters
                            .PrimaryBranchEndHeight + FloatTolerance <
                        maximum.Definition.ResolvedParameters
                            .PrimaryBranchStartHeight)
                    {
                        failure = "Primary branch band inverted during validation.";
                        return false;
                    }
                    if (!Approximately(
                            minimum.Definition.ResolvedParameters
                                .TrunkForkHeight,
                            maximum.Definition.ResolvedParameters
                                .TrunkForkHeight,
                            0.0001f))
                    {
                        failure =
                            "Primary branch band changed recipe-only fork placement.";
                        return false;
                    }
                    break;

                case "tierSpacing":
                    if (maximum.PrimaryAttachmentCount > 1 &&
                        maximum.PrimaryAttachmentMaximum + 0.03f <
                        maximum.Definition.ResolvedParameters
                            .PrimaryBranchEndHeight)
                    {
                        failure = "Tier Spacing omitted the final authored tier.";
                        return false;
                    }
                    break;

                case "tipUpturn":
                    if (!ValidateTipWindow(
                            minimum.Definition,
                            maximum.Definition,
                            out failure))
                    {
                        return false;
                    }
                    break;

                case "deadBranchChance":
                    if (minimum.Definition.Metrics.DeadBranchCount >=
                            maximum.Definition.Metrics.DeadBranchCount ||
                        minimum.Bark == null || maximum.Bark == null ||
                        minimum.Bark.GeometryFingerprint ==
                            maximum.Bark.GeometryFingerprint)
                    {
                        failure = "Dead Branch Chance produced no visible bark metadata response.";
                        return false;
                    }
                    break;

                case "barkTint":
                    if (!Approximately(
                            minimum.Definition.ResolvedParameters.BarkTint.a,
                            1f,
                            0.0001f) ||
                        !Approximately(
                            maximum.Definition.ResolvedParameters.BarkTint.a,
                            1f,
                            0.0001f))
                    {
                        failure = "Bark Tint alpha was not forced opaque.";
                        return false;
                    }
                    break;

                case "forkChance":
                    if (minimum.Definition.Metrics.BranchCount >=
                        maximum.Definition.Metrics.BranchCount)
                    {
                        failure = "Fork Chance produced no structural fork response.";
                        return false;
                    }
                    break;
            }

            return true;
        }

        private static bool ValidateTipWindow(
            TreeDefinition minimum,
            TreeDefinition maximum,
            out string failure)
        {
            failure = string.Empty;
            var byId = new Dictionary<int, TreeBranchDefinition>();
            for (int index = 0; index < minimum.Branches.Count; index++)
            {
                TreeBranchDefinition branch = minimum.Branches[index];
                if (branch != null)
                {
                    byId[branch.StableBranchId] = branch;
                }
            }

            float earlyMaximum = 0f;
            float lateMaximum = 0f;
            for (int index = 0; index < maximum.Branches.Count; index++)
            {
                TreeBranchDefinition changed = maximum.Branches[index];
                // Primary branches are the valid world-space witness for the
                // local Tip Upturn window because their parent trunk is invariant.
                // Higher-order roots may legitimately translate when their
                // parent's affected tip suffix moves.
                if (changed == null || changed.BranchOrder != 1 ||
                    !byId.TryGetValue(changed.StableBranchId, out TreeBranchDefinition original))
                {
                    continue;
                }

                int count = Mathf.Min(
                    original.Samples.Count,
                    changed.Samples.Count);
                for (int sampleIndex = 0; sampleIndex < count; sampleIndex++)
                {
                    float distance = changed.Samples[sampleIndex]
                        .NormalizedDistance;
                    float delta = Vector3.Distance(
                        original.Samples[sampleIndex].Position,
                        changed.Samples[sampleIndex].Position);
                    if (distance <= 0.72f + 0.0001f)
                    {
                        earlyMaximum = Mathf.Max(earlyMaximum, delta);
                    }
                    else
                    {
                        lateMaximum = Mathf.Max(lateMaximum, delta);
                    }
                }
            }

            if (earlyMaximum > 0.0005f)
            {
                failure = "Tip Upturn moved samples before the 72 percent tip window.";
                return false;
            }
            if (lateMaximum <= earlyMaximum + 0.001f)
            {
                failure = "Tip Upturn produced no confined late-branch displacement.";
                return false;
            }
            return true;
        }

        private static bool Advance(Job job)
        {
            job.SampleStage++;
            if (job.SampleStage < SamplesPerControl)
            {
                return true;
            }

            job.SampleStage = 0;
            job.Baseline = null;
            job.Samples.Clear();
            job.ControlIndex++;
            if (job.ControlIndex < job.Descriptors.Count)
            {
                return true;
            }

            job.ControlIndex = 0;
            job.RepresentativeIndex++;
            return job.RepresentativeIndex < job.Representatives.Count;
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
            try
            {
                job.CsvWriter?.Flush();
                job.CsvWriter?.Dispose();
            }
            catch
            {
                // Preserve the primary failure/report path.
            }

            WritePartialReport(job, outcome, fatalFailure);
            activeJob = null;
            currentProgress = outcome == "COMPLETE" ? 1f : currentProgress;
            currentDetail = "Suite " + outcome;
            currentEta = "Report checkpointed";
            TreeControlResponseSuiteWindow.RepaintOpenWindow();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            int failedControls = 0;
            for (int index = 0; index < job.Summaries.Count; index++)
            {
                if (!job.Summaries[index].Passed)
                {
                    failedControls++;
                }
            }

            string message =
                "[TREE-CONTROLS.4] Control response suite " + outcome +
                ". Completed cases=" + job.CompletedCases +
                ", failed cases=" + job.FailedCases +
                ", failed control/representative summaries=" +
                failedControls + ". Report: " + job.ReportPath;
            if (outcome == "COMPLETE" && failedControls == 0 &&
                job.FailedCases == 0)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogWarning(message);
            }
        }

        private static void WritePartialReport(
            Job job,
            string outcome,
            string fatalFailure = null)
        {
            if (job == null)
            {
                return;
            }

            var report = new StringBuilder(32768);
            report.AppendLine("[TREE-CONTROLS.4 Exhaustive Control Response Suite]");
            report.Append("Generated UTC: ")
                .AppendLine(DateTime.UtcNow.ToString("O"));
            report.Append("Outcome: ").AppendLine(outcome);
            report.Append("Generator version: ")
                .AppendLine(TreeGenerator.CurrentGeneratorVersion.ToString());
            report.Append("Bark algorithm version: ")
                .AppendLine(TreeBarkMeshGenerator.BarkAlgorithmVersion.ToString());
            report.Append("Representatives: ")
                .AppendLine(job.Representatives.Count.ToString());
            report.Append("Controls: ")
                .AppendLine(job.Descriptors.Count.ToString());
            report.Append("Completed / passed / failed cases: ")
                .Append(job.CompletedCases).Append(" / ")
                .Append(job.PassedCases).Append(" / ")
                .AppendLine(job.FailedCases.ToString());
            report.Append("Elapsed: ")
                .AppendLine(FormatDuration(
                    (DateTime.UtcNow - job.StartedUtc).TotalSeconds));
            if (!string.IsNullOrEmpty(fatalFailure))
            {
                report.AppendLine();
                report.AppendLine("[Fatal Failure]");
                report.AppendLine(fatalFailure);
            }

            report.AppendLine();
            report.AppendLine("[Representative Set]");
            for (int index = 0; index < job.Representatives.Count; index++)
            {
                Representative representative = job.Representatives[index];
                report.Append("- ").Append(representative.Name)
                    .Append(" | family=").Append(representative.Family)
                    .Append(" | seed=").Append(representative.Seed)
                    .Append(" | source=")
                    .AppendLine(representative.SourceIdentity);
            }

            report.AppendLine();
            report.AppendLine("[Completed Control Findings]");
            int passed = 0;
            int failed = 0;
            for (int index = 0; index < job.Summaries.Count; index++)
            {
                ControlSummary summary = job.Summaries[index];
                if (summary.Passed)
                {
                    passed++;
                }
                else
                {
                    failed++;
                }
                report.Append(summary.Passed ? "PASS | " : "FAIL | ")
                    .Append(summary.Representative).Append(" | ")
                    .Append(summary.Label).Append(" | ")
                    .Append(summary.StableId).Append(" | ")
                    .AppendLine(summary.Finding);
            }

            report.AppendLine();
            report.AppendLine("[Summary]");
            report.Append("Completed control/representative checks: ")
                .AppendLine(job.Summaries.Count.ToString());
            report.Append("Passed: ").AppendLine(passed.ToString());
            report.Append("Failed: ").AppendLine(failed.ToString());
            report.Append("CSV: ").AppendLine(job.CsvPath);
            string status = outcome == "COMPLETE" && failed == 0 &&
                job.FailedCases == 0
                    ? "PASS"
                    : outcome == "RUNNING"
                        ? "RUNNING"
                        : outcome;
            report.Append("Status: ").AppendLine(status);
            File.WriteAllText(job.ReportPath, report.ToString(), Encoding.UTF8);
        }

        private static List<Representative> CollectRepresentatives(
            ProceduralTreeInstance selected)
        {
            var desired = new[]
            {
                new
                {
                    Name = "Alder",
                    Recipe = "tree-recipe-curated-alder-standard",
                    Slot = TreeGenerationLibraryVariant.BuildStableKey(
                        TreeFamily.Common,
                        1)
                },
                new
                {
                    Name = "Norway Spruce",
                    Recipe = "tree-recipe-curated-norway-spruce-standard",
                    Slot = TreeGenerationLibraryVariant.BuildStableKey(
                        TreeFamily.Pine,
                        1)
                },
                new
                {
                    Name = "Wych Elm",
                    Recipe = "tree-recipe-curated-wych-elm-leaning",
                    Slot = TreeGenerationLibraryVariant.BuildStableKey(
                        TreeFamily.Twisted,
                        1)
                },
                new
                {
                    Name = "Dead",
                    Recipe = "tree-recipe-curated-dead-alder",
                    Slot = TreeGenerationLibraryVariant.BuildStableKey(
                        TreeFamily.Dead,
                        1)
                }
            };
            ProceduralTreeInstance[] instances =
                UnityEngine.Object.FindObjectsByType<ProceduralTreeInstance>(
                    FindObjectsInactive.Include);
            var representatives = new List<Representative>(4);
            var used = new HashSet<EntityId>();
            for (int desiredIndex = 0;
                desiredIndex < desired.Length;
                desiredIndex++)
            {
                ProceduralTreeInstance match = null;
                for (int index = 0; index < instances.Length; index++)
                {
                    ProceduralTreeInstance candidate = instances[index];
                    if (candidate == null || !candidate.HasExactControls ||
                        used.Contains(candidate.GetEntityId()))
                    {
                        continue;
                    }

                    string identity = candidate.Recipe != null
                        ? candidate.Recipe.StableIdentity
                        : candidate.ExactControlsSourceRecipeIdentity;
                    if (identity == desired[desiredIndex].Recipe &&
                        candidate.StableSlotIdentity ==
                            desired[desiredIndex].Slot)
                    {
                        match = candidate;
                        break;
                    }
                }

                if (match != null)
                {
                    used.Add(match.GetEntityId());
                    representatives.Add(CreateRepresentative(
                        desired[desiredIndex].Name,
                        match));
                }
            }

            return representatives;
        }

        private static Representative CreateRepresentative(
            string name,
            ProceduralTreeInstance instance)
        {
            TreeResolvedControls controls;
            string sourceIdentity;
            if (instance.Recipe != null &&
                instance.Recipe.ControlRanges != null)
            {
                controls = new TreeResolvedControls();
                controls.ResolveFrom(
                    instance.Recipe.ControlRanges,
                    instance.MasterSeed);
                sourceIdentity = instance.Recipe.StableIdentity;
            }
            else
            {
                controls = CloneControls(instance.ExactControls);
                sourceIdentity = !string.IsNullOrEmpty(
                    instance.ExactControlsSourceRecipeIdentity)
                        ? instance.ExactControlsSourceRecipeIdentity
                        : "tree-control-response-" + name;
            }

            return new Representative
            {
                Name = name,
                Family = instance.Family,
                Seed = instance.MasterSeed,
                SourceIdentity = sourceIdentity,
                Controls = controls
            };
        }

        private static TreeResolvedControls PrepareBaseline(
            TreeResolvedControls source,
            string propertyName)
        {
            TreeResolvedControls controls = CloneControls(source);
            SetFloat(controls, "missingBranchChance", 0f);
            SetFloat(controls, "brokenBranchChance", 0f);

            switch (propertyName)
            {
                case "bendFrequency":
                    SetFloat(controls, "bendAmount", 0.35f);
                    SetFloat(controls, "trunkDrift", 0f);
                    SetFloat(controls, "trunkRoughness", 0f);
                    break;
                case "leanDirection":
                    SetFloat(controls, "leanAmount", 0.25f);
                    break;
                case "pathSpiralRadius":
                    SetFloat(controls, "signedPathSpiralTurns", 3f);
                    break;
                case "signedPathSpiralTurns":
                    SetFloat(controls, "pathSpiralRadius", 0.50f);
                    break;
                case "rootCount":
                case "rootReach":
                case "rootThickness":
                case "rootHeight":
                    SetInt(controls, "rootCount", 6);
                    SetFloat(controls, "rootReach", 0.8f);
                    SetFloat(controls, "rootThickness", 0.5f);
                    SetFloat(controls, "rootHeight", 0.2f);
                    break;
                case "primaryBranchCount":
                    SetInt(controls, "maximumBranchOrder", 1);
                    SetFloat(controls, "forkChance", 0f);
                    break;
                case "branchStartHeight":
                    SetFloat(controls, "branchEndHeight", 0.95f);
                    SetInt(controls, "primaryBranchCount", 16);
                    break;
                case "branchEndHeight":
                    SetFloat(controls, "branchStartHeight", 0f);
                    SetInt(controls, "primaryBranchCount", 16);
                    break;
                case "maximumBranchOrder":
                    SetFloat(controls, "secondaryDensity", 2f);
                    SetFloat(controls, "tertiaryDensity", 1f);
                    SetInt(controls, "primaryBranchCount", 8);
                    break;
                case "secondaryDensity":
                    SetInt(controls, "maximumBranchOrder", 2);
                    SetInt(controls, "primaryBranchCount", 8);
                    break;
                case "tertiaryDensity":
                case "childScale":
                    SetInt(controls, "maximumBranchOrder", 3);
                    SetInt(controls, "primaryBranchCount", 8);
                    SetFloat(controls, "secondaryDensity", 2f);
                    SetFloat(controls, "tertiaryDensity", 1f);
                    break;
                case "deadBranchChance":
                case "missingBranchChance":
                case "brokenBranchChance":
                    SetInt(controls, "maximumBranchOrder", 3);
                    SetInt(controls, "primaryBranchCount", 12);
                    SetFloat(controls, "secondaryDensity", 1.5f);
                    SetFloat(controls, "tertiaryDensity", 0.7f);
                    break;
                case "directionalBiasAngle":
                    SetFloat(controls, "directionalBias", 0.75f);
                    break;
                case "tierSpacing":
                    SetInt(controls, "primaryBranchCount", 16);
                    SetFloat(controls, "branchStartHeight", 0.20f);
                    SetFloat(controls, "branchEndHeight", 0.90f);
                    break;
                case "branchArch":
                case "lateSag":
                case "tipUpturn":
                case "sideSweep":
                case "branchCurvature":
                    SetInt(controls, "primaryBranchCount", 10);
                    SetFloat(controls, "branchLength", 0.42f);
                    break;
                case "forkChance":
                    SetInt(controls, "primaryBranchCount", 8);
                    break;
            }

            controls.ValidateAndClamp();
            return controls;
        }

        private static object GetSampleValue(
            TreeControlDescriptor descriptor,
            int sampleIndex)
        {
            if (descriptor.Kind == TreeControlValueKind.Color)
            {
                if (sampleIndex == 0)
                {
                    return new Color(0.20f, 0.20f, 0.20f, 1f);
                }
                if (sampleIndex == 1)
                {
                    return new Color(0.55f, 0.55f, 0.55f, 1f);
                }
                return new Color(1f, 0.88f, 0.72f, 1f);
            }

            string property = descriptor.PropertyName;
            if (property == "height")
            {
                return sampleIndex == 0 ? 1f : sampleIndex == 1 ? 10f : 40f;
            }
            if (property == "trunkBaseRadius")
            {
                return sampleIndex == 0 ? 0.02f : sampleIndex == 1 ? 0.60f : 4f;
            }
            if (property == "axialTwist")
            {
                return sampleIndex == 0 ? -1080f : sampleIndex == 1 ? 0f : 1080f;
            }
            if (property == "signedPathSpiralTurns")
            {
                return sampleIndex == 0 ? -3f : sampleIndex == 1 ? 0f : 3f;
            }
            if (property == "leanDirection" ||
                property == "directionalBiasAngle")
            {
                return sampleIndex == 0 ? 0f : sampleIndex == 1 ? 120f : 240f;
            }
            if (property == "rootCount")
            {
                return sampleIndex == 0 ? 3 : sampleIndex == 1 ? 6 : 8;
            }
            if (property == "rootReach")
            {
                return sampleIndex == 0 ? 0f : sampleIndex == 1 ? 0.80f : 2f;
            }
            if (property == "rootThickness")
            {
                return sampleIndex == 0 ? 0.10f : sampleIndex == 1 ? 0.50f : 2f;
            }
            if (property == "rootHeight")
            {
                return sampleIndex == 0 ? 0.01f : sampleIndex == 1 ? 0.20f : 0.40f;
            }
            if (property == "branchStartHeight")
            {
                return sampleIndex == 0 ? 0f : sampleIndex == 1 ? 0.40f : 1f;
            }
            if (property == "branchEndHeight")
            {
                return sampleIndex == 0 ? 0f : sampleIndex == 1 ? 0.60f : 1f;
            }
            if (property == "tierSpacing")
            {
                return sampleIndex == 0 ? 0f : sampleIndex == 1 ? 0.08f : 0.50f;
            }
            if (property == "bendFrequency")
            {
                return sampleIndex == 0 ? 0f : sampleIndex == 1 ? 1.5f : 6f;
            }
            if (property == "primaryBranchCount")
            {
                return sampleIndex == 0 ? 0 : sampleIndex == 1 ? 16 : 64;
            }
            if (property == "maximumBranchOrder")
            {
                return sampleIndex + 1;
            }
            if (property == "secondaryDensity" ||
                property == "tertiaryDensity")
            {
                return sampleIndex == 0 ? 0f : sampleIndex == 1 ? 2.5f : 8f;
            }

            if (descriptor.Kind == TreeControlValueKind.Integer)
            {
                return Mathf.RoundToInt(Mathf.Lerp(
                    descriptor.HardMinimum,
                    descriptor.HardMaximum,
                    sampleIndex / 2f));
            }

            return Mathf.Lerp(
                descriptor.HardMinimum,
                descriptor.HardMaximum,
                sampleIndex / 2f);
        }

        private static bool RequiresBarkResponse(string propertyName)
        {
            switch (propertyName)
            {
                case "trunkTaper":
                case "axialTwist":
                case "rootCount":
                case "rootReach":
                case "rootThickness":
                case "rootHeight":
                case "deadBranchChance":
                    return true;
                default:
                    return false;
            }
        }

        private static void SetControlValue(
            TreeResolvedControls controls,
            TreeControlDescriptor descriptor,
            object value)
        {
            FieldInfo field = typeof(TreeResolvedControls).GetField(
                descriptor.PropertyName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(
                    typeof(TreeResolvedControls).FullName,
                    descriptor.PropertyName);
            }

            if (descriptor.Kind == TreeControlValueKind.Integer)
            {
                field.SetValue(controls, Convert.ToInt32(value));
            }
            else if (descriptor.Kind == TreeControlValueKind.Color)
            {
                Color color = (Color)value;
                color.a = 1f;
                field.SetValue(controls, color);
            }
            else
            {
                field.SetValue(
                    controls,
                    Convert.ToSingle(value, CultureInfo.InvariantCulture));
            }
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

        private static TreeResolvedControls CloneControls(
            TreeResolvedControls source)
        {
            if (source == null)
            {
                return null;
            }
            return JsonUtility.FromJson<TreeResolvedControls>(
                JsonUtility.ToJson(source));
        }

        private static void CalculatePrimaryAttachmentRange(
            TreeDefinition definition,
            out float minimum,
            out float maximum,
            out int count)
        {
            minimum = float.PositiveInfinity;
            maximum = float.NegativeInfinity;
            count = 0;
            for (int index = 0; index < definition.Branches.Count; index++)
            {
                TreeBranchDefinition branch = definition.Branches[index];
                if (branch == null || branch.BranchOrder != 1 ||
                    branch.ParentBranchIndex != definition.TrunkBranchIndex)
                {
                    continue;
                }
                minimum = Mathf.Min(minimum, branch.ParentAttachmentDistance);
                maximum = Mathf.Max(maximum, branch.ParentAttachmentDistance);
                count++;
            }
            if (count == 0)
            {
                minimum = 0f;
                maximum = 0f;
            }
        }

        private static void MeasureTrunkCenterline(
            TreeDefinition definition,
            out int controlPointCount,
            out int sampleCount,
            out float tipHeight,
            out float maximumTurnDegrees)
        {
            controlPointCount = 0;
            sampleCount = 0;
            tipHeight = 0f;
            maximumTurnDegrees = 0f;
            if (definition == null ||
                definition.Branches == null ||
                definition.TrunkBranchIndex < 0 ||
                definition.TrunkBranchIndex >= definition.Branches.Count)
            {
                return;
            }

            TreeBranchDefinition trunk =
                definition.Branches[definition.TrunkBranchIndex];
            if (trunk == null)
            {
                return;
            }

            controlPointCount = trunk.ControlPoints != null
                ? trunk.ControlPoints.Count
                : 0;
            sampleCount = trunk.Samples != null
                ? trunk.Samples.Count
                : 0;
            if (sampleCount == 0)
            {
                return;
            }

            tipHeight = trunk.Samples[sampleCount - 1].Position.y;
            for (int index = 1; index < sampleCount; index++)
            {
                maximumTurnDegrees = Mathf.Max(
                    maximumTurnDegrees,
                    Vector3.Angle(
                        trunk.Samples[index - 1].Tangent,
                        trunk.Samples[index].Tangent));
            }
        }

        private static bool ValidatePathSpiralHeightContract(
            SampleResult sample,
            out string failure)
        {
            failure = string.Empty;
            if (sample == null || sample.Definition == null ||
                sample.Definition.ResolvedParameters == null)
            {
                failure =
                    "Path Spiral validation received no generated definition.";
                return false;
            }

            TreeResolvedParameters parameters =
                sample.Definition.ResolvedParameters;
            bool active = parameters.RecipeOnlyControlSource &&
                parameters.TrunkSpiralStrength > 0.00001f;
            if (!active)
            {
                return true;
            }

            float tolerance = Mathf.Max(
                0.0005f,
                Mathf.Abs(parameters.Height) * 0.00005f);
            if (Mathf.Abs(sample.TrunkTipHeight - parameters.Height) >
                tolerance)
            {
                failure =
                    "Path Spiral changed authored trunk tip height: requested=" +
                    parameters.Height.ToString("F4", CultureInfo.InvariantCulture) +
                    ", emitted=" +
                    sample.TrunkTipHeight.ToString("F4", CultureInfo.InvariantCulture) +
                    ".";
                return false;
            }

            if (sample.EmittedTrunkControlPointCount < 2 ||
                sample.TrunkSampleCount < 2)
            {
                failure =
                    "Path Spiral emitted an incomplete trunk centreline.";
                return false;
            }

            return true;
        }

        private static float CalculateNormalizedTrunkDifference(
            TreeDefinition first,
            TreeDefinition second)
        {
            TreeBranchDefinition firstTrunk =
                first.Branches[first.TrunkBranchIndex];
            TreeBranchDefinition secondTrunk =
                second.Branches[second.TrunkBranchIndex];
            int count = Mathf.Min(
                firstTrunk.Samples.Count,
                secondTrunk.Samples.Count);
            if (count == 0 ||
                firstTrunk.Samples.Count != secondTrunk.Samples.Count)
            {
                return float.PositiveInfinity;
            }

            float firstHeight = Mathf.Max(
                0.0001f,
                first.ResolvedParameters.Height);
            float secondHeight = Mathf.Max(
                0.0001f,
                second.ResolvedParameters.Height);
            float maximum = 0f;
            for (int index = 0; index < count; index++)
            {
                Vector3 a = firstTrunk.Samples[index].Position / firstHeight;
                Vector3 b = secondTrunk.Samples[index].Position / secondHeight;
                maximum = Mathf.Max(maximum, Vector3.Distance(a, b));
            }
            return maximum;
        }

        private static bool SameInt(int first, int second, int third)
        {
            return first == second && second == third;
        }

        private static bool Approximately(
            float first,
            float second,
            float tolerance)
        {
            return Mathf.Abs(first - second) <= tolerance;
        }

        private static string GetSampleLabel(int stage)
        {
            switch (stage)
            {
                case 0: return "BASELINE";
                case 1: return "LOW";
                case 2: return "NEUTRAL";
                default: return "HIGH";
            }
        }

        private static string FormatAuthoredValue(object value)
        {
            if (value is Color color)
            {
                return color.ToString();
            }
            if (value is float number)
            {
                return number.ToString("F5", CultureInfo.InvariantCulture);
            }
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static string FormatDuration(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) ||
                seconds < 0.0)
            {
                return "unknown";
            }
            TimeSpan span = TimeSpan.FromSeconds(seconds);
            if (span.TotalHours >= 1.0)
            {
                return span.ToString(@"h\:mm\:ss");
            }
            return span.ToString(@"m\:ss");
        }

        private static string FirstFailureLine(string report)
        {
            if (string.IsNullOrWhiteSpace(report))
            {
                return "Generation failed without a report.";
            }
            string[] lines = report.Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                if (lines[index].StartsWith("FAIL", StringComparison.Ordinal))
                {
                    return lines[index].Trim();
                }
            }
            return "Generation failed; inspect the complete case report.";
        }

        private static void WriteCsvCase(
            StreamWriter writer,
            Representative representative,
            TreeControlDescriptor descriptor,
            SampleResult sample)
        {
            TreeDefinition definition = sample.Definition;
            TreeGenerationMetrics metrics = definition != null
                ? definition.Metrics
                : null;
            TreeResolvedParameters parameters = definition != null
                ? definition.ResolvedParameters
                : null;
            Bounds bounds = definition != null
                ? definition.LocalBounds
                : new Bounds();
            var values = new List<string>
            {
                representative.Name,
                descriptor.StableId,
                descriptor.Label,
                sample.Label,
                sample.AuthoredValue,
                sample.Passed ? "PASS" : "FAIL",
                sample.ExactFingerprint,
                definition != null ? definition.StructuralFingerprint : string.Empty,
                definition != null ? definition.TrunkFingerprint : string.Empty,
                definition != null ? definition.BranchFingerprint : string.Empty,
                definition != null ? definition.PaletteFingerprint : string.Empty,
                sample.Bark != null ? sample.Bark.GeometryFingerprint : string.Empty,
                parameters != null ? F(parameters.Height) : string.Empty,
                parameters != null ? parameters.TrunkControlPointCount.ToString() : string.Empty,
                sample.EmittedTrunkControlPointCount.ToString(),
                sample.TrunkSampleCount.ToString(),
                F(sample.TrunkTipHeight),
                F(sample.MaximumTrunkSampleTurnDegrees),
                metrics != null ? metrics.BranchCount.ToString() : string.Empty,
                metrics != null ? metrics.PrimaryBranchCount.ToString() : string.Empty,
                metrics != null ? metrics.SecondaryBranchCount.ToString() : string.Empty,
                metrics != null ? metrics.TertiaryBranchCount.ToString() : string.Empty,
                metrics != null ? metrics.DeadBranchCount.ToString() : string.Empty,
                metrics != null ? metrics.BrokenBranchCount.ToString() : string.Empty,
                definition != null ? F(bounds.size.x) : string.Empty,
                definition != null ? F(bounds.size.y) : string.Empty,
                definition != null ? F(bounds.size.z) : string.Empty,
                F(sample.PrimaryAttachmentMinimum),
                F(sample.PrimaryAttachmentMaximum),
                sample.Bark != null ? F(sample.Bark.GroundButtressCrestMultiplier) : string.Empty,
                sample.Bark != null ? F(sample.Bark.GroundRootHalfExtensionAngularWidthDegrees) : string.Empty,
                sample.Bark != null ? F(sample.Bark.GroundRootHalfExtensionChordWidth) : string.Empty,
                sample.Bark != null ? F(sample.Bark.RequestedRootSupportAngularWidthDegrees) : string.Empty,
                sample.Bark != null ? F(sample.Bark.EmittedRootSupportAngularWidthDegrees) : string.Empty,
                sample.Bark != null ? sample.Bark.RootSupportWidthClampedByCount.ToString() : string.Empty,
                sample.Bark != null ? F(sample.Bark.EvaluatedRootThickness) : string.Empty,
                sample.Bark != null ? F(sample.Bark.GroundRootBaseMergeFactor) : string.Empty,
                sample.Bark != null ? F(sample.Bark.RootFootShapePlateauEndNormalized) : string.Empty,
                sample.Bark != null ? sample.Bark.RootZoneLongitudinalIntervals.ToString() : string.Empty,
                sample.Bark != null ? F(sample.Bark.MeasuredAxialTwistDegrees) : string.Empty,
                sample.Failure
            };
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

        private static string F(float value)
        {
            return value.ToString("F6", CultureInfo.InvariantCulture);
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

    internal sealed class TreeControlResponseSuiteWindow : EditorWindow
    {
        private static TreeControlResponseSuiteWindow openWindow;

        internal static void ShowWindow()
        {
            openWindow = GetWindow<TreeControlResponseSuiteWindow>(
                false,
                "Tree Control Response",
                true);
            openWindow.minSize = new Vector2(460f, 190f);
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
                "TREE-CONTROLS.4 Exhaustive Response Suite",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "The diagnostic advances one bounded control case per Editor update. Closing this window does not stop it; use Cancel here or from the tree Inspector.",
                MessageType.None);
            Rect progressRect = GUILayoutUtility.GetRect(
                10f,
                20f,
                GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(
                progressRect,
                TreeControlResponseSuite.CurrentProgress,
                TreeControlResponseSuite.ProgressLabel);
            EditorGUILayout.LabelField(
                "Current",
                TreeControlResponseSuite.CurrentDetail);
            EditorGUILayout.LabelField(
                "Timing",
                TreeControlResponseSuite.CurrentEta);

            if (TreeControlResponseSuite.IsRunning)
            {
                if (GUILayout.Button("Cancel After Current Bounded Case"))
                {
                    TreeControlResponseSuite.RequestCancel();
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "The suite is not currently running. The last partial or final report remains available below.",
                    MessageType.Info);
            }

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(
                string.IsNullOrEmpty(
                    TreeControlResponseSuite.LastReportPath)))
            {
                if (GUILayout.Button("Copy Report"))
                {
                    TreeControlResponseSuite.CopyLastReport();
                }
                if (GUILayout.Button("Open Output Folder"))
                {
                    TreeControlResponseSuite.OpenOutputFolder();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

    }
}
