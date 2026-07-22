using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Vegetation
{
    public enum VegetationBenchmarkRunnerMode
    {
        EnabledStack = 0,
        ControlledLayerMatrix = 1
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Vegetation/Vegetation Benchmark Runner")]
    public sealed class VegetationBenchmarkRunner : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Optional hierarchy root used to limit layer inventory and timing. Leave empty to use the current scene.")]
        private Transform scopeRoot;

        [Header("Timed Benchmark")]
        [SerializeField]
        private VegetationBenchmarkRunnerMode benchmarkMode =
            VegetationBenchmarkRunnerMode.EnabledStack;

        [SerializeField]
        [Tooltip("Layer isolated by Controlled Layer Matrix mode. It must be active and inside the runner scope.")]
        private VegetationLayer controlledLayer;

        [SerializeField, Min(0.1f)]
        private float warmupSeconds = 0.75f;

        [SerializeField, Min(0.25f)]
        private float measurementSeconds = 2f;

        [SerializeField, Range(1, 10)]
        private int passesPerCase = 3;

        [SerializeField]
        private bool captureScreenshots = true;

        [SerializeField]
        private bool interleaveDisabledBaseline = true;

        private readonly List<LayerState> capturedLayerStates =
            new List<LayerState>();
        private Coroutine suiteCoroutine;
        private bool suiteRunning;
        private int suiteCurrentCase;
        private int suiteTotalCases;
        private string suiteStatus = "Not run";
        private string lastTimedSuiteReport = string.Empty;
        private string lastTimedSuiteReportPath = string.Empty;
        private VegetationBenchmarkRunnerMode capturedBenchmarkMode;
        private VegetationLayer capturedControlledLayer;
        private int capturedControlledDensity;
        private bool controlledConfigurationChanged;

        private static readonly List<VegetationBenchmarkRunner> ActiveRunnersInternal =
            new List<VegetationBenchmarkRunner>();

        public Transform ScopeRoot => scopeRoot;
        public VegetationBenchmarkRunnerMode BenchmarkMode => benchmarkMode;
        public VegetationLayer ControlledLayer => controlledLayer;
        public bool SuiteRunning => suiteRunning;
        public int SuiteCurrentCase => suiteCurrentCase;
        public int SuiteTotalCases => suiteTotalCases;
        public string SuiteStatus => suiteStatus;
        public bool HasTimedSuiteReport =>
            !string.IsNullOrEmpty(lastTimedSuiteReport);
        public string LastTimedSuiteReport => lastTimedSuiteReport;
        public string LastTimedSuiteReportPath => lastTimedSuiteReportPath;
        public static int ActiveRunnerCount => ActiveRunnersInternal.Count;

        private sealed class TimingResult
        {
            public readonly List<double> CpuSamples = new List<double>(512);
            public readonly List<double> GpuSamples = new List<double>(512);
        }

        private struct LayerState
        {
            public VegetationLayer Layer;
            public bool RenderingEnabled;
            public bool ActiveAndEnabled;
        }

        private struct BenchmarkCase
        {
            public string Label;
            public bool MeasuresAuthoredStack;
            public bool ChangesControlledConfiguration;
            public int Density;
        }

        private static readonly int[] ControlledDensityTiers = { 20, 35, 50 };

        private struct SuiteOutcome
        {
            public string Label;
            public double CpuDelta;
            public double CpuNoise;
            public double GpuDelta;
            public double GpuNoise;
        }

        private void OnEnable()
        {
            if (!ActiveRunnersInternal.Contains(this))
            {
                ActiveRunnersInternal.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveRunnersInternal.Remove(this);
            StopSuiteAndRestore("Runner disabled during benchmark.");
        }

        private void OnDestroy()
        {
            ActiveRunnersInternal.Remove(this);
            StopSuiteAndRestore("Runner destroyed during benchmark.");
        }

        private void OnValidate()
        {
            warmupSeconds = Mathf.Max(0.1f, warmupSeconds);
            measurementSeconds = Mathf.Max(0.25f, measurementSeconds);
            passesPerCase = Mathf.Clamp(passesPerCase, 1, 10);
        }

        public bool CanRunTimedSuite(out string reason)
        {
            reason = string.Empty;
            if (!Application.isPlaying)
            {
                reason = "Timed benchmarking requires Play Mode.";
                return false;
            }
            if (!isActiveAndEnabled)
            {
                reason = "The benchmark runner must be active and enabled.";
                return false;
            }
            if (ActiveRunnerCount != 1)
            {
                reason = "Exactly one active VegetationBenchmarkRunner is required.";
                return false;
            }
            if (suiteRunning)
            {
                reason = "A timed benchmark is already running.";
                return false;
            }

            var layers = new List<VegetationLayer>();
            CollectScopedLayers(layers);
            if (benchmarkMode == VegetationBenchmarkRunnerMode.EnabledStack)
            {
                for (int index = 0; index < layers.Count; index++)
                {
                    VegetationLayer layer = layers[index];
                    if (layer != null && layer.isActiveAndEnabled &&
                        layer.RenderingEnabled)
                    {
                        return true;
                    }
                }

                reason = "Enabled Stack mode requires at least one active, enabled, rendering layer in scope.";
                return false;
            }

            if (controlledLayer == null)
            {
                reason = "Controlled Layer Matrix mode requires a selected layer.";
                return false;
            }
            if (!layers.Contains(controlledLayer))
            {
                reason = "The controlled layer is outside the runner scope.";
                return false;
            }
            if (!controlledLayer.isActiveAndEnabled)
            {
                reason = "The controlled layer must be active and enabled.";
                return false;
            }
            return true;
        }

        public bool BeginTimedSuite()
        {
            if (!CanRunTimedSuite(out string reason))
            {
                suiteStatus = reason;
                return false;
            }

            suiteCoroutine = StartCoroutine(RunTimedSuite());
            return true;
        }

        public string BuildLayerInventoryReport()
        {
            var layers = new List<VegetationLayer>();
            CollectScopedLayers(layers);
            var builder = new StringBuilder(8192);
            builder.AppendLine("[Vegetation FOUNDATION.1 Scene Layer Inventory]");
            AppendRunnerIdentity(builder);
            AppendLayerInventory(builder, layers);
            builder.Append("Timed benchmark mode: ")
                .AppendLine(benchmarkMode.ToString());
            if (benchmarkMode == VegetationBenchmarkRunnerMode.ControlledLayerMatrix)
            {
                builder.Append("Controlled layer: ")
                    .AppendLine(controlledLayer != null
                        ? BuildHierarchyPath(controlledLayer.transform)
                        : "None");
            }
            builder.Append("Timed suite status: ").AppendLine(suiteStatus);
            return builder.ToString();
        }

        private IEnumerator RunTimedSuite()
        {
            var layers = new List<VegetationLayer>();
            CollectScopedLayers(layers);
            CaptureLayerState(layers);
            List<BenchmarkCase> cases = BuildCases();
            var outcomes = new List<SuiteOutcome>(cases.Count);
            var builder = new StringBuilder(131072);
            int successfulCases = 0;
            int failedCases = 0;
            int structuralRebuilds = 0;
            bool completedAllCases = false;
            string restorationResult = "Not attempted";

            suiteRunning = true;
            suiteCurrentCase = 0;
            suiteTotalCases = cases.Count;
            suiteStatus = "Preparing stack-aware benchmark";
            lastTimedSuiteReport = string.Empty;
            lastTimedSuiteReportPath = string.Empty;

            builder.AppendLine("[Vegetation FOUNDATION.1 Stack-Aware Timed Benchmark]");
            AppendRunnerIdentity(builder);
            builder.Append("Mode: ").AppendLine(capturedBenchmarkMode.ToString());
            if (capturedBenchmarkMode ==
                VegetationBenchmarkRunnerMode.ControlledLayerMatrix)
            {
                builder.AppendLine(
                    "Current authored stack case: Included as one exact captured configuration");
                builder.AppendLine(
                    "Production geometry: CrossedCards (fixed)");
                builder.AppendLine(
                    "Standard controlled density tiers: 20 / 35 / 50 clusters/m²");
                builder.Append("Additional exact controlled density: ")
                    .AppendLine(IsStandardControlledCase(capturedControlledDensity)
                        ? "Not required — exact density is already a standard tier"
                        : $"Included — {capturedControlledDensity}/m²");
            }
            builder.Append("Cases: ").AppendLine(cases.Count.ToString());
            builder.Append("Passes per case: ").AppendLine(passesPerCase.ToString());
            builder.Append("Warm-up per window: ")
                .Append(warmupSeconds.ToString("0.###")).AppendLine(" s");
            builder.Append("Measurement per window: ")
                .Append(measurementSeconds.ToString("0.###")).AppendLine(" s");
            builder.Append("Disabled-render baseline: ")
                .AppendLine(interleaveDisabledBaseline ? "Interleaved" : "Disabled");
            builder.Append("Automatic screenshots: ")
                .AppendLine(captureScreenshots ? "Enabled" : "Disabled");
            builder.AppendLine(
                "CPU and GPU values are whole-frame measurements. Reported vegetation deltas subtract adjacent all-scoped-layers-render-disabled baselines and remain estimates.");
            builder.AppendLine(
                "The suite does not claim per-feature shader cost or draw-call-isolated GPU cost.");
            bool performanceRankingValid = IsTargetResolution();
            AppendEnvironmentSummary(builder, performanceRankingValid);
            AppendLayerInventory(builder, layers);
            builder.AppendLine();

            try
            {
                for (int caseIndex = 0; caseIndex < cases.Count; caseIndex++)
                {
                    BenchmarkCase benchmarkCase = cases[caseIndex];
                    suiteCurrentCase = caseIndex + 1;
                    suiteStatus = BuildSuiteStatus(
                        caseIndex,
                        cases.Count,
                        benchmarkCase,
                        "preparing");

                    builder.AppendLine("============================================================");
                    builder.Append("Case ").Append(caseIndex + 1).Append(" / ")
                        .Append(cases.Count).Append(": ")
                        .AppendLine(benchmarkCase.Label);
                    builder.AppendLine("============================================================");

                    if (benchmarkCase.ChangesControlledConfiguration)
                    {
                        if (capturedControlledLayer == null)
                        {
                            failedCases++;
                            builder.AppendLine(
                                "Timed measurement: SKIPPED — controlled layer was destroyed or unloaded during the suite.");
                            builder.AppendLine();
                            yield return null;
                            continue;
                        }

                        capturedControlledLayer.SetDensityPerSquareMetre(benchmarkCase.Density);
                        capturedControlledLayer.RebuildVegetation();
                        controlledConfigurationChanged = true;
                        structuralRebuilds++;
                    }

                    AppendCaseStructure(builder, benchmarkCase);

                    if (!CaseResourcesReady(benchmarkCase, out string buildError))
                    {
                        failedCases++;
                        builder.AppendLine("Timed measurement: SKIPPED — resources not ready");
                        if (!string.IsNullOrEmpty(buildError))
                        {
                            builder.Append("Build error: ").AppendLine(buildError);
                        }
                        builder.AppendLine();
                        yield return null;
                        continue;
                    }

                    var enabledAggregate = new TimingResult();
                    var baselineAggregate = new TimingResult();
                    string screenshotPath = string.Empty;
                    for (int pass = 0; pass < passesPerCase; pass++)
                    {
                        bool baselineFirst = (pass & 1) == 0;
                        if (interleaveDisabledBaseline && baselineFirst)
                        {
                            yield return MeasureWindow(
                                false,
                                caseIndex,
                                cases.Count,
                                benchmarkCase,
                                pass,
                                "baseline",
                                baselineAggregate);
                        }

                        yield return MeasureWindow(
                            true,
                            caseIndex,
                            cases.Count,
                            benchmarkCase,
                            pass,
                            "vegetation",
                            enabledAggregate);

                        if (captureScreenshots && pass == 0)
                        {
                            screenshotPath = CaptureScreenshot(benchmarkCase, caseIndex);
                            yield return new WaitForEndOfFrame();
                            float deadline = Time.realtimeSinceStartup + 2f;
                            while (!IsScreenshotReady(screenshotPath) &&
                                   Time.realtimeSinceStartup < deadline)
                            {
                                yield return null;
                            }
                        }

                        if (interleaveDisabledBaseline && !baselineFirst)
                        {
                            yield return MeasureWindow(
                                false,
                                caseIndex,
                                cases.Count,
                                benchmarkCase,
                                pass,
                                "baseline",
                                baselineAggregate);
                        }
                    }

                    ApplyMeasurementRenderState(true, benchmarkCase);
                    successfulCases++;
                    AppendComparisonTimingSummary(
                        builder,
                        enabledAggregate,
                        interleaveDisabledBaseline ? baselineAggregate : null);
                    if (interleaveDisabledBaseline)
                    {
                        outcomes.Add(BuildSuiteOutcome(
                            benchmarkCase.Label,
                            enabledAggregate,
                            baselineAggregate));
                    }
                    if (!string.IsNullOrEmpty(screenshotPath))
                    {
                        builder.Append("Screenshot: ")
                            .AppendLine(DescribeScreenshot(screenshotPath));
                    }
                    builder.AppendLine();
                }
                completedAllCases = true;
            }
            finally
            {
                restorationResult = RestoreCapturedState();
                builder.AppendLine("============================================================");
                AppendSuiteRanking(
                    builder,
                    outcomes,
                    performanceRankingValid,
                    performanceRankingValid
                        ? string.Empty
                        : $"Expected 2560 × 1440, measured {Screen.width} × {Screen.height}.");
                builder.AppendLine("Suite summary");
                builder.Append("Successful timed cases: ").Append(successfulCases)
                    .Append(" / ").AppendLine(suiteTotalCases.ToString());
                builder.Append("Failed cases: ").Append(failedCases)
                    .Append(" / ").AppendLine(suiteTotalCases.ToString());
                builder.Append("Completed all cases: ")
                    .AppendLine(completedAllCases ? "Yes" : "No — suite interrupted or faulted");
                builder.Append("Controlled-layer structural rebuilds: ")
                    .AppendLine(structuralRebuilds.ToString());
                builder.Append("Restoration: ").AppendLine(restorationResult);

                string reportPath = GetTimedSuiteReportPath();
                builder.Append("Report file: ").AppendLine(reportPath);
                string finalReport = builder.ToString();
                string saveError = TrySaveTimedSuiteReport(reportPath, finalReport);
                if (!string.IsNullOrEmpty(saveError))
                {
                    builder.Append("REPORT SAVE FAILED: ").AppendLine(saveError);
                    finalReport = builder.ToString();
                    lastTimedSuiteReportPath = string.Empty;
                }
                else
                {
                    lastTimedSuiteReportPath = reportPath;
                }

                lastTimedSuiteReport = finalReport;
                bool restorationPassed = restorationResult.StartsWith(
                    "PASS",
                    StringComparison.Ordinal);
                bool reportSaved = string.IsNullOrEmpty(saveError);
                if (completedAllCases && failedCases == 0 && restorationPassed)
                {
                    suiteStatus = reportSaved
                        ? "Complete — report saved and ready to copy"
                        : "Complete — report remains in memory; file save failed";
                }
                else
                {
                    suiteStatus = reportSaved
                        ? "Incomplete or failed — report saved and ready to copy"
                        : "Incomplete or failed — report remains in memory; file save failed";
                }
                suiteRunning = false;
                suiteCoroutine = null;
                Debug.Log(
                    string.IsNullOrEmpty(lastTimedSuiteReportPath)
                        ? "[Vegetation FOUNDATION.1] Stack-aware benchmark completed; report remains in memory but file saving failed."
                        : $"[Vegetation FOUNDATION.1] Stack-aware benchmark completed. Report: {lastTimedSuiteReportPath}",
                    this);
            }
        }

        private IEnumerator MeasureWindow(
            bool vegetationEnabled,
            int caseIndex,
            int caseCount,
            BenchmarkCase benchmarkCase,
            int pass,
            string phase,
            TimingResult aggregate)
        {
            ApplyMeasurementRenderState(vegetationEnabled, benchmarkCase);
            suiteStatus = BuildSuiteStatus(
                caseIndex,
                caseCount,
                benchmarkCase,
                $"pass {pass + 1}/{passesPerCase} {phase} warm-up");

            float warmupEnd = Time.realtimeSinceStartup + warmupSeconds;
            while (Time.realtimeSinceStartup < warmupEnd)
            {
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
            }

            suiteStatus = BuildSuiteStatus(
                caseIndex,
                caseCount,
                benchmarkCase,
                $"pass {pass + 1}/{passesPerCase} {phase} measurement");
            var frameTimings = new FrameTiming[1];
            float measurementEnd = Time.realtimeSinceStartup + measurementSeconds;
            while (Time.realtimeSinceStartup < measurementEnd)
            {
                FrameTimingManager.CaptureFrameTimings();
                yield return null;

                AddFinitePositive(aggregate.CpuSamples, Time.unscaledDeltaTime * 1000.0);
                uint timingCount = FrameTimingManager.GetLatestTimings(1, frameTimings);
                if (timingCount > 0)
                {
                    AddFinitePositive(aggregate.GpuSamples, frameTimings[0].gpuFrameTime);
                }
            }
        }

        private void CaptureLayerState(List<VegetationLayer> layers)
        {
            capturedLayerStates.Clear();
            for (int index = 0; index < layers.Count; index++)
            {
                VegetationLayer layer = layers[index];
                if (layer == null)
                {
                    continue;
                }

                capturedLayerStates.Add(new LayerState
                {
                    Layer = layer,
                    RenderingEnabled = layer.RenderingEnabled,
                    ActiveAndEnabled = layer.isActiveAndEnabled
                });
            }

            capturedBenchmarkMode = benchmarkMode;
            capturedControlledLayer = capturedBenchmarkMode ==
                VegetationBenchmarkRunnerMode.ControlledLayerMatrix
                    ? controlledLayer
                    : null;
            if (capturedControlledLayer != null)
            {
                capturedControlledDensity =
                    capturedControlledLayer.DensityPerSquareMetre;
            }
            controlledConfigurationChanged = false;
        }

        private void ApplyMeasurementRenderState(
            bool vegetationEnabled,
            BenchmarkCase benchmarkCase)
        {
            for (int index = 0; index < capturedLayerStates.Count; index++)
            {
                LayerState state = capturedLayerStates[index];
                if (state.Layer == null)
                {
                    continue;
                }

                bool enabled = false;
                if (vegetationEnabled)
                {
                    enabled = benchmarkCase.MeasuresAuthoredStack
                        ? state.ActiveAndEnabled && state.RenderingEnabled
                        : state.Layer == capturedControlledLayer &&
                          state.Layer.isActiveAndEnabled;
                }
                state.Layer.SetRenderingEnabled(enabled);
            }
        }

        private string RestoreCapturedState()
        {
            if (capturedLayerStates.Count == 0 &&
                capturedControlledLayer == null)
            {
                return "PASS — no captured state remained.";
            }

            var errors = new StringBuilder();
            try
            {
                for (int index = 0; index < capturedLayerStates.Count; index++)
                {
                    LayerState state = capturedLayerStates[index];
                    if (state.Layer != null)
                    {
                        state.Layer.SetRenderingEnabled(state.RenderingEnabled);
                    }
                }

                if (capturedControlledLayer != null &&
                    controlledConfigurationChanged)
                {
                    capturedControlledLayer.SetDensityPerSquareMetre(
                        capturedControlledDensity);
                    capturedControlledLayer.RebuildVegetation();
                    if (!capturedControlledLayer.ResourcesReady)
                    {
                        errors.Append("Controlled layer restoration build failed: ")
                            .Append(capturedControlledLayer.LastBuildError);
                    }
                }
            }
            catch (Exception exception)
            {
                errors.Append(exception);
            }
            finally
            {
                capturedLayerStates.Clear();
                capturedBenchmarkMode = benchmarkMode;
                capturedControlledLayer = null;
                controlledConfigurationChanged = false;
            }

            return errors.Length == 0
                ? "PASS — all renderer flags and the exact controlled density restored."
                : "FAIL — " + errors;
        }

        private void StopSuiteAndRestore(string reason)
        {
            if (suiteCoroutine != null)
            {
                StopCoroutine(suiteCoroutine);
                suiteCoroutine = null;
            }

            if (capturedLayerStates.Count > 0 || capturedControlledLayer != null)
            {
                string restoration = RestoreCapturedState();
                suiteStatus = reason + " " + restoration;
            }
            suiteRunning = false;
        }

        private List<BenchmarkCase> BuildCases()
        {
            var cases = new List<BenchmarkCase>
            {
                new BenchmarkCase
                {
                    Label = "Current Authored Stack — exact captured recipes",
                    MeasuresAuthoredStack = true,
                    ChangesControlledConfiguration = false
                }
            };

            if (capturedBenchmarkMode == VegetationBenchmarkRunnerMode.EnabledStack)
            {
                return cases;
            }

            if (!IsStandardControlledCase(capturedControlledDensity))
            {
                cases.Add(new BenchmarkCase
                {
                    Label = $"{capturedControlledLayer.name}: Current Controlled Density — CrossedCards @ {capturedControlledDensity}/m²",
                    MeasuresAuthoredStack = false,
                    ChangesControlledConfiguration = false,
                    Density = capturedControlledDensity
                });
            }

            for (int densityIndex = 0;
                 densityIndex < ControlledDensityTiers.Length;
                 densityIndex++)
            {
                int density = ControlledDensityTiers[densityIndex];
                cases.Add(new BenchmarkCase
                {
                    Label = $"{capturedControlledLayer.name}: CrossedCards @ {density}/m² — authored coverage",
                    MeasuresAuthoredStack = false,
                    ChangesControlledConfiguration = true,
                    Density = density
                });
            }
            return cases;
        }

        private static bool IsStandardControlledCase(int density)
        {
            for (int index = 0; index < ControlledDensityTiers.Length; index++)
            {
                if (ControlledDensityTiers[index] == density)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CaseResourcesReady(
            BenchmarkCase benchmarkCase,
            out string error)
        {
            error = string.Empty;
            if (!benchmarkCase.MeasuresAuthoredStack)
            {
                if (capturedControlledLayer != null &&
                    capturedControlledLayer.ResourcesReady)
                {
                    return true;
                }
                error = capturedControlledLayer != null
                    ? capturedControlledLayer.LastBuildError
                    : "Controlled layer was lost during the suite.";
                return false;
            }

            bool foundMeasuredLayer = false;
            for (int index = 0; index < capturedLayerStates.Count; index++)
            {
                LayerState state = capturedLayerStates[index];
                if (!state.ActiveAndEnabled || !state.RenderingEnabled ||
                    state.Layer == null)
                {
                    continue;
                }
                foundMeasuredLayer = true;
                if (!state.Layer.ResourcesReady)
                {
                    error = BuildHierarchyPath(state.Layer.transform) + ": " +
                        state.Layer.LastBuildError;
                    return false;
                }
            }
            return foundMeasuredLayer;
        }

        private void AppendCaseStructure(
            StringBuilder builder,
            BenchmarkCase benchmarkCase)
        {
            if (!benchmarkCase.MeasuresAuthoredStack)
            {
                if (capturedControlledLayer == null)
                {
                    builder.AppendLine("Controlled layer: MISSING");
                }
                else
                {
                    AppendLayerStructure(
                        builder,
                        capturedControlledLayer,
                        "Controlled layer");
                }
                return;
            }

            int measuredLayers = 0;
            long totalInstances = 0L;
            long totalTriangles = 0L;
            long totalBufferBytes = 0L;
            for (int index = 0; index < capturedLayerStates.Count; index++)
            {
                LayerState state = capturedLayerStates[index];
                if (state.Layer == null || !state.ActiveAndEnabled ||
                    !state.RenderingEnabled)
                {
                    continue;
                }

                measuredLayers++;
                AppendLayerStructure(
                    builder,
                    state.Layer,
                    "Measured layer " + measuredLayers);
                totalInstances += state.Layer.InstanceCount;
                totalTriangles +=
                    (long)state.Layer.InstanceCount * state.Layer.ClusterTriangleCount;
                totalBufferBytes += state.Layer.InstanceBufferBytes;
            }
            builder.Append("Measured enabled layers: ")
                .AppendLine(measuredLayers.ToString());
            builder.Append("Aggregate instances / triangles / buffers: ")
                .Append(totalInstances.ToString("N0")).Append(" / ")
                .Append(totalTriangles.ToString("N0")).Append(" / ")
                .Append(totalBufferBytes.ToString("N0")).AppendLine(" bytes");
        }

        private static void AppendLayerStructure(
            StringBuilder builder,
            VegetationLayer layer,
            string label)
        {
            long triangles = (long)layer.InstanceCount * layer.ClusterTriangleCount;
            builder.Append(label).Append(": ")
                .AppendLine(BuildHierarchyPath(layer.transform));
            builder.Append("  Geometry / density: CrossedCards / ")
                .Append(layer.DensityPerSquareMetre).AppendLine(" clusters/m²");
            builder.Append("  Placement: ")
                .AppendLine(layer.PlacementDomainSummary);
            builder.Append("  Coverage: ")
                .Append(layer.CoverageResolution).Append("², ")
                .Append((layer.AverageCoverage * 100f).ToString("0.0"))
                .AppendLine("%");
            builder.Append("  Candidates / instances: ")
                .Append(layer.PlacementCandidateCount.ToString("N0")).Append(" / ")
                .AppendLine(layer.InstanceCount.ToString("N0"));
            builder.Append("  Cluster vertices / triangles: ")
                .Append(layer.ClusterVertexCount.ToString("N0")).Append(" / ")
                .AppendLine(layer.ClusterTriangleCount.ToString("N0"));
            builder.Append("  Submitted triangles / buffer: ")
                .Append(triangles.ToString("N0")).Append(" / ")
                .Append(layer.InstanceBufferBytes.ToString("N0"))
                .AppendLine(" bytes");
            builder.Append("  Build duration / ready: ")
                .Append(layer.LastBuildDurationMilliseconds.ToString("0.###"))
                .Append(" ms / ")
                .AppendLine(layer.ResourcesReady ? "Yes" : "No");
            if (!string.IsNullOrEmpty(layer.LastBuildError))
            {
                builder.Append("  Error: ").AppendLine(layer.LastBuildError);
            }
        }

        private void AppendRunnerIdentity(StringBuilder builder)
        {
            builder.Append("Runner: ").AppendLine(name);
            builder.Append("Scene: ").AppendLine(gameObject.scene.name);
            builder.Append("Scope: ")
                .AppendLine(scopeRoot != null
                    ? BuildHierarchyPath(scopeRoot)
                    : "Entire runner scene");
            builder.Append("Active runners: ")
                .AppendLine(ActiveRunnerCount.ToString());
            if (ActiveRunnerCount > 1)
            {
                builder.AppendLine(
                    "WARNING: Multiple active VegetationBenchmarkRunner components exist.");
            }
        }

        private static void AppendLayerInventory(
            StringBuilder builder,
            List<VegetationLayer> layers)
        {
            builder.Append("Discovered layers: ").AppendLine(layers.Count.ToString());
            long enabledInstances = 0L;
            long enabledTriangles = 0L;
            long enabledBufferBytes = 0L;
            int enabledLayerCount = 0;
            for (int index = 0; index < layers.Count; index++)
            {
                VegetationLayer layer = layers[index];
                long submittedTriangles =
                    (long)layer.InstanceCount * layer.ClusterTriangleCount;
                builder.Append(index + 1).Append(". ").AppendLine(layer.name);
                builder.Append("   Path: ")
                    .AppendLine(BuildHierarchyPath(layer.transform));
                builder.Append("   Component active / rendering enabled: ")
                    .Append(layer.isActiveAndEnabled ? "Yes" : "No")
                    .Append(" / ")
                    .AppendLine(layer.RenderingEnabled ? "Yes" : "No");
                builder.Append("   Geometry / density: CrossedCards / ")
                    .Append(layer.DensityPerSquareMetre)
                    .AppendLine(" clusters/m²");
                builder.Append("   Instances / triangles: ")
                    .Append(layer.InstanceCount.ToString("N0")).Append(" / ")
                    .AppendLine(submittedTriangles.ToString("N0"));
                builder.Append("   Instance buffer: ")
                    .Append(layer.InstanceBufferBytes.ToString("N0"))
                    .AppendLine(" bytes");
                builder.Append("   Coverage: ")
                    .Append(layer.CoverageResolution).Append("², ")
                    .Append((layer.AverageCoverage * 100f).ToString("0.0"))
                    .AppendLine("%");
                builder.Append("   Ready: ")
                    .AppendLine(layer.ResourcesReady ? "Yes" : "No");
                if (!string.IsNullOrEmpty(layer.LastBuildError))
                {
                    builder.Append("   Error: ").AppendLine(layer.LastBuildError);
                }

                if (layer.isActiveAndEnabled && layer.RenderingEnabled)
                {
                    enabledLayerCount++;
                    enabledInstances += layer.InstanceCount;
                    enabledTriangles += submittedTriangles;
                    enabledBufferBytes += layer.InstanceBufferBytes;
                }
            }

            builder.Append("Enabled rendering layers: ")
                .AppendLine(enabledLayerCount.ToString());
            builder.Append("Enabled-stack instances: ")
                .AppendLine(enabledInstances.ToString("N0"));
            builder.Append("Enabled-stack submitted triangles: ")
                .AppendLine(enabledTriangles.ToString("N0"));
            builder.Append("Enabled-stack instance buffers: ")
                .Append(enabledBufferBytes.ToString("N0"))
                .AppendLine(" bytes");
        }

        private void CollectScopedLayers(List<VegetationLayer> results)
        {
            results.Clear();
            if (scopeRoot != null)
            {
                VegetationLayer[] scopedLayers =
                    scopeRoot.GetComponentsInChildren<VegetationLayer>(true);
                results.AddRange(scopedLayers);
            }
            else
            {
                VegetationLayer[] sceneLayers =
                    UnityEngine.Object.FindObjectsByType<VegetationLayer>(
                        FindObjectsInactive.Include);
                for (int index = 0; index < sceneLayers.Length; index++)
                {
                    VegetationLayer layer = sceneLayers[index];
                    if (layer != null &&
                        layer.gameObject.scene == gameObject.scene)
                    {
                        results.Add(layer);
                    }
                }
            }

            results.Sort((left, right) => string.CompareOrdinal(
                BuildHierarchyPath(left != null ? left.transform : null),
                BuildHierarchyPath(right != null ? right.transform : null)));
        }

        private static string BuildSuiteStatus(
            int caseIndex,
            int caseCount,
            BenchmarkCase benchmarkCase,
            string phase)
        {
            return $"Case {caseIndex + 1}/{caseCount}: {benchmarkCase.Label} — {phase}";
        }

        private string CaptureScreenshot(
            BenchmarkCase benchmarkCase,
            int caseIndex)
        {
            try
            {
                string directory = Path.GetFullPath(Path.Combine(
                    Application.dataPath,
                    "../Library/VegetationBenchmarkCaptures"));
                Directory.CreateDirectory(directory);
                string safeLabel = SanitizeFileName(benchmarkCase.Label);
                string fileName =
                    $"Vegetation_INFRA2_{caseIndex + 1}_{safeLabel}_{Screen.width}x{Screen.height}.png";
                string path = Path.Combine(directory, fileName);
                ScreenCapture.CaptureScreenshot(path, 1);
                return path;
            }
            catch (Exception exception)
            {
                return "FAILED: " + exception.Message;
            }
        }

        private static string SanitizeFileName(string value)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(value.Length);
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                bool rejected = false;
                for (int invalidIndex = 0; invalidIndex < invalid.Length; invalidIndex++)
                {
                    if (character == invalid[invalidIndex])
                    {
                        rejected = true;
                        break;
                    }
                }
                builder.Append(rejected || char.IsWhiteSpace(character)
                    ? '_'
                    : character);
            }
            return builder.ToString();
        }

        private static bool IsScreenshotReady(string screenshotPath)
        {
            if (string.IsNullOrEmpty(screenshotPath) ||
                screenshotPath.StartsWith("FAILED:", StringComparison.Ordinal))
            {
                return true;
            }

            var file = new FileInfo(screenshotPath);
            return file.Exists && file.Length > 0;
        }

        private static string DescribeScreenshot(string screenshotPath)
        {
            if (string.IsNullOrEmpty(screenshotPath))
            {
                return "Not requested";
            }
            if (screenshotPath.StartsWith("FAILED:", StringComparison.Ordinal))
            {
                return screenshotPath;
            }

            var file = new FileInfo(screenshotPath);
            return file.Exists && file.Length > 0
                ? $"VERIFIED — {file.Length:N0} bytes — {screenshotPath}"
                : $"NOT VERIFIED — file missing or empty — {screenshotPath}";
        }

        private static void AddFinitePositive(List<double> samples, double value)
        {
            if (value > 0.0 && !double.IsNaN(value) && !double.IsInfinity(value))
            {
                samples.Add(value);
            }
        }

        private static SuiteOutcome BuildSuiteOutcome(
            string label,
            TimingResult enabled,
            TimingResult baseline)
        {
            CalculateDelta(
                enabled.CpuSamples,
                baseline.CpuSamples,
                out double cpuDelta,
                out double cpuNoise);
            CalculateDelta(
                enabled.GpuSamples,
                baseline.GpuSamples,
                out double gpuDelta,
                out double gpuNoise);
            return new SuiteOutcome
            {
                Label = label,
                CpuDelta = cpuDelta,
                CpuNoise = cpuNoise,
                GpuDelta = gpuDelta,
                GpuNoise = gpuNoise
            };
        }

        private static void AppendSuiteRanking(
            StringBuilder builder,
            List<SuiteOutcome> outcomes,
            bool performanceRankingValid,
            string invalidReason)
        {
            builder.AppendLine("Confidence-aware case ranking");
            if (outcomes.Count == 0)
            {
                builder.AppendLine("Ranking unavailable: no paired baseline outcomes.");
                builder.AppendLine();
                return;
            }

            if (!performanceRankingValid)
            {
                builder.AppendLine("Performance ranking: INVALID");
                builder.Append("Reason: ").AppendLine(invalidReason);
                builder.AppendLine(
                    "Timing rows are retained as measurements at the actual resolution but are not ordered and cannot select a winner.");
                AppendOutcomeRows(builder, outcomes, false);
                builder.AppendLine(
                    "Verdict: rerun at the required target resolution before using timing to make a performance decision.");
                builder.AppendLine();
                return;
            }

            bool rankByGpu = HasPositiveFiniteDelta(outcomes, true);
            if (!rankByGpu && !HasPositiveFiniteDelta(outcomes, false))
            {
                builder.AppendLine(
                    "Ranking unavailable: no positive enabled-minus-disabled cost delta was measured.");
                AppendOutcomeRows(builder, outcomes, false);
                builder.AppendLine(
                    "Verdict: negative deltas are baseline fluctuation and values within noise cannot select a winner.");
                builder.AppendLine();
                return;
            }

            outcomes.Sort((left, right) => CompareSuiteOutcomes(
                left,
                right,
                rankByGpu));
            builder.Append("Primary ordering: ")
                .AppendLine(rankByGpu
                    ? "positive estimated GPU median cost"
                    : "positive estimated CPU median cost — no positive GPU deltas available");
            bool anySeparated = AppendOutcomeRows(builder, outcomes, true);
            builder.AppendLine(anySeparated
                ? "Verdict: only positive rows separated from observed noise may influence a performance decision."
                : "Verdict: no positive case cost is separated from observed timing noise; do not select a winner from this run.");
            builder.AppendLine();
        }

        private static bool AppendOutcomeRows(
            StringBuilder builder,
            List<SuiteOutcome> outcomes,
            bool numberedRanking)
        {
            bool anySeparated = false;
            for (int index = 0; index < outcomes.Count; index++)
            {
                SuiteOutcome outcome = outcomes[index];
                bool gpuSeparated = IsSeparated(outcome.GpuDelta, outcome.GpuNoise);
                bool cpuSeparated = IsSeparated(outcome.CpuDelta, outcome.CpuNoise);
                bool hasNegativeDelta = IsNegativeFinite(outcome.GpuDelta) ||
                                        IsNegativeFinite(outcome.CpuDelta);
                anySeparated |= gpuSeparated || cpuSeparated;
                if (numberedRanking)
                {
                    builder.Append(index + 1).Append(". ");
                }
                else
                {
                    builder.Append("- ");
                }
                builder.Append(outcome.Label)
                    .Append(" | GPU Δ ").Append(FormatDelta(outcome.GpuDelta))
                    .Append(" ms, noise ").Append(FormatValue(outcome.GpuNoise))
                    .Append(" | CPU Δ ").Append(FormatDelta(outcome.CpuDelta))
                    .Append(" ms, noise ").Append(FormatValue(outcome.CpuNoise))
                    .Append(" | ")
                    .AppendLine(gpuSeparated || cpuSeparated
                        ? "POSITIVE COST SEPARATED IN AT LEAST ONE METRIC"
                        : hasNegativeDelta
                            ? "INCONCLUSIVE — NEGATIVE DELTA / BASELINE FLUCTUATION"
                            : "WITHIN OBSERVED NOISE");
            }
            return anySeparated;
        }

        private static bool HasPositiveFiniteDelta(
            List<SuiteOutcome> outcomes,
            bool useGpu)
        {
            for (int index = 0; index < outcomes.Count; index++)
            {
                double value = useGpu
                    ? outcomes[index].GpuDelta
                    : outcomes[index].CpuDelta;
                if (IsPositiveFinite(value))
                {
                    return true;
                }
            }
            return false;
        }

        private static int CompareSuiteOutcomes(
            SuiteOutcome left,
            SuiteOutcome right,
            bool useGpu)
        {
            double leftValue = useGpu ? left.GpuDelta : left.CpuDelta;
            double rightValue = useGpu ? right.GpuDelta : right.CpuDelta;
            bool leftUsable = IsPositiveFinite(leftValue);
            bool rightUsable = IsPositiveFinite(rightValue);
            if (leftUsable != rightUsable)
            {
                return leftUsable ? -1 : 1;
            }
            if (!leftUsable)
            {
                return string.CompareOrdinal(left.Label, right.Label);
            }
            int valueComparison = leftValue.CompareTo(rightValue);
            return valueComparison != 0
                ? valueComparison
                : string.CompareOrdinal(left.Label, right.Label);
        }

        private static bool IsSeparated(double delta, double noise)
        {
            return IsPositiveFinite(delta) &&
                   !double.IsNaN(noise) &&
                   !double.IsInfinity(noise) &&
                   noise > 0.0 &&
                   delta >= noise;
        }

        private static bool IsPositiveFinite(double value)
        {
            return value > 0.0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static bool IsNegativeFinite(double value)
        {
            return value < 0.0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static void AppendComparisonTimingSummary(
            StringBuilder builder,
            TimingResult enabled,
            TimingResult baseline)
        {
            builder.AppendLine("[Timed Measurement]");
            AppendMetricStatistics(builder, "Vegetation-enabled CPU", enabled.CpuSamples);
            AppendMetricStatistics(builder, "Vegetation-enabled GPU", enabled.GpuSamples);
            if (baseline == null)
            {
                builder.AppendLine("Disabled-render baseline: NOT MEASURED");
                return;
            }

            AppendMetricStatistics(builder, "Render-disabled CPU baseline", baseline.CpuSamples);
            AppendMetricStatistics(builder, "Render-disabled GPU baseline", baseline.GpuSamples);
            AppendDelta(builder, "Estimated vegetation CPU delta", enabled.CpuSamples, baseline.CpuSamples);
            AppendDelta(builder, "Estimated vegetation GPU delta", enabled.GpuSamples, baseline.GpuSamples);
        }

        private static void AppendMetricStatistics(
            StringBuilder builder,
            string label,
            List<double> samples)
        {
            builder.Append(label).Append(" samples: ")
                .AppendLine(samples.Count.ToString("N0"));
            if (samples.Count == 0)
            {
                builder.Append(label).AppendLine(": UNAVAILABLE");
                return;
            }

            CalculateRobustStatistics(
                samples,
                out double average,
                out double median,
                out double percentile95,
                out double minimum,
                out double maximum,
                out double standardDeviation);
            builder.Append(label).Append(" average: ")
                .Append(average.ToString("0.###")).AppendLine(" ms");
            builder.Append(label).Append(" median: ")
                .Append(median.ToString("0.###")).AppendLine(" ms");
            builder.Append(label).Append(" p95: ")
                .Append(percentile95.ToString("0.###")).AppendLine(" ms");
            builder.Append(label).Append(" minimum: ")
                .Append(minimum.ToString("0.###")).AppendLine(" ms");
            builder.Append(label).Append(" maximum: ")
                .Append(maximum.ToString("0.###")).AppendLine(" ms");
            builder.Append(label).Append(" standard deviation: ")
                .Append(standardDeviation.ToString("0.###")).AppendLine(" ms");
        }

        private static void AppendDelta(
            StringBuilder builder,
            string label,
            List<double> enabled,
            List<double> baseline)
        {
            if (enabled.Count == 0 || baseline.Count == 0)
            {
                builder.Append(label).AppendLine(": UNAVAILABLE");
                return;
            }

            CalculateDelta(enabled, baseline, out double delta, out double noiseFloor);
            builder.Append(label).Append(": ")
                .Append(delta.ToString("+0.###;-0.###;0"))
                .AppendLine(" ms (median difference)");
            builder.Append(label).Append(" confidence: ")
                .AppendLine(IsNegativeFinite(delta)
                    ? "INCONCLUSIVE — NEGATIVE DELTA / BASELINE FLUCTUATION"
                    : IsSeparated(delta, noiseFloor)
                        ? "POSITIVE COST SEPARATED FROM OBSERVED NOISE"
                        : "BELOW / WITHIN OBSERVED NOISE");
            builder.Append(label).Append(" combined noise estimate: ")
                .Append(noiseFloor.ToString("0.###")).AppendLine(" ms");
        }

        private static void CalculateDelta(
            List<double> enabled,
            List<double> baseline,
            out double delta,
            out double noiseFloor)
        {
            if (enabled.Count == 0 || baseline.Count == 0)
            {
                delta = double.NaN;
                noiseFloor = double.NaN;
                return;
            }

            CalculateRobustStatistics(
                enabled,
                out _,
                out double enabledMedian,
                out _,
                out _,
                out _,
                out double enabledStdDev);
            CalculateRobustStatistics(
                baseline,
                out _,
                out double baselineMedian,
                out _,
                out _,
                out _,
                out double baselineStdDev);
            delta = enabledMedian - baselineMedian;
            noiseFloor = Math.Sqrt(
                enabledStdDev * enabledStdDev +
                baselineStdDev * baselineStdDev);
        }

        private static void CalculateRobustStatistics(
            List<double> samples,
            out double average,
            out double median,
            out double percentile95,
            out double minimum,
            out double maximum,
            out double standardDeviation)
        {
            var sorted = new List<double>(samples);
            sorted.Sort();
            double sum = 0.0;
            for (int index = 0; index < sorted.Count; index++)
            {
                sum += sorted[index];
            }

            average = sum / sorted.Count;
            minimum = sorted[0];
            maximum = sorted[sorted.Count - 1];
            median = Percentile(sorted, 0.5);
            percentile95 = Percentile(sorted, 0.95);

            double squaredDifferenceSum = 0.0;
            for (int index = 0; index < sorted.Count; index++)
            {
                double difference = sorted[index] - average;
                squaredDifferenceSum += difference * difference;
            }
            standardDeviation = Math.Sqrt(squaredDifferenceSum / sorted.Count);
        }

        private static double Percentile(
            List<double> sortedSamples,
            double percentile)
        {
            if (sortedSamples.Count == 1)
            {
                return sortedSamples[0];
            }

            double position = (sortedSamples.Count - 1) * percentile;
            int lower = (int)Math.Floor(position);
            int upper = (int)Math.Ceiling(position);
            if (lower == upper)
            {
                return sortedSamples[lower];
            }

            double fraction = position - lower;
            return sortedSamples[lower] +
                   (sortedSamples[upper] - sortedSamples[lower]) * fraction;
        }

        private static string FormatDelta(double value)
        {
            return double.IsNaN(value)
                ? "N/A"
                : value.ToString("+0.###;-0.###;0");
        }

        private static string FormatValue(double value)
        {
            return double.IsNaN(value) ? "N/A" : value.ToString("0.###");
        }

        private static bool IsTargetResolution()
        {
            return Screen.width == 2560 && Screen.height == 1440;
        }

        private static void AppendEnvironmentSummary(
            StringBuilder builder,
            bool targetResolutionValid)
        {
            builder.Append("Resolution: ").Append(Screen.width).Append(" × ")
                .Append(Screen.height).AppendLine();
            builder.Append("Graphics API: ")
                .AppendLine(SystemInfo.graphicsDeviceType.ToString());
            builder.Append("GPU: ").AppendLine(SystemInfo.graphicsDeviceName);
            builder.Append("VSync count: ")
                .AppendLine(QualitySettings.vSyncCount.ToString());
            builder.Append("Target frame rate: ")
                .AppendLine(Application.targetFrameRate.ToString());
            builder.Append("Runtime context: ")
                .AppendLine(Application.isEditor
                    ? (Application.isPlaying
                        ? "Unity Editor Play Mode"
                        : "Unity Editor Edit Mode")
                    : "Player build");
            builder.Append("QualitySettings MSAA: ")
                .Append(QualitySettings.antiAliasing).AppendLine("×");
            builder.Append("URP pipeline MSAA: ")
                .AppendLine(ReadPipelineProperty("msaaSampleCount", "Unavailable"));
            builder.Append("Render scale: ")
                .AppendLine(ReadPipelineProperty("renderScale", "Unavailable"));
            if (!targetResolutionValid)
            {
                builder.Append("TARGET RESOLUTION CHECK: FAIL — expected 2560 × 1440, measured ")
                    .Append(Screen.width).Append(" × ").Append(Screen.height)
                    .AppendLine(". Performance ranking is invalid; timing applies only to the measured resolution.");
            }
            else
            {
                builder.AppendLine("Target resolution check: PASS — 2560 × 1440");
            }
        }

        private static string ReadPipelineProperty(
            string propertyName,
            string fallback)
        {
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline == null)
            {
                return fallback;
            }

            var property = pipeline.GetType().GetProperty(propertyName);
            object value = property != null
                ? property.GetValue(pipeline, null)
                : null;
            return value != null ? value.ToString() : fallback;
        }

        private static string GetTimedSuiteReportPath()
        {
            return Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "../Library/VegetationBenchmarkDiagnostics/Vegetation_FOUNDATION1_Stack_Aware_Benchmark_Report.txt"));
        }

        private static string TrySaveTimedSuiteReport(
            string reportPath,
            string report)
        {
            try
            {
                string directory = Path.GetDirectoryName(reportPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(reportPath, report);
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.ToString();
            }
        }

        private static string BuildHierarchyPath(Transform value)
        {
            if (value == null)
            {
                return "None";
            }

            var parts = new List<string>();
            Transform current = value;
            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
