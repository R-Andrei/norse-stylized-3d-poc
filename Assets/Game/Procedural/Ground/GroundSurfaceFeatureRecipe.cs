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

        [Tooltip("Painted Accent Lines only. Visible shoulder-to-shoulder width in metres for the crowned crest ribbon. The generated ribbon footprint uses this value directly; legacy BodyWidth remains texture/debug support only.")]
        [Range(0.01f, 0.35f)]
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

        [Tooltip("Painted Accent Lines only. Maximum longitudinal rise in metres for the crowned crest ribbon. Height is applied along independently sampled ground normals. The ribbon rises from the ground at its start and returns to the ground at its finish; its underside remains empty.")]
        [Range(0f, 0.50f)]
        [SerializeField]
        private float paintedAccentFoldHeight = 0.018f;

        [Tooltip("Painted Accent Lines only. Additional cross-sectional crown height in metres. The centre vertex rises above the two ribbon shoulders, producing real cross-sectional body and a stable silhouette without adding an underside or collider. The crown fades through the same end envelope as Fold Height.")]
        [Range(0f, 0.05f)]
        [SerializeField]
        private float paintedAccentCrestCrownHeight = 0.02f;

        [Tooltip("Painted Accent Lines only. Controls deterministic stochastic variation in the profile search used to derive crest height. Zero approaches one clean profile; one allows several overlapping smooth basis functions and stronger slow height variation along the stroke.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentFoldIrregularity = 0.55f;

        [Tooltip("Painted Accent Lines only. Controls how much of each stroke length is used to blend the raised fold back into the ground. Zero keeps only a minimal anti-clipping fade; one uses long soft end tapers.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentFoldEndTaper = 0.65f;

        [Tooltip("Painted Accent Lines only. Uniform opaque ink colour used across the entire double-sided 3D stroke. It is intentionally unlit and does not vary with crown position, endpoints, stroke seed, scene lights, shadows, probes, or viewing side.")]
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
