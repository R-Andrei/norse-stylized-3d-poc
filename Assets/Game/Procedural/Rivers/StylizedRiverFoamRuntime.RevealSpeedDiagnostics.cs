#if UNITY_EDITOR
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        public bool RunAutomaticBirthRevealSpeedReport()
        {
            topologyCacheDiagnosticRunCount++;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary =
                "Validating literal cells/s automatic reveal kinematics.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;

            river ??= GetComponent<StylizedRiver>();
            if (!Application.isPlaying)
            {
                return FinalizeAutomaticBirthRevealSpeedReport(
                    BuildUnavailableAutomaticBirthRevealSpeedReport(
                        "The reveal-speed report requires Play Mode so active " +
                        "events and captured authoring speeds are real."),
                    false,
                    "The reveal-speed report is Play Mode only.",
                    true);
            }

            if (river == null)
            {
                return FinalizeAutomaticBirthRevealSpeedReport(
                    BuildUnavailableAutomaticBirthRevealSpeedReport(
                        "No StylizedRiver owner is available."),
                    false,
                    "No StylizedRiver owner is available.",
                    true);
            }

            StringBuilder report = new(24576);
            report.AppendLine(
                "RIVER FOAM AUTOMATIC BIRTH REVEAL-SPEED REPORT");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                "D9 contract: headDistanceCells = speedCellsPerSecond × " +
                "elapsedSeconds, clamped to stroke path length; duration = " +
                "pathLengthCells / speedCellsPerSecond. No family m/s base, " +
                "pattern multiplier, speed jitter, or material-tick duration " +
                "floor participates in reveal timing.");
            report.AppendLine();

            bool passed = true;
            report.AppendLine("KINEMATICS MATRIX");
            float[] lengths = { 3f, 8f, 15f, 31f };
            float[] speeds = { 1f, 2f, 5f, 10f, 100f };
            float[] rates = { 8f, 12f, 16f };
            for (int lengthIndex = 0; lengthIndex < lengths.Length; lengthIndex++)
            {
                float length = lengths[lengthIndex];
                for (int speedIndex = 0; speedIndex < speeds.Length; speedIndex++)
                {
                    float speed = speeds[speedIndex];
                    ResolvedAutomaticRevealKinematics kinematics =
                        ResolveAutomaticRevealKinematics(length, speed);
                    float expectedDuration = length / speed;
                    bool durationPass = Mathf.Abs(
                        kinematics.DurationSeconds - expectedDuration) <= 0.000001f;
                    passed &= durationPass;
                    report.AppendLine(
                        $"  L={length:0.###} cells, v={speed:0.###} cells/s " +
                        $"=> T={kinematics.DurationSeconds:0.######} s " +
                        $"[{(durationPass ? "PASS" : "FAIL")}]");

                    for (int rateIndex = 0; rateIndex < rates.Length; rateIndex++)
                    {
                        float rate = rates[rateIndex];
                        float dt = 1f / rate;
                        float previousHead = 0f;
                        bool monotonic = true;
                        bool exactVelocity = true;
                        int ticks = Mathf.CeilToInt(expectedDuration / dt) + 1;
                        for (int tick = 1; tick <= ticks; tick++)
                        {
                            float elapsed = Mathf.Min(
                                expectedDuration,
                                tick * dt);
                            float head = ResolveAutomaticRevealHeadDistanceCells(
                                length,
                                speed,
                                elapsed);
                            float expectedHead = Mathf.Min(
                                length,
                                speed * elapsed);
                            monotonic &= head + 0.000001f >= previousHead;
                            exactVelocity &= Mathf.Abs(head - expectedHead) <=
                                0.00001f;
                            previousHead = head;
                        }

                        bool completion = Mathf.Abs(previousHead - length) <=
                            0.00001f;
                        passed &= monotonic && exactVelocity && completion;
                        report.AppendLine(
                            $"    {rate:0} Hz: monotonic={monotonic}; " +
                            $"exactHead={exactVelocity}; completion={completion}");
                    }
                }
            }

            report.AppendLine();
            report.AppendLine(
                "HARD EXAMPLES: 15 cells / 1 cell/s = 15.000 s; " +
                "15 cells / 5 cells/s = 3.000 s; " +
                "15 cells / 100 cells/s = 0.150 s.");
            report.AppendLine();

            report.AppendLine("LATEST OBSERVED TIMING BY RECIPE");
            AutomaticFoamSourceEventType[] recipeOrder =
            {
                AutomaticFoamSourceEventType.ShoreRibbon,
                AutomaticFoamSourceEventType.InwardWash,
                AutomaticFoamSourceEventType.ObjectContactArc,
                AutomaticFoamSourceEventType.ObjectContactSemiArc,
                AutomaticFoamSourceEventType.ObjectContactFleck,
                AutomaticFoamSourceEventType.FreeWaterLaceConnector,
                AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector,
                AutomaticFoamSourceEventType.FreeWaterTornFragment
            };
            for (int recipeIndex = 0;
                 recipeIndex < recipeOrder.Length;
                 recipeIndex++)
            {
                AutomaticFoamSourceEventType sourceType =
                    recipeOrder[recipeIndex];
                passed &= AppendAutomaticRevealTiming(
                    report,
                    sourceType,
                    CountActiveAutomaticSourceEvents(sourceType));
            }

            report.AppendLine();
            report.AppendLine("ACTIVE EVENT DETAIL");
            bool anyActive = false;
            for (int index = 0;
                 index < automaticFoamSourceEvents.Length;
                 index++)
            {
                AutomaticFoamSourceEvent sourceEvent =
                    automaticFoamSourceEvents[index];
                if (!sourceEvent.Active)
                {
                    continue;
                }

                anyActive = true;
                ResolveAutomaticSourceDepositionState(
                    sourceEvent,
                    sourceEvent.Elapsed,
                    out float phaseCode,
                    out float headDistanceCells);
                float activePathLengthCells =
                    IsAutomaticObjectContactCycle(sourceEvent.Type)
                        ? ResolveAutomaticObjectContactPhasePathLengthCells(
                            sourceEvent,
                            phaseCode)
                        : sourceEvent.RevealPathLengthCells;
                float speed = Mathf.Max(
                    0.0001f,
                    sourceEvent.RevealSpeedCellsPerSecond);
                float expectedPhaseDuration =
                    activePathLengthCells / speed;
                float storedPhaseDuration =
                    IsAutomaticObjectContactCycle(sourceEvent.Type) &&
                    phaseCode >= 0.5f
                        ? sourceEvent.ObjectContactStrokeDuration
                        : sourceEvent.ObjectBuildDuration > 0f
                            ? sourceEvent.ObjectBuildDuration
                            : sourceEvent.Duration;
                bool durationPass = Mathf.Abs(
                    storedPhaseDuration - expectedPhaseDuration) <= 0.0001f;
                passed &= durationPass;
                report.AppendLine(
                    $"Slot {index:00} / Event {sourceEvent.EventId} / " +
                    $"{AutomaticRevealSourceName(sourceEvent.Type)}");
                report.AppendLine(
                    $"  speed={speed:0.###} cells/s; " +
                    $"phase={(int)phaseCode}; path={activePathLengthCells:0.###} cells; " +
                    $"head={headDistanceCells:0.###} cells");
                report.AppendLine(
                    $"  storedPhaseDuration={storedPhaseDuration:0.######} s; " +
                    $"expected={expectedPhaseDuration:0.######} s; " +
                    $"{(durationPass ? "PASS" : "FAIL")}");
            }

            if (!anyActive)
            {
                report.AppendLine("No automatic source event is active.");
            }

            report.AppendLine();
            report.AppendLine(
                passed
                    ? "REPORT VERDICT: PASS — literal cells/s kinematics validated."
                    : "REPORT VERDICT: FAIL — one or more literal cells/s invariants failed.");
            return FinalizeAutomaticBirthRevealSpeedReport(
                report.ToString(),
                passed,
                passed
                    ? "Automatic reveal-speed cells/s invariants passed."
                    : "Automatic reveal-speed cells/s invariants failed.");
        }

        private int CountActiveAutomaticSourceEvents(
            AutomaticFoamSourceEventType sourceType)
        {
            int count = 0;
            for (int index = 0;
                 index < automaticFoamSourceEvents.Length;
                 index++)
            {
                if (automaticFoamSourceEvents[index].Active &&
                    automaticFoamSourceEvents[index].Type == sourceType)
                {
                    count++;
                }
            }

            return count;
        }

        private bool AppendAutomaticRevealTiming(
            StringBuilder report,
            AutomaticFoamSourceEventType sourceType,
            int activeCount)
        {
            int telemetryIndex = (int)sourceType;
            report.AppendLine(
                AutomaticRevealSourceName(sourceType) +
                $" — active={activeCount}");
            if (telemetryIndex <= 0 ||
                telemetryIndex >= automaticRevealTimingByType.Length ||
                !automaticRevealTimingByType[telemetryIndex].HasValue)
            {
                report.AppendLine(
                    "  No event of this recipe has started during the current " +
                    "automatic-source session.");
                return true;
            }

            AutomaticRevealTimingTelemetry timing =
                automaticRevealTimingByType[telemetryIndex];
            float expectedDuration = timing.PathLengthCells /
                Mathf.Max(0.0001f, timing.RequestedSpeedCellsPerSecond);
            bool pass = Mathf.Abs(
                timing.DurationSeconds - expectedDuration) <= 0.0001f;
            report.AppendLine(
                $"  latestEvent={timing.EventId}; " +
                $"path={timing.PathLengthCells:0.###} cells; " +
                $"speed={timing.RequestedSpeedCellsPerSecond:0.###} cells/s");
            report.AppendLine(
                $"  duration={timing.DurationSeconds:0.######} s; " +
                $"expected={expectedDuration:0.######} s; " +
                $"{(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        private static string AutomaticRevealSourceName(
            AutomaticFoamSourceEventType sourceType)
        {
            return sourceType switch
            {
                AutomaticFoamSourceEventType.ShoreRibbon => "Shore Ribbon",
                AutomaticFoamSourceEventType.InwardWash => "Inward Wash",
                AutomaticFoamSourceEventType.ObjectContactArc =>
                    "Object Contact Arc",
                AutomaticFoamSourceEventType.ObjectContactSemiArc =>
                    "Object Contact Semi-Arc",
                AutomaticFoamSourceEventType.ObjectContactFleck =>
                    "Object Contact Fleck",
                AutomaticFoamSourceEventType.FreeWaterLaceConnector =>
                    "Free Water Lace Connector",
                AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector =>
                    "Free Water Cross-Lace Connector",
                AutomaticFoamSourceEventType.FreeWaterTornFragment =>
                    "Free Water Torn Fragment",
                _ => sourceType.ToString()
            };
        }

        private string BuildUnavailableAutomaticBirthRevealSpeedReport(
            string reason)
        {
            StringBuilder report = new(2048);
            report.AppendLine(
                "RIVER FOAM AUTOMATIC BIRTH REVEAL-SPEED REPORT");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine();
            report.AppendLine(reason ?? "Unavailable.");
            report.AppendLine("REPORT VERDICT: UNAVAILABLE");
            return report.ToString();
        }

        private bool FinalizeAutomaticBirthRevealSpeedReport(
            string report,
            bool passed,
            string summary,
            bool unavailable = false)
        {
            topologyCacheDiagnosticState = passed
                ? "Passed"
                : unavailable ? "Unavailable" : "Failed";
            topologyCacheDiagnosticSummary = summary ?? string.Empty;
            topologyCacheDiagnosticReport = report ?? string.Empty;
            if (!TryWriteLatestDiagnosticReport(
                    "LatestAutomaticBirthRevealSpeed",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string writeError))
            {
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    "The reveal-speed report could not be written: " +
                    writeError;
                Debug.LogError(
                    "[River Foam Reveal Speed] " +
                    topologyCacheDiagnosticSummary,
                    river != null ? river : this);
                return false;
            }

            if (passed)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log(
                    "[River Foam Reveal Speed] PASS — " +
                    topologyCacheDiagnosticReportPath,
                    river != null ? river : this);
            }
            else
            {
                Debug.LogWarning(
                    "[River Foam Reveal Speed] " +
                    (unavailable ? "UNAVAILABLE" : "FAIL") + " — " +
                    topologyCacheDiagnosticReportPath,
                    river != null ? river : this);
            }

            return passed;
        }
    }
}
#endif
