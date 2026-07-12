using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear corrected-clone diagnostics

        private static void RunChamferDiagnosticHarness(
            ChamferBuildArtifacts artifacts,
            ref ChamferEmissionStats stats)
        {
            AuditChamferAuthoritativeSectorRepartitionAndCorrectedTopology(
                artifacts.Plan,
                artifacts.FailedSliverLoopIndices,
                artifacts.PrePatchFaceRecords,
                artifacts.SuccessfulPatchRecords,
                artifacts.Context,
                artifacts.NormalizedVertexBoundaries,
                artifacts.SourceBoundaryRecords,
                artifacts.MinimumStableEdgeLength,
                artifacts.MinimumStableFaceArea,
                artifacts.MinimumPatchTriangleArea,
                artifacts.SharedSpans,
                artifacts.ContainedPatchCandidates,
                ref stats);
        }

        private static void
            AuditChamferAuthoritativeSectorRepartitionAndCorrectedTopology(
                ChamferVertexPatchPlan plan,
                HashSet<int> failedSliverLoopIndices,
                List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
                List<ChamferProvisionalFaceRecord> successfulPatchRecords,
                ChamferTopologyContext context,
                List<ChamferExpectedVertexBoundary> normalizedVertexBoundaries,
                List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                float minimumPatchTriangleArea,
                Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
                List<ChamferContainedPatchCandidate>
                    containedPatchCandidates,
                ref ChamferEmissionStats stats)
        {
            bool hasClusterPlans = false;
            for (int i = 0; i < plan.PatchLoops.Count; i++)
            {
                if (plan.PatchLoops[i].Kind ==
                    ChamferVertexPatchLoopKind.ClosedSourceCluster)
                {
                    hasClusterPlans = true;
                    break;
                }
            }

            AuditChamferSuccessfulPatchIntersectionBaseline(
                successfulPatchRecords,
                prePatchFaceRecords,
                minimumStableEdgeLength,
                minimumPatchTriangleArea,
                ref stats);
            AuditChamferContainedPatchOwnershipTransfer(
                plan,
                prePatchFaceRecords,
                successfulPatchRecords,
                containedPatchCandidates,
                minimumStableEdgeLength,
                ref stats);
            AuditChamferContainedOwnerRepartition(
                plan,
                prePatchFaceRecords,
                successfulPatchRecords,
                containedPatchCandidates,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                minimumPatchTriangleArea,
                ref stats);

            if (!TryBuildChamferCorrectedPrePatchClone(
                    plan,
                    failedSliverLoopIndices,
                    prePatchFaceRecords,
                    context,
                    normalizedVertexBoundaries,
                    sourceBoundaryRecords,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    sharedSpans,
                    out List<ChamferProvisionalFaceRecord> correctedRecords,
                    out HashSet<TopologyEdgeKey> correctedSourceBoundaryEdges,
                    out int sliverCollapses,
                    out int cloneTopologyFailures,
                    out string cloneFailure))
            {
                stats.PatchCorrectedLoopsAttempted += plan.PatchLoops.Count;
                stats.ReadyForCorrectedChamferPatchTopology = 0;
                return;
            }

            if (!TryAppendChamferReservedSliverPatches(
                    plan,
                    failedSliverLoopIndices,
                    prePatchFaceRecords,
                    correctedRecords,
                    context,
                    normalizedVertexBoundaries,
                    sourceBoundaryRecords,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    minimumPatchTriangleArea,
                    sharedSpans,
                    out List<ChamferReservedSliverPatch> reservedSlivers,
                    out int reservedOccurrenceConflicts,
                    out string reservedSliverFailure))
            {
                cloneTopologyFailures++;
                cloneFailure = string.IsNullOrEmpty(cloneFailure)
                    ? reservedSliverFailure
                    : cloneFailure + "|" + reservedSliverFailure;
            }
            stats.PatchCorrectedReservedSliverLoops +=
                reservedSlivers.Count;
            stats.PatchCorrectedReservedSliverTriangles +=
                reservedSlivers.Count;

            HashSet<int> reservedLoopIndices = new HashSet<int>();
            for (int i = 0; i < reservedSlivers.Count; i++)
            {
                reservedLoopIndices.Add(reservedSlivers[i].LoopIndex);
            }
            List<ChamferVertexPatchLoop> activeLegacyPlans =
                new List<ChamferVertexPatchLoop>();
            for (int i = 0; i < plan.PatchLoops.Count; i++)
            {
                if (!reservedLoopIndices.Contains(
                        plan.PatchLoops[i].LoopIndex))
                {
                    activeLegacyPlans.Add(plan.PatchLoops[i]);
                }
            }

            ChamferHalfEdgeDecomposition sectorDecomposition =
                BuildChamferAuthoritativeHalfEdgeDecomposition(
                    correctedRecords,
                    true,
                    true);
            List<ChamferHalfEdgeBoundaryComponent> patchComponents =
                new List<ChamferHalfEdgeBoundaryComponent>();
            int mixedSourceBoundaryComponents = 0;
            for (int componentIndex = 0;
                 componentIndex < sectorDecomposition.Components.Count;
                 componentIndex++)
            {
                ChamferHalfEdgeBoundaryComponent component =
                    sectorDecomposition.Components[componentIndex];
                HashSet<TopologyEdgeKey> keys =
                    BuildChamferHalfEdgeComponentKeySet(
                        component,
                        sectorDecomposition.HalfEdges);
                int sourceKeyCount = 0;
                foreach (TopologyEdgeKey key in keys)
                {
                    if (correctedSourceBoundaryEdges.Contains(key))
                    {
                        sourceKeyCount++;
                    }
                }
                if (sourceKeyCount == keys.Count && keys.Count > 0)
                {
                    continue;
                }
                if (sourceKeyCount > 0)
                {
                    mixedSourceBoundaryComponents++;
                }
                patchComponents.Add(component);
            }

            List<ChamferSectorPlanAssignment> assignments =
                BuildChamferSectorPlanAssignments(
                    activeLegacyPlans,
                    patchComponents,
                    sectorDecomposition);
            ChamferSliverComponentDelta sliverComponentDelta =
                AuditChamferReservedSliverComponentDelta(
                    plan,
                    failedSliverLoopIndices,
                    prePatchFaceRecords,
                    patchComponents,
                    sectorDecomposition.HalfEdges,
                    reservedSlivers);
            if (reservedSlivers.Count > 0)
            {
                stats.PatchSliverDeltaPreCollapseComponents +=
                    sliverComponentDelta.PreCollapseComponents;
                stats.PatchSliverDeltaPostCollapseComponents +=
                    sliverComponentDelta.PostCollapseComponents;
                stats.PatchSliverDeltaReservedPreComponents +=
                    sliverComponentDelta.ReservedPreCollapseComponents;
                stats.PatchSliverDeltaExactComponentMatches +=
                    sliverComponentDelta.ExactComponentMatches;
                stats.PatchSliverDeltaDisappearedComponents +=
                    sliverComponentDelta.DisappearedPreComponents;
                stats.PatchSliverDeltaMergedPostComponents +=
                    sliverComponentDelta.MergedPostComponents;
                stats.PatchSliverDeltaSplitPreComponents +=
                    sliverComponentDelta.SplitPreComponents;
                stats.PatchSliverDeltaMissingLoopCount +=
                    sliverComponentDelta.MissingLoopCount;
                stats.PatchSliverDeltaDiagnostic =
                    sliverComponentDelta.Diagnostic;
            }
            for (int i = 0; i < assignments.Count; i++)
            {
                ChamferSectorPlanAssignment assignment = assignments[i];
                if (assignment.PlanListIndex < 0 ||
                    assignment.ComponentListIndex < 0)
                {
                    continue;
                }
            }

            HashSet<int> promotedSectorHalfEdges =
                BuildChamferPromotedSectorHalfEdgeIndices(
                    sectorDecomposition);
            AuditChamferSectorOccurrenceOwnership(
                activeLegacyPlans,
                patchComponents,
                sectorDecomposition.HalfEdges,
                promotedSectorHalfEdges,
                out int boundaryHalfEdges,
                out int legacyAssignedBoundaryHalfEdges,
                out int promotedBoundaryHalfEdges,
                out int boundaryHalfEdgesAssigned,
                out int boundaryHalfEdgesUnassigned,
                out int boundaryHalfEdgesMultiAssigned,
                out int plansContributingToMultipleSectorLoops,
                out int sectorLoopsCombiningMultiplePlans,
                out int plansWithoutOccurrences,
                out int sectorLoopsWithoutOccurrences);
            int provenanceFailures =
                boundaryHalfEdgesUnassigned +
                boundaryHalfEdgesMultiAssigned +
                plansWithoutOccurrences +
                sectorLoopsWithoutOccurrences +
                mixedSourceBoundaryComponents;

            bool loopCountInvariant =
                patchComponents.Count + reservedSlivers.Count ==
                    plan.PatchLoops.Count;
            bool sectorOwnershipClean =
                sectorDecomposition.OpenChainCount == 0 &&
                sectorDecomposition.SuccessorFailureCount == 0 &&
                sectorDecomposition.UnassignedBoundaryEdgeCount == 0 &&
                sectorDecomposition.MultiAssignedBoundaryEdgeCount == 0 &&
                sectorDecomposition.InternalDirectionFailureCount == 0 &&
                boundaryHalfEdgesUnassigned == 0 &&
                boundaryHalfEdgesMultiAssigned == 0 &&
                mixedSourceBoundaryComponents == 0;

            if (hasClusterPlans)
            {
                stats.PatchSectorExistingPlanLoops +=
                    plan.PatchLoops.Count;
                stats.PatchSectorAuthoritativeLoops +=
                    patchComponents.Count + reservedSlivers.Count;
                stats.PatchSectorBoundaryHalfEdges += boundaryHalfEdges;
                stats.PatchSectorBoundaryHalfEdgesAssigned +=
                    boundaryHalfEdgesAssigned;
            }

            int loopsAttempted = reservedSlivers.Count;
            int loopsBuilt = reservedSlivers.Count;
            int loopsFailed = 0;
            int intersectionFailures = 0;
            int occurrenceBoundaryFailures =
                reservedOccurrenceConflicts;
            List<ChamferDirectedTriangleGeometry> acceptedPatchTriangles =
                new List<ChamferDirectedTriangleGeometry>();
            List<ChamferBoundarySegment> acceptedPatchBoundarySegments =
                new List<ChamferBoundarySegment>();
            for (int i = 0; i < reservedSlivers.Count; i++)
            {
                acceptedPatchTriangles.Add(reservedSlivers[i].Geometry);
                acceptedPatchBoundarySegments.AddRange(
                    reservedSlivers[i].BoundarySegments);
            }
            List<ChamferProvisionalFaceRecord> correctedPrePatchRecords =
                BuildChamferNonPatchFaceRecordSnapshot(correctedRecords);
            ChamferFaceIntersectionCache correctedIntersectionCache =
                BuildChamferFaceIntersectionCache(
                    correctedPrePatchRecords,
                    minimumPatchTriangleArea,
                    ref stats);

            for (int componentListIndex = 0;
                 componentListIndex < patchComponents.Count;
                 componentListIndex++)
            {
                loopsAttempted++;
                ChamferHalfEdgeBoundaryComponent component =
                    patchComponents[componentListIndex];
                int boundaryCount =
                    component.OrderedHalfEdgeIndices.Count;
                if (component.Kind !=
                        ChamferHalfEdgeBoundaryComponentKind.Loop ||
                    boundaryCount < 3)
                {
                    loopsFailed++;
                    occurrenceBoundaryFailures++;
                    continue;
                }

                List<ChamferProvisionalFaceRecord> loopRecords;
                List<ChamferDirectedTriangleGeometry> candidateTriangles;
                if (boundaryCount == 3)
                {
                    if (!TryBuildChamferDirectPatchFromBoundaryComponent(
                            component,
                            sectorDecomposition.HalfEdges,
                            correctedRecords,
                            minimumPatchTriangleArea,
                            out ChamferProvisionalFaceRecord directRecord,
                            out ChamferDirectedTriangleGeometry directGeometry,
                            out _))
                    {
                        loopsFailed++;
                        continue;
                    }
                    loopRecords = new List<ChamferProvisionalFaceRecord>
                    {
                        directRecord
                    };
                    candidateTriangles =
                        new List<ChamferDirectedTriangleGeometry>
                        {
                            directGeometry
                        };
                }
                else
                {
                    List<Vector3> positions =
                        BuildChamferOppositeBoundaryPositions(
                            component,
                            sectorDecomposition.HalfEdges);
                    List<TopologyEdgeKey> orderedKeys =
                        BuildChamferOrderedBoundaryKeys(positions);
                    HashSet<TopologyEdgeKey> boundaryKeySet =
                        new HashSet<TopologyEdgeKey>(orderedKeys);
                    if (orderedKeys.Count != positions.Count ||
                        boundaryKeySet.Count != positions.Count)
                    {
                        loopsFailed++;
                        occurrenceBoundaryFailures++;
                        continue;
                    }

                    Vector3 expectedNormal =
                        CalculatePolygonNormal(positions);
                    if (!IsFinite(expectedNormal) ||
                        expectedNormal.sqrMagnitude <=
                            MinimumEdgeLengthSqr)
                    {
                        loopsFailed++;
                        continue;
                    }
                    expectedNormal.Normalize();
                    ChamferVertexPatchLoop correctedLoop =
                        new ChamferVertexPatchLoop(
                            component.ComponentIndex,
                            ChamferVertexPatchLoopKind.
                                LocalClosedComponent,
                            positions,
                            orderedKeys,
                            new List<ChamferVertexPatchComponent>(),
                            new List<Vector3>(),
                            expectedNormal,
                            CalculateChamferSectorFeatureStrength(
                                component,
                                sectorDecomposition.HalfEdges,
                                correctedRecords));

                    ChamferEmissionStats diagnosticStats = default;
                    if (!TryTriangulateChamferVertexPatchLoop(
                            correctedLoop,
                            new List<Vector3>(positions),
                            boundaryKeySet,
                            minimumPatchTriangleArea,
                            ref diagnosticStats,
                            out loopRecords,
                            out _,
                            out _,
                            out _))
                    {
                        loopsFailed++;
                        continue;
                    }

                    if (!TryBuildChamferCorrectedPatchTriangleGeometry(
                            loopRecords,
                            minimumPatchTriangleArea,
                            out candidateTriangles))
                    {
                        loopsFailed++;
                        continue;
                    }
                }

                List<ChamferBoundarySegment> loopBoundarySegments =
                    BuildChamferPatchBoundarySegments(loopRecords);
                int boundaryOccurrenceFailuresForLoop =
                    CountChamferCorrectedPatchBoundaryOccurrenceFailures(
                        loopRecords,
                        component,
                        sectorDecomposition.HalfEdges,
                        out ChamferBoundaryOccurrenceAudit
                            boundaryOccurrenceAudit);
                stats.PatchCorrectedBoundaryMissingOpposite +=
                    boundaryOccurrenceAudit.MissingOpposite;
                stats.PatchCorrectedBoundaryDuplicateOpposite +=
                    boundaryOccurrenceAudit.DuplicateOpposite;
                stats.PatchCorrectedBoundaryDirectionMismatch +=
                    boundaryOccurrenceAudit.DirectionMismatch;
                stats.PatchCorrectedBoundaryExtraPatchEdge +=
                    boundaryOccurrenceAudit.ExtraPatchBoundary;
                if (boundaryOccurrenceFailuresForLoop > 0)
                {
                    AppendChamferCompactDiagnostic(
                        ref stats.PatchCorrectedBoundaryOccurrenceDiagnostic,
                        "c" + component.ComponentIndex + ":" +
                        boundaryOccurrenceAudit.MissingOpposite + "/" +
                        boundaryOccurrenceAudit.DuplicateOpposite + "/" +
                        boundaryOccurrenceAudit.DirectionMismatch + "/" +
                        boundaryOccurrenceAudit.ExtraPatchBoundary,
                        4);
                    loopsFailed++;
                    occurrenceBoundaryFailures +=
                        boundaryOccurrenceFailuresForLoop;
                    continue;
                }

                int improperIntersections =
                    AuditChamferCorrectedPatchIntersections(
                        candidateTriangles,
                        loopBoundarySegments,
                        acceptedPatchTriangles,
                        acceptedPatchBoundarySegments,
                        correctedIntersectionCache,
                        minimumStableEdgeLength,
                        minimumPatchTriangleArea,
                        false,
                        component.ComponentIndex,
                        ref stats);
                if (improperIntersections > 0)
                {
                    loopsFailed++;
                    intersectionFailures += improperIntersections;
                    continue;
                }

                correctedRecords.AddRange(loopRecords);
                acceptedPatchTriangles.AddRange(candidateTriangles);
                acceptedPatchBoundarySegments.AddRange(
                    loopBoundarySegments);
                loopsBuilt++;
                if (loopRecords.Count !=
                    Mathf.Max(1, boundaryCount - 2))
                {
                    occurrenceBoundaryFailures++;
                }
            }

            stats.PatchCorrectedLoopsAttempted += loopsAttempted;
            stats.PatchCorrectedLoopsBuilt += loopsBuilt;


            List<PolygonFace> correctedFinalFaces =
                ExtractChamferProvisionalFaces(correctedRecords);
            Dictionary<TopologyEdgeKey, int> correctedFinalUseCounts =
                BuildTopologyEdgeUseCounts(correctedFinalFaces);
            int unexpectedOpenEdges = 0;
            int sourceBoundaryUseFailures = 0;
            foreach (KeyValuePair<TopologyEdgeKey, int> pair
                     in correctedFinalUseCounts)
            {
                if (pair.Value == 1 &&
                    !correctedSourceBoundaryEdges.Contains(pair.Key))
                {
                    unexpectedOpenEdges++;
                }
            }
            foreach (TopologyEdgeKey key
                     in correctedSourceBoundaryEdges)
            {
                int uses = correctedFinalUseCounts.TryGetValue(
                    key,
                    out int useCount)
                    ? useCount
                    : 0;
                if (uses != 1)
                {
                    sourceBoundaryUseFailures++;
                }
            }
            EdgeWearTopologyStats finalTopology = AuditEdgeWearTopology(
                correctedFinalFaces,
                minimumStableEdgeLength);
            stats.PatchCorrectedFinalUnexpectedOpenEdges +=
                unexpectedOpenEdges;
            stats.PatchCorrectedFinalNonManifoldEdges +=
                finalTopology.NonManifoldEdgeCount;
            stats.PatchCorrectedFinalTJunctions +=
                finalTopology.TJunctionCount;

            bool ready =
                cloneTopologyFailures == 0 &&
                loopCountInvariant &&
                sectorOwnershipClean &&
                provenanceFailures == 0 &&
                stats.PatchCorrectedBaselineLoopsRejected == 0 &&
                loopsAttempted == plan.PatchLoops.Count &&
                loopsBuilt == plan.PatchLoops.Count &&
                loopsFailed == 0 &&
                occurrenceBoundaryFailures == 0 &&
                intersectionFailures == 0 &&
                unexpectedOpenEdges == 0 &&
                sourceBoundaryUseFailures == 0 &&
                finalTopology.NonManifoldEdgeCount == 0 &&
                finalTopology.TJunctionCount == 0;
            stats.ReadyForCorrectedChamferPatchTopology = ready ? 1 : 0;

            string correctedFailurePrefix = cloneFailure;
            if (stats.PatchCorrectedBaselineLoopsRejected > 0)
            {
                correctedFailurePrefix = string.IsNullOrEmpty(
                    correctedFailurePrefix)
                    ? "baseline-intersection:" +
                        stats.PatchCorrectedBaselineLoopsRejected
                    : correctedFailurePrefix +
                        "|baseline-intersection:" +
                        stats.PatchCorrectedBaselineLoopsRejected;
            }
            if (reservedOccurrenceConflicts > 0)
            {
                correctedFailurePrefix = string.IsNullOrEmpty(
                    correctedFailurePrefix)
                    ? "reserved-sliver-occurrence:" +
                        reservedOccurrenceConflicts
                    : correctedFailurePrefix +
                        "|reserved-sliver-occurrence:" +
                        reservedOccurrenceConflicts;
            }
        }

        private static ChamferSliverComponentDelta
            AuditChamferReservedSliverComponentDelta(
                ChamferVertexPatchPlan plan,
                HashSet<int> failedSliverLoopIndices,
                List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
                List<ChamferHalfEdgeBoundaryComponent> postComponents,
                List<ChamferProvisionalHalfEdge> postHalfEdges,
                List<ChamferReservedSliverPatch> reservedSlivers)
        {
            ChamferSliverComponentDelta result =
                new ChamferSliverComponentDelta
                {
                    PostCollapseComponents = postComponents.Count,
                    MissingLoopCount = Mathf.Max(
                        0,
                        plan.PatchLoops.Count -
                        (postComponents.Count + reservedSlivers.Count))
                };
            List<string> diagnostics = new List<string>();
            if (failedSliverLoopIndices == null ||
                failedSliverLoopIndices.Count == 0)
            {
                return result;
            }

            ChamferHalfEdgeDecomposition preDecomposition =
                BuildChamferAuthoritativeHalfEdgeDecomposition(
                    prePatchFaceRecords,
                    true,
                    true);
            List<ChamferHalfEdgeBoundaryComponent> preComponents =
                new List<ChamferHalfEdgeBoundaryComponent>();
            for (int i = 0;
                 i < preDecomposition.Components.Count;
                 i++)
            {
                if (preDecomposition.Components[i].Kind ==
                    ChamferHalfEdgeBoundaryComponentKind.Loop)
                {
                    preComponents.Add(preDecomposition.Components[i]);
                }
            }
            result.PreCollapseComponents = preComponents.Count;

            Dictionary<VertexKey, VertexKey> collapseRemap =
                new Dictionary<VertexKey, VertexKey>();
            for (int i = 0; i < reservedSlivers.Count; i++)
            {
                collapseRemap[reservedSlivers[i].RemovedKey] =
                    reservedSlivers[i].RepresentativeKey;
            }
            List<HashSet<TopologyEdgeKey>> preOriginalKeySets =
                new List<HashSet<TopologyEdgeKey>>(preComponents.Count);
            List<HashSet<TopologyEdgeKey>> preRemappedKeySets =
                new List<HashSet<TopologyEdgeKey>>(preComponents.Count);
            for (int i = 0; i < preComponents.Count; i++)
            {
                preOriginalKeySets.Add(
                    BuildChamferHalfEdgeComponentKeySet(
                        preComponents[i],
                        preDecomposition.HalfEdges));
                preRemappedKeySets.Add(
                    BuildChamferRemappedHalfEdgeComponentKeySet(
                        preComponents[i],
                        preDecomposition.HalfEdges,
                        collapseRemap));
            }
            List<HashSet<TopologyEdgeKey>> postKeySets =
                new List<HashSet<TopologyEdgeKey>>(postComponents.Count);
            for (int i = 0; i < postComponents.Count; i++)
            {
                postKeySets.Add(BuildChamferHalfEdgeComponentKeySet(
                    postComponents[i],
                    postHalfEdges));
            }

            HashSet<int> reservedPreComponents = new HashSet<int>();
            foreach (int failedLoopIndex in failedSliverLoopIndices)
            {
                ChamferVertexPatchLoop failedLoop = null;
                for (int loopIndex = 0;
                     loopIndex < plan.PatchLoops.Count;
                     loopIndex++)
                {
                    if (plan.PatchLoops[loopIndex].LoopIndex ==
                        failedLoopIndex)
                    {
                        failedLoop = plan.PatchLoops[loopIndex];
                        break;
                    }
                }
                if (failedLoop == null)
                {
                    continue;
                }
                HashSet<TopologyEdgeKey> failedKeys =
                    new HashSet<TopologyEdgeKey>(failedLoop.OrderedBoundaryKeys);
                int bestComponent = -1;
                int bestOverlap = 0;
                for (int componentIndex = 0;
                     componentIndex < preOriginalKeySets.Count;
                     componentIndex++)
                {
                    int overlap = CountChamferTopologyKeyOverlap(
                        failedKeys,
                        preOriginalKeySets[componentIndex]);
                    if (overlap > bestOverlap)
                    {
                        bestOverlap = overlap;
                        bestComponent = componentIndex;
                    }
                }
                if (bestComponent >= 0 && bestOverlap > 0)
                {
                    reservedPreComponents.Add(bestComponent);
                }
            }
            result.ReservedPreCollapseComponents =
                reservedPreComponents.Count;
            foreach (int reservedIndex in reservedPreComponents)
            {
                diagnostics.Add(
                    "r" + preComponents[reservedIndex].ComponentIndex);
            }

            for (int preIndex = 0;
                 preIndex < preRemappedKeySets.Count;
                 preIndex++)
            {
                if (reservedPreComponents.Contains(preIndex))
                {
                    continue;
                }
                List<int> matchingPostComponents = new List<int>();
                bool exact = false;
                for (int postIndex = 0;
                     postIndex < postKeySets.Count;
                     postIndex++)
                {
                    int overlap = CountChamferTopologyKeyOverlap(
                        preRemappedKeySets[preIndex],
                        postKeySets[postIndex]);
                    if (overlap <= 0)
                    {
                        continue;
                    }
                    matchingPostComponents.Add(
                        postComponents[postIndex].ComponentIndex);
                    if (preRemappedKeySets[preIndex].SetEquals(
                            postKeySets[postIndex]))
                    {
                        exact = true;
                    }
                }
                if (exact)
                {
                    result.ExactComponentMatches++;
                }
                if (matchingPostComponents.Count == 0)
                {
                    result.DisappearedPreComponents++;
                }
                else if (matchingPostComponents.Count > 1)
                {
                    result.SplitPreComponents++;
                }
                if (!exact || matchingPostComponents.Count != 1)
                {
                    diagnostics.Add(
                        "p" + preComponents[preIndex].ComponentIndex +
                        ">" +
                        (matchingPostComponents.Count == 0
                            ? "none"
                            : string.Join(",", matchingPostComponents)));
                }
            }

            for (int postIndex = 0;
                 postIndex < postKeySets.Count;
                 postIndex++)
            {
                List<int> matchingPreComponents = new List<int>();
                for (int preIndex = 0;
                     preIndex < preRemappedKeySets.Count;
                     preIndex++)
                {
                    if (reservedPreComponents.Contains(preIndex))
                    {
                        continue;
                    }
                    if (CountChamferTopologyKeyOverlap(
                            preRemappedKeySets[preIndex],
                            postKeySets[postIndex]) > 0)
                    {
                        matchingPreComponents.Add(
                            preComponents[preIndex].ComponentIndex);
                    }
                }
                if (matchingPreComponents.Count > 1)
                {
                    result.MergedPostComponents++;
                    diagnostics.Add(
                        "m" + postComponents[postIndex].ComponentIndex +
                        "<" + string.Join(",", matchingPreComponents));
                }
            }
            result.Diagnostic = diagnostics.Count == 0
                ? "none"
                : string.Join(";", diagnostics);
            return result;
        }

        private static HashSet<TopologyEdgeKey>
            BuildChamferRemappedHalfEdgeComponentKeySet(
                ChamferHalfEdgeBoundaryComponent component,
                List<ChamferProvisionalHalfEdge> halfEdges,
                Dictionary<VertexKey, VertexKey> remap)
        {
            HashSet<TopologyEdgeKey> result =
                new HashSet<TopologyEdgeKey>();
            for (int i = 0;
                 i < component.OrderedHalfEdgeIndices.Count;
                 i++)
            {
                ChamferProvisionalHalfEdge edge = halfEdges[
                    component.OrderedHalfEdgeIndices[i]];
                VertexKey start = remap.TryGetValue(
                    edge.StartKey,
                    out VertexKey mappedStart)
                    ? mappedStart
                    : edge.StartKey;
                VertexKey end = remap.TryGetValue(
                    edge.EndKey,
                    out VertexKey mappedEnd)
                    ? mappedEnd
                    : edge.EndKey;
                if (!start.Equals(end))
                {
                    result.Add(new TopologyEdgeKey(start, end));
                }
            }
            return result;
        }

        private static int CountChamferTopologyKeyOverlap(
            HashSet<TopologyEdgeKey> first,
            HashSet<TopologyEdgeKey> second)
        {
            if (first == null || second == null)
            {
                return 0;
            }
            HashSet<TopologyEdgeKey> smaller =
                first.Count <= second.Count ? first : second;
            HashSet<TopologyEdgeKey> larger =
                ReferenceEquals(smaller, first) ? second : first;
            int count = 0;
            foreach (TopologyEdgeKey key in smaller)
            {
                if (larger.Contains(key))
                {
                    count++;
                }
            }
            return count;
        }

        private static HashSet<int>
            BuildChamferPromotedSectorHalfEdgeIndices(
                ChamferHalfEdgeDecomposition decomposition)
        {
            HashSet<int> result = new HashSet<int>();
            for (int i = 0;
                 i < decomposition.CoDirectedPairs.Count;
                 i++)
            {
                ChamferCoDirectedHalfEdgePair pair =
                    decomposition.CoDirectedPairs[i];
                result.Add(pair.FirstHalfEdgeIndex);
                result.Add(pair.SecondHalfEdgeIndex);
            }
            return result;
        }

        private static List<ChamferProvisionalFaceRecord>
            BuildChamferNonPatchFaceRecordSnapshot(
                List<ChamferProvisionalFaceRecord> records)
        {
            List<ChamferProvisionalFaceRecord> result =
                new List<ChamferProvisionalFaceRecord>();
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].Kind !=
                    ChamferProvisionalFaceKind.VertexPatch)
                {
                    result.Add(records[i]);
                }
            }
            return result;
        }

        private static bool TryAppendChamferReservedSliverPatches(
            ChamferVertexPatchPlan plan,
            HashSet<int> failedSliverLoopIndices,
            List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
            List<ChamferProvisionalFaceRecord> correctedRecords,
            ChamferTopologyContext context,
            List<ChamferExpectedVertexBoundary> normalizedVertexBoundaries,
            List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            float minimumPatchTriangleArea,
            Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
            out List<ChamferReservedSliverPatch> reservedSlivers,
            out int occurrenceConflicts,
            out string failure)
        {
            reservedSlivers = new List<ChamferReservedSliverPatch>();
            occurrenceConflicts = 0;
            failure = string.Empty;
            if (failedSliverLoopIndices.Count == 0)
            {
                return true;
            }

            for (int loopListIndex = 0;
                 loopListIndex < plan.PatchLoops.Count;
                 loopListIndex++)
            {
                ChamferVertexPatchLoop loop =
                    plan.PatchLoops[loopListIndex];
                if (!failedSliverLoopIndices.Contains(loop.LoopIndex))
                {
                    continue;
                }

                List<ChamferSliverEdgeCandidate> candidates =
                    BuildChamferSliverEdgeCandidates(
                        loop,
                        prePatchFaceRecords);
                if (candidates.Count == 0)
                {
                    occurrenceConflicts++;
                    failure = "reserved-sliver-has-no-candidate";
                    return false;
                }

                ChamferSliverNormalizationResult normalization =
                    EvaluateChamferVirtualSliverNormalization(
                        loop,
                        candidates[0],
                        prePatchFaceRecords,
                        context,
                        normalizedVertexBoundaries,
                        sourceBoundaryRecords,
                        false,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        minimumPatchTriangleArea,
                        sharedSpans);
                if (normalization.Resolution !=
                        ChamferSliverResolution.ResolvedToTriangle ||
                    normalization.PostCollapseBoundaryPositions == null ||
                    normalization.PostCollapseBoundaryPositions.Count != 3)
                {
                    occurrenceConflicts++;
                    failure = string.IsNullOrEmpty(normalization.Failure)
                        ? "reserved-sliver-local-proof-is-not-a-triangle"
                        : normalization.Failure;
                    return false;
                }

                List<Vector3> patchPositions = new List<Vector3>(
                    normalization.PostCollapseBoundaryPositions);
                ChamferHalfEdgeDecomposition decomposition =
                    BuildChamferAuthoritativeHalfEdgeDecomposition(
                        correctedRecords,
                        true,
                        false);
                List<int> boundaryHalfEdges = new List<int>(3);
                HashSet<int> usedHalfEdges = new HashSet<int>();
                for (int patchEdgeIndex = 0;
                     patchEdgeIndex < patchPositions.Count;
                     patchEdgeIndex++)
                {
                    VertexKey requiredSurfaceStart = new VertexKey(
                        patchPositions[
                            (patchEdgeIndex + 1) % patchPositions.Count]);
                    VertexKey requiredSurfaceEnd = new VertexKey(
                        patchPositions[patchEdgeIndex]);
                    int matchedHalfEdge = -1;
                    for (int boundaryIndex = 0;
                         boundaryIndex < decomposition.BoundaryHalfEdges.Count;
                         boundaryIndex++)
                    {
                        int halfEdgeIndex =
                            decomposition.BoundaryHalfEdges[boundaryIndex];
                        ChamferProvisionalHalfEdge halfEdge =
                            decomposition.HalfEdges[halfEdgeIndex];
                        if (usedHalfEdges.Contains(halfEdgeIndex) ||
                            !halfEdge.StartKey.Equals(
                                requiredSurfaceStart) ||
                            !halfEdge.EndKey.Equals(requiredSurfaceEnd))
                        {
                            continue;
                        }
                        if (matchedHalfEdge >= 0)
                        {
                            occurrenceConflicts++;
                            failure =
                                "reserved-sliver-exact-edge-has-multiple-occurrences";
                            return false;
                        }
                        matchedHalfEdge = halfEdgeIndex;
                    }
                    if (matchedHalfEdge < 0)
                    {
                        occurrenceConflicts++;
                        failure =
                            "reserved-sliver-exact-edge-has-no-occurrence";
                        return false;
                    }
                    usedHalfEdges.Add(matchedHalfEdge);
                    boundaryHalfEdges.Add(matchedHalfEdge);
                }

                if (!TryOrderChamferBoundaryHalfEdgesByEndpoints(
                        boundaryHalfEdges,
                        decomposition.HalfEdges,
                        out List<int> orderedBoundaryHalfEdges))
                {
                    occurrenceConflicts++;
                    failure =
                        "reserved-sliver-exact-occurrences-do-not-form-a-loop";
                    return false;
                }
                ChamferHalfEdgeBoundaryComponent diagnosticComponent =
                    new ChamferHalfEdgeBoundaryComponent(
                        -1,
                        ChamferHalfEdgeBoundaryComponentKind.Loop,
                        orderedBoundaryHalfEdges);
                if (!TryBuildChamferDirectPatchFromBoundaryComponent(
                        diagnosticComponent,
                        decomposition.HalfEdges,
                        correctedRecords,
                        minimumPatchTriangleArea,
                        loop.LoopIndex,
                        loop.FeatureStrength,
                        out ChamferProvisionalFaceRecord record,
                        out ChamferDirectedTriangleGeometry geometry,
                        out string triangleFailure))
                {
                    occurrenceConflicts++;
                    failure = triangleFailure;
                    return false;
                }
                int boundaryFailures =
                    CountChamferCorrectedPatchBoundaryOccurrenceFailures(
                        new List<ChamferProvisionalFaceRecord>
                        {
                            record
                        },
                        diagnosticComponent,
                        decomposition.HalfEdges,
                        out _);
                if (boundaryFailures > 0)
                {
                    occurrenceConflicts += boundaryFailures;
                    failure =
                        "reserved-sliver-occurrence-direction-failure";
                    return false;
                }

                correctedRecords.Add(record);
                List<ChamferBoundarySegment> boundarySegments =
                    BuildChamferPatchBoundarySegments(
                        new List<ChamferProvisionalFaceRecord>
                        {
                            record
                        });
                reservedSlivers.Add(new ChamferReservedSliverPatch(
                    loop.LoopIndex,
                    record,
                    geometry,
                    orderedBoundaryHalfEdges,
                    boundarySegments,
                    normalization.RemovedKey,
                    normalization.RepresentativeKey));
            }
            return true;
        }

        private static bool TryOrderChamferBoundaryHalfEdgesByEndpoints(
            List<int> sourceIndices,
            List<ChamferProvisionalHalfEdge> halfEdges,
            out List<int> ordered)
        {
            ordered = new List<int>();
            if (sourceIndices.Count == 0)
            {
                return false;
            }
            int start = sourceIndices[0];
            for (int i = 1; i < sourceIndices.Count; i++)
            {
                if (sourceIndices[i] < start)
                {
                    start = sourceIndices[i];
                }
            }
            HashSet<int> remaining = new HashSet<int>(sourceIndices);
            int current = start;
            while (remaining.Remove(current))
            {
                ordered.Add(current);
                VertexKey requiredStart = halfEdges[current].EndKey;
                int next = -1;
                foreach (int candidate in remaining)
                {
                    if (!halfEdges[candidate].StartKey.Equals(
                            requiredStart))
                    {
                        continue;
                    }
                    if (next >= 0)
                    {
                        ordered.Clear();
                        return false;
                    }
                    next = candidate;
                }
                if (remaining.Count == 0)
                {
                    break;
                }
                if (next < 0)
                {
                    ordered.Clear();
                    return false;
                }
                current = next;
            }
            return ordered.Count == sourceIndices.Count &&
                halfEdges[ordered[ordered.Count - 1]].EndKey.Equals(
                    halfEdges[ordered[0]].StartKey);
        }

        private static bool TryBuildChamferDirectPatchFromBoundaryComponent(
            ChamferHalfEdgeBoundaryComponent component,
            List<ChamferProvisionalHalfEdge> halfEdges,
            List<ChamferProvisionalFaceRecord> faceRecords,
            float minimumPatchTriangleArea,
            out ChamferProvisionalFaceRecord record,
            out ChamferDirectedTriangleGeometry geometry,
            out string failure)
        {
            return TryBuildChamferDirectPatchFromBoundaryComponent(
                component,
                halfEdges,
                faceRecords,
                minimumPatchTriangleArea,
                component.ComponentIndex,
                CalculateChamferSectorFeatureStrength(
                    component,
                    halfEdges,
                    faceRecords),
                out record,
                out geometry,
                out failure);
        }

        private static bool TryBuildChamferDirectPatchFromBoundaryComponent(
            ChamferHalfEdgeBoundaryComponent component,
            List<ChamferProvisionalHalfEdge> halfEdges,
            List<ChamferProvisionalFaceRecord> faceRecords,
            float minimumPatchTriangleArea,
            int patchLoopIndex,
            float featureStrength,
            out ChamferProvisionalFaceRecord record,
            out ChamferDirectedTriangleGeometry geometry,
            out string failure)
        {
            record = null;
            geometry = null;
            failure = string.Empty;
            if (component.OrderedHalfEdgeIndices.Count != 3)
            {
                failure = "direct-boundary-is-not-a-triangle";
                return false;
            }
            List<Vector3> positions =
                BuildChamferOppositeBoundaryPositions(
                    component,
                    halfEdges);
            if (!TryCreateChamferDirectedTriangleGeometry(
                    positions[0],
                    positions[1],
                    positions[2],
                    patchLoopIndex,
                    minimumPatchTriangleArea,
                    out geometry,
                    out failure))
            {
                return false;
            }
            PolygonFace face = new PolygonFace(
                positions,
                geometry.Normal,
                PolygonFaceFeature.ConvexEdgeWear,
                featureStrength);
            record = new ChamferProvisionalFaceRecord(
                face,
                ChamferProvisionalFaceKind.VertexPatch,
                -1,
                -1,
                patchLoopIndex);
            return true;
        }

        private static bool TryBuildChamferCorrectedPrePatchClone(
            ChamferVertexPatchPlan plan,
            HashSet<int> failedSliverLoopIndices,
            List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
            ChamferTopologyContext context,
            List<ChamferExpectedVertexBoundary> normalizedVertexBoundaries,
            List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
            out List<ChamferProvisionalFaceRecord> correctedRecords,
            out HashSet<TopologyEdgeKey> correctedSourceBoundaryEdges,
            out int sliverCollapses,
            out int topologyFailures,
            out string failure)
        {
            correctedRecords = CloneChamferProvisionalFaceRecords(
                prePatchFaceRecords);
            List<ChamferExpectedVertexBoundary> correctedBoundaries =
                new List<ChamferExpectedVertexBoundary>(
                    normalizedVertexBoundaries);
            List<ChamferSourceBoundaryRecord> correctedSourceBoundaries =
                CloneChamferSourceBoundaryRecords(
                    sourceBoundaryRecords);
            Dictionary<int, ChamferSharedEdgeSpan> correctedSharedSpans =
                new Dictionary<int, ChamferSharedEdgeSpan>(sharedSpans);
            correctedSourceBoundaryEdges =
                new HashSet<TopologyEdgeKey>(
                    plan.FinalSourceBoundaryEdges);
            sliverCollapses = 0;
            topologyFailures = 0;
            failure = string.Empty;

            if (failedSliverLoopIndices.Count == 0)
            {
                return true;
            }

            for (int loopListIndex = 0;
                 loopListIndex < plan.PatchLoops.Count;
                 loopListIndex++)
            {
                ChamferVertexPatchLoop loop =
                    plan.PatchLoops[loopListIndex];
                if (!failedSliverLoopIndices.Contains(loop.LoopIndex))
                {
                    continue;
                }
                List<ChamferSliverEdgeCandidate> candidates =
                    BuildChamferSliverEdgeCandidates(
                        loop,
                        correctedRecords);
                if (candidates.Count == 0)
                {
                    topologyFailures++;
                    failure =
                        "corrected-clone-sliver-has-no-candidate";
                    return false;
                }
                if (!TryDetermineChamferSliverRepresentative(
                        candidates[0],
                        correctedRecords,
                        out VertexKey removedKey,
                        out Vector3 representativePosition,
                        out string representativeFailure))
                {
                    topologyFailures++;
                    failure = representativeFailure;
                    return false;
                }
                if (!TryRemapAndSanitizeChamferDiagnosticFaces(
                        correctedRecords,
                        removedKey,
                        representativePosition,
                        minimumStableFaceArea,
                        out string faceFailure))
                {
                    topologyFailures++;
                    failure = faceFailure;
                    return false;
                }
                correctedBoundaries =
                    CloneAndRemapChamferExpectedVertexBoundaries(
                        correctedBoundaries,
                        removedKey,
                        representativePosition);
                correctedSourceBoundaries =
                    CloneAndRemapChamferSourceBoundaryRecords(
                        correctedSourceBoundaries,
                        removedKey,
                        representativePosition);
                correctedSharedSpans =
                    CloneAndRemapChamferSharedEdgeSpans(
                        correctedSharedSpans,
                        removedKey,
                        representativePosition);
                sliverCollapses++;
            }

            ChamferEmissionStats diagnosticStats = default;
            SegmentRawChamferTJunctions(
                correctedRecords,
                context,
                correctedSharedSpans,
                correctedBoundaries,
                correctedSourceBoundaries,
                minimumStableEdgeLength,
                ref diagnosticStats);
            if (diagnosticStats.TJunctionRecordsIncompatible > 0)
            {
                topologyFailures +=
                    diagnosticStats.TJunctionRecordsIncompatible;
                failure = "corrected-clone-incompatible-t-junction";
                return false;
            }

            HashSet<TopologyEdgeKey> retraceRemovedKeys =
                new HashSet<TopologyEdgeKey>();
            if (!NormalizeChamferProvisionalFaceWalks(
                    correctedRecords,
                    minimumStableFaceArea,
                    retraceRemovedKeys,
                    ref diagnosticStats,
                    out failure))
            {
                topologyFailures++;
                return false;
            }
            List<PolygonFace> correctedFaces =
                ExtractChamferProvisionalFaces(correctedRecords);
            Dictionary<TopologyEdgeKey, int> correctedUseCounts =
                BuildTopologyEdgeUseCounts(correctedFaces);
            RemoveRetraceDeletedChamferBoundaries(
                correctedBoundaries,
                correctedUseCounts,
                retraceRemovedKeys,
                ref diagnosticStats);
            HashSet<TopologyEdgeKey> sourceBoundarySegmentKeys =
                BuildChamferSourceBoundarySegmentKeys(
                    correctedSourceBoundaries);
            List<ChamferProvisionalSegmentRecord> rebuiltSegments =
                BuildChamferProvisionalSegmentRecords(
                    correctedRecords,
                    correctedBoundaries,
                    sourceBoundarySegmentKeys,
                    correctedSharedSpans);
            List<ChamferExpectedVertexBoundary> rebuiltBoundaries =
                NormalizeChamferVertexBoundaries(
                    correctedBoundaries,
                    correctedUseCounts,
                    rebuiltSegments,
                    ref diagnosticStats);
            Dictionary<TopologyEdgeKey,
                List<ChamferSourceBoundaryChildOccurrence>>
                rawSourceBoundaryOccurrences =
                    BuildChamferSourceBoundaryChildOccurrences(
                        correctedSourceBoundaries);
            HashSet<TopologyEdgeKey> rebuiltBoundaryKeys =
                BuildChamferExpectedBoundaryKeySet(
                    rebuiltBoundaries);
            NormalizeChamferSourceBoundaryLoops(
                correctedSourceBoundaries,
                correctedUseCounts,
                new HashSet<TopologyEdgeKey>(rebuiltBoundaryKeys),
                rebuiltSegments,
                ref diagnosticStats);
            CollapseChamferSourceBoundaryTerminalTransferAliases(
                correctedSourceBoundaries,
                rawSourceBoundaryOccurrences,
                correctedUseCounts,
                new HashSet<TopologyEdgeKey>(rebuiltBoundaryKeys),
                rebuiltSegments,
                ref diagnosticStats);
            correctedSourceBoundaryEdges =
                AuditChamferSourceBoundaryOwnership(
                    correctedSourceBoundaries,
                    correctedUseCounts,
                    new HashSet<TopologyEdgeKey>(rebuiltBoundaryKeys),
                    rebuiltSegments,
                    rawSourceBoundaryOccurrences,
                    ref diagnosticStats);
            if (diagnosticStats.SourceBoundaryLoopNormalizationFailureCount > 0 ||
                diagnosticStats.SourceBoundaryTerminalAliasNormalizationFailureCount > 0 ||
                diagnosticStats.SourceBoundaryChildIncidenceFailureCount > 0)
            {
                topologyFailures++;
                failure = "corrected-clone-boundary-normalization-failure";
                return false;
            }
            return true;
        }

        private static bool TryDetermineChamferSliverRepresentative(
            ChamferSliverEdgeCandidate chosen,
            List<ChamferProvisionalFaceRecord> faceRecords,
            out VertexKey removedKey,
            out Vector3 representativePosition,
            out string failure)
        {
            VertexKey firstKey = new VertexKey(chosen.Start);
            VertexKey secondKey = new VertexKey(chosen.End);
            bool hasDecision = false;
            VertexKey representativeKey = default;
            representativePosition = Vector3.zero;
            failure = string.Empty;
            for (int faceIndex = 0;
                 faceIndex < faceRecords.Count;
                 faceIndex++)
            {
                PolygonFace face = faceRecords[faceIndex].Face;
                bool containsFirst = ChamferFaceContainsVertexKey(
                    face,
                    firstKey);
                bool containsSecond = ChamferFaceContainsVertexKey(
                    face,
                    secondKey);
                if (!containsFirst || !containsSecond)
                {
                    continue;
                }
                List<ChamferTrackedSanitizeVertex> sanitized =
                    SanitizeChamferTrackedPolygon(
                        face.Vertices,
                        face.Normal);
                bool firstSurvives =
                    ChamferTrackedPolygonContainsOriginalKey(
                        sanitized,
                        firstKey);
                bool secondSurvives =
                    ChamferTrackedPolygonContainsOriginalKey(
                        sanitized,
                        secondKey);
                if (firstSurvives == secondSurvives)
                {
                    removedKey = default;
                    failure =
                        "corrected-clone-sliver-has-no-single-survivor";
                    return false;
                }
                VertexKey candidateRepresentative = firstSurvives
                    ? firstKey
                    : secondKey;
                Vector3 candidatePosition = firstSurvives
                    ? chosen.Start
                    : chosen.End;
                if (!hasDecision)
                {
                    hasDecision = true;
                    representativeKey = candidateRepresentative;
                    representativePosition = candidatePosition;
                }
                else if (!representativeKey.Equals(
                    candidateRepresentative))
                {
                    removedKey = default;
                    failure =
                        "corrected-clone-sliver-survivor-conflict";
                    return false;
                }
            }
            if (!hasDecision)
            {
                removedKey = default;
                failure = "corrected-clone-sliver-has-no-owner";
                return false;
            }
            removedKey = representativeKey.Equals(firstKey)
                ? secondKey
                : firstKey;
            return true;
        }

        private static bool TryRemapAndSanitizeChamferDiagnosticFaces(
            List<ChamferProvisionalFaceRecord> records,
            VertexKey removedKey,
            Vector3 representativePosition,
            float minimumStableFaceArea,
            out string failure)
        {
            failure = string.Empty;
            for (int faceIndex = 0;
                 faceIndex < records.Count;
                 faceIndex++)
            {
                ChamferProvisionalFaceRecord record = records[faceIndex];
                List<Vector3> remapped = new List<Vector3>(
                    record.Face.Vertices.Count);
                for (int vertexIndex = 0;
                     vertexIndex < record.Face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 position = record.Face.Vertices[vertexIndex];
                    remapped.Add(new VertexKey(position).Equals(removedKey)
                        ? representativePosition
                        : position);
                }
                List<Vector3> sanitized = SanitizePolygon(
                    remapped,
                    record.Face.Normal);
                if (sanitized.Count < 3 ||
                    CalculatePolygonArea(sanitized) <=
                        minimumStableFaceArea ||
                    !TryFindDuplicateChamferFaceEdge(
                        sanitized,
                        out _,
                        out _,
                        out _))
                {
                    failure = "corrected-clone-face-validation-failure";
                    return false;
                }
                Vector3 normal = CalculatePolygonNormal(sanitized);
                if (!IsFinite(normal) ||
                    Vector3.Dot(normal, record.Face.Normal) <= 0f)
                {
                    failure = "corrected-clone-face-winding-failure";
                    return false;
                }
                record.Face = new PolygonFace(
                    sanitized,
                    normal,
                    record.Face.Feature,
                    record.Face.FeatureStrength);
            }
            return true;
        }

        private static List<ChamferSectorPlanAssignment>
            BuildChamferSectorPlanAssignments(
                List<ChamferVertexPatchLoop> plans,
                List<ChamferHalfEdgeBoundaryComponent> components,
                ChamferHalfEdgeDecomposition decomposition)
        {
            List<ChamferSectorPlanAssignment> result =
                new List<ChamferSectorPlanAssignment>();
            bool[] planAssigned = new bool[plans.Count];
            bool[] componentAssigned = new bool[components.Count];

            for (int planIndex = 0;
                 planIndex < plans.Count;
                 planIndex++)
            {
                for (int componentIndex = 0;
                     componentIndex < components.Count;
                     componentIndex++)
                {
                    if (planAssigned[planIndex] ||
                        componentAssigned[componentIndex] ||
                        !DoesChamferHalfEdgeComponentMatchPatchOrder(
                            plans[planIndex].OrderedBoundaryKeys,
                            components[componentIndex],
                            decomposition.HalfEdges))
                    {
                        continue;
                    }
                    int occurrenceOverlap =
                        CountChamferPlanSectorOccurrenceOverlap(
                            plans[planIndex],
                            components[componentIndex],
                            decomposition.HalfEdges);
                    int keyOverlap = CountChamferPlanSectorKeyOverlap(
                        plans[planIndex],
                        components[componentIndex],
                        decomposition.HalfEdges);
                    result.Add(new ChamferSectorPlanAssignment(
                        planIndex,
                        componentIndex,
                        occurrenceOverlap,
                        keyOverlap,
                        true));
                    planAssigned[planIndex] = true;
                    componentAssigned[componentIndex] = true;
                }
            }

            while (result.Count < Mathf.Min(plans.Count, components.Count))
            {
                int bestPlan = -1;
                int bestComponent = -1;
                int bestOccurrenceOverlap = -1;
                int bestKeyOverlap = -1;
                for (int planIndex = 0;
                     planIndex < plans.Count;
                     planIndex++)
                {
                    if (planAssigned[planIndex])
                    {
                        continue;
                    }
                    for (int componentIndex = 0;
                         componentIndex < components.Count;
                         componentIndex++)
                    {
                        if (componentAssigned[componentIndex])
                        {
                            continue;
                        }
                        int occurrenceOverlap =
                            CountChamferPlanSectorOccurrenceOverlap(
                                plans[planIndex],
                                components[componentIndex],
                                decomposition.HalfEdges);
                        int keyOverlap = CountChamferPlanSectorKeyOverlap(
                            plans[planIndex],
                            components[componentIndex],
                            decomposition.HalfEdges);
                        if (occurrenceOverlap > bestOccurrenceOverlap ||
                            (occurrenceOverlap == bestOccurrenceOverlap &&
                             keyOverlap > bestKeyOverlap) ||
                            (occurrenceOverlap == bestOccurrenceOverlap &&
                             keyOverlap == bestKeyOverlap &&
                             (bestPlan < 0 ||
                              planIndex < bestPlan ||
                              (planIndex == bestPlan &&
                               componentIndex < bestComponent))))
                        {
                            bestPlan = planIndex;
                            bestComponent = componentIndex;
                            bestOccurrenceOverlap = occurrenceOverlap;
                            bestKeyOverlap = keyOverlap;
                        }
                    }
                }
                if (bestPlan < 0 || bestComponent < 0)
                {
                    break;
                }
                result.Add(new ChamferSectorPlanAssignment(
                    bestPlan,
                    bestComponent,
                    bestOccurrenceOverlap,
                    bestKeyOverlap,
                    false));
                planAssigned[bestPlan] = true;
                componentAssigned[bestComponent] = true;
            }

            for (int planIndex = 0;
                 planIndex < plans.Count;
                 planIndex++)
            {
                if (!planAssigned[planIndex])
                {
                    result.Add(new ChamferSectorPlanAssignment(
                        planIndex,
                        -1,
                        0,
                        0,
                        false));
                }
            }
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                if (!componentAssigned[componentIndex])
                {
                    result.Add(new ChamferSectorPlanAssignment(
                        -1,
                        componentIndex,
                        0,
                        0,
                        false));
                }
            }
            result.Sort((left, right) =>
            {
                int comparison = left.PlanListIndex.CompareTo(
                    right.PlanListIndex);
                if (comparison != 0)
                {
                    return comparison;
                }
                return left.ComponentListIndex.CompareTo(
                    right.ComponentListIndex);
            });
            return result;
        }

        private static void AuditChamferSectorOccurrenceOwnership(
            List<ChamferVertexPatchLoop> plans,
            List<ChamferHalfEdgeBoundaryComponent> components,
            List<ChamferProvisionalHalfEdge> halfEdges,
            HashSet<int> promotedSectorHalfEdges,
            out int boundaryHalfEdges,
            out int legacyAssignedBoundaryHalfEdges,
            out int promotedBoundaryHalfEdges,
            out int assignedBoundaryHalfEdges,
            out int unassignedBoundaryHalfEdges,
            out int multiAssignedBoundaryHalfEdges,
            out int plansContributingToMultipleSectorLoops,
            out int sectorLoopsCombiningMultiplePlans,
            out int plansWithoutOccurrences,
            out int sectorLoopsWithoutOccurrences)
        {
            boundaryHalfEdges = 0;
            legacyAssignedBoundaryHalfEdges = 0;
            promotedBoundaryHalfEdges = 0;
            assignedBoundaryHalfEdges = 0;
            unassignedBoundaryHalfEdges = 0;
            multiAssignedBoundaryHalfEdges = 0;
            bool[,] overlaps = new bool[plans.Count, components.Count];
            bool[] componentHasPromotedOccurrence =
                new bool[components.Count];
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                ChamferHalfEdgeBoundaryComponent component =
                    components[componentIndex];
                for (int edgeIndex = 0;
                     edgeIndex < component.OrderedHalfEdgeIndices.Count;
                     edgeIndex++)
                {
                    boundaryHalfEdges++;
                    int halfEdgeIndex =
                        component.OrderedHalfEdgeIndices[edgeIndex];
                    ChamferProvisionalHalfEdge halfEdge =
                        halfEdges[halfEdgeIndex];
                    int occurrenceOwners = 0;
                    for (int planIndex = 0;
                         planIndex < plans.Count;
                         planIndex++)
                    {
                        int matches =
                            CountChamferPlanHalfEdgeOccurrenceMatches(
                                plans[planIndex],
                                halfEdge);
                        if (matches <= 0)
                        {
                            continue;
                        }
                        overlaps[planIndex, componentIndex] = true;
                        occurrenceOwners += matches;
                    }
                    if (occurrenceOwners == 1)
                    {
                        legacyAssignedBoundaryHalfEdges++;
                        assignedBoundaryHalfEdges++;
                    }
                    else if (occurrenceOwners == 0 &&
                             promotedSectorHalfEdges.Contains(
                                 halfEdgeIndex))
                    {
                        promotedBoundaryHalfEdges++;
                        assignedBoundaryHalfEdges++;
                        componentHasPromotedOccurrence[componentIndex] =
                            true;
                    }
                    else if (occurrenceOwners == 0)
                    {
                        unassignedBoundaryHalfEdges++;
                    }
                    else
                    {
                        multiAssignedBoundaryHalfEdges++;
                    }
                }
            }

            plansContributingToMultipleSectorLoops = 0;
            plansWithoutOccurrences = 0;
            for (int planIndex = 0;
                 planIndex < plans.Count;
                 planIndex++)
            {
                int componentCount = 0;
                for (int componentIndex = 0;
                     componentIndex < components.Count;
                     componentIndex++)
                {
                    if (overlaps[planIndex, componentIndex])
                    {
                        componentCount++;
                    }
                }
                if (componentCount == 0)
                {
                    plansWithoutOccurrences++;
                }
                else if (componentCount > 1)
                {
                    plansContributingToMultipleSectorLoops++;
                }
            }

            sectorLoopsCombiningMultiplePlans = 0;
            sectorLoopsWithoutOccurrences = 0;
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                int planCount = 0;
                for (int planIndex = 0;
                     planIndex < plans.Count;
                     planIndex++)
                {
                    if (overlaps[planIndex, componentIndex])
                    {
                        planCount++;
                    }
                }
                if (planCount == 0 &&
                    !componentHasPromotedOccurrence[componentIndex])
                {
                    sectorLoopsWithoutOccurrences++;
                }
                else if (planCount > 1)
                {
                    sectorLoopsCombiningMultiplePlans++;
                }
            }
        }

        private static int CountChamferPlanHalfEdgeOccurrenceMatches(
            ChamferVertexPatchLoop plan,
            ChamferProvisionalHalfEdge halfEdge)
        {
            int matches = 0;
            for (int componentIndex = 0;
                 componentIndex < plan.Components.Count;
                 componentIndex++)
            {
                ChamferVertexPatchComponent sourceComponent =
                    plan.Components[componentIndex];
                for (int boundaryIndex = 0;
                     boundaryIndex <
                        sourceComponent.OrderedBoundaries.Count;
                     boundaryIndex++)
                {
                    ChamferExpectedVertexBoundary boundary =
                        sourceComponent.OrderedBoundaries[
                            boundaryIndex].Boundary;
                    bool provenanceMatches =
                        halfEdge.FaceKind ==
                            ChamferProvisionalFaceKind.ReplacementBase
                            ? boundary.FaceIndex ==
                                halfEdge.SourceFaceIndex
                            : halfEdge.FaceKind ==
                                ChamferProvisionalFaceKind.BevelStrip
                                ? boundary.SourceEdgeIndex ==
                                    halfEdge.SourceEdgeIndex
                                : boundary.FaceIndex ==
                                    halfEdge.SourceFaceIndex &&
                                  boundary.SourceEdgeIndex ==
                                    halfEdge.SourceEdgeIndex;
                    if (boundary.Key.Equals(
                            halfEdge.UndirectedKey) &&
                        provenanceMatches)
                    {
                        matches++;
                    }
                }
            }
            return matches;
        }

        private static int CountChamferPlanSectorOccurrenceOverlap(
            ChamferVertexPatchLoop plan,
            ChamferHalfEdgeBoundaryComponent component,
            List<ChamferProvisionalHalfEdge> halfEdges)
        {
            int overlap = 0;
            for (int edgeIndex = 0;
                 edgeIndex < component.OrderedHalfEdgeIndices.Count;
                 edgeIndex++)
            {
                ChamferProvisionalHalfEdge halfEdge = halfEdges[
                    component.OrderedHalfEdgeIndices[edgeIndex]];
                if (CountChamferPlanHalfEdgeOccurrenceMatches(
                        plan,
                        halfEdge) > 0)
                {
                    overlap++;
                }
            }
            return overlap;
        }

        private static int CountChamferPlanSectorKeyOverlap(
            ChamferVertexPatchLoop plan,
            ChamferHalfEdgeBoundaryComponent component,
            List<ChamferProvisionalHalfEdge> halfEdges)
        {
            HashSet<TopologyEdgeKey> planned =
                new HashSet<TopologyEdgeKey>(
                    plan.OrderedBoundaryKeys);
            int overlap = 0;
            for (int i = 0;
                 i < component.OrderedHalfEdgeIndices.Count;
                 i++)
            {
                if (planned.Contains(halfEdges[
                    component.OrderedHalfEdgeIndices[i]].UndirectedKey))
                {
                    overlap++;
                }
            }
            return overlap;
        }

        private static float CalculateChamferSectorFeatureStrength(
            ChamferHalfEdgeBoundaryComponent component,
            List<ChamferProvisionalHalfEdge> halfEdges,
            List<ChamferProvisionalFaceRecord> faceRecords)
        {
            float sum = 0f;
            int count = 0;
            HashSet<int> faces = new HashSet<int>();
            for (int i = 0;
                 i < component.OrderedHalfEdgeIndices.Count;
                 i++)
            {
                int faceIndex = halfEdges[
                    component.OrderedHalfEdgeIndices[i]].FaceRecordIndex;
                if (faces.Add(faceIndex) &&
                    faceIndex >= 0 &&
                    faceIndex < faceRecords.Count)
                {
                    sum += faceRecords[faceIndex].Face.FeatureStrength;
                    count++;
                }
            }
            return count > 0 ? sum / count : 1f;
        }
        private static int
            CountChamferCorrectedPatchBoundaryOccurrenceFailures(
                List<ChamferProvisionalFaceRecord> patchRecords,
                ChamferHalfEdgeBoundaryComponent component,
                List<ChamferProvisionalHalfEdge> halfEdges,
                out ChamferBoundaryOccurrenceAudit audit)
        {
            audit = new ChamferBoundaryOccurrenceAudit();
            Dictionary<ChamferDirectedEdgeKey, int> directedBoundaryUses =
                new Dictionary<ChamferDirectedEdgeKey, int>();
            Dictionary<TopologyEdgeKey, int> undirectedUses =
                new Dictionary<TopologyEdgeKey, int>();
            Dictionary<ChamferDirectedEdgeKey, int> allDirectedUses =
                new Dictionary<ChamferDirectedEdgeKey, int>();
            for (int recordIndex = 0;
                 recordIndex < patchRecords.Count;
                 recordIndex++)
            {
                PolygonFace face = patchRecords[recordIndex].Face;
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 2)
                {
                    continue;
                }
                for (int edgeIndex = 0;
                     edgeIndex < face.Vertices.Count;
                     edgeIndex++)
                {
                    VertexKey startKey = new VertexKey(
                        face.Vertices[edgeIndex]);
                    VertexKey endKey = new VertexKey(
                        face.Vertices[(edgeIndex + 1) %
                            face.Vertices.Count]);
                    ChamferDirectedEdgeKey directed =
                        new ChamferDirectedEdgeKey(startKey, endKey);
                    TopologyEdgeKey undirected =
                        new TopologyEdgeKey(startKey, endKey);
                    allDirectedUses.TryGetValue(
                        directed,
                        out int directedCount);
                    allDirectedUses[directed] = directedCount + 1;
                    undirectedUses.TryGetValue(undirected, out int count);
                    undirectedUses[undirected] = count + 1;
                }
            }

            foreach (KeyValuePair<ChamferDirectedEdgeKey, int> pair
                     in allDirectedUses)
            {
                ChamferDirectedEdgeKey directed = pair.Key;
                TopologyEdgeKey undirected = new TopologyEdgeKey(
                    directed.Start,
                    directed.End);
                int totalUses = undirectedUses.TryGetValue(
                    undirected,
                    out int uses)
                    ? uses
                    : 0;
                ChamferDirectedEdgeKey reverse =
                    new ChamferDirectedEdgeKey(
                        directed.End,
                        directed.Start);
                int reverseUses = allDirectedUses.TryGetValue(
                    reverse,
                    out int reverseCount)
                    ? reverseCount
                    : 0;
                bool exactInternalPair =
                    totalUses == 2 &&
                    pair.Value == 1 &&
                    reverseUses == 1;
                if (exactInternalPair)
                {
                    continue;
                }
                directedBoundaryUses[directed] = pair.Value;
            }

            HashSet<ChamferDirectedEdgeKey> requiredOpposite =
                new HashSet<ChamferDirectedEdgeKey>();
            HashSet<ChamferDirectedEdgeKey> requiredSame =
                new HashSet<ChamferDirectedEdgeKey>();
            for (int i = 0;
                 i < component.OrderedHalfEdgeIndices.Count;
                 i++)
            {
                ChamferProvisionalHalfEdge existing = halfEdges[
                    component.OrderedHalfEdgeIndices[i]];
                ChamferDirectedEdgeKey opposite =
                    new ChamferDirectedEdgeKey(
                        existing.EndKey,
                        existing.StartKey);
                ChamferDirectedEdgeKey same =
                    new ChamferDirectedEdgeKey(
                        existing.StartKey,
                        existing.EndKey);
                requiredOpposite.Add(opposite);
                requiredSame.Add(same);
                int oppositeUses = directedBoundaryUses.TryGetValue(
                    opposite,
                    out int oppositeCount)
                    ? oppositeCount
                    : 0;
                int sameUses = directedBoundaryUses.TryGetValue(
                    same,
                    out int sameCount)
                    ? sameCount
                    : 0;
                if (oppositeUses == 0)
                {
                    if (sameUses > 0)
                    {
                        audit.DirectionMismatch += sameUses;
                    }
                    else
                    {
                        audit.MissingOpposite++;
                    }
                }
                else
                {
                    if (oppositeUses > 1)
                    {
                        audit.DuplicateOpposite += oppositeUses - 1;
                    }
                    if (sameUses > 0)
                    {
                        audit.DirectionMismatch += sameUses;
                    }
                }
            }

            foreach (KeyValuePair<ChamferDirectedEdgeKey, int> pair
                     in directedBoundaryUses)
            {
                if (requiredOpposite.Contains(pair.Key) ||
                    requiredSame.Contains(pair.Key))
                {
                    continue;
                }
                audit.ExtraPatchBoundary += pair.Value;
            }
            return audit.Total;
        }

        #endregion
    }
}
