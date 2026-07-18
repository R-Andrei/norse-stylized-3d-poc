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
            float creaseBranching)
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
        public float CreaseAmount { get; }
        public float CreaseWidth { get; }
        public float CreaseLength { get; }
        public float CreaseBranching { get; }
    }
}
