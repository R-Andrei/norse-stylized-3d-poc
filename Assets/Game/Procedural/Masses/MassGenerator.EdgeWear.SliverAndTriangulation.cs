using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear sliver recovery, triangulation, and compact logging

        private static bool IsChamferSliverDiagnosticCandidate(
            ChamferVertexPatchLoop loop,
            List<Vector3> sourcePositions,
            List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
            List<PolygonFace> prePatchFaces,
            Dictionary<TopologyEdgeKey, int> prePatchUseCounts,
            HashSet<TopologyEdgeKey> finalSourceBoundaryEdges,
            float minimumPatchTriangleArea,
            float minimumStableEdgeLength)
        {
            if (loop.Kind !=
                    ChamferVertexPatchLoopKind.LocalClosedComponent ||
                sourcePositions.Count != 4 ||
                BuildChamferSliverEdgeCandidates(
                    loop,
                    prePatchFaceRecords).Count == 0)
            {
                return false;
            }

            List<Vector3> directedPositions =
                new List<Vector3>(sourcePositions);
            if (!TryBuildChamferDirectedBoundaryOwnership(
                    loop,
                    directedPositions,
                    prePatchFaces,
                    prePatchUseCounts,
                    out Dictionary<TopologyEdgeKey,
                        ChamferDirectedBoundaryEdge> boundaryOwnership,
                    out _,
                    out _))
            {
                return false;
            }

            Dictionary<int, List<List<ChamferDirectedTriangleIndex>>> memo =
                new Dictionary<int,
                    List<List<ChamferDirectedTriangleIndex>>>();
            List<List<ChamferDirectedTriangleIndex>> candidates =
                EnumerateChamferDirectedTriangulations(
                    0,
                    directedPositions.Count - 1,
                    directedPositions.Count,
                    memo);
            if (candidates.Count == 0)
            {
                return false;
            }

            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                ChamferDirectedTriangulationEvaluation evaluation =
                    EvaluateChamferDirectedTriangulation(
                        loop,
                        directedPositions,
                        candidates[candidateIndex],
                        boundaryOwnership,
                        prePatchFaces,
                        prePatchUseCounts,
                        finalSourceBoundaryEdges,
                        minimumPatchTriangleArea,
                        minimumStableEdgeLength);
                if (!evaluation.PassesIncidence ||
                    !evaluation.PassesTriangleIntersection ||
                    evaluation.PassesExistingFaceIntersection)
                {
                    return false;
                }
            }
            return true;
        }


        private static List<ChamferSliverEdgeCandidate>
            BuildChamferSliverEdgeCandidates(
                ChamferVertexPatchLoop loop,
                List<ChamferProvisionalFaceRecord> faceRecords)
        {
            List<ChamferSliverEdgeCandidate> candidates =
                new List<ChamferSliverEdgeCandidate>();
            for (int i = 0; i < loop.OrderedPositions.Count; i++)
            {
                Vector3 start = loop.OrderedPositions[i];
                Vector3 end = loop.OrderedPositions[
                    (i + 1) % loop.OrderedPositions.Count];
                float length = Vector3.Distance(start, end);
                if (length > PointMergeDistance)
                {
                    continue;
                }
                TopologyEdgeKey key = new TopologyEdgeKey(
                    new VertexKey(start),
                    new VertexKey(end));
                List<ChamferSliverEdgeOwner> owners =
                    FindChamferSliverEdgeOwners(key, faceRecords);
                candidates.Add(new ChamferSliverEdgeCandidate(
                    i,
                    start,
                    end,
                    key,
                    length,
                    owners));
            }
            candidates.Sort((left, right) =>
            {
                int lengthComparison = left.Length.CompareTo(right.Length);
                if (lengthComparison != 0)
                {
                    return lengthComparison;
                }
                return CompareTopologyEdgeKeys(left.Key, right.Key);
            });
            return candidates;
        }

        private static List<ChamferSliverEdgeOwner>
            FindChamferSliverEdgeOwners(
                TopologyEdgeKey key,
                List<ChamferProvisionalFaceRecord> faceRecords)
        {
            List<ChamferSliverEdgeOwner> owners =
                new List<ChamferSliverEdgeOwner>();
            for (int faceRecordIndex = 0;
                 faceRecordIndex < faceRecords.Count;
                 faceRecordIndex++)
            {
                ChamferProvisionalFaceRecord record =
                    faceRecords[faceRecordIndex];
                for (int cornerIndex = 0;
                     cornerIndex < record.Face.Vertices.Count;
                     cornerIndex++)
                {
                    Vector3 start = record.Face.Vertices[cornerIndex];
                    Vector3 end = record.Face.Vertices[
                        (cornerIndex + 1) % record.Face.Vertices.Count];
                    TopologyEdgeKey candidate = new TopologyEdgeKey(
                        new VertexKey(start),
                        new VertexKey(end));
                    if (candidate.Equals(key))
                    {
                        owners.Add(new ChamferSliverEdgeOwner(
                            faceRecordIndex,
                            cornerIndex,
                            record.Kind,
                            record.SourceFaceIndex,
                            record.SourceEdgeIndex));
                    }
                }
            }
            return owners;
        }

        private static ChamferSliverNormalizationResult
            EvaluateChamferVirtualSliverNormalization(
                ChamferVertexPatchLoop loop,
                ChamferSliverEdgeCandidate chosen,
                List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
                ChamferTopologyContext context,
                List<ChamferExpectedVertexBoundary> normalizedVertexBoundaries,
                List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
                bool useIndependentBoundarySectors,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                float minimumPatchTriangleArea,
                Dictionary<int, ChamferSharedEdgeSpan> sharedSpans)
        {
            ChamferSliverNormalizationResult result =
                new ChamferSliverNormalizationResult();
            VertexKey firstKey = new VertexKey(chosen.Start);
            VertexKey secondKey = new VertexKey(chosen.End);
            bool hasDecision = false;
            VertexKey representativeKey = default;
            Vector3 representativePosition = Vector3.zero;
            List<int> affectedFaceIndices = new List<int>();
            for (int faceIndex = 0;
                 faceIndex < prePatchFaceRecords.Count;
                 faceIndex++)
            {
                PolygonFace face = prePatchFaceRecords[faceIndex].Face;
                bool containsFirst = ChamferFaceContainsVertexKey(
                    face,
                    firstKey);
                bool containsSecond = ChamferFaceContainsVertexKey(
                    face,
                    secondKey);
                if (!containsFirst && !containsSecond)
                {
                    continue;
                }
                affectedFaceIndices.Add(faceIndex);
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
                    result.RepresentativeConflict = true;
                    result.Failure =
                        "incident-face-sanitation-has-no-single-survivor";
                    result.AffectedFaceIndices = affectedFaceIndices;
                    return result;
                }
                VertexKey faceRepresentative = firstSurvives
                    ? firstKey
                    : secondKey;
                Vector3 faceRepresentativePosition = firstSurvives
                    ? chosen.Start
                    : chosen.End;
                if (!hasDecision)
                {
                    hasDecision = true;
                    representativeKey = faceRepresentative;
                    representativePosition = faceRepresentativePosition;
                }
                else if (!representativeKey.Equals(faceRepresentative))
                {
                    result.RepresentativeConflict = true;
                    result.Failure =
                        "incident-faces-select-different-survivors";
                    result.AffectedFaceIndices = affectedFaceIndices;
                    return result;
                }
            }
            if (!hasDecision)
            {
                result.RepresentativeConflict = true;
                result.Failure = "no-incident-face-selected-a-survivor";
                result.AffectedFaceIndices = affectedFaceIndices;
                return result;
            }

            VertexKey removedKey = representativeKey.Equals(firstKey)
                ? secondKey
                : firstKey;
            result.RepresentativeKey = representativeKey;
            result.RepresentativePosition = representativePosition;
            result.RemovedKey = removedKey;
            result.AffectedFaceIndices = affectedFaceIndices;
            List<ChamferProvisionalFaceRecord> clonedRecords =
                CloneChamferProvisionalFaceRecords(prePatchFaceRecords);
            for (int faceIndex = 0;
                 faceIndex < clonedRecords.Count;
                 faceIndex++)
            {
                ChamferProvisionalFaceRecord record =
                    clonedRecords[faceIndex];
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
                    result.FaceFailureCount++;
                    continue;
                }
                Vector3 normal = CalculatePolygonNormal(sanitized);
                if (!IsFinite(normal) ||
                    Vector3.Dot(normal, record.Face.Normal) <= 0f)
                {
                    result.FaceFailureCount++;
                    continue;
                }
                record.Face = new PolygonFace(
                    sanitized,
                    normal,
                    record.Face.Feature,
                    record.Face.FeatureStrength);
            }
            if (result.FaceFailureCount > 0)
            {
                result.Failure = "post-collapse-face-validation-failure";
                return result;
            }
            result.Collapsed = true;

            List<ChamferExpectedVertexBoundary> clonedBoundaries =
                CloneAndRemapChamferExpectedVertexBoundaries(
                    normalizedVertexBoundaries,
                    removedKey,
                    representativePosition);
            List<ChamferSourceBoundaryRecord> clonedSourceBoundaries =
                CloneAndRemapChamferSourceBoundaryRecords(
                    sourceBoundaryRecords,
                    removedKey,
                    representativePosition);
            Dictionary<int, ChamferSharedEdgeSpan> clonedSharedSpans =
                CloneAndRemapChamferSharedEdgeSpans(
                    sharedSpans,
                    removedKey,
                    representativePosition);
            ChamferEmissionStats diagnosticStats = default;
            SegmentRawChamferTJunctions(
                clonedRecords,
                context,
                clonedSharedSpans,
                clonedBoundaries,
                clonedSourceBoundaries,
                minimumStableEdgeLength,
                ref diagnosticStats);
            result.PostSegmentationSplitCount =
                diagnosticStats.ProvenanceCompatibleTJunctionSplits;
            result.PostSegmentationPassCount =
                diagnosticStats.TJunctionSegmentationPasses;
            result.PostSegmentationIncompatibleTJunctions =
                diagnosticStats.TJunctionRecordsIncompatible;

            HashSet<TopologyEdgeKey> retraceRemovedKeys =
                new HashSet<TopologyEdgeKey>();
            if (!NormalizeChamferProvisionalFaceWalks(
                    clonedRecords,
                    minimumStableFaceArea,
                    retraceRemovedKeys,
                    ref diagnosticStats,
                    out string normalizationFailure))
            {
                result.TopologyFailureCount++;
                result.Failure = normalizationFailure;
                return result;
            }

            List<PolygonFace> clonedFaces =
                ExtractChamferProvisionalFaces(clonedRecords);
            Dictionary<TopologyEdgeKey, int> clonedUseCounts =
                BuildTopologyEdgeUseCounts(clonedFaces);
            RemoveRetraceDeletedChamferBoundaries(
                clonedBoundaries,
                clonedUseCounts,
                retraceRemovedKeys,
                ref diagnosticStats);
            HashSet<TopologyEdgeKey> sourceBoundarySegmentKeys =
                BuildChamferSourceBoundarySegmentKeys(
                    clonedSourceBoundaries);
            List<ChamferProvisionalSegmentRecord> rebuiltSegments =
                BuildChamferProvisionalSegmentRecords(
                    clonedRecords,
                    clonedBoundaries,
                    sourceBoundarySegmentKeys,
                    clonedSharedSpans);
            List<ChamferExpectedVertexBoundary> rebuiltBoundaries =
                NormalizeChamferVertexBoundaries(
                    clonedBoundaries,
                    clonedUseCounts,
                    rebuiltSegments,
                    ref diagnosticStats);
            Dictionary<TopologyEdgeKey,
                List<ChamferSourceBoundaryChildOccurrence>>
                rawSourceBoundaryOccurrences =
                    BuildChamferSourceBoundaryChildOccurrences(
                        clonedSourceBoundaries);
            NormalizeChamferSourceBoundaryLoops(
                clonedSourceBoundaries,
                clonedUseCounts,
                new HashSet<TopologyEdgeKey>(
                    BuildChamferExpectedBoundaryKeySet(
                        rebuiltBoundaries)),
                rebuiltSegments,
                ref diagnosticStats);
            CollapseChamferSourceBoundaryTerminalTransferAliases(
                clonedSourceBoundaries,
                rawSourceBoundaryOccurrences,
                clonedUseCounts,
                new HashSet<TopologyEdgeKey>(
                    BuildChamferExpectedBoundaryKeySet(
                        rebuiltBoundaries)),
                rebuiltSegments,
                ref diagnosticStats);
            HashSet<TopologyEdgeKey> diagnosticSourceBoundaryEdges =
                AuditChamferSourceBoundaryOwnership(
                    clonedSourceBoundaries,
                    clonedUseCounts,
                    new HashSet<TopologyEdgeKey>(
                        BuildChamferExpectedBoundaryKeySet(
                            rebuiltBoundaries)),
                    rebuiltSegments,
                    rawSourceBoundaryOccurrences,
                    ref diagnosticStats);
            result.RebuiltSegmentCount = rebuiltSegments.Count;

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                clonedFaces,
                minimumStableEdgeLength);
            result.PostCollapseNonManifoldEdges =
                topology.NonManifoldEdgeCount;
            result.PostCollapseTJunctions = topology.TJunctionCount;
            if (topology.NonManifoldEdgeCount > 0 ||
                topology.TJunctionCount > 0 ||
                diagnosticStats.TJunctionRecordsIncompatible > 0)
            {
                result.TopologyFailureCount++;
            }

            List<Vector3> remappedLoopPositions =
                new List<Vector3>(loop.OrderedPositions.Count);
            for (int i = 0; i < loop.OrderedPositions.Count; i++)
            {
                Vector3 position = loop.OrderedPositions[i];
                remappedLoopPositions.Add(
                    new VertexKey(position).Equals(removedKey)
                        ? representativePosition
                        : position);
            }
            remappedLoopPositions = SanitizePolygon(
                remappedLoopPositions,
                loop.ExpectedNormal);
            List<TopologyEdgeKey> remappedLoopKeys =
                BuildChamferOrderedBoundaryKeys(
                    remappedLoopPositions);
            ChamferHalfEdgeDecomposition postCollapseDecomposition =
                BuildChamferSliverHalfEdgeDecomposition(
                    clonedRecords,
                    useIndependentBoundarySectors);
            HashSet<int> postCollapseComponentIds =
                new HashSet<int>();
            bool[] parentEdgeMatched =
                new bool[remappedLoopPositions.Count];
            for (int boundaryIndex = 0;
                 boundaryIndex <
                    postCollapseDecomposition.BoundaryHalfEdges.Count;
                 boundaryIndex++)
            {
                int halfEdgeIndex =
                    postCollapseDecomposition.BoundaryHalfEdges[
                        boundaryIndex];
                ChamferProvisionalHalfEdge boundaryHalfEdge =
                    postCollapseDecomposition.HalfEdges[halfEdgeIndex];
                bool belongsToLoop = false;
                for (int parentIndex = 0;
                     parentIndex < remappedLoopPositions.Count;
                     parentIndex++)
                {
                    Vector3 parentStart =
                        remappedLoopPositions[parentIndex];
                    Vector3 parentEnd = remappedLoopPositions[
                        (parentIndex + 1) %
                        remappedLoopPositions.Count];
                    if (!IsChamferBoundaryHalfEdgeDescendantOfSegment(
                            boundaryHalfEdge,
                            parentStart,
                            parentEnd,
                            minimumStableEdgeLength))
                    {
                        continue;
                    }
                    parentEdgeMatched[parentIndex] = true;
                    belongsToLoop = true;
                    break;
                }
                if (belongsToLoop &&
                    postCollapseDecomposition.ComponentByHalfEdge.
                        TryGetValue(
                            halfEdgeIndex,
                            out int componentId))
                {
                    postCollapseComponentIds.Add(componentId);
                }
            }
            for (int parentIndex = 0;
                 parentIndex < parentEdgeMatched.Length;
                 parentIndex++)
            {
                if (!parentEdgeMatched[parentIndex])
                {
                    result.TopologyFailureCount++;
                }
            }
            if (remappedLoopKeys.Count == 0)
            {
                result.PostCollapseBoundaryPositions =
                    remappedLoopPositions;
                result.PostCollapseBoundaryKeys =
                    remappedLoopKeys;
            }
            else if (postCollapseComponentIds.Count == 1)
            {
                int componentId = GetOnlyChamferInteger(
                    postCollapseComponentIds);
                ChamferHalfEdgeBoundaryComponent component =
                    postCollapseDecomposition.Components[componentId];
                bool componentContained =
                    IsChamferBoundaryComponentContainedBySegments(
                        component,
                        postCollapseDecomposition.HalfEdges,
                        remappedLoopPositions,
                        minimumStableEdgeLength);
                if (component.Kind !=
                        ChamferHalfEdgeBoundaryComponentKind.Loop ||
                    !componentContained)
                {
                    result.TopologyFailureCount++;
                    result.PostCollapseBoundaryPositions =
                        remappedLoopPositions;
                    result.PostCollapseBoundaryKeys =
                        remappedLoopKeys;
                }
                else
                {
                    result.PostCollapseBoundaryPositions =
                        BuildChamferOppositeBoundaryPositions(
                            component,
                            postCollapseDecomposition.HalfEdges);
                    result.PostCollapseBoundaryKeys =
                        BuildChamferOrderedBoundaryKeys(
                            result.PostCollapseBoundaryPositions);
                }
            }
            else
            {
                result.TopologyFailureCount++;
                result.PostCollapseBoundaryPositions =
                    remappedLoopPositions;
                result.PostCollapseBoundaryKeys =
                    remappedLoopKeys;
            }

            if (result.PostCollapseBoundaryPositions.Count < 3)
            {
                result.Resolution =
                    result.TopologyFailureCount == 0
                        ? ChamferSliverResolution.ResolvedByElimination
                        : ChamferSliverResolution.Unresolved;
                result.Failure = result.TopologyFailureCount == 0
                    ? string.Empty
                    : "eliminated-boundary-has-topology-failures";
                return result;
            }
            if (result.PostCollapseBoundaryPositions.Count != 3)
            {
                result.Resolution = ChamferSliverResolution.Unresolved;
                result.Failure =
                    "post-collapse-boundary-is-not-a-triangle";
                return result;
            }

            ChamferVertexPatchLoop diagnosticLoop =
                new ChamferVertexPatchLoop(
                    loop.LoopIndex,
                    loop.Kind,
                    new List<Vector3>(
                        result.PostCollapseBoundaryPositions),
                    new List<TopologyEdgeKey>(
                        result.PostCollapseBoundaryKeys),
                    loop.Components,
                    loop.ComponentExpectedNormals,
                    loop.ExpectedNormal,
                    loop.FeatureStrength);
            List<Vector3> directedPositions =
                new List<Vector3>(
                    result.PostCollapseBoundaryPositions);
            if (!TryBuildChamferDirectedBoundaryOwnership(
                    diagnosticLoop,
                    directedPositions,
                    clonedFaces,
                    clonedUseCounts,
                    out Dictionary<TopologyEdgeKey,
                        ChamferDirectedBoundaryEdge> boundaryOwnership,
                    out _,
                    out string ownershipFailure))
            {
                result.TopologyFailureCount++;
                result.Failure = ownershipFailure;
                return result;
            }
            List<ChamferDirectedTriangleIndex> triangle =
                new List<ChamferDirectedTriangleIndex>
                {
                    new ChamferDirectedTriangleIndex(0, 1, 2)
                };
            ChamferDirectedTriangulationEvaluation evaluation =
                EvaluateChamferDirectedTriangulation(
                    diagnosticLoop,
                    directedPositions,
                    triangle,
                    boundaryOwnership,
                    clonedFaces,
                    clonedUseCounts,
                    diagnosticSourceBoundaryEdges,
                    minimumPatchTriangleArea,
                    minimumStableEdgeLength);
            result.TriangleEvaluation = evaluation;
            if (!evaluation.PassesExistingFaceIntersection)
            {
                result.IntersectionFailureCount++;
            }
            if (evaluation.TJunctionFailure ||
                evaluation.NonManifoldFailure ||
                !evaluation.PassesIncidence ||
                !evaluation.PassesTriangleIntersection)
            {
                result.TopologyFailureCount++;
            }
            if (evaluation.Feasible &&
                result.TopologyFailureCount == 0 &&
                result.IntersectionFailureCount == 0)
            {
                result.Resolution =
                    ChamferSliverResolution.ResolvedToTriangle;
                result.Failure = string.Empty;
            }
            else
            {
                result.Resolution = ChamferSliverResolution.Unresolved;
                result.Failure = evaluation.Failure;
            }
            return result;
        }

        private static bool IsChamferBoundaryHalfEdgeDescendantOfSegment(
            ChamferProvisionalHalfEdge halfEdge,
            Vector3 parentStart,
            Vector3 parentEnd,
            float minimumStableEdgeLength)
        {
            float tolerance = CalculateTopologyTJunctionTolerance(
                minimumStableEdgeLength);
            return IsChamferPointOnClosedSegment(
                    halfEdge.StartPosition,
                    parentStart,
                    parentEnd,
                    tolerance) &&
                IsChamferPointOnClosedSegment(
                    halfEdge.EndPosition,
                    parentStart,
                    parentEnd,
                    tolerance);
        }

        private static bool IsChamferPointOnClosedSegment(
            Vector3 point,
            Vector3 start,
            Vector3 end,
            float tolerance)
        {
            Vector3 segment = end - start;
            float lengthSqr = segment.sqrMagnitude;
            if (lengthSqr <= MinimumEdgeLengthSqr)
            {
                return (point - start).sqrMagnitude <=
                    tolerance * tolerance;
            }
            float t = Vector3.Dot(point - start, segment) / lengthSqr;
            float parameterTolerance = tolerance /
                Mathf.Sqrt(lengthSqr);
            if (t < -parameterTolerance ||
                t > 1f + parameterTolerance)
            {
                return false;
            }
            Vector3 closest = start + segment * Mathf.Clamp01(t);
            return (point - closest).sqrMagnitude <=
                tolerance * tolerance;
        }

        private static bool IsChamferBoundaryComponentContainedBySegments(
            ChamferHalfEdgeBoundaryComponent component,
            List<ChamferProvisionalHalfEdge> halfEdges,
            List<Vector3> parentPositions,
            float minimumStableEdgeLength)
        {
            for (int edgeIndex = 0;
                 edgeIndex < component.OrderedHalfEdgeIndices.Count;
                 edgeIndex++)
            {
                ChamferProvisionalHalfEdge halfEdge = halfEdges[
                    component.OrderedHalfEdgeIndices[edgeIndex]];
                bool contained = false;
                for (int parentIndex = 0;
                     parentIndex < parentPositions.Count;
                     parentIndex++)
                {
                    if (IsChamferBoundaryHalfEdgeDescendantOfSegment(
                            halfEdge,
                            parentPositions[parentIndex],
                            parentPositions[(parentIndex + 1) %
                                parentPositions.Count],
                            minimumStableEdgeLength))
                    {
                        contained = true;
                        break;
                    }
                }
                if (!contained)
                {
                    return false;
                }
            }
            return true;
        }

        private static HashSet<TopologyEdgeKey>
            BuildChamferExpectedBoundaryKeySet(
                List<ChamferExpectedVertexBoundary> boundaries)
        {
            HashSet<TopologyEdgeKey> keys =
                new HashSet<TopologyEdgeKey>();
            for (int i = 0; i < boundaries.Count; i++)
            {
                keys.Add(boundaries[i].Key);
            }
            return keys;
        }

        private static List<ChamferExpectedVertexBoundary>
            CloneAndRemapChamferExpectedVertexBoundaries(
                List<ChamferExpectedVertexBoundary> source,
                VertexKey removedKey,
                Vector3 representativePosition)
        {
            List<ChamferExpectedVertexBoundary> result =
                new List<ChamferExpectedVertexBoundary>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                ChamferExpectedVertexBoundary boundary = source[i];
                Vector3 start = RemapChamferDiagnosticPosition(
                    boundary.Start,
                    removedKey,
                    representativePosition);
                Vector3 end = RemapChamferDiagnosticPosition(
                    boundary.End,
                    removedKey,
                    representativePosition);
                VertexKey startKey = new VertexKey(start);
                VertexKey endKey = new VertexKey(end);
                if (startKey.Equals(endKey))
                {
                    continue;
                }
                result.Add(new ChamferExpectedVertexBoundary(
                    boundary.SourceVertexIndex,
                    boundary.SourceEdgeIndex,
                    boundary.FaceIndex,
                    boundary.Kind,
                    start,
                    end,
                    new TopologyEdgeKey(startKey, endKey)));
            }
            return result;
        }

        private static List<ChamferSourceBoundaryRecord>
            CloneChamferSourceBoundaryRecords(
                List<ChamferSourceBoundaryRecord> source)
        {
            List<ChamferSourceBoundaryRecord> result =
                new List<ChamferSourceBoundaryRecord>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                ChamferSourceBoundaryRecord record = source[i];
                ChamferSourceBoundaryRecord clone =
                    new ChamferSourceBoundaryRecord(
                        record.SourceEdgeIndex,
                        record.BoundaryLoopIndex,
                        record.BoundaryOrder,
                        record.SourceVertexStart,
                        record.SourceVertexEnd,
                        record.ParentStart,
                        record.ParentEnd);
                clone.Children.Clear();
                clone.Children.AddRange(record.Children);
                clone.RemovedChildren.Clear();
                clone.RemovedChildren.AddRange(
                    record.RemovedChildren);
                clone.RawChildCount = record.RawChildCount;
                clone.PostLoopNormalizationChildCount =
                    record.PostLoopNormalizationChildCount;
                clone.PostTerminalAliasChildCount =
                    record.PostTerminalAliasChildCount;
                result.Add(clone);
            }
            return result;
        }

        private static List<ChamferSourceBoundaryRecord>
            CloneAndRemapChamferSourceBoundaryRecords(
                List<ChamferSourceBoundaryRecord> source,
                VertexKey removedKey,
                Vector3 representativePosition)
        {
            List<ChamferSourceBoundaryRecord> result =
                new List<ChamferSourceBoundaryRecord>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                ChamferSourceBoundaryRecord record = source[i];
                Vector3 parentStart = RemapChamferDiagnosticPosition(
                    record.ParentStart,
                    removedKey,
                    representativePosition);
                Vector3 parentEnd = RemapChamferDiagnosticPosition(
                    record.ParentEnd,
                    removedKey,
                    representativePosition);
                ChamferSourceBoundaryRecord clone =
                    new ChamferSourceBoundaryRecord(
                        record.SourceEdgeIndex,
                        record.BoundaryLoopIndex,
                        record.BoundaryOrder,
                        record.SourceVertexStart,
                        record.SourceVertexEnd,
                        parentStart,
                        parentEnd);
                clone.Children.Clear();
                for (int childIndex = 0;
                     childIndex < record.Children.Count;
                     childIndex++)
                {
                    ChamferSourceBoundaryChild child =
                        record.Children[childIndex];
                    Vector3 start = RemapChamferDiagnosticPosition(
                        child.Start,
                        removedKey,
                        representativePosition);
                    Vector3 end = RemapChamferDiagnosticPosition(
                        child.End,
                        removedKey,
                        representativePosition);
                    if (new VertexKey(start).Equals(new VertexKey(end)))
                    {
                        continue;
                    }
                    clone.Children.Add(new ChamferSourceBoundaryChild(
                        start,
                        end,
                        child.TouchesParentStart,
                        child.TouchesParentEnd));
                }
                clone.RemovedChildren.Clear();
                for (int removalIndex = 0;
                     removalIndex < record.RemovedChildren.Count;
                     removalIndex++)
                {
                    ChamferSourceBoundaryChildRemoval removal =
                        record.RemovedChildren[removalIndex];
                    clone.RemovedChildren.Add(
                        new ChamferSourceBoundaryChildRemoval(
                            RemapChamferDiagnosticPosition(
                                removal.Start,
                                removedKey,
                                representativePosition),
                            RemapChamferDiagnosticPosition(
                                removal.End,
                                removedKey,
                                representativePosition),
                            removal.Stage,
                            removal.PartnerSourceEdgeIndex,
                            removal.PartnerBoundaryOrder));
                }
                clone.RawChildCount = record.RawChildCount;
                clone.PostLoopNormalizationChildCount =
                    record.PostLoopNormalizationChildCount;
                clone.PostTerminalAliasChildCount =
                    record.PostTerminalAliasChildCount;
                result.Add(clone);
            }
            return result;
        }

        private static Dictionary<int, ChamferSharedEdgeSpan>
            CloneAndRemapChamferSharedEdgeSpans(
                Dictionary<int, ChamferSharedEdgeSpan> source,
                VertexKey removedKey,
                Vector3 representativePosition)
        {
            Dictionary<int, ChamferSharedEdgeSpan> result =
                new Dictionary<int, ChamferSharedEdgeSpan>(source.Count);
            foreach (KeyValuePair<int, ChamferSharedEdgeSpan> pair
                     in source)
            {
                ChamferSharedEdgeSpan span = pair.Value;
                result.Add(
                    pair.Key,
                    new ChamferSharedEdgeSpan(
                        span.SourceEdgeIndex,
                        span.FaceA,
                        span.FaceB,
                        span.VertexA,
                        span.VertexB,
                        RemapChamferDiagnosticPosition(
                            span.SharedAtVertexA,
                            removedKey,
                            representativePosition),
                        RemapChamferDiagnosticPosition(
                            span.SharedAtVertexB,
                            removedKey,
                            representativePosition)));
            }
            return result;
        }

        private static Vector3 RemapChamferDiagnosticPosition(
            Vector3 position,
            VertexKey removedKey,
            Vector3 representativePosition)
        {
            return new VertexKey(position).Equals(removedKey)
                ? representativePosition
                : position;
        }

        private static ChamferHalfEdgeDecomposition
            BuildChamferSliverHalfEdgeDecomposition(
                List<ChamferProvisionalFaceRecord> faceRecords,
                bool useIndependentBoundarySectors)
        {
            return BuildChamferAuthoritativeHalfEdgeDecomposition(
                faceRecords,
                useIndependentBoundarySectors);
        }

        private static bool ChamferFaceContainsVertexKey(
            PolygonFace face,
            VertexKey key)
        {
            for (int i = 0; i < face.Vertices.Count; i++)
            {
                if (new VertexKey(face.Vertices[i]).Equals(key))
                {
                    return true;
                }
            }
            return false;
        }

        private static List<ChamferTrackedSanitizeVertex>
            SanitizeChamferTrackedPolygon(
                List<Vector3> vertices,
                Vector3 normal)
        {
            List<ChamferTrackedSanitizeVertex> result =
                new List<ChamferTrackedSanitizeVertex>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                result.Add(new ChamferTrackedSanitizeVertex(
                    vertices[i],
                    new VertexKey(vertices[i]),
                    i));
            }
            bool changed = true;
            int safetyPass = 0;
            while (changed && result.Count >= 3 && safetyPass < 12)
            {
                changed = false;
                safetyPass++;
                RemoveClosingChamferTrackedDuplicate(result);
                for (int i = result.Count - 1;
                     i >= 0 && result.Count >= 3;
                     i--)
                {
                    int previousIndex =
                        (i - 1 + result.Count) % result.Count;
                    int nextIndex = (i + 1) % result.Count;
                    Vector3 previousEdge =
                        result[i].Position -
                        result[previousIndex].Position;
                    Vector3 nextEdge =
                        result[nextIndex].Position -
                        result[i].Position;
                    float previousEdgeLengthSqr =
                        previousEdge.sqrMagnitude;
                    float nextEdgeLengthSqr = nextEdge.sqrMagnitude;
                    if (previousEdgeLengthSqr <= PointMergeDistanceSqr ||
                        nextEdgeLengthSqr <= PointMergeDistanceSqr)
                    {
                        result.RemoveAt(i);
                        changed = true;
                        continue;
                    }
                    float maximumAdjacentEdgeLengthSqr = Mathf.Max(
                        previousEdgeLengthSqr,
                        nextEdgeLengthSqr);
                    float turnAreaSqr = Vector3.Cross(
                        previousEdge,
                        nextEdge).sqrMagnitude;
                    float relativeCollinearThreshold =
                        maximumAdjacentEdgeLengthSqr *
                        maximumAdjacentEdgeLengthSqr *
                        RelativeCollinearEpsilon;
                    if (turnAreaSqr <= relativeCollinearThreshold)
                    {
                        result.RemoveAt(i);
                        changed = true;
                    }
                }
            }
            RemoveClosingChamferTrackedDuplicate(result);
            if (result.Count >= 3)
            {
                List<Vector3> positions = new List<Vector3>(result.Count);
                for (int i = 0; i < result.Count; i++)
                {
                    positions.Add(result[i].Position);
                }
                if (Vector3.Dot(
                        CalculatePolygonNormal(positions),
                        normal) < 0f)
                {
                    result.Reverse();
                }
            }
            return result;
        }

        private static void RemoveClosingChamferTrackedDuplicate(
            List<ChamferTrackedSanitizeVertex> vertices)
        {
            if (vertices.Count < 2)
            {
                return;
            }
            if ((vertices[0].Position -
                 vertices[vertices.Count - 1].Position).sqrMagnitude <=
                PointMergeDistanceSqr)
            {
                vertices.RemoveAt(vertices.Count - 1);
            }
        }

        private static bool ChamferTrackedPolygonContainsOriginalKey(
            List<ChamferTrackedSanitizeVertex> vertices,
            VertexKey key)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].OriginalKey.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }

        private static List<Vector3>
            BuildChamferOppositeBoundaryPositions(
                ChamferHalfEdgeBoundaryComponent component,
                List<ChamferProvisionalHalfEdge> halfEdges)
        {
            List<Vector3> positions = new List<Vector3>(
                component.OrderedHalfEdgeIndices.Count);
            for (int i = component.OrderedHalfEdgeIndices.Count - 1;
                 i >= 0;
                 i--)
            {
                positions.Add(halfEdges[
                    component.OrderedHalfEdgeIndices[i]].EndPosition);
            }
            return positions;
        }

        private static List<TopologyEdgeKey> BuildChamferOrderedBoundaryKeys(
            List<Vector3> positions)
        {
            List<TopologyEdgeKey> keys =
                new List<TopologyEdgeKey>();
            if (positions == null || positions.Count < 2)
            {
                return keys;
            }
            for (int i = 0; i < positions.Count; i++)
            {
                VertexKey start = new VertexKey(positions[i]);
                VertexKey end = new VertexKey(positions[
                    (i + 1) % positions.Count]);
                if (!start.Equals(end))
                {
                    keys.Add(new TopologyEdgeKey(start, end));
                }
            }
            return keys;
        }


        private static bool AuditChamferPatchTriangulationFeasibility(
            ChamferVertexPatchLoop loop,
            List<Vector3> sourcePositions,
            float minimumPatchTriangleArea,
            ref ChamferEmissionStats stats)
        {
            int boundaryCount = sourcePositions.Count;
            long candidateCount = CountChamferPatchTriangulations(
                boundaryCount);
            stats.PatchTriangulationCandidatesTested =
                SaturatingAddLong(
                    stats.PatchTriangulationCandidatesTested,
                    candidateCount);

            List<Vector3> positions =
                new List<Vector3>(sourcePositions);
            bool hasOrderedNormal =
                TryCalculateChamferPatchOrderedNormal(
                    positions,
                    out Vector3 orderedNormal);
            if (hasOrderedNormal &&
                Vector3.Dot(
                    orderedNormal,
                    loop.ExpectedNormal) < 0f)
            {
                positions.Reverse();
                hasOrderedNormal =
                    TryCalculateChamferPatchOrderedNormal(
                        positions,
                        out orderedNormal);
            }

            List<ChamferPatchTriangulationCandidate> feasible =
                new List<ChamferPatchTriangulationCandidate>();
            if (hasOrderedNormal &&
                Vector3.Dot(
                    orderedNormal,
                    loop.ExpectedNormal) > 0f)
            {
                Dictionary<int, List<ChamferPatchTriangulationCandidate>>
                    memo = new Dictionary<
                        int,
                        List<ChamferPatchTriangulationCandidate>>();
                feasible = EnumerateChamferPatchTriangulations(
                    loop,
                    positions,
                    0,
                    boundaryCount - 1,
                    boundaryCount,
                    minimumPatchTriangleArea,
                    memo);
            }

            stats.PatchFeasibleTriangulationCount =
                SaturatingAddLong(
                    stats.PatchFeasibleTriangulationCount,
                    feasible.Count);
            ChamferPatchTriangulationCandidate best = null;
            for (int i = 0; i < feasible.Count; i++)
            {
                if (best == null ||
                    CompareChamferPatchTriangulationCandidates(
                        feasible[i],
                        best) < 0)
                {
                    best = feasible[i];
                }
            }

            return best != null;
        }

        private static List<ChamferPatchTriangulationCandidate>
            EnumerateChamferPatchTriangulations(
                ChamferVertexPatchLoop loop,
                List<Vector3> positions,
                int startIndex,
                int endIndex,
                int boundaryCount,
                float minimumPatchTriangleArea,
                Dictionary<int, List<ChamferPatchTriangulationCandidate>> memo)
        {
            int memoKey = startIndex * boundaryCount + endIndex;
            if (memo.TryGetValue(
                    memoKey,
                    out List<ChamferPatchTriangulationCandidate> cached))
            {
                return cached;
            }

            List<ChamferPatchTriangulationCandidate> result =
                new List<ChamferPatchTriangulationCandidate>();
            if (endIndex <= startIndex + 1)
            {
                result.Add(new ChamferPatchTriangulationCandidate(
                    float.PositiveInfinity,
                    float.PositiveInfinity,
                    float.PositiveInfinity,
                    new List<int>()));
                memo[memoKey] = result;
                return result;
            }

            for (int splitIndex = startIndex + 1;
                 splitIndex < endIndex;
                 splitIndex++)
            {
                if (!TryMeasureChamferPatchTriangle(
                        loop,
                        positions[startIndex],
                        positions[splitIndex],
                        positions[endIndex],
                        minimumPatchTriangleArea,
                        out float triangleArea,
                        out float triangleQuality,
                        out float triangleAlignment))
                {
                    continue;
                }

                List<ChamferPatchTriangulationCandidate> left =
                    EnumerateChamferPatchTriangulations(
                        loop,
                        positions,
                        startIndex,
                        splitIndex,
                        boundaryCount,
                        minimumPatchTriangleArea,
                        memo);
                List<ChamferPatchTriangulationCandidate> right =
                    EnumerateChamferPatchTriangulations(
                        loop,
                        positions,
                        splitIndex,
                        endIndex,
                        boundaryCount,
                        minimumPatchTriangleArea,
                        memo);
                for (int leftIndex = 0;
                     leftIndex < left.Count;
                     leftIndex++)
                {
                    for (int rightIndex = 0;
                         rightIndex < right.Count;
                         rightIndex++)
                    {
                        HashSet<int> diagonalCodes =
                            new HashSet<int>(
                                left[leftIndex].DiagonalCodes);
                        diagonalCodes.UnionWith(
                            right[rightIndex].DiagonalCodes);
                        AddChamferPatchDiagonalCode(
                            diagonalCodes,
                            startIndex,
                            splitIndex,
                            boundaryCount);
                        AddChamferPatchDiagonalCode(
                            diagonalCodes,
                            splitIndex,
                            endIndex,
                            boundaryCount);
                        List<int> orderedDiagonalCodes =
                            new List<int>(diagonalCodes);
                        orderedDiagonalCodes.Sort();

                        result.Add(
                            new ChamferPatchTriangulationCandidate(
                                Mathf.Min(
                                    triangleArea,
                                    Mathf.Min(
                                        left[leftIndex].MinimumArea,
                                        right[rightIndex].MinimumArea)),
                                Mathf.Min(
                                    triangleQuality,
                                    Mathf.Min(
                                        left[leftIndex].MinimumQuality,
                                        right[rightIndex].MinimumQuality)),
                                Mathf.Min(
                                    triangleAlignment,
                                    Mathf.Min(
                                        left[leftIndex].MinimumAlignment,
                                        right[rightIndex].MinimumAlignment)),
                                orderedDiagonalCodes));
                    }
                }
            }
            memo[memoKey] = result;
            return result;
        }

        private static bool TryMeasureChamferPatchTriangle(
            ChamferVertexPatchLoop loop,
            Vector3 first,
            Vector3 second,
            Vector3 third,
            float minimumPatchTriangleArea,
            out float area,
            out float quality,
            out float alignment)
        {
            area = 0f;
            quality = 0f;
            alignment = 0f;
            if (!IsFinite(first) ||
                !IsFinite(second) ||
                !IsFinite(third))
            {
                return false;
            }
            HashSet<VertexKey> keys = new HashSet<VertexKey>
            {
                new VertexKey(first),
                new VertexKey(second),
                new VertexKey(third)
            };
            if (keys.Count != 3)
            {
                return false;
            }
            List<Vector3> triangle = new List<Vector3>
            {
                first,
                second,
                third
            };
            area = CalculatePolygonArea(triangle);
            if (!IsFiniteFloat(area) ||
                area <= minimumPatchTriangleArea)
            {
                return false;
            }
            if (!TryCalculateChamferPatchTriangleNormal(
                    first,
                    second,
                    third,
                    out Vector3 normal))
            {
                return false;
            }
            alignment = Vector3.Dot(
                normal,
                loop.ExpectedNormal);
            if (!IsFiniteFloat(alignment) || alignment <= 0f)
            {
                return false;
            }
            float firstLengthSquared = (second - first).sqrMagnitude;
            float secondLengthSquared = (third - second).sqrMagnitude;
            float thirdLengthSquared = (first - third).sqrMagnitude;
            float denominator = firstLengthSquared +
                secondLengthSquared +
                thirdLengthSquared;
            if (!IsFiniteFloat(denominator) || denominator <= 0f)
            {
                return false;
            }
            quality = 4f * Mathf.Sqrt(3f) * area / denominator;
            return IsFiniteFloat(quality) && quality > 0f;
        }

        private static void AddChamferPatchDiagonalCode(
            HashSet<int> diagonalCodes,
            int firstIndex,
            int secondIndex,
            int boundaryCount)
        {
            int minimum = Mathf.Min(firstIndex, secondIndex);
            int maximum = Mathf.Max(firstIndex, secondIndex);
            bool boundary = maximum == minimum + 1 ||
                (minimum == 0 && maximum == boundaryCount - 1);
            if (!boundary)
            {
                diagonalCodes.Add(minimum * boundaryCount + maximum);
            }
        }

        private static int CompareChamferPatchTriangulationCandidates(
            ChamferPatchTriangulationCandidate left,
            ChamferPatchTriangulationCandidate right)
        {
            int qualityComparison = right.MinimumQuality.CompareTo(
                left.MinimumQuality);
            if (qualityComparison != 0)
            {
                return qualityComparison;
            }
            int alignmentComparison = right.MinimumAlignment.CompareTo(
                left.MinimumAlignment);
            if (alignmentComparison != 0)
            {
                return alignmentComparison;
            }
            int areaComparison = right.MinimumArea.CompareTo(
                left.MinimumArea);
            if (areaComparison != 0)
            {
                return areaComparison;
            }
            int count = Mathf.Min(
                left.DiagonalCodes.Count,
                right.DiagonalCodes.Count);
            for (int i = 0; i < count; i++)
            {
                int comparison = left.DiagonalCodes[i].CompareTo(
                    right.DiagonalCodes[i]);
                if (comparison != 0)
                {
                    return comparison;
                }
            }
            return left.DiagonalCodes.Count.CompareTo(
                right.DiagonalCodes.Count);
        }

        private static long CountChamferPatchTriangulations(
            int boundaryCount)
        {
            int catalanIndex = Mathf.Max(0, boundaryCount - 2);
            long[] catalan = new long[catalanIndex + 1];
            catalan[0] = 1L;
            for (int i = 1; i <= catalanIndex; i++)
            {
                long value = 0L;
                for (int left = 0; left < i; left++)
                {
                    value = SaturatingAddLong(
                        value,
                        SaturatingMultiplyLong(
                            catalan[left],
                            catalan[i - 1 - left]));
                }
                catalan[i] = value;
            }
            return catalan[catalanIndex];
        }

        private static long SaturatingAddLong(long first, long second)
        {
            if (first >= long.MaxValue - second)
            {
                return long.MaxValue;
            }
            return first + second;
        }

        private static long SaturatingMultiplyLong(long first, long second)
        {
            if (first == 0L || second == 0L)
            {
                return 0L;
            }
            if (first > long.MaxValue / second)
            {
                return long.MaxValue;
            }
            return first * second;
        }





        private static int CompareTopologyEdgeKeys(
            TopologyEdgeKey left,
            TopologyEdgeKey right)
        {
            int firstComparison = left.First.CompareTo(right.First);
            return firstComparison != 0
                ? firstComparison
                : left.Second.CompareTo(right.Second);
        }

        private static bool IsFiniteFloat(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float CalculateChamferPatchSignedArea(
            List<Vector2> points)
        {
            float twiceArea = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % points.Count];
                twiceArea += current.x * next.y - next.x * current.y;
            }
            return twiceArea * 0.5f;
        }

        private static float ChamferPatchCross2D(
            Vector2 first,
            Vector2 second,
            Vector2 third)
        {
            Vector2 firstEdge = second - first;
            Vector2 secondEdge = third - first;
            return firstEdge.x * secondEdge.y -
                firstEdge.y * secondEdge.x;
        }

        private static bool ChamferPatchPointInOrOnTriangle(
            Vector2 point,
            Vector2 first,
            Vector2 second,
            Vector2 third,
            float epsilon)
        {
            float firstCross = ChamferPatchCross2D(
                first,
                second,
                point);
            float secondCross = ChamferPatchCross2D(
                second,
                third,
                point);
            float thirdCross = ChamferPatchCross2D(
                third,
                first,
                point);
            float areaEpsilon = epsilon * epsilon;
            return firstCross >= -areaEpsilon &&
                secondCross >= -areaEpsilon &&
                thirdCross >= -areaEpsilon;
        }

        private static bool ChamferPatchPolygonSelfIntersects(
            List<Vector2> points,
            float epsilon,
            out ChamferPatchIntersectionEvidence evidence)
        {
            evidence = default;
            for (int firstEdge = 0;
                 firstEdge < points.Count;
                 firstEdge++)
            {
                int firstNext = (firstEdge + 1) % points.Count;
                for (int secondEdge = firstEdge + 1;
                     secondEdge < points.Count;
                     secondEdge++)
                {
                    int secondNext = (secondEdge + 1) % points.Count;
                    if (firstEdge == secondEdge ||
                        firstNext == secondEdge ||
                        secondNext == firstEdge)
                    {
                        continue;
                    }
                    if (TryGetChamferPatchSegmentIntersectionEvidence(
                            points[firstEdge],
                            points[firstNext],
                            points[secondEdge],
                            points[secondNext],
                            epsilon,
                            firstEdge,
                            secondEdge,
                            out evidence))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool ChamferPatchDiagonalIntersectsRemainingBoundary(
            int firstIndex,
            int secondIndex,
            List<int> remaining,
            List<Vector2> projected,
            float epsilon)
        {
            Vector2 first = projected[firstIndex];
            Vector2 second = projected[secondIndex];
            for (int edgeIndex = 0;
                 edgeIndex < remaining.Count;
                 edgeIndex++)
            {
                int edgeStartIndex = remaining[edgeIndex];
                int edgeEndIndex = remaining[
                    (edgeIndex + 1) % remaining.Count];
                if (edgeStartIndex == firstIndex ||
                    edgeEndIndex == firstIndex ||
                    edgeStartIndex == secondIndex ||
                    edgeEndIndex == secondIndex)
                {
                    continue;
                }
                if (TryGetChamferPatchSegmentIntersectionEvidence(
                        first,
                        second,
                        projected[edgeStartIndex],
                        projected[edgeEndIndex],
                        epsilon,
                        -1,
                        edgeIndex,
                        out ChamferPatchIntersectionEvidence _))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool TryGetChamferPatchSegmentIntersectionEvidence(
            Vector2 firstStart,
            Vector2 firstEnd,
            Vector2 secondStart,
            Vector2 secondEnd,
            float epsilon,
            int firstEdgeIndex,
            int secondEdgeIndex,
            out ChamferPatchIntersectionEvidence evidence)
        {
            evidence = default;
            float areaEpsilon = epsilon * epsilon;
            float firstOrientation = ChamferPatchCross2D(
                firstStart,
                firstEnd,
                secondStart);
            float secondOrientation = ChamferPatchCross2D(
                firstStart,
                firstEnd,
                secondEnd);
            float thirdOrientation = ChamferPatchCross2D(
                secondStart,
                secondEnd,
                firstStart);
            float fourthOrientation = ChamferPatchCross2D(
                secondStart,
                secondEnd,
                firstEnd);
            bool properIntersection =
                ((firstOrientation > areaEpsilon &&
                  secondOrientation < -areaEpsilon) ||
                 (firstOrientation < -areaEpsilon &&
                  secondOrientation > areaEpsilon)) &&
                ((thirdOrientation > areaEpsilon &&
                  fourthOrientation < -areaEpsilon) ||
                 (thirdOrientation < -areaEpsilon &&
                  fourthOrientation > areaEpsilon));
            if (properIntersection)
            {
                evidence = new ChamferPatchIntersectionEvidence(
                    ChamferPatchIntersectionType.Proper,
                    firstEdgeIndex,
                    secondEdgeIndex,
                    firstStart,
                    firstEnd,
                    secondStart,
                    secondEnd,
                    firstOrientation,
                    secondOrientation,
                    thirdOrientation,
                    fourthOrientation);
                return true;
            }

            bool allCollinear =
                Mathf.Abs(firstOrientation) <= areaEpsilon &&
                Mathf.Abs(secondOrientation) <= areaEpsilon &&
                Mathf.Abs(thirdOrientation) <= areaEpsilon &&
                Mathf.Abs(fourthOrientation) <= areaEpsilon;
            if (allCollinear)
            {
                Vector2 firstDirection = firstEnd - firstStart;
                bool useX = Mathf.Abs(firstDirection.x) >=
                    Mathf.Abs(firstDirection.y);
                float firstMinimum = useX
                    ? Mathf.Min(firstStart.x, firstEnd.x)
                    : Mathf.Min(firstStart.y, firstEnd.y);
                float firstMaximum = useX
                    ? Mathf.Max(firstStart.x, firstEnd.x)
                    : Mathf.Max(firstStart.y, firstEnd.y);
                float secondMinimum = useX
                    ? Mathf.Min(secondStart.x, secondEnd.x)
                    : Mathf.Min(secondStart.y, secondEnd.y);
                float secondMaximum = useX
                    ? Mathf.Max(secondStart.x, secondEnd.x)
                    : Mathf.Max(secondStart.y, secondEnd.y);
                float overlap = Mathf.Min(firstMaximum, secondMaximum) -
                    Mathf.Max(firstMinimum, secondMinimum);
                if (overlap >= -epsilon)
                {
                    ChamferPatchIntersectionType type =
                        overlap > epsilon
                        ? ChamferPatchIntersectionType.CollinearOverlap
                        : ChamferPatchIntersectionType.EndpointTouch;
                    evidence = new ChamferPatchIntersectionEvidence(
                        type,
                        firstEdgeIndex,
                        secondEdgeIndex,
                        firstStart,
                        firstEnd,
                        secondStart,
                        secondEnd,
                        firstOrientation,
                        secondOrientation,
                        thirdOrientation,
                        fourthOrientation);
                    return true;
                }
            }

            bool endpointTouch =
                (Mathf.Abs(firstOrientation) <= areaEpsilon &&
                 ChamferPatchPointOnSegment2D(
                     secondStart,
                     firstStart,
                     firstEnd,
                     epsilon)) ||
                (Mathf.Abs(secondOrientation) <= areaEpsilon &&
                 ChamferPatchPointOnSegment2D(
                     secondEnd,
                     firstStart,
                     firstEnd,
                     epsilon)) ||
                (Mathf.Abs(thirdOrientation) <= areaEpsilon &&
                 ChamferPatchPointOnSegment2D(
                     firstStart,
                     secondStart,
                     secondEnd,
                     epsilon)) ||
                (Mathf.Abs(fourthOrientation) <= areaEpsilon &&
                 ChamferPatchPointOnSegment2D(
                     firstEnd,
                     secondStart,
                     secondEnd,
                     epsilon));
            if (!endpointTouch)
            {
                return false;
            }
            evidence = new ChamferPatchIntersectionEvidence(
                ChamferPatchIntersectionType.EndpointTouch,
                firstEdgeIndex,
                secondEdgeIndex,
                firstStart,
                firstEnd,
                secondStart,
                secondEnd,
                firstOrientation,
                secondOrientation,
                thirdOrientation,
                fourthOrientation);
            return true;
        }

        private static bool ChamferPatchPointOnSegment2D(
            Vector2 point,
            Vector2 start,
            Vector2 end,
            float epsilon)
        {
            return point.x >= Mathf.Min(start.x, end.x) - epsilon &&
                point.x <= Mathf.Max(start.x, end.x) + epsilon &&
                point.y >= Mathf.Min(start.y, end.y) - epsilon &&
                point.y <= Mathf.Max(start.y, end.y) + epsilon;
        }













        #endregion
    }
}
