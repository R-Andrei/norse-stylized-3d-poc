using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Deterministic CPU proof generator for static Pocket Aging Pressure.
    /// Pockets are hosted only by broad accepted Major interiors, preserve a
    /// positive rim, avoid important Connector cores and obstacle context, and
    /// remain independent from positive support. Authoritative live Pressure,
    /// Lee, and Shore protection is applied during GPU topology composition.
    /// </summary>
    public static class StylizedRiverFoamPocketTopologyGenerator
    {
        private const float MajorInteriorThreshold = 0.30f;
        private const float ConnectorCoreThreshold = 0.18f;
        private const float ConnectorProtectionRadiusMetres = 0.22f;
        private const float MinimumInteriorRadiusMetres = 0.48f;
        private const float PositiveRimWidthMetres = 0.22f;
        private const float RimFeatherMetres = 0.10f;
        private const float MinimumPocketRadiusMetres = 0.24f;
        private const float MaximumPocketRadiusMetres = 1.20f;
        private const float MinimumPocketSpacingMetres = 0.62f;
        private const float PocketCoverageThreshold = 0.15f;
        private const int MaximumPocketsPerHost = 2;

        public static StylizedRiverFoamPocketTopology Generate(
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength,
            StylizedRiverQuality quality,
            float shoreMotion,
            int seed,
            float[] obstacleMask,
            StylizedRiverFoamMajorTopology majorTopology,
            StylizedRiverFoamConnectorTopology connectorTopology)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int cellCount = Mathf.Max(0, width * height);
            float[] pressure = new float[cellCount];
            int reasonCount = Enum.GetValues(
                typeof(StylizedRiverFoamPocketRejectionReason)).Length;
            int[] rejectionCounts = new int[reasonCount];

            if (domain == null || !domain.IsValid ||
                majorTopology == null || connectorTopology == null ||
                width < 2 || height < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f ||
                majorTopology.Width != width || majorTopology.Height != height ||
                connectorTopology.Width != width ||
                connectorTopology.Height != height)
            {
                stopwatch.Stop();
                return CreateEmpty(
                    width,
                    height,
                    stopwatch.Elapsed.TotalMilliseconds,
                    pressure,
                    rejectionCounts);
            }

            float[] majorSupport = majorTopology.SupportData;
            float[] connectorSupport = connectorTopology.SupportData;
            if (majorSupport == null || majorSupport.Length < cellCount ||
                connectorSupport == null || connectorSupport.Length < cellCount)
            {
                stopwatch.Stop();
                return CreateEmpty(
                    width,
                    height,
                    stopwatch.Elapsed.TotalMilliseconds,
                    pressure,
                    rejectionCounts);
            }

            seed = Mathf.Max(0, seed);
            float[] fluidCoverage = new float[cellCount];
            bool[] validCells = new bool[cellCount];
            StylizedRiverFoamMajorTopologyGenerator.BuildFluidContext(
                domain,
                width,
                height,
                fieldLength,
                validFieldLength,
                quality,
                shoreMotion,
                obstacleMask,
                fluidCoverage,
                validCells);

            Vector2[] metricPositions = BuildMetricPositions(
                domain,
                width,
                height,
                fieldLength,
                validFieldLength);
            float[] connectorDistance = BuildDistanceFromSources(
                width,
                height,
                metricPositions,
                index => connectorSupport[index] >= ConnectorCoreThreshold,
                null);

            bool[] pocketInterior = new bool[cellCount];
            for (int index = 0; index < cellCount; index++)
            {
                pocketInterior[index] =
                    validCells[index] &&
                    fluidCoverage[index] > 0.35f &&
                    majorSupport[index] >= MajorInteriorThreshold &&
                    connectorDistance[index] >
                        ConnectorProtectionRadiusMetres;
            }

            float[] interiorDistance = BuildDistanceFromSources(
                width,
                height,
                metricPositions,
                index => !pocketInterior[index],
                pocketInterior);
            List<HostMetric> hosts = BuildHostMetrics(
                majorTopology.Regions,
                domain);
            List<PocketCandidate> candidates = FindCandidateCentres(
                width,
                height,
                domain,
                seed,
                pocketInterior,
                interiorDistance,
                metricPositions,
                hosts);

            HashSet<uint> eligibleHosts = new HashSet<uint>();
            for (int index = 0; index < candidates.Count; index++)
            {
                eligibleHosts.Add(candidates[index].HostRegionId);
            }

            candidates.Sort((a, b) =>
            {
                int scoreComparison = b.Score.CompareTo(a.Score);
                return scoreComparison != 0
                    ? scoreComparison
                    : a.StableId.CompareTo(b.StableId);
            });

            Dictionary<uint, int> acceptedByHost = new();
            List<AcceptedPocket> accepted = new();
            List<StylizedRiverFoamPocketRegion> regions = new();
            float[] candidateRaster = new float[cellCount];

            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                PocketCandidate candidate = candidates[candidateIndex];
                acceptedByHost.TryGetValue(
                    candidate.HostRegionId,
                    out int hostPocketCount);
                if (hostPocketCount >= MaximumPocketsPerHost)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason.Spacing]++;
                    continue;
                }

                float availableRadius = candidate.InteriorRadius -
                    PositiveRimWidthMetres;
                if (availableRadius < MinimumPocketRadiusMetres)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason.HostTooNarrow]++;
                    continue;
                }

                float radiusSelector = Hash01(candidate.StableId, 11u);
                float baseRadius = Mathf.Clamp(
                    availableRadius * Mathf.Lerp(
                        0.62f,
                        0.88f,
                        radiusSelector),
                    MinimumPocketRadiusMetres,
                    MaximumPocketRadiusMetres);
                float aspectSelector = Hash01(candidate.StableId, 12u);
                float alongRadius = baseRadius * Mathf.Lerp(
                    0.82f,
                    1.24f,
                    aspectSelector);
                float acrossRadius = baseRadius * Mathf.Lerp(
                    1.12f,
                    0.72f,
                    aspectSelector);
                float largestRadius = Mathf.Max(alongRadius, acrossRadius);

                bool spacingRejected = false;
                for (int acceptedIndex = 0;
                     acceptedIndex < accepted.Count;
                     acceptedIndex++)
                {
                    AcceptedPocket existing = accepted[acceptedIndex];
                    float requiredSpacing = Mathf.Max(
                        MinimumPocketSpacingMetres,
                        (largestRadius + existing.LargestRadius) * 0.72f);
                    if (Vector2.Distance(
                            candidate.Position,
                            existing.Position) < requiredSpacing)
                    {
                        spacingRejected = true;
                        break;
                    }
                }

                if (spacingRejected)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason.Spacing]++;
                    continue;
                }

                float orientation = Mathf.Lerp(
                    -Mathf.PI,
                    Mathf.PI,
                    Hash01(candidate.StableId, 13u));
                Array.Clear(candidateRaster, 0, candidateRaster.Length);
                int rasterCoverage = RasterizePocket(
                    candidate,
                    orientation,
                    alongRadius,
                    acrossRadius,
                    width,
                    height,
                    validCells,
                    majorSupport,
                    connectorSupport,
                    connectorDistance,
                    interiorDistance,
                    metricPositions,
                    candidateRaster);
                if (rasterCoverage <= 0)
                {
                    rejectionCounts[(int)
                        StylizedRiverFoamPocketRejectionReason
                            .NoRasterCoverage]++;
                    continue;
                }

                for (int index = 0; index < pressure.Length; index++)
                {
                    pressure[index] = Mathf.Max(
                        pressure[index],
                        candidateRaster[index]);
                }

                acceptedByHost[candidate.HostRegionId] = hostPocketCount + 1;
                accepted.Add(new AcceptedPocket(
                    candidate.Position,
                    largestRadius));

                uint evolutionSeed = MixBits(
                    candidate.StableId ^ 0xD1B54A35u);
                regions.Add(new StylizedRiverFoamPocketRegion(
                    candidate.StableId,
                    candidate.HostRegionId,
                    domain.GlobalDistanceMinimum + candidate.Position.x,
                    candidate.AcrossNormalized,
                    orientation,
                    alongRadius,
                    acrossRadius,
                    evolutionSeed,
                    Hash01(evolutionSeed, 21u),
                    Hash01(evolutionSeed, 22u),
                    Hash01(evolutionSeed, 23u),
                    Hash01(evolutionSeed, 24u),
                    Hash01(evolutionSeed, 25u),
                    Hash01(evolutionSeed, 26u),
                    Hash01(evolutionSeed, 27u)));
            }

            int coveredCellCount = 0;
            for (int index = 0; index < pressure.Length; index++)
            {
                if (pressure[index] >= PocketCoverageThreshold)
                {
                    coveredCellCount++;
                }
            }

            stopwatch.Stop();
            return new StylizedRiverFoamPocketTopology(
                width,
                height,
                eligibleHosts.Count,
                candidates.Count,
                regions.Count,
                coveredCellCount,
                stopwatch.Elapsed.TotalMilliseconds,
                pressure,
                regions.ToArray(),
                rejectionCounts);
        }

        private static StylizedRiverFoamPocketTopology CreateEmpty(
            int width,
            int height,
            double milliseconds,
            float[] pressure,
            int[] rejectionCounts)
        {
            return new StylizedRiverFoamPocketTopology(
                width,
                height,
                0,
                0,
                0,
                0,
                milliseconds,
                pressure,
                Array.Empty<StylizedRiverFoamPocketRegion>(),
                rejectionCounts);
        }

        private static List<HostMetric> BuildHostMetrics(
            IReadOnlyList<StylizedRiverFoamMajorRegion> regions,
            RiverDomainSnapshot domain)
        {
            List<HostMetric> hosts = new();
            if (regions == null)
            {
                return hosts;
            }

            for (int index = 0; index < regions.Count; index++)
            {
                StylizedRiverFoamMajorRegion region = regions[index];
                float localDistance = Mathf.Clamp(
                    region.CentreGlobalDistance -
                        domain.GlobalDistanceMinimum,
                    0f,
                    domain.LocalLength);
                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(localDistance);
                float lateral = SignedNormalizedToMetres(
                    region.CentreAcrossNormalized,
                    Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth),
                    Mathf.Max(0.05f, sample.RightSurfaceHalfWidth));
                hosts.Add(new HostMetric(
                    region.StableId,
                    new Vector2(localDistance, lateral)));
            }

            return hosts;
        }

        private static List<PocketCandidate> FindCandidateCentres(
            int width,
            int height,
            RiverDomainSnapshot domain,
            int seed,
            bool[] interior,
            float[] distance,
            Vector2[] positions,
            List<HostMetric> hosts)
        {
            List<PocketCandidate> candidates = new();
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int index = x + y * width;
                    float radius = distance[index];
                    if (!interior[index] ||
                        radius < MinimumInteriorRadiusMetres ||
                        !IsLocalMaximum(index, width, distance))
                    {
                        continue;
                    }

                    uint hostId = ResolveNearestHost(
                        positions[index],
                        hosts);
                    uint stableId = MixBits(
                        (uint)(seed + 1) * 0x9E3779B9u ^
                        hostId ^
                        (uint)(index + 1) * 0x85EBCA6Bu);
                    float jitter = Hash01(stableId, 4u) * 0.035f;
                    float acrossNormalized = ResolveAcrossNormalized(
                        domain,
                        positions[index]);
                    candidates.Add(new PocketCandidate(
                        stableId,
                        hostId,
                        index,
                        positions[index],
                        acrossNormalized,
                        radius,
                        radius + jitter));
                }
            }

            return candidates;
        }

        private static bool IsLocalMaximum(
            int index,
            int width,
            float[] distance)
        {
            float value = distance[index];
            int[] offsets =
            {
                -width - 1, -width, -width + 1,
                -1, 1,
                width - 1, width, width + 1
            };

            bool strictlyGreaterThanOne = false;
            for (int offsetIndex = 0;
                 offsetIndex < offsets.Length;
                 offsetIndex++)
            {
                float neighbour = distance[index + offsets[offsetIndex]];
                if (neighbour > value + 0.0001f)
                {
                    return false;
                }

                if (value > neighbour + 0.0001f)
                {
                    strictlyGreaterThanOne = true;
                }
            }

            return strictlyGreaterThanOne;
        }

        private static int RasterizePocket(
            PocketCandidate candidate,
            float orientation,
            float alongRadius,
            float acrossRadius,
            int width,
            int height,
            bool[] validCells,
            float[] majorSupport,
            float[] connectorSupport,
            float[] connectorDistance,
            float[] interiorDistance,
            Vector2[] positions,
            float[] destination)
        {
            float cos = Mathf.Cos(orientation);
            float sin = Mathf.Sin(orientation);
            float outerRadius = Mathf.Max(alongRadius, acrossRadius) * 1.35f;
            int written = 0;
            uint noiseSeed = MixBits(candidate.StableId ^ 0xA511E9B3u);

            for (int index = 0; index < destination.Length; index++)
            {
                if (!validCells[index] ||
                    majorSupport[index] < MajorInteriorThreshold ||
                    connectorSupport[index] >= ConnectorCoreThreshold ||
                    connectorDistance[index] <=
                        ConnectorProtectionRadiusMetres ||
                    interiorDistance[index] <= PositiveRimWidthMetres)
                {
                    continue;
                }

                Vector2 delta = positions[index] - candidate.Position;
                if (delta.sqrMagnitude > outerRadius * outerRadius)
                {
                    continue;
                }

                float localX = cos * delta.x + sin * delta.y;
                float localY = -sin * delta.x + cos * delta.y;
                float normalizedX = localX / Mathf.Max(0.001f, alongRadius);
                float normalizedY = localY / Mathf.Max(0.001f, acrossRadius);

                float noiseX = normalizedX * 1.75f + 13.7f;
                float noiseY = normalizedY * 1.75f - 8.3f;
                float warpX = (ValueNoise(noiseX, noiseY, noiseSeed) - 0.5f) *
                    0.22f;
                float warpY = (ValueNoise(
                    noiseX + 5.17f,
                    noiseY - 3.43f,
                    noiseSeed ^ 0x68BC21EBu) - 0.5f) * 0.22f;
                float warpedX = normalizedX + warpX;
                float warpedY = normalizedY + warpY;
                float radial = Mathf.Sqrt(
                    warpedX * warpedX + warpedY * warpedY);
                float broadNoise = ValueNoise(
                    normalizedX * 2.35f + 21.1f,
                    normalizedY * 2.35f + 4.7f,
                    noiseSeed ^ 0xC2B2AE35u);
                float shapeDistance = radial + (broadNoise - 0.5f) * 0.24f;
                float shape = 1f - SmoothStep(0.70f, 1.02f, shapeDistance);
                if (shape <= 0.001f)
                {
                    continue;
                }

                float rimGate = SmoothStep(
                    PositiveRimWidthMetres,
                    PositiveRimWidthMetres + RimFeatherMetres,
                    interiorDistance[index]);
                float connectorGate = SmoothStep(
                    ConnectorProtectionRadiusMetres,
                    ConnectorProtectionRadiusMetres + 0.12f,
                    connectorDistance[index]);
                float value = Mathf.Clamp01(
                    shape * rimGate * connectorGate);
                destination[index] = Mathf.Max(destination[index], value);
                if (value >= PocketCoverageThreshold)
                {
                    written++;
                }
            }

            return written;
        }

        private static float[] BuildDistanceFromSources(
            int width,
            int height,
            Vector2[] positions,
            Func<int, bool> isSource,
            bool[] traversable)
        {
            int cellCount = width * height;
            float[] distance = new float[cellCount];
            MinHeap heap = new MinHeap(cellCount);
            for (int index = 0; index < cellCount; index++)
            {
                if (isSource(index))
                {
                    distance[index] = 0f;
                    heap.Push(index, 0f);
                }
                else
                {
                    distance[index] = float.PositiveInfinity;
                }
            }

            int[] neighbourX = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] neighbourY = { -1, -1, -1, 0, 0, 1, 1, 1 };
            while (heap.Count > 0)
            {
                heap.Pop(out int index, out float currentDistance);
                if (currentDistance > distance[index] + 0.0001f)
                {
                    continue;
                }

                int x = index % width;
                int y = index / width;
                for (int neighbourIndex = 0;
                     neighbourIndex < neighbourX.Length;
                     neighbourIndex++)
                {
                    int nx = x + neighbourX[neighbourIndex];
                    int ny = y + neighbourY[neighbourIndex];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    {
                        continue;
                    }

                    int next = nx + ny * width;
                    if (traversable != null && !traversable[next])
                    {
                        continue;
                    }

                    float step = Vector2.Distance(
                        positions[index],
                        positions[next]);
                    float proposed = currentDistance + step;
                    if (proposed + 0.0001f >= distance[next])
                    {
                        continue;
                    }

                    distance[next] = proposed;
                    heap.Push(next, proposed);
                }
            }

            return distance;
        }

        private static Vector2[] BuildMetricPositions(
            RiverDomainSnapshot domain,
            int width,
            int height,
            float fieldLength,
            float validFieldLength)
        {
            Vector2[] positions = new Vector2[width * height];
            for (int x = 0; x < width; x++)
            {
                float localDistance = x /
                    (float)Mathf.Max(1, width - 1) * fieldLength;
                float clampedDistance = Mathf.Min(
                    localDistance,
                    validFieldLength);
                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(clampedDistance);
                float leftSurface = Mathf.Max(
                    0.05f,
                    sample.LeftSurfaceHalfWidth);
                float rightSurface = Mathf.Max(
                    0.05f,
                    sample.RightSurfaceHalfWidth);

                for (int y = 0; y < height; y++)
                {
                    float across01 = y /
                        (float)Mathf.Max(1, height - 1);
                    positions[x + y * width] = new Vector2(
                        localDistance,
                        Across01ToMetres(
                            across01,
                            leftSurface,
                            rightSurface));
                }
            }

            return positions;
        }

        private static uint ResolveNearestHost(
            Vector2 position,
            List<HostMetric> hosts)
        {
            if (hosts == null || hosts.Count == 0)
            {
                return MixBits(
                    (uint)Mathf.Max(1, Mathf.RoundToInt(position.x * 97f)) ^
                    (uint)Mathf.Max(1, Mathf.RoundToInt(
                        (position.y + 32f) * 131f)));
            }

            int bestIndex = 0;
            float bestDistance = float.PositiveInfinity;
            for (int index = 0; index < hosts.Count; index++)
            {
                float distance = (position - hosts[index].Position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = index;
                }
            }

            return hosts[bestIndex].StableId;
        }

        private static float ResolveAcrossNormalized(
            RiverDomainSnapshot domain,
            Vector2 position)
        {
            StylizedRiverSplineSample sample =
                domain.SampleAtOrientedDistance(Mathf.Clamp(
                    position.x,
                    0f,
                    domain.LocalLength));
            float left = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
            float right = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
            return position.y < 0f
                ? Mathf.Clamp(position.y / left, -1f, 0f)
                : Mathf.Clamp(position.y / right, 0f, 1f);
        }

        private static float Across01ToMetres(
            float across01,
            float leftWidth,
            float rightWidth)
        {
            if (across01 <= 0.5f)
            {
                return Mathf.Lerp(-leftWidth, 0f, across01 * 2f);
            }

            return Mathf.Lerp(0f, rightWidth, (across01 - 0.5f) * 2f);
        }

        private static float SignedNormalizedToMetres(
            float acrossNormalized,
            float leftWidth,
            float rightWidth)
        {
            return acrossNormalized < 0f
                ? acrossNormalized * leftWidth
                : acrossNormalized * rightWidth;
        }

        private static float ValueNoise(
            float x,
            float y,
            uint seed)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float tx = Smooth01(x - x0);
            float ty = Smooth01(y - y0);
            float a = HashLattice(x0, y0, seed);
            float b = HashLattice(x0 + 1, y0, seed);
            float c = HashLattice(x0, y0 + 1, seed);
            float d = HashLattice(x0 + 1, y0 + 1, seed);
            return Mathf.Lerp(
                Mathf.Lerp(a, b, tx),
                Mathf.Lerp(c, d, tx),
                ty);
        }

        private static float HashLattice(int x, int y, uint seed)
        {
            uint value = seed ^
                (uint)x * 0x8DA6B343u ^
                (uint)y * 0xD8163841u;
            return Hash01(MixBits(value), 0xCB1AB31Fu);
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            if (edge1 <= edge0 + 0.000001f)
            {
                return value >= edge1 ? 1f : 0f;
            }

            return Smooth01((value - edge0) / (edge1 - edge0));
        }

        private static float Hash01(uint value, uint salt)
        {
            uint mixed = MixBits(value ^ salt * 0x9E3779B9u);
            return (mixed & 0x00FFFFFFu) / 16777215f;
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

        private readonly struct HostMetric
        {
            public HostMetric(uint stableId, Vector2 position)
            {
                StableId = stableId;
                Position = position;
            }

            public uint StableId { get; }
            public Vector2 Position { get; }
        }

        private readonly struct PocketCandidate
        {
            public PocketCandidate(
                uint stableId,
                uint hostRegionId,
                int cellIndex,
                Vector2 position,
                float acrossNormalized,
                float interiorRadius,
                float score)
            {
                StableId = stableId;
                HostRegionId = hostRegionId;
                CellIndex = cellIndex;
                Position = position;
                AcrossNormalized = acrossNormalized;
                InteriorRadius = interiorRadius;
                Score = score;
            }

            public uint StableId { get; }
            public uint HostRegionId { get; }
            public int CellIndex { get; }
            public Vector2 Position { get; }
            public float AcrossNormalized { get; }
            public float InteriorRadius { get; }
            public float Score { get; }
        }

        private readonly struct AcceptedPocket
        {
            public AcceptedPocket(Vector2 position, float largestRadius)
            {
                Position = position;
                LargestRadius = largestRadius;
            }

            public Vector2 Position { get; }
            public float LargestRadius { get; }
        }

        private sealed class MinHeap
        {
            private int[] indices;
            private float[] priorities;

            public MinHeap(int capacity)
            {
                capacity = Mathf.Max(4, capacity);
                indices = new int[capacity];
                priorities = new float[capacity];
            }

            public int Count { get; private set; }

            public void Push(int index, float priority)
            {
                EnsureCapacity(Count + 1);
                int position = Count++;
                while (position > 0)
                {
                    int parent = (position - 1) >> 1;
                    if (priorities[parent] <= priority)
                    {
                        break;
                    }

                    indices[position] = indices[parent];
                    priorities[position] = priorities[parent];
                    position = parent;
                }

                indices[position] = index;
                priorities[position] = priority;
            }

            public void Pop(out int index, out float priority)
            {
                index = indices[0];
                priority = priorities[0];
                Count--;
                if (Count <= 0)
                {
                    return;
                }

                int tailIndex = indices[Count];
                float tailPriority = priorities[Count];
                int position = 0;
                while (true)
                {
                    int left = position * 2 + 1;
                    if (left >= Count)
                    {
                        break;
                    }

                    int right = left + 1;
                    int child = right < Count &&
                        priorities[right] < priorities[left]
                            ? right
                            : left;
                    if (priorities[child] >= tailPriority)
                    {
                        break;
                    }

                    indices[position] = indices[child];
                    priorities[position] = priorities[child];
                    position = child;
                }

                indices[position] = tailIndex;
                priorities[position] = tailPriority;
            }

            private void EnsureCapacity(int required)
            {
                if (required <= indices.Length)
                {
                    return;
                }

                int next = Mathf.Max(required, indices.Length * 2);
                Array.Resize(ref indices, next);
                Array.Resize(ref priorities, next);
            }
        }
    }
}
