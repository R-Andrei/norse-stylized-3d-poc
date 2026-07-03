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
            propertyBlock.SetTexture(
                FoamGuidanceId,
                ResolveBoundTexture(snapshot.Guidance));
            propertyBlock.SetTexture(FoamTopologyId, snapshot.Topology);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                snapshot.TopologySources);
            propertyBlock.SetTexture(
                FoamBoundaryId,
                ResolveBoundTexture(snapshot.Boundary));
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                ResolveBoundTexture(snapshot.ObstacleExclusion));
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
                river != null ? (float)river.FoamDebugView : 0f);
            propertyBlock.SetFloat(
                FoamSeedId,
                river != null ? river.VisualSeed : 0f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
            return true;
        }

        private void BindField()
        {
            if (surfaceRenderer == null || river == null ||
                currentState == null || previousState == null)
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
                FoamGuidanceId,
                ResolveBoundTexture(guidanceTexture));
            propertyBlock.SetTexture(
                FoamTopologyId,
                ResolveBoundTexture(topologyTexture));
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                ResolveBoundTexture(topologySourcesTexture));
            propertyBlock.SetTexture(
                FoamBoundaryId,
                ResolveBoundTexture(boundaryTexture));
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                ResolveBoundTexture(obstacleExclusionTexture));
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
            propertyBlock.SetTexture(FoamPreviousId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamCurrentId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamGuidanceId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamTopologyId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamBoundaryId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                Texture2D.blackTexture);
            propertyBlock.SetFloat(FoamInterpolationId, 1f);
            propertyBlock.SetFloat(FoamGlobalStartId, 0f);
            propertyBlock.SetFloat(FoamFieldLengthId, 1f);
            propertyBlock.SetColor(FoamColourId, Color.clear);
            propertyBlock.SetFloat(FoamStrengthId, 0f);
            propertyBlock.SetFloat(FoamCoverageId, 0f);
            propertyBlock.SetFloat(FoamSharpnessId, 1f);
            propertyBlock.SetFloat(FoamDetailScaleId, 1f);
            propertyBlock.SetFloat(FoamDetailStrengthId, 0f);
            propertyBlock.SetFloat(FoamDebugViewId, 0f);
            propertyBlock.SetFloat(FoamSeedId, 0f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

    }
}
