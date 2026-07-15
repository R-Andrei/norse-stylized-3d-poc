using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
[Serializable]
public sealed class GroundMaterialControls
{
    [Header("Palette")]
    [Tooltip("Per-ground base colour override applied through a material property block.")]
    [SerializeField]
    private Color baseColor = new Color(0.807f, 0.870f, 0.906f, 1f);

    [Tooltip("Tint used by snow/frost-capable ground response.")]
    [SerializeField]
    private Color frostColor = new Color(0.82f, 0.93f, 1f, 1f);

    [Tooltip("Colour target used by damp/deposit-biased ground.")]
    [SerializeField]
    private Color dampTint = new Color(0.47f, 0.42f, 0.34f, 1f);

    [Tooltip("How strongly damp/deposit patches shift toward Damp Tint.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float dampTintStrength = 0.20f;

    [Tooltip("Colour target used by rocky/dry secondary patches.")]
    [SerializeField]
    private Color rockyDryTint = new Color(0.68f, 0.70f, 0.68f, 1f);

    [Tooltip("How strongly rocky/dry patches shift toward Rocky/Dry Tint.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float rockyDryTintStrength = 0.18f;

    [Tooltip("Colour target reserved for future vegetation/moss-friendly ground response.")]
    [SerializeField]
    private Color vegetationTint = new Color(0.50f, 0.58f, 0.42f, 1f);

    [Tooltip("How strongly vegetation-suitable patches shift toward Vegetation Tint.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float vegetationTintStrength = 0.10f;

    [Header("Pixel and Macro Variation")]
    [Tooltip("World-space size of individual pixel-surface cells.")]
    [Range(0.005f, 0.5f)]
    [SerializeField]
    private float pixelCellSize = 0.055f;

    [Tooltip("Number of tonal steps available to pixel-cell variation.")]
    [Range(2f, 8f)]
    [SerializeField]
    private float pixelToneCount = 4.67f;

    [Tooltip("How much neighbouring pixel cells cluster into larger same-tone groups.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float pixelClusterStrength = 0.576f;

    [Tooltip("Fine pixel-cell value variation used by the ground shader.")]
    [Range(0f, 0.25f)]
    [SerializeField]
    private float pixelVariation = 0.024f;

    [InspectorName("Macro Patch Intensity")]
    [Tooltip("Visible tonal intensity of shader-side macro patches. This is independent from fine pixel and generated vertex variation.")]
    [Range(0f, 0.1f)]
    [SerializeField]
    private float broadVariation = 0.032f;

    [Tooltip("How strongly generated mesh vertex colour variation affects the ground material response.")]
    [Range(0f, 0.25f)]
    [SerializeField]
    private float vertexVariation = 0.2f;

    [Tooltip("Overall multiplier for combined pixel, vertex, and broad variation.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float pixelEffectStrength = 1f;

    [Tooltip("How much the pixel-cell lookup is spatially warped.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float cellWarpStrength = 0.08f;

    [Tooltip("Metre scale of broad ground material patches.")]
    [Range(0.5f, 12f)]
    [SerializeField]
    private float groundMacroPatchScale = 4.5f;

    [InspectorName("Macro Patch Transition Softness")]
    [Tooltip("Softness of the transition between neutral terrain and light or dark macro patches. Zero gives narrow edges; one gives broad seamless blending.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float groundMacroPatchTransitionSoftness = 0.75f;

    [Header("Semantic Response")]
    [Tooltip("Multiplier for profile-driven broad response contrast.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float profileContrastScale = 1f;

    [Tooltip("Multiplier for profile-driven pixel response contrast.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float profilePixelContrastScale = 1f;

    [Tooltip("Multiplier for snow/exposure response inherited from the surface profile.")]
    [Range(0f, 2.5f)]
    [SerializeField]
    private float groundSnowResponseScale = 1f;

    [Tooltip("Multiplier for damp/deposit response inherited from the surface profile.")]
    [Range(0f, 2.5f)]
    [SerializeField]
    private float groundDampResponseScale = 1f;

    [Tooltip("Multiplier for vegetation-suitability response inherited from the surface profile.")]
    [Range(0f, 2.5f)]
    [SerializeField]
    private float groundVegetationResponseScale = 1f;

    [Tooltip("Multiplier for rocky/dry response inherited from the surface profile.")]
    [Range(0f, 2.5f)]
    [SerializeField]
    private float groundRockyDryResponseScale = 1f;

    [Tooltip("Multiplier for shore dampening inherited from the surface profile.")]
    [Range(0f, 2.5f)]
    [SerializeField]
    private float groundShoreDampStrengthScale = 1f;

    [Tooltip("Strength of broad snow/damp/rocky patch blending in the final ground response.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float groundPatchBlendStrength = 0.52f;

    [Tooltip("How strongly snow-capable ground shifts toward the frost colour.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float groundSnowTintStrength = 0.74f;

    [Tooltip("Value lift applied to snow-capable exposed ground.")]
    [Range(0f, 0.5f)]
    [SerializeField]
    private float groundSnowBrightness = 0.14f;

    [Tooltip("Value darkening applied to damp/deposit-biased ground.")]
    [Range(0f, 0.75f)]
    [SerializeField]
    private float groundDampDarkenStrength = 0.34f;

    [Header("Weather and Finish")]
    [Tooltip("Global wetness response for this ground style.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float wetness = 0f;

    [Tooltip("How much global wetness darkens the ground.")]
    [Range(0f, 0.75f)]
    [SerializeField]
    private float wetDarkenStrength = 0.22f;

    [Tooltip("How much wetness suppresses pixel contrast.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float wetPixelSoftening = 0.55f;

    [Tooltip("How much wetness increases smoothness.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float wetSmoothnessBoost = 0.35f;

    [Tooltip("Frost material response strength used for contrast/smoothness shaping.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float frostStrength = 0f;

    [Tooltip("Frost contrast shaping applied to ground response.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float frostContrast = 1f;

    [Tooltip("Blends the ground toward a flatter broad snow/ice plate response.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float monolithicFlatten = 0f;

    [Tooltip("Smoothness boost applied when monolithic flattening is active.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float monolithicSmoothnessBoost = 0.18f;

    [Tooltip("Base smoothness used by the ground material.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float smoothness = 0.20f;

    [Tooltip("Specular strength used by the ground material.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float specularStrength = 0.16f;

    public Color BaseColor => ResolveColor(
        baseColor,
        new Color(0.807f, 0.870f, 0.906f, 1f));
    public Color FrostColor => ResolveColor(
        frostColor,
        new Color(0.82f, 0.93f, 1.00f, 1f));
    public Color DampTint => ResolveColor(
        dampTint,
        new Color(0.47f, 0.42f, 0.34f, 1f));
    public float DampTintStrength => Mathf.Clamp01(dampTintStrength);
    public Color RockyDryTint => ResolveColor(
        rockyDryTint,
        new Color(0.68f, 0.70f, 0.68f, 1f));
    public float RockyDryTintStrength => Mathf.Clamp01(rockyDryTintStrength);
    public Color VegetationTint => ResolveColor(
        vegetationTint,
        new Color(0.50f, 0.58f, 0.42f, 1f));
    public float VegetationTintStrength => Mathf.Clamp01(vegetationTintStrength);
    public float PixelCellSize => Mathf.Clamp(pixelCellSize, 0.005f, 0.5f);
    public float PixelToneCount => Mathf.Clamp(pixelToneCount, 2f, 8f);
    public float PixelClusterStrength => Mathf.Clamp01(pixelClusterStrength);
    public float PixelVariation => Mathf.Clamp(pixelVariation, 0f, 0.25f);
    public float BroadVariation => Mathf.Clamp(broadVariation, 0f, 0.1f);
    public float VertexVariation => Mathf.Clamp(vertexVariation, 0f, 0.25f);
    public float PixelEffectStrength => Mathf.Clamp(pixelEffectStrength, 0f, 2f);
    public float CellWarpStrength => Mathf.Clamp(cellWarpStrength, 0f, 2f);
    public float GroundMacroPatchScale => Mathf.Clamp(groundMacroPatchScale, 0.5f, 12f);
    public float GroundMacroPatchTransitionSoftness =>
        Mathf.Clamp01(groundMacroPatchTransitionSoftness);
    public float ProfileContrastScale => Mathf.Clamp(profileContrastScale, 0f, 2f);
    public float ProfilePixelContrastScale => Mathf.Clamp(profilePixelContrastScale, 0f, 2f);
    public float GroundSnowResponseScale => Mathf.Clamp(groundSnowResponseScale, 0f, 2.5f);
    public float GroundDampResponseScale => Mathf.Clamp(groundDampResponseScale, 0f, 2.5f);
    public float GroundVegetationResponseScale => Mathf.Clamp(groundVegetationResponseScale, 0f, 2.5f);
    public float GroundRockyDryResponseScale => Mathf.Clamp(groundRockyDryResponseScale, 0f, 2.5f);
    public float GroundShoreDampStrengthScale => Mathf.Clamp(groundShoreDampStrengthScale, 0f, 2.5f);
    public float GroundPatchBlendStrength => Mathf.Clamp01(groundPatchBlendStrength);
    public float GroundSnowTintStrength => Mathf.Clamp01(groundSnowTintStrength);
    public float GroundSnowBrightness => Mathf.Clamp(groundSnowBrightness, 0f, 0.5f);
    public float GroundDampDarkenStrength => Mathf.Clamp(groundDampDarkenStrength, 0f, 0.75f);
    public float Wetness => Mathf.Clamp01(wetness);
    public float WetDarkenStrength => Mathf.Clamp(wetDarkenStrength, 0f, 0.75f);
    public float WetPixelSoftening => Mathf.Clamp01(wetPixelSoftening);
    public float WetSmoothnessBoost => Mathf.Clamp01(wetSmoothnessBoost);
    public float FrostStrength => Mathf.Clamp01(frostStrength);
    public float FrostContrast => Mathf.Clamp(frostContrast, 0f, 2f);
    public float MonolithicFlatten => Mathf.Clamp01(monolithicFlatten);
    public float MonolithicSmoothnessBoost => Mathf.Clamp01(monolithicSmoothnessBoost);
    public float Smoothness => Mathf.Clamp01(smoothness);
    public float SpecularStrength => Mathf.Clamp01(specularStrength);

    private static Color ResolveColor(Color value, Color fallback)
    {
        float strongestChannel =
            Mathf.Max(value.r, Mathf.Max(value.g, value.b));

        if (value.a <= 0f && strongestChannel <= 0f)
        {
            return fallback;
        }

        return value;
    }

    public void CopyFrom(GroundMaterialControls source)
    {
        if (source == null)
        {
            ApplySnowfieldVariant(GroundSnowfieldVariant.Clean);
            return;
        }

        baseColor = source.baseColor;
        frostColor = source.frostColor;
        dampTint = source.dampTint;
        dampTintStrength = source.dampTintStrength;
        rockyDryTint = source.rockyDryTint;
        rockyDryTintStrength = source.rockyDryTintStrength;
        vegetationTint = source.vegetationTint;
        vegetationTintStrength = source.vegetationTintStrength;
        pixelCellSize = source.pixelCellSize;
        pixelToneCount = source.pixelToneCount;
        pixelClusterStrength = source.pixelClusterStrength;
        pixelVariation = source.pixelVariation;
        broadVariation = source.broadVariation;
        vertexVariation = source.vertexVariation;
        pixelEffectStrength = source.pixelEffectStrength;
        cellWarpStrength = source.cellWarpStrength;
        groundMacroPatchScale = source.groundMacroPatchScale;
        groundMacroPatchTransitionSoftness =
            source.groundMacroPatchTransitionSoftness;
        profileContrastScale = source.profileContrastScale;
        profilePixelContrastScale = source.profilePixelContrastScale;
        groundSnowResponseScale = source.groundSnowResponseScale;
        groundDampResponseScale = source.groundDampResponseScale;
        groundVegetationResponseScale = source.groundVegetationResponseScale;
        groundRockyDryResponseScale = source.groundRockyDryResponseScale;
        groundShoreDampStrengthScale = source.groundShoreDampStrengthScale;
        groundPatchBlendStrength = source.groundPatchBlendStrength;
        groundSnowTintStrength = source.groundSnowTintStrength;
        groundSnowBrightness = source.groundSnowBrightness;
        groundDampDarkenStrength = source.groundDampDarkenStrength;
        wetness = source.wetness;
        wetDarkenStrength = source.wetDarkenStrength;
        wetPixelSoftening = source.wetPixelSoftening;
        wetSmoothnessBoost = source.wetSmoothnessBoost;
        frostStrength = source.frostStrength;
        frostContrast = source.frostContrast;
        monolithicFlatten = source.monolithicFlatten;
        monolithicSmoothnessBoost = source.monolithicSmoothnessBoost;
        smoothness = source.smoothness;
        specularStrength = source.specularStrength;
    }

    public void ApplySnowfieldVariant(GroundSnowfieldVariant variant)
    {
        switch (variant)
        {
            case GroundSnowfieldVariant.Clean:
                ApplyCleanSnowfield();
                break;

            case GroundSnowfieldVariant.Patchy:
                ApplyPatchySnowfield();
                break;

            case GroundSnowfieldVariant.DirtyThawing:
                ApplyDirtyThawingSnowfield();
                break;

            case GroundSnowfieldVariant.WindScoured:
                ApplyWindScouredSnowfield();
                break;
        }
    }

    private void ApplyCleanSnowfield()
    {
        baseColor = new Color(0.807f, 0.870f, 0.906f, 1f);
        frostColor = new Color(0.82f, 0.93f, 1.00f, 1f);
        dampTint = new Color(0.47f, 0.42f, 0.34f, 1f);
        dampTintStrength = 0.16f;
        rockyDryTint = new Color(0.68f, 0.70f, 0.68f, 1f);
        rockyDryTintStrength = 0.10f;
        vegetationTint = new Color(0.50f, 0.58f, 0.42f, 1f);
        vegetationTintStrength = 0.04f;
        pixelCellSize = 0.055f;
        pixelToneCount = 4.67f;
        pixelClusterStrength = 0.576f;
        pixelVariation = 0.020f;
        broadVariation = 0.032f;
        vertexVariation = 0.18f;
        pixelEffectStrength = 0.92f;
        cellWarpStrength = 0.08f;
        groundMacroPatchScale = 4.8f;
        groundMacroPatchTransitionSoftness = 0.75f;
        profileContrastScale = 0.95f;
        profilePixelContrastScale = 0.92f;
        groundSnowResponseScale = 1.10f;
        groundDampResponseScale = 0.72f;
        groundVegetationResponseScale = 0.45f;
        groundRockyDryResponseScale = 0.55f;
        groundShoreDampStrengthScale = 0.85f;
        groundPatchBlendStrength = 0.52f;
        groundSnowTintStrength = 0.74f;
        groundSnowBrightness = 0.16f;
        groundDampDarkenStrength = 0.24f;
        wetness = 0f;
        wetDarkenStrength = 0.18f;
        wetPixelSoftening = 0.55f;
        wetSmoothnessBoost = 0.26f;
        frostStrength = 0.10f;
        frostContrast = 1.05f;
        monolithicFlatten = 0.06f;
        monolithicSmoothnessBoost = 0.18f;
        smoothness = 0.20f;
        specularStrength = 0.14f;
    }

    private void ApplyPatchySnowfield()
    {
        baseColor = new Color(0.760f, 0.825f, 0.845f, 1f);
        frostColor = new Color(0.80f, 0.91f, 0.98f, 1f);
        dampTint = new Color(0.42f, 0.39f, 0.34f, 1f);
        dampTintStrength = 0.30f;
        rockyDryTint = new Color(0.56f, 0.57f, 0.55f, 1f);
        rockyDryTintStrength = 0.34f;
        vegetationTint = new Color(0.48f, 0.56f, 0.39f, 1f);
        vegetationTintStrength = 0.08f;
        pixelCellSize = 0.055f;
        pixelToneCount = 4.2f;
        pixelClusterStrength = 0.68f;
        pixelVariation = 0.024f;
        broadVariation = 0.095f;
        vertexVariation = 0.25f;
        pixelEffectStrength = 1.18f;
        cellWarpStrength = 0.18f;
        groundMacroPatchScale = 3.25f;
        groundMacroPatchTransitionSoftness = 0.75f;
        profileContrastScale = 1.35f;
        profilePixelContrastScale = 1.16f;
        groundSnowResponseScale = 1.00f;
        groundDampResponseScale = 1.10f;
        groundVegetationResponseScale = 0.70f;
        groundRockyDryResponseScale = 1.45f;
        groundShoreDampStrengthScale = 1.15f;
        groundPatchBlendStrength = 0.88f;
        groundSnowTintStrength = 0.70f;
        groundSnowBrightness = 0.12f;
        groundDampDarkenStrength = 0.42f;
        wetness = 0.04f;
        wetDarkenStrength = 0.26f;
        wetPixelSoftening = 0.45f;
        wetSmoothnessBoost = 0.30f;
        frostStrength = 0.06f;
        frostContrast = 1.20f;
        monolithicFlatten = 0f;
        monolithicSmoothnessBoost = 0.18f;
        smoothness = 0.18f;
        specularStrength = 0.15f;
    }

    private void ApplyDirtyThawingSnowfield()
    {
        baseColor = new Color(0.675f, 0.700f, 0.665f, 1f);
        frostColor = new Color(0.74f, 0.80f, 0.82f, 1f);
        dampTint = new Color(0.38f, 0.30f, 0.22f, 1f);
        dampTintStrength = 0.72f;
        rockyDryTint = new Color(0.50f, 0.47f, 0.42f, 1f);
        rockyDryTintStrength = 0.38f;
        vegetationTint = new Color(0.42f, 0.50f, 0.34f, 1f);
        vegetationTintStrength = 0.14f;
        pixelCellSize = 0.060f;
        pixelToneCount = 3.8f;
        pixelClusterStrength = 0.72f;
        pixelVariation = 0.036f;
        broadVariation = 0.085f;
        vertexVariation = 0.24f;
        pixelEffectStrength = 1.08f;
        cellWarpStrength = 0.24f;
        groundMacroPatchScale = 3.1f;
        groundMacroPatchTransitionSoftness = 0.75f;
        profileContrastScale = 1.20f;
        profilePixelContrastScale = 1.02f;
        groundSnowResponseScale = 0.62f;
        groundDampResponseScale = 1.95f;
        groundVegetationResponseScale = 1.05f;
        groundRockyDryResponseScale = 1.05f;
        groundShoreDampStrengthScale = 1.80f;
        groundPatchBlendStrength = 0.90f;
        groundSnowTintStrength = 0.36f;
        groundSnowBrightness = 0.05f;
        groundDampDarkenStrength = 0.72f;
        wetness = 0.24f;
        wetDarkenStrength = 0.42f;
        wetPixelSoftening = 0.58f;
        wetSmoothnessBoost = 0.48f;
        frostStrength = 0.00f;
        frostContrast = 0.90f;
        monolithicFlatten = 0f;
        monolithicSmoothnessBoost = 0.18f;
        smoothness = 0.23f;
        specularStrength = 0.20f;
    }

    private void ApplyWindScouredSnowfield()
    {
        baseColor = new Color(0.840f, 0.900f, 0.930f, 1f);
        frostColor = new Color(0.86f, 0.96f, 1.00f, 1f);
        dampTint = new Color(0.52f, 0.51f, 0.48f, 1f);
        dampTintStrength = 0.08f;
        rockyDryTint = new Color(0.70f, 0.73f, 0.74f, 1f);
        rockyDryTintStrength = 0.12f;
        vegetationTint = new Color(0.54f, 0.62f, 0.45f, 1f);
        vegetationTintStrength = 0.02f;
        pixelCellSize = 0.052f;
        pixelToneCount = 5.5f;
        pixelClusterStrength = 0.44f;
        pixelVariation = 0.010f;
        broadVariation = 0.018f;
        vertexVariation = 0.10f;
        pixelEffectStrength = 0.72f;
        cellWarpStrength = 0.035f;
        groundMacroPatchScale = 8.6f;
        groundMacroPatchTransitionSoftness = 0.75f;
        profileContrastScale = 0.70f;
        profilePixelContrastScale = 0.70f;
        groundSnowResponseScale = 1.38f;
        groundDampResponseScale = 0.34f;
        groundVegetationResponseScale = 0.20f;
        groundRockyDryResponseScale = 0.35f;
        groundShoreDampStrengthScale = 0.45f;
        groundPatchBlendStrength = 0.34f;
        groundSnowTintStrength = 0.88f;
        groundSnowBrightness = 0.22f;
        groundDampDarkenStrength = 0.12f;
        wetness = 0f;
        wetDarkenStrength = 0.12f;
        wetPixelSoftening = 0.70f;
        wetSmoothnessBoost = 0.22f;
        frostStrength = 0.18f;
        frostContrast = 1.12f;
        monolithicFlatten = 0.34f;
        monolithicSmoothnessBoost = 0.24f;
        smoothness = 0.26f;
        specularStrength = 0.12f;
    }
}

}
