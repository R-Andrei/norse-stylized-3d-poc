using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Immutable Patch 4.9A payload source. It contains the complete accepted
    /// prepared topology graph plus the exact scalar obstacle mask used by the
    /// generators. Storage ownership and persistent input fingerprints remain
    /// deliberately deferred to Patch 4.9B.
    /// </summary>
    internal sealed class StylizedRiverFoamTopologyCachePackage
    {
        public StylizedRiverFoamTopologyCachePackage(
            int fieldWidth,
            int fieldHeight,
            float fieldLength,
            float validFieldLength,
            float domainGlobalDistanceMinimum,
            float domainGlobalDistanceMaximum,
            float domainLocalLength,
            float domainRequestedSampleSpacing,
            bool domainReverseFlow,
            int domainSampleCount,
            StylizedRiverQuality quality,
            float shoreMotion,
            int majorSeed,
            float majorAmount,
            float majorSize,
            float majorSizeVariation,
            float majorRecycleTerritoryDeviationPercent,
            float connectorAmount,
            float connectorDirectness,
            float connectorLengthPreference,
            float interiorPocketAmount,
            float edgeCavityAmount,
            float connectorWeakSpanAmount,
            float freeWaterEventAmount,
            float[] obstacleExclusion,
            StylizedRiverFoamMajorTopology majorTopology,
            StylizedRiverFoamConnectorTopology connectorTopology,
            StylizedRiverFoamPocketTopology pocketTopology)
        {
            FieldWidth = fieldWidth;
            FieldHeight = fieldHeight;
            FieldLength = fieldLength;
            ValidFieldLength = validFieldLength;
            DomainGlobalDistanceMinimum = domainGlobalDistanceMinimum;
            DomainGlobalDistanceMaximum = domainGlobalDistanceMaximum;
            DomainLocalLength = domainLocalLength;
            DomainRequestedSampleSpacing = domainRequestedSampleSpacing;
            DomainReverseFlow = domainReverseFlow;
            DomainSampleCount = domainSampleCount;
            Quality = quality;
            ShoreMotion = shoreMotion;
            MajorSeed = majorSeed;
            MajorAmount = majorAmount;
            MajorSize = majorSize;
            MajorSizeVariation = majorSizeVariation;
            MajorRecycleTerritoryDeviationPercent =
                majorRecycleTerritoryDeviationPercent;
            ConnectorAmount = connectorAmount;
            ConnectorDirectness = connectorDirectness;
            ConnectorLengthPreference = connectorLengthPreference;
            InteriorPocketAmount = interiorPocketAmount;
            EdgeCavityAmount = edgeCavityAmount;
            ConnectorWeakSpanAmount = connectorWeakSpanAmount;
            FreeWaterEventAmount = freeWaterEventAmount;
            ObstacleExclusion = obstacleExclusion != null
                ? (float[])obstacleExclusion.Clone()
                : Array.Empty<float>();
            MajorTopology = majorTopology;
            ConnectorTopology = connectorTopology;
            PocketTopology = pocketTopology;
        }

        public int FieldWidth { get; }
        public int FieldHeight { get; }
        public float FieldLength { get; }
        public float ValidFieldLength { get; }
        public float DomainGlobalDistanceMinimum { get; }
        public float DomainGlobalDistanceMaximum { get; }
        public float DomainLocalLength { get; }
        public float DomainRequestedSampleSpacing { get; }
        public bool DomainReverseFlow { get; }
        public int DomainSampleCount { get; }
        public StylizedRiverQuality Quality { get; }
        public float ShoreMotion { get; }
        public int MajorSeed { get; }
        public float MajorAmount { get; }
        public float MajorSize { get; }
        public float MajorSizeVariation { get; }
        public float MajorRecycleTerritoryDeviationPercent { get; }
        public float ConnectorAmount { get; }
        public float ConnectorDirectness { get; }
        public float ConnectorLengthPreference { get; }
        public float InteriorPocketAmount { get; }
        public float EdgeCavityAmount { get; }
        public float ConnectorWeakSpanAmount { get; }
        public float FreeWaterEventAmount { get; }
        public float[] ObstacleExclusion { get; }
        public StylizedRiverFoamMajorTopology MajorTopology { get; }
        public StylizedRiverFoamConnectorTopology ConnectorTopology { get; }
        public StylizedRiverFoamPocketTopology PocketTopology { get; }
    }

    internal readonly struct StylizedRiverFoamTopologyCacheRoundTripResult
    {
        public StylizedRiverFoamTopologyCacheRoundTripResult(
            bool passed,
            string summary,
            int payloadByteCount,
            ulong payloadHash,
            double serializationMilliseconds,
            double loadMilliseconds,
            double verificationMilliseconds)
        {
            Passed = passed;
            Summary = summary ?? string.Empty;
            PayloadByteCount = Mathf.Max(0, payloadByteCount);
            PayloadHash = payloadHash;
            SerializationMilliseconds = Math.Max(
                0.0,
                serializationMilliseconds);
            LoadMilliseconds = Math.Max(0.0, loadMilliseconds);
            VerificationMilliseconds = Math.Max(
                0.0,
                verificationMilliseconds);
        }

        public bool Passed { get; }
        public string Summary { get; }
        public int PayloadByteCount { get; }
        public ulong PayloadHash { get; }
        public double SerializationMilliseconds { get; }
        public double LoadMilliseconds { get; }
        public double VerificationMilliseconds { get; }
    }

    /// <summary>
    /// Patch 4.9A deterministic binary contract for the complete prepared Foam
    /// topology graph. The format intentionally uses exact 32-bit scalar values
    /// and explicit bounded counts. It is not yet a production cache provider:
    /// persistent storage, stable domain/obstacle fingerprints, and cache-first
    /// startup remain later 4.9 patches.
    /// </summary>
    internal static class StylizedRiverFoamTopologyCacheCodec
    {
        private const uint Magic = 0x31434652u; // "RFC1" in little-endian bytes.
        internal const int FormatVersion = 1;
        internal const int GeneratorContractVersion = 1;

        private const int ChecksumByteCount = sizeof(ulong);
        private const int MaximumFieldDimension = 8192;
        private const int MaximumFieldCellCount = 16 * 1024 * 1024;
        private const int MaximumCollectionCount = 4 * 1024 * 1024;
        private const int MaximumLocalMaskCellCount = 1024 * 1024;

        public static byte[] Serialize(
            StylizedRiverFoamTopologyCachePackage package)
        {
            ValidatePackage(package);

            byte[] body;
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(
                       stream,
                       Encoding.UTF8,
                       true))
            {
                writer.Write(Magic);
                writer.Write(FormatVersion);
                writer.Write(GeneratorContractVersion);
                WritePackage(writer, package);
                writer.Flush();
                body = stream.ToArray();
            }

            ulong checksum = ComputeChecksum(body, 0, body.Length);
            byte[] result = new byte[body.Length + ChecksumByteCount];
            Buffer.BlockCopy(body, 0, result, 0, body.Length);
            WriteUInt64LittleEndian(result, body.Length, checksum);
            return result;
        }

        public static bool TryDeserialize(
            byte[] payload,
            out StylizedRiverFoamTopologyCachePackage package,
            out string error)
        {
            package = null;
            error = string.Empty;

            try
            {
                if (payload == null || payload.Length < 20)
                {
                    throw new InvalidDataException(
                        "Payload is missing or too small for the cache header.");
                }

                int bodyLength = payload.Length - ChecksumByteCount;
                ulong storedChecksum = ReadUInt64LittleEndian(
                    payload,
                    bodyLength);
                ulong calculatedChecksum = ComputeChecksum(
                    payload,
                    0,
                    bodyLength);
                if (storedChecksum != calculatedChecksum)
                {
                    throw new InvalidDataException(
                        "Payload checksum mismatch; the cache is corrupt or truncated.");
                }

                using MemoryStream stream = new MemoryStream(
                    payload,
                    0,
                    bodyLength,
                    false,
                    true);
                using BinaryReader reader = new BinaryReader(
                    stream,
                    Encoding.UTF8,
                    true);

                uint magic = reader.ReadUInt32();
                if (magic != Magic)
                {
                    throw new InvalidDataException(
                        "Payload magic does not identify a Foam topology cache.");
                }

                int formatVersion = reader.ReadInt32();
                if (formatVersion != FormatVersion)
                {
                    throw new InvalidDataException(
                        $"Unsupported Foam topology cache format {formatVersion}; " +
                        $"expected {FormatVersion}.");
                }

                int generatorVersion = reader.ReadInt32();
                if (generatorVersion != GeneratorContractVersion)
                {
                    throw new InvalidDataException(
                        $"Unsupported Foam topology generator contract " +
                        $"{generatorVersion}; expected " +
                        $"{GeneratorContractVersion}.");
                }

                package = ReadPackage(reader);
                if (stream.Position != bodyLength)
                {
                    throw new InvalidDataException(
                        $"Payload contains {bodyLength - stream.Position} " +
                        "unexpected trailing byte(s).");
                }

                ValidatePackage(package);
                return true;
            }
            catch (Exception exception) when (
                exception is EndOfStreamException ||
                exception is IOException ||
                exception is InvalidDataException ||
                exception is ArgumentException ||
                exception is OverflowException)
            {
                package = null;
                error = exception.Message;
                return false;
            }
        }

        public static StylizedRiverFoamTopologyCacheRoundTripResult
            ValidateRoundTrip(
                StylizedRiverFoamTopologyCachePackage source)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            byte[] payload = Serialize(source);
            stopwatch.Stop();
            double serializationMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            ulong payloadHash = ReadUInt64LittleEndian(
                payload,
                payload.Length - ChecksumByteCount);

            stopwatch.Restart();
            bool loaded = TryDeserialize(
                payload,
                out StylizedRiverFoamTopologyCachePackage reconstructed,
                out string loadError);
            stopwatch.Stop();
            double loadMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            if (!loaded)
            {
                return new StylizedRiverFoamTopologyCacheRoundTripResult(
                    false,
                    $"Load failed: {loadError}",
                    payload.Length,
                    payloadHash,
                    serializationMilliseconds,
                    loadMilliseconds,
                    0.0);
            }

            stopwatch.Restart();
            byte[] repeatedPayload = Serialize(source);
            if (!ByteArraysEqual(payload, repeatedPayload))
            {
                stopwatch.Stop();
                return new StylizedRiverFoamTopologyCacheRoundTripResult(
                    false,
                    "Serializing the same immutable topology twice produced " +
                    "different bytes.",
                    payload.Length,
                    payloadHash,
                    serializationMilliseconds,
                    loadMilliseconds,
                    stopwatch.Elapsed.TotalMilliseconds);
            }

            byte[] reconstructedPayload = Serialize(reconstructed);
            if (!ByteArraysEqual(payload, reconstructedPayload))
            {
                stopwatch.Stop();
                return new StylizedRiverFoamTopologyCacheRoundTripResult(
                    false,
                    "The reconstructed topology did not reproduce the original " +
                    "exact payload bytes.",
                    payload.Length,
                    payloadHash,
                    serializationMilliseconds,
                    loadMilliseconds,
                    stopwatch.Elapsed.TotalMilliseconds);
            }

            if (!GeneratedChannelsEqual(source, reconstructed, out string channelError))
            {
                stopwatch.Stop();
                return new StylizedRiverFoamTopologyCacheRoundTripResult(
                    false,
                    channelError,
                    payload.Length,
                    payloadHash,
                    serializationMilliseconds,
                    loadMilliseconds,
                    stopwatch.Elapsed.TotalMilliseconds);
            }

            if (!CorruptionIsRejected(payload, out string corruptionError))
            {
                stopwatch.Stop();
                return new StylizedRiverFoamTopologyCacheRoundTripResult(
                    false,
                    corruptionError,
                    payload.Length,
                    payloadHash,
                    serializationMilliseconds,
                    loadMilliseconds,
                    stopwatch.Elapsed.TotalMilliseconds);
            }

            stopwatch.Stop();
            return new StylizedRiverFoamTopologyCacheRoundTripResult(
                true,
                "Exact payload bytes, obstacle mask, prepared identities, " +
                "path catalogues, local masks, evolution descriptors, and " +
                "initial generated channels survived the round trip. A " +
                "deliberately corrupted copy was rejected neutrally.",
                payload.Length,
                payloadHash,
                serializationMilliseconds,
                loadMilliseconds,
                stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void WritePackage(
            BinaryWriter writer,
            StylizedRiverFoamTopologyCachePackage package)
        {
            writer.Write(package.FieldWidth);
            writer.Write(package.FieldHeight);
            writer.Write(package.FieldLength);
            writer.Write(package.ValidFieldLength);
            writer.Write(package.DomainGlobalDistanceMinimum);
            writer.Write(package.DomainGlobalDistanceMaximum);
            writer.Write(package.DomainLocalLength);
            writer.Write(package.DomainRequestedSampleSpacing);
            writer.Write(package.DomainReverseFlow);
            writer.Write(package.DomainSampleCount);
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
            WriteFloatArray(writer, package.ObstacleExclusion);
            WriteMajorTopology(writer, package.MajorTopology);
            WriteConnectorTopology(writer, package.ConnectorTopology);
            WritePocketTopology(writer, package.PocketTopology);
        }

        private static StylizedRiverFoamTopologyCachePackage ReadPackage(
            BinaryReader reader)
        {
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            ValidateFieldDimensions(width, height);
            int cellCount = checked(width * height);

            float fieldLength = reader.ReadSingle();
            float validFieldLength = reader.ReadSingle();
            float globalMinimum = reader.ReadSingle();
            float globalMaximum = reader.ReadSingle();
            float localLength = reader.ReadSingle();
            float requestedSpacing = reader.ReadSingle();
            bool reverseFlow = reader.ReadBoolean();
            int sampleCount = ReadBoundedNonNegative(
                reader,
                MaximumCollectionCount,
                "domain sample count");
            int qualityValue = reader.ReadInt32();
            if (!Enum.IsDefined(typeof(StylizedRiverQuality), qualityValue))
            {
                throw new InvalidDataException(
                    $"Unknown river quality value {qualityValue}.");
            }

            StylizedRiverQuality quality = (StylizedRiverQuality)qualityValue;
            float shoreMotion = reader.ReadSingle();
            int majorSeed = reader.ReadInt32();
            float majorAmount = reader.ReadSingle();
            float majorSize = reader.ReadSingle();
            float majorSizeVariation = reader.ReadSingle();
            float recycleDeviation = reader.ReadSingle();
            float connectorAmount = reader.ReadSingle();
            float connectorDirectness = reader.ReadSingle();
            float connectorLengthPreference = reader.ReadSingle();
            float interiorAmount = reader.ReadSingle();
            float cavityAmount = reader.ReadSingle();
            float weakSpanAmount = reader.ReadSingle();
            float freeWaterAmount = reader.ReadSingle();
            float[] obstacle = ReadFloatArrayExact(
                reader,
                cellCount,
                "obstacle scalar field");
            StylizedRiverFoamMajorTopology major = ReadMajorTopology(
                reader,
                width,
                height,
                cellCount);
            StylizedRiverFoamConnectorTopology connector =
                ReadConnectorTopology(reader, width, height, cellCount);
            StylizedRiverFoamPocketTopology pocket = ReadPocketTopology(
                reader,
                width,
                height,
                cellCount);

            return new StylizedRiverFoamTopologyCachePackage(
                width,
                height,
                fieldLength,
                validFieldLength,
                globalMinimum,
                globalMaximum,
                localLength,
                requestedSpacing,
                reverseFlow,
                sampleCount,
                quality,
                shoreMotion,
                majorSeed,
                majorAmount,
                majorSize,
                majorSizeVariation,
                recycleDeviation,
                connectorAmount,
                connectorDirectness,
                connectorLengthPreference,
                interiorAmount,
                cavityAmount,
                weakSpanAmount,
                freeWaterAmount,
                obstacle,
                major,
                connector,
                pocket);
        }

        private static void WriteMajorTopology(
            BinaryWriter writer,
            StylizedRiverFoamMajorTopology topology)
        {
            writer.Write(topology.Width);
            writer.Write(topology.Height);
            writer.Write(topology.AttemptedOpportunityCount);
            writer.Write(topology.AcceptedRegionCount);
            writer.Write(topology.RejectedRegionCount);
            writer.Write(topology.ValidCellCount);
            writer.Write(topology.CoveredCellCount);
            writer.Write(topology.PreparedRecycleAnchorCount);
            writer.Write(topology.RecycleFallbackCount);
            WriteFloatArray(writer, topology.SupportData);

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

            int rejectionCount = Enum.GetValues(
                typeof(StylizedRiverFoamMajorPlacementRejectionReason)).Length;
            writer.Write(rejectionCount);
            for (int index = 0; index < rejectionCount; index++)
            {
                writer.Write(topology.GetRejectionCount(
                    (StylizedRiverFoamMajorPlacementRejectionReason)index));
            }
        }

        private static StylizedRiverFoamMajorTopology ReadMajorTopology(
            BinaryReader reader,
            int expectedWidth,
            int expectedHeight,
            int cellCount)
        {
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            RequireDimensions(width, height, expectedWidth, expectedHeight, "Major");
            int attempted = ReadCount(reader, "Major attempted opportunities");
            int accepted = ReadCount(reader, "Major accepted regions");
            int rejected = ReadCount(reader, "Major rejected regions");
            int validCells = ReadBoundedNonNegative(
                reader,
                cellCount,
                "Major valid cell count");
            int coveredCells = ReadBoundedNonNegative(
                reader,
                cellCount,
                "Major covered cell count");
            int preparedAnchorCount = ReadCount(
                reader,
                "Major prepared recycle anchor count");
            int fallbackCount = ReadCount(
                reader,
                "Major recycle fallback count");
            float[] support = ReadFloatArrayExact(
                reader,
                cellCount,
                "Major support field");

            int regionCount = ReadCount(reader, "Major region count");
            StylizedRiverFoamMajorRegion[] regions =
                new StylizedRiverFoamMajorRegion[regionCount];
            for (int index = 0; index < regionCount; index++)
            {
                regions[index] = new StylizedRiverFoamMajorRegion(
                    reader.ReadUInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadSingle(),
                    reader.ReadInt32(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadUInt32(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
            }

            int preparedCount = ReadCount(reader, "prepared Major region count");
            StylizedRiverFoamPreparedMajorRegion[] preparedRegions =
                new StylizedRiverFoamPreparedMajorRegion[preparedCount];
            for (int index = 0; index < preparedCount; index++)
            {
                if (!reader.ReadBoolean())
                {
                    continue;
                }

                uint stableId = reader.ReadUInt32();
                int resolution = ReadMaskResolution(reader, "Major mask");
                float[] localSupport = ReadFloatArrayExact(
                    reader,
                    checked(resolution * resolution),
                    "Major local support mask");
                Vector2 centroid = ReadVector2(reader);
                float principalAngle = reader.ReadSingle();
                float majorHalfExtent = reader.ReadSingle();
                float minorHalfExtent = reader.ReadSingle();
                int anchorCount = ReadCount(reader, "Major recycle anchor count");
                StylizedRiverFoamMajorRecycleAnchor[] anchors =
                    new StylizedRiverFoamMajorRecycleAnchor[anchorCount];
                for (int anchorIndex = 0;
                     anchorIndex < anchorCount;
                     anchorIndex++)
                {
                    anchors[anchorIndex] =
                        new StylizedRiverFoamMajorRecycleAnchor(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadBoolean());
                }

                preparedRegions[index] = new StylizedRiverFoamPreparedMajorRegion(
                    stableId,
                    resolution,
                    localSupport,
                    centroid,
                    principalAngle,
                    majorHalfExtent,
                    minorHalfExtent,
                    anchors);
            }

            int[] rejectionCounts = ReadRejectionCounts(
                reader,
                Enum.GetValues(
                    typeof(StylizedRiverFoamMajorPlacementRejectionReason)).Length,
                "Major");
            return new StylizedRiverFoamMajorTopology(
                width,
                height,
                attempted,
                accepted,
                rejected,
                validCells,
                coveredCells,
                0.0,
                support,
                regions,
                preparedRegions,
                preparedAnchorCount,
                fallbackCount,
                rejectionCounts);
        }

        private static void WriteConnectorTopology(
            BinaryWriter writer,
            StylizedRiverFoamConnectorTopology topology)
        {
            writer.Write(topology.Width);
            writer.Write(topology.Height);
            writer.Write(topology.EligibleEndpointCount);
            writer.Write(topology.PathAttemptCount);
            writer.Write(topology.AcceptedConnectorCount);
            writer.Write(topology.CoveredCellCount);
            WriteFloatArray(writer, topology.SupportData);

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

            WriteConnectorPaths(writer, topology.PreparedPaths);
            WriteConnectorPaths(
                writer,
                topology.PreparedRelationshipCataloguePaths);

            int rejectionCount = Enum.GetValues(
                typeof(StylizedRiverFoamConnectorRejectionReason)).Length;
            writer.Write(rejectionCount);
            for (int index = 0; index < rejectionCount; index++)
            {
                writer.Write(topology.GetRejectionCount(
                    (StylizedRiverFoamConnectorRejectionReason)index));
            }
        }

        private static StylizedRiverFoamConnectorTopology ReadConnectorTopology(
            BinaryReader reader,
            int expectedWidth,
            int expectedHeight,
            int cellCount)
        {
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            RequireDimensions(
                width,
                height,
                expectedWidth,
                expectedHeight,
                "Connector");
            int eligibleEndpoints = ReadCount(
                reader,
                "Connector eligible endpoint count");
            int attempts = ReadCount(reader, "Connector path attempt count");
            int accepted = ReadCount(reader, "Connector accepted count");
            int covered = ReadBoundedNonNegative(
                reader,
                cellCount,
                "Connector covered cell count");
            float[] support = ReadFloatArrayExact(
                reader,
                cellCount,
                "Connector support field");

            int relationshipCount = ReadCount(
                reader,
                "Connector relationship count");
            StylizedRiverFoamConnectorRelationship[] relationships =
                new StylizedRiverFoamConnectorRelationship[relationshipCount];
            for (int index = 0; index < relationshipCount; index++)
            {
                relationships[index] =
                    new StylizedRiverFoamConnectorRelationship(
                        reader.ReadUInt32(),
                        reader.ReadUInt32(),
                        reader.ReadUInt32(),
                        reader.ReadUInt32(),
                        reader.ReadUInt32(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadUInt32(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle());
            }

            StylizedRiverFoamConnectorPath[] paths = ReadConnectorPaths(reader);
            StylizedRiverFoamConnectorPath[] catalogue = ReadConnectorPaths(reader);
            int[] rejectionCounts = ReadRejectionCounts(
                reader,
                Enum.GetValues(
                    typeof(StylizedRiverFoamConnectorRejectionReason)).Length,
                "Connector");

            return new StylizedRiverFoamConnectorTopology(
                width,
                height,
                eligibleEndpoints,
                attempts,
                accepted,
                covered,
                0.0,
                support,
                relationships,
                paths,
                catalogue,
                rejectionCounts);
        }

        private static void WriteConnectorPaths(
            BinaryWriter writer,
            System.Collections.Generic.IReadOnlyList<
                StylizedRiverFoamConnectorPath> paths)
        {
            writer.Write(paths.Count);
            for (int index = 0; index < paths.Count; index++)
            {
                StylizedRiverFoamConnectorPath path = paths[index];
                writer.Write(path != null);
                if (path == null)
                {
                    continue;
                }

                writer.Write(path.StableId);
                WriteVector2Array(writer, path.MetricPointData);
                WriteVector2Array(writer, path.PreparedMetricPointData);
                WriteFloatArray(writer, path.NormalizedCumulativeLengthData);
                WriteEndpointBinding(writer, path.StartEndpointBinding);
                WriteEndpointBinding(writer, path.EndEndpointBinding);
                writer.Write(path.StartRecycleAnchorCount);
                writer.Write(path.EndRecycleAnchorCount);
                writer.Write(path.PathVariants.Count);
                for (int variantIndex = 0;
                     variantIndex < path.PathVariants.Count;
                     variantIndex++)
                {
                    StylizedRiverFoamConnectorPathVariant variant =
                        path.PathVariants[variantIndex];
                    writer.Write(variant != null);
                    if (variant == null)
                    {
                        continue;
                    }

                    writer.Write(variant.StartAnchorIndex);
                    writer.Write(variant.EndAnchorIndex);
                    writer.Write((int)variant.Availability);
                    WriteVector2Array(writer, variant.MetricPointData);
                    WriteFloatArray(
                        writer,
                        variant.NormalizedCumulativeLengthData);
                }
            }
        }

        private static StylizedRiverFoamConnectorPath[] ReadConnectorPaths(
            BinaryReader reader)
        {
            int count = ReadCount(reader, "Connector path count");
            StylizedRiverFoamConnectorPath[] paths =
                new StylizedRiverFoamConnectorPath[count];
            for (int index = 0; index < count; index++)
            {
                if (!reader.ReadBoolean())
                {
                    continue;
                }

                uint stableId = reader.ReadUInt32();
                Vector2[] metricPoints = ReadVector2Array(
                    reader,
                    "Connector accepted metric points");
                Vector2[] preparedPoints = ReadVector2Array(
                    reader,
                    "Connector prepared metric points");
                float[] cumulative = ReadFloatArray(
                    reader,
                    MaximumCollectionCount,
                    "Connector normalized cumulative lengths");
                StylizedRiverFoamConnectorEndpointBinding startBinding =
                    ReadEndpointBinding(reader);
                StylizedRiverFoamConnectorEndpointBinding endBinding =
                    ReadEndpointBinding(reader);
                int startAnchorCount = ReadCount(
                    reader,
                    "Connector start anchor count");
                int endAnchorCount = ReadCount(
                    reader,
                    "Connector end anchor count");
                int variantCount = ReadCount(
                    reader,
                    "Connector path variant count");
                StylizedRiverFoamConnectorPathVariant[] variants =
                    new StylizedRiverFoamConnectorPathVariant[variantCount];
                for (int variantIndex = 0;
                     variantIndex < variantCount;
                     variantIndex++)
                {
                    if (!reader.ReadBoolean())
                    {
                        continue;
                    }

                    int startAnchorIndex = reader.ReadInt32();
                    int endAnchorIndex = reader.ReadInt32();
                    int availabilityValue = reader.ReadInt32();
                    if (!Enum.IsDefined(
                            typeof(StylizedRiverFoamConnectorPathVariantAvailability),
                            availabilityValue))
                    {
                        throw new InvalidDataException(
                            $"Unknown Connector variant availability " +
                            $"{availabilityValue}.");
                    }

                    Vector2[] points = ReadVector2Array(
                        reader,
                        "Connector variant points");
                    float[] lengths = ReadFloatArray(
                        reader,
                        MaximumCollectionCount,
                        "Connector variant cumulative lengths");
                    variants[variantIndex] =
                        new StylizedRiverFoamConnectorPathVariant(
                            startAnchorIndex,
                            endAnchorIndex,
                            (StylizedRiverFoamConnectorPathVariantAvailability)
                                availabilityValue,
                            points,
                            lengths);
                }

                paths[index] = new StylizedRiverFoamConnectorPath(
                    stableId,
                    metricPoints,
                    preparedPoints,
                    cumulative,
                    startBinding,
                    endBinding,
                    startAnchorCount,
                    endAnchorCount,
                    variants);
            }

            return paths;
        }

        private static void WriteEndpointBinding(
            BinaryWriter writer,
            StylizedRiverFoamConnectorEndpointBinding binding)
        {
            writer.Write(binding.IsAvailable);
            writer.Write(binding.MajorStableId);
            writer.Write(binding.MajorPreparedIndex);
            WriteVector2(writer, binding.MajorLocalOffsetCells);
            WriteVector2(writer, binding.AcceptedMetricPosition);
            writer.Write(binding.AcceptedAcrossNormalized);
            writer.Write(binding.OwnershipSupport);
        }

        private static StylizedRiverFoamConnectorEndpointBinding
            ReadEndpointBinding(BinaryReader reader)
        {
            return new StylizedRiverFoamConnectorEndpointBinding(
                reader.ReadBoolean(),
                reader.ReadUInt32(),
                reader.ReadInt32(),
                ReadVector2(reader),
                ReadVector2(reader),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        private static void WritePocketTopology(
            BinaryWriter writer,
            StylizedRiverFoamPocketTopology topology)
        {
            writer.Write(topology.Width);
            writer.Write(topology.Height);
            writer.Write(topology.InteriorEligibleHostCount);
            writer.Write(topology.InteriorCandidateCount);
            writer.Write(topology.AcceptedInteriorPocketCount);
            writer.Write(topology.CavityEligibleHostCount);
            writer.Write(topology.CavityCandidateCount);
            writer.Write(topology.AcceptedEdgeCavityCount);
            writer.Write(topology.WeakSpanEligibleConnectorCount);
            writer.Write(topology.WeakSpanCandidateCount);
            writer.Write(topology.AcceptedConnectorWeakSpanCount);
            writer.Write(topology.FreeWaterOpportunityCount);
            writer.Write(topology.FreeWaterCandidateCount);
            writer.Write(topology.AcceptedFreeWaterEventCount);
            writer.Write(topology.CoveredCellCount);
            WriteFloatArray(writer, topology.PressureData);
            WriteFloatArray(writer, topology.HostedPressureData);
            WriteFloatArray(writer, topology.HostedFallbackPressureData);
            WriteFloatArray(writer, topology.IndependentPressureData);
            WriteFloatArray(writer, topology.StaticIndependentPressureData);
            WriteFloatArray(writer, topology.FreeWaterPressureData);

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

            int rejectionCount = Enum.GetValues(
                typeof(StylizedRiverFoamPocketRejectionReason)).Length;
            writer.Write(rejectionCount);
            for (int index = 0; index < rejectionCount; index++)
            {
                writer.Write(topology.GetRejectionCount(
                    (StylizedRiverFoamPocketRejectionReason)index));
            }
        }

        private static StylizedRiverFoamPocketTopology ReadPocketTopology(
            BinaryReader reader,
            int expectedWidth,
            int expectedHeight,
            int cellCount)
        {
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            RequireDimensions(width, height, expectedWidth, expectedHeight, "Negative");
            int interiorEligible = ReadCount(reader, "Interior eligible host count");
            int interiorCandidates = ReadCount(reader, "Interior candidate count");
            int interiorAccepted = ReadCount(reader, "Interior accepted count");
            int cavityEligible = ReadCount(reader, "Cavity eligible host count");
            int cavityCandidates = ReadCount(reader, "Cavity candidate count");
            int cavityAccepted = ReadCount(reader, "Cavity accepted count");
            int weakEligible = ReadCount(reader, "Weak Span eligible count");
            int weakCandidates = ReadCount(reader, "Weak Span candidate count");
            int weakAccepted = ReadCount(reader, "Weak Span accepted count");
            int freeOpportunities = ReadCount(reader, "Free-Water opportunity count");
            int freeCandidates = ReadCount(reader, "Free-Water candidate count");
            int freeAccepted = ReadCount(reader, "Free-Water accepted count");
            int covered = ReadBoundedNonNegative(
                reader,
                cellCount,
                "Negative covered cell count");
            float[] pressure = ReadFloatArrayExact(
                reader,
                cellCount,
                "aggregate negative field");
            float[] hosted = ReadFloatArrayExact(
                reader,
                cellCount,
                "hosted negative field");
            float[] hostedFallback = ReadFloatArrayExact(
                reader,
                cellCount,
                "hosted fallback field");
            float[] independent = ReadFloatArrayExact(
                reader,
                cellCount,
                "independent negative field");
            float[] staticIndependent = ReadFloatArrayExact(
                reader,
                cellCount,
                "static independent negative field");
            float[] freeWater = ReadFloatArrayExact(
                reader,
                cellCount,
                "Free-Water negative field");

            int regionCount = ReadCount(reader, "Negative region count");
            StylizedRiverFoamPocketRegion[] regions =
                new StylizedRiverFoamPocketRegion[regionCount];
            for (int index = 0; index < regionCount; index++)
            {
                StylizedRiverFoamNegativeRegionClass regionClass =
                    ReadNegativeRegionClass(reader);
                regions[index] = StylizedRiverFoamPocketRegion.FromSerialized(
                    regionClass,
                    reader.ReadUInt32(),
                    reader.ReadUInt32(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    ReadVector2(reader),
                    reader.ReadUInt32(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
            }

            int hostedCount = ReadCount(
                reader,
                "prepared hosted negative count");
            StylizedRiverFoamPreparedHostedNegativeRegion[] hostedPrepared =
                new StylizedRiverFoamPreparedHostedNegativeRegion[hostedCount];
            for (int index = 0; index < hostedCount; index++)
            {
                if (!reader.ReadBoolean())
                {
                    continue;
                }

                StylizedRiverFoamNegativeRegionClass regionClass =
                    ReadNegativeRegionClass(reader);
                uint stableId = reader.ReadUInt32();
                uint hostId = reader.ReadUInt32();
                int hostPreparedIndex = reader.ReadInt32();
                int resolution = ReadMaskResolution(reader, "hosted negative mask");
                float[] localPressure = ReadFloatArrayExact(
                    reader,
                    checked(resolution * resolution),
                    "hosted local pressure mask");
                Vector2 centre = ReadVector2(reader);
                int variantCount = ReadCount(
                    reader,
                    "hosted negative variant count");
                StylizedRiverFoamHostedNegativeVariant[] variants =
                    new StylizedRiverFoamHostedNegativeVariant[variantCount];
                for (int variantIndex = 0;
                     variantIndex < variantCount;
                     variantIndex++)
                {
                    variants[variantIndex] =
                        new StylizedRiverFoamHostedNegativeVariant(
                            ReadVector2(reader),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle());
                }

                hostedPrepared[index] =
                    new StylizedRiverFoamPreparedHostedNegativeRegion(
                        regionClass,
                        stableId,
                        hostId,
                        hostPreparedIndex,
                        resolution,
                        localPressure,
                        centre,
                        variants);
            }

            int freeCount = ReadCount(
                reader,
                "prepared Free-Water negative count");
            StylizedRiverFoamPreparedFreeWaterRegion[] freePrepared =
                new StylizedRiverFoamPreparedFreeWaterRegion[freeCount];
            for (int index = 0; index < freeCount; index++)
            {
                if (!reader.ReadBoolean())
                {
                    continue;
                }

                uint stableId = reader.ReadUInt32();
                int resolution = ReadMaskResolution(reader, "Free-Water mask");
                float[] localPressure = ReadFloatArrayExact(
                    reader,
                    checked(resolution * resolution),
                    "Free-Water local pressure mask");
                float centreLocalDistance = reader.ReadSingle();
                float centreAcross = reader.ReadSingle();
                float orientation = reader.ReadSingle();
                float metresPerCell = reader.ReadSingle();
                int anchorCount = ReadCount(
                    reader,
                    "Free-Water recycle anchor count");
                StylizedRiverFoamFreeWaterRecycleAnchor[] anchors =
                    new StylizedRiverFoamFreeWaterRecycleAnchor[anchorCount];
                for (int anchorIndex = 0;
                     anchorIndex < anchorCount;
                     anchorIndex++)
                {
                    anchors[anchorIndex] =
                        new StylizedRiverFoamFreeWaterRecycleAnchor(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadBoolean());
                }

                freePrepared[index] =
                    new StylizedRiverFoamPreparedFreeWaterRegion(
                        stableId,
                        resolution,
                        localPressure,
                        centreLocalDistance,
                        centreAcross,
                        orientation,
                        metresPerCell,
                        anchors);
            }

            int weakCount = ReadCount(
                reader,
                "prepared Weak Span count");
            StylizedRiverFoamPreparedWeakSpanRegion[] weakPrepared =
                new StylizedRiverFoamPreparedWeakSpanRegion[weakCount];
            for (int index = 0; index < weakCount; index++)
            {
                if (!reader.ReadBoolean())
                {
                    continue;
                }

                uint stableId = reader.ReadUInt32();
                uint connectorId = reader.ReadUInt32();
                int availabilityValue = reader.ReadInt32();
                if (!Enum.IsDefined(
                        typeof(StylizedRiverFoamPreparedWeakSpanAvailability),
                        availabilityValue))
                {
                    throw new InvalidDataException(
                        $"Unknown Weak Span availability {availabilityValue}.");
                }

                weakPrepared[index] =
                    StylizedRiverFoamPreparedWeakSpanRegion.FromSerialized(
                        stableId,
                        connectorId,
                        (StylizedRiverFoamPreparedWeakSpanAvailability)
                            availabilityValue,
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadUInt32(),
                        ReadVector2(reader),
                        reader.ReadSingle());
            }

            int[] rejectionCounts = ReadRejectionCounts(
                reader,
                Enum.GetValues(
                    typeof(StylizedRiverFoamPocketRejectionReason)).Length,
                "Negative");
            return new StylizedRiverFoamPocketTopology(
                width,
                height,
                interiorEligible,
                interiorCandidates,
                interiorAccepted,
                cavityEligible,
                cavityCandidates,
                cavityAccepted,
                weakEligible,
                weakCandidates,
                weakAccepted,
                freeOpportunities,
                freeCandidates,
                freeAccepted,
                covered,
                0.0,
                pressure,
                hosted,
                hostedFallback,
                independent,
                staticIndependent,
                freeWater,
                regions,
                hostedPrepared,
                freePrepared,
                weakPrepared,
                rejectionCounts);
        }

        private static StylizedRiverFoamNegativeRegionClass
            ReadNegativeRegionClass(BinaryReader reader)
        {
            int value = reader.ReadInt32();
            if (!Enum.IsDefined(
                    typeof(StylizedRiverFoamNegativeRegionClass),
                    value))
            {
                throw new InvalidDataException(
                    $"Unknown negative region class {value}.");
            }

            return (StylizedRiverFoamNegativeRegionClass)value;
        }

        private static bool GeneratedChannelsEqual(
            StylizedRiverFoamTopologyCachePackage source,
            StylizedRiverFoamTopologyCachePackage reconstructed,
            out string error)
        {
            int cellCount = checked(source.FieldWidth * source.FieldHeight);
            Color[] sourcePixels = new Color[cellCount];
            Color[] reconstructedPixels = new Color[cellCount];
            source.MajorTopology.FillUploadPixels(sourcePixels);
            source.ConnectorTopology.AddToUploadPixels(sourcePixels, false);
            source.PocketTopology.AddToUploadPixels(
                sourcePixels,
                false,
                false,
                false);
            reconstructed.MajorTopology.FillUploadPixels(reconstructedPixels);
            reconstructed.ConnectorTopology.AddToUploadPixels(
                reconstructedPixels,
                false);
            reconstructed.PocketTopology.AddToUploadPixels(
                reconstructedPixels,
                false,
                false,
                false);

            for (int index = 0; index < cellCount; index++)
            {
                if (!ColorBitsEqual(sourcePixels[index], reconstructedPixels[index]))
                {
                    error = $"Initial generated topology channel mismatch at " +
                        $"cell {index}.";
                    return false;
                }

                if (FloatBits(source.ObstacleExclusion[index]) !=
                    FloatBits(reconstructed.ObstacleExclusion[index]))
                {
                    error = $"Obstacle scalar mismatch at cell {index}.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private static bool CorruptionIsRejected(
            byte[] payload,
            out string error)
        {
            byte[] corrupted = (byte[])payload.Clone();
            int corruptionIndex = Mathf.Clamp(
                corrupted.Length / 2,
                12,
                corrupted.Length - ChecksumByteCount - 1);
            corrupted[corruptionIndex] ^= 0x5A;
            if (TryDeserialize(corrupted, out _, out _))
            {
                error = "A deliberately corrupted cache payload was accepted.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static void ValidatePackage(
            StylizedRiverFoamTopologyCachePackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            ValidateFieldDimensions(package.FieldWidth, package.FieldHeight);
            int cellCount = checked(package.FieldWidth * package.FieldHeight);
            if (package.ObstacleExclusion == null ||
                package.ObstacleExclusion.Length != cellCount)
            {
                throw new InvalidDataException(
                    $"Obstacle scalar field contains " +
                    $"{package.ObstacleExclusion?.Length ?? 0} values; " +
                    $"expected {cellCount}.");
            }

            if (package.MajorTopology == null ||
                package.ConnectorTopology == null ||
                package.PocketTopology == null)
            {
                throw new InvalidDataException(
                    "A complete Major, Connector, and Negative topology set is required.");
            }

            RequireDimensions(
                package.MajorTopology.Width,
                package.MajorTopology.Height,
                package.FieldWidth,
                package.FieldHeight,
                "Major");
            RequireDimensions(
                package.ConnectorTopology.Width,
                package.ConnectorTopology.Height,
                package.FieldWidth,
                package.FieldHeight,
                "Connector");
            RequireDimensions(
                package.PocketTopology.Width,
                package.PocketTopology.Height,
                package.FieldWidth,
                package.FieldHeight,
                "Negative");
            RequireLength(
                package.MajorTopology.SupportData,
                cellCount,
                "Major support field");
            RequireLength(
                package.ConnectorTopology.SupportData,
                cellCount,
                "Connector support field");
            RequireLength(
                package.PocketTopology.PressureData,
                cellCount,
                "aggregate negative field");
            RequireLength(
                package.PocketTopology.HostedPressureData,
                cellCount,
                "hosted negative field");
            RequireLength(
                package.PocketTopology.HostedFallbackPressureData,
                cellCount,
                "hosted fallback field");
            RequireLength(
                package.PocketTopology.IndependentPressureData,
                cellCount,
                "independent negative field");
            RequireLength(
                package.PocketTopology.StaticIndependentPressureData,
                cellCount,
                "static independent negative field");
            RequireLength(
                package.PocketTopology.FreeWaterPressureData,
                cellCount,
                "Free-Water negative field");
        }

        private static void ValidateFieldDimensions(int width, int height)
        {
            if (width < 1 || height < 1 ||
                width > MaximumFieldDimension ||
                height > MaximumFieldDimension)
            {
                throw new InvalidDataException(
                    $"Invalid cache field dimensions {width} × {height}.");
            }

            int cellCount = checked(width * height);
            if (cellCount > MaximumFieldCellCount)
            {
                throw new InvalidDataException(
                    $"Cache field contains {cellCount} cells; maximum is " +
                    $"{MaximumFieldCellCount}.");
            }
        }

        private static void RequireDimensions(
            int width,
            int height,
            int expectedWidth,
            int expectedHeight,
            string label)
        {
            if (width != expectedWidth || height != expectedHeight)
            {
                throw new InvalidDataException(
                    $"{label} topology dimensions {width} × {height} do not " +
                    $"match cache dimensions {expectedWidth} × {expectedHeight}.");
            }
        }

        private static void RequireLength(
            Array values,
            int expectedLength,
            string label)
        {
            if (values == null || values.Length != expectedLength)
            {
                throw new InvalidDataException(
                    $"{label} contains {values?.Length ?? 0} values; " +
                    $"expected {expectedLength}.");
            }
        }

        private static int ReadMaskResolution(BinaryReader reader, string label)
        {
            int resolution = ReadBoundedNonNegative(
                reader,
                1024,
                $"{label} resolution");
            if (resolution < 1 ||
                checked(resolution * resolution) > MaximumLocalMaskCellCount)
            {
                throw new InvalidDataException(
                    $"Invalid {label} resolution {resolution}.");
            }

            return resolution;
        }

        private static int ReadCount(BinaryReader reader, string label)
        {
            return ReadBoundedNonNegative(
                reader,
                MaximumCollectionCount,
                label);
        }

        private static int ReadBoundedNonNegative(
            BinaryReader reader,
            int maximum,
            string label)
        {
            int count = reader.ReadInt32();
            if (count < 0 || count > maximum)
            {
                throw new InvalidDataException(
                    $"Invalid {label} {count}; expected 0–{maximum}.");
            }

            return count;
        }

        private static int[] ReadRejectionCounts(
            BinaryReader reader,
            int expectedCount,
            string label)
        {
            int count = ReadBoundedNonNegative(
                reader,
                256,
                $"{label} rejection count length");
            if (count != expectedCount)
            {
                throw new InvalidDataException(
                    $"{label} rejection table contains {count} entries; " +
                    $"expected {expectedCount}.");
            }

            int[] result = new int[count];
            for (int index = 0; index < count; index++)
            {
                result[index] = ReadCount(
                    reader,
                    $"{label} rejection value {index}");
            }

            return result;
        }

        private static void WriteFloatArray(BinaryWriter writer, float[] values)
        {
            values ??= Array.Empty<float>();
            writer.Write(values.Length);
            for (int index = 0; index < values.Length; index++)
            {
                writer.Write(values[index]);
            }
        }

        private static float[] ReadFloatArrayExact(
            BinaryReader reader,
            int expectedCount,
            string label)
        {
            int count = ReadBoundedNonNegative(
                reader,
                Math.Max(MaximumCollectionCount, expectedCount),
                $"{label} length");
            if (count != expectedCount)
            {
                throw new InvalidDataException(
                    $"{label} contains {count} values; expected {expectedCount}.");
            }

            float[] values = new float[count];
            for (int index = 0; index < count; index++)
            {
                values[index] = reader.ReadSingle();
            }

            return values;
        }

        private static float[] ReadFloatArray(
            BinaryReader reader,
            int maximumCount,
            string label)
        {
            int count = ReadBoundedNonNegative(
                reader,
                maximumCount,
                $"{label} length");
            float[] values = new float[count];
            for (int index = 0; index < count; index++)
            {
                values[index] = reader.ReadSingle();
            }

            return values;
        }

        private static void WriteVector2Array(
            BinaryWriter writer,
            Vector2[] values)
        {
            values ??= Array.Empty<Vector2>();
            writer.Write(values.Length);
            for (int index = 0; index < values.Length; index++)
            {
                WriteVector2(writer, values[index]);
            }
        }

        private static Vector2[] ReadVector2Array(
            BinaryReader reader,
            string label)
        {
            int count = ReadCount(reader, $"{label} length");
            Vector2[] values = new Vector2[count];
            for (int index = 0; index < count; index++)
            {
                values[index] = ReadVector2(reader);
            }

            return values;
        }

        private static void WriteVector2(BinaryWriter writer, Vector2 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }

        private static Vector2 ReadVector2(BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            for (int index = 0; index < a.Length; index++)
            {
                if (a[index] != b[index])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ColorBitsEqual(Color a, Color b)
        {
            return FloatBits(a.r) == FloatBits(b.r) &&
                FloatBits(a.g) == FloatBits(b.g) &&
                FloatBits(a.b) == FloatBits(b.b) &&
                FloatBits(a.a) == FloatBits(b.a);
        }

        private static int FloatBits(float value)
        {
            return BitConverter.SingleToInt32Bits(value);
        }

        private static ulong ComputeChecksum(byte[] bytes, int offset, int count)
        {
            const ulong offsetBasis = 14695981039346656037ul;
            const ulong prime = 1099511628211ul;
            ulong hash = offsetBasis;
            int end = checked(offset + count);
            for (int index = offset; index < end; index++)
            {
                hash ^= bytes[index];
                hash *= prime;
            }

            return hash;
        }

        private static void WriteUInt64LittleEndian(
            byte[] destination,
            int offset,
            ulong value)
        {
            for (int byteIndex = 0; byteIndex < sizeof(ulong); byteIndex++)
            {
                destination[offset + byteIndex] = (byte)(value >> (byteIndex * 8));
            }
        }

        private static ulong ReadUInt64LittleEndian(byte[] source, int offset)
        {
            ulong value = 0ul;
            for (int byteIndex = 0; byteIndex < sizeof(ulong); byteIndex++)
            {
                value |= (ulong)source[offset + byteIndex] << (byteIndex * 8);
            }

            return value;
        }
    }
}
