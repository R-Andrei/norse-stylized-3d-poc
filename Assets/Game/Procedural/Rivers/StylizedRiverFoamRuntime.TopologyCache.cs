using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private void BeginTopologyStartupValidation()
        {
            if (topologyStartupValidationActive)
            {
                return;
            }

            topologyStartupValidationActive = true;
            topologyStartupValidationComplete = false;
            topologyStartupValidationStartedAt =
                Time.realtimeSinceStartupAsDouble;
            topologyStartupValidationTotalMilliseconds = 0.0;
            topologyStartupValidationSlowestStepMilliseconds = 0.0;
            topologyStartupValidationSlowestStep = "—";
            topologyStartupValidationStepCount = 0;
            topologyStartupValidationCacheInstallCount = 0;
            topologyStartupValidationObstacleBuildCount = 0;
            topologyStartupValidationMajorBuildCount = 0;
            topologyStartupValidationConnectorBuildCount = 0;
            topologyStartupValidationPocketBuildCount = 0;
            int phaseCount = Enum.GetValues(typeof(InitializationPhase)).Length;
            if (topologyStartupPhaseCallCounts.Length != phaseCount)
            {
                topologyStartupPhaseCallCounts = new int[phaseCount];
                topologyStartupPhaseAccumulatedMilliseconds =
                    new double[phaseCount];
            }
            else
            {
                Array.Clear(
                    topologyStartupPhaseCallCounts,
                    0,
                    topologyStartupPhaseCallCounts.Length);
                Array.Clear(
                    topologyStartupPhaseAccumulatedMilliseconds,
                    0,
                    topologyStartupPhaseAccumulatedMilliseconds.Length);
            }
            topologyStartupSummaryLogged = false;
        }

        private void RecordTopologyStartupStep(
            InitializationPhase phase,
            double elapsedMilliseconds)
        {
            if (!topologyStartupValidationActive)
            {
                return;
            }

            topologyStartupValidationStepCount++;
            int phaseIndex = (int)phase;
            if (phaseIndex >= 0 &&
                phaseIndex < topologyStartupPhaseCallCounts.Length)
            {
                topologyStartupPhaseCallCounts[phaseIndex]++;
                topologyStartupPhaseAccumulatedMilliseconds[phaseIndex] +=
                    elapsedMilliseconds;
            }
            if (elapsedMilliseconds >
                topologyStartupValidationSlowestStepMilliseconds)
            {
                topologyStartupValidationSlowestStepMilliseconds =
                    elapsedMilliseconds;
                topologyStartupValidationSlowestStep = phase.ToString();
            }
        }

        private void CompleteTopologyStartupValidation()
        {
            if (!topologyStartupValidationActive)
            {
                return;
            }

            topologyStartupValidationTotalMilliseconds = Math.Max(
                0.0,
                (Time.realtimeSinceStartupAsDouble -
                 topologyStartupValidationStartedAt) * 1000.0);
            topologyStartupValidationActive = false;
            topologyStartupValidationComplete = true;
            if (!editorTopologyPreparationInProgress)
            {
                LogTopologyStartupSummaryOnce();
            }
        }

        private string BuildTopologyStartupPhaseSummary()
        {
            if (topologyStartupPhaseCallCounts.Length == 0)
            {
                return "—";
            }

            var parts = new List<string>();
            for (int index = 0;
                 index < topologyStartupPhaseCallCounts.Length;
                 index++)
            {
                int count = topologyStartupPhaseCallCounts[index];
                if (count <= 0)
                {
                    continue;
                }

                parts.Add(
                    $"{(InitializationPhase)index} {count}x/" +
                    $"{topologyStartupPhaseAccumulatedMilliseconds[index]:0.000}ms");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "—";
        }

        private void LogTopologyStartupSummaryOnce()
        {
            if (topologyStartupSummaryLogged || river == null)
            {
                return;
            }

            topologyStartupSummaryLogged = true;
            Debug.Log(
                $"[River Foam Cache] Startup '{river.name}': " +
                $"outcome={topologyCacheStartupOutcome}, " +
                $"reasons={TopologyCacheStartupReasonNames}, " +
                $"total={topologyStartupValidationTotalMilliseconds:0.000}ms, " +
                $"field={fieldWidth}x{fieldHeight}, " +
                $"cells={(long)fieldWidth * fieldHeight:N0}, " +
                $"cache=attempt:{topologyCacheStartupAttemptCount}/" +
                $"hit:{topologyCacheStartupHitCount}/" +
                $"miss:{topologyCacheStartupMissCount}/" +
                $"install:{topologyStartupValidationCacheInstallCount}/" +
                $"build:{topologyStartupCacheBuildAttemptCount}/" +
                $"load:{topologyCacheStartupLoadMilliseconds:0.000}ms, " +
                $"builds=obstacle:{topologyStartupValidationObstacleBuildCount}/" +
                $"major:{topologyStartupValidationMajorBuildCount}/" +
                $"connector:{topologyStartupValidationConnectorBuildCount}/" +
                $"pocket:{topologyStartupValidationPocketBuildCount}, " +
                $"replacementAttempts={topologyStartupReplacementAttemptCount}, " +
                $"writes=attempt:{topologyStartupCacheWriteAttemptCount}/" +
                $"success:{topologyStartupCacheWriteSuccessCount}, " +
                $"registry=add:{topologyStartupSourceAddedCount}/" +
                $"remove:{topologyStartupSourceRemovedCount}/" +
                $"change:{topologyStartupSourceChangedCount}/" +
                $"distinct:{topologyStartupDistinctGeneratedSources.Count}, " +
                $"dirtyCycles={topologyStartupDirtyCycleCount}, " +
                $"dirtyReasons={TopologyStartupDirtyReasonNames}, " +
                $"restarts={topologyStartupRestartCount}, " +
                $"phases=[{BuildTopologyStartupPhaseSummary()}], " +
                $"slowest={topologyStartupValidationSlowestStep}/" +
                $"{topologyStartupValidationSlowestStepMilliseconds:0.000}ms.",
                river);
        }

        private void MarkActiveTopologyCacheStale(
            string state,
            string summary)
        {
            topologyCacheStartupState = state ?? "Stale";
            topologyCacheStartupSummary = summary ?? string.Empty;
        }

        public bool RequestIdenticalTopologyReplacementValidation()
        {
            if (!topologyStartupValidationComplete)
            {
                topologyStartupReplacementAttemptCount++;
            }
            topologyReplacementLastReason =
                "Unavailable — Play Mode topology generation is disabled";
            return false;
        }

        public bool RunTopologyCacheRoundTripValidation()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            topologyCacheRoundTripRunCount++;
            topologyCacheRoundTripState = "Running";
            topologyCacheRoundTripSummary =
                "Running the explicit exhaustive payload proof.";

            river ??= GetComponent<StylizedRiver>();
            if (river == null)
            {
                topologyCacheRoundTripState = "Unavailable";
                topologyCacheRoundTripSummary =
                    "The proof requires a StylizedRiver owner.";
                return false;
            }

            try
            {
                StylizedRiverFoamTopologyCachePackage package;
                int assignedPayloadBytes = 0;
                ulong assignedPayloadHash = 0ul;

                if (Application.isPlaying)
                {
                    if (!river.Domain.IsValid ||
                        !TopologyCacheRoundTripReady ||
                        TopologyReplacementInProgress)
                    {
                        topologyCacheRoundTripState = "Unavailable";
                        topologyCacheRoundTripSummary =
                            "Play Mode proof requires one fully initialized " +
                            "topology with no replacement build in progress.";
                        return false;
                    }

                    if (!TryCaptureTopologyCacheFingerprints(
                            out StylizedRiverFoamTopologyFingerprint
                                domainFingerprint,
                            out StylizedRiverFoamTopologyFingerprint
                                obstacleFingerprint,
                            out StylizedRiverFoamTopologyFingerprint
                                generationFingerprint,
                            out _,
                            out string fingerprintError))
                    {
                        topologyCacheRoundTripState = "Failed";
                        topologyCacheRoundTripSummary = fingerprintError;
                        return false;
                    }

                    package = CreateTopologyCachePackage(
                        domainFingerprint,
                        obstacleFingerprint,
                        generationFingerprint);
                }
                else
                {
                    StylizedRiverFoamTopologyCacheAsset asset =
                        river.FoamTopologyCacheAsset;
                    if (asset == null || !asset.HasSupportedStorageContract ||
                        !asset.HasPayload)
                    {
                        topologyCacheRoundTripState = "Unavailable";
                        topologyCacheRoundTripSummary =
                            "Assign and prepare a supported non-empty cache " +
                            "before running the exhaustive integrity proof.";
                        return false;
                    }

                    byte[] assignedPayload = asset.GetPayloadReadOnlyReference();
                    assignedPayloadBytes = assignedPayload.Length;
                    assignedPayloadHash =
                        StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(
                            assignedPayload);
                    if (!StylizedRiverFoamTopologyCacheCodec.TryDeserialize(
                            assignedPayload,
                            out package,
                            out string loadError))
                    {
                        topologyCacheRoundTripState = "Failed";
                        topologyCacheRoundTripSummary =
                            $"Assigned payload load failed: {loadError}";
                        return false;
                    }
                }

                StylizedRiverFoamTopologyCacheRoundTripResult result =
                    StylizedRiverFoamTopologyCacheCodec.ValidateRoundTrip(
                        package);
                bool matchesAssignedPayload = assignedPayloadBytes == 0 ||
                    (result.PayloadByteCount == assignedPayloadBytes &&
                     result.PayloadHash == assignedPayloadHash);

                topologyCacheRoundTripPayloadBytes = result.PayloadByteCount;
                topologyCacheRoundTripPayloadHash = result.PayloadHash;
                topologyCacheRoundTripSerializationMilliseconds =
                    result.SerializationMilliseconds;
                topologyCacheRoundTripLoadMilliseconds =
                    result.LoadMilliseconds;
                topologyCacheRoundTripVerificationMilliseconds =
                    result.VerificationMilliseconds;
                topologyCacheRoundTripState = result.Passed &&
                    matchesAssignedPayload
                        ? "Passed"
                        : "Failed";
                topologyCacheRoundTripSummary = !result.Passed
                    ? result.Summary
                    : !matchesAssignedPayload
                        ? "The assigned payload did not reproduce the exact " +
                          "validated payload size and checksum."
                        : result.Summary;

                bool passed = result.Passed && matchesAssignedPayload;
                if (passed)
                {
                    topologyCacheRoundTripPassCount++;
                    Debug.Log(
                        $"[River Foam Cache Proof] Passed for " +
                        $"'{river.name}': {result.PayloadByteCount:N0} bytes, " +
                        $"hash {result.PayloadHash:X16}.",
                        river);
                }
                else
                {
                    Debug.LogError(
                        $"[River Foam Cache Proof] Failed for " +
                        $"'{river.name}': {topologyCacheRoundTripSummary}",
                        river);
                }

                return passed;
            }
            catch (Exception exception)
            {
                topologyCacheRoundTripState = "Failed";
                topologyCacheRoundTripSummary = exception.Message;
                Debug.LogException(exception, river);
                return false;
            }
#else
            return false;
#endif
        }

        public bool TryBuildTopologyCache(
            out StylizedRiverFoamTopologyCacheBuildArtifact artifact)
        {
            artifact = default;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            topologyCacheBuildCount++;
            topologyCacheLastBuildSerializationCount = 0;
            if (topologyStartupValidationActive &&
                !editorTopologyPreparationInProgress)
            {
                topologyStartupCacheBuildAttemptCount++;
            }
            topologyCacheBuildState = "Building";
            topologyCacheBuildSummary =
                "Capturing stable inputs and serializing one complete payload.";

            if ((!Application.isPlaying &&
                 !editorTopologyPreparationInProgress) ||
                !TopologyCacheBuildReady ||
                river == null ||
                !river.Domain.IsValid ||
                TopologyReplacementInProgress)
            {
                topologyCacheBuildState = "Unavailable";
                topologyCacheBuildSummary =
                    "Cache building requires a fully initialized topology " +
                    "from Play Mode or the explicit Edit Mode preparation action, " +
                    "with no maintenance or replacement work in progress.";
                return false;
            }

            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (!TryCaptureTopologyCacheFingerprints(
                        out StylizedRiverFoamTopologyFingerprint
                            domainFingerprint,
                        out StylizedRiverFoamTopologyFingerprint
                            obstacleFingerprint,
                        out StylizedRiverFoamTopologyFingerprint
                            generationFingerprint,
                        out StylizedRiverFoamTopologyFingerprint
                            combinedFingerprint,
                        out string fingerprintError))
                {
                    stopwatch.Stop();
                    topologyCacheBuildState = "Failed";
                    topologyCacheBuildSummary = fingerprintError;
                    topologyCacheBuildMilliseconds =
                        stopwatch.Elapsed.TotalMilliseconds;
                    return false;
                }

                StylizedRiverFoamTopologyCachePackage package =
                    CreateTopologyCachePackage(
                        domainFingerprint,
                        obstacleFingerprint,
                        generationFingerprint);
#if UNITY_EDITOR
                CaptureP53CpuGpuObstacleParityIfRequested(
                    package.ObstacleExclusion);
#endif
                byte[] payload =
                    StylizedRiverFoamTopologyCacheCodec.Serialize(package);
                topologyCacheLastBuildSerializationCount++;
                ulong payloadHash =
                    StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(
                        payload);
                if (payload.Length < sizeof(ulong))
                {
                    stopwatch.Stop();
                    topologyCacheBuildState = "Failed";
                    topologyCacheBuildSummary =
                        "The serialized cache payload did not contain a valid " +
                        "checksum contract.";
                    topologyCacheBuildMilliseconds =
                        stopwatch.Elapsed.TotalMilliseconds;
                    return false;
                }

                stopwatch.Stop();
                topologyCacheBuildPayloadBytes = payload.Length;
                topologyCacheBuildPayloadHash = payloadHash.ToString("X16");
                topologyCacheBuildMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
                topologyCacheBuildSuccessCount++;
                topologyCacheBuildState = "Built";
                topologyCacheBuildSummary =
                    "A complete versioned payload was validated and serialized " +
                    "once from the active prepared topology. Exhaustive " +
                    "round-trip and corruption proof remains an explicit action.";

                artifact = new StylizedRiverFoamTopologyCacheBuildArtifact(
                    payload,
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion,
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion,
                    gridDescriptor,
                    domainFingerprint.ToString(),
                    obstacleFingerprint.ToString(),
                    generationFingerprint.ToString(),
                    combinedFingerprint.ToString(),
                    payloadHash.ToString("X16"),
                    topologyCacheBuildMilliseconds,
                    river.name);

                Debug.Log(
                    $"[River Foam Cache] Built payload for " +
                    $"'{river.name}': {payload.Length:N0} bytes, hash " +
                    $"{payloadHash:X16}, inputs {combinedFingerprint}.",
                    river);
                return true;
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                topologyCacheBuildState = "Failed";
                topologyCacheBuildSummary = exception.Message;
                topologyCacheBuildMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
                Debug.LogException(exception, river);
                return false;
            }
