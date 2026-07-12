using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear contained-owner repartition diagnostics

        private enum ChamferContainedRepartitionFailure
        {
            None,
            Arrangement,
            Triangulation,
            Area,
            Boundary,
            Topology,
            OverlapRemaining
        }

        private enum ChamferContainedResidualBuildMode
        {
            GuidedBoundary,
            GenericArrangement
        }

        private sealed class ChamferContainedRepartitionResult
        {
            public readonly ChamferContainedPatchCandidate Candidate;
            public readonly List<ChamferProvisionalFaceRecord>
                ResidualOwnerRecords;
            public readonly ChamferContainedRepartitionFailure Failure;
            public readonly float OriginalOwnerArea;
            public readonly float PatchArea;
            public readonly float ResidualArea;
            public readonly float AreaError;

            public bool Resolved =>
                Failure == ChamferContainedRepartitionFailure.None;

            public ChamferContainedRepartitionResult(
                ChamferContainedPatchCandidate candidate,
                List<ChamferProvisionalFaceRecord> residualOwnerRecords,
                ChamferContainedRepartitionFailure failure,
                float originalOwnerArea,
                float patchArea,
                float residualArea,
                float areaError)
            {
                Candidate = candidate;
                ResidualOwnerRecords = residualOwnerRecords;
                Failure = failure;
                OriginalOwnerArea = originalOwnerArea;
                PatchArea = patchArea;
                ResidualArea = residualArea;
                AreaError = areaError;
            }
        }

        private readonly struct ChamferRepartitionProjection
        {
            public readonly Vector3 Origin;
            public readonly Vector3 Tangent;
            public readonly Vector3 Bitangent;
            public readonly Vector3 Normal;

            public ChamferRepartitionProjection(
                Vector3 origin,
                Vector3 tangent,
                Vector3 bitangent,
                Vector3 normal)
            {
                Origin = origin;
                Tangent = tangent;
                Bitangent = bitangent;
                Normal = normal;
            }

            public Vector2 Project(Vector3 position)
            {
                Vector3 offset = position - Origin;
                return new Vector2(
                    Vector3.Dot(offset, Tangent),
                    Vector3.Dot(offset, Bitangent));
            }
        }

        private sealed class ChamferRepartitionSourceSegment
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly Vector2 ProjectedStart;
            public readonly Vector2 ProjectedEnd;
            public readonly int StableIndex;
            public readonly List<float> Parameters = new List<float>
            {
                0f,
                1f
            };

            public ChamferRepartitionSourceSegment(
                Vector3 start,
                Vector3 end,
                Vector2 projectedStart,
                Vector2 projectedEnd,
                int stableIndex)
            {
                Start = start;
                End = end;
                ProjectedStart = projectedStart;
                ProjectedEnd = projectedEnd;
                StableIndex = stableIndex;
            }
        }

        private sealed class ChamferRepartitionEdgeBalance
        {
            public readonly TopologyEdgeKey Key;
            public int Balance;

            public ChamferRepartitionEdgeBalance(TopologyEdgeKey key)
            {
                Key = key;
            }
        }

        private static void AuditChamferContainedOwnerRepartition(
            ChamferVertexPatchPlan plan,
            List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
            List<ChamferProvisionalFaceRecord> successfulPatchRecords,
            List<ChamferContainedPatchCandidate> candidates,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            float minimumPatchTriangleArea,
            ref ChamferEmissionStats stats)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return;
            }

            Dictionary<int, List<ChamferProvisionalFaceRecord>> byLoop =
                BuildChamferPatchRecordLookup(successfulPatchRecords);
            List<ChamferProvisionalFaceRecord> baselineRecords =
                BuildChamferContainedRepartitionRecordSet(
                    prePatchFaceRecords,
                    successfulPatchRecords,
                    null);
            List<PolygonFace> baselineFaces =
                ExtractChamferProvisionalFaces(baselineRecords);
            Dictionary<TopologyEdgeKey, int> baselineUseCounts =
                BuildTopologyEdgeUseCounts(baselineFaces);
            HashSet<TopologyEdgeKey> baselineUnexpectedOpenEdges =
                BuildChamferUnexpectedOpenEdgeSet(
                    baselineUseCounts,
                    plan.FinalSourceBoundaryEdges);
            HashSet<TopologyEdgeKey> baselineSourceBoundaryFailures =
                BuildChamferSourceBoundaryFailureSet(
                    baselineUseCounts,
                    plan.FinalSourceBoundaryEdges);
            EdgeWearTopologyStats baselineTopology =
                AuditEdgeWearTopology(
                    baselineFaces,
                    minimumStableEdgeLength);

            List<ChamferContainedPatchCandidate> individuallyResolved =
                new List<ChamferContainedPatchCandidate>();
            List<ChamferContainedPatchCandidate> orderedCandidates =
                new List<ChamferContainedPatchCandidate>(candidates);
            orderedCandidates.Sort(CompareChamferContainedPatchCandidates);
            for (int candidateIndex = 0;
                 candidateIndex < orderedCandidates.Count;
                 candidateIndex++)
            {
                ChamferContainedPatchCandidate candidate =
                    orderedCandidates[candidateIndex];
                stats.PatchContainedRepartitionCandidates++;
                ChamferContainedRepartitionResult result =
                    EvaluateChamferContainedOwnerRepartition(
                        plan,
                        prePatchFaceRecords,
                        successfulPatchRecords,
                        byLoop,
                        candidate,
                        baselineUnexpectedOpenEdges,
                        baselineSourceBoundaryFailures,
                        baselineTopology,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        minimumPatchTriangleArea,
                        ref stats);
                switch (result.Failure)
                {
                    case ChamferContainedRepartitionFailure.None:
                        stats.PatchContainedRepartitionResolved++;
                        individuallyResolved.Add(candidate);
                        break;
                    case ChamferContainedRepartitionFailure.Arrangement:
                        stats.PatchContainedRepartitionArrangementFailures++;
                        break;
                    case ChamferContainedRepartitionFailure.Triangulation:
                        stats.PatchContainedRepartitionTriangulationFailures++;
                        break;
                    case ChamferContainedRepartitionFailure.Area:
                        stats.PatchContainedRepartitionAreaFailures++;
                        break;
                    case ChamferContainedRepartitionFailure.Boundary:
                        stats.PatchContainedRepartitionBoundaryFailures++;
                        break;
                    case ChamferContainedRepartitionFailure.Topology:
                        stats.PatchContainedRepartitionTopologyFailures++;
                        break;
                    case ChamferContainedRepartitionFailure.OverlapRemaining:
                        stats.PatchContainedRepartitionOverlapRemaining++;
                        break;
                }
            }

            AuditChamferContainedOwnerCombinedRepartition(
                plan,
                prePatchFaceRecords,
                successfulPatchRecords,
                byLoop,
                individuallyResolved,
                baselineUnexpectedOpenEdges,
                baselineSourceBoundaryFailures,
                baselineTopology,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                minimumPatchTriangleArea,
                ref stats);
        }

        private static ChamferContainedRepartitionResult
            EvaluateChamferContainedOwnerRepartition(
                ChamferVertexPatchPlan plan,
                List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
                List<ChamferProvisionalFaceRecord> successfulPatchRecords,
                Dictionary<int, List<ChamferProvisionalFaceRecord>> byLoop,
                ChamferContainedPatchCandidate candidate,
                HashSet<TopologyEdgeKey> baselineUnexpectedOpenEdges,
                HashSet<TopologyEdgeKey> baselineSourceBoundaryFailures,
                EdgeWearTopologyStats baselineTopology,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                float minimumPatchTriangleArea,
                ref ChamferEmissionStats stats)
        {
            stats.PatchContainedRepairCandidates++;
            if (!candidate.HasDeterministicOwner ||
                candidate.OwnerPrePatchRecordIndex < 0 ||
                candidate.OwnerPrePatchRecordIndex >=
                    prePatchFaceRecords.Count ||
                !byLoop.TryGetValue(
                    candidate.PatchLoopIndex,
                    out List<ChamferProvisionalFaceRecord> patchRecords) ||
                patchRecords.Count == 0)
            {
                stats.PatchContainedRepairBuildFailures++;
                return BuildChamferContainedRepartitionFailure(
                    candidate,
                    ChamferContainedRepartitionFailure.Arrangement);
            }

            ChamferProvisionalFaceRecord ownerRecord =
                prePatchFaceRecords[
                    candidate.OwnerPrePatchRecordIndex];
            List<List<ChamferProvisionalFaceRecord>> patchGroups =
                new List<List<ChamferProvisionalFaceRecord>>
                {
                    patchRecords
                };
            if (!TryBuildChamferContainedOwnerResidual(
                    ownerRecord,
                    patchGroups,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    minimumPatchTriangleArea,
                    out List<ChamferProvisionalFaceRecord> residualRecords,
                    out ChamferContainedRepartitionFailure buildFailure,
                    out ChamferContainedResidualBuildMode buildMode,
                    out float ownerArea,
                    out float patchArea,
                    out float residualArea,
                    out float areaError))
            {
                stats.PatchContainedRepairBuildFailures++;
                return new ChamferContainedRepartitionResult(
                    candidate,
                    residualRecords,
                    buildFailure,
                    ownerArea,
                    patchArea,
                    residualArea,
                    areaError);
            }

            if (buildMode ==
                ChamferContainedResidualBuildMode.GuidedBoundary)
            {
                stats.PatchContainedRepairGuidedResiduals++;
            }
            else
            {
                stats.PatchContainedRepairGenericFallbacks++;
            }

            Dictionary<int, List<ChamferProvisionalFaceRecord>> replacements =
                new Dictionary<int, List<ChamferProvisionalFaceRecord>>
                {
                    [candidate.OwnerPrePatchRecordIndex] = residualRecords
                };
            List<ChamferProvisionalFaceRecord> transformedRecords =
                BuildChamferContainedRepartitionRecordSet(
                    prePatchFaceRecords,
                    successfulPatchRecords,
                    replacements);
            int endpointInsertions =
                SplitChamferContainedRepartitionEndpoints(
                    patchRecords,
                    transformedRecords,
                    minimumStableEdgeLength);
            if (endpointInsertions > 0)
            {
                stats.PatchContainedRepairEndpointAligned++;
            }
            ChamferContainedBoundaryAudit boundaryAudit =
                AuditChamferContainedBoundaryIncidence(
                    candidate,
                    ownerRecord,
                    patchRecords,
                    transformedRecords,
                    candidate.OwnerPrePatchRecordIndex,
                    residualRecords.Count,
                    minimumStableEdgeLength);
            RegisterChamferContainedBoundaryAudit(
                candidate,
                boundaryAudit,
                ref stats);
            ChamferContainedRepartitionShadowAudit shadowAudit =
                AuditChamferContainedRepartitionShadow(
                    plan,
                    patchRecords,
                    transformedRecords,
                    baselineUnexpectedOpenEdges,
                    baselineSourceBoundaryFailures,
                    baselineTopology,
                    minimumStableEdgeLength,
                    minimumPatchTriangleArea);
            RegisterChamferContainedRepartitionShadow(
                shadowAudit,
                ref stats);

            if (!boundaryAudit.ExactTwoUse)
            {
                stats.PatchContainedRepairBoundaryFailures++;
                return new ChamferContainedRepartitionResult(
                    candidate,
                    residualRecords,
                    ChamferContainedRepartitionFailure.Boundary,
                    ownerArea,
                    patchArea,
                    residualArea,
                    areaError);
            }

            if (!shadowAudit.TopologyClean)
            {
                stats.PatchContainedRepairTopologyFailures++;
                return new ChamferContainedRepartitionResult(
                    candidate,
                    residualRecords,
                    ChamferContainedRepartitionFailure.Topology,
                    ownerArea,
                    patchArea,
                    residualArea,
                    areaError);
            }

            if (!shadowAudit.OverlapRemoved)
            {
                stats.PatchContainedRepairOverlapRemaining++;
                return new ChamferContainedRepartitionResult(
                    candidate,
                    residualRecords,
                    ChamferContainedRepartitionFailure.OverlapRemaining,
                    ownerArea,
                    patchArea,
                    residualArea,
                    areaError);
            }

            stats.PatchContainedRepairResolved++;
            return new ChamferContainedRepartitionResult(
                candidate,
                residualRecords,
                ChamferContainedRepartitionFailure.None,
                ownerArea,
                patchArea,
                residualArea,
                areaError);
        }

        private static void AuditChamferContainedOwnerCombinedRepartition(
            ChamferVertexPatchPlan plan,
            List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
            List<ChamferProvisionalFaceRecord> successfulPatchRecords,
            Dictionary<int, List<ChamferProvisionalFaceRecord>> byLoop,
            List<ChamferContainedPatchCandidate> individuallyResolved,
            HashSet<TopologyEdgeKey> baselineUnexpectedOpenEdges,
            HashSet<TopologyEdgeKey> baselineSourceBoundaryFailures,
            EdgeWearTopologyStats baselineTopology,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            float minimumPatchTriangleArea,
            ref ChamferEmissionStats stats)
        {
            if (individuallyResolved.Count == 0)
            {
                return;
            }
            stats.PatchContainedCombinedAttempted +=
                individuallyResolved.Count;

            Dictionary<int, List<ChamferContainedPatchCandidate>> byOwner =
                new Dictionary<int,
                    List<ChamferContainedPatchCandidate>>();
            for (int i = 0; i < individuallyResolved.Count; i++)
            {
                ChamferContainedPatchCandidate candidate =
                    individuallyResolved[i];
                if (!byOwner.TryGetValue(
                        candidate.OwnerPrePatchRecordIndex,
                        out List<ChamferContainedPatchCandidate> ownerCandidates))
                {
                    ownerCandidates =
                        new List<ChamferContainedPatchCandidate>();
                    byOwner.Add(
                        candidate.OwnerPrePatchRecordIndex,
                        ownerCandidates);
                }
                ownerCandidates.Add(candidate);
            }

            List<int> ownerIndices = new List<int>(byOwner.Keys);
            ownerIndices.Sort();
            Dictionary<int, List<ChamferProvisionalFaceRecord>> replacements =
                new Dictionary<int, List<ChamferProvisionalFaceRecord>>();
            List<ChamferContainedPatchCandidate> appliedCandidates =
                new List<ChamferContainedPatchCandidate>();
            for (int ownerListIndex = 0;
                 ownerListIndex < ownerIndices.Count;
                 ownerListIndex++)
            {
                int ownerIndex = ownerIndices[ownerListIndex];
                List<ChamferContainedPatchCandidate> ownerCandidates =
                    byOwner[ownerIndex];
                ownerCandidates.Sort(CompareChamferContainedPatchCandidates);
                List<List<ChamferProvisionalFaceRecord>> patchGroups =
                    new List<List<ChamferProvisionalFaceRecord>>();
                bool missingPatch = false;
                for (int candidateIndex = 0;
                     candidateIndex < ownerCandidates.Count;
                     candidateIndex++)
                {
                    if (!byLoop.TryGetValue(
                            ownerCandidates[candidateIndex].PatchLoopIndex,
                            out List<ChamferProvisionalFaceRecord> patchRecords) ||
                        patchRecords.Count == 0)
                    {
                        missingPatch = true;
                        break;
                    }
                    patchGroups.Add(patchRecords);
                }
                if (missingPatch ||
                    !TryBuildChamferContainedOwnerResidual(
                        prePatchFaceRecords[ownerIndex],
                        patchGroups,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        minimumPatchTriangleArea,
                        out List<ChamferProvisionalFaceRecord> residualRecords,
                        out _,
                        out _,
                        out _,
                        out _,
                        out _,
                        out _))
                {
                    stats.PatchContainedCombinedOwnerConflicts +=
                        ownerCandidates.Count;
                    continue;
                }
                replacements.Add(ownerIndex, residualRecords);
                appliedCandidates.AddRange(ownerCandidates);
            }

            stats.PatchContainedCombinedApplied += appliedCandidates.Count;
            if (appliedCandidates.Count == 0)
            {
                return;
            }

            List<ChamferProvisionalFaceRecord> transformedRecords =
                BuildChamferContainedRepartitionRecordSet(
                    prePatchFaceRecords,
                    successfulPatchRecords,
                    replacements);
            List<ChamferBoundarySegment> combinedPatchBoundary =
                new List<ChamferBoundarySegment>();
            for (int candidateIndex = 0;
                 candidateIndex < appliedCandidates.Count;
                 candidateIndex++)
            {
                combinedPatchBoundary.AddRange(
                    BuildChamferPatchBoundarySegments(
                        byLoop[
                            appliedCandidates[candidateIndex].PatchLoopIndex]));
            }
            SplitChamferContainedRepartitionEndpoints(
                BuildChamferBoundaryEndpointList(combinedPatchBoundary),
                transformedRecords,
                minimumStableEdgeLength);
            bool boundaryFailure = false;
            for (int candidateIndex = 0;
                 candidateIndex < appliedCandidates.Count;
                 candidateIndex++)
            {
                List<ChamferProvisionalFaceRecord> patchRecords =
                    byLoop[appliedCandidates[candidateIndex].PatchLoopIndex];
                if (!DoesChamferContainedPatchBoundaryHaveTwoUses(
                        patchRecords,
                        transformedRecords))
                {
                    boundaryFailure = true;
                    break;
                }
            }
            bool topologyFailure = boundaryFailure ||
                DoesChamferContainedRepartitionWorsenTopology(
                    plan,
                    transformedRecords,
                    baselineUnexpectedOpenEdges,
                    baselineSourceBoundaryFailures,
                    baselineTopology,
                    minimumStableEdgeLength);
            if (topologyFailure)
            {
                stats.PatchContainedCombinedTopologyFailures +=
                    appliedCandidates.Count;
            }

            for (int candidateIndex = 0;
                 candidateIndex < appliedCandidates.Count;
                 candidateIndex++)
            {
                List<ChamferProvisionalFaceRecord> patchRecords =
                    byLoop[appliedCandidates[candidateIndex].PatchLoopIndex];
                if (DoesChamferContainedPatchStillOverlapReplacement(
                        patchRecords,
                        transformedRecords,
                        minimumStableEdgeLength,
                        minimumPatchTriangleArea))
                {
                    stats.PatchContainedCombinedRemainingOverlaps++;
                }
            }
        }

        private static bool TryBuildChamferContainedOwnerResidual(
            ChamferProvisionalFaceRecord ownerRecord,
            List<List<ChamferProvisionalFaceRecord>> patchGroups,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            float minimumPatchTriangleArea,
            out List<ChamferProvisionalFaceRecord> residualRecords,
            out ChamferContainedRepartitionFailure failure,
            out ChamferContainedResidualBuildMode buildMode,
            out float originalOwnerArea,
            out float patchArea,
            out float residualArea,
            out float areaError)
        {
            residualRecords = new List<ChamferProvisionalFaceRecord>();
            failure = ChamferContainedRepartitionFailure.Arrangement;
            buildMode = ChamferContainedResidualBuildMode.GenericArrangement;
            originalOwnerArea = 0f;
            patchArea = 0f;
            residualArea = 0f;
            areaError = 0f;
            if (ownerRecord == null ||
                ownerRecord.Kind !=
                    ChamferProvisionalFaceKind.ReplacementBase ||
                patchGroups == null || patchGroups.Count == 0)
            {
                return false;
            }

            PolygonFace ownerFace = BuildChamferRenderFaithfulFace(
                ownerRecord.Face);
            if (ownerFace == null ||
                !TryBuildChamferRepartitionProjection(
                    ownerFace,
                    out ChamferRepartitionProjection projection))
            {
                return false;
            }
            originalOwnerArea = CalculatePolygonArea(ownerFace.Vertices);
            if (originalOwnerArea <= minimumStableFaceArea)
            {
                return false;
            }

            List<Vector3> ownerVertices =
                new List<Vector3>(ownerFace.Vertices);
            if (CalculateChamferRepartitionSignedArea(
                    ownerVertices,
                    projection) < 0f)
            {
                ownerVertices.Reverse();
            }

            List<ChamferBoundarySegment> patchBoundary =
                new List<ChamferBoundarySegment>();
            HashSet<VertexKey> protectedKeys = new HashSet<VertexKey>();
            Dictionary<VertexKey, Vector3> preferredPositions =
                new Dictionary<VertexKey, Vector3>();
            for (int groupIndex = 0;
                 groupIndex < patchGroups.Count;
                 groupIndex++)
            {
                List<ChamferProvisionalFaceRecord> patchRecords =
                    patchGroups[groupIndex];
                if (patchRecords == null || patchRecords.Count == 0 ||
                    !TryBuildChamferCorrectedPatchTriangleGeometry(
                        patchRecords,
                        minimumPatchTriangleArea,
                        out List<ChamferDirectedTriangleGeometry>
                            patchTriangles))
                {
                    return false;
                }
                for (int triangleIndex = 0;
                     triangleIndex < patchTriangles.Count;
                     triangleIndex++)
                {
                    if (Vector3.Dot(
                            patchTriangles[triangleIndex].Normal,
                            projection.Normal) <= 0f)
                    {
                        return false;
                    }
                    patchArea += patchTriangles[triangleIndex].Area;
                }
                List<ChamferBoundarySegment> groupBoundary =
                    BuildChamferPatchBoundarySegments(patchRecords);
                if (groupBoundary.Count == 0)
                {
                    return false;
                }
                SortChamferBoundarySegments(groupBoundary);
                patchBoundary.AddRange(groupBoundary);
                for (int boundaryIndex = 0;
                     boundaryIndex < groupBoundary.Count;
                     boundaryIndex++)
                {
                    RegisterChamferRepartitionProtectedPosition(
                        groupBoundary[boundaryIndex].Start,
                        protectedKeys,
                        preferredPositions,
                        true);
                    RegisterChamferRepartitionProtectedPosition(
                        groupBoundary[boundaryIndex].End,
                        protectedKeys,
                        preferredPositions,
                        true);
                }
            }
            for (int i = 0; i < ownerVertices.Count; i++)
            {
                RegisterChamferRepartitionProtectedPosition(
                    ownerVertices[i],
                    protectedKeys,
                    preferredPositions,
                    false);
            }

            List<ChamferRepartitionSourceSegment> sourceSegments =
                new List<ChamferRepartitionSourceSegment>();
            int stableIndex = 0;
            for (int i = 0; i < ownerVertices.Count; i++)
            {
                Vector3 start = ownerVertices[i];
                Vector3 end = ownerVertices[
                    (i + 1) % ownerVertices.Count];
                sourceSegments.Add(new ChamferRepartitionSourceSegment(
                    start,
                    end,
                    projection.Project(start),
                    projection.Project(end),
                    stableIndex++));
            }
            for (int i = 0; i < patchBoundary.Count; i++)
            {
                Vector3 start = patchBoundary[i].End;
                Vector3 end = patchBoundary[i].Start;
                sourceSegments.Add(new ChamferRepartitionSourceSegment(
                    start,
                    end,
                    projection.Project(start),
                    projection.Project(end),
                    stableIndex++));
            }

            float projectionEpsilon = Mathf.Max(
                CalculateChamferRepartitionProjectionEpsilon(
                    sourceSegments),
                Mathf.Max(
                    PointMergeDistance * 2f,
                    minimumStableEdgeLength * 0.001f));
            if (projectionEpsilon <= 0f)
            {
                return false;
            }

            List<List<Vector3>> residualCycles;
            if (TryBuildChamferContainedGuidedResidualCycles(
                    ownerVertices,
                    patchGroups,
                    projection,
                    projectionEpsilon,
                    out residualCycles))
            {
                buildMode =
                    ChamferContainedResidualBuildMode.GuidedBoundary;
            }
            else
            {
                if (!TrySplitChamferRepartitionSegments(
                        sourceSegments,
                        projectionEpsilon) ||
                    !TryBuildChamferRepartitionBoundaryCycles(
                        sourceSegments,
                        projection,
                        projectionEpsilon,
                        protectedKeys,
                        preferredPositions,
                        out residualCycles))
                {
                    return false;
                }
                buildMode =
                    ChamferContainedResidualBuildMode.GenericArrangement;
            }

            float expectedResidualArea = originalOwnerArea - patchArea;
            float areaTolerance = Mathf.Max(
                minimumStableFaceArea * 4f,
                Mathf.Max(originalOwnerArea, patchArea) * 0.0001f);
            if (residualCycles.Count == 0)
            {
                areaError = Mathf.Abs(expectedResidualArea);
                if (areaError > areaTolerance)
                {
                    failure = ChamferContainedRepartitionFailure.Area;
                    return false;
                }
                failure = ChamferContainedRepartitionFailure.None;
                return true;
            }

            for (int cycleIndex = 0;
                 cycleIndex < residualCycles.Count;
                 cycleIndex++)
            {
                List<Vector3> cycle = residualCycles[cycleIndex];
                if (!TryProjectChamferPatchLoop(
                        cycle,
                        projection.Normal,
                        out List<Vector2> projected,
                        out float signedArea,
                        out float cycleEpsilon) ||
                    signedArea <= cycleEpsilon * cycleEpsilon ||
                    ChamferPatchPolygonSelfIntersects(
                        projected,
                        cycleEpsilon,
                        out _))
                {
                    failure =
                        ChamferContainedRepartitionFailure.Arrangement;
                    return false;
                }
                if (!TryEarClipChamferPolygonSilently(
                        cycle,
                        projected,
                        projection.Normal,
                        minimumPatchTriangleArea,
                        cycleEpsilon,
                        out List<List<Vector3>> triangles))
                {
                    failure =
                        ChamferContainedRepartitionFailure.Triangulation;
                    return false;
                }
                for (int triangleIndex = 0;
                     triangleIndex < triangles.Count;
                     triangleIndex++)
                {
                    List<Vector3> triangle = triangles[triangleIndex];
                    if (!TryCalculateChamferPatchTriangleNormal(
                            triangle[0],
                            triangle[1],
                            triangle[2],
                            out Vector3 triangleNormal) ||
                        Vector3.Dot(
                            triangleNormal,
                            projection.Normal) <= 0f)
                    {
                        failure =
                            ChamferContainedRepartitionFailure.Triangulation;
                        residualRecords.Clear();
                        return false;
                    }
                    float triangleArea = CalculatePolygonArea(triangle);
                    if (triangleArea <= minimumPatchTriangleArea)
                    {
                        failure =
                            ChamferContainedRepartitionFailure.Triangulation;
                        residualRecords.Clear();
                        return false;
                    }
                    residualArea += triangleArea;
                    residualRecords.Add(
                        new ChamferProvisionalFaceRecord(
                            new PolygonFace(
                                triangle,
                                ownerFace.Normal,
                                ownerFace.Feature,
                                ownerFace.FeatureStrength),
                            ChamferProvisionalFaceKind.ReplacementBase,
                            ownerRecord.SourceFaceIndex,
                            ownerRecord.SourceEdgeIndex));
                }
            }

            areaError = Mathf.Abs(
                originalOwnerArea - (patchArea + residualArea));
            if (areaError > areaTolerance ||
                residualArea < 0f ||
                residualArea > originalOwnerArea + areaTolerance)
            {
                failure = ChamferContainedRepartitionFailure.Area;
                residualRecords.Clear();
                return false;
            }

            failure = ChamferContainedRepartitionFailure.None;
            return true;
        }

        private static bool TryBuildChamferRepartitionProjection(
            PolygonFace ownerFace,
            out ChamferRepartitionProjection projection)
        {
            projection = default;
            if (ownerFace == null || ownerFace.Vertices == null ||
                ownerFace.Vertices.Count < 3 ||
                !IsFinite(ownerFace.Normal) ||
                ownerFace.Normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }
            Vector3 normal = ownerFace.Normal.normalized;
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
                tangent.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent);
            if (!IsFinite(bitangent) ||
                bitangent.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }
            bitangent.Normalize();
            projection = new ChamferRepartitionProjection(
                ownerFace.Vertices[0],
                tangent,
                bitangent,
                normal);
            return true;
        }

        private static float CalculateChamferRepartitionSignedArea(
            List<Vector3> positions,
            ChamferRepartitionProjection projection)
        {
            List<Vector2> projected = new List<Vector2>(positions.Count);
            for (int i = 0; i < positions.Count; i++)
            {
                projected.Add(projection.Project(positions[i]));
            }
            return CalculateChamferPatchSignedArea(projected);
        }

        private static float CalculateChamferRepartitionProjectionEpsilon(
            List<ChamferRepartitionSourceSegment> segments)
        {
            float minimumX = float.PositiveInfinity;
            float minimumY = float.PositiveInfinity;
            float maximumX = float.NegativeInfinity;
            float maximumY = float.NegativeInfinity;
            for (int i = 0; i < segments.Count; i++)
            {
                minimumX = Mathf.Min(
                    minimumX,
                    Mathf.Min(
                        segments[i].ProjectedStart.x,
                        segments[i].ProjectedEnd.x));
                minimumY = Mathf.Min(
                    minimumY,
                    Mathf.Min(
                        segments[i].ProjectedStart.y,
                        segments[i].ProjectedEnd.y));
                maximumX = Mathf.Max(
                    maximumX,
                    Mathf.Max(
                        segments[i].ProjectedStart.x,
                        segments[i].ProjectedEnd.x));
                maximumY = Mathf.Max(
                    maximumY,
                    Mathf.Max(
                        segments[i].ProjectedStart.y,
                        segments[i].ProjectedEnd.y));
            }
            float extent = Mathf.Max(
                maximumX - minimumX,
                maximumY - minimumY);
            return IsFiniteFloat(extent) && extent > 0f
                ? Mathf.Max(0.0000001f, extent * 0.000001f)
                : 0f;
        }

        private static bool TrySplitChamferRepartitionSegments(
            List<ChamferRepartitionSourceSegment> segments,
            float epsilon)
        {
            for (int firstIndex = 0;
                 firstIndex < segments.Count;
                 firstIndex++)
            {
                ChamferRepartitionSourceSegment first =
                    segments[firstIndex];
                Vector2 firstDirection =
                    first.ProjectedEnd - first.ProjectedStart;
                float firstLength = firstDirection.magnitude;
                if (firstLength <= epsilon)
                {
                    return false;
                }
                for (int secondIndex = firstIndex + 1;
                     secondIndex < segments.Count;
                     secondIndex++)
                {
                    ChamferRepartitionSourceSegment second =
                        segments[secondIndex];
                    Vector2 secondDirection =
                        second.ProjectedEnd - second.ProjectedStart;
                    float secondLength = secondDirection.magnitude;
                    if (secondLength <= epsilon)
                    {
                        return false;
                    }
                    Vector2 offset =
                        second.ProjectedStart - first.ProjectedStart;
                    float denominator =
                        CrossChamferRepartition2D(
                            firstDirection,
                            secondDirection);
                    float denominatorTolerance =
                        epsilon * Mathf.Max(firstLength, secondLength);
                    if (Mathf.Abs(denominator) > denominatorTolerance)
                    {
                        float firstParameter =
                            CrossChamferRepartition2D(
                                offset,
                                secondDirection) / denominator;
                        float secondParameter =
                            CrossChamferRepartition2D(
                                offset,
                                firstDirection) / denominator;
                        float firstParameterEpsilon = epsilon / firstLength;
                        float secondParameterEpsilon = epsilon / secondLength;
                        if (firstParameter < -firstParameterEpsilon ||
                            firstParameter > 1f + firstParameterEpsilon ||
                            secondParameter < -secondParameterEpsilon ||
                            secondParameter > 1f + secondParameterEpsilon)
                        {
                            continue;
                        }
                        bool firstInterior =
                            firstParameter > firstParameterEpsilon &&
                            firstParameter < 1f - firstParameterEpsilon;
                        bool secondInterior =
                            secondParameter > secondParameterEpsilon &&
                            secondParameter < 1f - secondParameterEpsilon;
                        if (firstInterior && secondInterior)
                        {
                            return false;
                        }
                        AddChamferRepartitionParameter(
                            first.Parameters,
                            Mathf.Clamp01(firstParameter),
                            firstParameterEpsilon);
                        AddChamferRepartitionParameter(
                            second.Parameters,
                            Mathf.Clamp01(secondParameter),
                            secondParameterEpsilon);
                        continue;
                    }

                    if (Mathf.Abs(CrossChamferRepartition2D(
                            offset,
                            firstDirection)) >
                        epsilon * firstLength)
                    {
                        continue;
                    }
                    AddChamferRepartitionEndpointParameters(
                        first,
                        second.ProjectedStart,
                        epsilon);
                    AddChamferRepartitionEndpointParameters(
                        first,
                        second.ProjectedEnd,
                        epsilon);
                    AddChamferRepartitionEndpointParameters(
                        second,
                        first.ProjectedStart,
                        epsilon);
                    AddChamferRepartitionEndpointParameters(
                        second,
                        first.ProjectedEnd,
                        epsilon);
                }
            }
            return true;
        }

        private static void AddChamferRepartitionEndpointParameters(
            ChamferRepartitionSourceSegment segment,
            Vector2 point,
            float epsilon)
        {
            Vector2 direction =
                segment.ProjectedEnd - segment.ProjectedStart;
            float lengthSquared = direction.sqrMagnitude;
            if (lengthSquared <= epsilon * epsilon)
            {
                return;
            }
            float parameter = Vector2.Dot(
                point - segment.ProjectedStart,
                direction) / lengthSquared;
            float parameterEpsilon = epsilon / Mathf.Sqrt(lengthSquared);
            if (parameter < -parameterEpsilon ||
                parameter > 1f + parameterEpsilon)
            {
                return;
            }
            Vector2 closest = segment.ProjectedStart +
                direction * Mathf.Clamp01(parameter);
            if ((closest - point).sqrMagnitude > epsilon * epsilon)
            {
                return;
            }
            AddChamferRepartitionParameter(
                segment.Parameters,
                Mathf.Clamp01(parameter),
                parameterEpsilon);
        }

        private static void AddChamferRepartitionParameter(
            List<float> parameters,
            float value,
            float epsilon)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                if (Mathf.Abs(parameters[i] - value) <= epsilon)
                {
                    return;
                }
            }
            parameters.Add(value);
        }

        private static bool TryBuildChamferRepartitionBoundaryCycles(
            List<ChamferRepartitionSourceSegment> sourceSegments,
            ChamferRepartitionProjection projection,
            float epsilon,
            HashSet<VertexKey> protectedKeys,
            Dictionary<VertexKey, Vector3> preferredPositions,
            out List<List<Vector3>> cycles)
        {
            cycles = new List<List<Vector3>>();
            Dictionary<VertexKey, Vector3> positions =
                new Dictionary<VertexKey, Vector3>(preferredPositions);
            Dictionary<TopologyEdgeKey, ChamferRepartitionEdgeBalance>
                balances =
                    new Dictionary<TopologyEdgeKey,
                        ChamferRepartitionEdgeBalance>();
            sourceSegments.Sort((left, right) =>
                left.StableIndex.CompareTo(right.StableIndex));
            for (int segmentIndex = 0;
                 segmentIndex < sourceSegments.Count;
                 segmentIndex++)
            {
                ChamferRepartitionSourceSegment segment =
                    sourceSegments[segmentIndex];
                segment.Parameters.Sort();
                for (int parameterIndex = 0;
                     parameterIndex < segment.Parameters.Count - 1;
                     parameterIndex++)
                {
                    float startParameter =
                        segment.Parameters[parameterIndex];
                    float endParameter =
                        segment.Parameters[parameterIndex + 1];
                    if (endParameter - startParameter <= 0.0000001f)
                    {
                        continue;
                    }
                    Vector3 start = Vector3.Lerp(
                        segment.Start,
                        segment.End,
                        startParameter);
                    Vector3 end = Vector3.Lerp(
                        segment.Start,
                        segment.End,
                        endParameter);
                    VertexKey startKey = new VertexKey(start);
                    VertexKey endKey = new VertexKey(end);
                    if (startKey.Equals(endKey))
                    {
                        continue;
                    }
                    if (preferredPositions.TryGetValue(
                            startKey,
                            out Vector3 preferredStart))
                    {
                        start = preferredStart;
                    }
                    if (preferredPositions.TryGetValue(
                            endKey,
                            out Vector3 preferredEnd))
                    {
                        end = preferredEnd;
                    }
                    positions[startKey] = start;
                    positions[endKey] = end;
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        startKey,
                        endKey);
                    if (!balances.TryGetValue(
                            key,
                            out ChamferRepartitionEdgeBalance balance))
                    {
                        balance = new ChamferRepartitionEdgeBalance(key);
                        balances.Add(key, balance);
                    }
                    balance.Balance += startKey.Equals(key.First) ? 1 : -1;
                }
            }

            Dictionary<VertexKey, VertexKey> nextByStart =
                new Dictionary<VertexKey, VertexKey>();
            Dictionary<VertexKey, int> incomingCounts =
                new Dictionary<VertexKey, int>();
            foreach (KeyValuePair<TopologyEdgeKey,
                         ChamferRepartitionEdgeBalance> pair in balances)
            {
                int balance = pair.Value.Balance;
                if (balance == 0)
                {
                    continue;
                }
                if (Mathf.Abs(balance) != 1)
                {
                    return false;
                }
                VertexKey start = balance > 0
                    ? pair.Key.First
                    : pair.Key.Second;
                VertexKey end = balance > 0
                    ? pair.Key.Second
                    : pair.Key.First;
                if (nextByStart.ContainsKey(start))
                {
                    return false;
                }
                nextByStart.Add(start, end);
                incomingCounts.TryGetValue(end, out int incoming);
                incomingCounts[end] = incoming + 1;
            }
            if (nextByStart.Count == 0)
            {
                return true;
            }
            foreach (KeyValuePair<VertexKey, VertexKey> pair in nextByStart)
            {
                if (!incomingCounts.TryGetValue(
                        pair.Key,
                        out int incoming) ||
                    incoming != 1)
                {
                    return false;
                }
            }

            List<VertexKey> starts = new List<VertexKey>(nextByStart.Keys);
            starts.Sort((left, right) => left.CompareTo(right));
            HashSet<VertexKey> visitedStarts = new HashSet<VertexKey>();
            for (int startIndex = 0; startIndex < starts.Count; startIndex++)
            {
                VertexKey start = starts[startIndex];
                if (visitedStarts.Contains(start))
                {
                    continue;
                }
                List<VertexKey> cycleKeys = new List<VertexKey>();
                VertexKey current = start;
                int guard = 0;
                while (guard++ <= nextByStart.Count)
                {
                    if (visitedStarts.Contains(current))
                    {
                        if (!current.Equals(start))
                        {
                            return false;
                        }
                        break;
                    }
                    if (!nextByStart.TryGetValue(
                            current,
                            out VertexKey next))
                    {
                        return false;
                    }
                    visitedStarts.Add(current);
                    cycleKeys.Add(current);
                    current = next;
                    if (current.Equals(start))
                    {
                        break;
                    }
                }
                if (!current.Equals(start) || cycleKeys.Count < 3)
                {
                    return false;
                }
                SimplifyChamferRepartitionCycle(
                    cycleKeys,
                    positions,
                    projection,
                    protectedKeys,
                    epsilon);
                if (cycleKeys.Count < 3)
                {
                    return false;
                }
                List<Vector3> cycle = new List<Vector3>(cycleKeys.Count);
                for (int i = 0; i < cycleKeys.Count; i++)
                {
                    cycle.Add(positions[cycleKeys[i]]);
                }
                float signedArea =
                    CalculateChamferRepartitionSignedArea(
                        cycle,
                        projection);
                if (signedArea <= epsilon * epsilon)
                {
                    return false;
                }
                cycles.Add(cycle);
            }
            return visitedStarts.Count == nextByStart.Count;
        }

        private static void SimplifyChamferRepartitionCycle(
            List<VertexKey> cycle,
            Dictionary<VertexKey, Vector3> positions,
            ChamferRepartitionProjection projection,
            HashSet<VertexKey> protectedKeys,
            float epsilon)
        {
            bool changed = true;
            while (changed && cycle.Count > 3)
            {
                changed = false;
                for (int i = 0; i < cycle.Count; i++)
                {
                    VertexKey currentKey = cycle[i];
                    if (protectedKeys.Contains(currentKey))
                    {
                        continue;
                    }
                    Vector2 previous = projection.Project(
                        positions[cycle[(i - 1 + cycle.Count) %
                            cycle.Count]]);
                    Vector2 current = projection.Project(
                        positions[currentKey]);
                    Vector2 next = projection.Project(
                        positions[cycle[(i + 1) % cycle.Count]]);
                    if (!IsChamferRepartitionPointOnSegment(
                            current,
                            previous,
                            next,
                            epsilon))
                    {
                        continue;
                    }
                    cycle.RemoveAt(i);
                    changed = true;
                    break;
                }
            }
        }

        private static bool IsChamferRepartitionPointOnSegment(
            Vector2 point,
            Vector2 start,
            Vector2 end,
            float epsilon)
        {
            Vector2 direction = end - start;
            float lengthSquared = direction.sqrMagnitude;
            if (lengthSquared <= epsilon * epsilon)
            {
                return false;
            }
            float parameter = Vector2.Dot(
                point - start,
                direction) / lengthSquared;
            if (parameter <= 0f || parameter >= 1f)
            {
                return false;
            }
            Vector2 closest = start + direction * parameter;
            return (closest - point).sqrMagnitude <= epsilon * epsilon;
        }

        private static float CrossChamferRepartition2D(
            Vector2 first,
            Vector2 second)
        {
            return first.x * second.y - first.y * second.x;
        }

        private static Dictionary<int, List<ChamferProvisionalFaceRecord>>
            BuildChamferPatchRecordLookup(
                List<ChamferProvisionalFaceRecord> successfulPatchRecords)
        {
            Dictionary<int, List<ChamferProvisionalFaceRecord>> result =
                new Dictionary<int,
                    List<ChamferProvisionalFaceRecord>>();
            for (int i = 0; i < successfulPatchRecords.Count; i++)
            {
                ChamferProvisionalFaceRecord record =
                    successfulPatchRecords[i];
                if (!result.TryGetValue(
                        record.PatchLoopIndex,
                        out List<ChamferProvisionalFaceRecord> records))
                {
                    records = new List<ChamferProvisionalFaceRecord>();
                    result.Add(record.PatchLoopIndex, records);
                }
                records.Add(record);
            }
            return result;
        }

        private static List<ChamferProvisionalFaceRecord>
            BuildChamferContainedRepartitionRecordSet(
                List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
                List<ChamferProvisionalFaceRecord> successfulPatchRecords,
                Dictionary<int, List<ChamferProvisionalFaceRecord>>
                    ownerReplacements)
        {
            List<ChamferProvisionalFaceRecord> result =
                new List<ChamferProvisionalFaceRecord>();
            for (int recordIndex = 0;
                 recordIndex < prePatchFaceRecords.Count;
                 recordIndex++)
            {
                if (ownerReplacements != null &&
                    ownerReplacements.TryGetValue(
                        recordIndex,
                        out List<ChamferProvisionalFaceRecord> replacements))
                {
                    for (int replacementIndex = 0;
                         replacementIndex < replacements.Count;
                         replacementIndex++)
                    {
                        result.Add(CloneChamferProvisionalFaceRecord(
                            replacements[replacementIndex]));
                    }
                    continue;
                }
                result.Add(CloneChamferProvisionalFaceRecord(
                    prePatchFaceRecords[recordIndex]));
            }
            for (int recordIndex = 0;
                 recordIndex < successfulPatchRecords.Count;
                 recordIndex++)
            {
                result.Add(CloneChamferProvisionalFaceRecord(
                    successfulPatchRecords[recordIndex]));
            }
            return result;
        }

        private static bool DoesChamferContainedPatchBoundaryHaveTwoUses(
            List<ChamferProvisionalFaceRecord> patchRecords,
            List<ChamferProvisionalFaceRecord> transformedRecords)
        {
            List<ChamferBoundarySegment> patchBoundary =
                BuildChamferPatchBoundarySegments(patchRecords);
            if (patchBoundary.Count == 0)
            {
                return false;
            }
            Dictionary<TopologyEdgeKey, int> useCounts =
                BuildTopologyEdgeUseCounts(
                    ExtractChamferProvisionalFaces(transformedRecords));
            for (int boundaryIndex = 0;
                 boundaryIndex < patchBoundary.Count;
                 boundaryIndex++)
            {
                TopologyEdgeKey key = new TopologyEdgeKey(
                    new VertexKey(patchBoundary[boundaryIndex].Start),
                    new VertexKey(patchBoundary[boundaryIndex].End));
                int uses = useCounts.TryGetValue(key, out int count)
                    ? count
                    : 0;
                if (uses != 2)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool DoesChamferContainedRepartitionWorsenTopology(
            ChamferVertexPatchPlan plan,
            List<ChamferProvisionalFaceRecord> transformedRecords,
            HashSet<TopologyEdgeKey> baselineUnexpectedOpenEdges,
            HashSet<TopologyEdgeKey> baselineSourceBoundaryFailures,
            EdgeWearTopologyStats baselineTopology,
            float minimumStableEdgeLength)
        {
            List<PolygonFace> transformedFaces =
                ExtractChamferProvisionalFaces(transformedRecords);
            Dictionary<TopologyEdgeKey, int> useCounts =
                BuildTopologyEdgeUseCounts(transformedFaces);
            HashSet<TopologyEdgeKey> unexpectedOpenEdges =
                BuildChamferUnexpectedOpenEdgeSet(
                    useCounts,
                    plan.FinalSourceBoundaryEdges);
            unexpectedOpenEdges.ExceptWith(
                baselineUnexpectedOpenEdges);
            HashSet<TopologyEdgeKey> sourceBoundaryFailures =
                BuildChamferSourceBoundaryFailureSet(
                    useCounts,
                    plan.FinalSourceBoundaryEdges);
            sourceBoundaryFailures.ExceptWith(
                baselineSourceBoundaryFailures);
            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                transformedFaces,
                minimumStableEdgeLength);
            return unexpectedOpenEdges.Count > 0 ||
                sourceBoundaryFailures.Count > 0 ||
                topology.NonManifoldEdgeCount >
                    baselineTopology.NonManifoldEdgeCount ||
                topology.TJunctionCount > baselineTopology.TJunctionCount;
        }

        private static bool DoesChamferContainedPatchStillOverlapReplacement(
            List<ChamferProvisionalFaceRecord> patchRecords,
            List<ChamferProvisionalFaceRecord> transformedRecords,
            float minimumStableEdgeLength,
            float minimumPatchTriangleArea)
        {
            if (!TryBuildChamferCorrectedPatchTriangleGeometry(
                    patchRecords,
                    minimumPatchTriangleArea,
                    out List<ChamferDirectedTriangleGeometry> patchTriangles))
            {
                return true;
            }
            List<ChamferProvisionalFaceRecord> nonPatchRecords =
                new List<ChamferProvisionalFaceRecord>();
            for (int i = 0; i < transformedRecords.Count; i++)
            {
                if (transformedRecords[i].Kind !=
                    ChamferProvisionalFaceKind.VertexPatch)
                {
                    nonPatchRecords.Add(
                        CloneChamferProvisionalFaceRecord(
                            transformedRecords[i]));
                }
            }
            ChamferEmissionStats ignoredStats = default;
            ChamferFaceIntersectionCache cache =
                BuildChamferFaceIntersectionCache(
                    nonPatchRecords,
                    minimumPatchTriangleArea,
                    ref ignoredStats);
            ChamferPatchOverlapClassification classification =
                ClassifyChamferPatchOverlap(
                    patchTriangles,
                    BuildChamferPatchBoundarySegments(patchRecords),
                    cache,
                    minimumStableEdgeLength);
            return classification.HasBlockingOverlap;
        }

        private static ChamferContainedRepartitionResult
            BuildChamferContainedRepartitionFailure(
                ChamferContainedPatchCandidate candidate,
                ChamferContainedRepartitionFailure failure)
        {
            return new ChamferContainedRepartitionResult(
                candidate,
                new List<ChamferProvisionalFaceRecord>(),
                failure,
                0f,
                0f,
                0f,
                0f);
        }

        private static void RegisterChamferRepartitionProtectedPosition(
            Vector3 position,
            HashSet<VertexKey> protectedKeys,
            Dictionary<VertexKey, Vector3> preferredPositions,
            bool overwrite)
        {
            VertexKey key = new VertexKey(position);
            protectedKeys.Add(key);
            if (overwrite || !preferredPositions.ContainsKey(key))
            {
                preferredPositions[key] = position;
            }
        }

        private static void SortChamferBoundarySegments(
            List<ChamferBoundarySegment> segments)
        {
            segments.Sort((left, right) =>
            {
                TopologyEdgeKey leftKey = new TopologyEdgeKey(
                    new VertexKey(left.Start),
                    new VertexKey(left.End));
                TopologyEdgeKey rightKey = new TopologyEdgeKey(
                    new VertexKey(right.Start),
                    new VertexKey(right.End));
                int keyComparison = CompareTopologyEdgeKeys(
                    leftKey,
                    rightKey);
                if (keyComparison != 0)
                {
                    return keyComparison;
                }
                int startComparison = new VertexKey(left.Start).CompareTo(
                    new VertexKey(right.Start));
                return startComparison != 0
                    ? startComparison
                    : new VertexKey(left.End).CompareTo(
                        new VertexKey(right.End));
            });
        }

        private static int CompareChamferContainedPatchCandidates(
            ChamferContainedPatchCandidate left,
            ChamferContainedPatchCandidate right)
        {
            int ownerComparison = left.OwnerPrePatchRecordIndex.CompareTo(
                right.OwnerPrePatchRecordIndex);
            return ownerComparison != 0
                ? ownerComparison
                : left.PatchLoopIndex.CompareTo(right.PatchLoopIndex);
        }

        #endregion
    }
}
