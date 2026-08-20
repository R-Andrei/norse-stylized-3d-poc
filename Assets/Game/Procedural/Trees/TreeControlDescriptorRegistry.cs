using System;
using System.Collections.Generic;

namespace ProgrammaticStylized3D.Trees
{
    public enum TreeControlSection
    {
        OverallForm = 0,
        TrunkShape = 1,
        TrunkSpiralAndTwist = 2,
        Roots = 3,
        PrimaryBranchPlacement = 4,
        PrimaryBranchShape = 5,
        BranchHierarchy = 6,
        Damage = 7,
        Appearance = 8,
        AdvancedTrunkDetail = 9,
        AdvancedBranchDistribution = 10,
        AdvancedPrimaryBranchDetail = 11,
        AdvancedForking = 12
    }

    public enum TreeControlValueKind
    {
        Float = 0,
        Integer = 1,
        Angle = 2,
        Color = 3
    }

    public sealed class TreeControlSectionDescriptor
    {
        public TreeControlSectionDescriptor(
            TreeControlSection sectionValue,
            string keyValue,
            string labelValue,
            string noteValue,
            bool advancedValue)
        {
            Section = sectionValue;
            Key = keyValue;
            Label = labelValue;
            Note = noteValue;
            Advanced = advancedValue;
        }

        public TreeControlSection Section { get; }
        public string Key { get; }
        public string Label { get; }
        public string Note { get; }
        public bool Advanced { get; }
    }

    public sealed class TreeControlDescriptor
    {
        public TreeControlDescriptor(
            TreeControlSection sectionValue,
            string stableIdValue,
            string propertyNameValue,
            string labelValue,
            string tooltipValue,
            TreeControlValueKind kindValue,
            float hardMinimumValue,
            float hardMaximumValue)
        {
            Section = sectionValue;
            StableId = stableIdValue;
            PropertyName = propertyNameValue;
            Label = labelValue;
            Tooltip = tooltipValue;
            Kind = kindValue;
            HardMinimum = hardMinimumValue;
            HardMaximum = hardMaximumValue;
        }

        public TreeControlSection Section { get; }
        public string StableId { get; }
        public string PropertyName { get; }
        public string Label { get; }
        public string Tooltip { get; }
        public TreeControlValueKind Kind { get; }
        public float HardMinimum { get; }
        public float HardMaximum { get; }
    }

