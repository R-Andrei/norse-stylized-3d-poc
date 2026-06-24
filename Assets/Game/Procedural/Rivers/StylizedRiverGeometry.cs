using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers
{
    public readonly struct StylizedRiverSplineSample
    {
        public StylizedRiverSplineSample(
            Vector3 centre,
            Vector3 tangent,
            Vector3 side,
            float distance,
            float normalizedTime)
            : this(
                centre,
                centre,
                tangent,
                side,
                Vector3.up,
                distance,
                distance,
                distance,
                0.5f,
                0.5f,
                normalizedTime,
                normalizedTime)
        {
            
        }

        public StylizedRiverSplineSample(
            Vector3 centre,
            Vector3 surfacePoint,
            Vector3 tangent,
            Vector3 side,
            Vector3 up,
            float localDistance,
            float orientedDistance,
            float globalDistance,
            float halfWidth,
            float surfaceHalfWidth,
            float normalizedDistance,
            float normalizedTime)
            : this(
                centre,
                surfacePoint,
                tangent,
                side,
                up,
                localDistance,
                orientedDistance,
                globalDistance,
                halfWidth,
                halfWidth,
                surfaceHalfWidth,
                surfaceHalfWidth,
                normalizedDistance,
                normalizedTime)
        {
        }

        public StylizedRiverSplineSample(
            Vector3 centre,
            Vector3 surfacePoint,
            Vector3 tangent,
            Vector3 side,
            Vector3 up,
            float localDistance,
            float orientedDistance,
            float globalDistance,
            float leftHalfWidth,
            float rightHalfWidth,
            float leftSurfaceHalfWidth,
            float rightSurfaceHalfWidth,
            float normalizedDistance,
            float normalizedTime)
        {
            Centre = centre;
            SurfacePoint = surfacePoint;
            Tangent = tangent;
            Side = side;
            Up = up;
            Distance = localDistance;
            OrientedDistance = orientedDistance;
            GlobalDistance = globalDistance;
            LeftHalfWidth = Mathf.Max(0.25f, leftHalfWidth);
            RightHalfWidth = Mathf.Max(0.25f, rightHalfWidth);
            LeftSurfaceHalfWidth =
                Mathf.Max(LeftHalfWidth, leftSurfaceHalfWidth);
            RightSurfaceHalfWidth =
                Mathf.Max(RightHalfWidth, rightSurfaceHalfWidth);
            HalfWidth = (LeftHalfWidth + RightHalfWidth) * 0.5f;
            SurfaceHalfWidth =
                Mathf.Max(LeftSurfaceHalfWidth, RightSurfaceHalfWidth);
            NormalizedDistance = normalizedDistance;
            NormalizedTime = normalizedTime;
        }

        /// <summary>Authored spline centre before the visual surface offset.</summary>
        public Vector3 Centre { get; }

        /// <summary>Actual centre of the generated water surface.</summary>
        public Vector3 SurfacePoint { get; }

        /// <summary>Planar tangent in authored spline order.</summary>
        public Vector3 Tangent { get; }

        /// <summary>Planar right vector across the river.</summary>
        public Vector3 Side { get; }

        /// <summary>Supported river-surface up direction. Currently world up.</summary>
        public Vector3 Up { get; }

        /// <summary>Local geometric distance from the authored spline start, in metres.</summary>
        public float Distance { get; }

        /// <summary>Distance measured downstream after reverse-flow orientation, in metres.</summary>
        public float OrientedDistance { get; }

        /// <summary>Connected-river offset plus oriented distance, in metres.</summary>
        public float GlobalDistance { get; }

        /// <summary>Average logical channel half-width retained for compatibility.</summary>
        public float HalfWidth { get; }

        /// <summary>Maximum generated surface half-width retained for compatibility.</summary>
        public float SurfaceHalfWidth { get; }

        public float LeftHalfWidth { get; }
        public float RightHalfWidth { get; }
        public float LeftSurfaceHalfWidth { get; }
        public float RightSurfaceHalfWidth { get; }

        public float GetVisibleHalfWidth(float signedLateral)
        {
            return signedLateral < 0f
                ? LeftHalfWidth
                : signedLateral > 0f
                    ? RightHalfWidth
                    : HalfWidth;
        }

        public float GetSurfaceHalfWidth(float signedLateral)
        {
            return signedLateral < 0f
                ? LeftSurfaceHalfWidth
                : signedLateral > 0f
                    ? RightSurfaceHalfWidth
                    : (LeftSurfaceHalfWidth + RightSurfaceHalfWidth) * 0.5f;
        }

        public float SurfaceHeight => SurfacePoint.y;
        public float NormalizedDistance { get; }
        public float NormalizedTime { get; }
    }

    public readonly struct StylizedRiverProjection
    {
        public StylizedRiverProjection(
            Vector3 centre,
            Vector3 tangent,
            Vector3 side,
            float distanceAlong,
            float acrossDistance)
            : this(
                centre,
                centre,
                tangent,
                side,
                Vector3.up,
                distanceAlong,
                distanceAlong,
                distanceAlong,
                acrossDistance,
                0f,
                0f,
                0f,
                false)
        {
        }

        public StylizedRiverProjection(
            Vector3 centre,
            Vector3 surfacePoint,
            Vector3 tangent,
            Vector3 side,
            Vector3 up,
            float localDistance,
            float orientedDistance,
            float globalDistance,
            float acrossMetres,
            float acrossNormalized,
            float distanceToNearestBank,
            float halfWidth,
            bool isInside)
        {
            Centre = centre;
            SurfacePoint = surfacePoint;
            Tangent = tangent;
            Side = side;
            Up = up;
            LocalDistance = localDistance;
            OrientedDistance = orientedDistance;
            GlobalDistance = globalDistance;
            AcrossMetres = acrossMetres;
            AcrossNormalized = acrossNormalized;
            DistanceToNearestBank = distanceToNearestBank;
            HalfWidth = halfWidth;
            IsInside = isInside;
        }

        public Vector3 Centre { get; }
        public Vector3 SurfacePoint { get; }
        public Vector3 Tangent { get; }
        public Vector3 Side { get; }
        public Vector3 Up { get; }
        public float LocalDistance { get; }
        public float OrientedDistance { get; }
        public float GlobalDistance { get; }
        public float AcrossMetres { get; }
        public float AcrossNormalized { get; }
        public float Across01 => AcrossNormalized * 0.5f + 0.5f;
        public float DistanceToNearestBank { get; }
        public float HalfWidth { get; }
        public bool IsInside { get; }

        // Compatibility aliases for older river consumers.
        public float DistanceAlong => LocalDistance;
        public float AcrossDistance => AcrossMetres;
    }

    public static class StylizedRiverGeometry
    {
        private const int MinimumLookupSamples = 64;
        private const int MaximumLookupSamples = 65536;
        private const float MinimumSampleSpacing = 0.05f;
        private const float MinimumLookupSpacing = 0.015f;

        public static RiverDomainSnapshot BuildDomain(
            SplineContainer container,
            float targetSpacing,
            float width,
            float edgeOverlap,
            float surfaceOffset,
            float connectedDistanceOffset,
            bool reverseFlow,
            int version)
        {
            return BuildDomain(
                container,
                targetSpacing,
                width,
                edgeOverlap,
                surfaceOffset,
                connectedDistanceOffset,
                reverseFlow,
                version,
                StylizedRiverNaturalVariationSettings.None);
        }

        public static RiverDomainSnapshot BuildDomain(
            SplineContainer container,
            float targetSpacing,
            float width,
            float edgeOverlap,
            float surfaceOffset,
            float connectedDistanceOffset,
            bool reverseFlow,
            int version,
            StylizedRiverNaturalVariationSettings naturalVariation)
        {
            float resolvedSpacing =
                Mathf.Max(
                    MinimumSampleSpacing,
                    targetSpacing);

            if (container == null || container.Splines.Count == 0)
            {
                return new RiverDomainSnapshot(
                    Array.Empty<StylizedRiverSplineSample>(),
                    0f,
                    resolvedSpacing,
                    connectedDistanceOffset,
                    reverseFlow,
                    version);
            }

            float approximateLength =
                Mathf.Max(
                    0.01f,
                    container.CalculateLength());

            float lookupSpacing =
                Mathf.Max(
                    MinimumLookupSpacing,
                    Mathf.Min(0.1f, resolvedSpacing * 0.2f));

            int lookupCount =
                Mathf.Clamp(
                    Mathf.CeilToInt(approximateLength / lookupSpacing) + 1,
                    MinimumLookupSamples,
                    MaximumLookupSamples);

            Vector3[] lookupPositions = new Vector3[lookupCount];
            float[] lookupParameters = new float[lookupCount];
            float[] lookupDistances = new float[lookupCount];

            float cumulativeDistance = 0f;

            for (int index = 0; index < lookupCount; index++)
            {
                float t = index / (float)(lookupCount - 1);
                float3 positionValue = container.EvaluatePosition(t);

                Vector3 position =
                    new Vector3(
                        positionValue.x,
                        positionValue.y,
                        positionValue.z);

                lookupPositions[index] = position;
                lookupParameters[index] = t;

                if (index > 0)
                {
                    cumulativeDistance +=
                        Vector3.Distance(
                            lookupPositions[index - 1],
                            position);
                }

                lookupDistances[index] = cumulativeDistance;
            }

            float actualLength = cumulativeDistance;

            if (actualLength <= 0.0001f)
            {
                return new RiverDomainSnapshot(
                    Array.Empty<StylizedRiverSplineSample>(),
                    0f,
                    resolvedSpacing,
                    connectedDistanceOffset,
                    reverseFlow,
                    version);
            }

            int sampleCount =
                Mathf.Max(
                    2,
                    Mathf.CeilToInt(actualLength / resolvedSpacing) + 1);

            float actualSampleSpacing =
                actualLength /
                Mathf.Max(1, sampleCount - 1);

            Vector3[] centres = new Vector3[sampleCount];
            float[] parameters = new float[sampleCount];
            float[] distances = new float[sampleCount];

            int lookupCursor = 1;

            for (int sampleIndex = 0;
                 sampleIndex < sampleCount;
                 sampleIndex++)
            {
                float targetDistance =
                    sampleIndex == sampleCount - 1
                        ? actualLength
                        : sampleIndex * actualSampleSpacing;

                while (lookupCursor < lookupCount - 1 &&
                       lookupDistances[lookupCursor] < targetDistance)
                {
                    lookupCursor++;
                }

                int lowerIndex = Mathf.Max(0, lookupCursor - 1);
                int upperIndex = Mathf.Min(lookupCount - 1, lookupCursor);

                float distanceT =
                    Mathf.InverseLerp(
                        lookupDistances[lowerIndex],
                        lookupDistances[upperIndex],
                        targetDistance);

                centres[sampleIndex] =
                    Vector3.Lerp(
                        lookupPositions[lowerIndex],
                        lookupPositions[upperIndex],
                        distanceT);

                parameters[sampleIndex] =
                    Mathf.Lerp(
                        lookupParameters[lowerIndex],
                        lookupParameters[upperIndex],
                        distanceT);

                distances[sampleIndex] = targetDistance;
            }

            float halfWidth = Mathf.Max(0.25f, width * 0.5f);
            float resolvedOverlap = Mathf.Max(0f, edgeOverlap);
            float safeShorelineScale =
                Mathf.Max(
                    naturalVariation.ShorelineIrregularityScale,
                    actualSampleSpacing * 4f,
                    naturalVariation.ResolveSafeShorelineAmplitude(halfWidth) *
                    4f);
            StylizedRiverNaturalVariationSettings domainVariation =
                new StylizedRiverNaturalVariationSettings(
                    naturalVariation.Seed,
                    naturalVariation.BedRoughness,
                    naturalVariation.BedRoughnessScale,
                    naturalVariation.BedRoughnessReach,
                    naturalVariation.ShorelineIrregularity,
                    safeShorelineScale,
                    naturalVariation.BankAsymmetry);

            StylizedRiverSplineSample[] samples =
                new StylizedRiverSplineSample[sampleCount];

            Vector3 previousTangent = Vector3.forward;

            for (int index = 0; index < sampleCount; index++)
            {
                Vector3 previous = centres[Mathf.Max(0, index - 1)];
                Vector3 next = centres[Mathf.Min(sampleCount - 1, index + 1)];
                Vector3 tangent = next - previous;
                tangent.y = 0f;

                if (tangent.sqrMagnitude < 0.000001f)
                {
                    float3 tangentValue =
                        container.EvaluateTangent(parameters[index]);

                    tangent =
                        new Vector3(
                            tangentValue.x,
                            0f,
                            tangentValue.z);
                }

                if (tangent.sqrMagnitude < 0.000001f)
                {
                    tangent = previousTangent;
                }
                else
                {
                    tangent.Normalize();
                }

                previousTangent = tangent;

                Vector3 up = Vector3.up;
                Vector3 side = Vector3.Cross(up, tangent).normalized;
                float localDistance = distances[index];
                float orientedDistance =
                    reverseFlow
                        ? actualLength - localDistance
                        : localDistance;

                float normalizedDistance =
                    actualLength > 0.0001f
                        ? localDistance / actualLength
                        : 0f;

                Vector3 centre = centres[index];
                Vector3 surfacePoint = centre + up * surfaceOffset;
                float globalDistance =
                    connectedDistanceOffset + orientedDistance;

                // Shape sampling follows stable authored distance so toggling
                // reverse flow does not regenerate a different shoreline.
                float shapeDistance =
                    connectedDistanceOffset + localDistance;

                StylizedRiverNaturalVariation.ResolveShoreWidths(
                    halfWidth,
                    resolvedOverlap,
                    shapeDistance,
                    domainVariation,
                    out float leftHalfWidth,
                    out float rightHalfWidth,
                    out float leftSurfaceHalfWidth,
                    out float rightSurfaceHalfWidth);

                samples[index] =
                    new StylizedRiverSplineSample(
                        centre,
                        surfacePoint,
                        tangent,
                        side,
                        up,
                        localDistance,
                        orientedDistance,
                        globalDistance,
                        leftHalfWidth,
                        rightHalfWidth,
                        leftSurfaceHalfWidth,
                        rightSurfaceHalfWidth,
                        normalizedDistance,
                        parameters[index]);
            }

            return new RiverDomainSnapshot(
                samples,
                actualLength,
                resolvedSpacing,
                connectedDistanceOffset,
                reverseFlow,
                version);
        }

        /// <summary>
        /// Compatibility bridge for older consumers. New systems should consume
        /// StylizedRiver.Domain so every subsystem shares one authoritative snapshot.
        /// </summary>
        public static float BuildSplineSamples(
            SplineContainer container,
            float targetSpacing,
            List<StylizedRiverSplineSample> samples)
        {
            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }

            RiverDomainSnapshot domain =
                BuildDomain(
                    container,
                    targetSpacing,
                    1f,
                    0f,
                    0f,
                    0f,
                    false,
                    0,
                    StylizedRiverNaturalVariationSettings.None);

            samples.Clear();

            for (int index = 0; index < domain.SampleCount; index++)
            {
                samples.Add(domain.Samples[index]);
            }

            return domain.LocalLength;
        }

        public static void BuildSurfaceMesh(
            Transform owner,
            RiverDomainSnapshot domain,
            int crossSegments,
            float targetLongitudinalSpacing,
            float maximumVerticalDisplacement,
            Mesh mesh)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            mesh.Clear();

            if (domain == null || !domain.IsValid)
            {
                return;
            }

            IReadOnlyList<StylizedRiverSplineSample> samples = domain.Samples;

            float resolvedSpacing = Mathf.Max(0.05f, targetLongitudinalSpacing);
            int rowCount = Mathf.Max(
                2,
                Mathf.CeilToInt(domain.LocalLength / resolvedSpacing) + 1);
            int acrossVertexCount = Mathf.Max(2, crossSegments + 1);
            int vertexCount = rowCount * acrossVertexCount;
            int triangleIndexCount =
                (rowCount - 1) *
                (acrossVertexCount - 1) *
                6;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector4[] tangents = new Vector4[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            List<Vector4> domainUvs = new List<Vector4>(vertexCount);
            List<Vector4> motionUvs = new List<Vector4>(vertexCount);
            Color[] colors = new Color[vertexCount];
            int[] triangles = new int[triangleIndexCount];

            for (int row = 0; row < rowCount; row++)
            {
                float localDistance =
                    row == rowCount - 1
                        ? domain.LocalLength
                        : domain.LocalLength * row / (float)(rowCount - 1);

                StylizedRiverSplineSample sample =
                    SampleAtDistance(samples, localDistance);

                Vector3 localTangent =
                    owner.InverseTransformDirection(
                        sample.Tangent).normalized;

                Vector3 localUp =
                    owner.InverseTransformDirection(
                        sample.Up).normalized;

                for (int acrossIndex = 0;
                     acrossIndex < acrossVertexCount;
                     acrossIndex++)
                {
                    float across01 =
                        acrossIndex /
                        (float)(acrossVertexCount - 1);

                    float acrossSigned = across01 * 2f - 1f;
                    float localSurfaceHalfWidth =
                        sample.GetSurfaceHalfWidth(acrossSigned);
                    float acrossMetres =
                        acrossSigned * localSurfaceHalfWidth;

                    Vector3 worldPosition =
                        sample.SurfacePoint +
                        sample.Side * acrossMetres;

                    int vertexIndex =
                        row * acrossVertexCount +
                        acrossIndex;

                    vertices[vertexIndex] =
                        owner.InverseTransformPoint(worldPosition);

                    normals[vertexIndex] = localUp;

                    tangents[vertexIndex] =
                        new Vector4(
                            localTangent.x,
                            localTangent.y,
                            localTangent.z,
                            1f);

                    // Existing contract: normalized cross-river position and
                    // local geometric metres along the authored spline.
                    uvs[vertexIndex] =
                        new Vector2(
                            across01,
                            sample.Distance);

                    // Reserved domain contract for later systems:
                    // x global downstream metres, y signed lateral metres,
                    // z oriented local metres, w generated surface half-width.
                    domainUvs.Add(
                        new Vector4(
                            sample.GlobalDistance,
                            acrossMetres,
                            sample.OrientedDistance,
                            localSurfaceHalfWidth));

                    float localVisibleHalfWidth =
                        sample.GetVisibleHalfWidth(acrossSigned);

                    // Stage 3 motion contract:
                    // x visible half-width, y generated surface half-width,
                    // z normalized visible-bank position, w reserved for Stage 5.
                    motionUvs.Add(
                        new Vector4(
                            localVisibleHalfWidth,
                            localSurfaceHalfWidth,
                            localVisibleHalfWidth > 0.0001f
                                ? acrossMetres / localVisibleHalfWidth
                                : 0f,
                            0f));

                    // Vertex colour remains non-authoritative compatibility data.
                    colors[vertexIndex] =
                        new Color(
                            across01,
                            sample.NormalizedDistance,
                            0f,
                            1f);
                }
            }

            int triangleCursor = 0;

            for (int row = 0; row < rowCount - 1; row++)
            {
                for (int acrossIndex = 0;
                     acrossIndex < acrossVertexCount - 1;
                     acrossIndex++)
                {
                    int a = row * acrossVertexCount + acrossIndex;
                    int b = a + 1;
                    int c = a + acrossVertexCount;
                    int d = c + 1;

                    triangles[triangleCursor++] = a;
                    triangles[triangleCursor++] = c;
                    triangles[triangleCursor++] = b;

                    triangles[triangleCursor++] = b;
                    triangles[triangleCursor++] = c;
                    triangles[triangleCursor++] = d;
                }
            }

            mesh.indexFormat =
                vertexCount > 65535
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16;

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.uv = uvs;
            mesh.SetUVs(1, domainUvs);
            mesh.SetUVs(2, motionUvs);
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            if (maximumVerticalDisplacement > 0.0001f)
            {
                Bounds expanded = mesh.bounds;
                expanded.Expand(
                    new Vector3(
                        0f,
                        maximumVerticalDisplacement * 2f + 0.05f,
                        0f));
                mesh.bounds = expanded;
            }
        }

        public static bool TryProjectPoint(
            IReadOnlyList<StylizedRiverSplineSample> samples,
            Vector3 worldPoint,
            out StylizedRiverProjection projection)
        {
            projection = default;

            if (samples == null || samples.Count < 2)
            {
                return false;
            }

            Vector2 point = new Vector2(worldPoint.x, worldPoint.z);
            float bestDistanceSqr = float.PositiveInfinity;

            for (int index = 0; index < samples.Count - 1; index++)
            {
                StylizedRiverSplineSample a = samples[index];
                StylizedRiverSplineSample b = samples[index + 1];

                Vector2 a2 = new Vector2(a.Centre.x, a.Centre.z);
                Vector2 b2 = new Vector2(b.Centre.x, b.Centre.z);
                Vector2 segment = b2 - a2;
                float lengthSqr = segment.sqrMagnitude;

                float t =
                    lengthSqr > 0.000001f
                        ? Mathf.Clamp01(
                            Vector2.Dot(point - a2, segment) /
                            lengthSqr)
                        : 0f;

                Vector2 nearest = a2 + segment * t;
                float distanceSqr = (point - nearest).sqrMagnitude;

                if (distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                StylizedRiverSplineSample sample = InterpolateStoredSample(a, b, t);
                Vector3 delta = worldPoint - sample.SurfacePoint;
                float acrossMetres = Vector3.Dot(delta, sample.Side);
                float localHalfWidth =
                    sample.GetVisibleHalfWidth(acrossMetres);
                float acrossNormalized =
                    localHalfWidth > 0.0001f
                        ? acrossMetres / localHalfWidth
                        : 0f;

                float bankDistance =
                    localHalfWidth - Mathf.Abs(acrossMetres);

                projection =
                    new StylizedRiverProjection(
                        sample.Centre,
                        sample.SurfacePoint,
                        sample.Tangent,
                        sample.Side,
                        sample.Up,
                        sample.Distance,
                        sample.OrientedDistance,
                        sample.GlobalDistance,
                        acrossMetres,
                        acrossNormalized,
                        bankDistance,
                        localHalfWidth,
                        bankDistance >= 0f);
            }

            return !float.IsPositiveInfinity(bestDistanceSqr);
        }

        public static StylizedRiverSplineSample SampleAtDistance(
            IReadOnlyList<StylizedRiverSplineSample> samples,
            float distance)
        {
            if (samples == null || samples.Count == 0)
            {
                return default;
            }

            if (distance <= 0f)
            {
                return samples[0];
            }

            StylizedRiverSplineSample last = samples[samples.Count - 1];

            if (distance >= last.Distance)
            {
                return last;
            }

            for (int index = 0; index < samples.Count - 1; index++)
            {
                StylizedRiverSplineSample a = samples[index];
                StylizedRiverSplineSample b = samples[index + 1];

                if (distance > b.Distance)
                {
                    continue;
                }

                float t =
                    Mathf.InverseLerp(
                        a.Distance,
                        b.Distance,
                        distance);

                return InterpolateStoredSample(a, b, t);
            }

            return last;
        }

        internal static StylizedRiverSplineSample InterpolateSample(
            StylizedRiverSplineSample a,
            StylizedRiverSplineSample b,
            float t,
            float localDistance,
            float localLength,
            float connectedDistanceOffset,
            bool reverseFlow)
        {
            Vector3 tangent =
                Vector3.Slerp(
                    a.Tangent,
                    b.Tangent,
                    t).normalized;

            Vector3 up =
                Vector3.Slerp(
                    a.Up,
                    b.Up,
                    t).normalized;

            Vector3 side = Vector3.Cross(up, tangent).normalized;
            float orientedDistance =
                reverseFlow
                    ? localLength - localDistance
                    : localDistance;

            return new StylizedRiverSplineSample(
                Vector3.Lerp(a.Centre, b.Centre, t),
                Vector3.Lerp(a.SurfacePoint, b.SurfacePoint, t),
                tangent,
                side,
                up,
                localDistance,
                orientedDistance,
                connectedDistanceOffset + orientedDistance,
                Mathf.Lerp(a.LeftHalfWidth, b.LeftHalfWidth, t),
                Mathf.Lerp(a.RightHalfWidth, b.RightHalfWidth, t),
                Mathf.Lerp(
                    a.LeftSurfaceHalfWidth,
                    b.LeftSurfaceHalfWidth,
                    t),
                Mathf.Lerp(
                    a.RightSurfaceHalfWidth,
                    b.RightSurfaceHalfWidth,
                    t),
                localLength > 0.0001f
                    ? localDistance / localLength
                    : 0f,
                Mathf.Lerp(a.NormalizedTime, b.NormalizedTime, t));
        }

        private static StylizedRiverSplineSample InterpolateStoredSample(
            StylizedRiverSplineSample a,
            StylizedRiverSplineSample b,
            float t)
        {
            Vector3 tangent =
                Vector3.Slerp(
                    a.Tangent,
                    b.Tangent,
                    t).normalized;

            Vector3 up =
                Vector3.Slerp(
                    a.Up,
                    b.Up,
                    t).normalized;

            Vector3 side = Vector3.Cross(up, tangent).normalized;

            return new StylizedRiverSplineSample(
                Vector3.Lerp(a.Centre, b.Centre, t),
                Vector3.Lerp(a.SurfacePoint, b.SurfacePoint, t),
                tangent,
                side,
                up,
                Mathf.Lerp(a.Distance, b.Distance, t),
                Mathf.Lerp(a.OrientedDistance, b.OrientedDistance, t),
                Mathf.Lerp(a.GlobalDistance, b.GlobalDistance, t),
                Mathf.Lerp(a.LeftHalfWidth, b.LeftHalfWidth, t),
                Mathf.Lerp(a.RightHalfWidth, b.RightHalfWidth, t),
                Mathf.Lerp(
                    a.LeftSurfaceHalfWidth,
                    b.LeftSurfaceHalfWidth,
                    t),
                Mathf.Lerp(
                    a.RightSurfaceHalfWidth,
                    b.RightSurfaceHalfWidth,
                    t),
                Mathf.Lerp(a.NormalizedDistance, b.NormalizedDistance, t),
                Mathf.Lerp(a.NormalizedTime, b.NormalizedTime, t));
        }
    }
}
