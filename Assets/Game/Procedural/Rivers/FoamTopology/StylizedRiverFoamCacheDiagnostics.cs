#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    internal readonly struct StylizedRiverFoamCacheDiagnosticSection
    {
        public StylizedRiverFoamCacheDiagnosticSection(
            string name,
            byte[] bytes,
            string detail)
        {
            Name = name ?? string.Empty;
            Bytes = bytes ?? Array.Empty<byte>();
            Hash = StylizedRiverFoamCacheDiagnosticUtility.HashBytes(Bytes);
            Detail = detail ?? string.Empty;
        }

        public string Name { get; }
        internal byte[] Bytes { get; }
        public int ByteCount => Bytes?.Length ?? 0;
        public ulong Hash { get; }
        public string Detail { get; }
    }

    internal sealed class StylizedRiverFoamCacheDiagnosticSnapshot
    {
        public StylizedRiverFoamCacheDiagnosticSnapshot(
            string label,
            byte[] payloadBytes,
            int payloadByteCount,
            ulong payloadHash,
            StylizedRiverFoamGridDescriptor descriptor,
            StylizedRiverFoamTopologyFingerprint domainFingerprint,
            StylizedRiverFoamTopologyFingerprint obstacleFingerprint,
            StylizedRiverFoamTopologyFingerprint generationFingerprint,
            StylizedRiverFoamTopologyFingerprint combinedFingerprint,
            IReadOnlyList<StylizedRiverFoamCacheDiagnosticSection> sections,
            string topologySummary)
        {
            Label = label ?? string.Empty;
            PayloadBytes = payloadBytes != null
                ? (byte[])payloadBytes.Clone()
                : Array.Empty<byte>();
            PayloadByteCount = Mathf.Max(0, payloadByteCount);
            PayloadHash = payloadHash;
            Descriptor = descriptor;
            DomainFingerprint = domainFingerprint;
            ObstacleFingerprint = obstacleFingerprint;
            GenerationFingerprint = generationFingerprint;
            CombinedFingerprint = combinedFingerprint;
            Sections = sections ??
                Array.Empty<StylizedRiverFoamCacheDiagnosticSection>();
            TopologySummary = topologySummary ?? string.Empty;
        }

        public string Label { get; }
        internal byte[] PayloadBytes { get; }
        public int PayloadByteCount { get; }
        public ulong PayloadHash { get; }
        public StylizedRiverFoamGridDescriptor Descriptor { get; }
        public StylizedRiverFoamTopologyFingerprint DomainFingerprint { get; }
        public StylizedRiverFoamTopologyFingerprint ObstacleFingerprint { get; }
        public StylizedRiverFoamTopologyFingerprint GenerationFingerprint { get; }
        public StylizedRiverFoamTopologyFingerprint CombinedFingerprint { get; }
        public IReadOnlyList<StylizedRiverFoamCacheDiagnosticSection> Sections
        {
            get;
        }
        public string TopologySummary { get; }

        public bool TryGetSection(
            string name,
            out StylizedRiverFoamCacheDiagnosticSection section)
        {
            for (int index = 0; index < Sections.Count; index++)
            {
                StylizedRiverFoamCacheDiagnosticSection candidate =
                    Sections[index];
                if (string.Equals(
                        candidate.Name,
                        name,
                        StringComparison.Ordinal))
                {
                    section = candidate;
                    return true;
                }
            }

            section = default;
            return false;
        }
    }

    internal readonly struct StylizedRiverFoamObstacleSourceDiagnostic
    {
        public StylizedRiverFoamObstacleSourceDiagnostic(
            string stableKey,
            string sourceEntityId,
            string ownerEntityId,
            string meshFilterEntityId,
            string hierarchyPath,
            string ownerType,
            string providerType,
            string meshFilterName,
            string meshName,
            int registryEnumerationOrder,
            bool includedInCombinedFingerprint,
            bool activeInHierarchy,
            bool meshReadable,
            int vertexCount,
            int triangleIndexCount,
            Bounds localBounds,
            Bounds worldBounds,
            GeneratedGeometryStableFingerprint localMeshFingerprint,
            GeneratedGeometryStableFingerprint transformFingerprint,
            GeneratedGeometryStableFingerprint providerWorldFingerprint,
            GeneratedGeometryStableFingerprint directWorldFingerprint,
            bool providerAvailable,
            bool directWorldAvailable,
            bool providerMatchesDirect,
            string providerStatus,
            string directStatus)
        {
            StableKey = stableKey ?? string.Empty;
            SourceEntityId = sourceEntityId ?? string.Empty;
            OwnerEntityId = ownerEntityId ?? string.Empty;
            MeshFilterEntityId = meshFilterEntityId ?? string.Empty;
            HierarchyPath = hierarchyPath ?? string.Empty;
            OwnerType = ownerType ?? string.Empty;
            ProviderType = providerType ?? string.Empty;
            MeshFilterName = meshFilterName ?? string.Empty;
            MeshName = meshName ?? string.Empty;
            RegistryEnumerationOrder = Mathf.Max(0, registryEnumerationOrder);
            IncludedInCombinedFingerprint = includedInCombinedFingerprint;
            ActiveInHierarchy = activeInHierarchy;
            MeshReadable = meshReadable;
            VertexCount = Mathf.Max(0, vertexCount);
            TriangleIndexCount = Mathf.Max(0, triangleIndexCount);
            LocalBounds = localBounds;
            WorldBounds = worldBounds;
            LocalMeshFingerprint = localMeshFingerprint;
            TransformFingerprint = transformFingerprint;
            ProviderWorldFingerprint = providerWorldFingerprint;
            DirectWorldFingerprint = directWorldFingerprint;
            ProviderAvailable = providerAvailable;
            DirectWorldAvailable = directWorldAvailable;
            ProviderMatchesDirect = providerMatchesDirect;
            ProviderStatus = providerStatus ?? string.Empty;
            DirectStatus = directStatus ?? string.Empty;
        }

        public string StableKey { get; }
        public string SourceEntityId { get; }
        public string OwnerEntityId { get; }
        public string MeshFilterEntityId { get; }
        public string HierarchyPath { get; }
        public string OwnerType { get; }
        public string ProviderType { get; }
        public string MeshFilterName { get; }
        public string MeshName { get; }
        public int RegistryEnumerationOrder { get; }
        public bool IncludedInCombinedFingerprint { get; }
        public bool ActiveInHierarchy { get; }
        public bool MeshReadable { get; }
        public int VertexCount { get; }
        public int TriangleIndexCount { get; }
        public Bounds LocalBounds { get; }
        public Bounds WorldBounds { get; }
        public GeneratedGeometryStableFingerprint LocalMeshFingerprint { get; }
        public GeneratedGeometryStableFingerprint TransformFingerprint { get; }
        public GeneratedGeometryStableFingerprint ProviderWorldFingerprint
        {
            get;
        }
        public GeneratedGeometryStableFingerprint DirectWorldFingerprint
        {
            get;
        }
        public bool ProviderAvailable { get; }
        public bool DirectWorldAvailable { get; }
        public bool ProviderMatchesDirect { get; }
        public string ProviderStatus { get; }
        public string DirectStatus { get; }

        public string CompactIdentity =>
            $"{StableKey} | mesh={MeshName} | vertices={VertexCount:N0} | " +
            $"indices={TriangleIndexCount:N0}";
    }

    internal sealed class StylizedRiverFoamObstacleDiagnosticSnapshot
    {
        public StylizedRiverFoamObstacleDiagnosticSnapshot(
            string label,
            DateTime capturedUtc,
            IReadOnlyList<StylizedRiverFoamObstacleSourceDiagnostic> sources,
            StylizedRiverFoamTopologyFingerprint combinedFingerprint,
            string status)
        {
            Label = label ?? string.Empty;
            CapturedUtc = capturedUtc;
            Sources = sources ??
                Array.Empty<StylizedRiverFoamObstacleSourceDiagnostic>();
            CombinedFingerprint = combinedFingerprint;
            Status = status ?? string.Empty;
        }

        public string Label { get; }
        public DateTime CapturedUtc { get; }
        public IReadOnlyList<StylizedRiverFoamObstacleSourceDiagnostic> Sources
        {
            get;
        }
        public StylizedRiverFoamTopologyFingerprint CombinedFingerprint
        {
            get;
        }
        public string Status { get; }
    }

    internal static class StylizedRiverFoamCacheDiagnosticUtility
    {
        private const ulong FnvOffset = 14695981039346656037ul;
        private const ulong FnvPrime = 1099511628211ul;
        private const int ObstacleBaselineVersion = 2;

        internal static ulong HashBytes(byte[] bytes)
        {
            unchecked
            {
                ulong hash = FnvOffset;
                if (bytes == null)
                {
                    return hash;
                }

                for (int index = 0; index < bytes.Length; index++)
                {
                    hash ^= bytes[index];
                    hash *= FnvPrime;
                }

                return hash;
            }
        }

        internal static int FindFirstDifference(
            byte[] left,
            byte[] right)
        {
            left ??= Array.Empty<byte>();
            right ??= Array.Empty<byte>();
            int sharedLength = Math.Min(left.Length, right.Length);
            for (int index = 0; index < sharedLength; index++)
            {
                if (left[index] != right[index])
                {
                    return index;
                }
            }

            return left.Length == right.Length ? -1 : sharedLength;
        }

        internal static string DescribeBytes(byte[] bytes, int offset)
        {
            if (bytes == null || offset < 0 || offset >= bytes.Length)
            {
                return "<end-of-section>";
            }

            int minimum = Math.Max(0, offset - 8);
            int maximum = Math.Min(bytes.Length, offset + 9);
            StringBuilder builder = new();
            for (int index = minimum; index < maximum; index++)
            {
                if (index > minimum)
                {
                    builder.Append(' ');
                }
                if (index == offset)
                {
                    builder.Append('[');
                }
                builder.Append(bytes[index].ToString("X2"));
                if (index == offset)
                {
                    builder.Append(']');
                }
            }

            return builder.ToString();
        }

        internal static string FormatVector3Exact(Vector3 value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "({0:R}, {1:R}, {2:R}) [0x{3:X8},0x{4:X8},0x{5:X8}]",
                value.x,
                value.y,
                value.z,
                BitConverter.SingleToInt32Bits(value.x),
                BitConverter.SingleToInt32Bits(value.y),
                BitConverter.SingleToInt32Bits(value.z));
        }

        internal static string FormatBoundsExact(Bounds bounds)
        {
            return $"centre={FormatVector3Exact(bounds.center)}, " +
                $"size={FormatVector3Exact(bounds.size)}";
        }

        internal static string BuildObstacleSnapshotReport(
            StylizedRiverFoamObstacleDiagnosticSnapshot snapshot)
        {
            StringBuilder builder = new(8192);
            builder.AppendLine($"Obstacle snapshot: {snapshot.Label}");
            builder.AppendLine(
                $"Captured UTC: {snapshot.CapturedUtc:O}");
            builder.AppendLine(
                $"Source count: {snapshot.Sources.Count:N0}");
            builder.AppendLine(
                $"Combined prepared fingerprint: " +
                snapshot.CombinedFingerprint);
            builder.AppendLine($"Capture status: {snapshot.Status}");
            builder.AppendLine();

            for (int index = 0; index < snapshot.Sources.Count; index++)
            {
                StylizedRiverFoamObstacleSourceDiagnostic source =
                    snapshot.Sources[index];
                builder.AppendLine($"SOURCE {index:N0}");
                builder.AppendLine($"  Stable key: {source.StableKey}");
                builder.AppendLine(
                    $"  Source entity: {source.SourceEntityId}");
                builder.AppendLine(
                    $"  Owner entity: {source.OwnerEntityId}");
                builder.AppendLine(
                    $"  MeshFilter entity: {source.MeshFilterEntityId}");
                builder.AppendLine(
                    $"  Hierarchy path: {source.HierarchyPath}");
                builder.AppendLine($"  Owner type: {source.OwnerType}");
                builder.AppendLine(
                    $"  Fingerprint provider: {source.ProviderType}");
                builder.AppendLine(
                    $"  MeshFilter: {source.MeshFilterName}");
                builder.AppendLine($"  Mesh: {source.MeshName}");
                builder.AppendLine(
                    $"  Registry enumeration order: " +
                    $"{source.RegistryEnumerationOrder:N0}");
                builder.AppendLine(
                    $"  Included in combined cache fingerprint: " +
                    source.IncludedInCombinedFingerprint);
                builder.AppendLine(
                    $"  Active / readable: {source.ActiveInHierarchy} / " +
                    source.MeshReadable);
                builder.AppendLine(
                    $"  Vertices / triangle indices: " +
                    $"{source.VertexCount:N0} / " +
                    $"{source.TriangleIndexCount:N0}");
                builder.AppendLine(
                    $"  Local bounds: " +
                    FormatBoundsExact(source.LocalBounds));
                builder.AppendLine(
                    $"  World bounds: " +
                    FormatBoundsExact(source.WorldBounds));
                builder.AppendLine(
                    $"  Local mesh fingerprint: " +
                    source.LocalMeshFingerprint);
                builder.AppendLine(
                    $"  Transform fingerprint: " +
                    source.TransformFingerprint);
                builder.AppendLine(
                    $"  Provider world fingerprint: " +
                    (source.ProviderAvailable
                        ? source.ProviderWorldFingerprint.ToString()
                        : "UNAVAILABLE"));
                builder.AppendLine(
                    $"  Direct world fingerprint: " +
                    (source.DirectWorldAvailable
                        ? source.DirectWorldFingerprint.ToString()
                        : "UNAVAILABLE"));
                builder.AppendLine(
                    $"  Provider/direct match: " +
                    source.ProviderMatchesDirect);
                builder.AppendLine(
                    $"  Provider status: {source.ProviderStatus}");
                builder.AppendLine(
                    $"  Direct status: {source.DirectStatus}");
                builder.AppendLine();
            }

            return builder.ToString();
        }

        internal static bool ObstacleSnapshotsEqual(
            StylizedRiverFoamObstacleDiagnosticSnapshot left,
            StylizedRiverFoamObstacleDiagnosticSnapshot right,
            out string comparison)
        {
            StringBuilder builder = new(4096);
            bool obstacleInputEqual = true;
            bool provenanceEqual = true;
            if (left.CombinedFingerprint != right.CombinedFingerprint)
            {
                obstacleInputEqual = false;
                builder.AppendLine(
                    $"Combined fingerprint changed: " +
                    $"{left.CombinedFingerprint} -> " +
                    right.CombinedFingerprint);
            }

            Dictionary<string, StylizedRiverFoamObstacleSourceDiagnostic>
                leftByKey = new(StringComparer.Ordinal);
            Dictionary<string, StylizedRiverFoamObstacleSourceDiagnostic>
                rightByKey = new(StringComparer.Ordinal);
            for (int index = 0; index < left.Sources.Count; index++)
            {
                StylizedRiverFoamObstacleSourceDiagnostic source =
                    left.Sources[index];
                if (leftByKey.ContainsKey(source.StableKey))
                {
                    obstacleInputEqual = false;
                    builder.AppendLine(
                        $"Duplicate stable source key in left snapshot: " +
                        source.StableKey);
                }
                else
                {
                    leftByKey.Add(source.StableKey, source);
                }
            }
            for (int index = 0; index < right.Sources.Count; index++)
            {
                StylizedRiverFoamObstacleSourceDiagnostic source =
                    right.Sources[index];
                if (rightByKey.ContainsKey(source.StableKey))
                {
                    obstacleInputEqual = false;
                    builder.AppendLine(
                        $"Duplicate stable source key in right snapshot: " +
                        source.StableKey);
                }
                else
                {
                    rightByKey.Add(source.StableKey, source);
                }
            }

            SortedSet<string> keys = new(StringComparer.Ordinal);
            keys.UnionWith(leftByKey.Keys);
            keys.UnionWith(rightByKey.Keys);
            foreach (string key in keys)
            {
                bool hasLeft = leftByKey.TryGetValue(key, out var a);
                bool hasRight = rightByKey.TryGetValue(key, out var b);
                if (!hasLeft || !hasRight)
                {
                    obstacleInputEqual = false;
                    builder.AppendLine(
                        $"Source {(hasLeft ? "removed" : "added")}: {key}");
                    continue;
                }

                AppendDifference(
                    "source entity",
                    a.SourceEntityId,
                    b.SourceEntityId,
                    false);
                AppendDifference(
                    "owner entity",
                    a.OwnerEntityId,
                    b.OwnerEntityId,
                    false);
                AppendDifference(
                    "MeshFilter entity",
                    a.MeshFilterEntityId,
                    b.MeshFilterEntityId,
                    false);
                AppendDifference(
                    "hierarchy path",
                    a.HierarchyPath,
                    b.HierarchyPath,
                    true);
                AppendDifference(
                    "owner type",
                    a.OwnerType,
                    b.OwnerType,
                    true);
                AppendDifference(
                    "provider type",
                    a.ProviderType,
                    b.ProviderType,
                    true);
                AppendDifference(
                    "MeshFilter name",
                    a.MeshFilterName,
                    b.MeshFilterName,
                    false);
                AppendDifference(
                    "mesh name",
                    a.MeshName,
                    b.MeshName,
                    false);
                AppendDifference(
                    "registry enumeration order",
                    a.RegistryEnumerationOrder.ToString(
                        CultureInfo.InvariantCulture),
                    b.RegistryEnumerationOrder.ToString(
                        CultureInfo.InvariantCulture),
                    false);
                AppendDifference(
                    "included in combined fingerprint",
                    a.IncludedInCombinedFingerprint.ToString(),
                    b.IncludedInCombinedFingerprint.ToString(),
                    true);
                AppendDifference(
                    "active in hierarchy",
                    a.ActiveInHierarchy.ToString(),
                    b.ActiveInHierarchy.ToString(),
                    true);
                AppendDifference(
                    "mesh readable",
                    a.MeshReadable.ToString(),
                    b.MeshReadable.ToString(),
                    false);
                AppendDifference(
                    "vertex count",
                    a.VertexCount.ToString(CultureInfo.InvariantCulture),
                    b.VertexCount.ToString(CultureInfo.InvariantCulture),
                    true);
                AppendDifference(
                    "triangle-index count",
                    a.TriangleIndexCount.ToString(CultureInfo.InvariantCulture),
                    b.TriangleIndexCount.ToString(CultureInfo.InvariantCulture),
                    true);
                AppendDifference(
                    "local bounds",
                    FormatBoundsExact(a.LocalBounds),
                    FormatBoundsExact(b.LocalBounds),
                    true);
                AppendDifference(
                    "world bounds",
                    FormatBoundsExact(a.WorldBounds),
                    FormatBoundsExact(b.WorldBounds),
                    true);
                AppendDifference(
                    "local mesh fingerprint",
                    a.LocalMeshFingerprint.ToString(),
                    b.LocalMeshFingerprint.ToString(),
                    true);
                AppendDifference(
                    "transform fingerprint",
                    a.TransformFingerprint.ToString(),
                    b.TransformFingerprint.ToString(),
                    true);
                AppendDifference(
                    "provider available",
                    a.ProviderAvailable.ToString(),
                    b.ProviderAvailable.ToString(),
                    true);
                AppendDifference(
                    "provider world fingerprint",
                    a.ProviderAvailable
                        ? a.ProviderWorldFingerprint.ToString()
                        : "UNAVAILABLE",
                    b.ProviderAvailable
                        ? b.ProviderWorldFingerprint.ToString()
                        : "UNAVAILABLE",
                    true);
                AppendDifference(
                    "direct world available",
                    a.DirectWorldAvailable.ToString(),
                    b.DirectWorldAvailable.ToString(),
                    true);
                AppendDifference(
                    "direct world fingerprint",
                    a.DirectWorldAvailable
                        ? a.DirectWorldFingerprint.ToString()
                        : "UNAVAILABLE",
                    b.DirectWorldAvailable
                        ? b.DirectWorldFingerprint.ToString()
                        : "UNAVAILABLE",
                    true);
                AppendDifference(
                    "provider/direct agreement",
                    a.ProviderMatchesDirect.ToString(),
                    b.ProviderMatchesDirect.ToString(),
                    true);
                AppendDifference(
                    "provider status",
                    a.ProviderStatus,
                    b.ProviderStatus,
                    false);
                AppendDifference(
                    "direct status",
                    a.DirectStatus,
                    b.DirectStatus,
                    false);

                void AppendDifference(
                    string label,
                    string leftValue,
                    string rightValue,
                    bool affectsObstacleInput)
                {
                    if (string.Equals(
                            leftValue,
                            rightValue,
                            StringComparison.Ordinal))
                    {
                        return;
                    }

                    if (affectsObstacleInput)
                    {
                        obstacleInputEqual = false;
                    }
                    else
                    {
                        provenanceEqual = false;
                    }
                    builder.AppendLine(
                        $"Source changed: {key}\n" +
                        $"  {label}" +
                        (affectsObstacleInput
                            ? " [input]"
                            : " [provenance-only]") +
                        $": {leftValue} -> {rightValue}");
                }
            }

            builder.AppendLine(
                $"Production-relevant obstacle input: " +
                $"{(obstacleInputEqual ? "EXACT" : "CHANGED")}");
            builder.AppendLine(
                $"Diagnostic provenance metadata: " +
                $"{(provenanceEqual ? "EXACT" : "CHANGED")}");
            comparison = builder.ToString().TrimEnd();
            return obstacleInputEqual;
        }

        internal static void WriteObstacleBaseline(
            string path,
            StylizedRiverFoamObstacleDiagnosticSnapshot snapshot)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using FileStream stream = File.Open(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read);
            using BinaryWriter writer = new(stream, Encoding.UTF8, false);
            writer.Write(ObstacleBaselineVersion);
            writer.Write(snapshot.Label);
            writer.Write(snapshot.CapturedUtc.ToBinary());
            writer.Write(snapshot.CombinedFingerprint.Low);
            writer.Write(snapshot.CombinedFingerprint.High);
            writer.Write(snapshot.Status);
            writer.Write(snapshot.Sources.Count);
            for (int index = 0; index < snapshot.Sources.Count; index++)
            {
                WriteObstacleSource(writer, snapshot.Sources[index]);
            }
        }

        internal static bool TryReadObstacleBaseline(
            string path,
            out StylizedRiverFoamObstacleDiagnosticSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            try
            {
                if (!File.Exists(path))
                {
                    error = $"No obstacle baseline exists at '{path}'.";
                    return false;
                }

                using FileStream stream = File.Open(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);
                using BinaryReader reader = new(stream, Encoding.UTF8, false);
                int version = reader.ReadInt32();
                if (version != ObstacleBaselineVersion)
                {
                    error =
                        $"Unsupported obstacle baseline version {version}; " +
                        $"expected {ObstacleBaselineVersion}.";
                    return false;
                }

                string label = reader.ReadString();
                DateTime captured = DateTime.FromBinary(reader.ReadInt64());
                StylizedRiverFoamTopologyFingerprint fingerprint = new(
                    reader.ReadUInt64(),
                    reader.ReadUInt64());
                string status = reader.ReadString();
                int sourceCount = reader.ReadInt32();
                if (sourceCount < 0 || sourceCount > 100000)
                {
                    error = $"Invalid obstacle baseline source count {sourceCount}.";
                    return false;
                }

                StylizedRiverFoamObstacleSourceDiagnostic[] sources =
                    new StylizedRiverFoamObstacleSourceDiagnostic[sourceCount];
                for (int index = 0; index < sourceCount; index++)
                {
                    sources[index] = ReadObstacleSource(reader);
                }

                if (stream.Position != stream.Length)
                {
                    error =
                        $"Obstacle baseline contains " +
                        $"{stream.Length - stream.Position:N0} trailing byte(s).";
                    return false;
                }

                snapshot = new StylizedRiverFoamObstacleDiagnosticSnapshot(
                    label,
                    captured,
                    sources,
                    fingerprint,
                    status);
                return true;
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is EndOfStreamException ||
                exception is ArgumentException)
            {
                error = exception.Message;
                return false;
            }
        }

        internal static string SanitizeFileName(string value)
        {
            value ??= "River";
            char[] invalid = Path.GetInvalidFileNameChars();
            StringBuilder builder = new(value.Length);
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                builder.Append(Array.IndexOf(invalid, character) >= 0
                    ? '_'
                    : character);
            }
            return builder.Length > 0 ? builder.ToString() : "River";
        }

        private static void WriteObstacleSource(
            BinaryWriter writer,
            StylizedRiverFoamObstacleSourceDiagnostic source)
        {
            writer.Write(source.StableKey);
            writer.Write(source.SourceEntityId);
            writer.Write(source.OwnerEntityId);
            writer.Write(source.MeshFilterEntityId);
            writer.Write(source.HierarchyPath);
            writer.Write(source.OwnerType);
            writer.Write(source.ProviderType);
            writer.Write(source.MeshFilterName);
            writer.Write(source.MeshName);
            writer.Write(source.RegistryEnumerationOrder);
            writer.Write(source.IncludedInCombinedFingerprint);
            writer.Write(source.ActiveInHierarchy);
            writer.Write(source.MeshReadable);
            writer.Write(source.VertexCount);
            writer.Write(source.TriangleIndexCount);
            WriteBounds(writer, source.LocalBounds);
            WriteBounds(writer, source.WorldBounds);
            WriteFingerprint(writer, source.LocalMeshFingerprint);
            WriteFingerprint(writer, source.TransformFingerprint);
            WriteFingerprint(writer, source.ProviderWorldFingerprint);
            WriteFingerprint(writer, source.DirectWorldFingerprint);
            writer.Write(source.ProviderAvailable);
            writer.Write(source.DirectWorldAvailable);
            writer.Write(source.ProviderMatchesDirect);
            writer.Write(source.ProviderStatus);
            writer.Write(source.DirectStatus);
        }

        private static StylizedRiverFoamObstacleSourceDiagnostic
            ReadObstacleSource(BinaryReader reader)
        {
            return new StylizedRiverFoamObstacleSourceDiagnostic(
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadInt32(),
                reader.ReadBoolean(),
                reader.ReadBoolean(),
                reader.ReadBoolean(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                ReadBounds(reader),
                ReadBounds(reader),
                ReadFingerprint(reader),
                ReadFingerprint(reader),
                ReadFingerprint(reader),
                ReadFingerprint(reader),
                reader.ReadBoolean(),
                reader.ReadBoolean(),
                reader.ReadBoolean(),
                reader.ReadString(),
                reader.ReadString());
        }

        private static void WriteBounds(BinaryWriter writer, Bounds bounds)
        {
            writer.Write(bounds.center.x);
            writer.Write(bounds.center.y);
            writer.Write(bounds.center.z);
            writer.Write(bounds.size.x);
            writer.Write(bounds.size.y);
            writer.Write(bounds.size.z);
        }

        private static Bounds ReadBounds(BinaryReader reader)
        {
            return new Bounds(
                new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()),
                new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()));
        }

        private static void WriteFingerprint(
            BinaryWriter writer,
            GeneratedGeometryStableFingerprint fingerprint)
        {
            writer.Write(fingerprint.Low);
            writer.Write(fingerprint.High);
        }

        private static GeneratedGeometryStableFingerprint ReadFingerprint(
            BinaryReader reader)
        {
            return new GeneratedGeometryStableFingerprint(
                reader.ReadUInt64(),
                reader.ReadUInt64());
        }
    }
}

