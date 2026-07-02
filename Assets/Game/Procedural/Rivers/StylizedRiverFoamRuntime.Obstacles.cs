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
        private void RequestBoundaryRebuild()
        {
            pendingBoundaryRebuild = true;
            pendingTopologyReplacementAfterMaintenance =
                DevelopmentTopologyGenerationInProgress;
            pendingTopologyRefresh = true;
        }

        private void RequestObstacleRebuild(
            int currentObstacleVersion,
            bool prepareTopologyReplacement)
        {
            pendingObstacleRebuild = true;
            pendingTopologyReplacementAfterMaintenance |=
                prepareTopologyReplacement;
            pendingTopologyRefresh = true;

            if (pendingObstacleObservedVersion != currentObstacleVersion)
            {
                pendingObstacleObservedVersion = currentObstacleVersion;
                pendingObstacleStableFrameCount = 0;
            }
        }

        private void QueueObstacleRebuildIfNeeded()
        {
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            int currentObstacleVersion = disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;

            if (currentObstacleVersion == obstacleGeometryVersion)
            {
                return;
            }

            if (DevelopmentTopologyGenerationInProgress)
            {
                RequestObstacleRebuild(
                    currentObstacleVersion,
                    true);
                return;
            }

            activeTopologyObstacleStale = true;
            if (IsAutomaticDevelopmentCacheEnabled)
            {
                automaticTopologyGenerationInProgress = true;
                automaticDevelopmentRebuildReason =
                    AutomaticDevelopmentRebuildReason.Obstacles;
                topologyCacheStartupState =
                    "Using Previous Cache — Rebuilding";
                topologyCacheStartupSummary =
                    "Static obstacle geometry changed. The previous generated " +
                    "topology remains visible while the exact live obstacle " +
                    "field and a complete replacement are prepared " +
                    "automatically.";
                RequestObstacleRebuild(
                    currentObstacleVersion,
                    true);
                return;
            }

            MarkActiveTopologyCacheStale(
                "Stale — Obstacles Changed",
                "Static obstacle geometry changed after activation. The " +
                "exact live obstacle field is refreshed, while the generated " +
                "topology is retained until explicit development regeneration " +
                "or a valid cache reload.");
            RequestObstacleRebuild(
                currentObstacleVersion,
                false);
        }

        private int ResolveCurrentObstacleGeometryVersion()
        {
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            return disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;
        }

        private void RebuildBoundaryTexture(bool applyToExistingState = true)
        {
            using var profilerScope = InitBuildBoundaryProfilerMarker.Auto();
            if (fieldWidth <= 0 || fieldHeight <= 0 || metricRows.Length != fieldWidth)
            {
                return;
            }

            Color[] pixels = new Color[fieldWidth * fieldHeight];
            float edgeCells = river.Quality switch
            {
                StylizedRiverQuality.Low => 1.5f,
                StylizedRiverQuality.Medium => 2.0f,
                StylizedRiverQuality.High => 2.5f,
                _ => 2.0f
            };

            for (int x = 0; x < fieldWidth; x++)
            {
                float localDistance =
                    StylizedRiverFoamTopologyFieldSpace.LocalDistanceAtTexel(
                        x,
                        fieldWidth,
                        fieldLength);
                if (localDistance > simulationFieldLength + 0.0001f)
                {
                    continue;
                }

                float globalDistance =
                    river.Domain.GlobalDistanceMinimum +
                    Mathf.Min(localDistance, validFieldLength);
                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float leftSurface = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
                float rightSurface = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
                float leftVisible = Mathf.Max(0.01f, sample.LeftHalfWidth);
                float rightVisible = Mathf.Max(0.01f, sample.RightHalfWidth);
                float animatedEnvelope = Mathf.Lerp(
                    0.25f,
                    0.90f,
                    Mathf.Clamp01(river.ShoreMotion));
                float leftFoamReach = Mathf.Lerp(
                    leftVisible,
                    leftSurface,
                    animatedEnvelope);
                float rightFoamReach = Mathf.Lerp(
                    rightVisible,
                    rightSurface,
                    animatedEnvelope);
                float edgeWidth = Mathf.Max(
                    0.05f,
                    StylizedRiverFoamTopologyFieldSpace.TexelSpacing(
                        leftSurface + rightSurface,
                        fieldHeight) * edgeCells);

                for (int y = 0; y < fieldHeight; y++)
                {
                    float across01 =
                        StylizedRiverFoamTopologyFieldSpace.Across01AtTexel(
                            y,
                            fieldHeight);
                    float lateral =
                        StylizedRiverFoamTopologyFieldSpace.Across01ToMetres(
                        across01,
                        leftSurface,
                        rightSurface);
                    float foamReach = lateral < 0f
                        ? leftFoamReach
                        : rightFoamReach;
                    float distanceInsideReach = foamReach - Mathf.Abs(lateral);
                    float coverage = Mathf.Clamp01(
                        distanceInsideReach / edgeWidth);
                    float attraction = coverage *
                        (1f - Mathf.SmoothStep(
                            0f,
                            1f,
                            Mathf.InverseLerp(0.10f, 0.95f, coverage)));
                    // R/G remain the legacy material-simulation fluid and
                    // attraction contract. B/A are reserved zero: registered
                    // solids now use the full-resolution Obstacle Footprint
                    // mask. Canonical Shore Support comes from the
                    // instantaneous Stage 3 edge.
                    pixels[y * fieldWidth + x] = new Color(
                        coverage,
                        attraction,
                        0f,
                        0f);
                }
            }

            // Registered solid geometry is no longer rasterised into this
            // static boundary texture. Obstacle Footprint is reconstructed
            // from cached exact transformed-mesh solid intervals in a dedicated
            // point-sampled mask.

            if (boundaryTexture == null ||
                boundaryTexture.width != fieldWidth ||
                boundaryTexture.height != fieldHeight)
            {
                if (boundaryTexture != null)
                {
                    DestroyUnityObject(boundaryTexture);
                }

                boundaryTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RGBAHalf,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamBoundary_Runtime",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            boundaryTexture.SetPixels(pixels);
            boundaryTexture.Apply(false, false);
            boundaryDirty = false;

            if (applyToExistingState)
            {
                ApplyBoundaryToState(stateA);
                ApplyBoundaryToState(stateB);
            }
        }

        private void RebuildObstacleExclusionCache(
            bool captureScalarForTopology = true)
        {
            using var profilerScope = InitBuildObstacleExclusionProfilerMarker.Auto();
            obstacleExclusionUsesCachedScalar = false;
            activeTopologyObstacleStale = !captureScalarForTopology;
            if (captureScalarForTopology)
            {
                topologyCacheLoadedForActiveResources = false;
            }

            ReleaseObstacleExclusionBuffers();
            obstacleExclusionMeshFilters.Clear();
            obstacleExclusionCells.Clear();
            obstacleExclusionSamples.Clear();

            if (captureScalarForTopology)
            {
                int cellCount = Mathf.Max(0, fieldWidth * fieldHeight);
                if (obstacleExclusionScalar.Length != cellCount)
                {
                    obstacleExclusionScalar = new float[cellCount];
                }
                else
                {
                    Array.Clear(
                        obstacleExclusionScalar,
                        0,
                        obstacleExclusionScalar.Length);
                }
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            obstacleGeometryVersion = disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;

            if (disturbanceRuntime == null ||
                fieldWidth < 1 || fieldHeight < 1 ||
                fieldLength <= 0.0001f ||
                river == null || !river.Domain.IsValid)
            {
                ClearObstacleExclusionMask();
                return;
            }

            disturbanceRuntime.CopyObstacleExclusionMeshFiltersTo(
                obstacleExclusionMeshFilters);
            if (obstacleExclusionMeshFilters.Count == 0)
            {
                ClearObstacleExclusionMask();
                return;
            }

            // TODO(PROCEDURAL-CHUNK-BUILD): this exact triangle preparation is
            // the temporary development fallback. Move it into the future map
            // chunk generation/building/linking phase after procedural objects
            // have their final transforms, then load the cached compact cells
            // and intervals here instead of rescanning meshes during startup.
            for (int index = 0;
                 index < obstacleExclusionMeshFilters.Count;
                 index++)
            {
                RiverObstacleExclusionResolver.TryBake(
                    river,
                    obstacleExclusionMeshFilters[index],
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    obstacleExclusionCells,
                    obstacleExclusionSamples,
                    out _);
            }

            if (obstacleExclusionCells.Count == 0 ||
                obstacleExclusionSamples.Count == 0)
            {
                ClearObstacleExclusionMask();
                return;
            }

            if (obstacleExclusionGpuCells.Length !=
                obstacleExclusionCells.Count)
            {
                obstacleExclusionGpuCells =
                    new FoamObstacleIntervalCellData[
                        obstacleExclusionCells.Count];
            }

            for (int index = 0;
                 index < obstacleExclusionCells.Count;
                 index++)
            {
                RiverObstacleExclusionCell cell =
                    obstacleExclusionCells[index];
                obstacleExclusionGpuCells[index] =
                    new FoamObstacleIntervalCellData
                    {
                        CoordinateAndOffset = new Vector4(
                            cell.Coordinate.x,
                            cell.Coordinate.y,
                            cell.IntervalOffset,
                            0f)
                    };
            }

            obstacleExclusionCellBuffer = new ComputeBuffer(
                obstacleExclusionGpuCells.Length,
                sizeof(float) * 4,
                ComputeBufferType.Structured);
            obstacleExclusionCellBuffer.SetData(obstacleExclusionGpuCells);

            obstacleExclusionSampleBuffer = new ComputeBuffer(
                obstacleExclusionSamples.Count,
                sizeof(float) * 8,
                ComputeBufferType.Structured);
            obstacleExclusionSampleBuffer.SetData(obstacleExclusionSamples);

            ConfigureTopologyParameters(0f);
            UpdateObstacleExclusionMask();
            if (captureScalarForTopology)
            {
                ReadBackObstacleExclusionScalar();
            }
        }

        private void ReadBackObstacleExclusionScalar()
        {
            if (obstacleExclusionTexture == null ||
                obstacleExclusionScalar.Length != fieldWidth * fieldHeight)
            {
                return;
            }

            if (SystemInfo.supportsAsyncGPUReadback)
            {
                AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(
                    obstacleExclusionTexture,
                    0,
                    TextureFormat.RFloat,
                    null);
                request.WaitForCompletion();
                if (!request.hasError)
                {
                    var data = request.GetData<float>();
                    int count = Mathf.Min(
                        data.Length,
                        obstacleExclusionScalar.Length);
                    for (int index = 0; index < count; index++)
                    {
                        obstacleExclusionScalar[index] =
                            Mathf.Clamp01(data[index]);
                    }

                    return;
                }
            }

            if (obstacleExclusionReadbackTexture == null ||
                obstacleExclusionReadbackTexture.width != fieldWidth ||
                obstacleExclusionReadbackTexture.height != fieldHeight)
            {
                if (obstacleExclusionReadbackTexture != null)
                {
                    DestroyUnityObject(obstacleExclusionReadbackTexture);
                }

                obstacleExclusionReadbackTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RFloat,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamObstacleFootprint_Readback",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = obstacleExclusionTexture;
            obstacleExclusionReadbackTexture.ReadPixels(
                new Rect(0f, 0f, fieldWidth, fieldHeight),
                0,
                0,
                false);
            obstacleExclusionReadbackTexture.Apply(false, false);
            RenderTexture.active = previous;

            var fallbackData =
                obstacleExclusionReadbackTexture.GetPixelData<float>(0);
            int fallbackCount = Mathf.Min(
                fallbackData.Length,
                obstacleExclusionScalar.Length);
            for (int index = 0; index < fallbackCount; index++)
            {
                obstacleExclusionScalar[index] =
                    Mathf.Clamp01(fallbackData[index]);
            }
        }

        private void ReleaseObstacleExclusionBuffers()
        {
            obstacleExclusionCellBuffer?.Release();
            obstacleExclusionCellBuffer = null;
            obstacleExclusionSampleBuffer?.Release();
            obstacleExclusionSampleBuffer = null;

            if (obstacleExclusionReadbackTexture != null)
            {
                DestroyUnityObject(obstacleExclusionReadbackTexture);
                obstacleExclusionReadbackTexture = null;
            }

            if (obstacleExclusionUploadTexture != null)
            {
                DestroyUnityObject(obstacleExclusionUploadTexture);
                obstacleExclusionUploadTexture = null;
            }
        }

        private void ClearObstacleExclusionMask()
        {
            if (computeShader == null ||
                obstacleExclusionTexture == null ||
                clearObstacleExclusionKernel < 0)
            {
                return;
            }

            computeShader.SetInts(
                "_FoamDimensions",
                fieldWidth,
                fieldHeight);
            computeShader.SetTexture(
                clearObstacleExclusionKernel,
                "_FoamObstacleExclusionWrite",
                obstacleExclusionTexture);
            Dispatch(
                clearObstacleExclusionKernel,
                fieldWidth,
                fieldHeight);
        }

        private void UpdateObstacleExclusionMask()
        {
            if (obstacleExclusionUsesCachedScalar)
            {
                return;
            }

            ClearObstacleExclusionMask();
            if (computeShader == null ||
                updateObstacleExclusionKernel < 0 ||
                obstacleExclusionTexture == null ||
                obstacleExclusionCellBuffer == null ||
                obstacleExclusionSampleBuffer == null ||
                obstacleExclusionCells.Count == 0)
            {
                return;
            }

            computeShader.SetInt(
                "_FoamObstacleCellCount",
                obstacleExclusionCells.Count);
            computeShader.SetBuffer(
                updateObstacleExclusionKernel,
                "_FoamObstacleCells",
                obstacleExclusionCellBuffer);
            computeShader.SetBuffer(
                updateObstacleExclusionKernel,
                "_FoamObstacleSamples",
                obstacleExclusionSampleBuffer);
            computeShader.SetTexture(
                updateObstacleExclusionKernel,
                "_FoamObstacleExclusionWrite",
                obstacleExclusionTexture);
            DispatchOneDimensional(
                updateObstacleExclusionKernel,
                obstacleExclusionCells.Count,
                64);
        }
    }
}
