using System.Runtime.InteropServices;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VegetationInstanceData
    {
        public const int Stride = 48;
        public static int RuntimeStride =>
            Marshal.SizeOf<VegetationInstanceData>();

        public Vector4 PositionYaw;
        public Vector4 ScaleStiffness;
        public Vector4 VariationPhase;

        public VegetationInstanceData(
            Vector3 localPosition,
            float yawRadians,
            Vector2 scale,
            float stiffness,
            float phase,
            float colorVariation,
            float bladeVariation)
        {
            PositionYaw = new Vector4(
                localPosition.x,
                localPosition.y,
                localPosition.z,
                yawRadians);
            ScaleStiffness = new Vector4(
                scale.x,
                scale.y,
                stiffness,
                0f);
            VariationPhase = new Vector4(
                phase,
                colorVariation,
                bladeVariation,
                0f);
        }
    }
}
