using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    /// <summary>
    /// Reusable dry material identity and cover compatibility used when
    /// River-coupled Ground response replaces or exposes a secondary bank or
    /// riverbed substrate. Wetness character belongs to the independent
    /// GroundHydrologyModifierProfile. Spatial placement remains owned by
    /// GeneratedGround material controls.
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

        [Header("Palette")]
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

        public Color BaseColor => baseColor;
        public Color DarkColor => darkColor;
        public Color LightColor => lightColor;
        public Color WetColor => wetColor;
        public float MacroContrast => Mathf.Clamp(macroContrast, 0f, 2f);
        public float PixelContrast => Mathf.Clamp(pixelContrast, 0f, 2f);
        public float DrySmoothness => Mathf.Clamp01(drySmoothness);
        public float DrySpecularStrength => Mathf.Clamp01(drySpecularStrength);
        public float WetDarkening => Mathf.Clamp(wetDarkening, 0f, 0.75f);
        public float WetTintStrength => Mathf.Clamp01(wetTintStrength);
        public float WetSmoothness => Mathf.Clamp01(wetSmoothness);
        public float VegetationRetention => Mathf.Clamp01(vegetationRetention);
        public float SnowRetention => Mathf.Clamp01(snowRetention);
        public float FrostRetention => Mathf.Clamp01(frostRetention);
        public float PaintedAccentRetention => Mathf.Clamp01(paintedAccentRetention);

        public void SetDisplayName(string value)
        {
            displayName = string.IsNullOrWhiteSpace(value)
                ? name
                : value.Trim();
        }
    }
}
