#if UNITY_EDITOR
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        /// <summary>
        /// One-button RG-METRIC-P7 validation pipeline. It installs the assigned
        /// current cache into temporary live resources, validates automatic and
        /// manual source unit/range contracts while those resources remain live,
        /// then releases the transaction and proves cache immutability.
        /// </summary>
        public bool RunP7ComprehensiveValidationReport()
        {
            topologyCacheDiagnosticRunCount++;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary =
                "Running the P7 automatic/manual source unit validation pipeline.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;

            if (Application.isPlaying)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "The P7 comprehensive report is Edit Mode only.");
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
            StringBuilder report = new(49152);
            report.AppendLine(
                "RIVER FOAM FIXED-METRIC P7 COMPREHENSIVE VALIDATION");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                "Operation: one explicit user-triggered report; installs the " +
                "assigned cache into temporary live resources, validates source " +
                "contracts before cleanup, then releases resources. No cache, " +
                "scene, prefab, material, or serialized river state is stored.");
            report.AppendLine(
                "Contracts: source-unit contract 1 + legacy-exact source ranges + " +
                "fixed-metric physical source ranges; active runtime mapping " +
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

            int pendingInjectionCountBefore = pendingInjections.Count;
            int pendingMaterialBirthCountBefore = pendingMaterialBirths.Count;
            int activeCompositionCountBefore = activeFoamCompositionEventCount;
            int activeAutomaticCountBefore = activeAutomaticFoamSourceEventCount;

            bool livePreparationReady = false;
            bool transactionStarted = false;
            bool resourcesCompleteWhileMeasured = false;
            bool activeSelectionExact = false;
            bool fixedCandidateReady = false;
            bool unitPolicyExact = false;
            bool automaticExact = false;
            bool sourceOwnershipExact = false;
            bool manualExact = false;
            bool coordinateProbeExact = false;
            bool evaluatorExact = false;
            bool lifecycleExact = false;
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
                activeSelectionExact = resourcesCompleteWhileMeasured &&
                    gridDescriptor.IsCreated &&
                    FoamGridSelectionMatchesActive;
                fixedCandidateReady = resourcesCompleteWhileMeasured &&
                    fixedMetricCandidateDescriptor.IsCreated &&
                    fixedMetricCandidateDescriptor.UsesFixedMetricLattice;
                sourceOwnershipExact = ValidateP7AutomaticSourceOwnershipContracts(report);

                report.AppendLine("ACTIVE SOURCE OWNERSHIP");
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
                    "P7 preserves the authored active mapping and does not change " +
                    "allocation, transport, film, shape, topology, or rendering.");
                report.AppendLine(
                    "ACTIVE SELECTION OWNERSHIP VERDICT: " +
                    (activeSelectionExact && fixedCandidateReady
                        ? "PASS"
                        : "FAIL"));
                report.AppendLine();

                if (resourcesCompleteWhileMeasured && activeSelectionExact &&
                    fixedCandidateReady)
                {
                    unitPolicyExact = ValidateP7SourceUnitPolicy(report);
                    automaticExact = ValidateP7AutomaticSources(report);
                    manualExact = ValidateP7ManualSources(report);
                    coordinateProbeExact =
                        ValidateP7CoordinatesAndProbe(report);
                    evaluatorExact = ValidateP7EvaluatorIdentity(report);
                    lifecycleExact = ValidateP7LifecycleContracts(
                        report,
                        pendingInjectionCountBefore,
                        pendingMaterialBirthCountBefore,
                        activeCompositionCountBefore,
                        activeAutomaticCountBefore);
                }
                else
                {
                    AppendP7SkippedContract(
                        report,
                        "SOURCE UNIT CLASSIFICATION",
                        "SOURCE UNIT POLICY VERDICT",
                        preparationFailure);
                    AppendP7SkippedContract(
                        report,
                        "AUTOMATIC SOURCE FAMILY CONTRACTS",
                        "AUTOMATIC SOURCE VERDICT",
                        preparationFailure);
                    AppendP7SkippedContract(
                        report,
                        "MANUAL SOURCE COMMAND CONTRACTS",
                        "MANUAL SOURCE VERDICT",
                        preparationFailure);
                    AppendP7SkippedContract(
                        report,
                        "SOURCE COORDINATE AND PROBE CONTRACTS",
                        "COORDINATE/PROBE VERDICT",
                        preparationFailure);
                    AppendP7SkippedContract(
                        report,
                        "PRODUCTION/DEBUG EVALUATOR IDENTITY",
                        "EVALUATOR IDENTITY VERDICT",
                        preparationFailure);
                    AppendP7SkippedContract(
                        report,
                        "LIFECYCLE AND CAPACITY INVARIANTS",
                        "LIFECYCLE/CAPACITY VERDICT",
                        preparationFailure);
                }
            }
            catch (Exception exception)
            {
                report.AppendLine("P7 LIVE EVIDENCE EXCEPTION");
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
                        "No P7-owned live transaction was started; no foreign " +
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
                resourcesCompleteWhileMeasured && activeSelectionExact &&
                fixedCandidateReady && unitPolicyExact && automaticExact &&
                sourceOwnershipExact && manualExact && coordinateProbeExact &&
                evaluatorExact &&
                lifecycleExact && cleanupExact && cacheExact;

            report.AppendLine("FINAL LEDGER");
            report.AppendLine(
                "Live diagnostic preparation transaction: " +
                (livePreparationReady ? "PASS" : "FAIL"));
            report.AppendLine(
                "Active descriptor matches authored selection: " +
                (activeSelectionExact && fixedCandidateReady
                    ? "PASS"
                    : "FAIL"));
            report.AppendLine(
                "Source unit classification: " +
                (unitPolicyExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "All eight automatic source families: " +
                (automaticExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Hybrid automatic-source ownership: " +
                (sourceOwnershipExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Manual ellipse/compound/segment commands: " +
                (manualExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Source coordinates and isolated probe: " +
                (coordinateProbeExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Production/debug shared evaluator: " +
                (evaluatorExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Lifecycle/event capacity invariants: " +
                (lifecycleExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Diagnostic cleanup and disabled bindings: " +
                (cleanupExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Assigned cache remained unchanged: " +
                (cacheExact ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Elapsed: {stopwatch.Elapsed.TotalMilliseconds:F3} ms");
            report.AppendLine("Overall: " + (overall ? "PASS" : "FAIL"));

            return FinalizeP7Report(
                report,
                overall,
                overall
                    ? "All P7 source-unit, parity, lifecycle, cleanup, and cache contracts passed."
                    : "One or more P7 source or diagnostic contracts failed; inspect the report ledger.");
        }

        private bool ValidateP7SourceUnitPolicy(StringBuilder report)
        {
            report.AppendLine("SOURCE UNIT CLASSIFICATION");
            float probeLength = P7FixedProbePatchLengthMetres;
            float probeWidth = P7FixedProbePatchWidthMetres;
            float probeGap = P7FixedProbeGapMetres;
            bool exact = P7SourceUnitContractVersion == 1 &&
                river.FoamShoreRibbonThicknessCells >= 0.5f &&
                river.FoamShoreRibbonOffsetVariationCells >= 0f &&
                probeLength > 0f && probeWidth > 0f && probeGap > 0f;

            report.AppendLine(
                $"Contract version: {P7SourceUnitContractVersion}");
            report.AppendLine(
                "Metres: fields explicitly named Metres/MetresPerSecond and " +
                "manual metric placement/drift commands.");
            report.AppendLine(
                "Seconds: source duration, build, hold, release, rest, and " +
                "formation timing.");
            report.AppendLine(
                "Normalized/unitless: coverage, activity, weights, lifecycle " +
                "fractions, seeds, progress, and compatibility position controls.");
            report.AppendLine(
                $"Compatibility cells: Shore Ribbon thickness=" +
                $"{river.FoamShoreRibbonThicknessCells:R}; offset variation=" +
                $"{river.FoamShoreRibbonOffsetVariationCells:R}.");
            report.AppendLine(
                $"Fixed probe physical contract: {probeLength:R}m x " +
                $"{probeWidth:R}m; gap={probeGap:R}m.");
            report.AppendLine(
                "SOURCE UNIT POLICY VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP7AutomaticSourceOwnershipContracts(
            StringBuilder report)
        {
            report.AppendLine("HYBRID AUTOMATIC-SOURCE OWNERSHIP");
            StylizedRiverFoamGridDescriptor descriptor =
                gridDescriptor.IsCreated
                    ? gridDescriptor
                    : fixedMetricCandidateDescriptor;
            AutomaticFoamSourceEvent shore = CreateP7SyntheticAutomaticSource(
                AutomaticFoamSourceEventType.ShoreRibbon,
                -1f,
                river.Domain.GlobalDistanceMinimum + 1.5f,
                river.Domain.GlobalDistanceMinimum + 3.5f,
                river.Domain.GlobalDistanceMinimum + 2.5f,
                -0.5f);
            shore.Elapsed = 0.80f;
            FoamSourceEventGpuData shoreBuild =
                BuildAutomaticFoamSourceGpuData(shore, 0.70f, descriptor);
            FoamSourceEventGpuData shoreRepeated =
                BuildAutomaticFoamSourceGpuData(shore, 0.80f, descriptor);

            AutomaticFoamSourceEvent arc = CreateP7SyntheticAutomaticSource(
                AutomaticFoamSourceEventType.ObjectContactArc,
                1f,
                river.Domain.GlobalDistanceMinimum + 4f,
                river.Domain.GlobalDistanceMinimum + 7f,
                river.Domain.GlobalDistanceMinimum + 5.5f,
                0.25f);
            arc.Elapsed = 0.65f;
            FoamSourceEventGpuData arcBuild =
                BuildAutomaticFoamSourceGpuData(arc, 0.55f, descriptor);
            arc.Elapsed = 0.75f;
            FoamSourceEventGpuData arcBuildToHold =
                BuildAutomaticFoamSourceGpuData(arc, 0.65f, descriptor);
            arc.Elapsed = 1.10f;
            FoamSourceEventGpuData arcHold =
                BuildAutomaticFoamSourceGpuData(arc, 0.95f, descriptor);
            arc.Elapsed = 1.65f;
            FoamSourceEventGpuData arcRelease =
                BuildAutomaticFoamSourceGpuData(arc, 1.50f, descriptor);

            bool shoreBuildAdvances =
                shoreBuild.Header.z > shoreBuild.Deposit.y;
            bool repeatedNonpersistentInteriorZero = P7FloatBitsEqual(
                shoreRepeated.Header.z,
                shoreRepeated.Deposit.y);
            bool arcBuildAdvances =
                arcBuild.Header.y == 0f && arcBuild.Deposit.x == 0f &&
                arcBuild.Header.z > arcBuild.Deposit.y;
            bool arcBuildToHoldTransition =
                arcBuildToHold.Header.y == 1f &&
                arcBuildToHold.Deposit.x == 0f &&
                arcBuildToHold.Header.z >= 0f &&
                arcBuildToHold.Header.z <= 1f;
            bool arcHoldActive =
                arcHold.Header.y == 1f && arcHold.Deposit.x == 1f &&
                arcHold.Header.z > arcHold.Deposit.y;
            bool arcReleaseActive =
                arcRelease.Header.y == 2f && arcRelease.Deposit.x == 2f &&
                arcRelease.Header.z > arcRelease.Deposit.y;
            bool arcEventHasRestBoundary =
                Mathf.Approximately(
                    arc.Duration,
                    arc.ObjectBuildDuration + arc.ObjectHoldDuration +
                    arc.ObjectReleaseDuration) &&
                arc.ObjectRestDuration > 0f;

            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string computePath = Path.Combine(
                projectRoot,
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.compute");
            string computeSource = File.Exists(computePath)
                ? File.ReadAllText(computePath)
                : string.Empty;
            string injectionPath = Path.Combine(
                projectRoot,
                "Assets/Game/Procedural/Rivers/" +
                "StylizedRiverFoamRuntime.Injection.cs");
            string injectionSource = File.Exists(injectionPath)
                ? File.ReadAllText(injectionPath)
                : string.Empty;
            bool nonpersistentDifferenceGate =
                computeSource.IndexOf(
                    "if (!persistentObjectEmitter)",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "max(0.0, currentContribution - previousContribution)",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "previousSourceEvent.header.y = sourceEvent.deposit.x;",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "previousSourceEvent.header.z = sourceEvent.deposit.y;",
                    StringComparison.Ordinal) >= 0;
            bool persistentObjectBypass =
                computeSource.IndexOf(
                    "bool persistentObjectEmitter =",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "else if (currentContribution <= FoamMaterialStateEpsilon)",
                    StringComparison.Ordinal) >= 0 &&
                injectionSource.IndexOf(
                    "hasNewDeposition = persistentObjectEmitter ||",
                    StringComparison.Ordinal) >= 0;
            bool phaseResolverRestored =
                injectionSource.IndexOf(
                    "phaseOrSide = 1f;",
                    StringComparison.Ordinal) >= 0 &&
                injectionSource.IndexOf(
                    "phaseOrSide = 2f;",
                    StringComparison.Ordinal) >= 0;
            bool absoluteTargetPreserved =
                computeSource.IndexOf(
                    "float birthContribution = currentContribution;",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "FoamMergeBornPresence(",
                    StringComparison.Ordinal) >= 0;
            bool warningHelperRemoved =
                computeSource.IndexOf(
                    "FoamEvaluateAutomaticSourceContribution",
                    StringComparison.Ordinal) < 0;
            bool nonpersistentDeadCellRemainsDead =
                repeatedNonpersistentInteriorZero &&
                computeSource.IndexOf(
                    "newlyRevealedContribution <= FoamMaterialStateEpsilon",
                    StringComparison.Ordinal) >= 0;

            bool exact = descriptor.IsCreated && shoreBuildAdvances &&
                repeatedNonpersistentInteriorZero && arcBuildAdvances &&
                arcBuildToHoldTransition && arcHoldActive &&
                arcReleaseActive && arcEventHasRestBoundary &&
                nonpersistentDifferenceGate && persistentObjectBypass &&
                phaseResolverRestored && absoluteTargetPreserved &&
                warningHelperRemoved && nonpersistentDeadCellRemainsDead;
            report.AppendLine(
                $"Nonpersistent Build frontier advances: {shoreBuildAdvances}; " +
                $"repeated Build interior zero: " +
                $"{repeatedNonpersistentInteriorZero}");
            report.AppendLine(
                $"Object Build advances: {arcBuildAdvances}; " +
                $"Build-to-Hold transition: {arcBuildToHoldTransition}");
            report.AppendLine(
                $"Object Hold active: {arcHoldActive}; progressive Release " +
                $"active: {arcReleaseActive}; Rest boundary retained: " +
                $"{arcEventHasRestBoundary}");
            report.AppendLine(
                $"Nonpersistent GPU difference gate: " +
                $"{nonpersistentDifferenceGate}; persistent Object bypass: " +
                $"{persistentObjectBypass}; phase resolver restored: " +
                $"{phaseResolverRestored}");
            report.AppendLine(
                $"Absolute birth target preserved: {absoluteTargetPreserved}; " +
                $"warning helper removed: {warningHelperRemoved}; " +
                $"nonpersistent dead cell remains dead: " +
                $"{nonpersistentDeadCellRemainsDead}");
            report.AppendLine(
                "HYBRID SOURCE OWNERSHIP VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP7AutomaticSources(StringBuilder report)
        {
            report.AppendLine("AUTOMATIC SOURCE FAMILY CONTRACTS");
            float domainMinimum = river.Domain.GlobalDistanceMinimum;
            float domainMaximum = river.Domain.GlobalDistanceMaximum;
            float centreGlobal = Mathf.Lerp(domainMinimum, domainMaximum, 0.52f);
            float halfLength = Mathf.Min(
                4f,
                Mathf.Max(0.5f, (domainMaximum - domainMinimum) * 0.08f));
            float startGlobal = Mathf.Clamp(
                centreGlobal - halfLength,
                domainMinimum,
                domainMaximum);
            float endGlobal = Mathf.Clamp(
                centreGlobal + halfLength,
                domainMinimum,
                domainMaximum);
            float centreLateral = Mathf.Clamp(
                0.35f,
                fixedMetricCandidateDescriptor
                    .RepresentedLateralMinimumMetres +
                    fixedMetricCandidateDescriptor.ResolvedDyMetres * 4f,
                fixedMetricCandidateDescriptor
                    .RepresentedLateralMaximumMetres -
                    fixedMetricCandidateDescriptor.ResolvedDyMetres * 4f);

            AutomaticFoamSourceEventType[] families =
            {
                AutomaticFoamSourceEventType.ShoreRibbon,
                AutomaticFoamSourceEventType.InwardWash,
                AutomaticFoamSourceEventType.ObjectContactArc,
                AutomaticFoamSourceEventType.ObjectContactFleck,
                AutomaticFoamSourceEventType.ObjectContactSemiArc,
                AutomaticFoamSourceEventType.FreeWaterLaceConnector,
                AutomaticFoamSourceEventType.FreeWaterTornFragment,
                AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector
            };

            bool allExact = true;
            for (int index = 0; index < families.Length; index++)
            {
                AutomaticFoamSourceEvent sourceEvent =
                    CreateP7SyntheticAutomaticSource(
                        families[index],
                        index % 2 == 0 ? -1f : 1f,
                        startGlobal,
                        endGlobal,
                        centreGlobal,
                        centreLateral);

                bool legacyResolved = TryResolveAutomaticSourceDispatchRange(
                    sourceEvent,
                    gridDescriptor,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    out P7SourceDispatchRange legacyRange);
                bool expectedResolved =
                    TryResolveP7LegacyAutomaticRangeExpected(
                        sourceEvent,
                        out P7SourceDispatchRange expectedLegacyRange);
                bool legacyExact = legacyResolved && expectedResolved &&
                    P7RangesEqual(legacyRange, expectedLegacyRange);

                bool fixedResolved = TryResolveAutomaticSourceDispatchRange(
                    sourceEvent,
                    fixedMetricCandidateDescriptor,
                    fixedMetricCandidateDescriptor.ColumnCount,
                    fixedMetricCandidateDescriptor.RowCount,
                    fixedMetricCandidateDescriptor.AllocatedLengthMetres,
                    out P7SourceDispatchRange fixedRange);
                bool fixedBounded = fixedResolved &&
                    P7RangeWithinDescriptor(
                        fixedRange,
                        fixedMetricCandidateDescriptor) &&
                    fixedRange.CountX > 0 && fixedRange.CountY > 0 &&
                    fixedRange.CountY < fixedMetricCandidateDescriptor.RowCount;
                bool fixedContains = fixedResolved &&
                    P7AutomaticRangeContainsPhysicalBounds(
                        sourceEvent,
                        fixedRange,
                        fixedMetricCandidateDescriptor);

                AutomaticFoamSourceEvent reversed = sourceEvent;
                reversed.StartGlobalDistance = sourceEvent.EndGlobalDistance;
                reversed.EndGlobalDistance = sourceEvent.StartGlobalDistance;
                bool reverseResolved = TryResolveAutomaticSourceDispatchRange(
                    reversed,
                    fixedMetricCandidateDescriptor,
                    fixedMetricCandidateDescriptor.ColumnCount,
                    fixedMetricCandidateDescriptor.RowCount,
                    fixedMetricCandidateDescriptor.AllocatedLengthMetres,
                    out P7SourceDispatchRange reversedRange);
                bool reversalExact = reverseResolved &&
                    P7RangesEqual(fixedRange, reversedRange);

                FoamSourceEventGpuData legacyGpu =
                    BuildAutomaticFoamSourceGpuData(
                        sourceEvent,
                        Mathf.Max(0f, sourceEvent.Elapsed - 0.10f),
                        gridDescriptor);
                FoamSourceEventGpuData fixedGpu =
                    BuildAutomaticFoamSourceGpuData(
                        sourceEvent,
                        Mathf.Max(0f, sourceEvent.Elapsed - 0.10f),
                        fixedMetricCandidateDescriptor);
                bool sourceUnitExact =
                    P7AutomaticGpuContractExact(
                        sourceEvent,
                        legacyGpu,
                        fixedGpu);

                bool familyExact = legacyExact && fixedBounded &&
                    fixedContains && reversalExact && sourceUnitExact;
                allExact &= familyExact;
                report.AppendLine(
                    $"{sourceEvent.Type}: legacy={P7RangeText(legacyRange)} " +
                    $"expected={P7RangeText(expectedLegacyRange)} " +
                    $"fixed={P7RangeText(fixedRange)}; " +
                    $"legacyExact={legacyExact}; fixedBounded={fixedBounded}; " +
                    $"physicalContainment={fixedContains}; " +
                    $"reversal={reversalExact}; gpuUnitsAndProgression=" +
                    $"{sourceUnitExact}; " +
                    $"verdict={(familyExact ? "PASS" : "FAIL")}");
            }

            report.AppendLine(
                "AUTOMATIC SOURCE VERDICT: " +
                (allExact ? "PASS" : "FAIL"));
            report.AppendLine();
            return allExact;
        }

        private AutomaticFoamSourceEvent CreateP7SyntheticAutomaticSource(
            AutomaticFoamSourceEventType type,
            float sideSign,
            float startGlobal,
            float endGlobal,
            float centreGlobal,
            float centreLateral)
        {
            float centreAcross = ResolveSourceAcrossNormalized(
                centreGlobal,
                centreLateral);
            float fixedSpacing = Mathf.Max(
                0.01f,
                fixedMetricCandidateDescriptor.ResolvedDyMetres);
            return new AutomaticFoamSourceEvent
            {
                Active = true,
                EventId = 7000 + (int)type,
                Type = type,
                SideSign = sideSign < 0f ? -1f : 1f,
                StartGlobalDistance = startGlobal,
                EndGlobalDistance = endGlobal,
                ObjectCentreGlobalDistance = centreGlobal,
                Duration = 2f,
                Elapsed = 0.65f,
                ObjectBuildDuration = 0.7f,
                ObjectHoldDuration = 0.6f,
                ObjectReleaseDuration = 0.7f,
                ObjectRestDuration = 0.5f,
                FormationSpeedMetresPerSecond = 0.55f,
                HeadTrailMetres = 0.45f,
                ShoreInsetMetres = 0.05f,
                WidthMetres = 0.20f,
                ShoreRibbonThicknessCells = 1.25f,
                ShoreRibbonThicknessMetres = 1.25f * fixedSpacing,
                InwardReachMetres = 0.65f,
                FeatherMetres = 0.08f,
                SourceAmount = 0.72f,
                RemainingLife = 0.82f,
                PatternSeed = 71.3f,
                SourceFillSeed = 91.7f,
                SourceFillFeatureSize = 0.24f,
                SourceFillBlend = 0.30f,
                ShapeSeed = 117.9f,
                BreakupScaleMetres = 0.35f,
                BreakupStrength = 0.25f,
                Curvature = 0.18f,
                ObjectCentreAcrossMetres = centreLateral,
                ObjectAlongHalfLengthMetres = 0.85f,
                ObjectAcrossHalfWidthMetres = 0.55f,
                ObjectContactOffsetMetres = 0.12f,
                ObjectSourceLateralCellSpacingMetres = fixedSpacing,
                ObjectWakeArmLengthMetres = 1.4f,
                ObjectContactPathLengthMetres = 3.2f,
                ObjectContactPoint0 = new Vector2(startGlobal, centreLateral),
                ObjectContactPoint1 = new Vector2(
                    centreGlobal - 0.45f,
                    centreLateral - 0.25f),
                ObjectContactPoint2 = new Vector2(
                    centreGlobal,
                    centreLateral - 0.45f),
                ObjectContactPoint3 = new Vector2(
                    centreGlobal + 0.45f,
                    centreLateral - 0.25f),
                ObjectContactPoint4 = new Vector2(endGlobal, centreLateral),
                ObjectContactFrontSplit = 0.5f,
                ObjectContactNegativeFirstSegmentSplit = 0.25f,
                ObjectContactPositiveFirstSegmentSplit = 0.75f,
                CentreAcrossNormalized = centreAcross,
                LateralPaddingMetres = 0.95f
            };
        }

        private bool TryResolveP7LegacyAutomaticRangeExpected(
            AutomaticFoamSourceEvent sourceEvent,
            out P7SourceDispatchRange range)
        {
            range = default;
            float startStorageGlobal =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    sourceEvent.StartGlobalDistance);
            float endStorageGlobal =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    sourceEvent.EndGlobalDistance);
            float padding = Mathf.Max(
                sourceEvent.FeatherMetres * 2f,
                Mathf.Max(
                    sourceEvent.WidthMetres,
                    sourceEvent.InwardReachMetres) * 1.25f);
            bool arc = sourceEvent.Type ==
                    AutomaticFoamSourceEventType.ObjectContactArc ||
                sourceEvent.Type ==
                    AutomaticFoamSourceEventType.ObjectContactSemiArc;
            if (arc)
            {
                float longitudinalCellSpacing = fieldWidth > 0
                    ? Mathf.Max(0.01f, fieldLength / fieldWidth)
                    : 0.01f;
                padding = longitudinalCellSpacing * 2f;
            }
            else if (sourceEvent.Type ==
                AutomaticFoamSourceEventType.ObjectContactFleck)
            {
                padding = Mathf.Max(
                    padding,
                    Mathf.Max(
                        sourceEvent.ObjectAlongHalfLengthMetres,
                        sourceEvent.ObjectAcrossHalfWidthMetres) +
                    sourceEvent.ObjectContactOffsetMetres +
                    sourceEvent.WidthMetres * 4f +
                    sourceEvent.FeatherMetres * 2f);
            }

            int startX = Mathf.Clamp(
                GlobalDistanceToX(
                    Mathf.Min(startStorageGlobal, endStorageGlobal) -
                    padding) - 2,
                0,
                fieldWidth - 1);
            int endX = Mathf.Clamp(
                GlobalDistanceToX(
                    Mathf.Max(startStorageGlobal, endStorageGlobal) +
                    padding) + 2,
                0,
                fieldWidth - 1);
            int startY = 0;
            int countY = fieldHeight;
            if (arc)
            {
                int centreY = StylizedRiverFoamTopologyFieldSpace
                    .SignedAcrossNormalizedToNearestTexel(
                        sourceEvent.CentreAcrossNormalized,
                        fieldHeight);
                float sourceSpacing = Mathf.Max(
                    0.01f,
                    sourceEvent.ObjectSourceLateralCellSpacingMetres);
                float lateralExtent = Mathf.Max(
                    sourceSpacing,
                    sourceEvent.LateralPaddingMetres);
                int padY = Mathf.CeilToInt(
                    lateralExtent / sourceSpacing) + 2;
                startY = Mathf.Clamp(
                    centreY - padY,
                    0,
                    fieldHeight - 1);
                int endY = Mathf.Clamp(
                    centreY + padY,
                    0,
                    fieldHeight - 1);
                countY = Mathf.Max(1, endY - startY + 1);
            }
            else if (sourceEvent.Type ==
                    AutomaticFoamSourceEventType.FreeWaterLaceConnector ||
                sourceEvent.Type ==
                    AutomaticFoamSourceEventType.FreeWaterTornFragment ||
                sourceEvent.Type ==
                    AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector)
            {
                float centreAcross01 = Mathf.Clamp01(
                    sourceEvent.CentreAcrossNormalized * 0.5f + 0.5f);
                int centreY = Mathf.Clamp(
                    Mathf.RoundToInt(
                        centreAcross01 * Mathf.Max(0, fieldHeight - 1)),
                    0,
                    fieldHeight - 1);
                float centreWorldDistance =
                    (sourceEvent.StartGlobalDistance +
                     sourceEvent.EndGlobalDistance) * 0.5f;
                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(centreWorldDistance);
                float visibleHalfWidth = sample.GetVisibleHalfWidth(
                    sourceEvent.CentreAcrossNormalized < 0f ? -1f : 1f);
                float normalizedPad = Mathf.Clamp01(
                    sourceEvent.LateralPaddingMetres /
                    Mathf.Max(0.10f, visibleHalfWidth));
                int padY = Mathf.CeilToInt(
                    normalizedPad * 0.5f * fieldHeight) + 3;
                startY = Mathf.Clamp(
                    centreY - padY,
                    0,
                    fieldHeight - 1);
                int endY = Mathf.Clamp(
                    centreY + padY,
                    0,
                    fieldHeight - 1);
                countY = Mathf.Max(1, endY - startY + 1);
            }

            range = new P7SourceDispatchRange(
                startX,
                Mathf.Max(0, endX - startX + 1),
                startY,
                countY);
            return range.IsValid;
        }

        private bool P7AutomaticRangeContainsPhysicalBounds(
            AutomaticFoamSourceEvent sourceEvent,
            P7SourceDispatchRange range,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            ResolveP7AutomaticLongitudinalBounds(
                sourceEvent,
                descriptor,
                out float minimumGlobal,
                out float maximumGlobal);
            minimumGlobal = Mathf.Clamp(
                minimumGlobal,
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum);
            maximumGlobal = Mathf.Clamp(
                maximumGlobal,
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum);

            if (!P7AutomaticRangeContainsLateralBoundsAtGlobal(
                    sourceEvent,
                    range,
                    descriptor,
                    minimumGlobal) ||
                !P7AutomaticRangeContainsLateralBoundsAtGlobal(
                    sourceEvent,
                    range,
                    descriptor,
                    maximumGlobal))
            {
                return false;
            }

            for (int localX = range.StartX;
                 localX <= range.EndX;
                 localX++)
            {
                float globalDistance = Mathf.Clamp(
                    river.Domain.GlobalDistanceMinimum +
                    descriptor.ResolveLocalDistanceAtColumnCentre(localX),
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum);
                if (!P7AutomaticRangeContainsLateralBoundsAtGlobal(
                        sourceEvent,
                        range,
                        descriptor,
                        globalDistance))
                {
                    return false;
                }
            }

            return true;
        }

        private static void ResolveP7AutomaticLongitudinalBounds(
            AutomaticFoamSourceEvent sourceEvent,
            StylizedRiverFoamGridDescriptor descriptor,
            out float minimumGlobal,
            out float maximumGlobal)
        {
            float padding = Mathf.Max(
                sourceEvent.FeatherMetres * 2f,
                Mathf.Max(
                    sourceEvent.WidthMetres,
                    sourceEvent.InwardReachMetres) * 1.25f);
            if (sourceEvent.Type ==
                    AutomaticFoamSourceEventType.ObjectContactArc ||
                sourceEvent.Type ==
                    AutomaticFoamSourceEventType.ObjectContactSemiArc)
            {
                padding = Mathf.Max(
                    0.01f,
                    descriptor.ResolvedDxMetres) * 2f;
            }
            else if (sourceEvent.Type ==
                AutomaticFoamSourceEventType.ObjectContactFleck)
            {
                padding = Mathf.Max(
                    padding,
                    Mathf.Max(
                        sourceEvent.ObjectAlongHalfLengthMetres,
                        sourceEvent.ObjectAcrossHalfWidthMetres) +
                    sourceEvent.ObjectContactOffsetMetres +
                    sourceEvent.WidthMetres * 4f +
                    sourceEvent.FeatherMetres * 2f);
            }

            minimumGlobal = Mathf.Min(
                sourceEvent.StartGlobalDistance,
                sourceEvent.EndGlobalDistance) - padding;
            maximumGlobal = Mathf.Max(
                sourceEvent.StartGlobalDistance,
                sourceEvent.EndGlobalDistance) + padding;
        }

        private bool P7AutomaticRangeContainsLateralBoundsAtGlobal(
            AutomaticFoamSourceEvent sourceEvent,
            P7SourceDispatchRange range,
            StylizedRiverFoamGridDescriptor descriptor,
            float globalDistance)
        {
            ResolveP7AutomaticLateralBoundsAtGlobal(
                sourceEvent,
                descriptor,
                globalDistance,
                out float minimumLateral,
                out float maximumLateral);
            return P7RangeContainsMetricPoint(
                    range,
                    descriptor,
                    globalDistance,
                    minimumLateral) &&
                P7RangeContainsMetricPoint(
                    range,
                    descriptor,
                    globalDistance,
                    maximumLateral);
        }

        private void ResolveP7AutomaticLateralBoundsAtGlobal(
            AutomaticFoamSourceEvent sourceEvent,
            StylizedRiverFoamGridDescriptor descriptor,
            float globalDistance,
            out float minimumLateral,
            out float maximumLateral)
        {
            if (sourceEvent.Type == AutomaticFoamSourceEventType.ShoreRibbon ||
                sourceEvent.Type == AutomaticFoamSourceEventType.InwardWash)
            {
                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float shore = sourceEvent.SideSign < 0f
                    ? -sample.LeftHalfWidth
                    : sample.RightHalfWidth;
                float inwardExtent = Mathf.Max(
                    sourceEvent.ShoreInsetMetres +
                        sourceEvent.InwardReachMetres,
                    sourceEvent.ShoreInsetMetres +
                        Mathf.Max(
                            sourceEvent.WidthMetres,
                            sourceEvent.ShoreRibbonThicknessMetres) * 0.5f) +
                    sourceEvent.FeatherMetres * 2f +
                    descriptor.ResolvedDyMetres;
                float outside = sourceEvent.FeatherMetres +
                    descriptor.ResolvedDyMetres;
                minimumLateral = sourceEvent.SideSign < 0f
                    ? shore - outside
                    : shore - inwardExtent;
                maximumLateral = sourceEvent.SideSign < 0f
                    ? shore + inwardExtent
                    : shore + outside;
                return;
            }

            float extent;
            switch (sourceEvent.Type)
            {
                case AutomaticFoamSourceEventType.ObjectContactArc:
                case AutomaticFoamSourceEventType.ObjectContactSemiArc:
                    extent = Mathf.Max(
                        Mathf.Max(
                            descriptor.ResolvedDyMetres,
                            sourceEvent.ObjectSourceLateralCellSpacingMetres),
                        sourceEvent.LateralPaddingMetres) +
                        descriptor.ResolvedDyMetres * 2f;
                    break;
                case AutomaticFoamSourceEventType.ObjectContactFleck:
                    extent = Mathf.Max(
                        sourceEvent.LateralPaddingMetres,
                        sourceEvent.ObjectAcrossHalfWidthMetres +
                        sourceEvent.ObjectContactOffsetMetres +
                        sourceEvent.WidthMetres * 4f +
                        sourceEvent.FeatherMetres * 2f) +
                        descriptor.ResolvedDyMetres * 2f;
                    break;
                default:
                    extent = Mathf.Max(
                        sourceEvent.LateralPaddingMetres,
                        sourceEvent.WidthMetres +
                        sourceEvent.FeatherMetres * 2f) +
                        descriptor.ResolvedDyMetres * 2f;
                    break;
            }

            minimumLateral = sourceEvent.ObjectCentreAcrossMetres - extent;
            maximumLateral = sourceEvent.ObjectCentreAcrossMetres + extent;
        }

        private bool ValidateP7ManualSources(StringBuilder report)
        {
            report.AppendLine("MANUAL SOURCE COMMAND CONTRACTS");
            float domainMinimum = river.Domain.GlobalDistanceMinimum;
            float domainMaximum = river.Domain.GlobalDistanceMaximum;
            float centreGlobal = Mathf.Lerp(domainMinimum, domainMaximum, 0.48f);
            float across = 0.22f;
            float lateral = ResolveSourceLateralMetres(centreGlobal, across);

            PendingInjection normalizedEllipse = new(
                centreGlobal,
                across,
                0.42f,
                0.8f,
                0.75f,
                11f,
                1.7f,
                true,
                17f,
                0.22f);
            PendingInjection metricEllipse = new(
                centreGlobal,
                across,
                0.42f,
                0.8f,
                0.75f,
                11f,
                1.7f,
                true,
                17f,
                0.22f,
                usesMetricLateral: true,
                lateralMetres: lateral);
            PendingInjection normalizedCompound = new(
                centreGlobal,
                across,
                0.42f,
                0.8f,
                0.75f,
                11f,
                1.7f,
                true,
                17f,
                0.22f,
                compoundShape: true);
            PendingInjection metricCompound = new(
                centreGlobal,
                across,
                0.42f,
                0.8f,
                0.75f,
                11f,
                1.7f,
                true,
                17f,
                0.22f,
                compoundShape: true,
                usesMetricLateral: true,
                lateralMetres: lateral);

            float segmentStartGlobal = centreGlobal - 1.35f;
            float segmentEndGlobal = centreGlobal + 1.55f;
            float segmentStartLateral = lateral - 0.45f;
            float segmentEndLateral = lateral + 0.60f;
            float segmentStartAcross = ResolveSourceAcrossNormalized(
                segmentStartGlobal,
                segmentStartLateral);
            float segmentEndAcross = ResolveSourceAcrossNormalized(
                segmentEndGlobal,
                segmentEndLateral);
            PendingInjection normalizedSegment = new(
                centreGlobal,
                across,
                0.35f,
                0.8f,
                0.75f,
                13f,
                1f,
                false,
                19f,
                0.20f,
                segmentShape: true,
                segmentStartGlobalDistance: segmentStartGlobal,
                segmentStartAcrossNormalized: segmentStartAcross,
                segmentStartRadius: 0.28f,
                segmentStartAmount: 0.65f,
                segmentEndGlobalDistance: segmentEndGlobal,
                segmentEndAcrossNormalized: segmentEndAcross,
                segmentEndRadius: 0.36f,
                segmentEndAmount: 0.82f);
            PendingInjection metricSegment = new(
                centreGlobal,
                across,
                0.35f,
                0.8f,
                0.75f,
                13f,
                1f,
                false,
                19f,
                0.20f,
                segmentShape: true,
                segmentStartGlobalDistance: segmentStartGlobal,
                segmentStartAcrossNormalized: segmentStartAcross,
                segmentStartRadius: 0.28f,
                segmentStartAmount: 0.65f,
                segmentEndGlobalDistance: segmentEndGlobal,
                segmentEndAcrossNormalized: segmentEndAcross,
                segmentEndRadius: 0.36f,
                segmentEndAmount: 0.82f,
                usesMetricLateral: true,
                lateralMetres: lateral,
                segmentStartLateralMetres: segmentStartLateral,
                segmentEndLateralMetres: segmentEndLateral);

            PendingInjection[] commands =
            {
                normalizedEllipse,
                metricEllipse,
                normalizedCompound,
                metricCompound,
                normalizedSegment,
                metricSegment
            };
            string[] names =
            {
                "Compatibility Ellipse",
                "Metric Ellipse",
                "Compatibility Compound",
                "Metric Compound",
                "Compatibility Segment",
                "Metric Segment"
            };

            bool allExact = true;
            P7SourceDispatchRange normalizedFixedEllipseRange = default;
            P7SourceDispatchRange metricFixedEllipseRange = default;
            P7SourceDispatchRange normalizedFixedCompoundRange = default;
            P7SourceDispatchRange metricFixedCompoundRange = default;
            P7SourceDispatchRange normalizedFixedSegmentRange = default;
            P7SourceDispatchRange metricFixedSegmentRange = default;
            for (int index = 0; index < commands.Length; index++)
            {
                PendingInjection command = commands[index];
                bool legacyResolved = TryResolveManualInjectionDispatchRange(
                    command,
                    gridDescriptor,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    out P7SourceDispatchRange legacyRange);
                bool expectedResolved =
                    TryResolveP7LegacyManualRangeExpected(
                        command,
                        out P7SourceDispatchRange expectedLegacyRange);
                bool legacyExact = legacyResolved && expectedResolved &&
                    P7RangesEqual(legacyRange, expectedLegacyRange);
                bool fixedResolved = TryResolveManualInjectionDispatchRange(
                    command,
                    fixedMetricCandidateDescriptor,
                    fixedMetricCandidateDescriptor.ColumnCount,
                    fixedMetricCandidateDescriptor.RowCount,
                    fixedMetricCandidateDescriptor.AllocatedLengthMetres,
                    out P7SourceDispatchRange fixedRange);
                bool fixedBounded = fixedResolved &&
                    P7RangeWithinDescriptor(
                        fixedRange,
                        fixedMetricCandidateDescriptor) &&
                    fixedRange.CountY < fixedMetricCandidateDescriptor.RowCount;
                bool fixedContains = fixedResolved &&
                    P7ManualRangeContainsPhysicalBounds(
                        command,
                        fixedRange,
                        fixedMetricCandidateDescriptor);
                bool commandExact = legacyExact && fixedBounded &&
                    fixedContains;
                allExact &= commandExact;

                if (index == 0)
                {
                    normalizedFixedEllipseRange = fixedRange;
                }
                else if (index == 1)
                {
                    metricFixedEllipseRange = fixedRange;
                }
                else if (index == 2)
                {
                    normalizedFixedCompoundRange = fixedRange;
                }
                else if (index == 3)
                {
                    metricFixedCompoundRange = fixedRange;
                }
                else if (index == 4)
                {
                    normalizedFixedSegmentRange = fixedRange;
                }
                else if (index == 5)
                {
                    metricFixedSegmentRange = fixedRange;
                }

                report.AppendLine(
                    $"{names[index]}: legacy={P7RangeText(legacyRange)} " +
                    $"expected={P7RangeText(expectedLegacyRange)} " +
                    $"fixed={P7RangeText(fixedRange)}; " +
                    $"legacyExact={legacyExact}; fixedBounded={fixedBounded}; " +
                    $"physicalContainment={fixedContains}; " +
                    $"verdict={(commandExact ? "PASS" : "FAIL")}");
            }

            bool ellipseAnchorExact = P7FloatBitsEqual(
                ResolveSourceLateralMetres(
                    normalizedEllipse.GlobalDistance,
                    normalizedEllipse.AcrossNormalized),
                metricEllipse.LateralMetres);
            bool compoundAnchorExact = P7FloatBitsEqual(
                ResolveSourceLateralMetres(
                    normalizedCompound.GlobalDistance,
                    normalizedCompound.AcrossNormalized),
                metricCompound.LateralMetres);
            bool segmentAnchorExact = P7FloatBitsEqual(
                    ResolveSourceLateralMetres(
                        normalizedSegment.SegmentStartGlobalDistance,
                        normalizedSegment.SegmentStartAcrossNormalized),
                    metricSegment.SegmentStartLateralMetres) &&
                P7FloatBitsEqual(
                    ResolveSourceLateralMetres(
                        normalizedSegment.SegmentEndGlobalDistance,
                        normalizedSegment.SegmentEndAcrossNormalized),
                    metricSegment.SegmentEndLateralMetres);
            bool unitModesDistinct =
                !normalizedEllipse.UsesMetricLateral &&
                metricEllipse.UsesMetricLateral &&
                !normalizedCompound.UsesMetricLateral &&
                metricCompound.UsesMetricLateral &&
                !normalizedSegment.UsesMetricLateral &&
                metricSegment.UsesMetricLateral;
            allExact &= ellipseAnchorExact && compoundAnchorExact &&
                segmentAnchorExact && unitModesDistinct;
            report.AppendLine(
                $"Compatibility/metric authored-anchor equivalence: ellipse=" +
                $"{ellipseAnchorExact}; compound={compoundAnchorExact}; " +
                $"segment={segmentAnchorExact}; explicitModes=" +
                $"{unitModesDistinct}.");
            report.AppendLine(
                $"Fixed rectangles are independently bounded and may differ " +
                $"on width-varying rows: ellipse compatibility=" +
                $"{P7RangeText(normalizedFixedEllipseRange)}, metric=" +
                $"{P7RangeText(metricFixedEllipseRange)}; compound compatibility=" +
                $"{P7RangeText(normalizedFixedCompoundRange)}, metric=" +
                $"{P7RangeText(metricFixedCompoundRange)}; segment compatibility=" +
                $"{P7RangeText(normalizedFixedSegmentRange)}, metric=" +
                $"{P7RangeText(metricFixedSegmentRange)}.");
            report.AppendLine(
                "MANUAL SOURCE VERDICT: " +
                (allExact ? "PASS" : "FAIL"));
            report.AppendLine();
            return allExact;
        }

        private bool TryResolveP7LegacyManualRangeExpected(
            PendingInjection injection,
            out P7SourceDispatchRange range)
        {
            range = default;
            float storageGlobalDistance =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.GlobalDistance);
            float storageSegmentStart =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.SegmentStartGlobalDistance);
            float storageSegmentEnd =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.SegmentEndGlobalDistance);
            float minimumGlobal;
            float maximumGlobal;
            if (injection.SegmentShape)
            {
                float padding = Mathf.Max(
                    injection.SegmentStartRadius,
                    injection.SegmentEndRadius);
                minimumGlobal = Mathf.Min(
                    storageSegmentStart,
                    storageSegmentEnd) - padding;
                maximumGlobal = Mathf.Max(
                    storageSegmentStart,
                    storageSegmentEnd) + padding;
            }
            else
            {
                float alongRadius = injection.Radius * injection.Elongation *
                    (injection.CompoundShape ? 1.25f : 1f);
                minimumGlobal = storageGlobalDistance - alongRadius;
                maximumGlobal = storageGlobalDistance + alongRadius;
            }

            int startX = Mathf.Clamp(
                GlobalDistanceToX(minimumGlobal) - 2,
                0,
                fieldWidth - 1);
            int endX = Mathf.Clamp(
                GlobalDistanceToX(maximumGlobal) + 2,
                0,
                fieldWidth - 1);
            range = new P7SourceDispatchRange(
                startX,
                Mathf.Max(0, endX - startX + 1),
                0,
                fieldHeight);
            return range.IsValid;
        }

        private bool P7ManualRangeContainsPhysicalBounds(
            PendingInjection injection,
            P7SourceDispatchRange range,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            float minimumGlobal;
            float maximumGlobal;
            if (injection.SegmentShape)
            {
                float longitudinalRadius = Mathf.Max(
                    injection.SegmentStartRadius,
                    injection.SegmentEndRadius);
                minimumGlobal = Mathf.Min(
                    injection.SegmentStartGlobalDistance,
                    injection.SegmentEndGlobalDistance) -
                    longitudinalRadius;
                maximumGlobal = Mathf.Max(
                    injection.SegmentStartGlobalDistance,
                    injection.SegmentEndGlobalDistance) +
                    longitudinalRadius;
            }
            else
            {
                float longitudinalRadius = injection.Radius *
                    injection.Elongation *
                    (injection.CompoundShape ? 1.25f : 1f);
                minimumGlobal = injection.GlobalDistance -
                    longitudinalRadius;
                maximumGlobal = injection.GlobalDistance +
                    longitudinalRadius;
            }

            float clampedMinimumGlobal = Mathf.Clamp(
                minimumGlobal,
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum);
            float clampedMaximumGlobal = Mathf.Clamp(
                maximumGlobal,
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum);
            if (!P7ManualRangeContainsLateralBoundsAtGlobal(
                    injection,
                    range,
                    descriptor,
                    clampedMinimumGlobal) ||
                !P7ManualRangeContainsLateralBoundsAtGlobal(
                    injection,
                    range,
                    descriptor,
                    clampedMaximumGlobal))
            {
                return false;
            }

            for (int localX = range.StartX;
                 localX <= range.EndX;
                 localX++)
            {
                float globalDistance = Mathf.Clamp(
                    river.Domain.GlobalDistanceMinimum +
                    descriptor.ResolveLocalDistanceAtColumnCentre(localX),
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum);
                if (!P7ManualRangeContainsLateralBoundsAtGlobal(
                        injection,
                        range,
                        descriptor,
                        globalDistance))
                {
                    return false;
                }
            }

            return true;
        }

        private bool P7ManualRangeContainsLateralBoundsAtGlobal(
            PendingInjection injection,
            P7SourceDispatchRange range,
            StylizedRiverFoamGridDescriptor descriptor,
            float globalDistance)
        {
            float minimumLateral;
            float maximumLateral;
            if (injection.SegmentShape)
            {
                float startLateral = injection.UsesMetricLateral
                    ? injection.SegmentStartLateralMetres
                    : ResolveSourceLateralMetres(
                        globalDistance,
                        injection.SegmentStartAcrossNormalized);
                float endLateral = injection.UsesMetricLateral
                    ? injection.SegmentEndLateralMetres
                    : ResolveSourceLateralMetres(
                        globalDistance,
                        injection.SegmentEndAcrossNormalized);
                float radius = Mathf.Max(
                    injection.SegmentStartRadius,
                    injection.SegmentEndRadius);
                minimumLateral = Mathf.Min(startLateral, endLateral) -
                    radius;
                maximumLateral = Mathf.Max(startLateral, endLateral) +
                    radius;
            }
            else
            {
                float centreLateral = injection.UsesMetricLateral
                    ? injection.LateralMetres
                    : ResolveSourceLateralMetres(
                        globalDistance,
                        injection.AcrossNormalized);
                float lateralRadius = injection.Radius *
                    (injection.CompoundShape ? 1.25f : 1f);
                minimumLateral = centreLateral - lateralRadius;
                maximumLateral = centreLateral + lateralRadius;
            }

            return P7RangeContainsMetricPoint(
                    range,
                    descriptor,
                    globalDistance,
                    minimumLateral) &&
                P7RangeContainsMetricPoint(
                    range,
                    descriptor,
                    globalDistance,
                    maximumLateral);
        }

        private bool ValidateP7CoordinatesAndProbe(StringBuilder report)
        {
            report.AppendLine("SOURCE COORDINATE AND PROBE CONTRACTS");
            bool fixedRoundTrip = true;
            Vector2Int[] cells =
            {
                new Vector2Int(0, 0),
                new Vector2Int(
                    fixedMetricCandidateDescriptor.ColumnCount / 2,
                    fixedMetricCandidateDescriptor.RowCount / 2),
                new Vector2Int(
                    fixedMetricCandidateDescriptor.ColumnCount - 1,
                    fixedMetricCandidateDescriptor.RowCount - 1)
            };
            for (int index = 0; index < cells.Length; index++)
            {
                Vector2 metricPoint = fixedMetricCandidateDescriptor
                    .ResolveMetricPositionAtCellCentre(
                        cells[index].x,
                        cells[index].y);
                bool mapped = fixedMetricCandidateDescriptor
                    .TryMetricToContainingCell(metricPoint, out Vector2Int roundTrip);
                bool exact = mapped && roundTrip == cells[index];
                fixedRoundTrip &= exact;
                report.AppendLine(
                    $"Fixed cell {cells[index]} -> metric=" +
                    $"({metricPoint.x:R},{metricPoint.y:R})m -> " +
                    $"{roundTrip}; exact={exact}");
            }

            bool normalizedRoundTrip = true;
            float[] normalizedValues = { -0.75f, 0f, 0.65f };
            float centreGlobal = Mathf.Lerp(
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum,
                0.5f);
            for (int index = 0; index < normalizedValues.Length; index++)
            {
                float lateral = ResolveSourceLateralMetres(
                    centreGlobal,
                    normalizedValues[index]);
                float roundTrip = ResolveSourceAcrossNormalized(
                    centreGlobal,
                    lateral);
                bool exact = Mathf.Abs(
                    roundTrip - normalizedValues[index]) <= 0.00001f;
                normalizedRoundTrip &= exact;
                report.AppendLine(
                    $"Legacy normalized {normalizedValues[index]:R} -> " +
                    $"{lateral:R}m -> {roundTrip:R}; exact={exact}");
            }

            ResolveP7LifeProbeLayout(
                gridDescriptor,
                fieldWidth,
                fieldHeight,
                0.47f,
                -0.18f,
                out int legacyCentreX,
                out int legacyCentreY,
                out int legacyWidth,
                out int legacyHeight,
                out int legacyGap);
            ResolveP7LegacyProbeExpected(
                0.47f,
                -0.18f,
                out int expectedCentreX,
                out int expectedCentreY,
                out int expectedWidth,
                out int expectedHeight,
                out int expectedGap);
            bool legacyProbeExact = legacyCentreX == expectedCentreX &&
                legacyCentreY == expectedCentreY &&
                legacyWidth == expectedWidth &&
                legacyHeight == expectedHeight &&
                legacyGap == expectedGap;

            ResolveP7LifeProbeLayout(
                fixedMetricCandidateDescriptor,
                fixedMetricCandidateDescriptor.ColumnCount,
                fixedMetricCandidateDescriptor.RowCount,
                0.47f,
                -0.18f,
                out int fixedCentreX,
                out int fixedCentreY,
                out int fixedWidth,
                out int fixedHeight,
                out int fixedGap);
            float resolvedLength = fixedWidth *
                fixedMetricCandidateDescriptor.ResolvedDxMetres;
            float resolvedWidth = fixedHeight *
                fixedMetricCandidateDescriptor.ResolvedDyMetres;
            float resolvedGap = fixedGap *
                fixedMetricCandidateDescriptor.ResolvedDxMetres;
            bool fixedProbeExact = fixedWidth >= 3 && fixedHeight >= 3 &&
                fixedGap >= 2 &&
                resolvedLength >= P7FixedProbePatchLengthMetres &&
                resolvedLength < P7FixedProbePatchLengthMetres +
                    fixedMetricCandidateDescriptor.ResolvedDxMetres + 0.0001f &&
                resolvedWidth >= P7FixedProbePatchWidthMetres &&
                resolvedWidth < P7FixedProbePatchWidthMetres +
                    fixedMetricCandidateDescriptor.ResolvedDyMetres + 0.0001f &&
                resolvedGap >= P7FixedProbeGapMetres &&
                resolvedGap < P7FixedProbeGapMetres +
                    fixedMetricCandidateDescriptor.ResolvedDxMetres + 0.0001f;

            report.AppendLine(
                $"Legacy probe: actual=centre({legacyCentreX},{legacyCentreY}) " +
                $"patch({legacyWidth},{legacyHeight}) gap={legacyGap}; " +
                $"expected=centre({expectedCentreX},{expectedCentreY}) " +
                $"patch({expectedWidth},{expectedHeight}) gap={expectedGap}; " +
                $"exact={legacyProbeExact}");
            report.AppendLine(
                $"Fixed probe: centre({fixedCentreX},{fixedCentreY}) " +
                $"patch({fixedWidth},{fixedHeight})=" +
                $"({resolvedLength:R},{resolvedWidth:R})m gap=" +
                $"{fixedGap}={resolvedGap:R}m; exact={fixedProbeExact}");

            bool exactAll = fixedRoundTrip && normalizedRoundTrip &&
                legacyProbeExact && fixedProbeExact;
            report.AppendLine(
                "COORDINATE/PROBE VERDICT: " +
                (exactAll ? "PASS" : "FAIL"));
            report.AppendLine();
            return exactAll;
        }

        private void ResolveP7LegacyProbeExpected(
            float distanceNormalized,
            float acrossNormalized,
            out int centreX,
            out int centreY,
            out int patchWidth,
            out int patchHeight,
            out int gap)
        {
            patchWidth = Mathf.Clamp(
                Mathf.RoundToInt(fieldWidth * 0.035f),
                3,
                10);
            patchHeight = Mathf.Clamp(
                Mathf.RoundToInt(fieldHeight * 0.075f),
                3,
                10);
            gap = Mathf.Clamp(
                Mathf.RoundToInt(fieldWidth * 0.018f),
                2,
                8);
            int groupHalfWidth = patchWidth + gap + patchWidth / 2 + 2;
            centreX = Mathf.RoundToInt(
                Mathf.Clamp01(distanceNormalized) *
                Mathf.Max(0, fieldWidth - 1));
            centreX = Mathf.Clamp(
                centreX,
                groupHalfWidth,
                Mathf.Max(groupHalfWidth, fieldWidth - 1 - groupHalfWidth));
            centreY = Mathf.RoundToInt(
                Mathf.Clamp01(acrossNormalized * 0.5f + 0.5f) *
                Mathf.Max(0, fieldHeight - 1));
            int halfHeight = Mathf.Max(1, patchHeight / 2);
            centreY = Mathf.Clamp(
                centreY,
                halfHeight + 1,
                Mathf.Max(
                    halfHeight + 1,
                    fieldHeight - halfHeight - 2));
        }

        private bool ValidateP7EvaluatorIdentity(StringBuilder report)
        {
            report.AppendLine("PRODUCTION/DEBUG EVALUATOR IDENTITY");
            string computeAssetPath = computeShader != null
                ? AssetDatabase.GetAssetPath(computeShader)
                : string.Empty;
            string computePath = P7ResolveProjectAbsolutePath(
                computeAssetPath);
            bool computeReadable = !string.IsNullOrEmpty(computePath) &&
                File.Exists(computePath);
            string computeSource = computeReadable
                ? File.ReadAllText(computePath)
                : string.Empty;
            string clearRangeBody = P7ExtractFunctionBody(
                computeSource,
                "void ClearRange");
            string injectBody = P7ExtractFunctionBody(
                computeSource,
                "void InjectFoam");
            string shapeBody = P7ExtractFunctionBody(
                computeSource,
                "void EvaluateFoamShape");
            string productionBody = P7ExtractFunctionBody(
                computeSource,
                "void RasterizeFoamSourceEvent");
            string debugBody = P7ExtractFunctionBody(
                computeSource,
                "void RasterizeFoamSourceEventDebug");

            bool twoSharedCalls = P7CountOccurrences(
                    computeSource,
                    "EvaluateFoamAutomaticSourceRasterSample(coordinate)") == 2;
            bool productionShared = P7ContainsOrdinal(
                productionBody,
                "EvaluateFoamAutomaticSourceRasterSample(coordinate)");
            bool debugShared = P7ContainsOrdinal(
                debugBody,
                "EvaluateFoamAutomaticSourceRasterSample(coordinate)");
            bool injectionYRange = P7ContainsOrdinal(
                    injectBody,
                    "_FoamRangeStartY") &&
                P7ContainsOrdinal(
                    injectBody,
                    "_FoamRangeCountY") &&
                P7ContainsOrdinal(
                    injectBody,
                    "FoamGridLocalDistanceAtTexel") &&
                P7ContainsOrdinal(
                    injectBody,
                    "FoamLateralMetresAtTexel");
            bool shapeRangeClean = !P7ContainsOrdinal(
                    shapeBody,
                    "_FoamRangeStartY") &&
                !P7ContainsOrdinal(
                    shapeBody,
                    "_FoamRangeCountY");
            bool clearRangeOwnership = P7ContainsOrdinal(
                    clearRangeBody,
                    "_FoamRangeStart") &&
                P7ContainsOrdinal(
                    clearRangeBody,
                    "_FoamRangeCount") &&
                P7ContainsOrdinal(
                    clearRangeBody,
                    "_FoamDimensions.y") &&
                !P7ContainsOrdinal(
                    clearRangeBody,
                    "_FoamRangeStartY") &&
                !P7ContainsOrdinal(
                    clearRangeBody,
                    "_FoamRangeCountY");

            string resourcesPath = Path.Combine(
                Path.GetDirectoryName(computePath) ?? string.Empty,
                "CS_RiverFoam.Resources.hlsl");
            bool resourcesReadable = File.Exists(resourcesPath);
            string resourcesSource = resourcesReadable
                ? File.ReadAllText(resourcesPath)
                : string.Empty;
            string injectionPath = P7ResolveProjectAbsolutePath(
                "Assets/Game/Procedural/Rivers/" +
                "StylizedRiverFoamRuntime.Injection.cs");
            bool injectionReadable = File.Exists(injectionPath);
            string injectionSource = injectionReadable
                ? File.ReadAllText(injectionPath)
                : string.Empty;
            string[] requiredUniforms =
            {
                "_FoamInjectionUsesMetricLateral",
                "_FoamInjectionLateralMetres",
                "_FoamInjectionSegmentStartLateralMetres",
                "_FoamInjectionSegmentEndLateralMetres"
            };
            bool uniformsDeclared = resourcesReadable;
            bool uniformsBound = injectionReadable;
            for (int index = 0; index < requiredUniforms.Length; index++)
            {
                uniformsDeclared &= P7ContainsOrdinal(
                    resourcesSource,
                    requiredUniforms[index]);
                uniformsBound &= P7ContainsOrdinal(
                    injectionSource,
                    "SetFloat(") &&
                    P7ContainsOrdinal(
                        injectionSource,
                        requiredUniforms[index]);
            }
            bool sourceRangeBound = injectionReadable &&
                P7ContainsOrdinal(
                    injectionSource,
                    "SetInt(\"_FoamRangeStartY\"") &&
                P7ContainsOrdinal(
                    injectionSource,
                    "SetInt(\"_FoamRangeCountY\"") &&
                P7ContainsOrdinal(
                    injectionSource,
                    "ConfigureGridDescriptorComputeParameters();");

            bool exact = computeReadable && twoSharedCalls &&
                productionShared && debugShared && injectionYRange &&
                shapeRangeClean && clearRangeOwnership &&
                uniformsDeclared && uniformsBound && sourceRangeBound;
            report.AppendLine($"Compute asset path: {computeAssetPath}");
            report.AppendLine($"Compute absolute path: {computePath}");
            report.AppendLine(
                $"Shared evaluator calls: expected=2; actual=" +
                $"{P7CountOccurrences(computeSource, "EvaluateFoamAutomaticSourceRasterSample(coordinate)")}; " +
                $"production={productionShared}; debug={debugShared}");
            report.AppendLine(
                $"InjectFoam descriptor/Y-range contract: {injectionYRange}");
            report.AppendLine(
                $"EvaluateFoamShape source-range isolation: {shapeRangeClean}");
            report.AppendLine(
                $"ClearRange retains its independent full-Y ownership: " +
                $"{clearRangeOwnership}");
            report.AppendLine(
                $"Metric manual-source uniforms declared/bound: " +
                $"{uniformsDeclared}/{uniformsBound}");
            report.AppendLine(
                $"Descriptor and source Y-range C# binding: " +
                $"{sourceRangeBound}");
            report.AppendLine(
                "EVALUATOR IDENTITY VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP7LifecycleContracts(
            StringBuilder report,
            int pendingInjectionCountBefore,
            int pendingMaterialBirthCountBefore,
            int activeCompositionCountBefore,
            int activeAutomaticCountBefore)
        {
            report.AppendLine("LIFECYCLE AND CAPACITY INVARIANTS");
            int sourceStride = Marshal.SizeOf<FoamSourceEventGpuData>();
            bool exact = FoamCompositionEventCapacity == 8 &&
                AutomaticFoamSourceEventCapacity == 32 &&
                foamCompositionEvents.Length == FoamCompositionEventCapacity &&
                automaticFoamSourceEvents.Length ==
                    AutomaticFoamSourceEventCapacity &&
                automaticFoamSourceEventGpuData.Length ==
                    AutomaticFoamSourceEventCapacity &&
                sourceStride == sizeof(float) * 32 &&
                pendingInjections.Count == pendingInjectionCountBefore &&
                pendingMaterialBirths.Count == pendingMaterialBirthCountBefore &&
                activeFoamCompositionEventCount == activeCompositionCountBefore &&
                activeAutomaticFoamSourceEventCount == activeAutomaticCountBefore;

            report.AppendLine(
                $"Composition capacity: constant={FoamCompositionEventCapacity}; " +
                $"array={foamCompositionEvents.Length}");
            report.AppendLine(
                $"Automatic capacity: constant=" +
                $"{AutomaticFoamSourceEventCapacity}; eventArray=" +
                $"{automaticFoamSourceEvents.Length}; gpuArray=" +
                $"{automaticFoamSourceEventGpuData.Length}");
            report.AppendLine(
                $"FoamSourceEventGpuData stride: {sourceStride} bytes; " +
                $"expected={sizeof(float) * 32}");
            report.AppendLine(
                $"Queue/event state unchanged while validating: injections=" +
                $"{pendingInjections.Count}/{pendingInjectionCountBefore}; " +
                $"materialBirths={pendingMaterialBirths.Count}/" +
                $"{pendingMaterialBirthCountBefore}; compositions=" +
                $"{activeFoamCompositionEventCount}/{activeCompositionCountBefore}; " +
                $"automatic={activeAutomaticFoamSourceEventCount}/" +
                $"{activeAutomaticCountBefore}");
            report.AppendLine(
                "LIFECYCLE/CAPACITY VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private static bool P7AutomaticGpuContractExact(
            AutomaticFoamSourceEvent sourceEvent,
            FoamSourceEventGpuData legacy,
            FoamSourceEventGpuData fixedMetric)
        {
            bool sharedLanesExact =
                P7VectorBitsEqual(legacy.Header, fixedMetric.Header) &&
                P7VectorBitsEqual(legacy.Distance, fixedMetric.Distance) &&
                P7FloatBitsEqual(legacy.Shore.x, fixedMetric.Shore.x) &&
                P7FloatBitsEqual(legacy.Shore.z, fixedMetric.Shore.z) &&
                P7FloatBitsEqual(legacy.Shore.w, fixedMetric.Shore.w) &&
                P7VectorBitsEqual(legacy.Material, fixedMetric.Material) &&
                P7VectorBitsEqual(legacy.Variation, fixedMetric.Variation) &&
                P7VectorBitsEqual(legacy.Kinematics, fixedMetric.Kinematics) &&
                P7VectorBitsEqual(legacy.ObjectData, fixedMetric.ObjectData) &&
                P7VectorBitsEqual(legacy.Deposit, fixedMetric.Deposit);
            if (!sharedLanesExact)
            {
                return false;
            }

            if (sourceEvent.Type ==
                AutomaticFoamSourceEventType.ShoreRibbon)
            {
                return P7FloatBitsEqual(
                        legacy.Shore.y,
                        sourceEvent.ShoreRibbonThicknessCells) &&
                    P7FloatBitsEqual(
                        fixedMetric.Shore.y,
                        sourceEvent.ShoreRibbonThicknessMetres);
            }

            return P7FloatBitsEqual(
                legacy.Shore.y,
                fixedMetric.Shore.y);
        }

        private static bool P7VectorBitsEqual(Vector4 left, Vector4 right)
        {
            return P7FloatBitsEqual(left.x, right.x) &&
                P7FloatBitsEqual(left.y, right.y) &&
                P7FloatBitsEqual(left.z, right.z) &&
                P7FloatBitsEqual(left.w, right.w);
        }

        private static string P7ResolveProjectAbsolutePath(
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

        private bool P7RangeContainsMetricPoint(
            P7SourceDispatchRange range,
            StylizedRiverFoamGridDescriptor descriptor,
            float globalDistance,
            float lateralMetres)
        {
            float localDistance = globalDistance -
                river.Domain.GlobalDistanceMinimum;
            if (!descriptor.TryMetricToContainingCell(
                    new Vector2(localDistance, lateralMetres),
                    out Vector2Int cell))
            {
                return false;
            }

            return cell.x >= range.StartX && cell.x <= range.EndX &&
                cell.y >= range.StartY && cell.y <= range.EndY;
        }

        private static bool P7RangeWithinDescriptor(
            P7SourceDispatchRange range,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            return range.IsValid &&
                range.StartX >= 0 && range.StartY >= 0 &&
                range.EndX < descriptor.ColumnCount &&
                range.EndY < descriptor.RowCount;
        }

        private static bool P7RangesEqual(
            P7SourceDispatchRange left,
            P7SourceDispatchRange right)
        {
            return left.StartX == right.StartX &&
                left.CountX == right.CountX &&
                left.StartY == right.StartY &&
                left.CountY == right.CountY;
        }

        private static bool P7FloatBitsEqual(float left, float right)
        {
            return BitConverter.SingleToInt32Bits(left) ==
                BitConverter.SingleToInt32Bits(right);
        }

        private static string P7RangeText(P7SourceDispatchRange range)
        {
            return range.IsValid
                ? $"x={range.StartX}..{range.EndX},y=" +
                    $"{range.StartY}..{range.EndY}"
                : "invalid";
        }

        private static void AppendP7SkippedContract(
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


        private static bool P7ContainsOrdinal(string text, string value)
        {
            return !string.IsNullOrEmpty(text) &&
                !string.IsNullOrEmpty(value) &&
                text.IndexOf(value, StringComparison.Ordinal) >= 0;
        }

        private static int P7CountOccurrences(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(
                       value,
                       index,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }
            return count;
        }

        private static string P7ExtractFunctionBody(
            string source,
            string signature)
        {
            if (string.IsNullOrEmpty(source) ||
                string.IsNullOrEmpty(signature))
            {
                return string.Empty;
            }

            int signatureIndex = source.IndexOf(
                signature,
                StringComparison.Ordinal);
            if (signatureIndex < 0)
            {
                return string.Empty;
            }

            int openBrace = source.IndexOf('{', signatureIndex);
            if (openBrace < 0)
            {
                return string.Empty;
            }

            int depth = 0;
            for (int index = openBrace; index < source.Length; index++)
            {
                char character = source[index];
                if (character == '{')
                {
                    depth++;
                }
                else if (character == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return source.Substring(
                            openBrace,
                            index - openBrace + 1);
                    }
                }
            }

            return string.Empty;
        }

        private bool FinalizeP7Report(
            StringBuilder report,
            bool passed,
            string summary)
        {
            topologyCacheDiagnosticState = passed ? "Passed" : "Failed";
            topologyCacheDiagnosticSummary = summary ?? string.Empty;
            topologyCacheDiagnosticReport = report?.ToString() ?? string.Empty;
            if (!TryWriteLatestDiagnosticReport(
                    "LatestP7ComprehensiveValidation",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string writeError))
            {
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    "The P7 report could not be written: " + writeError;
                Debug.LogError(
                    "[River Foam P7] " + topologyCacheDiagnosticSummary,
                    river);
                return false;
            }

            if (passed)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log(
                    "[River Foam P7] PASS — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            else
            {
                Debug.LogError(
                    "[River Foam P7] FAIL — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            return passed;
        }
    }
}
#endif
