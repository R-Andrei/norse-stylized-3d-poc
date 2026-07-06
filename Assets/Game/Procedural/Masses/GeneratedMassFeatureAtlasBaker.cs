using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    /// <summary>
    /// Builds the generated-mass surface-chart feature atlas used by the main
    /// material path. Patch 14C.5 keeps the atlas semantic: surface patches,
    /// boundary proximity, and boundary weight are baked as clean data, not
    /// final decorative paint. Raw channel diagnostics make the field contract
    /// inspectable before any final material response is built.
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
                int boundaryIndex,
                int chainIndex,
                float lengthWorld)
            {
                Start = start;
                End = end;
                Score = Mathf.Clamp01(score);
                BoundaryIndex = boundaryIndex;
                ChainIndex = chainIndex;
                LengthWorld = Mathf.Max(0.0001f, lengthWorld);
            }

            public Vector2 Start { get; }
            public Vector2 End { get; }
            public float Score { get; }
            public int BoundaryIndex { get; }
            public int ChainIndex { get; }
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
            public BoundaryFieldSample(float proximity, float weight)
            {
                Proximity = Mathf.Clamp01(proximity);
                Weight = Mathf.Clamp01(weight);
            }

            public float Proximity { get; }
            public float Weight { get; }
            public float Composite => Proximity * Weight;
        }

        public static Result Bake(
            MeshData meshData,
            MassSurfaceFeatureSettings settings,
            int requestedResolution = DefaultResolution)
        {
            if (meshData == null ||
                meshData.VertexCount < 3 ||
                meshData.Triangles.Count < 3)
            {
                return null;
            }

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

            Texture2D atlas0 = CreateAtlasTexture(
                resolution,
                "GeneratedMass_FeatureAtlas0_Temporary");
            Texture2D atlas1 = CreateAtlasTexture(
                resolution,
                "GeneratedMass_FeatureAtlas1_Temporary");
            Color32[] pixels0 = new Color32[resolution * resolution];
            Color32[] pixels1 = new Color32[resolution * resolution];
            for (int i = 0; i < pixels0.Length; i++)
            {
                pixels0[i] = new Color32(0, 0, 0, 0);
                pixels1[i] = new Color32(0, 0, 0, 0);
            }

            BakePatchDistanceFields(
                graph,
                charts,
                settings,
                bounds,
                pixels0,
                pixels1,
                resolution);

            atlas0.SetPixels32(pixels0);
            atlas0.Apply(false, false);
            atlas1.SetPixels32(pixels1);
            atlas1.Apply(false, false);
            return new Result(atlas0, atlas1, featureAtlasUV);
        }

        private static int SanitizeResolution(int requestedResolution)
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

                AddBoundarySegmentToPatch(boundary, boundary.PatchA, byPatch);
                AddBoundarySegmentToPatch(boundary, boundary.PatchB, byPatch);
            }
        }

        private static void AddBoundarySegmentToPatch(
            MassSurfaceFeatureGraph.Boundary boundary,
            int patchIndex,
            Dictionary<int, PatchChart> byPatch)
        {
            if (patchIndex < 0 || !byPatch.TryGetValue(patchIndex, out PatchChart chart))
            {
                return;
            }

            BoundarySegment segment = new(
                Project(boundary.Start, chart.Origin, chart.AxisU, chart.AxisV),
                Project(boundary.End, chart.Origin, chart.AxisU, chart.AxisV),
                boundary.Score,
                boundary.Index,
                boundary.ChainIndex,
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

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 pixel = new(x + 0.5f, y + 0.5f);
                    Vector2 local = AtlasPixelToLocalUnclamped(pixel, chart);
                    bool insideSurface = IsPointInsideAnyTriangle(local, projectedTriangles);

                    BoundaryFieldSample convex = default;
                    if (bakeConvex && chart.ConvexSegments.Count > 0)
                    {
                        convex = ResolveSemanticDistanceFieldSample(
                            local,
                            chart.ConvexSegments,
                            edgeWearWidth,
                            edgeSoftness,
                            texelWorld,
                            edgeAmount01,
                            edgeCoverage01,
                            false);
                    }

                    BoundaryFieldSample concave = default;
                    if (bakeConcave && chart.ConcaveSegments.Count > 0)
                    {
                        concave = ResolveSemanticDistanceFieldSample(
                            local,
                            chart.ConcaveSegments,
                            creaseWidth,
                            0.82f,
                            texelWorld,
                            creaseAmount01,
                            creaseCoverage01,
                            true);
                    }

                    if (!insideSurface)
                    {
                        // Outside-polygon pixels are only allowed to act as chart
                        // gutters close to the feature core. This prevents black
                        // seam bleed without turning empty atlas space into a broad
                        // fake feature field.
                        convex = convex.Proximity >= 0.20f
                            ? convex
                            : default;
                        concave = concave.Proximity >= 0.20f
                            ? concave
                            : default;
                    }

                    if (convex.Proximity <= 0.001f &&
                        convex.Weight <= 0.001f &&
                        concave.Proximity <= 0.001f &&
                        concave.Weight <= 0.001f)
                    {
                        continue;
                    }

                    int pixelIndex = y * resolution + x;

                    // FeatureAtlas0 channel contract:
                    // R = convex ridge proximity
                    // G = convex ridge weight / importance
                    // B = concave crease proximity
                    // A = concave crease weight / importance
                    if (convex.Proximity > 0.001f)
                    {
                        pixels0[pixelIndex].r = MaxByte(pixels0[pixelIndex].r, convex.Proximity);
                        pixels0[pixelIndex].g = MaxByte(pixels0[pixelIndex].g, convex.Weight);

                        Color edgeIrregularity = ResolveEdgeWearIrregularitySample(
                            local,
                            chart.ConvexSegments,
                            edgeWearWidth,
                            texelWorld,
                            surfaceSeed,
                            convex.Proximity);

                        // FeatureAtlas1 channel contract:
                        // R = baked edge-wear amplitude variation
                        // G = baked edge-wear width/smear variation
                        // B = baked edge-wear continuity / chip-thinning variation
                        // A = reserved
                        pixels1[pixelIndex].r = MaxByte(pixels1[pixelIndex].r, edgeIrregularity.r);
                        pixels1[pixelIndex].g = MaxByte(pixels1[pixelIndex].g, edgeIrregularity.g);
                        pixels1[pixelIndex].b = MaxByte(pixels1[pixelIndex].b, edgeIrregularity.b);
                    }

                    if (concave.Proximity > 0.001f)
                    {
                        pixels0[pixelIndex].b = MaxByte(pixels0[pixelIndex].b, concave.Proximity);
                        pixels0[pixelIndex].a = MaxByte(pixels0[pixelIndex].a, concave.Weight);
                    }
                }
            }
        }

        private static Color ResolveEdgeWearIrregularitySample(
            Vector2 local,
            List<BoundarySegment> segments,
            float widthWorld,
            float texelWorld,
            int surfaceSeed,
            float proximity)
        {
            if (segments == null || segments.Count == 0)
            {
                return new Color(0.5f, 0.5f, 0.72f, 0f);
            }

            float searchDistance = Mathf.Max(texelWorld * 2f, widthWorld) * GutterDistanceMultiplier;
            BoundarySegment bestSegment = default;
            float bestT = 0f;
            float bestScore = -1f;

            for (int i = 0; i < segments.Count; i++)
            {
                BoundarySegment segment = segments[i];
                float t;
                float distance = DistanceToSegment(local, segment.Start, segment.End, out t);
                if (distance > searchDistance)
                {
                    continue;
                }

                float distanceScore = 1f - Mathf.Clamp01(distance / Mathf.Max(0.0001f, searchDistance));
                float score = distanceScore * Mathf.Lerp(0.55f, 1.0f, segment.Score);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestSegment = segment;
                    bestT = t;
                }
            }

            if (bestScore < 0f)
            {
                return new Color(0.5f, 0.5f, 0.72f, 0f);
            }

            float seedBase = surfaceSeed * 0.00137f + bestSegment.BoundaryIndex * 0.173f;
            float chainSeed = (bestSegment.ChainIndex + 17) * 0.319f;
            float lengthScale = Mathf.Clamp(bestSegment.LengthWorld / Mathf.Max(widthWorld, texelWorld * 3f), 1.0f, 24.0f);
            float along = bestT * Mathf.Lerp(1.35f, 5.25f, Mathf.InverseLerp(1.0f, 18.0f, lengthScale));

            float chainBias = Hash01(surfaceSeed, bestSegment.ChainIndex + 101, 19);
            float boundaryBias = Hash01(surfaceSeed, bestSegment.BoundaryIndex + 211, 29);
            float low = Mathf.PerlinNoise(seedBase + chainSeed + along * 0.72f, 11.37f + chainBias * 19.0f);
            float mid = Mathf.PerlinNoise(seedBase * 1.73f + along * 1.85f, 31.11f + boundaryBias * 23.0f);
            float fine = Mathf.PerlinNoise(seedBase * 2.41f + along * 4.10f, 71.91f + chainBias * 13.0f);

            // Atlas1 stores visual-irregularity fields, not final color.
            // Keep the values stable and ridge-aware so the shader can cheaply
            // vary opacity, apparent width, and local thinning without adding
            // more runtime topology work or extra user-facing controls.
            float amplitude = Mathf.Clamp01(
                0.08f +
                chainBias * 0.18f +
                boundaryBias * 0.15f +
                low * 0.42f +
                mid * 0.26f -
                fine * 0.08f);

            float width = Mathf.Clamp01(
                0.10f +
                low * 0.24f +
                mid * 0.50f +
                fine * 0.16f +
                (boundaryBias - 0.5f) * 0.16f);

            float continuity = Mathf.Clamp01(
                0.12f +
                low * 0.30f +
                mid * 0.16f +
                fine * 0.36f +
                proximity * 0.18f);

            return new Color(amplitude, width, continuity, 0f);
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

        private static byte MaxByte(byte existing, float value01)
        {
            byte value = (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(value01) * 255f), 0, 255);
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

        private static BoundaryFieldSample ResolveSemanticDistanceFieldSample(
            Vector2 local,
            List<BoundarySegment> segments,
            float widthWorld,
            float softness,
            float texelWorld,
            float amount01,
            float coverage01,
            bool sharper)
        {
            BoundaryFieldSample bestSample = default;
            float bestComposite = 0f;
            float safeWidth = Mathf.Max(texelWorld * 2f, widthWorld);
            float softness01 = Mathf.Clamp01(softness);

            // Keep the semantic field inspectable. Earlier debug builds let too
            // much of the band sit at full proximity, which made the atlas read
            // like a binary selected-edge strip. The ridge/crease core must
            // remain present, but it should be narrow enough that the raw
            // proximity channel exposes a visible distance gradient.
            float core = Mathf.Max(
                texelWorld * (sharper ? 0.95f : 1.10f),
                safeWidth * Mathf.Lerp(sharper ? 0.024f : 0.030f, sharper ? 0.008f : 0.014f, softness01));
            float outer = Mathf.Max(
                core + texelWorld * 2f,
                safeWidth * Mathf.Lerp(sharper ? 0.56f : 0.68f, sharper ? 0.88f : 1.02f, softness01));

            for (int i = 0; i < segments.Count; i++)
            {
                BoundarySegment segment = segments[i];
                float t;
                float distance = DistanceToSegment(local, segment.Start, segment.End, out t);
                if (distance > outer * GutterDistanceMultiplier)
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
                    float distance01 = Mathf.InverseLerp(core, outer, distance);
                    float falloff = 1f - Mathf.SmoothStep(0f, 1f, distance01);

                    // A mild power curve makes the raw proximity diagnostic show
                    // a clearer bright-core / mid-falloff / outer-fade structure
                    // without adding decorative breakup to the data channel.
                    proximity = Mathf.Pow(
                        Mathf.Clamp01(falloff),
                        sharper ? 1.45f : 1.28f);
                }

                if (proximity <= 0.001f)
                {
                    continue;
                }

                // Coverage is semantic boundary inclusion, not decorative
                // line fragmentation. Low coverage keeps only the strongest
                // ridge/crease chains; high coverage includes weaker boundaries.
                float scoreThreshold = Mathf.Lerp(sharper ? 0.72f : 0.78f, 0.025f, coverage01);
                if (segment.Score < scoreThreshold)
                {
                    continue;
                }

                // Keep proximity and boundary weight separate. Proximity is the
                // clean distance field; weight is the semantic importance/eligibility
                // for final material interpretation. Decorative breakup/noise is
                // deferred to the material response patch.
                float normalizedScore = Mathf.InverseLerp(scoreThreshold, 1f, segment.Score);
                float semanticWeight = Mathf.SmoothStep(0f, 1f, normalizedScore);
                semanticWeight = Mathf.Lerp(sharper ? 0.32f : 0.38f, 1f, semanticWeight);
                semanticWeight *= Mathf.Clamp01(amount01);

                BoundaryFieldSample sample = new(proximity, semanticWeight);
                float composite = sample.Composite;
                if (composite > bestComposite)
                {
                    bestComposite = composite;
                    bestSample = sample;
                }
            }

            return bestSample;
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
