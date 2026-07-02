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
            if (!Application.isPlaying ||
                initializationPhase != InitializationPhase.Ready ||
                !AreResourcesCompleteAndCurrent())
            {
                return false;
            }

            RequestTopologyReplacement(
                TopologyReplacementReason.IdenticalValidation,
                true);
            return topologyReplacementBuild != null;
        }

        public bool RunTopologyCacheRoundTripValidation()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            topologyCacheRoundTripRunCount++;
            topologyCacheRoundTripState = "Running";
            topologyCacheRoundTripSummary =
                "Capturing the immutable prepared topology graph.";

            if (!Application.isPlaying ||
                !TopologyCacheRoundTripReady ||
                river == null ||
                !river.Domain.IsValid ||
                TopologyReplacementInProgress)
            {
                topologyCacheRoundTripState = "Unavailable";
                topologyCacheRoundTripSummary =
                    "The proof requires a fully initialized Play-mode topology " +
                    "with no replacement build in progress.";
                return false;
            }

            try
            {
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

                StylizedRiverFoamTopologyCachePackage package =
                    CreateTopologyCachePackage(
                        domainFingerprint,
                        obstacleFingerprint,
                        generationFingerprint);

                StylizedRiverFoamTopologyCacheRoundTripResult result =
                    StylizedRiverFoamTopologyCacheCodec.ValidateRoundTrip(
                        package);
                topologyCacheRoundTripPayloadBytes = result.PayloadByteCount;
                topologyCacheRoundTripPayloadHash = result.PayloadHash;
                topologyCacheRoundTripSerializationMilliseconds =
                    result.SerializationMilliseconds;
                topologyCacheRoundTripLoadMilliseconds =
                    result.LoadMilliseconds;
                topologyCacheRoundTripVerificationMilliseconds =
                    result.VerificationMilliseconds;
                topologyCacheRoundTripSummary = result.Summary;
                topologyCacheRoundTripState = result.Passed
                    ? "Passed"
                    : "Failed";
                if (result.Passed)
                {
                    topologyCacheRoundTripPassCount++;
                    Debug.Log(
                        $"[River Foam 4.9A] Cache round-trip passed for " +
                        $"'{river.name}': {result.PayloadByteCount:N0} bytes, " +
                        $"hash {result.PayloadHash:X16}.",
                        river);
                }
                else
                {
                    Debug.LogError(
                        $"[River Foam 4.9A] Cache round-trip failed for " +
                        $"'{river.name}': {result.Summary}",
                        river);
                }

                return result.Passed;
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
            topologyCacheBuildState = "Building";
            topologyCacheBuildSummary =
                "Capturing stable inputs and validating the complete payload.";

            if (!Application.isPlaying ||
                !TopologyCacheBuildReady ||
                river == null ||
                !river.Domain.IsValid ||
                TopologyReplacementInProgress)
            {
                topologyCacheBuildState = "Unavailable";
                topologyCacheBuildSummary =
                    "Cache building requires a fully initialized Play-mode " +
                    "topology whose maintenance and replacement work is complete.";
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
                StylizedRiverFoamTopologyCacheRoundTripResult proof =
                    StylizedRiverFoamTopologyCacheCodec.ValidateRoundTrip(
                        package);
                if (!proof.Passed)
                {
                    stopwatch.Stop();
                    topologyCacheBuildState = "Failed";
                    topologyCacheBuildSummary =
                        $"The accepted 4.9A proof failed during cache build: " +
                        proof.Summary;
                    topologyCacheBuildMilliseconds =
                        stopwatch.Elapsed.TotalMilliseconds;
                    return false;
                }

                byte[] payload =
                    StylizedRiverFoamTopologyCacheCodec.Serialize(package);
                ulong payloadHash =
                    StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(
                        payload);
                if (payload.Length != proof.PayloadByteCount ||
                    payloadHash != proof.PayloadHash)
                {
                    stopwatch.Stop();
                    topologyCacheBuildState = "Failed";
                    topologyCacheBuildSummary =
                        "The final cache payload did not reproduce the " +
                        "validated round-trip size and checksum.";
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
                    "A complete versioned payload was built from the active " +
                    "prepared topology. It has not been loaded or activated.";

                artifact = new StylizedRiverFoamTopologyCacheBuildArtifact(
                    payload,
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion,
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion,
                    domainFingerprint.ToString(),
                    obstacleFingerprint.ToString(),
                    generationFingerprint.ToString(),
                    combinedFingerprint.ToString(),
                    payloadHash.ToString("X16"),
                    topologyCacheBuildMilliseconds,
                    river.name);

                Debug.Log(
                    $"[River Foam 4.9B] Built cache payload for " +
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
        /// simulation field. Patch 4.9D build preflight uses this strict path; it
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

            byte[] payload = asset.GetPayloadCopy();
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
                        out obstacleStatus) &&
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
            StylizedRiverFoamTopologyFingerprint currentGeneration =
                StylizedRiverFoamTopologyFingerprints.ComputeGeneration(
                    river,
                    package.FieldWidth,
                    package.FieldHeight,
                    package.FieldLength,
                    package.ValidFieldLength);
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

            byte[] payload = asset.GetPayloadCopy();
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
                "Patch 4.9C startup ownership is reported separately above.";
            Debug.Log(
                $"[River Foam 4.9B] Cache hit candidate for '{river.name}': " +
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
            generationFingerprint =
                StylizedRiverFoamTopologyFingerprints.ComputeGeneration(
                    river,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    validFieldLength);

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
                        out obstacleStatus) &&
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

        private void BeginAutomaticDevelopmentStartupGeneration()
        {
            automaticTopologyGenerationInProgress = true;
            automaticDevelopmentRebuildReason =
                AutomaticDevelopmentRebuildReason.StartupMiss;
            pendingAutomaticDevelopmentRebuildAfterStartup = false;
            pendingStartupCacheStaleObstacles = false;
            pendingStartupCacheStaleSettings = false;
            topologyCacheLoadedForActiveResources = false;
            topologyCacheStartupState = "Auto-Generating Cache Miss";
            topologyCacheStartupSummary =
                "No current persistent payload could be installed. This " +
                "Editor/Development session is running the accepted expensive " +
                "preparation path automatically. The Editor will validate and " +
                "save the completed result without additional user actions.";
        }

        private void BeginAutomaticDevelopmentRebuildAfterStartup()
        {
            if (!pendingAutomaticDevelopmentRebuildAfterStartup ||
                !IsAutomaticDevelopmentCacheEnabled ||
                initializationPhase != InitializationPhase.Ready)
            {
                return;
            }

            pendingAutomaticDevelopmentRebuildAfterStartup = false;
            automaticTopologyGenerationInProgress = true;
            automaticDevelopmentObservedSignature =
                ResolveRequestedTopologySignature();
            automaticDevelopmentRebuildNotBefore =
                Time.realtimeSinceStartupAsDouble;

            if (pendingStartupCacheStaleObstacles)
            {
                automaticDevelopmentRebuildReason =
                    AutomaticDevelopmentRebuildReason.Obstacles;
                obstacleExclusionUsesCachedScalar = false;
                RequestObstacleRebuild(
                    ResolveCurrentObstacleGeometryVersion(),
                    true);
            }
            else
            {
                automaticDevelopmentRebuildReason =
                    AutomaticDevelopmentRebuildReason.Settings;
                RequestTopologyReplacement(
                    TopologyReplacementReason.Settings,
                    false);
            }

            pendingStartupCacheStaleObstacles = false;
            pendingStartupCacheStaleSettings = false;
        }

        private void CompleteDevelopmentTopologyGeneration(
            string completedPath)
        {
            bool completedAutomatically =
                automaticTopologyGenerationInProgress;
            bool completedExplicitly =
                explicitTopologyGenerationInProgress;
            automaticTopologyGenerationInProgress = false;
            explicitTopologyGenerationInProgress = false;
            automaticDevelopmentRebuildReason =
                AutomaticDevelopmentRebuildReason.None;
            activeTopologyObstacleStale = false;
            if (completedAutomatically || completedExplicitly)
            {
                topologyCacheLoadedForActiveResources = false;
            }

            if (completedAutomatically)
            {
                QueueAutomaticTopologyCachePersistence(completedPath);
            }
            else if (completedExplicitly)
            {
                topologyCacheStartupState = "Generated Explicitly";
                topologyCacheStartupSummary =
                    "Development diagnostics explicitly ran the expensive " +
                    "preparation path. Automatic development caching was not " +
                    "required for this operation.";
            }
        }

        private void QueueAutomaticTopologyCachePersistence(
            string completedPath)
        {
            if (!TryCaptureTopologyCacheFingerprints(
                    out _,
                    out _,
                    out _,
                    out StylizedRiverFoamTopologyFingerprint currentCombined,
                    out string fingerprintError))
            {
                automaticTopologyCacheWritePending = false;
                automaticTopologyCachePersistenceState =
                    "Cache Write Failed";
                automaticTopologyCachePersistenceSummary =
                    "The generated topology is active, but its stable input " +
                    $"key could not be captured: {fingerprintError}";
                topologyCacheStartupState =
                    "Generated — Cache Write Failed";
                topologyCacheStartupSummary =
                    automaticTopologyCachePersistenceSummary;
                return;
            }

            string currentInputKey = currentCombined.ToString();
            if (!topologyCacheSessionPersistentInputCaptured ||
                topologyCacheSessionPersistentInputKey != currentInputKey)
            {
                automaticTopologyCacheWritePending = false;
                automaticTopologyCachePersistenceState =
                    "Session-Only Development Topology";
                automaticTopologyCachePersistenceSummary =
                    "The generated topology is active for this Play session, " +
                    "but its inputs differ from the persistent values captured " +
                    "at Play entry, so the authored cache was not overwritten.";
                topologyCacheStartupState =
                    "Session-Only Development Topology";
                topologyCacheStartupSummary =
                    automaticTopologyCachePersistenceSummary;
                return;
            }

#if UNITY_EDITOR
            automaticTopologyCacheWritePending = true;
            automaticTopologyCachePersistenceState =
                "Pending Automatic Save";
            automaticTopologyCachePersistenceSummary =
                $"The complete topology produced by {completedPath} is active. " +
                "The Editor cache coordinator will build, validate, and save " +
                "its persistent payload automatically.";
            topologyCacheStartupState = "Auto-Generated — Saving Cache";
            topologyCacheStartupSummary =
                automaticTopologyCachePersistenceSummary;
#else
            automaticTopologyCacheWritePending = false;
            automaticTopologyCachePersistenceState =
                "Session-Only Development Topology";
            automaticTopologyCachePersistenceSummary =
                "A Development Player generated the topology successfully. " +
                "Asset persistence is Editor-only, so this result remains " +
                "session-local.";
            topologyCacheStartupState =
                "Session-Only Development Topology";
            topologyCacheStartupSummary =
                automaticTopologyCachePersistenceSummary;
#endif
        }

        public bool TryBuildAutomaticTopologyCache(
            out StylizedRiverFoamTopologyCacheBuildArtifact artifact)
        {
            artifact = default;
#if UNITY_EDITOR
            if (!automaticTopologyCacheWritePending ||
                !TopologyCacheBuildReady)
            {
                return false;
            }

            automaticTopologyCacheWritePending = false;
            automaticTopologyCacheWriteCount++;
            if (!TryBuildTopologyCache(out artifact))
            {
                automaticTopologyCachePersistenceState =
                    "Cache Write Failed";
                automaticTopologyCachePersistenceSummary =
                    "The automatic payload build failed. The generated topology " +
                    "remains active and any previous cache asset remains available.";
                topologyCacheStartupState =
                    "Generated — Cache Write Failed";
                topologyCacheStartupSummary =
                    automaticTopologyCachePersistenceSummary;
                return false;
            }

            automaticTopologyCachePersistenceState =
                "Built — Awaiting Editor Save";
            automaticTopologyCachePersistenceSummary =
                "The payload passed the exact round-trip proof and is ready for " +
                "the Editor coordinator to store in the assigned asset.";
            return true;
#else
            return false;
#endif
        }

        public bool ValidateAutomaticTopologyCacheArtifact(
            StylizedRiverFoamTopologyCacheBuildArtifact artifact,
            out string error)
        {
            error = string.Empty;
#if UNITY_EDITOR
            topologyCacheValidationCount++;
            if (!Application.isPlaying || !TopologyCacheBuildReady ||
                river == null || !river.Domain.IsValid)
            {
                error =
                    "Automatic persistence requires a complete current topology.";
                SetTopologyCacheValidationMiss(
                    "Unavailable",
                    error);
                return false;
            }

            if (artifact.PayloadByteCount <= 0 ||
                artifact.FormatVersion !=
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion ||
                artifact.GeneratorContractVersion !=
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion)
            {
                error =
                    "The automatic build artifact is empty or uses an unsupported contract.";
                SetTopologyCacheValidationMiss(
                    "Miss — Build Artifact",
                    error);
                return false;
            }

            byte[] payload = artifact.CopyPayload();
            if (!StylizedRiverFoamTopologyCacheCodec.TryDeserialize(
                    payload,
                    out StylizedRiverFoamTopologyCachePackage package,
                    out string loadError))
            {
                error = loadError;
                SetTopologyCacheValidationMiss(
                    "Miss — Invalid Build Artifact",
                    error);
                return false;
            }

            ulong payloadHash =
                StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(payload);
            StylizedRiverFoamTopologyFingerprint storedCombined =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    package.DomainFingerprint,
                    package.ObstacleFingerprint,
                    package.GenerationFingerprint);
            bool artifactMetadataMatches =
                artifact.PayloadHash == payloadHash.ToString("X16") &&
                artifact.DomainFingerprint ==
                    package.DomainFingerprint.ToString() &&
                artifact.ObstacleFingerprint ==
                    package.ObstacleFingerprint.ToString() &&
                artifact.GenerationFingerprint ==
                    package.GenerationFingerprint.ToString() &&
                artifact.CombinedFingerprint == storedCombined.ToString();
            if (!artifactMetadataMatches)
            {
                error =
                    "The automatic build artifact metadata does not match its payload.";
                SetTopologyCacheValidationMiss(
                    "Miss — Build Metadata",
                    error);
                return false;
            }

            if (!TryCaptureTopologyCacheFingerprints(
                    out StylizedRiverFoamTopologyFingerprint currentDomain,
                    out StylizedRiverFoamTopologyFingerprint currentObstacles,
                    out StylizedRiverFoamTopologyFingerprint currentGeneration,
                    out StylizedRiverFoamTopologyFingerprint currentCombined,
                    out string fingerprintError))
            {
                error = fingerprintError;
                SetTopologyCacheValidationMiss(
                    "Miss — Inputs Unavailable",
                    error);
                return false;
            }

            bool current = package.FieldWidth == fieldWidth &&
                package.FieldHeight == fieldHeight &&
                package.DomainFingerprint == currentDomain &&
                package.ObstacleFingerprint == currentObstacles &&
                package.GenerationFingerprint == currentGeneration &&
                artifact.CombinedFingerprint == currentCombined.ToString();
            if (!current)
            {
                error =
                    "Stable inputs changed after the automatic payload was built; the previous persistent cache was retained.";
                SetTopologyCacheValidationMiss(
                    "Miss — Inputs Changed During Save",
                    error);
                return false;
            }

            topologyCacheValidationHitCount++;
            topologyCacheValidationState = "Hit Candidate";
            topologyCacheValidationSummary =
                "The automatic payload passed its binary contract, metadata, " +
                "field-dimension, and current stable-input checks before the " +
                "persistent asset was modified.";
            return true;
#else
            error =
                "Automatic persistent asset validation is Editor-only.";
            return false;
#endif
        }

        public void ReportAutomaticTopologyCachePersisted(
            int payloadBytes,
            string payloadHash,
            string assetPath)
        {
#if UNITY_EDITOR
            automaticTopologyCacheWriteSuccessCount++;
            automaticTopologyCachePersistenceState =
                "Automatically Generated and Cached";
            automaticTopologyCachePersistenceSummary =
                $"The complete payload was validated and saved to " +
                $"'{assetPath}'. Future matching Play entries load it directly.";
            topologyCacheStartupPayloadBytes = Mathf.Max(0, payloadBytes);
            topologyCacheStartupPayloadHash =
                string.IsNullOrEmpty(payloadHash) ? "—" : payloadHash;
            topologyCacheStartupState =
                "Automatically Generated and Cached";
            topologyCacheStartupSummary =
                automaticTopologyCachePersistenceSummary;
            Debug.Log(
                $"[River Foam 4.9C.1] Automatically cached topology for " +
                $"'{river.name}': {payloadBytes:N0} bytes, hash {payloadHash}.",
                river);
#endif
        }

        public void ReportAutomaticTopologyCachePersistenceFailure(
            string reason)
        {
#if UNITY_EDITOR
            automaticTopologyCacheWritePending = false;
            automaticTopologyCachePersistenceState = "Cache Write Failed";
            automaticTopologyCachePersistenceSummary =
                string.IsNullOrEmpty(reason)
                    ? "The automatic Editor cache write failed."
                    : reason;
            topologyCacheStartupState =
                "Generated — Cache Write Failed";
            topologyCacheStartupSummary =
                automaticTopologyCachePersistenceSummary;
            Debug.LogWarning(
                $"[River Foam 4.9C.1] Automatic cache persistence failed for " +
                $"'{river.name}': {automaticTopologyCachePersistenceSummary}",
                river);
#endif
        }

        public bool RequestExplicitTopologyGeneration()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!Application.isPlaying || DevelopmentTopologyGenerationInProgress)
            {
                return false;
            }

            if (initializationPhase ==
                InitializationPhase.AwaitExplicitTopologyGeneration)
            {
                explicitTopologyGenerationRequested = true;
                explicitTopologyGenerationInProgress = true;
                automaticTopologyGenerationInProgress = false;
                automaticTopologyCacheWritePending = false;
                topologyCacheStartupState = "Generating Explicitly";
                topologyCacheStartupSummary =
                    "Development tooling explicitly authorized the expensive " +
                    "obstacle and topology preparation path.";
                return true;
            }

            if (initializationPhase == InitializationPhase.Ready)
            {
                int obstacleVersion = ResolveCurrentObstacleGeometryVersion();
                bool obstacleTopologyStale =
                    obstacleVersion != obstacleGeometryVersion ||
                    activeTopologyObstacleStale;
                bool settingsTopologyStale =
                    ResolveRequestedTopologySignature() !=
                    ResolveActiveTopologySignature();
                if (!obstacleTopologyStale && !settingsTopologyStale)
                {
                    return false;
                }

                explicitTopologyGenerationInProgress = true;
                automaticTopologyGenerationInProgress = false;
                automaticTopologyCacheWritePending = false;
                topologyCacheLoadedForActiveResources = false;
                if (obstacleTopologyStale)
                {
                    obstacleExclusionUsesCachedScalar = false;
                    RequestObstacleRebuild(
                        obstacleVersion,
                        true);
                }
                else
                {
                    RequestTopologyReplacement(
                        TopologyReplacementReason.Settings,
                        false);
                }

                topologyCacheStartupState = "Generating Explicitly";
                topologyCacheStartupSummary =
                    "Development tooling explicitly authorized replacement " +
                    "generation for the stale active topology.";
                return true;
            }

            return false;
#else
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
            if (!TryCaptureTopologyCacheFingerprints(
                    out StylizedRiverFoamTopologyFingerprint currentDomain,
                    out StylizedRiverFoamTopologyFingerprint currentObstacles,
                    out StylizedRiverFoamTopologyFingerprint currentGeneration,
                    out StylizedRiverFoamTopologyFingerprint currentCombined,
                    out string fingerprintError,
                    IsAutomaticDevelopmentCacheEnabled))
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Inputs Unavailable",
                    fingerprintError);
                return TopologyCacheStartupResolution.Miss;
            }

            CaptureTopologyCacheSessionPersistentInputKey(currentCombined);

            StylizedRiverFoamTopologyCacheAsset asset =
                river != null ? river.FoamTopologyCacheAsset : null;
            if (asset == null)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Unassigned",
                    "No Foam topology cache asset is assigned.");
                return TopologyCacheStartupResolution.Miss;
            }

            if (!asset.HasSupportedStorageContract)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Storage Version",
                    "The assigned asset uses an unsupported storage contract.");
                return TopologyCacheStartupResolution.Miss;
            }

            if (!asset.HasPayload)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Empty",
                    "The assigned cache asset contains no payload bytes.");
                return TopologyCacheStartupResolution.Miss;
            }

            byte[] payload = asset.GetPayloadCopy();
            if (!StylizedRiverFoamTopologyCacheCodec.TryDeserialize(
                    payload,
                    out package,
                    out string loadError))
            {
                SetTopologyCacheStartupMiss(
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
                    "Miss — Metadata Mismatch",
                    "The asset metadata does not match its binary payload.");
                package = null;
                return TopologyCacheStartupResolution.Miss;
            }

            bool completeCurrentContract =
                package.FieldWidth == fieldWidth &&
                package.FieldHeight == fieldHeight &&
                package.ObstacleExclusion != null &&
                package.ObstacleExclusion.Length == fieldWidth * fieldHeight &&
                package.MajorTopology != null &&
                package.ConnectorTopology != null &&
                package.PocketTopology != null;
            if (!completeCurrentContract)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Incomplete Contract",
                    "The payload does not contain a complete topology graph " +
                    "for the current field dimensions.");
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
                topologyCacheStartupState = "Hit — Ready to Install";
                topologyCacheStartupSummary =
                    "The complete payload and all stable inputs match. No " +
                    "obstacle mesh scan, GPU readback, or topology generator " +
                    "was used.";
                return TopologyCacheStartupResolution.ExactHit;
            }

            staleObstacles = !obstaclesMatch;
            staleSettings = !generationMatches || !combinedMatches;
            if (IsAutomaticDevelopmentCacheEnabled && domainMatches &&
                (staleObstacles || staleSettings))
            {
                topologyCacheStartupMissCount++;
                topologyCacheStartupState =
                    "Using Previous Cache — Rebuilding";
                topologyCacheStartupSummary =
                    "The assigned cache is structurally valid and uses the " +
                    "same river mapping and dimensions. Its previous generated " +
                    "topology will remain visible while current development " +
                    "inputs regenerate automatically.";
                return TopologyCacheStartupResolution.StaleCompatible;
            }

            if (!domainMatches)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Stale Domain",
                    "The complete resampled river-domain content changed.");
            }
            else if (staleObstacles)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Stale Obstacles",
                    "The exact transformed static obstacle geometry changed.");
            }
            else
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Stale Settings",
                    "Quality, mapping, or topology-generation settings changed.");
            }

            package = null;
            return TopologyCacheStartupResolution.Miss;
        }

        private void CaptureTopologyCacheSessionPersistentInputKey(
            StylizedRiverFoamTopologyFingerprint combinedFingerprint)
        {
            if (topologyCacheSessionPersistentInputCaptured)
            {
                return;
            }

            topologyCacheSessionPersistentInputKey =
                combinedFingerprint.ToString();
            topologyCacheSessionPersistentInputCaptured = true;
        }

        private void SetTopologyCacheStartupMiss(
            string state,
            string summary)
        {
            topologyCacheStartupMissCount++;
            topologyCacheStartupState = state ?? "Miss";
            topologyCacheStartupSummary = summary ?? string.Empty;
            if (IsAutomaticDevelopmentCacheEnabled)
            {
                topologyCacheStartupSummary +=
                    " Development startup will generate the complete topology " +
                    "automatically; the Editor will persist it when the inputs " +
                    "still match the values present at Play entry.";
            }
            else
            {
                topologyCacheStartupSummary +=
                    " Production startup remains neutral and never runs the " +
                    "expensive generator path automatically.";
            }

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
            if (package == null || package.FieldWidth != fieldWidth ||
                package.FieldHeight != fieldHeight ||
                package.ObstacleExclusion == null ||
                package.ObstacleExclusion.Length != fieldWidth * fieldHeight ||
                package.MajorTopology == null ||
                package.ConnectorTopology == null ||
                package.PocketTopology == null)
            {
                SetTopologyCacheStartupMiss(
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
            pendingAutomaticDevelopmentRebuildAfterStartup =
                IsAutomaticDevelopmentCacheEnabled &&
                (staleObstacles || staleSettings);

            if (pendingAutomaticDevelopmentRebuildAfterStartup)
            {
                topologyCacheStartupState =
                    "Using Previous Cache — Rebuilding";
                topologyCacheStartupSummary = staleObstacles
                    ? "The previous generated topology was installed. The live " +
                      "obstacle field and a complete replacement will now be " +
                      "prepared automatically without a neutral frame."
                    : "The previous generated topology was installed and will " +
                      "remain visible while current settings prepare a complete " +
                      "replacement automatically.";
                Debug.Log(
                    $"[River Foam 4.9C.1] Retained stale cache for " +
                    $"'{river.name}' while automatic regeneration starts.",
                    river);
            }
            else
            {
                topologyCacheStartupState = "Loaded";
                topologyCacheStartupSummary =
                    "Persistent topology and the exact cached obstacle scalar " +
                    "were installed. Runtime shore, obstacle, pressure, lee, " +
                    "wake, and other live sources remain composed normally.";
                Debug.Log(
                    $"[River Foam 4.9C] Loaded cache for '{river.name}': " +
                    $"{topologyCacheStartupPayloadBytes:N0} bytes, hash " +
                    $"{topologyCacheStartupPayloadHash}.",
                    river);
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
                fieldWidth,
                fieldHeight,
                fieldLength,
                validFieldLength,
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

        private void SetTopologyCacheValidationMiss(
            string state,
            string summary)
        {
            topologyCacheValidationState = state ?? "Miss";
            topologyCacheValidationSummary = summary ?? string.Empty;
        }
    }
}
