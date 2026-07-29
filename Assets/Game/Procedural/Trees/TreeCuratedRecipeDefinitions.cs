using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    public enum TreeCuratedRecipeId
    {
        AlderStandard = 0,
        AlderHighCrown = 1,
        AlderWindswept = 2,
        NorwaySpruceStandard = 3,
        NorwaySpruceHighCrown = 4,
        NorwaySpruceTall = 5,
        NorwaySpruceDrooping = 6,
        WychElmUpright = 7,
        WychElmLeaning = 8,
        DeadAlder = 9,
        DeadNorwaySpruce = 10,
        DeadWychElm = 11,
        TallDeadSnag = 12,
    }

    public enum TreeCuratedBarkMaterialKind
    {
        CommonPine = 0,
        Twisted = 1,
        Dead = 2
    }

    public sealed class TreeCuratedRecipeDefinition
    {
        internal TreeCuratedRecipeDefinition(
            TreeCuratedRecipeId id,
            string stableIdentity,
            string assetFileName,
            string displayName,
            string description,
            string[] tags,
            string referenceIntent,
            string foliageTarget,
            TreeCuratedBarkMaterialKind barkMaterialKind)
        {
            Id = id;
            StableIdentity = stableIdentity;
            AssetFileName = assetFileName;
            DisplayName = displayName;
            Description = description;
            Tags = tags ?? Array.Empty<string>();
            ReferenceIntent = referenceIntent;
            FoliageTarget = foliageTarget;
            BarkMaterialKind = barkMaterialKind;
        }

        public TreeCuratedRecipeId Id { get; }
        public string StableIdentity { get; }
        public string AssetFileName { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public IReadOnlyList<string> Tags { get; }
        public string ReferenceIntent { get; }
        public string FoliageTarget { get; }
        public TreeCuratedBarkMaterialKind BarkMaterialKind { get; }

        public TreeRecipeControlRanges CreateControlRanges()
        {
            return TreeCuratedRecipeDefinitions.CreateControlRanges(Id);
        }
    }

    public static class TreeCuratedRecipeDefinitions
    {
        public const int ExpectedRecipeCount = 13;
        public const int ExpectedControlCount = 42;

        private static readonly TreeCuratedRecipeDefinition[] definitions =
        {
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.AlderStandard,
                "tree-recipe-curated-alder-standard",
                "TR_Alder_Standard",
                "Alder Standard",
                "Baseline living alder with a dense rounded crown distributed through a broad branch band.",
                new[] { "alder", "broadleaf", "living", "standard" },
                "Broadleaf references 1–2; reference 3 may also fit.",
                "Dense rounded crown distributed through the upper 65–75% of the branch band.",
                TreeCuratedBarkMaterialKind.CommonPine),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.AlderHighCrown,
                "tree-recipe-curated-alder-high-crown",
                "TR_Alder_HighCrown",
                "Alder High-Crown",
                "Tall curved alder with an exposed lower trunk and a structurally high crown.",
                new[] { "alder", "broadleaf", "living", "high-crown", "exposed-trunk" },
                "Broadleaf reference 4.",
                "Foliage mostly above 55–65% of tree height; lower accepted branches remain exposed or dead.",
                TreeCuratedBarkMaterialKind.CommonPine),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.AlderWindswept,
                "tree-recipe-curated-alder-windswept",
                "TR_Alder_Windswept",
                "Alder Windswept",
                "Asymmetrical alder with coherent trunk lean and branch bias in one tree-local direction.",
                new[] { "alder", "broadleaf", "living", "windswept", "asymmetrical" },
                "Broadleaf reference 5; reference 3 may partly fit.",
                "Foliage occupancy biased toward the same local direction as Lean Direction and Directional Bias.",
                TreeCuratedBarkMaterialKind.CommonPine),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.NorwaySpruceStandard,
                "tree-recipe-curated-norway-spruce-standard",
                "TR_NorwaySpruce_Standard",
                "Norway Spruce Standard",
                "Baseline living Norway spruce with a continuous conical tiered structure.",
                new[] { "norway-spruce", "spruce", "conifer", "living", "standard" },
                "Conifer references 1, 2 and 5.",
                "Continuous conical foliage tiers from low trunk to tip.",
                TreeCuratedBarkMaterialKind.CommonPine),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.NorwaySpruceHighCrown,
                "tree-recipe-curated-norway-spruce-high-crown",
                "TR_NorwaySpruce_HighCrown",
                "Norway Spruce High-Crown",
                "Norway spruce with a substantial exposed lower trunk and upper conical crown.",
                new[] { "norway-spruce", "spruce", "conifer", "living", "high-crown", "exposed-trunk" },
                "Conifer reference 3.",
                "Lower 35–50% of trunk exposed; upper crown remains conical.",
                TreeCuratedBarkMaterialKind.CommonPine),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.NorwaySpruceTall,
                "tree-recipe-curated-norway-spruce-tall",
                "TR_NorwaySpruce_Tall",
                "Norway Spruce Tall",
                "Tall narrow Norway spruce with regular high-density tiers and a vertically extended crown.",
                new[] { "norway-spruce", "spruce", "conifer", "living", "tall" },
                "Conifer reference 4.",
                "Narrower, vertically extended conical crown.",
                TreeCuratedBarkMaterialKind.CommonPine),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.NorwaySpruceDrooping,
                "tree-recipe-curated-norway-spruce-drooping",
                "TR_NorwaySpruce_Drooping",
                "Norway Spruce Drooping",
                "Norway spruce whose primary branches launch downward and continue into strong late sag.",
                new[] { "norway-spruce", "spruce", "conifer", "living", "drooping", "branches-down" },
                "A deliberate additional archetype emphasizing downward branch launch.",
                "Foliage follows the downward branch silhouette while retaining small tip upturns.",
                TreeCuratedBarkMaterialKind.CommonPine),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.WychElmUpright,
                "tree-recipe-curated-wych-elm-upright",
                "TR_WychElm_Upright",
                "Wych Elm Upright",
                "Old upright Wych elm with strong axial twist, centreline spiral and broad forked structure.",
                new[] { "wych-elm", "elm", "broadleaf", "living", "twisted", "upright" },
                "Twisted references 2–5.",
                "Large separated crown masses following forks and upper primary branches.",
                TreeCuratedBarkMaterialKind.Twisted),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.WychElmLeaning,
                "tree-recipe-curated-wych-elm-leaning",
                "TR_WychElm_Leaning",
                "Wych Elm Leaning",
                "Contorted Wych elm combining strong twist with coherent lean and directional branch bias.",
                new[] { "wych-elm", "elm", "broadleaf", "living", "twisted", "leaning", "asymmetrical" },
                "Twisted reference 1 and the close leaning specimen.",
                "Crown mass biased in the local lean direction.",
                TreeCuratedBarkMaterialKind.Twisted),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.DeadAlder,
                "tree-recipe-curated-dead-alder",
                "TR_Dead_Alder",
                "Dead Alder",
                "Leafless dead alder with irregular broadleaf branching, missing limbs and broken tips.",
                new[] { "dead", "alder", "broadleaf", "leafless" },
                "The shorter, broad, irregular dead references.",
                "No foliage.",
                TreeCuratedBarkMaterialKind.Dead),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.DeadNorwaySpruce,
                "tree-recipe-curated-dead-norway-spruce",
                "TR_Dead_NorwaySpruce",
                "Dead Norway Spruce",
                "Leafless dead Norway spruce with sparse tiered branches angled strongly downward.",
                new[] { "dead", "norway-spruce", "spruce", "conifer", "leafless", "branches-down" },
                "The tall conifer-like dead references with downward tiers.",
                "No foliage.",
                TreeCuratedBarkMaterialKind.Dead),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.DeadWychElm,
                "tree-recipe-curated-dead-wych-elm",
                "TR_Dead_WychElm",
                "Dead Wych Elm",
                "Leafless contorted Wych elm with forked asymmetry, heavy twist and structural damage.",
                new[] { "dead", "wych-elm", "elm", "broadleaf", "leafless", "twisted" },
                "The most contorted, forked dead broadleaf reference.",
                "No foliage.",
                TreeCuratedBarkMaterialKind.Dead),
            new TreeCuratedRecipeDefinition(
                TreeCuratedRecipeId.TallDeadSnag,
                "tree-recipe-curated-tall-dead-snag",
                "TR_TallDeadSnag",
                "Tall Dead Snag",
                "Tall narrow dead snag with sparse upper tiers, severe breakage and no foliage.",
                new[] { "dead", "snag", "tall", "leafless", "sparse" },
                "The last tall sparse dead specimens.",
                "No foliage.",
                TreeCuratedBarkMaterialKind.Dead)
        };

        public static IReadOnlyList<TreeCuratedRecipeDefinition> All =>
            definitions;

        public static bool TryFindByStableIdentity(
            string stableIdentity,
            out TreeCuratedRecipeDefinition definition)
        {
            for (int index = 0; index < definitions.Length; index++)
            {
                TreeCuratedRecipeDefinition candidate = definitions[index];
                if (string.Equals(
                    candidate.StableIdentity,
                    stableIdentity,
                    StringComparison.Ordinal))
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        internal static TreeRecipeControlRanges CreateControlRanges(
            TreeCuratedRecipeId id)
        {
            return id switch
            {
                TreeCuratedRecipeId.AlderStandard => CreateAlderStandardRanges(),
                TreeCuratedRecipeId.AlderHighCrown => CreateAlderHighCrownRanges(),
                TreeCuratedRecipeId.AlderWindswept => CreateAlderWindsweptRanges(),
                TreeCuratedRecipeId.NorwaySpruceStandard => CreateNorwaySpruceStandardRanges(),
                TreeCuratedRecipeId.NorwaySpruceHighCrown => CreateNorwaySpruceHighCrownRanges(),
                TreeCuratedRecipeId.NorwaySpruceTall => CreateNorwaySpruceTallRanges(),
                TreeCuratedRecipeId.NorwaySpruceDrooping => CreateNorwaySpruceDroopingRanges(),
                TreeCuratedRecipeId.WychElmUpright => CreateWychElmUprightRanges(),
                TreeCuratedRecipeId.WychElmLeaning => CreateWychElmLeaningRanges(),
                TreeCuratedRecipeId.DeadAlder => CreateDeadAlderRanges(),
                TreeCuratedRecipeId.DeadNorwaySpruce => CreateDeadNorwaySpruceRanges(),
                TreeCuratedRecipeId.DeadWychElm => CreateDeadWychElmRanges(),
                TreeCuratedRecipeId.TallDeadSnag => CreateTallDeadSnagRanges(),
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }

        private static TreeRecipeControlRanges CreateAlderStandardRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(6.5f, 9.5f),
                new TreeFloatControlRange(0.28f, 0.48f),
                new TreeFloatControlRange(0.72f, 0.86f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.08f, 0.22f),
                new TreeFloatControlRange(0f, 0.06f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0f, 0.015f),
                new TreeFloatControlRange(0f, 0.2f),
                new TreeFloatControlRange(-20f, 20f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 6),
                new TreeFloatControlRange(0.5f, 0.72f),
                new TreeFloatControlRange(0.42f, 0.55f),
                new TreeFloatControlRange(0.13f, 0.19f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(10, 16),
                new TreeFloatControlRange(0.22f, 0.3f),
                new TreeFloatControlRange(0.84f, 0.94f),
                new TreeFloatControlRange(0.45f, 0.72f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.28f, 0.42f),
                new TreeFloatControlRange(0.32f, 0.48f),
                new TreeFloatControlRange(6f, 24f),
                new TreeFloatControlRange(0.1f, 0.24f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(3, 3),
                new TreeFloatControlRange(1.6f, 2.5f),
                new TreeFloatControlRange(0.6f, 1.4f),
                new TreeFloatControlRange(0.38f, 0.5f));
            ranges.SetDamage(
                new TreeFloatControlRange(0f, 0.04f),
                new TreeFloatControlRange(0f, 0.03f),
                new TreeFloatControlRange(0f, 0.02f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(0.8f, 1.6f),
                new TreeFloatControlRange(0.02f, 0.08f),
                new TreeFloatControlRange(0.02f, 0.08f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0f, 0.1f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0f, 0f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(-0.08f, 0.12f),
                new TreeFloatControlRange(0.03f, 0.14f),
                new TreeFloatControlRange(0.02f, 0.12f),
                new TreeFloatControlRange(-0.08f, 0.08f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0.02f, 0.12f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateAlderHighCrownRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(8f, 11f),
                new TreeFloatControlRange(0.28f, 0.48f),
                new TreeFloatControlRange(0.76f, 0.9f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.18f, 0.34f),
                new TreeFloatControlRange(0.02f, 0.1f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0.01f, 0.04f),
                new TreeFloatControlRange(0.1f, 0.35f),
                new TreeFloatControlRange(-20f, 20f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 6),
                new TreeFloatControlRange(0.5f, 0.72f),
                new TreeFloatControlRange(0.42f, 0.55f),
                new TreeFloatControlRange(0.13f, 0.19f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(7, 11),
                new TreeFloatControlRange(0.52f, 0.66f),
                new TreeFloatControlRange(0.88f, 0.98f),
                new TreeFloatControlRange(0.35f, 0.62f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.32f, 0.48f),
                new TreeFloatControlRange(0.3f, 0.46f),
                new TreeFloatControlRange(8f, 28f),
                new TreeFloatControlRange(0.14f, 0.3f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(3, 3),
                new TreeFloatControlRange(1.2f, 2f),
                new TreeFloatControlRange(0.3f, 0.9f),
                new TreeFloatControlRange(0.38f, 0.5f));
            ranges.SetDamage(
                new TreeFloatControlRange(0.08f, 0.16f),
                new TreeFloatControlRange(0.08f, 0.18f),
                new TreeFloatControlRange(0.04f, 0.1f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(0.8f, 1.6f),
                new TreeFloatControlRange(0.05f, 0.12f),
                new TreeFloatControlRange(0.02f, 0.08f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0f, 0.1f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0f, 0f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(-0.08f, 0.12f),
                new TreeFloatControlRange(0.02f, 0.1f),
                new TreeFloatControlRange(0.04f, 0.15f),
                new TreeFloatControlRange(-0.08f, 0.08f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0.1f, 0.25f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateAlderWindsweptRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(7f, 10f),
                new TreeFloatControlRange(0.28f, 0.48f),
                new TreeFloatControlRange(0.72f, 0.86f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.16f, 0.3f),
                new TreeFloatControlRange(0.12f, 0.24f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0f, 0.015f),
                new TreeFloatControlRange(0f, 0.2f),
                new TreeFloatControlRange(-20f, 20f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 6),
                new TreeFloatControlRange(0.55f, 0.8f),
                new TreeFloatControlRange(0.4f, 0.52f),
                new TreeFloatControlRange(0.14f, 0.2f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(9, 14),
                new TreeFloatControlRange(0.24f, 0.34f),
                new TreeFloatControlRange(0.82f, 0.94f),
                new TreeFloatControlRange(0.18f, 0.4f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.3f, 0.46f),
                new TreeFloatControlRange(0.32f, 0.48f),
                new TreeFloatControlRange(6f, 24f),
                new TreeFloatControlRange(0.16f, 0.32f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(3, 3),
                new TreeFloatControlRange(1.6f, 2.5f),
                new TreeFloatControlRange(0.6f, 1.4f),
                new TreeFloatControlRange(0.38f, 0.5f));
            ranges.SetDamage(
                new TreeFloatControlRange(0f, 0.04f),
                new TreeFloatControlRange(0f, 0.03f),
                new TreeFloatControlRange(0f, 0.02f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(0.8f, 1.6f),
                new TreeFloatControlRange(0.02f, 0.08f),
                new TreeFloatControlRange(0.02f, 0.08f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0.55f, 0.8f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0f, 0f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(0.05f, 0.2f),
                new TreeFloatControlRange(0.05f, 0.18f),
                new TreeFloatControlRange(0.02f, 0.12f),
                new TreeFloatControlRange(0.1f, 0.28f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0.02f, 0.12f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateNorwaySpruceStandardRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(7.5f, 11.5f),
                new TreeFloatControlRange(0.22f, 0.4f),
                new TreeFloatControlRange(0.82f, 0.93f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.02f, 0.1f),
                new TreeFloatControlRange(0f, 0.035f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0f, 0.008f),
                new TreeFloatControlRange(0f, 0.1f),
                new TreeFloatControlRange(-8f, 8f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 5),
                new TreeFloatControlRange(0.24f, 0.38f),
                new TreeFloatControlRange(0.28f, 0.4f),
                new TreeFloatControlRange(0.08f, 0.14f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(20, 30),
                new TreeFloatControlRange(0.16f, 0.24f),
                new TreeFloatControlRange(0.9f, 0.98f),
                new TreeFloatControlRange(0.82f, 0.96f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.24f, 0.36f),
                new TreeFloatControlRange(0.18f, 0.3f),
                new TreeFloatControlRange(-10f, 4f),
                new TreeFloatControlRange(0.04f, 0.14f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(3, 3),
                new TreeFloatControlRange(0.8f, 1.5f),
                new TreeFloatControlRange(0.3f, 0.8f),
                new TreeFloatControlRange(0.32f, 0.42f));
            ranges.SetDamage(
                new TreeFloatControlRange(0f, 0.03f),
                new TreeFloatControlRange(0.02f, 0.07f),
                new TreeFloatControlRange(0f, 0.03f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(0.4f, 1f),
                new TreeFloatControlRange(0f, 0.03f),
                new TreeFloatControlRange(0.01f, 0.04f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0f, 0.04f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0.07f, 0.11f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(-0.05f, 0.05f),
                new TreeFloatControlRange(0.12f, 0.28f),
                new TreeFloatControlRange(0f, 0.05f),
                new TreeFloatControlRange(-0.03f, 0.03f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0f, 0.02f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateNorwaySpruceHighCrownRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(8.5f, 12.5f),
                new TreeFloatControlRange(0.22f, 0.4f),
                new TreeFloatControlRange(0.82f, 0.93f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.02f, 0.1f),
                new TreeFloatControlRange(0f, 0.035f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0f, 0.008f),
                new TreeFloatControlRange(0f, 0.1f),
                new TreeFloatControlRange(-8f, 8f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 5),
                new TreeFloatControlRange(0.24f, 0.38f),
                new TreeFloatControlRange(0.28f, 0.4f),
                new TreeFloatControlRange(0.08f, 0.14f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(14, 22),
                new TreeFloatControlRange(0.4f, 0.56f),
                new TreeFloatControlRange(0.91f, 0.99f),
                new TreeFloatControlRange(0.82f, 0.96f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.24f, 0.38f),
                new TreeFloatControlRange(0.18f, 0.3f),
                new TreeFloatControlRange(-10f, 4f),
                new TreeFloatControlRange(0.04f, 0.14f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(3, 3),
                new TreeFloatControlRange(0.8f, 1.5f),
                new TreeFloatControlRange(0.3f, 0.8f),
                new TreeFloatControlRange(0.32f, 0.42f));
            ranges.SetDamage(
                new TreeFloatControlRange(0.08f, 0.18f),
                new TreeFloatControlRange(0.1f, 0.22f),
                new TreeFloatControlRange(0.04f, 0.1f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(0.4f, 1f),
                new TreeFloatControlRange(0f, 0.03f),
                new TreeFloatControlRange(0.01f, 0.04f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0f, 0.04f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0.08f, 0.12f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(-0.05f, 0.05f),
                new TreeFloatControlRange(0.1f, 0.25f),
                new TreeFloatControlRange(0f, 0.05f),
                new TreeFloatControlRange(-0.03f, 0.03f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0f, 0.02f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateNorwaySpruceTallRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(11.5f, 15.5f),
                new TreeFloatControlRange(0.24f, 0.42f),
                new TreeFloatControlRange(0.86f, 0.95f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.02f, 0.08f),
                new TreeFloatControlRange(0f, 0.035f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0f, 0.008f),
                new TreeFloatControlRange(0f, 0.1f),
                new TreeFloatControlRange(-8f, 8f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 5),
                new TreeFloatControlRange(0.26f, 0.4f),
                new TreeFloatControlRange(0.28f, 0.4f),
                new TreeFloatControlRange(0.08f, 0.14f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(24, 36),
                new TreeFloatControlRange(0.2f, 0.3f),
                new TreeFloatControlRange(0.94f, 0.99f),
                new TreeFloatControlRange(0.88f, 0.98f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.2f, 0.32f),
                new TreeFloatControlRange(0.16f, 0.28f),
                new TreeFloatControlRange(-10f, 4f),
                new TreeFloatControlRange(0.04f, 0.14f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(3, 3),
                new TreeFloatControlRange(0.8f, 1.5f),
                new TreeFloatControlRange(0.3f, 0.8f),
                new TreeFloatControlRange(0.32f, 0.42f));
            ranges.SetDamage(
                new TreeFloatControlRange(0f, 0.03f),
                new TreeFloatControlRange(0.02f, 0.07f),
                new TreeFloatControlRange(0f, 0.03f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(0.4f, 1f),
                new TreeFloatControlRange(0f, 0.03f),
                new TreeFloatControlRange(0.01f, 0.04f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0f, 0.04f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0.07f, 0.1f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(-0.05f, 0.05f),
                new TreeFloatControlRange(0.12f, 0.28f),
                new TreeFloatControlRange(0f, 0.05f),
                new TreeFloatControlRange(-0.03f, 0.03f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0f, 0.02f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateNorwaySpruceDroopingRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(8f, 12f),
                new TreeFloatControlRange(0.22f, 0.4f),
                new TreeFloatControlRange(0.82f, 0.93f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.02f, 0.1f),
                new TreeFloatControlRange(0f, 0.035f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0f, 0.008f),
                new TreeFloatControlRange(0f, 0.1f),
                new TreeFloatControlRange(-8f, 8f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 5),
                new TreeFloatControlRange(0.24f, 0.38f),
                new TreeFloatControlRange(0.28f, 0.4f),
                new TreeFloatControlRange(0.08f, 0.14f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(18, 28),
                new TreeFloatControlRange(0.18f, 0.28f),
                new TreeFloatControlRange(0.88f, 0.97f),
                new TreeFloatControlRange(0.82f, 0.96f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.28f, 0.42f),
                new TreeFloatControlRange(0.18f, 0.3f),
                new TreeFloatControlRange(-28f, -14f),
                new TreeFloatControlRange(0.06f, 0.18f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(3, 3),
                new TreeFloatControlRange(0.8f, 1.5f),
                new TreeFloatControlRange(0.3f, 0.8f),
                new TreeFloatControlRange(0.32f, 0.42f));
            ranges.SetDamage(
                new TreeFloatControlRange(0f, 0.03f),
                new TreeFloatControlRange(0.02f, 0.07f),
                new TreeFloatControlRange(0f, 0.03f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(0.4f, 1f),
                new TreeFloatControlRange(0f, 0.03f),
                new TreeFloatControlRange(0.01f, 0.04f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0f, 0.04f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0.07f, 0.12f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(-0.05f, 0.05f),
                new TreeFloatControlRange(0.38f, 0.62f),
                new TreeFloatControlRange(0.02f, 0.08f),
                new TreeFloatControlRange(-0.03f, 0.03f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0f, 0.02f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateWychElmUprightRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(9f, 14f),
                new TreeFloatControlRange(0.55f, 0.95f),
                new TreeFloatControlRange(0.58f, 0.78f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.18f, 0.34f),
                new TreeFloatControlRange(0f, 0.06f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0.08f, 0.15f),
                new TreeFloatControlRange(0.75f, 1.1f),
                new TreeFloatControlRange(260f, 380f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 6),
                new TreeFloatControlRange(0.7f, 0.95f),
                new TreeFloatControlRange(0.38f, 0.5f),
                new TreeFloatControlRange(0.18f, 0.25f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(10, 15),
                new TreeFloatControlRange(0.18f, 0.28f),
                new TreeFloatControlRange(0.82f, 0.94f),
                new TreeFloatControlRange(0.52f, 0.76f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.32f, 0.48f),
                new TreeFloatControlRange(0.38f, 0.55f),
                new TreeFloatControlRange(10f, 32f),
                new TreeFloatControlRange(0.2f, 0.38f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(3, 3),
                new TreeFloatControlRange(2.5f, 4f),
                new TreeFloatControlRange(1f, 2f),
                new TreeFloatControlRange(0.4f, 0.52f));
            ranges.SetDamage(
                new TreeFloatControlRange(0.03f, 0.08f),
                new TreeFloatControlRange(0.02f, 0.06f),
                new TreeFloatControlRange(0.02f, 0.06f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(1.2f, 2.2f),
                new TreeFloatControlRange(0.05f, 0.13f),
                new TreeFloatControlRange(0.04f, 0.12f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0f, 0.12f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0f, 0f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(0.08f, 0.28f),
                new TreeFloatControlRange(0.02f, 0.12f),
                new TreeFloatControlRange(0.06f, 0.18f),
                new TreeFloatControlRange(-0.14f, 0.14f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0.1f, 0.28f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateWychElmLeaningRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(9f, 15f),
                new TreeFloatControlRange(0.55f, 0.95f),
                new TreeFloatControlRange(0.58f, 0.78f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.26f, 0.48f),
                new TreeFloatControlRange(0.18f, 0.32f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0.1f, 0.2f),
                new TreeFloatControlRange(0.85f, 1.25f),
                new TreeFloatControlRange(300f, 460f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 6),
                new TreeFloatControlRange(0.78f, 1.05f),
                new TreeFloatControlRange(0.38f, 0.52f),
                new TreeFloatControlRange(0.2f, 0.28f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(9, 14),
                new TreeFloatControlRange(0.18f, 0.3f),
                new TreeFloatControlRange(0.8f, 0.94f),
                new TreeFloatControlRange(0.28f, 0.55f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.34f, 0.52f),
                new TreeFloatControlRange(0.38f, 0.55f),
                new TreeFloatControlRange(10f, 32f),
                new TreeFloatControlRange(0.24f, 0.44f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(3, 3),
                new TreeFloatControlRange(2.5f, 4f),
                new TreeFloatControlRange(1f, 2f),
                new TreeFloatControlRange(0.4f, 0.52f));
            ranges.SetDamage(
                new TreeFloatControlRange(0.03f, 0.08f),
                new TreeFloatControlRange(0.02f, 0.06f),
                new TreeFloatControlRange(0.02f, 0.06f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(1.2f, 2.2f),
                new TreeFloatControlRange(0.05f, 0.13f),
                new TreeFloatControlRange(0.04f, 0.12f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0.3f, 0.55f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0f, 0f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(0.08f, 0.28f),
                new TreeFloatControlRange(0.02f, 0.12f),
                new TreeFloatControlRange(0.06f, 0.18f),
                new TreeFloatControlRange(0.1f, 0.28f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0.14f, 0.35f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateDeadAlderRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(6.5f, 10.5f),
                new TreeFloatControlRange(0.28f, 0.52f),
                new TreeFloatControlRange(0.65f, 0.84f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.12f, 0.3f),
                new TreeFloatControlRange(0f, 0.1f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0f, 0.015f),
                new TreeFloatControlRange(0f, 0.2f),
                new TreeFloatControlRange(-20f, 20f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 6),
                new TreeFloatControlRange(0.5f, 0.72f),
                new TreeFloatControlRange(0.42f, 0.55f),
                new TreeFloatControlRange(0.13f, 0.19f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(8, 15),
                new TreeFloatControlRange(0.18f, 0.3f),
                new TreeFloatControlRange(0.78f, 0.94f),
                new TreeFloatControlRange(0.3f, 0.62f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.3f, 0.48f),
                new TreeFloatControlRange(0.26f, 0.44f),
                new TreeFloatControlRange(5f, 28f),
                new TreeFloatControlRange(0.12f, 0.3f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(2, 3),
                new TreeFloatControlRange(0.8f, 1.8f),
                new TreeFloatControlRange(0f, 0.6f),
                new TreeFloatControlRange(0.34f, 0.46f));
            ranges.SetDamage(
                new TreeFloatControlRange(0.12f, 0.25f),
                new TreeFloatControlRange(0.85f, 1f),
                new TreeFloatControlRange(0.15f, 0.35f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(0.8f, 1.6f),
                new TreeFloatControlRange(0.02f, 0.08f),
                new TreeFloatControlRange(0.02f, 0.08f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0f, 0.1f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0f, 0f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(-0.08f, 0.12f),
                new TreeFloatControlRange(0.03f, 0.14f),
                new TreeFloatControlRange(0.02f, 0.12f),
                new TreeFloatControlRange(-0.08f, 0.08f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0.08f, 0.2f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateDeadNorwaySpruceRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(8f, 14f),
                new TreeFloatControlRange(0.2f, 0.42f),
                new TreeFloatControlRange(0.84f, 0.96f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.02f, 0.1f),
                new TreeFloatControlRange(0f, 0.035f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0f, 0.008f),
                new TreeFloatControlRange(0f, 0.1f),
                new TreeFloatControlRange(-8f, 8f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 5),
                new TreeFloatControlRange(0.24f, 0.38f),
                new TreeFloatControlRange(0.28f, 0.4f),
                new TreeFloatControlRange(0.08f, 0.14f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(14, 26),
                new TreeFloatControlRange(0.18f, 0.3f),
                new TreeFloatControlRange(0.92f, 0.99f),
                new TreeFloatControlRange(0.82f, 0.98f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.28f, 0.42f),
                new TreeFloatControlRange(0.16f, 0.28f),
                new TreeFloatControlRange(-30f, -14f),
                new TreeFloatControlRange(0.04f, 0.14f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(2, 2),
                new TreeFloatControlRange(0.4f, 1.2f),
                new TreeFloatControlRange(0f, 0f),
                new TreeFloatControlRange(0.28f, 0.4f));
            ranges.SetDamage(
                new TreeFloatControlRange(0.18f, 0.35f),
                new TreeFloatControlRange(0.95f, 1f),
                new TreeFloatControlRange(0.2f, 0.45f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(0.4f, 1f),
                new TreeFloatControlRange(0f, 0.03f),
                new TreeFloatControlRange(0.01f, 0.04f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0f, 0.04f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0.08f, 0.13f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(-0.05f, 0.05f),
                new TreeFloatControlRange(0.35f, 0.6f),
                new TreeFloatControlRange(0f, 0.04f),
                new TreeFloatControlRange(-0.03f, 0.03f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0f, 0f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateDeadWychElmRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(8.5f, 14.5f),
                new TreeFloatControlRange(0.5f, 1f),
                new TreeFloatControlRange(0.55f, 0.8f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.22f, 0.45f),
                new TreeFloatControlRange(0.05f, 0.22f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0.08f, 0.18f),
                new TreeFloatControlRange(0.75f, 1.2f),
                new TreeFloatControlRange(260f, 440f));
            ranges.SetRoots(
                new TreeIntControlRange(5, 6),
                new TreeFloatControlRange(0.7f, 0.95f),
                new TreeFloatControlRange(0.38f, 0.5f),
                new TreeFloatControlRange(0.18f, 0.25f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(8, 14),
                new TreeFloatControlRange(0.18f, 0.3f),
                new TreeFloatControlRange(0.8f, 0.94f),
                new TreeFloatControlRange(0.3f, 0.6f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.34f, 0.54f),
                new TreeFloatControlRange(0.34f, 0.54f),
                new TreeFloatControlRange(8f, 34f),
                new TreeFloatControlRange(0.2f, 0.45f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(2, 3),
                new TreeFloatControlRange(1.2f, 2.5f),
                new TreeFloatControlRange(0.2f, 0.8f),
                new TreeFloatControlRange(0.38f, 0.5f));
            ranges.SetDamage(
                new TreeFloatControlRange(0.15f, 0.3f),
                new TreeFloatControlRange(0.9f, 1f),
                new TreeFloatControlRange(0.18f, 0.4f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(1.2f, 2.2f),
                new TreeFloatControlRange(0.05f, 0.13f),
                new TreeFloatControlRange(0.04f, 0.12f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0.15f, 0.4f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0f, 0f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(0.08f, 0.28f),
                new TreeFloatControlRange(0.02f, 0.12f),
                new TreeFloatControlRange(0.06f, 0.18f),
                new TreeFloatControlRange(0.06f, 0.22f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0.15f, 0.35f));
            ranges.ValidateAndClamp();
            return ranges;
        }
        private static TreeRecipeControlRanges CreateTallDeadSnagRanges()
        {
            var ranges = new TreeRecipeControlRanges();
            ranges.SetOverallForm(
                new TreeFloatControlRange(12f, 18f),
                new TreeFloatControlRange(0.26f, 0.52f),
                new TreeFloatControlRange(0.88f, 0.97f));
            ranges.SetTrunkShape(
                new TreeFloatControlRange(0.03f, 0.12f),
                new TreeFloatControlRange(0f, 0.06f),
                new TreeAngleControlRange(0f, 0f));
            ranges.SetTrunkSpiralAndTwist(
                new TreeFloatControlRange(0f, 0.01f),
                new TreeFloatControlRange(0f, 0.1f),
                new TreeFloatControlRange(-15f, 15f));
            ranges.SetRoots(
                new TreeIntControlRange(4, 5),
                new TreeFloatControlRange(0.32f, 0.5f),
                new TreeFloatControlRange(0.3f, 0.42f),
                new TreeFloatControlRange(0.1f, 0.16f));
            ranges.SetPrimaryBranchPlacement(
                new TreeIntControlRange(10, 18),
                new TreeFloatControlRange(0.28f, 0.42f),
                new TreeFloatControlRange(0.94f, 0.99f),
                new TreeFloatControlRange(0.76f, 0.94f));
            ranges.SetPrimaryBranchShape(
                new TreeFloatControlRange(0.2f, 0.36f),
                new TreeFloatControlRange(0.16f, 0.28f),
                new TreeFloatControlRange(-8f, 10f),
                new TreeFloatControlRange(0.03f, 0.12f));
            ranges.SetBranchHierarchy(
                new TreeIntControlRange(2, 2),
                new TreeFloatControlRange(0.3f, 0.9f),
                new TreeFloatControlRange(0f, 0f),
                new TreeFloatControlRange(0.28f, 0.38f));
            ranges.SetDamage(
                new TreeFloatControlRange(0.25f, 0.45f),
                new TreeFloatControlRange(1f, 1f),
                new TreeFloatControlRange(0.3f, 0.55f));
            ranges.SetAppearance(
                new TreeColorControlRange(Color.white, Color.white));
            ranges.SetAdvancedTrunkDetail(
                new TreeFloatControlRange(0.3f, 0.8f),
                new TreeFloatControlRange(0f, 0.04f),
                new TreeFloatControlRange(0.01f, 0.05f));
            ranges.SetAdvancedBranchDistribution(
                new TreeFloatControlRange(0f, 0.05f),
                new TreeAngleControlRange(0f, 0f),
                new TreeFloatControlRange(0.08f, 0.14f));
            ranges.SetAdvancedPrimaryBranchDetail(
                new TreeFloatControlRange(-0.05f, 0.05f),
                new TreeFloatControlRange(0.08f, 0.22f),
                new TreeFloatControlRange(0f, 0.04f),
                new TreeFloatControlRange(-0.03f, 0.03f));
            ranges.SetAdvancedForking(
                new TreeFloatControlRange(0f, 0.04f));
            ranges.ValidateAndClamp();
            return ranges;
        }
    }

    public sealed partial class TreeRecipeControlRanges
    {
        internal void SetOverallForm(
            TreeFloatControlRange heightRange,
            TreeFloatControlRange trunkBaseRadiusRange,
            TreeFloatControlRange trunkTaperRange)
        {
            height = heightRange;
            trunkBaseRadius = trunkBaseRadiusRange;
            trunkTaper = trunkTaperRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetTrunkShape(
            TreeFloatControlRange bendAmountRange,
            TreeFloatControlRange leanAmountRange,
            TreeAngleControlRange leanDirectionRange)
        {
            bendAmount = bendAmountRange;
            leanAmount = leanAmountRange;
            leanDirection = leanDirectionRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetTrunkSpiralAndTwist(
            TreeFloatControlRange pathSpiralRadiusRange,
            TreeFloatControlRange signedPathSpiralTurnsRange,
            TreeFloatControlRange axialTwistRange)
        {
            pathSpiralRadius = pathSpiralRadiusRange;
            signedPathSpiralTurns = signedPathSpiralTurnsRange;
            axialTwist = axialTwistRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetRoots(
            TreeIntControlRange rootCountRange,
            TreeFloatControlRange rootReachRange,
            TreeFloatControlRange rootThicknessRange,
            TreeFloatControlRange rootHeightRange)
        {
            rootCount = rootCountRange;
            rootReach = rootReachRange;
            rootThickness = rootThicknessRange;
            rootHeight = rootHeightRange;
            buttressTransition = new TreeFloatControlRange(1f, 1f);
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetPrimaryBranchPlacement(
            TreeIntControlRange primaryBranchCountRange,
            TreeFloatControlRange branchStartHeightRange,
            TreeFloatControlRange branchEndHeightRange,
            TreeFloatControlRange branchSymmetryRange)
        {
            primaryBranchCount = primaryBranchCountRange;
            branchStartHeight = branchStartHeightRange;
            branchEndHeight = branchEndHeightRange;
            branchSymmetry = branchSymmetryRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetPrimaryBranchShape(
            TreeFloatControlRange branchLengthRange,
            TreeFloatControlRange branchThicknessRange,
            TreeFloatControlRange branchElevationRange,
            TreeFloatControlRange branchCurvatureRange)
        {
            branchLength = branchLengthRange;
            branchThickness = branchThicknessRange;
            branchElevation = branchElevationRange;
            branchCurvature = branchCurvatureRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetBranchHierarchy(
            TreeIntControlRange maximumBranchOrderRange,
            TreeFloatControlRange secondaryDensityRange,
            TreeFloatControlRange tertiaryDensityRange,
            TreeFloatControlRange childScaleRange)
        {
            maximumBranchOrder = maximumBranchOrderRange;
            secondaryDensity = secondaryDensityRange;
            tertiaryDensity = tertiaryDensityRange;
            childScale = childScaleRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetDamage(
            TreeFloatControlRange missingBranchChanceRange,
            TreeFloatControlRange deadBranchChanceRange,
            TreeFloatControlRange brokenBranchChanceRange)
        {
            missingBranchChance = missingBranchChanceRange;
            deadBranchChance = deadBranchChanceRange;
            brokenBranchChance = brokenBranchChanceRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetAppearance(
            TreeColorControlRange barkTintRange)
        {
            barkTint = barkTintRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetAdvancedTrunkDetail(
            TreeFloatControlRange bendFrequencyRange,
            TreeFloatControlRange trunkDriftRange,
            TreeFloatControlRange trunkRoughnessRange)
        {
            bendFrequency = bendFrequencyRange;
            trunkDrift = trunkDriftRange;
            trunkRoughness = trunkRoughnessRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetAdvancedBranchDistribution(
            TreeFloatControlRange directionalBiasRange,
            TreeAngleControlRange directionalBiasAngleRange,
            TreeFloatControlRange tierSpacingRange)
        {
            directionalBias = directionalBiasRange;
            directionalBiasAngle = directionalBiasAngleRange;
            tierSpacing = tierSpacingRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetAdvancedPrimaryBranchDetail(
            TreeFloatControlRange branchArchRange,
            TreeFloatControlRange lateSagRange,
            TreeFloatControlRange tipUpturnRange,
            TreeFloatControlRange sideSweepRange)
        {
            branchArch = branchArchRange;
            lateSag = lateSagRange;
            tipUpturn = tipUpturnRange;
            sideSweep = sideSweepRange;
            schemaVersion = CurrentSchemaVersion;
        }
        internal void SetAdvancedForking(
            TreeFloatControlRange forkChanceRange)
        {
            forkChance = forkChanceRange;
            schemaVersion = CurrentSchemaVersion;
        }

        public int CountMatchingControls(
            TreeRecipeControlRanges expected,
            List<string> mismatches)
        {
            if (expected == null)
            {
                mismatches?.Add("Expected control ranges are null.");
                return 0;
            }

            int matched = 0;
            Compare("Height", MatchFloat(Height, expected.Height), mismatches, ref matched);
            Compare("Trunk Base Radius", MatchFloat(TrunkBaseRadius, expected.TrunkBaseRadius), mismatches, ref matched);
            Compare("Trunk Taper", MatchFloat(TrunkTaper, expected.TrunkTaper), mismatches, ref matched);
            Compare("Bend Amount", MatchFloat(BendAmount, expected.BendAmount), mismatches, ref matched);
            Compare("Lean Amount", MatchFloat(LeanAmount, expected.LeanAmount), mismatches, ref matched);
            Compare("Lean Direction", MatchAngle(LeanDirection, expected.LeanDirection), mismatches, ref matched);
            Compare("Path Spiral Radius", MatchFloat(PathSpiralRadius, expected.PathSpiralRadius), mismatches, ref matched);
            Compare("Signed Path Spiral Turns", MatchFloat(SignedPathSpiralTurns, expected.SignedPathSpiralTurns), mismatches, ref matched);
            Compare("Axial Twist", MatchFloat(AxialTwist, expected.AxialTwist), mismatches, ref matched);
            Compare("Root Count", MatchInt(RootCount, expected.RootCount), mismatches, ref matched);
            Compare("Root Reach", MatchFloat(RootReach, expected.RootReach), mismatches, ref matched);
            Compare("Root Thickness", MatchFloat(RootThickness, expected.RootThickness), mismatches, ref matched);
            Compare("Root Height", MatchFloat(RootHeight, expected.RootHeight), mismatches, ref matched);
            Compare("Primary Branch Count", MatchInt(PrimaryBranchCount, expected.PrimaryBranchCount), mismatches, ref matched);
            Compare("Branch Start Height", MatchFloat(BranchStartHeight, expected.BranchStartHeight), mismatches, ref matched);
            Compare("Branch End Height", MatchFloat(BranchEndHeight, expected.BranchEndHeight), mismatches, ref matched);
            Compare("Branch Symmetry", MatchFloat(BranchSymmetry, expected.BranchSymmetry), mismatches, ref matched);
            Compare("Branch Length", MatchFloat(BranchLength, expected.BranchLength), mismatches, ref matched);
            Compare("Branch Thickness", MatchFloat(BranchThickness, expected.BranchThickness), mismatches, ref matched);
            Compare("Branch Elevation", MatchFloat(BranchElevation, expected.BranchElevation), mismatches, ref matched);
            Compare("Branch Curvature", MatchFloat(BranchCurvature, expected.BranchCurvature), mismatches, ref matched);
            Compare("Maximum Branch Order", MatchInt(MaximumBranchOrder, expected.MaximumBranchOrder), mismatches, ref matched);
            Compare("Secondary Density", MatchFloat(SecondaryDensity, expected.SecondaryDensity), mismatches, ref matched);
            Compare("Tertiary Density", MatchFloat(TertiaryDensity, expected.TertiaryDensity), mismatches, ref matched);
            Compare("Child Scale", MatchFloat(ChildScale, expected.ChildScale), mismatches, ref matched);
            Compare("Missing Branch Chance", MatchFloat(MissingBranchChance, expected.MissingBranchChance), mismatches, ref matched);
            Compare("Dead Branch Chance", MatchFloat(DeadBranchChance, expected.DeadBranchChance), mismatches, ref matched);
            Compare("Broken Branch Chance", MatchFloat(BrokenBranchChance, expected.BrokenBranchChance), mismatches, ref matched);
            Compare("Bark Tint", MatchColor(BarkTint, expected.BarkTint), mismatches, ref matched);
            Compare("Bend Frequency", MatchFloat(BendFrequency, expected.BendFrequency), mismatches, ref matched);
            Compare("Trunk Drift", MatchFloat(TrunkDrift, expected.TrunkDrift), mismatches, ref matched);
            Compare("Trunk Roughness", MatchFloat(TrunkRoughness, expected.TrunkRoughness), mismatches, ref matched);
            Compare("Directional Bias", MatchFloat(DirectionalBias, expected.DirectionalBias), mismatches, ref matched);
            Compare("Directional Bias Angle", MatchAngle(DirectionalBiasAngle, expected.DirectionalBiasAngle), mismatches, ref matched);
            Compare("Tier Spacing", MatchFloat(TierSpacing, expected.TierSpacing), mismatches, ref matched);
            Compare("Branch Arch", MatchFloat(BranchArch, expected.BranchArch), mismatches, ref matched);
            Compare("Late Sag", MatchFloat(LateSag, expected.LateSag), mismatches, ref matched);
            Compare("Tip Upturn", MatchFloat(TipUpturn, expected.TipUpturn), mismatches, ref matched);
            Compare("Side Sweep", MatchFloat(SideSweep, expected.SideSweep), mismatches, ref matched);
            Compare("Fork Chance", MatchFloat(ForkChance, expected.ForkChance), mismatches, ref matched);
            return matched;
        }

        private static void Compare(
            string label,
            bool matches,
            List<string> mismatches,
            ref int matched)
        {
            if (matches)
            {
                matched++;
                return;
            }

            mismatches?.Add(label);
        }

        private static bool MatchFloat(
            TreeFloatControlRange actual,
            TreeFloatControlRange expected)
        {
            return Mathf.Approximately(actual.Minimum, expected.Minimum) &&
                Mathf.Approximately(actual.Maximum, expected.Maximum);
        }

        private static bool MatchInt(
            TreeIntControlRange actual,
            TreeIntControlRange expected)
        {
            return actual.Minimum == expected.Minimum &&
                actual.Maximum == expected.Maximum;
        }

        private static bool MatchAngle(
            TreeAngleControlRange actual,
            TreeAngleControlRange expected)
        {
            return Mathf.Approximately(actual.Minimum, expected.Minimum) &&
                Mathf.Approximately(actual.Maximum, expected.Maximum);
        }

        private static bool MatchColor(
            TreeColorControlRange actual,
            TreeColorControlRange expected)
        {
            return Approximately(actual.Minimum, expected.Minimum) &&
                Approximately(actual.Maximum, expected.Maximum);
        }

        private static bool Approximately(Color actual, Color expected)
        {
            return Mathf.Approximately(actual.r, expected.r) &&
                Mathf.Approximately(actual.g, expected.g) &&
                Mathf.Approximately(actual.b, expected.b) &&
                Mathf.Approximately(actual.a, expected.a);
        }
    }
}
