using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear contained-patch ownership diagnostics

        private static void AuditChamferContainedPatchOwnershipTransfer(
            ChamferVertexPatchPlan plan,
            List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
            List<ChamferProvisionalFaceRecord> successfulPatchRecords,
            List<ChamferContainedPatchCandidate> candidates,
            float minimumStableEdgeLength,
            ref ChamferEmissionStats stats)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return;
            }
            Dictionary<int, List<ChamferProvisionalFaceRecord>> byLoop =
                new Dictionary<int,
                    List<ChamferProvisionalFaceRecord>>();
            for (int i = 0; i < successfulPatchRecords.Count; i++)
            {
                ChamferProvisionalFaceRecord record =
                    successfulPatchRecords[i];
                if (!byLoop.TryGetValue(
                        record.PatchLoopIndex,
                        out List<ChamferProvisionalFaceRecord> records))
                {
                    records = new List<ChamferProvisionalFaceRecord>();
                    byLoop.Add(record.PatchLoopIndex, records);
                }
                records.Add(record);
            }

            List<ChamferProvisionalFaceRecord> baselineRecords =
                CloneChamferProvisionalFaceRecords(
                    prePatchFaceRecords);
            for (int recordIndex = 0;
                 recordIndex < successfulPatchRecords.Count;
                 recordIndex++)
            {
                baselineRecords.Add(
                    CloneChamferProvisionalFaceRecord(
                        successfulPatchRecords[recordIndex]));
            }
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

            float epsilon = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.001f);
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                ChamferContainedPatchCandidate candidate =
                    candidates[candidateIndex];
                stats.PatchContainedOwnershipCandidates++;
                if (!candidate.HasDeterministicOwner ||
                    candidate.OwnerPrePatchRecordIndex < 0 ||
                    candidate.OwnerPrePatchRecordIndex >=
                        prePatchFaceRecords.Count)
                {
                    stats.PatchContainedOwnershipOwnerAmbiguous++;
                    stats.PatchContainedOwnershipStillRequired++;
                    continue;
                }
                if (!byLoop.TryGetValue(
                        candidate.PatchLoopIndex,
                        out List<ChamferProvisionalFaceRecord> patchRecords) ||
                    patchRecords.Count == 0)
                {
                    stats.PatchContainedOwnershipTopologyFailures++;
                    stats.PatchContainedOwnershipStillRequired++;
                    continue;
                }

                List<ChamferBoundarySegment> patchBoundary =
                    BuildChamferPatchBoundarySegments(patchRecords);
                PolygonFace ownerFace = BuildChamferRenderFaithfulFace(
                    prePatchFaceRecords[
                        candidate.OwnerPrePatchRecordIndex].Face);
                List<ChamferBoundarySegment> ownerBoundary =
                    BuildChamferFaceBoundarySegments(ownerFace);
                bool boundaryTransferCompatible =
                    patchBoundary.Count > 0;
                for (int boundaryIndex = 0;
                     boundaryTransferCompatible &&
                     boundaryIndex < patchBoundary.Count;
                     boundaryIndex++)
                {
                    boundaryTransferCompatible =
                        IsChamferSegmentOnAnyBoundarySegment(
                            patchBoundary[boundaryIndex].Start,
                            patchBoundary[boundaryIndex].End,
                            ownerBoundary,
                            epsilon);
                }
                if (!boundaryTransferCompatible)
                {
                    stats.PatchContainedOwnershipBoundaryTransferFailures++;
                    stats.PatchContainedOwnershipStillRequired++;
                    continue;
                }

                List<ChamferProvisionalFaceRecord> omissionRecords =
                    CloneChamferProvisionalFaceRecords(
                        prePatchFaceRecords);
                for (int recordIndex = 0;
                     recordIndex < successfulPatchRecords.Count;
                     recordIndex++)
                {
                    if (successfulPatchRecords[recordIndex].PatchLoopIndex ==
                        candidate.PatchLoopIndex)
                    {
                        continue;
                    }
                    omissionRecords.Add(
                        CloneChamferProvisionalFaceRecord(
                            successfulPatchRecords[recordIndex]));
                }
                List<PolygonFace> omissionFaces =
                    ExtractChamferProvisionalFaces(omissionRecords);
                Dictionary<TopologyEdgeKey, int> useCounts =
                    BuildTopologyEdgeUseCounts(omissionFaces);
                bool boundaryUseFailure = false;
                for (int boundaryIndex = 0;
                     boundaryIndex < patchBoundary.Count;
                     boundaryIndex++)
                {
                    TopologyEdgeKey key = new TopologyEdgeKey(
                        new VertexKey(patchBoundary[boundaryIndex].Start),
                        new VertexKey(patchBoundary[boundaryIndex].End));
                    int uses = useCounts.TryGetValue(
                        key,
                        out int useCount)
                        ? useCount
                        : 0;
                    if (uses != 2)
                    {
                        boundaryUseFailure = true;
                        break;
                    }
                }
                if (boundaryUseFailure)
                {
                    stats.PatchContainedOwnershipBoundaryTransferFailures++;
                    stats.PatchContainedOwnershipStillRequired++;
                    continue;
                }

                HashSet<TopologyEdgeKey> omissionUnexpectedOpenEdges =
                    BuildChamferUnexpectedOpenEdgeSet(
                        useCounts,
                        plan.FinalSourceBoundaryEdges);
                omissionUnexpectedOpenEdges.ExceptWith(
                    baselineUnexpectedOpenEdges);
                HashSet<TopologyEdgeKey> omissionSourceFailures =
                    BuildChamferSourceBoundaryFailureSet(
                        useCounts,
                        plan.FinalSourceBoundaryEdges);
                omissionSourceFailures.ExceptWith(
                    baselineSourceBoundaryFailures);
                EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                    omissionFaces,
                    minimumStableEdgeLength);
                if (omissionUnexpectedOpenEdges.Count > 0 ||
                    omissionSourceFailures.Count > 0 ||
                    topology.NonManifoldEdgeCount >
                        baselineTopology.NonManifoldEdgeCount ||
                    topology.TJunctionCount >
                        baselineTopology.TJunctionCount)
                {
                    stats.PatchContainedOwnershipTopologyFailures++;
                    stats.PatchContainedOwnershipStillRequired++;
                    continue;
                }
                stats.PatchContainedOwnershipResolved++;
            }
        }


        private static HashSet<TopologyEdgeKey>
            BuildChamferUnexpectedOpenEdgeSet(
                Dictionary<TopologyEdgeKey, int> useCounts,
                HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges)
        {
            HashSet<TopologyEdgeKey> result =
                new HashSet<TopologyEdgeKey>();
            foreach (KeyValuePair<TopologyEdgeKey, int> pair
                     in useCounts)
            {
                if (pair.Value == 1 &&
                    !expectedSourceBoundaryEdges.Contains(pair.Key))
                {
                    result.Add(pair.Key);
                }
            }
            return result;
        }

        private static HashSet<TopologyEdgeKey>
            BuildChamferSourceBoundaryFailureSet(
                Dictionary<TopologyEdgeKey, int> useCounts,
                HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges)
        {
            HashSet<TopologyEdgeKey> result =
                new HashSet<TopologyEdgeKey>();
            foreach (TopologyEdgeKey key in expectedSourceBoundaryEdges)
            {
                int uses = useCounts.TryGetValue(
                    key,
                    out int useCount)
                    ? useCount
                    : 0;
                if (uses != 1)
                {
                    result.Add(key);
                }
            }
            return result;
        }

        #endregion
    }
}
