using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    [Flags]
    public enum GeneratedMassFeatureAtlasRequest
    {
        None = 0,
        FeatureAtlas0 = 1 << 0,
        FeatureAtlas1 = 1 << 1
    }

    /// <summary>
    /// Builds generated-mass surface-chart atlases used by the main material
    /// path. The atlases store reusable boundary facts: convex/concave
    /// proximity, structural salience, stable boundary identity, and
    /// boundary-local coordinates/modulation. Material features such as edge
    /// wear interpret those facts; the atlases themselves do not bake final
    /// decorative paint or feature-specific macro/micro controls.
    /// </summary>
    public static class GeneratedMassFeatureAtlasBaker
    {
        public const int DefaultResolution = 512;
        private const int MinimumResolution = 128;
        private const int MaximumResolution = 512;
        private const float GutterDistanceMultiplier = 1.45f;
        private const int ChartPaddingPixels = 4;
        private const int MinimumChartInteriorPixels = 6;
        private const float PatchPackUsage = 0.72f;
        private const float ChartFitMargin = 0.0005f;

        public sealed class Result
        {
            public Result(Texture2D atlas0, Texture2D atlas1, List<Vector2> featureAtlasUV)
            {
                Atlas0 = atlas0;
                Atlas1 = atlas1;
                FeatureAtlasUV = featureAtlasUV;
            }

            public Texture2D Atlas0 { get; }
            public Texture2D Atlas1 { get; }
            public List<Vector2> FeatureAtlasUV { get; }
        }

        private sealed class PatchChart
        {
            public MassSurfaceFeatureGraph.Patch Patch;
            public Vector3 Origin;
            public Vector3 AxisU;
            public Vector3 AxisV;
            public Vector2 LocalMin;
            public Vector2 LocalMax;
            public float WidthWorld;
            public float HeightWorld;
            public int X;
            public int Y;
            public int WidthPixels;
            public int HeightPixels;
            public float PixelsPerWorld;
            public readonly List<BoundarySegment> ConvexSegments = new();
            public readonly List<BoundarySegment> ConcaveSegments = new();
        }

        private readonly struct BoundarySegment
        {
            public BoundarySegment(
                Vector2 start,
                Vector2 end,
                float score,
                float salience,
                int boundaryIndex,
                int chainIndex,
                float chainDistanceAtStart,
                float chainDistanceAtEnd,
                float chainLengthWorld,
                float sideSign,
                float lengthWorld)
            {
                Start = start;
                End = end;
                Score = Mathf.Clamp01(score);
                Salience = Mathf.Clamp01(salience);
                BoundaryIndex = boundaryIndex;
                ChainIndex = chainIndex;
                ChainDistanceAtStart = Mathf.Max(0f, chainDistanceAtStart);
                ChainDistanceAtEnd = Mathf.Max(0f, chainDistanceAtEnd);
                ChainLengthWorld = Mathf.Max(0.0001f, chainLengthWorld);
                SideSign = sideSign < 0f ? -1f : sideSign > 0f ? 1f : 0f;
                LengthWorld = Mathf.Max(0.0001f, lengthWorld);
            }

            public Vector2 Start { get; }
            public Vector2 End { get; }
            public float Score { get; }
            public float Salience { get; }
            public int BoundaryIndex { get; }
            public int ChainIndex { get; }
            public float ChainDistanceAtStart { get; }
            public float ChainDistanceAtEnd { get; }
            public float ChainLengthWorld { get; }
            public float SideSign { get; }
            public float LengthWorld { get; }
        }

        private readonly struct ProjectedTriangle
        {
            public ProjectedTriangle(Vector2 a, Vector2 b, Vector2 c)
            {
                A = a;
                B = b;
                C = c;
            }

            public Vector2 A { get; }
            public Vector2 B { get; }
            public Vector2 C { get; }
        }

        private readonly struct BoundaryFieldSample
        {
            public BoundaryFieldSample(
                float proximity,
                float coreProximity,
                float salience,
                float identity,
                float along,
                float cross,
                float coarseModulation,
                float fineModulation,
                int groupKey)
            {
                Proximity = Mathf.Clamp01(proximity);
                CoreProximity = Mathf.Clamp01(coreProximity);
                Salience = Mathf.Clamp01(salience);
                Identity = Mathf.Clamp01(identity);
                Along = Mathf.Repeat(along, 1f);
                Cross = Mathf.Clamp01(cross);
                CoarseModulation = Mathf.Clamp01(coarseModulation);
                FineModulation = Mathf.Clamp01(fineModulation);
                GroupKey = groupKey;
            }

            public float Proximity { get; }
            public float CoreProximity { get; }
            public float Salience { get; }
            public float Identity { get; }
            public float Along { get; }
            public float Cross { get; }
            public float CoarseModulation { get; }
            public float FineModulation { get; }
            public int GroupKey { get; }
            public float Composite => Mathf.Max(Proximity, CoreProximity) * Mathf.Lerp(0.55f, 1f, Salience);
        }

        private struct BoundaryFieldGroupAccumulator
        {
            private const float MinimumWeight = 0.0001f;

            public BoundaryFieldGroupAccumulator(int key)
            {
                Key = key;
                Weight = 0f;
                SideSignalWeight = 0f;
                CrossSideSignalSum = 0f;
                ModulationWeightSum = 0f;
                CoarseModulationSum = 0f;
                FineModulationSum = 0f;
                BestComposite = 0f;
                BestSample = default;
            }

            public int Key { get; private set; }
            public float Weight { get; private set; }
            public float SideSignalWeight { get; private set; }
            public float CrossSideSignalSum { get; private set; }
            public float ModulationWeightSum { get; private set; }
            public float CoarseModulationSum { get; private set; }
            public float FineModulationSum { get; private set; }
            public float BestComposite { get; private set; }
            public BoundaryFieldSample BestSample { get; private set; }
            public bool HasValue => Weight > MinimumWeight;

            public void Add(BoundaryFieldSample sample, float weight)
            {
                weight = Mathf.Max(MinimumWeight, weight);
                Weight += weight;

                // Cross is a side/distance coordinate. Exact ridge-core samples
                // legitimately have Cross = 0.5, but they should not dominate a
                // low-resolution texel's side classification. Weight Cross by
                // its side signal so the resolved value comes from the dominant
                // side of the boundary neighborhood rather than from neutral core
                // samples or from a mixed average of both sides.
                float sideSignal = Mathf.Clamp01(Mathf.Abs(sample.Cross - 0.5f) * 2f);
                float sideWeight = weight * sideSignal;
                if (sideWeight > MinimumWeight)
                {
                    SideSignalWeight += sideWeight;
                    CrossSideSignalSum += sample.Cross * sideWeight;
                }

                ModulationWeightSum += weight;
                CoarseModulationSum += sample.CoarseModulation * weight;
                FineModulationSum += sample.FineModulation * weight;

                float composite = sample.Composite;
                if (composite >= BestComposite)
                {
                    BestComposite = composite;
                    BestSample = sample;
                }
            }

            public float ResolveCross()
            {
                return SideSignalWeight > MinimumWeight
                    ? Mathf.Clamp01(CrossSideSignalSum / SideSignalWeight)
                    : BestSample.Cross;
            }

            public float ResolveCoarseModulation()
            {
                return ModulationWeightSum > MinimumWeight
                    ? Mathf.Clamp01(CoarseModulationSum / ModulationWeightSum)
                    : BestSample.CoarseModulation;
            }

            public float ResolveFineModulation()
            {
                return ModulationWeightSum > MinimumWeight
                    ? Mathf.Clamp01(FineModulationSum / ModulationWeightSum)
                    : BestSample.FineModulation;
            }
        }

        private struct BoundaryFieldAccumulator
        {
            private const int MaxGroupsPerTexel = 8;

            private readonly int totalSamples;
            private int contributingSamples;
            private float proximitySum;
            private float maxProximity;
            private float maxCoreProximity;
            private float bestComposite;
            private BoundaryFieldSample bestSample;
            private int groupCount;
            private BoundaryFieldGroupAccumulator group0;
            private BoundaryFieldGroupAccumulator group1;
            private BoundaryFieldGroupAccumulator group2;
            private BoundaryFieldGroupAccumulator group3;
            private BoundaryFieldGroupAccumulator group4;
            private BoundaryFieldGroupAccumulator group5;
            private BoundaryFieldGroupAccumulator group6;
            private BoundaryFieldGroupAccumulator group7;

            public BoundaryFieldAccumulator(int totalSamples)
            {
                this.totalSamples = Mathf.Max(1, totalSamples);
                contributingSamples = 0;
                proximitySum = 0f;
                maxProximity = 0f;
                maxCoreProximity = 0f;
                bestComposite = 0f;
                bestSample = default;
                groupCount = 0;
                group0 = default;
                group1 = default;
                group2 = default;
                group3 = default;
                group4 = default;
                group5 = default;
                group6 = default;
                group7 = default;
            }

            public void Add(BoundaryFieldSample sample)
            {
                float proximity = Mathf.Clamp01(sample.Proximity);
                float coreProximity = Mathf.Clamp01(sample.CoreProximity);
                if (proximity <= 0.001f && coreProximity <= 0.001f)
                {
                    return;
                }

                contributingSamples++;
                proximitySum += proximity;
                maxProximity = Mathf.Max(maxProximity, proximity);
                maxCoreProximity = Mathf.Max(maxCoreProximity, coreProximity);

                float composite = sample.Composite;
                float groupWeight = Mathf.Max(0.0001f, composite);
                AddToGroup(sample, groupWeight);

                if (composite >= bestComposite)
                {
                    bestComposite = composite;
                    bestSample = sample;
                }
            }

            public BoundaryFieldSample Resolve()
            {
                if (contributingSamples <= 0 ||
                    (maxProximity <= 0.001f && maxCoreProximity <= 0.001f))
                {
                    return default;
                }

                float coverage = contributingSamples / (float)Mathf.Max(1, totalSamples);
                float average = proximitySum / Mathf.Max(1, totalSamples);

                // Low atlas resolutions cannot rely on a single texel-center
                // point sample for broad boundary shoulders. Use a conservative
                // coverage/max hybrid for the shoulder, but preserve the narrow
                // ridge core separately. Otherwise Compact/128 keeps the blur
                // band while losing the semantic hard edge that should anchor it.
                float conservative = maxProximity * Mathf.Pow(Mathf.Clamp01(coverage), 0.45f);
                float shoulderProximity = Mathf.Clamp01(Mathf.Max(average, conservative));
                float resolvedProximity = Mathf.Clamp01(Mathf.Max(shoulderProximity, maxCoreProximity));

                // Atlas1 channels are boundary-local coordinates/facts. At 128
                // and 256, one texel can cover multiple incompatible boundary
                // neighborhoods. Resolving Cross/B/A from a pooled average makes
                // side assignment and modulation follow the same jagged mixed
                // pattern. Pick the dominant boundary-side group first, then
                // resolve the coordinate/modulation fields only inside that group.
                BoundaryFieldGroupAccumulator dominantGroup = ResolveDominantGroup();
                BoundaryFieldSample dominantSample = dominantGroup.HasValue
                    ? dominantGroup.BestSample
                    : bestSample;
                float resolvedCross = dominantGroup.HasValue
                    ? dominantGroup.ResolveCross()
                    : dominantSample.Cross;
                float resolvedCoarseModulation = dominantGroup.HasValue
                    ? dominantGroup.ResolveCoarseModulation()
                    : dominantSample.CoarseModulation;
                float resolvedFineModulation = dominantGroup.HasValue
                    ? dominantGroup.ResolveFineModulation()
                    : dominantSample.FineModulation;

                return new BoundaryFieldSample(
                    resolvedProximity,
                    maxCoreProximity,
                    dominantSample.Salience,
                    dominantSample.Identity,
                    dominantSample.Along,
                    resolvedCross,
                    resolvedCoarseModulation,
                    resolvedFineModulation,
                    dominantSample.GroupKey);
            }

            private void AddToGroup(BoundaryFieldSample sample, float weight)
            {
                int groupIndex = FindGroupIndex(sample.GroupKey);
                if (groupIndex < 0)
                {
                    if (groupCount < MaxGroupsPerTexel)
                    {
                        groupIndex = groupCount;
                        SetGroup(groupIndex, new BoundaryFieldGroupAccumulator(sample.GroupKey));
                        groupCount++;
                    }
                    else
                    {
                        groupIndex = FindWeakestGroupIndex();
                        BoundaryFieldGroupAccumulator weakest = GetGroup(groupIndex);
                        if (weakest.HasValue && weakest.Weight > weight)
                        {
                            return;
                        }

                        SetGroup(groupIndex, new BoundaryFieldGroupAccumulator(sample.GroupKey));
                    }
                }

                BoundaryFieldGroupAccumulator group = GetGroup(groupIndex);
                group.Add(sample, weight);
                SetGroup(groupIndex, group);
            }

            private int FindGroupIndex(int key)
            {
                for (int i = 0; i < groupCount; i++)
                {
                    if (GetGroup(i).Key == key)
                    {
                        return i;
                    }
                }

                return -1;
            }

            private int FindWeakestGroupIndex()
            {
                int weakestIndex = 0;
                float weakestWeight = GetGroup(0).Weight;
                for (int i = 1; i < groupCount; i++)
                {
                    float weight = GetGroup(i).Weight;
                    if (weight < weakestWeight)
                    {
                        weakestWeight = weight;
                        weakestIndex = i;
                    }
                }

                return weakestIndex;
            }

            private BoundaryFieldGroupAccumulator ResolveDominantGroup()
            {
                BoundaryFieldGroupAccumulator dominant = default;
                float dominantWeight = 0f;
                for (int i = 0; i < groupCount; i++)
                {
                    BoundaryFieldGroupAccumulator group = GetGroup(i);
                    if (!group.HasValue)
                    {
                        continue;
                    }

                    // Prefer accumulated boundary support, with a small bonus for
                    // groups that also carry a clear side signal. This keeps exact
                    // ridge-core texels valid while preventing mixed side samples
                    // from dragging the result toward Cross = 0.5.
                    float weight = group.Weight + group.SideSignalWeight * 0.25f;
                    if (weight >= dominantWeight)
                    {
                        dominantWeight = weight;
                        dominant = group;
                    }
                }

                return dominant;
            }

            private BoundaryFieldGroupAccumulator GetGroup(int index)
            {
                return index switch
                {
                    0 => group0,
                    1 => group1,
                    2 => group2,
                    3 => group3,
                    4 => group4,
                    5 => group5,
                    6 => group6,
                    7 => group7,
                    _ => default
                };
            }

            private void SetGroup(int index, BoundaryFieldGroupAccumulator value)
            {
                switch (index)
                {
                    case 0:
                        group0 = value;
                        break;
                    case 1:
                        group1 = value;
                        break;
                    case 2:
                        group2 = value;
                        break;
                    case 3:
                        group3 = value;
                        break;
                    case 4:
                        group4 = value;
                        break;
                    case 5:
                        group5 = value;
                        break;
                    case 6:
                        group6 = value;
                        break;
                    case 7:
                        group7 = value;
                        break;
                }
            }
        }

        public static Result Bake(
            MeshData meshData,
            MassSurfaceFeatureSettings settings,
            int requestedResolution = DefaultResolution,
            GeneratedMassFeatureAtlasRequest atlasRequest =
                GeneratedMassFeatureAtlasRequest.FeatureAtlas0 |
                GeneratedMassFeatureAtlasRequest.FeatureAtlas1)
        {
            if (meshData == null ||
                meshData.VertexCount < 3 ||
                meshData.Triangles.Count < 3 ||
                atlasRequest == GeneratedMassFeatureAtlasRequest.None)
            {
                return null;
            }

            bool buildAtlas1 =
                (atlasRequest & GeneratedMassFeatureAtlasRequest.FeatureAtlas1) != 0;

            // FeatureAtlas1 stores coordinates/modulation that are meaningful only
            // against the same boundary layout as FeatureAtlas0. A request for
            // Atlas1 therefore implies Atlas0 rather than allowing an orphaned
            // coordinate atlas.
            bool buildAtlas0 =
                (atlasRequest & GeneratedMassFeatureAtlasRequest.FeatureAtlas0) != 0 ||
                buildAtlas1;

            int resolution = SanitizeResolution(requestedResolution);
            Bounds bounds = CalculateBounds(meshData.Vertices);
            MassSurfaceFeatureGraph graph = MassSurfaceFeatureGraph.Build(
                meshData.Vertices,
                meshData.Triangles,
                bounds);

            if (graph == null || graph.Patches.Count == 0)
            {
                return null;
            }

            List<PatchChart> charts = BuildPatchCharts(graph, resolution);
            if (charts.Count == 0)
            {
                return null;
            }

            List<Vector2> featureAtlasUV = BuildFeatureAtlasUV(
                meshData,
                graph,
                charts,
                resolution);

            Texture2D atlas0 = buildAtlas0
                ? CreateAtlasTexture(
                    resolution,
                    "GeneratedMass_FeatureAtlas0_Temporary")
                : null;
            Texture2D atlas1 = buildAtlas1
                ? CreateAtlasTexture(
                    resolution,
                    "GeneratedMass_FeatureAtlas1_Temporary")
                : null;
            Color32[] pixels0 = buildAtlas0
                ? new Color32[resolution * resolution]
                : null;
            Color32[] pixels1 = buildAtlas1
                ? new Color32[resolution * resolution]
                : null;
            float[] dominantComposite = new float[resolution * resolution];

            InitializePixels(pixels0);
            InitializePixels(pixels1);

            BakePatchDistanceFields(
                graph,
                charts,
                settings,
                bounds,
                pixels0,
                pixels1,
                dominantComposite,
                resolution);

            if (atlas0 != null && pixels0 != null)
            {
                atlas0.SetPixels32(pixels0);
                atlas0.Apply(false, true);
            }

            if (atlas1 != null && pixels1 != null)
            {
                atlas1.SetPixels32(pixels1);
                atlas1.Apply(false, true);
            }

            return new Result(atlas0, atlas1, featureAtlasUV);
        }

        private static void InitializePixels(Color32[] pixels)
        {
            if (pixels == null)
            {
                return;
            }

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
            }
        }

        public static int SanitizeResolution(int requestedResolution)
        {
            int resolution = Mathf.Clamp(
                requestedResolution,
                MinimumResolution,
                MaximumResolution);

            if (resolution <= 128)
            {
                return 128;
            }

            if (resolution <= 256)
            {
                return 256;
            }

            return 512;
        }

        private static Texture2D CreateAtlasTexture(int resolution, string textureName)
        {
            return new Texture2D(
                resolution,
                resolution,
                TextureFormat.RGBA32,
                false,
                true)
            {
                name = textureName,
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 0
            };
        }

        private static Bounds CalculateBounds(IReadOnlyList<Vector3> vertices)
        {
            Bounds bounds = new Bounds(vertices[0], Vector3.zero);
            for (int i = 1; i < vertices.Count; i++)
            {
                bounds.Encapsulate(vertices[i]);
            }

            return bounds;
        }

        private static List<PatchChart> BuildPatchCharts(
            MassSurfaceFeatureGraph graph,
            int resolution)
        {
            List<PatchChart> charts = new(graph.Patches.Count);
            for (int i = 0; i < graph.Patches.Count; i++)
            {
                PatchChart chart = BuildPatchChart(graph, graph.Patches[i]);
                if (chart.WidthWorld <= 0.000001f || chart.HeightWorld <= 0.000001f)
                {
                    continue;
                }

                charts.Add(chart);
            }

            if (charts.Count == 0)
            {
                return charts;
            }

            PackPatchCharts(charts, resolution);
            ResolveBoundarySegments(graph, charts);
            return charts;
        }

        private static PatchChart BuildPatchChart(
            MassSurfaceFeatureGraph graph,
            MassSurfaceFeatureGraph.Patch patch)
        {
            Vector3 normal =
                patch.Normal.sqrMagnitude > 0.000001f
                    ? patch.Normal.normalized
                    : Vector3.up;
            Vector3 reference =
                Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.82f
                    ? Vector3.right
                    : Vector3.up;
            Vector3 axisU = Vector3.Cross(reference, normal);
            if (axisU.sqrMagnitude <= 0.000001f)
            {
                axisU = Vector3.Cross(Vector3.forward, normal);
            }

            axisU.Normalize();
            Vector3 axisV = Vector3.Cross(normal, axisU).normalized;
            Vector3 origin = patch.Center;

            Vector2 localMin = new(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 localMax = new(float.NegativeInfinity, float.NegativeInfinity);

            for (int i = 0; i < patch.TriangleIndices.Count; i++)
            {
                MassSurfaceFeatureGraph.Triangle triangle = graph.Triangles[patch.TriangleIndices[i]];
                EncapsulateProjectedPoint(triangle.PositionA, origin, axisU, axisV, ref localMin, ref localMax);
                EncapsulateProjectedPoint(triangle.PositionB, origin, axisU, axisV, ref localMin, ref localMax);
                EncapsulateProjectedPoint(triangle.PositionC, origin, axisU, axisV, ref localMin, ref localMax);
            }

            if (float.IsNaN(localMin.x) || float.IsNaN(localMin.y) || float.IsInfinity(localMin.x) || float.IsInfinity(localMin.y))
            {
                localMin = Vector2.zero;
                localMax = Vector2.one * 0.01f;
            }

            float width = Mathf.Max(0.0001f, localMax.x - localMin.x);
            float height = Mathf.Max(0.0001f, localMax.y - localMin.y);
            return new PatchChart
            {
                Patch = patch,
                Origin = origin,
                AxisU = axisU,
                AxisV = axisV,
                LocalMin = localMin,
                LocalMax = localMax,
                WidthWorld = width,
                HeightWorld = height
            };
        }

        private static void EncapsulateProjectedPoint(
            Vector3 position,
            Vector3 origin,
            Vector3 axisU,
            Vector3 axisV,
            ref Vector2 localMin,
            ref Vector2 localMax)
        {
            Vector2 local = Project(position, origin, axisU, axisV);
            localMin = Vector2.Min(localMin, local);
            localMax = Vector2.Max(localMax, local);
        }

        private static Vector2 Project(
            Vector3 position,
            Vector3 origin,
            Vector3 axisU,
            Vector3 axisV)
        {
            Vector3 relative = position - origin;
            return new Vector2(Vector3.Dot(relative, axisU), Vector3.Dot(relative, axisV));
        }

        private static void PackPatchCharts(List<PatchChart> charts, int resolution)
        {
            charts.Sort((a, b) =>
            {
                float aMax = Mathf.Max(a.WidthWorld, a.HeightWorld);
                float bMax = Mathf.Max(b.WidthWorld, b.HeightWorld);
                int maxCompare = bMax.CompareTo(aMax);
                if (maxCompare != 0)
                {
                    return maxCompare;
                }

                int areaCompare = b.Patch.Area.CompareTo(a.Patch.Area);
                if (areaCompare != 0)
                {
                    return areaCompare;
                }

                return a.Patch.Index.CompareTo(b.Patch.Index);
            });

            float totalArea = 0f;
            for (int i = 0; i < charts.Count; i++)
            {
                totalArea += Mathf.Max(0.000001f, charts[i].WidthWorld * charts[i].HeightWorld);
            }

            float usablePixels = Mathf.Max(1f, resolution - ChartPaddingPixels * 2f);
            float initialScale = Mathf.Sqrt((usablePixels * usablePixels * PatchPackUsage) / Mathf.Max(0.000001f, totalArea));
            initialScale = Mathf.Min(initialScale, resolution * 0.72f / ResolveLargestChartExtent(charts));

            float scale = Mathf.Max(1f, initialScale);
            for (int attempt = 0; attempt < 16; attempt++)
            {
                if (TryPackPatchCharts(charts, resolution, scale))
                {
                    return;
                }

                scale *= 0.86f;
            }

            if (!TryPackPatchCharts(charts, resolution, Mathf.Max(1f, scale)))
            {
                PackPatchChartsAsGrid(charts, resolution);
            }
        }

        private static void PackPatchChartsAsGrid(List<PatchChart> charts, int resolution)
        {
            int grid = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(charts.Count)));
            int stride = Mathf.Max(1, resolution / grid);
            int padding = stride >= 12 ? 3 : stride >= 8 ? 2 : 1;
            int interior = Mathf.Max(2, stride - padding * 2);

            for (int i = 0; i < charts.Count; i++)
            {
                PatchChart chart = charts[i];
                int packedX = i % grid;
                int packedY = i / grid;
                chart.X = Mathf.Clamp(packedX * stride + padding, 0, resolution - interior);
                chart.Y = Mathf.Clamp(packedY * stride + padding, 0, resolution - interior);
                chart.WidthPixels = interior;
                chart.HeightPixels = interior;
                chart.PixelsPerWorld = Mathf.Min(
                    interior / Mathf.Max(0.0001f, chart.WidthWorld),
                    interior / Mathf.Max(0.0001f, chart.HeightWorld));
            }
        }

        private static float ResolveLargestChartExtent(List<PatchChart> charts)
        {
            float largest = 0.0001f;
            for (int i = 0; i < charts.Count; i++)
            {
                largest = Mathf.Max(largest, Mathf.Max(charts[i].WidthWorld, charts[i].HeightWorld));
            }

            return largest;
        }

        private static bool TryPackPatchCharts(
            List<PatchChart> charts,
            int resolution,
            float pixelsPerWorld)
        {
            int cursorX = ChartPaddingPixels;
            int cursorY = ChartPaddingPixels;
            int shelfHeight = 0;
            bool allFit = true;

            for (int i = 0; i < charts.Count; i++)
            {
                PatchChart chart = charts[i];
                int interiorWidth = Mathf.Max(
                    MinimumChartInteriorPixels,
                    Mathf.CeilToInt(chart.WidthWorld * pixelsPerWorld));
                int interiorHeight = Mathf.Max(
                    MinimumChartInteriorPixels,
                    Mathf.CeilToInt(chart.HeightWorld * pixelsPerWorld));
                int packedWidth = interiorWidth + ChartPaddingPixels * 2;
                int packedHeight = interiorHeight + ChartPaddingPixels * 2;

                if (packedWidth > resolution || packedHeight > resolution)
                {
                    allFit = false;
                    break;
                }

                if (cursorX + packedWidth > resolution)
                {
                    cursorX = ChartPaddingPixels;
                    cursorY += shelfHeight;
                    shelfHeight = 0;
                }

                if (cursorY + packedHeight > resolution)
                {
                    allFit = false;
                    break;
                }

                chart.X = cursorX + ChartPaddingPixels;
                chart.Y = cursorY + ChartPaddingPixels;
                chart.WidthPixels = interiorWidth;
                chart.HeightPixels = interiorHeight;
                chart.PixelsPerWorld = Mathf.Min(
                    interiorWidth / Mathf.Max(0.0001f, chart.WidthWorld),
                    interiorHeight / Mathf.Max(0.0001f, chart.HeightWorld));

                cursorX += packedWidth;
                shelfHeight = Mathf.Max(shelfHeight, packedHeight);
            }

            return allFit;
        }

        private static void ResolveBoundarySegments(
            MassSurfaceFeatureGraph graph,
            List<PatchChart> charts)
        {
            Dictionary<int, PatchChart> byPatch = new();
            for (int i = 0; i < charts.Count; i++)
            {
                byPatch[charts[i].Patch.Index] = charts[i];
            }

            for (int i = 0; i < graph.Boundaries.Count; i++)
            {
                MassSurfaceFeatureGraph.Boundary boundary = graph.Boundaries[i];
                if (boundary.Score <= 0.0001f ||
                    boundary.Kind == MassSurfaceBoundaryKind.FlatInternal ||
                    boundary.Kind == MassSurfaceBoundaryKind.OpenBorder ||
                    boundary.Kind == MassSurfaceBoundaryKind.Ambiguous)
                {
                    continue;
                }

                AddBoundarySegmentToPatch(graph, boundary, boundary.PatchA, byPatch);
                AddBoundarySegmentToPatch(graph, boundary, boundary.PatchB, byPatch);
            }
        }

        private static void AddBoundarySegmentToPatch(
            MassSurfaceFeatureGraph graph,
            MassSurfaceFeatureGraph.Boundary boundary,
            int patchIndex,
            Dictionary<int, PatchChart> byPatch)
        {
            if (patchIndex < 0 || !byPatch.TryGetValue(patchIndex, out PatchChart chart))
            {
                return;
            }

            float chainSalience = boundary.Score;
            float chainLength = boundary.Length;
            if (boundary.ChainIndex >= 0 &&
                boundary.ChainIndex < graph.BoundaryChains.Count)
            {
                MassSurfaceFeatureGraph.BoundaryChain chain =
                    graph.BoundaryChains[boundary.ChainIndex];
                chainSalience = chain.Salience;
                chainLength = chain.Length;
            }

            float sideSign = patchIndex == boundary.PatchA
                ? -1f
                : patchIndex == boundary.PatchB
                    ? 1f
                    : 0f;

            BoundarySegment segment = new(
                Project(boundary.Start, chart.Origin, chart.AxisU, chart.AxisV),
                Project(boundary.End, chart.Origin, chart.AxisU, chart.AxisV),
                boundary.Score,
                chainSalience,
                boundary.Index,
                boundary.ChainIndex,
                boundary.ChainDistanceAtStart,
                boundary.ChainDistanceAtEnd,
                chainLength,
                sideSign,
                boundary.Length);

            if (boundary.Kind == MassSurfaceBoundaryKind.ConvexRidge)
            {
                chart.ConvexSegments.Add(segment);
            }
            else if (boundary.Kind == MassSurfaceBoundaryKind.ConcaveCrease)
            {
                chart.ConcaveSegments.Add(segment);
            }
        }

        private static List<Vector2> BuildFeatureAtlasUV(
            MeshData meshData,
            MassSurfaceFeatureGraph graph,
            List<PatchChart> charts,
            int resolution)
        {
            List<Vector2> featureAtlasUV = new(meshData.VertexCount);
            for (int i = 0; i < meshData.VertexCount; i++)
            {
                featureAtlasUV.Add(Vector2.zero);
            }

            Dictionary<int, PatchChart> byPatch = new();
            for (int i = 0; i < charts.Count; i++)
            {
                byPatch[charts[i].Patch.Index] = charts[i];
            }

            for (int triangleIndex = 0; triangleIndex < graph.Triangles.Count; triangleIndex++)
            {
                MassSurfaceFeatureGraph.Triangle triangle = graph.Triangles[triangleIndex];
                if (!byPatch.TryGetValue(triangle.PatchIndex, out PatchChart chart))
                {
                    continue;
                }

                featureAtlasUV[triangle.VertexA] = LocalToAtlasUV(
                    Project(triangle.PositionA, chart.Origin, chart.AxisU, chart.AxisV),
                    chart,
                    resolution);
                featureAtlasUV[triangle.VertexB] = LocalToAtlasUV(
                    Project(triangle.PositionB, chart.Origin, chart.AxisU, chart.AxisV),
                    chart,
                    resolution);
                featureAtlasUV[triangle.VertexC] = LocalToAtlasUV(
                    Project(triangle.PositionC, chart.Origin, chart.AxisU, chart.AxisV),
                    chart,
                    resolution);
            }

            return featureAtlasUV;
        }

        private static Vector2 LocalToAtlasUV(
            Vector2 local,
            PatchChart chart,
            int resolution)
        {
            Vector2 local01 = new(
                Mathf.InverseLerp(chart.LocalMin.x, chart.LocalMax.x, local.x),
                Mathf.InverseLerp(chart.LocalMin.y, chart.LocalMax.y, local.y));
            float pixelX = chart.X + Mathf.Clamp01(local01.x) * Mathf.Max(1, chart.WidthPixels - 1) + 0.5f;
            float pixelY = chart.Y + Mathf.Clamp01(local01.y) * Mathf.Max(1, chart.HeightPixels - 1) + 0.5f;
            float invResolution = 1f / Mathf.Max(1, resolution);
            return new Vector2(pixelX * invResolution, pixelY * invResolution);
        }

        private static Vector2 AtlasPixelToLocal(Vector2 pixel, PatchChart chart)
        {
            float u =
                chart.WidthPixels <= 1
                    ? 0.5f
                    : Mathf.Clamp01((pixel.x - chart.X - 0.5f) / Mathf.Max(1, chart.WidthPixels - 1));
            float v =
                chart.HeightPixels <= 1
                    ? 0.5f
                    : Mathf.Clamp01((pixel.y - chart.Y - 0.5f) / Mathf.Max(1, chart.HeightPixels - 1));

            return new Vector2(
                Mathf.Lerp(chart.LocalMin.x, chart.LocalMax.x, u),
                Mathf.Lerp(chart.LocalMin.y, chart.LocalMax.y, v));
        }

        private static Vector2 AtlasPixelToLocalUnclamped(Vector2 pixel, PatchChart chart)
        {
            float u =
                chart.WidthPixels <= 1
                    ? 0.5f
                    : (pixel.x - chart.X - 0.5f) / Mathf.Max(1, chart.WidthPixels - 1);
            float v =
                chart.HeightPixels <= 1
                    ? 0.5f
                    : (pixel.y - chart.Y - 0.5f) / Mathf.Max(1, chart.HeightPixels - 1);

            return new Vector2(
                Mathf.LerpUnclamped(chart.LocalMin.x, chart.LocalMax.x, u),
                Mathf.LerpUnclamped(chart.LocalMin.y, chart.LocalMax.y, v));
        }

        private static void BakePatchDistanceFields(
            MassSurfaceFeatureGraph graph,
            List<PatchChart> charts,
            MassSurfaceFeatureSettings settings,
            Bounds bounds,
            Color32[] pixels0,
            Color32[] pixels1,
            float[] dominantComposite,
            int resolution)
        {
            float scale = Mathf.Max(0.0001f, Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)));
            float edgeWearWidth = scale * 0.018f * Mathf.Max(0.05f, settings.EdgeWearWidth);
            float creaseWidth = scale * 0.010f * Mathf.Max(0.05f, settings.CreaseWidth);
            float edgeAmount01 = Mathf.Clamp01(settings.EdgeWearAmount);
            float creaseAmount01 = Mathf.Clamp01(settings.CreaseAmount);
            float edgeCoverage01 = Mathf.InverseLerp(0.1f, 2f, settings.EdgeWearCoverage);
            float creaseCoverage01 = Mathf.InverseLerp(0.1f, 2f, settings.CreaseBranching);
            bool bakeConvex = edgeAmount01 > 0.001f;
            bool bakeConcave = creaseAmount01 > 0.001f;

            for (int i = 0; i < charts.Count; i++)
            {
                PatchChart chart = charts[i];
                if (chart.WidthPixels <= 0 || chart.HeightPixels <= 0)
                {
                    continue;
                }

                BakePatchCoverageAndFeatures(
                    graph,
                    chart,
                    bakeConvex,
                    edgeWearWidth,
                    edgeAmount01,
                    edgeCoverage01,
                    bakeConcave,
                    creaseWidth,
                    creaseAmount01,
                    creaseCoverage01,
                    settings.EdgeWearSoftness,
                    settings.SurfaceSeed,
                    pixels0,
                    pixels1,
                    dominantComposite,
                    resolution);
            }
        }

        private static void BakePatchCoverageAndFeatures(
            MassSurfaceFeatureGraph graph,
            PatchChart chart,
            bool bakeConvex,
            float edgeWearWidth,
            float edgeAmount01,
            float edgeCoverage01,
            bool bakeConcave,
            float creaseWidth,
            float creaseAmount01,
            float creaseCoverage01,
            float edgeSoftness,
            int surfaceSeed,
            Color32[] pixels0,
            Color32[] pixels1,
            float[] dominantComposite,
            int resolution)
        {
            if ((!bakeConvex || chart.ConvexSegments.Count == 0) &&
                (!bakeConcave || chart.ConcaveSegments.Count == 0))
            {
                return;
            }

            List<ProjectedTriangle> projectedTriangles = new(chart.Patch.TriangleIndices.Count);
            for (int i = 0; i < chart.Patch.TriangleIndices.Count; i++)
            {
                MassSurfaceFeatureGraph.Triangle triangle = graph.Triangles[chart.Patch.TriangleIndices[i]];
                projectedTriangles.Add(
                    new ProjectedTriangle(
                        Project(triangle.PositionA, chart.Origin, chart.AxisU, chart.AxisV),
                        Project(triangle.PositionB, chart.Origin, chart.AxisU, chart.AxisV),
                        Project(triangle.PositionC, chart.Origin, chart.AxisU, chart.AxisV)));
            }

            // Bake the valid chart interior plus its semantic gutter. The gutter
            // is not a decorative dilation pass: it carries the same ridge/crease
            // field just outside the surface polygon so bilinear filtering at a
            // patch boundary cannot pull black into the exact ridge core.
            int minX = Mathf.Clamp(chart.X - ChartPaddingPixels, 0, resolution - 1);
            int minY = Mathf.Clamp(chart.Y - ChartPaddingPixels, 0, resolution - 1);
            int maxX = Mathf.Clamp(chart.X + chart.WidthPixels - 1 + ChartPaddingPixels, 0, resolution - 1);
            int maxY = Mathf.Clamp(chart.Y + chart.HeightPixels - 1 + ChartPaddingPixels, 0, resolution - 1);
            float texelWorld = 1f / Mathf.Max(0.0001f, chart.PixelsPerWorld);
            int sampleAxis = ResolveSupersampleAxis(resolution);
            int sampleCount = sampleAxis * sampleAxis;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    BoundaryFieldAccumulator convexAccumulator = new(sampleCount);
                    BoundaryFieldAccumulator concaveAccumulator = new(sampleCount);

                    for (int sampleY = 0; sampleY < sampleAxis; sampleY++)
                    {
                        for (int sampleX = 0; sampleX < sampleAxis; sampleX++)
                        {
                            Vector2 pixel = new(
                                x + (sampleX + 0.5f) / sampleAxis,
                                y + (sampleY + 0.5f) / sampleAxis);
                            Vector2 local = AtlasPixelToLocalUnclamped(pixel, chart);
                            bool insideSurface = IsPointInsideAnyTriangle(local, projectedTriangles);

                            BoundaryFieldSample convexSample = default;
                            if (bakeConvex && chart.ConvexSegments.Count > 0)
                            {
                                convexSample = ResolveBoundaryDataFieldSample(
                                    local,
                                    chart.ConvexSegments,
                                    edgeWearWidth,
                                    edgeSoftness,
                                    texelWorld,
                                    edgeAmount01,
                                    edgeCoverage01,
                                    false,
                                    surfaceSeed,
                                    131);
                            }

                            BoundaryFieldSample concaveSample = default;
                            if (bakeConcave && chart.ConcaveSegments.Count > 0)
                            {
                                concaveSample = ResolveBoundaryDataFieldSample(
                                    local,
                                    chart.ConcaveSegments,
                                    creaseWidth,
                                    0.82f,
                                    texelWorld,
                                    creaseAmount01,
                                    creaseCoverage01,
                                    true,
                                    surfaceSeed,
                                    353);
                            }

                            if (!insideSurface)
                            {
                                // Outside-polygon samples are only allowed to act as
                                // chart gutters close to the feature core. This is
                                // evaluated per sub-sample so low-resolution atlases
                                // retain seam-safe gutters without painting empty
                                // chart space as valid boundary data.
                                convexSample = Mathf.Max(convexSample.Proximity, convexSample.CoreProximity) >= 0.20f
                                    ? convexSample
                                    : default;
                                concaveSample = Mathf.Max(concaveSample.Proximity, concaveSample.CoreProximity) >= 0.20f
                                    ? concaveSample
                                    : default;
                            }

                            convexAccumulator.Add(convexSample);
                            concaveAccumulator.Add(concaveSample);
                        }
                    }

                    BoundaryFieldSample convex = convexAccumulator.Resolve();
                    BoundaryFieldSample concave = concaveAccumulator.Resolve();

                    if (convex.Proximity <= 0.001f &&
                        concave.Proximity <= 0.001f)
                    {
                        continue;
                    }

                    int pixelIndex = y * resolution + x;

                    BoundaryFieldSample dominant =
                        convex.Composite >= concave.Composite
                            ? convex
                            : concave;

                    // FeatureAtlas0 — Boundary Structure Atlas:
                    // R = convex boundary proximity
                    // G = concave boundary proximity
                    // B = dominant boundary structural salience
                    // A = dominant boundary stable identity/seed
                    //
                    // FeatureAtlas1 — Boundary Coordinate/Modulation Atlas:
                    // R = dominant boundary along-chain coordinate / phase
                    // G = dominant boundary cross-boundary coordinate
                    // B = dominant boundary coarse local modulation
                    // A = dominant boundary fine local modulation
                    if (pixels0 != null && convex.Proximity > 0.001f)
                    {
                        pixels0[pixelIndex].r = MaxByte(pixels0[pixelIndex].r, convex.Proximity);
                    }

                    if (pixels0 != null && concave.Proximity > 0.001f)
                    {
                        pixels0[pixelIndex].g = MaxByte(pixels0[pixelIndex].g, concave.Proximity);
                    }

                    // Proximity channels are masks and can accumulate by maximum.
                    // Dominant identity/salience/along facts are copied from the
                    // strongest resolved boundary; Atlas1.G/B/A have already been
                    // side/coverage-stabilized inside the accumulator.
                    if (dominant.Proximity > 0.001f &&
                        dominant.Composite >= dominantComposite[pixelIndex])
                    {
                        dominantComposite[pixelIndex] = dominant.Composite;

                        if (pixels0 != null)
                        {
                            pixels0[pixelIndex].b = ToByte(dominant.Salience);
                            pixels0[pixelIndex].a = ToByte(dominant.Identity);
                        }

                        if (pixels1 != null)
                        {
                            pixels1[pixelIndex].r = ToByte(dominant.Along);
                            pixels1[pixelIndex].g = ToByte(dominant.Cross);
                            pixels1[pixelIndex].b = ToByte(dominant.CoarseModulation);
                            pixels1[pixelIndex].a = ToByte(dominant.FineModulation);
                        }
                    }
                }
            }
        }

        private static int ResolveSupersampleAxis(int resolution)
        {
            if (resolution <= 128)
            {
                return 4;
            }

            if (resolution <= 256)
            {
                return 2;
            }

            return 1;
        }

        private static float Hash01(int a, int b, int c)
        {
            unchecked
            {
                uint hash = 2166136261u;
                hash = (hash ^ (uint)a) * 16777619u;
                hash = (hash ^ (uint)b) * 16777619u;
                hash = (hash ^ (uint)c) * 16777619u;
                hash ^= hash >> 13;
                hash *= 1274126177u;
                hash ^= hash >> 16;
                return (hash & 0x00FFFFFFu) / 16777215f;
            }
        }

        private static byte ToByte(float value01)
        {
            return (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(value01) * 255f), 0, 255);
        }

        private static byte MaxByte(byte existing, float value01)
        {
            byte value = ToByte(value01);
            return value > existing ? value : existing;
        }

        private static bool IsPointInsideAnyTriangle(
            Vector2 point,
            List<ProjectedTriangle> triangles)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                if (IsPointInsideTriangle(point, triangles[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPointInsideTriangle(Vector2 point, ProjectedTriangle triangle)
        {
            const float epsilon = 0.00001f;
            float d1 = SignedArea(point, triangle.A, triangle.B);
            float d2 = SignedArea(point, triangle.B, triangle.C);
            float d3 = SignedArea(point, triangle.C, triangle.A);
            bool hasNegative = d1 < -epsilon || d2 < -epsilon || d3 < -epsilon;
            bool hasPositive = d1 > epsilon || d2 > epsilon || d3 > epsilon;
            return !(hasNegative && hasPositive);
        }

        private static float SignedArea(Vector2 point, Vector2 a, Vector2 b)
        {
            return (point.x - b.x) * (a.y - b.y) - (a.x - b.x) * (point.y - b.y);
        }

        private static BoundaryFieldSample ResolveBoundaryDataFieldSample(
            Vector2 local,
            List<BoundarySegment> segments,
            float widthWorld,
            float softness,
            float texelWorld,
            float amount01,
            float coverage01,
            bool sharper,
            int surfaceSeed,
            int kindSalt)
        {
            BoundaryFieldSample bestSample = default;
            float bestComposite = 0f;
            float featureWidth = Mathf.Max(0.0001f, widthWorld);
            float softness01 = Mathf.Clamp01(softness);

            // Proximity is an object/world-space distance field. Atlas resolution
            // may reduce fidelity, but it must not redefine the artist-authored
            // feature width. Texel size is therefore used only as a tiny bounded
            // raster/AA allowance, never as a minimum physical edge-wear width.
            float core = featureWidth * Mathf.Lerp(
                sharper ? 0.024f : 0.030f,
                sharper ? 0.008f : 0.014f,
                softness01);
            float outer = Mathf.Max(
                core + featureWidth * 0.025f,
                featureWidth * Mathf.Lerp(
                    sharper ? 0.56f : 0.68f,
                    sharper ? 0.88f : 1.02f,
                    softness01));
            float rasterAllowance = Mathf.Min(texelWorld * 0.35f, featureWidth * 0.20f);
            float coreRasterAllowance = Mathf.Min(texelWorld * 0.42f, featureWidth * 0.16f);
            float corePreservationOuter = core + coreRasterAllowance;
            float falloffOuter = outer + rasterAllowance;

            for (int i = 0; i < segments.Count; i++)
            {
                BoundarySegment segment = segments[i];
                float t;
                float distance = DistanceToSegment(local, segment.Start, segment.End, out t);
                if (distance > falloffOuter * GutterDistanceMultiplier)
                {
                    continue;
                }

                float proximity;
                if (distance <= core)
                {
                    proximity = 1f;
                }
                else
                {
                    float distance01 = Mathf.InverseLerp(core, falloffOuter, distance);
                    float falloff = 1f - Mathf.SmoothStep(0f, 1f, distance01);
                    proximity = Mathf.Pow(
                        Mathf.Clamp01(falloff),
                        sharper ? 1.45f : 1.28f);
                }

                // The authored core is intentionally narrow, but at Compact/128
                // it can become a sub-texel peak inside a much wider shoulder.
                // Preserve only the semantic ridge core with a bounded texel
                // allowance; this does not expand the broad wear width.
                float coreProximity = distance <= corePreservationOuter ? 1f : 0f;

                if (proximity <= 0.001f && coreProximity <= 0.001f)
                {
                    continue;
                }

                float scoreThreshold = Mathf.Lerp(sharper ? 0.72f : 0.78f, 0.025f, coverage01);
                if (segment.Score < scoreThreshold)
                {
                    continue;
                }

                float amount = Mathf.Clamp01(amount01);
                float chainU = Mathf.Repeat(
                    Mathf.Lerp(
                        segment.ChainDistanceAtStart,
                        segment.ChainDistanceAtEnd,
                        t) /
                    Mathf.Max(0.0001f, segment.ChainLengthWorld),
                    1f);
                float crossDistance01 = distance <= core
                    ? 0f
                    : Mathf.InverseLerp(core, outer, distance);
                float cross = Mathf.Clamp01(0.5f + segment.SideSign * crossDistance01 * 0.5f);
                float identity = ResolveBoundaryIdentity(surfaceSeed, segment, kindSalt);
                float salience = Mathf.Clamp01(segment.Salience * amount);

                float coarse = ResolveBoundaryLocalModulation(
                    surfaceSeed,
                    segment,
                    chainU,
                    identity,
                    kindSalt,
                    0.85f,
                    0.0f);
                float fine = ResolveBoundaryLocalModulation(
                    surfaceSeed,
                    segment,
                    chainU,
                    identity,
                    kindSalt,
                    3.15f,
                    41.0f);

                BoundaryFieldSample sample = new(
                    proximity,
                    coreProximity,
                    salience,
                    identity,
                    chainU,
                    cross,
                    coarse,
                    fine,
                    ResolveBoundaryGroupKey(segment));
                float composite = sample.Composite;
                if (composite > bestComposite)
                {
                    bestComposite = composite;
                    bestSample = sample;
                }
            }

            return bestSample;
        }

        private static int ResolveBoundaryGroupKey(BoundarySegment segment)
        {
            // Use the concrete boundary segment rather than the larger chain for
            // low-resolution field resolution. Chain identity is still used by
            // Macro/identity, but the atlas texel needs one local boundary-side
            // neighborhood so corners and adjacent ridges do not average their
            // Cross/modulation fields into an invalid hybrid.
            int sideKey = segment.SideSign < 0f ? 1 : segment.SideSign > 0f ? 2 : 0;
            return segment.BoundaryIndex * 4 + sideKey;
        }

        private static float ResolveBoundaryIdentity(
            int surfaceSeed,
            BoundarySegment segment,
            int kindSalt)
        {
            int stableIndex = segment.ChainIndex >= 0
                ? segment.ChainIndex
                : segment.BoundaryIndex;
            float seedOffset = Hash01(surfaceSeed + kindSalt, 101, 17);
            float raw = Mathf.Repeat(seedOffset + stableIndex * 0.61803398875f, 1f);

            // Use a golden-ratio stride so multiple chains on the same mass occupy
            // a broad stable identity range instead of clustering. This remains a
            // generic boundary identity/seed field; features decide how to map it.
            return Mathf.Clamp01(0.04f + raw * 0.96f);
        }

        private static float ResolveBoundaryLocalModulation(
            int surfaceSeed,
            BoundarySegment segment,
            float chainU,
            float identity,
            int kindSalt,
            float frequency,
            float offset)
        {
            int stableIndex = segment.ChainIndex >= 0
                ? segment.ChainIndex
                : segment.BoundaryIndex;
            float seedX =
                surfaceSeed * 0.00137f +
                kindSalt * 0.011f +
                stableIndex * 0.173f +
                identity * 5.31f +
                offset;
            float seedY =
                19.37f +
                kindSalt * 0.071f +
                identity * 23.0f +
                offset * 0.37f;
            return Mathf.PerlinNoise(seedX + chainU * frequency, seedY);
        }

        private static float DistanceToSegment(
            Vector2 point,
            Vector2 start,
            Vector2 end,
            out float t)
        {
            Vector2 segment = end - start;
            float lengthSqr = segment.sqrMagnitude;
            if (lengthSqr <= 0.000001f)
            {
                t = 0f;
                return Vector2.Distance(point, start);
            }

            t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSqr);
            Vector2 closest = start + segment * t;
            return Vector2.Distance(point, closest);
        }
    }
}
