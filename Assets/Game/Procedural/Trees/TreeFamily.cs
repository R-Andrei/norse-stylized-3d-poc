using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    public enum TreeFamily
    {
        [InspectorName("Common")]
        Common = 0,

        [InspectorName("Pine")]
        Pine = 1,

        [InspectorName("Twisted")]
        Twisted = 2,

        [InspectorName("Dead")]
        Dead = 3
    }

    public enum TreeReferenceRole
    {
        [InspectorName("Imported Reference")]
        ImportedReference = 0,

        [InspectorName("Procedural Comparison")]
        ProceduralComparison = 1
    }

    public enum TreeImportedWindMaskMode
    {
        [InspectorName("Bounds Height Fallback")]
        BoundsHeightFallback = 0,

        [InspectorName("Vertex Colour Red")]
        VertexColourRed = 1
    }

    public enum TreeReferenceDebugMode
    {
        [InspectorName("Final Rendering")]
        FinalRendering = 0,

        [InspectorName("Vertex Colour")]
        VertexColour = 1,

        [InspectorName("Bounds Height Mask")]
        BoundsHeightMask = 2,

        [InspectorName("Active Wind Mask")]
        ActiveWindMask = 3,

        [InspectorName("Foliage Fallback Phase")]
        FoliageFallbackPhase = 4,

        [InspectorName("Deformed World Normal")]
        DeformedWorldNormal = 5
    }

    public enum TreeFoliageDebugMode
    {
        [InspectorName("Final Rendering")]
        FinalRendering = 0,

        [InspectorName("Source Albedo")]
        SourceAlbedo = 1,

        [InspectorName("Alpha Mask")]
        AlphaMask = 2,

        [InspectorName("Front / Back Face")]
        FrontBackFace = 3,

        [InspectorName("Canopy Height")]
        CanopyHeight = 4,

        [InspectorName("Cluster Variation")]
        ClusterVariation = 5,

        [InspectorName("Orientation Factor")]
        OrientationFactor = 6,

        [InspectorName("Realtime Shadow")]
        RealtimeShadow = 7,

        [InspectorName("Cloud Cookie")]
        CloudCookie = 8,

        [InspectorName("Direct Light Response")]
        DirectLightResponse = 9,

        [InspectorName("Combined Lighting")]
        CombinedLighting = 10
    }

}
