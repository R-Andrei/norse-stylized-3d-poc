using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    internal sealed partial class StylizedRiverEditor
    {
        private const float DeferredPresenceCapacityLossReviewRatio = 0.01f;

        private void DrawRuntimeDiagnostics()
        {
            EditorGUILayout.LabelField(
                "Live runtime telemetry is grouped by owning system. " +
                "Every value is read-only; unavailable data keeps its row " +
                "instead of changing the Inspector layout.",
                EditorStyles.wordWrappedMiniLabel);

            DrawNestedSection(
                InspectorSection.DiagnosticsDomainGeometry,
                "Domain & Geometry",
                DrawDomainAndGeometryDiagnostics);
            DrawNestedSection(
                InspectorSection.DiagnosticsDisturbances,
                "Disturbances",
                DrawDisturbanceDiagnostics);
            DrawNestedSection(
                InspectorSection.DiagnosticsFoam,
                "Foam",
                DrawFoamDiagnostics);
        }

        private void DrawDomainAndGeometryDiagnostics()
        {
            StylizedRiver river = targets.Length == 1
                ? target as StylizedRiver
                : null;
            string unavailable = GetSingleSelectionDiagnosticValue(
                "Select one river for resolved diagnostics.");

            RiverDomainSnapshot domain =
                river != null ? river.Domain : default;
            bool domainValid = river != null && domain.IsValid;

            DrawReadOnlyRow(
                new GUIContent(
                    "Domain State",
                    "Current validity and total local length of the shared " +
                    "river-domain snapshot."),
                river == null
                    ? unavailable
                    : domainValid
                        ? $"Valid / {domain.LocalLength:0.00} m"
                        : "No valid domain");
            DrawReadOnlyRow(
                new GUIContent(
                    "Sample Count",
                    "Number of authoritative longitudinal domain samples."),
                river == null
                    ? unavailable
                    : domainValid
                        ? domain.SampleCount.ToString("N0")
                        : "—");
            DrawReadOnlyRow(
                new GUIContent(
                    "Actual Spacing",
                    "Minimum and maximum measured world-space spacing between " +
                    "adjacent domain samples."),
                river == null
                    ? unavailable
                    : domainValid
                        ? $"{domain.MinimumSampleSpacing:0.000}–" +
                          $"{domain.MaximumSampleSpacing:0.000} m"
                        : "—");
            DrawReadOnlyRow(
                new GUIContent(
                    "Global Range",
                    "Global downstream metre range assigned to this river."),
                river == null
                    ? unavailable
                    : domainValid
                        ? $"{domain.GlobalDistanceMinimum:0.00}–" +
                          $"{domain.GlobalDistanceMaximum:0.00} m"
                        : "—");
            DrawReadOnlyRow(
                new GUIContent(
                    "Generated Water Width",
                    "Resolved visible water-surface width after generation."),
                river == null
                    ? unavailable
                    : $"{river.GeneratedSurfaceWidth:0.000} m");
            DrawReadOnlyRow(
                new GUIContent(
                    "Hidden Shore Overlap",
                    "Resolved hidden water overlap beneath each shoreline."),
                river == null
                    ? unavailable
                    : $"{river.ResolvedShorelineOverlap:0.000} m / side");
            DrawReadOnlyRow(
                new GUIContent(
                    "Collider Handoff Width",
                    "Width of the generated corridor collider handoff."),
                river == null
                    ? unavailable
                    : river.CorridorHandoffWidth > 0f
                        ? $"{river.CorridorHandoffWidth:0.000} m"
                        : "Not generated");
            DrawReadOnlyRow(
                new GUIContent(
                    "Integration Apron",
                    "Hidden integration apron generated on each side of the " +
                    "visible river corridor."),
                river == null
                    ? unavailable
                    : river.CorridorIntegrationApronWidth > 0f
                        ? $"{river.CorridorIntegrationApronWidth:0.000} m / side"
                        : "Not generated");
            DrawReadOnlyRow(
                new GUIContent(
                    "Corridor Render Width",
                    "Total resolved outer width of the visible corridor mesh."),
                river == null
                    ? unavailable
                    : river.CorridorOuterWidth > 0f
                        ? $"{river.CorridorOuterWidth:0.000} m"
                        : "Not generated");
            DrawReadOnlyRow(
                new GUIContent(
                    "Surface Row Spacing",
                    "Resolved longitudinal spacing of generated surface rows."),
                river == null
                    ? unavailable
                    : $"{river.ResolvedSurfaceLongitudinalSpacing:0.000} m");
            DrawReadOnlyRow(
                new GUIContent(
                    "Downward Motion Clearance",
                    "Maximum resolved downward displacement reserved for " +
                    "surface motion and shoreline safety."),
                river == null
                    ? unavailable
                    : $"{river.ResolvedMaximumDownwardMotion:0.000} m");
            DrawReadOnlyRow(
                new GUIContent(
                    "Resolved Bed Roughness",
                    "Effective generated bed-roughness amplitude."),
                river == null
                    ? unavailable
                    : $"{river.ResolvedBedRoughness:0.000} m");
        }

        private void DrawDisturbanceDiagnostics()
        {
            DrawNestedSection(
                InspectorSection.DiagnosticsDisturbanceSummary,
                "Summary",
                DrawDisturbanceDiagnosticSummary);
            DrawNestedSection(
                InspectorSection.DiagnosticsDisturbanceDispatches,
                "Dispatches",
                DrawDisturbanceDiagnosticDispatches);
            DrawNestedSection(
                InspectorSection.DiagnosticsDisturbanceSources,
                "Sources & Rebuild State",
                DrawDisturbanceDiagnosticSources);
            DrawNestedSection(
                InspectorSection.DiagnosticsDisturbanceMemory,
                "Memory",
                DrawDisturbanceDiagnosticMemory);
        }

        private void DrawDisturbanceDiagnosticSummary()
        {
            GetSingleDisturbanceContext(
                out StylizedRiver river,
                out StylizedRiverDisturbanceRuntime runtime,
                out string unavailable);

            DrawReadOnlyRow(
                new GUIContent("Runtime State"),
                runtime != null
                    ? runtime.IsSleeping ? "Sleeping" : "Active"
                    : river != null && river.RuntimeDisturbancesEnabled
                        ? "Will be created automatically"
                        : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Compute Support"),
                runtime != null
                    ? runtime.IsSupported ? "Available" : "Unavailable"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Ripple Field"),
                runtime != null
                    ? runtime.IsAllocated
                        ? $"{runtime.FieldWidth} × {runtime.FieldHeight}"
                        : "Sleeping / not allocated"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Wake Field"),
                runtime != null
                    ? runtime.IsAllocated
                        ? $"{runtime.WakeFieldWidth} × {runtime.WakeFieldHeight}"
                        : "Sleeping / not allocated"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Logical Chunks"),
                runtime != null
                    ? $"{runtime.ActiveChunkCount} active / " +
                      $"{runtime.ChunkCount} total"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Wake Chunks"),
                runtime != null
                    ? $"{runtime.ActiveWakeChunkCount} active / " +
                      $"{runtime.ChunkCount} total"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Simulation Rates"),
                runtime != null
                    ? $"ripple {runtime.SimulationRate:0} Hz / " +
                      $"wake {runtime.WakeSimulationRate:0} Hz"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Continuous Sources"),
                runtime != null
                    ? runtime.ContinuousSourceCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Pending Impacts"),
                runtime != null
                    ? runtime.PendingImpactCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Active Reservations"),
                runtime != null
                    ? runtime.ActiveImpactReservationCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Longest Reservation"),
                runtime != null
                    ? runtime.ActiveImpactReservationCount > 0
                        ? $"{runtime.LongestImpactReservationRemainingSeconds:0.00} s"
                        : "Inactive"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Resolved Ripple Strength"),
                river != null
                    ? river.ResolvedImpactRippleStrength.ToString("0.00")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Effective Ripple Decay"),
                river != null
                    ? $"{river.ResolvedImpactRippleDecay:0.00} /s"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Impacts Injected"),
                runtime != null
                    ? runtime.ImpactsInjectedLastStep.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Ripple Substeps"),
                runtime != null
                    ? $"{runtime.CurrentRippleSubstepCount} current / " +
                      $"{runtime.MaximumRecentRippleSubstepCount} recent max"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Minimum Active Cell"),
                runtime != null
                    ? runtime.ActiveRippleMinimumCellSize > 0f
                        ? $"{runtime.ActiveRippleMinimumCellSize:0.000} m"
                        : "Inactive"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Substep Safety"),
                runtime != null
                    ? runtime.RippleSubstepLimitReached
                        ? "Limit reached"
                        : "Within limit"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Collision Sources"),
                runtime != null
                    ? runtime.RippleCollisionSourceCount.ToString("N0")
                    : unavailable);
        }

        private void DrawDisturbanceDiagnosticDispatches()
        {
            GetSingleDisturbanceContext(
                out _,
                out StylizedRiverDisturbanceRuntime runtime,
                out string unavailable);

            DrawReadOnlyRow(
                new GUIContent(
                    "Compute Dispatches",
                    "Compute-kernel submissions made by the latest river " +
                    "update and the recent five-second peak."),
                runtime != null
                    ? $"{runtime.LastUpdateComputeDispatchCount:N0} last / " +
                      $"{runtime.RecentPeakComputeDispatchCount:N0} peak"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Thread Groups"),
                runtime != null
                    ? $"{runtime.LastUpdateThreadGroupCount:N0} last / " +
                      $"{runtime.RecentPeakThreadGroupCount:N0} peak"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Cell Iterations"),
                runtime != null
                    ? $"{runtime.LastUpdateCellIterationCount:N0} last / " +
                      $"{runtime.RecentPeakCellIterationCount:N0} peak"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Ripple Simulation"),
                runtime != null
                    ? runtime.LastUpdateRippleSimulationDispatchCount
                        .ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Wake Simulation"),
                runtime != null
                    ? runtime.LastUpdateWakeSimulationDispatchCount
                        .ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Impact Injection"),
                runtime != null
                    ? runtime.LastUpdateImpactInjectionDispatchCount
                        .ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Dynamic Wake Injection"),
                runtime != null
                    ? runtime.LastUpdateWakeInjectionDispatchCount
                        .ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Pressure Bakes"),
                runtime != null
                    ? runtime.LastUpdateStaticPressureBakeDispatchCount
                        .ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Wake Source Bakes"),
                runtime != null
                    ? runtime.LastUpdateStaticWakeBakeDispatchCount
                        .ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Boundary Bakes"),
                runtime != null
                    ? runtime.LastUpdateRippleBoundaryBakeDispatchCount
                        .ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Clear Dispatches"),
                runtime != null
                    ? runtime.LastUpdateClearDispatchCount.ToString("N0")
                    : unavailable);
        }

        private void DrawDisturbanceDiagnosticSources()
        {
            GetSingleDisturbanceContext(
                out _,
                out StylizedRiverDisturbanceRuntime runtime,
                out string unavailable);

            DrawReadOnlyRow(
                new GUIContent("Registered Stationary Sources"),
                runtime != null
                    ? runtime.RegisteredStationarySourceCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Valid Pressure Sources"),
                runtime != null
                    ? runtime.ValidStaticPressureSourceCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Valid Wake Sources"),
                runtime != null
                    ? runtime.ValidStaticWakeSourceCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Field Rebuilds"),
                runtime != null
                    ? $"{runtime.LastUpdateFieldRebuildCount:N0} last / " +
                      $"{runtime.RecentPeakFieldRebuildCount:N0} peak"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Ripple Metric Rows"),
                runtime != null
                    ? runtime.RippleMetricRowCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Ripple Boundary Mask"),
                runtime != null
                    ? runtime.IsAllocated
                        ? $"{runtime.RippleBoundaryWidth} × " +
                          $"{runtime.RippleBoundaryHeight}"
                        : "Sleeping / not allocated"
                    : unavailable);
        }

        private void DrawDisturbanceDiagnosticMemory()
        {
            GetSingleDisturbanceContext(
                out _,
                out StylizedRiverDisturbanceRuntime runtime,
                out string unavailable);

            DrawReadOnlyRow(
                new GUIContent("Ripple State"),
                runtime != null
                    ? FormatMemoryBytes(runtime.RippleStateMemoryBytes)
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Static Pressure"),
                runtime != null
                    ? FormatMemoryBytes(runtime.StaticPressureMemoryBytes)
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Ripple Boundary"),
                runtime != null
                    ? FormatMemoryBytes(runtime.RippleBoundaryMemoryBytes)
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Wake State & Source"),
                runtime != null
                    ? FormatMemoryBytes(runtime.WakeFieldMemoryBytes)
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Ripple Metrics"),
                runtime != null
                    ? FormatMemoryBytes(runtime.RippleMetricMemoryBytes)
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Total"),
                runtime != null
                    ? FormatMemoryBytes(runtime.EstimatedMemoryBytes)
                    : unavailable);
        }

        private void DrawFoamDiagnostics()
        {
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamSummary,
                "Summary",
                DrawFoamDiagnosticSummary);
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamLayerA,
                "Layer A — Topology & Cache",
                DrawFoamDiagnosticLayerA);
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamLayerB,
                "Layer B — Velocity",
                DrawFoamDiagnosticLayerB);
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamLayerC,
                "Layer C — Material & Lifecycle",
                DrawFoamDiagnosticLayerC);
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamLayerD,
                "Layer D — Evaluated Shape",
                DrawFoamDiagnosticLayerD);
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamResources,
                "Runtime Resources",
                DrawFoamDiagnosticResources);
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamAdvanced,
                "Advanced Internals",
                DrawFoamDiagnosticAdvanced);
        }

        private void DrawFoamDiagnosticSummary()
        {
            GetSingleFoamContext(
                out StylizedRiver river,
                out StylizedRiverFoamRuntime runtime,
                out string unavailable);

            float storedArea = runtime != null
                ? runtime.IntegratedPresenceArea
                : 0f;
            float visibleArea = runtime != null
                ? runtime.VisiblePresenceCoreArea
                : 0f;
            float hiddenArea = Mathf.Max(0f, storedArea - visibleArea);

            DrawReadOnlyRow(
                new GUIContent("State"),
                river == null
                    ? unavailable
                    : !river.FoamEnabled
                        ? "Disabled"
                        : runtime == null
                            ? "No runtime"
                            : !runtime.ResourcesAllocated
                                ? "Runtime present / no resources"
                                : runtime.IsSleeping
                                    ? "Ready / sleeping"
                                    : "Active");
            DrawReadOnlyRow(
                new GUIContent("Active Rendered Debug View"),
                ResolveActiveDebugViewLabel());
            DrawReadOnlyRow(
                new GUIContent("Stored Material Area"),
                runtime != null
                    ? $"{storedArea:0.000} m²"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Visible Foam Area"),
                runtime != null
                    ? $"{visibleArea:0.000} m²"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Hidden Stored Area"),
                runtime != null
                    ? $"{hiddenArea:0.000} m²"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Transport Status"),
                runtime != null
                    ? ResolveFoamTransportSmoothnessStatus(runtime)
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Primary Warning"),
                ResolveFoamPrimaryWarning(river, runtime, unavailable));
        }

        private void DrawFoamDiagnosticLayerA()
        {
            GetSingleFoamContext(
                out StylizedRiver river,
                out StylizedRiverFoamRuntime runtime,
                out string unavailable);

            DrawReadOnlyRow(
                new GUIContent("Cache Asset"),
                river != null
                    ? river.FoamTopologyCacheAsset != null
                        ? river.FoamTopologyCacheAsset.name
                        : "Not assigned"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Cache Startup"),
                runtime != null
                    ? runtime.TopologyCacheStartupState
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Cache Validation"),
                runtime != null
                    ? $"{runtime.TopologyCacheValidationState} — " +
                      runtime.TopologyCacheValidationSummary
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Cache Fingerprint"),
                runtime != null
                    ? runtime.TopologyCacheCombinedFingerprint
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Major Candidates"),
                runtime != null
                    ? $"{runtime.MajorOpportunityCount:N0} opportunities / " +
                      $"{runtime.MajorAcceptedRegionCount:N0} accepted / " +
                      $"{runtime.MajorRejectedRegionCount:N0} rejected"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Topology Coverage"),
                runtime != null
                    ? $"major {FormatPercent(runtime.MajorSupportCoverage)} / " +
                      $"connectors {FormatPercent(runtime.ConnectorSupportCoverage)} / " +
                      $"negative {FormatPercent(runtime.NegativeAgingPressureCoverage)}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Topology Readback"),
                runtime != null
                    ? runtime.TopologyMetricsAgeSeconds < 0f
                        ? "No completed sample"
                        : runtime.TopologyMetricsFresh
                            ? $"Live / {runtime.TopologyMetricsAgeSeconds:0.00} s old"
                            : $"Stale / {runtime.TopologyMetricsAgeSeconds:0.00} s old"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Replacement State"),
                runtime != null
                    ? $"{runtime.TopologyReplacementState} / " +
                      $"{runtime.TopologyTransitionState}"
                    : unavailable);
        }

        private void DrawFoamDiagnosticLayerB()
        {
            GetSingleFoamContext(
                out _,
                out StylizedRiverFoamRuntime runtime,
                out string unavailable);

            DrawReadOnlyRow(
                new GUIContent("Transport Mode"),
                runtime != null
                    ? "Conservative local 2D donor-cell advection"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Resolved Speed"),
                runtime != null
                    ? $"downstream " +
                      $"{runtime.FoamBaseDownstreamSpeedMetresPerSecond:0.000} m/s / " +
                      $"max lateral " +
                      $"{runtime.FoamMaximumLateralSpeedMetresPerSecond:0.000} m/s"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Lane Phase"),
                runtime != null
                    ? $"{runtime.FoamMotionLaneScrollMetres:0.000} m / " +
                      $"{runtime.FoamMotionLaneScrollCells:0.00} cells"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Field Signatures"),
                runtime != null
                    ? $"lane {runtime.FoamMotionLaneSignature} / " +
                      $"obstacle {runtime.FoamObstacleRoutingSignature}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Material Tick"),
                runtime != null
                    ? $"{runtime.UpdateRate:0.#} Hz / " +
                      $"{runtime.MaterialStepsLastFrame} steps last frame"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Maximum CFL"),
                runtime != null
                    ? $"{runtime.TransportStepCfl:0.000} step / " +
                      $"{runtime.MaximumTransportCfl:0.000} per substep / " +
                      $"{runtime.TransportCflTarget:0.000} target"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Substeps"),
                runtime != null
                    ? $"{runtime.TransportSubstepsUsed} used / " +
                      $"{runtime.TransportSubstepsRequired} required / " +
                      $"{runtime.TransportSubstepLimit} limit"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Cell Travel"),
                runtime != null
                    ? $"downstream " +
                      $"{runtime.EstimatedTransportCellsPerStep:0.000} / " +
                      $"lateral " +
                      $"{runtime.EstimatedLateralTransportCellsPerStep:0.000} cells"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Safety Status"),
                runtime != null
                    ? runtime.TransportSafetyStatus
                    : unavailable);
        }

        private void DrawFoamDiagnosticLayerC()
        {
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamLayerCTransport,
                "Transport Accounting",
                DrawFoamDiagnosticTransportAccounting);
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamLayerCLifecycle,
                "Lifecycle Authority",
                DrawFoamDiagnosticLifecycle);
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamLayerCBirth,
                "Birth Activity",
                DrawFoamDiagnosticBirthActivity);
            DrawNestedSection(
                InspectorSection.DiagnosticsFoamLayerCProbe,
                "Isolated Probe State",
                DrawFoamDiagnosticProbeState);
        }

        private void DrawFoamDiagnosticTransportAccounting()
        {
            GetSingleFoamContext(
                out _,
                out StylizedRiverFoamRuntime runtime,
                out string unavailable);

            string metricsUnavailable = runtime == null
                ? unavailable
                : runtime.TransportMetricsSupported
                    ? "Awaiting asynchronous readback"
                    : "Async GPU readback unsupported";
            bool available = runtime != null &&
                runtime.TransportMetricsAvailable;

            DrawReadOnlyRow(
                new GUIContent("Presence Accounting"),
                available
                    ? $"{runtime.TransportPresenceBefore:0.0000} → " +
                      $"{runtime.TransportPresenceAfter:0.0000} / out " +
                      $"{runtime.TransportPresenceBoundaryOutflow:0.0000}"
                    : metricsUnavailable);
            DrawReadOnlyRow(
                new GUIContent("Life Moment Accounting"),
                available
                    ? $"{runtime.TransportLifeMomentBefore:0.0000} → " +
                      $"{runtime.TransportLifeMomentAfter:0.0000} / out " +
                      $"{runtime.TransportLifeBoundaryOutflow:0.0000}"
                    : "—");
            DrawReadOnlyRow(
                new GUIContent("Pattern Moment Accounting"),
                available
                    ? $"{runtime.TransportPatternMomentBefore:0.0000} → " +
                      $"{runtime.TransportPatternMomentAfter:0.0000} / out " +
                      $"{runtime.TransportPatternBoundaryOutflow:0.0000}"
                    : "—");
            DrawReadOnlyRow(
                new GUIContent("Unaccounted Error"),
                available
                    ? $"P {runtime.TransportPresenceUnaccountedErrorRatio * 100f:0.000}% / " +
                      $"Life {runtime.TransportLifeUnaccountedErrorRatio * 100f:0.000}% / " +
                      $"Pattern {runtime.TransportPatternUnaccountedErrorRatio * 100f:0.000}%"
                    : "—");
            DrawReadOnlyRow(
                new GUIContent("Capacity / Clamp Loss"),
                available
                    ? $"P {runtime.TransportPresenceClampLoss:0.000000} / " +
                      $"Life {runtime.TransportLifeClampLoss:0.000000} / " +
                      $"Pattern {runtime.TransportPatternClampLoss:0.000000} " +
                      $"({runtime.TransportPresenceClampLossRatio * 100f:0.000}% P)"
                    : "—");
            string capacityAttribution = available
                ? "unit " + FormatTransportLoss(
                    runtime.TransportPresenceUnitCapacityLoss,
                    runtime.TransportPresenceBefore) +
                  " / boundary " + FormatTransportLoss(
                    runtime.TransportPresenceBoundaryCapacityLoss,
                    runtime.TransportPresenceBefore) +
                  " / obstacle " + FormatTransportLoss(
                    runtime.TransportPresenceObstacleCapacityLoss,
                    runtime.TransportPresenceBefore)
                : "—";
            string otherPresenceLoss = available
                ? "validity " + FormatTransportLoss(
                    runtime.TransportPresenceStateValidityLoss,
                    runtime.TransportPresenceBefore) +
                  " / cutoff " + FormatTransportLoss(
                    runtime.TransportPresenceMinimumCutoffLoss,
                    runtime.TransportPresenceBefore) +
                  " / residual " + FormatSignedTransportLoss(
                    runtime.TransportPresenceAttributionResidual,
                    runtime.TransportPresenceBefore)
                : "—";

            DrawReadOnlyRow(
                new GUIContent(
                    "Capacity Attribution",
                    "Area-weighted Presence loss from unit storage, " +
                    "fractional shoreline coverage, and obstacle exclusion."),
                capacityAttribution);
            DrawReadOnlyRow(
                new GUIContent(
                    "Other Presence Loss",
                    "Area-weighted state-validity rejection, minimum-state " +
                    "cutoff, and the signed residual against total clamp loss."),
                otherPresenceLoss);
            DrawReadOnlyRow(
                new GUIContent(
                    "Capacity Peaks",
                    "Maximum raw transported Presence and maximum excess " +
                    "above the cell's boundary-and-obstacle fluid capacity."),
                available
                    ? $"raw {runtime.TransportMaximumRawPresence:0.0000} / " +
                      $"local excess " +
                      $"{runtime.TransportMaximumLocalCapacityExcess:0.0000}"
                    : "—");
            DrawReadOnlyRow(
                new GUIContent(
                    "Capacity Hit Samples",
                    "Cell-substep samples. One cell may be counted more than " +
                    "once across CFL substeps. Category counts can overlap " +
                    "when the same sample exceeds both unit and local boundary " +
                    "capacity; Total is the union of all capacity-hit samples."),
                available
                    ? $"total {runtime.TransportTotalCapacityHitCount:N0} / " +
                      $"unit {runtime.TransportUnitCapacityHitCount:N0} / " +
                      $"boundary " +
                      $"{runtime.TransportBoundaryCapacityHitCount:N0} / " +
                      $"obstacle " +
                      $"{runtime.TransportObstacleCapacityHitCount:N0}"
                    : "—");
            DrawReadOnlyRow(
                new GUIContent(
                    "Targets & Review",
                    "The original capacity-loss value remains the engineering " +
                    "target. The temporary 1% review threshold records the " +
                    "accepted PoC deferral; it does not redefine transport as " +
                    "numerically corrected."),
                runtime != null
                    ? $"error {runtime.TransportConservationErrorGateRatio * 100f:0.000}% / " +
                      $"capacity target {runtime.TransportClampLossGateRatio * 100f:0.000}% / " +
                      $"deferred review {DeferredPresenceCapacityLossReviewRatio * 100f:0.000}%"
                    : unavailable);
        }

        private static string FormatTransportLoss(
            float loss,
            float reference)
        {
            float ratio = Mathf.Abs(loss) /
                Mathf.Max(0.000001f, Mathf.Abs(reference));
            return $"{loss:0.000000} ({ratio * 100f:0.000}%)";
        }

        private static string FormatSignedTransportLoss(
            float loss,
            float reference)
        {
            float ratio = loss /
                Mathf.Max(0.000001f, Mathf.Abs(reference));
            return loss.ToString(
                    "+0.000000;-0.000000;0.000000") +
                " (" +
                (ratio * 100f).ToString(
                    "+0.000;-0.000;0.000") +
                "%)";
        }

        private void DrawFoamDiagnosticLifecycle()
        {
            GetSingleFoamContext(
                out _,
                out StylizedRiverFoamRuntime runtime,
                out string unavailable);

            bool completed = runtime != null &&
                runtime.TopologyMetricsAvailable;
            bool visible = completed &&
                runtime.VisibleFoamPresenceArea > 0.0001f;
            string freshness = runtime != null &&
                runtime.TopologyMetricsFresh
                    ? string.Empty
                    : "stale ";

            DrawReadOnlyRow(
                new GUIContent("Lifetime Authority"),
                runtime != null
                    ? runtime.MaterialLifetimeAuthorityActive
                        ? "Remaining Life / full-field direct simulation"
                        : runtime.LifetimeAuthorityStatus
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Visible Life"),
                runtime == null
                    ? unavailable
                    : visible
                        ? $"{freshness}avg " +
                          $"{runtime.AverageVisibleRemainingLife:0.00}"
                        : completed
                            ? "Completed sample found no visible foam"
                            : "No completed sample");
            DrawReadOnlyRow(
                new GUIContent("Local Aging"),
                runtime == null
                    ? unavailable
                    : visible
                        ? $"{freshness}" +
                          $"{runtime.AverageLocalAgingRateUnderVisibleFoam:0.00}× avg"
                        : completed
                            ? "Completed sample found no visible foam"
                            : "No completed sample");
            DrawReadOnlyRow(
                new GUIContent("Topology Under Foam"),
                runtime == null
                    ? unavailable
                    : visible
                        ? $"support " +
                          $"{runtime.AveragePositiveSupportUnderVisibleFoam:0.00} / " +
                          $"negative " +
                          $"{runtime.AverageNegativeAgingUnderVisibleFoam:0.00}"
                        : completed
                            ? "Completed sample found no visible foam"
                            : "No completed sample");
            DrawReadOnlyRow(
                new GUIContent("Strongest Sample"),
                runtime == null
                    ? unavailable
                    : visible
                        ? $"support " +
                          $"{runtime.StrongestPositiveSupportUnderFoam:0.00} / " +
                          $"negative " +
                          $"{runtime.StrongestNegativeAgingUnderFoam:0.00}"
                        : completed
                            ? "Completed sample found no visible foam"
                            : "No completed sample");
            DrawReadOnlyRow(
                new GUIContent("Material Clock"),
                runtime != null
                    ? runtime.MaterialClockStatus
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Runtime Aging Parameters"),
                runtime != null
                    ? runtime.RuntimeAgingParameterStatus
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Probe Decay Check"),
                runtime != null
                    ? runtime.ProbeDecayCheckStatus
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Visible Life Range"),
                runtime != null
                    ? runtime.VisibleLifeRangeStatus
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Metric Freshness"),
                runtime == null
                    ? unavailable
                    : runtime.TopologyMetricsAgeSeconds < 0f
                        ? "No completed sample"
                        : runtime.TopologyMetricsFresh
                            ? $"{runtime.TopologyMetricsAgeSeconds:0.00} s old"
                            : $"Stale / " +
                              $"{runtime.TopologyMetricsAgeSeconds:0.00} s old");
        }

        private void DrawFoamDiagnosticBirthActivity()
        {
            GetSingleFoamContext(
                out _,
                out StylizedRiverFoamRuntime runtime,
                out string unavailable);

            DrawReadOnlyRow(
                new GUIContent("Overall Birth State"),
                runtime != null
                    ? runtime.BirthActivityStatus
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Shore Sources"),
                runtime != null
                    ? $"{runtime.AutomaticShoreBirthStatus} / " +
                      $"{runtime.AutomaticShoreBirthSubmittedLastUpdate:N0} submitted / " +
                      $"{runtime.AutomaticShoreBirthRejectedLastUpdate:N0} rejected"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Object Sources"),
                runtime != null
                    ? $"{runtime.AutomaticObjectBirthStatus} / " +
                      $"{runtime.AutomaticObjectBirthSubmittedLastUpdate:N0} submitted / " +
                      $"{runtime.AutomaticObjectBirthRejectedLastUpdate:N0} rejected"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Free-Water Sources"),
                runtime != null
                    ? $"{runtime.AutomaticFreeWaterBirthStatus} / " +
                      $"{runtime.AutomaticFreeWaterBirthSubmittedLastUpdate:N0} submitted / " +
                      $"{runtime.AutomaticFreeWaterBirthRejectedLastUpdate:N0} rejected"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Queued Injections"),
                runtime != null
                    ? runtime.PendingInjectionCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Injected Last Update"),
                runtime != null
                    ? runtime.InjectedLastUpdate.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Composition Events"),
                runtime != null
                    ? $"{runtime.ActiveFoamCompositionEventCount} active / " +
                      $"{runtime.FoamCompositionStartedCount} started / " +
                      $"{runtime.FoamCompositionCompletedCount} completed"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Birth Budget"),
                runtime != null
                    ? $"{runtime.FoamCompositionBirthBudgetPerStep} composition / " +
                      $"{runtime.AutomaticShoreBirthBudgetPerTick} shore / " +
                      $"{runtime.AutomaticObjectBirthBudgetPerTick} object / " +
                      $"{runtime.AutomaticFreeWaterBirthBudgetPerTick} free-water"
                    : unavailable);
        }

        private void DrawFoamDiagnosticProbeState()
        {
            GetSingleFoamContext(
                out _,
                out StylizedRiverFoamRuntime runtime,
                out string unavailable);

            DrawReadOnlyRow(
                new GUIContent("Recommended Debug View"),
                "Foam / Layer C / Material Remaining Life");
            DrawReadOnlyRow(
                new GUIContent("Probe State"),
                runtime != null
                    ? runtime.IsolatedLifeProbeStatus
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Decay Check"),
                runtime != null
                    ? runtime.ProbeDecayCheckStatus
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Absolute-Life Authority"),
                runtime != null
                    ? runtime.LifetimeProofStatus
                    : unavailable);
        }

        private void DrawFoamDiagnosticLayerD()
        {
            GetSingleFoamContext(
                out _,
                out StylizedRiverFoamRuntime runtime,
                out string unavailable);

            float hiddenArea = runtime != null
                ? Mathf.Max(
                    0f,
                    runtime.IntegratedPresenceArea -
                    runtime.VisiblePresenceCoreArea)
                : 0f;

            DrawReadOnlyRow(
                new GUIContent("Stored / Visible Area"),
                runtime != null
                    ? $"{runtime.IntegratedPresenceArea:0.000} m² / " +
                      $"{runtime.VisiblePresenceCoreArea:0.000} m²"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Hidden Stored Area"),
                runtime != null
                    ? $"{hiddenArea:0.000} m²"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Perimeter Ratio"),
                runtime != null
                    ? FormatPercent(runtime.PerimeterRatio)
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Manual Proof Ratio"),
                runtime != null
                    ? runtime.ManualProofReferenceAvailable
                        ? runtime.ManualProofPresenceRatio.ToString("0.00")
                        : "No manual proof"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Temporal Sheet State"),
                runtime != null
                    ? runtime.VisualOccupancyAvailable
                        ? "Half-resolution temporal occupancy available"
                        : "Unavailable"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Source / Support / Target"),
                runtime != null
                    ? runtime.VisualOccupancyAvailable
                        ? "Available when a Layer D diagnostic requests them"
                        : "Unavailable"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Shape Comparison State"),
                runtime != null
                    ? !runtime.ManualProofReferenceAvailable
                        ? "No manual proof reference"
                        : runtime.ManualProofPresenceRatio > 1.25f ||
                          runtime.ManualProofPresenceRatio < 0.65f
                            ? "Manual proof outside accepted tolerance"
                            : "Within accepted tolerance"
                    : unavailable);
        }

        private void DrawFoamDiagnosticResources()
        {
            GetSingleFoamContext(
                out _,
                out StylizedRiverFoamRuntime runtime,
                out string unavailable);

            DrawReadOnlyRow(
                new GUIContent("Runtime State"),
                runtime != null
                    ? runtime.enabled
                        ? runtime.IsSleeping ? "Sleeping" : "Active"
                        : "Component disabled"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Layer C State Textures"),
                runtime != null
                    ? runtime.FieldWidth > 0 && runtime.FieldHeight > 0
                        ? $"2 × ARGBHalf / {runtime.FieldWidth} × " +
                          $"{runtime.FieldHeight}"
                        : "Not allocated"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Structural Field"),
                runtime != null
                    ? runtime.StructuralWidth > 0 &&
                      runtime.StructuralHeight > 0
                        ? $"{runtime.StructuralWidth} × " +
                          $"{runtime.StructuralHeight}"
                        : "Not allocated"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Topology Field"),
                runtime != null
                    ? runtime.TopologyWidth > 0 &&
                      runtime.TopologyHeight > 0
                        ? $"{runtime.TopologyWidth} × " +
                          $"{runtime.TopologyHeight}"
                        : "Not allocated"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Layer D Shape Textures"),
                runtime != null
                    ? runtime.VisualOccupancyAvailable
                        ? "Shape + film source/support + occupancy A/B"
                        : "Not allocated"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Chunk State"),
                runtime != null
                    ? $"{runtime.ActiveChunkCount} active / " +
                      $"{runtime.ActiveReservationCount} reserved"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Dispatch State"),
                runtime != null
                    ? $"{runtime.LastUpdateDispatches:N0} last / " +
                      $"{runtime.RecentPeakDispatches:N0} peak"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Cell Iterations"),
                runtime != null
                    ? $"{runtime.LastUpdateCellIterations:N0} last / " +
                      $"{runtime.RecentPeakCellIterations:N0} peak"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Estimated Memory"),
                runtime != null
                    ? FormatMemoryBytes(runtime.EstimatedMemoryBytes)
                    : unavailable);
        }

        private void DrawFoamDiagnosticAdvanced()
        {
            GetSingleFoamContext(
                out _,
                out StylizedRiverFoamRuntime runtime,
                out string unavailable);

            DrawReadOnlyRow(
                new GUIContent("Topology Replacement"),
                runtime != null
                    ? $"{runtime.TopologyReplacementState} / ready " +
                      $"{(runtime.TopologyReplacementReady ? "yes" : "no")}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Topology Transition"),
                runtime != null
                    ? $"{runtime.TopologyTransitionState} / " +
                      $"{FormatPercent(runtime.TopologyTransitionProgress)}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Startup"),
                runtime != null
                    ? runtime.TopologyStartupValidationComplete
                        ? $"Complete / " +
                          $"{runtime.TopologyStartupTotalMilliseconds:0.000} ms"
                        : runtime.TopologyCacheStartupState
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Startup Slowest Step"),
                runtime != null
                    ? $"{runtime.TopologyStartupSlowestStep} / " +
                      $"{runtime.TopologyStartupSlowestStepMilliseconds:0.000} ms"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Cache Outcome / Reasons"),
                runtime != null
                    ? $"{runtime.TopologyCacheStartupOutcomeName} / " +
                      runtime.TopologyCacheStartupReasonNames
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Startup Phase Totals"),
                runtime != null
                    ? runtime.TopologyStartupPhaseSummary
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Registry Events"),
                runtime != null
                    ? $"add {runtime.TopologyStartupSourceAddedCount:N0} / " +
                      $"remove {runtime.TopologyStartupSourceRemovedCount:N0} / " +
                      $"change {runtime.TopologyStartupSourceChangedCount:N0} / " +
                      $"distinct {runtime.TopologyStartupDistinctSourceCount:N0}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Startup Restarts / Dirty Cycles"),
                runtime != null
                    ? $"{runtime.TopologyStartupRestartCount:N0} / " +
                      $"{runtime.TopologyStartupDirtyCycleCount:N0}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Startup Dirty Reasons"),
                runtime != null
                    ? runtime.TopologyStartupDirtyReasonNames
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Cache / Replacement Attempts"),
                runtime != null
                    ? $"build {runtime.TopologyStartupCacheBuildAttemptCount:N0} / " +
                      $"replace {runtime.TopologyStartupReplacementAttemptCount:N0}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Cache Hits / Misses"),
                runtime != null
                    ? $"{runtime.TopologyCacheStartupHitCount:N0} / " +
                      $"{runtime.TopologyCacheStartupMissCount:N0}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Play Cache Persistence"),
                runtime != null
                    ? $"{runtime.AutomaticTopologyCachePersistenceState} / " +
                      $"startup attempts " +
                      $"{runtime.TopologyStartupCacheWriteAttemptCount:N0} / " +
                      $"saved {runtime.TopologyStartupCacheWriteSuccessCount:N0}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Explicit Cache Build Work"),
                runtime != null
                    ? $"GPU publications " +
                      $"{runtime.TopologyCachePreparationGeneratedUploadCount:N0} / " +
                      $"serializations " +
                      $"{runtime.TopologyCacheLastBuildSerializationCount:N0}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Exhaustive Cache Proof"),
                runtime != null
                    ? $"{runtime.TopologyCacheRoundTripState} / " +
                      runtime.TopologyCacheRoundTripSummary
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Major Evolution"),
                runtime != null
                    ? $"{runtime.MajorEvolutionMovingCount} moving / " +
                      $"{runtime.MajorEvolutionDwellingCount} dwelling / " +
                      $"{runtime.MajorEvolutionRecycleCount} recycled"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Connector Evolution"),
                runtime != null
                    ? $"{runtime.ConnectorEvolutionActiveCount} active / " +
                      $"{runtime.ConnectorEvolutionTemporaryAbsenceCount} absent"
                    : unavailable);

            double workSeconds = runtime != null
                ? runtime.SteadyStateWorkElapsedSeconds
                : 0.0;
            double inverseWorkSeconds = workSeconds > 0.0001
                ? 1.0 / workSeconds
                : 0.0;
            double averageSubsteps = runtime != null &&
                runtime.SteadyStateWorkMaterialStepCount > 0
                    ? runtime.SteadyStateWorkTransportSubstepCount /
                      (double)runtime.SteadyStateWorkMaterialStepCount
                    : 0.0;
            double emptyStepPercent = runtime != null &&
                runtime.SteadyStateWorkMaterialStepCount > 0
                    ? runtime.SteadyStateWorkEmptyMaterialStepCount * 100.0 /
                      runtime.SteadyStateWorkMaterialStepCount
                    : 0.0;
            double topologyCpuMilliseconds = runtime != null
                ? runtime.SteadyStateWorkTopologyCpuMilliseconds +
                  runtime.SteadyStateWorkTopologyEvolutionCpuMilliseconds
                : 0.0;

            DrawReadOnlyRow(
                new GUIContent("P4 Work Window"),
                runtime != null
                    ? $"{(runtime.SteadyStateWorkAccountingActive ? "Active" : "Inactive")} / " +
                      $"{workSeconds:0.000}s / " +
                      $"{runtime.SteadyStateWorkFrameCount:N0} frames"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("P4 Dispatch / Cell Rate"),
                runtime != null
                    ? $"{runtime.SteadyStateWorkTotalDispatchCount * inverseWorkSeconds:0.00}/s / " +
                      $"{runtime.SteadyStateWorkTotalCellIterations * inverseWorkSeconds:0.00}/s"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("P4 Material Work"),
                runtime != null
                    ? $"{runtime.SteadyStateWorkMaterialStepCount * inverseWorkSeconds:0.00} steps/s / " +
                      $"substeps avg {averageSubsteps:0.00}, max " +
                      $"{runtime.SteadyStateWorkMaximumTransportSubsteps} / " +
                      $"CFL/substep max {runtime.SteadyStateWorkMaximumTransportCfl:0.000} / " +
                      $"CPU submit {runtime.SteadyStateWorkMaterialCpuMilliseconds:0.000} ms"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("P4 Material Dispatch / Cells"),
                runtime != null
                    ? $"{runtime.SteadyStateWorkMaterialDispatchCount:N0} / " +
                      $"{runtime.SteadyStateWorkMaterialCellIterations:N0}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("P4 Topology Work"),
                runtime != null
                    ? $"maintenance {runtime.SteadyStateWorkTopologyMaintenanceCount:N0} / " +
                      $"refresh {runtime.SteadyStateWorkTopologyRefreshCount:N0} / " +
                      $"evolution checks {runtime.SteadyStateWorkTopologyEvolutionCount:N0} / " +
                      $"dispatch {runtime.SteadyStateWorkTopologyDispatchCount:N0} / " +
                      $"cells {runtime.SteadyStateWorkTopologyCellIterations:N0} / " +
                      $"CPU submit {topologyCpuMilliseconds:0.000} ms"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("P4 Shape Debug Work"),
                runtime != null
                    ? $"eval {runtime.SteadyStateWorkShapeEvaluationCount:N0} / " +
                      $"dispatch {runtime.SteadyStateWorkShapeDispatchCount:N0} / " +
                      $"cells {runtime.SteadyStateWorkShapeCellIterations:N0} / " +
                      $"CPU submit {runtime.SteadyStateWorkShapeCpuMilliseconds:0.000} ms"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("P4 Dirty Checks"),
                runtime != null
                    ? $"{runtime.SteadyStateWorkTopologyDirtyEvaluationCount:N0} / " +
                      $"positive {runtime.SteadyStateWorkTopologyDirtyPositiveCount:N0}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("P4 Metric Readbacks"),
                runtime != null
                    ? $"topology {runtime.SteadyStateWorkTopologyMetricRequestCount:N0}/" +
                      $"{runtime.SteadyStateWorkTopologyMetricCompletionCount:N0}, " +
                      $"transport {runtime.SteadyStateWorkTransportMetricRequestCount:N0}/" +
                      $"{runtime.SteadyStateWorkTransportMetricCompletionCount:N0}"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("P4 Empty Material Steps"),
                runtime != null
                    ? $"{runtime.SteadyStateWorkEmptyMaterialStepCount:N0} / " +
                      $"{emptyStepPercent:0.0}% latest-metric qualified"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("P4 Visibility Frames"),
                runtime != null
                    ? $"visible {runtime.SteadyStateWorkVisibleFrameCount:N0} / " +
                      $"offscreen {runtime.SteadyStateWorkOffscreenFrameCount:N0}"
                    : unavailable);
        }

        private void GetSingleDisturbanceContext(
            out StylizedRiver river,
            out StylizedRiverDisturbanceRuntime runtime,
            out string unavailable)
        {
            river = targets.Length == 1
                ? target as StylizedRiver
                : null;
            runtime = river != null
                ? river.GetComponent<StylizedRiverDisturbanceRuntime>()
                : null;
            unavailable = GetSingleSelectionDiagnosticValue(
                river == null
                    ? "Select one river for live diagnostics."
                    : Application.isPlaying
                        ? "Runtime unavailable"
                        : "Not in Play Mode");
        }

        private void GetSingleFoamContext(
            out StylizedRiver river,
            out StylizedRiverFoamRuntime runtime,
            out string unavailable)
        {
            river = targets.Length == 1
                ? target as StylizedRiver
                : null;
            runtime = river != null
                ? river.GetComponent<StylizedRiverFoamRuntime>()
                : null;
            unavailable = GetSingleSelectionDiagnosticValue(
                river == null
                    ? "Select one river for live diagnostics."
                    : Application.isPlaying
                        ? "Runtime unavailable"
                        : "Not in Play Mode");
        }

        private string ResolveActiveDebugViewLabel()
        {
            SerializedProperty body = Find("bodyDebugView");
            SerializedProperty motion = Find("motionDebugView");
            SerializedProperty refraction = Find("refractionDebugView");
            SerializedProperty disturbance = Find("disturbanceDebugView");
            SerializedProperty foam = Find("foamDebugView");

            if (body == null || motion == null || refraction == null ||
                disturbance == null || foam == null)
            {
                return "Unavailable";
            }

            if (body.hasMultipleDifferentValues ||
                motion.hasMultipleDifferentValues ||
                refraction.hasMultipleDifferentValues ||
                disturbance.hasMultipleDifferentValues ||
                foam.hasMultipleDifferentValues)
            {
                return "Mixed across selected rivers";
            }

            RiverDebugState state = ReadDebugState(serializedObject);
            RiverDebugFeature feature =
                ResolveRenderedDebugFeature(state);
            return GetRenderedDebugViewLabel(
                feature,
                GetDebugViewValue(state, feature));
        }

        private static string ResolveFoamPrimaryWarning(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime,
            string unavailable)
        {
            if (river == null)
            {
                return unavailable;
            }

            if (!river.FoamEnabled)
            {
                return "Foam disabled";
            }

            if (runtime == null)
            {
                return unavailable;
            }

            if (!runtime.ResourcesAllocated)
            {
                return "Runtime resources are not allocated";
            }

            if (runtime.TransportSafetyLimitExceeded)
            {
                return runtime.TransportSafetyStatus;
            }

            if (runtime.TransportMetricsAvailable)
            {
                float capacityLossRatio =
                    runtime.TransportPresenceClampLossRatio;
                if (capacityLossRatio >
                    DeferredPresenceCapacityLossReviewRatio)
                {
                    return
                        $"Presence capacity loss " +
                        $"{capacityLossRatio * 100f:0.000}% exceeds " +
                        $"{DeferredPresenceCapacityLossReviewRatio * 100f:0.000}% " +
                        $"deferred review threshold";
                }

                if (capacityLossRatio >
                    runtime.TransportClampLossGateRatio)
                {
                    return
                        $"Deferred known limitation: capacity loss " +
                        $"{capacityLossRatio * 100f:0.000}% " +
                        $"(target " +
                        $"{runtime.TransportClampLossGateRatio * 100f:0.000}%)";
                }
            }

            float hiddenArea = Mathf.Max(
                0f,
                runtime.IntegratedPresenceArea -
                runtime.VisiblePresenceCoreArea);
            if (hiddenArea > Mathf.Max(
                    0.05f,
                    runtime.VisiblePresenceCoreArea * 0.25f))
            {
                return "High hidden stored material area";
            }

            if (runtime.TopologyMetricsAvailable &&
                !runtime.TopologyMetricsFresh)
            {
                return "Topology metric sample is stale";
            }

            return "None";
        }

        private string GetSingleSelectionDiagnosticValue(
            string fallback)
        {
            return string.IsNullOrEmpty(fallback)
                ? "—"
                : fallback;
        }

        private void DrawGeneratedStatusSection()
        {
            StylizedRiver river = targets.Length == 1
                ? target as StylizedRiver
                : null;
            string unavailable = river != null
                ? "—"
                : "Select one river for generated status.";

            bool hasGeneratedSurface =
                river != null && river.SurfaceTriangleCount > 0;
            bool hasGeneratedCorridor =
                river != null && river.CorridorTriangleCount > 0;
            bool hasReflection = river != null &&
                river.GetComponent<StylizedRiverPlanarReflection>() != null;
            Bounds surfaceBounds = default;
            bool hasSurfaceBounds = river != null &&
                river.TryGetSurfaceBounds(out surfaceBounds);

            Material bodyMaterial = null;
            SerializedProperty materialProperty = Find("bodyMaterial");
            if (materialProperty != null &&
                !materialProperty.hasMultipleDifferentValues)
            {
                bodyMaterial =
                    materialProperty.objectReferenceValue as Material;
            }

            string materialStatus;
            if (river == null)
            {
                materialStatus = unavailable;
            }
            else if (bodyMaterial == null)
            {
                materialStatus =
                    $"Default / {StylizedRiver.CompatibleShaderName}";
            }
            else if (bodyMaterial.shader != null &&
                bodyMaterial.shader.name == StylizedRiver.CompatibleShaderName)
            {
                materialStatus =
                    $"Compatible override / {bodyMaterial.name}";
            }
            else
            {
                materialStatus =
                    bodyMaterial.shader != null
                        ? $"Incompatible override / {bodyMaterial.shader.name}"
                        : "Override has no shader";
            }

            DrawReadOnlyRow(
                new GUIContent("Generation State"),
                river == null
                    ? unavailable
                    : hasGeneratedSurface && hasGeneratedCorridor
                        ? "Surface and corridor generated"
                        : hasGeneratedSurface
                            ? "Surface generated / corridor missing"
                            : "Not generated");
            DrawReadOnlyRow(
                new GUIContent("River Length"),
                river != null
                    ? $"{river.RiverLength:0.00} m"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Domain Version"),
                river != null
                    ? river.Domain.Version.ToString()
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Domain Samples"),
                river != null
                    ? river.Domain.SampleCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Global Distance"),
                river != null
                    ? $"{river.GlobalDistanceMinimum:0.00}–" +
                      $"{river.GlobalDistanceMaximum:0.00} m"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Average Surface Height"),
                river != null
                    ? $"{river.AverageSurfaceHeight:0.00} m"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Surface Triangles"),
                river != null
                    ? river.SurfaceTriangleCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Surface Bounds"),
                river == null
                    ? unavailable
                    : hasSurfaceBounds
                        ? $"centre {surfaceBounds.center.x:0.00}, " +
                          $"{surfaceBounds.center.y:0.00}, " +
                          $"{surfaceBounds.center.z:0.00} / size " +
                          $"{surfaceBounds.size.x:0.00} × " +
                          $"{surfaceBounds.size.y:0.00} × " +
                          $"{surfaceBounds.size.z:0.00} m"
                        : "Not generated");
            DrawReadOnlyRow(
                new GUIContent("Corridor Rings"),
                river != null
                    ? river.CorridorRingCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Corridor Across Vertices"),
                river != null
                    ? river.CorridorAcrossVertexCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Corridor Triangles"),
                river != null
                    ? river.CorridorTriangleCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Collider Triangles"),
                river != null
                    ? river.CorridorColliderTriangleCount.ToString("N0")
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Ground Height Source"),
                river != null
                    ? river.CorridorUsesGroundHeightField
                        ? "Generated base terrain"
                        : "Fallback"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Material Compatibility"),
                materialStatus);
            DrawReadOnlyRow(
                new GUIContent("Tight Bend Safety"),
                river != null
                    ? river.CorridorHasTightBendWarning
                        ? "Warning — inspect inner-bank pinching"
                        : "Within accepted geometry envelope"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("Deferred Reflection"),
                river != null
                    ? hasReflection
                        ? "Component present / ignored by production shader"
                        : "Not present"
                    : unavailable);
            DrawReadOnlyRow(
                new GUIContent("GameObject Layer"),
                river != null
                    ? LayerMask.LayerToName(river.gameObject.layer)
                    : unavailable);
        }

        private static string ResolveFoamTransportSmoothnessStatus(
            StylizedRiverFoamRuntime runtime)
        {
            if (runtime == null)
            {
                return "Runtime unavailable";
            }
            if (!runtime.ResourcesAllocated)
            {
                return "Resources not allocated";
            }
            if (runtime.VisiblePresenceCoreArea <= 0.0001f &&
                runtime.IntegratedPresenceArea <= 0.0001f)
            {
                return "No Foam material";
            }
            if (runtime.TransportSafetyLimitExceeded)
            {
                return "Transport blocked by CFL safety limit";
            }
            if (runtime.MaximumTransportCfl >
                runtime.TransportCflTarget + 0.0001f)
            {
                return "CFL target exceeded";
            }
            if (runtime.TransportMetricsAvailable &&
                (runtime.TransportPresenceUnaccountedErrorRatio >
                    runtime.TransportConservationErrorGateRatio ||
                 runtime.TransportLifeUnaccountedErrorRatio >
                    runtime.TransportConservationErrorGateRatio ||
                 runtime.TransportPatternUnaccountedErrorRatio >
                    runtime.TransportConservationErrorGateRatio))
            {
                return $"Conservation error exceeds " +
                    $"{runtime.TransportConservationErrorGateRatio * 100f:0.000}% gate";
            }
            if (runtime.TransportMetricsAvailable &&
                runtime.TransportPresenceClampLossRatio >
                    runtime.TransportClampLossGateRatio)
            {
                return $"Clamp loss exceeds " +
                    $"{runtime.TransportClampLossGateRatio * 100f:0.000}% gate";
            }
            if (runtime.MaterialStepsLastFrame > 1)
            {
                return "Simulation burst detected";
            }

            return "Conservative transport within displayed gates";
        }
    }
}
