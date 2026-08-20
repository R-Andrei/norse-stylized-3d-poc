using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeCuratedGalleryUtility
    {
        internal const string DefaultCatalogPath =
            "Assets/Game/Demo/Profiles/Trees/TreeRecipeCatalog.asset";

        internal static bool TryResolveCatalog(
            TreeReferenceGallery gallery,
            out TreeRecipeCatalog catalog,
            out string failure)
        {
            catalog = gallery != null ? gallery.RecipeCatalog : null;
            failure = string.Empty;
            if (catalog == null)
            {
                catalog = AssetDatabase.LoadAssetAtPath<TreeRecipeCatalog>(
                    DefaultCatalogPath);
            }

            if (catalog == null)
            {
                string[] guids = AssetDatabase.FindAssets(
                    "t:TreeRecipeCatalog");
                if (guids.Length == 1)
                {
                    catalog = AssetDatabase.LoadAssetAtPath<TreeRecipeCatalog>(
                        AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (catalog == null)
            {
                failure =
                    "No TreeRecipeCatalog was assigned and no unique catalog asset could be found.";
                return false;
            }

            if (gallery != null && gallery.RecipeCatalog != catalog)
            {
                Undo.RecordObject(gallery, "Assign Curated Tree Recipe Catalog");
                gallery.SetRecipeCatalog(catalog);
                EditorUtility.SetDirty(gallery);
                if (gallery.gameObject.scene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(gallery.gameObject.scene);
                }
            }

            return true;
        }


        internal static bool TryResolveRecipeAuthoringTarget(
            TreeReferenceGallery gallery,
            ProceduralTreeInstance instance,
            out TreeReferenceSpecimen specimen,
            out TreeGenerationRecipe recipe,
            out string consumerSummary,
            out bool instanceRecipeMatches,
            out string failure)
        {
            specimen = null;
            recipe = null;
            consumerSummary = string.Empty;
            instanceRecipeMatches = false;
            failure = string.Empty;
            if (gallery == null || instance == null)
            {
                failure = "Gallery or source tree instance is null.";
                return false;
            }

            TreeReferenceGallery owningGallery =
                instance.GetComponentInParent<TreeReferenceGallery>();
            if (owningGallery != gallery)
            {
                failure =
                    "The last selected generated tree does not belong to this Tree Reference Gallery.";
                return false;
            }

            specimen = instance.GetComponentInParent<TreeReferenceSpecimen>();
            if (specimen == null ||
                specimen.Role != TreeReferenceRole.ProceduralComparison)
            {
                failure =
                    "The source tree is not under a procedural comparison slot.";
                return false;
            }

            TreeRecipeSpawner spawner =
                specimen.GetComponent<TreeRecipeSpawner>();
            if (spawner == null || spawner.GeneratedInstance != instance)
            {
                failure =
                    "The source tree is not the generated child owned by its curated gallery slot.";
                return false;
            }

            if (!instance.HasExactControls || instance.ExactControls == null)
            {
                failure =
                    "The source tree has no initialized exact controls to recenter from.";
                return false;
            }

            TreeRecipeCatalog catalog = gallery.RecipeCatalog;
            if (catalog == null)
            {
                failure =
                    "The gallery has no Recipe Catalog assigned. Recenter does not auto-assign or modify gallery bindings.";
                return false;
            }

            string recipeIdentity =
                TreeCuratedGalleryAssignment.ResolveRecipeStableIdentity(
                    specimen.Family,
                    specimen.SourceVariantIndex);
            if (!catalog.TryFindByStableIdentity(
                    recipeIdentity,
                    out recipe) ||
                recipe == null)
            {
                failure =
                    "The curated gallery assignment resolves to a recipe that is missing from the assigned catalog: " +
                    recipeIdentity;
                return false;
            }

            if (recipe.ControlRanges == null)
            {
                failure =
                    "The mapped recipe has no control ranges.";
                return false;
            }

            instanceRecipeMatches = instance.Recipe == recipe;
            consumerSummary = BuildRecipeConsumerSummary(recipe.StableIdentity);
            return true;
        }

        private static string BuildRecipeConsumerSummary(
            string recipeStableIdentity)
        {
            var consumers = new List<string>();
            Array families = Enum.GetValues(typeof(TreeFamily));
            for (int familyIndex = 0;
                familyIndex < families.Length;
                familyIndex++)
            {
                var family = (TreeFamily)families.GetValue(familyIndex);
                for (int variant = 1; variant <= 5; variant++)
                {
                    string mappedIdentity =
                        TreeCuratedGalleryAssignment.ResolveRecipeStableIdentity(
                            family,
                            variant);
                    if (string.Equals(
                            mappedIdentity,
                            recipeStableIdentity,
                            StringComparison.Ordinal))
                    {
                        consumers.Add(family + " " + variant);
                    }
                }
            }

            return consumers.Count == 0
                ? "none"
                : string.Join(", ", consumers);
        }

        internal static bool TryConfigureSpawner(
            TreeReferenceGallery gallery,
            TreeReferenceSpecimen specimen,
            out TreeRecipeSpawner spawner,
            out string failure)
        {
            spawner = null;
            failure = string.Empty;
            if (gallery == null || specimen == null)
            {
                failure = "Gallery or procedural specimen is null.";
                return false;
            }

            if (specimen.Role != TreeReferenceRole.ProceduralComparison)
            {
                failure = "Specimen is not a procedural comparison slot.";
                return false;
            }

            if (!TryResolveCatalog(gallery, out TreeRecipeCatalog catalog, out failure))
            {
                return false;
            }

            string recipeIdentity =
                TreeCuratedGalleryAssignment.ResolveRecipeStableIdentity(
                    specimen.Family,
                    specimen.SourceVariantIndex);
            if (!catalog.TryFindByStableIdentity(
                    recipeIdentity,
                    out TreeGenerationRecipe recipe) ||
                recipe == null)
            {
                failure =
                    "Curated recipe is missing from the catalog: " +
                    recipeIdentity;
                return false;
            }

            spawner = specimen.GetComponent<TreeRecipeSpawner>();
            if (spawner == null)
            {
                spawner = Undo.AddComponent<TreeRecipeSpawner>(
                    specimen.gameObject);
            }

            Undo.RecordObject(spawner, "Configure Curated Gallery Tree Spawner");
            string slotIdentity =
                TreeCuratedGalleryAssignment.BuildSlotIdentity(
                    specimen.Family,
                    specimen.SourceVariantIndex);
            int seed = TreeCuratedGalleryAssignment.ResolveGallerySeed(
                gallery.CuratedGallerySeed,
                specimen.Family,
                specimen.SourceVariantIndex);
            spawner.Configure(
                recipe,
                seed,
                specimen.Family,
                specimen.SourceVariantIndex,
                slotIdentity);
            EditorUtility.SetDirty(spawner);
            return true;
        }
    }
}
