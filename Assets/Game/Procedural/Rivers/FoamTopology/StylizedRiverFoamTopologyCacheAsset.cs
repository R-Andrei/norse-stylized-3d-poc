using System;
#if UNITY_EDITOR
using System.Text;
#endif
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Storage-neutral result produced by explicit Foam cache preparation.
    /// The codec owns the binary format; this artifact only transports bytes
    /// and diagnostics to an Editor or future procedural-run storage provider.
    /// </summary>
    public readonly struct StylizedRiverFoamTopologyCacheBuildArtifact
    {
        internal StylizedRiverFoamTopologyCacheBuildArtifact(
            byte[] payload,
            int formatVersion,
            int generatorContractVersion,
            StylizedRiverFoamGridDescriptor gridDescriptor,
            string domainFingerprint,
            string obstacleFingerprint,
            string generationFingerprint,
            string combinedFingerprint,
            string payloadHash,
            double buildMilliseconds,
            string sourceRiverName)
        {
            // The codec creates a fresh immutable payload for this artifact.
            // Keep ownership here and clone only when the ScriptableObject
            // stores it. This removes one full-payload copy from every
            // explicit cache preparation without exposing mutable bytes to
            // public callers.
            Payload = payload ?? Array.Empty<byte>();
            FormatVersion = formatVersion;
            GeneratorContractVersion = generatorContractVersion;
            GridDescriptor = gridDescriptor;
            DomainFingerprint = domainFingerprint ?? string.Empty;
            ObstacleFingerprint = obstacleFingerprint ?? string.Empty;
            GenerationFingerprint = generationFingerprint ?? string.Empty;
            CombinedFingerprint = combinedFingerprint ?? string.Empty;
            PayloadHash = payloadHash ?? string.Empty;
            BuildMilliseconds = Math.Max(0.0, buildMilliseconds);
            SourceRiverName = sourceRiverName ?? string.Empty;
        }

        internal byte[] Payload { get; }
        public int PayloadByteCount => Payload?.Length ?? 0;
        public int FormatVersion { get; }
        public int GeneratorContractVersion { get; }
        internal StylizedRiverFoamGridDescriptor GridDescriptor { get; }
        public int GridDescriptorContractVersion =>
            StylizedRiverFoamGridDescriptor.DescriptorContractVersion;
        public int GridMappingValue => (int)GridDescriptor.Mapping;
        public int GridMappingContractVersion =>
            GridDescriptor.MappingContractVersion;
        public string GridInitializationSignature =>
            GridDescriptor.InitializationSignature.ToString("X16");
        public string DomainFingerprint { get; }
        public string ObstacleFingerprint { get; }
        public string GenerationFingerprint { get; }
        public string CombinedFingerprint { get; }
        public string PayloadHash { get; }
        public double BuildMilliseconds { get; }
        public string SourceRiverName { get; }

        internal byte[] CopyPayload()
        {
            return Payload != null
                ? (byte[])Payload.Clone()
                : Array.Empty<byte>();
        }
    }

    /// <summary>
    /// One runtime-readable provider for a versioned Foam topology payload.
    /// The asset does not interpret or generate payload bytes, so the binary
    /// cache contract remains usable by future chunk/run files or other storage
    /// providers without depending on UnityEditor or this ScriptableObject.
    /// </summary>
    [PreferBinarySerialization]
    [CreateAssetMenu(
        fileName = "RiverFoamTopologyCache",
        menuName = "Programmatic Stylized 3D/Rivers/Foam Topology Cache")]
    public sealed class StylizedRiverFoamTopologyCacheAsset : ScriptableObject
    {
        private const int StorageContractVersion = 1;

        [SerializeField, HideInInspector]
        private int storageContractVersion = StorageContractVersion;
        [SerializeField, HideInInspector] private int payloadFormatVersion;
        [SerializeField, HideInInspector] private int generatorContractVersion;
        [SerializeField, HideInInspector]
        private int gridDescriptorContractVersion;
        [SerializeField, HideInInspector] private int gridMappingValue;
        [SerializeField, HideInInspector]
        private int gridMappingContractVersion;
        [SerializeField, HideInInspector]
        private string gridInitializationSignature = "";
        [SerializeField, HideInInspector] private string sourceRiverName = "";
        [SerializeField, HideInInspector] private string builtUtc = "";
        [SerializeField, HideInInspector] private string domainFingerprint = "";
        [SerializeField, HideInInspector] private string obstacleFingerprint = "";
        [SerializeField, HideInInspector] private string generationFingerprint = "";
        [SerializeField, HideInInspector] private string combinedFingerprint = "";
        [SerializeField, HideInInspector] private string payloadHash = "";
        [SerializeField, HideInInspector] private byte[] payload = Array.Empty<byte>();

        public bool HasPayload => payload != null && payload.Length > 0;
        public int PayloadByteCount => payload?.Length ?? 0;
#if UNITY_EDITOR
        public int StorageContractVersionValue => storageContractVersion;
#endif
        public int PayloadFormatVersion => payloadFormatVersion;
        public int GeneratorContractVersion => generatorContractVersion;
        public int GridDescriptorContractVersion =>
            gridDescriptorContractVersion;
        public int GridMappingValue => gridMappingValue;
#if UNITY_EDITOR
        public string GridMappingDisplayName => gridMappingValue switch
        {
            (int)StylizedRiverFoamGridMapping.LegacyNormalizedAcross =>
                "Legacy Normalized Across",
            (int)StylizedRiverFoamGridMapping.FixedMetricLattice =>
                "Fixed Metric Lattice",
            _ => $"Unknown ({gridMappingValue})"
        };
#endif
        public int GridMappingContractVersion =>
            gridMappingContractVersion;
        public string GridInitializationSignature =>
            gridInitializationSignature;
        public string SourceRiverName => sourceRiverName;
        public string BuiltUtc => builtUtc;
        public string DomainFingerprint => domainFingerprint;
        public string ObstacleFingerprint => obstacleFingerprint;
        public string GenerationFingerprint => generationFingerprint;
        public string CombinedFingerprint => combinedFingerprint;
        public string PayloadHash => payloadHash;

#if UNITY_EDITOR
        public bool TryGetPayloadDiagnosticReport(
            out string report,
            out string error)
        {
            report = string.Empty;
            error = string.Empty;
            if (!HasPayload)
            {
                error = "The cache asset contains no payload bytes.";
                return false;
            }

            if (!StylizedRiverFoamTopologyCacheCodec
                    .TryCreateDiagnosticSnapshot(
                        GetPayloadReadOnlyReference(),
                        "Selected Cache Asset",
                        out StylizedRiverFoamCacheDiagnosticSnapshot snapshot,
                        out error))
            {
                return false;
            }

            StringBuilder builder = new(8192);
            builder.AppendLine("CACHE PAYLOAD SECTION DIGESTS");
            builder.AppendLine(
                $"Descriptor: v{StylizedRiverFoamGridDescriptor.DescriptorContractVersion} / " +
                $"mapping-{(int)snapshot.Descriptor.Mapping}-v" +
                $"{snapshot.Descriptor.MappingContractVersion} / " +
                $"{snapshot.Descriptor.InitializationSignature:X16}");
            builder.AppendLine(
                $"Structural field: {snapshot.Descriptor.ColumnCount:N0} x " +
                $"{snapshot.Descriptor.RowCount:N0}");
            builder.AppendLine(
                $"Resolved spacing: dx={snapshot.Descriptor.ResolvedDxMetres:R} m, " +
                $"dy={snapshot.Descriptor.ResolvedDyMetres:R} m");
            builder.AppendLine(snapshot.TopologySummary);
            for (int index = 0; index < snapshot.Sections.Count; index++)
            {
                StylizedRiverFoamCacheDiagnosticSection section =
                    snapshot.Sections[index];
                builder.AppendLine(
                    $"{section.Name}: {section.ByteCount:N0} bytes / " +
                    $"{section.Hash:X16}");
                builder.AppendLine($"  {section.Detail}");
            }
            report = builder.ToString();
            return true;
        }

#endif

        public void StoreBuild(
            StylizedRiverFoamTopologyCacheBuildArtifact artifact)
        {
            storageContractVersion = StorageContractVersion;
            payloadFormatVersion = artifact.FormatVersion;
            generatorContractVersion = artifact.GeneratorContractVersion;
            gridDescriptorContractVersion =
                artifact.GridDescriptorContractVersion;
            gridMappingValue = artifact.GridMappingValue;
            gridMappingContractVersion =
                artifact.GridMappingContractVersion;
            gridInitializationSignature =
                artifact.GridInitializationSignature;
            sourceRiverName = artifact.SourceRiverName;
            builtUtc = DateTime.UtcNow.ToString("O");
            domainFingerprint = artifact.DomainFingerprint;
            obstacleFingerprint = artifact.ObstacleFingerprint;
            generationFingerprint = artifact.GenerationFingerprint;
            combinedFingerprint = artifact.CombinedFingerprint;
            payloadHash = artifact.PayloadHash;
            payload = artifact.CopyPayload();
        }

        public bool MatchesStoredBuild(
            StylizedRiverFoamTopologyCacheBuildArtifact artifact,
            out string summary)
        {
            bool metadataMatches =
                storageContractVersion == StorageContractVersion &&
                payloadFormatVersion == artifact.FormatVersion &&
                generatorContractVersion ==
                    artifact.GeneratorContractVersion &&
                gridDescriptorContractVersion ==
                    artifact.GridDescriptorContractVersion &&
                gridMappingValue == artifact.GridMappingValue &&
                gridMappingContractVersion ==
                    artifact.GridMappingContractVersion &&
                gridInitializationSignature ==
                    artifact.GridInitializationSignature &&
                sourceRiverName == artifact.SourceRiverName &&
                domainFingerprint == artifact.DomainFingerprint &&
                obstacleFingerprint == artifact.ObstacleFingerprint &&
                generationFingerprint == artifact.GenerationFingerprint &&
                combinedFingerprint == artifact.CombinedFingerprint &&
                payloadHash == artifact.PayloadHash;
            if (!metadataMatches)
            {
                summary =
                    "The stored cache metadata does not match the prepared " +
                    "build artifact.";
                return false;
            }

            byte[] preparedPayload = artifact.Payload;
            if (payload == null || preparedPayload == null ||
                payload.Length != preparedPayload.Length)
            {
                summary =
                    "The stored payload length does not match the prepared " +
                    "build artifact.";
                return false;
            }

            for (int index = 0; index < payload.Length; index++)
            {
                if (payload[index] == preparedPayload[index])
                {
                    continue;
                }

                summary =
                    "The stored payload differs from the prepared build at " +
                    $"byte {index:N0}.";
                return false;
            }

            summary =
                "Stored metadata and payload bytes exactly match the prepared " +
                "build artifact.";
            return true;
        }

        internal bool TryValidateStoredContract(
            out bool legacyNormalizedCoordinateContract,
            out bool legacyObstacleFingerprintContract,
            out bool legacyDynamicTopologyPhaseContract,
            out string summary)
        {
            legacyNormalizedCoordinateContract = false;
            legacyObstacleFingerprintContract = false;
            legacyDynamicTopologyPhaseContract = false;
            if (!HasSupportedStorageContract)
            {
                summary =
                    "The assigned asset uses an unsupported storage contract.";
                return false;
            }
            if (!StylizedRiverFoamTopologyCacheCodec
                    .TryValidateContractVersions(
                        payloadFormatVersion,
                        generatorContractVersion,
                        out legacyNormalizedCoordinateContract,
                        out legacyObstacleFingerprintContract,
                        out legacyDynamicTopologyPhaseContract,
                        out summary))
            {
                return false;
            }
            if (gridDescriptorContractVersion !=
                StylizedRiverFoamGridDescriptor.DescriptorContractVersion)
            {
                summary =
                    $"Unsupported Foam grid descriptor contract " +
                    $"{gridDescriptorContractVersion}; expected " +
                    $"{StylizedRiverFoamGridDescriptor.DescriptorContractVersion}.";
                return false;
            }
            if (!Enum.IsDefined(
                    typeof(StylizedRiverFoamGridMapping),
                    gridMappingValue))
            {
                summary =
                    $"Unknown stored Foam grid mapping value " +
                    $"{gridMappingValue}.";
                return false;
            }

            StylizedRiverFoamGridMapping mapping =
                (StylizedRiverFoamGridMapping)gridMappingValue;
            int expectedMappingContract = mapping ==
                StylizedRiverFoamGridMapping.FixedMetricLattice
                    ? StylizedRiverFoamGridDescriptor
                        .FixedMetricMappingContractVersion
                    : StylizedRiverFoamGridDescriptor
                        .LegacyMappingContractVersion;
            if (gridMappingContractVersion != expectedMappingContract ||
                string.IsNullOrEmpty(gridInitializationSignature))
            {
                summary =
                    "The cache asset is missing exact Foam grid mapping " +
                    "metadata. Rebuild it in Edit Mode.";
                return false;
            }

            summary = string.Empty;
            return true;
        }

        public byte[] GetPayloadCopy()
        {
            return payload != null
                ? (byte[])payload.Clone()
                : Array.Empty<byte>();
        }

        internal byte[] GetPayloadReadOnlyReference()
        {
            // Runtime cache readers validate and deserialize without mutating
            // the source byte array. Keeping this internal avoids a full
            // payload clone on every cache resolution or strict validation.
            return payload ?? Array.Empty<byte>();
        }

        internal bool HasSupportedStorageContract =>
            storageContractVersion == StorageContractVersion;
    }
}
