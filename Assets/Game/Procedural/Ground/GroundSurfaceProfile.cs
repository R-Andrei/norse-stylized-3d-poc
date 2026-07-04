using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    /// <summary>
    /// Surface/material identity for a generated ground patch. This is separate
    /// from GroundProfile, which only controls the physical heightfield shape.
    /// The first implementation uses this profile to bias deterministic static
    /// masks; future ground shaders, weather, grass, footsteps, and audio can
    /// read the same asset without adding more shape controls to GroundRecipe.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GSP_NewGroundSurface",
        menuName = "Programmatic Stylized 3D/Ground/Surface Profile")]
    public sealed class GroundSurfaceProfile : ScriptableObject
    {
        private const float FallbackPatchScale = 14f;
        private const float FallbackPatchContrast = 0.65f;
        private const float FallbackPatchSoftness = 0.45f;
        private const float FallbackExposureBias = 0.55f;
        private const float FallbackDampDepositBias = 0.35f;
        private const float FallbackVegetationSuitability = 0.2f;
        private const float FallbackRockyDrySuitability = 0.18f;
        private const float FallbackSnowEligibility = 0.85f;
        private const float FallbackRainAbsorption = 0.5f;
        private const float FallbackFootprintVisibility = 0.55f;
        private const float FallbackGrassRecoverySpeed = 0.5f;

        [Header("Identity")]
        [Tooltip("Human-readable label. This is not used as a serialized id.")]
        [SerializeField]
        private string displayName = "Ground Surface";

        [Header("Generated Patch Structure")]
        [Tooltip("Approximate metre scale of broad generated tonal patches.")]
        [Range(2f, 48f)]
        [SerializeField]
        private float patchScale = FallbackPatchScale;

        [Tooltip("How strongly the profile pushes generated tonal islands away from neutral.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float patchContrast = FallbackPatchContrast;

        [Tooltip("How softly generated patch values transition between low and high regions.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float patchEdgeSoftness = FallbackPatchSoftness;

        [Header("Static Surface Tendencies")]
        [Tooltip("Baseline tendency for high/up-facing places to hold snow, frost, or exposed lightening.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float exposureBias = FallbackExposureBias;

        [Tooltip("Baseline tendency for low/flat/shore places to collect dampness, mud, or dark deposits.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float dampDepositBias = FallbackDampDepositBias;

        [Tooltip("Baseline suitability for future grass, moss, or low vegetation systems.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float vegetationSuitability = FallbackVegetationSuitability;

        [Tooltip("Baseline suitability for dry/rocky flecks or exposed scrub patches.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float rockyDrySuitability = FallbackRockyDrySuitability;

        [Header("Future Weather / Interaction Hints")]
        [Tooltip("How eligible this surface is for future snow accumulation systems.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float snowEligibility = FallbackSnowEligibility;

        [Tooltip("How readily future rain/wetness systems should darken or retain water on this surface.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float rainAbsorption = FallbackRainAbsorption;

        [Tooltip("How visible future footprints/compression should be on this surface.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float footprintVisibility = FallbackFootprintVisibility;

        [Tooltip("How quickly future grass/trample state should visually recover on this surface.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float grassRecoverySpeed = FallbackGrassRecoverySpeed;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName)
            ? name
            : displayName;

        public float PatchScale => Mathf.Max(2f, patchScale);
        public float PatchContrast => Mathf.Clamp01(patchContrast);
        public float PatchEdgeSoftness => Mathf.Clamp01(patchEdgeSoftness);
        public float ExposureBias => Mathf.Clamp01(exposureBias);
        public float DampDepositBias => Mathf.Clamp01(dampDepositBias);
        public float VegetationSuitability => Mathf.Clamp01(vegetationSuitability);
        public float RockyDrySuitability => Mathf.Clamp01(rockyDrySuitability);
        public float SnowEligibility => Mathf.Clamp01(snowEligibility);
        public float RainAbsorption => Mathf.Clamp01(rainAbsorption);
        public float FootprintVisibility => Mathf.Clamp01(footprintVisibility);
        public float GrassRecoverySpeed => Mathf.Clamp01(grassRecoverySpeed);

        public static float ResolvePatchScale(GroundSurfaceProfile profile)
        {
            return profile != null ? profile.PatchScale : FallbackPatchScale;
        }

        public static float ResolvePatchContrast(GroundSurfaceProfile profile)
        {
            return profile != null ? profile.PatchContrast : FallbackPatchContrast;
        }

        public static float ResolvePatchEdgeSoftness(GroundSurfaceProfile profile)
        {
            return profile != null ? profile.PatchEdgeSoftness : FallbackPatchSoftness;
        }

        public static float ResolveExposureBias(GroundSurfaceProfile profile)
        {
            return profile != null ? profile.ExposureBias : FallbackExposureBias;
        }

        public static float ResolveDampDepositBias(GroundSurfaceProfile profile)
        {
            return profile != null ? profile.DampDepositBias : FallbackDampDepositBias;
        }

        public static float ResolveVegetationSuitability(GroundSurfaceProfile profile)
        {
            return profile != null
                ? profile.VegetationSuitability
                : FallbackVegetationSuitability;
        }

        public static float ResolveRockyDrySuitability(GroundSurfaceProfile profile)
        {
            return profile != null
                ? profile.RockyDrySuitability
                : FallbackRockyDrySuitability;
        }

        public static float ResolveSnowEligibility(GroundSurfaceProfile profile)
        {
            return profile != null ? profile.SnowEligibility : FallbackSnowEligibility;
        }

        public static float ResolveRainAbsorption(GroundSurfaceProfile profile)
        {
            return profile != null ? profile.RainAbsorption : FallbackRainAbsorption;
        }

        public static float ResolveFootprintVisibility(GroundSurfaceProfile profile)
        {
            return profile != null
                ? profile.FootprintVisibility
                : FallbackFootprintVisibility;
        }

        public static float ResolveGrassRecoverySpeed(GroundSurfaceProfile profile)
        {
            return profile != null
                ? profile.GrassRecoverySpeed
                : FallbackGrassRecoverySpeed;
        }
    }
}
