using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    internal enum GroundPaintedAccentProductionValidationStatus
    {
        NotRequired = 0,
        Current = 1,
        Missing = 2,
        Stale = 3,
        Incompatible = 4,
        OwnershipMismatch = 5,
        DuplicateIdentifier = 6,
        SharedProductionAsset = 7,
        ValidationFailed = 8,
        SceneUnavailable = 9
    }

    internal readonly struct GroundPaintedAccentProductionValidationResult
    {
        public GroundPaintedAccentProductionValidationResult(
            string scenePath,
            string groundPath,
            GroundPaintedAccentProductionValidationStatus status,
            string reason,
            string assetPath,
            string identifier)
        {
            ScenePath = scenePath ?? string.Empty;
            GroundPath = groundPath ?? string.Empty;
            Status = status;
            Reason = reason ?? string.Empty;
            AssetPath = assetPath ?? string.Empty;
            Identifier = identifier ?? string.Empty;
        }

        public string ScenePath { get; }
        public string GroundPath { get; }
        public GroundPaintedAccentProductionValidationStatus Status { get; }
        public string Reason { get; }
        public string AssetPath { get; }
        public string Identifier { get; }
        public bool IsValid =>
            Status == GroundPaintedAccentProductionValidationStatus.Current ||
            Status == GroundPaintedAccentProductionValidationStatus.NotRequired;

        public GroundPaintedAccentProductionValidationResult WithFailure(
            GroundPaintedAccentProductionValidationStatus status,
            string reason)
        {
            return new GroundPaintedAccentProductionValidationResult(
                ScenePath,
                GroundPath,
                status,
                reason,
                AssetPath,
                Identifier);
        }
    }

    internal sealed class GroundPaintedAccentProductionValidationReport
    {
        private readonly List<GroundPaintedAccentProductionValidationResult>
            results =
                new List<GroundPaintedAccentProductionValidationResult>(16);

        public IReadOnlyList<GroundPaintedAccentProductionValidationResult>
            Results => results;

        public int SceneCount { get; internal set; }
        public int GroundCount
        {
            get
            {
                int count = 0;
                for (int index = 0; index < results.Count; index++)
                {
                    if (results[index].Status !=
                        GroundPaintedAccentProductionValidationStatus
                            .SceneUnavailable)
                    {
                        count++;
                    }
                }

                return count;
            }
        }
        public int RequiredCount { get; internal set; }
        public int FailureCount
        {
            get
            {
                int count = 0;
                for (int index = 0; index < results.Count; index++)
                {
                    if (!results[index].IsValid)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public bool IsValid => FailureCount == 0;

        internal void Add(
            GroundPaintedAccentProductionValidationResult result)
        {
            results.Add(result);
            if (result.Status !=
                GroundPaintedAccentProductionValidationStatus.NotRequired &&
                result.Status !=
                GroundPaintedAccentProductionValidationStatus.SceneUnavailable)
            {
                RequiredCount++;
            }
        }

        internal void Replace(
            int index,
            GroundPaintedAccentProductionValidationResult result)
        {
            results[index] = result;
        }

        public string BuildSummary()
        {
            if (IsValid)
            {
                return
                    "Painted Accent production validation passed.\n" +
                    $"Build scenes: {SceneCount}\n" +
                    $"GeneratedGround components: {GroundCount}\n" +
                    $"Production bakes required and current: {RequiredCount}\n" +
                    $"Not required: {Mathf.Max(0, GroundCount - RequiredCount)}";
            }

            StringBuilder builder = new StringBuilder(1024);
            builder.Append("Painted Accent production validation failed with ")
                .Append(FailureCount)
                .Append(" issue(s) in enabled build scenes.\n");

            for (int index = 0; index < results.Count; index++)
            {
                GroundPaintedAccentProductionValidationResult result =
                    results[index];
                if (result.IsValid)
                {
                    continue;
                }

                builder.Append("\nScene: ")
                    .Append(string.IsNullOrWhiteSpace(result.ScenePath)
                        ? "<unavailable>"
                        : result.ScenePath)
                    .Append("\nGround: ")
                    .Append(string.IsNullOrWhiteSpace(result.GroundPath)
                        ? "<scene validation>"
                        : result.GroundPath)
                    .Append("\nStatus: ")
                    .Append(FormatStatus(result.Status))
                    .Append("\nReason: ")
                    .Append(result.Reason);

                if (!string.IsNullOrWhiteSpace(result.AssetPath))
                {
                    builder.Append("\nAsset: ")
                        .Append(result.AssetPath);
                }

                builder.Append("\nAction: ")
                    .Append(ResolveAction(result.Status));
            }

            return builder.ToString();
        }

        public string SelectedGroundSummary()
        {
            if (results.Count != 1)
            {
                return BuildSummary();
            }

            GroundPaintedAccentProductionValidationResult result =
                results[0];
            StringBuilder builder = new StringBuilder(512);
            builder.Append(
                    result.IsValid
                        ? "Painted Accent production validation passed."
                        : "Painted Accent production validation failed.")
                .Append("\nScene: ")
                .Append(string.IsNullOrWhiteSpace(result.ScenePath)
                    ? "<unsaved>"
                    : result.ScenePath)
                .Append("\nGround: ")
                .Append(string.IsNullOrWhiteSpace(result.GroundPath)
                    ? "<unavailable>"
                    : result.GroundPath)
                .Append("\nStatus: ")
                .Append(FormatStatus(result.Status))
                .Append("\nReason: ")
                .Append(result.Reason);

            if (!string.IsNullOrWhiteSpace(result.AssetPath))
            {
                builder.Append("\nAsset: ")
                    .Append(result.AssetPath);
            }

            if (!result.IsValid)
            {
                builder.Append("\nAction: ")
                    .Append(ResolveAction(result.Status));
            }

            return builder.ToString();
        }

        private static string FormatStatus(
            GroundPaintedAccentProductionValidationStatus status)
        {
            return status switch
            {
                GroundPaintedAccentProductionValidationStatus.NotRequired =>
                    "Not Required",
                GroundPaintedAccentProductionValidationStatus.Current =>
                    "Current",
                GroundPaintedAccentProductionValidationStatus.Missing =>
                    "Missing",
                GroundPaintedAccentProductionValidationStatus.Stale =>
                    "Stale",
                GroundPaintedAccentProductionValidationStatus.Incompatible =>
                    "Incompatible",
                GroundPaintedAccentProductionValidationStatus
                    .OwnershipMismatch => "Ownership Mismatch",
                GroundPaintedAccentProductionValidationStatus
                    .DuplicateIdentifier => "Duplicate Identifier",
                GroundPaintedAccentProductionValidationStatus
                    .SharedProductionAsset => "Shared Production Asset",
                GroundPaintedAccentProductionValidationStatus
                    .SceneUnavailable => "Scene Unavailable",
                _ => "Validation Failed"
            };
        }

        private static string ResolveAction(
            GroundPaintedAccentProductionValidationStatus status)
        {
            return status switch
            {
                GroundPaintedAccentProductionValidationStatus.Stale =>
                    "Open the scene, select the Ground, and press Bake Painted Accents.",
                GroundPaintedAccentProductionValidationStatus.Missing =>
                    "Open the scene, select the Ground, and press Bake Painted Accents.",
                GroundPaintedAccentProductionValidationStatus
                    .DuplicateIdentifier =>
                    "Open the scene and bake each conflicting Ground once so each receives unique generated-output ownership.",
                GroundPaintedAccentProductionValidationStatus
                    .OwnershipMismatch =>
                    "Open the scene and rebake the Ground into its scene-owned generated-output folder.",
                GroundPaintedAccentProductionValidationStatus
                    .SharedProductionAsset =>
                    "Open the affected scenes and rebake each Ground so no production texture is shared.",
                GroundPaintedAccentProductionValidationStatus
                    .SceneUnavailable =>
                    "Repair or remove the invalid entry in Build Settings.",
                GroundPaintedAccentProductionValidationStatus.Incompatible =>
                    "Open the scene and rebake the Ground with the current production format.",
                _ =>
                    "Open the scene, resolve the reported Ground generation problem, and validate again."
            };
        }
    }

    internal static class GroundPaintedAccentProductionValidator
    {
        public static GroundPaintedAccentProductionValidationResult
            ValidateGround(GeneratedGround ground)
        {
            if (ground == null)
            {
                return CreateFailure(
                    string.Empty,
                    string.Empty,
                    GroundPaintedAccentProductionValidationStatus
                        .ValidationFailed,
                    "No GeneratedGround was selected.",
                    string.Empty,
                    string.Empty);
            }

            Scene scene = ground.gameObject.scene;
            List<GeneratedGround> sceneGrounds = CollectSceneGrounds(scene);
            return ValidateGroundInternal(
                ground,
                sceneGrounds,
                scene.path);
        }

        public static GroundPaintedAccentProductionValidationReport
            ValidateEnabledBuildScenes()
        {
            GroundPaintedAccentProductionValidationReport report =
                new GroundPaintedAccentProductionValidationReport();
            BuildProfile activeProfile =
                BuildProfile.GetActiveBuildProfile();
            EditorBuildSettingsScene[] buildScenes =
                activeProfile != null
                    ? activeProfile.GetScenesForBuild()
                    : EditorBuildSettings.scenes;
            buildScenes ??= Array.Empty<EditorBuildSettingsScene>();

            for (int index = 0; index < buildScenes.Length; index++)
            {
                EditorBuildSettingsScene buildScene = buildScenes[index];
                if (buildScene == null || !buildScene.enabled)
                {
                    continue;
                }

                report.SceneCount++;
                ValidateBuildScene(buildScene.path, report);
            }

            ApplyCrossSceneAssetConflictValidation(report);
            return report;
        }

        public static void ShowBuildSceneValidationDialog()
        {
            GroundPaintedAccentProductionValidationReport report =
                ValidateEnabledBuildScenes();
            string message = report.BuildSummary();
            if (report.IsValid)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogError(message);
            }

            EditorUtility.DisplayDialog(
                report.IsValid
                    ? "Painted Accent Production Valid"
                    : "Painted Accent Production Invalid",
                message,
                "OK");
        }

        public static void ShowGroundValidationDialog(
            GeneratedGround ground)
        {
            GroundPaintedAccentProductionValidationResult result =
                ValidateGround(ground);
            GroundPaintedAccentProductionValidationReport report =
                new GroundPaintedAccentProductionValidationReport
                {
                    SceneCount = 1
                };
            report.Add(result);
            EditorUtility.DisplayDialog(
                result.IsValid
                    ? "Painted Accent Production Valid"
                    : "Painted Accent Production Invalid",
                report.SelectedGroundSummary(),
                "OK");
        }

        private static void ValidateBuildScene(
            string scenePath,
            GroundPaintedAccentProductionValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(scenePath) ||
                AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                report.Add(CreateFailure(
                    scenePath,
                    string.Empty,
                    GroundPaintedAccentProductionValidationStatus
                        .SceneUnavailable,
                    "The enabled build-scene asset is missing or cannot be loaded.",
                    string.Empty,
                    string.Empty));
                return;
            }

            Scene previewScene = default;
            try
            {
                previewScene = EditorSceneManager.OpenPreviewScene(scenePath);
                if (!previewScene.IsValid() || !previewScene.isLoaded)
                {
                    report.Add(CreateFailure(
                        scenePath,
                        string.Empty,
                        GroundPaintedAccentProductionValidationStatus
                            .SceneUnavailable,
                        "Unity could not open the enabled build scene for isolated production validation.",
                        string.Empty,
                        string.Empty));
                    return;
                }

                List<GeneratedGround> grounds =
                    CollectSceneGrounds(previewScene);
                for (int groundIndex = 0;
                     groundIndex < grounds.Count;
                     groundIndex++)
                {
                    report.Add(
                        ValidateGroundInternal(
                            grounds[groundIndex],
                            grounds,
                            scenePath));
                }
            }
            catch (Exception exception)
            {
                report.Add(CreateFailure(
                    scenePath,
                    string.Empty,
                    GroundPaintedAccentProductionValidationStatus
                        .SceneUnavailable,
                    "Scene validation threw an exception: " +
                    exception.Message,
                    string.Empty,
                    string.Empty));
            }
            finally
            {
                if (previewScene.IsValid())
                {
                    EditorSceneManager.ClosePreviewScene(previewScene);
                }
            }
        }

        private static GroundPaintedAccentProductionValidationResult
            ValidateGroundInternal(
                GeneratedGround ground,
                IReadOnlyList<GeneratedGround> sceneGrounds,
                string sourceScenePath)
        {
            string scenePath = sourceScenePath ?? string.Empty;
            string groundPath = BuildHierarchyPath(
                ground != null ? ground.transform : null);
            string identifier =
                ground != null
                    ? ground.PaintedAccentProductionBakeIdentifier
                    : string.Empty;
            Texture2D texture =
                ground != null
                    ? ground.PaintedAccentProductionCoverageTexture
                    : null;
            string assetPath =
                texture != null
                    ? AssetDatabase.GetAssetPath(texture)
                    : string.Empty;

            if (ground == null)
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus
                        .ValidationFailed,
                    "The GeneratedGround reference is missing.",
                    assetPath,
                    identifier);
            }

            if (!ground.PaintedAccentProductionBakeRequired)
            {
                return new GroundPaintedAccentProductionValidationResult(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus.NotRequired,
                    "No runtime-applicable Painted Accent recipe resolves.",
                    assetPath,
                    identifier);
            }

            if (!ground.gameObject.scene.IsValid() ||
                string.IsNullOrWhiteSpace(scenePath))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus
                        .OwnershipMismatch,
                    "The Ground is not owned by a saved scene with a stable GUID.",
                    assetPath,
                    identifier);
            }

            if (!GroundPaintedAccentProductionBaker.IsValidIdentifier(
                    identifier))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    texture == null
                        ? GroundPaintedAccentProductionValidationStatus.Missing
                        : GroundPaintedAccentProductionValidationStatus
                            .Incompatible,
                    "The Ground has no valid 32-character production-output identifier.",
                    assetPath,
                    identifier);
            }

            if (HasDuplicateIdentifier(
                    ground,
                    sceneGrounds,
                    identifier))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus
                        .DuplicateIdentifier,
                    "Another GeneratedGround in this scene uses the same production-output identifier.",
                    assetPath,
                    identifier);
            }

            if (texture == null)
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus.Missing,
                    "No persistent Painted Accent production texture is assigned.",
                    assetPath,
                    identifier);
            }

            if (string.IsNullOrWhiteSpace(assetPath) ||
                AssetDatabase.LoadMainAssetAtPath(assetPath) != texture)
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus.Missing,
                    "The referenced production texture does not exist as a loadable AssetDatabase main asset.",
                    assetPath,
                    identifier);
            }

            if (!GroundPaintedAccentProductionBaker.TryGetExpectedAssetPath(
                    scenePath,
                    identifier,
                    out string expectedPath,
                    out string pathFailure))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus
                        .OwnershipMismatch,
                    pathFailure,
                    assetPath,
                    identifier);
            }

            if (!string.Equals(
                    assetPath,
                    expectedPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus
                        .OwnershipMismatch,
                    "The production texture is not stored at the scene- and Ground-owned generated-output path.",
                    assetPath,
                    identifier);
            }

            if (HasSharedProductionAsset(ground, sceneGrounds, texture))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus
                        .SharedProductionAsset,
                    "Another GeneratedGround in this scene references the same production texture.",
                    assetPath,
                    identifier);
            }

            GroundPaintedAccentProductionBakeDiagnostics diagnostics =
                ground.GetPaintedAccentProductionBakeDiagnostics();
            if (diagnostics.StoredFormatRevision !=
                    GeneratedGround
                        .CurrentPaintedAccentProductionBakeFormatRevision ||
                texture.format != TextureFormat.R8 ||
                texture.width <= 0 ||
                texture.height <= 0 ||
                !texture.isReadable ||
                diagnostics.StoredOriginSize.z <= 0.0001f ||
                diagnostics.StoredOriginSize.w <= 0.0001f ||
                string.IsNullOrWhiteSpace(
                    diagnostics.StoredCoverageSignature))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus
                        .Incompatible,
                    "The production bake revision, R8 texture contract, readability, mapping, or stored signature is incompatible.",
                    assetPath,
                    identifier);
            }

            string expectedObjectName =
                Path.GetFileNameWithoutExtension(assetPath);
            if (!string.Equals(
                    texture.name,
                    expectedObjectName,
                    StringComparison.Ordinal))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus
                        .Incompatible,
                    "The production texture main-object name does not match its asset filename.",
                    assetPath,
                    identifier);
            }

            string artifactSignature =
                GeneratedGround
                    .EditorCalculatePaintedAccentProductionCoverageSignature(
                        texture,
                        diagnostics.StoredOriginSize);
            if (string.IsNullOrWhiteSpace(artifactSignature) ||
                !string.Equals(
                    artifactSignature,
                    diagnostics.StoredCoverageSignature,
                    StringComparison.Ordinal))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus
                        .Incompatible,
                    "The persistent texture bytes or stored mapping no longer match the signature recorded when the bake was created.",
                    assetPath,
                    identifier);
            }

            if (!ground.TryPreparePaintedAccentProductionValidation(
                    out GroundPaintedAccentProductionBakeSource source,
                    out string validationFailure))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus
                        .ValidationFailed,
                    validationFailure,
                    assetPath,
                    identifier);
            }

            if (!string.Equals(
                    source.CoverageSignature,
                    diagnostics.StoredCoverageSignature,
                    StringComparison.Ordinal))
            {
                return CreateFailure(
                    scenePath,
                    groundPath,
                    GroundPaintedAccentProductionValidationStatus.Stale,
                    "Current authoritative coverage or local mapping differs from the stored production bake. Ink Colour and Ink Opacity are excluded from this comparison.",
                    assetPath,
                    identifier);
            }

            return new GroundPaintedAccentProductionValidationResult(
                scenePath,
                groundPath,
                GroundPaintedAccentProductionValidationStatus.Current,
                "The persistent production bake matches current authoritative coverage.",
                assetPath,
                identifier);
        }

        private static List<GeneratedGround> CollectSceneGrounds(Scene scene)
        {
            List<GeneratedGround> grounds =
                new List<GeneratedGround>(4);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return grounds;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                GeneratedGround[] found =
                    roots[rootIndex]
                        .GetComponentsInChildren<GeneratedGround>(true);
                for (int groundIndex = 0;
                     groundIndex < found.Length;
                     groundIndex++)
                {
                    if (found[groundIndex] != null)
                    {
                        grounds.Add(found[groundIndex]);
                    }
                }
            }

            return grounds;
        }

        private static bool HasDuplicateIdentifier(
            GeneratedGround ground,
            IReadOnlyList<GeneratedGround> sceneGrounds,
            string identifier)
        {
            for (int index = 0; index < sceneGrounds.Count; index++)
            {
                GeneratedGround candidate = sceneGrounds[index];
                if (candidate == null || candidate == ground)
                {
                    continue;
                }

                if (string.Equals(
                        candidate.PaintedAccentProductionBakeIdentifier,
                        identifier,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSharedProductionAsset(
            GeneratedGround ground,
            IReadOnlyList<GeneratedGround> sceneGrounds,
            Texture2D texture)
        {
            for (int index = 0; index < sceneGrounds.Count; index++)
            {
                GeneratedGround candidate = sceneGrounds[index];
                if (candidate == null || candidate == ground)
                {
                    continue;
                }

                if (candidate.PaintedAccentProductionCoverageTexture == texture)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyCrossSceneAssetConflictValidation(
            GroundPaintedAccentProductionValidationReport report)
        {
            Dictionary<string, int> firstOwnerByAsset =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);
            HashSet<int> conflicts = new HashSet<int>();

            for (int index = 0; index < report.Results.Count; index++)
            {
                GroundPaintedAccentProductionValidationResult result =
                    report.Results[index];
                if (result.Status ==
                        GroundPaintedAccentProductionValidationStatus
                            .NotRequired ||
                    string.IsNullOrWhiteSpace(result.AssetPath))
                {
                    continue;
                }

                if (firstOwnerByAsset.TryGetValue(
                        result.AssetPath,
                        out int firstIndex))
                {
                    GroundPaintedAccentProductionValidationResult first =
                        report.Results[firstIndex];
                    if (!string.Equals(
                            first.ScenePath,
                            result.ScenePath,
                            StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(
                            first.GroundPath,
                            result.GroundPath,
                            StringComparison.Ordinal))
                    {
                        conflicts.Add(firstIndex);
                        conflicts.Add(index);
                    }
                }
                else
                {
                    firstOwnerByAsset.Add(result.AssetPath, index);
                }
            }

            foreach (int index in conflicts)
            {
                GroundPaintedAccentProductionValidationResult result =
                    report.Results[index];
                if (result.Status !=
                    GroundPaintedAccentProductionValidationStatus.Current)
                {
                    continue;
                }

                report.Replace(
                    index,
                    result.WithFailure(
                        GroundPaintedAccentProductionValidationStatus
                            .SharedProductionAsset,
                        "The same persistent production texture is referenced by more than one Ground across enabled build scenes."));
            }
        }

        private static GroundPaintedAccentProductionValidationResult
            CreateFailure(
                string scenePath,
                string groundPath,
                GroundPaintedAccentProductionValidationStatus status,
                string reason,
                string assetPath,
                string identifier)
        {
            return new GroundPaintedAccentProductionValidationResult(
                scenePath,
                groundPath,
                status,
                reason,
                assetPath,
                identifier);
        }

        private static string BuildHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            Stack<string> names = new Stack<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }
    }
}
