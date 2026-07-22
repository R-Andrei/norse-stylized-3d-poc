using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Vegetation.Editor
{
    [InitializeOnLoad]
    internal static class VegetationRendererEditorPreview
    {
        static VegetationRendererEditorPreview()
        {
            RenderPipelineManager.beginCameraRendering -=
                HandleBeginCameraRendering;
            RenderPipelineManager.beginCameraRendering +=
                HandleBeginCameraRendering;
        }

        private static void HandleBeginCameraRendering(
            ScriptableRenderContext context,
            Camera camera)
        {
            if (Application.isPlaying || camera == null ||
                (camera.cameraType != CameraType.SceneView &&
                 camera.cameraType != CameraType.Game))
            {
                return;
            }

            var renderers = VegetationRendererBase.ActiveRenderers;
            for (int index = 0; index < renderers.Count; index++)
            {
                VegetationRendererBase renderer = renderers[index];
                if (renderer == null || !renderer.isActiveAndEnabled)
                {
                    continue;
                }

                renderer.RenderEditorPreview(camera);
            }
        }
    }
}
