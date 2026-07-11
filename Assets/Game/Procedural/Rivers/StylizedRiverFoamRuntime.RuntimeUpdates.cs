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

            // Material cadence remains the accepted Low/Medium/High
            // 8/12/16 Hz contract. Conservative transport derives CFL-safe
            // substeps inside each material tick rather than changing the
            // authored simulation cadence.
            return qualityRate;
        }

        private float ResolveBaseFoamDownstreamSpeedMetresPerSecond()
        {
            if (river == null)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                Mathf.Abs(river.FlowSpeedMetresPerSecond) *
                river.LiquidFactor *
                river.FoamDownstreamSpeedRatio);
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
            automaticSourceEventsRasterizedLastUpdate = 0;
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
