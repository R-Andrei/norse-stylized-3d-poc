using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    [CustomEditor(typeof(TreeRecipeCatalog))]
    public sealed class TreeRecipeCatalogEditor : UnityEditor.Editor
    {
        private SerializedProperty stableIdentity;
        private SerializedProperty displayName;
        private SerializedProperty description;
        private SerializedProperty recipes;
        private TreeGenerationRecipe selectedRecipe;
        private string searchText = string.Empty;

        private void OnEnable()
        {
            stableIdentity = serializedObject.FindProperty("stableIdentity");
            displayName = serializedObject.FindProperty("displayName");
            description = serializedObject.FindProperty("description");
            recipes = serializedObject.FindProperty("recipes");
        }

        public override void OnInspectorGUI()
        {
            var catalog = (TreeRecipeCatalog)target;
            serializedObject.UpdateIfRequiredOrScript();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("m_Script"));
                EditorGUILayout.PropertyField(stableIdentity);
            }
            EditorGUILayout.PropertyField(displayName);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.HelpBox(
                "This catalog is an index only. It contributes no generation values and is not an inheritance layer.",
                MessageType.None);
            EditorGUILayout.PropertyField(recipes, true);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Initial Curated Catalog",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Creates only the thirteen approved Alder, Norway Spruce, Wych Elm and dead-tree recipes. Imported reference specimens are not converted into public recipes. Creating missing recipes never overwrites existing curated assets.",
                MessageType.Info);
            if (GUILayout.Button(
                "Create Missing Initial Curated Recipes"))
            {
                TreeCuratedRecipeCatalogBuilder.CreateMissing(catalog);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(
                "Validate Initial Curated Recipes"))
            {
                TreeCuratedRecipeCatalogBuilder.Validate(catalog);
            }
            if (GUILayout.Button(
                "Reset All To Approved Baseline"))
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Reset Initial Curated Recipes?",
                    "This explicitly overwrites the intervals, metadata and bark-material binding of all thirteen curated recipes with the approved TREE-CONTROLS.2 baseline. Extra user-created recipes are not changed.",
                    "Reset Curated Recipes",
                    "Cancel");
                if (confirmed)
                {
                    TreeCuratedRecipeCatalogBuilder
                        .ResetAllToApprovedBaseline(catalog);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Curated Report"))
            {
                TreeCuratedRecipeCatalogBuilder.CopyLatestReport();
            }
            if (GUILayout.Button("Open Report Folder"))
            {
                TreeCuratedRecipeCatalogBuilder.OpenReportFolder();
            }
            EditorGUILayout.EndHorizontal();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(
                    TreeCuratedRecipeCatalogBuilder.LatestSummary,
                    EditorStyles.textArea,
                    GUILayout.MinHeight(38f));
            }

            EditorGUILayout.Space(4f);
            selectedRecipe = (TreeGenerationRecipe)EditorGUILayout.ObjectField(
                new GUIContent(
                    "Selected Recipe",
                    "Recipe used by Copy Selected Recipe."),
                selectedRecipe,
                typeof(TreeGenerationRecipe),
                false);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New Recipe"))
            {
                TreeRecipeAssetUtility.CreateRecipe(null, catalog);
            }
            using (new EditorGUI.DisabledScope(selectedRecipe == null))
            {
                if (GUILayout.Button("Copy Selected Recipe"))
                {
                    TreeRecipeAssetUtility.CreateRecipe(
                        selectedRecipe,
                        catalog);
                }
                if (GUILayout.Button("Ping Selected Recipe"))
                {
                    Selection.activeObject = selectedRecipe;
                    EditorGUIUtility.PingObject(selectedRecipe);
                }
            }
            EditorGUILayout.EndHorizontal();

            searchText = EditorGUILayout.TextField(
                new GUIContent(
                    "Search",
                    "Filters the read-only catalog summary by recipe name, tag, stable ID or description."),
                searchText);
            DrawFilteredSummary(catalog);
        }

        private void DrawFilteredSummary(TreeRecipeCatalog catalog)
        {
            string filter = searchText?.Trim().ToLowerInvariant() ??
                string.Empty;
            EditorGUILayout.LabelField(
                "Catalog Summary",
                EditorStyles.boldLabel);
            for (int index = 0; index < catalog.Recipes.Count; index++)
            {
                TreeGenerationRecipe recipe = catalog.Recipes[index];
                if (recipe == null || !Matches(recipe, filter))
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(recipe.RecipeDisplayName);
                if (GUILayout.Button("Select", GUILayout.Width(58f)))
                {
                    selectedRecipe = recipe;
                    Selection.activeObject = recipe;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private static bool Matches(
            TreeGenerationRecipe recipe,
            string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }

            string stableIdentity = recipe.StableIdentity ?? string.Empty;
            if (recipe.RecipeDisplayName.ToLowerInvariant().Contains(filter) ||
                stableIdentity.ToLowerInvariant().Contains(filter) ||
                recipe.RecipeDescription.ToLowerInvariant().Contains(filter))
            {
                return true;
            }

            for (int index = 0; index < recipe.RecipeTags.Count; index++)
            {
                string tag = recipe.RecipeTags[index];
                if (!string.IsNullOrEmpty(tag) &&
                    tag.ToLowerInvariant().Contains(filter))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
