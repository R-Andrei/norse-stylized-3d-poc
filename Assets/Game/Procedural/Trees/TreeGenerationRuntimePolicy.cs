namespace ProgrammaticStylized3D.Trees
{
    /// <summary>
    /// Non-authoring generation safety and quality policy. This is deliberately
    /// separate from recipes: recipes own tree identity; this policy only keeps
    /// generated geometry finite, bounded, and within editor budgets.
    /// </summary>
    public sealed class TreeGenerationRuntimePolicy
    {
        internal int MaximumBranchCount { get; private set; }
        internal int RecipeOnlyTrunkControlPointCount { get; private set; }
        internal float RecipeOnlyMinimumTrunkTipRadiusRatio { get; private set; }
        internal int MaximumSamplesPerBranch { get; private set; }
        internal float MinimumBranchLength { get; private set; }
        internal float MinimumBranchRadius { get; private set; }
        internal float MaximumTrunkHorizontalDisplacementRatio { get; private set; }
        internal float MaximumTrunkSegmentTurnDegrees { get; private set; }
        public float MaximumBranchSegmentTurnDegrees { get; private set; }
        internal float MaximumPrimaryAccumulatedTurnDegrees { get; private set; }
        internal float MaximumHigherOrderAccumulatedTurnDegrees { get; private set; }
        internal float MaximumPrimaryArcChordRatio { get; private set; }
        internal float MaximumHigherOrderArcChordRatio { get; private set; }
        internal float MinimumForwardProgress { get; private set; }
        internal float MaximumParentReturnFraction { get; private set; }
        internal float SecondarySurvivalProbability { get; private set; }
        internal float TertiarySurvivalProbability { get; private set; }
        internal float CrownEnvelopeOvershoot { get; private set; }

        internal static TreeGenerationRuntimePolicy FromLegacy(
            TreeFamilyProfile profile)
        {
            TreeStructuralConstraintSettings constraints =
                profile.StructuralConstraints;
            return new TreeGenerationRuntimePolicy
            {
                MaximumBranchCount = profile.MaximumBranchCount,
                RecipeOnlyTrunkControlPointCount = 0,
                RecipeOnlyMinimumTrunkTipRadiusRatio = 0f,
                MaximumSamplesPerBranch = profile.MaximumSamplesPerBranch,
                MinimumBranchLength = profile.MinimumBranchLength,
                MinimumBranchRadius = profile.MinimumBranchRadius,
                MaximumTrunkHorizontalDisplacementRatio =
                    constraints.MaximumTrunkHorizontalDisplacementRatio,
                MaximumTrunkSegmentTurnDegrees =
                    constraints.MaximumTrunkSegmentTurnDegrees,
                MaximumBranchSegmentTurnDegrees =
                    constraints.MaximumBranchSegmentTurnDegrees,
                MaximumPrimaryAccumulatedTurnDegrees =
                    constraints.MaximumPrimaryAccumulatedTurnDegrees,
                MaximumHigherOrderAccumulatedTurnDegrees =
                    constraints.MaximumHigherOrderAccumulatedTurnDegrees,
                MaximumPrimaryArcChordRatio =
                    constraints.MaximumPrimaryArcChordRatio,
                MaximumHigherOrderArcChordRatio =
                    constraints.MaximumHigherOrderArcChordRatio,
                MinimumForwardProgress = constraints.MinimumForwardProgress,
                MaximumParentReturnFraction =
                    constraints.MaximumParentReturnFraction,
                SecondarySurvivalProbability =
                    constraints.SecondarySurvivalProbability,
                TertiarySurvivalProbability =
                    constraints.TertiarySurvivalProbability,
                CrownEnvelopeOvershoot = constraints.CrownEnvelopeOvershoot
            };
        }

        public static TreeGenerationRuntimePolicy RecipeOnly()
        {
            return new TreeGenerationRuntimePolicy
            {
                MaximumBranchCount = 512,
                // Stable normalized recipe-only trunk basis. Height and bend
                // frequency must not change random shape sample keys.
                RecipeOnlyTrunkControlPointCount = 12,
                // Prevent pathological base-to-tip ratios from producing a
                // collapsing terminal surface. The absolute minimum remains a
                // secondary floor for genuinely small trunks.
                RecipeOnlyMinimumTrunkTipRadiusRatio = 0.04f,
                MaximumSamplesPerBranch = 64,
                MinimumBranchLength = 0.06f,
                MinimumBranchRadius = 0.006f,
                MaximumTrunkHorizontalDisplacementRatio = 0.65f,
                MaximumTrunkSegmentTurnDegrees = 38f,
                MaximumBranchSegmentTurnDegrees = 48f,
                MaximumPrimaryAccumulatedTurnDegrees = 190f,
                MaximumHigherOrderAccumulatedTurnDegrees = 135f,
                MaximumPrimaryArcChordRatio = 1.60f,
                MaximumHigherOrderArcChordRatio = 1.42f,
                MinimumForwardProgress = 0.16f,
                MaximumParentReturnFraction = 0.14f,
                SecondarySurvivalProbability = 1f,
                TertiarySurvivalProbability = 1f,
                CrownEnvelopeOvershoot = 0f
            };
        }
    }
}
