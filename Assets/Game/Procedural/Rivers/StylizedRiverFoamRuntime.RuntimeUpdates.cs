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

        private float ResolveUpdateRate()
        {
            float qualityRate = river == null
                ? MediumMaterialTemporalUpdateRate
                : river.Quality switch
                {
                    StylizedRiverQuality.Low => LowMaterialTemporalUpdateRate,
                    StylizedRiverQuality.Medium => MediumMaterialTemporalUpdateRate,
                    StylizedRiverQuality.High => HighMaterialTemporalUpdateRate,
                    _ => MediumMaterialTemporalUpdateRate
                };

            // Patch 4.11C.5.2d removed base downstream movement from the
            // fractional transport solve. Material cadence now owns lifecycle,
            // source transfer, topology aging, and disturbance deformation;
            // base flow speed is handled by phase transport and whole-cell
            // commits, so it must not raise the material update rate.
            return qualityRate;
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

        private void ResetLastUpdateDiagnostics()
        {
            lastUpdateDispatches = 0;
            lastUpdateCellIterations = 0;
            injectedLastUpdate = 0;
            lastMaterialStepsThisFrame = 0;
            birthCommandsThisFrame = 0;
            automaticShoreBirthSubmittedLastUpdate = 0;
            automaticShoreBirthRejectedLastUpdate = 0;
            lastPhaseCommitCellsThisFrame = 0;
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
