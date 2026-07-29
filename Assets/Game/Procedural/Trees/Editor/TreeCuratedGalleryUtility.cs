using System;
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
