using System;
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

    [Serializable]
    public sealed partial class TreeRecipeControlRanges
    {
        public const int CurrentSchemaVersion = 2;

        [SerializeField, HideInInspector]
        private int schemaVersion;

        [SerializeField] private TreeFloatControlRange height;
        [SerializeField] private TreeFloatControlRange trunkBaseRadius;
        [SerializeField] private TreeFloatControlRange trunkTaper;

        [SerializeField] private TreeFloatControlRange bendAmount;
        [SerializeField] private TreeFloatControlRange leanAmount;
        [SerializeField] private TreeAngleControlRange leanDirection;

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
        public TreeAngleControlRange LeanDirection => leanDirection;
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
            leanDirection = new TreeAngleControlRange(0f, 360f);

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

        public void ValidateAndClamp()
        {
            height.ClampAndOrder(1f, 40f);
            trunkBaseRadius.ClampAndOrder(0.02f, 4f);
            trunkTaper.ClampAndOrder(0f, 1f);

            bendAmount.ClampAndOrder(0f, 1f);
            leanAmount.ClampAndOrder(0f, 0.60f);
            leanDirection.NormalizeValues();

            pathSpiralRadius.ClampAndOrder(0f, 0.50f);
            signedPathSpiralTurns.ClampAndOrder(-3f, 3f);
            axialTwist.ClampAndOrder(-1080f, 1080f);

            rootCount.ClampAndOrder(3, 8);
            rootReach.ClampAndOrder(0f, 2f);
            rootThickness.ClampAndOrder(0.10f, 1f);
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
