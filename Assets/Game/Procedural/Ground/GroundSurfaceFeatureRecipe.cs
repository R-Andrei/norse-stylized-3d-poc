using System;
using UnityEngine;
using UnityEngine.Serialization;

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

        [Tooltip("Stable world X/Z direction for directional features. Directional Streaks consume this as their directional bias. Painted Accent 3D strokes use Facing Direction Degrees instead; Pooled Wetness and Trampled Wear may ignore it.")]
        [SerializeField]
        private Vector2 direction = new Vector2(0.82f, 0.36f);

        [Tooltip("Stable per-feature seed offset mixed with the material seed.")]
        [SerializeField]
        private int seedOffset;

        [Tooltip("Painted Accent Lines only. Visible width in metres for the narrow secondary ridge. The generated ridge footprint uses this value directly; legacy BodyWidth remains texture/debug support only.")]
        [Range(0.04f, 0.35f)]
        [SerializeField]
        private float paintedAccentStrokeWidth = 0.12f;

        [Tooltip("Painted Accent Lines only. Approximate target number of generated 3D surface strokes per standard 40x40 ground patch before placement rejection.")]
        [Range(0f, 80f)]
        [SerializeField]
        private float paintedAccentStrokeDensity = 34f;

        [Tooltip("Painted Accent Lines only. Minimum generated 3D surface stroke length in metres.")]
        [Range(0.20f, 4.0f)]
        [SerializeField]
        private float paintedAccentStrokeLengthMin = 0.55f;

        [Tooltip("Painted Accent Lines only. Maximum generated 3D surface stroke length in metres.")]
        [Range(0.25f, 6.0f)]
        [SerializeField]
        private float paintedAccentStrokeLengthMax = 1.55f;

        [Tooltip("Painted Accent Lines only. Local X/Z player or camera-facing direction in degrees. Generated 3D surface strokes are perpendicular to this direction, then Angle Jitter Degrees rolls around that perpendicular stroke angle.")]
        [Range(0f, 360f)]
        [SerializeField]
        [FormerlySerializedAs("paintedAccentStrokeBaseAngleDegrees")]
        private float paintedAccentStrokeFacingDirectionDegrees = 90f;

        [Tooltip("Painted Accent Lines only. Maximum signed angle offset in degrees applied around the perpendicular stroke angle derived from Facing Direction Degrees. Each stroke rolls independently in [-value, +value].")]
        [Range(0f, 30f)]
        [SerializeField]
        private float paintedAccentStrokeAngleJitterDegrees = 18f;

        [Tooltip("Painted Accent Lines only. Maximum raised height in metres for the narrow secondary ridge. Height is applied along the sampled ground normal and tapers into the ground at both side boundaries and both ends.")]
        [Range(0f, 0.15f)]
        [SerializeField]
        private float paintedAccentFoldHeight = 0.018f;

        [Tooltip("Painted Accent Lines only. Controls deterministic stochastic variation in the 3D fold profile. Zero approaches one clean broad profile; one allows several overlapping smooth basis functions and stronger slow height variation along the stroke.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentFoldIrregularity = 0.55f;

        [Tooltip("Painted Accent Lines only. Controls how much of each stroke length is used to blend the raised fold back into the ground. Zero keeps only a minimal anti-clipping fade; one uses long soft end tapers.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentFoldEndTaper = 0.65f;

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

        public float PaintedAccentStrokeWidth
        {
            get
            {
                float value = paintedAccentStrokeWidth;
                if (value <= 0.001f)
                {
                    value = 0.12f;
                }

                return Mathf.Clamp(value, 0.04f, 0.35f);
            }
        }

        public float PaintedAccentStrokeDensity =>
            Mathf.Clamp(paintedAccentStrokeDensity, 0f, 80f);

        public float PaintedAccentStrokeLengthMin =>
            Mathf.Clamp(paintedAccentStrokeLengthMin, 0.20f, 4.0f);

        public float PaintedAccentStrokeLengthMax =>
            Mathf.Max(
                PaintedAccentStrokeLengthMin + 0.05f,
                Mathf.Clamp(paintedAccentStrokeLengthMax, 0.25f, 6.0f));

        public float PaintedAccentStrokeFacingDirectionDegrees =>
            Mathf.Repeat(paintedAccentStrokeFacingDirectionDegrees, 360f);

        public float PaintedAccentStrokeAngleJitterDegrees =>
            Mathf.Clamp(paintedAccentStrokeAngleJitterDegrees, 0f, 30f);

        public float PaintedAccentFoldHeight =>
            Mathf.Clamp(paintedAccentFoldHeight, 0f, 0.15f);

        public float PaintedAccentFoldIrregularity =>
            Mathf.Clamp01(paintedAccentFoldIrregularity);

        public float PaintedAccentFoldEndTaper =>
            Mathf.Clamp01(paintedAccentFoldEndTaper);

        public bool CanApplyAsShaderOnly =>
            enabled &&
            kind != GroundSurfaceFeatureKind.None &&
            costClass == GroundSurfaceFeatureCostClass.ShaderOnly &&
            Strength > 0f;
    }
}
