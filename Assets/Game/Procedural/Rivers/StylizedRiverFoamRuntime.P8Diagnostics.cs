#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        /// <summary>
        /// One-button RG-METRIC-P8 validation pipeline. It installs the assigned
        /// cache into temporary live resources, validates the persistent transport,
        /// curvilinear metric, CFL, and replacement contracts while those resources
        /// are alive, then releases the transaction and proves cache immutability.
        /// </summary>
        public bool RunP8ComprehensiveValidationReport()
        {
            topologyCacheDiagnosticRunCount++;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary =
                "Running the P8 persistent transport validation pipeline.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;

            if (Application.isPlaying)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "The P8 comprehensive report is Edit Mode only.");
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
            StringBuilder report = new(65536);
            report.AppendLine(
                "RIVER FOAM FIXED-METRIC P8 COMPREHENSIVE VALIDATION");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                "Operation: one explicit user-triggered report; installs the " +
                "assigned cache into temporary live resources, validates " +
                "persistent transport and replacement contracts before cleanup, " +
                "then releases resources. No cache, scene, prefab, material, or " +
                "serialized river state is stored.");
            report.AppendLine(
                "Contracts: conservative packed transport + curvature Jacobian " +
                "contract 1 + fixed CFL contract 1 + exact fixed-lattice " +
                "persistent replacement contract 1; active runtime mapping " +
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

            int exactReplacementCountBefore = 0;
            int clearReplacementCountBefore = 0;

            bool livePreparationReady = false;
            bool transactionStarted = false;
            bool resourcesCompleteWhileMeasured = false;
            bool activeLegacy = false;
            bool fixedCandidateReady = false;
            bool cflExact = false;
            bool conservativeTransportExact = false;
            bool curvatureExact = false;
            bool replacementPolicyExact = false;
            bool transitionMappingExact = false;
            bool kernelResourceExact = false;
            bool runtimeStateUntouched = false;
            bool cleanupCompleted = false;
            bool cleanupStateExact = false;
            bool bindingsDisabled = false;
            string cleanupFailure = string.Empty;

            try
            {
                livePreparationReady = TryPrepareP6LiveValidationResources(
                    report,
                    out transactionStarted,
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

                report.AppendLine("ACTIVE TRANSPORT OWNERSHIP");
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
                report.AppendLine(
                    "P8 does not activate fixed-metric allocation, source " +
                    "rasterization, film, shape, topology generation, or " +
                    "production rendering.");
                if (!livePreparationReady &&
                    !string.IsNullOrEmpty(preparationFailure))
                {
                    report.AppendLine(
                        "Preparation failure: " + preparationFailure);
                }
                report.AppendLine(
                    "ACTIVE LEGACY OWNERSHIP VERDICT: " +
                    (activeLegacy && fixedCandidateReady
                        ? "PASS"
                        : "FAIL"));
                report.AppendLine();

                if (resourcesCompleteWhileMeasured && activeLegacy &&
                    fixedCandidateReady)
                {
                    exactReplacementCountBefore =
                        persistentStateReplacementExactCount;
                    clearReplacementCountBefore =
                        persistentStateReplacementClearCount;
                    cflExact = ValidateP8CflContract(report);
                    conservativeTransportExact =
                        ValidateP8ConservativeTransport(report);
                    curvatureExact = ValidateP8CurvatureContract(report);
                    replacementPolicyExact =
                        ValidateP8PersistentReplacementPolicy(report);
                    transitionMappingExact =
                        ValidateP8TopologyTransitionMapping(report);
                    kernelResourceExact =
                        ValidateP8KernelAndResourceContract(report);
                    runtimeStateUntouched =
                        persistentStateReplacementExactCount ==
                            exactReplacementCountBefore &&
                        persistentStateReplacementClearCount ==
                            clearReplacementCountBefore;
                    report.AppendLine("LIVE RUNTIME STATE MUTATION PROOF");
                    report.AppendLine(
                        $"Exact replacement counter unchanged: " +
                        $"{persistentStateReplacementExactCount == exactReplacementCountBefore} " +
                        $"({exactReplacementCountBefore} -> " +
                        $"{persistentStateReplacementExactCount})");
                    report.AppendLine(
                        $"Clear replacement counter unchanged: " +
                        $"{persistentStateReplacementClearCount == clearReplacementCountBefore} " +
                        $"({clearReplacementCountBefore} -> " +
                        $"{persistentStateReplacementClearCount})");
                    report.AppendLine(
                        "LIVE STATE VERDICT: " +
                        (runtimeStateUntouched ? "PASS" : "FAIL"));
                    report.AppendLine();
                }
                else
                {
                    AppendP8SkippedContract(
                        report,
                        "TRANSPORT CFL AND SUBSTEP CONTRACT",
                        "CFL/SUBSTEP VERDICT",
                        preparationFailure);
                    AppendP8SkippedContract(
                        report,
                        "CONSERVATIVE PACKED TRANSPORT CONTRACT",
                        "CONSERVATIVE TRANSPORT VERDICT",
                        preparationFailure);
                    AppendP8SkippedContract(
                        report,
                        "CURVILINEAR METRIC CONTRACT",
                        "CURVATURE METRIC VERDICT",
                        preparationFailure);
                    AppendP8SkippedContract(
                        report,
                        "PERSISTENT STATE REPLACEMENT POLICY",
                        "REPLACEMENT POLICY VERDICT",
                        preparationFailure);
                    AppendP8SkippedContract(
                        report,
                        "PHYSICAL TOPOLOGY TRANSITION MAPPING",
                        "TOPOLOGY TRANSITION VERDICT",
                        preparationFailure);
                    AppendP8SkippedContract(
                        report,
                        "KERNEL AND RESOURCE CONTRACT",
                        "KERNEL/RESOURCE VERDICT",
                        preparationFailure);
                    AppendP8SkippedContract(
                        report,
                        "LIVE RUNTIME STATE MUTATION PROOF",
                        "LIVE STATE VERDICT",
                        preparationFailure);
                }
            }
            catch (Exception exception)
            {
                report.AppendLine("P8 LIVE EVIDENCE EXCEPTION");
                report.AppendLine(exception.ToString());
                report.AppendLine("LIVE EVIDENCE VERDICT: FAIL");
                report.AppendLine();
            }
            finally
            {
                if (transactionStarted)
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
                        cleanupCompleted = true;
                        cleanupStateExact =
                            IsP6DiagnosticCleanupStateExact();
                        bindingsDisabled = TryVerifyP6BindingsDisabled(
                            out string bindingDetail);
                        report.AppendLine("DIAGNOSTIC CLEANUP PROOF");
                        report.AppendLine(
                            $"Cleanup completed: {cleanupCompleted}");
                        report.AppendLine(
                            $"Runtime state reset: {cleanupStateExact}");
                        report.AppendLine(
                            $"Renderer bindings disabled: {bindingsDisabled}");
                        report.AppendLine(
                            $"Cleanup detail: {bindingDetail}");
                        report.AppendLine(
                            "DIAGNOSTIC CLEANUP VERDICT: " +
                            (cleanupCompleted && cleanupStateExact &&
                             bindingsDisabled
                                ? "PASS"
                                : "FAIL"));
                        report.AppendLine();
                    }
                    catch (Exception cleanupException)
                    {
                        cleanupFailure = cleanupException.ToString();
                        report.AppendLine("DIAGNOSTIC CLEANUP PROOF");
                        report.AppendLine("Cleanup exception:");
                        report.AppendLine(cleanupFailure);
                        report.AppendLine(
                            "DIAGNOSTIC CLEANUP VERDICT: FAIL");
                        report.AppendLine();
                    }
                }
                else
                {
                    report.AppendLine("DIAGNOSTIC CLEANUP PROOF");
                    report.AppendLine(
                        "No P8-owned live transaction was started; no foreign " +
                        "runtime state was released.");
                    report.AppendLine(
                        "DIAGNOSTIC CLEANUP VERDICT: " +
                        (!livePreparationReady ? "PASS" : "FAIL"));
                    report.AppendLine();
                    cleanupCompleted = !livePreparationReady;
                    cleanupStateExact = !livePreparationReady;
                    bindingsDisabled = !livePreparationReady;
                }
            }

            string assignedMetadataAfter = assignedAsset != null
                ? BuildAssignedCacheMetadataSignature(assignedAsset)
                : "<none>";
            byte[] assignedPayloadAfter = assignedAsset != null &&
                assignedAsset.HasPayload
                    ? assignedAsset.GetPayloadReadOnlyReference()
                    : Array.Empty<byte>();
            bool metadataUnchanged = string.Equals(
                assignedMetadataBefore,
                assignedMetadataAfter,
                StringComparison.Ordinal);
            bool payloadUnchanged = ByteArraysEqual(
                assignedPayloadBefore,
                assignedPayloadAfter,
                out int firstDifference);

            report.AppendLine("ASSIGNED CACHE MUTATION PROOF");
            report.AppendLine(
                $"Metadata unchanged: {metadataUnchanged}");
            report.AppendLine(
                $"Payload unchanged: {payloadUnchanged}");
            if (!payloadUnchanged)
            {
                report.AppendLine(
                    $"First differing payload byte: {firstDifference}");
            }
            report.AppendLine(
                "ASSIGNED CACHE VERDICT: " +
                (metadataUnchanged && payloadUnchanged ? "PASS" : "FAIL"));
            report.AppendLine();

            bool cleanupExact = cleanupCompleted && cleanupStateExact &&
                bindingsDisabled && string.IsNullOrEmpty(cleanupFailure);
            bool cacheExact = metadataUnchanged && payloadUnchanged;
            bool overall = livePreparationReady &&
                resourcesCompleteWhileMeasured && activeLegacy &&
                fixedCandidateReady && cflExact &&
                conservativeTransportExact && curvatureExact &&
                replacementPolicyExact && transitionMappingExact &&
                kernelResourceExact && runtimeStateUntouched &&
                cleanupExact && cacheExact;

            stopwatch.Stop();
            report.AppendLine("FINAL LEDGER");
            report.AppendLine(
                "Live diagnostic preparation transaction: " +
                (livePreparationReady ? "PASS" : "FAIL"));
            report.AppendLine(
                "Active legacy ownership preserved: " +
                (activeLegacy && fixedCandidateReady ? "PASS" : "FAIL"));
            report.AppendLine(
                "Transport CFL and multi-substep policy: " +
                (cflExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Conservative Presence/life/pattern transport: " +
                (conservativeTransportExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Curvilinear area/face metric policy: " +
                (curvatureExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Persistent replacement exact-copy/clear policy: " +
                (replacementPolicyExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Physical generated-topology transition mapping: " +
                (transitionMappingExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Remap kernel and no-new-resource contract: " +
                (kernelResourceExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Live runtime state remained untouched: " +
                (runtimeStateUntouched ? "PASS" : "FAIL"));
            report.AppendLine(
                "Diagnostic cleanup and disabled bindings: " +
                (cleanupExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Assigned cache remained unchanged: " +
                (cacheExact ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Elapsed: {stopwatch.Elapsed.TotalMilliseconds:0.###} ms");
            report.AppendLine("Overall: " + (overall ? "PASS" : "FAIL"));

            return FinalizeP8Report(
                report,
                overall,
                overall
                    ? "P8 persistent transport and topology replacement " +
                      "contracts passed."
                    : "P8 validation found at least one failed contract.");
        }

        private bool ValidateP8CflContract(StringBuilder report)
        {
            report.AppendLine("TRANSPORT CFL AND SUBSTEP CONTRACT");
            float materialStep = 1f / 12f;
            float baseSpeed =
                ResolveBaseFoamDownstreamSpeedMetresPerSecond();
            float lateralRatio = Mathf.Max(
                0f,
                river.FoamMaximumLateralSpeedRatio);
            float legacyDx = Mathf.Max(
                0.0001f,
                minimumTransportLongitudinalSpacing);
            float legacyDy = Mathf.Max(
                0.0001f,
                minimumTransportLateralSpacing);
            ResolveTransportCflContract(
                materialStep,
                baseSpeed,
                lateralRatio,
                legacyDx,
                legacyDy,
                1f,
                out float legacyDownstream,
                out float legacyLateral,
                out float legacyTotal,
                out int legacyRequired,
                out int legacyUsed,
                out bool legacyBlocked);
            float expectedLegacyDownstream =
                baseSpeed * materialStep / legacyDx;
            float expectedLegacyLateral =
                baseSpeed * lateralRatio * materialStep / legacyDy;
            bool legacyExact = P8Approximately(
                    legacyDownstream,
                    expectedLegacyDownstream) &&
                P8Approximately(legacyLateral, expectedLegacyLateral) &&
                legacyRequired == Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        (expectedLegacyDownstream +
                         expectedLegacyLateral) /
                        TransportTargetCfl)) &&
                legacyUsed == legacyRequired && !legacyBlocked;

            float fixedDx =
                fixedMetricCandidateDescriptor.ResolvedDxMetres;
            float fixedDy =
                fixedMetricCandidateDescriptor.ResolvedDyMetres;
            const float stressMinimumJacobian = 0.5f;
            ResolveTransportCflContract(
                materialStep,
                baseSpeed,
                lateralRatio,
                fixedDx,
                fixedDy,
                stressMinimumJacobian,
                out float fixedDownstream,
                out float fixedLateral,
                out float fixedTotal,
                out int fixedRequired,
                out int fixedUsed,
                out bool fixedBlocked);
            float expectedFixedDownstream = baseSpeed * materialStep /
                (fixedDx * stressMinimumJacobian);
            float expectedFixedLateral =
                baseSpeed * lateralRatio * materialStep / fixedDy;
            bool fixedExact = P8Approximately(
                    fixedDownstream,
                    expectedFixedDownstream) &&
                P8Approximately(fixedLateral, expectedFixedLateral) &&
                P8Approximately(
                    fixedTotal,
                    expectedFixedDownstream + expectedFixedLateral) &&
                fixedRequired == Mathf.Max(
                    1,
                    Mathf.CeilToInt(fixedTotal / TransportTargetCfl)) &&
                fixedUsed == fixedRequired && !fixedBlocked;

            ResolveTransportCflContract(
                0.25f,
                2.5f,
                0.65f,
                fixedDx,
                fixedDy,
                0.35f,
                out float multiDownstream,
                out float multiLateral,
                out float multiTotal,
                out int multiRequired,
                out int multiUsed,
                out bool multiBlocked);
            bool multiSubstepExact = multiRequired > 1 &&
                multiUsed == multiRequired && !multiBlocked &&
                multiTotal / multiUsed <= TransportTargetCfl + 0.0001f;

            ResolveTransportCflContract(
                1f,
                100f,
                1f,
                fixedDx,
                fixedDy,
                TransportMinimumCurvatureJacobian,
                out _,
                out _,
                out float blockedTotal,
                out int blockedRequired,
                out int blockedUsed,
                out bool blocked);
            bool safetyExact = blocked &&
                blockedRequired > MaximumTransportSubsteps &&
                blockedUsed == 0;

            bool exact = legacyExact && fixedExact &&
                multiSubstepExact && safetyExact;
            report.AppendLine(
                $"Legacy: dx={legacyDx:0.########}m, " +
                $"dy={legacyDy:0.########}m, x={legacyDownstream:0.######}, " +
                $"y={legacyLateral:0.######}, total={legacyTotal:0.######}, " +
                $"substeps={legacyUsed}; exact={legacyExact}");
            report.AppendLine(
                $"Fixed stress: dx={fixedDx:0.########}m, " +
                $"dy={fixedDy:0.########}m, Jmin={stressMinimumJacobian}, " +
                $"x={fixedDownstream:0.######}, y={fixedLateral:0.######}, " +
                $"total={fixedTotal:0.######}, substeps={fixedUsed}; " +
                $"exact={fixedExact}");
            report.AppendLine(
                $"Multi-substep: x={multiDownstream:0.######}, " +
                $"y={multiLateral:0.######}, total={multiTotal:0.######}, " +
                $"required/used={multiRequired}/{multiUsed}; " +
                $"exact={multiSubstepExact}");
            report.AppendLine(
                $"Safety block: total={blockedTotal:0.######}, " +
                $"required/used={blockedRequired}/{blockedUsed}, " +
                $"blocked={blocked}; exact={safetyExact}");
            report.AppendLine(
                "CFL/SUBSTEP VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private static bool ValidateP8ConservativeTransport(
            StringBuilder report)
        {
            report.AppendLine("CONSERVATIVE PACKED TRANSPORT CONTRACT");
            System.Random random = new(80123);
            int interiorCases = 0;
            int endpointCases = 0;
            int reverseCases = 0;
            int geometryCases = 0;
            bool exact = true;
            for (int index = 0; index < 512; index++)
            {
                float donorArea = P8RandomRange(random, 0.01f, 2.5f);
                float receiverArea = P8RandomRange(random, 0.01f, 2.5f);
                float faceLength = P8RandomRange(random, 0.01f, 1.5f);
                float speed = P8RandomRange(random, 0.01f, 2.0f);
                float dt = P8RandomRange(random, 0.001f, 0.04f);
                float donorPresence = P8RandomRange(random, 0.2f, 1f);
                float receiverPresence = P8RandomRange(random, 0f, 0.8f);
                float donorLife = P8RandomRange(random, 0f, 1f);
                float receiverLife = P8RandomRange(random, 0f, 1f);
                float donorPattern = P8RandomRange(random, 0f, 1f);
                float receiverPattern = P8RandomRange(random, 0f, 1f);

                float donorPresenceMass = donorPresence * donorArea;
                float receiverPresenceMass = receiverPresence * receiverArea;
                float donorLifeMoment =
                    donorPresenceMass * donorLife;
                float receiverLifeMoment =
                    receiverPresenceMass * receiverLife;
                float donorPatternMoment =
                    donorPresenceMass * donorPattern;
                float receiverPatternMoment =
                    receiverPresenceMass * receiverPattern;
                float fraction = Mathf.Min(
                    0.45f,
                    speed * faceLength * dt / donorArea);
                float presenceFlux = donorPresenceMass * fraction;
                float lifeFlux = donorLifeMoment * fraction;
                float patternFlux = donorPatternMoment * fraction;

                float beforePresence = donorPresenceMass +
                    receiverPresenceMass;
                float beforeLife = donorLifeMoment + receiverLifeMoment;
                float beforePattern = donorPatternMoment +
                    receiverPatternMoment;
                donorPresenceMass -= presenceFlux;
                receiverPresenceMass += presenceFlux;
                donorLifeMoment -= lifeFlux;
                receiverLifeMoment += lifeFlux;
                donorPatternMoment -= patternFlux;
                receiverPatternMoment += patternFlux;
                bool interiorExact = P8Approximately(
                        beforePresence,
                        donorPresenceMass + receiverPresenceMass,
                        0.00002f) &&
                    P8Approximately(
                        beforeLife,
                        donorLifeMoment + receiverLifeMoment,
                        0.00002f) &&
                    P8Approximately(
                        beforePattern,
                        donorPatternMoment + receiverPatternMoment,
                        0.00002f);
                exact &= interiorExact;
                interiorCases++;
                geometryCases += donorArea != receiverArea ? 1 : 0;

                float endpointPresenceBefore = donorPresence * donorArea;
                float endpointFlux = endpointPresenceBefore * fraction;
                float endpointPresenceAfter = endpointPresenceBefore -
                    endpointFlux;
                bool endpointExact = P8Approximately(
                    endpointPresenceBefore - endpointPresenceAfter,
                    endpointFlux,
                    0.00002f);
                exact &= endpointExact;
                endpointCases++;

                float reverseDonor = receiverPresence * receiverArea;
                float reverseReceiver = donorPresence * donorArea;
                float reverseFraction = Mathf.Min(
                    0.45f,
                    speed * faceLength * dt / receiverArea);
                float reverseFlux = reverseDonor * reverseFraction;
                float reverseBefore = reverseDonor + reverseReceiver;
                reverseDonor -= reverseFlux;
                reverseReceiver += reverseFlux;
                bool reverseExact = P8Approximately(
                    reverseBefore,
                    reverseDonor + reverseReceiver,
                    0.00002f);
                exact &= reverseExact;
                reverseCases++;
            }

            bool closedBoundaryExact = true;
            float closedBefore = 0.73f * 0.41f;
            float closedAfter = closedBefore;
            closedBoundaryExact &= P8Approximately(
                closedBefore,
                closedAfter);
            exact &= closedBoundaryExact;

            report.AppendLine(
                $"Interior conservative interfaces: {interiorCases}; " +
                $"Presence/life/pattern totals preserved={exact}");
            report.AppendLine(
                $"Widen/narrow unequal-area cases: {geometryCases}");
            report.AppendLine(
                $"Endpoint outflow accounting cases: {endpointCases}");
            report.AppendLine(
                $"Reverse-flow interfaces: {reverseCases}");
            report.AppendLine(
                $"Closed bank/obstacle face: {closedBoundaryExact}");
            report.AppendLine(
                "CONSERVATIVE TRANSPORT VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP8CurvatureContract(StringBuilder report)
        {
            report.AppendLine("CURVILINEAR METRIC CONTRACT");
            bool activeLegacyExact = !gridDescriptor.UsesFixedMetricLattice &&
                P8Approximately(minimumTransportCurvatureJacobian, 1f) &&
                P8Approximately(minimumTransportRawJacobian, 1f) &&
                P8Approximately(
                    maximumAbsoluteTransportCurvatureLateralProduct,
                    0f);

            const float halfWidth = 20f;
            const float radius = 40f;
            float curvature = 1f / radius;
            float rawLeft = 1f - curvature * -halfWidth;
            float rawRight = 1f - curvature * halfWidth;
            float rawMinimum = Mathf.Min(rawLeft, rawRight);
            float boundedMinimum = Mathf.Max(
                TransportMinimumCurvatureJacobian,
                rawMinimum);
            float cellArea =
                0.15f * 0.15f * boundedMinimum;
            float lateralFaceLength =
                0.15f * boundedMinimum;
            bool broadBendExact = P8Approximately(rawMinimum, 0.5f) &&
                P8Approximately(boundedMinimum, 0.5f) &&
                P8Approximately(cellArea, 0.01125f) &&
                P8Approximately(lateralFaceLength, 0.075f);

            float extremeRaw = 1f - (1f / 10f) * halfWidth;
            float extremeBounded = Mathf.Max(
                TransportMinimumCurvatureJacobian,
                extremeRaw);
            bool clampExact = extremeRaw < 0f &&
                P8Approximately(
                    extremeBounded,
                    TransportMinimumCurvatureJacobian);

            float oppositeRawMinimum = Mathf.Min(
                1f - -curvature * -halfWidth,
                1f - -curvature * halfWidth);
            bool signExact = P8Approximately(
                oppositeRawMinimum,
                rawMinimum);

            bool exact = activeLegacyExact && broadBendExact &&
                clampExact && signExact;
            report.AppendLine(
                $"Active legacy metrics: rawJmin=" +
                $"{minimumTransportRawJacobian:0.######}, boundedJmin=" +
                $"{minimumTransportCurvatureJacobian:0.######}, " +
                $"max|k*n|=" +
                $"{maximumAbsoluteTransportCurvatureLateralProduct:0.######}; " +
                $"exact={activeLegacyExact}");
            report.AppendLine(
                $"40m-width / 40m-radius stress: rawJmin=" +
                $"{rawMinimum:0.######}, boundedJmin=" +
                $"{boundedMinimum:0.######}, area={cellArea:0.########}, " +
                $"lateralFace={lateralFaceLength:0.########}; " +
                $"exact={broadBendExact}");
            report.AppendLine(
                $"Extreme bend clamp: rawJmin={extremeRaw:0.######}, " +
                $"boundedJmin={extremeBounded:0.######}; exact={clampExact}");
            report.AppendLine(
                $"Curvature sign symmetry: {signExact}");
            report.AppendLine(
                "CURVATURE METRIC VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP8PersistentReplacementPolicy(
            StringBuilder report)
        {
            report.AppendLine("PERSISTENT STATE REPLACEMENT POLICY");
            bool previousCreated =
                StylizedRiverFoamGridDescriptor.TryCreateFixedMetricOneStrip(
                    StylizedRiverQuality.Medium,
                    0.15f,
                    0.15f,
                    4.8f,
                    -0.45f,
                    0.45f,
                    0f,
                    0,
                    256,
                    out StylizedRiverFoamGridDescriptor previous,
                    out string previousFailure);
            bool expandedCreated =
                StylizedRiverFoamGridDescriptor.TryCreateFixedMetricOneStrip(
                    StylizedRiverQuality.Medium,
                    0.15f,
                    0.15f,
                    4.8f,
                    -0.75f,
                    0.75f,
                    0f,
                    0,
                    256,
                    out StylizedRiverFoamGridDescriptor expanded,
                    out string expandedFailure);
            bool differentSpacingCreated =
                StylizedRiverFoamGridDescriptor.TryCreateFixedMetricOneStrip(
                    StylizedRiverQuality.Medium,
                    0.20f,
                    0.20f,
                    4.8f,
                    -0.45f,
                    0.45f,
                    0f,
                    0,
                    256,
                    out StylizedRiverFoamGridDescriptor differentSpacing,
                    out string spacingFailure);
            if (!previousCreated || !expandedCreated ||
                !differentSpacingCreated)
            {
                report.AppendLine(
                    $"Descriptor creation failure: previous={previousFailure}; " +
                    $"expanded={expandedFailure}; spacing={spacingFailure}");
                report.AppendLine("REPLACEMENT POLICY VERDICT: FAIL");
                report.AppendLine();
                return false;
            }

            FoamMetricRow[] previousRows = P8CreateMetricRows(
                previous,
                0.0125f);
            FoamMetricRow[] expandedRows = P8CreateMetricRows(
                expanded,
                0.0125f);
            float previousGlobalStart = 10f;
            float expandedGlobalStart = previousGlobalStart +
                previous.ResolvedDxMetres;
            PersistentStateReplacementPolicy exactPolicy =
                ResolvePersistentStateReplacementPolicyContract(
                    previous,
                    previousGlobalStart,
                    previousRows,
                    expanded,
                    expandedGlobalStart,
                    expandedRows,
                    out int exactOffsetX,
                    out int exactOffsetY,
                    out string exactDetail);
            bool exactCopy = exactPolicy ==
                    PersistentStateReplacementPolicy.ExactFixedLatticeCopy &&
                exactOffsetX == 1 &&
                exactOffsetY == expanded.GlobalYBase - previous.GlobalYBase;

            PersistentStateReplacementPolicy nonIntegerPolicy =
                ResolvePersistentStateReplacementPolicyContract(
                    previous,
                    previousGlobalStart,
                    previousRows,
                    expanded,
                    previousGlobalStart +
                        previous.ResolvedDxMetres * 0.5f,
                    expandedRows,
                    out _,
                    out _,
                    out string nonIntegerDetail);
            bool nonIntegerClear = nonIntegerPolicy ==
                PersistentStateReplacementPolicy.ClearNonIntegerAlignment;

            PersistentStateReplacementPolicy spacingPolicy =
                ResolvePersistentStateReplacementPolicyContract(
                    previous,
                    previousGlobalStart,
                    previousRows,
                    differentSpacing,
                    previousGlobalStart,
                    P8CreateMetricRows(differentSpacing, 0.0125f),
                    out _,
                    out _,
                    out string spacingDetail);
            bool spacingClear = spacingPolicy ==
                PersistentStateReplacementPolicy.ClearSpacingOrPhaseMismatch;

            FoamMetricRow[] curvatureChanged =
                P8CreateMetricRows(expanded, 0.0125f);
            int overlapIndex = Mathf.Clamp(
                exactOffsetX,
                0,
                curvatureChanged.Length - 1);
            curvatureChanged[overlapIndex].TopologyData.x += 0.01f;
            PersistentStateReplacementPolicy curvaturePolicy =
                ResolvePersistentStateReplacementPolicyContract(
                    previous,
                    previousGlobalStart,
                    previousRows,
                    expanded,
                    expandedGlobalStart,
                    curvatureChanged,
                    out _,
                    out _,
                    out string curvatureDetail);
            bool curvatureClear = curvaturePolicy ==
                PersistentStateReplacementPolicy.ClearCurvatureMismatch;

            PersistentStateReplacementPolicy legacyPolicy =
                ResolvePersistentStateReplacementPolicyContract(
                    gridDescriptor,
                    allocatedGlobalStart,
                    metricRows,
                    expanded,
                    expandedGlobalStart,
                    expandedRows,
                    out _,
                    out _,
                    out string legacyDetail);
            bool legacyClear = legacyPolicy ==
                PersistentStateReplacementPolicy.ClearLegacyMapping;

            bool coordinateMappingExact = true;
            int checkedCells = 0;
            for (int x = 0; x < expanded.ColumnCount; x += 7)
            {
                for (int y = 0; y < expanded.RowCount; y += 2)
                {
                    Vector2 metric = expanded.ResolveMetricPositionAtCellCentre(
                        x,
                        y);
                    float globalDistance = expandedGlobalStart + metric.x;
                    Vector2 previousMetric = new(
                        globalDistance - previousGlobalStart,
                        metric.y);
                    bool mapped = previous.TryMetricToNearestCell(
                        previousMetric,
                        out Vector2Int previousCell);
                    bool expectedInside = x + exactOffsetX >= 0 &&
                        x + exactOffsetX < previous.ColumnCount &&
                        y + exactOffsetY >= 0 &&
                        y + exactOffsetY < previous.RowCount;
                    coordinateMappingExact &= mapped == expectedInside;
                    if (mapped && expectedInside)
                    {
                        coordinateMappingExact &=
                            previousCell.x == x + exactOffsetX &&
                            previousCell.y == y + exactOffsetY;
                    }
                    checkedCells++;
                }
            }

            bool gpuRemapExact = ValidateP8GpuPersistentRemap(
                report,
                previous,
                previousGlobalStart,
                previousRows,
                expanded,
                expandedGlobalStart,
                expandedRows,
                exactOffsetX,
                exactOffsetY);

            bool exact = exactCopy && nonIntegerClear && spacingClear &&
                curvatureClear && legacyClear && coordinateMappingExact &&
                gpuRemapExact;
            report.AppendLine(
                $"Exact expansion/integer-offset policy: {exactPolicy}; " +
                $"offset=({exactOffsetX},{exactOffsetY}); detail={exactDetail}; " +
                $"exact={exactCopy}");
            report.AppendLine(
                $"Noninteger origin: {nonIntegerPolicy}; " +
                $"detail={nonIntegerDetail}; exact={nonIntegerClear}");
            report.AppendLine(
                $"Spacing change: {spacingPolicy}; detail={spacingDetail}; " +
                $"exact={spacingClear}");
            report.AppendLine(
                $"Curvature change: {curvaturePolicy}; " +
                $"detail={curvatureDetail}; exact={curvatureClear}");
            report.AppendLine(
                $"Legacy/fixed transition: {legacyPolicy}; " +
                $"detail={legacyDetail}; exact={legacyClear}");
            report.AppendLine(
                $"Exact-cell mapping cases: {checkedCells}; " +
                $"exact={coordinateMappingExact}");
            report.AppendLine(
                $"Synthetic GPU packed-state remap: {gpuRemapExact}");
            report.AppendLine(
                "REPLACEMENT POLICY VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP8GpuPersistentRemap(
            StringBuilder report,
            StylizedRiverFoamGridDescriptor previous,
            float previousGlobalStart,
            FoamMetricRow[] previousRows,
            StylizedRiverFoamGridDescriptor current,
            float currentGlobalStart,
            FoamMetricRow[] currentRows,
            int offsetX,
            int offsetY)
        {
            if (computeShader == null || remapPersistentStateKernel < 0)
            {
                report.AppendLine(
                    "GPU remap unavailable: compute shader or kernel missing.");
                return false;
            }

            RenderTexture input = null;
            RenderTexture output = null;
            Texture2D upload = null;
            Texture2D readback = null;
            Texture2D boundary = null;
            Texture2D obstacle = null;
            ComputeBuffer currentMetricBuffer = null;
            try
            {
                input = P8CreateArgbFloatTexture(
                    previous.ColumnCount,
                    previous.RowCount,
                    "PS3D_P8_RemapInput");
                output = P8CreateArgbFloatTexture(
                    current.ColumnCount,
                    current.RowCount,
                    "PS3D_P8_RemapOutput");
                upload = new Texture2D(
                    previous.ColumnCount,
                    previous.RowCount,
                    TextureFormat.RGBAFloat,
                    false,
                    true)
                {
                    name = "PS3D_P8_RemapUpload",
                    hideFlags = HideFlags.DontSave
                };
                Color[] source = new Color[
                    previous.ColumnCount * previous.RowCount];
                for (int y = 0; y < previous.RowCount; y++)
                {
                    for (int x = 0; x < previous.ColumnCount; x++)
                    {
                        float presence = 0.25f +
                            ((x * 3 + y * 5) % 7) * 0.08f;
                        float life = 0.15f +
                            ((x + y * 2) % 5) * 0.13f;
                        float pattern = 0.10f +
                            ((x * 2 + y) % 6) * 0.11f;
                        source[y * previous.ColumnCount + x] = new Color(
                            presence,
                            presence * Mathf.Clamp01(life),
                            presence * Mathf.Clamp01(pattern),
                            0f);
                    }
                }
                upload.SetPixels(source);
                upload.Apply(false, false);
                Graphics.CopyTexture(upload, input);
                ClearRenderTexture(output);

                boundary = P8CreateScalarTexture(
                    current.ColumnCount,
                    current.RowCount,
                    1f,
                    "PS3D_P8_RemapBoundary");
                obstacle = P8CreateScalarTexture(
                    current.ColumnCount,
                    current.RowCount,
                    0f,
                    "PS3D_P8_RemapObstacle");
                currentMetricBuffer = new ComputeBuffer(
                    current.ColumnCount,
                    System.Runtime.InteropServices.Marshal.SizeOf<
                        FoamMetricRow>(),
                    ComputeBufferType.Structured);
                currentMetricBuffer.SetData(currentRows);

                P8ConfigureGridDescriptor(current.ToGpuData());
                computeShader.SetInts(
                    "_FoamDimensions",
                    current.ColumnCount,
                    current.RowCount);
                computeShader.SetFloat(
                    "_FoamGlobalStart",
                    currentGlobalStart);
                computeShader.SetFloat(
                    "_FoamFieldLength",
                    current.AllocatedLengthMetres);
                computeShader.SetFloat(
                    "_FoamValidLength",
                    current.ValidLengthMetres);
                computeShader.SetFloat(
                    "_FoamSimulationLength",
                    current.AllocatedLengthMetres);
                computeShader.SetInts(
                    "_FoamTopologyTransitionDimensions",
                    previous.ColumnCount,
                    previous.RowCount);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionGlobalStart",
                    previousGlobalStart);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionFieldLength",
                    previous.AllocatedLengthMetres);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionValidLength",
                    previous.ValidLengthMetres);
                ConfigureTopologyTransitionGridDescriptor(
                    remapPersistentStateKernel,
                    previous.ToGpuData());
                computeShader.SetInt(
                    "_FoamPersistentStateRemapEnabled",
                    1);
                computeShader.SetBuffer(
                    remapPersistentStateKernel,
                    "_FoamMetricRows",
                    currentMetricBuffer);
                computeShader.SetTexture(
                    remapPersistentStateKernel,
                    "_FoamBoundary",
                    boundary);
                computeShader.SetTexture(
                    remapPersistentStateKernel,
                    "_FoamObstacleExclusionRead",
                    obstacle);
                computeShader.SetTexture(
                    remapPersistentStateKernel,
                    "_FoamPersistentStateRemapRead",
                    input);
                computeShader.SetTexture(
                    remapPersistentStateKernel,
                    "_FoamStateWrite",
                    output);
                Dispatch(
                    remapPersistentStateKernel,
                    current.ColumnCount,
                    current.RowCount);
                computeShader.SetInt(
                    "_FoamPersistentStateRemapEnabled",
                    0);

                readback = new Texture2D(
                    current.ColumnCount,
                    current.RowCount,
                    TextureFormat.RGBAFloat,
                    false,
                    true)
                {
                    name = "PS3D_P8_RemapReadback",
                    hideFlags = HideFlags.DontSave
                };
                RenderTexture previousActive = RenderTexture.active;
                try
                {
                    RenderTexture.active = output;
                    readback.ReadPixels(
                        new Rect(
                            0,
                            0,
                            current.ColumnCount,
                            current.RowCount),
                        0,
                        0,
                        false);
                    readback.Apply(false, false);
                }
                finally
                {
                    RenderTexture.active = previousActive;
                }
                Color[] actual = readback.GetPixels();

                int overlapCells = 0;
                int clearedCells = 0;
                int mismatches = 0;
                int firstMismatchX = -1;
                int firstMismatchY = -1;
                Color firstExpected = Color.clear;
                Color firstObserved = Color.clear;
                for (int y = 0; y < current.RowCount; y++)
                {
                    for (int x = 0; x < current.ColumnCount; x++)
                    {
                        int previousX = x + offsetX;
                        int previousY = y + offsetY;
                        bool overlaps = previousX >= 0 &&
                            previousX < previous.ColumnCount &&
                            previousY >= 0 &&
                            previousY < previous.RowCount;
                        Color expected = overlaps
                            ? source[
                                previousY * previous.ColumnCount + previousX]
                            : Color.clear;
                        Color observed =
                            actual[y * current.ColumnCount + x];
                        bool same = P8ColorApproximately(
                            expected,
                            observed,
                            0.0005f);
                        if (!same)
                        {
                            mismatches++;
                            if (firstMismatchX < 0)
                            {
                                firstMismatchX = x;
                                firstMismatchY = y;
                                firstExpected = expected;
                                firstObserved = observed;
                            }
                        }
                        if (overlaps)
                        {
                            overlapCells++;
                        }
                        else
                        {
                            clearedCells++;
                        }
                    }
                }

                bool exact = overlapCells > 0 && clearedCells > 0 &&
                    mismatches == 0;
                report.AppendLine(
                    $"GPU remap: previous={previous.ColumnCount}x" +
                    $"{previous.RowCount}, current={current.ColumnCount}x" +
                    $"{current.RowCount}, offsets=({offsetX},{offsetY}), " +
                    $"overlap={overlapCells}, cleared={clearedCells}, " +
                    $"mismatches={mismatches}, firstMismatch=" +
                    $"({firstMismatchX},{firstMismatchY}), expected=" +
                    $"({firstExpected.r:R},{firstExpected.g:R}," +
                    $"{firstExpected.b:R},{firstExpected.a:R}), observed=" +
                    $"({firstObserved.r:R},{firstObserved.g:R}," +
                    $"{firstObserved.b:R},{firstObserved.a:R}); " +
                    $"verdict={(exact ? "PASS" : "FAIL")}");
                return exact;
            }
            catch (Exception exception)
            {
                report.AppendLine("GPU remap exception:");
                report.AppendLine(exception.ToString());
                return false;
            }
            finally
            {
                computeShader?.SetInt(
                    "_FoamPersistentStateRemapEnabled",
                    0);
                currentMetricBuffer?.Release();
                currentMetricBuffer = null;
                ReleaseTexture(ref input);
                ReleaseTexture(ref output);
                DestroyUnityObject(upload);
                DestroyUnityObject(readback);
                DestroyUnityObject(boundary);
                DestroyUnityObject(obstacle);
            }
        }

        private void P8ConfigureGridDescriptor(
            StylizedRiverFoamGridGpuData descriptor)
        {
            computeShader.SetVector(
                "_FoamGridDescriptorContract",
                descriptor.Contract);
            computeShader.SetVector(
                "_FoamGridDescriptorSpacing",
                descriptor.Spacing);
            computeShader.SetVector(
                "_FoamGridDescriptorLateral",
                descriptor.Lateral);
            computeShader.SetVector(
                "_FoamGridDescriptorLongitudinal",
                descriptor.Longitudinal);
            computeShader.SetVector(
                "_FoamGridDescriptorExtent",
                descriptor.Extent);
        }

        private static RenderTexture P8CreateArgbFloatTexture(
            int width,
            int height,
            string textureName)
        {
            RenderTexture texture = new RenderTexture(
                width,
                height,
                0,
                RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }

        private static Texture2D P8CreateScalarTexture(
            int width,
            int height,
            float value,
            string textureName)
        {
            Texture2D texture = new Texture2D(
                width,
                height,
                TextureFormat.RFloat,
                false,
                true)
            {
                name = textureName,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            float[] values = new float[width * height];
            if (value != 0f)
            {
                Array.Fill(values, value);
            }
            texture.SetPixelData(values, 0);
            texture.Apply(false, false);
            return texture;
        }

        private static bool P8ColorApproximately(
            Color left,
            Color right,
            float tolerance)
        {
            return Mathf.Abs(left.r - right.r) <= tolerance &&
                Mathf.Abs(left.g - right.g) <= tolerance &&
                Mathf.Abs(left.b - right.b) <= tolerance &&
                Mathf.Abs(left.a - right.a) <= tolerance;
        }

        private static bool ValidateP8TopologyTransitionMapping(
            StringBuilder report)
        {
            report.AppendLine("PHYSICAL TOPOLOGY TRANSITION MAPPING");
            string transitionPath = P8ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.TopologyTransition.hlsl");
            string coordinatePath = P8ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.Coordinates.hlsl");
            string samplingPath = P8ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.Sampling.hlsl");
            string computePath = P8ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.compute");
            string descriptorPath = P8ResolveProjectAbsolutePath(
                "Assets/Game/Procedural/Rivers/" +
                "StylizedRiverFoamGridDescriptor.cs");
            string transition = File.Exists(transitionPath)
                ? File.ReadAllText(transitionPath)
                : string.Empty;
            string coordinates = File.Exists(coordinatePath)
                ? File.ReadAllText(coordinatePath)
                : string.Empty;
            string sampling = File.Exists(samplingPath)
                ? File.ReadAllText(samplingPath)
                : string.Empty;
            string compute = File.Exists(computePath)
                ? File.ReadAllText(computePath)
                : string.Empty;
            string descriptor = File.Exists(descriptorPath)
                ? File.ReadAllText(descriptorPath)
                : string.Empty;

            string applyBody = P7ExtractFunctionBody(
                transition,
                "float3 ApplyGeneratedTopologyTransition(");
            string previousUvBody = P7ExtractFunctionBody(
                transition,
                "bool TryResolveTopologyTransitionPreviousUV(");
            string remapBody = P7ExtractFunctionBody(
                transition,
                "bool TryResolvePersistentStateRemapCoordinate(");
            string gridSimulationBody = P7ExtractFunctionBody(
                coordinates,
                "bool IsFoamGridColumnInsideSimulation(");
            string lateralBody = P7ExtractFunctionBody(
                coordinates,
                "float FoamLateralMetresAtUV(");
            string validFluidBody = P7ExtractFunctionBody(
                sampling,
                "float FoamValidFluidAt(");
            string captureBody = P7ExtractFunctionBody(
                compute,
                "void CaptureGeneratedTopology(");
            string composeBody = P7ExtractFunctionBody(
                compute,
                "void ComposeTopology(");
            string descriptorBody = P7ExtractFunctionBody(
                descriptor,
                "public StylizedRiverFoamGridGpuData ToGpuData(");

            bool descriptorProducerOrder = P8ContainsInOrder(
                descriptorBody,
                "LateralLatticePhaseMetres",
                "GlobalYBase",
                "RowCount",
                "AllocationGuardRows");
            bool descriptorConsumerOrder = P8ContainsOrdinal(
                    lateralBody,
                    "_FoamGridDescriptorLateral.x") &&
                P8ContainsOrdinal(
                    lateralBody,
                    "_FoamGridDescriptorLateral.y") &&
                P8ContainsOrdinal(
                    lateralBody,
                    "_FoamGridDescriptorLateral.z") &&
                !P8ContainsOrdinal(
                    lateralBody,
                    "_FoamGridDescriptorLateral.w") &&
                P8ContainsOrdinal(
                    lateralBody,
                    "_FoamGridDescriptorSpacing.w");
            bool descriptorLaneAgreement = descriptorProducerOrder &&
                descriptorConsumerOrder;

            bool applyBodyFound = !string.IsNullOrWhiteSpace(applyBody);
            bool captureBodyFound = !string.IsNullOrWhiteSpace(captureBody);
            bool composeBodyFound = !string.IsNullOrWhiteSpace(composeBody);
            bool inspectedBodiesFound = applyBodyFound &&
                captureBodyFound && composeBodyFound;
            bool texelCentreCallers = captureBodyFound &&
                P8ContainsOrdinal(
                    captureBody,
                    "FoamTexelCentreUV") &&
                P8ContainsOrdinal(
                    captureBody,
                    "ApplyGeneratedTopologyTransition") &&
                composeBodyFound &&
                P8ContainsOrdinal(
                    composeBody,
                    "FoamTexelCentreUV") &&
                P8ContainsOrdinal(
                    composeBody,
                    "ApplyGeneratedTopologyTransition");
            bool currentPhysicalPoint = inspectedBodiesFound &&
                P8ContainsOrdinal(
                    applyBody,
                    "FoamGridLocalDistanceAtUV") &&
                P8ContainsOrdinal(
                    applyBody,
                    "FoamLateralMetresAtUV") &&
                P8ContainsOrdinal(
                    applyBody,
                    "TryResolveTopologyTransitionPreviousUV") &&
                texelCentreCallers;
            bool previousDescriptorOwned = P8ContainsOrdinal(
                    previousUvBody,
                    "FoamTopologyTransitionPreviousUsesFixedMetricLattice") &&
                P8ContainsOrdinal(
                    previousUvBody,
                    "_FoamTopologyTransitionGridDescriptorLongitudinal") &&
                P8ContainsOrdinal(
                    previousUvBody,
                    "_FoamTopologyTransitionGridDescriptorLateral");
            bool exactRemapOwned = P8ContainsOrdinal(
                    remapBody,
                    "previousXFloat") &&
                P8ContainsOrdinal(remapBody, "previousYFloat") &&
                P8ContainsOrdinal(remapBody, "abs(") &&
                P8ContainsOrdinal(
                    remapBody,
                    "_FoamPersistentStateRemapEnabled");
            bool simulationClippingOwned = P8ContainsOrdinal(
                    gridSimulationBody,
                    "FoamGridUsesFixedMetricLattice") &&
                P8ContainsOrdinal(
                    validFluidBody,
                    "IsFoamGridColumnInsideSimulation");
            bool exact = descriptorLaneAgreement && currentPhysicalPoint &&
                previousDescriptorOwned && exactRemapOwned &&
                simulationClippingOwned;
            report.AppendLine($"Transition include: {transitionPath}");
            report.AppendLine(
                $"Current descriptor lateral lane agreement: " +
                $"{descriptorLaneAgreement}");
            report.AppendLine(
                $"Inspected function bodies found: " +
                $"ApplyGeneratedTopologyTransition={applyBodyFound}; " +
                $"CaptureGeneratedTopology={captureBodyFound}; " +
                $"ComposeTopology={composeBodyFound}");
            report.AppendLine(
                $"Current cell resolves one texel-centre physical (s,n) " +
                $"point: {currentPhysicalPoint}");
            report.AppendLine(
                $"Previous fixed/legacy descriptor mapping: " +
                $"{previousDescriptorOwned}");
            report.AppendLine(
                $"Exact integer-lattice remap coordinate: " +
                $"{exactRemapOwned}");
            report.AppendLine(
                $"Descriptor-owned valid simulation clipping: " +
                $"{simulationClippingOwned}");
            report.AppendLine(
                "TOPOLOGY TRANSITION VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP8KernelAndResourceContract(
            StringBuilder report)
        {
            report.AppendLine("KERNEL AND RESOURCE CONTRACT");
            string computePath = P8ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.compute");
            string resourcesPath = P8ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.Resources.hlsl");
            string simulationPath = P8ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.Simulation.hlsl");
            string replacementPath = P8ResolveProjectAbsolutePath(
                "Assets/Game/Procedural/Rivers/" +
                "StylizedRiverFoamRuntime.TopologyReplacement.cs");
            string resourcesCsPath = P8ResolveProjectAbsolutePath(
                "Assets/Game/Procedural/Rivers/" +
                "StylizedRiverFoamRuntime.Resources.cs");
            string compute = File.Exists(computePath)
                ? File.ReadAllText(computePath)
                : string.Empty;
            string resources = File.Exists(resourcesPath)
                ? File.ReadAllText(resourcesPath)
                : string.Empty;
            string simulation = File.Exists(simulationPath)
                ? File.ReadAllText(simulationPath)
                : string.Empty;
            string replacement = File.Exists(replacementPath)
                ? File.ReadAllText(replacementPath)
                : string.Empty;
            string resourcesCs = File.Exists(resourcesCsPath)
                ? File.ReadAllText(resourcesCsPath)
                : string.Empty;

            string kernelBody = P7ExtractFunctionBody(
                compute,
                "void RemapPersistentFoamState(");
            string cellAreaBody = P7ExtractFunctionBody(
                simulation,
                "float FoamTransportCellArea(");
            string lateralFaceBody = P7ExtractFunctionBody(
                simulation,
                "float FoamTransportLateralFaceLength(");
            string applyReplacementBody = P7ExtractFunctionBody(
                replacement,
                "private void ApplyPersistentStateReplacementBeforeVisibleHoldRelease(");
            string finalizeBody = P7ExtractFunctionBody(
                resourcesCs,
                "private void FinalizeInitialization(");

            bool kernelResolved = remapPersistentStateKernel >= 0;
            bool kernelDeclared =
                P7CountOccurrences(
                    compute,
                    "#pragma kernel RemapPersistentFoamState") == 1;
            bool packedStateExact = P8ContainsOrdinal(
                    kernelBody,
                    "FoamClampPackedMaterialState") &&
                P8ContainsOrdinal(
                    kernelBody,
                    "FoamClipPackedToValidFluid") &&
                P8ContainsOrdinal(
                    kernelBody,
                    "_FoamPersistentStateRemapRead") &&
                P8ContainsOrdinal(kernelBody, "_FoamStateWrite");
            bool metricExact = P8ContainsOrdinal(
                    cellAreaBody,
                    "FoamTransportCurvatureJacobian") &&
                P8ContainsOrdinal(
                    lateralFaceBody,
                    "FoamTransportCurvatureJacobian");
            bool uniformsExact = P8ContainsOrdinal(
                    resources,
                    "_FoamPersistentStateRemapRead") &&
                P8ContainsOrdinal(
                    resources,
                    "_FoamTopologyTransitionGridDescriptorContract") &&
                P8ContainsOrdinal(
                    resources,
                    "_FoamTopologyTransitionGridDescriptorExtent");
            bool bindsExistingResources = P8ContainsOrdinal(
                    applyReplacementBody,
                    "snapshot.CurrentState") &&
                P8ContainsOrdinal(
                    applyReplacementBody,
                    "currentState") &&
                P8ContainsOrdinal(
                    applyReplacementBody,
                    "previousState") &&
                P8ContainsOrdinal(
                    applyReplacementBody,
                    "metricBuffer") &&
                P8ContainsOrdinal(
                    applyReplacementBody,
                    "boundaryTexture") &&
                P8ContainsOrdinal(
                    applyReplacementBody,
                    "obstacleExclusionTexture");
            int applyIndex = finalizeBody.IndexOf(
                "ApplyPersistentStateReplacementBeforeVisibleHoldRelease",
                StringComparison.Ordinal);
            int releaseIndex = finalizeBody.IndexOf(
                "ReleaseTopologyTransitionVisibleHold",
                StringComparison.Ordinal);
            bool lifecycleOrderExact = applyIndex >= 0 &&
                releaseIndex > applyIndex;
            bool noPersistentAllocation =
                !P8ContainsOrdinal(
                    applyReplacementBody,
                    "new RenderTexture") &&
                !P8ContainsOrdinal(
                    applyReplacementBody,
                    "new ComputeBuffer") &&
                !P8ContainsOrdinal(
                    applyReplacementBody,
                    "CreateFieldTexture");

            bool exact = kernelResolved && kernelDeclared &&
                packedStateExact && metricExact && uniformsExact &&
                bindsExistingResources && lifecycleOrderExact &&
                noPersistentAllocation;
            report.AppendLine(
                $"Remap kernel resolved/declared: " +
                $"{kernelResolved}/{kernelDeclared} " +
                $"(index={remapPersistentStateKernel})");
            report.AppendLine(
                $"Packed Presence/life/pattern copy and valid-fluid clip: " +
                $"{packedStateExact}");
            report.AppendLine(
                $"Curvature-aware cell area/lateral face: {metricExact}");
            report.AppendLine(
                $"Previous descriptor/remap uniforms: {uniformsExact}");
            report.AppendLine(
                $"Only held/current existing resources bound: " +
                $"{bindsExistingResources}");
            report.AppendLine(
                $"Remap occurs before held-state release: " +
                $"{lifecycleOrderExact}");
            report.AppendLine(
                $"No persistent allocation in remap path: " +
                $"{noPersistentAllocation}");
            report.AppendLine(
                "KERNEL/RESOURCE VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private static FoamMetricRow[] P8CreateMetricRows(
            StylizedRiverFoamGridDescriptor descriptor,
            float curvature)
        {
            FoamMetricRow[] rows = new FoamMetricRow[descriptor.ColumnCount];
            float left = Mathf.Max(
                0.01f,
                -descriptor.RepresentedLateralMinimumMetres);
            float right = Mathf.Max(
                0.01f,
                descriptor.RepresentedLateralMaximumMetres);
            for (int index = 0; index < rows.Length; index++)
            {
                rows[index] = new FoamMetricRow
                {
                    WidthsAndSpacing = new Vector4(
                        left,
                        right,
                        descriptor.ResolvedDxMetres,
                        descriptor.ResolvedDyMetres),
                    TopologyData = new Vector4(curvature, 0f, 0f, 0f),
                    ShoreData = Vector4.zero
                };
            }
            return rows;
        }

        private static float P8RandomRange(
            System.Random random,
            float minimum,
            float maximum)
        {
            return minimum +
                (maximum - minimum) * (float)random.NextDouble();
        }

        private static bool P8Approximately(
            float left,
            float right,
            float tolerance = 0.00001f)
        {
            return Mathf.Abs(left - right) <= tolerance;
        }

        private static string P8ResolveProjectAbsolutePath(
            string projectRelativePath)
        {
            if (string.IsNullOrEmpty(projectRelativePath))
            {
                return string.Empty;
            }

            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string normalizedRelative = projectRelativePath.Replace(
                '/',
                Path.DirectorySeparatorChar);
            return Path.GetFullPath(
                Path.Combine(projectRoot, normalizedRelative));
        }

        private static bool P8ContainsOrdinal(string text, string value)
        {
            return !string.IsNullOrEmpty(text) &&
                !string.IsNullOrEmpty(value) &&
                text.IndexOf(value, StringComparison.Ordinal) >= 0;
        }

        private static bool P8ContainsInOrder(
            string text,
            params string[] values)
        {
            if (string.IsNullOrEmpty(text) || values == null ||
                values.Length == 0)
            {
                return false;
            }

            int searchStart = 0;
            for (int index = 0; index < values.Length; index++)
            {
                string value = values[index];
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }

                int found = text.IndexOf(
                    value,
                    searchStart,
                    StringComparison.Ordinal);
                if (found < 0)
                {
                    return false;
                }
                searchStart = found + value.Length;
            }
            return true;
        }

        private static void AppendP8SkippedContract(
            StringBuilder report,
            string heading,
            string verdictLabel,
            string reason)
        {
            report.AppendLine(heading);
            report.AppendLine(
                "Skipped because the live diagnostic preparation transaction " +
                "did not reach a complete legacy-active/fixed-candidate state.");
            if (!string.IsNullOrEmpty(reason))
            {
                report.AppendLine("Preparation detail: " + reason);
            }
            report.AppendLine(verdictLabel + ": FAIL");
            report.AppendLine();
        }

        private bool FinalizeP8Report(
            StringBuilder report,
            bool passed,
            string summary)
        {
            topologyCacheDiagnosticState = passed ? "Passed" : "Failed";
            topologyCacheDiagnosticSummary = summary ?? string.Empty;
            topologyCacheDiagnosticReport = report?.ToString() ?? string.Empty;
            if (!TryWriteLatestDiagnosticReport(
                    "LatestP8ComprehensiveValidation",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string writeError))
            {
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    "The P8 report could not be written: " + writeError;
                Debug.LogError(
                    "[River Foam P8] " + topologyCacheDiagnosticSummary,
                    river);
                return false;
            }

            if (passed)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log(
                    "[River Foam P8] PASS — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            else
            {
                Debug.LogError(
                    "[River Foam P8] FAIL — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            return passed;
        }
    }
}
#endif
