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
        private void BindField()
        {
            if (surfaceRenderer == null ||
                currentState == null ||
                previousState == null ||
                currentWake == null ||
                previousWake == null ||
                staticTarget == null ||
                staticWakeSource == null ||
                rippleBoundary == null)
            {
                BindDisabled();
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(DisturbanceEnabledId, 1f);
            propertyBlock.SetTexture(
                DisturbancePreviousId,
                previousState);
            propertyBlock.SetTexture(
                DisturbanceCurrentId,
                currentState);
            propertyBlock.SetTexture(
                DisturbanceStaticTargetId,
                staticTarget);
            propertyBlock.SetTexture(
                DisturbanceRippleBoundaryId,
                rippleBoundary);
            propertyBlock.SetTexture(
                DisturbanceStaticWakeSourceId,
                staticWakeSource);
            propertyBlock.SetVector(
                DisturbanceStaticWakeTexelSizeId,
                new Vector4(
                    1f / Mathf.Max(1, staticWakeSource.width),
                    1f / Mathf.Max(1, staticWakeSource.height),
                    staticWakeSource.width,
                    staticWakeSource.height));
            propertyBlock.SetTexture(
                DisturbanceWakePreviousId,
                previousWake);
            propertyBlock.SetTexture(
                DisturbanceWakeCurrentId,
                currentWake);
            propertyBlock.SetFloat(
                DisturbanceInterpolationId,
                simulationInterpolation);
            propertyBlock.SetFloat(
                DisturbanceWakeInterpolationId,
                wakeInterpolation);
            propertyBlock.SetFloat(
                DisturbanceGlobalStartId,
                river.Domain.GlobalDistanceMinimum);
            propertyBlock.SetFloat(
                DisturbanceFieldLengthId,
                Mathf.Max(0.001f, fieldLength));
            propertyBlock.SetFloat(
                DisturbanceGeometryStrengthId,
                river.DisturbanceGeometryStrength);
            propertyBlock.SetFloat(
                DisturbanceNormalStrengthId,
                river.DisturbanceNormalStrength);
            propertyBlock.SetFloat(
                DisturbanceShoreInteractionId,
                river.DisturbanceShoreInteraction);
            propertyBlock.SetFloat(
                DisturbanceMaximumHeightId,
                river.ResolvedImpactRippleMaximumHeight);
            propertyBlock.SetFloat(
                DisturbanceStaticMaximumHeightId,
                MaximumStaticPressureHeightMetres);
            propertyBlock.SetFloat(
                DisturbanceWakeGeometryHeightId,
                river.WakeSurfaceHeight);
            propertyBlock.SetFloat(
                DisturbanceWakeGeometryCompactnessId,
                river.WakeSurfaceCompactness);
            propertyBlock.SetFloat(
                DisturbanceDebugViewId,
                (float)river.DisturbanceDebugView);
            propertyBlock.SetFloat(
                DisturbanceFragmentDetailId,
                river.Quality == StylizedRiverQuality.Low ? 0f : 1f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

        private void BindDisabled()
        {
            if (surfaceRenderer == null && river != null)
            {
                surfaceRenderer = river.SurfaceRenderer;
            }

            if (surfaceRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(DisturbanceEnabledId, 0f);
            propertyBlock.SetFloat(DisturbanceWakeGeometryHeightId, 0f);
            propertyBlock.SetFloat(
                DisturbanceWakeGeometryCompactnessId,
                1.50f);
            propertyBlock.SetFloat(DisturbanceFragmentDetailId, 0f);
            propertyBlock.SetFloat(DisturbanceWakeInterpolationId, 1f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
