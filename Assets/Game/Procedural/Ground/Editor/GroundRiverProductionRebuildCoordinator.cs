using System;
using System.Collections.Generic;
using System.Text;
using ProgrammaticStylized3D.Rivers;
using ProgrammaticStylized3D.Rivers.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    /// <summary>
    /// Explicit, synchronous Edit Mode transaction for rebuilding one Ground,
    /// its active child Rivers, and their assigned production caches.
    /// </summary>
    internal static class GroundRiverProductionRebuildCoordinator
    {
        internal static bool TryRebuild(
            GeneratedGround ground,
            out string report)
        {
            report = string.Empty;
            if (!TryValidateContext(ground, out string failureReason))
            {
                report = failureReason;
                return false;
            }

            List<StylizedRiver> rivers = CollectActiveRivers(ground);
            StringBuilder details = new StringBuilder(2048);
            int foamStoredCount = 0;
            int foamSkippedCount = 0;
            int foamFailedCount = 0;
            int completedSteps = 0;
            int totalSteps = Mathf.Max(1, rivers.Count * 2 + 3);

            try
            {
                Undo.RecordObject(
                    ground,
                    "Rebuild Ground, Rivers & Production Caches");
                for (int index = 0; index < rivers.Count; index++)
                {
                    Undo.RecordObject(
                        rivers[index],
                        "Rebuild Ground, Rivers & Production Caches");
                }

                for (int index = 0; index < rivers.Count; index++)
                {
                    StylizedRiver river = rivers[index];
                    ShowProgress(
                        ground,
                        $"Rebuilding River {index + 1} of {rivers.Count}",
                        completedSteps++,
                        totalSteps);
                    river.RebuildSurfaceOnly();
                    EditorUtility.SetDirty(river);
                }

                ShowProgress(
                    ground,
                    "Refreshing Ground links",
                    completedSteps++,
                    totalSteps);
                ground.RefreshModifiers();
                ground.EditorForceNextStructuralRegeneration();

                string paintedAccentSummary;
                if (ground.PaintedAccentProductionBakeRequired)
                {
                    ShowProgress(
                        ground,
                        "Rebuilding Ground and Painted Accent production output",
                        completedSteps++,
                        totalSteps);
                    if (!GroundPaintedAccentProductionBaker.Bake(
                            ground,
                            out paintedAccentSummary))
                    {
                        report =
                            "Complete Ground/River rebuild failed while baking " +
                            "Painted Accent production output.\n\n" +
                            paintedAccentSummary;
                        return false;
                    }
                }
                else
                {
                    ShowProgress(
                        ground,
                        "Rebuilding Ground",
                        completedSteps++,
                        totalSteps);
                    ground.Regenerate();
                    paintedAccentSummary = "Not required by the current Ground recipe.";
                }
                EditorUtility.SetDirty(ground);

                ShowProgress(
                    ground,
                    "Validating Painted Accent production output",
                    completedSteps++,
                    totalSteps);
                GroundPaintedAccentProductionValidationResult
                    paintedAccentValidation =
                        GroundPaintedAccentProductionValidator.ValidateGround(
                            ground);
                if (!paintedAccentValidation.IsValid)
                {
                    report =
                        "Complete Ground/River rebuild failed Painted Accent " +
                        "production validation.\n\n" +
                        $"Status: {paintedAccentValidation.Status}\n" +
                        $"Reason: {paintedAccentValidation.Reason}\n" +
                        $"Asset: {paintedAccentValidation.AssetPath}";
                    return false;
                }

                details.AppendLine("River Foam cache results:");
                for (int index = 0; index < rivers.Count; index++)
                {
                    StylizedRiver river = rivers[index];
                    string riverPath = BuildHierarchyPath(
                        ground.transform,
                        river.transform);

                    ShowProgress(
                        ground,
                        $"Updating River Foam cache {index + 1} of {rivers.Count}",
                        completedSteps++,
                        totalSteps);

                    if (river.FoamTopologyCacheAsset == null)
                    {
                        foamSkippedCount++;
                        details.Append("  SKIPPED ")
                            .Append(riverPath)
                            .AppendLine(": no Foam topology cache asset is assigned.");
                        continue;
                    }

                    StylizedRiverFoamRuntime runtime =
                        river.GetComponent<StylizedRiverFoamRuntime>();
                    if (runtime == null)
                    {
                        foamFailedCount++;
                        details.Append("  FAILED ")
                            .Append(riverPath)
                            .AppendLine(": the assigned cache has no StylizedRiverFoamRuntime.");
                        continue;
                    }

                    bool prepared =
                        StylizedRiverFoamDevelopmentCacheCoordinator
                            .TryPrepareAndPersist(
                                river,
                                runtime,
                                out bool validationPassed,
                                out string cacheMessage);
                    if (!prepared || !validationPassed)
                    {
                        foamFailedCount++;
                        details.Append("  FAILED ")
                            .Append(riverPath)
                            .Append(": ")
                            .AppendLine(cacheMessage);
                        continue;
                    }

                    foamStoredCount++;
                    details.Append("  STORED ")
                        .Append(riverPath)
                        .Append(": ")
                        .AppendLine(cacheMessage);
                }

                bool succeeded = foamFailedCount == 0;
                StringBuilder summary = new StringBuilder(3072);
                summary.Append(
                        succeeded
                            ? "Complete Ground/River rebuild succeeded."
                            : "Ground and River geometry rebuilt, but one or more Foam caches failed.")
                    .AppendLine()
                    .Append("Ground: ")
                    .AppendLine(
                        BuildHierarchyPath(
                            ground.transform.root,
                            ground.transform))
                    .Append("Active Rivers rebuilt: ")
                    .AppendLine(rivers.Count.ToString())
                    .Append("Painted Accent production: ")
                    .AppendLine(paintedAccentValidation.Status.ToString())
                    .Append("Painted Accent result: ")
                    .AppendLine(paintedAccentSummary)
                    .Append("Foam caches stored: ")
                    .AppendLine(foamStoredCount.ToString())
                    .Append("Foam caches skipped: ")
                    .AppendLine(foamSkippedCount.ToString())
                    .Append("Foam caches failed: ")
                    .AppendLine(foamFailedCount.ToString())
                    .AppendLine()
                    .Append(details)
                    .AppendLine()
                    .Append(
                        "The scene was not saved automatically. Save it after " +
                        "reviewing the regenerated output.");
                report = summary.ToString();
                return succeeded;
            }
            catch (Exception exception)
            {
                report =
                    "Complete Ground/River rebuild stopped because an exception " +
                    "was thrown.\n\n" +
                    exception;
                return false;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                SceneView.RepaintAll();
            }
        }

        private static bool TryValidateContext(
            GeneratedGround ground,
            out string failureReason)
        {
            failureReason = string.Empty;
            if (ground == null)
            {
                failureReason = "Select one GeneratedGround scene instance.";
                return false;
            }

            if (Application.isPlaying ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                failureReason =
                    "Complete Ground/River rebuilding is available only in Edit Mode.";
                return false;
            }

            if (EditorUtility.IsPersistent(ground) ||
                PrefabUtility.IsPartOfPrefabAsset(ground) ||
                PrefabStageUtility.GetPrefabStage(ground.gameObject) != null)
            {
                failureReason =
                    "Select a GeneratedGround scene instance, not a prefab or persistent asset.";
                return false;
            }

            Scene scene = ground.gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded)
            {
                failureReason =
                    "The selected GeneratedGround must belong to a loaded scene.";
                return false;
            }

            if (ground.PaintedAccentProductionBakeRequired &&
                string.IsNullOrWhiteSpace(scene.path))
            {
                failureReason =
                    "Save the scene before rebuilding. Painted Accent production " +
                    "output requires a stable scene-owned generated-output path.";
                return false;
            }

            return true;
        }

        private static List<StylizedRiver> CollectActiveRivers(
            GeneratedGround ground)
        {
            StylizedRiver[] candidates =
                ground.GetComponentsInChildren<StylizedRiver>(true);
            List<StylizedRiver> rivers =
                new List<StylizedRiver>(candidates.Length);
            for (int index = 0; index < candidates.Length; index++)
            {
                StylizedRiver river = candidates[index];
                if (river != null && river.isActiveAndEnabled)
                {
                    rivers.Add(river);
                }
            }

            rivers.Sort(
                (left, right) => string.CompareOrdinal(
                    BuildHierarchyPath(ground.transform, left.transform),
                    BuildHierarchyPath(ground.transform, right.transform)));
            return rivers;
        }

        private static string BuildHierarchyPath(
            Transform root,
            Transform target)
        {
            if (target == null)
            {
                return "<missing>";
            }

            List<string> segments = new List<string>(8);
            Transform current = target;
            while (current != null)
            {
                segments.Add(
                    $"{current.name}[{current.GetSiblingIndex()}]");
                if (current == root)
                {
                    break;
                }

                current = current.parent;
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        private static void ShowProgress(
            GeneratedGround ground,
            string message,
            int completedSteps,
            int totalSteps)
        {
            EditorUtility.DisplayProgressBar(
                "Rebuild Ground, Rivers & Production Caches",
                $"{ground.name}: {message}",
                Mathf.Clamp01((float)completedSteps / totalSteps));
        }
    }
}
