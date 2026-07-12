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

        [Tooltip("Stable world X/Z direction for directional features. Directional Streaks consume this as their directional bias. Painted Accent descriptors use Facing Direction Degrees instead; Pooled Wetness and Trampled Wear may ignore it.")]
        [SerializeField]
        private Vector2 direction = new Vector2(0.82f, 0.36f);

        [Tooltip("Stable per-feature seed offset mixed with the material seed.")]
        [SerializeField]
        private int seedOffset;

        [Tooltip("Painted Accent Lines only. Visible projected-contour width in metres. The transformed footprint uses this value directly; BodyWidth remains texture/debug support only.")]
        [Range(0.01f, 0.35f)]
        [SerializeField]
        private float paintedAccentStrokeWidth = 0.12f;

        [Tooltip("Painted Accent Lines only. Approximate number of weighted stroke proposals per standard 40x40 ground patch before river, modifier, sampling, slope, and grade rejection. Final accepted count may be lower because rejected proposals are not backfilled.")]
        [Range(0f, 240f)]
        [SerializeField]
        private float paintedAccentStrokeDensity = 34f;

        [Tooltip("Painted Accent Lines only. World-space scale in metres of the continuous deterministic density patches used to prefer some regions and leave others sparse. Larger values create broader patches without hard island boundaries.")]
        [Range(2f, 24f)]
        [SerializeField]
        private float paintedAccentDistributionPatchScale = 9f;

        [Tooltip("Painted Accent Lines only. Strength of continuous weighted patch distribution. Zero approaches broad random coverage; one strongly prefers high-noise regions while retaining a non-zero chance elsewhere.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentDistributionPatchiness = 0.70f;

        [Tooltip("Painted Accent Lines only. Minimum patch-weight floor retained in sparse regions before semantic weighting. Lower values allow colder regions to become much quieter while preserving a non-zero proposal chance instead of creating hard exclusion islands.")]
        [Range(0.02f, 0.40f)]
        [SerializeField]
        private float paintedAccentDistributionSparseFloor = 0.18f;

        [Tooltip("Painted Accent Lines only. Minimum accepted ground-surface descriptor length in metres.")]
        [Range(0.20f, 4.0f)]
        [SerializeField]
        private float paintedAccentStrokeLengthMin = 0.55f;

        [Tooltip("Painted Accent Lines only. Maximum accepted ground-surface descriptor length in metres.")]
        [Range(0.25f, 6.0f)]
        [SerializeField]
        private float paintedAccentStrokeLengthMax = 1.55f;

        [Tooltip("Painted Accent Lines only. Local X/Z player or camera-facing direction in degrees. Accepted ground-surface descriptors are perpendicular to this direction, then Angle Jitter Degrees rolls around that perpendicular stroke angle.")]
        [Range(0f, 360f)]
        [SerializeField]
        [FormerlySerializedAs("paintedAccentStrokeBaseAngleDegrees")]
        private float paintedAccentStrokeFacingDirectionDegrees = 90f;

        [Tooltip("Painted Accent Lines only. Maximum signed angle offset in degrees applied around the perpendicular stroke angle derived from Facing Direction Degrees. Each stroke rolls independently in [-value, +value].")]
        [Range(0f, 30f)]
        [SerializeField]
        private float paintedAccentStrokeAngleJitterDegrees = 18f;

        [Tooltip("Painted Accent Lines only. Primary mesh-free projected contour amplitude in metres, applied toward fixed world +Z, which is permanent gameplay screen-up.")]
        [Range(0f, 0.50f)]
        [SerializeField]
        private float paintedAccentFoldHeight = 0.018f;

        [Tooltip("Painted Accent Lines only. Additional projected crest/cap amplitude in metres, added to the fixed world +Z contour displacement.")]
        [Range(0f, 0.05f)]
        [SerializeField]
        private float paintedAccentCrestCrownHeight = 0.02f;

        [Tooltip("Painted Accent Lines only. Controls seeded longitudinal variation in the mesh-free projected contour profile.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentFoldIrregularity = 0.55f;

        [Tooltip("Painted Accent Lines only. Controls the projected contour and visible-width endpoint envelope.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentFoldEndTaper = 0.65f;

        [Tooltip("Painted Accent Lines only. Family/variant-authored opaque ink colour blended through the generated projected coverage field into ground albedo.")]
        [ColorUsage(false, false)]
        [SerializeField]
        private Color paintedAccentInkColor =
            new Color(0.12f, 0.10f, 0.08f, 1f);

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

                return Mathf.Clamp(value, 0.01f, 0.35f);
            }
        }

        public float PaintedAccentStrokeDensity =>
            Mathf.Clamp(paintedAccentStrokeDensity, 0f, 240f);

        public float PaintedAccentDistributionPatchScale =>
            Mathf.Clamp(paintedAccentDistributionPatchScale, 2f, 24f);

        public float PaintedAccentDistributionPatchiness =>
            Mathf.Clamp01(paintedAccentDistributionPatchiness);

        public float PaintedAccentDistributionSparseFloor =>
            Mathf.Clamp(paintedAccentDistributionSparseFloor, 0.02f, 0.40f);

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
            Mathf.Clamp(paintedAccentFoldHeight, 0f, 0.50f);

        public float PaintedAccentCrestCrownHeight =>
            Mathf.Clamp(paintedAccentCrestCrownHeight, 0f, 0.05f);

        public float PaintedAccentFoldIrregularity =>
            Mathf.Clamp01(paintedAccentFoldIrregularity);

        public float PaintedAccentFoldEndTaper =>
            Mathf.Clamp01(paintedAccentFoldEndTaper);

        public Color PaintedAccentInkColor
        {
            get
            {
                Color value = paintedAccentInkColor;
                value.r = Mathf.Clamp01(value.r);
                value.g = Mathf.Clamp01(value.g);
                value.b = Mathf.Clamp01(value.b);
                value.a = 1f;
                return value;
            }
        }

        public bool CanApplyAsShaderOnly =>
            enabled &&
            kind != GroundSurfaceFeatureKind.None &&
            costClass == GroundSurfaceFeatureCostClass.ShaderOnly &&
            Strength > 0f;
    }
}
