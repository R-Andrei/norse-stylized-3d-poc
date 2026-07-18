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

        [Header("Payload")]
        [SerializeField]
        private StylizedSurfaceMaterialPayloadMode payloadMode =
            StylizedSurfaceMaterialPayloadMode.PaletteDetail;

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

        [Header("Authored Color")]
        [Tooltip("Blend strength of the optional authored colour-array payload. Palette Detail materials ignore this value.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float authoredColorStrength = 1f;

        [Tooltip("Optional reusable tint applied to the authored colour payload.")]
        [SerializeField]
        private Color authoredColorTint = Color.white;

        [Range(0f, 1f)]
        [SerializeField]
        private float authoredColorTintStrength;

        [Tooltip("Fraction of ordinary scene lighting retained over the authored stylized colour. Lower values preserve more painted form.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float authoredColorLightingStrength = 0.6f;

        [Tooltip("How strongly packed alpha roughness replaces the profile dry-smoothness target for authored-colour materials.")]
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

        [Tooltip("Strength of the packed alpha channel's authored per-element value variation. Authored Color materials use alpha as roughness instead.")]
        [Range(0f, 2f)]
        [SerializeField]
        private float detailValueStrength = 0f;

        [Header("Structural Detail")]
        [SerializeField]
        private bool detailEnabled;

        [SerializeField]
        private StylizedSurfaceDetailLibrary detailLibrary;

        [Tooltip("Stable library entry ID. It is resolved to the current detail and optional authored-colour slices at material-refresh time.")]
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

        [Tooltip("Additional use of positive authored form values toward the material light colour. Authored Color materials normally leave this at zero.")]
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

        [Tooltip("Signed dry smoothness modulation derived from packed alpha for Palette Detail materials.")]
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

        public StylizedSurfaceMaterialPayloadMode PayloadMode => payloadMode;
        public bool UsesAuthoredColor =>
            payloadMode == StylizedSurfaceMaterialPayloadMode.AuthoredColor;
        public Color BaseColor => baseColor;
        public Color DarkColor => darkColor;
        public Color LightColor => lightColor;
        public Color CavityColor => cavityColor;
        public float AuthoredColorStrength =>
            UsesAuthoredColor ? Mathf.Clamp01(authoredColorStrength) : 0f;
        public Color AuthoredColorTint => authoredColorTint;
        public float AuthoredColorTintStrength =>
            UsesAuthoredColor ? Mathf.Clamp01(authoredColorTintStrength) : 0f;
        public float AuthoredColorLightingStrength =>
            UsesAuthoredColor
                ? Mathf.Clamp01(authoredColorLightingStrength)
                : 1f;
        public float AuthoredRoughnessStrength =>
            UsesAuthoredColor ? Mathf.Clamp01(authoredRoughnessStrength) : 0f;
        public float MacroContrast => Mathf.Clamp(macroContrast, 0f, 2f);
        public float LegacyPixelCellInfluence =>
            Mathf.Clamp01(legacyPixelCellInfluence);
        public float DetailValueStrength =>
            UsesAuthoredColor
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
            UsesAuthoredColor
                ? 0f
                : Mathf.Clamp(detailFormHighlightStrength, 0f, 2f);
        public float DrySmoothness => Mathf.Clamp01(drySmoothness);
        public float DrySpecularStrength =>
            Mathf.Clamp01(drySpecularStrength);
        public float FinishVariationStrength =>
            UsesAuthoredColor
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

        public bool TryResolveAuthoredColor(
            out Texture2DArray textureArray,
            out int sliceIndex)
        {
            textureArray = null;
            sliceIndex = -1;

            return detailEnabled &&
                   UsesAuthoredColor &&
                   detailLibrary != null &&
                   detailLibrary.TryResolveAuthoredColor(
                       DetailEntryId,
                       out textureArray,
                       out sliceIndex);
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
