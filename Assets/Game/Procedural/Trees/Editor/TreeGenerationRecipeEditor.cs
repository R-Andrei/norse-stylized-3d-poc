using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    [CustomEditor(typeof(TreeGenerationRecipe))]
    public sealed class TreeGenerationRecipeEditor : UnityEditor.Editor
    {
        private SerializedProperty stableIdentity;
        private SerializedProperty recipeDisplayName;
        private SerializedProperty recipeDescription;
        private SerializedProperty recipeTags;
        private SerializedProperty barkMaterial;
        private SerializedProperty controlRanges;

        private SerializedProperty familyProfile;
        private SerializedProperty referenceCalibration;
        private SerializedProperty paletteOverride;
        private SerializedProperty ageClass;
        private SerializedProperty masterSeed;
        private SerializedProperty overridesProperty;
        private SerializedProperty seedLocks;

        private void OnEnable()
        {
            stableIdentity = serializedObject.FindProperty("stableIdentity");
            recipeDisplayName = serializedObject.FindProperty(
                "recipeDisplayName");
            recipeDescription = serializedObject.FindProperty(
                "recipeDescription");
            recipeTags = serializedObject.FindProperty("recipeTags");
            barkMaterial = serializedObject.FindProperty("barkMaterial");
            controlRanges = serializedObject.FindProperty("controlRanges");

            familyProfile = serializedObject.FindProperty("familyProfile");
            referenceCalibration = serializedObject.FindProperty(
                "referenceCalibration");
            paletteOverride = serializedObject.FindProperty("paletteOverride");
            ageClass = serializedObject.FindProperty("ageClass");
            masterSeed = serializedObject.FindProperty("masterSeed");
            overridesProperty = serializedObject.FindProperty("overrides");
            seedLocks = serializedObject.FindProperty("seedLocks");
        }

        public override void OnInspectorGUI()
        {
            var recipe = (TreeGenerationRecipe)target;
            serializedObject.UpdateIfRequiredOrScript();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("m_Script"));
            }

            EditorGUILayout.LabelField(
                "Standalone Recipe",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                recipeDisplayName,
                new GUIContent(
                    "Recipe Name",
                    "Author-facing recipe name. This does not participate in deterministic sampling."));
            EditorGUILayout.PropertyField(
                recipeDescription,
                new GUIContent(
                    "Description",
                    "Freeform authoring description. It does not affect generation."));
            EditorGUILayout.PropertyField(
                recipeTags,
                new GUIContent(
                    "Tags",
                    "Searchable author-defined labels such as alder, norway-spruce, windswept, high-crown or branches-down."),
                true);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    stableIdentity,
                    new GUIContent(
                        "Stable ID",
                        "Immutable internal identity. Copying a recipe creates a new ID; renaming does not change it."));
            }
            EditorGUILayout.PropertyField(
                barkMaterial,
                new GUIContent(
                    "Bark Material",
                    "Optional bark material binding for the recipe-only architecture. Shared material surface settings remain material-owned."));

            if (recipe.IsCuratedRecipeDefinition(
                out TreeCuratedRecipeDefinition curatedDefinition))
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextArea(
                        "Curated baseline: " +
                        curatedDefinition.ReferenceIntent +
                        "\nFoliage target (deferred): " +
                        curatedDefinition.FoliageTarget,
                        EditorStyles.textArea,
                        GUILayout.MinHeight(52f));
                }
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New Recipe"))
            {
                TreeRecipeAssetUtility.CreateRecipe(
                    null,
                    TreeRecipeAssetUtility.ResolvePreferredCatalog(recipe));
            }
            if (GUILayout.Button("Copy This Recipe"))
            {
                TreeRecipeAssetUtility.CreateRecipe(recipe, null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "These intervals are the live recipe-only authoring schema. Applying or spawning this recipe samples them into one exact tree snapshot. Existing spawned trees remain unchanged until explicitly reapplied.",
                MessageType.Info);

            EditorGUILayout.Space(4f);
            TreeControlRangeDrawer.Draw(controlRanges, target);

            DrawLegacyCompatibility();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLegacyCompatibility()
        {
            string sessionKey =
                "PS3D.TreeControls.LegacyRecipe." +
                target.GetEntityId();
            bool expanded = SessionState.GetBool(sessionKey, false);
            bool nextExpanded = EditorGUILayout.Foldout(
                expanded,
                "Legacy Compatibility — Temporary",
                true,
                EditorStyles.foldoutHeader);
            if (nextExpanded != expanded)
            {
                SessionState.SetBool(sessionKey, nextExpanded);
            }

            if (!nextExpanded)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(
                    "These fields are retained only for explicit legacy compatibility evidence. Standalone curated recipes do not read them during recipe-only generation.",
                    EditorStyles.textArea,
                    GUILayout.MinHeight(42f));
            }
            EditorGUILayout.PropertyField(familyProfile);
            EditorGUILayout.PropertyField(referenceCalibration);
            EditorGUILayout.PropertyField(paletteOverride);
            EditorGUILayout.PropertyField(ageClass);
            EditorGUILayout.PropertyField(masterSeed);
            EditorGUILayout.PropertyField(overridesProperty, true);
            EditorGUILayout.PropertyField(seedLocks, true);
            EditorGUILayout.EndVertical();
        }
    }

    internal static class TreeRecipeAssetUtility
    {
        internal static TreeRecipeCatalog ResolvePreferredCatalog(
            TreeGenerationRecipe source)
        {
            string[] catalogGuids = AssetDatabase.FindAssets(
                "t:TreeRecipeCatalog");
            TreeRecipeCatalog soleCatalog = null;
            for (int index = 0; index < catalogGuids.Length; index++)
            {
                TreeRecipeCatalog catalog =
                    AssetDatabase.LoadAssetAtPath<TreeRecipeCatalog>(
                        AssetDatabase.GUIDToAssetPath(catalogGuids[index]));
                if (catalog == null)
                {
                    continue;
                }

                soleCatalog = catalog;
                if (source != null && catalog.Contains(source))
                {
                    return catalog;
                }
            }

            return catalogGuids.Length == 1 ? soleCatalog : null;
        }

        internal static TreeGenerationRecipe CreateRecipe(
            TreeGenerationRecipe source,
            TreeRecipeCatalog preferredCatalog)
        {
            string suggestedName = source != null
                ? source.RecipeDisplayName + " Copy"
                : "New Tree Recipe";
            string suggestedFileName = SanitizeFileName(suggestedName) +
                ".asset";
            string defaultFolder = ResolveDefaultFolder(source, preferredCatalog);
            string path = EditorUtility.SaveFilePanelInProject(
                source != null ? "Copy Tree Recipe" : "Create Tree Recipe",
                suggestedFileName,
                "asset",
                "Choose where to save the standalone tree recipe.",
                defaultFolder);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                EditorUtility.DisplayDialog(
                    "Tree Recipe Already Exists",
                    "A project asset already exists at the selected path. Choose a new recipe filename; existing assets are never overwritten by Create or Copy Recipe.",
                    "OK");
                return null;
            }

            var recipe = ScriptableObject.CreateInstance<TreeGenerationRecipe>();
            if (source != null)
            {
                EditorUtility.CopySerialized(source, recipe);
                recipe.name = Path.GetFileNameWithoutExtension(path);
                recipe.SetRecipeDisplayName(suggestedName);
                recipe.RegenerateStableIdentity();
                recipe.EnsureRecipeOnlyFoundation();
            }
            else
            {
                recipe.name = Path.GetFileNameWithoutExtension(path);
                recipe.InitializeRecipeOnlyFoundation(suggestedName);
            }

            AssetDatabase.CreateAsset(recipe, path);
            RegisterInCatalogs(recipe, source, preferredCatalog);
            EditorUtility.SetDirty(recipe);
            AssetDatabase.SaveAssets();
            Selection.activeObject = recipe;
            EditorGUIUtility.PingObject(recipe);
            return recipe;
        }

        private static void RegisterInCatalogs(
            TreeGenerationRecipe created,
            TreeGenerationRecipe source,
            TreeRecipeCatalog preferredCatalog)
        {
            var catalogs = new List<TreeRecipeCatalog>();
            if (preferredCatalog != null)
            {
                catalogs.Add(preferredCatalog);
            }
            else
            {
                string[] catalogGuids = AssetDatabase.FindAssets(
                    "t:TreeRecipeCatalog");
                for (int index = 0; index < catalogGuids.Length; index++)
                {
                    string catalogPath = AssetDatabase.GUIDToAssetPath(
                        catalogGuids[index]);
                    TreeRecipeCatalog catalog =
                        AssetDatabase.LoadAssetAtPath<TreeRecipeCatalog>(
                            catalogPath);
                    if (catalog == null)
                    {
                        continue;
                    }

                    if (source != null && catalog.Contains(source))
                    {
                        catalogs.Add(catalog);
                    }
                }

                if (catalogs.Count == 0 && catalogGuids.Length == 1)
                {
                    TreeRecipeCatalog soleCatalog =
                        AssetDatabase.LoadAssetAtPath<TreeRecipeCatalog>(
                            AssetDatabase.GUIDToAssetPath(catalogGuids[0]));
                    if (soleCatalog != null)
                    {
                        catalogs.Add(soleCatalog);
                    }
                }
            }

            var seen = new HashSet<TreeRecipeCatalog>();
            for (int index = 0; index < catalogs.Count; index++)
            {
                TreeRecipeCatalog catalog = catalogs[index];
                if (catalog == null || !seen.Add(catalog))
                {
                    continue;
                }

                Undo.RecordObject(catalog, "Register Tree Recipe");
                if (catalog.Register(created))
                {
                    EditorUtility.SetDirty(catalog);
                }
            }
        }

        private static string ResolveDefaultFolder(
            TreeGenerationRecipe source,
            TreeRecipeCatalog preferredCatalog)
        {
            string assetPath = source != null
                ? AssetDatabase.GetAssetPath(source)
                : preferredCatalog != null
                    ? AssetDatabase.GetAssetPath(preferredCatalog)
                    : "Assets";
            if (string.IsNullOrEmpty(assetPath))
            {
                return "Assets";
            }

            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return assetPath;
            }

            string directory = Path.GetDirectoryName(assetPath);
            return string.IsNullOrEmpty(directory)
                ? "Assets"
                : directory.Replace('\\', '/');
        }

        private static string SanitizeFileName(string value)
        {
            string result = string.IsNullOrWhiteSpace(value)
                ? "TreeRecipe"
                : value.Trim();
            char[] invalid = Path.GetInvalidFileNameChars();
            for (int index = 0; index < invalid.Length; index++)
            {
                result = result.Replace(invalid[index], '_');
            }

            return result;
        }
    }
}
