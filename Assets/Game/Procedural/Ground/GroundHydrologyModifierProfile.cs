using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    /// <summary>
    /// Reusable wetness character applied after the resolved dry Ground and
    /// Bank substrate. Spatial placement remains owned by GeneratedGround
    /// material controls.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GHMP_NewGroundHydrologyModifier",
        menuName = "PS3D/Ground/Hydrology Modifier Profile")]
    public sealed class GroundHydrologyModifierProfile : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Human-facing name shown in GeneratedGround hydrology selectors.")]
        [SerializeField]
        private string displayName = "Shore Hydrology";

        [Header("Wet Colour Response")]
        [Tooltip("Colour target applied to the already resolved dry surface when this modifier is wet.")]
        [SerializeField]
        private Color wetTintColor = new Color(0.22f, 0.25f, 0.24f, 1f);

        [Tooltip("How strongly local Shore wetness shifts the resolved surface toward Wet Tint Colour.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float wetTintStrength = 0.35f;

        [Tooltip("How strongly local Shore wetness darkens the resolved surface.")]
        [Range(0f, 0.75f)]
        [SerializeField]
        private float wetDarkening = 0.30f;

        [Header("Surface Finish")]
        [Tooltip("How strongly local Shore wetness suppresses fine pixel-pattern contrast.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float pixelPatternSoftening = 0.35f;

        [Tooltip("How strongly local Shore wetness increases smoothness.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float smoothnessBoost = 0.30f;

        [Tooltip("Additional restrained specular multiplier contributed by fully local-wet surface.")]
        [Range(0f, 0.25f)]
        [SerializeField]
        private float specularBoost = 0.05f;

        [Header("Cover Interaction")]
        [Tooltip("How strongly local Shore wetness removes snow after Bank surface-cover retention is resolved.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float snowMeltInfluence = 0.65f;

        [Tooltip("How strongly local Shore wetness removes frost after Bank surface-cover retention is resolved.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float frostMeltInfluence = 0.75f;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? name
                : displayName;

        public Color WetTintColor => wetTintColor;
        public float WetTintStrength => Mathf.Clamp01(wetTintStrength);
        public float WetDarkening => Mathf.Clamp(wetDarkening, 0f, 0.75f);
        public float PixelPatternSoftening =>
            Mathf.Clamp01(pixelPatternSoftening);
        public float SmoothnessBoost => Mathf.Clamp01(smoothnessBoost);
        public float SpecularBoost => Mathf.Clamp(specularBoost, 0f, 0.25f);
        public float SnowMeltInfluence => Mathf.Clamp01(snowMeltInfluence);
        public float FrostMeltInfluence => Mathf.Clamp01(frostMeltInfluence);

        public void SetDisplayName(string value)
        {
            displayName = string.IsNullOrWhiteSpace(value)
                ? name
                : value.Trim();
        }
    }
}
