#if UNITY_EDITOR
using System;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private const double P12MinimumAccountingSeconds = 5.0;
        private const long P12MinimumAccountingFrames = 30;

        /// <summary>
        /// Captures one comparable live P12 candidate snapshot. Visual
        /// acceptance remains a user judgement; this report owns only active
        /// descriptor, cache, resource, transport, temporal presentation,
        /// Motion Lane, and steady-state work evidence.
        /// </summary>
        public bool RunP12ActiveCandidateSnapshotReport()
        {
            topologyCacheDiagnosticRunCount++;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary =
                "Capturing the active P12 candidate runtime snapshot.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;

            if (!Application.isPlaying)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "The P12 active candidate snapshot is Play Mode only.");
            }

            river = GetComponent<StylizedRiver>();
            surfaceRenderer = river != null ? river.SurfaceRenderer : null;
            if (river == null || !river.FoamEnabled ||
                !river.Domain.IsValid)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "A valid Foam-enabled river is required.");
            }

            bool runtimeReady =
                initializationPhase == InitializationPhase.Ready &&
                AreResourcesCompleteAndCurrent();
            bool selectionExact = runtimeReady &&
                FoamGridSelectionMatchesActive;
            bool committedStateOwnershipExact = runtimeReady &&
                presentationPreviousState != null &&
                previousState == presentationPreviousState &&
                currentState != previousState &&
                writeState != previousState &&
                currentState != writeState;
            bool cacheReady = runtimeReady &&
                TopologyCacheLoadedForActiveResources;
            bool topologyReady = runtimeReady &&
                majorTopology != null &&
                connectorTopology != null &&
                pocketTopology != null;
            bool descriptorFinite = runtimeReady &&
                gridDescriptor.ColumnCount > 0 &&
                gridDescriptor.RowCount > 0 &&
                IsP12FinitePositive(gridDescriptor.ResolvedDxMetres) &&
                (!gridDescriptor.UsesFixedMetricLattice ||
                 IsP12FinitePositive(gridDescriptor.ResolvedDyMetres));
            bool transportFinite =
                IsP12FiniteNonNegative(TransportDownstreamStepCfl) &&
                IsP12FiniteNonNegative(TransportLateralStepCfl) &&
                IsP12FiniteNonNegative(TransportStepCfl) &&
                IsP12FiniteNonNegative(MinimumTransportRawJacobian) &&
                IsP12FiniteNonNegative(MinimumTransportBoundedJacobian) &&
                IsP12FiniteNonNegative(
                    MaximumAbsoluteTransportCurvatureLateralProduct);
            bool transportSafe = runtimeReady && transportFinite &&
                !TransportSafetyLimitExceeded &&
                TransportSubstepsUsed <= TransportSubstepLimit;
            bool accountingReady = SteadyStateWorkAccountingActive &&
                SteadyStateWorkElapsedSeconds >=
                    P12MinimumAccountingSeconds &&
                SteadyStateWorkFrameCount >=
                    P12MinimumAccountingFrames;

            StringBuilder report = new(16384);
            report.AppendLine(
                "RIVER FOAM FIXED-METRIC P12 ACTIVE CANDIDATE SNAPSHOT");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                "Operation: read-only Play Mode snapshot of the current active " +
                "Foam candidate. No cache, scene, prefab, material, or serialized " +
                "River state is written.");
            report.AppendLine(
                "Evidence boundary: machine-verifiable runtime/cache/transport/" +
                "presentation/lane/work evidence only. Visual acceptance remains " +
                "external.");
            report.AppendLine();

            report.AppendLine("AUTHORED / ACTIVE GRID SELECTION");
            report.AppendLine(
                $"Quality: {river.Quality}");
            report.AppendLine(
                $"Authored mode: {river.FoamGridMode}");
            report.AppendLine(
                $"Authored fixed size: {river.FoamFixedMetricCellSize}");
            report.AppendLine(
                $"Material contract: " +
                ResolveMaterialContractDiagnosticLabel(
                    river.FoamMaterialContract));
            report.AppendLine(
                $"Requested fixed spacing: " +
                $"{ResolveEffectiveFoamRequestedCellSizeMetres():0.000} m");
            report.AppendLine(
                $"Active mapping: {FoamGridMapping}");
            report.AppendLine(
                $"Descriptor contract: {FoamGridDescriptorContractVersion} / " +
                $"mapping {FoamGridMappingContractVersion}");
            report.AppendLine(
                $"Descriptor signature: {FoamGridInitializationSignature}");
            report.AppendLine(
                $"Structural: {StructuralWidth} x {StructuralHeight}");
            report.AppendLine(
                $"Film: {filmFieldWidth} x {filmFieldHeight}");
            report.AppendLine(
                $"Resolved spacing: dx={FoamGridResolvedDxMetres:0.000000} m / " +
                $"dy={FoamGridResolvedDyMetres:0.000000} m");
            report.AppendLine(
                $"Lateral rows: base={FoamGridGlobalYBase} / " +
                $"count={FoamGridRowCount} / " +
                $"extent={FoamGridRepresentedLateralMinimumMetres:0.000}..." +
                $"{FoamGridRepresentedLateralMaximumMetres:0.000} m");
            report.AppendLine(
                $"Committed-state textures distinct: " +
                $"{committedStateOwnershipExact}");
            report.AppendLine(
                "AUTHORED / ACTIVE VERDICT: " +
                (selectionExact && descriptorFinite &&
                 committedStateOwnershipExact
                    ? "PASS"
                    : "FAIL"));
            report.AppendLine();

            report.AppendLine("CACHE / INITIALIZATION");
            report.AppendLine(
                $"Runtime ready: {runtimeReady}");
            report.AppendLine(
                $"Startup state: {TopologyCacheStartupState}");
            report.AppendLine(
                $"Startup outcome: {TopologyCacheStartupOutcomeName}");
            report.AppendLine(
                $"Startup reasons: {TopologyCacheStartupReasonNames}");
            report.AppendLine(
                $"Cache installed for active resources: {cacheReady}");
            report.AppendLine(
                $"Cache payload: {TopologyCacheStartupPayloadBytes:N0} bytes / " +
                $"{TopologyCacheStartupPayloadHash}");
            report.AppendLine(
                $"Initialization: {TopologyStartupStepCount:N0} steps / " +
                $"{TopologyStartupTotalMilliseconds:0.000} ms / slowest " +
                $"{TopologyStartupSlowestStep} " +
                $"{TopologyStartupSlowestStepMilliseconds:0.000} ms");
            report.AppendLine(
                $"Topology ready: {topologyReady} / major " +
                $"{MajorAcceptedRegionCount:N0} / connectors " +
                $"{ConnectorAcceptedCount:N0} / pockets " +
                $"{PocketAcceptedCount:N0}");
            report.AppendLine(
                "CACHE / INITIALIZATION VERDICT: " +
                (runtimeReady && cacheReady && topologyReady
                    ? "PASS"
                    : "FAIL"));
            report.AppendLine();

            bool sourceOwnershipExact =
                ValidateP7AutomaticSourceOwnershipContracts(report);

            report.AppendLine("TRANSPORT / CURVILINEAR METRICS");
            report.AppendLine(
                $"CFL downstream/lateral/total: " +
                $"{TransportDownstreamStepCfl:0.000000} / " +
                $"{TransportLateralStepCfl:0.000000} / " +
                $"{TransportStepCfl:0.000000}");
            report.AppendLine(
                $"Substeps required/used/limit: " +
                $"{TransportSubstepsRequired:N0} / " +
                $"{TransportSubstepsUsed:N0} / " +
                $"{TransportSubstepLimit:N0}");
            report.AppendLine(
                $"CFL per used substep / target: " +
                $"{MaximumTransportCfl:0.000000} / " +
                $"{TransportCflTarget:0.000000}");
            report.AppendLine(
                $"Jacobian raw/bounded minimum: " +
                $"{MinimumTransportRawJacobian:0.000000} / " +
                $"{MinimumTransportBoundedJacobian:0.000000}");
            report.AppendLine(
                $"Maximum |curvature x lateral|: " +
                $"{MaximumAbsoluteTransportCurvatureLateralProduct:0.000000}");
            report.AppendLine(
                $"Safety: {TransportSafetyStatus} / " +
                $"limitExceeded={TransportSafetyLimitExceeded}");
            report.AppendLine(
                "TRANSPORT VERDICT: " +
                (transportSafe ? "PASS" : "FAIL"));
            report.AppendLine();

            report.AppendLine("PRESENTATION / MOTION-LANE EVIDENCE");
            report.AppendLine(
                $"Material cadence: {UpdateRate:0.###} Hz / " +
                $"step {MaterialStepDuration:0.000000} s");
            report.AppendLine(
                $"Committed-state interpolation alpha: current " +
                $"{RenderInterpolationAlpha:0.000000} / capture " +
                $"{SteadyStateWorkRenderInterpolationMinimum:0.000000}..." +
                $"{SteadyStateWorkRenderInterpolationMaximum:0.000000} / " +
                $"samples {SteadyStateWorkRenderInterpolationSampleCount:N0}");
            report.AppendLine(
                $"Authored speed ratios: downstream " +
                $"{river.FoamDownstreamSpeedRatio:0.000000} / " +
                $"maximum lateral {ResolveEffectiveFoamMaximumLateralSpeedRatio():0.000000} / " +
                $"lane advection {river.FoamLaneAdvectionRatio:0.000000}");
            report.AppendLine(
                $"Resolved speed ceiling: downstream " +
                $"{FoamBaseDownstreamSpeedMetresPerSecond:0.000000} m/s / " +
                $"lateral {FoamMaximumLateralSpeedMetresPerSecond:0.000000} m/s");
            report.AppendLine(
                $"Generated lane intent: range " +
                $"{FoamMotionLaneMinimumIntent:0.000000}..." +
                $"{FoamMotionLaneMaximumIntent:0.000000} / mean absolute " +
                $"{FoamMotionLaneMeanAbsoluteIntent:0.000000} / RMS " +
                $"{FoamMotionLaneRootMeanSquareIntent:0.000000}");
            report.AppendLine(
                $"Lane sign coverage (>0.15 / <-0.15 / near-neutral): " +
                $"{FoamMotionLanePositiveFraction:0.000000} / " +
                $"{FoamMotionLaneNegativeFraction:0.000000} / " +
                $"{FoamMotionLaneNearNeutralFraction:0.000000}");
            report.AppendLine(
                $"Lateral face intent: mean absolute " +
                $"{FoamMotionLaneMeanAbsoluteFaceIntent:0.000000} / RMS " +
                $"{FoamMotionLaneRootMeanSquareFaceIntent:0.000000} / " +
                $"opposing {FoamMotionLaneOpposingFaceFraction:0.000000} / " +
                $"cancellation {FoamMotionLaneFaceCancellationRatio:0.000000}");
            report.AppendLine(
                $"Material-weighted lateral transport: speed " +
                $"{TransportLateralMaterialWeightedSpeed:0.000000} m/s / " +
                $"positive movement {TransportLateralPositiveMovement:0.000000} " +
                $"m² Presence / negative movement " +
                $"{TransportLateralNegativeMovement:0.000000} m² Presence / " +
                $"metrics available {TransportMetricsAvailable}");
            float interpolationCaptureRange =
                SteadyStateWorkRenderInterpolationMaximum -
                SteadyStateWorkRenderInterpolationMinimum;
            bool presentationEvidence =
                SteadyStateWorkRenderInterpolationSampleCount > 1 &&
                IsP12FiniteNonNegative(RenderInterpolationAlpha) &&
                RenderInterpolationAlpha <= 1.000001f &&
                IsP12FiniteNonNegative(
                    SteadyStateWorkRenderInterpolationMinimum) &&
                IsP12FiniteNonNegative(
                    SteadyStateWorkRenderInterpolationMaximum) &&
                SteadyStateWorkRenderInterpolationMinimum <=
                    SteadyStateWorkRenderInterpolationMaximum + 0.000001f &&
                SteadyStateWorkRenderInterpolationMaximum <= 1.000001f &&
                interpolationCaptureRange >= 0.25f;
            float laneCoverage =
                FoamMotionLanePositiveFraction +
                FoamMotionLaneNegativeFraction +
                FoamMotionLaneNearNeutralFraction;
            bool laneEvidence =
                IsP12FiniteNonNegative(FoamMotionLaneMeanAbsoluteIntent) &&
                IsP12FiniteNonNegative(FoamMotionLaneRootMeanSquareIntent) &&
                FoamMotionLaneMinimumIntent >= -1.0001f &&
                FoamMotionLaneMaximumIntent <= 1.0001f &&
                FoamMotionLaneMinimumIntent <=
                    FoamMotionLaneMaximumIntent + 0.000001f &&
                FoamMotionLanePositiveFraction >= 0f &&
                FoamMotionLaneNegativeFraction >= 0f &&
                FoamMotionLaneNearNeutralFraction >= 0f &&
                Mathf.Abs(laneCoverage - 1f) <= 0.001f &&
                IsP12FiniteNonNegative(
                    FoamMotionLaneMeanAbsoluteFaceIntent) &&
                IsP12FiniteNonNegative(
                    FoamMotionLaneRootMeanSquareFaceIntent) &&
                FoamMotionLaneOpposingFaceFraction >= 0f &&
                FoamMotionLaneOpposingFaceFraction <= 1.000001f &&
                FoamMotionLaneFaceCancellationRatio >= 0f &&
                FoamMotionLaneFaceCancellationRatio <= 1.000001f &&
                TransportMetricsAvailable &&
                IsP12FiniteNonNegative(
                    TransportLateralMaterialWeightedSpeed) &&
                IsP12FiniteNonNegative(TransportLateralPositiveMovement) &&
                IsP12FiniteNonNegative(TransportLateralNegativeMovement);
            report.AppendLine(
                $"Interpolation capture range: " +
                $"{interpolationCaptureRange:0.000000} / required >= 0.250000");
            report.AppendLine(
                $"Lane coverage sum: {laneCoverage:0.000000}");
            report.AppendLine(
                "PRESENTATION / MOTION-LANE VERDICT: " +
                (presentationEvidence && laneEvidence ? "PASS" : "FAIL"));
            report.AppendLine();

            report.AppendLine("RESOURCE / STEADY-STATE WORK");
            report.AppendLine(
                $"Estimated persistent runtime memory: " +
                $"{EstimatedMemoryBytes:N0} bytes");
            report.AppendLine(
                $"Accounting active: {SteadyStateWorkAccountingActive}");
            report.AppendLine(
                $"Accounting window: {SteadyStateWorkElapsedSeconds:0.000} s / " +
                $"{SteadyStateWorkFrameCount:N0} frames");
            report.AppendLine(BuildSteadyStateWorkSummary());
            report.AppendLine(
                "STEADY-STATE WORK VERDICT: " +
                (accountingReady ? "PASS" : "FAIL"));
            if (!accountingReady)
            {
                report.AppendLine(
                    $"Required minimum: {P12MinimumAccountingSeconds:0.0} s " +
                    $"and {P12MinimumAccountingFrames:N0} frames after " +
                    "Start / Reset P12 Candidate Capture.");
            }
            report.AppendLine();

            bool overallRuntime = selectionExact && descriptorFinite &&
                committedStateOwnershipExact && runtimeReady && cacheReady &&
                topologyReady &&
                sourceOwnershipExact && transportSafe && presentationEvidence &&
                laneEvidence && accountingReady;
            report.AppendLine("FINAL LEDGER");
            report.AppendLine(
                "Authored selection matches active descriptor: " +
                (selectionExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Descriptor dimensions and spacing are finite: " +
                (descriptorFinite ? "PASS" : "FAIL"));
            report.AppendLine(
                "Committed presentation state is distinct: " +
                (committedStateOwnershipExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Matching cache installed and topology ready: " +
                (runtimeReady && cacheReady && topologyReady
                    ? "PASS"
                    : "FAIL"));
            report.AppendLine(
                "Hybrid automatic-source ownership: " +
                (sourceOwnershipExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Transport/CFL/curvature safety: " +
                (transportSafe ? "PASS" : "FAIL"));
            report.AppendLine(
                "Committed-state interpolation and lane evidence: " +
                (presentationEvidence && laneEvidence ? "PASS" : "FAIL"));
            report.AppendLine(
                "Comparable steady-state work window: " +
                (accountingReady ? "PASS" : "FAIL"));
            report.AppendLine(
                "Visual candidate verdict: USER REVIEW REQUIRED");
            report.AppendLine(
                "Overall runtime evidence: " +
                (overallRuntime ? "PASS" : "FAIL"));

            topologyCacheDiagnosticReport = report.ToString();
            if (!TryWriteLatestDiagnosticReport(
                    "LatestP12CandidateSnapshot",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string writeError))
            {
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    "P12 runtime evidence was captured, but the report file " +
                    "could not be written: " + writeError;
                Debug.LogError(
                    "[River Foam P12] " + topologyCacheDiagnosticSummary,
                    river);
                return false;
            }

            topologyCacheDiagnosticState = overallRuntime
                ? "Passed"
                : "Failed";
            topologyCacheDiagnosticSummary = overallRuntime
                ? "P12 active candidate runtime evidence passed. Visual review " +
                  "is still required."
                : "P12 active candidate runtime evidence failed. See the " +
                  "latest report.";
            if (overallRuntime)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log(
                    "[River Foam P12] PASS — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            else
            {
                Debug.LogError(
                    "[River Foam P12] FAIL — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }

            return overallRuntime;
        }

        private static bool IsP12FinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }

        private static bool IsP12FiniteNonNegative(float value)
        {
            return value >= 0f && !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }
    }
}
#endif
