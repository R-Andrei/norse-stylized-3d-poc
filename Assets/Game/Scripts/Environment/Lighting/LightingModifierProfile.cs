using UnityEngine;

namespace Game.Lighting
{
    [CreateAssetMenu(
        fileName = "LMP_NewModifier",
        menuName = "Game/Lighting/Lighting Modifier Profile",
        order = 11)]
    public sealed class LightingModifierProfile : ScriptableObject
    {
        [Header("Sun")]
        [ColorUsage(false, false)]
        public Color sunTint = Color.white;

        [Min(0f)]
        public float sunIntensityMultiplier = 1f;

        public float sunIntensityOffset;

        [Header("Ambient floor")]
        [ColorUsage(false, false)]
        public Color ambientTint = Color.white;

        [Min(0f)]
        public float ambientIntensityMultiplier = 1f;

        public float ambientIntensityOffset;

        [Header("Procedural skybox")]
        [ColorUsage(false, false)]
        public Color skyTint = Color.white;

        [ColorUsage(false, false)]
        public Color skyGroundTint = Color.white;

        [Min(0f)]
        public float skyAtmosphereThicknessMultiplier = 1f;

        [Min(0f)]
        public float skyExposureMultiplier = 1f;

        public float skyExposureOffset;

        [Header("Environment reflections")]
        [Min(0f)]
        public float reflectionIntensityMultiplier = 1f;

        public float reflectionIntensityOffset;

        [Header("Fog")]
        [ColorUsage(false, false)]
        public Color fogTint = Color.white;

        [Min(0f)]
        public float fogDensityMultiplier = 1f;

        public float fogDensityOffset;

        public void Apply(ref TimeOfDayLightingState state, float weight)
        {
            weight = Mathf.Clamp01(weight);

            if (weight <= 0f)
            {
                return;
            }

            state.sunColor = MultiplyColors(
                state.sunColor,
                Color.Lerp(Color.white, sunTint, weight));

            state.sunIntensity = Mathf.Max(
                0f,
                state.sunIntensity *
                    Mathf.Lerp(1f, sunIntensityMultiplier, weight) +
                sunIntensityOffset * weight);

            state.ambientColor = MultiplyColors(
                state.ambientColor,
                Color.Lerp(Color.white, ambientTint, weight));

            state.ambientIntensity = Mathf.Max(
                0f,
                state.ambientIntensity *
                    Mathf.Lerp(1f, ambientIntensityMultiplier, weight) +
                ambientIntensityOffset * weight);

            state.skyTint = MultiplyColors(
                state.skyTint,
                Color.Lerp(Color.white, skyTint, weight));

            state.skyGroundColor = MultiplyColors(
                state.skyGroundColor,
                Color.Lerp(Color.white, skyGroundTint, weight));

            state.skyAtmosphereThickness = Mathf.Max(
                0f,
                state.skyAtmosphereThickness *
                    Mathf.Lerp(
                        1f,
                        skyAtmosphereThicknessMultiplier,
                        weight));

            state.skyExposure = Mathf.Max(
                0f,
                state.skyExposure *
                    Mathf.Lerp(1f, skyExposureMultiplier, weight) +
                skyExposureOffset * weight);

            state.reflectionIntensity = Mathf.Clamp01(
                state.reflectionIntensity *
                    Mathf.Lerp(1f, reflectionIntensityMultiplier, weight) +
                reflectionIntensityOffset * weight);

            state.fogColor = MultiplyColors(
                state.fogColor,
                Color.Lerp(Color.white, fogTint, weight));

            state.fogDensity = Mathf.Max(
                0f,
                state.fogDensity *
                    Mathf.Lerp(1f, fogDensityMultiplier, weight) +
                fogDensityOffset * weight);
        }

        private static Color MultiplyColors(Color left, Color right)
        {
            return new Color(
                left.r * right.r,
                left.g * right.g,
                left.b * right.b,
                1f);
        }
    }
}