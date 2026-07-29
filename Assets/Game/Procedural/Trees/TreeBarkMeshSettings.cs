using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    public enum TreeBarkMeshEfficiencyPolicy
    {
        Current = 0,
        Conservative = 1,
        Aggressive = 2,
        LegacyCurrent = 3,
        AxialAggressive = 4,
        RadialConservative = 5,
        RadialAggressive = 6
    }

    [Serializable]
    public sealed class TreeBarkMeshSettings
    {
        public const int CurrentSettingsVersion = 10;

        [SerializeField, HideInInspector]
        private int settingsVersion = CurrentSettingsVersion;

        [SerializeField, Range(3, 64)]
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

        [NonSerialized]
        private TreeBarkMeshEfficiencyPolicy efficiencyPolicy =
            TreeBarkMeshEfficiencyPolicy.Current;

        [NonSerialized]
        private bool geometryAuditTelemetryEnabled;

        public int SettingsVersion => settingsVersion;
        public int TrunkRadialSegments => Mathf.Clamp(trunkRadialSegments, 3, 64);
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
        public TreeBarkMeshEfficiencyPolicy EfficiencyPolicy =>
            efficiencyPolicy;
        internal bool GeometryAuditTelemetryEnabled =>
            geometryAuditTelemetryEnabled;

        public int ResolveRadialSegments(
            int branchOrder,
            int rootButtressCount = 0)
        {
            if (efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.Current ||
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.LegacyCurrent ||
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.AxialAggressive ||
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.RadialConservative ||
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.RadialAggressive)
            {
                switch (branchOrder)
                {
                    case 0:
                        return Mathf.Clamp(
                            Mathf.Max(
                                TrunkRadialSegments,
                                Mathf.Max(3, rootButtressCount) * 10),
                            3,
                            64);
                    case 1:
                        return PrimaryRadialSegments;
                    case 2:
                        return SecondaryRadialSegments;
                    default:
                        return TertiaryRadialSegments;
                }
            }

            if (efficiencyPolicy ==
                TreeBarkMeshEfficiencyPolicy.Conservative)
            {
                switch (branchOrder)
                {
                    case 0:
                        return Mathf.Clamp(
                            Mathf.Max(
                                TrunkRadialSegments,
                                Mathf.Max(3, rootButtressCount) * 6),
                            3,
                            48);
                    case 1:
                        return Mathf.Min(PrimaryRadialSegments, 7);
                    case 2:
                        return Mathf.Min(SecondaryRadialSegments, 5);
                    default:
                        return Mathf.Min(TertiaryRadialSegments, 4);
                }
            }

            switch (branchOrder)
            {
                case 0:
                    return Mathf.Clamp(
                        Mathf.Max(
                            8,
                            Mathf.Max(3, rootButtressCount) * 4),
                        3,
                        32);
                case 1:
                    return Mathf.Min(PrimaryRadialSegments, 6);
                case 2:
                    return Mathf.Min(SecondaryRadialSegments, 4);
                default:
                    return 3;
            }
        }

        internal bool UsesContourOwnedTrunkRadialResolution =>
            efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.Current ||
            efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.RadialConservative ||
            efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.RadialAggressive;

        internal bool UsesRadiusAwareBranchRadialResolution =>
            UsesContourOwnedTrunkRadialResolution;

        internal bool UsesAggressiveRadialResolution =>
            efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.RadialAggressive;

        internal int ResolveLobedTrunkSamplesPerLobe(
            float effectiveLobeAmplitude)
        {
            float amplitude = Mathf.Max(0f, effectiveLobeAmplitude);
            if (UsesAggressiveRadialResolution)
            {
                if (amplitude >= 0.18f)
                {
                    return 5;
                }
                if (amplitude >= 0.065f)
                {
                    return 4;
                }
                return 3;
            }

            if (amplitude >= 0.18f)
            {
                return 6;
            }
            if (amplitude >= 0.10f)
            {
                return 5;
            }
            if (amplitude >= 0.035f)
            {
                return 4;
            }
            return 3;
        }

        internal float ResolveCircularTrunkLobeReleaseThreshold()
        {
            return UsesAggressiveRadialResolution ? 0.010f : 0.006f;
        }

        internal int ResolveCircularTrunkRadialSegments(
            float radius,
            float trunkBaseRadius)
        {
            float ratio = Mathf.Max(0f, radius) /
                Mathf.Max(0.0001f, trunkBaseRadius);
            int resolved;
            if (UsesAggressiveRadialResolution)
            {
                resolved = ratio >= 0.55f
                    ? 10
                    : ratio >= 0.24f
                        ? 8
                        : 6;
            }
            else
            {
                resolved = ratio >= 0.55f
                    ? 12
                    : ratio >= 0.24f
                        ? 10
                        : 8;
            }

            return Mathf.Clamp(
                Mathf.Min(TrunkRadialSegments + 2, resolved),
                6,
                16);
        }

        internal int ResolveRadiusAwareBranchRadialSegments(
            int branchOrder,
            float maximumRadius,
            float trunkBaseRadius,
            int authoredSegments)
        {
            int safeAuthored = Mathf.Max(3, authoredSegments);
            float ratio = Mathf.Max(0f, maximumRadius) /
                Mathf.Max(0.0001f, trunkBaseRadius);
            int resolved;
            if (UsesAggressiveRadialResolution)
            {
                switch (branchOrder)
                {
                    case 1:
                        resolved = ratio >= 0.28f ? 6 :
                            ratio >= 0.14f ? 5 : 4;
                        break;
                    case 2:
                        resolved = ratio >= 0.14f ? 4 : 3;
                        break;
                    default:
                        resolved = 3;
                        break;
                }
            }
            else
            {
                switch (branchOrder)
                {
                    case 1:
                        resolved = ratio >= 0.28f ? 7 :
                            ratio >= 0.14f ? 6 : 5;
                        break;
                    case 2:
                        resolved = ratio >= 0.14f ? 5 : 4;
                        break;
                    default:
                        resolved = ratio >= 0.08f ? 4 : 3;
                        break;
                }
            }

            return Mathf.Clamp(Mathf.Min(safeAuthored, resolved), 3, safeAuthored);
        }

        internal int ResolveRootCollapseIntervals()
        {
            switch (efficiencyPolicy)
            {
                case TreeBarkMeshEfficiencyPolicy.Aggressive:
                case TreeBarkMeshEfficiencyPolicy.AxialAggressive:
                    return 14;
                default:
                    return 24;
            }
        }

        internal float ResolveMaximumTrunkTwistStepDegrees()
        {
            switch (efficiencyPolicy)
            {
                case TreeBarkMeshEfficiencyPolicy.Conservative:
                    return 12f;
                case TreeBarkMeshEfficiencyPolicy.Aggressive:
                case TreeBarkMeshEfficiencyPolicy.AxialAggressive:
                    return 16f;
                default:
                    return 10f;
            }
        }

        internal float ResolveMaximumTrunkTangentStepDegrees()
        {
            switch (efficiencyPolicy)
            {
                case TreeBarkMeshEfficiencyPolicy.Aggressive:
                case TreeBarkMeshEfficiencyPolicy.AxialAggressive:
                    return 16f;
                case TreeBarkMeshEfficiencyPolicy.Conservative:
                    return 12f;
                default:
                    return 10f;
            }
        }

        internal float ResolveMaximumTrunkRadiusChangeRatio()
        {
            switch (efficiencyPolicy)
            {
                case TreeBarkMeshEfficiencyPolicy.Aggressive:
                case TreeBarkMeshEfficiencyPolicy.AxialAggressive:
                    return 0.18f;
                case TreeBarkMeshEfficiencyPolicy.Conservative:
                    return 0.14f;
                default:
                    return 0.10f;
            }
        }

        internal float ResolveMaximumRootEnvelopeStep()
        {
            switch (efficiencyPolicy)
            {
                case TreeBarkMeshEfficiencyPolicy.Aggressive:
                case TreeBarkMeshEfficiencyPolicy.AxialAggressive:
                    return 0.12f;
                case TreeBarkMeshEfficiencyPolicy.Conservative:
                    return 0.09f;
                default:
                    return 0.075f;
            }
        }

        internal int ResolveMinimumBranchRenderRings(int branchOrder)
        {
            bool aggressive =
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.Aggressive ||
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.AxialAggressive;
            if (aggressive)
            {
                switch (branchOrder)
                {
                    case 1:
                        return 8;
                    case 2:
                        return 6;
                    default:
                        return 5;
                }
            }

            if (efficiencyPolicy ==
                TreeBarkMeshEfficiencyPolicy.Conservative)
            {
                switch (branchOrder)
                {
                    case 1:
                        return 10;
                    case 2:
                        return 8;
                    default:
                        return 6;
                }
            }

            switch (branchOrder)
            {
                case 1:
                    return 12;
                case 2:
                    return 10;
                default:
                    return 8;
            }
        }

        internal float ResolveBranchPositionErrorInRadii(int branchOrder)
        {
            bool aggressive =
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.Aggressive ||
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.AxialAggressive;
            if (aggressive)
            {
                switch (branchOrder)
                {
                    case 1:
                        return 0.45f;
                    case 2:
                        return 0.60f;
                    default:
                        return 0.80f;
                }
            }

            if (efficiencyPolicy ==
                TreeBarkMeshEfficiencyPolicy.Conservative)
            {
                switch (branchOrder)
                {
                    case 1:
                        return 0.28f;
                    case 2:
                        return 0.35f;
                    default:
                        return 0.45f;
                }
            }

            switch (branchOrder)
            {
                case 1:
                    return 0.20f;
                case 2:
                    return 0.24f;
                default:
                    return 0.30f;
            }
        }

        internal float ResolveBranchRadiusErrorRatio(int branchOrder)
        {
            bool aggressive =
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.Aggressive ||
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.AxialAggressive;
            if (aggressive)
            {
                switch (branchOrder)
                {
                    case 1:
                        return 0.12f;
                    case 2:
                        return 0.16f;
                    default:
                        return 0.20f;
                }
            }

            if (efficiencyPolicy ==
                TreeBarkMeshEfficiencyPolicy.Conservative)
            {
                switch (branchOrder)
                {
                    case 1:
                        return 0.08f;
                    case 2:
                        return 0.10f;
                    default:
                        return 0.12f;
                }
            }

            switch (branchOrder)
            {
                case 1:
                    return 0.05f;
                case 2:
                    return 0.06f;
                default:
                    return 0.08f;
            }
        }

        internal float ResolveBranchTangentErrorDegrees(int branchOrder)
        {
            bool aggressive =
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.Aggressive ||
                efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.AxialAggressive;
            if (aggressive)
            {
                switch (branchOrder)
                {
                    case 1:
                        return 12f;
                    case 2:
                        return 15f;
                    default:
                        return 18f;
                }
            }

            if (efficiencyPolicy ==
                TreeBarkMeshEfficiencyPolicy.Conservative)
            {
                switch (branchOrder)
                {
                    case 1:
                        return 8f;
                    case 2:
                        return 10f;
                    default:
                        return 12f;
                }
            }

            switch (branchOrder)
            {
                case 1:
                    return 6f;
                case 2:
                    return 8f;
                default:
                    return 10f;
            }
        }

        internal bool UsesLegacyAxialSampling =>
            efficiencyPolicy == TreeBarkMeshEfficiencyPolicy.LegacyCurrent;

        internal bool UsesAdaptiveCircularBranchSampling =>
            !UsesLegacyAxialSampling;

        internal bool ContainsDenseRootSamplingToCollapseDomain =>
            !UsesLegacyAxialSampling;

        public static TreeBarkMeshSettings CreateRecipeOnlyDefaults()
        {
            return new TreeBarkMeshSettings
            {
                trunkRadialSegments = 10,
                primaryRadialSegments = 8,
                secondaryRadialSegments = 6,
                tertiaryRadialSegments = 5,
                barkMetersPerTile = 1.9f,
                minimumRenderedRadius = 0.006f,
                capTrunkBase = true,
                capBranchTips = true,
                branchRootInsetRatio = 0.18f,
                branchRootBlendLengthInChildRadii = 2.5f,
                branchRootRadiusScale = 1.2f,
                branchRootCollarStrength = 0.48f,
                branchRootTransitionRingCount = 3
            };
        }


        public static TreeBarkMeshSettings CreateEfficiencyAuditDefaults(
            TreeFamily family,
            bool recipeOnly,
            TreeBarkMeshEfficiencyPolicy policy)
        {
            TreeBarkMeshSettings settings = recipeOnly
                ? CreateRecipeOnlyDefaults()
                : CreateVerticalSliceDefaults(family);
            settings.efficiencyPolicy = policy;
            settings.geometryAuditTelemetryEnabled = true;
            return settings;
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
