using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public sealed class TreeResolvedParameters
    {
        [SerializeField]
        private TreeFamily family;

        [SerializeField]
        private float height;

        [SerializeField]
        private float trunkBaseRadius;

        [SerializeField]
        private float crownStartHeight;

        [SerializeField]
        private float crownVolume;

        [SerializeField]
        private float crownWidthScale;

        [SerializeField]
        private float crownHeightScale;

        [SerializeField]
        private float crownFill;

        [SerializeField]
        private float crownAsymmetry;

        [SerializeField]
        private int crownLobeCount;

        [SerializeField]
        private float crownLobeRadius;

        [SerializeField]
        private int trunkControlPointCount;

        [SerializeField]
        private float trunkCurvature;

        [SerializeField]
        private float trunkBendCount;

        [SerializeField]
        private float trunkDirectionalDrift;

        [SerializeField]
        private float trunkLeanStrength;

        [FormerlySerializedAs("trunkTwistDegrees")]
        [SerializeField]
        private float trunkSurfaceTorsionDegrees;

        [SerializeField]
        private int rootButtressCount;

        [SerializeField]
        private float rootButtressStrength;

        [SerializeField]
        private float rootButtressHeight;

        [SerializeField]
        private float buttressTransition = 1f;

        [SerializeField]
        private float rootFlareScale = 1f;

        [SerializeField]
        private bool recipeOnlyControlSource;

        [SerializeField]
        private float rootReach;

        [SerializeField]
        private float rootThickness = 1f;

        [SerializeField]
        private float secondaryDensity;

        [SerializeField]
        private float tertiaryDensity;

        [SerializeField]
        private float childScale;

        [SerializeField]
        private float tierSpacing;

        [SerializeField]
        private float tipUpturn;

        [SerializeField]
        private float trunkSpiralStrength;

        [SerializeField]
        private float trunkSpiralTurns;

        [SerializeField]
        private float trunkSpiralDirection = 1f;

        [SerializeField]
        private float trunkIrregularity;

        [SerializeField]
        private float trunkTaper;

        [SerializeField]
        private float trunkForkProbability;

        [SerializeField]
        private float trunkForkHeight;

        [SerializeField]
        private int primaryBranchCount;

        [SerializeField]
        private float primaryBranchStartHeight;

        [SerializeField]
        private float primaryBranchEndHeight;

        [SerializeField]
        private float initialBranchElevationDegrees;

        [SerializeField]
        private float branchArchDirection;

        [SerializeField]
        private float branchArchStrength;

        [SerializeField]
        private float lateBranchSag;

        [SerializeField]
        private float azimuthSymmetry;

        [SerializeField]
        private float directionalBiasAngleDegrees;

        [SerializeField]
        private float directionalBiasStrength;

        [SerializeField]
        private float primaryAttachmentMinimum;

        [SerializeField]
        private float primaryAttachmentMaximum;

        [SerializeField]
        private float primaryBranchCurvature;

        [SerializeField]
        private float primaryBranchDroop;

        [SerializeField]
        private float primaryBranchUpwardBias;

        [SerializeField]
        private float primaryBranchSideSweep;

        [SerializeField]
        private float primaryBranchTwistDegrees;

        [SerializeField]
        private float primaryBranchIrregularity;

        [SerializeField]
        private float primaryBranchEndCurl;

        [SerializeField]
        private float primaryBranchLengthRatio;

        [SerializeField]
        private float primaryBranchRadiusRatio;

        [SerializeField]
        private int secondaryBranchesPerPrimary;

        [SerializeField]
        private int tertiaryBranchesPerSecondary;

        [SerializeField]
        private int maximumBranchOrder;

        [SerializeField]
        private float secondaryLengthRatio;

        [SerializeField]
        private float tertiaryLengthRatio;

        [SerializeField]
        private float higherOrderCurvatureScale;

        [SerializeField]
        private float clusterWidthScale;

        [SerializeField]
        private float clusterHeightScale;

        [SerializeField]
        private float clusterLengthScale;

        [SerializeField]
        private float clusterRadialSpread;

        [SerializeField]
        private float cardSizeScale;

        [SerializeField]
        private int foliageClusterCount;

        [SerializeField]
        private int cardsPerCluster;

        [SerializeField]
        private float foliageEligibility;

        [SerializeField]
        private float clusterOccupancy;

        [SerializeField]
        private float terminalFoliageProbability;

        [SerializeField]
        private float cardRetentionFraction;

        [SerializeField]
        private float missingBranchProbability;

        [SerializeField]
        private float deadBranchProbability;

        [SerializeField]
        private float breakProbability;

        [SerializeField]
        private Color barkTint = Color.white;

        [SerializeField]
        private Color foliageBaseColor = Color.white;

        [SerializeField]
        private Color foliageHighlightColor = Color.white;

        [SerializeField]
        private Color foliageShadowColor = Color.black;

        [SerializeField]
        private List<string> ownershipTrace = new List<string>();

        public TreeFamily Family { get => family; internal set => family = value; }
        public float Height { get => height; internal set => height = value; }
        public float TrunkBaseRadius { get => trunkBaseRadius; internal set => trunkBaseRadius = value; }
        public float CrownStartHeight { get => crownStartHeight; internal set => crownStartHeight = value; }
        public float CrownVolume { get => crownVolume; internal set => crownVolume = value; }
        public float CrownWidthScale { get => crownWidthScale; internal set => crownWidthScale = value; }
        public float CrownHeightScale { get => crownHeightScale; internal set => crownHeightScale = value; }
        public float CrownFill { get => crownFill; internal set => crownFill = value; }
        public float CrownAsymmetry { get => crownAsymmetry; internal set => crownAsymmetry = value; }
        public int CrownLobeCount { get => crownLobeCount; internal set => crownLobeCount = value; }
        public float CrownLobeRadius { get => crownLobeRadius; internal set => crownLobeRadius = value; }
        public int TrunkControlPointCount { get => trunkControlPointCount; internal set => trunkControlPointCount = value; }
        public float TrunkCurvature { get => trunkCurvature; internal set => trunkCurvature = value; }
        public float TrunkBendCount { get => trunkBendCount; internal set => trunkBendCount = value; }
        public float TrunkDirectionalDrift { get => trunkDirectionalDrift; internal set => trunkDirectionalDrift = value; }
        public float TrunkLeanStrength { get => trunkLeanStrength; internal set => trunkLeanStrength = value; }
        public float TrunkTwistDegrees { get => trunkSurfaceTorsionDegrees; internal set => trunkSurfaceTorsionDegrees = value; }
        public float TrunkSurfaceTorsionDegrees { get => trunkSurfaceTorsionDegrees; internal set => trunkSurfaceTorsionDegrees = value; }
        public int RootButtressCount { get => rootButtressCount; internal set => rootButtressCount = value; }
        public float RootButtressStrength { get => rootButtressStrength; internal set => rootButtressStrength = value; }
        public float RootButtressHeight { get => rootButtressHeight; internal set => rootButtressHeight = value; }
        public float ButtressTransition { get => buttressTransition; internal set => buttressTransition = value; }
        public float RootFlareScale { get => rootFlareScale; internal set => rootFlareScale = value; }
        public bool RecipeOnlyControlSource { get => recipeOnlyControlSource; internal set => recipeOnlyControlSource = value; }
        public float RootReach { get => rootReach; internal set => rootReach = value; }
        public float RootThickness { get => rootThickness; internal set => rootThickness = value; }
        public float SecondaryDensity { get => secondaryDensity; internal set => secondaryDensity = value; }
        public float TertiaryDensity { get => tertiaryDensity; internal set => tertiaryDensity = value; }
        public float ChildScale { get => childScale; internal set => childScale = value; }
        public float TierSpacing { get => tierSpacing; internal set => tierSpacing = value; }
        public float TipUpturn { get => tipUpturn; internal set => tipUpturn = value; }
        public float TrunkSpiralStrength { get => trunkSpiralStrength; internal set => trunkSpiralStrength = value; }
        public float TrunkSpiralTurns { get => trunkSpiralTurns; internal set => trunkSpiralTurns = value; }
        public float TrunkSpiralDirection { get => trunkSpiralDirection; internal set => trunkSpiralDirection = value; }
        public float TrunkIrregularity { get => trunkIrregularity; internal set => trunkIrregularity = value; }
        public float TrunkTaper { get => trunkTaper; internal set => trunkTaper = value; }
        public float TrunkForkProbability { get => trunkForkProbability; internal set => trunkForkProbability = value; }
        public float TrunkForkHeight { get => trunkForkHeight; internal set => trunkForkHeight = value; }
        public int PrimaryBranchCount { get => primaryBranchCount; internal set => primaryBranchCount = value; }
        public float PrimaryBranchStartHeight { get => primaryBranchStartHeight; internal set => primaryBranchStartHeight = value; }
        public float PrimaryBranchEndHeight { get => primaryBranchEndHeight; internal set => primaryBranchEndHeight = value; }
        public float InitialBranchElevationDegrees { get => initialBranchElevationDegrees; internal set => initialBranchElevationDegrees = value; }
        public float BranchArchDirection { get => branchArchDirection; internal set => branchArchDirection = value; }
        public float BranchArchStrength { get => branchArchStrength; internal set => branchArchStrength = value; }
        public float LateBranchSag { get => lateBranchSag; internal set => lateBranchSag = value; }
        public float AzimuthSymmetry { get => azimuthSymmetry; internal set => azimuthSymmetry = value; }
        public float DirectionalBiasAngleDegrees { get => directionalBiasAngleDegrees; internal set => directionalBiasAngleDegrees = value; }
        public float DirectionalBiasStrength { get => directionalBiasStrength; internal set => directionalBiasStrength = value; }
        public float PrimaryAttachmentMinimum { get => primaryAttachmentMinimum; internal set => primaryAttachmentMinimum = value; }
        public float PrimaryAttachmentMaximum { get => primaryAttachmentMaximum; internal set => primaryAttachmentMaximum = value; }
        public float PrimaryBranchCurvature { get => primaryBranchCurvature; internal set => primaryBranchCurvature = value; }
        public float PrimaryBranchDroop { get => primaryBranchDroop; internal set => primaryBranchDroop = value; }
        public float PrimaryBranchUpwardBias { get => primaryBranchUpwardBias; internal set => primaryBranchUpwardBias = value; }
        public float PrimaryBranchSideSweep { get => primaryBranchSideSweep; internal set => primaryBranchSideSweep = value; }
        public float PrimaryBranchTwistDegrees { get => primaryBranchTwistDegrees; internal set => primaryBranchTwistDegrees = value; }
        public float PrimaryBranchIrregularity { get => primaryBranchIrregularity; internal set => primaryBranchIrregularity = value; }
        public float PrimaryBranchEndCurl { get => primaryBranchEndCurl; internal set => primaryBranchEndCurl = value; }
        public float PrimaryBranchLengthRatio { get => primaryBranchLengthRatio; internal set => primaryBranchLengthRatio = value; }
        public float PrimaryBranchRadiusRatio { get => primaryBranchRadiusRatio; internal set => primaryBranchRadiusRatio = value; }
        public int SecondaryBranchesPerPrimary { get => secondaryBranchesPerPrimary; internal set => secondaryBranchesPerPrimary = value; }
        public int TertiaryBranchesPerSecondary { get => tertiaryBranchesPerSecondary; internal set => tertiaryBranchesPerSecondary = value; }
        public int MaximumBranchOrder { get => maximumBranchOrder; internal set => maximumBranchOrder = value; }
        public float SecondaryLengthRatio { get => secondaryLengthRatio; internal set => secondaryLengthRatio = value; }
        public float TertiaryLengthRatio { get => tertiaryLengthRatio; internal set => tertiaryLengthRatio = value; }
        public float HigherOrderCurvatureScale { get => higherOrderCurvatureScale; internal set => higherOrderCurvatureScale = value; }
        public float ClusterWidthScale { get => clusterWidthScale; internal set => clusterWidthScale = value; }
        public float ClusterHeightScale { get => clusterHeightScale; internal set => clusterHeightScale = value; }
        public float ClusterLengthScale { get => clusterLengthScale; internal set => clusterLengthScale = value; }
        public float ClusterRadialSpread { get => clusterRadialSpread; internal set => clusterRadialSpread = value; }
        public float CardSizeScale { get => cardSizeScale; internal set => cardSizeScale = value; }
        public int FoliageClusterCount { get => foliageClusterCount; internal set => foliageClusterCount = value; }
        public int CardsPerCluster { get => cardsPerCluster; internal set => cardsPerCluster = value; }
        public float FoliageEligibility { get => foliageEligibility; internal set => foliageEligibility = value; }
        public float ClusterOccupancy { get => clusterOccupancy; internal set => clusterOccupancy = value; }
        public float TerminalFoliageProbability { get => terminalFoliageProbability; internal set => terminalFoliageProbability = value; }
        public float CardRetentionFraction { get => cardRetentionFraction; internal set => cardRetentionFraction = value; }
        public float MissingBranchProbability { get => missingBranchProbability; internal set => missingBranchProbability = value; }
        public float DeadBranchProbability { get => deadBranchProbability; internal set => deadBranchProbability = value; }
        public float BreakProbability { get => breakProbability; internal set => breakProbability = value; }
        public Color BarkTint { get => barkTint; internal set => barkTint = value; }
        public Color FoliageBaseColor { get => foliageBaseColor; internal set => foliageBaseColor = value; }
        public Color FoliageHighlightColor { get => foliageHighlightColor; internal set => foliageHighlightColor = value; }
        public Color FoliageShadowColor { get => foliageShadowColor; internal set => foliageShadowColor = value; }
        public IReadOnlyList<string> OwnershipTrace => ownershipTrace;

        internal void AddOwnership(string entry)
        {
            if (!string.IsNullOrEmpty(entry))
            {
                ownershipTrace.Add(entry);
            }
        }

        public TreeResolvedParameters Clone()
        {
            return JsonUtility.FromJson<TreeResolvedParameters>(JsonUtility.ToJson(this));
        }
    }
}
