using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear boundary planning

        private static ChamferVertexPatchPlan AuditChamferVertexPatchComponents(
            List<ChamferExpectedVertexBoundary> boundaries,
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            Dictionary<int, float> widthByEdge,
            List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
            Dictionary<TopologyEdgeKey, int> useCounts,
            HashSet<TopologyEdgeKey> expectedVertexBoundaryEdges,
            HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges,
            List<ChamferProvisionalSegmentRecord> segments,
            ref ChamferEmissionStats stats)
        {
            List<string> failures = new List<string>();
            Dictionary<int, ChamferVertexPatchSourceContext> sourceContexts =
                BuildChamferVertexPatchSourceContexts(
                    context,
                    widthByEdge,
                    sourceBoundaryRecords,
                    ref stats,
                    failures);
            Dictionary<int, List<ChamferExpectedVertexBoundary>> byVertex =
                new Dictionary<int, List<ChamferExpectedVertexBoundary>>();
            for (int i = 0; i < boundaries.Count; i++)
            {
                ChamferExpectedVertexBoundary boundary = boundaries[i];
                if (!byVertex.TryGetValue(
                        boundary.SourceVertexIndex,
                        out List<ChamferExpectedVertexBoundary> list))
                {
                    list = new List<ChamferExpectedVertexBoundary>();
                    byVertex.Add(boundary.SourceVertexIndex, list);
                }
                list.Add(boundary);
            }

            List<int> sourceVertices = new List<int>(byVertex.Keys);
            sourceVertices.Sort();
            List<ChamferVertexPatchComponent> components =
                new List<ChamferVertexPatchComponent>();
            for (int vertexListIndex = 0;
                 vertexListIndex < sourceVertices.Count;
                 vertexListIndex++)
            {
                int sourceVertexIndex = sourceVertices[vertexListIndex];
                List<ChamferExpectedVertexBoundary> vertexBoundaries =
                    byVertex[sourceVertexIndex];
                vertexBoundaries.Sort(
                    CompareChamferExpectedVertexBoundaryProvenance);
                if (!sourceContexts.TryGetValue(
                        sourceVertexIndex,
                        out ChamferVertexPatchSourceContext sourceContext))
                {
                    stats.PatchComponentProvenanceFailureCount++;
                    AddChamferVertexPatchFailure(
                        failures,
                        "missing-source-context/vertex:" +
                            sourceVertexIndex);
                    continue;
                }

                List<ChamferVertexPatchComponent> vertexComponents =
                    BuildOrderedChamferVertexPatchComponents(
                        sourceVertexIndex,
                        vertexBoundaries,
                        sourceContext,
                        context,
                        widthByEdge,
                        ref stats,
                        failures);
                components.AddRange(vertexComponents);
            }

            foreach (KeyValuePair<int, ChamferVertexPatchSourceContext> pair
                     in sourceContexts)
            {
            }

            int assignedBoundaryRecords = 0;
            for (int i = 0; i < components.Count; i++)
            {
                assignedBoundaryRecords +=
                    components[i].OrderedBoundaries.Count;
                if (components[i].IsClosed)
                {
                    components[i].Closure =
                        ChamferVertexPatchClosure.ClosedLoop;
                }
                else
                {
                    if (components[i].ProvenanceValid)
                    {
                        ResolveChamferSourceBoundaryPatchClosure(
                            components[i],
                            expectedSourceBoundaryEdges);
                    }
                }
            }
            if (assignedBoundaryRecords != boundaries.Count)
            {
                stats.PatchComponentProvenanceFailureCount++;
                AddChamferVertexPatchFailure(
                    failures,
                    "boundary-assignment-mismatch/expected:" +
                        boundaries.Count +
                    "/assigned:" + assignedBoundaryRecords);
            }

            Dictionary<TopologyEdgeKey, int> plannedClosedSpokeUses =
                new Dictionary<TopologyEdgeKey, int>();
            for (int i = 0; i < components.Count; i++)
            {
                ChamferVertexPatchComponent component = components[i];
                if (component.IsClosed ||
                    !component.ProvenanceValid ||
                    component.SourceFanOpen ||
                    !component.HasEndpointSpokes)
                {
                    continue;
                }
                IncrementChamferTopologyKeyCount(
                    plannedClosedSpokeUses,
                    component.StartSpokeKey);
                IncrementChamferTopologyKeyCount(
                    plannedClosedSpokeUses,
                    component.EndSpokeKey);
            }

            for (int i = 0; i < components.Count; i++)
            {
                ChamferVertexPatchComponent component = components[i];
                if (component.Closure ==
                        ChamferVertexPatchClosure.ClosedLoop ||
                    component.Closure ==
                        ChamferVertexPatchClosure.OpenChainSourceBoundaryResolved)
                {
                    continue;
                }

                if (component.ProvenanceValid &&
                    !component.SourceFanOpen &&
                    TryResolveChamferClosedSourcePatchClosure(
                        component,
                        plannedClosedSpokeUses,
                        useCounts,
                        expectedVertexBoundaryEdges,
                        expectedSourceBoundaryEdges,
                        segments))
                {
                    component.Closure =
                        ChamferVertexPatchClosure.OpenChainClosedSourceResolved;
                }
            }

            ResolveChamferClosedSourcePatchClusters(
                components,
                ref stats,
                failures);

            // Give all still-unclassified open chains an explicit provisional
            // unresolved state so the diagnostic passes can inspect them.
            // Final unresolved counts are recorded only after proven source-
            // boundary completion promotion has had a chance to resolve them.
            for (int i = 0; i < components.Count; i++)
            {
                ChamferVertexPatchComponent component = components[i];
                if (!component.IsClosed &&
                    component.Closure ==
                        ChamferVertexPatchClosure.Unclassified)
                {
                    component.Closure =
                        ChamferVertexPatchClosure.OpenChainUnresolved;
                }
            }


            HashSet<TopologyEdgeKey> finalSourceBoundaryEdges =
                new HashSet<TopologyEdgeKey>(
                    expectedSourceBoundaryEdges);
            HashSet<TopologyEdgeKey> remainingVertexPatchBoundaryEdges =
                new HashSet<TopologyEdgeKey>(
                    expectedVertexBoundaryEdges);
            List<ChamferFinalSourceBoundaryLoop>
                finalSourceBoundaryLoops =
                    new List<ChamferFinalSourceBoundaryLoop>();
            ResolveAndAuditChamferSourceBoundaryCompletions(
                components,
                sourceBoundaryRecords,
                useCounts,
                expectedVertexBoundaryEdges,
                expectedSourceBoundaryEdges,
                finalSourceBoundaryEdges,
                remainingVertexPatchBoundaryEdges,
                finalSourceBoundaryLoops,
                ref stats);

            for (int i = 0; i < components.Count; i++)
            {
                ChamferVertexPatchComponent component = components[i];
                if (component.Closure ==
                    ChamferVertexPatchClosure.ClosedLoop)
                {
                    continue;
                }
                if (component.Closure ==
                        ChamferVertexPatchClosure.OpenChainSourceBoundaryResolved ||
                    component.Closure ==
                        ChamferVertexPatchClosure.OpenChainSourceBoundaryCompletionResolved ||
                    component.Closure ==
                        ChamferVertexPatchClosure.OpenChainSourceBoundaryMultiCycleResolved ||
                    component.Closure ==
                        ChamferVertexPatchClosure.OpenChainSourceBoundaryResidualPatchResolved)
                {
                    continue;
                }
                if (component.Closure ==
                        ChamferVertexPatchClosure.OpenChainClosedSourceResolved ||
                    component.Closure ==
                        ChamferVertexPatchClosure.OpenChainClosedSourceClusterResolved)
                {
                    continue;
                }

                component.Closure =
                    ChamferVertexPatchClosure.OpenChainUnresolved;
                stats.PatchUnresolvedOpenChainCount++;
                AddChamferVertexPatchFailure(
                    failures,
                    BuildChamferVertexPatchComponentDiagnostic(component));
            }

            bool ready =
                stats.PatchComponentOrderingFailureCount == 0 &&
                stats.PatchComponentProvenanceFailureCount == 0 &&
                stats.PatchUnresolvedOpenChainCount == 0 &&
                assignedBoundaryRecords == boundaries.Count;
            if (!ready)
            {
                return new ChamferVertexPatchPlan(
                    new List<ChamferVertexPatchLoop>(),
                    finalSourceBoundaryEdges,
                    remainingVertexPatchBoundaryEdges,
                    false,
                    "component-audit-incomplete");
            }

            if (!TryBuildChamferVertexPatchPlan(
                    components,
                    sourceFaces,
                    context,
                    useCounts,
                    finalSourceBoundaryEdges,
                    remainingVertexPatchBoundaryEdges,
                    ref stats,
                    out ChamferVertexPatchPlan patchPlan,
                    out string patchPlanFailure))
            {
                return patchPlan ?? new ChamferVertexPatchPlan(
                    new List<ChamferVertexPatchLoop>(),
                    finalSourceBoundaryEdges,
                    remainingVertexPatchBoundaryEdges,
                    false,
                    patchPlanFailure);
            }
            return patchPlan;
        }

        private static bool TryBuildChamferVertexPatchPlan(
            List<ChamferVertexPatchComponent> components,
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            Dictionary<TopologyEdgeKey, int> existingUseCounts,
            HashSet<TopologyEdgeKey> finalSourceBoundaryEdges,
            HashSet<TopologyEdgeKey> remainingVertexPatchBoundaryEdges,
            ref ChamferEmissionStats stats,
            out ChamferVertexPatchPlan plan,
            out string failure)
        {
            failure = string.Empty;
            List<ChamferVertexPatchLoop> loops =
                new List<ChamferVertexPatchLoop>();
            HashSet<ChamferVertexPatchCluster> emittedClusters =
                new HashSet<ChamferVertexPatchCluster>();

            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                ChamferVertexPatchComponent component =
                    components[componentIndex];
                ChamferVertexPatchLoopKind kind;
                List<Vector3> positions;
                List<TopologyEdgeKey> boundaryKeys;
                List<ChamferVertexPatchComponent> loopComponents;

                if (component.Closure ==
                    ChamferVertexPatchClosure.ClosedLoop)
                {
                    kind = ChamferVertexPatchLoopKind.LocalClosedComponent;
                    positions = new List<Vector3>(component.OrderedPositions);
                    boundaryKeys =
                        BuildChamferComponentBoundaryKeyList(component);
                    loopComponents =
                        new List<ChamferVertexPatchComponent> { component };
                }
                else if (component.Closure ==
                    ChamferVertexPatchClosure.OpenChainClosedSourceResolved)
                {
                    kind = ChamferVertexPatchLoopKind.ClosedSourceSpokeLoop;
                    positions = new List<Vector3>(component.OrderedPositions);
                    positions.Add(component.SourceContext.SourcePosition);
                    boundaryKeys =
                        BuildChamferComponentBoundaryKeyList(component);
                    boundaryKeys.Add(component.EndSpokeKey);
                    boundaryKeys.Add(component.StartSpokeKey);
                    loopComponents =
                        new List<ChamferVertexPatchComponent> { component };
                }
                else if (component.Closure ==
                    ChamferVertexPatchClosure.OpenChainClosedSourceClusterResolved)
                {
                    if (component.Cluster == null ||
                        !emittedClusters.Add(component.Cluster))
                    {
                        continue;
                    }
                    kind = ChamferVertexPatchLoopKind.ClosedSourceCluster;
                    positions = new List<Vector3>(
                        component.Cluster.OrderedPositions);
                    boundaryKeys =
                        BuildChamferClusterBoundaryKeyList(component.Cluster);
                    loopComponents = new List<ChamferVertexPatchComponent>(
                        component.Cluster.Components);
                }
                else if (component.Closure ==
                    ChamferVertexPatchClosure.OpenChainSourceBoundaryResidualPatchResolved)
                {
                    if (component.Cluster == null ||
                        !emittedClusters.Add(component.Cluster))
                    {
                        continue;
                    }
                    kind =
                        ChamferVertexPatchLoopKind.ResidualSourceBoundaryCycle;
                    positions = new List<Vector3>(
                        component.Cluster.OrderedPositions);
                    boundaryKeys =
                        BuildChamferClusterBoundaryKeyList(component.Cluster);
                    loopComponents = new List<ChamferVertexPatchComponent>(
                        component.Cluster.Components);
                }
                else
                {
                    continue;
                }

                stats.PatchLoopsAttempted++;
                if (!TryCreateChamferVertexPatchLoop(
                        loops.Count,
                        kind,
                        positions,
                        boundaryKeys,
                        loopComponents,
                        sourceFaces,
                        context,
                        out ChamferVertexPatchLoop loop,
                        out failure))
                {
                    stats.PatchLoopConstructionFailureCount++;
                    plan = new ChamferVertexPatchPlan(
                        loops,
                        finalSourceBoundaryEdges,
                        remainingVertexPatchBoundaryEdges,
                        false,
                        failure);
                    return false;
                }
                loops.Add(loop);
            }

            Dictionary<TopologyEdgeKey, int> plannedBoundaryUses =
                new Dictionary<TopologyEdgeKey, int>();
            HashSet<TopologyEdgeKey> patchBoundaryKeys =
                new HashSet<TopologyEdgeKey>();
            for (int loopIndex = 0; loopIndex < loops.Count; loopIndex++)
            {
                ChamferVertexPatchLoop loop = loops[loopIndex];
                for (int keyIndex = 0;
                     keyIndex < loop.OrderedBoundaryKeys.Count;
                     keyIndex++)
                {
                    TopologyEdgeKey key = loop.OrderedBoundaryKeys[keyIndex];
                    patchBoundaryKeys.Add(key);
                    IncrementChamferTopologyKeyCount(
                        plannedBoundaryUses,
                        key);
                }
            }

            foreach (TopologyEdgeKey key in finalSourceBoundaryEdges)
            {
                if (patchBoundaryKeys.Contains(key))
                {
                    failure = "patch-plan-claims-final-source-boundary";
                    stats.PatchLoopConstructionFailureCount++;
                    plan = new ChamferVertexPatchPlan(
                        loops,
                        finalSourceBoundaryEdges,
                        remainingVertexPatchBoundaryEdges,
                        false,
                        failure);
                    return false;
                }
            }
            foreach (TopologyEdgeKey key in remainingVertexPatchBoundaryEdges)
            {
                int planned = plannedBoundaryUses.TryGetValue(
                    key,
                    out int plannedCount)
                    ? plannedCount
                    : 0;
                if (planned != 1)
                {
                    failure = "remaining-patch-boundary-claim-count:" + planned;
                    stats.PatchLoopConstructionFailureCount++;
                    plan = new ChamferVertexPatchPlan(
                        loops,
                        finalSourceBoundaryEdges,
                        remainingVertexPatchBoundaryEdges,
                        false,
                        failure);
                    return false;
                }
            }
            foreach (KeyValuePair<TopologyEdgeKey, int> pair
                     in plannedBoundaryUses)
            {
                int existing = existingUseCounts.TryGetValue(
                    pair.Key,
                    out int existingCount)
                    ? existingCount
                    : 0;
                if (existing + pair.Value != 2)
                {
                    failure = "patch-plan-boundary-incidence/existing:" +
                        existing + "/planned:" + pair.Value;
                    stats.PatchLoopConstructionFailureCount++;
                    plan = new ChamferVertexPatchPlan(
                        loops,
                        finalSourceBoundaryEdges,
                        remainingVertexPatchBoundaryEdges,
                        false,
                        failure);
                    return false;
                }
            }

            plan = new ChamferVertexPatchPlan(
                loops,
                finalSourceBoundaryEdges,
                remainingVertexPatchBoundaryEdges,
                true,
                string.Empty);
            return true;
        }

        private static List<TopologyEdgeKey>
            BuildChamferComponentBoundaryKeyList(
                ChamferVertexPatchComponent component)
        {
            List<TopologyEdgeKey> keys =
                new List<TopologyEdgeKey>(
                    component.OrderedBoundaries.Count);
            for (int i = 0; i < component.OrderedBoundaries.Count; i++)
            {
                keys.Add(component.OrderedBoundaries[i].Boundary.Key);
            }
            return keys;
        }

        private static List<TopologyEdgeKey>
            BuildChamferClusterBoundaryKeyList(
                ChamferVertexPatchCluster cluster)
        {
            if (cluster.OrderedCompletionEdges.Count > 0)
            {
                List<TopologyEdgeKey> completionKeys =
                    new List<TopologyEdgeKey>(
                        cluster.OrderedCompletionEdges.Count);
                for (int i = 0;
                     i < cluster.OrderedCompletionEdges.Count;
                     i++)
                {
                    completionKeys.Add(
                        cluster.OrderedCompletionEdges[i].Key);
                }
                return completionKeys;
            }

            List<TopologyEdgeKey> keys =
                new List<TopologyEdgeKey>(
                    cluster.OrderedBoundaries.Count);
            for (int i = 0; i < cluster.OrderedBoundaries.Count; i++)
            {
                keys.Add(cluster.OrderedBoundaries[i].Boundary.Key);
            }
            return keys;
        }

        private static bool TryCreateChamferVertexPatchLoop(
            int loopIndex,
            ChamferVertexPatchLoopKind kind,
            List<Vector3> positions,
            List<TopologyEdgeKey> boundaryKeys,
            List<ChamferVertexPatchComponent> components,
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            out ChamferVertexPatchLoop loop,
            out string failure)
        {
            loop = null;
            failure = string.Empty;
            if (positions == null || boundaryKeys == null ||
                positions.Count < 3 ||
                positions.Count != boundaryKeys.Count)
            {
                failure = "invalid-patch-loop-counts/positions:" +
                    (positions != null ? positions.Count : -1) +
                    "/keys:" +
                    (boundaryKeys != null ? boundaryKeys.Count : -1);
                return false;
            }

            HashSet<VertexKey> uniqueVertices =
                new HashSet<VertexKey>();
            HashSet<TopologyEdgeKey> uniqueEdges =
                new HashSet<TopologyEdgeKey>();
            for (int i = 0; i < positions.Count; i++)
            {
                if (!IsFinite(positions[i]) ||
                    !uniqueVertices.Add(new VertexKey(positions[i])))
                {
                    failure = "non-finite-or-duplicate-patch-loop-vertex";
                    return false;
                }
                TopologyEdgeKey expectedKey = new TopologyEdgeKey(
                    new VertexKey(positions[i]),
                    new VertexKey(positions[(i + 1) % positions.Count]));
                if (!expectedKey.Equals(boundaryKeys[i]) ||
                    !uniqueEdges.Add(boundaryKeys[i]))
                {
                    failure = "patch-loop-edge-order-or-duplicate";
                    return false;
                }
            }

            HashSet<int> graphFaces = new HashSet<int>();
            HashSet<int> sourceEdges = new HashSet<int>();
            Vector3 expectedNormal = Vector3.zero;
            float featureStrength = 0f;
            Dictionary<int, float> strengthBySourceEdge =
                BuildChamferSelectedStrengthBySourceEdge(context);
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                ChamferVertexPatchComponent component =
                    components[componentIndex];
                for (int boundaryIndex = 0;
                     boundaryIndex < component.OrderedBoundaries.Count;
                     boundaryIndex++)
                {
                    ChamferExpectedVertexBoundary boundary =
                        component.OrderedBoundaries[boundaryIndex].Boundary;
                    if (graphFaces.Add(boundary.FaceIndex) &&
                        boundary.FaceIndex >= 0 &&
                        boundary.FaceIndex < context.Graph.Faces.Count)
                    {
                        int sourceFaceIndex =
                            context.Graph.Faces[boundary.FaceIndex]
                                .SourceFaceIndex;
                        if (sourceFaceIndex >= 0 &&
                            sourceFaceIndex < sourceFaces.Count)
                        {
                            expectedNormal +=
                                sourceFaces[sourceFaceIndex].Normal;
                        }
                    }
                    if (sourceEdges.Add(boundary.SourceEdgeIndex) &&
                        strengthBySourceEdge.TryGetValue(
                            boundary.SourceEdgeIndex,
                            out float strength))
                    {
                        featureStrength = Mathf.Max(
                            featureStrength,
                            strength);
                    }
                }
            }

            if (!IsFinite(expectedNormal) ||
                expectedNormal.sqrMagnitude <= 0.00000001f)
            {
                expectedNormal = CalculatePolygonNormal(positions);
            }
            if (!IsFinite(expectedNormal) ||
                expectedNormal.sqrMagnitude <= 0.00000001f ||
                featureStrength <= 0f)
            {
                failure = "invalid-patch-loop-normal-or-strength";
                return false;
            }

            List<Vector3> componentExpectedNormals =
                new List<Vector3>(components.Count);
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                if (!TryCalculateChamferPatchComponentExpectedNormal(
                        components[componentIndex],
                        sourceFaces,
                        context,
                        out Vector3 componentExpectedNormal))
                {
                    componentExpectedNormal = expectedNormal.normalized;
                }
                componentExpectedNormals.Add(componentExpectedNormal);
            }

            loop = new ChamferVertexPatchLoop(
                loopIndex,
                kind,
                positions,
                boundaryKeys,
                components,
                componentExpectedNormals,
                expectedNormal.normalized,
                featureStrength);
            return true;
        }

        private static bool
            TryCalculateChamferPatchComponentExpectedNormal(
                ChamferVertexPatchComponent component,
                List<PolygonFace> sourceFaces,
                ChamferTopologyContext context,
                out Vector3 expectedNormal)
        {
            expectedNormal = Vector3.zero;
            HashSet<int> graphFaces = new HashSet<int>();
            for (int boundaryIndex = 0;
                 boundaryIndex < component.OrderedBoundaries.Count;
                 boundaryIndex++)
            {
                ChamferExpectedVertexBoundary boundary =
                    component.OrderedBoundaries[boundaryIndex].Boundary;
                if (!graphFaces.Add(boundary.FaceIndex) ||
                    boundary.FaceIndex < 0 ||
                    boundary.FaceIndex >= context.Graph.Faces.Count)
                {
                    continue;
                }
                int sourceFaceIndex =
                    context.Graph.Faces[boundary.FaceIndex]
                        .SourceFaceIndex;
                if (sourceFaceIndex >= 0 &&
                    sourceFaceIndex < sourceFaces.Count)
                {
                    expectedNormal += sourceFaces[sourceFaceIndex].Normal;
                }
            }
            if (!IsFinite(expectedNormal) ||
                expectedNormal.sqrMagnitude <= 0.00000001f)
            {
                List<Vector3> localPositions =
                    new List<Vector3>(component.OrderedPositions);
                if (!component.IsClosed)
                {
                    localPositions.Add(
                        component.SourceContext.SourcePosition);
                }
                if (!TryCalculateChamferPatchOrderedNormal(
                        localPositions,
                        out expectedNormal))
                {
                    expectedNormal = Vector3.zero;
                    return false;
                }
            }
            expectedNormal.Normalize();
            return IsFinite(expectedNormal) &&
                expectedNormal.sqrMagnitude > 0.00000001f;
        }

        private static Dictionary<int, float>
            BuildChamferSelectedStrengthBySourceEdge(
                ChamferTopologyContext context)
        {
            Dictionary<int, float> strengths =
                new Dictionary<int, float>();
            for (int i = 0; i < context.SelectedEdges.Count; i++)
            {
                EdgeWearSelectedGraphEdge selected = context.SelectedEdges[i];
                strengths[selected.GraphEdgeIndex] =
                    selected.Candidate.Strength;
            }
            return strengths;
        }

        private static Dictionary<int, ChamferVertexPatchSourceContext>
            BuildChamferVertexPatchSourceContexts(
                ChamferTopologyContext context,
                Dictionary<int, float> widthByEdge,
                List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
                ref ChamferEmissionStats stats,
                List<string> failures)
        {
            Dictionary<int, List<ChamferSourceBoundaryRecord>>
                sourceBoundaryRecordsByVertex =
                    new Dictionary<int,
                        List<ChamferSourceBoundaryRecord>>();
            for (int i = 0; i < sourceBoundaryRecords.Count; i++)
            {
                ChamferSourceBoundaryRecord record = sourceBoundaryRecords[i];
                AddChamferSourceBoundaryRecordAtVertex(
                    sourceBoundaryRecordsByVertex,
                    record.SourceVertexStart,
                    record);
                if (record.SourceVertexEnd != record.SourceVertexStart)
                {
                    AddChamferSourceBoundaryRecordAtVertex(
                        sourceBoundaryRecordsByVertex,
                        record.SourceVertexEnd,
                        record);
                }
            }

            List<List<int>> outgoingByVertex =
                new List<List<int>>(context.Graph.Vertices.Count);
            for (int i = 0; i < context.Graph.Vertices.Count; i++)
            {
                outgoingByVertex.Add(new List<int>());
            }
            for (int i = 0; i < context.HalfEdges.Count; i++)
            {
                outgoingByVertex[context.HalfEdges[i].OriginVertex].Add(i);
            }

            Dictionary<int, ChamferVertexPatchSourceContext> contexts =
                new Dictionary<int, ChamferVertexPatchSourceContext>();
            for (int vertexIndex = 0;
                 vertexIndex < context.Graph.Vertices.Count;
                 vertexIndex++)
            {
                List<int> activeEdges = new List<int>();
                List<int> incidentEdges =
                    context.Graph.Vertices[vertexIndex].EdgeIndices;
                bool sourceFanOpen = false;
                for (int i = 0; i < incidentEdges.Count; i++)
                {
                    int edgeIndex = incidentEdges[i];
                    EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                    if (edge.FaceA < 0 || edge.FaceB < 0)
                    {
                        sourceFanOpen = true;
                    }
                    if (IsChamferSourceEdgeActive(widthByEdge, edgeIndex))
                    {
                        activeEdges.Add(edgeIndex);
                    }
                }
                activeEdges.Sort();

                int activeRunCount = 0;
                bool sourceContextValid = true;
                if (!TryCountActiveChamferRunsAtVertex(
                        context,
                        outgoingByVertex[vertexIndex],
                        widthByEdge,
                        out int orderedRunCount,
                        out bool orderedFanOpen))
                {
                    sourceContextValid = false;
                    stats.PatchComponentProvenanceFailureCount++;
                    AddChamferVertexPatchFailure(
                        failures,
                        "source-fan-ordering/vertex:" + vertexIndex);
                }
                else
                {
                    activeRunCount = orderedRunCount;
                    if (orderedFanOpen != sourceFanOpen)
                    {
                        sourceContextValid = false;
                        stats.PatchComponentProvenanceFailureCount++;
                        AddChamferVertexPatchFailure(
                            failures,
                            "source-fan-state-mismatch/vertex:" +
                                vertexIndex +
                            "/graphOpen:" +
                                (sourceFanOpen ? 1 : 0) +
                            "/orderedOpen:" +
                                (orderedFanOpen ? 1 : 0));
                    }
                }

                sourceBoundaryRecordsByVertex.TryGetValue(
                    vertexIndex,
                    out List<ChamferSourceBoundaryRecord> incidentRecords);
                contexts.Add(
                    vertexIndex,
                    new ChamferVertexPatchSourceContext(
                        vertexIndex,
                        context.Graph.Vertices[vertexIndex].Position,
                        sourceFanOpen,
                        sourceContextValid,
                        activeRunCount,
                        activeEdges,
                        incidentRecords != null
                            ? new List<ChamferSourceBoundaryRecord>(
                                incidentRecords)
                            : new List<ChamferSourceBoundaryRecord>()));
            }
            return contexts;
        }

        private static void AddChamferSourceBoundaryRecordAtVertex(
            Dictionary<int, List<ChamferSourceBoundaryRecord>> recordsByVertex,
            int sourceVertexIndex,
            ChamferSourceBoundaryRecord record)
        {
            if (!recordsByVertex.TryGetValue(
                    sourceVertexIndex,
                    out List<ChamferSourceBoundaryRecord> records))
            {
                records = new List<ChamferSourceBoundaryRecord>();
                recordsByVertex.Add(sourceVertexIndex, records);
            }
            records.Add(record);
        }

        private static bool TryCountActiveChamferRunsAtVertex(
            ChamferTopologyContext context,
            List<int> outgoing,
            Dictionary<int, float> widthByEdge,
            out int activeRunCount,
            out bool openFan)
        {
            activeRunCount = 0;
            openFan = false;
            if (outgoing.Count == 0)
            {
                return true;
            }

            int start = -1;
            for (int i = 0; i < outgoing.Count; i++)
            {
                ChamferHalfEdge candidate = context.HalfEdges[outgoing[i]];
                int previousOpposite =
                    context.HalfEdges[candidate.Previous].Opposite;
                if (previousOpposite < 0)
                {
                    start = candidate.Index;
                    openFan = true;
                    break;
                }
            }
            if (start < 0)
            {
                start = outgoing[0];
            }

            List<int> ordered = new List<int>(outgoing.Count);
            HashSet<int> visited = new HashSet<int>();
            int current = start;
            int guard = 0;
            while (current >= 0 && guard++ <= outgoing.Count)
            {
                if (!visited.Add(current))
                {
                    break;
                }
                ordered.Add(current);
                ChamferHalfEdge halfEdge = context.HalfEdges[current];
                int next = halfEdge.Opposite >= 0
                    ? context.HalfEdges[halfEdge.Opposite].Next
                    : -1;
                if (next < 0 || next == start)
                {
                    break;
                }
                current = next;
            }
            if (ordered.Count != outgoing.Count)
            {
                return false;
            }

            bool anyActive = false;
            bool allActive = ordered.Count > 0;
            List<bool> active = new List<bool>(ordered.Count);
            for (int i = 0; i < ordered.Count; i++)
            {
                bool isActive = IsChamferSourceEdgeActive(
                    widthByEdge,
                    context.HalfEdges[ordered[i]].SourceEdgeIndex);
                active.Add(isActive);
                anyActive |= isActive;
                allActive &= isActive;
            }
            if (!anyActive)
            {
                return true;
            }
            if (!openFan && allActive)
            {
                activeRunCount = 1;
                return true;
            }

            for (int i = 0; i < active.Count; i++)
            {
                if (!active[i])
                {
                    continue;
                }
                bool previousActive = i > 0
                    ? active[i - 1]
                    : !openFan && active[active.Count - 1];
                if (!previousActive)
                {
                    activeRunCount++;
                }
            }
            return true;
        }

        private static bool IsChamferSourceEdgeActive(
            Dictionary<int, float> widthByEdge,
            int sourceEdgeIndex)
        {
            return widthByEdge.TryGetValue(sourceEdgeIndex, out float width) &&
                width > PointMergeDistance;
        }

        private static List<ChamferVertexPatchComponent>
            BuildOrderedChamferVertexPatchComponents(
                int sourceVertexIndex,
                List<ChamferExpectedVertexBoundary> boundaries,
                ChamferVertexPatchSourceContext sourceContext,
                ChamferTopologyContext context,
                Dictionary<int, float> widthByEdge,
                ref ChamferEmissionStats stats,
                List<string> failures)
        {
            List<ChamferExpectedVertexBoundary> uniqueBoundaries =
                new List<ChamferExpectedVertexBoundary>(boundaries.Count);
            HashSet<TopologyEdgeKey> uniqueKeys =
                new HashSet<TopologyEdgeKey>();
            for (int i = 0; i < boundaries.Count; i++)
            {
                if (!uniqueKeys.Add(boundaries[i].Key))
                {
                    stats.PatchComponentOrderingFailureCount++;
                    AddChamferVertexPatchFailure(
                        failures,
                        "duplicate-component-key/vertex:" +
                            sourceVertexIndex +
                        "/edge:" + boundaries[i].SourceEdgeIndex +
                        "/face:" + boundaries[i].FaceIndex);
                    continue;
                }
                uniqueBoundaries.Add(boundaries[i]);
            }

            Dictionary<VertexKey, List<int>> adjacency =
                new Dictionary<VertexKey, List<int>>();
            for (int i = 0; i < uniqueBoundaries.Count; i++)
            {
                AddBoundaryAdjacency(
                    adjacency,
                    uniqueBoundaries[i].Key.First,
                    i);
                AddBoundaryAdjacency(
                    adjacency,
                    uniqueBoundaries[i].Key.Second,
                    i);
            }
            foreach (KeyValuePair<VertexKey, List<int>> pair in adjacency)
            {
                pair.Value.Sort((left, right) =>
                    CompareChamferExpectedVertexBoundaryProvenance(
                        uniqueBoundaries[left],
                        uniqueBoundaries[right]));
                if (pair.Value.Count > 2)
                {
                    stats.PatchComponentOrderingFailureCount++;
                    AddChamferVertexPatchFailure(
                        failures,
                        "component-branch/vertex:" +
                            sourceVertexIndex +
                        "/degree:" + pair.Value.Count);
                }
            }

            List<ChamferVertexPatchComponent> components =
                new List<ChamferVertexPatchComponent>();
            HashSet<int> visited = new HashSet<int>();
            for (int edgeIndex = 0;
                 edgeIndex < uniqueBoundaries.Count;
                 edgeIndex++)
            {
                if (visited.Contains(edgeIndex))
                {
                    continue;
                }

                List<int> componentEdges = new List<int>();
                Queue<int> queue = new Queue<int>();
                visited.Add(edgeIndex);
                queue.Enqueue(edgeIndex);
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    componentEdges.Add(current);
                    TopologyEdgeKey key = uniqueBoundaries[current].Key;
                    EnqueueAdjacentBoundaryEdges(
                        key.First,
                        adjacency,
                        visited,
                        queue);
                    EnqueueAdjacentBoundaryEdges(
                        key.Second,
                        adjacency,
                        visited,
                        queue);
                }
                componentEdges.Sort((left, right) =>
                    CompareChamferExpectedVertexBoundaryProvenance(
                        uniqueBoundaries[left],
                        uniqueBoundaries[right]));

                if (!TryOrderChamferVertexPatchComponent(
                        sourceVertexIndex,
                        uniqueBoundaries,
                        componentEdges,
                        adjacency,
                        sourceContext,
                        out ChamferVertexPatchComponent component))
                {
                    stats.PatchComponentOrderingFailureCount++;
                    AddChamferVertexPatchFailure(
                        failures,
                        "component-ordering/vertex:" +
                            sourceVertexIndex +
                        "/edges:" + componentEdges.Count);
                    continue;
                }
                component.ProvenanceValid =
                    ValidateChamferVertexPatchComponentProvenance(
                        component,
                        context,
                        widthByEdge);
                if (!component.ProvenanceValid)
                {
                    stats.PatchComponentProvenanceFailureCount++;
                    AddChamferVertexPatchFailure(
                        failures,
                        "component-provenance/vertex:" +
                            sourceVertexIndex +
                        "/edges:" +
                            component.OrderedBoundaries.Count);
                }
                components.Add(component);
            }
            return components;
        }

        private static bool TryOrderChamferVertexPatchComponent(
            int sourceVertexIndex,
            List<ChamferExpectedVertexBoundary> boundaries,
            List<int> componentEdges,
            Dictionary<VertexKey, List<int>> adjacency,
            ChamferVertexPatchSourceContext sourceContext,
            out ChamferVertexPatchComponent component)
        {
            component = null;
            if (componentEdges.Count == 0)
            {
                return false;
            }

            HashSet<int> componentEdgeSet =
                new HashSet<int>(componentEdges);
            HashSet<VertexKey> componentVertices =
                new HashSet<VertexKey>();
            for (int i = 0; i < componentEdges.Count; i++)
            {
                TopologyEdgeKey key = boundaries[componentEdges[i]].Key;
                componentVertices.Add(key.First);
                componentVertices.Add(key.Second);
            }

            List<VertexKey> degreeOne = new List<VertexKey>();
            foreach (VertexKey vertex in componentVertices)
            {
                int degree = CountChamferComponentAdjacency(
                    adjacency,
                    componentEdgeSet,
                    vertex);
                if (degree == 1)
                {
                    degreeOne.Add(vertex);
                }
                else if (degree != 2)
                {
                    return false;
                }
            }
            bool closed = degreeOne.Count == 0;
            if (!closed && degreeOne.Count != 2)
            {
                return false;
            }

            VertexKey startKey;
            if (closed)
            {
                startKey = GetMinimumChamferVertexKey(componentVertices);
            }
            else
            {
                degreeOne.Sort((left, right) => left.CompareTo(right));
                startKey = degreeOne[0];
            }

            int firstEdge = -1;
            List<int> startAdjacent = adjacency[startKey];
            for (int i = 0; i < startAdjacent.Count; i++)
            {
                int candidate = startAdjacent[i];
                if (!componentEdgeSet.Contains(candidate))
                {
                    continue;
                }
                if (firstEdge < 0 ||
                    CompareChamferExpectedVertexBoundaryProvenance(
                        boundaries[candidate],
                        boundaries[firstEdge]) < 0)
                {
                    firstEdge = candidate;
                }
            }
            if (firstEdge < 0)
            {
                return false;
            }

            List<ChamferOrientedVertexBoundary> ordered =
                new List<ChamferOrientedVertexBoundary>(
                    componentEdges.Count);
            List<Vector3> positions =
                new List<Vector3>(componentEdges.Count + 1);
            HashSet<int> used = new HashSet<int>();
            VertexKey currentKey = startKey;
            int currentEdge = firstEdge;
            int guard = 0;
            while (guard++ <= componentEdges.Count)
            {
                if (!componentEdgeSet.Contains(currentEdge) ||
                    !used.Add(currentEdge))
                {
                    return false;
                }
                ChamferExpectedVertexBoundary boundary =
                    boundaries[currentEdge];
                if (!TryOrientChamferVertexBoundary(
                        boundary,
                        currentKey,
                        out ChamferOrientedVertexBoundary oriented))
                {
                    return false;
                }
                ordered.Add(oriented);
                if (positions.Count == 0)
                {
                    positions.Add(oriented.Start);
                }
                currentKey = oriented.EndKey;
                if (!(closed &&
                      used.Count == componentEdges.Count &&
                      currentKey.Equals(startKey)))
                {
                    positions.Add(oriented.End);
                }

                if (used.Count == componentEdges.Count)
                {
                    break;
                }

                int nextEdge = -1;
                List<int> adjacent = adjacency[currentKey];
                for (int i = 0; i < adjacent.Count; i++)
                {
                    int candidate = adjacent[i];
                    if (!componentEdgeSet.Contains(candidate) ||
                        used.Contains(candidate))
                    {
                        continue;
                    }
                    if (nextEdge >= 0)
                    {
                        return false;
                    }
                    nextEdge = candidate;
                }
                if (nextEdge < 0)
                {
                    return false;
                }
                currentEdge = nextEdge;
            }

            if (used.Count != componentEdges.Count)
            {
                return false;
            }
            if (closed && !currentKey.Equals(startKey))
            {
                return false;
            }
            if (!closed &&
                !currentKey.Equals(degreeOne[1]))
            {
                return false;
            }

            component = new ChamferVertexPatchComponent(
                sourceVertexIndex,
                ordered,
                positions,
                closed,
                sourceContext);
            return true;
        }

        private static int CountChamferComponentAdjacency(
            Dictionary<VertexKey, List<int>> adjacency,
            HashSet<int> componentEdges,
            VertexKey vertex)
        {
            int count = 0;
            List<int> adjacent = adjacency[vertex];
            for (int i = 0; i < adjacent.Count; i++)
            {
                if (componentEdges.Contains(adjacent[i]))
                {
                    count++;
                }
            }
            return count;
        }

        private static VertexKey GetMinimumChamferVertexKey(
            HashSet<VertexKey> vertices)
        {
            bool hasValue = false;
            VertexKey minimum = default;
            foreach (VertexKey vertex in vertices)
            {
                if (!hasValue || vertex.CompareTo(minimum) < 0)
                {
                    minimum = vertex;
                    hasValue = true;
                }
            }
            return minimum;
        }

        private static bool TryOrientChamferVertexBoundary(
            ChamferExpectedVertexBoundary boundary,
            VertexKey startKey,
            out ChamferOrientedVertexBoundary oriented)
        {
            VertexKey boundaryStart = new VertexKey(boundary.Start);
            VertexKey boundaryEnd = new VertexKey(boundary.End);
            if (startKey.Equals(boundaryStart))
            {
                oriented = new ChamferOrientedVertexBoundary(
                    boundary,
                    boundary.Start,
                    boundary.End,
                    boundaryStart,
                    boundaryEnd);
                return true;
            }
            if (startKey.Equals(boundaryEnd))
            {
                oriented = new ChamferOrientedVertexBoundary(
                    boundary,
                    boundary.End,
                    boundary.Start,
                    boundaryEnd,
                    boundaryStart);
                return true;
            }
            oriented = default;
            return false;
        }

        private static bool ValidateChamferVertexPatchComponentProvenance(
            ChamferVertexPatchComponent component,
            ChamferTopologyContext context,
            Dictionary<int, float> widthByEdge)
        {
            int sourceVertexIndex = component.SourceVertexIndex;
            if (sourceVertexIndex < 0 ||
                sourceVertexIndex >= context.Graph.Vertices.Count ||
                component.OrderedBoundaries.Count == 0 ||
                component.OrderedPositions.Count == 0 ||
                !component.SourceContext.IsValid ||
                component.SourceContext.ActiveSourceEdges.Count == 0)
            {
                return false;
            }
            if (component.IsClosed &&
                component.OrderedPositions.Count < 3)
            {
                return false;
            }
            if (!component.IsClosed &&
                component.OrderedPositions.Count !=
                    component.OrderedBoundaries.Count + 1)
            {
                return false;
            }

            for (int i = 0;
                 i < component.OrderedBoundaries.Count;
                 i++)
            {
                ChamferExpectedVertexBoundary boundary =
                    component.OrderedBoundaries[i].Boundary;
                if (boundary.SourceVertexIndex != sourceVertexIndex ||
                    boundary.SourceEdgeIndex < 0 ||
                    boundary.SourceEdgeIndex >= context.Graph.Edges.Count ||
                    boundary.FaceIndex < 0 ||
                    boundary.FaceIndex >= context.Graph.Faces.Count)
                {
                    return false;
                }
                EdgeWearGraphEdge edge =
                    context.Graph.Edges[boundary.SourceEdgeIndex];
                if (edge.VertexA != sourceVertexIndex &&
                    edge.VertexB != sourceVertexIndex)
                {
                    return false;
                }
                EdgeWearGraphFace face =
                    context.Graph.Faces[boundary.FaceIndex];
                if (!face.VertexIndices.Contains(sourceVertexIndex) ||
                    !face.EdgeIndices.Contains(boundary.SourceEdgeIndex))
                {
                    return false;
                }
                bool active = IsChamferSourceEdgeActive(
                    widthByEdge,
                    boundary.SourceEdgeIndex);
                if (boundary.Kind ==
                        ChamferVertexBoundaryKind.BevelStripEndpoint &&
                    !active)
                {
                    return false;
                }
                if (boundary.Kind ==
                        ChamferVertexBoundaryKind.UnselectedEdgeTail &&
                    active)
                {
                    return false;
                }
                if (!boundary.Key.Equals(
                        new TopologyEdgeKey(
                            component.OrderedBoundaries[i].StartKey,
                            component.OrderedBoundaries[i].EndKey)))
                {
                    return false;
                }
            }
            return true;
        }

        private static void ResolveChamferSourceBoundaryPatchClosure(
            ChamferVertexPatchComponent component,
            HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges)
        {
            if (component.IsClosed ||
                !component.HasEndpointSpokes ||
                !component.SourceFanOpen)
            {
                return;
            }

            int startOwnerCount =
                CountChamferSourceBoundaryEndpointOwners(
                    component.SourceVertexIndex,
                    component.StartEndpointKey,
                    component.SourceContext.SourceBoundaryRecords,
                    expectedSourceBoundaryEdges,
                    out ChamferSourceBoundaryEndpointOwnership startOwner);
            int endOwnerCount =
                CountChamferSourceBoundaryEndpointOwners(
                    component.SourceVertexIndex,
                    component.EndEndpointKey,
                    component.SourceContext.SourceBoundaryRecords,
                    expectedSourceBoundaryEdges,
                    out ChamferSourceBoundaryEndpointOwnership endOwner);
            component.StartSourceBoundaryOwnerCount = startOwnerCount;
            component.EndSourceBoundaryOwnerCount = endOwnerCount;
            if (startOwnerCount == 1 &&
                endOwnerCount == 1 &&
                (!object.ReferenceEquals(startOwner.Record, endOwner.Record) ||
                 startOwner.ChildIndex != endOwner.ChildIndex))
            {
                component.Closure =
                    ChamferVertexPatchClosure.OpenChainSourceBoundaryResolved;
            }
        }

        private static int CountChamferSourceBoundaryEndpointOwners(
            int sourceVertexIndex,
            VertexKey endpointKey,
            List<ChamferSourceBoundaryRecord> records,
            HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges,
            out ChamferSourceBoundaryEndpointOwnership owner)
        {
            int count = 0;
            owner = default;
            for (int recordIndex = 0;
                 recordIndex < records.Count;
                 recordIndex++)
            {
                ChamferSourceBoundaryRecord record = records[recordIndex];
                for (int childIndex = 0;
                     childIndex < record.Children.Count;
                     childIndex++)
                {
                    ChamferSourceBoundaryChild child =
                        record.Children[childIndex];
                    if (!expectedSourceBoundaryEdges.Contains(child.Key))
                    {
                        continue;
                    }
                    bool matchesStart =
                        record.SourceVertexStart == sourceVertexIndex &&
                        child.TouchesParentStart &&
                        endpointKey.Equals(new VertexKey(child.Start));
                    bool matchesEnd =
                        record.SourceVertexEnd == sourceVertexIndex &&
                        child.TouchesParentEnd &&
                        endpointKey.Equals(new VertexKey(child.End));
                    if (!matchesStart && !matchesEnd)
                    {
                        continue;
                    }
                    count++;
                    if (count == 1)
                    {
                        owner = new ChamferSourceBoundaryEndpointOwnership(
                            record,
                            childIndex);
                    }
                }
            }
            return count;
        }

        private static bool TryResolveChamferClosedSourcePatchClosure(
            ChamferVertexPatchComponent component,
            Dictionary<TopologyEdgeKey, int> plannedSpokeUses,
            Dictionary<TopologyEdgeKey, int> existingUseCounts,
            HashSet<TopologyEdgeKey> expectedVertexBoundaryEdges,
            HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges,
            List<ChamferProvisionalSegmentRecord> segments)
        {
            if (!component.HasEndpointSpokes ||
                component.StartSpokeKey.Equals(component.EndSpokeKey))
            {
                return false;
            }

            component.StartExistingSpokeUseCount =
                existingUseCounts.TryGetValue(
                    component.StartSpokeKey,
                    out int startExisting)
                    ? startExisting
                    : 0;
            component.EndExistingSpokeUseCount =
                existingUseCounts.TryGetValue(
                    component.EndSpokeKey,
                    out int endExisting)
                    ? endExisting
                    : 0;
            component.StartPlannedSpokeUseCount =
                plannedSpokeUses.TryGetValue(
                    component.StartSpokeKey,
                    out int startPlanned)
                    ? startPlanned
                    : 0;
            component.EndPlannedSpokeUseCount =
                plannedSpokeUses.TryGetValue(
                    component.EndSpokeKey,
                    out int endPlanned)
                    ? endPlanned
                    : 0;

            bool startOwned =
                expectedVertexBoundaryEdges.Contains(
                    component.StartSpokeKey) ||
                expectedSourceBoundaryEdges.Contains(
                    component.StartSpokeKey);
            bool endOwned =
                expectedVertexBoundaryEdges.Contains(
                    component.EndSpokeKey) ||
                expectedSourceBoundaryEdges.Contains(
                    component.EndSpokeKey);
            if (startOwned || endOwned)
            {
                return false;
            }

            int startDistinctFaces = CountDistinctChamferFaceRecords(
                segments,
                component.StartSpokeKey);
            int endDistinctFaces = CountDistinctChamferFaceRecords(
                segments,
                component.EndSpokeKey);
            bool startProvenanceValid =
                startDistinctFaces ==
                    component.StartExistingSpokeUseCount;
            bool endProvenanceValid =
                endDistinctFaces ==
                    component.EndExistingSpokeUseCount;
            return startProvenanceValid &&
                endProvenanceValid &&
                component.StartExistingSpokeUseCount +
                    component.StartPlannedSpokeUseCount == 2 &&
                component.EndExistingSpokeUseCount +
                    component.EndPlannedSpokeUseCount == 2;
        }

        private static void ResolveChamferClosedSourcePatchClusters(
            List<ChamferVertexPatchComponent> components,
            ref ChamferEmissionStats stats,
            List<string> failures)
        {
            List<int> candidateIndices = new List<int>();
            Dictionary<VertexKey, List<int>> adjacency =
                new Dictionary<VertexKey, List<int>>();
            for (int componentIndex = 0;
                 componentIndex < components.Count;
                 componentIndex++)
            {
                ChamferVertexPatchComponent component =
                    components[componentIndex];
                if (component.IsClosed ||
                    !component.ProvenanceValid ||
                    component.SourceFanOpen ||
                    component.Closure !=
                        ChamferVertexPatchClosure.Unclassified)
                {
                    continue;
                }
                candidateIndices.Add(componentIndex);
                AddChamferPatchComponentAtEndpoint(
                    adjacency,
                    component.StartEndpointKey,
                    componentIndex);
                AddChamferPatchComponentAtEndpoint(
                    adjacency,
                    component.EndEndpointKey,
                    componentIndex);
            }
            candidateIndices.Sort((left, right) =>
                CompareChamferVertexPatchComponents(
                    components[left],
                    components[right]));

            HashSet<int> visited = new HashSet<int>();
            for (int candidateIndex = 0;
                 candidateIndex < candidateIndices.Count;
                 candidateIndex++)
            {
                int seed = candidateIndices[candidateIndex];
                if (!visited.Add(seed))
                {
                    continue;
                }

                List<int> clusterIndices = new List<int>();
                Queue<int> queue = new Queue<int>();
                queue.Enqueue(seed);
                while (queue.Count > 0)
                {
                    int currentIndex = queue.Dequeue();
                    clusterIndices.Add(currentIndex);
                    ChamferVertexPatchComponent current =
                        components[currentIndex];
                    EnqueueChamferPatchClusterNeighbors(
                        current.StartEndpointKey,
                        adjacency,
                        visited,
                        queue);
                    EnqueueChamferPatchClusterNeighbors(
                        current.EndEndpointKey,
                        adjacency,
                        visited,
                        queue);
                }
                clusterIndices.Sort((left, right) =>
                    CompareChamferVertexPatchComponents(
                        components[left],
                        components[right]));

                if (!TryBuildChamferClosedSourcePatchCluster(
                        components,
                        clusterIndices,
                        adjacency,
                        out ChamferVertexPatchCluster cluster,
                        out string failure))
                {
                    AddChamferVertexPatchFailure(
                        failures,
                        "closed-source-cluster/" + failure);
                    continue;
                }

                for (int i = 0; i < cluster.Components.Count; i++)
                {
                    cluster.Components[i].Closure =
                        ChamferVertexPatchClosure.OpenChainClosedSourceClusterResolved;
                    cluster.Components[i].Cluster = cluster;
                }
            }
        }

        private static void AddChamferPatchComponentAtEndpoint(
            Dictionary<VertexKey, List<int>> adjacency,
            VertexKey endpoint,
            int componentIndex)
        {
            if (!adjacency.TryGetValue(
                    endpoint,
                    out List<int> incident))
            {
                incident = new List<int>();
                adjacency.Add(endpoint, incident);
            }
            incident.Add(componentIndex);
        }

        private static void EnqueueChamferPatchClusterNeighbors(
            VertexKey endpoint,
            Dictionary<VertexKey, List<int>> adjacency,
            HashSet<int> visited,
            Queue<int> queue)
        {
            if (!adjacency.TryGetValue(endpoint, out List<int> incident))
            {
                return;
            }
            for (int i = 0; i < incident.Count; i++)
            {
                if (visited.Add(incident[i]))
                {
                    queue.Enqueue(incident[i]);
                }
            }
        }

        private static bool TryBuildChamferClosedSourcePatchCluster(
            List<ChamferVertexPatchComponent> components,
            List<int> clusterIndices,
            Dictionary<VertexKey, List<int>> adjacency,
            out ChamferVertexPatchCluster cluster,
            out string failure)
        {
            cluster = null;
            failure = string.Empty;
            if (clusterIndices.Count < 2)
            {
                failure = "insufficient-components/count:" +
                    clusterIndices.Count;
                return false;
            }

            HashSet<int> clusterSet = new HashSet<int>(clusterIndices);
            HashSet<VertexKey> endpoints = new HashSet<VertexKey>();
            for (int i = 0; i < clusterIndices.Count; i++)
            {
                ChamferVertexPatchComponent component =
                    components[clusterIndices[i]];
                if (component.StartEndpointKey.Equals(
                        component.EndEndpointKey))
                {
                    failure = "identical-component-endpoints/sourceVertex:" +
                        component.SourceVertexIndex;
                    return false;
                }
                endpoints.Add(component.StartEndpointKey);
                endpoints.Add(component.EndEndpointKey);
            }

            foreach (VertexKey endpoint in endpoints)
            {
                if (!adjacency.TryGetValue(
                        endpoint,
                        out List<int> incident))
                {
                    failure = "missing-endpoint-adjacency";
                    return false;
                }
                int degree = 0;
                for (int i = 0; i < incident.Count; i++)
                {
                    if (clusterSet.Contains(incident[i]))
                    {
                        degree++;
                    }
                }
                if (degree != 2)
                {
                    failure = "endpoint-degree/endpoint:" + endpoint +
                        "/degree:" + degree;
                    return false;
                }
            }

            VertexKey startEndpoint =
                GetMinimumChamferVertexKey(endpoints);
            List<int> startIncident = adjacency[startEndpoint];
            int firstComponent = -1;
            for (int i = 0; i < startIncident.Count; i++)
            {
                int candidate = startIncident[i];
                if (!clusterSet.Contains(candidate))
                {
                    continue;
                }
                if (firstComponent < 0 ||
                    CompareChamferVertexPatchComponents(
                        components[candidate],
                        components[firstComponent]) < 0)
                {
                    firstComponent = candidate;
                }
            }
            if (firstComponent < 0)
            {
                failure = "missing-first-component";
                return false;
            }

            List<ChamferVertexPatchComponent> orderedComponents =
                new List<ChamferVertexPatchComponent>(
                    clusterIndices.Count);
            List<ChamferOrientedVertexBoundary> orderedBoundaries =
                new List<ChamferOrientedVertexBoundary>();
            List<Vector3> orderedPositions = new List<Vector3>();
            HashSet<int> usedComponents = new HashSet<int>();
            HashSet<TopologyEdgeKey> usedBoundaries =
                new HashSet<TopologyEdgeKey>();
            HashSet<int> sourceVertices = new HashSet<int>();
            VertexKey currentEndpoint = startEndpoint;
            int currentComponent = firstComponent;
            int guard = 0;
            while (guard++ <= clusterIndices.Count)
            {
                if (!clusterSet.Contains(currentComponent) ||
                    !usedComponents.Add(currentComponent))
                {
                    failure = "component-reuse/component:" +
                        currentComponent;
                    return false;
                }

                ChamferVertexPatchComponent component =
                    components[currentComponent];
                bool forward;
                if (currentEndpoint.Equals(component.StartEndpointKey))
                {
                    forward = true;
                }
                else if (currentEndpoint.Equals(
                        component.EndEndpointKey))
                {
                    forward = false;
                }
                else
                {
                    failure = "component-endpoint-disconnect/component:" +
                        currentComponent;
                    return false;
                }

                if (!AppendChamferVertexPatchComponentArc(
                        component,
                        forward,
                        orderedBoundaries,
                        orderedPositions,
                        usedBoundaries,
                        out VertexKey nextEndpoint))
                {
                    failure = "component-arc-append/component:" +
                        currentComponent;
                    return false;
                }
                orderedComponents.Add(component);
                sourceVertices.Add(component.SourceVertexIndex);
                currentEndpoint = nextEndpoint;

                if (usedComponents.Count == clusterIndices.Count)
                {
                    break;
                }

                if (!adjacency.TryGetValue(
                        currentEndpoint,
                        out List<int> incident))
                {
                    failure = "missing-next-adjacency";
                    return false;
                }
                int nextComponent = -1;
                for (int i = 0; i < incident.Count; i++)
                {
                    int candidate = incident[i];
                    if (!clusterSet.Contains(candidate) ||
                        usedComponents.Contains(candidate))
                    {
                        continue;
                    }
                    if (nextComponent >= 0)
                    {
                        failure = "ambiguous-next-component/endpoint:" +
                            currentEndpoint;
                        return false;
                    }
                    nextComponent = candidate;
                }
                if (nextComponent < 0)
                {
                    failure = "missing-next-component/endpoint:" +
                        currentEndpoint;
                    return false;
                }
                currentComponent = nextComponent;
            }

            if (usedComponents.Count != clusterIndices.Count ||
                !currentEndpoint.Equals(startEndpoint))
            {
                failure = "walk-not-closed/used:" +
                    usedComponents.Count +
                    "/expected:" + clusterIndices.Count;
                return false;
            }
            if (orderedPositions.Count > 0)
            {
                VertexKey finalPositionKey = new VertexKey(
                    orderedPositions[orderedPositions.Count - 1]);
                if (finalPositionKey.Equals(startEndpoint))
                {
                    orderedPositions.RemoveAt(
                        orderedPositions.Count - 1);
                }
            }
            if (orderedPositions.Count != orderedBoundaries.Count ||
                orderedBoundaries.Count < 3)
            {
                failure = "invalid-ordered-loop/boundaries:" +
                    orderedBoundaries.Count +
                    "/positions:" + orderedPositions.Count;
                return false;
            }

            cluster = new ChamferVertexPatchCluster(
                orderedComponents,
                orderedBoundaries,
                orderedPositions,
                sourceVertices);
            return true;
        }

        private static bool AppendChamferVertexPatchComponentArc(
            ChamferVertexPatchComponent component,
            bool forward,
            List<ChamferOrientedVertexBoundary> orderedBoundaries,
            List<Vector3> orderedPositions,
            HashSet<TopologyEdgeKey> usedBoundaries,
            out VertexKey nextEndpoint)
        {
            int count = component.OrderedBoundaries.Count;
            if (count == 0)
            {
                nextEndpoint = default;
                return false;
            }

            for (int step = 0; step < count; step++)
            {
                int index = forward ? step : count - 1 - step;
                ChamferOrientedVertexBoundary original =
                    component.OrderedBoundaries[index];
                ChamferOrientedVertexBoundary oriented = forward
                    ? original
                    : new ChamferOrientedVertexBoundary(
                        original.Boundary,
                        original.End,
                        original.Start,
                        original.EndKey,
                        original.StartKey);
                if (!usedBoundaries.Add(oriented.Boundary.Key))
                {
                    nextEndpoint = default;
                    return false;
                }
                if (orderedPositions.Count == 0)
                {
                    orderedPositions.Add(oriented.Start);
                }
                else if (!new VertexKey(
                        orderedPositions[orderedPositions.Count - 1])
                    .Equals(oriented.StartKey))
                {
                    nextEndpoint = default;
                    return false;
                }
                orderedBoundaries.Add(oriented);
                orderedPositions.Add(oriented.End);
            }
            nextEndpoint = forward
                ? component.EndEndpointKey
                : component.StartEndpointKey;
            return true;
        }

        private static int CompareChamferVertexPatchComponents(
            ChamferVertexPatchComponent left,
            ChamferVertexPatchComponent right)
        {
            int comparison = left.SourceVertexIndex.CompareTo(
                right.SourceVertexIndex);
            if (comparison != 0)
            {
                return comparison;
            }
            comparison = left.OrderedBoundaries.Count.CompareTo(
                right.OrderedBoundaries.Count);
            if (comparison != 0)
            {
                return comparison;
            }
            if (left.OrderedBoundaries.Count > 0 &&
                right.OrderedBoundaries.Count > 0)
            {
                comparison =
                    CompareChamferExpectedVertexBoundaryProvenance(
                        left.OrderedBoundaries[0].Boundary,
                        right.OrderedBoundaries[0].Boundary);
                if (comparison != 0)
                {
                    return comparison;
                }
            }
            comparison = left.StartEndpointKey.CompareTo(
                right.StartEndpointKey);
            return comparison != 0
                ? comparison
                : left.EndEndpointKey.CompareTo(right.EndEndpointKey);
        }

        #endregion
    }
}