    public static class TreeControlDescriptorRegistry
    {
        private static readonly TreeControlSectionDescriptor[] sections =
        {
            new TreeControlSectionDescriptor(
                TreeControlSection.OverallForm,
                "overall-form",
                "Overall Form",
                "Height and base radius establish absolute scale. Taper controls how strongly the trunk narrows from base to tip. These controls do not change root angular thickness.",
                false),
            new TreeControlSectionDescriptor(
                TreeControlSection.TrunkShape,
                "trunk-shape",
                "Trunk Shape",
                "Bend Amount creates lateral trunk curvature. Lean Amount moves the whole trunk coherently in the canonical tree-local +X direction; rotate the whole tree object to change its world-space lean direction.",
                false),
            new TreeControlSectionDescriptor(
                TreeControlSection.TrunkSpiralAndTwist,
                "trunk-spiral-twist",
                "Trunk Spiral and Twist",
                "Path Spiral moves the trunk centreline while preserving authored vertical height. Axial Twist rotates only the bark/cross-section frame around that centreline and does not rotate structural branch attachments. Signed Path Spiral Turns uses sign for handedness and magnitude for revolutions.",
                false),
            new TreeControlSectionDescriptor(
                TreeControlSection.Roots,
                "roots",
                "Roots",
                "Root Count changes count. Root Reach changes ground-level radial projection. Root Thickness first broadens each buttress, then merges the lower base between neighbouring roots after individual support reaches its Root Count sector limit; it does not change crest reach. Root Height changes the ground-level root envelope. Buttress Persistence controls how far root-owned buttress lobes and their aligned frame persist before the trunk becomes circular. The final root geometry retains grounded widening and distinct root crests while high Thickness can form a broad shared base.",
                false),
            new TreeControlSectionDescriptor(
                TreeControlSection.PrimaryBranchPlacement,
                "primary-placement",
                "Primary Branch Placement",
                "Count requests primary branches. Start and End Height define an ordered normalized trunk band; recipe ranges are constrained so independently sampled End values cannot fall below sampled Start values. Symmetry blends random azimuths toward even distribution.",
                false),
            new TreeControlSectionDescriptor(
                TreeControlSection.PrimaryBranchShape,
                "primary-shape",
                "Primary Branch Shape",
                "Length is relative to tree height. Thickness is relative to parent radius. Elevation controls launch angle. Curvature controls centreline bending after launch.",
                false),
            new TreeControlSectionDescriptor(
                TreeControlSection.BranchHierarchy,
                "branch-hierarchy",
                "Branch Hierarchy",
                "Maximum Branch Order enables primary, secondary and tertiary structure. Fractional density deterministically controls the chance of one additional child. Child Scale controls successive-order length and derived thickness.",
                false),
            new TreeControlSectionDescriptor(
                TreeControlSection.Damage,
                "damage",
                "Damage",
                "Missing removes candidates before geometry creation. Dead preserves geometry and makes its bark darker and more wind-stiff while reserving the state for later foliage exclusion. Broken shortens or truncates accepted branches.",
                false),
            new TreeControlSectionDescriptor(
                TreeControlSection.Appearance,
                "appearance",
                "Appearance",
                "Bark Tint is opaque per-tree RGB appearance data applied through a renderer property block; alpha is fixed to one. Shared materials continue to own textures, normals, smoothness and specular response.",
                false),
            new TreeControlSectionDescriptor(
                TreeControlSection.AdvancedTrunkDetail,
                "advanced-trunk-detail",
                "Advanced Trunk Detail",
                "Bend Frequency changes bend-cycle count. Drift is coherent cumulative movement. Roughness is local jitter.",
                true),
            new TreeControlSectionDescriptor(
                TreeControlSection.AdvancedBranchDistribution,
                "advanced-branch-distribution",
                "Advanced Branch Distribution",
                "Directional Bias blends branches toward one preferred horizontal direction. Tier Spacing creates explicit bands across the full authored Start/End attachment interval; zero disables tiering.",
                true),
            new TreeControlSectionDescriptor(
                TreeControlSection.AdvancedPrimaryBranchDetail,
                "advanced-primary-detail",
                "Advanced Primary Branch Detail",
                "Branch Arch is one signed mid-branch bend. Late Sag affects the latter branch. Tip Upturn lifts only the tip. Side Sweep bends laterally relative to the primary branch plane.",
                true),
            new TreeControlSectionDescriptor(
                TreeControlSection.AdvancedForking,
                "advanced-forking",
                "Advanced Forking",
                "Fork Chance controls whether one structural trunk fork is created. The compact recipe-only fork form uses a fixed 68 percent placement independent of the primary branch attachment band; a separate placement control remains deferred until reference matching proves it necessary.",
                true)
        };

