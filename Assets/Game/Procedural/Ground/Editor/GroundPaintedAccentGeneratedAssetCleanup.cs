using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    internal static class GroundPaintedAccentGeneratedAssetCleanup
    {
        private sealed class GroundClaim
        {
            public string ScenePath;
            public string GroundPath;
            public string ActualAssetPath;
            public string ExpectedAssetPath;
            public bool Required;
            public bool OwnershipMatches;

            public string Description
            {
                get
                {
                    string scene = string.IsNullOrWhiteSpace(ScenePath)
                        ? "<unsaved scene>"
                        : ScenePath;
                    return
                        $"{scene} :: {GroundPath} " +
                        $"(required: {(Required ? "yes" : "no")}, " +
                        $"ownership: {(OwnershipMatches ? "current" : "mismatch")})";
                }
            }
        }

        private static GroundPaintedAccentGeneratedAssetAuditReport
            lastReport;

        public static GroundPaintedAccentGeneratedAssetAuditReport
            LastReport => lastReport;
        public static bool HasLastReport => lastReport != null;

        public static GroundPaintedAccentGeneratedAssetAuditReport RunAudit()
        {
            GroundPaintedAccentGeneratedAssetAuditReport report =
                new GroundPaintedAccentGeneratedAssetAuditReport();
            lastReport = report;

            try
            {
                List<string> generatedAssetPaths =
                    CollectGeneratedAssetPaths();
                HashSet<string> generatedAssetSet =
                    new HashSet<string>(
                        generatedAssetPaths,
                        StringComparer.OrdinalIgnoreCase);
                if (generatedAssetPaths.Count == 0)
                {
                    report.Completed = true;
                    return report;
                }

                Dictionary<string, List<GroundClaim>> claimsByAsset =
                    CreateClaimMap(generatedAssetPaths.Count);

                if (!CollectGroundClaims(
                        generatedAssetSet,
                        claimsByAsset,
                        report))
                {
                    report.Cancelled = true;
                    return report;
                }

                Dictionary<string, List<string>> referencesByAsset =
                    CreateReferenceMap(generatedAssetPaths.Count);
                if (!CollectProjectReferences(
                        generatedAssetSet,
                        referencesByAsset,
                        report))
                {
                    report.Cancelled = true;
                    return report;
                }

                AddDeletionBlockers(report);
                ClassifyAssets(
                    generatedAssetPaths,
                    claimsByAsset,
                    referencesByAsset,
                    report);
                report.Completed = report.AuditFailures.Count == 0;
                return report;
            }
            catch (Exception exception)
            {
                report.AddFailure(
                    "Generated asset audit threw an exception: " +
                    exception.Message);
                return report;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static void CopyLastReport()
        {
            if (lastReport == null)
            {
                return;
            }

            EditorGUIUtility.systemCopyBuffer = lastReport.BuildReport();
        }

        public static bool TryDeleteConfirmedOrphans(
            GroundPaintedAccentGeneratedAssetAuditReport expectedReport,
            out GroundPaintedAccentGeneratedAssetAuditReport refreshedReport,
            out string resultMessage)
        {
            refreshedReport = RunAudit();
            resultMessage = string.Empty;
            if (refreshedReport == null || !refreshedReport.Completed)
            {
                resultMessage =
                    "The fresh generated-asset audit did not complete. Nothing was deleted.";
                return false;
            }

            if (refreshedReport.DeletionBlockers.Count > 0)
            {
                resultMessage =
                    "Deletion is blocked until loaded scene and asset changes are saved or reverted. Nothing was deleted.";
                return false;
            }

            List<string> currentPaths =
                refreshedReport.GetConfirmedOrphanPaths();
            List<string> expectedPaths =
                expectedReport != null
                    ? expectedReport.GetConfirmedOrphanPaths()
                    : new List<string>();
            if (!PathListsMatch(expectedPaths, currentPaths))
            {
                resultMessage =
                    "The confirmed-orphan set changed during the safety re-audit. Review the refreshed report before deleting anything.";
                return false;
            }

            if (currentPaths.Count == 0)
            {
                resultMessage = "No confirmed orphan assets remain.";
                return true;
            }

            for (int index = 0; index < currentPaths.Count; index++)
            {
                string assetPath = currentPaths[index];
                if (!IsPathUnderGeneratedRoot(assetPath) ||
                    !TryParseManagedGeneratedAssetPath(
                        assetPath,
                        out _,
                        out _,
                        out _))
                {
                    resultMessage =
                        "A confirmed path no longer satisfies the managed generated-asset contract. Nothing was deleted.\n" +
                        assetPath;
                    return false;
                }
            }

            List<string> failures = new List<string>();
            bool allDeleted = AssetDatabase.DeleteAssets(
                currentPaths.ToArray(),
                failures);
            int deletedCount = currentPaths.Count - failures.Count;
            refreshedReport = RunAudit();
            StringBuilder builder = new StringBuilder(512);
            builder.Append("Deleted ")
                .Append(deletedCount)
                .Append(" confirmed Painted Accent orphan asset(s).");
            if (failures.Count > 0)
            {
                builder.Append("\n\nDeletion failures:");
                for (int index = 0; index < failures.Count; index++)
                {
                    builder.Append("\n- ")
                        .Append(failures[index]);
                }
            }

            resultMessage = builder.ToString();
            return allDeleted && failures.Count == 0;
        }

        private static List<string> CollectGeneratedAssetPaths()
        {
            List<string> paths = new List<string>();
            string root =
                GroundPaintedAccentProductionBaker.GeneratedRootPath;
            if (!AssetDatabase.IsValidFolder(root))
            {
                return paths;
            }

            string[] guids = AssetDatabase.FindAssets(
                string.Empty,
                new[] { root });
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                if (string.IsNullOrWhiteSpace(path) ||
                    AssetDatabase.IsValidFolder(path) ||
                    !IsPathUnderGeneratedRoot(path))
                {
                    continue;
                }

                paths.Add(path);
            }

            paths.Sort(StringComparer.OrdinalIgnoreCase);
            return paths;
        }

        private static Dictionary<string, List<GroundClaim>> CreateClaimMap(
            int capacity)
        {
            return new Dictionary<string, List<GroundClaim>>(
                Mathf.Max(1, capacity),
                StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, List<string>> CreateReferenceMap(
            int capacity)
        {
            return new Dictionary<string, List<string>>(
                Mathf.Max(1, capacity),
                StringComparer.OrdinalIgnoreCase);
        }

        private static bool CollectGroundClaims(
            HashSet<string> generatedAssetSet,
            Dictionary<string, List<GroundClaim>> claimsByAsset,
            GroundPaintedAccentGeneratedAssetAuditReport report)
        {
            HashSet<string> inspectedScenePaths =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int loadedSceneCount = SceneManager.sceneCount;
            for (int index = 0; index < loadedSceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(scene.path) &&
                    !inspectedScenePaths.Add(scene.path))
                {
                    continue;
                }

                CollectSceneClaims(
                    scene,
                    scene.path,
                    generatedAssetSet,
                    claimsByAsset);
            }

            string[] sceneGuids = AssetDatabase.FindAssets(
                "t:Scene",
                new[] { "Assets" });
            for (int index = 0; index < sceneGuids.Length; index++)
            {
                string scenePath =
                    AssetDatabase.GUIDToAssetPath(sceneGuids[index]);
                if (string.IsNullOrWhiteSpace(scenePath) ||
                    inspectedScenePaths.Contains(scenePath))
                {
                    continue;
                }

                if (EditorUtility.DisplayCancelableProgressBar(
                        "Auditing Painted Accent owners",
                        scenePath,
                        sceneGuids.Length > 0
                            ? (float)index / sceneGuids.Length
                            : 1f))
                {
                    return false;
                }

                Scene previewScene = default;
                try
                {
                    previewScene =
                        EditorSceneManager.OpenPreviewScene(scenePath);
                    if (!previewScene.IsValid() || !previewScene.isLoaded)
                    {
                        report.AddFailure(
                            "Could not open scene for generated-asset ownership audit: " +
                            scenePath);
                        continue;
                    }

                    CollectSceneClaims(
                        previewScene,
                        scenePath,
                        generatedAssetSet,
                        claimsByAsset);
                }
                catch (Exception exception)
                {
                    report.AddFailure(
                        "Scene ownership audit failed for " +
                        scenePath + ": " + exception.Message);
                }
                finally
                {
                    if (previewScene.IsValid())
                    {
                        EditorSceneManager.ClosePreviewScene(previewScene);
                    }
                }
            }

            return true;
        }

        private static void CollectSceneClaims(
            Scene scene,
            string sourceScenePath,
            HashSet<string> generatedAssetSet,
            Dictionary<string, List<GroundClaim>> claimsByAsset)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                GeneratedGround[] grounds =
                    roots[rootIndex]
                        .GetComponentsInChildren<GeneratedGround>(true);
                for (int groundIndex = 0;
                     groundIndex < grounds.Length;
                     groundIndex++)
                {
                    GeneratedGround ground = grounds[groundIndex];
                    if (ground == null)
                    {
                        continue;
                    }

                    AddGroundClaims(
                        ground,
                        sourceScenePath,
                        generatedAssetSet,
                        claimsByAsset);
                }
            }
        }

        private static void AddGroundClaims(
            GeneratedGround ground,
            string scenePath,
            HashSet<string> generatedAssetSet,
            Dictionary<string, List<GroundClaim>> claimsByAsset)
        {
            string identifier =
                ground.PaintedAccentProductionBakeIdentifier;
            Texture2D texture =
                ground.PaintedAccentProductionCoverageTexture;
            string actualPath = texture != null
                ? AssetDatabase.GetAssetPath(texture)
                : string.Empty;
            string expectedPath = string.Empty;
            if (GroundPaintedAccentProductionBaker.IsValidIdentifier(
                    identifier) &&
                !string.IsNullOrWhiteSpace(scenePath))
            {
                GroundPaintedAccentProductionBaker.TryGetExpectedAssetPath(
                    scenePath,
                    identifier,
                    out expectedPath,
                    out _);
            }

            bool ownershipMatches =
                !string.IsNullOrWhiteSpace(actualPath) &&
                !string.IsNullOrWhiteSpace(expectedPath) &&
                string.Equals(
                    actualPath,
                    expectedPath,
                    StringComparison.OrdinalIgnoreCase);
            GroundClaim claim = new GroundClaim
            {
                ScenePath = scenePath ?? string.Empty,
                GroundPath = BuildHierarchyPath(ground.transform),
                ActualAssetPath = actualPath,
                ExpectedAssetPath = expectedPath,
                Required = ground.PaintedAccentProductionBakeRequired,
                OwnershipMatches = ownershipMatches
            };

            if (!string.IsNullOrWhiteSpace(actualPath) &&
                generatedAssetSet.Contains(actualPath))
            {
                AddClaim(claimsByAsset, actualPath, claim);
            }

            if (!string.IsNullOrWhiteSpace(expectedPath) &&
                generatedAssetSet.Contains(expectedPath) &&
                !string.Equals(
                    expectedPath,
                    actualPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                AddClaim(claimsByAsset, expectedPath, claim);
            }
        }

        private static void AddClaim(
            Dictionary<string, List<GroundClaim>> claimsByAsset,
            string assetPath,
            GroundClaim claim)
        {
            if (!claimsByAsset.TryGetValue(
                    assetPath,
                    out List<GroundClaim> claims))
            {
                claims = new List<GroundClaim>(1);
                claimsByAsset.Add(assetPath, claims);
            }

            claims.Add(claim);
        }

        private static bool CollectProjectReferences(
            HashSet<string> generatedAssetSet,
            Dictionary<string, List<string>> referencesByAsset,
            GroundPaintedAccentGeneratedAssetAuditReport report)
        {
            string[] projectPaths = AssetDatabase.GetAllAssetPaths();
            for (int index = 0; index < projectPaths.Length; index++)
            {
                string sourcePath = projectPaths[index];
                if (string.IsNullOrWhiteSpace(sourcePath) ||
                    !sourcePath.StartsWith(
                        "Assets/",
                        StringComparison.OrdinalIgnoreCase) ||
                    AssetDatabase.IsValidFolder(sourcePath) ||
                    IsPathUnderGeneratedRoot(sourcePath))
                {
                    continue;
                }

                if (EditorUtility.DisplayCancelableProgressBar(
                        "Auditing Painted Accent references",
                        sourcePath,
                        projectPaths.Length > 0
                            ? (float)index / projectPaths.Length
                            : 1f))
                {
                    return false;
                }

                try
                {
                    string[] dependencies =
                        AssetDatabase.GetDependencies(sourcePath, false);
                    for (int dependencyIndex = 0;
                         dependencyIndex < dependencies.Length;
                         dependencyIndex++)
                    {
                        string dependency = dependencies[dependencyIndex];
                        if (string.Equals(
                                dependency,
                                sourcePath,
                                StringComparison.OrdinalIgnoreCase) ||
                            !generatedAssetSet.Contains(dependency))
                        {
                            continue;
                        }

                        if (!referencesByAsset.TryGetValue(
                                dependency,
                                out List<string> references))
                        {
                            references = new List<string>(1);
                            referencesByAsset.Add(
                                dependency,
                                references);
                        }

                        bool alreadyRecorded = false;
                        for (int referenceIndex = 0;
                             referenceIndex < references.Count;
                             referenceIndex++)
                        {
                            if (string.Equals(
                                    references[referenceIndex],
                                    sourcePath,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                alreadyRecorded = true;
                                break;
                            }
                        }

                        if (!alreadyRecorded)
                        {
                            references.Add(sourcePath);
                        }
                    }
                }
                catch (Exception exception)
                {
                    report.AddFailure(
                        "Dependency audit failed for " +
                        sourcePath + ": " + exception.Message);
                }
            }

            return true;
        }

        private static void AddDeletionBlockers(
            GroundPaintedAccentGeneratedAssetAuditReport report)
        {
            List<string> dirtyScenes = new List<string>();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene.IsValid() && scene.isLoaded && scene.isDirty)
                {
                    dirtyScenes.Add(
                        string.IsNullOrWhiteSpace(scene.path)
                            ? scene.name + " (unsaved scene)"
                            : scene.path);
                }
            }

            if (dirtyScenes.Count > 0)
            {
                report.AddDeletionBlocker(
                    "Loaded scenes contain unsaved changes. Save or revert them before deleting generated assets: " +
                    string.Join(", ", dirtyScenes));
            }

            HashSet<string> dirtyAssetPaths =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            UnityEngine.Object[] loadedObjects =
                Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
            for (int index = 0; index < loadedObjects.Length; index++)
            {
                UnityEngine.Object loadedObject = loadedObjects[index];
                if (loadedObject == null ||
                    !EditorUtility.IsPersistent(loadedObject) ||
                    EditorUtility.GetDirtyCount(loadedObject) == 0)
                {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(loadedObject);
                if (string.IsNullOrWhiteSpace(path) ||
                    !path.StartsWith(
                        "Assets/",
                        StringComparison.OrdinalIgnoreCase) ||
                    IsPathUnderGeneratedRoot(path))
                {
                    continue;
                }

                dirtyAssetPaths.Add(path);
            }

            if (dirtyAssetPaths.Count > 0)
            {
                List<string> sorted = new List<string>(dirtyAssetPaths);
                sorted.Sort(StringComparer.OrdinalIgnoreCase);
                report.AddDeletionBlocker(
                    "Loaded project assets contain unsaved changes. Save or revert them before deleting generated assets: " +
                    string.Join(", ", sorted));
            }
        }

        private static void ClassifyAssets(
            IReadOnlyList<string> generatedAssetPaths,
            Dictionary<string, List<GroundClaim>> claimsByAsset,
            Dictionary<string, List<string>> referencesByAsset,
            GroundPaintedAccentGeneratedAssetAuditReport report)
        {
            for (int index = 0;
                 index < generatedAssetPaths.Count;
                 index++)
            {
                string assetPath = generatedAssetPaths[index];
                if (!TryParseManagedGeneratedAssetPath(
                        assetPath,
                        out string sceneGuid,
                        out string groundIdentifier,
                        out string contractFailure))
                {
                    report.Add(
                        new GroundPaintedAccentGeneratedAssetAuditEntry(
                            assetPath,
                            GroundPaintedAccentGeneratedAssetAuditStatus
                                .UnknownUnsafe,
                            contractFailure));
                    continue;
                }

                UnityEngine.Object mainAsset =
                    AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (!(mainAsset is Texture2D texture) ||
                    texture.format != TextureFormat.R8)
                {
                    report.Add(
                        new GroundPaintedAccentGeneratedAssetAuditEntry(
                            assetPath,
                            GroundPaintedAccentGeneratedAssetAuditStatus
                                .UnknownUnsafe,
                            "The managed path does not contain an R8 Texture2D main asset."));
                    continue;
                }

                claimsByAsset.TryGetValue(
                    assetPath,
                    out List<GroundClaim> claims);
                referencesByAsset.TryGetValue(
                    assetPath,
                    out List<string> references);
                claims ??= new List<GroundClaim>();
                references ??= new List<string>();

                if (claims.Count > 1)
                {
                    report.Add(
                        new GroundPaintedAccentGeneratedAssetAuditEntry(
                            assetPath,
                            GroundPaintedAccentGeneratedAssetAuditStatus
                                .SharedIncorrectly,
                            "Claimed by multiple Grounds:\n" +
                            JoinClaimDescriptions(claims)));
                    continue;
                }

                if (claims.Count == 1)
                {
                    GroundClaim claim = claims[0];
                    if (!claim.OwnershipMatches)
                    {
                        report.Add(
                            new GroundPaintedAccentGeneratedAssetAuditEntry(
                                assetPath,
                                GroundPaintedAccentGeneratedAssetAuditStatus
                                    .OwnershipMismatch,
                                claim.Description +
                                "\nExpected: " +
                                (string.IsNullOrWhiteSpace(
                                     claim.ExpectedAssetPath)
                                    ? "<unavailable>"
                                    : claim.ExpectedAssetPath) +
                                "\nActual: " +
                                (string.IsNullOrWhiteSpace(
                                     claim.ActualAssetPath)
                                    ? "<unassigned>"
                                    : claim.ActualAssetPath)));
                        continue;
                    }

                    report.Add(
                        new GroundPaintedAccentGeneratedAssetAuditEntry(
                            assetPath,
                            claim.Required
                                ? GroundPaintedAccentGeneratedAssetAuditStatus
                                    .ActiveAndReferenced
                                : GroundPaintedAccentGeneratedAssetAuditStatus
                                    .ReferencedButNotRequired,
                            claim.Description));
                    continue;
                }

                if (references.Count > 0)
                {
                    references.Sort(StringComparer.OrdinalIgnoreCase);
                    report.Add(
                        new GroundPaintedAccentGeneratedAssetAuditEntry(
                            assetPath,
                            GroundPaintedAccentGeneratedAssetAuditStatus
                                .UnknownUnsafe,
                            "Referenced by project assets but not claimed by a GeneratedGround:\n" +
                            string.Join("\n", references)));
                    continue;
                }

                report.Add(
                    new GroundPaintedAccentGeneratedAssetAuditEntry(
                        assetPath,
                        GroundPaintedAccentGeneratedAssetAuditStatus
                            .ConfirmedOrphan,
                        "No project asset references this managed output, and no Ground in any project scene claims scene " +
                        sceneGuid + " / Ground " + groundIdentifier + "."));
            }
        }

        private static bool TryParseManagedGeneratedAssetPath(
            string assetPath,
            out string sceneGuid,
            out string groundIdentifier,
            out string failureReason)
        {
            sceneGuid = string.Empty;
            groundIdentifier = string.Empty;
            failureReason = string.Empty;
            string root =
                GroundPaintedAccentProductionBaker.GeneratedRootPath;
            string prefix =
                GroundPaintedAccentProductionBaker.AssetNamePrefix;
            string expectedPrefix = root + "/";
            if (string.IsNullOrWhiteSpace(assetPath) ||
                !assetPath.StartsWith(
                    expectedPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                failureReason =
                    "The asset is outside the managed Painted Accent generated-output root.";
                return false;
            }

            string relative = assetPath.Substring(expectedPrefix.Length);
            string[] segments = relative.Split('/');
            if (segments.Length != 2)
            {
                failureReason =
                    "The generated path does not have exactly one scene-GUID folder and one asset file.";
                return false;
            }

            sceneGuid = segments[0];
            if (!GroundPaintedAccentProductionBaker.IsValidIdentifier(
                    sceneGuid))
            {
                failureReason =
                    "The generated parent folder is not a 32-character scene GUID.";
                return false;
            }

            string filename = Path.GetFileNameWithoutExtension(segments[1]);
            if (!string.Equals(
                    Path.GetExtension(segments[1]),
                    ".asset",
                    StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(filename) ||
                !filename.StartsWith(
                    prefix,
                    StringComparison.Ordinal))
            {
                failureReason =
                    "The generated filename does not match the managed .asset naming contract.";
                return false;
            }

            groundIdentifier = filename.Substring(prefix.Length);
            if (!GroundPaintedAccentProductionBaker.IsValidIdentifier(
                    groundIdentifier))
            {
                failureReason =
                    "The generated filename does not contain a valid 32-character Ground identifier.";
                return false;
            }

            return true;
        }

        private static bool IsPathUnderGeneratedRoot(string assetPath)
        {
            string root =
                GroundPaintedAccentProductionBaker.GeneratedRootPath;
            return !string.IsNullOrWhiteSpace(assetPath) &&
                   assetPath.StartsWith(
                       root + "/",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool PathListsMatch(
            IReadOnlyList<string> left,
            IReadOnlyList<string> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            for (int index = 0; index < left.Count; index++)
            {
                if (!string.Equals(
                        left[index],
                        right[index],
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static string JoinClaimDescriptions(
            IReadOnlyList<GroundClaim> claims)
        {
            StringBuilder builder = new StringBuilder(256);
            for (int index = 0; index < claims.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append("\n");
                }

                builder.Append(claims[index].Description);
            }

            return builder.ToString();
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
