using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Vegetation
{
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
        private const int CardCount = 3;
        private const float ReferenceMasterBladeWidth = 0.0168f;

        public static Mesh Build(
            float clusterDiameter,
            float grassHeight,
            float masterBladeWidth,
            out VegetationClusterMeshStats stats)
        {
            float diameter = Mathf.Max(0.01f, clusterDiameter);
            float height = Mathf.Max(0.01f, grassHeight);
            float referenceWidth = Mathf.Max(0.001f, masterBladeWidth);
            float widthMultiplier = referenceWidth / ReferenceMasterBladeWidth;

            var vertices = new List<Vector3>(18);
            var normals = new List<Vector3>(18);
            var uvs = new List<Vector2>(18);
            var centerlineXZ = new List<Vector2>(18);
            var colors = new List<Color>(18);
            var indices = new List<int>(36);

            AddCards(
                CardCount,
                diameter,
                height,
                0.72f * widthMultiplier,
                vertices,
                normals,
                uvs,
                centerlineXZ,
                colors,
                indices);

            var mesh = new Mesh
            {
                name = "Vegetation_CrossedCards",
                indexFormat = IndexFormat.UInt16,
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

        private static void AddCards(
            int count,
            float diameter,
            float height,
            float widthFraction,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Vector2> centerlineXZ,
            List<Color> colors,
            List<int> indices)
        {
            float width = diameter * widthFraction;
            for (int cardIndex = 0; cardIndex < count; cardIndex++)
            {
                float angle = cardIndex * Mathf.PI / count;
                Vector2 lean = new Vector2(
                    Mathf.Cos(angle + 0.7f),
                    Mathf.Sin(angle + 0.7f)) * diameter * 0.08f;
                AddCard(
                    angle,
                    width,
                    height * Mathf.Lerp(0.9f, 1.05f, Hash01(cardIndex * 67 + 5)),
                    lean,
                    vertices,
                    normals,
                    uvs,
                    centerlineXZ,
                    colors,
                    indices);
            }
        }

        private static void AddCard(
            float yaw,
            float width,
            float height,
            Vector2 lean,
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
                float halfWidth = width * 0.5f;
                Vector3 center = new Vector3(
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
