using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    internal enum StylizedRiverFoamGridMapping
    {
        LegacyNormalizedAcross = 0,
        FixedMetricLattice = 1
    }

    /// <summary>
    /// CPU-side mirror of the five float4 Foam grid descriptor uniforms.
    /// The field order and 80-byte stride are validated in the Unity Editor.
    /// This patch establishes the ABI only; fixed-metric allocation is enabled
    /// by the later coordinate-migration phases.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct StylizedRiverFoamGridGpuData
    {
        public const int ExpectedStrideBytes = 80;

        public StylizedRiverFoamGridGpuData(
            Vector4 contract,
            Vector4 spacing,
            Vector4 lateral,
            Vector4 longitudinal,
            Vector4 extent)
        {
            Contract = contract;
            Spacing = spacing;
            Lateral = lateral;
            Longitudinal = longitudinal;
            Extent = extent;
        }

        public readonly Vector4 Contract;
        public readonly Vector4 Spacing;
        public readonly Vector4 Lateral;
        public readonly Vector4 Longitudinal;
        public readonly Vector4 Extent;
    }

    /// <summary>
    /// Immutable semantic authority for River Foam field dimensions and
    /// coordinate mapping. RG-METRIC-P3 makes this descriptor authoritative for
    /// allocation dimensions and defines the complete one-strip metric CPU
    /// conversion contract. The active runtime remains LegacyNormalizedAcross
    /// until every dependent GPU, cache, topology, source, transport, and render
    /// consumer is migrated; the fixed-metric path is therefore prepared and
    /// exhaustively validated without allowing mixed coordinate state.
    /// </summary>
    internal readonly struct StylizedRiverFoamGridDescriptor :
        IEquatable<StylizedRiverFoamGridDescriptor>
    {
        public const int DescriptorContractVersion = 1;
        public const int LegacyMappingContractVersion = 0;
        public const int FixedMetricMappingContractVersion = 1;
        public const float LongitudinalChunkLengthMetres = 32f;
        public const float ConservativeCandidateCellSizeMetres = 0.25f;
        public const float IntermediateCandidateCellSizeMetres = 0.20f;
        public const float TargetCandidateCellSizeMetres = 0.15f;
        public const float StressCandidateCellSizeMetres = 0.10f;

        private const float MinimumSpacingMetres = 0.0001f;
        private static bool foundationValidated;

        private StylizedRiverFoamGridDescriptor(
            StylizedRiverFoamGridMapping mapping,
            int mappingContractVersion,
            StylizedRiverQuality quality,
            float requestedDxMetres,
            float requestedDyMetres,
            int columnsPerChunk,
            float resolvedDxMetres,
            float resolvedDyMetres,
            float lateralLatticePhaseMetres,
            int globalYBase,
            int rowCount,
            float fieldOrStripStartMetres,
            float allocatedLengthMetres,
            float validLengthMetres,
            int columnCount,
            int filmWidth,
            int filmHeight,
            int allocationGuardRows,
            float representedLateralMinimumMetres,
            float representedLateralMaximumMetres)
        {
            Mapping = mapping;
            MappingContractVersion = mappingContractVersion;
            Quality = quality;
            RequestedDxMetres = requestedDxMetres;
            RequestedDyMetres = requestedDyMetres;
            ColumnsPerChunk = columnsPerChunk;
            ResolvedDxMetres = resolvedDxMetres;
            ResolvedDyMetres = resolvedDyMetres;
            LateralLatticePhaseMetres = lateralLatticePhaseMetres;
            GlobalYBase = globalYBase;
            RowCount = rowCount;
            FieldOrStripStartMetres = fieldOrStripStartMetres;
            AllocatedLengthMetres = allocatedLengthMetres;
            ValidLengthMetres = validLengthMetres;
            ColumnCount = columnCount;
            FilmWidth = filmWidth;
            FilmHeight = filmHeight;
            AllocationGuardRows = allocationGuardRows;
            RepresentedLateralMinimumMetres =
                representedLateralMinimumMetres;
            RepresentedLateralMaximumMetres =
                representedLateralMaximumMetres;
            InitializationSignature = CalculateInitializationSignature(
                mapping,
                mappingContractVersion,
                quality,
                requestedDxMetres,
                requestedDyMetres,
                columnsPerChunk,
                resolvedDxMetres,
                resolvedDyMetres,
                lateralLatticePhaseMetres,
                globalYBase,
                rowCount,
                fieldOrStripStartMetres,
                allocatedLengthMetres,
                validLengthMetres,
                columnCount,
                filmWidth,
                filmHeight,
                allocationGuardRows,
                representedLateralMinimumMetres,
                representedLateralMaximumMetres);
        }

        public StylizedRiverFoamGridMapping Mapping { get; }
        public int MappingContractVersion { get; }
        public StylizedRiverQuality Quality { get; }
        public float RequestedDxMetres { get; }
        public float RequestedDyMetres { get; }
        public int ColumnsPerChunk { get; }
        public float ResolvedDxMetres { get; }
        public float ResolvedDyMetres { get; }
        public float LateralLatticePhaseMetres { get; }
        public int GlobalYBase { get; }
        public int RowCount { get; }
        public float FieldOrStripStartMetres { get; }
        public float AllocatedLengthMetres { get; }
        public float ValidLengthMetres { get; }
        public int ColumnCount { get; }
        public int FilmWidth { get; }
        public int FilmHeight { get; }
        public int AllocationGuardRows { get; }
        public float RepresentedLateralMinimumMetres { get; }
        public float RepresentedLateralMaximumMetres { get; }
        public ulong InitializationSignature { get; }

        public bool IsCreated => ColumnCount > 0 && RowCount > 0;
        public bool UsesFixedMetricLattice =>
            Mapping == StylizedRiverFoamGridMapping.FixedMetricLattice;
        public long StructuralCellCount =>
            (long)Math.Max(0, ColumnCount) * Math.Max(0, RowCount);
        public float RepresentedLateralExtentMetres =>
            Mathf.Max(
                0f,
                RepresentedLateralMaximumMetres -
                RepresentedLateralMinimumMetres);
        public int GlobalYMaximum => GlobalYBase + RowCount - 1;
        public float AllocatedLocalDistanceMinimumMetres =>
            FieldOrStripStartMetres;
        public float AllocatedLocalDistanceMaximumMetres =>
            FieldOrStripStartMetres + AllocatedLengthMetres;
        public float ValidLocalDistanceMaximumMetres =>
            FieldOrStripStartMetres + ValidLengthMetres;

        public static float ResolveProvisionalRequestedCellSizeMetres(
            StylizedRiverQuality quality)
        {
            return quality switch
            {
                StylizedRiverQuality.Low =>
                    ConservativeCandidateCellSizeMetres,
                StylizedRiverQuality.Medium =>
                    TargetCandidateCellSizeMetres,
                StylizedRiverQuality.High =>
                    StressCandidateCellSizeMetres,
                _ => TargetCandidateCellSizeMetres
            };
        }

        public static StylizedRiverFoamGridDescriptor CreateLegacyNormalized(
            StylizedRiverQuality quality,
            int chunkCount,
            int columnsPerChunk,
            int columnCount,
            int rowCount,
            int filmWidth,
            int filmHeight,
            float allocatedLengthMetres,
            float validLengthMetres)
        {
            if (chunkCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkCount));
            }
            if (columnsPerChunk < 1 || columnCount < 1 || rowCount < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(columnsPerChunk),
                    "Legacy Foam dimensions must be positive.");
            }
            if (columnCount != chunkCount * columnsPerChunk)
            {
                throw new ArgumentException(
                    "Legacy Foam width must equal chunk count multiplied by " +
                    "columns per chunk.",
                    nameof(columnCount));
            }
            if (!IsPositiveFinite(allocatedLengthMetres) ||
                !IsNonNegativeFinite(validLengthMetres) ||
                validLengthMetres > allocatedLengthMetres + 0.0001f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(validLengthMetres),
                    "Legacy Foam lengths are invalid.");
            }

            float spacing = allocatedLengthMetres / columnCount;
            return new StylizedRiverFoamGridDescriptor(
                StylizedRiverFoamGridMapping.LegacyNormalizedAcross,
                LegacyMappingContractVersion,
                quality,
                spacing,
                0f,
                columnsPerChunk,
                spacing,
                0f,
                0f,
                0,
                rowCount,
                0f,
                allocatedLengthMetres,
                validLengthMetres,
                columnCount,
                Math.Max(1, filmWidth),
                Math.Max(1, filmHeight),
                0,
                0f,
                0f);
        }

        internal static bool TryCreateFromCacheContract(
            int descriptorContractVersion,
            StylizedRiverFoamGridMapping mapping,
            int mappingContractVersion,
            StylizedRiverQuality quality,
            float requestedDxMetres,
            float requestedDyMetres,
            int columnsPerChunk,
            float resolvedDxMetres,
            float resolvedDyMetres,
            float lateralLatticePhaseMetres,
            int globalYBase,
            int rowCount,
            float fieldOrStripStartMetres,
            float allocatedLengthMetres,
            float validLengthMetres,
            int columnCount,
            int filmWidth,
            int filmHeight,
            int allocationGuardRows,
            float representedLateralMinimumMetres,
            float representedLateralMaximumMetres,
            ulong expectedInitializationSignature,
            out StylizedRiverFoamGridDescriptor descriptor,
            out string failureReason)
        {
            descriptor = default;
            failureReason = string.Empty;

            if (descriptorContractVersion != DescriptorContractVersion)
            {
                failureReason =
                    $"Unsupported Foam grid descriptor contract " +
                    $"{descriptorContractVersion}; expected " +
                    $"{DescriptorContractVersion}.";
                return false;
            }
            if (!Enum.IsDefined(typeof(StylizedRiverFoamGridMapping), mapping))
            {
                failureReason = $"Unknown Foam grid mapping value {(int)mapping}.";
                return false;
            }
            if (!Enum.IsDefined(typeof(StylizedRiverQuality), quality))
            {
                failureReason = $"Unknown river quality value {(int)quality}.";
                return false;
            }

            int expectedMappingContract = mapping switch
            {
                StylizedRiverFoamGridMapping.LegacyNormalizedAcross =>
                    LegacyMappingContractVersion,
                StylizedRiverFoamGridMapping.FixedMetricLattice =>
                    FixedMetricMappingContractVersion,
                _ => int.MinValue
            };
            if (mappingContractVersion != expectedMappingContract)
            {
                failureReason =
                    $"Foam grid mapping {mapping} uses contract " +
                    $"{mappingContractVersion}; expected " +
                    $"{expectedMappingContract}.";
                return false;
            }
            if (columnsPerChunk < 1 || columnCount < 1 || rowCount < 1 ||
                filmWidth < 1 || filmHeight < 1 ||
                columnCount % columnsPerChunk != 0)
            {
                failureReason =
                    "Foam cache descriptor dimensions or chunk alignment are invalid.";
                return false;
            }
            if (!IsPositiveFinite(resolvedDxMetres) ||
                !IsPositiveFinite(allocatedLengthMetres) ||
                !IsNonNegativeFinite(validLengthMetres) ||
                validLengthMetres > allocatedLengthMetres + 0.0001f ||
                !IsFinite(fieldOrStripStartMetres) ||
                allocationGuardRows < 0)
            {
                failureReason =
                    "Foam cache descriptor longitudinal values are invalid.";
                return false;
            }
            float longitudinalTolerance = Mathf.Max(
                0.0001f,
                resolvedDxMetres * 0.001f);
            if (Mathf.Abs(
                    columnCount * resolvedDxMetres - allocatedLengthMetres) >
                    longitudinalTolerance ||
                Mathf.Abs(
                    columnsPerChunk * resolvedDxMetres -
                    LongitudinalChunkLengthMetres) > longitudinalTolerance)
            {
                failureReason =
                    "Foam cache descriptor X spacing does not reproduce its " +
                    "allocated length and 32-metre chunk contract.";
                return false;
            }
            if (filmWidth != Math.Max(1, Mathf.CeilToInt(columnCount * 0.5f)) ||
                filmHeight != Math.Max(1, Mathf.CeilToInt(rowCount * 0.5f)))
            {
                failureReason =
                    "Foam cache descriptor film dimensions do not match its " +
                    "structural dimensions.";
                return false;
            }
            long globalYMaximum = (long)globalYBase + rowCount - 1L;
            if (globalYMaximum < int.MinValue || globalYMaximum > int.MaxValue)
            {
                failureReason =
                    "Foam cache descriptor global-Y interval exceeds integer " +
                    "index limits.";
                return false;
            }

            if (mapping == StylizedRiverFoamGridMapping.LegacyNormalizedAcross)
            {
                bool legacyContractValid =
                    IsPositiveFinite(requestedDxMetres) &&
                    Mathf.Abs(requestedDxMetres - resolvedDxMetres) <=
                        longitudinalTolerance &&
                    requestedDyMetres == 0f &&
                    resolvedDyMetres == 0f &&
                    lateralLatticePhaseMetres == 0f &&
                    globalYBase == 0 &&
                    fieldOrStripStartMetres == 0f &&
                    allocationGuardRows == 0 &&
                    representedLateralMinimumMetres == 0f &&
                    representedLateralMaximumMetres == 0f;
                if (!legacyContractValid)
                {
                    failureReason =
                        "Legacy Foam cache descriptor contains fixed-lattice values.";
                    return false;
                }
            }
            else
            {
                if (!IsPositiveFinite(requestedDxMetres) ||
                    !IsPositiveFinite(requestedDyMetres) ||
                    !IsPositiveFinite(resolvedDyMetres) ||
                    Mathf.CeilToInt(
                        LongitudinalChunkLengthMetres / requestedDxMetres) !=
                        columnsPerChunk ||
                    !Mathf.Approximately(requestedDyMetres, resolvedDyMetres) ||
                    !IsFinite(lateralLatticePhaseMetres) ||
                    !IsFinite(representedLateralMinimumMetres) ||
                    !IsFinite(representedLateralMaximumMetres) ||
                    representedLateralMinimumMetres >=
                        representedLateralMaximumMetres)
                {
                    failureReason =
                        "Fixed-metric Foam cache descriptor contains invalid " +
                        "lateral values.";
                    return false;
                }

                float expectedMinimum = lateralLatticePhaseMetres +
                    (globalYBase - 0.5f) * resolvedDyMetres;
                float expectedMaximum = lateralLatticePhaseMetres +
                    (globalYBase + rowCount - 0.5f) * resolvedDyMetres;
                float lateralTolerance = Mathf.Max(
                    0.0001f,
                    resolvedDyMetres * 0.001f);
                if (Mathf.Abs(
                        expectedMinimum -
                        representedLateralMinimumMetres) > lateralTolerance ||
                    Mathf.Abs(
                        expectedMaximum -
                        representedLateralMaximumMetres) > lateralTolerance)
                {
                    failureReason =
                        "Fixed-metric Foam cache descriptor lateral extent does " +
                        "not match its lattice indices.";
                    return false;
                }
            }

            StylizedRiverFoamGridDescriptor reconstructed =
                new StylizedRiverFoamGridDescriptor(
                    mapping,
                    mappingContractVersion,
                    quality,
                    requestedDxMetres,
                    requestedDyMetres,
                    columnsPerChunk,
                    resolvedDxMetres,
                    resolvedDyMetres,
                    lateralLatticePhaseMetres,
                    globalYBase,
                    rowCount,
                    fieldOrStripStartMetres,
                    allocatedLengthMetres,
                    validLengthMetres,
                    columnCount,
                    filmWidth,
                    filmHeight,
                    allocationGuardRows,
                    representedLateralMinimumMetres,
                    representedLateralMaximumMetres);
            if (expectedInitializationSignature == 0ul ||
                reconstructed.InitializationSignature !=
                    expectedInitializationSignature)
            {
                failureReason =
                    "Foam cache descriptor initialization signature does not " +
                    "match its serialized values.";
                return false;
            }

            descriptor = reconstructed;
            return true;
        }

        public static bool TryCreateFixedMetricOneStrip(
            StylizedRiverQuality quality,
            float requestedDxMetres,
            float requestedDyMetres,
            float validLengthMetres,
            float lateralMinimumMetres,
            float lateralMaximumMetres,
            float lateralLatticePhaseMetres,
            int allocationGuardRows,
            int maximumDimension,
            out StylizedRiverFoamGridDescriptor descriptor,
            out string failureReason)
        {
            return TryCreateFixedMetricCandidate(
                quality,
                requestedDxMetres,
                requestedDyMetres,
                validLengthMetres,
                lateralMinimumMetres,
                lateralMaximumMetres,
                lateralLatticePhaseMetres,
                allocationGuardRows,
                maximumDimension,
                out descriptor,
                out failureReason);
        }

        public static bool TryCreateFixedMetricCandidate(
            StylizedRiverQuality quality,
            float requestedDxMetres,
            float requestedDyMetres,
            float validLengthMetres,
            float lateralMinimumMetres,
            float lateralMaximumMetres,
            float lateralLatticePhaseMetres,
            int allocationGuardRows,
            int maximumDimension,
            out StylizedRiverFoamGridDescriptor descriptor,
            out string failureReason)
        {
            descriptor = default;
            failureReason = string.Empty;

            if (!IsPositiveFinite(requestedDxMetres) ||
                requestedDxMetres < MinimumSpacingMetres)
            {
                failureReason = "Requested longitudinal spacing is invalid.";
                return false;
            }
            if (!IsPositiveFinite(requestedDyMetres) ||
                requestedDyMetres < MinimumSpacingMetres)
            {
                failureReason = "Requested lateral spacing is invalid.";
                return false;
            }
            if (!IsPositiveFinite(validLengthMetres))
            {
                failureReason = "Valid Foam length must be positive.";
                return false;
            }
            if (!IsFinite(lateralMinimumMetres) ||
                !IsFinite(lateralMaximumMetres) ||
                lateralMinimumMetres > lateralMaximumMetres)
            {
                failureReason = "The lateral water range is invalid.";
                return false;
            }
            if (!IsFinite(lateralLatticePhaseMetres))
            {
                failureReason = "The lateral lattice phase is invalid.";
                return false;
            }
            if (allocationGuardRows < 0)
            {
                failureReason = "Allocation guard rows cannot be negative.";
                return false;
            }
            if (maximumDimension < 1)
            {
                failureReason = "Maximum field dimension must be positive.";
                return false;
            }

            int columnsPerChunk = Mathf.CeilToInt(
                LongitudinalChunkLengthMetres / requestedDxMetres);
            if (columnsPerChunk < 1 || columnsPerChunk > maximumDimension)
            {
                failureReason =
                    "Requested longitudinal spacing exceeds field limits.";
                return false;
            }

            float resolvedDxMetres =
                LongitudinalChunkLengthMetres / columnsPerChunk;
            int chunkCount = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    validLengthMetres /
                    LongitudinalChunkLengthMetres));
            long columnCountLong = (long)chunkCount * columnsPerChunk;
            if (columnCountLong > maximumDimension)
            {
                failureReason =
                    "The contiguous metric field exceeds the X dimension " +
                    "limit without changing physical scale.";
                return false;
            }

            double firstRowValue =
                (lateralMinimumMetres -
                    0.5 * requestedDyMetres -
                    lateralLatticePhaseMetres) /
                requestedDyMetres;
            double lastRowValue =
                (lateralMaximumMetres +
                    0.5 * requestedDyMetres -
                    lateralLatticePhaseMetres) /
                requestedDyMetres;
            long firstGlobalYLong =
                (long)Math.Ceiling(firstRowValue) - allocationGuardRows;
            long lastGlobalYLong =
                (long)Math.Floor(lastRowValue) + allocationGuardRows;
            long rowCountLong = lastGlobalYLong - firstGlobalYLong + 1L;
            if (rowCountLong < 1L || rowCountLong > maximumDimension ||
                firstGlobalYLong < int.MinValue ||
                firstGlobalYLong > int.MaxValue ||
                lastGlobalYLong < int.MinValue ||
                lastGlobalYLong > int.MaxValue)
            {
                failureReason =
                    "The contiguous metric field exceeds the Y dimension " +
                    "or global-index limit without changing physical scale.";
                return false;
            }

            int columnCount = (int)columnCountLong;
            int rowCount = (int)rowCountLong;
            int firstGlobalY = (int)firstGlobalYLong;
            int lastGlobalY = (int)lastGlobalYLong;
            float allocatedLength =
                chunkCount * LongitudinalChunkLengthMetres;
            float representedMinimum =
                lateralLatticePhaseMetres +
                (firstGlobalY - 0.5f) * requestedDyMetres;
            float representedMaximum =
                lateralLatticePhaseMetres +
                (lastGlobalY + 0.5f) * requestedDyMetres;

            descriptor = new StylizedRiverFoamGridDescriptor(
                StylizedRiverFoamGridMapping.FixedMetricLattice,
                FixedMetricMappingContractVersion,
                quality,
                requestedDxMetres,
                requestedDyMetres,
                columnsPerChunk,
                resolvedDxMetres,
                requestedDyMetres,
                lateralLatticePhaseMetres,
                firstGlobalY,
                rowCount,
                0f,
                allocatedLength,
                validLengthMetres,
                columnCount,
                Math.Max(1, Mathf.CeilToInt(columnCount * 0.5f)),
                Math.Max(1, Mathf.CeilToInt(rowCount * 0.5f)),
                allocationGuardRows,
                representedMinimum,
                representedMaximum);
            return true;
        }

        public float ResolveLocalDistanceAtColumnCentre(int localX)
        {
            if (localX < 0 || localX >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(localX));
            }

            return FieldOrStripStartMetres +
                (localX + 0.5f) * ResolvedDxMetres;
        }

        public int ResolveGlobalY(int localY)
        {
            if (localY < 0 || localY >= RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(localY));
            }

            return GlobalYBase + localY;
        }

        public float ResolveLateralMetresAtRowCentre(int localY)
        {
            if (!UsesFixedMetricLattice)
            {
                throw new InvalidOperationException(
                    "Legacy normalized rows do not have one fixed metric " +
                    "lateral centre independent of the local river width.");
            }

            return LateralLatticePhaseMetres +
                ResolveGlobalY(localY) * ResolvedDyMetres;
        }

        public Vector2 ResolveMetricPositionAtCellCentre(
            int localX,
            int localY)
        {
            return new Vector2(
                ResolveLocalDistanceAtColumnCentre(localX),
                ResolveLateralMetresAtRowCentre(localY));
        }

        public bool ContainsAllocatedLocalDistance(float localDistanceMetres)
        {
            return IsFinite(localDistanceMetres) &&
                localDistanceMetres >=
                    AllocatedLocalDistanceMinimumMetres - MinimumSpacingMetres &&
                localDistanceMetres <=
                    AllocatedLocalDistanceMaximumMetres + MinimumSpacingMetres;
        }

        public bool ContainsValidLocalDistance(float localDistanceMetres)
        {
            return IsFinite(localDistanceMetres) &&
                localDistanceMetres >=
                    FieldOrStripStartMetres - MinimumSpacingMetres &&
                localDistanceMetres <=
                    ValidLocalDistanceMaximumMetres + MinimumSpacingMetres;
        }

        public bool ContainsAllocatedLateralMetres(float lateralMetres)
        {
            return UsesFixedMetricLattice &&
                IsFinite(lateralMetres) &&
                lateralMetres >=
                    RepresentedLateralMinimumMetres - MinimumSpacingMetres &&
                lateralMetres <=
                    RepresentedLateralMaximumMetres + MinimumSpacingMetres;
        }

        public bool ContainsAllocatedMetricPosition(Vector2 metricPosition)
        {
            return ContainsAllocatedLocalDistance(metricPosition.x) &&
                ContainsAllocatedLateralMetres(metricPosition.y);
        }

        public bool TryMetricToFractionalCellPosition(
            Vector2 metricPosition,
            out Vector2 cellPosition)
        {
            cellPosition = default;
            if (!UsesFixedMetricLattice || !IsCreated ||
                !ContainsAllocatedMetricPosition(metricPosition) ||
                ResolvedDxMetres < MinimumSpacingMetres ||
                ResolvedDyMetres < MinimumSpacingMetres)
            {
                return false;
            }

            cellPosition = new Vector2(
                (metricPosition.x - FieldOrStripStartMetres) /
                    ResolvedDxMetres,
                (metricPosition.y - LateralLatticePhaseMetres) /
                    ResolvedDyMetres - GlobalYBase + 0.5f);
            return true;
        }

        public bool TryMetricToContainingCell(
            Vector2 metricPosition,
            out Vector2Int cell)
        {
            cell = default;
            if (!TryMetricToFractionalCellPosition(
                    metricPosition,
                    out Vector2 cellPosition))
            {
                return false;
            }

            cell = new Vector2Int(
                Mathf.Clamp(
                    Mathf.FloorToInt(cellPosition.x),
                    0,
                    ColumnCount - 1),
                Mathf.Clamp(
                    Mathf.FloorToInt(cellPosition.y),
                    0,
                    RowCount - 1));
            return true;
        }

        public bool TryMetricToNearestCell(
            Vector2 metricPosition,
            out Vector2Int cell)
        {
            cell = default;
            if (!TryMetricToFractionalCellPosition(
                    metricPosition,
                    out Vector2 cellPosition))
            {
                return false;
            }

            cell = new Vector2Int(
                Mathf.Clamp(
                    Mathf.RoundToInt(cellPosition.x - 0.5f),
                    0,
                    ColumnCount - 1),
                Mathf.Clamp(
                    Mathf.RoundToInt(cellPosition.y - 0.5f),
                    0,
                    RowCount - 1));
            return true;
        }

        public bool TryResolveLocalY(int globalY, out int localY)
        {
            localY = globalY - GlobalYBase;
            return localY >= 0 && localY < RowCount;
        }

        public int ResolveNearestGlobalY(float lateralMetres)
        {
            if (!UsesFixedMetricLattice ||
                ResolvedDyMetres < MinimumSpacingMetres ||
                !IsFinite(lateralMetres))
            {
                throw new InvalidOperationException(
                    "A fixed metric lattice is required to resolve global Y.");
            }

            return Mathf.RoundToInt(
                (lateralMetres - LateralLatticePhaseMetres) /
                ResolvedDyMetres);
        }

        public int ResolveContainingGlobalY(float lateralMetres)
        {
            if (!UsesFixedMetricLattice ||
                ResolvedDyMetres < MinimumSpacingMetres ||
                !IsFinite(lateralMetres))
            {
                throw new InvalidOperationException(
                    "A fixed metric lattice is required to resolve global Y.");
            }

            return Mathf.FloorToInt(
                (lateralMetres - LateralLatticePhaseMetres) /
                ResolvedDyMetres + 0.5f);
        }

        public StylizedRiverFoamGridGpuData ToGpuData()
        {
            return new StylizedRiverFoamGridGpuData(
                new Vector4(
                    DescriptorContractVersion,
                    MappingContractVersion,
                    (int)Mapping,
                    (int)Quality),
                new Vector4(
                    RequestedDxMetres,
                    RequestedDyMetres,
                    ResolvedDxMetres,
                    ResolvedDyMetres),
                new Vector4(
                    LateralLatticePhaseMetres,
                    GlobalYBase,
                    RowCount,
                    AllocationGuardRows),
                new Vector4(
                    FieldOrStripStartMetres,
                    AllocatedLengthMetres,
                    ValidLengthMetres,
                    ColumnCount),
                new Vector4(
                    RepresentedLateralMinimumMetres,
                    RepresentedLateralMaximumMetres,
                    FilmWidth,
                    FilmHeight));
        }

        public bool Equals(StylizedRiverFoamGridDescriptor other)
        {
            return InitializationSignature == other.InitializationSignature &&
                Mapping == other.Mapping &&
                MappingContractVersion == other.MappingContractVersion &&
                Quality == other.Quality &&
                RequestedDxMetres.Equals(other.RequestedDxMetres) &&
                RequestedDyMetres.Equals(other.RequestedDyMetres) &&
                ColumnsPerChunk == other.ColumnsPerChunk &&
                ResolvedDxMetres.Equals(other.ResolvedDxMetres) &&
                ResolvedDyMetres.Equals(other.ResolvedDyMetres) &&
                LateralLatticePhaseMetres.Equals(
                    other.LateralLatticePhaseMetres) &&
                GlobalYBase == other.GlobalYBase &&
                RowCount == other.RowCount &&
                FieldOrStripStartMetres.Equals(
                    other.FieldOrStripStartMetres) &&
                AllocatedLengthMetres.Equals(other.AllocatedLengthMetres) &&
                ValidLengthMetres.Equals(other.ValidLengthMetres) &&
                ColumnCount == other.ColumnCount &&
                FilmWidth == other.FilmWidth &&
                FilmHeight == other.FilmHeight &&
                AllocationGuardRows == other.AllocationGuardRows &&
                RepresentedLateralMinimumMetres.Equals(
                    other.RepresentedLateralMinimumMetres) &&
                RepresentedLateralMaximumMetres.Equals(
                    other.RepresentedLateralMaximumMetres);
        }

        public override bool Equals(object obj)
        {
            return obj is StylizedRiverFoamGridDescriptor other &&
                Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked(
                (int)InitializationSignature ^
                (int)(InitializationSignature >> 32));
        }

        public static bool operator ==(
            StylizedRiverFoamGridDescriptor left,
            StylizedRiverFoamGridDescriptor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            StylizedRiverFoamGridDescriptor left,
            StylizedRiverFoamGridDescriptor right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return
                $"{Mapping} v{MappingContractVersion}, " +
                $"{ColumnCount}x{RowCount}, " +
                $"dx {ResolvedDxMetres:0.####} m, " +
                (UsesFixedMetricLattice
                    ? $"dy {ResolvedDyMetres:0.####} m, " +
                        $"globalY [{GlobalYBase}, " +
                        $"{GlobalYBase + RowCount - 1}]"
                    : "dy variable by river row");
        }

        [Conditional("UNITY_EDITOR")]
        public static void ValidateFoundation()
        {
            if (foundationValidated)
            {
                return;
            }

            AssertFoundation(
                Marshal.SizeOf<StylizedRiverFoamGridGpuData>() ==
                    StylizedRiverFoamGridGpuData.ExpectedStrideBytes,
                "Foam grid GPU descriptor stride is not 80 bytes.");
            AssertFoundation(
                Marshal.OffsetOf<StylizedRiverFoamGridGpuData>(
                    nameof(StylizedRiverFoamGridGpuData.Contract)).ToInt32() ==
                    0,
                "Foam grid GPU Contract offset changed.");
            AssertFoundation(
                Marshal.OffsetOf<StylizedRiverFoamGridGpuData>(
                    nameof(StylizedRiverFoamGridGpuData.Spacing)).ToInt32() ==
                    16,
                "Foam grid GPU Spacing offset changed.");
            AssertFoundation(
                Marshal.OffsetOf<StylizedRiverFoamGridGpuData>(
                    nameof(StylizedRiverFoamGridGpuData.Lateral)).ToInt32() ==
                    32,
                "Foam grid GPU Lateral offset changed.");
            AssertFoundation(
                Marshal.OffsetOf<StylizedRiverFoamGridGpuData>(
                    nameof(StylizedRiverFoamGridGpuData.Longitudinal)).ToInt32() ==
                    48,
                "Foam grid GPU Longitudinal offset changed.");
            AssertFoundation(
                Marshal.OffsetOf<StylizedRiverFoamGridGpuData>(
                    nameof(StylizedRiverFoamGridGpuData.Extent)).ToInt32() ==
                    64,
                "Foam grid GPU Extent offset changed.");

            bool created = TryCreateFixedMetricCandidate(
                StylizedRiverQuality.Medium,
                TargetCandidateCellSizeMetres,
                TargetCandidateCellSizeMetres,
                LongitudinalChunkLengthMetres,
                -2.5f,
                2.5f,
                0f,
                0,
                8192,
                out StylizedRiverFoamGridDescriptor candidate,
                out string failureReason);
            AssertFoundation(
                created,
                "Foam metric descriptor candidate failed: " + failureReason);
            AssertFoundation(
                candidate.ColumnsPerChunk == 214 &&
                candidate.ColumnCount == 214 &&
                candidate.GlobalYBase == -17 &&
                candidate.RowCount == 35 &&
                candidate.FilmWidth == 107 &&
                candidate.FilmHeight == 18,
                "Foam metric descriptor centreline-lattice dimensions changed.");
            AssertFoundation(
                Mathf.Abs(candidate.ResolveLateralMetresAtRowCentre(17)) <=
                    0.000001f,
                "Foam global Y zero is not centred on the river centreline.");
            StylizedRiverFoamGridDescriptor duplicate = candidate;
            AssertFoundation(
                candidate == duplicate &&
                candidate.InitializationSignature != 0ul,
                "Foam descriptor equality or signature is invalid.");
            AssertFoundation(
                TryCreateFromCacheContract(
                    DescriptorContractVersion,
                    candidate.Mapping,
                    candidate.MappingContractVersion,
                    candidate.Quality,
                    candidate.RequestedDxMetres,
                    candidate.RequestedDyMetres,
                    candidate.ColumnsPerChunk,
                    candidate.ResolvedDxMetres,
                    candidate.ResolvedDyMetres,
                    candidate.LateralLatticePhaseMetres,
                    candidate.GlobalYBase,
                    candidate.RowCount,
                    candidate.FieldOrStripStartMetres,
                    candidate.AllocatedLengthMetres,
                    candidate.ValidLengthMetres,
                    candidate.ColumnCount,
                    candidate.FilmWidth,
                    candidate.FilmHeight,
                    candidate.AllocationGuardRows,
                    candidate.RepresentedLateralMinimumMetres,
                    candidate.RepresentedLateralMaximumMetres,
                    candidate.InitializationSignature,
                    out StylizedRiverFoamGridDescriptor reconstructed,
                    out string reconstructionFailure) &&
                reconstructed == candidate,
                "Foam cache descriptor reconstruction failed: " +
                reconstructionFailure);

            StylizedRiverFoamGridGpuData gpuData = candidate.ToGpuData();
            AssertFoundation(
                Mathf.RoundToInt(gpuData.Contract.x) ==
                    DescriptorContractVersion &&
                Mathf.RoundToInt(gpuData.Contract.y) ==
                    FixedMetricMappingContractVersion &&
                Mathf.RoundToInt(gpuData.Contract.z) ==
                    (int)StylizedRiverFoamGridMapping.FixedMetricLattice &&
                Mathf.RoundToInt(gpuData.Longitudinal.w) == 214,
                "Foam CPU/GPU descriptor lanes changed.");

            Vector2 sourceCentre = candidate.ResolveMetricPositionAtCellCentre(
                45,
                3);
            AssertFoundation(
                candidate.TryMetricToFractionalCellPosition(
                    sourceCentre,
                    out Vector2 fractionalCell) &&
                Mathf.Abs(fractionalCell.x - 45.5f) <= 0.00001f &&
                Mathf.Abs(fractionalCell.y - 3.5f) <= 0.00001f,
                "Foam metric cell-centre round trip changed.");
            AssertFoundation(
                candidate.TryMetricToNearestCell(
                    sourceCentre,
                    out Vector2Int nearestCell) &&
                nearestCell == new Vector2Int(45, 3),
                "Foam metric nearest-cell conversion changed.");
            Vector2 allocatedMinimum = new Vector2(
                candidate.AllocatedLocalDistanceMinimumMetres,
                candidate.RepresentedLateralMinimumMetres);
            Vector2 allocatedMaximum = new Vector2(
                candidate.AllocatedLocalDistanceMaximumMetres,
                candidate.RepresentedLateralMaximumMetres);
            AssertFoundation(
                candidate.TryMetricToContainingCell(
                    allocatedMinimum,
                    out Vector2Int minimumCell) &&
                minimumCell == Vector2Int.zero &&
                candidate.TryMetricToContainingCell(
                    allocatedMaximum,
                    out Vector2Int maximumCell) &&
                maximumCell == new Vector2Int(
                    candidate.ColumnCount - 1,
                    candidate.RowCount - 1),
                "Foam metric represented-boundary conversion changed.");
            AssertFoundation(
                !candidate.TryMetricToFractionalCellPosition(
                    new Vector2(
                        candidate.AllocatedLocalDistanceMaximumMetres + 0.01f,
                        0f),
                    out _),
                "Foam metric out-of-field X was accepted.");
            AssertFoundation(
                !candidate.TryMetricToFractionalCellPosition(
                    new Vector2(
                        1f,
                        candidate.RepresentedLateralMaximumMetres + 0.01f),
                    out _),
                "Foam metric out-of-field Y was accepted.");

            ValidateCandidateSweep(
                ConservativeCandidateCellSizeMetres,
                128);
            ValidateCandidateSweep(
                IntermediateCandidateCellSizeMetres,
                160);
            ValidateCandidateSweep(
                TargetCandidateCellSizeMetres,
                214);
            ValidateCandidateSweep(
                StressCandidateCellSizeMetres,
                320);
            AssertFoundation(
                !TryCreateFixedMetricOneStrip(
                    StylizedRiverQuality.Medium,
                    TargetCandidateCellSizeMetres,
                    TargetCandidateCellSizeMetres,
                    LongitudinalChunkLengthMetres,
                    -2.5f,
                    2.5f,
                    0f,
                    0,
                    100,
                    out _,
                    out string dimensionFailure) &&
                dimensionFailure.Contains("longitudinal"),
                "Foam metric dimension-limit failure policy changed.");
            foundationValidated = true;
        }

        private static void ValidateCandidateSweep(
            float requestedSpacingMetres,
            int expectedColumnsPerChunk)
        {
            bool created = TryCreateFixedMetricOneStrip(
                StylizedRiverQuality.Medium,
                requestedSpacingMetres,
                requestedSpacingMetres,
                LongitudinalChunkLengthMetres * 1.25f,
                -3.2f,
                1.4f,
                0.025f,
                0,
                8192,
                out StylizedRiverFoamGridDescriptor candidate,
                out string failureReason);
            AssertFoundation(
                created,
                "Foam metric candidate sweep failed: " + failureReason);
            AssertFoundation(
                candidate.ColumnsPerChunk == expectedColumnsPerChunk &&
                candidate.ColumnCount == expectedColumnsPerChunk * 2 &&
                candidate.GlobalYBase < 0 &&
                candidate.GlobalYMaximum > 0 &&
                candidate.ContainsValidLocalDistance(
                    candidate.ValidLocalDistanceMaximumMetres),
                "Foam metric candidate sweep dimensions changed at " +
                    requestedSpacingMetres + " m.");

            int localY = candidate.RowCount / 2;
            Vector2 metricCentre = candidate.ResolveMetricPositionAtCellCentre(
                candidate.ColumnCount - 1,
                localY);
            AssertFoundation(
                candidate.TryMetricToNearestCell(
                    metricCentre,
                    out Vector2Int cell) &&
                cell.x == candidate.ColumnCount - 1 &&
                cell.y == localY,
                "Foam metric candidate sweep round trip changed at " +
                    requestedSpacingMetres + " m.");
        }

        private static ulong CalculateInitializationSignature(
            StylizedRiverFoamGridMapping mapping,
            int mappingContractVersion,
            StylizedRiverQuality quality,
            float requestedDxMetres,
            float requestedDyMetres,
            int columnsPerChunk,
            float resolvedDxMetres,
            float resolvedDyMetres,
            float lateralLatticePhaseMetres,
            int globalYBase,
            int rowCount,
            float fieldOrStripStartMetres,
            float allocatedLengthMetres,
            float validLengthMetres,
            int columnCount,
            int filmWidth,
            int filmHeight,
            int allocationGuardRows,
            float representedLateralMinimumMetres,
            float representedLateralMaximumMetres)
        {
            const ulong offsetBasis = 1469598103934665603ul;
            const ulong prime = 1099511628211ul;
            ulong hash = offsetBasis;

            AddInt(ref hash, prime, DescriptorContractVersion);
            AddInt(ref hash, prime, (int)mapping);
            AddInt(ref hash, prime, mappingContractVersion);
            AddInt(ref hash, prime, (int)quality);
            AddFloat(ref hash, prime, requestedDxMetres);
            AddFloat(ref hash, prime, requestedDyMetres);
            AddInt(ref hash, prime, columnsPerChunk);
            AddFloat(ref hash, prime, resolvedDxMetres);
            AddFloat(ref hash, prime, resolvedDyMetres);
            AddFloat(ref hash, prime, lateralLatticePhaseMetres);
            AddInt(ref hash, prime, globalYBase);
            AddInt(ref hash, prime, rowCount);
            AddFloat(ref hash, prime, fieldOrStripStartMetres);
            AddFloat(ref hash, prime, allocatedLengthMetres);
            AddFloat(ref hash, prime, validLengthMetres);
            AddInt(ref hash, prime, columnCount);
            AddInt(ref hash, prime, filmWidth);
            AddInt(ref hash, prime, filmHeight);
            AddInt(ref hash, prime, allocationGuardRows);
            AddFloat(ref hash, prime, representedLateralMinimumMetres);
            AddFloat(ref hash, prime, representedLateralMaximumMetres);
            return hash;
        }

        private static void AddInt(ref ulong hash, ulong prime, int value)
        {
            unchecked
            {
                uint bits = (uint)value;
                for (int index = 0; index < 4; index++)
                {
                    hash ^= (byte)(bits >> (index * 8));
                    hash *= prime;
                }
            }
        }

        private static void AddFloat(
            ref ulong hash,
            ulong prime,
            float value)
        {
            AddInt(ref hash, prime, BitConverter.SingleToInt32Bits(value));
        }

        private static bool IsPositiveFinite(float value)
        {
            return value > 0f && IsFinite(value);
        }

        private static bool IsNonNegativeFinite(float value)
        {
            return value >= 0f && IsFinite(value);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void AssertFoundation(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
