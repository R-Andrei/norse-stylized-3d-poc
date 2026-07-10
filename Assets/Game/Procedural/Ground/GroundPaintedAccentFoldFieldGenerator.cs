using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    internal readonly struct GroundPaintedAccentSurfaceStroke
    {
        public GroundPaintedAccentSurfaceStroke(
            Vector3[] localPoints,
            Vector3[] localNormals,
            float width,
            float bodyWidth,
            float strength,
            int seed)
        {
            LocalPoints = localPoints ?? Array.Empty<Vector3>();
            LocalNormals = localNormals ?? Array.Empty<Vector3>();
            Width = Mathf.Max(0.001f, width);
            BodyWidth = Mathf.Max(Width, bodyWidth);
            Strength = Mathf.Clamp01(strength);
            Seed = seed;
        }

        public Vector3[] LocalPoints { get; }
        public Vector3[] LocalNormals { get; }
        public float Width { get; }
        public float BodyWidth { get; }
        public float Strength { get; }
        public int Seed { get; }

        public bool IsValid =>
            LocalPoints != null &&
            LocalNormals != null &&
            LocalPoints.Length >= 2 &&
            LocalNormals.Length == LocalPoints.Length;
    }

    internal static class GroundPaintedAccentFoldFieldGenerator
    {
        public const int Resolution = 256;

        private const float MinimumFieldSize = 0.0001f;
        private const int MinimumStrokePointCount = 5;

        private readonly struct StrokeCandidate
        {
            public StrokeCandidate(
                int column,
                int row,
                uint sortKey)
            {
                Column = column;
                Row = row;
                SortKey = sortKey;
            }

            public int Column { get; }
            public int Row { get; }
            public uint SortKey { get; }
        }

        public static Texture2D Generate(
            Bounds localBounds,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            int shapeSeed,
            out Vector4 originSize,
            out Vector4 texelSize,
            out float[] bodyValues,
            out GroundPaintedAccentSurfaceStroke[] surfaceStrokes)
        {
            originSize =
                new Vector4(
                    localBounds.min.x,
                    localBounds.min.z,
                    Mathf.Max(MinimumFieldSize, localBounds.size.x),
                    Mathf.Max(MinimumFieldSize, localBounds.size.z));
            texelSize =
                new Vector4(
                    1f / Resolution,
                    1f / Resolution,
                    Resolution,
                    Resolution);

            FieldSettings settings =
                FieldSettings.Create(
                    feature,
                    shapeSeed,
                    originSize);

            surfaceStrokes =
                GenerateSurfaceStrokes(
                    originSize,
                    baseSurface,
                    feature,
                    settings);

            float[] selectedLineField = new float[Resolution * Resolution];
            float[] bodyField = new float[Resolution * Resolution];
            float[] signedSideField = new float[Resolution * Resolution];
            float[] supportField = new float[Resolution * Resolution];

            RasterizeSurfaceStrokes(
                originSize,
                baseSurface,
                feature,
                surfaceStrokes,
                settings,
                selectedLineField,
                bodyField,
                signedSideField,
                supportField);

            bodyValues = bodyField;

            Texture2D texture =
                new Texture2D(
                    Resolution,
                    Resolution,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    name = "GeneratedGround_PaintedAccentFoldField",
                    hideFlags = HideFlags.DontSave,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = TextureFilterModeForFoldField()
                };

            Color32[] pixels =
                BuildPixelsFromSurfaceStrokeFields(
                    selectedLineField,
                    bodyField,
                    signedSideField,
                    supportField);

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static FilterMode TextureFilterModeForFoldField()
        {
            return FilterMode.Bilinear;
        }

        private static GroundPaintedAccentSurfaceStroke[] GenerateSurfaceStrokes(
            Vector4 originSize,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            FieldSettings settings)
        {
            if (baseSurface == null ||
                !baseSurface.IsValid ||
                settings.TargetStrokeCount <= 0)
            {
                return Array.Empty<GroundPaintedAccentSurfaceStroke>();
            }

            List<GroundPaintedAccentSurfaceStroke> strokes =
                new List<GroundPaintedAccentSurfaceStroke>(settings.TargetStrokeCount);

            float aspect =
                Mathf.Max(0.25f, originSize.z / Mathf.Max(0.0001f, originSize.w));
            int candidateCellCount =
                Mathf.Max(
                    settings.TargetStrokeCount,
                    Mathf.CeilToInt(settings.TargetStrokeCount * 3.35f));
            int columns =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(Mathf.Sqrt(candidateCellCount * aspect)));
            int rows =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(candidateCellCount / (float)columns));
            float cellWidth = originSize.z / columns;
            float cellHeight = originSize.w / rows;
            List<StrokeCandidate> candidates =
                new List<StrokeCandidate>(columns * rows);

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int cellHash =
                        Hash(
                            column,
                            row,
                            settings.Seed);
                    uint sortKey =
                        (uint)Hash(
                            cellHash,
                            settings.Seed,
                            0x4D3A);
                    candidates.Add(
                        new StrokeCandidate(
                            column,
                            row,
                            sortKey));
                }
            }

            candidates.Sort(CompareStrokeCandidates);

            for (int attemptIndex = 0;
                 attemptIndex < candidates.Count && strokes.Count < settings.TargetStrokeCount;
                 attemptIndex++)
            {
                StrokeCandidate candidate = candidates[attemptIndex];
                int column = candidate.Column;
                int row = candidate.Row;

                int cellHash =
                    Hash(
                        column,
                        row,
                        settings.Seed);
                float jitterX = Hash01((uint)cellHash, 19u);
                float jitterZ = Hash01((uint)cellHash, 31u);
                float localX =
                    originSize.x +
                    (column + Mathf.Lerp(0.18f, 0.82f, jitterX)) * cellWidth;
                float localZ =
                    originSize.y +
                    (row + Mathf.Lerp(0.18f, 0.82f, jitterZ)) * cellHeight;
                Vector2 centerXZ = new Vector2(localX, localZ);

                float support =
                    ResolveSemanticSupport(
                        baseSurface,
                        centerXZ,
                        feature != null ? feature.MaskInfluence : 0f);
                float supportRoll = Hash01((uint)cellHash, 43u);
                if (supportRoll > Mathf.Lerp(0.82f, 0.995f, support))
                {
                    continue;
                }

                Vector2 axis = ResolveStrokeAxis(settings, (uint)cellHash);
                float length =
                    Mathf.Lerp(
                        settings.StrokeLengthMin,
                        settings.StrokeLengthMax,
                        Hash01((uint)cellHash, 53u));
                float width =
                    settings.StrokeWidthWorld *
                    Mathf.Lerp(
                        0.84f,
                        1.18f,
                        Hash01((uint)cellHash, 59u));
                float bodyWidth =
                    Mathf.Max(
                        width * 2.75f,
                        settings.BodyWidthWorld *
                        Mathf.Lerp(
                            0.82f,
                            1.18f,
                            Hash01((uint)cellHash, 61u)));
                float curvature =
                    bodyWidth *
                    Mathf.Lerp(0.10f, 0.30f, settings.Contrast) *
                    (Hash01((uint)cellHash, 67u) * 2f - 1f);
                float strength =
                    settings.Strength *
                    Mathf.Lerp(0.78f, 1.0f, support) *
                    Mathf.Lerp(0.80f, 1.0f, Hash01((uint)cellHash, 71u));

                if (TryCreateSurfaceStroke(
                        centerXZ,
                        axis,
                        length,
                        width,
                        bodyWidth,
                        curvature,
                        strength,
                        cellHash,
                        baseSurface,
                        out GroundPaintedAccentSurfaceStroke stroke))
                {
                    strokes.Add(stroke);
                }
            }

            return strokes.ToArray();
        }

        private static bool TryCreateSurfaceStroke(
            Vector2 centerXZ,
            Vector2 axis,
            float length,
            float width,
            float bodyWidth,
            float curvature,
            float strength,
            int strokeSeed,
            GroundHeightFieldSnapshot baseSurface,
            out GroundPaintedAccentSurfaceStroke stroke)
        {
            stroke = default;

            axis = ResolveSafeAxis(axis);
            Vector2 crossAxis = new Vector2(-axis.y, axis.x);
            int pointCount =
                Mathf.Clamp(
                    Mathf.CeilToInt(length * 1.85f),
                    MinimumStrokePointCount,
                    11);
            List<Vector3> localPoints = new List<Vector3>(pointCount);
            List<Vector3> localNormals = new List<Vector3>(pointCount);
            float phase = Hash01((uint)strokeSeed, 83u) * Mathf.PI * 2f;
            float secondaryPhase = Hash01((uint)strokeSeed, 89u) * Mathf.PI * 2f;

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                float t =
                    pointCount <= 1
                        ? 0f
                        : pointIndex / (float)(pointCount - 1);
                float signedT = t * 2f - 1f;
                float endFade = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);
                float wobble =
                    (Mathf.Sin(t * Mathf.PI * 1.35f + phase) * 0.72f +
                     Mathf.Sin(t * Mathf.PI * 2.10f + secondaryPhase) * 0.28f) *
                    curvature *
                    endFade;
                Vector2 localXZ =
                    centerXZ +
                    axis * (signedT * length * 0.5f) +
                    crossAxis * wobble;

                if (!baseSurface.TrySample(localXZ, out GroundSurfaceSample sample))
                {
                    continue;
                }

                localPoints.Add(new Vector3(localXZ.x, sample.Height, localXZ.y));
                localNormals.Add(sample.RenderNormal);
            }

            if (localPoints.Count < MinimumStrokePointCount)
            {
                return false;
            }

            stroke =
                new GroundPaintedAccentSurfaceStroke(
                    localPoints.ToArray(),
                    localNormals.ToArray(),
                    width,
                    bodyWidth,
                    strength,
                    strokeSeed);
            return stroke.IsValid;
        }

        private static void RasterizeSurfaceStrokes(
            Vector4 originSize,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            GroundPaintedAccentSurfaceStroke[] strokes,
            FieldSettings settings,
            float[] selectedLineField,
            float[] bodyField,
            float[] signedSideField,
            float[] supportField)
        {
            float maskInfluence =
                feature != null
                    ? feature.MaskInfluence
                    : 0f;
            float texelWorldSize =
                Mathf.Max(
                    originSize.z,
                    originSize.w) /
                Resolution;
            float lineFeather =
                Mathf.Max(texelWorldSize * 0.85f, settings.StrokeWidthWorld * 0.20f);

            for (int z = 0; z < Resolution; z++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    int index = z * Resolution + x;
                    Vector2 localXZ =
                        TexelToLocalXZ(
                            x,
                            z,
                            originSize);
                    float support =
                        ResolveSemanticSupport(
                            baseSurface,
                            localXZ,
                            maskInfluence);
                    supportField[index] = support;

                    float bestBody = 0f;
                    float bestSignedSide = 0f;
                    float bestLine = 0f;

                    for (int strokeIndex = 0;
                         strokeIndex < strokes.Length;
                         strokeIndex++)
                    {
                        GroundPaintedAccentSurfaceStroke stroke =
                            strokes[strokeIndex];
                        if (!stroke.IsValid)
                        {
                            continue;
                        }

                        if (!TryResolveClosestStrokeSample(
                                stroke,
                                localXZ,
                                out float distance,
                                out float signedSide))
                        {
                            continue;
                        }

                        float lineHalfWidth =
                            Mathf.Max(
                                stroke.Width * 0.5f,
                                texelWorldSize * 0.42f);
                        float line =
                            1f - SmoothStep(
                                lineHalfWidth,
                                lineHalfWidth + lineFeather,
                                distance);
                        float body =
                            1f - SmoothStep(
                                stroke.BodyWidth * 0.18f,
                                stroke.BodyWidth,
                                distance);

                        line = Mathf.Clamp01(line) * stroke.Strength * support;
                        body = Mathf.Clamp01(body) * stroke.Strength * support;

                        if (line > bestLine)
                        {
                            bestLine = line;
                        }

                        if (body > bestBody)
                        {
                            bestBody = body;
                            bestSignedSide = signedSide;
                        }
                    }

                    selectedLineField[index] = Mathf.Clamp01(bestLine);
                    bodyField[index] = Mathf.Clamp01(bestBody);
                    signedSideField[index] = Mathf.Clamp(bestSignedSide, -1f, 1f);
                }
            }
        }

        private static bool TryResolveClosestStrokeSample(
            GroundPaintedAccentSurfaceStroke stroke,
            Vector2 localXZ,
            out float distance,
            out float signedSide)
        {
            distance = float.PositiveInfinity;
            signedSide = 0f;

            Vector3[] points = stroke.LocalPoints;
            if (points == null || points.Length < 2)
            {
                return false;
            }

            for (int pointIndex = 0; pointIndex < points.Length - 1; pointIndex++)
            {
                Vector2 start = new Vector2(
                    points[pointIndex].x,
                    points[pointIndex].z);
                Vector2 end = new Vector2(
                    points[pointIndex + 1].x,
                    points[pointIndex + 1].z);
                Vector2 segment = end - start;
                float lengthSquared = segment.sqrMagnitude;
                if (lengthSquared <= 0.000001f)
                {
                    continue;
                }

                float t =
                    Mathf.Clamp01(
                        Vector2.Dot(localXZ - start, segment) /
                        lengthSquared);
                Vector2 closest = start + segment * t;
                Vector2 offset = localXZ - closest;
                float candidateDistance = offset.magnitude;
                if (candidateDistance >= distance)
                {
                    continue;
                }

                Vector2 segmentDirection = segment / Mathf.Sqrt(lengthSquared);
                Vector2 sideAxis = new Vector2(-segmentDirection.y, segmentDirection.x);
                float side =
                    Vector2.Dot(offset, sideAxis) /
                    Mathf.Max(0.0001f, stroke.BodyWidth);

                distance = candidateDistance;
                signedSide = Mathf.Clamp(side, -1f, 1f);
            }

            return !float.IsInfinity(distance);
        }

        private static Color32[] BuildPixelsFromSurfaceStrokeFields(
            float[] selectedLineField,
            float[] bodyField,
            float[] signedSideField,
            float[] supportField)
        {
            Color32[] pixels = new Color32[Resolution * Resolution];

            for (int index = 0; index < pixels.Length; index++)
            {
                float selectedLine =
                    selectedLineField != null && index < selectedLineField.Length
                        ? Mathf.Clamp01(selectedLineField[index])
                        : 0f;
                float body =
                    bodyField != null && index < bodyField.Length
                        ? Mathf.Clamp01(bodyField[index])
                        : 0f;
                float signedSide =
                    signedSideField != null && index < signedSideField.Length
                        ? Mathf.Clamp(signedSideField[index], -1f, 1f)
                        : 0f;
                float support =
                    supportField != null && index < supportField.Length
                        ? Mathf.Clamp01(supportField[index])
                        : 0f;

                pixels[index] =
                    new Color32(
                        ToByte(selectedLine),
                        ToByte(body),
                        ToByte(0.5f + signedSide * 0.5f),
                        ToByte(support));
            }

            return pixels;
        }

        private static float ResolveSemanticSupport(
            GroundHeightFieldSnapshot baseSurface,
            Vector2 localXZ,
            float maskInfluence)
        {
            if (baseSurface == null ||
                !baseSurface.IsValid ||
                !baseSurface.TrySample(localXZ, out GroundSurfaceSample sample))
            {
                return 1f;
            }

            float semanticSupport =
                0.22f +
                sample.DampDeposit * 0.22f +
                sample.VegetationSuitability * 0.24f +
                sample.Compaction * 0.24f +
                sample.ShoreInfluence * 0.10f +
                sample.RockyDry * 0.08f;

            return Mathf.Lerp(
                1f,
                Mathf.Clamp01(semanticSupport),
                Mathf.Clamp01(maskInfluence));
        }

        private static Vector2 TexelToLocalXZ(
            int x,
            int z,
            Vector4 originSize)
        {
            float u = (x + 0.5f) / Resolution;
            float v = (z + 0.5f) / Resolution;

            return new Vector2(
                originSize.x + u * originSize.z,
                originSize.y + v * originSize.w);
        }

        private static Vector2 ResolveStrokeAxis(
            FieldSettings settings,
            uint strokeHash)
        {
            float facingDirectionDegrees = settings.StrokeFacingDirectionDegrees;
            float perpendicularStrokeAngleDegrees = facingDirectionDegrees + 90f;
            float signedRoll = Hash01(strokeHash, 73u) * 2f - 1f;
            float jitterDegrees = signedRoll * settings.AngleJitterDegrees;
            float finalAngle =
                (perpendicularStrokeAngleDegrees + jitterDegrees) *
                Mathf.Deg2Rad;

            return new Vector2(
                Mathf.Cos(finalAngle),
                Mathf.Sin(finalAngle));
        }

        private static int CompareStrokeCandidates(
            StrokeCandidate left,
            StrokeCandidate right)
        {
            int order = left.SortKey.CompareTo(right.SortKey);
            if (order != 0)
            {
                return order;
            }

            order = left.Row.CompareTo(right.Row);
            if (order != 0)
            {
                return order;
            }

            return left.Column.CompareTo(right.Column);
        }

        private static Vector2 ResolveSafeAxis(Vector2 axis)
        {
            if (axis.sqrMagnitude < 0.0001f)
            {
                return Vector2.right;
            }

            return axis.normalized;
        }

        private static float SmoothStep(
            float edge0,
            float edge1,
            float value)
        {
            if (Mathf.Abs(edge1 - edge0) <= 0.00001f)
            {
                return value >= edge1 ? 1f : 0f;
            }

            return Smooth01((value - edge0) / (edge1 - edge0));
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Clamp01(value) * 255f),
                0,
                255);
        }

        private static int Hash(
            int a,
            int b,
            int salt)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)a) * 16777619u;
                h = (h ^ (uint)b) * 16777619u;
                h = (h ^ (uint)salt) * 16777619u;
                h ^= h >> 13;
                h *= 1274126177u;
                h ^= h >> 16;
                return (int)h;
            }
        }

        private static float Hash01(
            uint hash,
            uint salt)
        {
            uint value = hash ^ (salt * 0x9E3779B9u);
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215f;
        }

        private readonly struct FieldSettings
        {
            private FieldSettings(
                int seed,
                float strength,
                float contrast,
                int targetStrokeCount,
                float strokeLengthMin,
                float strokeLengthMax,
                float strokeWidthWorld,
                float bodyWidthWorld,
                float strokeFacingDirectionDegrees,
                float angleJitterDegrees)
            {
                Seed = seed;
                Strength = strength;
                Contrast = contrast;
                TargetStrokeCount = targetStrokeCount;
                StrokeLengthMin = strokeLengthMin;
                StrokeLengthMax = Mathf.Max(strokeLengthMin + 0.05f, strokeLengthMax);
                StrokeWidthWorld = strokeWidthWorld;
                BodyWidthWorld = bodyWidthWorld;
                StrokeFacingDirectionDegrees = strokeFacingDirectionDegrees;
                AngleJitterDegrees = angleJitterDegrees;
            }

            public int Seed { get; }
            public float Strength { get; }
            public float Contrast { get; }
            public int TargetStrokeCount { get; }
            public float StrokeLengthMin { get; }
            public float StrokeLengthMax { get; }
            public float StrokeWidthWorld { get; }
            public float BodyWidthWorld { get; }
            public float StrokeFacingDirectionDegrees { get; }
            public float AngleJitterDegrees { get; }

            public static FieldSettings Create(
                GroundSurfaceFeatureRecipe feature,
                int shapeSeed,
                Vector4 originSize)
            {
                float strength =
                    feature != null
                        ? Mathf.Clamp01(feature.Strength)
                        : 0f;
                float contrast =
                    feature != null
                        ? Mathf.Clamp01(feature.Contrast)
                        : 0.5f;
                int seed =
                    Hash(
                        shapeSeed,
                        feature != null ? feature.SeedOffset : 0,
                        0x5A3D);
                float area = Mathf.Max(1f, originSize.z * originSize.w);
                float areaFactor = Mathf.Max(0.05f, area / 1600f);
                float strokeDensity =
                    feature != null
                        ? feature.PaintedAccentStrokeDensity
                        : 34f;
                int targetStrokeCount =
                    Mathf.Clamp(
                        Mathf.RoundToInt(strokeDensity * areaFactor),
                        0,
                        128);
                float strokeLengthMin =
                    feature != null
                        ? feature.PaintedAccentStrokeLengthMin
                        : 0.55f;
                float strokeLengthMax =
                    feature != null
                        ? feature.PaintedAccentStrokeLengthMax
                        : 1.55f;
                strokeLengthMax = Mathf.Max(strokeLengthMin + 0.05f, strokeLengthMax);
                float strokeWidthWorld =
                    feature != null
                        ? feature.PaintedAccentStrokeWidth
                        : 0.12f;
                float bodyWidthWorld =
                    Mathf.Max(
                        strokeWidthWorld * 3.25f,
                        strokeLengthMax * 0.12f);
                float strokeFacingDirectionDegrees =
                    feature != null
                        ? feature.PaintedAccentStrokeFacingDirectionDegrees
                        : 90f;
                float angleJitterDegrees =
                    feature != null
                        ? feature.PaintedAccentStrokeAngleJitterDegrees
                        : 18f;

                return new FieldSettings(
                    seed,
                    Mathf.Max(0.05f, strength),
                    contrast,
                    targetStrokeCount,
                    strokeLengthMin,
                    strokeLengthMax,
                    strokeWidthWorld,
                    bodyWidthWorld,
                    strokeFacingDirectionDegrees,
                    angleJitterDegrees);
            }
        }
    }
}
