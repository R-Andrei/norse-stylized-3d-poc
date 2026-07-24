using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public sealed class TreeGenerationLibraryVariant
    {
        [SerializeField]
        private TreeFamily family;

        [SerializeField, Range(1, 5)]
        private int variantIndex = 1;

        [SerializeField]
        private string displayName = string.Empty;

        [SerializeField]
        private string sourceAssetPath = string.Empty;

        [SerializeField]
        private string sourceAssetGuid = string.Empty;

        [SerializeField]
        private TreeFamilyProfile familyProfile;

        [SerializeField]
        private TreeMaterialPalette palette;

        [SerializeField]
        private TreeReferenceCalibrationPreset calibration;

        [SerializeField]
        private TreeGenerationRecipe recipe;

        public TreeFamily Family => family;
        public int VariantIndex => variantIndex;
        public string DisplayName => displayName;
        public string SourceAssetPath => sourceAssetPath;
        public string SourceAssetGuid => sourceAssetGuid;
        public TreeFamilyProfile FamilyProfile => familyProfile;
        public TreeMaterialPalette Palette => palette;
        public TreeReferenceCalibrationPreset Calibration => calibration;
        public TreeGenerationRecipe Recipe => recipe;

        public string StableKey => BuildStableKey(family, variantIndex);

        public void Configure(
            TreeFamily targetFamily,
            int targetVariantIndex,
            string targetDisplayName,
            string targetSourceAssetPath,
            string targetSourceAssetGuid,
            TreeFamilyProfile profile,
            TreeMaterialPalette materialPalette,
            TreeReferenceCalibrationPreset calibrationPreset,
            TreeGenerationRecipe generationRecipe)
        {
            family = targetFamily;
            variantIndex = Mathf.Clamp(targetVariantIndex, 1, 5);
            displayName = string.IsNullOrWhiteSpace(targetDisplayName)
                ? targetFamily + " " + variantIndex
                : targetDisplayName;
            sourceAssetPath = targetSourceAssetPath ?? string.Empty;
            sourceAssetGuid = targetSourceAssetGuid ?? string.Empty;
            familyProfile = profile;
            palette = materialPalette;
            calibration = calibrationPreset;
            recipe = generationRecipe;
        }

        public static string BuildStableKey(TreeFamily targetFamily, int targetVariantIndex)
        {
            return targetFamily.ToString().ToLowerInvariant() + "-" +
                Mathf.Clamp(targetVariantIndex, 1, 5);
        }
    }

    [CreateAssetMenu(
        fileName = "TreeGenerationLibrary",
        menuName = "PS3D/Trees/Tree Generation Library")]
    public sealed class TreeGenerationLibrary : ScriptableObject
    {
        public const int CurrentLibraryVersion = 3;

        [SerializeField]
        private string stableIdentity = "tree-generation-library";

        [SerializeField]
        private int libraryVersion = CurrentLibraryVersion;

        [SerializeField]
        private List<TreeFamilyProfile> familyProfiles =
            new List<TreeFamilyProfile>();

        [SerializeField]
        private List<TreeMaterialPalette> materialPalettes =
            new List<TreeMaterialPalette>();

        [SerializeField]
        private List<TreeGenerationLibraryVariant> variants =
            new List<TreeGenerationLibraryVariant>();

        public string StableIdentity => stableIdentity;
        public int LibraryVersion => libraryVersion;
        public IReadOnlyList<TreeFamilyProfile> FamilyProfiles => familyProfiles;
        public IReadOnlyList<TreeMaterialPalette> MaterialPalettes => materialPalettes;
        public IReadOnlyList<TreeGenerationLibraryVariant> Variants => variants;
        public int VariantCount => variants != null ? variants.Count : 0;

        public TreeFamilyProfile FindFamilyProfile(TreeFamily family)
        {
            if (familyProfiles == null)
            {
                return null;
            }

            for (int index = 0; index < familyProfiles.Count; index++)
            {
                TreeFamilyProfile profile = familyProfiles[index];
                if (profile != null && profile.Family == family)
                {
                    return profile;
                }
            }

            return null;
        }

        public TreeMaterialPalette FindMaterialPalette(TreeFamily family)
        {
            TreeFamilyProfile profile = FindFamilyProfile(family);
            if (profile != null && profile.DefaultPalette != null)
            {
                return profile.DefaultPalette;
            }

            if (materialPalettes == null)
            {
                return null;
            }

            string expectedIdentity =
                "tree-palette-" + family.ToString().ToLowerInvariant();
            for (int index = 0; index < materialPalettes.Count; index++)
            {
                TreeMaterialPalette palette = materialPalettes[index];
                if (palette != null && palette.StableIdentity == expectedIdentity)
                {
                    return palette;
                }
            }

            return null;
        }

        public TreeGenerationLibraryVariant FindVariant(
            TreeFamily family,
            int variantIndex)
        {
            if (variants == null)
            {
                return null;
            }

            int clampedVariant = Mathf.Clamp(variantIndex, 1, 5);
            for (int index = 0; index < variants.Count; index++)
            {
                TreeGenerationLibraryVariant variant = variants[index];
                if (variant != null &&
                    variant.Family == family &&
                    variant.VariantIndex == clampedVariant)
                {
                    return variant;
                }
            }

            return null;
        }

        public TreeGenerationLibraryVariant FindVariant(
            TreeGenerationRecipe recipe)
        {
            if (recipe == null || variants == null)
            {
                return null;
            }

            for (int index = 0; index < variants.Count; index++)
            {
                TreeGenerationLibraryVariant variant = variants[index];
                if (variant != null && variant.Recipe == recipe)
                {
                    return variant;
                }
            }

            return null;
        }

        public bool ValidateLibrary(List<string> failures)
        {
            if (failures == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(stableIdentity))
            {
                failures.Add("Tree generation library stable identity is empty.");
            }

            if (familyProfiles == null || familyProfiles.Count != 4)
            {
                failures.Add("Tree generation library must contain exactly four family profiles.");
            }

            if (materialPalettes == null || materialPalettes.Count != 4)
            {
                failures.Add("Tree generation library must contain exactly four material palettes.");
            }

            if (variants == null || variants.Count != 20)
            {
                failures.Add("Tree generation library must contain exactly twenty reference-calibrated variants.");
            }

            var seenKeys = new HashSet<string>();
            if (variants != null)
            {
                for (int index = 0; index < variants.Count; index++)
                {
                    TreeGenerationLibraryVariant variant = variants[index];
                    if (variant == null)
                    {
                        failures.Add("Tree generation library contains a null variant entry.");
                        continue;
                    }

                    if (!seenKeys.Add(variant.StableKey))
                    {
                        failures.Add(
                            "Tree generation library contains duplicate variant " +
                            variant.StableKey + ".");
                    }

                    if (variant.FamilyProfile == null ||
                        variant.Palette == null ||
                        variant.Calibration == null ||
                        variant.Recipe == null)
                    {
                        failures.Add(
                            "Tree generation library variant " +
                            variant.StableKey + " has incomplete managed assets.");
                    }
                }
            }

            return failures.Count == 0;
        }

        public void ReplaceManagedContent(
            List<TreeFamilyProfile> profiles,
            List<TreeMaterialPalette> palettes,
            List<TreeGenerationLibraryVariant> libraryVariants)
        {
            familyProfiles = profiles ?? new List<TreeFamilyProfile>();
            materialPalettes = palettes ?? new List<TreeMaterialPalette>();
            variants = libraryVariants ?? new List<TreeGenerationLibraryVariant>();
            libraryVersion = CurrentLibraryVersion;
        }

        private void OnValidate()
        {
            libraryVersion = Mathf.Max(1, libraryVersion);
            familyProfiles ??= new List<TreeFamilyProfile>();
            materialPalettes ??= new List<TreeMaterialPalette>();
            variants ??= new List<TreeGenerationLibraryVariant>();
        }
    }
}
