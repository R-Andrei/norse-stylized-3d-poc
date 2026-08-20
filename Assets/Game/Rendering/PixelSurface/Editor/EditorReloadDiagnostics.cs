using System;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    internal static class EditorReloadDiagnostics
    {
        private const string SessionPrefix =
            "PS3D.EditorReloadDiagnostics.R1.";
        private const string CaptureActiveKey = SessionPrefix + "Active";
        private const string CaptureIdKey = SessionPrefix + "CaptureId";
        private const string CaptureStartTicksKey = SessionPrefix + "StartTicks";
        private const string TriggerKey = SessionPrefix + "Trigger";
        private const string TimelineKey = SessionPrefix + "Timeline";
        private const string LastReportKey = SessionPrefix + "LastReport";
        private const string BeforeReloadSeenKey = SessionPrefix + "BeforeReloadSeen";
        private const string AfterReloadSeenKey = SessionPrefix + "AfterReloadSeen";
        private const string CompilationErrorsKey = SessionPrefix + "CompilationErrors";
        private const string RepairCountKey = SessionPrefix + "RepairCount";
        private const string RepairScheduleCountKey = SessionPrefix + "RepairScheduleCount";
        private const string PendingRepairCountKey = SessionPrefix + "PendingRepairCount";
        private const string RequirePlayModeKey = SessionPrefix + "RequirePlayMode";
        private const string EnteredPlayModeSeenKey = SessionPrefix + "EnteredPlayModeSeen";
        private const string AfterReloadTicksKey = SessionPrefix + "AfterReloadTicks";

        private const double QuietSecondsBeforeFinalize = 0.75;
        private const double PostReloadSafetyTimeoutSeconds = 180.0;
        private const double ObservationHeartbeatSeconds = 10.0;
        private const double EditorUpdateGapThresholdSeconds = 1.0;

        private static int postReloadUpdateCount;
        private static long lastActivityUtcTicks;
        private static long lastObservationHeartbeatUtcTicks;
        private static long lastEditorUpdateUtcTicks;
        private static bool postReloadSamplingActive;
        private static bool firstDelayCallRecorded;
        private static bool repairSeenThisDomain;
        private static bool repairScheduleSeenThisDomain;
        private static bool captureActive =
            SessionState.GetBool(CaptureActiveKey, false);
        private static long captureStartUtcTicks =
            GetLong(CaptureStartTicksKey);
        private static StringBuilder timelineBuilder;

        internal static bool IsCaptureActive => captureActive;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            captureActive = SessionState.GetBool(CaptureActiveKey, false);
            captureStartUtcTicks = GetLong(CaptureStartTicksKey);
            AssemblyReloadEvents.beforeAssemblyReload -=
                OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload +=
                OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -=
                OnAfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload +=
                OnAfterAssemblyReload;

            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished -=
                OnAssemblyCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished +=
                OnAssemblyCompilationFinished;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (!IsCaptureActive)
            {
                return;
            }

            RecordEvent(
                "Reload",
                "R1 InitializeOnLoad diagnostics initialized in the current domain.");
            StartPostReloadSampling();
        }

        [MenuItem(
            "Tools/PS3D/Diagnostics/Editor Reload/Arm Next Incremental Reload + Play Capture")]
        private static void ArmNextReloadCapture()
        {
            ArmCapture("NaturalIncrementalReloadThenPlay", true);
            Debug.Log(
                "[Editor Reload Diagnostic] Armed for one representative incremental code edit, assembly reload, and Play Mode entry. After the reload settles, enter Play Mode promptly. The completed report will be copied to the clipboard and logged once.");
        }

        [MenuItem(
            "Tools/PS3D/Diagnostics/Editor Reload/Force Clean Script Recompile + Capture (Stress Test)")]
        private static void ForceScriptRecompileCapture()
        {
            ArmCapture("ForcedCleanBuildCacheCompilation", false);
            RecordEvent(
                "Trigger",
                "Requesting script compilation with CleanBuildCache. This guarantees a diagnostic compile/reload but is not an incremental-compilation performance baseline.");
            CompilationPipeline.RequestScriptCompilation(
                RequestScriptCompilationOptions.CleanBuildCache);
        }

        [MenuItem(
            "Tools/PS3D/Diagnostics/Editor Reload/Copy Last Reload Timeline Report")]
        private static void CopyLastReloadTimelineReport()
        {
            string report = SessionState.GetString(LastReportKey, string.Empty);
            if (string.IsNullOrWhiteSpace(report))
            {
                Debug.LogWarning(
                    "[Editor Reload Diagnostic] No completed R1 report is available in this Editor session.");
                return;
            }

            EditorGUIUtility.systemCopyBuffer = report;
            Debug.Log(
                "[Editor Reload Diagnostic] Copied the last completed R1 report to the clipboard.");
        }

        internal static long BeginTiming()
        {
            return IsCaptureActive ? Stopwatch.GetTimestamp() : 0L;
        }

        internal static void RecordTimedStage(
            string stage,
            long startTimestamp,
            string details = null)
        {
            if (!IsCaptureActive || startTimestamp == 0L)
            {
                return;
            }

            double milliseconds =
                (Stopwatch.GetTimestamp() - startTimestamp) *
                1000.0 /
                Stopwatch.Frequency;
            RecordEvent(
                "PixelSurface",
                $"{stage} {milliseconds:F3} ms" +
                (string.IsNullOrWhiteSpace(details)
                    ? string.Empty
                    : " | " + details));
        }

        internal static void RecordPixelSurfaceSchedule(
            string reason,
            bool alreadyScheduled,
            bool buildInProgress,
            bool isCompiling)
        {
            if (!IsCaptureActive)
            {
                return;
            }

            int count = SessionState.GetInt(RepairScheduleCountKey, 0) + 1;
            SessionState.SetInt(RepairScheduleCountKey, count);
            repairScheduleSeenThisDomain = true;
            int pending = SessionState.GetInt(PendingRepairCountKey, 0);
            if (!alreadyScheduled)
            {
                pending++;
            }
            else if (pending <= 0)
            {
                pending = 1;
            }

            SessionState.SetInt(PendingRepairCountKey, pending);
            RecordEvent(
                "PixelSurface",
                $"Repair schedule request #{count}: reason={reason}, " +
                $"alreadyScheduled={alreadyScheduled}, " +
                $"buildInProgress={buildInProgress}, compiling={isCompiling}, " +
                $"pendingDelayedRepairs={pending}.");
        }

        internal static int RecordPixelSurfaceRepairStart()
        {
            if (!IsCaptureActive)
            {
                return 0;
            }

            int invocation = SessionState.GetInt(RepairCountKey, 0) + 1;
            SessionState.SetInt(RepairCountKey, invocation);
            repairSeenThisDomain = true;
            int pending = Math.Max(
                0,
                SessionState.GetInt(PendingRepairCountKey, 0) - 1);
            SessionState.SetInt(PendingRepairCountKey, pending);
            RecordEvent(
                "PixelSurface",
                $"RepairAllLibraries invocation #{invocation} START; " +
                $"pendingDelayedRepairs={pending}.");
            return invocation;
        }

        internal static void RecordPixelSurfaceRepairEnd(
            int invocation,
            long startTimestamp,
            string outcome)
        {
            if (!IsCaptureActive)
            {
                return;
            }

            RecordTimedStage(
                $"RepairAllLibraries invocation #{invocation} TOTAL",
                startTimestamp,
                outcome);
            RecordEvent(
                "PixelSurface",
                $"RepairAllLibraries invocation #{invocation} END | {outcome}.");
        }

        internal static void RecordEvent(string category, string message)
        {
            AppendTimelineEvent(category, message, true);
        }

        private static void RecordObservationEvent(
            string category,
            string message)
        {
            AppendTimelineEvent(category, message, false);
        }

        private static void AppendTimelineEvent(
            string category,
            string message,
            bool meaningfulActivity)
        {
            if (!IsCaptureActive)
            {
                return;
            }

            long nowTicks = DateTime.UtcNow.Ticks;
            if (captureStartUtcTicks <= 0L)
            {
                captureStartUtcTicks = nowTicks;
                SetLong(CaptureStartTicksKey, captureStartUtcTicks);
            }

            double elapsedSeconds =
                TimeSpan.FromTicks(
                    Math.Max(0L, nowTicks - captureStartUtcTicks))
                    .TotalSeconds;
            string safeMessage = (message ?? string.Empty)
                .Replace('\r', ' ')
                .Replace('\n', ' ');
            string line =
                $"+{elapsedSeconds,9:F3}s | editor={EditorApplication.timeSinceStartup,12:F6}s | " +
                $"{category} | {safeMessage}";
            EnsureTimelineLoaded();
            if (timelineBuilder.Length > 0)
            {
                timelineBuilder.Append('\n');
            }

            timelineBuilder.Append(line);
            if (meaningfulActivity)
            {
                lastActivityUtcTicks = nowTicks;
            }
        }

        private static void ArmCapture(string trigger, bool requirePlayMode)
        {
            StopPostReloadSampling();
            string captureId = Guid.NewGuid().ToString("N").Substring(0, 8);
            long nowTicks = DateTime.UtcNow.Ticks;
            captureActive = true;
            captureStartUtcTicks = nowTicks;
            timelineBuilder = new StringBuilder(4096);
            SessionState.SetBool(CaptureActiveKey, true);
            SessionState.SetString(CaptureIdKey, captureId);
            SetLong(CaptureStartTicksKey, nowTicks);
            SessionState.SetString(TriggerKey, trigger ?? "Unknown");
            SessionState.SetString(TimelineKey, string.Empty);
            SessionState.SetBool(BeforeReloadSeenKey, false);
            SessionState.SetBool(AfterReloadSeenKey, false);
            SessionState.SetInt(CompilationErrorsKey, 0);
            SessionState.SetInt(RepairCountKey, 0);
            SessionState.SetInt(RepairScheduleCountKey, 0);
            SessionState.SetInt(PendingRepairCountKey, 0);
            SessionState.SetBool(RequirePlayModeKey, requirePlayMode);
            SessionState.SetBool(EnteredPlayModeSeenKey, false);
            SetLong(AfterReloadTicksKey, 0L);
            postReloadUpdateCount = 0;
            firstDelayCallRecorded = false;
            repairSeenThisDomain = false;
            repairScheduleSeenThisDomain = false;
            lastActivityUtcTicks = nowTicks;
            lastObservationHeartbeatUtcTicks = nowTicks;
            lastEditorUpdateUtcTicks = 0L;
            RecordEvent(
                "Capture",
                $"R1 capture {captureId} armed; trigger={trigger}.");
        }

        private static void OnCompilationStarted(object context)
        {
            RecordEvent(
                "Compilation",
                "Compilation event notification START; timestamp is not an authoritative compiler-duration boundary.");
        }

        private static void OnCompilationFinished(object context)
        {
            RecordEvent(
                "Compilation",
                $"Compilation event notification FINISH; errors={SessionState.GetInt(CompilationErrorsKey, 0)}; timestamp is not an authoritative compiler-duration boundary.");
            if (IsCaptureActive &&
                SessionState.GetInt(CompilationErrorsKey, 0) > 0)
            {
                EditorApplication.delayCall -= FinalizeFailedCompilation;
                EditorApplication.delayCall += FinalizeFailedCompilation;
            }
        }

        private static void OnAssemblyCompilationFinished(
            string assemblyPath,
            CompilerMessage[] messages)
        {
            int errors = 0;
            int warnings = 0;
            if (messages != null)
            {
                for (int index = 0; index < messages.Length; index++)
                {
                    switch (messages[index].type)
                    {
                        case CompilerMessageType.Error:
                            errors++;
                            break;
                        case CompilerMessageType.Warning:
                            warnings++;
                            break;
                    }
                }
            }

            if (errors > 0)
            {
                SessionState.SetInt(
                    CompilationErrorsKey,
                    SessionState.GetInt(CompilationErrorsKey, 0) + errors);
            }

            RecordEvent(
                "Compilation",
                $"Assembly compile FINISH notification | {assemblyPath} | errors={errors}, warnings={warnings}; message counts only, no duration inference.");
        }

        private static void OnBeforeAssemblyReload()
        {
            if (!IsCaptureActive)
            {
                return;
            }

            SessionState.SetBool(BeforeReloadSeenKey, true);
            RecordEvent("Reload", "beforeAssemblyReload.");
            SessionState.SetInt(PendingRepairCountKey, 0);
            PersistTimeline();
            StopPostReloadSampling();
        }

        private static void OnAfterAssemblyReload()
        {
            if (!IsCaptureActive)
            {
                return;
            }

            long nowTicks = DateTime.UtcNow.Ticks;
            SessionState.SetBool(AfterReloadSeenKey, true);
            SetLong(AfterReloadTicksKey, nowTicks);
            RecordEvent("Reload", "afterAssemblyReload.");
            StartPostReloadSampling();
        }

        private static void StartPostReloadSampling()
        {
            if (!IsCaptureActive || postReloadSamplingActive)
            {
                return;
            }

            postReloadSamplingActive = true;
            postReloadUpdateCount = 0;
            firstDelayCallRecorded = false;
            long nowTicks = DateTime.UtcNow.Ticks;
            lastObservationHeartbeatUtcTicks = nowTicks;
            lastEditorUpdateUtcTicks = nowTicks;
            EditorApplication.update -= OnPostReloadUpdate;
            EditorApplication.update += OnPostReloadUpdate;
            EditorApplication.delayCall -= OnFirstDiagnosticDelayCall;
            EditorApplication.delayCall += OnFirstDiagnosticDelayCall;
        }

        private static void StopPostReloadSampling()
        {
            postReloadSamplingActive = false;
            EditorApplication.update -= OnPostReloadUpdate;
            EditorApplication.delayCall -= OnFirstDiagnosticDelayCall;
        }

        private static void OnFirstDiagnosticDelayCall()
        {
            if (!IsCaptureActive || firstDelayCallRecorded)
            {
                return;
            }

            firstDelayCallRecorded = true;
            RecordEvent(
                "EditorCallback",
                "First R1 diagnostic delayCall serviced after reload.");
        }

        private static void OnPostReloadUpdate()
        {
            if (!IsCaptureActive)
            {
                StopPostReloadSampling();
                return;
            }

            long nowTicks = DateTime.UtcNow.Ticks;
            postReloadUpdateCount++;

            double updateGapSeconds = lastEditorUpdateUtcTicks > 0L
                ? TimeSpan.FromTicks(
                    Math.Max(0L, nowTicks - lastEditorUpdateUtcTicks))
                    .TotalSeconds
                : 0.0;
            if (postReloadUpdateCount <= 5)
            {
                RecordObservationEvent(
                    "EditorUpdate",
                    $"Post-reload editor update #{postReloadUpdateCount}.");
            }
            else if (updateGapSeconds >= EditorUpdateGapThresholdSeconds)
            {
                RecordObservationEvent(
                    "EditorUpdate",
                    $"Editor update resumed after {updateGapSeconds:F3} s gap | " +
                    $"isCompiling={EditorApplication.isCompiling}, " +
                    $"isUpdating={EditorApplication.isUpdating}, " +
                    $"isPlaying={EditorApplication.isPlaying}.");
            }

            double heartbeatSeconds = lastObservationHeartbeatUtcTicks > 0L
                ? TimeSpan.FromTicks(
                    Math.Max(
                        0L,
                        nowTicks - lastObservationHeartbeatUtcTicks))
                    .TotalSeconds
                : 0.0;
            if (heartbeatSeconds >= ObservationHeartbeatSeconds)
            {
                RecordObservationEvent(
                    "Heartbeat",
                    $"Capture active | updates={postReloadUpdateCount}, " +
                    $"readiness={BuildReadinessSummary()}.");
                lastObservationHeartbeatUtcTicks = nowTicks;
            }

            lastEditorUpdateUtcTicks = nowTicks;

            bool afterReloadSeen =
                SessionState.GetBool(AfterReloadSeenKey, false);
            bool requirePlayMode =
                SessionState.GetBool(RequirePlayModeKey, false);
            bool playReady =
                !requirePlayMode ||
                SessionState.GetBool(EnteredPlayModeSeenKey, false);
            int pendingRepairs =
                SessionState.GetInt(PendingRepairCountKey, 0);
            bool repairReady =
                pendingRepairs <= 0 &&
                (!repairScheduleSeenThisDomain || repairSeenThisDomain);
            double quietSeconds = lastActivityUtcTicks > 0L
                ? TimeSpan.FromTicks(
                    Math.Max(0L, nowTicks - lastActivityUtcTicks)).TotalSeconds
                : 0.0;

            if (afterReloadSeen &&
                firstDelayCallRecorded &&
                repairReady &&
                playReady &&
                quietSeconds >= QuietSecondsBeforeFinalize)
            {
                FinalizeCapture(
                    "Post-reload delayed work, PixelSurface repair readiness, Play requirement, and quiet-window requirements were satisfied.");
                return;
            }

            long afterReloadTicks = GetLong(AfterReloadTicksKey);
            double postReloadWallSeconds = afterReloadTicks > 0L
                ? TimeSpan.FromTicks(
                    Math.Max(0L, nowTicks - afterReloadTicks)).TotalSeconds
                : 0.0;
            if (afterReloadSeen &&
                postReloadWallSeconds >= PostReloadSafetyTimeoutSeconds)
            {
                FinalizeCapture(
                    "Post-reload safety timeout reached before readiness: " +
                    BuildReadinessSummary());
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!IsCaptureActive)
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SessionState.SetBool(EnteredPlayModeSeenKey, true);
            }

            RecordEvent(
                "PlayMode",
                $"State changed: {state} | isPlaying={EditorApplication.isPlaying}, " +
                $"isCompiling={EditorApplication.isCompiling}, " +
                $"isUpdating={EditorApplication.isUpdating}.");
        }

        private static string BuildReadinessSummary()
        {
            bool afterReloadSeen =
                SessionState.GetBool(AfterReloadSeenKey, false);
            bool requirePlayMode =
                SessionState.GetBool(RequirePlayModeKey, false);
            bool enteredPlayMode =
                SessionState.GetBool(EnteredPlayModeSeenKey, false);
            int pendingRepairs =
                SessionState.GetInt(PendingRepairCountKey, 0);
            return
                $"afterReload={afterReloadSeen}, " +
                $"diagnosticDelayCall={firstDelayCallRecorded}, " +
                $"repairScheduleSeenThisDomain={repairScheduleSeenThisDomain}, " +
                $"repairSeenThisDomain={repairSeenThisDomain}, " +
                $"pendingRepairs={pendingRepairs}, " +
                $"requirePlay={requirePlayMode}, enteredPlay={enteredPlayMode}";
        }

        private static void FinalizeFailedCompilation()
        {
            if (!IsCaptureActive ||
                SessionState.GetInt(CompilationErrorsKey, 0) <= 0)
            {
                return;
            }

            FinalizeCapture(
                "Compilation failed; no successful assembly reload was expected.");
        }

        private static void FinalizeCapture(string reason)
        {
            if (!IsCaptureActive)
            {
                return;
            }

            RecordEvent("Capture", "Finalizing R1 capture: " + reason);
            StopPostReloadSampling();

            string captureId = SessionState.GetString(CaptureIdKey, "<unknown>");
            string trigger = SessionState.GetString(TriggerKey, "<unknown>");
            long startTicks = captureStartUtcTicks;
            double totalSeconds = startTicks > 0L
                ? TimeSpan.FromTicks(
                    Math.Max(0L, DateTime.UtcNow.Ticks - startTicks))
                    .TotalSeconds
                : 0.0;
            int repairCount = SessionState.GetInt(RepairCountKey, 0);
            int scheduleCount = SessionState.GetInt(RepairScheduleCountKey, 0);
            int pendingRepairCount =
                SessionState.GetInt(PendingRepairCountKey, 0);
            int compilationErrors = SessionState.GetInt(CompilationErrorsKey, 0);
            bool beforeReloadSeen =
                SessionState.GetBool(BeforeReloadSeenKey, false);
            bool afterReloadSeen =
                SessionState.GetBool(AfterReloadSeenKey, false);
            bool requirePlayMode =
                SessionState.GetBool(RequirePlayModeKey, false);
            bool enteredPlayMode =
                SessionState.GetBool(EnteredPlayModeSeenKey, false);

            StringBuilder builder = new StringBuilder(8192);
            builder.AppendLine("EDITOR-RELOAD-DIAG-R1");
            builder.AppendLine("Domain Reload / PixelSurface Repair Timeline");
            builder.AppendLine($"Capture: {captureId}");
            builder.AppendLine($"Trigger: {trigger}");
            builder.AppendLine($"Total capture wall time: {totalSeconds:F3} s");
            builder.AppendLine(
                $"Reload boundaries: before={beforeReloadSeen}, after={afterReloadSeen}");
            builder.AppendLine($"Compilation errors: {compilationErrors}");
            builder.AppendLine(
                $"PixelSurface repair schedules: {scheduleCount}");
            builder.AppendLine(
                $"PixelSurface RepairAllLibraries invocations: {repairCount}");
            builder.AppendLine(
                $"Pending PixelSurface delayed repairs: {pendingRepairCount}");
            builder.AppendLine(
                $"Diagnostic delayCall serviced in final domain: {firstDelayCallRecorded}");
            builder.AppendLine(
                $"Play requirement: required={requirePlayMode}, entered={enteredPlayMode}");
            builder.AppendLine(
                $"Final readiness: {BuildReadinessSummary()}");
            builder.AppendLine($"Finalize reason: {reason}");
            builder.AppendLine(
                "Compilation event timestamps are notification markers only; do not interpret their deltas as authoritative compiler execution time.");
            builder.AppendLine();
            builder.AppendLine("Timeline:");
            string timeline = GetTimelineText();
            builder.AppendLine(
                string.IsNullOrWhiteSpace(timeline)
                    ? "<no timeline events recorded>"
                    : timeline);

            string report = builder.ToString();
            SessionState.SetString(LastReportKey, report);
            captureActive = false;
            SessionState.SetBool(CaptureActiveKey, false);
            EditorGUIUtility.systemCopyBuffer = report;
            Debug.Log(
                "[Editor Reload Diagnostic] Capture complete. Full report copied to clipboard.\n" +
                report);
        }


        private static void EnsureTimelineLoaded()
        {
            if (timelineBuilder != null)
            {
                return;
            }

            string persisted = SessionState.GetString(TimelineKey, string.Empty);
            timelineBuilder = new StringBuilder(
                Mathf.Max(4096, persisted.Length + 1024));
            if (!string.IsNullOrEmpty(persisted))
            {
                timelineBuilder.Append(persisted);
            }
        }

        private static string GetTimelineText()
        {
            EnsureTimelineLoaded();
            return timelineBuilder.ToString();
        }

        private static void PersistTimeline()
        {
            if (!IsCaptureActive)
            {
                return;
            }

            SessionState.SetString(TimelineKey, GetTimelineText());
        }

        private static void SetLong(string key, long value)
        {
            SessionState.SetString(
                key,
                value.ToString(CultureInfo.InvariantCulture));
        }

        private static long GetLong(string key)
        {
            string value = SessionState.GetString(key, string.Empty);
            return long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long parsed)
                    ? parsed
                    : 0L;
        }
    }
}