        private static readonly TreeControlDescriptor[] controls =
        {
            Float(TreeControlSection.OverallForm, "tree.height", "height", "Height", "Absolute tree height in metres.", 1f, 40f),
            Float(TreeControlSection.OverallForm, "tree.trunk.base-radius", "trunkBaseRadius", "Trunk Base Radius", "Trunk radius at ground level before root deformation, in metres.", 0.02f, 4f),
            Float(TreeControlSection.OverallForm, "tree.trunk.taper", "trunkTaper", "Trunk Taper", "Fraction of the available radius reduction from the authored base radius to the safe minimum tip radius. Zero keeps the base radius; one reaches the safe minimum with no dead slider region.", 0f, 1f),

            Float(TreeControlSection.TrunkShape, "tree.trunk.bend-amount", "bendAmount", "Bend Amount", "Lateral centreline bend amplitude as a normalized tree-shape control.", 0f, 1f),
            Float(TreeControlSection.TrunkShape, "tree.trunk.lean-amount", "leanAmount", "Lean Amount", "Coherent horizontal trunk displacement as a fraction of tree height.", 0f, 0.60f),

            Float(TreeControlSection.TrunkSpiralAndTwist, "tree.trunk.path-spiral-radius", "pathSpiralRadius", "Path Spiral Radius", "Radius of the actual centreline spiral as a fraction of tree height.", 0f, 0.50f),
            Float(TreeControlSection.TrunkSpiralAndTwist, "tree.trunk.path-spiral-turns", "signedPathSpiralTurns", "Signed Path Spiral Turns", "Number of height-preserving centreline revolutions. Positive and negative values select opposite handedness without changing authored tree height.", -3f, 3f),
            Float(TreeControlSection.TrunkSpiralAndTwist, "tree.trunk.axial-twist", "axialTwist", "Axial Twist", "Total bark/cross-section roll around the trunk centreline, in degrees. It does not move the centreline or rotate structural branch attachment azimuths.", -1080f, 1080f),

            Integer(TreeControlSection.Roots, "tree.root.count", "rootCount", "Root Count", "Number of buttresses around the trunk. Count does not redefine requested angular width; emitted width is clamped and reported only when the requested support would overlap adjacent roots.", 3, 8),
            Float(TreeControlSection.Roots, "tree.root.reach", "rootReach", "Root Reach", "Ground-level outward projection measured as added radius relative to the local trunk radius.", 0f, 2f),
            Float(TreeControlSection.Roots, "tree.root.thickness", "rootThickness", "Root Thickness", "Root breadth and lower-base mass. 0.5 reproduces accepted H4 breadth; higher values broaden individual support until neighbouring sectors meet, then progressively merge the lower base without changing Root Reach crest amplitude.", 0.10f, 2f),
            Float(TreeControlSection.Roots, "tree.root.height", "rootHeight", "Root Height", "Vertical extent of the ground-level root and buttress envelope as a fraction of tree height.", 0.01f, 0.40f),
            Float(TreeControlSection.Roots, "tree.root.buttress-transition", "buttressTransition", "Buttress Persistence", "Controls how far root-owned buttress lobes persist up the trunk. Zero transitions to a circular trunk at the earliest safe endpoint; one carries the root-owned lobes to the trunk tip.", 0f, 1f),

            Integer(TreeControlSection.PrimaryBranchPlacement, "tree.branch.primary.count", "primaryBranchCount", "Primary Branch Count", "Requested number of primary branches before deterministic damage and structural budgets.", 0, 64),
            Float(TreeControlSection.PrimaryBranchPlacement, "tree.branch.primary.start-height", "branchStartHeight", "Branch Start Height", "Lowest normalized trunk position where primary branches may attach. Recipe range validation keeps every independently sampled End at or above this range maximum.", 0f, 1f),
            Float(TreeControlSection.PrimaryBranchPlacement, "tree.branch.primary.end-height", "branchEndHeight", "Branch End Height", "Highest normalized trunk position where primary branches may attach. Recipe range validation keeps this range minimum at or above the Start range maximum.", 0f, 1f),
            Float(TreeControlSection.PrimaryBranchPlacement, "tree.branch.primary.symmetry", "branchSymmetry", "Branch Symmetry", "Zero uses random azimuths; one approaches even azimuth distribution.", 0f, 1f),

            Float(TreeControlSection.PrimaryBranchShape, "tree.branch.primary.length", "branchLength", "Branch Length", "Baseline primary branch length relative to total tree height.", 0.05f, 1f),
            Float(TreeControlSection.PrimaryBranchShape, "tree.branch.primary.thickness", "branchThickness", "Branch Thickness", "Primary branch base radius relative to its parent trunk radius.", 0.05f, 1f),
            Float(TreeControlSection.PrimaryBranchShape, "tree.branch.primary.elevation", "branchElevation", "Branch Elevation", "Initial branch launch angle in degrees. Negative points down; positive points up.", -90f, 90f),
            Float(TreeControlSection.PrimaryBranchShape, "tree.branch.primary.curvature", "branchCurvature", "Branch Curvature", "Primary branch centreline curvature after launch. Secondary and tertiary branches inherit a reduced proportion.", 0f, 1f),

            Integer(TreeControlSection.BranchHierarchy, "tree.branch.hierarchy.maximum-order", "maximumBranchOrder", "Maximum Branch Order", "One generates primaries only; two adds secondaries; three permits tertiaries.", 1, 3),
            Float(TreeControlSection.BranchHierarchy, "tree.branch.hierarchy.secondary-density", "secondaryDensity", "Secondary Density", "Expected secondary children per eligible primary. A fractional part is a deterministic chance of one additional child.", 0f, 8f),
            Float(TreeControlSection.BranchHierarchy, "tree.branch.hierarchy.tertiary-density", "tertiaryDensity", "Tertiary Density", "Expected tertiary children per eligible secondary. A fractional part is a deterministic chance of one additional child.", 0f, 8f),
            Float(TreeControlSection.BranchHierarchy, "tree.branch.hierarchy.child-scale", "childScale", "Child Scale", "Successive-order branch length ratio. Child thickness is derived allometrically from this value.", 0.05f, 0.90f),

            Float(TreeControlSection.Damage, "tree.damage.missing-chance", "missingBranchChance", "Missing Branch Chance", "Probability that a candidate branch is removed before geometry creation.", 0f, 1f),
            Float(TreeControlSection.Damage, "tree.damage.dead-chance", "deadBranchChance", "Dead Branch Chance", "Probability that an accepted branch becomes visibly dead geometry: darker bark, increased wind stiffness, and later foliage exclusion.", 0f, 1f),
            Float(TreeControlSection.Damage, "tree.damage.broken-chance", "brokenBranchChance", "Broken Branch Chance", "Probability that an accepted branch is shortened or truncated.", 0f, 1f),

            Color(TreeControlSection.Appearance, "tree.appearance.bark-tint", "barkTint", "Bark Tint", "Opaque per-tree bark RGB tint interval. Alpha is fixed to one; shared materials retain texture and surface-response ownership."),

            Float(TreeControlSection.AdvancedTrunkDetail, "tree.advanced.trunk.bend-frequency", "bendFrequency", "Bend Frequency", "Number of lateral trunk bend cycles from root to tip.", 0f, 6f),
            Float(TreeControlSection.AdvancedTrunkDetail, "tree.advanced.trunk.drift", "trunkDrift", "Trunk Drift", "Coherent cumulative deterministic horizontal random walk.", 0f, 0.50f),
            Float(TreeControlSection.AdvancedTrunkDetail, "tree.advanced.trunk.roughness", "trunkRoughness", "Trunk Roughness", "Local deterministic trunk control-point jitter.", 0f, 0.50f),

            Float(TreeControlSection.AdvancedBranchDistribution, "tree.advanced.branch.directional-bias", "directionalBias", "Directional Bias", "Blend amount toward one preferred horizontal branch direction.", 0f, 1f),
            Angle(TreeControlSection.AdvancedBranchDistribution, "tree.advanced.branch.directional-bias-angle", "directionalBiasAngle", "Directional Bias Angle", "Preferred tree-local branch direction in degrees. This interval may wrap through zero.", 0f, 360f),
            Float(TreeControlSection.AdvancedBranchDistribution, "tree.advanced.branch.tier-spacing", "tierSpacing", "Tier Spacing", "Target normalized vertical spacing between explicit branch tiers. Zero disables tiering; emitted spacing spans the full authored attachment band and is reported.", 0f, 0.50f),

            Float(TreeControlSection.AdvancedPrimaryBranchDetail, "tree.advanced.branch.arch", "branchArch", "Branch Arch", "One signed mid-branch arch value. Positive and negative values bend in opposite parent-tangent directions; higher orders inherit a reduced proportion.", -1f, 1f),
            Float(TreeControlSection.AdvancedPrimaryBranchDetail, "tree.advanced.branch.late-sag", "lateSag", "Late Sag", "Downward displacement applied mainly to the latter portion of the branch; higher orders inherit a reduced proportion.", 0f, 1f),
            Float(TreeControlSection.AdvancedPrimaryBranchDetail, "tree.advanced.branch.tip-upturn", "tipUpturn", "Tip Upturn", "Upward curl confined to the final 28 percent of the branch, with zero displacement and slope before the tip window.", 0f, 1f),
            Float(TreeControlSection.AdvancedPrimaryBranchDetail, "tree.advanced.branch.side-sweep", "sideSweep", "Side Sweep", "Signed lateral branch bending perpendicular to the main branch plane; higher orders inherit a reduced proportion.", -1f, 1f),

            Float(TreeControlSection.AdvancedForking, "tree.advanced.fork.chance", "forkChance", "Fork Chance", "Probability of creating one structural trunk fork. Recipe-only placement is fixed at 68 percent of trunk height and is independent of the primary branch band.", 0f, 1f)
        };

