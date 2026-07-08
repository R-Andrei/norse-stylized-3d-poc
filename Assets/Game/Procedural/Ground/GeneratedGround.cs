using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ProgrammaticStylized3D.Geometry;
using ProgrammaticStylized3D.Rivers;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundPatchSize
    {
        [InspectorName("Compact")]
        Compact20,

        [InspectorName("Standard")]
        Standard40,

        [InspectorName("Large")]
        Large60,

        [InspectorName("Huge")]
        Huge80
    }
    

    public enum GroundResolution
    {
        [InspectorName("Low")]
        Low17,

        [InspectorName("Medium")]
        Medium33,

        [InspectorName("High")]
        High65,

        [InspectorName("Very High")]
        VeryHigh129
    }

    public enum GroundProfile
    {
        Flat,
        Rolling,
        Basin,
        Ridge,
        Uneven
    }

    public enum GroundTransitionDirection
    {
        None,
        North,
        South,
        East,
        West,
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest
    }

    public enum GroundEdgeBlend
    {
        Narrow,
        Medium,
        Wide
    }

    public enum GroundSurfaceType
    {
        [InspectorName("Snowfield")]
        Snowfield = 0
    }

    public enum GroundSnowfieldVariant
    {
        Custom = 0,

        [InspectorName("Clean")]
        Clean = 1,

        [InspectorName("Patchy")]
        Patchy = 2,

        [InspectorName("Dirty Thawing")]
        DirtyThawing = 3,

        [InspectorName("Wind-Scoured")]
        WindScoured = 4
    }

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

        [Tooltip("Broad terrain-scale tonal variation mixed into the ground shader.")]
        [Range(0f, 0.25f)]
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
        public float BroadVariation => Mathf.Clamp(broadVariation, 0f, 0.25f);
        public float VertexVariation => Mathf.Clamp(vertexVariation, 0f, 0.25f);
        public float PixelEffectStrength => Mathf.Clamp(pixelEffectStrength, 0f, 2f);
        public float CellWarpStrength => Mathf.Clamp(cellWarpStrength, 0f, 2f);
        public float GroundMacroPatchScale => Mathf.Clamp(groundMacroPatchScale, 0.5f, 12f);
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

    [Serializable]
    public sealed class GroundRecipe
    {
        public const int MinimumSeed = 1;
        public const int MaximumSeed = 9999;

        [Tooltip("Controls the deterministic broad form and surface detail.")]
        [Range(MinimumSeed, MaximumSeed)]
        [SerializeField]
        private int shapeSeed = 1234;

        [SerializeField]
        private GroundPatchSize patchSize = GroundPatchSize.Standard40;

        [SerializeField]
        private GroundResolution resolution = GroundResolution.Medium33;

        [Tooltip("Used to vary stable noise between neighbouring generated patches.")]
        [SerializeField]
        private Vector2Int patchCoordinate = Vector2Int.zero;

        [SerializeField]
        private GroundTransitionDirection transitionDirection =
            GroundTransitionDirection.None;

        [Tooltip("Height difference, in metres, from the low side to the high side.")]
        [Range(-12f, 12f)]
        [SerializeField]
        private float transitionHeight = 0f;

        [SerializeField]
        private GroundProfile profile = GroundProfile.Rolling;

        [Tooltip("Height, in metres, contributed by the selected broad profile.")]
        [Range(0f, 6f)]
        [SerializeField]
        private float broadForm = 1.4f;

        [Tooltip("Controls how busy the broad and detail noise becomes.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float roughness = 0.35f;

        [Tooltip("Small-scale height variation. Kept deliberately restrained.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float surfaceDetail = 0.25f;

        [Tooltip("How far terrain variation fades before reaching patch borders.")]
        [SerializeField]
        private GroundEdgeBlend edgeBlend = GroundEdgeBlend.Medium;

        [Tooltip("Amount of deterministic broad variation written to vertex colour red.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float surfaceVariation = 0.35f;

        [SerializeField]
        private bool useModifiers = true;

        public int ShapeSeed => shapeSeed;
        public GroundPatchSize PatchSize => patchSize;
        public GroundResolution Resolution => resolution;
        public Vector2Int PatchCoordinate => patchCoordinate;
        public GroundTransitionDirection TransitionDirection => transitionDirection;
        public float TransitionHeight => transitionHeight;
        public GroundProfile Profile => profile;
        public float BroadForm => broadForm;
        public float Roughness => roughness;
        public float SurfaceDetail => surfaceDetail;
        public GroundEdgeBlend EdgeBlend => edgeBlend;
        public float SurfaceVariation => surfaceVariation;
        public bool UseModifiers => useModifiers;

        public void SetShapeSeed(int value)
        {
            shapeSeed = Mathf.Clamp(value, MinimumSeed, MaximumSeed);
        }
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public sealed class GeneratedGround : MonoBehaviour
    {
        [SerializeField]
        private GroundRecipe recipe = new GroundRecipe();

        [Tooltip("Surface/material identity used to generate deterministic ground masks. When empty, built-in defaults are used so old scenes do not require a profile asset.")]
        [SerializeField]
        private GroundSurfaceProfile surfaceProfile;

        [Tooltip("High-level ground visual family. This is intentionally separate from the height profile and from the surface mask profile.")]
        [SerializeField]
        private GroundSurfaceType groundSurfaceType =
            GroundSurfaceType.Snowfield;

        [Tooltip("Variant inside the selected ground surface type. The selected variant writes a complete visual recipe into Advanced Material Controls.")]
        [FormerlySerializedAs("groundVisualPreset")]
        [SerializeField]
        private GroundSnowfieldVariant snowfieldVariant =
            GroundSnowfieldVariant.Clean;

        [SerializeField]
        private GroundMaterialControls groundMaterialControls =
            new GroundMaterialControls();

        [Tooltip("Regenerate when recipe values change in the Inspector.")]
        [SerializeField]
        private bool regenerateOnValidate = true;

        [SerializeField, HideInInspector]
        private GroundModifier[] modifiers = Array.Empty<GroundModifier>();

        [SerializeField, HideInInspector]
        private StylizedRiver[] rivers = Array.Empty<StylizedRiver>();

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private Mesh generatedMesh;
        private GroundHeightFieldSnapshot baseSurface =
            GroundHeightFieldSnapshot.Empty;
        [SerializeField, HideInInspector]
        private string lastSurfaceMaskDiagnostics =
            "Surface masks have not been generated yet.";
        [SerializeField, HideInInspector]
        private int lastValidatedGenerationSignature;
        private MaterialPropertyBlock materialProperties;

        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");
        private static readonly int SurfaceContractId =
            Shader.PropertyToID("_SurfaceContract");
        private static readonly int ProfileContrastId =
            Shader.PropertyToID("_ProfileContrast");
        private static readonly int ProfilePixelContrastId =
            Shader.PropertyToID("_ProfilePixelContrast");
        private static readonly int GroundSnowResponseId =
            Shader.PropertyToID("_GroundSnowResponse");
        private static readonly int GroundDampResponseId =
            Shader.PropertyToID("_GroundDampResponse");
        private static readonly int GroundVegetationResponseId =
            Shader.PropertyToID("_GroundVegetationResponse");
        private static readonly int GroundRockyDryResponseId =
            Shader.PropertyToID("_GroundRockyDryResponse");
        private static readonly int GroundShoreDampStrengthId =
            Shader.PropertyToID("_GroundShoreDampStrength");
        private static readonly int PixelCellSizeId =
            Shader.PropertyToID("_PixelCellSize");
        private static readonly int PixelToneCountId =
            Shader.PropertyToID("_PixelToneCount");
        private static readonly int PixelClusterStrengthId =
            Shader.PropertyToID("_PixelClusterStrength");
        private static readonly int PixelVariationId =
            Shader.PropertyToID("_PixelVariation");
        private static readonly int PixelBroadVariationId =
            Shader.PropertyToID("_PixelBroadVariation");
        private static readonly int PixelVertexVariationId =
            Shader.PropertyToID("_PixelVertexVariation");
        private static readonly int PixelEffectStrengthId =
            Shader.PropertyToID("_PixelEffectStrength");
        private static readonly int PixelWarpStrengthId =
            Shader.PropertyToID("_PixelWarpStrength");
        private static readonly int GroundPatchBlendStrengthId =
            Shader.PropertyToID("_GroundPatchBlendStrength");
        private static readonly int GroundMacroPatchScaleId =
            Shader.PropertyToID("_GroundMacroPatchScale");
        private static readonly int GroundSnowTintStrengthId =
            Shader.PropertyToID("_GroundSnowTintStrength");
        private static readonly int GroundSnowBrightnessId =
            Shader.PropertyToID("_GroundSnowBrightness");
        private static readonly int GroundDampDarkenStrengthId =
            Shader.PropertyToID("_GroundDampDarkenStrength");
        private static readonly int GroundDampTintId =
            Shader.PropertyToID("_GroundDampTint");
        private static readonly int GroundDampTintStrengthId =
            Shader.PropertyToID("_GroundDampTintStrength");
        private static readonly int GroundRockyDryTintId =
            Shader.PropertyToID("_GroundRockyDryTint");
        private static readonly int GroundRockyDryTintStrengthId =
            Shader.PropertyToID("_GroundRockyDryTintStrength");
        private static readonly int GroundVegetationTintId =
            Shader.PropertyToID("_GroundVegetationTint");
        private static readonly int GroundVegetationTintStrengthId =
            Shader.PropertyToID("_GroundVegetationTintStrength");
        private static readonly int WetnessId =
            Shader.PropertyToID("_Wetness");
        private static readonly int WetDarkenStrengthId =
            Shader.PropertyToID("_WetDarkenStrength");
        private static readonly int WetPixelSofteningId =
            Shader.PropertyToID("_WetPixelSoftening");
        private static readonly int WetSmoothnessBoostId =
            Shader.PropertyToID("_WetSmoothnessBoost");
        private static readonly int FrostStrengthId =
            Shader.PropertyToID("_FrostStrength");
        private static readonly int FrostContrastId =
            Shader.PropertyToID("_FrostContrast");
        private static readonly int FrostColorId =
            Shader.PropertyToID("_FrostColor");
        private static readonly int MonolithicFlattenId =
            Shader.PropertyToID("_MonolithicFlatten");
        private static readonly int MonolithicSmoothnessBoostId =
            Shader.PropertyToID("_MonolithicSmoothnessBoost");
        private static readonly int SmoothnessId =
            Shader.PropertyToID("_Smoothness");
        private static readonly int SpecularStrengthId =
            Shader.PropertyToID("_SpecularStrength");

        public GroundRecipe Recipe => recipe;
        public GroundSurfaceProfile SurfaceProfile => surfaceProfile;
        public GroundSurfaceType SurfaceType => groundSurfaceType;
        public GroundSnowfieldVariant SnowfieldVariant => snowfieldVariant;
        public int ModifierCount => modifiers != null ? modifiers.Length : 0;
        public int RiverCount => rivers != null ? rivers.Length : 0;
        public Material SharedMaterial =>
            meshRenderer != null
                ? meshRenderer.sharedMaterial
                : null;
        public string LastSurfaceMaskDiagnostics =>
            string.IsNullOrWhiteSpace(lastSurfaceMaskDiagnostics)
                ? "Surface masks have not been generated yet."
                : lastSurfaceMaskDiagnostics;
        public float PatchSize =>
            recipe != null
                ? GroundGenerator.ResolvePatchSize(recipe.PatchSize)
                : 40f;

        public float GridSpacing
        {
            get
            {
                if (recipe == null)
                {
                    return 0.5f;
                }

                float patchSize =
                    GroundGenerator.ResolvePatchSize(recipe.PatchSize);

                int resolution =
                    GroundGenerator.ResolveResolution(recipe.Resolution);

                return patchSize / Mathf.Max(1, resolution - 1);
            }
        }

        public float GridCellDiagonal => GridSpacing * 1.41421356237f;

        private void OnEnable()
        {
            CacheComponents();
            RefreshModifiers();
            Regenerate();
        }

        private void OnValidate()
        {
            CacheComponents();

            int generationSignature = CalculateGenerationSignature();

            if (!regenerateOnValidate)
            {
                RefreshSurfaceMaterialProperties();
                return;
            }

            if (lastValidatedGenerationSignature != generationSignature)
            {
                RefreshModifiers();
                Regenerate();
                return;
            }

            RefreshSurfaceMaterialProperties();
        }

        [ContextMenu("Regenerate Ground")]
        public void Regenerate()
        {
            CacheComponents();

            if (recipe == null)
            {
                ClearGeneratedAssignments();
                return;
            }

            EnsureGeneratedMesh();

            List<GroundModifierSnapshot> snapshots =
                BuildModifierSnapshots();

            List<StylizedRiverGroundSnapshot> riverSnapshots =
                BuildRiverSnapshots();

            MeshData meshData = GroundGenerator.Generate(
                recipe,
                surfaceProfile,
                snapshots,
                riverSnapshots,
                out baseSurface,
                out lastSurfaceMaskDiagnostics);

            string meshName =
                $"GeneratedGround_{recipe.PatchSize}_{recipe.Resolution}_Seed{recipe.ShapeSeed}";

            MeshBuilder.ApplyToMesh(
                meshData,
                generatedMesh,
                meshName);

            meshFilter.sharedMesh = generatedMesh;

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = generatedMesh;
            meshCollider.convex = false;

            lastValidatedGenerationSignature =
                CalculateGenerationSignature();

            ApplySurfaceProfileMaterialProperties();
            NotifyRiverCorridorsChanged();
        }

        [ContextMenu("New Ground Shape")]
        public void CreateNewShape()
        {
            recipe.SetShapeSeed(
                GenerateDifferentSeed(recipe.ShapeSeed));

            Regenerate();
        }

        [ContextMenu("Find Ground Modifiers")]
        public void RefreshModifiers()
        {
            modifiers = GetComponentsInChildren<GroundModifier>(true);

            Array.Sort(
                modifiers,
                (left, right) =>
                    left.PriorityValue.CompareTo(right.PriorityValue));

            rivers = GetComponentsInChildren<StylizedRiver>(true);
        }

        public void NotifyModifierChanged(GroundModifier modifier)
        {
            if (modifier == null ||
                !modifier.transform.IsChildOf(transform))
            {
                return;
            }

            RefreshModifiers();
            Regenerate();
        }

        public void NotifyRiverChanged(StylizedRiver river)
        {
            if (river == null ||
                (river.transform != transform &&
                 !river.transform.IsChildOf(transform)))
            {
                return;
            }

            RefreshModifiers();
            Regenerate();
        }

        public void ApplySnowfieldVariant(GroundSnowfieldVariant variant)
        {
            groundMaterialControls ??= new GroundMaterialControls();
            groundSurfaceType = GroundSurfaceType.Snowfield;
            snowfieldVariant = variant;
            groundMaterialControls.ApplySnowfieldVariant(variant);
            RefreshSurfaceMaterialProperties();
        }

        public void MarkGroundVisualControlsCustom()
        {
            if (groundSurfaceType == GroundSurfaceType.Snowfield &&
                snowfieldVariant != GroundSnowfieldVariant.Custom)
            {
                snowfieldVariant = GroundSnowfieldVariant.Custom;
            }

            RefreshSurfaceMaterialProperties();
        }

        public void RefreshSurfaceMaterialProperties()
        {
            CacheComponents();
            ApplySurfaceProfileMaterialProperties();
            RefreshRiverCorridorMaterialProperties();
        }

        public bool TrySampleBaseSurface(
            Vector3 worldPosition,
            out float height,
            out Vector3 normal)
        {
            bool succeeded =
                TrySampleBaseSurface(
                    worldPosition,
                    out GroundSurfaceSample sample);

            height = sample.Height;
            normal = sample.Normal;
            return succeeded;
        }

        public bool TrySampleBaseSurface(
            Vector3 worldPosition,
            out GroundSurfaceSample sample)
        {
            sample =
                new GroundSurfaceSample(
                    0f,
                    Vector3.up,
                    0.5f,
                    0f);

            if (baseSurface == null || !baseSurface.IsValid)
            {
                return false;
            }

            Vector3 localPoint =
                transform.InverseTransformPoint(worldPosition);

            if (!baseSurface.TrySample(
                    new Vector2(localPoint.x, localPoint.z),
                    out GroundSurfaceSample localSample))
            {
                return false;
            }

            Vector3 worldPoint =
                transform.TransformPoint(
                    new Vector3(
                        localPoint.x,
                        localSample.Height,
                        localPoint.z));

            sample =
                new GroundSurfaceSample(
                    worldPoint.y,
                    transform
                        .TransformDirection(localSample.Normal)
                        .normalized,
                    transform
                        .TransformDirection(localSample.RenderNormal)
                        .normalized,
                    localSample.SurfaceVariation,
                    localSample.Exposure,
                    localSample.DampDeposit,
                    localSample.VegetationSuitability,
                    new Vector4(
                        localSample.Compaction,
                        localSample.ShoreInfluence,
                        localSample.RockyDry,
                        localSample.ReservedSurfaceMask),
                    localSample.MaterialClassification);

            return true;
        }

        public bool TrySampleSurface(
            Vector3 worldPosition,
            out float height,
            out Vector3 normal)
        {
            CacheComponents();

            if (meshCollider == null ||
                meshCollider.sharedMesh == null)
            {
                height = 0f;
                normal = Vector3.up;
                return false;
            }

            Bounds bounds = meshCollider.bounds;
            float rayStartHeight =
                Mathf.Max(worldPosition.y, bounds.max.y + 5f);

            Ray ray = new Ray(
                new Vector3(
                    worldPosition.x,
                    rayStartHeight,
                    worldPosition.z),
                Vector3.down);

            float maximumDistance =
                Mathf.Max(10f, rayStartHeight - bounds.min.y + 10f);

            if (meshCollider.Raycast(
                    ray,
                    out RaycastHit hit,
                    maximumDistance))
            {
                height = hit.point.y;
                normal = hit.normal;
                return true;
            }

            height = 0f;
            normal = Vector3.up;
            return false;
        }

        private List<GroundModifierSnapshot> BuildModifierSnapshots()
        {
            List<GroundModifierSnapshot> snapshots =
                new List<GroundModifierSnapshot>();

            if (!recipe.UseModifiers ||
                modifiers == null)
            {
                return snapshots;
            }

            for (int i = 0; i < modifiers.Length; i++)
            {
                GroundModifier modifier = modifiers[i];

                if (modifier == null ||
                    !modifier.isActiveAndEnabled)
                {
                    continue;
                }

                snapshots.Add(
                    modifier.CreateSnapshot(transform));
            }

            snapshots.Sort(
                (left, right) =>
                    left.Priority.CompareTo(right.Priority));

            return snapshots;
        }

        private List<StylizedRiverGroundSnapshot> BuildRiverSnapshots()
        {
            List<StylizedRiverGroundSnapshot> snapshots =
                new List<StylizedRiverGroundSnapshot>();

            if (rivers == null)
            {
                return snapshots;
            }

            for (int index = 0;
                 index < rivers.Length;
                 index++)
            {
                StylizedRiver river = rivers[index];

                if (river == null ||
                    !river.isActiveAndEnabled)
                {
                    continue;
                }

                StylizedRiverGroundSnapshot snapshot =
                    river.CreateGroundSnapshot(transform);

                if (snapshot.IsValid)
                {
                    snapshots.Add(snapshot);
                }
            }

            return snapshots;
        }

        private void NotifyRiverCorridorsChanged()
        {
            if (rivers == null)
            {
                return;
            }

            for (int index = 0; index < rivers.Length; index++)
            {
                StylizedRiver river = rivers[index];

                if (river == null ||
                    !river.isActiveAndEnabled)
                {
                    continue;
                }

                river.RebuildCorridorFromGround();
            }
        }

        private void RefreshRiverCorridorMaterialProperties()
        {
            if (rivers == null)
            {
                return;
            }

            for (int index = 0; index < rivers.Length; index++)
            {
                StylizedRiver river = rivers[index];

                if (river == null ||
                    !river.isActiveAndEnabled)
                {
                    continue;
                }

                river.RefreshCorridorMaterialProperties();
            }
        }

        private int CalculateGenerationSignature()
        {
            if (recipe == null)
            {
                return 0;
            }

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + recipe.ShapeSeed;
                hash = hash * 31 + (int)recipe.PatchSize;
                hash = hash * 31 + (int)recipe.Resolution;
                hash = hash * 31 + recipe.PatchCoordinate.x;
                hash = hash * 31 + recipe.PatchCoordinate.y;
                hash = hash * 31 + (int)recipe.TransitionDirection;
                hash = hash * 31 + Quantize(recipe.TransitionHeight);
                hash = hash * 31 + (int)recipe.Profile;
                hash = hash * 31 + Quantize(recipe.BroadForm);
                hash = hash * 31 + Quantize(recipe.Roughness);
                hash = hash * 31 + Quantize(recipe.SurfaceDetail);
                hash = hash * 31 + (int)recipe.EdgeBlend;
                hash = hash * 31 + Quantize(recipe.SurfaceVariation);
                hash = hash * 31 + (recipe.UseModifiers ? 1 : 0);
                hash = hash * 31 + (surfaceProfile != null ? 1 : 0);
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolvePatchScale(surfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolvePatchContrast(surfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolvePatchEdgeSoftness(surfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveExposureBias(surfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveDampDepositBias(surfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveVegetationSuitability(surfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveRockyDrySuitability(surfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveSnowEligibility(surfaceProfile));
                hash = hash * 31 + Quantize(
                    GroundSurfaceProfile.ResolveRainAbsorption(surfaceProfile));
                return hash;
            }
        }

        private static int Quantize(float value)
        {
            return Mathf.RoundToInt(value * 10000f);
        }

        private static int GenerateDifferentSeed(int current)
        {
            int candidate =
                1 +
                (int)((uint)Guid.NewGuid().GetHashCode() %
                      GroundRecipe.MaximumSeed);

            if (candidate == current)
            {
                candidate = current >= GroundRecipe.MaximumSeed
                    ? GroundRecipe.MinimumSeed
                    : current + 1;
            }

            return candidate;
        }

        private void CacheComponents()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            if (meshCollider == null)
            {
                meshCollider = GetComponent<MeshCollider>();
            }
        }

        private void ApplySurfaceProfileMaterialProperties()
        {
            ApplySurfaceProfileMaterialProperties(meshRenderer);
        }

        public void ApplySurfaceProfileMaterialProperties(Renderer targetRenderer)
        {
            if (targetRenderer == null)
            {
                return;
            }

            float patchContrast =
                GroundSurfaceProfile.ResolvePatchContrast(surfaceProfile);

            float exposureBias =
                GroundSurfaceProfile.ResolveExposureBias(surfaceProfile);

            float dampBias =
                GroundSurfaceProfile.ResolveDampDepositBias(surfaceProfile);

            float vegetationSuitability =
                GroundSurfaceProfile.ResolveVegetationSuitability(surfaceProfile);

            float rockyDrySuitability =
                GroundSurfaceProfile.ResolveRockyDrySuitability(surfaceProfile);

            float snowEligibility =
                GroundSurfaceProfile.ResolveSnowEligibility(surfaceProfile);

            float rainAbsorption =
                GroundSurfaceProfile.ResolveRainAbsorption(surfaceProfile);

            groundMaterialControls ??= new GroundMaterialControls();

            materialProperties ??= new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(materialProperties);

            // Surface Contract: 0 = generated mass/rock, 1 = generated ground.
            // The material may be shared with stones and corridor geometry, so
            // ground-specific response is selected per-renderer through a
            // property block rather than by mutating or duplicating the material
            // asset.
            materialProperties.SetColor(
                BaseColorId,
                groundMaterialControls.BaseColor);
            materialProperties.SetFloat(SurfaceContractId, 1f);
            materialProperties.SetFloat(
                ProfileContrastId,
                Mathf.Lerp(0.85f, 1.35f, patchContrast) *
                groundMaterialControls.ProfileContrastScale);
            materialProperties.SetFloat(
                ProfilePixelContrastId,
                Mathf.Lerp(0.80f, 1.35f, patchContrast) *
                groundMaterialControls.ProfilePixelContrastScale);
            materialProperties.SetFloat(
                GroundSnowResponseId,
                Mathf.Lerp(0.25f, 1.35f, snowEligibility) *
                Mathf.Lerp(0.90f, 1.15f, exposureBias) *
                groundMaterialControls.GroundSnowResponseScale);
            materialProperties.SetFloat(
                GroundDampResponseId,
                Mathf.Lerp(0.45f, 1.35f, dampBias) *
                Mathf.Lerp(0.90f, 1.20f, rainAbsorption) *
                groundMaterialControls.GroundDampResponseScale);
            materialProperties.SetFloat(
                GroundVegetationResponseId,
                Mathf.Lerp(0.05f, 0.55f, vegetationSuitability) *
                groundMaterialControls.GroundVegetationResponseScale);
            materialProperties.SetFloat(
                GroundRockyDryResponseId,
                Mathf.Lerp(0.15f, 0.95f, rockyDrySuitability) *
                groundMaterialControls.GroundRockyDryResponseScale);
            materialProperties.SetFloat(
                GroundShoreDampStrengthId,
                Mathf.Lerp(0.75f, 1.35f, rainAbsorption) *
                groundMaterialControls.GroundShoreDampStrengthScale);
            materialProperties.SetFloat(
                PixelCellSizeId,
                groundMaterialControls.PixelCellSize);
            materialProperties.SetFloat(
                PixelToneCountId,
                groundMaterialControls.PixelToneCount);
            materialProperties.SetFloat(
                PixelClusterStrengthId,
                groundMaterialControls.PixelClusterStrength);
            materialProperties.SetFloat(
                PixelVariationId,
                groundMaterialControls.PixelVariation);
            materialProperties.SetFloat(
                PixelBroadVariationId,
                groundMaterialControls.BroadVariation);
            materialProperties.SetFloat(
                PixelVertexVariationId,
                groundMaterialControls.VertexVariation);
            materialProperties.SetFloat(
                PixelEffectStrengthId,
                groundMaterialControls.PixelEffectStrength);
            materialProperties.SetFloat(
                PixelWarpStrengthId,
                groundMaterialControls.CellWarpStrength);
            materialProperties.SetFloat(
                GroundPatchBlendStrengthId,
                groundMaterialControls.GroundPatchBlendStrength);
            materialProperties.SetFloat(
                GroundMacroPatchScaleId,
                groundMaterialControls.GroundMacroPatchScale);
            materialProperties.SetFloat(
                GroundSnowTintStrengthId,
                groundMaterialControls.GroundSnowTintStrength);
            materialProperties.SetFloat(
                GroundSnowBrightnessId,
                groundMaterialControls.GroundSnowBrightness);
            materialProperties.SetFloat(
                GroundDampDarkenStrengthId,
                groundMaterialControls.GroundDampDarkenStrength);
            materialProperties.SetColor(
                GroundDampTintId,
                groundMaterialControls.DampTint);
            materialProperties.SetFloat(
                GroundDampTintStrengthId,
                groundMaterialControls.DampTintStrength);
            materialProperties.SetColor(
                GroundRockyDryTintId,
                groundMaterialControls.RockyDryTint);
            materialProperties.SetFloat(
                GroundRockyDryTintStrengthId,
                groundMaterialControls.RockyDryTintStrength);
            materialProperties.SetColor(
                GroundVegetationTintId,
                groundMaterialControls.VegetationTint);
            materialProperties.SetFloat(
                GroundVegetationTintStrengthId,
                groundMaterialControls.VegetationTintStrength);
            materialProperties.SetFloat(
                WetnessId,
                groundMaterialControls.Wetness);
            materialProperties.SetFloat(
                WetDarkenStrengthId,
                groundMaterialControls.WetDarkenStrength);
            materialProperties.SetFloat(
                WetPixelSofteningId,
                groundMaterialControls.WetPixelSoftening);
            materialProperties.SetFloat(
                WetSmoothnessBoostId,
                groundMaterialControls.WetSmoothnessBoost);
            materialProperties.SetFloat(
                FrostStrengthId,
                groundMaterialControls.FrostStrength);
            materialProperties.SetFloat(
                FrostContrastId,
                groundMaterialControls.FrostContrast);
            materialProperties.SetColor(
                FrostColorId,
                groundMaterialControls.FrostColor);
            materialProperties.SetFloat(
                MonolithicFlattenId,
                groundMaterialControls.MonolithicFlatten);
            materialProperties.SetFloat(
                MonolithicSmoothnessBoostId,
                groundMaterialControls.MonolithicSmoothnessBoost);
            materialProperties.SetFloat(
                SmoothnessId,
                groundMaterialControls.Smoothness);
            materialProperties.SetFloat(
                SpecularStrengthId,
                groundMaterialControls.SpecularStrength);

            targetRenderer.SetPropertyBlock(materialProperties);
        }

        private void EnsureGeneratedMesh()
        {
            if (generatedMesh != null)
            {
                return;
            }

            generatedMesh = new Mesh
            {
                name = "GeneratedGround_Temporary",
                hideFlags = HideFlags.DontSave
            };
        }

        private void ClearGeneratedAssignments()
        {
            baseSurface = GroundHeightFieldSnapshot.Empty;
            lastSurfaceMaskDiagnostics =
                "Surface masks have not been generated yet.";

            if (meshFilter != null &&
                meshFilter.sharedMesh == generatedMesh)
            {
                meshFilter.sharedMesh = null;
            }

            if (meshCollider != null &&
                meshCollider.sharedMesh == generatedMesh)
            {
                meshCollider.sharedMesh = null;
            }
        }

        private void OnDestroy()
        {
            ClearGeneratedAssignments();

            if (generatedMesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(generatedMesh);
            }

            generatedMesh = null;
        }
    }
}