namespace ProgrammaticStylized3D.Rivers
{
    internal static partial class StylizedRiverFoamTopologyCacheCodec
    {
        internal static bool TryCreateDiagnosticSnapshot(
            byte[] payload,
            string label,
            out StylizedRiverFoamCacheDiagnosticSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            if (payload == null || payload.Length < sizeof(ulong))
            {
                error = "A non-empty validated cache payload is required.";
                return false;
            }

            if (!TryDeserialize(
                    payload,
                    out StylizedRiverFoamTopologyCachePackage package,
                    out error))
            {
                return false;
            }

            try
            {
                List<StylizedRiverFoamCacheDiagnosticSection> sections =
                    BuildDiagnosticSections(package);
                StylizedRiverFoamTopologyFingerprint combined =
                    StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                        package.DomainFingerprint,
                        package.ObstacleFingerprint,
                        package.GenerationFingerprint);
                snapshot = new StylizedRiverFoamCacheDiagnosticSnapshot(
                    label,
                    payload,
                    payload.Length,
                    ReadPayloadHash(payload),
                    package.GridDescriptor,
                    package.DomainFingerprint,
                    package.ObstacleFingerprint,
                    package.GenerationFingerprint,
                    combined,
                    sections,
                    BuildDiagnosticTopologySummary(package));
                return true;
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is ArgumentException ||
                exception is InvalidOperationException ||
                exception is OverflowException)
            {
                error = exception.Message;
                return false;
            }
        }