        public static IReadOnlyList<TreeControlSectionDescriptor> Sections =>
            sections;

        public static IReadOnlyList<TreeControlDescriptor> Controls =>
            controls;

        public static IEnumerable<TreeControlDescriptor> Enumerate(
            TreeControlSection section)
        {
            for (int index = 0; index < controls.Length; index++)
            {
                if (controls[index].Section == section)
                {
                    yield return controls[index];
                }
            }
        }

        private static TreeControlDescriptor Float(
            TreeControlSection section,
            string id,
            string property,
            string label,
            string tooltip,
            float minimum,
            float maximum)
        {
            return new TreeControlDescriptor(
                section,
                id,
                property,
                label,
                tooltip,
                TreeControlValueKind.Float,
                minimum,
                maximum);
        }

        private static TreeControlDescriptor Integer(
            TreeControlSection section,
            string id,
            string property,
            string label,
            string tooltip,
            int minimum,
            int maximum)
        {
            return new TreeControlDescriptor(
                section,
                id,
                property,
                label,
                tooltip,
                TreeControlValueKind.Integer,
                minimum,
                maximum);
        }

        private static TreeControlDescriptor Angle(
            TreeControlSection section,
            string id,
            string property,
            string label,
            string tooltip,
            float minimum,
            float maximum)
        {
            return new TreeControlDescriptor(
                section,
                id,
                property,
                label,
                tooltip,
                TreeControlValueKind.Angle,
                minimum,
                maximum);
        }

        private static TreeControlDescriptor Color(
            TreeControlSection section,
            string id,
            string property,
            string label,
            string tooltip)
        {
            return new TreeControlDescriptor(
                section,
                id,
                property,
                label,
                tooltip,
                TreeControlValueKind.Color,
                0f,
                1f);
        }
    }
}
