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
        private void SimulateRippleField(float deltaTime)
        {
            if (!HasRippleActiveChunks())
            {
                activeRippleMinimumCellSize = 0f;
                rippleSubstepLimitReached = false;
                RecordRippleSubstepDiagnostics(0);
                return;
            }

            float propagationSpeed = Mathf.Max(
                0.01f,
                river.ImpactRipplePropagation);
            float inverseLength = ResolveActiveRippleStabilityInverseLength(
                out activeRippleMinimumCellSize);
            float maximumStableStep =
                RippleStabilitySafety /
                Mathf.Max(0.0001f, propagationSpeed * inverseLength);
            int requiredSubstepCount = Mathf.Max(
                1,
                Mathf.CeilToInt(deltaTime / maximumStableStep));
            rippleSubstepLimitReached =
                requiredSubstepCount > MaximumRippleSubsteps;
            int substepCount = Mathf.Min(
                requiredSubstepCount,
                MaximumRippleSubsteps);
            RecordRippleSubstepDiagnostics(substepCount);
            float substepDelta = deltaTime / substepCount;
            float dampingPerSecond = river.ResolvedImpactRippleDecay;
            float centrelineCellSize = Mathf.Max(
                0.001f,
                fieldLength / Mathf.Max(1, fieldWidth - 1));

            for (int substep = 0; substep < substepCount; substep++)
            {
                float advectionPixels =
                    Mathf.Abs(river.FlowSpeedMetresPerSecond) *
                    substepDelta /
                    centrelineCellSize;

                computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
                computeShader.SetFloat("_DeltaTime", substepDelta);
                computeShader.SetFloat("_PropagationSpeed", propagationSpeed);
                computeShader.SetFloat("_DampingPerSecond", dampingPerSecond);
                computeShader.SetFloat(
                    "_AdvectionPixels",
                    advectionPixels);
                computeShader.SetInt("_RippleMetricCount", fieldWidth);
                computeShader.SetFloat(
                    "_MaximumHeight",
                    river.ResolvedImpactRippleMaximumHeight);
                computeShader.SetBuffer(
                    simulateRippleKernel,
                    "_RippleMetricData",
                    rippleMetricBuffer);
                computeShader.SetTexture(
                    simulateRippleKernel,
                    "_RippleBoundaryRead",
                    rippleBoundary);
                computeShader.SetTexture(
                    simulateRippleKernel,
                    "_StateRead",
                    currentState);
                computeShader.SetTexture(
                    simulateRippleKernel,
                    "_StateWrite",
                    writeState);

                DispatchRippleActiveRanges();

                RenderTexture oldCurrent = currentState;
                currentState = writeState;
                previousState = oldCurrent;
                writeState = oldCurrent;
            }
        }

        private void RebuildRippleBoundary(double now)
        {
            if (rippleBoundary == null ||
                computeShader == null ||
                rippleMetricBuffer == null)
            {
                return;
            }

            RecordFieldRebuild();
            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetInt("_RippleMetricCount", fieldWidth);
            computeShader.SetFloat(
                "_RippleShoreReflection",
                river.ImpactRippleShoreReflection);
            computeShader.SetBuffer(
                bakeRippleBoundaryBaseKernel,
                "_RippleMetricData",
                rippleMetricBuffer);
            computeShader.SetTexture(
                bakeRippleBoundaryBaseKernel,
                "_RippleBoundaryWrite",
                rippleBoundary);
            DispatchCompute(
                bakeRippleBoundaryBaseKernel,
                Mathf.CeilToInt(fieldWidth / (float)ThreadGroupSize),
                Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.RippleBoundaryBake,
                fieldWidth,
                fieldHeight);

            rippleCollisionSourceCount = 0;
            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (!source.IsStatic ||
                    !source.RippleCollisionEnabled)
                {
                    continue;
                }

                if (DispatchRippleBoundaryObstacle(source))
                {
                    rippleCollisionSourceCount++;
                }
            }

            ApplyRippleBoundaryToState(stateA);
            ApplyRippleBoundaryToState(stateB);
            rippleBoundaryDirty = false;
            lastActivityTime = now;
        }

        private float ResolveRippleBoundaryEdgeWidth(float centreX)
        {
            int row = Mathf.Clamp(
                Mathf.RoundToInt(centreX),
                0,
                Mathf.Max(0, fieldWidth - 1));
            float along =
                row < rippleMetricMinimumAlongCell.Length
                    ? rippleMetricMinimumAlongCell[row]
                    : fieldLength / Mathf.Max(1, fieldWidth - 1);
            float lateral =
                row < rippleMetricMinimumLateralCell.Length
                    ? rippleMetricMinimumLateralCell[row]
                    : along;
            return Mathf.Min(
                Mathf.Max(0.001f, along),
                Mathf.Max(0.001f, lateral)) * 0.50f;
        }

        private void ApplyRippleBoundaryToState(RenderTexture state)
        {
            if (state == null)
            {
                return;
            }

            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetTexture(
                applyRippleBoundaryKernel,
                "_RippleBoundaryRead",
                rippleBoundary);
            computeShader.SetTexture(
                applyRippleBoundaryKernel,
                "_StateWrite",
                state);
            DispatchCompute(
                applyRippleBoundaryKernel,
                Mathf.CeilToInt(fieldWidth / (float)ThreadGroupSize),
                Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                1,
                PerformanceDispatchCategory.RippleBoundaryBake,
                fieldWidth,
                fieldHeight);
        }

        private void RecordRippleSubstepDiagnostics(int substepCount)
        {
            currentRippleSubstepCount = Mathf.Max(0, substepCount);
            double now = Time.realtimeSinceStartupAsDouble;

            if (rippleSubstepDiagnosticWindowStart <= 0.0 ||
                now - rippleSubstepDiagnosticWindowStart >=
                RippleSubstepDiagnosticWindowSeconds)
            {
                rippleSubstepDiagnosticWindowStart = now;
                maximumRecentRippleSubstepCount =
                    currentRippleSubstepCount;
                return;
            }

            maximumRecentRippleSubstepCount = Mathf.Max(
                maximumRecentRippleSubstepCount,
                currentRippleSubstepCount);
        }

        private bool BuildRippleMetricData()
        {
            if (river == null ||
                !river.Domain.IsValid ||
                fieldWidth < 2 ||
                fieldHeight < 2 ||
                chunkCount < 1 ||
                resolutionPerChunk < 1)
            {
                return false;
            }

            ReleaseBuffer(ref rippleMetricBuffer);

            try
            {
                Vector2[] centres = new Vector2[fieldWidth];
                Vector2[] tangents = new Vector2[fieldWidth];
                Vector2[] sides = new Vector2[fieldWidth];
                float[] leftWidths = new float[fieldWidth];
                float[] rightWidths = new float[fieldWidth];
                rippleMetricMinimumAlongCell = new float[fieldWidth];
                rippleMetricMinimumLateralCell = new float[fieldWidth];

                float longitudinalDenominator = Mathf.Max(1, fieldWidth - 1);
                for (int row = 0; row < fieldWidth; row++)
                {
                    float orientedDistance = Mathf.Min(
                        row / longitudinalDenominator * fieldLength,
                        validFieldLength);
                    ResolveRippleMetricRow(
                        orientedDistance,
                        out centres[row],
                        out tangents[row],
                        out sides[row],
                        out leftWidths[row],
                        out rightWidths[row]);
                }

                float nominalAlongCell = Mathf.Max(
                    0.001f,
                    fieldLength / longitudinalDenominator);
                float lateralDenominator = Mathf.Max(1, fieldHeight - 1);

                for (int row = 0; row < fieldWidth; row++)
                {
                    float minimumLateral = float.PositiveInfinity;
                    for (int lateral = 0; lateral < fieldHeight - 1; lateral++)
                    {
                        float acrossA =
                            lateral / lateralDenominator * 2f - 1f;
                        float acrossB =
                            (lateral + 1) / lateralDenominator * 2f - 1f;
                        Vector2 positionA = ResolveRippleMetricWorldPosition(
                            centres[row],
                            sides[row],
                            leftWidths[row],
                            rightWidths[row],
                            acrossA);
                        Vector2 positionB = ResolveRippleMetricWorldPosition(
                            centres[row],
                            sides[row],
                            leftWidths[row],
                            rightWidths[row],
                            acrossB);
                        float distance = Vector2.Distance(positionA, positionB);
                        if (distance > 0.0001f)
                        {
                            minimumLateral = Mathf.Min(
                                minimumLateral,
                                distance);
                        }
                    }

                    rippleMetricMinimumLateralCell[row] =
                        float.IsPositiveInfinity(minimumLateral)
                            ? Mathf.Max(
                                0.001f,
                                Mathf.Min(leftWidths[row], rightWidths[row]) *
                                2f / lateralDenominator)
                            : minimumLateral;
                }

                for (int row = 0; row < fieldWidth; row++)
                {
                    float minimumAlong = float.PositiveInfinity;
                    for (int lateral = 0; lateral < fieldHeight; lateral++)
                    {
                        float across =
                            lateral / lateralDenominator * 2f - 1f;
                        Vector2 centrePosition = ResolveRippleMetricWorldPosition(
                            centres[row],
                            sides[row],
                            leftWidths[row],
                            rightWidths[row],
                            across);

                        if (row > 0)
                        {
                            Vector2 previousPosition =
                                ResolveRippleMetricWorldPosition(
                                    centres[row - 1],
                                    sides[row - 1],
                                    leftWidths[row - 1],
                                    rightWidths[row - 1],
                                    across);
                            float distance = Vector2.Distance(
                                centrePosition,
                                previousPosition);
                            if (distance > 0.0001f)
                            {
                                minimumAlong = Mathf.Min(
                                    minimumAlong,
                                    distance);
                            }
                        }

                        if (row + 1 < fieldWidth)
                        {
                            Vector2 nextPosition =
                                ResolveRippleMetricWorldPosition(
                                    centres[row + 1],
                                    sides[row + 1],
                                    leftWidths[row + 1],
                                    rightWidths[row + 1],
                                    across);
                            float distance = Vector2.Distance(
                                centrePosition,
                                nextPosition);
                            if (distance > 0.0001f)
                            {
                                minimumAlong = Mathf.Min(
                                    minimumAlong,
                                    distance);
                            }
                        }
                    }

                    rippleMetricMinimumAlongCell[row] =
                        float.IsPositiveInfinity(minimumAlong)
                            ? nominalAlongCell
                            : minimumAlong;
                }

                RippleMetricRowData[] upload =
                    new RippleMetricRowData[fieldWidth];
                rippleChunkMaximumInverseLength = new float[chunkCount];
                rippleChunkMinimumCellSize = new float[chunkCount];
                for (int chunk = 0; chunk < chunkCount; chunk++)
                {
                    rippleChunkMinimumCellSize[chunk] =
                        float.PositiveInfinity;
                }

                for (int row = 0; row < fieldWidth; row++)
                {
                    float minimumAlong = Mathf.Max(
                        0.001f,
                        rippleMetricMinimumAlongCell[row]);
                    float minimumLateral = Mathf.Max(
                        0.001f,
                        rippleMetricMinimumLateralCell[row]);
                    upload[row] = new RippleMetricRowData
                    {
                        CentreAndTangent = new Vector4(
                            centres[row].x,
                            centres[row].y,
                            tangents[row].x,
                            tangents[row].y),
                        SideAndWidths = new Vector4(
                            sides[row].x,
                            sides[row].y,
                            leftWidths[row],
                            rightWidths[row])
                    };

                    int chunk = Mathf.Clamp(
                        row / resolutionPerChunk,
                        0,
                        chunkCount - 1);
                    float inverseLength = Mathf.Sqrt(
                        1f / (minimumAlong * minimumAlong) +
                        1f / (minimumLateral * minimumLateral));
                    rippleChunkMaximumInverseLength[chunk] = Mathf.Max(
                        rippleChunkMaximumInverseLength[chunk],
                        inverseLength);
                    rippleChunkMinimumCellSize[chunk] = Mathf.Min(
                        rippleChunkMinimumCellSize[chunk],
                        Mathf.Min(minimumAlong, minimumLateral));
                }

                for (int chunk = 0; chunk < chunkCount; chunk++)
                {
                    if (rippleChunkMaximumInverseLength[chunk] <= 0f)
                    {
                        rippleChunkMaximumInverseLength[chunk] =
                            Mathf.Sqrt(2f) / nominalAlongCell;
                    }

                    if (float.IsPositiveInfinity(
                            rippleChunkMinimumCellSize[chunk]))
                    {
                        rippleChunkMinimumCellSize[chunk] =
                            nominalAlongCell;
                    }
                }

                rippleMetricBuffer = new ComputeBuffer(
                    upload.Length,
                    sizeof(float) * 8,
                    ComputeBufferType.Structured);
                rippleMetricBuffer.SetData(upload);
                return true;
            }
            catch (Exception exception)
            {
                ReleaseBuffer(ref rippleMetricBuffer);
                Debug.LogError(
                    $"StylizedRiver on '{name}' could not build its " +
                    $"Impact Ripple metric buffer. {exception.Message}",
                    this);
                return false;
            }
        }

        private void ResolveRippleMetricRow(
            float orientedDistance,
            out Vector2 centre,
            out Vector2 tangent,
            out Vector2 side,
            out float leftWidth,
            out float rightWidth)
        {
            float clampedDistance = Mathf.Clamp(
                orientedDistance,
                0f,
                river.Domain.LocalLength);
            StylizedRiverSplineSample sample =
                river.Domain.SampleAtOrientedDistance(clampedDistance);
            Vector3 surfacePoint = sample.SurfacePoint;
            float extrapolation = Mathf.Max(
                0f,
                orientedDistance - river.Domain.LocalLength);

            if (extrapolation > 0.0001f)
            {
                Vector3 downstreamTangent = river.Domain.ReverseFlow
                    ? -sample.Tangent
                    : sample.Tangent;
                downstreamTangent.y = 0f;
                if (downstreamTangent.sqrMagnitude > 0.000001f)
                {
                    surfacePoint +=
                        downstreamTangent.normalized * extrapolation;
                }
            }

            Vector2 resolvedSide = new Vector2(
                sample.Side.x,
                sample.Side.z);
            if (resolvedSide.sqrMagnitude <= 0.000001f)
            {
                resolvedSide = Vector2.right;
            }
            else
            {
                resolvedSide.Normalize();
            }

            Vector3 downstreamTangent3 = river.Domain.ReverseFlow
                ? -sample.Tangent
                : sample.Tangent;
            Vector2 resolvedTangent = new Vector2(
                downstreamTangent3.x,
                downstreamTangent3.z);
            if (resolvedTangent.sqrMagnitude <= 0.000001f)
            {
                resolvedTangent = new Vector2(
                    -resolvedSide.y,
                    resolvedSide.x);
            }
            else
            {
                resolvedTangent.Normalize();
            }

            centre = new Vector2(surfacePoint.x, surfacePoint.z);
            tangent = resolvedTangent;
            side = resolvedSide;
            leftWidth = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
            rightWidth = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
        }

        private static Vector2 ResolveRippleMetricWorldPosition(
            Vector2 centre,
            Vector2 side,
            float leftWidth,
            float rightWidth,
            float acrossNormalized)
        {
            float clampedAcross = Mathf.Clamp(acrossNormalized, -1f, 1f);
            float width = clampedAcross < 0f
                ? leftWidth
                : rightWidth;
            return centre + side * (clampedAcross * width);
        }

        private float ResolveActiveRippleStabilityInverseLength(
            out float minimumCellSize)
        {
            float maximumInverseLength = 0f;
            minimumCellSize = float.PositiveInfinity;

            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (chunk >= chunkActive.Length || !chunkActive[chunk])
                {
                    continue;
                }

                if (chunk < rippleChunkMaximumInverseLength.Length)
                {
                    maximumInverseLength = Mathf.Max(
                        maximumInverseLength,
                        rippleChunkMaximumInverseLength[chunk]);
                }

                if (chunk < rippleChunkMinimumCellSize.Length)
                {
                    minimumCellSize = Mathf.Min(
                        minimumCellSize,
                        rippleChunkMinimumCellSize[chunk]);
                }
            }

            if (maximumInverseLength <= 0f)
            {
                float fallbackCell = Mathf.Max(
                    0.001f,
                    fieldLength / Mathf.Max(1, fieldWidth - 1));
                maximumInverseLength = Mathf.Sqrt(2f) / fallbackCell;
                minimumCellSize = fallbackCell;
            }
            else if (float.IsPositiveInfinity(minimumCellSize))
            {
                minimumCellSize = 0f;
            }

            return maximumInverseLength;
        }

        private void ResolveRippleInjectionRadiusPixels(
            float centreX,
            float radiusMetres,
            out float radiusX,
            out float radiusY)
        {
            float nominalAlongCell = Mathf.Max(
                0.001f,
                fieldLength / Mathf.Max(1, fieldWidth - 1));
            int estimateRadius = Mathf.CeilToInt(
                radiusMetres / nominalAlongCell) + 2;
            int minRow = Mathf.Clamp(
                Mathf.FloorToInt(centreX) - estimateRadius,
                0,
                fieldWidth - 1);
            int maxRow = Mathf.Clamp(
                Mathf.CeilToInt(centreX) + estimateRadius,
                0,
                fieldWidth - 1);
            float minimumAlong = ResolveMinimumMetricValue(
                rippleMetricMinimumAlongCell,
                minRow,
                maxRow,
                nominalAlongCell);
            radiusX = radiusMetres / Mathf.Max(0.001f, minimumAlong);

            minRow = Mathf.Clamp(
                Mathf.FloorToInt(centreX - radiusX) - 2,
                0,
                fieldWidth - 1);
            maxRow = Mathf.Clamp(
                Mathf.CeilToInt(centreX + radiusX) + 2,
                0,
                fieldWidth - 1);
            minimumAlong = ResolveMinimumMetricValue(
                rippleMetricMinimumAlongCell,
                minRow,
                maxRow,
                minimumAlong);
            float minimumLateral = ResolveMinimumMetricValue(
                rippleMetricMinimumLateralCell,
                minRow,
                maxRow,
                nominalAlongCell);
            radiusX = radiusMetres / Mathf.Max(0.001f, minimumAlong);
            radiusY = radiusMetres / Mathf.Max(0.001f, minimumLateral);
        }

        private static float ResolveMinimumMetricValue(
            float[] values,
            int minimumIndex,
            int maximumIndex,
            float fallback)
        {
            if (values == null || values.Length == 0)
            {
                return Mathf.Max(0.001f, fallback);
            }

            int safeMinimum = Mathf.Clamp(
                minimumIndex,
                0,
                values.Length - 1);
            int safeMaximum = Mathf.Clamp(
                maximumIndex,
                safeMinimum,
                values.Length - 1);
            float minimum = float.PositiveInfinity;
            for (int index = safeMinimum; index <= safeMaximum; index++)
            {
                float value = values[index];
                if (value > 0.0001f)
                {
                    minimum = Mathf.Min(minimum, value);
                }
            }

            return float.IsPositiveInfinity(minimum)
                ? Mathf.Max(0.001f, fallback)
                : minimum;
        }
    }
}
