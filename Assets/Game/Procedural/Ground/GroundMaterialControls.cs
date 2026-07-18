using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundRiverbedSurfaceSource
    {
        LegacyAuto = 0,

        [InspectorName("Primary Ground")]
        PrimaryGround = 1,

        [InspectorName("Inherit Bank Surface Layer")]
        InheritBankSurfaceLayer = 2,

        [InspectorName("Custom Riverbed Surface Layer")]
        CustomRiverbedSurfaceLayer = 3
    }

    public enum GroundRiverbedHydrologySource
    {
        [InspectorName("Inherit Shore Hydrology Modifier")]
        InheritShoreHydrologyModifier = 0,

        [InspectorName("Custom Hydrology Modifier")]
        CustomHydrologyModifier = 1,

        Disabled = 2
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

    [Header("River-Coupled Surface Layers")]
    [Tooltip("Optional reusable substrate exposed across River banks. Null inherits the primary Ground surface.")]
    [SerializeField]
    private GroundSurfaceLayerProfile bankSurfaceLayer;

    [Tooltip("Custom reusable substrate used when Riverbed Surface Source is Custom Riverbed Surface Layer. Legacy non-null values resolve to Custom; legacy null values resolve to Primary Ground.")]
    [SerializeField]
    private GroundSurfaceLayerProfile riverbedSurfaceLayer;

    [HideInInspector]
    [SerializeField]
    private GroundRiverbedSurfaceSource riverbedSurfaceSource =
        GroundRiverbedSurfaceSource.LegacyAuto;

    [Tooltip("Optional reusable wetness character applied independently from Bank substrate identity. Null disables local Shore hydrology.")]
    [SerializeField]
    private GroundHydrologyModifierProfile shoreHydrologyModifier;

    [HideInInspector]
    [SerializeField]
    private GroundRiverbedHydrologySource riverbedHydrologySource =
        GroundRiverbedHydrologySource.InheritShoreHydrologyModifier;

    [Tooltip("Custom reusable wetness character used when Riverbed Hydrology Source is Custom Hydrology Modifier.")]
    [SerializeField]
    private GroundHydrologyModifierProfile riverbedHydrologyModifier;

    [Header("River-Coupled Bank Composition")]
    [InspectorName("Bank Material Strength")]
    [Tooltip("Master amount of the selected Bank Surface Layer. Zero preserves the ordinary Ground surface everywhere.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float bankMaterialStrength = 1f;

    [InspectorName("Core Bank Reach")]
    [Tooltip("How much of the existing precise broad-bank field participates before any optional outward extension. This changes material composition only, not River or Ground geometry.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float bankMaterialReach = 0.65f;

    [InspectorName("Immediate-Bank Exposure")]
    [Tooltip("Additional substrate replacement across the stronger immediate-bank portion of the shore field.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float immediateBankExposure = 0.55f;

    [InspectorName("Waterline Material Strength")]
    [Tooltip("Additional substrate replacement at the highest corridor-only shore values. This controls material identity independently from wetness.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float waterlineMaterialStrength = 1f;

    [InspectorName("Core Bank Transition Softness")]
    [Tooltip("Softness of the core broad-bank, immediate-bank, and waterline transitions. This does not control the separately authored outer-bank fade.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float bankTransitionSoftness = 0.55f;

    [InspectorName("Outer Bank Extension")]
    [Tooltip("Additional distance in metres from the exact Riverbed Support boundary across the River corridor bank. Zero disables the outer extension.")]
    [Range(0f, 20f)]
    [SerializeField]
    private float outerBankExtension;

    [InspectorName("Outer Bank Strength")]
    [Tooltip("Material-blend strength of the optional outer extension. The master Bank Material Strength still scales the complete result.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float outerBankStrength = 0.5f;

    [InspectorName("Outer Bank Fade")]
    [Tooltip("Fade distance in metres beyond the authored Outer Bank Extension across the River corridor toward its terrain handoff.")]
    [Range(0.05f, 10f)]
    [SerializeField]
    private float outerBankFade = 1f;

    [Header("River-Coupled Bank Material Application")]
    [InspectorName("Detail Scale Multiplier")]
    [Tooltip("Multiplies the reusable material's natural world repeat size for this Bank application. Values above one make the authored stones larger; values below one make them finer. One preserves the shared material definition.")]
    [Range(0.25f, 4f)]
    [SerializeField]
    private float bankDetailScaleMultiplier = 1f;

    [InspectorName("Normal Strength Multiplier")]
    [Tooltip("Multiplies reusable structural-detail normal strength for this Bank application. One preserves the shared material definition.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float bankDetailNormalStrengthMultiplier = 1f;

    [InspectorName("Cavity Strength Multiplier")]
    [Tooltip("Multiplies reusable cavity and inter-stone separation strength for this Bank application. One preserves the shared material definition.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float bankDetailCavityStrengthMultiplier = 1f;

    [InspectorName("Value / Form Multiplier")]
    [Tooltip("Multiplies authored per-stone value and form-highlight response for this Bank application. One preserves the shared material definition.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float bankDetailValueFormMultiplier = 1f;

    [InspectorName("Finish Variation Multiplier")]
    [Tooltip("Multiplies reusable dry finish variation for this Bank application. One preserves the shared material definition.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float bankDetailFinishVariationMultiplier = 1f;

    [InspectorName("Legacy Cell Influence Multiplier")]
    [Tooltip("Multiplies the reusable material's retained legacy pixel-cell response for this Bank application. One preserves the shared material definition.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float bankLegacyPixelCellInfluenceMultiplier = 1f;

    [Header("River-Coupled Riverbed Composition")]
    [InspectorName("Riverbed Material Strength")]
    [Tooltip("Master amount of the selected dry Riverbed Surface Layer on exact Riverbed Support. This does not control submerged-cover exclusion or wetness.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float riverbedMaterialStrength = 1f;

    [Header("River-Coupled Riverbed Material Application")]
    [InspectorName("Detail Scale Multiplier")]
    [Tooltip("Multiplies the reusable material's natural world repeat size for this Riverbed application. Values above one make the authored stones larger; values below one make them finer. One preserves the shared material definition.")]
    [Range(0.25f, 4f)]
    [SerializeField]
    private float riverbedDetailScaleMultiplier = 1f;

    [InspectorName("Normal Strength Multiplier")]
    [Tooltip("Multiplies reusable structural-detail normal strength for this Riverbed application. One preserves the shared material definition.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float riverbedDetailNormalStrengthMultiplier = 1f;

    [InspectorName("Cavity Strength Multiplier")]
    [Tooltip("Multiplies reusable cavity and inter-stone separation strength for this Riverbed application. One preserves the shared material definition.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float riverbedDetailCavityStrengthMultiplier = 1f;

    [InspectorName("Value / Form Multiplier")]
    [Tooltip("Multiplies authored per-stone value and form-highlight response for this Riverbed application. One preserves the shared material definition.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float riverbedDetailValueFormMultiplier = 1f;

    [InspectorName("Finish Variation Multiplier")]
    [Tooltip("Multiplies reusable dry finish variation for this Riverbed application. One preserves the shared material definition.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float riverbedDetailFinishVariationMultiplier = 1f;

    [InspectorName("Legacy Cell Influence Multiplier")]
    [Tooltip("Multiplies the reusable material's retained legacy pixel-cell response for this Riverbed application. One preserves the shared material definition.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float riverbedLegacyPixelCellInfluenceMultiplier = 1f;

    [Header("River-Coupled Riverbed Hydrology")]
    [InspectorName("Riverbed Wetness Strength")]
    [Tooltip("Master exact-support wetness strength for the resolved Riverbed Hydrology Modifier. This has no reach or fade and does not change substrate identity.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float riverbedWetnessStrength = 1f;

    [InspectorName("Riverbed Edge Transition Distance")]
    [Tooltip("Distance in metres measured inward from the exact Riverbed Support boundary over which Riverbed wetness transitions from resolved Bank-edge wetness to full Riverbed wetness. Zero preserves the exact-support boundary.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float riverbedToBankWetnessBlendDistance = 0.2f;

    [InspectorName("Riverbed Edge Transition Softness")]
    [Tooltip("Shape of the inward Riverbed wetness transition. Zero is linear; one uses a fully smooth transition from edge wetness to interior wetness.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float riverbedToBankWetnessBlendSoftness = 0.75f;

    [InspectorName("Smoothness Response")]
    [Tooltip("How much of the Riverbed Hydrology Modifier's smoothness boost is permitted on submerged Ground. Zero keeps wet colour and darkening without adding submerged smoothness.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float riverbedWetSmoothnessResponse;

    [InspectorName("Specular Response")]
    [Tooltip("How much of the Riverbed Hydrology Modifier's specular boost is permitted on submerged Ground. Zero leaves reflective ownership to the water surface.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float riverbedWetSpecularResponse;

    [Header("River-Coupled Surface-Cover Response")]
    [InspectorName("Vegetation Retreat Strength")]
    [Tooltip("How strongly the selected Bank Surface Layer's vegetation-retention value suppresses the ordinary vegetation response. Zero preserves current rendering.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float vegetationRetreatStrength;

    [InspectorName("Snow Melt Strength")]
    [Tooltip("How strongly the selected Bank Surface Layer's snow-retention value suppresses the ordinary snow response. Zero preserves current rendering.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float snowMeltStrength;

    [InspectorName("Frost Retreat Strength")]
    [Tooltip("How strongly the selected Bank Surface Layer's frost-retention value suppresses the ordinary frost response. Zero preserves current rendering.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float frostRetreatStrength;

    [InspectorName("Painted Accent Retreat Strength")]
    [Tooltip("How strongly the selected Bank Surface Layer's Painted Accent retention suppresses rendered Painted Accent ink. Raw coverage remains unchanged. Zero preserves current rendering.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float paintedAccentRetreatStrength;


    [Header("River-Coupled Shore Hydrology")]
    [InspectorName("Shore Wetness Strength")]
    [Tooltip("Master strength of the selected Shore Hydrology Modifier. Zero preserves the dry A3A baseline.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float shoreWetnessStrength;

    [InspectorName("Shore Wetness Reach")]
    [Tooltip("Distance in metres from the exact Riverbed Support boundary across the corridor bank. This is independent from Bank Surface Layer reach.")]
    [Range(0f, 20f)]
    [SerializeField]
    private float shoreWetnessReach = 0.5f;

    [InspectorName("Shore Wetness Fade")]
    [Tooltip("Fade distance in metres beyond Shore Wetness Reach across the corridor bank.")]
    [Range(0.05f, 10f)]
    [SerializeField]
    private float shoreWetnessFade = 0.25f;

    [InspectorName("Broad-Bank Saturation")]
    [Tooltip("Baseline wetness contribution across the metre-based Shore Wetness Reach.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float broadBankSaturation = 0.45f;

    [InspectorName("Immediate-Bank Saturation")]
    [Tooltip("Additional wetness contribution across the stronger immediate-bank Shore semantic.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float immediateBankSaturation = 0.8f;

    [InspectorName("Waterline Saturation")]
    [Tooltip("Additional wetness contribution at the strongest waterline Shore semantic.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float waterlineSaturation = 1f;

    [InspectorName("Highlight Width")]
    [Tooltip("Full-width distance in metres of the stylized Shore highlight measured outward from the exact Riverbed Support edge across the corridor Bank.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float shoreWetHighlightWidth = 0.05f;

    [InspectorName("Highlight Feather")]
    [Tooltip("Additional fade distance in metres beyond Highlight Width. Lower values keep the highlight tightly pinned to the waterline.")]
    [Range(0.005f, 2f)]
    [SerializeField]
    private float shoreWetHighlightFeather = 0.05f;

    [InspectorName("Wet Highlight Strength")]
    [Tooltip("Strength of the narrow stylized Shore highlight added after ordinary Ground lighting. The effect remains masked by local Shore wetness.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float shoreWetHighlightStrength = 0.35f;

    [InspectorName("Wet Highlight Tightness")]
    [Tooltip("Angular tightness of the physical Shore highlight lobe. Higher values produce a narrower response.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float shoreWetHighlightTightness = 0.8f;

    [InspectorName("Camera-Centred Bias")]
    [Tooltip("Transfers Shore wet finish from the broad physical PBR response into a camera-centred stylized band. Zero preserves the physical response; one uses the camera-centred response.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float shoreWetHighlightCameraBias = 0.85f;

    [InspectorName("Vertical Falloff")]
    [Tooltip("How quickly the camera-centred Shore highlight fades toward the top and bottom of the active camera view.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float shoreWetHighlightVerticalFalloff = 0.6f;

    [Header("Macro Patch Composition")]
    [InspectorName("Macro Patch Scale")]
    [Tooltip("Metre scale of broad ground material patches.")]
    [Range(0.5f, 12f)]
    [SerializeField]
    private float groundMacroPatchScale = 4.5f;

    [InspectorName("Macro Patch Pattern Seed")]
    [Tooltip("Deterministic integer seed that reshuffles macro patch positions and silhouettes independently from the pixel seed. Adjacent Grounds should use the same value when their macro pattern must remain continuous.")]
    [SerializeField]
    private int groundMacroPatchPatternSeed;

    [InspectorName("Macro Patch Intensity")]
    [Tooltip("Visible tonal intensity of shader-side macro patches. This is independent from fine pixel and generated vertex variation.")]
    [Range(0f, 0.75f)]
    [SerializeField]
    private float broadVariation = 0.032f;

    [InspectorName("Macro Patch Transition Softness")]
    [Tooltip("Softness of the transition between neutral terrain and light or dark macro patches. Zero preserves the current narrow transition; one gives a much broader seamless blend.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float groundMacroPatchTransitionSoftness = 0.75f;

    [InspectorName("Average Patch Separation")]
    [Tooltip("Average amount of neutral terrain between dark and light macro patches. Zero allows some local contacts while preserving separation elsewhere; higher values make patches increasingly sparse.")]
    [Min(0f)]
    [SerializeField]
    private float groundMacroPatchSeparation = 1f;

    [Header("Elevation Readability")]
    [InspectorName("Relief Shading Strength")]
    [Tooltip("Adds a directional value cue from the actual Ground slope without changing lighting normals, geometry, or collision.")]
    [Range(0f, 0.75f)]
    [SerializeField]
    private float reliefShadingStrength;

    [InspectorName("Relative Height Contrast")]
    [Tooltip("Value shift per metre of generated local height relative to the undeformed Ground plane. Positive heights brighten and negative heights darken.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float relativeHeightContrast;

    [Header("Pixel Variation")]
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

    [Tooltip("How strongly generated mesh vertex colour variation affects the ground material response.")]
    [Range(0f, 0.25f)]
    [SerializeField]
    private float vertexVariation = 0.2f;

    [Tooltip("Overall multiplier for combined pixel and generated vertex variation.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float pixelEffectStrength = 1f;

    [Tooltip("How much the pixel-cell lookup is spatially warped.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float cellWarpStrength = 0.08f;

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

    [HideInInspector]
    [Tooltip("Legacy Shore damp scale retained for serialized compatibility. Active Shore wetness is owned by the independent hydrology modifier.")]
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
    public GroundSurfaceLayerProfile BankSurfaceLayer => bankSurfaceLayer;
    public GroundSurfaceLayerProfile RiverbedSurfaceLayer => riverbedSurfaceLayer;
    public GroundRiverbedSurfaceSource RiverbedSurfaceSource =>
        riverbedSurfaceSource == GroundRiverbedSurfaceSource.LegacyAuto
            ? riverbedSurfaceLayer != null
                ? GroundRiverbedSurfaceSource.CustomRiverbedSurfaceLayer
                : GroundRiverbedSurfaceSource.PrimaryGround
            : riverbedSurfaceSource;
    public GroundSurfaceLayerProfile ResolvedRiverbedSurfaceLayer =>
        RiverbedSurfaceSource switch
        {
            GroundRiverbedSurfaceSource.InheritBankSurfaceLayer =>
                bankSurfaceLayer,
            GroundRiverbedSurfaceSource.CustomRiverbedSurfaceLayer =>
                riverbedSurfaceLayer,
            _ => null
        };
    public GroundHydrologyModifierProfile ShoreHydrologyModifier =>
        shoreHydrologyModifier;
    public GroundRiverbedHydrologySource RiverbedHydrologySource =>
        riverbedHydrologySource;
    public GroundHydrologyModifierProfile RiverbedHydrologyModifier =>
        riverbedHydrologyModifier;
    public GroundHydrologyModifierProfile ResolvedRiverbedHydrologyModifier =>
        riverbedHydrologySource switch
        {
            GroundRiverbedHydrologySource.InheritShoreHydrologyModifier =>
                shoreHydrologyModifier,
            GroundRiverbedHydrologySource.CustomHydrologyModifier =>
                riverbedHydrologyModifier,
            _ => null
        };
    public float BankMaterialStrength => Mathf.Clamp01(bankMaterialStrength);
    public float BankMaterialReach => Mathf.Clamp01(bankMaterialReach);
    public float ImmediateBankExposure => Mathf.Clamp01(immediateBankExposure);
    public float WaterlineMaterialStrength => Mathf.Clamp01(waterlineMaterialStrength);
    public float BankTransitionSoftness => Mathf.Clamp01(bankTransitionSoftness);
    public float OuterBankExtension => Mathf.Clamp(outerBankExtension, 0f, 20f);
    public float OuterBankStrength => Mathf.Clamp01(outerBankStrength);
    public float OuterBankFade => Mathf.Clamp(outerBankFade, 0.05f, 10f);
    public float BankDetailScaleMultiplier =>
        Mathf.Clamp(bankDetailScaleMultiplier, 0.25f, 4f);
    public float BankDetailNormalStrengthMultiplier =>
        Mathf.Clamp(bankDetailNormalStrengthMultiplier, 0f, 2f);
    public float BankDetailCavityStrengthMultiplier =>
        Mathf.Clamp(bankDetailCavityStrengthMultiplier, 0f, 2f);
    public float BankDetailValueFormMultiplier =>
        Mathf.Clamp(bankDetailValueFormMultiplier, 0f, 2f);
    public float BankDetailFinishVariationMultiplier =>
        Mathf.Clamp(bankDetailFinishVariationMultiplier, 0f, 2f);
    public float BankLegacyPixelCellInfluenceMultiplier =>
        Mathf.Clamp(bankLegacyPixelCellInfluenceMultiplier, 0f, 2f);
    public float RiverbedMaterialStrength =>
        Mathf.Clamp01(riverbedMaterialStrength);
    public float RiverbedDetailScaleMultiplier =>
        Mathf.Clamp(riverbedDetailScaleMultiplier, 0.25f, 4f);
    public float RiverbedDetailNormalStrengthMultiplier =>
        Mathf.Clamp(riverbedDetailNormalStrengthMultiplier, 0f, 2f);
    public float RiverbedDetailCavityStrengthMultiplier =>
        Mathf.Clamp(riverbedDetailCavityStrengthMultiplier, 0f, 2f);
    public float RiverbedDetailValueFormMultiplier =>
        Mathf.Clamp(riverbedDetailValueFormMultiplier, 0f, 2f);
    public float RiverbedDetailFinishVariationMultiplier =>
        Mathf.Clamp(riverbedDetailFinishVariationMultiplier, 0f, 2f);
    public float RiverbedLegacyPixelCellInfluenceMultiplier =>
        Mathf.Clamp(riverbedLegacyPixelCellInfluenceMultiplier, 0f, 2f);
    public float RiverbedWetnessStrength =>
        Mathf.Clamp01(riverbedWetnessStrength);
    public float RiverbedToBankWetnessBlendDistance =>
        Mathf.Clamp(riverbedToBankWetnessBlendDistance, 0f, 2f);
    public float RiverbedToBankWetnessBlendSoftness =>
        Mathf.Clamp01(riverbedToBankWetnessBlendSoftness);
    public float RiverbedWetSmoothnessResponse =>
        Mathf.Clamp01(riverbedWetSmoothnessResponse);
    public float RiverbedWetSpecularResponse =>
        Mathf.Clamp01(riverbedWetSpecularResponse);
    public float VegetationRetreatStrength =>
        Mathf.Clamp01(vegetationRetreatStrength);
    public float SnowMeltStrength => Mathf.Clamp01(snowMeltStrength);
    public float FrostRetreatStrength => Mathf.Clamp01(frostRetreatStrength);
    public float PaintedAccentRetreatStrength =>
        Mathf.Clamp01(paintedAccentRetreatStrength);
    public float ShoreWetnessStrength => Mathf.Clamp01(shoreWetnessStrength);
    public float ShoreWetnessReach => Mathf.Clamp(shoreWetnessReach, 0f, 20f);
    public float ShoreWetnessFade => Mathf.Clamp(shoreWetnessFade, 0.05f, 10f);
    public float BroadBankSaturation => Mathf.Clamp01(broadBankSaturation);
    public float ImmediateBankSaturation =>
        Mathf.Clamp01(immediateBankSaturation);
    public float WaterlineSaturation => Mathf.Clamp01(waterlineSaturation);
    public float ShoreWetHighlightWidth =>
        Mathf.Clamp(shoreWetHighlightWidth, 0f, 2f);
    public float ShoreWetHighlightFeather =>
        Mathf.Clamp(shoreWetHighlightFeather, 0.005f, 2f);
    public float ShoreWetHighlightStrength =>
        Mathf.Clamp(shoreWetHighlightStrength, 0f, 2f);
    public float ShoreWetHighlightTightness =>
        Mathf.Clamp01(shoreWetHighlightTightness);
    public float ShoreWetHighlightCameraBias =>
        Mathf.Clamp01(shoreWetHighlightCameraBias);
    public float ShoreWetHighlightVerticalFalloff =>
        Mathf.Clamp01(shoreWetHighlightVerticalFalloff);
    public float PixelCellSize => Mathf.Clamp(pixelCellSize, 0.005f, 0.5f);
    public float PixelToneCount => Mathf.Clamp(pixelToneCount, 2f, 8f);
    public float PixelClusterStrength => Mathf.Clamp01(pixelClusterStrength);
    public float PixelVariation => Mathf.Clamp(pixelVariation, 0f, 0.25f);
    public float BroadVariation => Mathf.Clamp(broadVariation, 0f, 0.75f);
    public float VertexVariation => Mathf.Clamp(vertexVariation, 0f, 0.25f);
    public float PixelEffectStrength => Mathf.Clamp(pixelEffectStrength, 0f, 2f);
    public float CellWarpStrength => Mathf.Clamp(cellWarpStrength, 0f, 2f);
    public float GroundMacroPatchScale => Mathf.Clamp(groundMacroPatchScale, 0.5f, 12f);
    public int GroundMacroPatchPatternSeed => groundMacroPatchPatternSeed;
    public float GroundMacroPatchTransitionSoftness =>
        Mathf.Clamp01(groundMacroPatchTransitionSoftness);
    public float GroundMacroPatchSeparation =>
        Mathf.Max(0f, groundMacroPatchSeparation);
    public float ReliefShadingStrength =>
        Mathf.Clamp(reliefShadingStrength, 0f, 0.75f);
    public float RelativeHeightContrast =>
        Mathf.Clamp01(relativeHeightContrast);
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
            bankSurfaceLayer = null;
            riverbedSurfaceLayer = null;
            riverbedSurfaceSource =
                GroundRiverbedSurfaceSource.PrimaryGround;
            shoreHydrologyModifier = null;
            riverbedHydrologySource =
                GroundRiverbedHydrologySource.InheritShoreHydrologyModifier;
            riverbedHydrologyModifier = null;
            bankMaterialStrength = 1f;
            bankMaterialReach = 0.65f;
            immediateBankExposure = 0.55f;
            waterlineMaterialStrength = 1f;
            bankTransitionSoftness = 0.55f;
            outerBankExtension = 0f;
            outerBankStrength = 0.5f;
            outerBankFade = 1f;
            bankDetailScaleMultiplier = 1f;
            bankDetailNormalStrengthMultiplier = 1f;
            bankDetailCavityStrengthMultiplier = 1f;
            bankDetailValueFormMultiplier = 1f;
            bankDetailFinishVariationMultiplier = 1f;
            bankLegacyPixelCellInfluenceMultiplier = 1f;
            riverbedMaterialStrength = 1f;
            riverbedDetailScaleMultiplier = 1f;
            riverbedDetailNormalStrengthMultiplier = 1f;
            riverbedDetailCavityStrengthMultiplier = 1f;
            riverbedDetailValueFormMultiplier = 1f;
            riverbedDetailFinishVariationMultiplier = 1f;
            riverbedLegacyPixelCellInfluenceMultiplier = 1f;
            riverbedWetnessStrength = 1f;
            riverbedToBankWetnessBlendDistance = 0.2f;
            riverbedToBankWetnessBlendSoftness = 0.75f;
            riverbedWetSmoothnessResponse = 0f;
            riverbedWetSpecularResponse = 0f;
            vegetationRetreatStrength = 0f;
            snowMeltStrength = 0f;
            frostRetreatStrength = 0f;
            paintedAccentRetreatStrength = 0f;
            shoreWetnessStrength = 0f;
            shoreWetnessReach = 0.5f;
            shoreWetnessFade = 0.25f;
            broadBankSaturation = 0.45f;
            immediateBankSaturation = 0.8f;
            waterlineSaturation = 1f;
            shoreWetHighlightWidth = 0.05f;
            shoreWetHighlightFeather = 0.05f;
            shoreWetHighlightStrength = 0.35f;
            shoreWetHighlightTightness = 0.8f;
            shoreWetHighlightCameraBias = 0.85f;
            shoreWetHighlightVerticalFalloff = 0.6f;
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
        bankSurfaceLayer = source.bankSurfaceLayer;
        riverbedSurfaceLayer = source.riverbedSurfaceLayer;
        riverbedSurfaceSource = source.riverbedSurfaceSource;
        shoreHydrologyModifier = source.shoreHydrologyModifier;
        riverbedHydrologySource = source.riverbedHydrologySource;
        riverbedHydrologyModifier = source.riverbedHydrologyModifier;
        bankMaterialStrength = source.bankMaterialStrength;
        bankMaterialReach = source.bankMaterialReach;
        immediateBankExposure = source.immediateBankExposure;
        waterlineMaterialStrength = source.waterlineMaterialStrength;
        bankTransitionSoftness = source.bankTransitionSoftness;
        outerBankExtension = source.outerBankExtension;
        outerBankStrength = source.outerBankStrength;
        outerBankFade = source.outerBankFade;
        bankDetailScaleMultiplier = source.bankDetailScaleMultiplier;
        bankDetailNormalStrengthMultiplier =
            source.bankDetailNormalStrengthMultiplier;
        bankDetailCavityStrengthMultiplier =
            source.bankDetailCavityStrengthMultiplier;
        bankDetailValueFormMultiplier =
            source.bankDetailValueFormMultiplier;
        bankDetailFinishVariationMultiplier =
            source.bankDetailFinishVariationMultiplier;
        bankLegacyPixelCellInfluenceMultiplier =
            source.bankLegacyPixelCellInfluenceMultiplier;
        riverbedMaterialStrength = source.riverbedMaterialStrength;
        riverbedDetailScaleMultiplier =
            source.riverbedDetailScaleMultiplier;
        riverbedDetailNormalStrengthMultiplier =
            source.riverbedDetailNormalStrengthMultiplier;
        riverbedDetailCavityStrengthMultiplier =
            source.riverbedDetailCavityStrengthMultiplier;
        riverbedDetailValueFormMultiplier =
            source.riverbedDetailValueFormMultiplier;
        riverbedDetailFinishVariationMultiplier =
            source.riverbedDetailFinishVariationMultiplier;
        riverbedLegacyPixelCellInfluenceMultiplier =
            source.riverbedLegacyPixelCellInfluenceMultiplier;
        riverbedWetnessStrength = source.riverbedWetnessStrength;
        riverbedToBankWetnessBlendDistance =
            source.riverbedToBankWetnessBlendDistance;
        riverbedToBankWetnessBlendSoftness =
            source.riverbedToBankWetnessBlendSoftness;
        riverbedWetSmoothnessResponse =
            source.riverbedWetSmoothnessResponse;
        riverbedWetSpecularResponse =
            source.riverbedWetSpecularResponse;
        vegetationRetreatStrength = source.vegetationRetreatStrength;
        snowMeltStrength = source.snowMeltStrength;
        frostRetreatStrength = source.frostRetreatStrength;
        paintedAccentRetreatStrength = source.paintedAccentRetreatStrength;
        shoreWetnessStrength = source.shoreWetnessStrength;
        shoreWetnessReach = source.shoreWetnessReach;
        shoreWetnessFade = source.shoreWetnessFade;
        broadBankSaturation = source.broadBankSaturation;
        immediateBankSaturation = source.immediateBankSaturation;
        waterlineSaturation = source.waterlineSaturation;
        shoreWetHighlightWidth = source.shoreWetHighlightWidth;
        shoreWetHighlightFeather = source.shoreWetHighlightFeather;
        shoreWetHighlightStrength = source.shoreWetHighlightStrength;
        shoreWetHighlightTightness = source.shoreWetHighlightTightness;
        shoreWetHighlightCameraBias = source.shoreWetHighlightCameraBias;
        shoreWetHighlightVerticalFalloff =
            source.shoreWetHighlightVerticalFalloff;
        pixelCellSize = source.pixelCellSize;
        pixelToneCount = source.pixelToneCount;
        pixelClusterStrength = source.pixelClusterStrength;
        pixelVariation = source.pixelVariation;
        broadVariation = source.broadVariation;
        vertexVariation = source.vertexVariation;
        pixelEffectStrength = source.pixelEffectStrength;
        cellWarpStrength = source.cellWarpStrength;
        groundMacroPatchScale = source.groundMacroPatchScale;
        groundMacroPatchPatternSeed = source.groundMacroPatchPatternSeed;
        groundMacroPatchTransitionSoftness =
            source.groundMacroPatchTransitionSoftness;
        groundMacroPatchSeparation = source.groundMacroPatchSeparation;
        reliefShadingStrength = source.reliefShadingStrength;
        relativeHeightContrast = source.relativeHeightContrast;
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
        groundMacroPatchPatternSeed = 0;
        groundMacroPatchTransitionSoftness = 0.75f;
        groundMacroPatchSeparation = 1f;
        reliefShadingStrength = 0f;
        relativeHeightContrast = 0f;
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
        groundMacroPatchPatternSeed = 0;
        groundMacroPatchTransitionSoftness = 0.75f;
        groundMacroPatchSeparation = 1f;
        reliefShadingStrength = 0f;
        relativeHeightContrast = 0f;
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
        groundMacroPatchPatternSeed = 0;
        groundMacroPatchTransitionSoftness = 0.75f;
        groundMacroPatchSeparation = 1f;
        reliefShadingStrength = 0f;
        relativeHeightContrast = 0f;
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
        groundMacroPatchPatternSeed = 0;
        groundMacroPatchTransitionSoftness = 0.75f;
        groundMacroPatchSeparation = 1f;
        reliefShadingStrength = 0f;
        relativeHeightContrast = 0f;
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
