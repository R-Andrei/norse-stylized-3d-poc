using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace ProgrammaticStylized3D.Weather.Rendering
{
    public sealed class WeatherLightRayRenderPass :
        ScriptableRenderPass,
        IDisposable
    {
        private const int ProxySides = 12;
        private const int DownsampleDivisor = 4;

        private static readonly int BaseCentreHeightId =
            Shader.PropertyToID("_WeatherLightRayBaseCentreHeight");
        private static readonly int RayDirectionBaseRadiusId =
            Shader.PropertyToID("_WeatherLightRayDirectionBaseRadius");
        private static readonly int TopShapeId =
            Shader.PropertyToID("_WeatherLightRayTopShape");
        private static readonly int ColourId =
            Shader.PropertyToID("_WeatherLightRayColour");
        private static readonly int IntensityId =
            Shader.PropertyToID("_WeatherLightRayIntensity");
        private static readonly int CloudParametersId =
            Shader.PropertyToID("_WeatherLightRayCloudParameters");
        private static readonly int StrandShape0Id =
            Shader.PropertyToID("_WeatherLightRayStrandShape0");
        private static readonly int StrandShape1Id =
            Shader.PropertyToID("_WeatherLightRayStrandShape1");
        private static readonly int StrandShape2Id =
            Shader.PropertyToID("_WeatherLightRayStrandShape2");
        private static readonly int Evolution0Id =
            Shader.PropertyToID("_WeatherLightRayEvolution0");
        private static readonly int Evolution1Id =
            Shader.PropertyToID("_WeatherLightRayEvolution1");
        private static readonly int SurfaceShapeId =
            Shader.PropertyToID("_WeatherLightRaySurfaceShape");
        private static readonly int IlluminationId =
            Shader.PropertyToID("_WeatherLightRayIllumination");
        private static readonly int ScatterDirectionId =
            Shader.PropertyToID("_WeatherLightRayScatterDirection");
        private static readonly int ScatterParametersId =
            Shader.PropertyToID("_WeatherLightRayScatterParameters");
        private static readonly int DebugModeId =
            Shader.PropertyToID("_WeatherLightRayDebugMode");
        private static readonly int MaskTextureId =
            Shader.PropertyToID("_WeatherLightRayMaskTexture");
        private static readonly int ScatterTextureId =
            Shader.PropertyToID("_WeatherLightRayScatterTexture");

        private sealed class MaskPassData
        {
            public Material Material;
            public Mesh ProxyMesh;
            public Matrix4x4 ProxyMatrix;
            public ShaderParameters Parameters;
        }

        private sealed class ScatterPassData
        {
            public Material Material;
            public TextureHandle Source;
            public Vector4 ScatterDirection;
            public Vector4 ScatterParameters;
        }

        private sealed class CompositePassData
        {
            public Material Material;
            public TextureHandle Source;
            public ShaderParameters Parameters;
            public Vector4 ScatterDirection;
        }

        private struct ShaderParameters
        {
            public Vector4 BaseCentreHeight;
            public Vector4 RayDirectionBaseRadius;
            public Vector4 TopShape;
            public Vector4 Colour;
            public Vector4 Intensity;
            public Vector4 CloudParameters;
            public Vector4 StrandShape0;
            public Vector4 StrandShape1;
            public Vector4 StrandShape2;
            public Vector4 Evolution0;
            public Vector4 Evolution1;
            public Vector4 SurfaceShape;
            public Vector4 Illumination;
            public Vector4 ScatterParameters;
            public float DebugMode;
        }

        private readonly Material maskMaterial;
        private readonly Material scatterMaterial;
        private readonly Material compositeMaterial;
        private Mesh proxyMesh;
        private WeatherLightRaySnapshot snapshot;
        private WeatherLightRaySourceState sourceState;
        private WeatherLightRayRenderDebugView debugView;
        private Camera renderCamera;
        private bool hasSetup;

        public WeatherLightRayRenderPass(
            Material maskMaterial,
            Material scatterMaterial,
            Material compositeMaterial)
        {
            this.maskMaterial = maskMaterial;
            this.scatterMaterial = scatterMaterial;
            this.compositeMaterial = compositeMaterial;
            proxyMesh = CreateProxyMesh();
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public void Setup(
            WeatherLightRaySnapshot snapshot,
            WeatherLightRaySourceState sourceState,
            WeatherLightRayRenderDebugView debugView,
            Camera renderCamera)
        {
            this.snapshot = snapshot;
            this.sourceState = sourceState;
            this.debugView = debugView;
            this.renderCamera = renderCamera;
            hasSetup = true;
        }

        public override void RecordRenderGraph(
            RenderGraph renderGraph,
            ContextContainer frameData)
        {
            if (!hasSetup ||
                proxyMesh == null ||
                maskMaterial == null ||
                scatterMaterial == null ||
                compositeMaterial == null ||
                renderCamera == null)
            {
                return;
            }

            UniversalResourceData resourceData =
                frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData =
                frameData.Get<UniversalCameraData>();
            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            TextureHandle sourceColour = resourceData.activeColorTexture;
            TextureHandle depthTexture = resourceData.cameraDepthTexture;
            RenderTextureDescriptor cameraDescriptor =
                cameraData.cameraTargetDescriptor;
            ShaderParameters parameters = BuildShaderParameters();
            Matrix4x4 proxyMatrix = BuildProxyMatrix();
            Vector4 scatterDirection = BuildScatterDirection(
                cameraDescriptor.width,
                cameraDescriptor.height);

            RenderTextureDescriptor maskDescriptor = cameraDescriptor;
            maskDescriptor.width = Mathf.Max(
                1,
                cameraDescriptor.width / DownsampleDivisor);
            maskDescriptor.height = Mathf.Max(
                1,
                cameraDescriptor.height / DownsampleDivisor);
            maskDescriptor.depthBufferBits = 0;
            maskDescriptor.msaaSamples = 1;
            maskDescriptor.graphicsFormat =
                GraphicsFormat.R16G16B16A16_SFloat;
            maskDescriptor.sRGB = false;

            TextureHandle maskTexture =
                UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    maskDescriptor,
                    "_WeatherLightRayMaskTexture",
                    true);

            RenderTextureDescriptor scatterDescriptor = maskDescriptor;
            scatterDescriptor.graphicsFormat = GraphicsFormat.R16_SFloat;
            TextureHandle scatterTexture =
                UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    scatterDescriptor,
                    "_WeatherLightRayScatterTexture",
                    true);

            RenderTextureDescriptor destinationDescriptor = cameraDescriptor;
            destinationDescriptor.depthBufferBits = 0;
            destinationDescriptor.msaaSamples = 1;
            TextureHandle destination =
                UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    destinationDescriptor,
                    "_WeatherLightRayCameraColor",
                    false);

            RecordMaskPass(
                renderGraph,
                depthTexture,
                maskTexture,
                proxyMatrix,
                parameters);
            RecordScatterPass(
                renderGraph,
                maskTexture,
                depthTexture,
                scatterTexture,
                scatterDirection,
                parameters.ScatterParameters);
            RecordCompositePass(
                renderGraph,
                sourceColour,
                depthTexture,
                maskTexture,
                scatterTexture,
                destination,
                parameters,
                scatterDirection);

            resourceData.cameraColor = destination;
        }

        public void Dispose()
        {
            if (proxyMesh != null)
            {
                CoreUtils.Destroy(proxyMesh);
                proxyMesh = null;
            }
        }

        private void RecordMaskPass(
            RenderGraph renderGraph,
            TextureHandle depthTexture,
            TextureHandle maskTexture,
            Matrix4x4 proxyMatrix,
            ShaderParameters parameters)
        {
            using (var builder =
                renderGraph.AddRasterRenderPass<MaskPassData>(
                    "Weather LightRay Mask",
                    out MaskPassData passData))
            {
                passData.Material = maskMaterial;
                passData.ProxyMesh = proxyMesh;
                passData.ProxyMatrix = proxyMatrix;
                passData.Parameters = parameters;

                builder.UseTexture(depthTexture, AccessFlags.Read);
                builder.SetRenderAttachment(
                    maskTexture,
                    0,
                    AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(
                    maskTexture,
                    MaskTextureId);
                builder.AllowGlobalStateModification(true);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc(
                    static (MaskPassData data, RasterGraphContext context) =>
                    {
                        context.cmd.SetGlobalVector(
                            BaseCentreHeightId,
                            data.Parameters.BaseCentreHeight);
                        context.cmd.SetGlobalVector(
                            RayDirectionBaseRadiusId,
                            data.Parameters.RayDirectionBaseRadius);
                        context.cmd.SetGlobalVector(
                            TopShapeId,
                            data.Parameters.TopShape);
                        context.cmd.SetGlobalVector(
                            ColourId,
                            data.Parameters.Colour);
                        context.cmd.SetGlobalVector(
                            IntensityId,
                            data.Parameters.Intensity);
                        context.cmd.SetGlobalVector(
                            CloudParametersId,
                            data.Parameters.CloudParameters);
                        context.cmd.SetGlobalVector(
                            StrandShape0Id,
                            data.Parameters.StrandShape0);
                        context.cmd.SetGlobalVector(
                            StrandShape1Id,
                            data.Parameters.StrandShape1);
                        context.cmd.SetGlobalVector(
                            StrandShape2Id,
                            data.Parameters.StrandShape2);
                        context.cmd.SetGlobalVector(
                            Evolution0Id,
                            data.Parameters.Evolution0);
                        context.cmd.SetGlobalVector(
                            Evolution1Id,
                            data.Parameters.Evolution1);
                        context.cmd.SetGlobalVector(
                            SurfaceShapeId,
                            data.Parameters.SurfaceShape);
                        context.cmd.SetGlobalVector(
                            IlluminationId,
                            data.Parameters.Illumination);
                        context.cmd.SetGlobalVector(
                            ScatterParametersId,
                            data.Parameters.ScatterParameters);
                        context.cmd.SetGlobalFloat(
                            DebugModeId,
                            data.Parameters.DebugMode);
                        context.cmd.DrawMesh(
                            data.ProxyMesh,
                            data.ProxyMatrix,
                            data.Material,
                            0,
                            0);
                    });
            }
        }

        private void RecordScatterPass(
            RenderGraph renderGraph,
            TextureHandle maskTexture,
            TextureHandle depthTexture,
            TextureHandle scatterTexture,
            Vector4 scatterDirection,
            Vector4 scatterParameters)
        {
            using (var builder =
                renderGraph.AddRasterRenderPass<ScatterPassData>(
                    "Weather LightRay Scatter",
                    out ScatterPassData passData))
            {
                passData.Material = scatterMaterial;
                passData.Source = maskTexture;
                passData.ScatterDirection = scatterDirection;
                passData.ScatterParameters = scatterParameters;

                builder.UseTexture(maskTexture, AccessFlags.Read);
                builder.UseTexture(depthTexture, AccessFlags.Read);
                builder.SetRenderAttachment(
                    scatterTexture,
                    0,
                    AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(
                    scatterTexture,
                    ScatterTextureId);
                builder.AllowGlobalStateModification(true);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc(
                    static (ScatterPassData data, RasterGraphContext context) =>
                    {
                        context.cmd.SetGlobalVector(
                            ScatterDirectionId,
                            data.ScatterDirection);
                        context.cmd.SetGlobalVector(
                            ScatterParametersId,
                            data.ScatterParameters);
                        Blitter.BlitTexture(
                            context.cmd,
                            data.Source,
                            new Vector4(1f, 1f, 0f, 0f),
                            data.Material,
                            0);
                    });
            }
        }

        private void RecordCompositePass(
            RenderGraph renderGraph,
            TextureHandle sourceColour,
            TextureHandle depthTexture,
            TextureHandle maskTexture,
            TextureHandle scatterTexture,
            TextureHandle destination,
            ShaderParameters parameters,
            Vector4 scatterDirection)
        {
            using (var builder =
                renderGraph.AddRasterRenderPass<CompositePassData>(
                    "Weather LightRay Composite",
                    out CompositePassData passData))
            {
                passData.Material = compositeMaterial;
                passData.Source = sourceColour;
                passData.Parameters = parameters;
                passData.ScatterDirection = scatterDirection;

                builder.UseTexture(sourceColour, AccessFlags.Read);
                builder.UseTexture(maskTexture, AccessFlags.Read);
                builder.UseTexture(scatterTexture, AccessFlags.Read);
                builder.UseTexture(depthTexture, AccessFlags.Read);
                builder.SetRenderAttachment(
                    destination,
                    0,
                    AccessFlags.Write);
                builder.AllowGlobalStateModification(true);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc(
                    static (CompositePassData data, RasterGraphContext context) =>
                    {
                        context.cmd.SetGlobalVector(
                            BaseCentreHeightId,
                            data.Parameters.BaseCentreHeight);
                        context.cmd.SetGlobalVector(
                            RayDirectionBaseRadiusId,
                            data.Parameters.RayDirectionBaseRadius);
                        context.cmd.SetGlobalVector(
                            TopShapeId,
                            data.Parameters.TopShape);
                        context.cmd.SetGlobalVector(
                            ColourId,
                            data.Parameters.Colour);
                        context.cmd.SetGlobalVector(
                            IntensityId,
                            data.Parameters.Intensity);
                        context.cmd.SetGlobalVector(
                            CloudParametersId,
                            data.Parameters.CloudParameters);
                        context.cmd.SetGlobalVector(
                            StrandShape0Id,
                            data.Parameters.StrandShape0);
                        context.cmd.SetGlobalVector(
                            StrandShape1Id,
                            data.Parameters.StrandShape1);
                        context.cmd.SetGlobalVector(
                            StrandShape2Id,
                            data.Parameters.StrandShape2);
                        context.cmd.SetGlobalVector(
                            Evolution0Id,
                            data.Parameters.Evolution0);
                        context.cmd.SetGlobalVector(
                            Evolution1Id,
                            data.Parameters.Evolution1);
                        context.cmd.SetGlobalVector(
                            SurfaceShapeId,
                            data.Parameters.SurfaceShape);
                        context.cmd.SetGlobalVector(
                            IlluminationId,
                            data.Parameters.Illumination);
                        context.cmd.SetGlobalVector(
                            ScatterParametersId,
                            data.Parameters.ScatterParameters);
                        context.cmd.SetGlobalFloat(
                            DebugModeId,
                            data.Parameters.DebugMode);
                        context.cmd.SetGlobalVector(
                            ScatterDirectionId,
                            data.ScatterDirection);
                        Blitter.BlitTexture(
                            context.cmd,
                            data.Source,
                            new Vector4(1f, 1f, 0f, 0f),
                            data.Material,
                            0);
                    });
            }
        }

        private ShaderParameters BuildShaderParameters()
        {
            WeatherLightRayDescriptor descriptor = snapshot.Descriptor;
            float baseRadius = Mathf.Max(
                0.001f,
                descriptor.BaseEllipseAxes.x);
            float topRadius = Mathf.Max(
                0.001f,
                descriptor.TopEllipseAxes.x);
            Vector3 rayDirection = snapshot.RayDirectionWorld.sqrMagnitude >
                0.000001f
                    ? snapshot.RayDirectionWorld.normalized
                    : Vector3.down;
            float sourceIntensity = sourceState.SourceLight != null
                ? sourceState.Intensity
                : 1f;
            Color sourceColour = sourceState.SourceLight != null
                ? sourceState.Colour
                : Color.white;
            if (descriptor.SourceKind ==
                WeatherLightRaySourceKind.Sun)
            {
                Color warmSunColour = new Color(
                    1f,
                    0.76f,
                    0.46f,
                    1f);
                sourceColour = Color.Lerp(
                    sourceColour,
                    warmSunColour,
                    descriptor.WarmthContribution);
            }

            Color rayColour = sourceColour *
                sourceIntensity *
                descriptor.ColourMultiplier;
            float phase = (descriptor.VariationSeed % 10007u) /
                10007f * Mathf.PI * 2f;
            float seed01 = (descriptor.VariationSeed % 8191u) / 8191f;
            float presentationTime = Application.isPlaying
                ? Time.time
                : Time.realtimeSinceStartup;
            WeatherCloudShadowController cloudController =
                WeatherCloudShadowController.PublishedController;
            float shadedTransmission = cloudController != null
                ? cloudController.ShadedTransmission
                : 1f;
            float cloudPolicyFlag = descriptor.CloudPolicy ==
                WeatherLightRayCloudPolicy.IgnoreClouds
                    ? 1f
                    : 0f;
            bool cookieActive = cloudController != null &&
                cloudController.CloudShadowsEnabled &&
                cloudController.CookieReady &&
                cloudController.SunGateActive &&
                cloudController.ResolvedSun == sourceState.SourceLight;

            return new ShaderParameters
            {
                BaseCentreHeight = new Vector4(
                    snapshot.BaseCentreWorld.x,
                    snapshot.BaseCentreWorld.y,
                    snapshot.BaseCentreWorld.z,
                    descriptor.Height),
                RayDirectionBaseRadius = new Vector4(
                    rayDirection.x,
                    rayDirection.y,
                    rayDirection.z,
                    baseRadius),
                TopShape = new Vector4(
                    topRadius,
                    descriptor.VisualEnvelopeRadiusScale,
                    descriptor.VisualEnvelopeEdgeSoftness,
                    cloudPolicyFlag),
                Colour = rayColour,
                Intensity = new Vector4(
                    snapshot.CurrentIntensity,
                    descriptor.StrandIntensity,
                    descriptor.EnvelopeHazeIntensity,
                    1f),
                CloudParameters = new Vector4(
                    shadedTransmission,
                    sourceState.Available ? 1f : 0f,
                    cookieActive ? 1f : 0f,
                    1f),
                StrandShape0 = new Vector4(
                    descriptor.StrandCount,
                    descriptor.StrandWidthRange.x,
                    descriptor.StrandWidthRange.y,
                    descriptor.StrandSpread),
                StrandShape1 = new Vector4(
                    descriptor.StrandPositionVariation,
                    descriptor.StrandIntensityVariation,
                    descriptor.StrandLengthVariation,
                    descriptor.StrandTaper),
                StrandShape2 = new Vector4(
                    descriptor.StrandEdgeSoftness,
                    descriptor.StrandClusterBias,
                    descriptor.PerStrandPhaseVariation,
                    seed01),
                Evolution0 = new Vector4(
                    descriptor.IntensityFluctuationStrength,
                    descriptor.IntensityFluctuationSpeed,
                    descriptor.WidthBreathingStrength,
                    descriptor.LateralDriftStrength),
                Evolution1 = new Vector4(
                    descriptor.PatternEvolutionSpeed,
                    phase,
                    presentationTime,
                    0f),
                SurfaceShape = new Vector4(
                    descriptor.FootprintEdgeSoftness,
                    descriptor.FootprintIrregularity,
                    descriptor.HeightFade,
                    descriptor.CameraIntersectionFade),
                Illumination = new Vector4(
                    descriptor.GroundLightMultiplier,
                    descriptor.VisibleSurfaceLightMultiplier,
                    descriptor.CloudCompensationMultiplier,
                    descriptor.CoreEmphasis),
                ScatterParameters = new Vector4(
                    descriptor.ScatterLength,
                    descriptor.ScatterSoftness,
                    0f,
                    0f),
                DebugMode = (float)debugView
            };
        }

        private Matrix4x4 BuildProxyMatrix()
        {
            WeatherLightRayDescriptor descriptor = snapshot.Descriptor;
            Vector3 rayDirection = snapshot.RayDirectionWorld.sqrMagnitude >
                0.000001f
                    ? snapshot.RayDirectionWorld.normalized
                    : Vector3.down;
            Vector3 upwardAxis = -rayDirection;
            Quaternion rotation = Quaternion.FromToRotation(
                Vector3.up,
                upwardAxis);
            float maximumRadius = Mathf.Max(
                descriptor.BaseEllipseAxes.x,
                descriptor.TopEllipseAxes.x);
            maximumRadius *= Mathf.Max(
                1f,
                descriptor.VisualEnvelopeRadiusScale);
            maximumRadius *= 1f + descriptor.LateralDriftStrength;
            maximumRadius *= 1f / Mathf.Cos(
                Mathf.PI / ProxySides);
            return Matrix4x4.TRS(
                snapshot.BaseCentreWorld,
                rotation,
                new Vector3(
                    maximumRadius,
                    descriptor.Height,
                    maximumRadius));
        }

        private Vector4 BuildScatterDirection(
            int fullWidth,
            int fullHeight)
        {
            Vector3 rayDirection = snapshot.RayDirectionWorld.sqrMagnitude >
                0.000001f
                    ? snapshot.RayDirectionWorld.normalized
                    : Vector3.down;
            Vector3 topCentre = snapshot.BaseCentreWorld -
                rayDirection * snapshot.Descriptor.Height;
            Vector3 baseViewport = renderCamera.WorldToViewportPoint(
                snapshot.BaseCentreWorld);
            Vector3 topViewport = renderCamera.WorldToViewportPoint(
                topCentre);
            Vector2 direction = new Vector2(
                baseViewport.x - topViewport.x,
                baseViewport.y - topViewport.y);
            if (direction.sqrMagnitude <= 0.000001f)
            {
                direction = Vector2.up;
            }
            else
            {
                direction.Normalize();
            }

            int width = Mathf.Max(1, fullWidth / DownsampleDivisor);
            int height = Mathf.Max(1, fullHeight / DownsampleDivisor);
            return new Vector4(
                direction.x,
                direction.y,
                1f / width,
                1f / height);
        }

        private static Mesh CreateProxyMesh()
        {
            var vertices = new Vector3[ProxySides * 2 + 2];
            int baseCentreIndex = ProxySides * 2;
            int topCentreIndex = baseCentreIndex + 1;
            vertices[baseCentreIndex] = Vector3.zero;
            vertices[topCentreIndex] = Vector3.up;

            for (int side = 0; side < ProxySides; side++)
            {
                float angle = side / (float)ProxySides *
                    Mathf.PI * 2f;
                float x = Mathf.Cos(angle);
                float z = Mathf.Sin(angle);
                vertices[side] = new Vector3(x, 0f, z);
                vertices[ProxySides + side] = new Vector3(x, 1f, z);
            }

            var triangles = new int[ProxySides * 12];
            int triangleIndex = 0;
            for (int side = 0; side < ProxySides; side++)
            {
                int next = (side + 1) % ProxySides;
                int baseA = side;
                int baseB = next;
                int topA = ProxySides + side;
                int topB = ProxySides + next;

                triangles[triangleIndex++] = baseA;
                triangles[triangleIndex++] = topA;
                triangles[triangleIndex++] = topB;
                triangles[triangleIndex++] = baseA;
                triangles[triangleIndex++] = topB;
                triangles[triangleIndex++] = baseB;

                triangles[triangleIndex++] = baseCentreIndex;
                triangles[triangleIndex++] = baseA;
                triangles[triangleIndex++] = baseB;

                triangles[triangleIndex++] = topCentreIndex;
                triangles[triangleIndex++] = topB;
                triangles[triangleIndex++] = topA;
            }

            var mesh = new Mesh
            {
                name = "Weather LightRay Structured Proxy",
                hideFlags = HideFlags.HideAndDontSave,
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateBounds();
            mesh.UploadMeshData(true);
            return mesh;
        }
    }
}
