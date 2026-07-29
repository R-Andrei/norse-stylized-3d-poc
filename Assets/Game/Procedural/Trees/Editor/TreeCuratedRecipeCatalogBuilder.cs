using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeCuratedRecipeCatalogBuilder
    {
        internal const string ReportRelativePath =
            "Library/PS3D/Trees/CuratedRecipes/TreeCuratedRecipeCatalogReport.txt";

        private const string CommonPineMaterialPath =
            "Assets/Game/Demo/Materials/Trees/MAT_TreeBark_CommonPine.mat";
        private const string TwistedMaterialPath =
            "Assets/Game/Demo/Materials/Trees/MAT_TreeBark_Twisted.mat";
        private const string DeadMaterialPath =
            "Assets/Game/Demo/Materials/Trees/MAT_TreeBark_Dead.mat";

        private static string latestSummary =
            "No curated recipe catalog operation has run in this Editor session.";

        internal static string LatestSummary => latestSummary;

        internal static void CreateMissing(TreeRecipeCatalog catalog)
        {
            Execute(catalog, resetExisting: false);
        }

        internal static void ResetAllToApprovedBaseline(
            TreeRecipeCatalog catalog)
        {
            Execute(catalog, resetExisting: true);
        }

        internal static void Validate(TreeRecipeCatalog catalog)
        {
            if (!TryResolveCatalogFolder(
                catalog,
                out string catalogPath,
                out string outputFolder,
                out string failure))
            {
                latestSummary = failure;
                WriteReport(BuildFailureReport(
                    "Validate Initial Curated Recipes",
                    catalogPath,
                    outputFolder,
                    failure));
                return;
            }

            string report = BuildValidationReport(
                "Validate Initial Curated Recipes",
                catalog,
                catalogPath,
                outputFolder,
                created: 0,
                preserved: 0,
                reset: 0,
                registrationChanges: 0,
                operationFailures: Array.Empty<string>());
            WriteReport(report);
        }

        internal static void CopyLatestReport()
        {
            string absolutePath = GetAbsoluteReportPath();
            if (!File.Exists(absolutePath))
            {
                EditorGUIUtility.systemCopyBuffer =
                    "No TREE-CONTROLS.2 curated recipe report exists yet.";
                return;
            }

            EditorGUIUtility.systemCopyBuffer =
                File.ReadAllText(absolutePath);
        }

        internal static void OpenReportFolder()
        {
            string absolutePath = GetAbsoluteReportPath();
            string directory = Path.GetDirectoryName(absolutePath);
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            Directory.CreateDirectory(directory);
            EditorUtility.RevealInFinder(directory);
        }

        private static void Execute(
            TreeRecipeCatalog catalog,
            bool resetExisting)
        {
            string operation = resetExisting
                ? "Reset Initial Curated Recipes To Approved Baseline"
                : "Create Missing Initial Curated Recipes";

            if (!TryResolveCatalogFolder(
                catalog,
                out string catalogPath,
                out string outputFolder,
                out string failure))
            {
                latestSummary = failure;
                WriteReport(BuildFailureReport(
                    operation,
                    catalogPath,
                    outputFolder,
                    failure));
                return;
            }

            EnsureAssetFolder(outputFolder);

            int created = 0;
            int preserved = 0;
            int reset = 0;
            int registrationChanges = 0;
            var failures = new List<string>();

            IReadOnlyList<TreeCuratedRecipeDefinition> definitions =
                TreeCuratedRecipeDefinitions.All;
            for (int index = 0; index < definitions.Count; index++)
            {
                TreeCuratedRecipeDefinition definition = definitions[index];
                string expectedPath = outputFolder + "/" +
                    definition.AssetFileName + ".asset";

                TreeGenerationRecipe recipe = FindExistingRecipe(
                    catalog,
                    definition,
                    expectedPath);
                if (recipe == null)
                {
                    if (AssetDatabase.LoadMainAssetAtPath(expectedPath) != null)
                    {
                        failures.Add(
                            definition.DisplayName +
                            ": expected asset path is occupied by another asset: " +
                            expectedPath);
                        continue;
                    }

                    recipe =
                        ScriptableObject.CreateInstance<TreeGenerationRecipe>();
                    recipe.name = definition.AssetFileName;
                    recipe.ConfigureCuratedDefinition(
                        definition,
                        LoadBarkMaterial(definition.BarkMaterialKind));
                    AssetDatabase.CreateAsset(recipe, expectedPath);
                    EditorUtility.SetDirty(recipe);
                    created++;
                }
                else if (resetExisting)
                {
                    Undo.RecordObject(
                        recipe,
                        "Reset Curated Tree Recipe");
                    recipe.ConfigureCuratedDefinition(
                        definition,
                        LoadBarkMaterial(definition.BarkMaterialKind));
                    EditorUtility.SetDirty(recipe);
                    reset++;
                }
                else
                {
                    preserved++;
                }

                Undo.RecordObject(
                    catalog,
                    "Register Curated Tree Recipe");
                if (catalog.Register(recipe))
                {
                    registrationChanges++;
                    EditorUtility.SetDirty(catalog);
                }
            }

            Undo.RecordObject(
                catalog,
                "Sort Curated Tree Recipes");
            catalog.SortByDisplayName();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string report = BuildValidationReport(
                operation,
                catalog,
                catalogPath,
                outputFolder,
                created,
                preserved,
                reset,
                registrationChanges,
                failures);
            WriteReport(report);
        }

        private static TreeGenerationRecipe FindExistingRecipe(
            TreeRecipeCatalog catalog,
            TreeCuratedRecipeDefinition definition,
            string expectedPath)
        {
            if (catalog.TryFindByStableIdentity(
                definition.StableIdentity,
                out TreeGenerationRecipe catalogRecipe))
            {
                return catalogRecipe;
            }

            TreeGenerationRecipe pathRecipe =
                AssetDatabase.LoadAssetAtPath<TreeGenerationRecipe>(
                    expectedPath);
            if (pathRecipe != null)
            {
                return string.Equals(
                    pathRecipe.StableIdentity,
                    definition.StableIdentity,
                    StringComparison.Ordinal)
                        ? pathRecipe
                        : null;
            }

            string[] recipeGuids = AssetDatabase.FindAssets(
                "t:TreeGenerationRecipe");
            for (int index = 0; index < recipeGuids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(
                    recipeGuids[index]);
                TreeGenerationRecipe candidate =
                    AssetDatabase.LoadAssetAtPath<TreeGenerationRecipe>(
                        path);
                if (candidate != null &&
                    string.Equals(
                        candidate.StableIdentity,
                        definition.StableIdentity,
                        StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string BuildValidationReport(
            string operation,
            TreeRecipeCatalog catalog,
            string catalogPath,
            string outputFolder,
            int created,
            int preserved,
            int reset,
            int registrationChanges,
            IReadOnlyList<string> operationFailures)
        {
            var report = new StringBuilder(32768);
            report.AppendLine(
                "[TREE-CONTROLS.2 Curated Recipe Catalog]");
            report.AppendLine("UTC: " +
                DateTime.UtcNow.ToString("O"));
            report.AppendLine("Operation: " + operation);
            report.AppendLine("Catalog: " + catalogPath);
            report.AppendLine("Curated asset folder: " + outputFolder);
            report.AppendLine(
                "Imported reference policy: visual references only; no one-recipe-per-reference conversion");
            report.AppendLine(
                "Live generator policy: curated recipes drive exact recipe-only generation; legacy resolution is compatibility evidence only");
            report.AppendLine();

            report.AppendLine("[Operation]");
            report.AppendLine("Created: " + created);
            report.AppendLine("Preserved existing: " + preserved);
            report.AppendLine("Explicitly reset: " + reset);
            report.AppendLine(
                "Catalog registrations added: " + registrationChanges);
            report.AppendLine(
                "Operation failures: " + operationFailures.Count);
            for (int index = 0;
                index < operationFailures.Count;
                index++)
            {
                report.AppendLine(
                    "FAIL | " + operationFailures[index]);
            }
            report.AppendLine();

            int found = 0;
            int registered = 0;
            int initialized = 0;
            int validFoundation = 0;
            int controlsMatched = 0;
            int baselineDeviations = 0;
            int missingMaterials = 0;

            report.AppendLine("[Curated Recipes]");
            IReadOnlyList<TreeCuratedRecipeDefinition> definitions =
                TreeCuratedRecipeDefinitions.All;
            for (int index = 0; index < definitions.Count; index++)
            {
                TreeCuratedRecipeDefinition definition = definitions[index];
                if (!catalog.TryFindByStableIdentity(
                    definition.StableIdentity,
                    out TreeGenerationRecipe recipe))
                {
                    report.AppendLine(
                        "FAIL | " + definition.DisplayName +
                        " | missing from catalog");
                    continue;
                }

                found++;
                registered++;
                string path = AssetDatabase.GetAssetPath(recipe);
                var foundationFailures = new List<string>();
                bool foundationPass =
                    recipe.ValidateRecipeOnlyFoundation(
                        foundationFailures);
                if (foundationPass)
                {
                    validFoundation++;
                }

                TreeRecipeControlRanges expected =
                    definition.CreateControlRanges();
                var mismatches = new List<string>();
                int matched = recipe.ControlRanges != null
                    ? recipe.ControlRanges.CountMatchingControls(
                        expected,
                        mismatches)
                    : 0;
                controlsMatched += matched;
                bool baselineMatch =
                    matched ==
                    TreeCuratedRecipeDefinitions.ExpectedControlCount;
                if (!baselineMatch)
                {
                    baselineDeviations++;
                }

                if (recipe.ControlRanges != null &&
                    recipe.ControlRanges.IsInitialized)
                {
                    initialized++;
                }

                if (recipe.BarkMaterial == null)
                {
                    missingMaterials++;
                }

                string status =
                    foundationPass &&
                    recipe.ControlRanges != null &&
                    recipe.ControlRanges.IsInitialized
                        ? "PASS"
                        : "FAIL";
                report.Append(status);
                report.Append(" | ");
                report.Append(definition.DisplayName);
                report.Append(" | path=");
                report.Append(path);
                report.Append(" | controls=");
                report.Append(matched);
                report.Append("/");
                report.Append(
                    TreeCuratedRecipeDefinitions.ExpectedControlCount);
                report.Append(" | baseline=");
                report.Append(
                    baselineMatch
                        ? "MATCH"
                        : "AUTHOR-MODIFIED");
                report.Append(" | material=");
                report.Append(
                    recipe.BarkMaterial != null
                        ? recipe.BarkMaterial.name
                        : "MISSING");
                report.AppendLine();

                if (!foundationPass)
                {
                    for (int failureIndex = 0;
                        failureIndex < foundationFailures.Count;
                        failureIndex++)
                    {
                        report.AppendLine(
                            "  FOUNDATION FAIL | " +
                            foundationFailures[failureIndex]);
                    }
                }

                if (!baselineMatch)
                {
                    report.AppendLine(
                        "  NOTE | Approved initial baseline deviations: " +
                        string.Join(", ", mismatches));
                }
            }

            int curatedInCatalog = 0;
            int extraRecipes = 0;
            for (int index = 0;
                index < catalog.Recipes.Count;
                index++)
            {
                TreeGenerationRecipe recipe = catalog.Recipes[index];
                if (recipe != null &&
                    TreeCuratedRecipeDefinitions.TryFindByStableIdentity(
                        recipe.StableIdentity,
                        out _))
                {
                    curatedInCatalog++;
                }
                else if (recipe != null)
                {
                    extraRecipes++;
                }
            }

            report.AppendLine();
            report.AppendLine("[Summary]");
            report.AppendLine(
                "Approved curated definitions: " +
                TreeCuratedRecipeDefinitions.ExpectedRecipeCount);
            report.AppendLine(
                "Curated assets found/registered: " +
                found + "/" +
                TreeCuratedRecipeDefinitions.ExpectedRecipeCount);
            report.AppendLine(
                "Initialized range sets: " +
                initialized + "/" +
                TreeCuratedRecipeDefinitions.ExpectedRecipeCount);
            report.AppendLine(
                "Valid recipe foundations: " +
                validFoundation + "/" +
                TreeCuratedRecipeDefinitions.ExpectedRecipeCount);
            report.AppendLine(
                "Control values matching approved initial baseline: " +
                controlsMatched + "/" +
                (TreeCuratedRecipeDefinitions.ExpectedRecipeCount *
                 TreeCuratedRecipeDefinitions.ExpectedControlCount));
            report.AppendLine(
                "Author-modified curated recipes: " +
                baselineDeviations);
            report.AppendLine(
                "Curated catalog entries: " + curatedInCatalog);
            report.AppendLine(
                "Extra user-authored catalog entries preserved: " +
                extraRecipes);
            report.AppendLine(
                "Missing bark material bindings: " +
                missingMaterials);
            report.AppendLine(
                "Operation failures: " +
                operationFailures.Count);

            bool pass =
                found ==
                    TreeCuratedRecipeDefinitions.ExpectedRecipeCount &&
                registered ==
                    TreeCuratedRecipeDefinitions.ExpectedRecipeCount &&
                initialized ==
                    TreeCuratedRecipeDefinitions.ExpectedRecipeCount &&
                validFoundation ==
                    TreeCuratedRecipeDefinitions.ExpectedRecipeCount &&
                missingMaterials == 0 &&
                operationFailures.Count == 0;
            report.AppendLine("Status: " + (pass ? "PASS" : "FAIL"));
            report.AppendLine(
                "Baseline deviations are informational after manual authoring and do not fail catalog validity.");

            latestSummary =
                (pass ? "PASS" : "FAIL") +
                " — curated recipes " + found + "/" +
                TreeCuratedRecipeDefinitions.ExpectedRecipeCount +
                ", author-modified " + baselineDeviations +
                ", extras preserved " + extraRecipes;
            return report.ToString();
        }

        private static string BuildFailureReport(
            string operation,
            string catalogPath,
            string outputFolder,
            string failure)
        {
            var report = new StringBuilder();
            report.AppendLine(
                "[TREE-CONTROLS.2 Curated Recipe Catalog]");
            report.AppendLine("UTC: " +
                DateTime.UtcNow.ToString("O"));
            report.AppendLine("Operation: " + operation);
            report.AppendLine(
                "Catalog: " + (catalogPath ?? string.Empty));
            report.AppendLine(
                "Curated asset folder: " +
                (outputFolder ?? string.Empty));
            report.AppendLine("FAIL | " + failure);
            report.AppendLine("Status: FAIL");
            return report.ToString();
        }

        private static bool TryResolveCatalogFolder(
            TreeRecipeCatalog catalog,
            out string catalogPath,
            out string outputFolder,
            out string failure)
        {
            catalogPath = catalog != null
                ? AssetDatabase.GetAssetPath(catalog)
                : string.Empty;
            outputFolder = string.Empty;
            if (catalog == null)
            {
                failure = "No TreeRecipeCatalog was supplied.";
                return false;
            }

            if (string.IsNullOrEmpty(catalogPath))
            {
                failure =
                    "The selected TreeRecipeCatalog is not a saved project asset.";
                return false;
            }

            string directory = Path.GetDirectoryName(catalogPath);
            if (string.IsNullOrEmpty(directory))
            {
                failure =
                    "Could not resolve the selected catalog asset folder.";
                return false;
            }

            outputFolder = directory.Replace('\\', '/') +
                "/Recipes/Curated";
            failure = string.Empty;
            return true;
        }

        private static void EnsureAssetFolder(string path)
        {
            string normalized = path.Replace('\\', '/');
            string[] parts = normalized.Split('/');
            if (parts.Length == 0 ||
                !string.Equals(
                    parts[0],
                    "Assets",
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Curated recipe output must be under Assets: " +
                    normalized);
            }

            string current = "Assets";
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(
                        current,
                        parts[index]);
                }

                current = next;
            }
        }

        private static Material LoadBarkMaterial(
            TreeCuratedBarkMaterialKind kind)
        {
            string path = kind switch
            {
                TreeCuratedBarkMaterialKind.CommonPine =>
                    CommonPineMaterialPath,
                TreeCuratedBarkMaterialKind.Twisted =>
                    TwistedMaterialPath,
                TreeCuratedBarkMaterialKind.Dead =>
                    DeadMaterialPath,
                _ => string.Empty
            };

            return string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        private static void WriteReport(string report)
        {
            string absolutePath = GetAbsoluteReportPath();
            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(absolutePath, report);
            Debug.Log(report);
        }

        private static string GetAbsoluteReportPath()
        {
            string projectRoot = Path.GetDirectoryName(
                Application.dataPath);
            return Path.Combine(
                projectRoot ?? string.Empty,
                ReportRelativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
