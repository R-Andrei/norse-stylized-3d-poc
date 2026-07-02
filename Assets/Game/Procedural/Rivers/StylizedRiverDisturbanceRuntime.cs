using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    [AddComponentMenu("")]
    public sealed partial class StylizedRiverDisturbanceRuntime : MonoBehaviour
    {

        private void OnEnable()
        {
            river = GetComponent<StylizedRiver>();
            surfaceRenderer = river != null ? river.SurfaceRenderer : null;
            propertyBlock ??= new MaterialPropertyBlock();

            if (!ActiveRuntimes.Contains(this))
            {
                ActiveRuntimes.Add(this);
            }

            if (river != null)
            {
                river.DomainChanged += HandleDomainChanged;
            }

            GeneratedGeometryRegistry.SourceAdded +=
                HandleGeneratedGeometrySourceAdded;
            GeneratedGeometryRegistry.SourceRemoved +=
                HandleGeneratedGeometrySourceRemoved;
            GeneratedGeometryRegistry.SourceChanged +=
                HandleGeneratedGeometrySourceChanged;

            lastRuntimeTime = Time.realtimeSinceStartupAsDouble;
            resourcesDirty = true;
            generatedGeometryRegistryDirty = true;
            BindDisabled();
        }

        private void OnDisable()
        {
            ActiveRuntimes.Remove(this);

            if (river != null)
            {
                river.DomainChanged -= HandleDomainChanged;
            }

            GeneratedGeometryRegistry.SourceAdded -=
                HandleGeneratedGeometrySourceAdded;
            GeneratedGeometryRegistry.SourceRemoved -=
                HandleGeneratedGeometrySourceRemoved;
            GeneratedGeometryRegistry.SourceChanged -=
                HandleGeneratedGeometrySourceChanged;

            RemoveOwnedGeneratedDiagnostics();
            BindDisabled();
            ReleaseResources();
            continuousSources.Clear();
            continuousSourceIdsByOwner.Clear();
            obstacleGeometryVersion = 0;
            ownershipConflictWarningOwnerIds.Clear();
            automaticGeneratedSourceIds.Clear();
            refreshedAutomaticGeneratedSourceIds.Clear();
            generatedGeometryScratch.Clear();
            staticPressureProfileSourceIds.Clear();
            generatedGeometryRefreshInProgress = false;
            generatedGeometryRefreshIndex = 0;
            pendingImpacts.Clear();
            activeImpactReservations.Clear();
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private void LateUpdate()
        {
            BeginPerformanceDiagnosticsUpdate();

            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.RuntimeDisturbancesEnabled)
            {
                BindDisabled();
                ReleaseResources();
                return;
            }

            surfaceRenderer = river.SurfaceRenderer;

            if (!Application.isPlaying)
            {
                BindDisabled();
                return;
            }

            if (!IsSupported)
            {
                if (!supportWarningReported)
                {
                    Debug.LogWarning(
                        $"StylizedRiver disturbance field on '{name}' is disabled because compute shaders or required half-float random-write textures are unavailable.",
                        this);
                    supportWarningReported = true;
                }

                BindDisabled();
                return;
            }

            supportWarningReported = false;

            if (river.LiquidFactor <= 0.0001f)
            {
                if (!wasFrozen)
                {
                    ClearField();
                }

                // Impact requests can arrive after the freeze-transition clear
                // but before this runtime updates again. Discard them every
                // fully frozen frame so no event can survive and replay after
                // thawing.
                pendingImpacts.Clear();
                activeImpactReservations.Clear();
                impactsInjectedLastStep = 0;
                currentRippleSubstepCount = 0;
                maximumRecentRippleSubstepCount = 0;
                activeRippleMinimumCellSize = 0f;
                rippleSubstepLimitReached = false;
                rippleSubstepDiagnosticWindowStart = 0.0;
                wasFrozen = true;
                BindDisabled();
                return;
            }

            wasFrozen = false;

            if (generatedGeometryRegistryDirty ||
                generatedGeometryRefreshInProgress)
            {
                RefreshGeneratedGeometrySources();
            }

            double now = Time.realtimeSinceStartupAsDouble;
            float deltaTime = Mathf.Clamp(
                (float)(now - lastRuntimeTime),
                0f,
                0.1f);
            lastRuntimeTime = now;

            CleanupStaleSources(now);
            UpdateStaticPressureProfiles(deltaTime, now);
            UpdateStaticWakeVariations(deltaTime, now);

            bool requiresField =
                pendingImpacts.Count > 0 ||
                activeImpactReservations.Count > 0 ||
                continuousSources.Count > 0 ||
                HasActiveChunks();

            if (!requiresField)
            {
                if (currentState != null &&
                    now - lastActivityTime > 10.0)
                {
                    ReleaseResources();
                }

                BindDisabled();
                return;
            }

            if (!EnsureResources())
            {
                BindDisabled();
                return;
            }

            SetValidDomainComputeParameters();
            float interval = 1f / Mathf.Max(1f, ResolveSimulationRate());
            simulationAccumulator = Mathf.Min(
                simulationAccumulator + deltaTime,
                interval * 2.5f);

            int stepCount = 0;
            while (simulationAccumulator >= interval && stepCount < 2)
            {
                SimulateStep(interval, now);
                simulationAccumulator -= interval;
                stepCount++;
            }

            if (simulationAccumulator >= interval)
            {
                simulationAccumulator = 0f;
            }

            simulationInterpolation = Mathf.Clamp01(
                simulationAccumulator / interval);
            wakeInterpolation = simulationInterpolation;

            BindField();
        }

        public void NotifyRiverChanged()
        {
            resourcesDirty = true;
            staticPressureTargetDirty = true;
            staticWakeSourceDirty = true;
            rippleBoundaryDirty = true;
            generatedGeometryRegistryDirty = true;
        }

        public void ClearField()
        {
            if (computeShader != null)
            {
                DispatchClear(stateA, fieldWidth, fieldHeight, 0, fieldWidth);
                DispatchClear(stateB, fieldWidth, fieldHeight, 0, fieldWidth);
                DispatchClear(wakeA, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
                DispatchClear(wakeB, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
            }

            Array.Clear(chunkActive, 0, chunkActive.Length);
            Array.Clear(chunkActiveUntil, 0, chunkActiveUntil.Length);
            Array.Clear(wakeChunkActive, 0, wakeChunkActive.Length);
            Array.Clear(wakeChunkActiveUntil, 0, wakeChunkActiveUntil.Length);
            Array.Clear(chunkHasStaticSource, 0, chunkHasStaticSource.Length);
            Array.Clear(
                staticWakeChunkReleaseDuration,
                0,
                staticWakeChunkReleaseDuration.Length);
            staticWakeSourceDirty = true;
            pendingImpacts.Clear();
            activeImpactReservations.Clear();
            impactsInjectedLastStep = 0;
            currentRippleSubstepCount = 0;
            maximumRecentRippleSubstepCount = 0;
            activeRippleMinimumCellSize = 0f;
            rippleSubstepLimitReached = false;
            rippleSubstepDiagnosticWindowStart = 0.0;
            simulationAccumulator = 0f;
            staticWakeVariationAccumulator = 0f;
            simulationInterpolation = 1f;
            wakeInterpolation = 1f;
        }






        /// <summary>
        /// Copies the exact generated meshes currently registered as static
        /// river obstructions. Foam prepares and caches exact solid intervals
        /// from these meshes during its staged pre-gameplay build.
        ///
        /// TODO(PROCEDURAL-CHUNK-BUILD): transfer ownership of this preparation
        /// to the future chunk generation/building/linking phase after all
        /// generated objects have received their final transforms. The runtime
        /// method remains the temporary development fallback until that phase
        /// exists.
        /// </summary>

        /// <summary>
        /// Synchronously resolves the existing generated-geometry registry for
        /// release-cache validation. Normal runtime refresh remains frame-budgeted;
        /// this validation-only path is invoked by Editor build preflight and does
        /// not allocate disturbance textures or alter authored scene data.
        /// </summary>

        /// <summary>
        /// Copies exact fingerprints already prepared by registered generated
        /// static obstacle owners. This cache-first path never rereads mesh
        /// triangles. If one participating source lacks the provider contract,
        /// the complete set is rejected rather than partially validated.
        /// </summary>



        private void ReportOwnershipConflict(EntityId ownerId)
        {
            if (!ownershipConflictWarningOwnerIds.Add(ownerId))
            {
                return;
            }

            Debug.LogWarning(
                $"River disturbance continuous-source ownership conflict " +
                $"on '{name}' for physical owner {ownerId}. Generated " +
                "stationary geometry takes precedence, and a second " +
                "continuous source for the same GameObject was rejected.",
                this);
        }

        private static bool EntityIdsEqual(EntityId left, EntityId right)
        {
            return EqualityComparer<EntityId>.Default.Equals(left, right);
        }






        public static bool TryFindContainingRiver(
            Vector3 worldPosition,
            float maximumVerticalDistance,
            out StylizedRiverDisturbanceRuntime runtime,
            out StylizedRiverProjection projection)
        {
            runtime = null;
            projection = default;
            float bestVerticalDistance = float.PositiveInfinity;

            for (int index = ActiveRuntimes.Count - 1; index >= 0; index--)
            {
                StylizedRiverDisturbanceRuntime candidate =
                    ActiveRuntimes[index];

                if (candidate == null)
                {
                    ActiveRuntimes.RemoveAt(index);
                    continue;
                }

                if (!candidate.isActiveAndEnabled)
                {
                    continue;
                }

                StylizedRiver candidateRiver = candidate.river;
                if (candidateRiver == null ||
                    !candidateRiver.isActiveAndEnabled ||
                    !candidateRiver.RuntimeDisturbancesEnabled ||
                    !candidateRiver.TryProjectWorldPoint(
                        worldPosition,
                        out StylizedRiverProjection candidateProjection) ||
                    !candidateProjection.IsInside)
                {
                    continue;
                }

                float verticalDistance = Mathf.Abs(
                    worldPosition.y -
                    candidateProjection.SurfacePoint.y);

                if (verticalDistance > maximumVerticalDistance ||
                    verticalDistance >= bestVerticalDistance)
                {
                    continue;
                }

                runtime = candidate;
                projection = candidateProjection;
                bestVerticalDistance = verticalDistance;
            }

            return runtime != null;
        }

        private void HandleDomainChanged(RiverDomainSnapshot snapshot)
        {
            resourcesDirty = true;
            staticPressureTargetDirty = true;
            staticWakeSourceDirty = true;
            rippleBoundaryDirty = true;
            generatedGeometryRegistryDirty = true;
        }












        private void SimulateStep(float deltaTime, double now)
        {
            // TODO: Tighten these broad dirty flags so source/profile changes
            // rebuild only affected Static Pressure, Static Wake, and ripple
            // boundary textures instead of whole-pass targets.
            if (staticPressureTargetDirty)
            {
                RebuildStaticPressureTarget(now);
            }

            if (staticWakeSourceDirty)
            {
                RebuildStaticWakeSource(now);
            }

            if (rippleBoundaryDirty)
            {
                RebuildRippleBoundary(now);
            }

            float reservationLookAhead =
                ResolveImpactReservationLookAhead(deltaTime);
            ResetRippleChunkReservationDeadlines(now);
            UpdateImpactReservations(
                now,
                deltaTime,
                reservationLookAhead);
            ExpireChunks(now);
            ExpireWakeChunks(now);

            impactsInjectedLastStep = pendingImpacts.Count;
            for (int index = 0; index < pendingImpacts.Count; index++)
            {
                ImpactCommand impact = pendingImpacts[index];
                ImpactReservation reservation =
                    CreateImpactReservation(impact, now);
                if (UpdateImpactReservation(
                        ref reservation,
                        now,
                        0f,
                        reservationLookAhead))
                {
                    activeImpactReservations.Add(reservation);
                }
                DispatchRippleInjection(impact);
            }

            pendingImpacts.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (source.IsStatic)
                {
                    continue;
                }

                float absoluteFlowSpeed =
                    Mathf.Abs(river.FlowSpeedMetresPerSecond);
                float movementBlend = source.StationaryObstruction
                    ? Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.InverseLerp(
                            StationarySpeedStart,
                            MovingSpeedFull,
                            source.MovementSpeed))
                    : 1f;
                float flowInfluence = Mathf.Lerp(
                    0.35f,
                    1.25f,
                    Mathf.InverseLerp(0f, 2.5f, absoluteFlowSpeed));
                float movementInfluence = Mathf.Lerp(
                    0.45f,
                    1.55f,
                    Mathf.InverseLerp(0f, 3f, source.MovementSpeed));
                // Source-local strength is multiplied by the river's
                // canonical Wake Strength; there is no separate dynamic-wake
                // visual rule or source-specific response afterward.
                float wakeStrength =
                    source.Strength *
                    river.WakeStrength *
                    Mathf.Clamp01(source.NormalContribution) *
                    flowInfluence *
                    Mathf.Lerp(0.65f, movementInfluence, movementBlend);

                float segmentCentre =
                    (source.StartDistance + source.EndDistance) * 0.5f;
                float segmentHalfLength = Mathf.Abs(
                    source.EndDistance - source.StartDistance) * 0.5f;
                // Dynamic emitters prepare a swept source footprint, while
                // stationary geometry prepares cached contour releases. Both
                // then consume the same canonical Wake response settings.
                float wakeReach = ResolveObstructionWakeLength(
                    source.AcrossHalfWidth,
                    source.AlongHalfLength,
                    absoluteFlowSpeed) * river.WakeReach;
                MarkWakeActive(
                    segmentCentre + wakeReach * 0.5f,
                    segmentHalfLength + wakeReach * 0.5f +
                    Mathf.Max(source.AcrossHalfWidth, source.AlongHalfLength),
                    now);

                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(source.EndDistance);
                float surfaceHalf = Mathf.Max(
                    0.05f,
                    sample.GetSurfaceHalfWidth(source.EndAcrossNormalized));

                DispatchWakeInjection(
                    source,
                    surfaceHalf,
                    wakeStrength,
                    movementBlend,
                    deltaTime);
            }

            SimulateRippleField(deltaTime);
            SimulateWakeField(deltaTime, now);
            simulationInterpolation = 0f;
            wakeInterpolation = 0f;
        }





































        private static RiverDisturbancePressureBakeProfile
            ClonePressureProfile(
                RiverDisturbancePressureBakeProfile source)
        {
            if (!source.IsValid)
            {
                return default;
            }

            Vector4[] samples = new Vector4[source.Samples.Length];
            Array.Copy(source.Samples, samples, source.Samples.Length);
            float[] downstreamBoundaries = source.HasGeometryBounds
                ? new float[source.DownstreamBoundaries.Length]
                : Array.Empty<float>();
            if (downstreamBoundaries.Length > 0)
            {
                Array.Copy(
                    source.DownstreamBoundaries,
                    downstreamBoundaries,
                    source.DownstreamBoundaries.Length);
            }

            return new RiverDisturbancePressureBakeProfile(
                source.AcrossHalfWidth,
                source.LateralSampleCount,
                samples,
                downstreamBoundaries);
        }



        private static StaticWakeLeeVariationState
            CreateStaticWakeLeeVariationState(int sampleCount)
        {
            int resolvedSampleCount =
                RiverDisturbanceFootprintResolver.
                    ResolvePressureSupportLateralSampleCount(sampleCount);
            return new StaticWakeLeeVariationState
            {
                SampleCount = resolvedSampleCount,
                CurrentDepthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                TransitionStartDepthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                TargetDepthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                CurrentLengthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                TransitionStartLengthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                TargetLengthMultipliers = CreateFilledFloatArray(
                    resolvedSampleCount,
                    1f),
                CurrentTrailingEdgeOffsets = new float[
                    resolvedSampleCount],
                TransitionStartTrailingEdgeOffsets = new float[
                    resolvedSampleCount],
                TargetTrailingEdgeOffsets = new float[
                    resolvedSampleCount],
                RawScratch = new float[resolvedSampleCount],
                SmoothedScratch = new float[resolvedSampleCount],
                Transition = 1f,
                TransitionDuration = 0f,
                SelectedInterval = 0f,
                EventIndex = 0u,
                NextEventTime = 0.0,
                ScheduleInitialized = false,
                ProfileFamily = 0
            };
        }

        private static StaticWakeReleaseVariationState
            CreateStaticWakeReleaseVariationState()
        {
            return new StaticWakeReleaseVariationState
            {
                CurrentLateralOffset = 0f,
                TransitionStartLateralOffset = 0f,
                TargetLateralOffset = 0f,
                CurrentEnergyMultiplier = 1f,
                TransitionStartEnergyMultiplier = 1f,
                TargetEnergyMultiplier = 1f,
                CurrentWidthMultiplier = 1f,
                TransitionStartWidthMultiplier = 1f,
                TargetWidthMultiplier = 1f,
                CurrentDownstreamOffset = 0f,
                TransitionStartDownstreamOffset = 0f,
                TargetDownstreamOffset = 0f,
                Transition = 1f,
                TransitionDuration = 0f,
                SelectedInterval = 0f,
                EventIndex = 0u,
                NextEventTime = 0.0,
                ScheduleInitialized = false
            };
        }

        private static float[] CreateFilledFloatArray(
            int length,
            float value)
        {
            float[] result = new float[Mathf.Max(0, length)];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = value;
            }
            return result;
        }




        private void BeginPerformanceDiagnosticsUpdate()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            if (performanceDiagnosticWindowStart <= 0.0 ||
                now - performanceDiagnosticWindowStart >=
                PerformanceDiagnosticWindowSeconds)
            {
                performanceDiagnosticWindowStart = now;
                recentPeakComputeDispatchCount = 0;
                recentPeakThreadGroupCount = 0L;
                recentPeakCellIterationCount = 0L;
                recentPeakFieldRebuildCount = 0;
            }

            lastUpdateComputeDispatchCount = 0;
            lastUpdateThreadGroupCount = 0L;
            lastUpdateCellIterationCount = 0L;
            lastUpdateRippleSimulationDispatchCount = 0;
            lastUpdateWakeSimulationDispatchCount = 0;
            lastUpdateImpactInjectionDispatchCount = 0;
            lastUpdateWakeInjectionDispatchCount = 0;
            lastUpdateStaticPressureBakeDispatchCount = 0;
            lastUpdateStaticWakeBakeDispatchCount = 0;
            lastUpdateRippleBoundaryBakeDispatchCount = 0;
            lastUpdateClearDispatchCount = 0;
            lastUpdateFieldRebuildCount = 0;
        }

        public void ResetPerformanceDiagnosticPeaks()
        {
            performanceDiagnosticWindowStart =
                Time.realtimeSinceStartupAsDouble;
            recentPeakComputeDispatchCount = 0;
            recentPeakThreadGroupCount = 0L;
            recentPeakCellIterationCount = 0L;
            recentPeakFieldRebuildCount = 0;
        }

        private void RecordFieldRebuild()
        {
            lastUpdateFieldRebuildCount++;
            recentPeakFieldRebuildCount = Mathf.Max(
                recentPeakFieldRebuildCount,
                lastUpdateFieldRebuildCount);
        }








        private void MarkActiveInterval(
            float minimumGlobalDistance,
            float maximumGlobalDistance,
            double activeUntil,
            double now)
        {
            float domainMinimum = river.Domain.GlobalDistanceMinimum;
            float minimumLocalDistance = Mathf.Clamp(
                minimumGlobalDistance - domainMinimum,
                0f,
                validFieldLength);
            float maximumLocalDistance = Mathf.Clamp(
                maximumGlobalDistance - domainMinimum,
                0f,
                validFieldLength);
            if (maximumLocalDistance < minimumLocalDistance)
            {
                float swap = minimumLocalDistance;
                minimumLocalDistance = maximumLocalDistance;
                maximumLocalDistance = swap;
            }

            int firstChunk = Mathf.Clamp(
                Mathf.FloorToInt(
                    minimumLocalDistance / ChunkLengthMetres),
                0,
                chunkCount - 1);
            int lastChunk = Mathf.Clamp(
                Mathf.FloorToInt(
                    maximumLocalDistance / ChunkLengthMetres),
                0,
                chunkCount - 1);

            for (int chunk = firstChunk; chunk <= lastChunk; chunk++)
            {
                if (!chunkActive[chunk])
                {
                    int xOffset = chunk * resolutionPerChunk;
                    DispatchClear(
                        stateA,
                        fieldWidth,
                        fieldHeight,
                        xOffset,
                        resolutionPerChunk);
                    DispatchClear(
                        stateB,
                        fieldWidth,
                        fieldHeight,
                        xOffset,
                        resolutionPerChunk);
                    chunkActive[chunk] = true;
                }

                chunkActiveUntil[chunk] = Math.Max(
                    chunkActiveUntil[chunk],
                    activeUntil);
            }

            lastActivityTime = Math.Max(lastActivityTime, now);
        }






        private float ResolveSimulationRate()
        {
            float qualityRate = river != null
                ? river.Quality switch
                {
                    StylizedRiverQuality.Low => 12f,
                    StylizedRiverQuality.Medium => 20f,
                    StylizedRiverQuality.High => 30f,
                    _ => 20f
                }
                : 20f;

            return HasStaticSources() &&
                   !HasDynamicSources() &&
                   pendingImpacts.Count == 0 &&
                   activeImpactReservations.Count == 0 &&
                   !HasRippleActiveChunks()
                ? Mathf.Min(qualityRate, StaticOnlySimulationRate)
                : qualityRate;
        }



        private float ResolveAverageSurfaceHalfWidth()
        {
            if (river == null || !river.Domain.IsValid)
            {
                return 1f;
            }

            double sum = 0.0;
            for (int index = 0; index < river.Domain.SampleCount; index++)
            {
                StylizedRiverSplineSample sample = river.Domain.Samples[index];
                sum +=
                    (sample.LeftSurfaceHalfWidth +
                     sample.RightSurfaceHalfWidth) * 0.5;
            }

            return Mathf.Max(
                0.25f,
                (float)(sum / river.Domain.SampleCount));
        }


















    }
}
