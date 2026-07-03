using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private void BuildMetricBuffer()
        {
            using var profilerScope = InitBuildMetricBufferProfilerMarker.Auto();
            metricRows = new FoamMetricRow[fieldWidth];
            float longitudinalSpacing =
                StylizedRiverFoamTopologyFieldSpace.TexelSpacing(
                    fieldLength,
                    fieldWidth);
            float curvatureSampleDistance = Mathf.Max(
                0.5f,
                longitudinalSpacing * 2f);
            float flowSign = river.Domain.ReverseFlow ? -1f : 1f;

            for (int x = 0; x < fieldWidth; x++)
            {
                float localDistance =
                    StylizedRiverFoamTopologyFieldSpace.LocalDistanceAtTexel(
                        x,
                        fieldWidth,
                        fieldLength);
                float clampedLocalDistance = Mathf.Min(
                    localDistance,
                    validFieldLength);
                float globalDistance =
                    river.Domain.GlobalDistanceMinimum + clampedLocalDistance;
                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float left = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
                float right = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
                float minimumLateralSpacing =
                    StylizedRiverFoamTopologyFieldSpace.TexelSpacing(
                        left + right,
                        fieldHeight);

                float previousGlobal = Mathf.Max(
                    river.Domain.GlobalDistanceMinimum,
                    globalDistance - curvatureSampleDistance);
                float nextGlobal = Mathf.Min(
                    river.Domain.GlobalDistanceMaximum,
                    globalDistance + curvatureSampleDistance);
                StylizedRiverSplineSample previousSample =
                    river.Domain.SampleAtGlobalDistance(previousGlobal);
                StylizedRiverSplineSample nextSample =
                    river.Domain.SampleAtGlobalDistance(nextGlobal);
                Vector3 previousTangent =
                    previousSample.Tangent * flowSign;
                Vector3 nextTangent =
                    nextSample.Tangent * flowSign;
                // Topology lateral coordinates retain the domain's stored
                // Side basis under reverse flow, so curvature sign must be
                // expressed in that same basis rather than a flipped flow-side.
                Vector3 topologySide = sample.Side;
                float curvatureDistance = Mathf.Max(
                    0.01f,
                    nextGlobal - previousGlobal);
                float signedCurvature = Vector3.Dot(
                    (nextTangent - previousTangent) / curvatureDistance,
                    topologySide);
                float previousWidth =
                    previousSample.LeftSurfaceHalfWidth +
                    previousSample.RightSurfaceHalfWidth;
                float nextWidth =
                    nextSample.LeftSurfaceHalfWidth +
                    nextSample.RightSurfaceHalfWidth;
                float widthDerivative =
                    (nextWidth - previousWidth) / curvatureDistance;
                float asymmetry =
                    (right - left) / Mathf.Max(0.05f, left + right);

                metricRows[x] = new FoamMetricRow
                {
                    WidthsAndSpacing = new Vector4(
                        left,
                        right,
                        longitudinalSpacing,
                        minimumLateralSpacing),
                    TopologyData = new Vector4(
                        signedCurvature,
                        widthDerivative,
                        asymmetry,
                        localDistance <= validFieldLength + 0.0001f
                            ? 1f
                            : 0f),
                    ShoreData = new Vector4(
                        Mathf.Max(0.01f, sample.LeftHalfWidth),
                        Mathf.Max(0.01f, sample.RightHalfWidth),
                        sample.SurfaceHeight,
                        0f)
                };
            }

            metricBuffer?.Release();
            metricBuffer = new ComputeBuffer(
                fieldWidth,
                Marshal.SizeOf<FoamMetricRow>(),
                ComputeBufferType.Structured);
            metricBuffer.SetData(metricRows);
        }

        private void BuildMajorTopology()
        {
            using var profilerScope = TopologyBuildMajorProfilerMarker.Auto();
            if (river == null || !river.Domain.IsValid ||
                topologyGeneratedTexture == null ||
                fieldWidth < 2 || fieldHeight < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                majorTopology = null;
                connectorTopology = null;
                pocketTopology = null;
                majorTopologyInputSignature = int.MinValue;
                connectorTopologyInputSignature = int.MinValue;
                pocketTopologyInputSignature = int.MinValue;
                ReleaseMajorEvolutionResources();
                ClearRenderTexture(topologyGeneratedTexture);
                ClearRenderTexture(evolvingMajorTexture);
                ClearRenderTexture(evolvingHostedNegativeTexture);
                ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                ClearRenderTexture(evolvingConnectorTexture);
                ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                return;
            }

            int cellCount = fieldWidth * fieldHeight;
            if (obstacleExclusionScalar.Length != cellCount)
            {
                obstacleExclusionScalar = new float[cellCount];
            }

            majorTopology =
                StylizedRiverFoamMajorTopologyGenerator.Generate(
                    river.Domain,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    validFieldLength,
                    river.Quality,
                    river.ShoreMotion,
                    river.FoamMajorSupportAmount,
                    river.FoamMajorSupportSize,
                    river.FoamMajorSupportSizeVariation,
                    river.FoamMajorRecycleTerritoryDeviationPercent,
                    river.FoamMajorSupportSeed,
                    obstacleExclusionScalar);
            connectorTopology = null;
            pocketTopology = null;
            connectorTopologyInputSignature = int.MinValue;
            pocketTopologyInputSignature = int.MinValue;
            InitializeMajorEvolution();
            UploadGeneratedTopology();
            majorTopologyInputSignature = ResolveMajorTopologyInputSignature();
        }

        private void BuildConnectorTopology()
        {
            using var profilerScope =
                TopologyBuildConnectorProfilerMarker.Auto();
            if (river == null || !river.Domain.IsValid ||
                topologyGeneratedTexture == null ||
                majorTopology == null ||
                fieldWidth < 2 || fieldHeight < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                ReleaseConnectorIdentityReconstructionResources();
                connectorTopology = null;
                pocketTopology = null;
                connectorTopologyInputSignature = int.MinValue;
                pocketTopologyInputSignature = int.MinValue;
                InitializeHostedNegativeEvolution(false);
                InitializeFreeWaterEvolution(false);
                UploadGeneratedTopology();
                BuildEvolvingMajorField();
                return;
            }

            ReleaseConnectorIdentityReconstructionResources();
            connectorTopology =
                StylizedRiverFoamConnectorTopologyGenerator.Generate(
                    river.Domain,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    validFieldLength,
                    river.Quality,
                    river.ShoreMotion,
                    river.FoamMajorSupportSeed,
                    river.FoamConnectorAmount,
                    river.FoamConnectorDirectness,
                    river.FoamConnectorLengthPreference,
                    obstacleExclusionScalar,
                    majorTopology);
            connectorTopologyInputSignature =
                ResolveConnectorTopologyInputSignature();
            pocketTopology = null;
            pocketTopologyInputSignature = int.MinValue;
            InitializeHostedNegativeEvolution(false);
            InitializeFreeWaterEvolution(false);
            UploadGeneratedTopology();
            BuildEvolvingMajorField();
        }

        private void BuildPocketTopology()
        {
            using var profilerScope = TopologyBuildPocketProfilerMarker.Auto();
            if (
                topologyGeneratedTexture == null ||
                majorTopology == null ||
                connectorTopology == null ||
                river == null || !river.Domain.IsValid)
            {
                ReleaseConnectorIdentityReconstructionResources();
                pocketTopology = null;
                pocketTopologyInputSignature = int.MinValue;
                InitializeHostedNegativeEvolution(false);
                InitializeFreeWaterEvolution(false);
                UploadGeneratedTopology();
                BuildEvolvingMajorField();
                return;
            }

            pocketTopology =
                StylizedRiverFoamPocketTopologyGenerator.Generate(
                    river.Domain,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    validFieldLength,
                    river.Quality,
                    river.ShoreMotion,
                    river.FoamMajorSupportSeed,
                    river.FoamInteriorPocketAmount,
                    river.FoamEdgeCavityAmount,
                    river.FoamConnectorWeakSpanAmount,
                    river.FoamFreeWaterEventAmount,
                    obstacleExclusionScalar,
                    majorTopology,
                    connectorTopology);
            pocketTopologyInputSignature =
                ResolvePocketTopologyInputSignature();
            InitializeHostedNegativeEvolution(false);
            InitializeFreeWaterEvolution(false);
            InitializeConnectorIdentityReconstruction(false);
            UploadGeneratedTopology();
            BuildEvolvingMajorField();
        }

        private void UploadGeneratedTopology()
        {
            if (topologyGeneratedTexture == null ||
                fieldWidth < 1 || fieldHeight < 1)
            {
                return;
            }

            int cellCount = fieldWidth * fieldHeight;
            if (topologyGeneratedUploadPixels.Length != cellCount)
            {
                topologyGeneratedUploadPixels = new Color[cellCount];
            }
            else
            {
                Array.Clear(
                    topologyGeneratedUploadPixels,
                    0,
                    topologyGeneratedUploadPixels.Length);
            }

            majorTopology?.FillUploadPixels(topologyGeneratedUploadPixels);
            connectorTopology?.AddToUploadPixels(
                topologyGeneratedUploadPixels,
                connectorIdentityReconstructionReady);
            pocketTopology?.AddToUploadPixels(
                topologyGeneratedUploadPixels,
                hostedNegativeEvolutionReady,
                freeWaterEvolutionReady,
                weakSpanIdentityReconstructionReady);

            if (topologyGeneratedUploadTexture == null ||
                topologyGeneratedUploadTexture.width != fieldWidth ||
                topologyGeneratedUploadTexture.height != fieldHeight)
            {
                if (topologyGeneratedUploadTexture != null)
                {
                    DestroyUnityObject(topologyGeneratedUploadTexture);
                }

                topologyGeneratedUploadTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RGBAHalf,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamGeneratedTopology_Upload",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            topologyGeneratedUploadTexture.SetPixels(
                topologyGeneratedUploadPixels);
            topologyGeneratedUploadTexture.Apply(false, false);
            Graphics.Blit(
                topologyGeneratedUploadTexture,
                topologyGeneratedTexture);
        }

        private int ResolveMajorTopologyInputSignature()
        {
            if (river == null || !river.Domain.IsValid ||
                fieldWidth < 2 || fieldHeight < 2)
            {
                return int.MinValue + 1;
            }

            return ResolveMajorTopologyInputSignature(
                river.Domain,
                obstacleGeometryVersion,
                river.Quality,
                fieldWidth,
                fieldHeight,
                river.FoamMajorSupportSeed,
                river.FoamMajorSupportAmount,
                river.FoamMajorSupportSize,
                river.FoamMajorSupportSizeVariation,
                river.FoamMajorRecycleTerritoryDeviationPercent,
                river.ShoreMotion);
        }

        private int ResolveConnectorTopologyInputSignature()
        {
            if (river == null || !river.Domain.IsValid ||
                fieldWidth < 2 || fieldHeight < 2 ||
                majorTopology == null)
            {
                return int.MinValue + 2;
            }

            return ResolveConnectorTopologyInputSignature(
                majorTopologyInputSignature,
                river.FoamConnectorAmount,
                river.FoamConnectorDirectness,
                river.FoamConnectorLengthPreference);
        }

        private int ResolvePocketTopologyInputSignature()
        {
            if (river == null || !river.Domain.IsValid ||
                fieldWidth < 2 || fieldHeight < 2 ||
                majorTopology == null || connectorTopology == null)
            {
                return int.MinValue + 3;
            }

            return ResolvePocketTopologyInputSignature(
                majorTopologyInputSignature,
                connectorTopologyInputSignature,
                river.FoamInteriorPocketAmount,
                river.FoamEdgeCavityAmount,
                river.FoamConnectorWeakSpanAmount,
                river.FoamFreeWaterEventAmount);
        }

        private void ResolveRequestedTopologySignatures(
            out int majorSignature,
            out int connectorSignature,
            out int pocketSignature,
            out int combinedSignature)
        {
            majorSignature = ResolveMajorTopologyInputSignature(
                river.Domain,
                obstacleGeometryVersion,
                river.Quality,
                fieldWidth,
                fieldHeight,
                river.FoamMajorSupportSeed,
                river.FoamMajorSupportAmount,
                river.FoamMajorSupportSize,
                river.FoamMajorSupportSizeVariation,
                river.FoamMajorRecycleTerritoryDeviationPercent,
                river.ShoreMotion);
            connectorSignature = ResolveConnectorTopologyInputSignature(
                majorSignature,
                river.FoamConnectorAmount,
                river.FoamConnectorDirectness,
                river.FoamConnectorLengthPreference);
            pocketSignature = ResolvePocketTopologyInputSignature(
                majorSignature,
                connectorSignature,
                river.FoamInteriorPocketAmount,
                river.FoamEdgeCavityAmount,
                river.FoamConnectorWeakSpanAmount,
                river.FoamFreeWaterEventAmount);
            combinedSignature = CombineTopologySignatures(
                majorSignature,
                connectorSignature,
                pocketSignature);
        }

        private int ResolveRequestedTopologySignature()
        {
            ResolveRequestedTopologySignatures(
                out _,
                out _,
                out _,
                out int combinedSignature);
            return combinedSignature;
        }

        private int ResolveActiveTopologySignature()
        {
            if (majorTopology == null || connectorTopology == null ||
                pocketTopology == null)
            {
                return int.MinValue + 4;
            }

            return CombineTopologySignatures(
                majorTopologyInputSignature,
                connectorTopologyInputSignature,
                pocketTopologyInputSignature);
        }

        private static int ResolveMajorTopologyInputSignature(
            RiverDomainSnapshot domain,
            int obstacleVersion,
            StylizedRiverQuality quality,
            int width,
            int height,
            int majorSeed,
            float majorAmount,
            float majorSize,
            float majorSizeVariation,
            float recycleTerritoryDeviationPercent,
            float shoreMotion)
        {
            if (domain == null || !domain.IsValid || width < 2 || height < 2)
            {
                return int.MinValue + 1;
            }

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + domain.Version;
                hash = hash * 31 + obstacleVersion;
                hash = hash * 31 + (int)quality;
                hash = hash * 31 + width;
                hash = hash * 31 + height;
                hash = hash * 31 + majorSeed;
                hash = hash * 31 + Mathf.RoundToInt(majorAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(majorSize * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    majorSizeVariation * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    recycleTerritoryDeviationPercent * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(shoreMotion * 10000f);
                return hash;
            }
        }

        private static int ResolveConnectorTopologyInputSignature(
            int majorSignature,
            float connectorAmount,
            float connectorDirectness,
            float connectorLengthPreference)
        {
            unchecked
            {
                int hash = 23;
                hash = hash * 31 + majorSignature;
                hash = hash * 31 + Mathf.RoundToInt(
                    connectorAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    connectorDirectness * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    connectorLengthPreference * 10000f);
                return hash;
            }
        }

        private static int ResolvePocketTopologyInputSignature(
            int majorSignature,
            int connectorSignature,
            float interiorPocketAmount,
            float edgeCavityAmount,
            float weakSpanAmount,
            float freeWaterAmount)
        {
            unchecked
            {
                int hash = 29;
                hash = hash * 31 + majorSignature;
                hash = hash * 31 + connectorSignature;
                hash = hash * 31 + Mathf.RoundToInt(
                    interiorPocketAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    edgeCavityAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    weakSpanAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    freeWaterAmount * 10000f);
                hash = hash * 31 + 4;
                return hash;
            }
        }

        private static int CombineTopologySignatures(
            int majorSignature,
            int connectorSignature,
            int pocketSignature)
        {
            unchecked
            {
                int hash = 43;
                hash = hash * 31 + majorSignature;
                hash = hash * 31 + connectorSignature;
                hash = hash * 31 + pocketSignature;
                return hash;
            }
        }

        private void ConfigureTopologyParameters(float deltaTime)
        {
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamTopologyDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                allocatedGlobalStart);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamDeltaTime", deltaTime);
            computeShader.SetFloat(
                "_FoamFlowSpeed",
                river.FlowSpeedMetresPerSecond * river.LiquidFactor);
            computeShader.SetFloat(
                "_FoamFlowDirection",
                river.FlowDirection);
            computeShader.SetFloat(
                "_FoamTime",
                ResolveInitializationMotionTime());
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamMotionFlowSpeed",
                river.FlowSpeedMetresPerSecond);
            computeShader.SetFloat(
                "_FoamMotionWaveHeight",
                river.MotionWaveHeight);
            computeShader.SetFloat(
                "_FoamMotionWaveLength",
                river.MotionWaveLength);
            computeShader.SetFloat(
                "_FoamMotionWaveSteepness",
                river.MotionWaveSteepness);
            computeShader.SetFloat(
                "_FoamMotionTurbulence",
                river.MotionTurbulence);
            computeShader.SetFloat(
                "_FoamShoreMotion",
                river.ShoreMotion);
            computeShader.SetFloat(
                "_FoamShoreMotionWidth",
                river.ShoreMotionWidth);
            computeShader.SetFloat(
                "_FoamShoreWaveHeightScale",
                river.ShoreWaveHeightScale);
            computeShader.SetFloat(
                "_FoamShoreWaveLengthScale",
                river.ShoreWaveLengthScale);
            computeShader.SetFloat(
                "_FoamShoreWaveReach",
                river.ShoreWaveReach);
            computeShader.SetFloat(
                "_FoamShoreWaveTransitionLength",
                river.ShoreWaveTransitionLength);
            computeShader.SetFloat(
                "_FoamShoreWaveSizeVariation",
                river.ShoreWaveSizeVariation);
            computeShader.SetFloat(
                "_FoamShoreWaveSideAsymmetry",
                river.ShoreWaveSideAsymmetry);
            computeShader.SetFloat(
                "_FoamShoreWaveProfileVariation",
                river.ShoreWaveProfileVariation);
            computeShader.SetFloat(
                "_FoamShoreBankCover",
                river.ShorelineBankCover);
            computeShader.SetFloat(
                "_FoamFreezeAmount",
                river.FreezeAmount);
            computeShader.SetFloat(
                "_FoamShoreCaptureCoreWidth",
                ShoreSupportCoreWidthMetres);
            computeShader.SetFloat(
                "_FoamShoreCaptureFadeWidth",
                ShoreSupportFadeWidthMetres);
        }

        private void RefreshDynamicTopologySources(
            bool measureMetrics,
            bool refreshLiveInputs = true)
        {
            using var profilerScope = TopologyRefreshSourcesProfilerMarker.Auto();
            if (computeShader == null || topologyTexture == null ||
                topologySourcesTexture == null ||
                topologyGeneratedTexture == null ||
                evolvingMajorTexture == null ||
                evolvingHostedNegativeTexture == null ||
                evolvingFreeWaterNegativeTexture == null ||
                evolvingConnectorTexture == null ||
                evolvingWeakSpanNegativeTexture == null ||
                currentShoreEdgesTexture == null ||
                obstacleExclusionTexture == null ||
                boundaryTexture == null || metricBuffer == null ||
                buildCurrentShoreEdgesKernel < 0 ||
                composeTopologyKernel < 0)
            {
                return;
            }

            ConfigureTopologyParameters(0f);

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            bool disturbanceAvailable =
                disturbanceRuntime != null &&
                disturbanceRuntime.IsAllocated;
            RenderTexture staticWakeSource = disturbanceAvailable
                ? disturbanceRuntime.StaticWakeSourceTexture
                : null;
            RenderTexture staticPressureSource = disturbanceAvailable
                ? disturbanceRuntime.StaticPressureTexture
                : null;
            bool staticWakeAvailable = IsCreatedTexture(staticWakeSource);
            bool staticPressureAvailable = IsCreatedTexture(staticPressureSource);
            Texture staticWakeTexture = staticWakeAvailable
                ? staticWakeSource
                : neutralDisturbanceTexture;
            Texture staticPressureTexture = staticPressureAvailable
                ? staticPressureSource
                : neutralDisturbanceTexture;
            Vector2Int staticWakeDimensions = staticWakeAvailable
                ? new Vector2Int(staticWakeSource.width, staticWakeSource.height)
                : Vector2Int.one;
            Vector2Int staticPressureDimensions = staticPressureAvailable
                ? new Vector2Int(
                    staticPressureSource.width,
                    staticPressureSource.height)
                : Vector2Int.one;

            computeShader.SetInts(
                "_FoamStaticWakeDimensions",
                staticWakeDimensions.x,
                staticWakeDimensions.y);
            computeShader.SetInts(
                "_FoamStaticPressureDimensions",
                staticPressureDimensions.x,
                staticPressureDimensions.y);
            computeShader.SetFloat(
                "_FoamDisturbanceEnabled",
                staticWakeAvailable || staticPressureAvailable ? 1f : 0f);

            if (refreshLiveInputs)
            {
                computeShader.SetBuffer(
                    buildCurrentShoreEdgesKernel,
                    "_FoamMetricRows",
                    metricBuffer);
                computeShader.SetTexture(
                    buildCurrentShoreEdgesKernel,
                    "_FoamCurrentShoreEdgesWrite",
                    currentShoreEdgesTexture);
                DispatchOneDimensional(
                    buildCurrentShoreEdgesKernel,
                    guidanceWidth,
                    64);

                UpdateObstacleExclusionMask();
            }

            using (TopologyComposeProfilerMarker.Auto())
            {
                computeShader.SetBuffer(
                    composeTopologyKernel,
                    "_FoamMetricRows",
                    metricBuffer);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamBoundary",
                    boundaryTexture);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamObstacleExclusionRead",
                    obstacleExclusionTexture);
                BindGeneratedTopologyInputs(composeTopologyKernel);
                ConfigureTopologyTransitionInputs(composeTopologyKernel);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamStaticWakeField",
                    staticWakeTexture);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamStaticPressureField",
                    staticPressureTexture);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamCurrentShoreEdgesRead",
                    currentShoreEdgesTexture);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamTopologyWrite",
                    topologyTexture);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamTopologySourcesWrite",
                    topologySourcesTexture);
                Dispatch(
                    composeTopologyKernel,
                    guidanceWidth,
                    guidanceHeight);
            }

            if (measureMetrics)
            {
                MeasureTopologyMetrics();
            }
        }

        private void MeasureTopologyMetrics()
        {
            using var profilerScope = DiagnosticsMeasureTopologyProfilerMarker.Auto();
            if (computeShader == null || currentState == null ||
                topologyTexture == null || topologySourcesTexture == null ||
                boundaryTexture == null || obstacleExclusionTexture == null ||
                topologyMetricsBuffer == null ||
                resetTopologyMetricsKernel < 0 ||
                measureTopologyMetricsKernel < 0)
            {
                return;
            }

            computeShader.SetBuffer(
                resetTopologyMetricsKernel,
                "_FoamTopologyMetrics",
                topologyMetricsBuffer);
            DispatchOneDimensional(
                resetTopologyMetricsKernel,
                TopologyMetricCount,
                64);

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamTopologyDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamPresenceMetricThreshold",
                PresenceMetricThreshold);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamTopologyRead",
                topologyTexture);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamTopologySourcesRead",
                topologySourcesTexture);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetBuffer(
                measureTopologyMetricsKernel,
                "_FoamTopologyMetrics",
                topologyMetricsBuffer);
            Dispatch(
                measureTopologyMetricsKernel,
                guidanceWidth,
                guidanceHeight);
            RequestTopologyMetricsReadback();
        }

        private void RequestTopologyMetricsReadback()
        {
            if (!SystemInfo.supportsAsyncGPUReadback ||
                topologyMetricsReadbackPending ||
                topologyMetricsBuffer == null)
            {
                return;
            }

            topologyMetricsReadbackPending = true;
            int generation = topologyMetricsGeneration;
            ComputeBuffer requestedBuffer = topologyMetricsBuffer;
            AsyncGPUReadback.Request(
                requestedBuffer,
                request =>
                {
                    if (this == null || generation != topologyMetricsGeneration)
                    {
                        requestedBuffer?.Release();
                        return;
                    }

                    topologyMetricsReadbackPending = false;
                    if (request.hasError)
                    {
                        topologyMetricsAvailable = false;
                        return;
                    }

                    var data = request.GetData<uint>();
                    int count = Mathf.Min(
                        data.Length,
                        latestTopologyMetrics.Length);
                    for (int index = 0; index < count; index++)
                    {
                        latestTopologyMetrics[index] = data[index];
                    }

                    topologyMetricsAvailable = count == TopologyMetricCount;
                });
        }

        private float TopologyCoverageRatio(int numeratorIndex)
        {
            return TopologyRegionRatio(numeratorIndex, 0);
        }

        private float TopologyRegionRatio(
            int numeratorIndex,
            int denominatorIndex)
        {
            if (!topologyMetricsAvailable ||
                numeratorIndex < 0 || numeratorIndex >= TopologyMetricCount ||
                denominatorIndex < 0 || denominatorIndex >= TopologyMetricCount)
            {
                return 0f;
            }

            uint denominator = latestTopologyMetrics[denominatorIndex];
            return denominator > 0u
                ? latestTopologyMetrics[numeratorIndex] / (float)denominator
                : 0f;
        }
    }
}
