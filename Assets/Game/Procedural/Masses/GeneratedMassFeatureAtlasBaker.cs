using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    /// <summary>
    /// Builds the generated-mass surface-chart feature atlas used by the main
    /// material path. Patch 14C.4 keeps the atlas semantic: surface patches,
    /// boundary proximity, and boundary weight are baked as clean data, not
    /// final decorative paint.
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
            public Result(Texture2D atlas0, List<Vector2> featureAtlasUV)
            {
                Atlas0 = atlas0;
                FeatureAtlasUV = featureAtlasUV;
            }

            public Texture2D Atlas0 { get; }
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
            public BoundarySegment(Vector2 start, Vector2 end, float score)
            {
                Start = start;
                End = end;
                Score = Mathf.Clamp01(score);
            }

            public Vector2 Start { get; }
            public Vector2 End { get; }
            public float Score { get; }
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

            Texture2D atlas0 = CreateAtlasTexture(resolution);
            Color32[] pixels = new Color32[resolution * resolution];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
            }

            BakePatchDistanceFields(
                graph,
                charts,
                settings,
                bounds,
                pixels,
                resolution);

            atlas0.SetPixels32(pixels);
            atlas0.Apply(false, false);
            return new Result(atlas0, featureAtlasUV);
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

        private static Texture2D CreateAtlasTexture(int resolution)
        {
            return new Texture2D(
                resolution,
                resolution,
                TextureFormat.RGBA32,
                false,
                true)
            {
                name = "GeneratedMass_FeatureAtlas0_Temporary",
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
                boundary.Score);

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
            Color32[] pixels,
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
                    pixels,
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
            Color32[] pixels,
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
                        pixels[pixelIndex].r = MaxByte(pixels[pixelIndex].r, convex.Proximity);
                        pixels[pixelIndex].g = MaxByte(pixels[pixelIndex].g, convex.Weight);
                    }

                    if (concave.Proximity > 0.001f)
                    {
                        pixels[pixelIndex].b = MaxByte(pixels[pixelIndex].b, concave.Proximity);
                        pixels[pixelIndex].a = MaxByte(pixels[pixelIndex].a, concave.Weight);
                    }
                }
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
            float core = Mathf.Max(
                texelWorld * (sharper ? 1.20f : 1.55f),
                safeWidth * Mathf.Lerp(sharper ? 0.045f : 0.065f, sharper ? 0.016f : 0.030f, softness01));
            float outer = Mathf.Max(
                core + texelWorld,
                safeWidth * Mathf.Lerp(sharper ? 0.70f : 0.86f, sharper ? 1.06f : 1.26f, softness01));

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
                    proximity = 1f - Mathf.SmoothStep(core, outer, distance);
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
