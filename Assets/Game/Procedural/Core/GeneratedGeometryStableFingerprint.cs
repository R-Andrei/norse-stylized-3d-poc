using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry
{
    /// <summary>
    /// Stable 128-bit fingerprint of exact generated world-space triangle
    /// geometry. Generated owners may prepare this when their mesh changes so
    /// consumers can validate persistent data without rescanning triangles.
    /// </summary>
    public readonly struct GeneratedGeometryStableFingerprint :
        IEquatable<GeneratedGeometryStableFingerprint>,
        IComparable<GeneratedGeometryStableFingerprint>
    {
        public GeneratedGeometryStableFingerprint(ulong low, ulong high)
        {
            Low = low;
            High = high;
        }

        public ulong Low { get; }
        public ulong High { get; }

        public bool Equals(GeneratedGeometryStableFingerprint other)
        {
            return Low == other.Low && High == other.High;
        }

        public override bool Equals(object obj)
        {
            return obj is GeneratedGeometryStableFingerprint other &&
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

        public int CompareTo(GeneratedGeometryStableFingerprint other)
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
    }

    public interface IGeneratedGeometryStableFingerprintSource
    {
        bool TryGetStableWorldGeometryFingerprint(
            out GeneratedGeometryStableFingerprint fingerprint,
            out string status);
    }

    public static class GeneratedGeometryStableFingerprintUtility
    {
        private const int ContractVersion = 1;

        public static bool TryComputeExactWorldTriangleFingerprint(
            MeshFilter meshFilter,
            out GeneratedGeometryStableFingerprint fingerprint,
            out string status)
        {
            fingerprint = default;
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                status = "The generated MeshFilter has no readable mesh.";
                return false;
            }

            Vector3[] localVertices;
            int[] triangles;
            try
            {
                localVertices = meshFilter.sharedMesh.vertices;
                triangles = meshFilter.sharedMesh.triangles;
            }
            catch (UnityException exception)
            {
                status =
                    $"The generated mesh is not CPU-readable: {exception.Message}";
                return false;
            }

            if (localVertices == null || localVertices.Length < 3 ||
                triangles == null || triangles.Length < 3)
            {
                status =
                    "The generated mesh does not contain readable triangle geometry.";
                return false;
            }

            GeneratedGeometryStableHashBuilder builder =
                GeneratedGeometryStableHashBuilder.Create(
                "PS3D.RiverFoam.ObstacleSource");
            builder.AddInt32(ContractVersion);
            builder.AddInt32(localVertices.Length);

            Transform meshTransform = meshFilter.transform;
            for (int index = 0; index < localVertices.Length; index++)
            {
                builder.AddVector3(
                    meshTransform.TransformPoint(localVertices[index]));
            }

            builder.AddInt32(triangles.Length);
            for (int index = 0; index < triangles.Length; index++)
            {
                builder.AddInt32(triangles[index]);
            }

            fingerprint = builder.Finish();
            status = "Prepared the exact world-space triangle fingerprint.";
            return true;
        }

    }

    /// <summary>
    /// Byte-order-defined two-lane stable hash used by generated-geometry
    /// owners and river cache contracts. Keeping one implementation preserves
    /// the accepted 4.9B bytes without duplicating the hashing algorithm.
    /// </summary>
    internal struct GeneratedGeometryStableHashBuilder
    {
        private const ulong LowOffset = 14695981039346656037ul;
        private const ulong HighOffset = 7809847782465536322ul;
        private const ulong LowPrime = 1099511628211ul;
        private const ulong HighPrime = 14029467366897019727ul;

        private ulong low;
        private ulong high;

        public static GeneratedGeometryStableHashBuilder Create(
            string contract)
        {
            GeneratedGeometryStableHashBuilder builder = new()
            {
                low = LowOffset,
                high = HighOffset
            };
            builder.AddString(contract ?? string.Empty);
            return builder;
        }

        public void AddBoolean(bool value)
        {
            AddByte(value ? (byte)1 : (byte)0);
        }

        public void AddInt32(int value)
        {
            AddUInt32(unchecked((uint)value));
        }

        public void AddUInt32(uint value)
        {
            AddByte((byte)value);
            AddByte((byte)(value >> 8));
            AddByte((byte)(value >> 16));
            AddByte((byte)(value >> 24));
        }

        public void AddUInt64(ulong value)
        {
            AddUInt32((uint)value);
            AddUInt32((uint)(value >> 32));
        }

        public void AddSingle(float value)
        {
            AddInt32(BitConverter.SingleToInt32Bits(value));
        }

        public void AddVector2(Vector2 value)
        {
            AddSingle(value.x);
            AddSingle(value.y);
        }

        public void AddVector3(Vector3 value)
        {
            AddSingle(value.x);
            AddSingle(value.y);
            AddSingle(value.z);
        }

        public void AddFingerprint(
            GeneratedGeometryStableFingerprint fingerprint)
        {
            AddUInt64(fingerprint.Low);
            AddUInt64(fingerprint.High);
        }

        public void AddString(string value)
        {
            value ??= string.Empty;
            AddInt32(value.Length);
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                AddByte((byte)character);
                AddByte((byte)(character >> 8));
            }
        }

        public GeneratedGeometryStableFingerprint Finish()
        {
            ulong mixedLow = low ^ RotateLeft(high, 17);
            ulong mixedHigh = high ^ RotateLeft(low, 41);
            return new GeneratedGeometryStableFingerprint(
                Avalanche(mixedLow),
                Avalanche(mixedHigh));
        }

        private void AddByte(byte value)
        {
            unchecked
            {
                low ^= value;
                low *= LowPrime;

                high ^= (byte)(value ^ 0xA5);
                high *= HighPrime;
                high = RotateLeft(high, 7);
            }
        }

        private static ulong RotateLeft(ulong value, int amount)
        {
            return (value << amount) | (value >> (64 - amount));
        }

        private static ulong Avalanche(ulong value)
        {
            unchecked
            {
                value ^= value >> 30;
                value *= 0xBF58476D1CE4E5B9ul;
                value ^= value >> 27;
                value *= 0x94D049BB133111EBul;
                value ^= value >> 31;
                return value;
            }
        }
    }
}
