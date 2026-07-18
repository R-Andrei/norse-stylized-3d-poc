#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private string topologyCacheDiagnosticState = "Not Run";
        private string topologyCacheDiagnosticSummary =
            "Run an explicit Edit Mode audit from Foam Cache & Validation.";
        private string topologyCacheDiagnosticReport = string.Empty;
        private string topologyCacheDiagnosticReportPath = string.Empty;
        private int topologyCacheDiagnosticRunCount;
        private int topologyCacheDiagnosticPassCount;
        private bool p53CpuGpuCaptureRequested;
        private bool p53CpuGpuCaptureAvailable;
        private bool p53CpuGpuCapturePublicationExact;
        private bool p53CpuGpuCapturePhaseExact;
        private string p53CpuGpuCaptureLabel = string.Empty;
        private string p53CpuGpuCaptureReport = string.Empty;

        public string TopologyCacheDiagnosticState =>
            topologyCacheDiagnosticState;
        public string TopologyCacheDiagnosticSummary =>
            topologyCacheDiagnosticSummary;
        public string TopologyCacheDiagnosticReport =>
            topologyCacheDiagnosticReport;
        public string TopologyCacheDiagnosticReportPath =>
            topologyCacheDiagnosticReportPath;
        public int TopologyCacheDiagnosticRunCount =>
            topologyCacheDiagnosticRunCount;
        public int TopologyCacheDiagnosticPassCount =>
            topologyCacheDiagnosticPassCount;

        /// <summary>
        /// One-button P5.3 validation pipeline. It captures exact provider/direct
        /// obstacle identity, runs five independent non-storing cache builds at
        /// the canonical topology evaluation phase, compares the frozen pre-P5
        /// and descriptor-owned legacy raster paths, verifies CPU candidate/GPU
        /// publication ownership, and proves that the assigned cache asset was not
        /// mutated. The same report is rerun once after explicit rebuild and an
        /// Editor restart as the complete persistence confirmation.
        /// </summary>
        public bool RunP53ComprehensiveValidationReport()
        {
#if UNITY_EDITOR
            const int BuildCount = 5;
            topologyCacheDiagnosticRunCount++;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary =
                "Running the P5.3 deterministic-phase, provider, legacy-raster, " +
                "five-build, publication, and asset-mutation validation pipeline.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;

            if (Application.isPlaying)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "The P5.3 comprehensive report is Edit Mode only.");
            }

            river = GetComponent<StylizedRiver>();
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (river == null || disturbanceRuntime == null ||
                !river.Domain.IsValid)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "A valid river and disturbance runtime are required.");
            }

            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            StringBuilder report = new(196608);
            report.AppendLine(
                "RIVER FOAM FIXED-METRIC P5.3 COMPREHENSIVE VALIDATION");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                "Operation: one explicit user-triggered report; five " +
                "independent Edit Mode topology preparations; no generated " +
                "artifact is stored.");
            report.AppendLine(
                "Contracts: generated-provider zero rejection + obstacle set " +
                "fingerprint contract 2 + cache generator contract 4 + topology " +
                "phase contract 1 at time zero + frozen pre-P5 legacy raster parity.");
            report.AppendLine(
                $"Topology evaluation phase: contract " +
                $"{TopologyEvaluationPhaseContractVersion}, time=" +
                $"{TopologyEvaluationTimeSeconds:R}[0x" +
                $"{BitConverter.SingleToInt32Bits(TopologyEvaluationTimeSeconds):X8}]; " +
                "live river motion time is ignored by obstacle cache generation.");
            report.AppendLine();

            StylizedRiverFoamTopologyCacheAsset assignedAsset =
                river.FoamTopologyCacheAsset;
            bool assignedSupported = false;
            bool assignedLegacyCoordinate = false;
            bool assignedLegacyObstacle = false;
            bool assignedLegacyDynamicPhase = false;
            string assignedContractSummary = string.Empty;
            string assignedMetadataBefore = assignedAsset != null
                ? BuildAssignedCacheMetadataSignature(assignedAsset)
                : "<none>";
            byte[] assignedPayloadBefore = assignedAsset != null &&
                assignedAsset.HasPayload
                    ? (byte[])assignedAsset.GetPayloadReadOnlyReference().Clone()
                    : Array.Empty<byte>();
            if (assignedAsset != null)
            {
                AppendAssignedCacheMetadata(report, assignedAsset);
                assignedSupported = assignedAsset.TryValidateStoredContract(
                    out assignedLegacyCoordinate,
                    out assignedLegacyObstacle,
                    out assignedLegacyDynamicPhase,
                    out assignedContractSummary);
                report.AppendLine(
                    $"Assigned contract classification: " +
                    (assignedSupported
                        ? "CURRENT"
                        : assignedLegacyCoordinate
                            ? "LEGACY COORDINATE"
                            : assignedLegacyObstacle
                                ? "LEGACY OBSTACLE FINGERPRINT"
                                : assignedLegacyDynamicPhase
                                    ? "LEGACY DYNAMIC TOPOLOGY PHASE"
                                    : "UNSUPPORTED"));
                report.AppendLine(
                    "Contract detail: " + assignedContractSummary);
            }
            else
            {
                report.AppendLine("ASSIGNED CACHE: none");
            }
            report.AppendLine();

            if (!disturbanceRuntime
                    .TryCaptureObstacleExclusionDiagnosticSnapshot(
                        "P5.3 Initial Inputs",
                        out StylizedRiverFoamObstacleDiagnosticSnapshot
                            initialObstacles,
                        out string initialObstacleError))
            {
                report.AppendLine(
                    "INITIAL OBSTACLE CAPTURE FAILED: " +
                    initialObstacleError);
                return FinalizeP53Report(
                    report,
                    stopwatch,
                    false,
                    "Obstacle provenance capture failed.");
            }
            report.AppendLine(
                StylizedRiverFoamCacheDiagnosticUtility
                    .BuildObstacleSnapshotReport(initialObstacles));
            bool providerDirectExact =
                AllProvidersAgreeWithDirect(initialObstacles);

            List<StylizedRiverFoamCacheDiagnosticSnapshot> snapshots = new();
            bool obstacleStable = true;
            bool cpuGpuPublicationExact = true;
            bool topologyPhaseExact = true;
            bool buildsExact = true;
            StylizedRiverFoamTopologyCacheBuildArtifact referenceArtifact =
                default;
            byte[] referencePayload = Array.Empty<byte>();
            StylizedRiverFoamObstacleDiagnosticSnapshot previousObstacles =
                initialObstacles;

            for (int buildIndex = 0;
                 buildIndex < BuildCount;
                 buildIndex++)
            {
                string buildLabel = $"BUILD {buildIndex + 1}";
                BeginP53CpuGpuObstacleCapture(buildLabel);
                if (!TryPrepareTopologyCacheInEditor(
                        out StylizedRiverFoamTopologyCacheBuildArtifact build))
                {
                    CancelP53CpuGpuObstacleCapture();
                    report.AppendLine(
                        buildLabel + " FAILED: " +
                        TopologyCacheBuildState + ". " +
                        TopologyCacheBuildSummary);
                    return FinalizeP53Report(
                        report,
                        stopwatch,
                        false,
                        buildLabel + " failed.");
                }

                if (!TryConsumeP53CpuGpuObstacleCapture(
                        out bool buildCpuGpuPublicationExact,
                        out bool buildTopologyPhaseExact,
                        out string cpuGpuReport,
                        out string cpuGpuCaptureError))
                {
                    report.AppendLine(
                        buildLabel + " CPU/GPU CAPTURE FAILED: " +
                        cpuGpuCaptureError);
                    return FinalizeP53Report(
                        report,
                        stopwatch,
                        false,
                        buildLabel + " CPU/GPU capture failed.");
                }

                if (!StylizedRiverFoamTopologyCacheCodec
                        .TryCreateDiagnosticSnapshot(
                            build.Payload,
                            buildLabel,
                            out StylizedRiverFoamCacheDiagnosticSnapshot
                                snapshot,
                            out string snapshotError))
                {
                    report.AppendLine(
                        buildLabel + " SNAPSHOT FAILED: " + snapshotError);
                    return FinalizeP53Report(
                        report,
                        stopwatch,
                        false,
                        buildLabel + " snapshot failed.");
                }

                snapshots.Add(snapshot);
                if (buildIndex == 0)
                {
                    referenceArtifact = build;
                    referencePayload = build.CopyPayload();
                }
                AppendBuildArtifact(report, buildLabel, build, snapshot);
                cpuGpuPublicationExact &= buildCpuGpuPublicationExact;
                topologyPhaseExact &= buildTopologyPhaseExact;
                report.AppendLine(cpuGpuReport);
                report.AppendLine();

                if (!disturbanceRuntime
                        .TryCaptureObstacleExclusionDiagnosticSnapshot(
                            $"After {buildLabel}",
                            out StylizedRiverFoamObstacleDiagnosticSnapshot
                                currentObstacles,
                            out string currentObstacleError,
                            prepareRegistry: false))
                {
                    report.AppendLine(
                        $"OBSTACLE CAPTURE AFTER {buildLabel} FAILED: " +
                        currentObstacleError);
                    return FinalizeP53Report(
                        report,
                        stopwatch,
                        false,
                        "Obstacle capture failed after " + buildLabel + ".");
                }

                bool stepStable =
                    StylizedRiverFoamCacheDiagnosticUtility
                        .ObstacleSnapshotsEqual(
                            previousObstacles,
                            currentObstacles,
                            out string obstacleComparison);
                obstacleStable &= stepStable;
                providerDirectExact &=
                    AllProvidersAgreeWithDirect(currentObstacles);
                report.AppendLine(
                    $"OBSTACLE STABILITY AFTER {buildLabel}: " +
                    (stepStable ? "EXACT" : "CHANGED"));
                report.AppendLine(obstacleComparison);
                report.AppendLine();
                previousObstacles = currentObstacles;
            }

            StylizedRiverFoamCacheDiagnosticSnapshot reference = snapshots[0];
            report.AppendLine("FIVE-BUILD DETERMINISM MATRIX");
            for (int index = 0; index < snapshots.Count; index++)
            {
                StylizedRiverFoamCacheDiagnosticSnapshot candidate =
                    snapshots[index];
                string comparison =
                    StylizedRiverFoamTopologyCacheCodec
                        .CompareDiagnosticSnapshots(
                            reference,
                            candidate,
                            out bool exactBuild);
                buildsExact &= exactBuild;
                report.AppendLine(
                    $"Build 1 vs Build {index + 1}: " +
                    (exactBuild ? "EXACT" : "DIFFERENT"));
                report.AppendLine(comparison);
            }
            report.AppendLine();

            bool legacyParityRan = disturbanceRuntime
                .TryBuildObstacleRasterParityReport(
                    reference.Descriptor,
                    out bool legacyParityExact,
                    out string legacyParityReport,
                    out string legacyParityError);
            report.AppendLine("TRUE PRE-P5 VS P5 LEGACY RASTER PARITY");
            if (legacyParityRan)
            {
                report.AppendLine(legacyParityReport);
            }
            else
            {
                legacyParityExact = false;
                report.AppendLine("DIAGNOSTIC ERROR: " + legacyParityError);
            }
            report.AppendLine();

            bool assignedStageAccepted = false;
            bool assignedCurrentPayloadExact = false;
            bool assignedCurrentMetadataExact = false;
            int assignedReferenceFirstDifference = -1;
            report.AppendLine("ASSIGNED CACHE STAGE / CURRENT PARITY");
            if (assignedAsset == null)
            {
                report.AppendLine("Stage: MISSING — an assigned cache is required.");
            }
            else if (assignedLegacyDynamicPhase)
            {
                assignedStageAccepted = true;
                report.AppendLine(
                    "Stage: PRE-REBUILD LEGACY DYNAMIC TOPOLOGY PHASE — " +
                    "accepted only for this non-mutating deterministic-phase gate.");
                report.AppendLine(
                    "Required next action after Overall PASS: explicitly rebuild " +
                    "the assigned cache under generator contract 4.");
            }
            else if (assignedSupported)
            {
                assignedCurrentPayloadExact = ByteArraysEqual(
                    assignedPayloadBefore,
                    referencePayload,
                    out assignedReferenceFirstDifference);
                assignedCurrentMetadataExact =
                    AssignedCacheMetadataMatchesArtifact(
                        assignedAsset,
                        referenceArtifact,
                        out string assignedMetadataComparison);
                assignedStageAccepted =
                    assignedCurrentPayloadExact &&
                    assignedCurrentMetadataExact;
                report.AppendLine("Stage: POST-REBUILD CURRENT CONTRACT");
                report.AppendLine(
                    "Payload vs Build 1: " +
                    (assignedCurrentPayloadExact ? "EXACT" : "DIFFERENT"));
                if (!assignedCurrentPayloadExact)
                {
                    report.AppendLine(
                        $"First payload difference: " +
                        $"{assignedReferenceFirstDifference:N0}");
                }
                report.AppendLine(
                    "Metadata vs Build 1: " +
                    (assignedCurrentMetadataExact ? "EXACT" : "DIFFERENT"));
                report.AppendLine(assignedMetadataComparison);
            }
            else
            {
                report.AppendLine(
                    "Stage: REJECTED — " + assignedContractSummary);
            }
            report.AppendLine(
                "Stage/current parity verdict: " +
                (assignedStageAccepted ? "PASS" : "FAIL"));
            report.AppendLine();

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
                out int firstAssignedDifference);
            bool assignedUnchanged = metadataUnchanged && payloadUnchanged;
            report.AppendLine("ASSIGNED CACHE MUTATION PROOF");
            report.AppendLine(
                $"Metadata: {(metadataUnchanged ? "EXACT" : "CHANGED")}");
            report.AppendLine(
                $"Payload: {(payloadUnchanged ? "EXACT" : "CHANGED")}");
            if (!payloadUnchanged)
            {
                report.AppendLine(
                    $"First payload difference: {firstAssignedDifference:N0}");
            }
            report.AppendLine();

            bool inputFingerprintsExact = true;
            for (int index = 1; index < snapshots.Count; index++)
            {
                inputFingerprintsExact &=
                    reference.DomainFingerprint ==
                        snapshots[index].DomainFingerprint &&
                    reference.ObstacleFingerprint ==
                        snapshots[index].ObstacleFingerprint &&
                    reference.GenerationFingerprint ==
                        snapshots[index].GenerationFingerprint &&
                    reference.CombinedFingerprint ==
                        snapshots[index].CombinedFingerprint;
            }

            bool passed =
                assignedUnchanged &&
                assignedStageAccepted &&
                providerDirectExact &&
                obstacleStable &&
                inputFingerprintsExact &&
                buildsExact &&
                cpuGpuPublicationExact &&
                topologyPhaseExact &&
                legacyParityRan &&
                legacyParityExact;
            stopwatch.Stop();
            report.AppendLine("FINAL VERDICT");
            report.AppendLine(
                $"Provider/direct fingerprint parity: " +
                (providerDirectExact ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Obstacle stability across five builds: " +
                (obstacleStable ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Input fingerprint parity: " +
                (inputFingerprintsExact ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Five-build payload/section determinism: " +
                (buildsExact ? "PASS" : "FAIL"));
            report.AppendLine(
                $"CPU candidate/GPU publication parity: " +
                (cpuGpuPublicationExact ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Topology evaluation phase: " +
                (topologyPhaseExact ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Frozen pre-P5 vs descriptor legacy raster: " +
                (legacyParityExact ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Assigned cache stage/current parity: " +
                (assignedStageAccepted ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Assigned cache remained unchanged: " +
                (assignedUnchanged ? "PASS" : "FAIL"));
            report.AppendLine(
                $"Overall: {(passed ? "PASS" : "FAIL")}");
            report.AppendLine(
                $"Elapsed: {stopwatch.Elapsed.TotalMilliseconds:0.000} ms");
            report.AppendLine(
                "Validation workflow: paste this one report before rebuild, then " +
                "rerun the same button once after explicit rebuild and Editor " +
                "restart. Additional manual evidence is requested only when this " +
                "report cannot isolate a failure.");

            return FinalizeP53Report(
                report,
                stopwatch,
                passed,
                passed
                    ? "P5.3 comprehensive validation passed."
                    : "P5.3 comprehensive validation found one or more failures.");
#else
            return false;
#endif
        }

        public bool RunTopologyCacheDeterminismDiagnosticAudit()
        {
#if UNITY_EDITOR
            topologyCacheDiagnosticRunCount++;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary =
                "Capturing obstacle provenance and two independent cache builds.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;

            if (Application.isPlaying)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "The full determinism audit is Edit Mode only.");
            }

            river = GetComponent<StylizedRiver>();
            if (river == null || !river.Domain.IsValid)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "A valid StylizedRiver domain is required.");
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (disturbanceRuntime == null)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "The river has no StylizedRiverDisturbanceRuntime for " +
                    "obstacle-source provenance capture.");
            }

            System.Diagnostics.Stopwatch auditStopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            StringBuilder report = new(65536);
            AppendAuditHeader(report);

            StylizedRiverFoamTopologyCacheAsset assignedAsset =
                river.FoamTopologyCacheAsset;
            string assignedMetadataBefore = assignedAsset != null
                ? BuildAssignedCacheMetadataSignature(assignedAsset)
                : "<none>";
            StylizedRiverFoamCacheDiagnosticSnapshot assignedSnapshot = null;
            string assignedError = string.Empty;
            if (assignedAsset == null)
            {
                report.AppendLine("ASSIGNED CACHE: none");
            }
            else
            {
                AppendAssignedCacheMetadata(report, assignedAsset);
                if (assignedAsset.HasPayload &&
                    StylizedRiverFoamTopologyCacheCodec
                        .TryCreateDiagnosticSnapshot(
                            assignedAsset.GetPayloadReadOnlyReference(),
                            "Assigned Cache",
                            out assignedSnapshot,
                            out assignedError))
                {
                    report.AppendLine(
                        "Assigned cache diagnostic snapshot: available.");
                }
                else
                {
                    report.AppendLine(
                        "Assigned cache diagnostic snapshot: unavailable — " +
                        assignedError);
                }
            }
            report.AppendLine();

            if (!disturbanceRuntime
                    .TryCaptureObstacleExclusionDiagnosticSnapshot(
                        "Before Build A",
                        out StylizedRiverFoamObstacleDiagnosticSnapshot
                            obstaclesBeforeA,
                        out string obstacleErrorA))
            {
                report.AppendLine(
                    "OBSTACLE CAPTURE BEFORE A FAILED: " + obstacleErrorA);
                return FinalizeFailedAudit(
                    report,
                    auditStopwatch,
                    "Obstacle capture failed before Build A.");
            }
            report.AppendLine(
                StylizedRiverFoamCacheDiagnosticUtility
                    .BuildObstacleSnapshotReport(obstaclesBeforeA));

            if (!TryPrepareTopologyCacheInEditor(
                    out StylizedRiverFoamTopologyCacheBuildArtifact buildA))
            {
                report.AppendLine(
                    "BUILD A FAILED: " + TopologyCacheBuildState + ". " +
                    TopologyCacheBuildSummary);
                return FinalizeFailedAudit(
                    report,
                    auditStopwatch,
                    "Build A failed.");
            }
            if (!StylizedRiverFoamTopologyCacheCodec
                    .TryCreateDiagnosticSnapshot(
                        buildA.Payload,
                        "Build A",
                        out StylizedRiverFoamCacheDiagnosticSnapshot snapshotA,
                        out string snapshotErrorA))
            {
                report.AppendLine(
                    "BUILD A SNAPSHOT FAILED: " + snapshotErrorA);
                return FinalizeFailedAudit(
                    report,
                    auditStopwatch,
                    "Build A snapshot failed.");
            }
            AppendBuildArtifact(report, "BUILD A", buildA, snapshotA);

            if (!disturbanceRuntime
                    .TryCaptureObstacleExclusionDiagnosticSnapshot(
                        "Between Build A and Build B",
                        out StylizedRiverFoamObstacleDiagnosticSnapshot
                            obstaclesBetween,
                        out string obstacleErrorBetween))
            {
                report.AppendLine(
                    "OBSTACLE CAPTURE BETWEEN BUILDS FAILED: " +
                    obstacleErrorBetween);
                return FinalizeFailedAudit(
                    report,
                    auditStopwatch,
                    "Obstacle capture failed between builds.");
            }

            report.AppendLine(
                StylizedRiverFoamCacheDiagnosticUtility
                    .BuildObstacleSnapshotReport(obstaclesBetween));

            if (!TryPrepareTopologyCacheInEditor(
                    out StylizedRiverFoamTopologyCacheBuildArtifact buildB))
            {
                report.AppendLine(
                    "BUILD B FAILED: " + TopologyCacheBuildState + ". " +
                    TopologyCacheBuildSummary);
                return FinalizeFailedAudit(
                    report,
                    auditStopwatch,
                    "Build B failed.");
            }
            if (!StylizedRiverFoamTopologyCacheCodec
                    .TryCreateDiagnosticSnapshot(
                        buildB.Payload,
                        "Build B",
                        out StylizedRiverFoamCacheDiagnosticSnapshot snapshotB,
                        out string snapshotErrorB))
            {
                report.AppendLine(
                    "BUILD B SNAPSHOT FAILED: " + snapshotErrorB);
                return FinalizeFailedAudit(
                    report,
                    auditStopwatch,
                    "Build B snapshot failed.");
            }
            AppendBuildArtifact(report, "BUILD B", buildB, snapshotB);

            if (!disturbanceRuntime
                    .TryCaptureObstacleExclusionDiagnosticSnapshot(
                        "After Build B",
                        out StylizedRiverFoamObstacleDiagnosticSnapshot
                            obstaclesAfterB,
                        out string obstacleErrorAfter))
            {
                report.AppendLine(
                    "OBSTACLE CAPTURE AFTER B FAILED: " +
                    obstacleErrorAfter);
                return FinalizeFailedAudit(
                    report,
                    auditStopwatch,
                    "Obstacle capture failed after Build B.");
            }

            bool obstacleStableA =
                StylizedRiverFoamCacheDiagnosticUtility.ObstacleSnapshotsEqual(
                    obstaclesBeforeA,
                    obstaclesBetween,
                    out string obstacleComparisonA);
            bool obstacleStableB =
                StylizedRiverFoamCacheDiagnosticUtility.ObstacleSnapshotsEqual(
                    obstaclesBetween,
                    obstaclesAfterB,
                    out string obstacleComparisonB);
            bool obstacleStable = obstacleStableA && obstacleStableB;
            bool providerDirectAgreement =
                AllProvidersAgreeWithDirect(obstaclesBeforeA) &&
                AllProvidersAgreeWithDirect(obstaclesBetween) &&
                AllProvidersAgreeWithDirect(obstaclesAfterB);
            bool buildAObstacleCaptureAgreement =
                snapshotA.ObstacleFingerprint ==
                    obstaclesBetween.CombinedFingerprint;
            bool buildBObstacleCaptureAgreement =
                snapshotB.ObstacleFingerprint ==
                    obstaclesAfterB.CombinedFingerprint;
            bool buildObstacleCaptureAgreement =
                buildAObstacleCaptureAgreement &&
                buildBObstacleCaptureAgreement;

            report.AppendLine("OBSTACLE STABILITY COMPARISON");
            report.AppendLine(
                $"Before A -> Between: " +
                $"{(obstacleStableA ? "EXACT" : "CHANGED")}");
            report.AppendLine(obstacleComparisonA);
            report.AppendLine(
                $"Between -> After B: " +
                $"{(obstacleStableB ? "EXACT" : "CHANGED")}");
            report.AppendLine(obstacleComparisonB);
            report.AppendLine(
                $"Provider/direct exact-world agreement: " +
                $"{(providerDirectAgreement ? "PASS" : "FAIL")}");
            report.AppendLine(
                $"Build A obstacle fingerprint vs post-A capture: " +
                $"{(buildAObstacleCaptureAgreement ? "PASS" : "FAIL")} " +
                $"{snapshotA.ObstacleFingerprint} -> " +
                obstaclesBetween.CombinedFingerprint);
            report.AppendLine(
                $"Build B obstacle fingerprint vs post-B capture: " +
                $"{(buildBObstacleCaptureAgreement ? "PASS" : "FAIL")} " +
                $"{snapshotB.ObstacleFingerprint} -> " +
                obstaclesAfterB.CombinedFingerprint);
            report.AppendLine();
            report.AppendLine(
                StylizedRiverFoamCacheDiagnosticUtility
                    .BuildObstacleSnapshotReport(obstaclesAfterB));

            bool assignedAssetUnchanged = true;
            report.AppendLine("ASSIGNED CACHE MUTATION CHECK");
            if (assignedAsset == null)
            {
                report.AppendLine("No assigned cache asset; not applicable.");
            }
            else
            {
                string assignedMetadataAfter =
                    BuildAssignedCacheMetadataSignature(assignedAsset);
                bool metadataUnchanged = string.Equals(
                    assignedMetadataBefore,
                    assignedMetadataAfter,
                    StringComparison.Ordinal);
                bool payloadUnchanged;
                string payloadMutationEvidence;
                if (assignedSnapshot == null)
                {
                    payloadUnchanged = !assignedAsset.HasPayload;
                    payloadMutationEvidence = assignedAsset.HasPayload
                        ? "Initial assigned payload could not be decoded; " +
                          "mutation could not be proven."
                        : "Assigned asset had no payload before or after audit.";
                }
                else if (!StylizedRiverFoamTopologyCacheCodec
                             .TryCreateDiagnosticSnapshot(
                                 assignedAsset.GetPayloadReadOnlyReference(),
                                 "Assigned Cache After Audit",
                                 out StylizedRiverFoamCacheDiagnosticSnapshot
                                     assignedAfterSnapshot,
                                 out string assignedAfterError))
                {
                    payloadUnchanged = false;
                    payloadMutationEvidence =
                        "Post-audit assigned payload could not be decoded: " +
                        assignedAfterError;
                }
                else
                {
                    payloadMutationEvidence =
                        StylizedRiverFoamTopologyCacheCodec
                            .CompareDiagnosticSnapshots(
                                assignedSnapshot,
                                assignedAfterSnapshot,
                                out payloadUnchanged);
                }

                assignedAssetUnchanged =
                    metadataUnchanged && payloadUnchanged;
                report.AppendLine(
                    $"Metadata: {(metadataUnchanged ? "EXACT" : "CHANGED")}");
                if (!metadataUnchanged)
                {
                    report.AppendLine("Before: " + assignedMetadataBefore);
                    report.AppendLine("After:  " + assignedMetadataAfter);
                }
                report.AppendLine(
                    $"Payload: {(payloadUnchanged ? "EXACT" : "CHANGED")}");
                report.AppendLine(payloadMutationEvidence);
            }
            report.AppendLine();

            string buildComparison =
                StylizedRiverFoamTopologyCacheCodec
                    .CompareDiagnosticSnapshots(
                        snapshotA,
                        snapshotB,
                        out bool buildExact);
            report.AppendLine(buildComparison);
            report.AppendLine();

            if (assignedSnapshot != null)
            {
                report.AppendLine(
                    StylizedRiverFoamTopologyCacheCodec
                        .CompareDiagnosticSnapshots(
                            assignedSnapshot,
                            snapshotA,
                            out bool assignedEqualsA));
                report.AppendLine(
                    $"ASSIGNED CACHE VS BUILD A: " +
                    $"{(assignedEqualsA ? "EXACT" : "DIFFERENT")}");
                report.AppendLine();
                report.AppendLine(
                    StylizedRiverFoamTopologyCacheCodec
                        .CompareDiagnosticSnapshots(
                            assignedSnapshot,
                            snapshotB,
                            out bool assignedEqualsB));
                report.AppendLine(
                    $"ASSIGNED CACHE VS BUILD B: " +
                    $"{(assignedEqualsB ? "EXACT" : "DIFFERENT")}");
                report.AppendLine();
            }

            bool inputFingerprintsExact =
                snapshotA.DomainFingerprint == snapshotB.DomainFingerprint &&
                snapshotA.ObstacleFingerprint ==
                    snapshotB.ObstacleFingerprint &&
                snapshotA.GenerationFingerprint ==
                    snapshotB.GenerationFingerprint &&
                snapshotA.CombinedFingerprint ==
                    snapshotB.CombinedFingerprint;
            string classification = ResolveAuditClassification(
                assignedAssetUnchanged,
                obstacleStable,
                providerDirectAgreement,
                buildObstacleCaptureAgreement,
                inputFingerprintsExact,
                buildExact,
                snapshotA,
                snapshotB);
            bool passed =
                assignedAssetUnchanged &&
                obstacleStable &&
                providerDirectAgreement &&
                buildObstacleCaptureAgreement &&
                inputFingerprintsExact &&
                buildExact;

            auditStopwatch.Stop();
            report.AppendLine("FINAL VERDICT");
            report.AppendLine($"Classification: {classification}");
            report.AppendLine(
                $"Assigned cache remained unchanged: " +
                $"{(assignedAssetUnchanged ? "PASS" : "FAIL")}");
            report.AppendLine(
                $"Obstacle source stability: " +
                $"{(obstacleStable ? "PASS" : "FAIL")}");
            report.AppendLine(
                $"Provider/direct fingerprint parity: " +
                $"{(providerDirectAgreement ? "PASS" : "FAIL")}");
            report.AppendLine(
                $"Build/captured-obstacle fingerprint parity: " +
                $"{(buildObstacleCaptureAgreement ? "PASS" : "FAIL")}");
            report.AppendLine(
                $"Input fingerprint parity: " +
                $"{(inputFingerprintsExact ? "PASS" : "FAIL")}");
            report.AppendLine(
                $"Generated payload/section parity: " +
                $"{(buildExact ? "PASS" : "FAIL")}");
            report.AppendLine(
                $"Audit elapsed: " +
                $"{auditStopwatch.Elapsed.TotalMilliseconds:0.000} ms");
            report.AppendLine(
                "Persistent asset mutation check: " +
                (assignedAssetUnchanged
                    ? "PASS — neither build artifact was stored and the assigned " +
                      "cache metadata/payload remained exact."
                    : "FAIL — the assigned cache changed or could not be proven " +
                      "unchanged."));

            topologyCacheDiagnosticState = passed ? "Passed" : "Failed";
            topologyCacheDiagnosticSummary =
                $"{classification}. Build A={buildA.PayloadByteCount:N0}/" +
                $"{buildA.PayloadHash}; Build B=" +
                $"{buildB.PayloadByteCount:N0}/{buildB.PayloadHash}; " +
                $"obstacles={(obstacleStable ? "exact" : "changed")}.";
            topologyCacheDiagnosticReport = report.ToString();
            if (!TryWriteLatestDiagnosticReport(
                    "LatestCacheAudit",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string reportWriteError))
            {
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    "The audit completed, but its report could not be written: " +
                    reportWriteError;
                Debug.LogError(
                    "[River Foam Cache Diagnostic] " +
                    topologyCacheDiagnosticSummary + "\n" +
                    topologyCacheDiagnosticReport,
                    river);
                return false;
            }
            if (passed)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log(
                    "[River Foam Cache Diagnostic] " +
                    topologyCacheDiagnosticReport,
                    river);
            }
            else
            {
                Debug.LogError(
                    "[River Foam Cache Diagnostic] " +
                    topologyCacheDiagnosticReport,
                    river);
            }
            return passed;
#else
            return false;
#endif
        }

        public bool CaptureTopologyObstacleDiagnosticBaseline()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "Obstacle baseline capture is Edit Mode only.");
            }
            river = GetComponent<StylizedRiver>();
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (river == null || disturbanceRuntime == null ||
                !river.Domain.IsValid)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "A valid river and disturbance runtime are required.");
            }
            if (!disturbanceRuntime
                    .TryCaptureObstacleExclusionDiagnosticSnapshot(
                        "Explicit Baseline",
                        out StylizedRiverFoamObstacleDiagnosticSnapshot snapshot,
                        out string error))
            {
                return FailCacheDiagnostic("Failed", error);
            }

            string baselinePath = ResolveDiagnosticPath(
                "ObstacleBaseline",
                ".bin");
            string baselineReportPath = ResolveDiagnosticPath(
                "ObstacleBaseline",
                ".txt");
            try
            {
                StylizedRiverFoamCacheDiagnosticUtility.WriteObstacleBaseline(
                    baselinePath,
                    snapshot);
                if (!StylizedRiverFoamCacheDiagnosticUtility
                        .TryReadObstacleBaseline(
                            baselinePath,
                            out StylizedRiverFoamObstacleDiagnosticSnapshot
                                roundTrip,
                            out string roundTripError))
                {
                    return FailCacheDiagnostic(
                        "Failed",
                        "Obstacle baseline round-trip read failed: " +
                        roundTripError);
                }
                if (!StylizedRiverFoamCacheDiagnosticUtility
                        .ObstacleSnapshotsEqual(
                            snapshot,
                            roundTrip,
                            out string roundTripComparison))
                {
                    return FailCacheDiagnostic(
                        "Failed",
                        "Obstacle baseline round-trip changed production-relevant " +
                        "source data. " + roundTripComparison);
                }

                string report =
                    "RIVER FOAM OBSTACLE BASELINE\n" +
                    BuildCommonEnvironmentHeader() + "\n" +
                    "Machine baseline round-trip: PASS\n" +
                    StylizedRiverFoamCacheDiagnosticUtility
                        .BuildObstacleSnapshotReport(snapshot) +
                    $"\nMachine baseline: {baselinePath}\n";
                string directory = Path.GetDirectoryName(baselineReportPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(
                    baselineReportPath,
                    report,
                    Encoding.UTF8);

                topologyCacheDiagnosticState = "Baseline Captured";
                topologyCacheDiagnosticSummary =
                    $"Captured {snapshot.Sources.Count:N0} obstacle source(s), " +
                    $"fingerprint {snapshot.CombinedFingerprint}; binary " +
                    "round-trip passed.";
                topologyCacheDiagnosticReport = report;
                topologyCacheDiagnosticReportPath = baselineReportPath;
                Debug.Log("[River Foam Obstacle Baseline] " + report, river);
                return true;
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is ArgumentException)
            {
                return FailCacheDiagnostic(
                    "Failed",
                    "Obstacle baseline output failed: " + exception.Message);
            }
#else
            return false;
#endif
        }

        public bool CompareTopologyObstaclesAgainstDiagnosticBaseline()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "Obstacle baseline comparison is Edit Mode only.");
            }
            river = GetComponent<StylizedRiver>();
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (river == null || disturbanceRuntime == null ||
                !river.Domain.IsValid)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "A valid river and disturbance runtime are required.");
            }

            string baselinePath = ResolveDiagnosticPath(
                "ObstacleBaseline",
                ".bin");
            if (!StylizedRiverFoamCacheDiagnosticUtility
                    .TryReadObstacleBaseline(
                        baselinePath,
                        out StylizedRiverFoamObstacleDiagnosticSnapshot baseline,
                        out string baselineError))
            {
                return FailCacheDiagnostic("Failed", baselineError);
            }
            if (!disturbanceRuntime
                    .TryCaptureObstacleExclusionDiagnosticSnapshot(
                        "Current Comparison",
                        out StylizedRiverFoamObstacleDiagnosticSnapshot current,
                        out string currentError))
            {
                return FailCacheDiagnostic("Failed", currentError);
            }

            bool equal =
                StylizedRiverFoamCacheDiagnosticUtility.ObstacleSnapshotsEqual(
                    baseline,
                    current,
                    out string comparison);
            bool providerDirectAgreement = AllProvidersAgreeWithDirect(current);
            StringBuilder report = new(16384);
            report.AppendLine("RIVER FOAM OBSTACLE BASELINE COMPARISON");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                $"Baseline path: {baselinePath}");
            report.AppendLine(
                $"Verdict: {(equal ? "EXACT" : "CHANGED")}");
            report.AppendLine(
                $"Provider/direct agreement: " +
                $"{(providerDirectAgreement ? "PASS" : "FAIL")}");
            report.AppendLine(comparison);
            report.AppendLine();
            report.AppendLine(
                StylizedRiverFoamCacheDiagnosticUtility
                    .BuildObstacleSnapshotReport(baseline));
            report.AppendLine(
                StylizedRiverFoamCacheDiagnosticUtility
                    .BuildObstacleSnapshotReport(current));

            topologyCacheDiagnosticState =
                equal && providerDirectAgreement ? "Passed" : "Failed";
            topologyCacheDiagnosticSummary =
                $"Obstacle baseline {(equal ? "matches" : "changed")}; " +
                $"provider/direct parity=" +
                $"{(providerDirectAgreement ? "pass" : "fail")}.";
            topologyCacheDiagnosticReport = report.ToString();
            if (!TryWriteLatestDiagnosticReport(
                    "LatestObstacleComparison",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string reportWriteError))
            {
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    "The comparison completed, but its report could not be " +
                    "written: " + reportWriteError;
                Debug.LogError(
                    "[River Foam Obstacle Comparison] " +
                    topologyCacheDiagnosticSummary + "\n" +
                    topologyCacheDiagnosticReport,
                    river);
                return false;
            }
            if (equal && providerDirectAgreement)
            {
                Debug.Log(
                    "[River Foam Obstacle Comparison] " +
                    topologyCacheDiagnosticReport,
                    river);
                return true;
            }

            Debug.LogError(
                "[River Foam Obstacle Comparison] " +
                topologyCacheDiagnosticReport,
                river);
            return false;
