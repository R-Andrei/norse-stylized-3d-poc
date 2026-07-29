using ProgrammaticStylized3D.Geometry.Ground;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Trees/Tree Reference Gallery")]
    public sealed class TreeReferenceGallery : MonoBehaviour
    {
        public const string SourceRootPath = "Assets/References/Trees";
        public const int RequiredModelCount = 20;
        public const int RequiredTextureCount = 12;

        [Header("Reference Surface")]
        [SerializeField]
        [Tooltip("Explicit Ground used for vertical-slice height sampling and for locating/orienting the off-map complete gallery. The gallery remains a separate sibling/root object and does not inherit Ground ownership from hierarchy.")]
        private GeneratedGround referenceGround;

        [Header("Gallery Layout")]
        [SerializeField]
        [Min(0.01f)]
        private float sourceScale = 1f;

        [SerializeField]
        private bool alignToGround = true;

        [SerializeField]
        [Min(1f)]
        private float familyRowSpacing = 18f;

        [SerializeField]
        [Min(1f)]
        [Tooltip("Minimum centre-to-centre spacing between the imported reference and its procedural comparison slot.")]
        private float pairColumnSpacing = 12f;

        [SerializeField]
        [Min(0.1f)]
        [Tooltip("Additional canopy clearance added when audited source width exceeds the configured minimum pair spacing.")]
        private float comparisonPairOffset = 4f;


        [Header("Complete Imported Gallery")]
        [SerializeField]
        [Min(0.5f)]
        [Tooltip("Clear distance between the playable Ground domain's left edge and the complete imported gallery's nearest content or shadow pad.")]
        private float completeGalleryLeftClearance = 8f;

        [SerializeField]
        [Min(0.25f)]
        [Tooltip("Minimum free space between adjacent imported/comparison rows within a family page.")]
        private float completeGalleryRowGap = 2f;

        [SerializeField]
        [Min(0.5f)]
        [Tooltip("Clear space between adjacent family blocks in the simultaneous complete-gallery strip.")]
        private float completeGalleryFamilyGap = 6f;

        [SerializeField]
        [Min(0.25f)]
        [Tooltip("Extra border around each measured family block footprint used by its builder-owned shadow receiver pad.")]
        private float completeGalleryPadMargin = 2f;

        [SerializeField]
        [Min(0.01f)]
        private float completeGalleryPadThickness = 0.2f;

        [SerializeField]
        private Color completeGalleryPadColor =
            new Color(0.30f, 0.36f, 0.30f, 1f);

        [Header("Reference Rendering")]
        [SerializeField]
        private bool windEnabled = true;

        [SerializeField]
        private TreeImportedWindMaskMode importedWindMaskMode =
            TreeImportedWindMaskMode.BoundsHeightFallback;

        [SerializeField]
        private TreeReferenceDebugMode debugMode =
            TreeReferenceDebugMode.FinalRendering;

        [SerializeField]
        [Range(0f, 1f)]
        private float foliageAlphaCutoff = 0.5f;

        [SerializeField]
        private bool foliageAlphaShadowCasting = true;

        [SerializeField]
        private TreeFoliageDebugMode foliageDebugMode =
            TreeFoliageDebugMode.FinalRendering;

        [SerializeField]
        [Range(0f, 0.5f)]
        private float foliageCanopyDepthStrength = 0.16f;

        [SerializeField]
        [Range(0.05f, 4f)]
        private float foliageCanopyDepthPower = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float foliageOrientationContrast = 0.55f;

        [SerializeField]
        [Range(0f, 1f)]
        private float foliageOrientationReadability = 0.35f;

        [SerializeField]
        [Range(0f, 0.6f)]
        private float foliageUndersideDarkening = 0.14f;

        [SerializeField]
        [Range(0f, 0.3f)]
        private float foliageClusterVariationStrength = 0.06f;

        [SerializeField]
        [Range(0.25f, 4f)]
        private float foliageClusterVariationScale = 1.35f;

        [SerializeField]
        [Range(0f, 1f)]
        private float foliageDiffuseWrap = 0.45f;

        [SerializeField]
        [Range(0f, 1f)]
        private float foliageShadowReceiveStrength = 0.65f;

        [SerializeField]
        [Range(0f, 1f)]
        private float foliageShadowFloor = 0.38f;

        [Header("Curated Recipe Gallery")]
        [SerializeField]
        [Tooltip("Standalone recipe catalog used by all procedural comparison-slot spawners.")]
        private TreeRecipeCatalog recipeCatalog;

        [SerializeField]
        [Tooltip("Stable gallery-level seed. Every slot derives its own seed from this value and its stable family/index identity.")]
        private int curatedGallerySeed = 7319;

        [Header("Generated Mesh Storage / Legacy Compatibility")]
        [SerializeField]
        [Tooltip("Temporary managed asset container for generated bark meshes and legacy comparison evidence. It does not provide behavioral values to recipe-only generation.")]
        private TreeGenerationLibrary generationLibrary;

        [SerializeField]
        private bool showGeneratedStructuralPreviews = true;

        [SerializeField]
        private TreeStructuralPreviewScope generatedPreviewScope =
            TreeStructuralPreviewScope.SelectedTree;

        [SerializeField]
        private bool showGeneratedTrunk = true;

        [SerializeField]
        private bool showGeneratedPrimaryBranches = true;

        [SerializeField]
        private bool showGeneratedHigherOrderBranches = true;

        [SerializeField]
        private bool showGeneratedAttachmentPoints;

        [SerializeField]
        private bool showGeneratedBounds = true;

        [SerializeField]
        private bool showGeneratedTransportedFrames;

        [SerializeField]
        [HideInInspector]
        private bool sourceFolderAvailable;

        [SerializeField]
        [HideInInspector]
        private bool lastSourceAuditPassed;

        [SerializeField]
        [HideInInspector]
        private int sourceAuditRevision;

        [SerializeField]
        [HideInInspector]
        private int lastAuditedModelCount;

        [SerializeField]
        [HideInInspector]
        private int lastAuditedTextureCount;

        [SerializeField]
        [HideInInspector]
        private string lastSourceAuditTimestamp = string.Empty;

        [SerializeField]
        [HideInInspector]
        [TextArea(8, 40)]
        private string lastSourceAuditReport = string.Empty;

        [SerializeField]
        [HideInInspector]
        private bool lastVerticalSliceBuildPassed;

        [SerializeField]
        [HideInInspector]
        private int verticalSliceRevision;

        [SerializeField]
        [HideInInspector]
        private int lastVerticalSliceSpecimenCount;

        [SerializeField]
        [HideInInspector]
        private string lastVerticalSliceTimestamp = string.Empty;

        [SerializeField]
        [HideInInspector]
        [TextArea(8, 40)]
        private string lastVerticalSliceReport = string.Empty;


        [SerializeField]
        [HideInInspector]
        private bool lastCompleteGalleryBuildPassed;

        [SerializeField]
        [HideInInspector]
        private int completeGalleryRevision;

        [SerializeField]
        [HideInInspector]
        private int lastCompleteGallerySpecimenCount;

        [SerializeField]
        [HideInInspector]
        private string lastCompleteGalleryTimestamp = string.Empty;

        [SerializeField]
        [HideInInspector]
        [TextArea(8, 40)]
        private string lastCompleteGalleryReport = string.Empty;

        [SerializeField]
        [HideInInspector]
        private bool lastUnifiedGenerationPassed;

        [SerializeField]
        [HideInInspector]
        private int unifiedGenerationRevision;

        [SerializeField]
        [HideInInspector]
        private int lastGeneratedTreeCount;

        [SerializeField]
        [HideInInspector]
        private string lastUnifiedGenerationTimestamp = string.Empty;

        [SerializeField]
        [HideInInspector]
        [TextArea(8, 40)]
        private string lastUnifiedGenerationReport = string.Empty;

        public GeneratedGround ReferenceGround => referenceGround;
        public float SourceScale => sourceScale;
        public bool AlignToGround => alignToGround;
        public float FamilyRowSpacing => familyRowSpacing;
        public float PairColumnSpacing => pairColumnSpacing;
        public float ComparisonPairOffset => comparisonPairOffset;
        public float CompleteGalleryLeftClearance =>
            completeGalleryLeftClearance;
        public float CompleteGalleryRowGap => completeGalleryRowGap;
        public float CompleteGalleryFamilyGap => completeGalleryFamilyGap;
        public float CompleteGalleryPadMargin => completeGalleryPadMargin;
        public float CompleteGalleryPadThickness =>
            completeGalleryPadThickness;
        public Color CompleteGalleryPadColor => completeGalleryPadColor;
        public bool WindEnabled => windEnabled;
        public TreeImportedWindMaskMode ImportedWindMaskMode =>
            importedWindMaskMode;
        public TreeReferenceDebugMode DebugMode => debugMode;
        public float FoliageAlphaCutoff => foliageAlphaCutoff;
        public bool FoliageShadowCasting => foliageAlphaShadowCasting;
        public TreeFoliageDebugMode FoliageDebugMode => foliageDebugMode;
        public float FoliageCanopyDepthStrength =>
            foliageCanopyDepthStrength;
        public float FoliageCanopyDepthPower => foliageCanopyDepthPower;
        public float FoliageOrientationContrast =>
            foliageOrientationContrast;
        public float FoliageOrientationReadability =>
            foliageOrientationReadability;
        public float FoliageUndersideDarkening =>
            foliageUndersideDarkening;
        public float FoliageClusterVariationStrength =>
            foliageClusterVariationStrength;
        public float FoliageClusterVariationScale =>
            foliageClusterVariationScale;
        public float FoliageDiffuseWrap => foliageDiffuseWrap;
        public float FoliageShadowReceiveStrength =>
            foliageShadowReceiveStrength;
        public float FoliageShadowFloor => foliageShadowFloor;
        public TreeRecipeCatalog RecipeCatalog => recipeCatalog;
        public int CuratedGallerySeed => curatedGallerySeed;
        public TreeGenerationLibrary GenerationLibrary => generationLibrary;
        public bool ShowGeneratedStructuralPreviews =>
            showGeneratedStructuralPreviews;
        public TreeStructuralPreviewScope GeneratedPreviewScope =>
            generatedPreviewScope;
        public bool ShowGeneratedTrunk => showGeneratedTrunk;
        public bool ShowGeneratedPrimaryBranches =>
            showGeneratedPrimaryBranches;
        public bool ShowGeneratedHigherOrderBranches =>
            showGeneratedHigherOrderBranches;
        public bool ShowGeneratedAttachmentPoints =>
            showGeneratedAttachmentPoints;
        public bool ShowGeneratedBounds => showGeneratedBounds;
        public bool ShowGeneratedTransportedFrames =>
            showGeneratedTransportedFrames;
        public bool SourceFolderAvailable => sourceFolderAvailable;
        public bool LastSourceAuditPassed => lastSourceAuditPassed;
        public int SourceAuditRevision => sourceAuditRevision;
        public int LastAuditedModelCount => lastAuditedModelCount;
        public int LastAuditedTextureCount => lastAuditedTextureCount;
        public string LastSourceAuditTimestamp => lastSourceAuditTimestamp;
        public string LastSourceAuditReport => lastSourceAuditReport;
        public bool HasSourceAuditReport =>
            !string.IsNullOrEmpty(lastSourceAuditReport);
        public bool LastVerticalSliceBuildPassed =>
            lastVerticalSliceBuildPassed;
        public int VerticalSliceRevision => verticalSliceRevision;
        public int LastVerticalSliceSpecimenCount =>
            lastVerticalSliceSpecimenCount;
        public string LastVerticalSliceTimestamp =>
            lastVerticalSliceTimestamp;
        public string LastVerticalSliceReport => lastVerticalSliceReport;
        public bool HasVerticalSliceReport =>
            !string.IsNullOrEmpty(lastVerticalSliceReport);

        public bool LastCompleteGalleryBuildPassed =>
            lastCompleteGalleryBuildPassed;
        public int CompleteGalleryRevision => completeGalleryRevision;
        public int LastCompleteGallerySpecimenCount =>
            lastCompleteGallerySpecimenCount;
        public string LastCompleteGalleryTimestamp =>
            lastCompleteGalleryTimestamp;
        public string LastCompleteGalleryReport =>
            lastCompleteGalleryReport;
        public bool HasCompleteGalleryReport =>
            !string.IsNullOrEmpty(lastCompleteGalleryReport);
        public bool LastUnifiedGenerationPassed =>
            lastUnifiedGenerationPassed;
        public int UnifiedGenerationRevision => unifiedGenerationRevision;
        public int LastGeneratedTreeCount => lastGeneratedTreeCount;
        public string LastUnifiedGenerationTimestamp =>
            lastUnifiedGenerationTimestamp;
        public string LastUnifiedGenerationReport =>
            lastUnifiedGenerationReport;
        public bool HasUnifiedGenerationReport =>
            !string.IsNullOrEmpty(lastUnifiedGenerationReport);

        private void OnValidate()
        {
            sourceScale = Mathf.Max(0.01f, sourceScale);
            familyRowSpacing = Mathf.Max(1f, familyRowSpacing);
            pairColumnSpacing = Mathf.Max(1f, pairColumnSpacing);
            comparisonPairOffset = Mathf.Max(0.1f, comparisonPairOffset);
            completeGalleryLeftClearance = Mathf.Max(
                0.5f,
                completeGalleryLeftClearance);
            completeGalleryRowGap = Mathf.Max(
                0.25f,
                completeGalleryRowGap);
            completeGalleryFamilyGap = Mathf.Max(
                0.5f,
                completeGalleryFamilyGap);
            completeGalleryPadMargin = Mathf.Max(
                0.25f,
                completeGalleryPadMargin);
            completeGalleryPadThickness = Mathf.Max(
                0.01f,
                completeGalleryPadThickness);
            foliageAlphaCutoff = Mathf.Clamp01(foliageAlphaCutoff);
            foliageCanopyDepthStrength = Mathf.Clamp(
                foliageCanopyDepthStrength,
                0f,
                0.5f);
            foliageCanopyDepthPower = Mathf.Clamp(
                foliageCanopyDepthPower,
                0.05f,
                4f);
            foliageOrientationContrast = Mathf.Clamp01(
                foliageOrientationContrast);
            foliageOrientationReadability = Mathf.Clamp01(
                foliageOrientationReadability);
            foliageUndersideDarkening = Mathf.Clamp(
                foliageUndersideDarkening,
                0f,
                0.6f);
            foliageClusterVariationStrength = Mathf.Clamp(
                foliageClusterVariationStrength,
                0f,
                0.3f);
            foliageClusterVariationScale = Mathf.Clamp(
                foliageClusterVariationScale,
                0.25f,
                4f);
            foliageDiffuseWrap = Mathf.Clamp01(foliageDiffuseWrap);
            foliageShadowReceiveStrength = Mathf.Clamp01(
                foliageShadowReceiveStrength);
            foliageShadowFloor = Mathf.Clamp01(foliageShadowFloor);
            if (curatedGallerySeed == int.MinValue)
            {
                curatedGallerySeed = 0;
            }
        }

        public void SetReferenceGround(GeneratedGround ground)
        {
            referenceGround = ground;
        }

        public void SetRecipeCatalog(TreeRecipeCatalog catalog)
        {
            recipeCatalog = catalog;
        }

        public void SetGenerationLibrary(TreeGenerationLibrary library)
        {
            generationLibrary = library;
        }

        public void ApplyGeneratedPreviewSettings()
        {
            ProceduralTreeInstance[] instances =
                GetComponentsInChildren<ProceduralTreeInstance>(true);
            for (int index = 0; index < instances.Length; index++)
            {
                if (instances[index] != null)
                {
                    instances[index].SetPreviewSettings(
                        showGeneratedStructuralPreviews,
                        generatedPreviewScope,
                        showGeneratedTrunk,
                        showGeneratedPrimaryBranches,
                        showGeneratedHigherOrderBranches,
                        showGeneratedAttachmentPoints,
                        showGeneratedBounds,
                        showGeneratedTransportedFrames);
                }
            }
        }

        public void RecordSourceAudit(
            bool passed,
            bool folderAvailable,
            int modelCount,
            int textureCount,
            string timestamp,
            string report)
        {
            lastSourceAuditPassed = passed;
            sourceFolderAvailable = folderAvailable;
            lastAuditedModelCount = Mathf.Max(0, modelCount);
            lastAuditedTextureCount = Mathf.Max(0, textureCount);
            lastSourceAuditTimestamp = timestamp ?? string.Empty;
            lastSourceAuditReport = report ?? string.Empty;
            sourceAuditRevision++;
        }

        public void RecordVerticalSliceBuild(
            bool passed,
            int specimenCount,
            string timestamp,
            string report)
        {
            lastVerticalSliceBuildPassed = passed;
            lastVerticalSliceSpecimenCount = Mathf.Max(0, specimenCount);
            lastVerticalSliceTimestamp = timestamp ?? string.Empty;
            lastVerticalSliceReport = report ?? string.Empty;
            verticalSliceRevision++;
        }

        public void RecordCompleteGalleryBuild(
            bool passed,
            int specimenCount,
            string timestamp,
            string report)
        {
            lastCompleteGalleryBuildPassed = passed;
            lastCompleteGallerySpecimenCount = Mathf.Max(0, specimenCount);
            lastCompleteGalleryTimestamp = timestamp ?? string.Empty;
            lastCompleteGalleryReport = report ?? string.Empty;
            completeGalleryRevision++;
        }

        public void RecordUnifiedGenerationBuild(
            bool passed,
            int generatedTreeCount,
            string timestamp,
            string report)
        {
            lastUnifiedGenerationPassed = passed;
            lastGeneratedTreeCount = Mathf.Max(0, generatedTreeCount);
            lastUnifiedGenerationTimestamp = timestamp ?? string.Empty;
            lastUnifiedGenerationReport = report ?? string.Empty;
            unifiedGenerationRevision++;
        }
    }
}
