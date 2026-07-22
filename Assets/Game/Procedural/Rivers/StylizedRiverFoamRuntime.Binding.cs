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

        private void BindGridDescriptorToMaterialPropertyBlock()
        {
            propertyBlock.SetVector(
                FoamGridDescriptorContractId,
                gridDescriptorGpuData.Contract);
            propertyBlock.SetVector(
                FoamGridDescriptorSpacingId,
                gridDescriptorGpuData.Spacing);
            propertyBlock.SetVector(
                FoamGridDescriptorLateralId,
                gridDescriptorGpuData.Lateral);
            propertyBlock.SetVector(
                FoamGridDescriptorLongitudinalId,
                gridDescriptorGpuData.Longitudinal);
            propertyBlock.SetVector(
                FoamGridDescriptorExtentId,
                gridDescriptorGpuData.Extent);
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
            BindGridDescriptorToMaterialPropertyBlock();
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
                ResolveBoundTexture(automaticBirthDebugTexture));
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
                FoamChipActivationId,
                river != null ? river.FoamChipActivation : 0f);
            propertyBlock.SetFloat(
                FoamChipCandidateSpacingId,
                river != null ? river.FoamChipCandidateSpacing : 1.15f);
            propertyBlock.SetFloat(
                FoamChipSizeId,
                river != null ? river.FoamChipSize : 0.3152174f);
            propertyBlock.SetFloat(
                FoamChipIrregularityId,
                river != null ? river.FoamChipIrregularity : 1f);
            propertyBlock.SetFloat(
                FoamChipStableScreenRadiusPixelsId,
                river != null
                    ? river.FoamChipStableScreenRadiusPixels
                    : 2f);
            propertyBlock.SetFloat(
                FoamChipMaximumViewScaleId,
                river != null ? river.FoamChipMaximumViewScale : 1.75f);
            propertyBlock.SetFloat(
                FoamChipEdgeWidthPixelsId,
                river != null ? river.FoamChipEdgeWidthPixels : 4f);
            propertyBlock.SetFloat(
                FoamChipSoftEdgeStartId,
                river != null ? river.FoamChipSoftEdgeStart : 0.06f);
            propertyBlock.SetFloat(
                FoamChipInteriorAccessId,
                river != null ? river.FoamChipInteriorAccess : 0f);
            propertyBlock.SetFloat(
                FoamChipFieldSpeedId,
                river != null ? river.FoamChipFieldSpeed : 0f);
            propertyBlock.SetFloat(
                FoamChipFormationTimeId,
                river != null ? river.FoamChipFormationTime : 2.5f);
            propertyBlock.SetFloat(
                FoamChipStableTimeId,
                river != null ? river.FoamChipStableTime : 5f);
            propertyBlock.SetFloat(
                FoamChipDissolveTimeId,
                river != null ? river.FoamChipDissolveTime : 2.5f);
            propertyBlock.SetFloat(
                FoamChipDormantTimeId,
                river != null ? river.FoamChipDormantTime : 4f);
            propertyBlock.SetFloat(
                FoamChipLateralMotionAmountId,
                river != null ? river.FoamChipLateralMotionAmount : 0f);
            propertyBlock.SetFloat(
                FoamChipLateralMotionSpeedId,
                river != null ? river.FoamChipLateralMotionSpeed : 0.04f);
            propertyBlock.SetFloat(
                FoamChipRotationAmountDegreesId,
                river != null ? river.FoamChipRotationAmountDegrees : 0f);
            propertyBlock.SetFloat(
                FoamChipRotationSpeedId,
                river != null ? river.FoamChipRotationSpeed : 0.04f);
            propertyBlock.SetFloat(
                FoamChipSizePulseAmountId,
                river != null ? river.FoamChipSizePulseAmount : 0f);
            propertyBlock.SetFloat(
                FoamChipSizePulseSpeedId,
                river != null ? river.FoamChipSizePulseSpeed : 0.06f);
            propertyBlock.SetFloat(
                FoamChipShapeChangeAmountId,
                river != null ? river.FoamChipShapeChangeAmount : 0f);
            propertyBlock.SetFloat(
                FoamChipShapeChangeSpeedId,
                river != null ? river.FoamChipShapeChangeSpeed : 0.04f);
            propertyBlock.SetFloat(
                FoamChipShapeTransitionTimeId,
                river != null ? river.FoamChipShapeTransitionTime : 4f);
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
                FoamPresenceFootprintModeId,
                river != null
                    ? (float)river.FoamPresenceFootprintMode
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
            BindGridDescriptorToMaterialPropertyBlock();
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
                ResolveBoundTexture(automaticBirthDebugTexture));
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
                ResolveEffectiveFoamMaximumLateralSpeedRatio());
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
                FoamChipActivationId,
                river.FoamChipActivation);
            propertyBlock.SetFloat(
                FoamChipCandidateSpacingId,
                river.FoamChipCandidateSpacing);
            propertyBlock.SetFloat(
                FoamChipSizeId,
                river.FoamChipSize);
            propertyBlock.SetFloat(
                FoamChipIrregularityId,
                river.FoamChipIrregularity);
            propertyBlock.SetFloat(
                FoamChipStableScreenRadiusPixelsId,
                river.FoamChipStableScreenRadiusPixels);
            propertyBlock.SetFloat(
                FoamChipMaximumViewScaleId,
                river.FoamChipMaximumViewScale);
            propertyBlock.SetFloat(
                FoamChipEdgeWidthPixelsId,
                river.FoamChipEdgeWidthPixels);
            propertyBlock.SetFloat(
                FoamChipSoftEdgeStartId,
                river.FoamChipSoftEdgeStart);
            propertyBlock.SetFloat(
                FoamChipInteriorAccessId,
                river.FoamChipInteriorAccess);
            propertyBlock.SetFloat(
                FoamChipFieldSpeedId,
                river.FoamChipFieldSpeed);
            propertyBlock.SetFloat(
                FoamChipFormationTimeId,
                river.FoamChipFormationTime);
            propertyBlock.SetFloat(
                FoamChipStableTimeId,
                river.FoamChipStableTime);
            propertyBlock.SetFloat(
                FoamChipDissolveTimeId,
                river.FoamChipDissolveTime);
            propertyBlock.SetFloat(
                FoamChipDormantTimeId,
                river.FoamChipDormantTime);
            propertyBlock.SetFloat(
                FoamChipLateralMotionAmountId,
                river.FoamChipLateralMotionAmount);
            propertyBlock.SetFloat(
                FoamChipLateralMotionSpeedId,
                river.FoamChipLateralMotionSpeed);
            propertyBlock.SetFloat(
                FoamChipRotationAmountDegreesId,
                river.FoamChipRotationAmountDegrees);
            propertyBlock.SetFloat(
                FoamChipRotationSpeedId,
                river.FoamChipRotationSpeed);
            propertyBlock.SetFloat(
                FoamChipSizePulseAmountId,
                river.FoamChipSizePulseAmount);
            propertyBlock.SetFloat(
                FoamChipSizePulseSpeedId,
                river.FoamChipSizePulseSpeed);
            propertyBlock.SetFloat(
                FoamChipShapeChangeAmountId,
                river.FoamChipShapeChangeAmount);
            propertyBlock.SetFloat(
                FoamChipShapeChangeSpeedId,
                river.FoamChipShapeChangeSpeed);
            propertyBlock.SetFloat(
                FoamChipShapeTransitionTimeId,
                river.FoamChipShapeTransitionTime);
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
            propertyBlock.SetFloat(
                FoamPresenceFootprintModeId,
                (float)river.FoamPresenceFootprintMode);
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
            BindGridDescriptorToMaterialPropertyBlock();
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
            propertyBlock.SetFloat(FoamChipActivationId, 0f);
            propertyBlock.SetFloat(FoamChipCandidateSpacingId, 1.15f);
            propertyBlock.SetFloat(FoamChipSizeId, 0.3152174f);
            propertyBlock.SetFloat(FoamChipIrregularityId, 1f);
            propertyBlock.SetFloat(FoamChipStableScreenRadiusPixelsId, 2f);
            propertyBlock.SetFloat(FoamChipMaximumViewScaleId, 1.75f);
            propertyBlock.SetFloat(FoamChipEdgeWidthPixelsId, 4f);
            propertyBlock.SetFloat(FoamChipSoftEdgeStartId, 0.06f);
            propertyBlock.SetFloat(FoamChipInteriorAccessId, 0f);
            propertyBlock.SetFloat(FoamChipFieldSpeedId, 0f);
            propertyBlock.SetFloat(FoamChipFormationTimeId, 2.5f);
            propertyBlock.SetFloat(FoamChipStableTimeId, 5f);
            propertyBlock.SetFloat(FoamChipDissolveTimeId, 2.5f);
            propertyBlock.SetFloat(FoamChipDormantTimeId, 4f);
            propertyBlock.SetFloat(FoamChipLateralMotionAmountId, 0f);
            propertyBlock.SetFloat(FoamChipLateralMotionSpeedId, 0.04f);
            propertyBlock.SetFloat(FoamChipRotationAmountDegreesId, 0f);
            propertyBlock.SetFloat(FoamChipRotationSpeedId, 0.04f);
            propertyBlock.SetFloat(FoamChipSizePulseAmountId, 0f);
            propertyBlock.SetFloat(FoamChipSizePulseSpeedId, 0.06f);
            propertyBlock.SetFloat(FoamChipShapeChangeAmountId, 0f);
            propertyBlock.SetFloat(FoamChipShapeChangeSpeedId, 0.04f);
            propertyBlock.SetFloat(FoamChipShapeTransitionTimeId, 4f);
            propertyBlock.SetFloat(FoamStrandStrengthId, 0f);
            propertyBlock.SetFloat(FoamStrandScaleId, 0.55f);
            propertyBlock.SetFloat(FoamStrandDensityId, 0.5f);
            propertyBlock.SetFloat(FoamStrandReachId, 0.55f);
            propertyBlock.SetFloat(FoamSharpnessId, 1f);
            propertyBlock.SetFloat(FoamFinalVisibilityModeId, 0f);
            propertyBlock.SetFloat(FoamPresenceFootprintModeId, 0f);
            propertyBlock.SetFloat(FoamDebugViewId, 0f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

    }
}