#else
            return false;
#endif
        }

        public void LogLatestTopologyCacheDiagnosticReport()
        {
            if (string.IsNullOrEmpty(topologyCacheDiagnosticReport))
            {
                Debug.LogWarning(
                    "[River Foam Cache Diagnostic] No report has been run.",
                    river != null ? river : this);
                return;
            }
            Debug.Log(
                "[River Foam Cache Diagnostic] " +
                topologyCacheDiagnosticReport,
                river != null ? river : this);
        }

        private void BeginP53CpuGpuObstacleCapture(string label)
        {
            p53CpuGpuCaptureRequested = true;
            p53CpuGpuCaptureAvailable = false;
            p53CpuGpuCapturePublicationExact = false;
            p53CpuGpuCapturePhaseExact = false;
            p53CpuGpuCaptureLabel = label ?? string.Empty;
            p53CpuGpuCaptureReport = string.Empty;
        }

        private void CancelP53CpuGpuObstacleCapture()
        {
            p53CpuGpuCaptureRequested = false;
            p53CpuGpuCaptureAvailable = false;
            p53CpuGpuCapturePublicationExact = false;
            p53CpuGpuCapturePhaseExact = false;
            p53CpuGpuCaptureLabel = string.Empty;
            p53CpuGpuCaptureReport = string.Empty;
        }

        private void CaptureP53CpuGpuObstacleParityIfRequested(
            float[] gpuObstacleScalar)
        {
            if (!p53CpuGpuCaptureRequested)
            {
                return;
            }

            p53CpuGpuCaptureRequested = false;
            p53CpuGpuCaptureReport = BuildCpuGpuObstaclePublicationReport(
                obstacleExclusionCells,
                fieldWidth,
                fieldHeight,
                gpuObstacleScalar,
                string.IsNullOrEmpty(p53CpuGpuCaptureLabel)
                    ? "P5.3 CPU-CANDIDATE/GPU-PUBLICATION PARITY"
                    : p53CpuGpuCaptureLabel +
                      " CPU-CANDIDATE/GPU-PUBLICATION PARITY",
                lastConfiguredTopologyEvaluationTime,
                out p53CpuGpuCapturePublicationExact,
                out p53CpuGpuCapturePhaseExact);
            p53CpuGpuCaptureAvailable = true;
        }

        private bool TryConsumeP53CpuGpuObstacleCapture(
            out bool publicationExact,
            out bool phaseExact,
            out string report,
            out string error)
        {
            p53CpuGpuCaptureRequested = false;
            if (!p53CpuGpuCaptureAvailable)
            {
                publicationExact = false;
                phaseExact = false;
                report = string.Empty;
                error =
                    "The explicit cache build completed without publishing " +
                    "the requested pre-release CPU/GPU obstacle snapshot.";
                return false;
            }

            publicationExact = p53CpuGpuCapturePublicationExact;
            phaseExact = p53CpuGpuCapturePhaseExact;
            report = p53CpuGpuCaptureReport;
            error = string.Empty;
            p53CpuGpuCaptureAvailable = false;
            p53CpuGpuCapturePublicationExact = false;
            p53CpuGpuCapturePhaseExact = false;
            p53CpuGpuCaptureLabel = string.Empty;
            p53CpuGpuCaptureReport = string.Empty;
            return true;
        }

        private static string BuildCpuGpuObstaclePublicationReport(
            IReadOnlyList<RiverObstacleExclusionCell> cpuCells,
            int width,
            int height,
            float[] gpuObstacleScalar,
            string label,
            float configuredTopologyEvaluationTime,
            out bool publicationExact,
            out bool phaseExact)
        {
            cpuCells ??= Array.Empty<RiverObstacleExclusionCell>();
            gpuObstacleScalar ??= Array.Empty<float>();
            int cellCount = Mathf.Max(0, width * height);
            bool[] expected = new bool[cellCount];
            int duplicateCoordinates = 0;
            int outOfRangeCoordinates = 0;
            for (int index = 0; index < cpuCells.Count; index++)
            {
                Vector2Int coordinate = cpuCells[index].Coordinate;
                if (coordinate.x < 0 || coordinate.x >= width ||
                    coordinate.y < 0 || coordinate.y >= height)
                {
                    outOfRangeCoordinates++;
                    continue;
                }
                int flat = coordinate.y * width + coordinate.x;
                if (expected[flat])
                {
                    duplicateCoordinates++;
                }
                expected[flat] = true;
            }

            int uniqueCpuCells = 0;
            int gpuAcceptedCells = 0;
            int candidateOnlyCells = 0;
            int gpuOnlyCells = 0;
            int nonBinaryCount = 0;
            string firstCandidateOnly = string.Empty;
            string firstGpuOnly = string.Empty;
            for (int index = 0; index < cellCount; index++)
            {
                bool cpuCandidate = expected[index];
                if (cpuCandidate)
                {
                    uniqueCpuCells++;
                }

                float gpu = index < gpuObstacleScalar.Length
                    ? gpuObstacleScalar[index]
                    : float.NaN;
                int gpuBits = BitConverter.SingleToInt32Bits(gpu);
                bool gpuOccupied = !float.IsNaN(gpu) && gpu >= 0.5f;
                if (gpuOccupied)
                {
                    gpuAcceptedCells++;
                }
                if (!float.IsNaN(gpu) &&
                    gpuBits != BitConverter.SingleToInt32Bits(0f) &&
                    gpuBits != BitConverter.SingleToInt32Bits(1f))
                {
                    nonBinaryCount++;
                }

                if (cpuCandidate && !gpuOccupied)
                {
                    candidateOnlyCells++;
                    if (string.IsNullOrEmpty(firstCandidateOnly))
                    {
                        firstCandidateOnly =
                            $"index={index}, cell=({index % width}," +
                            $"{index / width}), GPU={gpu:R}[0x{gpuBits:X8}]";
                    }
                }
                else if (!cpuCandidate && gpuOccupied)
                {
                    gpuOnlyCells++;
                    if (string.IsNullOrEmpty(firstGpuOnly))
                    {
                        firstGpuOnly =
                            $"index={index}, cell=({index % width}," +
                            $"{index / width}), GPU={gpu:R}[0x{gpuBits:X8}]";
                    }
                }
            }

            int configuredTimeBits = BitConverter.SingleToInt32Bits(
                configuredTopologyEvaluationTime);
            phaseExact =
                TopologyEvaluationPhaseContractVersion == 1 &&
                configuredTimeBits == BitConverter.SingleToInt32Bits(
                    TopologyEvaluationTimeSeconds);
            publicationExact =
                gpuObstacleScalar.Length == cellCount &&
                duplicateCoordinates == 0 &&
                outOfRangeCoordinates == 0 &&
                gpuOnlyCells == 0 &&
                nonBinaryCount == 0;

            StringBuilder builder = new(1536);
            builder.AppendLine(label);
            builder.AppendLine(
                $"Topology phase contract=" +
                $"{TopologyEvaluationPhaseContractVersion}; " +
                $"configuredTime={configuredTopologyEvaluationTime:R}[0x" +
                $"{configuredTimeBits:X8}]; liveMotionIgnored=True");
            builder.AppendLine(
                $"CPU candidates={cpuCells.Count:N0}; unique=" +
                $"{uniqueCpuCells:N0}; duplicates={duplicateCoordinates:N0}; " +
                $"outOfRange={outOfRangeCoordinates:N0}");
            builder.AppendLine(
                $"GPU values={gpuObstacleScalar.Length:N0}; accepted=" +
                $"{gpuAcceptedCells:N0}; nonBinary={nonBinaryCount:N0}; " +
                $"candidateOnly={candidateOnlyCells:N0}; " +
                $"gpuOnly={gpuOnlyCells:N0}");
            builder.AppendLine(
                "Candidate-only cells are valid conservative candidates " +
                "rejected by the canonical water-height interval test.");
            if (!string.IsNullOrEmpty(firstCandidateOnly))
            {
                builder.AppendLine(
                    "First candidate-only cell: " + firstCandidateOnly);
            }
            if (!string.IsNullOrEmpty(firstGpuOnly))
            {
                builder.AppendLine(
                    "First invalid GPU-only cell: " + firstGpuOnly);
            }
            builder.AppendLine(
                "PUBLICATION VERDICT: " +
                (publicationExact ? "PASS" : "FAIL"));
            builder.AppendLine(
                "TOPOLOGY PHASE VERDICT: " +
                (phaseExact ? "PASS" : "FAIL"));
            return builder.ToString();
        }

        private bool FinalizeP53Report(
            StringBuilder report,
            System.Diagnostics.Stopwatch stopwatch,
            bool passed,
            string summary)
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }
            topologyCacheDiagnosticState = passed ? "Passed" : "Failed";
            topologyCacheDiagnosticSummary = summary ?? string.Empty;
            topologyCacheDiagnosticReport = report?.ToString() ?? string.Empty;
            if (!TryWriteLatestDiagnosticReport(
                    "LatestP53ComprehensiveValidation",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string writeError))
            {
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    "The P5.3 report could not be written: " + writeError;
                Debug.LogError(
                    "[River Foam P5.3] " +
                    topologyCacheDiagnosticSummary,
                    river);
                return false;
            }
            if (passed)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log(
                    "[River Foam P5.3] PASS — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            else
            {
                Debug.LogError(
                    "[River Foam P5.3] FAIL — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            return passed;
        }

        private static bool ByteArraysEqual(
            byte[] left,
            byte[] right,
            out int firstDifference)
        {
            left ??= Array.Empty<byte>();
            right ??= Array.Empty<byte>();
            int count = Mathf.Min(left.Length, right.Length);
            for (int index = 0; index < count; index++)
            {
                if (left[index] == right[index])
                {
                    continue;
                }
                firstDifference = index;
                return false;
            }
            if (left.Length != right.Length)
            {
                firstDifference = count;
                return false;
            }
            firstDifference = -1;
            return true;
        }

        private bool FinalizeFailedAudit(
            StringBuilder report,
            System.Diagnostics.Stopwatch stopwatch,
            string summary)
        {
            stopwatch.Stop();
            report.AppendLine();
            report.AppendLine("FINAL VERDICT");
            report.AppendLine("Classification: Audit Inconclusive");
            report.AppendLine("Failure: " + summary);
            report.AppendLine(
                $"Elapsed: {stopwatch.Elapsed.TotalMilliseconds:0.000} ms");
            topologyCacheDiagnosticState = "Failed";
            topologyCacheDiagnosticSummary = summary;
            topologyCacheDiagnosticReport = report.ToString();
            if (!TryWriteLatestDiagnosticReport(
                    "LatestCacheAudit",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string reportWriteError))
            {
                topologyCacheDiagnosticSummary +=
                    " Report output also failed: " + reportWriteError;
            }
            Debug.LogError(
                "[River Foam Cache Diagnostic] " +
                topologyCacheDiagnosticReport,
                river != null ? river : this);
            return false;
        }

        private bool FailCacheDiagnostic(string state, string summary)
        {
            topologyCacheDiagnosticState = state;
            topologyCacheDiagnosticSummary = summary ?? string.Empty;
            topologyCacheDiagnosticReport =
                "RIVER FOAM CACHE DIAGNOSTIC\n" +
                BuildCommonEnvironmentHeader() + "\n" +
                $"State: {state}\nSummary: {summary}\n";
            topologyCacheDiagnosticReportPath = string.Empty;
            Debug.LogWarning(
                "[River Foam Cache Diagnostic] " +
                topologyCacheDiagnosticReport,
                river != null ? river : this);
            return false;
        }

        private void AppendAuditHeader(StringBuilder report)
        {
            report.AppendLine(
                "RIVER FOAM FIXED-METRIC P5.1 DETERMINISM AUDIT");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                "Operation: two independent explicit Edit Mode topology " +
                "preparations; neither artifact is stored.");
            report.AppendLine(
                "Diagnostic contract: source provenance + section digests + " +
                "first-byte evidence.");
            report.AppendLine();
        }

        private string BuildCommonEnvironmentHeader()
        {
            river ??= GetComponent<StylizedRiver>();
            return
                $"UTC: {DateTime.UtcNow:O}\n" +
                $"Unity: {Application.unityVersion}\n" +
                $"Platform: {Application.platform}\n" +
                $"River: {(river != null ? river.name : "<none>")}\n" +
                $"Scene: {(river != null ? river.gameObject.scene.path : "<none>")}\n" +
                $"Hierarchy: {(river != null ? BuildTransformPath(river.transform) : "<none>")}";
        }

        private static bool AssignedCacheMetadataMatchesArtifact(
            StylizedRiverFoamTopologyCacheAsset asset,
            StylizedRiverFoamTopologyCacheBuildArtifact artifact,
            out string comparison)
        {
            if (asset == null)
            {
                comparison = "The assigned cache asset is missing.";
                return false;
            }

            StringBuilder builder = new(1024);
            bool exact = true;
            AppendMetadataComparison(
                builder,
                "Payload Format",
                asset.PayloadFormatVersion.ToString(),
                artifact.FormatVersion.ToString(),
                ref exact);
            AppendMetadataComparison(
                builder,
                "Generator Contract",
                asset.GeneratorContractVersion.ToString(),
                artifact.GeneratorContractVersion.ToString(),
                ref exact);
            AppendMetadataComparison(
                builder,
                "Descriptor Contract",
                asset.GridDescriptorContractVersion.ToString(),
                artifact.GridDescriptorContractVersion.ToString(),
                ref exact);
            AppendMetadataComparison(
                builder,
                "Grid Mapping",
                asset.GridMappingValue.ToString(),
                artifact.GridMappingValue.ToString(),
                ref exact);
            AppendMetadataComparison(
                builder,
                "Mapping Contract",
                asset.GridMappingContractVersion.ToString(),
                artifact.GridMappingContractVersion.ToString(),
                ref exact);
            AppendMetadataComparison(
                builder,
                "Grid Signature",
                asset.GridInitializationSignature,
                artifact.GridInitializationSignature,
                ref exact);
            AppendMetadataComparison(
                builder,
                "Source River",
                asset.SourceRiverName,
                artifact.SourceRiverName,
                ref exact);
            AppendMetadataComparison(
                builder,
                "Payload Bytes",
                asset.PayloadByteCount.ToString(),
                artifact.PayloadByteCount.ToString(),
                ref exact);
            AppendMetadataComparison(
                builder,
                "Payload Hash",
                asset.PayloadHash,
                artifact.PayloadHash,
                ref exact);
            AppendMetadataComparison(
                builder,
                "Domain Fingerprint",
                asset.DomainFingerprint,
                artifact.DomainFingerprint,
                ref exact);
            AppendMetadataComparison(
                builder,
                "Obstacle Fingerprint",
                asset.ObstacleFingerprint,
                artifact.ObstacleFingerprint,
                ref exact);
            AppendMetadataComparison(
                builder,
                "Generation Fingerprint",
                asset.GenerationFingerprint,
                artifact.GenerationFingerprint,
                ref exact);
            AppendMetadataComparison(
                builder,
                "Combined Fingerprint",
                asset.CombinedFingerprint,
                artifact.CombinedFingerprint,
                ref exact);
            comparison = builder.ToString();
            return exact;
        }

        private static void AppendMetadataComparison(
            StringBuilder builder,
            string label,
            string assigned,
            string generated,
            ref bool exact)
        {
            bool valueExact = string.Equals(
                assigned ?? string.Empty,
                generated ?? string.Empty,
                StringComparison.Ordinal);
            exact &= valueExact;
            builder.AppendLine(
                $"{label}: {(valueExact ? "EXACT" : "DIFFERENT")}; " +
                $"assigned={assigned ?? string.Empty}; " +
                $"generated={generated ?? string.Empty}");
        }

        private static string BuildAssignedCacheMetadataSignature(
            StylizedRiverFoamTopologyCacheAsset asset)
        {
            if (asset == null)
            {
                return "<none>";
            }

            return
                $"storage={asset.StorageContractVersionValue};" +
                $"payload={asset.PayloadFormatVersion};" +
                $"generator={asset.GeneratorContractVersion};" +
                $"descriptor={asset.GridDescriptorContractVersion};" +
                $"mapping={asset.GridMappingValue};" +
                $"mappingContract={asset.GridMappingContractVersion};" +
                $"signature={asset.GridInitializationSignature};" +
                $"source={asset.SourceRiverName};" +
                $"built={asset.BuiltUtc};" +
                $"bytes={asset.PayloadByteCount};" +
                $"payloadHash={asset.PayloadHash};" +
                $"domain={asset.DomainFingerprint};" +
                $"obstacles={asset.ObstacleFingerprint};" +
                $"generation={asset.GenerationFingerprint};" +
                $"combined={asset.CombinedFingerprint}";
        }

        private void AppendAssignedCacheMetadata(
            StringBuilder report,
            StylizedRiverFoamTopologyCacheAsset asset)
        {
            report.AppendLine("ASSIGNED CACHE METADATA");
            report.AppendLine($"Asset: {asset.name}");
            report.AppendLine(
                $"Storage / payload / generator: " +
                $"{asset.StorageContractVersionValue} / " +
                $"{asset.PayloadFormatVersion} / " +
                $"{asset.GeneratorContractVersion}");
            report.AppendLine(
                $"Grid descriptor / mapping / mapping contract: " +
                $"{asset.GridDescriptorContractVersion} / " +
                $"{asset.GridMappingDisplayName} / " +
                $"{asset.GridMappingContractVersion}");
            report.AppendLine(
                $"Grid signature: {asset.GridInitializationSignature}");
            report.AppendLine(
                $"Source river / built UTC: " +
                $"{asset.SourceRiverName} / {asset.BuiltUtc}");
            report.AppendLine(
                $"Payload: {asset.PayloadByteCount:N0} bytes / " +
                $"{asset.PayloadHash}");
            report.AppendLine(
                $"Domain fingerprint: {asset.DomainFingerprint}");
            report.AppendLine(
                $"Obstacle fingerprint: {asset.ObstacleFingerprint}");
            report.AppendLine(
                $"Generation fingerprint: {asset.GenerationFingerprint}");
            report.AppendLine(
                $"Combined fingerprint: {asset.CombinedFingerprint}");
        }

        private static void AppendBuildArtifact(
            StringBuilder report,
            string heading,
            StylizedRiverFoamTopologyCacheBuildArtifact artifact,
            StylizedRiverFoamCacheDiagnosticSnapshot snapshot)
        {
            report.AppendLine(heading);
            report.AppendLine(
                $"Payload: {artifact.PayloadByteCount:N0} bytes / " +
                artifact.PayloadHash);
            report.AppendLine(
                $"Format / generator: {artifact.FormatVersion} / " +
                artifact.GeneratorContractVersion);
            report.AppendLine(
                $"Descriptor: v{artifact.GridDescriptorContractVersion} / " +
                $"mapping-{artifact.GridMappingValue}-v" +
                $"{artifact.GridMappingContractVersion} / " +
                artifact.GridInitializationSignature);
            report.AppendLine(
                $"Domain fingerprint: {artifact.DomainFingerprint}");
            report.AppendLine(
                $"Obstacle fingerprint: {artifact.ObstacleFingerprint}");
            report.AppendLine(
                $"Generation fingerprint: {artifact.GenerationFingerprint}");
            report.AppendLine(
                $"Combined fingerprint: {artifact.CombinedFingerprint}");
            report.AppendLine(
                $"Build milliseconds: {artifact.BuildMilliseconds:0.000}");
            report.AppendLine(snapshot.TopologySummary);
            report.AppendLine("Section digests:");
            for (int index = 0; index < snapshot.Sections.Count; index++)
            {
                StylizedRiverFoamCacheDiagnosticSection section =
                    snapshot.Sections[index];
                report.AppendLine(
                    $"  {section.Name}: {section.ByteCount:N0} bytes / " +
                    $"{section.Hash:X16} / {section.Detail}");
            }
            report.AppendLine();
        }

        private static bool AllProvidersAgreeWithDirect(
            StylizedRiverFoamObstacleDiagnosticSnapshot snapshot)
        {
            for (int index = 0; index < snapshot.Sources.Count; index++)
            {
                StylizedRiverFoamObstacleSourceDiagnostic source =
                    snapshot.Sources[index];
                if (!source.ProviderAvailable ||
                    !source.DirectWorldAvailable ||
                    !source.ProviderMatchesDirect)
                {
                    return false;
                }
            }
            return true;
        }

        private static string ResolveAuditClassification(
            bool assignedAssetUnchanged,
            bool obstacleStable,
            bool providerDirectAgreement,
            bool buildObstacleCaptureAgreement,
            bool inputFingerprintsExact,
            bool buildExact,
            StylizedRiverFoamCacheDiagnosticSnapshot left,
            StylizedRiverFoamCacheDiagnosticSnapshot right)
        {
            if (!assignedAssetUnchanged)
            {
                return "Audit Inconclusive";
            }
            if (!obstacleStable)
            {
                return "Obstacle Input Drift";
            }
            if (!providerDirectAgreement ||
                !buildObstacleCaptureAgreement)
            {
                return "Input Fingerprint Gap";
            }
            if (!inputFingerprintsExact)
            {
                return "Input Fingerprint Gap";
            }
            if (buildExact)
            {
                return "Exact";
            }

            bool anySectionChanged = false;
            for (int index = 0; index < left.Sections.Count; index++)
            {
                StylizedRiverFoamCacheDiagnosticSection section =
                    left.Sections[index];
                if (!right.TryGetSection(section.Name, out var other) ||
                    section.ByteCount != other.ByteCount ||
                    section.Hash != other.Hash)
                {
                    anySectionChanged = true;
                    break;
                }
            }
            return anySectionChanged
                ? "Topology Generation Drift"
                : "Serialization Drift";
        }

        private bool TryWriteLatestDiagnosticReport(
            string suffix,
            string report,
            out string path,
            out string error)
        {
            path = string.Empty;
            error = string.Empty;
            try
            {
                string resolvedPath = ResolveDiagnosticPath(suffix, ".txt");
                string directory = Path.GetDirectoryName(resolvedPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(
                    resolvedPath,
                    report ?? string.Empty,
                    Encoding.UTF8);
                path = resolvedPath;
                return true;
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is ArgumentException)
            {
                error = exception.Message;
                return false;
            }
        }

        private string ResolveDiagnosticPath(string suffix, string extension)
        {
            river ??= GetComponent<StylizedRiver>();
            string riverName =
                StylizedRiverFoamCacheDiagnosticUtility.SanitizeFileName(
                    river != null ? river.name : name);
            string directory = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "../Library/RiverFoamDiagnostics"));
            return Path.Combine(
                directory,
                $"{riverName}_{suffix}{extension}");
        }

        private static string BuildTransformPath(Transform transform)
        {
            if (transform == null)
            {
                return "<null>";
            }
            System.Collections.Generic.List<string> parts = new();
            Transform current = transform;
            while (current != null)
            {
                parts.Add($"{current.name}[{current.GetSiblingIndex()}]");
                current = current.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
#endif
