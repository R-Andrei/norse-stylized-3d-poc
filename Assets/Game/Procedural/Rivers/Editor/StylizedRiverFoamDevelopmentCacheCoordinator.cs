using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    /// <summary>
    /// Editor-only orchestration for the Patch 4.9C.1 development workflow.
    /// Authored rivers receive a deterministic persistent cache asset before
    /// Play Mode starts, and completed development generations are written back
    /// only after the runtime has produced and round-trip-validated a complete
    /// payload. Player builds do not reference this coordinator.
    /// </summary>
    [InitializeOnLoad]
    internal static class StylizedRiverFoamDevelopmentCacheCoordinator
    {
        private const string GeneratedFolderName =
            "Generated/RiverFoamTopologyCaches";
        private const double PlayPollIntervalSeconds = 0.10;
        private const double EditPollIntervalSeconds = 1.0;

        internal static bool BuildPreflightInProgress { get; set; }

        private static double nextPollTime;
        private static double nextEditPollTime;
        private static readonly HashSet<string> ReportedAssignmentFailures = new();

        static StylizedRiverFoamDevelopmentCacheCoordinator()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            EditorApplication.update += Update;
            EditorApplication.delayCall += EnsureLoadedRiverCacheAssignments;
        }

        private static void HandlePlayModeStateChanged(
            PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EnsureLoadedRiverCacheAssignments();
            }
            else if (state == PlayModeStateChange.EnteredPlayMode)
            {
                nextPollTime = 0.0;
            }
        }

        private static void Update()
        {
            double now = EditorApplication.timeSinceStartup;
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                    !EditorApplication.isCompiling &&
                    !EditorApplication.isUpdating &&
                    now >= nextEditPollTime)
                {
                    nextEditPollTime = now + EditPollIntervalSeconds;
                    EnsureLoadedRiverCacheAssignments();
                }
                return;
            }

            if (EditorApplication.isPaused || now < nextPollTime)
            {
                return;
            }

            nextPollTime = now + PlayPollIntervalSeconds;
            PersistCompletedRuntimeBuilds();
        }

        private static void EnsureLoadedRiverCacheAssignments()
        {
            if (BuildPreflightInProgress)
            {
                return;
            }

            StylizedRiver[] rivers = UnityEngine.Object.FindObjectsByType<
                StylizedRiver>(FindObjectsInactive.Include);
            Array.Sort(
                rivers,
                (left, right) => string.CompareOrdinal(
                    ResolveStableObjectKey(left),
                    ResolveStableObjectKey(right)));
            Dictionary<StylizedRiverFoamTopologyCacheAsset, int> assetUsers =
                new();
            for (int index = 0; index < rivers.Length; index++)
            {
                StylizedRiver candidate = rivers[index];
                StylizedRiverFoamTopologyCacheAsset assigned = candidate != null
                    ? candidate.FoamTopologyCacheAsset
                    : null;
                if (assigned == null)
                {
                    continue;
                }

                assetUsers.TryGetValue(assigned, out int userCount);
                assetUsers[assigned] = userCount + 1;
            }

            bool assetsChanged = false;
            for (int index = 0; index < rivers.Length; index++)
            {
                StylizedRiver river = rivers[index];
                if (river == null || !river.FoamEnabled ||
                    !river.gameObject.scene.IsValid() ||
                    !river.gameObject.scene.isLoaded)
                {
                    continue;
                }

                StylizedRiverFoamTopologyCacheAsset assigned =
                    river.FoamTopologyCacheAsset;
                bool sharedAssignment = assigned != null &&
                    assetUsers.TryGetValue(assigned, out int userCount) &&
                    userCount > 1;
                bool mismatchedAutomaticAssignment = assigned != null &&
                    IsMismatchedAutomaticAssignment(river, assigned);
                if (assigned != null && !sharedAssignment &&
                    !mismatchedAutomaticAssignment)
                {
                    continue;
                }

                if (TryCreateOrFindCacheAsset(
                        river,
                        out StylizedRiverFoamTopologyCacheAsset asset,
                        out string error))
                {
                    if (asset != assigned)
                    {
                        AssignCacheAsset(river, asset);
                        assetsChanged = true;
                        Debug.Log(
                            $"[River Foam 4.9C.1] Automatically assigned " +
                            $"topology cache '{AssetDatabase.GetAssetPath(asset)}' " +
                            $"to '{river.name}'.",
                            river);
                    }

                    ReportedAssignmentFailures.Remove(
                        ResolveStableObjectKey(river));
                }
                else if (ReportedAssignmentFailures.Add(
                             ResolveStableObjectKey(river)))
                {
                    Debug.LogWarning(
                        $"[River Foam 4.9C.1] Could not create an automatic " +
                        $"topology cache for '{river.name}': {error} The " +
                        "river will still generate for this development " +
                        "session, but the result cannot persist automatically.",
                        river);
                }
            }

            if (assetsChanged)
            {
                AssetDatabase.SaveAssets();
            }
        }

        private static void PersistCompletedRuntimeBuilds()
        {
            StylizedRiverFoamRuntime[] runtimes =
                UnityEngine.Object.FindObjectsByType<
                    StylizedRiverFoamRuntime>(FindObjectsInactive.Include);
            Array.Sort(
                runtimes,
                (left, right) => string.CompareOrdinal(
                    ResolveStableObjectKey(left),
                    ResolveStableObjectKey(right)));
            bool assetsChanged = false;
            List<(StylizedRiverFoamRuntime runtime, int payloadBytes,
                string payloadHash, string assetPath)> completedWrites = new();

            for (int index = 0; index < runtimes.Length; index++)
            {
                StylizedRiverFoamRuntime runtime = runtimes[index];
                if (runtime == null ||
                    !runtime.AutomaticTopologyCacheWritePending)
                {
                    continue;
                }

                StylizedRiver river = runtime.GetComponent<StylizedRiver>();
                StylizedRiverFoamTopologyCacheAsset asset =
                    river != null ? river.FoamTopologyCacheAsset : null;
                if (asset == null)
                {
                    runtime.ReportAutomaticTopologyCachePersistenceFailure(
                        "No persistent cache asset is assigned. Exit Play Mode " +
                        "and press Play again after saving the scene, or assign " +
                        "an asset manually in Advanced Cache Diagnostics.");
                    continue;
                }

                if (!runtime.TryBuildAutomaticTopologyCache(
                        out StylizedRiverFoamTopologyCacheBuildArtifact artifact))
                {
                    continue;
                }

                if (!runtime.ValidateAutomaticTopologyCacheArtifact(
                        artifact,
                        out string validationError))
                {
                    runtime.ReportAutomaticTopologyCachePersistenceFailure(
                        validationError);
                    continue;
                }

                try
                {
                    asset.StoreBuild(artifact);
                    EditorUtility.SetDirty(asset);
                    assetsChanged = true;
                    completedWrites.Add((
                        runtime,
                        artifact.PayloadByteCount,
                        artifact.PayloadHash,
                        AssetDatabase.GetAssetPath(asset)));
                }
                catch (Exception exception)
                {
                    runtime.ReportAutomaticTopologyCachePersistenceFailure(
                        exception.Message);
                }
            }

            if (assetsChanged)
            {
                try
                {
                    AssetDatabase.SaveAssets();
                    for (int index = 0; index < completedWrites.Count; index++)
                    {
                        var completed = completedWrites[index];
                        completed.runtime.ReportAutomaticTopologyCachePersisted(
                            completed.payloadBytes,
                            completed.payloadHash,
                            completed.assetPath);
                    }
                }
                catch (Exception exception)
                {
                    for (int index = 0; index < completedWrites.Count; index++)
                    {
                        completedWrites[index].runtime
                            .ReportAutomaticTopologyCachePersistenceFailure(
                                exception.Message);
                    }
                }
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

        private static bool IsMismatchedAutomaticAssignment(
            StylizedRiver river,
            StylizedRiverFoamTopologyCacheAsset assigned)
        {
            string assignedPath = AssetDatabase.GetAssetPath(assigned)
                ?.Replace('\\', '/');
            if (string.IsNullOrEmpty(assignedPath) ||
                assignedPath.IndexOf(
                    $"/{GeneratedFolderName}/",
                    StringComparison.Ordinal) < 0 ||
                !Path.GetFileName(assignedPath).StartsWith(
                    "RiverFoamTopologyCache_",
                    StringComparison.Ordinal))
            {
                return false;
            }

            return TryResolveAutomaticCacheAssetPath(
                    river,
                    out _,
                    out string expectedPath,
                    out _) &&
                !string.Equals(
                    assignedPath,
                    expectedPath,
                    StringComparison.Ordinal);
        }

        private static bool TryResolveAutomaticCacheAssetPath(
            StylizedRiver river,
            out string folderPath,
            out string assetPath,
            out string error)
        {
            folderPath = string.Empty;
            assetPath = string.Empty;
            error = string.Empty;

            string scenePath = river != null
                ? river.gameObject.scene.path
                : string.Empty;
            if (string.IsNullOrEmpty(scenePath))
            {
                error = "The containing scene has not been saved yet.";
                return false;
            }

            if (!string.Equals(
                    Path.GetExtension(scenePath),
                    ".unity",
                    StringComparison.OrdinalIgnoreCase))
            {
                error =
                    "Automatic cache ownership currently requires a saved scene object.";
                return false;
            }

            GlobalObjectId globalObjectId =
                GlobalObjectId.GetGlobalObjectIdSlow(river);
            string globalIdText = globalObjectId.ToString();
            if (string.IsNullOrEmpty(globalIdText))
            {
                error =
                    "Unity did not provide a stable scene-object identity.";
                return false;
            }

            string sceneDirectory = Path.GetDirectoryName(scenePath)
                ?.Replace('\\', '/');
            if (string.IsNullOrEmpty(sceneDirectory) ||
                !sceneDirectory.StartsWith(
                    "Assets",
                    StringComparison.Ordinal))
            {
                error =
                    "The scene is not stored beneath the Assets folder.";
                return false;
            }

            folderPath = $"{sceneDirectory}/{GeneratedFolderName}";
            string identity = Hash128.Compute(globalIdText).ToString();
            assetPath =
                $"{folderPath}/RiverFoamTopologyCache_{identity}.asset";
            return true;
        }

        private static bool TryCreateOrFindCacheAsset(
            StylizedRiver river,
            out StylizedRiverFoamTopologyCacheAsset asset,
            out string error)
        {
            asset = null;
            if (!TryResolveAutomaticCacheAssetPath(
                    river,
                    out string folderPath,
                    out string assetPath,
                    out error))
            {
                return false;
            }

            EnsureAssetFolder(folderPath);
            asset = AssetDatabase.LoadAssetAtPath<
                StylizedRiverFoamTopologyCacheAsset>(assetPath);
            if (asset != null)
            {
                return true;
            }

            UnityEngine.Object existing =
                AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (existing != null)
            {
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            }

            asset = ScriptableObject.CreateInstance<
                StylizedRiverFoamTopologyCacheAsset>();
            AssetDatabase.CreateAsset(asset, assetPath);
            EditorUtility.SetDirty(asset);
            return true;
        }

        private static void AssignCacheAsset(
            StylizedRiver river,
            StylizedRiverFoamTopologyCacheAsset asset)
        {
            SerializedObject riverObject = new SerializedObject(river);
            SerializedProperty cacheProperty =
                riverObject.FindProperty("foamTopologyCacheAsset");
            cacheProperty.objectReferenceValue = asset;
            riverObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(river);
            EditorSceneManager.MarkSceneDirty(river.gameObject.scene);
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = $"{current}/{parts[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

    }
}
