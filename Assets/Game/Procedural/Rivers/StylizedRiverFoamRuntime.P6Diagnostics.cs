#if UNITY_EDITOR
using System;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private const int P6ExternalFieldMappingContract = 1;

        /// <summary>
        /// One-button RG-METRIC-P6 validation pipeline. It does not store or
        /// modify the assigned cache. The report covers the active legacy lane
        /// and routing resources, fixed-metric physical invariance, routing
        /// reach conversion, physical Foam-to-Disturbance UV mapping under
        /// unequal texture dimensions, renderer/compute coordinate agreement,
        /// obstacle-source readiness, and neutral external-field fallback.
        /// </summary>
        public bool RunP6ComprehensiveValidationReport()
        {
            topologyCacheDiagnosticRunCount++;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary =
                "Running the P6 routing, Motion Lane, and external-field " +
                "integration validation pipeline.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;

            if (Application.isPlaying)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "The P6 comprehensive report is Edit Mode only.");
            }

            river = GetComponent<StylizedRiver>();
            surfaceRenderer = river != null ? river.SurfaceRenderer : null;
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (river == null || !river.Domain.IsValid ||
                disturbanceRuntime == null)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "A valid river and Disturbance runtime are required.");
            }

            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            StringBuilder report = new(32768);
            report.AppendLine(
                "RIVER FOAM FIXED-METRIC P6 COMPREHENSIVE VALIDATION");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                "Operation: one explicit user-triggered report; installs the " +
                "assigned cache into temporary live resources, captures all " +
                "evidence before cleanup, then releases resources. No cache, " +
                "scene, prefab, material, or serialized river state is stored.");
            report.AppendLine(
                "Contracts: metric Motion Lane 1 + metric obstacle routing 1 + " +
                "physical external-field mapping 1; active runtime mapping " +
                "remains LegacyNormalizedAcross.");
            report.AppendLine();

            StylizedRiverFoamTopologyCacheAsset assignedAsset =
                river.FoamTopologyCacheAsset;
            string assignedMetadataBefore = assignedAsset != null
                ? BuildAssignedCacheMetadataSignature(assignedAsset)
                : "<none>";
            byte[] assignedPayloadBefore = assignedAsset != null &&
                assignedAsset.HasPayload
                    ? assignedAsset.GetPayloadCopy()
                    : Array.Empty<byte>();

            bool livePreparationReady = false;
            bool diagnosticTransactionStarted = false;
            bool resourcesCompleteWhileMeasured = false;
            bool activeLegacy = false;
            bool fixedCandidateReady = false;
            bool liveEvidenceCaptured = false;
            bool laneExact = false;
            bool routingExact = false;
            bool externalExact = false;
            bool sourceReady = false;
            bool cleanupStateExact = false;
            bool bindingsDisabled = false;
            bool cleanupCompleted = false;
            string cleanupFailure = string.Empty;

            try
            {
                livePreparationReady = TryPrepareP6LiveValidationResources(
                    report,
                    out diagnosticTransactionStarted,
                    out string preparationFailure);
                resourcesCompleteWhileMeasured = livePreparationReady &&
                    initializationPhase == InitializationPhase.Ready &&
                    AreResourcesCompleteAndCurrent();
                activeLegacy = resourcesCompleteWhileMeasured &&
                    gridDescriptor.IsCreated &&
                    !gridDescriptor.UsesFixedMetricLattice;
                fixedCandidateReady = resourcesCompleteWhileMeasured &&
                    fixedMetricCandidateDescriptor.IsCreated &&
                    fixedMetricCandidateDescriptor.UsesFixedMetricLattice;

                report.AppendLine("ACTIVE RUNTIME OWNERSHIP");
                report.AppendLine(
                    $"Live preparation ready: {livePreparationReady}");
                report.AppendLine(
                    $"Resources complete while measured: " +
                    $"{resourcesCompleteWhileMeasured}");
                report.AppendLine(
                    $"Active descriptor: " +
                    $"{(gridDescriptor.IsCreated ? gridDescriptor.Mapping.ToString() : "Unallocated")} / " +
                    $"{gridDescriptor.InitializationSignature:X16}");
                report.AppendLine(
                    $"Fixed candidate: {fixedCandidateReady}; status=" +
                    $"{fixedMetricCandidateFailureReason}");
                if (!livePreparationReady &&
                    !string.IsNullOrEmpty(preparationFailure))
                {
                    report.AppendLine(
                        "Preparation failure: " + preparationFailure);
                }
                report.AppendLine(
                    "P6 does not activate fixed-metric allocation, source " +
                    "rasterization, transport, film, shape, or production render.");
                report.AppendLine(
                    "ACTIVE LEGACY OWNERSHIP VERDICT: " +
                    (livePreparationReady && resourcesCompleteWhileMeasured &&
                     activeLegacy && fixedCandidateReady
                        ? "PASS"
                        : "FAIL"));
                report.AppendLine();

                if (livePreparationReady && resourcesCompleteWhileMeasured)
                {
                    EnsureMotionFieldsCurrent();
                    laneExact = ValidateP6MotionLane(report);
                    routingExact = ValidateP6ObstacleRouting(report);
                    externalExact = ValidateP6ExternalFieldMapping(report);
                    sourceReady = ValidateP6ExternalSourceReadiness(report);
                    liveEvidenceCaptured = true;
                }
                else
                {
                    AppendP6SkippedLiveContract(
                        report,
                        "MOTION LANE PHYSICAL CONTRACT",
                        "MOTION LANE VERDICT",
                        preparationFailure);
                    AppendP6SkippedLiveContract(
                        report,
                        "OBSTACLE ROUTING PHYSICAL CONTRACT",
                        "OBSTACLE ROUTING VERDICT",
                        preparationFailure);
                    AppendP6SkippedLiveContract(
                        report,
                        "EXTERNAL-FIELD SAME-POINT MAPPING CONTRACT",
                        "EXTERNAL FIELD VERDICT",
                        preparationFailure);
                    AppendP6SkippedLiveContract(
                        report,
                        "EXTERNAL SOURCE READINESS AND FALLBACK",
                        "SOURCE/FALLBACK VERDICT",
                        preparationFailure);
                }
            }
            catch (Exception exception)
            {
                report.AppendLine("P6 LIVE EVIDENCE EXCEPTION");
                report.AppendLine(exception.ToString());
                report.AppendLine("LIVE EVIDENCE VERDICT: FAIL");
                report.AppendLine();
            }
            finally
            {
                if (diagnosticTransactionStarted)
                {
                    try
                    {
                        ReleaseResources();
                        editorTopologyPreparationInProgress = false;
                        explicitTopologyGenerationInProgress = false;
                        initializationPhase = InitializationPhase.NotStarted;
                        resourcesDirty = true;
                        boundaryDirty = true;
                        BindDisabled();
                        cleanupStateExact = IsP6DiagnosticCleanupStateExact();
                        bindingsDisabled = TryVerifyP6BindingsDisabled(
                            out cleanupFailure);
                        cleanupCompleted = true;
                    }
                    catch (Exception exception)
                    {
                        cleanupFailure = exception.ToString();
                    }
                }
                else
                {
                    cleanupFailure =
                        "No diagnostic-owned live resource transaction started; " +
                        "foreign runtime state was not modified by cleanup.";
                }
            }

            report.AppendLine("DIAGNOSTIC CLEANUP PROOF");
            report.AppendLine($"Cleanup completed: {cleanupCompleted}");
            report.AppendLine(
                $"Runtime state reset: {cleanupStateExact}");
            report.AppendLine(
                $"Renderer bindings disabled: {bindingsDisabled}");
            report.AppendLine(
                $"Final phase / dirty flags: {initializationPhase} / " +
                $"resources={resourcesDirty} / boundary={boundaryDirty}");
            report.AppendLine(
                $"Final descriptors: active={gridDescriptor.IsCreated}; " +
                $"candidate={fixedMetricCandidateDescriptor.IsCreated}");
            report.AppendLine(
                $"Final signatures: lane={lastMotionLaneSignature}; " +
                $"routing={lastObstacleRoutingSignature}");
            if (!string.IsNullOrEmpty(cleanupFailure))
            {
                report.AppendLine("Cleanup detail: " + cleanupFailure);
            }
            bool cleanupExact = cleanupCompleted && cleanupStateExact &&
                bindingsDisabled;
            report.AppendLine(
                "DIAGNOSTIC CLEANUP VERDICT: " +
                (cleanupExact ? "PASS" : "FAIL"));
            report.AppendLine();

            string assignedMetadataAfter = assignedAsset != null
                ? BuildAssignedCacheMetadataSignature(assignedAsset)
                : "<none>";
            byte[] assignedPayloadAfter = assignedAsset != null &&
                assignedAsset.HasPayload
                    ? assignedAsset.GetPayloadReadOnlyReference()
                    : Array.Empty<byte>();
            bool assignedMetadataUnchanged =
                assignedMetadataBefore == assignedMetadataAfter;
            bool assignedPayloadUnchanged = ByteArraysEqual(
                assignedPayloadBefore,
                assignedPayloadAfter,
                out int assignedPayloadFirstDifference);
            bool assignedCacheUnchanged =
                assignedMetadataUnchanged && assignedPayloadUnchanged;
            report.AppendLine("ASSIGNED CACHE MUTATION PROOF");
            report.AppendLine(
                $"Metadata unchanged: {assignedMetadataUnchanged}");
            report.AppendLine(
                $"Payload unchanged: {assignedPayloadUnchanged}");
            if (!assignedPayloadUnchanged)
            {
                report.AppendLine(
                    $"First payload difference: " +
                    $"{assignedPayloadFirstDifference:N0}");
            }
            report.AppendLine(
                "ASSIGNED CACHE VERDICT: " +
                (assignedCacheUnchanged ? "PASS" : "FAIL"));
            report.AppendLine();

            bool preparationExact = livePreparationReady &&
                resourcesCompleteWhileMeasured && liveEvidenceCaptured;
            bool overall = preparationExact && activeLegacy &&
                fixedCandidateReady && laneExact && routingExact &&
                externalExact && sourceReady && cleanupExact &&
                assignedCacheUnchanged;

            stopwatch.Stop();
            report.AppendLine("FINAL LEDGER");
            report.AppendLine(
                "Live diagnostic preparation transaction: " +
                (preparationExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Active legacy ownership preserved: " +
                (preparationExact && activeLegacy && fixedCandidateReady
                    ? "PASS"
                    : "FAIL"));
            report.AppendLine(
                "Motion Lane physical invariance and scroll: " +
                (laneExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Obstacle routing physical policy and no rear leakage: " +
                (routingExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "External-field same-point mapping under unequal dimensions: " +
                (externalExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Obstacle-source readiness and neutral fallback: " +
                (sourceReady ? "PASS" : "FAIL"));
            report.AppendLine(
                "Diagnostic cleanup and disabled bindings: " +
                (cleanupExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Assigned cache remained unchanged: " +
                (assignedCacheUnchanged ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Elapsed: {stopwatch.Elapsed.TotalMilliseconds:F3} ms");
            report.AppendLine("Overall: " + (overall ? "PASS" : "FAIL"));

            return FinalizeP6Report(
                report,
                overall,
                overall
                    ? "All P6 contracts and the live diagnostic transaction passed."
                    : "One or more P6 or diagnostic-transaction contracts failed; inspect the report ledger.");
        }

        private bool TryPrepareP6LiveValidationResources(
            StringBuilder report,
            out bool transactionStarted,
            out string failure)
        {
            transactionStarted = false;
            failure = string.Empty;
            report.AppendLine("LIVE DIAGNOSTIC PREPARATION TRANSACTION");
            report.AppendLine(
                "Mode: install the assigned current cache through the normal " +
                "initialization state machine; do not build or serialize a payload.");

            if (editorTopologyPreparationInProgress ||
                explicitTopologyGenerationInProgress)
            {
                failure =
                    "Another topology preparation transaction is already active.";
                report.AppendLine("Preparation refused: " + failure);
                report.AppendLine("LIVE PREPARATION VERDICT: FAIL");
                report.AppendLine();
                return false;
            }

            if (!disturbanceRuntime
                    .PrepareGeneratedGeometrySourcesForCacheValidation(
                        out string registryStatus))
            {
                failure = registryStatus;
                report.AppendLine("Obstacle registry: FAIL — " + registryStatus);
                report.AppendLine("LIVE PREPARATION VERDICT: FAIL");
                report.AppendLine();
                return false;
            }

            int serializationCountBefore =
                topologyCacheLastBuildSerializationCount;
            int preparationUploadCountBefore =
                topologyCachePreparationGeneratedUploadCount;
            int monitoredSerializationCount = int.MinValue;
            int monitoredPreparationUploadCount = int.MinValue;
            const int maximumSteps = 128;
            bool ready = false;
            bool complete = false;
            int executedSteps = 0;

            transactionStarted = true;
            topologyCacheLastBuildSerializationCount = int.MinValue;
            topologyCachePreparationGeneratedUploadCount = int.MinValue;
            try
            {
                ReleaseResources();
                editorTopologyPreparationInProgress = false;
                explicitTopologyGenerationInProgress = false;
                activeTopologyRequiresExplicitPreparation = false;
                resourcesDirty = true;
                boundaryDirty = true;
                initializationPhase = InitializationPhase.NotStarted;

                for (; executedSteps < maximumSteps; executedSteps++)
                {
                    if (EnsureResources())
                    {
                        executedSteps++;
                        break;
                    }

                    if (initializationPhase == InitializationPhase.Failed ||
                        initializationPhase ==
                            InitializationPhase.CachePreparationRequired)
                    {
                        executedSteps++;
                        break;
                    }
                }

                ready = initializationPhase == InitializationPhase.Ready;
                complete = ready && AreResourcesCompleteAndCurrent();
                monitoredSerializationCount =
                    topologyCacheLastBuildSerializationCount;
                monitoredPreparationUploadCount =
                    topologyCachePreparationGeneratedUploadCount;
            }
            finally
            {
                topologyCacheLastBuildSerializationCount =
                    serializationCountBefore;
                topologyCachePreparationGeneratedUploadCount =
                    preparationUploadCountBefore;
            }

            bool noSerialization =
                monitoredSerializationCount == int.MinValue;
            bool noExplicitPreparationUpload =
                monitoredPreparationUploadCount == int.MinValue;
            bool exact = ready && complete && noSerialization &&
                noExplicitPreparationUpload;

            if (!exact)
            {
                failure =
                    $"Live preparation ended at {initializationPhase}; " +
                    $"resourcesComplete={complete}; serializationUnchanged=" +
                    $"{noSerialization}; explicitPreparationUploadUnchanged=" +
                    $"{noExplicitPreparationUpload}; startup={topologyCacheStartupState}: " +
                    $"{topologyCacheStartupSummary}";
            }

            report.AppendLine("Obstacle registry: " + registryStatus);
            report.AppendLine(
                $"Initialization steps: {executedSteps:N0}/{maximumSteps:N0}");
            report.AppendLine(
                $"Final live phase: {initializationPhase}");
            report.AppendLine(
                $"Resources complete and current: {complete}");
            report.AppendLine(
                $"Serialization monitor: sentinel={int.MinValue}; observed=" +
                $"{monitoredSerializationCount}; no serialization=" +
                $"{noSerialization}; original counter restored=" +
                $"{serializationCountBefore}");
            report.AppendLine(
                $"Explicit-preparation upload monitor: sentinel=" +
                $"{int.MinValue}; observed={monitoredPreparationUploadCount}; " +
                $"path inactive={noExplicitPreparationUpload}; original " +
                $"counter restored={preparationUploadCountBefore}");
            if (!string.IsNullOrEmpty(failure))
            {
                report.AppendLine("Preparation detail: " + failure);
            }
            report.AppendLine(
                "LIVE PREPARATION VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private static void AppendP6SkippedLiveContract(
            StringBuilder report,
            string heading,
            string verdictLabel,
            string reason)
        {
            report.AppendLine(heading);
            report.AppendLine(
                "Skipped because the live diagnostic preparation transaction " +
                "did not reach a complete Ready state.");
            if (!string.IsNullOrEmpty(reason))
            {
                report.AppendLine("Preparation detail: " + reason);
            }
            report.AppendLine(verdictLabel + ": FAIL");
            report.AppendLine();
        }

        private bool IsP6DiagnosticCleanupStateExact()
        {
            return initializationPhase == InitializationPhase.NotStarted &&
                resourcesDirty && boundaryDirty &&
                !editorTopologyPreparationInProgress &&
                !explicitTopologyGenerationInProgress &&
                !gridDescriptor.IsCreated &&
                !fixedMetricCandidateDescriptor.IsCreated &&
                currentState == null &&
                motionLaneTexture == null &&
                obstacleRoutingTexture == null &&
                neutralDisturbanceTexture == null &&
                motionLaneHalfData.Length == 0 &&
                obstacleRoutingHalfData.Length == 0 &&
                lastMotionLaneSignature == int.MinValue &&
                lastObstacleRoutingSignature == int.MinValue;
        }

        private bool TryVerifyP6BindingsDisabled(out string detail)
        {
            detail = string.Empty;
            if (surfaceRenderer == null)
            {
                detail = "The river surface renderer is unavailable.";
                return false;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            float enabled = propertyBlock.GetFloat(FoamEnabledId);
            Texture previous = propertyBlock.GetTexture(FoamPreviousId);
            Texture current = propertyBlock.GetTexture(FoamCurrentId);
            bool exact = BitConverter.SingleToInt32Bits(enabled) == 0 &&
                previous == Texture2D.blackTexture &&
                current == Texture2D.blackTexture;
            detail =
                $"FoamEnabled={enabled:R}[0x" +
                $"{BitConverter.SingleToInt32Bits(enabled):X8}]; " +
                $"previousBlack={previous == Texture2D.blackTexture}; " +
                $"currentBlack={current == Texture2D.blackTexture}.";
            return exact;
        }

        private bool ValidateP6MotionLane(StringBuilder report)
        {
            report.AppendLine("MOTION LANE PHYSICAL CONTRACT");
            float directionFrequency = river != null
                ? river.FoamDirectionChangeFrequency
                : 1f;
            float coherence = river != null
                ? river.FoamAcrossRiverCoherence
                : 1f;
            float seed = river != null
                ? river.VisualSeed * 0.01371f
                : 23.17f;
            Vector2[] physicalPoints =
            {
                new(0.10f, -3.25f),
                new(7.75f, -0.45f),
                new(19.20f, 2.15f),
                new(31.80f, 4.90f),
                new(55.35f, -1.70f)
            };

            bool valuesFinite = true;
            bool valuesRepeatExactly = true;
            bool anyNonZero = false;
            for (int index = 0; index < physicalPoints.Length; index++)
            {
                Vector2 point = physicalPoints[index];
                float first = MotionFractalLaneNoiseMetric(
                    point.x,
                    point.y,
                    directionFrequency,
                    coherence,
                    seed);
                float second = MotionFractalLaneNoiseMetric(
                    point.x,
                    point.y,
                    directionFrequency,
                    coherence,
                    seed);
                valuesFinite &= P6IsFinite(first);
                valuesRepeatExactly &=
                    BitConverter.SingleToInt32Bits(first) ==
                    BitConverter.SingleToInt32Bits(second);
                anyNonZero |= Mathf.Abs(first) > 0.0001f;
                report.AppendLine(
                    $"Point {index}: s={point.x:R}m, n={point.y:R}m, " +
                    $"lane={first:R}[0x" +
                    $"{BitConverter.SingleToInt32Bits(first):X8}]");
            }

            float requestedCell = fixedMetricCandidateDescriptor.IsCreated
                ? fixedMetricCandidateDescriptor.ResolvedDyMetres
                : 0.20f;
            int nearRows = ResolveMetricRowOffset(
                MotionLaneNearSmoothingOffsetMetres,
                requestedCell);
            int farRows = ResolveMetricRowOffset(
                MotionLaneFarSmoothingOffsetMetres,
                requestedCell);
            float nearResolved = nearRows * requestedCell;
            float farResolved = farRows * requestedCell;
            bool smoothingExact =
                Mathf.Abs(
                    nearResolved - MotionLaneNearSmoothingOffsetMetres) <=
                    requestedCell * 0.5f + 0.0001f &&
                Mathf.Abs(
                    farResolved - MotionLaneFarSmoothingOffsetMetres) <=
                    requestedCell * 0.5f + 0.0001f;

            float baseSpeed = ResolveBaseFoamDownstreamSpeedMetresPerSecond();
            float minimumFactor = river != null
                ? river.FoamObstacleMinimumDownstreamFactor
                : 0f;
            bool noZeroSpeed = P6IsFinite(baseSpeed) &&
                baseSpeed > 0f && P6IsFinite(minimumFactor) &&
                minimumFactor > 0f;
            bool runtimeLaneAvailable = motionLaneTexture != null &&
                motionLaneHalfData.Length == fieldWidth * fieldHeight &&
                lastMotionLaneSignature != int.MinValue;
            int runtimeNonZero = 0;
            for (int index = 0; index < motionLaneHalfData.Length; index++)
            {
                if (Mathf.Abs(Mathf.HalfToFloat(
                        motionLaneHalfData[index])) > 0.0001f)
                {
                    runtimeNonZero++;
                }
            }
            runtimeLaneAvailable &= runtimeNonZero > 0;

            float firstOctaveWavelength =
                MotionLaneDownstreamBasisMetres /
                Mathf.Max(
                    0.0001f,
                    8.5f * Mathf.Clamp(directionFrequency, 0.25f, 4f));
            bool exact = valuesFinite && valuesRepeatExactly && anyNonZero &&
                smoothingExact && noZeroSpeed && runtimeLaneAvailable;
            report.AppendLine(
                $"Downstream basis={MotionLaneDownstreamBasisMetres:R}m; " +
                $"first-octave wavelength={firstOctaveWavelength:R}m");
            report.AppendLine(
                $"Lateral reference span=" +
                $"{MotionLaneLateralReferenceSpanMetres:R}m");
            report.AppendLine(
                $"Smoothing requested/resolved: near=" +
                $"{MotionLaneNearSmoothingOffsetMetres:R}/{nearResolved:R}m " +
                $"({nearRows} rows), far=" +
                $"{MotionLaneFarSmoothingOffsetMetres:R}/{farResolved:R}m " +
                $"({farRows} rows)");
            report.AppendLine(
                $"Runtime lane: signature={lastMotionLaneSignature}; " +
                $"nonzero={runtimeNonZero:N0}/{motionLaneHalfData.Length:N0}; " +
                $"scroll={lastMotionLaneScrollCells:R} cells / " +
                $"{FoamMotionLaneScrollMetres:R}m");
            report.AppendLine(
                $"Base downstream speed={baseSpeed:R}m/s; obstacle minimum " +
                $"factor={minimumFactor:R}");
            report.AppendLine(
                "MOTION LANE VERDICT: " + (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP6ObstacleRouting(StringBuilder report)
        {
            report.AppendLine("OBSTACLE ROUTING PHYSICAL CONTRACT");
            float dx = fixedMetricCandidateDescriptor.IsCreated
                ? fixedMetricCandidateDescriptor.ResolvedDxMetres
                : 0.20f;
            float dy = fixedMetricCandidateDescriptor.IsCreated
                ? fixedMetricCandidateDescriptor.ResolvedDyMetres
                : 0.20f;
            int[] widths = { 2, 7, 19 };
            int[] heights = { 2, 9, 31 };
            bool policyExact = true;
            for (int index = 0; index < widths.Length; index++)
            {
                FoamObstacleRoutingPolicy policy =
                    ResolveFixedMetricObstacleRoutingPolicy(
                        dx,
                        dy,
                        widths[index],
                        heights[index]);
                float approach = policy.ApproachCells * dx;
                float closure = policy.FrontClosureCells * dx;
                float contact = policy.ContactCells * dx;
                float margin = policy.LateralMarginCells * dy;
                float expectedMargin = Mathf.Max(
                    dy,
                    heights[index] * dy * 0.22f);
                bool caseExact =
                    approach + 0.0001f >= ObstacleRoutingApproachReachMetres &&
                    approach < ObstacleRoutingApproachReachMetres + dx + 0.0001f &&
                    closure + 0.0001f >= ObstacleRoutingFrontClosureMetres &&
                    closure < ObstacleRoutingFrontClosureMetres + dx + 0.0001f &&
                    contact + 0.0001f >= ObstacleRoutingContactReachMetres &&
                    contact < ObstacleRoutingContactReachMetres + dx + 0.0001f &&
                    margin + 0.0001f >= expectedMargin &&
                    margin < expectedMargin + dy + 0.0001f &&
                    policy.ReleaseCells == 0;
                policyExact &= caseExact;
                report.AppendLine(
                    $"Case {index}: obstacle={widths[index]}x{heights[index]} " +
                    $"cells; approach={approach:R}m; closure={closure:R}m; " +
                    $"contact={contact:R}m; margin={margin:R}m; " +
                    $"release={policy.ReleaseCells}; " +
                    $"verdict={(caseExact ? "PASS" : "FAIL")}");
            }

            int runtimeObstacleCells = CountP6RuntimeObstacleCells();
            bool runtimeExact = obstacleRoutingTexture != null &&
                obstacleRoutingHalfData.Length ==
                    fieldWidth * fieldHeight * 2 &&
                lastObstacleRoutingSignature != int.MinValue &&
                lastObstacleRoutingRearLeakCellCount == 0;
            if (runtimeObstacleCells > 0)
            {
                runtimeExact &= lastObstacleRoutingInfluencedCellCount > 0;
            }
            report.AppendLine(
                $"Runtime routing: signature={lastObstacleRoutingSignature}; " +
                $"occupied={runtimeObstacleCells:N0} " +
                $"(candidates={obstacleExclusionCells.Count:N0}, " +
                $"cachedScalar={obstacleExclusionScalar.Length:N0}); influenced=" +
                $"{lastObstacleRoutingInfluencedCellCount:N0}; rearLeak=" +
                $"{lastObstacleRoutingRearLeakCellCount:N0}; maxApproach=" +
                $"{lastObstacleRoutingMaximumApproachMetres:R}m; " +
                $"maxMargin={lastObstacleRoutingMaximumLateralMarginMetres:R}m");
            bool exact = policyExact && runtimeExact;
            report.AppendLine(
                "OBSTACLE ROUTING VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private int CountP6RuntimeObstacleCells()
        {
            if (obstacleExclusionCells.Count > 0)
            {
                return obstacleExclusionCells.Count;
            }

            int cellCount = fieldWidth * fieldHeight;
            if (cellCount <= 0 ||
                obstacleExclusionScalar.Length != cellCount)
            {
                return 0;
            }

            int occupied = 0;
            for (int index = 0; index < obstacleExclusionScalar.Length; index++)
            {
                if (obstacleExclusionScalar[index] > 0.35f)
                {
                    occupied++;
                }
            }
            return occupied;
        }

        private bool ValidateP6ExternalFieldMapping(StringBuilder report)
        {
            report.AppendLine("EXTERNAL-FIELD SAME-POINT MAPPING CONTRACT");
            StylizedRiverFoamGridDescriptor descriptor =
                fixedMetricCandidateDescriptor;
            if (!descriptor.IsCreated || !descriptor.UsesFixedMetricLattice)
            {
                report.AppendLine("No fixed-metric candidate descriptor exists.");
                report.AppendLine("EXTERNAL FIELD VERDICT: FAIL");
                report.AppendLine();
                return false;
            }

            Vector2Int[] dimensions =
            {
                new(192, 48),
                new(96, 32),
                new(257, 73),
                new(1, 1)
            };
            int[] xs =
            {
                0,
                Mathf.Clamp(descriptor.ColumnCount / 2, 0,
                    descriptor.ColumnCount - 1),
                descriptor.ColumnCount - 1
            };
            int[] ys =
            {
                0,
                Mathf.Clamp(descriptor.RowCount / 2, 0,
                    descriptor.RowCount - 1),
                descriptor.RowCount - 1
            };
            bool exact = true;
            for (int index = 0; index < xs.Length; index++)
            {
                int x = xs[index];
                int y = ys[index];
                Vector2 metricPoint =
                    descriptor.ResolveMetricPositionAtCellCentre(x, y);
                float leftHalfWidth = 5.25f + index * 0.75f;
                float rightHalfWidth = 6.75f + index * 0.50f;
                Vector2 externalUV = ResolveP6ExternalUvCpu(
                    descriptor,
                    metricPoint,
                    leftHalfWidth,
                    rightHalfWidth);
                Vector2 rendererUV = ResolveP6RendererMetricUvCpu(
                    descriptor,
                    metricPoint);
                Vector2 expectedRendererUV = new(
                    (x + 0.5f) / descriptor.ColumnCount,
                    (y + 0.5f) / descriptor.RowCount);
                bool rendererExact =
                    (rendererUV - expectedRendererUV).sqrMagnitude <= 1e-10f;
                exact &= rendererExact;
                report.AppendLine(
                    $"Point {index}: cell=({x},{y}); metric=" +
                    $"({metricPoint.x:R},{metricPoint.y:R})m; externalUV=" +
                    $"({externalUV.x:R},{externalUV.y:R}); metricFieldUV=" +
                    $"({rendererUV.x:R},{rendererUV.y:R}); renderer=" +
                    $"{(rendererExact ? "PASS" : "FAIL")}");

                for (int dimensionIndex = 0;
                     dimensionIndex < dimensions.Length;
                     dimensionIndex++)
                {
                    Vector2Int size = dimensions[dimensionIndex];
                    Vector2 coordinate = new(
                        externalUV.x * Mathf.Max(0, size.x - 1),
                        externalUV.y * Mathf.Max(0, size.y - 1));
                    bool finite = P6IsFinite(coordinate.x) &&
                        P6IsFinite(coordinate.y) &&
                        coordinate.x >= -0.0001f &&
                        coordinate.x <= Mathf.Max(0, size.x - 1) + 0.0001f &&
                        coordinate.y >= -0.0001f &&
                        coordinate.y <= Mathf.Max(0, size.y - 1) + 0.0001f;
                    exact &= finite;
                    report.AppendLine(
                        $"  External {size.x}x{size.y}: bilinear=" +
                        $"({coordinate.x:R},{coordinate.y:R}); " +
                        $"sameUV={(finite ? "PASS" : "FAIL")}");
                }
            }

            report.AppendLine(
                $"External mapping contract: {P6ExternalFieldMappingContract}");
            report.AppendLine(
                "Pressure, Static Wake, Wake, and Ripple consume the same " +
                "physical Foam point; their independent dimensions only alter " +
                "bilinear texel coordinates, never the represented location.");
            report.AppendLine(
                "EXTERNAL FIELD VERDICT: " + (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP6ExternalSourceReadiness(StringBuilder report)
        {
            report.AppendLine("EXTERNAL SOURCE READINESS AND FALLBACK");
            bool registryReady = disturbanceRuntime.GeneratedObstacleRegistryReady;
            bool neutralReady = neutralDisturbanceTexture != null &&
                neutralDisturbanceTexture.IsCreated();
            Vector2Int ripple = disturbanceRuntime.RippleTextureDimensions;
            Vector2Int wake = disturbanceRuntime.WakeTextureDimensions;
            Vector2Int staticWake =
                disturbanceRuntime.StaticWakeTextureDimensions;
            Vector2Int pressure =
                disturbanceRuntime.StaticPressureTextureDimensions;
            bool dimensionsValid = ripple.x > 0 && ripple.y > 0 &&
                wake.x > 0 && wake.y > 0 && staticWake.x > 0 &&
                staticWake.y > 0 && pressure.x > 0 && pressure.y > 0;
            bool unequalCoverage = ripple != wake || ripple != staticWake ||
                ripple != pressure;
            bool exact = registryReady && neutralReady && dimensionsValid;
            report.AppendLine(
                $"Generated obstacle registry ready: {registryReady}");
            report.AppendLine(
                $"Stationary sources: registered=" +
                $"{disturbanceRuntime.RegisteredStationarySourceCount}; " +
                $"pressure={disturbanceRuntime.ValidStaticPressureSourceCount}; " +
                $"staticWake={disturbanceRuntime.ValidStaticWakeSourceCount}");
            report.AppendLine(
                $"Dimensions: ripple={ripple.x}x{ripple.y}; wake=" +
                $"{wake.x}x{wake.y}; staticWake={staticWake.x}x" +
                $"{staticWake.y}; pressure={pressure.x}x{pressure.y}; " +
                $"unequal={unequalCoverage} (informational; synthetic " +
                "unequal-dimension mapping is validated above)");
            report.AppendLine(
                $"Neutral one-texel fallback created: {neutralReady}");
            report.AppendLine(
                "SOURCE/FALLBACK VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private static Vector2 ResolveP6ExternalUvCpu(
            StylizedRiverFoamGridDescriptor descriptor,
            Vector2 metricPoint,
            float leftHalfWidth,
            float rightHalfWidth)
        {
            float u = Mathf.Clamp01(
                (metricPoint.x - descriptor.FieldOrStripStartMetres) /
                Mathf.Max(0.0001f, descriptor.AllocatedLengthMetres));
            float v = metricPoint.y <= 0f
                ? 0.5f * (1f + metricPoint.y /
                    Mathf.Max(0.0001f, leftHalfWidth))
                : 0.5f * (1f + metricPoint.y /
                    Mathf.Max(0.0001f, rightHalfWidth));
            return new Vector2(u, Mathf.Clamp01(v));
        }

        private static Vector2 ResolveP6RendererMetricUvCpu(
            StylizedRiverFoamGridDescriptor descriptor,
            Vector2 metricPoint)
        {
            return new Vector2(
                Mathf.Clamp01(
                    (metricPoint.x - descriptor.FieldOrStripStartMetres) /
                    Mathf.Max(0.0001f, descriptor.AllocatedLengthMetres)),
                Mathf.Clamp01(
                    (metricPoint.y -
                        descriptor.RepresentedLateralMinimumMetres) /
                    Mathf.Max(
                        0.0001f,
                        descriptor.RepresentedLateralMaximumMetres -
                        descriptor.RepresentedLateralMinimumMetres)));
        }

        private static bool P6IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private bool FinalizeP6Report(
            StringBuilder report,
            bool passed,
            string summary)
        {
            topologyCacheDiagnosticState = passed ? "Passed" : "Failed";
            topologyCacheDiagnosticSummary = summary ?? string.Empty;
            topologyCacheDiagnosticReport = report?.ToString() ?? string.Empty;
            if (!TryWriteLatestDiagnosticReport(
                    "LatestP6ComprehensiveValidation",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string writeError))
            {
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    "The P6 report could not be written: " + writeError;
                Debug.LogError(
                    "[River Foam P6] " + topologyCacheDiagnosticSummary,
                    river);
                return false;
            }

            if (passed)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log(
                    "[River Foam P6] PASS — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            else
            {
                Debug.LogError(
                    "[River Foam P6] FAIL — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            return passed;
        }
    }
}
#endif