#else
            topologyCacheBuildState = "Unavailable";
            topologyCacheBuildSummary =
                "Explicit cache building is available only in the Editor or " +
                "a Development Build.";
            return false;
#endif
        }

        /// <summary>
        /// Validates the assigned persistent topology cache against the exact
        /// current authored inputs without activating Foam or allocating its GPU
        /// simulation field. Release build preflight uses this strict path; it
        /// never falls back to mesh scanning, generation, asset creation, or
        /// cache mutation.
        /// </summary>
        public bool TryValidateAssignedTopologyCacheForRelease(
            out string state,
            out string summary,
            out int payloadBytes,
            out string payloadHash,
            out int obstacleSourceCount)
        {
            state = "Invalid";
            summary = string.Empty;
            payloadBytes = 0;
            payloadHash = "—";
            obstacleSourceCount = 0;

            river ??= GetComponent<StylizedRiver>();
            if (river == null)
            {
                summary = "The Foam runtime has no StylizedRiver owner.";
                return false;
            }

            if (!river.Domain.IsValid)
            {
                state = "Inputs Unavailable";
                summary =
                    "The authored river does not currently expose a valid resampled domain.";
                return false;
            }

            StylizedRiverFoamTopologyCacheAsset asset =
                river.FoamTopologyCacheAsset;
            if (asset == null)
            {
                state = "Unassigned";
                summary = "No persistent Foam topology cache is assigned.";
                return false;
            }

            if (!asset.HasSupportedStorageContract)
            {
                state = "Unsupported Storage";
                summary =
                    "The assigned asset uses an unsupported storage contract.";
                return false;
            }

            if (!asset.HasPayload)
            {
                state = "Empty";
                summary = "The assigned cache asset contains no payload bytes.";
                return false;
            }

            if (!asset.TryValidateStoredContract(
                    out bool legacyCoordinateContract,
                    out bool legacyObstacleFingerprintContract,
                    out bool legacyDynamicTopologyPhaseContract,
                    out string contractSummary))
            {
                state = legacyCoordinateContract
                    ? "Legacy Coordinate Contract"
                    : legacyObstacleFingerprintContract
                        ? "Legacy Obstacle Fingerprint Contract"
                        : legacyDynamicTopologyPhaseContract
                            ? "Legacy Dynamic Topology Phase"
                            : "Unsupported Cache Contract";
                summary = contractSummary;
                return false;
            }

            byte[] payload = asset.GetPayloadReadOnlyReference();
            payloadBytes = payload.Length;
            if (!StylizedRiverFoamTopologyCacheCodec.TryDeserialize(
                    payload,
                    out StylizedRiverFoamTopologyCachePackage package,
                    out string loadError))
            {
                state = "Invalid Payload";
                summary = loadError;
                return false;
            }

            ulong resolvedPayloadHash =
                StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(payload);
            payloadHash = resolvedPayloadHash.ToString("X16");
            StylizedRiverFoamTopologyFingerprint storedCombined =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    package.DomainFingerprint,
                    package.ObstacleFingerprint,
                    package.GenerationFingerprint);
            bool metadataMatches =
                asset.PayloadFormatVersion ==
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion &&
                asset.GeneratorContractVersion ==
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion &&
                asset.GridDescriptorContractVersion ==
                    StylizedRiverFoamGridDescriptor
                        .DescriptorContractVersion &&
                asset.GridMappingValue ==
                    (int)package.GridDescriptor.Mapping &&
                asset.GridMappingContractVersion ==
                    package.GridDescriptor.MappingContractVersion &&
                asset.GridInitializationSignature ==
                    package.GridDescriptor.InitializationSignature
                        .ToString("X16") &&
                asset.PayloadHash == payloadHash &&
                asset.DomainFingerprint ==
                    package.DomainFingerprint.ToString() &&
                asset.ObstacleFingerprint ==
                    package.ObstacleFingerprint.ToString() &&
                asset.GenerationFingerprint ==
                    package.GenerationFingerprint.ToString() &&
                asset.CombinedFingerprint == storedCombined.ToString();
            if (!metadataMatches)
            {
                state = "Metadata Mismatch";
                summary =
                    "The cache asset metadata does not match its validated binary payload.";
                return false;
            }

            bool completeContract =
                package.FieldWidth >= 2 &&
                package.FieldHeight >= 2 &&
                package.FieldLength > 0f &&
                package.ValidFieldLength > 0f &&
                package.ObstacleExclusion != null &&
                package.ObstacleExclusion.Length ==
                    package.FieldWidth * package.FieldHeight &&
                package.MajorTopology != null &&
                package.ConnectorTopology != null &&
                package.PocketTopology != null;
            if (!completeContract)
            {
                state = "Incomplete Contract";
                summary =
                    "The payload does not contain one complete prepared topology graph and obstacle field.";
                return false;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (disturbanceRuntime != null &&
                !disturbanceRuntime
                    .PrepareGeneratedGeometrySourcesForCacheValidation(
                        out string preparationStatus))
            {
                state = "Obstacle Inputs Unavailable";
                summary = preparationStatus;
                return false;
            }

            topologyCachePreparedObstacleFingerprints.Clear();
            StylizedRiverFoamTopologyFingerprint currentObstacles = default;
            string obstacleStatus = string.Empty;
            bool obstacleInputsAvailable;
            if (disturbanceRuntime == null)
            {
                obstacleInputsAvailable = RiverObstacleExclusionResolver
                    .TryCombineStableSourceFingerprints(
                        topologyCachePreparedObstacleFingerprints,
                        out currentObstacles,
                        out obstacleSourceCount,
                        out obstacleStatus);
            }
            else
            {
                obstacleInputsAvailable = disturbanceRuntime
                    .TryCopyObstacleExclusionStableFingerprintsTo(
                        topologyCachePreparedObstacleFingerprints,
                        out obstacleSourceCount,
                        out obstacleStatus,
                        verifyDirectGeometry: !Application.isPlaying) &&
                    RiverObstacleExclusionResolver
                        .TryCombineStableSourceFingerprints(
                            topologyCachePreparedObstacleFingerprints,
                            out currentObstacles,
                            out obstacleSourceCount,
                            out obstacleStatus);
            }
            if (!obstacleInputsAvailable)
            {
                state = "Obstacle Inputs Unavailable";
                summary = obstacleStatus;
                return false;
            }

            StylizedRiverFoamTopologyFingerprint currentDomain =
                StylizedRiverFoamTopologyFingerprints.ComputeDomain(
                    river.Domain);
            int preservedChunkCount = chunkCount;
            bool descriptorResolved = TryResolveLegacyInitializationDescriptor(
                river.Domain,
                SystemInfo.maxTextureSize,
                out StylizedRiverFoamGridDescriptor currentDescriptor);
            chunkCount = preservedChunkCount;
            if (!descriptorResolved)
            {
                state = "Grid Inputs Unavailable";
                summary =
                    "The current authored river cannot resolve its exact Foam " +
                    "grid descriptor without changing physical scale.";
                return false;
            }
            if (!TryValidateTopologyCacheGridDescriptor(
                    currentDescriptor,
                    out string gridError))
            {
                state = "Grid Contract Unsupported";
                summary = gridError;
                return false;
            }

            StylizedRiverFoamTopologyFingerprint currentGeneration =
                StylizedRiverFoamTopologyFingerprints.ComputeGeneration(
                    river,
                    currentDescriptor);
            StylizedRiverFoamTopologyFingerprint currentCombined =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    currentDomain,
                    currentObstacles,
                    currentGeneration);

            if (package.DomainFingerprint != currentDomain)
            {
                state = "Stale Domain";
                summary =
                    "The complete resampled river-domain content changed.";
                return false;
            }

            if (package.ObstacleFingerprint != currentObstacles)
            {
                state = "Stale Obstacles";
                summary =
                    "The exact transformed static obstacle-source geometry changed.";
                return false;
            }

            if (package.GridDescriptor != currentDescriptor)
            {
                state = "Stale Grid Contract";
                summary =
                    "The cached Foam coordinate descriptor does not match the " +
                    "current authored allocation and mapping contract.";
                return false;
            }

            if (package.GenerationFingerprint != currentGeneration)
            {
                state = "Stale Settings";
                summary =
                    "Quality, field mapping, or a topology-generation setting changed.";
                return false;
            }

            if (asset.CombinedFingerprint != currentCombined.ToString())
            {
                state = "Combined Key Mismatch";
                summary =
                    "The assigned cache does not match the current combined stable input key.";
                return false;
            }

            state = "Valid";
            summary =
                "The complete payload, metadata, domain, exact obstacle sources, and generation inputs are release-ready.";
            return true;
        }

        public bool ValidateAssignedTopologyCache()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            topologyCacheValidationCount++;
            topologyCacheValidationState = "Evaluating";
            topologyCacheValidationSummary =
                "Comparing the assigned payload with current stable inputs.";

            if (!Application.isPlaying ||
                !TopologyCacheBuildReady ||
                river == null ||
                !river.Domain.IsValid ||
                TopologyReplacementInProgress)
            {
                SetTopologyCacheValidationMiss(
                    "Unavailable",
                    "Validation requires a fully initialized Play-mode " +
                    "topology whose maintenance and replacement work is complete.");
                return false;
            }

            StylizedRiverFoamTopologyCacheAsset asset =
                river.FoamTopologyCacheAsset;
            if (asset == null)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Unassigned",
                    "No Foam topology cache asset is assigned to this river.");
                return false;
            }

            if (!asset.HasSupportedStorageContract)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Storage Version",
                    "The assigned asset uses an unsupported storage contract.");
                return false;
            }

            if (!asset.HasPayload)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Empty",
                    "The assigned cache asset contains no payload bytes.");
                return false;
            }

            if (!asset.TryValidateStoredContract(
                    out bool legacyCoordinateContract,
                    out bool legacyObstacleFingerprintContract,
                    out bool legacyDynamicTopologyPhaseContract,
                    out string contractSummary))
            {
                SetTopologyCacheValidationMiss(
                    legacyCoordinateContract
                        ? "Miss — Legacy Coordinate Contract"
                        : legacyObstacleFingerprintContract
                            ? "Miss — Legacy Obstacle Fingerprint Contract"
                            : legacyDynamicTopologyPhaseContract
                                ? "Miss — Legacy Dynamic Topology Phase"
                                : "Miss — Unsupported Cache Contract",
                    contractSummary);
                return false;
            }

            if (!TryCaptureTopologyCacheFingerprints(
                    out StylizedRiverFoamTopologyFingerprint
                        currentDomain,
                    out StylizedRiverFoamTopologyFingerprint
                        currentObstacles,
                    out StylizedRiverFoamTopologyFingerprint
                        currentGeneration,
                    out StylizedRiverFoamTopologyFingerprint
                        currentCombined,
                    out string fingerprintError))
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Inputs Unavailable",
                    fingerprintError);
                return false;
            }

            byte[] payload = asset.GetPayloadReadOnlyReference();
            if (!StylizedRiverFoamTopologyCacheCodec.TryDeserialize(
                    payload,
                    out StylizedRiverFoamTopologyCachePackage package,
                    out string loadError))
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Invalid Payload",
                    loadError);
                return false;
            }

            ulong payloadHash =
                StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(payload);
            StylizedRiverFoamTopologyFingerprint storedCombined =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    package.DomainFingerprint,
                    package.ObstacleFingerprint,
                    package.GenerationFingerprint);
            bool metadataMatches =
                asset.PayloadFormatVersion ==
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion &&
                asset.GeneratorContractVersion ==
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion &&
                asset.GridDescriptorContractVersion ==
                    StylizedRiverFoamGridDescriptor
                        .DescriptorContractVersion &&
                asset.GridMappingValue ==
                    (int)package.GridDescriptor.Mapping &&
                asset.GridMappingContractVersion ==
                    package.GridDescriptor.MappingContractVersion &&
                asset.GridInitializationSignature ==
                    package.GridDescriptor.InitializationSignature
                        .ToString("X16") &&
                asset.PayloadHash == payloadHash.ToString("X16") &&
                asset.DomainFingerprint ==
                    package.DomainFingerprint.ToString() &&
                asset.ObstacleFingerprint ==
                    package.ObstacleFingerprint.ToString() &&
                asset.GenerationFingerprint ==
                    package.GenerationFingerprint.ToString() &&
                asset.CombinedFingerprint == storedCombined.ToString();
            if (!metadataMatches)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Metadata Mismatch",
                    "The asset metadata does not match its validated binary " +
                    "payload. Rebuild the cache asset explicitly.");
                return false;
            }

            if (package.DomainFingerprint != currentDomain)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Stale Domain",
                    "The complete resampled river-domain content changed.");
                return false;
            }

            if (package.ObstacleFingerprint != currentObstacles)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Stale Obstacles",
                    "The exact transformed static obstacle-source geometry " +
                    "changed.");
                return false;
            }

            if (package.GridDescriptor != gridDescriptor)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Stale Grid Contract",
                    "The cached Foam coordinate descriptor does not match " +
                    "the active allocation and mapping contract.");
                return false;
            }

            if (package.GenerationFingerprint != currentGeneration)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Stale Settings",
                    "Quality, field mapping, or a topology-generation setting " +
                    "changed.");
                return false;
            }

            if (asset.CombinedFingerprint != currentCombined.ToString())
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Combined Key",
                    "The assigned cache does not match the current combined " +
                    "stable input key.");
                return false;
            }

            topologyCacheValidationHitCount++;
            topologyCacheValidationState = "Hit Candidate";
            topologyCacheValidationSummary =
                "The assigned payload is valid and all stable inputs match. " +
                "Cache startup ownership is reported separately above.";
            Debug.Log(
                $"[River Foam Cache] Hit candidate for '{river.name}': " +
                $"{payload.Length:N0} bytes, inputs {currentCombined}.",
                river);
            return true;
