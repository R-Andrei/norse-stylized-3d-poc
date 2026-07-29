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
        public const int CurrentRecipeVersion = 3;
        public const int CurrentDeterministicSeedVersion = 2;
        public const int CurrentRecipeOnlyFoundationVersion = 1;

        [Header("Identity")]
        [SerializeField]
        private string stableIdentity = "tree-generation-recipe";

        [SerializeField]
        private int recipeVersion = CurrentRecipeVersion;

        [Header("Recipe-Only Foundation")]
        [SerializeField]
        private int recipeOnlyFoundationVersion =
            CurrentRecipeOnlyFoundationVersion;

        [SerializeField]
        private string recipeDisplayName = string.Empty;

        [SerializeField, TextArea(2, 8)]
        private string recipeDescription = string.Empty;

        [SerializeField]
        private List<string> recipeTags = new List<string>();

        [SerializeField]
        private Material barkMaterial;

        [SerializeField]
        private TreeRecipeControlRanges controlRanges =
            new TreeRecipeControlRanges();

        [Header("Legacy Compatibility — Removed From Live Resolution In TREE-CONTROLS.3")]
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
        public int RecipeOnlyFoundationVersion => recipeOnlyFoundationVersion;
        public string RecipeDisplayName => string.IsNullOrWhiteSpace(recipeDisplayName)
            ? name
            : recipeDisplayName;
        public string RecipeDescription => recipeDescription ?? string.Empty;
        public IReadOnlyList<string> RecipeTags =>
            recipeTags ?? (IReadOnlyList<string>)Array.Empty<string>();
        public Material BarkMaterial => barkMaterial;
        public TreeRecipeControlRanges ControlRanges => controlRanges;
        public TreeFamilyProfile FamilyProfile => familyProfile;
        public TreeReferenceCalibrationPreset ReferenceCalibration => referenceCalibration;
        public TreeMaterialPalette PaletteOverride => paletteOverride;
        public TreeAgeClass AgeClass => ageClass;
        public int MasterSeed => masterSeed;
        public TreeGenerationOverrides Overrides => overrides;
        public IReadOnlyList<TreeSeedLock> SeedLocks => seedLocks;

        public void EnsureRecipeOnlyFoundation()
        {
            recipeOnlyFoundationVersion = Mathf.Max(
                1,
                recipeOnlyFoundationVersion);
            recipeTags ??= new List<string>();
            controlRanges ??= TreeRecipeControlRanges.CreateStarterDefaults();
            controlRanges.EnsureCurrentDefaults();
            if (string.IsNullOrWhiteSpace(recipeDisplayName))
            {
                recipeDisplayName = name;
            }

            if (string.IsNullOrWhiteSpace(stableIdentity) ||
                stableIdentity == "tree-generation-recipe" ||
                stableIdentity == "tree-recipe")
            {
                RegenerateStableIdentity();
            }

            recipeOnlyFoundationVersion =
                CurrentRecipeOnlyFoundationVersion;
        }

        public void InitializeRecipeOnlyFoundation(
            string displayName,
            string identity = null)
        {
            recipeDisplayName = string.IsNullOrWhiteSpace(displayName)
                ? name
                : displayName.Trim();
            stableIdentity = string.IsNullOrWhiteSpace(identity)
                ? BuildNewStableIdentity()
                : identity.Trim();
            recipeDescription = string.Empty;
            recipeTags = new List<string>();
            controlRanges = TreeRecipeControlRanges.CreateStarterDefaults();
            recipeOnlyFoundationVersion =
                CurrentRecipeOnlyFoundationVersion;
        }

        public void ConfigureCuratedDefinition(
            TreeCuratedRecipeDefinition definition,
            Material material)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            stableIdentity = definition.StableIdentity;
            recipeDisplayName = definition.DisplayName;
            recipeDescription = definition.Description;
            recipeTags = new List<string>(definition.Tags);
            barkMaterial = material;
            controlRanges = definition.CreateControlRanges();

            familyProfile = null;
            referenceCalibration = null;
            paletteOverride = null;
            ageClass = TreeAgeClass.Mature;
            masterSeed = 7319;
            overrides = new TreeGenerationOverrides();
            seedLocks = new List<TreeSeedLock>();

            recipeVersion = CurrentRecipeVersion;
            recipeOnlyFoundationVersion =
                CurrentRecipeOnlyFoundationVersion;
        }

        public bool IsCuratedRecipeDefinition(
            out TreeCuratedRecipeDefinition definition)
        {
            return TreeCuratedRecipeDefinitions.TryFindByStableIdentity(
                stableIdentity,
                out definition);
        }

        public void SetRecipeDisplayName(string displayName)
        {
            recipeDisplayName = string.IsNullOrWhiteSpace(displayName)
                ? name
                : displayName.Trim();
        }

        public void RegenerateStableIdentity()
        {
            stableIdentity = BuildNewStableIdentity();
        }

        public bool ValidateRecipeOnlyFoundation(List<string> failures)
        {
            if (failures == null)
            {
                return false;
            }

            EnsureRecipeOnlyFoundation();
            if (string.IsNullOrWhiteSpace(stableIdentity))
            {
                failures.Add("Standalone tree recipe stable identity is empty.");
            }

            if (string.IsNullOrWhiteSpace(RecipeDisplayName))
            {
                failures.Add("Standalone tree recipe display name is empty.");
            }

            if (controlRanges == null || !controlRanges.IsInitialized)
            {
                failures.Add("Standalone tree recipe control ranges are uninitialized.");
            }

            return failures.Count == 0;
        }

        private static string BuildNewStableIdentity()
        {
            return "tree-recipe-" + Guid.NewGuid().ToString("N");
        }

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
            EnsureRecipeOnlyFoundation();
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
            EnsureRecipeOnlyFoundation();
        }

        public bool UpgradeManagedDefaults(
            bool neutralComparisonBark,
            bool applyManagedTrunkTwistDefault,
            float managedTrunkTwistDegrees)
        {
            bool changed = false;
            overrides ??= new TreeGenerationOverrides();
            bool legacyTreeGen2BRecipe = recipeVersion < 2;
            bool legacyTreeGen2C1Recipe = recipeVersion < 3;
            if (legacyTreeGen2BRecipe && familyProfile != null)
            {
                changed |= overrides.EnsureLegacyPrimaryAttachmentInterval(
                    familyProfile.PrimaryBranches.AttachmentHeight);
            }
            changed |= overrides.UpgradeTreeGen2BControls();

            if (neutralComparisonBark)
            {
                changed |= overrides.EnsureNeutralComparisonBarkTint();
            }

            if (legacyTreeGen2C1Recipe &&
                applyManagedTrunkTwistDefault)
            {
                changed |= overrides.EnsureManagedTrunkTwistDefault(
                    managedTrunkTwistDegrees);
            }

            if (recipeVersion < CurrentRecipeVersion)
            {
                recipeVersion = CurrentRecipeVersion;
                changed = true;
            }

            return changed;
        }

        public bool ConfigureManagedTrunkTwistDefault(float twistDegrees)
        {
            overrides ??= new TreeGenerationOverrides();
            return overrides.EnsureManagedTrunkTwistDefault(twistDegrees);
        }

        public bool UpgradeManagedTrunkTwistDefault(
            float previousManagedDegrees,
            float currentManagedDegrees)
        {
            overrides ??= new TreeGenerationOverrides();
            return overrides.UpgradeManagedTrunkTwistDefault(
                previousManagedDegrees,
                currentManagedDegrees);
        }

        public bool ConfigureManagedRootButtressDefaults(
            int buttressCount,
            float buttressStrength,
            float buttressHeight,
            float flareScale)
        {
            overrides ??= new TreeGenerationOverrides();
            return overrides.EnsureManagedRootButtressDefaults(
                buttressCount,
                buttressStrength,
                buttressHeight,
                flareScale);
        }

        public bool UpgradeManagedRootButtressDefaults(
            int previousCount,
            int currentCount,
            float previousStrength,
            float currentStrength,
            float currentHeight,
            float previousFlare,
            float currentFlare)
        {
            overrides ??= new TreeGenerationOverrides();
            return overrides.UpgradeManagedRootButtressDefaults(
                previousCount,
                currentCount,
                previousStrength,
                currentStrength,
                currentHeight,
                previousFlare,
                currentFlare);
        }

        public bool ConfigureManagedPathSpiralDefaults(
            float strength,
            float turns,
            float direction)
        {
            overrides ??= new TreeGenerationOverrides();
            return overrides.EnsureManagedPathSpiralDefaults(
                strength,
                turns,
                direction);
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
            EnsureRecipeOnlyFoundation();
        }
    }
}
