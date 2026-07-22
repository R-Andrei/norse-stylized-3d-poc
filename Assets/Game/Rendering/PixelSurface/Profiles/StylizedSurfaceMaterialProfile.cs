using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface
{
    public enum StylizedSurfaceMaterialPayloadMode
    {
        PaletteDetail = 0,
        AuthoredColor = 1
    }

    /// <summary>
    /// Reusable dry stylized material identity. It contains no Ground, River,
    /// road, wall, placement, or hydrology ownership. Consumers choose
    /// projection and environmental modifiers independently.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SSMP_NewStylizedSurfaceMaterial",
        menuName = "PS3D/Pixel Surface/Stylized Surface Material")]
    public sealed class StylizedSurfaceMaterialProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string displayName = "Stylized Surface Material";

        // Retained only so existing assets deserialize without migration. Runtime
        // capability now resolves from the selected detail-library entry.
#pragma warning disable 0414
        [HideInInspector]
        [SerializeField]
        private StylizedSurfaceMaterialPayloadMode payloadMode =
            StylizedSurfaceMaterialPayloadMode.PaletteDetail;
#pragma warning restore 0414

        [Header("Palette")]
        [SerializeField]
        private Color baseColor = new Color(0.42f, 0.36f, 0.28f, 1f);

        [SerializeField]
        private Color darkColor = new Color(0.26f, 0.21f, 0.16f, 1f);

        [SerializeField]
        private Color lightColor = new Color(0.58f, 0.50f, 0.38f, 1f);

        [Tooltip("Authored colour used inside packed-detail cavities and gaps.")]
        [SerializeField]
        private Color cavityColor = new Color(0.12f, 0.10f, 0.08f, 1f);

        [Header("Texture Form")]
        [InspectorName("Texture Form Strength")]
        [Tooltip("How strongly an imported material-set form map moves the palette between Dark, Base, and Light. Entries without imported form data ignore this value.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float authoredColorStrength = 1f;

        // Retained only for serialized compatibility. Palette is now the sole
        // visible and runtime colour authority.
        [HideInInspector]
        [SerializeField]
        private Color authoredColorTint = Color.white;

        [HideInInspector]
        [Range(0f, 1f)]
        [SerializeField]
        private float authoredColorTintStrength;

        [InspectorName("Scene Lighting Response")]
        [Tooltip("Fraction of ordinary scene diffuse lighting applied to an imported texture-form material. Lower values preserve more authored form; one uses ordinary scene lighting.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float authoredColorLightingStrength = 0.6f;

        [InspectorName("Roughness Variation")]
        [Tooltip("Strength of bounded smoothness variation from the imported roughness map. Dry Smoothness always remains the baseline.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float authoredRoughnessStrength = 1f;

        [Header("Broad Response")]
        [Range(0f, 2f)]
        [SerializeField]
        private float macroContrast = 0.6f;

        [Tooltip("Fraction of the legacy quantized pixel-cell response retained by consumers that provide it.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float legacyPixelCellInfluence = 1f;

        [Tooltip("Strength of the packed alpha channel's authored per-element value variation. Imported material-set entries use a separate normalized texture-form map instead.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float detailValueStrength = 0f;

        [Header("Structural Detail")]
        [SerializeField]
        private bool detailEnabled;

        [SerializeField]
        private StylizedSurfaceDetailLibrary detailLibrary;

        [Tooltip("Stable library entry ID. It is resolved to the current detail and optional texture-form slices at material-refresh time.")]
        [SerializeField]
        private string detailEntryId = string.Empty;

        [Tooltip("World metres covered by one complete packed-detail repeat.")]
        [Min(0.01f)]
        [SerializeField]
        private float detailWorldScale = 2f;

        [Range(0f, 2f)]
        [SerializeField]
        private float detailNormalStrength = 0f;

        [Range(0f, 2f)]
        [SerializeField]
        private float detailCavityStrength = 0f;

        [Tooltip("Remaps the packed cavity channel. Lower values expose wider gaps; higher values retain only deeper gaps.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float detailCavityBias = 0.5f;

        [Tooltip("Additional use of positive packed form values toward the material Light Color. Imported material-set entries use normalized texture form and ignore this value.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float detailFormHighlightStrength = 0f;

        [Header("Dry Finish")]
        [Range(0f, 1f)]
        [SerializeField]
        private float drySmoothness = 0.15f;

        [Range(0f, 1f)]
        [SerializeField]
        private float drySpecularStrength = 0.1f;

        [Tooltip("Signed dry smoothness modulation derived from packed alpha for prepacked detail entries.")]
        [Range(0f, 0.5f)]
        [SerializeField]
        private float finishVariationStrength = 0f;

#if UNITY_EDITOR
        public static event Action<StylizedSurfaceMaterialProfile>
            EditorProfileChanged;
#endif

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? name
                : displayName;

        public bool UsesTextureForm =>
            detailEnabled &&
            detailLibrary != null &&
            detailLibrary.EntryUsesTextureForm(DetailEntryId);
        public bool UsesFeatureTextureForm =>
            UsesTextureForm &&
            detailLibrary.EntryUsesFeatureTextureForm(DetailEntryId);

        // Compatibility aliases retained for existing callers and serialized
        // terminology. Payload selection is automatic from the library entry.
        public StylizedSurfaceMaterialPayloadMode PayloadMode =>
            UsesTextureForm
                ? StylizedSurfaceMaterialPayloadMode.AuthoredColor
                : StylizedSurfaceMaterialPayloadMode.PaletteDetail;
        public bool UsesAuthoredColor => UsesTextureForm;

        public Color BaseColor => baseColor;
        public Color DarkColor => darkColor;
        public Color LightColor => lightColor;
        public Color CavityColor => cavityColor;
        public float TextureFormStrength =>
            UsesTextureForm ? Mathf.Clamp01(authoredColorStrength) : 0f;
        public float SceneLightingResponse =>
            UsesTextureForm
                ? Mathf.Clamp01(authoredColorLightingStrength)
                : 1f;
        public float RoughnessVariationStrength =>
            UsesTextureForm ? Mathf.Clamp01(authoredRoughnessStrength) : 0f;
        public float AuthoredColorStrength => TextureFormStrength;
        public Color AuthoredColorTint => Color.white;
        public float AuthoredColorTintStrength => 0f;
        public float AuthoredColorLightingStrength => SceneLightingResponse;
        public float AuthoredRoughnessStrength => RoughnessVariationStrength;
        public float MacroContrast => Mathf.Clamp(macroContrast, 0f, 2f);
        public float LegacyPixelCellInfluence =>
            Mathf.Clamp01(legacyPixelCellInfluence);
        public float DetailValueStrength =>
            UsesTextureForm
                ? 0f
                : Mathf.Clamp(detailValueStrength, 0f, 2f);
        public bool DetailEnabled => detailEnabled;
        public StylizedSurfaceDetailLibrary DetailLibrary => detailLibrary;
        public string DetailEntryId => detailEntryId ?? string.Empty;
        public float DetailWorldScale => Mathf.Max(0.01f, detailWorldScale);
        public float DetailNormalStrength =>
            Mathf.Clamp(detailNormalStrength, 0f, 2f);
        public float DetailCavityStrength =>
            Mathf.Clamp(detailCavityStrength, 0f, 2f);
        public float DetailCavityBias => Mathf.Clamp01(detailCavityBias);
        public float DetailFormHighlightStrength =>
            UsesTextureForm
                ? 0f
                : Mathf.Clamp(detailFormHighlightStrength, 0f, 2f);
        public float DrySmoothness => Mathf.Clamp01(drySmoothness);
        public float DrySpecularStrength =>
            Mathf.Clamp01(drySpecularStrength);
        public float FinishVariationStrength =>
            UsesTextureForm
                ? 0f
                : Mathf.Clamp(finishVariationStrength, 0f, 0.5f);

        public bool TryResolveDetail(
            out Texture2DArray textureArray,
            out int sliceIndex)
        {
            textureArray = null;
            sliceIndex = -1;

            return detailEnabled &&
                   detailLibrary != null &&
                   detailLibrary.TryResolve(
                       DetailEntryId,
                       out textureArray,
                       out sliceIndex);
        }

        public bool TryResolveTextureForm(
            out Texture2DArray textureArray,
            out int sliceIndex)
        {
            textureArray = null;
            sliceIndex = -1;

            return UsesTextureForm &&
                   detailLibrary.TryResolveAuthoredColor(
                       DetailEntryId,
                       out textureArray,
                       out sliceIndex);
        }

        public bool TryResolveAuthoredColor(
            out Texture2DArray textureArray,
            out int sliceIndex)
        {
            return TryResolveTextureForm(out textureArray, out sliceIndex);
        }

#if UNITY_EDITOR
        public void NotifyEditorChanged()
        {
            EditorProfileChanged?.Invoke(this);
        }
#endif

        public void SetDisplayName(string value)
        {
            displayName = string.IsNullOrWhiteSpace(value)
                ? name
                : value.Trim();
        }
    }
}
