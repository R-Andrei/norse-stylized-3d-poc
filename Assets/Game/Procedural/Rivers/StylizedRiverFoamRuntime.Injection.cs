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

            bool manualQueued = false;
            for (int index = 0; index < pendingInjections.Count; index++)
            {
                PendingInjection injection = pendingInjections[index];
                QueueMaterialBirth(injection);
                if (injection.IsManual)
                {
                    manualProofReferenceArea = 0f;
                    manualProofReferencePending = true;
                    manualQueued = true;
                }

                injectedLastUpdate++;
            }

            pendingInjections.Clear();
            return manualQueued;
        }

        private void QueueMaterialBirth(PendingInjection injection)
        {
            pendingMaterialBirths.Add(injection);
            RecordMaterialBirthCommand();
            materialLifetimeAuthorityActive = true;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus =
                "Remaining Life / full-field direct simulation";
        }

        private void RecordMaterialBirthCommand()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            birthCommandsThisFrame++;
            lastBirthCommandAt = now;
            if (!materialClockSessionActive)
            {
                materialClockSessionActive = true;
                materialClockSessionStartedAt = now;
                materialSimulatedSecondsSinceSession = 0f;
                materialStepCountSinceSession = 0;
            }
        }

        private void RecordMaterialSimulationStep(float deltaTime)
        {
            if (!materialClockSessionActive)
            {
                return;
            }

            materialSimulatedSecondsSinceSession += Mathf.Max(0f, deltaTime);
            materialStepCountSinceSession++;
        }

        private void SimulateFullField(float deltaTime)
        {
            if (currentState == null || writeState == null ||
                fieldWidth <= 0 || fieldHeight <= 0)
            {
                pendingMaterialBirths.Clear();
                return;
            }

            previousState = currentState;
            DispatchSimulation(0, fieldWidth);
            DispatchQueuedMaterialBirths(writeState);
            (currentState, writeState) = (writeState, currentState);
            RecordMaterialSimulationStep(deltaTime);
        }

        private void DispatchQueuedMaterialBirths(RenderTexture target)
        {
            if (pendingMaterialBirths.Count == 0 || target == null)
            {
                pendingMaterialBirths.Clear();
                return;
            }

            for (int index = 0; index < pendingMaterialBirths.Count; index++)
            {
                DispatchInjection(pendingMaterialBirths[index], target);
            }

            pendingMaterialBirths.Clear();
        }

        private float WorldGlobalDistanceToFoamStorageGlobalDistance(
            float globalDistance)
        {
            return globalDistance - foamPhaseTransportMetres;
        }

        private void DispatchInjection(PendingInjection injection, RenderTexture target)
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

            DispatchInjectionToState(target, countX);

            if (injection.IsManual)
            {
                lastInjectionStateSynchronized = true;
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
