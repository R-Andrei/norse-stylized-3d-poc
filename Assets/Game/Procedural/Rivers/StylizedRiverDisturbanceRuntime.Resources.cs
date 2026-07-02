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
        private bool EnsureResources()
        {
            if (river == null || !river.Domain.IsValid)
            {
                return false;
            }

            if (!resourcesDirty &&
                currentState != null &&
                currentWake != null &&
                rippleBoundary != null &&
                domainVersion == river.Domain.Version)
            {
                return true;
            }

            ReleaseResources();
            RecordFieldRebuild();

            computeShader = Resources.Load<ComputeShader>(
                ComputeResourcePath);

            if (computeShader == null)
            {
                Debug.LogError(
                    $"StylizedRiver on '{name}' could not load compute shader Resources/{ComputeResourcePath}.",
                    this);
                return false;
            }

            clearKernel = computeShader.FindKernel("ClearRange");
            injectRippleKernel = computeShader.FindKernel("InjectRipple");
            injectWakeKernel = computeShader.FindKernel("InjectWake");
            bakeStaticPressureKernel = computeShader.FindKernel("BakeStaticPressure");
            finalizeStaticPressureKernel = computeShader.FindKernel("FinalizeStaticPressure");
            bakeStaticWakeSourceKernel = computeShader.FindKernel("BakeStaticWakeSource");
            bakeRippleBoundaryBaseKernel =
                computeShader.FindKernel("BakeRippleBoundaryBase");
            bakeRippleBoundaryObstacleKernel =
                computeShader.FindKernel("BakeRippleBoundaryObstacle");
            applyRippleBoundaryKernel =
                computeShader.FindKernel("ApplyRippleBoundary");
            simulateRippleKernel = computeShader.FindKernel("SimulateRipple");
            simulateWakeKernel = computeShader.FindKernel("SimulateWake");

            chunkCount = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    river.Domain.LocalLength /
                    ChunkLengthMetres));

            resolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 64,
                StylizedRiverQuality.Medium => 96,
                StylizedRiverQuality.High => 128,
                _ => 96
            };
            wakeResolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 48,
                StylizedRiverQuality.Medium => 96,
                StylizedRiverQuality.High => 128,
                _ => 96
            };

            int maximumTextureSize = SystemInfo.maxTextureSize;
            if (!TryResolveChunkedTextureWidth(
                    chunkCount,
                    resolutionPerChunk,
                    16,
                    maximumTextureSize,
                    out resolutionPerChunk,
                    out fieldWidth) ||
                !TryResolveChunkedTextureWidth(
                    chunkCount,
                    wakeResolutionPerChunk,
                    16,
                    maximumTextureSize,
                    out wakeResolutionPerChunk,
                    out wakeFieldWidth))
            {
                ReportAllocationFailure(maximumTextureSize);
                return false;
            }

            fieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };
            wakeFieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 20,
                StylizedRiverQuality.Medium => 32,
                StylizedRiverQuality.High => 48,
                _ => 32
            };
            if (fieldHeight > maximumTextureSize ||
                wakeFieldHeight > maximumTextureSize)
            {
                ReportAllocationFailure(maximumTextureSize);
                return false;
            }

            fieldLength = chunkCount * ChunkLengthMetres;
            validFieldLength = river.Domain.LocalLength;
            validFieldWidth = ResolveValidColumnCount(
                fieldWidth,
                validFieldLength,
                fieldLength);
            validWakeFieldWidth = ResolveValidColumnCount(
                wakeFieldWidth,
                validFieldLength,
                fieldLength);
            allocationWarningReported = false;
            averageSurfaceHalfWidth = ResolveAverageSurfaceHalfWidth();
            domainVersion = river.Domain.Version;
            SetValidDomainComputeParameters();

            if (!BuildRippleMetricData())
            {
                ReleaseResources();
                return false;
            }

            stateA = CreateFieldTexture(
                "PS3D_RiverDisturbance_RippleA",
                fieldWidth,
                fieldHeight);
            stateB = CreateFieldTexture(
                "PS3D_RiverDisturbance_RippleB",
                fieldWidth,
                fieldHeight);
            staticTarget = CreateFieldTexture(
                "PS3D_RiverDisturbance_StaticPressure",
                fieldWidth,
                fieldHeight);
            rippleBoundary = CreateBoundaryTexture(
                "PS3D_RiverDisturbance_RippleBoundary",
                fieldWidth,
                fieldHeight);
            wakeA = CreateFieldTexture(
                "PS3D_RiverDisturbance_WakeA",
                wakeFieldWidth,
                wakeFieldHeight);
            wakeB = CreateFieldTexture(
                "PS3D_RiverDisturbance_WakeB",
                wakeFieldWidth,
                wakeFieldHeight);
            staticWakeSource = CreateFieldTexture(
                "PS3D_RiverDisturbance_StaticWakeSource",
                wakeFieldWidth,
                wakeFieldHeight);
            currentState = stateA;
            previousState = stateA;
            writeState = stateB;
            currentWake = wakeA;
            previousWake = wakeA;
            writeWake = wakeB;

            chunkActiveUntil = new double[chunkCount];
            chunkActive = new bool[chunkCount];
            chunkHasStaticSource = new bool[chunkCount];
            wakeChunkActiveUntil = new double[chunkCount];
            staticWakeChunkReleaseDuration = new double[chunkCount];
            wakeChunkActive = new bool[chunkCount];

            DispatchClear(stateA, fieldWidth, fieldHeight, 0, fieldWidth);
            DispatchClear(stateB, fieldWidth, fieldHeight, 0, fieldWidth);
            DispatchClear(staticTarget, fieldWidth, fieldHeight, 0, fieldWidth);
            DispatchClear(wakeA, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
            DispatchClear(wakeB, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
            DispatchClear(
                staticWakeSource,
                wakeFieldWidth,
                wakeFieldHeight,
                0,
                wakeFieldWidth);
            simulationAccumulator = 0f;
            staticWakeVariationAccumulator = 0f;
            simulationInterpolation = 1f;
            wakeInterpolation = 1f;
            validStaticSourceCount = 0;
            validStaticWakeSourceCount = 0;
            staticPressureTargetDirty = true;
            staticWakeSourceDirty = true;
            rippleBoundaryDirty = true;
            resourcesDirty = false;
            RebuildRippleBoundary(Time.realtimeSinceStartupAsDouble);
            return true;
        }

        private static bool TryResolveChunkedTextureWidth(
            int chunks,
            int desiredResolutionPerChunk,
            int minimumResolutionPerChunk,
            int maximumTextureSize,
            out int resolvedResolutionPerChunk,
            out int resolvedWidth)
        {
            resolvedResolutionPerChunk = 0;
            resolvedWidth = 0;
            if (chunks < 1 ||
                maximumTextureSize < minimumResolutionPerChunk ||
                (long)chunks * minimumResolutionPerChunk > maximumTextureSize)
            {
                return false;
            }

            resolvedResolutionPerChunk = Math.Min(
                desiredResolutionPerChunk,
                maximumTextureSize / chunks);
            if (resolvedResolutionPerChunk < minimumResolutionPerChunk)
            {
                return false;
            }

            long width = (long)resolvedResolutionPerChunk * chunks;
            if (width < 1 || width > maximumTextureSize)
            {
                return false;
            }

            resolvedWidth = (int)width;
            return true;
        }

        private static int ResolveValidColumnCount(
            int textureWidth,
            float validLength,
            float storageLength)
        {
            if (textureWidth <= 1)
            {
                return Mathf.Clamp(textureWidth, 0, 1);
            }

            float lastValidIndex =
                Mathf.Clamp01(validLength / Mathf.Max(0.001f, storageLength)) *
                (textureWidth - 1);
            // Include the first sample at or beyond the endpoint as a
            // deliberate one-cell outflow guard. Unlike the old padded tail,
            // it cannot own sources and all following columns are hard-zeroed.
            return Mathf.Clamp(
                Mathf.CeilToInt(lastValidIndex) + 1,
                1,
                textureWidth);
        }

        private void ReportAllocationFailure(int maximumTextureSize)
        {
            if (allocationWarningReported)
            {
                return;
            }

            Debug.LogWarning(
                $"StylizedRiver disturbance field on '{name}' is disabled " +
                $"because the required textures for {chunkCount} chunks " +
                $"cannot fit within the hardware texture limit of " +
                $"{maximumTextureSize} pixels. The field requires at least " +
                "16 columns per chunk.",
                this);
            allocationWarningReported = true;
        }

        private RenderTexture CreateFieldTexture(
            string textureName,
            int width,
            int height)
        {
            RenderTexture texture = new RenderTexture(
                width,
                height,
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            texture.Create();
            return texture;
        }

        private RenderTexture CreateBoundaryTexture(
            string textureName,
            int width,
            int height)
        {
            RenderTexture texture = new RenderTexture(
                width,
                height,
                0,
                RenderTextureFormat.RGHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            texture.Create();
            return texture;
        }

        private void ReleaseResources()
        {
            ReleaseBuffer(ref rippleMetricBuffer);
            ReleaseTexture(ref stateA);
            ReleaseTexture(ref stateB);
            ReleaseTexture(ref staticTarget);
            ReleaseTexture(ref staticWakeSource);
            ReleaseTexture(ref rippleBoundary);
            ReleaseTexture(ref wakeA);
            ReleaseTexture(ref wakeB);
            currentState = null;
            previousState = null;
            writeState = null;
            currentWake = null;
            previousWake = null;
            writeWake = null;
            computeShader = null;
            clearKernel = -1;
            injectRippleKernel = -1;
            injectWakeKernel = -1;
            bakeStaticPressureKernel = -1;
            finalizeStaticPressureKernel = -1;
            bakeStaticWakeSourceKernel = -1;
            bakeRippleBoundaryBaseKernel = -1;
            bakeRippleBoundaryObstacleKernel = -1;
            applyRippleBoundaryKernel = -1;
            simulateRippleKernel = -1;
            simulateWakeKernel = -1;
            fieldWidth = 0;
            fieldHeight = 0;
            wakeFieldWidth = 0;
            wakeFieldHeight = 0;
            chunkCount = 0;
            resolutionPerChunk = 0;
            wakeResolutionPerChunk = 0;
            fieldLength = 0f;
            validFieldLength = 0f;
            validFieldWidth = 0;
            validWakeFieldWidth = 0;
            domainVersion = -1;
            rippleMetricMinimumAlongCell = Array.Empty<float>();
            rippleMetricMinimumLateralCell = Array.Empty<float>();
            rippleChunkMaximumInverseLength = Array.Empty<float>();
            rippleChunkMinimumCellSize = Array.Empty<float>();
            activeRippleMinimumCellSize = 0f;
            rippleSubstepLimitReached = false;
            activeImpactReservations.Clear();
            chunkActiveUntil = Array.Empty<double>();
            chunkActive = Array.Empty<bool>();
            chunkHasStaticSource = Array.Empty<bool>();
            wakeChunkActiveUntil = Array.Empty<double>();
            staticWakeChunkReleaseDuration = Array.Empty<double>();
            wakeChunkActive = Array.Empty<bool>();
            validStaticSourceCount = 0;
            validStaticWakeSourceCount = 0;
            rippleCollisionSourceCount = 0;
            staticPressureTargetDirty = true;
            staticWakeSourceDirty = true;
            rippleBoundaryDirty = true;
            resourcesDirty = true;
        }

        private static void ReleaseBuffer(ref ComputeBuffer buffer)
        {
            if (buffer == null)
            {
                return;
            }

            buffer.Release();
            buffer = null;
        }

        private static void ReleaseTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            if (texture.IsCreated())
            {
                texture.Release();
            }

            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }

            texture = null;
        }
    }
}
