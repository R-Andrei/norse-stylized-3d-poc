using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    [Serializable]
    public sealed class GroundSurfaceFeatureRecipe
    {
        [Tooltip("Feature module represented by this recipe entry. Shader-only entries are resolved as a stack: variants may combine Directional Streaks, Pooled Wetness, Trampled Wear, Painted Accent Lines, and later supported layers.")]
        [SerializeField]
        private GroundSurfaceFeatureKind kind =
            GroundSurfaceFeatureKind.None;

        [Tooltip("Disables this feature entry without deleting its authored values.")]
        [SerializeField]
        private bool enabled = true;

        [Tooltip("Budget class used by current and future quality gates. Shader Only entries can be resolved by the ground shader feature stack when their feature kind is implemented.")]
        [SerializeField]
        private GroundSurfaceFeatureCostClass costClass =
            GroundSurfaceFeatureCostClass.ShaderOnly;

        [Tooltip("Primary feature intensity. Zero leaves the renderer unchanged.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float strength = 0.5f;

        [Tooltip("World-space feature scale in metres.")]
        [Range(0.1f, 30f)]
        [SerializeField]
        private float scale = 5f;

        [Tooltip("Shape contrast inside the feature mask.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float contrast = 0.5f;

        [Tooltip("How strongly semantic ground masks gate the feature. Zero applies broadly; one strongly respects suitable generated masks.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float maskInfluence = 0.5f;

        [Tooltip("Stable world X/Z direction for directional features. Directional Streaks and Painted Accent Lines consume this as a directional bias; Pooled Wetness and Trampled Wear may ignore it.")]
        [SerializeField]
        private Vector2 direction = new Vector2(0.82f, 0.36f);

        [Tooltip("Stable per-feature seed offset mixed with the material seed.")]
        [SerializeField]
        private int seedOffset;

        public GroundSurfaceFeatureKind Kind => kind;

        public bool Enabled => enabled;

        public GroundSurfaceFeatureCostClass CostClass => costClass;

        public float Strength => Mathf.Clamp01(strength);

        public float Scale => Mathf.Clamp(scale, 0.1f, 30f);

        public float Contrast => Mathf.Clamp01(contrast);

        public float MaskInfluence => Mathf.Clamp01(maskInfluence);

        public Vector2 Direction
        {
            get
            {
                if (direction.sqrMagnitude < 0.0001f)
                {
                    return new Vector2(1f, 0f);
                }

                return direction.normalized;
            }
        }

        public int SeedOffset => seedOffset;

        public bool CanApplyAsShaderOnly =>
            enabled &&
            kind != GroundSurfaceFeatureKind.None &&
            costClass == GroundSurfaceFeatureCostClass.ShaderOnly &&
            Strength > 0f;
    }
}
