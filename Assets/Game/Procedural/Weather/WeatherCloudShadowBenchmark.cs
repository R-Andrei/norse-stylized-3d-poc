using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Weather
{
    public sealed class WeatherCloudShadowBenchmark : MonoBehaviour
    {
        public readonly struct Settings
        {
            public readonly int WarmupFrames;
            public readonly int MeasurementFrames;
            public readonly int PairedRepetitions;
            public readonly int EvolutionWarmupFrames;
            public readonly float EvolutionTimeoutSeconds;

            public Settings(
                int warmupFrames,
                int measurementFrames,
                int pairedRepetitions,
                int evolutionWarmupFrames,
                float evolutionTimeoutSeconds)
            {
                WarmupFrames = Mathf.Clamp(warmupFrames, 30, 3600);
                MeasurementFrames = Mathf.Clamp(
                    measurementFrames,
                    120,
                    7200);
                PairedRepetitions = Mathf.Clamp(
                    pairedRepetitions,
                    1,
                    5);
                EvolutionWarmupFrames = Mathf.Clamp(
                    evolutionWarmupFrames,
                    30,
                    1800);
                EvolutionTimeoutSeconds = Mathf.Clamp(
                    evolutionTimeoutSeconds,
                    5f,
                    120f);
            }
        }

        private enum CandidateKind
        {
            StaticCookie = 0,
            MovingCookie = 1
        }

        private sealed class MetricSamples
        {
            private readonly double[] values;
            private int count;

            public int Count => count;

            public MetricSamples(int capacity)
            {
                values = new double[Mathf.Max(1, capacity)];
            }

            public void Add(double value)
            {
                if (count >= values.Length ||
                    value <= 0.0 ||
                    double.IsNaN(value) ||
                    double.IsInfinity(value))
                {
                    return;
                }

                values[count++] = value;
            }

            public Statistics Calculate()
            {
                return Statistics.Calculate(values, count);
            }
        }

        private sealed class CounterSamples
        {
            private readonly long[] values;
            private int count;

            public int Count => count;

            public CounterSamples(int capacity)
            {
                values = new long[Mathf.Max(1, capacity)];
            }

            public void Add(long value)
            {
                if (count >= values.Length || value < 0L)
                {
                    return;
                }

                values[count++] = value;
            }

            public CounterStatistics Calculate()
            {
                return CounterStatistics.Calculate(values, count);
            }
        }

        private sealed class MeasurementWindow
        {
            public readonly string Label;
            public readonly CandidateKind? Candidate;
            public readonly bool CloudEnabled;
            public readonly float MovementSpeed;
            public readonly int Repetition;
            public readonly bool Baseline;
            public readonly MetricSamples CpuTotal;
            public readonly MetricSamples CpuMain;
            public readonly MetricSamples CpuRender;
            public readonly MetricSamples Gpu;
            public readonly MetricSamples Wall;
            public readonly CounterSamples GcAllocated;
            public readonly CounterSamples Batches;
            public readonly CounterSamples DrawCalls;
            public readonly CounterSamples SetPassCalls;
            public int RequestedFrames;
            public int CapturedFrames;
            public int MissingFrameTimingFrames;
            public int ExecutionIndex;
            public double ExecutionStartedRealtimeSeconds;
            public double ExecutionStartOffsetSeconds;
            public double ExecutionElapsedSeconds;

            public MeasurementWindow(
                string label,
                CandidateKind? candidate,
                bool cloudEnabled,
                float movementSpeed,
                int repetition,
                bool baseline,
                int capacity)
            {
                Label = label;
                Candidate = candidate;
                CloudEnabled = cloudEnabled;
                MovementSpeed = movementSpeed;
                Repetition = repetition;
                Baseline = baseline;
                CpuTotal = new MetricSamples(capacity);
                CpuMain = new MetricSamples(capacity);
                CpuRender = new MetricSamples(capacity);
                Gpu = new MetricSamples(capacity);
                Wall = new MetricSamples(capacity);
                GcAllocated = new CounterSamples(capacity);
                Batches = new CounterSamples(capacity);
                DrawCalls = new CounterSamples(capacity);
                SetPassCalls = new CounterSamples(capacity);
            }
        }

        private readonly struct Statistics
        {
            public readonly int Count;
            public readonly double Mean;
            public readonly double Median;
            public readonly double Minimum;
            public readonly double Maximum;
            public readonly double P95;
            public readonly double P99;
            public readonly double StandardDeviation;

            private Statistics(
                int count,
                double mean,
                double median,
                double minimum,
                double maximum,
                double p95,
                double p99,
                double standardDeviation)
            {
                Count = count;
                Mean = mean;
                Median = median;
                Minimum = minimum;
                Maximum = maximum;
                P95 = p95;
                P99 = p99;
                StandardDeviation = standardDeviation;
            }

            public static Statistics Calculate(
                double[] source,
                int count)
            {
                if (source == null || count <= 0)
                {
                    return new Statistics(
                        0,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN);
                }

                var sorted = new double[count];
                Array.Copy(source, sorted, count);
                Array.Sort(sorted);
                double sum = 0.0;
                for (int index = 0; index < count; index++)
                {
                    sum += sorted[index];
                }

                double mean = sum / count;
                double squaredDeviationSum = 0.0;
                for (int index = 0; index < count; index++)
                {
                    double deviation = sorted[index] - mean;
                    squaredDeviationSum += deviation * deviation;
                }

                return new Statistics(
                    count,
                    mean,
                    Percentile(sorted, count, 0.5),
                    sorted[0],
                    sorted[count - 1],
                    Percentile(sorted, count, 0.95),
                    Percentile(sorted, count, 0.99),
                    Math.Sqrt(squaredDeviationSum / count));
            }
        }

        private readonly struct CounterStatistics
        {
            public readonly int Count;
            public readonly double Mean;
            public readonly long Median;
            public readonly long Minimum;
            public readonly long Maximum;

            private CounterStatistics(
                int count,
                double mean,
                long median,
                long minimum,
                long maximum)
            {
                Count = count;
                Mean = mean;
                Median = median;
                Minimum = minimum;
                Maximum = maximum;
            }

            public static CounterStatistics Calculate(
                long[] source,
                int count)
            {
                if (source == null || count <= 0)
                {
                    return new CounterStatistics(
                        0,
                        double.NaN,
                        0L,
                        0L,
                        0L);
                }

                var sorted = new long[count];
                Array.Copy(source, sorted, count);
                Array.Sort(sorted);
                double sum = 0.0;
                for (int index = 0; index < count; index++)
                {
                    sum += sorted[index];
                }

                long median = count % 2 == 0
                    ? (long)Math.Round(
                        (sorted[count / 2 - 1] +
                         sorted[count / 2]) * 0.5)
                    : sorted[count / 2];
                return new CounterStatistics(
                    count,
                    sum / count,
                    median,
                    sorted[0],
                    sorted[count - 1]);
            }
        }

        private readonly struct PairOutcome
        {
            public readonly CandidateKind Kind;
            public readonly int Repetition;
            public readonly MeasurementWindow Baseline;
            public readonly MeasurementWindow Candidate;
            public readonly bool CandidateFirst;

            public PairOutcome(
                CandidateKind kind,
                int repetition,
                MeasurementWindow baseline,
                MeasurementWindow candidate,
                bool candidateFirst)
            {
                Kind = kind;
                Repetition = repetition;
                Baseline = baseline;
                Candidate = candidate;
                CandidateFirst = candidateFirst;
            }
        }

        private struct OptionalRecorder : IDisposable
        {
            public string Name;
            public ProfilerRecorder Recorder;
            public bool Available;

            public long ReadLastValue()
            {
                return Available && Recorder.Valid && Recorder.Count > 0
                    ? Recorder.LastValue
                    : -1L;
            }

            public void Dispose()
            {
                if (Recorder.Valid)
                {
                    Recorder.Dispose();
                }

                Available = false;
            }
        }

        private sealed class RecorderSet : IDisposable
        {
            public OptionalRecorder GcAllocated;
            public OptionalRecorder Batches;
            public OptionalRecorder DrawCalls;
            public OptionalRecorder SetPassCalls;

            public RecorderSet()
            {
                GcAllocated = StartRecorder(
                    ProfilerCategory.Memory,
                    "GC Allocated In Frame");
                Batches = StartRecorderWithFallback(
                    ProfilerCategory.Render,
                    "Batches Count",
                    "Total Batches Count");
                DrawCalls = StartRecorder(
                    ProfilerCategory.Render,
                    "Draw Calls Count");
                SetPassCalls = StartRecorder(
                    ProfilerCategory.Render,
                    "SetPass Calls Count");
            }

            public void Dispose()
            {
                GcAllocated.Dispose();
                Batches.Dispose();
                DrawCalls.Dispose();
                SetPassCalls.Dispose();
            }
        }

        private const string RuntimeObjectName =
            "Weather Cloud Shadow Benchmark Runtime";
        private const string ReportFileName =
            "Weather_Cloud_Shadow_V03E2_Benchmark_Report.txt";
        private const int MaximumEvolutionSamples = 20000;

        private static WeatherCloudShadowBenchmark activeRunner;
        private static string lastReport = string.Empty;
        private static string lastReportPath = string.Empty;
        private static string lastStatus = "Not run";
        private static int currentStep;
        private static int totalSteps;
        private static int currentFrame;
        private static int currentFrameTarget;

        private WeatherCloudShadowController controller;
        private Settings settings;
        private Coroutine suiteCoroutine;
        private bool stateCaptured;
        private bool finishing;
        private WeatherCloudShadowController.BenchmarkState capturedState;
        private readonly List<MeasurementWindow> windows =
            new List<MeasurementWindow>(12);
        private readonly List<MeasurementWindow> executionWindows =
            new List<MeasurementWindow>(12);
        private readonly List<PairOutcome> pairOutcomes =
            new List<PairOutcome>(8);
        private RecorderSet recorderSet;
        private readonly FrameTiming[] frameTimings =
            new FrameTiming[1];
        private MeasurementWindow evolutionWindow;
        private MeasurementWindow postEvolutionWindow;
        private double capturedEvolutionPreparationMilliseconds;
        private double capturedEvolutionBlendUploadTotalMilliseconds;
        private double capturedEvolutionBlendUploadMaximumMilliseconds;
        private int capturedEvolutionBlendUploadTimingCount;
        private int capturedEvolutionUploadCount;
        private long capturedEvolutionUploadedTexelBytes;
        private double suiteExecutionStartRealtimeSeconds;
        private string cancellationReason = string.Empty;

        public static bool IsRunning => activeRunner != null;
        public static string LastStatus => lastStatus;
        public static int CurrentStep => currentStep;
        public static int TotalSteps => totalSteps;
        public static int CurrentFrame => currentFrame;
        public static int CurrentFrameTarget => currentFrameTarget;
        public static bool HasReport => !string.IsNullOrEmpty(lastReport);
        public static string LastReport => lastReport;
        public static string LastReportPath => lastReportPath;

        public static float Progress
        {
            get
            {
                if (totalSteps <= 0)
                {
                    return 0f;
                }

                float stepProgress = currentFrameTarget > 0
                    ? Mathf.Clamp01(
                        currentFrame /
                        (float)currentFrameTarget)
                    : 0f;
                return Mathf.Clamp01(
                    (currentStep - 1 + stepProgress) /
                    totalSteps);
            }
        }

        public static bool CanBegin(
            WeatherCloudShadowController targetController,
            out string reason)
        {
            reason = string.Empty;
            if (IsRunning)
            {
                reason = "A cloud-shadow benchmark is already running.";
                return false;
            }

            if (targetController == null)
            {
                reason = "A Weather Cloud Shadow Controller is required.";
                return false;
            }

            return targetController.CanRunPerformanceBenchmark(
                out reason);
        }

        public static bool Begin(
            WeatherCloudShadowController targetController,
            Settings benchmarkSettings,
            out string reason)
        {
            if (!CanBegin(targetController, out reason))
            {
                return false;
            }

            var runtimeObject = new GameObject(RuntimeObjectName)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            WeatherCloudShadowBenchmark runner =
                runtimeObject.AddComponent<WeatherCloudShadowBenchmark>();
            runner.controller = targetController;
            runner.settings = benchmarkSettings;
            activeRunner = runner;
            lastStatus = "Starting complete benchmark";
            currentStep = 0;
            totalSteps = benchmarkSettings.PairedRepetitions * 4 + 2;
            currentFrame = 0;
            currentFrameTarget = 0;
            runner.suiteCoroutine = runner.StartCoroutine(
                runner.RunCompleteSuite());
            return true;
        }

        public static void CancelAndRestore()
        {
            if (activeRunner != null)
            {
                activeRunner.CancelInternal(
                    "Cancelled by user.");
            }
        }

        private void OnDisable()
        {
            if (!finishing && stateCaptured)
            {
                CancelInternal(
                    "Benchmark runner disabled before completion.");
            }
        }

        private void OnDestroy()
        {
            if (!finishing && stateCaptured)
            {
                RestoreCapturedState();
            }

            if (activeRunner == this)
            {
                activeRunner = null;
            }
        }

        private IEnumerator RunCompleteSuite()
        {
            bool completed = false;
            string failure = string.Empty;
            lastReport = string.Empty;
            lastReportPath = string.Empty;
            windows.Clear();
            executionWindows.Clear();
            pairOutcomes.Clear();

            if (!TryInitializeSuite(out failure))
            {
                FinishSuite(false, failure);
                yield break;
            }

            var routineStack = new Stack<IEnumerator>(8);
            routineStack.Push(RunCompleteSuiteBody());
            while (routineStack.Count > 0)
            {
                IEnumerator routine = routineStack.Peek();
                if (!TryAdvanceRoutine(
                        routine,
                        out bool hasNext,
                        out object yieldedValue,
                        out failure))
                {
                    DisposeRoutineStack(routineStack, ref failure);
                    break;
                }

                if (!hasNext)
                {
                    routineStack.Pop();
                    if (!TryDisposeRoutine(routine, ref failure))
                    {
                        DisposeRoutineStack(routineStack, ref failure);
                        break;
                    }

                    continue;
                }

                if (yieldedValue is IEnumerator nestedRoutine)
                {
                    routineStack.Push(nestedRoutine);
                    continue;
                }

                yield return yieldedValue;
            }

            completed = routineStack.Count == 0 &&
                string.IsNullOrEmpty(failure);
            FinishSuite(completed, failure);
        }

        private bool TryInitializeSuite(out string failure)
        {
            try
            {
                capturedState =
                    controller.CapturePerformanceBenchmarkState();
                stateCaptured = true;
                recorderSet = new RecorderSet();
                suiteExecutionStartRealtimeSeconds =
                    Time.realtimeSinceStartupAsDouble;
                failure = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                failure = exception.ToString();
                return false;
            }
        }

        private IEnumerator RunCompleteSuiteBody()
        {
            yield return null;
            for (int repetition = 0;
                 repetition < settings.PairedRepetitions;
                 repetition++)
            {
                bool candidateFirst = (repetition & 1) != 0;
                yield return RunPersistentPair(
                    CandidateKind.StaticCookie,
                    repetition,
                    candidateFirst);
                yield return RunPersistentPair(
                    CandidateKind.MovingCookie,
                    repetition,
                    candidateFirst);
            }

            yield return RunEvolutionWindow();
            yield return RunPostEvolutionWindow();
        }

        private static bool TryAdvanceRoutine(
            IEnumerator routine,
            out bool hasNext,
            out object yieldedValue,
            out string failure)
        {
            try
            {
                hasNext = routine.MoveNext();
                yieldedValue = hasNext
                    ? routine.Current
                    : null;
                failure = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                hasNext = false;
                yieldedValue = null;
                failure = exception.ToString();
                return false;
            }
        }

        private static bool TryDisposeRoutine(
            IEnumerator routine,
            ref string failure)
        {
            if (!(routine is IDisposable disposable))
            {
                return true;
            }

            try
            {
                disposable.Dispose();
                return true;
            }
            catch (Exception exception)
            {
                AppendFailure(
                    ref failure,
                    "Benchmark iterator disposal failed.",
                    exception);
                return false;
            }
        }

        private static void DisposeRoutineStack(
            Stack<IEnumerator> routineStack,
            ref string failure)
        {
            while (routineStack.Count > 0)
            {
                IEnumerator routine = routineStack.Pop();
                if (!(routine is IDisposable disposable))
                {
                    continue;
                }

                try
                {
                    disposable.Dispose();
                }
                catch (Exception exception)
                {
                    AppendFailure(
                        ref failure,
                        "Benchmark iterator disposal failed.",
                        exception);
                }
            }
        }

        private static void AppendFailure(
            ref string failure,
            string context,
            Exception exception)
        {
            string appended = context + Environment.NewLine + exception;
            failure = string.IsNullOrEmpty(failure)
                ? appended
                : failure + Environment.NewLine +
                    Environment.NewLine + appended;
        }

        private IEnumerator RunPersistentPair(
            CandidateKind kind,
            int repetition,
            bool candidateFirst)
        {
            float movementSpeed = kind == CandidateKind.StaticCookie
                ? 0f
                : capturedState.MovementSpeedMetresPerSecond;
            MeasurementWindow baseline = CreatePersistentWindow(
                kind,
                repetition,
                true,
                movementSpeed);
            MeasurementWindow candidate = CreatePersistentWindow(
                kind,
                repetition,
                false,
                movementSpeed);

            if (candidateFirst)
            {
                yield return RunMeasurementWindow(candidate);
                yield return RunMeasurementWindow(baseline);
            }
            else
            {
                yield return RunMeasurementWindow(baseline);
                yield return RunMeasurementWindow(candidate);
            }

            pairOutcomes.Add(new PairOutcome(
                kind,
                repetition,
                baseline,
                candidate,
                candidateFirst));
        }

        private MeasurementWindow CreatePersistentWindow(
            CandidateKind kind,
            int repetition,
            bool baseline,
            float movementSpeed)
        {
            string candidateLabel = kind == CandidateKind.StaticCookie
                ? "Static cookie"
                : "Moving cookie";
            string label = baseline
                ? $"Cloud-cookie-disabled baseline for {candidateLabel}, repetition {repetition + 1}"
                : $"{candidateLabel}, repetition {repetition + 1}";
            var window = new MeasurementWindow(
                label,
                kind,
                !baseline,
                movementSpeed,
                repetition,
                baseline,
                settings.MeasurementFrames);
            windows.Add(window);
            return window;
        }

        private IEnumerator RunMeasurementWindow(
            MeasurementWindow window)
        {
            currentStep++;
            BeginWindowExecution(window);
            controller.ApplyPerformanceBenchmarkCase(
                window.CloudEnabled,
                window.MovementSpeed);
            lastStatus = $"Step {currentStep}/{totalSteps}: {window.Label} — warm-up";
            currentFrame = 0;
            currentFrameTarget = settings.WarmupFrames;
            for (int frame = 0;
                 frame < settings.WarmupFrames;
                 frame++)
            {
                currentFrame = frame + 1;
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
            }

            lastStatus = $"Step {currentStep}/{totalSteps}: {window.Label} — measurement";
            currentFrame = 0;
            currentFrameTarget = settings.MeasurementFrames;
            window.RequestedFrames = settings.MeasurementFrames;
            for (int frame = 0;
                 frame < settings.MeasurementFrames;
                 frame++)
            {
                currentFrame = frame + 1;
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
                CaptureFrameSample(window);
            }

            EndWindowExecution(window);
            yield return null;
        }

        private IEnumerator RunEvolutionWindow()
        {
            currentStep++;
            evolutionWindow = new MeasurementWindow(
                "Active cookie evolution",
                null,
                true,
                capturedState.MovementSpeedMetresPerSecond,
                0,
                false,
                MaximumEvolutionSamples);
            windows.Add(evolutionWindow);
            BeginWindowExecution(evolutionWindow);
            controller.ApplyPerformanceBenchmarkCase(
                true,
                capturedState.MovementSpeedMetresPerSecond);
            lastStatus = $"Step {currentStep}/{totalSteps}: Cookie evolution — warm-up";
            currentFrame = 0;
            currentFrameTarget = settings.EvolutionWarmupFrames;
            for (int frame = 0;
                 frame < settings.EvolutionWarmupFrames;
                 frame++)
            {
                currentFrame = frame + 1;
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
            }

            lastStatus = $"Step {currentStep}/{totalSteps}: Active cookie evolution";
            currentFrame = 0;
            currentFrameTarget = MaximumEvolutionSamples;
            FrameTimingManager.CaptureFrameTimings();
            bool started =
                controller.BeginPerformanceBenchmarkEvolution();
            if (!started)
            {
                throw new InvalidOperationException(
                    "The controller refused to start the benchmark evolution transition.");
            }

            double deadline = Time.realtimeSinceStartupAsDouble +
                settings.EvolutionTimeoutSeconds;
            while ((controller.EvolutionInProgress ||
                    evolutionWindow.CapturedFrames == 0) &&
                   Time.realtimeSinceStartupAsDouble < deadline &&
                   evolutionWindow.CapturedFrames < MaximumEvolutionSamples)
            {
                yield return null;
                CaptureFrameSample(evolutionWindow);
                currentFrame = evolutionWindow.CapturedFrames;
                FrameTimingManager.CaptureFrameTimings();
            }

            if (controller.EvolutionInProgress)
            {
                throw new TimeoutException(
                    $"Cookie evolution did not complete within {settings.EvolutionTimeoutSeconds:0.###} seconds.");
            }

            capturedEvolutionPreparationMilliseconds =
                controller.LastEvolutionPreparationMilliseconds;
            capturedEvolutionBlendUploadTotalMilliseconds =
                controller.EvolutionBlendUploadTotalMilliseconds;
            capturedEvolutionBlendUploadMaximumMilliseconds =
                controller.EvolutionBlendUploadMaximumMilliseconds;
            capturedEvolutionBlendUploadTimingCount =
                controller.EvolutionBlendUploadTimingCount;
            capturedEvolutionUploadCount =
                controller.EvolutionUploadCount;
            capturedEvolutionUploadedTexelBytes =
                controller.EvolutionUploadedTexelBytes;
            evolutionWindow.RequestedFrames =
                evolutionWindow.CapturedFrames;
            currentFrameTarget = Mathf.Max(1, evolutionWindow.CapturedFrames);
            EndWindowExecution(evolutionWindow);
            yield return null;
        }

        private IEnumerator RunPostEvolutionWindow()
        {
            currentStep++;
            postEvolutionWindow = new MeasurementWindow(
                "Post-evolution moving-cookie control",
                CandidateKind.MovingCookie,
                true,
                capturedState.MovementSpeedMetresPerSecond,
                0,
                false,
                settings.MeasurementFrames);
            windows.Add(postEvolutionWindow);
            yield return RunPostEvolutionMeasurement(
                postEvolutionWindow);
        }

        private IEnumerator RunPostEvolutionMeasurement(
            MeasurementWindow window)
        {
            BeginWindowExecution(window);
            controller.ApplyPerformanceBenchmarkCase(
                true,
                capturedState.MovementSpeedMetresPerSecond);
            lastStatus = $"Step {currentStep}/{totalSteps}: {window.Label} — warm-up";
            currentFrame = 0;
            currentFrameTarget = settings.WarmupFrames;
            for (int frame = 0;
                 frame < settings.WarmupFrames;
                 frame++)
            {
                currentFrame = frame + 1;
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
            }

            lastStatus = $"Step {currentStep}/{totalSteps}: {window.Label} — measurement";
            currentFrame = 0;
            currentFrameTarget = settings.MeasurementFrames;
            window.RequestedFrames = settings.MeasurementFrames;
            for (int frame = 0;
                 frame < settings.MeasurementFrames;
                 frame++)
            {
                currentFrame = frame + 1;
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
                CaptureFrameSample(window);
            }

            EndWindowExecution(window);
        }

        private void BeginWindowExecution(MeasurementWindow window)
        {
            window.ExecutionIndex = executionWindows.Count + 1;
            window.ExecutionStartedRealtimeSeconds =
                Time.realtimeSinceStartupAsDouble;
            window.ExecutionStartOffsetSeconds =
                window.ExecutionStartedRealtimeSeconds -
                suiteExecutionStartRealtimeSeconds;
            executionWindows.Add(window);
        }

        private static void EndWindowExecution(MeasurementWindow window)
        {
            window.ExecutionElapsedSeconds = Math.Max(
                0.0,
                Time.realtimeSinceStartupAsDouble -
                    window.ExecutionStartedRealtimeSeconds);
        }

        private void CaptureFrameSample(MeasurementWindow window)
        {
            window.CapturedFrames++;
            window.Wall.Add(Time.unscaledDeltaTime * 1000.0);
            uint timingCount = FrameTimingManager.GetLatestTimings(
                1,
                frameTimings);
            if (timingCount > 0)
            {
                FrameTiming timing = frameTimings[0];
                window.CpuTotal.Add(timing.cpuFrameTime);
                window.CpuMain.Add(timing.cpuMainThreadFrameTime);
                window.CpuRender.Add(timing.cpuRenderThreadFrameTime);
                window.Gpu.Add(timing.gpuFrameTime);
            }
            else
            {
                window.MissingFrameTimingFrames++;
            }

            window.GcAllocated.Add(
                recorderSet.GcAllocated.ReadLastValue());
            window.Batches.Add(
                recorderSet.Batches.ReadLastValue());
            window.DrawCalls.Add(
                recorderSet.DrawCalls.ReadLastValue());
            window.SetPassCalls.Add(
                recorderSet.SetPassCalls.ReadLastValue());
        }

        private void FinishSuite(
            bool completed,
            string failure)
        {
            if (finishing)
            {
                return;
            }

            finishing = true;
            string restorationResult = RestoreCapturedState();
            string report = BuildReport(
                completed,
                failure,
                restorationResult);
            recorderSet?.Dispose();
            recorderSet = null;
            lastReport = report;
            lastReportPath = GetReportPath();
            string saveError = TrySaveReport(
                lastReportPath,
                report);
            bool saved = string.IsNullOrEmpty(saveError);

            if (!saved)
            {
                lastReportPath = string.Empty;
                lastReport += "\n[Report Save Error]\n" + saveError + "\n";
            }

            if (completed && string.IsNullOrEmpty(failure))
            {
                lastStatus = saved
                    ? "Complete — report saved and ready to copy"
                    : "Complete — report retained in memory; save failed";
            }
            else
            {
                lastStatus = saved
                    ? "Incomplete — report saved and ready to copy"
                    : "Incomplete — report retained in memory; save failed";
            }

            currentFrame = currentFrameTarget;
            suiteCoroutine = null;
            stateCaptured = false;
            if (activeRunner == this)
            {
                activeRunner = null;
            }

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
        }

        private string RestoreCapturedState()
        {
            if (!stateCaptured || controller == null)
            {
                return "Not required";
            }

            try
            {
                controller.RestorePerformanceBenchmarkState(
                    capturedState);
                stateCaptured = false;
                return "PASS — captured cloud state restored";
            }
            catch (Exception exception)
            {
                stateCaptured = false;
                return "FAIL — " + exception;
            }
        }

        private void CancelInternal(string reason)
        {
            if (finishing)
            {
                return;
            }

            cancellationReason = reason;
            if (suiteCoroutine != null)
            {
                StopCoroutine(suiteCoroutine);
                suiteCoroutine = null;
            }

            FinishSuite(false, cancellationReason);
        }

        private string BuildReport(
            bool completed,
            string failure,
            string restorationResult)
        {
            var builder = new StringBuilder(131072);
            builder.AppendLine("[Weather Cloud-Shadow V0.3E2 Complete Runtime Benchmark]");
            builder.Append("Generated: ")
                .AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            builder.Append("Outcome: ")
                .AppendLine(completed && string.IsNullOrEmpty(failure)
                    ? "COMPLETE"
                    : "INCOMPLETE");
            builder.Append("Restoration: ")
                .AppendLine(restorationResult);
            if (!string.IsNullOrEmpty(failure))
            {
                builder.AppendLine("Failure / cancellation:");
                builder.AppendLine(failure);
            }

            builder.AppendLine();
            AppendEnvironment(builder);
            AppendConfiguration(builder);
            AppendRecorderAvailability(builder);
            AppendPersistentPairs(builder);
            AppendEvolution(builder);
            AppendAllWindows(builder);
            builder.AppendLine("[Interpretation Contract]");
            builder.AppendLine(
                "Persistent deltas are whole-frame candidate-minus-adjacent-baseline measurements for the complete native directional-cookie path.");
            builder.AppendLine(
                "They do not claim direct isolation of one shader sample. Negative deltas and deltas smaller than baseline variation are inconclusive.");
            builder.AppendLine(
                "The transient benchmark runner and optional profiler counters are active in every persistent window, so their common overhead is present on both sides of each pair.");
            builder.AppendLine(
                "Execution indices, start offsets, and elapsed times describe the order in which windows actually ran; paired summaries retain baseline-minus-candidate semantics regardless of execution order.");
            builder.AppendLine(
                "Editor Play Mode results are comparative development evidence. Final acceptance should also use a representative Player build at the target resolution and quality settings.");
            return builder.ToString();
        }

        private void AppendEnvironment(StringBuilder builder)
        {
            builder.AppendLine("[Environment]");
            builder.Append("Runtime context: ")
                .AppendLine(Application.isEditor
                    ? "Unity Editor Play Mode"
                    : "Player build");
            builder.Append("Resolution: ")
                .Append(Screen.width)
                .Append(" × ")
                .AppendLine(Screen.height.ToString());
            builder.Append("Graphics API: ")
                .AppendLine(SystemInfo.graphicsDeviceType.ToString());
            builder.Append("GPU: ")
                .AppendLine(SystemInfo.graphicsDeviceName);
            builder.Append("Graphics memory: ")
                .Append(SystemInfo.graphicsMemorySize)
                .AppendLine(" MB");
            builder.Append("VSync count: ")
                .AppendLine(QualitySettings.vSyncCount.ToString());
            builder.Append("Target frame rate: ")
                .AppendLine(Application.targetFrameRate.ToString());
            builder.Append("Time scale: ")
                .AppendLine(Time.timeScale.ToString("0.###"));
            builder.Append("Quality level: ")
                .AppendLine(QualitySettings.names[QualitySettings.GetQualityLevel()]);
            builder.Append("Active render pipeline: ")
                .AppendLine(
                    GraphicsSettings.currentRenderPipeline != null
                        ? GraphicsSettings.currentRenderPipeline.name
                        : "Built-in / None");
            builder.AppendLine();
        }

        private void AppendConfiguration(StringBuilder builder)
        {
            builder.AppendLine("[Suite Configuration]");
            builder.Append("Warm-up frames per persistent window: ")
                .AppendLine(settings.WarmupFrames.ToString());
            builder.Append("Measured frames per persistent window: ")
                .AppendLine(settings.MeasurementFrames.ToString());
            builder.Append("Paired repetitions: ")
                .AppendLine(settings.PairedRepetitions.ToString());
            builder.Append("Evolution warm-up frames: ")
                .AppendLine(settings.EvolutionWarmupFrames.ToString());
            builder.Append("Evolution timeout: ")
                .Append(settings.EvolutionTimeoutSeconds.ToString("0.###"))
                .AppendLine(" s");
            builder.Append("Captured ordinary movement speed: ")
                .Append(capturedState.MovementSpeedMetresPerSecond.ToString("0.###"))
                .AppendLine(" m/s");
            builder.Append("Cookie resolution / period: ")
                .Append(controller != null
                    ? controller.CookieResolution.ToString()
                    : "Unknown")
                .Append("² / ")
                .Append(controller != null
                    ? controller.CookieWorldSizeMetres.ToString("0.###")
                    : "Unknown")
                .AppendLine(" m");
            builder.AppendLine(
                "Persistent execution order alternates by repetition: baseline-first on odd repetitions, candidate-first on even repetitions.");
            builder.AppendLine(
                "Detailed windows are printed in actual execution order, not baseline/candidate presentation order.");
            builder.AppendLine();
        }

        private void AppendRecorderAvailability(StringBuilder builder)
        {
            builder.AppendLine("[Counter Availability]");
            AppendRecorderAvailabilityRow(
                builder,
                "GC allocated in frame",
                recorderSet != null
                    ? recorderSet.GcAllocated
                    : default);
            AppendRecorderAvailabilityRow(
                builder,
                "Batches",
                recorderSet != null
                    ? recorderSet.Batches
                    : default);
            AppendRecorderAvailabilityRow(
                builder,
                "Draw calls",
                recorderSet != null
                    ? recorderSet.DrawCalls
                    : default);
            AppendRecorderAvailabilityRow(
                builder,
                "SetPass calls",
                recorderSet != null
                    ? recorderSet.SetPassCalls
                    : default);
            builder.AppendLine();
        }

        private static void AppendRecorderAvailabilityRow(
            StringBuilder builder,
            string label,
            OptionalRecorder recorder)
        {
            builder.Append(label)
                .Append(": ")
                .AppendLine(recorder.Available
                    ? $"Available — {recorder.Name}"
                    : "Unavailable");
        }

        private void AppendPersistentPairs(StringBuilder builder)
        {
            builder.AppendLine("[Paired Persistent Cost]");
            AppendCandidatePairGroup(
                builder,
                CandidateKind.StaticCookie);
            AppendCandidatePairGroup(
                builder,
                CandidateKind.MovingCookie);
            builder.AppendLine();
        }

        private void AppendCandidatePairGroup(
            StringBuilder builder,
            CandidateKind kind)
        {
            string label = kind == CandidateKind.StaticCookie
                ? "Static cookie"
                : "Moving cookie";
            builder.AppendLine(label);
            double cpuMedianDeltaSum = 0.0;
            double gpuMedianDeltaSum = 0.0;
            int cpuDeltaCount = 0;
            int gpuDeltaCount = 0;
            for (int index = 0; index < pairOutcomes.Count; index++)
            {
                PairOutcome pair = pairOutcomes[index];
                if (pair.Kind != kind)
                {
                    continue;
                }

                Statistics baselineCpu =
                    pair.Baseline.CpuTotal.Calculate();
                Statistics candidateCpu =
                    pair.Candidate.CpuTotal.Calculate();
                Statistics baselineGpu =
                    pair.Baseline.Gpu.Calculate();
                Statistics candidateGpu =
                    pair.Candidate.Gpu.Calculate();
                double cpuDelta = ResolveDelta(
                    candidateCpu.Median,
                    baselineCpu.Median);
                double gpuDelta = ResolveDelta(
                    candidateGpu.Median,
                    baselineGpu.Median);
                if (!double.IsNaN(cpuDelta))
                {
                    cpuMedianDeltaSum += cpuDelta;
                    cpuDeltaCount++;
                }
                if (!double.IsNaN(gpuDelta))
                {
                    gpuMedianDeltaSum += gpuDelta;
                    gpuDeltaCount++;
                }

                MeasurementWindow firstWindow = pair.CandidateFirst
                    ? pair.Candidate
                    : pair.Baseline;
                MeasurementWindow secondWindow = pair.CandidateFirst
                    ? pair.Baseline
                    : pair.Candidate;
                double pairElapsedSeconds = Math.Max(
                        pair.Baseline.ExecutionStartedRealtimeSeconds +
                        pair.Baseline.ExecutionElapsedSeconds,
                        pair.Candidate.ExecutionStartedRealtimeSeconds +
                        pair.Candidate.ExecutionElapsedSeconds) -
                    Math.Min(
                        pair.Baseline.ExecutionStartedRealtimeSeconds,
                        pair.Candidate.ExecutionStartedRealtimeSeconds);

                builder.Append("  Repetition ")
                    .Append(pair.Repetition + 1)
                    .Append(" | order ")
                    .Append(pair.CandidateFirst
                        ? "candidate → baseline"
                        : "baseline → candidate")
                    .Append(" | execution #")
                    .Append(firstWindow.ExecutionIndex)
                    .Append(" → #")
                    .Append(secondWindow.ExecutionIndex)
                    .Append(" | pair elapsed ")
                    .Append(pairElapsedSeconds.ToString("0.###"))
                    .Append(" s | CPU median Δ ")
                    .Append(FormatSigned(cpuDelta))
                    .Append(" ms | GPU median Δ ")
                    .Append(FormatSigned(gpuDelta))
                    .Append(" ms | baseline/candidate GPU samples ")
                    .Append(baselineGpu.Count)
                    .Append("/")
                    .AppendLine(candidateGpu.Count.ToString());
            }

            builder.Append("  Mean paired CPU median delta: ")
                .AppendLine(cpuDeltaCount > 0
                    ? FormatSigned(cpuMedianDeltaSum / cpuDeltaCount) + " ms"
                    : "Unavailable");
            builder.Append("  Mean paired GPU median delta: ")
                .AppendLine(gpuDeltaCount > 0
                    ? FormatSigned(gpuMedianDeltaSum / gpuDeltaCount) + " ms"
                    : "Unavailable");
        }

        private void AppendEvolution(StringBuilder builder)
        {
            builder.AppendLine("[Evolution Transition]");
            builder.Append("Controller preparation CPU time: ")
                .Append(capturedEvolutionPreparationMilliseconds.ToString("0.###"))
                .AppendLine(" ms");
            builder.Append("Blend/upload timed updates: ")
                .AppendLine(capturedEvolutionBlendUploadTimingCount.ToString());
            builder.Append("Blend/upload CPU total / maximum: ")
                .Append(capturedEvolutionBlendUploadTotalMilliseconds.ToString("0.###"))
                .Append(" / ")
                .Append(capturedEvolutionBlendUploadMaximumMilliseconds.ToString("0.###"))
                .AppendLine(" ms");
            builder.Append("Texture uploads / raw texel bytes: ")
                .Append(capturedEvolutionUploadCount)
                .Append(" / ")
                .AppendLine(capturedEvolutionUploadedTexelBytes.ToString("N0"));
            if (evolutionWindow != null)
            {
                AppendWindowTimingSummary(builder, evolutionWindow, "  ");
            }
            if (postEvolutionWindow != null)
            {
                builder.AppendLine("Post-evolution moving-cookie control:");
                AppendWindowTimingSummary(
                    builder,
                    postEvolutionWindow,
                    "  ");
            }
            builder.AppendLine();
        }

        private void AppendAllWindows(StringBuilder builder)
        {
            builder.AppendLine(
                "[All Measurement Windows — Actual Execution Order]");
            for (int index = 0; index < executionWindows.Count; index++)
            {
                MeasurementWindow window = executionWindows[index];
                builder.Append(window.ExecutionIndex)
                    .Append(". ")
                    .AppendLine(window.Label);
                builder.Append("   Started: +")
                    .Append(window.ExecutionStartOffsetSeconds.ToString("0.###"))
                    .Append(" s | elapsed including warm-up: ")
                    .Append(window.ExecutionElapsedSeconds.ToString("0.###"))
                    .AppendLine(" s");
                AppendWindowTimingSummary(builder, window, "   ");
                AppendWindowCounterSummary(builder, window, "   ");
            }
            builder.AppendLine();
        }

        private static void AppendWindowTimingSummary(
            StringBuilder builder,
            MeasurementWindow window,
            string indent)
        {
            builder.Append(indent)
                .Append("Frames requested/captured/missing timing: ")
                .Append(window.RequestedFrames)
                .Append(" / ")
                .Append(window.CapturedFrames)
                .Append(" / ")
                .AppendLine(window.MissingFrameTimingFrames.ToString());
            AppendStatistics(
                builder,
                indent,
                "CPU total",
                window.CpuTotal.Calculate(),
                "ms");
            AppendStatistics(
                builder,
                indent,
                "CPU main",
                window.CpuMain.Calculate(),
                "ms");
            AppendStatistics(
                builder,
                indent,
                "CPU render",
                window.CpuRender.Calculate(),
                "ms");
            AppendStatistics(
                builder,
                indent,
                "GPU",
                window.Gpu.Calculate(),
                "ms");
            AppendStatistics(
                builder,
                indent,
                "Wall delta",
                window.Wall.Calculate(),
                "ms");
        }

        private static void AppendStatistics(
            StringBuilder builder,
            string indent,
            string label,
            Statistics statistics,
            string unit)
        {
            builder.Append(indent).Append(label).Append(": ");
            if (statistics.Count <= 0)
            {
                builder.AppendLine("Unavailable");
                return;
            }

            builder.Append("n=").Append(statistics.Count)
                .Append(" | mean ").Append(statistics.Mean.ToString("0.###"))
                .Append(" | median ").Append(statistics.Median.ToString("0.###"))
                .Append(" | min ").Append(statistics.Minimum.ToString("0.###"))
                .Append(" | max ").Append(statistics.Maximum.ToString("0.###"))
                .Append(" | p95 ").Append(statistics.P95.ToString("0.###"))
                .Append(" | p99 ").Append(statistics.P99.ToString("0.###"))
                .Append(" | std ").Append(statistics.StandardDeviation.ToString("0.###"))
                .Append(' ')
                .AppendLine(unit);
        }

        private static void AppendWindowCounterSummary(
            StringBuilder builder,
            MeasurementWindow window,
            string indent)
        {
            AppendCounterStatistics(
                builder,
                indent,
                "GC allocated",
                window.GcAllocated.Calculate(),
                "bytes");
            AppendCounterStatistics(
                builder,
                indent,
                "Batches",
                window.Batches.Calculate(),
                "count");
            AppendCounterStatistics(
                builder,
                indent,
                "Draw calls",
                window.DrawCalls.Calculate(),
                "count");
            AppendCounterStatistics(
                builder,
                indent,
                "SetPass calls",
                window.SetPassCalls.Calculate(),
                "count");
        }

        private static void AppendCounterStatistics(
            StringBuilder builder,
            string indent,
            string label,
            CounterStatistics statistics,
            string unit)
        {
            builder.Append(indent).Append(label).Append(": ");
            if (statistics.Count <= 0)
            {
                builder.AppendLine("Unavailable");
                return;
            }

            builder.Append("n=").Append(statistics.Count)
                .Append(" | mean ").Append(statistics.Mean.ToString("0.###"))
                .Append(" | median ").Append(statistics.Median.ToString("N0"))
                .Append(" | min ").Append(statistics.Minimum.ToString("N0"))
                .Append(" | max ").Append(statistics.Maximum.ToString("N0"))
                .Append(' ')
                .AppendLine(unit);
        }

        private static OptionalRecorder StartRecorder(
            ProfilerCategory category,
            string name)
        {
            var result = new OptionalRecorder
            {
                Name = name
            };
            try
            {
                result.Recorder = ProfilerRecorder.StartNew(
                    category,
                    name,
                    1);
                result.Available = result.Recorder.Valid;
            }
            catch
            {
                result.Available = false;
            }
            return result;
        }

        private static OptionalRecorder StartRecorderWithFallback(
            ProfilerCategory category,
            string primaryName,
            string fallbackName)
        {
            OptionalRecorder primary = StartRecorder(
                category,
                primaryName);
            if (primary.Available)
            {
                return primary;
            }

            primary.Dispose();
            return StartRecorder(category, fallbackName);
        }

        private static double Percentile(
            double[] sorted,
            int count,
            double percentile)
        {
            if (count <= 0)
            {
                return double.NaN;
            }

            double position = (count - 1) * percentile;
            int lower = Mathf.FloorToInt((float)position);
            int upper = Mathf.CeilToInt((float)position);
            if (lower == upper)
            {
                return sorted[lower];
            }

            double fraction = position - lower;
            return sorted[lower] +
                (sorted[upper] - sorted[lower]) * fraction;
        }

        private static double ResolveDelta(
            double candidate,
            double baseline)
        {
            return double.IsNaN(candidate) || double.IsNaN(baseline)
                ? double.NaN
                : candidate - baseline;
        }

        private static string FormatSigned(double value)
        {
            return double.IsNaN(value)
                ? "Unavailable"
                : value.ToString("+0.###;-0.###;0");
        }

        private static string GetReportPath()
        {
            if (Application.isEditor)
            {
                return Path.GetFullPath(Path.Combine(
                    Application.dataPath,
                    "../Library/WeatherCloudShadowBenchmarkDiagnostics",
                    ReportFileName));
            }

            return Path.Combine(
                Application.persistentDataPath,
                ReportFileName);
        }

        private static string TrySaveReport(
            string path,
            string report)
        {
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(path, report);
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.ToString();
            }
        }
    }
}
