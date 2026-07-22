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
                "Capturing automatic Layer C reveal-speed timing evidence.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;

            river ??= GetComponent<StylizedRiver>();
            if (!Application.isPlaying)
            {
                return FinalizeAutomaticBirthRevealSpeedReport(
                    BuildUnavailableAutomaticBirthRevealSpeedReport(
                        "The reveal-speed report requires Play Mode so active " +
                        "events, pool occupancy, and rejected starts are real."),
                    false,
                    "The reveal-speed report is Play Mode only.");
            }

            if (river == null)
            {
                return FinalizeAutomaticBirthRevealSpeedReport(
                    BuildUnavailableAutomaticBirthRevealSpeedReport(
                        "No StylizedRiver owner is available."),
                    false,
                    "No StylizedRiver owner is available.");
            }

            StringBuilder report = new(16384);
            report.AppendLine(
                "RIVER FOAM AUTOMATIC BIRTH REVEAL-SPEED REPORT");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                "Contract: requested speed = Base Reveal Speed × pattern " +
                "multiplier × deterministic jitter; resolved duration = " +
                "max(material step, path distance / requested speed).");
            report.AppendLine(
                "Arc/Semi-Arc timing below describes Build only. Hold, Release, " +
                "and Rest remain separate authored lifecycle phases.");
            report.AppendLine();

            float updateRate = ResolveUpdateRate();
            float materialStepDuration = 1f / Mathf.Max(1f, updateRate);
            report.AppendLine("RUNTIME SUMMARY");
            report.AppendLine($"Material update rate: {updateRate:0.###} Hz");
            report.AppendLine(
                $"Material step duration: {materialStepDuration:0.######} s");
            report.AppendLine(
                $"Automatic event pool: {activeAutomaticFoamSourceEventCount}/" +
                $"{automaticFoamSourceEvents.Length} active");
            report.AppendLine(
                $"Rasterized this material update: " +
                $"{automaticSourceEventsRasterizedLastUpdate}");
            report.AppendLine(
                $"Shore starts/rejected last update: " +
                $"{automaticShoreBirthSubmittedLastUpdate}/" +
                $"{automaticShoreBirthRejectedLastUpdate}; total starts=" +
                $"{automaticShoreBirthSubmittedTotal}");
            report.AppendLine(
                $"Object starts/rejected last update: " +
                $"{automaticObjectBirthSubmittedLastUpdate}/" +
                $"{automaticObjectBirthRejectedLastUpdate}; total starts=" +
                $"{automaticObjectBirthSubmittedTotal}");
            report.AppendLine(
                $"Free-Water starts/rejected last update: " +
                $"{automaticFreeWaterBirthSubmittedLastUpdate}/" +
                $"{automaticFreeWaterBirthRejectedLastUpdate}; total starts=" +
                $"{automaticFreeWaterBirthSubmittedTotal}");
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
                AppendAutomaticRevealTiming(
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
                float revealDuration =
                    IsPersistentAutomaticSourceEmitter(sourceEvent.Type)
                        ? sourceEvent.ObjectBuildDuration
                        : sourceEvent.Duration;
                float actualSpeed = sourceEvent.RevealPathDistanceMetres /
                    Mathf.Max(0.0001f, revealDuration);
                report.AppendLine(
                    $"Slot {index:00} / Event {sourceEvent.EventId} / " +
                    $"{AutomaticRevealSourceName(sourceEvent.Type)}");
                report.AppendLine(
                    $"  elapsed={sourceEvent.Elapsed:0.###} s; " +
                    $"revealDuration={revealDuration:0.###} s; " +
                    $"path={sourceEvent.RevealPathDistanceMetres:0.###} m");
                report.AppendLine(
                    $"  requested={sourceEvent.FormationSpeedMetresPerSecond:0.###} m/s; " +
                    $"raw={sourceEvent.RawRevealDurationSeconds:0.###} s; " +
                    $"actual={actualSpeed:0.###} m/s; " +
                    $"cadenceLimited={sourceEvent.RevealCadenceLimited}");
                if (IsPersistentAutomaticSourceEmitter(sourceEvent.Type))
                {
                    report.AppendLine(
                        $"  hold={sourceEvent.ObjectHoldDuration:0.###} s; " +
                        $"release={sourceEvent.ObjectReleaseDuration:0.###} s; " +
                        $"rest={sourceEvent.ObjectRestDuration:0.###} s");
                }
            }

            if (!anyActive)
            {
                report.AppendLine("No automatic source event is active.");
            }

            report.AppendLine();
            report.AppendLine(
                "REPORT VERDICT: PASS — live timing and capacity evidence captured.");
            return FinalizeAutomaticBirthRevealSpeedReport(
                report.ToString(),
                true,
                "Automatic birth reveal-speed evidence captured.");
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

        private void AppendAutomaticRevealTiming(
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
                return;
            }

            AutomaticRevealTimingTelemetry timing =
                automaticRevealTimingByType[telemetryIndex];
            report.AppendLine(
                $"  latestEvent={timing.EventId}; " +
                $"path={timing.PathDistanceMetres:0.###} m");
            report.AppendLine(
                $"  requested={timing.RequestedSpeedMetresPerSecond:0.###} m/s; " +
                $"rawDuration={timing.RawDurationSeconds:0.###} s; " +
                $"resolvedDuration={timing.ResolvedDurationSeconds:0.###} s");
            report.AppendLine(
                $"  actual={timing.ActualSpeedMetresPerSecond:0.###} m/s; " +
                $"cadenceLimited={timing.CadenceLimited}");
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
            string summary)
        {
            topologyCacheDiagnosticState = passed ? "Passed" : "Unavailable";
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
                    "[River Foam Reveal Speed] UNAVAILABLE — " +
                    topologyCacheDiagnosticReportPath,
                    river != null ? river : this);
            }

            return passed;
        }
    }
}
#endif
