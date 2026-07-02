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
        private void DispatchRippleInjection(ImpactCommand impact)
        {
            float centreX = GlobalDistanceToPixel(impact.Distance);
            float centreY = AcrossToPixel(impact.AcrossNormalized);
            ResolveRippleInjectionRadiusPixels(
                centreX,
                impact.Radius * RippleInjectionEnvelopeRadius,
                out float radiusX,
                out float radiusY);
            int minX = Mathf.Clamp(
                Mathf.FloorToInt(centreX - radiusX - 2f),
                0,
                fieldWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(centreX + radiusX + 2f),
                0,
                fieldWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(centreY - radiusY - 2f),
                0,
                fieldHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(centreY + radiusY + 2f),
                0,
                fieldHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);
            float resolvedStrength =
                river.ResolvedImpactRippleStrength;
            float signedImpulse =
                impact.SignedImpulse * resolvedStrength;

            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_RippleInjectRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_RippleInjectWorldPosition",
                new Vector4(
                    impact.WorldPositionXZ.x,
                    impact.WorldPositionXZ.y,
                    0f,
                    0f));
            computeShader.SetFloat(
                "_RippleInjectRadiusMetres",
                impact.Radius);
            computeShader.SetFloat(
                "_RippleInjectHeight",
                signedImpulse *
                Mathf.Clamp01(impact.GeometryContribution) *
                0.028f);
            computeShader.SetFloat(
                "_RippleInjectElevation",
                impact.InitialElevation *
                resolvedStrength *
                Mathf.Clamp01(impact.GeometryContribution));
            computeShader.SetFloat(
                "_RippleInjectVelocity",
                signedImpulse *
                Mathf.Clamp01(impact.GeometryContribution) *
                0.68f);
            computeShader.SetFloat(
                "_RippleInjectNormalDetail",
                signedImpulse *
                Mathf.Clamp01(impact.NormalContribution) *
                0.12f);
            computeShader.SetFloat(
                "_RippleInjectShape",
                Mathf.Clamp01(impact.Shape));
            computeShader.SetFloat(
                "_RippleInjectSharpness",
                Mathf.Clamp(
                    impact.Sharpness,
                    ImpactRippleEventSettings.MinimumSharpness,
                    ImpactRippleEventSettings.MaximumSharpness));
            computeShader.SetFloat(
                "_RippleInjectRidgeEmphasis",
                river.ImpactRippleRidgeEmphasis);
            computeShader.SetFloat(
                "_MaximumHeight",
                river.ResolvedImpactRippleMaximumHeight);
            computeShader.SetInt("_RippleMetricCount", fieldWidth);
            computeShader.SetBuffer(
                injectRippleKernel,
                "_RippleMetricData",
                rippleMetricBuffer);
            computeShader.SetTexture(
                injectRippleKernel,
                "_RippleBoundaryRead",
                rippleBoundary);
            computeShader.SetTexture(
                injectRippleKernel,
                "_StateWrite",
                currentState);
            DispatchCompute(
                injectRippleKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.ImpactInjection,
                width,
                height);
        }

        private void DispatchWakeInjection(
            ContinuousSource source,
            float surfaceHalfWidth,
            float wakeStrength,
            float movementBlend,
            float simulationDeltaTime)
        {
            float startX = WakeGlobalDistanceToPixel(source.StartDistance);
            float endX = WakeGlobalDistanceToPixel(source.EndDistance);
            float startY = WakeAcrossToPixel(source.StartAcrossNormalized);
            float endY = WakeAcrossToPixel(source.EndAcrossNormalized);
            float cellSizeX = fieldLength / Mathf.Max(1, wakeFieldWidth);
            float cellSizeY =
                surfaceHalfWidth * 2f / Mathf.Max(1, wakeFieldHeight);
            float alongPixels =
                source.AlongHalfLength / Mathf.Max(0.001f, cellSizeX);
            float acrossPixels =
                source.AcrossHalfWidth * river.WakeSpread /
                Mathf.Max(0.001f, cellSizeY);
            int minX = Mathf.Clamp(
                Mathf.FloorToInt(
                    Mathf.Min(startX, endX) - alongPixels * 1.25f - 2f),
                0,
                wakeFieldWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(
                    Mathf.Max(startX, endX) + alongPixels * 2.0f + 3f),
                0,
                wakeFieldWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(
                    Mathf.Min(startY, endY) - acrossPixels * 1.40f - 2f),
                0,
                wakeFieldHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(
                    Mathf.Max(startY, endY) + acrossPixels * 1.40f + 2f),
                0,
                wakeFieldHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);

            computeShader.SetInts(
                "_WakeFieldSize",
                wakeFieldWidth,
                wakeFieldHeight);
            computeShader.SetInts(
                "_WakeInjectRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_WakeInjectStart",
                new Vector4(startX, startY, 0f, 0f));
            computeShader.SetVector(
                "_WakeInjectEnd",
                new Vector4(endX, endY, 0f, 0f));
            computeShader.SetVector(
                "_WakeInjectFootprintPixels",
                new Vector4(
                    Mathf.Max(1f, alongPixels),
                    Mathf.Max(1f, acrossPixels),
                    0f,
                    0f));
            computeShader.SetFloat(
                "_WakeInjectStrength",
                Mathf.Max(0f, wakeStrength));
            computeShader.SetFloat(
                "_WakeInjectMovementBlend",
                Mathf.Clamp01(movementBlend));
            computeShader.SetFloat(
                "_WakeInjectPersistence",
                river.WakeReach);
            computeShader.SetFloat(
                "_WakeInjectDeltaTime",
                Mathf.Max(0.0001f, simulationDeltaTime));
            computeShader.SetTexture(
                injectWakeKernel,
                "_WakeWrite",
                currentWake);
            DispatchCompute(
                injectWakeKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.WakeInjection,
                width,
                height);
        }

        private bool DispatchRippleBoundaryObstacle(
            ContinuousSource source)
        {
            if (!river.TryProjectWorldPoint(
                    source.WorldPosition,
                    out StylizedRiverProjection projection) ||
                !projection.IsInside)
            {
                return false;
            }

            float centreX = GlobalDistanceToPixel(source.StartDistance);
            float centreY = AcrossToPixel(source.StartAcrossNormalized);
            float edgeWidth = Mathf.Max(
                0.025f,
                ResolveRippleBoundaryEdgeWidth(centreX));
            float envelopeRadius = Mathf.Max(
                source.RippleCollisionAlongHalfLength,
                source.RippleCollisionAcrossHalfWidth) +
                edgeWidth * 3f;
            ResolveRippleInjectionRadiusPixels(
                centreX,
                envelopeRadius,
                out float radiusX,
                out float radiusY);

            int minX = Mathf.Clamp(
                Mathf.FloorToInt(centreX - radiusX - 2f),
                0,
                fieldWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(centreX + radiusX + 2f),
                0,
                fieldWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(centreY - radiusY - 2f),
                0,
                fieldHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(centreY + radiusY + 2f),
                0,
                fieldHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);

            StylizedRiverSplineSample sample =
                river.Domain.SampleAtGlobalDistance(source.StartDistance);
            Vector3 downstream3 = sample.Tangent * river.FlowDirection;
            downstream3.y = 0f;
            downstream3 = downstream3.sqrMagnitude > 0.0001f
                ? downstream3.normalized
                : Vector3.forward;
            Vector3 across3 = sample.Side;
            across3.y = 0f;
            across3 = across3.sqrMagnitude > 0.0001f
                ? across3.normalized
                : Vector3.Cross(Vector3.up, downstream3).normalized;

            int contourCount = Mathf.Min(
                source.RippleCollisionContour != null
                    ? source.RippleCollisionContour.Length
                    : 0,
                MaximumStaticContourPoints);
            for (int index = 0;
                 index < MaximumStaticContourPoints;
                 index++)
            {
                if (index < contourCount)
                {
                    Vector2 point = source.RippleCollisionContour[index];
                    staticContourUpload[index] = new Vector4(
                        point.x,
                        point.y,
                        0f,
                        0f);
                }
                else
                {
                    staticContourUpload[index] = Vector4.zero;
                }
            }

            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetInt("_RippleMetricCount", fieldWidth);
            computeShader.SetInts(
                "_RippleObstacleRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_RippleObstacleWorldCentre",
                new Vector4(
                    source.WorldPosition.x,
                    source.WorldPosition.z,
                    0f,
                    0f));
            computeShader.SetVector(
                "_RippleObstacleDownstream",
                new Vector4(
                    downstream3.x,
                    downstream3.z,
                    0f,
                    0f));
            computeShader.SetVector(
                "_RippleObstacleAcross",
                new Vector4(
                    across3.x,
                    across3.z,
                    0f,
                    0f));
            computeShader.SetVector(
                "_RippleObstacleHalfSizeMetres",
                new Vector4(
                    source.RippleCollisionAlongHalfLength,
                    source.RippleCollisionAcrossHalfWidth,
                    0f,
                    0f));
            computeShader.SetFloat(
                "_RippleObstacleEdgeWidthMetres",
                edgeWidth);
            computeShader.SetFloat(
                "_RippleObstacleReflection",
                river.ImpactRippleObstacleReflection);
            computeShader.SetInt("_StaticContourCount", contourCount);
            computeShader.SetVectorArray(
                "_StaticContour",
                staticContourUpload);
            computeShader.SetBuffer(
                bakeRippleBoundaryObstacleKernel,
                "_RippleMetricData",
                rippleMetricBuffer);
            computeShader.SetTexture(
                bakeRippleBoundaryObstacleKernel,
                "_RippleBoundaryWrite",
                rippleBoundary);
            DispatchCompute(
                bakeRippleBoundaryObstacleKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.RippleBoundaryBake,
                width,
                height);
            return true;
        }

        private void DispatchStaticPressureBake(
            float globalDistance,
            float acrossNormalized,
            float surfaceHalfWidth,
            float acrossHalfWidth,
            float alongHalfLength,
            float targetHeightMetres,
            float responseStiffness,
            float unsteadiness,
            float phase,
            Vector2[] contour,
            RiverDisturbancePressureBakeProfile pressureProfile)
        {
            DispatchStaticBakeCommon(
                globalDistance,
                acrossNormalized,
                surfaceHalfWidth,
                acrossHalfWidth,
                alongHalfLength,
                contour,
                fieldWidth,
                fieldHeight,
                targetHeightMetres,
                0f,
                1f,
                1f,
                responseStiffness,
                unsteadiness,
                default,
                phase,
                bakeStaticPressureKernel,
                staticTarget,
                true,
                pressureProfile);
        }

        private void DispatchStaticWakeSourceBake(
            float globalDistance,
            float acrossNormalized,
            float surfaceHalfWidth,
            ContinuousSource source)
        {
            DispatchStaticBakeCommon(
                globalDistance,
                acrossNormalized,
                surfaceHalfWidth,
                source.AcrossHalfWidth,
                source.AlongHalfLength,
                source.StaticContour,
                wakeFieldWidth,
                wakeFieldHeight,
                0f,
                source.StaticWakeAmplitude,
                source.StaticWakeReachMultiplier,
                source.StaticWakeSpreadMultiplier,
                1f,
                0f,
                new StaticWakeBakeVariationParameters(
                    source.StaticWakeLeeVariation,
                    source.StaticWakeLeftReleaseVariation,
                    source.StaticWakeRightReleaseVariation),
                source.Phase,
                bakeStaticWakeSourceKernel,
                staticWakeSource,
                false,
                default);
        }

        private void DispatchStaticBakeCommon(
            float globalDistance,
            float acrossNormalized,
            float surfaceHalfWidth,
            float acrossHalfWidth,
            float alongHalfLength,
            Vector2[] contour,
            int targetWidth,
            int targetHeight,
            float targetHeightMetres,
            float wakeAmplitude,
            float wakePersistence,
            float wakeSpread,
            float responseStiffness,
            float unsteadiness,
            StaticWakeBakeVariationParameters wakeVariation,
            float phase,
            int kernel,
            RenderTexture targetTexture,
            bool pressurePass,
            RiverDisturbancePressureBakeProfile pressureProfile)
        {
            float centreX = FieldGlobalDistanceToPixel(
                globalDistance,
                targetWidth);
            float centreY = FieldAcrossToPixel(
                acrossNormalized,
                targetHeight);
            float cellSizeX = fieldLength / Mathf.Max(1, targetWidth);
            float cellSizeY =
                surfaceHalfWidth * 2f / Mathf.Max(1, targetHeight);
            float alongPixels =
                alongHalfLength / Mathf.Max(0.001f, cellSizeX);
            float acrossPixels =
                acrossHalfWidth / Mathf.Max(0.001f, cellSizeY);
            float pressureDepthMetres = pressurePass
                ? Mathf.Clamp(
                    Mathf.Max(
                        0.22f,
                        alongHalfLength * 2f * 0.08f,
                        cellSizeX * 1.15f,
                        river.ResolvedSurfaceLongitudinalSpacing * 1.50f),
                    0.22f,
                    0.48f)
                : 0f;
            float pressureInsideOverlapMetres = pressurePass
                ? Mathf.Clamp(
                    Mathf.Max(0.08f, cellSizeX * 0.35f),
                    0.08f,
                    0.16f)
                : 0f;
            float pressureDepthPixels = pressurePass
                ? pressureDepthMetres / Mathf.Max(0.001f, cellSizeX)
                : 0f;
            float pressureInsideOverlapPixels = pressurePass
                ? pressureInsideOverlapMetres / Mathf.Max(0.001f, cellSizeX)
                : 0f;
            float longitudinalExpansion = pressurePass ? 1f : 1.75f;
            float lateralExpansion = pressurePass
                ? 1.20f
                : 1.55f * Mathf.Clamp(wakeSpread, 0.5f, 2f);

            int minX = Mathf.Clamp(
                Mathf.FloorToInt(
                    pressurePass
                        ? centreX - alongPixels - pressureDepthPixels - 3f
                        : centreX - alongPixels * longitudinalExpansion - 4f),
                0,
                targetWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(
                    pressurePass
                        ? centreX + alongPixels + 3f
                        : centreX + alongPixels * longitudinalExpansion + 5f),
                0,
                targetWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(
                    centreY - acrossPixels * lateralExpansion - 4f),
                0,
                targetHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(
                    centreY + acrossPixels * lateralExpansion + 4f),
                0,
                targetHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);
            int contourCount = UploadStaticContour(
                contour,
                cellSizeX,
                cellSizeY);
            bool pressureGeometryValid = UploadStaticPressureProfile(
                pressurePass,
                pressureProfile,
                cellSizeX);

            StaticWakeLeeVariationState wakeLeeVariation =
                wakeVariation.Lee;
            int wakeVariationProfileCount = UploadStaticWakeVariationProfile(
                pressurePass,
                wakeLeeVariation);

            computeShader.SetInts("_FieldSize", targetWidth, targetHeight);
            computeShader.SetInts(
                "_StaticRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_StaticCentre",
                new Vector4(centreX, centreY, 0f, 0f));
            computeShader.SetVector(
                "_StaticHalfSizePixels",
                new Vector4(
                    Mathf.Max(1f, alongPixels),
                    Mathf.Max(1f, acrossPixels),
                    0f,
                    0f));
            computeShader.SetVector(
                "_StaticCellSize",
                new Vector4(cellSizeX, cellSizeY, 0f, 0f));
            computeShader.SetInt("_StaticContourCount", contourCount);
            computeShader.SetVectorArray(
                "_StaticContour",
                staticContourUpload);
            computeShader.SetVectorArray(
                "_StaticPressureProfile",
                staticPressureProfileUpload);
            computeShader.SetVectorArray(
                "_StaticPressureGeometry",
                staticPressureGeometryUpload);
            computeShader.SetVectorArray(
                "_StaticWakeVariationProfile",
                staticWakeVariationProfileUpload);
            computeShader.SetInt(
                "_StaticWakeVariationProfileCount",
                wakeVariationProfileCount);
            computeShader.SetFloat(
                "_StaticWakeVariationProfileHalfWidthPixels",
                acrossPixels);
            computeShader.SetInt(
                "_StaticPressureGeometryValid",
                pressureGeometryValid ? 1 : 0);
            computeShader.SetInt(
                "_StaticPressureProfileCount",
                pressurePass && pressureProfile.IsValid
                    ? pressureProfile.LateralSampleCount
                    : 0);
            computeShader.SetFloat(
                "_StaticPressureProfileHalfWidthPixels",
                pressurePass && pressureProfile.IsValid
                    ? pressureProfile.AcrossHalfWidth /
                      Mathf.Max(0.001f, cellSizeY)
                    : acrossPixels);
            computeShader.SetInt(
                "_StaticPressureProfileValid",
                pressurePass && pressureProfile.IsValid ? 1 : 0);
            computeShader.SetFloat(
                "_StaticTargetHeight",
                Mathf.Clamp(
                    targetHeightMetres,
                    0f,
                    MaximumStaticPressureHeightMetres));
            computeShader.SetFloat(
                "_StaticPressureDepthPixels",
                pressureDepthPixels);
            computeShader.SetFloat(
                "_StaticPressureInsideOverlapPixels",
                pressureInsideOverlapPixels);
            computeShader.SetFloat(
                "_StaticMaximumHeight",
                MaximumStaticPressureHeightMetres);
            computeShader.SetFloat(
                "_StaticWakeSourceStrength",
                Mathf.Clamp(wakeAmplitude, 0f, 4f));
            computeShader.SetFloat(
                "_StaticWakePersistence",
                Mathf.Clamp(wakePersistence, 0.25f, 3f));
            computeShader.SetFloat(
                "_StaticWakeSpread",
                Mathf.Clamp(wakeSpread, 0.5f, 2f));
            StaticWakeReleaseVariationState leftRelease =
                wakeVariation.Left;
            StaticWakeReleaseVariationState rightRelease =
                wakeVariation.Right;
            computeShader.SetVector(
                "_StaticWakeLeftReleaseVariation",
                new Vector4(
                    leftRelease.CurrentLateralOffset,
                    leftRelease.CurrentEnergyMultiplier,
                    leftRelease.CurrentWidthMultiplier,
                    leftRelease.CurrentDownstreamOffset));
            computeShader.SetVector(
                "_StaticWakeRightReleaseVariation",
                new Vector4(
                    rightRelease.CurrentLateralOffset,
                    rightRelease.CurrentEnergyMultiplier,
                    rightRelease.CurrentWidthMultiplier,
                    rightRelease.CurrentDownstreamOffset));
            computeShader.SetFloat(
                "_StaticPhase",
                Mathf.Repeat(phase, 1f));
            computeShader.SetFloat(
                "_StaticContactSharpness",
                Mathf.Clamp(responseStiffness, 0.5f, 4f));
            computeShader.SetFloat(
                "_StaticWaveResponse",
                Mathf.Clamp(unsteadiness, 0f, 2f));
            computeShader.SetFloat(
                "_MaximumHeight",
                river.ResolvedImpactRippleMaximumHeight);
            computeShader.SetTexture(
                kernel,
                pressurePass
                    ? "_StaticPressureWrite"
                    : "_StaticWakeSourceWrite",
                targetTexture);
            DispatchCompute(
                kernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1,
                pressurePass
                    ? PerformanceDispatchCategory.StaticPressureBake
                    : PerformanceDispatchCategory.StaticWakeBake,
                width,
                height);
        }

        private int UploadStaticContour(
            Vector2[] contour,
            float cellSizeX,
            float cellSizeY)
        {
            int contourCount = Mathf.Min(
                contour != null ? contour.Length : 0,
                MaximumStaticContourPoints);
            for (int index = 0; index < MaximumStaticContourPoints; index++)
            {
                if (index < contourCount)
                {
                    Vector2 point = contour[index];
                    staticContourUpload[index] = new Vector4(
                        point.x / Mathf.Max(0.001f, cellSizeX),
                        point.y / Mathf.Max(0.001f, cellSizeY),
                        0f,
                        0f);
                }
                else
                {
                    staticContourUpload[index] = Vector4.zero;
                }
            }

            return contourCount;
        }

        private bool UploadStaticPressureProfile(
            bool pressurePass,
            RiverDisturbancePressureBakeProfile pressureProfile,
            float cellSizeX)
        {
            bool pressureGeometryValid =
                pressurePass &&
                pressureProfile.IsValid &&
                pressureProfile.HasGeometryBounds;
            for (int index = 0;
                 index < staticPressureProfileUpload.Length;
                 index++)
            {
                if (pressurePass &&
                    pressureProfile.IsValid &&
                    index < pressureProfile.Samples.Length)
                {
                    Vector4 sample = pressureProfile.Samples[index];
                    staticPressureProfileUpload[index] = new Vector4(
                        sample.x / Mathf.Max(0.001f, cellSizeX),
                        sample.y / Mathf.Max(0.001f, cellSizeX),
                        sample.z,
                        sample.w);
                    staticPressureGeometryUpload[index] =
                        pressureGeometryValid &&
                        index < pressureProfile.DownstreamBoundaries.Length
                            ? new Vector4(
                                pressureProfile.DownstreamBoundaries[index] /
                                Mathf.Max(0.001f, cellSizeX),
                                0f,
                                0f,
                                0f)
                            : Vector4.zero;
                }
                else
                {
                    staticPressureProfileUpload[index] = Vector4.zero;
                    staticPressureGeometryUpload[index] = Vector4.zero;
                }
            }

            return pressureGeometryValid;
        }

        private int UploadStaticWakeVariationProfile(
            bool pressurePass,
            StaticWakeLeeVariationState wakeLeeVariation)
        {
            int wakeVariationProfileCount =
                !pressurePass &&
                HasValidStaticWakeLeeVariationState(wakeLeeVariation)
                    ? wakeLeeVariation.SampleCount
                    : 0;
            for (int index = 0;
                 index < staticWakeVariationProfileUpload.Length;
                 index++)
            {
                staticWakeVariationProfileUpload[index] =
                    index < wakeVariationProfileCount
                        ? new Vector4(
                            wakeLeeVariation.
                                CurrentDepthMultipliers[index],
                            wakeLeeVariation.
                                CurrentLengthMultipliers[index],
                            wakeLeeVariation.
                                CurrentTrailingEdgeOffsets[index],
                            1f)
                        : new Vector4(1f, 1f, 0f, 0f);
            }

            return wakeVariationProfileCount;
        }
    }
}
