using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private static Texture ResolveBoundTexture(Texture texture)
        {
            return texture != null
                ? texture
                : Texture2D.blackTexture;
        }

        private bool BindTopologyTransitionHold()
        {
            TopologyTransitionSnapshot snapshot = topologyTransitionSnapshot;
            if (snapshot == null || !snapshot.HoldsVisibleResources ||
                snapshot.PreviousState == null || snapshot.CurrentState == null ||
                snapshot.Topology == null || snapshot.TopologySources == null)
            {
                return false;
            }

            if (surfaceRenderer == null && river != null)
            {
                surfaceRenderer = river.SurfaceRenderer;
            }
            if (surfaceRenderer == null)
            {
                return false;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FoamEnabledId, 1f);
            propertyBlock.SetTexture(FoamPreviousId, snapshot.PreviousState);
            propertyBlock.SetTexture(FoamCurrentId, snapshot.CurrentState);
            propertyBlock.SetTexture(FoamShapeMaskId, snapshot.CurrentState);
            propertyBlock.SetTexture(FoamFilmSourceId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamFilmSupportId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamVisualOccupancyId,
                Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamBirthDebugId,
                ResolveBoundTexture(progressiveBirthDebugTexture));
            propertyBlock.SetTexture(FoamTopologyId, snapshot.Topology);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                snapshot.TopologySources);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                ResolveBoundTexture(snapshot.ObstacleExclusion));
            propertyBlock.SetTexture(FoamMotionLaneId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamObstacleRoutingId, Texture2D.blackTexture);
            propertyBlock.SetFloat(FoamMotionLaneScrollCellsId, 0f);
            propertyBlock.SetFloat(FoamBaseDownstreamSpeedId, 0f);
            propertyBlock.SetFloat(FoamMaximumLateralSpeedRatioId, 0f);
            propertyBlock.SetFloat(FoamObstacleSlowdownStrengthId, 0f);
            propertyBlock.SetFloat(
                FoamObstacleMinimumDownstreamFactorId,
                1f);
            propertyBlock.SetFloat(
                FoamInterpolationId,
                snapshot.Interpolation);
            propertyBlock.SetFloat(FoamGlobalStartId, snapshot.GlobalStart);
            propertyBlock.SetFloat(
                FoamFieldLengthId,
                Mathf.Max(0.001f, snapshot.FieldLength));
            propertyBlock.SetColor(
                FoamColourId,
                river != null ? river.FoamColour : Color.clear);
            propertyBlock.SetFloat(
                FoamInteriorOpacityFloorId,
                river != null ? river.FoamInteriorOpacityFloor : 0f);
            propertyBlock.SetFloat(
                FoamEdgeContrastId,
                river != null ? river.FoamEdgeContrast : 0f);
            propertyBlock.SetFloat(
                FoamFrayStrengthId,
                river != null ? river.FoamFrayStrength : 0f);
            propertyBlock.SetFloat(
                FoamBreakupScaleId,
                river != null ? river.FoamBreakupScale : 0.5f);
            propertyBlock.SetFloat(
                FoamChipActivationId,
                river != null ? river.FoamChipActivation : 0f);
            propertyBlock.SetFloat(
                FoamChipCandidateSpacingId,
                river != null ? river.FoamChipCandidateSpacing : 1.15f);
            propertyBlock.SetFloat(
                FoamChipDistributionIrregularityId,
                river != null
                    ? river.FoamChipDistributionIrregularity
                    : 1f);
            propertyBlock.SetFloat(
                FoamChipRadiusRatioId,
                river != null
                    ? river.FoamChipRadiusRatio
                    : 0.275f / 1.15f);
            propertyBlock.SetFloat(
                FoamChipSizeIrregularityId,
                river != null ? river.FoamChipSizeIrregularity : 1f);
            propertyBlock.SetFloat(
                FoamChipShapeIrregularityId,
                river != null ? river.FoamChipShapeIrregularity : 1f);
            propertyBlock.SetFloat(
                FoamChipSelectionDepthId,
                river != null ? river.FoamChipSelectionDepth : 0.42f);
            propertyBlock.SetFloat(
                FoamChipFieldSpeedId,
                river != null ? river.FoamChipFieldSpeed : 0f);
            propertyBlock.SetFloat(
                FoamChipEvolutionRateId,
                river != null ? river.FoamChipEvolutionRate : 0.20f);
            propertyBlock.SetFloat(
                FoamChipEvolutionAmountId,
                river != null ? river.FoamChipEvolutionAmount : 0f);
            propertyBlock.SetFloat(
                FoamFraySelectionDepthId,
                river != null ? river.FoamFraySelectionDepth : 0.68f);
            propertyBlock.SetFloat(
                FoamFrayWavelengthId,
                river != null ? river.FoamFrayWavelength : 0.12f);
            propertyBlock.SetFloat(
                FoamFrayDepthId,
                river != null ? river.FoamFrayDepth : 0f);
            propertyBlock.SetFloat(
                FoamStrandStrengthId,
                river != null ? river.FoamStrandStrength : 0f);
            propertyBlock.SetFloat(
                FoamStrandScaleId,
                river != null ? river.FoamStrandScale : 0.55f);
            propertyBlock.SetFloat(
                FoamStrandDensityId,
                river != null ? river.FoamStrandDensity : 0.5f);
            propertyBlock.SetFloat(
                FoamStrandReachId,
                river != null ? river.FoamStrandReach : 0.55f);
            propertyBlock.SetFloat(
                FoamSharpnessId,
                MaterialContourSharpness);
            propertyBlock.SetFloat(
                FoamFinalVisibilityModeId,
                river != null
                    ? (float)river.FoamFinalVisibilityMode
                    : 0f);
            propertyBlock.SetFloat(
                FoamDebugViewId,
                river != null ? (float)river.FoamDebugView : 0f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
            return true;
        }

        private void BindField()
        {
            if (surfaceRenderer == null || river == null ||
                currentState == null || previousState == null ||
                shapeMaskTexture == null)
            {
                BindDisabled();
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FoamEnabledId, 1f);
            propertyBlock.SetTexture(FoamPreviousId, previousState);
            propertyBlock.SetTexture(FoamCurrentId, currentState);
            propertyBlock.SetTexture(
                FoamShapeMaskId,
                ResolveBoundTexture(shapeMaskTexture));
            propertyBlock.SetTexture(
                FoamFilmSourceId,
                ResolveBoundTexture(filmSourceTexture));
            propertyBlock.SetTexture(
                FoamFilmSupportId,
                ResolveBoundTexture(filmSupportTexture));
            propertyBlock.SetTexture(
                FoamVisualOccupancyId,
                ResolveBoundTexture(currentVisualOccupancy));
            propertyBlock.SetTexture(
                FoamBirthDebugId,
                ResolveBoundTexture(progressiveBirthDebugTexture));
            propertyBlock.SetTexture(
                FoamTopologyId,
                ResolveBoundTexture(topologyTexture));
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                ResolveBoundTexture(topologySourcesTexture));
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                ResolveBoundTexture(obstacleExclusionTexture));
            propertyBlock.SetTexture(
                FoamMotionLaneId,
                ResolveBoundTexture(motionLaneTexture));
            propertyBlock.SetTexture(
                FoamObstacleRoutingId,
                ResolveBoundTexture(obstacleRoutingTexture));
            propertyBlock.SetFloat(
                FoamMotionLaneScrollCellsId,
                motionLaneScrollCells);
            propertyBlock.SetFloat(
                FoamBaseDownstreamSpeedId,
                ResolveBaseFoamDownstreamSpeedMetresPerSecond());
            propertyBlock.SetFloat(
                FoamMaximumLateralSpeedRatioId,
                river.FoamMaximumLateralSpeedRatio);
            propertyBlock.SetFloat(
                FoamObstacleSlowdownStrengthId,
                river.FoamObstacleSlowdownStrength);
            propertyBlock.SetFloat(
                FoamObstacleMinimumDownstreamFactorId,
                river.FoamObstacleMinimumDownstreamFactor);
            propertyBlock.SetFloat(FoamInterpolationId, simulationInterpolation);
            propertyBlock.SetFloat(FoamGlobalStartId, allocatedGlobalStart);
            propertyBlock.SetFloat(FoamFieldLengthId, Mathf.Max(0.001f, fieldLength));
            propertyBlock.SetColor(FoamColourId, river.FoamColour);
            propertyBlock.SetFloat(
                FoamInteriorOpacityFloorId,
                river.FoamInteriorOpacityFloor);
            propertyBlock.SetFloat(
                FoamEdgeContrastId,
                river.FoamEdgeContrast);
            propertyBlock.SetFloat(
                FoamFrayStrengthId,
                river.FoamFrayStrength);
            propertyBlock.SetFloat(
                FoamBreakupScaleId,
                river.FoamBreakupScale);
            propertyBlock.SetFloat(
                FoamChipActivationId,
                river.FoamChipActivation);
            propertyBlock.SetFloat(
                FoamChipCandidateSpacingId,
                river.FoamChipCandidateSpacing);
            propertyBlock.SetFloat(
                FoamChipDistributionIrregularityId,
                river.FoamChipDistributionIrregularity);
            propertyBlock.SetFloat(
                FoamChipRadiusRatioId,
                river.FoamChipRadiusRatio);
            propertyBlock.SetFloat(
                FoamChipSizeIrregularityId,
                river.FoamChipSizeIrregularity);
            propertyBlock.SetFloat(
                FoamChipShapeIrregularityId,
                river.FoamChipShapeIrregularity);
            propertyBlock.SetFloat(
                FoamChipSelectionDepthId,
                river.FoamChipSelectionDepth);
            propertyBlock.SetFloat(
                FoamChipFieldSpeedId,
                river.FoamChipFieldSpeed);
            propertyBlock.SetFloat(
                FoamChipEvolutionRateId,
                river.FoamChipEvolutionRate);
            propertyBlock.SetFloat(
                FoamChipEvolutionAmountId,
                river.FoamChipEvolutionAmount);
            propertyBlock.SetFloat(
                FoamFraySelectionDepthId,
                river.FoamFraySelectionDepth);
            propertyBlock.SetFloat(
                FoamFrayWavelengthId,
                river.FoamFrayWavelength);
            propertyBlock.SetFloat(
                FoamFrayDepthId,
                river.FoamFrayDepth);
            propertyBlock.SetFloat(
                FoamStrandStrengthId,
                river.FoamStrandStrength);
            propertyBlock.SetFloat(
                FoamStrandScaleId,
                river.FoamStrandScale);
            propertyBlock.SetFloat(
                FoamStrandDensityId,
                river.FoamStrandDensity);
            propertyBlock.SetFloat(
                FoamStrandReachId,
                river.FoamStrandReach);
            propertyBlock.SetFloat(
                FoamSharpnessId,
                MaterialContourSharpness);
            propertyBlock.SetFloat(
                FoamFinalVisibilityModeId,
                (float)river.FoamFinalVisibilityMode);
            propertyBlock.SetFloat(FoamDebugViewId, (float)river.FoamDebugView);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

        private void BindDisabled()
        {
            if (surfaceRenderer == null && river != null)
            {
                surfaceRenderer = river.SurfaceRenderer;
            }

            if (surfaceRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FoamEnabledId, 0f);
            propertyBlock.SetTexture(FoamPreviousId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamCurrentId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamShapeMaskId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamFilmSourceId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamFilmSupportId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamVisualOccupancyId,
                Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamBirthDebugId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamTopologyId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamMotionLaneId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamObstacleRoutingId, Texture2D.blackTexture);
            propertyBlock.SetFloat(FoamMotionLaneScrollCellsId, 0f);
            propertyBlock.SetFloat(FoamBaseDownstreamSpeedId, 0f);
            propertyBlock.SetFloat(FoamMaximumLateralSpeedRatioId, 0f);
            propertyBlock.SetFloat(FoamObstacleSlowdownStrengthId, 0f);
            propertyBlock.SetFloat(
                FoamObstacleMinimumDownstreamFactorId,
                1f);
            propertyBlock.SetFloat(FoamInterpolationId, 1f);
            propertyBlock.SetFloat(FoamGlobalStartId, 0f);
            propertyBlock.SetFloat(FoamFieldLengthId, 1f);
            propertyBlock.SetColor(FoamColourId, Color.clear);
            propertyBlock.SetFloat(FoamInteriorOpacityFloorId, 0f);
            propertyBlock.SetFloat(FoamEdgeContrastId, 0f);
            propertyBlock.SetFloat(FoamFrayStrengthId, 0f);
            propertyBlock.SetFloat(FoamBreakupScaleId, 0.5f);
            propertyBlock.SetFloat(FoamChipActivationId, 0f);
            propertyBlock.SetFloat(FoamChipCandidateSpacingId, 1.15f);
            propertyBlock.SetFloat(FoamChipDistributionIrregularityId, 1f);
            propertyBlock.SetFloat(FoamChipRadiusRatioId, 0.275f / 1.15f);
            propertyBlock.SetFloat(FoamChipSizeIrregularityId, 1f);
            propertyBlock.SetFloat(FoamChipShapeIrregularityId, 1f);
            propertyBlock.SetFloat(FoamChipSelectionDepthId, 0.42f);
            propertyBlock.SetFloat(FoamChipFieldSpeedId, 0f);
            propertyBlock.SetFloat(FoamChipEvolutionRateId, 0.20f);
            propertyBlock.SetFloat(FoamChipEvolutionAmountId, 0f);
            propertyBlock.SetFloat(FoamFraySelectionDepthId, 0.68f);
            propertyBlock.SetFloat(FoamFrayWavelengthId, 0.12f);
            propertyBlock.SetFloat(FoamFrayDepthId, 0f);
            propertyBlock.SetFloat(FoamStrandStrengthId, 0f);
            propertyBlock.SetFloat(FoamStrandScaleId, 0.55f);
            propertyBlock.SetFloat(FoamStrandDensityId, 0.5f);
            propertyBlock.SetFloat(FoamStrandReachId, 0.55f);
            propertyBlock.SetFloat(FoamSharpnessId, 1f);
            propertyBlock.SetFloat(FoamFinalVisibilityModeId, 0f);
            propertyBlock.SetFloat(FoamDebugViewId, 0f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

    }
}
