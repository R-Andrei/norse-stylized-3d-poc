using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [CreateAssetMenu(
        fileName = "TreeRecipeCatalog",
        menuName = "PS3D/Trees/Tree Recipe Catalog")]
    public sealed class TreeRecipeCatalog : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private string stableIdentity = string.Empty;

        [SerializeField]
        private string displayName = "Tree Recipes";

        [SerializeField, TextArea(2, 6)]
        private string description =
            "Index of standalone tree recipes. The catalog contributes no generation values.";

        [SerializeField]
        private List<TreeGenerationRecipe> recipes =
            new List<TreeGenerationRecipe>();

        public string StableIdentity => stableIdentity;
        public string DisplayName => displayName;
        public string Description => description;
        public IReadOnlyList<TreeGenerationRecipe> Recipes =>
            recipes ?? (IReadOnlyList<TreeGenerationRecipe>)
                Array.Empty<TreeGenerationRecipe>();

        public bool Register(TreeGenerationRecipe recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            recipes ??= new List<TreeGenerationRecipe>();
            if (recipes.Contains(recipe))
            {
                return false;
            }

            recipes.Add(recipe);
            return true;
        }

        public bool Unregister(TreeGenerationRecipe recipe)
        {
            return recipe != null && recipes != null && recipes.Remove(recipe);
        }

        public bool Contains(TreeGenerationRecipe recipe)
        {
            return recipe != null && recipes != null && recipes.Contains(recipe);
        }

        public bool TryFindByStableIdentity(
            string stableIdentity,
            out TreeGenerationRecipe recipe)
        {
            if (recipes != null)
            {
                for (int index = 0; index < recipes.Count; index++)
                {
                    TreeGenerationRecipe candidate = recipes[index];
                    if (candidate != null &&
                        string.Equals(
                            candidate.StableIdentity,
                            stableIdentity,
                            StringComparison.Ordinal))
                    {
                        recipe = candidate;
                        return true;
                    }
                }
            }

            recipe = null;
            return false;
        }

        public void SortByDisplayName()
        {
            recipes ??= new List<TreeGenerationRecipe>();
            recipes.Sort(CompareRecipes);
        }

        private static int CompareRecipes(
            TreeGenerationRecipe first,
            TreeGenerationRecipe second)
        {
            if (ReferenceEquals(first, second))
            {
                return 0;
            }

            if (first == null)
            {
                return 1;
            }

            if (second == null)
            {
                return -1;
            }

            return string.Compare(
                first.RecipeDisplayName,
                second.RecipeDisplayName,
                StringComparison.OrdinalIgnoreCase);
        }

        public void RegenerateStableIdentity()
        {
            stableIdentity = "tree-recipe-catalog-" +
                Guid.NewGuid().ToString("N");
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(stableIdentity))
            {
                RegenerateStableIdentity();
            }

            displayName = string.IsNullOrWhiteSpace(displayName)
                ? name
                : displayName.Trim();
            recipes ??= new List<TreeGenerationRecipe>();

            var unique = new HashSet<TreeGenerationRecipe>();
            for (int index = recipes.Count - 1; index >= 0; index--)
            {
                TreeGenerationRecipe recipe = recipes[index];
                if (recipe == null || !unique.Add(recipe))
                {
                    recipes.RemoveAt(index);
                }
            }
        }
    }
}
