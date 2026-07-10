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
        private struct FoamObstacleRoutingComponent
        {
            public int Id;
            public int MinX;
            public int MaxX;
            public int MinY;
            public int MaxY;
            public int Count;
            public float SumX;
            public float SumY;

            public float AverageX => Count > 0 ? SumX / Count : 0f;
            public float AverageY => Count > 0 ? SumY / Count : 0f;
        }

        private void RequestBoundaryRebuild()
        {
            pendingBoundaryRebuild = true;
            pendingTopologyReplacementAfterMaintenance =
                DevelopmentTopologyGenerationInProgress;
            pendingTopologyRefresh = true;
        }

        private void RequestObstacleRebuild(
            int currentObstacleVersion,
            bool prepareTopologyReplacement)
        {
            pendingObstacleRebuild = true;
            pendingTopologyReplacementAfterMaintenance |=
                prepareTopologyReplacement;
            pendingTopologyRefresh = true;

            if (pendingObstacleObservedVersion != currentObstacleVersion)
            {
                pendingObstacleObservedVersion = currentObstacleVersion;
                pendingObstacleStableFrameCount = 0;
            }
        }

        private void QueueObstacleRebuildIfNeeded()
        {
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            int currentObstacleVersion = disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;

            if (currentObstacleVersion == obstacleGeometryVersion)
            {
                return;
            }

            if (DevelopmentTopologyGenerationInProgress)
            {
                RequestObstacleRebuild(
                    currentObstacleVersion,
                    true);
                return;
            }

            activeTopologyObstacleStale = true;
            if (IsAutomaticDevelopmentCacheEnabled)
            {
                automaticTopologyGenerationInProgress = true;
                automaticDevelopmentRebuildReason =
                    AutomaticDevelopmentRebuildReason.Obstacles;
                topologyCacheStartupState =
                    "Using Previous Cache — Rebuilding";
                topologyCacheStartupSummary =
                    "Static obstacle geometry changed. The previous generated " +
                    "topology remains visible while the exact live obstacle " +
                    "field and a complete replacement are prepared " +
                    "automatically.";
                RequestObstacleRebuild(
                    currentObstacleVersion,
                    true);
                return;
            }

            MarkActiveTopologyCacheStale(
                "Stale — Obstacles Changed",
                "Static obstacle geometry changed after activation. The " +
                "exact live obstacle field is refreshed, while the generated " +
                "topology is retained until explicit development regeneration " +
                "or a valid cache reload.");
            RequestObstacleRebuild(
                currentObstacleVersion,
                false);
        }

        private int ResolveCurrentObstacleGeometryVersion()
        {
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            return disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;
        }

        private void RebuildBoundaryTexture(bool applyToExistingState = true)
        {
            using var profilerScope = InitBuildBoundaryProfilerMarker.Auto();
            if (fieldWidth <= 0 || fieldHeight <= 0 || metricRows.Length != fieldWidth)
            {
                return;
            }

            Color[] pixels = new Color[fieldWidth * fieldHeight];
            float edgeCells = river.Quality switch
            {
                StylizedRiverQuality.Low => 1.5f,
                StylizedRiverQuality.Medium => 2.0f,
                StylizedRiverQuality.High => 2.5f,
                _ => 2.0f
            };

            for (int x = 0; x < fieldWidth; x++)
            {
                float localDistance =
                    StylizedRiverFoamTopologyFieldSpace.LocalDistanceAtTexel(
                        x,
                        fieldWidth,
                        fieldLength);
                if (localDistance > simulationFieldLength + 0.0001f)
                {
                    continue;
                }

                float globalDistance =
                    river.Domain.GlobalDistanceMinimum +
                    Mathf.Min(localDistance, validFieldLength);
                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float leftSurface = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
                float rightSurface = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
                float leftVisible = Mathf.Max(0.01f, sample.LeftHalfWidth);
                float rightVisible = Mathf.Max(0.01f, sample.RightHalfWidth);
                float animatedEnvelope = Mathf.Lerp(
                    0.25f,
                    0.90f,
                    Mathf.Clamp01(river.ShoreMotion));
                float leftFoamReach = Mathf.Lerp(
                    leftVisible,
                    leftSurface,
                    animatedEnvelope);
                float rightFoamReach = Mathf.Lerp(
                    rightVisible,
                    rightSurface,
                    animatedEnvelope);
                float edgeWidth = Mathf.Max(
                    0.05f,
                    StylizedRiverFoamTopologyFieldSpace.TexelSpacing(
                        leftSurface + rightSurface,
                        fieldHeight) * edgeCells);

                for (int y = 0; y < fieldHeight; y++)
                {
                    float across01 =
                        StylizedRiverFoamTopologyFieldSpace.Across01AtTexel(
                            y,
                            fieldHeight);
                    float lateral =
                        StylizedRiverFoamTopologyFieldSpace.Across01ToMetres(
                        across01,
                        leftSurface,
                        rightSurface);
                    float foamReach = lateral < 0f
                        ? leftFoamReach
                        : rightFoamReach;
                    float distanceInsideReach = foamReach - Mathf.Abs(lateral);
                    float coverage = Mathf.Clamp01(
                        distanceInsideReach / edgeWidth);
                    // The boundary texture stores valid-water coverage only.
                    // Shore Support is reconstructed independently from the
                    // instantaneous Stage 3 edge, and persistent material has
                    // no boundary-attraction or shore-suction channel.
                    pixels[y * fieldWidth + x] = new Color(
                        coverage,
                        0f,
                        0f,
                        0f);
                }
            }

            // Registered solid geometry is no longer rasterised into this
            // static boundary texture. Obstacle Footprint is reconstructed
            // from cached exact transformed-mesh solid intervals in a dedicated
            // point-sampled mask.

            if (boundaryTexture == null ||
                boundaryTexture.width != fieldWidth ||
                boundaryTexture.height != fieldHeight)
            {
                if (boundaryTexture != null)
                {
                    DestroyUnityObject(boundaryTexture);
                }

                boundaryTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RGBAHalf,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamBoundary_Runtime",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            boundaryTexture.SetPixels(pixels);
            boundaryTexture.Apply(false, false);
            boundaryDirty = false;

            if (applyToExistingState)
            {
                ApplyBoundaryToState(stateA);
                ApplyBoundaryToState(stateB);
            }
        }


        private void EnsureMotionFieldsCurrent()
        {
            if (fieldWidth <= 0 || fieldHeight <= 0 || river == null)
            {
                return;
            }

            if (motionLaneTexture == null ||
                motionLaneTexture.width != fieldWidth ||
                motionLaneTexture.height != fieldHeight)
            {
                if (motionLaneTexture != null)
                {
                    DestroyUnityObject(motionLaneTexture);
                }

                motionLaneTexture = CreateMotionLaneTexture();
                motionLaneFieldSignature = int.MinValue;
            }

            if (obstacleRoutingTexture == null ||
                obstacleRoutingTexture.width != fieldWidth ||
                obstacleRoutingTexture.height != fieldHeight)
            {
                if (obstacleRoutingTexture != null)
                {
                    DestroyUnityObject(obstacleRoutingTexture);
                }

                obstacleRoutingTexture = CreateObstacleRoutingTexture();
                obstacleRoutingFieldSignature = int.MinValue;
            }

            int laneSignature = ResolveMotionLaneFieldSignature();
            if (laneSignature != motionLaneFieldSignature)
            {
                RebuildMotionLaneField(laneSignature);
            }

            int routingSignature = ResolveObstacleRoutingFieldSignature();
            if (routingSignature != obstacleRoutingFieldSignature)
            {
                RebuildObstacleRoutingField(routingSignature);
            }
        }

        private void AdvanceMotionLaneScroll(float deltaTime)
        {
            if (river == null || fieldWidth <= 0)
            {
                lastMotionLaneScrollCells = 0f;
                return;
            }

            float longitudinalSpacing =
                minimumTransportLongitudinalSpacing > 0.0001f
                    ? minimumTransportLongitudinalSpacing
                    : fieldLength / Mathf.Max(1, fieldWidth);
            float baseFoamSpeed =
                ResolveBaseFoamDownstreamSpeedMetresPerSecond();
            float scrollMetres = baseFoamSpeed *
                river.FoamLaneAdvectionRatio *
                Mathf.Max(0f, deltaTime) *
                river.FlowDirection;
            float scrollCells = scrollMetres /
                Mathf.Max(0.0001f, longitudinalSpacing);
            motionLaneScrollCells = RepeatSigned(
                motionLaneScrollCells + scrollCells,
                fieldWidth);
            lastMotionLaneScrollCells = motionLaneScrollCells;
        }

        private int ResolveMotionLaneFieldSignature()
        {
            int hash = 17;
            hash = AccumulateHash(hash, fieldWidth);
            hash = AccumulateHash(hash, fieldHeight);
            hash = AccumulateHash(hash, 3); // 4.11C.5.16A.1 independent downstream/across-river frequency algorithm.
            hash = AccumulateHash(hash, river != null ? river.VisualSeed : 0);
            hash = AccumulateHash(
                hash,
                Mathf.RoundToInt((river != null
                    ? river.FoamLowLateralMotionCoverage
                    : 0.10f) * 10000f));
            hash = AccumulateHash(
                hash,
                Mathf.RoundToInt((river != null
                    ? river.FoamDirectionChangeFrequency
                    : 1f) * 1000f));
            hash = AccumulateHash(
                hash,
                Mathf.RoundToInt((river != null
                    ? river.FoamAcrossRiverCoherence
                    : 1f) * 1000f));
            return hash;
        }

        private int ResolveObstacleRoutingFieldSignature()
        {
            int hash = 23;
            hash = AccumulateHash(hash, fieldWidth);
            hash = AccumulateHash(hash, fieldHeight);
            hash = AccumulateHash(hash, obstacleGeometryVersion);
            hash = AccumulateHash(hash, obstacleExclusionCells.Count);
            hash = AccumulateHash(hash, obstacleExclusionSamples.Count);
            hash = AccumulateHash(hash, obstacleExclusionUsesCachedScalar ? 1 : 0);
            hash = AccumulateHash(
                hash,
                river != null && river.FlowDirection < 0f ? -1 : 1);
            return hash;
        }

        private void RebuildMotionLaneField(int signature)
        {
            using var profilerScope = MotionBuildLaneProfilerMarker.Auto();
            if (motionLaneTexture == null || fieldWidth <= 0 || fieldHeight <= 0)
            {
                return;
            }

            int cellCount = fieldWidth * fieldHeight;
            if (motionLaneHalfData.Length != cellCount)
            {
                motionLaneHalfData = new ushort[cellCount];
            }

            if (motionLaneRawValues.Length != cellCount)
            {
                motionLaneRawValues = new float[cellCount];
            }

            float directionChangeFrequency = river != null
                ? river.FoamDirectionChangeFrequency
                : 1f;
            float acrossRiverCoherence = river != null
                ? river.FoamAcrossRiverCoherence
                : 1f;
            float seed = river != null
                ? river.VisualSeed * 0.01371f
                : 23.17f;
            float aspect = fieldHeight > 0
                ? Mathf.Max(1f, (float)fieldWidth / fieldHeight)
                : 1f;

            for (int y = 0; y < fieldHeight; y++)
            {
                float v = ((float)y + 0.5f) / Mathf.Max(1, fieldHeight);
                for (int x = 0; x < fieldWidth; x++)
                {
                    float u = ((float)x + 0.5f) / Mathf.Max(1, fieldWidth);
                    float raw = MotionFractalLaneNoise(
                        u,
                        v,
                        aspect,
                        directionChangeFrequency,
                        acrossRiverCoherence,
                        seed);
                    motionLaneRawValues[y * fieldWidth + x] = Mathf.Clamp(raw, -1f, 1f);
                }
            }

            SmoothMotionLaneAcrossWidth(
                motionLaneRawValues,
                fieldWidth,
                fieldHeight);

            float neutralCoverage = river != null
                ? river.FoamLowLateralMotionCoverage
                : 0.10f;
            float neutralThreshold = ResolveNeutralThreshold(
                motionLaneRawValues,
                Mathf.Clamp01(neutralCoverage));
            const float minimumLaneMotionMagnitude = 0.125f;
            float denominator = Mathf.Max(0.0001f, 1f - neutralThreshold);
            for (int index = 0; index < cellCount; index++)
            {
                float raw = motionLaneRawValues[index];
                float magnitude = Mathf.Abs(raw);
                float resolved = 0f;
                if (magnitude > 0.0001f)
                {
                    float sign = Mathf.Sign(raw);
                    if (neutralThreshold > 0.0001f &&
                        magnitude <= neutralThreshold)
                    {
                        float lowMotionT = Mathf.Clamp01(
                            magnitude / neutralThreshold);
                        lowMotionT = lowMotionT * lowMotionT *
                            (3f - 2f * lowMotionT);
                        resolved = sign *
                            minimumLaneMotionMagnitude *
                            lowMotionT;
                    }
                    else
                    {
                        float normalized = Mathf.Clamp01(
                            (magnitude - neutralThreshold) / denominator);
                        normalized = normalized * normalized *
                            (3f - 2f * normalized);
                        resolved = sign * Mathf.Lerp(
                            minimumLaneMotionMagnitude,
                            1f,
                            normalized);
                    }
                }

                motionLaneHalfData[index] = Mathf.FloatToHalf(resolved);
            }

            motionLaneTexture.SetPixelData(motionLaneHalfData, 0);
            motionLaneTexture.Apply(false, false);
            motionLaneFieldSignature = signature;
            lastMotionLaneSignature = signature;
        }

        private static void SmoothMotionLaneAcrossWidth(
            float[] values,
            int width,
            int height)
        {
            if (values == null || width <= 0 || height <= 0 ||
                values.Length != width * height)
            {
                return;
            }

            float[] buffer = new float[values.Length];
            for (int pass = 0; pass < 2; pass++)
            {
                for (int y = 0; y < height; y++)
                {
                    int y0 = Mathf.Max(0, y - 2);
                    int y1 = Mathf.Max(0, y - 1);
                    int y2 = Mathf.Min(height - 1, y + 1);
                    int y3 = Mathf.Min(height - 1, y + 2);
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * width + x;
                        float centre = values[index];
                        float near = values[y1 * width + x] +
                            values[y2 * width + x];
                        float far = values[y0 * width + x] +
                            values[y3 * width + x];
                        buffer[index] = Mathf.Clamp(
                            centre * 0.52f + near * 0.19f + far * 0.05f,
                            -1f,
                            1f);
                    }
                }

                Array.Copy(buffer, values, values.Length);
            }
        }

        private void RebuildObstacleRoutingField(int signature)
        {
            using var profilerScope = MotionBuildObstacleRoutingProfilerMarker.Auto();
            if (obstacleRoutingTexture == null || fieldWidth <= 0 || fieldHeight <= 0)
            {
                return;
            }

            int cellCount = fieldWidth * fieldHeight;
            int halfCount = cellCount * 2;
            if (obstacleRoutingHalfData.Length != halfCount)
            {
                obstacleRoutingHalfData = new ushort[halfCount];
            }
            else
            {
                Array.Clear(obstacleRoutingHalfData, 0, obstacleRoutingHalfData.Length);
            }

            EnsureObstacleRoutingScratch(cellCount);
            BuildObstacleRoutingOccupancy(cellCount);
            BuildObstacleRoutingComponents();

            for (int index = 0; index < obstacleRoutingComponents.Count; index++)
            {
                StampObstacleRoutingComponent(obstacleRoutingComponents[index]);
            }

            obstacleRoutingTexture.SetPixelData(obstacleRoutingHalfData, 0);
            obstacleRoutingTexture.Apply(false, false);
            obstacleRoutingFieldSignature = signature;
            lastObstacleRoutingSignature = signature;
        }

        private void EnsureObstacleRoutingScratch(int cellCount)
        {
            if (obstacleRoutingOccupied.Length != cellCount)
            {
                obstacleRoutingOccupied = new bool[cellCount];
            }
            else
            {
                Array.Clear(obstacleRoutingOccupied, 0, obstacleRoutingOccupied.Length);
            }

            if (obstacleRoutingVisited.Length != cellCount)
            {
                obstacleRoutingVisited = new bool[cellCount];
            }
            else
            {
                Array.Clear(obstacleRoutingVisited, 0, obstacleRoutingVisited.Length);
            }

            if (obstacleRoutingComponentIds.Length != cellCount)
            {
                obstacleRoutingComponentIds = new int[cellCount];
            }
            else
            {
                Array.Clear(obstacleRoutingComponentIds, 0, obstacleRoutingComponentIds.Length);
            }

            if (obstacleRoutingQueue.Length != cellCount)
            {
                obstacleRoutingQueue = new int[cellCount];
            }

            obstacleRoutingComponents.Clear();
        }

        private void BuildObstacleRoutingOccupancy(int cellCount)
        {
            if (obstacleExclusionCells.Count > 0)
            {
                for (int index = 0; index < obstacleExclusionCells.Count; index++)
                {
                    Vector2Int coordinate = obstacleExclusionCells[index].Coordinate;
                    if (coordinate.x < 0 || coordinate.x >= fieldWidth ||
                        coordinate.y < 0 || coordinate.y >= fieldHeight)
                    {
                        continue;
                    }

                    obstacleRoutingOccupied[coordinate.y * fieldWidth + coordinate.x] = true;
                }

                return;
            }

            if (obstacleExclusionScalar.Length != cellCount)
            {
                return;
            }

            for (int index = 0; index < cellCount; index++)
            {
                obstacleRoutingOccupied[index] = obstacleExclusionScalar[index] > 0.35f;
            }
        }

        private void BuildObstacleRoutingComponents()
        {
            for (int y = 0; y < fieldHeight; y++)
            {
                for (int x = 0; x < fieldWidth; x++)
                {
                    int index = y * fieldWidth + x;
                    if (!obstacleRoutingOccupied[index] || obstacleRoutingVisited[index])
                    {
                        continue;
                    }

                    int componentId = obstacleRoutingComponents.Count + 1;
                    FoamObstacleRoutingComponent component = FloodObstacleRoutingComponent(
                        x,
                        y,
                        componentId);
                    if (component.Count > 0)
                    {
                        obstacleRoutingComponents.Add(component);
                    }
                }
            }
        }

        private FoamObstacleRoutingComponent FloodObstacleRoutingComponent(
            int startX,
            int startY,
            int componentId)
        {
            int read = 0;
            int write = 0;
            int startIndex = startY * fieldWidth + startX;
            obstacleRoutingQueue[write++] = startIndex;
            obstacleRoutingVisited[startIndex] = true;
            obstacleRoutingComponentIds[startIndex] = componentId;

            FoamObstacleRoutingComponent component = new FoamObstacleRoutingComponent
            {
                Id = componentId,
                MinX = startX,
                MaxX = startX,
                MinY = startY,
                MaxY = startY,
                Count = 0,
                SumX = 0f,
                SumY = 0f
            };

            while (read < write)
            {
                int index = obstacleRoutingQueue[read++];
                int x = index % fieldWidth;
                int y = index / fieldWidth;

                component.Count++;
                component.SumX += x;
                component.SumY += y;
                component.MinX = Mathf.Min(component.MinX, x);
                component.MaxX = Mathf.Max(component.MaxX, x);
                component.MinY = Mathf.Min(component.MinY, y);
                component.MaxY = Mathf.Max(component.MaxY, y);

                for (int offsetY = -1; offsetY <= 1; offsetY++)
                {
                    int ny = y + offsetY;
                    if (ny < 0 || ny >= fieldHeight)
                    {
                        continue;
                    }

                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        if (offsetX == 0 && offsetY == 0)
                        {
                            continue;
                        }

                        int nx = x + offsetX;
                        if (nx < 0 || nx >= fieldWidth)
                        {
                            continue;
                        }

                        int neighbourIndex = ny * fieldWidth + nx;
                        if (!obstacleRoutingOccupied[neighbourIndex] ||
                            obstacleRoutingVisited[neighbourIndex])
                        {
                            continue;
                        }

                        obstacleRoutingVisited[neighbourIndex] = true;
                        obstacleRoutingComponentIds[neighbourIndex] = componentId;
                        obstacleRoutingQueue[write++] = neighbourIndex;
                    }
                }
            }

            return component;
        }

        private void StampObstacleRoutingComponent(
            FoamObstacleRoutingComponent component)
        {
            if (component.Count <= 0)
            {
                return;
            }

            int flowSign = river != null && river.FlowDirection < 0f ? -1 : 1;
            int minS = Mathf.Min(flowSign * component.MinX, flowSign * component.MaxX);
            int maxS = Mathf.Max(flowSign * component.MinX, flowSign * component.MaxX);
            int componentWidth = Mathf.Max(1, maxS - minS + 1);
            int componentHeight = Mathf.Max(1, component.MaxY - component.MinY + 1);

            // Obstacle routing is a collision-shadow, not a proximity halo.
            // The bounds below are only a cheap iteration window.  Written
            // influence is constrained by ResolveComponentCollisionRiskInfluence
            // so side-passing material is not redirected just because it is
            // close to the obstacle.
            int approachCells = Mathf.Max(
                6,
                Mathf.RoundToInt(fieldWidth * 0.055f));
            int frontCells = Mathf.Max(
                1,
                Mathf.RoundToInt(componentWidth * 0.28f));
            int releaseCells = 0;
            int frontClosureCells = Mathf.Clamp(
                Mathf.RoundToInt(componentWidth * 0.08f),
                1,
                2);
            int lateralMarginCells = Mathf.Max(
                1,
                Mathf.RoundToInt(componentHeight * 0.22f));
            float tieSide = ResolveComponentTieSide(
                component,
                flowSign,
                approachCells);

            int startS = minS - approachCells;
            int endS = maxS + releaseCells;
            int startY = Mathf.Max(0, component.MinY - lateralMarginCells);
            int endY = Mathf.Min(fieldHeight - 1, component.MaxY + lateralMarginCells);

            for (int s = startS; s <= endS; s++)
            {
                int x = flowSign * s;
                if (x < 0 || x >= fieldWidth)
                {
                    continue;
                }

                for (int y = startY; y <= endY; y++)
                {
                    float influence = ResolveComponentCollisionRiskInfluence(
                        component,
                        s,
                        y,
                        minS,
                        maxS,
                        approachCells,
                        frontCells,
                        releaseCells,
                        frontClosureCells,
                        lateralMarginCells,
                        flowSign);
                    if (influence <= 0.001f)
                    {
                        continue;
                    }

                    float direction = ResolveComponentRoutingSide(
                        component,
                        y,
                        tieSide);
                    WriteObstacleRoutingCell(x, y, direction, influence);
                }
            }
        }

        private float ResolveComponentTieSide(
            FoamObstacleRoutingComponent component,
            int flowSign,
            int approachCells)
        {
            int minS = Mathf.Min(flowSign * component.MinX, flowSign * component.MaxX);
            int upstreamStartS = minS - approachCells;
            int upstreamEndS = minS - Mathf.Max(1, approachCells / 3);
            int centerY = Mathf.Clamp(
                Mathf.RoundToInt((component.MinY + component.MaxY) * 0.5f),
                0,
                fieldHeight - 1);
            int sampleRadiusY = Mathf.Max(
                1,
                Mathf.RoundToInt(Mathf.Max(1, component.MaxY - component.MinY + 1) * 0.35f));
            float sum = 0f;
            int count = 0;

            for (int s = upstreamStartS; s <= upstreamEndS; s++)
            {
                int x = flowSign * s;
                if (x < 0 || x >= fieldWidth)
                {
                    continue;
                }

                int yMin = Mathf.Max(0, centerY - sampleRadiusY);
                int yMax = Mathf.Min(fieldHeight - 1, centerY + sampleRadiusY);
                for (int y = yMin; y <= yMax; y++)
                {
                    sum += SampleResolvedMotionLaneValue(x, y);
                    count++;
                }
            }

            if (count > 0 && Mathf.Abs(sum) > 0.01f)
            {
                return sum >= 0f ? 1f : -1f;
            }

            return MotionHash01(
                component.AverageX * 0.137f + component.Count * 0.019f,
                component.AverageY * 0.173f + component.MinX * 0.031f) >= 0.5f
                ? 1f
                : -1f;
        }

        private float ResolveComponentCollisionRiskInfluence(
            FoamObstacleRoutingComponent component,
            int s,
            int y,
            int minS,
            int maxS,
            int approachCells,
            int frontCells,
            int releaseCells,
            int frontClosureCells,
            int lateralMarginCells,
            int flowSign)
        {
            float centerY = (component.MinY + component.MaxY) * 0.5f;
            float halfHeight = Mathf.Max(
                0.5f,
                (component.MaxY - component.MinY + 1) * 0.5f);
            float absLateral = Mathf.Abs(y - centerY);

            int rowLeadingS = ResolveComponentRowLeadingS(
                component,
                y,
                flowSign,
                minS);

            // Obstacle routing is now a one-sided collision shadow.  The far
            // upstream tip is soft, but the obstacle-facing end is deliberately
            // not softened: the last valid cells before the obstacle are the
            // highest-risk cells and should be the strongest part of the field.
            // Permit a tiny front-contact closure so the visible routing
            // band touches the obstacle/negative topology boundary instead
            // of stopping one row short.  The lateral collision corridor below
            // still prevents this from recreating a broad side halo.
            int closureLeadingS = rowLeadingS + Mathf.Max(0, frontClosureCells);
            if (s >= closureLeadingS)
            {
                return 0f;
            }

            int upstreamDistance = Mathf.Max(1, rowLeadingS - s);
            if (upstreamDistance > approachCells)
            {
                return 0f;
            }

            float approachRaw = Mathf.Clamp01(
                1f - ((float)upstreamDistance / Mathf.Max(1, approachCells)));
            float approachEase = Mathf.Pow(Smooth01(approachRaw), 2.75f);

            float footprintHalfWidth = halfHeight;
            float safetyMargin = Mathf.Max(1f, halfHeight * 0.10f);
            float directOuterHalfWidth = footprintHalfWidth + safetyMargin;
            float directFootprint = RoundedCorridorFactor(
                absLateral,
                footprintHalfWidth,
                directOuterHalfWidth);

            // The upstream approach starts as a narrow, nearly invisible hint
            // and expands toward the obstacle footprint.  It is still gated by
            // collision overlap so side-passing material can continue downstream
            // almost untouched.
            float approachCoreHalfWidth = Mathf.Lerp(
                Mathf.Max(0.5f, halfHeight * 0.10f),
                Mathf.Max(0.5f, footprintHalfWidth * 0.88f),
                approachEase);
            float approachOuterHalfWidth = Mathf.Lerp(
                Mathf.Max(0.75f, halfHeight * 0.24f),
                directOuterHalfWidth,
                approachEase);
            float collisionCorridor = RoundedCorridorFactor(
                absLateral,
                approachCoreHalfWidth,
                approachOuterHalfWidth);
            if (collisionCorridor <= 0f)
            {
                return 0f;
            }

            float approachCap = Mathf.Lerp(0.035f, 0.58f, approachEase);
            float influence = approachCap * collisionCorridor;

            int contactCells = Mathf.Max(2, Mathf.Min(5, approachCells / 4));
            int contactDistance = Mathf.Max(1, rowLeadingS - s);
            if (contactDistance <= contactCells)
            {
                float contactT = Mathf.Clamp01(
                    1f - ((float)(contactDistance - 1) /
                    Mathf.Max(1, contactCells - 1)));
                contactT = Mathf.Pow(Smooth01(contactT), 0.55f);
                float contactInfluence = Mathf.Lerp(0.70f, 1f, contactT) *
                    directFootprint;
                influence = Mathf.Max(influence, contactInfluence);
            }

            influence = Mathf.Clamp01(influence);
            return influence < 0.03f ? 0f : influence;
        }

        private int ResolveComponentRowLeadingS(
            FoamObstacleRoutingComponent component,
            int y,
            int flowSign,
            int fallbackMinS)
        {
            if (y < component.MinY || y > component.MaxY)
            {
                return fallbackMinS;
            }

            int bestS = int.MaxValue;
            for (int x = component.MinX; x <= component.MaxX; x++)
            {
                int index = y * fieldWidth + x;
                if (index < 0 || index >= obstacleRoutingComponentIds.Length ||
                    obstacleRoutingComponentIds[index] != component.Id)
                {
                    continue;
                }

                bestS = Mathf.Min(bestS, flowSign * x);
            }

            return bestS == int.MaxValue ? fallbackMinS : bestS;
        }

        private static float RoundedCorridorFactor(
            float absLateral,
            float coreHalfWidth,
            float outerHalfWidth)
        {
            if (absLateral <= coreHalfWidth)
            {
                return 1f;
            }

            if (absLateral >= outerHalfWidth)
            {
                return 0f;
            }

            float t = Mathf.Clamp01(
                (absLateral - coreHalfWidth) /
                Mathf.Max(0.001f, outerHalfWidth - coreHalfWidth));
            return 1f - Smooth01(t);
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private float ResolveComponentRoutingSide(
            FoamObstacleRoutingComponent component,
            int y,
            float tieSide)
        {
            float centerY = (component.MinY + component.MaxY) * 0.5f;
            float deadBand = Mathf.Max(
                0.75f,
                (component.MaxY - component.MinY + 1) * 0.10f);
            if (y > centerY + deadBand)
            {
                return 1f;
            }

            if (y < centerY - deadBand)
            {
                return -1f;
            }

            return tieSide >= 0f ? 1f : -1f;
        }

        private float SampleResolvedMotionLaneValue(int x, int y)
        {
            if (x < 0 || x >= fieldWidth || y < 0 || y >= fieldHeight ||
                motionLaneHalfData.Length != fieldWidth * fieldHeight)
            {
                return 0f;
            }

            return Mathf.Clamp(
                Mathf.HalfToFloat(motionLaneHalfData[y * fieldWidth + x]),
                -1f,
                1f);
        }

        private void WriteObstacleRoutingCell(
            int x,
            int y,
            float direction,
            float influence)
        {
            int baseIndex = (y * fieldWidth + x) * 2;
            float existingInfluence = Mathf.HalfToFloat(
                obstacleRoutingHalfData[baseIndex + 1]);
            if (influence <= existingInfluence)
            {
                return;
            }

            obstacleRoutingHalfData[baseIndex] = Mathf.FloatToHalf(
                Mathf.Clamp(direction, -1f, 1f));
            obstacleRoutingHalfData[baseIndex + 1] = Mathf.FloatToHalf(
                Mathf.Clamp01(influence));
        }

        private static float ResolveNeutralThreshold(
            float[] values,
            float neutralCoverage)
        {
            if (values == null || values.Length == 0 || neutralCoverage <= 0f)
            {
                return 0f;
            }

            float[] magnitudes = new float[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                magnitudes[index] = Mathf.Abs(values[index]);
            }

            Array.Sort(magnitudes);
            int thresholdIndex = Mathf.Clamp(
                Mathf.RoundToInt((magnitudes.Length - 1) * neutralCoverage),
                0,
                magnitudes.Length - 1);
            return Mathf.Clamp01(magnitudes[thresholdIndex]);
        }

        private static float MotionFractalLaneNoise(
            float u,
            float v,
            float aspect,
            float directionChangeFrequency,
            float acrossRiverCoherence,
            float seed)
        {
            float aspectSafe = Mathf.Max(1f, aspect);
            float downstreamScale = Mathf.Clamp(
                directionChangeFrequency,
                0.25f,
                4f);
            float coherence = Mathf.Clamp(
                acrossRiverCoherence,
                0.5f,
                4f);
            float lateralFrequencyScale = 1f / coherence;
            float acrossWarpFrequency = Mathf.Max(
                0.25f,
                1.85f * lateralFrequencyScale);
            int warpPeriodX = Mathf.Max(
                6,
                Mathf.RoundToInt(
                    10.0f * downstreamScale * aspectSafe));
            float warpX = MotionValueNoiseTiledX(
                u * warpPeriodX + seed * 1.37f,
                v * acrossWarpFrequency - seed * 0.61f,
                warpPeriodX) * 2f - 1f;
            float warpY = MotionValueNoiseTiledX(
                u * warpPeriodX - seed * 0.83f + 19.31f,
                v * acrossWarpFrequency + seed * 1.11f - 7.17f,
                warpPeriodX) * 2f - 1f;

            float warpedU = u + warpX * 0.075f;
            float warpedV = v + warpY * 0.045f;
            float sum = 0f;
            float weightSum = 0f;

            // Patch 4.11C.5.16A.1: downstream sign-change frequency and
            // across-river coherence are independent authoring dimensions.
            // Higher downstreamScale creates more frequent irregular left/right
            // changes along X. Higher coherence lowers Y frequency so neighbouring
            // rows remain grouped. The existing two-pass Y smoothing remains the
            // final anti-checkerboard coherence guarantee.
            sum += MotionLaneAnisotropicSignedNoise(
                warpedU,
                warpedV,
                8.50f * downstreamScale * aspectSafe,
                1.15f * lateralFrequencyScale,
                seed + 3.17f) * 0.22f;
            weightSum += 0.22f;
            sum += MotionLaneAnisotropicSignedNoise(
                warpedU + warpedV * 0.045f,
                warpedV - warpedU * 0.006f,
                15.50f * downstreamScale * aspectSafe,
                1.65f * lateralFrequencyScale,
                seed - 11.73f) * 0.24f;
            weightSum += 0.24f;
            sum += MotionLaneAnisotropicSignedNoise(
                warpedU - warpedV * 0.060f,
                warpedV + warpedU * 0.010f,
                25.00f * downstreamScale * aspectSafe,
                2.25f * lateralFrequencyScale,
                seed + 29.41f) * 0.22f;
            weightSum += 0.22f;
            sum += MotionLaneAnisotropicSignedNoise(
                warpedU + warpedV * 0.090f,
                warpedV - warpedU * 0.014f,
                40.00f * downstreamScale * aspectSafe,
                3.15f * lateralFrequencyScale,
                seed - 43.09f) * 0.17f;
            weightSum += 0.17f;
            sum += MotionLaneAnisotropicSignedNoise(
                warpedU - warpedV * 0.130f,
                warpedV + warpedU * 0.020f,
                64.00f * downstreamScale * aspectSafe,
                4.30f * lateralFrequencyScale,
                seed + 61.83f) * 0.10f;
            weightSum += 0.10f;

            float raw = weightSum > 0.0001f
                ? sum / weightSum
                : 0f;

            float breaker = MotionLaneAnisotropicSignedNoise(
                warpedU + warpedV * 0.050f,
                warpedV - warpedU * 0.010f,
                34.00f * downstreamScale * aspectSafe,
                1.80f * lateralFrequencyScale,
                seed + 101.9f) * 0.18f;
            breaker += MotionLaneAnisotropicSignedNoise(
                warpedU - warpedV * 0.070f,
                warpedV + warpedU * 0.016f,
                58.00f * downstreamScale * aspectSafe,
                2.55f * lateralFrequencyScale,
                seed - 137.6f) * 0.12f;

            float crossCut = MotionLaneAnisotropicSignedNoise(
                warpedU + warpedV * 0.035f,
                warpedV - warpedU * 0.008f,
                72.00f * downstreamScale * aspectSafe,
                1.60f * lateralFrequencyScale,
                seed + 211.4f) * 0.12f;
            crossCut += MotionLaneAnisotropicSignedNoise(
                warpedU - warpedV * 0.050f,
                warpedV + warpedU * 0.012f,
                105.00f * downstreamScale * aspectSafe,
                2.10f * lateralFrequencyScale,
                seed - 257.8f) * 0.08f;

            raw = Mathf.Clamp(
                raw * 0.92f +
                breaker * (1.00f - 0.30f * Mathf.Abs(raw)) +
                crossCut * (0.88f - 0.24f * Mathf.Abs(raw)),
                -1f,
                1f);

            raw = Mathf.Clamp(raw * 1.55f, -1f, 1f);
            float magnitude = Mathf.Pow(Mathf.Abs(raw), 0.78f);
            return Mathf.Sign(raw) * magnitude;
        }

        private static float MotionLaneAnisotropicSignedNoise(
            float u,
            float v,
            float downstreamFrequency,
            float lateralFrequency,
            float seed)
        {
            float safeDownstreamFrequency = Mathf.Max(1f, Mathf.Round(downstreamFrequency));
            float safeLateralFrequency = Mathf.Max(0.25f, lateralFrequency);
            int periodX = Mathf.Max(1, Mathf.RoundToInt(safeDownstreamFrequency));

            return MotionValueNoiseTiledX(
                u * safeDownstreamFrequency + seed * 1.713f,
                v * safeLateralFrequency - seed * 0.947f,
                periodX) * 2f - 1f;
        }

        private static float MotionValueNoiseTiledX(
            float x,
            float y,
            int periodX)
        {
            float cellXFloat = Mathf.Floor(x);
            float cellY = Mathf.Floor(y);
            int cellX = Mathf.FloorToInt(cellXFloat);
            int wrappedCellX = PositiveModulo(cellX, Mathf.Max(1, periodX));
            int wrappedNextX = PositiveModulo(cellX + 1, Mathf.Max(1, periodX));
            float fractionX = SmoothNoiseFraction(x - cellXFloat);
            float fractionY = SmoothNoiseFraction(y - cellY);
            float a = MotionHash01(wrappedCellX, cellY);
            float b = MotionHash01(wrappedNextX, cellY);
            float c = MotionHash01(wrappedCellX, cellY + 1f);
            float d = MotionHash01(wrappedNextX, cellY + 1f);
            return Mathf.Lerp(
                Mathf.Lerp(a, b, fractionX),
                Mathf.Lerp(c, d, fractionX),
                fractionY);
        }

        private static int PositiveModulo(int value, int modulus)
        {
            if (modulus <= 0)
            {
                return 0;
            }

            int result = value % modulus;
            return result < 0 ? result + modulus : result;
        }

        private static float MotionHash01(float x, float y)
        {
            float px = Repeat01(x * 123.34f);
            float py = Repeat01(y * 456.21f);
            float offset = px * (px + 45.32f) + py * (py + 45.32f);
            px += offset;
            py += offset;
            return Repeat01(px * py);
        }

        private static float SmoothNoiseFraction(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static float Repeat01(float value)
        {
            return value - Mathf.Floor(value);
        }

        private static float RepeatSigned(float value, float length)
        {
            if (length <= 0.0001f)
            {
                return 0f;
            }

            value %= length;
            if (value < 0f)
            {
                value += length;
            }

            return value;
        }

        private static int AccumulateHash(int hash, int value)
        {
            unchecked
            {
                return hash * 397 ^ value;
            }
        }

        private void RebuildObstacleExclusionCache(
            bool captureScalarForTopology = true)
        {
            using var profilerScope = InitBuildObstacleExclusionProfilerMarker.Auto();
            obstacleExclusionUsesCachedScalar = false;
            activeTopologyObstacleStale = !captureScalarForTopology;
            if (captureScalarForTopology)
            {
                topologyCacheLoadedForActiveResources = false;
            }

            ReleaseObstacleExclusionBuffers();
            obstacleExclusionMeshFilters.Clear();
            obstacleExclusionCells.Clear();
            obstacleExclusionSamples.Clear();

            if (captureScalarForTopology)
            {
                int cellCount = Mathf.Max(0, fieldWidth * fieldHeight);
                if (obstacleExclusionScalar.Length != cellCount)
                {
                    obstacleExclusionScalar = new float[cellCount];
                }
                else
                {
                    Array.Clear(
                        obstacleExclusionScalar,
                        0,
                        obstacleExclusionScalar.Length);
                }
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            obstacleGeometryVersion = disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;

            if (disturbanceRuntime == null ||
                fieldWidth < 1 || fieldHeight < 1 ||
                fieldLength <= 0.0001f ||
                river == null || !river.Domain.IsValid)
            {
                ClearObstacleExclusionMask();
                return;
            }

            disturbanceRuntime.CopyObstacleExclusionMeshFiltersTo(
                obstacleExclusionMeshFilters);
            if (obstacleExclusionMeshFilters.Count == 0)
            {
                ClearObstacleExclusionMask();
                return;
            }

            // TODO(PROCEDURAL-CHUNK-BUILD): this exact triangle preparation is
            // the temporary development fallback. Move it into the future map
            // chunk generation/building/linking phase after procedural objects
            // have their final transforms, then load the cached compact cells
            // and intervals here instead of rescanning meshes during startup.
            for (int index = 0;
                 index < obstacleExclusionMeshFilters.Count;
                 index++)
            {
                RiverObstacleExclusionResolver.TryBake(
                    river,
                    obstacleExclusionMeshFilters[index],
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    obstacleExclusionCells,
                    obstacleExclusionSamples,
                    out _);
            }

            if (obstacleExclusionCells.Count == 0 ||
                obstacleExclusionSamples.Count == 0)
            {
                ClearObstacleExclusionMask();
                return;
            }

            if (obstacleExclusionGpuCells.Length !=
                obstacleExclusionCells.Count)
            {
                obstacleExclusionGpuCells =
                    new FoamObstacleIntervalCellData[
                        obstacleExclusionCells.Count];
            }

            for (int index = 0;
                 index < obstacleExclusionCells.Count;
                 index++)
            {
                RiverObstacleExclusionCell cell =
                    obstacleExclusionCells[index];
                obstacleExclusionGpuCells[index] =
                    new FoamObstacleIntervalCellData
                    {
                        CoordinateAndOffset = new Vector4(
                            cell.Coordinate.x,
                            cell.Coordinate.y,
                            cell.IntervalOffset,
                            0f)
                    };
            }

            obstacleExclusionCellBuffer = new ComputeBuffer(
                obstacleExclusionGpuCells.Length,
                sizeof(float) * 4,
                ComputeBufferType.Structured);
            obstacleExclusionCellBuffer.SetData(obstacleExclusionGpuCells);

            obstacleExclusionSampleBuffer = new ComputeBuffer(
                obstacleExclusionSamples.Count,
                sizeof(float) * 8,
                ComputeBufferType.Structured);
            obstacleExclusionSampleBuffer.SetData(obstacleExclusionSamples);

            ConfigureTopologyParameters(0f);
            UpdateObstacleExclusionMask();
            if (captureScalarForTopology)
            {
                ReadBackObstacleExclusionScalar();
            }
        }

        private void ReadBackObstacleExclusionScalar()
        {
            if (obstacleExclusionTexture == null ||
                obstacleExclusionScalar.Length != fieldWidth * fieldHeight)
            {
                return;
            }

            if (SystemInfo.supportsAsyncGPUReadback)
            {
                AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(
                    obstacleExclusionTexture,
                    0,
                    TextureFormat.RFloat,
                    null);
                request.WaitForCompletion();
                if (!request.hasError)
                {
                    var data = request.GetData<float>();
                    int count = Mathf.Min(
                        data.Length,
                        obstacleExclusionScalar.Length);
                    for (int index = 0; index < count; index++)
                    {
                        obstacleExclusionScalar[index] =
                            Mathf.Clamp01(data[index]);
                    }

                    return;
                }
            }

            if (obstacleExclusionReadbackTexture == null ||
                obstacleExclusionReadbackTexture.width != fieldWidth ||
                obstacleExclusionReadbackTexture.height != fieldHeight)
            {
                if (obstacleExclusionReadbackTexture != null)
                {
                    DestroyUnityObject(obstacleExclusionReadbackTexture);
                }

                obstacleExclusionReadbackTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RFloat,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamObstacleFootprint_Readback",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = obstacleExclusionTexture;
            obstacleExclusionReadbackTexture.ReadPixels(
                new Rect(0f, 0f, fieldWidth, fieldHeight),
                0,
                0,
                false);
            obstacleExclusionReadbackTexture.Apply(false, false);
            RenderTexture.active = previous;

            var fallbackData =
                obstacleExclusionReadbackTexture.GetPixelData<float>(0);
            int fallbackCount = Mathf.Min(
                fallbackData.Length,
                obstacleExclusionScalar.Length);
            for (int index = 0; index < fallbackCount; index++)
            {
                obstacleExclusionScalar[index] =
                    Mathf.Clamp01(fallbackData[index]);
            }
        }

        private void ReleaseObstacleExclusionBuffers()
        {
            obstacleExclusionCellBuffer?.Release();
            obstacleExclusionCellBuffer = null;
            obstacleExclusionSampleBuffer?.Release();
            obstacleExclusionSampleBuffer = null;

            if (obstacleExclusionReadbackTexture != null)
            {
                DestroyUnityObject(obstacleExclusionReadbackTexture);
                obstacleExclusionReadbackTexture = null;
            }

            if (obstacleExclusionUploadTexture != null)
            {
                DestroyUnityObject(obstacleExclusionUploadTexture);
                obstacleExclusionUploadTexture = null;
            }
        }

        private void ClearObstacleExclusionMask()
        {
            if (computeShader == null ||
                obstacleExclusionTexture == null ||
                clearObstacleExclusionKernel < 0)
            {
                return;
            }

            computeShader.SetInts(
                "_FoamDimensions",
                fieldWidth,
                fieldHeight);
            computeShader.SetTexture(
                clearObstacleExclusionKernel,
                "_FoamObstacleExclusionWrite",
                obstacleExclusionTexture);
            Dispatch(
                clearObstacleExclusionKernel,
                fieldWidth,
                fieldHeight);
        }

        private void UpdateObstacleExclusionMask()
        {
            if (obstacleExclusionUsesCachedScalar)
            {
                return;
            }

            ClearObstacleExclusionMask();
            if (computeShader == null ||
                updateObstacleExclusionKernel < 0 ||
                obstacleExclusionTexture == null ||
                obstacleExclusionCellBuffer == null ||
                obstacleExclusionSampleBuffer == null ||
                obstacleExclusionCells.Count == 0)
            {
                return;
            }

            computeShader.SetInt(
                "_FoamObstacleCellCount",
                obstacleExclusionCells.Count);
            computeShader.SetBuffer(
                updateObstacleExclusionKernel,
                "_FoamObstacleCells",
                obstacleExclusionCellBuffer);
            computeShader.SetBuffer(
                updateObstacleExclusionKernel,
                "_FoamObstacleSamples",
                obstacleExclusionSampleBuffer);
            computeShader.SetTexture(
                updateObstacleExclusionKernel,
                "_FoamObstacleExclusionWrite",
                obstacleExclusionTexture);
            DispatchOneDimensional(
                updateObstacleExclusionKernel,
                obstacleExclusionCells.Count,
                64);
        }
    }
}
