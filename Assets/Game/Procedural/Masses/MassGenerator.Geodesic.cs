using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Geodesic topology

        private static Topology BuildGeodesicTopology(int frequency)
        {
            List<Vector3> directions = new List<Vector3>();
            List<int> triangles = new List<int>();
            Dictionary<VertexKey, int> lookup =
                new Dictionary<VertexKey, int>();

            for (int face = 0; face < BaseTriangles.Length; face += 3)
            {
                Vector3 a = BaseVertices[BaseTriangles[face]].normalized;
                Vector3 b = BaseVertices[BaseTriangles[face + 1]].normalized;
                Vector3 c = BaseVertices[BaseTriangles[face + 2]].normalized;

                int[,] localIndices =
                    new int[frequency + 1, frequency + 1];

                for (int row = 0; row <= frequency; row++)
                {
                    int maximumColumn = frequency - row;

                    for (int column = 0; column <= maximumColumn; column++)
                    {
                        float weightB = row / (float)frequency;
                        float weightC = column / (float)frequency;
                        float weightA = 1f - weightB - weightC;

                        Vector3 direction =
                            (a * weightA + b * weightB + c * weightC).normalized;

                        localIndices[row, column] = GetOrCreateVertex(
                            direction,
                            directions,
                            lookup);
                    }
                }

                for (int row = 0; row < frequency; row++)
                {
                    int cellCount = frequency - row;

                    for (int column = 0; column < cellCount; column++)
                    {
                        int v0 = localIndices[row, column];
                        int v1 = localIndices[row + 1, column];
                        int v2 = localIndices[row, column + 1];

                        AddOutwardTopologyTriangle(
                            directions,
                            triangles,
                            v0,
                            v1,
                            v2);

                        if (column >= cellCount - 1)
                        {
                            continue;
                        }

                        int v3 = localIndices[row + 1, column + 1];

                        AddOutwardTopologyTriangle(
                            directions,
                            triangles,
                            v1,
                            v3,
                            v2);
                    }
                }
            }

            List<int>[] neighbours = BuildNeighbourLists(
                directions.Count,
                triangles);

            return new Topology(directions, triangles, neighbours);
        }

        private static int GetOrCreateVertex(
            Vector3 direction,
            List<Vector3> directions,
            Dictionary<VertexKey, int> lookup)
        {
            VertexKey key = new VertexKey(direction);

            if (lookup.TryGetValue(key, out int existing))
            {
                return existing;
            }

            int index = directions.Count;
            directions.Add(direction);
            lookup.Add(key, index);
            return index;
        }

        private static void AddOutwardTopologyTriangle(
            List<Vector3> positions,
            List<int> triangles,
            int a,
            int b,
            int c)
        {
            Vector3 normal = Vector3.Cross(
                positions[b] - positions[a],
                positions[c] - positions[a]);

            Vector3 faceCentre =
                (positions[a] + positions[b] + positions[c]) / 3f;

            if (Vector3.Dot(normal, faceCentre) < 0f)
            {
                int temporary = b;
                b = c;
                c = temporary;
            }

            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        private static List<int>[] BuildNeighbourLists(
            int vertexCount,
            List<int> triangles)
        {
            HashSet<int>[] sets = new HashSet<int>[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                sets[i] = new HashSet<int>();
            }

            for (int i = 0; i < triangles.Count; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                AddNeighbourPair(sets, a, b);
                AddNeighbourPair(sets, b, c);
                AddNeighbourPair(sets, c, a);
            }

            List<int>[] neighbours = new List<int>[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                neighbours[i] = new List<int>(sets[i]);
            }

            return neighbours;
        }

        private static void AddNeighbourPair(
            HashSet<int>[] sets,
            int a,
            int b)
        {
            sets[a].Add(b);
            sets[b].Add(a);
        }

        #endregion
    }
}
