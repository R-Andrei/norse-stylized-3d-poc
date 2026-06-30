using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public readonly struct RiverDisturbanceFootprint
    {
        public RiverDisturbanceFootprint(
            Vector3 worldPosition,
            Vector3 worldDownstream,
            Vector3 worldAcross,
            float acrossHalfWidth,
            float alongHalfLength,
            Vector2[] contour,
            bool usedBoundsFallback)
        {
            WorldPosition = worldPosition;
            WorldDownstream = worldDownstream;
            WorldAcross = worldAcross;
            AcrossHalfWidth = acrossHalfWidth;
            AlongHalfLength = alongHalfLength;
            Contour = contour ?? System.Array.Empty<Vector2>();
            UsedBoundsFallback = usedBoundsFallback;
        }

        public Vector3 WorldPosition { get; }

        /// <summary>
        /// Exact world-space basis used when the cached waterline contour was
        /// resolved. Foam must reconstruct each contour vertex through this
        /// basis before projecting it into the authoritative river domain.
        /// </summary>
        public Vector3 WorldDownstream { get; }
        public Vector3 WorldAcross { get; }

        public float AcrossHalfWidth { get; }
        public float AlongHalfLength { get; }

        /// <summary>
        /// Convex source-local waterline contour centred on WorldPosition.
        /// X is measured along WorldDownstream and Y along WorldAcross. It is
        /// not already expressed in global river-distance/across coordinates.
        /// </summary>
        public Vector2[] Contour { get; }

        public bool UsedBoundsFallback { get; }
    }


    public readonly struct RiverDisturbancePressureSupportProfile
    {
        public RiverDisturbancePressureSupportProfile(
            float acrossHalfWidth,
            float representativeHeight,
            float maximumHeight,
            float[] sliceHeights,
            Vector4[] samples,
            bool usedBoundsFallback)
            : this(
                acrossHalfWidth,
                representativeHeight,
                maximumHeight,
                ResolveLateralSampleCount(samples),
                sliceHeights,
                samples,
                usedBoundsFallback)
        {
        }

        public RiverDisturbancePressureSupportProfile(
            float acrossHalfWidth,
            float representativeHeight,
            float maximumHeight,
            int lateralSampleCount,
            float[] sliceHeights,
            Vector4[] samples,
            bool usedBoundsFallback)
        {
            AcrossHalfWidth = acrossHalfWidth;
            RepresentativeHeight = representativeHeight;
            MaximumHeight = maximumHeight;
            LateralSampleCount = lateralSampleCount;
            SliceHeights = sliceHeights ?? System.Array.Empty<float>();
            Samples = samples ?? System.Array.Empty<Vector4>();
            UsedBoundsFallback = usedBoundsFallback;
        }

        public float AcrossHalfWidth { get; }
        public float RepresentativeHeight { get; }
        public float MaximumHeight { get; }
        public int LateralSampleCount { get; }
        public float[] SliceHeights { get; }

        /// <summary>
        /// Row-major support samples. For every height slice and lateral row,
        /// X is the upstream boundary, Y is the downstream boundary, and Z is
        /// one when the slice intersects the object at that row.
        /// </summary>
        public Vector4[] Samples { get; }
        public bool UsedBoundsFallback { get; }

        public bool IsValid =>
            SliceHeights != null &&
            Samples != null &&
            SliceHeights.Length ==
                RiverDisturbanceFootprintResolver.PressureSupportHeightSlices &&
            RiverDisturbanceFootprintResolver.IsSupportedPressureSampleCount(
                LateralSampleCount) &&
            Samples.Length ==
                LateralSampleCount *
                RiverDisturbanceFootprintResolver.PressureSupportHeightSlices &&
            RepresentativeHeight > 0.001f;

        private static int ResolveLateralSampleCount(Vector4[] samples)
        {
            int sampleLength = samples != null ? samples.Length : 0;
            int heightSlices =
                RiverDisturbanceFootprintResolver.PressureSupportHeightSlices;
            return heightSlices > 0 && sampleLength % heightSlices == 0
                ? sampleLength / heightSlices
                : 0;
        }
    }



    public readonly struct RiverDisturbancePressureBakeProfile
    {
        public RiverDisturbancePressureBakeProfile(
            float acrossHalfWidth,
            Vector4[] samples)
            : this(
                acrossHalfWidth,
                samples != null ? samples.Length : 0,
                samples,
                System.Array.Empty<float>())
        {
        }

        public RiverDisturbancePressureBakeProfile(
            float acrossHalfWidth,
            int lateralSampleCount,
            Vector4[] samples)
            : this(
                acrossHalfWidth,
                lateralSampleCount,
                samples,
                System.Array.Empty<float>())
        {
        }

        public RiverDisturbancePressureBakeProfile(
            float acrossHalfWidth,
            int lateralSampleCount,
            Vector4[] samples,
            float[] downstreamBoundaries)
        {
            AcrossHalfWidth = acrossHalfWidth;
            LateralSampleCount = lateralSampleCount;
            Samples = samples ?? System.Array.Empty<Vector4>();
            DownstreamBoundaries = downstreamBoundaries ??
                System.Array.Empty<float>();
        }

        public float AcrossHalfWidth { get; }
        public int LateralSampleCount { get; }

        /// <summary>
        /// One compact sample per lateral row. X is the waterline upstream
        /// boundary, Y is the boundary's downstream shift per metre of ridge
        /// height, Z is the modulation-safe cached height, and W is the local
        /// supported modulation ceiling. A non-positive W marks an invalid row.
        /// </summary>
        public Vector4[] Samples { get; }

        /// <summary>
        /// Waterline downstream boundary for each compact lateral row, in
        /// metres relative to the source. This remains separate from Samples so
        /// the accepted height/contact profile contract is unchanged.
        /// </summary>
        public float[] DownstreamBoundaries { get; }

        public bool HasGeometryBounds =>
            DownstreamBoundaries != null &&
            DownstreamBoundaries.Length == LateralSampleCount;

        public bool IsValid =>
            Samples != null &&
            RiverDisturbanceFootprintResolver.IsSupportedPressureSampleCount(
                LateralSampleCount) &&
            Samples.Length == LateralSampleCount &&
            AcrossHalfWidth > 0.001f;
    }

    /// <summary>
    /// Resolves a static object's water-contact footprint from its final mesh.
    /// This is intentionally event-time work: the result is cached by the
    /// disturbance runtime and is not recalculated every simulation step.
    /// </summary>
    public static class RiverDisturbanceFootprintResolver
    {
        public const int MaximumContourPoints = 16;
        public const int MinimumPressureSupportLateralSamples = 16;
        public const int MediumPressureSupportLateralSamples = 32;
        public const int MaximumPressureSupportLateralSamples = 64;

        // Compatibility aliases retain the former fixed-size public contract.
        public const int PressureSupportLateralSamples =
            MinimumPressureSupportLateralSamples;
        public const int PressureSupportHeightSlices = 8;
        public const int PressureSupportSampleCount =
            PressureSupportLateralSamples * PressureSupportHeightSlices;

        private const float MinimumHalfExtent = 0.05f;
        private const float PlaneIntersectionEpsilon = 0.005f;
        private const float ContourPointMergeDistance = 0.0025f;
        private const float ContourPointMergeDistanceSqr =
            ContourPointMergeDistance * ContourPointMergeDistance;

        private static readonly int[,] BoundsEdges =
        {
            { 0, 1 }, { 0, 2 }, { 0, 4 },
            { 1, 3 }, { 1, 5 }, { 2, 3 },
            { 2, 6 }, { 3, 7 }, { 4, 5 },
            { 4, 6 }, { 5, 7 }, { 6, 7 }
        };

        public static bool IsSupportedPressureSampleCount(int sampleCount)
        {
            return sampleCount == MinimumPressureSupportLateralSamples ||
                   sampleCount == MediumPressureSupportLateralSamples ||
                   sampleCount == MaximumPressureSupportLateralSamples;
        }

        public static int ResolvePressureSupportLateralSampleCount(
            int requestedSampleCount)
        {
            if (requestedSampleCount <=
                MinimumPressureSupportLateralSamples)
            {
                return MinimumPressureSupportLateralSamples;
            }

            return requestedSampleCount <=
                MediumPressureSupportLateralSamples
                    ? MediumPressureSupportLateralSamples
                    : MaximumPressureSupportLateralSamples;
        }

        public static bool TryResolve(
            StylizedRiver river,
            MeshFilter meshFilter,
            float padding,
            out RiverDisturbanceFootprint footprint,
            out string status)
        {
            footprint = default;

            if (river == null)
            {
                status = "No river was supplied.";
                return false;
            }

            Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            if (meshFilter == null || mesh == null)
            {
                status = "No generated mesh is available.";
                return false;
            }

            if (!TryGetWorldBounds(meshFilter, out Bounds worldBounds) ||
                !river.TryProjectWorldPoint(
                    worldBounds.center,
                    out StylizedRiverProjection centreProjection))
            {
                status = "The generated mesh could not be projected into the river domain.";
                return false;
            }

            Vector3 up = centreProjection.Up.sqrMagnitude > 0.0001f
                ? centreProjection.Up.normalized
                : Vector3.up;
            Vector3 downstream =
                centreProjection.Tangent * river.FlowDirection;
            downstream.y = 0f;
            downstream = downstream.sqrMagnitude > 0.0001f
                ? downstream.normalized
                : Vector3.forward;
            Vector3 across = centreProjection.Side.sqrMagnitude > 0.0001f
                ? centreProjection.Side.normalized
                : Vector3.Cross(up, downstream).normalized;
            Vector3 planePoint = centreProjection.SurfacePoint;

            List<Vector3> waterlinePoints = new();
            bool usedBoundsFallback = false;

            try
            {
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;

                for (int index = 0;
                     index + 2 < triangles.Length;
                     index += 3)
                {
                    Vector3 a = meshFilter.transform.TransformPoint(
                        vertices[triangles[index]]);
                    Vector3 b = meshFilter.transform.TransformPoint(
                        vertices[triangles[index + 1]]);
                    Vector3 c = meshFilter.transform.TransformPoint(
                        vertices[triangles[index + 2]]);

                    AddTrianglePlaneIntersections(
                        a,
                        b,
                        c,
                        planePoint,
                        up,
                        waterlinePoints);
                }
            }
            catch (UnityException)
            {
                waterlinePoints.Clear();
            }

            if (waterlinePoints.Count < 2)
            {
                AddBoundsPlaneIntersections(
                    meshFilter,
                    mesh.bounds,
                    planePoint,
                    up,
                    waterlinePoints);
                usedBoundsFallback = true;
            }

            if (waterlinePoints.Count < 2)
            {
                status = "The generated mesh does not intersect the river surface.";
                return false;
            }

            List<Vector3> insidePoints = new();
            for (int index = 0; index < waterlinePoints.Count; index++)
            {
                Vector3 point = waterlinePoints[index];
                if (river.TryProjectWorldPoint(
                        point,
                        out StylizedRiverProjection pointProjection) &&
                    pointProjection.IsInside)
                {
                    insidePoints.Add(point);
                }
            }

            List<Vector3> resolvedPoints = insidePoints.Count >= 2
                ? insidePoints
                : waterlinePoints;

            float minimumAlong = float.PositiveInfinity;
            float maximumAlong = float.NegativeInfinity;
            float minimumAcross = float.PositiveInfinity;
            float maximumAcross = float.NegativeInfinity;
            List<Vector2> projectedPoints = new();

            for (int index = 0; index < resolvedPoints.Count; index++)
            {
                Vector3 offset = resolvedPoints[index] - planePoint;
                float along = Vector3.Dot(offset, downstream);
                float lateral = Vector3.Dot(offset, across);
                minimumAlong = Mathf.Min(minimumAlong, along);
                maximumAlong = Mathf.Max(maximumAlong, along);
                minimumAcross = Mathf.Min(minimumAcross, lateral);
                maximumAcross = Mathf.Max(maximumAcross, lateral);
                projectedPoints.Add(new Vector2(along, lateral));
            }

            if (float.IsInfinity(minimumAlong) ||
                float.IsInfinity(minimumAcross))
            {
                status = "The generated waterline footprint was invalid.";
                return false;
            }

            float centreAlong = (minimumAlong + maximumAlong) * 0.5f;
            float centreAcross = (minimumAcross + maximumAcross) * 0.5f;
            Vector2 contourCentre = new Vector2(centreAlong, centreAcross);
            Vector2[] contour = BuildPaddedContour(
                projectedPoints,
                contourCentre,
                Mathf.Max(0f, padding));

            float rawAlongHalfLength =
                (maximumAlong - minimumAlong) * 0.5f;
            float rawAcrossHalfWidth =
                (maximumAcross - minimumAcross) * 0.5f;
            float alongHalfLength = Mathf.Max(
                MinimumHalfExtent,
                rawAlongHalfLength + Mathf.Max(0f, padding));
            float acrossHalfWidth = Mathf.Max(
                MinimumHalfExtent,
                rawAcrossHalfWidth + Mathf.Max(0f, padding));

            if (contour.Length >= 3)
            {
                alongHalfLength = MinimumHalfExtent;
                acrossHalfWidth = MinimumHalfExtent;
                for (int index = 0; index < contour.Length; index++)
                {
                    alongHalfLength = Mathf.Max(
                        alongHalfLength,
                        Mathf.Abs(contour[index].x));
                    acrossHalfWidth = Mathf.Max(
                        acrossHalfWidth,
                        Mathf.Abs(contour[index].y));
                }
            }

            Vector3 worldPosition =
                planePoint +
                downstream * centreAlong +
                across * centreAcross;

            if (!river.TryProjectWorldPoint(
                    worldPosition,
                    out StylizedRiverProjection resolvedProjection) ||
                !resolvedProjection.IsInside)
            {
                status = "The generated mesh touches the surface, but its resolved footprint centre is outside this river.";
                return false;
            }

            footprint = new RiverDisturbanceFootprint(
                worldPosition,
                downstream,
                across,
                acrossHalfWidth,
                alongHalfLength,
                contour,
                usedBoundsFallback);
            status = usedBoundsFallback
                ? "Resolved from generated-mesh bounds because readable waterline triangles were unavailable."
                : "Resolved from the generated mesh at the river surface.";
            return true;
        }

        public static bool TryResolveBoundsOnly(
            StylizedRiver river,
            MeshFilter meshFilter,
            float padding,
            out RiverDisturbanceFootprint footprint,
            out string status)
        {
            footprint = default;

            if (river == null)
            {
                status = "No river was supplied.";
                return false;
            }

            Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            if (meshFilter == null || mesh == null)
            {
                status = "No generated mesh is available.";
                return false;
            }

            if (!TryGetWorldBounds(meshFilter, out Bounds worldBounds) ||
                !river.TryProjectWorldPoint(
                    worldBounds.center,
                    out StylizedRiverProjection centreProjection))
            {
                status = "The generated mesh bounds could not be projected into the river domain.";
                return false;
            }

            Vector3 up = centreProjection.Up.sqrMagnitude > 0.0001f
                ? centreProjection.Up.normalized
                : Vector3.up;
            Vector3 downstream =
                centreProjection.Tangent * river.FlowDirection;
            downstream.y = 0f;
            downstream = downstream.sqrMagnitude > 0.0001f
                ? downstream.normalized
                : Vector3.forward;
            Vector3 across = centreProjection.Side.sqrMagnitude > 0.0001f
                ? centreProjection.Side.normalized
                : Vector3.Cross(up, downstream).normalized;
            Vector3 planePoint = centreProjection.SurfacePoint;

            List<Vector3> waterlinePoints = new();
            AddBoundsPlaneIntersections(
                meshFilter,
                mesh.bounds,
                planePoint,
                up,
                waterlinePoints);

            if (waterlinePoints.Count < 2)
            {
                AddWorldBoundsProjectionPoints(
                    worldBounds,
                    downstream,
                    across,
                    waterlinePoints);
            }

            if (waterlinePoints.Count < 2)
            {
                status = "The generated mesh bounds do not intersect the river surface.";
                return false;
            }

            float minimumAlong = float.PositiveInfinity;
            float maximumAlong = float.NegativeInfinity;
            float minimumAcross = float.PositiveInfinity;
            float maximumAcross = float.NegativeInfinity;
            List<Vector2> projectedPoints = new();

            for (int index = 0; index < waterlinePoints.Count; index++)
            {
                Vector3 offset = waterlinePoints[index] - planePoint;
                float along = Vector3.Dot(offset, downstream);
                float lateral = Vector3.Dot(offset, across);
                minimumAlong = Mathf.Min(minimumAlong, along);
                maximumAlong = Mathf.Max(maximumAlong, along);
                minimumAcross = Mathf.Min(minimumAcross, lateral);
                maximumAcross = Mathf.Max(maximumAcross, lateral);
                projectedPoints.Add(new Vector2(along, lateral));
            }

            if (float.IsInfinity(minimumAlong) ||
                float.IsInfinity(minimumAcross))
            {
                status = "The generated bounds footprint was invalid.";
                return false;
            }

            float centreAlong = (minimumAlong + maximumAlong) * 0.5f;
            float centreAcross = (minimumAcross + maximumAcross) * 0.5f;
            Vector2 contourCentre = new Vector2(centreAlong, centreAcross);
            Vector2[] contour = BuildPaddedContour(
                projectedPoints,
                contourCentre,
                Mathf.Max(0f, padding));

            float alongHalfLength = MinimumHalfExtent;
            float acrossHalfWidth = MinimumHalfExtent;
            if (contour.Length >= 3)
            {
                for (int index = 0; index < contour.Length; index++)
                {
                    alongHalfLength = Mathf.Max(
                        alongHalfLength,
                        Mathf.Abs(contour[index].x));
                    acrossHalfWidth = Mathf.Max(
                        acrossHalfWidth,
                        Mathf.Abs(contour[index].y));
                }
            }
            else
            {
                alongHalfLength = Mathf.Max(
                    MinimumHalfExtent,
                    (maximumAlong - minimumAlong) * 0.5f +
                    Mathf.Max(0f, padding));
                acrossHalfWidth = Mathf.Max(
                    MinimumHalfExtent,
                    (maximumAcross - minimumAcross) * 0.5f +
                    Mathf.Max(0f, padding));
                contour = BuildRectangleContour(
                    alongHalfLength,
                    acrossHalfWidth);
            }

            Vector3 worldPosition =
                planePoint +
                downstream * centreAlong +
                across * centreAcross;

            if (!river.TryProjectWorldPoint(
                    worldPosition,
                    out StylizedRiverProjection resolvedProjection) ||
                !resolvedProjection.IsInside)
            {
                status = "The generated mesh bounds touch the surface, but their resolved footprint centre is outside this river.";
                return false;
            }

            footprint = new RiverDisturbanceFootprint(
                worldPosition,
                downstream,
                across,
                acrossHalfWidth,
                alongHalfLength,
                contour,
                true);
            status = "Resolved from generated-mesh bounds under the runtime performance cap.";
            return true;
        }

        /// <summary>
        /// Builds the accepted Static Pressure support profile.
        ///
        /// This exact path scans generated mesh slices and must not be used by
        /// automatic Play-startup refreshes under the hard performance cap.
        /// Automatic generated sources use TryResolvePressureSupportFromFootprint.
        /// </summary>
        public static bool TryResolvePressureSupport(
            StylizedRiver river,
            MeshFilter meshFilter,
            RiverDisturbanceFootprint baseFootprint,
            float maximumHeightToInspect,
            out RiverDisturbancePressureSupportProfile profile,
            out string status)
        {
            return TryResolvePressureSupport(
                river,
                meshFilter,
                baseFootprint,
                maximumHeightToInspect,
                PressureSupportLateralSamples,
                out profile,
                out status);
        }

        public static bool TryResolvePressureSupport(
            StylizedRiver river,
            MeshFilter meshFilter,
            RiverDisturbanceFootprint baseFootprint,
            float maximumHeightToInspect,
            int requestedLateralSampleCount,
            out RiverDisturbancePressureSupportProfile profile,
            out string status)
        {
            profile = default;

            if (river == null || meshFilter == null ||
                meshFilter.sharedMesh == null ||
                baseFootprint.Contour == null ||
                baseFootprint.Contour.Length < 3)
            {
                status = "A valid river, generated mesh, and raw waterline footprint are required.";
                return false;
            }

            int lateralSampleCount =
                ResolvePressureSupportLateralSampleCount(
                    requestedLateralSampleCount);

            if (!river.TryProjectWorldPoint(
                    baseFootprint.WorldPosition,
                    out StylizedRiverProjection projection))
            {
                status = "The pressure-support origin could not be projected into the river domain.";
                return false;
            }

            Vector3 up = projection.Up.sqrMagnitude > 0.0001f
                ? projection.Up.normalized
                : Vector3.up;
            Vector3 downstream =
                projection.Tangent * river.FlowDirection;
            downstream.y = 0f;
            downstream = downstream.sqrMagnitude > 0.0001f
                ? downstream.normalized
                : Vector3.forward;
            Vector3 across = projection.Side.sqrMagnitude > 0.0001f
                ? projection.Side.normalized
                : Vector3.Cross(up, downstream).normalized;
            Vector3 planePoint = projection.SurfacePoint;

            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] worldVertices = System.Array.Empty<Vector3>();
            int[] triangles = System.Array.Empty<int>();
            bool usedBoundsFallback = false;

            try
            {
                Vector3[] localVertices = mesh.vertices;
                triangles = mesh.triangles;
                worldVertices = new Vector3[localVertices.Length];
                for (int index = 0; index < localVertices.Length; index++)
                {
                    worldVertices[index] =
                        meshFilter.transform.TransformPoint(localVertices[index]);
                }
            }
            catch (UnityException)
            {
                worldVertices = System.Array.Empty<Vector3>();
                triangles = System.Array.Empty<int>();
                usedBoundsFallback = true;
            }

            float maximumObjectHeight = 0f;
            for (int index = 0; index < worldVertices.Length; index++)
            {
                maximumObjectHeight = Mathf.Max(
                    maximumObjectHeight,
                    Vector3.Dot(worldVertices[index] - planePoint, up));
            }

            if (maximumObjectHeight <= 0.001f &&
                TryGetWorldBounds(meshFilter, out Bounds worldBounds))
            {
                Vector3 extents = worldBounds.extents;
                Vector3[] corners =
                {
                    worldBounds.center + new Vector3(-extents.x, -extents.y, -extents.z),
                    worldBounds.center + new Vector3( extents.x, -extents.y, -extents.z),
                    worldBounds.center + new Vector3(-extents.x,  extents.y, -extents.z),
                    worldBounds.center + new Vector3( extents.x,  extents.y, -extents.z),
                    worldBounds.center + new Vector3(-extents.x, -extents.y,  extents.z),
                    worldBounds.center + new Vector3( extents.x, -extents.y,  extents.z),
                    worldBounds.center + new Vector3(-extents.x,  extents.y,  extents.z),
                    worldBounds.center + new Vector3( extents.x,  extents.y,  extents.z)
                };
                for (int index = 0; index < corners.Length; index++)
                {
                    maximumObjectHeight = Mathf.Max(
                        maximumObjectHeight,
                        Vector3.Dot(corners[index] - planePoint, up));
                }
                usedBoundsFallback = true;
            }

            float inspectedHeight = Mathf.Min(
                Mathf.Max(0.05f, maximumHeightToInspect),
                maximumObjectHeight);
            if (inspectedHeight <= 0.02f)
            {
                status = "The generated object has no usable height above the river surface.";
                return false;
            }

            float[] sliceHeights =
                new float[PressureSupportHeightSlices];
            Vector4[] samples =
                new Vector4[
                    lateralSampleCount * PressureSupportHeightSlices];
            float acrossHalfWidth = Mathf.Max(
                MinimumHalfExtent,
                baseFootprint.AcrossHalfWidth);

            for (int slice = 0;
                 slice < PressureSupportHeightSlices;
                 slice++)
            {
                float slice01 = slice /
                    (float)(PressureSupportHeightSlices - 1);
                float sliceHeight = inspectedHeight * slice01;
                sliceHeights[slice] = sliceHeight;

                Vector2[] contour;
                if (slice == 0)
                {
                    contour = baseFootprint.Contour;
                }
                else if (worldVertices.Length > 0 && triangles.Length >= 3)
                {
                    List<Vector3> intersections = new();
                    Vector3 slicePoint = planePoint + up * sliceHeight;
                    for (int triangle = 0;
                         triangle + 2 < triangles.Length;
                         triangle += 3)
                    {
                        AddTrianglePlaneIntersections(
                            worldVertices[triangles[triangle]],
                            worldVertices[triangles[triangle + 1]],
                            worldVertices[triangles[triangle + 2]],
                            slicePoint,
                            up,
                            intersections);
                    }

                    List<Vector2> projected = new();
                    for (int pointIndex = 0;
                         pointIndex < intersections.Count;
                         pointIndex++)
                    {
                        Vector3 offset =
                            intersections[pointIndex] -
                            baseFootprint.WorldPosition;
                        projected.Add(new Vector2(
                            Vector3.Dot(offset, downstream),
                            Vector3.Dot(offset, across)));
                    }
                    contour = BuildPaddedContour(
                        projected,
                        Vector2.zero,
                        0f);
                }
                else
                {
                    contour = baseFootprint.Contour;
                }

                for (int row = 0;
                     row < lateralSampleCount;
                     row++)
                {
                    float row01 = row /
                        (float)(lateralSampleCount - 1);
                    float lateral = Mathf.Lerp(
                        -acrossHalfWidth,
                        acrossHalfWidth,
                        row01);
                    int sampleIndex =
                        slice * lateralSampleCount + row;

                    if (TryResolveContourRow(
                            contour,
                            lateral,
                            out float upstream,
                            out float downstreamBoundary))
                    {
                        samples[sampleIndex] = new Vector4(
                            upstream,
                            downstreamBoundary,
                            1f,
                            0f);
                    }
                    else
                    {
                        samples[sampleIndex] = Vector4.zero;
                    }
                }
            }

            List<float> centralSupportHeights = new();
            float resolvedMaximumHeight = 0f;
            for (int row = 0;
                 row < lateralSampleCount;
                 row++)
            {
                float row01 = row /
                    (float)(lateralSampleCount - 1);
                float lateral01 = Mathf.Abs(row01 * 2f - 1f);
                float rowHeight = 0f;
                for (int slice = 0;
                     slice < PressureSupportHeightSlices;
                     slice++)
                {
                    Vector4 sample = samples[
                        slice * lateralSampleCount + row];
                    if (sample.z > 0.5f)
                    {
                        rowHeight = sliceHeights[slice];
                    }
                }

                resolvedMaximumHeight = Mathf.Max(
                    resolvedMaximumHeight,
                    rowHeight);
                if (lateral01 <= 0.78f && rowHeight > 0.02f)
                {
                    centralSupportHeights.Add(rowHeight);
                }
            }

            if (centralSupportHeights.Count == 0)
            {
                status = "The generated mesh did not provide a usable height-aware upstream support profile.";
                return false;
            }

            centralSupportHeights.Sort();
            int percentileIndex = Mathf.Clamp(
                Mathf.RoundToInt(
                    (centralSupportHeights.Count - 1) * 0.60f),
                0,
                centralSupportHeights.Count - 1);
            float representativeHeight =
                centralSupportHeights[percentileIndex];

            profile = new RiverDisturbancePressureSupportProfile(
                acrossHalfWidth,
                representativeHeight,
                resolvedMaximumHeight,
                lateralSampleCount,
                sliceHeights,
                samples,
                usedBoundsFallback);
            status = usedBoundsFallback
                ? "Resolved height-aware pressure support using the generated-mesh bounds fallback."
                : "Resolved height-aware pressure support from generated-mesh slices.";
            return profile.IsValid;
        }

        public static bool TryResolvePressureSupportFromFootprint(
            RiverDisturbanceFootprint baseFootprint,
            float maximumHeightToInspect,
            int requestedLateralSampleCount,
            out RiverDisturbancePressureSupportProfile profile,
            out string status)
        {
            profile = default;

            if (baseFootprint.Contour == null ||
                baseFootprint.Contour.Length < 3)
            {
                status = "A valid footprint contour is required for pressure support.";
                return false;
            }

            int lateralSampleCount =
                ResolvePressureSupportLateralSampleCount(
                    requestedLateralSampleCount);
            float inspectedHeight = Mathf.Clamp(
                maximumHeightToInspect,
                0.05f,
                2.5f);
            float[] sliceHeights =
                new float[PressureSupportHeightSlices];
            Vector4[] samples =
                new Vector4[
                    lateralSampleCount * PressureSupportHeightSlices];
            float acrossHalfWidth = Mathf.Max(
                MinimumHalfExtent,
                baseFootprint.AcrossHalfWidth);

            for (int slice = 0;
                 slice < PressureSupportHeightSlices;
                 slice++)
            {
                float slice01 = slice /
                    (float)(PressureSupportHeightSlices - 1);
                float sliceHeight = inspectedHeight * slice01;
                sliceHeights[slice] = sliceHeight;

                for (int row = 0;
                     row < lateralSampleCount;
                     row++)
                {
                    float row01 = row /
                        (float)(lateralSampleCount - 1);
                    float lateral = Mathf.Lerp(
                        -acrossHalfWidth,
                        acrossHalfWidth,
                        row01);
                    int sampleIndex =
                        slice * lateralSampleCount + row;

                    if (TryResolveContourRow(
                            baseFootprint.Contour,
                            lateral,
                            out float upstream,
                            out float downstreamBoundary))
                    {
                        samples[sampleIndex] = new Vector4(
                            upstream,
                            downstreamBoundary,
                            1f,
                            0f);
                    }
                    else
                    {
                        samples[sampleIndex] = Vector4.zero;
                    }
                }
            }

            profile = new RiverDisturbancePressureSupportProfile(
                acrossHalfWidth,
                inspectedHeight,
                inspectedHeight,
                lateralSampleCount,
                sliceHeights,
                samples,
                true);
            status = "Resolved pressure support from cached footprint contour under the runtime performance cap.";
            return profile.IsValid;
        }

        public static bool TryBuildPressureBakeProfile(
            RiverDisturbancePressureSupportProfile support,
            float targetHeight,
            float maximumModulation,
            out RiverDisturbancePressureBakeProfile profile)
        {
            profile = default;

            if (!support.IsValid || targetHeight <= 0.0001f)
            {
                return false;
            }

            float safeModulation = Mathf.Max(1f, maximumModulation);
            const float endpointTaperStart = 0.82f;
            const float minimumSideFacingPressure = 0.25f;
            int lateralSampleCount = support.LateralSampleCount;
            float lateralSampleSpacing =
                support.AcrossHalfWidth * 2f /
                Mathf.Max(1, lateralSampleCount - 1);
            Vector4[] compactSamples =
                new Vector4[lateralSampleCount];
            float[] downstreamBoundaries =
                new float[lateralSampleCount];
            int validSampleCount = 0;

            for (int row = 0; row < lateralSampleCount; row++)
            {
                Vector4 baseSample = support.Samples[row];
                if (baseSample.z <= 0.5f)
                {
                    compactSamples[row] = Vector4.zero;
                    downstreamBoundaries[row] = 0f;
                    continue;
                }

                downstreamBoundaries[row] = baseSample.y;

                float localMaximumHeight = 0f;
                for (int slice = 0; slice < PressureSupportHeightSlices; slice++)
                {
                    Vector4 sample = support.Samples[
                        slice * lateralSampleCount + row];
                    if (sample.z > 0.5f)
                    {
                        localMaximumHeight = support.SliceHeights[slice];
                    }
                }

                float cachedHeight = Mathf.Min(
                    targetHeight,
                    localMaximumHeight / safeModulation);
                float modulationCeiling = Mathf.Min(
                    targetHeight * safeModulation,
                    localMaximumHeight);
                // Reduce static-pressure buildup where the local waterline
                // contour turns downstream around an obstruction's sides. A
                // nearly constant upstream boundary faces the flow and keeps
                // full pressure; steeper lateral boundary changes approach the
                // retained side-facing minimum. This remains cached event-time
                // work and does not alter the local support ceiling.
                float previousUpstream = baseSample.x;
                float nextUpstream = baseSample.x;
                int previousStepCount = 0;
                int nextStepCount = 0;

                if (row > 0)
                {
                    Vector4 previousBaseSample =
                        support.Samples[row - 1];
                    if (previousBaseSample.z > 0.5f)
                    {
                        previousUpstream = previousBaseSample.x;
                        previousStepCount = 1;
                    }
                }

                if (row + 1 < lateralSampleCount)
                {
                    Vector4 nextBaseSample = support.Samples[row + 1];
                    if (nextBaseSample.z > 0.5f)
                    {
                        nextUpstream = nextBaseSample.x;
                        nextStepCount = 1;
                    }
                }

                int measuredStepCount =
                    previousStepCount + nextStepCount;
                if (measuredStepCount > 0)
                {
                    float measuredLateralDistance =
                        lateralSampleSpacing * measuredStepCount;
                    float boundarySlope = Mathf.Abs(
                        nextUpstream - previousUpstream) /
                        Mathf.Max(0.0001f, measuredLateralDistance);
                    float flowFacingFactor = Mathf.Max(
                        minimumSideFacingPressure,
                        1f / Mathf.Sqrt(
                            1f + boundarySlope * boundarySlope));
                    cachedHeight *= flowFacingFactor;
                }

                if (cachedHeight <= 0.001f || modulationCeiling <= 0.001f)
                {
                    compactSamples[row] = Vector4.zero;
                    continue;
                }

                float baseUpstream = baseSample.x;
                float ceilingUpstream = ResolveUpstreamAtHeight(
                    support,
                    row,
                    modulationCeiling,
                    baseUpstream);
                float contactSlope = (ceilingUpstream - baseUpstream) /
                    Mathf.Max(0.001f, modulationCeiling);

                // Pathological overhangs and tiny slice differences should not
                // create enormous contact shifts in the compact GPU profile.
                contactSlope = Mathf.Clamp(contactSlope, -4f, 4f);

                float row01 = row /
                    (float)(lateralSampleCount - 1);
                float lateral01 = Mathf.Abs(row01 * 2f - 1f);
                float endpointTaper = 1f - Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.InverseLerp(
                        endpointTaperStart,
                        1f,
                        lateral01));
                cachedHeight *= endpointTaper;
                modulationCeiling *= endpointTaper;

                compactSamples[row] = new Vector4(
                    baseUpstream,
                    contactSlope,
                    cachedHeight,
                    modulationCeiling);
                validSampleCount++;
            }

            if (validSampleCount == 0)
            {
                return false;
            }

            profile = new RiverDisturbancePressureBakeProfile(
                support.AcrossHalfWidth,
                lateralSampleCount,
                compactSamples,
                downstreamBoundaries);
            return profile.IsValid;
        }

        private static float ResolveUpstreamAtHeight(
            RiverDisturbancePressureSupportProfile support,
            int row,
            float height,
            float fallback)
        {
            float previousHeight = 0f;
            float previousUpstream = fallback;
            bool hasPrevious = false;

            for (int slice = 0; slice < PressureSupportHeightSlices; slice++)
            {
                Vector4 sample = support.Samples[
                    slice * support.LateralSampleCount + row];
                if (sample.z <= 0.5f)
                {
                    break;
                }

                float sliceHeight = support.SliceHeights[slice];
                if (!hasPrevious)
                {
                    previousHeight = sliceHeight;
                    previousUpstream = sample.x;
                    hasPrevious = true;
                }

                if (sliceHeight >= height)
                {
                    float interpolation = Mathf.InverseLerp(
                        previousHeight,
                        sliceHeight,
                        height);
                    return Mathf.Lerp(
                        previousUpstream,
                        sample.x,
                        interpolation);
                }

                previousHeight = sliceHeight;
                previousUpstream = sample.x;
            }

            return hasPrevious ? previousUpstream : fallback;
        }

        public static bool TryGetWorldBounds(
            MeshFilter meshFilter,
            out Bounds worldBounds)
        {
            worldBounds = default;
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                return false;
            }

            MeshRenderer renderer =
                meshFilter.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                worldBounds = renderer.bounds;
                return true;
            }

            Bounds localBounds = meshFilter.sharedMesh.bounds;
            Vector3 centre =
                meshFilter.transform.TransformPoint(localBounds.center);
            Vector3 extents = localBounds.extents;
            Vector3 axisX =
                meshFilter.transform.TransformVector(
                    new Vector3(extents.x, 0f, 0f));
            Vector3 axisY =
                meshFilter.transform.TransformVector(
                    new Vector3(0f, extents.y, 0f));
            Vector3 axisZ =
                meshFilter.transform.TransformVector(
                    new Vector3(0f, 0f, extents.z));
            extents = new Vector3(
                Mathf.Abs(axisX.x) +
                Mathf.Abs(axisY.x) +
                Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) +
                Mathf.Abs(axisY.y) +
                Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) +
                Mathf.Abs(axisY.z) +
                Mathf.Abs(axisZ.z));
            worldBounds = new Bounds(centre, extents * 2f);
            return true;
        }

        private static bool TryResolveContourRow(
            IReadOnlyList<Vector2> contour,
            float lateral,
            out float upstream,
            out float downstream)
        {
            upstream = float.PositiveInfinity;
            downstream = float.NegativeInfinity;
            int intersections = 0;

            if (contour == null || contour.Count < 3)
            {
                return false;
            }

            for (int index = 0; index < contour.Count; index++)
            {
                Vector2 start = contour[index];
                Vector2 end = contour[(index + 1) % contour.Count];
                float delta = end.y - start.y;
                if (Mathf.Abs(delta) <= 0.00001f)
                {
                    continue;
                }

                float minimum = Mathf.Min(start.y, end.y);
                float maximum = Mathf.Max(start.y, end.y);
                if (lateral < minimum || lateral > maximum)
                {
                    continue;
                }

                float interpolation = (lateral - start.y) / delta;
                if (interpolation < -0.0001f || interpolation > 1.0001f)
                {
                    continue;
                }

                float along = Mathf.Lerp(
                    start.x,
                    end.x,
                    Mathf.Clamp01(interpolation));
                upstream = Mathf.Min(upstream, along);
                downstream = Mathf.Max(downstream, along);
                intersections++;
            }

            return intersections >= 2 &&
                   downstream > upstream + 0.005f;
        }

        private static Vector2[] BuildPaddedContour(
            List<Vector2> sourcePoints,
            Vector2 centre,
            float padding)
        {
            List<Vector2> unique = new();
            for (int index = 0; index < sourcePoints.Count; index++)
            {
                Vector2 point = sourcePoints[index];
                bool duplicate = false;
                for (int existing = 0; existing < unique.Count; existing++)
                {
                    if ((unique[existing] - point).sqrMagnitude <=
                        ContourPointMergeDistanceSqr)
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                {
                    unique.Add(point);
                }
            }

            if (unique.Count < 3)
            {
                return System.Array.Empty<Vector2>();
            }

            unique.Sort((left, right) =>
            {
                int xComparison = left.x.CompareTo(right.x);
                return xComparison != 0
                    ? xComparison
                    : left.y.CompareTo(right.y);
            });

            List<Vector2> lower = new();
            for (int index = 0; index < unique.Count; index++)
            {
                Vector2 point = unique[index];
                while (lower.Count >= 2 &&
                       Cross(
                           lower[lower.Count - 2],
                           lower[lower.Count - 1],
                           point) <= 0f)
                {
                    lower.RemoveAt(lower.Count - 1);
                }
                lower.Add(point);
            }

            List<Vector2> upper = new();
            for (int index = unique.Count - 1; index >= 0; index--)
            {
                Vector2 point = unique[index];
                while (upper.Count >= 2 &&
                       Cross(
                           upper[upper.Count - 2],
                           upper[upper.Count - 1],
                           point) <= 0f)
                {
                    upper.RemoveAt(upper.Count - 1);
                }
                upper.Add(point);
            }

            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);

            for (int index = 0; index < lower.Count; index++)
            {
                Vector2 relative = lower[index] - centre;
                if (padding > 0f)
                {
                    Vector2 direction = relative.sqrMagnitude > 0.000001f
                        ? relative.normalized
                        : Vector2.right;
                    relative += direction * padding;
                }
                lower[index] = relative;
            }

            if (lower.Count <= MaximumContourPoints)
            {
                return lower.ToArray();
            }

            return ResampleClosedContour(lower, MaximumContourPoints);
        }

        private static Vector2[] ResampleClosedContour(
            List<Vector2> contour,
            int targetCount)
        {
            float[] cumulative = new float[contour.Count + 1];
            for (int index = 0; index < contour.Count; index++)
            {
                Vector2 current = contour[index];
                Vector2 next = contour[(index + 1) % contour.Count];
                cumulative[index + 1] =
                    cumulative[index] + Vector2.Distance(current, next);
            }

            float perimeter = cumulative[cumulative.Length - 1];
            if (perimeter <= 0.0001f)
            {
                return System.Array.Empty<Vector2>();
            }

            Vector2[] result = new Vector2[targetCount];
            int edge = 0;
            for (int sample = 0; sample < targetCount; sample++)
            {
                float targetDistance = perimeter * sample / targetCount;
                while (edge + 1 < cumulative.Length &&
                       cumulative[edge + 1] < targetDistance)
                {
                    edge++;
                }

                int startIndex = edge % contour.Count;
                int endIndex = (startIndex + 1) % contour.Count;
                float edgeLength = Mathf.Max(
                    0.0001f,
                    cumulative[edge + 1] - cumulative[edge]);
                float interpolation =
                    (targetDistance - cumulative[edge]) / edgeLength;
                result[sample] = Vector2.Lerp(
                    contour[startIndex],
                    contour[endIndex],
                    interpolation);
            }

            return result;
        }

        private static float Cross(
            Vector2 origin,
            Vector2 first,
            Vector2 second)
        {
            Vector2 a = first - origin;
            Vector2 b = second - origin;
            return a.x * b.y - a.y * b.x;
        }

        private static void AddTrianglePlaneIntersections(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 planePoint,
            Vector3 planeNormal,
            List<Vector3> target)
        {
            float distanceA = Vector3.Dot(a - planePoint, planeNormal);
            float distanceB = Vector3.Dot(b - planePoint, planeNormal);
            float distanceC = Vector3.Dot(c - planePoint, planeNormal);

            AddEdgePlaneIntersection(
                a, b, distanceA, distanceB, target);
            AddEdgePlaneIntersection(
                b, c, distanceB, distanceC, target);
            AddEdgePlaneIntersection(
                c, a, distanceC, distanceA, target);
        }

        private static void AddEdgePlaneIntersection(
            Vector3 start,
            Vector3 end,
            float startDistance,
            float endDistance,
            List<Vector3> target)
        {
            bool startOnPlane =
                Mathf.Abs(startDistance) <= PlaneIntersectionEpsilon;
            bool endOnPlane =
                Mathf.Abs(endDistance) <= PlaneIntersectionEpsilon;

            if (startOnPlane)
            {
                target.Add(start);
            }

            if (endOnPlane)
            {
                target.Add(end);
            }

            bool crossesPlane =
                (startDistance < -PlaneIntersectionEpsilon &&
                 endDistance > PlaneIntersectionEpsilon) ||
                (startDistance > PlaneIntersectionEpsilon &&
                 endDistance < -PlaneIntersectionEpsilon);

            if (!crossesPlane)
            {
                return;
            }

            float denominator = startDistance - endDistance;
            if (Mathf.Abs(denominator) <= 0.000001f)
            {
                return;
            }

            float interpolation = startDistance / denominator;
            target.Add(Vector3.LerpUnclamped(
                start,
                end,
                interpolation));
        }

        private static void AddBoundsPlaneIntersections(
            MeshFilter meshFilter,
            Bounds localBounds,
            Vector3 planePoint,
            Vector3 planeNormal,
            List<Vector3> target)
        {
            Vector3 minimum = localBounds.min;
            Vector3 maximum = localBounds.max;
            Vector3[] corners =
            {
                new Vector3(minimum.x, minimum.y, minimum.z),
                new Vector3(maximum.x, minimum.y, minimum.z),
                new Vector3(minimum.x, maximum.y, minimum.z),
                new Vector3(maximum.x, maximum.y, minimum.z),
                new Vector3(minimum.x, minimum.y, maximum.z),
                new Vector3(maximum.x, minimum.y, maximum.z),
                new Vector3(minimum.x, maximum.y, maximum.z),
                new Vector3(maximum.x, maximum.y, maximum.z)
            };

            for (int index = 0; index < corners.Length; index++)
            {
                corners[index] =
                    meshFilter.transform.TransformPoint(corners[index]);
            }

            for (int edge = 0;
                 edge < BoundsEdges.GetLength(0);
                 edge++)
            {
                Vector3 start = corners[BoundsEdges[edge, 0]];
                Vector3 end = corners[BoundsEdges[edge, 1]];
                float startDistance =
                    Vector3.Dot(start - planePoint, planeNormal);
                float endDistance =
                    Vector3.Dot(end - planePoint, planeNormal);
                AddEdgePlaneIntersection(
                    start,
                    end,
                    startDistance,
                    endDistance,
                    target);
            }
        }

        private static void AddWorldBoundsProjectionPoints(
            Bounds worldBounds,
            Vector3 downstream,
            Vector3 across,
            List<Vector3> target)
        {
            Vector3 extents = worldBounds.extents;
            float alongExtent =
                Mathf.Abs(Vector3.Dot(Vector3.right * extents.x, downstream)) +
                Mathf.Abs(Vector3.Dot(Vector3.up * extents.y, downstream)) +
                Mathf.Abs(Vector3.Dot(Vector3.forward * extents.z, downstream));
            float acrossExtent =
                Mathf.Abs(Vector3.Dot(Vector3.right * extents.x, across)) +
                Mathf.Abs(Vector3.Dot(Vector3.up * extents.y, across)) +
                Mathf.Abs(Vector3.Dot(Vector3.forward * extents.z, across));

            alongExtent = Mathf.Max(MinimumHalfExtent, alongExtent);
            acrossExtent = Mathf.Max(MinimumHalfExtent, acrossExtent);
            Vector3 centre = worldBounds.center;
            target.Add(centre - downstream * alongExtent - across * acrossExtent);
            target.Add(centre + downstream * alongExtent - across * acrossExtent);
            target.Add(centre + downstream * alongExtent + across * acrossExtent);
            target.Add(centre - downstream * alongExtent + across * acrossExtent);
        }

        private static Vector2[] BuildRectangleContour(
            float alongHalfLength,
            float acrossHalfWidth)
        {
            alongHalfLength = Mathf.Max(MinimumHalfExtent, alongHalfLength);
            acrossHalfWidth = Mathf.Max(MinimumHalfExtent, acrossHalfWidth);
            return new[]
            {
                new Vector2(-alongHalfLength, -acrossHalfWidth),
                new Vector2(alongHalfLength, -acrossHalfWidth),
                new Vector2(alongHalfLength, acrossHalfWidth),
                new Vector2(-alongHalfLength, acrossHalfWidth)
            };
        }
    }
}
