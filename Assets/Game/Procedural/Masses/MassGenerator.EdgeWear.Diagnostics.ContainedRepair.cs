using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear contained-owner boundary-guided repair

        private static bool TryBuildChamferContainedGuidedResidualCycles(
            List<Vector3> ownerVertices,
            List<List<ChamferProvisionalFaceRecord>> patchGroups,
            ChamferRepartitionProjection projection,
            float tolerance,
            out List<List<Vector3>> residualCycles)
        {
            residualCycles = new List<List<Vector3>>();
            if (ownerVertices == null || ownerVertices.Count < 3 ||
                patchGroups == null || patchGroups.Count == 0)
            {
                return false;
            }

            List<Vector3> currentCycle = new List<Vector3>(ownerVertices);
            for (int groupIndex = 0;
                 groupIndex < patchGroups.Count;
                 groupIndex++)
            {
                if (!TryBuildChamferContainedOrderedPatchLoop(
                        patchGroups[groupIndex],
                        projection,
                        tolerance,
                        out List<Vector3> patchLoop) ||
                    !TryApplyChamferContainedBoundaryNotch(
                        currentCycle,
                        patchLoop,
                        projection,
                        tolerance,
                        out List<Vector3> updatedCycle))
                {
                    residualCycles.Clear();
                    return false;
                }
                currentCycle = updatedCycle;
            }

            residualCycles.Add(currentCycle);
            return true;
        }

        private static bool TryBuildChamferContainedOrderedPatchLoop(
            List<ChamferProvisionalFaceRecord> patchRecords,
            ChamferRepartitionProjection projection,
            float tolerance,
            out List<Vector3> orderedLoop)
        {
            orderedLoop = new List<Vector3>();
            List<ChamferBoundarySegment> segments =
                BuildChamferPatchBoundarySegments(patchRecords);
            if (segments.Count < 3)
            {
                return false;
            }

            Dictionary<VertexKey, int> outgoing =
                new Dictionary<VertexKey, int>();
            Dictionary<VertexKey, int> incoming =
                new Dictionary<VertexKey, int>();
            for (int i = 0; i < segments.Count; i++)
            {
                VertexKey start = new VertexKey(segments[i].Start);
                VertexKey end = new VertexKey(segments[i].End);
                if (start.Equals(end) || outgoing.ContainsKey(start))
                {
                    return false;
                }
                outgoing.Add(start, i);
                incoming.TryGetValue(end, out int count);
                incoming[end] = count + 1;
            }
            foreach (KeyValuePair<VertexKey, int> pair in outgoing)
            {
                if (!incoming.TryGetValue(pair.Key, out int count) ||
                    count != 1)
                {
                    return false;
                }
            }

            VertexKey first = default;
            bool hasFirst = false;
            foreach (VertexKey key in outgoing.Keys)
            {
                if (!hasFirst || key.CompareTo(first) < 0)
                {
                    first = key;
                    hasFirst = true;
                }
            }
            if (!hasFirst)
            {
                return false;
            }

            HashSet<int> visited = new HashSet<int>();
            VertexKey current = first;
            while (visited.Count < segments.Count)
            {
                if (!outgoing.TryGetValue(current, out int segmentIndex) ||
                    !visited.Add(segmentIndex))
                {
                    orderedLoop.Clear();
                    return false;
                }
                orderedLoop.Add(segments[segmentIndex].Start);
                current = new VertexKey(segments[segmentIndex].End);
            }
            if (!current.Equals(first) || orderedLoop.Count != segments.Count)
            {
                orderedLoop.Clear();
                return false;
            }

            float signedArea = CalculateChamferRepartitionSignedArea(
                orderedLoop,
                projection);
            if (Mathf.Abs(signedArea) <= tolerance * tolerance)
            {
                orderedLoop.Clear();
                return false;
            }
            if (signedArea < 0f)
            {
                orderedLoop.Reverse();
            }
            return true;
        }

        private static bool TryApplyChamferContainedBoundaryNotch(
            List<Vector3> sourceCycle,
            List<Vector3> patchLoop,
            ChamferRepartitionProjection projection,
            float tolerance,
            out List<Vector3> result)
        {
            result = new List<Vector3>();
            if (sourceCycle == null || sourceCycle.Count < 3 ||
                patchLoop == null || patchLoop.Count < 3)
            {
                return false;
            }

            List<Vector3> splitCycle = SplitChamferCycleAtPositions(
                sourceCycle,
                patchLoop,
                tolerance,
                out _);
            List<ChamferBoundarySegment> cycleBoundary =
                BuildChamferCycleBoundarySegments(splitCycle);
            bool[] sharedEdges = new bool[patchLoop.Count];
            int sharedCount = 0;
            for (int i = 0; i < patchLoop.Count; i++)
            {
                ChamferBoundarySegment patchEdge =
                    new ChamferBoundarySegment(
                        patchLoop[i],
                        patchLoop[(i + 1) % patchLoop.Count]);
                sharedEdges[i] = IsChamferContainedBoundarySegmentCovered(
                    patchEdge,
                    cycleBoundary,
                    tolerance);
                if (sharedEdges[i])
                {
                    sharedCount++;
                }
            }
            if (sharedCount <= 0 || sharedCount >= patchLoop.Count)
            {
                return false;
            }

            int sharedRunStart = -1;
            int sharedRunCount = 0;
            for (int i = 0; i < sharedEdges.Length; i++)
            {
                int previous = (i - 1 + sharedEdges.Length) %
                    sharedEdges.Length;
                if (sharedEdges[i] && !sharedEdges[previous])
                {
                    sharedRunStart = i;
                    sharedRunCount++;
                }
            }
            if (sharedRunCount != 1)
            {
                return false;
            }

            int sharedLength = 0;
            while (sharedLength < sharedEdges.Length &&
                sharedEdges[(sharedRunStart + sharedLength) %
                    sharedEdges.Length])
            {
                sharedLength++;
            }
            int sharedEndVertex = (sharedRunStart + sharedLength) %
                patchLoop.Count;
            VertexKey sharedStartKey = new VertexKey(
                patchLoop[sharedRunStart]);
            VertexKey sharedEndKey = new VertexKey(
                patchLoop[sharedEndVertex]);
            int cycleStartIndex = FindChamferCycleVertexIndex(
                splitCycle,
                sharedStartKey);
            int cycleEndIndex = FindChamferCycleVertexIndex(
                splitCycle,
                sharedEndKey);
            if (cycleStartIndex < 0 || cycleEndIndex < 0 ||
                cycleStartIndex == cycleEndIndex)
            {
                return false;
            }

            List<ChamferBoundarySegment> sharedSegments =
                new List<ChamferBoundarySegment>();
            for (int offset = 0; offset < sharedLength; offset++)
            {
                int edgeIndex = (sharedRunStart + offset) %
                    patchLoop.Count;
                sharedSegments.Add(new ChamferBoundarySegment(
                    patchLoop[edgeIndex],
                    patchLoop[(edgeIndex + 1) % patchLoop.Count]));
            }
            if (!IsChamferCycleForwardPathCovered(
                    splitCycle,
                    cycleStartIndex,
                    cycleEndIndex,
                    sharedSegments,
                    tolerance))
            {
                return false;
            }

            int currentCycleIndex = cycleEndIndex;
            while (true)
            {
                AddChamferUniqueCyclePosition(
                    result,
                    splitCycle[currentCycleIndex]);
                if (currentCycleIndex == cycleStartIndex)
                {
                    break;
                }
                currentCycleIndex =
                    (currentCycleIndex + 1) % splitCycle.Count;
                if (result.Count > splitCycle.Count + patchLoop.Count)
                {
                    result.Clear();
                    return false;
                }
            }

            int patchIndex = (sharedRunStart - 1 + patchLoop.Count) %
                patchLoop.Count;
            while (patchIndex != sharedEndVertex)
            {
                AddChamferUniqueCyclePosition(
                    result,
                    patchLoop[patchIndex]);
                patchIndex = (patchIndex - 1 + patchLoop.Count) %
                    patchLoop.Count;
                if (result.Count > splitCycle.Count + patchLoop.Count)
                {
                    result.Clear();
                    return false;
                }
            }
            RemoveChamferClosingDuplicate(result);
            if (result.Count < 3)
            {
                result.Clear();
                return false;
            }

            float signedArea = CalculateChamferRepartitionSignedArea(
                result,
                projection);
            if (Mathf.Abs(signedArea) <= tolerance * tolerance)
            {
                result.Clear();
                return false;
            }
            if (signedArea < 0f)
            {
                result.Reverse();
            }
            return true;
        }

        private static List<Vector3> SplitChamferCycleAtPositions(
            List<Vector3> source,
            List<Vector3> splitPositions,
            float tolerance,
            out int insertedCount)
        {
            List<Vector3> result = new List<Vector3>();
            insertedCount = 0;
            float toleranceSquared = tolerance * tolerance;
            for (int edgeIndex = 0; edgeIndex < source.Count; edgeIndex++)
            {
                Vector3 start = source[edgeIndex];
                Vector3 end = source[(edgeIndex + 1) % source.Count];
                AddChamferUniqueCyclePosition(result, start);
                List<KeyValuePair<float, Vector3>> interior =
                    new List<KeyValuePair<float, Vector3>>();
                for (int positionIndex = 0;
                     positionIndex < splitPositions.Count;
                     positionIndex++)
                {
                    Vector3 position = splitPositions[positionIndex];
                    if (!TryGetChamferSegmentInteriorParameter(
                            position,
                            start,
                            end,
                            toleranceSquared,
                            out float parameter))
                    {
                        continue;
                    }
                    bool duplicate = false;
                    for (int i = 0; i < interior.Count; i++)
                    {
                        if (new VertexKey(interior[i].Value).Equals(
                                new VertexKey(position)))
                        {
                            duplicate = true;
                            break;
                        }
                    }
                    if (!duplicate)
                    {
                        interior.Add(new KeyValuePair<float, Vector3>(
                            parameter,
                            position));
                    }
                }
                interior.Sort((left, right) =>
                    left.Key.CompareTo(right.Key));
                for (int i = 0; i < interior.Count; i++)
                {
                    int before = result.Count;
                    AddChamferUniqueCyclePosition(result, interior[i].Value);
                    if (result.Count > before)
                    {
                        insertedCount++;
                    }
                }
            }
            RemoveChamferClosingDuplicate(result);
            return result;
        }

        private static int SplitChamferContainedRepartitionEndpoints(
            List<ChamferProvisionalFaceRecord> patchRecords,
            List<ChamferProvisionalFaceRecord> transformedRecords,
            float minimumStableEdgeLength)
        {
            List<ChamferBoundarySegment> patchBoundary =
                BuildChamferPatchBoundarySegments(patchRecords);
            List<Vector3> endpoints = BuildChamferBoundaryEndpointList(
                patchBoundary);
            return SplitChamferContainedRepartitionEndpoints(
                endpoints,
                transformedRecords,
                minimumStableEdgeLength);
        }

        private static int SplitChamferContainedRepartitionEndpoints(
            List<Vector3> endpoints,
            List<ChamferProvisionalFaceRecord> transformedRecords,
            float minimumStableEdgeLength)
        {
            if (endpoints == null || endpoints.Count == 0)
            {
                return 0;
            }
            float tolerance = CalculateTopologyTJunctionTolerance(
                minimumStableEdgeLength);
            int insertedTotal = 0;
            for (int recordIndex = 0;
                 recordIndex < transformedRecords.Count;
                 recordIndex++)
            {
                ChamferProvisionalFaceRecord record =
                    transformedRecords[recordIndex];
                PolygonFace face = record.Face;
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 3)
                {
                    continue;
                }
                List<Vector3> split = SplitChamferCycleAtPositions(
                    face.Vertices,
                    endpoints,
                    tolerance,
                    out int inserted);
                if (inserted <= 0 || split.Count < 3)
                {
                    continue;
                }
                record.Face = new PolygonFace(
                    split,
                    face.Normal,
                    face.Feature,
                    face.FeatureStrength);
                insertedTotal += inserted;
            }
            return insertedTotal;
        }

        private static List<Vector3> BuildChamferBoundaryEndpointList(
            List<ChamferBoundarySegment> segments)
        {
            List<Vector3> result = new List<Vector3>();
            HashSet<VertexKey> keys = new HashSet<VertexKey>();
            for (int i = 0; i < segments.Count; i++)
            {
                AddChamferBoundaryEndpoint(
                    segments[i].Start,
                    result,
                    keys);
                AddChamferBoundaryEndpoint(
                    segments[i].End,
                    result,
                    keys);
            }
            return result;
        }

        private static void AddChamferBoundaryEndpoint(
            Vector3 position,
            List<Vector3> positions,
            HashSet<VertexKey> keys)
        {
            VertexKey key = new VertexKey(position);
            if (keys.Add(key))
            {
                positions.Add(position);
            }
        }

        private static List<ChamferBoundarySegment>
            BuildChamferCycleBoundarySegments(List<Vector3> cycle)
        {
            List<ChamferBoundarySegment> result =
                new List<ChamferBoundarySegment>();
            for (int i = 0; i < cycle.Count; i++)
            {
                result.Add(new ChamferBoundarySegment(
                    cycle[i],
                    cycle[(i + 1) % cycle.Count]));
            }
            return result;
        }

        private static bool IsChamferCycleForwardPathCovered(
            List<Vector3> cycle,
            int startIndex,
            int endIndex,
            List<ChamferBoundarySegment> coveringSegments,
            float tolerance)
        {
            int current = startIndex;
            int edgeCount = 0;
            while (current != endIndex)
            {
                int next = (current + 1) % cycle.Count;
                if (!IsChamferContainedBoundarySegmentCovered(
                        new ChamferBoundarySegment(
                            cycle[current],
                            cycle[next]),
                        coveringSegments,
                        tolerance))
                {
                    return false;
                }
                current = next;
                edgeCount++;
                if (edgeCount > cycle.Count)
                {
                    return false;
                }
            }
            return edgeCount > 0;
        }

        private static int FindChamferCycleVertexIndex(
            List<Vector3> cycle,
            VertexKey key)
        {
            int found = -1;
            for (int i = 0; i < cycle.Count; i++)
            {
                if (!new VertexKey(cycle[i]).Equals(key))
                {
                    continue;
                }
                if (found >= 0)
                {
                    return -1;
                }
                found = i;
            }
            return found;
        }

        private static bool TryGetChamferSegmentInteriorParameter(
            Vector3 point,
            Vector3 start,
            Vector3 end,
            float toleranceSquared,
            out float parameter)
        {
            parameter = 0f;
            Vector3 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= MinimumEdgeLengthSqr ||
                (point - start).sqrMagnitude <= toleranceSquared ||
                (point - end).sqrMagnitude <= toleranceSquared)
            {
                return false;
            }
            parameter = Vector3.Dot(point - start, segment) /
                lengthSquared;
            if (parameter <= 0f || parameter >= 1f)
            {
                return false;
            }
            Vector3 closest = start + segment * parameter;
            return (point - closest).sqrMagnitude <= toleranceSquared;
        }

        private static void AddChamferUniqueCyclePosition(
            List<Vector3> positions,
            Vector3 position)
        {
            if (positions.Count > 0 &&
                new VertexKey(positions[positions.Count - 1]).Equals(
                    new VertexKey(position)))
            {
                return;
            }
            positions.Add(position);
        }

        private static void RemoveChamferClosingDuplicate(
            List<Vector3> positions)
        {
            if (positions.Count > 1 &&
                new VertexKey(positions[0]).Equals(
                    new VertexKey(positions[positions.Count - 1])))
            {
                positions.RemoveAt(positions.Count - 1);
            }
        }

        #endregion
    }
}
