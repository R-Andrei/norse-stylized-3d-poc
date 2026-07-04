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
                if (injection.IsManual)
                {
                    manualProofReferenceArea = 0f;
                    manualProofReferencePending = true;
                }

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
                AlongRadius =
                    injection.Radius * injection.Elongation *
                    (injection.CompoundShape ? 1.25f : 1f),
                Elapsed = 0f,
                MaximumLifetime = Mathf.Clamp(
                    injection.RemainingLife *
                    ResolveMaximumMaterialReservationSeconds(),
                    0.05f,
                    MaximumManualReservationSeconds)
            };
        }

        private void UpdateReservations(float deltaTime, float now)
        {
            float speed =
                river.FlowSpeedMetresPerSecond *
                river.LiquidFactor *
                river.FoamMaterialFlowSpeedMultiplier *
                river.FlowDirection;
            // Reservations follow the actual downstream transport centre. They
            // do not grow with an inherited spread estimate: material footprint
            // growth is no longer an accepted runtime behaviour.
            for (int index = reservations.Count - 1; index >= 0; index--)
            {
                FoamReservation reservation = reservations[index];
                reservation.Elapsed += deltaTime;
                reservation.CentreGlobalDistance += speed * deltaTime;
                if (reservation.Elapsed >= reservation.MaximumLifetime)
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
                supportedLifetime,
                river.FoamNeutralLifetime,
                MaximumManualReservationSeconds);
        }

        private void ActivateInjectionRange(PendingInjection injection, float now)
        {
            float padding = injection.SegmentShape
                ? Mathf.Max(
                    0.5f,
                    Mathf.Max(
                        injection.SegmentStartRadius,
                        injection.SegmentEndRadius))
                : Mathf.Max(
                    0.5f,
                    injection.Radius * injection.Elongation);
            float minimumGlobal = injection.SegmentShape
                ? Mathf.Min(
                    injection.SegmentStartGlobalDistance,
                    injection.SegmentEndGlobalDistance)
                : injection.GlobalDistance;
            float maximumGlobal = injection.SegmentShape
                ? Mathf.Max(
                    injection.SegmentStartGlobalDistance,
                    injection.SegmentEndGlobalDistance)
                : injection.GlobalDistance;
            ActivateTransportSafeRange(
                minimumGlobal - padding,
                maximumGlobal + padding,
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
                    river.FoamMaterialFlowSpeedMultiplier) /
                Mathf.Max(1f, ResolveUpdateRate()) * 2f);
            ActivateTransportSafeRange(
                reservation.CentreGlobalDistance - reservation.AlongRadius - margin,
                reservation.CentreGlobalDistance + reservation.AlongRadius + margin,
                now + 1.5f / Mathf.Max(1f, ResolveUpdateRate()));
        }

        private void ActivateTransportSafeRange(
            float minimumGlobal,
            float maximumGlobal,
            double activeUntil)
        {
            // Conservative transport shares one face flux between its donor and
            // receiver. Keep one downstream chunk scheduled beyond the known
            // CPU reservation so a patch can cross a chunk boundary without
            // losing the receiver half of that flux. This is a work halo only;
            // it never grows, paints, or otherwise changes material state.
            float downstreamHalo = ChunkLengthMetres;
            if (river != null && river.FlowDirection < 0f)
            {
                minimumGlobal -= downstreamHalo;
            }
            else
            {
                maximumGlobal += downstreamHalo;
            }

            ActivateGlobalRange(minimumGlobal, maximumGlobal, activeUntil);
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

        private float WorldGlobalDistanceToFoamStorageGlobalDistance(
            float globalDistance)
        {
            return globalDistance - foamPhaseTransportMetres;
        }

        private void DispatchInjection(PendingInjection injection)
        {
            float minimumGlobal;
            float maximumGlobal;
            float storageGlobalDistance =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.GlobalDistance);
            float storageSegmentStartGlobalDistance =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.SegmentStartGlobalDistance);
            float storageSegmentEndGlobalDistance =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    injection.SegmentEndGlobalDistance);

            if (injection.SegmentShape)
            {
                float segmentPadding = Mathf.Max(
                    injection.SegmentStartRadius,
                    injection.SegmentEndRadius);
                minimumGlobal = Mathf.Min(
                    storageSegmentStartGlobalDistance,
                    storageSegmentEndGlobalDistance) - segmentPadding;
                maximumGlobal = Mathf.Max(
                    storageSegmentStartGlobalDistance,
                    storageSegmentEndGlobalDistance) + segmentPadding;
            }
            else
            {
                float alongRadius = injection.Radius * injection.Elongation *
                    (injection.CompoundShape ? 1.25f : 1f);
                minimumGlobal = storageGlobalDistance - alongRadius;
                maximumGlobal = storageGlobalDistance + alongRadius;
            }

            int startX = Mathf.Clamp(
                GlobalDistanceToX(minimumGlobal) - 2,
                0,
                fieldWidth - 1);
            int endX = Mathf.Clamp(
                GlobalDistanceToX(maximumGlobal) + 2,
                0,
                fieldWidth - 1);
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
            computeShader.SetFloat("_FoamInjectionGlobalDistance", storageGlobalDistance);
            computeShader.SetFloat("_FoamInjectionAcrossNormalized", injection.AcrossNormalized);
            computeShader.SetFloat("_FoamInjectionRadius", injection.Radius);
            computeShader.SetFloat("_FoamInjectionAmount", injection.SourceAmount);
            computeShader.SetFloat(
                "_FoamInjectionRemainingLife",
                injection.RemainingLife);
            computeShader.SetFloat(
                "_FoamInjectionPatternSeed",
                injection.PatternSeed);
            computeShader.SetFloat("_FoamInjectionElongation", injection.Elongation);
            computeShader.SetFloat(
                "_FoamInjectionSourceFillSeed",
                injection.SourceFillSeed);
            computeShader.SetFloat(
                "_FoamInjectionSourceFillFeatureSize",
                injection.SourceFillFeatureSize);
            computeShader.SetFloat("_FoamInjectionShapeSeed", injection.ShapeSeed);
            computeShader.SetFloat("_FoamInjectionShapeVariety", injection.ShapeVariety);
            computeShader.SetFloat(
                "_FoamInjectionCompound",
                injection.CompoundShape ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamInjectionSegment",
                injection.SegmentShape ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartGlobalDistance",
                storageSegmentStartGlobalDistance);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartAcrossNormalized",
                injection.SegmentStartAcrossNormalized);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartRadius",
                injection.SegmentStartRadius);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartAmount",
                injection.SegmentStartSourceAmount);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndGlobalDistance",
                storageSegmentEndGlobalDistance);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndAcrossNormalized",
                injection.SegmentEndAcrossNormalized);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndRadius",
                injection.SegmentEndRadius);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndAmount",
                injection.SegmentEndSourceAmount);
            computeShader.SetBuffer(injectKernel, "_FoamMetricRows", metricBuffer);
            computeShader.SetTexture(injectKernel, "_FoamBoundary", boundaryTexture);
            computeShader.SetTexture(
                injectKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);

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
                foamRenderTravelMetres = foamPhaseTransportMetres;
                lastFoamPhaseTransportMetres = foamPhaseTransportMetres;
                lastFoamPhaseCellFraction =
                    minimumTransportLongitudinalSpacing > 0.0001f
                        ? Mathf.Clamp01(
                            Mathf.Abs(foamPhaseTransportMetres) /
                            minimumTransportLongitudinalSpacing)
                        : 0f;
                lastFoamRenderTravelMetres = foamRenderTravelMetres;
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
