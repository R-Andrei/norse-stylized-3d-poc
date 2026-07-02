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
        private void MeasurePopulation()
        {
            using var profilerScope = DiagnosticsMeasurePopulationProfilerMarker.Auto();
            if (computeShader == null || currentState == null ||
                boundaryTexture == null || populationMetricsBuffer == null ||
                resetPopulationKernel < 0 || measurePopulationKernel < 0)
            {
                return;
            }

            computeShader.SetInt("_FoamChunkCount", chunkCount);
            computeShader.SetBuffer(
                resetPopulationKernel,
                "_FoamPopulationMetrics",
                populationMetricsBuffer);
            DispatchOneDimensional(
                resetPopulationKernel,
                chunkCount,
                64);

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInt(
                "_FoamResolutionPerChunk",
                resolutionPerChunk);
            computeShader.SetFloat(
                "_FoamVisibleThreshold",
                ProvisionalMaterialVisibleThreshold);
            computeShader.SetTexture(
                measurePopulationKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                measurePopulationKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetInts(
                "_FoamGuidanceDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetTexture(
                measurePopulationKernel,
                "_FoamGuidanceRead",
                guidanceTexture);
            computeShader.SetBuffer(
                measurePopulationKernel,
                "_FoamPopulationMetrics",
                populationMetricsBuffer);
            Dispatch(measurePopulationKernel, fieldWidth, fieldHeight);
        }

        private void UpdateFractureField(float deltaTime)
        {
            if (computeShader == null || currentState == null ||
                currentFracture == null || writeFracture == null ||
                updateFractureKernel < 0)
            {
                return;
            }

            computeShader.SetFloat(
                "_FoamFractureDeltaTime",
                Mathf.Max(0.0001f, deltaTime));
            computeShader.SetTexture(
                updateFractureKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                updateFractureKernel,
                "_FoamFractureRead",
                currentFracture);
            computeShader.SetTexture(
                updateFractureKernel,
                "_FoamFractureWrite",
                writeFracture);
            Dispatch(updateFractureKernel, fractureWidth, fractureHeight);
            (currentFracture, writeFracture) =
                (writeFracture, currentFracture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamFractureRead",
                currentFracture);
        }

        private void HandleDomainChanged(RiverDomainSnapshot _)
        {
            resourcesDirty = true;
            boundaryDirty = true;
            if (initializationPhase == InitializationPhase.Failed)
            {
                initializationPhase = InitializationPhase.NotStarted;
            }
        }

        private void HandleGeneratedSourceChanged(IGeneratedGeometrySource _)
        {
            boundaryDirty = true;
        }

        private float ResolveGuidanceUpdateRate()
        {
            if (river == null)
            {
                return 6f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 4f,
                StylizedRiverQuality.Medium => 6f,
                StylizedRiverQuality.High => 8f,
                _ => 6f
            };
        }

        private float ResolvePopulationUpdateRate()
        {
            if (river == null)
            {
                return 6f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 4f,
                StylizedRiverQuality.Medium => 6f,
                StylizedRiverQuality.High => 8f,
                _ => 6f
            };
        }

        private float ResolveFractureUpdateRate()
        {
            if (river == null)
            {
                return 10f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 8f,
                StylizedRiverQuality.Medium => 10f,
                StylizedRiverQuality.High => 12f,
                _ => 10f
            };
        }

        private float ResolveUpdateRate()
        {
            if (river == null)
            {
                return 20f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 12f,
                StylizedRiverQuality.Medium => 20f,
                StylizedRiverQuality.High => 30f,
                _ => 20f
            };
        }

        private int GlobalDistanceToX(float globalDistance)
        {
            float localDistance =
                globalDistance - river.Domain.GlobalDistanceMinimum;
            return StylizedRiverFoamTopologyFieldSpace
                .LocalDistanceToNearestTexel(
                    localDistance,
                    fieldWidth,
                    fieldLength);
        }

        private int GlobalDistanceToChunk(float globalDistance)
        {
            float local = globalDistance - river.Domain.GlobalDistanceMinimum;
            return Mathf.Clamp(
                Mathf.FloorToInt(local / ChunkLengthMetres),
                0,
                Mathf.Max(0, chunkCount - 1));
        }

        private int CountActiveChunks()
        {
            int count = 0;
            for (int index = 0; index < chunkActive.Length; index++)
            {
                if (chunkActive[index])
                {
                    count++;
                }
            }

            return count;
        }

        private void ResetLastUpdateDiagnostics()
        {
            lastUpdateDispatches = 0;
            lastUpdateCellIterations = 0;
            injectedLastUpdate = 0;
        }

        private void UpdateRecentPeaks()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            if (now > recentPeakWindowEnd)
            {
                recentPeakDispatches = lastUpdateDispatches;
                recentPeakCellIterations = lastUpdateCellIterations;
                recentPeakWindowEnd = now + 5.0;
                return;
            }

            recentPeakDispatches = Mathf.Max(
                recentPeakDispatches,
                lastUpdateDispatches);
            recentPeakCellIterations = Math.Max(
                recentPeakCellIterations,
                lastUpdateCellIterations);
        }
    }
}
