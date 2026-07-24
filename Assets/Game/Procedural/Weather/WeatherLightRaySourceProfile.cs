using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    [CreateAssetMenu(
        fileName = "WLRSP_Sun",
        menuName = "PS3D/Weather/LightRay Source Profile",
        order = 40)]
    public sealed class WeatherLightRaySourceProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private WeatherLightRaySourceKind sourceKind =
            WeatherLightRaySourceKind.Sun;

        [SerializeField]
        [ColorUsage(false, true)]
        private Color colourMultiplier = Color.white;

        [Header("Source Gate")]
        [SerializeField, Min(0f)]
        private float minimumSourceIntensity = 0.01f;

        [SerializeField, Range(-0.25f, 0.75f)]
        private float minimumSourceElevation = 0.1f;

        [SerializeField, Min(0.001f)]
        private float elevationFadeRange = 0.15f;

        [SerializeField, Range(0f, 75f)]
        private float maximumPresentationLeanDegrees = 25f;

        public WeatherLightRaySourceKind SourceKind => sourceKind;
        public Color ColourMultiplier => colourMultiplier;
        public float MinimumSourceIntensity => minimumSourceIntensity;
        public float MinimumSourceElevation => minimumSourceElevation;
        public float ElevationFadeRange => elevationFadeRange;
        public float MaximumPresentationLeanDegrees =>
            maximumPresentationLeanDegrees;

        public bool EvaluateAvailability(
            Light sourceLight,
            float elevation,
            out string reason)
        {
            reason = string.Empty;
            if (sourceLight == null)
            {
                reason = "No source light is assigned.";
                return false;
            }

            if (sourceLight.type != LightType.Directional)
            {
                reason = "The LightRay source light is not directional.";
                return false;
            }

            if (!sourceLight.enabled ||
                !sourceLight.gameObject.activeInHierarchy)
            {
                reason = "The LightRay source light is disabled or inactive.";
                return false;
            }

            if (sourceLight.intensity < minimumSourceIntensity)
            {
                reason =
                    "The LightRay source intensity is below the profile gate.";
                return false;
            }

            if (elevation < minimumSourceElevation)
            {
                reason =
                    "The LightRay source elevation is inside the horizon dead zone.";
                return false;
            }

            return true;
        }

        private void OnValidate()
        {
            minimumSourceIntensity = Mathf.Max(
                0f,
                minimumSourceIntensity);
            minimumSourceElevation = Mathf.Clamp(
                minimumSourceElevation,
                -0.25f,
                0.75f);
            elevationFadeRange = Mathf.Max(0.001f, elevationFadeRange);
            maximumPresentationLeanDegrees = Mathf.Clamp(
                maximumPresentationLeanDegrees,
                0f,
                75f);
        }
    }
}
