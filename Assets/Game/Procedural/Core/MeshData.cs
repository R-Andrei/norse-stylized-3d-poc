using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry
{
    /// <summary>
    /// Mutable, Unity-independent mesh construction data.
    /// Generators write into this object before it is converted to a Unity Mesh.
    /// </summary>
    [Serializable]
    public sealed class MeshData
    {
        public readonly List<Vector3> Vertices = new();
        public readonly List<int> Triangles = new();
        public readonly List<Vector2> UV0 = new();
        public readonly List<Color> Colors = new();

        public int VertexCount => Vertices.Count;
        public int TriangleCount => Triangles.Count / 3;

        public void Clear()
        {
            Vertices.Clear();
            Triangles.Clear();
            UV0.Clear();
            Colors.Clear();
        }

        public int AddVertex(
            Vector3 position,
            Vector2 uv,
            Color color)
        {
            int index = Vertices.Count;

            Vertices.Add(position);
            UV0.Add(uv);
            Colors.Add(color);

            return index;
        }

        public void AddTriangle(int a, int b, int c)
        {
            ValidateVertexIndex(a);
            ValidateVertexIndex(b);
            ValidateVertexIndex(c);

            Triangles.Add(a);
            Triangles.Add(b);
            Triangles.Add(c);
        }

        public void AddQuad(int bottomLeft, int topLeft, int topRight, int bottomRight)
        {
            AddTriangle(bottomLeft, topLeft, topRight);
            AddTriangle(bottomLeft, topRight, bottomRight);
        }

        public void Validate()
        {
            if (Vertices.Count < 3)
            {
                throw new InvalidOperationException(
                    "Mesh data must contain at least three vertices.");
            }

            if (Triangles.Count == 0 || Triangles.Count % 3 != 0)
            {
                throw new InvalidOperationException(
                    "Triangle index count must be a non-zero multiple of three.");
            }

            if (UV0.Count != Vertices.Count)
            {
                throw new InvalidOperationException(
                    "UV0 count must match the vertex count.");
            }

            if (Colors.Count != Vertices.Count)
            {
                throw new InvalidOperationException(
                    "Vertex color count must match the vertex count.");
            }

            for (int i = 0; i < Triangles.Count; i++)
            {
                ValidateVertexIndex(Triangles[i]);
            }
        }

        private void ValidateVertexIndex(int index)
        {
            if (index < 0 || index >= Vertices.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    $"Vertex index must be between 0 and {Vertices.Count - 1}.");
            }
        }
    }
}