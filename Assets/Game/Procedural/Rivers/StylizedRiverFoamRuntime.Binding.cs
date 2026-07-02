using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
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
            propertyBlock.SetTexture(
                FoamGuidanceId,
                snapshot.Guidance != null
                    ? snapshot.Guidance
                    : Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamTopologyId, snapshot.Topology);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                snapshot.TopologySources);
            propertyBlock.SetTexture(
                FoamFractureId,
                snapshot.Fracture != null
                    ? snapshot.Fracture
                    : Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamBoundaryId,
                snapshot.Boundary != null
                    ? snapshot.Boundary
                    : Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                snapshot.ObstacleExclusion != null
                    ? snapshot.ObstacleExclusion
                    : Texture2D.blackTexture);
            propertyBlock.SetFloat(
                FoamInterpolationId,
                snapshot.Interpolation);
            propertyBlock.SetFloat(FoamGlobalStartId, snapshot.GlobalStart);
            propertyBlock.SetFloat(
                FoamFieldLengthId,
                Mathf.Max(0.001f, snapshot.FieldLength));
            propertyBlock.SetColor(FoamColourId, river.FoamColour);
            propertyBlock.SetFloat(
                FoamStrengthId,
                ProvisionalMaterialStrength);
            propertyBlock.SetFloat(
                FoamCoverageId,
                ProvisionalMaterialCoverage);
            propertyBlock.SetFloat(
                FoamSharpnessId,
                ProvisionalMaterialSharpness);
            propertyBlock.SetFloat(
                FoamDetailScaleId,
                ProvisionalMaterialDetailScale);
            propertyBlock.SetFloat(
                FoamDetailStrengthId,
                ProvisionalMaterialDetailStrength);
            propertyBlock.SetFloat(
                FoamDebugViewId,
                (float)river.FoamDebugView);
            propertyBlock.SetFloat(FoamSeedId, river.VisualSeed);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
            return true;
        }

        private void BindField()
        {
            if (surfaceRenderer == null || currentState == null || previousState == null)
            {
                BindDisabled();
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FoamEnabledId, 1f);
            propertyBlock.SetTexture(FoamPreviousId, previousState);
            propertyBlock.SetTexture(FoamCurrentId, currentState);
            propertyBlock.SetTexture(FoamGuidanceId, guidanceTexture);
            propertyBlock.SetTexture(FoamTopologyId, topologyTexture);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                topologySourcesTexture);
            propertyBlock.SetTexture(FoamFractureId, currentFracture);
            propertyBlock.SetTexture(FoamBoundaryId, boundaryTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                obstacleExclusionTexture);
            propertyBlock.SetFloat(FoamInterpolationId, simulationInterpolation);
            propertyBlock.SetFloat(FoamGlobalStartId, allocatedGlobalStart);
            propertyBlock.SetFloat(FoamFieldLengthId, Mathf.Max(0.001f, fieldLength));
            propertyBlock.SetColor(FoamColourId, river.FoamColour);
            propertyBlock.SetFloat(
                FoamStrengthId,
                ProvisionalMaterialStrength);
            propertyBlock.SetFloat(
                FoamCoverageId,
                ProvisionalMaterialCoverage);
            propertyBlock.SetFloat(
                FoamSharpnessId,
                ProvisionalMaterialSharpness);
            propertyBlock.SetFloat(
                FoamDetailScaleId,
                ProvisionalMaterialDetailScale);
            propertyBlock.SetFloat(
                FoamDetailStrengthId,
                ProvisionalMaterialDetailStrength);
            propertyBlock.SetFloat(FoamDebugViewId, (float)river.FoamDebugView);
            propertyBlock.SetFloat(FoamSeedId, river.VisualSeed);
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
            propertyBlock.SetTexture(FoamGuidanceId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamTopologyId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamFractureId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamBoundaryId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                Texture2D.blackTexture);
            propertyBlock.SetFloat(FoamInterpolationId, 1f);
            propertyBlock.SetFloat(
                FoamDebugViewId,
                river != null && river.FoamEnabled
                    ? (float)river.FoamDebugView
                    : 0f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

    }
}
