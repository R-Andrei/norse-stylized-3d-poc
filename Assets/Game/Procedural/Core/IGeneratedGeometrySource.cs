using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry
{
    /// <summary>
    /// Generic notification contract for procedural components that own a
    /// generated MeshFilter. Consumers can cache derived data and invalidate
    /// it only when the generated geometry actually changes.
    /// </summary>
    public interface IGeneratedGeometrySource
    {
        event Action GeometryChanged;

        MeshFilter GeometryMeshFilter { get; }
        bool IsSolidGeometry { get; }
        bool IsStaticGeometry { get; }
    }
}

