using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public sealed class TreeGenerationOverrides
    {
        [Header("Overall Form")]
        [SerializeField]
        private TreeFloatOverride height;

        [SerializeField]
        private TreeFloatOverride trunkBaseRadius;

        [SerializeField]
        private TreeFloatOverride crownStartHeight;

        [SerializeField]
        private TreeFloatOverride crownVolume;

        [SerializeField]
        private TreeFloatOverride crownWidthScale;

        [SerializeField]
        private TreeFloatOverride crownHeightScale;

        [SerializeField]
        private TreeFloatOverride crownFill;

        [SerializeField]
        private TreeFloatOverride crownAsymmetry;

        [SerializeField]
        private TreeIntOverride crownLobeCount;

        [SerializeField]
        private TreeFloatOverride crownLobeRadius;

        [Header("Trunk")]
        [SerializeField]
        private TreeIntOverride trunkControlPointCount;

        [SerializeField]
        private TreeFloatOverride trunkCurvature;

        [SerializeField]
        private TreeFloatOverride trunkBendCount;

        [SerializeField]
        private TreeFloatOverride trunkDirectionalDrift;

        [SerializeField]
        private TreeFloatOverride trunkLeanStrength;

        [SerializeField]
        private TreeFloatOverride trunkLeanDirectionDegrees;

        [FormerlySerializedAs("trunkTwistDegrees")]
        [InspectorName("Trunk Twist Degrees")]
        [SerializeField]
        private TreeFloatOverride trunkSurfaceTorsionDegrees;

        [Header("Root Buttress")]
        [InspectorName("Root Buttress Count")]
        [SerializeField]
        private TreeIntOverride rootButtressCount;

        [SerializeField]
        private TreeFloatOverride rootButtressStrength;

        [SerializeField]
        private TreeFloatOverride rootButtressHeight;

        [SerializeField]
        private TreeFloatOverride rootFlareScale;

        [Header("Trunk Path Spiral")]
        [InspectorName("Trunk Path Spiral Strength")]
        [SerializeField]
        private TreeFloatOverride trunkSpiralStrength;

        [InspectorName("Trunk Path Spiral Turns")]
        [SerializeField]
        private TreeFloatOverride trunkSpiralTurns;

        [InspectorName("Trunk Path Spiral Direction")]
        [SerializeField]
        private TreeFloatOverride trunkSpiralDirection;

        [SerializeField]
        private TreeFloatOverride trunkIrregularity;

        [SerializeField]
        private TreeFloatOverride trunkTaper;

        [SerializeField]
        private TreeFloatOverride trunkForkProbability;

        [SerializeField]
        private TreeFloatOverride trunkForkHeight;

        [Header("Primary Branches")]
        [SerializeField]
        private TreeIntOverride primaryBranchCount;

        [SerializeField]
        private TreeFloatOverride primaryBranchStartHeight;

        [SerializeField]
        private TreeFloatOverride primaryBranchEndHeight;

        [SerializeField]
        private TreeFloatOverride initialBranchElevationDegrees;

        [SerializeField]
        private TreeFloatOverride branchArchDirection;

        [SerializeField]
        private TreeFloatOverride branchArchStrength;

        [SerializeField]
        private TreeFloatOverride lateBranchSag;

        [SerializeField]
        private TreeFloatOverride azimuthSymmetry;

        [SerializeField]
        private TreeFloatOverride directionalBiasAngleDegrees;

        [SerializeField]
        private TreeFloatOverride directionalBiasStrength;

        [SerializeField]
        private TreeFloatOverride primaryBranchCurvature;

        [SerializeField, HideInInspector]
        private TreeFloatOverride primaryBranchDroop;

        [SerializeField, HideInInspector]
        private TreeFloatOverride primaryBranchUpwardBias;

        [SerializeField]
        private TreeFloatOverride primaryBranchSideSweep;

        [SerializeField]
        private TreeFloatOverride primaryBranchTwistDegrees;

        [SerializeField]
        private TreeFloatOverride primaryBranchIrregularity;

        [SerializeField]
        private TreeFloatOverride primaryBranchEndCurl;

        [SerializeField]
        private TreeFloatOverride primaryBranchLengthRatio;

        [SerializeField]
        private TreeFloatOverride primaryBranchRadiusRatio;

        [Header("Higher-Order Branches")]
        [SerializeField]
        private TreeIntOverride secondaryBranchesPerPrimary;

        [SerializeField]
        private TreeIntOverride tertiaryBranchesPerSecondary;

        [SerializeField]
        private TreeIntOverride maximumBranchOrder;

        [SerializeField]
        private TreeFloatOverride secondaryLengthRatio;

        [SerializeField]
        private TreeFloatOverride tertiaryLengthRatio;

        [SerializeField]
        private TreeFloatOverride higherOrderCurvatureScale;

        [Header("Foliage Volume")]
        [SerializeField]
        private TreeFloatOverride clusterWidthScale;

        [SerializeField]
        private TreeFloatOverride clusterHeightScale;

        [SerializeField]
        private TreeFloatOverride clusterLengthScale;

        [SerializeField]
        private TreeFloatOverride clusterRadialSpread;

        [SerializeField]
        private TreeFloatOverride cardSizeScale;

        [Header("Foliage Density")]
        [SerializeField]
        private TreeIntOverride foliageClusterCount;

        [SerializeField]
        private TreeIntOverride cardsPerCluster;

        [SerializeField]
        private TreeFloatOverride foliageEligibility;

        [SerializeField]
        private TreeFloatOverride clusterOccupancy;

        [SerializeField]
        private TreeFloatOverride terminalFoliageProbability;

        [SerializeField]
        private TreeFloatOverride cardRetentionFraction;

        [Header("Damage")]
        [SerializeField]
        private TreeFloatOverride missingBranchProbability;

        [SerializeField]
        private TreeFloatOverride deadBranchProbability;

        [SerializeField]
        private TreeFloatOverride breakProbability;

        [Header("Palette")]
        [SerializeField]
        private TreeColorOverride barkTint;

        [SerializeField]
        private TreeColorOverride foliageBaseColor;

        [SerializeField]
        private TreeColorOverride foliageHighlightColor;

        [SerializeField]
        private TreeColorOverride foliageShadowColor;

        public TreeFloatOverride Height => height;
        public TreeFloatOverride TrunkBaseRadius => trunkBaseRadius;
        public TreeFloatOverride CrownStartHeight => crownStartHeight;
        public TreeFloatOverride CrownVolume => crownVolume;
        public TreeFloatOverride CrownWidthScale => crownWidthScale;
        public TreeFloatOverride CrownHeightScale => crownHeightScale;
        public TreeFloatOverride CrownFill => crownFill;
        public TreeFloatOverride CrownAsymmetry => crownAsymmetry;
        public TreeIntOverride CrownLobeCount => crownLobeCount;
        public TreeFloatOverride CrownLobeRadius => crownLobeRadius;
        public TreeIntOverride TrunkControlPointCount => trunkControlPointCount;
        public TreeFloatOverride TrunkCurvature => trunkCurvature;
        public TreeFloatOverride TrunkBendCount => trunkBendCount;
        public TreeFloatOverride TrunkDirectionalDrift => trunkDirectionalDrift;
        public TreeFloatOverride TrunkLeanStrength => trunkLeanStrength;
        public TreeFloatOverride TrunkLeanDirectionDegrees => trunkLeanDirectionDegrees;
        public TreeFloatOverride TrunkTwistDegrees => trunkSurfaceTorsionDegrees;
        public TreeFloatOverride TrunkSurfaceTorsionDegrees => trunkSurfaceTorsionDegrees;
        public TreeIntOverride RootButtressCount => rootButtressCount;
        public TreeFloatOverride RootButtressStrength => rootButtressStrength;
        public TreeFloatOverride RootButtressHeight => rootButtressHeight;
        public TreeFloatOverride RootFlareScale => rootFlareScale;
        public TreeFloatOverride TrunkSpiralStrength => trunkSpiralStrength;
        public TreeFloatOverride TrunkSpiralTurns => trunkSpiralTurns;
        public TreeFloatOverride TrunkSpiralDirection => trunkSpiralDirection;
        public TreeFloatOverride TrunkIrregularity => trunkIrregularity;
        public TreeFloatOverride TrunkTaper => trunkTaper;
        public TreeFloatOverride TrunkForkProbability => trunkForkProbability;
        public TreeFloatOverride TrunkForkHeight => trunkForkHeight;
        public TreeIntOverride PrimaryBranchCount => primaryBranchCount;
        public TreeFloatOverride PrimaryBranchStartHeight => primaryBranchStartHeight;
        public TreeFloatOverride PrimaryBranchEndHeight => primaryBranchEndHeight;
        public TreeFloatOverride InitialBranchElevationDegrees => initialBranchElevationDegrees;
        public TreeFloatOverride BranchArchDirection => branchArchDirection;
        public TreeFloatOverride BranchArchStrength => branchArchStrength;
        public TreeFloatOverride LateBranchSag => lateBranchSag;
        public TreeFloatOverride AzimuthSymmetry => azimuthSymmetry;
        public TreeFloatOverride DirectionalBiasAngleDegrees => directionalBiasAngleDegrees;
        public TreeFloatOverride DirectionalBiasStrength => directionalBiasStrength;
        public TreeFloatOverride PrimaryBranchCurvature => primaryBranchCurvature;
        public TreeFloatOverride PrimaryBranchDroop => primaryBranchDroop;
        public TreeFloatOverride PrimaryBranchUpwardBias => primaryBranchUpwardBias;
        public TreeFloatOverride PrimaryBranchSideSweep => primaryBranchSideSweep;
        public TreeFloatOverride PrimaryBranchTwistDegrees => primaryBranchTwistDegrees;
        public TreeFloatOverride PrimaryBranchIrregularity => primaryBranchIrregularity;
        public TreeFloatOverride PrimaryBranchEndCurl => primaryBranchEndCurl;
        public TreeFloatOverride PrimaryBranchLengthRatio => primaryBranchLengthRatio;
        public TreeFloatOverride PrimaryBranchRadiusRatio => primaryBranchRadiusRatio;
        public TreeIntOverride SecondaryBranchesPerPrimary => secondaryBranchesPerPrimary;
        public TreeIntOverride TertiaryBranchesPerSecondary => tertiaryBranchesPerSecondary;
        public TreeIntOverride MaximumBranchOrder => maximumBranchOrder;
        public TreeFloatOverride SecondaryLengthRatio => secondaryLengthRatio;
        public TreeFloatOverride TertiaryLengthRatio => tertiaryLengthRatio;
        public TreeFloatOverride HigherOrderCurvatureScale => higherOrderCurvatureScale;
        public TreeFloatOverride ClusterWidthScale => clusterWidthScale;
        public TreeFloatOverride ClusterHeightScale => clusterHeightScale;
        public TreeFloatOverride ClusterLengthScale => clusterLengthScale;
        public TreeFloatOverride ClusterRadialSpread => clusterRadialSpread;
        public TreeFloatOverride CardSizeScale => cardSizeScale;
        public TreeIntOverride FoliageClusterCount => foliageClusterCount;
        public TreeIntOverride CardsPerCluster => cardsPerCluster;
        public TreeFloatOverride FoliageEligibility => foliageEligibility;
        public TreeFloatOverride ClusterOccupancy => clusterOccupancy;
        public TreeFloatOverride TerminalFoliageProbability => terminalFoliageProbability;
        public TreeFloatOverride CardRetentionFraction => cardRetentionFraction;
        public TreeFloatOverride MissingBranchProbability => missingBranchProbability;
        public TreeFloatOverride DeadBranchProbability => deadBranchProbability;
        public TreeFloatOverride BreakProbability => breakProbability;
        public TreeColorOverride BarkTint => barkTint;
        public TreeColorOverride FoliageBaseColor => foliageBaseColor;
        public TreeColorOverride FoliageHighlightColor => foliageHighlightColor;
        public TreeColorOverride FoliageShadowColor => foliageShadowColor;

        public bool HasAnyOverride
        {
            get
            {
                return height.IsSet ||
                    trunkBaseRadius.IsSet ||
                    crownStartHeight.IsSet ||
                    crownVolume.IsSet ||
                    crownWidthScale.IsSet ||
                    crownHeightScale.IsSet ||
                    crownFill.IsSet ||
                    crownAsymmetry.IsSet ||
                    crownLobeCount.IsSet ||
                    crownLobeRadius.IsSet ||
                    trunkControlPointCount.IsSet ||
                    trunkCurvature.IsSet ||
                    trunkBendCount.IsSet ||
                    trunkDirectionalDrift.IsSet ||
                    trunkLeanStrength.IsSet ||
                    trunkLeanDirectionDegrees.IsSet ||
                    trunkSurfaceTorsionDegrees.IsSet ||
                    rootButtressCount.IsSet ||
                    rootButtressStrength.IsSet ||
                    rootButtressHeight.IsSet ||
                    rootFlareScale.IsSet ||
                    trunkSpiralStrength.IsSet ||
                    trunkSpiralTurns.IsSet ||
                    trunkSpiralDirection.IsSet ||
                    trunkIrregularity.IsSet ||
                    trunkTaper.IsSet ||
                    trunkForkProbability.IsSet ||
                    trunkForkHeight.IsSet ||
                    primaryBranchCount.IsSet ||
                    primaryBranchStartHeight.IsSet ||
                    primaryBranchEndHeight.IsSet ||
                    initialBranchElevationDegrees.IsSet ||
                    branchArchDirection.IsSet ||
                    branchArchStrength.IsSet ||
                    lateBranchSag.IsSet ||
                    azimuthSymmetry.IsSet ||
                    directionalBiasAngleDegrees.IsSet ||
                    directionalBiasStrength.IsSet ||
                    primaryBranchCurvature.IsSet ||
                    primaryBranchDroop.IsSet ||
                    primaryBranchUpwardBias.IsSet ||
                    primaryBranchSideSweep.IsSet ||
                    primaryBranchTwistDegrees.IsSet ||
                    primaryBranchIrregularity.IsSet ||
                    primaryBranchEndCurl.IsSet ||
                    primaryBranchLengthRatio.IsSet ||
                    primaryBranchRadiusRatio.IsSet ||
                    secondaryBranchesPerPrimary.IsSet ||
                    tertiaryBranchesPerSecondary.IsSet ||
                    maximumBranchOrder.IsSet ||
                    secondaryLengthRatio.IsSet ||
                    tertiaryLengthRatio.IsSet ||
                    higherOrderCurvatureScale.IsSet ||
                    clusterWidthScale.IsSet ||
                    clusterHeightScale.IsSet ||
                    clusterLengthScale.IsSet ||
                    clusterRadialSpread.IsSet ||
                    cardSizeScale.IsSet ||
                    foliageClusterCount.IsSet ||
                    cardsPerCluster.IsSet ||
                    foliageEligibility.IsSet ||
                    clusterOccupancy.IsSet ||
                    terminalFoliageProbability.IsSet ||
                    cardRetentionFraction.IsSet ||
                    missingBranchProbability.IsSet ||
                    deadBranchProbability.IsSet ||
                    breakProbability.IsSet ||
                    barkTint.Enabled ||
                    foliageBaseColor.Enabled ||
                    foliageHighlightColor.Enabled ||
                    foliageShadowColor.Enabled;
            }
        }

        public TreeGenerationOverrides Clone()
        {
            return JsonUtility.FromJson<TreeGenerationOverrides>(
                JsonUtility.ToJson(this));
        }


        public void ConfigureReferenceDimensions(
            float visibleHeight,
            float visibleWidth)
        {
            height = TreeFloatOverride.Exact(Mathf.Max(0.1f, visibleHeight));
        }

        public void ConfigureReferenceDimensions(
            float visibleHeight,
            float visibleWidth,
            TreeFloatRange allowedPrimaryLengthRatio)
        {
            float safeHeight = Mathf.Max(0.1f, visibleHeight);
            float safeWidth = Mathf.Max(0.1f, visibleWidth);
            height = TreeFloatOverride.Exact(safeHeight);
            primaryBranchLengthRatio = TreeFloatOverride.Exact(
                allowedPrimaryLengthRatio.Clamp(
                    (safeWidth / safeHeight) * 0.5f));
        }

        internal void SetCrownVolumeForTest(float value)
        {
            crownVolume = TreeFloatOverride.Exact(value);
        }

        internal void SetFoliageClusterCountForTest(int value)
        {
            foliageClusterCount = TreeIntOverride.Exact(value);
        }

        internal void SetPrimaryBranchCountForTest(int value)
        {
            primaryBranchCount = TreeIntOverride.Exact(value);
        }

        internal void SetTrunkCurvatureForTest(float value)
        {
            trunkCurvature = TreeFloatOverride.Exact(value);
        }

        internal void SetTrunkTwistDegreesForTest(float value)
        {
            trunkSurfaceTorsionDegrees = TreeFloatOverride.Exact(value);
        }

        internal bool EnsureManagedTrunkTwistDefault(float value)
        {
            if (trunkSurfaceTorsionDegrees.IsSet)
            {
                return false;
            }

            trunkSurfaceTorsionDegrees = TreeFloatOverride.Exact(value);
            return true;
        }

        internal bool UpgradeManagedTrunkTwistDefault(
            float previousManagedValue,
            float currentManagedValue)
        {
            if (!trunkSurfaceTorsionDegrees.IsSet)
            {
                trunkSurfaceTorsionDegrees =
                    TreeFloatOverride.Exact(currentManagedValue);
                return true;
            }

            if (trunkSurfaceTorsionDegrees.Mode !=
                TreeOverrideMode.Exact)
            {
                return false;
            }

            if (Mathf.Abs(
                    trunkSurfaceTorsionDegrees.ExactValue -
                    currentManagedValue) <= 0.0001f)
            {
                return false;
            }

            if (Mathf.Abs(
                    trunkSurfaceTorsionDegrees.ExactValue -
                    previousManagedValue) <= 0.0001f)
            {
                trunkSurfaceTorsionDegrees =
                    TreeFloatOverride.Exact(currentManagedValue);
                return true;
            }

            return false;
        }

        internal bool EnsureManagedRootButtressDefaults(
            int buttressCount,
            float buttressStrength,
            float buttressHeight,
            float flareScale)
        {
            bool changed = false;
            if (!rootButtressCount.IsSet)
            {
                rootButtressCount = TreeIntOverride.Exact(
                    Mathf.Clamp(buttressCount, 3, 8));
                changed = true;
            }
            if (!rootButtressStrength.IsSet)
            {
                rootButtressStrength =
                    TreeFloatOverride.Exact(buttressStrength);
                changed = true;
            }
            if (!rootButtressHeight.IsSet)
            {
                rootButtressHeight =
                    TreeFloatOverride.Exact(buttressHeight);
                changed = true;
            }
            if (!rootFlareScale.IsSet)
            {
                rootFlareScale = TreeFloatOverride.Exact(flareScale);
                changed = true;
            }

            return changed;
        }

        internal bool UpgradeManagedRootButtressDefaults(
            int previousCount,
            int currentCount,
            float previousStrength,
            float currentStrength,
            float currentHeight,
            float previousFlare,
            float currentFlare)
        {
            bool changed = UpgradeManagedExact(
                ref rootButtressCount,
                previousCount,
                currentCount);
            changed |= UpgradeManagedExact(
                ref rootButtressStrength,
                previousStrength,
                currentStrength);
            if (!rootButtressHeight.IsSet)
            {
                rootButtressHeight =
                    TreeFloatOverride.Exact(currentHeight);
                changed = true;
            }
            changed |= UpgradeManagedExact(
                ref rootFlareScale,
                previousFlare,
                currentFlare);
            return changed;
        }

        private static bool UpgradeManagedExact(
            ref TreeIntOverride value,
            int previousManagedValue,
            int currentManagedValue)
        {
            currentManagedValue = Mathf.Clamp(currentManagedValue, 3, 8);
            previousManagedValue = Mathf.Clamp(previousManagedValue, 3, 8);
            if (!value.IsSet)
            {
                value = TreeIntOverride.Exact(currentManagedValue);
                return true;
            }

            if (value.Mode != TreeOverrideMode.Exact ||
                value.ExactValue != previousManagedValue)
            {
                return false;
            }

            value = TreeIntOverride.Exact(currentManagedValue);
            return true;
        }

        private static bool UpgradeManagedExact(
            ref TreeFloatOverride value,
            float previousManagedValue,
            float currentManagedValue)
        {
            if (!value.IsSet)
            {
                value = TreeFloatOverride.Exact(currentManagedValue);
                return true;
            }

            if (value.Mode != TreeOverrideMode.Exact ||
                Mathf.Abs(value.ExactValue - previousManagedValue) > 0.0001f)
            {
                return false;
            }

            value = TreeFloatOverride.Exact(currentManagedValue);
            return true;
        }

        internal bool EnsureManagedPathSpiralDefaults(
            float strength,
            float turns,
            float direction)
        {
            bool changed = false;
            if (!trunkSpiralStrength.IsSet)
            {
                trunkSpiralStrength = TreeFloatOverride.Exact(strength);
                changed = true;
            }

            if (!trunkSpiralTurns.IsSet)
            {
                trunkSpiralTurns = TreeFloatOverride.Exact(turns);
                changed = true;
            }

            if (!trunkSpiralDirection.IsSet)
            {
                trunkSpiralDirection = TreeFloatOverride.Exact(
                    direction < 0f ? -1f : 1f);
                changed = true;
            }

            return changed;
        }

        internal void SetRootButtressCountForTest(int value)
        {
            rootButtressCount = TreeIntOverride.Exact(value);
        }

        internal void SetRootButtressStrengthForTest(float value)
        {
            rootButtressStrength = TreeFloatOverride.Exact(value);
        }

        internal void SetTrunkSpiralStrengthForTest(float value)
        {
            trunkSpiralStrength = TreeFloatOverride.Exact(value);
        }

        internal void SetBranchArchStrengthForTest(float value)
        {
            branchArchStrength = TreeFloatOverride.Exact(value);
        }

        internal void SetPrimaryBranchStartHeightForTest(float value)
        {
            primaryBranchStartHeight = TreeFloatOverride.Exact(value);
        }

        internal void SetAzimuthSymmetryForTest(float value)
        {
            azimuthSymmetry = TreeFloatOverride.Exact(value);
        }

        internal void SetDirectionalBiasStrengthForTest(float value)
        {
            directionalBiasStrength = TreeFloatOverride.Exact(value);
        }

        internal void SetBarkTintForTest(Color value)
        {
            barkTint = TreeColorOverride.Exact(value);
        }

        internal bool UpgradeTreeGen2BControls()
        {
            bool changed = false;
            if (!initialBranchElevationDegrees.IsSet &&
                primaryBranchUpwardBias.IsSet)
            {
                GetOverrideBounds(
                    primaryBranchUpwardBias,
                    out float upwardMinimum,
                    out float upwardMaximum);
                float droopMinimum = 0f;
                float droopMaximum = 0f;
                if (primaryBranchDroop.IsSet)
                {
                    GetOverrideBounds(
                        primaryBranchDroop,
                        out droopMinimum,
                        out droopMaximum);
                }

                float minimumElevation = Mathf.Atan(
                    upwardMinimum - droopMaximum + 0.08f) * Mathf.Rad2Deg;
                float maximumElevation = Mathf.Atan(
                    upwardMaximum - droopMinimum + 0.08f) * Mathf.Rad2Deg;
                initialBranchElevationDegrees =
                    primaryBranchUpwardBias.Mode == TreeOverrideMode.Exact &&
                    (!primaryBranchDroop.IsSet ||
                     primaryBranchDroop.Mode == TreeOverrideMode.Exact)
                        ? TreeFloatOverride.Exact(minimumElevation)
                        : TreeFloatOverride.Ranged(
                            Mathf.Min(minimumElevation, maximumElevation),
                            Mathf.Max(minimumElevation, maximumElevation));
                changed = true;
            }

            if (!lateBranchSag.IsSet && primaryBranchDroop.IsSet)
            {
                lateBranchSag = MapFloatOverride(
                    primaryBranchDroop,
                    value => Mathf.Max(0f, value));
                changed = true;
            }

            if (!branchArchDirection.IsSet && primaryBranchDroop.IsSet)
            {
                float representative = primaryBranchDroop.Mode ==
                    TreeOverrideMode.Range
                    ? primaryBranchDroop.Range.Midpoint
                    : primaryBranchDroop.ExactValue;
                if (Mathf.Abs(representative) > 0.0001f)
                {
                    branchArchDirection = TreeFloatOverride.Exact(
                        representative < 0f ? 1f : -1f);
                    changed = true;
                }
            }

            return changed;
        }


        private static void GetOverrideBounds(
            TreeFloatOverride source,
            out float minimum,
            out float maximum)
        {
            if (source.Mode == TreeOverrideMode.Range)
            {
                TreeFloatRange ordered = source.Range.Ordered();
                minimum = ordered.Minimum;
                maximum = ordered.Maximum;
                return;
            }

            minimum = source.ExactValue;
            maximum = source.ExactValue;
        }

        private static TreeFloatOverride MapFloatOverride(
            TreeFloatOverride source,
            Func<float, float> map)
        {
            switch (source.Mode)
            {
                case TreeOverrideMode.Exact:
                    return TreeFloatOverride.Exact(map(source.ExactValue));
                case TreeOverrideMode.Range:
                    float a = map(source.Range.Minimum);
                    float b = map(source.Range.Maximum);
                    return TreeFloatOverride.Ranged(
                        Mathf.Min(a, b),
                        Mathf.Max(a, b));
                default:
                    return default;
            }
        }

        internal bool EnsureLegacyPrimaryAttachmentInterval(
            TreeFloatRange legacyAttachmentRange)
        {
            TreeFloatRange ordered = legacyAttachmentRange.Ordered();
            bool changed = false;
            if (!primaryBranchStartHeight.IsSet)
            {
                primaryBranchStartHeight = TreeFloatOverride.Exact(
                    ordered.Minimum);
                changed = true;
            }

            if (!primaryBranchEndHeight.IsSet)
            {
                primaryBranchEndHeight = TreeFloatOverride.Exact(
                    ordered.Maximum);
                changed = true;
            }

            return changed;
        }

        internal bool EnsureNeutralComparisonBarkTint()
        {
            if (barkTint.Enabled)
            {
                return false;
            }

            barkTint = TreeColorOverride.Exact(Color.white);
            return true;
        }

        internal void SetFoliageColorForTest(Color value)
        {
            foliageBaseColor = TreeColorOverride.Exact(value);
        }
    }
}
