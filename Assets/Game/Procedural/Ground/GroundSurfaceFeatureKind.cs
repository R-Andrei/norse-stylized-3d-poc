using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundSurfaceFeatureKind
    {
        None = 0,

        [InspectorName("Directional Streaks")]
        DirectionalStreaks = 10,

        [InspectorName("Melt Patches")]
        MeltPatches = 11,

        [InspectorName("Pooled Wetness")]
        PooledWetness = 12,

        [InspectorName("Painted Accent Lines")]
        PaintedAccentLines = 20,

        [InspectorName("Pebble Scatter")]
        PebbleScatter = 30,

        [InspectorName("Mud Crust Cracks")]
        MudCrustCracks = 31,

        [InspectorName("Trampled Wear")]
        TrampledWear = 32,

        [InspectorName("Frosted Rock Dust")]
        FrostedRockDust = 33
    }
}
