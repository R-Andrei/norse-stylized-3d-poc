using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverDisturbanceRuntime
    {
        public bool RegisterStaticSource(
            EntityId sourceId,
            EntityId ownerId,
            Vector3 worldPosition,
            float acrossHalfWidth,
            float alongHalfLength,
            float strength,
            float geometryContribution,
            float normalContribution,
            float targetHeightFraction = -1f,
            float staticWakeAmplitude = -1f,
            float responseStiffness = 1f,
            float wakeReachMultiplier = 1f,
            float unsteadiness = 1f,
            IReadOnlyList<Vector2> contour = null,
            float explicitTargetHeightMetres = -1f,
            float pressureAcrossHalfWidth = -1f,
            float pressureAlongHalfLength = -1f,
            IReadOnlyList<Vector2> pressureContour = null,
            RiverDisturbancePressureBakeProfile pressureProfile = default,
            MeshFilter obstacleExclusionMeshFilter = null,
            bool deferStaticTargetRebuild = false,
            float wakeSpreadMultiplier = 1f,
            float profileChangeIntervalMin =
                StylizedRiver.DefaultStaticPressureProfileChangeIntervalMin,
            float profileChangeIntervalMax =
                StylizedRiver.DefaultStaticPressureProfileChangeIntervalMax,
            float wakeVariation = 0.35f,
            float wakeVariationIntervalMin =
                StylizedRiver.DefaultStaticWakeVariationIntervalMin,
            float wakeVariationIntervalMax =
                StylizedRiver.DefaultStaticWakeVariationIntervalMax,
            bool rippleCollisionEnabled = true,
            float rippleCollisionAcrossHalfWidth = -1f,
            float rippleCollisionAlongHalfLength = -1f,
            IReadOnlyList<Vector2> rippleCollisionContour = null,
            IGeneratedGeometryStableFingerprintSource
                obstacleExclusionFingerprintSource = null)
        {
            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.RuntimeDisturbancesEnabled ||
                !river.TryProjectWorldPoint(
                    worldPosition,
                    out StylizedRiverProjection projection) ||
                !projection.IsInside)
            {
                RemoveContinuousSource(sourceId);
                return false;
            }

            if (!TryClaimContinuousSourceOwner(
                    ownerId,
                    sourceId,
                    true))
            {
                RemoveContinuousSource(sourceId);
                return false;
            }

            StylizedRiverSplineSample sample =
                river.SampleAtLocalDistance(projection.LocalDistance);
            float surfaceHalfWidth = Mathf.Max(
                0.05f,
                sample.GetSurfaceHalfWidth(projection.AcrossMetres));
            float phase = ResolveSourcePhase(sourceId);
            float resolvedHeightMetres = explicitTargetHeightMetres >= 0f
                ? Mathf.Clamp(
                    explicitTargetHeightMetres,
                    0f,
                    MaximumStaticPressureHeightMetres)
                : targetHeightFraction >= 0f
                    ? river.ResolvedImpactRippleMaximumHeight *
                      Mathf.Clamp01(targetHeightFraction)
                    : Mathf.Clamp(
                        Mathf.Max(0f, strength) *
                        Mathf.Clamp01(geometryContribution) *
                        0.040f,
                        0f,
                        MaximumStaticPressureHeightMetres);
            float resolvedWakeAmplitude = staticWakeAmplitude >= 0f
                ? Mathf.Max(0f, staticWakeAmplitude)
                : Mathf.Max(0f, strength) *
                  Mathf.Clamp01(normalContribution) *
                  0.22f;
            RiverDisturbancePressureBakeProfile basePressureProfile =
                ClonePressureProfile(pressureProfile);
            RiverDisturbancePressureBakeProfile animatedPressureProfile =
                ClonePressureProfile(pressureProfile);
            float[] currentProfileMultipliers =
                CreateUnitPressureProfileMultipliers(basePressureProfile);
            float[] transitionStartMultipliers =
                CreateUnitPressureProfileMultipliers(basePressureProfile);
            float[] targetProfileMultipliers =
                CreateUnitPressureProfileMultipliers(basePressureProfile);
            float[] rawProfileScratch =
                CreatePressureProfileScratch(basePressureProfile);
            float[] smoothedProfileScratch =
                CreatePressureProfileScratch(basePressureProfile);
            int wakeVariationSampleCount =
                ResolveStaticWakeVariationLateralSampleCount(
                    acrossHalfWidth,
                    surfaceHalfWidth * 2f);
            StaticWakeLeeVariationState wakeLeeVariation =
                CreateStaticWakeLeeVariationState(
                    wakeVariationSampleCount);
            StaticWakeReleaseVariationState leftWakeVariation =
                CreateStaticWakeReleaseVariationState();
            StaticWakeReleaseVariationState rightWakeVariation =
                CreateStaticWakeReleaseVariationState();

            continuousSources[sourceId] =
                new ContinuousSource
                {
                    WorldPosition = worldPosition,
                    StartDistance = projection.GlobalDistance,
                    EndDistance = projection.GlobalDistance,
                    StartAcrossNormalized = Mathf.Clamp(
                        projection.AcrossMetres / surfaceHalfWidth,
                        -1f,
                        1f),
                    EndAcrossNormalized = Mathf.Clamp(
                        projection.AcrossMetres / surfaceHalfWidth,
                        -1f,
                        1f),
                    AcrossHalfWidth = Mathf.Max(
                        0.05f,
                        acrossHalfWidth),
                    AlongHalfLength = Mathf.Max(
                        0.05f,
                        alongHalfLength),
                    Strength = Mathf.Max(0f, strength),
                    GeometryContribution = Mathf.Clamp01(
                        geometryContribution),
                    NormalContribution = Mathf.Clamp01(
                        normalContribution),
                    StaticTargetHeightMetres = resolvedHeightMetres,
                    StaticPressureAcrossHalfWidth = pressureAcrossHalfWidth > 0f
                        ? Mathf.Max(0.05f, pressureAcrossHalfWidth)
                        : Mathf.Max(0.05f, acrossHalfWidth),
                    StaticPressureAlongHalfLength = pressureAlongHalfLength > 0f
                        ? Mathf.Max(0.05f, pressureAlongHalfLength)
                        : Mathf.Max(0.05f, alongHalfLength),
                    StaticPressureContour = CopyStaticContour(
                        pressureContour ?? contour),
                    StaticPressureProfile = animatedPressureProfile,
                    StaticPressureBaseProfile = basePressureProfile,
                    ObstacleExclusionMeshFilter =
                        obstacleExclusionMeshFilter,
                    ObstacleExclusionFingerprintSource =
                        obstacleExclusionFingerprintSource,
                    StaticPressureCurrentMultipliers =
                        currentProfileMultipliers,
                    StaticPressureTransitionStartMultipliers =
                        transitionStartMultipliers,
                    StaticPressureTargetMultipliers =
                        targetProfileMultipliers,
                    StaticPressureRawScratch = rawProfileScratch,
                    StaticPressureSmoothedScratch =
                        smoothedProfileScratch,
                    StaticPressureProfileTransition = 1f,
                    StaticPressureProfileTransitionDuration = 0f,
                    StaticPressureProfileChangeIntervalMin = Mathf.Clamp(
                        Mathf.Min(
                            profileChangeIntervalMin,
                            profileChangeIntervalMax),
                        StylizedRiver.MinimumStaticPressureProfileChangeInterval,
                        StylizedRiver.MaximumStaticPressureProfileChangeInterval),
                    StaticPressureProfileChangeIntervalMax = Mathf.Clamp(
                        Mathf.Max(
                            profileChangeIntervalMin,
                            profileChangeIntervalMax),
                        StylizedRiver.MinimumStaticPressureProfileChangeInterval,
                        StylizedRiver.MaximumStaticPressureProfileChangeInterval),
                    StaticPressureProfileEventIndex = 0u,
                    StaticPressureNextProfileEventTime = 0.0,
                    StaticPressureProfileScheduleInitialized = false,
                    StaticWakeAmplitude = resolvedWakeAmplitude,
                    StaticContactSharpness = Mathf.Clamp(
                        responseStiffness,
                        0.5f,
                        4f),
                    StaticWakeReachMultiplier = Mathf.Clamp(
                        wakeReachMultiplier,
                        0.25f,
                        3f),
                    StaticWakeSpreadMultiplier = Mathf.Clamp(
                        wakeSpreadMultiplier,
                        0.5f,
                        2f),
                    StaticWakeVariation = Mathf.Clamp01(wakeVariation),
                    StaticWakeLeeVariation = wakeLeeVariation,
                    StaticWakeLeftReleaseVariation = leftWakeVariation,
                    StaticWakeRightReleaseVariation = rightWakeVariation,
                    StaticWakeVariationIntervalMin = Mathf.Clamp(
                        Mathf.Min(
                            wakeVariationIntervalMin,
                            wakeVariationIntervalMax),
                        StylizedRiver.MinimumStaticWakeVariationInterval,
                        StylizedRiver.MaximumStaticWakeVariationInterval),
                    StaticWakeVariationIntervalMax = Mathf.Clamp(
                        Mathf.Max(
                            wakeVariationIntervalMin,
                            wakeVariationIntervalMax),
                        StylizedRiver.MinimumStaticWakeVariationInterval,
                        StylizedRiver.MaximumStaticWakeVariationInterval),
                    StaticProfileVariation = Mathf.Clamp(
                        unsteadiness,
                        0f,
                        2f),
                    StaticContour = CopyStaticContour(contour),
                    RippleCollisionEnabled = rippleCollisionEnabled,
                    RippleCollisionAcrossHalfWidth =
                        rippleCollisionAcrossHalfWidth > 0f
                            ? Mathf.Max(
                                0.05f,
                                rippleCollisionAcrossHalfWidth)
                            : Mathf.Max(0.05f, acrossHalfWidth),
                    RippleCollisionAlongHalfLength =
                        rippleCollisionAlongHalfLength > 0f
                            ? Mathf.Max(
                                0.05f,
                                rippleCollisionAlongHalfLength)
                            : Mathf.Max(0.05f, alongHalfLength),
                    RippleCollisionContour = CopyStaticContour(
                        rippleCollisionContour ?? contour),
                    MovementSpeed = 0f,
                    Phase = phase,
                    OwnerId = ownerId,
                    IsStatic = true,
                    StationaryObstruction = true,
                    LastSeen = double.PositiveInfinity
                };

            if (!deferStaticTargetRebuild)
            {
                staticPressureTargetDirty = true;
                staticWakeSourceDirty = true;
                rippleBoundaryDirty = true;
            }

            lastActivityTime = Time.realtimeSinceStartupAsDouble;
            return true;
        }

        public bool UpdateContinuousSource(
            EntityId sourceId,
            EntityId ownerId,
            Vector3 previousWorldPosition,
            Vector3 currentWorldPosition,
            float sampleDeltaTime,
            float acrossHalfWidth,
            float alongHalfLength,
            float strength,
            float geometryContribution,
            float normalContribution,
            bool stationaryObstruction)
        {
            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.RuntimeDisturbancesEnabled ||
                !river.TryProjectWorldPoint(
                    currentWorldPosition,
                    out StylizedRiverProjection currentProjection) ||
                !currentProjection.IsInside)
            {
                RemoveContinuousSource(sourceId);
                return false;
            }

            if (!TryClaimContinuousSourceOwner(
                    ownerId,
                    sourceId,
                    false))
            {
                RemoveContinuousSource(sourceId);
                return false;
            }

            bool previousValid =
                river.TryProjectWorldPoint(
                    previousWorldPosition,
                    out StylizedRiverProjection previousProjection) &&
                previousProjection.IsInside;

            if (!previousValid)
            {
                previousProjection = currentProjection;
            }

            StylizedRiverSplineSample currentSample =
                river.SampleAtLocalDistance(
                    currentProjection.LocalDistance);
            StylizedRiverSplineSample previousSample =
                river.SampleAtLocalDistance(
                    previousProjection.LocalDistance);

            float currentSurfaceHalf = Mathf.Max(
                0.05f,
                currentSample.GetSurfaceHalfWidth(
                    currentProjection.AcrossMetres));
            float previousSurfaceHalf = Mathf.Max(
                0.05f,
                previousSample.GetSurfaceHalfWidth(
                    previousProjection.AcrossMetres));

            float riverSpaceTravel = new Vector2(
                currentProjection.GlobalDistance -
                previousProjection.GlobalDistance,
                currentProjection.AcrossMetres -
                previousProjection.AcrossMetres).magnitude;

            if (continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource previousSource) &&
                previousSource.IsStatic)
            {
                staticPressureTargetDirty = true;
                staticWakeSourceDirty = true;
            }

            continuousSources[sourceId] =
                new ContinuousSource
                {
                    WorldPosition = currentWorldPosition,
                    StartDistance = previousProjection.GlobalDistance,
                    EndDistance = currentProjection.GlobalDistance,
                    StartAcrossNormalized = Mathf.Clamp(
                        previousProjection.AcrossMetres /
                        previousSurfaceHalf,
                        -1f,
                        1f),
                    EndAcrossNormalized = Mathf.Clamp(
                        currentProjection.AcrossMetres /
                        currentSurfaceHalf,
                        -1f,
                        1f),
                    AcrossHalfWidth = Mathf.Max(
                        0.05f,
                        acrossHalfWidth),
                    AlongHalfLength = Mathf.Max(
                        0.05f,
                        alongHalfLength),
                    Strength = Mathf.Max(0f, strength),
                    GeometryContribution = Mathf.Clamp01(
                        geometryContribution),
                    NormalContribution = Mathf.Clamp01(
                        normalContribution),
                    StaticTargetHeightMetres = 0f,
                    StaticWakeAmplitude = 0f,
                    StaticContactSharpness = 1f,
                    StaticWakeReachMultiplier = 1f,
                    StaticWakeSpreadMultiplier = 1f,
                    StaticProfileVariation = 1f,
                    StaticContour = Array.Empty<Vector2>(),
                    RippleCollisionEnabled = false,
                    RippleCollisionAcrossHalfWidth = 0f,
                    RippleCollisionAlongHalfLength = 0f,
                    RippleCollisionContour = Array.Empty<Vector2>(),
                    MovementSpeed =
                        riverSpaceTravel /
                        Mathf.Max(0.001f, sampleDeltaTime),
                    Phase = ResolveSourcePhase(sourceId),
                    OwnerId = ownerId,
                    IsStatic = false,
                    StationaryObstruction = stationaryObstruction,
                    LastSeen = Time.realtimeSinceStartupAsDouble
                };

            lastActivityTime = Time.realtimeSinceStartupAsDouble;
            return true;
        }

        public bool ContainsContinuousSource(EntityId sourceId)
        {
            return continuousSources.ContainsKey(sourceId);
        }

        public void RemoveContinuousSource(EntityId sourceId)
        {
            if (!continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource source))
            {
                return;
            }

            if (source.IsStatic)
            {
                staticPressureTargetDirty = true;
                staticWakeSourceDirty = true;
                rippleBoundaryDirty = true;
            }

            if (continuousSourceIdsByOwner.TryGetValue(
                    source.OwnerId,
                    out EntityId ownedSourceId) &&
                EntityIdsEqual(ownedSourceId, sourceId))
            {
                continuousSourceIdsByOwner.Remove(source.OwnerId);
                ownershipConflictWarningOwnerIds.Remove(source.OwnerId);
            }

            continuousSources.Remove(sourceId);
        }

        private bool TryClaimContinuousSourceOwner(
            EntityId ownerId,
            EntityId sourceId,
            bool staticRegistrySource)
        {
            if (continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource previousSource) &&
                !EntityIdsEqual(previousSource.OwnerId, ownerId) &&
                continuousSourceIdsByOwner.TryGetValue(
                    previousSource.OwnerId,
                    out EntityId previousOwnerSourceId) &&
                EntityIdsEqual(previousOwnerSourceId, sourceId))
            {
                continuousSourceIdsByOwner.Remove(previousSource.OwnerId);
            }

            if (continuousSourceIdsByOwner.TryGetValue(
                    ownerId,
                    out EntityId existingSourceId) &&
                !EntityIdsEqual(existingSourceId, sourceId))
            {
                if (!continuousSources.TryGetValue(
                        existingSourceId,
                        out ContinuousSource existingSource))
                {
                    continuousSourceIdsByOwner.Remove(ownerId);
                }
                else if (staticRegistrySource && !existingSource.IsStatic)
                {
                    RemoveContinuousSource(existingSourceId);
                }
                else
                {
                    ReportOwnershipConflict(ownerId);
                    return false;
                }
            }

            continuousSourceIdsByOwner[ownerId] = sourceId;
            ownershipConflictWarningOwnerIds.Remove(ownerId);
            return true;
        }

        private float ResolveSourcePhase(EntityId sourceId)
        {
            if (continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource source))
            {
                return source.Phase;
            }

            float phase = Mathf.Repeat(
                sourcePhaseSequence * GoldenPhaseStep,
                1f);
            sourcePhaseSequence++;
            return phase;
        }

        private void CleanupStaleSources(double now)
        {
            staleSourceIds.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in continuousSources)
            {
                if (!pair.Value.IsStatic &&
                    now - pair.Value.LastSeen > SourceStaleSeconds)
                {
                    staleSourceIds.Add(pair.Key);
                }
            }

            for (int index = 0; index < staleSourceIds.Count; index++)
            {
                RemoveContinuousSource(staleSourceIds[index]);
            }
        }

        private bool HasStaticSources()
        {
            foreach (ContinuousSource source in continuousSources.Values)
            {
                if (source.IsStatic)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasDynamicSources()
        {
            foreach (ContinuousSource source in continuousSources.Values)
            {
                if (!source.IsStatic)
                {
                    return true;
                }
            }

            return false;
        }

        private int CountRegisteredStationarySources()
        {
            int count = 0;
            foreach (ContinuousSource source in continuousSources.Values)
            {
                if (source.IsStatic)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
