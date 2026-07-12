using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear boundary normalization and segmentation

        private static bool NormalizeChamferProvisionalFaceWalks(
            List<ChamferProvisionalFaceRecord> faceRecords,
            float minimumStableFaceArea,
            HashSet<TopologyEdgeKey> retraceRemovedEdgeKeys,
            ref ChamferEmissionStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            for (int faceRecordIndex = 0;
                 faceRecordIndex < faceRecords.Count;
                 faceRecordIndex++)
            {
                ChamferProvisionalFaceRecord record =
                    faceRecords[faceRecordIndex];
                List<Vector3> normalized = new List<Vector3>(
                    record.Face.Vertices);
                int removedPairs = ReduceChamferFaceRetraces(
                    normalized,
                    retraceRemovedEdgeKeys);

                if (normalized.Count < 3 ||
                    CalculatePolygonArea(normalized) <= minimumStableFaceArea)
                {
                    stats.FaceLocalNormalizationFailureCount++;
                    blocker = "a provisional face collapses after exact face-local retrace removal";
                    return false;
                }

                Vector3 normal = CalculatePolygonNormal(normalized);
                if (!IsFinite(normal) ||
                    normal.sqrMagnitude <= 0.00000001f ||
                    Vector3.Dot(normal, record.Face.Normal) <= 0.25f)
                {
                    stats.FaceLocalNormalizationFailureCount++;
                    blocker = "a provisional face loses stable winding after exact face-local retrace removal";
                    return false;
                }

                if (!TryFindDuplicateChamferFaceEdge(
                        normalized,
                        out TopologyEdgeKey duplicateKey,
                        out int firstLocalEdgeIndex,
                        out int secondLocalEdgeIndex))
                {
                    stats.FaceLocalDuplicateEdgeFailureCount++;
                    blocker = "a provisional face contains a repeated non-retrace topology edge";
                    return false;
                }

                if (removedPairs > 0 ||
                    normalized.Count != record.Face.Vertices.Count)
                {
                    record.Face = new PolygonFace(
                        normalized,
                        normal,
                        record.Face.Feature,
                        record.Face.FeatureStrength);
                }
            }
            return true;
        }

        private static void RemoveRetraceDeletedChamferBoundaries(
            List<ChamferExpectedVertexBoundary> boundaries,
            Dictionary<TopologyEdgeKey, int> useCounts,
            HashSet<TopologyEdgeKey> retraceRemovedEdgeKeys,
            ref ChamferEmissionStats stats)
        {
            for (int i = boundaries.Count - 1; i >= 0; i--)
            {
                ChamferExpectedVertexBoundary boundary = boundaries[i];
                useCounts.TryGetValue(boundary.Key, out int useCount);
                if (useCount > 0 ||
                    !retraceRemovedEdgeKeys.Contains(boundary.Key))
                {
                    continue;
                }

                boundaries.RemoveAt(i);
            }
        }

        private static List<ChamferExpectedVertexBoundary>
            NormalizeChamferVertexBoundaries(
                List<ChamferExpectedVertexBoundary> registrations,
                Dictionary<TopologyEdgeKey, int> useCounts,
                List<ChamferProvisionalSegmentRecord> segments,
                ref ChamferEmissionStats stats)
        {
            Dictionary<TopologyEdgeKey, List<ChamferExpectedVertexBoundary>> groups =
                new Dictionary<TopologyEdgeKey, List<ChamferExpectedVertexBoundary>>();
            for (int i = 0; i < registrations.Count; i++)
            {
                ChamferExpectedVertexBoundary boundary = registrations[i];
                if (!groups.TryGetValue(
                        boundary.Key,
                        out List<ChamferExpectedVertexBoundary> group))
                {
                    group = new List<ChamferExpectedVertexBoundary>();
                    groups.Add(boundary.Key, group);
                }
                group.Add(boundary);
            }

            Dictionary<TopologyEdgeKey, List<ChamferProvisionalSegmentRecord>>
                segmentUsesByKey =
                    new Dictionary<TopologyEdgeKey,
                        List<ChamferProvisionalSegmentRecord>>();
            for (int i = 0; i < segments.Count; i++)
            {
                ChamferProvisionalSegmentRecord segment = segments[i];
                if (!segmentUsesByKey.TryGetValue(
                        segment.Key,
                        out List<ChamferProvisionalSegmentRecord> uses))
                {
                    uses = new List<ChamferProvisionalSegmentRecord>();
                    segmentUsesByKey.Add(segment.Key, uses);
                }
                uses.Add(segment);
            }

            List<ChamferExpectedVertexBoundary> normalized =
                new List<ChamferExpectedVertexBoundary>(groups.Count);
            foreach (KeyValuePair<TopologyEdgeKey, List<ChamferExpectedVertexBoundary>> pair
                     in groups)
            {
                List<ChamferExpectedVertexBoundary> group = pair.Value;
                useCounts.TryGetValue(pair.Key, out int useCount);
                segmentUsesByKey.TryGetValue(
                    pair.Key,
                    out List<ChamferProvisionalSegmentRecord> segmentUses);
                HashSet<int> faceRecords = new HashSet<int>();
                if (segmentUses != null)
                {
                    for (int i = 0; i < segmentUses.Count; i++)
                    {
                        faceRecords.Add(segmentUses[i].FaceRecordIndex);
                    }
                }

                // Boundary openness is incidence-based. Two uses on two
                // distinct faces are internally closed even when their stored
                // segment directions match; preserve that as a diagnostic.
                if (useCount == 2 &&
                    segmentUses != null &&
                    segmentUses.Count == 2 &&
                    faceRecords.Count == 2)
                {
                    continue;
                }

                bool sameOwner = true;
                ChamferExpectedVertexBoundary first = group[0];
                for (int i = 1; i < group.Count; i++)
                {
                    ChamferExpectedVertexBoundary current = group[i];
                    if (current.SourceVertexIndex != first.SourceVertexIndex ||
                        current.SourceEdgeIndex != first.SourceEdgeIndex ||
                        current.FaceIndex != first.FaceIndex ||
                        current.Kind != first.Kind)
                    {
                        sameOwner = false;
                        break;
                    }
                }

                if (useCount == 1 && sameOwner)
                {
                    normalized.Add(first);
                    continue;
                }

                if (useCount == 0)
                {
                    stats.StaleBoundaryRegistrationFailureCount++;
                }
                else if (sameOwner)
                {
                    stats.VertexBoundarySameOwnerDuplicateFailureCount++;
                }
                else
                {
                    stats.VertexBoundaryMultiOwnerFailureCount++;
                }

            }
            return normalized;
        }

        private static List<PolygonFace> ExtractChamferProvisionalFaces(
            List<ChamferProvisionalFaceRecord> records)
        {
            List<PolygonFace> faces = new List<PolygonFace>(records.Count);
            for (int i = 0; i < records.Count; i++)
            {
                faces.Add(records[i].Face);
            }
            return faces;
        }

        private static ChamferProvisionalFaceRecord
            CloneChamferProvisionalFaceRecord(
                ChamferProvisionalFaceRecord record)
        {
            PolygonFace face = record.Face;
            PolygonFace clonedFace = new PolygonFace(
                new List<Vector3>(face.Vertices),
                face.Normal,
                face.Feature,
                face.FeatureStrength);
            return new ChamferProvisionalFaceRecord(
                clonedFace,
                record.Kind,
                record.SourceFaceIndex,
                record.SourceEdgeIndex,
                record.PatchLoopIndex);
        }

        private static List<ChamferProvisionalFaceRecord>
            CloneChamferProvisionalFaceRecords(
                List<ChamferProvisionalFaceRecord> records)
        {
            List<ChamferProvisionalFaceRecord> clones =
                new List<ChamferProvisionalFaceRecord>(records.Count);
            for (int i = 0; i < records.Count; i++)
            {
                ChamferProvisionalFaceRecord record = records[i];
                PolygonFace face = record.Face;
                PolygonFace clonedFace = new PolygonFace(
                    new List<Vector3>(face.Vertices),
                    face.Normal,
                    face.Feature,
                    face.FeatureStrength);
                clones.Add(new ChamferProvisionalFaceRecord(
                    clonedFace,
                    record.Kind,
                    record.SourceFaceIndex,
                    record.SourceEdgeIndex,
                    record.PatchLoopIndex));
            }
            return clones;
        }

        private static bool TryBuildChamferSourceBoundaryRecords(
            ChamferTopologyContext context,
            ChamferCornerSolution solution,
            out List<ChamferSourceBoundaryRecord> records,
            out string blocker)
        {
            records = new List<ChamferSourceBoundaryRecord>();
            blocker = string.Empty;
            Dictionary<int, List<int>> outgoingByVertex =
                new Dictionary<int, List<int>>();
            for (int i = 0; i < context.HalfEdges.Count; i++)
            {
                ChamferHalfEdge halfEdge = context.HalfEdges[i];
                if (halfEdge.Opposite >= 0)
                {
                    continue;
                }
                if (!outgoingByVertex.TryGetValue(
                        halfEdge.OriginVertex,
                        out List<int> outgoing))
                {
                    outgoing = new List<int>();
                    outgoingByVertex.Add(halfEdge.OriginVertex, outgoing);
                }
                outgoing.Add(i);
            }

            HashSet<int> visited = new HashSet<int>();
            int loopIndex = 0;
            for (int i = 0; i < context.HalfEdges.Count; i++)
            {
                if (context.HalfEdges[i].Opposite >= 0 || visited.Contains(i))
                {
                    continue;
                }

                int current = i;
                int order = 0;
                int guard = 0;
                while (guard++ <= context.HalfEdges.Count)
                {
                    if (!visited.Add(current))
                    {
                        if (current != i)
                        {
                            blocker = "source-boundary record traversal revisited a different half-edge";
                            return false;
                        }
                        break;
                    }

                    ChamferHalfEdge halfEdge = context.HalfEdges[current];
                    ChamferFaceCornerKey startKey = new ChamferFaceCornerKey(
                        halfEdge.FaceIndex,
                        halfEdge.OriginVertex);
                    ChamferFaceCornerKey endKey = new ChamferFaceCornerKey(
                        halfEdge.FaceIndex,
                        halfEdge.DestinationVertex);
                    if (!solution.Corners.TryGetValue(
                            startKey,
                            out ChamferSolvedCorner startCorner) ||
                        !solution.Corners.TryGetValue(
                            endKey,
                            out ChamferSolvedCorner endCorner))
                    {
                        blocker = "source-boundary record is missing a solved face corner";
                        return false;
                    }

                    records.Add(new ChamferSourceBoundaryRecord(
                        halfEdge.SourceEdgeIndex,
                        loopIndex,
                        order,
                        halfEdge.OriginVertex,
                        halfEdge.DestinationVertex,
                        startCorner.Position,
                        endCorner.Position));
                    order++;

                    if (!outgoingByVertex.TryGetValue(
                            halfEdge.DestinationVertex,
                            out List<int> nextCandidates) ||
                        nextCandidates.Count != 1)
                    {
                        blocker = "source-boundary record traversal found an ambiguous continuation";
                        return false;
                    }
                    current = nextCandidates[0];
                    if (current == i)
                    {
                        break;
                    }
                }
                if (guard > context.HalfEdges.Count)
                {
                    blocker = "source-boundary record traversal exceeded its guard";
                    return false;
                }
                loopIndex++;
            }
            return true;
        }

        private static HashSet<TopologyEdgeKey>
            BuildChamferSourceBoundarySegmentKeys(
                List<ChamferSourceBoundaryRecord> records)
        {
            HashSet<TopologyEdgeKey> keys = new HashSet<TopologyEdgeKey>();
            for (int i = 0; i < records.Count; i++)
            {
                List<ChamferSourceBoundaryChild> children = records[i].Children;
                for (int childIndex = 0;
                     childIndex < children.Count;
                     childIndex++)
                {
                    keys.Add(children[childIndex].Key);
                }
            }
            return keys;
        }

        private static void ApplyChamferSourceBoundarySplitPlans(
            List<ChamferSourceBoundaryRecord> records,
            Dictionary<TopologyEdgeKey, List<ChamferSplitPoint>> splitPlans,
            ref ChamferEmissionStats stats)
        {
            for (int recordIndex = 0;
                 recordIndex < records.Count;
                 recordIndex++)
            {
                ChamferSourceBoundaryRecord record = records[recordIndex];
                List<ChamferSourceBoundaryChild> rebuilt =
                    new List<ChamferSourceBoundaryChild>(
                        record.Children.Count + 4);
                for (int childIndex = 0;
                     childIndex < record.Children.Count;
                     childIndex++)
                {
                    ChamferSourceBoundaryChild child =
                        record.Children[childIndex];
                    if (!splitPlans.TryGetValue(
                            child.Key,
                            out List<ChamferSplitPoint> plan))
                    {
                        rebuilt.Add(child);
                        continue;
                    }

                    Vector3 segment = child.End - child.Start;
                    float lengthSqr = segment.sqrMagnitude;
                    if (lengthSqr <= MinimumEdgeLengthSqr)
                    {
                        rebuilt.Add(child);
                        continue;
                    }

                    List<KeyValuePair<float, Vector3>> ordered =
                        new List<KeyValuePair<float, Vector3>>(plan.Count);
                    HashSet<VertexKey> acceptedKeys = new HashSet<VertexKey>();
                    VertexKey startKey = new VertexKey(child.Start);
                    VertexKey endKey = new VertexKey(child.End);
                    for (int i = 0; i < plan.Count; i++)
                    {
                        VertexKey pointKey = plan[i].Key;
                        if (pointKey.Equals(startKey) ||
                            pointKey.Equals(endKey) ||
                            !acceptedKeys.Add(pointKey))
                        {
                            continue;
                        }
                        float t = Vector3.Dot(
                            plan[i].Position - child.Start,
                            segment) / lengthSqr;
                        if (t <= 0f || t >= 1f)
                        {
                            continue;
                        }
                        ordered.Add(new KeyValuePair<float, Vector3>(
                            t,
                            plan[i].Position));
                    }
                    if (ordered.Count == 0)
                    {
                        rebuilt.Add(child);
                        continue;
                    }

                    ordered.Sort((left, right) =>
                        left.Key.CompareTo(right.Key));
                    Vector3 previous = child.Start;
                    for (int i = 0; i <= ordered.Count; i++)
                    {
                        Vector3 next = i < ordered.Count
                            ? ordered[i].Value
                            : child.End;
                        if (!new VertexKey(previous).Equals(
                                new VertexKey(next)))
                        {
                            rebuilt.Add(new ChamferSourceBoundaryChild(
                                previous,
                                next,
                                i == 0 && child.TouchesParentStart,
                                i == ordered.Count && child.TouchesParentEnd));
                        }
                        previous = next;
                    }
                }
                record.Children.Clear();
                record.Children.AddRange(rebuilt);
            }
        }


        private static void NormalizeChamferSourceBoundaryLoops(
            List<ChamferSourceBoundaryRecord> records,
            Dictionary<TopologyEdgeKey, int> useCounts,
            HashSet<TopologyEdgeKey> expectedVertexBoundaryEdges,
            List<ChamferProvisionalSegmentRecord> segments,
            ref ChamferEmissionStats stats)
        {
            Dictionary<int, List<ChamferSourceBoundaryRecord>> recordsByLoop =
                new Dictionary<int, List<ChamferSourceBoundaryRecord>>();
            for (int i = 0; i < records.Count; i++)
            {
                ChamferSourceBoundaryRecord record = records[i];
                record.RawChildCount = record.Children.Count;
                if (!recordsByLoop.TryGetValue(
                        record.BoundaryLoopIndex,
                        out List<ChamferSourceBoundaryRecord> loopRecords))
                {
                    loopRecords = new List<ChamferSourceBoundaryRecord>();
                    recordsByLoop.Add(
                        record.BoundaryLoopIndex,
                        loopRecords);
                }
                loopRecords.Add(record);
            }

            List<int> loopIndices =
                new List<int>(recordsByLoop.Keys);
            loopIndices.Sort();
            for (int loopIndex = 0;
                 loopIndex < loopIndices.Count;
                 loopIndex++)
            {
                int boundaryLoopIndex = loopIndices[loopIndex];
                List<ChamferSourceBoundaryRecord> loopRecords =
                    recordsByLoop[boundaryLoopIndex];
                loopRecords.Sort((left, right) =>
                    left.BoundaryOrder.CompareTo(right.BoundaryOrder));
                bool validOrder = true;
                for (int i = 0; i < loopRecords.Count; i++)
                {
                    if (loopRecords[i].BoundaryOrder == i)
                    {
                        continue;
                    }
                    validOrder = false;
                    stats.SourceBoundaryLoopNormalizationFailureCount++;
                    break;
                }
                if (!validOrder)
                {
                    continue;
                }

                while (true)
                {
                    List<ChamferSourceBoundaryChildLocation> children =
                        new List<ChamferSourceBoundaryChildLocation>();
                    for (int recordIndex = 0;
                         recordIndex < loopRecords.Count;
                         recordIndex++)
                    {
                        ChamferSourceBoundaryRecord record =
                            loopRecords[recordIndex];
                        for (int childIndex = 0;
                             childIndex < record.Children.Count;
                             childIndex++)
                        {
                            children.Add(
                                new ChamferSourceBoundaryChildLocation(
                                    record,
                                    childIndex,
                                    record.Children[childIndex]));
                        }
                    }
                    if (children.Count < 2)
                    {
                        break;
                    }

                    bool removedPair = false;
                    bool rejectedPair = false;
                    for (int i = 0; i < children.Count; i++)
                    {
                        ChamferSourceBoundaryChildLocation first =
                            children[i];
                        ChamferSourceBoundaryChildLocation second =
                            children[(i + 1) % children.Count];
                        VertexKey firstStart =
                            new VertexKey(first.Child.Start);
                        VertexKey firstEnd =
                            new VertexKey(first.Child.End);
                        VertexKey secondStart =
                            new VertexKey(second.Child.Start);
                        VertexKey secondEnd =
                            new VertexKey(second.Child.End);
                        bool exactInverse =
                            firstStart.Equals(secondEnd) &&
                            firstEnd.Equals(secondStart) &&
                            first.Child.Key.Equals(second.Child.Key);
                        if (!exactInverse)
                        {
                            continue;
                        }

                        int useCount = useCounts.TryGetValue(
                            first.Child.Key,
                            out int count)
                            ? count
                            : 0;
                        int distinctFaceRecords =
                            CountDistinctChamferFaceRecords(
                                segments,
                                first.Child.Key);
                        bool vertexOwned =
                            expectedVertexBoundaryEdges.Contains(
                                first.Child.Key);
                        if (useCount != 2 ||
                            distinctFaceRecords != 2 ||
                            vertexOwned)
                        {
                            stats.SourceBoundaryLoopNormalizationFailureCount++;
                            rejectedPair = true;
                            break;
                        }

                        RecordChamferSourceBoundaryChildRemoval(
                            first.Record,
                            first.Child,
                            "adjacent-retrace",
                            second.Record);
                        RecordChamferSourceBoundaryChildRemoval(
                            second.Record,
                            second.Child,
                            "adjacent-retrace",
                            first.Record);

                        if (object.ReferenceEquals(
                                first.Record,
                                second.Record))
                        {
                            int highIndex = Mathf.Max(
                                first.ChildIndex,
                                second.ChildIndex);
                            int lowIndex = Mathf.Min(
                                first.ChildIndex,
                                second.ChildIndex);
                            first.Record.Children.RemoveAt(highIndex);
                            first.Record.Children.RemoveAt(lowIndex);
                        }
                        else
                        {
                            first.Record.Children.RemoveAt(
                                first.ChildIndex);
                            second.Record.Children.RemoveAt(
                                second.ChildIndex);
                        }
                        removedPair = true;
                        break;
                    }
                    if (!removedPair || rejectedPair)
                    {
                        break;
                    }
                }
            }

            for (int i = 0; i < records.Count; i++)
            {
                records[i].PostLoopNormalizationChildCount =
                    records[i].Children.Count;
            }
        }

        private static void RecordChamferSourceBoundaryChildRemoval(
            ChamferSourceBoundaryRecord record,
            ChamferSourceBoundaryChild child,
            string stage,
            ChamferSourceBoundaryRecord partner)
        {
            record.RemovedChildren.Add(
                new ChamferSourceBoundaryChildRemoval(
                    child.Start,
                    child.End,
                    stage,
                    partner.SourceEdgeIndex,
                    partner.BoundaryOrder));
        }

        // Collapse only duplicate source-boundary ownership aliases for one
        // already-closed terminal transition. Provisional face geometry is
        // intentionally untouched.
        private static void CollapseChamferSourceBoundaryTerminalTransferAliases(
            List<ChamferSourceBoundaryRecord> records,
            Dictionary<
                TopologyEdgeKey,
                List<ChamferSourceBoundaryChildOccurrence>>
                rawOccurrencesByKey,
            Dictionary<TopologyEdgeKey, int> useCounts,
            HashSet<TopologyEdgeKey> expectedVertexBoundaryEdges,
            List<ChamferProvisionalSegmentRecord> segments,
            ref ChamferEmissionStats stats)
        {
            HashSet<TopologyEdgeKey> rejectedKeys =
                new HashSet<TopologyEdgeKey>();
            List<string> failures = new List<string>();

            while (true)
            {
                Dictionary<
                    TopologyEdgeKey,
                    List<ChamferSourceBoundaryChildOccurrence>>
                    normalizedOccurrencesByKey =
                        BuildChamferSourceBoundaryChildOccurrences(records);
                List<KeyValuePair<
                    TopologyEdgeKey,
                    List<ChamferSourceBoundaryChildOccurrence>>> groups =
                        new List<KeyValuePair<
                            TopologyEdgeKey,
                            List<ChamferSourceBoundaryChildOccurrence>>>(
                                normalizedOccurrencesByKey);
                groups.Sort((left, right) =>
                {
                    ChamferSourceBoundaryChildOccurrence leftFirst =
                        left.Value[0];
                    ChamferSourceBoundaryChildOccurrence rightFirst =
                        right.Value[0];
                    int comparison = leftFirst.Record.BoundaryLoopIndex
                        .CompareTo(rightFirst.Record.BoundaryLoopIndex);
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                    comparison = leftFirst.Record.BoundaryOrder
                        .CompareTo(rightFirst.Record.BoundaryOrder);
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                    comparison = leftFirst.ChildIndex.CompareTo(
                        rightFirst.ChildIndex);
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                    return leftFirst.Record.SourceEdgeIndex.CompareTo(
                        rightFirst.Record.SourceEdgeIndex);
                });

                bool removedPair = false;
                for (int groupIndex = 0;
                     groupIndex < groups.Count;
                     groupIndex++)
                {
                    TopologyEdgeKey key = groups[groupIndex].Key;
                    List<ChamferSourceBoundaryChildOccurrence> occurrences =
                        groups[groupIndex].Value;
                    if (rejectedKeys.Contains(key))
                    {
                        continue;
                    }

                    int firstOccurrenceIndex = -1;
                    int secondOccurrenceIndex = -1;
                    for (int firstIndex = 0;
                         firstIndex < occurrences.Count - 1 &&
                         firstOccurrenceIndex < 0;
                         firstIndex++)
                    {
                        for (int secondIndex = firstIndex + 1;
                             secondIndex < occurrences.Count;
                             secondIndex++)
                        {
                            if (occurrences[firstIndex].TerminalTransition &&
                                occurrences[secondIndex].TerminalTransition &&
                                AreChamferSourceBoundaryOccurrencesExactInverse(
                                    occurrences[firstIndex],
                                    occurrences[secondIndex]))
                            {
                                firstOccurrenceIndex = firstIndex;
                                secondOccurrenceIndex = secondIndex;
                                break;
                            }
                        }
                    }
                    if (firstOccurrenceIndex < 0)
                    {
                        continue;
                    }

                    ChamferSourceBoundaryChildOccurrence first =
                        occurrences[firstOccurrenceIndex];
                    ChamferSourceBoundaryChildOccurrence second =
                        occurrences[secondOccurrenceIndex];
                    bool normalizedOccurrenceCountValid =
                        occurrences.Count == 2;

                    rawOccurrencesByKey.TryGetValue(
                        key,
                        out List<ChamferSourceBoundaryChildOccurrence>
                            rawOccurrences);
                    bool rawOccurrenceCountValid =
                        rawOccurrences != null &&
                        rawOccurrences.Count == 2;
                    bool sameLoop =
                        first.Record.BoundaryLoopIndex ==
                        second.Record.BoundaryLoopIndex;
                    bool differentRecords = !object.ReferenceEquals(
                        first.Record,
                        second.Record);
                    bool consecutiveRecords =
                        TryResolveConsecutiveChamferSourceBoundaryRecords(
                            records,
                            first.Record,
                            second.Record,
                            out int sharedSourceVertex);
                    bool nonAdjacent =
                        AreChamferSourceBoundaryOccurrencesNonAdjacent(
                            first,
                            second);
                    int useCount = useCounts.TryGetValue(
                        key,
                        out int count)
                        ? count
                        : 0;
                    int distinctFaceRecords =
                        CountDistinctChamferFaceRecords(segments, key);
                    bool vertexOwned =
                        expectedVertexBoundaryEdges.Contains(key);

                    if (!normalizedOccurrenceCountValid ||
                        !rawOccurrenceCountValid ||
                        !sameLoop ||
                        !differentRecords ||
                        !consecutiveRecords ||
                        !nonAdjacent ||
                        useCount != 2 ||
                        distinctFaceRecords != 2 ||
                        vertexOwned)
                    {
                        stats.SourceBoundaryTerminalAliasNormalizationFailureCount++;
                        rejectedKeys.Add(key);
                        if (failures.Count < 3)
                        {
                            failures.Add(
                                "firstSourceEdge:" +
                                    first.Record.SourceEdgeIndex +
                                "/firstLoop:" +
                                    first.Record.BoundaryLoopIndex +
                                "/firstOrder:" +
                                    first.Record.BoundaryOrder +
                                "/firstChild:" + first.ChildIndex +
                                "/secondSourceEdge:" +
                                    second.Record.SourceEdgeIndex +
                                "/secondLoop:" +
                                    second.Record.BoundaryLoopIndex +
                                "/secondOrder:" +
                                    second.Record.BoundaryOrder +
                                "/secondChild:" + second.ChildIndex +
                                "/normalizedOccurrences:" +
                                    occurrences.Count +
                                "/rawOccurrences:" +
                                    (rawOccurrences != null
                                        ? rawOccurrences.Count
                                        : 0) +
                                "/sameLoop:" + (sameLoop ? 1 : 0) +
                                "/differentRecords:" +
                                    (differentRecords ? 1 : 0) +
                                "/consecutiveRecords:" +
                                    (consecutiveRecords ? 1 : 0) +
                                "/sharedSourceVertex:" +
                                    sharedSourceVertex +
                                "/nonAdjacent:" +
                                    (nonAdjacent ? 1 : 0) +
                                "/uses:" + useCount +
                                "/distinctFaceRecords:" +
                                    distinctFaceRecords +
                                "/vertexOwned:" +
                                    (vertexOwned ? 1 : 0));
                        }
                        continue;
                    }

                    RecordChamferSourceBoundaryChildRemoval(
                        first.Record,
                        first.Child,
                        "terminal-transfer-alias",
                        second.Record);
                    RecordChamferSourceBoundaryChildRemoval(
                        second.Record,
                        second.Child,
                        "terminal-transfer-alias",
                        first.Record);
                    first.Record.Children.RemoveAt(first.ChildIndex);
                    second.Record.Children.RemoveAt(second.ChildIndex);
                    removedPair = true;
                    break;
                }

                if (!removedPair)
                {
                    break;
                }
            }

            for (int i = 0; i < records.Count; i++)
            {
                records[i].PostTerminalAliasChildCount =
                    records[i].Children.Count;
            }

        }

        private static bool
            AreChamferSourceBoundaryOccurrencesExactInverse(
                ChamferSourceBoundaryChildOccurrence first,
                ChamferSourceBoundaryChildOccurrence second)
        {
            return
                new VertexKey(first.Child.Start).Equals(
                    new VertexKey(second.Child.End)) &&
                new VertexKey(first.Child.End).Equals(
                    new VertexKey(second.Child.Start)) &&
                first.Child.Key.Equals(second.Child.Key);
        }

        private static bool
            AreChamferSourceBoundaryOccurrencesNonAdjacent(
                ChamferSourceBoundaryChildOccurrence first,
                ChamferSourceBoundaryChildOccurrence second)
        {
            if (first.Record.BoundaryLoopIndex !=
                    second.Record.BoundaryLoopIndex ||
                first.LoopChildCount <= 0 ||
                first.LoopChildCount != second.LoopChildCount)
            {
                return false;
            }

            int loopChildCount = first.LoopChildCount;
            int forwardDistance =
                (second.FlattenedLoopIndex -
                 first.FlattenedLoopIndex +
                 loopChildCount) % loopChildCount;
            int reverseDistance =
                (first.FlattenedLoopIndex -
                 second.FlattenedLoopIndex +
                 loopChildCount) % loopChildCount;
            return forwardDistance > 1 && reverseDistance > 1;
        }

        private static bool
            TryResolveConsecutiveChamferSourceBoundaryRecords(
                List<ChamferSourceBoundaryRecord> records,
                ChamferSourceBoundaryRecord first,
                ChamferSourceBoundaryRecord second,
                out int sharedSourceVertex)
        {
            sharedSourceVertex = -1;
            if (first.BoundaryLoopIndex != second.BoundaryLoopIndex)
            {
                return false;
            }

            int loopRecordCount = 0;
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].BoundaryLoopIndex ==
                    first.BoundaryLoopIndex)
                {
                    loopRecordCount++;
                }
            }
            if (loopRecordCount < 2)
            {
                return false;
            }

            bool[] seenOrders = new bool[loopRecordCount];
            for (int i = 0; i < records.Count; i++)
            {
                ChamferSourceBoundaryRecord record = records[i];
                if (record.BoundaryLoopIndex != first.BoundaryLoopIndex)
                {
                    continue;
                }
                if (record.BoundaryOrder < 0 ||
                    record.BoundaryOrder >= loopRecordCount ||
                    seenOrders[record.BoundaryOrder])
                {
                    return false;
                }
                seenOrders[record.BoundaryOrder] = true;
            }

            bool firstThenSecond =
                second.BoundaryOrder ==
                (first.BoundaryOrder + 1) % loopRecordCount;
            if (firstThenSecond &&
                first.SourceVertexEnd == second.SourceVertexStart)
            {
                sharedSourceVertex = first.SourceVertexEnd;
                return true;
            }

            bool secondThenFirst =
                first.BoundaryOrder ==
                (second.BoundaryOrder + 1) % loopRecordCount;
            if (secondThenFirst &&
                second.SourceVertexEnd == first.SourceVertexStart)
            {
                sharedSourceVertex = second.SourceVertexEnd;
                return true;
            }
            return false;
        }

        private static HashSet<TopologyEdgeKey>
            AuditChamferSourceBoundaryOwnership(
                List<ChamferSourceBoundaryRecord> records,
                Dictionary<TopologyEdgeKey, int> useCounts,
                HashSet<TopologyEdgeKey> expectedVertexBoundaryEdges,
                List<ChamferProvisionalSegmentRecord> segments,
                Dictionary<
                    TopologyEdgeKey,
                    List<ChamferSourceBoundaryChildOccurrence>>
                    rawOccurrencesByKey,
                ref ChamferEmissionStats stats)
        {
            HashSet<TopologyEdgeKey> expectedOpen =
                new HashSet<TopologyEdgeKey>();
            HashSet<TopologyEdgeKey> allChildren =
                new HashSet<TopologyEdgeKey>();
            HashSet<TopologyEdgeKey> duplicateGroupsLogged =
                new HashSet<TopologyEdgeKey>();
            Dictionary<
                TopologyEdgeKey,
                List<ChamferSourceBoundaryChildOccurrence>>
                normalizedOccurrencesByKey =
                    BuildChamferSourceBoundaryChildOccurrences(records);
            List<string> failures = new List<string>();
            List<string> duplicateGroups = new List<string>();

            for (int recordIndex = 0;
                 recordIndex < records.Count;
                 recordIndex++)
            {
                ChamferSourceBoundaryRecord record = records[recordIndex];
                bool subdivided = record.Children.Count > 1;
                for (int childIndex = 0;
                     childIndex < record.Children.Count;
                     childIndex++)
                {
                    ChamferSourceBoundaryChild child =
                        record.Children[childIndex];
                    int useCount = useCounts.TryGetValue(
                        child.Key,
                        out int count)
                        ? count
                        : 0;
                    bool vertexOwned =
                        expectedVertexBoundaryEdges.Contains(child.Key);
                    bool terminalTransition = subdivided &&
                        (child.TouchesParentStart ||
                         child.TouchesParentEnd);

                    if (!allChildren.Add(child.Key))
                    {
                        stats.SourceBoundaryDuplicateChildKeyFailureCount++;
                        AddChamferSourceBoundaryDuplicateGroupDiagnostic(
                            duplicateGroups,
                            duplicateGroupsLogged,
                            child.Key,
                            rawOccurrencesByKey,
                            normalizedOccurrencesByKey,
                            useCounts,
                            expectedVertexBoundaryEdges,
                            segments);
                        AddChamferSourceBoundaryFailure(
                            failures,
                            record,
                            child,
                            childIndex,
                            useCount,
                            vertexOwned,
                            "duplicate-child-key");
                        continue;
                    }

                    if (terminalTransition)
                    {
                        // Terminal descendants are explicit transition candidates.
                        // One use keeps the source boundary open; two uses on
                        // distinct faces prove transfer into the source-vertex
                        // transition without inferring ownership from arbitrary holes.
                        if (useCount == 2 &&
                            CountDistinctChamferFaceRecords(
                                segments,
                                child.Key) == 2)
                        {
                            continue;
                        }
                        if (useCount == 1 && !vertexOwned)
                        {
                        }
                        else
                        {
                            stats.SourceBoundaryTerminalTransferFailureCount++;
                            AddChamferSourceBoundaryFailure(
                                failures,
                                record,
                                child,
                                childIndex,
                                useCount,
                                vertexOwned,
                                "terminal-child-incidence");
                            continue;
                        }
                    }

                    stats.ExpectedSourceBoundaryEdgeCount++;
                    if (!expectedOpen.Add(child.Key))
                    {
                        stats.SourceBoundaryDuplicateChildKeyFailureCount++;
                        AddChamferSourceBoundaryDuplicateGroupDiagnostic(
                            duplicateGroups,
                            duplicateGroupsLogged,
                            child.Key,
                            rawOccurrencesByKey,
                            normalizedOccurrencesByKey,
                            useCounts,
                            expectedVertexBoundaryEdges,
                            segments);
                        AddChamferSourceBoundaryFailure(
                            failures,
                            record,
                            child,
                            childIndex,
                            useCount,
                            vertexOwned,
                            "duplicate-expected-key");
                        continue;
                    }
                    if (useCount == 1 && !vertexOwned)
                    {
                        stats.MatchedSourceBoundaryEdgeCount++;
                    }
                    else
                    {
                        stats.SourceBoundaryChildIncidenceFailureCount++;
                        AddChamferSourceBoundaryFailure(
                            failures,
                            record,
                            child,
                            childIndex,
                            useCount,
                            vertexOwned,
                            vertexOwned
                                ? "source-child-also-vertex-owned"
                                : "source-child-incidence");
                    }
                }
            }

            return expectedOpen;
        }

        private static Dictionary<
            TopologyEdgeKey,
            List<ChamferSourceBoundaryChildOccurrence>>
            BuildChamferSourceBoundaryChildOccurrences(
                List<ChamferSourceBoundaryRecord> records)
        {
            Dictionary<int, List<ChamferSourceBoundaryRecord>> recordsByLoop =
                new Dictionary<int, List<ChamferSourceBoundaryRecord>>();
            for (int i = 0; i < records.Count; i++)
            {
                ChamferSourceBoundaryRecord record = records[i];
                if (!recordsByLoop.TryGetValue(
                        record.BoundaryLoopIndex,
                        out List<ChamferSourceBoundaryRecord> loopRecords))
                {
                    loopRecords = new List<ChamferSourceBoundaryRecord>();
                    recordsByLoop.Add(
                        record.BoundaryLoopIndex,
                        loopRecords);
                }
                loopRecords.Add(record);
            }

            Dictionary<
                TopologyEdgeKey,
                List<ChamferSourceBoundaryChildOccurrence>> occurrencesByKey =
                    new Dictionary<
                        TopologyEdgeKey,
                        List<ChamferSourceBoundaryChildOccurrence>>();
            List<int> loopIndices = new List<int>(recordsByLoop.Keys);
            loopIndices.Sort();
            for (int loopIndex = 0;
                 loopIndex < loopIndices.Count;
                 loopIndex++)
            {
                List<ChamferSourceBoundaryRecord> loopRecords =
                    recordsByLoop[loopIndices[loopIndex]];
                loopRecords.Sort((left, right) =>
                    left.BoundaryOrder.CompareTo(right.BoundaryOrder));
                int loopChildCount = 0;
                for (int recordIndex = 0;
                     recordIndex < loopRecords.Count;
                     recordIndex++)
                {
                    loopChildCount += loopRecords[recordIndex].Children.Count;
                }

                int flattenedLoopIndex = 0;
                for (int recordIndex = 0;
                     recordIndex < loopRecords.Count;
                     recordIndex++)
                {
                    ChamferSourceBoundaryRecord record =
                        loopRecords[recordIndex];
                    int recordChildCount = record.Children.Count;
                    bool subdivided = recordChildCount > 1;
                    for (int childIndex = 0;
                         childIndex < recordChildCount;
                         childIndex++)
                    {
                        ChamferSourceBoundaryChild child =
                            record.Children[childIndex];
                        ChamferSourceBoundaryChildOccurrence occurrence =
                            new ChamferSourceBoundaryChildOccurrence(
                                record,
                                childIndex,
                                child,
                                flattenedLoopIndex,
                                loopChildCount,
                                recordChildCount,
                                subdivided &&
                                    (child.TouchesParentStart ||
                                     child.TouchesParentEnd));
                        if (!occurrencesByKey.TryGetValue(
                                child.Key,
                                out List<ChamferSourceBoundaryChildOccurrence>
                                    occurrences))
                        {
                            occurrences =
                                new List<
                                    ChamferSourceBoundaryChildOccurrence>();
                            occurrencesByKey.Add(child.Key, occurrences);
                        }
                        occurrences.Add(occurrence);
                        flattenedLoopIndex++;
                    }
                }
            }
            return occurrencesByKey;
        }

        private static void AddChamferSourceBoundaryDuplicateGroupDiagnostic(
            List<string> diagnostics,
            HashSet<TopologyEdgeKey> loggedKeys,
            TopologyEdgeKey key,
            Dictionary<
                TopologyEdgeKey,
                List<ChamferSourceBoundaryChildOccurrence>>
                rawOccurrencesByKey,
            Dictionary<
                TopologyEdgeKey,
                List<ChamferSourceBoundaryChildOccurrence>>
                normalizedOccurrencesByKey,
            Dictionary<TopologyEdgeKey, int> useCounts,
            HashSet<TopologyEdgeKey> expectedVertexBoundaryEdges,
            List<ChamferProvisionalSegmentRecord> segments)
        {
            if (diagnostics.Count >= 3 || !loggedKeys.Add(key))
            {
                return;
            }

            rawOccurrencesByKey.TryGetValue(
                key,
                out List<ChamferSourceBoundaryChildOccurrence> rawOccurrences);
            normalizedOccurrencesByKey.TryGetValue(
                key,
                out List<ChamferSourceBoundaryChildOccurrence>
                    normalizedOccurrences);
            int useCount = useCounts.TryGetValue(key, out int count)
                ? count
                : 0;
            int distinctFaceRecords = CountDistinctChamferFaceRecords(
                segments,
                key);
            bool vertexOwned = expectedVertexBoundaryEdges.Contains(key);

            string diagnostic =
                "rawOccurrences:" +
                    (rawOccurrences != null ? rawOccurrences.Count : 0) +
                "/normalizedOccurrences:" +
                    (normalizedOccurrences != null
                        ? normalizedOccurrences.Count
                        : 0) +
                "/uses:" + useCount +
                "/distinctFaceRecords:" + distinctFaceRecords +
                "/vertexOwned:" + (vertexOwned ? 1 : 0) +
                "/rawPair:" +
                    BuildChamferSourceBoundaryPairDiagnostic(rawOccurrences) +
                "/normalizedPair:" +
                    BuildChamferSourceBoundaryPairDiagnostic(
                        normalizedOccurrences);

            if (rawOccurrences != null)
            {
                for (int i = 0; i < rawOccurrences.Count; i++)
                {
                    ChamferSourceBoundaryChildOccurrence occurrence =
                        rawOccurrences[i];
                    diagnostic +=
                        "/rawOccurrence[" + i + "]:" +
                        BuildChamferSourceBoundaryOccurrenceDiagnostic(
                            occurrence,
                            useCount,
                            distinctFaceRecords,
                            vertexOwned);
                }
            }
            if (normalizedOccurrences != null)
            {
                for (int i = 0; i < normalizedOccurrences.Count; i++)
                {
                    ChamferSourceBoundaryChildOccurrence occurrence =
                        normalizedOccurrences[i];
                    diagnostic +=
                        "/occurrence[" + i + "]:" +
                        BuildChamferSourceBoundaryOccurrenceDiagnostic(
                            occurrence,
                            useCount,
                            distinctFaceRecords,
                            vertexOwned);
                }
            }
            diagnostics.Add(diagnostic);
        }

        private static string BuildChamferSourceBoundaryPairDiagnostic(
            List<ChamferSourceBoundaryChildOccurrence> occurrences)
        {
            if (occurrences == null || occurrences.Count < 2)
            {
                return "insufficient";
            }

            ChamferSourceBoundaryChildOccurrence first = occurrences[0];
            ChamferSourceBoundaryChildOccurrence second = occurrences[1];
            VertexKey firstStart = new VertexKey(first.Child.Start);
            VertexKey firstEnd = new VertexKey(first.Child.End);
            VertexKey secondStart = new VertexKey(second.Child.Start);
            VertexKey secondEnd = new VertexKey(second.Child.End);
            string relationship;
            if (firstStart.Equals(secondStart) &&
                firstEnd.Equals(secondEnd))
            {
                relationship = "same-direction";
            }
            else if (firstStart.Equals(secondEnd) &&
                     firstEnd.Equals(secondStart))
            {
                relationship = "inverse-direction";
            }
            else
            {
                relationship = "directionally-incompatible";
            }

            bool sameLoop =
                first.Record.BoundaryLoopIndex ==
                second.Record.BoundaryLoopIndex;
            int forwardDistance = -1;
            int reverseDistance = -1;
            bool adjacent = false;
            if (sameLoop &&
                first.LoopChildCount > 0 &&
                first.LoopChildCount == second.LoopChildCount)
            {
                int loopChildCount = first.LoopChildCount;
                forwardDistance =
                    (second.FlattenedLoopIndex -
                     first.FlattenedLoopIndex +
                     loopChildCount) % loopChildCount;
                reverseDistance =
                    (first.FlattenedLoopIndex -
                     second.FlattenedLoopIndex +
                     loopChildCount) % loopChildCount;
                adjacent = forwardDistance == 1 || reverseDistance == 1;
            }

            return
                "relationship:" + relationship +
                "/sameLoop:" + (sameLoop ? 1 : 0) +
                "/forwardDistance:" + forwardDistance +
                "/reverseDistance:" + reverseDistance +
                "/adjacent:" + (adjacent ? 1 : 0);
        }

        private static string BuildChamferSourceBoundaryOccurrenceDiagnostic(
            ChamferSourceBoundaryChildOccurrence occurrence,
            int useCount,
            int distinctFaceRecords,
            bool vertexOwned)
        {
            ChamferSourceBoundaryRecord record = occurrence.Record;
            ChamferSourceBoundaryChild child = occurrence.Child;
            return
                "sourceEdge:" + record.SourceEdgeIndex +
                "/loop:" + record.BoundaryLoopIndex +
                "/order:" + record.BoundaryOrder +
                "/child:" + occurrence.ChildIndex +
                "/flatIndex:" + occurrence.FlattenedLoopIndex +
                "/loopChildren:" + occurrence.LoopChildCount +
                "/recordChildren:" + occurrence.RecordChildCount +
                "/sourceVertices:" + record.SourceVertexStart +
                    "->" + record.SourceVertexEnd +
                "/parentStart:" + record.ParentStart.ToString("F4") +
                "/parentEnd:" + record.ParentEnd.ToString("F4") +
                "/start:" + child.Start.ToString("F4") +
                "/end:" + child.End.ToString("F4") +
                "/touchStart:" +
                    (child.TouchesParentStart ? 1 : 0) +
                "/touchEnd:" +
                    (child.TouchesParentEnd ? 1 : 0) +
                "/terminalTransition:" +
                    (occurrence.TerminalTransition ? 1 : 0) +
                "/disposition:" +
                    ResolveChamferSourceBoundaryOccurrenceDisposition(
                        occurrence,
                        useCount,
                        distinctFaceRecords,
                        vertexOwned);
        }

        private static string ResolveChamferSourceBoundaryOccurrenceDisposition(
            ChamferSourceBoundaryChildOccurrence occurrence,
            int useCount,
            int distinctFaceRecords,
            bool vertexOwned)
        {
            if (occurrence.TerminalTransition)
            {
                if (useCount == 2 && distinctFaceRecords == 2)
                {
                    return "terminal-transfer";
                }
                if (useCount == 1 && !vertexOwned)
                {
                    return "terminal-open";
                }
                return "invalid-terminal";
            }
            if (useCount == 1 && !vertexOwned)
            {
                return "expected-open";
            }
            return vertexOwned
                ? "invalid-source-child-vertex-owned"
                : "invalid-source-child";
        }

        private static int CountDistinctChamferFaceRecords(
            List<ChamferProvisionalSegmentRecord> segments,
            TopologyEdgeKey key)
        {
            HashSet<int> faceRecords = new HashSet<int>();
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i].Key.Equals(key))
                {
                    faceRecords.Add(segments[i].FaceRecordIndex);
                }
            }
            return faceRecords.Count;
        }

        private static void AddChamferSourceBoundaryFailure(
            List<string> failures,
            ChamferSourceBoundaryRecord record,
            ChamferSourceBoundaryChild child,
            int childIndex,
            int useCount,
            bool vertexOwned,
            string reason)
        {
            if (failures.Count >= 3)
            {
                return;
            }
            failures.Add(
                "sourceEdge:" + record.SourceEdgeIndex +
                "/loop:" + record.BoundaryLoopIndex +
                "/order:" + record.BoundaryOrder +
                "/sourceVertices:" + record.SourceVertexStart +
                    "->" + record.SourceVertexEnd +
                "/parentStart:" + record.ParentStart.ToString("F4") +
                "/parentEnd:" + record.ParentEnd.ToString("F4") +
                "/child:" + childIndex +
                "/reason:" + reason +
                "/start:" + child.Start.ToString("F4") +
                "/end:" + child.End.ToString("F4") +
                "/uses:" + useCount +
                "/vertexOwned:" + (vertexOwned ? 1 : 0) +
                "/touchStart:" + (child.TouchesParentStart ? 1 : 0) +
                "/touchEnd:" + (child.TouchesParentEnd ? 1 : 0));
        }

        private static List<ChamferProvisionalSegmentRecord>
            BuildChamferProvisionalSegmentRecords(
                List<ChamferProvisionalFaceRecord> faceRecords,
                List<ChamferExpectedVertexBoundary> boundaries,
                HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges,
                Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
                HashSet<TopologyEdgeKey> vertexPatchFaceBoundaryEdges = null)
        {
            Dictionary<TopologyEdgeKey, ChamferVertexBoundaryKind> boundaryKinds =
                new Dictionary<TopologyEdgeKey, ChamferVertexBoundaryKind>();
            for (int i = 0; i < boundaries.Count; i++)
            {
                ChamferExpectedVertexBoundary boundary = boundaries[i];
                if (!boundaryKinds.ContainsKey(boundary.Key) ||
                    boundary.Kind == ChamferVertexBoundaryKind.BevelStripEndpoint)
                {
                    boundaryKinds[boundary.Key] = boundary.Kind;
                }
            }

            HashSet<TopologyEdgeKey> sharedSpanKeys =
                new HashSet<TopologyEdgeKey>();
            foreach (ChamferSharedEdgeSpan span in sharedSpans.Values)
            {
                sharedSpanKeys.Add(new TopologyEdgeKey(
                    new VertexKey(span.SharedAtVertexA),
                    new VertexKey(span.SharedAtVertexB)));
            }

            List<ChamferProvisionalSegmentRecord> segments =
                new List<ChamferProvisionalSegmentRecord>();
            for (int faceIndex = 0; faceIndex < faceRecords.Count; faceIndex++)
            {
                ChamferProvisionalFaceRecord record = faceRecords[faceIndex];
                List<Vector3> vertices = record.Face.Vertices;
                if (vertices == null || vertices.Count < 2)
                {
                    continue;
                }
                for (int edgeIndex = 0; edgeIndex < vertices.Count; edgeIndex++)
                {
                    Vector3 start = vertices[edgeIndex];
                    Vector3 end = vertices[(edgeIndex + 1) % vertices.Count];
                    VertexKey startKey = new VertexKey(start);
                    VertexKey endKey = new VertexKey(end);
                    if (startKey.Equals(endKey))
                    {
                        continue;
                    }
                    TopologyEdgeKey key = new TopologyEdgeKey(startKey, endKey);
                    ChamferSegmentRole role;
                    if (record.Kind ==
                        ChamferProvisionalFaceKind.VertexPatch)
                    {
                        role = vertexPatchFaceBoundaryEdges != null &&
                            vertexPatchFaceBoundaryEdges.Contains(key)
                            ? ChamferSegmentRole.VertexPatchBoundary
                            : ChamferSegmentRole.VertexPatchDiagonal;
                    }
                    else if (boundaryKinds.TryGetValue(
                            key,
                            out ChamferVertexBoundaryKind boundaryKind))
                    {
                        role = boundaryKind ==
                            ChamferVertexBoundaryKind.BevelStripEndpoint
                            ? ChamferSegmentRole.BevelEndpoint
                            : ChamferSegmentRole.ReplacementVertexTail;
                    }
                    else if (expectedSourceBoundaryEdges.Contains(key))
                    {
                        role = ChamferSegmentRole.PreservedSourceBoundary;
                    }
                    else if (record.Kind == ChamferProvisionalFaceKind.BevelStrip)
                    {
                        role = ChamferSegmentRole.BevelRail;
                    }
                    else if (sharedSpanKeys.Contains(key))
                    {
                        role = ChamferSegmentRole.ReplacementSharedSpan;
                    }
                    else
                    {
                        role = ChamferSegmentRole.ReplacementOrdinaryEdge;
                    }

                    segments.Add(new ChamferProvisionalSegmentRecord(
                        faceIndex,
                        edgeIndex,
                        key,
                        start,
                        end,
                        record.Kind,
                        role,
                        record.SourceFaceIndex,
                        record.SourceEdgeIndex,
                        record.PatchLoopIndex));
                }
            }
            return segments;
        }

        private static Dictionary<VertexKey, ChamferBoundaryPointRecord>
            BuildChamferBoundaryPointRecords(
                List<ChamferExpectedVertexBoundary> boundaries)
        {
            Dictionary<VertexKey, ChamferBoundaryPointRecord> points =
                new Dictionary<VertexKey, ChamferBoundaryPointRecord>();
            for (int i = 0; i < boundaries.Count; i++)
            {
                ChamferExpectedVertexBoundary boundary = boundaries[i];
                AddChamferBoundaryPointRecord(
                    points,
                    new VertexKey(boundary.Start),
                    boundary.Start,
                    boundary.SourceVertexIndex);
                AddChamferBoundaryPointRecord(
                    points,
                    new VertexKey(boundary.End),
                    boundary.End,
                    boundary.SourceVertexIndex);
            }
            return points;
        }

        private static void AddChamferBoundaryPointRecord(
            Dictionary<VertexKey, ChamferBoundaryPointRecord> points,
            VertexKey key,
            Vector3 position,
            int sourceVertexIndex)
        {
            if (!points.TryGetValue(key, out ChamferBoundaryPointRecord point))
            {
                point = new ChamferBoundaryPointRecord(key, position);
                points.Add(key, point);
            }
            point.SourceVertexIndices.Add(sourceVertexIndex);
        }

        private static bool IsChamferSplitCompatible(
            ChamferBoundaryPointRecord point,
            ChamferProvisionalSegmentRecord segment,
            ChamferTopologyContext context,
            HashSet<VertexKey> provisionalVertexKeys)
        {
            if (!HasValidRawChamferPointProvenance(
                    point,
                    context,
                    provisionalVertexKeys))
            {
                return false;
            }

            if (segment.Role == ChamferSegmentRole.PreservedSourceBoundary)
            {
                // This is segmentation-only: the point already exists in the
                // provisional mesh and does not move or seal the source boundary.
                return segment.FaceKind ==
                    ChamferProvisionalFaceKind.ReplacementBase;
            }

            foreach (int sourceVertexIndex in point.SourceVertexIndices)
            {
                if (segment.FaceKind == ChamferProvisionalFaceKind.BevelStrip)
                {
                    if (segment.SourceEdgeIndex < 0 ||
                        segment.SourceEdgeIndex >= context.Graph.Edges.Count)
                    {
                        continue;
                    }
                    EdgeWearGraphEdge edge =
                        context.Graph.Edges[segment.SourceEdgeIndex];
                    if (edge.VertexA == sourceVertexIndex ||
                        edge.VertexB == sourceVertexIndex)
                    {
                        return true;
                    }
                }
                else
                {
                    if (segment.SourceFaceIndex < 0 ||
                        segment.SourceFaceIndex >= context.Graph.Faces.Count)
                    {
                        continue;
                    }
                    EdgeWearGraphFace face =
                        context.Graph.Faces[segment.SourceFaceIndex];
                    if (face.VertexIndices.Contains(sourceVertexIndex))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool HasValidRawChamferPointProvenance(
            ChamferBoundaryPointRecord point,
            ChamferTopologyContext context,
            HashSet<VertexKey> provisionalVertexKeys)
        {
            if (point.SourceVertexIndices.Count == 0 ||
                !provisionalVertexKeys.Contains(point.Key))
            {
                return false;
            }

            foreach (int sourceVertexIndex in point.SourceVertexIndices)
            {
                if (sourceVertexIndex >= 0 &&
                    sourceVertexIndex < context.Graph.Vertices.Count)
                {
                    return true;
                }
            }
            return false;
        }

        private static HashSet<VertexKey> BuildChamferProvisionalVertexKeys(
            List<ChamferProvisionalSegmentRecord> segments)
        {
            HashSet<VertexKey> keys = new HashSet<VertexKey>();
            for (int i = 0; i < segments.Count; i++)
            {
                keys.Add(new VertexKey(segments[i].Start));
                keys.Add(new VertexKey(segments[i].End));
            }
            return keys;
        }

        private static int SegmentRawChamferTJunctions(
            List<ChamferProvisionalFaceRecord> faceRecords,
            ChamferTopologyContext context,
            Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
            List<ChamferExpectedVertexBoundary> boundaries,
            List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
            float minimumStableEdgeLength,
            ref ChamferEmissionStats stats)
        {
            const int MaximumSegmentationPasses = 4;
            int totalUniqueSplits = 0;
            float tolerance = CalculateTopologyTJunctionTolerance(
                minimumStableEdgeLength);
            float toleranceSqr = tolerance * tolerance;
            HashSet<ChamferTJunctionRecordKey> foundRecords =
                new HashSet<ChamferTJunctionRecordKey>();
            HashSet<ChamferTJunctionRecordKey> compatibleRecords =
                new HashSet<ChamferTJunctionRecordKey>();

            for (int pass = 0; pass < MaximumSegmentationPasses; pass++)
            {
                HashSet<TopologyEdgeKey> sourceBoundarySegmentKeys =
                    BuildChamferSourceBoundarySegmentKeys(
                        sourceBoundaryRecords);
                List<ChamferProvisionalSegmentRecord> segments =
                    BuildChamferProvisionalSegmentRecords(
                        faceRecords,
                        boundaries,
                        sourceBoundarySegmentKeys,
                        sharedSpans);
                HashSet<VertexKey> provisionalVertexKeys =
                    BuildChamferProvisionalVertexKeys(segments);
                Dictionary<TopologyEdgeKey, List<ChamferProvisionalSegmentRecord>>
                    segmentsByKey =
                        new Dictionary<TopologyEdgeKey, List<ChamferProvisionalSegmentRecord>>();
                for (int i = 0; i < segments.Count; i++)
                {
                    ChamferProvisionalSegmentRecord segment = segments[i];
                    if (!segmentsByKey.TryGetValue(
                            segment.Key,
                            out List<ChamferProvisionalSegmentRecord> group))
                    {
                        group = new List<ChamferProvisionalSegmentRecord>();
                        segmentsByKey.Add(segment.Key, group);
                    }
                    group.Add(segment);
                }

                Dictionary<VertexKey, ChamferBoundaryPointRecord> points =
                    BuildChamferBoundaryPointRecords(boundaries);
                Dictionary<TopologyEdgeKey, List<ChamferSplitPoint>> splitPlans =
                    new Dictionary<TopologyEdgeKey, List<ChamferSplitPoint>>();

                foreach (KeyValuePair<TopologyEdgeKey,
                         List<ChamferProvisionalSegmentRecord>> segmentPair
                         in segmentsByKey)
                {
                    List<ChamferProvisionalSegmentRecord> uses =
                        segmentPair.Value;
                    if (uses.Count == 0)
                    {
                        continue;
                    }
                    ChamferProvisionalSegmentRecord representative = uses[0];
                    foreach (ChamferBoundaryPointRecord point in points.Values)
                    {
                        if (point.Key.Equals(segmentPair.Key.First) ||
                            point.Key.Equals(segmentPair.Key.Second) ||
                            point.SourceVertexIndices.Count == 0 ||
                            !provisionalVertexKeys.Contains(point.Key))
                        {
                            continue;
                        }

                        bool liesOnAnyUse = false;
                        bool compatible = false;
                        ChamferProvisionalSegmentRecord containingUse = representative;
                        ChamferProvisionalSegmentRecord compatibleUse = representative;
                        for (int useIndex = 0; useIndex < uses.Count; useIndex++)
                        {
                            ChamferProvisionalSegmentRecord use = uses[useIndex];
                            if (!IsPointOnSegmentInterior(
                                    point.Position,
                                    use.Start,
                                    use.End,
                                    toleranceSqr))
                            {
                                continue;
                            }
                            liesOnAnyUse = true;
                            containingUse = use;
                            if (IsChamferSplitCompatible(
                                    point,
                                    use,
                                    context,
                                    provisionalVertexKeys))
                            {
                                compatible = true;
                                compatibleUse = use;
                                break;
                            }
                        }
                        if (!liesOnAnyUse)
                        {
                            continue;
                        }

                        foundRecords.Add(new ChamferTJunctionRecordKey(
                            point.Key,
                            segmentPair.Key,
                            containingUse.FaceRecordIndex,
                            containingUse.Role));
                        if (!compatible)
                        {
                            continue;
                        }

                        Vector3 compatibleSegment =
                            compatibleUse.End - compatibleUse.Start;
                        float compatibleLengthSqr =
                            compatibleSegment.sqrMagnitude;
                        if (compatibleLengthSqr <= MinimumEdgeLengthSqr)
                        {
                            continue;
                        }
                        float t = Vector3.Dot(
                            point.Position - compatibleUse.Start,
                            compatibleSegment) / compatibleLengthSqr;
                        if (t <= 0f || t >= 1f)
                        {
                            continue;
                        }

                        if (!splitPlans.TryGetValue(
                                segmentPair.Key,
                                out List<ChamferSplitPoint> plan))
                        {
                            plan = new List<ChamferSplitPoint>();
                            splitPlans.Add(segmentPair.Key, plan);
                        }
                        bool alreadyPlanned = false;
                        for (int splitIndex = 0;
                             splitIndex < plan.Count;
                             splitIndex++)
                        {
                            if (plan[splitIndex].Key.Equals(point.Key))
                            {
                                alreadyPlanned = true;
                                break;
                            }
                        }
                        if (alreadyPlanned)
                        {
                            continue;
                        }

                        plan.Add(new ChamferSplitPoint(
                            point.Key,
                            point.Position,
                            t,
                            compatibleUse.FaceRecordIndex,
                            compatibleUse.LocalEdgeIndex));
                        compatibleRecords.Add(new ChamferTJunctionRecordKey(
                            point.Key,
                            segmentPair.Key,
                            compatibleUse.FaceRecordIndex,
                            compatibleUse.Role));
                    }
                }

                if (splitPlans.Count == 0)
                {
                    break;
                }

                int appliedThisPass = ApplyChamferSplitPlans(
                    faceRecords,
                    boundaries,
                    sourceBoundaryRecords,
                    sourceBoundarySegmentKeys,
                    sharedSpans,
                    splitPlans,
                    ref stats);
                if (appliedThisPass == 0)
                {
                    break;
                }
                stats.TJunctionSegmentationPasses++;
                totalUniqueSplits += appliedThisPass;
            }

            HashSet<ChamferTJunctionRecordKey> unresolvedRecords =
                CollectUnresolvedRawChamferTJunctionRecords(
                    faceRecords,
                    sharedSpans,
                    boundaries,
                    BuildChamferSourceBoundarySegmentKeys(
                        sourceBoundaryRecords),
                    toleranceSqr);
            foreach (ChamferTJunctionRecordKey record in unresolvedRecords)
            {
                foundRecords.Add(record);
            }

            stats.ProvenanceCompatibleTJunctionSplits = totalUniqueSplits;
            stats.TJunctionRecordsIncompatible = unresolvedRecords.Count;
            return totalUniqueSplits;
        }

        private static HashSet<ChamferTJunctionRecordKey>
            CollectUnresolvedRawChamferTJunctionRecords(
                List<ChamferProvisionalFaceRecord> faceRecords,
                Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
                List<ChamferExpectedVertexBoundary> boundaries,
                HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges,
                float toleranceSqr)
        {
            List<ChamferProvisionalSegmentRecord> segments =
                BuildChamferProvisionalSegmentRecords(
                    faceRecords,
                    boundaries,
                    expectedSourceBoundaryEdges,
                    sharedSpans);
            HashSet<VertexKey> provisionalVertexKeys =
                BuildChamferProvisionalVertexKeys(segments);
            Dictionary<TopologyEdgeKey, List<ChamferProvisionalSegmentRecord>>
                segmentsByKey =
                    new Dictionary<TopologyEdgeKey, List<ChamferProvisionalSegmentRecord>>();
            for (int i = 0; i < segments.Count; i++)
            {
                ChamferProvisionalSegmentRecord segment = segments[i];
                if (!segmentsByKey.TryGetValue(
                        segment.Key,
                        out List<ChamferProvisionalSegmentRecord> group))
                {
                    group = new List<ChamferProvisionalSegmentRecord>();
                    segmentsByKey.Add(segment.Key, group);
                }
                group.Add(segment);
            }

            Dictionary<VertexKey, ChamferBoundaryPointRecord> points =
                BuildChamferBoundaryPointRecords(boundaries);
            HashSet<ChamferTJunctionRecordKey> unresolved =
                new HashSet<ChamferTJunctionRecordKey>();
            foreach (KeyValuePair<TopologyEdgeKey,
                     List<ChamferProvisionalSegmentRecord>> segmentPair
                     in segmentsByKey)
            {
                List<ChamferProvisionalSegmentRecord> uses = segmentPair.Value;
                if (uses.Count == 0)
                {
                    continue;
                }
                foreach (ChamferBoundaryPointRecord point in points.Values)
                {
                    if (point.Key.Equals(segmentPair.Key.First) ||
                        point.Key.Equals(segmentPair.Key.Second) ||
                        point.SourceVertexIndices.Count == 0 ||
                        !provisionalVertexKeys.Contains(point.Key))
                    {
                        continue;
                    }

                    for (int useIndex = 0; useIndex < uses.Count; useIndex++)
                    {
                        ChamferProvisionalSegmentRecord use = uses[useIndex];
                        if (!IsPointOnSegmentInterior(
                                point.Position,
                                use.Start,
                                use.End,
                                toleranceSqr))
                        {
                            continue;
                        }

                        unresolved.Add(new ChamferTJunctionRecordKey(
                            point.Key,
                            segmentPair.Key,
                            use.FaceRecordIndex,
                            use.Role));
                        break;
                    }
                }
            }
            return unresolved;
        }

        private static int ApplyChamferSplitPlans(
            List<ChamferProvisionalFaceRecord> faceRecords,
            List<ChamferExpectedVertexBoundary> boundaries,
            List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
            HashSet<TopologyEdgeKey> sourceBoundarySegmentKeys,
            Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
            Dictionary<TopologyEdgeKey, List<ChamferSplitPoint>> splitPlans,
            ref ChamferEmissionStats stats)
        {
            int uniqueSplitCount = 0;
            foreach (List<ChamferSplitPoint> plan in splitPlans.Values)
            {
                uniqueSplitCount += plan.Count;
            }

            for (int faceIndex = 0; faceIndex < faceRecords.Count; faceIndex++)
            {
                ChamferProvisionalFaceRecord record = faceRecords[faceIndex];
                List<Vector3> source = record.Face.Vertices;
                List<Vector3> rebuilt = new List<Vector3>(source.Count + 4);
                bool changed = false;
                for (int edgeIndex = 0; edgeIndex < source.Count; edgeIndex++)
                {
                    Vector3 start = source[edgeIndex];
                    Vector3 end = source[(edgeIndex + 1) % source.Count];
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        new VertexKey(start),
                        new VertexKey(end));
                    rebuilt.Add(start);
                    if (!splitPlans.TryGetValue(
                            key,
                            out List<ChamferSplitPoint> plan))
                    {
                        continue;
                    }

                    Vector3 segment = end - start;
                    float lengthSqr = segment.sqrMagnitude;
                    if (lengthSqr <= MinimumEdgeLengthSqr)
                    {
                        continue;
                    }
                    List<KeyValuePair<float, Vector3>> ordered =
                        new List<KeyValuePair<float, Vector3>>(plan.Count);
                    for (int i = 0; i < plan.Count; i++)
                    {
                        float t = Vector3.Dot(
                            plan[i].Position - start,
                            segment) / lengthSqr;
                        ordered.Add(new KeyValuePair<float, Vector3>(
                            t,
                            plan[i].Position));
                    }
                    ordered.Sort((left, right) => left.Key.CompareTo(right.Key));
                    VertexKey lastKey = new VertexKey(start);
                    for (int i = 0; i < ordered.Count; i++)
                    {
                        VertexKey pointKey = new VertexKey(ordered[i].Value);
                        if (pointKey.Equals(lastKey) ||
                            pointKey.Equals(new VertexKey(end)))
                        {
                            continue;
                        }
                        rebuilt.Add(ordered[i].Value);
                        lastKey = pointKey;
                        changed = true;
                        CountChamferSegmentSplit(
                            record,
                            key,
                            boundaries,
                            sourceBoundarySegmentKeys,
                            sharedSpans,
                            ref stats);
                    }
                }

                if (changed)
                {
                    RemoveClosingDuplicate(rebuilt);
                    record.Face = new PolygonFace(
                        rebuilt,
                        record.Face.Normal,
                        record.Face.Feature,
                        record.Face.FeatureStrength);
                }
            }

            List<ChamferExpectedVertexBoundary> rebuiltBoundaries =
                new List<ChamferExpectedVertexBoundary>(
                    boundaries.Count + uniqueSplitCount);
            for (int i = 0; i < boundaries.Count; i++)
            {
                ChamferExpectedVertexBoundary boundary = boundaries[i];
                if (!splitPlans.TryGetValue(
                        boundary.Key,
                        out List<ChamferSplitPoint> plan))
                {
                    rebuiltBoundaries.Add(boundary);
                    continue;
                }
                AppendSplitChamferBoundary(
                    rebuiltBoundaries,
                    boundary,
                    plan,
                    ref stats);
            }
            boundaries.Clear();
            boundaries.AddRange(rebuiltBoundaries);

            ApplyChamferSourceBoundarySplitPlans(
                sourceBoundaryRecords,
                splitPlans,
                ref stats);
            return uniqueSplitCount;
        }

        private static void CountChamferSegmentSplit(
            ChamferProvisionalFaceRecord record,
            TopologyEdgeKey key,
            List<ChamferExpectedVertexBoundary> boundaries,
            HashSet<TopologyEdgeKey> sourceBoundarySegmentKeys,
            Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
            ref ChamferEmissionStats stats)
        {
            if (sourceBoundarySegmentKeys.Contains(key))
            {
                return;
            }

            bool isExpectedBoundary = false;
            for (int i = 0; i < boundaries.Count; i++)
            {
                if (!boundaries[i].Key.Equals(key))
                {
                    continue;
                }
                isExpectedBoundary = true;
                break;
            }

            if (record.Kind == ChamferProvisionalFaceKind.BevelStrip)
            {
                return;
            }
            if (isExpectedBoundary)
            {
                return;
            }
            foreach (ChamferSharedEdgeSpan span in sharedSpans.Values)
            {
                TopologyEdgeKey sharedKey = new TopologyEdgeKey(
                    new VertexKey(span.SharedAtVertexA),
                    new VertexKey(span.SharedAtVertexB));
                if (sharedKey.Equals(key))
                {
                    return;
                }
            }
        }

        private static void AppendSplitChamferBoundary(
            List<ChamferExpectedVertexBoundary> output,
            ChamferExpectedVertexBoundary boundary,
            List<ChamferSplitPoint> plan,
            ref ChamferEmissionStats stats)
        {
            Vector3 segment = boundary.End - boundary.Start;
            float lengthSqr = segment.sqrMagnitude;
            if (lengthSqr <= MinimumEdgeLengthSqr)
            {
                output.Add(boundary);
                return;
            }
            List<KeyValuePair<float, Vector3>> ordered =
                new List<KeyValuePair<float, Vector3>>(plan.Count);
            for (int i = 0; i < plan.Count; i++)
            {
                float t = Vector3.Dot(
                    plan[i].Position - boundary.Start,
                    segment) / lengthSqr;
                ordered.Add(new KeyValuePair<float, Vector3>(
                    t,
                    plan[i].Position));
            }
            ordered.Sort((left, right) => left.Key.CompareTo(right.Key));

            Vector3 current = boundary.Start;
            for (int i = 0; i < ordered.Count; i++)
            {
                Vector3 next = ordered[i].Value;
                if (new VertexKey(current).Equals(new VertexKey(next)))
                {
                    continue;
                }
                AddExpectedVertexBoundary(
                    output,
                    boundary.SourceVertexIndex,
                    boundary.SourceEdgeIndex,
                    boundary.FaceIndex,
                    boundary.Kind,
                    current,
                    next);
                current = next;
            }
            if (!new VertexKey(current).Equals(new VertexKey(boundary.End)))
            {
                AddExpectedVertexBoundary(
                    output,
                    boundary.SourceVertexIndex,
                    boundary.SourceEdgeIndex,
                    boundary.FaceIndex,
                    boundary.Kind,
                    current,
                    boundary.End);
            }
        }

        #endregion
    }
}
