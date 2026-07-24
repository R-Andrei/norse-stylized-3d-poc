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
                        " | added=compact trunk ridge and root-buttress grammar" +
                        " | preserved=existing twist range, structural seed versions, and all unrelated authored fields");
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
                if (recipe == null)
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

                bool hadExplicitBarkTint = recipe.Overrides.BarkTint.Enabled;
                if (recipe.UpgradeManagedDefaults(neutralComparisonBark: true))
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
                        " | legacy attachment endpoints pinned and branch overrides mapped when present" +
                        " | barkTint=" + (neutralized
                            ? "neutral comparison override added"
                            : "existing explicit override preserved"));
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
