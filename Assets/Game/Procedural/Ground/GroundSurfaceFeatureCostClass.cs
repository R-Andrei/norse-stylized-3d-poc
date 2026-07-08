using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundSurfaceFeatureCostClass
    {
        [InspectorName("Shader Only")]
        ShaderOnly = 0,

        [InspectorName("Mesh Mask Driven")]
        MeshMaskDriven = 1,

        [InspectorName("Generated Texture")]
        GeneratedTexture = 2,

        [InspectorName("Runtime State")]
        RuntimeState = 3
    }
}
