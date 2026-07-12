using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear patch construction and feasibility


        private static bool TryEmitAndAuditChamferVertexPatches(
            List<ChamferProvisionalFaceRecord> provisionalFaceRecords,
            ChamferVertexPatchPlan plan,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            float minimumPatchTriangleArea,
            Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
            ChamferTopologyContext context,
            List<ChamferExpectedVertexBoundary> normalizedVertexBoundaries,
            List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
            ref ChamferEmissionStats stats,
            out ChamferBuildArtifacts buildArtifacts,
            out string blocker)
        {
            buildArtifacts = null;
            blocker = string.Empty;
            List<ChamferProvisionalFaceRecord> prePatchFaceRecords =
                CloneChamferProvisionalFaceRecords(
                    provisionalFaceRecords);
            List<PolygonFace> prePatchFaces =
                ExtractChamferProvisionalFaces(prePatchFaceRecords);
            Dictionary<TopologyEdgeKey, int> prePatchUseCounts =
                BuildTopologyEdgeUseCounts(prePatchFaces);
            for (int loopIndex = 0;
                 loopIndex < plan.PatchLoops.Count;
                 loopIndex++)
            {
                int boundaryCount =
                    plan.PatchLoops[loopIndex].OrderedPositions.Count;
                stats.PatchTrianglesAttempted += Mathf.Max(
                    1,
                    boundaryCount - 2);
                stats.PatchMaximumBoundaryCount = Mathf.Max(
                    stats.PatchMaximumBoundaryCount,
                    boundaryCount);
            }

            HashSet<TopologyEdgeKey> allPatchBoundaryKeys =
                new HashSet<TopologyEdgeKey>();
            HashSet<int> failedSliverLoopIndices =
                new HashSet<int>();
            bool anyLoopFailure = false;
            string firstLoopBlocker = string.Empty;
            for (int loopIndex = 0;
                 loopIndex < plan.PatchLoops.Count;
                 loopIndex++)
            {
                ChamferVertexPatchLoop loop = plan.PatchLoops[loopIndex];
                List<Vector3> positions =
                    new List<Vector3>(loop.OrderedPositions);
                bool sliverDiagnosticCandidate = false;

                if (positions.Count >= 4)
                {
                    AuditChamferPatchTriangulationFeasibility(
                        loop,
                        positions,
                        minimumPatchTriangleArea,
                        ref stats);
                }
                if (loop.Kind ==
                        ChamferVertexPatchLoopKind.LocalClosedComponent &&
                    positions.Count == 4)
                {
                    sliverDiagnosticCandidate =
                        IsChamferSliverDiagnosticCandidate(
                            loop,
                            positions,
                            prePatchFaceRecords,
                            prePatchFaces,
                            prePatchUseCounts,
                            plan.FinalSourceBoundaryEdges,
                            minimumPatchTriangleArea,
                            minimumStableEdgeLength);
                }

                if (!TryCalculateChamferPatchOrderedNormal(
                        positions,
                        out Vector3 polygonNormal))
                {
                    stats.PatchNonFiniteFailureCount++;
                    stats.PatchLoopsFailed++;
                    string loopBlocker =
                        "a vertex-patch loop has no stable polygon normal";
                    if (string.IsNullOrEmpty(firstLoopBlocker))
                    {
                        firstLoopBlocker = loopBlocker;
                    }
                    anyLoopFailure = true;
                    continue;
                }
                if (Vector3.Dot(
                        polygonNormal,
                        loop.ExpectedNormal) < 0f)
                {
                    positions.Reverse();
                    TryCalculateChamferPatchOrderedNormal(
                        positions,
                        out polygonNormal);
                }
                if (!IsFinite(polygonNormal) ||
                    Vector3.Dot(
                        polygonNormal,
                        loop.ExpectedNormal) <= 0f)
                {
                    stats.PatchWindingFailureCount++;
                    stats.PatchLoopsFailed++;
                    string loopBlocker =
                        "a vertex-patch loop cannot align to its expected normal";
                    if (string.IsNullOrEmpty(firstLoopBlocker))
                    {
                        firstLoopBlocker = loopBlocker;
                    }
                    anyLoopFailure = true;
                    continue;
                }

                HashSet<TopologyEdgeKey> orientedBoundarySet =
                    new HashSet<TopologyEdgeKey>();
                for (int i = 0; i < positions.Count; i++)
                {
                    orientedBoundarySet.Add(new TopologyEdgeKey(
                        new VertexKey(positions[i]),
                        new VertexKey(positions[(i + 1) % positions.Count])));
                }
                if (orientedBoundarySet.Count !=
                        loop.OrderedBoundaryKeys.Count ||
                    !orientedBoundarySet.SetEquals(
                        loop.OrderedBoundaryKeys))
                {
                    stats.PatchDuplicateEdgeFailureCount++;
                    stats.PatchLoopsFailed++;
                    string loopBlocker =
                        "a vertex-patch loop changes its boundary key set during orientation";
                    if (string.IsNullOrEmpty(firstLoopBlocker))
                    {
                        firstLoopBlocker = loopBlocker;
                    }
                    anyLoopFailure = true;
                    continue;
                }
                allPatchBoundaryKeys.UnionWith(orientedBoundarySet);

                if (!TryTriangulateChamferVertexPatchLoop(
                        loop,
                        positions,
                        orientedBoundarySet,
                        minimumPatchTriangleArea,
                        ref stats,
                        out List<ChamferProvisionalFaceRecord> loopRecords,
                        out HashSet<TopologyEdgeKey> internalDiagonalKeys,
                        out bool usedEarClipping,
                        out string triangulationBlocker))
                {
                    if (sliverDiagnosticCandidate)
                    {
                        failedSliverLoopIndices.Add(loop.LoopIndex);
                    }
                    stats.PatchLoopsFailed++;
                    if (string.IsNullOrEmpty(firstLoopBlocker))
                    {
                        firstLoopBlocker = triangulationBlocker;
                    }
                    anyLoopFailure = true;
                    continue;
                }

                provisionalFaceRecords.AddRange(loopRecords);
                stats.PatchLoopsBuilt++;
                stats.PatchTrianglesBuilt += loopRecords.Count;
            }

            List<ChamferProvisionalFaceRecord> successfulPatchRecords =
                new List<ChamferProvisionalFaceRecord>();
            for (int recordIndex = prePatchFaceRecords.Count;
                 recordIndex < provisionalFaceRecords.Count;
                 recordIndex++)
            {
                if (provisionalFaceRecords[recordIndex].Kind ==
                    ChamferProvisionalFaceKind.VertexPatch)
                {
                    successfulPatchRecords.Add(
                        CloneChamferProvisionalFaceRecord(
                            provisionalFaceRecords[recordIndex]));
                }
            }
            List<ChamferContainedPatchCandidate>
                containedPatchCandidates =
                    BuildChamferContainedPatchCandidates(
                        successfulPatchRecords,
                        prePatchFaceRecords,
                        minimumStableEdgeLength,
                        minimumPatchTriangleArea);
            buildArtifacts = new ChamferBuildArtifacts(
                plan,
                failedSliverLoopIndices,
                prePatchFaceRecords,
                successfulPatchRecords,
                context,
                normalizedVertexBoundaries,
                sourceBoundaryRecords,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                minimumPatchTriangleArea,
                sharedSpans,
                containedPatchCandidates);

            if (anyLoopFailure)
            {
                stats.ReadyForChamferPatchTopology = 0;
                blocker = string.IsNullOrEmpty(firstLoopBlocker)
                    ? "one or more provisional vertex-patch loops failed"
                    : firstLoopBlocker;
                return false;
            }

            List<PolygonFace> finalFaces =
                ExtractChamferProvisionalFaces(provisionalFaceRecords);
            Dictionary<TopologyEdgeKey, int> finalUseCounts =
                BuildTopologyEdgeUseCounts(finalFaces);
            HashSet<TopologyEdgeKey> finalOpenEdges =
                new HashSet<TopologyEdgeKey>();
            foreach (KeyValuePair<TopologyEdgeKey, int> pair
                     in finalUseCounts)
            {
                if (pair.Value == 1)
                {
                    finalOpenEdges.Add(pair.Key);
                }
            }

            foreach (TopologyEdgeKey key in allPatchBoundaryKeys)
            {
                int uses = finalUseCounts.TryGetValue(
                    key,
                    out int useCount)
                    ? useCount
                    : 0;
                if (uses != 2)
                {
                    stats.PatchBoundaryUseFailureCount++;
                }
            }

            List<ChamferProvisionalSegmentRecord> finalPatchSegments =
                BuildChamferProvisionalSegmentRecords(
                    provisionalFaceRecords,
                    new List<ChamferExpectedVertexBoundary>(),
                    plan.FinalSourceBoundaryEdges,
                    sharedSpans,
                    allPatchBoundaryKeys);
            HashSet<TopologyEdgeKey> patchDiagonalKeys =
                new HashSet<TopologyEdgeKey>();
            for (int i = 0; i < finalPatchSegments.Count; i++)
            {
                ChamferProvisionalSegmentRecord segment =
                    finalPatchSegments[i];
                if (segment.FaceKind ==
                        ChamferProvisionalFaceKind.VertexPatch &&
                    segment.Role == ChamferSegmentRole.VertexPatchDiagonal)
                {
                    patchDiagonalKeys.Add(segment.Key);
                }
            }
            foreach (TopologyEdgeKey key in patchDiagonalKeys)
            {
                int uses = finalUseCounts.TryGetValue(
                    key,
                    out int useCount)
                    ? useCount
                    : 0;
                if (uses != 2)
                {
                    stats.PatchDiagonalUseFailureCount++;
                }
            }

            foreach (TopologyEdgeKey key in plan.FinalSourceBoundaryEdges)
            {
                int uses = finalUseCounts.TryGetValue(
                    key,
                    out int useCount)
                    ? useCount
                    : 0;
                if (uses != 1 || !finalOpenEdges.Contains(key))
                {
                    stats.FinalSourceBoundaryUseFailureCount++;
                }
            }
            foreach (TopologyEdgeKey key in finalOpenEdges)
            {
                if (!plan.FinalSourceBoundaryEdges.Contains(key))
                {
                    stats.FinalUnexpectedOpenEdgeCount++;
                }
            }

            EdgeWearTopologyStats finalTopology = AuditEdgeWearTopology(
                finalFaces,
                minimumStableEdgeLength);
            stats.FinalPatchNonManifoldEdgeCount =
                finalTopology.NonManifoldEdgeCount;
            stats.FinalPatchTJunctionCount = finalTopology.TJunctionCount;

            bool ready =
                stats.PatchLoopsBuilt == stats.PatchLoopsAttempted &&
                stats.PatchLoopsFailed == 0 &&
                stats.PatchTrianglesBuilt == stats.PatchTrianglesAttempted &&
                stats.PatchLoopConstructionFailureCount == 0 &&
                stats.PatchNonFiniteFailureCount == 0 &&
                stats.PatchAreaFailureCount == 0 &&
                stats.PatchWindingFailureCount == 0 &&
                stats.PatchDuplicateEdgeFailureCount == 0 &&
                stats.PatchProjectionFailureCount == 0 &&
                stats.PatchSelfIntersectionFailureCount == 0 &&
                stats.PatchEarSelectionFailureCount == 0 &&
                stats.PatchDiagonalIntersectionFailureCount == 0 &&
                stats.PatchBoundaryUseFailureCount == 0 &&
                stats.PatchDiagonalUseFailureCount == 0 &&
                stats.FinalSourceBoundaryUseFailureCount == 0 &&
                stats.FinalUnexpectedOpenEdgeCount == 0 &&
                stats.FinalPatchNonManifoldEdgeCount == 0 &&
                stats.FinalPatchTJunctionCount == 0 &&
                finalOpenEdges.SetEquals(plan.FinalSourceBoundaryEdges);
            stats.ReadyForChamferPatchTopology = ready ? 1 : 0;
            if (!ready)
            {
                blocker = "provisional vertex-patch topology does not satisfy the final source-boundary-only open-edge contract";
                return false;
            }
            return true;
        }

        private static bool TryTriangulateChamferVertexPatchLoop(
            ChamferVertexPatchLoop loop,
            List<Vector3> positions,
            HashSet<TopologyEdgeKey> boundaryKeys,
            float minimumPatchTriangleArea,
            ref ChamferEmissionStats stats,
            out List<ChamferProvisionalFaceRecord> loopRecords,
            out HashSet<TopologyEdgeKey> internalDiagonalKeys,
            out bool usedEarClipping,
            out string blocker)
        {
            loopRecords = new List<ChamferProvisionalFaceRecord>();
            internalDiagonalKeys = new HashSet<TopologyEdgeKey>();
            usedEarClipping = positions.Count > 3;
            blocker = string.Empty;
            if (positions.Count < 3)
            {
                stats.PatchLoopConstructionFailureCount++;
                blocker = "a vertex-patch loop has fewer than three boundary positions";
                return false;
            }

            List<List<Vector3>> triangles = new List<List<Vector3>>();
            if (positions.Count == 3)
            {
                triangles.Add(new List<Vector3>
                {
                    positions[0],
                    positions[1],
                    positions[2]
                });
            }
            else
            {
                if (!TryProjectChamferPatchLoop(
                        positions,
                        loop.ExpectedNormal,
                        out List<Vector2> projected,
                        out float projectedSignedArea,
                        out float projectionEpsilon))
                {
                    stats.PatchProjectionFailureCount++;
                    blocker = "a vertex-patch loop cannot be projected onto a stable expected-normal basis";
                    return false;
                }
                if (projectedSignedArea < 0f)
                {
                    positions.Reverse();
                    projected.Reverse();
                    projectedSignedArea = -projectedSignedArea;
                }
                if (projectedSignedArea <=
                    projectionEpsilon * projectionEpsilon)
                {
                    stats.PatchProjectionFailureCount++;
                    blocker = "a vertex-patch loop has insufficient projected area";
                    return false;
                }
                if (ChamferPatchPolygonSelfIntersects(
                        projected,
                        projectionEpsilon,
                        out ChamferPatchIntersectionEvidence
                            intersectionEvidence))
                {
                    stats.PatchSelfIntersectionFailureCount++;
                    blocker = "a projected vertex-patch loop self-intersects";
                    return false;
                }
                if (!TryEarClipChamferPatchLoop(
                        loop,
                        positions,
                        projected,
                        boundaryKeys,
                        minimumPatchTriangleArea,
                        projectionEpsilon,
                        ref stats,
                        out triangles,
                        out internalDiagonalKeys,
                        out blocker))
                {
                    return false;
                }
            }

            if (triangles.Count != positions.Count - 2)
            {
                stats.PatchLoopConstructionFailureCount++;
                blocker = "a vertex-patch triangulation produced the wrong triangle count";
                return false;
            }

            Dictionary<TopologyEdgeKey, int> loopEdgeUses =
                new Dictionary<TopologyEdgeKey, int>();
            for (int triangleIndex = 0;
                 triangleIndex < triangles.Count;
                 triangleIndex++)
            {
                List<Vector3> triangle = triangles[triangleIndex];
                if (!TryCreateChamferVertexPatchTriangle(
                        loop,
                        triangle,
                        triangleIndex,
                        minimumPatchTriangleArea,
                        ref stats,
                        out ChamferProvisionalFaceRecord record,
                        out blocker))
                {
                    return false;
                }
                loopRecords.Add(record);
                HashSet<TopologyEdgeKey> triangleKeys =
                    BuildChamferFaceEdgeKeySet(triangle);
                foreach (TopologyEdgeKey key in triangleKeys)
                {
                    if (!loopEdgeUses.ContainsKey(key))
                    {
                        loopEdgeUses[key] = 0;
                    }
                    loopEdgeUses[key]++;
                }
            }

            foreach (TopologyEdgeKey key in boundaryKeys)
            {
                if (!loopEdgeUses.TryGetValue(key, out int uses) ||
                    uses != 1)
                {
                    stats.PatchBoundaryUseFailureCount++;
                    blocker = "a vertex-patch triangulation does not consume each loop boundary exactly once";
                    return false;
                }
            }

            HashSet<TopologyEdgeKey> auditedDiagonals =
                new HashSet<TopologyEdgeKey>();
            foreach (KeyValuePair<TopologyEdgeKey, int> pair
                     in loopEdgeUses)
            {
                if (boundaryKeys.Contains(pair.Key))
                {
                    continue;
                }
                auditedDiagonals.Add(pair.Key);
                if (pair.Value != 2)
                {
                    stats.PatchDiagonalUseFailureCount++;
                    blocker = "a vertex-patch internal diagonal does not have two loop-triangle uses";
                    return false;
                }
            }
            if (auditedDiagonals.Count != positions.Count - 3 ||
                !auditedDiagonals.SetEquals(internalDiagonalKeys))
            {
                stats.PatchDiagonalUseFailureCount++;
                blocker = "a vertex-patch triangulation produced an unexpected internal diagonal set";
                return false;
            }
            return true;
        }

        private static bool TryCreateChamferVertexPatchTriangle(
            ChamferVertexPatchLoop loop,
            List<Vector3> triangle,
            int triangleIndex,
            float minimumPatchTriangleArea,
            ref ChamferEmissionStats stats,
            out ChamferProvisionalFaceRecord record,
            out string blocker)
        {
            record = null;
            blocker = string.Empty;
            if (triangle == null || triangle.Count != 3)
            {
                stats.PatchLoopConstructionFailureCount++;
                blocker = "a vertex-patch triangle record does not contain exactly three positions";
                return false;
            }
            if (!IsFinite(triangle[0]) ||
                !IsFinite(triangle[1]) ||
                !IsFinite(triangle[2]))
            {
                stats.PatchNonFiniteFailureCount++;
                blocker = "a vertex-patch triangle contains a non-finite position";
                return false;
            }
            HashSet<VertexKey> vertexKeys = new HashSet<VertexKey>
            {
                new VertexKey(triangle[0]),
                new VertexKey(triangle[1]),
                new VertexKey(triangle[2])
            };
            if (vertexKeys.Count != 3)
            {
                stats.PatchDuplicateEdgeFailureCount++;
                blocker = "a vertex-patch triangle has repeated topology vertices";
                return false;
            }

            float triangleArea = CalculatePolygonArea(triangle);
            if (triangleArea <= minimumPatchTriangleArea)
            {
                stats.PatchAreaFailureCount++;
                blocker = "a vertex-patch triangle has insufficient area";
                return false;
            }
            if (!TryCalculateChamferPatchTriangleNormal(
                    triangle[0],
                    triangle[1],
                    triangle[2],
                    out Vector3 triangleNormal))
            {
                stats.PatchNonFiniteFailureCount++;
                blocker = "a vertex-patch triangle has no finite normal";
                return false;
            }
            float normalAlignment = Vector3.Dot(
                triangleNormal,
                loop.ExpectedNormal);
            if (normalAlignment <= 0f)
            {
                stats.PatchWindingFailureCount++;
                blocker = "a vertex-patch triangle has invalid winding";
                return false;
            }
            HashSet<TopologyEdgeKey> triangleKeys =
                BuildChamferFaceEdgeKeySet(triangle);
            if (triangleKeys.Count != 3)
            {
                stats.PatchDuplicateEdgeFailureCount++;
                blocker = "a vertex-patch triangle contains a repeated edge";
                return false;
            }

            PolygonFace patchFace = new PolygonFace(
                triangle,
                triangleNormal,
                PolygonFaceFeature.ConvexEdgeWear,
                loop.FeatureStrength);
            record = new ChamferProvisionalFaceRecord(
                patchFace,
                ChamferProvisionalFaceKind.VertexPatch,
                -1,
                -1,
                loop.LoopIndex);
            return true;
        }

        private static bool TryProjectChamferPatchLoop(
            List<Vector3> positions,
            Vector3 expectedNormal,
            out List<Vector2> projected,
            out float signedArea,
            out float epsilon)
        {
            projected = new List<Vector2>();
            signedArea = 0f;
            epsilon = 0f;
            if (positions == null || positions.Count < 3 ||
                !IsFinite(expectedNormal) ||
                expectedNormal.sqrMagnitude <= 0.00000001f)
            {
                return false;
            }

            Vector3 normal = expectedNormal.normalized;
            Vector3 reference;
            float absX = Mathf.Abs(normal.x);
            float absY = Mathf.Abs(normal.y);
            float absZ = Mathf.Abs(normal.z);
            if (absX <= absY && absX <= absZ)
            {
                reference = Vector3.right;
            }
            else if (absY <= absZ)
            {
                reference = Vector3.up;
            }
            else
            {
                reference = Vector3.forward;
            }
            Vector3 tangent = Vector3.Cross(reference, normal);
            if (!IsFinite(tangent) ||
                tangent.sqrMagnitude <= 0.00000001f)
            {
                return false;
            }
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent);
            if (!IsFinite(bitangent) ||
                bitangent.sqrMagnitude <= 0.00000001f)
            {
                return false;
            }
            bitangent.Normalize();

            Vector3 origin = positions[0];
            float minimumX = float.PositiveInfinity;
            float minimumY = float.PositiveInfinity;
            float maximumX = float.NegativeInfinity;
            float maximumY = float.NegativeInfinity;
            for (int i = 0; i < positions.Count; i++)
            {
                if (!IsFinite(positions[i]))
                {
                    return false;
                }
                Vector3 offset = positions[i] - origin;
                Vector2 point = new Vector2(
                    Vector3.Dot(offset, tangent),
                    Vector3.Dot(offset, bitangent));
                if (!IsFiniteFloat(point.x) || !IsFiniteFloat(point.y))
                {
                    return false;
                }
                projected.Add(point);
                minimumX = Mathf.Min(minimumX, point.x);
                minimumY = Mathf.Min(minimumY, point.y);
                maximumX = Mathf.Max(maximumX, point.x);
                maximumY = Mathf.Max(maximumY, point.y);
            }
            float extent = Mathf.Max(
                maximumX - minimumX,
                maximumY - minimumY);
            if (!IsFiniteFloat(extent) || extent <= 0f)
            {
                return false;
            }
            epsilon = Mathf.Max(0.0000001f, extent * 0.000001f);
            signedArea = CalculateChamferPatchSignedArea(projected);
            return IsFiniteFloat(signedArea);
        }

        private static bool TryEarClipChamferPatchLoop(
            ChamferVertexPatchLoop loop,
            List<Vector3> positions,
            List<Vector2> projected,
            HashSet<TopologyEdgeKey> boundaryKeys,
            float minimumPatchTriangleArea,
            float epsilon,
            ref ChamferEmissionStats stats,
            out List<List<Vector3>> triangles,
            out HashSet<TopologyEdgeKey> internalDiagonalKeys,
            out string blocker)
        {
            triangles = new List<List<Vector3>>();
            internalDiagonalKeys = new HashSet<TopologyEdgeKey>();
            blocker = string.Empty;
            List<int> remaining = new List<int>();
            for (int i = 0; i < positions.Count; i++)
            {
                remaining.Add(i);
            }

            while (remaining.Count > 3)
            {
                int selectedRemainingIndex = -1;
                int selectedOriginalIndex = int.MaxValue;
                TopologyEdgeKey selectedDiagonal = default;
                bool sawDiagonalIntersection = false;
                for (int remainingIndex = 0;
                     remainingIndex < remaining.Count;
                     remainingIndex++)
                {
                    int previousIndex = remaining[
                        (remainingIndex - 1 + remaining.Count) %
                        remaining.Count];
                    int currentIndex = remaining[remainingIndex];
                    int nextIndex = remaining[
                        (remainingIndex + 1) % remaining.Count];
                    Vector2 previous = projected[previousIndex];
                    Vector2 current = projected[currentIndex];
                    Vector2 next = projected[nextIndex];
                    float turn = ChamferPatchCross2D(
                        previous,
                        current,
                        next);
                    if (turn <= epsilon * epsilon)
                    {
                        continue;
                    }

                    bool containsOtherPoint = false;
                    for (int candidateIndex = 0;
                         candidateIndex < remaining.Count;
                         candidateIndex++)
                    {
                        int pointIndex = remaining[candidateIndex];
                        if (pointIndex == previousIndex ||
                            pointIndex == currentIndex ||
                            pointIndex == nextIndex)
                        {
                            continue;
                        }
                        if (ChamferPatchPointInOrOnTriangle(
                                projected[pointIndex],
                                previous,
                                current,
                                next,
                                epsilon))
                        {
                            containsOtherPoint = true;
                            break;
                        }
                    }
                    if (containsOtherPoint)
                    {
                        continue;
                    }

                    TopologyEdgeKey diagonal = new TopologyEdgeKey(
                        new VertexKey(positions[previousIndex]),
                        new VertexKey(positions[nextIndex]));
                    if (boundaryKeys.Contains(diagonal) ||
                        internalDiagonalKeys.Contains(diagonal))
                    {
                        continue;
                    }
                    if (ChamferPatchDiagonalIntersectsRemainingBoundary(
                            previousIndex,
                            nextIndex,
                            remaining,
                            projected,
                            epsilon))
                    {
                        sawDiagonalIntersection = true;
                        continue;
                    }

                    List<Vector3> candidateTriangle = new List<Vector3>
                    {
                        positions[previousIndex],
                        positions[currentIndex],
                        positions[nextIndex]
                    };
                    float candidateArea =
                        CalculatePolygonArea(candidateTriangle);
                    if (candidateArea <= minimumPatchTriangleArea)
                    {
                        continue;
                    }
                    if (!TryCalculateChamferPatchTriangleNormal(
                            candidateTriangle[0],
                            candidateTriangle[1],
                            candidateTriangle[2],
                            out Vector3 candidateNormal) ||
                        Vector3.Dot(
                            candidateNormal,
                            loop.ExpectedNormal) <= 0f)
                    {
                        continue;
                    }

                    if (currentIndex < selectedOriginalIndex ||
                        (currentIndex == selectedOriginalIndex &&
                         CompareTopologyEdgeKeys(
                             diagonal,
                             selectedDiagonal) < 0))
                    {
                        selectedRemainingIndex = remainingIndex;
                        selectedOriginalIndex = currentIndex;
                        selectedDiagonal = diagonal;
                    }
                }

                if (selectedRemainingIndex < 0)
                {
                    if (sawDiagonalIntersection)
                    {
                        stats.PatchDiagonalIntersectionFailureCount++;
                        blocker = "no deterministic ear remains without an intersecting diagonal";
                    }
                    else
                    {
                        stats.PatchEarSelectionFailureCount++;
                        blocker = "no valid deterministic ear remains for the projected vertex-patch loop";
                    }
                    return false;
                }

                int previousSelectedIndex = remaining[
                    (selectedRemainingIndex - 1 + remaining.Count) %
                    remaining.Count];
                int currentSelectedIndex =
                    remaining[selectedRemainingIndex];
                int nextSelectedIndex = remaining[
                    (selectedRemainingIndex + 1) % remaining.Count];
                triangles.Add(new List<Vector3>
                {
                    positions[previousSelectedIndex],
                    positions[currentSelectedIndex],
                    positions[nextSelectedIndex]
                });
                internalDiagonalKeys.Add(selectedDiagonal);
                remaining.RemoveAt(selectedRemainingIndex);
            }

            if (remaining.Count != 3)
            {
                stats.PatchEarSelectionFailureCount++;
                blocker = "ear clipping did not finish with exactly one final triangle";
                return false;
            }
            triangles.Add(new List<Vector3>
            {
                positions[remaining[0]],
                positions[remaining[1]],
                positions[remaining[2]]
            });
            return true;
        }

        private static bool TryCalculateChamferPatchOrderedNormal(
            List<Vector3> positions,
            out Vector3 normal)
        {
            Vector3 accumulated = Vector3.zero;
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 current = positions[i];
                Vector3 next = positions[(i + 1) % positions.Count];
                if (!IsFinite(current) || !IsFinite(next))
                {
                    normal = Vector3.zero;
                    return false;
                }
                accumulated.x +=
                    (current.y - next.y) * (current.z + next.z);
                accumulated.y +=
                    (current.z - next.z) * (current.x + next.x);
                accumulated.z +=
                    (current.x - next.x) * (current.y + next.y);
            }
            if (!IsFinite(accumulated) ||
                accumulated.sqrMagnitude <= 0f)
            {
                normal = Vector3.zero;
                return false;
            }
            float magnitude = Mathf.Sqrt(accumulated.sqrMagnitude);
            if (!IsFiniteFloat(magnitude) || magnitude <= 0f)
            {
                normal = Vector3.zero;
                return false;
            }
            normal = accumulated / magnitude;
            return IsFinite(normal);
        }

        private static bool TryCalculateChamferPatchTriangleNormal(
            Vector3 first,
            Vector3 second,
            Vector3 third,
            out Vector3 normal)
        {
            Vector3 rawNormal = Vector3.Cross(
                second - first,
                third - first);
            if (!IsFinite(rawNormal) || rawNormal.sqrMagnitude <= 0f)
            {
                normal = Vector3.zero;
                return false;
            }
            float magnitude = Mathf.Sqrt(rawNormal.sqrMagnitude);
            if (!IsFiniteFloat(magnitude) || magnitude <= 0f)
            {
                normal = Vector3.zero;
                return false;
            }
            normal = rawNormal / magnitude;
            return IsFinite(normal);
        }

        private static bool TryBuildChamferDirectedBoundaryOwnership(
            ChamferVertexPatchLoop loop,
            List<Vector3> positions,
            List<PolygonFace> prePatchFaces,
            Dictionary<TopologyEdgeKey, int> prePatchUseCounts,
            out Dictionary<TopologyEdgeKey,
                ChamferDirectedBoundaryEdge> ownership,
            out bool reversed,
            out string failure)
        {
            ownership = new Dictionary<TopologyEdgeKey,
                ChamferDirectedBoundaryEdge>();
            reversed = false;
            failure = string.Empty;
            for (int boundaryIndex = 0;
                 boundaryIndex < loop.OrderedBoundaryKeys.Count;
                 boundaryIndex++)
            {
                TopologyEdgeKey key =
                    loop.OrderedBoundaryKeys[boundaryIndex];
                int occurrenceCount = 0;
                Vector3 existingStart = Vector3.zero;
                Vector3 existingEnd = Vector3.zero;
                int owningFaceIndex = -1;
                Vector3 owningFaceNormal = Vector3.zero;
                for (int faceIndex = 0;
                     faceIndex < prePatchFaces.Count;
                     faceIndex++)
                {
                    PolygonFace face = prePatchFaces[faceIndex];
                    for (int edgeIndex = 0;
                         edgeIndex < face.Vertices.Count;
                         edgeIndex++)
                    {
                        Vector3 start = face.Vertices[edgeIndex];
                        Vector3 end = face.Vertices[
                            (edgeIndex + 1) % face.Vertices.Count];
                        TopologyEdgeKey candidate = new TopologyEdgeKey(
                            new VertexKey(start),
                            new VertexKey(end));
                        if (!candidate.Equals(key))
                        {
                            continue;
                        }
                        occurrenceCount++;
                        existingStart = start;
                        existingEnd = end;
                        owningFaceIndex = faceIndex;
                        owningFaceNormal = face.Normal;
                    }
                }
                int useCount = prePatchUseCounts.TryGetValue(
                    key,
                    out int foundUseCount)
                    ? foundUseCount
                    : 0;
                if (occurrenceCount != 1 || useCount != 1)
                {
                    failure =
                        "boundary-edge-occurrence:" + boundaryIndex +
                        "/occurrences:" + occurrenceCount +
                        "/uses:" + useCount;
                    return false;
                }
                ownership[key] = new ChamferDirectedBoundaryEdge(
                    key,
                    existingStart,
                    existingEnd,
                    owningFaceIndex,
                    owningFaceNormal);
            }

            bool forwardMatches =
                DoesChamferDirectedBoundaryCycleMatch(
                    positions,
                    ownership);
            List<Vector3> reversedPositions =
                new List<Vector3>(positions);
            reversedPositions.Reverse();
            bool reverseMatches =
                DoesChamferDirectedBoundaryCycleMatch(
                    reversedPositions,
                    ownership);
            if (!forwardMatches && !reverseMatches)
            {
                failure = "directed-boundary-conflict";
                return false;
            }
            if (!forwardMatches && reverseMatches)
            {
                positions.Reverse();
                reversed = true;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 patchStart = positions[i];
                Vector3 patchEnd = positions[
                    (i + 1) % positions.Count];
                TopologyEdgeKey key = new TopologyEdgeKey(
                    new VertexKey(patchStart),
                    new VertexKey(patchEnd));
                ChamferDirectedBoundaryEdge old = ownership[key];
                ownership[key] = old.WithPatchDirection(
                    patchStart,
                    patchEnd);
            }
            return true;
        }

        private static bool DoesChamferDirectedBoundaryCycleMatch(
            List<Vector3> positions,
            Dictionary<TopologyEdgeKey,
                ChamferDirectedBoundaryEdge> ownership)
        {
            if (positions == null || positions.Count != ownership.Count)
            {
                return false;
            }
            for (int i = 0; i < positions.Count; i++)
            {
                VertexKey patchStart = new VertexKey(positions[i]);
                VertexKey patchEnd = new VertexKey(positions[
                    (i + 1) % positions.Count]);
                TopologyEdgeKey key = new TopologyEdgeKey(
                    patchStart,
                    patchEnd);
                if (!ownership.TryGetValue(
                        key,
                        out ChamferDirectedBoundaryEdge edge) ||
                    !patchStart.Equals(edge.ExistingEndKey) ||
                    !patchEnd.Equals(edge.ExistingStartKey))
                {
                    return false;
                }
            }
            return true;
        }

        private static List<List<ChamferDirectedTriangleIndex>>
            EnumerateChamferDirectedTriangulations(
                int startIndex,
                int endIndex,
                int boundaryCount,
                Dictionary<int,
                    List<List<ChamferDirectedTriangleIndex>>> memo)
        {
            int memoKey = startIndex * boundaryCount + endIndex;
            if (memo.TryGetValue(
                    memoKey,
                    out List<List<ChamferDirectedTriangleIndex>> cached))
            {
                return cached;
            }
            List<List<ChamferDirectedTriangleIndex>> result =
                new List<List<ChamferDirectedTriangleIndex>>();
            if (endIndex <= startIndex + 1)
            {
                result.Add(new List<ChamferDirectedTriangleIndex>());
                memo[memoKey] = result;
                return result;
            }
            for (int splitIndex = startIndex + 1;
                 splitIndex < endIndex;
                 splitIndex++)
            {
                List<List<ChamferDirectedTriangleIndex>> left =
                    EnumerateChamferDirectedTriangulations(
                        startIndex,
                        splitIndex,
                        boundaryCount,
                        memo);
                List<List<ChamferDirectedTriangleIndex>> right =
                    EnumerateChamferDirectedTriangulations(
                        splitIndex,
                        endIndex,
                        boundaryCount,
                        memo);
                for (int leftIndex = 0;
                     leftIndex < left.Count;
                     leftIndex++)
                {
                    for (int rightIndex = 0;
                         rightIndex < right.Count;
                         rightIndex++)
                    {
                        List<ChamferDirectedTriangleIndex> triangles =
                            new List<ChamferDirectedTriangleIndex>(
                                1 + left[leftIndex].Count +
                                right[rightIndex].Count)
                            {
                                new ChamferDirectedTriangleIndex(
                                    startIndex,
                                    splitIndex,
                                    endIndex)
                            };
                        triangles.AddRange(left[leftIndex]);
                        triangles.AddRange(right[rightIndex]);
                        result.Add(triangles);
                    }
                }
            }
            memo[memoKey] = result;
            return result;
        }

        private static ChamferDirectedTriangulationEvaluation
            EvaluateChamferDirectedTriangulation(
                ChamferVertexPatchLoop loop,
                List<Vector3> positions,
                List<ChamferDirectedTriangleIndex> triangleIndices,
                Dictionary<TopologyEdgeKey,
                    ChamferDirectedBoundaryEdge> boundaryOwnership,
                List<PolygonFace> prePatchFaces,
                Dictionary<TopologyEdgeKey, int> prePatchUseCounts,
                HashSet<TopologyEdgeKey> finalSourceBoundaryEdges,
                float minimumPatchTriangleArea,
                float minimumStableEdgeLength)
        {
            ChamferDirectedTriangulationEvaluation result =
                new ChamferDirectedTriangulationEvaluation();
            int boundaryCount = positions.Count;
            if (triangleIndices.Count != boundaryCount - 2)
            {
                result.Failure = "triangle-count";
                return result;
            }

            Dictionary<VertexKey, int> boundaryIndices =
                new Dictionary<VertexKey, int>();
            for (int i = 0; i < positions.Count; i++)
            {
                boundaryIndices[new VertexKey(positions[i])] = i;
            }
            Dictionary<TopologyEdgeKey,
                List<ChamferDirectedEdgeUse>> edgeUses =
                    new Dictionary<TopologyEdgeKey,
                        List<ChamferDirectedEdgeUse>>();
            List<ChamferDirectedTriangleGeometry> triangles =
                new List<ChamferDirectedTriangleGeometry>(
                    triangleIndices.Count);
            result.MinimumArea = float.PositiveInfinity;
            result.MinimumQuality = float.PositiveInfinity;
            for (int triangleIndex = 0;
                 triangleIndex < triangleIndices.Count;
                 triangleIndex++)
            {
                ChamferDirectedTriangleIndex indices =
                    triangleIndices[triangleIndex];
                if (!TryCreateChamferDirectedTriangleGeometry(
                        positions[indices.First],
                        positions[indices.Second],
                        positions[indices.Third],
                        triangleIndex,
                        minimumPatchTriangleArea,
                        out ChamferDirectedTriangleGeometry triangle,
                        out string triangleFailure))
                {
                    result.Failure = triangleFailure;
                    return result;
                }
                triangles.Add(triangle);
                result.MinimumArea = Mathf.Min(
                    result.MinimumArea,
                    triangle.Area);
                result.MinimumQuality = Mathf.Min(
                    result.MinimumQuality,
                    triangle.Quality);
                AddChamferDirectedEdgeUse(
                    edgeUses,
                    triangle.First,
                    triangle.Second,
                    triangleIndex);
                AddChamferDirectedEdgeUse(
                    edgeUses,
                    triangle.Second,
                    triangle.Third,
                    triangleIndex);
                AddChamferDirectedEdgeUse(
                    edgeUses,
                    triangle.Third,
                    triangle.First,
                    triangleIndex);
            }

            HashSet<TopologyEdgeKey> diagonalKeys =
                new HashSet<TopologyEdgeKey>();
            foreach (KeyValuePair<TopologyEdgeKey,
                     List<ChamferDirectedEdgeUse>> pair in edgeUses)
            {
                if (finalSourceBoundaryEdges.Contains(pair.Key))
                {
                    result.Failure = "final-source-boundary-overlap";
                    return result;
                }
                int existingUses = prePatchUseCounts.TryGetValue(
                    pair.Key,
                    out int foundUses)
                    ? foundUses
                    : 0;
                if (boundaryOwnership.TryGetValue(
                        pair.Key,
                        out ChamferDirectedBoundaryEdge boundary))
                {
                    if (pair.Value.Count != 1 || existingUses != 1)
                    {
                        result.Failure = "boundary-incidence";
                        return result;
                    }
                    ChamferDirectedEdgeUse use = pair.Value[0];
                    if (!use.StartKey.Equals(boundary.PatchStartKey) ||
                        !use.EndKey.Equals(boundary.PatchEndKey))
                    {
                        result.Failure = "boundary-direction";
                        return result;
                    }
                }
                else
                {
                    diagonalKeys.Add(pair.Key);
                    if (pair.Value.Count != 2 || existingUses != 0)
                    {
                        result.Failure = "diagonal-incidence";
                        return result;
                    }
                    ChamferDirectedEdgeUse first = pair.Value[0];
                    ChamferDirectedEdgeUse second = pair.Value[1];
                    if (!first.StartKey.Equals(second.EndKey) ||
                        !first.EndKey.Equals(second.StartKey))
                    {
                        result.Failure = "diagonal-direction";
                        return result;
                    }
                }
                if (existingUses + pair.Value.Count != 2)
                {
                    result.Failure = "combined-use-count";
                    return result;
                }
            }
            if (edgeUses.Count !=
                    boundaryOwnership.Count + boundaryCount - 3 ||
                diagonalKeys.Count != boundaryCount - 3)
            {
                result.Failure = "edge-set-count";
                return result;
            }
            foreach (TopologyEdgeKey key in boundaryOwnership.Keys)
            {
                if (!edgeUses.ContainsKey(key))
                {
                    result.Failure = "missing-boundary-edge";
                    return result;
                }
            }
            result.DiagonalCodes = BuildChamferDirectedDiagonalCodes(
                diagonalKeys,
                boundaryIndices,
                boundaryCount);
            result.PassesIncidence = true;
            result.StageScore = 1;

            float intersectionEpsilon = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.001f);
            for (int firstIndex = 0;
                 firstIndex < triangles.Count;
                 firstIndex++)
            {
                for (int secondIndex = firstIndex + 1;
                     secondIndex < triangles.Count;
                     secondIndex++)
                {
                    if (ChamferDirectedTrianglesIntersectImproperly(
                            triangles[firstIndex],
                            triangles[secondIndex],
                            intersectionEpsilon))
                    {
                        result.Failure =
                            "triangle-intersection:" + firstIndex +
                            "/" + secondIndex;
                        return result;
                    }
                }
            }
            result.PassesTriangleIntersection = true;
            result.StageScore = 2;

            for (int triangleIndex = 0;
                 triangleIndex < triangles.Count;
                 triangleIndex++)
            {
                for (int faceIndex = 0;
                     faceIndex < prePatchFaces.Count;
                     faceIndex++)
                {
                    List<ChamferDirectedTriangleGeometry> faceTriangles =
                        BuildChamferDirectedFaceTriangles(
                            prePatchFaces[faceIndex],
                            faceIndex,
                            minimumPatchTriangleArea);
                    for (int faceTriangleIndex = 0;
                         faceTriangleIndex < faceTriangles.Count;
                         faceTriangleIndex++)
                    {
                        if (ChamferDirectedTrianglesIntersectImproperly(
                                triangles[triangleIndex],
                                faceTriangles[faceTriangleIndex],
                                intersectionEpsilon))
                        {
                            result.Failure =
                                "existing-face-intersection:" +
                                triangleIndex + "/" + faceIndex +
                                "/" + faceTriangleIndex;
                            return result;
                        }
                    }
                }
            }
            result.PassesExistingFaceIntersection = true;
            result.StageScore = 3;

            List<PolygonFace> combinedFaces =
                new List<PolygonFace>(
                    prePatchFaces.Count + triangles.Count);
            combinedFaces.AddRange(prePatchFaces);
            for (int i = 0; i < triangles.Count; i++)
            {
                combinedFaces.Add(new PolygonFace(
                    new List<Vector3>
                    {
                        triangles[i].First,
                        triangles[i].Second,
                        triangles[i].Third
                    },
                    triangles[i].Normal,
                    PolygonFaceFeature.ConvexEdgeWear,
                    loop.FeatureStrength));
            }
            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                combinedFaces,
                minimumStableEdgeLength);
            if (topology.TJunctionCount > 0)
            {
                result.TJunctionFailure = true;
                result.Failure = "t-junctions:" +
                    topology.TJunctionCount;
                return result;
            }
            if (topology.NonManifoldEdgeCount > 0)
            {
                result.NonManifoldFailure = true;
                result.Failure = "non-manifold:" +
                    topology.NonManifoldEdgeCount;
                return result;
            }
            result.StageScore = 4;
            result.MaximumInternalDihedral =
                CalculateChamferDirectedMaximumInternalDihedral(
                    diagonalKeys,
                    edgeUses,
                    triangles);
            result.MaximumBoundaryDihedral =
                CalculateChamferDirectedMaximumBoundaryDihedral(
                    boundaryOwnership,
                    edgeUses,
                    triangles);
            result.Feasible = true;
            result.Failure = "none";
            return result;
        }

        private static bool TryCreateChamferDirectedTriangleGeometry(
            Vector3 first,
            Vector3 second,
            Vector3 third,
            int triangleIndex,
            float minimumPatchTriangleArea,
            out ChamferDirectedTriangleGeometry triangle,
            out string failure)
        {
            triangle = null;
            failure = string.Empty;
            if (!IsFinite(first) || !IsFinite(second) || !IsFinite(third))
            {
                failure = "non-finite-triangle";
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
                failure = "repeated-triangle-vertex";
                return false;
            }
            Vector3 cross = Vector3.Cross(
                second - first,
                third - first);
            float twiceArea = cross.magnitude;
            float area = twiceArea * 0.5f;
            if (!IsFiniteFloat(area) || area <= minimumPatchTriangleArea)
            {
                failure = "triangle-area:" + area.ToString("G9");
                return false;
            }
            Vector3 normal = cross / twiceArea;
            if (!IsFinite(normal))
            {
                failure = "triangle-normal";
                return false;
            }
            float denominator =
                (second - first).sqrMagnitude +
                (third - second).sqrMagnitude +
                (first - third).sqrMagnitude;
            if (!IsFiniteFloat(denominator) || denominator <= 0f)
            {
                failure = "triangle-quality";
                return false;
            }
            float quality = 4f * Mathf.Sqrt(3f) * area /
                denominator;
            triangle = new ChamferDirectedTriangleGeometry(
                first,
                second,
                third,
                normal,
                area,
                quality,
                triangleIndex);
            return true;
        }

        private static void AddChamferDirectedEdgeUse(
            Dictionary<TopologyEdgeKey,
                List<ChamferDirectedEdgeUse>> edgeUses,
            Vector3 start,
            Vector3 end,
            int triangleIndex)
        {
            TopologyEdgeKey key = new TopologyEdgeKey(
                new VertexKey(start),
                new VertexKey(end));
            if (!edgeUses.TryGetValue(
                    key,
                    out List<ChamferDirectedEdgeUse> uses))
            {
                uses = new List<ChamferDirectedEdgeUse>(2);
                edgeUses[key] = uses;
            }
            uses.Add(new ChamferDirectedEdgeUse(
                start,
                end,
                triangleIndex));
        }

        private static List<int> BuildChamferDirectedDiagonalCodes(
            HashSet<TopologyEdgeKey> diagonalKeys,
            Dictionary<VertexKey, int> boundaryIndices,
            int boundaryCount)
        {
            List<int> codes = new List<int>(diagonalKeys.Count);
            foreach (TopologyEdgeKey key in diagonalKeys)
            {
                if (!boundaryIndices.TryGetValue(
                        key.First,
                        out int firstIndex) ||
                    !boundaryIndices.TryGetValue(
                        key.Second,
                        out int secondIndex))
                {
                    continue;
                }
                int minimum = Mathf.Min(firstIndex, secondIndex);
                int maximum = Mathf.Max(firstIndex, secondIndex);
                codes.Add(minimum * boundaryCount + maximum);
            }
            codes.Sort();
            return codes;
        }

        private static float CalculateChamferDirectedMaximumInternalDihedral(
            HashSet<TopologyEdgeKey> diagonalKeys,
            Dictionary<TopologyEdgeKey,
                List<ChamferDirectedEdgeUse>> edgeUses,
            List<ChamferDirectedTriangleGeometry> triangles)
        {
            float maximum = 0f;
            foreach (TopologyEdgeKey key in diagonalKeys)
            {
                List<ChamferDirectedEdgeUse> uses = edgeUses[key];
                if (uses.Count != 2)
                {
                    continue;
                }
                maximum = Mathf.Max(
                    maximum,
                    Vector3.Angle(
                        triangles[uses[0].TriangleIndex].Normal,
                        triangles[uses[1].TriangleIndex].Normal));
            }
            return maximum;
        }

        private static float CalculateChamferDirectedMaximumBoundaryDihedral(
            Dictionary<TopologyEdgeKey,
                ChamferDirectedBoundaryEdge> boundaryOwnership,
            Dictionary<TopologyEdgeKey,
                List<ChamferDirectedEdgeUse>> edgeUses,
            List<ChamferDirectedTriangleGeometry> triangles)
        {
            float maximum = 0f;
            foreach (KeyValuePair<TopologyEdgeKey,
                     ChamferDirectedBoundaryEdge> pair in boundaryOwnership)
            {
                if (!edgeUses.TryGetValue(
                        pair.Key,
                        out List<ChamferDirectedEdgeUse> uses) ||
                    uses.Count != 1)
                {
                    continue;
                }
                maximum = Mathf.Max(
                    maximum,
                    Vector3.Angle(
                        triangles[uses[0].TriangleIndex].Normal,
                        pair.Value.OwningFaceNormal));
            }
            return maximum;
        }

        private static List<ChamferDirectedTriangleGeometry>
            BuildChamferDirectedFaceTriangles(
                PolygonFace face,
                int faceIndex,
                float minimumPatchTriangleArea)
        {
            List<ChamferDirectedTriangleGeometry> triangles =
                new List<ChamferDirectedTriangleGeometry>();
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 3)
            {
                return triangles;
            }
            for (int i = 1; i < face.Vertices.Count - 1; i++)
            {
                if (TryCreateChamferDirectedTriangleGeometry(
                        face.Vertices[0],
                        face.Vertices[i],
                        face.Vertices[i + 1],
                        faceIndex * 1024 + i,
                        minimumPatchTriangleArea,
                        out ChamferDirectedTriangleGeometry triangle,
                        out _))
                {
                    triangles.Add(triangle);
                }
            }
            return triangles;
        }

        private static bool ChamferDirectedTrianglesIntersectImproperly(
            ChamferDirectedTriangleGeometry first,
            ChamferDirectedTriangleGeometry second,
            float epsilon)
        {
            List<VertexKey> sharedKeys = new List<VertexKey>();
            for (int i = 0; i < 3; i++)
            {
                VertexKey key = first.GetKey(i);
                if (second.ContainsKey(key))
                {
                    sharedKeys.Add(key);
                }
            }
            if (sharedKeys.Count == 3)
            {
                return true;
            }
            float normalAlignment = Mathf.Abs(Vector3.Dot(
                first.Normal,
                second.Normal));
            float planeDistance = Mathf.Abs(Vector3.Dot(
                second.First - first.First,
                first.Normal));
            bool coplanar = normalAlignment >= 0.9999f &&
                planeDistance <= epsilon;
            if (coplanar)
            {
                return ChamferDirectedCoplanarTrianglesOverlapImproperly(
                    first,
                    second,
                    sharedKeys,
                    epsilon);
            }

            for (int edgeIndex = 0; edgeIndex < 3; edgeIndex++)
            {
                Vector3 start = first.GetPosition(edgeIndex);
                Vector3 end = first.GetPosition((edgeIndex + 1) % 3);
                if (TryChamferDirectedSegmentTriangleIntersection(
                        start,
                        end,
                        second,
                        epsilon,
                        out Vector3 point) &&
                    !IsChamferDirectedAllowedSharedContact(
                        point,
                        first,
                        second,
                        sharedKeys,
                        epsilon))
                {
                    return true;
                }
            }
            for (int edgeIndex = 0; edgeIndex < 3; edgeIndex++)
            {
                Vector3 start = second.GetPosition(edgeIndex);
                Vector3 end = second.GetPosition((edgeIndex + 1) % 3);
                if (TryChamferDirectedSegmentTriangleIntersection(
                        start,
                        end,
                        first,
                        epsilon,
                        out Vector3 point) &&
                    !IsChamferDirectedAllowedSharedContact(
                        point,
                        first,
                        second,
                        sharedKeys,
                        epsilon))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ChamferDirectedCoplanarTrianglesOverlapImproperly(
            ChamferDirectedTriangleGeometry first,
            ChamferDirectedTriangleGeometry second,
            List<VertexKey> sharedKeys,
            float epsilon)
        {
            int dropAxis = GetChamferDirectedProjectionDropAxis(first.Normal);
            Vector2[] firstProjected = new Vector2[3];
            Vector2[] secondProjected = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                firstProjected[i] = ProjectChamferDirectedPoint(
                    first.GetPosition(i),
                    dropAxis);
                secondProjected[i] = ProjectChamferDirectedPoint(
                    second.GetPosition(i),
                    dropAxis);
            }
            for (int firstEdge = 0; firstEdge < 3; firstEdge++)
            {
                TopologyEdgeKey firstKey = new TopologyEdgeKey(
                    first.GetKey(firstEdge),
                    first.GetKey((firstEdge + 1) % 3));
                for (int secondEdge = 0; secondEdge < 3; secondEdge++)
                {
                    TopologyEdgeKey secondKey = new TopologyEdgeKey(
                        second.GetKey(secondEdge),
                        second.GetKey((secondEdge + 1) % 3));
                    if (!TryGetChamferPatchSegmentIntersectionEvidence(
                            firstProjected[firstEdge],
                            firstProjected[(firstEdge + 1) % 3],
                            secondProjected[secondEdge],
                            secondProjected[(secondEdge + 1) % 3],
                            epsilon,
                            firstEdge,
                            secondEdge,
                            out ChamferPatchIntersectionEvidence evidence))
                    {
                        continue;
                    }
                    if (firstKey.Equals(secondKey))
                    {
                        continue;
                    }
                    if (evidence.Type ==
                        ChamferPatchIntersectionType.EndpointTouch &&
                        ChamferDirectedEdgesShareEndpoint(
                            first,
                            firstEdge,
                            second,
                            secondEdge))
                    {
                        continue;
                    }
                    return true;
                }
            }
            for (int i = 0; i < 3; i++)
            {
                if (!second.ContainsKey(first.GetKey(i)) &&
                    ChamferDirectedPointStrictlyInsideTriangle2D(
                        firstProjected[i],
                        secondProjected[0],
                        secondProjected[1],
                        secondProjected[2],
                        epsilon))
                {
                    return true;
                }
                if (!first.ContainsKey(second.GetKey(i)) &&
                    ChamferDirectedPointStrictlyInsideTriangle2D(
                        secondProjected[i],
                        firstProjected[0],
                        firstProjected[1],
                        firstProjected[2],
                        epsilon))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ChamferDirectedEdgesShareEndpoint(
            ChamferDirectedTriangleGeometry first,
            int firstEdge,
            ChamferDirectedTriangleGeometry second,
            int secondEdge)
        {
            VertexKey firstStart = first.GetKey(firstEdge);
            VertexKey firstEnd = first.GetKey((firstEdge + 1) % 3);
            VertexKey secondStart = second.GetKey(secondEdge);
            VertexKey secondEnd = second.GetKey((secondEdge + 1) % 3);
            return firstStart.Equals(secondStart) ||
                firstStart.Equals(secondEnd) ||
                firstEnd.Equals(secondStart) ||
                firstEnd.Equals(secondEnd);
        }

        private static bool ChamferDirectedPointStrictlyInsideTriangle2D(
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
            bool positive = firstCross > areaEpsilon &&
                secondCross > areaEpsilon &&
                thirdCross > areaEpsilon;
            bool negative = firstCross < -areaEpsilon &&
                secondCross < -areaEpsilon &&
                thirdCross < -areaEpsilon;
            return positive || negative;
        }

        private static bool TryChamferDirectedSegmentTriangleIntersection(
            Vector3 start,
            Vector3 end,
            ChamferDirectedTriangleGeometry triangle,
            float epsilon,
            out Vector3 point)
        {
            point = Vector3.zero;
            Vector3 direction = end - start;
            Vector3 edgeOne = triangle.Second - triangle.First;
            Vector3 edgeTwo = triangle.Third - triangle.First;
            Vector3 p = Vector3.Cross(direction, edgeTwo);
            float determinant = Vector3.Dot(edgeOne, p);
            float determinantEpsilon = Mathf.Max(
                0.00000001f,
                epsilon * epsilon);
            if (Mathf.Abs(determinant) <= determinantEpsilon)
            {
                return false;
            }
            float inverse = 1f / determinant;
            Vector3 t = start - triangle.First;
            float u = Vector3.Dot(t, p) * inverse;
            float barycentricEpsilon = 0.00001f;
            if (u < -barycentricEpsilon ||
                u > 1f + barycentricEpsilon)
            {
                return false;
            }
            Vector3 q = Vector3.Cross(t, edgeOne);
            float v = Vector3.Dot(direction, q) * inverse;
            if (v < -barycentricEpsilon ||
                u + v > 1f + barycentricEpsilon)
            {
                return false;
            }
            float segmentParameter = Vector3.Dot(edgeTwo, q) * inverse;
            if (segmentParameter < -barycentricEpsilon ||
                segmentParameter > 1f + barycentricEpsilon)
            {
                return false;
            }
            point = start + direction * Mathf.Clamp01(segmentParameter);
            return IsFinite(point);
        }

        private static bool IsChamferDirectedAllowedSharedContact(
            Vector3 point,
            ChamferDirectedTriangleGeometry first,
            ChamferDirectedTriangleGeometry second,
            List<VertexKey> sharedKeys,
            float epsilon)
        {
            float epsilonSquared = epsilon * epsilon;
            for (int i = 0; i < sharedKeys.Count; i++)
            {
                Vector3 sharedPosition = first.GetPosition(sharedKeys[i]);
                if ((point - sharedPosition).sqrMagnitude <=
                    epsilonSquared)
                {
                    return true;
                }
            }
            if (sharedKeys.Count >= 2)
            {
                Vector3 edgeStart = first.GetPosition(sharedKeys[0]);
                Vector3 edgeEnd = first.GetPosition(sharedKeys[1]);
                if (DistanceChamferDirectedPointToSegmentSquared(
                        point,
                        edgeStart,
                        edgeEnd) <= epsilonSquared)
                {
                    return true;
                }
            }
            return false;
        }

        private static float DistanceChamferDirectedPointToSegmentSquared(
            Vector3 point,
            Vector3 start,
            Vector3 end)
        {
            Vector3 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= MinimumEdgeLengthSqr)
            {
                return (point - start).sqrMagnitude;
            }
            float parameter = Mathf.Clamp01(
                Vector3.Dot(point - start, segment) /
                lengthSquared);
            Vector3 closest = start + segment * parameter;
            return (point - closest).sqrMagnitude;
        }

        private static int GetChamferDirectedProjectionDropAxis(
            Vector3 normal)
        {
            float x = Mathf.Abs(normal.x);
            float y = Mathf.Abs(normal.y);
            float z = Mathf.Abs(normal.z);
            if (x >= y && x >= z)
            {
                return 0;
            }
            return y >= z ? 1 : 2;
        }

        private static Vector2 ProjectChamferDirectedPoint(
            Vector3 point,
            int dropAxis)
        {
            if (dropAxis == 0)
            {
                return new Vector2(point.y, point.z);
            }
            if (dropAxis == 1)
            {
                return new Vector2(point.x, point.z);
            }
            return new Vector2(point.x, point.y);
        }

        #endregion
    }
}
