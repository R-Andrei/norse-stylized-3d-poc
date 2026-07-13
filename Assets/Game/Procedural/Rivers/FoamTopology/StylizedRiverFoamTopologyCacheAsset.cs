using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Storage-neutral result produced by the explicit Patch 4.9B cache build.
    /// The codec owns the binary format; this artifact only transports bytes
    /// and diagnostics to an Editor or future procedural-run storage provider.
    /// </summary>
    public readonly struct StylizedRiverFoamTopologyCacheBuildArtifact
    {
        internal StylizedRiverFoamTopologyCacheBuildArtifact(
            byte[] payload,
            int formatVersion,
            int generatorContractVersion,
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
        public int PayloadFormatVersion => payloadFormatVersion;
        public int GeneratorContractVersion => generatorContractVersion;
        public string SourceRiverName => sourceRiverName;
        public string BuiltUtc => builtUtc;
        public string DomainFingerprint => domainFingerprint;
        public string ObstacleFingerprint => obstacleFingerprint;
        public string GenerationFingerprint => generationFingerprint;
        public string CombinedFingerprint => combinedFingerprint;
        public string PayloadHash => payloadHash;

        public void StoreBuild(
            StylizedRiverFoamTopologyCacheBuildArtifact artifact)
        {
            storageContractVersion = StorageContractVersion;
            payloadFormatVersion = artifact.FormatVersion;
            generatorContractVersion = artifact.GeneratorContractVersion;
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
