using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Deterministic CPU proof generator for one local Major Support candidate.
    /// It performs no river placement and owns no runtime object or resource.
    /// </summary>
    public static class StylizedRiverFoamMajorCandidateGenerator
    {
        public const int DefaultResolution = 96;
        public const int DefaultEmptyMargin = 7;
        public const int DefaultMaximumAttempts = 6;

        private const float DiagonalDistance = 1.41421356f;
        private static readonly Vector2Int[] CardinalNeighbours =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        public static StylizedRiverFoamMajorCandidate Generate(int seed)
        {
            uint identity = MixBits((uint)Mathf.Max(0, seed));
            float targetOccupancy = Mathf.Lerp(
                0.30f,
                0.46f,
                Hash01(identity, 1u));
            float warpStrength = Mathf.Lerp(
                0.12f,
                0.29f,
                Hash01(identity, 2u));

            StylizedRiverFoamMajorCandidateSettings settings =
                new StylizedRiverFoamMajorCandidateSettings(
                    DefaultResolution,
                    DefaultEmptyMargin,
                    DefaultMaximumAttempts,
                    targetOccupancy,
                    warpStrength,
                    0.16f,
                    0.58f,
                    5,
                    0.18f,
                    0.82f,
                    2.5f);
            return Generate(seed, settings);
        }

        public static StylizedRiverFoamMajorCandidate Generate(
            int seed,
            StylizedRiverFoamMajorCandidateSettings settings)
        {
            StylizedRiverFoamMajorCandidate lastCandidate = null;
            int maximumAttempts = Mathf.Max(1, settings.MaximumAttempts);

            for (int attempt = 0; attempt < maximumAttempts; attempt++)
            {
                StylizedRiverFoamMajorCandidate candidate = GenerateAttempt(
                    Mathf.Max(0, seed),
                    attempt,
                    settings);
                if (candidate.Accepted)
                {
                    return candidate;
                }

                lastCandidate = candidate;
            }

            return lastCandidate;
        }

        private static StylizedRiverFoamMajorCandidate GenerateAttempt(
            int seed,
            int attemptIndex,
            StylizedRiverFoamMajorCandidateSettings settings)
        {
            int resolution = settings.Resolution;
            int cellCount = resolution * resolution;
            uint attemptSeed = MixBits(
                (uint)seed ^
                ((uint)(attemptIndex + 1) * 0x9E3779B9u));

            float[] rawField = new float[cellCount];
            float[] unnormalisedField = new float[cellCount];
            byte[] thresholded = new byte[cellCount];
            byte[] cleaned = new byte[cellCount];
            float[] support = new float[cellCount];

            GenerateRawField(
                attemptSeed,
                settings,
                rawField,
                unnormalisedField);
            ThresholdByPercentile(
                settings,
                unnormalisedField,
                thresholded);

            KeepLargestConnectedComponent(
                thresholded,
                cleaned,
                resolution);
            FillEnclosedHoles(cleaned, resolution);
            CloseTinyGaps(cleaned, resolution, settings.EmptyMargin);
            RemoveShortContourSpikes(
                cleaned,
                resolution,
                settings.EmptyMargin);
            KeepLargestConnectedComponentInPlace(cleaned, resolution);
            FillEnclosedHoles(cleaned, resolution);
            ClearMandatoryMargin(
                cleaned,
                resolution,
                settings.EmptyMargin);

            CandidateMetrics metrics = MeasureCandidate(
                cleaned,
                resolution,
                settings);
            StylizedRiverFoamMajorCandidateRejectionReason rejection =
                ResolveRejection(metrics, settings);
            bool accepted = rejection ==
                StylizedRiverFoamMajorCandidateRejectionReason.None;

            BuildSoftSupport(
                cleaned,
                resolution,
                settings.SupportFeatherCells,
                support);

            return new StylizedRiverFoamMajorCandidate(
                seed,
                attemptIndex,
                resolution,
                settings.EmptyMargin,
                accepted,
                rejection,
                metrics.OccupiedCells,
                metrics.AreaFraction,
                metrics.MinimumNeckWidthCells,
                metrics.Compactness,
                rawField,
                thresholded,
                cleaned,
                support);
        }

        private static void GenerateRawField(
            uint seed,
            StylizedRiverFoamMajorCandidateSettings settings,
            float[] normalizedOutput,
            float[] unnormalisedOutput)
        {
            int resolution = settings.Resolution;
            int margin = settings.EmptyMargin;
            int interiorResolution = resolution - margin * 2;

            float angle = Hash01(seed, 3u) * Mathf.PI * 2f;
            float cosine = Mathf.Cos(angle);
            float sine = Mathf.Sin(angle);
            float aspect = Mathf.Pow(
                2f,
                Mathf.Lerp(-0.45f, 0.45f, Hash01(seed, 4u)));
            float shear = Mathf.Lerp(-0.32f, 0.32f, Hash01(seed, 5u));

            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int index = 0; index < unnormalisedOutput.Length; index++)
            {
                unnormalisedOutput[index] = float.NegativeInfinity;
            }

            for (int y = margin; y < resolution - margin; y++)
            {
                float v = (y - margin + 0.5f) / interiorResolution;
                for (int x = margin; x < resolution - margin; x++)
                {
                    float u = (x - margin + 0.5f) / interiorResolution;
                    float px = (u - 0.5f) * 2f;
                    float py = (v - 0.5f) * 2f;

                    float transformedX = cosine * px - sine * py;
                    float transformedY = sine * px + cosine * py;
                    transformedX *= aspect;
                    transformedY /= aspect;
                    transformedX += shear * transformedY;

                    float warpX = ValueNoise(
                        transformedX * 1.1f + 11.2f,
                        transformedY * 1.1f - 7.1f,
                        seed,
                        11u) * 2f - 1f;
                    float warpY = ValueNoise(
                        transformedX * 1.1f - 5.4f,
                        transformedY * 1.1f + 13.7f,
                        seed,
                        12u) * 2f - 1f;
                    float sampleX = transformedX +
                        warpX * settings.WarpStrength;
                    float sampleY = transformedY +
                        warpY * settings.WarpStrength;

                    float broad = ValueNoise(
                        sampleX * 1.55f + 3.1f,
                        sampleY * 1.55f - 2.6f,
                        seed,
                        21u);
                    float broadSecondary = ValueNoise(
                        sampleX * 2.3f - 9.4f,
                        sampleY * 1.8f + 5.7f,
                        seed,
                        22u);
                    float medium = ValueNoise(
                        sampleX * 5.2f + 17.3f,
                        sampleY * 5.2f - 11.8f,
                        seed,
                        23u);
                    float detail = ValueNoise(
                        sampleX * 8.5f - 4.8f,
                        sampleY * 8.5f + 19.2f,
                        seed,
                        24u);
                    float directionalVariation = ValueNoise(
                        sampleX * 0.85f + 30f,
                        sampleY * 0.85f - 30f,
                        seed,
                        25u) - 0.5f;

                    float edgeDistance = Mathf.Min(
                        Mathf.Min(u, 1f - u),
                        Mathf.Min(v, 1f - v));
                    float interiorEnvelope = SmoothStep(
                        0f,
                        0.24f,
                        edgeDistance);

                    float value =
                        broad * 0.48f +
                        broadSecondary * 0.22f +
                        medium * 0.22f +
                        detail * 0.08f +
                        interiorEnvelope * 0.30f +
                        directionalVariation * 0.08f;
                    int index = x + y * resolution;
                    unnormalisedOutput[index] = value;
                    minimum = Mathf.Min(minimum, value);
                    maximum = Mathf.Max(maximum, value);
                }
            }

            float range = Mathf.Max(0.000001f, maximum - minimum);
            for (int index = 0; index < unnormalisedOutput.Length; index++)
            {
                float value = unnormalisedOutput[index];
                normalizedOutput[index] = float.IsNegativeInfinity(value)
                    ? 0f
                    : Mathf.Clamp01((value - minimum) / range);
            }
        }

        private static void ThresholdByPercentile(
            StylizedRiverFoamMajorCandidateSettings settings,
            float[] field,
            byte[] output)
        {
            int resolution = settings.Resolution;
            int margin = settings.EmptyMargin;
            int interiorCellCount =
                (resolution - margin * 2) * (resolution - margin * 2);
            float[] sortedValues = new float[interiorCellCount];
            int writeIndex = 0;

            for (int y = margin; y < resolution - margin; y++)
            {
                for (int x = margin; x < resolution - margin; x++)
                {
                    sortedValues[writeIndex++] = field[x + y * resolution];
                }
            }

            Array.Sort(sortedValues);
            int thresholdIndex = Mathf.Clamp(
                Mathf.FloorToInt(
                    (1f - settings.TargetOccupancy) *
                    (sortedValues.Length - 1)),
                0,
                sortedValues.Length - 1);
            float threshold = sortedValues[thresholdIndex];

            Array.Clear(output, 0, output.Length);
            for (int y = margin; y < resolution - margin; y++)
            {
                for (int x = margin; x < resolution - margin; x++)
                {
                    int index = x + y * resolution;
                    output[index] = field[index] >= threshold
                        ? (byte)1
                        : (byte)0;
                }
            }
        }

        private static void KeepLargestConnectedComponent(
            byte[] source,
            byte[] destination,
            int resolution)
        {
            Array.Clear(destination, 0, destination.Length);
            bool[] visited = new bool[source.Length];
            List<int> queue = new List<int>(source.Length / 4);
            List<int> largest = new List<int>(source.Length / 4);

            for (int index = 0; index < source.Length; index++)
            {
                if (source[index] == 0 || visited[index])
                {
                    continue;
                }

                queue.Clear();
                queue.Add(index);
                visited[index] = true;
                int readIndex = 0;
                while (readIndex < queue.Count)
                {
                    int current = queue[readIndex++];
                    int x = current % resolution;
                    int y = current / resolution;
                    for (int neighbourIndex = 0;
                         neighbourIndex < CardinalNeighbours.Length;
                         neighbourIndex++)
                    {
                        Vector2Int offset = CardinalNeighbours[neighbourIndex];
                        int neighbourX = x + offset.x;
                        int neighbourY = y + offset.y;
                        if (neighbourX < 0 || neighbourX >= resolution ||
                            neighbourY < 0 || neighbourY >= resolution)
                        {
                            continue;
                        }

                        int neighbour = neighbourX + neighbourY * resolution;
                        if (source[neighbour] == 0 || visited[neighbour])
                        {
                            continue;
                        }

                        visited[neighbour] = true;
                        queue.Add(neighbour);
                    }
                }

                if (queue.Count > largest.Count)
                {
                    largest.Clear();
                    largest.AddRange(queue);
                }
            }

            for (int index = 0; index < largest.Count; index++)
            {
                destination[largest[index]] = 1;
            }
        }

        private static void KeepLargestConnectedComponentInPlace(
            byte[] mask,
            int resolution)
        {
            byte[] retained = new byte[mask.Length];
            KeepLargestConnectedComponent(mask, retained, resolution);
            Buffer.BlockCopy(retained, 0, mask, 0, mask.Length);
        }

        private static void FillEnclosedHoles(byte[] mask, int resolution)
        {
            bool[] outside = new bool[mask.Length];
            Queue<int> queue = new Queue<int>();

            for (int x = 0; x < resolution; x++)
            {
                EnqueueOutside(x, 0, resolution, mask, outside, queue);
                EnqueueOutside(
                    x,
                    resolution - 1,
                    resolution,
                    mask,
                    outside,
                    queue);
            }

            for (int y = 0; y < resolution; y++)
            {
                EnqueueOutside(0, y, resolution, mask, outside, queue);
                EnqueueOutside(
                    resolution - 1,
                    y,
                    resolution,
                    mask,
                    outside,
                    queue);
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                int x = current % resolution;
                int y = current / resolution;
                for (int neighbourIndex = 0;
                     neighbourIndex < CardinalNeighbours.Length;
                     neighbourIndex++)
                {
                    Vector2Int offset = CardinalNeighbours[neighbourIndex];
                    EnqueueOutside(
                        x + offset.x,
                        y + offset.y,
                        resolution,
                        mask,
                        outside,
                        queue);
                }
            }

            for (int index = 0; index < mask.Length; index++)
            {
                if (mask[index] == 0 && !outside[index])
                {
                    mask[index] = 1;
                }
            }
        }

        private static void EnqueueOutside(
            int x,
            int y,
            int resolution,
            byte[] mask,
            bool[] outside,
            Queue<int> queue)
        {
            if (x < 0 || x >= resolution || y < 0 || y >= resolution)
            {
                return;
            }

            int index = x + y * resolution;
            if (mask[index] != 0 || outside[index])
            {
                return;
            }

            outside[index] = true;
            queue.Enqueue(index);
        }

        private static void CloseTinyGaps(
            byte[] mask,
            int resolution,
            int emptyMargin)
        {
            byte[] dilated = new byte[mask.Length];
            byte[] closed = new byte[mask.Length];
            Dilate(mask, dilated, resolution);
            Erode(dilated, closed, resolution);
            Buffer.BlockCopy(closed, 0, mask, 0, mask.Length);
            ClearMandatoryMargin(mask, resolution, emptyMargin);
        }

        private static void RemoveShortContourSpikes(
            byte[] mask,
            int resolution,
            int emptyMargin)
        {
            byte[] next = new byte[mask.Length];
            for (int pass = 0; pass < 2; pass++)
            {
                Buffer.BlockCopy(mask, 0, next, 0, mask.Length);
                for (int y = 1; y < resolution - 1; y++)
                {
                    for (int x = 1; x < resolution - 1; x++)
                    {
                        int index = x + y * resolution;
                        int neighbours = CountEightNeighbours(
                            mask,
                            x,
                            y,
                            resolution);
                        if (mask[index] != 0 && neighbours <= 2)
                        {
                            next[index] = 0;
                        }
                        else if (mask[index] == 0 && neighbours >= 7)
                        {
                            next[index] = 1;
                        }
                    }
                }

                Buffer.BlockCopy(next, 0, mask, 0, mask.Length);
                ClearMandatoryMargin(mask, resolution, emptyMargin);
            }
        }

        private static void Dilate(
            byte[] source,
            byte[] destination,
            int resolution)
        {
            Array.Clear(destination, 0, destination.Length);
            for (int y = 1; y < resolution - 1; y++)
            {
                for (int x = 1; x < resolution - 1; x++)
                {
                    bool occupied = false;
                    for (int offsetY = -1; offsetY <= 1 && !occupied; offsetY++)
                    {
                        for (int offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            if (source[
                                    x + offsetX +
                                    (y + offsetY) * resolution] != 0)
                            {
                                occupied = true;
                                break;
                            }
                        }
                    }

                    destination[x + y * resolution] = occupied
                        ? (byte)1
                        : (byte)0;
                }
            }
        }

        private static void Erode(
            byte[] source,
            byte[] destination,
            int resolution)
        {
            Array.Clear(destination, 0, destination.Length);
            for (int y = 1; y < resolution - 1; y++)
            {
                for (int x = 1; x < resolution - 1; x++)
                {
                    bool occupied = true;
                    for (int offsetY = -1; offsetY <= 1 && occupied; offsetY++)
                    {
                        for (int offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            if (source[
                                    x + offsetX +
                                    (y + offsetY) * resolution] == 0)
                            {
                                occupied = false;
                                break;
                            }
                        }
                    }

                    destination[x + y * resolution] = occupied
                        ? (byte)1
                        : (byte)0;
                }
            }
        }

        private static int CountEightNeighbours(
            byte[] mask,
            int x,
            int y,
            int resolution)
        {
            int count = 0;
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    if (offsetX == 0 && offsetY == 0)
                    {
                        continue;
                    }

                    if (mask[
                            x + offsetX +
                            (y + offsetY) * resolution] != 0)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static void ClearMandatoryMargin(
            byte[] mask,
            int resolution,
            int margin)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    if (x < margin || x >= resolution - margin ||
                        y < margin || y >= resolution - margin)
                    {
                        mask[x + y * resolution] = 0;
                    }
                }
            }
        }

        private static CandidateMetrics MeasureCandidate(
            byte[] mask,
            int resolution,
            StylizedRiverFoamMajorCandidateSettings settings)
        {
            int occupiedCells = 0;
            int perimeterEdges = 0;
            bool touchesStorageBoundary = false;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int index = x + y * resolution;
                    if (mask[index] == 0)
                    {
                        continue;
                    }

                    occupiedCells++;
                    if (x == 0 || x == resolution - 1 ||
                        y == 0 || y == resolution - 1)
                    {
                        touchesStorageBoundary = true;
                    }

                    for (int neighbourIndex = 0;
                         neighbourIndex < CardinalNeighbours.Length;
                         neighbourIndex++)
                    {
                        Vector2Int offset = CardinalNeighbours[neighbourIndex];
                        int neighbourX = x + offset.x;
                        int neighbourY = y + offset.y;
                        if (neighbourX < 0 || neighbourX >= resolution ||
                            neighbourY < 0 || neighbourY >= resolution ||
                            mask[neighbourX + neighbourY * resolution] == 0)
                        {
                            perimeterEdges++;
                        }
                    }
                }
            }

            int interiorResolution = resolution - settings.EmptyMargin * 2;
            int interiorCellCount = interiorResolution * interiorResolution;
            float areaFraction = interiorCellCount > 0
                ? occupiedCells / (float)interiorCellCount
                : 0f;
            float compactness = perimeterEdges > 0
                ? 4f * Mathf.PI * occupiedCells /
                  (perimeterEdges * perimeterEdges)
                : 0f;
            int componentCount = CountComponents(mask, resolution, 1);
            int holeCount = CountEnclosedHoles(mask, resolution);
            int minimumNeckWidth = EstimateMinimumNeckWidth(
                mask,
                resolution,
                10);

            return new CandidateMetrics(
                occupiedCells,
                areaFraction,
                compactness,
                minimumNeckWidth,
                componentCount,
                holeCount,
                touchesStorageBoundary);
        }

        private static StylizedRiverFoamMajorCandidateRejectionReason
            ResolveRejection(
                CandidateMetrics metrics,
                StylizedRiverFoamMajorCandidateSettings settings)
        {
            if (metrics.OccupiedCells <= 0)
            {
                return StylizedRiverFoamMajorCandidateRejectionReason.Empty;
            }

            if (metrics.TouchesStorageBoundary)
            {
                return StylizedRiverFoamMajorCandidateRejectionReason
                    .TouchesStorageBoundary;
            }

            if (metrics.ComponentCount != 1)
            {
                return StylizedRiverFoamMajorCandidateRejectionReason
                    .Disconnected;
            }

            if (metrics.HoleCount > 0)
            {
                return StylizedRiverFoamMajorCandidateRejectionReason
                    .ContainsHole;
            }

            if (metrics.AreaFraction < settings.MinimumAreaFraction)
            {
                return StylizedRiverFoamMajorCandidateRejectionReason
                    .AreaTooSmall;
            }

            if (metrics.AreaFraction > settings.MaximumAreaFraction)
            {
                return StylizedRiverFoamMajorCandidateRejectionReason
                    .AreaTooLarge;
            }

            if (metrics.MinimumNeckWidthCells <
                settings.MinimumNeckWidthCells)
            {
                return StylizedRiverFoamMajorCandidateRejectionReason
                    .NeckTooNarrow;
            }

            if (metrics.Compactness > settings.MaximumCompactness)
            {
                return StylizedRiverFoamMajorCandidateRejectionReason
                    .TooCompact;
            }

            if (metrics.Compactness < settings.MinimumCompactness)
            {
                return StylizedRiverFoamMajorCandidateRejectionReason
                    .TooFragmented;
            }

            return StylizedRiverFoamMajorCandidateRejectionReason.None;
        }

        private static int CountComponents(
            byte[] mask,
            int resolution,
            int minimumSize)
        {
            bool[] visited = new bool[mask.Length];
            Queue<int> queue = new Queue<int>();
            int componentCount = 0;

            for (int index = 0; index < mask.Length; index++)
            {
                if (mask[index] == 0 || visited[index])
                {
                    continue;
                }

                int size = 0;
                visited[index] = true;
                queue.Enqueue(index);
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    size++;
                    int x = current % resolution;
                    int y = current / resolution;
                    for (int neighbourIndex = 0;
                         neighbourIndex < CardinalNeighbours.Length;
                         neighbourIndex++)
                    {
                        Vector2Int offset = CardinalNeighbours[neighbourIndex];
                        int neighbourX = x + offset.x;
                        int neighbourY = y + offset.y;
                        if (neighbourX < 0 || neighbourX >= resolution ||
                            neighbourY < 0 || neighbourY >= resolution)
                        {
                            continue;
                        }

                        int neighbour = neighbourX + neighbourY * resolution;
                        if (mask[neighbour] == 0 || visited[neighbour])
                        {
                            continue;
                        }

                        visited[neighbour] = true;
                        queue.Enqueue(neighbour);
                    }
                }

                if (size >= minimumSize)
                {
                    componentCount++;
                }
            }

            return componentCount;
        }

        private static int CountEnclosedHoles(byte[] mask, int resolution)
        {
            bool[] outside = new bool[mask.Length];
            Queue<int> queue = new Queue<int>();
            for (int x = 0; x < resolution; x++)
            {
                EnqueueOutside(x, 0, resolution, mask, outside, queue);
                EnqueueOutside(
                    x,
                    resolution - 1,
                    resolution,
                    mask,
                    outside,
                    queue);
            }

            for (int y = 0; y < resolution; y++)
            {
                EnqueueOutside(0, y, resolution, mask, outside, queue);
                EnqueueOutside(
                    resolution - 1,
                    y,
                    resolution,
                    mask,
                    outside,
                    queue);
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                int x = current % resolution;
                int y = current / resolution;
                for (int neighbourIndex = 0;
                     neighbourIndex < CardinalNeighbours.Length;
                     neighbourIndex++)
                {
                    Vector2Int offset = CardinalNeighbours[neighbourIndex];
                    EnqueueOutside(
                        x + offset.x,
                        y + offset.y,
                        resolution,
                        mask,
                        outside,
                        queue);
                }
            }

            byte[] holes = new byte[mask.Length];
            for (int index = 0; index < mask.Length; index++)
            {
                holes[index] = mask[index] == 0 && !outside[index]
                    ? (byte)1
                    : (byte)0;
            }

            return CountComponents(holes, resolution, 1);
        }

        private static int EstimateMinimumNeckWidth(
            byte[] source,
            int resolution,
            int maximumErosions)
        {
            byte[] current = new byte[source.Length];
            byte[] next = new byte[source.Length];
            Buffer.BlockCopy(source, 0, current, 0, source.Length);

            for (int erosion = 1;
                 erosion <= maximumErosions;
                 erosion++)
            {
                Erode(current, next, resolution);
                int componentCount = CountComponents(next, resolution, 4);
                if (componentCount > 1)
                {
                    return erosion * 2 + 1;
                }

                if (componentCount == 0)
                {
                    return Mathf.Max(1, erosion * 2 - 1);
                }

                (current, next) = (next, current);
            }

            return maximumErosions * 2 + 1;
        }

        private static void BuildSoftSupport(
            byte[] mask,
            int resolution,
            float featherCells,
            float[] output)
        {
            float[] distanceToOutside = BuildDistanceField(
                mask,
                resolution,
                0);
            float[] distanceToInside = BuildDistanceField(
                mask,
                resolution,
                1);

            for (int index = 0; index < mask.Length; index++)
            {
                float signedDistance = mask[index] != 0
                    ? distanceToOutside[index] - 0.5f
                    : -distanceToInside[index] + 0.5f;
                output[index] = SmoothStep(
                    -featherCells,
                    featherCells,
                    signedDistance);
            }
        }

        private static float[] BuildDistanceField(
            byte[] mask,
            int resolution,
            byte targetValue)
        {
            float[] distance = new float[mask.Length];
            const float infinity = 100000f;
            for (int index = 0; index < mask.Length; index++)
            {
                distance[index] = mask[index] == targetValue
                    ? 0f
                    : infinity;
            }

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int index = x + y * resolution;
                    float value = distance[index];
                    if (x > 0)
                    {
                        value = Mathf.Min(value, distance[index - 1] + 1f);
                    }
                    if (y > 0)
                    {
                        value = Mathf.Min(
                            value,
                            distance[index - resolution] + 1f);
                    }
                    if (x > 0 && y > 0)
                    {
                        value = Mathf.Min(
                            value,
                            distance[index - resolution - 1] +
                            DiagonalDistance);
                    }
                    if (x < resolution - 1 && y > 0)
                    {
                        value = Mathf.Min(
                            value,
                            distance[index - resolution + 1] +
                            DiagonalDistance);
                    }
                    distance[index] = value;
                }
            }

            for (int y = resolution - 1; y >= 0; y--)
            {
                for (int x = resolution - 1; x >= 0; x--)
                {
                    int index = x + y * resolution;
                    float value = distance[index];
                    if (x < resolution - 1)
                    {
                        value = Mathf.Min(value, distance[index + 1] + 1f);
                    }
                    if (y < resolution - 1)
                    {
                        value = Mathf.Min(
                            value,
                            distance[index + resolution] + 1f);
                    }
                    if (x < resolution - 1 && y < resolution - 1)
                    {
                        value = Mathf.Min(
                            value,
                            distance[index + resolution + 1] +
                            DiagonalDistance);
                    }
                    if (x > 0 && y < resolution - 1)
                    {
                        value = Mathf.Min(
                            value,
                            distance[index + resolution - 1] +
                            DiagonalDistance);
                    }
                    distance[index] = value;
                }
            }

            return distance;
        }

        private static float ValueNoise(
            float x,
            float y,
            uint seed,
            uint stream)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float tx = x - x0;
            float ty = y - y0;
            float smoothX = SmootherStep(tx);
            float smoothY = SmootherStep(ty);

            float a = HashCell(seed, x0, y0, stream);
            float b = HashCell(seed, x0 + 1, y0, stream);
            float c = HashCell(seed, x0, y0 + 1, stream);
            float d = HashCell(seed, x0 + 1, y0 + 1, stream);
            return Mathf.Lerp(
                Mathf.Lerp(a, b, smoothX),
                Mathf.Lerp(c, d, smoothX),
                smoothY);
        }

        private static float HashCell(
            uint seed,
            int x,
            int y,
            uint stream)
        {
            uint value = seed;
            value ^= unchecked((uint)x) * 0x9E3779B9u;
            value ^= unchecked((uint)y) * 0x85EBCA6Bu;
            value ^= stream * 0xC2B2AE35u;
            return (MixBits(value) & 0x00FFFFFFu) /
                16777216f;
        }

        private static float Hash01(uint seed, uint stream)
        {
            return (MixBits(seed ^ MixBits(stream + 0xC2B2AE35u)) &
                0x00FFFFFFu) / 16777216f;
        }

        private static uint MixBits(uint value)
        {
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return value;
        }

        private static float SmootherStep(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * value *
                (value * (value * 6f - 15f) + 10f);
        }

        private static float SmoothStep(float minimum, float maximum, float value)
        {
            if (maximum <= minimum)
            {
                return value >= maximum ? 1f : 0f;
            }

            float t = Mathf.Clamp01((value - minimum) / (maximum - minimum));
            return t * t * (3f - 2f * t);
        }

        private readonly struct CandidateMetrics
        {
            public CandidateMetrics(
                int occupiedCells,
                float areaFraction,
                float compactness,
                int minimumNeckWidthCells,
                int componentCount,
                int holeCount,
                bool touchesStorageBoundary)
            {
                OccupiedCells = occupiedCells;
                AreaFraction = areaFraction;
                Compactness = compactness;
                MinimumNeckWidthCells = minimumNeckWidthCells;
                ComponentCount = componentCount;
                HoleCount = holeCount;
                TouchesStorageBoundary = touchesStorageBoundary;
            }

            public int OccupiedCells { get; }
            public float AreaFraction { get; }
            public float Compactness { get; }
            public int MinimumNeckWidthCells { get; }
            public int ComponentCount { get; }
            public int HoleCount { get; }
            public bool TouchesStorageBoundary { get; }
        }
    }
}
