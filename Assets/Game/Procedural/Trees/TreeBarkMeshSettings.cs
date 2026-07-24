using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public sealed class TreeBarkMeshSettings
    {
        public const int CurrentSettingsVersion = 4;

        [SerializeField, HideInInspector]
        private int settingsVersion = CurrentSettingsVersion;

        [SerializeField, Range(3, 24)]
        private int trunkRadialSegments = 10;

        [SerializeField, Range(3, 16)]
        private int primaryRadialSegments = 8;

        [SerializeField, Range(3, 12)]
        private int secondaryRadialSegments = 6;

        [SerializeField, Range(3, 10)]
        private int tertiaryRadialSegments = 5;

        [SerializeField, Min(0.1f)]
        private float barkMetersPerTile = 1.8f;

        [SerializeField, Min(0.0001f)]
        private float minimumRenderedRadius = 0.006f;

        [SerializeField]
        private bool capTrunkBase = true;

        [SerializeField]
        private bool capBranchTips = true;

        [Header("Branch Root Transition")]
        [SerializeField, Range(0f, 1f)]
        private float branchRootInsetRatio = 0.18f;

        [SerializeField, Range(0.5f, 8f)]
        private float branchRootBlendLengthInChildRadii = 2.4f;

        [SerializeField, Range(0.5f, 2f)]
        private float branchRootRadiusScale = 1.2f;

        [SerializeField, Range(0f, 1f)]
        private float branchRootCollarStrength = 0.45f;

        [SerializeField, Range(2, 8)]
        private int branchRootTransitionRingCount = 3;

        public int SettingsVersion => settingsVersion;
        public int TrunkRadialSegments => Mathf.Clamp(trunkRadialSegments, 3, 24);
        public int PrimaryRadialSegments => Mathf.Clamp(primaryRadialSegments, 3, 16);
        public int SecondaryRadialSegments => Mathf.Clamp(secondaryRadialSegments, 3, 12);
        public int TertiaryRadialSegments => Mathf.Clamp(tertiaryRadialSegments, 3, 10);
        public float BarkMetersPerTile => Mathf.Max(0.1f, barkMetersPerTile);
        public float MinimumRenderedRadius => Mathf.Max(0.0001f, minimumRenderedRadius);
        public bool CapTrunkBase => capTrunkBase;
        public bool CapBranchTips => capBranchTips;
        public float BranchRootInsetRatio => Mathf.Clamp01(branchRootInsetRatio);
        public float BranchRootBlendLengthInChildRadii => Mathf.Clamp(branchRootBlendLengthInChildRadii, 0.5f, 8f);
        public float BranchRootRadiusScale => Mathf.Clamp(branchRootRadiusScale, 0.5f, 2f);
        public float BranchRootCollarStrength => Mathf.Clamp01(branchRootCollarStrength);
        public int BranchRootTransitionRingCount => Mathf.Clamp(branchRootTransitionRingCount, 2, 8);

        public int ResolveRadialSegments(
            int branchOrder,
            int trunkTwistRidgeCount = 0)
        {
            switch (branchOrder)
            {
                case 0:
                    return Mathf.Clamp(
                        Mathf.Max(
                            TrunkRadialSegments,
                            Mathf.Max(3, trunkTwistRidgeCount) * 3),
                        3,
                        24);
                case 1:
                    return PrimaryRadialSegments;
                case 2:
                    return SecondaryRadialSegments;
                default:
                    return TertiaryRadialSegments;
            }
        }

        public static TreeBarkMeshSettings CreateVerticalSliceDefaults(
            TreeFamily family)
        {
            var settings = new TreeBarkMeshSettings();
            switch (family)
            {
                case TreeFamily.Twisted:
                    settings.trunkRadialSegments = 12;
                    settings.primaryRadialSegments = 9;
                    settings.secondaryRadialSegments = 7;
                    settings.tertiaryRadialSegments = 5;
                    settings.barkMetersPerTile = 2.2f;
                    settings.branchRootCollarStrength = 0.55f;
                    settings.branchRootBlendLengthInChildRadii = 2.8f;
                    break;
                case TreeFamily.Dead:
                    settings.trunkRadialSegments = 10;
                    settings.primaryRadialSegments = 8;
                    settings.secondaryRadialSegments = 6;
                    settings.tertiaryRadialSegments = 5;
                    settings.barkMetersPerTile = 2f;
                    settings.branchRootCollarStrength = 0.52f;
                    settings.branchRootBlendLengthInChildRadii = 2.6f;
                    break;
                case TreeFamily.Pine:
                    settings.trunkRadialSegments = 10;
                    settings.primaryRadialSegments = 7;
                    settings.secondaryRadialSegments = 5;
                    settings.tertiaryRadialSegments = 4;
                    settings.barkMetersPerTile = 1.7f;
                    settings.branchRootCollarStrength = 0.38f;
                    settings.branchRootBlendLengthInChildRadii = 2.1f;
                    break;
                default:
                    settings.trunkRadialSegments = 10;
                    settings.primaryRadialSegments = 8;
                    settings.secondaryRadialSegments = 6;
                    settings.tertiaryRadialSegments = 5;
                    settings.barkMetersPerTile = 1.8f;
                    break;
            }

            return settings;
        }
    }
}
