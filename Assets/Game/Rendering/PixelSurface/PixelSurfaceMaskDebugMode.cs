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
        GroundCombined = 14,

        // Authored ground modifier standing-water/puddle-potential mask.
        // Kept outside the existing 15-26 generated-mass diagnostic range so
        // those serialized debug values remain stable.
        GroundStandingWaterPotential = 27,

        // Generated accepted projected-glyph coverage. This is the sole active
        // Painted Accent shader diagnostic after legacy fold-field retirement.
        GroundPaintedAccentLines = 28,

        // GeneratedMass generic boundary-atlas diagnostics. These are
        // intentionally placed after the ground debug modes so existing
        // serialized GeneratedMass debug values 0-6 remain stable.
        ConvexBoundaryProximity = 15,
        ConcaveBoundaryProximity = 16,
        ConvexBoundarySalienceComposite = 17,
        BoundarySalience = 18,
        BoundaryIdentity = 19,
        ConcaveBoundarySalienceComposite = 20,
        BoundaryFieldDiagnostic = 21,
        BoundaryModulationDiagnostic = 22,
        BoundaryAlongCoordinate = 23,
        BoundaryCrossCoordinate = 24,
        BoundaryCoarseModulation = 25,
        BoundaryFineModulation = 26
    }
}
