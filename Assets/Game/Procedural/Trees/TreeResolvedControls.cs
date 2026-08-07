using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public sealed class TreeResolvedControls
    {
        public const int CurrentSchemaVersion = 2;

        [SerializeField, HideInInspector]
        private int schemaVersion;

        [SerializeField] private float height;
        [SerializeField] private float trunkBaseRadius;
        [SerializeField] private float trunkTaper;

        [SerializeField] private float bendAmount;
        [SerializeField] private float leanAmount;
        [SerializeField] private float leanDirection;

        [SerializeField] private float pathSpiralRadius;
        [SerializeField] private float signedPathSpiralTurns;
        [SerializeField] private float axialTwist;

        [SerializeField] private int rootCount;
        [SerializeField] private float rootReach;
        [SerializeField] private float rootThickness;
        [SerializeField] private float rootHeight;
        [SerializeField] private float buttressTransition = 1f;

        [SerializeField] private int primaryBranchCount;
        [SerializeField] private float branchStartHeight;
        [SerializeField] private float branchEndHeight;
        [SerializeField] private float branchSymmetry;

        [SerializeField] private float branchLength;
        [SerializeField] private float branchThickness;
        [SerializeField] private float branchElevation;
        [SerializeField] private float branchCurvature;

        [SerializeField] private int maximumBranchOrder;
        [SerializeField] private float secondaryDensity;
        [SerializeField] private float tertiaryDensity;
        [SerializeField] private float childScale;

        [SerializeField] private float missingBranchChance;
        [SerializeField] private float deadBranchChance;
        [SerializeField] private float brokenBranchChance;

        [SerializeField] private Color barkTint = Color.white;

        [SerializeField] private float bendFrequency;
        [SerializeField] private float trunkDrift;
        [SerializeField] private float trunkRoughness;

        [SerializeField] private float directionalBias;
        [SerializeField] private float directionalBiasAngle;
        [SerializeField] private float tierSpacing;

        [SerializeField] private float branchArch;
        [SerializeField] private float lateSag;
        [SerializeField] private float tipUpturn;
        [SerializeField] private float sideSweep;

        [SerializeField] private float forkChance;

        public int SchemaVersion => schemaVersion;
        public bool IsInitialized => schemaVersion >= CurrentSchemaVersion;
        public float Height => height;
        public float TrunkBaseRadius => trunkBaseRadius;
        public float TrunkTaper => trunkTaper;
        public float BendAmount => bendAmount;
        public float LeanAmount => leanAmount;
        public float LeanDirection => leanDirection;
        public float PathSpiralRadius => pathSpiralRadius;
        public float SignedPathSpiralTurns => signedPathSpiralTurns;
        public float AxialTwist => axialTwist;
        public int RootCount => rootCount;
        public float RootReach => rootReach;
        public float RootThickness => rootThickness;
        public float RootHeight => rootHeight;
        public float ButtressTransition => buttressTransition;
        public int PrimaryBranchCount => primaryBranchCount;
        public float BranchStartHeight => branchStartHeight;
        public float BranchEndHeight => branchEndHeight;
        public float BranchSymmetry => branchSymmetry;
        public float BranchLength => branchLength;
        public float BranchThickness => branchThickness;
        public float BranchElevation => branchElevation;
        public float BranchCurvature => branchCurvature;
        public int MaximumBranchOrder => maximumBranchOrder;
        public float SecondaryDensity => secondaryDensity;
        public float TertiaryDensity => tertiaryDensity;
        public float ChildScale => childScale;
        public float MissingBranchChance => missingBranchChance;
        public float DeadBranchChance => deadBranchChance;
        public float BrokenBranchChance => brokenBranchChance;
        public Color BarkTint => barkTint;
        public float BendFrequency => bendFrequency;
        public float TrunkDrift => trunkDrift;
        public float TrunkRoughness => trunkRoughness;
        public float DirectionalBias => directionalBias;
        public float DirectionalBiasAngle => directionalBiasAngle;
        public float TierSpacing => tierSpacing;
        public float BranchArch => branchArch;
        public float LateSag => lateSag;
        public float TipUpturn => tipUpturn;
        public float SideSweep => sideSweep;
        public float ForkChance => forkChance;

        public void ResolveFrom(TreeRecipeControlRanges ranges, int masterSeed)
        {
            if (ranges == null)
            {
                ranges = TreeRecipeControlRanges.CreateStarterDefaults();
            }

            ranges.EnsureCurrentDefaults();
            height = ranges.Height.Sample(masterSeed, "tree.height");
            trunkBaseRadius = ranges.TrunkBaseRadius.Sample(
                masterSeed,
                "tree.trunk.base-radius");
            trunkTaper = ranges.TrunkTaper.Sample(
                masterSeed,
                "tree.trunk.taper");

            bendAmount = ranges.BendAmount.Sample(
                masterSeed,
                "tree.trunk.bend-amount");
            leanAmount = ranges.LeanAmount.Sample(
                masterSeed,
                "tree.trunk.lean-amount");
            leanDirection = ranges.LeanDirection.Sample(
                masterSeed,
                "tree.trunk.lean-direction");

            pathSpiralRadius = ranges.PathSpiralRadius.Sample(
                masterSeed,
                "tree.trunk.path-spiral-radius");
            signedPathSpiralTurns = ranges.SignedPathSpiralTurns.Sample(
                masterSeed,
                "tree.trunk.path-spiral-turns");
            axialTwist = ranges.AxialTwist.Sample(
                masterSeed,
                "tree.trunk.axial-twist");

            rootCount = ranges.RootCount.Sample(
                masterSeed,
                "tree.root.count");
            rootReach = ranges.RootReach.Sample(
                masterSeed,
                "tree.root.reach");
            rootThickness = ranges.RootThickness.Sample(
                masterSeed,
                "tree.root.thickness");
            rootHeight = ranges.RootHeight.Sample(
                masterSeed,
                "tree.root.height");
            buttressTransition = ranges.ButtressTransition.Sample(
                masterSeed,
                "tree.root.buttress-transition");

            primaryBranchCount = ranges.PrimaryBranchCount.Sample(
                masterSeed,
                "tree.branch.primary.count");
            branchStartHeight = ranges.BranchStartHeight.Sample(
                masterSeed,
                "tree.branch.primary.start-height");
            branchEndHeight = ranges.BranchEndHeight.Sample(
                masterSeed,
                "tree.branch.primary.end-height");
            branchSymmetry = ranges.BranchSymmetry.Sample(
                masterSeed,
                "tree.branch.primary.symmetry");

            branchLength = ranges.BranchLength.Sample(
                masterSeed,
                "tree.branch.primary.length");
            branchThickness = ranges.BranchThickness.Sample(
                masterSeed,
                "tree.branch.primary.thickness");
            branchElevation = ranges.BranchElevation.Sample(
                masterSeed,
                "tree.branch.primary.elevation");
            branchCurvature = ranges.BranchCurvature.Sample(
                masterSeed,
                "tree.branch.primary.curvature");

            maximumBranchOrder = ranges.MaximumBranchOrder.Sample(
                masterSeed,
                "tree.branch.hierarchy.maximum-order");
            secondaryDensity = ranges.SecondaryDensity.Sample(
                masterSeed,
                "tree.branch.hierarchy.secondary-density");
            tertiaryDensity = ranges.TertiaryDensity.Sample(
                masterSeed,
                "tree.branch.hierarchy.tertiary-density");
            childScale = ranges.ChildScale.Sample(
                masterSeed,
                "tree.branch.hierarchy.child-scale");

            missingBranchChance = ranges.MissingBranchChance.Sample(
                masterSeed,
                "tree.damage.missing-chance");
            deadBranchChance = ranges.DeadBranchChance.Sample(
                masterSeed,
                "tree.damage.dead-chance");
            brokenBranchChance = ranges.BrokenBranchChance.Sample(
                masterSeed,
                "tree.damage.broken-chance");

            barkTint = ranges.BarkTint.Sample(
                masterSeed,
                "tree.appearance.bark-tint");

            bendFrequency = ranges.BendFrequency.Sample(
                masterSeed,
                "tree.advanced.trunk.bend-frequency");
            trunkDrift = ranges.TrunkDrift.Sample(
                masterSeed,
                "tree.advanced.trunk.drift");
            trunkRoughness = ranges.TrunkRoughness.Sample(
                masterSeed,
                "tree.advanced.trunk.roughness");
            directionalBias = ranges.DirectionalBias.Sample(
                masterSeed,
                "tree.advanced.branch.directional-bias");
            directionalBiasAngle = ranges.DirectionalBiasAngle.Sample(
                masterSeed,
                "tree.advanced.branch.directional-bias-angle");
            tierSpacing = ranges.TierSpacing.Sample(
                masterSeed,
                "tree.advanced.branch.tier-spacing");

            branchArch = ranges.BranchArch.Sample(
                masterSeed,
                "tree.advanced.branch.arch");
            lateSag = ranges.LateSag.Sample(
                masterSeed,
                "tree.advanced.branch.late-sag");
            tipUpturn = ranges.TipUpturn.Sample(
                masterSeed,
                "tree.advanced.branch.tip-upturn");
            sideSweep = ranges.SideSweep.Sample(
                masterSeed,
                "tree.advanced.branch.side-sweep");

            forkChance = ranges.ForkChance.Sample(
                masterSeed,
                "tree.advanced.fork.chance");

            schemaVersion = CurrentSchemaVersion;
            ValidateAndClamp();
        }

        public void EnsureInitialized(TreeRecipeControlRanges ranges, int seed)
        {
            if (!IsInitialized)
            {
                ResolveFrom(ranges, seed);
            }
            else
            {
                ValidateAndClamp();
            }
        }

        public string CalculateFingerprint()
        {
            ValidateAndClamp();
            ulong hash = TreeDeterministicUtility.BeginHash();
            TreeDeterministicUtility.Append(ref hash, schemaVersion);
            TreeDeterministicUtility.Append(ref hash, height);
            TreeDeterministicUtility.Append(ref hash, trunkBaseRadius);
            TreeDeterministicUtility.Append(ref hash, trunkTaper);
            TreeDeterministicUtility.Append(ref hash, bendAmount);
            TreeDeterministicUtility.Append(ref hash, leanAmount);
            TreeDeterministicUtility.Append(ref hash, leanDirection);
            TreeDeterministicUtility.Append(ref hash, pathSpiralRadius);
            TreeDeterministicUtility.Append(ref hash, signedPathSpiralTurns);
            TreeDeterministicUtility.Append(ref hash, axialTwist);
            TreeDeterministicUtility.Append(ref hash, rootCount);
            TreeDeterministicUtility.Append(ref hash, rootReach);
            TreeDeterministicUtility.Append(ref hash, rootThickness);
            TreeDeterministicUtility.Append(ref hash, rootHeight);
            TreeDeterministicUtility.Append(ref hash, buttressTransition);
            TreeDeterministicUtility.Append(ref hash, primaryBranchCount);
            TreeDeterministicUtility.Append(ref hash, branchStartHeight);
            TreeDeterministicUtility.Append(ref hash, branchEndHeight);
            TreeDeterministicUtility.Append(ref hash, branchSymmetry);
            TreeDeterministicUtility.Append(ref hash, branchLength);
            TreeDeterministicUtility.Append(ref hash, branchThickness);
            TreeDeterministicUtility.Append(ref hash, branchElevation);
            TreeDeterministicUtility.Append(ref hash, branchCurvature);
            TreeDeterministicUtility.Append(ref hash, maximumBranchOrder);
            TreeDeterministicUtility.Append(ref hash, secondaryDensity);
            TreeDeterministicUtility.Append(ref hash, tertiaryDensity);
            TreeDeterministicUtility.Append(ref hash, childScale);
            TreeDeterministicUtility.Append(ref hash, missingBranchChance);
            TreeDeterministicUtility.Append(ref hash, deadBranchChance);
            TreeDeterministicUtility.Append(ref hash, brokenBranchChance);
            TreeDeterministicUtility.Append(ref hash, barkTint);
            TreeDeterministicUtility.Append(ref hash, bendFrequency);
            TreeDeterministicUtility.Append(ref hash, trunkDrift);
            TreeDeterministicUtility.Append(ref hash, trunkRoughness);
            TreeDeterministicUtility.Append(ref hash, directionalBias);
            TreeDeterministicUtility.Append(ref hash, directionalBiasAngle);
            TreeDeterministicUtility.Append(ref hash, tierSpacing);
            TreeDeterministicUtility.Append(ref hash, branchArch);
            TreeDeterministicUtility.Append(ref hash, lateSag);
            TreeDeterministicUtility.Append(ref hash, tipUpturn);
            TreeDeterministicUtility.Append(ref hash, sideSweep);
            TreeDeterministicUtility.Append(ref hash, forkChance);
            return TreeDeterministicUtility.FormatHash(hash);
        }

        public void ValidateAndClamp()
        {
            height = FiniteClamp(height, 1f, 40f);
            trunkBaseRadius = FiniteClamp(trunkBaseRadius, 0.02f, 4f);
            trunkTaper = FiniteClamp(trunkTaper, 0f, 1f);
            bendAmount = FiniteClamp(bendAmount, 0f, 1f);
            leanAmount = FiniteClamp(leanAmount, 0f, 0.60f);
            leanDirection = NormalizeAngle(leanDirection);
            pathSpiralRadius = FiniteClamp(pathSpiralRadius, 0f, 0.50f);
            signedPathSpiralTurns = FiniteClamp(
                signedPathSpiralTurns,
                -3f,
                3f);
            axialTwist = FiniteClamp(axialTwist, -1080f, 1080f);
            rootCount = Mathf.Clamp(rootCount, 3, 8);
            rootReach = FiniteClamp(rootReach, 0f, 2f);
            rootThickness = FiniteClamp(rootThickness, 0.10f, 2f);
            rootHeight = FiniteClamp(rootHeight, 0.01f, 0.40f);
            buttressTransition = FiniteClamp(buttressTransition, 0f, 1f);
            primaryBranchCount = Mathf.Clamp(primaryBranchCount, 0, 64);
            branchStartHeight = FiniteClamp(branchStartHeight, 0f, 1f);
            branchEndHeight = Mathf.Max(
                branchStartHeight,
                FiniteClamp(branchEndHeight, 0f, 1f));
            branchSymmetry = FiniteClamp(branchSymmetry, 0f, 1f);
            branchLength = FiniteClamp(branchLength, 0.05f, 1f);
            branchThickness = FiniteClamp(branchThickness, 0.05f, 1f);
            branchElevation = FiniteClamp(branchElevation, -90f, 90f);
            branchCurvature = FiniteClamp(branchCurvature, 0f, 1f);
            maximumBranchOrder = Mathf.Clamp(maximumBranchOrder, 1, 3);
            secondaryDensity = FiniteClamp(secondaryDensity, 0f, 8f);
            tertiaryDensity = FiniteClamp(tertiaryDensity, 0f, 8f);
            childScale = FiniteClamp(childScale, 0.05f, 0.90f);
            missingBranchChance = FiniteClamp(missingBranchChance, 0f, 1f);
            deadBranchChance = FiniteClamp(deadBranchChance, 0f, 1f);
            brokenBranchChance = FiniteClamp(brokenBranchChance, 0f, 1f);
            barkTint = ClampOpaqueColor01(barkTint);
            bendFrequency = FiniteClamp(bendFrequency, 0f, 6f);
            trunkDrift = FiniteClamp(trunkDrift, 0f, 0.50f);
            trunkRoughness = FiniteClamp(trunkRoughness, 0f, 0.50f);
            directionalBias = FiniteClamp(directionalBias, 0f, 1f);
            directionalBiasAngle = NormalizeAngle(directionalBiasAngle);
            tierSpacing = FiniteClamp(tierSpacing, 0f, 0.50f);
            branchArch = FiniteClamp(branchArch, -1f, 1f);
            lateSag = FiniteClamp(lateSag, 0f, 1f);
            tipUpturn = FiniteClamp(tipUpturn, 0f, 1f);
            sideSweep = FiniteClamp(sideSweep, -1f, 1f);
            forkChance = FiniteClamp(forkChance, 0f, 1f);
            schemaVersion = CurrentSchemaVersion;
        }

        private static float FiniteClamp(
            float value,
            float minimum,
            float maximum)
        {
            return TreeDeterministicUtility.IsFinite(value)
                ? Mathf.Clamp(value, minimum, maximum)
                : minimum;
        }

        private static float NormalizeAngle(float value)
        {
            if (!TreeDeterministicUtility.IsFinite(value))
            {
                return 0f;
            }

            value %= 360f;
            return value < 0f ? value + 360f : value;
        }

        private static Color ClampOpaqueColor01(Color value)
        {
            return new Color(
                FiniteClamp(value.r, 0f, 1f),
                FiniteClamp(value.g, 0f, 1f),
                FiniteClamp(value.b, 0f, 1f),
                1f);
        }
    }
}