#else
            SetTopologyCacheValidationMiss(
                "Unavailable",
                "Explicit cache validation is available only in the Editor " +
                "or a Development Build.");
            return false;
#endif
        }

        private bool AreTopologyCacheInputsCurrent()
        {
            if (!TopologyCacheRoundTripReady ||
                HasQueuedRebuildWork ||
                pendingTopologyReplacementAfterMaintenance ||
                TopologyReplacementInProgress ||
                river == null ||
                !river.Domain.IsValid)
            {
                return false;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            int currentObstacleGeometryVersion =
                disturbanceRuntime != null
                    ? disturbanceRuntime.ObstacleGeometryVersion
                    : -1;
            if (currentObstacleGeometryVersion != obstacleGeometryVersion)
            {
                return false;
            }

            return majorTopologyInputSignature ==
                    ResolveMajorTopologyInputSignature() &&
                connectorTopologyInputSignature ==
                    ResolveConnectorTopologyInputSignature() &&
                pocketTopologyInputSignature ==
                    ResolvePocketTopologyInputSignature();
        }

        private bool TryCaptureTopologyCacheFingerprints(
            out StylizedRiverFoamTopologyFingerprint domainFingerprint,
            out StylizedRiverFoamTopologyFingerprint obstacleFingerprint,
            out StylizedRiverFoamTopologyFingerprint generationFingerprint,
            out StylizedRiverFoamTopologyFingerprint combinedFingerprint,
            out string error,
            bool allowMeshFingerprintFallback = true)
        {
            domainFingerprint = default;
            obstacleFingerprint = default;
            generationFingerprint = default;
            combinedFingerprint = default;
            error = string.Empty;

            if (river == null || !river.Domain.IsValid)
            {
                error = "A valid resampled river domain is required.";
                return false;
            }

            domainFingerprint =
                StylizedRiverFoamTopologyFingerprints.ComputeDomain(
                    river.Domain);
            if (!TryValidateTopologyCacheGridDescriptor(
                    gridDescriptor,
                    out error))
            {
                return false;
            }

            generationFingerprint =
                StylizedRiverFoamTopologyFingerprints.ComputeGeneration(
                    river,
                    gridDescriptor);

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (disturbanceRuntime != null &&
                !disturbanceRuntime.GeneratedObstacleRegistryReady)
            {
                error =
                    "The generated obstacle registry is still refreshing " +
                    $"({disturbanceRuntime.GeneratedObstacleRegistryProcessedCount:N0} / " +
                    $"{disturbanceRuntime.GeneratedObstacleRegistryTotalCount:N0} sources).";
                return false;
            }

            topologyCachePreparedObstacleFingerprints.Clear();
            int sourceCount;
            string obstacleStatus;
            bool preparedFingerprintsAvailable = disturbanceRuntime == null
                ? RiverObstacleExclusionResolver
                    .TryCombineStableSourceFingerprints(
                        topologyCachePreparedObstacleFingerprints,
                        out obstacleFingerprint,
                        out sourceCount,
                        out obstacleStatus)
                : disturbanceRuntime
                    .TryCopyObstacleExclusionStableFingerprintsTo(
                        topologyCachePreparedObstacleFingerprints,
                        out sourceCount,
                        out obstacleStatus,
                        verifyDirectGeometry:
                            allowMeshFingerprintFallback &&
                            !Application.isPlaying) &&
                  RiverObstacleExclusionResolver
                    .TryCombineStableSourceFingerprints(
                        topologyCachePreparedObstacleFingerprints,
                        out obstacleFingerprint,
                        out sourceCount,
                        out obstacleStatus);

            if (!preparedFingerprintsAvailable)
            {
                if (!allowMeshFingerprintFallback)
                {
                    error = obstacleStatus;
                    return false;
                }

                topologyCacheFingerprintMeshFilters.Clear();
                disturbanceRuntime?.CopyObstacleExclusionMeshFiltersTo(
                    topologyCacheFingerprintMeshFilters);
                if (!RiverObstacleExclusionResolver
                    .TryComputeStableSourceFingerprint(
                        topologyCacheFingerprintMeshFilters,
                        out obstacleFingerprint,
                        out sourceCount,
                        out obstacleStatus))
                {
                    error = obstacleStatus;
                    return false;
                }
            }

            combinedFingerprint =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    domainFingerprint,
                    obstacleFingerprint,
                    generationFingerprint);
            topologyCacheDomainFingerprint = domainFingerprint.ToString();
            topologyCacheObstacleFingerprint =
                obstacleFingerprint.ToString();
            topologyCacheGenerationFingerprint =
                generationFingerprint.ToString();
            topologyCacheCombinedFingerprint =
                combinedFingerprint.ToString();
            topologyCacheObstacleSourceCount = sourceCount;
            return true;
        }

        private void CompleteDevelopmentTopologyGeneration()
        {
            bool completedP12Sweep =
                CompleteP12CandidateSweepTransientTopologyGeneration();
            bool completedExplicitly =
                explicitTopologyGenerationInProgress;
            explicitTopologyGenerationInProgress = false;
            activeTopologyObstacleStale = false;
            if (completedP12Sweep)
            {
                topologyCacheLoadedForActiveResources = false;
                topologyCacheStartupState =
                    "Generated for P12 Sweep";
                topologyCacheStartupSummary =
                    "The explicit P12 sweep generated transient topology for " +
                    "the active test descriptor. The assigned cache asset was " +
                    "not read, written, or replaced.";
            }
            else if (completedExplicitly)
            {
                topologyCacheLoadedForActiveResources = false;
                topologyCacheStartupState = "Generated Explicitly";
                topologyCacheStartupSummary =
                    "The explicit Edit Mode preparation path completed. " +
                    "No automatic Play Mode cache persistence was requested.";
            }
        }

        // Retained as a compatibility surface for older Editor callers. The
        // cache contract deliberately rejects automatic build or persistence.
        public bool TryBuildAutomaticTopologyCache(
            out StylizedRiverFoamTopologyCacheBuildArtifact artifact)
        {
            artifact = default;
            automaticTopologyCacheWriteCount++;
            if (!topologyStartupValidationComplete)
            {
                topologyStartupCacheBuildAttemptCount++;
                topologyStartupCacheWriteAttemptCount++;
            }
            automaticTopologyCacheWritePending = false;
            automaticTopologyCachePersistenceState = "Blocked";
            automaticTopologyCachePersistenceSummary =
                "An automatic topology-cache build was rejected. Prepare the " +
                "cache explicitly in Edit Mode.";
            return false;
        }

        public void ReportAutomaticTopologyCachePersisted(
            int payloadBytes,
            string payloadHash,
            string assetPath)
        {
            if (!topologyStartupValidationComplete)
            {
                topologyStartupCacheWriteAttemptCount++;
                topologyStartupCacheWriteSuccessCount++;
            }
            automaticTopologyCacheWritePending = false;
            automaticTopologyCachePersistenceState = "Blocked";
            automaticTopologyCachePersistenceSummary =
                "Automatic Play Mode cache persistence is disabled; a legacy " +
                "persistence callback was ignored.";
        }

        public void ReportAutomaticTopologyCachePersistenceFailure(
            string reason)
        {
            if (!topologyStartupValidationComplete)
            {
                topologyStartupCacheWriteAttemptCount++;
            }
            automaticTopologyCacheWritePending = false;
            automaticTopologyCachePersistenceState = "Disabled";
            automaticTopologyCachePersistenceSummary =
                string.IsNullOrEmpty(reason)
                    ? "Automatic Play Mode cache persistence is disabled."
                    : $"Automatic persistence is disabled. Legacy report: {reason}";
        }

        public bool RequestExplicitTopologyGeneration()
        {
            topologyCacheStartupState = "Preparation Required";
            topologyCacheStartupSummary =
                "Play Mode topology generation is disabled. Use Prepare / " +
                "Rebuild Foam Topology Cache in Edit Mode.";
            return false;
        }

        public bool TryPrepareTopologyCacheInEditor(
            out StylizedRiverFoamTopologyCacheBuildArtifact artifact)
        {
            artifact = default;
#if UNITY_EDITOR
            if (Application.isPlaying || editorTopologyPreparationInProgress)
            {
                topologyCacheBuildState = "Unavailable";
                topologyCacheBuildSummary =
                    "Edit Mode cache preparation cannot run during Play Mode " +
                    "or while another preparation is active.";
                return false;
            }

            river = GetComponent<StylizedRiver>();
            surfaceRenderer = river != null ? river.SurfaceRenderer : null;
            if (river == null || !river.Domain.IsValid)
            {
                topologyCacheBuildState = "Unavailable";
                topologyCacheBuildSummary =
                    "The River domain is not ready. Regenerate the River " +
                    "structure before preparing its Foam topology cache.";
                return false;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (disturbanceRuntime != null &&
                !disturbanceRuntime
                    .PrepareGeneratedGeometrySourcesForCacheValidation(
                        out string registryPreparationStatus))
            {
                topologyCacheBuildState = "Unavailable";
                topologyCacheBuildSummary = registryPreparationStatus;
                return false;
            }

            try
            {
                ReleaseResources();
                topologyCachePreparationGeneratedUploadCount = 0;
                editorTopologyPreparationInProgress = true;
                explicitTopologyGenerationInProgress = true;
                activeTopologyRequiresExplicitPreparation = false;
                resourcesDirty = true;
                boundaryDirty = true;
                initializationPhase = InitializationPhase.NotStarted;

                const int maximumSteps = 96;
                for (int step = 0; step < maximumSteps; step++)
                {
                    if (EnsureResources())
                    {
                        break;
                    }

                    if (initializationPhase == InitializationPhase.Failed ||
                        initializationPhase ==
                            InitializationPhase.CachePreparationRequired)
                    {
                        break;
                    }
                }

                if (initializationPhase != InitializationPhase.Ready ||
                    !AreResourcesCompleteAndCurrent())
                {
                    topologyCacheBuildState = "Failed";
                    topologyCacheBuildSummary =
                        "Explicit Edit Mode preparation did not reach a complete " +
                        $"topology state (phase {initializationPhase}).";
                    return false;
                }

                if (topologyCachePreparationGeneratedUploadCount != 1)
                {
                    topologyCacheBuildState = "Failed";
                    topologyCacheBuildSummary =
                        "Explicit preparation must publish generated topology " +
                        "exactly once after the complete Major, Connector, and " +
                        $"Pocket graph is ready; observed " +
                        $"{topologyCachePreparationGeneratedUploadCount:N0} uploads.";
                    return false;
                }

                if (!TryBuildTopologyCache(out artifact))
                {
                    return false;
                }

                if (topologyCacheLastBuildSerializationCount != 1)
                {
                    artifact = default;
                    topologyCacheBuildState = "Failed";
                    topologyCacheBuildSummary =
                        "Explicit preparation must serialize the normal cache " +
                        "payload exactly once; observed " +
                        $"{topologyCacheLastBuildSerializationCount:N0} serializations.";
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                topologyCacheBuildState = "Failed";
                topologyCacheBuildSummary = exception.Message;
                Debug.LogException(exception, river);
                return false;
            }
            finally
            {
                ReleaseResources();
                editorTopologyPreparationInProgress = false;
                explicitTopologyGenerationInProgress = false;
                initializationPhase = InitializationPhase.NotStarted;
                resourcesDirty = true;
                boundaryDirty = true;
                BindDisabled();
            }
#else
            topologyCacheBuildState = "Unavailable";
            topologyCacheBuildSummary =
                "Explicit cache preparation is Editor-only.";
            return false;
#endif
        }

        private TopologyCacheStartupResolution
            TryResolveAssignedTopologyCacheForStartup(
                out StylizedRiverFoamTopologyCachePackage package,
                out bool staleObstacles,
                out bool staleSettings)
        {
            package = null;
            staleObstacles = false;
            staleSettings = false;
            topologyCacheStartupAttemptCount++;
            topologyCacheStartupState = "Evaluating";
            topologyCacheStartupSummary =
                "Validating the assigned payload against prepared stable inputs.";
            topologyCacheStartupPayloadBytes = 0;
            topologyCacheStartupPayloadHash = "—";
            topologyCacheStartupLoadMilliseconds = 0.0;

            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            StylizedRiverFoamTopologyCacheAsset asset =
                river != null ? river.FoamTopologyCacheAsset : null;
            if (asset == null)
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.Unassigned,
                    "Miss — Unassigned",
                    "No Foam topology cache asset is assigned.");
                return TopologyCacheStartupResolution.Miss;
            }

            if (!asset.HasSupportedStorageContract)
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.StorageVersion,
                    "Miss — Storage Version",
                    "The assigned asset uses an unsupported storage contract.");
                return TopologyCacheStartupResolution.Miss;
            }

            if (!asset.HasPayload)
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.EmptyPayload,
                    "Miss — Empty",
                    "The assigned cache asset contains no payload bytes.");
                return TopologyCacheStartupResolution.Miss;
            }

            if (!asset.TryValidateStoredContract(
                    out bool legacyCoordinateContract,
                    out bool legacyObstacleFingerprintContract,
                    out bool legacyDynamicTopologyPhaseContract,
                    out string contractSummary))
            {
                SetTopologyCacheStartupMiss(
                    legacyCoordinateContract
                        ? TopologyCacheStartupReason.CoordinateContract
                        : legacyObstacleFingerprintContract
                            ? TopologyCacheStartupReason
                                .ObstacleFingerprintContract
                            : legacyDynamicTopologyPhaseContract
                                ? TopologyCacheStartupReason
                                    .DynamicTopologyPhaseContract
                                : TopologyCacheStartupReason.CacheContract,
                    legacyCoordinateContract
                        ? "Miss — Legacy Coordinate Contract"
                        : legacyObstacleFingerprintContract
                            ? "Miss — Legacy Obstacle Fingerprint Contract"
                            : legacyDynamicTopologyPhaseContract
                                ? "Miss — Legacy Dynamic Topology Phase"
                                : "Miss — Unsupported Cache Contract",
                    contractSummary);
                return TopologyCacheStartupResolution.Miss;
            }

            byte[] payload = asset.GetPayloadReadOnlyReference();
            if (!StylizedRiverFoamTopologyCacheCodec.TryDeserialize(
                    payload,
                    out package,
                    out string loadError))
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.InvalidPayload,
                    "Miss — Invalid Payload",
                    loadError);
                package = null;
                return TopologyCacheStartupResolution.Miss;
            }

            ulong payloadHash =
                StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(payload);
            StylizedRiverFoamTopologyFingerprint storedCombined =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    package.DomainFingerprint,
                    package.ObstacleFingerprint,
                    package.GenerationFingerprint);
            bool metadataMatches =
                asset.PayloadFormatVersion ==
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion &&
                asset.GeneratorContractVersion ==
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion &&
                asset.GridDescriptorContractVersion ==
                    StylizedRiverFoamGridDescriptor
                        .DescriptorContractVersion &&
                asset.GridMappingValue ==
                    (int)package.GridDescriptor.Mapping &&
                asset.GridMappingContractVersion ==
                    package.GridDescriptor.MappingContractVersion &&
                asset.GridInitializationSignature ==
                    package.GridDescriptor.InitializationSignature
                        .ToString("X16") &&
                asset.PayloadHash == payloadHash.ToString("X16") &&
                asset.DomainFingerprint ==
                    package.DomainFingerprint.ToString() &&
                asset.ObstacleFingerprint ==
                    package.ObstacleFingerprint.ToString() &&
                asset.GenerationFingerprint ==
                    package.GenerationFingerprint.ToString() &&
                asset.CombinedFingerprint == storedCombined.ToString();
            if (!metadataMatches)
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.MetadataMismatch,
                    "Miss — Metadata Mismatch",
                    "The asset metadata does not match its binary payload.");
                package = null;
                return TopologyCacheStartupResolution.Miss;
            }

            bool descriptorMatches =
                package.GridDescriptor == gridDescriptor;
            bool completeCurrentContract =
                descriptorMatches &&
                package.FieldWidth == fieldWidth &&
                package.FieldHeight == fieldHeight &&
                package.ObstacleExclusion != null &&
                package.ObstacleExclusion.Length == fieldWidth * fieldHeight &&
                package.MajorTopology != null &&
                package.ConnectorTopology != null &&
                package.PocketTopology != null;
            if (!descriptorMatches)
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.GridDescriptorMismatch,
                    "Miss — Stale Grid Contract",
                    "The cached Foam coordinate descriptor does not match " +
                    "the active allocation and mapping contract.");
                package = null;
                return TopologyCacheStartupResolution.Miss;
            }

            if (!completeCurrentContract)
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.IncompleteContract,
                    "Miss — Incomplete Contract",
                    "The payload does not contain a complete topology graph " +
                    "for the current field dimensions.");
                package = null;
                return TopologyCacheStartupResolution.Miss;
            }

            if (!TryCaptureTopologyCacheFingerprints(
                    out StylizedRiverFoamTopologyFingerprint currentDomain,
                    out StylizedRiverFoamTopologyFingerprint currentObstacles,
                    out StylizedRiverFoamTopologyFingerprint currentGeneration,
                    out StylizedRiverFoamTopologyFingerprint currentCombined,
                    out string fingerprintError,
                    IsAutomaticDevelopmentCacheEnabled))
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.InputsUnavailable,
                    "Miss — Inputs Unavailable",
                    fingerprintError);
                package = null;
                return TopologyCacheStartupResolution.Miss;
            }

            bool domainMatches = package.DomainFingerprint == currentDomain;
            bool obstaclesMatch =
                package.ObstacleFingerprint == currentObstacles;
            bool generationMatches =
                package.GenerationFingerprint == currentGeneration;
            bool combinedMatches =
                asset.CombinedFingerprint == currentCombined.ToString();

            stopwatch.Stop();
            topologyCacheStartupPayloadBytes = payload.Length;
            topologyCacheStartupPayloadHash = payloadHash.ToString("X16");
            topologyCacheStartupLoadMilliseconds =
                stopwatch.Elapsed.TotalMilliseconds;

            if (domainMatches && obstaclesMatch && generationMatches &&
                combinedMatches)
            {
                topologyCacheStartupHitCount++;
                topologyCacheStartupOutcome = TopologyCacheStartupOutcome.Exact;
                topologyCacheStartupReasons = TopologyCacheStartupReason.None;
                topologyCacheStartupState = "Hit — Ready to Install";
                topologyCacheStartupSummary =
                    "The complete payload and all stable inputs match. No " +
                    "obstacle mesh scan, GPU readback, or topology generator " +
                    "was used.";
                return TopologyCacheStartupResolution.ExactHit;
            }

            staleObstacles = !obstaclesMatch;
            staleSettings = !generationMatches || !combinedMatches;
            if (domainMatches && (staleObstacles || staleSettings))
            {
                topologyCacheStartupMissCount++;
                topologyCacheStartupOutcome =
                    TopologyCacheStartupOutcome.StaleCompatible;
                topologyCacheStartupReasons = TopologyCacheStartupReason.None;
                if (staleObstacles)
                {
                    topologyCacheStartupReasons |=
                        TopologyCacheStartupReason.ObstacleMismatch;
                }
                if (!generationMatches)
                {
                    topologyCacheStartupReasons |=
                        TopologyCacheStartupReason.GenerationMismatch;
                }
                if (!combinedMatches)
                {
                    topologyCacheStartupReasons |=
                        TopologyCacheStartupReason.CombinedMismatch;
                }
                topologyCacheStartupState = "Loaded — Stale Compatible";
                topologyCacheStartupSummary =
                    "The assigned cache is structurally compatible and will be " +
                    "used for this session. Play Mode will not rebuild or save " +
                    "a replacement; prepare the cache explicitly in Edit Mode.";
                return TopologyCacheStartupResolution.StaleCompatible;
            }

            if (!domainMatches)
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.DomainMismatch,
                    "Miss — Stale Domain",
                    "The complete resampled river-domain content changed.");
            }
            else if (staleObstacles)
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.ObstacleMismatch,
                    "Miss — Stale Obstacles",
                    "The exact transformed static obstacle geometry changed.");
            }
            else
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.GenerationMismatch,
                    "Miss — Stale Settings",
                    "Quality, mapping, or topology-generation settings changed.");
            }

            package = null;
            return TopologyCacheStartupResolution.Miss;
        }

        private void SetTopologyCacheStartupMiss(
            TopologyCacheStartupReason reason,
            string state,
            string summary)
        {
            topologyCacheStartupMissCount++;
            topologyCacheStartupOutcome =
                TopologyCacheStartupOutcome.PreparationRequired;
            topologyCacheStartupReasons = reason;
            topologyCacheStartupState = state ?? "Preparation Required";
            topologyCacheStartupSummary = (summary ?? string.Empty) +
                " Play Mode will not generate or persist topology. Use Prepare / " +
                "Rebuild Foam Topology Cache in Edit Mode.";
            topologyCacheStartupPayloadBytes = 0;
            topologyCacheStartupPayloadHash = "—";
            topologyCacheStartupLoadMilliseconds = 0.0;
        }

        private bool InstallStartupTopologyCache(
            StylizedRiverFoamTopologyCachePackage package,
            bool staleObstacles,
            bool staleSettings)
        {
            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            if (package == null ||
                package.GridDescriptor != gridDescriptor ||
                package.FieldWidth != fieldWidth ||
                package.FieldHeight != fieldHeight ||
                package.ObstacleExclusion == null ||
                package.ObstacleExclusion.Length != fieldWidth * fieldHeight ||
                package.MajorTopology == null ||
                package.ConnectorTopology == null ||
                package.PocketTopology == null)
            {
                SetTopologyCacheStartupMiss(
                    TopologyCacheStartupReason.InstallRejected,
                    "Miss — Install Rejected",
                    "The resolved payload became incomplete before installation.");
                return false;
            }

            ReleaseObstacleExclusionBuffers();
            obstacleExclusionMeshFilters.Clear();
            topologyCacheFingerprintMeshFilters.Clear();
            topologyCachePreparedObstacleFingerprints.Clear();
            obstacleExclusionCells.Clear();
            obstacleExclusionSamples.Clear();
            obstacleExclusionGpuCells =
                Array.Empty<FoamObstacleIntervalCellData>();
            obstacleExclusionScalar = package.ObstacleExclusion;
            obstacleGeometryVersion = ResolveCurrentObstacleGeometryVersion();
            obstacleExclusionUsesCachedScalar = true;
            activeTopologyObstacleStale = staleObstacles;
            UploadCachedObstacleExclusionScalar();

            majorTopology = package.MajorTopology;
            connectorTopology = package.ConnectorTopology;
            pocketTopology = package.PocketTopology;
            if (staleObstacles || staleSettings)
            {
                ApplyCachedTopologyInputSignatures(
                    package,
                    staleObstacles);
            }
            else
            {
                majorTopologyInputSignature =
                    ResolveMajorTopologyInputSignature();
                connectorTopologyInputSignature =
                    ResolveConnectorTopologyInputSignature();
                pocketTopologyInputSignature =
                    ResolvePocketTopologyInputSignature();
            }

            InitializeMajorEvolution();
            InitializeConnectorIdentityReconstruction(false);
            UploadGeneratedTopology();
            BuildEvolvingMajorField();
            topologyCacheLoadedForActiveResources = true;
            stopwatch.Stop();
            topologyCacheStartupLoadMilliseconds +=
                stopwatch.Elapsed.TotalMilliseconds;

            pendingStartupCacheStaleObstacles = staleObstacles;
            pendingStartupCacheStaleSettings = staleSettings;
            activeTopologyRequiresExplicitPreparation =
                staleObstacles || staleSettings;

            if (activeTopologyRequiresExplicitPreparation)
            {
                topologyCacheStartupOutcome =
                    TopologyCacheStartupOutcome.StaleCompatible;
                topologyCacheStartupState = "Loaded — Stale Compatible";
                topologyCacheStartupSummary =
                    "The structurally compatible cache is active for this " +
                    "session. Play Mode will not regenerate or save it; use " +
                    "Prepare / Rebuild Foam Topology Cache in Edit Mode.";
            }
            else
            {
                topologyCacheStartupOutcome = TopologyCacheStartupOutcome.Exact;
                topologyCacheStartupReasons = TopologyCacheStartupReason.None;
                topologyCacheStartupState = "Loaded — Exact";
                topologyCacheStartupSummary =
                    "Persistent topology and the exact cached obstacle scalar " +
                    "were installed without generation or asset persistence.";
            }

            return true;
        }

        private void ApplyCachedTopologyInputSignatures(
            StylizedRiverFoamTopologyCachePackage package,
            bool staleObstacles)
        {
            int signatureObstacleVersion = staleObstacles
                ? int.MinValue + 49
                : obstacleGeometryVersion;
            majorTopologyInputSignature =
                ResolveMajorTopologyInputSignature(
                    river.Domain,
                    signatureObstacleVersion,
                    package.Quality,
                    package.FieldWidth,
                    package.FieldHeight,
                    package.MajorSeed,
                    package.MajorAmount,
                    package.MajorSize,
                    package.MajorSizeVariation,
                    package.MajorRecycleTerritoryDeviationPercent,
                    package.ShoreMotion);
            connectorTopologyInputSignature =
                ResolveConnectorTopologyInputSignature(
                    majorTopologyInputSignature,
                    package.ConnectorAmount,
                    package.ConnectorDirectness,
                    package.ConnectorLengthPreference);
            pocketTopologyInputSignature =
                ResolvePocketTopologyInputSignature(
                    majorTopologyInputSignature,
                    connectorTopologyInputSignature,
                    package.InteriorPocketAmount,
                    package.EdgeCavityAmount,
                    package.ConnectorWeakSpanAmount,
                    package.FreeWaterEventAmount);
        }

        private void UploadCachedObstacleExclusionScalar()
        {
            if (obstacleExclusionTexture == null ||
                obstacleExclusionScalar.Length != fieldWidth * fieldHeight)
            {
                return;
            }

            if (obstacleExclusionUploadTexture == null ||
                obstacleExclusionUploadTexture.width != fieldWidth ||
                obstacleExclusionUploadTexture.height != fieldHeight)
            {
                if (obstacleExclusionUploadTexture != null)
                {
                    DestroyUnityObject(obstacleExclusionUploadTexture);
                }

                obstacleExclusionUploadTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RFloat,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamObstacleExclusion_Upload",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            obstacleExclusionUploadTexture.SetPixelData(
                obstacleExclusionScalar,
                0);
            obstacleExclusionUploadTexture.Apply(false, false);
            Graphics.Blit(
                obstacleExclusionUploadTexture,
                obstacleExclusionTexture);
        }

        private StylizedRiverFoamTopologyCachePackage
            CreateTopologyCachePackage(
                StylizedRiverFoamTopologyFingerprint domainFingerprint,
                StylizedRiverFoamTopologyFingerprint obstacleFingerprint,
                StylizedRiverFoamTopologyFingerprint generationFingerprint)
        {
            RiverDomainSnapshot domain = river.Domain;
            return new StylizedRiverFoamTopologyCachePackage(
                gridDescriptor,
                domain.GlobalDistanceMinimum,
                domain.GlobalDistanceMaximum,
                domain.LocalLength,
                domain.RequestedSampleSpacing,
                domain.ReverseFlow,
                domain.SampleCount,
                domainFingerprint,
                obstacleFingerprint,
                generationFingerprint,
                river.Quality,
                river.ShoreMotion,
                river.FoamMajorSupportSeed,
                river.FoamMajorSupportAmount,
                river.FoamMajorSupportSize,
                river.FoamMajorSupportSizeVariation,
                river.FoamMajorRecycleTerritoryDeviationPercent,
                river.FoamConnectorAmount,
                river.FoamConnectorDirectness,
                river.FoamConnectorLengthPreference,
                river.FoamInteriorPocketAmount,
                river.FoamEdgeCavityAmount,
                river.FoamConnectorWeakSpanAmount,
                river.FoamFreeWaterEventAmount,
                obstacleExclusionScalar,
                majorTopology,
                connectorTopology,
                pocketTopology);
        }

        private static bool TryValidateTopologyCacheGridDescriptor(
            StylizedRiverFoamGridDescriptor descriptor,
            out string error)
        {
            if (!descriptor.IsCreated)
            {
                error = "The active Foam grid descriptor is unavailable.";
                return false;
            }
            if (descriptor.ColumnCount >
                    StylizedRiverFoamTopologyCacheCodec.MaximumFieldDimension ||
                descriptor.RowCount >
                    StylizedRiverFoamTopologyCacheCodec.MaximumFieldDimension)
            {
                error =
                    $"Foam grid {descriptor.ColumnCount} × " +
                    $"{descriptor.RowCount} exceeds the contiguous cache " +
                    $"dimension limit " +
                    $"{StylizedRiverFoamTopologyCacheCodec.MaximumFieldDimension}. " +
                    "The runtime will not silently change physical scale; use " +
                    "the later strip architecture or a deliberately lower " +
                    "quality tier.";
                return false;
            }

            long cellCount = descriptor.StructuralCellCount;
            if (cellCount >
                StylizedRiverFoamTopologyCacheCodec.MaximumFieldCellCount)
            {
                error =
                    $"Foam grid contains {cellCount:N0} cells; the contiguous " +
                    $"cache limit is " +
                    $"{StylizedRiverFoamTopologyCacheCodec.MaximumFieldCellCount:N0}. " +
                    "Physical scale was not degraded.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void SetTopologyCacheValidationMiss(
            string state,
            string summary)
        {
            topologyCacheValidationState = state ?? "Miss";
            topologyCacheValidationSummary = summary ?? string.Empty;
        }
    }
}
