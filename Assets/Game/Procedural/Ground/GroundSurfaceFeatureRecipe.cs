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
        [Range(0.002f, 0.20f)]
        [SerializeField]
        private float paintedAccentStrokeWidth = 0.12f;

        [Tooltip("Painted Accent Lines only. Approximate requested stroke proposals per standard 40x40 ground patch. Regional concentration redistributes a fixed average share of this population; physical validation may reduce the final accepted count.")]
        [Range(0f, 2000f)]
        [SerializeField]
        private float paintedAccentStrokeDensity = 34f;

        [Tooltip("Painted Accent Lines only. High-level world-space size of sparse and dense distribution structure. Lower values create smaller, more frequent variation; higher values create broader local patches and larger coherent regions. This single control drives both underlying distribution scales.")]
        [Range(2f, 24f)]
        [SerializeField]
        private float paintedAccentDistributionPatchScale = 9f;

        [Tooltip("Painted Accent Lines only. High-level strength of sparse-versus-dense separation. Zero approaches an even field; one strongly concentrates marks into accent areas while retaining a protected non-zero sparse-region floor. This single control drives patch preference, regional redistribution, and the sparse floor.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentDistributionPatchiness = 0.70f;

        [SerializeField, HideInInspector]
        private float paintedAccentDistributionSparseFloor = 0.18f;

        [SerializeField, HideInInspector]
        private float paintedAccentCompositionRegionScale = 4f;

        [SerializeField, HideInInspector]
        private float paintedAccentCompositionDensityContrast = 0.70f;

        [Tooltip("Painted Accent Lines only. Authoritative target share of final valid projected marks assigned to complete two- or three-member companion clusters. Zero keeps every mark independent; one assigns every mathematically and geometrically feasible mark to a cluster. The resolved whole-mark quota is reported after generation.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentHorizontalCompanionStrength;

        [Tooltip("Painted Accent Lines only. Of all marks assigned to companion clusters, the authoritative target share assigned to three-member clusters. The remainder is assigned to pairs. Whole-cluster rounding is resolved deterministically and reported after generation.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentCompanionTripletShare = 0.45f;

        [Tooltip("Painted Accent Lines only. Controls where the fixed companion quota is concentrated. Zero distributes cluster anchors like the overall field; one strongly favours the denser accent regions. This changes cluster location only, never the global pair/triplet quota.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentCompanionAccentBias = 0.65f;

        [Tooltip("Painted Accent Lines only. Controls endpoint spacing inside companion clusters. Zero leaves broader gaps; one targets touching marks or an approximately one-to-two-pixel rendered break. Members remain independent and validate separately.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentCompanionTightness = 0.65f;

        [Tooltip("Painted Accent Lines only. Controls translation-driven stepping inside both pair and triplet clusters. Zero favours shallow offsets; one creates pronounced terraces and vertical centre differences. This does not change companion participation, pair/triplet quotas, or the bounded Angle Jitter contract.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentCompanionTripletVerticality = 1f;

        [SerializeField, HideInInspector]
        private bool paintedAccentHorizontalCompanionsInitialized;

        [SerializeField, HideInInspector]
        private bool paintedAccentCompanionTripletVerticalityInitialized;

        [SerializeField, HideInInspector]
        private bool paintedAccentCompanionQuotaControlsInitialized;

        [Tooltip("Painted Accent Lines only. Relative authoritative weight for Stepped pair layouts. Pair layout weights are normalized and converted to exact whole-cluster counts.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentPairSteppedWeight = 0.45f;

        [Tooltip("Painted Accent Lines only. Relative authoritative weight for Shoulder pair layouts.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentPairShoulderWeight = 0.30f;

        [Tooltip("Painted Accent Lines only. Relative authoritative weight for Offset pair layouts.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentPairOffsetWeight = 0.20f;

        [Tooltip("Painted Accent Lines only. Relative authoritative weight for the quieter Shallow Offset pair layout. This layout preserves a visible break or pair-local offset and never becomes a seamless collinear continuation.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentPairShallowWeight = 0.05f;

        [Tooltip("Painted Accent Lines only. Relative authoritative weight for Stepped Run triplets.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentTripletSteppedRunWeight = 0.40f;

        [Tooltip("Painted Accent Lines only. Relative authoritative weight for Crown Run triplets.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentTripletCrownRunWeight = 0.30f;

        [Tooltip("Painted Accent Lines only. Relative authoritative weight for Broken Terrace triplets.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentTripletBrokenTerraceWeight = 0.25f;

        [Tooltip("Painted Accent Lines only. Relative authoritative weight for quieter Shallow Run triplets.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentTripletShallowRunWeight = 0.05f;

        [SerializeField, HideInInspector]
        private bool paintedAccentCompanionLayoutWeightsInitialized;

        [Tooltip("Painted Accent Lines only. Relative selection weight for the complete two-sided mound glyph family. Family weights are normalized internally and do not alter authored stroke length or width.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentCompleteMoundWeight = 0.20f;

        [Tooltip("Painted Accent Lines only. Relative selection weight for the asymmetric mound glyph family. Family weights are normalized internally and do not alter authored stroke length or width.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentAsymmetricMoundWeight = 0.30f;

        [Tooltip("Painted Accent Lines only. Relative selection weight for the single-shoulder glyph family. Family weights are normalized internally and do not alter authored stroke length or width.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentSingleShoulderWeight = 0.30f;

        [Tooltip("Painted Accent Lines only. Relative selection weight for the shallow-crest glyph family. Family weights are normalized internally and do not alter authored stroke length or width.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentShallowCrestWeight = 0.20f;

        [SerializeField, HideInInspector]
        private bool paintedAccentGlyphFamilyWeightsInitialized;

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

        [Tooltip("Painted Accent Lines only. Controls smooth lateral curvature of the ground-surface stroke path before the projected contour profile is applied. Zero keeps the baseline nearly straight; one permits the strongest non-looping organic bend. This is independent from Profile Irregularity and generic feature Contrast.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float paintedAccentStrokePathWiggle = 0.35f;

        [SerializeField, HideInInspector]
        private bool paintedAccentStrokePathWiggleInitialized;

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

                return Mathf.Clamp(value, 0.002f, 0.20f);
            }
        }

        public float PaintedAccentStrokeDensity =>
            Mathf.Clamp(paintedAccentStrokeDensity, 0f, 2000f);

        public float PaintedAccentDistributionScale =>
            Mathf.Clamp(paintedAccentDistributionPatchScale, 2f, 24f);

        public float PaintedAccentDistributionContrast =>
            Mathf.Clamp01(paintedAccentDistributionPatchiness);

        public float PaintedAccentDistributionPatchScale =>
            PaintedAccentDistributionScale;

        public float PaintedAccentDistributionPatchiness =>
            PaintedAccentDistributionContrast;

        public float PaintedAccentDistributionSparseFloor =>
            Mathf.Lerp(0.40f, 0.10f, PaintedAccentDistributionContrast);

        public float PaintedAccentCompositionRegionScale
        {
            get
            {
                float normalizedScale =
                    Mathf.InverseLerp(
                        2f,
                        24f,
                        PaintedAccentDistributionScale);
                return Mathf.Lerp(1f, 13.5f, normalizedScale);
            }
        }

        public float PaintedAccentCompositionDensityContrast =>
            PaintedAccentDistributionContrast;

        public float PaintedAccentCompanionParticipation =>
            paintedAccentHorizontalCompanionsInitialized
                ? Mathf.Clamp01(paintedAccentHorizontalCompanionStrength)
                : 0f;

        public float PaintedAccentHorizontalCompanionStrength =>
            PaintedAccentCompanionParticipation;

        public float PaintedAccentCompanionTripletShare =>
            paintedAccentCompanionQuotaControlsInitialized
                ? Mathf.Clamp01(paintedAccentCompanionTripletShare)
                : 0.45f;

        public float PaintedAccentClusterRegionBias =>
            paintedAccentCompanionQuotaControlsInitialized
                ? Mathf.Clamp01(paintedAccentCompanionAccentBias)
                : 0.65f;

        public float PaintedAccentCompanionAccentBias =>
            PaintedAccentClusterRegionBias;

        public float PaintedAccentCompanionTightness =>
            paintedAccentHorizontalCompanionsInitialized
                ? Mathf.Clamp01(paintedAccentCompanionTightness)
                : 0.65f;

        public float PaintedAccentCompanionTripletVerticality =>
            paintedAccentCompanionTripletVerticalityInitialized
                ? Mathf.Clamp01(paintedAccentCompanionTripletVerticality)
                : 1f;

        public float PaintedAccentClusterVerticality =>
            PaintedAccentCompanionTripletVerticality;

        public Vector4 PaintedAccentCompanionPairLayoutWeights
        {
            get
            {
                if (!paintedAccentCompanionLayoutWeightsInitialized)
                {
                    return new Vector4(0.45f, 0.30f, 0.20f, 0.05f);
                }

                Vector4 weights =
                    new Vector4(
                        Mathf.Clamp01(paintedAccentPairSteppedWeight),
                        Mathf.Clamp01(paintedAccentPairShoulderWeight),
                        Mathf.Clamp01(paintedAccentPairOffsetWeight),
                        Mathf.Clamp01(paintedAccentPairShallowWeight));
                if (weights.x + weights.y + weights.z + weights.w <= 0.0001f)
                {
                    weights.x = 1f;
                }

                return weights;
            }
        }

        public Vector4 PaintedAccentCompanionTripletLayoutWeights
        {
            get
            {
                if (!paintedAccentCompanionLayoutWeightsInitialized)
                {
                    return new Vector4(0.40f, 0.30f, 0.25f, 0.05f);
                }

                Vector4 weights =
                    new Vector4(
                        Mathf.Clamp01(paintedAccentTripletSteppedRunWeight),
                        Mathf.Clamp01(paintedAccentTripletCrownRunWeight),
                        Mathf.Clamp01(paintedAccentTripletBrokenTerraceWeight),
                        Mathf.Clamp01(paintedAccentTripletShallowRunWeight));
                if (weights.x + weights.y + weights.z + weights.w <= 0.0001f)
                {
                    weights.x = 1f;
                }

                return weights;
            }
        }

        public float PaintedAccentCompleteMoundWeight =>
            Mathf.Clamp01(paintedAccentCompleteMoundWeight);

        public float PaintedAccentAsymmetricMoundWeight =>
            Mathf.Clamp01(paintedAccentAsymmetricMoundWeight);

        public float PaintedAccentSingleShoulderWeight =>
            Mathf.Clamp01(paintedAccentSingleShoulderWeight);

        public float PaintedAccentShallowCrestWeight =>
            Mathf.Clamp01(paintedAccentShallowCrestWeight);

        public Vector4 PaintedAccentGlyphFamilyWeights
        {
            get
            {
                if (!paintedAccentGlyphFamilyWeightsInitialized)
                {
                    return new Vector4(0.20f, 0.30f, 0.30f, 0.20f);
                }

                Vector4 weights =
                    new Vector4(
                        PaintedAccentCompleteMoundWeight,
                        PaintedAccentAsymmetricMoundWeight,
                        PaintedAccentSingleShoulderWeight,
                        PaintedAccentShallowCrestWeight);
                if (weights.x + weights.y + weights.z + weights.w <= 0.0001f)
                {
                    weights.x = 1f;
                }

                return weights;
            }
        }

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

        public float PaintedAccentStrokePathWiggle =>
            paintedAccentStrokePathWiggleInitialized
                ? Mathf.Clamp01(paintedAccentStrokePathWiggle)
                : 0.35f;

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
