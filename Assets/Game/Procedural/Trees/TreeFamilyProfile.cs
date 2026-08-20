using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public sealed class TreeOverallFormSettings
    {
        [SerializeField]
        private TreeFloatRange height = new TreeFloatRange(7f, 10f);

        [SerializeField]
        private TreeFloatRange trunkBaseRadius = new TreeFloatRange(0.25f, 0.55f);

        [SerializeField]
        private TreeFloatRange crownStartHeight = new TreeFloatRange(0.25f, 0.42f);

        [SerializeField]
        private TreeFloatRange crownVolume = new TreeFloatRange(0.9f, 1.25f);

        [SerializeField]
        private TreeFloatRange crownWidthScale = new TreeFloatRange(0.85f, 1.25f);

        [SerializeField]
        private TreeFloatRange crownHeightScale = new TreeFloatRange(0.9f, 1.15f);

        [SerializeField]
        private TreeFloatRange crownFill = new TreeFloatRange(0.55f, 0.85f);

        [SerializeField]
        private TreeFloatRange crownAsymmetry = new TreeFloatRange(0.05f, 0.28f);

        [SerializeField]
        private TreeIntRange crownLobeCount = new TreeIntRange(4, 8);

        [SerializeField]
        private TreeFloatRange crownLobeRadius = new TreeFloatRange(0.75f, 1.25f);

        public TreeFloatRange Height => height;
        public TreeFloatRange TrunkBaseRadius => trunkBaseRadius;
        public TreeFloatRange CrownStartHeight => crownStartHeight;
        public TreeFloatRange CrownVolume => crownVolume;
        public TreeFloatRange CrownWidthScale => crownWidthScale;
        public TreeFloatRange CrownHeightScale => crownHeightScale;
        public TreeFloatRange CrownFill => crownFill;
        public TreeFloatRange CrownAsymmetry => crownAsymmetry;
        public TreeIntRange CrownLobeCount => crownLobeCount;
        public TreeFloatRange CrownLobeRadius => crownLobeRadius;

        internal void Set(
            TreeFloatRange heightRange,
            TreeFloatRange radiusRange,
            TreeFloatRange crownStartRange,
            TreeFloatRange volumeRange,
            TreeFloatRange widthRange,
            TreeFloatRange heightScaleRange,
            TreeFloatRange fillRange,
            TreeFloatRange asymmetryRange,
            TreeIntRange lobeCountRange,
            TreeFloatRange lobeRadiusRange)
        {
            height = heightRange;
            trunkBaseRadius = radiusRange;
            crownStartHeight = crownStartRange;
            crownVolume = volumeRange;
            crownWidthScale = widthRange;
            crownHeightScale = heightScaleRange;
            crownFill = fillRange;
            crownAsymmetry = asymmetryRange;
            crownLobeCount = lobeCountRange;
            crownLobeRadius = lobeRadiusRange;
        }
    }

    [Serializable]
    public sealed class TreeTrunkSettings
    {
        [SerializeField]
        private TreeIntRange controlPointCount = new TreeIntRange(5, 8);

        [SerializeField]
        private TreeFloatRange curvature = new TreeFloatRange(0.08f, 0.28f);

        [SerializeField]
        private TreeFloatRange bendCount = new TreeFloatRange(1f, 2.5f);

        [SerializeField]
        private TreeFloatRange directionalDrift = new TreeFloatRange(0.02f, 0.14f);

        [SerializeField]
        private TreeFloatRange leanStrength = new TreeFloatRange(0f, 0.12f);

        [FormerlySerializedAs("twistDegrees")]
        [InspectorName("Trunk Twist Degrees")]
        [SerializeField]
        private TreeFloatRange surfaceTorsionDegrees = new TreeFloatRange(-25f, 25f);

        [Header("Root Buttress")]
        [InspectorName("Root Buttress Count")]
        [SerializeField]
        private TreeIntRange rootButtressCount = new TreeIntRange(4, 6);

        [SerializeField]
        private TreeFloatRange rootButtressStrength = new TreeFloatRange(0.3f, 0.6f);

        [SerializeField]
        private TreeFloatRange rootButtressHeight = new TreeFloatRange(0.12f, 0.22f);

        [SerializeField]
        private TreeFloatRange rootFlareScale = new TreeFloatRange(1.15f, 1.35f);

        [Header("Trunk Path Spiral")]
        [InspectorName("Trunk Path Spiral Strength")]
        [SerializeField]
        private TreeFloatRange spiralStrength = new TreeFloatRange(0f, 0.04f);

        [InspectorName("Trunk Path Spiral Turns")]
        [SerializeField]
        private TreeFloatRange spiralTurns = new TreeFloatRange(0.5f, 1.25f);

        [InspectorName("Trunk Path Spiral Direction")]
        [SerializeField]
        private TreeFloatRange spiralDirection = new TreeFloatRange(-1f, 1f);

        [SerializeField]
        private TreeFloatRange irregularity = new TreeFloatRange(0.04f, 0.18f);

        [SerializeField]
        private TreeFloatRange taper = new TreeFloatRange(0.72f, 0.9f);

        [SerializeField]
        private TreeFloatRange forkProbability = new TreeFloatRange(0f, 0.18f);

        [SerializeField]
        private TreeFloatRange forkHeight = new TreeFloatRange(0.55f, 0.82f);

        public TreeIntRange ControlPointCount => controlPointCount;
        public TreeFloatRange Curvature => curvature;
        public TreeFloatRange BendCount => bendCount;
        public TreeFloatRange DirectionalDrift => directionalDrift;
        public TreeFloatRange LeanStrength => leanStrength;
        public TreeFloatRange TwistDegrees => surfaceTorsionDegrees;
        public TreeFloatRange SurfaceTorsionDegrees => surfaceTorsionDegrees;
        public TreeIntRange RootButtressCount => rootButtressCount;
        public TreeFloatRange RootButtressStrength => rootButtressStrength;
        public TreeFloatRange RootButtressHeight => rootButtressHeight;
        public TreeFloatRange RootFlareScale => rootFlareScale;
        public TreeFloatRange SpiralStrength => spiralStrength;
        public TreeFloatRange SpiralTurns => spiralTurns;
        public TreeFloatRange SpiralDirection => spiralDirection;
        public TreeFloatRange Irregularity => irregularity;
        public TreeFloatRange Taper => taper;
        public TreeFloatRange ForkProbability => forkProbability;
        public TreeFloatRange ForkHeight => forkHeight;

        internal void ApplyTreeGen2BDefaults(TreeFamily family)
        {
            switch (family)
            {
                case TreeFamily.Pine:
                    spiralStrength = new TreeFloatRange(0f, 0.025f);
                    spiralTurns = new TreeFloatRange(0.35f, 0.9f);
                    spiralDirection = new TreeFloatRange(-1f, 1f);
                    break;
                case TreeFamily.Twisted:
                    spiralStrength = new TreeFloatRange(0.08f, 0.24f);
                    spiralTurns = new TreeFloatRange(0.8f, 2.25f);
                    spiralDirection = new TreeFloatRange(-1f, 1f);
                    break;
                case TreeFamily.Dead:
                    spiralStrength = new TreeFloatRange(0.04f, 0.18f);
                    spiralTurns = new TreeFloatRange(0.65f, 1.8f);
                    spiralDirection = new TreeFloatRange(-1f, 1f);
                    break;
                default:
                    spiralStrength = new TreeFloatRange(0f, 0.055f);
                    spiralTurns = new TreeFloatRange(0.4f, 1.2f);
                    spiralDirection = new TreeFloatRange(-1f, 1f);
                    break;
            }
        }

        internal void ApplyTreeGen2CDefaults(TreeFamily family)
        {
            switch (family)
            {
                case TreeFamily.Pine:
                    rootButtressStrength = new TreeFloatRange(0.12f, 0.30f);
                    rootButtressHeight = new TreeFloatRange(0.08f, 0.16f);
                    rootFlareScale = new TreeFloatRange(1.05f, 1.18f);
                    break;
                case TreeFamily.Twisted:
                    rootButtressStrength = new TreeFloatRange(0.45f, 0.80f);
                    rootButtressHeight = new TreeFloatRange(0.18f, 0.35f);
                    rootFlareScale = new TreeFloatRange(1.20f, 1.55f);
                    break;
                case TreeFamily.Dead:
                    rootButtressStrength = new TreeFloatRange(0.40f, 0.75f);
                    rootButtressHeight = new TreeFloatRange(0.16f, 0.32f);
                    rootFlareScale = new TreeFloatRange(1.18f, 1.48f);
                    break;
                default:
                    rootButtressStrength = new TreeFloatRange(0.30f, 0.65f);
                    rootButtressHeight = new TreeFloatRange(0.12f, 0.25f);
                    rootFlareScale = new TreeFloatRange(1.15f, 1.40f);
                    break;
            }
        }

        internal void ApplyTreeGen2C1Defaults(TreeFamily family)
        {
            switch (family)
            {
                case TreeFamily.Pine:
                    surfaceTorsionDegrees = new TreeFloatRange(-10f, 10f);
                    break;
                case TreeFamily.Twisted:
                    surfaceTorsionDegrees = new TreeFloatRange(-240f, -160f);
                    break;
                case TreeFamily.Dead:
                    surfaceTorsionDegrees = new TreeFloatRange(-180f, -100f);
                    break;
                default:
                    surfaceTorsionDegrees = new TreeFloatRange(-20f, 20f);
                    break;
            }
        }

        internal bool ApplyTreeGen2C1Migration(TreeFamily family)
        {
            if ((family == TreeFamily.Twisted || family == TreeFamily.Dead) &&
                ApproximatelyRange(surfaceTorsionDegrees, -110f, 110f))
            {
                ApplyTreeGen2C1Defaults(family);
                return true;
            }

            return false;
        }

        internal void ApplyTreeGen2C2Defaults(TreeFamily family)
        {
            switch (family)
            {
                case TreeFamily.Twisted:
                    surfaceTorsionDegrees =
                        new TreeFloatRange(-420f, -280f);
                    break;
                case TreeFamily.Dead:
                    surfaceTorsionDegrees =
                        new TreeFloatRange(-330f, -210f);
                    break;
            }
        }

        internal bool ApplyTreeGen2C2Migration(TreeFamily family)
        {
            if (family == TreeFamily.Twisted &&
                ApproximatelyRange(surfaceTorsionDegrees, -240f, -160f))
            {
                ApplyTreeGen2C2Defaults(family);
                return true;
            }

            if (family == TreeFamily.Dead &&
                ApproximatelyRange(surfaceTorsionDegrees, -180f, -100f))
            {
                ApplyTreeGen2C2Defaults(family);
                return true;
            }

            return false;
        }

        internal void ApplyTreeGen2C3Defaults(TreeFamily family)
        {
            switch (family)
            {
                case TreeFamily.Pine:
                    rootButtressCount = new TreeIntRange(3, 5);
                    break;
                case TreeFamily.Twisted:
                    rootButtressCount = new TreeIntRange(4, 6);
                    rootButtressStrength = new TreeFloatRange(0.45f, 1.00f);
                    rootFlareScale = new TreeFloatRange(1.20f, 1.70f);
                    break;
                case TreeFamily.Dead:
                    rootButtressCount = new TreeIntRange(4, 6);
                    rootButtressStrength = new TreeFloatRange(0.40f, 0.95f);
                    rootFlareScale = new TreeFloatRange(1.18f, 1.68f);
                    break;
                default:
                    rootButtressCount = new TreeIntRange(4, 6);
                    rootButtressStrength = new TreeFloatRange(0.30f, 0.85f);
                    rootFlareScale = new TreeFloatRange(1.15f, 1.55f);
                    break;
            }
        }

        internal bool ApplyTreeGen2C3Migration(TreeFamily family)
        {
            rootButtressCount = family == TreeFamily.Pine
                ? new TreeIntRange(3, 5)
                : new TreeIntRange(4, 6);
            bool changed = true;
            if (family == TreeFamily.Twisted)
            {
                changed |= ReplaceIfMatches(
                    ref rootButtressStrength,
                    new TreeFloatRange(0.45f, 0.80f),
                    new TreeFloatRange(0.45f, 1.00f));
                changed |= ReplaceIfMatches(
                    ref rootFlareScale,
                    new TreeFloatRange(1.20f, 1.55f),
                    new TreeFloatRange(1.20f, 1.70f));
            }
            else if (family == TreeFamily.Dead)
            {
                changed |= ReplaceIfMatches(
                    ref rootButtressStrength,
                    new TreeFloatRange(0.40f, 0.75f),
                    new TreeFloatRange(0.40f, 0.95f));
                changed |= ReplaceIfMatches(
                    ref rootFlareScale,
                    new TreeFloatRange(1.18f, 1.48f),
                    new TreeFloatRange(1.18f, 1.68f));
            }
            else if (family == TreeFamily.Common)
            {
                changed |= ReplaceIfMatches(
                    ref rootButtressStrength,
                    new TreeFloatRange(0.30f, 0.65f),
                    new TreeFloatRange(0.30f, 0.85f));
                changed |= ReplaceIfMatches(
                    ref rootFlareScale,
                    new TreeFloatRange(1.15f, 1.40f),
                    new TreeFloatRange(1.15f, 1.55f));
            }

            return changed;
        }

        internal void ApplyTreeGen2C5Defaults(TreeFamily family)
        {
            if (family == TreeFamily.Twisted)
            {
                rootButtressStrength = new TreeFloatRange(0.38f, 0.82f);
                rootFlareScale = new TreeFloatRange(1.18f, 1.52f);
            }
            else if (family == TreeFamily.Dead)
            {
                rootButtressStrength = new TreeFloatRange(0.36f, 0.80f);
                rootFlareScale = new TreeFloatRange(1.16f, 1.48f);
            }
            else if (family == TreeFamily.Common)
            {
                rootButtressStrength = new TreeFloatRange(0.25f, 0.70f);
                rootFlareScale = new TreeFloatRange(1.12f, 1.42f);
            }
        }

        internal bool ApplyTreeGen2C5Migration(TreeFamily family)
        {
            bool changed = false;
            if (family == TreeFamily.Twisted)
            {
                changed |= ReplaceIfMatches(
                    ref rootButtressStrength,
                    new TreeFloatRange(0.45f, 1.00f),
                    new TreeFloatRange(0.38f, 0.82f));
                changed |= ReplaceIfMatches(
                    ref rootFlareScale,
                    new TreeFloatRange(1.20f, 1.70f),
                    new TreeFloatRange(1.18f, 1.52f));
            }
            else if (family == TreeFamily.Dead)
            {
                changed |= ReplaceIfMatches(
                    ref rootButtressStrength,
                    new TreeFloatRange(0.40f, 0.95f),
                    new TreeFloatRange(0.36f, 0.80f));
                changed |= ReplaceIfMatches(
                    ref rootFlareScale,
                    new TreeFloatRange(1.18f, 1.68f),
                    new TreeFloatRange(1.16f, 1.48f));
            }
            else if (family == TreeFamily.Common)
            {
                changed |= ReplaceIfMatches(
                    ref rootButtressStrength,
                    new TreeFloatRange(0.30f, 0.85f),
                    new TreeFloatRange(0.25f, 0.70f));
                changed |= ReplaceIfMatches(
                    ref rootFlareScale,
                    new TreeFloatRange(1.15f, 1.55f),
                    new TreeFloatRange(1.12f, 1.42f));
            }

            return changed;
        }

        internal void ApplyTreeGen2C6Defaults(TreeFamily family)
        {
            if (family == TreeFamily.Twisted)
            {
                rootButtressStrength = new TreeFloatRange(0.38f, 0.92f);
                rootFlareScale = new TreeFloatRange(1.18f, 1.55f);
            }
            else if (family == TreeFamily.Dead)
            {
                rootButtressStrength = new TreeFloatRange(0.36f, 0.88f);
                rootFlareScale = new TreeFloatRange(1.16f, 1.52f);
            }
            else if (family == TreeFamily.Common)
            {
                rootButtressStrength = new TreeFloatRange(0.25f, 0.82f);
                rootFlareScale = new TreeFloatRange(1.12f, 1.45f);
            }
        }

        internal bool ApplyTreeGen2C6Migration(TreeFamily family)
        {
            bool changed = false;
            if (family == TreeFamily.Twisted)
            {
                changed |= ReplaceIfMatches(
                    ref rootButtressStrength,
                    new TreeFloatRange(0.38f, 0.82f),
                    new TreeFloatRange(0.38f, 0.92f));
                changed |= ReplaceIfMatches(
                    ref rootFlareScale,
                    new TreeFloatRange(1.18f, 1.52f),
                    new TreeFloatRange(1.18f, 1.55f));
            }
            else if (family == TreeFamily.Dead)
            {
                changed |= ReplaceIfMatches(
                    ref rootButtressStrength,
                    new TreeFloatRange(0.36f, 0.80f),
                    new TreeFloatRange(0.36f, 0.88f));
                changed |= ReplaceIfMatches(
                    ref rootFlareScale,
                    new TreeFloatRange(1.16f, 1.48f),
                    new TreeFloatRange(1.16f, 1.52f));
            }
            else if (family == TreeFamily.Common)
            {
                changed |= ReplaceIfMatches(
                    ref rootButtressStrength,
                    new TreeFloatRange(0.25f, 0.70f),
                    new TreeFloatRange(0.25f, 0.82f));
                changed |= ReplaceIfMatches(
                    ref rootFlareScale,
                    new TreeFloatRange(1.12f, 1.42f),
                    new TreeFloatRange(1.12f, 1.45f));
            }

            return changed;
        }

        private static bool ReplaceIfMatches(
            ref TreeFloatRange value,
            TreeFloatRange expected,
            TreeFloatRange replacement)
        {
            if (Mathf.Abs(value.Minimum - expected.Minimum) > 0.0001f ||
                Mathf.Abs(value.Maximum - expected.Maximum) > 0.0001f)
            {
                return false;
            }

            value = replacement;
            return true;
        }

        private static bool ApproximatelyRange(
            TreeFloatRange range,
            float minimum,
            float maximum)
        {
            return Mathf.Abs(range.Minimum - minimum) <= 0.0001f &&
                Mathf.Abs(range.Maximum - maximum) <= 0.0001f;
        }

        internal void Set(
            TreeIntRange pointCount,
            TreeFloatRange curvatureRange,
            TreeFloatRange bendCountRange,
            TreeFloatRange driftRange,
            TreeFloatRange leanRange,
            TreeFloatRange twistRange,
            TreeFloatRange irregularityRange,
            TreeFloatRange taperRange,
            TreeFloatRange forkProbabilityRange,
            TreeFloatRange forkHeightRange)
        {
            controlPointCount = pointCount;
            curvature = curvatureRange;
            bendCount = bendCountRange;
            directionalDrift = driftRange;
            leanStrength = leanRange;
            surfaceTorsionDegrees = twistRange;
            irregularity = irregularityRange;
            taper = taperRange;
            forkProbability = forkProbabilityRange;
            forkHeight = forkHeightRange;
        }
    }

    [Serializable]
    public class TreeBranchOrderSettings
    {
        [SerializeField]
        private TreeIntRange count = new TreeIntRange(8, 12);

        [SerializeField, HideInInspector]
        private TreeFloatRange attachmentHeight = new TreeFloatRange(0.24f, 0.88f);

        [SerializeField]
        private TreeFloatRange lengthRatio = new TreeFloatRange(0.22f, 0.42f);

        [SerializeField]
        private TreeFloatRange radiusRatio = new TreeFloatRange(0.28f, 0.48f);

        [SerializeField]
        private TreeFloatRange curvature = new TreeFloatRange(0.12f, 0.34f);

        [SerializeField, HideInInspector]
        private TreeFloatRange droop = new TreeFloatRange(0f, 0.28f);

        [SerializeField, HideInInspector]
        private TreeFloatRange upwardBias = new TreeFloatRange(0.05f, 0.38f);

        [SerializeField]
        private TreeFloatRange sideSweep = new TreeFloatRange(-0.18f, 0.18f);

        [SerializeField]
        private TreeFloatRange twistDegrees = new TreeFloatRange(-35f, 35f);

        [SerializeField]
        private TreeFloatRange irregularity = new TreeFloatRange(0.05f, 0.22f);

        [SerializeField]
        private TreeFloatRange endCurl = new TreeFloatRange(-0.1f, 0.25f);

        [SerializeField]
        private TreeFloatRange minimumAttachmentSpacing = new TreeFloatRange(0.035f, 0.09f);

        [SerializeField]
        private TreeIntRange tierCount = new TreeIntRange(0, 0);

        [SerializeField]
        private TreeIntRange branchesPerTier = new TreeIntRange(0, 0);

        [SerializeField]
        private TreeFloatRange tierIrregularity = new TreeFloatRange(0f, 0.15f);

        [SerializeField, HideInInspector]
        private TreeFloatRange sideBias = new TreeFloatRange(-0.15f, 0.15f);

        public TreeIntRange Count => count;
        public TreeFloatRange AttachmentHeight => attachmentHeight;
        public TreeFloatRange LengthRatio => lengthRatio;
        public TreeFloatRange RadiusRatio => radiusRatio;
        public TreeFloatRange Curvature => curvature;
        public TreeFloatRange Droop => droop;
        public TreeFloatRange UpwardBias => upwardBias;
        public TreeFloatRange SideSweep => sideSweep;
        public TreeFloatRange TwistDegrees => twistDegrees;
        public TreeFloatRange SurfaceTorsionDegrees => twistDegrees;
        public TreeFloatRange Irregularity => irregularity;
        public TreeFloatRange EndCurl => endCurl;
        public TreeFloatRange MinimumAttachmentSpacing => minimumAttachmentSpacing;
        public TreeIntRange TierCount => tierCount;
        public TreeIntRange BranchesPerTier => branchesPerTier;
        public TreeFloatRange TierIrregularity => tierIrregularity;
        public TreeFloatRange SideBias => sideBias;

        internal void ApplyTreeGen2C3Defaults(
            TreeFamily family,
            int branchOrder)
        {
            if (branchOrder == 1 && family == TreeFamily.Twisted)
            {
                count = new TreeIntRange(7, 13);
                lengthRatio = new TreeFloatRange(0.20f, 0.46f);
                radiusRatio = new TreeFloatRange(0.34f, 0.60f);
            }
            else if (branchOrder == 2 && family == TreeFamily.Twisted)
            {
                count = new TreeIntRange(2, 3);
                lengthRatio = new TreeFloatRange(0.24f, 0.40f);
                radiusRatio = new TreeFloatRange(0.28f, 0.46f);
            }
            else if (branchOrder == 1 && family == TreeFamily.Dead)
            {
                count = new TreeIntRange(12, 20);
                lengthRatio = new TreeFloatRange(0.14f, 0.32f);
                radiusRatio = new TreeFloatRange(0.32f, 0.60f);
            }
            else if (branchOrder == 2 && family == TreeFamily.Dead)
            {
                count = new TreeIntRange(2, 4);
                lengthRatio = new TreeFloatRange(0.18f, 0.30f);
                radiusRatio = new TreeFloatRange(0.26f, 0.44f);
            }
        }

        internal bool ApplyTreeGen2C3Migration(
            TreeFamily family,
            int branchOrder)
        {
            bool changed = false;
            if (branchOrder == 1 && family == TreeFamily.Twisted)
            {
                changed |= ReplaceIfMatches(
                    ref count,
                    new TreeIntRange(5, 11),
                    new TreeIntRange(7, 13));
                changed |= ReplaceIfMatches(
                    ref lengthRatio,
                    new TreeFloatRange(0.25f, 0.55f),
                    new TreeFloatRange(0.20f, 0.46f));
                changed |= ReplaceIfMatches(
                    ref radiusRatio,
                    new TreeFloatRange(0.28f, 0.55f),
                    new TreeFloatRange(0.34f, 0.60f));
            }
            else if (branchOrder == 2 && family == TreeFamily.Twisted)
            {
                changed |= ReplaceIfMatches(
                    ref count,
                    new TreeIntRange(1, 2),
                    new TreeIntRange(2, 3));
                changed |= ReplaceIfMatches(
                    ref lengthRatio,
                    new TreeFloatRange(0.24f, 0.42f),
                    new TreeFloatRange(0.24f, 0.40f));
                changed |= ReplaceIfMatches(
                    ref radiusRatio,
                    new TreeFloatRange(0.24f, 0.42f),
                    new TreeFloatRange(0.28f, 0.46f));
            }
            else if (branchOrder == 1 && family == TreeFamily.Dead)
            {
                changed |= ReplaceIfMatches(
                    ref count,
                    new TreeIntRange(9, 17),
                    new TreeIntRange(12, 20));
                changed |= ReplaceIfMatches(
                    ref lengthRatio,
                    new TreeFloatRange(0.16f, 0.38f),
                    new TreeFloatRange(0.14f, 0.32f));
                changed |= ReplaceIfMatches(
                    ref radiusRatio,
                    new TreeFloatRange(0.25f, 0.52f),
                    new TreeFloatRange(0.32f, 0.60f));
            }
            else if (branchOrder == 2 && family == TreeFamily.Dead)
            {
                changed |= ReplaceIfMatches(
                    ref count,
                    new TreeIntRange(1, 3),
                    new TreeIntRange(2, 4));
                changed |= ReplaceIfMatches(
                    ref lengthRatio,
                    new TreeFloatRange(0.20f, 0.34f),
                    new TreeFloatRange(0.18f, 0.30f));
                changed |= ReplaceIfMatches(
                    ref radiusRatio,
                    new TreeFloatRange(0.22f, 0.38f),
                    new TreeFloatRange(0.26f, 0.44f));
            }

            return changed;
        }

        internal void ApplyTreeGen2C5Defaults(
            TreeFamily family,
            int branchOrder)
        {
            if (branchOrder == 1 && family == TreeFamily.Twisted)
            {
                count = new TreeIntRange(10, 14);
                lengthRatio = new TreeFloatRange(0.18f, 0.38f);
            }
            else if (branchOrder == 2 && family == TreeFamily.Twisted)
            {
                count = new TreeIntRange(3, 4);
                lengthRatio = new TreeFloatRange(0.20f, 0.34f);
            }
            else if (branchOrder == 1 && family == TreeFamily.Dead)
            {
                count = new TreeIntRange(14, 20);
                lengthRatio = new TreeFloatRange(0.12f, 0.26f);
            }
            else if (branchOrder == 2 && family == TreeFamily.Dead)
            {
                count = new TreeIntRange(3, 4);
                lengthRatio = new TreeFloatRange(0.16f, 0.26f);
            }
        }

        internal bool ApplyTreeGen2C5Migration(
            TreeFamily family,
            int branchOrder)
        {
            bool changed = false;
            if (branchOrder == 1 && family == TreeFamily.Twisted)
            {
                changed |= ReplaceIfMatches(
                    ref count,
                    new TreeIntRange(7, 13),
                    new TreeIntRange(10, 14));
                changed |= ReplaceIfMatches(
                    ref lengthRatio,
                    new TreeFloatRange(0.20f, 0.46f),
                    new TreeFloatRange(0.18f, 0.38f));
            }
            else if (branchOrder == 2 && family == TreeFamily.Twisted)
            {
                changed |= ReplaceIfMatches(
                    ref count,
                    new TreeIntRange(2, 3),
                    new TreeIntRange(3, 4));
                changed |= ReplaceIfMatches(
                    ref lengthRatio,
                    new TreeFloatRange(0.24f, 0.40f),
                    new TreeFloatRange(0.20f, 0.34f));
            }
            else if (branchOrder == 1 && family == TreeFamily.Dead)
            {
                changed |= ReplaceIfMatches(
                    ref count,
                    new TreeIntRange(12, 20),
                    new TreeIntRange(14, 20));
                changed |= ReplaceIfMatches(
                    ref lengthRatio,
                    new TreeFloatRange(0.14f, 0.32f),
                    new TreeFloatRange(0.12f, 0.26f));
            }
            else if (branchOrder == 2 && family == TreeFamily.Dead)
            {
                changed |= ReplaceIfMatches(
                    ref count,
                    new TreeIntRange(2, 4),
                    new TreeIntRange(3, 4));
                changed |= ReplaceIfMatches(
                    ref lengthRatio,
                    new TreeFloatRange(0.18f, 0.30f),
                    new TreeFloatRange(0.16f, 0.26f));
            }

            return changed;
        }

        private static bool ReplaceIfMatches(
            ref TreeFloatRange value,
            TreeFloatRange expected,
            TreeFloatRange replacement)
        {
            if (Mathf.Abs(value.Minimum - expected.Minimum) > 0.0001f ||
                Mathf.Abs(value.Maximum - expected.Maximum) > 0.0001f)
            {
                return false;
            }

            value = replacement;
            return true;
        }

        private static bool ReplaceIfMatches(
            ref TreeIntRange value,
            TreeIntRange expected,
            TreeIntRange replacement)
        {
            if (value.Minimum != expected.Minimum ||
                value.Maximum != expected.Maximum)
            {
                return false;
            }

            value = replacement;
            return true;
        }

        internal void Set(
            TreeIntRange countRange,
            TreeFloatRange attachmentRange,
            TreeFloatRange lengthRange,
            TreeFloatRange radiusRange,
            TreeFloatRange curvatureRange,
            TreeFloatRange droopRange,
            TreeFloatRange upwardRange,
            TreeFloatRange sweepRange,
            TreeFloatRange twistRange,
            TreeFloatRange irregularityRange,
            TreeFloatRange curlRange,
            TreeFloatRange spacingRange,
            TreeIntRange tierCountRange,
            TreeIntRange perTierRange,
            TreeFloatRange tierIrregularityRange,
            TreeFloatRange sideBiasRange)
        {
            count = countRange;
            attachmentHeight = attachmentRange;
            lengthRatio = lengthRange;
            radiusRatio = radiusRange;
            curvature = curvatureRange;
            droop = droopRange;
            upwardBias = upwardRange;
            sideSweep = sweepRange;
            twistDegrees = twistRange;
            irregularity = irregularityRange;
            endCurl = curlRange;
            minimumAttachmentSpacing = spacingRange;
            tierCount = tierCountRange;
            branchesPerTier = perTierRange;
            tierIrregularity = tierIrregularityRange;
            sideBias = sideBiasRange;
        }
    }

    [Serializable]
    public sealed class TreePrimaryBranchSettings : TreeBranchOrderSettings
    {
        [SerializeField]
        private TreeFloatRange startHeight = new TreeFloatRange(0.22f, 0.3f);

        [SerializeField]
        private TreeFloatRange endHeight = new TreeFloatRange(0.82f, 0.92f);

        [SerializeField]
        private TreeFloatRange initialElevationDegrees = new TreeFloatRange(8f, 28f);

        [SerializeField]
        private TreeFloatRange archDirection = new TreeFloatRange(0.15f, 0.8f);

        [SerializeField]
        private TreeFloatRange archStrength = new TreeFloatRange(0.04f, 0.18f);

        [SerializeField]
        private TreeFloatRange lateSag = new TreeFloatRange(0f, 0.15f);

        [SerializeField]
        private TreeFloatRange azimuthSymmetry = new TreeFloatRange(0.55f, 0.9f);

        [SerializeField]
        private TreeFloatRange directionalBiasAngleDegrees = new TreeFloatRange(0f, 360f);

        [SerializeField]
        private TreeFloatRange directionalBiasStrength = new TreeFloatRange(0f, 0.2f);

        public TreeFloatRange StartHeight => startHeight;
        public TreeFloatRange EndHeight => endHeight;
        public TreeFloatRange InitialElevationDegrees => initialElevationDegrees;
        public TreeFloatRange ArchDirection => archDirection;
        public TreeFloatRange ArchStrength => archStrength;
        public TreeFloatRange LateSag => lateSag;
        public TreeFloatRange AzimuthSymmetry => azimuthSymmetry;
        public TreeFloatRange DirectionalBiasAngleDegrees => directionalBiasAngleDegrees;
        public TreeFloatRange DirectionalBiasStrength => directionalBiasStrength;

        internal void ApplyTreeGen2BDefaults(TreeFamily family)
        {
            switch (family)
            {
                case TreeFamily.Pine:
                    startHeight = new TreeFloatRange(0.18f, 0.28f);
                    endHeight = new TreeFloatRange(0.78f, 0.94f);
                    initialElevationDegrees = new TreeFloatRange(-8f, 14f);
                    archDirection = new TreeFloatRange(-0.8f, 0.1f);
                    archStrength = new TreeFloatRange(0.08f, 0.28f);
                    lateSag = new TreeFloatRange(0.14f, 0.38f);
                    azimuthSymmetry = new TreeFloatRange(0.82f, 1f);
                    directionalBiasAngleDegrees = new TreeFloatRange(0f, 360f);
                    directionalBiasStrength = new TreeFloatRange(0f, 0.12f);
                    break;
                case TreeFamily.Twisted:
                    startHeight = new TreeFloatRange(0.2f, 0.36f);
                    endHeight = new TreeFloatRange(0.72f, 0.94f);
                    initialElevationDegrees = new TreeFloatRange(-12f, 24f);
                    archDirection = new TreeFloatRange(-1f, 1f);
                    archStrength = new TreeFloatRange(0.16f, 0.46f);
                    lateSag = new TreeFloatRange(0.04f, 0.3f);
                    azimuthSymmetry = new TreeFloatRange(0.1f, 0.52f);
                    directionalBiasAngleDegrees = new TreeFloatRange(0f, 360f);
                    directionalBiasStrength = new TreeFloatRange(0.3f, 0.82f);
                    break;
                case TreeFamily.Dead:
                    startHeight = new TreeFloatRange(0.18f, 0.38f);
                    endHeight = new TreeFloatRange(0.68f, 0.94f);
                    initialElevationDegrees = new TreeFloatRange(-18f, 28f);
                    archDirection = new TreeFloatRange(-1f, 0.8f);
                    archStrength = new TreeFloatRange(0.12f, 0.42f);
                    lateSag = new TreeFloatRange(0.02f, 0.28f);
                    azimuthSymmetry = new TreeFloatRange(0.15f, 0.65f);
                    directionalBiasAngleDegrees = new TreeFloatRange(0f, 360f);
                    directionalBiasStrength = new TreeFloatRange(0.18f, 0.74f);
                    break;
                default:
                    startHeight = new TreeFloatRange(0.22f, 0.32f);
                    endHeight = new TreeFloatRange(0.82f, 0.92f);
                    initialElevationDegrees = new TreeFloatRange(8f, 30f);
                    archDirection = new TreeFloatRange(0.1f, 0.85f);
                    archStrength = new TreeFloatRange(0.04f, 0.2f);
                    lateSag = new TreeFloatRange(0f, 0.16f);
                    azimuthSymmetry = new TreeFloatRange(0.55f, 0.92f);
                    directionalBiasAngleDegrees = new TreeFloatRange(0f, 360f);
                    directionalBiasStrength = new TreeFloatRange(0f, 0.22f);
                    break;
            }
        }

        internal void ApplyTreeGen2C3Defaults(TreeFamily family)
        {
            if (family == TreeFamily.Twisted)
            {
                initialElevationDegrees = new TreeFloatRange(8f, 34f);
                archDirection = new TreeFloatRange(0.15f, 1f);
                archStrength = new TreeFloatRange(0.12f, 0.36f);
                lateSag = new TreeFloatRange(0.02f, 0.18f);
            }
            else if (family == TreeFamily.Dead)
            {
                initialElevationDegrees = new TreeFloatRange(10f, 38f);
                archDirection = new TreeFloatRange(0.20f, 1f);
                archStrength = new TreeFloatRange(0.12f, 0.34f);
                lateSag = new TreeFloatRange(0f, 0.16f);
            }
        }

        internal bool ApplyTreeGen2C3Migration(TreeFamily family)
        {
            bool changed = false;
            if (family == TreeFamily.Twisted)
            {
                changed |= ReplaceIfMatches(
                    ref initialElevationDegrees,
                    new TreeFloatRange(-17.744672f, 22.293629f),
                    new TreeFloatRange(8f, 34f));
                changed |= ReplaceIfMatches(
                    ref archDirection,
                    new TreeFloatRange(-1f, 1f),
                    new TreeFloatRange(0.15f, 1f));
                changed |= ReplaceIfMatches(
                    ref archStrength,
                    new TreeFloatRange(0.16f, 0.46f),
                    new TreeFloatRange(0.12f, 0.36f));
                changed |= ReplaceIfMatches(
                    ref lateSag,
                    new TreeFloatRange(0f, 0.35f),
                    new TreeFloatRange(0.02f, 0.18f));
            }
            else if (family == TreeFamily.Dead)
            {
                changed |= ReplaceIfMatches(
                    ref initialElevationDegrees,
                    new TreeFloatRange(-15.109575f, 24.227745f),
                    new TreeFloatRange(10f, 38f));
                changed |= ReplaceIfMatches(
                    ref archDirection,
                    new TreeFloatRange(-1f, 0.8f),
                    new TreeFloatRange(0.20f, 1f));
                changed |= ReplaceIfMatches(
                    ref archStrength,
                    new TreeFloatRange(0.12f, 0.42f),
                    new TreeFloatRange(0.12f, 0.34f));
                changed |= ReplaceIfMatches(
                    ref lateSag,
                    new TreeFloatRange(0f, 0.30f),
                    new TreeFloatRange(0f, 0.16f));
            }

            return changed;
        }

        internal void ApplyTreeGen2C5Defaults(TreeFamily family)
        {
            if (family == TreeFamily.Twisted)
            {
                startHeight = new TreeFloatRange(0.24f, 0.40f);
                endHeight = new TreeFloatRange(0.74f, 0.94f);
                initialElevationDegrees = new TreeFloatRange(14f, 38f);
                azimuthSymmetry = new TreeFloatRange(0.80f, 0.96f);
                directionalBiasStrength = new TreeFloatRange(0.02f, 0.18f);
            }
            else if (family == TreeFamily.Dead)
            {
                startHeight = new TreeFloatRange(0.30f, 0.48f);
                endHeight = new TreeFloatRange(0.74f, 0.94f);
                initialElevationDegrees = new TreeFloatRange(14f, 40f);
                azimuthSymmetry = new TreeFloatRange(0.70f, 0.92f);
                directionalBiasStrength = new TreeFloatRange(0.02f, 0.20f);
            }
        }

        internal bool ApplyTreeGen2C5Migration(TreeFamily family)
        {
            bool changed = false;
            if (family == TreeFamily.Twisted)
            {
                changed |= ReplaceIfMatches(
                    ref startHeight,
                    new TreeFloatRange(0.20f, 0.36f),
                    new TreeFloatRange(0.24f, 0.40f));
                changed |= ReplaceIfMatches(
                    ref endHeight,
                    new TreeFloatRange(0.72f, 0.94f),
                    new TreeFloatRange(0.74f, 0.94f));
                changed |= ReplaceIfMatches(
                    ref initialElevationDegrees,
                    new TreeFloatRange(8f, 34f),
                    new TreeFloatRange(14f, 38f));
                changed |= ReplaceIfMatches(
                    ref azimuthSymmetry,
                    new TreeFloatRange(0.10f, 0.52f),
                    new TreeFloatRange(0.80f, 0.96f));
                changed |= ReplaceIfMatches(
                    ref directionalBiasStrength,
                    new TreeFloatRange(0.30f, 0.82f),
                    new TreeFloatRange(0.02f, 0.18f));
            }
            else if (family == TreeFamily.Dead)
            {
                changed |= ReplaceIfMatches(
                    ref startHeight,
                    new TreeFloatRange(0.18f, 0.38f),
                    new TreeFloatRange(0.30f, 0.48f));
                changed |= ReplaceIfMatches(
                    ref endHeight,
                    new TreeFloatRange(0.68f, 0.94f),
                    new TreeFloatRange(0.74f, 0.94f));
                changed |= ReplaceIfMatches(
                    ref initialElevationDegrees,
                    new TreeFloatRange(10f, 38f),
                    new TreeFloatRange(14f, 40f));
                changed |= ReplaceIfMatches(
                    ref azimuthSymmetry,
                    new TreeFloatRange(0.15f, 0.65f),
                    new TreeFloatRange(0.70f, 0.92f));
                changed |= ReplaceIfMatches(
                    ref directionalBiasStrength,
                    new TreeFloatRange(0.18f, 0.74f),
                    new TreeFloatRange(0.02f, 0.20f));
            }

            return changed;
        }

        private static bool ReplaceIfMatches(
            ref TreeFloatRange value,
            TreeFloatRange expected,
            TreeFloatRange replacement)
        {
            if (Mathf.Abs(value.Minimum - expected.Minimum) > 0.0001f ||
                Mathf.Abs(value.Maximum - expected.Maximum) > 0.0001f)
            {
                return false;
            }

            value = replacement;
            return true;
        }

        internal void ApplyTreeGen2BMigration(TreeFamily family)
        {
            ApplyTreeGen2BDefaults(family);

            TreeFloatRange orderedAttachment = AttachmentHeight.Ordered();
            float attachmentSpan = Mathf.Max(
                0f,
                orderedAttachment.Maximum - orderedAttachment.Minimum);
            float endpointBand = Mathf.Min(
                0.08f,
                attachmentSpan * 0.25f);
            float midpoint = orderedAttachment.Midpoint;
            startHeight = new TreeFloatRange(
                orderedAttachment.Minimum,
                Mathf.Min(midpoint, orderedAttachment.Minimum + endpointBand));
            endHeight = new TreeFloatRange(
                Mathf.Max(midpoint, orderedAttachment.Maximum - endpointBand),
                orderedAttachment.Maximum);

            TreeFloatRange orderedUpward = UpwardBias.Ordered();
            TreeFloatRange orderedDroop = Droop.Ordered();
            float minimumSlope =
                orderedUpward.Minimum - orderedDroop.Maximum + 0.08f;
            float maximumSlope =
                orderedUpward.Maximum - orderedDroop.Minimum + 0.08f;
            float minimumElevation =
                Mathf.Atan(minimumSlope) * Mathf.Rad2Deg;
            float maximumElevation =
                Mathf.Atan(maximumSlope) * Mathf.Rad2Deg;
            initialElevationDegrees = new TreeFloatRange(
                Mathf.Min(minimumElevation, maximumElevation),
                Mathf.Max(minimumElevation, maximumElevation));
            lateSag = new TreeFloatRange(
                Mathf.Max(0f, orderedDroop.Minimum),
                Mathf.Max(0f, orderedDroop.Maximum));

            if (family == TreeFamily.Twisted || family == TreeFamily.Dead)
            {
                TreeFloatRange orderedSideBias = SideBias.Ordered();
                float maximumBias = Mathf.Max(
                    Mathf.Abs(orderedSideBias.Minimum),
                    Mathf.Abs(orderedSideBias.Maximum));
                float minimumBias =
                    orderedSideBias.Minimum <= 0f &&
                    orderedSideBias.Maximum >= 0f
                        ? 0f
                        : Mathf.Min(
                            Mathf.Abs(orderedSideBias.Minimum),
                            Mathf.Abs(orderedSideBias.Maximum));
                directionalBiasStrength = new TreeFloatRange(
                    Mathf.Clamp01(minimumBias),
                    Mathf.Clamp01(maximumBias));
            }
        }
    }

    [Serializable]
    public sealed class TreeFoliageSettings
    {
        [SerializeField]
        private TreeFloatRange clusterWidthScale = new TreeFloatRange(0.8f, 1.3f);

        [SerializeField]
        private TreeFloatRange clusterHeightScale = new TreeFloatRange(0.8f, 1.25f);

        [SerializeField]
        private TreeFloatRange clusterLengthScale = new TreeFloatRange(0.8f, 1.35f);

        [SerializeField]
        private TreeFloatRange clusterRadialSpread = new TreeFloatRange(0.7f, 1.25f);

        [SerializeField]
        private TreeFloatRange cardSizeScale = new TreeFloatRange(0.85f, 1.25f);

        [SerializeField]
        private TreeIntRange clusterCount = new TreeIntRange(18, 42);

        [SerializeField]
        private TreeIntRange cardsPerCluster = new TreeIntRange(3, 6);

        [SerializeField]
        private TreeFloatRange eligibility = new TreeFloatRange(0.65f, 0.95f);

        [SerializeField]
        private TreeFloatRange occupancy = new TreeFloatRange(0.55f, 0.9f);

        [SerializeField]
        private TreeFloatRange terminalProbability = new TreeFloatRange(0.6f, 0.95f);

        [SerializeField]
        private TreeFloatRange retention = new TreeFloatRange(0.85f, 1f);

        public TreeFloatRange ClusterWidthScale => clusterWidthScale;
        public TreeFloatRange ClusterHeightScale => clusterHeightScale;
        public TreeFloatRange ClusterLengthScale => clusterLengthScale;
        public TreeFloatRange ClusterRadialSpread => clusterRadialSpread;
        public TreeFloatRange CardSizeScale => cardSizeScale;
        public TreeIntRange ClusterCount => clusterCount;
        public TreeIntRange CardsPerCluster => cardsPerCluster;
        public TreeFloatRange Eligibility => eligibility;
        public TreeFloatRange Occupancy => occupancy;
        public TreeFloatRange TerminalProbability => terminalProbability;
        public TreeFloatRange Retention => retention;

        internal void Set(
            TreeFloatRange widthRange,
            TreeFloatRange heightRange,
            TreeFloatRange lengthRange,
            TreeFloatRange spreadRange,
            TreeFloatRange cardScaleRange,
            TreeIntRange clusterCountRange,
            TreeIntRange cardsPerClusterRange,
            TreeFloatRange eligibilityRange,
            TreeFloatRange occupancyRange,
            TreeFloatRange terminalRange,
            TreeFloatRange retentionRange)
        {
            clusterWidthScale = widthRange;
            clusterHeightScale = heightRange;
            clusterLengthScale = lengthRange;
            clusterRadialSpread = spreadRange;
            cardSizeScale = cardScaleRange;
            clusterCount = clusterCountRange;
            cardsPerCluster = cardsPerClusterRange;
            eligibility = eligibilityRange;
            occupancy = occupancyRange;
            terminalProbability = terminalRange;
            retention = retentionRange;
        }
    }

    [Serializable]
    public sealed class TreeStructuralConstraintSettings
    {
        [SerializeField, Range(0.05f, 1f)]
        private float maximumTrunkHorizontalDisplacementRatio = 0.24f;

        [SerializeField, Range(5f, 90f)]
        private float maximumTrunkSegmentTurnDegrees = 24f;

        [SerializeField, Range(5f, 120f)]
        private float maximumBranchSegmentTurnDegrees = 30f;

        [SerializeField, Range(15f, 360f)]
        private float maximumPrimaryAccumulatedTurnDegrees = 110f;

        [SerializeField, Range(10f, 240f)]
        private float maximumHigherOrderAccumulatedTurnDegrees = 75f;

        [SerializeField, Range(1f, 3f)]
        private float maximumPrimaryArcChordRatio = 1.22f;

        [SerializeField, Range(1f, 3f)]
        private float maximumHigherOrderArcChordRatio = 1.14f;

        [SerializeField, Range(0f, 1f)]
        private float minimumForwardProgress = 0.25f;

        [SerializeField, Range(0f, 1f)]
        private float maximumParentReturnFraction = 0.08f;

        [SerializeField, Range(0f, 1f)]
        private float secondarySurvivalProbability = 0.88f;

        [SerializeField, Range(0f, 1f)]
        private float tertiarySurvivalProbability = 0.5f;

        [SerializeField, Range(0f, 0.5f)]
        private float crownEnvelopeOvershoot = 0.12f;

        public float MaximumTrunkHorizontalDisplacementRatio =>
            Mathf.Clamp(maximumTrunkHorizontalDisplacementRatio, 0.05f, 1f);
        public float MaximumTrunkSegmentTurnDegrees =>
            Mathf.Clamp(maximumTrunkSegmentTurnDegrees, 5f, 90f);
        public float MaximumBranchSegmentTurnDegrees =>
            Mathf.Clamp(maximumBranchSegmentTurnDegrees, 5f, 120f);
        public float MaximumPrimaryAccumulatedTurnDegrees =>
            Mathf.Clamp(maximumPrimaryAccumulatedTurnDegrees, 15f, 360f);
        public float MaximumHigherOrderAccumulatedTurnDegrees =>
            Mathf.Clamp(maximumHigherOrderAccumulatedTurnDegrees, 10f, 240f);
        public float MaximumPrimaryArcChordRatio =>
            Mathf.Clamp(maximumPrimaryArcChordRatio, 1f, 3f);
        public float MaximumHigherOrderArcChordRatio =>
            Mathf.Clamp(maximumHigherOrderArcChordRatio, 1f, 3f);
        public float MinimumForwardProgress =>
            Mathf.Clamp01(minimumForwardProgress);
        public float MaximumParentReturnFraction =>
            Mathf.Clamp01(maximumParentReturnFraction);
        public float SecondarySurvivalProbability =>
            Mathf.Clamp01(secondarySurvivalProbability);
        public float TertiarySurvivalProbability =>
            Mathf.Clamp01(tertiarySurvivalProbability);
        public float CrownEnvelopeOvershoot =>
            Mathf.Clamp(crownEnvelopeOvershoot, 0f, 0.5f);

        internal void ApplyTreeGen2C3Defaults(TreeFamily family)
        {
            if (family == TreeFamily.Twisted)
            {
                secondarySurvivalProbability = 0.84f;
            }
            else if (family == TreeFamily.Dead)
            {
                secondarySurvivalProbability = 0.82f;
            }
        }

        internal bool ApplyTreeGen2C3Migration(TreeFamily family)
        {
            float expected = family == TreeFamily.Twisted
                ? 0.72f
                : family == TreeFamily.Dead
                    ? 0.62f
                    : secondarySurvivalProbability;
            float replacement = family == TreeFamily.Twisted
                ? 0.84f
                : family == TreeFamily.Dead
                    ? 0.82f
                    : secondarySurvivalProbability;
            if (family != TreeFamily.Twisted && family != TreeFamily.Dead)
            {
                return false;
            }

            if (Mathf.Abs(secondarySurvivalProbability - expected) > 0.0001f)
            {
                return false;
            }

            secondarySurvivalProbability = replacement;
            return true;
        }

        internal void ApplyTreeGen2C5Defaults(TreeFamily family)
        {
            if (family == TreeFamily.Twisted || family == TreeFamily.Dead)
            {
                secondarySurvivalProbability = 0.90f;
            }
        }

        internal bool ApplyTreeGen2C5Migration(TreeFamily family)
        {
            float expected = family == TreeFamily.Twisted
                ? 0.84f
                : family == TreeFamily.Dead
                    ? 0.82f
                    : secondarySurvivalProbability;
            if (family != TreeFamily.Twisted && family != TreeFamily.Dead)
            {
                return false;
            }

            if (Mathf.Abs(secondarySurvivalProbability - expected) > 0.0001f)
            {
                return false;
            }

            secondarySurvivalProbability = 0.90f;
            return true;
        }

        internal void Set(
            float trunkDisplacementRatio,
            float trunkSegmentTurn,
            float branchSegmentTurn,
            float primaryAccumulatedTurn,
            float higherAccumulatedTurn,
            float primaryArcChord,
            float higherArcChord,
            float forwardProgress,
            float parentReturn,
            float secondarySurvival,
            float tertiarySurvival,
            float envelopeOvershoot)
        {
            maximumTrunkHorizontalDisplacementRatio = trunkDisplacementRatio;
            maximumTrunkSegmentTurnDegrees = trunkSegmentTurn;
            maximumBranchSegmentTurnDegrees = branchSegmentTurn;
            maximumPrimaryAccumulatedTurnDegrees = primaryAccumulatedTurn;
            maximumHigherOrderAccumulatedTurnDegrees = higherAccumulatedTurn;
            maximumPrimaryArcChordRatio = primaryArcChord;
            maximumHigherOrderArcChordRatio = higherArcChord;
            minimumForwardProgress = forwardProgress;
            maximumParentReturnFraction = parentReturn;
            secondarySurvivalProbability = secondarySurvival;
            tertiarySurvivalProbability = tertiarySurvival;
            crownEnvelopeOvershoot = envelopeOvershoot;
        }
    }

    [Serializable]
    public sealed class TreeDamageSettings
    {
        [SerializeField]
        private TreeFloatRange missingBranchProbability = new TreeFloatRange(0f, 0.08f);

        [SerializeField]
        private TreeFloatRange deadBranchProbability = new TreeFloatRange(0f, 0.08f);

        [SerializeField]
        private TreeFloatRange breakProbability = new TreeFloatRange(0f, 0.05f);

        public TreeFloatRange MissingBranchProbability => missingBranchProbability;
        public TreeFloatRange DeadBranchProbability => deadBranchProbability;
        public TreeFloatRange BreakProbability => breakProbability;

        internal void ApplyTreeGen2C3Defaults(TreeFamily family)
        {
            if (family == TreeFamily.Twisted || family == TreeFamily.Dead)
            {
                missingBranchProbability = new TreeFloatRange(0.03f, 0.12f);
            }
        }

        internal bool ApplyTreeGen2C3Migration(TreeFamily family)
        {
            TreeFloatRange expected;
            if (family == TreeFamily.Twisted)
            {
                expected = new TreeFloatRange(0.05f, 0.22f);
            }
            else if (family == TreeFamily.Dead)
            {
                expected = new TreeFloatRange(0.08f, 0.24f);
            }
            else
            {
                return false;
            }

            if (Mathf.Abs(missingBranchProbability.Minimum - expected.Minimum) > 0.0001f ||
                Mathf.Abs(missingBranchProbability.Maximum - expected.Maximum) > 0.0001f)
            {
                return false;
            }

            missingBranchProbability = new TreeFloatRange(0.03f, 0.12f);
            return true;
        }

        internal void ApplyTreeGen2C5Defaults(TreeFamily family)
        {
            if (family == TreeFamily.Twisted || family == TreeFamily.Dead)
            {
                missingBranchProbability = new TreeFloatRange(0.02f, 0.08f);
            }
        }

        internal bool ApplyTreeGen2C5Migration(TreeFamily family)
        {
            if (family != TreeFamily.Twisted && family != TreeFamily.Dead)
            {
                return false;
            }

            if (Mathf.Abs(missingBranchProbability.Minimum - 0.03f) > 0.0001f ||
                Mathf.Abs(missingBranchProbability.Maximum - 0.12f) > 0.0001f)
            {
                return false;
            }

            missingBranchProbability = new TreeFloatRange(0.02f, 0.08f);
            return true;
        }

        internal void Set(
            TreeFloatRange missingRange,
            TreeFloatRange deadRange,
            TreeFloatRange breakRange)
        {
            missingBranchProbability = missingRange;
            deadBranchProbability = deadRange;
            breakProbability = breakRange;
        }
    }

    [CreateAssetMenu(
        fileName = "TreeFamilyProfile",
        menuName = "PS3D/Trees/Tree Family Profile")]
    public sealed class TreeFamilyProfile : ScriptableObject
    {
        public const int CurrentProfileVersion = 4;
        public const int CurrentBarkGrammarVersion = 6;

        [Header("Identity")]
        [SerializeField]
        private string stableIdentity = "tree-family-profile";

        [SerializeField]
        private int profileVersion = CurrentProfileVersion;

        [SerializeField, HideInInspector]
        private int barkGrammarVersion;

        [SerializeField]
        private TreeFamily family = TreeFamily.Common;

        [SerializeField]
        private TreeMaterialPalette defaultPalette;

        [Header("Family Grammar")]
        [SerializeField]
        private TreeOverallFormSettings overallForm = new TreeOverallFormSettings();

        [SerializeField]
        private TreeTrunkSettings trunk = new TreeTrunkSettings();

        [SerializeField]
        private TreePrimaryBranchSettings primaryBranches = new TreePrimaryBranchSettings();

        [SerializeField]
        private TreeBranchOrderSettings secondaryBranches = new TreeBranchOrderSettings();

        [SerializeField]
        private TreeBranchOrderSettings tertiaryBranches = new TreeBranchOrderSettings();

        [SerializeField]
        private TreeIntRange maximumBranchOrder = new TreeIntRange(2, 3);

        [SerializeField]
        private TreeFoliageSettings foliage = new TreeFoliageSettings();

        [SerializeField]
        private TreeDamageSettings damage = new TreeDamageSettings();

        [SerializeField]
        private TreeStructuralConstraintSettings structuralConstraints =
            new TreeStructuralConstraintSettings();

        [Header("Structural Safety")]
        [SerializeField, Min(8)]
        private int maximumBranchCount = 384;

        [SerializeField, Min(4)]
        private int maximumSamplesPerBranch = 48;

        [SerializeField, Min(0.001f)]
        private float minimumBranchLength = 0.08f;

        [SerializeField, Min(0.0001f)]
        private float minimumBranchRadius = 0.008f;

        public string StableIdentity => stableIdentity;
        public int ProfileVersion => profileVersion;
        public int BarkGrammarVersion => barkGrammarVersion;
        public TreeFamily Family => family;
        public TreeMaterialPalette DefaultPalette => defaultPalette;
        public TreeOverallFormSettings OverallForm => overallForm;
        public TreeTrunkSettings Trunk => trunk;
        public TreePrimaryBranchSettings PrimaryBranches => primaryBranches;
        public TreeBranchOrderSettings SecondaryBranches => secondaryBranches;
        public TreeBranchOrderSettings TertiaryBranches => tertiaryBranches;
        public TreeIntRange MaximumBranchOrder => maximumBranchOrder;
        public TreeFoliageSettings Foliage => foliage;
        public TreeDamageSettings Damage => damage;
        public TreeStructuralConstraintSettings StructuralConstraints =>
            structuralConstraints;
        public int MaximumBranchCount => Mathf.Max(8, maximumBranchCount);
        public int MaximumSamplesPerBranch => Mathf.Max(4, maximumSamplesPerBranch);
        public float MinimumBranchLength => Mathf.Max(0.001f, minimumBranchLength);
        public float MinimumBranchRadius => Mathf.Max(0.0001f, minimumBranchRadius);

        public void SetDefaultPalette(TreeMaterialPalette palette)
        {
            defaultPalette = palette;
        }

        public void ResetToFamilyDefaults(TreeFamily targetFamily)
        {
            family = targetFamily;
            profileVersion = CurrentProfileVersion;
            barkGrammarVersion = CurrentBarkGrammarVersion;
            stableIdentity = "tree-family-" + targetFamily.ToString().ToLowerInvariant();

            switch (targetFamily)
            {
                case TreeFamily.Pine:
                    ConfigurePine();
                    break;
                case TreeFamily.Twisted:
                    ConfigureTwisted();
                    break;
                case TreeFamily.Dead:
                    ConfigureDead();
                    break;
                default:
                    ConfigureCommon();
                    break;
            }

            ApplyTreeGen2BDefaults(targetFamily);
            ApplyTreeGen2CDefaults(targetFamily);
            ApplyTreeGen2C1Defaults(targetFamily);
            ApplyTreeGen2C2Defaults(targetFamily);
            ApplyTreeGen2C3Defaults(targetFamily);
            ApplyTreeGen2C5Defaults(targetFamily);
            ApplyTreeGen2C6Defaults(targetFamily);
        }

        public bool UpgradeManagedDefaults(TreeFamily expectedFamily)
        {
            if (family != expectedFamily)
            {
                ResetToFamilyDefaults(expectedFamily);
                return true;
            }

            bool changed = false;
            if (profileVersion < CurrentProfileVersion)
            {
                ApplyTreeGen2BMigration(expectedFamily);
                profileVersion = CurrentProfileVersion;
                changed = true;
            }

            if (barkGrammarVersion < 1)
            {
                ApplyTreeGen2CDefaults(expectedFamily);
                changed = true;
            }

            if (barkGrammarVersion < 2)
            {
                changed |= ApplyTreeGen2C1Migration(expectedFamily);
            }

            if (barkGrammarVersion < 3)
            {
                changed |= ApplyTreeGen2C2Migration(expectedFamily);
            }

            if (barkGrammarVersion < 4)
            {
                changed |= ApplyTreeGen2C3Migration(expectedFamily);
            }

            if (barkGrammarVersion < 5)
            {
                changed |= ApplyTreeGen2C5Migration(expectedFamily);
            }

            if (barkGrammarVersion < 6)
            {
                changed |= ApplyTreeGen2C6Migration(expectedFamily);
            }

            if (barkGrammarVersion < CurrentBarkGrammarVersion)
            {
                barkGrammarVersion = CurrentBarkGrammarVersion;
                changed = true;
            }

            return changed;
        }

        private void ApplyTreeGen2BDefaults(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            primaryBranches ??= new TreePrimaryBranchSettings();
            trunk.ApplyTreeGen2BDefaults(targetFamily);
            primaryBranches.ApplyTreeGen2BDefaults(targetFamily);
        }

        private void ApplyTreeGen2CDefaults(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            trunk.ApplyTreeGen2CDefaults(targetFamily);
        }

        private void ApplyTreeGen2C1Defaults(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            trunk.ApplyTreeGen2C1Defaults(targetFamily);
        }

        private bool ApplyTreeGen2C1Migration(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            return trunk.ApplyTreeGen2C1Migration(targetFamily);
        }

        private void ApplyTreeGen2C2Defaults(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            trunk.ApplyTreeGen2C2Defaults(targetFamily);
        }

        private bool ApplyTreeGen2C2Migration(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            return trunk.ApplyTreeGen2C2Migration(targetFamily);
        }

        private void ApplyTreeGen2C3Defaults(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            primaryBranches ??= new TreePrimaryBranchSettings();
            secondaryBranches ??= new TreeBranchOrderSettings();
            damage ??= new TreeDamageSettings();
            structuralConstraints ??= new TreeStructuralConstraintSettings();
            trunk.ApplyTreeGen2C3Defaults(targetFamily);
            primaryBranches.ApplyTreeGen2C3Defaults(targetFamily, 1);
            primaryBranches.ApplyTreeGen2C3Defaults(targetFamily);
            secondaryBranches.ApplyTreeGen2C3Defaults(targetFamily, 2);
            damage.ApplyTreeGen2C3Defaults(targetFamily);
            structuralConstraints.ApplyTreeGen2C3Defaults(targetFamily);
        }

        private bool ApplyTreeGen2C3Migration(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            primaryBranches ??= new TreePrimaryBranchSettings();
            secondaryBranches ??= new TreeBranchOrderSettings();
            damage ??= new TreeDamageSettings();
            structuralConstraints ??= new TreeStructuralConstraintSettings();
            bool changed = trunk.ApplyTreeGen2C3Migration(targetFamily);
            changed |= primaryBranches.ApplyTreeGen2C3Migration(targetFamily, 1);
            changed |= primaryBranches.ApplyTreeGen2C3Migration(targetFamily);
            changed |= secondaryBranches.ApplyTreeGen2C3Migration(targetFamily, 2);
            changed |= damage.ApplyTreeGen2C3Migration(targetFamily);
            changed |= structuralConstraints.ApplyTreeGen2C3Migration(targetFamily);
            return changed;
        }

        private void ApplyTreeGen2C5Defaults(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            primaryBranches ??= new TreePrimaryBranchSettings();
            secondaryBranches ??= new TreeBranchOrderSettings();
            damage ??= new TreeDamageSettings();
            structuralConstraints ??= new TreeStructuralConstraintSettings();
            trunk.ApplyTreeGen2C5Defaults(targetFamily);
            primaryBranches.ApplyTreeGen2C5Defaults(targetFamily, 1);
            primaryBranches.ApplyTreeGen2C5Defaults(targetFamily);
            secondaryBranches.ApplyTreeGen2C5Defaults(targetFamily, 2);
            damage.ApplyTreeGen2C5Defaults(targetFamily);
            structuralConstraints.ApplyTreeGen2C5Defaults(targetFamily);
        }

        private bool ApplyTreeGen2C5Migration(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            primaryBranches ??= new TreePrimaryBranchSettings();
            secondaryBranches ??= new TreeBranchOrderSettings();
            damage ??= new TreeDamageSettings();
            structuralConstraints ??= new TreeStructuralConstraintSettings();
            bool changed = trunk.ApplyTreeGen2C5Migration(targetFamily);
            changed |= primaryBranches.ApplyTreeGen2C5Migration(targetFamily, 1);
            changed |= primaryBranches.ApplyTreeGen2C5Migration(targetFamily);
            changed |= secondaryBranches.ApplyTreeGen2C5Migration(targetFamily, 2);
            changed |= damage.ApplyTreeGen2C5Migration(targetFamily);
            changed |= structuralConstraints.ApplyTreeGen2C5Migration(targetFamily);
            return changed;
        }

        private void ApplyTreeGen2C6Defaults(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            trunk.ApplyTreeGen2C6Defaults(targetFamily);
        }

        private bool ApplyTreeGen2C6Migration(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            return trunk.ApplyTreeGen2C6Migration(targetFamily);
        }

        private void ApplyTreeGen2BMigration(TreeFamily targetFamily)
        {
            trunk ??= new TreeTrunkSettings();
            primaryBranches ??= new TreePrimaryBranchSettings();
            trunk.ApplyTreeGen2BDefaults(targetFamily);
            primaryBranches.ApplyTreeGen2BMigration(targetFamily);
        }

        public bool ValidateProfile(List<string> failures)
        {
            if (failures == null)
            {
                throw new ArgumentNullException(nameof(failures));
            }

            if (string.IsNullOrWhiteSpace(stableIdentity))
            {
                failures.Add("Family profile stable identity is empty.");
            }

            ValidateRange(overallForm.Height, "Overall height", failures, 0.1f);
            ValidateRange(overallForm.TrunkBaseRadius, "Trunk base radius", failures, 0.001f);
            ValidateRange(overallForm.CrownStartHeight, "Crown start height", failures, 0f, 1f);
            ValidateRange(trunk.Curvature, "Trunk curvature", failures, 0f);
            ValidateRange(trunk.SurfaceTorsionDegrees, "Trunk twist degrees", failures, -720f, 720f);
            ValidateRange(trunk.SpiralStrength, "Trunk spiral strength", failures, 0f, 1f);
            ValidateRange(trunk.SpiralTurns, "Trunk spiral turns", failures, 0f, 8f);
            ValidateRange(trunk.SpiralDirection, "Trunk spiral direction", failures, -1f, 1f);
            if (!trunk.RootButtressCount.IsValid ||
                trunk.RootButtressCount.Minimum < 3 ||
                trunk.RootButtressCount.Maximum > 8)
            {
                failures.Add("Root buttress-count range must be ordered from 3 through 8.");
            }
            ValidateRange(trunk.RootButtressStrength, "Root buttress strength", failures, 0f, 1.5f);
            ValidateRange(trunk.RootButtressHeight, "Root buttress height", failures, 0.01f, 0.6f);
            ValidateRange(trunk.RootFlareScale, "Root flare scale", failures, 1f, 2.5f);
            ValidateRange(trunk.Taper, "Trunk taper", failures, 0.01f, 0.99f);
            ValidateRange(primaryBranches.AttachmentHeight, "Primary attachment height", failures, 0f, 1f);
            ValidateRange(primaryBranches.StartHeight, "Primary branch start height", failures, 0f, 1f);
            ValidateRange(primaryBranches.EndHeight, "Primary branch end height", failures, 0f, 1f);
            ValidateRange(primaryBranches.InitialElevationDegrees, "Initial branch elevation", failures, -89f, 89f);
            ValidateRange(primaryBranches.ArchDirection, "Branch arch direction", failures, -1f, 1f);
            ValidateRange(primaryBranches.ArchStrength, "Branch arch strength", failures, 0f, 2f);
            ValidateRange(primaryBranches.LateSag, "Late branch sag", failures, 0f, 2f);
            ValidateRange(primaryBranches.AzimuthSymmetry, "Azimuth symmetry", failures, 0f, 1f);
            ValidateRange(primaryBranches.DirectionalBiasAngleDegrees, "Directional bias angle", failures, 0f, 360f);
            ValidateRange(primaryBranches.DirectionalBiasStrength, "Directional bias strength", failures, 0f, 1f);
            ValidateRange(primaryBranches.LengthRatio, "Primary length ratio", failures, 0.01f);
            ValidateRange(primaryBranches.RadiusRatio, "Primary radius ratio", failures, 0.01f, 0.95f);
            ValidateRange(foliage.Eligibility, "Foliage eligibility", failures, 0f, 1f);
            ValidateRange(damage.MissingBranchProbability, "Missing branch probability", failures, 0f, 1f);
            ValidateRange(damage.DeadBranchProbability, "Dead branch probability", failures, 0f, 1f);
            ValidateRange(damage.BreakProbability, "Break probability", failures, 0f, 1f);

            if (structuralConstraints.MaximumPrimaryArcChordRatio < 1f ||
                structuralConstraints.MaximumHigherOrderArcChordRatio < 1f)
            {
                failures.Add("Structural arc/chord limits cannot be below 1.");
            }

            if (!trunk.ControlPointCount.IsValid || trunk.ControlPointCount.Minimum < 3)
            {
                failures.Add("Trunk control-point range must be ordered and at least 3.");
            }

            if (!primaryBranches.Count.IsValid || primaryBranches.Count.Minimum < 0)
            {
                failures.Add("Primary branch-count range is invalid.");
            }

            if (primaryBranches.StartHeight.Maximum > primaryBranches.EndHeight.Minimum)
            {
                failures.Add("Primary branch start/end ranges overlap; a resolved start could exceed the resolved end.");
            }

            if (!maximumBranchOrder.IsValid || maximumBranchOrder.Minimum < 1 || maximumBranchOrder.Maximum > 3)
            {
                failures.Add("TREE-GEN.2C supports maximum branch orders from 1 through 3.");
            }

            return failures.Count == 0;
        }

        private void OnValidate()
        {
            profileVersion = Mathf.Max(1, profileVersion);
            barkGrammarVersion = Mathf.Max(0, barkGrammarVersion);
            maximumBranchCount = Mathf.Max(8, maximumBranchCount);
            maximumSamplesPerBranch = Mathf.Max(4, maximumSamplesPerBranch);
            minimumBranchLength = Mathf.Max(0.001f, minimumBranchLength);
            minimumBranchRadius = Mathf.Max(0.0001f, minimumBranchRadius);
            structuralConstraints ??= new TreeStructuralConstraintSettings();
        }

        private static void ValidateRange(
            TreeFloatRange range,
            string label,
            List<string> failures,
            float minimumAllowed,
            float maximumAllowed = float.PositiveInfinity)
        {
            if (!range.IsValid ||
                range.Minimum < minimumAllowed ||
                range.Maximum > maximumAllowed)
            {
                failures.Add(label + " range is invalid or outside its safety limits.");
            }
        }

        private void ConfigureCommon()
        {
            overallForm.Set(
                new TreeFloatRange(7f, 10f),
                new TreeFloatRange(0.28f, 0.58f),
                new TreeFloatRange(0.22f, 0.4f),
                new TreeFloatRange(0.95f, 1.45f),
                new TreeFloatRange(0.9f, 1.35f),
                new TreeFloatRange(0.9f, 1.2f),
                new TreeFloatRange(0.62f, 0.92f),
                new TreeFloatRange(0.06f, 0.3f),
                new TreeIntRange(5, 10),
                new TreeFloatRange(0.8f, 1.35f));
            trunk.Set(
                new TreeIntRange(5, 8),
                new TreeFloatRange(0.04f, 0.18f),
                new TreeFloatRange(0.8f, 1.8f),
                new TreeFloatRange(0.01f, 0.06f),
                new TreeFloatRange(0f, 0.12f),
                new TreeFloatRange(-20f, 20f),
                new TreeFloatRange(0.02f, 0.10f),
                new TreeFloatRange(0.72f, 0.9f),
                new TreeFloatRange(0f, 0.18f),
                new TreeFloatRange(0.55f, 0.82f));
            primaryBranches.Set(
                new TreeIntRange(8, 14),
                new TreeFloatRange(0.22f, 0.9f),
                new TreeFloatRange(0.24f, 0.46f),
                new TreeFloatRange(0.3f, 0.5f),
                new TreeFloatRange(0.08f, 0.22f),
                new TreeFloatRange(0f, 0.14f),
                new TreeFloatRange(0.08f, 0.35f),
                new TreeFloatRange(-0.10f, 0.10f),
                new TreeFloatRange(-22f, 22f),
                new TreeFloatRange(0.03f, 0.12f),
                new TreeFloatRange(-0.03f, 0.10f),
                new TreeFloatRange(0.035f, 0.09f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0f, 0.15f),
                new TreeFloatRange(-0.12f, 0.12f));
            secondaryBranches.Set(
                new TreeIntRange(2, 3),
                new TreeFloatRange(0.30f, 0.88f),
                new TreeFloatRange(0.26f, 0.44f),
                new TreeFloatRange(0.28f, 0.48f),
                new TreeFloatRange(0.10f, 0.26f),
                new TreeFloatRange(0f, 0.25f),
                new TreeFloatRange(0.05f, 0.35f),
                new TreeFloatRange(-0.2f, 0.2f),
                new TreeFloatRange(-40f, 40f),
                new TreeFloatRange(0.04f, 0.14f),
                new TreeFloatRange(-0.04f, 0.12f),
                new TreeFloatRange(0.07f, 0.2f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0f, 0.2f),
                new TreeFloatRange(-0.18f, 0.18f));
            tertiaryBranches.Set(
                new TreeIntRange(0, 1),
                new TreeFloatRange(0.40f, 0.92f),
                new TreeFloatRange(0.22f, 0.36f),
                new TreeFloatRange(0.24f, 0.42f),
                new TreeFloatRange(0.10f, 0.24f),
                new TreeFloatRange(0f, 0.28f),
                new TreeFloatRange(0.02f, 0.3f),
                new TreeFloatRange(-0.25f, 0.25f),
                new TreeFloatRange(-45f, 45f),
                new TreeFloatRange(0.04f, 0.14f),
                new TreeFloatRange(-0.04f, 0.10f),
                new TreeFloatRange(0.1f, 0.3f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0f, 0.2f),
                new TreeFloatRange(-0.2f, 0.2f));
            foliage.Set(
                new TreeFloatRange(0.9f, 1.55f),
                new TreeFloatRange(0.9f, 1.4f),
                new TreeFloatRange(0.9f, 1.45f),
                new TreeFloatRange(0.8f, 1.4f),
                new TreeFloatRange(0.85f, 1.3f),
                new TreeIntRange(24, 64),
                new TreeIntRange(3, 7),
                new TreeFloatRange(0.7f, 0.98f),
                new TreeFloatRange(0.62f, 0.95f),
                new TreeFloatRange(0.7f, 0.98f),
                new TreeFloatRange(0.85f, 1f));
            damage.Set(
                new TreeFloatRange(0f, 0.08f),
                new TreeFloatRange(0f, 0.08f),
                new TreeFloatRange(0f, 0.05f));
            maximumBranchOrder = new TreeIntRange(2, 3);
            structuralConstraints.Set(
                0.24f, 24f, 30f, 110f, 75f,
                1.22f, 1.14f, 0.25f, 0.08f,
                0.88f, 0.50f, 0.12f);
            maximumBranchCount = 160;
        }

        private void ConfigurePine()
        {
            ConfigureCommon();
            overallForm.Set(
                new TreeFloatRange(7f, 11f),
                new TreeFloatRange(0.24f, 0.48f),
                new TreeFloatRange(0.18f, 0.36f),
                new TreeFloatRange(0.85f, 1.25f),
                new TreeFloatRange(0.7f, 1.15f),
                new TreeFloatRange(1f, 1.25f),
                new TreeFloatRange(0.55f, 0.82f),
                new TreeFloatRange(0.02f, 0.16f),
                new TreeIntRange(6, 12),
                new TreeFloatRange(0.65f, 1.05f));
            trunk.Set(
                new TreeIntRange(5, 7),
                new TreeFloatRange(0.02f, 0.14f),
                new TreeFloatRange(0.8f, 1.8f),
                new TreeFloatRange(0f, 0.06f),
                new TreeFloatRange(0f, 0.06f),
                new TreeFloatRange(-10f, 10f),
                new TreeFloatRange(0.01f, 0.08f),
                new TreeFloatRange(0.78f, 0.93f),
                new TreeFloatRange(0f, 0.05f),
                new TreeFloatRange(0.7f, 0.9f));
            primaryBranches.Set(
                new TreeIntRange(16, 30),
                new TreeFloatRange(0.18f, 0.92f),
                new TreeFloatRange(0.2f, 0.38f),
                new TreeFloatRange(0.2f, 0.4f),
                new TreeFloatRange(0.05f, 0.2f),
                new TreeFloatRange(0.12f, 0.5f),
                new TreeFloatRange(-0.1f, 0.18f),
                new TreeFloatRange(-0.08f, 0.08f),
                new TreeFloatRange(-15f, 15f),
                new TreeFloatRange(0.02f, 0.12f),
                new TreeFloatRange(-0.08f, 0.1f),
                new TreeFloatRange(0.02f, 0.06f),
                new TreeIntRange(5, 10),
                new TreeIntRange(3, 5),
                new TreeFloatRange(0.02f, 0.12f),
                new TreeFloatRange(-0.05f, 0.05f));
            secondaryBranches.Set(
                new TreeIntRange(1, 2),
                new TreeFloatRange(0.30f, 0.88f),
                new TreeFloatRange(0.24f, 0.40f),
                new TreeFloatRange(0.24f, 0.40f),
                new TreeFloatRange(0.07f, 0.18f),
                new TreeFloatRange(0.08f, 0.30f),
                new TreeFloatRange(0.02f, 0.22f),
                new TreeFloatRange(-0.08f, 0.08f),
                new TreeFloatRange(-18f, 18f),
                new TreeFloatRange(0.02f, 0.10f),
                new TreeFloatRange(-0.06f, 0.06f),
                new TreeFloatRange(0.08f, 0.20f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0f, 0.1f),
                new TreeFloatRange(-0.08f, 0.08f));
            tertiaryBranches.Set(
                new TreeIntRange(0, 1),
                new TreeFloatRange(0.45f, 0.90f),
                new TreeFloatRange(0.20f, 0.32f),
                new TreeFloatRange(0.20f, 0.34f),
                new TreeFloatRange(0.06f, 0.14f),
                new TreeFloatRange(0.06f, 0.22f),
                new TreeFloatRange(0f, 0.16f),
                new TreeFloatRange(-0.06f, 0.06f),
                new TreeFloatRange(-15f, 15f),
                new TreeFloatRange(0.02f, 0.08f),
                new TreeFloatRange(-0.04f, 0.04f),
                new TreeFloatRange(0.12f, 0.25f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0f, 0.08f),
                new TreeFloatRange(-0.06f, 0.06f));
            foliage.Set(
                new TreeFloatRange(0.75f, 1.2f),
                new TreeFloatRange(0.65f, 1.05f),
                new TreeFloatRange(1f, 1.7f),
                new TreeFloatRange(0.55f, 1f),
                new TreeFloatRange(0.75f, 1.15f),
                new TreeIntRange(20, 56),
                new TreeIntRange(3, 6),
                new TreeFloatRange(0.7f, 0.98f),
                new TreeFloatRange(0.55f, 0.86f),
                new TreeFloatRange(0.8f, 1f),
                new TreeFloatRange(0.9f, 1f));
            maximumBranchOrder = new TreeIntRange(2, 3);
            structuralConstraints.Set(
                0.16f, 18f, 24f, 85f, 60f,
                1.16f, 1.10f, 0.34f, 0.05f,
                0.82f, 0.42f, 0.10f);
            maximumBranchCount = 180;
        }

        private void ConfigureTwisted()
        {
            ConfigureCommon();
            overallForm.Set(
                new TreeFloatRange(14f, 20f),
                new TreeFloatRange(0.5f, 1.1f),
                new TreeFloatRange(0.28f, 0.55f),
                new TreeFloatRange(0.55f, 1.05f),
                new TreeFloatRange(0.8f, 1.35f),
                new TreeFloatRange(0.85f, 1.25f),
                new TreeFloatRange(0.32f, 0.68f),
                new TreeFloatRange(0.3f, 0.75f),
                new TreeIntRange(3, 7),
                new TreeFloatRange(0.65f, 1.25f));
            trunk.Set(
                new TreeIntRange(7, 12),
                new TreeFloatRange(0.18f, 0.45f),
                new TreeFloatRange(1.2f, 2.5f),
                new TreeFloatRange(0.04f, 0.18f),
                new TreeFloatRange(0.08f, 0.4f),
                new TreeFloatRange(-110f, 110f),
                new TreeFloatRange(0.08f, 0.24f),
                new TreeFloatRange(0.62f, 0.86f),
                new TreeFloatRange(0.05f, 0.28f),
                new TreeFloatRange(0.42f, 0.78f));
            primaryBranches.Set(
                new TreeIntRange(5, 11),
                new TreeFloatRange(0.2f, 0.9f),
                new TreeFloatRange(0.25f, 0.55f),
                new TreeFloatRange(0.28f, 0.55f),
                new TreeFloatRange(0.18f, 0.42f),
                new TreeFloatRange(-0.05f, 0.35f),
                new TreeFloatRange(-0.05f, 0.28f),
                new TreeFloatRange(-0.24f, 0.24f),
                new TreeFloatRange(-45f, 45f),
                new TreeFloatRange(0.10f, 0.26f),
                new TreeFloatRange(-0.10f, 0.22f),
                new TreeFloatRange(0.05f, 0.14f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0.1f, 0.35f),
                new TreeFloatRange(-0.65f, 0.65f));
            secondaryBranches.Set(
                new TreeIntRange(1, 2),
                new TreeFloatRange(0.32f, 0.88f),
                new TreeFloatRange(0.24f, 0.42f),
                new TreeFloatRange(0.24f, 0.42f),
                new TreeFloatRange(0.12f, 0.28f),
                new TreeFloatRange(-0.02f, 0.20f),
                new TreeFloatRange(-0.02f, 0.20f),
                new TreeFloatRange(-0.18f, 0.18f),
                new TreeFloatRange(-35f, 35f),
                new TreeFloatRange(0.08f, 0.20f),
                new TreeFloatRange(-0.08f, 0.16f),
                new TreeFloatRange(0.10f, 0.24f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0.04f, 0.16f),
                new TreeFloatRange(-0.3f, 0.3f));
            tertiaryBranches.Set(
                new TreeIntRange(0, 1),
                new TreeFloatRange(0.45f, 0.90f),
                new TreeFloatRange(0.18f, 0.32f),
                new TreeFloatRange(0.18f, 0.34f),
                new TreeFloatRange(0.10f, 0.22f),
                new TreeFloatRange(-0.02f, 0.16f),
                new TreeFloatRange(-0.02f, 0.16f),
                new TreeFloatRange(-0.14f, 0.14f),
                new TreeFloatRange(-28f, 28f),
                new TreeFloatRange(0.06f, 0.16f),
                new TreeFloatRange(-0.06f, 0.12f),
                new TreeFloatRange(0.12f, 0.28f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0.03f, 0.12f),
                new TreeFloatRange(-0.22f, 0.22f));
            foliage.Set(
                new TreeFloatRange(0.65f, 1.2f),
                new TreeFloatRange(0.65f, 1.1f),
                new TreeFloatRange(0.75f, 1.25f),
                new TreeFloatRange(0.45f, 1.05f),
                new TreeFloatRange(0.75f, 1.2f),
                new TreeIntRange(8, 30),
                new TreeIntRange(2, 5),
                new TreeFloatRange(0.3f, 0.75f),
                new TreeFloatRange(0.3f, 0.7f),
                new TreeFloatRange(0.35f, 0.8f),
                new TreeFloatRange(0.55f, 0.95f));
            damage.Set(
                new TreeFloatRange(0.05f, 0.22f),
                new TreeFloatRange(0.08f, 0.3f),
                new TreeFloatRange(0.04f, 0.2f));
            maximumBranchOrder = new TreeIntRange(2, 3);
            structuralConstraints.Set(
                0.38f, 30f, 36f, 145f, 95f,
                1.30f, 1.17f, 0.18f, 0.12f,
                0.72f, 0.34f, 0.16f);
            maximumBranchCount = 120;
        }

        private void ConfigureDead()
        {
            ConfigureTwisted();
            overallForm.Set(
                new TreeFloatRange(9f, 17f),
                new TreeFloatRange(0.35f, 0.85f),
                new TreeFloatRange(0.3f, 0.65f),
                new TreeFloatRange(0f, 0.1f),
                new TreeFloatRange(0.8f, 1.2f),
                new TreeFloatRange(0.8f, 1.1f),
                new TreeFloatRange(0f, 0.05f),
                new TreeFloatRange(0.18f, 0.6f),
                new TreeIntRange(0, 1),
                new TreeFloatRange(0f, 0.1f));
            primaryBranches.Set(
                new TreeIntRange(9, 17),
                new TreeFloatRange(0.18f, 0.92f),
                new TreeFloatRange(0.16f, 0.38f),
                new TreeFloatRange(0.25f, 0.52f),
                new TreeFloatRange(0.18f, 0.58f),
                new TreeFloatRange(-0.05f, 0.3f),
                new TreeFloatRange(-0.05f, 0.32f),
                new TreeFloatRange(-0.4f, 0.4f),
                new TreeFloatRange(-70f, 70f),
                new TreeFloatRange(0.12f, 0.4f),
                new TreeFloatRange(-0.25f, 0.4f),
                new TreeFloatRange(0.04f, 0.12f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0.08f, 0.3f),
                new TreeFloatRange(-0.55f, 0.55f));
            secondaryBranches.Set(
                new TreeIntRange(1, 3),
                new TreeFloatRange(0.28f, 0.88f),
                new TreeFloatRange(0.20f, 0.34f),
                new TreeFloatRange(0.22f, 0.38f),
                new TreeFloatRange(0.10f, 0.24f),
                new TreeFloatRange(-0.04f, 0.18f),
                new TreeFloatRange(-0.04f, 0.16f),
                new TreeFloatRange(-0.15f, 0.15f),
                new TreeFloatRange(-30f, 30f),
                new TreeFloatRange(0.06f, 0.18f),
                new TreeFloatRange(-0.08f, 0.14f),
                new TreeFloatRange(0.10f, 0.24f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0.03f, 0.12f),
                new TreeFloatRange(-0.25f, 0.25f));
            tertiaryBranches.Set(
                new TreeIntRange(0, 0),
                new TreeFloatRange(0.45f, 0.90f),
                new TreeFloatRange(0.18f, 0.28f),
                new TreeFloatRange(0.18f, 0.30f),
                new TreeFloatRange(0.08f, 0.18f),
                new TreeFloatRange(0f, 0.14f),
                new TreeFloatRange(0f, 0.12f),
                new TreeFloatRange(-0.10f, 0.10f),
                new TreeFloatRange(-20f, 20f),
                new TreeFloatRange(0.04f, 0.12f),
                new TreeFloatRange(-0.04f, 0.08f),
                new TreeFloatRange(0.12f, 0.25f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0.02f, 0.08f),
                new TreeFloatRange(-0.15f, 0.15f));
            foliage.Set(
                new TreeFloatRange(0f, 0f),
                new TreeFloatRange(0f, 0f),
                new TreeFloatRange(0f, 0f),
                new TreeFloatRange(0f, 0f),
                new TreeFloatRange(0f, 0f),
                new TreeIntRange(0, 0),
                new TreeIntRange(0, 0),
                new TreeFloatRange(0f, 0f),
                new TreeFloatRange(0f, 0f),
                new TreeFloatRange(0f, 0f),
                new TreeFloatRange(0f, 0f));
            damage.Set(
                new TreeFloatRange(0.08f, 0.24f),
                new TreeFloatRange(0.55f, 0.95f),
                new TreeFloatRange(0.16f, 0.42f));
            maximumBranchOrder = new TreeIntRange(2, 2);
            structuralConstraints.Set(
                0.30f, 26f, 32f, 125f, 82f,
                1.25f, 1.14f, 0.20f, 0.10f,
                0.62f, 0f, 0.18f);
            maximumBranchCount = 112;
        }
    }
}
