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
        private void SimulateWakeField(float deltaTime, double now)
        {
            if (!HasWakeActiveChunks())
            {
                return;
            }

            float cellSizeX = fieldLength / Mathf.Max(1, wakeFieldWidth);
            float cellSizeY =
                averageSurfaceHalfWidth * 2f /
                Mathf.Max(1, wakeFieldHeight - 1);
            float advectionPixels =
                Mathf.Abs(river.FlowSpeedMetresPerSecond) *
                deltaTime /
                Mathf.Max(0.001f, cellSizeX);
            const float decayPerSecond = 1.15f;
            float lateralSpread = river.WakeWidening;
            float flowFactor = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(
                    0.05f,
                    1.25f,
                    Mathf.Abs(river.FlowSpeedMetresPerSecond)));

            computeShader.SetInts(
                "_WakeFieldSize",
                wakeFieldWidth,
                wakeFieldHeight);
            computeShader.SetFloat("_WakeDeltaTime", deltaTime);
            computeShader.SetFloat("_WakeAdvectionPixels", advectionPixels);
            computeShader.SetFloat("_WakeCellSizeX", cellSizeX);
            computeShader.SetFloat("_WakeCellSizeY", cellSizeY);
            computeShader.SetFloat("_WakeLateralSpread", lateralSpread);
            computeShader.SetFloat("_WakeDecayPerSecond", decayPerSecond);
            computeShader.SetFloat("_WakeSourceRate", 1.45f);
            computeShader.SetFloat("_WakeFlowFactor", flowFactor);
            computeShader.SetFloat("_WakeTime", river.MotionTime);
            computeShader.SetFloat("_WakeGradientStrength", 0.32f);
            computeShader.SetTexture(
                simulateWakeKernel,
                "_WakeRead",
                currentWake);
            computeShader.SetTexture(
                simulateWakeKernel,
                "_WakeWrite",
                writeWake);
            computeShader.SetTexture(
                simulateWakeKernel,
                "_StaticWakeSourceRead",
                staticWakeSource);

            DispatchWakeActiveRanges();

            RenderTexture oldCurrent = currentWake;
            currentWake = writeWake;
            previousWake = oldCurrent;
            writeWake = oldCurrent;
        }

        private void ExpireChunks(double now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!chunkActive[chunk] ||
                    now < chunkActiveUntil[chunk])
                {
                    continue;
                }

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
                chunkActive[chunk] = false;
                chunkActiveUntil[chunk] = 0.0;
            }
        }

        private void ExpireWakeChunks(double now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!wakeChunkActive[chunk] ||
                    chunkHasStaticSource[chunk] ||
                    now < wakeChunkActiveUntil[chunk])
                {
                    continue;
                }

                int xOffset = chunk * wakeResolutionPerChunk;
                DispatchClear(
                    wakeA,
                    wakeFieldWidth,
                    wakeFieldHeight,
                    xOffset,
                    wakeResolutionPerChunk);
                DispatchClear(
                    wakeB,
                    wakeFieldWidth,
                    wakeFieldHeight,
                    xOffset,
                    wakeResolutionPerChunk);
                wakeChunkActive[chunk] = false;
                wakeChunkActiveUntil[chunk] = 0.0;
            }
        }

        private float GlobalDistanceToPixel(float globalDistance)
        {
            return FieldGlobalDistanceToPixel(globalDistance, fieldWidth);
        }

        private float WakeGlobalDistanceToPixel(float globalDistance)
        {
            return FieldGlobalDistanceToPixel(globalDistance, wakeFieldWidth);
        }

        private float FieldGlobalDistanceToPixel(
            float globalDistance,
            int targetWidth)
        {
            float localDistance = Mathf.Clamp(
                globalDistance - river.Domain.GlobalDistanceMinimum,
                0f,
                validFieldLength);
            return localDistance / Mathf.Max(0.001f, fieldLength) *
                   Mathf.Max(0, targetWidth - 1);
        }

        private float AcrossToPixel(float acrossNormalized)
        {
            return FieldAcrossToPixel(acrossNormalized, fieldHeight);
        }

        private float WakeAcrossToPixel(float acrossNormalized)
        {
            return FieldAcrossToPixel(acrossNormalized, wakeFieldHeight);
        }

        private static float FieldAcrossToPixel(
            float acrossNormalized,
            int targetHeight)
        {
            return
                (Mathf.Clamp(acrossNormalized, -1f, 1f) * 0.5f + 0.5f) *
                Mathf.Max(0, targetHeight - 1);
        }

        private bool HasActiveChunks()
        {
            return HasRippleActiveChunks() || HasWakeActiveChunks();
        }

        private bool HasRippleActiveChunks()
        {
            for (int index = 0; index < chunkActive.Length; index++)
            {
                if (chunkActive[index])
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasWakeActiveChunks()
        {
            for (int index = 0; index < wakeChunkActive.Length; index++)
            {
                if (wakeChunkActive[index])
                {
                    return true;
                }
            }

            return false;
        }

        private int CountActiveChunks()
        {
            int count = 0;
            for (int index = 0; index < chunkCount; index++)
            {
                bool rippleActive =
                    index < chunkActive.Length && chunkActive[index];
                bool wakeActive =
                    index < wakeChunkActive.Length && wakeChunkActive[index];
                if (rippleActive || wakeActive)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountActiveWakeChunks()
        {
            int count = 0;
            for (int index = 0; index < wakeChunkActive.Length; index++)
            {
                if (wakeChunkActive[index])
                {
                    count++;
                }
            }

            return count;
        }
    }
}
