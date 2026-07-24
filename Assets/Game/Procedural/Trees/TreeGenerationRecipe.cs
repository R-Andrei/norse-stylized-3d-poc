using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [CreateAssetMenu(
        fileName = "TreeGenerationRecipe",
        menuName = "PS3D/Trees/Tree Generation Recipe")]
    public sealed class TreeGenerationRecipe : ScriptableObject
    {
        public const int CurrentRecipeVersion = 2;

        [Header("Identity")]
        [SerializeField]
        private string stableIdentity = "tree-generation-recipe";

        [SerializeField]
        private int recipeVersion = CurrentRecipeVersion;

        [Header("Authoring Layers")]
        [SerializeField]
        private TreeFamilyProfile familyProfile;

        [SerializeField]
        private TreeReferenceCalibrationPreset referenceCalibration;

        [SerializeField]
        private TreeMaterialPalette paletteOverride;

        [SerializeField]
        private TreeAgeClass ageClass = TreeAgeClass.Mature;

        [SerializeField]
        private int masterSeed = 7319;

        [SerializeField]
        private TreeGenerationOverrides overrides = new TreeGenerationOverrides();

        [SerializeField]
        private List<TreeSeedLock> seedLocks = new List<TreeSeedLock>();

        public string StableIdentity => stableIdentity;
        public int RecipeVersion => recipeVersion;
        public TreeFamilyProfile FamilyProfile => familyProfile;
        public TreeReferenceCalibrationPreset ReferenceCalibration => referenceCalibration;
        public TreeMaterialPalette PaletteOverride => paletteOverride;
        public TreeAgeClass AgeClass => ageClass;
        public int MasterSeed => masterSeed;
        public TreeGenerationOverrides Overrides => overrides;
        public IReadOnlyList<TreeSeedLock> SeedLocks => seedLocks;

        public void Initialize(
            TreeFamilyProfile profile,
            TreeMaterialPalette palette,
            string identity)
        {
            Initialize(
                profile,
                palette,
                null,
                identity,
                masterSeed);
        }

        public void Initialize(
            TreeFamilyProfile profile,
            TreeMaterialPalette palette,
            TreeReferenceCalibrationPreset calibration,
            string identity,
            int seed)
        {
            familyProfile = profile;
            paletteOverride = palette;
            referenceCalibration = calibration;
            stableIdentity = string.IsNullOrWhiteSpace(identity)
                ? "tree-recipe"
                : identity;
            masterSeed = seed == int.MinValue ? 0 : Mathf.Abs(seed);
            recipeVersion = CurrentRecipeVersion;
            overrides ??= new TreeGenerationOverrides();
        }

        public void RepairManagedBindings(
            TreeFamilyProfile profile,
            TreeMaterialPalette palette,
            TreeReferenceCalibrationPreset calibration)
        {
            familyProfile = profile;
            paletteOverride = palette;
            referenceCalibration = calibration;
            overrides ??= new TreeGenerationOverrides();
        }

        public bool UpgradeManagedDefaults(bool neutralComparisonBark)
        {
            bool changed = false;
            overrides ??= new TreeGenerationOverrides();
            bool legacyRecipe = recipeVersion < CurrentRecipeVersion;
            if (legacyRecipe && familyProfile != null)
            {
                changed |= overrides.EnsureLegacyPrimaryAttachmentInterval(
                    familyProfile.PrimaryBranches.AttachmentHeight);
            }
            changed |= overrides.UpgradeTreeGen2BControls();

            if (neutralComparisonBark)
            {
                changed |= overrides.EnsureNeutralComparisonBarkTint();
            }

            if (recipeVersion < CurrentRecipeVersion)
            {
                recipeVersion = CurrentRecipeVersion;
                changed = true;
            }

            return changed;
        }

        public bool TryGetLockedSeed(TreeSeedStream stream, out int seed)
        {
            for (int index = 0; index < seedLocks.Count; index++)
            {
                TreeSeedLock candidate = seedLocks[index];
                if (candidate.Stream == stream && candidate.Locked)
                {
                    seed = candidate.Seed;
                    return true;
                }
            }

            seed = 0;
            return false;
        }

        public bool ValidateRecipe(List<string> failures)
        {
            if (failures == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(stableIdentity))
            {
                failures.Add("Generation recipe stable identity is empty.");
            }

            if (familyProfile == null)
            {
                failures.Add("Generation recipe has no family profile.");
                return false;
            }

            familyProfile.ValidateProfile(failures);

            if (referenceCalibration != null)
            {
                referenceCalibration.ValidatePreset(familyProfile, failures);
            }

            TreeMaterialPalette palette = ResolvePalette();
            if (palette != null)
            {
                palette.ValidatePalette(failures);
            }

            var seen = new HashSet<TreeSeedStream>();
            for (int index = 0; index < seedLocks.Count; index++)
            {
                TreeSeedLock candidate = seedLocks[index];
                if (!candidate.Locked)
                {
                    continue;
                }

                if (!seen.Add(candidate.Stream))
                {
                    failures.Add(
                        "Generation recipe contains duplicate locked seed entries for " +
                        candidate.Stream + ".");
                }
            }

            return failures.Count == 0;
        }

        public TreeMaterialPalette ResolvePalette()
        {
            if (paletteOverride != null)
            {
                return paletteOverride;
            }

            if (referenceCalibration != null &&
                referenceCalibration.PaletteOverride != null)
            {
                return referenceCalibration.PaletteOverride;
            }

            return familyProfile != null ? familyProfile.DefaultPalette : null;
        }

        private void OnValidate()
        {
            recipeVersion = Mathf.Max(1, recipeVersion);
            overrides ??= new TreeGenerationOverrides();
            seedLocks ??= new List<TreeSeedLock>();
        }
    }
}
