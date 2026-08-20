using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private float ResolveEffectiveFoamRequestedCellSizeMetres()
        {
#if UNITY_EDITOR
            if (p12CandidateSweepUseCellSizeOverride)
            {
                return p12CandidateSweepRequestedCellSizeMetres;
            }
#endif
            return river != null
                ? river.FoamFixedMetricRequestedCellSizeMetres
                : 0f;
        }

        private float ResolveEffectiveFoamMaximumLateralSpeedRatio()
        {
#if UNITY_EDITOR
            if (p12CandidateSweepUseLateralRatioOverride)
            {
                return p12CandidateSweepMaximumLateralSpeedRatio;
            }
#endif
            return river != null
                ? river.FoamMaximumLateralSpeedRatio
                : 0f;
        }

        private float ResolveEffectiveFoamInitializationMotionTime()
        {
#if UNITY_EDITOR
            if (p12CandidateSweepActive)
            {
                return p12CandidateSweepInitializationMotionTime;
            }
#endif
            return river != null ? river.MotionTime : 0f;
        }

        private bool P12CandidateSweepForcesRuntimeWork
        {
            get
            {
#if UNITY_EDITOR
                return p12CandidateSweepActive;
#else
                return false;
#endif
            }
        }

        private bool P12CandidateSweepAllowsTransientTopologyGeneration
        {
            get
            {
#if UNITY_EDITOR
                return p12CandidateSweepActive &&
                    p12CandidateSweepPhase !=
                        P12CandidateSweepPhase.Restoring;
#else
                return false;
#endif
            }
        }

        private bool P12CandidateSweepTransientTopologyGenerationActive
        {
            get
            {
#if UNITY_EDITOR
                return p12CandidateSweepTransientTopologyGenerationInProgress;
#else
                return false;
#endif
            }
        }

        private void BeginP12CandidateSweepTransientTopologyGeneration()
        {
#if UNITY_EDITOR
            p12CandidateSweepTransientTopologyGenerationInProgress = true;
#endif
        }

        private bool CompleteP12CandidateSweepTransientTopologyGeneration()
        {
#if UNITY_EDITOR
            if (!p12CandidateSweepTransientTopologyGenerationInProgress)
            {
                return false;
            }

            p12CandidateSweepTransientTopologyGenerationInProgress = false;
            return true;
#else
            return false;
#endif
        }

        private void AdvanceP12CandidateSweepFrame(bool runtimeReady)
        {
#if UNITY_EDITOR
            if (!p12CandidateSweepActive)
            {
                return;
            }

            AdvanceP12CandidateSweep(runtimeReady);
#endif
        }

