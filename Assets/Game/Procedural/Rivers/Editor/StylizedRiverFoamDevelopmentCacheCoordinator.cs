using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    /// <summary>
    /// Explicit Edit Mode transaction for preparing and persisting one River
    /// Foam topology cache. P1 deliberately gives this type no
    /// InitializeOnLoad hooks, global polling, Play Mode scans, or automatic
    /// asset assignment. P3 keeps the transaction to one final topology
    /// publication, one normal serialization, one storage clone, and one save.
    /// </summary>
    internal static class StylizedRiverFoamDevelopmentCacheCoordinator
    {
        internal static bool TryPrepareAndPersist(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime,
            out bool validationPassed,
            out string message)
        {
            validationPassed = false;
            message = string.Empty;

            if (river == null || runtime == null)
            {
                message = "A single River and its Foam runtime are required.";
                return false;
            }

            if (Application.isPlaying)
            {
                message =
                    "Foam topology cache preparation is available only in Edit Mode.";
                return false;
            }

            StylizedRiverFoamTopologyCacheAsset asset =
                river.FoamTopologyCacheAsset;
            if (asset == null)
            {
                message = "Assign a Foam topology cache asset before preparation.";
                return false;
            }

            System.Diagnostics.Stopwatch transactionStopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            if (!runtime.TryPrepareTopologyCacheInEditor(
                    out StylizedRiverFoamTopologyCacheBuildArtifact artifact))
            {
                transactionStopwatch.Stop();
                message =
                    $"{runtime.TopologyCacheBuildState}. " +
                    runtime.TopologyCacheBuildSummary +
                    $" Total={transactionStopwatch.Elapsed.TotalMilliseconds:0.000} ms.";
                return false;
            }

            Undo.RecordObject(asset, "Update River Foam Topology Cache");
            asset.StoreBuild(artifact);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            validationPassed = asset.MatchesStoredBuild(
                artifact,
                out string storageSummary);
            transactionStopwatch.Stop();
            string state = validationPassed
                ? "Stored"
                : "Storage Verification Failed";
            message =
                $"{state}. {storageSummary} " +
                $"Payload={artifact.PayloadByteCount:N0} bytes, " +
                $"hash={artifact.PayloadHash}, " +
                $"obstacles={runtime.TopologyCacheObstacleSourceCount:N0}, " +
                $"payloadBuild={artifact.BuildMilliseconds:0.000} ms, " +
                $"total={transactionStopwatch.Elapsed.TotalMilliseconds:0.000} ms, " +
                $"GPU publications=" +
                $"{runtime.TopologyCachePreparationGeneratedUploadCount:N0}, " +
                $"serializations=" +
                $"{runtime.TopologyCacheLastBuildSerializationCount:N0}.";
            return true;
        }
    }
}
