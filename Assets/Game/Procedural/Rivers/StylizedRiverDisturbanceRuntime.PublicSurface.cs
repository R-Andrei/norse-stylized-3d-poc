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
        public bool IsSupported =>
            SystemInfo.supportsComputeShaders &&
            SystemInfo.SupportsRenderTextureFormat(
                RenderTextureFormat.ARGBHalf) &&
            SystemInfo.SupportsRenderTextureFormat(
                RenderTextureFormat.RGHalf);
        public bool IsAllocated => currentState != null;
        public bool IsSleeping =>
            !HasActiveChunks() &&
            continuousSources.Count == 0 &&
            pendingImpacts.Count == 0 &&
            activeImpactReservations.Count == 0;
        public int FieldWidth => fieldWidth;
        public int FieldHeight => fieldHeight;
        public int ChunkCount => chunkCount;
        public int ActiveChunkCount => CountActiveChunks();
        public int WakeFieldWidth => wakeFieldWidth;
        public int WakeFieldHeight => wakeFieldHeight;
        public RenderTexture CurrentWakeTexture => currentWake;
        public RenderTexture CurrentRippleTexture => currentState;
        public RenderTexture StaticWakeSourceTexture => staticWakeSource;
        // Stage 6 consumes the already accepted stationary Pressure target as
        // a read-only Pressure Support input. Foam never writes to or reinterprets
        // the Stage 5 field.
        public RenderTexture StaticPressureTexture => staticTarget;
        public Vector2Int WakeTextureDimensions => currentWake != null
            ? new Vector2Int(currentWake.width, currentWake.height)
            : Vector2Int.one;
        public Vector2Int RippleTextureDimensions => currentState != null
            ? new Vector2Int(currentState.width, currentState.height)
            : Vector2Int.one;
        public Vector2Int StaticWakeTextureDimensions => staticWakeSource != null
            ? new Vector2Int(staticWakeSource.width, staticWakeSource.height)
            : Vector2Int.one;
        public Vector2Int StaticPressureTextureDimensions => staticTarget != null
            ? new Vector2Int(staticTarget.width, staticTarget.height)
            : Vector2Int.one;
        public int ActiveWakeChunkCount => CountActiveWakeChunks();
        public int ContinuousSourceCount => continuousSources.Count;

        public void CopyStaticObjectFoamSourcesTo(
            List<RiverFoamStaticObjectSource> output)
        {
            if (output == null)
            {
                return;
            }

            output.Clear();
            river ??= GetComponent<StylizedRiver>();
            if (river == null || !river.Domain.IsValid)
            {
                return;
            }

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                MeshFilter meshFilter = source.ObstacleExclusionMeshFilter;
                if (!source.IsStatic ||
                    !source.StationaryObstruction ||
                    meshFilter == null ||
                    meshFilter.sharedMesh == null ||
                    !meshFilter.gameObject.activeInHierarchy ||
                    source.StaticTargetHeightMetres <= 0.0001f ||
                    !river.TryProjectWorldPoint(
                        source.WorldPosition,
                        out StylizedRiverProjection projection) ||
                    !projection.IsInside)
                {
                    continue;
                }

                StylizedRiverSplineSample sample =
                    river.SampleAtLocalDistance(projection.LocalDistance);
                float surfaceHalfWidth = Mathf.Max(
                    0.05f,
                    sample.GetSurfaceHalfWidth(projection.AcrossMetres));
                float acrossNormalized = Mathf.Clamp(
                    projection.AcrossMetres / surfaceHalfWidth,
                    -1f,
                    1f);

                output.Add(new RiverFoamStaticObjectSource(
                    pair.Key,
                    source.OwnerId,
                    projection.GlobalDistance,
                    projection.AcrossMetres,
                    acrossNormalized,
                    surfaceHalfWidth,
                    Mathf.Max(0.05f, source.AlongHalfLength),
                    Mathf.Max(0.05f, source.AcrossHalfWidth),
                    Mathf.Max(0.05f, source.StaticPressureAlongHalfLength),
                    Mathf.Max(0.05f, source.StaticPressureAcrossHalfWidth),
                    Mathf.Max(0f, source.StaticTargetHeightMetres),
                    source.Phase));
            }

            output.Sort((left, right) =>
                left.SourceId.CompareTo(right.SourceId));
        }

        public int PendingImpactCount => pendingImpacts.Count;
        public int ActiveImpactReservationCount =>
            activeImpactReservations.Count;
        public float LongestImpactReservationRemainingSeconds =>
            ResolveLongestImpactReservationRemainingSeconds();
        public int ImpactsInjectedLastStep => impactsInjectedLastStep;
        public int CurrentRippleSubstepCount => currentRippleSubstepCount;
        public int MaximumRecentRippleSubstepCount =>
            maximumRecentRippleSubstepCount;
        public int RippleMetricRowCount =>
            rippleMetricBuffer != null ? fieldWidth : 0;
        public int RippleBoundaryWidth =>
            rippleBoundary != null ? rippleBoundary.width : 0;
        public int RippleBoundaryHeight =>
            rippleBoundary != null ? rippleBoundary.height : 0;
        public int RippleCollisionSourceCount =>
            rippleCollisionSourceCount;
        public float ActiveRippleMinimumCellSize =>
            activeRippleMinimumCellSize;
        public bool RippleSubstepLimitReached =>
            rippleSubstepLimitReached;
        public int RegisteredStationarySourceCount =>
            CountRegisteredStationarySources();
        public int ValidStaticPressureSourceCount => validStaticSourceCount;
        public int ValidStaticWakeSourceCount => validStaticWakeSourceCount;
        public int ObstacleGeometryVersion => obstacleGeometryVersion;
        public bool GeneratedObstacleRegistryReady =>
            (river != null && !river.RuntimeDisturbancesEnabled) ||
            (!generatedGeometryRegistryDirty &&
             !generatedGeometryRefreshInProgress);
        public int GeneratedObstacleRegistryProcessedCount =>
            generatedGeometryRefreshInProgress
                ? Mathf.Clamp(
                    generatedGeometryRefreshIndex,
                    0,
                    generatedGeometryScratch.Count)
                : automaticGeneratedSourceIds.Count;
        public int GeneratedObstacleRegistryTotalCount =>
            generatedGeometryRefreshInProgress
                ? generatedGeometryScratch.Count
                : automaticGeneratedSourceIds.Count;
        public int LastUpdateComputeDispatchCount =>
            lastUpdateComputeDispatchCount;
        public int RecentPeakComputeDispatchCount =>
            recentPeakComputeDispatchCount;
        public long LastUpdateThreadGroupCount =>
            lastUpdateThreadGroupCount;
        public long RecentPeakThreadGroupCount =>
            recentPeakThreadGroupCount;
        public long LastUpdateCellIterationCount =>
            lastUpdateCellIterationCount;
        public long RecentPeakCellIterationCount =>
            recentPeakCellIterationCount;
        public int LastUpdateRippleSimulationDispatchCount =>
            lastUpdateRippleSimulationDispatchCount;
        public int LastUpdateWakeSimulationDispatchCount =>
            lastUpdateWakeSimulationDispatchCount;
        public int LastUpdateImpactInjectionDispatchCount =>
            lastUpdateImpactInjectionDispatchCount;
        public int LastUpdateWakeInjectionDispatchCount =>
            lastUpdateWakeInjectionDispatchCount;
        public int LastUpdateStaticPressureBakeDispatchCount =>
            lastUpdateStaticPressureBakeDispatchCount;
        public int LastUpdateStaticWakeBakeDispatchCount =>
            lastUpdateStaticWakeBakeDispatchCount;
        public int LastUpdateRippleBoundaryBakeDispatchCount =>
            lastUpdateRippleBoundaryBakeDispatchCount;
        public int LastUpdateClearDispatchCount =>
            lastUpdateClearDispatchCount;
        public int LastUpdateFieldRebuildCount =>
            lastUpdateFieldRebuildCount;
        public int RecentPeakFieldRebuildCount =>
            recentPeakFieldRebuildCount;
        public long RippleStateMemoryBytes =>
            IsAllocated ? (long)fieldWidth * fieldHeight * 8L * 2L : 0L;
        public long StaticPressureMemoryBytes =>
            IsAllocated ? (long)fieldWidth * fieldHeight * 8L : 0L;
        public long RippleBoundaryMemoryBytes =>
            rippleBoundary != null
                ? (long)fieldWidth * fieldHeight * 4L
                : 0L;
        public long WakeFieldMemoryBytes =>
            IsAllocated
                ? (long)wakeFieldWidth * wakeFieldHeight * 8L * 3L
                : 0L;
        public long RippleMetricMemoryBytes =>
            rippleMetricBuffer != null ? (long)fieldWidth * 32L : 0L;
        public float SimulationRate => ResolveSimulationRate();
        public float WakeSimulationRate => ResolveSimulationRate();
        public long EstimatedMemoryBytes =>
            RippleStateMemoryBytes +
            StaticPressureMemoryBytes +
            RippleBoundaryMemoryBytes +
            WakeFieldMemoryBytes +
            RippleMetricMemoryBytes;
    }
}