#if UNITY_EDITOR
        private const double P12SweepWarmupSeconds = 2.0;
        private const double P12SweepCaptureSeconds = 5.0;
        private const double P12SweepMetricSettleTimeoutSeconds = 3.0;
        private const double P12SweepInitializationTimeoutSeconds = 30.0;
        private const double P12SweepRestoreTimeoutSeconds = 30.0;
        private const int P12SweepSpacingCount = 4;
        private const int P12SweepLateralCaseCount = 3;
        private const int P12SweepTotalCaseCount =
            P12SweepSpacingCount * P12SweepLateralCaseCount;
        private const float P12SweepZeroLateralTolerance = 0.00001f;

        private static readonly float[] P12SweepRequestedSpacings =
        {
            0.25f,
            0.20f,
            0.15f,
            0.10f
        };

        private enum P12CandidateSweepPhase
        {
            Idle,
            InitializingCandidate,
            WarmingCase,
            CapturingCase,
            WaitingForMetrics,
            Restoring,
            Complete,
            Failed,
            Cancelled
        }

        private sealed class P12CandidateSweepCaseResult
        {
            public int CaseIndex;
            public int SpacingIndex;
            public int LateralIndex;
            public float RequestedSpacing;
            public float ResolvedDx;
            public float ResolvedDy;
            public float LateralRatio;
            public int StructuralWidth;
            public int StructuralHeight;
            public int FilmWidth;
            public int FilmHeight;
            public long StructuralCells;
            public long EstimatedMemoryBytes;
            public string DescriptorSignature;
            public string TopologySource;
            public double InitializationMilliseconds;
            public int InitializationSteps;
            public string InitializationSlowestStep;
            public double InitializationSlowestMilliseconds;
            public int MajorCount;
            public int ConnectorCount;
            public int PocketCount;
            public float DownstreamCfl;
            public float LateralCfl;
            public float TotalCfl;
            public int RequiredSubsteps;
            public int UsedSubsteps;
            public float RawJacobian;
            public float BoundedJacobian;
            public float MaximumCurvatureLateral;
            public float CellMeanAbsoluteIntent;
            public float FaceMeanAbsoluteIntent;
            public float FaceRmsIntent;
            public float OpposingFaceFraction;
            public float FaceCancellationRatio;
            public float MaterialWeightedLateralSpeed;
            public float PositiveLateralMovement;
            public float NegativeLateralMovement;
            public double CaptureSeconds;
            public long CaptureFrames;
            public long Dispatches;
            public long CellIterations;
            public long MaterialSteps;
            public double MaterialCpuMilliseconds;
            public double TopologyCpuMilliseconds;
            public string WorkSummary;
            public bool RuntimeReady;
            public bool DescriptorExact;
            public bool TopologyReady;
            public bool TransportSafe;
            public bool AccountingReady;
            public bool LaneEvidenceReady;
            public bool ZeroRatioIsolated;
            public bool Passed;
        }

        private bool p12CandidateSweepActive;
        private bool p12CandidateSweepUseCellSizeOverride;
        private bool p12CandidateSweepUseLateralRatioOverride;
        private bool p12CandidateSweepTransientTopologyGenerationInProgress;
        private float p12CandidateSweepRequestedCellSizeMetres;
        private float p12CandidateSweepMaximumLateralSpeedRatio;
        private float p12CandidateSweepAuthoredLateralRatio;
        private float p12CandidateSweepInitializationMotionTime;
        private int p12CandidateSweepSpacingIndex;
        private int p12CandidateSweepLateralIndex;
        private int p12CandidateSweepCompletedCaseCount;
        private double p12CandidateSweepPhaseStartedAt;
        private P12CandidateSweepPhase p12CandidateSweepPhase =
            P12CandidateSweepPhase.Idle;
        private P12CandidateSweepPhase p12CandidateSweepTerminalPhase =
            P12CandidateSweepPhase.Idle;
        private string p12CandidateSweepStatus = "Not Run";
        private string p12CandidateSweepSummary =
            "Run the complete P12 candidate sweep in Play Mode.";
        private string p12CandidateSweepReport = string.Empty;
        private string p12CandidateSweepReportPath = string.Empty;
        private readonly List<P12CandidateSweepCaseResult>
            p12CandidateSweepResults = new(P12SweepTotalCaseCount);
        private StylizedRiverFoamTopologyCacheAsset
            p12CandidateSweepAssignedCacheAsset;
        private string p12CandidateSweepAssignedCacheMetadataBefore =
            "<none>";
        private byte[] p12CandidateSweepAssignedCachePayloadBefore =
            Array.Empty<byte>();
        private string p12CandidateSweepTerminalReason = string.Empty;

        public bool P12CandidateSweepActive => p12CandidateSweepActive;
        public string P12CandidateSweepStatus => p12CandidateSweepStatus;
        public string P12CandidateSweepSummary => p12CandidateSweepSummary;
        public string P12CandidateSweepReport => p12CandidateSweepReport;
        public string P12CandidateSweepReportPath =>
            p12CandidateSweepReportPath;
        public float P12CandidateSweepProgress
        {
            get
            {
                if (!p12CandidateSweepActive)
                {
                    return p12CandidateSweepPhase ==
                        P12CandidateSweepPhase.Complete
                            ? 1f
                            : 0f;
                }

                float phaseFraction = p12CandidateSweepPhase switch
                {
                    P12CandidateSweepPhase.InitializingCandidate => 0.05f,
                    P12CandidateSweepPhase.WarmingCase =>
                        ResolveP12SweepTimedPhaseFraction(
                            P12SweepWarmupSeconds) * 0.20f,
                    P12CandidateSweepPhase.CapturingCase =>
                        0.20f + ResolveP12SweepTimedPhaseFraction(
                            P12SweepCaptureSeconds) * 0.70f,
                    P12CandidateSweepPhase.WaitingForMetrics => 0.95f,
                    P12CandidateSweepPhase.Restoring => 0.99f,
                    _ => 0f
                };
                return Mathf.Clamp01(
                    (p12CandidateSweepCompletedCaseCount + phaseFraction) /
                    P12SweepTotalCaseCount);
            }
        }

        public bool StartP12CandidateSweep()
        {
            if (!Application.isPlaying)
            {
                p12CandidateSweepStatus = "Unavailable";
                p12CandidateSweepSummary =
                    "The complete P12 candidate sweep is Play Mode only.";
                return false;
            }

            river = GetComponent<StylizedRiver>();
            surfaceRenderer = river != null ? river.SurfaceRenderer : null;
            if (p12CandidateSweepActive || river == null ||
                !river.FoamEnabled || !river.Domain.IsValid || !IsSupported ||
                river.FoamGridMode != StylizedRiverFoamGridMode.FixedMetric ||
                river.FoamStateHeld || river.FreezeAmount >= 0.999f)
            {
                p12CandidateSweepStatus = "Unavailable";
                if (p12CandidateSweepActive)
                {
                    p12CandidateSweepSummary =
                        "A P12 candidate sweep is already running.";
                }
                else if (river != null && river.FoamStateHeld)
                {
                    p12CandidateSweepSummary =
                        "Release Hold Foam State before running the sweep.";
                }
                else if (river != null && river.FreezeAmount >= 0.999f)
                {
                    p12CandidateSweepSummary =
                        "Unfreeze the River before running the sweep.";
                }
                else if (river != null && river.FoamGridMode !=
                         StylizedRiverFoamGridMode.FixedMetric)
                {
                    p12CandidateSweepSummary =
                        "Select Fixed Metric Grid Mode before running the sweep.";
                }
                else
                {
                    p12CandidateSweepSummary =
                        "A valid supported Foam-enabled river is required.";
                }
                return false;
            }

            topologyCacheDiagnosticRunCount++;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary =
                "Running the complete P12 fixed-spacing and lateral-response " +
                "candidate sweep.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;

            p12CandidateSweepAssignedCacheAsset =
                river.FoamTopologyCacheAsset;
            p12CandidateSweepAssignedCacheMetadataBefore =
                p12CandidateSweepAssignedCacheAsset != null
                    ? BuildAssignedCacheMetadataSignature(
                        p12CandidateSweepAssignedCacheAsset)
                    : "<none>";
            p12CandidateSweepAssignedCachePayloadBefore =
                p12CandidateSweepAssignedCacheAsset != null &&
                p12CandidateSweepAssignedCacheAsset.HasPayload
                    ? p12CandidateSweepAssignedCacheAsset.GetPayloadCopy()
                    : Array.Empty<byte>();

            p12CandidateSweepActive = true;
            p12CandidateSweepUseCellSizeOverride = true;
            p12CandidateSweepUseLateralRatioOverride = true;
            p12CandidateSweepTransientTopologyGenerationInProgress = false;
            p12CandidateSweepAuthoredLateralRatio =
                river.FoamMaximumLateralSpeedRatio;
            p12CandidateSweepInitializationMotionTime = river.MotionTime;
            p12CandidateSweepSpacingIndex = 0;
            p12CandidateSweepLateralIndex = 0;
            p12CandidateSweepCompletedCaseCount = 0;
            p12CandidateSweepResults.Clear();
            p12CandidateSweepReport = string.Empty;
            p12CandidateSweepReportPath = string.Empty;
            p12CandidateSweepTerminalReason = string.Empty;
            p12CandidateSweepTerminalPhase = P12CandidateSweepPhase.Idle;
            p12CandidateSweepStatus = "Running";
            p12CandidateSweepSummary =
                "Preparing spacing 0.25 m, lateral ratio 0.000.";

            ApplyP12SweepSpacingCandidate();
            return true;
        }

        public void CancelP12CandidateSweep()
        {
            if (!p12CandidateSweepActive)
            {
                return;
            }

            p12CandidateSweepTerminalReason =
                "Cancelled by the explicit Inspector action.";
            BeginP12CandidateSweepRestore(
                P12CandidateSweepPhase.Cancelled);
        }

        private void ApplyP12SweepSpacingCandidate()
        {
            p12CandidateSweepRequestedCellSizeMetres =
                P12SweepRequestedSpacings[p12CandidateSweepSpacingIndex];
            p12CandidateSweepMaximumLateralSpeedRatio =
                ResolveP12SweepLateralRatio(p12CandidateSweepLateralIndex);
            p12CandidateSweepTransientTopologyGenerationInProgress = false;
            p12CandidateSweepPhase =
                P12CandidateSweepPhase.InitializingCandidate;
            p12CandidateSweepPhaseStartedAt =
                Time.realtimeSinceStartupAsDouble;
            p12CandidateSweepSummary =
                $"Initializing spacing " +
                $"{p12CandidateSweepRequestedCellSizeMetres:0.00} m " +
                "through the real fixed-metric allocation and transient " +
                "topology path.";

            ClearSteadyStateWorkAccounting(false);
            ReleaseResources();
            resourcesDirty = true;
            boundaryDirty = true;
            initializationPhase = InitializationPhase.NotStarted;
            topologyStartupValidationComplete = false;
            lastRuntimeTime = Time.realtimeSinceStartup;
        }

        private void BeginP12SweepCase()
        {
            p12CandidateSweepMaximumLateralSpeedRatio =
                ResolveP12SweepLateralRatio(p12CandidateSweepLateralIndex);
            ResetP12SweepSimulationState();
            p12CandidateSweepPhase = P12CandidateSweepPhase.WarmingCase;
            p12CandidateSweepPhaseStartedAt =
                Time.realtimeSinceStartupAsDouble;
            p12CandidateSweepSummary =
                $"Warming case {p12CandidateSweepCompletedCaseCount + 1}/" +
                $"{P12SweepTotalCaseCount}: " +
                $"{p12CandidateSweepRequestedCellSizeMetres:0.00} m / " +
                $"lateral {p12CandidateSweepMaximumLateralSpeedRatio:0.000}.";
        }

        private void ResetP12SweepSimulationState()
        {
            ClearSteadyStateWorkAccounting(false);
            ClearFoam();
            automaticShoreBirthCursor = 0;
            automaticObjectBirthAccumulator = 0f;
            automaticObjectBirthCursor = 0;
            automaticFreeWaterBirthAccumulator = 0f;
            automaticFreeWaterBirthCursor = 0;
            automaticObjectContactCycleTime = 0f;
            automaticObjectPatternAuthoritySignature = int.MinValue;
            motionLaneScrollCells = 0f;
            lastMotionLaneScrollCells = 0f;
            lastRuntimeTime = Time.realtimeSinceStartup;
        }

        private void AdvanceP12CandidateSweep(bool runtimeReady)
        {
            double now = Time.realtimeSinceStartupAsDouble;
            switch (p12CandidateSweepPhase)
            {
                case P12CandidateSweepPhase.InitializingCandidate:
                    if (runtimeReady &&
                        initializationPhase == InitializationPhase.Ready &&
                        AreResourcesCompleteAndCurrent())
                    {
                        BeginP12SweepCase();
                    }
                    else if (now - p12CandidateSweepPhaseStartedAt >=
                             P12SweepInitializationTimeoutSeconds)
                    {
                        FailP12CandidateSweep(
                            "Candidate initialization exceeded 30 seconds " +
                            $"at {p12CandidateSweepRequestedCellSizeMetres:0.00} m " +
                            $"(phase {initializationPhase}).");
                    }
                    break;

                case P12CandidateSweepPhase.WarmingCase:
                    if (!runtimeReady)
                    {
                        FailP12CandidateSweep(
                            "Runtime resources became unavailable during the " +
                            "case warmup.");
                    }
                    else if (now - p12CandidateSweepPhaseStartedAt >=
                             P12SweepWarmupSeconds)
                    {
                        ResetSteadyStateWorkAccounting();
                        p12CandidateSweepPhase =
                            P12CandidateSweepPhase.CapturingCase;
                        p12CandidateSweepPhaseStartedAt = now;
                        p12CandidateSweepSummary =
                            $"Capturing case " +
                            $"{p12CandidateSweepCompletedCaseCount + 1}/" +
                            $"{P12SweepTotalCaseCount}: " +
                            $"{p12CandidateSweepRequestedCellSizeMetres:0.00} m / " +
                            $"lateral {p12CandidateSweepMaximumLateralSpeedRatio:0.000}.";
                    }
                    break;

                case P12CandidateSweepPhase.CapturingCase:
                    if (!runtimeReady)
                    {
                        FailP12CandidateSweep(
                            "Runtime resources became unavailable during the " +
                            "case capture.");
                    }
                    else if (SteadyStateWorkElapsedSeconds >=
                                 P12MinimumAccountingSeconds &&
                             SteadyStateWorkFrameCount >=
                                 P12MinimumAccountingFrames)
                    {
                        if (TransportMetricsAvailable)
                        {
                            CompleteP12SweepCase();
                        }
                        else
                        {
                            p12CandidateSweepPhase =
                                P12CandidateSweepPhase.WaitingForMetrics;
                            p12CandidateSweepPhaseStartedAt = now;
                            p12CandidateSweepSummary =
                                "Waiting for the final transport metric " +
                                "readback for the current case.";
                        }
                    }
                    break;

                case P12CandidateSweepPhase.WaitingForMetrics:
                    if (!runtimeReady)
                    {
                        FailP12CandidateSweep(
                            "Runtime resources became unavailable while " +
                            "waiting for transport evidence.");
                    }
                    else if (TransportMetricsAvailable)
                    {
                        CompleteP12SweepCase();
                    }
                    else if (now - p12CandidateSweepPhaseStartedAt >=
                             P12SweepMetricSettleTimeoutSeconds)
                    {
                        FailP12CandidateSweep(
                            "Transport metrics were not available within the " +
                            "three-second settle window.");
                    }
                    break;

                case P12CandidateSweepPhase.Restoring:
                {
                    bool authoredOwnershipRestored =
                        !p12CandidateSweepUseCellSizeOverride &&
                        !p12CandidateSweepUseLateralRatioOverride &&
                        !p12CandidateSweepTransientTopologyGenerationInProgress;
                    bool authoredRuntimeReady = runtimeReady &&
                        FoamGridSelectionMatchesActive;
                    bool normalPreparationRequired =
                        initializationPhase ==
                            InitializationPhase.CachePreparationRequired;
                    if (authoredOwnershipRestored &&
                        (authoredRuntimeReady || normalPreparationRequired))
                    {
                        FinalizeP12CandidateSweep(
                            authoredRuntimeReady,
                            normalPreparationRequired);
                    }
                    else if (now - p12CandidateSweepPhaseStartedAt >=
                             P12SweepRestoreTimeoutSeconds)
                    {
                        p12CandidateSweepTerminalReason =
                            "The authored runtime did not restore within 30 " +
                            $"seconds (phase {initializationPhase}).";
                        FinalizeP12CandidateSweep(false, false);
                    }
                    break;
                }
            }
        }

        private void CompleteP12SweepCase()
        {
            bool runtimeReady =
                initializationPhase == InitializationPhase.Ready &&
                AreResourcesCompleteAndCurrent();
            bool descriptorExact = runtimeReady &&
                gridDescriptor.UsesFixedMetricLattice &&
                Mathf.Approximately(
                    gridDescriptor.RequestedDxMetres,
                    p12CandidateSweepRequestedCellSizeMetres);
            bool topologyReady = runtimeReady && majorTopology != null &&
                connectorTopology != null && pocketTopology != null;
            bool transportSafe = runtimeReady &&
                IsP12FiniteNonNegative(TransportDownstreamStepCfl) &&
                IsP12FiniteNonNegative(TransportLateralStepCfl) &&
                IsP12FiniteNonNegative(TransportStepCfl) &&
                IsP12FiniteNonNegative(MinimumTransportRawJacobian) &&
                IsP12FiniteNonNegative(MinimumTransportBoundedJacobian) &&
                IsP12FiniteNonNegative(
                    MaximumAbsoluteTransportCurvatureLateralProduct) &&
                !TransportSafetyLimitExceeded &&
                TransportSubstepsUsed <= TransportSubstepLimit;
            bool accountingReady = SteadyStateWorkAccountingActive &&
                SteadyStateWorkElapsedSeconds >=
                    P12MinimumAccountingSeconds &&
                SteadyStateWorkFrameCount >= P12MinimumAccountingFrames;
            bool laneEvidenceReady = TransportMetricsAvailable &&
                IsP12FiniteNonNegative(
                    FoamMotionLaneMeanAbsoluteFaceIntent) &&
                IsP12FiniteNonNegative(
                    FoamMotionLaneRootMeanSquareFaceIntent) &&
                IsP12FiniteNonNegative(
                    TransportLateralMaterialWeightedSpeed) &&
                IsP12FiniteNonNegative(TransportLateralPositiveMovement) &&
                IsP12FiniteNonNegative(TransportLateralNegativeMovement);
            bool zeroRatioIsolated =
                p12CandidateSweepMaximumLateralSpeedRatio >
                    P12SweepZeroLateralTolerance ||
                (TransportLateralMaterialWeightedSpeed <=
                     P12SweepZeroLateralTolerance &&
                 TransportLateralPositiveMovement <=
                     P12SweepZeroLateralTolerance &&
                 TransportLateralNegativeMovement <=
                     P12SweepZeroLateralTolerance);

            var result = new P12CandidateSweepCaseResult
            {
                CaseIndex = p12CandidateSweepCompletedCaseCount,
                SpacingIndex = p12CandidateSweepSpacingIndex,
                LateralIndex = p12CandidateSweepLateralIndex,
                RequestedSpacing =
                    p12CandidateSweepRequestedCellSizeMetres,
                ResolvedDx = FoamGridResolvedDxMetres,
                ResolvedDy = FoamGridResolvedDyMetres,
                LateralRatio =
                    p12CandidateSweepMaximumLateralSpeedRatio,
                StructuralWidth = structuralWidth,
                StructuralHeight = structuralHeight,
                FilmWidth = filmFieldWidth,
                FilmHeight = filmFieldHeight,
                StructuralCells =
                    (long)StructuralWidth * StructuralHeight,
                EstimatedMemoryBytes = this.EstimatedMemoryBytes,
                DescriptorSignature = FoamGridInitializationSignature,
                TopologySource = topologyCacheLoadedForActiveResources
                    ? "Assigned cache"
                    : "Transient P12 sweep generation",
                InitializationMilliseconds =
                    topologyStartupValidationTotalMilliseconds,
                InitializationSteps = topologyStartupValidationStepCount,
                InitializationSlowestStep =
                    topologyStartupValidationSlowestStep,
                InitializationSlowestMilliseconds =
                    topologyStartupValidationSlowestStepMilliseconds,
                MajorCount = majorTopology != null
                    ? majorTopology.AcceptedRegionCount
                    : 0,
                ConnectorCount = connectorTopology != null
                    ? connectorTopology.AcceptedConnectorCount
                    : 0,
                PocketCount = pocketTopology != null
                    ? pocketTopology.AcceptedPocketCount
                    : 0,
                DownstreamCfl = TransportDownstreamStepCfl,
                LateralCfl = TransportLateralStepCfl,
                TotalCfl = TransportStepCfl,
                RequiredSubsteps = TransportSubstepsRequired,
                UsedSubsteps = TransportSubstepsUsed,
                RawJacobian = MinimumTransportRawJacobian,
                BoundedJacobian = MinimumTransportBoundedJacobian,
                MaximumCurvatureLateral =
                    MaximumAbsoluteTransportCurvatureLateralProduct,
                CellMeanAbsoluteIntent =
                    FoamMotionLaneMeanAbsoluteIntent,
                FaceMeanAbsoluteIntent =
                    FoamMotionLaneMeanAbsoluteFaceIntent,
                FaceRmsIntent =
                    FoamMotionLaneRootMeanSquareFaceIntent,
                OpposingFaceFraction =
                    FoamMotionLaneOpposingFaceFraction,
                FaceCancellationRatio =
                    FoamMotionLaneFaceCancellationRatio,
                MaterialWeightedLateralSpeed =
                    TransportLateralMaterialWeightedSpeed,
                PositiveLateralMovement =
                    TransportLateralPositiveMovement,
                NegativeLateralMovement =
                    TransportLateralNegativeMovement,
                CaptureSeconds = SteadyStateWorkElapsedSeconds,
                CaptureFrames = SteadyStateWorkFrameCount,
                Dispatches = SteadyStateWorkTotalDispatchCount,
                CellIterations = SteadyStateWorkTotalCellIterations,
                MaterialSteps = SteadyStateWorkMaterialStepCount,
                MaterialCpuMilliseconds =
                    SteadyStateWorkMaterialCpuMilliseconds,
                TopologyCpuMilliseconds =
                    SteadyStateWorkTopologyCpuMilliseconds,
                WorkSummary = BuildSteadyStateWorkSummary(),
                RuntimeReady = runtimeReady,
                DescriptorExact = descriptorExact,
                TopologyReady = topologyReady,
                TransportSafe = transportSafe,
                AccountingReady = accountingReady,
                LaneEvidenceReady = laneEvidenceReady,
                ZeroRatioIsolated = zeroRatioIsolated,
                Passed = runtimeReady && descriptorExact && topologyReady &&
                    transportSafe && accountingReady && laneEvidenceReady &&
                    zeroRatioIsolated
            };
            p12CandidateSweepResults.Add(result);
            p12CandidateSweepCompletedCaseCount++;

            if (p12CandidateSweepLateralIndex + 1 <
                P12SweepLateralCaseCount)
            {
                p12CandidateSweepLateralIndex++;
                // Reinitialize the same descriptor so every lateral case starts
                // from the same generated topology/evolution state rather than
                // inheriting several seconds of the preceding case.
                ApplyP12SweepSpacingCandidate();
                return;
            }

            if (p12CandidateSweepSpacingIndex + 1 <
                P12SweepSpacingCount)
            {
                p12CandidateSweepSpacingIndex++;
                p12CandidateSweepLateralIndex = 0;
                ApplyP12SweepSpacingCandidate();
                return;
            }

            p12CandidateSweepTerminalReason =
                "All 12 candidate cases completed.";
            BeginP12CandidateSweepRestore(
                P12CandidateSweepPhase.Complete);
        }

        private void FailP12CandidateSweep(string reason)
        {
            p12CandidateSweepTerminalReason = reason;
            BeginP12CandidateSweepRestore(P12CandidateSweepPhase.Failed);
        }

        private void BeginP12CandidateSweepRestore(
            P12CandidateSweepPhase terminalPhase)
        {
            p12CandidateSweepTerminalPhase = terminalPhase;
            p12CandidateSweepPhase = P12CandidateSweepPhase.Restoring;
            p12CandidateSweepStatus = terminalPhase switch
            {
                P12CandidateSweepPhase.Complete => "Restoring",
                P12CandidateSweepPhase.Cancelled => "Cancelling",
                _ => "Failed / restoring"
            };
            p12CandidateSweepSummary =
                "Restoring the authored grid selection and normal cache-only " +
                "startup ownership.";
            p12CandidateSweepPhaseStartedAt =
                Time.realtimeSinceStartupAsDouble;
            p12CandidateSweepUseCellSizeOverride = false;
            p12CandidateSweepUseLateralRatioOverride = false;
            p12CandidateSweepTransientTopologyGenerationInProgress = false;
            ClearSteadyStateWorkAccounting(false);
            ReleaseResources();
            resourcesDirty = true;
            boundaryDirty = true;
            initializationPhase = InitializationPhase.NotStarted;
            topologyStartupValidationComplete = false;
            lastRuntimeTime = Time.realtimeSinceStartup;
        }

        private void FinalizeP12CandidateSweep(
            bool authoredRuntimeReady,
            bool normalPreparationRequired)
        {
            string metadataAfter =
                p12CandidateSweepAssignedCacheAsset != null
                    ? BuildAssignedCacheMetadataSignature(
                        p12CandidateSweepAssignedCacheAsset)
                    : "<none>";
            byte[] payloadAfter =
                p12CandidateSweepAssignedCacheAsset != null &&
                p12CandidateSweepAssignedCacheAsset.HasPayload
                    ? p12CandidateSweepAssignedCacheAsset
                        .GetPayloadReadOnlyReference()
                    : Array.Empty<byte>();
            bool metadataUnchanged = string.Equals(
                p12CandidateSweepAssignedCacheMetadataBefore,
                metadataAfter,
                StringComparison.Ordinal);
            bool payloadUnchanged = ByteArraysEqual(
                p12CandidateSweepAssignedCachePayloadBefore,
                payloadAfter,
                out int firstDifference);
            bool allCasesCompleted =
                p12CandidateSweepResults.Count == P12SweepTotalCaseCount;
            bool allCasesPassed = allCasesCompleted;
            for (int index = 0;
                 index < p12CandidateSweepResults.Count;
                 index++)
            {
                allCasesPassed &= p12CandidateSweepResults[index].Passed;
            }
            bool restoredOwnership =
                !p12CandidateSweepUseCellSizeOverride &&
                !p12CandidateSweepUseLateralRatioOverride &&
                !p12CandidateSweepTransientTopologyGenerationInProgress &&
                (authoredRuntimeReady || normalPreparationRequired);
            bool completedNormally = p12CandidateSweepTerminalPhase ==
                P12CandidateSweepPhase.Complete;
            bool overall = completedNormally && allCasesPassed &&
                metadataUnchanged && payloadUnchanged && restoredOwnership;

            p12CandidateSweepReport = BuildP12CandidateSweepReport(
                allCasesCompleted,
                allCasesPassed,
                metadataUnchanged,
                payloadUnchanged,
                firstDifference,
                authoredRuntimeReady,
                normalPreparationRequired,
                restoredOwnership,
                overall);
            topologyCacheDiagnosticReport = p12CandidateSweepReport;

            if (!TryWriteLatestDiagnosticReport(
                    "LatestP12CandidateSweep",
                    p12CandidateSweepReport,
                    out p12CandidateSweepReportPath,
                    out string writeError))
            {
                overall = false;
                int overallLineIndex = p12CandidateSweepReport.LastIndexOf(
                    "Overall: ",
                    StringComparison.Ordinal);
                if (overallLineIndex >= 0)
                {
                    p12CandidateSweepReport =
                        p12CandidateSweepReport.Substring(0, overallLineIndex) +
                        "Report file write: FAIL — " + writeError +
                        Environment.NewLine +
                        "Overall: FAIL" + Environment.NewLine;
                    topologyCacheDiagnosticReport =
                        p12CandidateSweepReport;
                }
                p12CandidateSweepStatus = "Failed";
                p12CandidateSweepSummary =
                    "The sweep completed, but the combined report file could " +
                    "not be written: " + writeError;
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    p12CandidateSweepSummary;
                topologyCacheDiagnosticReportPath = string.Empty;
                p12CandidateSweepActive = false;
                p12CandidateSweepPhase = P12CandidateSweepPhase.Failed;
                Debug.LogError(
                    "[River Foam P12 Sweep] " +
                    p12CandidateSweepSummary,
                    river);
                return;
            }

            topologyCacheDiagnosticReportPath =
                p12CandidateSweepReportPath;
            topologyCacheDiagnosticState = overall ? "Passed" : "Failed";
            topologyCacheDiagnosticSummary = overall
                ? "The complete P12 fixed-spacing and lateral-response " +
                  "sweep passed all machine-verifiable gates. Visual review " +
                  "is still required."
                : "The complete P12 candidate sweep did not pass every gate. " +
                  "See the combined report.";
            p12CandidateSweepStatus = overall
                ? "Passed"
                : p12CandidateSweepTerminalPhase ==
                    P12CandidateSweepPhase.Cancelled
                        ? "Cancelled"
                        : "Failed";
            p12CandidateSweepSummary = topologyCacheDiagnosticSummary;
            p12CandidateSweepActive = false;
            p12CandidateSweepPhase = overall
                ? P12CandidateSweepPhase.Complete
                : p12CandidateSweepTerminalPhase ==
                    P12CandidateSweepPhase.Cancelled
                        ? P12CandidateSweepPhase.Cancelled
                        : P12CandidateSweepPhase.Failed;
            if (overall)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log(
                    "[River Foam P12 Sweep] PASS — " +
                    p12CandidateSweepReportPath,
                    river);
            }
            else
            {
                Debug.LogError(
                    "[River Foam P12 Sweep] FAIL — " +
                    p12CandidateSweepReportPath,
                    river);
            }
        }

        private string BuildP12CandidateSweepReport(
            bool allCasesCompleted,
            bool allCasesPassed,
            bool metadataUnchanged,
            bool payloadUnchanged,
            int firstDifference,
            bool authoredRuntimeReady,
            bool normalPreparationRequired,
            bool restoredOwnership,
            bool overall)
        {
            StringBuilder report = new(65536);
            report.AppendLine(
                "RIVER FOAM FIXED-METRIC P12 COMPLETE CANDIDATE SWEEP");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                "Operation: one explicit Play Mode action; cycles the real " +
                "fixed-metric allocation through 0.25/0.20/0.15/0.10 m and " +
                "tests each spacing at lateral ratios 0, authored, and 1. " +
                "Mismatched candidates use diagnostic-only transient topology " +
                "generation. No cache, scene, prefab, material, or serialized " +
                "River state is written.");
            report.AppendLine(
                "Capture policy: each case clears deterministic source/material " +
                "state, warms for 2 seconds, then captures at least 5 seconds " +
                "and 30 frames. Visual acceptance remains external.");
            report.AppendLine(
                $"Authored lateral ratio used by middle cases: " +
                $"{p12CandidateSweepAuthoredLateralRatio:0.000000}");
            report.AppendLine(
                $"Material contract held for all cases: " +
                ResolveMaterialContractDiagnosticLabel(
                    river.FoamMaterialContract));
            report.AppendLine(
                $"Frozen initialization Motion Time for all 12 cases: " +
                $"{p12CandidateSweepInitializationMotionTime:0.000000} s");
            report.AppendLine(
                $"Terminal reason: {p12CandidateSweepTerminalReason}");
            report.AppendLine();

            for (int spacingIndex = 0;
                 spacingIndex < P12SweepSpacingCount;
                 spacingIndex++)
            {
                report.AppendLine(
                    $"SPACING {P12SweepRequestedSpacings[spacingIndex]:0.00} m");
                for (int lateralIndex = 0;
                     lateralIndex < P12SweepLateralCaseCount;
                     lateralIndex++)
                {
                    P12CandidateSweepCaseResult result =
                        FindP12SweepCase(spacingIndex, lateralIndex);
                    if (result == null)
                    {
                        report.AppendLine(
                            $"Case lateral {ResolveP12SweepLateralRatio(lateralIndex):0.000}: " +
                            "MISSING");
                        continue;
                    }

                    report.AppendLine(
                        $"Case {result.CaseIndex + 1}/{P12SweepTotalCaseCount} — " +
                        $"lateral ratio {result.LateralRatio:0.000000}: " +
                        (result.Passed ? "PASS" : "FAIL"));
                    report.AppendLine(
                        $"Descriptor: requested={result.RequestedSpacing:0.000} m / " +
                        $"resolved dx={result.ResolvedDx:0.000000} m / " +
                        $"dy={result.ResolvedDy:0.000000} m / " +
                        $"structural={result.StructuralWidth}x{result.StructuralHeight} / " +
                        $"film={result.FilmWidth}x{result.FilmHeight} / " +
                        $"cells={result.StructuralCells:N0} / " +
                        $"signature={result.DescriptorSignature}");
                    report.AppendLine(
                        $"Topology: {result.TopologySource} / initialization " +
                        $"{result.InitializationSteps:N0} steps / " +
                        $"{result.InitializationMilliseconds:0.000} ms / " +
                        $"slowest {result.InitializationSlowestStep} " +
                        $"{result.InitializationSlowestMilliseconds:0.000} ms / " +
                        $"major {result.MajorCount:N0} / connectors " +
                        $"{result.ConnectorCount:N0} / pockets " +
                        $"{result.PocketCount:N0}");
                    report.AppendLine(
                        $"Transport: CFL x/y/total " +
                        $"{result.DownstreamCfl:0.000000}/" +
                        $"{result.LateralCfl:0.000000}/" +
                        $"{result.TotalCfl:0.000000} / substeps " +
                        $"{result.RequiredSubsteps}/{result.UsedSubsteps} / " +
                        $"Jraw/Jbounded {result.RawJacobian:0.000000}/" +
                        $"{result.BoundedJacobian:0.000000} / max |κn| " +
                        $"{result.MaximumCurvatureLateral:0.000000}");
                    report.AppendLine(
                        $"Lane: cell mean |intent| " +
                        $"{result.CellMeanAbsoluteIntent:0.000000} / face mean " +
                        $"{result.FaceMeanAbsoluteIntent:0.000000} / face RMS " +
                        $"{result.FaceRmsIntent:0.000000} / opposing " +
                        $"{result.OpposingFaceFraction:0.000000} / cancellation " +
                        $"{result.FaceCancellationRatio:0.000000}");
                    report.AppendLine(
                        $"Lateral movement: weighted speed " +
                        $"{result.MaterialWeightedLateralSpeed:0.000000} m/s / " +
                        $"positive {result.PositiveLateralMovement:0.000000} / " +
                        $"negative {result.NegativeLateralMovement:0.000000} " +
                        "m² Presence");
                    report.AppendLine(
                        $"Cost: memory {result.EstimatedMemoryBytes:N0} bytes / " +
                        $"window {result.CaptureSeconds:0.000} s / " +
                        $"frames {result.CaptureFrames:N0} / dispatches " +
                        $"{result.Dispatches:N0} / cells {result.CellIterations:N0} / " +
                        $"material steps {result.MaterialSteps:N0} / material CPU " +
                        $"{result.MaterialCpuMilliseconds:0.000} ms / topology CPU " +
                        $"{result.TopologyCpuMilliseconds:0.000} ms");
                    report.AppendLine(
                        "Gates: runtime=" + result.RuntimeReady +
                        "; descriptor=" + result.DescriptorExact +
                        "; topology=" + result.TopologyReady +
                        "; transport=" + result.TransportSafe +
                        "; accounting=" + result.AccountingReady +
                        "; lane=" + result.LaneEvidenceReady +
                        "; zero-isolation=" + result.ZeroRatioIsolated);
                    report.AppendLine("Work: " + result.WorkSummary);
                }
                AppendP12SweepSpacingComparison(report, spacingIndex);
                report.AppendLine();
            }

            report.AppendLine("ASSIGNED CACHE MUTATION PROOF");
            report.AppendLine($"Metadata unchanged: {metadataUnchanged}");
            report.AppendLine($"Payload unchanged: {payloadUnchanged}");
            if (!payloadUnchanged)
            {
                report.AppendLine(
                    $"First differing payload byte: {firstDifference}");
            }
            report.AppendLine(
                "ASSIGNED CACHE VERDICT: " +
                (metadataUnchanged && payloadUnchanged ? "PASS" : "FAIL"));
            report.AppendLine();

            report.AppendLine("AUTHORED RUNTIME RESTORATION");
            report.AppendLine(
                $"Authored runtime ready: {authoredRuntimeReady}");
            report.AppendLine(
                $"Normal cache preparation required: " +
                $"{normalPreparationRequired}");
            report.AppendLine(
                $"Sweep overrides/transient generation released: " +
                $"{restoredOwnership}");
            report.AppendLine(
                "RESTORATION VERDICT: " +
                (restoredOwnership ? "PASS" : "FAIL"));
            report.AppendLine();

            report.AppendLine("FINAL LEDGER");
            report.AppendLine(
                "All 12 matrix cases completed: " +
                (allCasesCompleted ? "PASS" : "FAIL"));
            report.AppendLine(
                "Every case passed runtime gates: " +
                (allCasesPassed ? "PASS" : "FAIL"));
            report.AppendLine(
                "Assigned cache remained unchanged: " +
                (metadataUnchanged && payloadUnchanged ? "PASS" : "FAIL"));
            report.AppendLine(
                "Authored runtime ownership restored: " +
                (restoredOwnership ? "PASS" : "FAIL"));
            report.AppendLine(
                "Visual candidate verdict: USER REVIEW REQUIRED");
            report.AppendLine("Overall: " + (overall ? "PASS" : "FAIL"));
            return report.ToString();
        }

        private void AppendP12SweepSpacingComparison(
            StringBuilder report,
            int spacingIndex)
        {
            P12CandidateSweepCaseResult zero =
                FindP12SweepCase(spacingIndex, 0);
            P12CandidateSweepCaseResult authored =
                FindP12SweepCase(spacingIndex, 1);
            P12CandidateSweepCaseResult maximum =
                FindP12SweepCase(spacingIndex, 2);
            if (zero == null || authored == null || maximum == null)
            {
                report.AppendLine(
                    "Lateral contrast: INCOMPLETE — one or more cases missing.");
                return;
            }

            float authoredGain = authored.MaterialWeightedLateralSpeed -
                zero.MaterialWeightedLateralSpeed;
            float maximumGain = maximum.MaterialWeightedLateralSpeed -
                zero.MaterialWeightedLateralSpeed;
            report.AppendLine(
                $"Lateral contrast: zero/authored/max weighted speed " +
                $"{zero.MaterialWeightedLateralSpeed:0.000000}/" +
                $"{authored.MaterialWeightedLateralSpeed:0.000000}/" +
                $"{maximum.MaterialWeightedLateralSpeed:0.000000} m/s / " +
                $"authored gain {authoredGain:0.000000} / " +
                $"max gain {maximumGain:0.000000} m/s.");
        }

        private P12CandidateSweepCaseResult FindP12SweepCase(
            int spacingIndex,
            int lateralIndex)
        {
            for (int index = 0;
                 index < p12CandidateSweepResults.Count;
                 index++)
            {
                P12CandidateSweepCaseResult result =
                    p12CandidateSweepResults[index];
                if (result.SpacingIndex == spacingIndex &&
                    result.LateralIndex == lateralIndex)
                {
                    return result;
                }
            }

            return null;
        }

        private float ResolveP12SweepLateralRatio(int lateralIndex)
        {
            return lateralIndex switch
            {
                0 => 0f,
                1 => p12CandidateSweepAuthoredLateralRatio,
                2 => 1f,
                _ => p12CandidateSweepAuthoredLateralRatio
            };
        }

        private float ResolveP12SweepTimedPhaseFraction(double duration)
        {
            if (duration <= 0.0)
            {
                return 1f;
            }

            return Mathf.Clamp01((float)(
                (Time.realtimeSinceStartupAsDouble -
                 p12CandidateSweepPhaseStartedAt) / duration));
        }
#endif
    }
}
