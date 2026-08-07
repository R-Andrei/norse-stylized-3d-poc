using System;
using System.Runtime.InteropServices;
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
        private const int MinimumBeamCount =
            WeatherLightRayAreaLayout.MinimumBeamCount;
        private static readonly int BaseCentreHeightId =
            Shader.PropertyToID("_WeatherLightRayBaseCentreHeight");
        private static readonly int RayDirectionAreaDiameterId =
            Shader.PropertyToID("_WeatherLightRayDirectionAreaDiameter");
        private static readonly int GroundContactAxisWorldId =
            Shader.PropertyToID("_WeatherLightRayGroundContactAxisWorld");
        private static readonly int ColourId =
            Shader.PropertyToID("_WeatherLightRayColour");
        private static readonly int IntensityId =
            Shader.PropertyToID("_WeatherLightRayIntensity");
        private static readonly int BeamShape0Id =
            Shader.PropertyToID("_WeatherLightRayBeamShape0");
        private static readonly int BeamShape1Id =
            Shader.PropertyToID("_WeatherLightRayBeamShape1");
        private static readonly int BeamShape2Id =
            Shader.PropertyToID("_WeatherLightRayBeamShape2");
        private static readonly int SofteningDirectionId =
            Shader.PropertyToID("_WeatherLightRaySofteningDirection");
        private static readonly int BeamBufferId =
            Shader.PropertyToID("_WeatherLightRayBeamBuffer");
        private static readonly int ZoneBufferId =
            Shader.PropertyToID("_WeatherLightRayZoneBuffer");
        private static readonly int ZoneIndexId =
            Shader.PropertyToID("_WeatherLightRayZoneIndex");
        private static readonly int SofteningParametersId =
            Shader.PropertyToID("_WeatherLightRaySofteningParameters");
        private static readonly int SurfaceParameters0Id =
            Shader.PropertyToID("_WeatherLightRaySurfaceParameters0");
        private static readonly int SurfaceParameters1Id =
            Shader.PropertyToID("_WeatherLightRaySurfaceParameters1");
        private static readonly int SurfaceScreenBoundsId =
            Shader.PropertyToID("_WeatherLightRaySurfaceScreenBounds");
        private static readonly int DebugModeId =
            Shader.PropertyToID("_WeatherLightRayDebugMode");
        private static readonly int MaskTextureId =
            Shader.PropertyToID("_WeatherLightRayMaskTexture");
        private static readonly int SoftenedTextureId =
            Shader.PropertyToID("_WeatherLightRaySoftenedTexture");

        [StructLayout(LayoutKind.Sequential)]
        private struct BeamRecord
        {
            public Vector4 A0;
            public Vector4 A1;
            public Vector4 A2;
            public Vector4 B0;
            public Vector4 B1;
            public Vector4 B2;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ZoneRecord
        {
            public Vector4 Data;
        }

        private struct ZoneDraw
        {
            public ShaderParameters Parameters;
            public int BeamCount;
            public int ZoneIndex;
            public int GroupIndex;
        }

        private struct PresentationGroup
        {
            public ShaderParameters Parameters;
            public Vector4 SofteningDirection;
            public int DrawCount;
            public bool HasScreenSpaceSurfaceResponse;
        }

        private sealed class MaskPassData
        {
            public Material Material;
            public ZoneDraw[] Draws;
            public int DrawCount;
            public GraphicsBuffer BeamBuffer;
            public GraphicsBuffer ZoneBuffer;
            public int GroupIndex;
        }

        private sealed class SofteningPassData
        {
            public Material Material;
            public TextureHandle Source;
            public Vector4 SofteningDirection;
            public Vector4 SofteningParameters;
        }

        private sealed class CompositePassData
        {
            public Material Material;
            public TextureHandle Source;
            public ShaderParameters Parameters;
            public Vector4 SofteningDirection;
        }

        private struct ShaderParameters
        {
            public Vector4 BaseCentreHeight;
            public Vector4 RayDirectionAreaDiameter;
            public Vector4 GroundContactAxisWorld;
            public Vector4 Colour;
            public Vector4 Intensity;
            public Vector4 BeamShape0;
            public Vector4 BeamShape1;
            public Vector4 BeamShape2;
            public Vector4 SofteningParameters;
            public Vector4 SurfaceParameters0;
            public Vector4 SurfaceParameters1;
            public Vector4 SurfaceScreenBounds;
            public float DebugMode;
        }

        private readonly Material maskMaterial;
        private readonly Material softeningMaterial;
        private readonly Material compositeMaterial;
        private WeatherLightRaySnapshot[] snapshots;
        private int snapshotCount;
        private WeatherLightRayRenderDebugView debugView;
        private Camera renderCamera;
        private bool hasSetup;

        private GraphicsBuffer beamBuffer;
        private GraphicsBuffer zoneBuffer;
        private BeamRecord[] beamRecords = Array.Empty<BeamRecord>();
        private ZoneRecord[] zoneRecords = Array.Empty<ZoneRecord>();
        private ZoneDraw[] zoneDraws = Array.Empty<ZoneDraw>();
        private PresentationGroup[] presentationGroups =
            Array.Empty<PresentationGroup>();
        private int presentationGroupCount;
        private float[] temporaryWidths = Array.Empty<float>();
        private float[] temporaryOverlaps = Array.Empty<float>();
        private float[] temporaryIntensities = Array.Empty<float>();
        private readonly Plane[] frustumPlanes = new Plane[6];
        private int beamCapacity;
        private int zoneCapacity;
        private ulong endpointSignature;
        private bool endpointDataValid;
        private int endpointUploadCount;
        private int zoneUploadCount;

        public static int LastVisibleZoneCount { get; private set; }
        public static int LastBufferedZoneCount { get; private set; }
        public static int LastTotalBeamCount { get; private set; }
        public static int LastBeamBufferCapacity { get; private set; }
        public static int LastZoneBufferCapacity { get; private set; }
        public static int LastEndpointUploadCount { get; private set; }
        public static int LastZoneUploadCount { get; private set; }
        public static int LastPresentationGroupCount { get; private set; }

        public WeatherLightRayRenderPass(
            Material maskMaterial,
            Material scatterMaterial,
            Material compositeMaterial)
        {
            this.maskMaterial = maskMaterial;
            softeningMaterial = scatterMaterial;
            this.compositeMaterial = compositeMaterial;
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public void Setup(
            WeatherLightRaySnapshot[] snapshots,
            int snapshotCount,
            WeatherLightRayRenderDebugView debugView,
            Camera renderCamera)
        {
            this.snapshots = snapshots;
            this.snapshotCount = Mathf.Max(0, snapshotCount);
            this.debugView = debugView;
            this.renderCamera = renderCamera;
            hasSetup = snapshots != null && snapshotCount > 0;
        }

        public override void RecordRenderGraph(
            RenderGraph renderGraph,
            ContextContainer frameData)
        {
            if (!hasSetup ||
                maskMaterial == null ||
                softeningMaterial == null ||
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

            RenderTextureDescriptor cameraDescriptor =
                cameraData.cameraTargetDescriptor;
            int drawCount = PrepareZoneData(
                cameraDescriptor.width,
                cameraDescriptor.height);
            if (drawCount <= 0 ||
                presentationGroupCount <= 0 ||
                beamBuffer == null ||
                zoneBuffer == null)
            {
                return;
            }

            TextureHandle currentColour = resourceData.activeColorTexture;
            TextureHandle depthTexture = resourceData.cameraDepthTexture;
            RenderTextureDescriptor atmosphereDescriptor =
                cameraDescriptor;
            atmosphereDescriptor.depthBufferBits = 0;
            atmosphereDescriptor.msaaSamples = 1;
            atmosphereDescriptor.graphicsFormat = GraphicsFormat.R16_SFloat;
            atmosphereDescriptor.sRGB = false;

            RenderTextureDescriptor destinationDescriptor = cameraDescriptor;
            destinationDescriptor.depthBufferBits = 0;
            destinationDescriptor.msaaSamples = 1;

            for (int groupIndex = 0;
                groupIndex < presentationGroupCount;
                groupIndex++)
            {
                PresentationGroup group = presentationGroups[groupIndex];
                TextureHandle maskTexture =
                    UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph,
                        atmosphereDescriptor,
                        "_WeatherLightRayMaskTexture",
                        true);
                TextureHandle softenedTexture =
                    UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph,
                        atmosphereDescriptor,
                        "_WeatherLightRaySoftenedTexture",
                        true);
                TextureHandle destination =
                    UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph,
                        destinationDescriptor,
                        "_WeatherLightRayCameraColor",
                        false);

                RecordMaskPass(
                    renderGraph,
                    currentColour,
                    depthTexture,
                    maskTexture,
                    drawCount,
                    groupIndex);
                RecordSofteningPass(
                    renderGraph,
                    maskTexture,
                    softenedTexture,
                    group.SofteningDirection,
                    group.Parameters.SofteningParameters);
                RecordCompositePass(
                    renderGraph,
                    currentColour,
                    depthTexture,
                    maskTexture,
                    softenedTexture,
                    destination,
                    group.Parameters,
                    group.SofteningDirection);
                currentColour = destination;
            }

            resourceData.cameraColor = currentColour;
        }

        public void Dispose()
        {
            ReleaseBuffers();
        }

        private void RecordMaskPass(
            RenderGraph renderGraph,
            TextureHandle sequenceDependency,
            TextureHandle depthTexture,
            TextureHandle maskTexture,
            int drawCount,
            int groupIndex)
        {
            using (var builder =
                renderGraph.AddRasterRenderPass<MaskPassData>(
                    "Weather LightRay Multi-Zone Beam Mask",
                    out MaskPassData passData))
            {
                passData.Material = maskMaterial;
                passData.Draws = zoneDraws;
                passData.DrawCount = drawCount;
                passData.BeamBuffer = beamBuffer;
                passData.ZoneBuffer = zoneBuffer;
                passData.GroupIndex = groupIndex;

                // Each group reuses the same global mask/softened shader IDs.
                // Reading the previous camera-colour result deliberately chains
                // the next group's mask after the previous group's composite,
                // preventing RenderGraph from reordering a later group's global
                // texture publication ahead of an earlier composite.
                builder.UseTexture(sequenceDependency, AccessFlags.Read);
                builder.UseTexture(depthTexture, AccessFlags.Read);
                builder.SetRenderAttachment(maskTexture, 0, AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(maskTexture, MaskTextureId);
                builder.AllowGlobalStateModification(true);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc(
                    static (MaskPassData data, RasterGraphContext context) =>
                    {
                        context.cmd.SetGlobalBuffer(BeamBufferId, data.BeamBuffer);
                        context.cmd.SetGlobalBuffer(ZoneBufferId, data.ZoneBuffer);
                        for (int drawIndex = 0;
                            drawIndex < data.DrawCount;
                            drawIndex++)
                        {
                            ZoneDraw draw = data.Draws[drawIndex];
                            if (draw.GroupIndex != data.GroupIndex)
                            {
                                continue;
                            }
                            ApplyShaderParameters(context.cmd, draw.Parameters);
                            context.cmd.SetGlobalInt(ZoneIndexId, draw.ZoneIndex);
                            context.cmd.DrawProcedural(
                                Matrix4x4.identity,
                                data.Material,
                                0,
                                MeshTopology.Triangles,
                                draw.BeamCount * 6,
                                1);
                        }
                    });
            }
        }

        private static void ApplyShaderParameters(
            RasterCommandBuffer command,
            ShaderParameters parameters)
        {
            command.SetGlobalVector(BaseCentreHeightId, parameters.BaseCentreHeight);
            command.SetGlobalVector(RayDirectionAreaDiameterId, parameters.RayDirectionAreaDiameter);
            command.SetGlobalVector(GroundContactAxisWorldId, parameters.GroundContactAxisWorld);
            command.SetGlobalVector(ColourId, parameters.Colour);
            command.SetGlobalVector(IntensityId, parameters.Intensity);
            command.SetGlobalVector(BeamShape0Id, parameters.BeamShape0);
            command.SetGlobalVector(BeamShape1Id, parameters.BeamShape1);
            command.SetGlobalVector(BeamShape2Id, parameters.BeamShape2);
            command.SetGlobalVector(SofteningParametersId, parameters.SofteningParameters);
            command.SetGlobalVector(SurfaceParameters0Id, parameters.SurfaceParameters0);
            command.SetGlobalVector(SurfaceParameters1Id, parameters.SurfaceParameters1);
            command.SetGlobalVector(SurfaceScreenBoundsId, parameters.SurfaceScreenBounds);
            command.SetGlobalFloat(DebugModeId, parameters.DebugMode);
        }

        private void RecordSofteningPass(
            RenderGraph renderGraph,
            TextureHandle maskTexture,
            TextureHandle softenedTexture,
            Vector4 softeningDirection,
            Vector4 softeningParameters)
        {
            using (var builder =
                renderGraph.AddRasterRenderPass<SofteningPassData>(
                    "Weather LightRay Gap-Preserving Softening",
                    out SofteningPassData passData))
            {
                passData.Material = softeningMaterial;
                passData.Source = maskTexture;
                passData.SofteningDirection = softeningDirection;
                passData.SofteningParameters = softeningParameters;

                builder.UseTexture(maskTexture, AccessFlags.Read);
                builder.SetRenderAttachment(
                    softenedTexture,
                    0,
                    AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(
                    softenedTexture,
                    SoftenedTextureId);
                builder.AllowGlobalStateModification(true);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc(
                    static (
                        SofteningPassData data,
                        RasterGraphContext context) =>
                    {
                        context.cmd.SetGlobalVector(
                            SofteningDirectionId,
                            data.SofteningDirection);
                        context.cmd.SetGlobalVector(
                            SofteningParametersId,
                            data.SofteningParameters);
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
            TextureHandle softenedTexture,
            TextureHandle destination,
            ShaderParameters parameters,
            Vector4 softeningDirection)
        {
            using (var builder =
                renderGraph.AddRasterRenderPass<CompositePassData>(
                    "Weather LightRay Continuous Beam Composite",
                    out CompositePassData passData))
            {
                passData.Material = compositeMaterial;
                passData.Source = sourceColour;
                passData.Parameters = parameters;
                passData.SofteningDirection = softeningDirection;

                builder.UseTexture(sourceColour, AccessFlags.Read);
                builder.UseTexture(maskTexture, AccessFlags.Read);
                builder.UseTexture(softenedTexture, AccessFlags.Read);
                builder.UseTexture(depthTexture, AccessFlags.Read);
                builder.SetRenderAttachment(
                    destination,
                    0,
                    AccessFlags.Write);
                builder.AllowGlobalStateModification(true);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc(
                    static (
                        CompositePassData data,
                        RasterGraphContext context) =>
                    {
                        ApplyShaderParameters(
                            context.cmd,
                            data.Parameters);
                        context.cmd.SetGlobalVector(
                            SofteningDirectionId,
                            data.SofteningDirection);
                        Blitter.BlitTexture(
                            context.cmd,
                            data.Source,
                            new Vector4(1f, 1f, 0f, 0f),
                            data.Material,
                            0);
                    });
            }
        }

        private ShaderParameters BuildShaderParameters(
            in WeatherLightRaySnapshot raySnapshot)
        {
            WeatherLightRayDescriptor descriptor = raySnapshot.Descriptor;
            Vector3 rayDirection = raySnapshot.RayDirectionWorld.sqrMagnitude >
                0.000001f
                    ? raySnapshot.RayDirectionWorld.normalized
                    : Vector3.down;
            Vector3 contactAxis = BuildGroundContactAxis();
            Color rayColour = raySnapshot.ResolvedSourceColour *
                raySnapshot.ResolvedSourceIntensity *
                descriptor.ColourMultiplier;
            // AI4A frozen endpoint interpolation is supplied by the persistent beam
            // and zone buffers.
            return new ShaderParameters
            {
                BaseCentreHeight = new Vector4(
                    raySnapshot.BaseCentreWorld.x,
                    raySnapshot.BaseCentreWorld.y,
                    raySnapshot.BaseCentreWorld.z,
                    descriptor.Height),
                RayDirectionAreaDiameter = new Vector4(
                    rayDirection.x,
                    rayDirection.y,
                    rayDirection.z,
                    descriptor.AreaDiameterMetres),
                GroundContactAxisWorld = new Vector4(
                    contactAxis.x,
                    contactAxis.y,
                    contactAxis.z,
                    0f),
                Colour = rayColour,
                Intensity = new Vector4(
                    raySnapshot.CurrentIntensity *
                        descriptor.AtmosphericIntensity,
                    1f,
                    descriptor.CameraIntersectionFade,
                    0f),
                BeamShape0 = new Vector4(
                    descriptor.BeamCount,
                    descriptor.BeamWidthRatioRange.x,
                    descriptor.BeamWidthRatioRange.y,
                    descriptor.BeamEdgeSoftness),
                BeamShape1 = new Vector4(
                    descriptor.UpperFade,
                    descriptor.GroundFade,
                    descriptor.BeamIntensityVariation,
                    0f),
                BeamShape2 = new Vector4(
                    descriptor.BeamSoftnessVariation,
                    descriptor.FootprintRadiusMetres,
                    WeatherLightRayAreaLayout.MinimumAdjacentOverlapRatio,
                    WeatherLightRayAreaLayout.MaximumAdjacentOverlapRatio),
                SofteningParameters = new Vector4(
                    descriptor.SofteningStrength,
                    0f,
                    0f,
                    0f),
                SurfaceParameters0 = new Vector4(
                    descriptor.ScreenSpaceSurfaceIntensity,
                    descriptor.FootprintEdgeSoftness,
                    raySnapshot.CurrentIntensity,
                    0f),
                SurfaceParameters1 = new Vector4(
                    descriptor.FootprintRadiusMetres,
                    descriptor.AreaDiameterMetres,
                    descriptor.BeamPitchMetres,
                    0f),
                SurfaceScreenBounds = BuildSurfaceScreenBounds(
                    raySnapshot,
                    rayDirection,
                    contactAxis,
                    descriptor),
                DebugMode = (float)debugView
            };
        }


        private static Vector3 BuildGroundContactAxis()
        {
            return Vector3.right;
        }

        private Vector4 BuildSofteningDirection(
            in WeatherLightRaySnapshot raySnapshot,
            Vector4 contactAxisWorld,
            float representativeBeamWidthMetres,
            int fullWidth,
            int fullHeight,
            out float softeningRadiusPixels)
        {
            int width = Mathf.Max(1, fullWidth);
            int height = Mathf.Max(1, fullHeight);
            Vector3 contactAxis = new Vector3(
                contactAxisWorld.x,
                contactAxisWorld.y,
                contactAxisWorld.z);
            Vector3 baseViewport = renderCamera.WorldToViewportPoint(
                raySnapshot.BaseCentreWorld);
            Vector3 unitViewport = renderCamera.WorldToViewportPoint(
                raySnapshot.BaseCentreWorld + contactAxis);
            Vector2 deltaViewport = new Vector2(
                unitViewport.x - baseViewport.x,
                unitViewport.y - baseViewport.y);
            Vector2 directionPixels = new Vector2(
                deltaViewport.x * width,
                deltaViewport.y * height);
            if (directionPixels.sqrMagnitude <= 0.000001f)
            {
                directionPixels = Vector2.right;
            }
            else
            {
                directionPixels.Normalize();
            }

            float projectedBeamWidthPixels = Mathf.Max(
                0.001f,
                new Vector2(
                    deltaViewport.x * width,
                    deltaViewport.y * height).magnitude *
                Mathf.Max(0.001f, representativeBeamWidthMetres));
            float strength = raySnapshot.Descriptor.SofteningStrength;
            softeningRadiusPixels = Mathf.Clamp(
                projectedBeamWidthPixels * Mathf.Lerp(
                    0.06f,
                    0.18f,
                    strength),
                1.5f,
                8f);
            return new Vector4(
                directionPixels.x / width,
                directionPixels.y / height,
                1f / width,
                1f / height);
        }

        private Vector4 BuildSurfaceScreenBounds(
            in WeatherLightRaySnapshot raySnapshot,
            Vector3 rayDirection,
            Vector3 contactAxis,
            WeatherLightRayDescriptor descriptor)
        {
            if (renderCamera == null)
            {
                return new Vector4(0f, 0f, 1f, 1f);
            }

            Vector3 upwardAxis = -rayDirection;
            Vector3 baseCentre = raySnapshot.BaseCentreWorld;
            Vector3 topCentre = baseCentre +
                upwardAxis * descriptor.Height;
            float bundleExtent = descriptor.FootprintRadiusMetres;
            float radialExtent = descriptor.FootprintRadiusMetres;
            Vector3 cameraRight = renderCamera.transform.right;
            Vector3 cameraUp = renderCamera.transform.up;
            Vector2 minimum = new Vector2(
                float.PositiveInfinity,
                float.PositiveInfinity);
            Vector2 maximum = new Vector2(
                float.NegativeInfinity,
                float.NegativeInfinity);

            bool valid = EncapsulateSurfaceEndpointBounds(
                baseCentre,
                contactAxis,
                bundleExtent,
                cameraRight,
                cameraUp,
                radialExtent,
                ref minimum,
                ref maximum);
            valid &= EncapsulateSurfaceEndpointBounds(
                topCentre,
                contactAxis,
                bundleExtent,
                cameraRight,
                cameraUp,
                radialExtent,
                ref minimum,
                ref maximum);

            if (!valid)
            {
                return new Vector4(0f, 0f, 1f, 1f);
            }

            const float margin = 0.025f;
            return new Vector4(
                Mathf.Clamp01(minimum.x - margin),
                Mathf.Clamp01(minimum.y - margin),
                Mathf.Clamp01(maximum.x + margin),
                Mathf.Clamp01(maximum.y + margin));
        }

        private bool EncapsulateSurfaceEndpointBounds(
            Vector3 endpointCentre,
            Vector3 contactAxis,
            float bundleExtent,
            Vector3 cameraRight,
            Vector3 cameraUp,
            float radialExtent,
            ref Vector2 minimum,
            ref Vector2 maximum)
        {
            bool valid = true;
            for (int bundleSign = -1;
                bundleSign <= 1;
                bundleSign += 2)
            {
                Vector3 beamEdgeCentre = endpointCentre +
                    contactAxis * (bundleExtent * bundleSign);
                for (int rightSign = -1;
                    rightSign <= 1;
                    rightSign += 2)
                {
                    for (int upSign = -1;
                        upSign <= 1;
                        upSign += 2)
                    {
                        Vector3 corner = beamEdgeCentre +
                            cameraRight * (radialExtent * rightSign) +
                            cameraUp * (radialExtent * upSign);
                        valid &= TryEncapsulateViewportPoint(
                            corner,
                            ref minimum,
                            ref maximum);
                    }
                }
            }

            return valid;
        }

        private bool TryEncapsulateViewportPoint(
            Vector3 worldPosition,
            ref Vector2 minimum,
            ref Vector2 maximum)
        {
            Vector3 viewport = renderCamera.WorldToViewportPoint(
                worldPosition);
            if (viewport.z <= 0.0001f)
            {
                return false;
            }

            Vector2 viewportPoint = new Vector2(
                viewport.x,
                viewport.y);
            minimum = Vector2.Min(minimum, viewportPoint);
            maximum = Vector2.Max(maximum, viewportPoint);
            return true;
        }

        private int PrepareZoneData(int renderWidth, int renderHeight)
        {
            int bufferedZoneCount = 0;
            int visibleZoneCount = 0;
            int totalBeamCount = 0;
            presentationGroupCount = 0;
            GeometryUtility.CalculateFrustumPlanes(renderCamera, frustumPlanes);
            for (int index = 0; index < snapshotCount; index++)
            {
                WeatherLightRaySnapshot candidate = snapshots[index];
                if (candidate.CurrentIntensity <= 0.0001f)
                {
                    continue;
                }

                bufferedZoneCount++;
                totalBeamCount += Mathf.Max(
                    MinimumBeamCount,
                    candidate.Descriptor.BeamCount);
                if (IsPotentiallyVisible(candidate))
                {
                    visibleZoneCount++;
                }
            }

            if (bufferedZoneCount <= 0 ||
                visibleZoneCount <= 0 ||
                totalBeamCount <= 0)
            {
                LastVisibleZoneCount = 0;
                LastBufferedZoneCount = bufferedZoneCount;
                LastTotalBeamCount = totalBeamCount;
                LastPresentationGroupCount = 0;
                return 0;
            }

            EnsureCpuCapacity(totalBeamCount, bufferedZoneCount);
            EnsureBufferCapacity(totalBeamCount, bufferedZoneCount);

            ulong signature = 1469598103934665603UL;
            int beamOffset = 0;
            int zoneIndex = 0;
            int drawIndex = 0;
            for (int index = 0; index < snapshotCount; index++)
            {
                WeatherLightRaySnapshot candidate = snapshots[index];
                if (candidate.CurrentIntensity <= 0.0001f)
                {
                    continue;
                }

                int beamCount = Mathf.Max(
                    MinimumBeamCount,
                    candidate.Descriptor.BeamCount);
                signature = MixSignature(signature, candidate.Handle.SlotIndex);
                signature = MixSignature(signature, (int)candidate.Handle.Generation);
                signature = MixSignature(signature, beamCount);
                signature = MixSignature(signature, (int)candidate.EvolutionCurrentSeed);
                signature = MixSignature(signature, (int)candidate.EvolutionNextSeed);
                signature = MixSignature(signature, FloatBits(candidate.Descriptor.AreaDiameterMetres));
                signature = MixSignature(signature, FloatBits(candidate.Descriptor.BeamSpacingMetres));
                signature = MixSignature(signature, FloatBits(candidate.Descriptor.BeamWidthRatioRange.x));
                signature = MixSignature(signature, FloatBits(candidate.Descriptor.BeamWidthRatioRange.y));
                signature = MixSignature(signature, FloatBits(candidate.Descriptor.BeamIntensityVariation));
                signature = MixSignature(signature, FloatBits(candidate.Descriptor.BeamEdgeSoftness));
                signature = MixSignature(signature, FloatBits(candidate.Descriptor.BeamSoftnessVariation));
                signature = MixSignature(signature, FloatBits(candidate.Descriptor.EvolutionStrength));

                zoneRecords[zoneIndex] = new ZoneRecord
                {
                    Data = new Vector4(
                        beamOffset,
                        beamCount,
                        candidate.EvolutionBlend,
                        candidate.Descriptor.ContactPlaneOpacity)
                };

                if (IsPotentiallyVisible(candidate))
                {
                    ShaderParameters parameters = BuildShaderParameters(candidate);
                    float representativeBeamWidth =
                        WeatherLightRayAreaLayout.Calculate(
                            candidate.Descriptor.AreaDiameterMetres,
                            candidate.Descriptor.BeamSpacingMetres)
                            .AverageAtmosphericBeamWidthMetres;
                    Vector4 softeningDirection = BuildSofteningDirection(
                        candidate,
                        parameters.GroundContactAxisWorld,
                        representativeBeamWidth,
                        renderWidth,
                        renderHeight,
                        out float softeningRadiusPixels);
                    parameters.SofteningParameters.y = softeningRadiusPixels;
                    int groupIndex = ResolvePresentationGroup(
                        parameters,
                        softeningDirection);
                    PresentationGroup group = presentationGroups[groupIndex];
                    group.DrawCount++;
                    presentationGroups[groupIndex] = group;
                    zoneDraws[drawIndex] = new ZoneDraw
                    {
                        Parameters = parameters,
                        BeamCount = beamCount,
                        ZoneIndex = zoneIndex,
                        GroupIndex = groupIndex
                    };
                    drawIndex++;
                }

                beamOffset += beamCount;
                zoneIndex++;
            }

            if (!endpointDataValid || signature != endpointSignature)
            {
                beamOffset = 0;
                for (int index = 0; index < snapshotCount; index++)
                {
                    WeatherLightRaySnapshot candidate = snapshots[index];
                    if (candidate.CurrentIntensity <= 0.0001f)
                    {
                        continue;
                    }

                    GenerateZoneBeamRecords(candidate, beamOffset);
                    beamOffset += Mathf.Max(
                        MinimumBeamCount,
                        candidate.Descriptor.BeamCount);
                }

                beamBuffer.SetData(beamRecords, 0, 0, totalBeamCount);
                endpointSignature = signature;
                endpointDataValid = true;
                endpointUploadCount++;
            }

            zoneBuffer.SetData(zoneRecords, 0, 0, bufferedZoneCount);
            zoneUploadCount++;
            LastVisibleZoneCount = drawIndex;
            LastBufferedZoneCount = bufferedZoneCount;
            LastTotalBeamCount = totalBeamCount;
            LastBeamBufferCapacity = beamCapacity;
            LastZoneBufferCapacity = zoneCapacity;
            LastEndpointUploadCount = endpointUploadCount;
            LastZoneUploadCount = zoneUploadCount;
            LastPresentationGroupCount = presentationGroupCount;
            return drawIndex;
        }

        private int ResolvePresentationGroup(
            ShaderParameters parameters,
            Vector4 softeningDirection)
        {
            bool hasScreenSpaceSurfaceResponse =
                parameters.SurfaceParameters0.x > 0.0001f;

            // Debug views intentionally retain one combined diagnostic group.
            if (debugView != WeatherLightRayRenderDebugView.FinalComposite)
            {
                if (presentationGroupCount == 0)
                {
                    presentationGroups[0] = new PresentationGroup
                    {
                        Parameters = parameters,
                        SofteningDirection = softeningDirection,
                        DrawCount = 0,
                        HasScreenSpaceSurfaceResponse =
                            hasScreenSpaceSurfaceResponse
                    };
                    presentationGroupCount = 1;
                }
                return 0;
            }

            if (!hasScreenSpaceSurfaceResponse)
            {
                for (int groupIndex = 0;
                    groupIndex < presentationGroupCount;
                    groupIndex++)
                {
                    PresentationGroup group =
                        presentationGroups[groupIndex];
                    if (!group.HasScreenSpaceSurfaceResponse &&
                        PresentationGroupMatches(
                            group,
                            parameters,
                            softeningDirection))
                    {
                        return groupIndex;
                    }
                }
            }

            int newGroupIndex = presentationGroupCount++;
            presentationGroups[newGroupIndex] = new PresentationGroup
            {
                Parameters = parameters,
                SofteningDirection = softeningDirection,
                DrawCount = 0,
                HasScreenSpaceSurfaceResponse =
                    hasScreenSpaceSurfaceResponse
            };
            return newGroupIndex;
        }

        private static bool PresentationGroupMatches(
            PresentationGroup group,
            ShaderParameters parameters,
            Vector4 softeningDirection)
        {
            const float tolerance = 0.0001f;
            return Approximately(
                    group.Parameters.Colour,
                    parameters.Colour,
                    tolerance) &&
                Mathf.Abs(
                    group.Parameters.SofteningParameters.x -
                    parameters.SofteningParameters.x) <= tolerance &&
                Mathf.Abs(
                    group.Parameters.SofteningParameters.y -
                    parameters.SofteningParameters.y) <= tolerance &&
                Approximately(
                    group.SofteningDirection,
                    softeningDirection,
                    tolerance);
        }

        private static bool Approximately(
            Vector4 a,
            Vector4 b,
            float tolerance)
        {
            return Mathf.Abs(a.x - b.x) <= tolerance &&
                Mathf.Abs(a.y - b.y) <= tolerance &&
                Mathf.Abs(a.z - b.z) <= tolerance &&
                Mathf.Abs(a.w - b.w) <= tolerance;
        }

        private bool IsPotentiallyVisible(
            WeatherLightRaySnapshot candidate)
        {
            Vector3 rayDirection = candidate.RayDirectionWorld.sqrMagnitude >
                0.000001f
                    ? candidate.RayDirectionWorld.normalized
                    : Vector3.down;
            Vector3 topCentre = candidate.BaseCentreWorld -
                rayDirection * candidate.Descriptor.Height;
            Vector3 centre = (candidate.BaseCentreWorld + topCentre) * 0.5f;
            float extent = candidate.Descriptor.Height * 0.5f +
                candidate.Descriptor.FootprintRadiusMetres;
            var bounds = new Bounds(
                centre,
                Vector3.one * Mathf.Max(0.1f, extent * 2f));
            return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
        }

        private void GenerateZoneBeamRecords(
            WeatherLightRaySnapshot zoneSnapshot,
            int beamOffset)
        {
            int beamCount = Mathf.Max(
                MinimumBeamCount,
                zoneSnapshot.Descriptor.BeamCount);
            GenerateEndpoint(
                zoneSnapshot,
                zoneSnapshot.EvolutionCurrentSeed,
                beamOffset,
                true);
            GenerateEndpoint(
                zoneSnapshot,
                zoneSnapshot.EvolutionNextSeed,
                beamOffset,
                false);
        }

        private void GenerateEndpoint(
            WeatherLightRaySnapshot zoneSnapshot,
            uint seed,
            int beamOffset,
            bool endpointA)
        {
            WeatherLightRayDescriptor descriptor = zoneSnapshot.Descriptor;
            int beamCount = Mathf.Max(MinimumBeamCount, descriptor.BeamCount);
            float evolutionStrength = Mathf.Clamp01(
                descriptor.EvolutionStrength);
            float compositionStrength = Mathf.Lerp(
                0.72f,
                1f,
                evolutionStrength);
            float minimumRatio = descriptor.BeamWidthRatioRange.x;
            float maximumRatio = descriptor.BeamWidthRatioRange.y;
            int dominantIndex = Mathf.Min(
                beamCount - 1,
                Mathf.FloorToInt(Hash01(seed, 0, 109u) * beamCount));
            int secondaryIndex = Mathf.Min(
                beamCount - 1,
                Mathf.FloorToInt(Hash01(seed, 0, 113u) * beamCount));
            if (beamCount > 1 && secondaryIndex == dominantIndex)
            {
                secondaryIndex = (secondaryIndex +
                    1 +
                    Mathf.FloorToInt(Hash01(seed, 0, 127u) *
                        (beamCount - 1))) % beamCount;
            }
            int faintIndex = Mathf.Min(
                beamCount - 1,
                Mathf.FloorToInt(Hash01(seed, 0, 129u) * beamCount));
            if (beamCount > 1 && faintIndex == dominantIndex)
            {
                faintIndex = (faintIndex + 1) % beamCount;
            }
            if (beamCount > 2 && faintIndex == secondaryIndex)
            {
                faintIndex = (faintIndex + 1) % beamCount;
                if (faintIndex == dominantIndex)
                {
                    faintIndex = (faintIndex + 1) % beamCount;
                }
            }

            float widthSum = 0f;
            float overlapSum = 0f;
            float intensitySum = 0f;

            for (int index = 0; index < beamCount; index++)
            {
                float widthRandom = Hash01(seed, index, 17u);
                float widthHierarchy = Mathf.Pow(widthRandom, 2.35f);
                float authoredWidth = Mathf.Lerp(
                    minimumRatio,
                    maximumRatio,
                    widthHierarchy);
                float hierarchyScale = Mathf.Lerp(
                    0.48f,
                    1.75f,
                    widthHierarchy);
                if (index == dominantIndex)
                {
                    hierarchyScale = Mathf.Max(
                        hierarchyScale * Mathf.Lerp(
                            1.35f,
                            1.85f,
                            Hash01(seed, index, 131u)),
                        Mathf.Lerp(
                            1.65f,
                            2.10f,
                            Hash01(seed, index, 133u)));
                }
                else if (index == secondaryIndex)
                {
                    hierarchyScale *= Mathf.Lerp(
                        1.10f,
                        1.40f,
                        Hash01(seed, index, 137u));
                }

                temporaryWidths[index] = Mathf.Lerp(
                    1f,
                    authoredWidth * hierarchyScale,
                    compositionStrength);
                widthSum += temporaryWidths[index];

                float intensityRandom = Hash01(seed, index, 31u);
                float intensityHierarchy = Mathf.Pow(
                    intensityRandom,
                    2.65f);
                float authoredVariation = Mathf.Lerp(
                    0.58f,
                    0.92f,
                    descriptor.BeamIntensityVariation / 0.75f);
                float minimumIntensity = Mathf.Lerp(
                    0.32f,
                    0.035f,
                    authoredVariation);
                float maximumIntensity = Mathf.Lerp(
                    1.20f,
                    1.55f,
                    authoredVariation);
                float intensity = Mathf.Lerp(
                    minimumIntensity,
                    maximumIntensity,
                    intensityHierarchy);
                if (index == dominantIndex)
                {
                    intensity = Mathf.Max(
                        intensity * Mathf.Lerp(
                            1.15f,
                            1.40f,
                            Hash01(seed, index, 139u)),
                        Mathf.Lerp(
                            1.25f,
                            1.60f,
                            Hash01(seed, index, 141u)));
                }
                else if (index == secondaryIndex)
                {
                    intensity *= Mathf.Lerp(
                        0.95f,
                        1.20f,
                        Hash01(seed, index, 149u));
                }
                if (index == faintIndex)
                {
                    intensity *= Mathf.Lerp(
                        0.12f,
                        0.32f,
                        Hash01(seed, index, 153u));
                }

                temporaryIntensities[index] = Mathf.Lerp(
                    1f,
                    intensity,
                    compositionStrength);
                intensitySum += temporaryIntensities[index];
            }

            for (int index = 0; index < beamCount - 1; index++)
            {
                float overlapRandom = Mathf.Pow(
                    Hash01(seed, index, 47u),
                    1.35f);
                float overlapRatio = Mathf.Lerp(
                    WeatherLightRayAreaLayout.MinimumAdjacentOverlapRatio,
                    WeatherLightRayAreaLayout.MaximumAdjacentOverlapRatio,
                    overlapRandom);
                temporaryOverlaps[index] = Mathf.Min(
                    temporaryWidths[index],
                    temporaryWidths[index + 1]) * overlapRatio;
                overlapSum += temporaryOverlaps[index];
            }

            float scale = descriptor.AreaDiameterMetres /
                Mathf.Max(0.0001f, widthSum - overlapSum);
            float cursor = -descriptor.AreaDiameterMetres * 0.5f;
            float endpointMeanIntensity = Mathf.Lerp(
                0.78f,
                1.08f,
                Hash01(seed, 0, 151u));
            float intensityScale =
                beamCount * endpointMeanIntensity /
                Mathf.Max(0.0001f, intensitySum);
            float baseFeather = Mathf.Lerp(
                0.035f,
                0.26f,
                descriptor.BeamEdgeSoftness);
            float minimumSoftness = Mathf.Max(
                0.045f,
                baseFeather * 0.42f);
            float profileVariation = Mathf.Lerp(
                0.42f,
                0.90f,
                descriptor.BeamSoftnessVariation / 0.75f) *
                compositionStrength;

            for (int index = 0; index < beamCount; index++)
            {
                float width = temporaryWidths[index] * scale;
                float centre = cursor + width * 0.5f;
                cursor += width;
                if (index < beamCount - 1)
                {
                    cursor -= temporaryOverlaps[index] * scale;
                }

                float phase = Hash01(seed, index, 59u) * Mathf.PI * 2f;
                float upper = Mathf.Lerp(
                    0.72f,
                    1.28f,
                    Hash01(seed, index, 67u));
                float ground = Mathf.Lerp(
                    0.72f,
                    1.28f,
                    Hash01(seed, index, 71u));
                float sideBias = Mathf.Lerp(
                    -1f,
                    1f,
                    Hash01(seed, index, 73u));
                float leftFactor = Mathf.Lerp(
                    1f - profileVariation,
                    1f + profileVariation,
                    Mathf.Clamp01(0.5f + sideBias * 0.5f));
                float rightFactor = Mathf.Lerp(
                    1f - profileVariation,
                    1f + profileVariation,
                    Mathf.Clamp01(0.5f - sideBias * 0.5f));
                leftFactor *= Mathf.Lerp(
                    0.82f,
                    1.18f,
                    Hash01(seed, index, 79u));
                rightFactor *= Mathf.Lerp(
                    0.82f,
                    1.18f,
                    Hash01(seed, index, 83u));
                float leftSoft = Mathf.Max(
                    minimumSoftness,
                    baseFeather * leftFactor);
                float rightSoft = Mathf.Max(
                    minimumSoftness,
                    baseFeather * rightFactor);
                float bias = Mathf.Lerp(
                    -0.32f,
                    0.32f,
                    Hash01(seed, index, 89u)) * compositionStrength;
                float transmissionFloor = Mathf.Lerp(
                    0.52f,
                    0.18f,
                    descriptor.BeamIntensityVariation / 0.75f);
                float leftTransmission = Mathf.Lerp(
                    transmissionFloor,
                    1f,
                    Mathf.Pow(Hash01(seed, index, 97u), 1.35f));
                float rightTransmission = Mathf.Lerp(
                    transmissionFloor,
                    1f,
                    Mathf.Pow(Hash01(seed, index, 101u), 1.35f));
                float contactScale = Mathf.Lerp(
                    0.82f,
                    1.18f,
                    Hash01(seed, index, 103u));
                Vector4 value0 = new Vector4(
                    centre,
                    width,
                    Mathf.Max(0.01f,
                        temporaryIntensities[index] * intensityScale),
                    phase);
                Vector4 value1 = new Vector4(
                    upper,
                    ground,
                    leftSoft,
                    rightSoft);
                Vector4 value2 = new Vector4(
                    bias,
                    leftTransmission,
                    rightTransmission,
                    contactScale);
                int target = beamOffset + index;
                BeamRecord record = beamRecords[target];
                if (endpointA)
                {
                    record.A0 = value0;
                    record.A1 = value1;
                    record.A2 = value2;
                }
                else
                {
                    record.B0 = value0;
                    record.B1 = value1;
                    record.B2 = value2;
                }
                beamRecords[target] = record;
            }
        }

        private static float Hash01(uint seed, int index, uint salt)
        {
            uint value = seed ^ ((uint)index * 0x9E3779B9u) ^ salt;
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215f;
        }

        private static int FloatBits(float value)
        {
            return BitConverter.SingleToInt32Bits(value);
        }

        private static ulong MixSignature(ulong hash, int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                return hash * 1099511628211UL;
            }
        }

        private void EnsureCpuCapacity(int beams, int zones)
        {
            if (beamRecords.Length < beams)
            {
                int capacity = NextPowerOfTwo(beams);
                Array.Resize(ref beamRecords, capacity);
                Array.Resize(ref temporaryWidths, capacity);
                Array.Resize(ref temporaryOverlaps, capacity);
                Array.Resize(ref temporaryIntensities, capacity);
            }
            if (zoneRecords.Length < zones)
            {
                int capacity = NextPowerOfTwo(zones);
                Array.Resize(ref zoneRecords, capacity);
                Array.Resize(ref zoneDraws, capacity);
                Array.Resize(ref presentationGroups, capacity);
            }
            else if (presentationGroups.Length < zones)
            {
                Array.Resize(
                    ref presentationGroups,
                    NextPowerOfTwo(zones));
            }
        }

        private void EnsureBufferCapacity(int beams, int zones)
        {
            if (beamBuffer == null || beamCapacity < beams)
            {
                beamBuffer?.Dispose();
                beamCapacity = NextPowerOfTwo(beams);
                beamBuffer = new GraphicsBuffer(
                    GraphicsBuffer.Target.Structured,
                    beamCapacity,
                    Marshal.SizeOf<BeamRecord>());
                endpointSignature = 0UL;
                endpointDataValid = false;
            }
            if (zoneBuffer == null || zoneCapacity < zones)
            {
                zoneBuffer?.Dispose();
                zoneCapacity = NextPowerOfTwo(zones);
                zoneBuffer = new GraphicsBuffer(
                    GraphicsBuffer.Target.Structured,
                    zoneCapacity,
                    Marshal.SizeOf<ZoneRecord>());
            }
        }

        private static int NextPowerOfTwo(int value)
        {
            int result = 1;
            while (result < value && result < (1 << 30))
            {
                result <<= 1;
            }
            return Mathf.Max(1, result);
        }

        private void ReleaseBuffers()
        {
            beamBuffer?.Dispose();
            zoneBuffer?.Dispose();
            beamBuffer = null;
            zoneBuffer = null;
            beamCapacity = 0;
            zoneCapacity = 0;
            endpointSignature = 0UL;
            endpointDataValid = false;
            endpointUploadCount = 0;
            zoneUploadCount = 0;
            LastVisibleZoneCount = 0;
            LastBufferedZoneCount = 0;
            LastTotalBeamCount = 0;
            LastBeamBufferCapacity = 0;
            LastZoneBufferCapacity = 0;
            LastEndpointUploadCount = 0;
            LastZoneUploadCount = 0;
            LastPresentationGroupCount = 0;
            presentationGroupCount = 0;
        }
    }
}
