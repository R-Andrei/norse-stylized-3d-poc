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
        private void ResetManualInjectionSequence()
        {
            manualInjectionSequence = 0;
        }

        private bool ProcessPendingInjections(float now)
        {
            if (pendingInjections.Count == 0)
            {
                return false;
            }

            bool manualInjected = false;
            for (int index = 0; index < pendingInjections.Count; index++)
            {
                PendingInjection injection = pendingInjections[index];
                ActivateInjectionRange(injection, now);
                DispatchInjection(injection);
                reservations.Add(CreateReservation(injection));

                injectedLastUpdate++;
                manualInjected = true;
            }

            pendingInjections.Clear();
            return manualInjected;
        }

        private FoamReservation CreateReservation(PendingInjection injection)
        {
            return new FoamReservation
            {
                CentreGlobalDistance = injection.GlobalDistance,
                AlongRadius = injection.Radius * injection.Elongation,
                RemainingAmount = injection.Amount,
                Elapsed = 0f,
                MaximumLifetime = Mathf.Clamp(
                    injection.RemainingLife *
                    ResolveMaximumMaterialReservationSeconds() +
                    EndOfLifeDissipationSeconds * 2f,
                    EndOfLifeDissipationSeconds,
                    MaximumManualReservationSeconds)
            };
        }

        private void UpdateReservations(float deltaTime, float now)
        {
            float liquid = river.LiquidFactor;
            float speed =
                river.FlowSpeedMetresPerSecond *
                ProvisionalMaterialFlowFollow * liquid;
            // The CPU reservation is deliberately conservative because the
            // exact GPU Remaining Life depends on material-facing topology along
            // the transported path. Let it approach expiry over the maximum
            // supported lifetime instead of retiring a still-living patch early.
            float spread =
                ProvisionalMaterialSpread *
                ProvisionalMaterialEvolution;

            for (int index = reservations.Count - 1; index >= 0; index--)
            {
                FoamReservation reservation = reservations[index];
                reservation.Elapsed += deltaTime;
                reservation.CentreGlobalDistance += speed * deltaTime;
                reservation.AlongRadius +=
                    (0.15f + spread * 0.35f) * deltaTime;
                float reservationDecay =
                    DecayToFivePercent /
                    Mathf.Max(0.05f, reservation.MaximumLifetime);
                reservation.RemainingAmount *=
                    Mathf.Exp(-reservationDecay * deltaTime);

                if (reservation.Elapsed >= reservation.MaximumLifetime ||
                    reservation.RemainingAmount < 0.015f)
                {
                    reservations.RemoveAt(index);
                    continue;
                }

                ActivateReservationRange(reservation, now);
            }
        }

        private float ResolveMaximumMaterialReservationSeconds()
        {
            if (river == null)
            {
                return MaximumManualReservationSeconds;
            }

            float slowestAgeRate = Mathf.Max(
                0.05f,
                river.FoamSupportedAgingRate);
            float supportedLifetime =
                river.FoamNeutralLifetime / slowestAgeRate;
            return Mathf.Clamp(
                supportedLifetime + EndOfLifeDissipationSeconds * 2f,
                river.FoamNeutralLifetime,
                MaximumManualReservationSeconds);
        }

        private void ActivateInjectionRange(PendingInjection injection, float now)
        {
            float padding = Mathf.Max(0.5f, injection.Radius * injection.Elongation);
            ActivateGlobalRange(
                injection.GlobalDistance - padding,
                injection.GlobalDistance + padding,
                now + Mathf.Min(
                    5f,
                    ResolveMaximumMaterialReservationSeconds()));
        }

        private void ActivateReservationRange(FoamReservation reservation, float now)
        {
            float margin = Mathf.Max(
                0.5f,
                Mathf.Abs(
                    river.FlowSpeedMetresPerSecond *
                    ProvisionalMaterialFlowFollow) /
                Mathf.Max(1f, ResolveUpdateRate()) * 2f);
            ActivateGlobalRange(
                reservation.CentreGlobalDistance - reservation.AlongRadius - margin,
                reservation.CentreGlobalDistance + reservation.AlongRadius + margin,
                now + 1.5f / Mathf.Max(1f, ResolveUpdateRate()));
        }

        private void ActivateGlobalRange(
            float minimumGlobal,
            float maximumGlobal,
            double activeUntil)
        {
            int minimumChunk = GlobalDistanceToChunk(minimumGlobal);
            int maximumChunk = GlobalDistanceToChunk(maximumGlobal);

            for (int chunk = minimumChunk; chunk <= maximumChunk; chunk++)
            {
                if (!chunkActive[chunk])
                {
                    // Inactive chunks are already cleared by allocation or
                    // the existing sleep path; reactivation needs no extra
                    // clear dispatch.
                    chunkActive[chunk] = true;
                }

                chunkActiveUntil[chunk] = Math.Max(
                    chunkActiveUntil[chunk],
                    activeUntil);
            }
        }

        private void ActivateAllChunks(double activeUntil)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                chunkActive[chunk] = true;
                chunkActiveUntil[chunk] = Math.Max(
                    chunkActiveUntil[chunk],
                    activeUntil);
            }
        }

        private void UpdateActiveChunks(float now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!chunkActive[chunk] || now <= chunkActiveUntil[chunk])
                {
                    continue;
                }

                chunkActive[chunk] = false;
                chunkActiveUntil[chunk] = 0.0;
                ClearChunk(chunk);
            }
        }

        private void SimulateActiveChunks(float deltaTime)
        {
            if (CountActiveChunks() == 0)
            {
                return;
            }

            previousState = currentState;

            int chunk = 0;
            while (chunk < chunkCount)
            {
                while (chunk < chunkCount && !chunkActive[chunk])
                {
                    chunk++;
                }

                if (chunk >= chunkCount)
                {
                    break;
                }

                int startChunk = chunk;
                while (chunk < chunkCount && chunkActive[chunk])
                {
                    chunk++;
                }

                int endChunkExclusive = chunk;
                int startX = startChunk * resolutionPerChunk;
                int countX = Mathf.Min(
                    fieldWidth - startX,
                    (endChunkExclusive - startChunk) * resolutionPerChunk);
                DispatchSimulation(startX, countX);
            }

            (currentState, writeState) = (writeState, currentState);
        }

        private void DispatchInjection(PendingInjection injection)
        {
            int centreX = GlobalDistanceToX(injection.GlobalDistance);
            float alongRadius = injection.Radius * injection.Elongation *
                (injection.CompoundShape ? 1.25f : 1f);
            float dx = StylizedRiverFoamTopologyFieldSpace.TexelSpacing(
                fieldLength,
                fieldWidth);
            int radiusPixels = Mathf.CeilToInt(alongRadius / Mathf.Max(0.001f, dx)) + 2;
            int startX = Mathf.Clamp(centreX - radiusPixels, 0, fieldWidth - 1);
            int endX = Mathf.Clamp(centreX + radiusPixels, 0, fieldWidth - 1);
            int countX = endX - startX + 1;

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);
            computeShader.SetFloat("_FoamGlobalStart", river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamInjectionGlobalDistance", injection.GlobalDistance);
            computeShader.SetFloat("_FoamInjectionAcrossNormalized", injection.AcrossNormalized);
            computeShader.SetFloat("_FoamInjectionRadius", injection.Radius);
            computeShader.SetFloat("_FoamInjectionAmount", injection.Amount);
            computeShader.SetFloat(
                "_FoamInjectionRemainingLife",
                injection.RemainingLife);
            computeShader.SetFloat("_FoamInjectionIntegrity", injection.Integrity);
            computeShader.SetFloat("_FoamInjectionPhase", injection.Phase);
            computeShader.SetFloat("_FoamInjectionElongation", injection.Elongation);
            computeShader.SetFloat("_FoamInjectionShapeSeed", injection.ShapeSeed);
            computeShader.SetFloat("_FoamInjectionShapeVariety", injection.ShapeVariety);
            computeShader.SetFloat(
                "_FoamInjectionCompound",
                injection.CompoundShape ? 1f : 0f);
            computeShader.SetBuffer(injectKernel, "_FoamMetricRows", metricBuffer);
            computeShader.SetTexture(injectKernel, "_FoamBoundary", boundaryTexture);

            DispatchInjectionToState(currentState, countX);

            if (injection.IsManual)
            {
                // Manual diagnostics exist in both temporal states so
                // interpolation cannot hide a fresh source behind an empty
                // previous field. They then evolve through the complete solver.
                if (stateA != null && stateA != currentState)
                {
                    DispatchInjectionToState(stateA, countX);
                }

                if (stateB != null && stateB != currentState && stateB != stateA)
                {
                    DispatchInjectionToState(stateB, countX);
                }

                lastInjectionStateSynchronized = stateA != null && stateB != null;
                lastInjectionBoundaryCoverage = SampleInjectionBoundaryCoverage(injection);
                simulationInterpolation = 1f;
            }
        }

        private void DispatchInjectionToState(RenderTexture target, int countX)
        {
            if (target == null)
            {
                return;
            }

            computeShader.SetTexture(injectKernel, "_FoamStateWrite", target);
            Dispatch(injectKernel, countX, fieldHeight);
        }

        private float SampleInjectionBoundaryCoverage(PendingInjection injection)
        {
            if (boundaryTexture == null || fieldLength <= 0.0001f)
            {
                return -1f;
            }

            float u = Mathf.Clamp01(
                (injection.GlobalDistance - river.Domain.GlobalDistanceMinimum) /
                fieldLength);
            float v = Mathf.Clamp01(injection.AcrossNormalized * 0.5f + 0.5f);
            return Mathf.Clamp01(boundaryTexture.GetPixelBilinear(u, v).r);
        }
    }
}
