using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProgrammaticStylized3D.Weather.Rendering
{
    public sealed class WeatherLightRayRendererFeature :
        ScriptableRendererFeature
    {
        private const string MaskShaderName =
            "Hidden/PS3D/Weather LightRay Mask";
        private const string ScatterShaderName =
            "Hidden/PS3D/Weather LightRay Scatter";
        private const string CompositeShaderName =
            "Hidden/PS3D/Weather LightRay Composite";

        private WeatherLightRayRenderPass renderPass;
        private Material maskMaterial;
        private Material scatterMaterial;
        private Material compositeMaterial;
        private bool countedAsActive;
        private bool missingShaderReported;

        public static int ActiveFeatureCount { get; private set; }

        public override void Create()
        {
            renderPass?.Dispose();
            renderPass = null;
            DestroyMaterials();

            maskMaterial = CreateMaterial(MaskShaderName);
            scatterMaterial = CreateMaterial(ScatterShaderName);
            compositeMaterial = CreateMaterial(CompositeShaderName);
            if (maskMaterial == null ||
                scatterMaterial == null ||
                compositeMaterial == null)
            {
                renderPass = null;
                if (!missingShaderReported)
                {
                    Debug.LogError(
                        "[Weather LightRay V1.1] One or more hidden LightRay " +
                        "shaders could not be resolved. Confirm all three shader " +
                        "files compile before enabling the Renderer Feature.");
                    missingShaderReported = true;
                }
            }
            else
            {
                missingShaderReported = false;
                renderPass = new WeatherLightRayRenderPass(
                    maskMaterial,
                    scatterMaterial,
                    compositeMaterial)
                {
                    renderPassEvent =
                        RenderPassEvent.AfterRenderingTransparents
                };
            }

            if (renderPass != null && !countedAsActive)
            {
                ActiveFeatureCount++;
                countedAsActive = true;
            }
            else if (renderPass == null && countedAsActive)
            {
                ActiveFeatureCount = Mathf.Max(
                    0,
                    ActiveFeatureCount - 1);
                countedAsActive = false;
            }
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            if (renderPass == null)
            {
                return;
            }

            Camera camera = renderingData.cameraData.camera;
            WeatherLightRayController controller =
                WeatherLightRayController.PublishedController;
            if (controller == null ||
                camera == null ||
                camera.cameraType != CameraType.Game ||
                renderingData.cameraData.renderType != CameraRenderType.Base ||
                controller.ResolvedRenderCamera != camera ||
                !controller.TryGetPrimaryRenderableRay(
                    out WeatherLightRaySnapshot snapshot,
                    out WeatherLightRaySourceState sourceState))
            {
                return;
            }

            renderPass.Setup(
                snapshot,
                sourceState,
                controller.RenderDebugView,
                camera);
            renderer.EnqueuePass(renderPass);
        }

        protected override void Dispose(bool disposing)
        {
            renderPass?.Dispose();
            renderPass = null;
            DestroyMaterials();

            if (countedAsActive)
            {
                ActiveFeatureCount = Mathf.Max(
                    0,
                    ActiveFeatureCount - 1);
                countedAsActive = false;
            }
        }

        private static Material CreateMaterial(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            return shader != null
                ? CoreUtils.CreateEngineMaterial(shader)
                : null;
        }

        private void DestroyMaterials()
        {
            CoreUtils.Destroy(maskMaterial);
            CoreUtils.Destroy(scatterMaterial);
            CoreUtils.Destroy(compositeMaterial);
            maskMaterial = null;
            scatterMaterial = null;
            compositeMaterial = null;
        }
    }
}