        internal static string CompareDiagnosticSnapshots(
            StylizedRiverFoamCacheDiagnosticSnapshot left,
            StylizedRiverFoamCacheDiagnosticSnapshot right,
            out bool exact)
        {
            StringBuilder builder = new(16384);
            bool comparisonExact = true;
            builder.AppendLine(
                $"CACHE SNAPSHOT COMPARISON: {left.Label} vs {right.Label}");
            builder.AppendLine(
                $"Payload: {left.PayloadByteCount:N0} bytes / " +
                $"{left.PayloadHash:X16}  ->  " +
                $"{right.PayloadByteCount:N0} bytes / " +
                $"{right.PayloadHash:X16}");
            int payloadDifference =
                StylizedRiverFoamCacheDiagnosticUtility.FindFirstDifference(
                    left.PayloadBytes,
                    right.PayloadBytes);
            if (payloadDifference >= 0 ||
                left.PayloadByteCount != right.PayloadByteCount ||
                left.PayloadHash != right.PayloadHash)
            {
                comparisonExact = false;
                builder.AppendLine(
                    $"First full-payload difference: " +
                    $"{payloadDifference:N0}");
                builder.AppendLine(
                    "  left payload bytes:  " +
                    StylizedRiverFoamCacheDiagnosticUtility.DescribeBytes(
                        left.PayloadBytes,
                        payloadDifference));
                builder.AppendLine(
                    "  right payload bytes: " +
                    StylizedRiverFoamCacheDiagnosticUtility.DescribeBytes(
                        right.PayloadBytes,
                        payloadDifference));
            }
            else
            {
                builder.AppendLine("First full-payload difference: none");
            }

            AppendValueComparison(
                builder,
                "Descriptor",
                DescribeGridDescriptor(left.Descriptor),
                DescribeGridDescriptor(right.Descriptor),
                ref comparisonExact);
            AppendValueComparison(
                builder,
                "Domain fingerprint",
                left.DomainFingerprint.ToString(),
                right.DomainFingerprint.ToString(),
                ref comparisonExact);
            AppendValueComparison(
                builder,
                "Obstacle fingerprint",
                left.ObstacleFingerprint.ToString(),
                right.ObstacleFingerprint.ToString(),
                ref comparisonExact);
            AppendValueComparison(
                builder,
                "Generation fingerprint",
                left.GenerationFingerprint.ToString(),
                right.GenerationFingerprint.ToString(),
                ref comparisonExact);
            AppendValueComparison(
                builder,
                "Combined fingerprint",
                left.CombinedFingerprint.ToString(),
                right.CombinedFingerprint.ToString(),
                ref comparisonExact);
            builder.AppendLine();
            builder.AppendLine("SECTION DIGESTS");

            SortedSet<string> sectionNames = new(StringComparer.Ordinal);
            for (int index = 0; index < left.Sections.Count; index++)
            {
                sectionNames.Add(left.Sections[index].Name);
            }
            for (int index = 0; index < right.Sections.Count; index++)
            {
                sectionNames.Add(right.Sections[index].Name);
            }

            string firstChangedSection = string.Empty;
            foreach (string sectionName in sectionNames)
            {
                bool hasLeft = left.TryGetSection(sectionName, out var a);
                bool hasRight = right.TryGetSection(sectionName, out var b);
                if (!hasLeft || !hasRight)
                {
                    comparisonExact = false;
                    firstChangedSection = string.IsNullOrEmpty(firstChangedSection)
                        ? sectionName
                        : firstChangedSection;
                    builder.AppendLine(
                        $"  FAIL {sectionName}: " +
                        (hasLeft ? "missing from right" : "missing from left"));
                    continue;
                }

                bool sectionEqual =
                    a.ByteCount == b.ByteCount && a.Hash == b.Hash;
                if (!sectionEqual)
                {
                    comparisonExact = false;
                    firstChangedSection = string.IsNullOrEmpty(firstChangedSection)
                        ? sectionName
                        : firstChangedSection;
                }
                int firstDifference =
                    StylizedRiverFoamCacheDiagnosticUtility
                        .FindFirstDifference(a.Bytes, b.Bytes);
                builder.AppendLine(
                    $"  {(sectionEqual ? "PASS" : "FAIL")} {sectionName}: " +
                    $"bytes {a.ByteCount:N0}->{b.ByteCount:N0}, " +
                    $"hash {a.Hash:X16}->{b.Hash:X16}, " +
                    $"firstDifference=" +
                    (firstDifference >= 0
                        ? firstDifference.ToString("N0")
                        : "none"));
                if (!sectionEqual)
                {
                    builder.AppendLine(
                        $"    left bytes:  " +
                        StylizedRiverFoamCacheDiagnosticUtility.DescribeBytes(
                            a.Bytes,
                            firstDifference));
                    builder.AppendLine(
                        $"    right bytes: " +
                        StylizedRiverFoamCacheDiagnosticUtility.DescribeBytes(
                            b.Bytes,
                            firstDifference));
                    builder.AppendLine($"    left detail:  {a.Detail}");
                    builder.AppendLine($"    right detail: {b.Detail}");
                }
            }

            builder.AppendLine();
            builder.AppendLine("LEFT TOPOLOGY SUMMARY");
            builder.AppendLine(left.TopologySummary);
            builder.AppendLine("RIGHT TOPOLOGY SUMMARY");
            builder.AppendLine(right.TopologySummary);
            builder.AppendLine(
                $"Comparison verdict: {(comparisonExact ? "EXACT" : "DIFFERENT")}");
            builder.AppendLine(
                "First changed section: " +
                (string.IsNullOrEmpty(firstChangedSection)
                    ? "none"
                    : firstChangedSection));
            exact = comparisonExact;
            return builder.ToString();
        }

