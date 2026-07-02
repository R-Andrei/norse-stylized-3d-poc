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
        private void InitializeConnectorIdentityReconstruction(
            bool rebuildField = true)
        {
            ReleaseConnectorIdentityReconstructionResources();
            if (!majorEvolutionReady ||
                connectorTopology == null || pocketTopology == null ||
                computeShader == null ||
                majorEvolutionBuffer == null ||
                majorMaskTextureArray == null ||
                hostedNegativeEvolutionBuffer == null ||
                hostedNegativeMaskTextureArray == null ||
                freeWaterEvolutionBuffer == null ||
                freeWaterNegativeMaskTextureArray == null ||
                topologyGeneratedTexture == null ||
                boundaryTexture == null ||
                obstacleExclusionTexture == null ||
                metricBuffer == null ||
                evolvingConnectorTexture == null ||
                evolvingWeakSpanNegativeTexture == null ||
                buildEvolvingMajorSupportKernel < 0)
            {
                ClearRenderTexture(evolvingConnectorTexture);
                ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                return;
            }

            int acceptedConnectorCount =
                connectorTopology.AcceptedConnectorCount;
            IReadOnlyList<StylizedRiverFoamConnectorPath> acceptedPaths =
                connectorTopology.PreparedPaths;
            IReadOnlyList<StylizedRiverFoamConnectorPath> cataloguePaths =
                connectorTopology.PreparedRelationshipCataloguePaths;
            bool allConnectorsPrepared = acceptedConnectorCount > 0 &&
                connectorTopology.PreparedConnectorCount ==
                    acceptedConnectorCount &&
                acceptedPaths.Count == acceptedConnectorCount &&
                cataloguePaths.Count >= acceptedConnectorCount &&
                connectorTopology.PreparedRelationshipCatalogueCount ==
                    cataloguePaths.Count;

            List<FoamConnectorIdentityData> connectorRecords = new(
                acceptedConnectorCount);
            List<ConnectorEvolutionSlot> evolutionSlots = new(
                acceptedConnectorCount);
            List<ConnectorRelationshipCandidate> relationshipCandidates = new(
                cataloguePaths.Count);
            Dictionary<uint, int> candidateIndexByStableId = new(
                cataloguePaths.Count);
            Dictionary<uint, int> connectorIndexByStableId = new(
                acceptedConnectorCount);
            int maximumPathPointCount = 0;

            if (allConnectorsPrepared)
            {
                for (int pathIndex = 0;
                     pathIndex < cataloguePaths.Count;
                     pathIndex++)
                {
                    StylizedRiverFoamConnectorPath path =
                        cataloguePaths[pathIndex];
                    Vector2[] points = path?.PreparedMetricPointData;
                    float[] cumulative = path?.NormalizedCumulativeLengthData;
                    if (path == null || !path.PreparationAvailable ||
                        points == null || points.Length < 2 ||
                        points.Length >
                            StylizedRiverFoamConnectorTopologyGenerator
                                .MaximumPreparedPathPointCount ||
                        cumulative == null ||
                        cumulative.Length != points.Length ||
                        candidateIndexByStableId.ContainsKey(path.StableId) ||
                        !TryResolveMajorEvolutionSlot(
                            path.StartEndpointBinding,
                            out int startHostSlotIndex) ||
                        !TryResolveMajorEvolutionSlot(
                            path.EndEndpointBinding,
                            out int endHostSlotIndex) ||
                        startHostSlotIndex == endHostSlotIndex)
                    {
                        allConnectorsPrepared = false;
                        break;
                    }

                    candidateIndexByStableId.Add(
                        path.StableId,
                        relationshipCandidates.Count);
                    relationshipCandidates.Add(
                        new ConnectorRelationshipCandidate
                        {
                            Path = path,
                            StartHostSlotIndex = startHostSlotIndex,
                            EndHostSlotIndex = endHostSlotIndex,
                            BasePathLengthMetres =
                                MeasureConnectorPreparedPathLength(points),
                            SelectionWeight = 1f
                        });
                    maximumPathPointCount = Mathf.Max(
                        maximumPathPointCount,
                        points.Length);
                }
            }

            if (allConnectorsPrepared)
            {
                ResolveConnectorRelationshipSelectionWeights(
                    relationshipCandidates);
            }

            Vector4[] flattenedPoints = allConnectorsPrepared
                ? new Vector4[
                    acceptedConnectorCount * maximumPathPointCount]
                : Array.Empty<Vector4>();
            if (allConnectorsPrepared)
            {
                for (int pathIndex = 0;
                     pathIndex < acceptedPaths.Count;
                     pathIndex++)
                {
                    StylizedRiverFoamConnectorPath path =
                        acceptedPaths[pathIndex];
                    Vector2[] points = path?.PreparedMetricPointData;
                    float[] cumulative = path?.NormalizedCumulativeLengthData;
                    if (path == null ||
                        !candidateIndexByStableId.TryGetValue(
                            path.StableId,
                            out int candidateIndex) ||
                        connectorIndexByStableId.ContainsKey(path.StableId) ||
                        points == null || points.Length < 2 ||
                        cumulative == null || cumulative.Length != points.Length)
                    {
                        allConnectorsPrepared = false;
                        break;
                    }

                    int pointOffset = pathIndex * maximumPathPointCount;
                    for (int pointIndex = 0;
                         pointIndex < points.Length;
                         pointIndex++)
                    {
                        flattenedPoints[pointOffset + pointIndex] =
                            new Vector4(
                                points[pointIndex].x,
                                points[pointIndex].y,
                                Mathf.Clamp01(cumulative[pointIndex]),
                                0f);
                    }

                    float outerRadius = Mathf.Lerp(
                        0.17f,
                        0.27f,
                        HashConnectorIdentity(path.StableId, 31u));
                    float coreRadius = outerRadius * Mathf.Lerp(
                        0.20f,
                        0.36f,
                        HashConnectorIdentity(path.StableId, 32u));
                    connectorIndexByStableId.Add(
                        path.StableId,
                        connectorRecords.Count);
                    connectorRecords.Add(new FoamConnectorIdentityData
                    {
                        PointRangeAndRadii = new Vector4(
                            pointOffset,
                            points.Length,
                            outerRadius,
                            coreRadius)
                    });
                    evolutionSlots.Add(new ConnectorEvolutionSlot
                    {
                        StableId = path.StableId,
                        OriginalCandidateIndex = candidateIndex,
                        AssignedCandidateIndex = candidateIndex,
                        ActiveCandidateIndex = candidateIndex,
                        LastReleasedCandidateIndex = -1,
                        ReleaseCooldownTicks = 0,
                        RelationshipRevision = 0,
                        PointOffset = pointOffset,
                        PointCapacity = maximumPathPointCount,
                        PointCount = points.Length,
                        ActiveStartAnchorIndex = -1,
                        ActiveEndAnchorIndex = -1,
                        PendingReleaseReason = ConnectorReleaseReason.None,
                        TurnoverFallbackCandidateIndex = -1,
                        ReferenceLengthMetres = 0f,
                        ReferenceCandidateIndex = -1,
                        ReferenceStartAnchorIndex = -2,
                        ReferenceEndAnchorIndex = -2,
                        ObservedStartRecycleCount = -1,
                        ObservedEndRecycleCount = -1,
                        StretchBlockedCandidateIndex = -1,
                        StretchBlockedStartRecycleCount = -1,
                        StretchBlockedEndRecycleCount = -1,
                        IsActive = true,
                        HasRuntimeState = false
                    });
                }
            }

            if (!allConnectorsPrepared ||
                connectorRecords.Count != acceptedConnectorCount ||
                evolutionSlots.Count != acceptedConnectorCount ||
                relationshipCandidates.Count != cataloguePaths.Count)
            {
                connectorRecords.Clear();
                evolutionSlots.Clear();
                relationshipCandidates.Clear();
                connectorIndexByStableId.Clear();
                flattenedPoints = Array.Empty<Vector4>();
            }

            connectorIdentityGpuData = connectorRecords.ToArray();
            connectorPathPointGpuData = flattenedPoints;
            connectorEvolutionSlots = evolutionSlots.ToArray();
            connectorRelationshipCandidates = relationshipCandidates.ToArray();
            connectorCandidateClaimed = new bool[
                connectorRelationshipCandidates.Length];
            connectorMajorDegree = new int[majorEvolutionSlots.Length];
            connectorPreviousMajorDegree = new int[
                majorEvolutionSlots.Length];
            connectorMajorPairClaimed = new bool[
                majorEvolutionSlots.Length * majorEvolutionSlots.Length];
            connectorIdentityReconstructionReady =
                acceptedConnectorCount > 0 &&
                connectorIdentityGpuData.Length == acceptedConnectorCount &&
                connectorEvolutionSlots.Length == acceptedConnectorCount &&
                connectorRelationshipCandidates.Length >=
                    acceptedConnectorCount &&
                maximumPathPointCount >= 2 &&
                connectorPathPointGpuData.Length ==
                    acceptedConnectorCount * maximumPathPointCount;

            IReadOnlyList<StylizedRiverFoamPreparedWeakSpanRegion> weakSpans =
                pocketTopology.PreparedWeakSpanRegions;
            int acceptedWeakSpanCount =
                pocketTopology.AcceptedConnectorWeakSpanCount;
            bool allWeakSpansPrepared =
                connectorIdentityReconstructionReady &&
                acceptedWeakSpanCount > 0 &&
                pocketTopology.PreparedWeakSpanRegionCount ==
                    acceptedWeakSpanCount &&
                weakSpans.Count == acceptedWeakSpanCount;
            List<FoamWeakSpanIdentityData> weakSpanRecords = new(
                acceptedWeakSpanCount);
            if (allWeakSpansPrepared)
            {
                for (int weakSpanIndex = 0;
                     weakSpanIndex < weakSpans.Count;
                     weakSpanIndex++)
                {
                    StylizedRiverFoamPreparedWeakSpanRegion weakSpan =
                        weakSpans[weakSpanIndex];
                    if (weakSpan == null || !weakSpan.IsAvailable ||
                        !connectorIndexByStableId.TryGetValue(
                            weakSpan.ConnectorStableId,
                            out int connectorIndex))
                    {
                        allWeakSpansPrepared = false;
                        break;
                    }

                    weakSpanRecords.Add(new FoamWeakSpanIdentityData
                    {
                        ConnectorAndPath = new Vector4(
                            connectorIndex,
                            weakSpan.NormalizedPathDistance,
                            weakSpan.MinimumNormalizedPathDistance,
                            weakSpan.MaximumNormalizedPathDistance),
                        Shape = new Vector4(
                            weakSpan.AlongRadiusMetres,
                            weakSpan.AcrossRadiusMetres,
                            weakSpan.Strength,
                            weakSpan.AcceptedOrientationRadians),
                        NoiseSeed = weakSpan.StableId ^ 0x6C8E9CF5u,
                        Reserved0 = 0u,
                        Reserved1 = 0u,
                        Reserved2 = 0u
                    });
                }
            }

            if (!allWeakSpansPrepared ||
                weakSpanRecords.Count != acceptedWeakSpanCount)
            {
                weakSpanRecords.Clear();
            }

            weakSpanIdentityGpuData = weakSpanRecords.ToArray();
            weakSpanIdentityReconstructionReady =
                weakSpanIdentityGpuData.Length == acceptedWeakSpanCount &&
                acceptedWeakSpanCount > 0;

            UpdateConnectorEvolutionDescriptors(false);
            EnsureConnectorIdentityBuffers();
            if (connectorIdentityReconstructionReady)
            {
                connectorIdentityBuffer.SetData(connectorIdentityGpuData);
                connectorPathPointBuffer.SetData(connectorPathPointGpuData);
            }
            if (weakSpanIdentityReconstructionReady)
            {
                weakSpanIdentityBuffer.SetData(weakSpanIdentityGpuData);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            connectorIdentityParityPending =
                connectorIdentityReconstructionReady &&
                IsTopologyDebugActive &&
                SystemInfo.supportsAsyncGPUReadback;
            connectorIdentityParityReadbackPending = false;
            connectorIdentityParityAvailable = false;
            connectorIdentityParityMeanDifference = 0f;
            connectorIdentityParityMaximumDifference = 0f;
            weakSpanIdentityParityPending =
                weakSpanIdentityReconstructionReady &&
                IsTopologyDebugActive &&
                SystemInfo.supportsAsyncGPUReadback;
            weakSpanIdentityParityReadbackPending = false;
            weakSpanIdentityParityAvailable = false;
            weakSpanIdentityParityMeanDifference = 0f;
            weakSpanIdentityParityMaximumDifference = 0f;
#endif

            if (rebuildField)
            {
                BuildEvolvingMajorField();
            }
        }

        private void UpdateConnectorEvolutionDescriptors(
            bool trackTransitions)
        {
            ResetConnectorEvolutionDescriptorCounters();
            if (!CanUpdateConnectorEvolutionDescriptors())
            {
                return;
            }

            CapturePreviousConnectorMajorDegree();
            ResetConnectorEvolutionClaimsAndSlots();
            PreserveValidConnectorRelationships(trackTransitions);
            ApplyDirectedConnectorReplacementRequests();
            AssignAvailableConnectorRelationships();
            WriteConnectorEvolutionRuntimeState(trackTransitions);
            UpdateConnectorDegreeTelemetry();
            UpdateWeakSpanEvolutionActiveCount();
        }

        private void ResetConnectorEvolutionDescriptorCounters()
        {
            connectorEvolutionActiveCount = 0;
            connectorEvolutionTemporaryAbsenceCount = 0;
            connectorEvolutionIdentityPathCount = 0;
            connectorEvolutionRecycleVariantCount = 0;
            connectorEvolutionOriginalRelationshipCount = 0;
            connectorEvolutionReplacementRelationshipCount = 0;
            weakSpanEvolutionActiveCount = 0;
        }

        private bool CanUpdateConnectorEvolutionDescriptors()
        {
            return connectorIdentityReconstructionReady &&
                connectorEvolutionSlots.Length ==
                    connectorIdentityGpuData.Length &&
                connectorRelationshipCandidates.Length > 0 &&
                connectorCandidateClaimed.Length ==
                    connectorRelationshipCandidates.Length &&
                connectorMajorDegree.Length == majorEvolutionSlots.Length &&
                connectorPreviousMajorDegree.Length ==
                    majorEvolutionSlots.Length &&
                connectorMajorPairClaimed.Length ==
                    majorEvolutionSlots.Length * majorEvolutionSlots.Length &&
                majorTopology != null && river != null &&
                river.Domain.IsValid;
        }

        private void CapturePreviousConnectorMajorDegree()
        {
            Array.Clear(
                connectorPreviousMajorDegree,
                0,
                connectorPreviousMajorDegree.Length);
            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ConnectorEvolutionSlot previousSlot =
                    connectorEvolutionSlots[connectorIndex];
                int previousCandidateIndex =
                    previousSlot.AssignedCandidateIndex;
                if (!previousSlot.IsActive || previousCandidateIndex < 0 ||
                    previousCandidateIndex >=
                        connectorRelationshipCandidates.Length)
                {
                    continue;
                }

                ConnectorRelationshipCandidate previousCandidate =
                    connectorRelationshipCandidates[
                        previousCandidateIndex];
                int previousStartHost =
                    previousCandidate.StartHostSlotIndex;
                int previousEndHost = previousCandidate.EndHostSlotIndex;
                if (previousStartHost >= 0 &&
                    previousStartHost <
                        connectorPreviousMajorDegree.Length &&
                    previousEndHost >= 0 &&
                    previousEndHost <
                        connectorPreviousMajorDegree.Length &&
                    previousStartHost != previousEndHost)
                {
                    connectorPreviousMajorDegree[previousStartHost]++;
                    connectorPreviousMajorDegree[previousEndHost]++;
                }
            }
        }

        private void ResetConnectorEvolutionClaimsAndSlots()
        {
            Array.Clear(
                connectorCandidateClaimed,
                0,
                connectorCandidateClaimed.Length);
            Array.Clear(
                connectorMajorDegree,
                0,
                connectorMajorDegree.Length);
            Array.Clear(
                connectorMajorPairClaimed,
                0,
                connectorMajorPairClaimed.Length);

            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ref ConnectorEvolutionSlot slot =
                    ref connectorEvolutionSlots[connectorIndex];
                slot.PendingReleaseReason = ConnectorReleaseReason.None;
                slot.TurnoverFallbackCandidateIndex = -1;
                RefreshConnectorStretchBlock(ref slot);
            }
        }

        private void PreserveValidConnectorRelationships(bool trackTransitions)
        {
            // Preserve valid current relationships first, except when a host
            // recycle makes the deterministic turnover decision request a new
            // pair or the live path exceeds its assignment-relative stretch
            // limit. Those releases are handled in a dedicated second pass so
            // retained relationships cannot be stolen by replacement slots.
            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ref ConnectorEvolutionSlot slot =
                    ref connectorEvolutionSlots[connectorIndex];
                int candidateIndex = slot.AssignedCandidateIndex;
                if (candidateIndex < 0)
                {
                    continue;
                }

                bool stateAvailable = TryResolveConnectorCandidateState(
                    candidateIndex,
                    out int startAnchorIndex,
                    out int endAnchorIndex,
                    out Vector2[] basePoints,
                    out float[] baseCumulative,
                    out Vector2 startGate,
                    out Vector2 endGate);
                if (!stateAvailable)
                {
                    ReleaseConnectorSlotForRebind(
                        ref slot,
                        candidateIndex,
                        ConnectorReleaseReason.Unavailable,
                        false);
                    continue;
                }

                ConnectorRelationshipCandidate candidate =
                    connectorRelationshipCandidates[candidateIndex];
                bool referenceMatchesCurrentState =
                    slot.ReferenceLengthMetres > 0.0001f &&
                    slot.ReferenceCandidateIndex == candidateIndex &&
                    slot.ReferenceStartAnchorIndex == startAnchorIndex &&
                    slot.ReferenceEndAnchorIndex == endAnchorIndex;
                if (referenceMatchesCurrentState)
                {
                    bool measured = TryMeasureConnectorDeformedPath(
                        slot,
                        candidate,
                        basePoints,
                        baseCumulative,
                        startGate,
                        endGate,
                        out float currentLengthMetres);
                    float stretchLimit = slot.ReferenceLengthMetres *
                        river.FoamConnectorBreakStretchRatio;
                    if (!measured ||
                        currentLengthMetres > stretchLimit + 0.0001f)
                    {
                        if (measured)
                        {
                            RecordConnectorStretchBlock(
                                ref slot,
                                candidateIndex);
                            if (trackTransitions)
                            {
                                connectorEvolutionStretchBreakCount++;
                            }
                        }
                        ReleaseConnectorSlotForRebind(
                            ref slot,
                            candidateIndex,
                            measured
                                ? ConnectorReleaseReason.StretchBreak
                                : ConnectorReleaseReason.Unavailable,
                            false);
                        continue;
                    }
                }

                int startRecycleCount = majorEvolutionSlots[
                    candidate.StartHostSlotIndex].RecycleCount;
                int endRecycleCount = majorEvolutionSlots[
                    candidate.EndHostSlotIndex].RecycleCount;
                bool sameObservedRelationship = slot.HasRuntimeState &&
                    slot.ActiveCandidateIndex == candidateIndex &&
                    slot.ObservedStartRecycleCount >= 0 &&
                    slot.ObservedEndRecycleCount >= 0;
                bool hostRecycled = sameObservedRelationship &&
                    (slot.ObservedStartRecycleCount != startRecycleCount ||
                     slot.ObservedEndRecycleCount != endRecycleCount);
                if (hostRecycled)
                {
                    bool requestTurnover =
                        ShouldRequestConnectorRelationshipTurnover(
                            slot,
                            candidateIndex,
                            startRecycleCount,
                            endRecycleCount,
                            out bool crowdingBoostedTurnover);
                    if (requestTurnover)
                    {
                        if (trackTransitions)
                        {
                            connectorEvolutionTurnoverRequestCount++;
                            if (crowdingBoostedTurnover)
                            {
                                connectorEvolutionCrowdingBoostedTurnoverCount++;
                            }
                        }
                        ReleaseConnectorSlotForRebind(
                            ref slot,
                            candidateIndex,
                            ConnectorReleaseReason.Turnover,
                            true);
                        continue;
                    }

                    if (trackTransitions)
                    {
                        connectorEvolutionRetainDecisionCount++;
                    }
                }

                if (!TryClaimConnectorCandidate(candidateIndex))
                {
                    ReleaseConnectorSlotForRebind(
                        ref slot,
                        candidateIndex,
                        ConnectorReleaseReason.Unavailable,
                        false);
                }
            }
        }

        private void ApplyDirectedConnectorReplacementRequests()
        {
            // Turnover requests and stretch breaks explicitly exclude the old
            // Major pair. A requested turnover may retain its old relationship
            // only when no different prepared pair is currently available and
            // the old state remains valid. Stretch-broken relationships never
            // fall back to the path that just exceeded its limit.
            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ref ConnectorEvolutionSlot slot =
                    ref connectorEvolutionSlots[connectorIndex];
                bool turnoverRequested = slot.PendingReleaseReason ==
                    ConnectorReleaseReason.Turnover;
                bool stretchBroken = slot.PendingReleaseReason ==
                    ConnectorReleaseReason.StretchBreak;
                if (!turnoverRequested && !stretchBroken)
                {
                    continue;
                }

                int releasedCandidateIndex =
                    slot.TurnoverFallbackCandidateIndex >= 0
                        ? slot.TurnoverFallbackCandidateIndex
                        : slot.LastReleasedCandidateIndex;
                int selectedCandidate = SelectConnectorReplacementCandidate(
                    slot,
                    releasedCandidateIndex,
                    true);
                if (selectedCandidate >= 0)
                {
                    slot.AssignedCandidateIndex = selectedCandidate;
                    slot.RelationshipRevision++;
                    continue;
                }

                if (turnoverRequested && releasedCandidateIndex >= 0 &&
                    TryResolveConnectorCandidateState(
                        releasedCandidateIndex,
                        out _,
                        out _,
                        out _,
                        out _,
                        out _,
                        out _) &&
                    TryClaimConnectorCandidate(
                        releasedCandidateIndex))
                {
                    slot.AssignedCandidateIndex = releasedCandidateIndex;
                }
            }
        }

        private void AssignAvailableConnectorRelationships()
        {
            // Slots released because their previous state became unavailable,
            // plus slots that were already absent, may claim any currently
            // valid prepared relationship. Directed turnover/stretch releases
            // have already had their one bounded selection attempt above.
            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ref ConnectorEvolutionSlot slot =
                    ref connectorEvolutionSlots[connectorIndex];
                if (slot.AssignedCandidateIndex >= 0)
                {
                    continue;
                }
                if (slot.PendingReleaseReason ==
                        ConnectorReleaseReason.Turnover ||
                    slot.PendingReleaseReason ==
                        ConnectorReleaseReason.StretchBreak)
                {
                    continue;
                }

                int selectedCandidate =
                    SelectConnectorReplacementCandidate(
                        slot,
                        -1,
                        false);
                if (selectedCandidate >= 0)
                {
                    slot.AssignedCandidateIndex = selectedCandidate;
                    slot.RelationshipRevision++;
                }
            }
        }

        private void WriteConnectorEvolutionRuntimeState(bool trackTransitions)
        {
            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ref ConnectorEvolutionSlot slot =
                    ref connectorEvolutionSlots[connectorIndex];
                if (slot.ReleaseCooldownTicks > 0)
                {
                    slot.ReleaseCooldownTicks--;
                }

                bool previousActive = slot.IsActive;
                int previousCandidateIndex = slot.ActiveCandidateIndex;
                int previousStartAnchorIndex =
                    slot.ActiveStartAnchorIndex;
                int previousEndAnchorIndex = slot.ActiveEndAnchorIndex;

                int assignedCandidateIndex =
                    slot.AssignedCandidateIndex;
                int startAnchorIndex = -1;
                int endAnchorIndex = -1;
                Vector2[] basePoints = null;
                float[] baseCumulative = null;
                Vector2 startGate = Vector2.zero;
                Vector2 endGate = Vector2.zero;
                float currentLengthMetres = 0f;
                bool active = assignedCandidateIndex >= 0 &&
                    TryResolveConnectorCandidateState(
                        assignedCandidateIndex,
                        out startAnchorIndex,
                        out endAnchorIndex,
                        out basePoints,
                        out baseCumulative,
                        out startGate,
                        out endGate) &&
                    basePoints.Length <= slot.PointCapacity &&
                    WriteConnectorDeformedPath(
                        slot,
                        connectorRelationshipCandidates[
                            assignedCandidateIndex],
                        basePoints,
                        baseCumulative,
                        startGate,
                        endGate,
                        out currentLengthMetres);

                if (!active)
                {
                    startAnchorIndex = -1;
                    endAnchorIndex = -1;
                    if (assignedCandidateIndex >= 0)
                    {
                        ReleaseConnectorSlotForRebind(
                            ref slot,
                            assignedCandidateIndex,
                            ConnectorReleaseReason.Unavailable,
                            false);
                    }
                }
                else
                {
                    slot.PointCount = basePoints.Length;
                    bool referenceStateChanged =
                        slot.ReferenceLengthMetres <= 0.0001f ||
                        slot.ReferenceCandidateIndex !=
                            assignedCandidateIndex ||
                        slot.ReferenceStartAnchorIndex !=
                            startAnchorIndex ||
                        slot.ReferenceEndAnchorIndex != endAnchorIndex;
                    if (referenceStateChanged)
                    {
                        slot.ReferenceLengthMetres = currentLengthMetres;
                        slot.ReferenceCandidateIndex =
                            assignedCandidateIndex;
                        slot.ReferenceStartAnchorIndex = startAnchorIndex;
                        slot.ReferenceEndAnchorIndex = endAnchorIndex;
                    }
                }

                if (trackTransitions && active &&
                    slot.PendingReleaseReason ==
                        ConnectorReleaseReason.Turnover)
                {
                    if (assignedCandidateIndex ==
                        slot.TurnoverFallbackCandidateIndex)
                    {
                        connectorEvolutionNoAlternativeFallbackCount++;
                    }
                    else
                    {
                        connectorEvolutionSuccessfulTurnoverCount++;
                    }
                }

                FoamConnectorIdentityData record =
                    connectorIdentityGpuData[connectorIndex];
                record.PointRangeAndRadii.y = active
                    ? slot.PointCount
                    : 0f;
                connectorIdentityGpuData[connectorIndex] = record;

                if (trackTransitions && slot.HasRuntimeState)
                {
                    bool relationshipChanged = active &&
                        previousCandidateIndex >= 0 &&
                        previousCandidateIndex != assignedCandidateIndex;
                    bool anchorCombinationChanged = active &&
                        previousCandidateIndex == assignedCandidateIndex &&
                        (previousStartAnchorIndex != startAnchorIndex ||
                         previousEndAnchorIndex != endAnchorIndex);
                    if (relationshipChanged)
                    {
                        connectorEvolutionRelationshipRebindCount++;
                    }
                    if (anchorCombinationChanged)
                    {
                        connectorEvolutionVariantSwitchCount++;
                    }
                    if (previousActive && !active)
                    {
                        connectorEvolutionAbsenceEventCount++;
                    }
                    else if (!previousActive && active)
                    {
                        connectorEvolutionReappearanceCount++;
                    }
                }

                if (active)
                {
                    slot.ActiveCandidateIndex = assignedCandidateIndex;
                    slot.ActiveStartAnchorIndex = startAnchorIndex;
                    slot.ActiveEndAnchorIndex = endAnchorIndex;
                    ConnectorRelationshipCandidate activeCandidate =
                        connectorRelationshipCandidates[
                            assignedCandidateIndex];
                    slot.ObservedStartRecycleCount = majorEvolutionSlots[
                        activeCandidate.StartHostSlotIndex].RecycleCount;
                    slot.ObservedEndRecycleCount = majorEvolutionSlots[
                        activeCandidate.EndHostSlotIndex].RecycleCount;
                }
                slot.IsActive = active;
                slot.HasRuntimeState = true;

                if (active)
                {
                    connectorEvolutionActiveCount++;
                    bool usesRecycleVariant =
                        startAnchorIndex >= 0 || endAnchorIndex >= 0;
                    if (usesRecycleVariant)
                    {
                        connectorEvolutionRecycleVariantCount++;
                    }
                    else
                    {
                        connectorEvolutionIdentityPathCount++;
                    }

                    if (assignedCandidateIndex ==
                        slot.OriginalCandidateIndex)
                    {
                        connectorEvolutionOriginalRelationshipCount++;
                    }
                    else
                    {
                        connectorEvolutionReplacementRelationshipCount++;
                    }
                }
                else
                {
                    connectorEvolutionTemporaryAbsenceCount++;
                }
            }
        }

        private void UpdateWeakSpanEvolutionActiveCount()
        {
            if (!weakSpanIdentityReconstructionReady)
            {
                return;
            }

            for (int weakSpanIndex = 0;
                 weakSpanIndex < weakSpanIdentityGpuData.Length;
                 weakSpanIndex++)
            {
                int connectorIndex = Mathf.RoundToInt(
                    weakSpanIdentityGpuData[weakSpanIndex]
                        .ConnectorAndPath.x);
                if (connectorIndex >= 0 &&
                    connectorIndex < connectorEvolutionSlots.Length &&
                    connectorEvolutionSlots[connectorIndex].IsActive)
                {
                    weakSpanEvolutionActiveCount++;
                }
            }
        }

        private static float MeasureConnectorPreparedPathLength(
            IReadOnlyList<Vector2> points)
        {
            if (points == null || points.Count < 2)
            {
                return 0f;
            }

            float length = 0f;
            for (int pointIndex = 1;
                 pointIndex < points.Count;
                 pointIndex++)
            {
                length += Vector2.Distance(
                    points[pointIndex - 1],
                    points[pointIndex]);
            }
            return length;
        }

        private void ResolveConnectorRelationshipSelectionWeights(
            List<ConnectorRelationshipCandidate> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return;
            }

            float minimumLength = float.PositiveInfinity;
            float maximumLength = 0f;
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                float length = Mathf.Max(
                    0f,
                    candidates[candidateIndex].BasePathLengthMetres);
                minimumLength = Mathf.Min(minimumLength, length);
                maximumLength = Mathf.Max(maximumLength, length);
            }

            float centredLengthPreference = river != null
                ? (Mathf.Clamp01(river.FoamConnectorLengthPreference) -
                   0.5f) * 2f
                : 0f;
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                ConnectorRelationshipCandidate candidate =
                    candidates[candidateIndex];
                float normalizedLength = maximumLength >
                        minimumLength + 0.0001f
                    ? Mathf.InverseLerp(
                        minimumLength,
                        maximumLength,
                        candidate.BasePathLengthMetres)
                    : 0.5f;
                float lengthPreferenceWeight = Mathf.Exp(
                    centredLengthPreference *
                    (normalizedLength - 0.5f) *
                    ConnectorLengthPreferenceWeightStrength);
                float deterministicJitter = Mathf.Lerp(
                    ConnectorSelectionJitterMinimum,
                    ConnectorSelectionJitterMaximum,
                    HashConnectorIdentity(candidate.Path.StableId, 173u));
                candidate.SelectionWeight = Mathf.Max(
                    0.0001f,
                    lengthPreferenceWeight * deterministicJitter);
                candidates[candidateIndex] = candidate;
            }
        }

        private void UpdateConnectorDegreeTelemetry()
        {
            connectorEvolutionMajorDegreeZeroCount = 0;
            connectorEvolutionMajorDegreeOneCount = 0;
            connectorEvolutionMajorDegreeTwoCount = 0;
            connectorEvolutionMajorDegreeThreePlusCount = 0;
            connectorEvolutionMaximumMajorDegree = 0;

            for (int majorIndex = 0;
                 majorIndex < connectorMajorDegree.Length;
                 majorIndex++)
            {
                int degree = connectorMajorDegree[majorIndex];
                connectorEvolutionMaximumMajorDegree = Mathf.Max(
                    connectorEvolutionMaximumMajorDegree,
                    degree);
                if (degree <= 0)
                {
                    connectorEvolutionMajorDegreeZeroCount++;
                }
                else if (degree == 1)
                {
                    connectorEvolutionMajorDegreeOneCount++;
                }
                else if (degree == 2)
                {
                    connectorEvolutionMajorDegreeTwoCount++;
                }
                else
                {
                    connectorEvolutionMajorDegreeThreePlusCount++;
                }
            }
        }

        private void RefreshConnectorStretchBlock(
            ref ConnectorEvolutionSlot slot)
        {
            int candidateIndex = slot.StretchBlockedCandidateIndex;
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length)
            {
                ClearConnectorStretchBlock(ref slot);
                return;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            if (candidate.StartHostSlotIndex < 0 ||
                candidate.StartHostSlotIndex >= majorEvolutionSlots.Length ||
                candidate.EndHostSlotIndex < 0 ||
                candidate.EndHostSlotIndex >= majorEvolutionSlots.Length ||
                majorEvolutionSlots[candidate.StartHostSlotIndex]
                    .RecycleCount !=
                    slot.StretchBlockedStartRecycleCount ||
                majorEvolutionSlots[candidate.EndHostSlotIndex]
                    .RecycleCount !=
                    slot.StretchBlockedEndRecycleCount)
            {
                ClearConnectorStretchBlock(ref slot);
            }
        }

        private void RecordConnectorStretchBlock(
            ref ConnectorEvolutionSlot slot,
            int candidateIndex)
        {
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length)
            {
                ClearConnectorStretchBlock(ref slot);
                return;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            if (candidate.StartHostSlotIndex < 0 ||
                candidate.StartHostSlotIndex >= majorEvolutionSlots.Length ||
                candidate.EndHostSlotIndex < 0 ||
                candidate.EndHostSlotIndex >= majorEvolutionSlots.Length)
            {
                ClearConnectorStretchBlock(ref slot);
                return;
            }

            slot.StretchBlockedCandidateIndex = candidateIndex;
            slot.StretchBlockedStartRecycleCount = majorEvolutionSlots[
                candidate.StartHostSlotIndex].RecycleCount;
            slot.StretchBlockedEndRecycleCount = majorEvolutionSlots[
                candidate.EndHostSlotIndex].RecycleCount;
        }

        private static void ClearConnectorStretchBlock(
            ref ConnectorEvolutionSlot slot)
        {
            slot.StretchBlockedCandidateIndex = -1;
            slot.StretchBlockedStartRecycleCount = -1;
            slot.StretchBlockedEndRecycleCount = -1;
        }

        private static void ReleaseConnectorSlotForRebind(
            ref ConnectorEvolutionSlot slot,
            int candidateIndex,
            ConnectorReleaseReason releaseReason,
            bool allowTurnoverFallback)
        {
            if (candidateIndex >= 0)
            {
                slot.LastReleasedCandidateIndex = candidateIndex;
                slot.ReleaseCooldownTicks = 1;
            }
            slot.AssignedCandidateIndex = -1;
            slot.PendingReleaseReason = releaseReason;
            slot.TurnoverFallbackCandidateIndex =
                allowTurnoverFallback ? candidateIndex : -1;
            ClearConnectorReferenceLength(ref slot);
        }

        private static void ClearConnectorReferenceLength(
            ref ConnectorEvolutionSlot slot)
        {
            slot.ReferenceLengthMetres = 0f;
            slot.ReferenceCandidateIndex = -1;
            slot.ReferenceStartAnchorIndex = -2;
            slot.ReferenceEndAnchorIndex = -2;
        }

        private bool ShouldRequestConnectorRelationshipTurnover(
            ConnectorEvolutionSlot slot,
            int candidateIndex,
            int startRecycleCount,
            int endRecycleCount,
            out bool crowdingBoostedTurnover)
        {
            crowdingBoostedTurnover = false;
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length)
            {
                return false;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            int startDegree = candidate.StartHostSlotIndex >= 0 &&
                candidate.StartHostSlotIndex <
                    connectorPreviousMajorDegree.Length
                    ? connectorPreviousMajorDegree[
                        candidate.StartHostSlotIndex]
                    : 0;
            int endDegree = candidate.EndHostSlotIndex >= 0 &&
                candidate.EndHostSlotIndex <
                    connectorPreviousMajorDegree.Length
                    ? connectorPreviousMajorDegree[
                        candidate.EndHostSlotIndex]
                    : 0;
            int excessDegree = Mathf.Max(0, startDegree - 1) +
                Mathf.Max(0, endDegree - 1);
            float turnoverProbability = Mathf.Min(
                ConnectorTurnoverMaximumProbability,
                ConnectorTurnoverBaseProbability +
                excessDegree * ConnectorTurnoverCrowdingIncrement);

            uint stream;
            unchecked
            {
                stream = 151u ^
                    ((uint)(candidateIndex + 1) * 0x9E3779B9u) ^
                    ((uint)(startRecycleCount + 1) * 0x85EBCA6Bu) ^
                    ((uint)(endRecycleCount + 1) * 0xC2B2AE35u) ^
                    ((uint)(slot.RelationshipRevision + 1) * 0x27D4EB2Du);
            }
            float sample = HashConnectorIdentity(slot.StableId, stream);
            crowdingBoostedTurnover =
                sample >= ConnectorTurnoverBaseProbability &&
                sample < turnoverProbability;
            return sample < turnoverProbability;
        }

        private int SelectConnectorReplacementCandidate(
            ConnectorEvolutionSlot slot,
            int excludedCandidateIndex,
            bool excludeSameMajorPair)
        {
            int candidateCount = connectorRelationshipCandidates.Length;
            if (candidateCount <= 0)
            {
                return -1;
            }

            double totalWeight = 0.0;
            for (int candidateIndex = 0;
                 candidateIndex < candidateCount;
                 candidateIndex++)
            {
                if (!TryResolveConnectorSelectionWeight(
                        slot,
                        candidateIndex,
                        excludedCandidateIndex,
                        excludeSameMajorPair,
                        out float candidateWeight))
                {
                    continue;
                }

                totalWeight += candidateWeight;
            }

            if (totalWeight <= 0.0)
            {
                return -1;
            }

            uint stream;
            unchecked
            {
                stream = 211u ^
                    ((uint)(slot.RelationshipRevision + 1) * 0x9E3779B9u) ^
                    ((uint)(excludedCandidateIndex + 2) * 0x85EBCA6Bu) ^
                    (excludeSameMajorPair ? 0xC2B2AE35u : 0u);
            }
            double threshold = HashConnectorIdentity(
                slot.StableId,
                stream) * totalWeight;
            double cumulativeWeight = 0.0;
            int fallbackIndex = -1;
            for (int candidateIndex = 0;
                 candidateIndex < candidateCount;
                 candidateIndex++)
            {
                if (!TryResolveConnectorSelectionWeight(
                        slot,
                        candidateIndex,
                        excludedCandidateIndex,
                        excludeSameMajorPair,
                        out float candidateWeight))
                {
                    continue;
                }

                fallbackIndex = candidateIndex;
                cumulativeWeight += candidateWeight;
                if (threshold <= cumulativeWeight)
                {
                    return TryClaimConnectorCandidate(candidateIndex)
                        ? candidateIndex
                        : -1;
                }
            }

            return fallbackIndex >= 0 &&
                TryClaimConnectorCandidate(fallbackIndex)
                    ? fallbackIndex
                    : -1;
        }

        private bool TryResolveConnectorSelectionWeight(
            ConnectorEvolutionSlot slot,
            int candidateIndex,
            int excludedCandidateIndex,
            bool excludeSameMajorPair,
            out float weight)
        {
            weight = 0f;
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length ||
                candidateIndex == excludedCandidateIndex ||
                (slot.StretchBlockedCandidateIndex >= 0 &&
                 IsSameConnectorMajorPair(
                     candidateIndex,
                     slot.StretchBlockedCandidateIndex)) ||
                (excludeSameMajorPair &&
                 IsSameConnectorMajorPair(
                     candidateIndex,
                     excludedCandidateIndex)) ||
                (slot.ReleaseCooldownTicks > 0 &&
                 candidateIndex == slot.LastReleasedCandidateIndex) ||
                !CanClaimConnectorCandidate(candidateIndex) ||
                !TryResolveConnectorCandidateState(
                    candidateIndex,
                    out _,
                    out _,
                    out _,
                    out _,
                    out _,
                    out _))
            {
                return false;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            int startDegree = connectorMajorDegree[
                candidate.StartHostSlotIndex];
            int endDegree = connectorMajorDegree[
                candidate.EndHostSlotIndex];
            int combinedDegree = startDegree + endDegree;
            int maximumDegree = Mathf.Max(startDegree, endDegree);
            float loadWeight = Mathf.Pow(
                ConnectorLoadPenaltyBase,
                combinedDegree);
            float hubWeight = Mathf.Pow(
                ConnectorHubPenaltyBase,
                maximumDegree);
            weight = Mathf.Max(
                0.0000001f,
                candidate.SelectionWeight * loadWeight * hubWeight);
            return true;
        }

        private bool IsSameConnectorMajorPair(
            int firstCandidateIndex,
            int secondCandidateIndex)
        {
            if (firstCandidateIndex < 0 ||
                firstCandidateIndex >=
                    connectorRelationshipCandidates.Length ||
                secondCandidateIndex < 0 ||
                secondCandidateIndex >=
                    connectorRelationshipCandidates.Length)
            {
                return false;
            }

            ConnectorRelationshipCandidate first =
                connectorRelationshipCandidates[firstCandidateIndex];
            ConnectorRelationshipCandidate second =
                connectorRelationshipCandidates[secondCandidateIndex];
            int firstLow = Mathf.Min(
                first.StartHostSlotIndex,
                first.EndHostSlotIndex);
            int firstHigh = Mathf.Max(
                first.StartHostSlotIndex,
                first.EndHostSlotIndex);
            int secondLow = Mathf.Min(
                second.StartHostSlotIndex,
                second.EndHostSlotIndex);
            int secondHigh = Mathf.Max(
                second.StartHostSlotIndex,
                second.EndHostSlotIndex);
            return firstLow == secondLow && firstHigh == secondHigh;
        }

        private bool CanClaimConnectorCandidate(int candidateIndex)
        {
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length ||
                connectorCandidateClaimed[candidateIndex])
            {
                return false;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            int startHost = candidate.StartHostSlotIndex;
            int endHost = candidate.EndHostSlotIndex;
            if (startHost < 0 || startHost >= connectorMajorDegree.Length ||
                endHost < 0 || endHost >= connectorMajorDegree.Length ||
                startHost == endHost)
            {
                return false;
            }

            int lowHost = Mathf.Min(startHost, endHost);
            int highHost = Mathf.Max(startHost, endHost);
            int pairIndex = lowHost * connectorMajorDegree.Length + highHost;
            return pairIndex >= 0 &&
                pairIndex < connectorMajorPairClaimed.Length &&
                !connectorMajorPairClaimed[pairIndex];
        }

        private bool TryClaimConnectorCandidate(int candidateIndex)
        {
            if (!CanClaimConnectorCandidate(candidateIndex))
            {
                return false;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            int startHost = candidate.StartHostSlotIndex;
            int endHost = candidate.EndHostSlotIndex;
            int lowHost = Mathf.Min(startHost, endHost);
            int highHost = Mathf.Max(startHost, endHost);
            int pairIndex = lowHost * connectorMajorDegree.Length + highHost;

            connectorCandidateClaimed[candidateIndex] = true;
            connectorMajorPairClaimed[pairIndex] = true;
            connectorMajorDegree[startHost]++;
            connectorMajorDegree[endHost]++;
            return true;
        }

        private bool TryResolveConnectorCandidateState(
            int candidateIndex,
            out int startAnchorIndex,
            out int endAnchorIndex,
            out Vector2[] metricPoints,
            out float[] normalizedCumulativeLength,
            out Vector2 startGate,
            out Vector2 endGate)
        {
            startAnchorIndex = -1;
            endAnchorIndex = -1;
            metricPoints = null;
            normalizedCumulativeLength = null;
            startGate = Vector2.zero;
            endGate = Vector2.zero;
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length)
            {
                return false;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            if (candidate.Path == null ||
                candidate.StartHostSlotIndex < 0 ||
                candidate.StartHostSlotIndex >= majorEvolutionSlots.Length ||
                candidate.EndHostSlotIndex < 0 ||
                candidate.EndHostSlotIndex >= majorEvolutionSlots.Length)
            {
                return false;
            }

            startAnchorIndex = majorEvolutionSlots[
                candidate.StartHostSlotIndex].LastAnchorIndex;
            endAnchorIndex = majorEvolutionSlots[
                candidate.EndHostSlotIndex].LastAnchorIndex;
            return TryResolveConnectorBasePath(
                    candidate.Path,
                    startAnchorIndex,
                    endAnchorIndex,
                    out metricPoints,
                    out normalizedCumulativeLength) &&
                TryResolveConnectorEndpointGate(
                    candidate.Path.StartEndpointBinding,
                    candidate.StartHostSlotIndex,
                    out startGate) &&
                TryResolveConnectorEndpointGate(
                    candidate.Path.EndEndpointBinding,
                    candidate.EndHostSlotIndex,
                    out endGate);
        }

        private static bool TryResolveConnectorBasePath(
            StylizedRiverFoamConnectorPath path,
            int startAnchorIndex,
            int endAnchorIndex,
            out Vector2[] metricPoints,
            out float[] normalizedCumulativeLength)
        {
            if (path == null)
            {
                metricPoints = null;
                normalizedCumulativeLength = null;
                return false;
            }

            return path.TryResolvePreparedPath(
                startAnchorIndex,
                endAnchorIndex,
                out metricPoints,
                out normalizedCumulativeLength);
        }

        private bool TryResolveConnectorEndpointGate(
            StylizedRiverFoamConnectorEndpointBinding binding,
            int hostSlotIndex,
            out Vector2 metricPosition)
        {
            metricPosition = binding.AcceptedMetricPosition;
            if (!binding.IsAvailable || hostSlotIndex < 0 ||
                hostSlotIndex >= majorEvolutionSlots.Length ||
                majorTopology == null || river == null ||
                !river.Domain.IsValid)
            {
                return false;
            }

            MajorEvolutionSlot host = majorEvolutionSlots[hostSlotIndex];
            if (host.StableId != binding.MajorStableId ||
                host.PreparedIndex != binding.MajorPreparedIndex ||
                host.PreparedIndex < 0 ||
                host.PreparedIndex >= majorTopology.PreparedRegions.Count)
            {
                return false;
            }

            StylizedRiverFoamPreparedMajorRegion prepared =
                majorTopology.PreparedRegions[host.PreparedIndex];
            MajorEvolutionPose pose = ResolveMajorPose(host);
            Vector2 sourceOffset = ResolveConnectorPreWarpLocalOffset(
                binding.MajorLocalOffsetCells,
                pose,
                prepared);

            float principalMinorMetres = sourceOffset.y *
                pose.MetresPerCandidateCell * pose.ScaleAcross;
            float principalMajorMetres =
                (sourceOffset.x + sourceOffset.y * pose.Shear) *
                pose.MetresPerCandidateCell * pose.ScaleAlong;
            float orientationCosine = Mathf.Cos(pose.OrientationRadians);
            float orientationSine = Mathf.Sin(pose.OrientationRadians);
            float deltaAlong =
                orientationCosine * principalMajorMetres -
                orientationSine * principalMinorMetres;
            float deltaAcross =
                orientationSine * principalMajorMetres +
                orientationCosine * principalMinorMetres;
            float localDistance = pose.LocalDistance + deltaAlong;
            if (localDistance < 0f || localDistance > validFieldLength)
            {
                return false;
            }

            StylizedRiverSplineSample sample =
                river.Domain.SampleAtOrientedDistance(Mathf.Clamp(
                    localDistance,
                    0f,
                    river.Domain.LocalLength));
            float centreAcrossMetres =
                StylizedRiverFoamTopologyFieldSpace.SignedNormalizedToMetres(
                    pose.AcrossNormalized,
                    Mathf.Max(0.05f, sample.LeftHalfWidth),
                    Mathf.Max(0.05f, sample.RightHalfWidth));
            metricPosition = new Vector2(
                localDistance,
                centreAcrossMetres + deltaAcross);
            return true;
        }

        private static Vector2 ResolveConnectorPreWarpLocalOffset(
            Vector2 desiredLocalOffset,
            MajorEvolutionPose pose,
            StylizedRiverFoamPreparedMajorRegion prepared)
        {
            Vector2 source = desiredLocalOffset;
            float majorExtent = Mathf.Max(
                0.5f,
                prepared.MajorHalfExtentCells);
            float minorExtent = Mathf.Max(
                0.5f,
                prepared.MinorHalfExtentCells);
            for (int iteration = 0;
                 iteration < ConnectorEndpointWarpSolveIterations;
                 iteration++)
            {
                float normalMajor = source.x / majorExtent;
                float normalMinor = source.y / minorExtent;
                float majorWarp =
                    Mathf.Sin(normalMinor * 3.35f + pose.WarpPhaseA) *
                        pose.WarpAlong * majorExtent +
                    Mathf.Sin(
                        (normalMajor + normalMinor) * 1.85f +
                        pose.WarpPhaseB) *
                        pose.WarpAlong * majorExtent * 0.42f;
                float minorWarp =
                    Mathf.Sin(normalMajor * 2.80f + pose.WarpPhaseB) *
                        pose.WarpAcross * minorExtent +
                    Mathf.Sin(
                        (normalMajor - normalMinor) * 2.10f +
                        pose.WarpPhaseA) *
                        pose.WarpAcross * minorExtent * 0.36f;
                source = desiredLocalOffset -
                    new Vector2(majorWarp, minorWarp);
            }

            return source;
        }

        private bool WriteConnectorDeformedPath(
            ConnectorEvolutionSlot slot,
            ConnectorRelationshipCandidate candidate,
            Vector2[] basePoints,
            float[] baseCumulative,
            Vector2 startGate,
            Vector2 endGate,
            out float pathLengthMetres)
        {
            return EvaluateConnectorDeformedPath(
                slot,
                candidate,
                basePoints,
                baseCumulative,
                startGate,
                endGate,
                true,
                out pathLengthMetres);
        }

        private bool TryMeasureConnectorDeformedPath(
            ConnectorEvolutionSlot slot,
            ConnectorRelationshipCandidate candidate,
            Vector2[] basePoints,
            float[] baseCumulative,
            Vector2 startGate,
            Vector2 endGate,
            out float pathLengthMetres)
        {
            return EvaluateConnectorDeformedPath(
                slot,
                candidate,
                basePoints,
                baseCumulative,
                startGate,
                endGate,
                false,
                out pathLengthMetres);
        }

        private bool EvaluateConnectorDeformedPath(
            ConnectorEvolutionSlot slot,
            ConnectorRelationshipCandidate candidate,
            Vector2[] basePoints,
            float[] baseCumulative,
            Vector2 startGate,
            Vector2 endGate,
            bool writePoints,
            out float pathLengthMetres)
        {
            pathLengthMetres = 0f;
            int pointCount = basePoints != null
                ? basePoints.Length
                : 0;
            if (basePoints == null || baseCumulative == null ||
                baseCumulative.Length != pointCount ||
                pointCount < 2 || pointCount > slot.PointCapacity ||
                slot.PointOffset < 0 ||
                slot.PointOffset + pointCount >
                    connectorPathPointGpuData.Length ||
                candidate.StartHostSlotIndex < 0 ||
                candidate.StartHostSlotIndex >= majorEvolutionSlots.Length ||
                candidate.EndHostSlotIndex < 0 ||
                candidate.EndHostSlotIndex >= majorEvolutionSlots.Length)
            {
                return false;
            }

            MajorEvolutionSlot startHost =
                majorEvolutionSlots[candidate.StartHostSlotIndex];
            MajorEvolutionSlot endHost =
                majorEvolutionSlots[candidate.EndHostSlotIndex];
            MajorEvolutionPose startPose = ResolveMajorPose(startHost);
            MajorEvolutionPose endPose = ResolveMajorPose(endHost);
            Vector2 startDelta = startGate - basePoints[0];
            Vector2 endDelta = endGate -
                basePoints[basePoints.Length - 1];
            float endpointMotion =
                (startDelta.magnitude + endDelta.magnitude) * 0.5f;
            float deformationActivity = Mathf.Clamp01(
                endpointMotion / 0.65f);
            if (startHost.IsMoving || endHost.IsMoving)
            {
                deformationActivity = Mathf.Max(
                    deformationActivity,
                    0.30f);
            }

            float amplitude = Mathf.Lerp(
                ConnectorMinimumInteriorDeformationMetres,
                ConnectorMaximumInteriorDeformationMetres,
                HashConnectorIdentity(slot.StableId, 41u)) *
                deformationActivity;
            float frequency = Mathf.Lerp(
                1.20f,
                2.25f,
                HashConnectorIdentity(slot.StableId, 42u));
            float phase =
                HashConnectorIdentity(slot.StableId, 43u) *
                    Mathf.PI * 2f +
                (startPose.LocalDistance + endPose.LocalDistance) * 0.31f +
                (startPose.AcrossNormalized -
                    endPose.AcrossNormalized) * 1.70f +
                (startHost.HopIndex + endHost.HopIndex) * 0.37f +
                (startHost.RecycleCount + endHost.RecycleCount) * 0.83f;

            float cumulativeLength = 0f;
            Vector2 previousPosition = startGate;
            for (int pointIndex = 0;
                 pointIndex < pointCount;
                 pointIndex++)
            {
                float pathFraction = Mathf.Clamp01(
                    baseCumulative[pointIndex]);
                Vector2 endpointWarp = Vector2.Lerp(
                    startDelta,
                    endDelta,
                    pathFraction);
                Vector2 position = basePoints[pointIndex] + endpointWarp;

                if (pointIndex > 0 &&
                    pointIndex < pointCount - 1 &&
                    amplitude > 0.0001f)
                {
                    int previousIndex = Mathf.Max(0, pointIndex - 1);
                    int nextIndex = Mathf.Min(
                        pointCount - 1,
                        pointIndex + 1);
                    Vector2 previousReference =
                        basePoints[previousIndex] + Vector2.Lerp(
                            startDelta,
                            endDelta,
                            Mathf.Clamp01(
                                baseCumulative[previousIndex]));
                    Vector2 nextReference =
                        basePoints[nextIndex] + Vector2.Lerp(
                            startDelta,
                            endDelta,
                            Mathf.Clamp01(baseCumulative[nextIndex]));
                    Vector2 tangent = nextReference - previousReference;
                    if (tangent.sqrMagnitude > 0.000001f)
                    {
                        tangent.Normalize();
                        Vector2 normal = new Vector2(
                            -tangent.y,
                            tangent.x);
                        float envelope = Mathf.Sin(
                            pathFraction * Mathf.PI);
                        envelope *= envelope;
                        float wave = Mathf.Sin(
                            pathFraction * frequency *
                                Mathf.PI * 2f + phase);
                        position += normal *
                            (amplitude * envelope * wave);
                    }
                }

                if (pointIndex == 0)
                {
                    position = startGate;
                }
                else if (pointIndex == pointCount - 1)
                {
                    position = endGate;
                }

                if (pointIndex > 0)
                {
                    cumulativeLength += Vector2.Distance(
                        previousPosition,
                        position);
                }
                if (writePoints)
                {
                    connectorPathPointGpuData[
                        slot.PointOffset + pointIndex] = new Vector4(
                            position.x,
                            position.y,
                            cumulativeLength,
                            0f);
                }
                previousPosition = position;
            }

            if (cumulativeLength <= 0.0001f)
            {
                return false;
            }

            pathLengthMetres = cumulativeLength;
            if (!writePoints)
            {
                return true;
            }

            float inverseLength = 1f / cumulativeLength;
            for (int pointIndex = 0;
                 pointIndex < pointCount;
                 pointIndex++)
            {
                int flattenedIndex = slot.PointOffset + pointIndex;
                Vector4 point = connectorPathPointGpuData[flattenedIndex];
                point.z = Mathf.Clamp01(point.z * inverseLength);
                connectorPathPointGpuData[flattenedIndex] = point;
            }

            return true;
        }

        private void EnsureConnectorIdentityBuffers()
        {
            if (connectorIdentityBuffer == null)
            {
                connectorIdentityBuffer = new ComputeBuffer(
                    Mathf.Max(1, connectorIdentityGpuData.Length),
                    sizeof(float) * 4,
                    ComputeBufferType.Structured);
                if (connectorIdentityGpuData.Length == 0)
                {
                    connectorIdentityBuffer.SetData(
                        new FoamConnectorIdentityData[1]);
                }
            }
            if (connectorPathPointBuffer == null)
            {
                connectorPathPointBuffer = new ComputeBuffer(
                    Mathf.Max(1, connectorPathPointGpuData.Length),
                    sizeof(float) * 4,
                    ComputeBufferType.Structured);
                if (connectorPathPointGpuData.Length == 0)
                {
                    connectorPathPointBuffer.SetData(new Vector4[1]);
                }
            }
            if (weakSpanIdentityBuffer == null)
            {
                weakSpanIdentityBuffer = new ComputeBuffer(
                    Mathf.Max(1, weakSpanIdentityGpuData.Length),
                    sizeof(float) * 12,
                    ComputeBufferType.Structured);
                if (weakSpanIdentityGpuData.Length == 0)
                {
                    weakSpanIdentityBuffer.SetData(
                        new FoamWeakSpanIdentityData[1]);
                }
            }
        }

        private void ReleaseConnectorIdentityReconstructionResources()
        {
            connectorIdentityBuffer?.Release();
            connectorIdentityBuffer = null;
            connectorPathPointBuffer?.Release();
            connectorPathPointBuffer = null;
            weakSpanIdentityBuffer?.Release();
            weakSpanIdentityBuffer = null;
            connectorIdentityGpuData =
                Array.Empty<FoamConnectorIdentityData>();
            connectorPathPointGpuData = Array.Empty<Vector4>();
            weakSpanIdentityGpuData =
                Array.Empty<FoamWeakSpanIdentityData>();
            connectorEvolutionSlots = Array.Empty<ConnectorEvolutionSlot>();
            connectorRelationshipCandidates =
                Array.Empty<ConnectorRelationshipCandidate>();
            connectorCandidateClaimed = Array.Empty<bool>();
            connectorMajorPairClaimed = Array.Empty<bool>();
            connectorMajorDegree = Array.Empty<int>();
            connectorPreviousMajorDegree = Array.Empty<int>();
            connectorEvolutionActiveCount = 0;
            connectorEvolutionTemporaryAbsenceCount = 0;
            connectorEvolutionIdentityPathCount = 0;
            connectorEvolutionRecycleVariantCount = 0;
            connectorEvolutionOriginalRelationshipCount = 0;
            connectorEvolutionReplacementRelationshipCount = 0;
            connectorEvolutionRelationshipRebindCount = 0;
            connectorEvolutionVariantSwitchCount = 0;
            connectorEvolutionStretchBreakCount = 0;
            connectorEvolutionRetainDecisionCount = 0;
            connectorEvolutionTurnoverRequestCount = 0;
            connectorEvolutionSuccessfulTurnoverCount = 0;
            connectorEvolutionNoAlternativeFallbackCount = 0;
            connectorEvolutionCrowdingBoostedTurnoverCount = 0;
            connectorEvolutionMajorDegreeZeroCount = 0;
            connectorEvolutionMajorDegreeOneCount = 0;
            connectorEvolutionMajorDegreeTwoCount = 0;
            connectorEvolutionMajorDegreeThreePlusCount = 0;
            connectorEvolutionMaximumMajorDegree = 0;
            connectorEvolutionAbsenceEventCount = 0;
            connectorEvolutionReappearanceCount = 0;
            weakSpanEvolutionActiveCount = 0;
            connectorIdentityReconstructionReady = false;
            weakSpanIdentityReconstructionReady = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            connectorIdentityParityGeneration++;
            connectorIdentityParityPending = false;
            connectorIdentityParityReadbackPending = false;
            connectorIdentityParityAvailable = false;
            connectorIdentityParityMeanDifference = 0f;
            connectorIdentityParityMaximumDifference = 0f;
            weakSpanIdentityParityGeneration++;
            weakSpanIdentityParityPending = false;
            weakSpanIdentityParityReadbackPending = false;
            weakSpanIdentityParityAvailable = false;
            weakSpanIdentityParityMeanDifference = 0f;
            weakSpanIdentityParityMaximumDifference = 0f;
#endif
        }

        private void RequestConnectorIdentityParityIfNeeded()
        {
            // Identity parity is development-only proof that the retained
            // Connector path records reconstruct the accepted static field.
            if (!connectorIdentityParityPending ||
                connectorIdentityParityReadbackPending ||
                !IsTopologyDebugActive ||
                !SystemInfo.supportsAsyncGPUReadback ||
                evolvingConnectorTexture == null ||
                connectorTopology == null)
            {
                return;
            }

            connectorIdentityParityPending = false;
            connectorIdentityParityReadbackPending = true;
            int generation = connectorIdentityParityGeneration;
            StylizedRiverFoamConnectorTopology requestedTopology =
                connectorTopology;
            AsyncGPUReadback.Request(
                evolvingConnectorTexture,
                0,
                TextureFormat.RFloat,
                request =>
                {
                    if (this == null ||
                        generation != connectorIdentityParityGeneration ||
                        requestedTopology != connectorTopology)
                    {
                        return;
                    }

                    connectorIdentityParityReadbackPending = false;
                    if (request.hasError)
                    {
                        connectorIdentityParityAvailable = false;
                        return;
                    }

                    var data = request.GetData<float>();
                    float[] expected = requestedTopology.SupportData;
                    int count = Mathf.Min(data.Length, expected.Length);
                    if (count <= 0)
                    {
                        connectorIdentityParityAvailable = false;
                        return;
                    }

                    double totalDifference = 0.0;
                    int relevantCount = 0;
                    float maximumDifference = 0f;
                    for (int index = 0; index < count; index++)
                    {
                        float reconstructed = Mathf.Clamp01(data[index]);
                        float expectedValue = Mathf.Clamp01(expected[index]);
                        float difference = Mathf.Abs(
                            reconstructed - expectedValue);
                        if (Mathf.Max(reconstructed, expectedValue) > 0.01f)
                        {
                            totalDifference += difference;
                            relevantCount++;
                        }
                        maximumDifference = Mathf.Max(
                            maximumDifference,
                            difference);
                    }

                    connectorIdentityParityMeanDifference =
                        relevantCount > 0
                            ? (float)(totalDifference / relevantCount)
                            : 0f;
                    connectorIdentityParityMaximumDifference =
                        maximumDifference;
                    connectorIdentityParityAvailable = true;
                });
        }
    }
}
