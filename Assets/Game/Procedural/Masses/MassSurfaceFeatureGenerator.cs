using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public readonly struct MassSurfaceFeatureSettings
    {
        public MassSurfaceFeatureSettings(
            MassArchetype archetype,
            int surfaceSeed,
            float edgeWearAmount,
            float edgeWearWidth,
            float edgeWearCoverage,
            float edgeWearMacroVariationCoverage,
            float edgeWearMacroVariation,
            float edgeWearSoftness,
            float creaseAmount,
            float creaseWidth,
            float creaseLength,
            float creaseBranching,
            bool cornerChippingEnabled = false,
            float cornerChipDepth = 0.18f,
            float cornerChipDepthVariation = 0.15f,
            float cornerChipTopFacingPreference = 0.65f,
            float cornerChipCapRingWidthScale = 0.75f,
            float cornerChipCapRingWearStrength = 1f)
        {
            Archetype = archetype;
            SurfaceSeed = surfaceSeed;
            EdgeWearAmount = Mathf.Clamp(edgeWearAmount, 0f, 2f);
            EdgeWearWidth = Mathf.Clamp(edgeWearWidth, 0.05f, 2f);
            EdgeWearCoverage = Mathf.Clamp(edgeWearCoverage, 0.1f, 2f);
            EdgeWearMacroVariationCoverage =
                Mathf.Clamp01(edgeWearMacroVariationCoverage);
            EdgeWearMacroVariation =
                Mathf.Clamp01(edgeWearMacroVariation);
            EdgeWearSoftness = Mathf.Clamp01(edgeWearSoftness);
            CornerChippingEnabled = cornerChippingEnabled;
            CornerChipDepth = Mathf.Clamp(cornerChipDepth, 0.04f, 0.35f);
            CornerChipDepthVariation = Mathf.Clamp(
                cornerChipDepthVariation,
                0f,
                0.50f);
            CornerChipTopFacingPreference = Mathf.Clamp01(
                cornerChipTopFacingPreference);
            CornerChipCapRingWidthScale = Mathf.Clamp(
                cornerChipCapRingWidthScale,
                0.20f,
                1.25f);
            CornerChipCapRingWearStrength = Mathf.Clamp(
                cornerChipCapRingWearStrength,
                0f,
                1.50f);
            CreaseAmount = Mathf.Clamp(creaseAmount, 0f, 2f);
            CreaseWidth = Mathf.Clamp(creaseWidth, 0.25f, 2f);
            CreaseLength = Mathf.Clamp(creaseLength, 0.25f, 2f);
            CreaseBranching = Mathf.Clamp(creaseBranching, 0f, 2f);
        }

        public MassArchetype Archetype { get; }
        public int SurfaceSeed { get; }
        public float EdgeWearAmount { get; }
        public float EdgeWearWidth { get; }
        public float EdgeWearCoverage { get; }
        public float EdgeWearMacroVariationCoverage { get; }
        public float EdgeWearMacroVariation { get; }
        public float EdgeWearSoftness { get; }
        public bool CornerChippingEnabled { get; }
        public float CornerChipDepth { get; }
        public float CornerChipDepthVariation { get; }
        public float CornerChipTopFacingPreference { get; }
        public float CornerChipCapRingWidthScale { get; }
        public float CornerChipCapRingWearStrength { get; }
        public float CreaseAmount { get; }
        public float CreaseWidth { get; }
        public float CreaseLength { get; }
        public float CreaseBranching { get; }
    }
}