        private static void AppendValueComparison(
            StringBuilder builder,
            string label,
            string left,
            string right,
            ref bool exact)
        {
            bool equal = string.Equals(left, right, StringComparison.Ordinal);
            if (!equal)
            {
                exact = false;
            }

            builder.AppendLine(
                $"{label}: {(equal ? "PASS" : "FAIL")} " +
                $"{left} -> {right}");
        }

        private static List<StylizedRiverFoamCacheDiagnosticSection>
            BuildDiagnosticSections(
                StylizedRiverFoamTopologyCachePackage package)
        {
            List<StylizedRiverFoamCacheDiagnosticSection> sections = new();
            AddSection(
                "01 Grid Descriptor",
                writer => WriteGridDescriptor(writer, package.GridDescriptor),
                DescribeGridDescriptor(package.GridDescriptor));
            AddSection(
                "02 Domain Contract",
                writer =>
                {
                    writer.Write(package.DomainGlobalDistanceMinimum);
                    writer.Write(package.DomainGlobalDistanceMaximum);
                    writer.Write(package.DomainLocalLength);
                    writer.Write(package.DomainRequestedSampleSpacing);
                    writer.Write(package.DomainReverseFlow);
                    writer.Write(package.DomainSampleCount);
                    WriteFingerprint(writer, package.DomainFingerprint);
                },
                $"range={package.DomainGlobalDistanceMinimum:R}.." +
                $"{package.DomainGlobalDistanceMaximum:R}, " +
                $"localLength={package.DomainLocalLength:R}, " +
                $"samples={package.DomainSampleCount:N0}, " +
                $"reverse={package.DomainReverseFlow}, " +
                $"fingerprint={package.DomainFingerprint}");
            AddSection(
                "03 Input Fingerprints",
                writer =>
                {
                    WriteFingerprint(writer, package.DomainFingerprint);
                    WriteFingerprint(writer, package.ObstacleFingerprint);
                    WriteFingerprint(writer, package.GenerationFingerprint);
                },
                $"domain={package.DomainFingerprint}, " +
                $"obstacles={package.ObstacleFingerprint}, " +
                $"generation={package.GenerationFingerprint}");
            AddSection(
                "04 Generation Settings",
                writer => WriteDiagnosticGenerationSettings(writer, package),
                $"quality={package.Quality}, shore={package.ShoreMotion:R}, " +
                $"majorSeed={package.MajorSeed}, " +
                $"major={package.MajorAmount:R}/{package.MajorSize:R}/" +
                $"{package.MajorSizeVariation:R}, " +
                $"connector={package.ConnectorAmount:R}/" +
                $"{package.ConnectorDirectness:R}/" +
                $"{package.ConnectorLengthPreference:R}, " +
                $"negative={package.InteriorPocketAmount:R}/" +
                $"{package.EdgeCavityAmount:R}/" +
                $"{package.ConnectorWeakSpanAmount:R}/" +
                $"{package.FreeWaterEventAmount:R}");
            AddSection(
                "05 Obstacle Scalar Field",
                writer => WriteFloatArray(writer, package.ObstacleExclusion),
                BuildFloatFieldDetail(package.ObstacleExclusion));

            AddSection(
                "10 Major Support",
                writer => WriteFloatArray(
                    writer,
                    package.MajorTopology.SupportData),
                BuildFloatFieldDetail(package.MajorTopology.SupportData));
            AddSection(
                "11 Major Regions",
                writer => WriteDiagnosticMajorRegions(
                    writer,
                    package.MajorTopology),
                $"regions={package.MajorTopology.Regions.Count:N0}");
            AddSection(
                "12 Major Prepared Regions",
                writer => WriteDiagnosticMajorPrepared(
                    writer,
                    package.MajorTopology),
                $"prepared={package.MajorTopology.PreparedRegions.Count:N0}, " +
                $"anchors={package.MajorTopology.PreparedRecycleAnchorCount:N0}, " +
                $"fallbacks={package.MajorTopology.RecycleFallbackCount:N0}");
            AddSection(
                "13 Major Complete",
                writer => WriteMajorTopology(
                    writer,
                    package.MajorTopology),
                BuildMajorDetail(package.MajorTopology));

            AddSection(
                "20 Connector Support",
                writer => WriteFloatArray(
                    writer,
                    package.ConnectorTopology.SupportData),
                BuildFloatFieldDetail(package.ConnectorTopology.SupportData));
            AddSection(
                "21 Connector Relationships",
                writer => WriteDiagnosticConnectorRelationships(
                    writer,
                    package.ConnectorTopology),
                $"relationships=" +
                $"{package.ConnectorTopology.Relationships.Count:N0}");
            AddSection(
                "22 Connector Prepared Paths",
                writer => WriteConnectorPaths(
                    writer,
                    package.ConnectorTopology.PreparedPaths),
                BuildConnectorPathDetail(
                    package.ConnectorTopology.PreparedPaths));
            AddSection(
                "23 Connector Catalogue Paths",
                writer => WriteConnectorPaths(
                    writer,
                    package.ConnectorTopology
                        .PreparedRelationshipCataloguePaths),
                BuildConnectorPathDetail(
                    package.ConnectorTopology
                        .PreparedRelationshipCataloguePaths));
            AddSection(
                "24 Connector Complete",
                writer => WriteConnectorTopology(
                    writer,
                    package.ConnectorTopology),
                BuildConnectorDetail(package.ConnectorTopology));

            AddSection(
                "30 Pocket Scalar Fields",
                writer => WriteDiagnosticPocketScalarFields(
                    writer,
                    package.PocketTopology),
                BuildPocketScalarDetail(package.PocketTopology));
            AddSection(
                "31 Pocket Regions",
                writer => WriteDiagnosticPocketRegions(
                    writer,
                    package.PocketTopology),
                $"regions={package.PocketTopology.Regions.Count:N0}");
            AddSection(
                "32 Pocket Prepared Hosted",
                writer => WriteDiagnosticPocketPreparedHosted(
                    writer,
                    package.PocketTopology),
                $"preparedHosted=" +
                $"{package.PocketTopology.PreparedHostedRegions.Count:N0}");
            AddSection(
                "33 Pocket Prepared Free Water",
                writer => WriteDiagnosticPocketPreparedFreeWater(
                    writer,
                    package.PocketTopology),
                $"preparedFreeWater=" +
                $"{package.PocketTopology.PreparedFreeWaterRegions.Count:N0}");
            AddSection(
                "34 Pocket Prepared Weak Spans",
                writer => WriteDiagnosticPocketPreparedWeakSpans(
                    writer,
                    package.PocketTopology),
                $"preparedWeakSpans=" +
                $"{package.PocketTopology.PreparedWeakSpanRegions.Count:N0}");
            AddSection(
                "35 Pocket Complete",
                writer => WritePocketTopology(
                    writer,
                    package.PocketTopology),
                BuildPocketDetail(package.PocketTopology));
            return sections;

            void AddSection(
                string name,
                Action<BinaryWriter> write,
                string detail)
            {
                sections.Add(new StylizedRiverFoamCacheDiagnosticSection(
                    name,
                    SerializeDiagnosticSection(write),
                    detail));
            }
        }

