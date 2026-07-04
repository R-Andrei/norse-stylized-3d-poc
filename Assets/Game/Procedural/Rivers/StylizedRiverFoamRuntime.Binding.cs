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
                FoamBirthDebugId,
                ResolveBoundTexture(progressiveBirthDebugTexture));
            propertyBlock.SetTexture(
                FoamBirthTransferDebugId,
                ResolveBoundTexture(progressiveBirthTransferDebugTexture));
            propertyBlock.SetTexture(FoamTopologyId, snapshot.Topology);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                snapshot.TopologySources);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                ResolveBoundTexture(snapshot.ObstacleExclusion));
            propertyBlock.SetFloat(
                FoamInterpolationId,
                snapshot.Interpolation);
            propertyBlock.SetFloat(FoamRenderTravelMetresId, 0f);
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
                FoamBirthDebugId,
                ResolveBoundTexture(progressiveBirthDebugTexture));
            propertyBlock.SetTexture(
                FoamBirthTransferDebugId,
                ResolveBoundTexture(progressiveBirthTransferDebugTexture));
            propertyBlock.SetTexture(
                FoamTopologyId,
                ResolveBoundTexture(topologyTexture));
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                ResolveBoundTexture(topologySourcesTexture));
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                ResolveBoundTexture(obstacleExclusionTexture));
            propertyBlock.SetFloat(FoamInterpolationId, simulationInterpolation);
            propertyBlock.SetFloat(FoamRenderTravelMetresId, foamRenderTravelMetres);
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
            propertyBlock.SetTexture(FoamBirthDebugId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamBirthTransferDebugId,
                Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamTopologyId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                Texture2D.blackTexture);
            propertyBlock.SetFloat(FoamInterpolationId, 1f);
            propertyBlock.SetFloat(FoamRenderTravelMetresId, 0f);
            propertyBlock.SetFloat(FoamGlobalStartId, 0f);
            propertyBlock.SetFloat(FoamFieldLengthId, 1f);
            propertyBlock.SetColor(FoamColourId, Color.clear);
            propertyBlock.SetFloat(FoamSharpnessId, 1f);
            propertyBlock.SetFloat(FoamDebugViewId, 0f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

    }
}
