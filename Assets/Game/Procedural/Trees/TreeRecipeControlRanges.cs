using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public struct TreeFloatControlRange
    {
        [SerializeField] private float minimum;
        [SerializeField] private float maximum;

        public TreeFloatControlRange(float minimumValue, float maximumValue)
        {
            minimum = minimumValue;
            maximum = maximumValue;
        }

        public float Minimum => minimum;
        public float Maximum => maximum;

        public float Sample(int seed, string stableControlId)
        {
            if (maximum <= minimum)
            {
                return minimum;
            }

            return Mathf.Lerp(
                minimum,
                maximum,
                TreeDeterministicUtility.Sample01(seed, stableControlId));
        }

        internal void ClampAndOrder(float hardMinimum, float hardMaximum)
        {
            float first = TreeDeterministicUtility.IsFinite(minimum)
                ? Mathf.Clamp(minimum, hardMinimum, hardMaximum)
                : hardMinimum;
            float second = TreeDeterministicUtility.IsFinite(maximum)
                ? Mathf.Clamp(maximum, hardMinimum, hardMaximum)
                : hardMaximum;
            minimum = Mathf.Min(first, second);
            maximum = Mathf.Max(first, second);
        }
    }

    [Serializable]
    public struct TreeIntControlRange
    {
        [SerializeField] private int minimum;
        [SerializeField] private int maximum;

        public TreeIntControlRange(int minimumValue, int maximumValue)
        {
            minimum = minimumValue;
            maximum = maximumValue;
        }

        public int Minimum => minimum;
        public int Maximum => maximum;

        public int Sample(int seed, string stableControlId)
        {
            if (maximum <= minimum)
            {
                return minimum;
            }

            float value = TreeDeterministicUtility.Sample01(
                seed,
                stableControlId);
            int span = maximum - minimum + 1;
            return minimum + Mathf.Min(
                span - 1,
                Mathf.FloorToInt(value * span));
        }

        internal void ClampAndOrder(int hardMinimum, int hardMaximum)
        {
            int first = Mathf.Clamp(minimum, hardMinimum, hardMaximum);
            int second = Mathf.Clamp(maximum, hardMinimum, hardMaximum);
            minimum = Mathf.Min(first, second);
            maximum = Mathf.Max(first, second);
        }
    }

    [Serializable]
    public struct TreeAngleControlRange
    {
        [SerializeField] private float minimum;
        [SerializeField] private float maximum;

        public TreeAngleControlRange(float minimumValue, float maximumValue)
        {
            minimum = Normalize(minimumValue);
            maximum = Normalize(maximumValue);
        }

        public float Minimum => minimum;
        public float Maximum => maximum;
        public bool WrapsThroughZero => maximum < minimum;

        public float Sample(int seed, string stableControlId)
        {
            float sample = TreeDeterministicUtility.Sample01(
                seed,
                stableControlId);
            if (!WrapsThroughZero)
            {
                return Mathf.Lerp(minimum, maximum, sample);
            }

            float span = (360f - minimum) + maximum;
            return Normalize(minimum + span * sample);
        }

        internal void NormalizeValues()
        {
            minimum = Normalize(minimum);
            maximum = Normalize(maximum);
        }

        private static float Normalize(float value)
        {
            if (!TreeDeterministicUtility.IsFinite(value))
            {
                return 0f;
            }

            if (Mathf.Approximately(value, 360f))
            {
                return 360f;
            }

            value %= 360f;
            return value < 0f ? value + 360f : value;
        }
    }

    [Serializable]
    public struct TreeColorControlRange
    {
        [SerializeField] private Color minimum;
        [SerializeField] private Color maximum;

        public TreeColorControlRange(Color minimumValue, Color maximumValue)
        {
            minimum = minimumValue;
            maximum = maximumValue;
        }

        public Color Minimum => minimum;
        public Color Maximum => maximum;

        public Color Sample(int seed, string stableControlId)
        {
            return Color.Lerp(
                minimum,
                maximum,
                TreeDeterministicUtility.Sample01(seed, stableControlId));
        }

        internal void Clamp01()
        {
            minimum = ClampFiniteColor(minimum);
            maximum = ClampFiniteColor(maximum);
        }

        private static Color ClampFiniteColor(Color value)
        {
            return new Color(
                TreeDeterministicUtility.IsFinite(value.r)
                    ? Mathf.Clamp01(value.r)
                    : 0f,
                TreeDeterministicUtility.IsFinite(value.g)
                    ? Mathf.Clamp01(value.g)
                    : 0f,
                TreeDeterministicUtility.IsFinite(value.b)
                    ? Mathf.Clamp01(value.b)
                    : 0f,
                1f);
        }
    }

    public struct TreeRecipeRecenterResult
    {
        public TreeRecipeRecenterResult(
            bool passed,
            int processedControlCount,
            int changedControlCount,
            int constrainedCenterCount,
            string report)
        {
            Passed = passed;
            ProcessedControlCount = processedControlCount;
            ChangedControlCount = changedControlCount;
            ConstrainedCenterCount = constrainedCenterCount;
            Report = report ?? string.Empty;
        }

        public bool Passed { get; }
        public int ProcessedControlCount { get; }
        public int ChangedControlCount { get; }
        public int ConstrainedCenterCount { get; }
        public string Report { get; }
    }

    [Serializable]
    public sealed partial class TreeRecipeControlRanges
    {
        public const int CurrentSchemaVersion = 3;

        [SerializeField, HideInInspector]
        private int schemaVersion;

        [SerializeField] private TreeFloatControlRange height;
        [SerializeField] private TreeFloatControlRange trunkBaseRadius;
        [SerializeField] private TreeFloatControlRange trunkTaper;

        [SerializeField] private TreeFloatControlRange bendAmount;
        [SerializeField] private TreeFloatControlRange leanAmount;

        [SerializeField] private TreeFloatControlRange pathSpiralRadius;
        [SerializeField] private TreeFloatControlRange signedPathSpiralTurns;
        [SerializeField] private TreeFloatControlRange axialTwist;

        [SerializeField] private TreeIntControlRange rootCount;
        [SerializeField] private TreeFloatControlRange rootReach;
        [SerializeField] private TreeFloatControlRange rootThickness;
        [SerializeField] private TreeFloatControlRange rootHeight;
        [SerializeField] private TreeFloatControlRange buttressTransition = new TreeFloatControlRange(1f, 1f);

        [SerializeField] private TreeIntControlRange primaryBranchCount;
        [SerializeField] private TreeFloatControlRange branchStartHeight;
        [SerializeField] private TreeFloatControlRange branchEndHeight;
        [SerializeField] private TreeFloatControlRange branchSymmetry;

        [SerializeField] private TreeFloatControlRange branchLength;
        [SerializeField] private TreeFloatControlRange branchThickness;
        [SerializeField] private TreeFloatControlRange branchElevation;
        [SerializeField] private TreeFloatControlRange branchCurvature;

        [SerializeField] private TreeIntControlRange maximumBranchOrder;
        [SerializeField] private TreeFloatControlRange secondaryDensity;
        [SerializeField] private TreeFloatControlRange tertiaryDensity;
        [SerializeField] private TreeFloatControlRange childScale;

        [SerializeField] private TreeFloatControlRange missingBranchChance;
        [SerializeField] private TreeFloatControlRange deadBranchChance;
        [SerializeField] private TreeFloatControlRange brokenBranchChance;

        [SerializeField] private TreeColorControlRange barkTint;

        [SerializeField] private TreeFloatControlRange bendFrequency;
        [SerializeField] private TreeFloatControlRange trunkDrift;
        [SerializeField] private TreeFloatControlRange trunkRoughness;

        [SerializeField] private TreeFloatControlRange directionalBias;
        [SerializeField] private TreeAngleControlRange directionalBiasAngle;
        [SerializeField] private TreeFloatControlRange tierSpacing;

        [SerializeField] private TreeFloatControlRange branchArch;
        [SerializeField] private TreeFloatControlRange lateSag;
        [SerializeField] private TreeFloatControlRange tipUpturn;
        [SerializeField] private TreeFloatControlRange sideSweep;

        [SerializeField] private TreeFloatControlRange forkChance;

        public int SchemaVersion => schemaVersion;
        public bool IsInitialized => schemaVersion >= CurrentSchemaVersion;

        public TreeFloatControlRange Height => height;
        public TreeFloatControlRange TrunkBaseRadius => trunkBaseRadius;
        public TreeFloatControlRange TrunkTaper => trunkTaper;
        public TreeFloatControlRange BendAmount => bendAmount;
        public TreeFloatControlRange LeanAmount => leanAmount;
        public TreeFloatControlRange PathSpiralRadius => pathSpiralRadius;
        public TreeFloatControlRange SignedPathSpiralTurns => signedPathSpiralTurns;
        public TreeFloatControlRange AxialTwist => axialTwist;
        public TreeIntControlRange RootCount => rootCount;
        public TreeFloatControlRange RootReach => rootReach;
        public TreeFloatControlRange RootThickness => rootThickness;
        public TreeFloatControlRange RootHeight => rootHeight;
        public TreeFloatControlRange ButtressTransition => buttressTransition;
        public TreeIntControlRange PrimaryBranchCount => primaryBranchCount;
        public TreeFloatControlRange BranchStartHeight => branchStartHeight;
        public TreeFloatControlRange BranchEndHeight => branchEndHeight;
        public TreeFloatControlRange BranchSymmetry => branchSymmetry;
        public TreeFloatControlRange BranchLength => branchLength;
        public TreeFloatControlRange BranchThickness => branchThickness;
        public TreeFloatControlRange BranchElevation => branchElevation;
        public TreeFloatControlRange BranchCurvature => branchCurvature;
        public TreeIntControlRange MaximumBranchOrder => maximumBranchOrder;
        public TreeFloatControlRange SecondaryDensity => secondaryDensity;
        public TreeFloatControlRange TertiaryDensity => tertiaryDensity;
        public TreeFloatControlRange ChildScale => childScale;
        public TreeFloatControlRange MissingBranchChance => missingBranchChance;
        public TreeFloatControlRange DeadBranchChance => deadBranchChance;
        public TreeFloatControlRange BrokenBranchChance => brokenBranchChance;
        public TreeColorControlRange BarkTint => barkTint;
        public TreeFloatControlRange BendFrequency => bendFrequency;
        public TreeFloatControlRange TrunkDrift => trunkDrift;
        public TreeFloatControlRange TrunkRoughness => trunkRoughness;
        public TreeFloatControlRange DirectionalBias => directionalBias;
        public TreeAngleControlRange DirectionalBiasAngle => directionalBiasAngle;
        public TreeFloatControlRange TierSpacing => tierSpacing;
        public TreeFloatControlRange BranchArch => branchArch;
        public TreeFloatControlRange LateSag => lateSag;
        public TreeFloatControlRange TipUpturn => tipUpturn;
        public TreeFloatControlRange SideSweep => sideSweep;
        public TreeFloatControlRange ForkChance => forkChance;

        public static TreeRecipeControlRanges CreateStarterDefaults()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.ResetToStarterDefaults();
            return ranges;
        }

        public void EnsureCurrentDefaults()
        {
            if (schemaVersion <= 0)
            {
                ResetToStarterDefaults();
                return;
            }

            if (schemaVersion < 2)
            {
                buttressTransition = new TreeFloatControlRange(1f, 1f);
            }

            schemaVersion = Mathf.Max(schemaVersion, CurrentSchemaVersion);
            ValidateAndClamp();
        }

        public void ResetToStarterDefaults()
        {
            height = new TreeFloatControlRange(6f, 10f);
            trunkBaseRadius = new TreeFloatControlRange(0.25f, 0.50f);
            trunkTaper = new TreeFloatControlRange(0.75f, 0.90f);

            bendAmount = new TreeFloatControlRange(0.05f, 0.20f);
            leanAmount = new TreeFloatControlRange(0f, 0.08f);

            pathSpiralRadius = new TreeFloatControlRange(0f, 0.02f);
            signedPathSpiralTurns = new TreeFloatControlRange(-0.50f, 0.50f);
            axialTwist = new TreeFloatControlRange(-20f, 20f);

            rootCount = new TreeIntControlRange(4, 5);
            rootReach = new TreeFloatControlRange(0.25f, 0.50f);
            rootThickness = new TreeFloatControlRange(0.45f, 0.65f);
            rootHeight = new TreeFloatControlRange(0.10f, 0.20f);
            buttressTransition = new TreeFloatControlRange(1f, 1f);

            primaryBranchCount = new TreeIntControlRange(10, 18);
            branchStartHeight = new TreeFloatControlRange(0.22f, 0.32f);
            branchEndHeight = new TreeFloatControlRange(0.82f, 0.94f);
            branchSymmetry = new TreeFloatControlRange(0.45f, 0.80f);

            branchLength = new TreeFloatControlRange(0.25f, 0.42f);
            branchThickness = new TreeFloatControlRange(0.25f, 0.45f);
            branchElevation = new TreeFloatControlRange(-10f, 25f);
            branchCurvature = new TreeFloatControlRange(0.08f, 0.25f);

            maximumBranchOrder = new TreeIntControlRange(2, 3);
            secondaryDensity = new TreeFloatControlRange(1f, 2f);
            tertiaryDensity = new TreeFloatControlRange(0.50f, 1.50f);
            childScale = new TreeFloatControlRange(0.35f, 0.50f);

            missingBranchChance = new TreeFloatControlRange(0f, 0.04f);
            deadBranchChance = new TreeFloatControlRange(0f, 0.03f);
            brokenBranchChance = new TreeFloatControlRange(0f, 0.02f);

            barkTint = new TreeColorControlRange(Color.white, Color.white);

            bendFrequency = new TreeFloatControlRange(0.80f, 1.80f);
            trunkDrift = new TreeFloatControlRange(0f, 0.08f);
            trunkRoughness = new TreeFloatControlRange(0.01f, 0.08f);

            directionalBias = new TreeFloatControlRange(0f, 0.15f);
            directionalBiasAngle = new TreeAngleControlRange(0f, 360f);
            tierSpacing = new TreeFloatControlRange(0f, 0f);

            branchArch = new TreeFloatControlRange(-0.15f, 0.15f);
            lateSag = new TreeFloatControlRange(0.05f, 0.25f);
            tipUpturn = new TreeFloatControlRange(0f, 0.12f);
            sideSweep = new TreeFloatControlRange(-0.08f, 0.08f);

            forkChance = new TreeFloatControlRange(0f, 0.05f);

            schemaVersion = CurrentSchemaVersion;
            ValidateAndClamp();
        }

        public TreeRecipeRecenterResult RecenterFromResolvedControls(
            TreeResolvedControls source)
        {
            if (source == null || !source.IsInitialized)
            {
                return BuildRecenterFailure(
                    0,
                    "The source exact controls are null or uninitialized.");
            }

            if (schemaVersion <= 0)
            {
                return BuildRecenterFailure(
                    0,
                    "The target recipe ranges are uninitialized. " +
                    "Initialize or validate the recipe before recentering it.");
            }

            var baseline = (TreeRecipeControlRanges)MemberwiseClone();
            baseline.EnsureCurrentDefaults();
            var candidate =
                (TreeRecipeControlRanges)baseline.MemberwiseClone();
            var constrainedControls = new List<string>();
            int processed = 0;
            int changed = 0;
            if (!candidate.TryRecenterAll(
                    source,
                    constrainedControls,
                    ref processed,
                    ref changed,
                    out string failure))
            {
                return BuildRecenterFailure(processed, failure);
            }

            if (processed != TreeControlDescriptorRegistry.Controls.Count)
            {
                return BuildRecenterFailure(
                    processed,
                    "Recenter coverage mismatch. Processed " + processed +
                    " controls but the live registry exposes " +
                    TreeControlDescriptorRegistry.Controls.Count + ".");
            }

            candidate.schemaVersion = CurrentSchemaVersion;
            candidate.ValidateAndClamp();
            if (!candidate.HasPreservedSpans(baseline, out failure))
            {
                return BuildRecenterFailure(processed, failure);
            }

            candidate.CopyInto(this);
            string constrained = constrainedControls.Count == 0
                ? "none"
                : string.Join(", ", constrainedControls);
            string report =
                "Status: PASS\n" +
                "Controls processed: " + processed + "\n" +
                "Ranges changed: " + changed + "\n" +
                "Center-constrained controls: " +
                constrainedControls.Count + "\n" +
                "Constrained controls: " + constrained;
            return new TreeRecipeRecenterResult(
                true,
                processed,
                changed,
                constrainedControls.Count,
                report);
        }

        private bool TryRecenterAll(
            TreeResolvedControls source,
            List<string> constrainedControls,
            ref int processed,
            ref int changed,
            out string failure)
        {
            if (!TryRecenterFloat(ref height, source.Height, "height", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref trunkBaseRadius, source.TrunkBaseRadius, "trunkBaseRadius", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref trunkTaper, source.TrunkTaper, "trunkTaper", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref bendAmount, source.BendAmount, "bendAmount", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref leanAmount, source.LeanAmount, "leanAmount", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref pathSpiralRadius, source.PathSpiralRadius, "pathSpiralRadius", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref signedPathSpiralTurns, source.SignedPathSpiralTurns, "signedPathSpiralTurns", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref axialTwist, source.AxialTwist, "axialTwist", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterInt(ref rootCount, source.RootCount, "rootCount", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref rootReach, source.RootReach, "rootReach", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref rootThickness, source.RootThickness, "rootThickness", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref rootHeight, source.RootHeight, "rootHeight", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref buttressTransition, source.ButtressTransition, "buttressTransition", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterInt(ref primaryBranchCount, source.PrimaryBranchCount, "primaryBranchCount", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterOrderedFloatPair(
                    ref branchStartHeight,
                    source.BranchStartHeight,
                    "branchStartHeight",
                    ref branchEndHeight,
                    source.BranchEndHeight,
                    "branchEndHeight",
                    constrainedControls,
                    ref processed,
                    ref changed,
                    out failure) ||
                !TryRecenterFloat(ref branchSymmetry, source.BranchSymmetry, "branchSymmetry", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref branchLength, source.BranchLength, "branchLength", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref branchThickness, source.BranchThickness, "branchThickness", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref branchElevation, source.BranchElevation, "branchElevation", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref branchCurvature, source.BranchCurvature, "branchCurvature", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterInt(ref maximumBranchOrder, source.MaximumBranchOrder, "maximumBranchOrder", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref secondaryDensity, source.SecondaryDensity, "secondaryDensity", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref tertiaryDensity, source.TertiaryDensity, "tertiaryDensity", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref childScale, source.ChildScale, "childScale", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref missingBranchChance, source.MissingBranchChance, "missingBranchChance", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref deadBranchChance, source.DeadBranchChance, "deadBranchChance", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref brokenBranchChance, source.BrokenBranchChance, "brokenBranchChance", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterColor(ref barkTint, source.BarkTint, "barkTint", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref bendFrequency, source.BendFrequency, "bendFrequency", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref trunkDrift, source.TrunkDrift, "trunkDrift", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref trunkRoughness, source.TrunkRoughness, "trunkRoughness", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref directionalBias, source.DirectionalBias, "directionalBias", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterAngle(ref directionalBiasAngle, source.DirectionalBiasAngle, "directionalBiasAngle", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref tierSpacing, source.TierSpacing, "tierSpacing", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref branchArch, source.BranchArch, "branchArch", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref lateSag, source.LateSag, "lateSag", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref tipUpturn, source.TipUpturn, "tipUpturn", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref sideSweep, source.SideSweep, "sideSweep", constrainedControls, ref processed, ref changed, out failure) ||
                !TryRecenterFloat(ref forkChance, source.ForkChance, "forkChance", constrainedControls, ref processed, ref changed, out failure))
            {
                return false;
            }

            failure = string.Empty;
            return true;
        }

        private static bool TryRecenterFloat(
            ref TreeFloatControlRange range,
            float target,
            string propertyName,
            List<string> constrainedControls,
            ref int processed,
            ref int changed,
            out string failure)
        {
            processed++;
            if (!TryGetDescriptor(
                    propertyName,
                    TreeControlValueKind.Float,
                    out TreeControlDescriptor descriptor,
                    out failure))
            {
                return false;
            }

            if (!TreeDeterministicUtility.IsFinite(target))
            {
                failure = descriptor.Label + " source value is non-finite.";
                return false;
            }

            float width = range.Maximum - range.Minimum;
            float domainWidth = descriptor.HardMaximum - descriptor.HardMinimum;
            if (!TreeDeterministicUtility.IsFinite(width) ||
                width < -0.00001f ||
                width > domainWidth + 0.00001f)
            {
                failure = descriptor.Label +
                    " has an invalid existing recipe width.";
                return false;
            }

            width = Mathf.Max(0f, width);
            float halfWidth = width * 0.5f;
            float minimumCenter = descriptor.HardMinimum + halfWidth;
            float maximumCenter = descriptor.HardMaximum - halfWidth;
            float center = minimumCenter > maximumCenter
                ? (descriptor.HardMinimum + descriptor.HardMaximum) * 0.5f
                : Mathf.Clamp(target, minimumCenter, maximumCenter);
            var next = new TreeFloatControlRange(
                center - halfWidth,
                center + halfWidth);
            if (!SameFloatRange(range, next))
            {
                changed++;
            }
            if (!Mathf.Approximately(center, target))
            {
                constrainedControls.Add(descriptor.Label);
            }

            range = next;
            failure = string.Empty;
            return true;
        }

        private static bool TryRecenterInt(
            ref TreeIntControlRange range,
            int target,
            string propertyName,
            List<string> constrainedControls,
            ref int processed,
            ref int changed,
            out string failure)
        {
            processed++;
            if (!TryGetDescriptor(
                    propertyName,
                    TreeControlValueKind.Integer,
                    out TreeControlDescriptor descriptor,
                    out failure))
            {
                return false;
            }

            int hardMinimum = Mathf.RoundToInt(descriptor.HardMinimum);
            int hardMaximum = Mathf.RoundToInt(descriptor.HardMaximum);
            int span = range.Maximum - range.Minimum;
            if (span < 0 || span > hardMaximum - hardMinimum)
            {
                failure = descriptor.Label +
                    " has an invalid existing integer span.";
                return false;
            }

            int desiredMinimum = Mathf.RoundToInt(target - span * 0.5f);
            int minimum = Mathf.Clamp(
                desiredMinimum,
                hardMinimum,
                hardMaximum - span);
            var next = new TreeIntControlRange(minimum, minimum + span);
            if (!SameIntRange(range, next))
            {
                changed++;
            }

            float center = (next.Minimum + next.Maximum) * 0.5f;
            if (!Mathf.Approximately(center, target))
            {
                constrainedControls.Add(descriptor.Label);
            }

            range = next;
            failure = string.Empty;
            return true;
        }

        private static bool TryRecenterAngle(
            ref TreeAngleControlRange range,
            float target,
            string propertyName,
            List<string> constrainedControls,
            ref int processed,
            ref int changed,
            out string failure)
        {
            processed++;
            if (!TryGetDescriptor(
                    propertyName,
                    TreeControlValueKind.Angle,
                    out TreeControlDescriptor descriptor,
                    out failure))
            {
                return false;
            }

            if (!TreeDeterministicUtility.IsFinite(target))
            {
                failure = descriptor.Label + " source value is non-finite.";
                return false;
            }

            float span = ResolveAngleSpan(range);
            if (span >= 359.9999f)
            {
                failure = string.Empty;
                return true;
            }

            float center = NormalizeCircularAngle(target);
            float halfSpan = span * 0.5f;
            var next = new TreeAngleControlRange(
                NormalizeCircularAngle(center - halfSpan),
                NormalizeCircularAngle(center + halfSpan));
            if (!SameAngleRange(range, next))
            {
                changed++;
            }

            range = next;
            failure = string.Empty;
            return true;
        }

        private static bool TryRecenterColor(
            ref TreeColorControlRange range,
            Color target,
            string propertyName,
            List<string> constrainedControls,
            ref int processed,
            ref int changed,
            out string failure)
        {
            processed++;
            if (!TryGetDescriptor(
                    propertyName,
                    TreeControlValueKind.Color,
                    out TreeControlDescriptor descriptor,
                    out failure))
            {
                return false;
            }

            if (!TreeDeterministicUtility.IsFinite(target.r) ||
                !TreeDeterministicUtility.IsFinite(target.g) ||
                !TreeDeterministicUtility.IsFinite(target.b))
            {
                failure = descriptor.Label + " source color is non-finite.";
                return false;
            }

            bool constrained = false;
            if (!TryRecenterColorChannel(
                    range.Minimum.r,
                    range.Maximum.r,
                    target.r,
                    out float minimumR,
                    out float maximumR,
                    ref constrained) ||
                !TryRecenterColorChannel(
                    range.Minimum.g,
                    range.Maximum.g,
                    target.g,
                    out float minimumG,
                    out float maximumG,
                    ref constrained) ||
                !TryRecenterColorChannel(
                    range.Minimum.b,
                    range.Maximum.b,
                    target.b,
                    out float minimumB,
                    out float maximumB,
                    ref constrained))
            {
                failure = descriptor.Label +
                    " has an invalid existing channel span.";
                return false;
            }

            var next = new TreeColorControlRange(
                new Color(minimumR, minimumG, minimumB, 1f),
                new Color(maximumR, maximumG, maximumB, 1f));
            if (!SameColorRange(range, next))
            {
                changed++;
            }
            if (constrained)
            {
                constrainedControls.Add(descriptor.Label);
            }

            range = next;
            failure = string.Empty;
            return true;
        }

        private static bool TryRecenterOrderedFloatPair(
            ref TreeFloatControlRange firstRange,
            float firstTarget,
            string firstPropertyName,
            ref TreeFloatControlRange secondRange,
            float secondTarget,
            string secondPropertyName,
            List<string> constrainedControls,
            ref int processed,
            ref int changed,
            out string failure)
        {
            processed += 2;
            if (!TryGetDescriptor(
                    firstPropertyName,
                    TreeControlValueKind.Float,
                    out TreeControlDescriptor firstDescriptor,
                    out failure) ||
                !TryGetDescriptor(
                    secondPropertyName,
                    TreeControlValueKind.Float,
                    out TreeControlDescriptor secondDescriptor,
                    out failure))
            {
                return false;
            }

            if (!TreeDeterministicUtility.IsFinite(firstTarget) ||
                !TreeDeterministicUtility.IsFinite(secondTarget))
            {
                failure = "Ordered range source values are non-finite.";
                return false;
            }

            if (!Mathf.Approximately(
                    firstDescriptor.HardMinimum,
                    secondDescriptor.HardMinimum) ||
                !Mathf.Approximately(
                    firstDescriptor.HardMaximum,
                    secondDescriptor.HardMaximum))
            {
                failure =
                    "Ordered range hard domains do not match.";
                return false;
            }

            float hardMinimum = firstDescriptor.HardMinimum;
            float hardMaximum = firstDescriptor.HardMaximum;
            float domainWidth = hardMaximum - hardMinimum;
            float firstWidth = firstRange.Maximum - firstRange.Minimum;
            float secondWidth = secondRange.Maximum - secondRange.Minimum;
            if (firstWidth < -0.00001f ||
                secondWidth < -0.00001f ||
                firstWidth + secondWidth > domainWidth + 0.00001f)
            {
                failure =
                    "Branch Start Height and Branch End Height cannot preserve " +
                    "their existing widths inside the ordered hard domain.";
                return false;
            }

            firstWidth = Mathf.Max(0f, firstWidth);
            secondWidth = Mathf.Max(0f, secondWidth);
            float firstMinimum = Mathf.Clamp(
                firstTarget - firstWidth * 0.5f,
                hardMinimum,
                hardMaximum - firstWidth);
            float secondMinimum = Mathf.Clamp(
                secondTarget - secondWidth * 0.5f,
                hardMinimum,
                hardMaximum - secondWidth);

            if (secondMinimum < firstMinimum + firstWidth)
            {
                float desiredFirstMinimum =
                    firstTarget - firstWidth * 0.5f;
                float desiredSecondMinimum =
                    secondTarget - secondWidth * 0.5f;
                float touchingFirstMinimum =
                    (desiredFirstMinimum +
                     desiredSecondMinimum -
                     firstWidth) * 0.5f;
                float maximumTouchingFirstMinimum =
                    hardMaximum - firstWidth - secondWidth;
                firstMinimum = Mathf.Clamp(
                    touchingFirstMinimum,
                    hardMinimum,
                    maximumTouchingFirstMinimum);
                secondMinimum = firstMinimum + firstWidth;
            }

            var nextFirst = new TreeFloatControlRange(
                firstMinimum,
                firstMinimum + firstWidth);
            var nextSecond = new TreeFloatControlRange(
                secondMinimum,
                secondMinimum + secondWidth);
            if (!SameFloatRange(firstRange, nextFirst))
            {
                changed++;
            }
            if (!SameFloatRange(secondRange, nextSecond))
            {
                changed++;
            }

            float firstCenter =
                (nextFirst.Minimum + nextFirst.Maximum) * 0.5f;
            float secondCenter =
                (nextSecond.Minimum + nextSecond.Maximum) * 0.5f;
            if (!Mathf.Approximately(firstCenter, firstTarget))
            {
                constrainedControls.Add(firstDescriptor.Label);
            }
            if (!Mathf.Approximately(secondCenter, secondTarget))
            {
                constrainedControls.Add(secondDescriptor.Label);
            }

            firstRange = nextFirst;
            secondRange = nextSecond;
            failure = string.Empty;
            return true;
        }

        private static bool TryRecenterColorChannel(
            float existingMinimum,
            float existingMaximum,
            float target,
            out float minimum,
            out float maximum,
            ref bool constrained)
        {
            float signedSpan = existingMaximum - existingMinimum;
            float span = Mathf.Abs(signedSpan);
            if (!TreeDeterministicUtility.IsFinite(span) ||
                span > 1f + 0.00001f)
            {
                minimum = 0f;
                maximum = 0f;
                return false;
            }

            span = Mathf.Clamp01(span);
            float halfSpan = span * 0.5f;
            float center = Mathf.Clamp(target, halfSpan, 1f - halfSpan);
            constrained |= !Mathf.Approximately(center, target);
            float low = center - halfSpan;
            float high = center + halfSpan;
            if (signedSpan < 0f)
            {
                minimum = high;
                maximum = low;
            }
            else
            {
                minimum = low;
                maximum = high;
            }
            return true;
        }

        private static bool TryGetDescriptor(
            string propertyName,
            TreeControlValueKind expectedKind,
            out TreeControlDescriptor descriptor,
            out string failure)
        {
            IReadOnlyList<TreeControlDescriptor> controls =
                TreeControlDescriptorRegistry.Controls;
            for (int index = 0; index < controls.Count; index++)
            {
                TreeControlDescriptor candidate = controls[index];
                if (!string.Equals(
                        candidate.PropertyName,
                        propertyName,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (candidate.Kind != expectedKind)
                {
                    descriptor = null;
                    failure =
                        "Control kind mismatch for " + propertyName + ".";
                    return false;
                }

                descriptor = candidate;
                failure = string.Empty;
                return true;
            }

            descriptor = null;
            failure = "No live control descriptor exists for " +
                propertyName + ".";
            return false;
        }

        private bool HasPreservedSpans(
            TreeRecipeControlRanges original,
            out string failure)
        {
            if (original == null)
            {
                failure = "Original recipe ranges are null.";
                return false;
            }

            bool spansPreserved =
                SameFloatSpan(height, original.height) &&
                SameFloatSpan(trunkBaseRadius, original.trunkBaseRadius) &&
                SameFloatSpan(trunkTaper, original.trunkTaper) &&
                SameFloatSpan(bendAmount, original.bendAmount) &&
                SameFloatSpan(leanAmount, original.leanAmount) &&
                SameFloatSpan(pathSpiralRadius, original.pathSpiralRadius) &&
                SameFloatSpan(signedPathSpiralTurns, original.signedPathSpiralTurns) &&
                SameFloatSpan(axialTwist, original.axialTwist) &&
                SameIntSpan(rootCount, original.rootCount) &&
                SameFloatSpan(rootReach, original.rootReach) &&
                SameFloatSpan(rootThickness, original.rootThickness) &&
                SameFloatSpan(rootHeight, original.rootHeight) &&
                SameFloatSpan(buttressTransition, original.buttressTransition) &&
                SameIntSpan(primaryBranchCount, original.primaryBranchCount) &&
                SameFloatSpan(branchStartHeight, original.branchStartHeight) &&
                SameFloatSpan(branchEndHeight, original.branchEndHeight) &&
                SameFloatSpan(branchSymmetry, original.branchSymmetry) &&
                SameFloatSpan(branchLength, original.branchLength) &&
                SameFloatSpan(branchThickness, original.branchThickness) &&
                SameFloatSpan(branchElevation, original.branchElevation) &&
                SameFloatSpan(branchCurvature, original.branchCurvature) &&
                SameIntSpan(maximumBranchOrder, original.maximumBranchOrder) &&
                SameFloatSpan(secondaryDensity, original.secondaryDensity) &&
                SameFloatSpan(tertiaryDensity, original.tertiaryDensity) &&
                SameFloatSpan(childScale, original.childScale) &&
                SameFloatSpan(missingBranchChance, original.missingBranchChance) &&
                SameFloatSpan(deadBranchChance, original.deadBranchChance) &&
                SameFloatSpan(brokenBranchChance, original.brokenBranchChance) &&
                SameColorSpan(barkTint, original.barkTint) &&
                SameFloatSpan(bendFrequency, original.bendFrequency) &&
                SameFloatSpan(trunkDrift, original.trunkDrift) &&
                SameFloatSpan(trunkRoughness, original.trunkRoughness) &&
                SameFloatSpan(directionalBias, original.directionalBias) &&
                SameAngleSpan(directionalBiasAngle, original.directionalBiasAngle) &&
                SameFloatSpan(tierSpacing, original.tierSpacing) &&
                SameFloatSpan(branchArch, original.branchArch) &&
                SameFloatSpan(lateSag, original.lateSag) &&
                SameFloatSpan(tipUpturn, original.tipUpturn) &&
                SameFloatSpan(sideSweep, original.sideSweep) &&
                SameFloatSpan(forkChance, original.forkChance);
            if (!spansPreserved)
            {
                failure =
                    "Range validation changed one or more authored spans. " +
                    "The recenter transaction was not committed.";
                return false;
            }

            if (branchEndHeight.Minimum + 0.00001f <
                branchStartHeight.Maximum)
            {
                failure =
                    "Branch End Height no longer guarantees values at or above " +
                    "every Branch Start Height sample.";
                return false;
            }

            failure = string.Empty;
            return true;
        }

        private void CopyInto(TreeRecipeControlRanges destination)
        {
            destination.schemaVersion = schemaVersion;
            destination.height = height;
            destination.trunkBaseRadius = trunkBaseRadius;
            destination.trunkTaper = trunkTaper;
            destination.bendAmount = bendAmount;
            destination.leanAmount = leanAmount;
            destination.pathSpiralRadius = pathSpiralRadius;
            destination.signedPathSpiralTurns = signedPathSpiralTurns;
            destination.axialTwist = axialTwist;
            destination.rootCount = rootCount;
            destination.rootReach = rootReach;
            destination.rootThickness = rootThickness;
            destination.rootHeight = rootHeight;
            destination.buttressTransition = buttressTransition;
            destination.primaryBranchCount = primaryBranchCount;
            destination.branchStartHeight = branchStartHeight;
            destination.branchEndHeight = branchEndHeight;
            destination.branchSymmetry = branchSymmetry;
            destination.branchLength = branchLength;
            destination.branchThickness = branchThickness;
            destination.branchElevation = branchElevation;
            destination.branchCurvature = branchCurvature;
            destination.maximumBranchOrder = maximumBranchOrder;
            destination.secondaryDensity = secondaryDensity;
            destination.tertiaryDensity = tertiaryDensity;
            destination.childScale = childScale;
            destination.missingBranchChance = missingBranchChance;
            destination.deadBranchChance = deadBranchChance;
            destination.brokenBranchChance = brokenBranchChance;
            destination.barkTint = barkTint;
            destination.bendFrequency = bendFrequency;
            destination.trunkDrift = trunkDrift;
            destination.trunkRoughness = trunkRoughness;
            destination.directionalBias = directionalBias;
            destination.directionalBiasAngle = directionalBiasAngle;
            destination.tierSpacing = tierSpacing;
            destination.branchArch = branchArch;
            destination.lateSag = lateSag;
            destination.tipUpturn = tipUpturn;
            destination.sideSweep = sideSweep;
            destination.forkChance = forkChance;
        }

        private static TreeRecipeRecenterResult BuildRecenterFailure(
            int processed,
            string failure)
        {
            return new TreeRecipeRecenterResult(
                false,
                processed,
                0,
                0,
                "Status: FAIL\n" + (failure ?? "Unknown recenter failure."));
        }

        private static bool SameFloatRange(
            TreeFloatControlRange first,
            TreeFloatControlRange second)
        {
            return Mathf.Approximately(first.Minimum, second.Minimum) &&
                Mathf.Approximately(first.Maximum, second.Maximum);
        }

        private static bool SameIntRange(
            TreeIntControlRange first,
            TreeIntControlRange second)
        {
            return first.Minimum == second.Minimum &&
                first.Maximum == second.Maximum;
        }

        private static bool SameAngleRange(
            TreeAngleControlRange first,
            TreeAngleControlRange second)
        {
            return Mathf.Approximately(first.Minimum, second.Minimum) &&
                Mathf.Approximately(first.Maximum, second.Maximum);
        }

        private static bool SameColorRange(
            TreeColorControlRange first,
            TreeColorControlRange second)
        {
            return Approximately(first.Minimum, second.Minimum) &&
                Approximately(first.Maximum, second.Maximum);
        }

        private static bool SameFloatSpan(
            TreeFloatControlRange first,
            TreeFloatControlRange second)
        {
            return Mathf.Abs(
                (first.Maximum - first.Minimum) -
                (second.Maximum - second.Minimum)) <= 0.00001f;
        }

        private static bool SameIntSpan(
            TreeIntControlRange first,
            TreeIntControlRange second)
        {
            return first.Maximum - first.Minimum ==
                second.Maximum - second.Minimum;
        }

        private static bool SameAngleSpan(
            TreeAngleControlRange first,
            TreeAngleControlRange second)
        {
            return Mathf.Abs(
                ResolveAngleSpan(first) - ResolveAngleSpan(second)) <=
                0.0001f;
        }

        private static bool SameColorSpan(
            TreeColorControlRange first,
            TreeColorControlRange second)
        {
            Color firstSpan = first.Maximum - first.Minimum;
            Color secondSpan = second.Maximum - second.Minimum;
            return Mathf.Abs(firstSpan.r - secondSpan.r) <= 0.00001f &&
                Mathf.Abs(firstSpan.g - secondSpan.g) <= 0.00001f &&
                Mathf.Abs(firstSpan.b - secondSpan.b) <= 0.00001f;
        }

        private static float ResolveAngleSpan(TreeAngleControlRange range)
        {
            if (!range.WrapsThroughZero)
            {
                return Mathf.Clamp(range.Maximum - range.Minimum, 0f, 360f);
            }

            return Mathf.Clamp(
                (360f - range.Minimum) + range.Maximum,
                0f,
                360f);
        }

        private static float NormalizeCircularAngle(float value)
        {
            value %= 360f;
            return value < 0f ? value + 360f : value;
        }

        public void ValidateAndClamp()
        {
            height.ClampAndOrder(1f, 40f);
            trunkBaseRadius.ClampAndOrder(0.02f, 4f);
            trunkTaper.ClampAndOrder(0f, 1f);

            bendAmount.ClampAndOrder(0f, 1f);
            leanAmount.ClampAndOrder(0f, 0.60f);

            pathSpiralRadius.ClampAndOrder(0f, 0.50f);
            signedPathSpiralTurns.ClampAndOrder(-3f, 3f);
            axialTwist.ClampAndOrder(-1080f, 1080f);

            rootCount.ClampAndOrder(3, 8);
            rootReach.ClampAndOrder(0f, 2f);
            rootThickness.ClampAndOrder(0.10f, 2f);
            rootHeight.ClampAndOrder(0.01f, 0.40f);
            buttressTransition.ClampAndOrder(0f, 1f);

            primaryBranchCount.ClampAndOrder(0, 64);
            branchStartHeight.ClampAndOrder(0f, 1f);
            branchEndHeight.ClampAndOrder(0f, 1f);
            // Guarantee every independently sampled pair is ordered. The old
            // minimum/minimum correction could still sample Start above End.
            branchEndHeight = new TreeFloatControlRange(
                Mathf.Max(
                    branchEndHeight.Minimum,
                    branchStartHeight.Maximum),
                Mathf.Max(
                    branchEndHeight.Maximum,
                    branchStartHeight.Maximum));
            branchSymmetry.ClampAndOrder(0f, 1f);

            branchLength.ClampAndOrder(0.05f, 1f);
            branchThickness.ClampAndOrder(0.05f, 1f);
            branchElevation.ClampAndOrder(-90f, 90f);
            branchCurvature.ClampAndOrder(0f, 1f);

            maximumBranchOrder.ClampAndOrder(1, 3);
            secondaryDensity.ClampAndOrder(0f, 8f);
            tertiaryDensity.ClampAndOrder(0f, 8f);
            childScale.ClampAndOrder(0.05f, 0.90f);

            missingBranchChance.ClampAndOrder(0f, 1f);
            deadBranchChance.ClampAndOrder(0f, 1f);
            brokenBranchChance.ClampAndOrder(0f, 1f);
            barkTint.Clamp01();

            bendFrequency.ClampAndOrder(0f, 6f);
            trunkDrift.ClampAndOrder(0f, 0.50f);
            trunkRoughness.ClampAndOrder(0f, 0.50f);

            directionalBias.ClampAndOrder(0f, 1f);
            directionalBiasAngle.NormalizeValues();
            tierSpacing.ClampAndOrder(0f, 0.50f);

            branchArch.ClampAndOrder(-1f, 1f);
            lateSag.ClampAndOrder(0f, 1f);
            tipUpturn.ClampAndOrder(0f, 1f);
            sideSweep.ClampAndOrder(-1f, 1f);
            forkChance.ClampAndOrder(0f, 1f);

            schemaVersion = CurrentSchemaVersion;
        }
    }
}