        private static byte[] SerializeDiagnosticSection(
            Action<BinaryWriter> write)
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream, Encoding.UTF8, true))
            {
                write(writer);
                writer.Flush();
            }
            return stream.ToArray();
        }

        private static void WriteDiagnosticGenerationSettings(
            BinaryWriter writer,
            StylizedRiverFoamTopologyCachePackage package)
        {
            writer.Write((int)package.Quality);
            writer.Write(package.ShoreMotion);
            writer.Write(package.MajorSeed);
            writer.Write(package.MajorAmount);
            writer.Write(package.MajorSize);
            writer.Write(package.MajorSizeVariation);
            writer.Write(package.MajorRecycleTerritoryDeviationPercent);
            writer.Write(package.ConnectorAmount);
            writer.Write(package.ConnectorDirectness);
            writer.Write(package.ConnectorLengthPreference);
            writer.Write(package.InteriorPocketAmount);
            writer.Write(package.EdgeCavityAmount);
            writer.Write(package.ConnectorWeakSpanAmount);
            writer.Write(package.FreeWaterEventAmount);
        }

        private static void WriteDiagnosticMajorRegions(
            BinaryWriter writer,
            StylizedRiverFoamMajorTopology topology)
        {
            writer.Write(topology.Regions.Count);
            for (int index = 0; index < topology.Regions.Count; index++)
            {
                StylizedRiverFoamMajorRegion region = topology.Regions[index];
                writer.Write(region.StableId);
                writer.Write(region.LongitudinalOpportunityIndex);
                writer.Write(region.LateralOpportunityIndex);
                writer.Write(region.ActivationOrder);
                writer.Write(region.ActivationRank);
                writer.Write(region.CandidateSeed);
                writer.Write(region.CentreGlobalDistance);
                writer.Write(region.CentreAcrossNormalized);
                writer.Write(region.OrientationRadians);
                writer.Write(region.MetresPerCandidateCell);
                writer.Write(region.EvolutionSeed);
                writer.Write(region.DriftRateSelector);
                writer.Write(region.PhaseOffset);
                writer.Write(region.FadeInSelector);
                writer.Write(region.FadeOutSelector);
                writer.Write(region.MovementSpanSelector);
                writer.Write(region.RecycleSelector);
                writer.Write(region.AnchoringStrength);
            }
        }

        private static void WriteDiagnosticMajorPrepared(
            BinaryWriter writer,
            StylizedRiverFoamMajorTopology topology)
        {
            writer.Write(topology.PreparedRegions.Count);
            for (int index = 0; index < topology.PreparedRegions.Count; index++)
            {
                StylizedRiverFoamPreparedMajorRegion prepared =
                    topology.PreparedRegions[index];
                writer.Write(prepared != null);
                if (prepared == null)
                {
                    continue;
                }
                writer.Write(prepared.StableId);
                writer.Write(prepared.MaskResolution);
                WriteFloatArray(writer, prepared.LocalSupportData);
                WriteVector2(writer, prepared.CentroidCells);
                writer.Write(prepared.PrincipalAngleRadians);
                writer.Write(prepared.MajorHalfExtentCells);
                writer.Write(prepared.MinorHalfExtentCells);
                writer.Write(prepared.RecycleAnchors.Count);
                for (int anchorIndex = 0;
                     anchorIndex < prepared.RecycleAnchors.Count;
                     anchorIndex++)
                {
                    StylizedRiverFoamMajorRecycleAnchor anchor =
                        prepared.RecycleAnchors[anchorIndex];
                    writer.Write(anchor.CentreLocalDistance);
                    writer.Write(anchor.CentreAcrossNormalized);
                    writer.Write(anchor.OrientationRadians);
                    writer.Write(anchor.MetresPerCandidateCell);
                    writer.Write(anchor.IsFallback);
                }
            }
        }

        private static void WriteDiagnosticConnectorRelationships(
            BinaryWriter writer,
            StylizedRiverFoamConnectorTopology topology)
        {
            writer.Write(topology.Relationships.Count);
            for (int index = 0; index < topology.Relationships.Count; index++)
            {
                StylizedRiverFoamConnectorRelationship relationship =
                    topology.Relationships[index];
                writer.Write(relationship.StableId);
                writer.Write(relationship.StartComponentId);
                writer.Write(relationship.EndComponentId);
                writer.Write(relationship.StartEndpointId);
                writer.Write(relationship.EndEndpointId);
                writer.Write(relationship.StartGlobalDistance);
                writer.Write(relationship.StartAcrossNormalized);
                writer.Write(relationship.EndGlobalDistance);
                writer.Write(relationship.EndAcrossNormalized);
                writer.Write(relationship.PathLengthMetres);
                writer.Write(relationship.EvolutionSeed);
                writer.Write(relationship.DriftRateSelector);
                writer.Write(relationship.PhaseOffset);
                writer.Write(relationship.FadeInSelector);
                writer.Write(relationship.FadeOutSelector);
                writer.Write(relationship.MovementSpanSelector);
                writer.Write(relationship.RecycleSelector);
                writer.Write(relationship.FragilitySelector);
            }
        }

        private static void WriteDiagnosticPocketScalarFields(
            BinaryWriter writer,
            StylizedRiverFoamPocketTopology topology)
        {
            WriteFloatArray(writer, topology.PressureData);
            WriteFloatArray(writer, topology.HostedPressureData);
            WriteFloatArray(writer, topology.HostedFallbackPressureData);
            WriteFloatArray(writer, topology.IndependentPressureData);
            WriteFloatArray(writer, topology.StaticIndependentPressureData);
            WriteFloatArray(writer, topology.FreeWaterPressureData);
        }

        private static void WriteDiagnosticPocketRegions(
            BinaryWriter writer,
            StylizedRiverFoamPocketTopology topology)
        {
            writer.Write(topology.Regions.Count);
            for (int index = 0; index < topology.Regions.Count; index++)
            {
                StylizedRiverFoamPocketRegion region = topology.Regions[index];
                writer.Write((int)region.RegionClass);
                writer.Write(region.StableId);
                writer.Write(region.HostRegionId);
                writer.Write(region.CentreGlobalDistance);
                writer.Write(region.CentreAcrossNormalized);
                writer.Write(region.OrientationRadians);
                writer.Write(region.AlongRadiusMetres);
                writer.Write(region.AcrossRadiusMetres);
                WriteVector2(writer, region.BreachDirection);
                writer.Write(region.EvolutionSeed);
                writer.Write(region.DriftRateSelector);
                writer.Write(region.PhaseOffset);
                writer.Write(region.FadeInSelector);
                writer.Write(region.FadeOutSelector);
                writer.Write(region.MovementSpanSelector);
                writer.Write(region.RecycleSelector);
                writer.Write(region.GrowthSelector);
            }
        }

        private static void WriteDiagnosticPocketPreparedHosted(
            BinaryWriter writer,
            StylizedRiverFoamPocketTopology topology)
        {
            writer.Write(topology.PreparedHostedRegions.Count);
            for (int index = 0;
                 index < topology.PreparedHostedRegions.Count;
                 index++)
            {
                StylizedRiverFoamPreparedHostedNegativeRegion prepared =
                    topology.PreparedHostedRegions[index];
                writer.Write(prepared != null);
                if (prepared == null)
                {
                    continue;
                }
                writer.Write((int)prepared.RegionClass);
                writer.Write(prepared.StableId);
                writer.Write(prepared.HostRegionId);
                writer.Write(prepared.HostPreparedIndex);
                writer.Write(prepared.MaskResolution);
                WriteFloatArray(writer, prepared.LocalPressureData);
                WriteVector2(writer, prepared.CentreCandidateCells);
                writer.Write(prepared.Variants.Count);
                for (int variantIndex = 0;
                     variantIndex < prepared.Variants.Count;
                     variantIndex++)
                {
                    StylizedRiverFoamHostedNegativeVariant variant =
                        prepared.Variants[variantIndex];
                    WriteVector2(writer, variant.OffsetCells);
                    writer.Write(variant.RotationRadians);
                    writer.Write(variant.ScaleAlong);
                    writer.Write(variant.ScaleAcross);
                }
            }
        }

        private static void WriteDiagnosticPocketPreparedFreeWater(
            BinaryWriter writer,
            StylizedRiverFoamPocketTopology topology)
        {
            writer.Write(topology.PreparedFreeWaterRegions.Count);
            for (int index = 0;
                 index < topology.PreparedFreeWaterRegions.Count;
                 index++)
            {
                StylizedRiverFoamPreparedFreeWaterRegion prepared =
                    topology.PreparedFreeWaterRegions[index];
                writer.Write(prepared != null);
                if (prepared == null)
                {
                    continue;
                }
                writer.Write(prepared.StableId);
                writer.Write(prepared.MaskResolution);
                WriteFloatArray(writer, prepared.LocalPressureData);
                writer.Write(prepared.CentreLocalDistance);
                writer.Write(prepared.CentreAcrossNormalized);
                writer.Write(prepared.OrientationRadians);
                writer.Write(prepared.MetresPerCell);
                writer.Write(prepared.RecycleAnchors.Count);
                for (int anchorIndex = 0;
                     anchorIndex < prepared.RecycleAnchors.Count;
                     anchorIndex++)
                {
                    StylizedRiverFoamFreeWaterRecycleAnchor anchor =
                        prepared.RecycleAnchors[anchorIndex];
                    writer.Write(anchor.CentreLocalDistance);
                    writer.Write(anchor.CentreAcrossNormalized);
                    writer.Write(anchor.OrientationRadians);
                    writer.Write(anchor.IsFallback);
                }
            }
        }

        private static void WriteDiagnosticPocketPreparedWeakSpans(
            BinaryWriter writer,
            StylizedRiverFoamPocketTopology topology)
        {
            writer.Write(topology.PreparedWeakSpanRegions.Count);
            for (int index = 0;
                 index < topology.PreparedWeakSpanRegions.Count;
                 index++)
            {
                StylizedRiverFoamPreparedWeakSpanRegion prepared =
                    topology.PreparedWeakSpanRegions[index];
                writer.Write(prepared != null);
                if (prepared == null)
                {
                    continue;
                }
                writer.Write(prepared.StableId);
                writer.Write(prepared.ConnectorStableId);
                writer.Write((int)prepared.Availability);
                writer.Write(prepared.NormalizedPathDistance);
                writer.Write(prepared.MinimumNormalizedPathDistance);
                writer.Write(prepared.MaximumNormalizedPathDistance);
                writer.Write(prepared.AlongRadiusMetres);
                writer.Write(prepared.AcrossRadiusMetres);
                writer.Write(prepared.Strength);
                writer.Write(prepared.EvolutionSeed);
                WriteVector2(writer, prepared.AcceptedTangent);
                writer.Write(prepared.AcceptedOrientationRadians);
            }
        }

        private static string DescribeGridDescriptor(
            StylizedRiverFoamGridDescriptor descriptor)
        {
            return
                $"descriptor-v{StylizedRiverFoamGridDescriptor.DescriptorContractVersion}/" +
                $"mapping-{(int)descriptor.Mapping}-v" +
                $"{descriptor.MappingContractVersion}/" +
                $"{descriptor.InitializationSignature:X16}; " +
                $"quality={descriptor.Quality}; " +
                $"field={descriptor.ColumnCount}x{descriptor.RowCount}; " +
                $"film={descriptor.FilmWidth}x{descriptor.FilmHeight}; " +
                $"dx={descriptor.ResolvedDxMetres:R}; " +
                $"dy={descriptor.ResolvedDyMetres:R}; " +
                $"columnsPerChunk={descriptor.ColumnsPerChunk}; " +
                $"globalY={descriptor.GlobalYBase}..{descriptor.GlobalYMaximum}; " +
                $"allocated={descriptor.AllocatedLengthMetres:R}; " +
                $"valid={descriptor.ValidLengthMetres:R}; " +
                $"lateral={descriptor.RepresentedLateralMinimumMetres:R}.." +
                $"{descriptor.RepresentedLateralMaximumMetres:R}";
        }

        private static string BuildDiagnosticTopologySummary(
            StylizedRiverFoamTopologyCachePackage package)
        {
            StringBuilder builder = new(2048);
            builder.AppendLine(BuildMajorDetail(package.MajorTopology));
            builder.AppendLine(BuildConnectorDetail(package.ConnectorTopology));
            builder.AppendLine(BuildPocketDetail(package.PocketTopology));
            return builder.ToString().TrimEnd();
        }

        private static string BuildMajorDetail(
            StylizedRiverFoamMajorTopology topology)
        {
            return
                $"Major: attempted={topology.AttemptedOpportunityCount:N0}, " +
                $"accepted={topology.AcceptedRegionCount:N0}, " +
                $"rejected={topology.RejectedRegionCount:N0}, " +
                $"validCells={topology.ValidCellCount:N0}, " +
                $"coveredCells={topology.CoveredCellCount:N0}, " +
                $"regions={topology.Regions.Count:N0}, " +
                $"prepared={topology.PreparedRegions.Count:N0}, " +
                $"anchors={topology.PreparedRecycleAnchorCount:N0}, " +
                $"fallbacks={topology.RecycleFallbackCount:N0}, " +
                $"rejections=[{topology.GetTopRejectionSummary(8)}].";
        }

        private static string BuildConnectorDetail(
            StylizedRiverFoamConnectorTopology topology)
        {
            return
                $"Connector: endpoints={topology.EligibleEndpointCount:N0}, " +
                $"attempts={topology.PathAttemptCount:N0}, " +
                $"accepted={topology.AcceptedConnectorCount:N0}, " +
                $"coveredCells={topology.CoveredCellCount:N0}, " +
                $"relationships={topology.Relationships.Count:N0}, " +
                $"preparedPaths={topology.PreparedPaths.Count:N0}, " +
                $"cataloguePaths=" +
                $"{topology.PreparedRelationshipCataloguePaths.Count:N0}, " +
                $"preparedPoints={topology.PreparedPathPointCount:N0}, " +
                $"variants={topology.PreparedPathVariantCount:N0}, " +
                $"unavailableVariants=" +
                $"{topology.UnavailablePathVariantCount:N0}, " +
                $"rejections=[{topology.GetTopRejectionSummary(4)}].";
        }

        private static string BuildPocketDetail(
            StylizedRiverFoamPocketTopology topology)
        {
            return
                $"Pocket: interior={topology.AcceptedInteriorPocketCount:N0}/" +
                $"{topology.InteriorCandidateCount:N0}, " +
                $"cavity={topology.AcceptedEdgeCavityCount:N0}/" +
                $"{topology.CavityCandidateCount:N0}, " +
                $"weak={topology.AcceptedConnectorWeakSpanCount:N0}/" +
                $"{topology.WeakSpanCandidateCount:N0}, " +
                $"freeWater={topology.AcceptedFreeWaterEventCount:N0}/" +
                $"{topology.FreeWaterCandidateCount:N0}, " +
                $"coveredCells={topology.CoveredCellCount:N0}, " +
                $"regions={topology.Regions.Count:N0}, " +
                $"preparedHosted=" +
                $"{topology.PreparedHostedRegions.Count:N0}, " +
                $"preparedFreeWater=" +
                $"{topology.PreparedFreeWaterRegions.Count:N0}, " +
                $"preparedWeakSpans=" +
                $"{topology.PreparedWeakSpanRegions.Count:N0}, " +
                $"rejections=[{topology.GetTopRejectionSummary(6)}].";
        }

        private static string BuildFloatFieldDetail(float[] values)
        {
            values ??= Array.Empty<float>();
            int nonzero = 0;
            double sum = 0.0;
            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int index = 0; index < values.Length; index++)
            {
                float value = values[index];
                if (value != 0f)
                {
                    nonzero++;
                }
                sum += value;
                minimum = Mathf.Min(minimum, value);
                maximum = Mathf.Max(maximum, value);
            }
            if (values.Length == 0)
            {
                minimum = 0f;
                maximum = 0f;
            }
            return
                $"count={values.Length:N0}, nonzero={nonzero:N0}, " +
                $"min={minimum:R}, max={maximum:R}, sum={sum:R}";
        }

        private static string BuildConnectorPathDetail(
            System.Collections.Generic.IReadOnlyList<
                StylizedRiverFoamConnectorPath> paths)
        {
            int nonnull = 0;
            int metricPoints = 0;
            int preparedPoints = 0;
            int variants = 0;
            int variantPoints = 0;
            for (int index = 0; index < paths.Count; index++)
            {
                StylizedRiverFoamConnectorPath path = paths[index];
                if (path == null)
                {
                    continue;
                }
                nonnull++;
                metricPoints += path.MetricPointData.Length;
                preparedPoints += path.PreparedMetricPointData.Length;
                variants += path.PathVariants.Count;
                for (int variantIndex = 0;
                     variantIndex < path.PathVariants.Count;
                     variantIndex++)
                {
                    StylizedRiverFoamConnectorPathVariant variant =
                        path.PathVariants[variantIndex];
                    if (variant != null)
                    {
                        variantPoints += variant.MetricPointData.Length;
                    }
                }
            }
            return
                $"slots={paths.Count:N0}, nonnull={nonnull:N0}, " +
                $"metricPoints={metricPoints:N0}, " +
                $"preparedPoints={preparedPoints:N0}, " +
                $"variants={variants:N0}, " +
                $"variantPoints={variantPoints:N0}";
        }

        private static string BuildPocketScalarDetail(
            StylizedRiverFoamPocketTopology topology)
        {
            return
                $"aggregate[{BuildFloatFieldDetail(topology.PressureData)}]; " +
                $"hosted[{BuildFloatFieldDetail(topology.HostedPressureData)}]; " +
                $"hostedFallback[" +
                $"{BuildFloatFieldDetail(topology.HostedFallbackPressureData)}]; " +
                $"independent[" +
                $"{BuildFloatFieldDetail(topology.IndependentPressureData)}]; " +
                $"staticIndependent[" +
                $"{BuildFloatFieldDetail(topology.StaticIndependentPressureData)}]; " +
                $"freeWater[" +
                $"{BuildFloatFieldDetail(topology.FreeWaterPressureData)}]";
        }
    }
}
#endif
