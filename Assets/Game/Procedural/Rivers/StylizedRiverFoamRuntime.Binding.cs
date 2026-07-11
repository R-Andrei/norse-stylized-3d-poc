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
            propertyBlock.SetFloat(FoamRenderAdvectionSecondsId, 0f);
            propertyBlock.SetFloat(
                FoamFlowDirectionId,
                river != null ? river.FlowDirection : 1f);
            propertyBlock.SetFloat(FoamGlobalStartId, snapshot.GlobalStart);
            propertyBlock.SetFloat(
                FoamFieldLengthId,
                Mathf.Max(0.001f, snapshot.FieldLength));
            propertyBlock.SetColor(
                FoamColourId,
                river != null ? river.FoamColour : Color.clear);
            propertyBlock.SetFloat(
                FoamSharpnessId,
                MaterialContourSharpness);
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
            propertyBlock.SetFloat(
                FoamRenderAdvectionSecondsId,
                foamRenderAdvectionSeconds);
            propertyBlock.SetFloat(FoamFlowDirectionId, river.FlowDirection);
            propertyBlock.SetFloat(FoamGlobalStartId, allocatedGlobalStart);
            propertyBlock.SetFloat(FoamFieldLengthId, Mathf.Max(0.001f, fieldLength));
            propertyBlock.SetColor(FoamColourId, river.FoamColour);
            propertyBlock.SetFloat(
                FoamSharpnessId,
                MaterialContourSharpness);
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
            propertyBlock.SetFloat(FoamRenderAdvectionSecondsId, 0f);
            propertyBlock.SetFloat(FoamFlowDirectionId, 1f);
            propertyBlock.SetFloat(FoamGlobalStartId, 0f);
            propertyBlock.SetFloat(FoamFieldLengthId, 1f);
            propertyBlock.SetColor(FoamColourId, Color.clear);
            propertyBlock.SetFloat(FoamSharpnessId, 1f);
            propertyBlock.SetFloat(FoamDebugViewId, 0f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

    }
}
