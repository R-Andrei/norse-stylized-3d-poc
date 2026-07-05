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

            float scrollCells = river.FoamMotionFieldScrollHz *
                Mathf.Max(0f, deltaTime) * fieldWidth * river.FlowDirection;
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
            hash = AccumulateHash(hash, river != null ? river.VisualSeed : 0);
            hash = AccumulateHash(
                hash,
                Mathf.RoundToInt((river != null
                    ? river.FoamMotionFieldNeutralCoverage
                    : 0.10f) * 10000f));
            hash = AccumulateHash(
                hash,
                Mathf.RoundToInt((river != null
                    ? river.FoamMotionFieldLaneScale
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

            float laneScale = river != null
                ? river.FoamMotionFieldLaneScale
                : 1f;
            float seed = river != null
                ? river.VisualSeed * 0.01371f
                : 23.17f;
            float frequency = Mathf.Clamp(laneScale, 0.25f, 4f);
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
                        frequency,
                        seed);
                    motionLaneRawValues[y * fieldWidth + x] = Mathf.Clamp(raw, -1f, 1f);
                }
            }

            float neutralCoverage = river != null
                ? river.FoamMotionFieldNeutralCoverage
                : 0.10f;
            float neutralThreshold = ResolveNeutralThreshold(
                motionLaneRawValues,
                Mathf.Clamp01(neutralCoverage));
            float denominator = Mathf.Max(0.0001f, 1f - neutralThreshold);
            for (int index = 0; index < cellCount; index++)
            {
                float raw = motionLaneRawValues[index];
                float magnitude = Mathf.Abs(raw);
                float resolved = 0f;
                if (magnitude > neutralThreshold)
                {
                    float normalized = Mathf.Clamp01(
                        (magnitude - neutralThreshold) / denominator);
                    normalized = normalized * normalized * (3f - 2f * normalized);
                    resolved = Mathf.Sign(raw) * normalized;
                }

                motionLaneHalfData[index] = Mathf.FloatToHalf(resolved);
            }

            motionLaneTexture.SetPixelData(motionLaneHalfData, 0);
            motionLaneTexture.Apply(false, false);
            motionLaneFieldSignature = signature;
            lastMotionLaneSignature = signature;
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

                    FoamObstacleRoutingComponent component = FloodObstacleRoutingComponent(x, y);
                    if (component.Count > 0)
                    {
                        obstacleRoutingComponents.Add(component);
                    }
                }
            }
        }

        private FoamObstacleRoutingComponent FloodObstacleRoutingComponent(
            int startX,
            int startY)
        {
            int read = 0;
            int write = 0;
            int startIndex = startY * fieldWidth + startX;
            obstacleRoutingQueue[write++] = startIndex;
            obstacleRoutingVisited[startIndex] = true;

            FoamObstacleRoutingComponent component = new FoamObstacleRoutingComponent
            {
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
            int approachCells = Mathf.Max(
                6,
                Mathf.RoundToInt(fieldWidth * 0.065f));
            int frontCells = Mathf.Max(
                2,
                Mathf.RoundToInt(componentWidth * 0.45f));
            int releaseCells = Mathf.Max(
                1,
                Mathf.RoundToInt(fieldWidth * 0.006f));
            int sidePaddingCells = Mathf.Max(
                3,
                Mathf.RoundToInt(componentHeight * 0.70f));
            float tieSide = ResolveComponentTieSide(
                component,
                flowSign,
                approachCells);
            int startS = minS - approachCells;
            int endS = maxS + releaseCells;
            int startY = Mathf.Max(0, component.MinY - sidePaddingCells);
            int endY = Mathf.Min(fieldHeight - 1, component.MaxY + sidePaddingCells);

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
                        sidePaddingCells);
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
            int sidePaddingCells)
        {
            float centerY = (component.MinY + component.MaxY) * 0.5f;
            float halfHeight = Mathf.Max(
                0.5f,
                (component.MaxY - component.MinY + 1) * 0.5f);
            float signedLateral = y - centerY;
            float absLateral = Mathf.Abs(signedLateral);
            float safetyMargin = Mathf.Max(1f, halfHeight * 0.18f);
            float frontCoreHalfWidth = halfHeight + safetyMargin;
            float sideOuterHalfWidth = frontCoreHalfWidth + Mathf.Max(1f, sidePaddingCells);

            if (s < minS)
            {
                float approachT = Mathf.Clamp01(
                    1f - ((float)(minS - s) / Mathf.Max(1, approachCells)));
                approachT = Smooth01(approachT);
                float approachCap = Mathf.Lerp(0.18f, 0.60f, approachT);
                float imminentT = Mathf.Clamp01((approachT - 0.72f) / 0.28f);
                imminentT = Smooth01(imminentT);
                float maxInfluence = Mathf.Lerp(approachCap, 1f, imminentT);
                float envelopeHalfWidth = Mathf.Lerp(
                    Mathf.Max(1f, halfHeight * 0.45f),
                    sideOuterHalfWidth,
                    approachT);
                float corridor = RoundedCorridorFactor(
                    absLateral,
                    frontCoreHalfWidth,
                    envelopeHalfWidth);

                return Mathf.Clamp01(maxInfluence * corridor);
            }

            if (s <= maxS)
            {
                float frontT = Mathf.Clamp01(
                    1f - ((float)(s - minS) / Mathf.Max(1, frontCells)));
                frontT = Smooth01(frontT);
                float directCollision = RoundedCorridorFactor(
                    absLateral,
                    frontCoreHalfWidth,
                    frontCoreHalfWidth + Mathf.Max(1f, sidePaddingCells * 0.45f));
                float sideSkirt = RoundedCorridorFactor(
                    absLateral,
                    halfHeight + safetyMargin,
                    sideOuterHalfWidth);
                float maxInfluence = Mathf.Lerp(0.12f, 1f, frontT);
                float influence = Mathf.Max(
                    directCollision * maxInfluence,
                    sideSkirt * 0.14f * frontT);

                return Mathf.Clamp01(influence);
            }

            float releaseT = Mathf.Clamp01(
                1f - ((float)(s - maxS) / Mathf.Max(1, releaseCells)));
            releaseT = Smooth01(releaseT);
            float releaseCorridor = RoundedCorridorFactor(
                absLateral,
                halfHeight + safetyMargin,
                sideOuterHalfWidth);
            return Mathf.Clamp01(releaseT * releaseCorridor * 0.12f);
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
            float frequency,
            float seed)
        {
            float warpBaseY = Mathf.Max(2.0f, 2.85f * frequency);
            int warpPeriodX = Mathf.Max(
                4,
                Mathf.RoundToInt(warpBaseY * aspect));
            float warpX = MotionValueNoiseTiledX(
                u * warpPeriodX + seed * 1.37f,
                v * warpBaseY - seed * 0.61f,
                warpPeriodX) * 2f - 1f;
            float warpY = MotionValueNoiseTiledX(
                u * warpPeriodX - seed * 0.83f + 19.31f,
                v * warpBaseY + seed * 1.11f - 7.17f,
                warpPeriodX) * 2f - 1f;

            float warpedU = u + warpX * 0.035f;
            float warpedV = v + warpY * 0.115f;
            float sum = 0f;
            float weightSum = 0f;
            sum += MotionLaneOctave(
                warpedU,
                warpedV,
                aspect,
                4.20f * frequency,
                0.24f,
                seed + 3.17f,
                ref weightSum);
            sum += MotionLaneOctave(
                warpedU,
                warpedV,
                aspect,
                7.80f * frequency,
                0.25f,
                seed - 11.73f,
                ref weightSum);
            sum += MotionLaneOctave(
                warpedU,
                warpedV,
                aspect,
                13.60f * frequency,
                0.22f,
                seed + 29.41f,
                ref weightSum);
            sum += MotionLaneOctave(
                warpedU,
                warpedV,
                aspect,
                23.50f * frequency,
                0.17f,
                seed - 43.09f,
                ref weightSum);
            sum += MotionLaneOctave(
                warpedU,
                warpedV,
                aspect,
                39.00f * frequency,
                0.12f,
                seed + 61.83f,
                ref weightSum);

            float raw = weightSum > 0.0001f
                ? sum / weightSum
                : 0f;
            raw = Mathf.Clamp(raw * 1.42f, -1f, 1f);
            float magnitude = Mathf.Pow(Mathf.Abs(raw), 0.82f);
            return Mathf.Sign(raw) * magnitude;
        }

        private static float MotionLaneOctave(
            float u,
            float v,
            float aspect,
            float verticalFrequency,
            float weight,
            float seed,
            ref float weightSum)
        {
            float safeVerticalFrequency = Mathf.Max(1f, verticalFrequency);
            int periodX = Mathf.Max(
                4,
                Mathf.RoundToInt(safeVerticalFrequency * Mathf.Max(1f, aspect)));
            float sample = MotionValueNoiseTiledX(
                u * periodX + seed * 0.37f,
                v * safeVerticalFrequency - seed * 0.19f,
                periodX) * 2f - 1f;
            weightSum += weight;
            return sample * weight;
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
