namespace ProgrammaticStylized3D.Rendering
{
    /// <summary>
    /// Shared material inspector enum for SH_PixelSurfaceLit mask debugging.
    /// Unity's inline shader [Enum(name,value,...)] drawer has a limited
    /// constructor arity, so this C# enum keeps the shader property readable
    /// without splitting the debug selector into multiple material controls.
    /// Values must stay synchronized with SH_PixelSurfaceLit.shader.
    /// </summary>
    public enum PixelSurfaceMaskDebugMode
    {
        None = 0,
        SurfaceVariation = 1,
        Exposure = 2,
        CreviceBase = 3,
        ConvexEdgeWear = 4,
        ConcaveCrease = 5,
        DirtDeposit = 6,
        GroundTonal = 7,
        GroundExposure = 8,
        GroundDampDeposit = 9,
        GroundVegetation = 10,
        GroundCompaction = 11,
        GroundShore = 12,
        GroundRockyDry = 13,
        GroundCombined = 14
    }
}
