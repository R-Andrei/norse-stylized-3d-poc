using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace ProgrammaticStylized3D.Weather.Rendering
{
    // WEATHER VEGETATION ACCENT SIDECAR CONTRACT — PROTECTED MODULE BOUNDARY.
    //
    // This feature publishes one GPU record in EACH CAMERA'S OWN URP
    // additional-light order. Never reuse the Game camera's record order for
    // Scene View or another camera. The atmospheric LightRay composite remains
    // restricted to the resolved Base Game camera; only the metadata sidecar is
    // camera-local.
    //
    // The C# VegetationAccentGpuRecord layout must remain byte-for-byte mirrored
    // by VegetationAdditionalLightAccentData in VegetationLighting.hlsl:
    //   Parameters      = strength, whole-card coverage, edge softness, override
    //   SourceDirection = horizontal direction toward source, direction-valid
    //
    // Do not replace this O(1) indexed contract with a shader-side LightRay
    // search, geometric Spot matching, Rendering Layer identity, or a single
    // global owner.
    public sealed class WeatherLightRayRendererFeature :
        ScriptableRendererFeature
    {
        private const string MaskShaderName =
            "Hidden/PS3D/Weather LightRay Mask";
        private const string ScatterShaderName =
            "Hidden/PS3D/Weather LightRay Scatter";
        private const string CompositeShaderName =
            "Hidden/PS3D/Weather LightRay Composite";
        private const int VegetationAccentGpuRecordStride =
            sizeof(float) * 8;

        [StructLayout(LayoutKind.Sequential)]
        private struct VegetationAccentGpuRecord
        {
            public Vector4 Parameters;
            public Vector4 SourceDirectionWS;
        }

        private WeatherLightRayRenderPass renderPass;
        private VegetationAccentBindingPass vegetationAccentFallbackBindingPass;
        private Material maskMaterial;
        private Material scatterMaterial;
        private Material compositeMaterial;
        private bool countedAsActive;
        private bool missingShaderReported;
        private WeatherLightRaySnapshot[] snapshotBuffer =
            Array.Empty<WeatherLightRaySnapshot>();
        private static readonly VegetationAccentGpuRecord[]
            EmptyVegetationAccentRecord =
            {
                new VegetationAccentGpuRecord
                {
                    Parameters = Vector4.zero,
                    SourceDirectionWS = Vector4.zero
                }
            };

        private readonly List<CameraVegetationAccentResources>
            vegetationAccentCameraResources =
                new List<CameraVegetationAccentResources>();
        private GraphicsBuffer vegetationAccentFallbackBuffer;

        private static readonly int VegetationAccentDataBufferId =
            Shader.PropertyToID("_VegetationAdditionalLightAccentData");
        private static readonly int VegetationAccentDataCountId =
            Shader.PropertyToID("_VegetationAdditionalLightAccentDataCount");

        public static int ActiveFeatureCount { get; private set; }

        private sealed class VegetationAccentBindingPass : ScriptableRenderPass
        {
            private sealed class PassData
            {
                public GraphicsBuffer Buffer;
                public int Count;
            }

            private GraphicsBuffer buffer;
            private int count;

            public void Setup(GraphicsBuffer sourceBuffer, int sourceCount)
            {
                buffer = sourceBuffer;
                count = Mathf.Max(0, sourceCount);
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData)
            {
                if (buffer == null)
                {
                    return;
                }

                using (var builder =
                    renderGraph.AddRasterRenderPass<PassData>(
                        "Weather LightRay Vegetation Accent Sidecar Binding",
                        out PassData passData))
                {
                    passData.Buffer = buffer;
                    passData.Count = count;
                    builder.AllowGlobalStateModification(true);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc(
                        static (PassData data, RasterGraphContext context) =>
                        {
                            context.cmd.SetGlobalBuffer(
                                VegetationAccentDataBufferId,
                                data.Buffer);
                            context.cmd.SetGlobalInt(
                                VegetationAccentDataCountId,
                                data.Count);
                        });
                }
            }
        }

        private sealed class CameraVegetationAccentResources : IDisposable
        {
            public Camera Camera;
            public VegetationAccentBindingPass BindingPass;
            public VegetationAccentGpuRecord[] Records =
                Array.Empty<VegetationAccentGpuRecord>();
            public GraphicsBuffer Buffer;
            public int BufferCapacity;

            public void Dispose()
            {
                Buffer?.Dispose();
                Buffer = null;
                BufferCapacity = 0;
                Records = Array.Empty<VegetationAccentGpuRecord>();
                BindingPass = null;
                Camera = null;
            }
        }

        public override void Create()
        {
            renderPass?.Dispose();
            renderPass = null;
            DisposeVegetationAccentCameraResources();
            vegetationAccentFallbackBindingPass =
                CreateVegetationAccentBindingPass();
            vegetationAccentFallbackBuffer?.Dispose();
            vegetationAccentFallbackBuffer = null;
            EnsureVegetationAccentFallbackBuffer();
            vegetationAccentFallbackBindingPass.Setup(
                vegetationAccentFallbackBuffer,
                0);
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
            EnsureVegetationAccentFallbackBuffer();
            PruneDestroyedVegetationAccentCameraResources();

            Camera camera = renderingData.cameraData.camera;
            bool relevantBaseGameCamera =
                camera != null &&
                camera.cameraType == CameraType.Game &&
                renderingData.cameraData.renderType == CameraRenderType.Base;

            WeatherLightRayController controller =
                WeatherLightRayController.PublishedController;
            bool validLightRayCamera =
                relevantBaseGameCamera &&
                controller != null &&
                controller.ResolvedRenderCamera == camera;

            // SIDECAR CAMERA CONTRACT:
            // Game and Scene View cameras may render the production vegetation
            // shader and therefore must publish records from THEIR visible-light
            // ordering. Preview/reflection/unrelated cameras bind the valid zero
            // fallback. This does not authorize atmospheric rendering outside the
            // resolved Base Game camera below.
            bool publishCameraSidecar =
                controller != null &&
                camera != null &&
                (camera.cameraType == CameraType.Game ||
                    camera.cameraType == CameraType.SceneView);

            VegetationAccentBindingPass bindingPass =
                vegetationAccentFallbackBindingPass;
            GraphicsBuffer accentBufferToBind =
                vegetationAccentFallbackBuffer;
            int accentRecordCount = 0;
            if (publishCameraSidecar)
            {
                CameraVegetationAccentResources resources =
                    GetOrCreateVegetationAccentCameraResources(camera);
                accentRecordCount = PublishVegetationAccentSidecar(
                    controller,
                    resources,
                    ref renderingData,
                    validLightRayCamera);
                if (accentRecordCount > 0 && resources.Buffer != null)
                {
                    bindingPass = resources.BindingPass;
                    accentBufferToBind = resources.Buffer;
                }
            }

            bindingPass?.Setup(
                accentBufferToBind,
                accentRecordCount);
            if (bindingPass != null)
            {
                renderer.EnqueuePass(bindingPass);
            }

            if (renderPass == null || !validLightRayCamera)
            {
                return;
            }

            int required = controller.CopyActiveSnapshots(null);
            if (snapshotBuffer.Length < required)
            {
                Array.Resize(
                    ref snapshotBuffer,
                    Mathf.NextPowerOfTwo(Mathf.Max(1, required)));
            }
            int copied = controller.CopyActiveSnapshots(snapshotBuffer);
            if (copied <= 0)
            {
                return;
            }

            WeatherLightRaySourceState sourceState = default;
            bool foundRenderable = false;
            for (int index = 0; index < copied; index++)
            {
                if (snapshotBuffer[index].CurrentIntensity <= 0.0001f)
                {
                    continue;
                }

                sourceState = controller.ResolveRenderableSourceState(
                    snapshotBuffer[index]);
                foundRenderable = true;
                break;
            }
            if (!foundRenderable)
            {
                return;
            }

            renderPass.Setup(
                snapshotBuffer,
                copied,
                sourceState,
                controller.RenderDebugView,
                camera);
            renderer.EnqueuePass(renderPass);
        }

        protected override void Dispose(bool disposing)
        {
            renderPass?.Dispose();
            renderPass = null;
            vegetationAccentFallbackBindingPass = null;
            DisposeVegetationAccentCameraResources();
            vegetationAccentFallbackBuffer?.Dispose();
            vegetationAccentFallbackBuffer = null;
            Shader.SetGlobalInt(VegetationAccentDataCountId, 0);
            DestroyMaterials();

            if (countedAsActive)
            {
                ActiveFeatureCount = Mathf.Max(
                    0,
                    ActiveFeatureCount - 1);
                countedAsActive = false;
            }
        }

        // CAMERA-ORDERING CONTRACT.
        // Build this buffer only from the RenderingData supplied for the same
        // camera whose BindingPass owns the destination buffer. URP Forward and
        // Forward+ indices are meaningful only inside that camera's light list.
        // Never copy records from another camera and never reorder Weather lights
        // independently from URP's visible-light traversal.
        private int PublishVegetationAccentSidecar(
            WeatherLightRayController controller,
            CameraVegetationAccentResources resources,
            ref RenderingData renderingData,
            bool recordControllerPublication)
        {
            int requestedCount = Mathf.Max(
                0,
                renderingData.lightData.additionalLightsCount);
            if (requestedCount <= 0)
            {
                if (recordControllerPublication)
                {
                    controller.RecordVegetationAccentSidecarPublication(
                        0,
                        0,
                        resources.BufferCapacity,
                        false);
                }
                return 0;
            }

            EnsureVegetationAccentCapacity(resources, requestedCount);
            int mainLightIndex = renderingData.lightData.mainLightIndex;
            int written = 0;
            int weatherOverrideCount = 0;
            var visibleLights = renderingData.lightData.visibleLights;
            for (int visibleIndex = 0;
                visibleIndex < visibleLights.Length && written < requestedCount;
                visibleIndex++)
            {
                if (visibleIndex == mainLightIndex)
                {
                    continue;
                }

                VegetationAccentGpuRecord record = default;
                Light light = visibleLights[visibleIndex].light;
                if (controller.TryGetVegetationAccentOverride(
                        light,
                        out Vector4 parameters,
                        out Vector4 sourceDirectionWS))
                {
                    record.Parameters = parameters;
                    record.SourceDirectionWS = sourceDirectionWS;
                    weatherOverrideCount++;
                }

                resources.Records[written++] = record;
            }

            bool overflow = written != requestedCount;
            for (int index = written; index < requestedCount; index++)
            {
                resources.Records[index] = default;
            }

            resources.Buffer.SetData(
                resources.Records,
                0,
                0,
                requestedCount);
            if (recordControllerPublication)
            {
                controller.RecordVegetationAccentSidecarPublication(
                    requestedCount,
                    weatherOverrideCount,
                    resources.BufferCapacity,
                    overflow);
            }
            return requestedCount;
        }

        private CameraVegetationAccentResources
            GetOrCreateVegetationAccentCameraResources(Camera camera)
        {
            for (int index = 0;
                index < vegetationAccentCameraResources.Count;
                index++)
            {
                CameraVegetationAccentResources existing =
                    vegetationAccentCameraResources[index];
                if (existing.Camera == camera)
                {
                    return existing;
                }
            }

            var created = new CameraVegetationAccentResources
            {
                Camera = camera,
                BindingPass = CreateVegetationAccentBindingPass()
            };
            vegetationAccentCameraResources.Add(created);
            return created;
        }

        private static VegetationAccentBindingPass
            CreateVegetationAccentBindingPass()
        {
            return new VegetationAccentBindingPass
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingOpaques
            };
        }

        private void PruneDestroyedVegetationAccentCameraResources()
        {
            for (int index = vegetationAccentCameraResources.Count - 1;
                index >= 0;
                index--)
            {
                CameraVegetationAccentResources resources =
                    vegetationAccentCameraResources[index];
                if (resources.Camera != null)
                {
                    continue;
                }

                resources.Dispose();
                vegetationAccentCameraResources.RemoveAt(index);
            }
        }

        private void DisposeVegetationAccentCameraResources()
        {
            for (int index = 0;
                index < vegetationAccentCameraResources.Count;
                index++)
            {
                vegetationAccentCameraResources[index].Dispose();
            }
            vegetationAccentCameraResources.Clear();
        }

        private void EnsureVegetationAccentFallbackBuffer()
        {
            if (vegetationAccentFallbackBuffer != null)
            {
                return;
            }

            vegetationAccentFallbackBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                1,
                VegetationAccentGpuRecordStride);
            vegetationAccentFallbackBuffer.SetData(
                EmptyVegetationAccentRecord);
        }

        private static void EnsureVegetationAccentCapacity(
            CameraVegetationAccentResources resources,
            int required)
        {
            if (required <= resources.BufferCapacity &&
                resources.Buffer != null)
            {
                return;
            }

            int capacity = Mathf.NextPowerOfTwo(Mathf.Max(1, required));
            resources.Buffer?.Dispose();
            resources.Buffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                capacity,
                VegetationAccentGpuRecordStride);
            resources.BufferCapacity = capacity;
            if (resources.Records.Length < capacity)
            {
                Array.Resize(ref resources.Records, capacity);
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
