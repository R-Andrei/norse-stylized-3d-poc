using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    public enum TreeAgeClass
    {
        [InspectorName("Young")]
        Young = 0,

        [InspectorName("Mature")]
        Mature = 1,

        [InspectorName("Old Growth")]
        OldGrowth = 2,

        [InspectorName("Declining")]
        Declining = 3
    }

    [Flags]
    public enum TreeBranchState
    {
        None = 0,
        Dead = 1 << 0,
        Broken = 1 << 1
    }

    public enum TreeOverrideMode
    {
        [InspectorName("Inherit")]
        Inherit = 0,

        [InspectorName("Exact")]
        Exact = 1,

        [InspectorName("Range")]
        Range = 2
    }



    public enum TreeStructuralPreviewScope
    {
        [InspectorName("Selected Tree")]
        SelectedTree = 0,

        [InspectorName("Selected Family")]
        SelectedFamily = 1,

        [InspectorName("All Trees")]
        AllTrees = 2
    }

    public enum TreeSeedStream
    {
        TrunkShape = 0,
        TrunkForks = 1,
        PrimaryBranchLayout = 2,
        SecondaryBranchLayout = 3,
        TertiaryBranchLayout = 4,
        BranchCurvature = 5,
        StructuralDamage = 6,
        FoliageClusterPlacement = 7,
        FoliageClusterShape = 8,
        FoliageCardPlacement = 9,
        FoliageCardShape = 10,
        MaterialVariation = 11,
        LodSelection = 12,
        ProxyGeneration = 13
    }

    [Serializable]
    public struct TreeFloatRange
    {
        [SerializeField]
        private float minimum;

        [SerializeField]
        private float maximum;

        public TreeFloatRange(float minimumValue, float maximumValue)
        {
            minimum = minimumValue;
            maximum = maximumValue;
        }

        public float Minimum => minimum;
        public float Maximum => maximum;
        public float Midpoint => (minimum + maximum) * 0.5f;
        public bool IsValid =>
            TreeDeterministicUtility.IsFinite(minimum) &&
            TreeDeterministicUtility.IsFinite(maximum) &&
            maximum >= minimum;

        public float Sample(int seed, string parameterKey)
        {
            if (maximum <= minimum)
            {
                return minimum;
            }

            return Mathf.Lerp(
                minimum,
                maximum,
                TreeDeterministicUtility.Sample01(seed, parameterKey));
        }

        public float Clamp(float value)
        {
            return Mathf.Clamp(value, minimum, maximum);
        }

        public TreeFloatRange Ordered()
        {
            return minimum <= maximum
                ? this
                : new TreeFloatRange(maximum, minimum);
        }
    }

    [Serializable]
    public struct TreeIntRange
    {
        [SerializeField]
        private int minimum;

        [SerializeField]
        private int maximum;

        public TreeIntRange(int minimumValue, int maximumValue)
        {
            minimum = minimumValue;
            maximum = maximumValue;
        }

        public int Minimum => minimum;
        public int Maximum => maximum;
        public int Midpoint => minimum + ((maximum - minimum) / 2);
        public bool IsValid => maximum >= minimum;

        public int Sample(int seed, string parameterKey)
        {
            if (maximum <= minimum)
            {
                return minimum;
            }

            float value = TreeDeterministicUtility.Sample01(seed, parameterKey);
            int span = maximum - minimum + 1;
            return minimum + Mathf.Min(span - 1, Mathf.FloorToInt(value * span));
        }

        public int Clamp(int value)
        {
            return Mathf.Clamp(value, minimum, maximum);
        }

        public TreeIntRange Ordered()
        {
            return minimum <= maximum
                ? this
                : new TreeIntRange(maximum, minimum);
        }
    }

    [Serializable]
    public struct TreeFloatOverride
    {
        [SerializeField]
        private TreeOverrideMode mode;

        [SerializeField]
        private float exactValue;

        [SerializeField]
        private TreeFloatRange range;

        public TreeOverrideMode Mode => mode;
        public bool IsSet => mode != TreeOverrideMode.Inherit;
        public float ExactValue => exactValue;
        public TreeFloatRange Range => range;

        public static TreeFloatOverride Exact(float value)
        {
            return new TreeFloatOverride
            {
                mode = TreeOverrideMode.Exact,
                exactValue = value,
                range = new TreeFloatRange(value, value)
            };
        }

        public static TreeFloatOverride Ranged(float minimum, float maximum)
        {
            return new TreeFloatOverride
            {
                mode = TreeOverrideMode.Range,
                exactValue = (minimum + maximum) * 0.5f,
                range = new TreeFloatRange(minimum, maximum)
            };
        }

        public float Resolve(float inherited, int seed, string parameterKey)
        {
            switch (mode)
            {
                case TreeOverrideMode.Exact:
                    return exactValue;
                case TreeOverrideMode.Range:
                    return range.Ordered().Sample(seed, parameterKey);
                default:
                    return inherited;
            }
        }
    }

    [Serializable]
    public struct TreeIntOverride
    {
        [SerializeField]
        private TreeOverrideMode mode;

        [SerializeField]
        private int exactValue;

        [SerializeField]
        private TreeIntRange range;

        public TreeOverrideMode Mode => mode;
        public bool IsSet => mode != TreeOverrideMode.Inherit;
        public int ExactValue => exactValue;
        public TreeIntRange Range => range;

        public static TreeIntOverride Exact(int value)
        {
            return new TreeIntOverride
            {
                mode = TreeOverrideMode.Exact,
                exactValue = value,
                range = new TreeIntRange(value, value)
            };
        }

        public static TreeIntOverride Ranged(int minimum, int maximum)
        {
            return new TreeIntOverride
            {
                mode = TreeOverrideMode.Range,
                exactValue = minimum + ((maximum - minimum) / 2),
                range = new TreeIntRange(minimum, maximum)
            };
        }

        public int Resolve(int inherited, int seed, string parameterKey)
        {
            switch (mode)
            {
                case TreeOverrideMode.Exact:
                    return exactValue;
                case TreeOverrideMode.Range:
                    return range.Ordered().Sample(seed, parameterKey);
                default:
                    return inherited;
            }
        }
    }

    [Serializable]
    public struct TreeColorOverride
    {
        [SerializeField]
        private bool enabled;

        [SerializeField]
        private Color value;

        public bool Enabled => enabled;
        public Color Value => value;

        public static TreeColorOverride Exact(Color color)
        {
            return new TreeColorOverride
            {
                enabled = true,
                value = color
            };
        }

        public Color Resolve(Color inherited)
        {
            return enabled ? value : inherited;
        }
    }

    [Serializable]
    public struct TreeSeedLock
    {
        [SerializeField]
        private TreeSeedStream stream;

        [SerializeField]
        private bool locked;

        [SerializeField]
        private int seed;

        public TreeSeedStream Stream => stream;
        public bool Locked => locked;
        public int Seed => seed;
    }

    [Serializable]
    public struct TreeSeedRecord
    {
        [SerializeField]
        private TreeSeedStream stream;

        [SerializeField]
        private int seed;

        [SerializeField]
        private bool locked;

        public TreeSeedRecord(TreeSeedStream streamValue, int seedValue, bool isLocked)
        {
            stream = streamValue;
            seed = seedValue;
            locked = isLocked;
        }

        public TreeSeedStream Stream => stream;
        public int Seed => seed;
        public bool Locked => locked;
    }

    [Serializable]
    public sealed class TreeSeedSet
    {
        [SerializeField]
        private List<TreeSeedRecord> records = new List<TreeSeedRecord>();

        public IReadOnlyList<TreeSeedRecord> Records => records;

        public int GetSeed(TreeSeedStream stream)
        {
            for (int index = 0; index < records.Count; index++)
            {
                if (records[index].Stream == stream)
                {
                    return records[index].Seed;
                }
            }

            return 0;
        }

        internal void SetRecords(List<TreeSeedRecord> values)
        {
            records = values ?? new List<TreeSeedRecord>();
        }
    }

    public static class TreeDeterministicUtility
    {
        private const ulong FnvOffsetBasis = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        internal static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public static int DeriveSeed(params object[] values)
        {
            ulong hash = FnvOffsetBasis;
            for (int index = 0; index < values.Length; index++)
            {
                AppendObject(ref hash, values[index]);
            }

            int seed = unchecked((int)(hash ^ (hash >> 32)));
            return seed == int.MinValue ? 0 : Mathf.Abs(seed);
        }

        internal static float Sample01(int seed, string parameterKey)
        {
            int parameterSeed = DeriveSeed(seed, parameterKey ?? string.Empty);
            uint value = unchecked((uint)parameterSeed);
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            return (value & 0x00FFFFFFu) / 16777216f;
        }

        internal static float SampleSigned(int seed, string parameterKey)
        {
            return Sample01(seed, parameterKey) * 2f - 1f;
        }

        internal static Vector2 DirectionXZ(int seed, string parameterKey)
        {
            float angle = Sample01(seed, parameterKey) * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        internal static string FormatHash(ulong value)
        {
            return value.ToString("X16");
        }

        internal static ulong BeginHash()
        {
            return FnvOffsetBasis;
        }

        internal static void Append(ref ulong hash, int value)
        {
            Append(ref hash, unchecked((uint)value));
        }

        internal static void Append(ref ulong hash, uint value)
        {
            hash ^= value & 0xFFu;
            hash *= FnvPrime;
            hash ^= (value >> 8) & 0xFFu;
            hash *= FnvPrime;
            hash ^= (value >> 16) & 0xFFu;
            hash *= FnvPrime;
            hash ^= (value >> 24) & 0xFFu;
            hash *= FnvPrime;
        }

        internal static void Append(ref ulong hash, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Append(ref hash, BitConverter.ToInt32(bytes, 0));
        }

        internal static void Append(ref ulong hash, bool value)
        {
            Append(ref hash, value ? 1 : 0);
        }

        internal static void Append(ref ulong hash, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Append(ref hash, 0);
                return;
            }

            for (int index = 0; index < value.Length; index++)
            {
                ushort character = value[index];
                hash ^= (ulong)(character & 0xFF);
                hash *= FnvPrime;
                hash ^= (ulong)((character >> 8) & 0xFF);
                hash *= FnvPrime;
            }
        }

        internal static void Append(ref ulong hash, Vector3 value)
        {
            Append(ref hash, value.x);
            Append(ref hash, value.y);
            Append(ref hash, value.z);
        }

        internal static void Append(ref ulong hash, Color value)
        {
            Append(ref hash, value.r);
            Append(ref hash, value.g);
            Append(ref hash, value.b);
            Append(ref hash, value.a);
        }

        private static void AppendObject(ref ulong hash, object value)
        {
            if (value == null)
            {
                Append(ref hash, 0);
                return;
            }

            switch (value)
            {
                case int integer:
                    Append(ref hash, integer);
                    break;
                case uint unsignedInteger:
                    Append(ref hash, unsignedInteger);
                    break;
                case float single:
                    Append(ref hash, single);
                    break;
                case bool boolean:
                    Append(ref hash, boolean);
                    break;
                case Enum enumeration:
                    Append(ref hash, Convert.ToInt32(enumeration));
                    break;
                default:
                    Append(ref hash, value.ToString());
                    break;
            }
        }
    }
}
