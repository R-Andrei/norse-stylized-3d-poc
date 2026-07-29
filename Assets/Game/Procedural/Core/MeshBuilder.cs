using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Geometry
{
    /// <summary>
    /// Converts validated MeshData into a Unity Mesh.
    /// </summary>
    public static class MeshBuilder
    {
        public static void ApplyToMesh(
            MeshData data,
            Mesh targetMesh,
            string meshName)
        {
            
            if (data == null)
            {
                throw new System.ArgumentNullException(nameof(data));
            }

            if (targetMesh == null)
            {
                throw new System.ArgumentNullException(nameof(targetMesh));
            }

            data.Validate();

            targetMesh.Clear();
            targetMesh.name = meshName;

            targetMesh.indexFormat =
                data.VertexCount > ushort.MaxValue
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16;

            targetMesh.SetVertices(data.Vertices);
            targetMesh.SetTriangles(data.Triangles, 0);
            targetMesh.SetUVs(0, data.UV0);

            if (data.HasUV2)
            {
                targetMesh.SetUVs(2, data.UV2);
            }

            if (data.HasSurfaceFeatures)
            {
                // TEXCOORD4 is reserved for Generated Mass structural
                // feature contributions. TEXCOORD3 remains untouched because
                // the retired diagnostic feature-atlas path still uses it.
                targetMesh.SetUVs(4, data.SurfaceFeatures);
            }

            targetMesh.SetColors(data.Colors);

            if (data.HasNormals)
            {
                targetMesh.SetNormals(data.Normals);
            }
            else
            {
                targetMesh.RecalculateNormals();
            }

            targetMesh.RecalculateTangents();
            targetMesh.RecalculateBounds();
        }
    }
}
