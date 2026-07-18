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
        private void QueueMajorTopologyRebuildIfNeeded()
        {
            if (river == null || !river.Domain.IsValid ||
                initializationPhase != InitializationPhase.Ready ||
                resourcesDirty || activeTopologyRequiresExplicitPreparation ||
                fieldWidth < 2 || fieldHeight < 2)
            {
                return;
            }

            int requestedSignature = ResolveRequestedTopologySignature();
            int activeSignature = ResolveActiveTopologySignature();
            if (requestedSignature == activeSignature)
            {
                return;
            }

            if (!topologyStartupValidationComplete)
            {
                topologyStartupDirtyReasons |=
                    TopologyStartupDirtyReason.TopologySettingsChanged;
            }
            activeTopologyRequiresExplicitPreparation = true;
            if (!activeTopologyObstacleStale)
            {
                MarkActiveTopologyCacheStale(
                    "Stale — Settings Changed",
                    "Topology-generation settings changed after activation. " +
                    "The active prepared topology remains visible; Play Mode " +
                    "will not build a replacement or save an asset. Prepare " +
                    "the cache explicitly in Edit Mode.");
            }

            CancelTopologyReplacementBuild(true);
        }

        private void RequestTopologyReplacement(
            TopologyReplacementReason reason,
            bool identicalValidation)
        {
            if (!topologyStartupValidationComplete)
            {
                topologyStartupReplacementAttemptCount++;
            }
            if (river == null || !river.Domain.IsValid ||
                initializationPhase != InitializationPhase.Ready ||
                resourcesDirty || domainVersion != river.Domain.Version ||
                allocatedQuality != river.Quality ||
                fieldWidth < 2 || fieldHeight < 2)
            {
                return;
            }

            if (Application.isPlaying)
            {
                activeTopologyRequiresExplicitPreparation = true;
                MarkActiveTopologyCacheStale(
                    "Stale — Explicit Preparation Required",
                    "A topology replacement was requested during Play Mode. " +
                    "P1 blocks runtime topology generation; prepare the cache " +
                    "explicitly in Edit Mode.");
                CancelTopologyReplacementBuild(true);
                return;
            }

            ResolveRequestedTopologySignatures(
                out int majorSignature,
                out int connectorSignature,
                out int pocketSignature,
                out int targetSignature);
            int activeSignature = ResolveActiveTopologySignature();
            if (!identicalValidation && targetSignature == activeSignature)
            {
                return;
            }

            if (topologyReplacementBuild != null)
            {
                bool sameRequest =
                    topologyReplacementBuild.TargetSignature ==
                        targetSignature &&
                    topologyReplacementBuild.IsIdenticalValidation ==
                        identicalValidation;
                if (sameRequest)
                {
                    return;
                }

                CancelTopologyReplacementBuild(true);
            }

            float[] obstacleSnapshot = obstacleExclusionScalar.Length ==
                fieldWidth * fieldHeight
                    ? (float[])obstacleExclusionScalar.Clone()
                    : new float[fieldWidth * fieldHeight];

            topologyReplacementBuild = new TopologyReplacementBuild
            {
                RequestId = ++topologyReplacementRequestSequence,
                TargetSignature = targetSignature,
                MajorInputSignature = majorSignature,
                ConnectorInputSignature = connectorSignature,
                PocketInputSignature = pocketSignature,
                Reason = reason,
                IsIdenticalValidation = identicalValidation,
                Domain = river.Domain,
                Quality = river.Quality,
                FieldWidth = fieldWidth,
                FieldHeight = fieldHeight,
                FieldLength = fieldLength,
                ValidFieldLength = validFieldLength,
                ShoreMotion = river.ShoreMotion,
                MajorAmount = river.FoamMajorSupportAmount,
                MajorSize = river.FoamMajorSupportSize,
                MajorSizeVariation = river.FoamMajorSupportSizeVariation,
                MajorRecycleTerritoryDeviationPercent =
                    river.FoamMajorRecycleTerritoryDeviationPercent,
                MajorSeed = river.FoamMajorSupportSeed,
                ConnectorAmount = river.FoamConnectorAmount,
                ConnectorDirectness = river.FoamConnectorDirectness,
                ConnectorLengthPreference =
                    river.FoamConnectorLengthPreference,
                InteriorPocketAmount = river.FoamInteriorPocketAmount,
                EdgeCavityAmount = river.FoamEdgeCavityAmount,
                ConnectorWeakSpanAmount =
                    river.FoamConnectorWeakSpanAmount,
                FreeWaterEventAmount = river.FoamFreeWaterEventAmount,
                ObstacleExclusion = obstacleSnapshot
            };
            topologyReplacementPhase =
                TopologyReplacementPhase.BuildMajorTopology;
            topologyReplacementRequestCount++;
            topologyReplacementLastReason =
                FormatTopologyReplacementReason(reason);
        }

        private bool AdvanceTopologyReplacementBuild()
        {
            TopologyReplacementBuild build = topologyReplacementBuild;
            if (build == null ||
                topologyReplacementPhase == TopologyReplacementPhase.Idle)
            {
                return false;
            }

            if (river == null || !river.Domain.IsValid || resourcesDirty ||
                domainVersion != river.Domain.Version ||
                allocatedQuality != river.Quality ||
                build.FieldWidth != fieldWidth ||
                build.FieldHeight != fieldHeight)
            {
                CancelTopologyReplacementBuild(false);
                return false;
            }

            int currentTargetSignature = ResolveRequestedTopologySignature();
            if (build.IsIdenticalValidation)
            {
                if (currentTargetSignature != ResolveActiveTopologySignature())
                {
                    CancelTopologyReplacementBuild(true);
                    RequestTopologyReplacement(
                        TopologyReplacementReason.Settings,
                        false);
                    return false;
                }
            }
            else if (currentTargetSignature != build.TargetSignature)
            {
                CancelTopologyReplacementBuild(true);
                RequestTopologyReplacement(
                    TopologyReplacementReason.Settings,
                    false);
                return false;
            }

            switch (topologyReplacementPhase)
            {
                case TopologyReplacementPhase.BuildMajorTopology:
                    using (ReplacementBuildMajorProfilerMarker.Auto())
                    {
                        build.MajorTopology =
                            StylizedRiverFoamMajorTopologyGenerator.Generate(
                                build.Domain,
                                build.FieldWidth,
                                build.FieldHeight,
                                build.FieldLength,
                                build.ValidFieldLength,
                                build.Quality,
                                build.ShoreMotion,
                                build.MajorAmount,
                                build.MajorSize,
                                build.MajorSizeVariation,
                                build.MajorRecycleTerritoryDeviationPercent,
                                build.MajorSeed,
                                build.ObstacleExclusion);
                    }
                    topologyReplacementPhase =
                        TopologyReplacementPhase.BuildConnectorTopology;
                    break;

                case TopologyReplacementPhase.BuildConnectorTopology:
                    using (ReplacementBuildConnectorProfilerMarker.Auto())
                    {
                        build.ConnectorTopology =
                            StylizedRiverFoamConnectorTopologyGenerator.Generate(
                                build.Domain,
                                build.FieldWidth,
                                build.FieldHeight,
                                build.FieldLength,
                                build.ValidFieldLength,
                                build.Quality,
                                build.ShoreMotion,
                                build.MajorSeed,
                                build.ConnectorAmount,
                                build.ConnectorDirectness,
                                build.ConnectorLengthPreference,
                                build.ObstacleExclusion,
                                build.MajorTopology);
                    }
                    topologyReplacementPhase =
                        TopologyReplacementPhase.BuildPocketTopology;
                    break;

                case TopologyReplacementPhase.BuildPocketTopology:
                    using (ReplacementBuildPocketProfilerMarker.Auto())
                    {
                        build.PocketTopology =
                            StylizedRiverFoamPocketTopologyGenerator.Generate(
                                build.Domain,
                                build.FieldWidth,
                                build.FieldHeight,
                                build.FieldLength,
                                build.ValidFieldLength,
                                build.Quality,
                                build.ShoreMotion,
                                build.MajorSeed,
                                build.InteriorPocketAmount,
                                build.EdgeCavityAmount,
                                build.ConnectorWeakSpanAmount,
                                build.FreeWaterEventAmount,
                                build.ObstacleExclusion,
                                build.MajorTopology,
                                build.ConnectorTopology);
                    }
                    topologyReplacementPhase =
                        TopologyReplacementPhase.PrepareGeneratedTexture;
                    break;

                case TopologyReplacementPhase.PrepareGeneratedTexture:
                    using (ReplacementPrepareTextureProfilerMarker.Auto())
                    {
                        PrepareTopologyReplacementGeneratedTexture(build);
                    }
                    topologyReplacementPhase =
                        TopologyReplacementPhase.ReadyToActivate;
                    break;

                case TopologyReplacementPhase.ReadyToActivate:
                    if (build.IsIdenticalValidation)
                    {
                        topologyReplacementIdenticalPreparedCount++;
                        topologyReplacementLastReason =
                            FormatTopologyReplacementReason(build.Reason);
                        ReleaseTopologyReplacementBuild(true);
                        return false;
                    }
                    using (ReplacementActivateProfilerMarker.Auto())
                    {
                        return ActivateTopologyReplacement(build);
                    }
            }

            return false;
        }

        private void PrepareTopologyReplacementGeneratedTexture(
            TopologyReplacementBuild build)
        {
            int cellCount = build.FieldWidth * build.FieldHeight;
            Color[] pixels = new Color[cellCount];
            build.MajorTopology?.FillUploadPixels(pixels);
            build.ConnectorTopology?.AddToUploadPixels(pixels, false);
            build.PocketTopology?.AddToUploadPixels(
                pixels,
                false,
                false,
                false);

            build.GeneratedTexture = CreateStructuralTexture(
                $"PS3D_RiverFoam_TopologyGenerated_Replacement_{build.RequestId}");
            Texture2D upload = new Texture2D(
                build.FieldWidth,
                build.FieldHeight,
                TextureFormat.RGBAHalf,
                false,
                true)
            {
                name =
                    $"T_PS3D_RiverFoamTopologyReplacement_{build.RequestId}",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            upload.SetPixels(pixels);
            upload.Apply(false, false);
            Graphics.Blit(upload, build.GeneratedTexture);
            DestroyUnityObject(upload);
        }

        private bool ActivateTopologyReplacement(
            TopologyReplacementBuild build)
        {
            if (build == null || build.MajorTopology == null ||
                build.ConnectorTopology == null ||
                build.PocketTopology == null ||
                build.GeneratedTexture == null ||
                !build.GeneratedTexture.IsCreated())
            {
                CancelTopologyReplacementBuild(false);
                return false;
            }

            CaptureActiveGeneratedTopologyTransition(false);

            RenderTexture retiredGeneratedTexture = topologyGeneratedTexture;
            topologyGeneratedTexture = build.GeneratedTexture;
            build.GeneratedTexture = null;
            majorTopology = build.MajorTopology;
            connectorTopology = build.ConnectorTopology;
            pocketTopology = build.PocketTopology;
            majorTopologyInputSignature = build.MajorInputSignature;
            connectorTopologyInputSignature = build.ConnectorInputSignature;
            pocketTopologyInputSignature = build.PocketInputSignature;

            InitializeMajorEvolution();
            InitializeConnectorIdentityReconstruction(false);
            UploadGeneratedTopology();
            BuildEvolvingMajorField();
            RefreshDynamicTopologySources(true);

            RetireGeneratedTopologyTexture(retiredGeneratedTexture);
            topologyReplacementActivatedCount++;
            topologyReplacementLastReason =
                FormatTopologyReplacementReason(build.Reason);
            ReleaseTopologyReplacementBuild(false);
            activeTopologyObstacleStale = false;
            if (DevelopmentTopologyGenerationInProgress)
            {
                CompleteDevelopmentTopologyGeneration();
            }
            return true;
        }

        private void CancelTopologyReplacementBuild(bool coalesced)
        {
            if (topologyReplacementBuild == null)
            {
                return;
            }

            topologyReplacementCancelledCount++;
            if (coalesced)
            {
                topologyReplacementCoalescedCount++;
            }
            ReleaseTopologyReplacementBuild(true);
        }

        private void ReleaseTopologyReplacementBuild(bool releaseTexture)
        {
            if (topologyReplacementBuild != null && releaseTexture &&
                topologyReplacementBuild.GeneratedTexture != null)
            {
                RenderTexture texture =
                    topologyReplacementBuild.GeneratedTexture;
                ReleaseTexture(ref texture);
                topologyReplacementBuild.GeneratedTexture = null;
            }

            topologyReplacementBuild = null;
            topologyReplacementPhase = TopologyReplacementPhase.Idle;
        }

        private static string FormatTopologyReplacementReason(
            TopologyReplacementReason reason)
        {
            return reason == TopologyReplacementReason.None
                ? "None"
                : reason.ToString().Replace(",", " +");
        }

        private void PrepareDimensionChangingTopologyTransition()
        {
            if (HasTopologyTransitionVisibleHold ||
                !CanCaptureActiveGeneratedTopology())
            {
                return;
            }

            if (CaptureActiveGeneratedTopologyTransition(true))
            {
                topologyTransitionRemappedCount++;
            }
        }

        private bool CanCaptureActiveGeneratedTopology()
        {
            return computeShader != null &&
                captureGeneratedTopologyKernel >= 0 &&
                fieldWidth > 0 && fieldHeight > 0 &&
                structuralWidth > 0 && structuralHeight > 0 &&
                metricBuffer != null &&
                metricRows.Length == fieldWidth &&
                topologyGeneratedTexture != null &&
                topologyGeneratedTexture.IsCreated() &&
                evolvingMajorTexture != null &&
                evolvingHostedNegativeTexture != null &&
                evolvingFreeWaterNegativeTexture != null &&
                evolvingConnectorTexture != null &&
                evolvingWeakSpanNegativeTexture != null;
        }

        private bool CaptureActiveGeneratedTopologyTransition(
            bool holdVisibleResources)
        {
            if (!CanCaptureActiveGeneratedTopology())
            {
                return false;
            }

            using var profilerScope =
                TopologyTransitionCaptureProfilerMarker.Auto();
            RenderTexture capture = CreateTopologyTexture(
                structuralWidth,
                structuralHeight,
                $"PS3D_RiverFoam_TopologyTransition_{Time.frameCount}");
            ComputeBuffer capturedMetricBuffer = new ComputeBuffer(
                fieldWidth,
                Marshal.SizeOf<FoamMetricRow>(),
                ComputeBufferType.Structured);
            capturedMetricBuffer.SetData(metricRows);

            ConfigureTopologyParameters(0f);
            computeShader.SetFloat("_FoamGlobalStart", allocatedGlobalStart);
            BindGeneratedTopologyInputs(captureGeneratedTopologyKernel);
            ConfigureTopologyTransitionInputs(
                captureGeneratedTopologyKernel);
            computeShader.SetBuffer(
                captureGeneratedTopologyKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                captureGeneratedTopologyKernel,
                "_FoamTopologyTransitionCaptureWrite",
                capture);
            Dispatch(
                captureGeneratedTopologyKernel,
                structuralWidth,
                structuralHeight);

            TopologyTransitionSnapshot previousSnapshot =
                topologyTransitionSnapshot;
            topologyTransitionSnapshot = new TopologyTransitionSnapshot
            {
                GeneratedTexture = capture,
                MetricBuffer = capturedMetricBuffer,
                Width = structuralWidth,
                Height = structuralHeight,
                DomainVersion = domainVersion,
                GlobalStart = allocatedGlobalStart,
                FieldLength = fieldLength,
                ValidFieldLength = validFieldLength,
                Descriptor = gridDescriptor,
                DescriptorGpuData = gridDescriptorGpuData,
                MetricRows = (FoamMetricRow[])metricRows.Clone(),
                MaterialLifetimeAuthorityActive =
                    materialLifetimeAuthorityActive
            };
            topologyTransitionElapsed = 0f;
            topologyTransitionStartedCount++;

            if (previousSnapshot != null)
            {
                topologyTransitionFlattenedCount++;
                RetireTopologyTransitionSnapshot(previousSnapshot);
            }

            if (holdVisibleResources)
            {
                DetachVisibleResourcesToTopologyTransition(
                    topologyTransitionSnapshot);
            }

            return true;
        }

        private void BindGeneratedTopologyInputs(int kernel)
        {
            computeShader.SetFloat(
                "_FoamMajorEvolutionEnabled",
                majorEvolutionReady ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamHostedNegativeEvolutionEnabled",
                hostedNegativeEvolutionReady ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamFreeWaterNegativeEvolutionEnabled",
                freeWaterEvolutionReady ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamConnectorIdentityReconstructionEnabled",
                connectorIdentityReconstructionReady ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamWeakSpanIdentityReconstructionEnabled",
                weakSpanIdentityReconstructionReady ? 1f : 0f);
            computeShader.SetTexture(
                kernel,
                "_FoamTopologyGeneratedRead",
                topologyGeneratedTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamEvolvingMajorRead",
                evolvingMajorTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamEvolvingHostedNegativeRead",
                evolvingHostedNegativeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamEvolvingFreeWaterNegativeRead",
                evolvingFreeWaterNegativeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamEvolvingConnectorRead",
                evolvingConnectorTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamEvolvingWeakSpanNegativeRead",
                evolvingWeakSpanNegativeTexture);
        }

        private void ConfigureTopologyTransitionGridDescriptor(
            int kernel,
            StylizedRiverFoamGridGpuData descriptor)
        {
            computeShader.SetVector(
                "_FoamTopologyTransitionGridDescriptorContract",
                descriptor.Contract);
            computeShader.SetVector(
                "_FoamTopologyTransitionGridDescriptorSpacing",
                descriptor.Spacing);
            computeShader.SetVector(
                "_FoamTopologyTransitionGridDescriptorLateral",
                descriptor.Lateral);
            computeShader.SetVector(
                "_FoamTopologyTransitionGridDescriptorLongitudinal",
                descriptor.Longitudinal);
            computeShader.SetVector(
                "_FoamTopologyTransitionGridDescriptorExtent",
                descriptor.Extent);
        }

        private void ConfigureTopologyTransitionInputs(int kernel)
        {
            TopologyTransitionSnapshot snapshot = topologyTransitionSnapshot;
            bool enabled = snapshot != null &&
                !snapshot.HoldsVisibleResources &&
                snapshot.GeneratedTexture != null &&
                snapshot.GeneratedTexture.IsCreated() &&
                snapshot.MetricBuffer != null;
            if (!enabled)
            {
                computeShader.SetFloat(
                    "_FoamTopologyTransitionEnabled",
                    0f);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionBlend",
                    1f);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionSameMapping",
                    1f);
                computeShader.SetInts(
                    "_FoamTopologyTransitionDimensions",
                    structuralWidth,
                    structuralHeight);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionGlobalStart",
                    allocatedGlobalStart);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionFieldLength",
                    fieldLength);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionValidLength",
                    validFieldLength);
                ConfigureTopologyTransitionGridDescriptor(
                    kernel,
                    gridDescriptorGpuData);
                computeShader.SetBuffer(
                    kernel,
                    "_FoamTopologyTransitionMetricRows",
                    metricBuffer);
                computeShader.SetTexture(
                    kernel,
                    "_FoamTopologyTransitionFromRead",
                    topologyGeneratedTexture);
                return;
            }

            bool sameMapping = snapshot.DomainVersion == domainVersion &&
                snapshot.Descriptor == gridDescriptor &&
                Mathf.Abs(snapshot.GlobalStart - allocatedGlobalStart) <
                    TransportLatticeAlignmentTolerance;
            computeShader.SetFloat(
                "_FoamTopologyTransitionEnabled",
                1f);
            computeShader.SetFloat(
                "_FoamTopologyTransitionBlend",
                TopologyTransitionProgress);
            computeShader.SetFloat(
                "_FoamTopologyTransitionSameMapping",
                sameMapping ? 1f : 0f);
            computeShader.SetInts(
                "_FoamTopologyTransitionDimensions",
                snapshot.Width,
                snapshot.Height);
            computeShader.SetFloat(
                "_FoamTopologyTransitionGlobalStart",
                snapshot.GlobalStart);
            computeShader.SetFloat(
                "_FoamTopologyTransitionFieldLength",
                snapshot.FieldLength);
            computeShader.SetFloat(
                "_FoamTopologyTransitionValidLength",
                snapshot.ValidFieldLength);
            ConfigureTopologyTransitionGridDescriptor(
                kernel,
                snapshot.DescriptorGpuData);
            computeShader.SetBuffer(
                kernel,
                "_FoamTopologyTransitionMetricRows",
                snapshot.MetricBuffer);
            computeShader.SetTexture(
                kernel,
                "_FoamTopologyTransitionFromRead",
                snapshot.GeneratedTexture);
        }

        private bool AdvanceTopologyTransition(float deltaTime)
        {
            if (topologyTransitionSnapshot == null ||
                topologyTransitionSnapshot.HoldsVisibleResources ||
                initializationPhase != InitializationPhase.Ready)
            {
                return false;
            }

            float previousProgress = TopologyTransitionProgress;
            topologyTransitionElapsed = Mathf.Min(
                TopologyReplacementTransitionSeconds,
                topologyTransitionElapsed + Mathf.Max(0f, deltaTime));
            if (topologyTransitionElapsed >=
                TopologyReplacementTransitionSeconds - 0.0001f)
            {
                TopologyTransitionSnapshot completed =
                    topologyTransitionSnapshot;
                topologyTransitionSnapshot = null;
                topologyTransitionElapsed = 0f;
                topologyTransitionCompletedCount++;
                RetireTopologyTransitionSnapshot(completed);
                return true;
            }

            return TopologyTransitionProgress > previousProgress + 0.000001f;
        }

        private void DetachVisibleResourcesToTopologyTransition(
            TopologyTransitionSnapshot snapshot)
        {
            if (snapshot == null || currentState == null ||
                previousState == null || topologyTexture == null ||
                topologySourcesTexture == null)
            {
                return;
            }

            snapshot.PreviousState = previousState;
            snapshot.CurrentState = currentState;
            snapshot.Topology = topologyTexture;
            snapshot.TopologySources = topologySourcesTexture;
            snapshot.ObstacleExclusion = obstacleExclusionTexture;
            snapshot.Boundary = boundaryTexture;
            snapshot.Interpolation = simulationInterpolation;
            snapshot.HoldsVisibleResources = true;

            RenderTexture detachedWriteState = writeState;
            if (stateA == previousState || stateA == currentState ||
                stateA == detachedWriteState)
            {
                stateA = null;
            }
            if (stateB == previousState || stateB == currentState ||
                stateB == detachedWriteState)
            {
                stateB = null;
            }
            if (presentationPreviousState == previousState ||
                presentationPreviousState == currentState)
            {
                presentationPreviousState = null;
            }
            previousState = null;
            currentState = null;
            writeState = null;
            if (detachedWriteState != snapshot.PreviousState &&
                detachedWriteState != snapshot.CurrentState)
            {
                ReleaseTexture(ref detachedWriteState);
            }
            topologyTexture = null;
            topologySourcesTexture = null;
            obstacleExclusionTexture = null;
            boundaryTexture = null;
        }

        private PersistentStateReplacementPolicy
            ResolvePersistentStateReplacementPolicy(
                TopologyTransitionSnapshot snapshot,
                out int offsetX,
                out int offsetY,
                out string detail)
        {
            offsetX = 0;
            offsetY = 0;
            detail = "No held persistent state.";
            if (snapshot == null || !snapshot.HoldsVisibleResources ||
                snapshot.CurrentState == null)
            {
                return PersistentStateReplacementPolicy.None;
            }

            return ResolvePersistentStateReplacementPolicyContract(
                snapshot.Descriptor,
                snapshot.GlobalStart,
                snapshot.MetricRows,
                gridDescriptor,
                allocatedGlobalStart,
                metricRows,
                out offsetX,
                out offsetY,
                out detail);
        }

        private static PersistentStateReplacementPolicy
            ResolvePersistentStateReplacementPolicyContract(
                StylizedRiverFoamGridDescriptor previous,
                float previousGlobalStart,
                FoamMetricRow[] previousMetricRows,
                StylizedRiverFoamGridDescriptor current,
                float currentGlobalStart,
                FoamMetricRow[] currentMetricRows,
                out int offsetX,
                out int offsetY,
                out string detail)
        {
            offsetX = 0;
            offsetY = 0;
            if (!previous.IsCreated || !current.IsCreated ||
                !previous.UsesFixedMetricLattice ||
                !current.UsesFixedMetricLattice)
            {
                detail =
                    "Persistent material is cleared unless both descriptors " +
                    "use the fixed metric lattice.";
                return PersistentStateReplacementPolicy.ClearLegacyMapping;
            }

            if (previous.MappingContractVersion !=
                    current.MappingContractVersion ||
                previous.Mapping != current.Mapping)
            {
                detail =
                    "Persistent material is cleared for unsupported descriptor " +
                    "or mapping contracts.";
                return PersistentStateReplacementPolicy.
                    ClearUnsupportedContract;
            }

            if (Mathf.Abs(previous.ResolvedDxMetres -
                    current.ResolvedDxMetres) >
                    TransportLatticeAlignmentTolerance ||
                Mathf.Abs(previous.ResolvedDyMetres -
                    current.ResolvedDyMetres) >
                    TransportLatticeAlignmentTolerance ||
                Mathf.Abs(previous.LateralLatticePhaseMetres -
                    current.LateralLatticePhaseMetres) >
                    TransportLatticeAlignmentTolerance)
            {
                detail =
                    "Persistent material is cleared when spacing or lateral " +
                    "lattice phase changes.";
                return PersistentStateReplacementPolicy.
                    ClearSpacingOrPhaseMismatch;
            }

            float previousOrigin = previousGlobalStart +
                previous.FieldOrStripStartMetres;
            float currentOrigin = currentGlobalStart +
                current.FieldOrStripStartMetres;
            float offsetXFloat = (currentOrigin - previousOrigin) /
                Mathf.Max(0.0001f, previous.ResolvedDxMetres);
            int resolvedOffsetX = Mathf.RoundToInt(offsetXFloat);
            if (Mathf.Abs(offsetXFloat - resolvedOffsetX) >
                TransportLatticeAlignmentTolerance)
            {
                detail =
                    "Persistent material is cleared when longitudinal lattice " +
                    "origins are not integer aligned.";
                return PersistentStateReplacementPolicy.
                    ClearNonIntegerAlignment;
            }

            int resolvedOffsetY = current.GlobalYBase -
                previous.GlobalYBase;
            if (!ArePersistentStateCurvatureRowsCompatible(
                    previous,
                    previousMetricRows,
                    current,
                    currentMetricRows,
                    resolvedOffsetX))
            {
                detail =
                    "Persistent material is cleared when overlapping physical " +
                    "rows changed centreline curvature.";
                return PersistentStateReplacementPolicy.
                    ClearCurvatureMismatch;
            }

            offsetX = resolvedOffsetX;
            offsetY = resolvedOffsetY;
            detail =
                $"Exact fixed-lattice copy; offsets=({offsetX},{offsetY}).";
            return PersistentStateReplacementPolicy.ExactFixedLatticeCopy;
        }

        private static bool ArePersistentStateCurvatureRowsCompatible(
            StylizedRiverFoamGridDescriptor previous,
            FoamMetricRow[] previousMetricRows,
            StylizedRiverFoamGridDescriptor current,
            FoamMetricRow[] currentMetricRows,
            int offsetX)
        {
            if (previousMetricRows == null || currentMetricRows == null ||
                previousMetricRows.Length != previous.ColumnCount ||
                currentMetricRows.Length != current.ColumnCount)
            {
                return false;
            }

            for (int currentX = 0;
                 currentX < currentMetricRows.Length;
                 currentX++)
            {
                int previousX = currentX + offsetX;
                if (previousX < 0 ||
                    previousX >= previousMetricRows.Length)
                {
                    continue;
                }

                float previousCurvature =
                    previousMetricRows[previousX].TopologyData.x;
                float currentCurvature =
                    currentMetricRows[currentX].TopologyData.x;
                if (Mathf.Abs(previousCurvature - currentCurvature) >
                    TransportCurvatureCompatibilityTolerance)
                {
                    return false;
                }
            }

            return true;
        }

        private void ApplyPersistentStateReplacementBeforeVisibleHoldRelease()
        {
            TopologyTransitionSnapshot snapshot = topologyTransitionSnapshot;
            if (snapshot == null || !snapshot.HoldsVisibleResources)
            {
                return;
            }

            PersistentStateReplacementPolicy policy =
                ResolvePersistentStateReplacementPolicy(
                    snapshot,
                    out int offsetX,
                    out int offsetY,
                    out string detail);
            snapshot.ReplacementPolicy = policy;
            snapshot.ReplacementOffsetX = offsetX;
            snapshot.ReplacementOffsetY = offsetY;
            snapshot.ReplacementDetail = detail;
            lastPersistentStateReplacementPolicy = policy;
            lastPersistentStateReplacementDetail = detail;

            if (policy !=
                PersistentStateReplacementPolicy.ExactFixedLatticeCopy)
            {
                if (policy != PersistentStateReplacementPolicy.None)
                {
                    persistentStateReplacementClearCount++;
                }
                return;
            }

            if (computeShader == null || remapPersistentStateKernel < 0 ||
                snapshot.CurrentState == null ||
                currentState == null || previousState == null ||
                metricBuffer == null || boundaryTexture == null ||
                obstacleExclusionTexture == null)
            {
                lastPersistentStateReplacementPolicy =
                    PersistentStateReplacementPolicy.ClearUnsupportedContract;
                lastPersistentStateReplacementDetail =
                    "Exact fixed-lattice remap resources were unavailable; " +
                    "new persistent state remains deliberately clear.";
                persistentStateReplacementClearCount++;
                return;
            }

            ConfigureGridDescriptorComputeParameters();
            computeShader.SetInts(
                "_FoamDimensions",
                fieldWidth,
                fieldHeight);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                allocatedGlobalStart);
            computeShader.SetInts(
                "_FoamTopologyTransitionDimensions",
                snapshot.Width,
                snapshot.Height);
            computeShader.SetFloat(
                "_FoamTopologyTransitionGlobalStart",
                snapshot.GlobalStart);
            computeShader.SetFloat(
                "_FoamTopologyTransitionFieldLength",
                snapshot.FieldLength);
            computeShader.SetFloat(
                "_FoamTopologyTransitionValidLength",
                snapshot.ValidFieldLength);
            ConfigureTopologyTransitionGridDescriptor(
                remapPersistentStateKernel,
                snapshot.DescriptorGpuData);
            computeShader.SetInt(
                "_FoamPersistentStateRemapEnabled",
                1);
            computeShader.SetBuffer(
                remapPersistentStateKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                remapPersistentStateKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                remapPersistentStateKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                remapPersistentStateKernel,
                "_FoamPersistentStateRemapRead",
                snapshot.CurrentState);

            computeShader.SetTexture(
                remapPersistentStateKernel,
                "_FoamStateWrite",
                currentState);
            Dispatch(
                remapPersistentStateKernel,
                fieldWidth,
                fieldHeight);
            computeShader.SetTexture(
                remapPersistentStateKernel,
                "_FoamStateWrite",
                previousState);
            Dispatch(
                remapPersistentStateKernel,
                fieldWidth,
                fieldHeight);
            computeShader.SetInt(
                "_FoamPersistentStateRemapEnabled",
                0);

            materialLifetimeAuthorityActive =
                snapshot.MaterialLifetimeAuthorityActive;
            persistentStateReplacementExactCount++;
        }

        private void ReleaseTopologyTransitionVisibleHold()
        {
            TopologyTransitionSnapshot snapshot = topologyTransitionSnapshot;
            if (snapshot == null || !snapshot.HoldsVisibleResources)
            {
                return;
            }

            ReleaseHeldTopologyTransitionTextures(snapshot);
            snapshot.HoldsVisibleResources = false;
        }

        private void RetireTopologyTransitionSnapshot(
            TopologyTransitionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            ReleaseHeldTopologyTransitionTextures(snapshot);
            ReleaseRetiredTopologyTransitionResourcesNow();
            retiredTopologyTransitionTexture = snapshot.GeneratedTexture;
            retiredTopologyTransitionMetricBuffer = snapshot.MetricBuffer;
            retiredTopologyTransitionReleaseFrame = Time.frameCount + 2;
            snapshot.GeneratedTexture = null;
            snapshot.MetricBuffer = null;
            snapshot.MetricRows = Array.Empty<FoamMetricRow>();
        }

        private void ReleaseHeldTopologyTransitionTextures(
            TopologyTransitionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            RenderTexture previous = snapshot.PreviousState;
            RenderTexture current = snapshot.CurrentState;
            if (current == previous)
            {
                current = null;
            }
            ReleaseTexture(ref previous);
            ReleaseTexture(ref current);
            snapshot.PreviousState = null;
            snapshot.CurrentState = null;

            RenderTexture topology = snapshot.Topology;
            RenderTexture sources = snapshot.TopologySources;
            RenderTexture obstacle = snapshot.ObstacleExclusion;
            ReleaseTexture(ref topology);
            ReleaseTexture(ref sources);
            ReleaseTexture(ref obstacle);
            snapshot.Topology = null;
            snapshot.TopologySources = null;
            snapshot.ObstacleExclusion = null;

            if (snapshot.Boundary != null)
            {
                DestroyUnityObject(snapshot.Boundary);
                snapshot.Boundary = null;
            }
        }

        private void ReleaseRetiredTopologyTransitionResourcesIfReady()
        {
            if (retiredTopologyTransitionReleaseFrame < 0 ||
                Time.frameCount < retiredTopologyTransitionReleaseFrame)
            {
                return;
            }

            ReleaseRetiredTopologyTransitionResourcesNow();
        }

        private void ReleaseRetiredTopologyTransitionResourcesNow()
        {
            ReleaseTexture(ref retiredTopologyTransitionTexture);
            retiredTopologyTransitionMetricBuffer?.Release();
            retiredTopologyTransitionMetricBuffer = null;
            retiredTopologyTransitionReleaseFrame = -1;
        }

        private void RetireGeneratedTopologyTexture(RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            ReleaseRetiredGeneratedTopologyTextureNow();
            retiredGeneratedTopologyTexture = texture;
            retiredGeneratedTopologyReleaseFrame = Time.frameCount + 2;
        }

        private void ReleaseRetiredGeneratedTopologyTextureIfReady()
        {
            if (retiredGeneratedTopologyReleaseFrame < 0 ||
                Time.frameCount < retiredGeneratedTopologyReleaseFrame)
            {
                return;
            }

            ReleaseRetiredGeneratedTopologyTextureNow();
        }

        private void ReleaseRetiredGeneratedTopologyTextureNow()
        {
            ReleaseTexture(ref retiredGeneratedTopologyTexture);
            retiredGeneratedTopologyReleaseFrame = -1;
        }

        private void ReleaseTopologyTransition(bool releaseVisibleResources)
        {
            if (topologyTransitionSnapshot != null)
            {
                if (releaseVisibleResources)
                {
                    ReleaseHeldTopologyTransitionTextures(
                        topologyTransitionSnapshot);
                }
                RenderTexture generated =
                    topologyTransitionSnapshot.GeneratedTexture;
                ReleaseTexture(ref generated);
                topologyTransitionSnapshot.GeneratedTexture = null;
                topologyTransitionSnapshot.MetricBuffer?.Release();
                topologyTransitionSnapshot.MetricBuffer = null;
                topologyTransitionSnapshot.MetricRows =
                    Array.Empty<FoamMetricRow>();
                topologyTransitionSnapshot = null;
            }

            topologyTransitionElapsed = 0f;
            ReleaseRetiredTopologyTransitionResourcesNow();
            ReleaseRetiredGeneratedTopologyTextureNow();
        }

        private long EstimateTopologyTransitionBytes()
        {
            TopologyTransitionSnapshot snapshot = topologyTransitionSnapshot;
            if (snapshot == null)
            {
                return 0L;
            }

            return EstimateTextureBytes(snapshot.GeneratedTexture) +
                EstimateTextureBytes(snapshot.PreviousState) +
                EstimateTextureBytes(snapshot.CurrentState) +
                EstimateTextureBytes(snapshot.Topology) +
                EstimateTextureBytes(snapshot.TopologySources) +
                EstimateTextureBytes(snapshot.ObstacleExclusion) +
                EstimateTextureBytes(snapshot.Boundary);
        }

        private bool AdvanceQueuedRebuild()
        {
            PromotePendingRebuildPhase();
            if (rebuildPhase == RebuildPhase.Idle)
            {
                return false;
            }

            switch (rebuildPhase)
            {
                case RebuildPhase.BuildBoundary:
                    using (RebuildBuildBoundaryProfilerMarker.Auto())
                    {
                        RebuildBoundaryTexture(false);
                    }

                    pendingBoundaryRebuild = false;
                    rebuildPhase = RebuildPhase.ApplyBoundary;
                    break;

                case RebuildPhase.ApplyBoundary:
                    using (RebuildApplyBoundaryProfilerMarker.Auto())
                    {
                        ApplyBoundaryToState(stateA);
                        ApplyBoundaryToState(stateB);
                        ApplyBoundaryToState(presentationPreviousState);
                    }

                    if (!pendingObstacleRebuild &&
                        pendingTopologyReplacementAfterMaintenance)
                    {
                        RequestTopologyReplacement(
                            TopologyReplacementReason.Boundary,
                            false);
                        pendingTopologyReplacementAfterMaintenance = false;
                    }

                    rebuildPhase = pendingObstacleRebuild
                        ? RebuildPhase.WaitForObstacleStability
                        : RebuildPhase.RefreshTopologySources;
                    break;

                case RebuildPhase.WaitForObstacleStability:
                    using (RebuildWaitObstacleProfilerMarker.Auto())
                    {
                        disturbanceRuntime ??=
                            GetComponent<StylizedRiverDisturbanceRuntime>();
                        if (disturbanceRuntime != null &&
                            !disturbanceRuntime.GeneratedObstacleRegistryReady)
                        {
                            pendingObstacleObservedVersion = int.MinValue;
                            pendingObstacleStableFrameCount = 0;
                            break;
                        }

                        int currentObstacleVersion = disturbanceRuntime != null
                            ? disturbanceRuntime.ObstacleGeometryVersion
                            : -1;

                        if (currentObstacleVersion !=
                            pendingObstacleObservedVersion)
                        {
                            pendingObstacleObservedVersion =
                                currentObstacleVersion;
                            pendingObstacleStableFrameCount = 0;
                        }
                        else
                        {
                            pendingObstacleStableFrameCount++;
                            if (pendingObstacleStableFrameCount >=
                                ObstacleRebuildStableFrameCount)
                            {
                                rebuildPhase =
                                    RebuildPhase.BuildObstacleExclusion;
                            }
                        }
                    }
                    break;

                case RebuildPhase.BuildObstacleExclusion:
                {
                    if (Application.isPlaying &&
                        !editorTopologyPreparationInProgress)
                    {
                        activeTopologyObstacleStale = true;
                        activeTopologyRequiresExplicitPreparation = true;
                        MarkActiveTopologyCacheStale(
                            "Stale — Obstacles Changed",
                            "A queued obstacle rebuild reached Play Mode. P1 " +
                            "blocked mesh scanning and GPU readback; prepare " +
                            "the Foam topology cache explicitly in Edit Mode.");
                        pendingObstacleRebuild = false;
                        pendingTopologyReplacementAfterMaintenance = false;
                        pendingObstacleObservedVersion = int.MinValue;
                        pendingObstacleStableFrameCount = 0;
                        rebuildPhase = RebuildPhase.RefreshTopologySources;
                        break;
                    }

                    bool prepareTopologyReplacement =
                        pendingTopologyReplacementAfterMaintenance;
                    using (RebuildBuildObstacleProfilerMarker.Auto())
                    {
                        RebuildObstacleExclusionCache(
                            prepareTopologyReplacement);
                    }

                    pendingObstacleRebuild = false;
                    pendingObstacleObservedVersion = int.MinValue;
                    pendingObstacleStableFrameCount = 0;
                    if (prepareTopologyReplacement)
                    {
                        RequestTopologyReplacement(
                            TopologyReplacementReason.Obstacle,
                            false);
                        pendingTopologyReplacementAfterMaintenance = false;
                    }
                    rebuildPhase = RebuildPhase.RefreshTopologySources;
                    break;
                }

                case RebuildPhase.RefreshTopologySources:
                    using (RebuildRefreshSourcesProfilerMarker.Auto())
                    {
                        RefreshDynamicTopologySources(true);
                    }

                    pendingTopologyRefresh = false;
                    rebuildPhase = RebuildPhase.Idle;
                    break;
            }

            return true;
        }

        private void PromotePendingRebuildPhase()
        {
            if (pendingBoundaryRebuild &&
                (rebuildPhase == RebuildPhase.Idle ||
                 (int)rebuildPhase > (int)RebuildPhase.BuildBoundary))
            {
                rebuildPhase = RebuildPhase.BuildBoundary;
                return;
            }

            if (pendingObstacleRebuild &&
                (rebuildPhase == RebuildPhase.Idle ||
                 (int)rebuildPhase >
                    (int)RebuildPhase.BuildObstacleExclusion))
            {
                rebuildPhase = RebuildPhase.WaitForObstacleStability;
                return;
            }

            if (pendingTopologyRefresh &&
                rebuildPhase == RebuildPhase.Idle)
            {
                rebuildPhase = RebuildPhase.RefreshTopologySources;
            }
        }
    }
}
