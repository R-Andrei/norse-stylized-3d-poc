using ProgrammaticStylized3D.Rendering.PixelSurface;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    /// <summary>
    /// Ground-specific adapter used when a secondary substrate participates in
    /// Ground rendering. Reusable dry identity resolves from the optional
    /// StylizedSurfaceMaterialProfile; legacy fields remain a compatibility
    /// fallback. Cover retention remains Ground-owned, wetness belongs to the
    /// independent GroundHydrologyModifierProfile, and spatial placement remains
    /// owned by GeneratedGround material controls.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GSLP_NewGroundSurfaceLayer",
        menuName = "PS3D/Ground/Surface Layer Profile")]
    public sealed class GroundSurfaceLayerProfile : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Human-facing name shown in GeneratedGround layer selectors.")]
        [SerializeField]
        private string displayName = "Surface Layer";

        [Tooltip("Reusable dry material identity. When assigned, palette, structural detail, and dry finish resolve from this generic Pixel Surface profile. Legacy fields below remain the null-reference fallback.")]
        [SerializeField]
        private StylizedSurfaceMaterialProfile surfaceMaterial;

        [Header("Legacy Appearance Fallback")]
        [Tooltip("Primary dry colour of this exposed substrate.")]
        [SerializeField]
        private Color baseColor = new Color(0.42f, 0.36f, 0.28f, 1f);

        [Tooltip("Dark-side colour available to macro and cavity response.")]
        [SerializeField]
        private Color darkColor = new Color(0.26f, 0.21f, 0.16f, 1f);

        [Tooltip("Light-side colour available to broad tonal response.")]
        [SerializeField]
        private Color lightColor = new Color(0.58f, 0.50f, 0.38f, 1f);

        [HideInInspector]
        [Tooltip("Legacy A2A hydrology value retained for serialized compatibility. Active wetness character is owned by GroundHydrologyModifierProfile.")]
        [SerializeField]
        private Color wetColor = new Color(0.24f, 0.21f, 0.17f, 1f);

        [Header("Surface Character")]
        [Tooltip("How strongly this layer participates in broad material variation once rendering support is enabled.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float macroContrast = 0.6f;

        [Tooltip("How strongly this layer participates in fine pixel-cell variation once rendering support is enabled.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float pixelContrast = 0.6f;

        [Tooltip("Dry smoothness target for this substrate.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float drySmoothness = 0.15f;

        [Tooltip("Dry specular strength target for this substrate.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float drySpecularStrength = 0.1f;

        [HideInInspector]
        [Tooltip("Legacy A2A hydrology value retained for serialized compatibility. Active wetness character is owned by GroundHydrologyModifierProfile.")]
        [Range(0f, 0.75f)]
        [SerializeField]
        private float wetDarkening = 0.3f;

        [HideInInspector]
        [Tooltip("Legacy A2A hydrology value retained for serialized compatibility. Active wetness character is owned by GroundHydrologyModifierProfile.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float wetTintStrength = 0.65f;

        [HideInInspector]
        [Tooltip("Legacy A2A hydrology value retained for serialized compatibility. Active wetness character is owned by GroundHydrologyModifierProfile.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float wetSmoothness = 0.45f;

        [Header("Cover Compatibility")]
        [Tooltip("Fraction of ordinary vegetation cover retained when this layer fully replaces the primary Ground surface.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float vegetationRetention = 0.2f;

        [Tooltip("Fraction of ordinary snow cover retained when this layer fully replaces the primary Ground surface.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float snowRetention = 0.5f;

        [Tooltip("Fraction of ordinary frost response retained when this layer fully replaces the primary Ground surface.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float frostRetention = 0.4f;

        [Tooltip("Fraction of Painted Accent response retained when this layer fully replaces the primary Ground surface.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentRetention = 0.35f;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? name
                : displayName;

        public StylizedSurfaceMaterialProfile SurfaceMaterial =>
            surfaceMaterial;
        public Color BaseColor =>
            surfaceMaterial != null
                ? surfaceMaterial.BaseColor
                : baseColor;
        public Color DarkColor =>
            surfaceMaterial != null
                ? surfaceMaterial.DarkColor
                : darkColor;
        public Color LightColor =>
            surfaceMaterial != null
                ? surfaceMaterial.LightColor
                : lightColor;
        public Color CavityColor =>
            surfaceMaterial != null
                ? surfaceMaterial.CavityColor
                : darkColor;
        public Color WetColor => wetColor;
        public StylizedSurfaceMaterialPayloadMode PayloadMode =>
            surfaceMaterial != null
                ? surfaceMaterial.PayloadMode
                : StylizedSurfaceMaterialPayloadMode.PaletteDetail;
        public bool UsesAuthoredColor =>
            surfaceMaterial != null && surfaceMaterial.UsesAuthoredColor;
        public float AuthoredColorStrength =>
            surfaceMaterial != null
                ? surfaceMaterial.AuthoredColorStrength
                : 0f;
        public Color AuthoredColorTint =>
            surfaceMaterial != null
                ? surfaceMaterial.AuthoredColorTint
                : Color.white;
        public float AuthoredColorTintStrength =>
            surfaceMaterial != null
                ? surfaceMaterial.AuthoredColorTintStrength
                : 0f;
        public float AuthoredColorLightingStrength =>
            surfaceMaterial != null
                ? surfaceMaterial.AuthoredColorLightingStrength
                : 1f;
        public float AuthoredRoughnessStrength =>
            surfaceMaterial != null
                ? surfaceMaterial.AuthoredRoughnessStrength
                : 0f;
        public float MacroContrast =>
            surfaceMaterial != null
                ? surfaceMaterial.MacroContrast
                : Mathf.Clamp(macroContrast, 0f, 2f);
        public float PixelContrast => Mathf.Clamp(pixelContrast, 0f, 2f);
        public float LegacyPixelCellInfluence =>
            surfaceMaterial != null
                ? surfaceMaterial.LegacyPixelCellInfluence
                : 1f;
        public float DetailValueStrength =>
            surfaceMaterial != null
                ? surfaceMaterial.DetailValueStrength
                : 0f;
        public float DetailWorldScale =>
            surfaceMaterial != null
                ? surfaceMaterial.DetailWorldScale
                : 1f;
        public float DetailNormalStrength =>
            surfaceMaterial != null
                ? surfaceMaterial.DetailNormalStrength
                : 0f;
        public float DetailCavityStrength =>
            surfaceMaterial != null
                ? surfaceMaterial.DetailCavityStrength
                : 0f;
        public float DetailCavityBias =>
            surfaceMaterial != null
                ? surfaceMaterial.DetailCavityBias
                : 0.5f;
        public float DetailFormHighlightStrength =>
            surfaceMaterial != null
                ? surfaceMaterial.DetailFormHighlightStrength
                : 0f;
        public float FinishVariationStrength =>
            surfaceMaterial != null
                ? surfaceMaterial.FinishVariationStrength
                : 0f;
        public float DrySmoothness =>
            surfaceMaterial != null
                ? surfaceMaterial.DrySmoothness
                : Mathf.Clamp01(drySmoothness);
        public float DrySpecularStrength =>
            surfaceMaterial != null
                ? surfaceMaterial.DrySpecularStrength
                : Mathf.Clamp01(drySpecularStrength);
        public float WetDarkening => Mathf.Clamp(wetDarkening, 0f, 0.75f);
        public float WetTintStrength => Mathf.Clamp01(wetTintStrength);
        public float WetSmoothness => Mathf.Clamp01(wetSmoothness);
        public float VegetationRetention => Mathf.Clamp01(vegetationRetention);
        public float SnowRetention => Mathf.Clamp01(snowRetention);
        public float FrostRetention => Mathf.Clamp01(frostRetention);
        public float PaintedAccentRetention => Mathf.Clamp01(paintedAccentRetention);

        public bool TryResolveDetail(
            out Texture2DArray textureArray,
            out int sliceIndex)
        {
            textureArray = null;
            sliceIndex = -1;

            return surfaceMaterial != null &&
                   surfaceMaterial.TryResolveDetail(
                       out textureArray,
                       out sliceIndex);
        }

        public bool TryResolveAuthoredColor(
            out Texture2DArray textureArray,
            out int sliceIndex)
        {
            textureArray = null;
            sliceIndex = -1;

            return surfaceMaterial != null &&
                   surfaceMaterial.TryResolveAuthoredColor(
                       out textureArray,
                       out sliceIndex);
        }

        public void SetDisplayName(string value)
        {
            displayName = string.IsNullOrWhiteSpace(value)
                ? name
                : value.Trim();
        }
    }
}
