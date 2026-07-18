using System;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Stable 128-bit content fingerprint used by explicit cache validation
    /// and cache-first startup.
    /// It deliberately excludes session-local revision counters and runtime
    /// timings. Values are hashed from exact IEEE-754 bits and explicit integer
    /// fields so unchanged authored/cache inputs reproduce across sessions.
    /// </summary>
    internal readonly struct StylizedRiverFoamTopologyFingerprint :
        IEquatable<StylizedRiverFoamTopologyFingerprint>,
        IComparable<StylizedRiverFoamTopologyFingerprint>
    {
        public StylizedRiverFoamTopologyFingerprint(ulong low, ulong high)
        {
            Low = low;
            High = high;
        }

        public ulong Low { get; }
        public ulong High { get; }

        public bool Equals(StylizedRiverFoamTopologyFingerprint other)
        {
            return Low == other.Low && High == other.High;
        }

        public override bool Equals(object obj)
        {
            return obj is StylizedRiverFoamTopologyFingerprint other &&
                Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Low * 397) ^ (int)(Low >> 32) ^
                    (int)High ^ (int)(High >> 32);
            }
        }

        public int CompareTo(StylizedRiverFoamTopologyFingerprint other)
        {
            int highComparison = High.CompareTo(other.High);
            return highComparison != 0
                ? highComparison
                : Low.CompareTo(other.Low);
        }

        public override string ToString()
        {
            return $"{High:X16}{Low:X16}";
        }

        public static bool operator ==(
            StylizedRiverFoamTopologyFingerprint left,
            StylizedRiverFoamTopologyFingerprint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            StylizedRiverFoamTopologyFingerprint left,
            StylizedRiverFoamTopologyFingerprint right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Allocation-free, byte-order-defined two-lane FNV-style builder. The
    /// second lane uses an independent seed and byte transform so the stored
    /// 128-bit value is substantially safer as a persistent cache key than the
    /// old 32-bit in-session signatures.
    /// </summary>
    internal struct StylizedRiverFoamStableHashBuilder
    {
        private GeneratedGeometryStableHashBuilder builder;

        public static StylizedRiverFoamStableHashBuilder Create(
            string contract)
        {
            return new StylizedRiverFoamStableHashBuilder
            {
                builder = GeneratedGeometryStableHashBuilder.Create(contract)
            };
        }

        public void AddBoolean(bool value)
        {
            builder.AddBoolean(value);
        }

        public void AddInt32(int value)
        {
            builder.AddInt32(value);
        }

        public void AddUInt32(uint value)
        {
            builder.AddUInt32(value);
        }

        public void AddUInt64(ulong value)
        {
            builder.AddUInt64(value);
        }

        public void AddSingle(float value)
        {
            builder.AddSingle(value);
        }

        public void AddVector2(Vector2 value)
        {
            builder.AddVector2(value);
        }

        public void AddVector3(Vector3 value)
        {
            builder.AddVector3(value);
        }

        public void AddFingerprint(
            StylizedRiverFoamTopologyFingerprint fingerprint)
        {
            builder.AddUInt64(fingerprint.Low);
            builder.AddUInt64(fingerprint.High);
        }

        public void AddString(string value)
        {
            builder.AddString(value);
        }

        public StylizedRiverFoamTopologyFingerprint Finish()
        {
            GeneratedGeometryStableFingerprint fingerprint = builder.Finish();
            return new StylizedRiverFoamTopologyFingerprint(
                fingerprint.Low,
                fingerprint.High);
        }
    }

    internal static class StylizedRiverFoamTopologyFingerprints
    {
        private const int DomainFingerprintContractVersion = 1;
        private const int GenerationFingerprintContractVersion = 2;
        private const int CombinedFingerprintContractVersion = 2;

        public static StylizedRiverFoamTopologyFingerprint ComputeDomain(
            RiverDomainSnapshot domain)
        {
            StylizedRiverFoamStableHashBuilder builder =
                StylizedRiverFoamStableHashBuilder.Create(
                    "PS3D.RiverFoam.Domain");
            builder.AddInt32(DomainFingerprintContractVersion);

            if (domain == null)
            {
                builder.AddBoolean(false);
                return builder.Finish();
            }

            builder.AddBoolean(true);
            builder.AddSingle(domain.LocalLength);
            builder.AddSingle(domain.RequestedSampleSpacing);
            builder.AddSingle(domain.ConnectedDistanceOffset);
            builder.AddBoolean(domain.ReverseFlow);
            builder.AddInt32(domain.SampleCount);

            for (int index = 0; index < domain.SampleCount; index++)
            {
                StylizedRiverSplineSample sample = domain.Samples[index];
                builder.AddVector3(sample.Centre);
                builder.AddVector3(sample.SurfacePoint);
                builder.AddVector3(sample.Tangent);
                builder.AddVector3(sample.Side);
                builder.AddVector3(sample.Up);
                builder.AddSingle(sample.Distance);
                builder.AddSingle(sample.OrientedDistance);
                builder.AddSingle(sample.GlobalDistance);
                builder.AddSingle(sample.LeftHalfWidth);
                builder.AddSingle(sample.RightHalfWidth);
                builder.AddSingle(sample.LeftSurfaceHalfWidth);
                builder.AddSingle(sample.RightSurfaceHalfWidth);
                builder.AddSingle(sample.NormalizedDistance);
                builder.AddSingle(sample.NormalizedTime);
            }

            return builder.Finish();
        }

        public static StylizedRiverFoamTopologyFingerprint ComputeGeneration(
            StylizedRiver river,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            StylizedRiverFoamStableHashBuilder builder =
                StylizedRiverFoamStableHashBuilder.Create(
                    "PS3D.RiverFoam.GenerationInputs");
            builder.AddInt32(GenerationFingerprintContractVersion);

            if (river == null)
            {
                builder.AddBoolean(false);
                return builder.Finish();
            }

            builder.AddBoolean(true);
            AddGridDescriptor(ref builder, descriptor);
            builder.AddSingle(river.ShoreMotion);
            builder.AddInt32(river.FoamMajorSupportSeed);
            builder.AddSingle(river.FoamMajorSupportAmount);
            builder.AddSingle(river.FoamMajorSupportSize);
            builder.AddSingle(river.FoamMajorSupportSizeVariation);
            builder.AddSingle(
                river.FoamMajorRecycleTerritoryDeviationPercent);
            builder.AddSingle(river.FoamConnectorAmount);
            builder.AddSingle(river.FoamConnectorDirectness);
            builder.AddSingle(river.FoamConnectorLengthPreference);
            builder.AddSingle(river.FoamInteriorPocketAmount);
            builder.AddSingle(river.FoamEdgeCavityAmount);
            builder.AddSingle(river.FoamConnectorWeakSpanAmount);
            builder.AddSingle(river.FoamFreeWaterEventAmount);
            return builder.Finish();
        }

        private static void AddGridDescriptor(
            ref StylizedRiverFoamStableHashBuilder builder,
            StylizedRiverFoamGridDescriptor descriptor)
        {
            builder.AddInt32(
                StylizedRiverFoamGridDescriptor.DescriptorContractVersion);
            builder.AddInt32((int)descriptor.Mapping);
            builder.AddInt32(descriptor.MappingContractVersion);
            builder.AddInt32((int)descriptor.Quality);
            builder.AddSingle(descriptor.RequestedDxMetres);
            builder.AddSingle(descriptor.RequestedDyMetres);
            builder.AddInt32(descriptor.ColumnsPerChunk);
            builder.AddSingle(descriptor.ResolvedDxMetres);
            builder.AddSingle(descriptor.ResolvedDyMetres);
            builder.AddSingle(descriptor.LateralLatticePhaseMetres);
            builder.AddInt32(descriptor.GlobalYBase);
            builder.AddInt32(descriptor.RowCount);
            builder.AddSingle(descriptor.FieldOrStripStartMetres);
            builder.AddSingle(descriptor.AllocatedLengthMetres);
            builder.AddSingle(descriptor.ValidLengthMetres);
            builder.AddInt32(descriptor.ColumnCount);
            builder.AddInt32(descriptor.FilmWidth);
            builder.AddInt32(descriptor.FilmHeight);
            builder.AddInt32(descriptor.AllocationGuardRows);
            builder.AddSingle(
                descriptor.RepresentedLateralMinimumMetres);
            builder.AddSingle(
                descriptor.RepresentedLateralMaximumMetres);
            builder.AddUInt64(descriptor.InitializationSignature);
        }

        public static StylizedRiverFoamTopologyFingerprint ComputeCombined(
            StylizedRiverFoamTopologyFingerprint domain,
            StylizedRiverFoamTopologyFingerprint obstacles,
            StylizedRiverFoamTopologyFingerprint generation)
        {
            StylizedRiverFoamStableHashBuilder builder =
                StylizedRiverFoamStableHashBuilder.Create(
                    "PS3D.RiverFoam.CombinedInputs");
            builder.AddInt32(CombinedFingerprintContractVersion);
            builder.AddFingerprint(domain);
            builder.AddFingerprint(obstacles);
            builder.AddFingerprint(generation);
            return builder.Finish();
        }
    }
}
