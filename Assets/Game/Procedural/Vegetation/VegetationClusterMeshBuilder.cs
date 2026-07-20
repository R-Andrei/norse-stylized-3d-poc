using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Vegetation
{
    public enum VegetationBenchmarkGeometry
    {
        OpaqueStrips = 0,
        CrossedCards = 1,
        Hybrid = 2
    }

    public readonly struct VegetationClusterMeshStats
    {
        public VegetationClusterMeshStats(int vertexCount, int triangleCount)
        {
            VertexCount = vertexCount;
            TriangleCount = triangleCount;
        }

        public int VertexCount { get; }
        public int TriangleCount { get; }
    }

    public static class VegetationClusterMeshBuilder
    {
        private const int BladeSegments = 2;
        private const int OpaqueBladeCount = 8;
        private const int CardCount = 3;
        private const int HybridBladeCount = 4;
        private const int HybridCardCount = 2;
        private const float ReferenceMasterBladeWidth = 0.0168f;

        public static Mesh Build(
            VegetationBenchmarkGeometry geometry,
            float clusterDiameter,
            float grassHeight,
            float masterBladeWidth,
            float tipWidthRatio,
            float taperStart,
            out VegetationClusterMeshStats stats)
        {
            float diameter = Mathf.Max(0.01f, clusterDiameter);
            float height = Mathf.Max(0.01f, grassHeight);
            float referenceWidth = Mathf.Max(0.001f, masterBladeWidth);
            float widthMultiplier = referenceWidth / ReferenceMasterBladeWidth;
            float clampedTipWidthRatio = Mathf.Clamp(tipWidthRatio, 0f, 0.5f);
            float clampedTaperStart = Mathf.Clamp(taperStart, 0f, 0.95f);

            var vertices = new List<Vector3>(128);
            var normals = new List<Vector3>(128);
            var uvs = new List<Vector2>(128);
            var centerlineXZ = new List<Vector2>(128);
            var colors = new List<Color>(128);
            var indices = new List<int>(256);

            switch (geometry)
            {
                case VegetationBenchmarkGeometry.OpaqueStrips:
                    AddBladeRing(
                        OpaqueBladeCount,
                        diameter,
                        height,
                        referenceWidth,
                        0.24f,
                        clampedTipWidthRatio,
                        clampedTaperStart,
                        vertices,
                        normals,
                        uvs,
                        centerlineXZ,
                        colors,
                        indices);
                    break;

                case VegetationBenchmarkGeometry.CrossedCards:
                    AddCards(
                        CardCount,
                        diameter,
                        height,
                        0.72f * widthMultiplier,
                        clampedTipWidthRatio,
                        clampedTaperStart,
                        vertices,
                        normals,
                        uvs,
                        centerlineXZ,
                        colors,
                        indices);
                    break;

                case VegetationBenchmarkGeometry.Hybrid:
                    AddBladeRing(
                        HybridBladeCount,
                        diameter,
                        height,
                        referenceWidth * (0.075f / 0.07f),
                        0.22f,
                        clampedTipWidthRatio,
                        clampedTaperStart,
                        vertices,
                        normals,
                        uvs,
                        centerlineXZ,
                        colors,
                        indices);
                    AddCards(
                        HybridCardCount,
                        diameter * 0.92f,
                        height * 0.96f,
                        0.46f * widthMultiplier,
                        clampedTipWidthRatio,
                        clampedTaperStart,
                        vertices,
                        normals,
                        uvs,
                        centerlineXZ,
                        colors,
                        indices,
                        Mathf.PI * 0.25f);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(geometry),
                        geometry,
                        "Unsupported vegetation benchmark geometry.");
            }

            var mesh = new Mesh
            {
                name = $"VegetationBenchmark_{geometry}",
                indexFormat = vertices.Count > ushort.MaxValue
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16,
                hideFlags = HideFlags.HideAndDontSave
            };

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetUVs(1, centerlineXZ);
            mesh.SetColors(colors);
            mesh.SetTriangles(indices, 0, true);
            mesh.bounds = new Bounds(
                new Vector3(0f, height * 0.5f, 0f),
                new Vector3(diameter * 1.5f, height * 1.25f, diameter * 1.5f));

            stats = new VegetationClusterMeshStats(
                vertices.Count,
                indices.Count / 3);
            return mesh;
        }

        private static void AddBladeRing(
            int count,
            float diameter,
            float height,
            float bladeWidth,
            float radialFraction,
            float tipWidthRatio,
            float taperStart,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Vector2> centerlineXZ,
            List<Color> colors,
            List<int> indices)
        {
            float radius = diameter * radialFraction;

            for (int bladeIndex = 0; bladeIndex < count; bladeIndex++)
            {
                float normalized = bladeIndex / (float)count;
                float angle = normalized * Mathf.PI * 2f;
                float radialOffset = radius * (0.45f + 0.55f * Hash01(bladeIndex * 17 + 3));
                Vector3 origin = new Vector3(
                    Mathf.Cos(angle) * radialOffset,
                    0f,
                    Mathf.Sin(angle) * radialOffset);
                float yaw = angle + Mathf.PI * (0.18f + Hash01(bladeIndex * 31 + 11));
                float bladeHeight = height * Mathf.Lerp(
                    0.76f,
                    1.08f,
                    Hash01(bladeIndex * 43 + 7));
                float bladeWidthScale = Mathf.Lerp(
                    0.72f,
                    1.2f,
                    Hash01(bladeIndex * 59 + 19));
                Vector2 lean = new Vector2(
                    Mathf.Cos(angle * 1.7f + 0.4f),
                    Mathf.Sin(angle * 1.3f - 0.2f)) * diameter * 0.12f;

                AddStrip(
                    origin,
                    yaw,
                    bladeWidth * bladeWidthScale,
                    bladeHeight,
                    lean,
                    tipWidthRatio,
                    taperStart,
                    true,
                    vertices,
                    normals,
                    uvs,
                    centerlineXZ,
                    colors,
                    indices);
            }
        }

        private static void AddCards(
            int count,
            float diameter,
            float height,
            float widthFraction,
            float tipWidthRatio,
            float taperStart,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Vector2> centerlineXZ,
            List<Color> colors,
            List<int> indices,
            float angleOffset = 0f)
        {
            float width = diameter * widthFraction;
            for (int cardIndex = 0; cardIndex < count; cardIndex++)
            {
                float angle = angleOffset + cardIndex * Mathf.PI / count;
                Vector2 lean = new Vector2(
                    Mathf.Cos(angle + 0.7f),
                    Mathf.Sin(angle + 0.7f)) * diameter * 0.08f;
                AddStrip(
                    Vector3.zero,
                    angle,
                    width,
                    height * Mathf.Lerp(0.9f, 1.05f, Hash01(cardIndex * 67 + 5)),
                    lean,
                    tipWidthRatio,
                    taperStart,
                    false,
                    vertices,
                    normals,
                    uvs,
                    centerlineXZ,
                    colors,
                    indices);
            }
        }

        private static void AddStrip(
            Vector3 origin,
            float yaw,
            float width,
            float height,
            Vector2 lean,
            float tipWidthRatio,
            float taperStart,
            bool applyGeometryTaper,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Vector2> centerlineXZ,
            List<Color> colors,
            List<int> indices)
        {
            int firstVertex = vertices.Count;
            Vector3 right = new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw));
            Vector3 normal = Vector3.Cross(Vector3.up, right).normalized;

            for (int row = 0; row <= BladeSegments; row++)
            {
                float t = row / (float)BladeSegments;
                float taper = applyGeometryTaper
                    ? EvaluateWidthProfile(t, taperStart, tipWidthRatio)
                    : 1f;
                float halfWidth = width * 0.5f * taper;
                Vector3 center = origin + new Vector3(
                    lean.x * t * t,
                    height * t,
                    lean.y * t * t);

                vertices.Add(center - right * halfWidth);
                vertices.Add(center + right * halfWidth);
                normals.Add(normal);
                normals.Add(normal);
                uvs.Add(new Vector2(0f, t));
                uvs.Add(new Vector2(1f, t));
                centerlineXZ.Add(new Vector2(center.x, center.z));
                centerlineXZ.Add(new Vector2(center.x, center.z));
                colors.Add(new Color(t, 0f, 0f, 1f));
                colors.Add(new Color(t, 0f, 0f, 1f));
            }

            for (int segment = 0; segment < BladeSegments; segment++)
            {
                int row = firstVertex + segment * 2;
                indices.Add(row);
                indices.Add(row + 2);
                indices.Add(row + 1);
                indices.Add(row + 1);
                indices.Add(row + 2);
                indices.Add(row + 3);
            }
        }

        private static float EvaluateWidthProfile(
            float normalizedHeight,
            float taperStart,
            float tipWidthRatio)
        {
            if (normalizedHeight <= taperStart)
            {
                return 1f;
            }

            float taperRange = Mathf.Max(0.0001f, 1f - taperStart);
            float t = Mathf.Clamp01((normalizedHeight - taperStart) / taperRange);
            t = t * t * (3f - 2f * t);
            return Mathf.Lerp(1f, tipWidthRatio, t);
        }

        private static float Hash01(int value)
        {
            unchecked
            {
                uint state = (uint)value;
                state ^= state >> 16;
                state *= 0x7feb352du;
                state ^= state >> 15;
                state *= 0x846ca68bu;
                state ^= state >> 16;
                return (state & 0x00ffffffu) / 16777215f;
            }
        }
    }
}
