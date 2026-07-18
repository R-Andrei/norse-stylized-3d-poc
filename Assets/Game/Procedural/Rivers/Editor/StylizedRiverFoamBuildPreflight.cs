using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    /// <summary>
    /// River Foam release gate. Non-development player builds are rejected
    /// before build work begins when any enabled authored Foam river in an
    /// enabled build scene lacks one exact, current, complete persistent cache.
    /// Validation never creates, assigns, rebuilds, or saves assets.
    /// </summary>
    public sealed class StylizedRiverFoamBuildPreflight :
        IPreprocessBuildWithReport
    {
        private const string MenuPath =
            "Tools/Programmatic Stylized 3D/Rivers/Validate Release Foam Caches";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if ((report.summary.options & BuildOptions.Development) != 0)
            {
                return;
            }

            PreflightResult result = ValidateEnabledBuildScenes();
            if (!result.Passed)
            {
                throw new BuildFailedException(result.FormatFailureMessage());
            }

            Debug.Log(result.FormatSuccessMessage());
        }

        [MenuItem(MenuPath)]
        private static void ValidateReleaseFoamCachesFromMenu()
        {
            PreflightResult result = ValidateEnabledBuildScenes();
            if (result.Passed)
            {
                Debug.Log(result.FormatSuccessMessage());
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "River Foam Release Preflight",
                        result.FormatSuccessMessage(),
                        "OK");
                }

                return;
            }

            string message = result.FormatFailureMessage();
            Debug.LogError(message);
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "River Foam Release Preflight Failed",
                    message,
                    "OK");
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool CanValidateReleaseFoamCachesFromMenu()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode &&
                !BuildPipeline.isBuildingPlayer;
        }

        private static PreflightResult ValidateEnabledBuildScenes()
        {
            List<string> failures = new();
            Scene activeSceneBeforeValidation = SceneManager.GetActiveScene();
            int sceneCount = 0;
            int riverCount = 0;
            long payloadBytes = 0L;

            try
            {
                EditorBuildSettingsScene[] buildScenes =
                    EditorBuildSettings.scenes;
                for (int index = 0; index < buildScenes.Length; index++)
                {
                    EditorBuildSettingsScene buildScene = buildScenes[index];
                    if (!buildScene.enabled ||
                        string.IsNullOrEmpty(buildScene.path))
                    {
                        continue;
                    }

                    sceneCount++;
                    Scene scene = SceneManager.GetSceneByPath(buildScene.path);
                    bool openedForValidation = false;
                    if (!scene.IsValid() || !scene.isLoaded)
                    {
                        try
                        {
                            scene = EditorSceneManager.OpenScene(
                                buildScene.path,
                                OpenSceneMode.Additive);
                            openedForValidation = true;
                        }
                        catch (Exception exception)
                        {
                            failures.Add(
                                $"{buildScene.path}: scene could not be opened " +
                                $"for cache validation ({exception.Message}).");
                            continue;
                        }
                    }

                    try
                    {
                        ValidateScene(
                            scene,
                            failures,
                            ref riverCount,
                            ref payloadBytes);
                    }
                    finally
                    {
                        if (openedForValidation &&
                            scene.IsValid() &&
                            scene.isLoaded)
                        {
                            EditorSceneManager.CloseScene(scene, true);
                        }
                    }
                }
            }
            finally
            {
                if (activeSceneBeforeValidation.IsValid() &&
                    activeSceneBeforeValidation.isLoaded)
                {
                    SceneManager.SetActiveScene(activeSceneBeforeValidation);
                }

            }

            if (sceneCount == 0)
            {
                failures.Add(
                    "No enabled scenes exist in Build Settings, so release " +
                    "Foam caches could not be validated.");
            }

            return new PreflightResult(
                sceneCount,
                riverCount,
                payloadBytes,
                failures);
        }

        private static void ValidateScene(
            Scene scene,
            List<string> failures,
            ref int riverCount,
            ref long payloadBytes)
        {
            StylizedRiver[] rivers = UnityEngine.Object.FindObjectsByType<
                StylizedRiver>(FindObjectsInactive.Include);
            Array.Sort(
                rivers,
                (left, right) => string.CompareOrdinal(
                    ResolveStableObjectKey(left),
                    ResolveStableObjectKey(right)));

            for (int index = 0; index < rivers.Length; index++)
            {
                StylizedRiver river = rivers[index];
                if (river == null ||
                    river.gameObject.scene.handle != scene.handle ||
                    !river.enabled ||
                    !river.FoamEnabled)
                {
                    continue;
                }

                riverCount++;
                string riverPath = ResolveHierarchyPath(river.transform);
                string prefix =
                    $"{scene.path} :: {riverPath}";

                if (!river.gameObject.activeInHierarchy)
                {
                    failures.Add(
                        $"{prefix}: river is inactive, so its current domain " +
                        "and exact obstacle-source contract cannot be proven. " +
                        "Temporarily activate it and prepare its cache, or " +
                        "disable Foam/the component if it is not a release river.");
                    continue;
                }

                StylizedRiverFoamRuntime runtime =
                    river.GetComponent<StylizedRiverFoamRuntime>();
                if (runtime == null)
                {
                    failures.Add(
                        $"{prefix}: the hidden Foam runtime is unavailable.");
                    continue;
                }

                if (!runtime.TryValidateAssignedTopologyCacheForRelease(
                        out string state,
                        out string summary,
                        out int riverPayloadBytes,
                        out string riverPayloadHash,
                        out int obstacleSourceCount))
                {
                    failures.Add(
                        $"{prefix}: {state}. {summary}");
                    continue;
                }

                payloadBytes += riverPayloadBytes;
                Debug.Log(
                    $"[River Foam 4.9D] Release cache valid: {prefix}; " +
                    $"{riverPayloadBytes:N0} bytes, hash {riverPayloadHash}, " +
                    $"descriptor-v{runtime.FoamGridDescriptorContractVersion}/" +
                    $"{runtime.FoamGridMapping}-v" +
                    $"{runtime.FoamGridMappingContractVersion}, " +
                    $"{obstacleSourceCount} exact obstacle source(s).",
                    river);
            }
        }

        private static string ResolveStableObjectKey(Component component)
        {
            if (component == null)
            {
                return string.Empty;
            }

            string globalId = GlobalObjectId
                .GetGlobalObjectIdSlow(component)
                .ToString();
            return !string.IsNullOrEmpty(globalId)
                ? globalId
                : component.GetEntityId().ToString();
        }

        private static string ResolveHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "<missing river>";
            }

            List<string> names = new();
            Transform current = transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }

        private readonly struct PreflightResult
        {
            public PreflightResult(
                int sceneCount,
                int riverCount,
                long payloadBytes,
                List<string> failures)
            {
                SceneCount = sceneCount;
                RiverCount = riverCount;
                PayloadBytes = payloadBytes;
                Failures = failures ?? new List<string>();
            }

            public int SceneCount { get; }
            public int RiverCount { get; }
            public long PayloadBytes { get; }
            public List<string> Failures { get; }
            public bool Passed => Failures.Count == 0;

            public string FormatSuccessMessage()
            {
                return
                    $"[River Foam 4.9D] Release preflight passed: " +
                    $"{RiverCount} Foam river(s) across {SceneCount} enabled " +
                    $"build scene(s), {PayloadBytes / 1024f:0.0} KiB total " +
                    "persistent topology payload.";
            }

            public string FormatFailureMessage()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(
                    "River Foam release-cache preflight failed. Release " +
                    "builds remain cache-only and will not generate topology " +
                    "at runtime.");
                for (int index = 0; index < Failures.Count; index++)
                {
                    builder.Append("  ")
                        .Append(index + 1)
                        .Append(". ")
                        .AppendLine(Failures[index]);
                }

                builder.Append(
                    "Use Prepare / Rebuild Foam Topology Cache in Edit Mode " +
                    "for each failed river, then run the preflight again.");
                return builder.ToString();
            }
        }
    }
}
