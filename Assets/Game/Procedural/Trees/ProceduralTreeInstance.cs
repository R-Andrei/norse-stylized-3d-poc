using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Trees/Procedural Tree Instance")]
    public sealed class ProceduralTreeInstance : MonoBehaviour
    {
        private static bool editorPreviewSelectionValid;
        private static EntityId editorPreviewSelectionEntityId;
        private static TreeFamily editorPreviewSelectionFamily;

        [Header("Library Binding")]
        [SerializeField]
        private TreeGenerationLibrary library;

        [SerializeField]
        private TreeFamily family;

        [SerializeField, Range(1, 5)]
        private int sourceVariantIndex = 1;

        [SerializeField]
        private TreeGenerationRecipe recipe;

        [SerializeField]
        private int masterSeed = 7319;

        [SerializeField]
        private TreeGenerationOverrides instanceOverrides =
            new TreeGenerationOverrides();

        [Header("Recipe-Only Exact Controls")]
        [SerializeField]
        private TreeResolvedControls exactControls =
            new TreeResolvedControls();

        [SerializeField, HideInInspector]
        private string exactControlsSourceRecipeIdentity = string.Empty;

        [Header("Structural Preview")]
        [SerializeField]
        private bool showStructuralPreview = true;

        [SerializeField]
        private TreeStructuralPreviewScope previewScope =
            TreeStructuralPreviewScope.SelectedTree;

        [SerializeField]
        private bool showTrunk = true;

        [SerializeField]
        private bool showPrimaryBranches = true;

        [SerializeField]
        private bool showHigherOrderBranches = true;

        [SerializeField]
        private bool showAttachmentPoints;

        [SerializeField]
        private bool showBounds = true;

        [SerializeField]
        private bool showFramesWhenSelected;

        [SerializeField, Range(0.01f, 0.5f)]
        private float frameDisplayScale = 0.12f;

        [Header("Last Generated Output")]
        [SerializeField, HideInInspector]
        private TreeDefinition generatedDefinition;

        [SerializeField, HideInInspector]
        private bool lastGenerationPassed;

        [SerializeField, HideInInspector]
        private string lastGenerationTimestamp = string.Empty;

        [SerializeField, TextArea(6, 30)]
        private string lastGenerationReport = string.Empty;

        [SerializeField, HideInInspector]
        private int generationRevision;

        [Header("Generated Bark Mesh")]
        [SerializeField, HideInInspector]
        private Mesh generatedBarkMesh;

        [SerializeField, HideInInspector]
        private GameObject generatedBarkObject;

        [SerializeField, HideInInspector]
        private bool lastBarkMeshPassed;

        [SerializeField, HideInInspector]
        private int generatedBarkVertexCount;

        [SerializeField, HideInInspector]
        private int generatedBarkTriangleCount;

        [SerializeField, HideInInspector]
        private string generatedBarkFingerprint = string.Empty;

        [SerializeField, TextArea(3, 12)]
        private string lastBarkMeshReport = string.Empty;

        public TreeGenerationLibrary Library => library;
        public TreeFamily Family => family;
        public int SourceVariantIndex => sourceVariantIndex;
        public TreeGenerationRecipe Recipe => recipe;
        public int MasterSeed => masterSeed;
        public TreeGenerationOverrides InstanceOverrides => instanceOverrides;
        public TreeResolvedControls ExactControls => exactControls;
        public string ExactControlsSourceRecipeIdentity =>
            exactControlsSourceRecipeIdentity;
        public bool HasExactControls =>
            exactControls != null && exactControls.IsInitialized;
        public bool UsesRecipeOnlyGeneration =>
            recipe != null &&
            recipe.FamilyProfile == null &&
            HasExactControls;
        public TreeDefinition GeneratedDefinition => generatedDefinition;
        public bool HasGeneratedDefinition =>
            generatedDefinition != null && generatedDefinition.IsValid;
        public bool LastGenerationPassed => lastGenerationPassed;
        public string LastGenerationTimestamp => lastGenerationTimestamp;
        public string LastGenerationReport => lastGenerationReport;
        public bool HasGenerationReport =>
            !string.IsNullOrEmpty(lastGenerationReport);
        public int GenerationRevision => generationRevision;
        public Mesh GeneratedBarkMesh => generatedBarkMesh;
        public GameObject GeneratedBarkObject => generatedBarkObject;
        public bool HasGeneratedBarkMesh =>
            lastBarkMeshPassed &&
            generatedBarkMesh != null &&
            generatedBarkObject != null;
        public bool LastBarkMeshPassed => lastBarkMeshPassed;
        public int GeneratedBarkVertexCount => generatedBarkVertexCount;
        public int GeneratedBarkTriangleCount => generatedBarkTriangleCount;
        public string GeneratedBarkFingerprint => generatedBarkFingerprint;
        public string LastBarkMeshReport => lastBarkMeshReport;
        public bool ShowStructuralPreview => showStructuralPreview;
        public TreeStructuralPreviewScope PreviewScope => previewScope;
        public bool ShowTrunk => showTrunk;
        public bool ShowPrimaryBranches => showPrimaryBranches;
        public bool ShowHigherOrderBranches => showHigherOrderBranches;
        public bool ShowAttachmentPoints => showAttachmentPoints;
        public bool ShowBounds => showBounds;
        public bool ShowFramesWhenSelected => showFramesWhenSelected;
        public float FrameDisplayScale => Mathf.Clamp(frameDisplayScale, 0.01f, 0.5f);

        public string StableSlotIdentity =>
            TreeGenerationLibraryVariant.BuildStableKey(
                family,
                sourceVariantIndex);

        public void ConfigureManagedBinding(
            TreeGenerationLibrary generationLibrary,
            TreeGenerationLibraryVariant variant)
        {
            if (variant == null)
            {
                throw new ArgumentNullException(nameof(variant));
            }

            library = generationLibrary;
            family = variant.Family;
            sourceVariantIndex = variant.VariantIndex;
            recipe = variant.Recipe;
            if (recipe != null && generationRevision == 0)
            {
                masterSeed = recipe.MasterSeed;
            }

            instanceOverrides ??= new TreeGenerationOverrides();
            exactControls ??= new TreeResolvedControls();
            if (recipe != null &&
                exactControlsSourceRecipeIdentity != recipe.StableIdentity)
            {
                SampleExactControlsFromRecipe();
            }
        }

        public void ConfigureRecipe(
            TreeGenerationLibrary generationLibrary,
            TreeGenerationLibraryVariant variant,
            bool adoptRecipeSeed)
        {
            ConfigureManagedBinding(generationLibrary, variant);
            if (adoptRecipeSeed && recipe != null)
            {
                masterSeed = recipe.MasterSeed;
            }

            SampleExactControlsFromRecipe();
        }

        public void ConfigureStandaloneRecipe(
            TreeGenerationRecipe standaloneRecipe,
            bool adoptRecipeSeed)
        {
            ConfigureStandaloneRecipe(
                standaloneRecipe,
                adoptRecipeSeed,
                family);
        }

        public void ConfigureStandaloneRecipe(
            TreeGenerationRecipe standaloneRecipe,
            bool adoptRecipeSeed,
            TreeFamily referenceGrouping)
        {
            library = null;
            family = referenceGrouping;
            recipe = standaloneRecipe;
            instanceOverrides = new TreeGenerationOverrides();
            if (adoptRecipeSeed && recipe != null)
            {
                masterSeed = recipe.MasterSeed;
            }

            SampleExactControlsFromRecipe();
        }

        public void ConfigureRecipeOnlySpawn(
            TreeGenerationRecipe standaloneRecipe,
            int seed,
            TreeFamily referenceGrouping,
            int referenceVariantIndex = 1,
            TreeGenerationLibrary meshStorageLibrary = null)
        {
            // Recipe-only generation never reads behavioral values from this
            // library. The optional reference exists solely so generated bark
            // meshes remain persistent subassets across scene/domain reloads.
            library = meshStorageLibrary;
            family = referenceGrouping;
            sourceVariantIndex = Mathf.Clamp(referenceVariantIndex, 1, 5);
            recipe = standaloneRecipe;
            masterSeed = seed == int.MinValue ? 0 : Mathf.Abs(seed);
            instanceOverrides = new TreeGenerationOverrides();
            SampleExactControlsFromRecipe();
        }

        public bool SampleExactControlsFromRecipe()
        {
            if (recipe == null)
            {
                return false;
            }

            recipe.EnsureRecipeOnlyFoundation();
            exactControls ??= new TreeResolvedControls();
            exactControls.ResolveFrom(recipe.ControlRanges, masterSeed);
            exactControlsSourceRecipeIdentity = recipe.StableIdentity;
            return true;
        }

        public void ValidateExactControls()
        {
            exactControls ??= new TreeResolvedControls();
            if (recipe != null)
            {
                recipe.EnsureRecipeOnlyFoundation();
                if (exactControlsSourceRecipeIdentity != recipe.StableIdentity)
                {
                    exactControls.ResolveFrom(
                        recipe.ControlRanges,
                        masterSeed);
                    exactControlsSourceRecipeIdentity = recipe.StableIdentity;
                }
                else
                {
                    exactControls.EnsureInitialized(
                        recipe.ControlRanges,
                        masterSeed);
                }
            }
            else if (exactControls.IsInitialized)
            {
                exactControls.ValidateAndClamp();
            }
            else
            {
                exactControls.ResolveFrom(
                    TreeRecipeControlRanges.CreateStarterDefaults(),
                    masterSeed);
                exactControlsSourceRecipeIdentity = string.Empty;
            }
        }

        public TreeGenerationResult GenerateStructure()
        {
            TreeGenerationResult result;
            if (UsesRecipeOnlyGeneration)
            {
                exactControls.ValidateAndClamp();
                result = TreeGenerator.Generate(
                    exactControls,
                    masterSeed,
                    exactControlsSourceRecipeIdentity,
                    family);
            }
            else
            {
                instanceOverrides ??= new TreeGenerationOverrides();
                instanceOverrides.UpgradeTreeGen2BControls();
                result = TreeGenerator.Generate(
                    recipe,
                    instanceOverrides,
                    masterSeed);
            }

            RecordGeneration(result);
            return result;
        }

        public TreeGenerationResult RegenerateFromExactControls()
        {
            exactControls ??= new TreeResolvedControls();
            exactControls.ValidateAndClamp();
            TreeGenerationResult result = TreeGenerator.Generate(
                exactControls,
                masterSeed,
                exactControlsSourceRecipeIdentity,
                family);
            RecordGeneration(result);
            return result;
        }

        public void RecordGeneration(TreeGenerationResult result)
        {
            generatedDefinition = result != null ? result.Definition : null;
            lastGenerationPassed = result != null && result.Passed;
            lastGenerationTimestamp = result != null
                ? result.Timestamp ?? string.Empty
                : string.Empty;
            lastGenerationReport = result != null
                ? result.Report ?? string.Empty
                : string.Empty;
            generationRevision++;
            lastBarkMeshPassed = false;
            generatedBarkFingerprint = string.Empty;
            lastBarkMeshReport =
                "Structure changed; generated bark mesh requires rebuilding.";
        }

        public void RecordGeneratedBarkMesh(
            Mesh mesh,
            GameObject barkObject,
            TreeBarkMeshBuildResult result,
            string report)
        {
            generatedBarkMesh = mesh;
            generatedBarkObject = barkObject;
            lastBarkMeshPassed = result != null && result.Passed;
            generatedBarkVertexCount = result != null
                ? result.VertexCount
                : 0;
            generatedBarkTriangleCount = result != null
                ? result.TriangleCount
                : 0;
            generatedBarkFingerprint = result != null
                ? result.GeometryFingerprint ?? string.Empty
                : string.Empty;
            lastBarkMeshReport = report ?? string.Empty;
        }

        public void ClearGeneratedBarkOutput()
        {
            generatedBarkMesh = null;
            generatedBarkObject = null;
            lastBarkMeshPassed = false;
            generatedBarkVertexCount = 0;
            generatedBarkTriangleCount = 0;
            generatedBarkFingerprint = string.Empty;
            lastBarkMeshReport = string.Empty;
        }

        public void SetMasterSeed(int seed)
        {
            masterSeed = seed == int.MinValue ? 0 : Mathf.Abs(seed);
        }

        public void SetPreviewSettings(
            bool visible,
            TreeStructuralPreviewScope scope,
            bool trunkVisible,
            bool primaryVisible,
            bool higherOrderVisible,
            bool attachmentsVisible,
            bool boundsVisible,
            bool framesVisible)
        {
            showStructuralPreview = visible;
            previewScope = scope;
            showTrunk = trunkVisible;
            showPrimaryBranches = primaryVisible;
            showHigherOrderBranches = higherOrderVisible;
            showAttachmentPoints = attachmentsVisible;
            showBounds = boundsVisible;
            showFramesWhenSelected = framesVisible;
        }

        public void SetPreviewVisibility(bool visible, bool boundsVisible)
        {
            SetPreviewSettings(
                visible,
                previewScope,
                showTrunk,
                showPrimaryBranches,
                showHigherOrderBranches,
                showAttachmentPoints,
                boundsVisible,
                showFramesWhenSelected);
        }

        public void ResetInstanceOverrides()
        {
            instanceOverrides = new TreeGenerationOverrides();
        }

        public void ClearGeneratedOutput()
        {
            generatedDefinition = null;
            lastGenerationPassed = false;
            lastGenerationTimestamp = string.Empty;
            lastGenerationReport = string.Empty;
            ClearGeneratedBarkOutput();
        }

        private void OnValidate()
        {
            sourceVariantIndex = Mathf.Clamp(sourceVariantIndex, 1, 5);
            frameDisplayScale = Mathf.Clamp(frameDisplayScale, 0.01f, 0.5f);
            instanceOverrides ??= new TreeGenerationOverrides();
            exactControls ??= new TreeResolvedControls();
            ValidateExactControls();
        }

        private void OnDrawGizmos()
        {
            if (!showStructuralPreview ||
                !HasGeneratedDefinition ||
                !ShouldDrawForPreviewScope())
            {
                return;
            }

            DrawDefinition(false);
        }

        private void OnDrawGizmosSelected()
        {
            if (!showStructuralPreview ||
                !showFramesWhenSelected ||
                !HasGeneratedDefinition)
            {
                return;
            }

            DrawDefinition(true);
        }

        public static void SetEditorPreviewSelection(
            ProceduralTreeInstance selectedInstance)
        {
            editorPreviewSelectionValid = selectedInstance != null;
            editorPreviewSelectionEntityId = selectedInstance != null
                ? selectedInstance.GetEntityId()
                : default;
            editorPreviewSelectionFamily = selectedInstance != null
                ? selectedInstance.Family
                : TreeFamily.Common;
        }

        private bool ShouldDrawForPreviewScope()
        {
            if (previewScope == TreeStructuralPreviewScope.AllTrees)
            {
                return true;
            }

            if (!editorPreviewSelectionValid)
            {
                return false;
            }

            if (previewScope == TreeStructuralPreviewScope.SelectedFamily)
            {
                return editorPreviewSelectionFamily == family;
            }

            return editorPreviewSelectionEntityId.Equals(GetEntityId());
        }

        private void DrawDefinition(bool framesOnly)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;
            Gizmos.matrix = transform.localToWorldMatrix;
            try
            {
                IReadOnlyList<TreeBranchDefinition> branches =
                    generatedDefinition.Branches;
                for (int branchIndex = 0;
                     branchIndex < branches.Count;
                     branchIndex++)
                {
                    TreeBranchDefinition branch = branches[branchIndex];
                    IReadOnlyList<TreeCurveSample> samples = branch.Samples;
                    if (samples == null || samples.Count < 2)
                    {
                        continue;
                    }

                    if (!framesOnly)
                    {
                        if (!ShouldDrawBranchOrder(branch.BranchOrder))
                        {
                            continue;
                        }

                        Gizmos.color = ResolveBranchColor(branch);
                        for (int sampleIndex = 1;
                             sampleIndex < samples.Count;
                             sampleIndex++)
                        {
                            Gizmos.DrawLine(
                                samples[sampleIndex - 1].Position,
                                samples[sampleIndex].Position);
                        }

                        if (showAttachmentPoints &&
                            branch.ParentBranchIndex >= 0)
                        {
                            Gizmos.DrawSphere(
                                samples[0].Position,
                                Mathf.Max(0.025f, branch.BaseRadius * 0.18f));
                        }
                    }
                    else
                    {
                        int stride = Mathf.Max(1, samples.Count / 6);
                        for (int sampleIndex = 0;
                             sampleIndex < samples.Count;
                             sampleIndex += stride)
                        {
                            TreeCurveSample sample = samples[sampleIndex];
                            float scale = frameDisplayScale;
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(
                                sample.Position,
                                sample.Position + sample.Normal * scale);
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(
                                sample.Position,
                                sample.Position + sample.Binormal * scale);
                        }
                    }
                }

                if (!framesOnly && showBounds)
                {
                    Gizmos.color = new Color(1f, 1f, 1f, 0.65f);
                    Gizmos.DrawWireCube(
                        generatedDefinition.LocalBounds.center,
                        generatedDefinition.LocalBounds.size);
                }
            }
            finally
            {
                Gizmos.matrix = previousMatrix;
                Gizmos.color = previousColor;
            }
        }

        private bool ShouldDrawBranchOrder(int branchOrder)
        {
            if (branchOrder == 0)
            {
                return showTrunk;
            }

            if (branchOrder == 1)
            {
                return showPrimaryBranches;
            }

            return showHigherOrderBranches;
        }

        private static Color ResolveBranchColor(TreeBranchDefinition branch)
        {
            if (branch.IsBroken)
            {
                return new Color(1f, 0.32f, 0.18f, 1f);
            }

            if (branch.IsDead)
            {
                return new Color(0.45f, 0.42f, 0.38f, 1f);
            }

            switch (branch.BranchOrder)
            {
                case 0:
                    return new Color(0.92f, 0.72f, 0.26f, 1f);
                case 1:
                    return new Color(0.32f, 0.9f, 0.38f, 1f);
                case 2:
                    return new Color(0.25f, 0.72f, 1f, 1f);
                default:
                    return new Color(0.75f, 0.48f, 1f, 1f);
            }
        }
    }
}
