#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverDisturbanceRuntime
    {
        internal bool TryCaptureObstacleExclusionDiagnosticSnapshot(
            string label,
            out StylizedRiverFoamObstacleDiagnosticSnapshot snapshot,
            out string error,
            bool prepareRegistry = true)
        {
            snapshot = null;
            error = string.Empty;
            river ??= GetComponent<StylizedRiver>();
            if (river == null || !river.Domain.IsValid)
            {
                error = "A valid river domain is required.";
                return false;
            }

            if (prepareRegistry &&
                !PrepareGeneratedGeometrySourcesForCacheValidation(
                    out string preparationStatus))
            {
                error = preparationStatus;
                return false;
            }

            if (!GeneratedObstacleRegistryReady)
            {
                error =
                    "The generated obstacle registry is not settled " +
                    $"({GeneratedObstacleRegistryProcessedCount:N0} / " +
                    $"{GeneratedObstacleRegistryTotalCount:N0}).";
                return false;
            }

            List<StylizedRiverFoamObstacleSourceDiagnostic> diagnostics =
                new();
            List<GeneratedGeometryStableFingerprint> providerFingerprints =
                new();
            HashSet<EntityId> seenMeshFilters = new();
            int registryEnumerationOrder = 0;
            Dictionary<string, int> stableKeyOccurrences = new(
                StringComparer.Ordinal);

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                int sourceEnumerationOrder = registryEnumerationOrder++;
                ContinuousSource source = pair.Value;
                MeshFilter meshFilter = source.ObstacleExclusionMeshFilter;
                if (!source.IsStatic ||
                    meshFilter == null ||
                    meshFilter.sharedMesh == null ||
                    !meshFilter.gameObject.activeInHierarchy)
                {
                    continue;
                }

                EntityId meshEntityId = meshFilter.GetEntityId();
                bool includedInCombinedFingerprint =
                    seenMeshFilters.Add(meshEntityId);

                Mesh mesh = meshFilter.sharedMesh;
                IGeneratedGeometryStableFingerprintSource provider =
                    source.ObstacleExclusionFingerprintSource;
                GeneratedGeometryStableFingerprint providerFingerprint =
                    default;
                string providerStatus = string.Empty;
                bool providerReported = provider != null &&
                    provider.TryGetStableWorldGeometryFingerprint(
                        out providerFingerprint,
                        out providerStatus);
                providerStatus ??= string.Empty;
                bool providerAvailable =
                    providerReported && !providerFingerprint.IsDefault;
                if (providerReported && providerFingerprint.IsDefault)
                {
                    providerStatus =
                        "Provider returned success with the reserved all-zero " +
                        "fingerprint; the result was rejected. " + providerStatus;
                }
                if (includedInCombinedFingerprint && providerAvailable)
                {
                    providerFingerprints.Add(providerFingerprint);
                }

                bool directAvailable =
                    GeneratedGeometryStableFingerprintUtility
                        .TryComputeExactWorldTriangleFingerprint(
                            meshFilter,
                            out GeneratedGeometryStableFingerprint
                                directFingerprint,
                            out string directStatus);
                directStatus ??= string.Empty;

                bool localAvailable = TryComputeLocalMeshFingerprint(
                    meshFilter,
                    out GeneratedGeometryStableFingerprint localFingerprint,
                    out int vertexCount,
                    out int triangleIndexCount,
                    out string localStatus);
                bool transformAvailable = TryComputeTransformFingerprint(
                    meshFilter.transform,
                    out GeneratedGeometryStableFingerprint
                        transformFingerprint,
                    out string transformStatus);

                Component providerComponent = provider as Component;
                string hierarchyPath = BuildHierarchyPath(
                    meshFilter.transform);
                string ownerType = providerComponent != null
                    ? providerComponent.GetType().FullName
                    : provider != null
                        ? provider.GetType().FullName
                        : "<none>";
                string providerType = provider != null
                    ? provider.GetType().FullName
                    : "<none>";
                string providerIdentity = BuildComponentIdentity(
                    providerComponent);
                string stableKeyBase =
                    $"mesh={hierarchyPath}|" +
                    $"meshFilterType={meshFilter.GetType().FullName}|" +
                    $"provider={providerIdentity}|" +
                    $"providerType={providerType}|" +
                    $"meshName={mesh.name}";
                stableKeyOccurrences.TryGetValue(
                    stableKeyBase,
                    out int stableKeyOccurrence);
                stableKeyOccurrences[stableKeyBase] =
                    stableKeyOccurrence + 1;
                string stableKey =
                    $"{stableKeyBase}|registration={stableKeyOccurrence}";
                Bounds localBounds = mesh.bounds;
                Bounds worldBounds = ResolveWorldBounds(
                    meshFilter.transform,
                    localBounds);
                string mergedDirectStatus =
                    $"world={directStatus}; local={localStatus}; " +
                    $"transform={transformStatus}";

                diagnostics.Add(
                    new StylizedRiverFoamObstacleSourceDiagnostic(
                        stableKey,
                        pair.Key.ToString(),
                        source.OwnerId.ToString(),
                        meshEntityId.ToString(),
                        hierarchyPath,
                        ownerType,
                        providerType,
                        meshFilter.name,
                        mesh.name,
                        sourceEnumerationOrder,
                        includedInCombinedFingerprint,
                        meshFilter.gameObject.activeInHierarchy,
                        mesh.isReadable,
                        vertexCount,
                        triangleIndexCount,
                        localBounds,
                        worldBounds,
                        localAvailable ? localFingerprint : default,
                        transformAvailable ? transformFingerprint : default,
                        providerAvailable ? providerFingerprint : default,
                        directAvailable ? directFingerprint : default,
                        providerAvailable,
                        directAvailable,
                        providerAvailable && directAvailable &&
                            providerFingerprint.Equals(directFingerprint),
                        providerStatus,
                        mergedDirectStatus));
            }

            diagnostics.Sort((left, right) => string.CompareOrdinal(
                left.StableKey,
                right.StableKey));
            providerFingerprints.Sort();
            if (!RiverObstacleExclusionResolver
                    .TryCombineStableSourceFingerprints(
                        providerFingerprints,
                        out StylizedRiverFoamTopologyFingerprint
                            combinedFingerprint,
                        out int combinedSourceCount,
                        out string combineStatus))
            {
                error = combineStatus;
                return false;
            }

            string status =
                $"Captured {diagnostics.Count:N0} exact obstacle registration(s); " +
                $"unique MeshFilters={seenMeshFilters.Count:N0}; " +
                $"combined provider count={combinedSourceCount:N0}; " +
                $"duplicate registrations=" +
                $"{Mathf.Max(0, diagnostics.Count - seenMeshFilters.Count):N0}. " +
                combineStatus;
            snapshot = new StylizedRiverFoamObstacleDiagnosticSnapshot(
                label,
                DateTime.UtcNow,
                diagnostics,
                combinedFingerprint,
                status);
            return true;
        }

        internal bool TryBuildObstacleRasterParityReport(
            StylizedRiverFoamGridDescriptor descriptor,
            out bool exact,
            out string report,
            out string error)
        {
            exact = false;
            report = string.Empty;
            error = string.Empty;
            river ??= GetComponent<StylizedRiver>();
            if (river == null || !river.Domain.IsValid)
            {
                error = "A valid river domain is required.";
                return false;
            }

            if (!PrepareGeneratedGeometrySourcesForCacheValidation(
                    out string preparationStatus))
            {
                error = preparationStatus;
                return false;
            }

            List<MeshFilter> meshFilters = new();
            CopyObstacleExclusionMeshFiltersTo(meshFilters);
            meshFilters.Sort((left, right) => string.CompareOrdinal(
                BuildHierarchyPath(left != null ? left.transform : null),
                BuildHierarchyPath(right != null ? right.transform : null)));

            StringBuilder builder = new(32768);
            builder.AppendLine("P5.2 TRUE LEGACY OBSTACLE RASTER PARITY");
            builder.AppendLine($"Source count: {meshFilters.Count:N0}");
            builder.AppendLine(
                $"Descriptor: {descriptor.Mapping}; " +
                $"field={descriptor.ColumnCount}x{descriptor.RowCount}; " +
                $"allocated={descriptor.AllocatedLengthMetres:R}");
            builder.AppendLine();

            bool allExact = true;
            for (int index = 0; index < meshFilters.Count; index++)
            {
                MeshFilter meshFilter = meshFilters[index];
                builder.AppendLine($"SOURCE {index}");
                builder.AppendLine(
                    "Hierarchy: " + BuildHierarchyPath(
                        meshFilter != null ? meshFilter.transform : null));
                if (!RiverObstacleExclusionResolver
                        .TryBuildLegacyRasterParityReport(
                            river,
                            meshFilter,
                            descriptor,
                            out bool sourceExact,
                            out string sourceReport,
                            out string sourceError))
                {
                    allExact = false;
                    builder.AppendLine("DIAGNOSTIC ERROR: " + sourceError);
                    builder.AppendLine();
                    continue;
                }

                allExact &= sourceExact;
                builder.AppendLine(sourceReport);
            }

            exact = allExact;
            builder.AppendLine("AGGREGATE VERDICT");
            builder.AppendLine(
                $"Legacy obstacle parity: {(allExact ? "EXACT" : "DIFFERENT")}");
            report = builder.ToString();
            return true;
        }

        private static bool TryComputeLocalMeshFingerprint(
            MeshFilter meshFilter,
            out GeneratedGeometryStableFingerprint fingerprint,
            out int vertexCount,
            out int triangleIndexCount,
            out string status)
        {
            fingerprint = default;
            vertexCount = 0;
            triangleIndexCount = 0;
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                status = "No readable mesh was assigned.";
                return false;
            }

            try
            {
                Vector3[] vertices = meshFilter.sharedMesh.vertices;
                int[] triangles = meshFilter.sharedMesh.triangles;
                vertexCount = vertices?.Length ?? 0;
                triangleIndexCount = triangles?.Length ?? 0;
                if (vertexCount < 3 || triangleIndexCount < 3)
                {
                    status = "The mesh contains no complete triangle geometry.";
                    return false;
                }

                GeneratedGeometryStableHashBuilder builder =
                    GeneratedGeometryStableHashBuilder.Create(
                        "PS3D.RiverFoam.ObstacleDiagnostic.LocalMesh.v1");
                builder.AddInt32(vertexCount);
                for (int index = 0; index < vertexCount; index++)
                {
                    builder.AddVector3(vertices[index]);
                }
                builder.AddInt32(triangleIndexCount);
                for (int index = 0; index < triangleIndexCount; index++)
                {
                    builder.AddInt32(triangles[index]);
                }

                fingerprint = builder.Finish();
                status = "Exact local vertices and triangle indices captured.";
                return true;
            }
            catch (UnityException exception)
            {
                status =
                    $"The mesh is not CPU-readable: {exception.Message}";
                return false;
            }
        }

        private static bool TryComputeTransformFingerprint(
            Transform transform,
            out GeneratedGeometryStableFingerprint fingerprint,
            out string status)
        {
            fingerprint = default;
            if (transform == null)
            {
                status = "No transform was available.";
                return false;
            }

            Matrix4x4 matrix = transform.localToWorldMatrix;
            GeneratedGeometryStableHashBuilder builder =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.RiverFoam.ObstacleDiagnostic.Transform.v1");
            builder.AddString(BuildHierarchyPath(transform));
            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    builder.AddSingle(matrix[row, column]);
                }
            }

            fingerprint = builder.Finish();
            status =
                "Hierarchy path and exact local-to-world matrix captured.";
            return true;
        }


        private static string BuildComponentIdentity(Component component)
        {
            if (component == null)
            {
                return "<none>";
            }

            Component[] sameType = component.GetComponents(component.GetType());
            int componentIndex = 0;
            for (int index = 0; index < sameType.Length; index++)
            {
                if (ReferenceEquals(sameType[index], component))
                {
                    componentIndex = index;
                    break;
                }
            }

            return $"{BuildHierarchyPath(component.transform)}|" +
                $"{component.GetType().FullName}[{componentIndex}]";
        }

        private static string BuildHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "<null>";
            }

            List<string> parts = new();
            Transform current = transform;
            while (current != null)
            {
                parts.Add($"{current.name}[{current.GetSiblingIndex()}]");
                current = current.parent;
            }
            parts.Reverse();
            string scenePath = transform.gameObject.scene.path;
            return (string.IsNullOrEmpty(scenePath)
                ? "<unsaved-scene>"
                : scenePath) + ":/" + string.Join("/", parts);
        }

        private static Bounds ResolveWorldBounds(
            Transform transform,
            Bounds localBounds)
        {
            Vector3 centre = localBounds.center;
            Vector3 extents = localBounds.extents;
            Vector3 first = transform.TransformPoint(
                centre + new Vector3(-extents.x, -extents.y, -extents.z));
            Bounds worldBounds = new(first, Vector3.zero);
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        worldBounds.Encapsulate(transform.TransformPoint(
                            centre + new Vector3(
                                extents.x * x,
                                extents.y * y,
                                extents.z * z)));
                    }
                }
            }
            return worldBounds;
        }
    }
}
#endif
