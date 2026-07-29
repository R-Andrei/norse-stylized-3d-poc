using System;

namespace ProgrammaticStylized3D.Trees
{
    public static class TreeCuratedGalleryAssignment
    {
        public static string BuildSlotIdentity(
            TreeFamily family,
            int sourceVariantIndex)
        {
            return family + ":" + Math.Max(1, Math.Min(5, sourceVariantIndex));
        }

        public static string ResolveRecipeStableIdentity(
            TreeFamily family,
            int sourceVariantIndex)
        {
            int variant = Math.Max(1, Math.Min(5, sourceVariantIndex));
            switch (family)
            {
                case TreeFamily.Pine:
                    switch (variant)
                    {
                        case 3:
                            return "tree-recipe-curated-norway-spruce-high-crown";
                        case 4:
                            return "tree-recipe-curated-norway-spruce-tall";
                        default:
                            return "tree-recipe-curated-norway-spruce-standard";
                    }

                case TreeFamily.Twisted:
                    return variant == 1
                        ? "tree-recipe-curated-wych-elm-leaning"
                        : "tree-recipe-curated-wych-elm-upright";

                case TreeFamily.Dead:
                    switch (variant)
                    {
                        case 1:
                            return "tree-recipe-curated-dead-alder";
                        case 2:
                            return "tree-recipe-curated-dead-wych-elm";
                        case 5:
                            return "tree-recipe-curated-tall-dead-snag";
                        default:
                            return "tree-recipe-curated-dead-norway-spruce";
                    }

                default:
                    switch (variant)
                    {
                        case 4:
                            return "tree-recipe-curated-alder-high-crown";
                        case 5:
                            return "tree-recipe-curated-alder-windswept";
                        default:
                            return "tree-recipe-curated-alder-standard";
                    }
            }
        }

        public static int ResolveGallerySeed(
            int gallerySeed,
            TreeFamily family,
            int sourceVariantIndex)
        {
            return TreeDeterministicUtility.DeriveSeed(
                gallerySeed,
                "tree-curated-gallery-slot",
                BuildSlotIdentity(family, sourceVariantIndex));
        }
    }
}
