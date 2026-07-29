using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeGenerationLibraryBuilder
    {
        internal const string LibraryFolderPath =
            "Assets/Game/Demo/Profiles/Trees";
        internal const string LibraryAssetPath =
            LibraryFolderPath + "/TreeGenerationLibrary.asset";

        internal static bool EnsureLibrary(
            TreeReferenceGallery gallery,
            IReadOnlyList<TreeReferenceSpecimen> proceduralSlots,
            StringBuilder report,
            out TreeGenerationLibrary library,
            out string failure)
        {
            library = null;
            failure = string.Empty;
            if (gallery == null)
            {
                failure = "Tree reference gallery is null.";
                return false;
            }

            if (proceduralSlots == null || proceduralSlots.Count != 20)
            {
                failure =
                    "Unified generation requires exactly twenty procedural comparison slots; found " +
                    (proceduralSlots != null ? proceduralSlots.Count : 0) + ".";
                return false;
            }

            EnsureFolderPath(LibraryFolderPath);
            library = AssetDatabase.LoadAssetAtPath<TreeGenerationLibrary>(
                LibraryAssetPath);
            bool createdLibrary = false;
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<TreeGenerationLibrary>();
                library.name = "TreeGenerationLibrary";
                AssetDatabase.CreateAsset(library, LibraryAssetPath);
                createdLibrary = true;
            }

            UnityEngine.Object[] existingSubAssets =
                AssetDatabase.LoadAllAssetsAtPath(LibraryAssetPath);
            var profiles = new List<TreeFamilyProfile>(4);
            var palettes = new List<TreeMaterialPalette>(4);
            int upgradedProfileCount = 0;
            int upgradedRecipeCount = 0;
            int neutralizedBarkTintCount = 0;
            var migrationNotes = new List<string>();
            for (int familyIndex = 0; familyIndex < 4; familyIndex++)
            {
                TreeFamily family = (TreeFamily)familyIndex;
                TreeMaterialPalette palette = FindPalette(
                    existingSubAssets,
                    family,
                    "TMP_" + family);
                if (palette == null)
                {
                    palette = ScriptableObject.CreateInstance<TreeMaterialPalette>();
                    palette.name = "TMP_" + family;
                    palette.ResetForFamily(family);
                    AssetDatabase.AddObjectToAsset(palette, library);
                }

                TreeFamilyProfile profile = FindProfile(
                    existingSubAssets,
                    family);
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<TreeFamilyProfile>();
                    profile.name = "TFP_" + family;
                    profile.ResetToFamilyDefaults(family);
                    AssetDatabase.AddObjectToAsset(profile, library);
                }
                else if (profile.UpgradeManagedDefaults(family))
                {
                    upgradedProfileCount++;
                    migrationNotes.Add(
                        "MIGRATE | " + profile.name +
                        " | barkGrammar=" + profile.BarkGrammarVersion +
                        " | twistRange=" +
                        profile.Trunk.SurfaceTorsionDegrees.Minimum.ToString("F1") +
                        ".." +
                        profile.Trunk.SurfaceTorsionDegrees.Maximum.ToString("F1") +
                        " | rootCount=" +
                        profile.Trunk.RootButtressCount.Minimum +
                        ".." +
                        profile.Trunk.RootButtressCount.Maximum +
                        " | branchCalibration=Twisted/Dead denser, thicker, shorter, more upward defaults where legacy values matched" +
                        " | preserved=non-legacy authored ranges and all unrelated fields");
                    EditorUtility.SetDirty(profile);
                }

                if (profile.DefaultPalette != palette)
                {
                    profile.SetDefaultPalette(palette);
                    EditorUtility.SetDirty(profile);
                }

                palettes.Add(palette);
                profiles.Add(profile);
            }

            var variants = new List<TreeGenerationLibraryVariant>(20);
            for (int slotIndex = 0; slotIndex < proceduralSlots.Count; slotIndex++)
            {
                TreeReferenceSpecimen slot = proceduralSlots[slotIndex];
                if (slot == null ||
                    slot.Role != TreeReferenceRole.ProceduralComparison)
                {
                    failure =
                        "Unified generation encountered an invalid procedural slot at index " +
                        slotIndex + ".";
                    return false;
                }

                TreeFamilyProfile profile = profiles[(int)slot.Family];
                TreeMaterialPalette palette = palettes[(int)slot.Family];
                string identitySuffix =
                    slot.Family.ToString().ToLowerInvariant() + "-" +
                    slot.SourceVariantIndex;
                string calibrationIdentity =
                    "tree-reference-" + identitySuffix;
                string recipeIdentity = "tree-recipe-" + identitySuffix;

                TreeReferenceCalibrationPreset calibration = FindCalibration(
                    existingSubAssets,
                    calibrationIdentity,
                    "TRC_" + slot.Family + "_" + slot.SourceVariantIndex);
                if (calibration == null)
                {
                    calibration = ScriptableObject.CreateInstance<
                        TreeReferenceCalibrationPreset>();
                    calibration.name =
                        "TRC_" + slot.Family + "_" +
                        slot.SourceVariantIndex;
                    calibration.Initialize(
                        slot.Family,
                        calibrationIdentity,
                        slot.SourceAssetPath,
                        slot.SourceAssetGuid,
                        slot.VisibleHeight,
                        slot.AuditedBounds.size.x,
                        slot.AuditedBounds.size.z);
                    AssetDatabase.AddObjectToAsset(calibration, library);
                }
                else
                {
                    calibration.SynchronizeImportedReference(
                        slot.SourceAssetPath,
                        slot.SourceAssetGuid,
                        slot.VisibleHeight,
                        slot.AuditedBounds.size.x,
                        slot.AuditedBounds.size.z);
                    EditorUtility.SetDirty(calibration);
                }

                calibration.ApplyFamilyStructuralRanges(profile);
                EditorUtility.SetDirty(calibration);

                TreeGenerationRecipe recipe = FindRecipe(
                    existingSubAssets,
                    recipeIdentity,
                    "TR_" + slot.Family + "_" + slot.SourceVariantIndex);
                bool createdRecipe = recipe == null;
                if (createdRecipe)
                {
                    recipe = ScriptableObject.CreateInstance<TreeGenerationRecipe>();
                    recipe.name =
                        "TR_" + slot.Family + "_" +
                        slot.SourceVariantIndex;
                    int seed = TreeDeterministicUtility.DeriveSeed(
                        "tree-generation-library",
                        slot.Family,
                        slot.SourceVariantIndex,
                        slot.SourceAssetGuid);
                    recipe.Initialize(
                        profile,
                        palette,
                        calibration,
                        recipeIdentity,
                        seed);
                    AssetDatabase.AddObjectToAsset(recipe, library);
                }
                else
                {
                    recipe.RepairManagedBindings(
                        profile,
                        palette,
                        calibration);
                }

                bool hadExplicitTrunkTwist =
                    recipe.Overrides.TrunkSurfaceTorsionDegrees.IsSet;
                bool applyManagedTwist = TryResolveManagedRepresentativeTwist(
                    slot.Family,
                    slot.SourceVariantIndex,
                    out float previousManagedTwistDegrees,
                    out float managedTwistDegrees);
                bool twistDefaultAdded = false;
                bool managedTwistUpgraded = false;
                if (applyManagedTwist)
                {
                    if (createdRecipe)
                    {
                        twistDefaultAdded =
                            recipe.ConfigureManagedTrunkTwistDefault(
                                managedTwistDegrees);
                    }
                    else
                    {
                        managedTwistUpgraded =
                            recipe.UpgradeManagedTrunkTwistDefault(
                                previousManagedTwistDegrees,
                                managedTwistDegrees);
                    }
                }

                bool applyManagedRoot = TryResolveManagedRepresentativeRoot(
                    slot.Family,
                    slot.SourceVariantIndex,
                    out int previousManagedRootCount,
                    out int managedRootCount,
                    out float previousManagedRootStrength,
                    out float managedRootStrength,
                    out float managedRootHeight,
                    out float previousManagedRootFlare,
                    out float managedRootFlare);
                bool rootDefaultsAdded = false;
                if (applyManagedRoot)
                {
                    rootDefaultsAdded = createdRecipe
                        ? recipe.ConfigureManagedRootButtressDefaults(
                            managedRootCount,
                            managedRootStrength,
                            managedRootHeight,
                            managedRootFlare)
                        : recipe.UpgradeManagedRootButtressDefaults(
                            previousManagedRootCount,
                            managedRootCount,
                            previousManagedRootStrength,
                            managedRootStrength,
                            managedRootHeight,
                            previousManagedRootFlare,
                            managedRootFlare);
                }

                bool applyManagedPathSpiral =
                    TryResolveManagedRepresentativePathSpiral(
                        slot.Family,
                        slot.SourceVariantIndex,
                        out float managedPathStrength,
                        out float managedPathTurns,
                        out float managedPathDirection);
                bool pathSpiralDefaultsAdded = applyManagedPathSpiral &&
                    recipe.ConfigureManagedPathSpiralDefaults(
                        managedPathStrength,
                        managedPathTurns,
                        managedPathDirection);

                bool hadExplicitBarkTint = recipe.Overrides.BarkTint.Enabled;
                bool upgradedRecipe = recipe.UpgradeManagedDefaults(
                    neutralComparisonBark: true,
                    applyManagedTrunkTwistDefault: applyManagedTwist,
                    managedTrunkTwistDegrees: managedTwistDegrees);
                bool managedTwistAdded =
                    applyManagedTwist &&
                    !hadExplicitTrunkTwist &&
                    recipe.Overrides.TrunkSurfaceTorsionDegrees.IsSet;
                if (upgradedRecipe ||
                    twistDefaultAdded ||
                    managedTwistUpgraded ||
                    rootDefaultsAdded ||
                    pathSpiralDefaultsAdded)
                {
                    upgradedRecipeCount++;
                    bool neutralized =
                        !hadExplicitBarkTint && recipe.Overrides.BarkTint.Enabled;
                    if (neutralized)
                    {
                        neutralizedBarkTintCount++;
                    }

                    migrationNotes.Add(
                        "MIGRATE | " + recipe.name +
                        " | barkTint=" + (neutralized
                            ? "neutral comparison override added"
                            : "existing explicit override preserved") +
                        " | trunkTwist=" + (managedTwistUpgraded
                            ? previousManagedTwistDegrees.ToString("F1") +
                              " -> " +
                              recipe.Overrides.TrunkSurfaceTorsionDegrees.ExactValue.ToString("F1") +
                              " degrees managed default upgraded"
                            : managedTwistAdded
                                ? recipe.Overrides.TrunkSurfaceTorsionDegrees.ExactValue.ToString("F1") + " degrees managed default added"
                                : hadExplicitTrunkTwist
                                    ? "existing explicit override preserved"
                                    : "not applicable") +
                        " | root=" + (rootDefaultsAdded
                            ? recipe.Overrides.RootButtressCount.ExactValue +
                              "/" +
                              recipe.Overrides.RootButtressStrength.ExactValue.ToString("F3") +
                              "/" +
                              recipe.Overrides.RootButtressHeight.ExactValue.ToString("F3") +
                              "/" +
                              recipe.Overrides.RootFlareScale.ExactValue.ToString("F3") +
                              " managed defaults added/upgraded"
                            : applyManagedRoot
                                ? "existing explicit values preserved"
                                : "not applicable") +
                        " | pathSpiral=" + (pathSpiralDefaultsAdded
                            ? recipe.Overrides.TrunkSpiralStrength.ExactValue.ToString("F3") +
                              "/" +
                              recipe.Overrides.TrunkSpiralTurns.ExactValue.ToString("F2") +
                              "/" +
                              recipe.Overrides.TrunkSpiralDirection.ExactValue.ToString("F0") +
                              " managed defaults added"
                            : applyManagedPathSpiral
                                ? "existing explicit values preserved"
                                : "not applicable"));
                }
                EditorUtility.SetDirty(recipe);

                var variant = new TreeGenerationLibraryVariant();
                variant.Configure(
                    slot.Family,
                    slot.SourceVariantIndex,
                    slot.Family + " " + slot.SourceVariantIndex,
                    slot.SourceAssetPath,
                    slot.SourceAssetGuid,
                    profile,
                    palette,
                    calibration,
                    recipe);
                variants.Add(variant);
            }

            variants.Sort(CompareVariants);
            library.ReplaceManagedContent(profiles, palettes, variants);
            EditorUtility.SetDirty(library);
            gallery.SetGenerationLibrary(library);
            EditorUtility.SetDirty(gallery);
            AssetDatabase.SaveAssets();

            var failures = new List<string>();
            bool valid = library.ValidateLibrary(failures);
            report.AppendLine("[Managed Generation Library]");
            report.Append(valid ? "PASS" : "FAIL")
                .Append(" | ")
                .Append(LibraryAssetPath)
                .Append(" | created=")
                .Append(createdLibrary ? "Yes" : "No")
                .Append(" | profiles=")
                .Append(library.FamilyProfiles.Count)
                .Append(" | palettes=")
                .Append(library.MaterialPalettes.Count)
                .Append(" | recipes=")
                .Append(library.VariantCount)
                .Append(" | libraryVersion=")
                .Append(TreeGenerationLibrary.CurrentLibraryVersion)
                .Append(" | profileVersion=")
                .Append(TreeFamilyProfile.CurrentProfileVersion)
                .Append(" | seedVersion=")
                .Append(TreeGenerationRecipe.CurrentDeterministicSeedVersion)
                .Append(" | barkGrammarVersion=")
                .Append(TreeFamilyProfile.CurrentBarkGrammarVersion)
                .Append(" | upgradedProfiles=")
                .Append(upgradedProfileCount)
                .Append(" | upgradedRecipes=")
                .Append(upgradedRecipeCount)
                .Append(" | neutralBarkRecipes=")
                .Append(neutralizedBarkTintCount)
                .AppendLine();
            for (int index = 0; index < migrationNotes.Count; index++)
            {
                report.AppendLine(migrationNotes[index]);
            }
            for (int index = 0; index < failures.Count; index++)
            {
                report.Append("FAIL | ").AppendLine(failures[index]);
            }

            if (!valid)
            {
                failure = "Managed tree-generation library validation failed.";
                return false;
            }

            return true;
        }

        private static bool TryResolveManagedRepresentativeTwist(
            TreeFamily family,
            int variantIndex,
            out float previousManagedDegrees,
            out float currentManagedDegrees)
        {
            if (variantIndex == 1 && family == TreeFamily.Twisted)
            {
                previousManagedDegrees = -210f;
                currentManagedDegrees = -330f;
                return true;
            }

            if (variantIndex == 1 && family == TreeFamily.Dead)
            {
                previousManagedDegrees = -180f;
                currentManagedDegrees = -270f;
                return true;
            }

            previousManagedDegrees = 0f;
            currentManagedDegrees = 0f;
            return false;
        }

        private static bool TryResolveManagedRepresentativeRoot(
            TreeFamily family,
            int variantIndex,
            out int previousCount,
            out int currentCount,
            out float previousStrength,
            out float currentStrength,
            out float currentHeight,
            out float previousFlare,
            out float currentFlare)
        {
            if (variantIndex != 1)
            {
                previousCount = 0;
                currentCount = 0;
                previousStrength = 0f;
                currentStrength = 0f;
                currentHeight = 0f;
                previousFlare = 1f;
                currentFlare = 1f;
                return false;
            }

            switch (family)
            {
                case TreeFamily.Common:
                    previousCount = 5;
                    currentCount = 5;
                    previousStrength = 0.68f;
                    currentStrength = 0.72f;
                    currentHeight = 0.16f;
                    previousFlare = 1.12f;
                    currentFlare = 1.39f;
                    return true;
                case TreeFamily.Pine:
                    previousCount = 5;
                    currentCount = 5;
                    previousStrength = 0.30f;
                    currentStrength = 0.30f;
                    currentHeight = 0.16f;
                    previousFlare = 1.18f;
                    currentFlare = 1.18f;
                    return true;
                case TreeFamily.Twisted:
                    previousCount = 5;
                    currentCount = 5;
                    previousStrength = 0.82f;
                    currentStrength = 0.88f;
                    currentHeight = 0.22f;
                    previousFlare = 1.18f;
                    currentFlare = 1.52f;
                    return true;
                case TreeFamily.Dead:
                    previousCount = 6;
                    currentCount = 6;
                    previousStrength = 0.70f;
                    currentStrength = 0.84f;
                    currentHeight = 0.20f;
                    previousFlare = 1.38f;
                    currentFlare = 1.48f;
                    return true;
                default:
                    previousCount = 0;
                    currentCount = 0;
                    previousStrength = 0f;
                    currentStrength = 0f;
                    currentHeight = 0f;
                    previousFlare = 1f;
                    currentFlare = 1f;
                    return false;
            }
        }

        private static bool TryResolveManagedRepresentativePathSpiral(
            TreeFamily family,
            int variantIndex,
            out float strength,
            out float turns,
            out float direction)
        {
            if (variantIndex == 1 && family == TreeFamily.Twisted)
            {
                strength = 0.18f;
                turns = 1f;
                direction = -1f;
                return true;
            }

            if (variantIndex == 1 && family == TreeFamily.Dead)
            {
                strength = 0.10f;
                turns = 0.75f;
                direction = -1f;
                return true;
            }

            strength = 0f;
            turns = 0f;
            direction = 1f;
            return false;
        }

        private static int CompareVariants(
            TreeGenerationLibraryVariant left,
            TreeGenerationLibraryVariant right)
        {
            int familyComparison = left.Family.CompareTo(right.Family);
            return familyComparison != 0
                ? familyComparison
                : left.VariantIndex.CompareTo(right.VariantIndex);
        }

        private static TreeFamilyProfile FindProfile(
            UnityEngine.Object[] assets,
            TreeFamily family)
        {
            for (int index = 0; index < assets.Length; index++)
            {
                if (assets[index] is TreeFamilyProfile profile &&
                    profile.Family == family)
                {
                    return profile;
                }
            }

            return null;
        }

        private static TreeMaterialPalette FindPalette(
            UnityEngine.Object[] assets,
            TreeFamily family,
            string expectedName)
        {
            string expectedIdentity =
                "tree-palette-" + family.ToString().ToLowerInvariant();
            for (int index = 0; index < assets.Length; index++)
            {
                if (assets[index] is TreeMaterialPalette palette &&
                    (palette.StableIdentity == expectedIdentity ||
                     palette.name == expectedName))
                {
                    return palette;
                }
            }

            return null;
        }

        private static TreeReferenceCalibrationPreset FindCalibration(
            UnityEngine.Object[] assets,
            string identity,
            string expectedName)
        {
            for (int index = 0; index < assets.Length; index++)
            {
                if (assets[index] is TreeReferenceCalibrationPreset calibration &&
                    (calibration.StableIdentity == identity ||
                     calibration.name == expectedName))
                {
                    return calibration;
                }
            }

            return null;
        }

        private static TreeGenerationRecipe FindRecipe(
            UnityEngine.Object[] assets,
            string identity,
            string expectedName)
        {
            for (int index = 0; index < assets.Length; index++)
            {
                if (assets[index] is TreeGenerationRecipe recipe &&
                    (recipe.StableIdentity == identity ||
                     recipe.name == expectedName))
                {
                    return recipe;
                }
            }

            return null;
        }

        private static void EnsureFolderPath(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }
    }
}
